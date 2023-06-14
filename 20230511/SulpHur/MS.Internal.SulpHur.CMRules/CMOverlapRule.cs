using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.UICompliance;

namespace MS.Internal.SulpHur.CMRules
{
    public class OverlapRule : UIComplianceRuleBase
    {
        const string ruleName = "Overlap Rule";
        const string ruleDescrition = "The rule verify whether the control has overlap";

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

        private List<ControlType> filteredTypes = new List<ControlType>() {ControlType.ToolBar,ControlType.Group,ControlType.MenuItem,ControlType.Thumb,
            ControlType.Pane, ControlType.TabItem, ControlType.Window, ControlType.ToolTip, ControlType.TreeItem, ControlType.Menu, ControlType.Tab };

        public OverlapRule()
        {
        }

        /// <summary>
        /// Rule entry point
        /// </summary>
        /// <param name="Controls"></param>
        /// <returns></returns>
        public override List<UIComplianceResultBase> UIVerify(ICollection<ElementInformation> Controls)
        {
            List<UIComplianceResultBase> results = new List<UIComplianceResultBase>();
            ElementInformation Parent = CommonRuleUtility.GetRootElement(Controls);
            if (CommonRuleUtility.IsDialogBox(Parent)) {
                UIComplianceResultBase passResult = new UIComplianceResultBase(ResultType.Pass, "No Overlap controls found in window : " + Parent.Name, ruleName);
                results.Add(passResult);
                return results;
            }
            // Filter controls
            List<ElementInformation> CheckControls = ControlsFilter(Controls);
            CheckControls.Remove(Parent);

            return CheckControlOverlap(Parent, CheckControls);
        }

       

        //v-edy: bug 485731:-result ID 5078372,Pix3 would miss some overlop issue, so no pix fixed
        #region bug 485731
        private bool Pix0IntersectsWith(System.Windows.Rect A, System.Windows.Rect B)
        {
            System.Drawing.Rectangle temp1 = Scal0Pix(A);
            System.Drawing.Rectangle temp2 = Scal0Pix(B);
            return temp1.IntersectsWith(temp2);
        }

        private System.Drawing.Rectangle Scal0Pix(System.Windows.Rect rect)
        {
            return new System.Drawing.Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
        }
        #endregion

        private bool Pix3IntersectsWith(System.Windows.Rect A, System.Windows.Rect B)
        {
            System.Drawing.Rectangle temp1 = Scal3Pix(A);
            System.Drawing.Rectangle temp2 = Scal3Pix(B);
            return temp1.IntersectsWith(temp2);
        }

        private System.Drawing.Rectangle Scal3Pix(System.Windows.Rect rect)
        {
            if (rect.Width > 6 && rect.Height > 6)
            {
                return new System.Drawing.Rectangle((int)rect.X + 3, (int)rect.Y + 3, (int)rect.Width - 6, (int)rect.Height - 6);
            }
            else
            {
                return new System.Drawing.Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
            }
        }

        /// <summary>
        /// Filter the controls needed to checked
        /// </summary>
        /// <param name="controls"></param>
        /// <returns></returns>
        public List<ElementInformation> ControlsFilter(ICollection<ElementInformation> controls)
        {
            List<ElementInformation> ControlsOverlapRule = new List<ElementInformation>();
            foreach (ElementInformation control in controls)
            {
                if (CommonRuleUtility.IsClendar(control)||
                    CommonRuleUtility.IsChildOfReportViewControl(control)||
                    CommonRuleUtility.IsChildOfWebBrowser(control)||
                    CommonRuleUtility.IsChildOfText(control)||
                    control.Ancestors.Any(c => c.ControlType == ControlType.ComboBox)||
                    control.ControlType == ControlType.ScrollBar)
                {
                    continue;
                }

                //If comboBox is expand, it's size should be collpllsed size
                if (control.ControlType == ControlType.ComboBox && control.Height > 21 && control.FirstChild != null && control.Height > control.FirstChild.Height)
                {
                    control.BoundingRectangle = new System.Windows.Rect(control.X, control.Y, control.Width, control.Height - control.FirstChild.Height);
                }

                if (!control.IsOffscreen && control.Parent != null && !filteredTypes.Contains(control.ControlType))
                {
                    if (!IsOKCancelButton(control) || !(control.Parent != null && control.Parent.ControlType == ControlType.ComboBox))
                    {
                        ControlsOverlapRule.Add(control);
                    }
                }
            }
            return ControlsOverlapRule;
        }

        /// <summary>
        /// Check overlap controls
        /// </summary>
        /// <param name="control"></param>
        /// <param name="OverlapControls"></param>
        private List<UIComplianceResultBase> CheckControlOverlap(ElementInformation Parent, List<ElementInformation> list)
        {
            List<UIComplianceResultBase> results = new List<UIComplianceResultBase>();

            // enumerate all controls
            ElementInformation[] controls = list.ToArray();

            for (int i = 0; i < list.Count; i++)
            {
                ElementInformation control = controls[i];

                ElementInformation ParentControl = control.Parent;
                string controlName = CommonRuleUtility.TruncateControlFullName(control.Name);
                         
                if (ParentControl != null && !ParentControl.BoundingRectangle.Contains(control.BoundingRectangle) && !filteredTypes.Contains(ParentControl.ControlType))
                {
                    string parentControlName = CommonRuleUtility.TruncateControlFullName(ParentControl.Name);
                    #region false alarm                                        
                    if (control.ControlType == ControlType.Edit)
                    {
                        if (control.BoundingRectangle.Height < 25 && control.NextSibling != null && control.NextSibling.ControlType == ControlType.Button)
                        {
                            continue;
                        }
                    }
                    #endregion
                    StringBuilder sb = new StringBuilder();                    
                    sb.AppendLine(string.Format("[Name:{0}, ControlType:{1}, Location:{2}] and it's parent [Name:{0}, ControlType:{1}, Location:{2}] are overlap",
                        controlName,control.ControlType,control.BoundingRectangle,parentControlName,ParentControl.ControlType,ParentControl.BoundingRectangle));
                    UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Fail, sb.ToString(), this.Name);
                    //result.TakePicture = true;
                    result.AddRelatedControls(control);
                    result.AddRelatedControls(ParentControl);
                    results.Add(result);
                }


                for (int j = i + 1; j < list.Count; j++)
                {
                    ElementInformation comparedControl = controls[j];

                    //if (comparedControl.BoundingRectangle.Contains(control.BoundingRectangle) || control.BoundingRectangle.Contains(comparedControl.BoundingRectangle))
                    //{
                    //    continue;
                    //}
                    //v-edy: bug 485731:-result ID 5078372,Pix3 would miss some overlop issue, so no pix fixed
                    if (Pix0IntersectsWith(comparedControl.BoundingRectangle, control.BoundingRectangle))
                    {
                        string comparedControlName = CommonRuleUtility.TruncateControlFullName(comparedControl.Name);
                        //if (!IsHidedbyParent(control, comparedControl))
                        //{
                        #region false alarm

                        //add a function for overlaprule,Filter overlap when the parent control is different
                        //bug 3672254
                        bool IsHas = IsHasSameFirstParentControl(control, comparedControl);
                        if (!IsHas) { continue; }

                        if ((control.ControlType == ControlType.DataGrid && comparedControl.ControlType == ControlType.Edit)
                            || (control.ControlType == ControlType.Edit && comparedControl.ControlType == ControlType.DataGrid))
                        {
                            if (control.ControlType == ControlType.Edit)
                            {
                                if (control.Height < 25 &&control.NextSibling!=null&& control.NextSibling.ControlType == ControlType.Button)
                                    continue;
                            }
                            else
                            {
                                if (comparedControl.Height < 25 && comparedControl.NextSibling != null && comparedControl.NextSibling.ControlType == ControlType.Button)
                                    continue;
                            }
                            continue;
                        }

                        if (control.Parent != comparedControl.Parent)
                        {
                            if (IsInContainerHasScrollBar(control) || IsInContainerHasScrollBar(comparedControl))
                            {
                                continue;
                            }
                        }


                        if (control.Ancestors.Contains(comparedControl) || comparedControl.Ancestors.Contains(control))
                        {
                            continue;
                        }

                        //WPF ListView
                        if (control.ControlType == ControlType.Edit && (control.Siblings.Any(c => c.ControlType == ControlType.DataGrid) || control.Siblings.Any(c => c.ControlType == ControlType.Tree))
                            && control.Siblings.Any(c => c.ControlType == ControlType.Button)
                                && control.BoundingRectangle.Contains(comparedControl.BoundingRectangle))
                        {
                            continue;
                        }

                        //workaround for hide label
                        if ((control.AutomationId == "labelUser" && control.Ancestors.Any(c => c.AutomationId == "ClientAgentSettingsPageControl"))
                            || (comparedControl.AutomationId == "labelUser" && comparedControl.Ancestors.Any(c => c.AutomationId == "ClientAgentSettingsPageControl")))
                        {
                            continue;
                        }
                        #endregion

                        // Overlapped, Create a failure result and add to the result list
                        StringBuilder sb = new StringBuilder();
                        if (control.Name != null && comparedControl.Name != null)
                        { 
                            sb.Append(string.Format("[Name:{0}, ControlType:{1}, Rectangle:{2}] and [Name:{3}, ControlType:{4}, Rectangle:{5}] are overlappped."
                                , controlName, control.ControlType, control.BoundingRectangle, comparedControlName, comparedControl.ControlType, comparedControl.BoundingRectangle));
                        }
                        else
                        {
                            sb.Append(string.Format("[Name:{0}, ControlType:{1}, Rectangle:{2}] and [Name:{3}, ControlType:{4}, Rectangle:{5}] are overlappped."
                                , string.Empty, control.ControlType, control.BoundingRectangle, comparedControl.Name, comparedControl.ControlType, comparedControl.BoundingRectangle));
                        }
                        UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Fail, sb.ToString(), this.Name);
                        result.AddRelatedControls(control);
                        result.AddRelatedControls(comparedControl);
                        results.Add(result);

                        //}

                    }
                }
            }

            if (results.Count == 0)
            {
                UIComplianceResultBase passResult = new UIComplianceResultBase(ResultType.Pass, "No Overlap controls found in window : " + Parent.Name, ruleName);
                results.Add(passResult);
            }

            return results;
        }

        //All parent controls are judged and are not currently in use
        private bool IsHasSameParentControl(ElementInformation control, ElementInformation comparedControl, ElementInformation parent)
        {

            bool result = false;
            List<ElementInformation> List1 = new List<ElementInformation>();
            List<ElementInformation> List2 = new List<ElementInformation>();
            ElementInformation tempCon1 = control;
            ElementInformation tempCon2 = comparedControl;

            //controls.Remove(parent);

            while (tempCon1.Parent != null && tempCon1.Parent != parent)
            {
                List1.Add(tempCon1.Parent);
                tempCon1 = tempCon1.Parent;
            }
            while (tempCon2.Parent != null && tempCon2.Parent != parent)
            {
                List2.Add(tempCon2.Parent);
                tempCon2 = tempCon2.Parent;
            }

            foreach (ElementInformation item in List1)
            {
                if (List2.Contains(item))
                {
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Filter overlap when the parent control is different
        /// </summary>
        /// <param name="control">One of the two controls being compared</param>
        /// <param name="comparedControl">the other of the two controls being compared</param>
        /// <returns></returns>
        private bool IsHasSameFirstParentControl(ElementInformation control, ElementInformation comparedControl)
        {
            bool result = false;
            if (control.Parent!=null&&comparedControl.Parent!=null)
            {
                if (control.Parent.Equals(comparedControl.Parent))
                {
                    result = true;
                }
            }
            return result;
        }

        private bool IsInContainerHasScrollBar(ElementInformation ei)
        {
            if (ei.Parent.Children.Any(c => c.ControlType == ControlType.ScrollBar)) return true;
            return false;
        }
        private static bool IsNavigationText(ElementInformation ei)
        {
            bool find = false;
            if (ei.Ancestors.Any(c => c.AutomationId == "_navPanel"))
            {
                ElementInformation navPanel = ei.Ancestors.Single(c => c.AutomationId == "_navPanel");
                if (navPanel.Children.Any(c => c.ControlType == ControlType.ScrollBar))
                {
                    foreach (ElementInformation eS in navPanel.Children.Where(c => c.ControlType == ControlType.ScrollBar))
                    {
                        if (eS.IsEnabled)
                        {
                            find = true;
                        }
                    }

                }
            }
            return find;
        }
        public static bool IsHidedbyParent(ElementInformation c1, ElementInformation c2)
        {
            List<ElementInformation> Parents = new List<ElementInformation>();
            ElementInformation p1 = c1.Parent;
            while (p1 != null)
            {
                Parents.Add(p1);
                p1 = p1.Parent;
            }
            Parents.Reverse();

            List<ElementInformation> Parents2 = new List<ElementInformation>();
            ElementInformation p2 = c2.Parent;
            while (p2 != null)
            {
                Parents2.Add(p2);
                p2 = p2.Parent;
            }
            Parents2.Reverse();

            int i = 0;
            while (Parents[i] == Parents2[i])
            {
                i++;
                if (i >= Parents.Count || i >= Parents2.Count)
                    break;
            }
            ElementInformation share = Parents2[i - 1];
            ElementInformation pi = (i < Parents.Count) ? Parents[i] : c1;
            ElementInformation pi2 = (i < Parents2.Count) ? Parents2[i] : c2;
            if (Math.Abs(pi.X - pi2.X) < 5 && Math.Abs(pi.Y - pi2.Y) < 5)
                return true;
            return false;
        }
        public static bool CheckHasHideChildren(ElementInformation Parent)
        {
            ElementInformation c1 = Parent.FirstChild;
            if (c1 == null)
                return false;
            ElementInformation c2 = c1.NextSibling;
            if (c2 == null)
                return false;
            if (Math.Abs(c1.X - c2.X) < 5 && Math.Abs(c1.Y - c2.Y) < 5)
                return true;
            return false;

        }
        public static List<ElementInformation> GetHidenControls(ElementInformation Parent)
        {
            ElementInformation c1 = Parent.FirstChild;
            if (c1 == null)
                return null;
            ElementInformation c2 = c1.NextSibling;
            if (c2 == null)
                return null;
            if (Math.Abs(c1.X - c2.X) < 5 && Math.Abs(c1.Y - c2.Y) < 5)
            {
                List<ElementInformation> results = new List<ElementInformation>();
                while (c1 != null)
                {
                    results.Add(c1);
                    c1 = c1.NextSibling;
                }
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
        public static bool IsOKCancelButton(ElementInformation c1)
        {
            if (c1.ControlType != ControlType.Button)
                return false;
            if (string.IsNullOrEmpty(c1.Name))
            {
                return false;
            }
            if (c1.Name.ToLower() == "ok")
                return true;
            if (c1.Name.ToLower() == "cancel")
                return true;
            if (c1.Name.ToLower() == "apply")
                return true;
            if (c1.Name.ToLower() == "help")
                return true;
            return false;
        }
        public static string PrintHierachy(ICollection<ElementInformation> Controls)
        {
            ElementInformation win1 = null;
            foreach (ElementInformation w1 in Controls)
            {
                win1 = w1;
                break;
            }
            StringBuilder sb = new StringBuilder();
            PrintHierachy(win1, sb, 0);
            return sb.ToString();

        }
        public static void PrintHierachy(ElementInformation control, StringBuilder sb, int level)
        {
            for (int i = 0; i < level; i++)
            {
                sb.Append("   ");
            }
            sb.AppendLine(string.Format("{0}  {5}  [{1}, {2}] [{3}, {4}]", control.ControlType, control.BoundingRectangle.X, control.BoundingRectangle.Y
                , control.Width, control.Height, control.Name));
            ElementInformation child = control.FirstChild;
            while (child != null)
            {
                PrintHierachy(child, sb, level + 1);
                child = child.NextSibling;
            }

        }

        //private static void CheckControlOverlap(Window control, Dictionary<int,List<Maui.Core.Window>> OverlapControls)
        //{
        //    // ignore the invisible controls
        //    if(!control.Extended.IsOffscreen)
        //        return;
        //    Window Parent = control.Extended.Parent;

        //    // Controls should have Parent
        //    if(Parent == null)
        //        return;

        //    Rectangle controlRect = new Rectangle(control.Extended.Left,control.Extended.Top,
        //                                    control.Extended.Width,control.Extended.Height);

        //    // Check the overlap with Parent
        //    if (controlRect.Top < Parent.Extended.Top || controlRect.Left < Parent.Extended.Left ||
        //        ((controlRect.Bottom) > Parent.Extended.Bottom) ||
        //        ((controlRect.Right) > Parent.Extended.Right))
        //    {
        //        List<Maui.Core.Window> tempList = new List<Maui.Core.Window>();
        //        tempList.Add(control);
        //        tempList.Add(control.Extended.Parent);
        //        int hashCode = control.GetHashCode() + Parent.GetHashCode();
        //        OverlapControls.Add(hashCode, tempList);
        //    }

        //    // Check overlap with slibing
        //    Window slibing = control.Extended.NextSibling;
        //    for(;slibing!=null;slibing = slibing.Extended.NextSibling)
        //    {
        //        // ignore the invisible controls
        //        if (!slibing.Extended.IsOffscreen)
        //            continue;

        //        Rectangle rect = new Rectangle(slibing.Extended.Left, slibing.Extended.Top,
        //                                    slibing.Extended.Width, slibing.Extended.Height);
        //        if (controlRect.IntersectsWith(rect))
        //        {
        //            int hashCode = control.GetHashCode() + slibing.GetHashCode();
        //            if (!OverlapControls.ContainsKey(hashCode))
        //            {
        //                //Parent.Extended.ha
        //                List<Maui.Core.Window> tempList = new List<Maui.Core.Window>();
        //                tempList.Add(control);
        //                tempList.Add(slibing);
        //                OverlapControls.Add(hashCode, tempList);
        //            }
        //        }
        //    }

        //}


        public override List<UIComplianceResultBase> UIVerify(ICollection<WebElementInfo> webElements)
        {
            throw new NotImplementedException();
        }
    }
}
