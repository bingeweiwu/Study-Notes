using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MS.Internal.SulpHur.SulpHurClient.Metro.Controls
{
    public class ClosingWindowEventHandlerArgs : EventArgs
    {
        public bool Cancelled { get; set; }
    }
}
