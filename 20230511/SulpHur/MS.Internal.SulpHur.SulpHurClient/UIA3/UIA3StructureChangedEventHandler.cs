using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using interop.UIAutomationCore;

namespace MS.Internal.SulpHur.SulpHurClient.UIA3
{
    public class UIA3StructureChangedEventHandler : IUIAutomationStructureChangedEventHandler
    {
        private Action<IUIAutomationElement, UIA3StructureChangedEventArgs> handler;
        public UIA3StructureChangedEventHandler(Action<IUIAutomationElement, UIA3StructureChangedEventArgs> handler)
        {
            this.handler = handler;
        }

        public void HandleStructureChangedEvent(IUIAutomationElement sender, StructureChangeType changeType, int[] runtimeId)
        {
            this.handler(sender, new UIA3StructureChangedEventArgs(changeType, runtimeId));
        }
    }

    //event arg
    public class UIA3StructureChangedEventArgs : EventArgs
    {
        public StructureChangeType changeType { get; set; }
        public int[] runtimeId { get; set; }

        public UIA3StructureChangedEventArgs(StructureChangeType changeType, int[] runtimeId)
        {
            this.changeType = changeType;
            this.runtimeId = runtimeId;
        }
    }
}
