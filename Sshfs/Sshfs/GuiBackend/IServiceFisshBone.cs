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
        [OperationContract]
        void DoWork();

        [OperationContract]
        void Mount();

        [OperationContract]
        void UMount();

        [OperationContract]
        void add();

        [OperationContract]
        void remove();

        [OperationContract]
        void Connect();

        [OperationContract]
        void Disconnect();

        [OperationContract]
        void Change();



    }
}
