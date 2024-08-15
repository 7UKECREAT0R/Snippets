namespace Snippets
{
    partial class PrimaryForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PrimaryForm));
            startupCheckbox = new CheckBox();
            title = new Label();
            subtitle = new Label();
            hideButton = new Button();
            notifyIcon = new NotifyIcon(components);
            exitButton = new Button();
            SuspendLayout();
            // 
            // startupCheckbox
            // 
            startupCheckbox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            startupCheckbox.BackColor = Color.Transparent;
            startupCheckbox.CheckAlign = ContentAlignment.MiddleRight;
            startupCheckbox.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            startupCheckbox.ForeColor = Color.White;
            startupCheckbox.Location = new Point(12, 99);
            startupCheckbox.Name = "startupCheckbox";
            startupCheckbox.Size = new Size(274, 32);
            startupCheckbox.TabIndex = 0;
            startupCheckbox.Text = "Run on Startup";
            startupCheckbox.UseVisualStyleBackColor = false;
            // 
            // title
            // 
            title.AutoSize = true;
            title.Font = new Font("Segoe UI Black", 32F, FontStyle.Regular, GraphicsUnit.Point);
            title.ForeColor = Color.White;
            title.Location = new Point(12, 9);
            title.Name = "title";
            title.Size = new Size(209, 59);
            title.TabIndex = 1;
            title.Text = "Snippets";
            // 
            // subtitle
            // 
            subtitle.AutoSize = true;
            subtitle.Font = new Font("Segoe UI Semibold", 8F, FontStyle.Regular, GraphicsUnit.Point);
            subtitle.ForeColor = Color.FromArgb(177, 188, 203);
            subtitle.Location = new Point(21, 72);
            subtitle.Name = "subtitle";
            subtitle.Size = new Size(249, 13);
            subtitle.TabIndex = 2;
            subtitle.Text = "Press Win+Alt+V to open the snippets window. ";
            // 
            // hideButton
            // 
            hideButton.BackColor = Color.FromArgb(41, 48, 56);
            hideButton.FlatAppearance.BorderColor = Color.FromArgb(41, 48, 56);
            hideButton.FlatAppearance.BorderSize = 0;
            hideButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(21, 28, 36);
            hideButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(51, 58, 66);
            hideButton.FlatStyle = FlatStyle.Flat;
            hideButton.ForeColor = Color.White;
            hideButton.Location = new Point(12, 187);
            hideButton.Name = "hideButton";
            hideButton.Size = new Size(244, 23);
            hideButton.TabIndex = 3;
            hideButton.Text = "Hide Window";
            hideButton.UseVisualStyleBackColor = false;
            hideButton.Click += hideButton_Click;
            // 
            // notifyIcon
            // 
            notifyIcon.Icon = (Icon)resources.GetObject("notifyIcon.Icon");
            notifyIcon.Text = "Snippets - Click to return settings window.";
            notifyIcon.MouseClick += notifyIcon_MouseClick;
            // 
            // exitButton
            // 
            exitButton.BackColor = Color.FromArgb(41, 48, 56);
            exitButton.FlatAppearance.BorderColor = Color.FromArgb(41, 48, 56);
            exitButton.FlatAppearance.BorderSize = 0;
            exitButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(21, 28, 36);
            exitButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(51, 58, 66);
            exitButton.FlatStyle = FlatStyle.Flat;
            exitButton.ForeColor = Color.White;
            exitButton.Location = new Point(262, 187);
            exitButton.Name = "exitButton";
            exitButton.Size = new Size(28, 23);
            exitButton.TabIndex = 3;
            exitButton.Text = "X";
            exitButton.UseVisualStyleBackColor = false;
            exitButton.Click += exitButton_Click;
            // 
            // PrimaryForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(31, 38, 46);
            ClientSize = new Size(302, 222);
            Controls.Add(exitButton);
            Controls.Add(hideButton);
            Controls.Add(subtitle);
            Controls.Add(title);
            Controls.Add(startupCheckbox);
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "PrimaryForm";
            Text = "Snippet Settings";
            Load += PrimaryForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckBox startupCheckbox;
        private Label title;
        private Label subtitle;
        private Button hideButton;
        private NotifyIcon notifyIcon;
        private Button exitButton;
    }
}