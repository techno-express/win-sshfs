using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fissh_cmdline_interface
{
    /// Class for all text ouput methods
    /**
     * 
     */
    public static class fissh_print
    {
        private const string cmpnt = "fissh";

        /// print error which is caused by wrong syntax
        /**
         * This mehtod prints an error message and shows
         * how the programm should be used.
         * 
         * The given error message can be an thrown exception message for example.
         * 
         */
        public static void wrong_use_error_message(string error_message)
        {
            simple_error_message(error_message);
            Console.WriteLine("Usage:");
            //Console.WriteLine("");
            Console.WriteLine("To mount a drive");
            Console.WriteLine("     fissh mount SERVERNAME [FOLDERLIST]");
            Console.WriteLine("     fissh mount [-l user] [-p port] [-k password=...| privat_key=...]");
            Console.WriteLine("                 -s PATH [-d DRIVE-LETTER | -v VIRTUAL_DRIVE_FOLDER] [user@]host[:port]");
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

        public static void simple_error_message(string error_message)
        {
            Console.WriteLine("");
            Console.WriteLine("fissh: {0}", error_message);
            Console.WriteLine();

            logger.log.writeLog(SimpleMind.Loglevel.Debug, cmpnt, error_message);
        }



        public static void simple_output_message(string error_message)
        {
            Console.WriteLine("");
            Console.WriteLine("{0}", error_message);
            Console.WriteLine();

            logger.log.writeLog(SimpleMind.Loglevel.Debug, cmpnt, error_message);
        }

        public static void help_message()
        {
            Console.WriteLine("This is a command line interface for the SSHfs client FiSSH.");
            Console.WriteLine("You should start the FiSSH software before using this interface.");
            Console.WriteLine();
            Console.WriteLine("To mount a drive");
            Console.WriteLine("     fissh mount SERVERNAME [FOLDERLIST]");
            Console.WriteLine("ex.: this will mount all folder entries of server entry \"DB-Server\"");
            Console.WriteLine("     > fissh mount DB-Server");
            Console.WriteLine("ex.: this will mount folder \"One\", \"Two\" and \"Home\" of \"DB-Server\"");
            Console.WriteLine("     > fissh mount DB-Server One,Two,Home");
            Console.WriteLine("");
            Console.WriteLine("To mount a drive which is not registered in backend");
            Console.WriteLine("     fissh mount [-l user] [-p port] [-k password=...| privat_key=...]");
            Console.WriteLine("                 -s PATH [-d DRIVE-LETTER | -v VIRTUAL_DRIVE_FOLDER] [user@]host[:port]");
            Console.WriteLine("ex.: > fissh mount -k password=s3cr3t -s /home/user -d Z: user@93.216.119:22");
            Console.WriteLine("");
            Console.WriteLine("To unmount a drive");
            Console.WriteLine("     fissh umount DRIVE-LETTER | -v VIRTUAL_DRIVE_FOLDER");
            Console.WriteLine("ex.: > fissh umount Z:");
            Console.WriteLine("     fissh umount SERVERNAME [FOLDERLIST]");
            Console.WriteLine("ex.: > fissh umount DB-Server One,Two,Home");
            Console.WriteLine("");
            Console.WriteLine("Ask if a drive is mounted");
            Console.WriteLine("     fissh status DRIVE-LETTER | -v VIRTUAL_DRIVE_FOLDER");
            Console.WriteLine("ex.: > fissh status Z:");
            Console.WriteLine("     fissh status SERVERNAME FOLDER");
            Console.WriteLine("ex.: > fissh status DB-Server One,Two,Home");
            Console.WriteLine("");
            Console.WriteLine("To print this info page");
            Console.WriteLine("     fissh help");
            Console.WriteLine("");

        }
    }
}
