using CollectionUtils;
using Plasmoid.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PinscapeConfigTool
{
    public partial class PlungerSetup : Form
    {
        public PlungerSetup(DeviceInfo dev)
        {
            InitializeComponent();
            this.dev = new DeviceInfo(dev);
        }

        private void PlungerSetup_Load(object sender, EventArgs e)
        {
            // get the plunger type
            byte[] buf = dev.QueryConfigVar(5);
            if (buf != null)
            {
                // remember the type
                plungerType = buf[0];

                // note the characteristics
                switch (plungerType)
                {
                    case PlungerTypeTSL1410R:
                    case PlungerTypeTSL1412R:
                    case PlungerTypeTCD1103:
                        // pixel edge detection sensors
                        pixelSensor = true;
                        edgeSensor = true;
                        break;

                    case PlungerTypeAEDR8300:
                        quadratureSensor = true;
                        //relativeSensor = true;
                        break;

                    case PlungerTypeTSL1410CL:
                        pixelSensor = true;
                        barCodeSensor = true;
                        break;

                    case PlungerTypeVL6180X:
                        //distanceSensor = true;
                        break;

                    case PlungerTypeAEAT6012:
                        break;
                }
            }

            // check to see if the firmware has jitter filtering capability
            if ((buf = dev.QueryConfigVar(0)) != null && buf[0] >= 19)
            {
                // set the jitter filter control bounds
                txtJitterWindow.Minimum = 0;
                txtJitterWindow.Maximum = 65535;

                // read the jitter filter setting
                if ((cfgVar19 = buf = dev.QueryConfigVar(19)) != null)
                {
                    jitterWindow = origJitterWindow = buf[0] | (buf[1] << 8);
                    txtJitterWindow.Value = jitterWindow;
                }
            }
            else
            {
                // no jitter filtering capability - hide that section
                int h = pnlJitter.Height;
                pnlBarCode.Top -= h;
                pnlBottom.Top -= h;
                Height -= h;
                pnlJitter.Visible = false;
            }

            // Check for reverse orientation capability.  To do this, check bit 0x80
            // of cfgVar19[2].  If this bit is set, the feature is supported.
            if (cfgVar19 != null && (cfgVar19[2] & 0x80) != 0)
            {
                // the feature is enabled - get the current value
                reverseOrientation = origReverseOrientation = (cfgVar19[2] & 0x01) != 0;
                ckReverseOrientation.Checked = reverseOrientation;
            }
            else
            {
                // the feature is diabled - hide that section
                int h = pnlReverse.Height;
                pnlJitter.Top -= h;
                pnlBarCode.Top -= h;
                pnlBottom.Top -= h;
                Height -= h;
                pnlReverse.Visible = false;
            }

            // show the bar code section only for bar code sensors
            if (barCodeSensor && (cfgVar20 = buf = dev.QueryConfigVar(20)) != null)
            {
                // read the current offset setting
                cfgBarCodeOffset = origBarCodeOffset = buf[0] | (buf[1] << 8);
                txtBarCodeOffset.Value = cfgBarCodeOffset;
            }
            else
            {
                // not a bar code sensor - hide the bar code section
                int h = pnlBarCode.Height;
                pnlBottom.Top -= h;
                Height -= h;
                pnlBarCode.Visible = false;
            }

            // select the default zoom
            cbZoom.SelectedIndex = 0;

            // start the exposure reader thread
            done = false;
            thread = new Thread(this.ExposureThread);
            thread.Start();
        }

        private void PlungerSetup_Shown(object sender, EventArgs e)
        {
        }

        // plunger type - plunger type code as defined in the USB protocol
        byte plungerType = PlungerTypeNone;

        // plunger types currently supported
        const byte PlungerTypeNone = 0;             // no plunger installed
        const byte PlungerTypeTSL1410R = 1;         // TSL1410R, edge detection
        const byte PlungerTypeTSL1412R = 3;         // TSL1412R, edge detection
        const byte PlungerTypePot = 5;              // potentiometer or other analog voltage sensor
        const byte PlungerTypeAEDR8300 = 6;         // AEDR8300 75lpi optical quadrature
        const byte PlungerTypeTSL1410CL = 8;        // TSL1410CL, bar code positioning
        const byte PlungerTypeVL6180X = 9;          // VL6180X time-of-flight distance sensor
        const byte PlungerTypeAEAT6012 = 10;        // AEAT-6012-A06 rotary absolute encoder
        const byte PlungerTypeTCD1103 = 11;         // TCD1103GFG linear image sensor, edge detection

        // Plunger characteristics
        bool edgeSensor = false;                    // edge-detection sensor
        bool pixelSensor = false;                   // image sensor
        bool barCodeSensor = false;                 // sensor reads a bar code
        bool quadratureSensor = false;              // quadrature sensor type
        //bool relativeSensor = false;              // sensor uses relative positioning (e.g., quadrature) (not currently used)
        //bool distanceSensor = false;              // non-contact distance measurement sensor

        // QueryConfigVar results for variable 19, plunger filters
        byte[] cfgVar19 = null;

        // current and original jitter filter values
        int jitterWindow = 0;
        int origJitterWindow = 0;

        // current and original orientation reversal filter values
        bool reverseOrientation = false;
        bool origReverseOrientation = false;

        // current and original bar code offset config values
        int cfgBarCodeOffset = 0;
        int origBarCodeOffset = 0;
        byte[] cfgVar20 = null;

        // extra data for barcode sensors
        struct BarCodeInfo
        {
            public int nBits;           // number of bits in code
            public int codeType;        // code type: 1=Gray code with Manchester pixel coding
            public int startOfs;        // starting pixel offset
            public int pixPerBit;       // pixel width of each bit
            public int raw;             // raw bar code bits
            public int mask;            // mask of successfully decoded bits
        }

        // extra data for quadrature sensors
        struct QuadratureInfo
        {
            public int chA;             // channel "A" reading
            public int chB;             // channel "B" reading
        }

        // Zoom factor, for optical sensors
        int zoom = 1;

        // Horizontal scroll offset; this is used only when zoomed in on an optical sensor
        int scrollOfs = 0;

        private void exposure_Paint(object sender, PaintEventArgs e)
        {
            // figure the number of on-screen pixels per sensor pixel
            int wid = exposure.Width;
            int ht = exposure.Height;

            // get the gdi handle for drawing onto the control
            Graphics g = e.Graphics;

            // fill the background with gray
            using (Brush dkgray = new SolidBrush(Color.DarkGray))
                g.FillRectangle(dkgray, 0, 0, wid, ht);

            // Get a private copy of the current sensor data.  Serialize
            // access via the mutex to ensure we get a consistent view,
            // since the device reader thread can update the sensor 
            // readings at any time.
            mutex.WaitOne();
            byte[] mypix = null;
            if (pix != null)
            {
                mypix = new byte[pix.Length];
                pix.CopyTo(mypix, 0);
            }
            int pos = reportedPos;
            int posScale = this.posScale;
            int dir = orientation;
            int npix = this.npix;
            int calMax = this.calMax;
            int calZero = this.calZero;
            bool calMode = this.calMode;
            int jfHi = this.jfHi;
            int jfLo = this.jfLo;
            int axcTime = this.axcTime;
            int rawPos = this.rawPos;
            BarCodeInfo barcode = this.barcode;
            QuadratureInfo quadrature = this.quadrature;
            String t1 = txtInfo_text;
            String t2 = txtInfo2_text;
            String t3 = txtInfo3_text;
            mutex.ReleaseMutex();

            // update the jitter filter units label
            String units =
                pixelSensor & edgeSensor ? "Pixels" :
                barCodeSensor ? "Bar code stops" :
                "Native device units";
            lblJitterUnits.Text = "(" + units + ", 0 à " + posScale + "; utiliser 0 pour désactiver)";

            // Figure the pixel-to-luminance mapping.  By default, this is
            // just a linear mapping from raw pixel brightness levels to 
            // grayscale levels.  If enhanced contrast is selected, we
            // scale the raw pixels to exaggerate the contrast by pulling
            // pixels near the low end of the raw pixel histogram to zero 
            // brightness in the on-screen rendering, pulling the high 
            // end of the histogram to maximum brightness on-screen, and
            // scaling everything in between for the stretched scale.
            byte[] lum = new byte[256];
            if (enhanceContrast)
            {
                // make a histogram of the brightness levels
                int[] hist = new int[256];
                mypix.ForEach(b => hist[b]++);

                // Find the min and max brightness levels, discarding the
                // two at each extreme as outliers
                int bmin = 0, bmax = 255;
                for (int i = 0, n = 0; i < 256; ++i)
                {
                    if (hist[i] != 0 && ++n > 2)
                    {
                        bmin = i;
                        break;
                    }
                }
                for (int i = 255, n = 0; i >= 0; --i)
                {
                    if (hist[i] != 0 && ++n > 2)
                    {
                        bmax = i;
                        break;
                    }
                }

                // now populate the luminance map with brightnesses scaled
                // between the two extremes
                for (int i = 0; i < bmin; ++i)
                    lum[i] = 0;
                double gray = 0.0;
                double inc = bmin >= bmax ? 0 : 256.0 / (bmax - bmin);
                for (int i = bmin; i < bmax; ++i, gray += inc)
                    lum[i] = (byte)Math.Round(gray);
                for (int i = bmax; i < 256; ++i)
                    lum[i] = 255;
            }
            else
            {
                // no contrast enhancement - draw pixels at gray levels directly
                // from the sensor
                for (int i = 0; i < 256; ++i)
                    lum[i] = (byte)i;
            }

            // Draw the pixels.  For a large sensor, draw the pixels first
            // onto a full-sized bitmap, then rescale it onto the window.
            // For a small sensor, draw the pixels directly into the window.
            if (npix * zoom > wid)
            {
                // Large sensor.
                // The sensor is larger than the on-screen display area.
                // Create a memory bitmap exactly as wide as the sensor,
                // draw the pixels one-to-one, then rescale the result
                // into the window.
                int bmwid = npix != 0 ? npix : 4096;
                using (Bitmap bitmap = new Bitmap(bmwid, ht))
                {
                    using (Graphics gbitmap = Graphics.FromImage(bitmap))
                    {
                        // if we have pixels to display, display them
                        if (mypix != null && npix != 0 && !calMode)
                        {
                            // draw the pixels onto the bitmap Graphics object
                            for (int i = scrollOfs, x = 0; i < npix && x < bmwid; ++i, x += zoom)
                            {
                                byte gray = lum[mypix[i]];
                                using (SolidBrush br = new SolidBrush(Color.FromArgb(gray, gray, gray)))
                                    gbitmap.FillRectangle(br, x, 0, x + zoom - 1, ht);
                            }
                        }

                        // stretch the bitmap onto the control area
                        g.DrawImage(bitmap, 0, 0, wid, ht);
                    }
                }
            }
            else
            {
                // Small sensor.
                // The sensor is no larger than the on-screen display area.
                // Do the pixel scaling directly to minimize blur.

                // figure the scaling factor (display pixels per sensor pixel)
                float s = (float)wid / (npix * zoom);
                float x = 0;
                for (int i = 0; i < npix; ++i, x += s)
                {
                    byte gray = lum[mypix[i]];
                    using (SolidBrush br = new SolidBrush(Color.FromArgb(gray, gray, gray)))
                        g.FillRectangle(br, x, 0, s, ht);
                }
            }

            // draw the quadrature sensor visualization if appropriate
            if (quadratureSensor)
            {
                // figure the bar layout
                const int nbars = 16;
                float barwid = (float)wid / nbars;

                // The center bar will represent the current sensor reading,
                // with "B" on the leading edge side.  So for the standard
                // orientation, "B" is on the right, and for reverse orientation,
                // "A" is on the left.  The "A" and "B" channels each represent
                // half a bar.  If both channels are the same, the A/B region
                // covers one whole bar; otherwise there's a bar edge between
                // A and B.
                float halfway = wid / 2.0f, startofs = halfway;
                if (quadrature.chA == quadrature.chB)
                    startofs -= barwid / 2.0f;

                // Figure the value of the bar to the right
                bool right = (reverseOrientation ? quadrature.chB : quadrature.chA) != 0;
                bool left = (reverseOrientation ? quadrature.chA : quadrature.chB) != 0;

                // set up brushes
                using (Brush off1 = new SolidBrush(Color.DimGray),
                    on1 = new SolidBrush(Color.WhiteSmoke),
                    on2 = new SolidBrush(Color.White),
                    off2 = new SolidBrush(Color.Black))
                {
                    // draw the bars to the right
                    bool color = right;
                    for (float x = startofs; x < wid + barwid; x += barwid, color = !color)
                        g.FillRectangle(color ? on1 : off1, x, 0.0f, Math.Min(wid, wid - x), ht);

                    // draw bars to the left
                    color = quadrature.chA == quadrature.chB ? !left : left;
                    for (float x = startofs - barwid; x > -barwid; x -= barwid, color = !color)
                        g.FillRectangle(color ? on1 : off1, x, 0.0f, barwid, ht);

                    // now fill in the half bars for "A" and "B"
                    g.FillRectangle(right ? on2 : off2, halfway, 0.0f, barwid / 2.0f, ht);
                    g.FillRectangle(left ? on2 : off2, halfway - barwid / 2.0f, 0.0f, barwid / 2.0f, ht);

                    // draw the outlines
                    Color colorA = Color.Aqua, colorB = Color.Lime;
                    float ofsA = reverseOrientation ? -barwid / 2.0f : barwid / 2.0f;
                    using (Pen pen = new Pen(colorA, 3.0f))
                        g.DrawLine(pen, halfway + ofsA, 0.0f, halfway + ofsA, ht);
                    using (Pen pen = new Pen(colorB, 3.0f))
                        g.DrawLine(pen, halfway - ofsA, 0.0f, halfway - ofsA, ht);

                    // draw the labels
                    using (Font font = new Font("Arial", 12, FontStyle.Bold))
                    {
                        StringFormat fmt = StringFormat.GenericTypographic;
                        fmt.Alignment = StringAlignment.Center;
                        fmt.LineAlignment = StringAlignment.Far;
                        using (Brush br = new SolidBrush(colorA))
                            g.DrawString("A", font, br, halfway + ofsA / 2.0f, ht / 2.0f, fmt);
                        using (Brush br = new SolidBrush(colorB))
                            g.DrawString("B", font, br, halfway - ofsA / 2.0f, ht / 2.0f, fmt);
                    }
                }
            }

            // draw the bar code visualization if appropriate
            if (barCodeSensor && mypix != null)
            {
                // get the scaling factor to map sensor pixel positions to 'g' coordinates
                float gx = (float)wid / npix;

                using (Brush red = new SolidBrush(Color.Red),
                    dkred = new SolidBrush(Color.DarkRed),
                    pink = new SolidBrush(Color.Pink))
                {
                    // draw each bar
                    StringFormat sf = new StringFormat();
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Far;
                    int barht = 10;
                    int bitwid = barcode.pixPerBit;
                    for (int i = barcode.nBits - 1, x = barcode.startOfs; i >= 0; --i, x += bitwid)
                    {
                        // get this item
                        int x0 = x, x1 = x0 + bitwid / 2, x2 = x0 + bitwid;
                        if (x0 >= 0)
                        {
                            // Get the bit value and validity
                            int bitval = (barcode.raw >> i) & 1;
                            bool bitok = ((barcode.mask >> i) & 1) == 1;

                            // draw the edge of the bar
                            g.FillRectangle(red, x0 * gx, 0, 1, ht);

                            // draw the half-bits if valid
                            if (bitok)
                            {
                                g.FillRectangle(bitval == 0 ? dkred : pink, x0 * gx, ht / 2, bitwid / 2 * gx, barht);
                                g.FillRectangle(bitval == 1 ? dkred : pink, x1 * gx, ht / 2, bitwid / 2 * gx, barht);
                            }

                            // show the interpretation
                            using (Font barBitFont = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Bold))
                                g.DrawString(bitok ? bitval.ToString() : "?", barBitFont, red, x1 * gx, ht / 2 + barht, sf);
                        }
                    }

                    // mark the right end of the code
                    int barEnd = barcode.startOfs + (bitwid * barcode.nBits);
                    g.FillRectangle(red, barEnd * gx, 0, 1, ht);
                }
            }

            // superimpose a green bar showing the plunger position
            if (pos != 0xFFFF)
            {
                using (Brush green = new SolidBrush(Color.Green))
                {
                    // rescale from the position scale to the bitmap width
                    float gpos = (pos - scrollOfs) * zoom * (float)wid / posScale;

                    // draw on the left side for reversed orientation (-1), 
                    // otherwise on the right side
                    int y = ht * 3 / 4, rht = ht - y;
                    if (dir == -1)
                        g.FillRectangle(green, 0, y, gpos, rht);
                    else
                        g.FillRectangle(green, gpos, y, wid - gpos, rht);
                }
            }

            // draw arrows at the calibration points, if known
            using (Brush magenta = new SolidBrush(Color.Magenta), red = new SolidBrush(Color.Red))
            {
                using (Bitmap upPurple = PinscapeConfigTool.Resources.ArrowUpPurple,
                    upRed = PinscapeConfigTool.Resources.ArrowUpRed,
                    downPurple = PinscapeConfigTool.Resources.ArrowDownPurple,
                    downRed = PinscapeConfigTool.Resources.ArrowDownRed)
                {
                    DrawCalArrow(g, calZero, "(Park)", posScale, dir, magenta, upPurple, downPurple);
                    DrawCalArrow(g, calMax, "(Max)", posScale, dir, red, upRed, downRed);
                }
            }

            // superimpose the jitter window, if known
            if (jfLo != jfHi)
            {
                using (Brush ltgreen = new SolidBrush(Color.LightGreen),
                    green = new SolidBrush(Color.Green),
                    red = new SolidBrush(Color.Red),
                    ltgray = new SolidBrush(Color.LightGray))
                {
                    // rescale from the position scale to the bitmap 
                    int lo = (int)Math.Round((double)(jfLo - scrollOfs) * zoom / posScale * wid);
                    int hi = (int)Math.Round((double)(jfHi - scrollOfs) * zoom / posScale * wid);
                    int rp = (int)Math.Round((double)(rawPos - scrollOfs) * zoom / posScale * wid);

                    // mirror the coordinates if using reversed orientation
                    if (dir == -1)
                    {
                        lo = wid - lo;
                        hi = wid - hi;
                    }
                    if (lo > hi)
                    {
                        int tmp = lo;
                        lo = hi;
                        hi = tmp;
                    }

                    // figure the jitter window area over the end of the bar
                    int y = ht * 3 / 4 - 1, rht = (ht - y);

                    // draw a line at the raw position if known
                    if (rawPos >= jfLo && rawPos <= jfHi)
                        g.FillRectangle(red, rp - 1, y + 1, 2, rht - 2);

                    // outline the jitter window
                    using (Pen plg = new Pen(Color.LightGreen, 2))
                        g.DrawRectangle(plg, lo, y, hi - lo - 2, rht - 2);

                    // label it
                    String msg1 = "Fenêtre de gigue ";
                    String msg2 = String.Format("(Raw={0})", rawPos);
                    using (Font font = SystemFonts.DefaultFont)
                    {
                        SizeF sz1 = g.MeasureString(msg1, font);
                        SizeF sz2 = g.MeasureString(msg2, font);
                        float w = sz1.Width + sz2.Width;
                        float left = (lo + hi - w) / 2;
                        float top = y - sz1.Height - 2;
                        if (left + w > wid - 5)
                            left = wid - 5 - w;
                        if (left < 5)
                            left = 5;

                        GraphicsExtension.FillRoundedRectangle(
                            g, ltgray, left, top, w, sz1.Height, 2);
                        g.DrawString(msg1, font, green, left, top);
                        g.DrawString(msg2, font, red, left + sz1.Width, top);
                    }
                }
            }

            // show the plunger position on top of the green bar
            if (pos != 0xFFFF)
            {
                String msg = pos.ToString();
                using (Font font = SystemFonts.DefaultFont)
                {
                    Size sz = g.MeasureString(msg, font).ToSize();
                    using (Brush white = new SolidBrush(Color.White),
                        green = new SolidBrush(Color.Green),
                        ltgray = new SolidBrush(Color.LightGray))
                    {
                        int xTxt = (int)Math.Round((float)(pos - scrollOfs) * zoom / posScale * wid);
                        int yTxt = ht * 3 / 4 + (ht / 4 - sz.Height) / 2;
                        if (dir == 1)
                        {
                            if (xTxt + 2 + sz.Width < wid)
                                g.DrawString(msg, font, white, xTxt + 2, yTxt);
                            else
                            {
                                GraphicsExtension.FillRoundedRectangle(
                                    g, ltgray, xTxt - sz.Width - 2, yTxt, sz.Width, sz.Height, 2);
                                g.DrawString(msg, font, green, xTxt - sz.Width - 2, yTxt);
                            }
                        }
                        else
                        {
                            if (xTxt - sz.Width - 2 > 0)
                                g.DrawString(msg, font, white, xTxt - sz.Width - 2, yTxt);
                            else
                            {
                                GraphicsExtension.FillRoundedRectangle(
                                    g, ltgray, xTxt + 2, yTxt, sz.Width, sz.Height, 2);
                                g.DrawString(msg, font, green, xTxt + 2, yTxt);
                            }
                        }
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
                String msg = "CALIBRATION";
                using (Font font = SystemFonts.CaptionFont)
                {
                    Size sz = g.MeasureString(msg, font).ToSize();
                    using (Brush br = new SolidBrush(calModeColor ? Color.Purple : Color.Blue))
                        g.DrawString(msg, font, br, (wid - sz.Width) / 2, (ht - sz.Height) / 2);
                }
            }

            // update the statistics text
            txtInfo.Text = t1;
            txtInfo2.Text = t2;
            txtInfo3.Text = t3;
            GC.Collect(); return;// $$$

        }
        DateTime flashTime = DateTime.Now;
        bool calModeColor;

        private void DrawCalArrow(
            Graphics g, int pos, String caption, int posScale, int dir,
            Brush br, Image arrowUp, Image arrowDown)
        {
            if (pos != -1)
            {
                // adjust for zoom and scroll
                pos -= scrollOfs;
                pos *= zoom;

                // get the drawing area size
                int wid = exposure.Width;
                int ht = exposure.Height;

                // figure the drawing position - mirror it if the orientation is reversed
                int drawpos = (dir == -1 ? posScale - pos : pos);

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

                using (Font font = SystemFonts.DefaultFont)
                {
                    Size sz = g.MeasureString(msg, font).ToSize();
                    int x = xArrow + (drawpos < posScale / 2 ? arrowDown.Width + 2 : -sz.Width - 2);
                    int y = (arrowDown.Height - sz.Height) / 2;
                    using (Brush ltgray = new SolidBrush(Color.LightGray))
                        GraphicsExtension.FillRoundedRectangle(g, ltgray, x, y, sz.Width, sz.Height, 2);
                    g.DrawString(msg, font, br, x, y);
                }
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

        private void PlungerSetup_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (dev != null)
            {
                dev.Dispose();
                dev = null;
            }
        }

        private void PlungerSetup_FormClosing(object sender, FormClosingEventArgs e)
        {
            // wait for the image reader thread to end
            done = true;
            thread.Join();

            // check for updated config settings
            List<String> msg = new List<String>();
            if (jitterWindow != origJitterWindow)
                msg.Add("filtre de gigue");
            if (reverseOrientation != origReverseOrientation)
                msg.Add("orientation inversée");
            if (cfgBarCodeOffset != origBarCodeOffset)
                msg.Add("décalage de code à barres");

            // ask if they'd like to save jitter filter changes
            if (msg.Count != 0)
            {
                if (MessageBox.Show("Vous avez changé le " + msg.SerialJoin() + " réglages."
                    + " Souhaitez-vous enregistrer le nouveau paramètre" + (msg.Count > 1 ? "s" : "")
                    + " sur l'appareil?",
                    "Outil de configuration Pinscape", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    // Yes, save - send the updated settings to the device
                    if (jitterWindow != origJitterWindow || reverseOrientation != origReverseOrientation)
                        SendFilters(jitterWindow, reverseOrientation);
                    if (cfgBarCodeOffset != origBarCodeOffset)
                        SendBarCodeOffset(cfgBarCodeOffset);

                    // Save the update to flash.  Note that there's no need to reboot 
                    // the device, as the plunger variables we update take effect immediately 
                    // without a reboot.
                    dev.SaveConfig(false);
                }
                else
                {
                    // Restore the old settings.  There's no need to write anything to
                    // flash, since we've only changed it in the device RAM so far.
                    if (jitterWindow != origJitterWindow || reverseOrientation != origReverseOrientation)
                        SendFilters(origJitterWindow, origReverseOrientation);
                    if (cfgBarCodeOffset != origBarCodeOffset)
                        SendBarCodeOffset(origBarCodeOffset);
                }
            }
        }

        // update the jitter filter and reverse orientation settings on the device
        void SendFilters(int jitterWindow, bool reverse)
        {
            // update the config var 19 buffer - use the old buffer so that we
            // retain the original values of any other data in the buffer that
            // we don't currently parse (in case of newer firmware)
            cfgVar19[0] = (byte)(jitterWindow & 0xff);
            cfgVar19[1] = (byte)((jitterWindow >> 8) & 0xff);
            cfgVar19[2] = (byte)((cfgVar19[2] & ~0x01) | (reverse ? 0x01 : 0x00));

            // update it on the device
            dev.SetConfigVar(19, cfgVar19);
        }

        // update the bar code offset setting on the device
        void SendBarCodeOffset(int val)
        {
            // update the config var 20 buffer - use the old buffer so that we
            // retain the original values of any other data in the buffer that
            // we don't currently parse (in case of newer firmware)
            cfgVar20[0] = (byte)(val & 0xff);
            cfgVar20[1] = (byte)((val >> 8) & 0xff);

            // update it on the device
            dev.SetConfigVar(20, cfgVar20);
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
                    if (!Program.IsInForeground())
                    {
                        Thread.Sleep(250);
                        continue;
                    }

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
                        //   bytes 9:10:11  -> average sensor scan time in 10us units
                        //   bytes 12:13:14 -> image processing time for this scan in 10us units

                        // get the flags
                        int flags = buf[8];

                        // read the pixel count
                        int newnpix = buf[4] + (buf[5] << 8);

                        // the position is reported in terms of the pixel count if this is an
                        // imaging sensor; if not, it's reported on the generic joystick axis
                        // scale (0..4095)
                        int posScale = (newnpix != 0 ? newnpix : 4096);

                        // get the reported position
                        int reportedPos = buf[6] + (buf[7] << 8);

                        // read the orientation
                        orientation =
                            (flags & 0x01) != 0 ? 1 :
                            (flags & 0x02) != 0 ? -1 :
                            0;

                        // check for calibration mode
                        bool calMode = (flags & 0x04) != 0;

                        // read the average scan time, converting from the 10us units
                        // in the report to milliseconds (10us = 0.01ms -> 1ms = 100 units)
                        long t = buf[9] + ((long)buf[10] << 8) + ((long)buf[11] << 16);
                        avgScanTime = t / 100.0;

                        // read the image processing and convert to milliseconds
                        t = buf[12] + ((long)buf[13] << 8) + ((long)buf[14] << 16);
                        processingTime = t / 100.0;

                        // jitter filter bounds and raw position will come in the 
                        // second report, if present
                        int jfLo = -1, jfHi = -1;
                        int rawPos = -1;
                        int axcTime = 0;

                        // Check for a second header report.  This report is optional
                        // and is only sent by newer firmware versions, so it might
                        // not be there at all.
                        if ((buf = tdev.ReadUSB()) != null
                            && buf[1] == 0xff && buf[2] == 0x87 && buf[3] == 1)
                        {
                            // It's the second header report

                            // Get the native position scale.  This tells us the
                            // actual position scale the device uses, which overrides
                            // our earlier assumption that it's equal to the pixel array 
                            // size for an image sensor or the joystick scale for non-
                            // image sensors.
                            posScale = buf[4] | (buf[5] << 8);

                            // get the jitter filter window bounds
                            jfLo = buf[6] | (buf[7] << 8);
                            jfHi = buf[8] | (buf[9] << 8);

                            // get the raw position
                            rawPos = buf[10] | (buf[11] << 8);

                            // get the auto-exposure time
                            axcTime = buf[12] | (buf[13] << 8);

                            // consume the report
                            buf = null;
                        }

                        // Check for extra sensor-specific reports.  These reports follow
                        // the two standard sensor reports, and are identified by the first
                        // two bytes 0xFF87.  The next byte specifies the subtype, which
                        // depends on the particular sensor type in use.
                        if ((buf = tdev.ReadUSB()) != null && buf[1] == 0xff && buf[2] == 0x87)
                        {
                            // check the subtype
                            if (buf[3] == 2)
                            {
                                // Barcode sensor report
                                barcode.nBits = buf[4];
                                barcode.codeType = buf[5];
                                barcode.startOfs = buf[6] | (buf[7] << 8);
                                barcode.pixPerBit = buf[8];
                                barcode.raw = buf[9] | (buf[10] << 8);
                                barcode.mask = buf[11] | (buf[12] << 8);

                                // consume the report
                                buf = null;
                            }
                            else if (buf[3] == 3)
                            {
                                // Quadrature sensor report
                                quadrature.chA = buf[4];
                                quadrature.chB = buf[5];
                            }
                        }

                        // read the pixel reports if it's an imaging sensor and
                        // we're not in calibration mode
                        byte[] newpix = new byte[newnpix];
                        if (!calMode && newnpix != 0)
                        {
                            // Figure the expected number of reports, assuming 10 pixels
                            // per report, but add some extra padding in case of interleaved 
                            // reports of other types.  The loop bounds are just to ensure
                            // that we don't get stuck if we don't get the expected reports;
                            // if everything goes smoothly, we'll be able to tell when we're
                            // actually done, because each report tells us where it is in
                            // the pixel array.
                            for (int j = newnpix / 10 + 100; j > 0 && !done; --j)
                            {
                                // read a report if we don't have one already queued up
                                if (buf == null)
                                    buf = tdev.ReadUSB();

                                // Only consider reports that are in the special exposure report 
                                // format.
                                if (buf != null && (buf[2] & 0xF8) == 0x80)
                                {
                                    // Figure the starting index of the current batch.  If the
                                    // index is 0x7FF == 2047, it's an extra header report.  We
                                    // already parsed the known types above, but future versions
                                    // of the firmware might add new ones that we don't know
                                    // about in this config tool version, so simply ignore any
                                    // we didn't already parse.
                                    int idx = (((int)buf[2] & 0x7) << 8) | (int)buf[1];
                                    if (idx == 0x7FF)
                                    {
                                        // It's an extra header we don't recognize.  Skip it,
                                        // and don't count it against the report limit, since
                                        // it's a valid plunger status report even if it's not
                                        // one we recognize specifically.
                                        ++j;
                                    }
                                    else
                                    {
                                        // Store this batch
                                        for (int k = 3; k < buf.Length && idx < newnpix; k += 1)
                                            newpix[idx++] = buf[k];

                                        // If this is the last batch of pixels, we're done.
                                        if (idx >= newnpix)
                                            j = 1;
                                    }
                                }

                                // we've consumed this report
                                buf = null;
                            }
                        }

                        // If a pixel file is open, and we have a pixel buffer, save the pixels
                        // to our capture list (don't write the file quite yet - we'll defer
                        // that until we're done, to avoid slowing down the capture)
                        if (pixFile != null && npix != 0)
                        {
                            // add it to the pixel list
                            capturedFrames.Add(new PixFrame(
                                (DateTime.Now - t0Thread).TotalMilliseconds, pix, reportedPos));

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

                        // copy the new pixel list and other data to the drawing copy
                        npix = newnpix;
                        pix = newpix;
                        this.calMax = calMax;
                        this.calZero = calZero;
                        this.tRelease = tRelease;
                        this.reportedPos = reportedPos;
                        this.posScale = posScale;
                        this.calMode = calMode;
                        this.jfHi = jfHi;
                        this.jfLo = jfLo;
                        this.axcTime = axcTime;
                        this.rawPos = rawPos;

                        // Update the sensor scan time information
                        String times = String.Format(
                            "Temps moyen de balayage du capteur: {0} ms, temps de traitement de la trame actuelle: {1} ms",
                            avgScanTime, processingTime);

                        // update statistics according to sensor type (imaging or non-imaging)
                        if (newnpix != 0)
                        {
                            // Imaging sensor type

                            // line 1 - pixel array information and position
                            if (edgeSensor)
                            {
                                txtInfo_text = String.Format(
                                    "Pixels: {0}, Orientation: {1}, Edge pos: {2}, Release time: {3} ms",
                                    newnpix,
                                    orientation == 1 ? "Standard" : orientation == -1 ? "Inversé" : "Inconnu",
                                    reportedPos == 0xFFFF ? "Pas détecté" : reportedPos.ToString(),
                                    tRelease);
                            }
                            else if (barCodeSensor)
                            {
                                txtInfo_text = String.Format(
                                    "Pixels: {0}, position du piston: {1}, temps de relâchement: {2} ms",
                                    newnpix,
                                    reportedPos == 0xFFFF ? "Pas détecté" : reportedPos.ToString(),
                                    tRelease);
                            }
                            else
                            {
                                txtInfo_text = String.Format(
                                    "Pixels: {0}, Orientation: {1}, Position du piston: {2}, Temps de relâchement: {3} ms",
                                    newnpix,
                                    orientation == 1 ? "Standard" : orientation == -1 ? "Inversé" : "Inconnu",
                                    reportedPos == 0xFFFF ? "Pas détecté" : reportedPos.ToString(),
                                    tRelease);
                            }

                            // line 2 - brightness range
                            txtInfo2_text = String.Format("Luminosité Min: {0}, Max: {1}, % Saturation: {2}, % Zero: {3}",
                                    pixMin, pixMax, (int)Math.Round(numSat * 100.0 / newnpix),
                                    (int)Math.Round(numZero * 100.0 / npix));

                            if (axcTime > 0)
                                txtInfo2_text += ", Temps d'exposition automatique " + axcTime + "us";

                            // line 3 - scan time
                            txtInfo3_text = times;
                        }
                        else if (quadratureSensor)
                        {
                            // Quadrature sensor type

                            // line 1 - position
                            txtInfo_text = String.Format(
                                "Position du piston: {0}, orientation: {1}, temps de relâchement: {2} ms",
                                reportedPos == 0xFFFF ? "Inconnu" : reportedPos.ToString() + "/" + (posScale - 1),
                                orientation == 1 ? "Standard" : orientation == -1 ? "Inversé" : "Inconnu",
                                tRelease);

                            // line 2 - scan time
                            txtInfo2_text = times;

                            // line 3 - Channel A/B values
                            txtInfo3_text = String.Format("Canal en quadrature A/B: {0}/{1}", quadrature.chA, quadrature.chB);
                        }
                        else
                        {
                            // Other non-imaging sensor type

                            // line 1 - position
                            txtInfo_text = String.Format(
                                "Position du piston: {0}, orientation: {1}, temps de relâchement: {2} ms",
                                reportedPos == 0xFFFF ? "Inconnu" : reportedPos.ToString() + "/" + (posScale - 1),
                                orientation == 1 ? "Standard" : orientation == -1 ? "Inversé" : "Inconnu",
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
                lblCal.Text = "Calibration en cours. Cela fonctionnera "
                    + "pendant environ 15 secondes. Pendant son exécution:\r\n\r\n"
                    + " • Tirez le piston vers l'arrière\r\n"
                    + " • Tenez un instant, puis relâchez\r\n"
                    + " • Attendez qu'il s'arrête\r\n"
                    + " • Répétez plusieurs fois comme vous le souhaitez\r\n";
                btnCal.Enabled = false;
            }
            else if (calStarted)
            {
                // calibration done
                lblCal.Text = "Calibration terminée.\r\n\r\n"
                    + "Si vous souhaitez répéter le processus, assurez-vous que le piston "
                    + "est au repos à sa position normale de stationnement, puis appuyez sur le bouton "
                    + "Bouton Calibrer.";
                btnCal.Enabled = true;
            }
            else
            {
                // we haven't done the first calibration with this dialog yet
                lblCal.Text = "Calibrez le capteur lorsque vous "
                    + "l'installez d'abord, et chaque fois que vous ajustez sa position. "
                    + "Vous devez également recalibrer si la flèche \"Park\" est affichée "
                    + "ci-dessus ne correspond pas à la position de repos réelle du piston."
                    + "\r\n\r\n"
                    + "Pour calibrer, assurez-vous que le piston est au repos "
                    + "position de stationnement normale, puis appuyez sur Calibrer et suivez "
                    + "les instructions à l'écran.";
                btnCal.Enabled = true;
            }
        }

        private void timerCal_Tick(object sender, EventArgs e)
        {
            // update the calibration mode
            UpdateCalMode();

            // Show or hide the pixel-related controls according to whether this
            // is an imaging or non-imaging sensor.  For imaging sensors, only
            // show the resolution controls for large sensors (>=512 pixels).
            // Note that the native scale is the relevant metric rather than
            // the number of pixels in the captured image, since we might be
            // showing the image at reduced resolution.
            bool showResCtls = (pixelSensor && posScale >= 512);
            rbHiRes.Visible = showResCtls;
            rbLowRes.Visible = showResCtls;
            ckEnhance.Visible = pixelSensor;
            btnSave.Visible = (!btnStopSave.Visible && pixelSensor);

            // show the zoom control only for non-bar-code pixel sensors
            cbZoom.Visible = lblZoom.Visible = pixelSensor && !barCodeSensor;

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
        int reportedPos = 0xffff;     // plunger position reported from device in current frame
        int rawPos = -1;              // raw position reported (might differ from reported position due to filtering)
        int posScale = 0;             // scale for the position - number of pixels for an image sensor, or 
                                      // the generic scale (4096) for other sensor types
        int orientation = 0;          // detected sensor orientation (1 = normal, -1 = reversed, 0 = unknown)
        double avgScanTime = -1.0;    // average sensor scan time
        double processingTime = -1.0; // frame processing time
        int axcTime = 0;              // auto-exposure time in microseconds
        bool calMode = false;         // sensor calibration in progress on device
        bool enhanceContrast = false; // show enhanced contrast in sensor view
        int jfLo = -1, jfHi = -1;     // bounds of current jitter filter window on the device, if known
        BarCodeInfo barcode;          // bar code descriptor
        QuadratureInfo quadrature;    // quadrature sensor status

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
            Close();
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
                Close();
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
                    pixFile.WriteLine("# Pixels du capteur capturés");
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
                    MessageBox.Show("Une erreur s'est produite lors de la tentative d'ouverture du fichier: " + ex.Message);
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

        private void ckEnhance_CheckedChanged(object sender, EventArgs e)
        {
            enhanceContrast = ckEnhance.Checked;
        }

        private void txtJitterWindow_ValueChanged(object sender, EventArgs e)
        {
            // if the window value has changed, send the update to the device
            int v = (int)txtJitterWindow.Value;
            if (v != jitterWindow)
                SendFilters(jitterWindow = v, reverseOrientation);
        }

        private void ckReverseOrientation_CheckedChanged(object sender, EventArgs e)
        {
            bool v = ckReverseOrientation.Checked;
            if (v != reverseOrientation)
                SendFilters(jitterWindow, reverseOrientation = v);
        }

        private void txtBarCodeOffset_ValueChanged(object sender, EventArgs e)
        {
            int v = (int)txtBarCodeOffset.Value;
            if (v != cfgBarCodeOffset)
            {
                cfgBarCodeOffset = v;
                SendBarCodeOffset(v);
            }
        }

        private void cbZoom_SelectedIndexChanged(object sender, EventArgs e)
        {
            int i;
            if (int.TryParse(cbZoom.SelectedItem.ToString().Replace("%", ""), out i))
            {
                zoom = i / 100;
                ConstrainScroll();

                exposure.Cursor = (zoom == 1 ? Cursors.Default : Cursors.SizeWE);
            }
        }

        void ConstrainScroll()
        {
            if (scrollOfs < 0)
                scrollOfs = 0;

            if (npix != 0)
            {
                int maxofs = npix - npix / zoom;
                if (scrollOfs > maxofs)
                    scrollOfs = maxofs;
            }
        }

        bool scrollTrack = false;
        int scrollTrackX = 0, scrollTrackOfs;
        private void exposure_MouseDown(object sender, MouseEventArgs e)
        {
            scrollTrack = true;
            scrollTrackX = e.X;
            scrollTrackOfs = scrollOfs;
            exposure.Capture = true;
        }

        private void exposure_MouseMove(object sender, MouseEventArgs e)
        {
            if (scrollTrack && npix != 0)
            {
                float screenPixPerSensorPix = (float)exposure.Width / npix * zoom;
                scrollOfs = (int)(scrollTrackOfs + (scrollTrackX - e.Location.X) / screenPixPerSensorPix);
                ConstrainScroll();
            }
        }

        private void exposure_MouseUp(object sender, MouseEventArgs e)
        {
            if (scrollTrack)
                exposure.Capture = false;
        }

        private void exposure_MouseCaptureChanged(object sender, EventArgs e)
        {
            scrollTrack = false;
        }

    }
}
