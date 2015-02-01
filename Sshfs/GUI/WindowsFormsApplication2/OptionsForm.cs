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

using Sshfs.GuiBackend.Remoteable;

namespace GUI_WindowsForms
{

    /// this Form is a combination of the about-window and the Option-Window
    public partial class OptionsForm : Form
    {
        IServiceFisshBone bone_server;

        public OptionsForm()
        {
            bone_server = IPCConnection.ClientConnect();
            InitializeComponent();
            writeAvailableDrivesInCombo();

            // Look into registry for autostart and set check box
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey
                    ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if(registryKey.GetValue("FiSSH") == null)
            {
                checkBox_startup.Checked = false;
            }
            else
            {
                checkBox_startup.Checked = true;
            }

            checkBox3.Checked = bone_server.IsReconnectAfterWakeUpSet();
            Loglevel.SelectedIndex = Loglevel.Items.IndexOf(bone_server.getLogLevel());

            char VirtualDriveLetter = bone_server.GetVirtualDriveLetter();
            virtualdriveletter.Items.Add(VirtualDriveLetter + ":");
            virtualdriveletter.SelectedIndex = virtualdriveletter.Items.IndexOf(VirtualDriveLetter + ":");

        }
       
        private void Loglevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            bone_server.setLogLevel( (SimpleMind.Loglevel) Loglevel.SelectedItem );
        }

        private void virtualdriveletter_DropDownClosed(object sender, EventArgs e)
        {
            char c = virtualdriveletter.SelectedItem.ToString().ToCharArray()[0];
            if (bone_server.GetVirtualDriveLetter() != c)
            {
                bone_server.SetVirtualDriveLetter(c);
            }
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

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            bone_server.SetReconnectAfterWakeUp(checkBox3.Checked);
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
