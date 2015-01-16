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


#define DEBUG



using System;
using System.Collections.Generic;
using fissh_command;
//using System.Linq;
//using System.Text;



namespace fissh_cmdline_interface
{
    class Program
    {

        static void Main(string[] args)
        {
            SimpleMind.SimpleMind logger = new SimpleMind.SimpleMind();


            //var cmdline_parameters = new fissh_command.fissh_command_expression();
            //fissh_command.fissh_command_expression cmdline_parameters = null;

            try
            {
                // arguments get parsed by contructor; all parameters will be available in the new object
                fissh_command_expression.parse(args);
            }
            catch (Exception e)
            {
                fissh_command.fissh_print.wrong_use_error_message(e.Message);
                logger.writeLog(SimpleMind.Loglevel.Debug, "cmdline", e.Message );
                
#if DEBUG
                Console.ReadLine();
#endif

                Environment.Exit(-1);
            }

            


            try
            {
                #region Region
                switch (fissh_command_expression.keyword)
                {
                    case (byte)fissh_command.fissh_command_keywords.mount:

                        // If user want to mount a registered server
                        if (!fissh_command_expression.parameter_host.is_set_flag)
                        {

                            // If user added a folderlist in parameter
                            if (fissh_command_expression.parameter_folderlist.is_set_flag)
                            {
                                fissh_command.actions.mount_registered_folders();
                            }
                            // If no folderlist is used
                            else
                            {
                                fissh_command.actions.mount_complet_server();
                            }
                        }
                        // If user want to mount a unregisteres Server
                        else
                        {
                            // If Port is not set, use Port 22
                            if (!fissh_command_expression.option_port.is_set_flag)
                            {
                                fissh_command_expression.option_port.set("22");
                            }

                            // If source path is not set, use /
                            if (!fissh_command_expression.option_path.is_set_flag)
                            {
                                fissh_command_expression.option_path.set("/");
                            }

                            //IF user is not set, use root
                            if (!fissh_command_expression.option_login_name.is_set_flag)
                            {
                                fissh_command_expression.option_login_name.set("root");
                            }

                            // If driveletter is not set, use Z:\
                            if(!fissh_command_expression.option_drive.is_set_flag
                                && !fissh_command_expression.option_virtual_drive.is_set_flag)
                            {
                                fissh_command_expression.option_drive.set("Z:");
                            }

                            // If no Authentification-Key is set
                            if (!fissh_command_expression.option_key.is_set_flag)
                            {
                                Console.Write("Enter Password >");
                                fissh_command_expression.option_key.set(
                                    "password=" + Console.ReadLine());
                            }

                            //fissh_command.actions.mount_unregistered_folder();

                            //::FIXME::
                            Console.WriteLine("You want me to mount a server with the URL {0} on Port {1}.", fissh_command_expression.parameter_host.get(), fissh_command_expression.option_port.get());
                            Console.WriteLine("Path on Server: {0}", fissh_command_expression.option_path.get());
                            Console.WriteLine("Loginname: {0}", fissh_command_expression.option_login_name.get());
                            Console.WriteLine("Authentifikation: {0}", fissh_command_expression.option_key.get());
                        }
                        break;


                    case (byte)fissh_command.fissh_command_keywords.umount:
                        // If user wants to umount a simple drive
                        if (fissh_command_expression.option_drive.is_set_flag)
                        {
                            fissh_command.actions.umount_driveletter();
                        }

                        // If user wants to umount a virtual drive
                        else if (fissh_command_expression.option_virtual_drive.is_set_flag)
                        {
                            fissh_command.actions.umount_virtualdrive();
                        }

                        // If user wants to umount folders on a registered server
                        else if (fissh_command_expression.parameter_folderlist.is_set_flag)
                        {
                            fissh_command.actions.umount_registered_folders();
                        }

                        // If user wants to umount a complet registered server
                        else
                        {
                            fissh_command.actions.umount_complet_server();
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

            }
            catch (Exception e) 
            {
                Console.WriteLine(e.Message);
            }
 
#if DEBUG
            Console.ReadLine();
#endif


        }



    }
}

