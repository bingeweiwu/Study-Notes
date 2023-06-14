using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using interop.UIAutomationCore;

namespace MS.Internal.SulpHur.SulpHurClient.UIA3
{
    public class UIA3AutomationEventHandler : IUIAutomationEventHandler
    {
        private Action<IUIAutomationElement, UIA3AutomationEventArgs> handler;
        public UIA3AutomationEventHandler(Action<IUIAutomationElement, UIA3AutomationEventArgs> handler)
        {
            this.handler = handler;
        }

        public void HandleAutomationEvent(IUIAutomationElement sender, int eventId)
        {
            this.handler(sender, new UIA3AutomationEventArgs(eventId));
        }
    }

    //event arg
    public class UIA3AutomationEventArgs : EventArgs
    {
        public int eventId { get; set; }

        public UIA3AutomationEventArgs(int eventId)
        {
            this.eventId = eventId;
        }
    }
}
