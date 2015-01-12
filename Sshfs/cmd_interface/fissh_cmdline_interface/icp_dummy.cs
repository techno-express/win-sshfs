using System;
using System.Collections.Generic;
using System.Linq;



namespace icp_dummy
{
    public static class icp
    {
        public static int search_server (string servernickname)
        {
            if (servernickname == "MyTestServer")
            {
                return 42;
            }
            else
            {
                throw new Exception("no registered Server named " + servernickname);
                return -1;
            }
        }

        
        public static int search_folder(string foldernickname, int server_id)
        {
            if (server_id == 42)
            {
                switch (foldernickname)
                {
                    case "MyFirstFolder": return 1;
                    case "MySecondFolder": return 2;
                    default: return -1;
                }
            }
            else
            {
                return -2;
            }
        }

        public static Tuple<int,int> search_driveletter(string driveletter)
        {
            if (driveletter.IndexOf("Z") == 0)
            {
                return new Tuple<int,int> (42, 1);
            }
            else
            {
                return new Tuple<int,int> (-1, -1);
            }
        }


        public static Tuple<int, int> search_virtualdrive(string virtualdrive)
        {


            if (virtualdrive.IndexOf("FirstVirtualDrive") <= 3 && virtualdrive.IndexOf("FirstVirtualDrive") >= 0)
            {
                return new Tuple<int, int>(42, 1);
            }
            else if (virtualdrive.IndexOf("SecondVirtualDrive") <= 3 && virtualdrive.IndexOf("SecondVirtualDrive") >=0)
            {
                return new Tuple<int, int>(42, 2);
            }
            else
            {
                return new Tuple<int, int>(-1, -1);
            }
        }

        public static List<int> get_folder_ids(int server_id)
        {
            List<int> list = new List<int>();
            if (server_id == 42)
            {
                list.Add(1);
                list.Add(2);

            }
            return list;
        }

        public static int mount(int server_id, int folder_id)
        {
            if (server_id == 42)
            {
                switch (folder_id)
                {
                    case 1: Console.WriteLine("IPC: MyFirstFolder auf MyTestServer gemountet"); break;
                    case 2: Console.WriteLine("IPC: MySecondFolder auf MyTestServer gemountet"); break;
                }
            }
            return 0;
        }

        public static int umount(int server_id, int folder_id)
        {
            if (server_id == 42)
            {
                switch (folder_id)
                {
                    case 1: Console.WriteLine("IPC: MyFirstFolder auf MyTestServer ungemountet"); break;
                    case 2: Console.WriteLine("IPC: MySecondFolder auf MyTestServer ungemountet"); break;
                }
            }
            return 0;
        }
    }
}