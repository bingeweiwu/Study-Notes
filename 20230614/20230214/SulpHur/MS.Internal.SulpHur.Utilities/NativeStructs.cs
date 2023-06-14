using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MS.Internal.SulpHur.Utilities
{
    public delegate void WinEventProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, long idObject, long idChild, int dwEventThread, int dwmsEventTime);

    public sealed class NativeStructs
    {
        [Flags]
        public enum SetWinEventHookFlags
        {
            WINEVENT_INCONTEXT = 4,
            WINEVENT_OUTOFCONTEXT = 0,
            WINEVENT_SKIPOWNPROCESS = 2,
            WINEVENT_SKIPOWNTHREAD = 1
        }

        public enum EventId
        {
            EVENT_MIN = 0x00000001,
            EVENT_SYSTEM_FOREGROUND = 0x03,
            EVENT_SYSTEM_DIALOGSTART = 0x0010,
            EVENT_SYSTEM_DIALOGEND = 0x0011,
            EVENT_SYSTEM_MOVESIZESTART = 0x0A,
            EVENT_SYSTEM_MOVESIZEEND = 0x0B,
            EVENT_SYSTEM_EVENT_SYSTEM_MINIMIZESTART = 0x16,
            EVENT_SYSTEM_EVENT_SYSTEM_MINIMIZEEND = 0x17,
            EVENT_OBJECT_LOCATIONCHANGE = 0x800B,
            EVENT_OBJECT_CREATE = 0x8000,
            EVENT_OBJECT_DESTROY = 0x8001,
            EVENT_OBJECT_SHOW = 0x8002,
            EVENT_OBJECT_HIDE = 0x8003,
            EVENT_MAX = 0x7FFFFFFF
        }

        public enum GaFlags
        {
            GA_PARENT = 1,
            GA_ROOT = 2,
            GA_ROOTOWNER = 3
        }
    }
}
