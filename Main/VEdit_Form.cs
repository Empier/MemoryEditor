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

    
    public partial class VEdit_Form : Form
    {

        public int VALUE { get; set; }


        public VEdit_Form()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.VALUE = Convert.ToInt32(textBox1.Text);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
