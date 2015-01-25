using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sshfs.GuiBackend
{
    /// a class to generate Folder Objects
    public class FolderModel
    {
        public bool use_global_login;
        public Guid ID;
        public string Name;
        public string Note;
        public string Folder;
        public char Letter;
        public string Username;
        public string Password;
        public string Passphrase;
        public string PrivatKey;
        public DriveStatus Status;
        public ConnectionType Type;
        public string VirtualDriveFolder;
        public bool use_virtual_drive; 

        public System.Windows.Forms.TreeNode gui_node = null;

    }
}
