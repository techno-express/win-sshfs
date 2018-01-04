﻿using System;
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
        public string PrivateKey;
        public DriveStatus Status;
        public ConnectionType Type;
        public string VirtualDriveFolder;
        public bool use_virtual_drive;
        public bool Automount;


        public System.Windows.Forms.TreeNode gui_node = null;

    

        public FolderModel()
        {}

        public FolderModel(FolderModel F)
        {
            this.Set(F);
        }

        public void Set(FolderModel F)
        {
            use_global_login = F.use_global_login;
            ID = F.ID;
            Name = F.Name;
            Note = F.Note;
            Folder = F.Folder;
            Letter = F.Letter;
            Username = F.Username;
            Password = F.Password;
            Passphrase = F.Passphrase;
            PrivateKey = F.PrivateKey;
            Status = F.Status;
            Type = F.Type;
            VirtualDriveFolder = F.VirtualDriveFolder;
            use_virtual_drive = F.use_virtual_drive;
            Automount = F.Automount;
        }
        /// creates a new copy of a FolderModel object
        /**
         * DuplicateFolder creates an copy of the object.
         * The new object has differnt ID and Name as the original.
         * The Folder.Status is unmounted.
         * 
         * 
         * @return new FolderModel object
         * 
         */
        public FolderModel DuplicateFolder()
        {
            FolderModel r = (FolderModel)this.MemberwiseClone();
            r.ID = Guid.NewGuid();
            r.Status = DriveStatus.Unmounted;
            return r;
        }

    }
}
