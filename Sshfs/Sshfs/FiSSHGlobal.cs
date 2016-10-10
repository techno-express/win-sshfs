using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sshfs
{
    public static class FiSSHGlobal
    {
         public static string HomeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\FiSSH\";
    }
}
