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


            // Everything after this line ist just for testing
            #region Testing
            Console.WriteLine("");
            Console.WriteLine("=== Testbench ===");

            switch(cmdline_parameters.keyword) 
            {
                case (byte)fissh_command.fissh_command_keywords.mount:

                    // If user want to mount a registered server
                    if (!cmdline_parameters.parameter_host.is_set_flag) 
                    {
                        
                        // If user added a folderlist in parameter
                        if (cmdline_parameters.parameter_folderlist.is_set_flag)
                        {
                            Console.WriteLine("You want me to mount {0} on server {1}.", cmdline_parameters.parameter_folderlist.get(), cmdline_parameters.parameter_servername.get());
                        }
                        else {
                            Console.WriteLine("You want me to mount the complet server {0}.", cmdline_parameters.parameter_servername.get());
                        }
                    }
                    else
                    {
                        Console.WriteLine("You want me to mount a server with the URL {0} on Port {1}.", cmdline_parameters.parameter_host.get(), cmdline_parameters.option_port.get());
                        Console.WriteLine("Path on Server: {0}", cmdline_parameters.option_path.get());
                        Console.WriteLine("Loginname: {0}", cmdline_parameters.option_login_name.get());
                        Console.WriteLine("Authentifikation: {0}", cmdline_parameters.option_key.get());
                    }
                    break;

                case (byte)fissh_command.fissh_command_keywords.umount:
                    Console.WriteLine("You want to unmount.");
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



    }
}

