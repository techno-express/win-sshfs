using System;
using System.ServiceModel;
using Sshfs;
using Sshfs.GuiBackend;
using Sshfs.GuiBackend.Remoteable;
using Sshfs.GuiBackend.IPCChannelRemoting;

using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;


public class Class1
{
	
        public static void Main()
        {
            // WCF-Server einrichten
            using (ServiceHost bone_server = new ServiceHost(
              typeof(ServiceFisshBone),
              new Uri[]{
          new Uri("net.pipe://localhost")
        }))
            {
                bone_server.AddServiceEndpoint(typeof(IServiceFisshBone),
                  new NetNamedPipeBinding(),
                  "FiSSH");
                bone_server.Open();

                //Ein Server mit Ordner zum Testen ins Datenmodel einfügen
                ServerModel ein_server = new ServerModel();

                ein_server.Name = "Ubuntu at VBox";
                ein_server.Host = "10.0.2.13";
                ein_server.Port = 2244;
                ein_server.Password = "user";
                ein_server.Username = "user";

                FolderModel ein_folder = new FolderModel();
                ein_folder.Letter = 'G';
                ein_folder.use_global_login = true;
                ein_folder.Folder = "/home/user";
                ein_folder.Name = "home vom user";

                ServiceFisshBone bone_local = new ServiceFisshBone();
                Guid ein_server_ID = bone_local.addServer(ein_server);

                Guid ein_folder_ID = bone_local.addFolder(ein_server_ID, ein_folder);

                Console.WriteLine("Please enter to stop the server");
                Console.ReadLine();

                bone_server.Close();
            }


        }
}
