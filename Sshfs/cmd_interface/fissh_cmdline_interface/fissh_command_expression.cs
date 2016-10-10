using System;
using System.Collections.Generic;
using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using NDesk.Options;//for getopt




namespace fissh_cmdline_interface
{

    enum fissh_command_keywords : byte { _no_match, mount, umount, status, help };

    /// class to parse and store options and parameters
    /**
     * A Instanz of this class gets all shell arguments with its contructor
     * and parse them. It checks also the Syntax of the arguments.
     * The only private method is the contructor which handles everything else.
     * 
     * If there is any error with arguments an Exception will be thrown, 
     * no messages will be printed at IO
     * 
     */
    public static class fissh_command_expression
    {

        public static byte keyword;    //keyword comes from the first argument, eg. "mount", "umount", "help", "status"
        
        public static bool any_option_is_set_flag;     //will ne true, if there is any option set
        
        // options are all arguments which look like "-l user" or "-p 22"
        public static option option_login_name = new option();
        public static int_option option_port = new int_option();
        public static option option_path = new option();
        public static option option_letter = new option();
        public static option option_virtual_drive = new option();
        public static key_option option_key = new key_option();
        
        // parameters are arguments without "-" or "/" as first letter, like "servername" or "folder1,folder2,folderx"
        public static option parameter_host = new option();
        public static option parameter_servername = new option();
        public static option parameter_folderlist = new option();

        


        /// contructor - it will parse the given arguments
        /**
         * This contructor gets all arguments as array like Main()
         * It will handle all parsing actions.
         * 
         * Attention: first argument is not the programm name in C#
         *          ex: "> fissh mount" -> args[0] = "mount";
         * 
         * @param args  a string array with all arguments
         */
        public static void parse(string[] args)
        {
            fetch_keyword(args);    //parse first argument

            //delete the fist argument, which should be the keyword
            List<string> args_list = args.ToList();
            args_list.RemoveAt(0); 
            args = args_list.ToArray();
            
            // parse other arguments
            // first all options, then parameters
            fetch_parameters(fetch_options(args));  
        }


        /// Instanzes of this class are used to present Option or Parameters
        /**
         * It stores if the option is presented in argument
         * list and the value of the option itself 
         */
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
                if (str != null)
                {
                    is_set_flag = true;
                    value = str;
                }
            }

            public string get()
            {
                return value;
            }
        }

        public class key_option : option
        {
            public Sshfs.ConnectionType type;

            public key_option()
            {
                is_set_flag = false;
                value = null;
            }
        }

        public class int_option
        {
            private int value;
            public bool is_set_flag;

            public int_option()
            {
                is_set_flag = false;
                value = 0;
            }

            public void set(int i)
            {
                if (i != null)
                {
                    is_set_flag = true;
                    value = i;
                }
            }

            public int get()
            {
                return value;
            }
        }


        #region METHODS_for_Argument_Parsing
        /// looks at the first Argument and looks for a matching keyword
        /**
         * This methods looks for the first argument which is the keyword.
         * If the keyword does not match to the available ones it will throw an Exception
         * 
         * @param args      arguments in a string array as given by Main()
         * 
         */
        private static void fetch_keyword(string[] args)
        {   
            //if there are no arguments, exit
            if (args.Length == 0)
            {
                throw new Exception("no arguments found");
            }

            else
            {
                switch (args[0])
                {
                    case "mount": keyword = (byte)fissh_command_keywords.mount; return;
                    case "umount": keyword = (byte)fissh_command_keywords.umount; return;
                    case "status": keyword = (byte)fissh_command_keywords.status; return;
                    case "help": keyword = (byte)fissh_command_keywords.help; return;

                    default:
                        // if first argument looks like a option or a path
                        if(args[0].Substring(0,1) == "-" || args[0].Substring(0,1) == "/")
                        {
                            throw new Exception("no keyword found");
                        }
                        else
                        {
                            throw new Exception("no matching keyword found");
                        }
                }
            }
        }



        /// fetch all options from arguments
        /**
         * This method parse all options.
         * The option value will be stored in option objects
         * which are definded global in this class
         * 
         * It uses NDesk.Options for parsing http://www.ndesk.org/Options
         * 
         * If there is an unknown option a it will throw an Exception.
         * Every argument which is not an option will be put in a string list 
         * and returned. These arguments will be parsed by an other method.
         * 
         * The argument array given as parameter must not include the keyword!
         * Do remove first argument before givinging it to this method as parameters 
         * 
         * @param args      string array with all arguments but the keyword
         * 
         * @return          string list of all arguments which are no options
         * 
         */
        private static List<string> fetch_options(string[] args)
        {
            List<string> extra_parameters; // to store the arguments, which are not options

            any_option_is_set_flag = false;

            // OptionSet() from NDesk.Options
            // all possible options are definde here
            // {"option-switch", "Text for Help(not used here)", what to do when found}
            var p = new OptionSet() {
		   	    { "l=", "Login Name", v => {option_login_name.set(v); any_option_is_set_flag = true;} },
                { "p=", "Port", (int v) => { if(v>0)
                                             {
                                                 option_port.set(v);
                                                 any_option_is_set_flag = true;
                                            }
                                            else
                                                {throw new Exception(v + " is out of range.");}}  },
                { "k=", "Key for Authentification", v => {
                                                bool f = false;
                                                if(v.StartsWith("password=")) 
                                                {
                                                    option_key.type = Sshfs.ConnectionType.Password;
                                                    f = true;
                                                }
                                                if(v.StartsWith("private_key="))
                                                {
                                                    option_key.type = Sshfs.ConnectionType.PrivateKey;
                                                    f = true;
                                                }
                                                
                                                if(f)
                                                {
                                                    option_key.set(v.Substring(v.IndexOf("=")+1));
                                                    any_option_is_set_flag = true;
                                                } 
                                            } },

                { "s=", "Path on Server", v => {option_path.set(v); any_option_is_set_flag = true;}  },
                { "d=", "Driveletter", v => {
                                                if(v.Length > 2 || (v.Length == 2 && v.ToCharArray()[1] != ':'))
                                                {
                                                    throw new Exception(v + " is no drive letter.");   
                                                }
                                                else
                                                { 
                                                    option_letter.set(v);
                                                    any_option_is_set_flag = true;
                                                }
                                            }  },
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
                        //Console.WriteLine("fissh: unknown option {0}", extra_parameters[i]);
                        //fissh_print.error_message("unknown option " + extra_parameters[i]);
                        //Environment.Exit(-1);
                        throw new Exception("unknown option " + extra_parameters[i]);
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
                //fissh_print.error_message(e.Message);
                //Environment.Exit(-1);

                throw new Exception(e.Message);
                return null; //useless, but VS is complaning whithout return
            }


        }


        /// parse parameters
        /**
         * Arguments which are neither keywords not options 
         * are parameters like name of server or folderlist.
         * This method puts the parameter in option object.
         * In which depens on the given keyword.
         * 
         * 
         * The parameter must be a list of all parmeters
         * like the return value of fetch_options()
         * 
         * @param   string list with all parameter
         *
         */
        private static void fetch_parameters(List<string> parameters)
        {
            switch (keyword)    //uses of arguments depends on the keyword 
            {
                case (byte)fissh_command_keywords.mount:
                    #region mount

                    //User wants to mount a uregistered server
                    if (any_option_is_set_flag)
                    {
                        int i_start, i_end;     //indices to isolate host from username and port (user@host:port)
                        string puffer_login_name, puffer_port;

                        // If there is no parameter
                        if (parameters.Count() == 0)
                        {
                            throw new Exception("missing parameter");
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
                            i_end = parameters[0].Length;   //so hostname ends with last char
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
                        if (!option_port.is_set_flag && puffer_port != null)
                        {
                            option_port.set(Convert.ToInt32(puffer_port));
                        }

                        //isolate hostname
                        parameter_host.set(parameters[0].Substring(i_start, i_end - i_start));
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
                    #endregion

                case (byte) fissh_command_keywords.umount:
                case (byte) fissh_command_keywords.status:
                    #region umount_or_status
                    // If Option -d or -v is set, we allready know what the user wants to unmount
                    if (option_letter.is_set_flag || option_virtual_drive.is_set_flag)
                    {
                        return;
                    }
                    
                    // If not, there must be other arguments
                    else
                    {
                        //If there are no other arguments, print error-Mesage and exit
                        if(parameters.Count() == 0)
                        {
                            throw new Exception("missing parameter");
                        }
                        
                        // If the first Parameter is a driveletter or a Virtualdrive-Path
                        else if (parameters[0].IndexOf(":") == 1)
                        {
                            //if it is a driveletter
                            if (parameters[0].Length <= 3)
                            {
                                option_letter.set(parameters[0]);
                            }
                            //if it is a virtualdrive
                            else
                            {
                                option_virtual_drive.set(parameters[0]);
                            }
                        }

                        // If parameters are referenzes to a registered server
                        else
                        {
                            parameter_servername.set(parameters[0]);

                            if (parameters.Count() > 1)
                            {
                                parameter_folderlist.set(parameters[1]);
                            }
                        }

                    }
                    break;
                    #endregion

               default: return;
            }
        }
        #endregion
    }





}