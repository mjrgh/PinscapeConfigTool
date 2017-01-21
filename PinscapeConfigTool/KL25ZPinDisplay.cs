using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Runtime.InteropServices;

namespace PinscapeConfigTool
{
    class KL25ZPinDisplay
    {
        // Instatiate the object during the containing form's Load handler
        public KL25ZPinDisplay(PictureBox pic)
        {
            // set up the on-screen picture
            this.pic = pic;
            pic.Image = new Bitmap("html\\kl25zPinsSmaller.png");

            // load the pin callout images
            calloutLeft = new Bitmap("html\\kl25zPinsSmaller-Callout-left.png");
            calloutRight = new Bitmap("html\\kl25zPinsSmaller-Callout-right.png");

            // initialize the pin mappings
            InitPins();

            // set up the mouse event handlers
            pic.MouseMove += MouseMove;
            pic.MouseLeave += MouseLeave;
        }

        // Handle a paint event for the control.  The form's xxx_Paint()
        // event handler should simply delegate the call to us.
        public void Paint(PaintEventArgs e)
        {
            // draw highlighted pins
            Graphics g = e.Graphics;
            Pen hilitePen = new Pen(Color.Red, 3);
            Pen onPen = new Pen(Color.Yellow, 3);
            const int r = 5;
            foreach (Pin pin in pinTab.Values)
            {
                if (pin.hover || pin == callout)
                    g.DrawEllipse(hilitePen, pin.x - r, pin.y - r, r * 2, r * 2);
                if (pin.isOn)
                    g.DrawEllipse(onPen, pin.x - r - 3, pin.y - r - 3, r * 2 + 6, r * 2 + 6);
            }

            // draw the hover callout, if any
            if (callout != null)
            {
                // figure whether it's left or right
                Rectangle src = new Rectangle(0, 0, calloutLeft.Width, calloutLeft.Height);  // both sides are the same size
                Rectangle dst;
                int txtx;
                if (callout.x + calloutRight.Width < pic.Width)
                {
                    // not near the right edge - use the left callout
                    dst = new Rectangle(callout.x, 2 + callout.y - calloutLeft.Height / 2, src.Width, src.Height); 
                    g.DrawImage(calloutLeft, dst, src, GraphicsUnit.Pixel);
                    txtx = dst.Left + (dst.Width - 21) / 2 + 21;
                }
                else
                {
                    // near the right edge - use the right callout
                    dst = new Rectangle(callout.x - calloutRight.Width, 2 + callout.y - calloutRight.Height / 2, src.Width, src.Height);
                    g.DrawImage(calloutRight, dst, src, GraphicsUnit.Pixel);
                    txtx = dst.Left + (dst.Width - 21) / 2;
                }

                // draw the caption
                Font font = SystemFonts.MenuFont;
                Brush brush = Brushes.Black;
                StringFormat centerText = new StringFormat();
                centerText.Alignment = StringAlignment.Center;
                centerText.LineAlignment = StringAlignment.Center;
                g.DrawString(callout.pinName, font, brush, txtx, dst.Y + dst.Height/2, centerText);
            }
        }

        // Set a pin's hover status
        public void SetHover(String pinName, bool hover)
        {
            if (pinName != "NC" && pinTab.ContainsKey(pinName))
            {
                Pin pin = pinTab[pinName];
                if (pin.hover != hover)
                {
                    pinTab[pinName].hover = hover;
                    pic.Invalidate();
                }

                if (hover && pin != callout)
                    SetCallout(pin);
                else if (!hover && pin == callout)
                    SetCallout(null);
            }
        }

        // Mark a pin as on or off for visual highlighting
        public void SetOnOff(String pinName, bool on)
        {
            if (pinName != "NC" && pinTab.ContainsKey(pinName))
            {
                Pin pin = pinTab[pinName];
                if (pin.isOn != on)
                {
                    pinTab[pinName].isOn = on;
                    pic.Invalidate();
                }
            }
        }

        // on-screen picture box control
        PictureBox pic;

        // callout images
        Image calloutLeft, calloutRight;

        // current callout pin
        Pin callout = null;

        // MouseMove handler - show a callout for the pin we're over
        void MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // check to see if we're over a pin
            const double maxDist = 5.0;
            Pin newCallout = null;
            foreach (Pin pin in pinTab.Values)
            {
                // figure the distance to the pin center; if we're close to the
                // pin, show the callout
                int dx = pin.x - e.X, dy = pin.y - e.Y;
                double r = Math.Sqrt(dx * dx + dy * dy);
                if (r < maxDist)
                {
                    // note the new callout pin and stop looking
                    newCallout = pin;
                    break;
                }
            }

            // set the new callout, if any
            SetCallout(newCallout);
        }

        void SetCallout(Pin pin)
        {
            if (pin != callout)
            {
                callout = pin;
                pic.Invalidate();
            }
        }

        // MouseLeave handler - remove any callout
        void MouseLeave(object sender, EventArgs e)
        {
            // cleear any callout
            SetCallout(null);
        }

        // Pin descriptor
        public class Pin
        {
            public Pin(String headerName, int pinNo, String pinName, int x, int y)
            {
                this.headerName = headerName;
                this.pinNo = pinNo;
                this.pinName = pinName;
                this.x = x;
                this.y = y;
                this.hover = false;
                this.isOn = false;
            }
            public String pinName;
            public int pinNo;
            public String headerName;
            public int x, y;
            public bool hover;      // mouse is hovering over the pin
            public bool isOn;       // pin is on
        }

        // table of pin descriptors, indexed by pin name
        public Dictionary<String, Pin> pinTab = new Dictionary<String, Pin>();

        // KL25Z diagram pin header layouts.  For each header, the pins are
        // arranged across the pin 1 row, then the pin 2 row.  So the pin
        // numbers on the header are 1-3-5..., 2-4-6...
        String[] kl25z_j1 = new String[]{
            "PTC7", "PTC0", "PTC3", "PTC4",  "PTC5", "PTC6", "PTC10", "PTC11",
	        "PTA1", "PTA2", "PTD4", "PTA12", "PTA4", "PTA5", "PTC8",  "PTC9"
        };
        String[] kl25z_j2 = new String[]{
            "PTC12", "PTC13", "PTC16", "PTC17", "PTA16", "PTA17", "PTE31", "NC",    "PTD6", "PTD7",
	        "PTA13", "PTD5",  "PTD0",  "PTD2",  "PTD3",  "PTD1",  "GND",   "VREFH", "PTE0", "PTE1"
        };
        String[] kl25z_j9 = new String[]{
            "PTB8",    "PTB9", "PTB10", "PTB11", "PTE2",  "PTE3", "PTE4", "PTE5",
	        "SDA_PTD", "P3V3", "RESET", "P3V3",  "USB5V", "GND",  "GND",  "VIN"
        };
        String[] kl25z_j10 = new String[]{
            "PTE20", "PTE21", "PTE22", "PTE23", "PTE29", "PTE30",
	        "PTB0",  "PTB1",  "PTB2",  "PTB3",  "PTC2",  "CLKIN"
        };

        // initialize the pins
        public void InitPins()
        {
            Action<String, int, int, int, int, String[]> InitHeader = (headerName, x1, y1, xN, yN, pins) =>
            {
                int x = x1, y = y1;
                int dy = (yN - y1) / (pins.Length / 2 - 1);
                int pinNo = 1;
                for (int i = 0; i < pins.Length; ++i)
                {
                    // create this pin
                    Pin pin = new Pin(headerName, pinNo, pins[i], x, y);
                    pinTab[pins[i]] = pin;

                    // advance to the next row or column
                    if (i + 1 == pins.Length / 2)
                    {
                        y = y1;
                        x = xN;
                        pinNo = 2;
                    }
                    else
                    {
                        y += dy;
                        pinNo += 2;
                    }
                }
            };

            InitHeader("J1", 19, 65, 10, 130, kl25z_j1);
            InitHeader("J2", 19, 145, 10, 228, kl25z_j2);
            InitHeader("J9", 177, 196, 186, 130, kl25z_j9);
            InitHeader("J10", 177, 111, 186, 65, kl25z_j10);
        }

    }
}
