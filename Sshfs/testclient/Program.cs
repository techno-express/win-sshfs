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
           //Verbindungsteil-Ende
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
                // Die Daten aus dem Backend holen
                List<ServerModel> liste_von_server = bone.listAll();

                // So erhält man den Server zu einem Servernamen
                ServerModel gesuchter_server = liste_von_server.Find(x => x.Name == "Ubuntu at VBox");

                // So erhält man den Ordnereintrag zu einem Ordnereintragsnamen
                FolderModel gesuchter_folder = gesuchter_server.Folders.Find(x => x.Name == "home vom user");

                // Editiern 
                Console.WriteLine("Editieren ...");
                    Console.ReadLine();

                // edit test
                ServerModel vbx_ubuntu = gesuchter_server;
                vbx_ubuntu.Name += " editiert";
                vbx_ubuntu.Port = 1337;
                vbx_ubuntu.Notes = "hui, tolle notizen";
                bone.editServer(vbx_ubuntu);

                Console.WriteLine("Editiert.");
                    Console.ReadLine();
                
                
                // edit test
                FolderModel home = gesuchter_folder;
                home.Name += " editiert";
                home.Note = "hui, noch tollere notizen";
                bone.editFolder(gesuchter_server.ID, home);

                Console.WriteLine("Editiert.");
                    Console.ReadLine();
                    return;
                // So Mountet man einen Eintrag
                bone.Mount(gesuchter_server.ID, gesuchter_folder.ID);
                Console.WriteLine("Gemounted, Enter drücken um Unmounten");
                Console.ReadLine();

                // So Unmountet man einen Eintrag
                bone.UMount(gesuchter_server.ID, gesuchter_folder.ID);

                // So löscht man einen Ordner
                //bone.removeFolder(gesuchter_server.ID, gesuchter_folder.ID);

                //So löscht man einen Server
                //bone.removeServer(gesuchter_server.ID);

                Console.WriteLine("\nIm Folgenden wir alles aus dem Datenmodell angezeigt.");
                liste_von_server = bone.listAll();
                foreach (ServerModel i in liste_von_server)
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

            Console.WriteLine("Enter drücken um Client zu beenden");
            Console.ReadLine();


        }
    }
}
