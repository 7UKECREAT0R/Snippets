namespace Snippets
{
    partial class SnippetsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SnippetsForm));
            keyBox = new TextBox();
            createButton = new Button();
            removeButton = new Button();
            toolTip = new ToolTip(components);
            snippetList = new FlowLayoutPanel();
            SuspendLayout();
            // 
            // keyBox
            // 
            keyBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            keyBox.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
            keyBox.Location = new Point(12, 41);
            keyBox.Name = "keyBox";
            keyBox.PlaceholderText = "Snippet Key";
            keyBox.Size = new Size(524, 25);
            keyBox.TabIndex = 2;
            keyBox.TextChanged += keyBox_TextChanged;
            // 
            // createButton
            // 
            createButton.Enabled = false;
            createButton.Location = new Point(12, 12);
            createButton.Name = "createButton";
            createButton.Size = new Size(372, 23);
            createButton.TabIndex = 0;
            createButton.Text = "New from Clipboard (Enter)";
            createButton.UseVisualStyleBackColor = true;
            createButton.Click += createButton_Click;
            // 
            // removeButton
            // 
            removeButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            removeButton.Enabled = false;
            removeButton.Location = new Point(390, 12);
            removeButton.Name = "removeButton";
            removeButton.Size = new Size(146, 23);
            removeButton.TabIndex = 1;
            removeButton.Text = "Remove (Delete)";
            removeButton.UseVisualStyleBackColor = true;
            // 
            // snippetList
            // 
            snippetList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            snippetList.AutoScroll = true;
            snippetList.BackColor = Color.FromArgb(15, 15, 19);
            snippetList.FlowDirection = FlowDirection.TopDown;
            snippetList.ImeMode = ImeMode.Off;
            snippetList.Location = new Point(12, 72);
            snippetList.Name = "snippetList";
            snippetList.Size = new Size(524, 346);
            snippetList.TabIndex = 4;
            snippetList.WrapContents = false;
            // 
            // SnippetsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(15, 15, 19);
            ClientSize = new Size(548, 430);
            Controls.Add(snippetList);
            Controls.Add(removeButton);
            Controls.Add(createButton);
            Controls.Add(keyBox);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "SnippetsForm";
            Text = "SnippetsForm";
            TopMost = true;
            Load += SnippetsForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TextBox keyBox;
        private Button createButton;
        private Button removeButton;
        private ToolTip toolTip;
        private FlowLayoutPanel snippetList;
    }
}