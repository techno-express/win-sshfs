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
    // HINWEIS: Mit dem Befehl "Umbenennen" im Menü "Umgestalten" können Sie den Klassennamen "Service1" sowohl im Code als auch in der Konfigurationsdatei ändern.
    public class ServiceFisshBone : MarshalByRefObject, IServiceFisshBone
    {
        const String Comp = "Backend";

        public ServiceFisshBone() { }

/*
        public static void Configure(ServiceConfiguration config)
        {
            ServiceEndpoint se = new ServiceEndpoint(new ContractDescription("IService1"), new BasicHttpBinding(), new EndpointAddress("basic"));
            se.Behaviors.Add(new MyEndpointBehavior());
            config.AddServiceEndpoint(se);

            config.Description.Behaviors.Add(new ServiceMetadataBehavior { HttpGetEnabled = true });
            config.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });
        }
        */

        //private readonly StringBuilder _balloonText = new StringBuilder(255){}
        //private readonly List<String> _configVars = new List<String>(){}
        //private readonly Regex _regex = new Regex(@"^New Drive\s\d{1,2}$", RegexOptions.Compiled){}
        /*
        //private readonly Queue<SftpDrive> _suspendedDrives = new Queue<SftpDrive>(){}
        //private readonly List<SftpDrive> _drives = new List<SftpDrive>(){}
       
        public FisshBone()
        { }

*/
        private SimpleMind.SimpleMind Log = new SimpleMind.SimpleMind();
        
        private List<ServerModel> LServermodel = new List<ServerModel>();
        //private List<SftpDrive> LSftpDrive = new List<SftpDrive>();
        private Dictionary<Tuple<Guid, Guid>, SftpDrive> LSftpDrive = new Dictionary<Tuple<Guid,Guid>, SftpDrive>(); //erste Guid vom Server, zweite des Folder
        private List<VirtualDrive> LVirtualDrive = new List<VirtualDrive>();





        // internal Method to Find Servermodel by Guid and printing Errorlog
        private int Find(Guid ID, String ErrMsg)
        {
            int Index = -1;
            try{
                Index = LServermodel.FindIndex(x => x.ServerID == ID);
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

            return drive;
        }

#region INTERFACE
        public ServerModel search(Guid ID){

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
        }

        
        /*ServerModel search(char letter){

            LServermodel.Find(x => x.)

        return LServermodel.In;
        }*/


        public int Mount(Guid ServerID, Guid FolderID)
        {
           Log.writeLog(SimpleMind.Loglevel.Debug, Comp, "Mounting ...");

           Console.WriteLine("Mounting ...");
            /*int ServerIndex;
            FolderModel folder;
            ServerModel server;
            SftpDrive drive = new SftpDrive();
            
            ServerIndex = Find(ID, "Couldn't find server to create drive");
            if (ServerIndex < 0) // Falls Server nicht gefunden, return Fehlermeldung
            {
                return -1;
            }

            try {
                server = LServermodel.ElementAt(ServerIndex);
                folder = server.Folders[FolderID];
                LSftpDrive[ServerID, FolderID] = drive;
                
                drive.Host = server.Host;
                drive.Port = server.Port;
                
                drive.Letter = folder.Letter;
                drive.Root = folder.Root;

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
            


                
            }
            catch(Exception e) {
                Console.WriteLine("Fehlernehandlung nicht implementiert: {0}", e.Message);
            }

             */
           return -1;
        }


        public int UMount(Guid ID)
        {
        return -1;
        }


        public DriveStatus getStatus(Guid ID)
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


        public List<ServerModel> listAll()
        {
        return new List<ServerModel>(LServermodel);
        }

        /*
        int removeServer(Guid ID){

            int Index = Find(ID, "Couldn't find ServerID to remove.");

            if (Index >= 0)
            {
                LServermodel.RemoveAt(Index);
            }

            return Index;    
        }*/



        public int editServer(ServerModel Server)
        {

            int Index = Find(Server.ServerID, "Couldn't find ServerID to edit use addServer instead to add a new server.");

            if (Index >= 0)
            {
                LServermodel[Index] = Server;
                //LServermodel.Insert(Index + 1, Server);
                //LServermodel.RemoveAt(Index);
            }

            return Index;
        }


        public Guid /*ID*/ addServer(ServerModel newServer)
        {
            
            Guid ReturnValue;

            newServer.ServerID = Guid.NewGuid();

            if(newServer.ServerID == Guid.Empty)
            {
                Log.writeLog(SimpleMind.Loglevel.Error, Comp, "Error while creat ServerID");
                ReturnValue = new Guid("0");
            }

            else
            {
                LServermodel.Add(newServer);
                ReturnValue = newServer.ServerID;
            }

            return ReturnValue;
        }


        public int addFolder(Guid ID, FolderModel Mountpoint)
        {

            int Index = Find(ID, "Couldn't find ServerID to add folder");

            if (Index >= 0)
            {
                LServermodel.ElementAt(Index).Folders.Add(Mountpoint);

                DriveStatus DS = LServermodel.ElementAt(Index).Status;

                if(DS == DriveStatus.Mounted || DS ==  DriveStatus.Mounting)
                {
                    Log.writeLog(SimpleMind.Loglevel.Warning, Comp, "Drive is mounted changes effect after remount");
                }
            }

            return Index;
        }

        public int removeFolder(Guid ID_Server, Guid ID_Folder)
        {
            ServerModel tempServer;
            int Index = Find(ID_Server, "Coludn't find ServerID to remove Folder");
            int mpIndex = -1;

            if (Index >= 0)
            {
                tempServer = LServermodel.ElementAt(Index);

                try
                {
                    mpIndex = tempServer.Folders.FindIndex(x => x.FolderID == ID_Folder);
                }
                catch (ArgumentNullException e)
                {
                    Log.writeLog(SimpleMind.Loglevel.Error, Comp, "Couldn't find folder in Servermodel with ID: " + tempServer.ServerID.ToString());
                    Index = -1;
                    mpIndex = -1;
                }

                if (mpIndex >= 0)
                {
                    tempServer.Folders.RemoveAt(mpIndex);
                    if (editServer(tempServer) == -1)
                    {
                        Index = -1;
                        Log.writeLog(SimpleMind.Loglevel.Error, Comp, "Couldn't update Data while remove mountpoint");
                    }
                }

            }

            return Index;
        }

        // Returnvalue is -1 in error case or else the remove index
        public int removeServer(Guid ID)
        {
        
            int Index;
            Index = Find(ID, "Couldn't find ServerID to remove");

            if(Index >= 0)
            {
                try
                {
                    LServermodel.RemoveAt(Index);
                }
                catch(ArgumentOutOfRangeException e)
                {
                    Log.writeLog(SimpleMind.Loglevel.Error, Comp, "Error while remove Server");
                    Index = -1;
                }
            }

            else
            {
                Index = -1;
            }

            return Index;
        }


        public SimpleMind.Loglevel setLogLevel(SimpleMind.Loglevel newLogLevel)
        {
            Log.setLogLevel((int)newLogLevel);
            return (SimpleMind.Loglevel) Log.getLogLevel();
        }
        /*
        int Connect(Guid ID){return -1;}

        void Disconnect(Guid ID){}*/
#endregion
        
    }
}
