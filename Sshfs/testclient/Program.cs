using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Sshfs.GuiBackend;
using Sshfs.GuiBackend.Remoteable;
using Sshfs.GuiBackend.IPCChannelRemoting;

using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;

namespace testclient
{
    class Program
    {
        static void Main(string[] args)
        {



            IpcChannel ipcCh = new IpcChannel("myClient");
            ChannelServices.RegisterChannel(ipcCh, true);

            IServiceFisshBone obj =
               (Sshfs.GuiBackend.Remoteable.IServiceFisshBone)Activator.GetObject
               (typeof(Sshfs.GuiBackend.Remoteable.IServiceFisshBone),
                "ipc://FiSSH/fisshy");

            obj.Mount(Guid.Empty, Guid.Empty);
            
            Console.ReadLine();


        }
    }
}
