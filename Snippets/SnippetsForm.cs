using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Snippets
{
    public partial class SnippetsForm : Form
    {
        private readonly Stopwatch deltaStopwatch = new();
        private float GetDeltaTime(float max)
        {
            float dt = (float)deltaStopwatch.Elapsed.TotalSeconds;
            deltaStopwatch.Restart();

            return Math.Min(dt, max);
        }


        private readonly Snippets snippets;
        private readonly List<SnippetItem> snippetControls;
        const float ANIMATE_LERP = 25f;
        const int FREE_HEIGHT = 77;
        const int MIN_HEIGHT = 120 + FREE_HEIGHT;
        const int MAX_HEIGHT = 720 + FREE_HEIGHT;

        bool enableClosing = false;
        private readonly int x;
        private readonly int y;

        public int SnippetListFullHeight
        {
            get
            {
                return snippetControls.Sum(ctrl => ctrl.Height);
            }
        }


        public SnippetsForm(Snippets snippets, int x, int y)
        {
            InitializeComponent();
            this.snippets = snippets;
            this.x = x;
            this.y = y;
            this.Disposed += OnDisposed;
            Application.Idle += Idle;

            snippetControls = new List<SnippetItem>();

            toolTip.SetToolTip(createButton, "Create a new snippet with the name in the box; the contents being the current clipboard's contents.");
            toolTip.SetToolTip(removeButton, "Remove the selected snippet. This cannot be undone.");
            toolTip.SetToolTip(keyBox, "The name of the snippet to search-for or create.");
        }
        private void OnDisposed(object? sender, EventArgs e)
        {
            Application.Idle -= Idle;
        }
        private void SnippetsForm_Load(object sender, EventArgs e)
        {
            this.GotFocus += OnGotFocus;
            this.LostFocus += OnLostFocus;

            foreach (Control control in Controls)
            {
                control.GotFocus += OnGotFocus;
                control.LostFocus += OnLostFocus;
            }

            PopulateList();
            ApplyList();
            SetHeight();

            AnimateIn = true;

            Screen screen = Screen.FromControl(this);
            int bottomCoord = screen.WorkingArea.Bottom + screen.WorkingArea.Y;
            int rightCoord = screen.WorkingArea.Right + screen.WorkingArea.X;

            bool overflowX = x + Width >= rightCoord;
            bool overflowY = y + Height >= bottomCoord;

            int adjustedX = x;
            int adjustedY = y;

            if (overflowX)
                adjustedX -= Width;
            if (overflowY)
                adjustedY -= Height;

            Left = adjustedX;
            Top = adjustedY;
        }

        bool _animateIn = true;

        bool AnimateIn
        {
            set
            {
                _animateIn = value;
                currentOpacity = _animateIn ? 0f : 1f;
                goalOpacity = _animateIn ? 1f : 0f;
            }
        }

        float currentOpacity;
        float goalOpacity;
        private void Idle(object? sender, EventArgs e)
        {
            float deltaTime = GetDeltaTime(1f / 30f); // 30fps minimum animation speed

            float oldOpacity = currentOpacity;
            currentOpacity = Visuals.Interpolate(currentOpacity, goalOpacity, ANIMATE_LERP * deltaTime);

            if (Math.Abs(oldOpacity - currentOpacity) > 0.01f)
                Opacity = currentOpacity;
            else if (Opacity != 1f)
                Opacity = 1f;
        }
        private void OnGotFocus(object? sender, EventArgs e)
        {
            enableClosing = true;
        }
        private void OnLostFocus(object? sender, EventArgs e)
        {
            foreach (Control control in Controls)
            {
                if (control.Focused)
                    return;
            }

            if (enableClosing)
                Close();
        }

        public void SetHeight()
        {
            int requestedHeight = SnippetListFullHeight + FREE_HEIGHT;
            Height = Math.Clamp(requestedHeight, MIN_HEIGHT, MAX_HEIGHT);
        }
        private SnippetItem CreateControl(string key, SnippetsDataObject data)
        {
            SnippetItem item = new(key, data);
            item.MouseEnter += (sender, e) => { item.HoverStart(sender, e); };
            item.MouseLeave += (sender, e) => { item.HoverEnd(sender, e); };
            return item;
        }
        public void ApplyList()
        {
            snippetList.Controls.Clear();

            if (snippetControls.Count == 0)
                return;

            foreach (SnippetItem item in snippetControls)
                snippetList.Controls.Add(item);
        }
        public void PopulateList()
        {
            Dictionary<string, SnippetsDataObject> snippets = this.snippets.snippets;

            List<string> keysToBePopulated = new List<string>(snippets.Count);
            keysToBePopulated.AddRange(snippets.Keys);

            // dispose and wipe ones that no longer exist. 
            int numberOfItems = snippetControls.Count;
            for (int i = numberOfItems - 1; i >= 0; i--)
            {
                SnippetItem item = snippetControls[i];
                string keyOfElement = item.SnippetTitle;

                if (snippets.ContainsKey(keyOfElement))
                {
                    keysToBePopulated.Remove(keyOfElement);
                    continue;
                }

                snippetControls[i].Dispose();
                snippetControls.RemoveAt(i);
                continue;
            }

            // create the element(s) that are now needed.
            foreach (string key in keysToBePopulated)
            {
                SnippetsDataObject data = snippets[key];
                SnippetItem item = CreateControl(key, data);
                snippetControls.Add(item);
            }
        }

        private void KeyHasText(bool hasText)
        {
            createButton.Enabled = hasText;
        }
        private void keyBox_TextChanged(object sender, EventArgs e)
        {
            string text = keyBox.Text.Trim();

            char[] invalidChars = Path.GetInvalidFileNameChars();
            bool hasInvalidChars = text.Any(c => invalidChars.Contains(c));

            if (hasInvalidChars)
            {
                string filteredText = new string(text.Where(c => !invalidChars.Contains(c)).ToArray());
                keyBox.Text = filteredText;
                keyBox.SelectionStart = filteredText.Length;
                SystemSounds.Exclamation.Play();
                return;
            }

            bool valid = !string.IsNullOrEmpty(text);
            KeyHasText(valid);
        }

        private void CreateSnippet()
        {
            string key = keyBox.Text.Trim();

            if (string.IsNullOrEmpty(key))
                return;

            if (!snippets.CreateFromClipboard(key, out SnippetsDataObject? data))
                MessageBox.Show("No valid data in the clipboard.", "Snippets", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                SnippetItem newItem = CreateControl(key, data!);
                snippetControls.Add(newItem);
                ApplyList();
                SetHeight();
            }
        }
        private void createButton_Click(object sender, EventArgs e)
        {
            CreateSnippet();
        }
    }
}
