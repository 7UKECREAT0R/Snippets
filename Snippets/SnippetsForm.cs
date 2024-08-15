using System.Diagnostics;
using System.Media;

namespace Snippets;

public partial class SnippetsForm : Form
{
    private const float ANIMATE_LERP = 25f;
    private const int FREE_HEIGHT = 77;
    private const int MIN_HEIGHT = 120 + FREE_HEIGHT;
    private const int MAX_HEIGHT = 720 + FREE_HEIGHT;
    private readonly Stopwatch deltaStopwatch = new();
    private readonly List<SnippetItem> snippetControls;

    private readonly Snippets snippets;
    private readonly int x;
    private readonly int y;

    private bool _animateIn = true;

    private float currentOpacity;

    private bool enableClosing;
    private float goalOpacity;

    public SnippetsForm(Snippets snippets, int x, int y)
    {
        InitializeComponent();
        this.snippets = snippets;
        this.x = x;
        this.y = y;
        this.Disposed += OnDisposed;
        Application.Idle += Idle;

        this.snippetControls = new List<SnippetItem>();

        this.toolTip.SetToolTip(this.createButton,
            "Create a new snippet with the name in the box; the contents being the current clipboard's contents.");
        this.toolTip.SetToolTip(this.removeButton, "Remove the selected snippet. This cannot be undone.");
        this.toolTip.SetToolTip(this.keyBox, "The name of the snippet to search-for or create.");
    }

    public int SnippetListFullHeight
    {
        get { return this.snippetControls.Sum(ctrl => ctrl.Height); }
    }

    private bool AnimateIn
    {
        set
        {
            this._animateIn = value;
            this.currentOpacity = this._animateIn ? 0f : 1f;
            this.goalOpacity = this._animateIn ? 1f : 0f;
        }
    }
    private float GetDeltaTime(float max)
    {
        float dt = (float) this.deltaStopwatch.Elapsed.TotalSeconds;
        this.deltaStopwatch.Restart();

        return Math.Min(dt, max);
    }
    private void OnDisposed(object? sender, EventArgs e)
    {
        Application.Idle -= Idle;
    }
    private void SnippetsForm_Load(object sender, EventArgs e)
    {
        this.GotFocus += OnGotFocus;
        this.LostFocus += OnLostFocus;

        foreach (Control control in this.Controls)
        {
            control.GotFocus += OnGotFocus;
            control.LostFocus += OnLostFocus;
        }

        PopulateList();
        ApplyList();
        SetHeight();

        this.AnimateIn = true;

        Screen screen = Screen.FromControl(this);
        int bottomCoord = screen.WorkingArea.Bottom + screen.WorkingArea.Y;
        int rightCoord = screen.WorkingArea.Right + screen.WorkingArea.X;

        bool overflowX = this.x + this.Width >= rightCoord;
        bool overflowY = this.y + this.Height >= bottomCoord;

        int adjustedX = this.x;
        int adjustedY = this.y;

        if (overflowX)
            adjustedX -= this.Width;
        if (overflowY)
            adjustedY -= this.Height;

        this.Left = adjustedX;
        this.Top = adjustedY;
    }
    private void Idle(object? sender, EventArgs e)
    {
        float deltaTime = GetDeltaTime(1f / 30f); // 30fps minimum animation speed

        float oldOpacity = this.currentOpacity;
        this.currentOpacity = Visuals.Interpolate(this.currentOpacity, this.goalOpacity, ANIMATE_LERP * deltaTime);

        if (Math.Abs(oldOpacity - this.currentOpacity) > 0.01f)
            this.Opacity = this.currentOpacity;
        else if (this.Opacity != 1f)
            this.Opacity = 1f;
    }
    private void OnGotFocus(object? sender, EventArgs e)
    {
        this.enableClosing = true;
    }
    private void OnLostFocus(object? sender, EventArgs e)
    {
        foreach (Control control in this.Controls)
            if (control.Focused)
                return;

        if (this.enableClosing)
            Close();
    }

    public void SetHeight()
    {
        int requestedHeight = this.SnippetListFullHeight + FREE_HEIGHT;
        this.Height = Math.Clamp(requestedHeight, MIN_HEIGHT, MAX_HEIGHT);
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
        this.snippetList.Controls.Clear();

        if (this.snippetControls.Count == 0)
            return;

        foreach (SnippetItem item in this.snippetControls)
            this.snippetList.Controls.Add(item);
    }
    public void PopulateList()
    {
        Dictionary<string, SnippetsDataObject> snippets = this.snippets.snippets;

        var keysToBePopulated = new List<string>(snippets.Count);
        keysToBePopulated.AddRange(snippets.Keys);

        // dispose and wipe ones that no longer exist. 
        int numberOfItems = this.snippetControls.Count;
        for (int i = numberOfItems - 1; i >= 0; i--)
        {
            SnippetItem item = this.snippetControls[i];
            string keyOfElement = item.SnippetTitle;

            if (snippets.ContainsKey(keyOfElement))
            {
                keysToBePopulated.Remove(keyOfElement);
                continue;
            }

            this.snippetControls[i].Dispose();
            this.snippetControls.RemoveAt(i);
        }

        // create the element(s) that are now needed.
        foreach (string key in keysToBePopulated)
        {
            SnippetsDataObject data = snippets[key];
            SnippetItem item = CreateControl(key, data);
            this.snippetControls.Add(item);
        }
    }

    private void KeyHasText(bool hasText)
    {
        this.createButton.Enabled = hasText;
    }
    private void keyBox_TextChanged(object sender, EventArgs e)
    {
        string text = this.keyBox.Text.Trim();

        char[] invalidChars = Path.GetInvalidFileNameChars();
        bool hasInvalidChars = text.Any(c => invalidChars.Contains(c));

        if (hasInvalidChars)
        {
            string filteredText = new(text.Where(c => !invalidChars.Contains(c)).ToArray());
            this.keyBox.Text = filteredText;
            this.keyBox.SelectionStart = filteredText.Length;
            SystemSounds.Exclamation.Play();
            return;
        }

        bool valid = !string.IsNullOrEmpty(text);
        KeyHasText(valid);
    }

    private void CreateSnippet()
    {
        string key = this.keyBox.Text.Trim();

        if (string.IsNullOrEmpty(key))
            return;

        if (!this.snippets.CreateFromClipboard(key, out SnippetsDataObject? data))
        {
            MessageBox.Show("No valid data in the clipboard.", "Snippets", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        else
        {
            SnippetItem newItem = CreateControl(key, data!);
            this.snippetControls.Add(newItem);
            ApplyList();
            SetHeight();
        }
    }
    private void createButton_Click(object sender, EventArgs e)
    {
        CreateSnippet();
    }
}