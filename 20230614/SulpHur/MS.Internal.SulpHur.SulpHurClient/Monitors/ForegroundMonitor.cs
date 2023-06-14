using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.Utilities;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MS.Internal.SulpHur.SulpHurClient.Monitors
{
    public class ForegroundMonitor: MonitorBase
    {
        //static property
        private static ForegroundMonitor instance = null;
        public event EventHandler ForegroundChanged;

        private ForegroundMonitor(): base(NativeStructs.EventId.EVENT_SYSTEM_FOREGROUND)
        {
            Trace.WriteLine("ForegroundMonitor Started.");
        }
        public static ForegroundMonitor Instance
        {
            get
            {
                if (instance == null)
                    instance = new ForegroundMonitor();

                return instance;
            }
        }

        protected override void Handler(object sender, WinEventWaiterArgs e)
        {
            //Trace.WriteLine(string.Format("ForegroundMonitor.ThreadId: {0}", NativeMethods.GetCurrentThreadId()));
            this.IsChanged = true;
            if (this.ForegroundChanged != null)
            {
                this.ForegroundChanged(this, null);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            instance = null;
        }
    }
}
