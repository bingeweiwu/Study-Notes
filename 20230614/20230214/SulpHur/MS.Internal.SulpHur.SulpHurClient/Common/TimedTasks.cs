using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MS.Internal.SulpHur.SulpHurClient.Common
{
    /// <summary>
    /// Timed tasks
    /// </summary>
    public class TimedTasks
    {
        /// <summary>
        /// Method entry
        /// </summary>
        public static void RunTasks()
        {
            UpgradeTask();
        }

        /// <summary>
        /// The client will auto upgrade per 24h. 
        /// </summary>
        public static void UpgradeTask()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    if (Tool.UpdateState())
                    {
                        Trace.WriteLine("Upgrade!!!");
                        Tool.RunBat(@"C:\SulpHurClient\bin\ForceUpgrade.bat");
                    }
                    Thread.Sleep(TimeSpan.FromMinutes(5));
                }
            });
        }
    }
}
