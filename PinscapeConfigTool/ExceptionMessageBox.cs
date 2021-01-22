using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PinscapeConfigTool
{
    public partial class ExceptionMessageBox : Form
    {
        public ExceptionMessageBox(Exception e)
        {
            exc = e;
            InitializeComponent();
        }

        Exception exc;

        private void ExceptionMessageBox_Load(object sender, EventArgs e)
        {
            txtMessage.Text = "An internal program error occured: "
                + exc.Message
                + "\r\n\r\n"
                + "(This is a problem within the setup tool, not anything you did wrong. "
                + "you did wrong. If you report it, please provide any details you can "
                + "about what you were doing when it occurred, plus the technical details "
                + "below.)";
        }

        private void ExceptionMessageBox_Shown(object sender, EventArgs e)
        {
            using (Graphics g = CreateGraphics())
            {
                StringFormat format = new StringFormat(StringFormatFlags.LineLimit);
                SizeF size = g.MeasureString(txtMessage.Text, txtMessage.Font, txtMessage.Width, format);
                int dy = (int)(size.Height - txtMessage.Height) + txtMessage.Margin.Vertical * 2;
                int dx = -(int)(size.Width - txtMessage.Width);
                if (dy > 0)
                {
                    txtMessage.Height += dy;
                    ChangeHeight(dy);
                }
                if (dx > 20)
                {
                    int wid = ClientRectangle.Width;
                    int margin = ActiveForm.Width - btnClose.Right;
                    if (dx > btnClose.Left - margin)
                        dx = btnClose.Left - margin;
                    if (wid - dx < btnDetails.Right + margin)
                        dx = wid - btnDetails.Right - margin;
                    txtMessage.Width -= dx;
                    btnClose.Left -= dx;
                    ActiveForm.Width -= dx;
                }
            }
        }

        private void ChangeHeight(int dy)
        {
            if (dy > 0)
            {
                lblGrayBar.Top += dy;
                btnClose.Top += dy;
                btnDetails.Top += dy;
                Height += dy;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnDetails_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            List<String> list = new List<String>();
            for (Exception ecur = exc; ecur != null; ecur = ecur.InnerException)
            {
                String text = "";
                if (ecur != exc)
                    text = "Source error: " + ecur.Message + "\r\n";

                text += exc.StackTrace;

                list.Add(text);
            }

            txtDetails.Text = String.Join("\r\n\r\n", list);
            txtDetails.Select(0, 0);
            txtDetails.Visible = true;
            btnDetails.Visible = false;
            ChangeHeight(txtDetails.Height);
        }

    }
}
