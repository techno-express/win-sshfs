/*
Copyright (c) 2014 2015 thb42 bjoe-phi

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

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
// For reconnect after wakeup
using Microsoft.Win32;


namespace Sshfs.GuiBackend.IPCChannelRemoting
{
    public class ServiceFisshBone : IServiceFisshBone 
    {
        public static bool ShutMeDown = false;
        const string Comp = "Backend";
        private static string path_to_config_directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\FiSSH";

        private static SimpleMind.SimpleMind Log = new SimpleMind.SimpleMind();

        private static bool StartWithSystemstartFlag;
        private static bool ReconnectAfterWakeUpFlag;

        private static List<ServerModel> LServermodel = new List<ServerModel>();
        //private List<SftpDrive> LSftpDrive = new List<SftpDrive>();
        private static Dictionary<Tuple<Guid, Guid>, SftpDrive> LSftpDrive = new Dictionary<Tuple<Guid,Guid>, SftpDrive>(); //erste Guid vom Server, zweite des Folder
        private static Dictionary<Tuple<Guid, Guid>, SftpDrive> LSftpDriveWakeUp = new Dictionary<Tuple<Guid,Guid>, SftpDrive>(); //erste Guid vom Server, zweite des Folder
        //private static List<VirtualDrive> LVirtualDrive = new List<VirtualDrive>();
        private static VirtualDrive VirtualDrive = new VirtualDrive();
        

        public ServiceFisshBone() { }

        /// Initialize everything
        public static void Init()
        {
            // Wake up event handler
            SystemEvents.PowerModeChanged += WakeUpHandler;

            LoadConfiguration(path_to_config_directory);

            try
            {
                if (!IsDriveAvailable(VirtualDrive.Letter))
                {
                    VirtualDrive.Letter = GetFreeDriveLetter();
                }
                VirtualDrive.Mount();
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Cannot start Software. There is no free drive letter or you library is not installed: http://dokan-dev.net/en/download");
                Environment.Exit(1);
            }

            if(!Directory.Exists(path_to_config_directory))
            {
                Directory.CreateDirectory(path_to_config_directory);
            }
            LoadServerlist(path_to_config_directory);

            AutoMountAtStartUp();
        }

        /// Mount at Software startup
        private static void AutoMountAtStartUp()
        {
            try
            {
                foreach (ServerModel server in LServermodel)
                {
                    foreach (FolderModel folder in server.Folders)
                    {
                        if (folder.Automount)
                        {
                            try
                            {
                                MountDrive(server, folder);
                            }
                            catch { }
                        }
                    }
                }
            }
            catch { }
        }

        /// Handle reconnection after wake up
        private static void WakeUpHandler(object s, PowerModeChangedEventArgs e)
        {

            switch (e.Mode)
            {
                // When going to sleep
                case PowerModes.Suspend:
                    Log.writeLog(SimpleMind.Loglevel.Debug, Comp, "WakeUpHandler(): System is going to sleep.");


                    if (ReconnectAfterWakeUpFlag)
                    {
                        LSftpDriveGarbageCollection();
                        LSftpDriveWakeUp = LSftpDrive;
                    }
                    
                    foreach (KeyValuePair<Tuple<Guid, Guid>, SftpDrive> i in LSftpDrive)
                    {
                        UnmountDrive(i.Key.Item1, i.Key.Item2);
                        /*if (i.Value.Letter == ' ')
                        {
                            VirtualDrive.RemoveSubFS(i.Value);
                            i.Value.Unmount();
                        }
                        else
                        {
                            i.Value.Unmount();
                        }*/
                    }

                    LSftpDrive = new Dictionary<Tuple<Guid,Guid>,SftpDrive>();

                    break;

                // When waking up
                case PowerModes.Resume:
                    if (!ReconnectAfterWakeUpFlag) break;

                    Log.writeLog(SimpleMind.Loglevel.Debug, Comp, "WakeUpHandler(): System is waking up.");

                    int k = 0;
                    while(! System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                    {
                        System.Threading.Thread.Sleep(1000);
                        
                        k++;
                        if ( k > 60 )
                        {
                            return;
                        }

                    }

                    LSftpDrive = LSftpDriveWakeUp;
                    foreach (KeyValuePair<Tuple<Guid, Guid>, SftpDrive> i in LSftpDrive)
                    {
                        if (i.Value.Letter == ' ')
                        {

                            VirtualDrive.AddSubFS(i.Value);
                            LookIntoVirtualDrive(i.Value.MountPoint);
                            Log.writeLog(SimpleMind.Loglevel.Debug, Comp,
                                                "WakeUpHandler() reconnected virtual drive " + i.Value.MountPoint +
                                                ": folder " + i.Value.Root +
                                                " on server " + i.Value.Host + " with id ");// + i.Key.ToString());
                        }
                        else
                        {
                            i.Value.Mount();
                            Log.writeLog(SimpleMind.Loglevel.Debug, Comp,
                               "WakeUpHandler() reconnected folder " + i.Value.Root +
                               " on server " + i.Value.Host + " with id ");// + i.Key.ToString());
                        }
                        
                    }
                    break;
            }
        }

        /// Remove every unmounted drive
        private static void LSftpDriveGarbageCollection ()
         {
             foreach(KeyValuePair<Tuple<Guid, Guid>, SftpDrive> i in LSftpDrive) 
             {
                 // When drive is neither mounted nor mounting
                 if( i.Value.Status != DriveStatus.Mounted &&
                     i.Value.Status != DriveStatus.Mounting)
                 {
                     LSftpDrive.Remove(i.Key);
                     VirtualDrive.RemoveSubFS(i.Value);
                     Log.writeLog(SimpleMind.Loglevel.Debug, Comp,
                         "LSftpGarbageCollection() removed folder " + i.Value.Root +
                         " on server " + i.Value.Host + " with id " + i.Key.ToString());
                 }
             }
         }

        ///Saving LServermodel into an XML file
        /**
         * SaveServerList() gets a string with the path, where to save the file connections.xml
         * It saves the LServermodel to this XML file. 
         * Password, Passphrase and Key are not saved.
         * 
         * @param String Savepath-Path where the XML file will be saved
         * 
         */
        public void SaveServerlist(String Savepath)
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

                Server.AppendChild(doc.CreateElement("PrivateKey")).InnerText = element.PrivateKey;

                Server.AppendChild(doc.CreateElement("Password")).InnerText = element.Password;

                Server.AppendChild(doc.CreateElement("Passphrase")).InnerText = element.Passphrase;

                Server.AppendChild(doc.CreateElement("Username")).InnerText = element.Username;

                Server.AppendChild(doc.CreateElement("Host")).InnerText = element.Host;

                Server.AppendChild(doc.CreateElement("Port")).InnerText = element.Port.ToString();

                foreach(FolderModel FElement in element.Folders)
                {
                    Folder.AppendChild(doc.CreateElement("GlobalLogin")).InnerText = (FElement.use_global_login.ToString());
                    Folder.AppendChild(doc.CreateElement("Automount")).InnerText = (FElement.Automount.ToString());
                    Folder.AppendChild(doc.CreateElement("FolderID")).InnerText = FElement.ID.ToString();
                    Folder.AppendChild(doc.CreateElement("Name")).InnerText = FElement.Name;
                    Folder.AppendChild(doc.CreateElement("Note")).InnerText = FElement.Note;
                    Folder.AppendChild(doc.CreateElement("Folder")).InnerText = FElement.Folder;
                    Folder.AppendChild(doc.CreateElement("Letter")).InnerText = FElement.Letter.ToString();
                    Folder.AppendChild(doc.CreateElement("Username")).InnerText = FElement.Username;
                    Folder.AppendChild(doc.CreateElement("Password")).InnerText = FElement.Password;
                    Folder.AppendChild(doc.CreateElement("Passphrase")).InnerText = FElement.Passphrase;
                    Folder.AppendChild(doc.CreateElement("PrivateKey")).InnerText = FElement.PrivateKey;
                    Folder.AppendChild(doc.CreateElement("ConnectionType")).InnerText = FElement.Type.ToString();
                    Folder.AppendChild(doc.CreateElement("UseVirtualDrive")).InnerText = FElement.use_virtual_drive.ToString();
                    Folder.AppendChild(doc.CreateElement("VirtualDrive")).InnerText = FElement.VirtualDriveFolder;
                    //Folder.AppendChild(doc.CreateElement("Drive Status")).InnerText = DriveStatus.Unmounted.ToString();

                    Folderlist.AppendChild(Folder);
                    Folder = doc.CreateElement("Folder");
                }

                Server.AppendChild(Folderlist);
                Serverlist.AppendChild(Server);
                Server = doc.CreateElement("Server");
                Folderlist = doc.CreateElement("Folderlist");
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
            catch(Exception e)
            {
                Log.writeLog(SimpleMind.Loglevel.Error, Comp, "Could not save Serverlist.");
                Log.writeLog(SimpleMind.Loglevel.Debug, Comp, e.Message);
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

        private static void LoadServerlist(String Savepath)
        {
            bool error_on_load = false;
            XmlDocument doc = new XmlDocument();
            
            if (Savepath.EndsWith(@"\"))
            {
                Savepath.Remove(Savepath.Length - 1);
            }

            try
            {
                doc.Load(Savepath + @"\connections.xml");
            }

            catch(Exception e)
            {
                Log.writeLog(SimpleMind.Loglevel.Error, Comp, "Could not load connections!");
                Log.writeLog(SimpleMind.Loglevel.Debug, Comp, e.Message);
                error_on_load = true;
            }

            if (!error_on_load)
            {
                XmlNodeList sn = doc.DocumentElement.SelectNodes("Server");
                XmlNodeList fn;// = doc.DocumentElement.SelectNodes("Serverlist/Server/Folderlist/Folder");
                List<ServerModel> SL = new List<ServerModel>();
                List<FolderModel> FL = new List<FolderModel>();


                foreach (XmlNode Snode in sn)
                {
                    ServerModel Server = new ServerModel();

                    Server.Name = Snode.SelectSingleNode("Name").InnerText;
                    try
                    {
                        Server.ID = new Guid(Snode.SelectSingleNode("ServerID").InnerText);
                    }
                    catch (Exception e)
                    {
                        Log.writeLog(SimpleMind.Loglevel.Debug, Comp, e.Message);
                        Log.writeLog(SimpleMind.Loglevel.Warning, Comp, "Could not read Guid from XML file, generate new Guid.");
                        Server.ID = Guid.NewGuid();
                    }

                    Server.Notes = Snode.SelectSingleNode("Notes").InnerText;
                    Server.PrivateKey = Snode.SelectSingleNode("PrivateKey").InnerText;
                    Server.Password = Snode.SelectSingleNode("Password").InnerText;
                    Server.Passphrase = Snode.SelectSingleNode("Passphrase").InnerText;
                    Server.Username = Snode.SelectSingleNode("Username").InnerText;
                    Server.Host = Snode.SelectSingleNode("Host").InnerText;
                    try
                    {
                        Server.Port = Convert.ToInt32(Snode.SelectSingleNode("Port").InnerText);
                    }

                    catch (Exception e)
                    {
                        Log.writeLog(SimpleMind.Loglevel.Debug, Comp, e.Message);
                        Log.writeLog(SimpleMind.Loglevel.Warning, Comp, "Could not read port from XML file, set port to 22");
                        Server.Port = 22;
                    }

                    fn = Snode.SelectSingleNode("Folderlist").SelectNodes("Folder");

                    foreach (XmlNode Fnode in fn)
                    {
                        FolderModel Folder = new FolderModel();

                        Folder.Name = Fnode.SelectSingleNode("Name").InnerText;
                        try
                        {
                            Folder.ID = new Guid(Fnode.SelectSingleNode("FolderID").InnerText);
                        }
                        catch (Exception e)
                        {
                            Log.writeLog(SimpleMind.Loglevel.Debug, Comp, e.Message);
                            Log.writeLog(SimpleMind.Loglevel.Warning, Comp, "Generate new Guid");
                            Folder.ID = Guid.NewGuid();
                        }

                        try
                        {
                            Folder.Note = Fnode.SelectSingleNode("Note").InnerText;
                            Folder.Folder = Fnode.SelectSingleNode("Folder").InnerText;
                            Folder.use_virtual_drive = Convert.ToBoolean(Fnode.SelectSingleNode("UseVirtualDrive").InnerText);
                            Folder.Type = (ConnectionType)Enum.Parse(typeof(ConnectionType), Fnode.SelectSingleNode("ConnectionType").InnerText);
                            Folder.VirtualDriveFolder = Fnode.SelectSingleNode("VirtualDrive").InnerText;
                        }
                        catch { }

                        try
                        {
                            Folder.Letter = Convert.ToChar(Fnode.SelectSingleNode("Letter").InnerText);
                        }
                        catch (Exception e)
                        {
                            Log.writeLog(SimpleMind.Loglevel.Warning, Comp, "Could not read drive letter from XML file, set letter to x");
                            Log.writeLog(SimpleMind.Loglevel.Debug, Comp, e.Message);
                            Folder.Letter = 'x';
                        }

                        try
                        {
                            Folder.use_global_login = Convert.ToBoolean(Fnode.SelectSingleNode("GlobalLogin").InnerText);
                        }
                        catch (Exception e)
                        {
                            Log.writeLog(SimpleMind.Loglevel.Warning, Comp, "Could not load folder login");
                            Log.writeLog(SimpleMind.Loglevel.Debug, Comp, e.Message);
                            Folder.use_global_login = true;
                        }

                        try
                        {
                            Folder.Automount = Convert.ToBoolean(Fnode.SelectSingleNode("Automount").InnerText);
                        }
                        catch (Exception e)
                        {
                            Log.writeLog(SimpleMind.Loglevel.Warning, Comp, "Could not load attribute Automount. Set to false");
                            Log.writeLog(SimpleMind.Loglevel.Debug, Comp, e.Message);
                            Folder.Automount = false;
                        }

                        Folder.Username = Fnode.SelectSingleNode("Username").InnerText;
                        Folder.Password = Fnode.SelectSingleNode("Password").InnerText;
                        Folder.Passphrase = Fnode.SelectSingleNode("Passphrase").InnerText;
                        Folder.PrivateKey = Fnode.SelectSingleNode("PrivateKey").InnerText;

                        Folder.Status = DriveStatus.Unmounted;

                        Server.Folders.Add(Folder);
                    }

                    LServermodel.Add(Server);
                }

            }

        }


        ///Saving Configurations into an XML file
        /**
         * SaveConfiguration() gets a string with the path, where to save the file config.xml
         * It saves all configurations to this XML file. 
         * 
         * @param String Savepath-Path where the XML file will be saved
         * 
         */
        private void SaveConfiguration(String Savepath)
        {

            XmlDocument doc = new XmlDocument();
            XmlNode Config;
            XmlDeclaration XmlDec;

            Config = doc.CreateElement("Configuration");

            XmlDec = doc.CreateXmlDeclaration("1.0", null, null);

            Config.AppendChild(doc.CreateElement("VirtualDriveLetter")).InnerText = VirtualDrive.Letter.ToString();
            Config.AppendChild(doc.CreateElement("ReconnectAfterWakeUp")).InnerText = ReconnectAfterWakeUpFlag.ToString();
            Config.AppendChild(doc.CreateElement("LogLevel")).InnerText = "" + (int) Log.getLogLevel();

            doc.AppendChild(Config);
            doc.InsertBefore(XmlDec, Config);
            try
            {

                if (Savepath.EndsWith(@"\"))
                {
                    Savepath.Remove(Savepath.Length - 1);
                }

                doc.Save(Savepath + @"\config.xml");
            }
            catch (Exception e)
            {
                Log.writeLog(SimpleMind.Loglevel.Error, Comp, "Could not save Configuration.");
                Log.writeLog(SimpleMind.Loglevel.Debug, Comp, e.Message);
            }
        }

        ///Load Configuration from an XML file
        /**
         * LoadConfiguration() gets a string with the path, where the file config.xml is saved.
         * It loads the configuration saved in the XML file.
         * 
         * @param String Savepath-Path where the XML file is saved
         * 
         */
        private static void LoadConfiguration(String Savepath)
        {
            bool error_on_load = false;
            XmlDocument doc = new XmlDocument();

            if (Savepath.EndsWith(@"\"))
            {
                Savepath.Remove(Savepath.Length - 1);
            }

            try
            {
                doc.Load(Savepath + @"\config.xml");
            }

            catch (Exception e)
            {
                Log.writeLog(SimpleMind.Loglevel.Error, Comp, "Could not load configurations!");
                Log.writeLog(SimpleMind.Loglevel.Debug, Comp, e.Message);
                error_on_load = true;
                VirtualDrive.Letter = 'Z';
                ReconnectAfterWakeUpFlag = false;
                Log.setLogLevel((int)SimpleMind.Loglevel.None);
            }

            if (!error_on_load)
            {
                XmlNode config = doc.DocumentElement;//.SelectSingleNode("Configuration");


                try
                {
                    VirtualDrive.Letter = config.SelectSingleNode("VirtualDriveLetter").InnerText.ToCharArray()[0];
                    ReconnectAfterWakeUpFlag = Convert.ToBoolean(config.SelectSingleNode("ReconnectAfterWakeUp").InnerText);
                    Log.setLogLevel(Convert.ToInt32(config.SelectSingleNode("LogLevel").InnerText));
                }
                catch
                {
                    VirtualDrive.Letter = 'Z';
                    ReconnectAfterWakeUpFlag = false;
                    Log.setLogLevel((int)SimpleMind.Loglevel.None);
                }
            }

        }


        /// Look into virtual drive
        /**
         * This method executes a "dir" command.
         * That is necessary so virtual drive will be mounted
         */
        private static void LookIntoVirtualDrive(string folder)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C dir " + VirtualDrive.Letter + ":\\" + folder;
            process.StartInfo = startInfo;
            process.Start();
            return;
        }

        private static void MountDrive(ServerModel server, FolderModel folder)
        {
            Log.writeLog(SimpleMind.Loglevel.Debug, Comp, "Client tries to mount folder \"" + folder.ID + "\" mounted on server \"" + server.ID + "\"");
            SftpDrive drive = new SftpDrive();

            LSftpDrive[new Tuple<Guid, Guid>(server.ID, folder.ID)] = drive;

            drive.Host = server.Host;
            drive.Port = server.Port;

            drive.Root = folder.Folder;
            drive.Name = folder.Name;

            if (folder.use_global_login)
            {
                drive.Username = server.Username;
                drive.Password = server.Password;
                drive.PrivateKey = server.PrivateKey;
                drive.ConnectionType = server.Type;
            }
            else
            {
                drive.Username = folder.Username;
                drive.Password = folder.Password;
                drive.PrivateKey = folder.PrivateKey;
                drive.ConnectionType = folder.Type;
            }

            if (folder.use_virtual_drive) 
            {
                // check if virtual drive folder exists
                if (System.IO.Directory.Exists(VirtualDrive.Letter + ":\\" + folder.VirtualDriveFolder))
                {
                    throw new FaultException("Such virtual drive folder allready exists.");
                }

                drive.Letter = ' ';

                try
                {
                    // Adding folder to virtual drive
                    drive.MountPoint = folder.VirtualDriveFolder;
                    VirtualDrive.AddSubFS(drive);
                    drive.Mount();
                }
                catch (Exception e)
                {
                    VirtualDrive.RemoveSubFS(drive);
                    throw e;
                }
                // look into virtual drive, so it will be mounted
                LookIntoVirtualDrive(drive.MountPoint);

                Log.writeLog(SimpleMind.Loglevel.Debug, Comp, "folder \"" + folder.ID + "\" on server \"" + server.ID + "\" mounted in virtual drive.");
    
            }
            else {
                drive.Letter = folder.Letter;
                drive.Mount();
                Log.writeLog(SimpleMind.Loglevel.Debug, Comp, "folder \"" + folder.ID + "\" on server \"" + server.ID + "\" mounted at drive letter " + folder.Letter + ":.");
            }

        }
        
        private static void UnmountDrive(Guid ServerID, Guid FolderID)
        {
            try
            {
                SftpDrive drive = LSftpDrive[new Tuple<Guid, Guid>(ServerID, FolderID)];

                VirtualDrive.RemoveSubFS(drive);
                drive.Unmount();
                LSftpDrive.Remove(new Tuple<Guid, Guid>(ServerID, FolderID));

                Log.writeLog(SimpleMind.Loglevel.Debug, Comp, "folder \"" + FolderID + "\" on server \"" + ServerID + "\" unmounted.");
                return;
            }
            catch (Exception e)
            {
                Log.writeLog(SimpleMind.Loglevel.Error, Comp, "Could not unmount drive");
                Log.writeLog(SimpleMind.Loglevel.Debug, Comp, e.Message);
                throw e;
            }
        }

        public void Shutdown()
        {
            try
            {
                var keys = new List<Tuple<Guid, Guid>>(LSftpDrive.Keys);
                foreach (Tuple<Guid, Guid> i in keys)
                {
                    UnmountDrive(i.Item1, i.Item2);
                }
                ShutMeDown = true;
            }
            catch(Exception e)
            {
                throw new FaultException(e.Message);
            }

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

        /// get a drive to a proper virtual driver
        /**
         * This method gets a letter and looks this letter up in LSftpDrive.
         * If there is a mounted drive with the given virtual drive this method will
         * return the drive's ids.
         * If there is no such drive it will return empty ids (Guid.Empty)
         * 
         * @param virtual_drive_folder    virtual drive you want to search for
         * 
         * @return ids of found server and folder or empty ids
         */
        public Tuple<Guid, Guid> GetVirtualDriveUsage(string virtual_drive_folder)
        {
            foreach (KeyValuePair<Tuple<Guid, Guid>, SftpDrive> i in LSftpDrive)
            {
                if (i.Value.MountPoint == virtual_drive_folder && 
                    i.Value.Status == DriveStatus.Mounted)
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

        /// Move Server in datamodell
        /**
         * This method moves a given server.
         * 
         * @param ServerToMoveID   server id of that server you want to move
         * @param ServerToInserverAfterID   server id of that server where the moving server should  be inserted after
         * 
         */
        public void MoveServerAfter(Guid ServerToMoveID, Guid ServerToInsertAfterID )
        {
            try 
            {
                ServerModel server = LServermodel.Find(x => x.ID == ServerToMoveID);
                int IndexToInsertIn;

                LServermodel.Remove(server);
                if (ServerToInsertAfterID == Guid.Empty)
                {
                    IndexToInsertIn = 0;
                }
                else {
                    IndexToInsertIn = 1 + LServermodel.FindIndex(x => x.ID == ServerToInsertAfterID);
                }
                LServermodel.Insert(IndexToInsertIn, server);

                SaveServerlist(path_to_config_directory);
                
                return;
            }
            catch (Exception e)
            {
                Log.writeLog(SimpleMind.Loglevel.Debug, Comp, e.Message);
                throw new FaultException<Fault>(new Fault(e.Message));
            }
        }

        /// Move folder in datamodell
        /**
         * This method moves a given folder from one server to an other.
         * The given folder will be inserted after the folder which is given by the 
         * second folder parameter (FolderToInsertAfterID).
         * If you want to push the folder to the first position you need to give a empty id.
         * If you just want to move a folder inside a server you can give the same source and sink id.
         * 
         * @param SourceServerID        server id, where the folder, which you want to move, is located
         * @param SinkServerID          server id, where you want to move the folder to
         * @param FolderToMoveID        folder id of the folder you want to move
         * @param FolderToInsertAfterID folder id of that folder, where you want to insert the moving folder after
         */
        public void MoveFolderAfter(Guid SourceServerID, Guid SinkServerID, Guid FolderToMoveID, Guid FolderToInsertAfterID)
        {
            try
            {
                ServerModel source_server = LServermodel.Find(x => x.ID == SourceServerID);
                ServerModel sink_server = LServermodel.Find(x => x.ID == SinkServerID);

                FolderModel folder = source_server.Folders.Find(x => x.ID == FolderToMoveID);
                int IndexToInsertIn;

                source_server.Folders.Remove(folder);
                if (FolderToInsertAfterID == Guid.Empty)
                {
                    IndexToInsertIn = 0;
                }
                else
                {
                    IndexToInsertIn = 1 + sink_server.Folders.FindIndex(x => x.ID == FolderToInsertAfterID);
                }

                if (sink_server != source_server)
                {
                    folder.Name = CheckFolderName(sink_server, folder.Name);
                }
                sink_server.Folders.Insert(IndexToInsertIn, folder);

                SaveServerlist(path_to_config_directory);

                return;
            }
            catch (Exception e)
            {
                Log.writeLog(SimpleMind.Loglevel.Debug, Comp, e.Message);
                throw new FaultException<Fault>(new Fault(e.Message));
            }
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
                Log.writeLog(SimpleMind.Loglevel.Error, Comp, "Could not mount drive");
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
                UnmountDrive(ServerID, FolderID);
                return;
            }
            catch(Exception e) {
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

            if (local_server_reference.Name != RemoveIllegalChars(Server.Name))
            {
                local_server_reference.Name = CheckServerName(Server.Name);
            }
            local_server_reference.Password = Server.Password;
            local_server_reference.Passphrase = Server.Passphrase;
            local_server_reference.Notes = Server.Notes;
            local_server_reference.PrivateKey = Server.PrivateKey;
            local_server_reference.Username = Server.Username;
            local_server_reference.Type = Server.Type;
            local_server_reference.Host = Server.Host;
            local_server_reference.Port = Server.Port;

            Log.writeLog(SimpleMind.Loglevel.Debug, Comp, "server " + Server.ID + " has been edit");
            SaveServerlist(path_to_config_directory);
            return;
       }

        /// overwrites the folder properties
        /**
         * editFolder() gets a ServerModel from client and a server id.
         * It will search for a server with same id in
         * 
         * @param ServerID  id of server where wanted folder is located
         * @param Folder    edited folder, with same id as original folder
         */
        public void editFolder(Guid ServerID, FolderModel Folder)
        {
            ServerModel local_server_reference = LServermodel.Find(x => x.ID == ServerID);

            if (local_server_reference == null)
            {
                string message = "editFolder() got unkown server id";
                Log.writeLog(SimpleMind.Loglevel.Error, Comp, message);
                throw new FaultException<Fault>(new Fault(message));
            }

            
            FolderModel local_folder_reference = local_server_reference.Folders.Find(x => x.ID == Folder.ID);

            if (local_folder_reference == null)
            {
                string message = "editFolder() got unkown folder id";
                Log.writeLog(SimpleMind.Loglevel.Error, Comp, message);
                throw new FaultException<Fault>(new Fault(message));
            }

            if (local_folder_reference.Name != RemoveIllegalChars(Folder.Name))
            {
                Folder.Name = CheckFolderName(local_server_reference, Folder.Name);
            }
            local_folder_reference.Set(Folder);
            /*local_folder_reference.Password = Folder.Password;
            local_folder_reference.Passphrase = Folder.Passphrase;
            local_folder_reference.PrivateKey = Folder.PrivateKey;
            local_folder_reference.Username = Folder.Username;
            local_folder_reference.Letter = Folder.Letter;
            local_folder_reference.Type = Folder.Type;
            local_folder_reference.Username = Folder.Username;
            local_folder_reference.VirtualDriveFolder = Folder.VirtualDriveFolder;
            local_folder_reference.use_virtual_drive = Folder.use_virtual_drive;
            local_folder_reference.use_global_login = Folder.use_global_login;*/

            Log.writeLog(SimpleMind.Loglevel.Debug, Comp, "folder " + Folder.ID + " has been edit");
            SaveServerlist(path_to_config_directory);
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
            newServer.Name = CheckServerName(newServer.Name);
            newServer.ID = Guid.NewGuid();

            if(newServer.ID == Guid.Empty)
            {
                return addServer(newServer);
            }

            else
            {
                LServermodel.Add(newServer);
                SaveServerlist(path_to_config_directory);
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
                string message = "addFolder() got unkown server id";
                Log.writeLog(SimpleMind.Loglevel.Error , Comp, message);
                throw new FaultException<Fault>(new Fault(message));
            }

            Folder.ID = Guid.NewGuid();
            Folder.Name = CheckFolderName(server, Folder.Name);

            if(Folder.Letter <= 'A' || Folder.Letter >= 'Z')
            {
                try
                {
                    Folder.Letter = GetFreeDriveLetter();
                }
                catch
                {
                    Folder.Letter = 'Z';
                }
            }

            if (Folder.ID == Guid.Empty)
            {
                return addFolder(ServerID, Folder);
            }
            else
            {
                server.Folders.Add(Folder);
                SaveServerlist(path_to_config_directory);
                return Folder.ID;
            }
        }

        /// Duplicate a Server
        public Guid duplicateServer(Guid ServerID)
        {
            try
            {

                int i = LServermodel.FindIndex(x => x.ID == ServerID);
                ServerModel server = LServermodel[i];
                ServerModel newServer = server.DuplicateServer();
 
                newServer.Name = CheckServerName(newServer.Name);
                LServermodel.Insert(i+1, newServer);
                SaveServerlist(path_to_config_directory);
                return newServer.ID;
            }
            catch (Exception e)
            {
                Log.writeLog(SimpleMind.Loglevel.Error, Comp, "duplicateServer(): " + e.Message);
                throw new FaultException(e.Message);
            }
        }
        
        /// Duplicate a folder
        public Guid duplicateFolder(Guid ServerID, Guid FolderID)
        {
            try
            {
                ServerModel server = LServermodel.Find(x => x.ID == ServerID);
                int i = server.Folders.FindIndex(x => x.ID == FolderID);
                FolderModel newFolder = server.Folders[i].DuplicateFolder();
                newFolder.Name = CheckFolderName(server, newFolder.Name);
                server.Folders.Insert(i+1, newFolder);
                SaveServerlist(path_to_config_directory);
                return newFolder.ID;
            }
            catch (Exception e)
            {
                Log.writeLog(SimpleMind.Loglevel.Error, Comp, "duplicateServer(): " + e.Message);
                throw new FaultException(e.Message);
            }
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

            SaveServerlist(path_to_config_directory);
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

            SaveServerlist(path_to_config_directory);
            return;
       }

        /// Set the Loglevel in Backend
        public void setLogLevel(SimpleMind.Loglevel newLogLevel)
        {
            Log.setLogLevel((int)newLogLevel);
            SaveConfiguration(path_to_config_directory);
        }

        /// Get the Loglevel in Backend return value is the Loglevel after update
        public SimpleMind.Loglevel getLogLevel()
        {
            return (SimpleMind.Loglevel)Log.getLogLevel();
        }

        /// Get "Start Software with Systemstart" flag
        public bool IsStartBySystemStartSet()
        {
            return StartWithSystemstartFlag;
        }

        /// Set "Start Software with Systemstart flag; true means yes-"start with systemstart§
        public void SetStartBySystemStart(bool TrueMeansYes)
        {
            StartWithSystemstartFlag = TrueMeansYes;
            // Hier Speicherfunktion einfügen :::FIXME:::
        }

        /// Get "Reconnect after wake up" flag
        public bool IsReconnectAfterWakeUpSet()
        {
            return ReconnectAfterWakeUpFlag;
        }

        /// Set "Reconnect after wake up" flag; true means yes-"start with systemstart"
        public void SetReconnectAfterWakeUp(bool TrueMeansYes)
        {
            ReconnectAfterWakeUpFlag = TrueMeansYes;
            SaveConfiguration(path_to_config_directory);
        }
        
        /// Get virtual drive letter
        public char GetVirtualDriveLetter()
        {
            return VirtualDrive.Letter;
        }

        /// Set virtual drive letter
        public void SetVirtualDriveLetter(char letter)
        {
            if (IsDriveAvailable(letter))
            {
                VirtualDrive.Unmount();

                if (IsDriveAvailable(letter))
                {
                    VirtualDrive.Letter = letter;
                }
                else
                {
                    Log.writeLog(SimpleMind.Loglevel.Error, Comp, "SetVirtualDriveLetter() got unavailable drive letter.");
                }

                try
                {
                    VirtualDrive.Mount();
                }
                catch
                {
                    Log.writeLog(SimpleMind.Loglevel.Error, Comp, "Cannot mount virtual drive. Maybe there is not available drive letter.");
                    Environment.Exit(1);
                }

                // look into every virtual drive so they will be mounted
                foreach (KeyValuePair<Tuple<Guid, Guid>, SftpDrive> i in LSftpDrive)
                {
                    if (i.Value.Letter == ' ') 
                    {
                        LookIntoVirtualDrive(i.Value.MountPoint);
                    }
                }

                SaveConfiguration(path_to_config_directory);
            }
            else
            {
                string message = "Drive letter is not available";
                Log.writeLog(SimpleMind.Loglevel.Error, Comp, "SetVirtualDriveLetter() got an unavailible drive letter.");
                throw new FaultException(message);
            }
        }

        public string CheckServerName(string name)
        {
            name = RemoveIllegalChars(name);
            
            if (name == "" || name == null)
            {
                return CheckServerName("New server");
            }

            while (LServermodel.Find(x => x.Name == name) != null)
            {
                name = NameExtensionAdd(name);
            }
            return name;
        }

        public string CheckFolderName(ServerModel server, string name)
        {

            name = RemoveIllegalChars(name);
            
            if (name == "" || name == null)
            {
                return CheckFolderName(server, "New folder");
            }

            while (server.Folders.Find(x => x.Name == name) != null)
            {
                name = NameExtensionAdd(name);
            }
            return name;
        }

        private string RemoveIllegalChars(string name)
        {
            try
            {
                name = name.Replace(":", "");
                name = name.Replace("/", "");
                name = name.Replace("\\", "");
                while (name.First() == '-')
                {
                    name = name.Substring(1, name.Length - 1);
                }
                return name;
            }
            catch
            {
                return "";
            }
        }


        private static string NameExtensionAdd(string name)
        {
            int start = name.LastIndexOf('(');
            int end = name.LastIndexOf(')');

            if (start > 0 && start < end)
            {
                string substring = name.Substring(start + 1, end - start - 1);
                int number = Convert.ToInt32(substring);

                if (substring == number.ToString())
                {
                    number++;
                    return name.Substring(0, start) + "(" + number + ")";
                }
            }

            return name + "(1)";
        }
 
        /// Get s free dirve letter
        /**
         * This method chechs every drive letter starting with 'Z'
         * and going down till 'A'.
         * If it has found one, it will return it.
         * If there is no available drive letter, a execption will be thrown.
         * 
         * It starts with the highest drive letter 
         * because user is used to have memory sticks at the lower ones.
         * 
         * @return  last free drive letter
         */
        private static char GetFreeDriveLetter()
        {
            // Check every letter starting at 'Z'
            // and going down till 'A'
            for (int i = 'Z'; i >= 'A'; i--)
            {
                if (IsDriveAvailable((char)i))
                {
                    return (char)i;
                }
            }

            //if we could not find any free drive letter
            string message = "Couldn't find free Drive Letter";
            Log.writeLog(SimpleMind.Loglevel.Error, Comp, message);
            throw new Exception(message);
        }

        private static bool IsDriveAvailable(char letter)
        {
            List<char> not_available = new List<char>();
            not_available.Add('a');
            not_available.Add('A');
            not_available.Add('b');
            not_available.Add('B');

            if (not_available.Contains(letter) ||
                Directory.GetLogicalDrives().Contains(((letter).ToString() + @":\")))
            {
                return false;
            }
            else
            {
                foreach(ServerModel server in LServermodel)
                {
                    foreach(FolderModel folder in server.Folders)
                    {
                        if(! folder.use_virtual_drive && folder.Letter == letter)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        // Deconstructor
        /*~ServiceFisshBone()
        { 
            foreach(  var e in LSftpDrive)
            {
                if (e.Value.Status == DriveStatus.Mounted)
                {
                    this.UMount(e.Key.Item1, e.Key.Item2);
                }
            }

            VirtualDrive.Unmount();
        }*/

    }
}
