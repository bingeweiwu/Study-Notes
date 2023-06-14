using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.UICompliance;

namespace MS.Internal.SulpHur.CMRules
{
    public enum HorizontalAlignmentType
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
        protected const float VerticalHeightMultipleLimit = 2f;
        protected const int HorizontalAlignmentLimit = 8;

        #endregion

        protected delegate List<UIComplianceResultBase> AlignmentChecker(List<ElementInformation> controls);
        protected List<AlignmentChecker> AlignmentCheckers = null;

        public CMAlignmentRule()
        {
            AlignmentCheckers = new List<AlignmentChecker>();
            AlignmentCheckers.Add(new AlignmentChecker(CheckHorizontalAlignment));
            AlignmentCheckers.Add(new AlignmentChecker(CheckVerticalAlignment));
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

        public List<UIComplianceResultBase> CheckVerticalAlignment(List<ElementInformation> controls)
        {
            List<UIComplianceResultBase> results = new List<UIComplianceResultBase>();

            List<List<ElementInformation>> VerticalLinks = VerticalControlsClassify(controls);

            // Check by links
            foreach (List<ElementInformation> list in VerticalLinks)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].Name.StartsWith("Sync every"))
                    {
                        int test = 1;
                    }
                    ElementInformation control = list[i];
                    for (int j = i + 1; j < list.Count; j++)
                    {
                        ElementInformation compareControl = list[j];
                        if (!IsTopAligned(control, compareControl) && !IsMiddleAligned(control, compareControl)
                            && !IsBottomAligned(control, compareControl))
                        {
                            // Not aligned. Create a new fail result
                            string message = "Vertical alignment error, These controls are not top-aligned, middle aligned or bottom aligned!";
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
        /// This method return the controls which should be compared with Vertically.
        /// </summary>
        /// <param name="controls"></param>
        /// <returns></returns>
        public List<List<ElementInformation>> VerticalControlsClassify(List<ElementInformation> controls)
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

                    // Compare Vertical & height
                    System.Windows.Rect compareControlRect = list[0].BoundingRectangle;
                    System.Windows.Rect controlRect = control.BoundingRectangle;

                    // ignore the controls whose height is 2 time bigger than the one
                    //if ((compareControlRect.Height > (int)(controlRect.Height * HorizontalHeightMultipleLimit)) ||
                    //    (controlRect.Height > (int)(compareControlRect.Height*HorizontalHeightMultipleLimit)))
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

                        if ((compareControlRect.Top < controlRect.Bottom) && (compareControlRect.Bottom > controlRect.Top))
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

            //Special for "Minimize, Maximize, Close". 
            //Delete "Form1" or "Item 1" from them.
            string[] specialAutomationId = { "Minimize", "Maximize", "Close" };
            int count = 0, length = specialAutomationId.Length;
            List<List<ElementInformation>> controlsListAll = new List<List<ElementInformation>>();
            foreach (List<ElementInformation> list in controlsListContainer)
            {
                List<ElementInformation> special = new List<ElementInformation>();
                foreach (ElementInformation ei in list)
                {
                    count = 0;
                    for (int i = 0; i < length; i++)
                        if (ei.AutomationId == specialAutomationId[i])
                            count++;
                    if (count > 0)
                        special.Add(ei);
                }
                //A: Changed elements of list.
                //B: Not Changed elements of list.
                if (count > 0 && special != null)
                    controlsListAll.Add(special);
                else if (count < 1)
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
                    (winType == ControlType.Unknown))
                {
                    controls.RemoveAt(i);
                    continue;
                }

                i++;
            }
        }

        protected bool IsTopAligned(ElementInformation control1, ElementInformation control2)
        {
            return (control1.BoundingRectangle.Top == control2.BoundingRectangle.Top);
        }
        protected bool IsBottomAligned(ElementInformation control1, ElementInformation control2)
        {
            return (control1.BoundingRectangle.Bottom == control2.BoundingRectangle.Bottom);
        }
        protected bool IsMiddleAligned(ElementInformation control1, ElementInformation control2)
        {
            System.Windows.Rect rect1 = control1.BoundingRectangle;
            System.Windows.Rect rect2 = control2.BoundingRectangle;
            return ((Math.Abs(rect1.Bottom + rect1.Top - rect2.Top - rect2.Bottom)) / 2 < 2);
        }


        /// <summary>
        /// Check Horizontal Alignment
        /// </summary>
        /// <param name="controls"></param>
        /// <returns></returns>
        public List<UIComplianceResultBase> CheckHorizontalAlignment(List<ElementInformation> controls)
        {
            List<UIComplianceResultBase> results = new List<UIComplianceResultBase>();

            // Check the left horizontal alignment
            List<List<ElementInformation>> LeftHorizontalLinks = HorizontalControlsClassify(controls, HorizontalAlignmentType.LeftAlignment);

            foreach (List<ElementInformation> list in LeftHorizontalLinks)
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
                            string message = "Horizontal alignment error, These controls are not Left-aligned!";
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

            // Check the Right horizontal alignment
            List<List<ElementInformation>> RightHorizontalLinks = HorizontalControlsClassify(controls, HorizontalAlignmentType.RightAlignment);

            foreach (List<ElementInformation> list in RightHorizontalLinks)
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
                            string message = "Horizontal alignment error, These controls are not Right-aligned!";
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

        public List<List<ElementInformation>> HorizontalControlsClassify(List<ElementInformation> controls, HorizontalAlignmentType horizontalAlign)
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
                    //if ((compareControlRect.Height > (int)(controlRect.Height * HorizontalHeightMultipleLimit)) ||
                    //    (controlRect.Height > (int)(compareControlRect.Height*HorizontalHeightMultipleLimit)))
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

                        if (horizontalAlign == HorizontalAlignmentType.LeftAlignment && (Math.Abs(compareControlRect.Left - controlRect.Left) <= HorizontalAlignmentLimit))
                        {
                            // The control find its category, Add to this list
                            list.Add(control);
                            findList = true;
                            goto NextControl;
                        }
                        else if (horizontalAlign == HorizontalAlignmentType.RightAlignment && (Math.Abs(compareControlRect.Right - controlRect.Right) <= HorizontalAlignmentLimit))
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
            return (control1.BoundingRectangle.Left == control2.BoundingRectangle.Left);
        }

        protected bool IsRightAligned(ElementInformation control1, ElementInformation control2)
        {
            return (control1.BoundingRectangle.Right == control2.BoundingRectangle.Right);
        }

        public override List<UIComplianceResultBase> UIVerify(ICollection<WebElementInfo> webElements)
        {
            throw new NotImplementedException();
        }
    }
}
