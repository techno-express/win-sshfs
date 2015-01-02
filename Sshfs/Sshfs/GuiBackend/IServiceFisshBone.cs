using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Sshfs.GuiBackend
{
    // HINWEIS: Mit dem Befehl "Umbenennen" im Menü "Umgestalten" können Sie den Schnittstellennamen "IService1" sowohl im Code als auch in der Konfigurationsdatei ändern.
    [ServiceContract]
    public interface IServiceFisshBone
    {
       
        //Returns the Servermodel for the ID, if ID couldn't be find the returned ServerModel is a null Element
        [OperationContract]
        ServerModel search(Guid ID);

        /*[OperationContract]
        ServerModel search(char letter);*/

        [OperationContract]
        int Mount(Guid ID);

        [OperationContract]
        int UMount(Guid ID);

        //Return the Drivestatus of the Server with ID, if Server does not exist Drivestatus is Error
        [OperationContract]
        DriveStatus getStatus(Guid ID);

        //Returns a list of all servers currently known
        [OperationContract] 
        List<ServerModel> listAll();

        [OperationContract]
        int removeServer(Guid ID);

        //Replaces the Server with the ID of the parameter Server, returnvalue is the Index of the replaced server or -1 if no ID matches
        //Notice, to add a new Server use, addServer(ServerModel);
        [OperationContract]
        int editServer(ServerModel Server);

        //Adding Server to List of known Server Returnvalue is the ID of the new Server or in error case 0
        [OperationContract]
        Guid /*ID*/ addServer(ServerModel Server); 

        // Returnvalue the remove index or in error case -1
        [OperationContract]
        int removeServer(Guid ID);

        // Adds Folder to the Mountpoint list of Server with ID, returnvalue is the Index of the changed Server or in error case -1
        [OperationContract]
        int addFolder(Guid ID, string Folder);

        // Removes Folder from Server with ID, returnvalue is Index of the server or in error case -1
        [OperationContract]
        int removeFolder(Guid ID, string Folder);

        // Set the Loglevel in Backend return value is the Loglevel after update
        [OperationContract]
        SimpleMind.Loglevel setLogLevel(SimpleMind.Loglevel newLogLevel);

        /*[OperationContract]
        int Connect(Guid ID);

        [OperationContract]
        void Disconnect(Guid ID);*/
        

    }
}
