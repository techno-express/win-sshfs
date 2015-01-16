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
        public Boolean use_global_login;
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

        public System.Windows.Forms.TreeNode gui_node = null;

    }
}
