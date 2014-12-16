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
        //[OperationContract]
        //void DoWork();

        [OperationContract]
        List<SftpDrive_for_hierachy> search(Guid ID);

        [OperationContract]
        List<SftpDrive_for_hierachy> search(char letter);

        [OperationContract]
        int Mount(Guid ID);

        [OperationContract]
        int UMount(Guid ID);

        [OperationContract]
        DriveStatus getStatus(Guid ID);

        [OperationContract]
        List<SftpDrive_for_hierachy> listAll();

        [OperationContract]
        int removeServer(Guid ID);

        [OperationContract]
        int removeFolder(Guid ID);

        [OperationContract]
        int editDrive(SftpDrive_for_hierachy);

        [OperationContract]
        Guid /*ID*/ addServer(SftpDrive_for_hierachy);

        [OperationContract]
        Guid /*ID*/ addFolder(Guid ID /*Folderdeskription*/);

        [OperationContract]
        int removeServer(Guid ID);

        [OperationContract]
        int Connect(Guid ID);

        [OperationContract]
        void Disconnect(Guid ID);
        

    }
}
