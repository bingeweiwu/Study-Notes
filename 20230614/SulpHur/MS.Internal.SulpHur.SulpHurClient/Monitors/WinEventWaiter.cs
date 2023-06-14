using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.Utilities;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MS.Internal.SulpHur.SulpHurClient.Monitors
{
    public class WinEventWaiterArgs: EventArgs
    {
        public WinEventWaiterArgs(int iEvent, 
                                  IntPtr hWnd, 
                                  long idObject,
                                  long idChild, 
                                  int dwEventThread, 
                                  int dwmsEventTime)
        {
            this.iEvent = iEvent;
            this.hWnd = hWnd;
            this.idObject = idObject;
            this.idChild = idChild;
            this.dwEventThread = dwEventThread;
            this.dwmsEventTime = dwmsEventTime;
        }

        public int iEvent { get; private set; }
        public IntPtr hWnd { get; private set; }
        public long idObject { get; private set; }
        public long idChild { get; private set; }
        public int dwEventThread { get; private set; }
        public int dwmsEventTime { get; private set; }
    }

    public class WinEventWaiter : IDisposable
    {
        //property
        private bool isDisposed = false;
        private WinEventProc eventProc = null;
        private HookSafeHandle pHook;
        private GCHandle gcHandle;
        private NativeStructs.EventId minEventId;
        private NativeStructs.EventId maxEventId;
        public event EventHandler<WinEventWaiterArgs> Notify;

        public WinEventWaiter(NativeStructs.EventId minEventId, NativeStructs.EventId maxEventId)
        {
            this.minEventId = minEventId;
            this.maxEventId = maxEventId;
            this.eventProc = new WinEventProc(this.EventCallback);
            this.Start();
        }

        /// <summary>
        /// Start the event pump
        /// </summary>
        /// <param name="sink"></param>
        private void Start()
        {
            //event hook
            if (pHook == null || pHook.IsInvalid)
            {
                MessageLooper.Instance.Invoke(delegate
                {
                    // pin handle to eventProc
                    gcHandle = GCHandle.Alloc(eventProc);
                    // set the event hook
                    pHook = NativeMethods.SetWinEventHook(this.minEventId, this.maxEventId, IntPtr.Zero, eventProc, 0, 0, NativeStructs.SetWinEventHookFlags.WINEVENT_OUTOFCONTEXT | NativeStructs.SetWinEventHookFlags.WINEVENT_SKIPOWNPROCESS);
                });
            }
        }

        /// <summary>
        /// Stop the event pump
        /// </summary>
        private void Stop()
        {
            // unhook win events
            if (pHook != null && pHook.IsInvalid == false)
            {
                this.pHook.Close();
                pHook = null;
            }

            // unpin eventProc
            if (gcHandle != null && gcHandle.IsAllocated)
            {
                gcHandle.Free();
            }
        }
        private void EventCallback(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, long idObject, long idChild, int dwEventThread, int dwmsEventTime)
        {
            if (hWnd.Equals(IntPtr.Zero))
                return;

            NativeStructs.EventId eventId = (NativeStructs.EventId)iEvent;
            if (!eventId.Equals(NativeStructs.EventId.EVENT_SYSTEM_FOREGROUND) && !Utility.IsProductProcess(hWnd))
            {
                return;
            }
            
            WinEventWaiterArgs e = new WinEventWaiterArgs(iEvent, hWnd, idObject, idChild, dwEventThread, dwmsEventTime);
            Notify(this, e);
        }

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
        private void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                this.Stop();
                this.isDisposed = true;
            }
        }

        /// <summary>
        /// destructor for GC finalization
        /// </summary>
        ~WinEventWaiter()
        {
            this.Dispose(false);
        }
    }
}
