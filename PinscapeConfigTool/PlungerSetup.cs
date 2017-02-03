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
    public partial class PlungerSetup : Form
    {
        public PlungerSetup(DeviceInfo dev)
        {
            InitializeComponent();
            this.dev = new DeviceInfo(dev);
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            // start the exposure reader thread
            done = false;
            thread = new Thread(this.ExposureThread);
            thread.Start();
        }

        private void exposure_Paint(object sender, PaintEventArgs e)
        {
            // figure the number of on-screen pixels per CCD pixel
            int wid = exposure.Width;
            int ht = exposure.Height;

            // get the gdi handle for drawing onto the control
            Graphics g = e.Graphics;

            // for debugging - fill the background with red while waiting for the new pixels
            g.FillRectangle(Brushes.Red, new Rectangle(0, 0, wid, ht));

            // get a private copy of the current pixel array (serializing
            // access via the mutex, since the device reader thread can
            // install a new pixel array at any time)
            mutex.WaitOne();
            byte[] mypix = null;
            if (pix != null)
            {
                mypix = new byte[pix.Length];
                pix.CopyTo(mypix, 0);
            }
            int pos = edgePos;
            int posScale = this.posScale;
            int dir = orientation;
            int npix = this.npix;
            int calMax = this.calMax;
            int calZero = this.calZero;
            bool calMode = this.calMode;
            String t1 = txtInfo_text;
            String t2 = txtInfo2_text;
            String t3 = txtInfo3_text;
            mutex.ReleaseMutex();

            // fill the background with gray when done fetching pixels
            g.FillRectangle(Brushes.DarkGray, 0, 0, wid, ht);

            // create a drawing surface for the pixels and position display
            using (Bitmap bitmap = new Bitmap(posScale != 0 ? posScale : 4096, ht))
            {
                Graphics gbitmap = Graphics.FromImage(bitmap);

                // if we have pixels to display, display them
                if (pix != null && npix != 0 && !calMode)
                {
                    // create a brush for drawing the grays
                    using (SolidBrush graybrush = new SolidBrush(Color.FromArgb(0, 0, 0)))
                    {
                        // draw the pixels onto the bitmap Graphics object
                        for (int i = 0; i < npix; ++i)
                        {
                            byte lum = mypix[i];
                            graybrush.Color = Color.FromArgb(lum, lum, lum);
                            gbitmap.FillRectangle(graybrush, i, 0, i, ht);
                        }
                    }
                }

                // superimpose a green bar showing the plunger position
                if (pos != 0xFFFF)
                {
                    if (dir == 1)
                        gbitmap.FillRectangle(Brushes.Green, pos, ht * 3 / 4, posScale, ht);
                    else
                        gbitmap.FillRectangle(Brushes.Green, pos - npix, ht * 3 / 4, posScale, ht);
                }

                // stretch the bitmap onto the control area
                g.DrawImage(bitmap, 0, 0, wid, ht);
            }

            // draw arrows at the calibration points, if known
            DrawCalArrow(g, calZero, "(Park)", posScale, dir, Brushes.Magenta,
                PinscapeConfigTool.Resources.ArrowUpPurple, PinscapeConfigTool.Resources.ArrowDownPurple);
            DrawCalArrow(g, calMax, "(Max)", posScale, dir, Brushes.Red,
                PinscapeConfigTool.Resources.ArrowUpRed, PinscapeConfigTool.Resources.ArrowDownRed);

            // show the plunger position on top of the green bar
            if (pos != 0xFFFF)
            {
                String msg = pos.ToString();
                Font font = SystemFonts.DefaultFont;
                Size sz = g.MeasureString(msg, font).ToSize();
                Brush br = Brushes.White;
                int xTxt = (int)Math.Round((float)pos / posScale * wid);
                int yTxt = ht * 3 / 4 + (ht / 4 - sz.Height) / 2;
                if (dir == 1)
                {
                    if (xTxt + 2 + sz.Width < wid)
                        g.DrawString(msg, font, br, xTxt + 2, yTxt);
                    else
                    {
                        GraphicsExtension.FillRoundedRectangle(
                            g, Brushes.LightGray, xTxt - sz.Width - 2, yTxt, sz.Width, sz.Height, 2);
                        g.DrawString(msg, font, Brushes.Green, xTxt - sz.Width - 2, yTxt);
                    }
                }
                else
                {
                    if (xTxt - sz.Width - 2 > 0)
                        g.DrawString(msg, font, br, xTxt - sz.Width - 2, yTxt);
                    else
                    {
                        GraphicsExtension.FillRoundedRectangle(
                            g, Brushes.LightGray, xTxt + 2, yTxt, sz.Width, sz.Height, 2);
                        g.DrawString(msg, font, Brushes.Green, xTxt + 2, yTxt);
                    }
                }
            }

            // if in cal mode, show a flashing calibration notice
            if (calMode)
            {
                // invert the color every 500ms
                if ((DateTime.Now - flashTime).TotalMilliseconds > 500)
                {
                    calModeColor = !calModeColor;
                    flashTime = DateTime.Now;
                }
                String msg = "CALIBRATING";
                Font font = SystemFonts.CaptionFont;
                Size sz = g.MeasureString(msg, font).ToSize();
                Brush br = calModeColor ? Brushes.Purple : Brushes.Blue;
                g.DrawString(msg, font, br, (wid - sz.Width) / 2, (ht - sz.Height) / 2);
            }

            // update the statistics text
            txtInfo.Text = t1;
            txtInfo2.Text = t2;
            txtInfo3.Text = t3;
        }
        DateTime flashTime = DateTime.Now;
        bool calModeColor;

        private void DrawCalArrow(
            Graphics g, int pos, String caption, int posScale, int dir,
            Brush br, Image arrowUp, Image arrowDown)
        {
            if (pos != -1)
            {
                // get the drawing area size
                int wid = exposure.Width;
                int ht = exposure.Height;

                // figure the drawing position - mirror it if the orientation is reversed
                int drawpos = (dir == -1 ? posScale - pos : pos);
                
                // get a suitable font
                Font font = SystemFonts.DefaultFont;

                // figure the arrow position
                int xArrow = (int)Math.Round((float)drawpos / posScale * wid - arrowDown.Width / 2);

                // draw the arrow at the top
                g.DrawImage(arrowDown, xArrow, 0);

                // draw the arrow at the bottom
                g.DrawImage(arrowUp, xArrow, ht - arrowUp.Height - 1);

                // show the value next to the arrow
                String msg = pos.ToString();
                if (caption != null)
                    msg += " " + caption;
                Size sz = g.MeasureString(msg, font).ToSize();
                int x = xArrow + (drawpos < posScale / 2 ? arrowDown.Width + 2 : - sz.Width - 2);
                int y = (arrowDown.Height - sz.Height) / 2;
                GraphicsExtension.FillRoundedRectangle(g, Brushes.LightGray, x, y, sz.Width, sz.Height, 2);
                g.DrawString(msg, font, br, x, y);
            }
        }

        public static byte[] gamma = new byte[] 
        {
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0, 
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   1,   1,   1,   1, 
            1,   1,   1,   1,   1,   1,   1,   1,   1,   2,   2,   2,   2,   2,   2,   2, 
            2,   3,   3,   3,   3,   3,   3,   3,   4,   4,   4,   4,   4,   5,   5,   5, 
            5,   6,   6,   6,   6,   7,   7,   7,   7,   8,   8,   8,   9,   9,   9,  10, 
            10,  10,  11,  11,  11,  12,  12,  13,  13,  13,  14,  14,  15,  15,  16,  16, 
            17,  17,  18,  18,  19,  19,  20,  20,  21,  21,  22,  22,  23,  24,  24,  25, 
            25,  26,  27,  27,  28,  29,  29,  30,  31,  32,  32,  33,  34,  35,  35,  36, 
            37,  38,  39,  39,  40,  41,  42,  43,  44,  45,  46,  47,  48,  49,  50,  50, 
            51,  52,  54,  55,  56,  57,  58,  59,  60,  61,  62,  63,  64,  66,  67,  68, 
            69,  70,  72,  73,  74,  75,  77,  78,  79,  81,  82,  83,  85,  86,  87,  89, 
            90,  92,  93,  95,  96,  98,  99, 101, 102, 104, 105, 107, 109, 110, 112, 114, 
            115, 117, 119, 120, 122, 124, 126, 127, 129, 131, 133, 135, 137, 138, 140, 142, 
            144, 146, 148, 150, 152, 154, 156, 158, 160, 162, 164, 167, 169, 171, 173, 175, 
            177, 180, 182, 184, 186, 189, 191, 193, 196, 198, 200, 203, 205, 208, 210, 213, 
            215, 218, 220, 223, 225, 228, 231, 233, 236, 239, 241, 244, 247, 249, 252, 255
        };

        private void Form3_FormClosed(object sender, FormClosedEventArgs e)
        {
            dev.Dispose();
        }

        private void PlungerSetup_FormClosing(object sender, FormClosingEventArgs e)
        {
            done = true;
            thread.Join();
        }

        // pixel data snapshot, for writing to a capture file
        struct PixFrame
        {
            public PixFrame(double t, byte[] pix, int pos)
            {
                this.t = t;
                this.pix = pix;
                this.pos = pos;
            }
            public double t;        // timestamp - milliseconds relative to thread start time
            public byte[] pix;      // pixels
            public int pos;         // detected edge position

            public void Write(StreamWriter s)
            {
                s.WriteLine("{0,8:D}     {1,8:D}   {2}",
                    (int)t, pos, String.Join(" ", pix.Select(b => b.ToString("X2"))));
            }
        };

        // sensor image reader thread entrypoint
        private void ExposureThread()
        {
            // create a clone of the device record to get a private handle
            // for the thread
            using (DeviceInfo tdev = new DeviceInfo(dev))
            {
                // set up a list to hold captured pixel frames
                List<PixFrame> capturedFrames = new List<PixFrame>();

                // close out the pixel file
                Action ClosePixFile = () =>
                {
                    try
                    {
                        // write the remaining frames and close the file
                        capturedFrames.ForEach(frame => frame.Write(pixFile));
                    }
                    catch (Exception)
                    {
                    }
                    try
                    {
                        // done - close the file and forget it
                        pixFile.Close();
                    }
                    catch (Exception)
                    {
                    }
                    pixFile = null;

                    // clear the frame capture list
                    capturedFrames.Clear();
                };

                // thread start time
                DateTime t0Thread = DateTime.Now;

                // note the start time for the current iteration
                DateTime t0 = DateTime.Now;

                while (!done)
                {
                    // If we're not the foreground application, don't update the pixel
                    // display.  This avoids saturating the USB connection while we're
                    // running in the background.
                    //if (!Program.IsInForeground())
                    //{
                    //    Thread.Sleep(250);
                    //    continue;
                    //}

                    // request a plunger sensor status report
                    byte[] buf = tdev.SpecialRequest(3, new byte[] { pixFlags, 0 },
                        (r) => { return r[1] == 0xff && r[2] == 0x87 && r[3] == 0; });

                    // decode the reply
                    if (buf != null)
                    {
                        //   bytes 4:5      -> number of pixels
                        //   bytes 6:7      -> plunger position (0000..FFFF scale)
                        //   byte  8        -> flags: 
                        //                      0x01 = standard orientation
                        //                      0x02 = reversed orientation
                        //                      0x04 = calibration in progress
                        //                      0x08 = error reading position
                        //   bytes 9:10:11  -> average sensor scan time in 10us units
                        //   bytes 12:13:14 -> image processing time for this scan in 10us units

                        // read the pixel count
                        int newnpix = buf[4] + (buf[5] << 8);

                        // the position is reported in terms of the pixel count if this is an
                        // imaging sensor; if not, it's reported on the generic joystick axis
                        // scale (0..4095)
                        int posScale = (newnpix != 0 ? newnpix : 4096);

                        // read the edge position
                        int edgePos = buf[6] + (buf[7] << 8);

                        // read the orientation
                        orientation =
                            (buf[8] & 0x01) != 0 ? 1 :
                            (buf[8] & 0x02) != 0 ? -1 :
                            0;

                        // check for calibraiton mode
                        bool calMode = (buf[8] & 0x04) != 0;

                        // read the average scan time, converting from the 10us units
                        // in the report to milliseconds (10us = 0.01ms -> 1ms = 100 units)
                        long t = buf[9] + ((long)buf[10] << 8) + ((long)buf[11] << 16);
                        avgScanTime = t / 100.0;

                        // read the image processing and convert to milliseconds
                        t = buf[12] + ((long)buf[13] << 8) + ((long)buf[14] << 16);
                        processingTime = t / 100.0;

                        // read the pixel reports if not in calibration mode
                        byte[] newpix = new byte[newnpix];
                        if (!calMode && newnpix != 0)
                        {
                            for (int j = newnpix / 10 + 100; j > 0 && !done; --j)
                            {
                                // read a report
                                buf = tdev.ReadUSB();

                                // only consider reports that are in the special exposure report format
                                if (buf != null && (buf[2] & 0xF8) == 0x80)
                                {
                                    // figure the starting index of the current batch
                                    int idx = (((int)buf[2] & 0x7) << 8) | (int)buf[1];

                                    // Store this batch
                                    for (int k = 3; k < buf.Length && idx < newnpix; k += 1)
                                        newpix[idx++] = buf[k];

                                    // If this is the last batch of pixels, we're done.
                                    if (idx >= newnpix)
                                        j = 1;
                                }
                            }
                        }

                        // If a pixel file is open, and we have a pixel buffer, save the pixels
                        // to our capture list (don't write the file quite yet - we'll defer
                        // that until we're done, to avoid slowing down the capture)
                        if (pixFile != null && npix != 0)
                        {
                            // add it to the pixel list
                            capturedFrames.Add(new PixFrame(
                                (DateTime.Now - t0Thread).TotalMilliseconds, pix, edgePos));

                            // if we have more than 100 frames, flush the file now to avoid
                            // taking up too much memory
                            if (capturedFrames.Count > 100)
                            {
                                try
                                {
                                    // write the remaining frames and close the file
                                    capturedFrames.ForEach(frame => frame.Write(pixFile));
                                }
                                catch (Exception)
                                {
                                }
                                capturedFrames.Clear();
                            }

                            // if it hasn't been 60ms since the last visual update, skip
                            // this visual update and just do another file capture, so that
                            // we capture file data more quickly
                            if ((DateTime.Now - t0).TotalMilliseconds < 60.0)
                                continue;
                        }

                        // query the calibration data (special request 9 = query config
                        // variable, variable ID 15 = plunger calibration data)
                        int calMax = -1, calZero = -1, tRelease = -1;
                        byte[] reply = tdev.QueryConfigVar(13);
                        if (reply != null)
                        {
                            // decode the variables
                            calZero = (int)reply[0] + ((int)reply[1] << 8);
                            calMax = (int)reply[2] + ((int)reply[3] << 8);
                            tRelease = (int)reply[4];

                            // The calibration ranges are in terms of the 0x0000..0xFFFF
                            // scale of the raw sensor readings.  Rescale to the reported
                            // pixel scale.
                            calZero = (int)Math.Round(calZero / 65535.0 * posScale);
                            calMax = (int)Math.Round(calMax / 65535.0 * posScale);
                        }

                        // figure the statistics
                        byte pixMin = 0xFF, pixMax = 0x00;
                        int numSat = 0, numZero = 0;
                        for (int j = 0; j < newnpix; ++j)
                        {
                            byte p = newpix[j];
                            if (p == 0)
                                ++numZero;
                            if (p == 0xff)
                                ++numSat;
                            if (p < pixMin)
                                pixMin = p;
                            if (p > pixMax)
                                pixMax = p;
                        }

                        // lock the mutex while updating shared data
                        mutex.WaitOne();

                        // copy the new pixel list to the drawing copy
                        npix = newnpix;
                        pix = newpix;
                        this.calMax = calMax;
                        this.calZero = calZero;
                        this.tRelease = tRelease;
                        this.edgePos = edgePos;
                        this.posScale = posScale;
                        this.calMode = calMode;

                        // Update the sensor scan time information
                        String times = String.Format(
                            "Average sensor scan time: {0} ms, Curent frame processing time: {1} ms",
                            avgScanTime, processingTime);

                        // update statistics according to sensor type (imaging or non-imaging)
                        if (newnpix != 0)
                        {
                            // Imaging sensor type

                            // line 1 - pixel array information and position
                            txtInfo_text = String.Format(
                                "Pixels: {0}, Orientation: {1}, Edge pos: {2}, Release time: {3} ms",
                                newnpix,
                                orientation == 1 ? "Standard" : orientation == -1 ? "Reversed" : "Unknown",
                                edgePos == 0xFFFF ? "Not detected" : edgePos.ToString(),
                                tRelease);

                            // line 2 - brightness range
                            txtInfo2_text = String.Format("Brightness Min: {0}, Max: {1}, % Saturated: {2}, % Zero: {3}",
                                    pixMin, pixMax, (int)Math.Round(numSat * 100.0 / newnpix),
                                    (int)Math.Round(numZero * 100.0 / npix));

                            // line 3 - scan time
                            txtInfo3_text = times;
                        }
                        else
                        {
                            // Non-imaging sensor type

                            // line 1 - position
                            txtInfo_text = String.Format(
                                "Plunger position: {0}, Orientation: {1}, Release time: {2} ms",
                                edgePos == 0xFFFF ? "Unknown" : edgePos.ToString() + "/" + (posScale - 1),
                                orientation == 1 ? "Standard" : orientation == -1 ? "Reversed" : "Unknown",
                                tRelease);

                            // line 2 - scan time
                            txtInfo2_text = times;

                            // line 3 - unused
                            txtInfo3_text = "";
                        }


                        // done updating
                        mutex.ReleaseMutex();

                        // Wait about 30ms between requests.  This provides a good refresh
                        // rate in the UI (about 60fps, similar to video playback) but
                        // avoids flooding the USB connection with requests.
                        double dt = (DateTime.Now - t0).TotalMilliseconds;
                        if (dt < 30.0)
                            Thread.Sleep(30 - (int)dt);

                        // set the new iteration start time
                        t0 = DateTime.Now;
                    }

                    // if the user asked us to close out the pixel file, do so
                    if (closePixFile && pixFile != null)
                        ClosePixFile();
                }

                // if there's a pixel file, close it before we exit
                if (pixFile != null)
                    ClosePixFile();
            }
        }

        private void btnCal_Click(object sender, EventArgs e)
        {
            BeginCal();
        }

        public void BeginCal()
        {
            // send a calibration request to the device
            dev.SpecialRequest(2);
            calStarted = true;
        }

        bool calStarted;

        void UpdateCalMode()
        {
            if (IsCalMode())
            {
                // calibration is in progress - show instructions
                lblCal.Text = "Calibration in progress. This will run "
                    + "for about 15 seconds. While it's running:\r\n\r\n"
                    + "  • Pull the plunger back\r\n"
                    + "  • Hold for a moment, then release\r\n"
                    + "  • Wait for it to come to rest\r\n"
                    + "  • Repeat a few times as desired\r\n";
                btnCal.Enabled = false;
            }
            else if (calStarted)
            {
                // calibration done
                lblCal.Text = "Calibration completed.\r\n\r\n"
                    + "If you want to repeat the process, make sure the plunger "
                    + "is at rest at its normal park position, then press the "
                    + "Calibrate button.";
                btnCal.Enabled = true;
            }
            else
            {
                // we haven't done the first calibration with this dialog yet
                lblCal.Text = "Calibrate the sensor when you "
                    + "first install it, and whenever you adjust its position. "
                    + "You should also recalibrate if the \"Park\" arrow shown "
                    + "above doesn't match the actual plunger rest position."
                    + "\r\n\r\n"
                    + "To calibrate, make sure the plunger is at rest at its "
                    + "normal park position, then press Calibrate and follow "
                    + "the on-screen instructions.";
                btnCal.Enabled = true;
            }
        }

        private void timerCal_Tick(object sender, EventArgs e)
        {
            // update the calibration mode
            UpdateCalMode();

            // show or hide the pixel-related controls according to whether this
            // is an imaging or non-imaging sensor
            bool show = (npix != 0);
            rbHiRes.Visible = show;
            rbLowRes.Visible = show;
            btnSave.Visible = (!btnStopSave.Visible && show);

            // if we've finished closing the pixel capture file, show the Save button
            if (closePixFile && pixFile == null)
            {
                btnSave.Visible = true;
                btnStopSave.Visible = false;
                closePixFile = false;
            }
        }

        private DeviceInfo dev;       // device info object

        private bool done;            // flag - window closed; background thread uses this to tell when to exit

        private int npix = 0;         // the exposure report tells us the number of pixels
        private int calZero = -1;     // calibrated zero (rest) position, or -1 if not known
        private int calMax = -1;      // calibrated plunger maximum, or -1 if not known
        private int tRelease = -1;    // measured plunger release travel time, in milliseconds
        private byte[] pix;           // pixel array for the exposure meter
        String txtInfo_text;          // pixel statistics text
        String txtInfo2_text;         // statistics text line 2
        String txtInfo3_text;         // statistics text line 3
        int edgePos = 0xffff;         // detected edge pos in current image
        int posScale = 0;             // scale for the position - number of pixels for an image sensor, or 
                                      // the generic scale (4096) for other sensor types
        int orientation = 0;          // detected sensor orientation (1 = normal, -1 = reversed, 0 = unknown)
        double avgScanTime = -1.0;    // average sensor scan time
        double processingTime = -1.0; // frame processing time
        bool calMode = false;         // sensor calibration in progress on device

        // Pixel capture file.  The foreground UI thread opens the file when the user 
        // clicks the Save button, and the background thread writes pixels to the file
        // as it reads them from the USB connection.  To close the file, the UI thread
        // sets the closePixFile flag; the background thread closes the file when it's
        // done with the current batch of pixels and nulls the handle.
        StreamWriter pixFile = null;
        bool closePixFile = false;

        public bool IsCalMode() { return calMode; }

        byte pixFlags = 0;      // pixel dump flags: 0x01 -> low res scan

        private Mutex mutex = new Mutex();
        private Thread thread;

        private void btnClose_Click(object sender, EventArgs e)
        {
            ActiveForm.Close();
        }

        private void adjustPixFlags(object rb, byte bit)
        {
            if (((RadioButton)rb).Checked)
                pixFlags |= bit;
            else
                pixFlags &= (byte)~bit;
        }

        private void rbLowRes_CheckedChanged(object sender, EventArgs e)
        {
            adjustPixFlags(sender, 0x01);    // low res mode - bit 0x01
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            (new Help("HelpPlungerViewer.htm")).Show();
        }

        private void PlungerSetup_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 27)
                ActiveForm.Close();
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
                    // open the file
                    pixFile = new StreamWriter(new FileStream(dlg.FileName, FileMode.Create), Encoding.ASCII);

                    // write the file header
                    pixFile.WriteLine("# Captured sensor pixels");
                    pixFile.WriteLine("# " + DateTime.Now);
                    pixFile.WriteLine("#");
                    pixFile.WriteLine("# Time(ms)   Position   Pixels...");
                    pixFile.WriteLine();
                    
                    // hide Save and show Stop
                    btnSave.Visible = false;
                    btnStopSave.Visible = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred trying to open the file: " + ex.Message);
                }
            }
        }

        private void btnStopSave_Click(object sender, EventArgs e)
        {
            // set the closing flag for the background thread to pick up
            closePixFile = true;
        }

        private void timerRefresh_Tick(object sender, EventArgs e)
        {
            // invalidate the visual display so we redraw it with the new data
            exposure.Invalidate();
        }

   }
}
