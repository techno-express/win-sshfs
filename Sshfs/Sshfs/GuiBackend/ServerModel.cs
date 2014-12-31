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
        public String Name { get; set; }
        public Guid ServerID{get; set;}

        public String Notes{get; set;}

        public String Host{get; set;}
        public int Port{get; set;}

        public string PrivateKey;
        public string Password { get; set; }
        public string Passphrase { get; set; }
        public string Username { get; set; }

        public ConnectionType ConnectionType {get; set; }
        public DriveStatus Status { get; set; }

        public String Root;
        public String Mountpoint;

        //Folders for a Server kann be stored as List of Foldermodels
        // or as List of Strings
        //List<FolderModel> Folders= new List<FolderModel>();
        List<string> Folders = new List<string>();

#endregion

    }
}
