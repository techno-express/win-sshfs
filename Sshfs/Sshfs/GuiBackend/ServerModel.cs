using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sshfs.GuiBackend
{
    /// a class to generate Server Objects, that contains a list of Folder Objects.
    public class ServerModel
    {
#region ATTRIBUTES
        public string Name { get; set; }
        public Guid ID{get; set;}

        public string Notes{get; set;}

        public string PrivateKey { get; set; }
        public string Password { get; set; }
        public string Passphrase { get; set; }

        public string Username { get; set; }

        public string Host { get; set; }
        public int Port { get; set; }

        public ConnectionType Type;

        public System.Windows.Forms.TreeNode gui_node = null;

        //public bool Automount { get; set; }

        //public ConnectionType ConnectionType {get; set; }
        //public DriveStatus Status { get; set; }

        //public string Root;
        //public char DriveLetter;

        //Folders for a Server can be stored as List of Foldermodels
        // or as List of Strings
        //public List<FolderModel> Mountpoints = new List<FolderModel>();
        //public List<string> Mountpoint;
        public List<FolderModel>Folders;// = new Dictionary<Guid, FolderModel>();
#endregion
        public ServerModel()
        {
//            Mountpoints = new List<FolderModel>();
            Folders = new List<FolderModel>();
        }

        public ServerModel(ServerModel S)
        { 
            Name = S.Name;
            ID = S.ID;
            Notes = S.Notes;
            PrivateKey = S.PrivateKey;
            Password = S.Password;
            Passphrase = S.Passphrase;
            Username = S.Username;
            Host = S.Host;
            Port = S.Port;
            Type = S.Type;

            List<FolderModel>Folders = new List<FolderModel>();

            foreach (FolderModel F in S.Folders)
            {
                Folders.Add(new FolderModel(F));
            }
        
        }


        /// creates a new copy of a ServerModel object
        /**
         * DuplicateServer creates an copy of the object.
         * The new object has differnt ID and Name as the original.
         * The folderlist is copied as well. The ID's and names are
         * different, too. The Folder.Status is unmounted.
         * 
         * 
         * @return new ServerModel object
         * 
         */
        public ServerModel DuplicateServer()
        {
            ServerModel r = new ServerModel(this);
            r.ID = Guid.NewGuid();
            r.Name += " Copy";

            foreach (FolderModel F in r.Folders)
            {
                F.Name += " Copy";
                F.ID = Guid.NewGuid();
                F.Status = DriveStatus.Unmounted;
            }

            return r;
        }

    }
}
