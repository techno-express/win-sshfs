using System;
using System.ServiceModel;
using Sshfs;
using Sshfs.GuiBackend;
using Sshfs.GuiBackend.Remoteable;
using Sshfs.GuiBackend.IPCChannelRemoting;

using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;

namespace FiSSHBone.GuiBackend
{
    public class Class1
    {
        public static void Main()
        {
            {
                SimpleMind.SimpleMind Log = new SimpleMind.SimpleMind();
                ServiceHost bone_server = IPCConnection.ServerConnect();
                ServiceFisshBone.Init();

                WaitHandler.Wait();
                bone_server.Close();
                Log.writeLog(SimpleMind.Loglevel.Debug, "FiSSHBoneMain", "FiSSHBone exit.");
            }


        }
    }
}
