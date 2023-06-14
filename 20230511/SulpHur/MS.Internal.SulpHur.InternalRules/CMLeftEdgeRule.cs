using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.UICompliance;

namespace MS.Internal.SulpHur.CMRules
{
    public class LeftEdgeRule : UIComplianceRuleBase
    {
        //disable this rule by default
        public LeftEdgeRule()
        {
            base.IsEnabled = false;
        }

        const string ruleName = "Left Edge Rule";
        const string ruleDescrition = @"LeftEdgeRule is used to check whether the edge of left side of control in a page control is 12";

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

                LeftEdgeRuleVerify(results);
                return results;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region utility
        void LeftEdgeRuleVerify(List<UIComplianceResultBase> results)
        {
            ElementInformation f1 = mPageControl.FirstChild;
            while (f1 != null)
            {
                if (IsTheLeftSideControl(f1))
                {
                    double lEdge = f1.BoundingRectangle.Left - mPageControl.BoundingRectangle.Left;
                    if (lEdge != 12)
                    {
                        string str = string.Format("The left edge for page control of the control[{0}  {1}   {2}] is not 12, it is {3}",
                            f1.ControlType, f1.Name, f1.AccessKey, lEdge);
                        UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Warning, str, Name);
                        result.AddRelatedControls(f1);
                        results.Add(result);
                    }
                }
                f1 = f1.NextSibling;
            }
        }


        #endregion

        #region utility
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

        bool IsTheLeftSideControl(ElementInformation info)
        {
            if (info.Parent != mPageControl)
                return false;
            ElementInformation f1 = mPageControl.FirstChild;
            while (f1 != null)
            {
                if (f1 != info && IsLeftOf(f1, info))
                {
                    return false;
                }
                f1 = f1.NextSibling;
            }

            return true;

        }

        bool IsTheRightSideControl(ElementInformation info)
        {
            if (info.Parent != mPageControl)
                return false;
            ElementInformation f1 = mPageControl.FirstChild;
            while (f1 != null)
            {
                if (f1 != info && IsLeftOf(info, f1))
                {
                    return false;
                }
                f1 = f1.NextSibling;
            }

            return true;

        }

        bool IsLeftOf(ElementInformation left, ElementInformation right)
        {
            if (right.BoundingRectangle.Y > left.BoundingRectangle.Y && right.BoundingRectangle.Y < left.BoundingRectangle.Y + left.BoundingRectangle.Height)
            {
                if (right.BoundingRectangle.X > left.BoundingRectangle.X + left.BoundingRectangle.Width)
                    return true;
            }
            else if (right.BoundingRectangle.Y + right.BoundingRectangle.Height > left.BoundingRectangle.Y
                && +right.BoundingRectangle.Height < left.BoundingRectangle.Y + left.BoundingRectangle.Height)
            {
                if (right.BoundingRectangle.X > left.BoundingRectangle.X + left.BoundingRectangle.Width)
                    return true;
            }
            return false;
        }
        #endregion

        public override List<UIComplianceResultBase> UIVerify(ICollection<WebElementInfo> webElements)
        {
            throw new NotImplementedException();
        }
    }
}
