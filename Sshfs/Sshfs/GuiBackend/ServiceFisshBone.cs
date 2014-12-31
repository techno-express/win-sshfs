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
            
        }

#region INTERFACE
        List<SftpDrive_for_hierachy> search(Guid ID){
        return new List<SftpDrive_for_hierachy>;
        }

        
        List<SftpDrive_for_hierachy> search(char letter){
        return new List<SftpDrive_for_hierachy>;
        }

        
        int Mount(Guid ID){
        return -1;
        }

        
        int UMount(Guid ID){
        return -1;
        }

        
        DriveStatus getStatus(Guid ID){

            DriveStatus DS = DriveStatus.Undefined;

            int Index = Find(ID, "Could not find ServerID to print Drivestatus");

            if(Index >= 0)
            {
                DS = LServermodel[Index].Status;
            }

            return DS;
        }

        
        List<ServerModel> listAll(){
        return new List<ServerModel>(LServermodel);
        }

        
        int removeServer(Guid ID){ return -1;}

        
        int removeFolder(Guid ID){return -1;}

        
        int editDrive(SftpDrive_for_hierachy){return -1;}

        
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

        
        Guid /*ID*/ addFolder(Guid ID /*Folderdeskription*/){return new Guid("0");}

        // Returnvalue is -1 in Error case or else the remove index
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

        
        int Connect(Guid ID){return -1;}

        
        void Disconnect(Guid ID){}
#endregion
        
    }
}
