using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.UICompliance;

namespace MS.Internal.SulpHur.CMRules
{
    public class CMDivide3Rule : UIComplianceRuleBase
    {
        //disable this rule by default
        public CMDivide3Rule()
        {
            base.IsEnabled = false;
        }
        const string ruleName = "Divide 3 Rule";
        const string ruleDescrition = "Divide 3 Rule is used to verify the size of control can divide 3.";

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
            List<UIComplianceResultBase> results = new List<UIComplianceResultBase>();
            try
            {
                FindPageControl(Controls.ToList());
                if (mPageControl == null)
                {
                    string wInfo = string.Format("Cannot find pagecontrol in the current page");
                    UIComplianceResultBase tempResult = new UIComplianceResultBase(ResultType.Fail, wInfo, ruleName);
                    results.Add(tempResult);
                    return results;
                }

                Divide3RuleVerify(results);
                return results;
            }
            catch (Exception ex)
            {
                ////WriteLineToLog(ex.Message);
                ////WriteLineToLog(ex.StackTrace);
                return results;
            }
        }

        //public static void WriteToDailyCopyLog(string line)
        //{
        //    try
        //    {
        //        string mCopyDLLog = "c:\\telescope\\Divide3.txt";
        //        System.IO.FileInfo fi = new System.IO.FileInfo(mCopyDLLog);
        //        if (!fi.Directory.Exists)
        //            fi.Directory.Create();
        //        System.IO.FileStream fs = new System.IO.FileStream(mCopyDLLog, System.IO.FileMode.Append, System.IO.FileAccess.Write);
        //        System.IO.StreamWriter sw = new System.IO.StreamWriter(fs);
        //        sw.WriteLine(string.Format("[{0}] {1}", DateTime.Now.ToString(), line));
        //        sw.Close();
        //        fs.Close();
        //    }
        //    catch
        //    {
        //        //eat it
        //    }
        //}
        #endregion

        #region utility
        void Divide3RuleVerify(List<UIComplianceResultBase> results)
        {
            if (mPageControl != null)
            {
                Divide3RuleVerify(mPageControl, results);
            }
        }

        void Divide3RuleVerify(ElementInformation control, List<UIComplianceResultBase> results)
        {
            if (control != mPageControl)
            {
                double width = control.BoundingRectangle.Width;
                double height = control.BoundingRectangle.Height;

                double X = control.BoundingRectangle.X;
                double Y = control.BoundingRectangle.Y;
                StringBuilder sb = new StringBuilder();
                if (width % 3 != 0)
                {
                    sb.AppendLine(string.Format("The width of control [{0} {1}] can not be divied by 3, it is {2} now.",
                        control.Name, control.ControlType, width));
                }
                else if (height % 3 != 0)
                {
                    sb.AppendLine(string.Format("The height of control [{0} {1}] can not be divied by 3, it is {2} now.",
                        control.Name, control.ControlType, height));
                }
                //if (X % 3 != 0)
                //{
                //    sb.AppendLine(string.Format("The X of control [{0} {1}] can not be divied by 3, it is {2} now.",
                //        control.Name, control.ControlType, X));
                //}
                //if (Y % 3 != 0)
                //{
                //    sb.AppendLine(string.Format("The Y of control [{0} {1}] can not be divied by 3, it is {2} now.",
                //        control.Name, control.ControlType, Y));
                //}
                if (sb.Length > 0)
                {
                    UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Warning, sb.ToString(), Name);
                    result.AddRelatedControls(control);
                    results.Add(result);
                }
            }

            ElementInformation c1 = null;
            if (control.FirstChild != null)
            {
                c1 = control.FirstChild;
                Divide3RuleVerify(c1, results);
                c1 = c1.NextSibling;
            }

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
