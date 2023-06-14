using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace MS.Internal.SulpHur.Utilities
{
    
    public class SulpHurTextWriterTraceListener : TextWriterTraceListener
    {
        //FileName
        private string fileName = string.Empty;
        public string FileName
        {
            get
            {
                return fileName;
            }
        }

        public SulpHurTextWriterTraceListener()
            : base()
        { }
        public SulpHurTextWriterTraceListener(string fileName)
            : base(fileName)
        {
            this.fileName = fileName;

            try
            {
                //create log folder if not exist
                string fileDir = Path.GetDirectoryName(fileName);
                if (!Directory.Exists(fileDir))
                {
                    Directory.CreateDirectory(fileDir);
                }
                //backup old log
                if (File.Exists(fileName))
                { 
                    string backupFileName = string.Format("{0}_{1}", fileName, DateTime.Now.ToString("yyyyMMddHHmmss"));
                    File.Move(fileName, backupFileName);
                }
            }
            catch
            { }
        }


        public override void WriteLine(object o)
        {
            base.WriteLine(string.Format("[{0}] [{1}]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), o));
        }
        public override void Write(object o)
        {
            base.Write(string.Format("[{0}] [{1}]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), o));
        }
        public override void WriteLine(string message)
        {
            //In order to thread safe, add a temp string
            string tempmessage = message;
            base.WriteLine(string.Format("[{0}] [{1}]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), tempmessage));
            
        }
        public override void Write(string message)
        {
            base.Write(string.Format("[{0}] [{1}]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), message));
        }
    }
}
