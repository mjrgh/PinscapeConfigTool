using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Security.Permissions;
using System.Diagnostics;
using System.Text.RegularExpressions;
using CollectionUtils;
using StringUtils;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Microsoft.VisualBasic.FileIO;


namespace PinscapeConfigTool
{
    public partial class MainSetup : Form
    {
        // For the main window, use a null initial page to load the main UI front 
        // page.  This class can be used to show other pages as well for dialogs.
        public MainSetup(String initialPage = null)
        {
            // remember the startup page
            this.initialPage = initialPage;

            // initialize the form
            InitializeComponent();

            // initialize the navigation table
            InitNavTab();
        }
        private string initialPage;

        private void MainSetup_Load(object sender, EventArgs e)
        {
            // set up our browser scripting callback interface
            ifc = new ScriptInterface(this);
            webBrowser1.ObjectForScripting = ifc;
        }

        private void MainSetup_Shown(object sender, EventArgs e)
        {
            // do some extra checks if this is the main page
            if (initialPage == null)
            {
                // check the WebBrowser control version
                Version vsn = webBrowser1.Version;
                if (vsn.Major < 11)
                {
                    AdviceDialog.Show(
                        "IEVersionWarning",
                        "Il semble que vous ayez installé une ancienne version d'IE (IE " + vsn.Major
                        + "). CE PROGRAMME POURRAIT NE PAS FONCTIONNER CORRECTEMENT SUR VOTRE SYSTÈME sauf si vous mettez à jour "
                        + "vers IE 11 ou plus récent. Si vous rencontrez des boîtes de dialogue \"Erreur de script\", "
                        + "vous devrez mettre à jour. "
                        + "\r\n\r\n"
                        + "Vous pouvez mettre à jour IE via Windows Update ou en téléchargeant la dernière "
                        + "version du site Web de Microsoft. Notez que la mise à jour est requise "
                        + "même si vous n'utilisez jamais IE pour la navigation Web, car IE contient un composant "
                        + "système que ce programme utilise en interne.");
                }

                // show DOF update advice
                AdviceDialog.Show(
                    "DOFNotice",
                    "Si vous utilisez DOF (DirectOutput Framework), assurez-vous d'avoir "
                    + "la dernière version. Cliquez sur le lien DOF Update dans la section "
                    + "divers sur la page principale pour être redirigé vers les dernières versions.");

                // we want status reports from the worker thread
                bgworkerDownload.WorkerReportsProgress = true;

                // set up the background worker to check for .bin file updates
                if (Program.options["CheckForDownloads"].ToUpper().StartsWith("Y"))
                {
                    // start an update check
                    StartUpdateCheck();
                }
                else
                {
                    downloadStatusObj = "({message:\"Le téléchargement automatique est désactivé\", fait: true})";
                    SendDownloadStatusUpdate();
                }
            }

            // if the initial page is a relative path, it's a file in the program folder
            String page = initialPage != null ? initialPage : "html/Top.htm";
            if (!Regex.IsMatch(page, @"\w+://.*"))
                page = "file:///" + Path.Combine(Program.programDir, page);

            // set up the browser object and navigate to our main menu page
            webBrowser1.Navigate(page);
            webBrowser1.AllowWebBrowserDrop = false;
            webBrowser1.IsWebBrowserContextMenuEnabled = false;
            webBrowser1.WebBrowserShortcutsEnabled = false;
        }

        // our downloader
        Downloader downloader = new Downloader();

        // Pinscape device list
        List<DeviceInfo> devlist;

        // Get a device from the device list by CPUID
        DeviceInfo GetDeviceByCPUID(String cpuid)
        {
            // if there's no device list, populate it
            if (devlist == null)
                devlist = DeviceInfo.FindDevices();

            // look up the device by CPUID
            DeviceInfo dev = devlist.FirstOrDefault((x) => x.CPUID == cpuid);

            // if we didn't find it, try rebuilding the device list, in case the device
            // was unplugged and plugged back in
            if (dev == null)
            {
                devlist = DeviceInfo.FindDevices();
                dev = devlist.FirstOrDefault((x) => x.CPUID == cpuid);
            }

            // return the result
            return dev;
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            // send the last download status update - the script wasn't ready
            // to receive it if it happened before the doc was done
            SendDownloadStatusUpdate();
        }

        delegate void Shower(String devId);
        Dictionary<String, Shower> navTab = new Dictionary<String, Shower>();
        void InitNavTab()
        {
            navTab.Add("PlungerConfig", ShowPlungerWindow);
            navTab.Add("JoystickViewer", ShowJoystickViewer);
            navTab.Add("TvOnTester", ShowTvOnTester);
            navTab.Add("ButtonTester", ShowButtonTester);
            navTab.Add("OutputTester", ShowOutputTester);
        }

        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            // look up the device if the query string has an ID= field
            Match m = Regex.Match(e.Url.Query, @"\WID=([\da-fA-F\-]+)");
            String devId = m.Success ? m.Groups[1].Value : null;

            // check for dialog URLs
            String lastEle = e.Url.Segments.Last();
            if (devId != null && navTab.ContainsKey(lastEle))
            {
                navTab[lastEle](devId);
                e.Cancel = true;
                return;
            }
        }

        // show the plunger pixel viewer window
        bool PlungerWindowOpen = false;
        void ShowPlungerWindow(String devname)
        {
            DeviceInfo dev = GetDeviceByCPUID(devname);
            if (dev != null)
            {
                // note that the plunger window is open
                PlungerWindowOpen = true;

                // show the plunger dialog
                using (PlungerSetup dlg = new PlungerSetup(dev))
                    dlg.ShowDialog();

                // window is now closed
                PlungerWindowOpen = false;
            }
        }

        // show the joystick input viewer
        bool JoystickViewerOpen = false;
        void ShowJoystickViewer(String devname)
        {
            DeviceInfo dev = GetDeviceByCPUID(devname);
            if (dev != null)
            {
                // note that the window is open
                JoystickViewerOpen = true;

                // show the window
                using (JoystickViewer dlg = new JoystickViewer(dev))
                    dlg.ShowDialog();

                // the window is now closed
                JoystickViewerOpen = false;
            }
        }

        // show the TV ON tester
        bool TvOnTesterOpen = false;
        void ShowTvOnTester(String devname)
        {
            DeviceInfo dev = GetDeviceByCPUID(devname);
            if (dev != null)
            {
                // note that the window is open
                TvOnTesterOpen = true;

                // show the window
                using (TvOnTester dlg = new TvOnTester(dev))
                    dlg.ShowDialog();

                // the window is now closed
                TvOnTesterOpen = false;
            }
        }

        // show the button tester
        bool ButtonTesterOpen = false;
        void ShowButtonTester(String devname)
        {
            DeviceInfo dev = GetDeviceByCPUID(devname);
            if (dev != null)
            {
                ButtonTesterOpen = true;
                using (ButtonStatus dlg = new ButtonStatus(dev))
                    dlg.ShowDialog();
                ButtonTesterOpen = false;
            }
        }

        // show the output tester
        bool OutputTesterOpen = false;
        void ShowOutputTester(String devname)
        {
            DeviceInfo dev = GetDeviceByCPUID(devname);
            if (dev != null)
            {
                // before showing the tester, make sure all ports on the device
                // are in the default OFF condition
                AllOutputPortsOff(dev);
                
                // flag that the tester is open
                OutputTesterOpen = true;

                // show the tester as a modal dialog
                //using (OutputTester dlg = new OutputTester(dev))
                using (MainSetup dlg = new MainSetup("html/OutputTester.htm?ID=" + devname))
                    dlg.ShowDialog();

                // flag that the tester is closed
                OutputTesterOpen = false;

                // Turn off any output ports that were activated in the tester
                AllOutputPortsOff(dev);
            }
        }

        // turn off all output ports
        void AllOutputPortsOff(DeviceInfo dev)
        {
            // "All ports off" is special request code 5
            dev.SpecialRequest(5);
        }

        // show the IR learning window
        bool IRLearnOpen = false;
        String ShowIRLearn(String devname)
        {
            DeviceInfo dev = GetDeviceByCPUID(devname);
            String result = null;
            if (dev != null)
            {
                // run the dialog
                IRLearnOpen = true;
                using (IRLearn dlg = new IRLearn(dev))
                {
                    // show the dialog
                    dlg.ShowDialog();

                    // get the result, if any
                    if (dlg.result != null)
                        result = dlg.result.ToString();
                }

                // done with the dialog
                IRLearnOpen = false;
            }

            // return the command result
            return result;
        }

        // Current download status.  This is a string encoding a javascript object in
        // source form, with the following fields:
        //
        //   message: string with HTML message text to display in status field on page
        //   done: boolean indicating if the update check is completed
        //
        String downloadStatusObj;

        // send the download status to the WebBrowser javascript handler
        void SendDownloadStatusUpdate()
        {
            if (webBrowser1.Document != null)
                webBrowser1.Document.InvokeScript("downloadStatus", new Object[] { downloadStatusObj });
        }

        void StartUpdateCheck()
        {
            if (!bgworkerDownload.IsBusy)
            {
                startedUpdateCheck = true;
                bgworkerDownload.RunWorkerAsync();
            }
        }
        bool startedUpdateCheck;

        // download thread
        private void bgworkerDownload_DoWork(object sender, DoWorkEventArgs e)
        {
            // status object
            DLStatus stat = new DLStatus(this);

            // download URL prefix
			String configToolUrlPrefix = "http://mescreations.dyndns-free.com/phpBB3/download/PinscapeConfigTool_Fr/";
            // String urlpre = "http://mescreations.dyndns-free.com/phpBB3/download/PinscapeConfigTool_Fr/";
            // String urlpre = "http://mjrnet.org/pinscape/downloads/";

            // presume we'll need an update of the setup tool if we don't already
            // have a copy of the same file locally
            bool needFirmware = true;
            bool needSetup = true;

            // get the latest BuildInfo.txt file, with the build number info
            String buildInfoError;
            Downloader.Status infoResult = downloader.CheckForUpdates(
                " nouveau",
                urlpre + "BuildInfo.txt", "BuildInfo.txt", stat, out buildInfoError);

            // if successful, check to see if we already have the latest versions
            // of the respective files
            if (infoResult == Downloader.Status.DownloadDone || infoResult == Downloader.Status.NoUpdate)
            {
                // read the file
                String bi = File.ReadAllText(Path.Combine(Program.dlFolder, "BuildInfo.txt"));

                // Check the firmware version.  If we already have a copy of the
                // same version, don't download it again.  Note that we only check
                // for a file that matches the build number according to our naming
                // convention and placed in the normal download folder.  If the user 
                // has renamed or moved the file, we won't catch it, so we'll do a
                // redundant download.  Fortunately the .bin file is small (the
                // biggest it can be is about 128K, given the size of the KL25Z's
                // flash memory), so this costs little in time and bandwidth.
                Match m;
                if ((m = Regex.Match(bi, @"(?mi)^\s*firmware:\s*(\S+)")).Success
                    && File.Exists(Path.Combine(Program.dlFolder, "Pinscape Controller " + m.Groups[1].Value + ".bin")))
                    needFirmware = false;

                // Check the setup tool build version.  If our own build number is
                // equal or higher, we don't need to download anything.
                if ((m = Regex.Match(bi, @"(?mi)^\s*setup-tool:\s*(\d+)")).Success
                    && int.Parse(VersionInfo.BuildNumber) >= int .Parse(m.Groups[1].Value))
                    needSetup = false;
            }

            // get the latest "Pinscape_Controller_KL25Z.bin" file
            Downloader.Status firmwareResult = Downloader.Status.NoUpdate;
            String firmwareError = "";
            if (needFirmware)
            {
                String tmpfile = "Pinscape_Controller_KL25Z.bin.download";
                firmwareResult = downloader.CheckForUpdates(
                    "Pinscape Controller firmware",
                    "http://mjrnet.org/pinscape/downloads/" + "Pinscape_Controller_KL25Z.bin",
                    tmpfile, stat, out firmwareError);
                //urlpre
                // if we successfully downloaded a new file, parse the build timestamp from 
                // the file contents, the file accordingly
                if (firmwareResult == Downloader.Status.DownloadDone)
                {
                    // get the full path to the downloaded file
                    String dlfile = Path.Combine(Program.dlFolder, tmpfile);

                    try
                    {
                        // read the file
                        byte[] bytes = File.ReadAllBytes(dlfile);

                        // look for the build ID sentinel string
                        byte[] pat = Encoding.ASCII.GetBytes("///Pinscape.Build.ID///");
                        int idx = bytes.IndexOf(pat);
                        if (idx >= 0)
                        {
                            // Pull out the timestamp bytes - this will be a date/time string 
                            // in the format "Jan 01 1970 01:23:45".  (This is a fixed format;
                            // no localization is ever applied.)
                            String datetime = Encoding.ASCII.GetString(bytes.Slice(idx + pat.Length, 20));
                            Dictionary<String, String> months = new Dictionary<String, String> { 
                                { "Jan", "01" }, { "Feb", "02" }, { "Mar", "03" }, { "Apr", "04" }, { "May", "05" }, { "Jun", "06" },
                                { "Jul", "07" }, { "Aug", "08" }, { "Sep", "09" }, { "Oct", "10" }, { "Nov", "11" }, { "Dec", "12" } 
                            };
                            String suffix = datetime.Substring(7, 4) + "-"
                                + months.ValueOrDefault(datetime.Substring(0, 3), "00") + "-"
                                + datetime.Substring(4, 2).Replace(' ', '0') + "-"
                                + datetime.Substring(12, 2)
                                + datetime.Substring(15, 2);

                            // Rename the downloaded file to incorporate the build ID
                            File.Move(dlfile, Path.Combine(Program.dlFolder, "Pinscape Controller " + suffix + ".bin"));
                        }
                    }
                    catch (Exception)
                    {
                        // ignore errors
                    }

                    // If all went well, we renamed the temp file to a final name based on 
                    // the build timestamp.  If the temp file is still there, something
                    // went wrong, so discard the file.
                    if (File.Exists(dlfile))
                    {
                        try
                        {
                            File.Delete(dlfile);
                        }
                        catch (Exception)
                        {
                            // ignore errors
                        }
                    }
                }
            }

            // If we didn't already determine that the setup tool is up-to-date based
            // on the build number, check the actual file to see if we have a copy,
            // and download it if not.
            Downloader.Status setupResult = Downloader.Status.NoUpdate;
            String setupError = "";
            if (needSetup)
            {
                // the build number is unavailable or looks new - download the zip file
                setupResult = downloader.CheckForUpdates(
                    "L'outil de configuration",
                    "http://mescreations.dyndns-free.com/phpBB3/download/PinscapeConfigTool_Fr/PinscapeConfigTool_Fr.zip",
                // String urlpre = "http://mjrnet.org/pinscape/downloads/PinscapeConfigTool.zip";
                    Program.updaterZip, stat, out setupError);
            }

            // We're done - update the status message to reflect the results.
            if (firmwareResult == Downloader.Status.NoUpdate && setupResult == Downloader.Status.NoUpdate)
            {
                stat.Progress("<span class=\"upToDate\">Vérification du téléchargement terminée.</span>", true, false);
            }
            else
            {
                Func<String, Downloader.Status, String, String> DoneMsg = (desc, s, errmsg) =>
                {
                    switch (s)
                    {
                        case Downloader.Status.CheckFailed:
                        case Downloader.Status.DownloadFailed:
                            return "<span class='error'>"
                                + desc + ": " + errmsg.Htmlify()
                                + "</span>";

                        case Downloader.Status.NoUpdate:
                            return "<span class=\"upToDate\">"
                                + desc + " est à jour"
                                + "</span>";

                        case Downloader.Status.DownloadDone:
                            return "<span class=\"newVersion\">"
                                + "Nouvelle " + desc + " version téléchargée"
                                + "</span>";

                        default:
                            return "<span class=\"error\">"
                                + desc + ": statut inconnu"
                                + "</span>";
                    }
                };
                stat.Progress(
                    DoneMsg("Firmware", firmwareResult, firmwareError)
                    + "; " + DoneMsg("outil de configuration", setupResult, setupError),
                    true, firmwareResult == Downloader.Status.DownloadDone || setupResult == Downloader.Status.DownloadDone);
            }
        }

        // receive status updates from the downloader thread
        private void bgworkerDownload_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            DLStatusArgs stat = e.UserState as DLStatusArgs;
            downloadStatusObj = "({"
                + "message: \"" + stat.htmlMessage.JSStringify() + "\","
                + "done: " + (stat.done ? "true" : "false") + ","
                + "downloadsFound: " + (stat.downloadsFound ? "true" : "false")
                + "})";
            SendDownloadStatusUpdate();
        }

        // Status callback object for the downloader
        class DLStatusArgs
        {
            public DLStatusArgs(String htmlMessage, bool done, bool downloadsFound)
            {
                this.htmlMessage = htmlMessage;
                this.done = done;
                this.downloadsFound = downloadsFound;
            }
            public String htmlMessage;
            public bool done;
            public bool downloadsFound;
        }
        class DLStatus : Downloader.IStatus
        {
            public DLStatus(MainSetup main)
            {
                this.main = main;
            }

            public void Progress(String htmlmsg, bool done, bool downloadsFound)
            {
                main.bgworkerDownload.ReportProgress(0, new DLStatusArgs(htmlmsg, done, downloadsFound));
            }

            MainSetup main;
        }

        // Device configuration variable descriptors.  These describe the
        // device-side config variables for interchange with the Javascript
        // UI.  The format is:
        //
        //   id  name  data
        //
        // 'id' is the config variable ID through the USB protocol (message
        // 66 for SET, 65 9 for GET).  This is an 8-bit unsigned int.
        //
        // 'name' is the Javascript property name we use when building the 
        // Javascript version.  This is a property of the main config object.
        // 
        // If the ID is followed by empty square brackets ("254[]"), it's an
        // array variable.  The javascript property (config.'name') will be
        // populated with numbered subproperties: config.name.1, config.name.2,
        // and so on, starting at 1 and continuing sequentially.
        //
        // 'data' is the data description, mapping the bytes in the USB message 
        // to/from the Javascript types.  This can be a simple scalar, or it 
        // can be a Javascript object or array literal.  The bytes of the 
        // message are mapped to scalars via these special strings:
        //
        //   $B  -> Byte: message unsigned byte <-> javascript int
        //
        //   $W  -> Word (16 bits): message byte+byte (little-endian unsigned)
        //          <-> javascript int
        //
        //   $D  -> DWord (32 bits): message byte+byte+byte+byte (little-endian unsigned)
        //          <-> javascript int
        //
        //   $P  -> Pin: messsage GPIO pin ID byte <-> javascript Pin Name string.
        //          The pin ID byte is encoded with the port number [0-4 for
        //          PTA-PTE] in the high 3 bits, and the pin number in the low 
        //          5 bits.  The special value 0xFF means Not Connected.
        //
        //   $o  -> Output port: message byte+byte (port type, pin number) <-> 
        //          javascript struture {type: <number>, pin: <pin name or number>}.
        //          The 'type' entry is simply the first byte of the message byte
        //          pair.  The 'pin' entry is EITHER a Pin Name string or a port
        //          number, according to the type.  For GPIO ports, it's a Pin Name
        //          string.  For peripheral chips (TLC5940, 74HC595), it's a port
        //          number.
        //
        // These are mapped sequentially from the first byte of the message,
        // and each one consumes the message bytes.  E.g., [$B,$B,$B] would
        // map the first three bytes of the message into an array of three
        // int values; [$W,$W,$W] would map the first 6 bytes into an array
        // of three ints.
        // 
        String[] configVarDesc = new String[]{ 
            "1 USBID {vendor:$W,product:$W}",
            "2 pinscapeID $B",
            "3 joystick {enabled:$B,axisFormat:$B,reportInterval:$D}", 
            "4 accelerometer {orientation:$B,dynamicRange:$B,autoCenterMode:$B,stutter:$B}",
            "5 plungerType $B",
            "6 plungerPins {a:$P,b:$P,c:$P,d:$P}",
            "7 calButtonPins {enabled:$B,button:$P,led:$P}",
            "8 ZBLaunchBall {port:$B,keytype:$B,keycode:$B,pushDistance:$W}",
            "9 TVon {statusPin:$P,latchPin:$P,relayPin:$P,delay:$W}",
            "10 TLC5940 {nchips:$B,SIN:$P,SCLK:$P,XLAT:$P,BLANK:$P,GSCLK:$P}",
            "11 HC595 {nchips:$B,SIN:$P,SCLK:$P,LATCH:$P,ENA:$P}",
            "12 disconnectRebootTime $B",
            "13 plungerCal {zero:$W,max:$W,tRelease:$B,calibrated:$B}",
            "14 expansionBoards {type:$B,version:$B,ext0:$B,ext1:$B,ext2:$B}",
            "15 nightMode {button:$B,flags:$B,output:$B}",
            "16 shiftButton {index:$B,mode:$B}",
            "17 IRRemote {sensorPin:$P,ledPin:$P}",
            "18 plungerAutoZero {flags:$B,time:$B}",
            "19 plungerFilters {jitterWindowSize:$W,reverseOrientation:$B}",
            "20 plungerBarCode {startPix:$W}",
            "21 TLC59116 {chipMask:$W,SDA:$P,SCL:$P,RESET:$P}",
            "22 plungerCalRaw {raw0:$W,raw1:$W,raw2:$W}",
            "250[] IRCode3 {codeHi:$D}",
            "251[] IRCode2 {protocol:$B,codeLo:$D}",
            "252[] IRCode1 {flags:$B,keytype:$B,keycode:$B}",
            "253[] xbuttons {keytype:$B,keycode:$B,IRCommand:$B}",
            "254[] buttons {pin:$P,keytype:$B,keycode:$B,flags:$B,IRCommand:$B}",
            "255[] outputs {port:$o,flags:$B,flipperLogic:$B}"
        };

        // Scripting callback interface.  This lets the javascript in
        // the web browser control call back into the C# application to
        // get information and carry out tasks.  Note that we could just
        // use the main form object for this, but using a separate object
        // gives us scripting isolation, so that the javascript can't 
        // accidentally call any of our own internal functionality.
        // This isn't a security issue, as we only load our own pages
        // and our own javascript code into the control; it's just to
        // clarify the interface and avoid coding errors.  Javascript
        // can only access the public members of this interface.
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        [System.Runtime.InteropServices.ComVisibleAttribute(true)]
        public class ScriptInterface
        {
            public ScriptInterface(MainSetup form)
            {
                this.form = form;
            }
            MainSetup form;

            // close the window
            public void CloseWindow()
            {
                form.Close();
            }

            // check for updates to the config tool
            public bool CheckForProgramUpdates()
            {
                return AutoUpdater.Check(false);
            }

            // install the config tool update
            public bool InstallProgramUpdate()
            {
                // try installing an update
                if (AutoUpdater.Check(true))
                {
                    // installer launched - close the form and tell the
                    // calling script that the update is under way
                    form.Close();
                    return true;
                }
                else
                {
                    // no update is available
                    return false;
                }
            }

            // show the plunger sensor window
            public void ShowPlungerWindow(String devname)
            {
                form.ShowPlungerWindow(devname);
            }

            // show the joystick viewer
            public void ShowJoystickViewer(String devname)
            {
                form.ShowJoystickViewer(devname);
            }

            // show the TV ON tester
            public void ShowTvOnTester(String devname)
            {
                form.ShowTvOnTester(devname);
            }

            // show the button tester
            public void ShowButtonTester(String devname)
            {
                form.ShowButtonTester(devname);
            }

            // show the output tester
            public void ShowOutputTester(String devname)
            {
                form.ShowOutputTester(devname);
            }

            // show the IR learning window
            public String ShowIRLearn(String devname)
            {
                return form.ShowIRLearn(devname);
            }

            // get the WebControl version string
            public String WebControlVersion()
            {
                Version v = form.webBrowser1.Version;
                return v.ToString();
            }

            // Show a standard message box with an OK button
            public void Alert(String msg)
            {
                MessageBox.Show(msg, "Configuration de Pinscape", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } 

            // Show a Yes/No dialog with the given message text.  Returns true 
            // if the user clicks Yes, false if the user clicks No.
            public bool YesNoDialog(String msg)
            {
                return MessageBox.Show(msg, "Configuration de Pinscape", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
            }

            // Show a Discard/Cancel dialog with the given message text.  Returns
            // true if the user clicks Discard, false for Cancel.
            public bool DiscardCancelDialog(String msg, String buttons = null)
            {
                return PinscapeConfigTool.DiscardCancelDialog.Show(msg, buttons) == DialogResult.OK;
            }

            // Show a Retry/Cancel dialog.  Returns true if the user clicked retry.
            public bool RetryCancelDialog(String msg)
            {
                return MessageBox.Show(msg, "Configuration de Pinscape", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry;
            }

            // Show an "Advice" dialog.  This type of dialog displays a message,
            // an OK button, and a checkbox to hide the dialog in the future.
            // If the user has already clicked the same "don't show again" checkbox
            // in the past, we'll skip the dialog entirely; otherwise, we'll display
            // the dialog and wait for the user to click OK, and note if the user
            // ticks the checkbox this time.  The "don't show again" state is saved
            // in the program options under the given key, with the value "Hide"
            // if the checkbox has been ticked.
            public void ShowAdviceDialog(String dialogName, String msg)
            {
                AdviceDialog.Show(dialogName, msg);
            }

            // Restore hidden dialogs
            public void RestoreHiddenDialogs()
            {
                AdviceDialog.RestoreAll();
            }

            // print the current page
            public void PrintPage()
            {
                form.webBrowser1.ShowPrintDialog();
            }

            // get the Pinscape Controller device list
            public String GetDeviceList()
            {
                // if we don't have a device list yet, fetch it
                if (form.devlist == null)
                    form.devlist = DeviceInfo.FindDevices();

                // return the list as a javascript string representing an array of objects
                return "[" + String.Join(",", form.devlist.Select((d) =>
                {
                    return "{"

                        + String.Format(
                            "PinscapeUnitNum: {0}, LedWizUnitNum: {1}, CPUID: \"{2}\", BuildID: \"{3}\", ",
                            d.PinscapeUnitNo, d.LedWizUnitNo, d.CPUID, FormatBuildID(d.BuildID))

                        + String.Format("OpenSDAID: \"{0}\", USBVersion:{1}, PlungerEnabled: {2}, ",
                            d.OpenSDAID, d.version, d.PlungerEnabled ? 1 : 0)

                        + String.Format("JoystickEnabled: {0}, ", d.JoystickEnabled ? 1 : 0)
                        + String.Format("TvOnEnabled: {0}, ", d.TvOnEnabled ? 1 : 0)

                        + String.Format("USBVendorID: \"{0:X4}\", USBProductID: \"{1:X4}\"",
                            d.vendorID, d.productID)

                        + "}";

                })) + "]";
            }

            // check if a given device is present
            public bool IsDevicePresent(String cpuid)
            {
                var lst = DeviceInfo.FindDevices();
                return lst.Any(d => d.CPUID == cpuid);
            }

            // check if the application is in the foreground
            public bool IsInForeground()
            {
                return Program.IsInForeground();
            }

            // flush the device list
            public void FlushDeviceList()
            {
                form.devlist = null;
            }

            // check for updates to the device list
            public bool CheckForDeviceUpdates()
            {
                // if any of the special viewer/test windows are open, skip the update check
                if (form.PlungerWindowOpen || form.JoystickViewerOpen 
                    || form.TvOnTesterOpen || form.ButtonTesterOpen || form.OutputTesterOpen
                    || form.IRLearnOpen)
                    return false;

                // get the new device list
                List<DeviceInfo> newDevList = DeviceInfo.FindDevices();

                // compare it
                if (form.devlist == null || !form.devlist.SequenceEqual(newDevList, new DeviceInfo.Comparer()))
                {
                    // it's different - replace our old list with the new one and tell the caller
                    // we found changes
                    form.devlist = newDevList;
                    return true;
                }
                else
                {
                    // no changes - dispose of the new list (so that we don't keep file handles
                    // open unnecessarily) and tell the caller there are no changes
                    newDevList.ForEach(d => d.Dispose());
                    return false;
                }
            }

            // open the documents folder in Windows Explorer
            public void ShowDocsFolder()
            {
                Process.Start(Program.dlFolder);
            }

            // get a list of .bin files in the docs folder
            public String ListBinFiles()
            {
                IEnumerable<String> files = Directory.EnumerateFiles(Program.dlFolder, "*.bin");
                List<String> arr = new List<String>();
                foreach (String file in files)
                    arr.Add("\"" + file.JSStringify() + "\"");
                return "[" + String.Join(",", arr) + "]";
            }

            // get the latest .bin file
            public String LatestBinFile()
            {
                String latest = null;
                IEnumerable<String> files = Directory.EnumerateFiles(Program.dlFolder, "Pinscape Controller *.bin");
                foreach (String file in files)
                {
                    if (latest == null || String.Compare(file, latest, true) > 0)
                        latest = file;
                }

                // return the latest file we found
                return latest;
            }

            // get/set program options
            public String GetOption(String key)
            {
                return Program.options.ValueOrDefault(key);
            }
            public void SetOption(String key, String val)
            {
                Program.options[key] = val;
            }

            // Start an update check.  If 'once' is true, we do the update
            // only if we've never done one on this run.  Otherwise we start
            // a new one as long as one isn't already running.
            public void CheckForDownloads(bool once)
            {
                if (!once || !form.startedUpdateCheck)
                    form.StartUpdateCheck();
            }

            // Show a help window
            public void ShowHelp(String file)
            {
                (new Help(file)).Show();
            }

            // Format a Build ID.  Formats the flat YYYYMMDDhhmm string into
            // a more human-readable date/time format.
            String FormatBuildID(String id)
            {
                return id != null ? Regex.Replace(id, @"(\d\d\d\d)(\d\d)(\d\d)(\d\d)(\d\d)", "$1-$2-$3-$4$5") : "";
            }

            // get the basic identifying information for a device
            public String GetDeviceInfo(String id)
            {
                // find the device list entry
                DeviceInfo dev = form.GetDeviceByCPUID(id);
                if (dev == null)
                    return null;

                // get the configuration report
                DeviceInfo.ConfigReport cfg = dev.GetConfigReport();
                String configInfo = "";
                if (cfg != null)
                {
                    configInfo = String.Format(@",""NumOutputs"":{0}", cfg.numOutputs)
                        + String.Format(@",""FreeHeapBytes"":{0}", cfg.freeHeapBytes)
                        + String.Format(@",""SBXPBX"": {0}", cfg.sbxpbx ? "true" : "false")
                        + String.Format(@",""AccelFeatures"": {0}", cfg.accelFeatures ? "true" : "false")
                        + String.Format(@",""ChimeLogicFeatures"": {0}", cfg.chimeLogicFeatures ? "true" : "false")
                        + String.Format(@",""ReportTimingFeatures"": {0}", cfg.reportTimingFeatures ? "true" : "false");
                }

                // return the basic info from the device list entry
                return "({"
                    + String.Format(@"""PinscapeUnitNum"":{0}, ""LedWizUnitNum"": {1}, ""CPUID"": ""{2}"", ""BuildID"": ""{3}"", ",
                        dev.PinscapeUnitNo, 
                        dev.LedWizUnitNo, 
                        dev.CPUID, 
                        FormatBuildID(dev.BuildID))

                    + String.Format(@"""OpenSDAID"": ""{0}"", ", dev.OpenSDAID)

                    + String.Format(@"""PlungerEnabled"": {0}, ""JoystickEnabled"": {1}",
                        dev.PlungerEnabled ? 1 : 0,
                        dev.JoystickEnabled ? 1 : 0)

                    + configInfo

                    + "})";
            }

            // Get a list of all connected OpenSDA drives.  Returns a Dictionary
            // where the keys are the OpenSDA TUID strings (all upper case), and the 
            // values are strings encoding javascript objects of the form:
            //
            //   { sdaid: sdaid, path: path, volumeLabel: volumeLabel }
            //
            private Dictionary<String, String> GetSDADrives(bool includeOldBootLoaders)
            {
                // search all drives
                Dictionary<String, String> dict = new Dictionary<String, String>();
                foreach (DriveInfo d in DriveInfo.GetDrives())
                {
                    // Proceed if the device is ready, is a removable drive, and is in
                    // the valid size range for a KL25Z.
                    // Proceed only if the volume is ready and its label matches the device
                    // name.  The volume label is part of the OpenSDA spec, so we should be 
                    // able to count on it as a necessary condition that this is in fact an 
                    // OpenSDA volume.  It's not a sufficient condition, as the user could
                    // have coincidentally chosen the same label for a regular hard disk
                    // or thumb drive, but it at least helps reduce false positives.
                    if (d.IsReady 
                        && d.DriveType == DriveType.Removable
                        && d.TotalSize >= 128000000 && d.TotalSize < 128*1024*1024)
                    {
                        // The OpenSDA spec says that the volume label for a KL25Z with
                        // the current SDA boot loader will be "FRDM-KL25Z", so consider
                        // it a KL25Z if we match that label.  This isn't a perfect test,
                        // since a user could assign the same label to just about actual
                        // disk-like media (like a hard disk or a thumb drive), but it
                        // should at least minimize false positives.
                        //
                        // If we're including devices with the old factory boot loader,
                        // the volume label may be "BOOTLOADERAPP" or something similar.
                        // 
                        bool include = false;
                        if (d.VolumeLabel == "FRDM-KL25Z")
                        {
                            // it has the right volume label - include it
                            include = true;
                        }
                        else if (includeOldBootLoaders && Regex.IsMatch(d.VolumeLabel, @".*BOOTLOADER.*"))
                        {
                            // It has the old boot loader volume name.  Check to see if 
                            // a valid SDA_INFO.HTM file is present.  If so, and it specifies
                            // "BOARD" type FRDM-KL25Z, include it.
                            SDAInfo sdaInfo = ReadSDAInfo(d.RootDirectory.Name);
                            if (sdaInfo != null
                                && sdaInfo.SDAID != null
                                && sdaInfo.BootVer != null
                                && Regex.IsMatch(sdaInfo.Board, @".*KL25Z.*"))
                            {
                                include = true;
                            }
                        }
                        
                        // if we passed the volume label test, check for the SDA ID
                        if (include)
                        {  
                            // get the full SDA ID
                            SDAInfo sdaInfo = ReadSDAInfo(d.RootDirectory.Name);
                            if (sdaInfo != null && sdaInfo.SDAID != null)
                            {
                                // Truncate the SDAID to the last 80 bits.  This is 10 bytes,
                                // or 20 hex digits, plus two extra characters for the dashes
                                // in the formatted version (XXXX-XXXXXXXX-XXXXXXXX).
                                String sdaid = sdaInfo.SDAID.Substring(sdaInfo.SDAID.Length - 22);

                                // add it to the dictionary
                                dict[sdaid] = "({"
                                    + "path:\"" + d.RootDirectory.Name.JSStringify() + "\","
                                    + "sdaid:\"" + sdaid.JSStringify() + "\","
                                    + "volumeLabel:\"" + d.VolumeLabel.JSStringify() + "\","
                                    + "totalSize:" + d.TotalSize + ","
                                    + "bootVer:{text:\"" + sdaInfo.BootVer.Text + "\","
                                    +    "major:" + sdaInfo.BootVer.Major + ","
                                    +    "minor:" + sdaInfo.BootVer.Minor + "},"
                                    + "appVer:{text:\"" + sdaInfo.AppVer.Text + "\","
                                    +    "major:" + sdaInfo.AppVer.Major + ","
                                    +    "minor:" + sdaInfo.AppVer.Minor + "}"
                                    + "})";
                            }
                        }
                    }
                }

                // return the dictionary
                return dict;
            }

            // SDA info struct
            class SDAInfo
            {
                public class Version
                {
                    public String Text = null;
                    public int Major;
                    public int Minor;
                }

                // boot loader version, in the format N.NN, and parsed into major and minor fields
                public Version BootVer;

                // application version
                public Version AppVer;

                // OpenSDA TUID, in the format XXXXXXXX-XXXXXXXX-XXXXXXXX-XXXXXXXX
                public String SDAID = null;

                // "BOARD" ID - "FRDM-KL25Z" for one of ours
                public String Board = null;
            }

            // Read the OpenSDA TUID from an OpenSDA drive.  This returns the
            // full 128-bit TUID, in the format XXXXXXXX-XXXXXXXX-XXXXXXXX-XXXXXXXX.
            private SDAInfo ReadSDAInfo(String root)
            {
                // Look for SDA_INFO.HTM in the root directory.  This is another
                // part of the OpenSDA spec.  Again, a regular hard disk could have
                // the same file, so this isn't a guarantee either.
                String fname = Path.Combine(root, "SDA_INFO.HTM");
                if (File.Exists(fname))
                {
                    SDAInfo info = new SDAInfo();

                    try
                    {
                        // read the file 
                        String s = File.ReadAllText(fname);

                        // extract the value from an <input name="NAME" value="VALUE"> field
                        Func<String, String> extract = (String name) =>
                        {
                            // try matching the <input name="NAME"> part
                            Match match = Regex.Match(s, @"(?i)<input.*name=(['""])" + name + @"\1.*>");
                            if (match.Success)
                            {
                                // got it - look for the value="VALUE" part
                                match = Regex.Match(match.Value, @"(?i)value=(['""])(.*?)\1");
                                if (match.Success)
                                {
                                    // extract and return the value
                                    return match.Groups[2].Value;
                                }
                            }

                            // not found
                            return null;
                        };

                        // extra a version string
                        Func<String, SDAInfo.Version> extractVsn = (String name) =>
                        {
                            String vs = extract(name);
                            if (vs != null)
                            {
                                Match match = Regex.Match(vs, @"(\d+)\.(\d+)");
                                if (match.Success)
                                {
                                    SDAInfo.Version version = new SDAInfo.Version();
                                    version.Text = vs;
                                    version.Major = int.Parse(match.Groups[1].Value);
                                    version.Minor = int.Parse(match.Groups[2].Value);

                                    return version;
                                }
                            }

                            // not found
                            return null;
                        };

                        // extract our fields
                        info.SDAID = extract("TUID");           // unique hardware ID
                        info.BootVer = extractVsn("BOOTVER");   // boot loader version
                        info.AppVer = extractVsn("APPVER");     // SDA application version
                        info.Board = extract("BOARD");          // hardware type

                        // return the results
                        return info;
                    }
                    catch (Exception)
                    {
                        // couldn't read the file - no SDA ID
                    }
                }

                // no SDA file
                return null;
            }

            // Get a list of all OpenSDA drives currently attached, as a string encoding
            // a javascript array of objects with the drive names and SDA IDs.
            public String AllSDADrives(bool includeOldBootLoaders)
            {
                // get the drive list, and turn it into a js array
                return "[" + String.Join(",", GetSDADrives(includeOldBootLoaders).Values) + "]";
            }

            // Get the OpenSDA root directory for a given device entry.  This searches
            // for a mounted disk drive containing an SDA_INFO.HTM in its root directory
            // with a TUID that matches the stored TUID in the device.
            public String FindSDADrive(String sdaid)
            {
                // if they didn't provide an ID, there's nothing to find
                if (sdaid == null)
                    return null;

                // get the drive list
                Dictionary<String, String> dict = GetSDADrives(false);

                // return the entry for our SDA ID
                sdaid = sdaid.ToUpper();
                return dict.ContainsKey(sdaid) ? dict[sdaid] : null;
            }

            // Install firmware.  
            //
            // If cpuidForBackup is provided, we use it load the saved config backup for 
            // the device and patch the data into the firwmare in the host-loaded config
            // area.  Pass null to use the device's factory defaults.
            public String InstallFirmware(String sdaPath, string binFile, string cpuidForBackup)
            {
                // get the SDA ID from the drive
                SDAInfo sdaInfo = ReadSDAInfo(sdaPath);
                if (sdaInfo == null || sdaInfo.SDAID == null)
                    return "({status:\"error\",message:\"Impossible de lire l'ID OpenSDA à partir du lecteur de port de programmation ("
                        + sdaPath.JSStringify() + ". Ce n'est peut-être pas un lecteur OpenSDA.\"})";

                // If we're going to apply a backup of the config data, load the backup
                List<VarData> configVars = null;
                String xconfig = null;
                if (cpuidForBackup != null)
                {
                    string cfgErr = ReadConfigFile(cpuidForBackup, "backup", out configVars, out xconfig);
                    if (cfgErr != null)
                        return cfgErr;
                }

                // Parse the OpenSDA ID.  It's of the form XXXXXXXX-XXXXXXXX-XXXXXXXX-XXXXXXX;
                // parse the hex digit pairs into consecutive bytes.
                byte[] sdabytes = new byte[16];
                for (int i = 0, ofs = 0, bidx = 0 ; i < 4 ; ++i, ++ofs)
                {
                    for (int j = 0 ; j < 4 ; ++j, ofs += 2)
                        sdabytes[bidx++] = Byte.Parse(sdaInfo.SDAID.Substring(ofs, 2), System.Globalization.NumberStyles.HexNumber);
                }

                // load the bin file into memory
                byte[] filebytes;
                try
                {
                    filebytes = File.ReadAllBytes(binFile);
                }
                catch (Exception ex)
                {
                    return "({status:\"error\",message:\"Erreur lors de la lecture du fichier du micrologiciel: " + ex.Message.JSStringify() + "\"})";
                }

                // search for the SDA ID patch bytes in the file
                byte[] pat = Encoding.ASCII.GetBytes("///Pinscape.OpenSDA.TUID///");
                int idx = filebytes.IndexOf(pat);
                if (idx < 0)
                    return "({status:\"error\",message:\"Le fichier du micrologiciel ne contient pas d'octets de correctif OpenSDA ID\"})";
                
                // the actual patch buffer starts after the prefix string
                idx += pat.Length;

                // patch the OpenSDA ID into the byte array
                for (int i = 0; i < sdabytes.Length; ++i)
                    filebytes[idx + i] = sdabytes[i];

                // If we have config data, look for the host-loaded config area so we can
                // patch it in.
                bool configStored = false;
                if (configVars != null)
                {
                    // look for the area signature
                    pat = Encoding.ASCII.GetBytes("///Pinscape.HostLoadedConfig//");
                    idx = filebytes.IndexOf(pat);
                    if (idx >= 0)
                    {
                        // Found it.  Get the available data area size - that's in the
                        // two bytes following the signature.
                        idx += 30;
                        int areaLen = filebytes[idx] | (filebytes[idx + 1] << 8);
                        idx += 2;

                        // make sure we have room for all variables - each takes 8 bytes
                        if (areaLen > configVars.Count * 8)
                        {
                            // patch in the variables
                            configVars.ForEach(v =>
                            {
                                int ofs = idx;
                                filebytes[ofs++] = 66;            // Set Config Variable command ID
                                filebytes[ofs++] = v.varID;       // variable ID
                                if (v.arrIdx != 0)                // array variables have a non-zero index
                                    filebytes[ofs++] = v.arrIdx;  // ...so store it if present
                                for (int j = 0; j < v.data.Length ; ++j)   // store the data bytes
                                    filebytes[ofs++] = v.data[j];

                                // advance to the next block - always 8 bytes
                                idx += 8;
                            });

                            // we've successfully stored the config data
                            configStored = true;
                        }
                    }
                }

                // clear the cached device list - we'll need to re-fetch the SDA information
                // after the update
                form.devlist = null;

                // write the patched file to the SDA drive
                try
                {
                    // Generate a random name for the output file.  The OpenSDA drive doesn't
                    // actually store anything - it simply treats the .bin file as an executable
                    // image, and copies the bytes into the target processor's flash memory.
                    // But Windows thinks it's a regular USB disk drive, so it layers its normal
                    // caching onto the drive.  The point of the random name is simply to give
                    // each download has a unique name, to prevent Windows from trying to combine
                    // a new download with a previous download in its cache.
                    byte[] randbytes = new byte[16];
                    (new Random()).NextBytes(randbytes);
                    String outFile = Path.Combine(
                        sdaPath,
                        String.Join("", randbytes.Select(b => b.ToString("X2"))) + ".bin");

                    // write the data to a temporary file
                    String tmpFile = Path.GetTempFileName();
                    File.WriteAllBytes(tmpFile, filebytes);

                    // Copy the file to the OpenSDA drive via the Windows shell copy operation.
                    // It's important to use the shell file copier, for reasons I haven't figured
                    // out.  Directly writing via the Win32 file API or via the C# file functions
                    // works *most* of the time, but not always.  Sometimes, following no pattern
                    // I can detect, those writes will *appear* to work, but won't trigger the
                    // OpenSDA drive to actually install the .bin file.  It's perplexing because
                    // Windows Explorer will show that the file was copied, and will show it with
                    // its correct contents.  But there's a huge caveat to that, which is the
                    // OpenSDA drive doesn't actually store anything - it's basically write-only
                    // for firmware files, with a file write triggering the firmware install on
                    // the target device via the internal cross-module debug & programming
                    // connection.  The appearance that the .bin file exists on the drive is
                    // actually an illusion created by the Windows disk cache.  So the fact that
                    // the file appears to have been copied correctly only tells us that *Windows*
                    // thinks the file was successfully copied - it doesn't tell us anything about
                    // what actually happened internally on the OpenSDA drive.  
                    //
                    // My best guess about what's going on is that the OpenSDA software on the
                    // KL25Z has some special code that's explicitly designed to recognize the
                    // sequence of device I/O calls that result from a Windows desktop copy, and
                    // that regular low-level Win32 file I/O calls don't trigger this special
                    // sequence the device is looking for.  There are some vague references to 
                    // this sort of "magic" in a couple of Q&A exchanges from the developers on
                    // the Freescale site.  In particular, the notorious Win 8+ incompatibility
                    // in the older factory boot loader firmware is supposed to be related to 
                    // the search index files that Windows creates on newly attached drives in
                    // Win 8+.  The fix was probably to make sure that a Shell copy operation
                    // was triggering the writes rather than something else, by looking for
                    // some distinctive pattern of file system calls.  I'm sure we could figure
                    // out what that distinctive pattern is via the kernel debugger, but given
                    // that the whole point of the SDA code is probably to detect Shell copy 
                    // operations, the correct solution is to actually do a Shell copy.  And
                    // happily, that solution isn't just correct, but is also fairly easy,
                    // since Microsoft was good enough to expose Shell copying as an API.
                    //
                    // Note that showing the UI isn't necessarily required for the magic call
                    // sequence (although it might be), but I'm using it anyway because it adds
                    // feedback on the copy progress.  I think this improves the UX since the
                    // copy takes a few seconds.  I also like the reassurance that the copy is
                    // actually happening, especially after observing the flaky behavior of the
                    // other ways of attempting this.
                    FileSystem.CopyFile(tmpFile, outFile, UIOption.AllDialogs);

                    // delete the temp file
                    File.Delete(tmpFile);
                }
                catch (Exception ex)
                {
                    return "({status:\"error\",message:\"Erreur d'écriture du fichier du micrologiciel: " + ex.Message.JSStringify() + "\"})";
                }

                // make the xconfig data active, if present
                if (xconfig != null)
                    PutDeviceXConfig(cpuidForBackup, xconfig);

                // success
                return "({status:\"ok\","
                    + "configStored:" + (configStored ? "true" : "false") + ","
                    + "message:\"Firmware file " + binFile.JSStringify()
                    + " installed onto " + sdaPath.JSStringify()
                    + " (OpenSDA ID " + sdaInfo.SDAID.JSStringify() + ")\"})";
            }

            // Browse for a file.  'type' gives the file type to select:
            //
            //   "bin" -> firmware .bin file
            //
            // Returns the filename on success, null if the user cancels.
            public String BrowseForFile(String type)
            {
                OpenFileDialog dlg = new OpenFileDialog();
                switch (type)
                {
                    case "bin":
                        dlg.Filter = "Binary files|*.bin|All Files|*.*";
                        break;

                    default:
                        dlg.Filter = "All Files|*.*";
                        break;
                }

                dlg.FilterIndex = 1;
                if (dlg.ShowDialog() == DialogResult.OK)
                    return dlg.FileName;
                else
                    return null;
            }

            // Save the device config settings to a file.  "mode" specifies what kind
            // of file to use: 
            //
            //   "backup" -> automatically choose a backup file in Documents\Pinscape directory 
            //               based on CPU ID
            //   "browse" -> show a Save File dialog to let the user browse for a file
            //
            public String SaveDeviceConfig(String cpuid, String mode = "browse")
            {
                // show a wait cursor while we're reading the config data from the device
                Cursor.Current = Cursors.WaitCursor;

                // find the device
                DeviceInfo dev = form.GetDeviceByCPUID(cpuid);
                if (dev == null)
                    return "({status:\"error\",message:\"Cet appareil KL25Z n'est pas disponible actuellement. "
                            + "Veuillez vérifier qu'il est branché et réessayer. \"}) ";

                // query the number of variables
                byte[] buf = dev.QueryConfigVar(0);
                if (buf == null)
                    return "({status:\"error\",message:\"Impossible d'interroger les descripteurs de variable de configuration.\"})";
                byte nScalar = buf[0];
                byte nArray = buf[1];

                // set up a list to hold all of the variables
                List<byte[]> vars = new List<byte[]>();
                byte[] s;

                // query the scalar variables
                for (byte i = 1; i <= nScalar; ++i)
                { 
                    // add this scalar: the first byte is the variable ID, the rest is the variable data
                    s = dev.QueryConfigVar(i);
                    if (s == null)
                        return "({status:\"error\",message:\"Erreur lors de l'interrogation de la variable scalaire " + i + "\"})";
                    vars.Add(new byte[] { i }.Concat(s.Slice(0, 6)));
                }

                // query the array variables
                for (byte i = 0; i < nArray ; ++i)
                {
                    // query the number of array slots
                    byte arrayVar = (byte)(256 - nArray + i);
                    buf = dev.QueryConfigVar(arrayVar, 0);
                    if (buf == null)
                        return "({status:\"error\",message:\"Erreur lors de l'interrogation du nombre d'emplacements pour la variable de tableau " + i + "\"})";

                    // query the individual slots
                    byte slots = buf[0];
                    for (byte j = 1; j <= slots; ++j)
                    {
                        // add this array variable: the first byte is the variable ID, the second is the
                        // array index, and the rest is the variable data
                        s = dev.QueryConfigVar(arrayVar, j);
                        if (s == null)
                            return "({status:\"error\",message:\"Erreur lors de l'interrogation de la variable de tableau " + i + "[" + j + "]\"})";
                        vars.Add(new byte[] { arrayVar, j }.Concat(s.Slice(0, 5)));
                    }
                }

                // save the data to a file
                return WriteConfigFile(dev, null, mode, vars, GetDeviceXConfig(cpuid));
            }

            // Write settings to a file.  The variables are specified in the
            // same format as for PutDeviceConfig:
            //
            //    vardata ; vardata ; vardata ...
            //
            // where each 'vardata' element is a byte array, in text format, like this:
            //
            //    byte byte byte ...
            //    
            // The bytes are encoded as decimal values.
            //
            // 'mode' has the same meaning as in SaveDeviceConfig().  
            //
            // The CPUID is optional.  If it's omitted, we'll simply leave out any device 
            // information from the file.  The device information is only for reference anyway, 
            // so it's not vital.
            //
            // 'comments' is a string containing comment text to add to the file; this is
            // added at the start of the file, after the standard header comments.  This can
            // be null.  Multiple lines can be provided by separating lines with \n.
            public String WriteConfigFile(String cpuid, String comments, String mode, 
                String vardata, String xconfig)
            {
                // get the device information, if a CPUID was provided
                DeviceInfo dev = (cpuid != null && cpuid != "" ? form.GetDeviceByCPUID(cpuid) : null);

                // convert the string of ";"-delimited vardata entries to a list of byte arrays
                List<byte[]> vars = new List<byte[]>();
                vardata.Split(';').ForEach(v => 
                    vars.Add(v.Split(' ').ToList().Select(b => (byte)int.Parse(b)).ToArray()));

                // save the list to a file
                return WriteConfigFile(
                    dev, comments != null ? comments.Split('\n') : null, mode,
                    vars, xconfig);
            }

            // Save configuration variables to a file.  The variables are specified as a list
            // of byte arrays.  Each byte array is in the basic format for sending to the
            // device via USB: the first byte is the variable ID, and the rest is the variable
            // data.  For an array variable, the second byte is the array index.  Returns a
            // javavscript status object (encoded as a string) in our usual format.
            private String WriteConfigFile(DeviceInfo dev, String[] comments, String mode,
                List<byte[]> vars, String xconfig)
            {
                // "backup" mode isn't allowed if we don't have a device - switch browse mode
                if (mode == "backup" && dev == null)
                    mode = "browse";

                // select the file
                String filename;
                switch (mode)
                {
                    case "browse":
                    default:
                        // ask for a file
                        SaveFileDialog dlg = new SaveFileDialog();
                        dlg.Filter = "Pinscape Config Files|*.psconfig|All Files|*.*";
                        dlg.FilterIndex = 1;
                        if (dlg.ShowDialog() == DialogResult.OK)
                            filename = dlg.FileName;
                        else
                            return "({status:\"cancel\"})";
                        break;

                    case "backup":
                        // choose a backup file name automatically based on the CPU ID
                        filename = Path.Combine(Program.dlFolder, "Settings Backup - " + dev.CPUID + ".psconfig");
                        break;
                }

                try
                {
                    // open the file 
                    StreamWriter f = new StreamWriter(filename);
                    try
                    {
                        // write header comments
                        f.WriteLine("# Pinscape Controller configuration");
                        if (dev != null)
                            f.WriteLine("# CPU ID " + dev.CPUID + ", OpenSDA ID " + dev.OpenSDAID);
                        f.WriteLine("# " + DateTime.Now.ToString("F"));

                        // add any comments
                        if (comments != null)
                            comments.ForEach(c => f.WriteLine("# " + c));

                        // add a blank line after the comment block
                        f.WriteLine();

                        // make an indexed list of the variable descriptor string, to provide
                        // a more human-readable interpretation for known variables in the file
                        Dictionary<int, String> descs = new Dictionary<int, String>();
                        foreach (String d in form.configVarDesc)
                        {
                            Match m = Regex.Match(d, @"\d+");
                            descs[int.Parse(m.Value)] = d;
                        }

                        // write the variables
                        foreach (byte[] b in vars)
                        {
                            // the variable index is always the first byte
                            byte varID = b[0];

                            // if there's a variable descriptor, write a comment
                            if (descs.ContainsKey(varID))
                            {
                                // get the descriptor
                                String desc = descs[varID];

                                // check the descriptor to see if it's an array variable
                                if (b.Length >= 2 && Regex.IsMatch(desc, @"\d+\[\]"))
                                {
                                    // write in array format
                                    byte arrIdx = b[1];
                                    byte[] data = b.Slice(2).PadTo(5);
                                    f.WriteLine("# " + varID + "[" + arrIdx + "] = " + ConfigVarToJS(data, varID, arrIdx, desc));
                                    f.WriteLine(varID + "[" + arrIdx + "]: " + String.Join(" ", data.Select(e => e.ToString("D"))));
                                }
                                else
                                {
                                    // write in scalar format
                                    byte[] data = b.Slice(1).PadTo(6);
                                    f.WriteLine("# " + varID + " = " + ConfigVarToJS(data, varID, 0, desc));
                                    f.WriteLine(varID + ": " + String.Join(" ", data.Select(e => e.ToString("D"))));
                                }
                            }
                        }

                        // write the xconfig data, if present
                        if (xconfig != null)
                            f.WriteLine("###XCONFIG=" + Regex.Replace(xconfig, "[\r\n]", " "));
                    }
                    finally
                    {
                        // done with the file
                        f.Close();
                    }
                }
                catch (Exception e)
                {
                    return "({status:\"error\","
                        + "filename:\"" + filename.JSStringify() + "\","
                        + "message:\"Erreur d'écriture du fichier: " + e.Message.JSStringify() + "\"})";
                }

                // success
                return "({status:\"ok\","
                    + "filename:\"" + filename.JSStringify() + "\","
                    + "message:\"La configuration a été enregistrée avec succès.\"})";
            }

            // variable data loaded from file
            struct VarData
            {
                public byte varID;      // variable ID
                public byte arrIdx;     // array index; 0 for a scalar variable
                public byte[] data;     // value, in wire format
            };
 
            // Restore the device config settings from a file.
            // 'mode' specifies the file mode, using the same conventions as SaveDeviceConfig().
            public String RestoreDeviceConfig(String cpuid, String mode = "browse")
            {
                // find the device
                DeviceInfo dev = form.GetDeviceByCPUID(cpuid);
                if (dev == null)
                    return "({status:\"error\",message:\"Cet appareil KL25Z n'est pas disponible actuellement. "
                            + "Veuillez vérifier qu'il est branché et réessayer. \"}) ";

                // read the file
                List<VarData> vars;
                String xconfig;
                String status = ReadConfigFile(cpuid, mode, out vars, out xconfig);

                // if that failed, return the error information
                if (status != null)
                    return status;

                // send the new configuration to the device
                vars.ForEach(v =>
                {
                    // send the variable update to the device
                    if (v.arrIdx != 0)
                        dev.SetConfigVar(v.varID, v.arrIdx, v.data);
                    else
                        dev.SetConfigVar(v.varID, v.data);
                });

                // save configuration and reboot
                dev.SaveConfigAndReboot();

                // since we're rebooting, flush the device list
                form.devlist = null;

                // make the new xconfig active
                if (xconfig != null)
                    PutDeviceXConfig(cpuid, xconfig);

                // success
                return "({status:\"ok\",message:\"La configuration a été restaurée avec succès sur la KL25Z.\"})";
            }

            // Read a configuration file, returning a javascript result object.  On
            // success, we'll set result.status to "ok" as usual, and set result.config
            // to the configuration data, using the same object structure as returned by
            // GetDeviceConfig().  On error, we'll return the error information in the 
            // usual way through the status object.
            public String ReadConfigFile(String cpuid, String mode)
            {
                // start by loading the file into a VarData list
                List<VarData> vars;
                String xconfig;
                String result = ReadConfigFile(cpuid, mode, out vars, out xconfig);
                
                // if that failed, return the status
                if (result != null)
                    return result;

                // Build a dictionary of the variable descriptors indexed by variable
                // ID for fast access.  Each entry is a list, to allow for array 
                // variables.  Scalars will only occupy one list slot.
                Dictionary<byte, List<VarData>> varDict = new Dictionary<byte, List<VarData>>();
                vars.ForEach(v =>
                {
                    List<VarData> lst;
                    if (varDict.ContainsKey(v.varID))
                        lst = varDict[v.varID];
                    else
                    {
                        lst = new List<VarData>();
                        varDict.Add(v.varID, lst);
                    }
                    lst.Add(v);
                });

                // run through the variable descriptors and format each value into a 
                // javascript expression
                List<String> jsdata = new List<String>();
                form.configVarDesc.ForEach(desc =>
                {
                    // break the descriptor up into its constituent fields
                    String[] d = desc.Split(' ');

                    // get the variable ID, and note if it's an array
                    byte varID = byte.Parse(Regex.Match(d[0], @"\d+").Value);
                    bool isArray = Regex.IsMatch(d[0], @"^\d+\[\]");

                    // if the variable exists in the file, format it into the result
                    if (varDict.ContainsKey(varID))
                    {
                        // get the variable
                        List<VarData> vlst = varDict[varID];

                        // format the scalar or array, as appropriate
                        if (isArray)
                        {
                            // build the sub-object based on array indices
                            List<String> arrObj = new List<String>();
                            vlst.ForEach(v =>
                                arrObj.Add(ConfigVarToJS(v.data, varID, v.arrIdx, v.arrIdx.ToString(), d[2])));

                            // combine it into an object
                            jsdata.Add(d[1] + ":{" + String.Join(",", arrObj) + "}");
                        }
                        else
                        {
                            // it's a simple scalar - just add the first list element
                            VarData v = vlst[0];
                            jsdata.Add(ConfigVarToJS(v.data, varID, 0, d[1], d[2]));
                        }
                    }
                });

                // build the data entries into an object
                String jsConfig = "{" + String.Join(",", jsdata) + "}";

                // use an empty object if we didn't get any xconfig data
                if (xconfig == null)
                    xconfig = "{ }";

                // wrap the result in a successful status object
                return "({status:\"ok\",config:" + jsConfig + ",xconfig:" + xconfig + "})";
            }

            // Read configuration variables from a file.  On success, fills in 'vars' 
            // with a list of VarData elements describing the variables read from the 
            // file, fills in 'xconfig' with the external configuration data from the
            // file, and returns null.  On error, returns a javascript-encoded status 
            // object with the error information, following our usual conventions.
            private String ReadConfigFile(String cpuid, String mode, 
                out List<VarData> vars, out String xconfig)
            {
                // presume we won't have anything to return
                vars = null;
                xconfig = null;
                
                // if no CPU ID was passed in, we can't use "backup" mode, since that
                // mode requires a CPU ID to use as the basis of the filename
                if (cpuid == null && mode == "backup")
                    mode = "browse";
                
                // select the file
                String filename;
                switch (mode)
                {
                    case "browse":
                    default:
                        // ask for a file
                        OpenFileDialog dlg = new OpenFileDialog();
                        dlg.Filter = "Pinscape Config Files|*.psconfig|All Files|*.*";
                        dlg.FilterIndex = 1;
                        if (dlg.ShowDialog() == DialogResult.OK)
                            filename = dlg.FileName;
                        else
                            return "({status:\"cancel\"})";
                        break;

                    case "backup":
                        // choose a backup file name automatically based on the CPU ID
                        filename = Path.Combine(Program.dlFolder, "Settings Backup - " + cpuid + ".psconfig");
                        break;
                }

                // set up the list to hold the variable data we read
                vars = new List<VarData>();

                int lineno = 0;
                try
                {
                    // open the file
                    StreamReader f = new StreamReader(filename);
                    try
                    {
                        // read each line
                        for (lineno = 1 ; ; lineno++)
                        {
                            // read the next line, stop at EOF
                            String l = f.ReadLine();
                            if (l == null)
                                break;

                            // check for the xconfig data - this looks like a comment so
                            // that older versions will simply ignore it
                            Match m = Regex.Match(l, @"^\s*###XCONFIG=(.*)$");
                            if (m.Success)
                            {
                                xconfig = m.Groups[1].Value;
                                continue;
                            }

                            // skip comments
                            if (Regex.IsMatch(l, @"^\s*#|^\s*$"))
                                continue;

                            // parse the line
                            m = Regex.Match(l, @"^\s*(\d+)\s*(\[\s*(\d+)\s*\]\s*)?:(\s*\d+)+\s*$");
                            if (!m.Success)
                                return "({status:\"error\",message:\"Syntaxe incorrecte à la ligne " + lineno + "\"})";

                            // set up the variable data descriptor
                            VarData v = new VarData();
                            v.varID = byte.Parse(m.Groups[1].Value);
                            v.arrIdx = (m.Groups[2].Captures.Count == 0 ? (byte)0 : byte.Parse(m.Groups[3].Value));

                            // read the data bytes
                            v.data = new byte[m.Groups[4].Captures.Count];
                            for (int i = 0; i < v.data.Length; ++i)
                                v.data[i] = byte.Parse(m.Groups[4].Captures[i].Value);

                            // add it to the list
                            vars.Add(v);
                        }
                    }
                    finally
                    {
                        // done with the file
                        f.Close();
                    }
                }
                catch (Exception e)
                {
                    return "({status:\"error\", message:\"Erreur de lecture du fichier"
                        + (lineno > 0 ? " at line " + lineno : "") + ": "
                        + e.Message.JSStringify() + "\"})";
                }

                // success - return no status string
                return null;
            }

            // Get the full configuration for a given device
            public String GetDeviceConfig(String id)
            {
                // Loop until we get the configuration.  
                //
                // It seems to happen once in a while that Windows exposes the 
                // USB interface for the device before it's really ready to 
                // field requests.  To handle this situation more gracefully 
                // and transparently, give the device a moment to come to its
                // senses, then try again.  To minimize the wait time when it's
                // just a short hiccup, make each wait short; to make sure we 
                // don't give up too early, do several of these short waits.
                for (int tries = 0; tries < 10; ++tries)
                {
                    // find the device
                    DeviceInfo dev = form.GetDeviceByCPUID(id);
                    if (dev == null)
                        return null;

                    // try getting the configuration
                    String config = TryGetDeviceConfig(dev);

                    // if that succeeded, return the result
                    if (config != null)
                        return config;

                    // wait a moment to give the device time to finish connecting,
                    // if indeed that's the problem
                    Thread.Sleep(500);

                    // flush the device list, in case the device changed interfaces
                    // while we were waiting
                    FlushDeviceList();
                }

                // failed
                return null;
            }

            private String TryGetDeviceConfig(DeviceInfo dev)
            {
                // show the wait cursor while working
                Cursor.Current = Cursors.WaitCursor;

                // set up a list for the output
                List<String> ret = new List<String>();

                // query the number of scalar and array config variables supported
                // in the device firmware
                byte[] buf = dev.QueryConfigVar(0);
                if (buf == null)
                    return "({\"error\":\"Erreur lors de la lecture du nombre de variables\"})";

                // decode the counts
                byte nScalar = buf[0];
                byte nArray = buf[1];

                // grab each variable
                foreach (String d in form.configVarDesc)
                {
                    // decompose the variable descriptor string
                    String[] v = d.Split(' ');

                    // check for array variables
                    Match m = Regex.Match(v[0], @"(\d+)\[\]");
                    if (m.Success)
                    {
                        // set up a list to hold the array elements
                        List<String> arr = new List<String>();

                        // It's an array variable.  Get the variable ID.  If it's
                        // not present on the device, simply add an empty array.
                        byte vid = byte.Parse(m.Groups[1].Value);
                        if (vid >= 256 - nArray)
                        {
                            // Query the array size from the device.  The array size 
                            // is obtained by querying this variable with index == 0.
                            buf = dev.QueryConfigVar(vid, 0);
                            if (buf == null)
                                return "({\"error\":\"Erreur lors de l'interrogation de la taille du tableau pour var " + vid + "\"})";

                            // read the maximum index
                            byte maxIdx = buf[0];

                            // process each array element
                            for (byte i = 1; i <= maxIdx; ++i)
                            {
                                // retrieve this value
                                buf = dev.QueryConfigVar(vid, i);
                                if (buf == null)
                                    return "({\"error\":\"Erreur lors de la récupération de la valeur de var " + vid + "[" + i + "]\"})";

                                // add it to the list
                                arr.Add(ConfigVarToJS(buf, vid, i, i.ToString(), v[2]));

                                // Special case: If this is the "outputs" array, stop
                                // when we reach the first disabled output (type==0 in
                                // first byte of message packet).  The first disabled
                                // output marks the end of the in-use outputs, even
                                // though the array size might be larger.  Simply fill
                                // out the remaining array elements with repeats of the
                                // disabled output in this case.
                                if (vid == 255 && buf != null && buf[0] == 0)
                                {
                                    // it's the first disabled entry - repeat it for
                                    // the rest of the array
                                    for (++i; i <= maxIdx; ++i)
                                        arr.Add(ConfigVarToJS(buf, vid, i, i.ToString(), v[2]));
                                }
                            }
                        }

                        // turn the array into an object string and add it to the results
                        ret.Add(v[1] + ":{" + String.Join(",", arr) + "}");
                    }
                    else
                    {
                        // scalar - get the variable ID
                        byte vid = Byte.Parse(v[0]);

                        // check if it's present on the device
                        if (vid <= nScalar)
                        {
                            // present - retrieve the data from the device
                            buf = dev.QueryConfigVar(vid);
                            if (buf == null)
                                return "({\"error\":\"Erreur lors de la récupération de la valeur de var " + vid + "\"})";

                            // add it to the list
                            ret.Add(ConfigVarToJS(buf, vid, 0, v[1], v[2]));
                        }
                        else
                        {
                            // not present - use defaults
                            ret.Add(ConfigVarDefaultsToJS(vid, 0, v[1], v[2]));
                        }

                    }
                }

                // combine the list into javascript object notation
                return "({" + String.Join(",", ret) + "})";
            }

            // Get the configuration variable descriptors
            public String GetConfigVarDescs()
            {
                return String.Join("|", form.configVarDesc);
            }

            // Get the external configuration (xconfig) for a given device.
            // The xconfig is additional config data that's stored locally
            // in the PC file system rather than on the device.  This is used
            // for settings that aren't needed at run-time and take up too
            // much space to be practical to store on the device, such as
            // descriptive strings for output ports and stored IR commands.
            //
            // The data to and from the UI window is in JSON format.  We
            // simply store the JSON data as a text file with a name based
            // on the CPU ID.
            public String GetDeviceXConfig(String CPUID)
            {
                // try loading the file
                try
                {
                    return File.ReadAllText(Path.Combine(Program.dlFolder, CPUID + ".xconfig"));
                }
                catch
                {
                    return "{ }";
                }
            }

            // Write the external configuration (xconfig) for a given device.
            public String PutDeviceXConfig(String CPUID, String xc)
            {
                try
                {
                    File.WriteAllText(Path.Combine(Program.dlFolder, CPUID + ".xconfig"), xc);
                    return @"({status:""ok"",message:""Données de configuration externes enregistrées.""})";
                }
                catch (Exception e)
                {
                    return @"({status:""erreur"",message:""Une erreur s'est produite lors de l'enregistrement de "
                        + @"données de configuration externes dans le système de fichiers local. (Erreur de fichier: "
                        + e.Message + @")""})";
                }
            }

            // Put the device configuration.  The new configuration variable list
            // is specified in 's' like this:
            //
            //    vardata ; vardata ; vardata ...
            //
            // Each 'vardata' element is a byte array, in text format, like this:
            //
            //    byte byte byte ...
            //
            // Each byte value is specified as a decimal integer.  Each byte array is
            // in a format suitable for sending to the device in a "set config variable"
            // message: the first byte is the variable ID, and the following bytes are
            // the variable value.  For an array variable, the second byte is the array
            // index (>=1).  
            public String PutDeviceConfig(String cpuid, String s)
            {
                // get the device ID
                DeviceInfo dev = form.GetDeviceByCPUID(cpuid);
                if (dev == null)
                    return "({status:\"erreur\",message:\"Cet appareil KL25Z n'est actuellement pas accessible. "
                        + "Assurez-vous qu'il est branché via son port joystick.\"})";

                // presume success
                bool ok = true;

                // Split the string by ";" delimiters and visit each byte list.  For each
                // byte list, split it by space delimiters, convert the decimal number strings
                // to actual byte arrays.  Then do a SetConfigVar call on each byte array.
                // If a call fails, note it in 'ok', but keep going.
                s.Split(';').ForEach(ss =>
                    ok = ok && dev.SetConfigVar(ss.Split(' ').Select(b => (byte)int.Parse(b))));

                // if all writes succeeded, save the updates
                if (ok)
                {
                    // save updates and reboot the KL25Z
                    ok = dev.SaveConfigAndReboot();

                    // rebooting the device invalidates the device list, so forget it
                    form.devlist = null;
                }

                // return results
                if (ok)
                    return "({status:\"ok\",message:\"Les nouveaux paramètres ont été programmés avec succès dans la KL25Z.\"})";
                else
                    return "({status:\"erreur\",message:\"Une erreur s'est produite lors de la mise à jour des paramètres. Vous pouvez "
                        + "réessayez. Si le problème persiste, essayez de réinitialiser la KL25Z manuellement et réessayez.\"});";
            }

            // Format a device configuration variable, given the descriptor table entry
            private String ConfigVarToJS(byte[] buf, byte varid, byte varidx, String desc)
            {
                String[] d = desc.Split(' ');
                return ConfigVarToJS(buf, varid, varidx, varidx == 0 ? d[1] : d[1] + "[" + varidx + "]", d[2]);
            }

            // Format a device configuration variable query into javascript object format.
            // If varidx is zero, this is a scalar variable; otherwise it's an array variable.
            private String ConfigVarToJS(byte[] buf, byte varid, byte varidx, String varname, String format)
            {
                // translate the format string:
                //    $W  -> 16-bit word
                //    $D  -> 32-bit dword
                //    $B  -> 8-bit byte
                //    $P  -> port, as a GPIO pin number
                //    $o  -> output port: port type code and port number
                int idx = 0;
                format = Regex.Replace(format, @"\$(.)", delegate(Match m)
                {
                    String ret = "";
                    String ch = m.Groups[1].Value;
                    switch (ch)
                    {
                        case "D":
                            // DWORD - 32-bit unsigned int from little-endian byte[4]
                            ret = "0x"
                                + (buf[idx] 
                                + (buf[idx + 1] << 8) 
                                + (buf[idx + 2] << 16)
                                + (buf[idx + 3] << 24)).ToString("X");
                            idx += 4;
                            break;

                        case "W":
                            // WORD - 16-bit unsigned int from little-endian byte[2]
                            ret = (buf[idx] + (buf[idx + 1] << 8)).ToString();
                            idx += 2;
                            break;

                        case "B":
                            // BYTE - 8-bit unsigned int from byte
                            ret = buf[idx].ToString();
                            idx += 1;
                            break;

                        case "P":
                            // PORT - port name from ID byte
                            ret = "\"" + DeviceInfo.WireToPinName(buf[idx]) + "\"";
                            idx += 1;
                            break;

                        case "o":
                            // OUTPUT PORT - port type and pin name or number.  The
                            // first byte of the message is the type code:
                            //    0=Disabled
                            //    1=GPIO PWM Out
                            //    2=GPIO Digital Out
                            //    3=TLC5940 Out
                            //    4=74HC595 Out
                            //    5=Virtual Out
                            //    6=TLC59116 Out
                            // The second byte is the port number.  For GPIO pins, this
                            // is a PinName code.  For others, it's simply the index of the
                            // port on the chip.
                            String pin;
                            switch (buf[idx])
                            {
                                case 1:
                                case 2:
                                    // GPIO port - it's a pin name
                                    pin = "\"" + DeviceInfo.WireToPinName(buf[idx + 1]) + "\"";
                                    break;

                                default:
                                    // for others it's a port index
                                    pin = "" + buf[idx + 1];
                                    break;
                            }
                            ret = "{"
                                + "type:" + buf[idx] + ","
                                + "pin:" + pin
                                + "}";
                            idx += 2;
                            break;

                        default:
                            // substitute the original character
                            ret = "$" + ch;
                            break;
                    }
                    return ret;
                });

                // add the name prefix
                return varname + ":" + format;
            }

            // Generate default values in javascript format for a configuration variable
            private String ConfigVarDefaultsToJS(byte varid, byte varidx, String varname, String format)
            {
                // translate the format string:
                //    $W  -> 16-bit word
                //    $D  -> 32-bit dword
                //    $B  -> 8-bit byte
                //    $P  -> port, as a GPIO pin number
                //    $o  -> output port: port type code and port number
                format = Regex.Replace(format, @"\$(.)", delegate(Match m)
                {
                    String ret = "";
                    String ch = m.Groups[1].Value;
                    switch (ch)
                    {
                        case "D":   // DWORD - 32-bit unsigned int
                        case "W":   // WORD - 16-bit unsigned int
                        case "B":   // BYTE - 8-bit unsigned int
                            // all integer types default to zero
                            ret = "0";
                            break;

                        case "P":
                            // PORT - GPIO port name from ID byte
                            // defaults to "NC" (Not Connected)
                            ret = "\"NC\"";
                            break;

                        case "o":
                            // OUTPUT PORT - port type and pin name or number.  Defaults
                            // to disabled (0).
                            ret = "{type:0,pin:0}";
                            break;

                        default:
                            // substitute the original character
                            ret = "$" + ch;
                            break;
                    }
                    return ret;
                });

                // add the name prefix
                return varname + ":" + format;
            }

            public String GetBuildInfo()
            {
                return "({"
                    + "BuildNumber:\"" + VersionInfo.BuildNumber + "\","
                    + "Date:\"" + VersionInfo.BuildDate.ToString("yyyy-MM-dd") + "\""
                    + "})";
            }

            public String GetDiagnosticCounters(String cpuid)
            {
                DeviceInfo dev = form.GetDeviceByCPUID(cpuid);
                if (dev != null)
                {
                    List<String> v = new List<String>();
                    v.Add("mainLoopIterTime:" + GetDiagnosticCounter(dev, 1));
                    v.Add("mainLoopMsgTime:" + GetDiagnosticCounter(dev, 2));
                    v.Add("pwmUpdateTime:" + GetDiagnosticCounter(dev, 3));
                    v.Add("lwFlashUpdateTime:" + GetDiagnosticCounter(dev, 4));
                    v.Add("plungerScanTime:" + GetDiagnosticCounter(dev, 30));

                    List<String> l = new List<String>();
                    for (int i = 0; i < 12; ++i)
                        l.Add(GetDiagnosticCounter(dev, (byte)(i + 5)).ToString());
                    v.Add("mainLoopIterCheckpt:[" + String.Join(",", l) + "]");

                    return "({" + String.Join(",", v.ToArray()) + "})";
                }
                return "";
            }

            private int GetDiagnosticCounter(DeviceInfo dev, byte n)
            {
                byte[] buf = dev.QueryConfigVar(220, n);
                if (buf != null)
                    return buf[0] | (buf[1] << 8) | (buf[2] << 16) | (buf[3] << 16);
                else
                    return 0;
            }

            // Send an IR command.  The code is in our printable format:
            // protocol.flags.code, all parts as hex numbers.
            public String SendIRCommand(String cpuid, String code)
            {
                // look up the device
                DeviceInfo dev = form.GetDeviceByCPUID(cpuid);
                if (dev == null)
                    return "({status:\"erreur\",message:\"L'appareil ne semble pas connecté.\"})";

                // parse the code
                Match m = Regex.Match(code ?? "", @"(?i)([0-9a-f]+)\.([0-9a-f]+)\.([0-9a-f]+)");
                if (!m.Success)
                    return "({status:\"erreur\",message:\"Le code IR n'est pas formaté correctement. "
                        + "Utilisez Protocol.Flags.Code, avec chaque partie sous forme de nombre hexadécimal.\"})";

                // pull out the sections
                int protocol = int.Parse(m.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);
                int flags = int.Parse(m.Groups[2].Value, System.Globalization.NumberStyles.HexNumber);
                UInt64 cmd = UInt64.Parse(m.Groups[3].Value, System.Globalization.NumberStyles.HexNumber);

                // Send the two-part USB message.  Special request 15 sets up the 
                // protocol, flags, and low 32 bits of the command code.  Request
                // 16 fills out the high 32 bits and executes the command.
                dev.SpecialRequest(15, new byte[]{ 
                    (byte)protocol, (byte)flags, 
                    (byte)(cmd & 0xff), (byte)((cmd >> 8) & 0xff), 
                    (byte)((cmd >> 16) & 0xff), (byte)((cmd >> 24) & 0xff)
                });

                dev.SpecialRequest(16, new byte[]{
                    (byte)((cmd >> 32) & 0xff), (byte)((cmd >> 40) & 0xff),
                    (byte)((cmd >> 48) & 0xff), (byte)((cmd >> 56) & 0xff)
                });

                // success
                return "({status:\"ok\",message:\"La commande IR a été envoyée.\"})";
            }

            // Send updates to a group of output ports.  The protocol requires
            // us to send updates in groups of seven ports.  The base port is
            // given in the logical device port numbering scheme, starting at
            // port 0.  This must always be a multiple of 7 because of the
            // way the USB protocol is defined.  The port settings are given
            // as PWM duty cycles on a linear scale from 0 (fully off) to 255 
            // (fully on).
            public void SetOutputPorts(String cpuid, int basePort, byte pwm1, byte pwm2, byte pwm3, byte pwm4, byte pwm5, byte pwm6, byte pwm7)
            {
                // get the device
                DeviceInfo dev = form.GetDeviceByCPUID(cpuid);
                if (dev != null)
                {
                    // Set up the USB message buffer for the 'extended set brightness' 
                    // command: 
                    //   byte 0 = 0      -> report ID, always 0
                    //   byte 1 = 200+n  -> command ID, 200 + port group (group 0 for ports
                    //                      0-6, group 1 for ports 7-13, etc)
                    //   byte 2..8       -> brightness level, one byte per port, level 0-255
                    byte[] buf = new byte[9] { 
                    0,                          // report ID
                    (byte)(200 + basePort/7),   // command ID 200 + port group number
                    pwm1, pwm2, pwm3, pwm4, pwm5, pwm6, pwm7 // PWM values
                };

                    // send the command
                    dev.WriteUSB(buf);
                }
            }

            // Set/clear night mode
            public void SetNightMode(String cpuid, bool on)
            {
                // get the device
                DeviceInfo dev = form.GetDeviceByCPUID(cpuid);
                if (dev != null)
                {
                    // send a request to invert the night mode status in the device
                    byte b = (byte)(on ? 1 : 0);
                    dev.SpecialRequest(8, new byte[] { b });
                }
            }

            // Get night mode status
            public bool IsNightMode(String cpuid)
            {
                // presume night mode is off
                bool result = false;

                // get the device
                DeviceInfo dev = form.GetDeviceByCPUID(cpuid);
                if (dev != null)
                {
                    // read a report to get the status
                    byte[] rpt = dev.ReadStatusReport();
                    if (rpt != null)
                    {
                        // night mode is bit 0x02 of status byte [1]
                        result = (rpt[1] & 0x02) != 0;
                    }
                }

                // return the result
                return result;
            }

        };

        // our script interface object
        ScriptInterface ifc;

        private void MainSetup_FormClosing(object sender, FormClosingEventArgs e)
        {
            // give the form a chance to object
            object result = webBrowser1.Document.InvokeScript("OnCloseWindow");
            if (result != null && result.ToString() == "cancel")
                e.Cancel = true;
        }

        private void webBrowser1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.Alt && e.KeyCode == Keys.F4) Close();
        }

        private void broadcastTimer_Tick(object sender, EventArgs e)
        {
            // send our broadcast notification periodically
            Program.BroadcastNotification(1);
        }

    }
}
