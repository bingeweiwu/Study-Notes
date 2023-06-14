using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.UICompliance;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace MS.Internal.SulpHur.CMRules
{
    public class CMControlTruncationRule : UIComplianceRuleBase
    {
        const string ruleName = "Control Truncation Rule";
        const string ruleDescrition = "The rule can verify whether there is any truncation between the control and its parent control";

        #region Control's Margin
        private static System.Windows.Forms.Padding managePushButtonDefaultMargin = new System.Windows.Forms.Padding(8, 5, 6, 0);
        private static System.Windows.Forms.Padding manageLabelDefaultMargin = new System.Windows.Forms.Padding(0, 0, 0, 0);
        private static System.Windows.Forms.Padding manageRadioDefaultMargin = new System.Windows.Forms.Padding(17, 2, 8, 0);
        private static System.Windows.Forms.Padding manageCheckDefaultMargin = new System.Windows.Forms.Padding(17, 2, 8, 0);
        private static System.Windows.Forms.Padding managedGroupBoxDefaultmargin = new System.Windows.Forms.Padding(10, 0, 25, 0);

        private static System.Windows.Forms.Padding ZeroMargin = new System.Windows.Forms.Padding(0, 0, 0, 0);
        #endregion

        public override string Name
        {
            get
            {
                return ruleName;
            }
        }

        public override string Description
        {
            get
            {
                return ruleDescrition;
            }
        }

        public CMControlTruncationRule()
        {
        }

        /// <summary>
        /// Rule entry point
        /// </summary>
        /// <param name="Controls"></param>
        /// <returns></returns>
        public override List<UIComplianceResultBase> UIVerify(ICollection<ElementInformation> Controls)
        {
            List<UIComplianceResultBase> results = new List<UIComplianceResultBase>();
            try
            {
                //List<ElementInformation> myWindows = new List<ElementInformation>(Windows);

                // Get Parent Window
                IEnumerator<ElementInformation> enumerator = Controls.GetEnumerator();
                enumerator.MoveNext();
                ElementInformation Parent = enumerator.Current;

                // Check each controls of that window
                foreach (ElementInformation winInfo in Controls)
                {
                
                    ////ignore group used as split
                    //if (winInfo.ControlType == ControlType.Group && winInfo.Height <= 10) {
                    //    continue;
                    //}

                    if (winInfo.Name == null || winInfo.Name.Length == 0)
                        continue;

                    // Check each control and add the result to container
                    UIComplianceResultBase tempResult = null;
                    if (winInfo.Parent != null)
                    {
                        if (IsInContainerHasScrollBar(winInfo) == true)
                        { continue; }
                        tempResult = CommonControlTruncationCheck(winInfo.Parent, winInfo);
                    }

                    if (tempResult != null)
                    {
                        tempResult.AddRelatedControls(winInfo);
                        results.Add(tempResult);
                    }

                }

                // If no Error Result, Create a Pass Result for the window
                if (results.Count == 0)
                {
                    UIComplianceResultBase passResult = new UIComplianceResultBase(ResultType.Pass, "No truncation found in window : " + Parent.Name, ruleName);
                    results.Add(passResult);
                }

                return results;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return results;
            }
        }



        private bool IsInContainerHasScrollBar(ElementInformation ei)
        {
            if (ei.Parent.Children.Any(c => c.ControlType == ControlType.ScrollBar)) return true;
            return false;
        }

        private UIComplianceResultBase CommonControlTruncationCheck(ElementInformation parent, ElementInformation child)
        {



            if (parent.BoundingRectangle.Left > child.BoundingRectangle.Left || parent.BoundingRectangle.Right < child.BoundingRectangle.Right
                || parent.BoundingRectangle.Top > child.BoundingRectangle.Top || parent.BoundingRectangle.Bottom + 1 < child.BoundingRectangle.Bottom)
            // OK,Cancel,Apply button bottom often below parent control with size 1.
            {
                string message = string.Format("[Name:{0}, ControlType:{1}, Rectangle:{2}] is truncated."
                , child.Name, child.ControlType, child.BoundingRectangle);
                //string message = "The " + winInfo.ControlType.ToString() + " control : '" + winInfo.Name + "' is truncated!";
                return (new UIComplianceResultBase(ResultType.Fail, message, this.Name));

            }
            return null;
        }



        public override List<UIComplianceResultBase> UIVerify(ICollection<WebElementInfo> webElements)
        {
            throw new NotImplementedException();
        }
    }
    
}
