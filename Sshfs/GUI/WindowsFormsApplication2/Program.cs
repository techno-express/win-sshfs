using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sshfs.GuiBackend.Remoteable;
using System.Diagnostics;

namespace GUI_WindowsForms
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(String[] args)
        {
            //if argument for no gui start is given, set this false
            bool StartGUI = true;

            //start ServiceHost
            ProcessStartInfo startEXE = new ProcessStartInfo();
            startEXE.FileName = "FiSSHBone.exe";
            startEXE.WorkingDirectory = "";
            startEXE.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startEXE.ErrorDialog = true;

            if (Process.Start(startEXE) != null && StartGUI)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new FiSSHForm());
            }
        }
    }
}
