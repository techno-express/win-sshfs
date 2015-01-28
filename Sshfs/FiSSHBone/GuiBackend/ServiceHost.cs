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
            /*
            // WCF-Server einrichten
            using (
            ServiceHost bone_server = new ServiceHost(
                typeof(ServiceFisshBone),
                new Uri[]{
          new Uri("net.pipe://localhost")
        });
             */
            {
            /*    bone_server.AddServiceEndpoint(typeof(IServiceFisshBone),
                  new NetNamedPipeBinding(),
                  "FiSSH");
                bone_server.Open();
             */
            ServiceHost bone_server = IPCConnection.ServerConnect();
                
                // um statische elemente zu bearneiten
                ServiceFisshBone bone_local = new ServiceFisshBone();

                ServiceFisshBone.Init();

                //Ein Server mit Ordner zum Testen ins Datenmodel einfügen
                ServerModel ein_server = new ServerModel();
                ein_server.Name = "Ubuntu at VBox";
                ein_server.Host = "10.0.2.13";
                ein_server.Port = 22;
                ein_server.Password = "user";
                ein_server.Type = ConnectionType.Password;
                ein_server.Username = "user";
                Guid ein_server_ID = bone_local.addServer(ein_server);

                FolderModel ein_folder = new FolderModel();
                ein_folder.Letter = 'G';
                ein_folder.use_global_login = false;
                ein_folder.Folder = "/home/user/ein";
                ein_folder.Name = "home vom user";
                ein_folder.Type = ConnectionType.PrivateKey;
                ein_folder.PrivateKey = "C:\\nopass";
                ein_folder.Username = "user";
                ein_folder.Note = "Folder with private key";
                Guid ein_folder_ID = bone_local.addFolder(ein_server_ID, ein_folder);

                ein_folder = new FolderModel();
                ein_folder.Letter = 'T';
                ein_folder.use_global_login = true;
                ein_folder.Folder = "/home/user/zwei";
                ein_folder.Name = "home zwei";
                ein_folder.Note = "Folder with virtual drive I";
                ein_folder.VirtualDriveFolder = "vd_zwei";
                ein_folder.use_virtual_drive = true;
                ein_folder_ID = bone_local.addFolder(ein_server_ID, ein_folder);

                ein_folder = new FolderModel();
                ein_folder.Letter = 'T';
                ein_folder.use_global_login = true;
                ein_folder.Folder = "/home/user/drei";
                ein_folder.Name = "home drei";
                ein_folder.Note = "Folder with virtual drive II";
                ein_folder.VirtualDriveFolder = "vd_zwei";
                ein_folder.use_virtual_drive = true;
                ein_folder_ID = bone_local.addFolder(ein_server_ID, ein_folder);

                ein_folder = new FolderModel();
                ein_folder.Letter = 'U';
                ein_folder.use_global_login = true;
                ein_folder.Folder = "/home/user/drei";
                ein_folder.Name = "home drei";
                ein_folder_ID = bone_local.addFolder(ein_server_ID, ein_folder);

                
                ein_server = new ServerModel();
                ein_server.Name = "Ubuntu mit falscher IP";
                ein_server.Host = "10.0.2.10";
                ein_server.Port = 22;
                ein_server.Password = "user";
                ein_server.Username = "user";
                ein_server_ID = bone_local.addServer(ein_server);

                ein_folder = new FolderModel();
                ein_folder.Letter = 'G';
                ein_folder.use_global_login = true;
                ein_folder.Folder = "/home/user";
                ein_folder.Name = "home vom user";
                ein_folder_ID = bone_local.addFolder(ein_server_ID, ein_folder);



                string puffer = "";
                while (puffer != "q")
                {
                    
                    if (puffer == "w")
                    {
                        ServerModel server1 = bone_local.listAll().Find(x => x.Name == "Ubuntu at VBox");
                        ServerModel server2 = bone_local.listAll().Find(x => x.Name == "Ubuntu mit falscher IP");

                        Guid id1 = server1.Folders.Find(x => x.Name == "home zwei").ID;
                        Guid id2 = server2.Folders.Find(x => x.Name == "home drei").ID;
                        bone_local.MoveFolderAfter(server1.ID, server2.ID, id1, id2);
                    }
                    if (puffer == "e")
                    {
                        ein_folder.Name += " eee";
                        bone_local.editFolder(ein_server_ID, ein_folder);
                    }
                    if(puffer == "r1")
                    {
                        bone_local.removeFolder(ein_server_ID, ein_folder_ID);
//                        bone_local.duplicateFolder(ein_server_ID, ein_folder_ID);
                    }
                    if(puffer == "r2")
                    {
                        bone_local.removeServer(ein_server_ID);
//                        bone_local.duplicateFolder(ein_server_ID, ein_folder_ID);
                    }
                    if(puffer == "i1")
                    {
                        ServerModel tmp = new ServerModel();
                        tmp.Name = "eingefügt";
                        bone_local.addServer(tmp);
                    }
                    if (puffer == "i2")
                    {
                        FolderModel tmp = new FolderModel();
                        tmp.Name = "eingefügt";
                        bone_local.addFolder(ein_server_ID, tmp);
                    }
                    if (puffer == "d1")
                    {
                        bone_local.duplicateServer(ein_server_ID);
                    }
                    if (puffer == "d2")
                    {
                        bone_local.duplicateFolder(ein_server_ID, ein_folder_ID);
                    }

                    if (puffer == "s1")
                    {
                        bone_local.SaveServerlist(@"c:\user\thomas\");
                    }

                    foreach (ServerModel i in bone_local.listAll())
                    {
                        Console.WriteLine("\n" + i.Name + " --- " + i.ID);

                        foreach (FolderModel j in i.Folders)
                        {
                            Console.WriteLine("    " + j.Name + " --- " + j.ID);
                        }
                    }

                    Console.WriteLine("Please enter \"q\" to stop the server");
                    puffer = Console.ReadLine();
                }
                bone_server.Close();
            }


        }
}
