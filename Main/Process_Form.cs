using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
namespace Main
{
    public partial class Process_Form : Form
    {
        Process[] allProc;
        public int PID { get; set; }
        public string PNAME { get; set; }
        //public string ReturnValue2 { get; set; }


        public Process_Form()
        {
            InitializeComponent();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
          
           // Process[] allProc = Process.GetProcesses();
            
           // listBox1.Items.Add(((TextBox)sender).Text);
            //listBox1.Items.Add("1234");
            //MessageBox.Show("asdf");
        }

        private void Process_Form_Load(object sender, EventArgs e)
        {
            allProc = Process.GetProcesses();
            foreach(Process p in allProc)
            {
                string k = p.Id.ToString() + "-" + p.ProcessName;
                listBox1.Items.Add(k);
            }
            //this.DialogResult =  ;
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Console.WriteLine(allProc[listBox1.SelectedIndex].Id);
            /*
            foreach (Process p in allProc)
            {
                string k = p.Id.ToString() + "-" + p.ProcessName;
                if(k== listBox1.SelectedItem.ToString())
                {
                    //Console.WriteLine(p.Id);
                    this.PID = p.Id;
                    this.Close();
                    break;
                }
            }
             */
            this.PID = allProc[listBox1.SelectedIndex].Id;
            this.PNAME = allProc[listBox1.SelectedIndex].ProcessName;
            this.Close();
            
        }
    }
}
