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
namespace Main
{
    public partial class Main_Form : Form
    {
        public Main_Form()
        {
            InitializeComponent();
        }

        public int PID { get; set; }
        public string Value_Type_Text { get; set; }
        public int Value_Type { get; set; }
        [DllImport("kernel32.dll", EntryPoint = "LoadLibrary")]
        static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpLibFileName);

        [DllImport("kernel32.dll", EntryPoint = "GetProcAddress")]
        static extern IntPtr GetProcAddress(int hModule, [MarshalAs(UnmanagedType.LPStr)] string lpProcName);

        [DllImport("kernel32.dll", EntryPoint = "FreeLibrary")]
        static extern bool FreeLibrary(int hModule);

        [DllImport("kernel32.dll", EntryPoint = "AllocConsole")]
        static extern void AllocConsole();


        [DllImport("Empire.dll", EntryPoint = "init")]
        static extern void init();
        [DllImport("Empire.dll", EntryPoint = "RPM")]
        static extern void RPM(UInt64 pid, UInt64 startaddress, UInt16 bytestoread, IntPtr r);
        [DllImport("Empire.dll", EntryPoint = "WPM")]
        static extern void WPM(UInt64 pid, UInt64 startaddress, UInt16 bytestowrite, IntPtr w);

        /*
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate bool RPM(UInt64 pid, UInt64 startaddress, UInt16 bytestoread, IntPtr r);
        delegate bool WPM(UInt64 pid, UInt64 startaddress, UInt16 bytestowrite, IntPtr w);
        */




        private void Main_Form_Load(object sender, EventArgs e)
        {
            init();
            AllocConsole();
            
            //printf("%d", __arglist(1234));
            //
            //Console.WriteLine(Convert.ToUInt64(textBox2.Text,16));

            ComboboxItem item = new ComboboxItem();
            item.Text = "4 Bytes";
            item.Value = 1;

            comboBox1.Items.Add(item);

            item = new ComboboxItem();
            item.Text = "1 Bytes";
            item.Value = 2;

            comboBox1.Items.Add(item);

            item = new ComboboxItem();
            item.Text = "String";
            item.Value = 3;

            comboBox1.Items.Add(item);

            comboBox1.SelectedIndex = 0;

            //comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;

            /*
            listView1.View = View.Details;
            listView1.GridLines = true;
            listView1.FullRowSelect = true;
            listView1.Columns.Add("Address", 80);
            listView1.Columns.Add("Value", 70);
            */
            
            //listView1.Columns.Add("Quantity", 70);




            

            // listView1.View = View.Details;
            // listView1.GridLines = true;
            // listView1.FullRowSelect = true;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Process_Form processForm = new Process_Form();
            var k = processForm.ShowDialog(this);
            PID = processForm.PID;
            label4.Text = processForm.PNAME;
            //SuspendLayout();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Value_Type = Convert.ToInt32((comboBox1.SelectedItem as ComboboxItem).Value);
            Value_Type_Text = (comboBox1.SelectedItem as ComboboxItem).Value.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {

            listView1.Items.Clear();
            unsafe
            {
                void* buffer = (byte*)Memory.Alloc(100);
                

                //   try
                {
                    //IntPtr hModule = LoadLibrary("Empire.dll");
                    //IntPtr RPMaddr = GetProcAddress((int)hModule, "RPM");
                    //RPM ReadProcessMemory = (RPM)Marshal.GetDelegateForFunctionPointer(RPMaddr, typeof(RPM));
                    //IntPtr WPMaddr = GetProcAddress((int)hModule, "WPM");
                    //WPM WriteProcessMemory = (WPM)Marshal.GetDelegateForFunctionPointer(WPMaddr, typeof(WPM));

                    UInt64 startmemory = Convert.ToUInt64(textBox2.Text, 16);
                    UInt64 endmemory = Convert.ToUInt64(textBox3.Text, 16);
                    UInt32 scanvalue = 0;
                    int Value_Size=0;

                    if (Value_Type==1)
                    {
                        scanvalue=Convert.ToUInt32(textBox1.Text, 10);
                        Value_Size = 4;
                    }
                    else if(Value_Type==2)
                    {
                        scanvalue=Convert.ToUInt32(textBox1.Text, 10);
                        Value_Size = 1;
                    }
                    else if(Value_Type==3)
                    {

                        Value_Size = textBox1.Text.Length;
                        
                    }
                    Console.WriteLine("Start");
                    for (UInt32 off = 0; (startmemory + off) < endmemory; off += 1)
                    {
                        RPM((UInt64)PID, startmemory + off, (UInt16)Value_Size, (IntPtr)buffer);
                        //Console.WriteLine((*(UInt32*)buffer).ToString()+" "+scanvalue);

                        if (Value_Type != 3)
                        {
                           

                            if (scanvalue == *(UInt32*)buffer)
                            {
                                string[] row = { (startmemory + off).ToString("X"), scanvalue.ToString() };


                                var listViewItem = new ListViewItem(row);


                                listView1.Items.Add(listViewItem);

                            }
                        }
                        else /*string*/
                        {
                            byte *ptr = (byte *)buffer ;
                            int j = 0;
                            // Console.WriteLine(Convert.ToString((*ptr).ToString())
                            //     );

                            for (int i = 0; i < textBox1.Text.Length; i++)
                            {
                                
                                if (Convert.ToInt32(textBox1.Text[i])==Convert.ToInt32(((byte*)ptr)[i]))
                                {
                                    j++;
                                    //Console.WriteLine((startmemory+off).ToString("X")+" "+j + " " + textBox1.Text.Length);
                                    if(j== textBox1.Text.Length)
                                    {
                                        string[] row = { (startmemory + off).ToString("X"), textBox1.Text };

                                        var listViewItem = new ListViewItem(row);

                                        listView1.Items.Add(listViewItem);
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }

                           

                        }
                    }
                    Console.WriteLine("End");
                    Memory.Free(buffer); 
                }
               
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            unsafe
            {
                
                //progressBar1.PerformStep();
                byte* buffer = (byte*)Memory.Alloc(100);
                {
                   foreach(ListViewItem item in listView1.Items)
                    {
                        Console.WriteLine(item.Text);
                    }
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            

        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            Settings_Form settingsForm = new Settings_Form();
            var k = settingsForm.ShowDialog(this);
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

      

      

        private void ListView1_Click_Event(object sender, EventArgs e)
        {

            if (listView1.SelectedItems.Count == 1)
            {
                ListView.SelectedListViewItemCollection items = listView1.SelectedItems;
                ListViewItem lvItem = items[0];
                string addr = lvItem.SubItems[0].Text;
                string value = lvItem.SubItems[1].Text;
                string[] row = { "", addr, value, "de", "No Description" };

                listView2.Items.Add(new ListViewItem(row));
            }


        }

        private void listView2_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            
            //e.
            /*
            if (listView2.SelectedItems.Count == 1)
            {
                ListView.SelectedListViewItemCollection items = listView2.SelectedItems;
                ListViewItem lvItem = items[0];
                string addr = lvItem.SubItems[1].Text;
                // string value = lvItem.SubItems[1].Text;
                // string[] row = { "", addr, value, "de", "No Description" };
                Console.WriteLine(addr);
                //listView2.Items.Add(new ListViewItem(row));
            }
            */
        }

        private void listView2_ColumnReordered(object sender, ColumnReorderedEventArgs e)
        {
            //MessageBox.Show("BBB");
        }

        private void listView2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
      
            ListViewHitTestInfo hitTest = listView2.HitTest(e.X,e.Y);
            int columnIndex = hitTest.Item.SubItems.IndexOf(hitTest.SubItem);
            if(columnIndex == 2)
            {
                VEdit_Form veditForm = new VEdit_Form();
                DialogResult k = veditForm.ShowDialog(this);
                if(k==DialogResult.OK)
                {
                    unsafe
                    {
                        byte* buffer = (byte*)Memory.Alloc(16);
                        *(UInt32*)buffer = (UInt32)veditForm.VALUE;
                        
                        WPM((UInt64)PID, Convert.ToUInt64(listView2.FocusedItem.SubItems[1].Text,16), (UInt16)Value_Type, (IntPtr)buffer);
                        
                        Memory.Free(buffer);
                    }
                }
            }
      
      
        }
    }


    public unsafe class Memory
    {

        static int ph = GetProcessHeap();

        private Memory() { }

        public static void* Alloc(int size)
        {
            void* result = HeapAlloc(ph, HEAP_ZERO_MEMORY, size);
            if (result == null) throw new OutOfMemoryException();
            return result;
        }

        public static void Copy(void* src, void* dst, int count)
        {
            byte* ps = (byte*)src;
            byte* pd = (byte*)dst;
            if (ps > pd)
            {
                for (; count != 0; count--) *pd++ = *ps++;
            }
            else if (ps < pd)
            {
                for (ps += count, pd += count; count != 0; count--) *--pd = *--ps;
            }
        }

        public static void Free(void* block)
        {
            if (!HeapFree(ph, 0, block)) throw new InvalidOperationException();
        }

        public static void* ReAlloc(void* block, int size)
        {
            void* result = HeapReAlloc(ph, HEAP_ZERO_MEMORY, block, size);
            if (result == null) throw new OutOfMemoryException();
            return result;
        }

        public static int SizeOf(void* block)
        {
            int result = HeapSize(ph, 0, block);
            if (result == -1) throw new InvalidOperationException();
            return result;
        }

        const int HEAP_ZERO_MEMORY = 0x00000008;
        // Heap API functions
        [DllImport("kernel32")]
        static extern int GetProcessHeap();
        [DllImport("kernel32")]
        static extern void* HeapAlloc(int hHeap, int flags, int size);
        [DllImport("kernel32")]
        static extern bool HeapFree(int hHeap, int flags, void* block);
        [DllImport("kernel32")]
        static extern void* HeapReAlloc(int hHeap, int flags,
           void* block, int size);
        [DllImport("kernel32")]
        static extern int HeapSize(int hHeap, int flags, void* block);

        [DllImport("msvcrt40.dll")]
        public static extern void* memcpy(void* dest, string src, UInt32 count);

        [DllImport("msvcrt40.dll")]
        public static extern void* memset(void* dest, void* src, UInt32 count);

    }

    public class ComboboxItem
    {
        public string Text { get; set; }
        public object Value { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }

}
