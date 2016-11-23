using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace NightMode
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public class ListItem
        {
            public ListItem(String label, DeviceInfo dev)
            {
                this.label = label;
                this.dev = dev;
            }
            public String label;
            public DeviceInfo dev;
            override public String ToString() { return label; }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            foreach (DeviceInfo dev in DeviceInfo.FindDevices())
                AddDev(dev);
        }

        private ListItem AddDev(DeviceInfo dev)
        {
            String txt = "Pinscape unit #" + dev.PinscapeUnitNo;
            if (dev.LedWizUnitNo != 0)
                txt += " (LedWiz #" + dev.LedWizUnitNo + ")";
            ListItem i = new ListItem(txt, dev);
            checkedListBox1.Items.Add(i);
            curList.Add(i);
            return i;
        }

        private List<ListItem> curList = new List<ListItem>();

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (IsInForeground())
            {
                // get the new device list
                List<DeviceInfo> newList = DeviceInfo.FindDevices();

                // add any devices that aren't present, and update the checkbox status
                foreach (DeviceInfo dev in newList)
                {
                    // if the device isn't in the list, add it
                    ListItem li = curList.Find(l => l.dev.PinscapeUnitNo == dev.PinscapeUnitNo);
                    if (li == null)
                        li = AddDev(dev);

                    // update the checkbox status
                    byte[] buf = dev.ReadStatusReport();
                    if (buf != null)
                    {
                        bool nightMode = (buf[1] & 0x02) != 0;
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf(li), nightMode);
                    }
                }

                // remove any devices that are no longer present
                List<ListItem> delList = new List<ListItem>();
                foreach (ListItem l in curList)
                {
                    if (newList.Find(d => d.PinscapeUnitNo == l.dev.PinscapeUnitNo) == null)
                        delList.Add(l);
                }

                foreach (ListItem l in delList)
                {
                    checkedListBox1.Items.Remove(l);
                    curList.Remove(l);
                }
            }
        }

        // Win32 imports for IsInForeground
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        private static extern UInt32 GetWindowThreadProcessId(IntPtr hWnd, out UInt32 lpdwProcessId);

        // are we the foreground application?
        bool IsInForeground()
        {
            // get the foreground window handle
            IntPtr hwnd = GetForegroundWindow();
            if (hwnd.ToInt32() != 0)
            {
                // get the foreground window's PID
                UInt32 pid;
                if (GetWindowThreadProcessId(hwnd, out pid) != 0)
                {
                    // we're in the foreground if the window belongs to this process
                    return pid == Process.GetCurrentProcess().Id;
                }
            }

            // if anything went wrong, presume we're not in the foreground
            return false;
        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            ListItem item = checkedListBox1.Items[e.Index] as ListItem;
            item.dev.SpecialRequest(8, new byte[] { (byte)(e.NewValue == CheckState.Checked ? 0x01 : 0x00) });
        }

    }
}
