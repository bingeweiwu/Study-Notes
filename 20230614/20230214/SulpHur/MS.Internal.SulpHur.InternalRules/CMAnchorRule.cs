using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.UICompliance;

namespace MS.Internal.SulpHur.CMRules
{
    public class CMAnchorRule : UIComplianceRuleBase
    {
        const string ruleName = "AnchorRule Rule";
        const string ruleDescrition = "The rule can verify whether there is any text truncation in the controls";

        #region override
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

        /// <summary>
        /// Rule entry point
        /// </summary>
        /// <param name="Controls"></param>
        /// <returns></returns>
        public override List<UIComplianceResultBase> UIVerify(ICollection<ElementInformation> Controls)
        {
            foreach (ElementInformation info in Controls)
            {
                if (info is ManagedElementInformation)
                {
                    ManagedElementInformation mInfo = info as ManagedElementInformation;
                    System.Windows.Forms.Button b1;
                    //object o1 = mInfo.GetProperty("Anchor");
                    //object o2 = mInfo.GetProperty("AutoSize");
                }
            }
            return null;
        }
        #endregion

        public override List<UIComplianceResultBase> UIVerify(ICollection<WebElementInfo> webElements)
        {
            throw new NotImplementedException();
        }
    }
}
