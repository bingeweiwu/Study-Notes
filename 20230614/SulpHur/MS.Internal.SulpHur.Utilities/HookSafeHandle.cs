using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32.SafeHandles;
using System.Runtime.ConstrainedExecution;
using System.Security.Permissions;

namespace MS.Internal.SulpHur.Utilities
{
    [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
    public class HookSafeHandle : SafeHandleZeroOrMinusOneIsInvalid 
    {
        public HookSafeHandle()
            : base(true)
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        protected override bool ReleaseHandle()
        {
            if (this.IsInvalid)
            {
                return true;
            }

            if (NativeMethods.UnhookWinEvent(this.handle) == 0)
            {
                this.SetHandle(IntPtr.Zero);
                return true;
            }

            return false;
        }
    }
}
