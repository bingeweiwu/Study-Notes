using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace MS.Internal.SulpHur.SulpHurService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            //BugSync bugSync = new SulpHurService.BugSync();
            //bugSync.Start();
            BugSync.BugSyncTask();

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new SulpHurWindowsService() 
			};
            ServiceBase.Run(ServicesToRun);
        }
    }
}
