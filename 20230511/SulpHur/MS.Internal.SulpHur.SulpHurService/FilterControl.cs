using MS.Internal.SulpHur.UICompliance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MS.Internal.SulpHur.SulpHurService
{
    class FilterControl
    {
        /// <summary>
        /// filter controls of the property IsOffscreen = true
        /// </summary>
        /// <param name="Controls"></param>
        /// <returns></returns>
        public static List<ElementInformation> FilterCon(ICollection<ElementInformation> Controls)
        {
            List<ElementInformation> filterControls = new List<ElementInformation>();
            foreach (var control in Controls)
            {
                //This causes the root control to become a child of the outermost control
                if (control.ControlType == ControlType.ToolTip /*|| control.ControlType == ControlType.Window */|| control.ControlType == ControlType.TreeItem || control.ControlType == ControlType.Tree || control.ControlType == ControlType.Menu || control.ControlType == ControlType.MenuItem)
                {
                    continue;
                }
                if (!control.IsOffscreen)
                {
                    filterControls.Add(control);
                }
            }
            return filterControls;
        }
    }
}
