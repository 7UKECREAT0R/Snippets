using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Snippets
{
    internal partial class SnippetItem : UserControl
    {
        internal const float LERP_AMOUNT = 35f;
        internal static readonly Color BACK_COLOR_DEFAULT = Color.FromArgb(39, 44, 54);
        internal static readonly Color BACK_COLOR_HOVER = Color.FromArgb(33, 38, 48);

        internal bool hovered = false;
        internal Color goalColor = BACK_COLOR_DEFAULT;
        internal Color backColor = BACK_COLOR_DEFAULT;

        private string _snippetTitle;
        private SnippetsDataObject _snippetPreview;

        private bool? isPreviewLabel = null;
        private void SetPreviewToLabel()
        {
            if (isPreviewLabel != null && isPreviewLabel == true)
                return;
            isPreviewLabel = true;

            pictureBox.Hide();
            labelDescription.Show();
        }
        private void SetPreviewToImage(int width, int height)
        {
            if (isPreviewLabel != null && isPreviewLabel == false)
                return;
            isPreviewLabel = false;

            pictureBox.Show();
            labelDescription.Hide();

            int maxWidth = Width - 20;

            if(width > maxWidth)
            {
                float shrinkRatio = (float)width / maxWidth;
                width = maxWidth;
                height = (int)Math.Round((float)height / shrinkRatio);
            }
            
            float whRatio = (float)width / height;
            pictureBox.Height = height;
            pictureBox.Width = width;
        }

        internal string SnippetTitle
        {
            get => _snippetTitle;
            set
            {
                _snippetTitle = value;
                labelKey.Text = value;
            }
        }
        internal SnippetsDataObject SnippetPreview
        {
            get => _snippetPreview;
            set
            {
                _snippetPreview = value;

                if (value.data == null)
                    throw new NullReferenceException("No valid data in this SnippetsDataObject.");

                if (value.IsPreviewString)
                {
                    SetPreviewToLabel();
                    labelDescription.Text = value.GetPreviewString();
                    Height = labelDescription.Bottom + 10;
                }
                else if (value.type == SnippetsDataObject.FormatType.Image)
                {
                    Image image = (Image)value.data;
                    SetPreviewToImage(image.Width, image.Height);
                    pictureBox.BackgroundImage = image;
                    pictureBox.BackgroundImageLayout = ImageLayout.Zoom;
                    Height = pictureBox.Bottom + 10; // 10 px of padding from the bottom
                }
                else
                {
                    throw new NullReferenceException("Trying to display a SnippetsDataObject with no data in it.");
                }
            }
        }

#pragma warning disable CS8618
        internal SnippetItem(string snippetTitle, SnippetsDataObject snippetPreview)
#pragma warning restore CS8618
        {
            InitializeComponent();

            _snippetTitle = snippetTitle;
            _snippetPreview = snippetPreview;

            // event handlers
            this.MouseEnter += HoverStart;
            this.MouseLeave += HoverEnd;
        }
        private void SnippetItem_Load(object sender, EventArgs e)
        {
            // fields
            this.SnippetTitle = _snippetTitle;
            this.SnippetPreview = _snippetPreview;
        }

        internal void AnimationTick(float deltaTime)
        {
            backColor = Visuals.Interpolate(backColor, goalColor, deltaTime * LERP_AMOUNT);
            BackColor = backColor;
        }
        internal void HoverStart(object? sender, EventArgs e)
        {
            hovered = true;
            goalColor = BACK_COLOR_HOVER;
        }
        internal void HoverEnd(object? sender, EventArgs e)
        {
            hovered = false;
            goalColor = BACK_COLOR_DEFAULT;
        }
    }
}
