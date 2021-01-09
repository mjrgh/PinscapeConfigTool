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
using System.Windows.Input;
using System.IO;

namespace PinscapeConfigTool
{
    public partial class JoystickViewer : Form
    {
        public JoystickViewer(DeviceInfo dev)
        {
            this.dev = new DeviceInfo(dev);
            InitializeComponent();
        }

        // device handle
        DeviceInfo dev;

        // crosshairs image
        Image crosshairs = PinscapeConfigTool.Resources.crosshairs;

        // current joystick position from the background thread
        int x, y, z;

        // current joystick button state
        uint jsButtons = 0;

        // log entries pending addition to the UI, and mutex to protect them
        List<String> log = new List<String>();
        Mutex logMutex = new Mutex();

        // flag: window closing, background thread should exit
        bool done;

        // starting time for logging purposes
        DateTime t0;

        private void JoystickViewer_Shown(object sender, EventArgs e)
        {
            // note the original window size
            oldFormWidth = ActiveForm.Width;
            oldFormHeight = ActiveForm.Height;

            // note the starting time
            t0 = DateTime.Now;

            // add the header to the text box
            textBox1.AppendText("X\tY\tZ\tTime\r\n\r\n");

            // start the updater thread
            Thread th = new Thread(Updater);
            done = false;
            th.Start();

            // hide the Center Now button if the new accelerometer
            // features aren't present in the firmware
            DeviceInfo.ConfigReport cfg = dev.GetConfigReport();
            if (cfg != null && !cfg.accelFeatures)
                linkCenter.Visible = false;
        }

        Font menuFont = SystemFonts.MenuFont;

        private void JoystickViewer_FormClosed(object sender, FormClosedEventArgs e)
        {
            // dispose of GDI resources
            jsOn.Dispose(); jsOn = null;
            jsOff.Dispose(); jsOff = null;
            crosshairs.Dispose(); crosshairs = null;
            menuFont.Dispose();

            // tell the background thread to terminate
            done = true;

            // dispose of the device object
            dev.Dispose();
        }

        // updater thread
        void Updater()
        {
            // get a private copy of the device for the thread
            using (DeviceInfo tdev = new DeviceInfo(dev))
            {
                // loop until done
                while (!done)
                {
                    // pause 10ms between checks
                    Thread.Sleep(10);

                    // catch up to real time on the input
                    tdev.FlushReadUSB();

                    // read input from the USB interface
                    byte[] buf = tdev.ReadUSB();

                    // if it's not a regular status report, skip it
                    if (buf == null || (buf[2] & 0x80) != 0)
                        continue;

                    // Read the X, Y, and Z coordinates from the report.  These are
                    // 16-bit signed values.
                    x = (int)(Int16)(buf[9] + (buf[10] << 8));
                    y = (int)(Int16)(buf[11] + (buf[12] << 8));
                    z = (int)(Int16)(buf[13] + (buf[14] << 8));

                    // read the button state
                    jsButtons = (uint)(buf[5] | (buf[6] << 8) | (buf[7] << 16) | (buf[8] << 24));

                    // if logging keys, add to the log
                    if (logging)
                    {
                        logMutex.WaitOne();
                        log.Add(x + "\t" + y + "\t" + z + "\t" + (DateTime.Now - t0).TotalMilliseconds + "ms");
                        logMutex.ReleaseMutex();
                        logged = true;
                    }
                    else if (logged)
                    {
                        // add a blank line after each run of log entries
                        logMutex.WaitOne();
                        log.Add("");
                        logMutex.ReleaseMutex();
                        logged = false;
                    }
                }
            }
        }

        private void lblAccel_Paint(object sender, PaintEventArgs e)
        {
            // get the drawing context
            Graphics g = e.Graphics;
            int wid = lblAccel.Width, ht = lblAccel.Height;

            // figure the positions proportionally within the box, with the
            // zero point at the center
            int xprop = (int)(x * (wid / 2)/4095.0) + (wid / 2);
            int yprop = (int)(y * (ht / 2)/4095.0) + (ht / 2);

            // overwrite the background and draw the crosshairs
            g.FillRectangle(Brushes.White, 0, 0, wid, ht);
            g.DrawImage(crosshairs, xprop, yprop);

            // update the text label
            lblXY.Text = x + ", " + y;
        }

        private void lblPlunger_Paint(object sender, PaintEventArgs e)
        {
            // get the drawing context
            Graphics g = e.Graphics;
            int wid = lblPlunger.Width, ht = lblPlunger.Height;

            // get the z position proportional to the box area, with the
            // zero point at 1/6 from the left
            int zprop = (int)(z * (wid*5.0/6.0)/4095.0 + (wid/6.0));

            // draw a bar showing the plunger position
            g.FillRectangle(Brushes.White, 0, 0, zprop, ht);
            g.FillRectangle(Brushes.Green, zprop, 0, wid, ht);

            // update the text label
            lblZ.Text = z.ToString();
        }

        int oldFormWidth, oldFormHeight;
        private void JoystickViewer_Resize(object sender, EventArgs e)
        {
#if false
            // note the new size and the delta from the last size
            int wid = ActiveForm.Width, ht = ActiveForm.Height;
            int dx = wid - oldFormWidth, dy = ht - oldFormHeight;
            oldFormWidth = wid;
            oldFormHeight = ht;

            // adjust the title bar and icon
            lblTitleBar.Width = wid;
            lblIcon.Left += dx;

            // adjust the text box, separator bar, and save button
            lblTextBoxBar.Width = wid;
            textBox1.Width += dx;
            textBox1.Height += dy;
            btnSave.Left += dx;
#endif
        }

        bool logging = false;
        bool logged = false;
        private void timer1_Tick(object sender, EventArgs e)
        {
            // get the log mutex, and exchange the log with a fresh one
            logMutex.WaitOne();
            List<String> ourLog = null;
            if (log.Count != 0)
            {
                ourLog = log;
                log = new List<String>();
            }
            logMutex.ReleaseMutex();

            // add each item from the log to the text box
            if (ourLog != null)
            {
                foreach (String s in ourLog)
                    textBox1.AppendText(s + "\r\n");
            }

            // note the new logging state
            logging = (Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) > 0
                || (Keyboard.GetKeyStates(Key.RightCtrl) & KeyStates.Down) > 0;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // ask for a file
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Text Files|*.txt|All Files|*.*";
            dlg.FilterIndex = 1;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(dlg.FileName, textBox1.Text);
                }
                catch (Exception exc)
                {
                    MessageBox.Show("Une erreur s'est produite lors de l'écriture du fichier: " + exc.Message);
                }
            }
        }

        private void btnLogging_Click(object sender, EventArgs e)
        {
            btnLogging.Visible = false;
            ActiveForm.Height += lblBottomMarker.Top - lblTextBoxBar.Top;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            // update the viewer
            lblAccel.Invalidate();
            lblPlunger.Invalidate();
            jsKeyState.Invalidate();
        }

        Image jsOn = new Bitmap("html\\jskeySmallOn.png");
        Image jsOff = new Bitmap("html\\jskeySmallOff.png");
        private void jsKeyState_Paint(object sender, PaintEventArgs e)
        {
            StringFormat centerText = new StringFormat();
            centerText.Alignment = StringAlignment.Center;
            centerText.LineAlignment = StringAlignment.Center;

            int wid = jsOn.Width, ht = jsOn.Height;
            int padding = 3;

            Graphics g = e.Graphics;
            int row = 0, col = 0;
            Rectangle src = new Rectangle(0, 0, wid, ht);
            Font font = menuFont;
            for (int i = 0; i < 32; ++i)
            {
                // get the status
                bool on = (jsButtons & (1 << i)) != 0;

                // figure the position of this button image
                int x = padding + col * (wid + padding);
                int y = padding + row * (ht + padding);

                // draw the ON or OFF image
                Rectangle dst = new Rectangle(x, y, wid, ht);
                g.DrawImage(on ? jsOn : jsOff, dst, src, GraphicsUnit.Pixel);

                // draw the button number
                String num = (i + 1).ToString();
                g.MeasureString(num, font);
                g.DrawString(num, font, on ? Brushes.White : Brushes.Gray, new Point(x + wid / 2, y + ht / 2), centerText);

                // on to the next column 
                if (++col > 15)
                {
                    col = 0;
                    ++row;
                }
            }
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            (new Help("HelpJoystickViewer.htm")).Show();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void linkCenter_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            dev.SpecialRequest(14);
        }
    }
}
