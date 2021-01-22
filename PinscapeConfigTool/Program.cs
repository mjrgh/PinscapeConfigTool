using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PinscapeConfigTool
{
    static class Program
    {
        [DllImport("kernel32.dll",
            EntryPoint = "GetStdHandle",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetStdHandle(int nStdHandle);
        [DllImport("kernel32.dll",
            EntryPoint = "AllocConsole",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern int AllocConsole();
        private const int STD_OUTPUT_HANDLE = -11;
        private const int MY_CODE_PAGE = 437;

        [DllImport("User32.dll",
            EntryPoint = "RegisterWindowMessage",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern UInt32 RegisterWindowMessage(String str);

        [DllImport("User32.dll",
            EntryPoint = "BroadcastSystemMessage",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern UInt32 BroadcastSystemMessage(
            uint dwFlags,
            ref uint lpdwRecipients,
            uint uiMessage,
            IntPtr wParam,
            IntPtr lParam
        );
        const UInt32 BSF_POSTMESSAGE = 0x00000010;
        const UInt32 BSM_APPLICATIONS = 0x00000008;

        // Message ID for broadcast notification
        static UInt32 notificationMessage;

        // Send our broadcast notification.  wparam indicates the running status:
        // 1=running, 0=stopping.
        public static void BroadcastNotification(int wparam)
        {
            UInt32 recipients = BSM_APPLICATIONS;
            BroadcastSystemMessage(BSF_POSTMESSAGE, ref recipients, notificationMessage, new IntPtr(wparam), IntPtr.Zero);
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Register our special broadcast message to let cooperating programs
            // know we're running.  Other software that reads the joystick will
            // see random garbage when we're running because of the special status
            // reports in the USB protocol, which are piggybacked on the joystick
            // interface.  The broadcast notification lets other programs know to
            // ignore Pinscape joysticks at these times.
            notificationMessage = RegisterWindowMessage("Pinscape.ConfigTool.Running");

            // send the RUNNING message (wparam = 1)
            BroadcastNotification(1);

            try
            {

#if USE_CONSOLE
                AllocConsole();
                IntPtr stdHandle = GetStdHandle(STD_OUTPUT_HANDLE);
                SafeFileHandle safeFileHandle = new SafeFileHandle(stdHandle, true);
                FileStream fileStream = new FileStream(safeFileHandle, FileAccess.Write);
                Encoding encoding = System.Text.Encoding.GetEncoding(MY_CODE_PAGE);
                StreamWriter standardOutput = new StreamWriter(fileStream, encoding);
                standardOutput.AutoFlush = true;
                Console.SetOut(standardOutput);  
#endif
                // get the program directory and .exe file name
                programDir = AppDomain.CurrentDomain.BaseDirectory;
                programFile = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);

                // get our special folders
                dlFolder = GetSpecialFolder(
                    Environment.SpecialFolder.MyDocuments, @"Pinscape\Downloads");

                // build the updater ZIP file name
                updaterZip = Path.Combine(dlFolder, "PinscapeConfigTool.AutoUpdate.zip");

                // get the options file path, in our Application Data folder
                optfile = Path.Combine(
                    GetSpecialFolder(Environment.SpecialFolder.ApplicationData, @"Pinscape\Controller"),
                    "options.txt");

                // set default options, then load the options file
                SetDefaultOpts();
                ReadOptFile();

                // do the normal C# form setup work
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // run the main window form
                Application.Run(new MainSetup());
            }
            catch (Exception e)
            {
                ExceptionMessageBox emb = new ExceptionMessageBox(e);
                emb.ShowDialog();
                emb.Dispose();
            }
            finally
            {
                // notify cooperating programs that we're terminating (wparam=0)
                BroadcastNotification(0);
            }

            // write any changes to the options file
            WriteOptFile();
        }

        // set the default options
        static void SetDefaultOpts()
        {
            options["CheckForDownloads"] = "yes";
        }

        // read the options file
        static void ReadOptFile()
        {
            if (File.Exists(optfile))
            {
                String[] txt = File.ReadAllLines(optfile);
                foreach (var l in txt)
                {
                    // skip comment lines and blank lines
                    if (Regex.IsMatch(l, @"^\s*#?\s*$"))
                        continue;

                    // check for an option line
                    Match m = Regex.Match(l, @"(?i)^\s*([a-z._\-$]+)\s*=(.+)$");
                    if (m.Success)
                        options[m.Groups[1].Value] = m.Groups[2].Value.Trim();
                }
            }
        }

        // write the options file
        static void WriteOptFile()
        {
            File.WriteAllLines(optfile, options.Select((kv) => { return kv.Key + " = " + kv.Value; }));
        }

        static public String GetSpecialFolder(Environment.SpecialFolder location, String subdir)
        {
            // get the full path
            String path = Path.Combine(Environment.GetFolderPath(location), subdir);

            // make sure the directory exists (ignoring any errors)
            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            catch (Exception) { }

            // return the path
            return path;
        }

        // options file
        static String optfile;

        // options list
        static public Dictionary<String, String> options = new Dictionary<String, String>();

        // download folder
        static public String dlFolder;

        // updater ZIP file
        static public String updaterZip;

        // program install directory and main program .EXE file
        static public String programDir;
        static public String programFile;

        // Win32 imports for IsInForeground
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        private static extern UInt32 GetWindowThreadProcessId(IntPtr hWnd, out UInt32 lpdwProcessId);

        // are we the foreground application?
        public static bool IsInForeground()
        {
            // get the foreground window handle
            IntPtr hwnd = GetForegroundWindow();
            if (hwnd.ToInt32() != 0)
            {
                // get the foreground window's PID
                UInt32 pid;
                if (GetWindowThreadProcessId(hwnd, out pid) != 0)
                {
                    // we're in the foreground if the window belongs to this process
                    return pid == Process.GetCurrentProcess().Id;
                }
            }

            // if anything went wrong, presume we're not in the foreground
            return false;
        }

    }
}
