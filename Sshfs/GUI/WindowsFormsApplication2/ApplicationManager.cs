using Microsoft.VisualBasic.ApplicationServices;
using System.Windows.Forms;


namespace GUI_WindowsForms
{
    /// Class to start just one instance
    internal class ApplicationManager : WindowsFormsApplicationBase
    {
        public ApplicationManager()
        {
            IsSingleInstance = true;
            EnableVisualStyles = true;
        }

        protected override void OnCreateMainForm()
        {
            MainForm = new FiSSHForm();
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            if(MainForm.WindowState == FormWindowState.Normal)
            {
                MessageBox.Show("FiSSH is allready running.");
            }
            else
            {
                (MainForm as FiSSHForm).ReShow();
            }
        }
    }
}
