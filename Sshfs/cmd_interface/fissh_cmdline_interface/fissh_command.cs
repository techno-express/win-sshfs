using System;
using System.Collections.Generic;
using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using NDesk.Options;//for getopt

using icp_dummy;


namespace fissh_command
{

    enum fissh_command_keywords : byte { _no_match, mount, umount, status, help };

    /// <summary>
    /// class to parse and store options and parameters
    /// </summary>
    public class fissh_command_expression
    {

        public byte keyword;    //keyword comes from the first argument, eg. "mount", "umount", "help", "status"
        
        public bool any_option_is_set_flag;     //will ne true, if there is any option set
        
        // options are all arguments which look like "-l user" or "-p 22"
        public option option_login_name = new option();
        public option option_port = new option();
        public option option_key = new option();
        public option option_path = new option();
        public option option_drive = new option();
        public option option_virtual_drive = new option();
        
        // parameters are arguments without "-" or "/" as first letter, like "servername" or "folder1,folder2,folderx"
        public option parameter_host = new option();
        public option parameter_servername = new option();
        public option parameter_folderlist = new option();



        /// <summary>
        /// contructor; it will parse the given arguments
        /// </summary>
        /// <param name="args">arguments to parse</param>
        public fissh_command_expression(string[] args)
        {
            fetch_keyword(args);    //parse first argument

            // delete the fist argument, which should be the keyword
            List<string> args_list = args.ToList();
            args_list.RemoveAt(0);  //delete first element
            args = args_list.ToArray();
            
            // parse other arguments
            // first all options, then parameters
            fetch_parameters(fetch_options(args));  
        }


        /// <summary>
        /// Instanzes of this class are used to present Option or Parameters
        /// </summary>
        public class option
        {
            public string value; // to save the parameter
            public bool is_set_flag;    // will be true, if the parameter is used 

            public option()
            {
                is_set_flag = false;
                value = null;
            }

            public void set(string str)
            {
                is_set_flag = true;
                value = str;
            }

            public string get()
            {
                return value;
            }
        }


        #region METHODS_for_Argument_Parsing
        /// <summary>
        /// looks at the first Argument and looks for a matching keyword
        /// </summary>
        /// <param name="args">arguments to parse</param>
        private void fetch_keyword(string[] args)
        {   
            //if there are no arguments, exit
            if (args.Length == 0)
            {
                fissh_print.error_message("no arguments found");
                Environment.Exit(-1);
            }

            //else: compare first argument with defined keywords
            else
            {
                switch (args[0])
                {
                    case "mount": keyword = (byte)fissh_command_keywords.mount; return;
                    case "umount": keyword = (byte)fissh_command_keywords.umount; return;
                    case "status": keyword = (byte)fissh_command_keywords.status; return;
                    case "help": keyword = (byte)fissh_command_keywords.help; return;

                    default: 
                        if(args[0].Substring(0,1) == "-" || args[0].Substring(0,1) == "/")
                        {
                            fissh_print.error_message("no keyword found");
                        }
                        else
                        {
                            fissh_print.error_message("no matching keyword found");
                        }

                        Environment.Exit(-1);
                        return;

                }
            }
        }



        /// <summary>
        /// get all options from the arguments
        /// It uses NDesk.Options for parsing 
        /// 
        /// </summary>
        /// <param name="args">arguments to parse</param>
        /// <returns>those arguments, which are no options will be return</returns>
        private List<string> fetch_options(string[] args)
        {
            List<string> extra_parameters; // to store the arguments, which are not options

            any_option_is_set_flag = false;

            // OptionSet() from NDesk.Options
            // all possible options are definde here
            // {"option-switch", "Text for Help(not used here)", what to do when found}
            var p = new OptionSet() {
		   	    { "l=", "Login Name", v => {option_login_name.set(v); any_option_is_set_flag = true;} },
                { "p=", "Port", (int v) => {option_port.set(v.ToString()); any_option_is_set_flag = true;}  },
                { "k=", "Key for Authentification", v => {option_key.set(v); any_option_is_set_flag = true;}  },
                { "s=", "Path on Server", v => {option_path.set(v); any_option_is_set_flag = true;}  },
                { "d=", "Driveletter", v => {option_drive.set(v); any_option_is_set_flag = true;}  },
                { "v=", "Virtualdrive", v => {option_virtual_drive.set(v); any_option_is_set_flag = true;}  },
                //  {"<>", v => Console.WriteLine("unknown option: {0}", v) },
		    };


            try
            {
                extra_parameters = p.Parse(args);   //options are parsed, remaining arguments are returned
                
                //looking for unknown options in remaining arguments
                //if there is one, close programe
                for (int i = 0; i < extra_parameters.Count; i++)
                {
                    if (extra_parameters[i].Substring(0, 1) == "-" || extra_parameters[i].Substring(0, 1) == "/")
                    {
                        Console.WriteLine("fissh: unknown option {0}", extra_parameters[i]);
                        fissh_print.error_message("unknown option " + extra_parameters[i]);
                        Environment.Exit(-1);
                    }
                }

                //otherwise return remainig arguments
                return extra_parameters;

            }
            catch (OptionException e)   //eg. when Port is no INT
            {
                //Console.Write("fissh: ");
                //Console.WriteLine(e.Message);
                //Console.WriteLine("Try `fissh help' for more information.");
                fissh_print.error_message(e.Message);
                Environment.Exit(-1);
                return null; //useless, but VS is complaning whithout return
            }


        }


        /// <summary>
        /// for parsing arguments, which are neither a keyword nor options
        /// those arguments are parameters like servername etc.
        /// </summary>
        /// <param name="parameters">remainig arguments</param>
        private void fetch_parameters(List<string> parameters)
        {
            switch (keyword)    //uses of arguments depends on the keyword 
            {
                case (byte)fissh_command_keywords.mount:


                    //User wants to mount a new server
                    if (any_option_is_set_flag)
                    {
                        int i_start, i_end;     //indices to isolate host from username and port (user@host:port)
                        string puffer_login_name, puffer_port;

                        // If there is no parameter
                        if (parameters.Count() == 0)
                        {
                            fissh_print.error_message("fissh: missing parameter");
                            Environment.Exit(-1);
                        }

                        //isolate username
                        i_start = parameters[0].IndexOf("@");
                        if (i_start == -1)      //if there is no "@"
                        {
                            puffer_login_name = null;
                            i_start = 0;        //so hostname starts with first char
                        }
                        else
                        {
                            puffer_login_name = parameters[0].Substring(0, i_start);
                            i_start++;
                        }

                        //isolate portnumber
                        i_end = parameters[0].IndexOf(":");
                        if (i_end == -1)    //if ther is no ":"
                        {
                            puffer_port = null;
                            i_end = parameters[0].Length - 1;   //so hostname ends with last char
                        }
                        else
                        {
                            puffer_port = parameters[0].Substring(i_end + 1);
                        }


                        //if username is not already set by option, use username from parameter 
                        if (!option_login_name.is_set_flag)
                        {
                            option_login_name.set(puffer_login_name);
                        }

                        //if portnumber is not already set by option, use portnumber from parameter 
                        if (!option_port.is_set_flag)
                        {
                            option_port.set(puffer_port);
                        }

                        //isolate hostname
                        parameter_host.set(parameters[0].Substring(i_start, i_end - i_start + 1));
                    }

                    //User wants to mount an existing server
                    else
                    {
                        int i = parameters.Count();
                        if (i >= 1)
                        {
                            parameter_servername.set(parameters[0]);    //first argument is servername
                        }
                        if (i >= 2)
                        {
                            parameter_folderlist.set(parameters[1]);    //if there is a secound, it is the folderlist
                        }

                    }
                    break;
                default: return;
            }
        }
        #endregion
    }

    /// <summary>
    /// class for all methods to use ipc to fisshbone
    /// </summary>
    public static class actions
    {
        public static void mount_complet_server(fissh_command_expression arguments)
        {
            int server_id;
            List<int> folder_ids;
            server_id = icp.search_server(arguments.parameter_servername.get());

            folder_ids = icp.get_folder_ids(server_id);

            folder_ids.ForEach(delegate(int i)
            {
                icp.mount(server_id, i);
            });
        }

        public static void mount_registered_folders(fissh_command_expression arguments)
        {
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
                icp.mount(server_id, i);
            }); 
        }
    }

    /// <summary>
    /// class for output methods
    /// </summary>
    public static class fissh_print 
    {
        public static void error_message(string error_message)
        {
            Console.WriteLine("");
            Console.WriteLine("fissh: {0}", error_message);
            Console.WriteLine();
            Console.WriteLine("Usage:");
            //Console.WriteLine("");
            Console.WriteLine("To mount a drive");
            Console.WriteLine("     fissh mount SERVERNAME [FOLDERLIST]");
            Console.WriteLine("     fissh mount [-l user] [-p port] [-k password=...| public_key=...]"); 
            Console.WriteLine("                 -s PATH -d DRIVE-LETTER [user@]host[:port]");
            Console.WriteLine("");
            Console.WriteLine("To unmount a drive");
            Console.WriteLine("     fissh umount DRIVE-LETTER | -v VIRTUAL_DRIVE_FOLDER");
            Console.WriteLine("     fissh umount SERVERNAME [FOLDERLIST]");
            Console.WriteLine("");
            Console.WriteLine("Ask if a drive is mounted");
            Console.WriteLine("     fissh status DRIVE-LETTER | -v VIRTUAL_DRIVE_FOLDER");
            Console.WriteLine("     fissh status SERVERNAME FOLDER");
            Console.WriteLine("");
            Console.WriteLine("For more informations");
            Console.WriteLine("     fissh help");
            Console.WriteLine("");

        }
    }

}