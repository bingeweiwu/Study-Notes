using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MS.Internal.SulpHur.UICompliance;
using System.Drawing;
using MS.Internal.SulpHur.Utilities;
using MS.Internal.SulpHur.SulpHurService.DataAccess;
using System.Diagnostics;

namespace MS.Internal.SulpHur.SulpHurService
{
    public class UIContentVerification : VerifyOperations
    {
        private Thread verifyThread;
        private bool serviceStarted = true;
        //private RuleManager ruleManager;
        private Queue<UIANDRule> dirtyUIQueue;
        private object obj = new object();

        public UIContentVerification()
        {
            verifyThread = new Thread(DoVerify);

            //this.ruleManager = ruleManager;

            dirtyUIQueue = new Queue<UIANDRule>();
        }

        public void Start()
        {
            serviceStarted = true;
            verifyThread.Start();
        }

        public bool IsAlive()
        {
            return verifyThread.IsAlive;
        }

        public void Stop()
        {
            serviceStarted = false;
            verifyThread.Join(new TimeSpan(0, 2, 0));
        }

        private void DoVerify()
        {
            Log.WriteServerLog("Start DoVerify...");
            //v-yiwzha: bug481051 initial try times while out of memory
            int trytimes = 0;
            while (serviceStarted)
            {
                try
                {
                    using (DBOperator dbOperator = new DBOperator())
                    {
                        List<UIANDRule> allDirtyUI = dbOperator.QueryAllDirtyUI();
                        lock (obj)
                        {
                            foreach (UIANDRule temp in allDirtyUI)
                            {
                                dirtyUIQueue.Enqueue(temp);
                            }
                        }
                        
                        while (dirtyUIQueue.Count > 0)
                        {
                            UIANDRule temp;
                            lock (obj)
                            {
                                temp = dirtyUIQueue.Dequeue();
                            }

                            UIComplianceRuleBase rule = RuleManager.RuleList.SingleOrDefault(r => r.Name == temp.RuleName);
                            Log.WriteServerLog($"dirtyUIQueue.Count:{dirtyUIQueue.Count}", TraceLevel.Verbose);
                            Log.WriteServerLog($"rule:id{temp.Uiid},rulename{temp.RuleName}", TraceLevel.Verbose);
                            if (rule == null)
                            {
                                Log.WriteServerLog("Fail to get rule instance, rule name:" + temp.RuleName, TraceLevel.Verbose);
                                Log.WriteServerLog("All available rule name:", TraceLevel.Verbose);
                                foreach (UIComplianceRuleBase b in RuleManager.RuleList)
                                {
                                    Log.WriteServerLog(b.Name, TraceLevel.Verbose);
                                }
                                continue;
                            }
                            Log.WriteServerLog(string.Format("Start to verify UI ID:{0}, ruleName:{1}", temp.Uiid, temp.RuleName), TraceLevel.Verbose);
                            Bitmap bitMap = temp.Bitmap;
                            ControlScreen.CurrentBit = bitMap;
                            if (temp.IsWebUI)
                            {
                                continue;
                                #region WebUI
                                List<WebElementInfo> webEiList = ParseWebUIToList(temp.Element);
                                List<UIComplianceResultBase> results = null;
                                try
                                {
                                    results = rule.UIVerify(webEiList);
                                }
                                catch
                                {
                                    // results.Add(new UIComplianceResultBase(ResultType.Pass,"", 
                                }

                                //if(results==null)
                                foreach (UIComplianceResultBase b in results)
                                {
                                    Log.WriteServerLog(string.Format("Insert result uiid:{0}, ruleName:{1}, msg:{2}", temp.Uiid, b.RuleName, b.Message), TraceLevel.Verbose);
                                    Bitmap tempBit = new Bitmap(bitMap);

                                    int top = 0;
                                    int left = 0;
                                    int right = 0;
                                    int bottom = 0;
                                    foreach (WebElementInfo webEi in b.WebTags)
                                    {
                                        Rectangle rect = webEi.BoundingRectangle;

                                        if (rect.X < 0 && System.Math.Abs(rect.X) > left)
                                        {
                                            left = System.Math.Abs(rect.X);
                                        }
                                        if (rect.Y < 0 && System.Math.Abs(rect.Y) > top)
                                        {
                                            top = System.Math.Abs(rect.Y);
                                        }
                                        if (rect.X + rect.Width > tempBit.Width && (rect.X + rect.Width - tempBit.Width) > right)
                                        {
                                            right = rect.X + rect.Width - tempBit.Width;
                                        }
                                        if (rect.Y + rect.Height > tempBit.Height && (rect.Y + rect.Height - tempBit.Height) > bottom)
                                        {
                                            bottom = rect.Y + rect.Height - tempBit.Height;
                                        }
                                    }

                                    tempBit = ExtendBitmap(tempBit, top, left, right, bottom);

                                    Graphics gfxImage = Graphics.FromImage(tempBit);
                                    foreach (WebElementInfo webEi in b.WebTags)
                                    {
                                        Pen controlPen = new Pen(Color.Yellow, 2);
                                        gfxImage.DrawRectangle(controlPen, webEi.BoundingRectangle);
                                    }
                                    b.Image = tempBit;
                                    b.UIID = temp.Uiid;

                                    dbOperator.InsertResult(b);
                                }
                                #endregion
                            }
                            else
                            {
                                Log.WriteServerLog(string.Format("Parse tree to list, UI ID:{0}", temp.Uiid), TraceLevel.Verbose);

                                List<ElementInformation> eiList = ParseTreeToList(temp.Element);
                                eiList = FilterControl.FilterCon(eiList);

                                //bug 7258537
                                if (eiList.Count == 0)
                                {
                                    UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Pass, "No issue found.", temp.RuleName);
                                    result.UIID = temp.Uiid;

                                    dbOperator.InsertResult(result);
                                    continue;
                                }

                                ElementInformation root = eiList[0];
                                //if (IsDialogBox(root) && rule.Name != "ScreenShot Rule") {
                                //    Log.WriteServerLog("Dialog is a dialogbox, and rule is not screenshot rule,ignore this UI");
                                //    continue;
                                //}
                                //miss not implemented rule
                                List<UIComplianceResultBase> results = null;
                                try
                                {
                                    ControlScreen cs = new ControlScreen();
                                    string log = string.Empty;
                                    if (cs.CommonScreen(eiList, out log))
                                    {
                                        rule.lan = dbOperator.QuerylanbycontentID(temp.Uiid);
                                        if (rule.Name == "Tab Order Rule" || rule.Name == "Automation ID Miss Rule" || rule.Name.Contains("Access Key"))
                                        {
                                            rule.isOSidentity = true;
                                            //foreach (System.Xml.Linq.XElement t in dbOperator.GetHistoryElementsListForContent(temp.Uiid))
                                            //{
                                            //    int i;
                                            //    if (eiList.Count != ParseTreeToList(t).Count)
                                            //    {
                                            //        Log.WriteServerLog("The 2 UI have different elements count:" + eiList.Count + "&" + ParseTreeToList(t).Count);
                                            //        break;
                                            //    }
                                            //    for (i = 0; i < eiList.Count && i < ParseTreeToList(t).Count; i++ )
                                            //    {
                                            //        if (String.Compare(dbOperator.GetNameFromElementList(eiList)[i],dbOperator.GetNameFromElementList(ParseTreeToList(t))[i]) != 0)
                                            //        {
                                            //            Log.WriteServerLog("The " + i + " element name are different between " + dbOperator.GetNameFromElementList(eiList)[i] + "|" + dbOperator.GetNameFromElementList(ParseTreeToList(t))[i]);
                                            //            i = eiList.Count + 1;
                                            //            break;
                                            //        }
                                            //    }
                                            //    if (i == eiList.Count && i == ParseTreeToList(t).Count)
                                            //    {
                                            //        Log.WriteServerLog("Set " + temp.Uiid + " as OS Identity.");
                                            //        rule.isOSidentity = false;
                                            //        break;
                                            //    }
                                            //}
                                        }
                                        results = rule.UIVerify(eiList);
                                    }
                                    else
                                    {
                                        Log.WriteServerLog(log, TraceLevel.Info);
                                        continue;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Log.WriteServerLog(string.Format("ERROR:[ID:{0}]", temp.Uiid), TraceLevel.Error);
                                    Log.WriteServerLog(ex.ToString(), TraceLevel.Error);
                                    continue;
                                }
                                //workaround with rule issue
                                if (results == null)
                                    continue;
                                //write result to DB
                                dbOperator.DeleteExistingResult(temp.Uiid, temp.RuleName);

                                foreach (UIComplianceResultBase b in results)
                                {
                                    Log.WriteServerLog(string.Format("Insert result UI ID:{0}, ruleName:{1}, type:{2}", temp.Uiid, b.RuleName, b.Type), TraceLevel.Verbose);


                                    if (b.Image == null)
                                    {
                                        if (b.RuleName == "CM Alignment Rule")
                                        {
                                            b.Image = AlignmentDrawBitmap(bitMap, b.Controls, root, b.Message);
                                        }
                                        else
                                        {
                                            b.Image = DrawBitmap(bitMap, b.Controls, root);
                                        }
                                    }
                                    b.UIID = temp.Uiid;

                                    dbOperator.InsertResult(b);

                                    //release bitmap - results
                                    if (b.Image!=null)
                                    {
                                        b.Image.Dispose();
                                    }
                                }
                                //release bitmap - temp
                                temp.Bitmap.Dispose();
                            }
                        }
                    }
                }
                catch (OutOfMemoryException e)
                {
                    //v-yiwzha: bug481051 clear GC to resolve the out of memory exception
                    if (trytimes < 10)
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();
                        Log.WriteServerLog(e.Message, TraceLevel.Error);
                        Log.WriteServerLog("Force GC with 10 times.", TraceLevel.Warning);
                        trytimes++;
                    }
                    else
                    {
                        //v-yiwzha: bug481051 blocking the calling thread with 2 minutes until other thread terminates
                        verifyThread.Join(new TimeSpan(0, 2, 0));
                        verifyThread.Start();
                        Log.WriteServerLog("Restart verify thread.", TraceLevel.Warning);
                    }
                }
                catch (Exception e)
                {
                    Log.WriteServerLog(e.Message, TraceLevel.Error);
                }
                System.Threading.Thread.Sleep(1000);
            }
        }

        public Bitmap DrawBitmap(Bitmap bit, List<ElementInformation> relatedControls, ElementInformation root)
        {
            Bitmap tempBit = new Bitmap(bit);

            Graphics gfxImage = Graphics.FromImage(tempBit);
            foreach (ElementInformation ei in relatedControls)
            {
                Pen controlPen = new Pen(Color.Yellow, 2);
                System.Windows.Rect rectTemp = new System.Windows.Rect(ei.BoundingRectangle.X, ei.BoundingRectangle.Y, ei.BoundingRectangle.Width, ei.BoundingRectangle.Height);
                double offsetX = -root.BoundingRectangle.X;
                double offsetY = -root.BoundingRectangle.Y;
                rectTemp.Offset(offsetX, offsetY);

                if (rectTemp.X >= root.BoundingRectangle.Width || rectTemp.Y >= root.BoundingRectangle.Height || rectTemp.X + rectTemp.Width <= 0 || rectTemp.Y + rectTemp.Height <= 0)
                    continue;

                if (rectTemp.X < 0) rectTemp.X = 0;
                if (rectTemp.Y < 0) rectTemp.Y = 0;
                if (rectTemp.Y + rectTemp.Height > root.BoundingRectangle.Height) rectTemp.Height = root.BoundingRectangle.Height - rectTemp.Y;
                if (rectTemp.X + rectTemp.Width > root.BoundingRectangle.Width) rectTemp.Width = root.BoundingRectangle.Width - rectTemp.X;
                gfxImage.DrawRectangle(controlPen, CommonUtility.ToWinRectangle(rectTemp));
            }
            gfxImage.Dispose();
            return tempBit;
        }
        public Bitmap AlignmentDrawBitmap(Bitmap bit, List<ElementInformation> relatedControls, ElementInformation root, string message)
        {
            Bitmap tempBit = new Bitmap(bit);
            bool drawflag = false;
            Graphics gfxImage = Graphics.FromImage(tempBit);
            foreach (ElementInformation ei in relatedControls)
            {
                Pen controlPen = new Pen(Color.Yellow, 2);
                System.Windows.Rect rectTemp = new System.Windows.Rect(ei.BoundingRectangle.X, ei.BoundingRectangle.Y, ei.BoundingRectangle.Width, ei.BoundingRectangle.Height);
                double offsetX = -root.BoundingRectangle.X;
                double offsetY = -root.BoundingRectangle.Y;
                rectTemp.Offset(offsetX, offsetY);
                gfxImage.DrawRectangle(controlPen, CommonUtility.ToWinRectangle(rectTemp));
                
                Pen LinePen = new Pen(Color.Purple, 1);
                if (drawflag == false)
                {
                    if (message.StartsWith("Vertical"))
                    {
                        if (message.Contains("Left-aligned"))
                        {
                            gfxImage.DrawLine(LinePen, new Point((int)ei.BoundingRectangle.X + (int)offsetX, 0), new Point((int)ei.BoundingRectangle.X + (int)offsetX, tempBit.Height));
                            drawflag = true;
                        }
                        else
                        {
                            gfxImage.DrawLine(LinePen, new Point((int)ei.BoundingRectangle.X + (int)offsetX + (int)ei.Width, 0), new Point((int)ei.BoundingRectangle.X + (int)offsetX + (int)ei.Width, tempBit.Height));
                            drawflag = true;
                        }
                    }
                    else if (message.Contains("central-aligned"))
                    {
                        gfxImage.DrawLine(LinePen, new Point(0, (int)ei.BoundingRectangle.Y + (int)offsetY + (int)ei.Height / 2), new Point(tempBit.Width, (int)ei.BoundingRectangle.Y + (int)offsetY + (int)ei.Height / 2));
                        drawflag = true;
                    }
                    else
                    {
                        gfxImage.DrawLine(LinePen, new Point(0, (int)ei.BoundingRectangle.Y + (int)offsetY), new Point(tempBit.Width, (int)ei.BoundingRectangle.Y + (int)offsetY));
                        drawflag = true;
                    }
                }
            }
            gfxImage.Dispose();
            return tempBit;
        }


        private Bitmap ExtendBitmap(Bitmap bitmap, int top, int left, int right, int bottom)
        {
            Bitmap newImage = new Bitmap(bitmap.Width + left + right, bitmap.Height + top + bottom, bitmap.PixelFormat);
            using (Graphics g = Graphics.FromImage(newImage))
            {
                g.FillRectangle(Brushes.White, 0, 0, newImage.Width, newImage.Height);
                g.DrawImage(bitmap, left, top);
            }
            return newImage;
        }

        private List<WebElementInfo> ParseWebUIToList(System.Xml.Linq.XElement xElement)
        {
            WebElementInfo pageInfo = MS.Internal.SulpHur.Utilities.ExtensionMethods.FromXElement<WebElementInfo>(xElement);
            List<WebElementInfo> infoList = new List<WebElementInfo>();

            infoList.Add(pageInfo);
            AddChild(pageInfo, infoList);
            return infoList;
        }
        private bool IsDialogBox(ElementInformation ei)
        {
            return ei.ClassName == "#32770" ? true : false;
        }
        public List<ElementInformation> ParseTreeToList(System.Xml.Linq.XElement xElement)
        {
            ElementInformation eiRoot = MS.Internal.SulpHur.Utilities.ExtensionMethods.FromXElement<ElementInformation>(xElement);
            eiRoot.treeLevel = 0;

            List<ElementInformation> infoList = new List<ElementInformation>();
            infoList.Add(eiRoot);

            AddChild(eiRoot, infoList);

            foreach (ElementInformation ei in infoList)
            {
                ei.Descendants = new List<ElementInformation>();
                ei.Ancestors = new List<ElementInformation>();
            }

            foreach (ElementInformation ei in infoList)
            {
                AddDescents(ei, ei.Descendants);
                AddAncestors(ei, ei.Ancestors);
            }
            return infoList;
        }

        public List<ElementInformation> ParseTreeToList(ElementInformation eiRoot)
        {
            List<ElementInformation> infoList = new List<ElementInformation>();
            infoList.Add(eiRoot);

            AddChild(eiRoot, infoList);

            foreach (ElementInformation ei in infoList)
            {
                AddDescents(ei, ei.Descendants);
                AddAncestors(ei, ei.Ancestors);
            }
            return infoList;
        }

        private void AddChild(WebElementInfo webEi, List<WebElementInfo> infoList)
        {
            for (int i = 0; i < webEi.Children.Count; i++)
            {
                WebElementInfo temp = webEi.Children[i];
                temp.Parent = webEi;
                infoList.Add(temp);
                AddChild(temp, infoList);
            }
        }

        private void AddChild(ElementInformation ei, List<ElementInformation> infoList)
        {
            if (ei.Children == null) return;
            if (ei.Children.Count > 0)
            {
                ei.FirstChild = ei.Children.First();
                ei.LastChild = ei.Children.Last();
            }

            for (int i = 0; i < ei.Children.Count; i++)
            {
                ElementInformation temp = ei.Children[i];
                temp.Parent = ei;
                if (temp.NativeWindowHandle != 0 || temp.ControlType == ControlType.Hyperlink)
                {
                    //This attribute not used current time
                    //temp.treeLevel = ei.treeLevel + 1;
                    AddSibling(i, temp, ei.Children);
                    infoList.Add(temp);
                    AddChild(temp, infoList);
                }
            }
        }

        internal void AddDescents(ElementInformation ei, List<ElementInformation> descents)
        {
            foreach (ElementInformation temp in ei.Children)
            {
                descents.Add(temp);
                AddDescents(temp, descents);
            }
        }

        internal void AddAncestors(ElementInformation ei, List<ElementInformation> ancestors)
        {
            if (ei.Parent == null) return;
            else
            {
                ancestors.Add(ei.Parent);
                AddAncestors(ei.Parent, ancestors);
            }
        }

        internal void AddSibling(int current, ElementInformation currentEi, List<ElementInformation> siblingCollection)
        {
            for (int i = 0; i < siblingCollection.Count; i++)
            {
                if (i != current)
                {
                    if (currentEi.Siblings == null) currentEi.Siblings = new List<ElementInformation>();
                    currentEi.Siblings.Add(siblingCollection[i]);
                }

                if (i == current - 1)
                {
                    if (currentEi.PreviousSibling == null) currentEi.PreviousSibling = new ElementInformation();
                    currentEi.PreviousSibling = siblingCollection[i];
                }

                if (i == current + 1)
                {
                    if (currentEi.NextSibling == null) currentEi.NextSibling = new ElementInformation();
                    currentEi.NextSibling = siblingCollection[i];
                }
            }
        }

        public void RescanByBuildNo(string buildNO)
        {
            try
            {
                int scanedCount = 0;
                using (DBOperator db = new DBOperator())
                {
                    var resultList = db.QuerybyBuildNo(buildNO);
                    lock (obj)
                    {
                        foreach (int ur in resultList)
                        {
                            foreach (UIComplianceRuleBase b in RuleManager.RuleList)
                            {
                                if (b.IsEnabled)
                                {
                                    db.InsertScanedUI(ur, b.Name);
                                    scanedCount++;
                                }
                            }
                        }
                    }
                }
                Log.WriteServerLog(string.Format("{0} UI will be rescaned, buildno:{1}", scanedCount, buildNO), TraceLevel.Info);
            }
            catch (Exception e)
            {
                Log.WriteServerLog(e.Message, TraceLevel.Error);
            }
        }

        public void RescanByBuildNo(string buildNO, List<string> rules)
        {
            int scanedCount = 0;
            using (DBOperator db = new DBOperator())
            {
                lock (obj)
                {
                    var list = db.QuerybyBuildNo(buildNO);
                    foreach (int ur in list)
                    {
                        foreach (string b in rules)
                        {
                            db.InsertScanedUI(ur, b);
                            scanedCount++;
                        }
                    }
                }
            }
            Log.WriteServerLog(string.Format("{0} UI will be rescaned, buildno:{1}", scanedCount, buildNO), TraceLevel.Info);
        }


        public void RescanByContentList(List<int> contentList, List<string> rules)
        {
            try
            {
                int scanedCount = 0;
                using (DBOperator db = new DBOperator())
                {
                    lock (obj)
                    {
                        foreach (int ur in contentList)
                        {
                            foreach (string b in rules)
                            {
                                db.InsertScanedUI(ur, b);
                                scanedCount++;
                            }
                        }
                    }
                }
                Log.WriteServerLog(string.Format("{0} UI will be rescaned", scanedCount), TraceLevel.Info);
            }
            catch (Exception e)
            {
                Log.WriteServerLog(e.Message, TraceLevel.Error);
            }
        }
    }


    public interface VerifyOperations
    {
        void RescanByBuildNo(string buildNO, List<string> rules);
        void RescanByBuildNo(string buildNO);
        void RescanByContentList(List<int> contentList, List<string> rules);
        List<ElementInformation> ParseTreeToList(ElementInformation eiRoot);
        Bitmap DrawBitmap(Bitmap bit, List<ElementInformation> relatedControls, ElementInformation root);
    }
}
