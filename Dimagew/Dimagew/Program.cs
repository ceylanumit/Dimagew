using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dimagew
{
    internal static class Program
    {
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (Environment.OSVersion.Version.Major >= 6) { SetProcessDPIAware(); } // Windows Vista ve üzeri olduğunda çalışır.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}
