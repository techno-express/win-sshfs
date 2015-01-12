using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fissh_command
{
    /// Class for all text ouput methods
    /**
     * 
     */
    public static class fissh_print
    {
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
