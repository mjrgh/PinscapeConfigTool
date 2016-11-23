using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;

namespace PinscapeConfigToolUpdater
{
    public partial class UpdateWindow : Form
    {
        public UpdateWindow(String zipfile, String mainprog, int parentPid)
        {
            InitializeComponent();
            this.zipfile = zipfile;
            this.mainprog = mainprog;
            this.parentPid = parentPid;

            statusMutex = new Mutex();
            worker = new Thread(ThreadMain);
            worker.Start();
        }

        String zipfile;
        String mainprog;
        int parentPid;
        Thread worker;

        String statusMsg;
        Mutex statusMutex;

        void Status(String msg)
        {
            statusMutex.WaitOne();
            statusMsg = msg;
            statusMutex.ReleaseMutex();
        }

        bool done = false;
        void ThreadMain()
        {
            Extract();
            done = true;
        }

        void Extract()
        {
            // wait for the parent process to close
            try
            {
                Status("Waiting for main program to close");
                Process parent = Process.GetProcessById(parentPid);

                // wait in a loop so we can retry if desired
                for (; ; )
                {
                    // wait a while for the program to exit; if it does, we're ready to move on
                    if (!parent.WaitForExit(60000))
                        break;

                    // ask the user if they want to wait
                    if (!AskForRetry(
                        "Before installing the update, the main program must be closed. Please "
                        + "make sure that you've closed all Pinscape Config Tool windows."))
                        return;
                }
            }
            catch (Exception)
            {
                // Ignore errors.  The most likely error is that the process ID doesn't
                // exist because the process has already exited, in which case our work
                // here is done.
            }

            Status("Opening update ZIP file");

            // get the program name
            String progfile = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);

            // get the program folder
            String progdir = AppDomain.CurrentDomain.BaseDirectory;

            // extract files: loop until we succeed or the user gives up
            for (; ; )
            {
                // Extract the ZIP file contents to the original install directory
                try
                {
                    // open the ZIP file
                    using (ZipArchive archive = ZipFile.Open(zipfile, ZipArchiveMode.Read))
                    {

                        // extract all files except for the updater program
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            // Skip the auto updater program itself - we can't replace it
                            // because it's our own executable and hence is a locked file
                            // as long as we're running.  But it's also already up to date,
                            // since the main program extracts the new updater version
                            // before launching it.
                            if (entry.Name == progfile)
                                continue;

                            // get the entry name, with Zip / path separators converted to \
                            String entryName = entry.FullName.Replace("/", "\\");

                            // skip directory entries (filenames ending in \)
                            if (entryName.EndsWith(@"\"))
                                continue;

                            // if there's a path portion, ensure the directory exists
                            String entryPath = Path.GetDirectoryName(entryName);
                            if (entryPath != "" && entryPath != null)
                            {
                                Status("Creating directory " + entryPath);
                                Directory.CreateDirectory(Path.Combine(progdir, entryPath));
                            }

                            // Extract the file.  Do this in a loop so that we can automatically
                            // retry if we get a "busy" error with an .exe file.  We sometimes
                            // get those because the main program's process, which launched us in 
                            // the first place, hasn't finished terminating yet.  C# can take a
                            // while to close down, so even though the main program terminates
                            // itself immediately after launching us, it might still be running
                            // for some time after we start.  On my machine, it can take 10-15
                            // seconds for the main program to fully terminate to the point where
                            // the .exe file becomes unlocked; my machine is pretty fast, so it's
                            // easy to imagine that it could take as much as a minute on a slow
                            // machine.  
                            for (int tries = 0; tries < 20; ++tries)
                            {
                                try
                                {
                                    // extract the file, overwriting any existing version
                                    Status("Extracting " + entryName);
                                    entry.ExtractToFile(Path.Combine(progdir, entryName), true);

                                    // success - stop the retry loop
                                    break;
                                }
                                catch (IOException exc)
                                {
                                    // if this is an executable, wait a few seconds and try again; 
                                    // otherwise give up and pass the error up to the UI
                                    if (Regex.IsMatch(entryName, @"(?i)\.exe$"))
                                    {
                                        Thread.Sleep(3000);
                                        continue;
                                    }
                                    else
                                        throw exc;
                                }
                            }
                        }
                    }

                    // if we made it this far, we succeeded - stop looping
                    break;
                }
                catch (Exception ex)
                {
                    // show the error and offer Retry/Cancel options
                    if (!AskForRetry(
                        "We're trying to update your Pinscape Config Tool, but an error occurred "
                        + "extracting new files:\r\n\r\n"
                        + ex.Message
                        + "\r\n\r\nIf you'd like to try again, please make sure that you've closed "
                        + "all Pinscape Config Tool windows and that the tool's program folder ("
                        + progdir + ") is accessible."))
                        return;
                }
            }

            // launch the new program: again, loop until we succeed or cancel
            for (; ; )
            {
                // Launch the config tool
                try
                {
                    Process.Start(Path.Combine(progdir, mainprog));
                }
                catch (Exception ex)
                {
                    if (!AskForRetry(
                        "We're trying to update your Pinscape Config Tool, and it looks like the "
                        + "new files were successfully installed. But we couldn't launch the "
                        + "new version of the program (here's the Windows error: " + ex.Message
                        + ")."))
                        return;
                }

                // success - stop looping
                break;
            }
        }

        // Ask for retry.  Returns true if we want to retry, false if we want to cancel.
        bool AskForRetry(String msg)
        {
            // show the message box; if they click Retry, tell the caller to try again
            if (MessageBox.Show(msg, Program.caption, MessageBoxButtons.RetryCancel) == DialogResult.Retry)
                return true;

            // they canceled - show the cancellation message and tell the caller to abort
            MessageBox.Show(
                "Okay, the update has been canceled.  The config tool won't try again "
                + "to install this update on its own."
                + "\r\n\r\n"
                + "If you want to install the update yourself later, simply download the "
                + "latest version from the Pinscape web site and unzip it over your existing "
                + "config tool program files.",
                Program.caption);

            // tell the caller not to retry
            return false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // update the status text
            statusMutex.WaitOne();
            txtStatus.Text = statusMsg;
            statusMutex.ReleaseMutex();
            
            // if the update thread is finished, close the window
            if (done)
                ActiveForm.Close();
        }
    }
}
