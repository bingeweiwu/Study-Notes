using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.UICompliance;

namespace MS.Internal.SulpHur.CMRules
{
    public class HelpTopicRule : UIComplianceRuleBase
    {

        public override string Name
        {
            get { return "Help Topic Rule"; }
        }

        public override string Description
        {
            get { return "This rule used to check if dialog contains help topic icon."; }
        }

        public HelpTopicRule()
        {
        }

        public override List<UIComplianceResultBase> UIVerify(ICollection<ElementInformation> Controls)
        {
            List<UIComplianceResultBase> results = new List<UIComplianceResultBase>();
            ElementInformation root = CommonRuleUtility.GetRootElement(Controls);
            if (root.AutomationId == "SccmPageControlDialog"
                || root.AutomationId == "SheetFramework"
                || root.AutomationId == "SmsWizardForm")
            {
                if (Controls.Any(c => (c.AutomationId == "smsHelpLink" || c.AutomationId == "helpLink") && (c.ControlType == ControlType.Text || c.ControlType == ControlType.Hyperlink)))
                {
                    UIComplianceResultBase fail = new UIComplianceResultBase(ResultType.Fail, "helptopic link should be removed.", this.Name);
                    results.Add(fail);
                }
            }
            //else
            //{
            //    results.Add(new UIComplianceResultBase(ResultType.Pass, "This dialog don't have help topic link.", this.Name));
            //}
            if (results.Count == 0)
            {
                results.Add(new UIComplianceResultBase(ResultType.Pass, "No Help Topic Issue found.", this.Name));
            }
            return results;
        }

        public override List<UIComplianceResultBase> UIVerify(ICollection<WebElementInfo> webElements)
        {
            throw new NotImplementedException();
        }
    }
}
