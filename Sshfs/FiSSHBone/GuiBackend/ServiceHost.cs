using System;
using System.ServiceModel;
using Sshfs;
using Sshfs.GuiBackend;
using Sshfs.GuiBackend.Remoteable;
using Sshfs.GuiBackend.IPCChannelRemoting;

using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;


public class Class1
{
	
        public static void Main()
        {

            {

                ServiceHost bone_server = IPCConnection.ServerConnect();
                ServiceFisshBone.Init();
                
                while (!ServiceFisshBone.ShutMeDown) { }
                bone_server.Close();
            }


        }
}
