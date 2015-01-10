using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sshfs.GuiBackend.Remoteable;
using System.ServiceModel;
using Sshfs.GuiBackend.IPCChannelRemoting;



using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;

namespace Sshfs.GuiBackend.Remoteable
{
    public class IPCConnection
    {
        public static IServiceFisshBone bone_client;
        public static ServiceHost bone_server;

        public IPCConnection() { }

        public static IServiceFisshBone ClientConnect ()
        {
            ChannelFactory<IServiceFisshBone> pipeFactory =
              new ChannelFactory<IServiceFisshBone>(
                new NetNamedPipeBinding(),
                new EndpointAddress(
                  "net.pipe://localhost/FiSSH"));

            bone_client = pipeFactory.CreateChannel();
            return bone_client;
        }

        public static ServiceHost ServerConnect() 
        {

            bone_server = new ServiceHost(
                typeof(ServiceFisshBone),
                new Uri[]{ new Uri("net.pipe://localhost") }
                );
            
            bone_server.AddServiceEndpoint(typeof(IServiceFisshBone),
                new NetNamedPipeBinding(),
                "FiSSH");
            bone_server.Open();
            
            return bone_server;
        }
    }
}
