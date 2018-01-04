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

            // From here begins the connection part, must be in each client (Copy & Past)
            // then you can access the connection via the object "bone"
            // Connection Part End
            /* ChannelFactory<IServiceFisshBone> pipeFactory =
               new ChannelFactory<IServiceFisshBone>(
                 new NetNamedPipeBinding(),
                 new EndpointAddress(
                   "net.pipe://localhost/FiSSH"));

             IServiceFisshBone bone =
               pipeFactory.CreateChannel();
             */
            IServiceFisshBone bone = IPCConnection.ClientConnect();


            try
            {
                // Get the data from the backend
                List<ServerModel> list_of_server = bone.listAll();

                // This will get the server to a server name
                ServerModel searched_server = list_of_server.Find(x => x.Name == "Ubuntu at VBox");

                // This will get the folder entry to a folder entry name
                FolderModel searched_folder = searched_server.Folders.Find(x => x.Name == "home from user");

                // editors
                Console.WriteLine("Edit ...");
                Console.ReadLine();

                // edit test
                ServerModel vbx_ubuntu = searched_server;
                vbx_ubuntu.Name += "edited";
                vbx_ubuntu.Port = 1337;
                vbx_ubuntu.Notes = "hui, great notes";
                bone.editServer(vbx_ubuntu);

                Console.WriteLine("Edited.");
                Console.ReadLine();


                // edit test
                FolderModel home = searched_folder;
                home.Name += "edited";
                home.Note = "hui, even better notes";
                bone.editFolder(searched_server.ID, home);

                Console.WriteLine("Edited.");
                Console.ReadLine();

                // To mount an entry
                bone.Mount(searched_server.ID, searched_folder.ID);
                Console.WriteLine("Mounted, press Enter to Unmount");
                Console.ReadLine();

                // How to Unmount an Entry
                bone.UMount(searched_server.ID, searched_folder.ID);

                // How to delete a folder
                //bone.removefolder (searched_server.ID, searched_folder.ID);

                // How to delete a server
                //bone.removeServer(searched_server.ID);

                Console.WriteLine("\nIn the following, we will display everything from the data model.");
                list_of_server = bone.listAll();
                foreach (ServerModel i in list_of_server)
                {
                    Console.WriteLine(i.ID + ": " + i.Name);

                    foreach (FolderModel j in i.Folders)
                    {
                        Console.WriteLine("    " + j.ID + ": " + j.Name);
                    }
                    Console.WriteLine();
                }
            }
            catch (FaultException<Fault> e)
            {
                 Console.WriteLine(e.Detail.Message);
                 Console.ReadLine();
                 return;
            }

            Console.WriteLine("Press enter to exit client");
            Console.ReadLine();


        }
    }
}
