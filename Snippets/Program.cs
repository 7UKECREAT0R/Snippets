using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;
using System.Windows.Input;

namespace Snippets
{
    internal static class Program
    {
        public const string APP = "ClipboardSnippets";
        public const string STARTUP_ARG = "/startup";

        public static bool STARTUP = false;
        public static Snippets core = new Snippets();

        /// <summary>
        /// Returns if the application is currently running as administrator.
        /// </summary>
        public static bool IsElevated
        {
            get
            {
                return new WindowsPrincipal
                    (WindowsIdentity.GetCurrent()).IsInRole
                    (WindowsBuiltInRole.Administrator);
            }
        }
        /// <summary>
        /// Attempts to obtain administrator permissions by prompting the user. Returns if the process should continue.
        /// </summary>
        /// <returns></returns>
        public static bool TryElevatePermission()
        {
            if (IsElevated)
                return true;

            var result = MessageBox.Show("This setting requires administrator to change it. Relaunch application with administrator permissions?", "Snippets", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if(result == DialogResult.Yes)
            {
                var elevateInfo = new ProcessStartInfo()
                {
                    UseShellExecute = true,
                    Verb = "runas",
                    FileName = Application.ExecutablePath,
                };
                Process.Start(elevateInfo);
                Environment.Exit(0);
            }

            return false;
        }

        static PrimaryForm? form;
        static HotkeyContract? hotkey;
        public static void RegisterHotkeys(IntPtr windowHandle)
        {
            hotkey = GlobalHotkeys.RegisterHotkey(windowHandle,
                KeyModifiers.Windows | KeyModifiers.Alt | KeyModifiers.NoRepeat,
                Keys.V,
                form,
                (_form) =>
                {
                    PrimaryForm? form = _form as PrimaryForm;

                    if (form == null)
                        return;

                    form.HotkeyPressed();
                });
        }

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // set working directory to install location ALWAYS.
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Application.ExecutablePath)!);

            if(args.Length > 0)
            {
                string arg0 = args[0];
                STARTUP = arg0.Equals(STARTUP_ARG);
            }

            try
            {
                // loading code
                Debug.WriteLine("Loading snippets...");
                core.Load();

                // config
                ApplicationConfiguration.Initialize();

                // form code, hide form if running on startup, but add tray icon.
                form = new PrimaryForm();
                Application.Run(form);

                // saving code, exit
                Debug.WriteLine("Saving snippets...");
                core.Save();
            } catch(Exception exc)
            {
                string[] errorBlips = new[]
                {
                    "Nobody panic!",
                    "Something happened...",
                    "Big Error",
                    "Program gotta stop for a bit",
                    "It's your fault, isn't it?",
                    "Wow thanks Luke",
                    "Woo, go lukecreator.dev!",
                    "Well this is nice, isn't it?",
                    "Bug report incoming"
                };

                Random r = new();
                int len = errorBlips.Length;
                int pick = r.Next(len);
                string blip = errorBlips[pick];

                MessageBox.Show($"CRASH! ({exc.Message})\n\n" + exc.ToString(), blip);
            } finally
            {
                hotkey?.Dispose();
                form?.Dispose();
            }
        }
    }
}