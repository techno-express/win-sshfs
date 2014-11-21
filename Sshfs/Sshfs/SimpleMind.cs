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



namespace SimpleMind
{
    public class SimpleMind
    {
        private string _LogFile; // name of Logfile
        private string _Folder; // folder to Logfile
        private string _PathToLogFile;
        private static int _LogLevel;
        // 0 > == None
        // 0 == Warning
        // 1 == Error
        // 3 == Event such as Clicks
        // 4 == Debug/All

        public SimpleMind(int iLogLevel, string sFile, string sPath)
        {

            if (!sPath.EndsWith(@"\"))
            {
                //*** This comment makes no sens. *** Line 42 is the truth
                sPath = sPath + @"\";
            }

            // init
            _Folder = sPath;
            _LogFile = sFile;
            _PathToLogFile = _Folder + _LogFile;
            _LogLevel = iLogLevel;

            try
            {
                //ceck for target directory and create if not found
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
            }

        }

        public SimpleMind(int iLogLevel, string sFile)
        {
            string MyDocDir = @"C:\"; //Default

            //********************************************************
            //finding MyDocuments directory if no Path is given
            try
            {
                MyDocDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                //throws: ArgumentException if Folder does not exist
            }
            catch (Exception e)
            {
                //FIXME MyDocuments does not exist
                MyDocDir = @"C:\";
            }
            
            //init
            _Folder = MyDocDir;
            _LogFile = sFile;
            _PathToLogFile = _Folder + _LogFile;
            _LogLevel = iLogLevel;


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
            : this(iLogLevel, @"log.txt")
        {
        }

        public SimpleMind()
            : this(4, @"log.txt")
        {
        }


        //*** Methods ***
        #region
        //*** private ***
        // writing LogMsg into Logfile with current timestamp no LogLevel is checked
        private void writeLog(string LogMsg)
        {
            DateTime Now = DateTime.Now;

            using (StreamWriter fs = new StreamWriter(_PathToLogFile, true))
            {
                fs.WriteLine("[" + Now.Year + @"\"
                   + Now.Month + @"\"
                   + Now.Day + " "
                   + Now.Hour + ":"
                   + Now.Minute + ":"
                   + Now.Second + "] " + LogMsg);
            }
        }


        //*** public ***
        public void writeLog(int iLogType, string Msg)
        {
            if (iLogType >= 0 && iLogType <= _LogLevel)
            {
                writeLog(Msg);
            }

        }

        public void setLogLevel(int i)
        {
            if (i <= 4)
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

