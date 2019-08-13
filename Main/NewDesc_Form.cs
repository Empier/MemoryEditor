using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Main
{
    public partial class NewDesc_Form : Form
    {
        public string VALUE { get; set; }
        public NewDesc_Form()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.VALUE = textBox1.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Enter(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.button1_Click(sender, e);
            }
            else
            {

            }
        }
    }
}
