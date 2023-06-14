using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace MS.Internal.SulpHur.UICompliance
{
    public class ControlScreen
    {
        public ControlScreen() { }

        public bool CommonScreen(ICollection<ElementInformation> Controls, out string log)
        {
            log = string.Empty;
            if (Controls == null || Controls.Count == 0)
            {
                log = "Controls is null or empty";
                return false;
            }

            return true;
        }

        public static Bitmap CurrentBit;
    }
}
