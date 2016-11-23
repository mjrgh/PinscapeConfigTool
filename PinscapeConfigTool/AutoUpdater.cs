using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Text.RegularExpressions;
using System.IO.Compression;
using System.Diagnostics;

namespace PinscapeConfigTool
{
    class AutoUpdater
    {
        // Check for an auto-update.  If a new version ZIP file is available
        // in our download directory, we'll extract the updater executable
        // and launch it.
        //
        // The update procedure necessarily involves two .EXE programs, because
        // a single .EXE can't update itself: Windows locks an .EXE file while
        // it's loaded, so a running process can't overwrite its own .EXE.  To
        // work around this, we have two .EXE's: the main program, and a separate
        // Updater program whose only job is to update the rest of the files.
        // The main program does the work of downloading the new version, storing
        // it as a ZIP file in the downloads directory.  The main program also
        // extracts the Updater .EXE file from the ZIP, overwriting the old version
        // in the install directory.  The main program doesn't extract anything
        // else - just the Updater .EXE.  It then launches the Updater and quits.
        // At this point, the Updater is running, but the main program isn't,
        // since it terminated itself right after launching the Updater.  This
        // means that the main program .EXE file is now unlocked and can be
        // overwritten.  The Updater program extracts all of the remaining files
        // from the ZIP, overwriting the old files in the install directory; the
        // only one it doesn't extract is the new Updater .EXE, since it can't
        // overwrite itself.  But that file was already updated by the main 
        // program before we launched the updater, so it's already up to date.
        // Finally, the Updater deletes the ZIP file, re-launches the main
        // program, and terminates.  All files are now up to date, so what we
        // end up launching is the new version of the main program.
        //
        // Returns true if we launch an updater, false if not.  The application
        // should terminate immediately if we do the launch to allow the updater
        // to overwrite the application in the install directory.
        public static bool Check()
        {
            // Set up the filename strings:
            //  exename = name of updater executable
            //  exefile = full path to updater executable
            //  zipfile = full path to downloaded zip file with update
            String exename = "PinscapeConfigToolUpdater.exe";
            String exefile = Path.Combine(Program.programDir, exename);
            String zipfile = Program.updaterZip;

            // Check to see if an update ZIP file exists.  This will have been
            // downloaded on past runs and left for us to discover on a future run.
            if (File.Exists(zipfile))
            {
                // extract the installer .exe from the updater
                String doing = "initializing";
                try
                {
                    // open the ZIP file
                    doing = "opening zip file";
                    using (ZipArchive archive = ZipFile.Open(zipfile, ZipArchiveMode.Read))
                    {
                        // read the contents of the Build Number text file
                        doing = "finding build number";
                        ZipArchiveEntry entry = archive.GetEntry("BuildNumber.txt");
                        int NewBuildNumber = 0;
                        using (StreamReader s = new StreamReader(entry.Open()))
                        {
                            // read the contents of the file and parse it as an integer
                            int v;
                            if (int.TryParse(s.ReadToEnd(), out v))
                                NewBuildNumber = v;
                        }

                        // if the new build isn't newer, do nothing
                        if (NewBuildNumber <= int.Parse(VersionInfo.BuildNumber))
                        {
                            // this version is already installed - delete the zip file
                            doing = "deleting zip file";
                            archive.Dispose();
                            File.Delete(zipfile);
                            return false;
                        }

                        // find the auto-updater program
                        doing = "finding updater";
                        entry = archive.GetEntry(exename);

                        // extract it to the program fol
                        doing = "extracting updater";
                        entry.ExtractToFile(exefile, true);

                        // run it
                        doing = "launching updater";
                        Process.Start(exefile,
                            "\"zip=" + zipfile + "\""
                            + " \"program=" + Path.Combine(Program.programDir, Program.programFile) + "\""
                            + " parentpid=" + Process.GetCurrentProcess().Id);

                        // tell the caller that we launched the updater
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    // failed - ignore for this round
                    Console.WriteLine("Auto-update failed " + doing + ": " + ex.Message);
                    return false;
                }
            }

            // we didn't launch anything
            return false;
        }
    }
}
