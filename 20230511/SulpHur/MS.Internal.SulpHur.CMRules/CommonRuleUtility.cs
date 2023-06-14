using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.UICompliance;
using System.Drawing;

namespace MS.Internal.SulpHur.CMRules
{
    public class CommonRuleUtility
    {
        static int controlNameLength = int.Parse(System.Configuration.ConfigurationManager.AppSettings["controlNameLength"]);
        public static UIComplianceResultBase CreateResult(ResultType resultType, string msg, string ruleName, ICollection<ElementInformation> RelatedControls)
        {
            UIComplianceResultBase r = new UIComplianceResultBase(resultType, msg, ruleName);
            foreach (ElementInformation ei in RelatedControls)
            {
                r.AddRelatedControls(ei);
            }
            return r;
        }
        public static UIComplianceResultBase CreateWarningResult(string msg, string ruleName, ICollection<ElementInformation> RelatedControls)
        {
            return CreateResult(ResultType.Warning, msg, ruleName, RelatedControls);
        }
        public static UIComplianceResultBase CreatePassResult(string msg, string ruleName, ICollection<ElementInformation> RelatedControls)
        {
            return CreateResult(ResultType.Pass, msg, ruleName, RelatedControls);
        }
        public static UIComplianceResultBase CreateFailResult(string msg, string ruleName, ICollection<ElementInformation> RelatedControls)
        {
            return CreateResult(ResultType.Fail, msg, ruleName, RelatedControls);
        }
        public static ElementInformation GetRootElement(ICollection<ElementInformation> Controls)
        {
            IEnumerator<ElementInformation> enumerator = Controls.GetEnumerator();
            enumerator.Reset();
            enumerator.MoveNext();
            ElementInformation rootControl = enumerator.Current;
            while (rootControl.Parent != null)
                rootControl = rootControl.Parent;
            return rootControl;
        }
        public static List<ElementInformation> GetHidenControls(ElementInformation Parent)
        {
            ElementInformation c1 = Parent.FirstChild;
            if (c1 == null)
                return null;
            ElementInformation c2 = c1.NextSibling;
            if (c2 == null)
                return null;
            if (Math.Abs(c1.X - c2.X) < 5 && Math.Abs(c1.Y - c2.Y) < 5
                && c1.Width > 5 && c1.Height > 5 && c2.Width > 5 && c2.Height > 5)
            {
                List<ElementInformation> results = new List<ElementInformation>();
                results.Add(c1);
                GetAllChildren(c1, results);
                return results;
            }

            while (c1 != null)
            {
                List<ElementInformation> results = GetHidenControls(c1);
                if (results != null)
                    return results;
                c1 = c1.NextSibling;
            }
            return null;
        }
        public static bool HasHidenChildren(ElementInformation Parent)
        {
            ElementInformation c1 = Parent.FirstChild;
            if (c1 == null)
                return false;
            ElementInformation c2 = c1.NextSibling;
            if (c2 == null)
                return false;
            if (Math.Abs(c1.BoundingRectangle.X - c2.BoundingRectangle.X) < 5 && Math.Abs(c1.BoundingRectangle.Y - c2.BoundingRectangle.Y) < 5
                && c1.BoundingRectangle.Width > 5 && c1.BoundingRectangle.Height > 5 && c2.BoundingRectangle.Width > 5 && c2.BoundingRectangle.Height > 5)
                return true;
            return false;
        }
        public static void GetAllChildren(ElementInformation Parent, List<ElementInformation> list)
        {
            if (Parent.FirstChild == null)
                return;
            ElementInformation child = Parent.FirstChild;
            while (child != null)
            {
                list.Add(child);
                GetAllChildren(child, list);
                child = child.NextSibling;
            }
        }
        public static List<ControlType> leafTypes = new List<ControlType>() { ControlType.ComboBox };
        public static List<ControlType> UnCheckedManagedPropertyTypes = new List<ControlType>() { ControlType.TabItem, ControlType.Separator, ControlType.ScrollBar };
        public static void HasNativeControls(ElementInformation root, List<ElementInformation> native)
        {
            // NumericUpandDown controls do not get a cursor
            //if (IsNumericUpandDown(root)|| IsCommentUpAndDown(root))
            if (IsNumericUpandDown(root))
            {
                return;
            }

            if (!root.IsManagedControlProperty && !UnCheckedManagedPropertyTypes.Contains(root.ControlType))
            {
                native.Add(root);
            }
            
            //if (leafTypes.Contains(root.ControlType) || IsNumericUpandDown(root))            
            if (leafTypes.Contains(root.ControlType))
            {
                return;
            }
            else
            {
                foreach (ElementInformation ei in root.Children)
                {
                    HasNativeControls(ei, native);
                }
            }

        }
        public static bool IsNumericUpandDown(ElementInformation ei)
        {           
            if ((ei.ClassName.StartsWith("WindowsForms10.SysDateTimePick32.app", StringComparison.CurrentCultureIgnoreCase) && ei.ControlType == ControlType.Pane)
                || (ei.ClassName.StartsWith("WindowsForms10.Window.8.app", StringComparison.CurrentCultureIgnoreCase) && ei.ControlType == ControlType.Spinner && ei.Children != null && ei.Children.Count == 2 && ei.Children[0].ControlType == ControlType.Button && ei.Children[1].ControlType == ControlType.Button)
                ||(ei.ClassName== "msctls_updown32"&& ei.Children[0].ControlType==ControlType.Button && ei.Children[1].ControlType==ControlType.Button)                
                ) return true;
            return false;
        }
        public static bool IsCommentUpAndDown(ElementInformation ei)
        {
            //this is the content up and down
            //resultID = 37933209
            if (ei.Parent!=null&&ei.Parent.ClassName.StartsWith("WindowsForms10.EDIT.app")&&ei.ControlType==ControlType.ScrollBar) { 
            Console.WriteLine("ei.ClassName = "+ei.ClassName);
                return true;
            }
            return false;
        }
        public static bool IsIPAddressControl(ElementInformation ei)
        {
            List<ElementInformation> list1 = ei.Children.Where(c => c.ControlType == ControlType.Edit).ToList();
            List<ElementInformation> list2 = ei.Children.Where(c => c.ControlType == ControlType.Text).ToList();
            if (ei.Children != null && ei.Children.Count == 7 && ei.Children.Where(c => c.ControlType == ControlType.Edit).ToList().Count == 4
                && ei.Children.Where(c => c.ControlType == ControlType.Text).ToList().Count == 3 && ei.ControlType == ControlType.Pane && ei.IsManagedControlProperty)
            {
                return true;
            }
            return false;
        }
        public static bool IsChildOfText(ElementInformation ei)
        {
            if (ei.Parent != null && ei.Parent.ControlType == ControlType.Text)
            {
                return true;
            }
            return false;
        }
        public static bool IsClendar(ElementInformation ei)
        {
            if (ei.Parent != null && ei.Parent.Parent != null && ei.Ancestors.Any(c => c.ClassName == "SysMonthCal32"))
            {
                return true;
            }                            
            return false;
        }
        public static bool IsChildOfReportViewControl(ElementInformation ei)
        {
            if (ei.Ancestors.Any(c => c.AutomationId == "winRSviewer"))
            {
                return true;
            }
            return false;
        }
        public static bool IsChildOfWebBrowser(ElementInformation ei)
        {
            if (ei.Ancestors.Any(c => c.AutomationId == "webBrowser"))
            {
                return true;
            }
            return false;
        }
        public static bool IsDialogBox(ElementInformation ei)
        {
            return ei.ClassName == "#32770" ? true : false;
        }
        public static void NULLAssign(List<ElementInformation> list)
        {
            foreach (ElementInformation ei in list)
            {
                if (ei.Ancestors == null)
                {
                    ei.Ancestors = new List<ElementInformation>();
                }
                if (ei.Children == null)
                {
                    ei.Children = new List<ElementInformation>();
                }
                if (ei.Siblings == null)
                {
                    ei.Siblings = new List<ElementInformation>();
                }
            }
        }
        public static string TruncateControlFullName(string controlFullName)
        {
            if (string.IsNullOrEmpty(controlFullName))
                return controlFullName;
            if (controlFullName.Length < controlNameLength) return controlFullName;
            return controlFullName.Substring(0, controlNameLength) + "...";
        }
    }
}
