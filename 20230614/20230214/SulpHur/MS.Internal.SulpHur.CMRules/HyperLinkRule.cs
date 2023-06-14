using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.UICompliance;
using System.Resources;

namespace MS.Internal.SulpHur.CMRules
{
    public class HyperLinkRule : UIComplianceRuleBase
    {
        public HyperLinkRule() {
        }

        public override string Name
        {
            get { return "HyperLink Rule"; }
        }

        public override string Description
        {
            get { return "Check all UIs with hyperlink controls and throw it to warning, manually check needed."; }
        }

        public override List<UIComplianceResultBase> UIVerify(ICollection<ElementInformation> Controls)
        {
            List<ElementInformation> errorControls = new List<ElementInformation>();
            List<UIComplianceResultBase> results = new List<UIComplianceResultBase>();
            foreach (ElementInformation ei in Controls)
            {
                if (ei.ControlType == ControlType.Hyperlink)
                {
                    //if (ei.Parent.Parent.AutomationId == "_navPanel" || ei.Parent.AutomationId == "_navPanel")
                    //{
                    //    continue;
                    //}
                    if (ei.Parent != null && ei.Parent.AutomationId == "_navPanel")
                        continue;
                    else if (ei.Parent.Parent != null && ei.Parent.Parent.AutomationId == "_navPanel")
                        continue;
                    errorControls.Add(ei);
                }
            }
            if (errorControls.Count > 0)
            {
                UIComplianceResultBase warning = new UIComplianceResultBase(ResultType.Warning, "The following control is hyperlink control, need manually check", this.Name);
                foreach (ElementInformation ei in errorControls)
                {
                    warning.AddRelatedControls(ei);
                }
                results.Add(warning);
            }

            if (results.Count == 0)
            {
                UIComplianceResultBase pass = new UIComplianceResultBase(ResultType.Pass, "No hyperlink \"online\" issue found.", this.Name);
                results.Add(pass);
            }
            return results;
        }

        public override List<UIComplianceResultBase> UIVerify(ICollection<WebElementInfo> webElements)
        {
            throw new NotImplementedException();
        }

        //public override string Description
        //{
        //    get { return "Check all the hyperlink which will open internet webpage has text \"Online\" in it."; }
        //}

        //List<string> onlineResources = new List<string>() { 
        //    "online", 
        //    "オンライン", 
        //    "Online", 
        //    "在线", 
        //    "en ligne",
        //    "en línea",
        //    "온라인으로",
        //    "онлайн"};

        //public override List<UIComplianceResultBase> UIVerify(ICollection<ElementInformation> Controls)
        //{
        //    List<ElementInformation> errorControls = new List<ElementInformation>();
        //    List<UIComplianceResultBase> results = new List<UIComplianceResultBase>();
        //    foreach (ElementInformation ei in Controls)
        //    {

        //        if (ei.ControlType == ControlType.Hyperlink)
        //        {
        //            if (ei.Parent.Parent.AutomationId == "_navPanel" || ei.Parent.AutomationId == "_navPanel")
        //            {
        //                continue;
        //            }

        //            bool isEndWithOnline = false;
        //            foreach (string x in onlineResources)
        //            {
        //                if (ei.Name.ToLower().Contains(x))
        //                    isEndWithOnline = true;
        //            }
        //            if (!isEndWithOnline)
        //            {
        //                errorControls.Add(ei);
        //            }
        //        }
        //    }
        //    if (errorControls.Count > 0)
        //    {
        //        UIComplianceResultBase fail = new UIComplianceResultBase(ResultType.Fail, "Hightlight Hyperlink not end with \"online\"", this.Name);
        //        foreach (ElementInformation ei in errorControls) {
        //            fail.AddRelatedControls(ei);
        //        }
        //        results.Add(fail);
        //    }

        //    if (results.Count == 0)
        //    {
        //        UIComplianceResultBase pass = new UIComplianceResultBase(ResultType.Pass, "No hyperlink \"online\" issue found.", this.Name);
        //        results.Add(pass);
        //    }
        //    return results;
        //}

        //public override List<UIComplianceResultBase> UIVerify(ICollection<WebElementInfo> webElements)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
