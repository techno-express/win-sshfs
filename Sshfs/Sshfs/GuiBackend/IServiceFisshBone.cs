using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

// added for throwing over WCF (FaultException<TDetail> Class)
// look here: http://msdn.microsoft.com/en-us/library/ms576199.aspx
using System.Net.Security;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Sshfs.GuiBackend.Remoteable
{  
    // HINWEIS: Mit dem Befehl "Umbenennen" im Menü "Umgestalten" können Sie den Schnittstellennamen "IService1" sowohl im Code als auch in der Konfigurationsdatei ändern.
    [ServiceContract]
    public interface IServiceFisshBone
    {
        //Returns the Servermodel for the ID, if ID couldn't be find the returned ServerModel is a null Element
//        [OperationContract]
//        ServerModel get_server_by_id(Guid ID);

        /*[OperationContract]
        ServerModel search(char letter);*/

        [OperationContract]
        [FaultContractAttribute(typeof(Fault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        void Shutdown();

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
        [OperationContract]
        Tuple<Guid, Guid> GetLetterUsage(char letter);

        [OperationContract]
        Tuple<Guid, Guid> GetVirtualDriveUsage(string virtual_drive_folder);

        [OperationContract]
        [FaultContractAttribute(typeof(Fault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        void MoveServerAfter(Guid ServerToMoveID, Guid ServerToInsertAfterID);

        [OperationContract]
        [FaultContractAttribute(typeof(Fault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        void MoveFolderAfter(Guid SourceServerID, Guid SinkServerID, Guid FolderToMoveID, Guid FolderToInsertAfterID);

        [OperationContract]
        [FaultContractAttribute( typeof(Fault), ProtectionLevel = ProtectionLevel.EncryptAndSign )]
        void UnregisteredMount(ServerModel server);

        [OperationContract]
        [FaultContractAttribute( typeof(Fault), ProtectionLevel = ProtectionLevel.EncryptAndSign )]
        void Mount(Guid ServerID, Guid FolderID);

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
        [OperationContract]
        [FaultContractAttribute( typeof(Fault), ProtectionLevel = ProtectionLevel.EncryptAndSign )]
        void UMount(Guid ServerID, Guid FolderID);

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
        [OperationContract]
        DriveStatus getStatus(Guid ServerID, Guid FolderID);

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
        [OperationContract]
        List<ServerModel> listAll();
        //string listAll();

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
        // Notice, to add a new Server use, addServer(ServerModel);
        [OperationContract]
        [FaultContractAttribute( typeof(Fault), ProtectionLevel = ProtectionLevel.EncryptAndSign )]
        void editServer(ServerModel Server);

        [OperationContract]
        [FaultContractAttribute(typeof(Fault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        void editFolder(Guid ServerID, FolderModel Folder);

        /// Adding Server to List of known Server
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
         * @return created serverID for new servermodel or in error case 0
         */
        [OperationContract]
        [FaultContractAttribute( typeof(Fault), ProtectionLevel = ProtectionLevel.EncryptAndSign )]
        Guid addServer(ServerModel Server);

        /// removes server from ServerModel list
        /**
         * removeServer() gets a id of a server.
         * It searches for the server given by id 
         * and removes it from the list.
         * 
         * @param ServerID  id of server you want to remove
         * 
         * @return the remove index or in error case -1
         * 
         */
        [OperationContract]
        [FaultContractAttribute( typeof(Fault), ProtectionLevel = ProtectionLevel.EncryptAndSign )]
        void removeServer(Guid ID);

        /// Adds Folder to the Mountpovoid list of Server with folderID
        /**
         * addFolder() gets a new FolderModel and a existing server id.
         * It will create a new id for the new folder and adds it to
         * its server in ServerModel list. The server is given by 
         * its id.
         * 
         * @param ServerID  ID of server where the folder should be added to
         * @param Folder    FolderModel which should be added
         * 
         * @return          Created ID of added folder or in error case -1
         * 
         */
        [OperationContract]
        [FaultContractAttribute( typeof(Fault), ProtectionLevel = ProtectionLevel.EncryptAndSign )]
        Guid addFolder(Guid ID, FolderModel Mountpoint); //changed - bjoe-phi

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
        [OperationContract]
        [FaultContractAttribute( typeof(Fault), ProtectionLevel = ProtectionLevel.EncryptAndSign )]
        void removeFolder(Guid ID_Server, Guid ID_Folder);

        /// Duplicate a Server
        [OperationContract]
        [FaultContractAttribute( typeof(Fault), ProtectionLevel = ProtectionLevel.EncryptAndSign )]
        Guid duplicateServer(Guid ServerID);
        
        /// Duplicate a folder
        [OperationContract]
        [FaultContractAttribute(typeof(Fault), ProtectionLevel = ProtectionLevel.EncryptAndSign)]
        Guid duplicateFolder(Guid ServerID, Guid FolderID);
    
       
        /// Set the Loglevel in Backend
        [OperationContract]
        void setLogLevel(SimpleMind.Loglevel newLogLevel);

        /// Get the Loglevel in Backend return value is the Loglevel after update
        [OperationContract]
        SimpleMind.Loglevel getLogLevel();

        /// Get "Start Software with Systemstart" flag
        [OperationContract]
        bool IsStartBySystemStartSet();

        /// Set "Start Software with Systemstart flag; true means yes-"start with systemstart"
        [OperationContract]
        void SetStartBySystemStart(bool TrueMeansYes);

        /// Get "Reconnect after wake up" flag
        [OperationContract]
        bool IsReconnectAfterWakeUpSet();

        /// Set "Reconnect after wake up" flag; true means yes-"start with systemstart"
        [OperationContract]
        void SetReconnectAfterWakeUp(bool TrueMeansYes);
        
        /// Get virtual drive letter
        [OperationContract]
        char GetVirtualDriveLetter();

        /// Set virtual drive letter
        [OperationContract]
        void SetVirtualDriveLetter(char letter);

        /*[OperationContract]
        void Connect(Guid ID);

        [OperationContract]
        void Disconnect(Guid ID);*/
    }


    
    /// This class includes method which are needed to use the interface by server and client
    public static class IServiceTools
    {
        
        /// This method turns a serialized object back into a object
        /// @param Object T can be any data type
      public static T DeserializeObject<T>(this string toDeserialize)
        {
            var xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
            var textReader = new System.IO.StringReader(toDeserialize);
            return (T) xmlSerializer.Deserialize(textReader);
        }

        /// <summary>
        /// This method turns any object into a string which can be send over the IPC-Interface
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toSerialize"></param>
        /// <returns></returns>
        public static string SerializeObject<T>(this T toSerialize)
        {
            var xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
            var textWriter = new System.IO.StringWriter();
            xmlSerializer.Serialize(textWriter, toSerialize);
            return textWriter.ToString();
        } 
    }

    [DataContractAttribute]
    public class Fault
    {
        private string report;

        public Fault(string message)
        {
            this.report = message;
        }

        [DataMemberAttribute]
        public string Message
        {
            get { return this.report; }
            set { this.report = value; }
        }
    }

}
