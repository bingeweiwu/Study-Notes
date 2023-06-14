using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using interop.UIAutomationCore;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MS.Internal.SulpHur.SulpHurClient.UIA3
{
    public class UIA3Automation
    {
        //RawInstance
        private static IUIAutomation _rawInstance = null;
        public static IUIAutomation RawInstance
        {
            get 
            {
                if (_rawInstance == null)
                {
                    _rawInstance = new CUIAutomation();
                }
                return _rawInstance;
            }
        }
        //RootElement
        private static IUIAutomationElement rootElement = null;
        public static IUIAutomationElement RootElement
        {
            get
            {
                if (rootElement == null)
                    rootElement = RawInstance.GetRootElement();

                return rootElement;
            }
        }

        //automation event
        public static void AddAutomationEventHandler(int eventId, IUIAutomationElement element, TreeScope scope, UIA3AutomationEventHandler handler)
        {
      //      ThreadPool.QueueUserWorkItem(delegate
      //      {
                try
                {
                    RawInstance.AddAutomationEventHandler(eventId, element, scope, null, handler);
                    Trace.WriteLine(string.Format("******debug********* UIAutomationEvent {0} is added", eventId.ToString()));
                }
                catch (COMException)
                {
                    Trace.WriteLine("COMException happens during RemoveAutomationEventHandler.");
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
                System.Diagnostics.Trace.WriteLine(string.Format("AddAutomationEventHandler.ThreadId: {0}", Thread.CurrentThread.ManagedThreadId));
        //    });
        }
        public static void RemoveAutomationEventHandler(int eventId, IUIAutomationElement element, UIA3AutomationEventHandler handler)
        {
       //     ThreadPool.QueueUserWorkItem(delegate
       //     {
                try
                {
                    RawInstance.RemoveAutomationEventHandler(eventId, element, handler);
                    Trace.WriteLine(string.Format("******debug********* UIAutomationEvent {0} is removed", eventId.ToString()));
                }
                catch (COMException)
                {
                    Trace.WriteLine("COMException happens during RemoveAutomationEventHandler.");
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
       //     });
        }

        //structure changed event
        public static void AddStructureChangedEventHandler(IUIAutomationElement element, TreeScope scope, UIA3StructureChangedEventHandler handler)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    RawInstance.AddStructureChangedEventHandler(element, scope, null, handler);
                }
                catch (COMException)
                {
                    Trace.WriteLine("COMException happens during AddStructureChangedEventHandler.");
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
                //System.Diagnostics.Trace.WriteLine(string.Format("AddStructureChangedEventHandler.ThreadId: {0}", Thread.CurrentThread.ManagedThreadId));
            });
        }
        public static void RemoveStructureChangedEventHandler(IUIAutomationElement element, UIA3StructureChangedEventHandler handler)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    RawInstance.RemoveStructureChangedEventHandler(element, handler);
                }
                catch (COMException)
                {
                    Trace.WriteLine("COMException happens during RemoveStructureChangedEventHandler.");
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
                //System.Diagnostics.Trace.WriteLine(string.Format("AddStructureChangedEventHandler.ThreadId: {0}", Thread.CurrentThread.ManagedThreadId));
            });
        }
    }
}
