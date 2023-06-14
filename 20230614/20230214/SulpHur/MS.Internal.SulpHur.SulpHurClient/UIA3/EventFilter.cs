using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MS.Internal.SulpHur.SulpHurClient.UIA3
{
    public delegate void FilteredEventHandler();
    public class EventFilter: IDisposable
    {
        public event FilteredEventHandler FilteredStructureChangedEvent;
        //public event FilteredEventHandler FilteredWindowOpenedEvent;

        private static EventFilter _instance;
        private static readonly object _structureChangedEventlock = new object();
        public Thread monitorThread { get; private set; }
        public int structureChangedEventTries = 0;
        //public int windowOpenedEventTries = 0;
        private int interval = 100;

        private EventFilter()
        {
            this.monitorThread = new Thread(new ThreadStart(this.StartMonitoring));
            this.monitorThread.IsBackground = true;
            this.monitorThread.Start();
        }
        public static EventFilter Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new EventFilter();

                return _instance;
            }
        }

        private void StartMonitoring()
        {
            int prevStructureChangedEventTries = 0;
            //int prevWindowOpenedEventTries = 0;
            while (true)
            {
                System.Threading.Thread.Sleep(this.interval);

                ////miss control load event when window open
                //if (!this.windowOpenedEventTries.Equals(0))
                //    this.structureChangedEventTries = 0;

                //StructureChangedEvent
                if (!this.structureChangedEventTries.Equals(0))
                {
                    if (this.structureChangedEventTries.Equals(prevStructureChangedEventTries))
                    {
                        //ThreadPool.QueueUserWorkItem(delegate
                        //{
                        //    //trigger FilteredStructureChangedEvent
                        //    this.FilteredStructureChangedEvent();
                        //});

                        //reset counter
                        lock (_structureChangedEventlock)
                        {
                            this.structureChangedEventTries = 0;
                            prevStructureChangedEventTries = 0;
                        }

                        //trigger FilteredStructureChangedEvent
                        this.FilteredStructureChangedEvent();
                    }
                    else
                    {
                        lock (_structureChangedEventlock)
                        {
                            prevStructureChangedEventTries = this.structureChangedEventTries;
                        }
                    }
                }

                ////WindowOpenedEvent
                //if (!this.windowOpenedEventTries.Equals(0))
                //{
                //    if (this.windowOpenedEventTries.Equals(prevWindowOpenedEventTries))
                //    {
                //        ThreadPool.QueueUserWorkItem(delegate
                //        {
                //            //trigger FilteredWindowOpenedEvent
                //            this.FilteredWindowOpenedEvent();
                //        });

                //        //reset counter
                //        this.windowOpenedEventTries = 0;
                //        prevWindowOpenedEventTries = 0;
                //    }
                //    else
                //    {
                //        prevWindowOpenedEventTries = this.windowOpenedEventTries;
                //    }
                //}
            }
        }

        //public void TryTriggerWindowOpenedEvent()
        //{
        //    this.windowOpenedEventTries++;
        //}
        public void TryTriggerStructureChangedEvent()
        {
            lock (_structureChangedEventlock)
            {
                this.structureChangedEventTries++;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            this.monitorThread.Abort();
            this.monitorThread = null;
            _instance = null;
        }

        ~EventFilter()
        {
            this.Dispose();
        }
    }

}
