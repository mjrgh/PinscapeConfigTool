using CollectionUtils;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PinscapeConfigTool
{
    public partial class AdviceDialog : Form
    {
        public static void Show(String dialogName, String message)
        {
            // the options key is the dialog name prefixed with "AdviceDialog." 
            String optionKey = "AdviceDialog." + dialogName;

            // show the dialog only if the user hasn't previously checked
            // the box to hide it
            if (Program.options.ValueOrDefault(optionKey) != "Hide")
            {
                AdviceDialog dlg = new AdviceDialog(optionKey, message);
                dlg.ShowDialog();
                dlg.Dispose();
            }
        }

        // Restore all dialogs marked as hidden in the options
        public static void RestoreAll()
        {
            // set all of the option keys starting with our prefix to "Show"
            Program.options.Where(kv => kv.Key.StartsWith("AdviceDialog.")).ToList().ForEach(
                kv => Program.options[kv.Key] = "Show");
        }

        public AdviceDialog(String optionKey, String message)
        {
            InitializeComponent();
            this.OptionKey = optionKey;
            txtMessage.Text = message;
        }

        // program option key for remembering to hide this box in the future
        String OptionKey;

        private void btnOK_Click(object sender, EventArgs e)
        {
            // if the "don't show again" box is checked, note the option change
            if (ckHide.Checked)
                Program.options[OptionKey] = "Hide";

            // close the form
            ActiveForm.Close();
        }

        private void AdviceDialog_Shown(object sender, EventArgs e)
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
                    ckHide.Top += dy;
                    btnOK.Top += dy;
                    ActiveForm.Height += dy;
                }
                if (dx > 20)
                {
                    int margin = ActiveForm.Width - btnOK.Right;
                    if (dx > ckHide.Left - margin)
                        dx = ckHide.Left - margin;
                    txtMessage.Width -= dx;
                    ckHide.Left -= dx;
                    btnOK.Left -= dx;
                    ActiveForm.Width -= dx;
                }
            }
        }
    }
}
