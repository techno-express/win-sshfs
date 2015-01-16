using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



//using icp_dummy;

using Sshfs.GuiBackend.Remoteable;
using Sshfs.GuiBackend;
using System.ServiceModel;

using SimpleMind;



namespace fissh_cmdline_interface
{
    /// static class for all methods to use the ipc interface to fisshbone
    /**
     * This class is completely static. Public methods take an fissh_command_expression
     * object as argument where are all information they need to mount a complete server 
     * for example.
     */
    public static class actions
    {
        private const string log_cmpnt = "cmdline:actions";
        private static IServiceFisshBone bone_server = null;
        private static List<ServerModel> all_data = null;
        private static List<Tuple<ServerModel, FolderModel>> to_mount = new List<Tuple<ServerModel,FolderModel>>();
        private static List<Tuple<ServerModel, FolderModel>> to_umount = new List<Tuple<ServerModel,FolderModel>>();


        /// init methods
        /**
         * This method is called by other methods first.
         * It checks if there is already a connection to the IPC-Server.
         * If there is not it will be made.
         */
        private static void Init()
        {
            if (bone_server == null)
            {
                bone_server = IPCConnection.ClientConnect();
            }

            if (all_data == null)
            {
                all_data = bone_server.listAll();
            }

            return;
        }


        /// mount a complete server
        /**
         * If you got on server but no folderlist in parameters
         * you should mount every every folder of this server.
         * ex: >fissh Servername
         * 
         * This method gets all server and folder data from backend,
         * searches for the server name. It sends a mount command for
         * every folder of the server.
         * 
         * 
         * @param   arguments   parsed arguments in a fissh_command_expression object
         */
        public static void mount_complet_server()
        {
            ServerModel server;

            try
            {
                Init();

                server = name2server(fissh_command_expression.parameter_servername.get());

                foreach (FolderModel i in server.Folders)
                {
                        to_mount.Add(new Tuple<ServerModel, FolderModel>(server, i));
                }

                mount_them();
            }
            catch (Exception e)
            {
                throw e;
            }
       }

        /// mount a folders of one server
        /**
         * If you got a server name and one or more folders 
         * you should mount every named folder on this server.
         * ex: >fissh Servername Folder1,Folder2,Folder3
         * 
         * This method ... 
         *   1. pulls all the ServerModel list.
         *   2. searchs for the server name 
         *   3. searches for names folders in this server
         *   4. sends mount command to backend for every single folder
         * 
         * If any name cannot be found in ServerModel list the method will throw an
         * Exception which includes an human readable error message.
         *
         *  
         * @param   arguments   parsed arguments in a fissh_command_expression object
         */
        public static void mount_registered_folders()
        {
            ServerModel server;
            List<FolderModel> folders = new List<FolderModel>();
            List<string> folder_names;


            try
            {
                Init();

                server = name2server(fissh_command_expression.parameter_servername.get());
                folder_names = fissh_command_expression.parameter_folderlist.get().Split(',').ToList();

                foreach (string i in folder_names)
                {
                    to_mount.Add(new Tuple<ServerModel,FolderModel>(server, name2folder(server, i)));
                }

                mount_them();
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        /// mount a unregistered folder
        /**
         * The cmd line interface is able to mount a remote directory
         * which is not already listed in Backend.
         * ex.: >fissh mount user@93.184.216.34:22 -s /home/user -d Z: -k password=3dj92d
         * 
         * (not implemented yet)
         * 
         * @param   arguments   parsed arguments in a fissh_command_expression object
         */
        public static void mount_unregistered_folder()
        {
            ServerModel server = new ServerModel();
            FolderModel folder = new FolderModel();

            server.Host = fissh_command_expression.parameter_host.get();
            server.Port = fissh_command_expression.option_port.get();
            server.Username = fissh_command_expression.option_login_name.get();

            switch (fissh_command_expression.option_key.type)
            {
                case Sshfs.ConnectionType.Password:
                    server.Password = fissh_command_expression.option_key.get();
                    break;
                    
                default:
                    throw new Exception("not implemented yet");
            }

            folder.use_global_login = true;
            folder.Letter = fissh_command_expression.option_letter.get().ToCharArray()[0];
            folder.Folder = fissh_command_expression.option_path.get();
            server.Folders.Add(folder);
            Init();

            try
            {
                bone_server.UnregisteredMount(server);
            }
            catch (FaultException<Fault> e)
            {
                throw new Exception(e.Detail.Message);
            }
        }


        /// unmount a complete server
        /**
         * This method looks for all folders with the status "mounted"
         * of one given server and unmounts them all.
         * ex: >fissh unmount Servername
         * 
         * 
         * @param   arguments   parsed arguments in a fissh_command_expression object
         */
        public static void umount_complet_server()
        {
            ServerModel server;

            try
            {
                Init();

                server = name2server(fissh_command_expression.parameter_servername.get());

                logger.log.writeLog(Loglevel.Debug, log_cmpnt, "Adding folders of " + server.Name +" to unmount list:");
                foreach (FolderModel i in server.Folders)
                {
                    to_umount.Add(new Tuple<ServerModel, FolderModel>(server, i));
                    logger.log.writeLog(Loglevel.Debug, log_cmpnt, "\tAdded folder " + i.Name + " to unmount list.");
                }

                umount_them();
            }
            catch (Exception e)
            {
                throw e;
            }
      }



        /// unmount one or more folders of a server
        /**
         * This method sends unmount commands for every listed folder of the given server.
         * ex.: fissh unmount Servername Folder1,Folder2,Folder3
         * 
         * It should work similar to mount_registered_folders()
         *
         * (This method does not work with Interface yet)
         * 
         * @param   arguments   parsed arguments in a fissh_command_expression object
         */
        public static void umount_registered_folders()
        {
            ServerModel server;
            List<FolderModel> folders = new List<FolderModel>();
            List<string> folder_names;


            try
            {
                Init();
                
                server = name2server(fissh_command_expression.parameter_servername.get());
                folder_names = fissh_command_expression.parameter_folderlist.get().Split(',').ToList();

                foreach (string i in folder_names)
                {
                    to_umount.Add(new Tuple<ServerModel, FolderModel>(server, name2folder(server, i)));
                }

                umount_them();
            }
            catch (Exception e)
            {
                throw e;
            }
       }

        /// unmount a connection which is given by a driveletter
        /**
         * If a connection is already mounted you can unmounted it by giving its mountpoit.
         * ex: >fissh unmount Z:
         * 
         * 
         * @param   arguments   parsed arguments in a fissh_command_expression object
         */
        public static void umount_driveletter()
        {
            Tuple<Guid, Guid> ids = null;


            try
            {
                Init();

                char letter = fissh_command_expression.option_letter.get().ToCharArray()[0];
                ids = bone_server.GetLetterUsage(letter);

                if (ids.Item1 == Guid.Empty && ids.Item2 == Guid.Empty)
                {
                    fissh_print.simple_error_message("Nothing mounted at " + letter + ":");
                }
                else
                {
                    ServerModel server;
                    FolderModel folder;
                    try
                    {
                        server = all_data.Find(x => x.ID == ids.Item1);
                        folder = server.Folders.Find(x => x.ID == ids.Item2);
                    }
                    catch (NullReferenceException w)
                    {
                        server = new ServerModel();
                        server.ID = ids.Item1;
                        folder = new FolderModel();
                        folder.ID = ids.Item2;
                        server.Folders.Add(folder);
                    }
                    folder.Status = Sshfs.DriveStatus.Mounted;
                    to_umount.Add(new Tuple<ServerModel, FolderModel>(server, folder));
                    umount_them();
                }
           }
            catch (Exception e)
            {
                throw e;
            }
 
        }

        /// unmount a connection which is given by its virtual drive folder
        /**
         * If a connection is already mounted you can unmounted it by giving its mountpoit.
         * ex: >fissh unmount Y:\Folder1
         *
         *  
         * (This method does not work with Interface yet)
         * 
         * 
         * @param   arguments   parsed arguments in a fissh_command_expression object
         */
        public static void umount_virtualdrive()
        {
            throw new Exception("not implemented yet");
            /*Tuple<int, int> puffer_tupel;
            int server_id, folder_id;

            puffer_tupel = icp.search_virtualdrive(arguments.option_virtual_drive.get());
            server_id = puffer_tupel.Item1;
            folder_id = puffer_tupel.Item2;

            if (server_id > 0 && folder_id > 0)
            {
                icp.umount(server_id, folder_id);
            }*/
        }

        /// find server to given name
        /**
         * This mehtod gets a name of a server and searches
         * for a proper ServerModel and returns this ServerModel.
         * If there is no such ServerModel a exception will be thrown.
         * 
         * @param servername    name of wanted server
         * 
         * @return wanted ServerModel 
         * 
         */
        private static ServerModel name2server(string servername)
        {
            try
            {
                return all_data.Find(x => x.Name == servername);
            }
            catch (NullReferenceException error)
            {
                string message = "Cannot find server with name " + fissh_command_expression.parameter_servername.get();
                logger.log.writeLog(Loglevel.Warning, log_cmpnt, message);
                throw new Exception(message);
            }
        }

        /// find folder to given name
        /**
         * This method gets a ServerModel and a folder name.
         * It searches in given ServerModel for a folder named like given parameter
         * and the found FolderModel.
         * If there is no such FolderModel in given ServerModel a exception will be thrown.
         * 
         * @param server        ServerModel in which you want to search a folder
         * @param foldername    name of wanted folder
         * 
         * @return wanted FolderModel
         */
        private static FolderModel name2folder(ServerModel server, string foldername)
        {
            try
            {
                return server.Folders.Find(x => x.Name == foldername);
            }
            catch (NullReferenceException error)
            {
                string message = "Cannot find folder with name " + fissh_command_expression.parameter_servername.get() +
                    " in " + server.Name;
                logger.log.writeLog(Loglevel.Warning, log_cmpnt, message);
                throw new Exception(message);
            } 
        }

        /// mount all folders in "to mount" list
        /**
         * mount_them() will mount every folder listed in to_mount list.
         * Other methods, who want to mount anything, 
         * will put a Tuple of a ServerModel and a FolderModel and this method.
         * mount_them will mount every entry of this list by sending their ids to the backend.
         */
        private static void mount_them()
        {
            foreach (Tuple<ServerModel,FolderModel> i in to_mount)
            {
                try
                {
                    if (i.Item2.Status != Sshfs.DriveStatus.Mounted &&
                        i.Item2.Status != Sshfs.DriveStatus.Mounting)
                    {
                        bone_server.Mount(i.Item1.ID, i.Item2.ID);
                        logger.log.writeLog(Loglevel.Debug, log_cmpnt, "Mounted folder " + i.Item2.Name + " on server " + i.Item1.Name);
                    }
                    else
                    {
                        fissh_print.simple_error_message(i.Item2.Name + " on " + i.Item1.Name + " is already mounted");
                    }
                    
                }
                catch (FaultException<Fault> e)
                {
                    string message = "While mounting " + i.Item1.Name + " on " + i.Item2.Name +
                                        ": " + e.Detail.Message;
                    logger.log.writeLog(Loglevel.Warning, log_cmpnt, message);
                    throw new Exception(message);
                }
            }
        }




        /// unmount all folders in "to unmount" list
        /**
         * umount_them() will unmount every folder listed in to_umount list.
         * Other methods, who want to unmount anything, 
         * will put a Tuple of a ServerModel and a FolderModel and this method.
         * umount_them will unmount every entry of this list by sending their ids to the backend.
         */
     
        private static void umount_them()
        {
            foreach (Tuple<ServerModel,FolderModel> i in to_umount)
            {
                try
                {
                    if (i.Item2.Status != Sshfs.DriveStatus.Unmounted &&
                        i.Item2.Status != Sshfs.DriveStatus.Unmounting)
                    {
                        bone_server.UMount(i.Item1.ID, i.Item2.ID);
                        logger.log.writeLog(Loglevel.Debug, log_cmpnt, "Unmounted folder " + i.Item2.Name + " on server " + i.Item1.Name);
                    }
                    else
                    {
                        fissh_print.simple_error_message(i.Item2.Name + " on " + i.Item1.Name + " is not mounted");
                    }
                    
                }
                catch (FaultException<Fault> e)
                {
                    string message = "While unmounting " + i.Item1.Name + " on " + i.Item2.Name +
                                        ": " + e.Detail.Message;
                    logger.log.writeLog(Loglevel.Warning, log_cmpnt, message);
                    throw new Exception(message);
                }
            }
        }
    }
}
