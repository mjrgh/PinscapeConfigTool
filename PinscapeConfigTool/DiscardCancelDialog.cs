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
    public partial class DiscardCancelDialog : Form
    {
        // Show the dialog and return the result:
        //
        //    Discard -> OK (proceed with the operation that triggered the warning)
        //    Cancel  -> Cancel (abort the operation so that the user can go back and save work)
        //
        // If "buttons" is non-null, it overrides the standard button labels.  Specify the
        // new labels in the form "&Discard|Cancel".
        public static DialogResult Show(String msg, String buttons)
        {
            DiscardCancelDialog dlg = new DiscardCancelDialog(msg, buttons);
            dlg.ShowDialog();
            return dlg.DialogResult;
        }

        public DiscardCancelDialog(String msg, String buttons)
        {
            InitializeComponent();
            txtMessage.Text = msg;
            if (buttons != null)
            {
                String[] b = buttons.Split('|');
                btnDiscard.Text = b[0];
                btnCancel.Text = b[1];
            }
        }

        private void btnDiscard_Click(object sender, EventArgs e)
        {

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {

        }

        private void DiscardCancelDialog_Shown(object sender, EventArgs e)
        {
            using (Graphics g = CreateGraphics())
            {
                SizeF size = g.MeasureString(txtMessage.Text, txtMessage.Font, txtMessage.Width);
                int dy = (int)(size.Height - txtMessage.Height);
                int dx = -(int)(size.Width - txtMessage.Width);
                if (dy > 0)
                {
                    txtMessage.Height += dy;
                    lblGrayBar.Top += dy;
                    btnDiscard.Top += dy;
                    btnCancel.Top += dy;
                    ActiveForm.Height += dy;
                }
                if (dx > 20)
                {
                    int margin = ActiveForm.Width - btnCancel.Right;
                    if (dx > btnDiscard.Left - margin)
                        dx = btnDiscard.Left - margin;
                    txtMessage.Width -= dx;
                    btnDiscard.Left -= dx;
                    btnCancel.Left -= dx;
                    ActiveForm.Width -= dx;
                }
            }
        }
    }
}
