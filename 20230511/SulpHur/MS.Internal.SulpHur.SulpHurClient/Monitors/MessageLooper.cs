using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using MS.Internal.SulpHur.Utilities;

namespace MS.Internal.SulpHur.SulpHurClient.Monitors
{
    public class MessageLooper: IDisposable
    {
        private static MessageLooper instance = null;
        private Form form = null;
        private bool isDisposed = false;
        private MessageLooper()
        {
            // event that signals that form has loaded
            ManualResetEvent resetEvent = new ManualResetEvent(false);

            // create thread hosting the winform
            Thread messageLoopThread = new Thread(delegate()
            {
                // create an instance of the form
                form = new Form() { 
                    Width = 1,
                    Height = 1,
                    ShowInTaskbar = false,
                    WindowState = FormWindowState.Minimized
                };

                //form loaded event
                form.Load += delegate
                {
                    // signal the form has loaded
                    resetEvent.Set();
                };

                Trace.WriteLine(string.Format("MessageLooper.ThreadId: {0}", NativeMethods.GetCurrentThreadId()));
                // start the message pump
                Application.Run(form);
            });

            // start the thread
            messageLoopThread.Name = "MessageLooper";
            messageLoopThread.SetApartmentState(ApartmentState.STA);
            messageLoopThread.Start();

            // wait until thread has loaded
            resetEvent.WaitOne();
            resetEvent.Close();
        }

        /// <summary>
        /// singleton instance
        /// </summary>
        public static MessageLooper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MessageLooper();
                }
                return instance;
            }
        }

        public void Invoke(Action action)
        {
            form.Invoke(action);
        }

        public IAsyncResult BeginInvoke(Action action)
        {
            return form.BeginInvoke(action);
        }

        public void EndInvoke(IAsyncResult result)
        {
            form.EndInvoke(result);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                this.BeginInvoke(delegate
                {
                    Application.ExitThread();
                });
                instance = null;
            }
        }

        ~MessageLooper()
        {
            this.Dispose(false);
        }
    }
}
