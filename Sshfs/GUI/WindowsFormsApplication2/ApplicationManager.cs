using Microsoft.VisualBasic.ApplicationServices;


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
            (MainForm as FiSSHForm).ReShow();
        }
    }
}
