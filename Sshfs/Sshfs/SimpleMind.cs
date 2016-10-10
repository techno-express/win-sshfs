// Copyright (c) 2014 Thomas Bauer
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.Threading;


namespace SimpleMind
{
    public enum Loglevel:int { 
        None = 0,
        Error = 1, 
        Warning = 2, 
        Debug = 3, 
    }
    

    /// writes a logfile with 4 loglevels
    /**
     * the 4 loglevels are: no logging, only errors, warnings+errors and log all
     */
    public class SimpleMind
    {
        const int _DefaultLogMax = 3; // default highest LogLevel
        const string _DefaultFileName = "log.txt";

        private string _LogFile; // name of Logfile
        private string _Folder; // folder to Logfile
        private string _PathToLogFile;
        private static int _LogLevel;
        private byte attempts; //to prevent deathlocks      

        public SimpleMind(int iLogLevel, string sFile, string sPath)
        {

            if (!sPath.EndsWith(@"\"))
            {
                sPath = sPath + @"\";
            }

            // init
            _Folder = sPath;
            _LogFile = sFile;
            _PathToLogFile = _Folder + _LogFile;

            switch (iLogLevel) {
                case (int) Loglevel.None:
                                    _LogLevel = (int) Loglevel.None;
                                    break;
                case (int) Loglevel.Error: 
                                    _LogLevel = (int) Loglevel.Error;    
                                    break;
                case (int) Loglevel.Warning:
                                    _LogLevel = (int) Loglevel.Warning;
                                    break;
                case (int) Loglevel.Debug:
                                    _LogLevel = (int) Loglevel.Debug;
                                    break;
            }
                                    
            
            try
            {
                //check for target directory and create if not found
                if (!System.IO.Directory.Exists(_Folder))
                {
                    //System.IO.Directory.CreateDirectory(_Folder);
                    Directory.CreateDirectory(_Folder);
                }

                //create file or open
                if (!File.Exists(_PathToLogFile))
                {
                    using (System.IO.FileStream fs = System.IO.File.Create(_PathToLogFile))
                    {
                        //just create file
                        fs.Close();
                    }
                }
            }

            catch (Exception e)
            {
                // FIXME: define behavior in case of excepetion
                Console.WriteLine("Error: Could not create File or Directory: \" " + _Folder + _LogFile + "\"");
            }

        }

        public SimpleMind(int iLogLevel, string sFile)
        {
            string MyDocDir = @"C:\"; //Default

            //********************************************************
            //finding MyDocuments directory if no Path is given
            try
            {
                MyDocDir = Sshfs.FiSSHGlobal.HomeDirectory; //Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                //throws: ArgumentException if Folder does not exist
            }
            catch (Exception e)
            {
                //FIXME MyDocuments does not exist
                MyDocDir = @"C:\";
                Console.WriteLine(@"Warning: Could not find MyDocuments. Saving at C:\");
            }

            //init
            _Folder = MyDocDir;
            _LogFile = sFile;
            _PathToLogFile = _Folder + _LogFile;

            switch (iLogLevel)
            {
                case (int)Loglevel.None:
                    _LogLevel = (int)Loglevel.None;
                    break;
                case (int)Loglevel.Error:
                    _LogLevel = (int)Loglevel.Error;
                    break;
                case (int)Loglevel.Warning:
                    _LogLevel = (int)Loglevel.Warning;
                    break;
                case (int)Loglevel.Debug:
                    _LogLevel = (int)Loglevel.Debug;
                    break;
            }


            // Check if file exists
            if (!File.Exists(_PathToLogFile))
            {
                using (System.IO.FileStream fs = System.IO.File.Create(_PathToLogFile))
                {
                    //just create file
                    fs.Close();
                }
            }

        }

        public SimpleMind(int iLogLevel)
            : this(iLogLevel, _DefaultFileName)
        {
        }

        public SimpleMind()
            : this(_DefaultLogMax, _DefaultFileName)
        {
        }


        //*** Methods ***
        #region
        //*** private ***
        // writing LogMsg into Logfile with current timestamp no LogLevel is checked
        private void write(Loglevel iLogType, string cmpnt, string LogMsg, byte attempts)
        {
            DateTime Now = DateTime.Now;

            if (attempts < 100)   
            {
                try
                {
                    using (StreamWriter fs = new StreamWriter(_PathToLogFile, true))
                    {
                        switch (iLogType)
                        {
                            case Loglevel.Error:
                                fs.WriteLine("[" + Now.ToString("yyyy/MM/dd HH:mm:ss") + "]" + "[Error]" + "[" + cmpnt + "] " + LogMsg + " ");
                                break;
                            case Loglevel.Warning:
                                fs.WriteLine("[" + Now.ToString("yyyy/MM/dd HH:mm:ss") + "]" + "[Warning]" + "[" + cmpnt + "] " + LogMsg + " ");
                                break;
                            case Loglevel.Debug:
                                fs.WriteLine("[" + Now.ToString("yyyy/MM/dd HH:mm:ss") + "]" + "[Debug]" + "[" + cmpnt + "] " + LogMsg + " ");
                                break;
                        }

                        /*
                         //no zero when time got one digit elements
                       fs.WriteLine("[" + Now.Year.ToString() + @"/"
                          + Now.Month.ToString() + @"/"
                          + Now.Day.ToString() + " "
                          + Now.Hour.ToString() + ":"
                          + Now.Minute.ToString() + ":"
                          + Now.Second.ToString() + "] " + LogMsg);*/
                    }
                }
                catch (IOException e)                       //put current thread on hold if another one is already writing in the logfile 
                {
                    attempts++;
                    Thread.Sleep(attempts);
                    write(iLogType, cmpnt, LogMsg, attempts);
                }
                catch (UnauthorizedAccessException e)
                {

                    System.Windows.Forms.MessageBox.Show("An error while logging has occurred. Check if file is write protected. Loglevel has been set to \"None\"");
                    SimpleMind._LogLevel = 0;
                   
                } 
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show("An error while logging has occurred. Check if path exists. Loglevel has been set to \"None\"");
                    SimpleMind._LogLevel = 0;
                }
            }
            
            
           
        }

        //*** public ***
        public void writeLog(Loglevel iLogType, string cmpnt, string Msg)
        {
            if ((int)iLogType >= 0 && (int)iLogType <= _LogLevel)
            {
                attempts = 0;
                write(iLogType,cmpnt, Msg, attempts);
            }

        }

        public void setLogLevel(int i)
        {
            if (i >= 0 && i <= _DefaultLogMax)
            {
                _LogLevel = i;
            }
        }

        public int getLogLevel()
        {
            return _LogLevel;
        }

        public string getPathToLogFile()
        {
            return _PathToLogFile;
        }
        #endregion
    }
}

