//using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest
{
    public class MyLog
    {
        public static void Init()
        {
            //remove the log4net. if you want to use log4net,you should use these two lines code to init.The config data is already writed to App.config.
            //string logConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App.config");
            //XmlConfigurator.Configure(new FileStream(logConfigPath, FileMode.Open));

            //you may use log like that:
            //    MyLog.Init();
            //    log4net.ILog log = log4net.LogManager.GetLogger("HelloLog4");
            //    log.Debug("Hello log4");

        }
    }
}
