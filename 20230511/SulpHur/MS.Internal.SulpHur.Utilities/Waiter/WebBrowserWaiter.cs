using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SHDocVw;
using System.Windows.Forms;

namespace MS.Internal.SulpHur.Utilities.Waiter
{
    public class WebBrowserWaiter
    {
        //IeWindow
        private InternetExplorer ieWindow;
        public InternetExplorer IeWindow
        {
            get { return ieWindow; }
            set { ieWindow = value; }
        }

        public WebBrowserWaiter(InternetExplorer ieWindow)
        {
            this.ieWindow = ieWindow;
        }

        public void WaitForCompleted(int timeout = 10000)
        {
            int interval = 100;
            int tries = 0;
            while (interval * tries < timeout)
            {
                //check condition
                if (this.IeWindow.ReadyState.Equals(tagREADYSTATE.READYSTATE_COMPLETE))
                    return;

                //loop message
                Application.DoEvents();

                //sleep
                System.Threading.Thread.Sleep(100);
                tries++;
            }
        }
        public void WaitForCompleted(TimeSpan timeout)
        {
            WaitForCompleted(timeout.Milliseconds);
        }
    }
}
