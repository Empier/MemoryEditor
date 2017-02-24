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
    public partial class Form1 : Form
    {
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

        


        [DllImport("msvcrt40.dll")]
        public static extern int printf(string format, __arglist);

       


        // [DllImport("Empire.dll", EntryPoint = "test")]
        // static extern void test();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate bool RPM(UInt64 pid, UInt64 startaddress, UInt16 bytestoread, IntPtr r);
        delegate bool WPM(UInt64 pid, UInt64 startaddress, UInt16 bytestowrite, IntPtr w);

        delegate void MyDllFunc();
        // private delegate void MyDllFunc();















        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AllocConsole();
            //printf("%d", __arglist(1234));
            //
            //Console.WriteLine(Convert.ToUInt64(textBox2.Text,16));

            ComboboxItem item = new ComboboxItem();
            item.Text = "4 Bytes";
            item.Value = 4;

            comboBox1.Items.Add(item);

            item = new ComboboxItem();
            item.Text = "1 Bytes";
            item.Value = 1;

            comboBox1.Items.Add(item);

            comboBox1.SelectedIndex=0;

            //comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;

            
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Process_Form settingsForm = new Process_Form();
            var k = settingsForm.ShowDialog(this);
            PID = settingsForm.PID;

            //SuspendLayout();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Value_Type = Convert.ToInt32((comboBox1.SelectedItem as ComboboxItem).Value);
            Value_Type_Text = (comboBox1.SelectedItem as ComboboxItem).Value.ToString();
            //Console.WriteLine(Value_Type.ToString());

        }

        private void button2_Click(object sender, EventArgs e)
        {
            // MessageBox.Show(comboBox1.SelectedValue.ToString());
            // BOOL ReadProcessMemory(UINT64 pid, UINT64 startaddress, WORD bytestoread, DWORD *r)
            //BOOL WriteProcessMemory(UINT64 pid, UINT64 startaddress, WORD bytestowrite)
            //IntPtr r=InsufficientMemo
            unsafe
            {
                //progressBar1.PerformStep();
                void* buffer = (byte*)Memory.Alloc(512);
                

                //   try
                {
                    IntPtr hModule = LoadLibrary("Empire.dll");
                    IntPtr RPMaddr = GetProcAddress((int)hModule, "RPM");
                    RPM ReadProcessMemory = (RPM)Marshal.GetDelegateForFunctionPointer(RPMaddr, typeof(RPM));
                    IntPtr WPMaddr = GetProcAddress((int)hModule, "WPM");
                    WPM WriteProcessMemory = (WPM)Marshal.GetDelegateForFunctionPointer(WPMaddr, typeof(WPM));



                    ReadProcessMemory((UInt64)PID, Convert.ToUInt64(textBox2.Text, 16), (UInt16)Value_Type, (IntPtr)buffer);

                    //MessageBox.Show(buffer );
                    MessageBox.Show((*(UInt32*)buffer).ToString());
                    //WriteProcessMemory((UInt64)PID, 0x00400000, 3);

                    //MessageBox.Show(buffer[0]

                }
                //    catch
                {


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
                byte* buffer = (byte*)Memory.Alloc(512);


                //   try
                {
                    IntPtr hModule = LoadLibrary("Empire.dll");
                    IntPtr RPMaddr = GetProcAddress((int)hModule, "RPM");
                    RPM ReadProcessMemory = (RPM)Marshal.GetDelegateForFunctionPointer(RPMaddr, typeof(RPM));
                    IntPtr WPMaddr = GetProcAddress((int)hModule, "WPM");
                    WPM WriteProcessMemory = (WPM)Marshal.GetDelegateForFunctionPointer(WPMaddr, typeof(WPM));

                   // Memory.memcpy(buffer, , 4);

                   // Memory.Copy((void*)String.Format("{0:X08}", Convert.ToInt32(textBox1.Text, 10)), (void*)buffer, 4);

                   //String.Format("{0:X08}", Convert.ToInt32(textBox1.Text, 10))

                    //Console.WriteLine(String.Format("{0:X08}", Convert.ToInt32(textBox1.Text, 10)));

                    *(UInt32 *)buffer = Convert.ToUInt32(textBox1.Text, 10);
                    //String.Format("{0:00000000}",Convert.ToInt32(textBox1.Text, 10).ToString("X"))
                    WriteProcessMemory((UInt64)PID, Convert.ToUInt64(textBox2.Text, 16), (UInt16)Value_Type, (IntPtr)buffer);

                    //MessageBox.Show(buffer );
                    //MessageBox.Show((*(UInt32*)buffer).ToString());
                    //WriteProcessMemory((UInt64)PID, 0x00400000, 3);

                    //MessageBox.Show(buffer[0]

                }
                //    catch
                {


                }

            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            
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
        public static extern void *memcpy(void* dest, string src, UInt32 count);

        [DllImport("msvcrt40.dll")]
        public static extern void *memset(void* dest, void* src, UInt32 count);

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
