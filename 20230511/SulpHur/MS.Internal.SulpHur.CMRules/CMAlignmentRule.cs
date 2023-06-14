using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.UICompliance;

namespace MS.Internal.SulpHur.CMRules
{
    public enum VerticalAlignmentType
    {
        LeftAlignment,
        RightAlignment,
    }

    public class CMAlignmentRule : UIComplianceRuleBase
    {
        #region rule's Name & Description
        const string ruleName = "CM Alignment Rule";
        const string ruleDescrition = "The rule can verify the alignment problem in UI";

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

        #region rules' constant
        protected const float HorizontalHeightMultipleLimit = 2f;
        protected const int VerticalAlignmentLimit = 8;

        #endregion

        protected delegate List<UIComplianceResultBase> AlignmentChecker(List<ElementInformation> controls);
        protected List<AlignmentChecker> AlignmentCheckers = null;

        public CMAlignmentRule()
        {
            AlignmentCheckers = new List<AlignmentChecker>();
            AlignmentCheckers.Add(new AlignmentChecker(CheckVerticalAlignment));
            AlignmentCheckers.Add(new AlignmentChecker(CheckHorizontalAlignment));
        }

        public override List<UIComplianceResultBase> UIVerify(ICollection<ElementInformation> Controls)
        {
            // remove Parent from check list
            List<ElementInformation> FilteredControls = new List<ElementInformation>(Controls);
            //List<ElementInformation> FilteredControls = (List<ElementInformation>)Controls;
            ElementInformation ParentInfo = FilteredControls[0];
            FilteredControls.RemoveAt(0);

            // Filter controls out of list
            FilterControls(ref FilteredControls);


            // Run Checkers
            List<UIComplianceResultBase> results = new List<UIComplianceResultBase>();
            foreach (AlignmentChecker checker in AlignmentCheckers)
            {
                List<UIComplianceResultBase> tempResults = checker(FilteredControls);
                if (tempResults != null && tempResults.Count > 0)
                {
                    results.AddRange(tempResults);
                }
            }

            // If no fail, add a pass rule
            if (results.Count == 0)
            {
                UIComplianceResultBase passResult = new UIComplianceResultBase(ResultType.Pass, "There is no alignment problem!", ruleName);
                //passresult.TakePicture = true;
                results.Add(passResult);
            }
            return results;
        }

        public List<UIComplianceResultBase> CheckHorizontalAlignment(List<ElementInformation> controls)
        {
            List<UIComplianceResultBase> results = new List<UIComplianceResultBase>();

            List<List<ElementInformation>> HorizontalLinks = HorizontalControlsClassify(controls);

            // Check by links
            foreach (List<ElementInformation> list in HorizontalLinks)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    ElementInformation control = list[i];
                    for (int j = i + 1; j < list.Count; j++)
                    {
                        ElementInformation compareControl = list[j];
                        if ((control.ControlType == ControlType.DataGrid && compareControl.ControlType == ControlType.Button)
                            || (control.ControlType == ControlType.Button && compareControl.ControlType == ControlType.DataGrid))
                        {
                            goto NextLinkList;
                        }
                        if ((control.ControlType == ControlType.Edit &&
                            (compareControl.ControlType == ControlType.Text && compareControl.Parent.AutomationId != "_navPanel"))
                          || ((control.ControlType == ControlType.Text &&  control.Parent.AutomationId != "_navPanel")
                          && compareControl.ControlType == ControlType.Edit))
                        {
                            if (!IsMiddleAligned(control, compareControl))
                            {
                                // Not aligned. Create a new fail result
                                string message = "Horizontal alignment error,Label should be central-aligned with its corresponding input control";
                                UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Warning, message, Name);
                                foreach (ElementInformation relatedControl in list)
                                {
                                    result.AddRelatedControls(relatedControl);
                                }
                                //result.TakePicture = true;

                                results.Add(result);
                                goto NextLinkList;
                            }
                        }
                        if (!IsTopAligned(control, compareControl) && !IsMiddleAligned(control, compareControl)
                            && !IsBottomAligned(control, compareControl))
                        {
                            // Not aligned. Create a new fail result
                            string message = "Horizontal alignment error, These controls are not top-aligned, middle aligned or bottom aligned!";
                            UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Warning, message, Name);
                            foreach (ElementInformation relatedControl in list)
                            {
                                result.AddRelatedControls(relatedControl);
                            }
                            //result.TakePicture = true;

                            results.Add(result);
                            goto NextLinkList;
                        }
                    }
                }
            NextLinkList:
                ;
            }
            return results;

        }

        /// <summary>
        /// Classify controls into linked lists. Each Linked list contains one sort of controls.
        /// This method return the controls which should be compared with Horizontally.
        /// </summary>
        /// <param name="controls"></param>
        /// <returns></returns>
        public List<List<ElementInformation>> HorizontalControlsClassify(List<ElementInformation> controls)
        {
            List<List<ElementInformation>> controlsListContainer = new List<List<ElementInformation>>();

            // Classify controls by Parent firstly
            foreach (ElementInformation control in controls)
            {
                bool findList = false;
                foreach (List<ElementInformation> list in controlsListContainer)
                {
                    // defenitly not same category
                    if ((list == null) || (list.Count == 0) || (list[0].Parent.NativeWindowHandle != control.Parent.NativeWindowHandle))
                    {
                        continue;
                    }

                    // Compare Horizontal & height
                    System.Windows.Rect compareControlRect = list[0].BoundingRectangle;
                    System.Windows.Rect controlRect = control.BoundingRectangle;

                    // ignore the controls whose height is 2 time bigger than the one
                    //if ((compareControlRect.Height > (int)(controlRect.Height * VerticalHeightMultipleLimit)) ||
                    //    (controlRect.Height > (int)(compareControlRect.Height*VerticalHeightMultipleLimit)))
                    //{
                    //    findList = false;
                    //    break;
                    //}

                    // We add it to one category when there is any item in category meets the condition
                    foreach (ElementInformation tempControl in list)
                    {
                        compareControlRect = tempControl.BoundingRectangle;
                        // ignore the controls which contains each other
                        if (controlRect.IntersectsWith(compareControlRect))
                        {
                            continue;
                        }

                        if ((tempControl.ControlType == ControlType.Edit && (control.ControlType == ControlType.Text && control.Parent.AutomationId != "_navPanel"))
                            || (tempControl.ControlType == ControlType.Text && tempControl.Parent.AutomationId != "_navPanel") && control.ControlType == ControlType.Edit)
                        {

                            if (Math.Abs(compareControlRect.Bottom - controlRect.Bottom) < VerticalAlignmentLimit)
                            {
                                // The control find its category, Add to this list
                                list.Add(control);
                                findList = true;
                                goto NextControl;
                            }
                        }


                        if (Math.Abs(compareControlRect.Bottom - controlRect.Bottom) < VerticalAlignmentLimit)
                        {
                            // The control find its category, Add to this list
                            list.Add(control);
                            findList = true;
                            goto NextControl;
                        }
                    }
                }

                //Create a new list for the control which can't find same category
                if (!findList)
                {

                    List<ElementInformation> tempList = new List<ElementInformation>();
                    tempList.Add(control);
                    controlsListContainer.Add(tempList);
                }

            NextControl:
                ;
            }

            //(a): Special for "Minimize, Maximize, Close". They are fixed to "TitleBar".
            //     Delete "Form1" or "Item 1" from them.
            //(b): Special for "help" button.
            string[] specialAutomationId = { "Minimize", "Maximize", "Close" };
            string strHelp = "help";
            int count = 0, length = specialAutomationId.Length;
            List<List<ElementInformation>> controlsListAll = new List<List<ElementInformation>>();
            foreach (List<ElementInformation> list in controlsListContainer)
            {
                List<ElementInformation> special = new List<ElementInformation>();
                ElementInformation eiHelp = null;
                foreach (ElementInformation ei in list)
                {
                    count = 0;
                    for (int i = 0; i < length; i++)
                    {
                        if (ei.AutomationId == specialAutomationId[i] && ei.Parent.ControlType == ControlType.TitleBar)
                            count++;
                    }
                    if (count > 0)
                        special.Add(ei);
                    if (ei.AutomationId.ToLower().IndexOf(strHelp) > -1)
                        eiHelp = ei;
                }
                if (eiHelp != null)
                {
                    special.Remove(eiHelp);
                    list.Remove(eiHelp);
                }
                //A: Changed elements of list.
                //B: Not Changed elements of list.
                if (count > 0 && special != null)
                    controlsListAll.Add(special);
                else if (count < 1 && list != null)
                    controlsListAll.Add(list);
            }
            controlsListContainer = controlsListAll;

            return controlsListContainer;
        }

        /// <summary>
        /// Filter the controls should not pass to alignment check
        /// </summary>
        /// <param name="controls"></param>
        public void FilterControls(ref List<ElementInformation> controls)
        {
            for (int i = 0; i < controls.Count; )
            {
                // filter the control by size
                System.Windows.Rect controlRect = controls[i].BoundingRectangle;
                if (controlRect.Height < 3 || controlRect.Width < 3)
                {
                    controls.RemoveAt(i);
                    continue;
                }

                // filter the window by type
                ControlType winType = controls[i].ControlType;
                if ((winType == ControlType.Tab) ||
                    (winType == ControlType.TabItem)||(winType==ControlType.Pane))
                {
                    controls.RemoveAt(i);
                    continue;
                }

                i++;
            }
        }

        protected bool IsTopAligned(ElementInformation control1, ElementInformation control2)
        {
            return (control1.Top == control2.Top);
        }
        protected bool IsBottomAligned(ElementInformation control1, ElementInformation control2)
        {
            return (control1.Bottom == control2.Bottom);
        }
        protected bool IsMiddleAligned(ElementInformation control1, ElementInformation control2)
        {
            System.Windows.Rect rect1 = control1.BoundingRectangle;
            System.Windows.Rect rect2 = control2.BoundingRectangle;
            return ((Math.Abs(rect1.Bottom + rect1.Top - rect2.Top - rect2.Bottom)) / 2 < 2);
        }


        /// <summary>
        /// Check Vertical Alignment
        /// </summary>
        /// <param name="controls"></param>
        /// <returns></returns>
        public List<UIComplianceResultBase> CheckVerticalAlignment(List<ElementInformation> controls)
        {
            List<UIComplianceResultBase> results = new List<UIComplianceResultBase>();

            // Check the left Vertical alignment
            List<List<ElementInformation>> LeftVerticalLinks = VerticalControlsClassify(controls, VerticalAlignmentType.LeftAlignment);

            foreach (List<ElementInformation> list in LeftVerticalLinks)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    ElementInformation control = list[i];
                    for (int j = i + 1; j < list.Count; j++)
                    {
                        ElementInformation compareControl = list[j];
                        if (!IsLeftAligned(control, compareControl))
                        {
                            // ignore if controls have right alignment
                            if (IsRightAligned(control, compareControl))
                            {
                                continue;
                            }

                            // Not aligned. Create a new fail result
                            string message = "Vertical alignment error, These controls are not Left-aligned!";
                            UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Warning, message, Name);
                            foreach (ElementInformation relatedControl in list)
                            {
                                result.AddRelatedControls(relatedControl);
                            }
                            //result.TakePicture = true;

                            results.Add(result);
                            goto LeftAlignNextLinkList;
                        }
                    }
                }
            LeftAlignNextLinkList:
                ;
            }

            // Check the Right Vertical alignment
            List<List<ElementInformation>> RightVerticalLinks = VerticalControlsClassify(controls, VerticalAlignmentType.RightAlignment);

            foreach (List<ElementInformation> list in RightVerticalLinks)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    ElementInformation control = list[i];
                    for (int j = i + 1; j < list.Count; j++)
                    {
                        ElementInformation compareControl = list[j];
                        if (!IsRightAligned(control, compareControl))
                        {
                            // ignore the controls if they have left alignment
                            if (IsLeftAligned(control, compareControl))
                            {
                                continue;
                            }

                            // Not right aligned. Create a new fail result
                            string message = "Vertical alignment error, These controls are not Right-aligned!";
                            UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Warning, message, Name);
                            foreach (ElementInformation relatedControl in list)
                            {
                                result.AddRelatedControls(relatedControl);
                            }
                            //result.TakePicture = true;

                            results.Add(result);
                            goto RightAlignNextLinkList;
                        }
                    }
                }
            RightAlignNextLinkList:
                ;
            }

            return results;
        }

        public List<List<ElementInformation>> VerticalControlsClassify(List<ElementInformation> controls, VerticalAlignmentType VerticalAlign)
        {
            List<List<ElementInformation>> controlsListContainer = new List<List<ElementInformation>>();

            // Classify controls by Parent firstly
            foreach (ElementInformation control in controls)
            {
                bool findList = false;
                foreach (List<ElementInformation> list in controlsListContainer)
                {
                    // defenitly not same category, if they don't have same Parent
                    if ((list == null) || (list.Count == 0) || (list[0].Parent.NativeWindowHandle != control.Parent.NativeWindowHandle))
                    {
                        continue;
                    }

                    System.Windows.Rect compareControlRect = list[0].BoundingRectangle;
                    System.Windows.Rect controlRect = control.BoundingRectangle;

                    // ignore the controls whose height is 2 time bigger than the one
                    //if ((compareControlRect.Height > (int)(controlRect.Height * VerticalHeightMultipleLimit)) ||
                    //    (controlRect.Height > (int)(compareControlRect.Height*VerticalHeightMultipleLimit)))
                    //{
                    //    findList = false;
                    //    break;
                    //}

                    // We add it to one category when there is any item in category meets the condition
                    foreach (ElementInformation tempControl in list)
                    {
                        compareControlRect = tempControl.BoundingRectangle;
                        // ignore the controls which contains each other
                        if (controlRect.IntersectsWith(compareControlRect))
                        {
                            continue;
                        }

                        if (VerticalAlign == VerticalAlignmentType.LeftAlignment && (Math.Abs(compareControlRect.Left - controlRect.Left) <= VerticalAlignmentLimit))
                        {
                            // The control find its category, Add to this list
                            list.Add(control);
                            findList = true;
                            goto NextControl;
                        }
                        else if (VerticalAlign == VerticalAlignmentType.RightAlignment && (Math.Abs(compareControlRect.Right - controlRect.Right) <= VerticalAlignmentLimit))
                        {
                            list.Add(control);
                            findList = true;
                            goto NextControl;
                        }
                    }
                }

                //Create a new list for the control which can't find same category
                if (!findList)
                {
                    List<ElementInformation> tempList = new List<ElementInformation>();
                    tempList.Add(control);
                    controlsListContainer.Add(tempList);
                }

            NextControl:
                ;
            }
            return controlsListContainer;
        }

        protected bool IsLeftAligned(ElementInformation control1, ElementInformation control2)
        {
            return (control1.Left == control2.Left);
        }

        protected bool IsRightAligned(ElementInformation control1, ElementInformation control2)
        {
            return (control1.Right == control2.Right);
        }

        public override List<UIComplianceResultBase> UIVerify(ICollection<WebElementInfo> webElements)
        {
            throw new NotImplementedException();
        }
    }
}
