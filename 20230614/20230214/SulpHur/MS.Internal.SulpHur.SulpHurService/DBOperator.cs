using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.UICompliance;
using System.Xml;
using System.Xml.Linq;
using System.Drawing;
using MS.Internal.SulpHur.SulpHurService.DataAccess;
using System.Data.Linq;
using SulpHurServiceAbstract;
using System.IO;
using System.Text.RegularExpressions;
using MS.Internal.SulpHur.Utilities;

namespace MS.Internal.SulpHur.SulpHurService
{
    public class DBOperator : QueryOperations, IDisposable
    {
        LinqDataClassesDataContext dataContext = new LinqDataClassesDataContext();

        public static LinqDataClassesDataContext dataContextStatic = new LinqDataClassesDataContext();
        public static string mainRootFolderName = System.Configuration.ConfigurationManager.AppSettings["buildMainRootFolerName"];
        public static string releaseRootFolderName = System.Configuration.ConfigurationManager.AppSettings["buildReleaseRootFolerName"];
        public static string adminConsoleLCGFileRelatedPath = System.Configuration.ConfigurationManager.AppSettings["adminConsoleLCGFileRelatedPath"];

        public DBOperator()
        {
            dataContext = new LinqDataClassesDataContext();
        }

        /// <summary>
        /// change buildNo to old format
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static string ChangeBuildNo(string buildNo)
        {

            string productbuild = string.Empty;

            if (IsStandardFormat(buildNo))
            {
                Log.WriteServerLog("format IsStandardFormat", TraceLevel.Info);
                productbuild = buildNo;
            }
            else
            {
                //search new-standard build version relation table
                BuildVersionMap buildVersionMap = dataContextStatic.BuildVersionMaps.FirstOrDefault(bvm => bvm.NewFormatBuildVersion == buildNo);
                if (buildVersionMap == null)
                {
                    Log.WriteServerLog("Cannot find standard build version from buildVersionMap for new build format: " + buildNo, TraceLevel.Info);
                    //search file to get standard build version
                    productbuild = FileUtility.ChangeNewBuildToOldFormat(buildNo);
                    if (string.IsNullOrEmpty(productbuild))
                    {
                        Log.WriteServerLog("Cannot find old build version from new build format: " + buildNo, TraceLevel.Info);
                        productbuild = buildNo;
                    }
                    else
                    {
                        Log.WriteServerLog("read file result,productbuild:" + productbuild, TraceLevel.Info);
                        //insert standard-new build version relation table
                        int newIndex = dataContextStatic.BuildVersionMaps.Max(bvm => bvm.BuildVersionMapID) + 1;

                        BuildVersionMap tempBuildVersionMap = new BuildVersionMap()
                        { BuildVersionMapID = newIndex, NewFormatBuildVersion = buildNo, StandardFormatBuildVersion = productbuild };

                        dataContextStatic.BuildVersionMaps.InsertOnSubmit(tempBuildVersionMap);
                        dataContextStatic.SubmitChanges();
                    }
                }
                else
                {
                    productbuild = buildVersionMap.StandardFormatBuildVersion;
                }
            }

            return productbuild;
        }

        public void InsertUI(
            System.Xml.Linq.XElement elementTree,
            System.Data.Linq.Binary image,
            MS.Internal.SulpHur.UICompliance.AdditionInformations info,
            string UIName,
            List<MS.Internal.SulpHur.UICompliance.AssemblyInfo> assemblyInfoList,
            string launchedFrom,
            string windowHierarchy,
            bool isWebUI,
            out UploadResults re)
        {
            re = new UploadResults();

            //v-yiwzha bug 506859 [SulpHurTool] ShowTest Coverage broken for new build format
            string productbuild = string.Empty;
            //Log.WriteServerLog("ProductVersion.tostring() : " + info.ProductVersion);

            try
            {
                productbuild = ChangeBuildNo(info.ProductVersion.ToString());
            }
            catch (Exception ex)
            {
                re.Type = UploadResultType.Failed;
                re.Message = "insert BuildVersionMap failed.exception:" + ex.Message;
                return;
            }

            re.id = elementTree.ToString();
            string logMsg = "Start";
            try
            {
                logMsg = "Validate data";
                //validate data
                if (!Validation.NotNullOrEmpty(productbuild) ||
                    !IsStandardFormat(productbuild) ||
                    !Validation.NotNullOrEmpty(info.Alias) ||
                    !Validation.NotNullOrEmpty(info.MacAddress))
                {
                    re.Type = UploadResultType.Failed;
                    re.Message = "Invalid information.";
                    Log.WriteServerLog("[Abandon this UI]:Has invalid addition information", TraceLevel.Info);
                    return;
                }
                //miss existing UI
                //GUID
                CMGUIDGenerator generator = new CMGUIDGenerator();
                Guid currentGUID = generator.GenerateGUID(elementTree, info.ProductLanguage);
                logMsg = "Identifier exist or not";
                //AssemblyInfo
                MS.Internal.SulpHur.UICompliance.AssemblyInfo pageIdentifier = null;
                if (assemblyInfoList != null)
                {
                    pageIdentifier = assemblyInfoList.FirstOrDefault(item => item.IsPageIdentifier == true);
                }
                if (pageIdentifier != null)
                {
                    if (dataContext.UIContents.Any(b => b.GUID.Equals(currentGUID) &&
                                                   b.BuildInfo.BuildNo.Equals(productbuild) &&
                                                   b.BuildInfo.Language.Equals(info.ProductLanguage) &&
                                                   b.Client.OSType.Equals(info.OSType) &&
                                                   b.Reserve2.Equals(info.BuildType) &&
                                                   b.AssemblyLinks.FirstOrDefault(al => al.IsPageIdentifier).AssemblyInfo.AssemblyName == pageIdentifier.AssemblyName &&
                                                   b.AssemblyLinks.FirstOrDefault(al => al.IsPageIdentifier).AssemblyInfo.FullTypeName == pageIdentifier.FullTypeName))
                    {
                        re.Type = UploadResultType.Exists;
                        re.Message = "No Message.";
                        Log.WriteServerLog("[Abandon this UI]:UI Exists, GUID:" + currentGUID, TraceLevel.Info);
                        return;
                    }
                    else
                    {
                        #region update assembly info
                        var uiContentWithoutAssemblyInfo = dataContext.UIContents.Where(b => b.GUID.Equals(currentGUID) &&
                                                                     b.BuildInfo.BuildNo.Equals(productbuild) &&
                                                                     b.BuildInfo.Language.Equals(info.ProductLanguage) &&
                                                                     b.Reserve2.Equals(info.BuildType) &&
                                                                     b.Client.OSType.Equals(info.OSType) &&
                                                                     b.AssemblyLinks.Count == 0)
                                                              .Select(u => new { u.ContentID, u.AssemblyLinks })
                                                              .FirstOrDefault();
                        //add assembly info to the UIContent exist without assembly info in DB
                        if (uiContentWithoutAssemblyInfo != null)
                        {
                            //AssemblyInfo
                            if (assemblyInfoList != null && assemblyInfoList.Count > 0)
                            {
                                foreach (MS.Internal.SulpHur.UICompliance.AssemblyInfo assembly in assemblyInfoList)
                                {
                                    string assemblyName = assembly.AssemblyName;
                                    string fullTypeName = assembly.FullTypeName;

                                    try
                                    {
                                        //AssemblyInfo
                                        if (!string.IsNullOrEmpty(assemblyName) &&
                                            !string.IsNullOrEmpty(fullTypeName) &&
                                            !dataContext.AssemblyInfos.Any(row => row.AssemblyName.Equals(assemblyName) && row.FullTypeName.Equals(fullTypeName)))
                                        {
                                            MS.Internal.SulpHur.SulpHurService.DataAccess.AssemblyInfo assemblyInfo = new MS.Internal.SulpHur.SulpHurService.DataAccess.AssemblyInfo();
                                            assemblyInfo.AssemblyName = assemblyName;
                                            assemblyInfo.FullTypeName = fullTypeName;
                                            //typeName
                                            string[] namespaceArr = fullTypeName.Split('.');
                                            assemblyInfo.TypeName = namespaceArr[namespaceArr.Length - 1];
                                            //insert
                                            dataContext.AssemblyInfos.InsertOnSubmit(assemblyInfo);
                                            dataContext.SubmitChanges();
                                        }

                                        //AssemblyLink
                                        AssemblyLink assemblyLink = new AssemblyLink();
                                        MS.Internal.SulpHur.SulpHurService.DataAccess.AssemblyInfo tmpAssemblyInfo = dataContext.AssemblyInfos.Single(row => row.AssemblyName.Equals(assemblyName) && row.FullTypeName.Equals(fullTypeName));
                                        assemblyLink.TypeID = tmpAssemblyInfo.TypeID;
                                        assemblyLink.ContentID = uiContentWithoutAssemblyInfo.ContentID;
                                        assemblyLink.IsPageIdentifier = assembly.IsPageIdentifier;
                                        //insert
                                        dataContext.AssemblyLinks.InsertOnSubmit(assemblyLink);
                                        dataContext.SubmitChanges();
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.WriteServerLog(string.Format("Exception when write assembly info: {0}. [AssemblyName: {1}, FullTypeName:{2}]", ex, assemblyName, fullTypeName), TraceLevel.Error);
                                    }
                                }
                            }

                            re.Type = UploadResultType.Exists;
                            re.Message = "No Message.";
                            Log.WriteServerLog("Add assembly info to exist UI.", TraceLevel.Verbose);
                            return;
                        }
                        #endregion
                    }
                }
                else
                {
                    if (dataContext.UIContents.Any(b => b.GUID.Equals(currentGUID) &&
                                                   b.BuildInfo.BuildNo.Equals(productbuild) &&
                                                   b.Reserve2.Equals(info.BuildType) &&
                                                   b.BuildInfo.Language.Equals(info.ProductLanguage) &&
                                                   b.Client.OSType.Equals(info.OSType)))
                    {
                        re.Type = UploadResultType.Exists;
                        re.Message = "No Message.";
                        Log.WriteServerLog("[Abandon this UI]:UI Exists.", TraceLevel.Info);
                        return;
                    }
                }

                logMsg = "Client info";
                //Clients
                //v-danpgu: add the oslanguage, otherwise if the same user change the oslanguage later it wouldn't be updated
                if (!dataContext.Clients.Any(u => u.UserName == info.Alias &&
                                             u.MacAddress == info.MacAddress &&
                                             u.OSType == info.OSType &&
                                         u.OSLanguage == info.OSLanguage))
                {
                    Client userTable = new Client();
                    userTable.UserName = info.Alias;
                    userTable.MacAddress = info.MacAddress;
                    userTable.IPAddress = info.IP.ToString();
                    userTable.MachineName = info.ComputerName;
                    userTable.OSType = info.OSType;
                    userTable.OSLanguage = info.OSLanguage;
                    userTable.DateCreated = DateTime.Now;
                    try
                    {
                        dataContext.Clients.InsertOnSubmit(userTable);
                        dataContext.SubmitChanges();
                    }
                    catch (Exception ex)
                    {
                        re.Type = UploadResultType.Failed;
                        re.Message = "Invalid information.";
                        Log.WriteServerLog(string.Format("UserName:{0}, MacAddress:{1}, IPAddress:{2}, MachineName:{3}, OSType:{4}, OSLanguage:{5}, DateCreated:{6}", userTable.UserName, userTable.MacAddress, userTable.IPAddress, userTable.MachineName, userTable.OSType, userTable.OSLanguage, userTable.DateCreated), TraceLevel.Warning);
                        Log.WriteServerLog(string.Format("insert client failed: {0}", ex), TraceLevel.Error);
                        return;
                    }
                }

                logMsg = "Build info";
                //BuildInfo
                if (!dataContext.BuildInfos.Any(b => b.BuildNo == productbuild && b.Language == info.ProductLanguage))
                {
                    BuildInfo buildTable = new BuildInfo();
                    buildTable.BuildNo = productbuild;
                    buildTable.Language = info.ProductLanguage.ToString();
                    buildTable.DateCreated = DateTime.Now;
                    try
                    {
                        Log.WriteServerLog("buildTable.buildno:" + buildTable.BuildNo + ",lan:" + buildTable.Language, TraceLevel.Info);
                        dataContext.BuildInfos.InsertOnSubmit(buildTable);
                        dataContext.SubmitChanges();
                    }
                    catch (Exception ex)
                    {
                        re.Type = UploadResultType.Failed;
                        re.Message = "insert build failed.";
                        Log.WriteServerLog(string.Format("BuildNo:{0}, Language:{1}, DateCreated:{2}", buildTable.BuildNo, buildTable.Language, buildTable.DateCreated), TraceLevel.Warning);
                        Log.WriteServerLog(string.Format("insert build failed: {0}", ex), TraceLevel.Error);
                        return;
                    }
                }

                logMsg = "UIContent info";
                UIContent contentTable = new UIContent();
                //UIContents
                //v-danpgu: for insert new record for same user but later change the os language
                try
                {
                    Client client = dataContext.Clients.First(u => u.UserName == info.Alias &&
                                                                             u.MacAddress == info.MacAddress &&
                                                                             u.OSType == info.OSType &&
                                                                             u.OSLanguage == info.OSLanguage);
                    BuildInfo build = dataContext.BuildInfos.First(b => b.BuildNo == productbuild && b.Language == info.ProductLanguage);
                    //AssemblyInfo assembly = dataContext.AssemblyInfos.SingleOrDefault(row => row.AssemblyName.Equals(assemblyName) && row.FullTypeName.Equals(fullTypeName));
                    //UIContent contentTable = new UIContent();
                    contentTable.UIContent1 = elementTree;
                    contentTable.GUID = currentGUID;
                    //elementTree.Save(System.IO.Path.Combine(ServiceEnviroment.LogFolder,"1.xml"));
                    contentTable.UIScreenShot = image;
                    contentTable.BuildID = build.BuildID;
                    //if (assembly != null)
                    //{
                    //    contentTable.TypeID = assembly.TypeID;
                    //}
                    contentTable.DateUploaded = DateTime.Now.ToUniversalTime();
                    contentTable.ClientID = client.ClientID;
                    contentTable.IsWebUI = isWebUI;
                    contentTable.UIName = UIName;
                    contentTable.LaunchedFrom = launchedFrom;
                    contentTable.WindowHierarchy = windowHierarchy;
                    contentTable.Reserve2 = info.BuildType;
                    Log.WriteServerLog("contentTable,guid:" + currentGUID + ",buildid:" + contentTable.BuildID, TraceLevel.Info);
                }
                catch (Exception ex)
                {
                    Log.WriteServerLog("contentTable prepare data failed:" + ex.Message, TraceLevel.Error);
                }

                try
                {
                    dataContext.UIContents.InsertOnSubmit(contentTable);
                    dataContext.SubmitChanges();
                }
                catch (Exception ex)
                {
                    re.Type = UploadResultType.Failed;
                    re.Message = "insert UIContent failed.";
                    Log.WriteServerLog(string.Format("GUID:{0}, BuildID:{1}, DateUploaded:{2}, UIName:{3}, LaunchedFrom:{4}, WindowHierarchy:{5}", contentTable.GUID, contentTable.BuildID, contentTable.DateUploaded, contentTable.UIName, contentTable.LaunchedFrom, contentTable.WindowHierarchy), TraceLevel.Error);
                    Log.WriteServerLog(string.Format("insert UIContent failed: {0}", ex), TraceLevel.Error);
                    return;
                }

                logMsg = "Assembly info";
                //AssemblyInfo
                if (assemblyInfoList != null && assemblyInfoList.Count > 0)
                {
                    foreach (MS.Internal.SulpHur.UICompliance.AssemblyInfo assembly in assemblyInfoList)
                    {
                        string assemblyName = assembly.AssemblyName;
                        string fullTypeName = assembly.FullTypeName;

                        try
                        {
                            //AssemblyInfo
                            if (!string.IsNullOrEmpty(assemblyName) &&
                                !string.IsNullOrEmpty(fullTypeName) &&
                                !dataContext.AssemblyInfos.Any(row => row.AssemblyName.Equals(assemblyName) && row.FullTypeName.Equals(fullTypeName)))
                            {
                                MS.Internal.SulpHur.SulpHurService.DataAccess.AssemblyInfo assemblyInfo = new MS.Internal.SulpHur.SulpHurService.DataAccess.AssemblyInfo();
                                assemblyInfo.AssemblyName = assemblyName;
                                assemblyInfo.FullTypeName = fullTypeName;
                                //typeName
                                string[] namespaceArr = fullTypeName.Split('.');
                                assemblyInfo.TypeName = namespaceArr[namespaceArr.Length - 1];
                                //insert
                                dataContext.AssemblyInfos.InsertOnSubmit(assemblyInfo);
                                dataContext.SubmitChanges();
                            }

                            //AssemblyLink
                            AssemblyLink assemblyLink = new AssemblyLink();
                            MS.Internal.SulpHur.SulpHurService.DataAccess.AssemblyInfo tmpAssemblyInfo = dataContext.AssemblyInfos.Single(row => row.AssemblyName.Equals(assemblyName) && row.FullTypeName.Equals(fullTypeName));
                            assemblyLink.TypeID = tmpAssemblyInfo.TypeID;
                            assemblyLink.ContentID = contentTable.ContentID;
                            assemblyLink.IsPageIdentifier = assembly.IsPageIdentifier;
                            //insert
                            dataContext.AssemblyLinks.InsertOnSubmit(assemblyLink);
                            dataContext.SubmitChanges();
                        }
                        catch (Exception ex)
                        {
                            Log.WriteServerLog(string.Format("Exception when write assembly info: {0}. [AssemblyName: {1}, FullTypeName:{2}]", ex, assemblyName, fullTypeName), TraceLevel.Error);
                        }
                    }
                }

                logMsg = "RuleStatus info";
                //RuleStatus
                List<RuleStatus> ruleStatusList = new List<RuleStatus>();
                var availabelRuleIDs = from r in dataContext.Rules
                                       where r.IsEnabled == true
                                       select r.RuleID;
                foreach (int id in availabelRuleIDs)
                {
                    RuleStatus tempScanStatus = new RuleStatus();
                    tempScanStatus.ContentID = contentTable.ContentID;
                    tempScanStatus.RuleID = id;
                    tempScanStatus.IsChecked = false;
                    tempScanStatus.DateUpdated = DateTime.Now;

                    ruleStatusList.Add(tempScanStatus);
                }
                dataContext.RuleStatus.InsertAllOnSubmit(ruleStatusList);
                dataContext.SubmitChanges();

                Log.WriteServerLog(string.Format("UI [ID:{0}] Inserted.", contentTable.ContentID), TraceLevel.Info);
                re.Type = UploadResultType.UpLoaded;
                re.Message = "Pending Verify";
            }
            catch (Exception e1)
            {
                re.Type = UploadResultType.Failed;
                re.Message = "No Message";
                Log.WriteServerLog(logMsg, TraceLevel.Warning);
                Log.WriteServerLog(e1.Message, TraceLevel.Error);
                return;
            }
        }

        public void UpdateRule(List<MS.Internal.SulpHur.UICompliance.UIComplianceRuleBase> ruleList)
        {
            try
            {
                Log.WriteServerLog("Start Update Rule in DB:", TraceLevel.Verbose);

                //Disable not available rules, enable available rules
                List<string> availableNames = ruleList.Select(r => r.Name).ToList();
                var availabelRule = from r in dataContext.Rules
                                        //where r.IsEnabled == true
                                    select r;
                foreach (var v in availabelRule)
                {
                    if (!availableNames.Contains(v.RuleName))
                    {
                        Log.WriteServerLog(string.Format("Rule [{0}] not found in assembly.", v.RuleName), TraceLevel.Verbose);
                        v.IsEnabled = false;
                    }
                    else
                    {
                        UIComplianceRuleBase rule = ruleList.SingleOrDefault(item => item.Name.Equals(v.RuleName));
                        if (rule != null)
                        {
                            Log.WriteServerLog(string.Format("{0}: {1}", rule.Name, rule.IsEnabled), TraceLevel.Verbose);
                            v.IsEnabled = rule.IsEnabled;
                        }
                    }
                }
                dataContext.SubmitChanges();

                //Insert New rule
                List<Rule> dbRules = new List<Rule>();
                foreach (UIComplianceRuleBase rule in ruleList)
                {
                    if (!dataContext.Rules.Any(r => r.RuleName == rule.Name))
                    {
                        Rule ruleTable = new Rule();
                        ruleTable.RuleName = rule.Name;
                        ruleTable.IsEnabled = rule.IsEnabled;
                        ruleTable.RuleDesc = rule.Description;
                        Log.WriteServerLog(string.Format("New Rule [{0}] added.", rule.Name, rule.IsEnabled), TraceLevel.Verbose);
                        ruleTable.DateCreated = DateTime.Now;
                        dbRules.Add(ruleTable);
                    }
                }
                dataContext.Rules.InsertAllOnSubmit(dbRules);
                dataContext.SubmitChanges();
            }
            catch (Exception error)
            {
                Log.WriteServerLog(string.Format("Fail to update rule in DB:{0}", error.ToString()), TraceLevel.Error);
            }
        }

        public void SetRuleStatus(bool state, string ruleName)
        {
            var availabelRule = from r in dataContext.Rules
                                where r.RuleName == ruleName
                                select r;

            foreach (var v in availabelRule)
            {
                v.IsEnabled = state;
            }

            dataContext.SubmitChanges();
        }

        public List<UIANDRule> QueryAllDirtyUI()
        {
            Log.WriteServerLog("Start QueryAllDirtyUI...");
            var varList = (from rs in dataContext.RuleStatus
                           where !rs.IsChecked && rs.Rule.IsEnabled
                           select new UIANDRule()
                           {
                               RuleName = rs.Rule.RuleName,
                               Uiid = rs.UIContent.ContentID
                           }).Take(10);
            List<UIANDRule> verifyList = new List<UIANDRule>();
            foreach (UIANDRule u in varList)
            {
                verifyList.Add(u);
            }
            Dictionary<int, UIContentMini> uiContentDisc = new Dictionary<int, UIContentMini>();
            for (int i = 0; i < verifyList.Count; i++)
            {
                if (!uiContentDisc.ContainsKey(verifyList[i].Uiid))
                {
                    var uiContents = from r in dataContext.UIContents
                                     where r.ContentID == verifyList[i].Uiid
                                     select r;
                    UIContent uiContent = uiContents.FirstOrDefault();
                    //Some binary cannot be converted into bitmap throw below method.
                    //Need more investgate here...
                    Bitmap bitmap = MS.Internal.SulpHur.UICompliance.Utility.BytesToBmp_MemStream(uiContent.UIScreenShot);

                    UIContentMini temp = new UIContentMini()
                    {
                        UiIsWebUI = uiContent.IsWebUI,
                        UiXmlElement = uiContent.UIContent1,
                        UiBitmap = bitmap,
                    };
                    uiContentDisc.Add(uiContent.ContentID, temp);
                }
                verifyList[i].IsWebUI = uiContentDisc[verifyList[i].Uiid].UiIsWebUI;
                verifyList[i].Element = new XElement(uiContentDisc[verifyList[i].Uiid].UiXmlElement);
                verifyList[i].Bitmap = new Bitmap(uiContentDisc[verifyList[i].Uiid].UiBitmap);
            }
            foreach (int i in uiContentDisc.Keys)
            {
                uiContentDisc[i].UiXmlElement = null;
                uiContentDisc[i].UiBitmap.Dispose();
                uiContentDisc[i].UiBitmap = null;
            }
            return verifyList;
        }
        //v-yiwzha Bug:443134 Add function to identity the UIANDRule reviewed or not
        public bool IsUIANDRuleReviewed(XElement ele, string ruleName, Bitmap bitmap, bool iswebui)
        {
            var reviewedList = (from r in dataContext.Results
                                where r.ReviewFlag == 1
                                select new UIANDRule(r.UIContent.UIContent1, r.Rule.RuleName, r.UIContent.IsWebUI, MS.Internal.SulpHur.UICompliance.Utility.BytesToBmp_MemStream(r.UIContent.UIScreenShot), r.UIContent.ContentID)).Take(10);
            foreach (UIANDRule t in reviewedList)
            {
                if (t.Element == ele && t.RuleName == ruleName && t.Bitmap == bitmap && t.IsWebUI == iswebui) return true;
            }
            return false;

        }
        //v-yiwzha bug489464 Get controls name list from content elements
        public List<string> GetNameFromElementList(List<ElementInformation> eilist)
        {
            List<string> nameList = new List<string>();
            for (int i = 0; i < eilist.Count; i++)
            {
                nameList.Add(eilist[i].Name);
            }
            return nameList;
        }
        //v-yiwzha bug489464 Get elements into list which have same build info, assembly and fulltype
        public List<XElement> GetHistoryElementsListForContent(int contentID)
        {
            var contentList = from c in dataContext.UIContents
                              where c.ContentID == contentID
                              select new BuildANDAssembly(c.BuildInfo.BuildNo, c.BuildInfo.Language, c.AssemblyLinks.FirstOrDefault(al => al.IsPageIdentifier).AssemblyInfo.AssemblyName, c.AssemblyLinks.FirstOrDefault(al => al.IsPageIdentifier).AssemblyInfo.FullTypeName, c.UIContent1, c.Client.OSType);
            BuildANDAssembly t = contentList.FirstOrDefault();
            var varList = from c in dataContext.UIContents
                          where c.BuildInfo.BuildNo == t.BuildNo && c.BuildInfo.Language == t.BuildLanguage && c.AssemblyLinks.FirstOrDefault(al => al.IsPageIdentifier).AssemblyInfo.AssemblyName == t.Assembly && c.AssemblyLinks.FirstOrDefault(al => al.IsPageIdentifier).AssemblyInfo.FullTypeName == t.FullTypeName && c.ContentID < contentID && c.Client.OSType != t.OsType
                          select c.UIContent1;
            var idList = from c in dataContext.UIContents
                         where c.BuildInfo.BuildNo == t.BuildNo && c.BuildInfo.Language == t.BuildLanguage && c.AssemblyLinks.FirstOrDefault(al => al.IsPageIdentifier).AssemblyInfo.AssemblyName == t.Assembly && c.AssemblyLinks.FirstOrDefault(al => al.IsPageIdentifier).AssemblyInfo.FullTypeName == t.FullTypeName && c.ContentID < contentID && c.Client.OSType != t.OsType
                         select c.ContentID;
            var BAList = from c in dataContext.UIContents
                         where c.BuildInfo.BuildNo == t.BuildNo && c.BuildInfo.Language == t.BuildLanguage && c.AssemblyLinks.FirstOrDefault(al => al.IsPageIdentifier).AssemblyInfo.AssemblyName == t.Assembly && c.AssemblyLinks.FirstOrDefault(al => al.IsPageIdentifier).AssemblyInfo.FullTypeName == t.FullTypeName && c.ContentID < contentID && c.Client.OSType != t.OsType
                         select new BuildANDAssembly(c.BuildInfo.BuildNo, c.BuildInfo.Language, c.AssemblyLinks.FirstOrDefault(al => al.IsPageIdentifier).AssemblyInfo.AssemblyName, c.AssemblyLinks.FirstOrDefault(al => al.IsPageIdentifier).AssemblyInfo.FullTypeName, c.UIContent1, c.Client.OSType);
            if (idList.FirstOrDefault() != 0)
            {
                Log.WriteServerLog("Test " + contentID + " is verified in content " + idList.FirstOrDefault(), TraceLevel.Info);
                BuildANDAssembly temp = BAList.FirstOrDefault();
                Log.WriteServerLog("Content:" + contentID + " build is " + t.BuildNo + " language is " + t.BuildLanguage + " OS Type is " + t.OsType, TraceLevel.Info);
                Log.WriteServerLog("History Content:" + idList.FirstOrDefault() + " build is " + temp.BuildNo + " language is " + temp.BuildLanguage + " OS Type is " + temp.OsType, TraceLevel.Info);
            }
            List<XElement> xmlelement = new List<XElement>();
            foreach (XElement ele in varList)
            {
                xmlelement.Add(ele);
            }
            return xmlelement;
        }
        //v-yiwzha bug479869 Get Content Language by contentID
        public string QuerylanbycontentID(int contentID)
        {
            var varList = from c in dataContext.UIContents
                          where c.ContentID == contentID
                          select c.BuildInfo.Language;
            string language = string.Empty;
            foreach (string lan in varList)
            {
                language = lan;
            }
            return language;
        }
        //v-yiwzha bug 506859 [SulpHurTool] ShowTest Coverage broken for new build format
        public static string ChangeNewBuildToOldFormat(string newBuild)
        {
            //***directories is sorted by name, search folder from new to old
            string[] folders = { releaseRootFolderName, mainRootFolderName };
            string[] subFolders = null;
            string fileFullPath = string.Empty;
            string oldVersion = string.Empty;
            string newVersionFromFile = string.Empty;
            foreach (string folder in folders)
            {
                try
                {
                    subFolders = Directory.GetDirectories(folder);
                }
                catch (Exception e)
                {
                    Log.WriteServerLog("Failed to get directories from foler:" + folder, TraceLevel.Error);
                    Log.WriteServerLog(e.Message, TraceLevel.Error);
                    return null;
                }
                for (int i = subFolders.Length - 1; i >= 0; i--)
                {
                    oldVersion = subFolders[i].Substring(subFolders[i].LastIndexOf('\\') + 1);
                    if (!oldVersion.StartsWith("5.0."))
                        continue;
                    fileFullPath = subFolders[i] + adminConsoleLCGFileRelatedPath;
                    newVersionFromFile = GetNewBuildsFromFile(fileFullPath);
                    if (string.IsNullOrEmpty(newVersionFromFile))
                        continue;
                    if (newBuild.Equals(newVersionFromFile))
                        return oldVersion;
                    else if (string.Compare(newBuild, newVersionFromFile) > 0)
                        break;
                }
            }
            return null;
        }
        public static string GetNewBuildsFromFile(string path)
        {
            try
            {
                if (!File.Exists(path))
                    return null;
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNode rootNode = doc.LastChild;
                foreach (XmlNode itemNode in rootNode.ChildNodes)
                {
                    if (itemNode.Name == "Item" && itemNode.Attributes["ItemId"].Value == ";Property")
                    {
                        foreach (XmlNode childItemNode in itemNode.ChildNodes)
                        {
                            if (childItemNode.Name == "Item" && childItemNode.Attributes["ItemId"].Value == ";AdminConsoleVersion")
                            {
                                return childItemNode.InnerText;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.WriteServerLog("Failed to access file:" + path, TraceLevel.Error);
                Log.WriteServerLog(e.Message, TraceLevel.Error);
                return null;
            }
            return null;
        }
        public ComplianceResult QueryResultByName(string UIName)
        {
            var varList = from r in dataContext.Results
                          where r.UIContent.UIName == UIName
                          select new ComplianceResult(r.ResultType, r.ResultLog, MS.Internal.SulpHur.UICompliance.Utility.BytesToBmp_MemStream(r.ResultImage));

            foreach (ComplianceResult r in varList)
            {
                return r;
            }
            return new ComplianceResult();
        }

        public void DeleteExistingResult(int contentID, string ruleName)
        {
            int deleted = 0;

            Rule rule = dataContext.Rules.Single(r => r.RuleName == ruleName);
            int id = rule.RuleID;

            var rows = from rec in dataContext.Results
                       where rec.ContentID == contentID && rec.RuleID == id
                       select rec;
            foreach (var row in rows)
            {
                dataContext.Results.DeleteOnSubmit(row);
                deleted++;
            }
            dataContext.SubmitChanges();
            if (deleted > 0)
            {
                Log.WriteServerLog(string.Format("Delete existing result, content id:{0}, ruleName:{1},record count:{2}", contentID, ruleName, deleted), TraceLevel.Verbose);
            }
        }

        public void InsertResult(UIComplianceResultBase rBase)
        {
            Rule rule = dataContext.Rules.Single(r => r.RuleName == rBase.RuleName);
            int id = rule.RuleID;
            UIContent ui = dataContext.UIContents.Single(u => u.ContentID == rBase.UIID);
            Result resultTable = new Result();
            resultTable.ContentID = rBase.UIID;
            if (rBase.Type == ResultType.Pass)
            {
                //Null for pass results
            }
            else
            {
                //v-danpgu: Assign Pass as the resulttype for Text 30% Truncation Rule, whose UIs are in non-English Language. 
                if (id == 20)
                {
                    var bl = from bi in dataContext.BuildInfos
                             join uc in dataContext.UIContents on bi.BuildID equals uc.BuildID
                             where uc.ContentID == resultTable.ContentID
                             select bi;
                    foreach (var b in bl)
                    {
                        if (b.Language != "ENU")
                        {
                            rBase.Type = ResultType.Pass;
                            Log.WriteServerLog("UI build language is:" + b.Language + "; 30% buffer rule can be ignored. Assign result type Pass", TraceLevel.Info);
                        }
                        else
                        {
                            resultTable.ResultImage = MS.Internal.SulpHur.UICompliance.Utility.BmpToBytes_MemStream(rBase.Image);
                        }
                    }
                }
                else
                {
                    resultTable.ResultImage = MS.Internal.SulpHur.UICompliance.Utility.BmpToBytes_MemStream(rBase.Image);
                }
            }

            //resultTable.ResultImage = MS.Internal.SulpHur.UICompliance.Utility.BmpToBytes_MemStream(rBase.Image);
            resultTable.ResultType = rBase.Type.ToString();
            resultTable.ResultLog = rBase.Message;
            resultTable.RuleID = id;
            resultTable.CreateDate = DateTime.Now.ToUniversalTime();
            //v-yiwzha Bug:443134 make the default reviewflag as 0, or it will be null and cause many errors in DB.
            resultTable.ReviewFlag = 0;
            //v-yiwzha Bug:443134 if the UIANDRule verified, mark as reviewed
            if (IsUIANDRuleReviewed(ui.UIContent1, rBase.RuleName, rBase.Image, ui.IsWebUI))
            {
                resultTable.ReviewFlag = 1;
            }
            dataContext.Results.InsertOnSubmit(resultTable);
            dataContext.SubmitChanges();

            //update status
            var sList = from rs in dataContext.RuleStatus
                        where rs.ContentID == rBase.UIID && rs.RuleID == rule.RuleID
                        select rs;
            foreach (var s in sList)
            {
                s.IsChecked = true;
            }

            dataContext.SubmitChanges();
        }

        public IQueryable<int> QuerybyBuildNo(string buildNo)
        {
            var varList = from r in dataContext.UIContents
                          where r.BuildInfo.BuildNo == buildNo
                          select r.ContentID;

            return varList;
        }

        public void InsertScanedUI(int contentID, string ruleName)
        {
            try
            {
                int ruleID = dataContext.Rules.Single(c => c.RuleName == ruleName).RuleID;
                var list = from x in dataContext.RuleStatus
                           where x.ContentID == contentID && x.RuleID == ruleID
                           select x;
                foreach (var x in list)
                {
                    x.IsChecked = false;
                    x.DateUpdated = DateTime.Now;
                }

                dataContext.SubmitChanges();
            }
            catch (Exception e)
            {
                Log.WriteServerLog(e.Message, TraceLevel.Error);
            }
        }


        public List<string> QueryBuildInfo()
        {
            var buildList = from b in dataContext.BuildInfos
                            select b.BuildNo;
            List<string> list = new List<string>();
            foreach (string x in buildList)
            {
                if (!list.Contains(x))
                {
                    list.Add(x);
                }
            }
            return list;
        }

        public void Dispose()
        {
            dataContext.Dispose();
        }

        // Standard format: 5.0.8847.1000; ignore the last minor version
        public static bool IsStandardFormat(string buildString)
        {
            string standardFormat = @"^5.0.[0-9]{4}.[0-9]{3}[0-9]+$";
            return Regex.IsMatch(buildString, standardFormat);
        }
    }

    public interface QueryOperations
    {
        IQueryable<int> QuerybyBuildNo(string buildNo);

        List<UIANDRule> QueryAllDirtyUI();

        List<string> QueryBuildInfo();
    }

    public class UIANDRule
    {
        public UIANDRule() { }
        public UIANDRule(XElement ele, string ruleName, bool isWebUI, Bitmap bit, int uiid)
        {
            this.element = ele;
            this.ruleName = ruleName;
            this.isWebUI = isWebUI;
            this.bitmap = bit;
            this.uiid = uiid;
        }

        public UIANDRule(XElement ele, bool isWebUI, Bitmap bit, int uiid)
        {
            this.element = ele;
            this.isWebUI = isWebUI;
            this.bitmap = bit;
            this.uiid = uiid;
        }

        public UIANDRule(XElement ele, bool isWebUI, Binary binary, int uiid)
        {
            this.element = ele;
            this.isWebUI = isWebUI;
            this.binary = binary;
            this.uiid = uiid;
        }
        XElement element;

        Bitmap bitmap;

        public Bitmap Bitmap
        {
            get { return bitmap; }
            set { bitmap = value; }
        }

        Binary binary;

        public Binary Binary
        {
            get { return binary; }
            set { binary = value; }
        }


        public XElement Element
        {
            get { return element; }
            set { element = value; }
        }
        string ruleName;

        public string RuleName
        {
            get { return ruleName; }
            set { ruleName = value; }
        }

        private bool isWebUI;

        public bool IsWebUI
        {
            get { return isWebUI; }
            set { isWebUI = value; }
        }

        int uiid;

        public int Uiid
        {
            get { return uiid; }
            set { uiid = value; }
        }

    }

    public class UIContentMini
    {
        public int Uiid { get; set; }
        public bool UiIsWebUI { get; set; }
        public XElement UiXmlElement { get; set; }
        public Bitmap UiBitmap { get; set; }
    }

    public class BuildANDAssembly
    {
        public BuildANDAssembly(string buildNo, string buildLanguage, string assembly, string fullTypeName, XElement xelement, string osType)
        {
            this.xelement = xelement;
            this.buildNo = buildNo;
            this.buildLanguage = buildLanguage;
            this.assembly = assembly;
            this.fullTypeName = fullTypeName;
            this.osType = osType;
        }
        XElement xelement;
        public XElement Xelement
        {
            get { return xelement; }
            set { xelement = value; }
        }

        string buildNo;
        public string BuildNo
        {
            get { return buildNo; }
            set { buildNo = value; }
        }
        string buildLanguage;
        public string BuildLanguage
        {
            get { return buildLanguage; }
            set { buildLanguage = value; }
        }
        string assembly;
        public string Assembly
        {
            get { return assembly; }
            set { assembly = value; }
        }
        string fullTypeName;
        public string FullTypeName
        {
            get { return fullTypeName; }
            set { fullTypeName = value; }
        }
        string osType;
        public string OsType
        {
            get { return osType; }
            set { osType = value; }
        }
    }

    public class DataRowAdapter : IDataAdapter
    {
        private AdditionInformations ai;
        public DataRowAdapter(AdditionInformations ai)
        {
            this.ai = ai;
        }
        public T GetRow<T>() where T : Row
        {
            if (typeof(T) == typeof(SulpHurClientIdentity))
            {
                SulpHurClientIdentity temp = new SulpHurClientIdentity();
                temp.MacAddress = ai.MacAddress;
                temp.OSType = ai.OSType;
                temp.UserName = ai.Alias;
                return (T)(object)temp;
            }
            if (typeof(T) == typeof(SulpHurBuildInfo))
            {
                SulpHurBuildInfo temp = new SulpHurBuildInfo();
                temp.BuildNo = ai.ProductVersion.ToString();
                temp.Language = ai.ProductLanguage;
                return (T)(object)temp;
            }
            if (typeof(T) == typeof(SulpHurClientInfo))
            {
                SulpHurClientInfo temp = new SulpHurClientInfo();
                temp.MachineName = ai.ComputerName;
                temp.OSLanguage = ai.OSLanguage;
                temp.MacAddress = ai.MacAddress;
                temp.IPAddress = ai.IP.ToString();
                temp.OSType = ai.OSType;
                temp.UserName = ai.Alias;
                return (T)(object)temp;
            }
            return null;
        }
    }
}
