using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Sshfs.GuiBackend
{
    // HINWEIS: Mit dem Befehl "Umbenennen" im Menü "Umgestalten" können Sie den Klassennamen "Service1" sowohl im Code als auch in der Konfigurationsdatei ändern.
    public class ServiceFisshBone : IServiceFisshBone
    {

        //private readonly StringBuilder _balloonText = new StringBuilder(255){}
        //private readonly List<String> _configVars = new List<String>(){}
        //private readonly Regex _regex = new Regex(@"^New Drive\s\d{1,2}$", RegexOptions.Compiled){}
        /*
        //private readonly Queue<SftpDrive> _suspendedDrives = new Queue<SftpDrive>(){}
        //private readonly List<SftpDrive> _drives = new List<SftpDrive>(){}
       
        public FisshBone()
        { }

*/


        
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
        return -1;
        }

        
        List<SftpDrive_for_hierachy> listAll(){
        return new List<SftpDrive_for_hierachy>;
        }

        
        int removeServer(Guid ID){ return -1;}

        
        int removeFolder(Guid ID){return -1;}

        
        int editDrive(SftpDrive_for_hierachy){return -1;}

        
        Guid /*ID*/ addServer(SftpDrive_for_hierachy){return new Guid("0");}

        
        Guid /*ID*/ addFolder(Guid ID /*Folderdeskription*/){return new Guid("0");}

        
        int removeServer(Guid ID){return -1;}

        
        int Connect(Guid ID){return -1;}

        
        void Disconnect(Guid ID){}
        
    }
}
