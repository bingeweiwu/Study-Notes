using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.UICompliance;

namespace MS.Internal.SulpHur.CMRules
{
    public class CMButtonRule : UIComplianceRuleBase
    {
        const string ruleName = "Button Rule";
        const string ruleDescrition = "Button Rule for verify default button size.";


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
            List<UIComplianceResultBase> results = new List<UIComplianceResultBase>();
            foreach (ElementInformation control in Controls)
            {
                switch (control.ControlType)
                {
                    case MS.Internal.SulpHur.UICompliance.ControlType.Button:
                        {
                            ButtonRuleVerify(control, results);
                            break;
                        }
                    default:
                        break;
                }
            }
            return results;
        }

        #endregion

        public void ButtonRuleVerify(ElementInformation button, List<UIComplianceResultBase> results)
        {
            double width = button.BoundingRectangle.Width;
            double height = button.BoundingRectangle.Height;

            //try
            //{
            //    if (button is ManagedElementInformation)
            //    {
            //        Size o1 = (Size)(button as ManagedElementInformation).GetProperty("Size");
            //        width = o1.Width;
            //        height = o1.Height;

            //    }
            //}
            //catch
            //{
            //}

            //try
            //{
            //    if (button is ManagedElementInformation)
            //    {
            //        ManagedElementInformation mb = button as ManagedElementInformation;
            //        System.Drawing.Image image = (System.Drawing.Image)mb.GetProperty("Image");
            //        if (image != null)
            //            return;
            //    }
            //}
            //catch
            //{
            //}

            if (width < 100 && height < 50 && (width - height) < 20)
            {
                if (width == 75 && height == 23)
                    return;
                else
                {
                    string wInfo = string.Format("The size of button may be set to [75, 23]? Current is [{0}, {1}]", width, height);
                    UIComplianceResultBase tempResult = new UIComplianceResultBase(ResultType.Warning, wInfo, ruleName);

                    tempResult.AddRelatedControls(button);
                    results.Add(tempResult);
                }
            }

            if (height != 23)
            {
                string wInfo = string.Format("The height of button should be set to 23. Current button size is [{0}, {1}]", width, height);
                UIComplianceResultBase tempResult = new UIComplianceResultBase(ResultType.Warning, wInfo, ruleName);
                if (height < 23)
                    tempResult.Type = ResultType.Fail;

                tempResult.AddRelatedControls(button);
                results.Add(tempResult);
            }

        }

        public override List<UIComplianceResultBase> UIVerify(ICollection<WebElementInfo> webElements)
        {
            throw new NotImplementedException();
        }
    }
}
