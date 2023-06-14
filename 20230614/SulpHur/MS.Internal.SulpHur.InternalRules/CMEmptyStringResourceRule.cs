using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.UICompliance;
using System.Drawing;

namespace MS.Internal.SulpHur.CMRules
{
    public class EmptyStringResourceRule : UIComplianceRuleBase
    {
        const string ruleName = "String Mis-Resource Rule";
        const string ruleDescrition = "The rule verify whether the control have empty string resource with it";

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
            List<UIComplianceResultBase> results = new List<UIComplianceResultBase>();
            //List<ElementInformation> myWindows = new List<ElementInformation>(Windows);

            // Get Parent Window
            IEnumerator<ElementInformation> enumerator = Controls.GetEnumerator();
            enumerator.MoveNext();
            ElementInformation Parent = enumerator.Current;

            // Check each controls of that window
            foreach (ElementInformation winInfo in Controls)
            {
                // Check whether there is any control has empty text
                UIComplianceResultBase tempResult = null;
                if (winInfo.ControlType == ControlType.Text || winInfo.ControlType == ControlType.CheckBox ||
                    winInfo.ControlType == ControlType.Button || winInfo.ControlType == ControlType.RadioButton)
                {

                    // Exceptions
                    if (winInfo.ControlType == ControlType.Button)
                    {
                        ManagedElementInformation managedInfo = (ManagedElementInformation)winInfo;
                        if(managedInfo.IsImageButton)continue;
                    }

                    string controlName = "(Error evaluating the name of the control)";
                    if (winInfo.ControlType == ControlType.Text)
                    {
                        try
                        {
                            ManagedElementInformation mWinInfo = winInfo as ManagedElementInformation;
                            controlName = mWinInfo.Name.ToString();
                            if (controlName == "_headlineLabel")
                                continue;
                        }
                        catch { }

                        try
                        {
                            ManagedElementInformation mWinInfo = winInfo.Parent as ManagedElementInformation;
                            if (mWinInfo.Name.ToString() == "_navPanel")
                                continue;
                        }
                        catch { }
                    }

                    if (winInfo.Name == null || winInfo.Name.Length == 0)
                    {
                        tempResult = new UIComplianceResultBase(ResultType.Warning, winInfo.ControlType.ToString() + "(" + controlName + ") is using the mis-string resource", ruleName);
                        //tempresult.TakePicture = true;
                        tempResult.AddRelatedControls(winInfo);
                        results.Add(tempResult);
                    }
                }
            }

            // If no Error Result, Create a Pass Result for the window
            if (results.Count == 0)
            {
                UIComplianceResultBase passResult = new UIComplianceResultBase(ResultType.Pass, "No Mis-string resource is found in window : " + Parent.Name, ruleName);
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
