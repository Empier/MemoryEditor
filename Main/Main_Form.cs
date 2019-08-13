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

        private int scan_status=0;
        private void timUpdate_Tick(object sender, EventArgs e)
        {
            unsafe
            {

                void* buffer = (byte*)Memory.Alloc(8);

                try
                {
                    listViewEx1.BeginUpdate();

                    foreach (ListViewItem item in listViewEx1.Items)
                    {

                        ushort size = 0;
                        switch (item.SubItems[3].Text)
                        {
                            case "Byte":
                                size = 1;
                                break;
                            case "4 Bytes":
                                size = 4;
                                break;

                        }

                        if (item.Checked == true)
                        {
                            *(uint*)buffer = Convert.ToUInt32(item.SubItems[2].Text, 10);
                            WPM((UInt64)PID, Convert.ToUInt32(item.SubItems[1].Text, 16), size, (IntPtr)buffer);
                            continue;
                        }
                        RPM((UInt64)PID, Convert.ToUInt32(item.SubItems[1].Text, 16), size, (IntPtr)buffer);
                        item.SubItems[2].Text = (*(int*)buffer).ToString();
                    }
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {
                    listViewEx1.EndUpdate();
                    Memory.Free(buffer);
                }
            }

        }
        private Timer timUpdate;

        private void Main_Form_Load(object sender, EventArgs e)
        {

            init();
            AllocConsole();

            ComboboxItem item = new ComboboxItem();
            item.Text = "Byte";
            item.Value = 2;

            comboBox1.Items.Add(item);


            item = new ComboboxItem();
            item.Text = "4 Bytes";
            item.Value = 1;

            comboBox1.Items.Add(item);

            
            item = new ComboboxItem();
            item.Text = "String";
            item.Value = 3;

            comboBox1.Items.Add(item);

            item = new ComboboxItem();
            item.Text = "Array of bytes";
            item.Value = 4;

            comboBox1.Items.Add(item);

            comboBox1.SelectedIndex = 0;



            
           
            timUpdate = new Timer();
            timUpdate.Interval = 150;
            timUpdate.Tick += new EventHandler(timUpdate_Tick);
            timUpdate.Start();


        }

        private void button1_Click(object sender, EventArgs e)
        {
            Process_Form processForm = new Process_Form();
            var k = processForm.ShowDialog(this);
            if (k == DialogResult.OK)
            {
                PID = processForm.PID;
                label4.Text = processForm.PNAME;

                button2.Enabled = true;
                textBox1.Enabled = true;
                comboBox1.Enabled = true;

            }
            //SuspendLayout();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Value_Type = Convert.ToInt32((comboBox1.SelectedItem as ComboboxItem).Value);
            Value_Type_Text = (comboBox1.SelectedItem as ComboboxItem).Value.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (scan_status == 0)
            {
                scan_status = 1;
                listView1.Items.Clear();

                unsafe
                {
                    void* buffer = (byte*)Memory.Alloc(1024 * 16);

                    {
                        UInt64 startmemory = Convert.ToUInt64(textBox2.Text, 16);
                        UInt64 endmemory = Convert.ToUInt64(textBox3.Text, 16);
                        UInt32 scanvalue = 0;
                        int Value_Size = 0;
                        uint Scan_Size = 0;

                        switch (Value_Type)
                        {
                            case 1:
                                scanvalue = Convert.ToUInt32(textBox1.Text, 10);
                                Value_Size = 4;
                                Scan_Size = 1024 * 16;
                                break;
                            case 2:
                                scanvalue = Convert.ToUInt32(textBox1.Text, 10);
                                Value_Size = 1;
                                Scan_Size = 1024 * 16;
                                break;

                        }

                        Console.WriteLine("Start");

                        uint[] saveaddr = new uint[30000];
                        string[] savevalue = new string[30000];
                        int idx = 0;

                        for (UInt32 off = 0; (startmemory + off - Convert.ToUInt32(Scan_Size)) < endmemory; off += Scan_Size)
                        {
                            RPM((UInt64)PID, startmemory + off, (UInt16)Scan_Size, (IntPtr)buffer);

                            switch (Value_Type)
                            {
                                case 1: //4byte
                                    for (int i = 0; i < (1024 * 16) / 4; i++)
                                    {
                                        if (scanvalue == *(UInt32*)((int*)buffer + i))
                                        {
                                            if (idx >= 30000)
                                            {
                                                idx++;
                                                continue;
                                            }
                                            saveaddr[idx] = Convert.ToUInt32(startmemory + off + ((uint)i * 4));
                                            savevalue[idx] = scanvalue.ToString();

                                            idx++;
                                        }
                                    }
                                    break;
                                case 2:
                                    for (int i = 0; i < (1024 * 16); i++)
                                    {
                                        if (scanvalue == *(byte*)((byte*)buffer + i))
                                        {
                                            if (idx >= 30000)
                                            {
                                                idx++;
                                                continue;
                                            }
                                            saveaddr[idx] = Convert.ToUInt32(startmemory + off + (uint)i);
                                            savevalue[idx] = scanvalue.ToString();
                                            idx++;
                                        }
                                    }
                                    break;

                                case 3:
                                    byte* ptr = (byte*)buffer;
                                    int j = 0;

                                    for (int i = 0; i < textBox1.Text.Length; i++)
                                    {

                                        if (Convert.ToInt32(textBox1.Text[i]) == Convert.ToInt32(((byte*)ptr)[i]))
                                        {
                                            j++;
                                            if (j == textBox1.Text.Length)
                                            {
                                                saveaddr[idx] = Convert.ToUInt32(startmemory + off);
                                                savevalue[idx] = textBox1.Text;
                                                idx++;
                                                if (idx >= 10000)
                                                {
                                                    MessageBox.Show("너무 많이 검색되서 중단함");
                                                    endmemory = 0;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }

                                    break;
                                case 4:
                                    break;
                            }
                        }

                        int x = 0;

                        label6.Text = "Found: " + idx.ToString();
                        if (idx >= 30000)
                        {
                            MessageBox.Show("너무 많이 검색되서 일부는 리스트에 출력되지않음.");
                            idx = 30000;
                        }
                        listView1.BeginUpdate();

                        var items = new ListViewItem[idx];
                        for (; x < idx; x++)
                        {
                            items[x] = new ListViewItem();
                            items[x].Text = string.Format("{0:X8}", saveaddr[x]);
                            items[x].SubItems.Add(savevalue[x]);
                            items[x].ForeColor = Color.Green;
                            items[x].SubItems[1].ForeColor = Color.Red;
                            items[x].UseItemStyleForSubItems = false;
                        }

                        listView1.Items.AddRange(items);
                        listView1.EndUpdate();
                        Console.WriteLine("End");
                        Memory.Free(buffer);

                        button2.Text = "New Read";
                        button3.Enabled = true;
                        comboBox1.Enabled = false;
                        textBox2.Enabled = false;
                        textBox3.Enabled = false;
                    }

                }
            }
            else
            {
                scan_status = 0;
                button2.Text = "First Read";
                button3.Enabled = false;
                comboBox1.Enabled = true;
                textBox2.Enabled = true;
                textBox3.Enabled = true;
                listView1.Items.Clear();
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
                    try
                    {
                        listView1.BeginUpdate();

                        foreach (ListViewItem item in listView1.Items)
                        {

                            ushort size = 0;
                            switch (comboBox1.Text)
                            {
                                case "Byte":
                                    size = 1;
                                    break;
                                case "4 Bytes":
                                    size = 4;
                                    break;

                            }
                            //Console.WriteLine(Convert.ToUInt32(item.SubItems[0].Text, 16));
                            *buffer = 0;
                            RPM((UInt64)PID, Convert.ToUInt32(item.SubItems[0].Text, 16), size, (IntPtr)buffer);
                            
                            if(*buffer != Convert.ToUInt32(textBox1.Text,10))
                            {
                                listView1.Items.Remove(item);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        throw (ex);
                    }
                    finally
                    {
                        label6.Text = "Found: " + listView1.Items.Count.ToString();

                        listView1.EndUpdate();
                        Memory.Free(buffer);
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
                string type = null;
                switch(Value_Type)
                {
                    case 1:
                        type = "4 Bytes";
                        break;
                    case 2:
                        type = "Byte";
                        break;
                }

                string[] row = { "", addr, value,type , "No Description" };

                listViewEx1.Items.Add(new ListViewItem(row));
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

        private bool aaa = false;
        private void listView2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
      
            ListViewHitTestInfo hitTest = listViewEx1.HitTest(e.X,e.Y);
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

                        ushort size = 0;
                        switch (listViewEx1.FocusedItem.SubItems[3].Text)
                        {
                            case "Byte":
                                size = 1;
                                break;
                            case "4 Bytes":
                                size = 4;
                                break;

                        }
                        WPM((UInt64)PID, Convert.ToUInt64(listViewEx1.FocusedItem.SubItems[1].Text,16), size, (IntPtr)buffer);
                        
                        Memory.Free(buffer);
                    }
                }
            }
            else if (columnIndex == 4)
            {
                NewDesc_Form newdescForm = new NewDesc_Form();
                DialogResult k = newdescForm.ShowDialog(this);
                if (k == DialogResult.OK)
                {
                    listViewEx1.FocusedItem.SubItems[4].Text = newdescForm.VALUE;
                }
            }




        }

        private void listView2_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if(aaa!=true)
                e.NewValue = e.CurrentValue;
        }

        

        private void listView2_MouseClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo hitTest = listViewEx1.HitTest(e.X, e.Y);
            int columnIndex = hitTest.Item.SubItems.IndexOf(hitTest.SubItem);
            aaa = false;
            if (columnIndex == 0)
            {
                aaa = true;
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
