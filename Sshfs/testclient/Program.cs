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



            IpcChannel ipcCh = new IpcChannel("myClient");
            ChannelServices.RegisterChannel(ipcCh, true);

            IServiceFisshBone bone =
               (Sshfs.GuiBackend.Remoteable.IServiceFisshBone)Activator.GetObject
               (typeof(Sshfs.GuiBackend.Remoteable.IServiceFisshBone),
                "ipc://FiSSH/fisshy");


            string liste_von_server_als_string = bone.listAll();
            /*var xmlSerializer = new System.Xml.Serialization.XmlSerializer(new List<ServerModel>().GetType());
            var textReader = new System.IO.StringReader(liste_von_server_als_string);

            List<ServerModel> liste_von_server = (List<ServerModel>)xmlSerializer.Deserialize(textReader);
            */
            List<ServerModel> liste_von_server = IServiceTools.DeserializeObject<List<ServerModel>>(liste_von_server_als_string);
            ServerModel gesuchter_server = liste_von_server.Find(x => x.Name == "Ubuntu at VBox");

            FolderModel gesuchter_folder = gesuchter_server.Folders.Find(x => x.name == "home vom user");


            Console.WriteLine("Enterdrücken um zu mounten von");
            Console.ReadLine();
            bone.Mount(gesuchter_server.ServerID, gesuchter_folder.FolderID);
                
            Console.ReadLine();


        }
    }
}
