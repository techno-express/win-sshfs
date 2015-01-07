using System;
using System.ServiceModel;
using Sshfs.GuiBackend;


public class Class1
{
	public Class1()
	{}

        public static void Main()
        {

            using(ServiceHost bone = new ServiceHost(typeof(ServiceFisshBone),
																new Uri[]{new Uri("net.pipe://localhost")}))
            {
				bone.AddServiceEndpoint(typeof(IServiceFisshBone), new NetNamedPipeBinding(), "Fisshbone");
                bone.Open();
				
                Console.WriteLine("Service started.");
                Console.WriteLine("Press <return> to terminate.");
                Console.ReadLine();
				
				bone.Close();

            }


        }
}
