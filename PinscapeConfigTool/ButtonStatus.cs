using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Input;

namespace PinscapeConfigTool
{
    public partial class ButtonStatus : Form
    {
        public ButtonStatus(DeviceInfo dev)
        {
            this.dev = dev;
            InitializeComponent();

            // set up the key caps translated from the javascript on-screen keyboard
            InitKeyCaps();

            // The numeric keypad and cursor keys are a bit of a mess.  The WM_KEYxxx
            // messages can distinguish three separate versions of each cursor key:
            //
            //    - "keypad 4/left arrow" is VK_LEFT (keyboard left arrow) when Num Lock is OFF
            //    - "keypad 4/left arrow" is VK_NUMPAD4 (numeric keypad 4) when Num Lock is ON
            //    - "keyboard left arrow" ks VK_LEFT+Extended (regardless of Num Lock)
            //
            // So far so good - our input reader can tell all of these apart.  Here's
            // where it gets messy, though.  VP *doesn't* distinguish the keypad keys
            // by Num Lock status: "keypad 4/left arrow" is the same key in VP whether
            // Num Lock is on or off.  Our main goal here is to make it easier to set
            // things up in VP, so it would be confusing if we didn't behave the same
            // way as VP.  So remap the keypad cursor keys to keypad keys.  We can tell
            // it's a keypad key if the Extended bit is OFF.  If the Extended bit is 
            // ON, it's one of the non-keypad cursor keys, so keep it as-is. 
            //
            // To make the mapping, build a table of the cursor key VK codes to numeric
            // keypad equivalents.
            numpadMap[VK_INSERT] = 0x60;
            numpadMap[VK_END] = 0x61;
            numpadMap[VK_DOWN] = 0x62;
            numpadMap[VK_PAGEDN] = 0x63;
            numpadMap[VK_LEFT] = 0x64;
            numpadMap[VK_CLEAR] = 0x65;
            numpadMap[VK_RIGHT] = 0x66;
            numpadMap[VK_HOME] = 0x67;
            numpadMap[VK_UP] = 0x68;
            numpadMap[VK_PAGEUP] = 0x69;
            numpadMap[VK_DELETE] = 0x6E;

            // load the control images
            kbImage = new Bitmap("html\\kb.png");
            jsImage = new Bitmap("html\\joystickButton.png");
            jsOn = new Bitmap("html\\jskeySmallOn.png");
            jsOff = new Bitmap("html\\jskeySmallOff.png");
        }

        Font captionFont = SystemFonts.CaptionFont;
        Font menuFont = SystemFonts.MenuFont;

        bool disposed = false;
        private void ButtonStatus_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (disposed)
                return;

            // dispose of GDI resources
            picKl25z.Dispose(); picKl25z = null;
            kbImage.Dispose(); kbImage = null;
            jsImage.Dispose(); jsImage = null;
            jsOn.Dispose(); jsOn = null;
            jsOff.Dispose(); jsOff = null;
            kbPic.Image.Dispose(); kbPic.Image = null;
            captionFont.Dispose(); captionFont = null;
            menuFont.Dispose(); menuFont = null;

            foreach (Control panel in btnListPanel.Controls)
            {
                foreach (Control chi in panel.Controls)
                {
                    PictureBox pb = chi as PictureBox;
                    if (pb != null && pb.Image != null)
                    {
                        pb.Image.Dispose();
                        pb.Image = null;
                    }
                }
            }

            disposed = true;
        }

        // our Pinscape device handle
        DeviceInfo dev;

        // keyboard and joystick button images
        Image kbImage;
        Image jsImage;

        // joystick button images, for the on-screen status display
        Image jsOn, jsOff;

        struct KeyCap
        {
            public KeyCap(int usbkey, uint vkey, String name, int x, int y, int wid, int ht, int cx, int cy, int cwid, int cht)
            {
                this.usbkey = usbkey;
                this.vkey = vkey;
                this.name = name;
                this.x = x;
                this.y = y;
                this.wid = wid;
                this.ht = ht;
                this.cx = cx;
                this.cy = cy;
                this.cwid = cwid;
                this.cht = cht;
            }
            public int usbkey;      // USB key code
            public uint vkey;       // Windows virtual key code
            public String name;     // display name
            public int x, y;        // x/y location of key on keyboard layout image
            public int wid, ht;     // width/height of key on keyboard layout image
            public int cx, cy;      // location of list view version of key cap on image
            public int cwid, cht;   // size of list view version of key cap
        }

        Dictionary<int, KeyCap> keyCaps = new Dictionary<int, KeyCap>();
        Dictionary<int, KeyCap> mediaCaps = new Dictionary<int, KeyCap>();
        Dictionary<uint, KeyCap> vkeyCaps = new Dictionary<uint, KeyCap>();
        Dictionary<uint, uint> numpadMap = new Dictionary<uint, uint>();

        void InitKeyCap(
            Dictionary<int, KeyCap> dict, int usbkey, uint vkey, String name,
            int x, int y, int wid, int ht, int cx, int cy, int cwid, int cht)
        {
            // create the key cap and add it to the dictionary for the device key code mapping
            KeyCap kc = new KeyCap(usbkey, vkey, name, x, y, wid, ht, cx, cy, cwid, cht);
            dict.Add(usbkey, kc);

            // If it's not already in the Windows vkey mapping dictionary, add it.  Note that
            // some keys have device mappings in both the regular keyboard keys and media keys,
            // but the Windows vkey system unifies them both in one namespace, so we only need
            // one version of the key cap for the Windows mapping.
            if (!vkeyCaps.ContainsKey(vkey))
                vkeyCaps.Add(vkey, kc);
        }

        // status labels
        Label[] statusLabels;

        // pin names, indexed by button number
        String[] pinNames;

        // scale of small keyboard image to full keyboard image
        float kbPicScale;

        int icon1MarginX;
        int btnMarginX, btnSpacingX;
        int bottomPanelMarginY;
        private void ButtonStatus_Load(object sender, EventArgs e)
        {
            // load the small on-screen keyboard image
            kbPic.Image = new Bitmap("html\\kbSmall.png");

            // figure the scale relative to the full keyboard image, which is
            // what the layout coordinates in the keycaps are based on 
            kbPicScale = (float)kbPic.Image.Width / kbImage.Width;

            // set up the pin display object
            pinDisplay = new KL25ZPinDisplay(picKl25z);

            // note layout items for resizing purposes
            int w = ClientRectangle.Width, h = ClientRectangle.Height;
            icon1MarginX = w - icon1.Right;
            btnMarginX = w - btnClose.Right;
            btnSpacingX = btnClose.Left - btnHelp.Right;
            bottomPanelMarginY = h - bottomPanel.Top;

            // start with the prototype row
            Panel btnPanel = btnPanel1;
            Label btnNum = btnNum1;
            Label btnPort = btnPort1;
            Label btnState = btnState1;
            PictureBox btnKeyA = btnKey1a;
            PictureBox btnKeyB = btnKey1b;
            PictureBox btnShift = btnShift1;

            // row copying functions
            Func<Panel, Panel> CopyPanel = (p) =>
            {
                Panel n = new Panel();
                btnListPanel.Controls.Add(n);
                n.Left = p.Left;
                n.Top = p.Top + p.Height;
                n.Width = p.Width;
                n.Height = p.Height;
                return n;
            };
            Func<Label, Label> CopyLabel = (l) =>
            {
                Label n = new Label();
                btnPanel.Controls.Add(n);
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
            Func<PictureBox, PictureBox> CopyPic = (p) =>
            {
                PictureBox n = new PictureBox();
                btnPanel.Controls.Add(n);
                n.Left = p.Left;
                n.Top = p.Top;
                n.Width = p.Width;
                n.Height = p.Height;
                n.SizeMode = PictureBoxSizeMode.StretchImage;
                return n;
            };

            StringFormat centerText = new StringFormat();
            centerText.Alignment = StringAlignment.Center;
            centerText.LineAlignment = StringAlignment.Center;
            Action<PictureBox, int, int> SetKeyCap = (pic, keyType, keyCode) =>
            {
                switch (keyType)
                {
                    case 0:
                        // no key assignment
                        break;

                    case 1:
                        // joystick
                        pic.Image = new Bitmap(jsImage);
                        using (var g = Graphics.FromImage(pic.Image))
                        {
                            Font font = captionFont;
                            String num = keyCode.ToString();
                            g.MeasureString(num, font);
                            g.DrawString(num, font, Brushes.White, new Point(jsImage.Height / 2, jsImage.Width / 2), centerText);
                        }
                        break;

                    case 2:
                    case 3:
                        // keyboard or media key
                        Dictionary<int, KeyCap> dict = keyType == 2 ? keyCaps : mediaCaps;
                        if (dict.ContainsKey(keyCode))
                        {
                            KeyCap kc = dict[keyCode];
                            Bitmap cb = new Bitmap(kc.cwid, kc.cht);
                            Rectangle src = new Rectangle(kc.cx, kc.cy, kc.cwid, kc.cht);
                            Rectangle dst = new Rectangle(0, 0, kc.cwid, kc.cht);
                            using (var g = Graphics.FromImage(cb))
                            {
                                g.DrawImage(kbImage, dst, src, GraphicsUnit.Pixel);
                            }
                            pic.Image = cb;
                        }
                        break;
                }
            };

            // check to see if a shift button is enabled
            int shiftButtonIdx = 0;
            byte[] buf = dev.QueryConfigVar(16);
            if (buf != null)
                shiftButtonIdx = buf[0];

            // gray out the "Shifted" column header if there's no shift button
            if (shiftButtonIdx == 0)
                lblShifted.ForeColor = Color.Gray;

            // Mouse enter/leave handler generators.  Highlight the associated pin 
            // on the KL25Z diagram.
            Action<String, bool> SetPinHover = (pin, hilite) =>
            {
                pinDisplay.SetHover(pin, hilite);
            };
            Func<String, EventHandler> PanelMouseEnter = (pin) =>
            {
                EventHandler h1 = (object s1, EventArgs e1) =>
                {
                    SetPinHover(pin, true);
                    btnListPanel.Focus();
                };
                return new EventHandler(h1);
            };
            Func<String, EventHandler> PanelMouseLeave = (pin) =>
            {
                EventHandler h1 = (object s1, EventArgs e1) => { SetPinHover(pin, false); };
                return new EventHandler(h1);
            };

            // load the button configuration
            buf = dev.QueryConfigVar(254, 0);
            if (buf != null)
            {
                byte nButtons = buf[0];
                statusLabels = new Label[nButtons];
                pinNames = new String[nButtons];
                for (byte i = 1; i <= nButtons; ++i)
                {
                    // if this isn't the first row, copy the prototype
                    if (i > 1)
                    {
                        btnPanel = CopyPanel(btnPanel);
                        btnNum = CopyLabel(btnNum);
                        btnPort = CopyLabel(btnPort);
                        btnState = CopyLabel(btnState);
                        btnKeyA = CopyPic(btnKeyA);
                        btnKeyB = CopyPic(btnKeyB);
                        btnShift = CopyPic(btnShift);
                    }

                    if ((i & 1) == 0)
                        btnPanel.BackColor = Color.GhostWhite;

                    // retrieve the button description
                    buf = dev.QueryConfigVar(254, i);
                    byte[] xbuf = dev.QueryConfigVar(253, i);

                    // fill in the labels
                    btnNum.Text = i.ToString();
                    String pin = DeviceInfo.WireToPinName(buf[0]);
                    btnPort.Text = (pin == "NC" ? "None" : pin);
                    btnState.Text = "Off";

                    // Set up the mouse event handlers on the panel and its children.
                    // It's a little tedious, but we have to install the handlers on
                    // all of the children, because the event model doesn't stack: the
                    // mouse is only in one control at a time, so it leaves the parent
                    // when entering a child.  We want the parent and all children to
                    // be the same for the purposes of the hovering, so we need the same
                    // handlers on all of them.
                    EventHandler enter = PanelMouseEnter(pin);
                    EventHandler leave = PanelMouseLeave(pin);
                    btnPanel.MouseEnter += enter;
                    btnPanel.MouseLeave += leave;
                    foreach (Control chi in btnPanel.Controls)
                    {
                        chi.MouseEnter += enter;
                        chi.MouseLeave += leave;
                    }

                    // set the key caps
                    SetKeyCap(btnKeyA, buf[1], buf[2]);
                    if (shiftButtonIdx != 0 && shiftButtonIdx != i)
                        SetKeyCap(btnKeyB, xbuf[0], xbuf[1]);

                    // if this is the shift button, set the shift button image
                    if (i == shiftButtonIdx)
                        btnShift.Image = new Bitmap("html\\shiftButtonSmall.png");

                    // remember the status label and pin name
                    statusLabels[i - 1] = btnState;
                    pinNames[i - 1] = pin;
                }
            }

        }

        private void ButtonStatus_Resize(object sender, EventArgs e)
        {
            int w = ClientRectangle.Width, h = ClientRectangle.Height;
            bar1.Width = w + 1;
            bar2.Width = w + 1;
            bar3.Width = w - bar3.Left;
            bar4.Width = w - bar4.Left;
            icon1.Left = w - icon1MarginX - icon1.Width;
            btnClose.Left = w - btnMarginX - btnClose.Width;
            btnHelp.Left = btnClose.Left - btnSpacingX - btnHelp.Width;
            bottomPanel.Top = h - bottomPanelMarginY;
            bottomPanel.Width = w;
            btnListPanel.Height = bottomPanel.Top - btnListPanel.Top + 1;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }


        private void btnHelp_Click(object sender, EventArgs e)
        {
            (new Help("HelpButtonTester.htm")).Show();
        }

        // joystick button states, packed 1 bit per button
        uint jsState = 0;

        // update the button pin status and joystick button status periodically
        private void statusTimer_Tick(object sender, EventArgs e)
        {
            // read the button status - special request 13, special reply type 0xA100
            byte[] buf = dev.SpecialRequest(13, new byte[] { }, (r) => r[1] == 0x00 && r[2] == 0xA1);
            if (buf != null)
            {
                int n = buf[3];
                for (int i = 0, bi = 4, shift = 0; i < n; ++i)
                {
                    // pull out this bit
                    bool state = ((buf[bi] >> shift) & 0x01) != 0;

                    // set the label
                    if (i < statusLabels.Length)
                    {
                        Label l = statusLabels[i];
                        if (state)
                        {
                            l.Text = "ON";
                            l.ForeColor = Color.White;
                            l.BackColor = Color.Red;
                        }
                        else
                        {
                            l.Text = "OFF";
                            l.ForeColor = Color.DarkGray;
                            l.BackColor = l.Parent.BackColor;
                        }

                        // update the KL25Z diagram pin
                        pinDisplay.SetOnOff(pinNames[i], state);
                    }

                    // on to the next bit
                    if (++shift > 7)
                    {
                        shift = 0;
                        ++bi;
                    }
                }
            }

            // Get the state of the shift keys.  For some reason, we don't seem
            // to get a WM_KEYUP message for LEFT SHIFT when RIGHT SHIFT is being
            // held down, and vice versa.  These seem to be the only keys affected
            // by this.  And for unrealted reasons, the GUI keys sometimes miss
            // their WM_KEYUP messages to us as well, because pressing these can
            // shift focus away from our window.
            Action<uint, Key> CheckKey = (vk, key) =>
            {
                if (keysDown.ContainsKey(vk) && !Keyboard.IsKeyDown(key))
                {
                    keysDown.Remove(vk);
                    kbPic.Invalidate();
                }
            };
            CheckKey(VK_LSHIFT, Key.LeftShift);
            CheckKey(VK_RSHIFT, Key.RightShift);
            CheckKey(VK_LGUI, Key.LWin);
            CheckKey(VK_RGUI, Key.RWin);

            // get the joystick status
            uint jsNew = jsState;
            for (int i = 0; i < 16; ++i)
            {
                // read a report
                buf = dev.ReadUSB();

                // make sure it's a regular joystick report
                if (buf != null && (buf[2] & 0x80) == 0)
                {
                    jsNew = (uint)(buf[5] | (buf[6] << 8) | (buf[7] << 16) | (buf[8] << 24));
                    break;
                }
            }

            // if the joystick state changed, redraw the buttons
            if (jsNew != jsState)
            {
                jsState = jsNew;
                jsKeyState.Invalidate();
            }
        }

        // Draw the on-screen keyboard.  This draws a highlight rectangle
        // around each pressed key.
        private void kbPic_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            using (Pen pen = new Pen(Color.Red, 4))
            {
                int x = 0, y = 0;
                foreach (uint vk in keysDown.Keys)
                {
                    if (vkeyCaps.ContainsKey(vk))
                    {
                        KeyCap kc = vkeyCaps[vk];
                        g.DrawRectangle(pen, x + kc.x * kbPicScale, y + kc.y * kbPicScale,
                            kc.wid * kbPicScale + 4, kc.ht * kbPicScale + 4);
                    }
                }
            }
        }

        // Draw the joystick button status display
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
                bool on = (jsState & (1 << i)) != 0;

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

        // Intercept key messages, to keep track of which keys are pressed,
        // for the on-screen display.
        const int WM_KEYDOWN = 0x100;
        const int WM_KEYUP = 0x101;
        const int WM_SYSKEYDOWN = 0x104;
        const int WM_SYSKEYUP = 0x105;
        Dictionary<uint, bool> keysDown = new Dictionary<uint, bool>();
        protected override bool ProcessKeyPreview(ref Message msg)
        {
            switch (msg.Msg)
            {
                case WM_KEYDOWN:
                case WM_SYSKEYDOWN:
                    uint vk = MapLeftRightKeys(msg.WParam, msg.LParam);
                    if (!keysDown.ContainsKey(vk))
                    {
                        keysDown[vk] = true;
                        kbPic.Invalidate();

                        if (keysDown.ContainsKey(VK_F4)
                            && (keysDown.ContainsKey(VK_LMENU) || keysDown.ContainsKey(VK_RMENU)))
                            Close();
                    }
                    return true;

                case WM_KEYUP:
                case WM_SYSKEYUP:
                    vk = MapLeftRightKeys(msg.WParam, msg.LParam);
                    if (keysDown.ContainsKey(vk))
                    {
                        keysDown.Remove(vk);
                        kbPic.Invalidate();
                    }
                    return true;
            }
            return base.ProcessKeyPreview(ref msg);
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            switch (keyData & ~Keys.Shift)
            {
                case Keys.Enter:
                case Keys.Tab:
                case Keys.Escape:
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                case Keys.PageUp:
                case Keys.PageDown:
                    return false;

                default:
                    return base.ProcessDialogKey(keyData);
            }
        }

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern uint MapVirtualKey(
                uint uCode,
                uint uMapType);
        const int MAPVK_VSC_TO_EX = 3;

        const uint VK_SHIFT = 0x10;
        const uint VK_LSHIFT = 0xA0;
        const uint VK_RSHIFT = 0xA1;
        const uint VK_CONTROL = 0x11;
        const uint VK_LCONTROL = 0xA2;
        const uint VK_RCONTROL = 0xA3;
        const uint VK_MENU = 0x12;  // Alt key
        const uint VK_LMENU = 0xA4;
        const uint VK_RMENU = 0xA5;
        const uint VK_LGUI = 0x5B;
        const uint VK_RGUI = 0x5C;
        const uint VK_INSERT = 0x2D;
        const uint VK_DELETE = 0x2E;
        const uint VK_HOME = 0x24;
        const uint VK_END = 0x23;
        const uint VK_LEFT = 0x25;
        const uint VK_RIGHT = 0x27;
        const uint VK_UP = 0x26;
        const uint VK_DOWN = 0x28;
        const uint VK_PAGEUP = 0x21; // VK_PRIOR
        const uint VK_PAGEDN = 0x22; // VK_NEXT
        const uint VK_CLEAR = 0x0C;
        const uint VK_ENTER = 0x0D;
        const uint VK_F4 = 0x73;
        uint MapLeftRightKeys(IntPtr wparam, IntPtr lparam)
        {
            uint vk = (uint)wparam.ToInt32();
            uint flags = (uint)lparam.ToInt64();
            uint scancode = (flags & 0x00ff0000) >> 16;
            bool extended = (flags & 0x01000000) != 0;
            switch (vk)
            {
                case VK_SHIFT:
                    vk = MapVirtualKey(scancode, MAPVK_VSC_TO_EX);
                    break;

                case VK_CONTROL:
                    vk = extended ? VK_RCONTROL : VK_LCONTROL;
                    break;

                case VK_MENU:
                    vk = extended ? VK_RMENU : VK_LMENU;
                    break;

                case VK_INSERT:
                case VK_DELETE:
                case VK_HOME:
                case VK_END:
                case VK_PAGEUP:
                case VK_PAGEDN:
                case VK_LEFT:
                case VK_RIGHT:
                case VK_UP:
                case VK_DOWN:
                case VK_CLEAR:
                    // For compatibility with VP, map the cursor keys on the keypad
                    // to the keypad keys.  It's a keypad cursor key if the Extended
                    // bit is OFF.  If the Extended bit is ON, it's one of the non-keypad
                    // cursor keys on an extended keyboard, so leave it as-is.
                    if (!extended)
                        vk = numpadMap[vk];
                    break;

                case VK_ENTER:
                    // Keyboard Enter and Num Keypad Enter both have the same VK_ scan
                    // code, but are distinguishable by the Extended bit (Num Enter has
                    // it, regular Enter doesn't).  VP distinguishes these keys, so use
                    // a separate synthetic scan code of 0x100D for the keypad version.
                    if (extended)
                        vk = 0x100D;
                    break;
            }
            return vk;
        }

        private void btnListPanel_MouseEnter(object sender, EventArgs e)
        {
            // gain focus when the mouse is over us, to allow wheel scrolling
            btnListPanel.Focus();
        }

        // pin display
        KL25ZPinDisplay pinDisplay;

        // table of pin descriptors, 
        Dictionary<int, KL25ZPinDisplay.Pin> pinByButtonNo = new Dictionary<int, KL25ZPinDisplay.Pin>();

        // Set up the key caps, mechanically generated from html/keycaps.js
        private void InitKeyCaps()
        {
            InitKeyCap(keyCaps, 0x04, 0x41, "Keyboard A", 68, 168, 30, 31, 68, 168, 30, 31);
            InitKeyCap(keyCaps, 0x05, 0x42, "Keyboard B", 231, 205, 30, 31, 231, 205, 30, 31);
            InitKeyCap(keyCaps, 0x06, 0x43, "Keyboard C", 157, 205, 30, 31, 157, 205, 30, 31);
            InitKeyCap(keyCaps, 0x07, 0x44, "Keyboard D", 141, 168, 30, 31, 141, 168, 30, 31);
            InitKeyCap(keyCaps, 0x08, 0x45, "Keyboard E", 131, 131, 30, 31, 131, 131, 30, 31);
            InitKeyCap(keyCaps, 0x09, 0x46, "Keyboard F", 178, 168, 30, 31, 178, 168, 30, 31);
            InitKeyCap(keyCaps, 0x0A, 0x47, "Keyboard G", 214, 168, 30, 31, 214, 168, 30, 31);
            InitKeyCap(keyCaps, 0x0B, 0x48, "Keyboard H", 252, 168, 30, 31, 252, 168, 30, 31);
            InitKeyCap(keyCaps, 0x0C, 0x49, "Keyboard I", 316, 131, 30, 31, 316, 131, 30, 31);
            InitKeyCap(keyCaps, 0x0D, 0x4A, "Keyboard J", 289, 168, 30, 31, 289, 168, 30, 31);
            InitKeyCap(keyCaps, 0x0E, 0x4B, "Keyboard K", 326, 168, 30, 31, 326, 168, 30, 31);
            InitKeyCap(keyCaps, 0x0F, 0x4C, "Keyboard L", 362, 168, 30, 31, 362, 168, 30, 31);
            InitKeyCap(keyCaps, 0x10, 0x4D, "Keyboard M", 306, 205, 30, 31, 306, 205, 30, 31);
            InitKeyCap(keyCaps, 0x11, 0x4E, "Keyboard N", 268, 205, 30, 31, 268, 205, 30, 31);
            InitKeyCap(keyCaps, 0x12, 0x4F, "Keyboard O", 352, 131, 30, 31, 352, 131, 30, 31);
            InitKeyCap(keyCaps, 0x13, 0x50, "Keyboard P", 389, 131, 30, 31, 389, 131, 30, 31);
            InitKeyCap(keyCaps, 0x14, 0x51, "Keyboard Q", 57, 131, 30, 31, 57, 131, 30, 31);
            InitKeyCap(keyCaps, 0x15, 0x52, "Keyboard R", 168, 131, 30, 31, 168, 131, 30, 31);
            InitKeyCap(keyCaps, 0x16, 0x53, "Keyboard S", 104, 168, 30, 31, 104, 168, 30, 31);
            InitKeyCap(keyCaps, 0x17, 0x54, "Keyboard T", 205, 131, 30, 31, 205, 131, 30, 31);
            InitKeyCap(keyCaps, 0x18, 0x55, "Keyboard U", 279, 131, 30, 31, 279, 131, 30, 31);
            InitKeyCap(keyCaps, 0x19, 0x56, "Keyboard V", 194, 205, 30, 31, 194, 205, 30, 31);
            InitKeyCap(keyCaps, 0x1A, 0x57, "Keyboard W", 94, 131, 30, 31, 94, 131, 30, 31);
            InitKeyCap(keyCaps, 0x1B, 0x58, "Keyboard X", 120, 205, 30, 31, 120, 205, 30, 31);
            InitKeyCap(keyCaps, 0x1C, 0x59, "Keyboard Y", 242, 131, 30, 31, 242, 131, 30, 31);
            InitKeyCap(keyCaps, 0x1D, 0x5A, "Keyboard Z", 83, 205, 30, 31, 83, 205, 30, 31);
            InitKeyCap(keyCaps, 0x1E, 0x31, "Keyboard 1 !", 42, 94, 30, 31, 42, 94, 30, 31);
            InitKeyCap(keyCaps, 0x1F, 0x32, "Keyboard 2 @", 79, 94, 30, 31, 79, 94, 30, 31);
            InitKeyCap(keyCaps, 0x20, 0x33, "Keyboard 3 #", 116, 94, 30, 31, 116, 94, 30, 31);
            InitKeyCap(keyCaps, 0x21, 0x34, "Keyboard 4 $", 153, 94, 30, 31, 153, 94, 30, 31);
            InitKeyCap(keyCaps, 0x22, 0x35, "Keyboard 5 %", 190, 94, 30, 31, 190, 94, 30, 31);
            InitKeyCap(keyCaps, 0x23, 0x36, "Keyboard 6 ^", 227, 94, 30, 31, 227, 94, 30, 31);
            InitKeyCap(keyCaps, 0x24, 0x37, "Keyboard 7 &", 264, 94, 30, 31, 264, 94, 30, 31);
            InitKeyCap(keyCaps, 0x25, 0x38, "Keyboard 8 *", 300, 94, 30, 31, 300, 94, 30, 31);
            InitKeyCap(keyCaps, 0x26, 0x39, "Keyboard 9 (", 337, 94, 30, 31, 337, 94, 30, 31);
            InitKeyCap(keyCaps, 0x27, 0x30, "Keyboard 0 )", 374, 94, 30, 31, 374, 94, 30, 31);
            InitKeyCap(keyCaps, 0x28, 0x0D, "Keyboard Enter", 471, 168, 72, 31, 480, 281, 41, 31);
            InitKeyCap(keyCaps, 0x29, 0x1B, "Keyboard Escape", 4, 41, 30, 31, 4, 41, 30, 31);
            InitKeyCap(keyCaps, 0x2A, 0x08, "Keyboard Backspace", 485, 94, 59, 31, 437, 281, 41, 31);
            InitKeyCap(keyCaps, 0x2B, 0x09, "Keyboard Tab", 5, 131, 44, 31, 393, 281, 41, 31);
            InitKeyCap(keyCaps, 0x2C, 0x20, "Keyboard Spacebar", 141, 242, 219, 31, 350, 281, 41, 31);
            InitKeyCap(keyCaps, 0x2D, 0xBD, "Keyboard -_", 411, 94, 30, 31, 411, 94, 30, 31);
            InitKeyCap(keyCaps, 0x2E, 0xBB, "Keyboard =+", 448, 94, 30, 31, 448, 94, 30, 31);
            InitKeyCap(keyCaps, 0x2F, 0xDB, "Keyboard [{", 426, 131, 30, 31, 426, 131, 30, 31);
            InitKeyCap(keyCaps, 0x30, 0xDD, "Keyboard ]}", 463, 131, 30, 31, 463, 131, 30, 31);
            InitKeyCap(keyCaps, 0x31, 0xDC, "Keyboard |", 500, 131, 44, 31, 608, 281, 44, 31);
            InitKeyCap(keyCaps, 0x33, 0xBA, "Keyboard ;:", 399, 168, 30, 31, 399, 168, 30, 31);
            InitKeyCap(keyCaps, 0x34, 0xDE, "Keyboard '\"", 436, 168, 30, 31, 436, 168, 30, 31);
            InitKeyCap(keyCaps, 0x35, 0xC0, "Keyboard `~", 5, 94, 30, 31, 5, 94, 30, 31);
            InitKeyCap(keyCaps, 0x36, 0xBC, "Keyboard ,<", 342, 205, 30, 31, 342, 205, 30, 31);
            InitKeyCap(keyCaps, 0x37, 0xBE, "Keyboard .>", 378, 205, 30, 31, 378, 205, 30, 31);
            InitKeyCap(keyCaps, 0x38, 0xBF, "Keyboard /?", 415, 205, 30, 31, 415, 205, 30, 31);
            InitKeyCap(keyCaps, 0x39, 0x14, "Keyboard Caps Lock", 5, 168, 55, 31, 523, 281, 41, 31);
            InitKeyCap(keyCaps, 0x3A, 0x70, "Keyboard F1", 77, 41, 30, 31, 77, 41, 30, 31);
            InitKeyCap(keyCaps, 0x3B, 0x71, "Keyboard F2", 114, 41, 30, 31, 114, 41, 30, 31);
            InitKeyCap(keyCaps, 0x3C, 0x72, "Keyboard F3", 151, 41, 30, 31, 151, 41, 30, 31);
            InitKeyCap(keyCaps, 0x3D, 0x73, "Keyboard F4", 188, 41, 30, 31, 188, 41, 30, 31);
            InitKeyCap(keyCaps, 0x3E, 0x74, "Keyboard F5", 242, 41, 30, 31, 242, 41, 30, 31);
            InitKeyCap(keyCaps, 0x3F, 0x75, "Keyboard F6", 280, 41, 30, 31, 280, 41, 30, 31);
            InitKeyCap(keyCaps, 0x40, 0x76, "Keyboard F7", 317, 41, 30, 31, 317, 41, 30, 31);
            InitKeyCap(keyCaps, 0x41, 0x77, "Keyboard F8", 354, 41, 30, 31, 354, 41, 30, 31);
            InitKeyCap(keyCaps, 0x42, 0x78, "Keyboard F9", 409, 41, 30, 31, 409, 41, 30, 31);
            InitKeyCap(keyCaps, 0x43, 0x79, "Keyboard F10", 446, 41, 30, 31, 446, 41, 30, 31);
            InitKeyCap(keyCaps, 0x44, 0x7A, "Keyboard F11", 482, 41, 30, 31, 482, 41, 30, 31);
            InitKeyCap(keyCaps, 0x45, 0x7B, "Keyboard F12", 520, 41, 30, 31, 520, 41, 30, 31);
            InitKeyCap(keyCaps, 0x47, 0x91, "Keyboard Scroll Lock", 602, 41, 30, 31, 602, 41, 30, 31);
            InitKeyCap(keyCaps, 0x48, 0x13, "Keyboard Pause", 638, 41, 30, 31, 638, 41, 30, 31);
            InitKeyCap(keyCaps, 0x49, 0x2D, "Keyboard Insert", 565, 94, 30, 31, 565, 94, 30, 31);
            InitKeyCap(keyCaps, 0x4A, 0x24, "Keyboard Home", 602, 94, 30, 31, 602, 94, 30, 31);
            InitKeyCap(keyCaps, 0x4B, 0x21, "Keyboard Page Up", 639, 94, 30, 31, 639, 94, 30, 31);
            InitKeyCap(keyCaps, 0x4C, 0x2E, "Keyboard Delete", 565, 131, 30, 31, 565, 131, 30, 31);
            InitKeyCap(keyCaps, 0x4D, 0x23, "Keyboard End", 602, 131, 30, 31, 602, 131, 30, 31);
            InitKeyCap(keyCaps, 0x4E, 0x22, "Keyboard Page Down", 639, 131, 30, 31, 639, 131, 30, 31);
            InitKeyCap(keyCaps, 0x4F, 0x27, "Keyboard Right Arrow", 638, 242, 30, 31, 638, 242, 30, 31);
            InitKeyCap(keyCaps, 0x50, 0x25, "Keyboard Left Arrow", 564, 242, 30, 31, 564, 242, 30, 31);
            InitKeyCap(keyCaps, 0x51, 0x28, "Keyboard Down Arrow", 602, 242, 30, 31, 602, 242, 30, 31);
            InitKeyCap(keyCaps, 0x52, 0x26, "Keyboard Up Arrow", 601, 205, 30, 31, 601, 205, 30, 31);
            InitKeyCap(keyCaps, 0x53, 0x90, "Keyboard Num Lock", 682, 94, 30, 31, 682, 94, 30, 31);
            InitKeyCap(keyCaps, 0x54, 0x6F, "Keypad /", 718, 94, 30, 31, 718, 94, 30, 31);
            InitKeyCap(keyCaps, 0x55, 0x6A, "Keypad *", 756, 94, 30, 31, 756, 94, 30, 31);
            InitKeyCap(keyCaps, 0x56, 0x6D, "Keypad -", 792, 94, 30, 31, 792, 94, 30, 31);
            InitKeyCap(keyCaps, 0x57, 0x6B, "Keypad +", 792, 131, 30, 66, 749, 278, 30, 66);
            InitKeyCap(keyCaps, 0x58, 0x100D, "Keypad Enter", 791, 204, 30, 66, 717, 278, 30, 66);
            InitKeyCap(keyCaps, 0x59, 0x61, "Keypad 1/End", 682, 204, 30, 31, 682, 204, 30, 31);
            InitKeyCap(keyCaps, 0x5A, 0x62, "Keypad 2/Down Arrow", 720, 204, 30, 31, 720, 204, 30, 31);
            InitKeyCap(keyCaps, 0x5B, 0x63, "Keypad 3/Page Down", 757, 204, 30, 31, 757, 204, 30, 31);
            InitKeyCap(keyCaps, 0x5C, 0x64, "Keypad 4/Left Arrow", 682, 167, 30, 31, 682, 167, 30, 31);
            InitKeyCap(keyCaps, 0x5D, 0x65, "Keypad 5", 720, 167, 30, 31, 720, 167, 30, 31);
            InitKeyCap(keyCaps, 0x5E, 0x66, "Keypad 6/Right Arrow", 757, 167, 30, 31, 757, 167, 30, 31);
            InitKeyCap(keyCaps, 0x5F, 0x67, "Keypad 7/Home", 682, 131, 30, 31, 682, 131, 30, 31);
            InitKeyCap(keyCaps, 0x60, 0x68, "Keypad 8/Up Arrow", 720, 131, 30, 31, 720, 131, 30, 31);
            InitKeyCap(keyCaps, 0x61, 0x69, "Keypad 9/Page Up", 757, 131, 30, 31, 757, 131, 30, 31);
            InitKeyCap(keyCaps, 0x62, 0x60, "Keypad 0/Insert", 683, 241, 68, 31, 684, 278, 68, 31);
            InitKeyCap(keyCaps, 0x63, 0x6E, "Keypad ./Delete", 757, 241, 30, 31, 757, 241, 30, 31);
            InitKeyCap(keyCaps, 0x65, 0x5D, "Application Key", 458, 241, 41, 31, 458, 241, 41, 31);
            InitKeyCap(keyCaps, 0x68, 0x7C, "Keyboard F13", 77, 4, 30, 31, 77, 4, 30, 31);
            InitKeyCap(keyCaps, 0x69, 0x7D, "Keyboard F14", 114, 4, 30, 31, 114, 4, 30, 31);
            InitKeyCap(keyCaps, 0x6A, 0x7E, "Keyboard F15", 151, 4, 30, 31, 151, 4, 30, 31);
            InitKeyCap(keyCaps, 0x6B, 0x7F, "Keyboard F16", 188, 4, 30, 31, 188, 4, 30, 31);
            InitKeyCap(keyCaps, 0x6C, 0x80, "Keyboard F17", 242, 4, 30, 31, 242, 4, 30, 31);
            InitKeyCap(keyCaps, 0x6D, 0x81, "Keyboard F18", 280, 4, 30, 31, 280, 4, 30, 31);
            InitKeyCap(keyCaps, 0x6E, 0x82, "Keyboard F19", 317, 4, 30, 31, 317, 4, 30, 31);
            InitKeyCap(keyCaps, 0x6F, 0x83, "Keyboard F20", 354, 4, 30, 31, 354, 4, 30, 31);
            InitKeyCap(keyCaps, 0x70, 0x84, "Keyboard F21", 409, 4, 30, 31, 409, 4, 30, 31);
            InitKeyCap(keyCaps, 0x71, 0x85, "Keyboard F22", 446, 4, 30, 31, 446, 4, 30, 31);
            InitKeyCap(keyCaps, 0x72, 0x86, "Keyboard F23", 482, 4, 30, 31, 482, 4, 30, 31);
            InitKeyCap(keyCaps, 0x73, 0x87, "Keyboard F24", 520, 4, 30, 31, 520, 4, 30, 31);
            InitKeyCap(keyCaps, 0x7F, 0xAD, "Keyboard Mute", 791, 42, 30, 31, 791, 42, 30, 31);
            InitKeyCap(keyCaps, 0x80, 0xAF, "Keyboard Volume Up", 722, 42, 30, 31, 722, 42, 30, 31);
            InitKeyCap(keyCaps, 0x81, 0xAE, "Keyboard Volume Down", 753, 42, 30, 31, 753, 42, 30, 31);
            InitKeyCap(keyCaps, 0xE0, 0xA2, "Keyboard Left Control", 5, 242, 41, 31, 91, 281, 41, 31);
            InitKeyCap(keyCaps, 0xE1, 0xA0, "Keyboard Left Shift", 5, 205, 69, 31, 5, 281, 41, 31);
            InitKeyCap(keyCaps, 0xE2, 0xA4, "Keyboard Left Alt", 95, 242, 41, 31, 178, 281, 41, 31);
            InitKeyCap(keyCaps, 0xE3, 0x5B, "Keyboard Left GUI", 50, 242, 41, 31, 264, 281, 41, 31);
            InitKeyCap(keyCaps, 0xE4, 0xA3, "Keyboard Right Control", 504, 242, 41, 31, 134, 281, 41, 31);
            InitKeyCap(keyCaps, 0xE5, 0xA1, "Keyboard Right Shift", 453, 205, 91, 31, 47, 281, 41, 31);
            InitKeyCap(keyCaps, 0xE6, 0xA5, "Keyboard Right Alt", 368, 242, 41, 31, 221, 281, 41, 31);
            InitKeyCap(keyCaps, 0xE7, 0x5C, "Keyboard Right GUI", 413, 242, 41, 31, 306, 281, 41, 31);
            InitKeyCap(mediaCaps, 0xB5, 0xB0, "Media Next Track", 753, 4, 30, 31, 753, 4, 30, 31);
            InitKeyCap(mediaCaps, 0xB6, 0xB1, "Media Previous Track", 722, 4, 30, 31, 722, 4, 30, 31);
            InitKeyCap(mediaCaps, 0xCD, 0xB3, "Media Play/Pause", 791, 4, 30, 31, 791, 4, 30, 31);
            InitKeyCap(mediaCaps, 0xE2, 0xAD, "Media Mute", 791, 42, 30, 31, 791, 42, 30, 31);
            InitKeyCap(mediaCaps, 0xE9, 0xAF, "Media Volume Up", 722, 42, 30, 31, 722, 42, 30, 31);
            InitKeyCap(mediaCaps, 0xEA, 0xAE, "Media Volume Down", 753, 42, 30, 31, 753, 42, 30, 31);
        }

    }
}
