using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sshfs.GuiBackend
{
    class FolderModel
    {
        public Boolean use_global_login;
        /*Guid FolderID*/
        public string Folder;
        public string Mountpoint;
        public string Username;
        public string Passphrase;
    }
}
