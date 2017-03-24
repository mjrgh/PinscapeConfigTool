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
    public partial class TextViewer : Form
    {
        public TextViewer()
        {
            InitializeComponent();
        }

        public void SetText(String text)
        {
            richTextBox1.Text = text;
        }

        private void TextViewer_Load(object sender, EventArgs e)
        {
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Copy();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.SelectAll();
        }
    }
}
