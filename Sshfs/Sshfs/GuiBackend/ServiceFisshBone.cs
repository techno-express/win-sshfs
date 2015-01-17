using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Sshfs;
using System.Configuration;
using Sshfs.GuiBackend.Remoteable;
using System.Xml;
using System.IO;

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

        ///Saving LServermodel into an XML file
        /**
         * SaveServerList() gets a string with the path, where to save the file connections.xml
         * It saves the LServermodel to this XML file. 
         * Password, Passphrase and Key are not saved.
         * 
         * @param String Savepath-Path where the XML file will be saved
         * 
         */
        private void SaveServerlist(String Savepath)
        {

            XmlDocument doc = new XmlDocument();
            XmlNode Serverlist, Server, Folderlist, Folder;
            XmlDeclaration XmlDec;
            
            Serverlist = doc.CreateElement("Serverlist");
            Server = doc.CreateElement("Server");
            Folderlist = doc.CreateElement("Folderlist");
            Folder = doc.CreateElement("Folder");

            XmlDec = doc.CreateXmlDeclaration("1.0", null, null);

            foreach (ServerModel element in LServermodel)
            {
                Server.AppendChild(doc.CreateElement("Name")).InnerText = element.Name;

                Server.AppendChild(doc.CreateElement("ServerID")).InnerText = element.ID.ToString();

                Server.AppendChild(doc.CreateElement("Notes")).InnerText = element.Notes;

                Server.AppendChild(doc.CreateElement("PrivateKey")).InnerText = "Not Saved";

                Server.AppendChild(doc.CreateElement("Password")).InnerText = "Not Saved";

                Server.AppendChild(doc.CreateElement("Passphrase")).InnerText = "Not Saved";

                Server.AppendChild(doc.CreateElement("Username")).InnerText = element.Username;

                Server.AppendChild(doc.CreateElement("Host")).InnerText = element.Host;

                Server.AppendChild(doc.CreateElement("Port")).InnerText = element.Port.ToString();

                foreach(FolderModel FElement in element.Folders)
                {
                    Folder.AppendChild(doc.CreateElement("Global Login")).InnerText = FElement.use_global_login.ToString();
                    Folder.AppendChild(doc.CreateElement("FolderID")).InnerText = FElement.ID.ToString();
                    Folder.AppendChild(doc.CreateElement("Name")).InnerText = FElement.Name;
                    Folder.AppendChild(doc.CreateElement("Note")).InnerText = FElement.Note;
                    Folder.AppendChild(doc.CreateElement("Folder")).InnerText = FElement.Folder;
                    Folder.AppendChild(doc.CreateElement("Letter")).InnerText = FElement.Letter.ToString();
                    Folder.AppendChild(doc.CreateElement("Username")).InnerText = FElement.Username;
                    Folder.AppendChild(doc.CreateElement("Password")).InnerText = "Not Saved";
                    Folder.AppendChild(doc.CreateElement("Passphrase")).InnerText = "Not Saved";
                    Folder.AppendChild(doc.CreateElement("Private Key")).InnerText = "Not Saved";
                    //Folder.AppendChild(doc.CreateElement("Drive Status")).InnerText = DriveStatus.Unmounted.ToString();

                    Folderlist.AppendChild(Folder);
                }
                Server.AppendChild(Folderlist);
                Serverlist.AppendChild(Server);
            }

            
            doc.AppendChild(Serverlist);
            doc.InsertBefore(XmlDec, Serverlist);
            try
            {

                if (Savepath.EndsWith(@"\"))
                {
                    Savepath.Remove(Savepath.Length - 1);
                }

                doc.Save(Savepath + @"\connections.xml");
            }
            catch
            {
                Log.writeLog(SimpleMind.Loglevel.Error, Comp, "Could save Serverlist.");
            }
        }

        ///Load Serverconfiguration from an XML file
        /**
         * LoadServerlist() gets a string with the path, where the file connections.xml is saved.
         * It loads the configuration saved in the XML file into the Serverlist.
         * Passwords, Keys etc. are not loaded.
         * 
         * @param String Savepath-Path where the XML file is saved
         * 
         */

        private void LoadServerlist(String Savepath)
        {
            if (Savepath.EndsWith(@"\"))
            {
                Savepath.Remove(Savepath.Length - 1);
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(Savepath + @"\connections.xml");

            XmlNodeList sn = doc.DocumentElement.SelectNodes("Serverlist/Server");
            XmlNodeList fn = doc.DocumentElement.SelectNodes("Serverlist/Server/Folderlist/Folder");
            List<ServerModel> SL = new List<ServerModel>();
            List<FolderModel> FL = new List<FolderModel>();

             foreach(XmlNode Snode  in sn)
             {
                 ServerModel Server = new ServerModel();

                 Server.Name = Snode.SelectSingleNode("Name").InnerText;
                 try
                 {
                     Server.ID = new Guid(Snode.SelectSingleNode("ServerID").InnerText);
                 }
                 catch(Exception e)
                 {
                     Log.writeLog(SimpleMind.Loglevel.Warning, Comp, e.Message);
                     Log.writeLog(SimpleMind.Loglevel.Debug, Comp, "Generate new Guid.");
                     Server.ID = Guid.NewGuid();
                 }

                 Server.Notes = Snode.SelectSingleNode("Notes").InnerText;
                 Server.PrivateKey = "";
                 Server.Password = "";
                 Server.Passphrase = "";
                 Server.Username = Snode.SelectSingleNode("Username").InnerText;
                 Server.Host = Snode.SelectSingleNode("Host").InnerText;
                 try
                 {
                     Server.Port = Convert.ToInt32(Snode.SelectSingleNode("Port").InnerText);
                 }

                 catch(Exception e) {
                     Log.writeLog(SimpleMind.Loglevel.Error, Comp, e.Message);
                     Server.Port = 22;
                 }

                 fn = Snode.SelectSingleNode("Serverlist").SelectNodes("Folder");

                 foreach(XmlNode Fnode in fn)
                 {
                     FolderModel Folder = new FolderModel();

                     Folder.Name = Fnode.SelectSingleNode("Name").InnerText;
                     try
                     {
                         Folder.ID = new Guid(Fnode.SelectSingleNode("FolderID").InnerText);
                     }
                     catch (Exception e)
                     {
                         Log.writeLog(SimpleMind.Loglevel.Warning, Comp, e.Message);
                         Log.writeLog(SimpleMind.Loglevel.Debug, Comp, "Generate new Guid");
                         Folder.ID = Guid.NewGuid();
                     }
                     Folder.Note = Fnode.SelectSingleNode("Note").InnerText;
                     Folder.Folder = Fnode.SelectSingleNode("Folder").InnerText;
                     try
                     {
                         Folder.Letter = Convert.ToChar(Fnode.SelectSingleNode("Letter").InnerText);
                     }
                     catch(Exception e)
                     {
                         Log.writeLog(SimpleMind.Loglevel.Error, Comp, e.Message);
                         Folder.Letter = 'x';
                     }

                     try
                     {
                         Folder.use_global_login = Convert.ToBoolean(Fnode.SelectSingleNode("Global Login").InnerText);
                     }
                     catch
                     {
                         Folder.use_global_login = true;
                     }

                     if(!Folder.use_global_login)
                     {
                         Folder.Username = Fnode.SelectSingleNode("Username").InnerText;
                         Folder.Password = "";
                         Folder.Passphrase = "";
                         Folder.PrivatKey = "";
                     }

                     Folder.Status = DriveStatus.Unmounted;

                     Server.Folders.Add(Folder);
                 }

                 LServermodel.Add(Server);
             }
                


        }

        private void MountDrive(ServerModel server, FolderModel folder)
        {
            Log.writeLog(SimpleMind.Loglevel.Debug, Comp, "Client tries to mount folder \"" + folder.ID + "\" mounted on server \"" + server.ID + "\"");
            SftpDrive drive = new SftpDrive();

            LSftpDrive[new Tuple<Guid, Guid>(server.ID, folder.ID)] = drive;

            drive.Host = server.Host;
            drive.Port = server.Port;

            drive.Letter = folder.Letter;
            drive.Root = folder.Folder;

            if (folder.use_global_login)
            {
                drive.Username = server.Username;
                drive.Password = server.Password;
            }
            else
            {
                drive.Username = folder.Username;
                drive.Password = folder.Password;
            }

            drive.Mount();
            Log.writeLog(SimpleMind.Loglevel.Debug, Comp, "folder \"" + folder.ID + "\" mounted on server \"" + server.ID + "\"");
                
        }
        
        /// get a drive to a proper drive letter
        /**
         * This method gets a letter and looks this letter up in LSftpDrive.
         * If there is a mounted drive with the given letter this method will
         * return the drive's ids.
         * If there is no such drive it will return empty ids (Guid.Empty)
         * 
         * @param letter    drive letter aou want to search for
         * 
         * @return ids of found server and folder or empty ids
         */
        public Tuple<Guid, Guid> GetLetterUsage(char letter)
        {
            foreach (KeyValuePair<Tuple<Guid, Guid>, SftpDrive> i in LSftpDrive)
            {
                if (i.Value.Letter == letter && i.Value.Status == DriveStatus.Mounted)
                {
                    return i.Key;
                }
                else
                {
                    continue;
                }
            }
            return new Tuple<Guid, Guid>(Guid.Empty, Guid.Empty);
        }

        /// mount a drive which is not in database
        /**
         * This methods creats a drive from the given server and its first folder.
         * 
         * @param server a ServerModel with at least one FolderModel
         */
        public void UnregisteredMount(ServerModel server)
        {
            try
            {
                FolderModel folder;
                server.ID = Guid.NewGuid();
                folder = server.Folders.ElementAt(0);
                folder.ID = Guid.NewGuid();
                Log.writeLog(SimpleMind.Loglevel.Debug, Comp, "Client tries to mount unregistered folder \"" + folder.Folder + "\" mounted on server \"" + server.Host + "\"" + " on Port " + server.Port
                                                               + " with username " + server.Username + " and password " + server.Password);
                MountDrive(server, folder);
                return;
            }
            catch (Exception e)
            {
                Log.writeLog(SimpleMind.Loglevel.Debug, Comp, e.Message);
                throw new FaultException<Fault>(new Fault(e.Message));
            }
        }


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

                MountDrive(server, folder);

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
                Log.writeLog(SimpleMind.Loglevel.Debug, "Backend:getStatus", "Asked for status of " + FolderID + " on " + ServerID + " which is allready in LSftpDrive with status " + status.ToString());
                return status;
            }
            catch (NullReferenceException e)
            {
                Log.writeLog(SimpleMind.Loglevel.Debug, "Backend:getStatus", "Asked for status of " + FolderID + " on " + ServerID + " which is not in LSftpDrive");
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
