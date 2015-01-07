using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Sshfs;

namespace Sshfs.GuiBackend
{
    // HINWEIS: Mit dem Befehl "Umbenennen" im Menü "Umgestalten" können Sie den Klassennamen "Service1" sowohl im Code als auch in der Konfigurationsdatei ändern.
    public class ServiceFisshBone : IServiceFisshBone
    {
        const String Comp = "Backend";

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
        private List<SftpDrive> LSftpDrive = new List<SftpDrive>();
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
            int Index = Find(ID, "Couldn't find server to create drive");
            ServerModel tempServer;

            SftpDrive drive = null; /*new SftpDrive
                             {
                                 Name = "",
                                 Port = "",
                                 Root = ".", //?
                                 Letter = 'X',
                                 MountPoint = "",
                             };*/

            if(Index > -1)
            {
                tempServer = LServermodel.ElementAt(Index);
                drive.Name = tempServer.Name;
                drive.Port = tempServer.Port;
                drive.Root = tempServer.Root;
                //FIXME
                drive.Letter = tempServer.DriveLetter;
                drive.MountPoint = tempServer.Mountpoints.ElementAt(0).Mountpoint;
            }


            return drive;
        }

#region INTERFACE
        ServerModel search(Guid ID){

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

        
        int Mount(Guid ID){
        return -1;
        }

        
        int UMount(Guid ID){
        return -1;
        }

        
        DriveStatus getStatus(Guid ID){

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

        
        List<ServerModel> listAll(){
        return new List<ServerModel>(LServermodel);
        }

        
        int removeServer(Guid ID){

            int Index = Find(ID, "Couldn't find ServerID to remove.");

            if (Index >= 0)
            {
                LServermodel.RemoveAt(Index);
            }

            return Index;    
        }
        
        
        
        int editServer(ServerModel Server){

            int Index = Find(Server.ServerID, "Couldn't find ServerID to edit use addServer instead to add a new server.");

            if (Index >= 0)
            {
                LServermodel.Insert(Index + 1, Server);
                LServermodel.RemoveAt(Index);
            }

            return Index;
        }

        
        Guid /*ID*/ addServer(ServerModel newServer){
            
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

        
        int addFolder(Guid ID, FolderModel Mountpoint){

            int Index = Find(ID, "Couldn't find ServerID to add folder");

            if (Index >= 0)
            {
                LServermodel.ElementAt(Index).Mountpoints.Add(Mountpoint);

                DriveStatus DS = LServermodel.ElementAt(Index).Status;

                if(DS == DriveStatus.Mounted || DS ==  DriveStatus.Mounting)
                {
                    Log.writeLog(SimpleMind.Loglevel.Warning, Comp, "Drive is mounted changes effect after remount");
                }
            }

            return Index;
        }

        int removeFolder(Guid ID, FolderModel Mountpoint)
        {
            ServerModel tempServer;
            int Index = Find(ID, "Coludn't find ServerID to remove Folder");
            int mpIndex = -1;

            if (Index >= 0)
            {
                tempServer = LServermodel.ElementAt(Index);

                try
                {
                    mpIndex = tempServer.Mountpoints.FindIndex(x => x.Mountpoint == Mountpoint.Mountpoint);
                }
                catch (ArgumentNullException e)
                {
                    Log.writeLog(SimpleMind.Loglevel.Error, Comp, "Couldn't find mountpoint in Servermodel with ID: " + LServermodel.ElementAt(Index).ServerID.ToString());
                    Index = -1;
                    mpIndex = -1;
                }

                if (mpIndex >= 0)
                {
                    tempServer.Mountpoints.RemoveAt(mpIndex);
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
        int removeServer(Guid ID){
        
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


        SimpleMind.Loglevel setLogLevel(SimpleMind.Loglevel newLogLevel)
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
