namespace Snippets;

internal partial class SnippetItem : UserControl
{
    internal const float LERP_AMOUNT = 35f;
    internal static readonly Color BACK_COLOR_DEFAULT = Color.FromArgb(39, 44, 54);
    internal static readonly Color BACK_COLOR_HOVER = Color.FromArgb(33, 38, 48);
    private SnippetsDataObject _snippetPreview;

    private string _snippetTitle;
    internal Color backColor = BACK_COLOR_DEFAULT;
    internal Color goalColor = BACK_COLOR_DEFAULT;

    internal bool hovered;

    private bool? isPreviewLabel;

#pragma warning disable CS8618
    internal SnippetItem(string snippetTitle, SnippetsDataObject snippetPreview)
#pragma warning restore CS8618
    {
        InitializeComponent();

        this._snippetTitle = snippetTitle;
        this._snippetPreview = snippetPreview;

        // event handlers
        this.MouseEnter += HoverStart;
        this.MouseLeave += HoverEnd;
    }

    internal string SnippetTitle
    {
        get => this._snippetTitle;
        set
        {
            this._snippetTitle = value;
            this.labelKey.Text = value;
        }
    }
    internal SnippetsDataObject SnippetPreview
    {
        get => this._snippetPreview;
        set
        {
            this._snippetPreview = value;

            if (value.data == null)
                throw new NullReferenceException("No valid data in this SnippetsDataObject.");

            if (value.IsPreviewString)
            {
                SetPreviewToLabel();
                this.labelDescription.Text = value.GetPreviewString();
                this.Height = this.labelDescription.Bottom + 10;
            }
            else if (value.type == SnippetsDataObject.FormatType.Image)
            {
                var image = (Image) value.data;
                SetPreviewToImage(image.Width, image.Height);
                this.pictureBox.BackgroundImage = image;
                this.pictureBox.BackgroundImageLayout = ImageLayout.Zoom;
                this.Height = this.pictureBox.Bottom + 10; // 10 px of padding from the bottom
            }
            else
            {
                throw new NullReferenceException("Trying to display a SnippetsDataObject with no data in it.");
            }
        }
    }
    private void SetPreviewToLabel()
    {
        if (this.isPreviewLabel != null && this.isPreviewLabel == true)
            return;
        this.isPreviewLabel = true;

        this.pictureBox.Hide();
        this.labelDescription.Show();
    }
    private void SetPreviewToImage(int width, int height)
    {
        if (this.isPreviewLabel != null && this.isPreviewLabel == false)
            return;
        this.isPreviewLabel = false;

        this.pictureBox.Show();
        this.labelDescription.Hide();

        int maxWidth = this.Width - 20;

        if (width > maxWidth)
        {
            float shrinkRatio = (float) width / maxWidth;
            width = maxWidth;
            height = (int) Math.Round(height / shrinkRatio);
        }

        float whRatio = (float) width / height;
        this.pictureBox.Height = height;
        this.pictureBox.Width = width;
    }
    private void SnippetItem_Load(object sender, EventArgs e)
    {
        // fields
        this.SnippetTitle = this._snippetTitle;
        this.SnippetPreview = this._snippetPreview;
    }

    internal void AnimationTick(float deltaTime)
    {
        this.backColor = Visuals.Interpolate(this.backColor, this.goalColor, deltaTime * LERP_AMOUNT);
        this.BackColor = this.backColor;
    }
    internal void HoverStart(object? sender, EventArgs e)
    {
        this.hovered = true;
        this.goalColor = BACK_COLOR_HOVER;
    }
    internal void HoverEnd(object? sender, EventArgs e)
    {
        this.hovered = false;
        this.goalColor = BACK_COLOR_DEFAULT;
    }
}