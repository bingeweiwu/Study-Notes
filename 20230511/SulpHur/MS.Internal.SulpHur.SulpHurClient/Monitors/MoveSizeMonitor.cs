using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.Utilities;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MS.Internal.SulpHur.SulpHurClient.Monitors
{
    public class MoveSizeMonitor: MonitorBase
    {
        //static property
        private static MoveSizeMonitor instance = null;
        public event EventHandler WindowMoved;

        private MoveSizeMonitor()
            : base(NativeStructs.EventId.EVENT_SYSTEM_MOVESIZESTART, NativeStructs.EventId.EVENT_SYSTEM_MOVESIZEEND)
        {
            Trace.WriteLine("MoveSizeMonitor Started.");
        }
        public static MoveSizeMonitor Instance
        {
            get
            {
                if (instance == null)
                    instance = new MoveSizeMonitor();

                return instance;
            }
        }

        protected override void Handler(object sender, WinEventWaiterArgs e)
        {
            //Trace.WriteLine(string.Format("MoveSizeMonitor.ThreadId: {0}", NativeMethods.GetCurrentThreadId()));

            NativeStructs.EventId eventId = (NativeStructs.EventId)e.iEvent;
            if (eventId.Equals(NativeStructs.EventId.EVENT_SYSTEM_MOVESIZEEND))
            {
                this.IsChanged = true;
                if (this.WindowMoved != null)
                {
                    this.WindowMoved(this, null);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            instance = null;
        }
    }
}
