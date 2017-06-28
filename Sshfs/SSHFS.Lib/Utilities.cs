#region

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;
using Renci.SshNet;

#endregion

namespace Sshfs
{
    internal static class Utilities
    {
        private static readonly DirectoryInfo datadir = Directory.CreateDirectory(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\WinSshFS"
            );


        public static string ProtectString(string stringToProtect)
        {
            return stringToProtect != null
                       ? Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(stringToProtect), null,
                                                                      DataProtectionScope.CurrentUser))
                       : null;
        }

        public static string UnprotectString(string stringToUnprotect)
        {
            try
            {
                return stringToUnprotect != null
                           ? Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(stringToUnprotect),
                                                                             null,
                                                                             DataProtectionScope.CurrentUser))
                           : null;
            }
            catch
            {
                //in case of migration of config.xml between hosts - passwords cannot and shoud not work
                return null;
            }
        }

        public static IEnumerable<char> GetAvailableDrives()
        {
            return Enumerable.Range('D', 23).Select(value => (char) value).Except(
                Directory.GetLogicalDrives().Select(drive => drive[0]));
        }

        public static bool IsValidUnixPath(string value)
        {
            return !string.IsNullOrEmpty(value) && (value[0] == '.' || value[0] == '/') && (value.IndexOf('\\') == -1);
        }

       /* public static bool IsValidPrivateKey(string path)
        {
            try
            {
                new PrivateKeyFile(path);
                return true;
            }
            catch
            {
                return false;
            }
        }*/

        public static void SetNetworkDriveName(string connection,string name)
        {
           var drive= Registry.CurrentUser.OpenSubKey
               ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\MountPoints2", false).OpenSubKey(connection.Replace("\\","#"),true);
            if(drive!=null)
            {
                drive.SetValue("_LabelFromReg", name);
            }
        }
    }
}