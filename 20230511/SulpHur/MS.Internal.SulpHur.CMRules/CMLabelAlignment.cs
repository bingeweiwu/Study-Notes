﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.UICompliance;

namespace MS.Internal.SulpHur.CMRules
{
    public class CMLabelAlignment : UIComplianceRuleBase
    {
        #region rule's Name & Description
        const string ruleName = "LabelText Rule";
        const string ruleDescrition = "The rule can verify the alignment between label and textbox";

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
        #endregion

        int DISTANCE = 8;

        public CMLabelAlignment()
        {
        }

        public override List<UIComplianceResultBase> UIVerify(ICollection<ElementInformation> Controls)
        {
            List<ElementInformation> lControls = new List<ElementInformation>(Controls);
            List<UIComplianceResultBase> results = new List<UIComplianceResultBase>();
            for (int i = 0; i < lControls.Count - 1; i++)
            {
                ElementInformation w1 = lControls[i];
                if (w1.ControlType == ControlType.Edit || w1.ControlType == ControlType.Text)
                {
                    for (int j = i + 1; j < lControls.Count; j++)
                    {
                        ElementInformation w2 = lControls[j];
                        if ((w1.ControlType == ControlType.Edit && (w2.ControlType == ControlType.Text && w2.Parent.AutomationId != "_navPanel"))
                            || (w1.ControlType == ControlType.Text && w2.Parent.AutomationId != "_navPanel") && w2.ControlType == ControlType.Edit)
                        {
                            if (Math.Abs(w1.Bottom - w2.Bottom) < DISTANCE)
                            {
                                int dist1 = (int)(w1.Bottom - w2.Bottom);
                                int dist2 = (int)(w2.Top - w1.Top);
                                if (Math.Abs(dist1 - dist2) > 1)
                                {
                                    UIComplianceResultBase failResult = new UIComplianceResultBase(ResultType.Fail, "Label textbox not aligned", ruleName);
                                    failResult.AddRelatedControls(w1);
                                    failResult.AddRelatedControls(w2);
                                    //failresult.TakePicture = true;
                                    results.Add(failResult);

                                }
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }

            // If no Error Result, Create a Pass Result for the window
            if (results.Count == 0)
            {
                // Get Parent Window
                IEnumerator<ElementInformation> enumerator = Controls.GetEnumerator();
                enumerator.MoveNext();
                ElementInformation Parent = enumerator.Current;

                UIComplianceResultBase passResult = new UIComplianceResultBase(ResultType.Pass, "No CMLabelAlignment Compliance found in window : " + Parent.Name, ruleName);
                results.Add(passResult);
            }

            return results;
        }

        public override List<UIComplianceResultBase> UIVerify(ICollection<WebElementInfo> webElements)
        {
            throw new NotImplementedException();
        }
    }
}
