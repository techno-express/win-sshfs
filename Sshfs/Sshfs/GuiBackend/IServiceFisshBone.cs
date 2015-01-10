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
        [FaultContractAttribute( typeof(Fault), ProtectionLevel = ProtectionLevel.EncryptAndSign )]
        void Mount(Guid ServerID, Guid FolderID);

        [OperationContract]
        void UMount(Guid ServerID, Guid FolderID);

        //Return the Drivestatus of the Server with ID, if Server does not exist Drivestatus is Error
        [OperationContract]
        DriveStatus getStatus(Guid ID);

        //Returns a list of all servers currently known
        [OperationContract]
        List<ServerModel> listAll();
        //string listAll();

        //Replaces the Server with the ID of the parameter Server, returnvalue is the Index of the replaced server or -1 if no ID matches
        //Notice, to add a new Server use, addServer(ServerModel);
        [OperationContract]
        void editServer(ServerModel Server);

        //Adding Server to List of known Server Returnvalue is the ID of the new Server or in error case 0
        [OperationContract]
        Guid addServer(ServerModel Server);

        // Returnvalue the remove index or in error case -1
        [OperationContract]
        void removeServer(Guid ID);

        // Adds Folder to the Mountpovoid list of Server with ID, returnvalue is the Index of the changed Server or in error case -1
        [OperationContract]
        Guid addFolder(Guid ID, FolderModel Mountpoint); //changed - bjoe-phi

        // Removes Folder from Server with ID, returnvalue is Index of the server or in error case -1
        [OperationContract]
        void removeFolder(Guid ID_Server, Guid ID_Folder);

        // Set the Loglevel in Backend return value is the Loglevel after update
        [OperationContract]
        SimpleMind.Loglevel setLogLevel(SimpleMind.Loglevel newLogLevel);

        /*[OperationContract]
        void Connect(Guid ID);

        [OperationContract]
        void Disconnect(Guid ID);*/
    }


    /// <summary>
    /// This class includes method which are needed to use the interface by server and client
    /// </summary>
    public static class IServiceTools
    {
        /// <summary>
        /// This method turns a serialized object back into a object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toDeserialize"></param>
        /// <returns></returns>
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
