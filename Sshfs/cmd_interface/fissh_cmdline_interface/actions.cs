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
            throw new Exception("not implemented yet");
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
         * (This method does not work with Interface yet)
         * 
         * @param   arguments   parsed arguments in a fissh_command_expression object
         */
        public static void umount_driveletter()
        {
            throw new Exception("not implemented yet");
            /*Tuple<int, int> puffer_tupel;
            int server_id, folder_id;

            puffer_tupel = icp.search_driveletter(arguments.option_drive.get());
            server_id = puffer_tupel.Item1;
            folder_id = puffer_tupel.Item2;

            if (server_id > 0 && folder_id > 0)
            {
                icp.umount(server_id, folder_id);
            }*/
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
                        fissh_print.simple_error_message(i.Item2.Name + " on " + i.Item1.Name + " is already mounted");
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
