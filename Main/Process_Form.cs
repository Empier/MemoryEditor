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


        private void Process_Form_Load(object sender, EventArgs e)
        {
            allProc = Process.GetProcesses();
          

            ImageList Imagelist = new ImageList();

            foreach (Process process in allProc)
            {
                string[] row = {
                    process.ProcessName,
                    process.Id.ToString()
                };
  
                try
                {
                    Imagelist.Images.Add(
                        process.Id.ToString(),       
                        Icon.ExtractAssociatedIcon(process.MainModule.FileName).ToBitmap()
                    );
                }
                catch { }

                 ListViewItem item = new ListViewItem(row)
                {
                     ImageIndex = Imagelist.Images.IndexOfKey(process.Id.ToString())
                };
                listView1.Items.Add(item);
            }
            
            listView1.LargeImageList = Imagelist;
            listView1.SmallImageList = Imagelist;


            listView1.Select();
        }
 

        private void button1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                int index = listView1.Items.IndexOf(listView1.SelectedItems[0]);
                this.PID = allProc[index].Id;
                this.PNAME = allProc[index].ProcessName;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Get Process Error");
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

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
