using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SimpleMind;

namespace fissh_cmdline_interface
{
    public static class logger
    {
        public static SimpleMind.SimpleMind log = Init();

        private static SimpleMind.SimpleMind Init()
        {
            return new SimpleMind.SimpleMind((int)Loglevel.Debug);
        }

    }
}
