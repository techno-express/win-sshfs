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
        public String Name;
        public Guid ServerID;

        public String Notes;

        public String Host;
        public int Port{get; set;};

        public string PrivateKey;
        public string Password { get; set; }
        public string Passphrase { get; set; }
        public string Username { get; set; }

        public ConnectionType ConnectionType {get; set; }

        //Folders for a Server kann be stored as List of Foldermodels
        // or as List of Strings
        List<FolderModel> Folders= new List<FolderModel>();
        /*List<string> Folders = new List<string>();*/

#endregion

    }
}
