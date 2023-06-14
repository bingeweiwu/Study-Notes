using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Configuration;

namespace MS.Internal.SulpHur.SulpHurService
{
    public class Log
    {
        //private static StreamWriter scanLog = null;
        //private static StreamWriter serverLog = null;

        //public static StreamWriter ServerLog
        //{
        //    get
        //    {
        //        Log.serverLog = File.AppendText(ServiceEnviroment.ServerLogPath);
        //        return serverLog;
        //    }
        //    set
        //    {
        //        serverLog = value;
        //    }
        //}
        //public static StreamWriter ScanLog
        //{
        //    get
        //    {

        //        Log.scanLog = File.AppendText(ServiceEnviroment.ScanLogPath);
        //        return Log.scanLog;
        //    }
        //    set
        //    {
        //        scanLog = value;
        //    }
        //}

        //private static void WriteLog(string s, StreamWriter sw)
        //{
        //    WriteRawLine(string.Format("[{0}] {1}", DateTime.Now, s), sw);
        //}

        //private static void WriteRawLine(string s, StreamWriter sw)
        //{
        //    try
        //    {
        //        sw.WriteLine(s);
        //    }
        //    finally {
        //        sw.Close();
        //    }
        //}

        public static void WriteServerLog(string s)
        {
            Trace.WriteLine(s, DateTime.Now.ToString());
        }

        public static TraceSwitch generalSwitch = new TraceSwitch("LogSwitch", "Switch server log");

        static string ServerPath = @"F:\SulpHur\SulpHurService";

        public static void WriteServerLog(string s, TraceLevel level)
        {
            try
            {
                FileInfo file = new FileInfo(ServerPath + @"\Service.log");
                if (file.Length > 5242880)
                {
                    SaveLogToNewFile();
                }
            }
            catch (Exception)
            {
                // When the file moves, this exception will inevitably appear, does not affect the program, ignore it
            }

            s = string.Format("[{0}][{1}]{2}", level.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss"), s);
            switch (level)
            {
                case TraceLevel.Error:
                    Trace.WriteLineIf(generalSwitch.TraceError, s);
                    break;
                case TraceLevel.Warning:
                    Trace.WriteLineIf(generalSwitch.TraceWarning, s);
                    break;
                case TraceLevel.Info:
                    Trace.WriteLineIf(generalSwitch.TraceInfo, s);
                    break;
                case TraceLevel.Verbose:
                    Trace.WriteLineIf(generalSwitch.TraceVerbose, s);
                    break;
                default:
                    Trace.WriteLineIf(generalSwitch.TraceInfo, s);
                    break;
            }
        }

        public static void TraceException(Exception e)
        {
            Trace.WriteLine(e.ToString(), DateTime.Now.ToString());
        }

        public static void WriteRules(List<MS.Internal.SulpHur.UICompliance.UIComplianceRuleBase> rules)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Rules detail:");
            foreach (MS.Internal.SulpHur.UICompliance.UIComplianceRuleBase rule in rules)
            {
                sb.AppendLine(string.Format("Rule Name:{0}, Description:{1}", rule.Name, rule.Description));
            }
            WriteServerLog(sb.ToString(), TraceLevel.Info);
        }

        /// <summary>
        /// when the log file is too large,the file will be moved to the LogFile to save.
        /// </summary>
        public static void SaveLogToNewFile()
        {
            //close TraceListener
            Trace.Listeners[0].Close();
            string sourceFile = ServerPath + @"\Service.log";
            string destFile = ServerPath + @"\Log\" + DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss") + @".log";
            string LogPath = ServerPath + @"\Log";
            if (!Directory.Exists(LogPath))
            {
                Directory.CreateDirectory(LogPath);
            }
            try
            {
                File.Move(sourceFile, destFile);
            }
            catch (Exception ex)
            {
                WriteServerLog("Log file move fail:" + ex.Message);
            }
        }
    }


    public enum TraceLevel
    {
        Warning,
        Info,
        Verbose,
        Error
    }
}
