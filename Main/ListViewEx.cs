using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Main
{
    public class ListViewEx : ListView
    {

        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        static extern Boolean SetWindowTheme(IntPtr hWindow, String subAppName, String subIDList);

        public ListViewEx()
        {
            SetStyle((ControlStyles)0x22010, true);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // 핸들이 생성된 후에 테마를 적용한다.
            SetWindowTheme(Handle, "explorer", null);
        }
    }
}
