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

        /// <summary>
        /// Rule entry point
        /// </summary>
        /// <param name="Controls"></param>
        /// <returns></returns>
        public override List<UIComplianceResultBase> UIVerify(ICollection<ElementInformation> Controls)
        {
            //PrintHierachy(Controls);
            //List<UIComplianceResultBase> rs = new List<UIComplianceResultBase>();
            //UIComplianceResultBase passResult = new UIComplianceResultBase(ResultType.Fail, "No Overlap controls found in window", this.Name);
            //foreach (ElementInformation wInfo in Controls)
            //{
            //    passResult.AddRelatedControls(wInfo);
            //}
            //pass//result.TakePicture = true;
            //rs.Add(passResult);
            //return rs;

            List<UIComplianceResultBase> results = new List<UIComplianceResultBase>();
            //List<ElementInformation> myWindows = new List<ElementInformation>(Windows);

            // Get Parent Window
            IEnumerator<ElementInformation> enumerator = Controls.GetEnumerator();
            enumerator.MoveNext();
            ElementInformation Parent = enumerator.Current;

            // Filter controls
            List<ElementInformation> CheckControls = ControlsFilter(Controls);
            CheckControls.Remove(Parent);

            return CheckControlOverlap(Parent, CheckControls);
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
                if (control.IsOffscreen && control.Parent != null)
                {
                    if (!IsOKCancelButton(control))
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
                if (ParentControl != null && !ParentControl.BoundingRectangle.Contains(control.BoundingRectangle))
                {
                    #region false alarm
                    if (control.ControlType == ControlType.Edit)
                    {
                        if (control.BoundingRectangle.Height < 25 && control.NextSibling.ControlType == ControlType.Button)
                        {
                            continue;
                        }
                    }
                    #endregion
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Control : " + control.Name + " and its Parent are overlapped");

                    UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Fail, sb.ToString(), this.Name);
                    //result.TakePicture = true;
                    result.AddRelatedControls(control);
                    result.AddRelatedControls(ParentControl);
                    results.Add(result);
                }


                for (int j = i + 1; j < list.Count; j++)
                {
                    ElementInformation comparedControl = controls[j];

                    // Compare
                    if (comparedControl.BoundingRectangle.Contains(control.BoundingRectangle) || control.BoundingRectangle.Contains(comparedControl.BoundingRectangle))
                    {
                        continue;
                    }

                    if (comparedControl.BoundingRectangle.IntersectsWith(control.BoundingRectangle))
                    {
                        if (!IsHidedbyParent(control, comparedControl))
                        {
                            // Overlapped, Create a failure result and add to the result list
                            StringBuilder sb = new StringBuilder();
                            if (control.Name != null && control.Name.Length > 0 && comparedControl.Name != null && comparedControl.Name.Length > 0)
                            {
                                sb.AppendLine("Control : " + control.Name + " and Control:" + comparedControl.Name + " are overlapped");
                            }
                            else
                            {
                                sb.AppendLine("'" + control.ControlType + "' and '" + comparedControl.ControlType + "' are overlapped");
                            }

                            #region false alarm
                            if ((control.ControlType == ControlType.DataGrid && comparedControl.ControlType == ControlType.Edit)
                                || (control.ControlType == ControlType.Edit && comparedControl.ControlType == ControlType.DataGrid))
                            {
                                if (control.ControlType == ControlType.Edit)
                                {
                                    if (control.BoundingRectangle.Height < 25 && control.NextSibling.ControlType == ControlType.Button)
                                        continue;
                                }
                                else
                                {
                                    if (comparedControl.BoundingRectangle.Height < 25 && comparedControl.NextSibling.ControlType == ControlType.Button)
                                        continue;
                                }
                                continue;
                            }
                            #endregion

                            UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Fail, sb.ToString(), this.Name);
                            //result.TakePicture = true;
                            result.AddRelatedControls(control);
                            result.AddRelatedControls(comparedControl);
                            results.Add(result);

                        }

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
            if (Math.Abs(pi.BoundingRectangle.X - pi2.BoundingRectangle.X) < 5 && Math.Abs(pi.BoundingRectangle.Y - pi2.BoundingRectangle.Y) < 5)
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
            if (Math.Abs(c1.BoundingRectangle.X - c2.BoundingRectangle.X) < 5 && Math.Abs(c1.BoundingRectangle.Y - c2.BoundingRectangle.Y) < 5)
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
            if (Math.Abs(c1.BoundingRectangle.X - c2.BoundingRectangle.X) < 5 && Math.Abs(c1.BoundingRectangle.Y - c2.BoundingRectangle.Y) < 5)
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
                , control.BoundingRectangle.Width, control.BoundingRectangle.Height, control.Name));
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
