using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Plasmoid.Extensions;
using System.IO;
using CollectionUtils;

namespace PinscapeConfigTool
{
    public partial class IRLearn : Form
    {
        public IRLearn(DeviceInfo dev)
        {
            InitializeComponent();
            this.dev = new DeviceInfo(dev);
        }

        // result command
        public Command result = null;

        private void IRLearn_Load(object sender, EventArgs e)
        {
            // start the exposure reader thread
            done = false;
            thread = new Thread(this.IRThread);
            thread.Start();

            // remember the details text
            origDetails = lblDetails.Text;
        }

        private void IRLearn_FormClosed(object sender, FormClosedEventArgs e)
        {
            dev.Dispose();
        }

        private void IRLearn_FormClosing(object sender, FormClosingEventArgs e)
        {
            done = true;
            thread.Join();
        }

        // IR monitor thread entrypoint
        private void IRThread()
        {
            // create a clone of the device record to get a private handle
            // for the thread
            using (DeviceInfo tdev = new DeviceInfo(dev))
            {
                // keep going until the main thread tells us to stop via the 'done' flag
                while (!done)
                {
                    // read a status report
                    byte[] buf = tdev.ReadUSB();
                    if (buf == null)
                    {
                        // no status report - sleep for a little bit so that we don't
                        // suck up tons of CPU if we get repeated failures
                        Thread.Sleep(250);
                        continue;
                    }

                    // If it's a regular status report, and the "IR learning mode"
                    // status bit (0x20) is cleared, the device is no longer in
                    // learning mode.  If we think we're in learning mode, we must
                    // have missed the mode exit message.  Count this as a failure
                    // since we didn't see a successful code report.
                    if (buf[2] == 0 && (buf[1] & 0x20) == 0)
                    {
                        // protect resources while working
                        if (mutex.WaitOne(1000))
                        {
                            // if we think we're in learning mode, this is an 
                            // unexpected exit, so count it as a failure
                            if (mode == Mode.Learning)
                                mode = Mode.Failed;

                            // done with the mutex
                            mutex.ReleaseMutex();
                        }
                    }

                    // check if it's an IR report
                    if (buf[1] == 0x00 && buf[2] == 0xA2)
                    {
                        // grab the mutex while working
                        if (mutex.WaitOne(1000))
                        {
                            // check which type of raw data report we have
                            switch (buf[3])
                            {
                                case 0:
                                    // 0 -> learning mode timed out
                                    mode = Mode.Failed;
                                    break;

                                case 0xFF:
                                    // 0xFF -> command learned
                                    mode = Mode.Success;
                                    command = new Command(buf[4], buf[5],
                                        (UInt64)buf[6] 
                                        + ((UInt64)buf[7] << 8) 
                                        + ((UInt64)buf[8] << 16) 
                                        + ((UInt64)buf[9] << 24)
                                        + ((UInt64)buf[10] << 32)
                                        + ((UInt64)buf[11] << 40)
                                        + ((UInt64)buf[12] << 48)
                                        + ((UInt64)buf[13] << 56));
                                    break;

                                default:
                                    // Anything else is a raw IR sample count.  Add the
                                    // samples to the raw data list.
                                    newData = true;
                                    for (int i = 0, ofs = 4 ; i < buf[3] ; ++i, ofs += 2)
                                        data.Add((UInt16)(buf[ofs] + (buf[ofs+1] << 8)));
                                    break;
                            }

                            // done updating
                            mutex.ReleaseMutex();
                        }
                    }

                }
            }
        }

        // device interface
        private DeviceInfo dev;

        // flag - window closed; background thread uses this to tell when to exit
        private bool done;

        // IR reader thread
        private Thread thread;

        // synchronization mutex, for protecting variables shared with the IR thread
        private Mutex mutex = new Mutex();

        // Raw IR data list.  Negative numbers are spaces, positive numbers are marks.
        List<UInt16> data = new List<UInt16>();

        // do we have a data update?
        bool newData = false;

        // Learned command
        public class Command
        {
            public Command(byte protocol, byte flags, UInt64 code)
            {
                this.protocol = protocol;
                this.flags = flags;
                this.code = code;
            }

            override public String ToString()
            {
                return protocol.ToString("X") + "." + flags.ToString("X") + "." + code.ToString("X");
            }
            public String Details()
            {
                return ToString() + (protocol != 0 ? " (" + protocolInfo() + ")" : "");
            }

            String protocolInfo()
            {
                List<String> f = new List<String>();
                f.Add(protocol < protocolNames.Length ? protocolNames[protocol] : "No Protocol");
                if ((flags & 0x02) != 0)
                    f.Add("Dittos");
                return String.Join(", ", f);
            }

            public String protocolName
            {
                get { return protocol < protocolNames.Length ? protocolNames[protocol] : "None";  }
            }

            public byte protocol;
            public byte flags;
            public UInt64 code;

            // Printable names for Pinscape protocol ID integer codes.  See 
            // IRRemote/IRProtocolID.h in Pinscape firmware for the ID list.
            static String[] protocolNames = new String[]{
                "No Protocol",  // 0
                "NEC32", // 1
                "NEC32X", // 2
                "NEC48", // 3
                "Philips-RC5", // 4
                "Philips-RC6", // 5
                "Kaseikyo48", // 6
                "Kaseikyo56", // 7
                "Denon-K", // 8
                "Fujitsu48", // 9
                "Fujitsu56", // 10
                "JVC48", // 11
                "JVC56", // 12
                "Mitsubishi-K", // 13
                "Panasonic48", // 14
                "Panasonic56", // 15
                "Sharp-K", // 16
                "Teac-K", // 17
                "Denon15", // 18
                "Pioneer", // 19
                "Samsun20", // 20
                "Samsung36", // 21
                "Sony8", // 22
                "Sony12", // 23
                "Unassigned", // 24
                "Sony15", // 25
                "Sony20", // 26
                "OrtecMCE", // 27
                "Lutron", // 28
            };
        }
        public Command command = null;

        // current mode
        enum Mode
        {
            Starting,
            Learning,
            Failed,
            Success
        }
        Mode oldMode = Mode.Starting;
        Mode mode = Mode.Starting;

        // close the form
        private void btnClose_Click(object sender, EventArgs e)
        {
            ActiveForm.Close();
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            (new Help("HelpIRLearn.htm")).Show();
        }

        private void IRLearn_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 27)
                ActiveForm.Close();
        }

        DateTime flashTime = DateTime.Now;
        String origDetails;
        private void timerRefresh_Tick(object sender, EventArgs e)
        {
            // check for a mode change
            if (mode != oldMode)
            {
                switch (mode)
                {
                    case Mode.Success:
                        lblStatus.Text = "SUCCESS";
                        lblStatus.ForeColor = Color.DarkGreen;
                        lblDetails.Text = "Command code " + command.Details() + " recognized.  Click Save to store this code.";
                        btnSave.Enabled = true;
                        lnkRedo.Visible = true;
                        lnkRawData.Visible = true;
                        break;

                    case Mode.Failed:
                        lblStatus.Text = "FAILED";
                        lblStatus.ForeColor = Color.DarkRed;
                        if (data.Count > 0)
                            lblDetails.Text = "The command could not be decoded. You might want to try again. If the "
                                + "code can't be learned after several tries, the remote might use a data format that "
                                + "the Pinscape firmware doesn't recognize. If you contact us about it, include the "
                                + "pulse data below, which will help us identify the remote's data format.";
                        else
                            lblDetails.Text = "No IR signals were detected. Check that the sensor is attached and "
                                + "configured in the software. If the sensor is set up correctly and works with other "
                                + "remotes, your remote might use an IR wavelength that the sensor can't detect.";
                        lnkRedo.Visible = true;
                        lnkRawData.Visible = true;
                        break;

                    case Mode.Learning:
                        lblStatus.Text = "*** LEARNING ***";
                        lblDetails.Text = origDetails;
                        btnSave.Enabled = false;
                        lnkRedo.Visible = false;
                        lnkRawData.Visible = false;
                        break;
                }

                // remember the updated mode
                oldMode = mode;
            }

            // if we're in learning mode, update the display
            if (mode == Mode.Learning)
            {
                // flash the status indicator
                if ((DateTime.Now - flashTime).TotalMilliseconds > 500)
                {
                    lblStatus.ForeColor = (lblStatus.ForeColor == Color.Blue ? Color.Purple : Color.Blue);
                    flashTime = DateTime.Now;
                }

                // protect shared variables
                if (mutex.WaitOne(100))
                {
                    // update the raw data box if there's new data
                    if (newData)
                    {
                        lblRawData.Invalidate();
                        newData = false;
                    }

                    // done with the mutex
                    mutex.ReleaseMutex();
                }
            }
        }

        private void btnLearn_Click(object sender, EventArgs e)
        {
            // switch to learning mode
            EnterLearningMode();
        }

        private void lnkRedo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // switch to learning mode
            EnterLearningMode();
        }

        void EnterLearningMode()
        {
            if (mode != Mode.Learning)
            {
                // clear any prior learned command and raw data list
                command = null;
                data.Clear();

                // send the learning mode command to the Pinscape unit
                dev.SpecialRequest(12);

                // wait for a status report to come back with the learning mode
                // bit set (0x20 in the first status byte)
                dev.FlushReadUSB();
                for (int i = 0 ; i < 16 ; ++i)
                {
                    byte[] buf = dev.ReadStatusReport();
                    if (buf != null && (buf[1] & 0x20) != 0)
                    {
                        // switch to the learning panel
                        panelStart.Visible = false;
                        panelLearn.Visible = true;

                        // set the status text to Learning
                        lblStatus.Text = "*** LEARNING ***";

                        // now in learning mode
                        mode = Mode.Learning;
                        break;
                    }
                }

                // if we didn't enter learning mode, show an error
                if (mode != Mode.Learning)
                {
                    MessageBox.Show("The Pinscape device didn't acknowledge the request to enter "
                        + "learning mode.\r\n\r\n"
                        + "If you haven't already set up the IR Receiver in the settings, you'll "
                        + "need to do that (and program the KL25Z with the new settings) before "
                        + "the device can learn IR commands.\r\n\r\n"
                        + "Also make sure that an up-to-date version of the firmware is "
                        + "installed, since older versions didn't support the IR features.");
                }
            }
        }

        private void lblRawData_Paint(object sender, PaintEventArgs e)
        {
            // get the gdi handle for drawing onto the control
            Graphics g = e.Graphics;

            // make a private copy of the IR timing list
            List<UInt16> data = new List<UInt16>();
            if (mutex.WaitOne(100))
            {
                this.data.ForEach(d => data.Add(d));
                mutex.ReleaseMutex();
            }

            // sum the times in the list (in 2us increments)
            int usTot = 0;
            data.ForEach(d => usTot += 2*(d & 0xFFFE));

            // Microseconds per pixel.  Most remote codes are about 50ms
            // long, so pick a scale where 50ms fits in the available width.
            // If the actual code is longer, fit the actual length to the
            // available width.
            int cht = lblRawData.Height;
            int cwid = lblRawData.Width;
            int usPerPix = Math.Max(50000, usTot)/cwid;

            // Create a drawing surface for the IR code display
            int yMark = 2, ySpace = cht - 4;
            using (Bitmap bitmap = new Bitmap(cwid, cht))
            {
                Graphics gbitmap = Graphics.FromImage(bitmap);
                int x = 0;
                g.FillRectangle(Brushes.White, 0, 0, cwid, cht);
                data.ForEach(d =>
                {
                    // decode the value
                    bool mark = (d & 0x0001) != 0;
                    int us = 2 * (d & 0xFFFE);
                    int xe = x + us/usPerPix;

                    // draw it
                    Pen pen = Pens.DarkRed;
                    int y = mark ? yMark : ySpace;
                    gbitmap.DrawLine(pen, x, y, xe, y);
                    if (mark)
                    {
                        for (int xx = x ; xx < xe ; xx += 2)
                            gbitmap.DrawLine(pen, xx, yMark, xx, ySpace);
                        gbitmap.DrawLine(pen, xe, yMark, xe, ySpace);
                    }

                    // move past it
                    x = xe;
                });

                // draw the bitmap onto the control area
                g.DrawImage(bitmap, 0, 0, cwid, cht);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (command != null)
            {
                result = command;
                Close();
            }
            else
            {
                MessageBox.Show("No command has been recognized. Please go through the learning "
                    + "procedure to program a command.");
            }
        }

        private void lnkRawData_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            List<String> txt = new List<String>();
            if (mutex.WaitOne(100))
            {
                data.ForEach(d => txt.Add(((d & 0x0001) != 0 ? "/" : "\\") + 2*(d & 0xFFFE)));
                mutex.ReleaseMutex();
            }

            TextViewer win = new TextViewer();
            win.SetText(
                @"These are the raw pulse times that the IR sensor registered. ""/"" represents "
                + @"a ""mark"" or IR ON interval, and ""\"" represents a ""space"" or IR OFF "
                + @"interval. All times are in microseconds. If the Pinscape unit won't recognize "
                + @"a code after several attempts, your remote might use a data format (protocol) "
                + @"that the Pinscape software doesn't recognize. Let the Pinscape developers "
                + @"know about it and we might be able to add support for new protocol. "
                + @"Include this raw timing data in your report, along with the make and model "
                + @"of the TV or other device that the remote control is for."
                + "\r\n\r\n"
                + (txt.Count == 0 ? "*** No signals received ***" : String.Join(" ", txt)));
            win.Text = "Raw IR Data";
            win.Show();
        }

   }
}
