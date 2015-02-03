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

            IServiceFisshBone bone_server = IPCConnection.ClientConnect();
            //start ServiceHost if it is not running
            try
            {
                bone_server.listAll();
            }
            catch
            {
                ProcessStartInfo startEXE = new ProcessStartInfo();
                startEXE.FileName = "FiSSHBone.exe";
                startEXE.WorkingDirectory = "";
                startEXE.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startEXE.ErrorDialog = true;
                if (Process.Start(startEXE) == null)
                {
                    Environment.Exit(1);
                }
            }

            // Loop: wait until IPC server is online
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    bone_server.listAll();
                }
                catch
                {
                    System.Threading.Thread.Sleep(100);
                }
            }


            if (StartGUI)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new FiSSHForm());
            }
    }
    }
}
