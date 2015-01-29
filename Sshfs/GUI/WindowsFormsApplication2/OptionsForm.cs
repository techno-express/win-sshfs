using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;

namespace GUI_WindowsForms
{

    /// this Form is a combination of the about-window and the Option-Window
    public partial class OptionsForm : Form
    {
        public OptionsForm()
        {
            InitializeComponent();
            Loglevel.SelectedIndex = 0;// Debugging ist beim Start der Form ausgewählt
            writeAvailableDrivesInCombo();

        }
       
        private void Loglevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void virtualdriveletter_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Utilities.GetAvailableDrives() schaut nach den verfügbaren drive letters 
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        /// open the link by clicking the hyperlink
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Specify that the link was visited.
            this.linkLabel1.LinkVisited = true;

            // Navigate to the URL.
            System.Diagnostics.Process.Start("https://github.com/thb42/win-sshfs");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void checkBox_startup_CheckedChanged_1(object sender, EventArgs e)
        {     
            RegisterInStartup(checkBox_startup.Checked);
        }



        ///  starts the program at windows startup by creating a registry key
        private void RegisterInStartup(bool isChecked)
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey
                    ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (isChecked)
            {
                registryKey.SetValue("FiSSH", Application.ExecutablePath);
            }
            else
            {
                registryKey.DeleteValue("FiSSH");
            }
        }

        private static bool IsDriveAvailable(char letter)
        {
            List<char> not_available = new List<char>();
            not_available.Add('a');
            not_available.Add('A');
            not_available.Add('b');
            not_available.Add('B');

            if (not_available.Contains(letter) ||
                Directory.GetLogicalDrives().Contains(((letter).ToString() + @":\")))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void writeAvailableDrivesInCombo()
        {
            this.virtualdriveletter.Items.Clear();
            for (int i = 'Z'; i >= 'A'; i--)
            {
                if (IsDriveAvailable((char)i) == true) this.virtualdriveletter.Items.Add((char)i + ":");
            }
        }
    }
}
