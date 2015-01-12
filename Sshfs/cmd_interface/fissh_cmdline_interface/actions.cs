using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



//using icp_dummy;

using Sshfs.GuiBackend.Remoteable;
using Sshfs.GuiBackend;
using System.ServiceModel;



namespace fissh_command
{
    /// static class for all methods to use the ipc interface to fisshbone
    /**
     * This class is completely static. Public methods take an fissh_command_expression
     * object as argument where are all information they need to mount a complete server 
     * for example.
     */
    public static class actions
    {
        private static IServiceFisshBone bone_server = null;


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
         * (This method does not work with Interface yet)
         * 
         * 
         * @param   arguments   parsed arguments in a fissh_command_expression object
         */
        public static void mount_complet_server(fissh_command_expression arguments)
        {
            throw new Exception("not implemented yet");
            /*int server_id;
            List<int> folder_ids;
            server_id = icp.search_server(arguments.parameter_servername.get());

            folder_ids = icp.get_folder_ids(server_id);

            folder_ids.ForEach(delegate(int i)
            {
                icp.mount(server_id, i);
            });*/
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
        public static void mount_registered_folders(fissh_command_expression arguments)
        {
            List<ServerModel> all_data;
            ServerModel server;
            List<FolderModel> folders = new List<FolderModel>();
            List<string> folder_names;




            try
            {
                Init();
                all_data = bone_server.listAll();

                try
                {
                    server = all_data.Find(x => x.Name == arguments.parameter_servername.get());
                }
                catch (NullReferenceException error)
                {
                    throw new NullReferenceException(
                        "Cannot find server with name " + arguments.parameter_servername.get());
                }

                folder_names = arguments.parameter_folderlist.get().Split(',').ToList();

                foreach (string i in folder_names)
                {
                    try
                    {
                        folders.Add(server.Folders.Find(x => x.Name == i));
                    }
                    catch (NullReferenceException error)
                    {
                        throw new NullReferenceException(
                               "Cannot find folder with name " + i);
                    }
                }


                foreach (FolderModel i in folders)
                {
                    try
                    {
                        bone_server.Mount(server.ID, i.ID);
                    }
                    catch (FaultException<Fault> e)
                    {
                        throw new Exception("While mounting " + server.Name + " on " + i.Name +
                                            ": " + e.Detail.Message);
                    }
                }

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
        public static void mount_unregistered_folder(fissh_command_expression arguments)
        {
            throw new Exception("not implemented yet");
        }


        /// unmount a complete server
        /**
         * This method looks for all folders with the status "mounted"
         * of one given server and unmounts them all.
         * ex: >fissh unmount Servername
         * 
         * (This method does not work with Interface yet)
         * 
         * 
         * @param   arguments   parsed arguments in a fissh_command_expression object
         */
        public static void umount_complet_server(fissh_command_expression arguments)
        {
            throw new Exception("not implemented yet");
            /*
            int server_id;
            List<int> folder_ids;
            server_id = icp.search_server(arguments.parameter_servername.get());

            folder_ids = icp.get_folder_ids(server_id);

            folder_ids.ForEach(delegate(int i)
            {
                icp.umount(server_id, i);
            });
             */
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
        public static void umount_registered_folders(fissh_command_expression arguments)
        {
            throw new Exception("not implemented yet");
            /*
            int server_id;
            List<string> folder_names;
            List<int> folder_ids = new List<int>();
            server_id = icp.search_server(arguments.parameter_servername.get());

            folder_names = arguments.parameter_folderlist.get().Split(',').ToList();

            folder_names.ForEach(delegate(string puffer)
            {
                folder_ids.Add(icp.search_folder(puffer, server_id));
            });

            folder_ids.ForEach(delegate(int i)
            {
                icp.umount(server_id, i);
            });*/
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
        public static void umount_driveletter(fissh_command_expression arguments)
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
        public static void umount_virtualdrive(fissh_command_expression arguments)
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





    }
}
