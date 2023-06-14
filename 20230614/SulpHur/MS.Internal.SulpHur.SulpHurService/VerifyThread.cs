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
using System.Data.SqlClient;
using System.Data;
using System.Drawing.Imaging;
using System.IO;
using System.Web.UI.WebControls.Expressions;

namespace MS.Internal.SulpHur.SulpHurService
{
    public class UIContentVerification : VerifyOperations
    {
        private Thread verifyThread;
        private Thread cacheFastTableThread;
        private Thread ClearTempResultImageThread;
        private bool serviceStarted = true;
        //private RuleManager ruleManager;
        private Queue<UIANDRule> dirtyUIQueue;
        private object obj = new object();
        string connStr;
        string[] aliases;
        bool isEmpty = false;
        bool isClearTmp_ResultImage = false;
        int index = 0;


        //int testClearHour = int.Parse(System.Configuration.ConfigurationManager.AppSettings["testClearHour"]);
        //int testClearMinute = int.Parse(System.Configuration.ConfigurationManager.AppSettings["testClearMinute"]);
        public UIContentVerification()
        {
            Log.WriteServerLog("Starting public UIContentVerification...", TraceLevel.Info);
            try
            {
                connStr = System.Configuration.ConfigurationManager.ConnectionStrings["ADOConn"].ToString();

            }
            catch (Exception ex)
            {
                Log.WriteServerLog(ex.Message, TraceLevel.Info);
                Log.WriteServerLog(ex.ToString(), TraceLevel.Info);
            }
           
            verifyThread = new Thread(DoVerify);           
            dirtyUIQueue = new Queue<UIANDRule>();
        }

        public void Start()
        {
            serviceStarted = true;
            verifyThread.Start();
            //cacheFastTableThread.Start();
            //  ClearTempResultImageThread.Start();
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
                                Log.WriteServerLog("dirtyUI Enqueue");
                                dirtyUIQueue.Enqueue(temp);
                            }
                        }

                        while (dirtyUIQueue.Count > 0)
                        {
                            Log.WriteServerLog("dirtyUIQueue.count = " + dirtyUIQueue.Count);
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
                                    // results.Add(new UIComplianceResultBase(ResultType.Pass,", 
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
                                    if (b.Image != null)
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


        public void TimingCacheResultInfo()
        {
            while (true)
            {
                Trace.WriteLine("enter TimingCacheResultInfo");
                Trace.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                string currentDate = DateTime.Now.AddHours(15).ToString();
                string beforeCurrentDate = DateTime.Now.AddDays(-120).ToString();
                string[] resultIDs = null;
                DateTime localTime = DateTime.Now;
                TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
                DateTime beijingTime = TimeZoneInfo.ConvertTime(localTime, cstZone);

                string[] insertSqls = new string[10];
                insertSqls[0] = @"INSERT INTO ResultsInfoByAlias (ResultID, buildNo, Language, RuleName, ResultType, UIName, UserName, OSType, DateUploaded, CreateDate, ReviewFlag, alias) SELECT ResultID, buildNo, Language, RuleName, ResultType, UIName, UserName, OSType, DateUploaded, CreateDate, ReviewFlag,'" + aliases[0] + @"' as alias 
FROM (
 SELECT ROW_NUMBER() OVER (ORDER BY [TOTAL].[BuildNo] DESC, [TOTAL].[ResultID] DESC) AS rowNum, [TOTAL].[ResultID], [TOTAL].[BuildNo], 
 [TOTAL].[Language], [TOTAL].[RuleName], [TOTAL].[ResultType], [TOTAL].[UIName], [TOTAL].[UserName], [TOTAL].[OSType], [TOTAL].[DateUploaded], 
 [TOTAL].[CreateDate], [TOTAL].[ReviewFlag] FROM (
 SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [Rules].[RuleName], [UIContents].[UIName], [Clients].[UserName], 
 [UIContents].[DateUploaded], [Results].[ResultType], [Results].[CreateDate], [Results].[ReviewFlag], [BuildInfo].[Language], [Clients].[OSType]
 FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 WHERE 1=1  AND [BuildInfo].[Language] IN ('CHS','CHT','CSY','DEU','JPN') AND [Results].[RuleID] IN (36) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"' AND [Results].[ResultType] IN ('Fail') AND [Results].[ReviewFlag]=0) AS [TOTAL]
 INNER JOIN ( 
 SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType] FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 WHERE 1=1  AND [BuildInfo].[Language] IN ('CHS','CHT','CSY','DEU','JPN') AND [Results].[RuleID] IN (36) AND [CreateDate]> '" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"'
 GROUP BY [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType]) AS [GroupTable]
 ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
 AND [TOTAL].[UIName]=[GroupTable].[UIName]
 AND [TOTAL].[OSType]=[GroupTable].[OSType]
 AND [TOTAL].[Language]=[GroupTable].[Language]) AS newT  ORDER BY rowNum";
                insertSqls[1] = @"INSERT INTO ResultsInfoByAlias (ResultID, buildNo, Language, RuleName, ResultType, UIName, UserName, OSType, DateUploaded, CreateDate, ReviewFlag, alias) SELECT ResultID, buildNo, Language, RuleName, ResultType, UIName, UserName, OSType, DateUploaded, CreateDate, ReviewFlag,'" + aliases[1] + @"' as alias  FROM (
 SELECT ROW_NUMBER() OVER (ORDER BY [TOTAL].[BuildNo] DESC, [TOTAL].[ResultID] DESC) AS rowNum, [TOTAL].[ResultID], [TOTAL].[BuildNo], 
 [TOTAL].[Language], [TOTAL].[RuleName], [TOTAL].[ResultType], [TOTAL].[UIName], [TOTAL].[UserName], [TOTAL].[OSType], [TOTAL].[DateUploaded], 
 [TOTAL].[CreateDate], [TOTAL].[ReviewFlag] FROM (
 SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [Rules].[RuleName], [UIContents].[UIName], [Clients].[UserName], 
 [UIContents].[DateUploaded], [Results].[ResultType], [Results].[CreateDate], [Results].[ReviewFlag], [BuildInfo].[Language], [Clients].[OSType]
 FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 WHERE 1=1  AND [BuildInfo].[Language] IN ('ENU') AND [Results].[RuleID] IN (36) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"' AND [Results].[ResultType] IN ('Fail') AND [Results].[ReviewFlag]=0) AS [TOTAL]
 INNER JOIN ( 
 SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType] FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 WHERE 1=1  AND [BuildInfo].[Language] IN ('ENU') AND [Results].[RuleID] IN (36) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"'
 GROUP BY [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType]) AS [GroupTable]
 ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
 AND [TOTAL].[UIName]=[GroupTable].[UIName]
 AND [TOTAL].[OSType]=[GroupTable].[OSType]
 AND [TOTAL].[Language]=[GroupTable].[Language]) AS newT  ORDER BY rowNum";
                insertSqls[2] = @"INSERT INTO ResultsInfoByAlias (ResultID, buildNo, Language, RuleName, ResultType, UIName, UserName, OSType, DateUploaded, CreateDate, ReviewFlag, alias) SELECT ResultID, buildNo, Language, RuleName, ResultType, UIName, UserName, OSType, DateUploaded, CreateDate, ReviewFlag,'" + aliases[2] + @"' as alias  FROM (
 SELECT ROW_NUMBER() OVER (ORDER BY [TOTAL].[BuildNo] DESC, [TOTAL].[ResultID] DESC) AS rowNum, [TOTAL].[ResultID], [TOTAL].[BuildNo], 
 [TOTAL].[Language], [TOTAL].[RuleName], [TOTAL].[ResultType], [TOTAL].[UIName], [TOTAL].[UserName], [TOTAL].[OSType], [TOTAL].[DateUploaded], 
 [TOTAL].[CreateDate], [TOTAL].[ReviewFlag] FROM (
 SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [Rules].[RuleName], [UIContents].[UIName], [Clients].[UserName], 
 [UIContents].[DateUploaded], [Results].[ResultType], [Results].[CreateDate], [Results].[ReviewFlag], [BuildInfo].[Language], [Clients].[OSType]
 FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 WHERE 1=1  AND [BuildInfo].[Language] IN ('ESN','FRA','HUN','ITA','KOR','NLD') AND [Results].[RuleID] IN (36) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"' AND [Results].[ResultType] IN ('Fail') AND [Results].[ReviewFlag]=0) AS [TOTAL]
 INNER JOIN ( 
 SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType] FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 WHERE 1=1  AND [BuildInfo].[Language] IN ('ESN','FRA','HUN','ITA','KOR','NLD') AND [Results].[RuleID] IN (36) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"'
 GROUP BY [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType]) AS [GroupTable]
 ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
 AND [TOTAL].[UIName]=[GroupTable].[UIName]
 AND [TOTAL].[OSType]=[GroupTable].[OSType]
 AND [TOTAL].[Language]=[GroupTable].[Language]) AS newT  ORDER BY rowNum";
                insertSqls[3] = @"INSERT INTO ResultsInfoByAlias (ResultID, buildNo, Language, RuleName, ResultType, UIName, UserName, OSType, DateUploaded, CreateDate, ReviewFlag, alias) SELECT ResultID, buildNo, Language, RuleName, ResultType, UIName, UserName, OSType, DateUploaded, CreateDate, ReviewFlag,'" + aliases[3] + @"' as alias  FROM (
 SELECT ROW_NUMBER() OVER (ORDER BY [TOTAL].[BuildNo] DESC, [TOTAL].[ResultID] DESC) AS rowNum, [TOTAL].[ResultID], [TOTAL].[BuildNo], 
 [TOTAL].[Language], [TOTAL].[RuleName], [TOTAL].[ResultType], [TOTAL].[UIName], [TOTAL].[UserName], [TOTAL].[OSType], [TOTAL].[DateUploaded], 
 [TOTAL].[CreateDate], [TOTAL].[ReviewFlag] FROM (
 SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [Rules].[RuleName], [UIContents].[UIName], [Clients].[UserName], 
 [UIContents].[DateUploaded], [Results].[ResultType], [Results].[CreateDate], [Results].[ReviewFlag], [BuildInfo].[Language], [Clients].[OSType]
 FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 WHERE 1=1  AND [BuildInfo].[Language] IN ('PLK','PTB','PTG','RUS','SVE','TRK') AND [Results].[RuleID] IN (36) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"' AND [Results].[ResultType] IN ('Fail') AND [Results].[ReviewFlag]=0) AS [TOTAL]
 INNER JOIN ( 
 SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType] FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 WHERE 1=1  AND [BuildInfo].[Language] IN ('PLK','PTB','PTG','RUS','SVE','TRK') AND [Results].[RuleID] IN (36) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"'
 GROUP BY [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType]) AS [GroupTable]
 ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
 AND [TOTAL].[UIName]=[GroupTable].[UIName]
 AND [TOTAL].[OSType]=[GroupTable].[OSType]
 AND [TOTAL].[Language]=[GroupTable].[Language]) AS newT  ORDER BY rowNum";
                insertSqls[4] = @"INSERT INTO ResultsInfoByAlias (ResultID, buildNo, Language, RuleName, ResultType, UIName, UserName, OSType, DateUploaded, CreateDate, ReviewFlag, alias) SELECT ResultID, buildNo, Language, RuleName, ResultType, UIName, UserName, OSType, DateUploaded, CreateDate, ReviewFlag,'" + aliases[4] + @"' as alias  FROM (
 SELECT ROW_NUMBER() OVER (ORDER BY [TOTAL].[BuildNo] DESC, [TOTAL].[ResultID] DESC) AS rowNum, [TOTAL].[ResultID], [TOTAL].[BuildNo], 
 [TOTAL].[Language], [TOTAL].[RuleName], [TOTAL].[ResultType], [TOTAL].[UIName], [TOTAL].[UserName], [TOTAL].[OSType], [TOTAL].[DateUploaded], 
 [TOTAL].[CreateDate], [TOTAL].[ReviewFlag] FROM (
 SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [Rules].[RuleName], [UIContents].[UIName], [Clients].[UserName], 
 [UIContents].[DateUploaded], [Results].[ResultType], [Results].[CreateDate], [Results].[ReviewFlag], [BuildInfo].[Language], [Clients].[OSType]
 FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 WHERE 1=1  AND [BuildInfo].[Language] IN ('CHS') AND [Results].[RuleID] IN (31) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"' AND [Results].[ResultType] IN ('Fail') AND [Results].[ReviewFlag]=0) AS [TOTAL]
 INNER JOIN ( 
 SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType] FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 WHERE 1=1  AND [BuildInfo].[Language] IN ('CHS') AND [Results].[RuleID] IN (31) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"'
 GROUP BY [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType]) AS [GroupTable]
 ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
 AND [TOTAL].[UIName]=[GroupTable].[UIName]
 AND [TOTAL].[OSType]=[GroupTable].[OSType]
 AND [TOTAL].[Language]=[GroupTable].[Language]) AS newT  ORDER BY rowNum";
                insertSqls[5] = @"INSERT INTO ResultsInfoByAlias (ResultID, buildNo, Language, RuleName, ResultType, UIName, UserName, OSType, DateUploaded, CreateDate, ReviewFlag, alias) SELECT ResultID, buildNo, Language, RuleName, ResultType, UIName, UserName, OSType, DateUploaded, CreateDate, ReviewFlag,'" + aliases[5] + @"' as alias  FROM (
 SELECT ROW_NUMBER() OVER (ORDER BY [TOTAL].[BuildNo] DESC, [TOTAL].[ResultID] DESC) AS rowNum, [TOTAL].[ResultID], [TOTAL].[BuildNo], 
 [TOTAL].[Language], [TOTAL].[RuleName], [TOTAL].[ResultType], [TOTAL].[UIName], [TOTAL].[UserName], [TOTAL].[OSType], [TOTAL].[DateUploaded], 
 [TOTAL].[CreateDate], [TOTAL].[ReviewFlag] FROM (
 SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [Rules].[RuleName], [UIContents].[UIName], [Clients].[UserName], 
 [UIContents].[DateUploaded], [Results].[ResultType], [Results].[CreateDate], [Results].[ReviewFlag], [BuildInfo].[Language], [Clients].[OSType]
 FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 WHERE 1=1  AND [BuildInfo].[Language] IN ('CHT','CSY','DAN','DEU','ELL','ESN','FIN','FRA','HUN','ITA','JPN','KOR','NLD','NOR','PLK','PTB','PTG','RUS','SVE','TRK') AND [Results].[RuleID] IN (31) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"' AND [Results].[ResultType] IN ('Fail') AND [Results].[ReviewFlag]=0) AS [TOTAL]
 INNER JOIN ( 
 SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType] FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 WHERE 1=1  AND [BuildInfo].[Language] IN ('CHT','CSY','DAN','DEU','ELL','ESN','FIN','FRA','HUN','ITA','JPN','KOR','NLD','NOR','PLK','PTB','PTG','RUS','SVE','TRK') AND [Results].[RuleID] IN (31) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"'
 GROUP BY [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType]) AS [GroupTable]
 ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
 AND [TOTAL].[UIName]=[GroupTable].[UIName]
 AND [TOTAL].[OSType]=[GroupTable].[OSType]
 AND [TOTAL].[Language]=[GroupTable].[Language]) AS newT ORDER BY rowNum";
                insertSqls[6] = @"INSERT INTO ResultsInfoByAlias (ResultID, buildNo, Language, RuleName, ResultType, UIName, UserName, OSType, DateUploaded, CreateDate, ReviewFlag, alias) SELECT ResultID, buildNo, Language, RuleName, ResultType, UIName, UserName, OSType, DateUploaded, CreateDate, ReviewFlag,'" + aliases[6] + @"' as alias  FROM (
 SELECT ROW_NUMBER() OVER (ORDER BY [TOTAL].[BuildNo] DESC, [TOTAL].[ResultID] DESC) AS rowNum, [TOTAL].[ResultID], [TOTAL].[BuildNo], 
 [TOTAL].[Language], [TOTAL].[RuleName], [TOTAL].[ResultType], [TOTAL].[UIName], [TOTAL].[UserName], [TOTAL].[OSType], [TOTAL].[DateUploaded], 
 [TOTAL].[CreateDate], [TOTAL].[ReviewFlag] FROM (
 SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [Rules].[RuleName], [UIContents].[UIName], [Clients].[UserName], 
 [UIContents].[DateUploaded], [Results].[ResultType], [Results].[CreateDate], [Results].[ReviewFlag], [BuildInfo].[Language], [Clients].[OSType]
 FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 WHERE 1=1  AND [BuildInfo].[Language] IN ('ENU') AND [Results].[RuleID] IN (31) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"' AND [Results].[ResultType] IN ('Fail') AND [Results].[ReviewFlag]=0) AS [TOTAL]
 INNER JOIN ( 
 SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType] FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 WHERE 1=1  AND [BuildInfo].[Language] IN ('ENU') AND [Results].[RuleID] IN (31) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"'
 GROUP BY [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType]) AS [GroupTable]
 ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
 AND [TOTAL].[UIName]=[GroupTable].[UIName]
 AND [TOTAL].[OSType]=[GroupTable].[OSType]
 AND [TOTAL].[Language]=[GroupTable].[Language]) AS newT  ORDER BY rowNum";
                insertSqls[7] = @"INSERT INTO ResultsInfoByAlias (ResultID, buildNo, Language, RuleName, ResultType, UIName, UserName, OSType, DateUploaded, CreateDate, ReviewFlag, alias) SELECT ResultID, buildNo, Language, RuleName, ResultType, UIName, UserName, OSType, DateUploaded, CreateDate, ReviewFlag,'" + aliases[7] + @"' as alias  FROM (
 SELECT ROW_NUMBER() OVER (ORDER BY [TOTAL].[BuildNo] DESC, [TOTAL].[ResultID] DESC) AS rowNum, [TOTAL].[ResultID], [TOTAL].[BuildNo], 
 [TOTAL].[Language], [TOTAL].[RuleName], [TOTAL].[ResultType], [TOTAL].[UIName], [TOTAL].[UserName], [TOTAL].[OSType], [TOTAL].[DateUploaded], 
 [TOTAL].[CreateDate], [TOTAL].[ReviewFlag] FROM (
 SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [Rules].[RuleName], [UIContents].[UIName], [Clients].[UserName], 
 [UIContents].[DateUploaded], [Results].[ResultType], [Results].[CreateDate], [Results].[ReviewFlag], [BuildInfo].[Language], [Clients].[OSType]
 FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 WHERE 1=1  AND [BuildInfo].[Language] IN ('ENU') AND [Results].[RuleID] IN (39) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"' AND [Results].[ResultType] IN ('Fail') AND [Results].[ReviewFlag]=0) AS [TOTAL]
 INNER JOIN ( 
 SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName]
 FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 WHERE 1=1  AND [BuildInfo].[Language] IN ('ENU') AND [Results].[RuleID] IN (39) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"'
 GROUP BY [UIContents].[UIName]) AS [GroupTable]
 ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
 AND [TOTAL].[UIName]=[GroupTable].[UIName]) AS newT ORDER BY rowNum";
                insertSqls[8] = @"INSERT INTO ResultsInfoByAlias (ResultID, buildNo, Language, RuleName, ResultType, UIName, UserName, OSType, DateUploaded, CreateDate, ReviewFlag, alias) 
SELECT ResultID, buildNo, Language, RuleName, ResultType, UIName, UserName, OSType, DateUploaded, CreateDate, ReviewFlag,'" + aliases[8] + @"' as alias  FROM (
 SELECT ROW_NUMBER() OVER (ORDER BY [BuildNo] DESC, [ResultID] DESC) AS rowNum, * FROM (
 SELECT distinct [Results].[ResultID], [BuildInfo].[BuildNo], [BuildInfo].[Language], [Rules].[RuleName], [Results].[ResultType], 
 [UIContents].[UIName], [Clients].[UserName], [BuildInfo].[BuildID], [Clients].[OSType], [UIContents].[DateUploaded], [Results].[CreateDate], 
 [Results].[ReviewFlag] FROM [dbo].[Results] 
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID]
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID]
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID]
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID] 
 LEFT OUTER JOIN [AssemblyLink] ON [AssemblyLink].[ContentID]=[UIContents].[ContentID]
 LEFT OUTER JOIN [AssemblyInfo] ON [AssemblyInfo].[TypeID]=[AssemblyLink].[TypeID]
 WHERE 1=1 AND [Results].[RuleID] IN (41) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"' AND [Results].[ResultType] IN ('Fail') AND [Results].[ReviewFlag]=0) AS newT) AS t ORDER BY rowNum";
                insertSqls[9] = @"INSERT INTO ResultsInfoByAlias (ResultID, buildNo, Language, RuleName, ResultType, UIName, UserName, OSType, DateUploaded, CreateDate, ReviewFlag, alias)
SELECT ResultID, buildNo, Language, RuleName, ResultType, UIName, UserName, OSType, DateUploaded, CreateDate, ReviewFlag,'" + aliases[9] + @"' as alias  FROM (
 SELECT ROW_NUMBER() OVER (ORDER BY [BuildNo] DESC, [ResultID] DESC) AS rowNum, * FROM (
 SELECT distinct [Results].[ResultID], [BuildInfo].[BuildNo], [BuildInfo].[Language], [Rules].[RuleName], [Results].[ResultType], 
 [UIContents].[UIName], [Clients].[UserName], [BuildInfo].[BuildID], [Clients].[OSType], [UIContents].[DateUploaded], [Results].[CreateDate], 
 [Results].[ReviewFlag] FROM [dbo].[Results] 
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID]
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID]
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID]
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID] 
 LEFT OUTER JOIN [AssemblyLink] ON [AssemblyLink].[ContentID]=[UIContents].[ContentID]
 LEFT OUTER JOIN [AssemblyInfo] ON [AssemblyInfo].[TypeID]=[AssemblyLink].[TypeID]
 WHERE 1=1  AND [BuildInfo].[Language] IN ('ENU') AND [Results].[RuleID] IN (48) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"' AND [Results].[ResultType] IN ('Fail') AND [Results].[ReviewFlag]=0) AS newT) AS t  ORDER BY rowNum";
                bool isWithinSchedule = false;


                if (beijingTime.DayOfWeek == DayOfWeek.Friday && beijingTime.Hour == 4 && !isEmpty)
                {
                    Trace.WriteLine("start clear  ResultsInfoByAlias ");
                    Trace.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    TruncateCacheTable(connStr, "TRUNCATE TABLE ResultsInfoByAlias");
                    Trace.WriteLine("clear ResultsInfoByAlias complete");
                    isEmpty = true;
                }
                //else if (beijingTime.DayOfWeek == DayOfWeek.Tuesday && !isEmpty && (beijingTime.Hour == testClearHour && beijingTime.Minute <= testClearMinute))
                //{
                //    Trace.WriteLine("test clause start clear  ResultsInfoByAlias ");
                //    TruncateCacheTable(connStr, "TRUNCATE TABLE ResultsInfoByAlias");
                //    Trace.WriteLine("test clause clear ResultsInfoByAlias complete");
                //    isEmpty = true;
                //}


                //if (beijingTime.DayOfWeek == DayOfWeek.Thursday && beijingTime.Hour >= 23)
                //{
                //    Trace.WriteLine("time after 23:00 on Thursday, isEmpty = " + isEmpty.ToString());
                //    isWithinSchedule = true;
                //}
                //else
                if (beijingTime.DayOfWeek == DayOfWeek.Friday && beijingTime.Hour <= 12 && beijingTime.Hour >= 6)
                {
                    Trace.WriteLine("time in 6:00 - 12:00 on Friday, isEmpty = " + isEmpty.ToString());
                    isWithinSchedule = true;
                }
                //else if (beijingTime.DayOfWeek == DayOfWeek.Tuesday && beijingTime.Hour < (testClearHour + 5) && (beijingTime.Hour == testClearHour && beijingTime.Minute >= (testClearMinute + 1)))
                //{
                //    Trace.WriteLine("test cache clause, isEmpty = " + isEmpty.ToString());
                //    isWithinSchedule = true;
                //}



                /*-----------------------------------------------------------------------------------------------------------------*/

                //if (beijingTime.DayOfWeek == DayOfWeek.Monday && !isClearTmp_ResultImage)
                //{
                //    string folderPath = @"F:\v-mogenzhang\Deploy\SulpHurReport\Tmp_ResultImage";
                //    string[] files = Directory.GetFiles(folderPath);
                //    string clause = string.Empty;
                //    string[] readyToDelete;
                //    string fileName;
                //    string ResultID;
                //    foreach (string file in files)
                //    {
                //        fileName = Path.GetFileName(file);
                //        ResultID = fileName.Substring(0, fileName.IndexOf("."));
                //        clause += ResultID + ",";
                //    }
                //    clause = clause.TrimEnd(',');
                //    string getReviewedResultID = "select ResultID from Results where reviewflag =1 and resultID in (" + clause + ")";
                //    readyToDelete = ExecuteQuery(connStr, getReviewedResultID, "ResultID");
                //    foreach (string s in readyToDelete)
                //    {
                //        File.Delete(folderPath + '\\' + s + ".jpg");
                //    }
                //    isClearTmp_ResultImage = true;
                //}

                /*-----------------------------------------------------------------------------------------------------------------*/

                if (isWithinSchedule && isEmpty)
                {
                    Trace.WriteLine("The current time is within the specified time ");
                    for (index = 0; index < aliases.Length; index++)
                    {
                        Trace.WriteLine("start cache " + aliases[index] + " data");
                        try
                        {
                            ExecuteScalar(connStr, insertSqls[index]);
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine("index = " + index + "alias = " + aliases[index] + " ex.Message = " + ex.Message);
                        }
                        Trace.WriteLine("cache " + aliases[index] + " data complete");
                    }

                    resultIDs = ExecuteQuery(connStr, "select ResultID from ResultsInfoByAlias", "ResultID");
                    foreach (string resultID in resultIDs)
                    {
                        try
                        {
                            Trace.WriteLine("start cache image to F:\\v-mogenzhang\\Deploy\\SulpHurReport\\Tmp_ResultImage by resultid = " + resultID);
                            CacheScreenShot(int.Parse(resultID), "F:\\v-mogenzhang\\Deploy\\SulpHurReport\\");
                            Trace.WriteLine("cache image " + resultID + ".png complete");

                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine("cache to F:\\v-mogenzhang\\Deploy\\SulpHurReport\\Tmp_ResultImage " + resultID + ".png exception " + ex.Message);
                        }

                        try
                        {
                            Trace.WriteLine("start cache image to F:\\WebSites\\SulpHurReports\\Tmp_ResultImage by resultid = " + resultID);
                            CacheScreenShot(int.Parse(resultID), "F:\\WebSites\\SulpHurReports\\Tmp_ResultImage");
                            Trace.WriteLine("cache image " + resultID + ".png complete");
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine("cache to F:\\WebSites\\SulpHurReports\\Tmp_ResultImage " + resultID + ".png exception " + ex.Message);

                        }

                    }
                    isClearTmp_ResultImage = false;
                    isEmpty = false;
                }
                else if (!isWithinSchedule)
                {
                    Trace.WriteLine("The current time is not within the specified time range, sleep one hour");
                    System.Threading.Thread.Sleep(TimeSpan.FromHours(1));
                }
                else if (!isEmpty)
                {
                    Trace.WriteLine("The data table has been filled in isEmpty not is false, sleep one hour");
                    System.Threading.Thread.Sleep(TimeSpan.FromHours(1));
                }
            }
        }

        public void TimingClearTempResultImage()
        {
            Trace.WriteLine("In TimingClearTempResultImage but out while");
            while (true)
            {
                if (DateTime.Now.DayOfWeek == DayOfWeek.Tuesday)
                {
                    Trace.WriteLine("DayOfWeek is Tuesday, enter TimingClearTempResultImage if");
                    if (!isClearTmp_ResultImage)
                    {
                        Trace.WriteLine("isClearTmp_ResultImage is " + isClearTmp_ResultImage.ToString());
                        string captureUIsReportFolderPath = @"F:\v-mogenzhang\Deploy\SulpHurReport\Tmp_ResultImage";
                        string oldSulphurReport = @"F:\WebSites\SulpHurReports\Tmp_ResultImage";
                        //string[] files = Directory.GetFiles(folderPath);
                        //string clause = string.Empty;
                        //string[] readyToDelete;
                        //string fileName;
                        //string ResultID;
                        //foreach (string file in files)
                        //{
                        //    fileName = Path.GetFileName(file);
                        //    ResultID = fileName.Substring(0, fileName.IndexOf("."));
                        //    clause += ResultID + ",";
                        //}
                        //clause = clause.TrimEnd(',');
                        //string getReviewedResultID = "select ResultID from Results where reviewflag =1 and resultID in (" + clause + ")";
                        //readyToDelete = ExecuteQuery(connStr, getReviewedResultID, "ResultID");
                        //foreach (string s in readyToDelete)
                        //{
                        //    Trace.WriteLine("delete " + folderPath + '\\' + s + ".jpg");
                        //    File.Delete(folderPath + '\\' + s + ".jpg");
                        //}
                        ClearTempResultImage(captureUIsReportFolderPath);
                        ClearTempResultImage(oldSulphurReport);

                        isClearTmp_ResultImage = true;
                    }
                    else
                    {
                        Trace.WriteLine("isClearTmp_ResultImage is " + isClearTmp_ResultImage.ToString() + " thread sleep 1 day");
                        Thread.Sleep(TimeSpan.FromDays(1));
                    }
                }
                else
                {
                    Trace.WriteLine("Today is not Tuesday, isClearTmp_ResultImage is " + isClearTmp_ResultImage.ToString() + " thread sleep 1 day");
                    Thread.Sleep(TimeSpan.FromDays(1));
                }
            }
        }

        public void ClearTempResultImage(string folderPath)
        {
            string[] files = Directory.GetFiles(folderPath);
            string clause = string.Empty;
            string[] readyToDelete;
            string fileName;
            string ResultID;
            foreach (string file in files)
            {
                fileName = Path.GetFileName(file);
                ResultID = fileName.Substring(0, fileName.IndexOf("."));
                clause += ResultID + ",";
            }
            clause = clause.TrimEnd(',');
            string getReviewedResultID = "select ResultID from Results where reviewflag =1 and resultID in (" + clause + ")";
            readyToDelete = ExecuteQuery(connStr, getReviewedResultID, "ResultID");
            foreach (string s in readyToDelete)
            {
                Trace.WriteLine("delete " + folderPath + '\\' + s + ".jpg");
                File.Delete(folderPath + '\\' + s + ".jpg");
            }
        }

        public static string[] ExecuteQuery(string connStr, string sql, string columnName)
        {
            Log.WriteServerLog(sql + "   " + columnName, TraceLevel.Info);
            Trace.WriteLine(sql + "   " + columnName);
            List<string> resultList = new List<string>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    SqlCommand command = new SqlCommand(sql, conn);
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 30000;
                    conn.Open();

                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string columnValue = reader[columnName].ToString();
                        resultList.Add(columnValue);
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                Log.WriteServerLog(ex.Message, TraceLevel.Info);
                Log.WriteServerLog(ex.ToString(), TraceLevel.Info);
            }
            finally
            {
                Log.WriteServerLog("FINALLY", TraceLevel.Info);
            }
            Log.WriteServerLog("in ExecuteQuery end", TraceLevel.Info);
            return resultList.ToArray();
        }

        public static void TruncateCacheTable(string connStr, string truncateSql)
        {
            Trace.WriteLine(truncateSql);
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = conn;
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 300000;
                    command.CommandText = truncateSql;
                    command.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        public static void ExecuteScalar(string connStr, string sql)
        {
            Trace.WriteLine($"execute {sql}");
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (SqlCommand command = new SqlCommand(sql, conn))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 300000;
                    command.ExecuteNonQuery();
                }
                conn.Close();
            }
        }


        public void CacheScreenShot(int resultID, string projectPath)
        {
            string imagePath = string.Format(@"Tmp_ResultImage/{0}.jpg", resultID);
            Trace.WriteLine("imagePath = " + imagePath);
            string localImagePath = Path.Combine(projectPath, imagePath);
            Trace.WriteLine("localImagePath = " + localImagePath);
            if (!File.Exists(localImagePath))
            {
                byte[] resultImage = GetResultScreenshot(resultID);
                System.Drawing.Image image = null;
                if (resultImage == null)
                {
                    resultImage = GetUIScreenshot(resultID);
                    if (resultImage != null)
                        image = Bitmap.FromStream(new MemoryStream(resultImage));
                }
                else
                    image = Bitmap.FromStream(new MemoryStream(resultImage));
                image.Save(localImagePath, ImageFormat.Jpeg);
            }
        }

        public static byte[] GetResultScreenshot(int resultID)
        {
            Trace.WriteLine("GetResultScreenshot(" + resultID + ")");
            try
            {
                LinqDataClassesDataContext entities = new LinqDataClassesDataContext();
                var item = from r in entities.Results
                           where r.ResultID == resultID
                           select r.ResultImage;
                //byte[] resultImage = item.FirstOrDefault();
                byte[] resultImage = item.FirstOrDefault()?.ToArray();
                return resultImage;
            }
            catch (MappingException)
            {
                return GetResultScreenshot(resultID);
            }
        }

        public static Byte[] GetUIScreenshot(int resultID)
        {
            Trace.WriteLine("GetUIScreenshot(" + resultID + ")");
            LinqDataClassesDataContext entities = new LinqDataClassesDataContext();
            var list = from r in entities.Results
                       join u in entities.UIContents on r.ContentID equals u.ContentID
                       where r.ResultID == resultID
                       select u.UIScreenShot;
            Byte[] uiScreenshot = list.FirstOrDefault()?.ToArray();
            return uiScreenshot;
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
