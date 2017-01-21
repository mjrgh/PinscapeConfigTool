using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PinscapeConfigTool
{
    public partial class TvOnTester : Form
    {
        public TvOnTester(DeviceInfo dev)
        {
            this.dev = dev;
            InitializeComponent();
        }

        // the Pinscape unit we're talking to
        DeviceInfo dev;

        String statusPin, latchPin, relayPin;

        private void TvOnTester_Load(object sender, EventArgs e)
        {
            // retrieve the TV ON configuration
            byte[] buf = dev.QueryConfigVar(9);
            if (buf != null)
            {
                lblStatusPin.Text = statusPin = DeviceInfo.WireToPinName(buf[0]);
                lblLatchPin.Text = latchPin = DeviceInfo.WireToPinName(buf[1]);
                lblRelayPin.Text = relayPin = DeviceInfo.WireToPinName(buf[2]);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        static String[] statusMsg = new String[]{
            "PSU2 Power is on",
            "PSU2 Power is off",
            "PSU2 Power is off",
            "TV ON delay timer running",
            "Pulsing TV ON relay"
        };
        private void statusTimer_Tick(object sender, EventArgs e)
        {
            // read a device status report
            byte[] buf = dev.ReadUSB();
            if (buf != null)
            {
                // get the current TV ON timer status
                int status = (buf[0] >> 2) & 0x07;
                lblStatus.Text = (status < statusMsg.Length ?
                    statusMsg[status] : "Invalid");
            }
        }

        private void ckRelayOn_CheckedChanged(object sender, EventArgs e)
        {
            // send the TV ON relay manual control request (11 + 0=off/1=on)
            dev.SpecialRequest(11, new byte[] { (byte)(ckRelayOn.Checked ? 1 : 0) });
        }

        private void btnRelayPulse_Click(object sender, EventArgs e)
        {
            // turn off the On/Off button
            ckRelayOn.Checked = false;

            // pulse the relay - special request 11 + 2=pulse
            dev.SpecialRequest(11, new byte[] { 2 });
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            (new Help("HelpTvOnTester.htm")).Show();
        }
    }
}
