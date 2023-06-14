using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.UICompliance;
using System.Drawing;

namespace MS.Internal.SulpHur.CMRules
{
    public class TabOrderRule : UIComplianceRuleBase
    {
        public override string Name
        {
            get { return @"Tab Order Rule"; }
        }

        public override string Description
        {
            get { return @"This rule checks the tab order of the controls on the UI. It follows the rule that tab index should be incremental from top to bottom, from left to right."; }
        }

        public TabOrderRule()
        {
            ruleProperties = new TabOrderRuleProperties();
        }

        public override List<UIComplianceResultBase> UIVerify(ICollection<ElementInformation> controls)
        {
            List<UIComplianceResultBase> results = new List<UIComplianceResultBase>();

            try
            {
                if (controls == null || controls.Count == 0)
                {
                    results.Add(new UIComplianceResultBase(ResultType.Info, "There are no controls to check.", Name));
                    return results;
                }

                IEnumerator<ElementInformation> enumerator = controls.GetEnumerator();
                enumerator.Reset();
                enumerator.MoveNext();
                ElementInformation rootControl = enumerator.Current;
                while (rootControl.Parent != null)
                    rootControl = rootControl.Parent;

                List<ElementInformation> hidens = TabOrderRuleUtilities.GetHidenControls(rootControl);
                if (hidens != null)
                {
                    StringBuilder sbMessage = new StringBuilder();
                    sbMessage.AppendLine("The dialog or pagecontrol contains hide controls:");
                    UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Warning, sbMessage.ToString(), Name);
                    foreach (ElementInformation child in hidens)
                    {
                        result.AddRelatedControls(child);
                    }
                    results.Add(result);
                }

                bool hasNativeControls = false;
                List<ManagedElementInformation> controlsInActualTabOrder = new List<ManagedElementInformation>();
                TabOrderRuleUtilities.GetLeafControlsInActualTabOrder(rootControl, controlsInActualTabOrder, ruleProperties.TabStopOnly, ruleProperties.IncludeLabels, ref hasNativeControls);
                if (TabOrderRuleUtilities.RemoveHiddenControls(controlsInActualTabOrder))
                {
                    results.Add(new UIComplianceResultBase(ResultType.Warning, "The window contains hidden control(s). The tab orders for those controls are not checked.", Name));
                }
                if (controlsInActualTabOrder.Count == 0)
                {
                    results.Add(new UIComplianceResultBase(ResultType.Warning, "Tab order cannot be checked because:\n1. The window contains native controls only, or;\n2. No controls has tab stop.", Name));
                    return results;
                }
                else if (hasNativeControls)
                {
                    results.Add(new UIComplianceResultBase(ResultType.Warning, "The window contains native control(s). The tab orders for those controls are not checked.", Name));
                }

                List<ManagedElementInformation> controlsInExpectedTabOrder = new List<ManagedElementInformation>(controlsInActualTabOrder);
                if (ruleProperties.IgnoreContainer)
                    controlsInExpectedTabOrder.Sort(TabOrderRuleUtilities.PositionComparer.IgnoreContainerComparer);
                else
                    controlsInExpectedTabOrder.Sort(TabOrderRuleUtilities.PositionComparer.DefaultComparer);
                TabOrderRuleUtilities.SpecialReorderExpectedTabOrder(controlsInExpectedTabOrder);

                if (ruleProperties.ShowTabOrder)
                {
                    UIComplianceResultBase expectedTabOrderInfo = new UIComplianceResultBase(ResultType.Info, string.Empty, Name);
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("The current tab order is..: ");
                    foreach (ManagedElementInformation control in controlsInActualTabOrder)
                        sb.AppendLine(control.Name + " \"" + control.Name + "\" (" + control.ControlType.ToString() + "), ");
                    sb.Remove(sb.Length - 4, 4);
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("The expected tab order is: ");
                    foreach (ManagedElementInformation control in controlsInExpectedTabOrder)
                    {
                        sb.AppendLine(control.Name + " \"" + control.Name + "\"  (" + control.ControlType.ToString() + "), ");
                        expectedTabOrderInfo.AddRelatedControls(control);
                    }
                    sb.Remove(sb.Length - 4, 4);
                    expectedTabOrderInfo.Message = sb.ToString();
                    //expectedTabOrderInfo.TakePicture = true;
                    results.Add(expectedTabOrderInfo);
                }

                if (ruleProperties.IncludeLabels && ruleProperties.ShowControlLabel)
                {
                    List<ManagedElementInformation> controlsWithoutLabel = new List<ManagedElementInformation>();
                    foreach (ManagedElementInformation control in controlsInActualTabOrder)
                    {
                        if (TabOrderRuleUtilities.NeedsExternalLabel(control))
                        {
                            int index = controlsInActualTabOrder.IndexOf(control);
                            if (index == 0 ||
                                controlsInActualTabOrder[index - 1].ControlType != ControlType.Text ||
                                (ruleProperties.LabelMustHasAccessKey && !TabOrderRuleUtilities.IsReadOnly(control) && !controlsInActualTabOrder[index - 1].Name.Contains("&")))
                            {
                                controlsWithoutLabel.Add(control);
                            }
                            else
                            {
                                StringBuilder sbLabel = new StringBuilder();
                                sbLabel.Append("The label \"");
                                sbLabel.Append(controlsInActualTabOrder[index - 1].Name);
                                sbLabel.Append("\" is paired with the control \"");
                                sbLabel.Append(control.Name + "\" (" + control.ControlType.ToString() + ").");
                                UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Info, sbLabel.ToString(), Name);
                                result.AddRelatedControls(control);
                                result.AddRelatedControls(controlsInActualTabOrder[index - 1]);
                                results.Add(result);
                            }
                        }
                    }

                    if (ruleProperties.ReportMissingLabel && controlsWithoutLabel.Count > 0)
                    {
                        UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Warning, string.Empty, Name);
                        StringBuilder sbLabel = new StringBuilder();
                        sbLabel.AppendLine("The following controls do not have a label:");
                        foreach (ManagedElementInformation control in controlsWithoutLabel)
                        {
                            sbLabel.AppendLine("\"" + control.Name + "\" (" + control.ControlType.ToString() + "), ");
                            result.AddRelatedControls(control);
                        }
                        sbLabel.Remove(sbLabel.Length - 4, 4);
                        result.Message = sbLabel.ToString();
                        results.Add(result);
                    }
                }

                if (!ruleProperties.VerifyLabelsOnly)
                {
                    if (ruleProperties.IncludeLabels)
                    {
                        List<ManagedElementInformation> allLabels = new List<ManagedElementInformation>();
                        foreach (ManagedElementInformation control in controlsInActualTabOrder)
                            if (control.ControlType == ControlType.Text)
                                allLabels.Add(control);

                        foreach (ManagedElementInformation label in allLabels)
                        {
                            controlsInActualTabOrder.Remove(label);
                            controlsInExpectedTabOrder.Remove(label);
                        }
                    }

                    if (controlsInActualTabOrder[0] != controlsInExpectedTabOrder[0])
                    {
                        results.Add(new UIComplianceResultBase(ResultType.Fail, "The tab order does not start at the upper-left control.", Name));
                        int startingPoint = controlsInExpectedTabOrder.IndexOf(controlsInActualTabOrder[0]);
                        List<ManagedElementInformation> head = controlsInExpectedTabOrder.GetRange(0, startingPoint);
                        controlsInExpectedTabOrder.RemoveRange(0, startingPoint);
                        controlsInExpectedTabOrder.AddRange(head);
                    }
                    for (int i = 1; i < controlsInActualTabOrder.Count; i++)
                    {
                        int previousIndex = controlsInExpectedTabOrder.IndexOf(controlsInActualTabOrder[i - 1]);
                        int currentIndex = controlsInExpectedTabOrder.IndexOf(controlsInActualTabOrder[i]);
                        if (previousIndex != currentIndex - 1)
                        {
                            //if (i < controlsInActualTabOrder.Count - 1 && controlsInActualTabOrder[i + 1].ControlType == ControlType.ListView)
                            //{
                            //    //False alarm eliminate for searchable list view.
                            //    break;
                            //}
                            StringBuilder sbMessage = new StringBuilder();
                            sbMessage.AppendLine("The following controls are not in expected tab order:");
                            sbMessage.AppendLine("\"" + controlsInActualTabOrder[i].Name + "\"");
                            sbMessage.AppendLine("\"" + controlsInActualTabOrder[i - 1].Name + "\"");
                            UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Fail, sbMessage.ToString(), Name);
                            result.AddRelatedControls(controlsInActualTabOrder[i - 1]);
                            result.AddRelatedControls(controlsInActualTabOrder[i]);
                            //result.TakePicture = true;
                            results.Add(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                while (ex != null)
                {
                    sb.AppendLine("Exception: " + ex.GetType().Name);
                    sb.AppendLine("Message: \"" + ex.Message + "\"");
                    sb.AppendLine("Stack Trace:");
                    sb.AppendLine(ex.StackTrace);
                    sb.AppendLine("==========");
                    ex = ex.InnerException;
                }
                sb.AppendLine("Current Directory: " + Environment.CurrentDirectory);

                results.Add(new UIComplianceResultBase(ResultType.Warning, "Tab order rule encountered a critical error:\n" + sb.ToString(), Name));
            }

            return results;
        }

        private TabOrderRuleProperties ruleProperties;

        public override List<UIComplianceResultBase> UIVerify(ICollection<WebElementInfo> webElements)
        {
            throw new NotImplementedException();
        }
    }

    class TabOrderRuleUtilities
    {
        static public int GetChildIndex(ElementInformation control)
        {
            if (control == null)
                return -1;

            if (control.Parent == null)
                return 0;

            int i = 0;
            ElementInformation current = control.Parent.FirstChild;
            while (current != control)
            {
                current = current.NextSibling;
                i++;
            }

            if (current != control)
                return -1;

            return i;
        }

        static public bool IsReadOnly(ManagedElementInformation control)
        {
            try
            {
                return control.IsReadOnly;
            }
            catch { }
            return false;
        }

        static public void GetLeafControlsInActualTabOrder(ElementInformation ParentControl, List<ManagedElementInformation> results, bool tabStopOnly, bool includeLabels, ref bool hasNativeControl)
        {
            if (ParentControl.FirstChild == null || ParentControl.ControlType == ControlType.Tab)
            {
                ManagedElementInformation managedControl = ParentControl as ManagedElementInformation;
                if (managedControl != null)
                {
                    //if (managedControl.ControlType != ControlType.Client)
                    //{
                        if (!tabStopOnly || (tabStopOnly && managedControl.TabStop) || (includeLabels && managedControl.ControlType == ControlType.Text))
                        {
                            results.Add(managedControl);
                        }
                    //}
                }
                else
                    hasNativeControl = true;

                if (ParentControl.FirstChild == null)
                    return;
            }

            List<ElementInformation> subControls = new List<ElementInformation>();
            if (CheckHasHideChildren(ParentControl))
            {
                ElementInformation child = ParentControl.FirstChild;
                subControls.Add(child);
            }
            else
            {
                ElementInformation child = ParentControl.FirstChild;
                while (child != null)
                {
                    subControls.Add(child);
                    child = child.NextSibling;
                }
            }
            subControls.Sort(TabIndexComparer.DefaultComparer);

            foreach (ElementInformation control in subControls)
                GetLeafControlsInActualTabOrder(control, results, tabStopOnly, includeLabels, ref hasNativeControl);
        }

        static private bool IsButtonWithoutText(ManagedElementInformation control)
        {
            return (control.ControlType == ControlType.Button && string.IsNullOrEmpty(control.Name));
        }

        public static List<ElementInformation> GetHidenControls(ElementInformation Parent)
        {
            ElementInformation c1 = Parent.FirstChild;
            if (c1 == null)
                return null;
            ElementInformation c2 = c1.NextSibling;
            if (c2 == null)
                return null;
            if (Math.Abs(c1.BoundingRectangle.X - c2.BoundingRectangle.X) < 5 && Math.Abs(c1.BoundingRectangle.Y - c2.BoundingRectangle.Y) < 5
                && c1.BoundingRectangle.Width > 5 && c1.BoundingRectangle.Height > 5 && c2.BoundingRectangle.Width > 5 && c2.BoundingRectangle.Height > 5)
            {
                List<ElementInformation> results = new List<ElementInformation>();
                //while (c1 != null)
                //{
                //    results.Add(c1);
                //    c1 = c1.NextSibling;
                //}
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

        public static bool CheckHasHideChildren(ElementInformation Parent)
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

        static public void GetAllChildren(ElementInformation Parent, List<ElementInformation> list)
        {
            if (Parent.FirstChild == null)
                return;

            // Enum children controls
            ElementInformation child = Parent.FirstChild;
            while (child != null)
            {
                list.Add(child);
                GetAllChildren(child, list);
                child = child.NextSibling;
            }
        }

        static public bool RemoveHiddenControls(List<ManagedElementInformation> controls)
        {
            bool ret = false;
            for (int i = 0; i < controls.Count; )
                if (controls[i].IsOffscreen)
                {
                    i++;
                }
                else
                {
                    controls.RemoveAt(i);
                    ret = true;
                }
            return ret;
        }

        static public void SpecialReorderExpectedTabOrder(List<ManagedElementInformation> orderedControls)
        {
            // Special Reorder #1
            SpecialReorder(orderedControls);

            // Update tab order of SearchableListView and its action buttons
            UpdateOrderOfSearchableListViewControls(orderedControls);
        }

        private static void SpecialReorder(List<ManagedElementInformation> orderedControls)
        {
            int begin = 0;
            for (int i = 0; i < orderedControls.Count - 1; i++)
            {
                if (IsButtonWithoutText(orderedControls[i]) && orderedControls[i + 1].ControlType == ControlType.DataGrid)
                {
                    int insertAfter = i - 1;
                    if (orderedControls[i - 1].ControlType == ControlType.Edit)
                        continue;
                    while (insertAfter >= begin && IsButtonWithoutText(orderedControls[insertAfter]))
                        insertAfter--;

                    ManagedElementInformation listView = orderedControls[i + 1];
                    orderedControls.RemoveAt(i + 1);
                    orderedControls.Insert(insertAfter + 1, listView);
                    begin = i + 2;
                }
            }
        }

        private static void UpdateOrderOfSearchableListViewControls(List<ManagedElementInformation> orderedControls)
        {
            List<SearchableListView> searchableListviews = new List<SearchableListView>();

            // Get Searchablelistviews
            foreach (var control in orderedControls.Where(c => c.ControlType == ControlType.DataGrid))
            {
                SearchableListView searchableListview = null;
                if (SearchableListView.TryToContructAnInstance(control, out searchableListview))
                {
                    searchableListviews.Add(searchableListview);
                }
            }

            // Process each searchablelistview
            foreach (var slv in searchableListviews)
            {
                System.Drawing.Rectangle slvRect = slv.Rectangle;
                System.Drawing.Rectangle areaToSearch = new Rectangle(slvRect.X, 0, slvRect.Width, slvRect.Y);

                ManagedElementInformation actionButton = orderedControls.Where(c => c.ControlType == ControlType.Button && 
                    areaToSearch.Contains(new Rectangle((int)c.BoundingRectangle.X,(int)c.BoundingRectangle.Y,(int)c.BoundingRectangle.Width,(int)c.BoundingRectangle.Height)) 
                    && IsSearchableListviewActionButton(c))
                    .OrderBy(c => c.BoundingRectangle.Y)
                    .OrderBy(c => c.BoundingRectangle.X)
                    .LastOrDefault();

                if (actionButton != null)
                {
                    int buttonIndex = orderedControls.IndexOf(actionButton);

                    int startIndexOfActionButtons = buttonIndex;
                    for (int i = buttonIndex - 1; i >= 0; i--)
                    {
                        ManagedElementInformation control = orderedControls[i];
                        if (control == null || !IsSearchableListviewActionButton(control))
                        {
                            break;
                        }

                        startIndexOfActionButtons = i;
                    }

                    int endIndexOfActionButtons = buttonIndex;
                    for (int i = buttonIndex + 1; i < orderedControls.Count; i++)
                    {
                        ManagedElementInformation control = orderedControls[i];
                        if (control == null || !IsSearchableListviewActionButton(control))
                        {
                            break;
                        }

                        endIndexOfActionButtons = i;
                    }

                    // Move all the action buttons to the position after the listview control
                    List<ManagedElementInformation> controlsToMove = new List<ManagedElementInformation>();
                    for (int i = startIndexOfActionButtons; i <= endIndexOfActionButtons; i++)
                    {
                        controlsToMove.Add(orderedControls[i]);
                    }

                    orderedControls.RemoveRange(startIndexOfActionButtons, endIndexOfActionButtons - startIndexOfActionButtons + 1);
                    int lvIndex = orderedControls.IndexOf(slv.ContainedListview);
                    orderedControls.InsertRange(lvIndex + 1, controlsToMove);
                }
            }
        }

        private static bool IsSearchableListviewActionButton(ManagedElementInformation control)
        {
            if (control.ControlType != ControlType.Button)
            {
                return false;
            }

            // There should have an icon on the button
            if (!control.IsImageButton)
            {
                return false;
            }

            // There should have no text on the button
            if (!string.IsNullOrEmpty(control.Name))
            {
                return false;
            }

            // You can add more conditions to help judge that the button is 
            // an action button (e.g. Add, Edit, Delete, etc.) for a SearchableListview
            // ...

            return true;
        }

        static public bool NeedsExternalLabel(ManagedElementInformation control)
        {
            if (control.ControlType == ControlType.ComboBox ||
                control.ControlType == ControlType.Calendar ||
                control.ControlType == ControlType.SplitButton ||
                control.ControlType == ControlType.Edit ||
                control.ControlType == ControlType.List ||
                control.ControlType == ControlType.DataGrid ||
                control.ControlType == ControlType.NumericUpDown ||
                control.ControlType == ControlType.Tree)
                return true;
            return false;
        }

        class SearchableListView
        {
            private SearchableListView() { }

            /// <summary>
            /// Try to construct an SearchableListView instance from a listview
            /// </summary>
            /// <param name="listviewControl"></param>
            /// <param name="searchableListView"></param>
            /// <returns></returns>
            public static bool TryToContructAnInstance(ManagedElementInformation listviewControl, out SearchableListView searchableListView)
            {
                searchableListView = null;

                if (listviewControl.ControlType != ControlType.DataGrid)
                    return false;
                if (listviewControl.Parent == null)
                    return false;
                if (listviewControl.NextSibling == null)
                    return false;

                ElementInformation nextSibling = listviewControl.NextSibling;
                if (nextSibling.FirstChild != null && nextSibling.FirstChild.ControlType == ControlType.Edit &&
                    nextSibling.FirstChild.NextSibling != null && nextSibling.FirstChild.NextSibling.ControlType == ControlType.Button &&
                    nextSibling.FirstChild.NextSibling.NextSibling == null)
                {
                    searchableListView = new SearchableListView();
                    searchableListView.ContainedListview = listviewControl;

                    ElementInformation ParentControl = listviewControl.Parent;
                    searchableListView.Rectangle = new Rectangle((int)ParentControl.BoundingRectangle.X, (int)ParentControl.BoundingRectangle.Y, (int)ParentControl.BoundingRectangle.Width, (int)ParentControl.BoundingRectangle.Height);

                    return true;
                }

                return false;
            }

            public ManagedElementInformation ContainedListview
            {
                get;
                private set;
            }

            public Rectangle Rectangle
            {
                get;
                private set;
            }
        }

        public class PositionComparer : IComparer<ManagedElementInformation>
        {
            private bool ignoreContainer;
            public PositionComparer(bool ignoreContainer)
            {
                this.ignoreContainer = ignoreContainer;
            }

            private bool IsChildOf(ElementInformation control, ElementInformation Parent)
            {
                if (control == null)
                    return false;
                if (control == Parent)
                    return true;
                return IsChildOf(control.Parent, Parent);
            }

            public int Compare(ManagedElementInformation x, ManagedElementInformation y)
            {
                if (IsChildOf(x, y)) return 1;
                if (IsChildOf(y, x)) return -1;

                int result = 0;
                ManagedElementInformation ox = x;
                ManagedElementInformation oy = y;
                if (!ignoreContainer && x.Parent != y.Parent && x.Parent != null && y.Parent != null)
                {
                    ElementInformation ParentX = x;
                    ElementInformation ParentY = y;
                    while (ParentX.Parent != null)
                    {
                        while (ParentY.Parent != null)
                        {
                            if (ParentX.Parent == ParentY.Parent)
                                goto EndLoops;

                            ParentY = ParentY.Parent;
                        }
                        ParentX = ParentX.Parent;
                        ParentY = y;
                    }

                EndLoops:
                    if (ParentX.Parent == ParentY.Parent && ParentX.Parent != null)
                    {
                        ManagedElementInformation managedParentX = ParentX as ManagedElementInformation;
                        ManagedElementInformation managedParentY = ParentY as ManagedElementInformation;
                        if (managedParentX != null && managedParentY != null)
                        {
                            x = managedParentX;
                            y = managedParentY;
                        }
                    }
                }
                if (x != y)
                {
                    Point absPosX = new Point((int)x.BoundingRectangle.X, (int)x.BoundingRectangle.Y);
                    Point absPosY = new Point((int)y.BoundingRectangle.X, (int)y.BoundingRectangle.Y);

                    if (absPosX.Y + x.BoundingRectangle.Height <= absPosY.Y)		// Y under X
                        result = -1;
                    else if (absPosY.Y + y.BoundingRectangle.Height <= absPosX.Y)	// X under Y
                        result = 1;
                    else if (absPosX.X == absPosY.X)						// Vertically overlapped
                        result = absPosX.Y - absPosY.Y;
                    else													// Same row
                        result = absPosX.X - absPosY.X;

                    if (result == 0 && (x != ox || y != oy))
                        result = TabIndexComparer.DefaultComparer.Compare(x, y);
                }
                return result;
            }

            public readonly static PositionComparer IgnoreContainerComparer = new PositionComparer(true);
            public readonly static PositionComparer DefaultComparer = new PositionComparer(false);
        }

        public class TabIndexComparer : IComparer<ElementInformation>
        {
            public int Compare(ElementInformation x, ElementInformation y)
            {
                if (x.Parent == null || x.Parent != y.Parent)
                {
                    throw new Exception("The two controls must be in a same container.");
                }

                ManagedElementInformation managedX = x as ManagedElementInformation;
                ManagedElementInformation managedY = y as ManagedElementInformation;

                try
                {
                    int xT = managedX.TabIndex;
                }
                catch
                {
                    return 0;
                    //if (managedX.FirstChild == null)
                    //    return managedY.TabIndex;
                }

                try
                {
                    int yT = managedY.TabIndex;
                }
                catch
                {
                    return 0;
                    //if (managedY.FirstChild == null)
                    //    return managedX.TabIndex;
                }

                int result = 0;
                if (managedX != null && managedY != null)
                    result = managedX.TabIndex - managedY.TabIndex;
                if (result == 0 && x != y)
                    result = TabOrderRuleUtilities.GetChildIndex(x) - TabOrderRuleUtilities.GetChildIndex(y);
                return result;
            }

            public readonly static TabIndexComparer DefaultComparer = new TabIndexComparer();
        }
    }

    public class TabOrderRuleProperties
    {
        public bool IgnoreContainer
        {
            get { return ignoreContainer; }
            set { ignoreContainer = value; }
        }
        private bool ignoreContainer = false;

        public bool TabStopOnly
        {
            get { return tabStopOnly; }
            set { tabStopOnly = value; }
        }
        private bool tabStopOnly = true;

        public bool ShowTabOrder
        {
            get { return showTabOrder; }
            set { showTabOrder = value; }
        }
        private bool showTabOrder = true;

        public bool IncludeLabels
        {
            get { return includeLabels; }
            set
            {
                if (value == false)
                {
                    showControlLabel = false;
                    verifyLabelsOnly = false;
                    reportMissingLabel = false;
                    labelMustHasAccessKey = false;
                }
                includeLabels = value;
            }
        }
        private bool includeLabels = true;

        public bool ShowControlLabel
        {
            get { return showControlLabel; }
            set
            {
                if (value == true)
                    includeLabels = true;
                else
                {
                    reportMissingLabel = false;
                    verifyLabelsOnly = false;
                    labelMustHasAccessKey = false;
                }
                showControlLabel = value;
            }
        }
        private bool showControlLabel = false;

        public bool LabelMustHasAccessKey
        {
            get { return labelMustHasAccessKey; }
            set
            {
                if (value == true)
                {
                    includeLabels = true;
                    showControlLabel = true;
                }
                labelMustHasAccessKey = value;
            }
        }
        private bool labelMustHasAccessKey = false;

        public bool ReportMissingLabel
        {
            get { return reportMissingLabel; }
            set
            {
                if (value == true)
                {
                    includeLabels = true;
                    showControlLabel = true;
                }
                reportMissingLabel = value;
            }
        }
        private bool reportMissingLabel = false;

        public bool VerifyLabelsOnly
        {
            get { return verifyLabelsOnly; }
            set
            {
                if (value == true)
                {
                    includeLabels = true;
                    showControlLabel = true;
                }
                verifyLabelsOnly = value;
            }
        }
        private bool verifyLabelsOnly = false;
    }
}
