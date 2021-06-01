using System;
using System.Drawing;
using System.Windows.Forms;

namespace PinscapeConfigTool
{
    public partial class OutputTester : Form
    {
        public OutputTester(DeviceInfo dev)
        {
            this.dev = dev;
            InitializeComponent();
        }

        private void OutputTester_Load(object sender, EventArgs e)
        {
            // set up the pin display object
            pinDisplay = new KL25ZPinDisplay(picKl25z);

            // note layout items for resizing purposes
            int w = ClientRectangle.Width, h = ClientRectangle.Height;
            icon1MarginX = w - icon1.Right;
            bar2MarginY = h - bar2.Top;
            btnMarginX = w - btnClose.Right;
            btnSpacingX = btnClose.Left - btnHelp.Right;
            bottomPanelMarginY = h - bottomPanel.Top;

            // current row controls
            Panel outPanel = outPanel1;
            Label outNum = outNum1;
            Label outPin = outPin1;
            Label outLevel = outLevel1;
            TrackBar outBar = outBar1;
            CheckBox outOnOff = outOnOff1;

            // row copying functions
            Func<Panel, Panel> CopyPanel = (p) =>
            {
                Panel n = new Panel();
                outputListPanel.Controls.Add(n);
                n.Left = p.Left;
                n.Top = p.Top + p.Height;
                n.Width = p.Width;
                n.Height = p.Height;
                return n;
            };
            Func<Label, Label> CopyLabel = (l) =>
            {
                Label n = new Label();
                outPanel.Controls.Add(n);
                n.AutoSize = false;
                n.TextAlign = ContentAlignment.MiddleCenter;
                n.Left = l.Left;
                n.Width = l.Width;
                n.Height = l.Height;
                n.Top = l.Top;
                n.Font = l.Font;
                n.Padding = l.Padding;
                return n;
            };
            Func<CheckBox, CheckBox> CopyCheckbox = (c) =>
            {
                CheckBox n = new CheckBox();
                outPanel.Controls.Add(n);
                n.AutoSize = false;
                n.TextAlign = ContentAlignment.MiddleCenter;
                n.Left = c.Left;
                n.Width = c.Width;
                n.Height = c.Height;
                n.Top = c.Top;
                n.Text = c.Text;
                n.Appearance = c.Appearance;
                return n;
            };
            Func<TrackBar, TrackBar> CopyBar = (b) =>
            {
                TrackBar n = new TrackBar();
                outPanel.Controls.Add(n);
                n.Left = b.Left;
                n.Top = b.Top;
                n.Width = b.Width;
                n.Height = b.Height;
                n.TickFrequency = b.TickFrequency;
                n.TickStyle = b.TickStyle;
                n.Minimum = b.Minimum;
                n.Maximum = b.Maximum;
                return n;
            };

            // Mouse enter/leave handler generators
            Action<String, bool> SetPinHover = (pin, hilite) =>
            {
                pinDisplay.SetHover(pin, hilite);
            };
            Func<String, EventHandler> PanelMouseEnter = (pin) =>
            {
                EventHandler h1 = (object s1, EventArgs e1) =>
                {
                    SetPinHover(pin, true);
                    outputListPanel.Focus();
                };
                return new EventHandler(h1);
            };
            Func<String, EventHandler> PanelMouseLeave = (pin) =>
            {
                EventHandler h1 = (object s1, EventArgs e1) => { SetPinHover(pin, false); };
                return new EventHandler(h1);
            };

            // TrackBar and CheckBox handler generators
            Func<int, TrackBar, Label, EventHandler> TrackChange = (port, bar, levelLabel) =>
            {
                EventHandler h1 = (object s1, EventArgs e1) =>
                {
                    SetPort(port, bar.Value);
                    levelLabel.Text = bar.Value.ToString();
                };
                return new EventHandler(h1);
            };
            Func<int, TrackBar, Label, MouseEventHandler> TrackMouseDown = (port, bar, levelLabel) =>
            {
                MouseEventHandler h1 = (object s1, MouseEventArgs e1) =>
                {
                    const int marginLeft = 12, marginRight = 16;
                    double x = Math.Max(0, e1.X - marginLeft);
                    double wid = bar.Width - marginLeft - marginRight;
                    bar.Value = (int)Math.Round(Math.Min(1.0, x / wid) * (bar.Maximum - bar.Minimum) + bar.Minimum);
                    SetPort(port, bar.Value);
                    levelLabel.Text = bar.Value.ToString();
                };
                return new MouseEventHandler(h1);
            };
            Func<int, CheckBox, EventHandler> CheckBoxChange = (port, ck) =>
            {
                EventHandler h1 = (object s1, EventArgs e1) =>
                {
                    bool on = ck.Checked;
                    SetPort(port, on ? 255 : 0);
                    ck.BackColor = on ? Color.Yellow : ck.Parent.BackColor;
                    ck.Text = on ? "ON" : "Off";
                };
                return new EventHandler(h1);
            };

            // populate the output port list
            byte[] buf = dev.QueryConfigVar(255, 0);
            int nPorts = 0;
            if (buf != null)
            {
                byte nOutputs = buf[0];
                for (byte i = 1; i <= nOutputs; ++i)
                {
                    // retrieve this output description
                    buf = dev.QueryConfigVar(255, i);
                    byte typ = buf[0];  // 0=disabled, 1=GPIO PWM, 2=GPIO Digital, 3=TLC5940, 4=74HC595, 5=virtual, 6=TLC59116
                    byte pin = buf[1];  // GPIO pin ID (wire format), or TLC/HC595 pin number
                    bool pwm = false;   // presume it's not a PWM port
                    String klPin = null;  // presume it's not a KL25Z pin

                    // if the output is type 0 = disabled, it marks the end of the
                    // output list
                    if (typ == 0)
                        break;

                    // count the port
                    ++nPorts;

                    // figure the pin name
                    String pinName = null;
                    switch (typ)
                    {
                        case 1:
                            // KL25Z GPIO PWM
                            klPin = DeviceInfo.WireToPinName(pin);
                            pinName = klPin + "\r\n" + "(PWM)";
                            pwm = true;
                            break;

                        case 2:
                            // KL25Z GPIO Digital
                            klPin = DeviceInfo.WireToPinName(pin);
                            pinName = klPin + "\r\n" + "(Digital)";
                            break;

                        case 3:
                            // TLC5940.  These have 16 outputs per chip, and we number the ports
                            // sequentially across the daisy chain: pins 0-15 are chip 1, pins
                            // 16-31 are chip 2, etc.
                            pinName = "TLC5940 #" + ((pin / 16) + 1) + "\r\n" + "OUT" + (pin % 16);
                            pwm = true;
                            break;

                        case 4:
                            // 74HC595.  These have 8 outputs per chip, numbered sequentially
                            // across the daisy chain as with the TLC5940.
                            pinName = "74HC595 #" + ((pin / 8) + 1) + "\r\n" + "OUT" + (pin % 8);
                            break;

                        case 5:
                            // virtual output
                            pinName = "Virtual";
                            pwm = true;
                            break;

                        case 6:
                            // TLC59116.  These have 16 outputs per chip.  Each chip has a 4-bit
                            // address (the low 4 bits of its I2C address).  The pin number
                            // encodes the address in the high 4 bits and the output number in
                            // the low 4 bits.
                            pinName = "TLC59116 @" + ((pin >> 4) & 0x0F) + "\r\n" + "OUT" + (pin & 0x0F);
                            pwm = true;
                            break;
                    }

                    // if this isn't the first row, copy the prototype
                    if (i > 1)
                    {
                        outPanel = CopyPanel(outPanel);
                        outNum = CopyLabel(outNum);
                        outPin = CopyLabel(outPin);
                        outLevel = CopyLabel(outLevel);
                        outBar = CopyBar(outBar);
                        outOnOff = CopyCheckbox(outOnOff);
                    }

                    // set the labels
                    outNum.Text = i.ToString();
                    outPin.Text = pinName;
                    outLevel.Text = "0";

                    // show the trackbar or on/off button, according to the PWM status
                    outBar.Visible = outBar.Enabled = outLevel.Visible = pwm;
                    outOnOff.Visible = outOnOff.Enabled = !pwm;

                    // set the event handlers
                    if (pwm)
                    {
                        outBar.ValueChanged += TrackChange(i, outBar, outLevel);
                        outBar.MouseDown += TrackMouseDown(i, outBar, outLevel);
                    }
                    else
                        outOnOff.CheckedChanged += CheckBoxChange(i, outOnOff);

                    // if it's a KL25Z pin, attach the mouse methods to connect the pin
                    if (klPin != null)
                    {
                        // Add the MouseEnter and MouseLeave handlers to the panel and
                        // all of its children.  We have to repeat the handlers on the
                        // children because the event model doesn't nest: entering a
                        // child leaves the parent.  We want the whole area within the
                        // panel to act the same way, so we need to populate all of
                        // the child controls with the same handlers as the parent.
                        EventHandler enter = PanelMouseEnter(klPin);
                        EventHandler leave = PanelMouseLeave(klPin);
                        outPanel.MouseEnter += enter;
                        outPanel.MouseLeave += leave;
                        foreach (Control chi in outPanel.Controls)
                        {
                            chi.MouseEnter += enter;
                            chi.MouseLeave += leave;
                        }
                    }
                }
            }

            // create the output port level arrays
            newLevel = new int[nPorts];
            oldLevel = new int[nPorts];

            // start the update timer
            updateTimer.Enabled = true;
        }

        // Set a port's output level.  We simply note each change in the
        // newLevel array, so that the timer will pick up the change on
        // the next check.
        void SetPort(int port, int level)
        {
            // adjust to a 0-based port number
            --port;

            // if the port is valid and the level is changing, set it in the
            // new array
            if (port >= 0 && port < newLevel.Length)
                newLevel[port] = level;
        }

        // periodically check for changes to the output levels and
        // send updates
        private void updateTimer_Tick(object sender, EventArgs e)
        {
            // Set up the USB message buffer for the 'extended set brightness' 
            // command: 
            //   byte 0 = 0      -> report ID, always 0
            //   byte 1 = 200+n  -> command ID, 200 + port group (group 0 for ports
            //                      0-6, group 1 for ports 7-13, etc)
            //   byte 2..8       -> brightness level, one byte per port, level 0-255
            byte[] buf = new byte[9] { 0, 200, 0, 0, 0, 0, 0, 0, 0 };

            // run through the outputs looking for changes since the last update
            for (int basePort = 0; basePort < newLevel.Length; basePort += 7, buf[1] += 1)
            {
                // check for changes in this group
                bool changes = false;
                int lastPort = Math.Min(basePort + 7, newLevel.Length);
                for (int i = basePort; i < lastPort; ++i)
                {
                    if (oldLevel[i] != newLevel[i])
                    {
                        changes = true;
                        break;
                    }
                }

                // if we found changes, send the update
                if (changes)
                {
                    // fill in the command buffer and update oldLevel to the
                    // new levels
                    for (int i = basePort, msgi = 2; i < lastPort; ++i, ++msgi)
                    {
                        buf[msgi] = (byte)newLevel[i];
                        oldLevel[i] = newLevel[i];
                    }

                    // send the command
                    dev.WriteUSB(buf);
                }
            }

            // read a status report to get the current night mode setting
            byte[] rpt = dev.ReadStatusReport();
            if (rpt != null)
            {
                bool nightMode = (rpt[1] & 0x02) != 0;
                if (nightMode != ckNightMode.Checked)
                    ckNightMode.Checked = nightMode;
            }
        }

        // Pinscape unit interface
        DeviceInfo dev;

        // Current output port brightness levels (0-255 per port)
        int[] newLevel;
        int[] oldLevel;

        // kl25z pin display
        KL25ZPinDisplay pinDisplay;

        // layout parameters
        int bar2MarginY;
        int icon1MarginX;
        int btnMarginX, btnSpacingX;
        int bottomPanelMarginY;

        private void OutputTester_Resize(object sender, EventArgs e)
        {
            int w = ClientRectangle.Width, h = ClientRectangle.Height;
            bar1.Width = w + 1;
            bar2.Width = w + 1;
            icon1.Left = w - icon1MarginX - icon1.Width;
            btnClose.Left = w - btnMarginX - btnClose.Width;
            btnHelp.Left = btnClose.Left - btnSpacingX - btnHelp.Width;
            bottomPanel.Width = w;
            bottomPanel.Top = h - bottomPanelMarginY;
            outputListPanel.Height = bottomPanel.Top - outputListPanel.Top + 1;
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            (new Help("HelpOutputTester.htm")).Show();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void OutputTester_FormClosed(object sender, FormClosedEventArgs e)
        {
            // dispose of the KL25Z displayer
            picKl25z.Dispose();
            picKl25z = null;

            // send an ALL OUTPUTS OFF command to the KL25Z
            dev.SpecialRequest(5);
        }

        private void ckNightMode_CheckedChanged(object sender, EventArgs e)
        {
            if (ckNightMode.Checked)
            {
                ckNightMode.ForeColor = Color.White;
                ckNightMode.BackColor = Color.Navy;
                ckNightMode.FlatAppearance.MouseDownBackColor = Color.DarkBlue;
                ckNightMode.FlatAppearance.MouseOverBackColor = Color.Blue;
            }
            else
            {
                ckNightMode.ForeColor = Color.Purple;
                ckNightMode.BackColor = ckNightMode.Parent.BackColor;
                ckNightMode.FlatAppearance.MouseDownBackColor = Color.LightYellow;
                ckNightMode.FlatAppearance.MouseOverBackColor = Color.Yellow;
            }
        }

        private void ckNightMode_Click(object sender, EventArgs e)
        {
            // send a request to invert the night mode status in the device
            byte nightMode = (byte)(ckNightMode.Checked ? 1 : 0);
            dev.SpecialRequest(8, new byte[] { nightMode });
        }

    }
}
