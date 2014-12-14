/*
Copyright (c) 2014 bjoe-phi

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/


// This Software is a interface to controll FiSSH over Command-Line


using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;



namespace fissh_cmdline_interface
{
    class Program
    {
        static void Main(string[] args)
        {

            // arguments get parsed by contructor; all parameters will be available in the new object
            var cmdline_parameters = new fissh_command.fissh_command_expression(args);


            #region Testing
            Console.WriteLine("");
            Console.WriteLine("=== Testbench ===");


            try
            {
                switch (cmdline_parameters.keyword)
                {
                    case (byte)fissh_command.fissh_command_keywords.mount:

                        // If user want to mount a registered server
                        if (!cmdline_parameters.parameter_host.is_set_flag)
                        {

                            // If user added a folderlist in parameter
                            if (cmdline_parameters.parameter_folderlist.is_set_flag)
                            {
                                fissh_command.actions.mount_registered_folders(cmdline_parameters);
                            }
                            // If no folderlist is used
                            else
                            {
                                fissh_command.actions.mount_complet_server(cmdline_parameters);
                            }
                        }
                        // If user want to mount a unregisteres Server
                        else
                        {
                            //::FIXME::
                            Console.WriteLine("You want me to mount a server with the URL {0} on Port {1}.", cmdline_parameters.parameter_host.get(), cmdline_parameters.option_port.get());
                            Console.WriteLine("Path on Server: {0}", cmdline_parameters.option_path.get());
                            Console.WriteLine("Loginname: {0}", cmdline_parameters.option_login_name.get());
                            Console.WriteLine("Authentifikation: {0}", cmdline_parameters.option_key.get());
                        }
                        break;


                    case (byte)fissh_command.fissh_command_keywords.umount:
                        // If user wants to umount a simple drive
                        if (cmdline_parameters.option_drive.is_set_flag)
                        {
                            fissh_command.actions.umount_driveletter(cmdline_parameters);
                        }

                        // If user wants to umount a virtual drive
                        else if (cmdline_parameters.option_virtual_drive.is_set_flag)
                        {
                            fissh_command.actions.umount_virtualdrive(cmdline_parameters);
                        }

                        // If user wants to umount folders on a registered server
                        else if (cmdline_parameters.parameter_folderlist.is_set_flag)
                        {
                            fissh_command.actions.umount_registered_folders(cmdline_parameters);
                        }

                        // If user wants to umount a complet registered server
                        else
                        {
                            fissh_command.actions.umount_complet_server(cmdline_parameters);
                        }

                        break;

                    case (byte)fissh_command.fissh_command_keywords.status:
                        Console.WriteLine("You ask for a status.");
                        break;

                    case (byte)fissh_command.fissh_command_keywords.help:
                        Console.WriteLine("HELP!!!");
                        break;


                    default:
                        Console.WriteLine("Unknown keyword");
                        break;
                }
                Console.WriteLine(" ");
            #endregion

                Console.ReadLine();
            }
            catch (Exception e) 
            {
                fissh_command.fissh_print.error_message(e.Message);
                Console.ReadLine();
            }
        }



    }
}

