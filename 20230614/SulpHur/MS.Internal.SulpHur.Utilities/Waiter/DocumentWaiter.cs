using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mshtml;
using System.Windows.Forms;

namespace MS.Internal.SulpHur.Utilities.Waiter
{
    public class DocumentWaiter
    {
        //Document
        private IHTMLDocument2 document;
        public IHTMLDocument2 Document
        {
            get { return document; }
            set { document = value; }
        }

        public DocumentWaiter(IHTMLDocument2 document)
        {
            this.document = document;
        }

        public void WaitForCompleted(int timeout = 10000)
        {
            int interval = 100;
            int tries = 0;
            while (interval * tries < timeout)
            {
                //check condition
                if (this.document.readyState.Equals("complete"))
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
