namespace Snippets
{
    partial class SnippetItem
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            labelKey = new Label();
            labelDescription = new Label();
            pictureBox = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
            SuspendLayout();
            // 
            // labelKey
            // 
            labelKey.Font = new Font("Segoe UI Black", 14F, FontStyle.Regular, GraphicsUnit.Point);
            labelKey.ForeColor = Color.White;
            labelKey.ImageAlign = ContentAlignment.MiddleLeft;
            labelKey.Location = new Point(8, 8);
            labelKey.Name = "labelKey";
            labelKey.Size = new Size(492, 27);
            labelKey.TabIndex = 0;
            labelKey.Text = "title";
            labelKey.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelDescription
            // 
            labelDescription.AutoSize = true;
            labelDescription.Font = new Font("Segoe UI Light", 10F, FontStyle.Regular, GraphicsUnit.Point);
            labelDescription.ForeColor = Color.White;
            labelDescription.ImageAlign = ContentAlignment.TopLeft;
            labelDescription.Location = new Point(8, 35);
            labelDescription.Name = "labelDescription";
            labelDescription.Size = new Size(241, 19);
            labelDescription.TabIndex = 0;
            labelDescription.Text = "Subtitle, note raw text and/or an image.";
            labelDescription.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pictureBox
            // 
            pictureBox.Location = new Point(10, 45);
            pictureBox.Margin = new Padding(10);
            pictureBox.Name = "pictureBox";
            pictureBox.Size = new Size(480, 295);
            pictureBox.TabIndex = 1;
            pictureBox.TabStop = false;
            // 
            // SnippetItem
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(39, 44, 54);
            Controls.Add(pictureBox);
            Controls.Add(labelDescription);
            Controls.Add(labelKey);
            Name = "SnippetItem";
            Size = new Size(500, 70);
            Load += SnippetItem_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelKey;
        private Label labelDescription;
        private PictureBox pictureBox;
    }
}
