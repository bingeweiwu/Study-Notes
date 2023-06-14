using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.UICompliance;

namespace MS.Internal.SulpHur.InternalRules
{
    public class CMQueryAllUIRule : UIComplianceRuleBase
    {
        public override string Name
        {
            get { return "All ScreenShot Rule"; }
        }

        public override string Description
        {
            get { return "This rule used for quey all UI."; }
        }

        public override List<UIComplianceResultBase> UIVerify(ICollection<ElementInformation> Controls)
        {
            UIComplianceResultBase res = new UIComplianceResultBase(ResultType.Pass, "", this.Name);
            List<UIComplianceResultBase> list = new List<UIComplianceResultBase>();
            list.Add(res);
            return list;
        }

        public override List<UIComplianceResultBase> UIVerify(ICollection<WebElementInfo> webElements)
        {
            throw new NotImplementedException();
        }
    }
}
