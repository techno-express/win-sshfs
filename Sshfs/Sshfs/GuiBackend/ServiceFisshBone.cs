using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Sshfs;
using System.Configuration;
using Sshfs.GuiBackend.Remoteable;


namespace Sshfs.GuiBackend.IPCChannelRemoting
{
    public class ServiceFisshBone : IServiceFisshBone 
    {
        const String Comp = "Backend";

        private static SimpleMind.SimpleMind Log = new SimpleMind.SimpleMind();

        private static List<ServerModel> LServermodel = new List<ServerModel>();
        //private List<SftpDrive> LSftpDrive = new List<SftpDrive>();
        private static Dictionary<Tuple<Guid, Guid>, SftpDrive> LSftpDrive = new Dictionary<Tuple<Guid,Guid>, SftpDrive>(); //erste Guid vom Server, zweite des Folder
        private static List<VirtualDrive> LVirtualDrive = new List<VirtualDrive>();
        public static int x;


        public ServiceFisshBone() { }

        // internal Method to Find Servermodel by Guid and printing Errorlog
        private int Find(Guid ID, String ErrMsg)
        {
            int Index = -1;
            try{
                Index = LServermodel.FindIndex(x => x.ID == ID);
            }

            catch(ArgumentNullException e)
            {
                Log.writeLog(SimpleMind.Loglevel.Error, Comp, ErrMsg);
                Index = -1;
            }
           
            return Index;
        }

        private SftpDrive createSftp(ServerModel Server)
        {
            //int Index = Find(ID, "Couldn't find server to create drive");
            //ServerModel tempServer;
            
            SftpDrive drive = null; 
            /*
            if(Server != null)
            {
                //tempServer = LServermodel.ElementAt(Index);
                drive.Name = Server.Name;
                drive.Port = Server.Port;
                drive.Root = Server.Root;
                //FIXME
                drive.Letter = Server.DriveLetter;
                drive.MountPoint = Server.Folders[0].Folder;
            }
            */
            return drive;
        }

#region INTERFACE
/*        public ServerModel get_server_by_id(Guid ID){

            ServerModel Server;
            int Index = Find(ID, "Couldn't find ServerID: " + ID.ToString());
            

            if (Index == -1)
            {
                Server = null;
            }

            else
            {
                Server = LServermodel.ElementAt(Index);
            }

            return Server;       
        }*/


        public IServiceTools.error_codes Mount(Guid ServerID, Guid FolderID)
        {
            FolderModel folder;
            ServerModel server;
            SftpDrive drive = new SftpDrive();
            
            try {
                server = LServermodel.Find(x => x.ID == ServerID);
                if (server == null)
                {
                    Log.writeLog(SimpleMind.Loglevel.Error, Comp, "Mount() got unkown server id");
                    return IServiceTools.error_codes.error_impossible;
                }

                folder = server.Folders.Find(x => x.ID == FolderID);
                if (folder == null)
                {
                    Log.writeLog(SimpleMind.Loglevel.Error, Comp, "Mount() got unkown folder id");
                    return IServiceTools.error_codes.error_impossible;
                }

 
                LSftpDrive[new Tuple<Guid, Guid>(ServerID, FolderID)] = drive;
                
                drive.Host = server.Host;
                drive.Port = server.Port;
                
                drive.Letter = folder.Letter;
                drive.Root = folder.Folder;

                if(folder.use_global_login)
                {
                    drive.Username = server.Username;
                    drive.Password = server.Password;
                }
                else{
                    drive.Username = folder.Username;
                    drive.Password = folder.Password;
                }

                drive.Mount();
                Log.writeLog(SimpleMind.Loglevel.Debug , Comp, "folder \"" + FolderID +"\" mounted on server \"" + ServerID + "\"");
                
                return IServiceTools.error_codes.no_error;
            }
            catch(Exception e) {
                    Log.writeLog(SimpleMind.Loglevel.Debug , Comp, e.Message);
                    return IServiceTools.error_codes.any_error;
            }
        }


        public IServiceTools.error_codes UMount(Guid ServerID, Guid FolderID)
        {
            try
            {
                LSftpDrive[new Tuple<Guid, Guid>(ServerID, FolderID)].Unmount();
                Log.writeLog(SimpleMind.Loglevel.Debug , Comp, "folder \"" + FolderID +"\" unmounted on server \"" + ServerID + "\"");
                return IServiceTools.error_codes.no_error;
            }
            catch(Exception e) {
                    Log.writeLog(SimpleMind.Loglevel.Debug , Comp, e.Message);
                    return IServiceTools.error_codes.any_error;
            }
        }


        public DriveStatus getStatus(Guid ID)
        {

            DriveStatus DS = DriveStatus.Undefined;
            /*
            int Index = Find(ID, "Could not find ServerID to get Drivestatus");

            if (Index >= 0)
            {
                DS = LServermodel[Index].Status;
            }

            else
            {
                DS = DriveStatus.Error;
            }
            */
            return DS;
        }


        public List<ServerModel> listAll()
        {
            //return new List<ServerModel>(LServermodel);
            return LServermodel;
        }
       
 
        public IServiceTools.error_codes editServer(ServerModel Server)
        {
            ServerModel local_server_reference = LServermodel.Find(x => x.ID == Server.ID);

            if (local_server_reference == null)
            {
                Log.writeLog(SimpleMind.Loglevel.Error, Comp, "editServer() got unkown server id");
                return IServiceTools.error_codes.error_impossible;
            }

            local_server_reference.Name = Server.Name;
            local_server_reference.Password = Server.Password;
            local_server_reference.Passphrase = Server.Passphrase;
            local_server_reference.Notes = Server.Notes;
            local_server_reference.PrivateKey = Server.PrivateKey;
            local_server_reference.Username = Server.Username;
            local_server_reference.ID = Server.ID;
            local_server_reference.Host = Server.Host;
            local_server_reference.Port = Server.Port;
                
            Log.writeLog(SimpleMind.Loglevel.Debug, Comp, "server " + Server.ID + " has been edit");
            return IServiceTools.error_codes.no_error;
       }


        public  Guid addServer(ServerModel newServer)
        {
            
            Guid ReturnValue;

            newServer.ID = Guid.NewGuid();

            if(newServer.ID == Guid.Empty)
            {
                Log.writeLog(SimpleMind.Loglevel.Error, Comp, "Error while creat ServerID");
                ReturnValue = new Guid("0");
            }

            else
            {
                LServermodel.Add(newServer);
                ReturnValue = newServer.ID;
            }

            return ReturnValue;
        }


        public Guid addFolder(Guid ServerID, FolderModel Mountpoint)
        {
            ServerModel server = LServermodel.Find(x => x.ID == ServerID);

            if (server == null)
            {
                Log.writeLog(SimpleMind.Loglevel.Error, Comp, "editServer() got unkown server id");
                return Guid.Empty;
                //return IServiceTools.error_codes.error_impossible;
            }

            Mountpoint.ID = Guid.NewGuid();
            server.Folders.Add(Mountpoint);

            return Mountpoint.ID;
        }

        public IServiceTools.error_codes removeFolder(Guid ServerID, Guid FolderID)
        {
            ServerModel server;
            FolderModel folder;

            server = LServermodel.Find(x => x.ID == ServerID);
            if (server == null)
            {
                Log.writeLog(SimpleMind.Loglevel.Error, Comp, "removeFolder() got unkown server id");
                return IServiceTools.error_codes.error_impossible;
            }

            folder = server.Folders.Find(x => x.ID == FolderID);
            if (folder == null)
            {
                Log.writeLog(SimpleMind.Loglevel.Error, Comp, "removeFolder() got unkown folder id");
                return IServiceTools.error_codes.error_impossible;
            }

            server.Folders.Remove(folder);

            return IServiceTools.error_codes.no_error;
        }

        // Returnvalue is -1 in error case or else the remove index
        public IServiceTools.error_codes removeServer(Guid ServerID)
        {
            ServerModel server;
            FolderModel folder;

            server = LServermodel.Find(x => x.ID == ServerID);
            if (server == null)
            {
                Log.writeLog(SimpleMind.Loglevel.Error, Comp, "removeFolder() got unkown server id");
                return IServiceTools.error_codes.error_impossible;
            }

            LServermodel.Remove(server);

            return IServiceTools.error_codes.no_error;
       }


        public SimpleMind.Loglevel setLogLevel(SimpleMind.Loglevel newLogLevel)
        {
            Log.setLogLevel((int)newLogLevel);
            return (SimpleMind.Loglevel) Log.getLogLevel();
        }
#endregion
        
    }
}
