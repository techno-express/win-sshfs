using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Sshfs.GuiBackend;
using Sshfs.GuiBackend.Remoteable;
using Sshfs.GuiBackend.IPCChannelRemoting;

using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;

namespace testclient
{
    class Program
    {
        static void Main(string[] args)
        {

            // Ab hier beginnt der Verbindungsteil, muss in jeden Client (Copy&Past)
            // danach kann auf die Verbindung über das Objekt "bone" zugreifen
            IpcChannel ipcCh = new IpcChannel("myClient");
            ChannelServices.RegisterChannel(ipcCh, true);

            IServiceFisshBone bone =
               (Sshfs.GuiBackend.Remoteable.IServiceFisshBone)Activator.GetObject
               (typeof(Sshfs.GuiBackend.Remoteable.IServiceFisshBone),
                "ipc://FiSSH/fisshy");
            //Verbindungsteil-Ende

            // Die Daten aus dem Backend holen
            string liste_von_server_als_string = bone.listAll();
            
            // Die Daten in ein nutzbares Objekt umwandeln
            List<ServerModel> liste_von_server = IServiceTools.DeserializeObject<List<ServerModel>>(liste_von_server_als_string);
            
            // So erhält man den Server zu einem Servernamen
            ServerModel gesuchter_server = liste_von_server.Find(x => x.Name == "Ubuntu at VBox");

            // So erhält man den Ordnereintrag zu einem Ordnereintragsnamen
            FolderModel gesuchter_folder = gesuchter_server.Folders.Find(x => x.name == "home vom user");


            Console.WriteLine("Enterdrücken um zu mounten von");
            Console.ReadLine();
            
            // So Mountet man einen Eintrag
            bone.Mount(gesuchter_server.ServerID, gesuchter_folder.FolderID);
                
            Console.ReadLine();


        }
    }
}
