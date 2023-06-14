using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.UICompliance;
using System.Drawing;
using MS.Internal.SulpHur.Utilities;

namespace MS.Internal.SulpHur.CMRules
{
    public class TabOrderRule : UIComplianceRuleBase
    {
        public static List<string> verticalSortPageNameList;
        public static bool belongToVerticalSortPageName;
        static TabOrderRule()
        {
            SulpHurEntitiesForCMRules sulpHurEntitiesForCMRules = new SulpHurEntitiesForCMRules();
            verticalSortPageNameList = sulpHurEntitiesForCMRules.VerticalSortPageNameLists.Where(x => x.name != null).Select(x => x.name).ToList();
        }


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
        }

        public override List<UIComplianceResultBase> UIVerify(ICollection<ElementInformation> controls)
        {
            List<UIComplianceResultBase> results = new List<UIComplianceResultBase>();
            if (this.isOSidentity == false)
            {
                results.Add(new UIComplianceResultBase(ResultType.Warning, "This UI is already verified in different OS type.\n", Name));
                return results;
            }
            else
            {
                if (lan == "ENU")
                {
                    try
                    {
                        ElementInformation rootControl = CommonRuleUtility.GetRootElement(controls);
                        List<ElementInformation> hidens = CommonRuleUtility.GetHidenControls(rootControl);
                        //create warning result
                        if (hidens != null)
                        {
                            string msg = "The dialog or pagecontrol contains hide controls:";
                            UIComplianceResultBase result = CommonRuleUtility.CreateWarningResult(msg, this.Name, hidens);
                            results.Add(result);
                        }
                        List<ElementInformation> controlsInActualTabOrder = new List<ElementInformation>();
                        belongToVerticalSortPageName = verticalSortPageNameList.Contains(rootControl.Name);

                        TabOrderRuleUtilities.GetLeafControlsInActualTabOrder(rootControl, controlsInActualTabOrder);
                        #region Warning offscreen controls/Warning only native controls or no tabstop
                        bool hasMsgWarning = false;
                        StringBuilder msgWaningMsg = new StringBuilder();
                        if (TabOrderRuleUtilities.RemoveHiddenControls(controlsInActualTabOrder))
                        {
                            msgWaningMsg.AppendLine("The window contains offscreen control(s). The tab orders for those controls are not checked.");
                            hasMsgWarning = true;
                        }

                        if (controlsInActualTabOrder.Count == 0)
                        {
                            msgWaningMsg.AppendLine("Tab order cannot be checked because:\n1. The window contains native controls only, or;\n2. No controls has tab stop.");
                            hasMsgWarning = true;
                        }

                        if (hasMsgWarning)
                        {
                            UIComplianceResultBase r = new UIComplianceResultBase(ResultType.Warning, msgWaningMsg.ToString(), this.Name);
                            results.Add(r);
                            return results;
                        }
                        #endregion

                        #region Warning native controls not checked
                        List<ElementInformation> nativeControls = new List<ElementInformation>();
                        CommonRuleUtility.HasNativeControls(rootControl, nativeControls);
                        if (nativeControls.Count > 0)
                        {
                            string msg = "The window contains native control(s). The tab orders for those controls are not checked.";
                            UIComplianceResultBase r = CommonRuleUtility.CreateWarningResult(msg, this.Name, nativeControls);
                            results.Add(r);
                        }

                        List<ElementInformation> controlsInExpectedTabOrder = new List<ElementInformation>(controlsInActualTabOrder);
                        controlsInExpectedTabOrder.Sort(TabOrderRuleUtilities.PositionComparer.DefaultComparer);
                        TabOrderRuleUtilities.SpecialReorderExpectedTabOrder(controlsInExpectedTabOrder);
                        #endregion
                        #region Fail
                        bool isFail = false;
                        // tab order should be as cycle
                        if (controlsInActualTabOrder.Count > 2)
                        {
                            int startIndex = 0;
                            for (; startIndex < controlsInActualTabOrder.Count; startIndex++)
                            {
                                if (controlsInActualTabOrder[startIndex] == controlsInExpectedTabOrder[0])
                                    break;
                            }
                            int index = 0;
                            while (index < controlsInExpectedTabOrder.Count)
                            {
                                if (controlsInExpectedTabOrder[index] != controlsInActualTabOrder[startIndex])
                                {
                                    isFail = true;
                                    break;
                                }
                                index++;
                                startIndex++;
                                if (startIndex == controlsInExpectedTabOrder.Count)
                                    startIndex = 0;
                            }
                        }
                        //if (controlsInActualTabOrder[0] != controlsInExpectedTabOrder[0])
                        //{
                        //    isFail = true;
                        //}
                        //for (int i = 1; i < controlsInActualTabOrder.Count; i++)
                        //{
                        //    int previousIndex = controlsInExpectedTabOrder.IndexOf(controlsInActualTabOrder[i - 1]);
                        //    int currentIndex = controlsInExpectedTabOrder.IndexOf(controlsInActualTabOrder[i]);
                        //    if (previousIndex != currentIndex - 1)
                        //    {
                        //        /*  for test
                        //        Console.WriteLine(controlsInExpectedTabOrder[i - 1].Name);
                        //        Console.WriteLine(controlsInExpectedTabOrder[i].Name);
                        //        */
                        //        isFail = true;
                        //        break;
                        //    }
                        //}
                        #endregion
                        if (isFail)
                        {
                            Bitmap actualOrderBitmap = DrawBitmapWithNumber(ControlScreen.CurrentBit, controlsInActualTabOrder, rootControl);
                            Bitmap expectedOrderBitmap = DrawBitmapWithNumber(ControlScreen.CurrentBit, controlsInExpectedTabOrder, rootControl);
                            Bitmap final = TabOrderRuleUtilities.MergeBitmap(actualOrderBitmap, expectedOrderBitmap);
                            UIComplianceResultBase r = new UIComplianceResultBase(ResultType.Fail, "See Attached Picture, Left(Actual Order), Right(Expected Order)", this.Name);
                            r.Image = final;
                            results.Add(r);
                            actualOrderBitmap.Dispose();
                            expectedOrderBitmap.Dispose();
                        }                       
                        if (results.Count == 0)
                        {
                            UIComplianceResultBase r = new UIComplianceResultBase(ResultType.Pass, "Actual tab order is equals expected tab order", this.Name);
                            r.Image = ControlScreen.CurrentBit;
                            results.Add(r);
                        }
                    }
                    catch (Exception ex)
                    {
                        results.Add(new UIComplianceResultBase(ResultType.Warning, "Tab order rule encountered a critical error:\n" + ex.ToString(), Name));
                    }
                    return results;
                }
                else
                {
                    results.Add(new UIComplianceResultBase(ResultType.Warning, "Tab order rule Only works on ENU. Others will set to warning.\n", Name));
                    return results;
                }
            }
        }

        private Bitmap DrawBitmapWithNumber(Bitmap bit, List<MS.Internal.SulpHur.UICompliance.ElementInformation> relatedControls, MS.Internal.SulpHur.UICompliance.ElementInformation root)
        {
            Bitmap tempBit = new Bitmap(bit);

            Graphics gfxImage = Graphics.FromImage(tempBit);
            int i = 1;
            foreach (MS.Internal.SulpHur.UICompliance.ElementInformation ei in relatedControls)
            {
                System.Drawing.Pen controlPen = new System.Drawing.Pen(System.Drawing.Color.Yellow, 2);//(color,width)
                System.Windows.Rect rectTemp = new System.Windows.Rect(ei.BoundingRectangle.X, ei.BoundingRectangle.Y, ei.BoundingRectangle.Width, ei.BoundingRectangle.Height);
                double offsetX = -root.BoundingRectangle.X;
                double offsetY = -root.BoundingRectangle.Y;
                rectTemp.Offset(offsetX, offsetY);
                gfxImage.DrawRectangle(controlPen, CommonUtility.ToWinRectangle(rectTemp));
                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Center;
                stringFormat.LineAlignment = StringAlignment.Center;
                if (rectTemp.Width < 40)
                {
                    gfxImage.DrawString(i.ToString(), new Font("Segoe UI", 10), System.Drawing.Brushes.Red, CommonUtility.ToWinRectangle(rectTemp), stringFormat);
                }
                else if (rectTemp.Height < 15)
                {
                    gfxImage.DrawString(i.ToString(), new Font("Segoe UI", 18), System.Drawing.Brushes.Red, CommonUtility.ToWinRectangle(rectTemp), stringFormat);
                }
                else
                {
                    gfxImage.DrawString(i.ToString(), new Font("Segoe UI", 24), System.Drawing.Brushes.Red, CommonUtility.ToWinRectangle(rectTemp), stringFormat);

                }
                i++;
            }
            return tempBit;
        }

        public override List<UIComplianceResultBase> UIVerify(ICollection<WebElementInfo> webElements)
        {
            throw new NotImplementedException();
        }
    }
   
    class TabOrderRuleUtilities
    {
        private static List<ControlType> filteredTypes = new List<ControlType>() { ControlType.Pane, ControlType.Tab, ControlType.Window, ControlType.ToolTip };
        private static List<ControlType> leafTypes = new List<ControlType>() {
            ControlType.ComboBox,//children of combobox will not checked
            ControlType.DataGrid,//If tabstop of wpf datagrid is not set to true
            ControlType.Button//WPF Button has children
        };
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
        static public bool IsReadOnly(ElementInformation control)
        {
            try
            {
                return control.IsReadOnly;
            }
            catch { }
            return false;
        }
        static public void GetLeafControlsInActualTabOrder(ElementInformation ParentControl, List<ElementInformation> results)
        {
            if (ParentControl.FirstChild == null || ParentControl.ControlType == ControlType.TabItem || leafTypes.Contains(ParentControl.ControlType) || CommonRuleUtility.IsIPAddressControl(ParentControl))
            {
                if (!filteredTypes.Contains(ParentControl.ControlType) || CommonRuleUtility.IsIPAddressControl(ParentControl))
                {
                    if (ParentControl.TabStop || ParentControl.ControlType == ControlType.Document)
                    {
                        results.Add(ParentControl);
                    }
                }
                if (ParentControl.FirstChild == null || leafTypes.Contains(ParentControl.ControlType) || CommonRuleUtility.IsIPAddressControl(ParentControl))
                    return;
            }

            List<ElementInformation> subControls = new List<ElementInformation>();
            if (CommonRuleUtility.HasHidenChildren(ParentControl))
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
                GetLeafControlsInActualTabOrder(control, results);
        }
        static private bool IsButtonWithoutText(ElementInformation control)
        {
            return (control.ControlType == ControlType.Button && string.IsNullOrEmpty(control.Name));
        }

        static public bool RemoveHiddenControls(List<ElementInformation> controls)
        {
            bool ret = false;
            for (int i = 0; i < controls.Count;)
                if (!controls[i].IsOffscreen)
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
        static public void SpecialReorderExpectedTabOrder(List<ElementInformation> orderedControls)
        {
            //Special order of List and it's shoulder button
            bool hasChange = false;
            Dictionary<int, List<ElementInformation>> removedList = new Dictionary<int, List<ElementInformation>>();
            for (int i = 0; i < orderedControls.Count; i++)
            {
                if (orderedControls[i].ControlType == ControlType.List || orderedControls[i].ControlType == ControlType.DataGrid || orderedControls[i].ControlType == ControlType.Tree)
                {
                    ElementInformation list = orderedControls[i];
                    int key = i;
                    List<ElementInformation> temp = new List<ElementInformation>();
                    int k = i - 1;
                    for (int j = 0; j < 7 && k >= 0; j++)
                    {
                        if ((orderedControls[k].IsImageButton &&
                            string.IsNullOrEmpty(orderedControls[k].Name) //If IsImageButton property not get success
                            &&
                            orderedControls[k].Parent.AutomationId != "panelFilterBar")//Miss search button
                            || IsShoulderButton(orderedControls[k], list))
                        {
                            hasChange = true;
                            temp.Insert(0, orderedControls[k]);
                            orderedControls.RemoveAt(k);
                            key--;
                        }
                        k--;
                    }
                    removedList.Add(key, temp);
                }
            }

            if (hasChange)
            {
                int nextoffset = 0;
                foreach (KeyValuePair<int, List<ElementInformation>> pair in removedList)
                {
                    orderedControls.InsertRange(pair.Key + 1 + nextoffset, pair.Value);
                    nextoffset += pair.Value.Count;
                }
            }

            //Special order of Wizard header text
            //ElementInformation temp1=null;
            //ElementInformation temp2=null;
            //foreach (ElementInformation ei in orderedControls) {
            //    if (ei.AutomationId == "_headerBar") {
            //        temp1 = ei;
            //    }
            //    if (ei.AutomationId == "_headlineLabel") {
            //        temp2 = ei;
            //    }
            //}
            //if (temp1 != null)
            //{
            //    orderedControls.Remove(temp1);
            //    orderedControls.Add(temp1);
            //}
            //if (temp2 != null)
            //{
            //    orderedControls.Remove(temp2);
            //    orderedControls.Add(temp2);
            //}
        }

        private static bool IsShoulderButton(ElementInformation ei, ElementInformation list)
        {
            if (Math.Abs(list.Y - ei.Y - ei.Height) <= 10 && ei.X > (list.X + list.Width / 4) && ei.ControlType == ControlType.Button && ei.AutomationId != "buttonImage") return true;
            return false;
        }
        public static Bitmap MergeBitmap(Bitmap left, Bitmap right)
        {
            Bitmap mer = new Bitmap(2 * left.Width, left.Height);
            using (Graphics g = Graphics.FromImage(mer))
            {
                g.DrawImage(left, 0, 0, left.Width, left.Height);
                g.DrawImage(right, left.Width, 0, left.Width, left.Height);
            }
            return mer;
        }
        


        public class PositionComparer : IComparer<ElementInformation>
        {
            private bool ignoreContainer;
            public PositionComparer(bool ignoreContainer)
            {
                this.ignoreContainer = ignoreContainer;
            }


            // Recursion is less efficient
            //private bool IsChildOf(ElementInformation control, ElementInformation Parent)
            //{
            //    if (control == null)
            //        return false;
            //    if (control == Parent)
            //        return true;
            //    return IsChildOf(control.Parent, Parent);
            //}

            //iteration is more efficient
            private bool IsChildOf(ElementInformation control, ElementInformation Parent)
            {
                if (control == null) return false;
                else if (control == Parent) return true;
                while (control != Parent)
                {
                    control = control.Parent;
                    if (control == Parent) return true;
                    else if (control == null) return false;
                }
                return false;
            }



            public int Compare(ElementInformation x, ElementInformation y)
            {
                if (IsChildOf(x, y)) return 1;
                if (IsChildOf(y, x)) return -1;

                int result = 0;
                ElementInformation ox = x;  //original x
                ElementInformation oy = y;  //original y

                if (!ignoreContainer && x.Parent != y.Parent && x.Parent != null && y.Parent != null)
                {
                    ElementInformation ParentX = x;
                    ElementInformation ParentY = y;

                    bool endloop = false;
                    while (ParentX.Parent != null)
                    {
                        while (ParentY.Parent != null)
                        {
                            if (ParentX.Parent == ParentY.Parent && ParentX.Parent != null)
                            {
                                x = ParentX;
                                y = ParentY;
                                endloop = true;
                                break;
                            }
                            ParentY = ParentY.Parent;
                        }
                        if (endloop) break;
                        ParentX = ParentX.Parent;
                        ParentY = y;
                    }
                }

                if ("_navPanel" == x.AutomationId.Trim() && ("_navPanel" != y.AutomationId.Trim()))
                {
                    result = 1;
                }
                else if (!("_navPanel" == x.AutomationId.Trim()) && "_navPanel" == y.AutomationId.Trim())
                {
                    result = -1;
                }
                else if (x != y)
                {
                    if (TabOrderRule.belongToVerticalSortPageName && x.Parent.Name == "Schedule" && y.Parent.Name == "Schedule")
                        result = specialCompareFor_VSPN(x, y);  //vertical sort page name
                    else                    
                    {
                        Point absPosX = new Point(x.X, x.Y);
                        Point absPosY = new Point(y.X, y.Y);
                        if (absPosX.Y + x.Height <= absPosY.Y + 1)      // Y under X 
                            result = -1;
                        else if (absPosY.Y + y.Height <= absPosX.Y + 1) // X under Y
                            result = 1;
                        else if (absPosX.X == absPosY.X)                // Vertically overlapped
                            result = absPosX.Y - absPosY.Y;
                        else                                            // Same row
                            result = absPosX.X - absPosY.X;

                        if (result == 0 && (x != ox || y != oy))
                            result = TabIndexComparer.DefaultComparer.Compare(x, y);
                    }
                    #region
                    /*
                    if (!(x.Parent.Name == "Schedule" && y.Parent.Name == "Schedule" && TabOrderRule.belongToVerticalSortPageName))
                    {
                        Point absPosX = new Point(x.X, x.Y);
                        Point absPosY = new Point(y.X, y.Y);
                        if (absPosX.Y + x.Height <= absPosY.Y + 1)      // Y under X 
                            result = -1;
                        if (absPosY.Y + y.Height <= absPosX.Y + 1)      // X under Y
                            result = 1;
                        if (absPosX.X == absPosY.X)                     // Vertically overlapped
                            result = absPosX.Y - absPosY.Y;
                        if (absPosX.Y == absPosY.Y)                     // Same row
                            result = absPosX.X - absPosY.X;

                        if (result == 0 && (x != ox || y != oy))
                            result = TabIndexComparer.DefaultComparer.Compare(x, y);
                    }
                    if (TabOrderRule.belongToVerticalSortPageName && x.Parent.Name == "Schedule" && y.Parent.Name == "Schedule")
                        result = specialCompareFor_VSPN(x, y);  //vertical sort page name
                    */
                    #endregion
                }
                return result;
            }
            public int specialCompareFor_VSPN(ElementInformation x, ElementInformation y)
            {
                Point absPosX = new Point(x.X, x.Y);
                Point absPosY = new Point(y.X, y.Y);                
                int result = 0;
                if (absPosX.X + x.Width <= absPosY.X)
                {
                    result = -1;
                }
                else
                {
                    if (absPosX.X > absPosY.X + y.Width)
                        result = 1;
                    else if (absPosX.Y + x.Height <= absPosY.Y + 1)
                        result = -1;
                    else
                        result = 1;
                }

                #region
                /*
                  bool notIsAbsltLandR = true;
                if (absPosX.X + x.Width <= absPosY.X)
                { result = -1; notIsAbsltLandR = false; }

                if (absPosX.X > absPosY.X + y.Width)
                {
                    result = 1; notIsAbsltLandR = false;
                }
                if (notIsAbsltLandR && (absPosX.Y + x.Height <= absPosY.Y + 1))
                    result = -1;
                if (notIsAbsltLandR && (absPosY.Y + y.Height <= absPosX.Y + 1))
                    result = 1;
                */
                #endregion
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
                ElementInformation managedX = x;
                ElementInformation managedY = y;

                try
                {
                    int xT = managedX.TabIndex;
                }
                catch
                {
                    return 0;
                }

                try
                {
                    int yT = managedY.TabIndex;
                }
                catch
                {
                    return 0;
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
}
