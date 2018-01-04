using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Sshfs.GuiBackend
{
    public static class WaitHandler
    {
        private static EventWaitHandle sleepTillExit = new AutoResetEvent(false);

        public static void Wait()
        {
            sleepTillExit.WaitOne();
            return;
        }
	    
        public static void Exit()
        {
            sleepTillExit.Set();
        }
    
    }
}
