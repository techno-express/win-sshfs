using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sshfs.GuiBackend
{
    class ServerModel
    {
#region ATTRIBUTES
        public string Name { get; set; }
        public Guid ServerID{get; set;}

        public string Notes{get; set;}

        public string PrivateKey { get; set; }
        public string Password { get; set; }
        public string Passphrase { get; set; }

        public string Username { get; set; }

        public string Host { get; set; }
        public int Port { get; set; }

        public bool Automount { get; set; }

        public ConnectionType ConnectionType {get; set; }
        public DriveStatus Status { get; private set; }

        public string Root;
        public char DriveLetter;

        //Folders for a Server kann be stored as List of Foldermodels
        // or as List of Strings
        //List<FolderModel> Folders= new List<FolderModel>();
        public List<string> Mountpoint;

        ServerModel()
        {
            Mountpoint = new List<string>();
        }



#endregion

    }
}
