using System.Runtime.InteropServices;

namespace Snippets;

/// <summary>
///     Utilities for managing advanced
///     visuals in the application.
/// </summary>
internal static class Visuals
{
    // Win32 Section
    public const int WM_NCLBUTTONDOWN = 0xA1;
    public const int HT_CAPTION = 0x2;
    /// <summary>
    ///     Rounds the border of this control.
    /// </summary>
    /// <param name="ctrl">The control to round.</param>
    /// <param name="rad">The radius, in pixels, of the rounding.</param>
    public static void RoundRegion(this Control ctrl, int rad)
    {
        Region currentRegion = ctrl.Region;
        currentRegion?.Dispose();

        IntPtr toDestroy = CreateRoundRectRgn(0, 0,
            ctrl.Width, ctrl.Height, rad, rad);
        ctrl.Region = Region.FromHrgn(toDestroy);

        // Region.FromHrgn creates a copy, so destroy the template.
        DeleteObject(toDestroy);
    }

    /// <summary>
    ///     Interpolate between two floats linearly.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public static float Interpolate(float a, float b, float t)
    {
        return a * (1.0f - t) + b * t;
    }
    /// <summary>
    ///     Interpolate between two colors linearly.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public static Color Interpolate(Color a, Color b, float t)
    {
        return Color.FromArgb(
            Interpolate(a.R, b.R, t),
            Interpolate(a.G, b.G, t),
            Interpolate(a.B, b.B, t),
            Interpolate(a.A, b.A, t));
    }
    /// <summary>
    ///     Interpolate between two bytes linearly without exceeding bounds.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public static byte Interpolate(byte a, byte b, float t)
    {
        return (byte) Math.Round(Interpolate(a, (float) b, t));
    }
    /// <summary>
    ///     Interpolate between two bytes linearly.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public static int Interpolate(int a, int b, float t)
    {
        return (int) Math.Round(Interpolate(a, (float) b, t));
    }

    public static double Distance(PointF a, PointF b)
    {
        var diff = new PointF(Math.Abs(b.X - a.X), Math.Abs(b.Y - a.Y));
        return Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y);
    }

    /// <summary>
    ///     Makes this control draggable.
    /// </summary>
    /// <param name="target"></param>
    public static void MakeDraggable(this Control target)
    {
        target.MouseDown += delegate(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && sender != null)
            {
                var obj = (Control) sender;
                ReleaseCapture();
                SendMessage(obj.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        };
    }
    /// <summary>
    ///     Makes this control a drag handle for a form.
    /// </summary>
    /// <param name="target"></param>
    public static void MakeHandle(this Control handle, Form control)
    {
        handle.MouseDown += delegate(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(control.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        };
    }

    [DllImport("gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
    public static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
        int nWidthEllipse, int nHeightEllipse);

    [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DeleteObject([In] IntPtr hObject);

    [DllImport("user32.dll")]
    public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

    [DllImport("user32.dll")]
    public static extern bool ReleaseCapture();
}