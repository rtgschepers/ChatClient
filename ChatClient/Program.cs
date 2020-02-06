using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Windows.Forms;

namespace ChatClient
{
    static class Program
    {
        public static Form frmLogin = new frmLogin();
        public static TcpClient client;
        public static NetworkStream stream;
        public static string username;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(frmLogin);
            if (Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location)).Count() > 1) 
                Process.GetCurrentProcess().Kill();
        }
    }
}
