using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.Utilities;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MS.Internal.SulpHur.SulpHurClient.Monitors
{
    public abstract class MonitorBase : IDisposable
    {
        //instance property
        private bool isDisposed = false;
        public bool IsChanged { get; protected set; }
        private WinEventWaiter waiter;

        public MonitorBase(NativeStructs.EventId eventId): this(eventId, eventId)
        {
        }

        public MonitorBase(NativeStructs.EventId minEventId, NativeStructs.EventId maxEventId)
        {
            this.waiter = new WinEventWaiter(minEventId, maxEventId);
            this.waiter.Notify += new EventHandler<WinEventWaiterArgs>(Handler);
        }

        public void Reset()
        {
            this.IsChanged = false;
        }

        protected abstract void Handler(object sender, WinEventWaiterArgs e);

        /// <summary>
        /// Dispose implementation
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Stop the event source
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed == false)
            {
                if(this.waiter!=null)
                    this.waiter.Dispose();
                this.isDisposed = true;
            }
        }

        ~MonitorBase()
        {
            this.Dispose(false);
        }
    }
}
