using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using MS.Internal.SulpHur.Utilities;
using interop.UIAutomationCore;
using MS.Internal.SulpHur.SulpHurClient.UIA3;
using MS.Internal.SulpHur.Utilities.Exceptions;

namespace MS.Internal.SulpHur.SulpHurClient.Monitors
{
    public class ElementChangeMonitor
    {
        private IUIAutomationElement objPageIdentifyControl = null;
        private bool hasPageIdentifyControl = false;
        private bool hasException = false;

        public ElementChangeMonitor(IUIAutomationElement uiObject)
        {
            try
            {
                this.hasPageIdentifyControl = Utility.TryGetPageIdentifyControl(uiObject, out this.objPageIdentifyControl);
                //detect if page complete loading
                if (this.hasPageIdentifyControl)
                {
                    string firstChildAutomationId = this.objPageIdentifyControl.FirstChild().CurrentAutomationId;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Fail to get page identifier.ex:{ex.Message}");
                throw new SulpHurClientUINotCompelteLoading();
            }
        }

        public bool IsChanged
        {
            get
            {
                if (this.hasException)
                {
                    return true;
                }

                if (this.hasPageIdentifyControl)
                {
                    try
                    {
                        //check if page identify control exist
                        IUIAutomationElement objFirstChild = this.objPageIdentifyControl.FirstChild();
                        string automationId = objFirstChild.CurrentAutomationId;
                        return false;
                    }
                    catch (NullReferenceException)
                    {
                        Trace.WriteLine("Page changed.");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(string.Format("IsChanged: {0}", ex));
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
