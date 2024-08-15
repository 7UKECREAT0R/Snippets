using System.Diagnostics;
using Microsoft.Win32;

namespace Snippets;

public partial class PrimaryForm : Form
{
    private SnippetsForm? popup;
    public bool runOnStartup;

    public PrimaryForm()
    {
        InitializeComponent();

        this.runOnStartup = GetStartup();
        this.startupCheckbox.Checked = this.runOnStartup;
        this.startupCheckbox.CheckedChanged += startupCheckbox_CheckedChanged;
        this.Opacity = Program.STARTUP ? 0.00F : 1.00F;

        this.MakeDraggable();
        this.title.MakeHandle(this);
        this.subtitle.MakeHandle(this);
    }

    private static string AppKey => Application.ExecutablePath + ' ' + Program.STARTUP_ARG;
    protected override void WndProc(ref Message m)
    {
        if (GlobalHotkeys.ProcessMessage(ref m))
            return;
        base.WndProc(ref m);
    }

    /// <summary>
    ///     Called when Win+Alt+V is pressed anywhere on the system.
    /// </summary>
    public void HotkeyPressed()
    {
        var caret = Point.Empty;

        try
        {
            caret = CaretPosition.TryGetCaretPosition(out _);
        }
        catch (NotImplementedException) { /* thrown by internal DLLs/unmanaged code */}
        finally
        {
            if (caret == Point.Empty)
                caret = Cursor.Position;
        }

        Snippets snippets = Program.core;

        // create form and get rid of old one
        this.popup?.Close();
        this.popup?.Dispose();
        this.popup = new SnippetsForm(snippets, caret.X, caret.Y);
        this.popup.Show();
    }
    private void PrimaryForm_Load(object sender, EventArgs e)
    {
        // hotkeys
        Debug.WriteLine("Registering hotkey...");
        Program.RegisterHotkeys(this.Handle);

        // round the corners of some components (hot)
        this.RoundRegion(10);
        this.hideButton.RoundRegion(5);
        this.exitButton.RoundRegion(5);

        if (Program.STARTUP)
            BeginInvoke(() =>
            {
                // hide if this is on startup
                SetHidden(true);
            });
    }

    private void startupCheckbox_CheckedChanged(object? sender, EventArgs e)
    {
        this.runOnStartup = this.startupCheckbox.Checked;

        if (!SetStartup(this.runOnStartup))
        {
            // no permission, reverse the change
            this.runOnStartup = !this.startupCheckbox.Checked;
            this.startupCheckbox.CheckedChanged -= startupCheckbox_CheckedChanged;
            this.startupCheckbox.Checked = this.runOnStartup;
            this.startupCheckbox.CheckedChanged += startupCheckbox_CheckedChanged;
        }
    }
    private static bool SetStartup(bool enable)
    {
        if (!Program.TryElevatePermission())
            return false; // Environment.Exit(0) has been called

        using RegistryKey rk =
            Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true)!;

        if (enable)
        {
            rk.DeleteValue(Program.APP, false);
            rk.SetValue(Program.APP, AppKey);
        }
        else
        {
            rk.DeleteValue(Program.APP, false);
        }

        return true;
    }
    private static bool GetStartup()
    {
        using RegistryKey rk =
            Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true)!;

        object? value = rk.GetValue(Program.APP, false);
        return value != null && value.Equals(AppKey);
    }

    internal void SetHidden(bool hidden)
    {
        this.notifyIcon.Visible = hidden;

        if (hidden)
        {
            Hide();
        }
        else
        {
            this.Opacity = 1.00F;
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