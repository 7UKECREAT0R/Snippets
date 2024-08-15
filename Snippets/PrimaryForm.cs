using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UIAutomationClient;

namespace Snippets
{
    public partial class PrimaryForm : Form
    {
        public bool runOnStartup = false;
        private SnippetsForm? popup = null;

        public PrimaryForm()
        {
            InitializeComponent();

            runOnStartup = GetStartup();
            startupCheckbox.Checked = runOnStartup;
            startupCheckbox.CheckedChanged += startupCheckbox_CheckedChanged;
            Opacity = Program.STARTUP ? 0.00F : 1.00F;

            Visuals.MakeDraggable(this);
            Visuals.MakeHandle(title, this);
            Visuals.MakeHandle(subtitle, this);
        }
        protected override void WndProc(ref Message m)
        {
            if (GlobalHotkeys.ProcessMessage(ref m))
                return;
            base.WndProc(ref m);
        }
        /// <summary>
        /// Called when Win+Alt+V is pressed anywhere on the system.
        /// </summary>
        public void HotkeyPressed()
        {
            Point caret = Point.Empty;

            try
            {
                caret = CaretPosition.TryGetCaretPosition(out _);
            }
            catch (NotImplementedException) { }
            finally
            {
                if (caret == Point.Empty)
                    caret = Cursor.Position;
            }

            Snippets snippets = Program.core;

            // create form and get rid of old one
            popup?.Close();
            popup?.Dispose();
            popup = new SnippetsForm(snippets, caret.X, caret.Y);
            popup.Show();
        }
        private void PrimaryForm_Load(object sender, EventArgs e)
        {
            // hotkeys
            Debug.WriteLine("Registering hotkey...");
            Program.RegisterHotkeys(Handle);
            
            // round the corners of some of the components (hot)
            Visuals.RoundRegion(this, 10);
            Visuals.RoundRegion(hideButton, 5);
            Visuals.RoundRegion(exitButton, 5);

            if (Program.STARTUP)
            {
                BeginInvoke(() =>
                {
                    // hide if this is on startup
                    SetHidden(true);
                });
            }
        }

        private void startupCheckbox_CheckedChanged(object? sender, EventArgs e)
        {
            runOnStartup = startupCheckbox.Checked;

            if (!SetStartup(runOnStartup))
            {
                // no permission, reverse the change
                runOnStartup = !startupCheckbox.Checked;
                startupCheckbox.CheckedChanged -= startupCheckbox_CheckedChanged;
                startupCheckbox.Checked = runOnStartup;
                startupCheckbox.CheckedChanged += startupCheckbox_CheckedChanged;
            }
        }

        private static string AppKey { get => Application.ExecutablePath + ' ' + Program.STARTUP_ARG; }
        private static bool SetStartup(bool enable)
        {
            if (!Program.TryElevatePermission())
            {
                return false; // Environment.Exit(0) has been called
            }

            using RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true)!;

            if (enable)
            {
                rk.DeleteValue(Program.APP, false);
                rk.SetValue(Program.APP, AppKey);
            }
            else
                rk.DeleteValue(Program.APP, false);

            return true;
        }
        private static bool GetStartup()
        {
            using RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true)!;

            object? value = rk.GetValue(Program.APP, false);
            return value != null && value.Equals(AppKey);
        }

        internal void SetHidden(bool hidden)
        {
            notifyIcon.Visible = hidden;

            if (hidden)
                Hide();
            else
            {
                Opacity = 1.00F;
                Show();
            }
        }

        private void hideButton_Click(object sender, EventArgs e)
        {
            SetHidden(true);
        }
        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            SetHidden(false);
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
