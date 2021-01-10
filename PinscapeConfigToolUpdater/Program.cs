using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace PinscapeConfigToolUpdater
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // set arguments to "unknown"
            int parentPid = -1;
            String zipfile = null;
            String mainprog = null;

            // parse arguments
            //   zip=zip file name
            //   program=main program name
            //   parentpid=calling program pid
            foreach (String arg in args)
            {
                if (arg.StartsWith("zip="))
                    zipfile = arg.Substring(4);
                else if (arg.StartsWith("program="))
                    mainprog = arg.Substring(8);
                else if (arg.StartsWith("parentpid=") && !int.TryParse(arg.Substring(10), out parentPid))
                    parentPid = -1;
            }

            // if any arguments were missing, it's an error
            if (parentPid < 0 || zipfile == null || mainprog == null)
            {
                MessageBox.Show(
                    "Ce programme de mise à jour est conçu pour être exécuté automatiquement à partir de Pinscape Config Tool.",
                    caption);
                return;
            }

            // run the main window form
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UpdateWindow(zipfile, mainprog, parentPid));
           
            // delete the zip file when we're done
            try
            {
                File.Delete(zipfile);
            }
            catch (Exception)
            {
                // ignore the error
            }
        }

        // message box caption
        public static String caption = "Mise à jour automatique de Pinscape Config Tool";

    }
}
