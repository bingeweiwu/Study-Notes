using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace MS.Internal.SulpHur.SulpHurClient.Monitors
{
    public class GlobalChangeMonitor
    {
        private static GlobalChangeMonitor instance;

        private GlobalChangeMonitor()
        {
        }
        public static GlobalChangeMonitor Instance
        {
            get
            {
                if (instance == null)
                    instance = new GlobalChangeMonitor();

                return instance;
            }
        }

        public void Reset()
        {
            ForegroundMonitor.Instance.Reset();
            MoveSizeMonitor.Instance.Reset();
        }
        public bool IsChanged
        {
            get 
            {
                //foreground changed or size moved
                if (ForegroundMonitor.Instance.IsChanged)
                {
                    Trace.WriteLine("Foreground changed.");
                    return true;
                }
                if (MoveSizeMonitor.Instance.IsChanged)
                {
                    Trace.WriteLine("Window moved.");
                    return true;
                }

                return false;
            }
        }
    }
}
