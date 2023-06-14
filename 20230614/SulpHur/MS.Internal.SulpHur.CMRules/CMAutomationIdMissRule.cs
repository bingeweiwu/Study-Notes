using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.UICompliance;

namespace MS.Internal.SulpHur.CMRules
{
    public class CMAutomationIdMissRule : UIComplianceRuleBase
    {
        const string ruleName = "Automation ID Miss Rule";
        const string ruleDescrition = "The rule can verify the control has automation ID or not";

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

        public CMAutomationIdMissRule()
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
            if (this.isOSidentity == false)
            {
                results.Add(new UIComplianceResultBase(ResultType.Warning, "This UI is already verified in different OS type.\n", Name));
                return results;
            }
            else
            {
                try
                {
                    string errorInfo = "";

                    // Check each controls of that window
                    foreach (ElementInformation winInfo in Controls)
                    {
                        //ignore error provider
                        if (winInfo.ControlType == ControlType.Pane && (winInfo.Children == null || (winInfo.Children != null && winInfo.Children.Count == 0)))
                        {
                            continue;
                        }

                        //ignore confirm/error/warning dialog
                        if (winInfo.ClassName != null)
                        {
                            if (winInfo.ClassName.Contains("32770"))
                            {
                                continue;
                            }
                        }

                        //ignore Menu / Tool Tip
                        if (winInfo.ControlType == ControlType.Menu || winInfo.ControlType == ControlType.ToolTip)
                        {
                            continue;
                        }

                        //ignore List
                        if (winInfo.ControlType == ControlType.List && (winInfo.Parent == null || (winInfo.Parent != null && winInfo.Parent.ControlType == ControlType.ComboBox)))
                        {
                            continue;
                        }

                        //ignore hyperlinktext
                        if (winInfo.ControlType == ControlType.Hyperlink && (winInfo.Parent == null || (winInfo.Parent != null && winInfo.Parent.ControlType == ControlType.Text)))
                        {
                            continue;
                        }

                        if (winInfo.AutomationId == null || winInfo.AutomationId.Trim() == "")
                        {
                            if (!string.IsNullOrEmpty(winInfo.Name))
                            {
                                errorInfo = string.Format("[Name:{0}, ControlType:{1}] has no Automation ID,Please fix it if it’s your new code.", winInfo.Name, winInfo.ControlType);

                            }
                            else
                            {
                                errorInfo = string.Format("[Name:{0}, ControlType:{1}] has no Automation ID,Please fix it if it’s your new code.", "No Name", winInfo.ControlType);

                            }
                            UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Warning, errorInfo, this.Name);
                            result.AddRelatedControls(winInfo);
                            results.Add(result);

                        }
                    }

                    // If no Error Result, Create a Pass Result for the window
                    if (results.Count == 0)
                    {
                        UIComplianceResultBase passResult = new UIComplianceResultBase(ResultType.Pass, "No Automation ID Missing.", ruleName);
                        results.Add(passResult);
                    }
                    return results;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }


        public override List<UIComplianceResultBase> UIVerify(ICollection<WebElementInfo> webElements)
        {
            throw new NotImplementedException();
        }
    }

}

