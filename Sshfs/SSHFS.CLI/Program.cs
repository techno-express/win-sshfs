using CommandLine;
using DokanNet;
using Renci.SshNet;
using Sshfs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSHFS.CLI
{
    class Options
    {
        // Paths
        [Option('d', "drive-letter", Required = true, HelpText = "Drive letter to mount the remote SFTP path under")]
        public char DriveLetter { get; set; }
        [Option('r', "path", Required = true, HelpText = "Absolute path of directory to be mounted from remote system")]
        public string Root { get; set; }

        // Remote host
        [Option('h', "host", Required = true, HelpText = "IP or hostname of remote host")]
        public string Host { get; set; }
        [Option('p', "port", Required = false, HelpText = "SSH service port on remote server")]
        public int? Port { get; set; }

        // Auth
        [Option('u', "username", Required = true, HelpText = "Username")]
        public string Username { get; set; }
        [Option('x', "password", Required = false, HelpText = "Password")]
        public string Password { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            Parser.Default.ParseArgumentsStrict(args, options);

            var auth = options.Port.HasValue
                ? new KeyboardInteractiveConnectionInfo(options.Host, options.Port.Value, options.Username)
                : new KeyboardInteractiveConnectionInfo(options.Host, options.Username);

            auth.AuthenticationPrompt += (sender, e) =>
            {
                foreach (var p in e.Prompts.Where(p => p.Request.StartsWith("Password:")))
                    p.Response = options.Password;
            };
            var sftpFS = new SftpFilesystem(auth, options.Root);
            sftpFS.Connect();
            sftpFS.Mount($"{options.DriveLetter}");
            Console.ReadLine();
        }
    }
}
