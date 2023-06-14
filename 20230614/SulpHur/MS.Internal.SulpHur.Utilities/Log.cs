using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MS.Internal.SulpHur.Utilities
{
    public class Log
    {
        private static string logFile = @"C:\Telescop_debug.log";

        public static void WriteLog(string logMessage)
        {
            TextWriter tw = null;
            try
            {
                if (!File.Exists(logFile))
                    File.Create(logFile);

                //File.AppendAllText(logFile, logMessage);

                tw = File.AppendText(logFile);
                tw.WriteLine(logMessage);
                tw.Flush();
            }
            catch
            { 
            }
            finally
            {
                if (tw != null)
                {
                    tw.Close();
                }
            }
        }
    }
}
