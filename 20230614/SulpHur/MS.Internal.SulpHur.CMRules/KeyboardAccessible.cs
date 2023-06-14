using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.UICompliance;

namespace MS.Internal.SulpHur.CMRules
{
    public class KeyboardAccessible : UIComplianceRuleBase
    {
        public override string Name
        {
            get { return "Keyboard Accessible Rule"; }
        }

        public override string Description
        {
            get { return "All active controls are accessible via keyboard"; }
        }

        public KeyboardAccessible() {
        }

        List<ControlType> kaControls = new List<ControlType>() { 
            ControlType.Edit,
            ControlType.Button,
            ControlType.ComboBox,
            ControlType.List,
            ControlType.CheckBox,
            ControlType.RadioButton
        };

        public override List<UIComplianceResultBase> UIVerify(ICollection<ElementInformation> Controls)
        {
            List<UIComplianceResultBase> results = new List<UIComplianceResultBase>();
            List<ElementInformation> errorControls = new List<ElementInformation>();
            foreach (ElementInformation ei in Controls)
            {
                if (kaControls.Contains(ei.ControlType) && ei.IsEnabled && !ei.IsOffscreen)
                {
                    if (!string.IsNullOrEmpty(ei.AccessKey) ||ei.TabIndex != 0)
                    {
                        continue;
                    }
                    else
                    {
                        errorControls.Add(ei);
                    }
                }
            }
            if (errorControls.Count > 0)
            {
                UIComplianceResultBase fail = new UIComplianceResultBase(ResultType.Fail, "Controls don't have AccessKey and TabIndex.", this.Name);
                foreach (ElementInformation ei in errorControls) {
                    fail.AddRelatedControls(ei);
                }
                results.Add(fail);
            }
            else
            {
                UIComplianceResultBase pass = new UIComplianceResultBase(ResultType.Pass, "No keyboard accessible issue found.", this.Name);
                results.Add(pass);
            }
            return results;
        }

        public override List<UIComplianceResultBase> UIVerify(ICollection<WebElementInfo> webElements)
        {
            throw new NotImplementedException();
        }
    }
}
