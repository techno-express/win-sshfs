using System;
using System.ServiceModel;
using Sshfs;
using Sshfs.GuiBackend;
using Sshfs.GuiBackend.Remoteable;
using Sshfs.GuiBackend.IPCChannelRemoting;

//Added like in Codeguru
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;


public class Class1
{
	
        public static void Main()
        {

            IpcChannel ipcCh = new IpcChannel("FiSSH");

            ChannelServices.RegisterChannel(ipcCh, true);
            RemotingConfiguration.RegisterWellKnownServiceType
               (typeof(Sshfs.GuiBackend.IPCChannelRemoting.ServiceFisshBone),
                       "fisshy",
                       WellKnownObjectMode.SingleCall);
            Console.WriteLine("Please enter to stop the server");
            Console.ReadLine();


        }
}
