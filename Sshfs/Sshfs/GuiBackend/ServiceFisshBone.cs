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


        public void Mount(Guid ServerID, Guid FolderID)
        {
            FolderModel folder;
            ServerModel server;
            SftpDrive drive = new SftpDrive();
            
            try {
                server = LServermodel.Find(x => x.ID == ServerID);
                if (server == null)
                {
                    string message = "Mount() got unkown server id";
                    Log.writeLog(SimpleMind.Loglevel.Error, Comp, message);
                    throw new FaultException<Fault>(new Fault(message));
                }

                folder = server.Folders.Find(x => x.ID == FolderID);
                if (folder == null)
                {
                    string message = "Mount() got unkown folder id";
                    Log.writeLog(SimpleMind.Loglevel.Error, Comp, message);
                    throw new FaultException<Fault>(new Fault(message));
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
                
                return;
            }
            catch(Exception e) {
                Log.writeLog(SimpleMind.Loglevel.Debug , Comp, e.Message);
                throw new FaultException<Fault>(new Fault(e.Message));
            }
        }


        public void UMount(Guid ServerID, Guid FolderID)
        {
            try
            {
                LSftpDrive[new Tuple<Guid, Guid>(ServerID, FolderID)].Unmount();
                Log.writeLog(SimpleMind.Loglevel.Debug , Comp, "folder \"" + FolderID +"\" unmounted on server \"" + ServerID + "\"");
                return;
            }
            catch(Exception e) {
                Log.writeLog(SimpleMind.Loglevel.Error , Comp, e.Message);
                throw new FaultException<Fault>(new Fault(e.Message));
            }
        }


/*        public DriveStatus getStatus(Guid ID)
        {

            DriveStatus DS = DriveStatus.Undefined;
            
            int Index = Find(ID, "Could not find ServerID to get Drivestatus");

            if (Index >= 0)
            {
                DS = LServermodel[Index].Status;
            }

            else
            {
                DS = DriveStatus.Error;
            }
            
            return DS;
        }
    */

        public List<ServerModel> listAll()
        {
            //return new List<ServerModel>(LServermodel);
            return LServermodel;
        }
       
 
        public void editServer(ServerModel Server)
        {
            ServerModel local_server_reference = LServermodel.Find(x => x.ID == Server.ID);

            if (local_server_reference == null)
            {
                string message = "editServer() got unkown server id";
                Log.writeLog(SimpleMind.Loglevel.Error , Comp, message);
                throw new FaultException<Fault>(new Fault(message));
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
            return;
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
                //return;
            }

            Mountpoint.ID = Guid.NewGuid();
            server.Folders.Add(Mountpoint);

            return Mountpoint.ID;
        }

        public void removeFolder(Guid ServerID, Guid FolderID)
        {
            ServerModel server;
            FolderModel folder;

            server = LServermodel.Find(x => x.ID == ServerID);
            if (server == null)
            {
                string message = "removeFolder() got unkown server id";
                Log.writeLog(SimpleMind.Loglevel.Error , Comp, message);
                throw new FaultException<Fault>(new Fault(message));
            }

            folder = server.Folders.Find(x => x.ID == FolderID);
            if (folder == null)
            {
                string message =  "removeFolder() got unkown folder id";
                Log.writeLog(SimpleMind.Loglevel.Error , Comp, message);
                throw new FaultException<Fault>(new Fault(message));
            }

            server.Folders.Remove(folder);

            return;
        }

        // Returnvalue is -1 in error case or else the remove index
        public void removeServer(Guid ServerID)
        {
            ServerModel server;

            server = LServermodel.Find(x => x.ID == ServerID);
            if (server == null)
            {
                string message = "removeFolder() got unkown server id";
                Log.writeLog(SimpleMind.Loglevel.Error , Comp, message);
                throw new FaultException<Fault>(new Fault(message));
            }

            LServermodel.Remove(server);

            return;
       }


        public SimpleMind.Loglevel setLogLevel(SimpleMind.Loglevel newLogLevel)
        {
            Log.setLogLevel((int)newLogLevel);
            return (SimpleMind.Loglevel) Log.getLogLevel();
        }
#endregion
        
    }
}
