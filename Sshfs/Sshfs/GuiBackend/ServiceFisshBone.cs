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






        /// create a connection to a directory on a server
        /**
         *  Mount() gets IDs of a Server and Folder as parameters.
         *  Further information like IP, port, account an folderlocation are pulled from
         *  Servermodel-List. It creats a new SftpDrive with these information and 
         *  execute the drive's mount() method which will start the sshfs connection 
         *  and offers the remote folder as a local driveletter.
         *  
         *  Finaly the drive will be stored in LSftpDrive dictionary
         *  so other methods can access them. 
         * 
         * 
         *  Mount() should also be able to mount a Virtual Drive but it is not
         *  implemented yet.
         * 
         * 
         * @param Guid ServerID,Guid FolderID-distinct identification of directory
         * 
         * 
         */
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


        /// disconnects the client from directory on a server
        /** 
         * UMount() gets IDs of server and folder of a connection.
         * It searches for a proper dirve in LSftpDrive and disconnect it by
         * executing its unmount method.
         * 
         * 
         * UMount() should also deal with virtual drive. (not implemented)
         * 
         * @param Guid ServerID,Guid FolderID-distinct identification of directory
         * 
         */
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

        /// method to find out which status the connection has
        /**
         * getStatus() gets IDs of server and folder. It looks them up in the
         * LSftpDrive dictionary to get a proper drive 
         * and return the drive's status (mounted, unmounted, etc.)
         * If there is not proper drive it will return "unmounted.
         * 
         * The return type is an enum named DriveStatus which is also use by SFtpDrive
         * 
         * getStatus() should also deal with virtual drive. (not implemented)
         * 
         * 
         * @param Guid ServerID,Guid FolderID-distinct identification of directory
         * 
         * @return Mounted or Unmounted as enum DriveStatus 
         */
        public DriveStatus getStatus(Guid ServerID, Guid FolderID)
        {
            try
            {
                DriveStatus status = LSftpDrive[new Tuple<Guid, Guid>(ServerID, FolderID)].Status;
                Console.WriteLine(status.ToString());
                return status;
            }
            catch (NullReferenceException e)
            {
                return DriveStatus.Unmounted;
            }
            catch
            {
                // :::FIXME:::
                return DriveStatus.Unmounted;
            }
       }
    

        /// sending a copy of the datamodell to clients
        /** 
         * listAll() return the ServerModell list.
         * Serialisation will be handled by WCF.
         * Before sending the data listAll() updates
         * the status attribute of all FolderModells.
         * 
         * @return LServermodel, a list of all servers currently known
         * 
         */
        public List<ServerModel> listAll()
        {
            // Update status
            foreach(ServerModel i in LServermodel)
            {
                foreach(FolderModel j in i.Folders)
                {
                    j.Status = getStatus(i.ID, j.ID);
                }
            }
            //return new List<ServerModel>(LServermodel);
            return LServermodel;
        }
       
 
        /// overwrites the server properties
        /**
         * editServer() gets a ServerModel from client.
         * It will search for a server with same id in
         * ServerModel list.
         * If it has found one every attribute of it will be set
         * with the new content from given parameter.
         * 
         * 
         * @param ServerModel with some new attributes and an existing id
         */
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


        /// add a new server to Servermodel list
        /**
         * addServer() gets a new ServerModel from client.
         * It creates a new id for this new server
         * and adds it to the ServerModel list.
         * The new id will be returned to sender.
         * 
         * This method should check the attributes of new server
         * before adding it to the list. (not implmented)
         * 
         * @param newServer     ServerModel which should be added to list
         * @return created serverID for new servermodel
         */
        public  Guid addServer(ServerModel newServer)
        {
            
            newServer.ID = Guid.NewGuid();

            if(newServer.ID == Guid.Empty)
            {
                return addServer(newServer);
            }

            else
            {
                LServermodel.Add(newServer);
                return newServer.ID;
            }
        }


        /// generate a folderID for a new folder
        /**
         * addFolder() gets a new FolderModel and a existing server id.
         * It will create a new id for the new folder and adds it to
         * its server in ServerModel list. The server is given by 
         * its id.
         * 
         * @param ServerID  ID of server where the folder should be added to
         * @param Folder    FolderModel which should be added
         * 
         * @return          Created ID of added folder
         * 
         */
        public Guid addFolder(Guid ServerID, FolderModel Folder)
        {
            ServerModel server = LServermodel.Find(x => x.ID == ServerID);

            if (server == null)
            {
                string message = "editServer() got unkown server id";
                Log.writeLog(SimpleMind.Loglevel.Error , Comp, message);
                throw new FaultException<Fault>(new Fault(message));
            }

            Folder.ID = Guid.NewGuid();
            if (Folder.ID == Guid.Empty)
            {
                addFolder(ServerID, Folder);
            }
            else
            {
                server.Folders.Add(Folder);
            }
            return Folder.ID;
        }
        

        /// removes fodler from a server
        /**
         * removeFolder() gets id of server and folder.
         * It searches for the given server by id in ServerModel list.
         * Afterwards it searches for the given folder in
         * this ServerModel and removes it.
         * 
         * 
         * @param Guid ServerID,Guid FolderID-distinct identification of directory
         * 
         * @return Index of the server or in error case -1
         * 
         */
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

        /// removes server from ServerModel list
        /**
         * removeServer() gets a id of a server.
         * It searches for the server given by id 
         * and removes it from the list.
         * 
         * @param ServerID  id of server you want to remove
         * 
         */
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


        
    }
}
