using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.UICompliance;

namespace MS.Internal.SulpHur.CMRules
{
    public class FirstControlRule : UIComplianceRuleBase
    {
        //disable this rule by default
        public FirstControlRule()
        {
            base.IsEnabled = false;
        }

        const string ruleName = "First Control Rule";
        const string ruleDescrition = "FirstControlRule is used to verify the first control in PageControl is [12,12]";

        ElementInformation mPageControl = null;

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
            try
            {
                List<UIComplianceResultBase> results = new List<UIComplianceResultBase>();
                FindPageControl(Controls.ToList());
                if (mPageControl == null)
                {
                    string wInfo = string.Format("Cannot find pagecontrol in the current page");
                    UIComplianceResultBase tempResult = new UIComplianceResultBase(ResultType.Fail, wInfo, ruleName);
                    results.Add(tempResult);
                    return results;
                }

                FirstControlRuleVerify(results);
                return results;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region utility
        void FirstControlRuleVerify(List<UIComplianceResultBase> results)
        {
            ElementInformation f1 = mPageControl.FirstChild;
            ElementInformation minControl = null;
            double minX = 10000.0;
            double minY = 10000.0;

            while (f1 != null)
            {
                double x = f1.BoundingRectangle.X - mPageControl.BoundingRectangle.X;
                double y = f1.BoundingRectangle.Y - mPageControl.BoundingRectangle.Y;
                if (y < minY)
                {
                    minY = y;
                    minX = x;
                    minControl = f1;
                }
                else if (y == minY)
                {
                    if (x < minX)
                    {
                        minY = y;
                        minX = x;
                        minControl = f1;
                    }
                }
                f1 = f1.NextSibling;
            }

            //if (minControl is ManagedElementInformation)
            //{
            //    try
            //    {
            //        Point size = (Point)(minControl as ManagedElementInformation).GetProperty("Location");
            //        minX = size.X;
            //        minY = size.Y;
            //    }
            //    catch
            //    {
            //    }
            //}

            if (minX != 12 || minY != 12)
            {
                string wInfo = string.Format("The position of first control in PageControl should be [12, 12], it is [{0}, {1}] now"
                    , minX, minY);
                UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Warning, wInfo, Name);
                result.AddRelatedControls(minControl);
                results.Add(result);
            }


            //if (mPageControl != null)
            //{
            //    ElementInformation control = mPageControl.FirstChild;
            //    int x = control.BoundingRectangle.X - mPageControl.BoundingRectangle.X;
            //    int y = control.BoundingRectangle.Y - mPageControl.BoundingRectangle.Y;
            //    if (x != 12 || y != 12)
            //    {
            //        string wInfo = string.Format("The position of first control in PageControl should be [12, 12], it is [{0}, {1}] now"
            //            , x, y);
            //        UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Warning, wInfo, Name, true);
            //        result.AddRelatedControls(control);
            //        results.Add(result);
            //    }
            //}
        }

        void FindPageControl(List<ElementInformation> controls)
        {
            foreach (ElementInformation info in controls)
            {
                if (info.Name != null && info.Name.StartsWith("Microsoft.ConfigurationManagement"))
                {
                    mPageControl = info;
                    return;
                }
            }
        }
        #endregion

        public override List<UIComplianceResultBase> UIVerify(ICollection<WebElementInfo> webElements)
        {
            throw new NotImplementedException();
        }
    }
}
