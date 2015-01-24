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

        // this flag will be set false if programm could not mount or unmount a drive
        public static bool no_mounting_error = true;

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

        #region mount
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
        public static void mount_complete_server()
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
         * ex.: >fissh mount -s /home/user -d Z: -k password=3dj92d user@93.184.216.34:22
         * 
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
            server.Type = fissh_command_expression.option_key.type;

            switch (fissh_command_expression.option_key.type)
            {
                case Sshfs.ConnectionType.Password:
                    server.Password = fissh_command_expression.option_key.get();
                    break;

                case Sshfs.ConnectionType.PrivateKey:
                    server.PrivateKey = fissh_command_expression.option_key.get();
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
                string error_message = "While mounting " + folder.Folder + " on " + server.Host + ": " + e.Detail.Message;
                throw new System.ComponentModel.WarningException(error_message);
            }
        }
        #endregion

        #region umount
        /// unmount a complete server
        /**
         * This method looks for all folders with the status "mounted"
         * of one given server and unmounts them all.
         * ex: >fissh unmount Servername
         * 
         * 
         * @param   arguments   parsed arguments in a fissh_command_expression object
         */
        public static void umount_complete_server()
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
            Tuple<Guid, Guid> ids = null;

            try
            {
                Init();

                string virtual_drive_folder = fissh_command_expression.option_virtual_drive.get();
                ids = bone_server.GetVirtualDriveUsage(virtual_drive_folder);

                if (ids.Item1 == Guid.Empty && ids.Item2 == Guid.Empty)
                {
                    fissh_print.simple_error_message("Nothing mounted at virtual drive" + virtual_drive_folder + ".");
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
        #endregion

        #region status
        /// show all folders of a server and if there are mounted
        /**
         * 
         * @return  if any folder is not mounted or mounting 
         * it will return unmounted, else mounted
         */
        public static Sshfs.DriveStatus status_complete_server() 
        {
            ServerModel server;
            Sshfs.DriveStatus return_value = Sshfs.DriveStatus.Mounted;

            try
            {
                Init();

                server = name2server(fissh_command_expression.parameter_servername.get());

                foreach (FolderModel i in server.Folders)
                {
                    if (i.Status != Sshfs.DriveStatus.Mounted && i.Status != Sshfs.DriveStatus.Mounting)
                    {
                        return_value = Sshfs.DriveStatus.Unmounted;
                    }
                    fissh_print.simple_output_message("Folder " + i.Name + " is " + i.Status.ToString() + ".");
                }
                return return_value;

            }
            catch (Exception e)
            {
                throw e;
            }
  
        }

        /// shows if the given folders are mounted or not
        /**
         * 
         * @return  if any folder is not mounted or mounting 
         * it will return unmounted, else mounted
         */
        public static Sshfs.DriveStatus status_registered_folders()
        {
            ServerModel server;
            List<FolderModel> folders = new List<FolderModel>();
            List<string> folder_names = new List<string>();
            Sshfs.DriveStatus return_value = Sshfs.DriveStatus.Mounted;


            try
            {
                Init();
                
                server = name2server(fissh_command_expression.parameter_servername.get());
                folder_names = fissh_command_expression.parameter_folderlist.get().Split(',').ToList();

                foreach (string i in folder_names)
                {
                    folders.Add(name2folder(server, i));
                }

                foreach (FolderModel i in folders)
                {
                    if (i.Status != Sshfs.DriveStatus.Mounted && i.Status != Sshfs.DriveStatus.Mounting)
                    {
                        return_value = Sshfs.DriveStatus.Unmounted;
                    }
                    fissh_print.simple_output_message("Folder " + i.Name + " is " + i.Status.ToString() + ".");
                }
                return return_value;
            }
            catch (Exception e)
            {
                throw e;
            }
 
        }

        /// shows which folder is mounted under given drive letter
        public static Sshfs.DriveStatus status_driveletter() 
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
                    return Sshfs.DriveStatus.Unmounted;
                }
                else
                {
                    ServerModel server;
                    FolderModel folder;
                    try
                    {
                        server = all_data.Find(x => x.ID == ids.Item1);
                        folder = server.Folders.Find(x => x.ID == ids.Item2);
                        fissh_print.simple_output_message(
                            "Folder " + folder.Name + " on server " + server.Name +
                            " mounted under drive letter " + letter +":." +
                            " Its status is " + folder.Status.ToString() + ".");
                        return folder.Status;
                    }
                    catch (NullReferenceException w)
                    {
                        fissh_print.simple_output_message(
                            "Unregistered server " +
                            "mounted under drive letter " + letter + ":.");
                        return Sshfs.DriveStatus.Mounted;
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
 
 
        }

        /// shows which folder is mounted under given virtual drive
        public static Sshfs.DriveStatus status_virtualdrive() 
        {
            throw new Exception("not implemented yet.");
        }

        #endregion

        #region tools
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
                ServerModel server = all_data.Find(x => x.Name == servername);

                if (server == null)
                {
                    string message = "Cannot find server with name " + fissh_command_expression.parameter_servername.get();
                    logger.log.writeLog(Loglevel.Warning, log_cmpnt, message);
                    throw new System.ComponentModel.WarningException(message);
                }
                else
                {
                    return server;
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
                FolderModel folder = server.Folders.Find(x => x.Name == foldername);

                if (folder == null)
                {
                    string message = "Cannot find folder with name " + foldername +
                        " in " + server.Name;
                    logger.log.writeLog(Loglevel.Warning, log_cmpnt, message);
                    throw new Exception(message);
                }
                else
                {
                    return folder;
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
                // try for every element so you can go on if there is an error with one drive 
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
                    string error_message = "While mounting " + i.Item1.Name + " on " + i.Item2.Name +
                                        ": " + e.Detail.Message;
                    logger.log.writeLog(SimpleMind.Loglevel.Warning, "cmdline", error_message);
                    fissh_print.simple_error_message(error_message);

                    no_mounting_error = false;
                }
                catch (Exception e)
                {
                    throw e;
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
                    string error_message = "While unmounting " + i.Item1.Name + " on " + i.Item2.Name +
                                        ": " + e.Detail.Message;
                    logger.log.writeLog(SimpleMind.Loglevel.Warning, "cmdline", error_message);
                    fissh_print.simple_error_message(error_message);
                    no_mounting_error = false;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        #endregion
    }
}
