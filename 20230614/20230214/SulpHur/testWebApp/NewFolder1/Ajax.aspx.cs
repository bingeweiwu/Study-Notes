using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SulpHurManagementSystem.Models;
using System.Drawing;
using System.IO;
using System.IO.MemoryMappedFiles;
using SulpHurManagementSystem.Common;
using System.Reflection;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Data;
using System.Text;
using System.Threading;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Configuration;
using System.Text.RegularExpressions;

namespace SulpHurManagementSystem
{
    public partial class Ajax : System.Web.UI.Page
    {
        string ConnectionString = ConfigurationManager.ConnectionStrings["ADOConn"].ToString();
        string serverName = ConfigurationManager.AppSettings["ServerName"];
        string psProductName = "SMS";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.Request.Form["IsAjax"] != null && this.Request.Form["IsAjax"].ToLower().Equals("true"))
            {
                try
                {

                    MethodInfo methodInfo = this.GetType().GetMethod(this.Request.Form["Method"].ToString());
                    //parse paraments
                    ParameterInfo[] parameterInfoArr = methodInfo.GetParameters();
                    object[] objParameters = new object[parameterInfoArr.Length];
                    foreach (ParameterInfo parameter in parameterInfoArr)
                    {
                        if (this.Request.Form.AllKeys.Contains(parameter.Name, false))
                        {
                            object value = null;
                            try
                            {
                                switch (parameter.ParameterType.ToString())
                                {
                                    case "System.Int32":
                                        value = Convert.ToInt32(this.Request.Form[parameter.Name]);
                                        break;
                                    case "System.Int64":
                                        value = Convert.ToInt64(this.Request.Form[parameter.Name]);
                                        break;
                                    case "System.Boolean":
                                        value = Convert.ToBoolean(this.Request.Form[parameter.Name]);
                                        break;
                                    default:
                                        value = this.Request.Form[parameter.Name];
                                        break;
                                }
                            }
                            catch
                            {
                                throw new SulpHurErrorException(string.Format("Invalid parameter: {0}!", this.Request.Form.AllKeys.First(item => item.Equals(parameter.Name, StringComparison.OrdinalIgnoreCase))));
                            }
                            objParameters[parameter.Position] = value;
                        }
                        else
                        {
                            throw new SulpHurErrorException(string.Format("Miss required parameter!"));
                        }
                    }
                    //invoke
                    methodInfo.Invoke(this, objParameters);
                }
                catch (SulpHurErrorException ex)
                {
                    Response.Write(string.Format("[E]:{0}", ex.Message));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError("Exception: {0}", ex);
                    Response.Write(string.Format("[E]:{0}", "Unknown error."));
                }
            }
        }

        public void Ajax_GetReviewLog(int resultID)
        {
            string reviewlog = null;
            try
            {
                this.GetReviewLog(resultID, out reviewlog);
                if (reviewlog == null || reviewlog == string.Empty || reviewlog == "")
                {
                    Response.Write("[S]:No reviewed comments.");
                }
                else
                {
                    Response.Write(string.Format("[S]:Reviewed Comments:{0}", reviewlog));
                }
            }
            catch (SulpHurErrorException ex)
            {
                Response.Write(string.Format("[E]:{0}", ex.Message));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
                Response.Write(string.Format("[E]:{0}", "Unknown error."));
            }
        }
        public void Ajax_GetDetailInfo(int resultID)
        {
            string logMessage = null;
            string screenshotPath = null;
            string assemblyName = null;
            string fullTypeName = null;
            string needManualEdit = "No";
            try
            {
                this.GetDetailInfo(resultID, out logMessage, out screenshotPath);
                try
                {
                    int uiContentID;
                    this.GetPageIdentifierAssemblyInfo(resultID, out uiContentID, out assemblyName, out fullTypeName);
                }
                catch
                {
                    assemblyName = "Can't find assembly Info";
                    fullTypeName = "Can't find fullTypeName";
                }
                if (logMessage.Contains("No available hotkey to sign") || logMessage.Contains("Could not find control name and possible control name for highlight control") || logMessage.Contains("Could not find previous control for highlight control") || assemblyName == "Can't find assembly Info")
                {
                    needManualEdit = "Yes";
                }
                Response.Write(string.Format("{0}[|]{1}[|]{2}", screenshotPath, logMessage, needManualEdit));
            }
            catch (SulpHurErrorException ex)
            {
                Response.Write(string.Format("[E]:{0}", ex.Message));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
                Response.Write(string.Format("[E]:{0}", ex.Message));
            }
        }
        public void Ajax_LinkBugID(int resultID, string bugIDs)
        {
            string retMessage = string.Empty;

            try
            {
                List<int> iBugIDList = new List<int>();
                try
                {
                    //bugID
                    List<string> strBugIDList = bugIDs.Split(',').ToList();
                    strBugIDList.ForEach(item => iBugIDList.Add(int.Parse(item.Trim())));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex);
                    throw new SulpHurErrorException("Invalid Paraments!");
                }


                //process
                SulpHurEntities entities = new SulpHurEntities();
                int uiContentID = 0;
                string assemblyName = string.Empty;
                string fullTypeName = string.Empty;
                this.GetPageIdentifierAssemblyInfo(resultID, out uiContentID, out assemblyName, out fullTypeName);

                var list = from r in entities.bugs
                           join a in entities.bug_surface on r.bug_id equals a.bug_id
                           join b in entities.surfaces on a.s_id equals b.s_id
                           where b.assembly_name == assemblyName && b.class_name == fullTypeName
                           select r;

                string existBugIDs = string.Empty;
                iBugIDList.ForEach(bugID =>
                {
                    if (list.Any(item => item.bug_id == bugID))
                    {
                        existBugIDs = string.Format("{0},{1}", bugID, existBugIDs);
                    }
                    else
                    {
                        //// update table - bugLink
                        //BugLink bugLink = new BugLink
                        //{
                        //    BugID = bugID,
                        //    ContentID = uiContentID,
                        //};
                        //entities.BugLinks.AddObject(bugLink);

                        // update table - bugs, surfaces, bug_surface
                        this.LinkToBug(bugID, assemblyName, fullTypeName);
                    }
                });

                #region use local method instead of web service
                //BugHudService Client
                //ServiceReference1.BugHudServiceClient client = new ServiceReference1.BugHudServiceClient();
                //ServiceReference1.BugSummary[] bugSummaryArr = client.GetBugs(assemblyName, fullTypeName);
                ////insert bugID link
                //string existBugIDs = string.Empty;
                //iBugIDList.ForEach(bugID =>
                //{
                //    if (bugSummaryArr.Any(item => item.Id == bugID))
                //    {
                //        existBugIDs = string.Format("{0},{1}", bugID, existBugIDs);
                //    }
                //    else
                //    {
                //        //link to SulpHur DB
                //        BugLink bugLink = new BugLink
                //        {
                //            BugID = bugID,
                //            ContentID = uiContentID,
                //        };
                //        entities.BugLinks.AddObject(bugLink);

                //        //link to BugHud DB
                //        client.LinkToBug(bugID, assemblyName, fullTypeName);
                //    }
                //});
                #endregion
                // save the info which are written into table bugLink
                //entities.SaveChanges();

                //close BugHudService client
                //try
                //{
                //    client.Close();
                //}
                //catch
                //{
                //    client.Abort();
                //}

                if (!string.IsNullOrEmpty(existBugIDs))
                {
                    existBugIDs = existBugIDs.TrimEnd();
                    retMessage = string.Format("[W]:[{0}] are already linked, so ignore these bugs.", existBugIDs);
                }

                Response.Write(retMessage);
            }
            catch (SulpHurErrorException ex)
            {
                Response.Write(string.Format("[E]:{0}", ex.Message));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
                Response.Write(string.Format("[E]:{0}", "Unknown error."));
            }
        }
        public void Ajax_CompareWith(int resultID, string buildNos, string buildLanguages, bool isFuzzySearch)
        {
            try
            {
                string retValue = string.Empty;
                SulpHurEntities entities = new SulpHurEntities();
                List<string> buildList = new List<string>();
                Guid? guid = null;
                int? typeID = null;
                int ruleID;


                //parse request builds
                string[] buildNoArr = buildNos.Split(',');
                string[] buildLanguageArr = buildLanguages.Split(',');
                foreach (string buildNo in buildNoArr)
                {
                    foreach (string buildLanguage in buildLanguageArr)
                    {
                        buildList.Add(string.Format("{0},{1}", buildNo.Trim(), buildLanguage.Trim()));
                    }
                }

                //get Content GUID and TypeID
                var list = from r in entities.Results
                           join u in entities.UIContents on r.ContentID equals u.ContentID
                           join a in entities.AssemblyLinks on u.ContentID equals a.ContentID into Assembly_UIContent
                           from au in Assembly_UIContent.DefaultIfEmpty()
                           where r.ResultID == resultID
                           select new { r.RuleID, u.GUID, AssemblyLink = au };
                var result = list.FirstOrDefault(item => item.AssemblyLink == null ? true : item.AssemblyLink.IsPageIdentifier == true);
                guid = result.GUID;
                ruleID = result.RuleID;
                if (result.AssemblyLink == null)
                {
                    throw new SulpHurErrorException("Failed due to no page identifier assembly info.");
                }
                else
                {
                    typeID = result.AssemblyLink.TypeID;
                }

                //process each build
                foreach (string build in buildList)
                {
                    //parse build info
                    string[] buildArr = build.Split(',');
                    string buildNo = buildArr[0];
                    string buildLanguage = buildArr[1];

                    //get resultID
                    IQueryable<dynamic> outResults = null;
                    if (isFuzzySearch)
                    {
                        //assembly info search
                        if (typeID == null)
                            throw new SulpHurErrorException("Failed due to no assembly info.");

                        outResults = from item in entities.Results
                                     where item.UIContent.BuildInfo.BuildNo.Trim() == buildNo &&
                                           item.UIContent.BuildInfo.Language.Trim() == buildLanguage &&
                                           item.UIContent.AssemblyLinks.Any(al => al.TypeID == typeID) &&
                                           item.UIContent.UIName != "Configuration Manager" &&
                                           item.RuleID == ruleID
                                     select
                                       new
                                       {
                                           item.ResultID,
                                           item.ResultType
                                       };
                    }
                    else
                    {
                        //GUID search
                        if (guid == null)
                            throw new SulpHurErrorException("Failed due to no GUID.");

                        outResults = from item in entities.Results
                                     where item.UIContent.BuildInfo.BuildNo.Trim() == buildNo &&
                                           item.UIContent.BuildInfo.Language.Trim() == buildLanguage &&
                                           item.UIContent.GUID == guid &&
                                           item.RuleID == ruleID
                                     select
                                         new
                                         {
                                             item.ResultID,
                                             item.ResultType
                                         };
                    }
                    //no result found
                    if (outResults.Count().Equals(0))
                    {
                        retValue += string.Format("{0}[|]{1}[|]{2}[|]{3}[$]", build, "", "", "");
                        continue;
                    }

                    foreach (dynamic outResult in outResults)
                    {
                        //get LogMessage and screenshot
                        string logMessage = null;
                        string screenshotPath = null;
                        this.GetDetailInfo(outResult.ResultID, out logMessage, out screenshotPath);
                        retValue += string.Format("{0}[|]{1}[|]{2}[|]{3}[$]", build, logMessage, screenshotPath, outResult.ResultType);
                    }
                }
                if (!string.IsNullOrEmpty(retValue))
                {
                    retValue = retValue.Remove(retValue.Length - 3);
                }

                Response.Write(retValue);
            }
            catch (SulpHurErrorException ex)
            {
                Response.Write(string.Format("[E]:{0}", ex.Message));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
                Response.Write(string.Format("[E]:{0}", "Unknown error."));
            }
        }
        public void Ajax_GetTestCoverageResult(int resultID, string languages, string rules, string buildNo)
        {
            try
            {
                string text = string.Empty;
                string assemblyname = string.Empty;
                string fulltypename = string.Empty;
                this.GetAssemblyInfo(resultID, out assemblyname, out fulltypename);
                text = assemblyname + '~' + fulltypename + '~';
                string[] rulearray = rules.Split(',');
                string[] languagearray = languages.Split(',');
                int num = rulearray.Length;
                for (int i = 0; i < rulearray.Length; i++)
                {
                    string str = rulearray[i];
                    text += str;
                    for (int j = 0; j < languagearray.Length; j++)
                    {
                        string resultType = string.Empty;
                        string buildLanguage = languagearray[j];
                        string latestbuildNo = string.Empty;
                        this.GetResultTypeByInfo(rulearray[i], assemblyname, fulltypename, out latestbuildNo, buildLanguage, out resultType);
                        int inputbuild = int.Parse(buildNo);
                        if (latestbuildNo == "0")
                        {
                            text += "[|]NotRun";
                        }
                        else
                        {
                            bool isStandardBuildFormat = this.IsStandardFormat(latestbuildNo);
                            if (isStandardBuildFormat)
                            {
                                int latestbuild = int.Parse(latestbuildNo.Substring(4, 4));
                                if (inputbuild <= latestbuild)
                                {
                                    if (resultType == "Pass") text = text + "[|][P]:" + latestbuild;
                                    if (resultType == "Warning") text = text + "[|][W]:" + latestbuild;
                                    if (resultType == "Fail") text = text + "[|][F]:" + latestbuild;
                                }
                                else
                                {
                                    text += "[|]NotRun";
                                }
                            }
                            else
                            {
                                if (resultType == "Pass") text = text + "[|][P]:" + latestbuildNo;
                                if (resultType == "Warning") text = text + "[|][W]:" + latestbuildNo;
                                if (resultType == "Fail") text = text + "[|][F]:" + latestbuildNo;
                            }
                        }
                    }
                    text += "\n";
                }
                base.Response.Write(text);
            }
            catch (Exception ex)
            {
                base.Response.Write("[E]:" + ex.Message);
            }
        }
        public void Ajax_GeneratePSQ(string path, int resultID)
        {
            try
            {
                SulpHurEntities entities = new SulpHurEntities();
                int uiContentID = 0;
                string assemblyName = string.Empty;
                string fullTypeName = string.Empty;
                this.GetPageIdentifierAssemblyInfo(resultID, out uiContentID, out assemblyName, out fullTypeName);
                var list = from r in entities.bugs
                           join a in entities.bug_surface on r.bug_id equals a.bug_id
                           join b in entities.surfaces on a.s_id equals b.s_id
                           where b.assembly_name == assemblyName && b.class_name == fullTypeName
                           orderby r.bug_id descending
                           select r;
                string templetestr = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" + "<Data Application=\"Product Studio\" Type=\"Query\" Version=\"2.1\"><Query Product=\"SMS\" CurrentMode=\"Bugs\" CurrentTree=\"1\"><Mode Type=\"Bugs\" ModeFormat=\"QueryBuilder\"><QueryBuilder><Expression AttachWith=\"\" Field=\"Product\" Operator=\"Equals\" Value=\"SMS\"/><Expression AttachWith=\"And\" Field=\"ID\" Operator=\"Equals\" Value=\"ReplaceSpace\"/></QueryBuilder><ResultList><DisplayColumns><Column Name=\"ID\" Width=\"140\"/><Column Name=\"Title\" Width=\"991\"/></DisplayColumns><SortColumns><Column Name=\"ID\" Ascending=\"-1\"/></SortColumns></ResultList></Mode><Mode Type=\"Contacts\"><Browse Product=\"SMS\" BrowseBy=\"Path\" BrowsePath=\"\\\"/></Mode><Mode Type=\"Links\" ModeFormat=\"QueryBuilder\"><QueryBuilder><Expression AttachWith=\"\" Field=\"Product\" Operator=\"Equals\" Value=\"SMS\"/><Expression AttachWith=\"And\" Field=\"Dependent Link Status\" Operator=\"Equals\" Value=\"Pending\"/></QueryBuilder><ResultList><DisplayColumns><Column Name=\"Parent Bug ID\" Width=\"140\"/><Column Name=\"Child Bug Product\" Width=\"140\"/><Column Name=\"Child Bug ID\" Width=\"140\"/><Column Name=\"Link Comment\" Width=\"140\"/></DisplayColumns><SortColumns><Column Name=\"Parent Bug ID\" Ascending=\"-1\"/></SortColumns></ResultList></Mode><Mode Type=\"TestCase\" ModeFormat=\"QueryBuilder\"><QueryBuilder><Expression AttachWith=\"\" Field=\"Product\" Operator=\"Equals\" Value=\"SMS\"/></QueryBuilder><ResultList><DisplayColumns><Column Name=\"ID\" Width=\"140\"/><Column Name=\"Title\" Width=\"140\"/></DisplayColumns><SortColumns><Column Name=\"ID\" Ascending=\"-1\"/></SortColumns></ResultList></Mode><Mode Type=\"TestResult\" ModeFormat=\"QueryBuilder\"><QueryBuilder><Expression AttachWith=\"\" Field=\"Product\" Operator=\"Equals\" Value=\"SMS\"/></QueryBuilder><ResultList><DisplayColumns><Column Name=\"ID\" Width=\"140\"/><Column Name=\"Result Value\" Width=\"140\"/></DisplayColumns><SortColumns><Column Name=\"ID\" Ascending=\"-1\"/></SortColumns></ResultList></Mode></Query></Data>";
                string fileName = "BugQueryAboutResult" + resultID + ".psq";
                string filePath = System.IO.Path.Combine(path, fileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                //Create a str1 that contains the bugid and some necessary select content.
                string bugstr = string.Empty;
                int index = 0;
                foreach (bug item in list)
                {
                    if (index == 0)
                    {
                        bugstr = item.bug_id.ToString();
                    }
                    else
                    {
                        bugstr = bugstr + "\"/><Expression AttachWith=\"Or\" Field=\"ID\" Operator=\"Equals\" Value=\"" + item.bug_id.ToString();
                    }
                    index++;
                }
                string desstr = templetestr.Replace("ReplaceSpace", bugstr);
                FileStream fs = new FileStream(filePath, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(desstr);
                sw.Close();
                fs.Close();
                Response.Write(string.Format("[S]:{0}", "Successful to generate PSQ file:" + filePath));
            }
            catch (Exception ex)
            {
                Response.Write(string.Format("[E]:{0}", ex.Message));
            }
        }
        //v-danpgu file multi bugs

        //public void Ajax_ExportMultiPdf(string openby, string path, string ResultIDList, bool retry)
        //{
        //    CaptureUIsService service = new CaptureUIsService();
        //    string result = string.Empty;
        //    string error = string.Empty;
        //    string file = string.Empty;
        //    string[] arrResults = ResultIDList.Split(',');
        //    StringBuilder bugIDList = new StringBuilder();
        //    foreach (string id in arrResults)
        //    {
        //        try
        //        {
        //            if (!string.IsNullOrEmpty(path))
        //            {
        //                Ajax_ExportInformation2(path, Convert.ToInt32(id), 1, ref file);
        //                //file = string.Empty;
        //            }
        //            string bugid = service.FileBug(openby, id, file);

        //            //retry filebug

        //            Ajax_LinkBugID(Convert.ToInt32(id), bugid);
        //            result = result + bugid + ",";
        //            ResultIDList = ResultIDList.Replace(id, "").TrimStart(',');
        //            bugIDList.Append(bugid).Append(",");
        //            WriteReTryFile(ResultIDList, openby);
        //            if (retry)
        //            {
        //                WriteBugIdFile(bugid, openby, retry);
        //            }
        //            else
        //            {
        //                WriteBugIdFile(bugIDList.ToString(), openby, retry);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            error = "These resultIDs failed to filebug: ";
        //            error = error + id + ',';
        //        }
        //    }
        //    result = result.TrimEnd(',');
        //    result = result + ". BugIds have been attached.";
        //    Response.Write(result + error.TrimEnd(','));
        //}
        //public void Ajax_ExportMultiPdfInVSO(string openby, string path, string ResultIDList, bool retry)
        //{
        //    CaptureUIsService service = new CaptureUIsService();
        //    string result = string.Empty;
        //    string error = string.Empty;
        //    string file = string.Empty;
        //    string[] arrResults = ResultIDList.Split(',');
        //    StringBuilder bugIDList = new StringBuilder();
        //    foreach (string id in arrResults)
        //    {
        //        try
        //        {
        //            if (!string.IsNullOrEmpty(path))
        //            {
        //                Ajax_ExportInformation2(path, Convert.ToInt32(id), 1, ref file);
        //                //file = string.Empty;
        //            }
        //            string bugid = service.FileVSOBug(openby, id, file);

        //            //retry filebug

        //            Ajax_LinkBugID(Convert.ToInt32(id), bugid);
        //            result = result + bugid + ",";
        //            ResultIDList = ResultIDList.Replace(id, "").TrimStart(',');
        //            bugIDList.Append(bugid).Append(",");
        //            WriteReTryFile(ResultIDList, openby);
        //            if (retry)
        //            {
        //                WriteBugIdFile(bugid, openby, retry);
        //            }
        //            else
        //            {
        //                WriteBugIdFile(bugIDList.ToString(), openby, retry);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            error = "These resultIDs failed to filebug: ";
        //            error = error + id + ',';
        //        }
        //    }
        //    result = result.TrimEnd(',');
        //    result = result + ". BugIds have been attached.";
        //    Response.Write(result + error.TrimEnd(','));
        //}

        private void WriteReTryFile(string retryResultIDs, string openby)
        {
            openby = openby.Replace("\\", "");
            string filePath = @"\\scfs\Users\INTL\RetryResultIDs\retryResultIDs_" + openby + ".txt";
            if (!System.IO.Directory.Exists(@"\\scfs\Users\INTL\RetryResultIDs"))
            {
                System.IO.Directory.CreateDirectory(@"\\scfs\Users\INTL\RetryResultIDs");
            }
            System.IO.File.WriteAllText(filePath, retryResultIDs);
        }

        private void WriteBugIdFile(string bugIDList, string openby, bool retry)
        {
            openby = openby.Replace("\\", "");
            bugIDList = bugIDList.TrimEnd(',');
            string filePath = @"\\scfs\Users\INTL\RetryResultIDs\bugIDList_" + openby + ".txt";
            if (!System.IO.Directory.Exists(@"\\scfs\Users\INTL\RetryResultIDs"))
            {
                System.IO.Directory.CreateDirectory(@"\\scfs\Users\INTL\RetryResultIDs");
            }
            if (retry)
            {
                System.IO.File.AppendAllText(filePath, "," + bugIDList);
            }
            else
            {
                System.IO.File.WriteAllText(filePath, bugIDList);
            }
        }

        //v-yiwzha ExportInformation method
        public void Ajax_ExportInformation(string path, int resultID, int flag)
        {
            try
            {
                SulpHurEntities entities = new SulpHurEntities();
                // v-yiwzha Initialize information to export
                string OSLanguage = string.Empty;
                string Language = string.Empty;
                string buildNo = string.Empty;
                string OSType = string.Empty;
                int uiContentID = 0;
                string uiName = string.Empty;
                string assemblyName = string.Empty;
                string fullTypeName = string.Empty;
                string screenshotPath = string.Empty;
                string logMessage = string.Empty;
                string rulename = string.Empty;
                string ruledescription = string.Empty;
                string reprostep = string.Empty;
                string resultType = string.Empty;

                this.GetUIInfo(resultID, out uiName, out reprostep, out OSLanguage, out OSType);
                if (reprostep == string.Empty || reprostep == "" || reprostep == null)
                {
                    reprostep = "No repro steps captured";
                }
                try
                {
                    this.GetPageIdentifierAssemblyInfo(resultID, out uiContentID, out assemblyName, out fullTypeName);
                }
                catch
                {
                    assemblyName = "Can't find assembly Info";
                    fullTypeName = "Can't find fullTypeName";
                }
                this.GetBuildInfo(resultID, out Language, out buildNo);
                this.GetDetailInfo(resultID, out logMessage, out screenshotPath);
                this.GetRuleInfo(resultID, out rulename, out ruledescription);
                this.GetResultType(resultID, out resultType);
                //v-yiwzha bug467064 Move the fontpath to config file which user can easy set the path
                string fontpath = System.Configuration.ConfigurationManager.AppSettings["FontPath"];
                screenshotPath = string.Format(@"{0}Tmp_ResultImage\{1}.jpg", Request.PhysicalApplicationPath, resultID);
                //Default Europe language
                BaseFont _bfDef = BaseFont.CreateFont(fontpath + "Arial.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                //Simple Chinese
                BaseFont _bfChs = BaseFont.CreateFont(fontpath + "SIMSUN.TTC,1", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                //Japanese
                BaseFont _bfJpn = BaseFont.CreateFont(fontpath + "MSMincho.TTC,1", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                //Korean
                BaseFont _bfKor = BaseFont.CreateFont(fontpath + "Gulim.TTC,1", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                //Traditional Chinese
                BaseFont _bfCht = BaseFont.CreateFont(fontpath + "Msjh.TTC,1", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                string filename = rulename + " [" + resultType + "] " + Language + "_" + uiName + "_" + "ResultID" + resultID.ToString();
                string shortuiName = string.Empty;
                if (uiName.Length > 40)
                {
                    shortuiName = uiName.Substring(0, 40) + "~~~";
                    filename = rulename + " [" + resultType + "] " + Language + "_" + shortuiName + "_" + "ResultID" + resultID.ToString();
                }
                if (rulename.Contains("Access Key"))
                {
                    string info = string.Empty;
                    if (logMessage.Contains("Follow Controls miss short cut"))
                    {
                        info = "Controls miss hotkeys";
                    }
                    if (logMessage.Contains("The following controls have the same access key"))
                    {
                        info = logMessage.Replace("The following controls have the ", "");
                        int count = 0;
                        string shortcontrolname1 = string.Empty;
                        string controlname1 = string.Empty;
                        string shortcontrolname2 = string.Empty;
                        string controlname2 = string.Empty;
                        int start1 = 0;
                        int start2 = 0;
                        int end = 0;
                        int tag = 0;
                        for (int i = 0; i < info.Length; i++)
                        {
                            if (info[i] == '\"') count++;
                            if (count == 1 && tag == 0)
                            {
                                start1 = i;
                                tag++;
                            }
                            if (count == 2 && tag == 1)
                            {
                                end = i;
                                tag++;
                                controlname1 = info.Substring(start1, end - start1);
                                if (end - start1 > 40)
                                {
                                    shortcontrolname1 = info.Substring(start1, 40);
                                }
                            }
                            if (count == 3 && tag == 2)
                            {
                                start2 = i;
                                tag++;
                            }
                            if (count == 4 && tag == 3)
                            {
                                end = i;
                                controlname2 = info.Substring(start2, end - start2);
                                if (end - start2 > 40)
                                {
                                    shortcontrolname2 = info.Substring(start2, 40);
                                }
                                info = info.Substring(0, i + 1);
                                break;
                            }
                            if (i == info.Length - 1)
                            {
                                info = resultID.ToString();
                            }
                        }
                        if (shortcontrolname2 != string.Empty)
                        {
                            info = info.Replace(controlname2, shortcontrolname2 + "~~~");
                        }
                        if (shortcontrolname1 != string.Empty)
                        {
                            info = info.Replace(controlname1, shortcontrolname1 + "~~~");
                        }
                    }
                    if (info != string.Empty)
                    {
                        filename = filename.Replace("ResultID" + resultID.ToString(), info);
                        filename = filename.Replace("': \"", "' in \"");
                    }
                }

                //v-danpgu 478039
                filename = filename.Replace("'", "‘");

                //v-edy: bug486578
                filename = filename.Replace("\\", "");
                filename = filename.Replace("/", "");
                filename = filename.Replace(":", "");
                filename = filename.Replace("*", "");
                filename = filename.Replace("?", "");
                filename = filename.Replace("<", "");
                filename = filename.Replace(">", "");
                filename = filename.Replace("|", "");
                filename = filename.Replace("\"", "‘");
                filename = filename.Replace(".", "");
                filename = filename.Replace("\r", "");
                filename = filename.Replace("\n", "");

                string file = path + "\\" + filename + ".pdf";
                //make sure file should not more than 248 characters
                if (file.Length > 248)
                {
                    file = file.Substring(0, 240) + ".pdf";
                }
                Document d = new Document(PageSize.A4, 10, 10, 25, 25);
                try
                {
                    FileStream fs = new FileStream(file, FileMode.OpenOrCreate);
                    PdfWriter writer = PdfWriter.GetInstance(d, fs);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(screenshotPath);
                if (img.Width > 550)
                {
                    img.ScaleToFit(550, 600);
                }
                iTextSharp.text.Font fontTitle = new iTextSharp.text.Font(_bfDef, 16, 1);
                iTextSharp.text.Font fontDesc = new iTextSharp.text.Font(_bfDef, 11, 1);
                iTextSharp.text.Font fontLink = FontFactory.GetFont(fontpath + "Arial.TTF", 12, iTextSharp.text.Font.UNDERLINE, new BaseColor(0, 0, 255));
                if (Language == "CHS")
                {
                    fontTitle = new iTextSharp.text.Font(_bfChs, 16, 1);
                    fontDesc = new iTextSharp.text.Font(_bfChs, 11, 1);
                    fontLink = FontFactory.GetFont(fontpath + "SIMSUN.TTC,1", 12, iTextSharp.text.Font.UNDERLINE, new BaseColor(0, 0, 255));
                }
                if (Language == "JPN")
                {
                    fontTitle = new iTextSharp.text.Font(_bfJpn, 16, 1);
                    fontDesc = new iTextSharp.text.Font(_bfJpn, 11, 1);
                    fontLink = FontFactory.GetFont(fontpath + "MSMincho.TTC,1", 12, iTextSharp.text.Font.UNDERLINE, new BaseColor(0, 0, 255));
                }
                if (Language == "KOR")
                {
                    fontTitle = new iTextSharp.text.Font(_bfKor, 16, 1);
                    fontDesc = new iTextSharp.text.Font(_bfKor, 11, 1);
                    fontLink = FontFactory.GetFont(fontpath + "Gulim.TTC,1", 12, iTextSharp.text.Font.UNDERLINE, new BaseColor(0, 0, 255));
                }
                if (Language == "CHT")
                {
                    fontTitle = new iTextSharp.text.Font(_bfCht, 16, 1);
                    fontDesc = new iTextSharp.text.Font(_bfCht, 11, 1);
                    fontLink = FontFactory.GetFont(fontpath + "Msjh.TTC,1", 12, iTextSharp.text.Font.UNDERLINE, new BaseColor(0, 0, 255));
                }
                //string link = "http://sulphurserver15/SulpHurReports/CapturedUIReport.aspx?buildlanguage=" + Language + "&rule=" + rulename + "" + assemblyName + "&fulltypename=" + fullTypeName + "&resulttype=Pass&showlatest=buildno";
                string link = string.Format("http://{0}/SulpHurReports/CapturedUIReport.aspx?buildlanguage={1}&rule={2}&assembly={3}&fulltypename={4}&resulttype=Pass&showlatest=buildno",
                    this.serverName, Language, rulename, assemblyName, fullTypeName);
                Anchor anchor = new Anchor(link, fontLink);
                anchor.Reference = link;
                d.Open();
                d.Add(new Paragraph("File Name:", fontTitle));
                d.Add(new Paragraph(filename, fontDesc));
                d.Add(new Paragraph("Result ID:", fontTitle));
                d.Add(new Paragraph(resultID.ToString(), fontDesc));
                d.Add(new Paragraph("UI Name:", fontTitle));
                d.Add(new Paragraph(uiName, fontDesc));
                d.Add(new Paragraph("Repro Steps:", fontTitle));
                d.Add(new Paragraph(reprostep, fontDesc));
                d.Add(new Paragraph("Build Language:", fontTitle));
                d.Add(new Paragraph(Language, fontDesc));
                d.Add(new Paragraph("Real OS Language:", fontTitle));
                d.Add(new Paragraph(OSLanguage, fontDesc));
                d.Add(new Paragraph("OS Type:", fontTitle));
                d.Add(new Paragraph(OSType, fontDesc));
                d.Add(new Paragraph("Build No:", fontTitle));
                d.Add(new Paragraph(buildNo, fontDesc));
                d.Add(new Paragraph("Assembly:", fontTitle));
                d.Add(new Paragraph(assemblyName, fontDesc));
                d.Add(new Paragraph("Full Type Name:", fontTitle));
                d.Add(new Paragraph(fullTypeName, fontDesc));
                d.Add(new Paragraph("Rule Name:", fontTitle));
                d.Add(new Paragraph(rulename, fontDesc));
                d.Add(new Paragraph("Rule Description:", fontTitle));
                d.Add(new Paragraph(ruledescription, fontDesc));
                d.Add(new Paragraph("Latest Pass result:", fontTitle));
                d.Add(anchor);
                d.Add(new Paragraph("Result Log:", fontTitle));
                if (rulename.Contains("Access Key"))
                {
                    if (!logMessage.Contains("More Control Infomation"))
                    {
                        d.Add(new Paragraph(logMessage, fontDesc));
                    }
                    if (logMessage.Contains("More Control Infomation"))
                    {
                        string logMessage1 = string.Empty;
                        for (int i = 0; i < logMessage.Length; i++)
                        {
                            if (logMessage[i] == 'H' && logMessage[i + 1] == 'o' && logMessage[i + 2] == 't' && logMessage[i + 3] == 'k' && logMessage[i + 4] == 'e' && logMessage[i + 5] == 'y' && logMessage[i + 6] == 's' && logMessage[i + 7] == ' ' && logMessage[i + 8] == 'I' && logMessage[i + 9] == 'n' && logMessage[i + 10] == ' ' && logMessage[i + 11] == 'C' && logMessage[i + 12] == 'o' && logMessage[i + 13] == 'n' && logMessage[i + 14] == 't' && logMessage[i + 15] == 'r' && logMessage[i + 16] == 'o' && logMessage[i + 17] == 'l' && logMessage[i + 18] == ':')
                            {
                                logMessage1 = logMessage.Substring(0, i);
                                logMessage = logMessage.Substring(i + 20);
                                logMessage = logMessage.Replace("in control", "                                 ");
                                logMessage = logMessage.Replace("\" '", "\"\n'");
                                break;
                            }
                        }
                        d.Add(new Paragraph(logMessage1, fontDesc));
                        d.Add(new Paragraph("Hotkey Table:", fontTitle));
                        d.Add(new Paragraph("Access Key        Control Name", fontTitle));
                        d.Add(new Paragraph(logMessage, fontDesc));
                    }
                }
                else
                {
                    d.Add(new Paragraph(logMessage, fontDesc));
                }
                d.Add(img);
                d.Close();
                Response.Write(file);
            }
            catch (Exception ex)
            {
                Response.Write(string.Format("Exception:{0}", ex.Message));
            }
        }

        public void Ajax_ExportInformation2(string path, int resultID, int flag, ref string fileoutPath)
        {
            try
            {
                SulpHurEntities entities = new SulpHurEntities();
                // v-yiwzha Initialize information to export
                string OSLanguage = string.Empty;
                string Language = string.Empty;
                string buildNo = string.Empty;
                string OSType = string.Empty;
                int uiContentID = 0;
                string uiName = string.Empty;
                string assemblyName = string.Empty;
                string fullTypeName = string.Empty;
                string screenshotPath = string.Empty;
                string logMessage = string.Empty;
                string rulename = string.Empty;
                string ruledescription = string.Empty;
                string reprostep = string.Empty;
                string resultType = string.Empty;

                this.GetUIInfo(resultID, out uiName, out reprostep, out OSLanguage, out OSType);
                if (reprostep == string.Empty || reprostep == "" || reprostep == null)
                {
                    reprostep = "No repro steps captured";
                }
                try
                {
                    this.GetPageIdentifierAssemblyInfo(resultID, out uiContentID, out assemblyName, out fullTypeName);
                }
                catch
                {
                    assemblyName = "Can't find assembly Info";
                    fullTypeName = "Can't find fullTypeName";
                }
                this.GetBuildInfo(resultID, out Language, out buildNo);
                this.GetDetailInfo(resultID, out logMessage, out screenshotPath);
                this.GetRuleInfo(resultID, out rulename, out ruledescription);
                this.GetResultType(resultID, out resultType);
                //v-yiwzha bug467064 Move the fontpath to config file which user can easy set the path
                string fontpath = System.Configuration.ConfigurationManager.AppSettings["FontPath"];
                screenshotPath = string.Format(@"{0}Tmp_ResultImage\{1}.jpg", Request.PhysicalApplicationPath, resultID);
                //Default Europe language
                BaseFont _bfDef = BaseFont.CreateFont(fontpath + "Arial.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                //Simple Chinese
                BaseFont _bfChs = BaseFont.CreateFont(fontpath + "SIMSUN.TTC,1", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                //Japanese
                BaseFont _bfJpn = BaseFont.CreateFont(fontpath + "MSMincho.TTC,1", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                //Korean
                BaseFont _bfKor = BaseFont.CreateFont(fontpath + "Gulim.TTC,1", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                //Traditional Chinese
                BaseFont _bfCht = BaseFont.CreateFont(fontpath + "Msjh.TTC,1", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                string filename = rulename + " [" + resultType + "] " + Language + "_" + uiName + "_" + "ResultID" + resultID.ToString();
                string shortuiName = string.Empty;
                if (uiName.Length > 40)
                {
                    shortuiName = uiName.Substring(0, 40) + "~~~";
                    filename = rulename + " [" + resultType + "] " + Language + "_" + shortuiName + "_" + "ResultID" + resultID.ToString();
                }
                if (rulename.Contains("Access Key"))
                {
                    string info = string.Empty;
                    if (logMessage.Contains("Follow Controls miss short cut"))
                    {
                        info = "Controls miss hotkeys";
                    }
                    if (logMessage.Contains("The following controls have the same access key"))
                    {
                        info = logMessage.Replace("The following controls have the ", "");
                        int count = 0;
                        string shortcontrolname1 = string.Empty;
                        string controlname1 = string.Empty;
                        string shortcontrolname2 = string.Empty;
                        string controlname2 = string.Empty;
                        int start1 = 0;
                        int start2 = 0;
                        int end = 0;
                        int tag = 0;
                        for (int i = 0; i < info.Length; i++)
                        {
                            if (info[i] == '\"') count++;
                            if (count == 1 && tag == 0)
                            {
                                start1 = i;
                                tag++;
                            }
                            if (count == 2 && tag == 1)
                            {
                                end = i;
                                tag++;
                                controlname1 = info.Substring(start1, end - start1);
                                if (end - start1 > 40)
                                {
                                    shortcontrolname1 = info.Substring(start1, 40);
                                }
                            }
                            if (count == 3 && tag == 2)
                            {
                                start2 = i;
                                tag++;
                            }
                            if (count == 4 && tag == 3)
                            {
                                end = i;
                                controlname2 = info.Substring(start2, end - start2);
                                if (end - start2 > 40)
                                {
                                    shortcontrolname2 = info.Substring(start2, 40);
                                }
                                info = info.Substring(0, i + 1);
                                break;
                            }
                            if (i == info.Length - 1)
                            {
                                info = resultID.ToString();
                            }
                        }
                        if (shortcontrolname2 != string.Empty)
                        {
                            info = info.Replace(controlname2, shortcontrolname2 + "~~~");
                        }
                        if (shortcontrolname1 != string.Empty)
                        {
                            info = info.Replace(controlname1, shortcontrolname1 + "~~~");
                        }
                    }
                    if (info != string.Empty)
                    {
                        filename = filename.Replace("ResultID" + resultID.ToString(), info);
                        filename = filename.Replace("': \"", "' in \"");
                    }
                }

                //v-danpgu 478039
                filename = filename.Replace("'", "‘");

                //v-edy: bug486578
                filename = filename.Replace("\\", "");
                filename = filename.Replace("/", "");
                filename = filename.Replace(":", "");
                filename = filename.Replace("*", "");
                filename = filename.Replace("?", "");
                filename = filename.Replace("<", "");
                filename = filename.Replace(">", "");
                filename = filename.Replace("|", "");
                filename = filename.Replace("\"", "‘");
                filename = filename.Replace(".", "");
                filename = filename.Replace("\r", "");
                filename = filename.Replace("\n", "");

                string file = path + "\\" + filename + ".pdf";
                //make sure file should not more than 248 characters
                if (file.Length > 248)
                {
                    file = file.Substring(0, 240) + ".pdf";
                }
                Document d = new Document(PageSize.A4, 10, 10, 25, 25);
                PdfWriter writer = PdfWriter.GetInstance(d, new FileStream(file, FileMode.Create));
                iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(screenshotPath);
                if (img.Width > 550)
                {
                    img.ScaleToFit(550, 600);
                }
                iTextSharp.text.Font fontTitle = new iTextSharp.text.Font(_bfDef, 16, 1);
                iTextSharp.text.Font fontDesc = new iTextSharp.text.Font(_bfDef, 11, 1);
                if (Language == "CHS")
                {
                    fontTitle = new iTextSharp.text.Font(_bfChs, 16, 1);
                    fontDesc = new iTextSharp.text.Font(_bfChs, 11, 1);
                }
                if (Language == "JPN")
                {
                    fontTitle = new iTextSharp.text.Font(_bfJpn, 16, 1);
                    fontDesc = new iTextSharp.text.Font(_bfJpn, 11, 1);
                }
                if (Language == "KOR")
                {
                    fontTitle = new iTextSharp.text.Font(_bfKor, 16, 1);
                    fontDesc = new iTextSharp.text.Font(_bfKor, 11, 1);
                }
                if (Language == "CHT")
                {
                    fontTitle = new iTextSharp.text.Font(_bfCht, 16, 1);
                    fontDesc = new iTextSharp.text.Font(_bfCht, 11, 1);
                }
                d.Open();
                d.Add(new Paragraph("File Name:", fontTitle));
                d.Add(new Paragraph(filename, fontDesc));
                d.Add(new Paragraph("Result ID:", fontTitle));
                d.Add(new Paragraph(resultID.ToString(), fontDesc));
                d.Add(new Paragraph("UI Name:", fontTitle));
                d.Add(new Paragraph(uiName, fontDesc));
                d.Add(new Paragraph("Repro Steps:", fontTitle));
                d.Add(new Paragraph(reprostep, fontDesc));
                d.Add(new Paragraph("Build Language:", fontTitle));
                d.Add(new Paragraph(Language, fontDesc));
                d.Add(new Paragraph("Real OS Language:", fontTitle));
                d.Add(new Paragraph(OSLanguage, fontDesc));
                d.Add(new Paragraph("OS Type:", fontTitle));
                d.Add(new Paragraph(OSType, fontDesc));
                d.Add(new Paragraph("Build No:", fontTitle));
                d.Add(new Paragraph(buildNo, fontDesc));
                d.Add(new Paragraph("Assembly:", fontTitle));
                d.Add(new Paragraph(assemblyName, fontDesc));
                d.Add(new Paragraph("Full Type Name:", fontTitle));
                d.Add(new Paragraph(fullTypeName, fontDesc));
                d.Add(new Paragraph("Rule Name:", fontTitle));
                d.Add(new Paragraph(rulename, fontDesc));
                d.Add(new Paragraph("Rule Description:", fontTitle));
                d.Add(new Paragraph(ruledescription, fontDesc));
                d.Add(new Paragraph("Result Log:", fontTitle));
                if (rulename.Contains("Access Key"))
                {
                    if (!logMessage.Contains("More Control Infomation"))
                    {
                        d.Add(new Paragraph(logMessage, fontDesc));
                    }
                    if (logMessage.Contains("More Control Infomation"))
                    {
                        string logMessage1 = string.Empty;
                        for (int i = 0; i < logMessage.Length; i++)
                        {
                            if (logMessage[i] == 'H' && logMessage[i + 1] == 'o' && logMessage[i + 2] == 't' && logMessage[i + 3] == 'k' && logMessage[i + 4] == 'e' && logMessage[i + 5] == 'y' && logMessage[i + 6] == 's' && logMessage[i + 7] == ' ' && logMessage[i + 8] == 'I' && logMessage[i + 9] == 'n' && logMessage[i + 10] == ' ' && logMessage[i + 11] == 'C' && logMessage[i + 12] == 'o' && logMessage[i + 13] == 'n' && logMessage[i + 14] == 't' && logMessage[i + 15] == 'r' && logMessage[i + 16] == 'o' && logMessage[i + 17] == 'l' && logMessage[i + 18] == ':')
                            {
                                logMessage1 = logMessage.Substring(0, i);
                                logMessage = logMessage.Substring(i + 20);
                                logMessage = logMessage.Replace("in control", "                                 ");
                                logMessage = logMessage.Replace("\" '", "\"\n'");
                                break;
                            }
                        }
                        d.Add(new Paragraph(logMessage1, fontDesc));
                        d.Add(new Paragraph("Hotkey Table:", fontTitle));
                        d.Add(new Paragraph("Access Key        Control Name", fontTitle));
                        d.Add(new Paragraph(logMessage, fontDesc));
                    }
                }
                else
                {
                    d.Add(new Paragraph(logMessage, fontDesc));
                }
                d.Add(img);
                d.Close();
                //Response.Write(string.Format("[S]:{0}", "Successful to create pdf file:" + file));
                fileoutPath = file;
                Response.Write(file);
            }
            catch (Exception ex)
            {
                //Response.Write(string.Format("[E]:{0}", ex.Message));
                Response.Write(string.Format("Exception:{0}", ex.Message));
            }
        }


        public void Ajax_GetRelatedBugs(int resultID)
        {
            string ConnectState = string.Empty;
            try
            {
                //process
                SulpHurEntities entities = new SulpHurEntities();

                string retBugs1 = string.Empty;
                string retBugs2 = string.Empty;
                string retBugs3 = string.Empty;
                string retBugs4 = string.Empty;
                string retBugs5 = string.Empty;
                string retBugs = string.Empty;

                int uiContentID = 0;
                string assemblyName = string.Empty;
                string fullTypeName = string.Empty;
                string ruleName = string.Empty;
                string ruleDescription = string.Empty;
                string Lan = string.Empty;
                string buildNo = string.Empty;
                this.GetPageIdentifierAssemblyInfo(resultID, out uiContentID, out assemblyName, out fullTypeName);
                this.GetRuleInfo(resultID, out ruleName, out ruleDescription);
                this.GetBuildInfo(resultID, out Lan, out buildNo);

                var list = from r in entities.bugs
                           join a in entities.bug_surface on r.bug_id equals a.bug_id
                           join b in entities.surfaces on a.s_id equals b.s_id
                           where b.assembly_name == assemblyName && b.class_name == fullTypeName orderby r.bug_id descending
                           select r;

                //convert format for ajax return
                foreach (bug item in list)
                {
                    if (string.IsNullOrEmpty(item.title))
                    {
                        item.title = string.Empty;
                    }
                    //[|] is separator for data field
                    if (item.title.Contains(ruleName) && item.title.Contains(Lan))
                    {
                        retBugs1 = string.Format("{0}{1}[|]{2}[|]{3}\n", retBugs1, item.bug_id, item.title, item.status);
                    }
                    if (item.title.Contains(ruleName) && !item.title.Contains(Lan) && !item.title.Contains("ENU"))
                    {
                        retBugs2 = string.Format("{0}{1}[|]{2}[|]{3}\n", retBugs2, item.bug_id, item.title, item.status);
                    }
                    if (item.title.Contains(ruleName) && !item.title.Contains(Lan) && item.title.Contains("ENU"))
                    {
                        retBugs5 = string.Format("{0}{1}[|]{2}[|]{3}\n", retBugs5, item.bug_id, item.title, item.status);
                    }
                    if (!item.title.Contains(ruleName) && item.title.Contains(Lan))
                    {
                        retBugs3 = string.Format("{0}{1}[|]{2}[|]{3}\n", retBugs3, item.bug_id, item.title, item.status);
                    }
                    if (!item.title.Contains(ruleName) && !item.title.Contains(Lan))
                    {
                        retBugs4 = string.Format("{0}{1}[|]{2}[|]{3}\n", retBugs4, item.bug_id, item.title, item.status);
                    }
                }

                if (!string.IsNullOrEmpty(retBugs1))
                {
                    retBugs1 = string.Format("{0}[|]{1}{2}\n", "Same Rule and Same Languages:", "================\n", retBugs1);
                }
                if (!string.IsNullOrEmpty(retBugs4))
                {
                    retBugs1 = string.Format("{0}{1}[|]{2}\n", retBugs1, "Different Rule and different Languages:", "================");
                }
                if (!string.IsNullOrEmpty(retBugs5))
                {
                    retBugs4 = string.Format("{0}{1}[|]{2}\n", retBugs4, "Same Rule and core code bug:", "================");
                }
                if (!string.IsNullOrEmpty(retBugs2))
                {
                    retBugs5 = string.Format("{0}{1}[|]{2}\n", retBugs5, "Same Rule but different Languages:", "================");
                }
                if (!string.IsNullOrEmpty(retBugs3))
                {
                    retBugs2 = string.Format("{0}{1}[|]{2}\n", retBugs2, "Different Rule but same Languages:", "================");
                }
                retBugs = retBugs1 + retBugs4 + retBugs5 + retBugs2 + retBugs3;
                if (!string.IsNullOrEmpty(retBugs))
                {
                    retBugs = retBugs.TrimEnd('\n');
                }

                Response.Write(retBugs);
            }
            catch (SulpHurErrorException ex)
            {
                Response.Write(string.Format("[E]:{0}", ex.Message));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
                if (ConnectState == null)
                {
                    Response.Write(string.Format("[E]:Unknown error."));
                }
                else
                {
                    Response.Write(string.Format("[E]:{0}", ConnectState));
                }
            }
        }
        public void Ajax_GetAssemblyInfo(int resultID)
        {
            string retValue = string.Empty;

            try
            {
                SulpHurEntities entities = new SulpHurEntities();

                //get assembly name and full type name
                var list = from r in entities.Results
                           join a in entities.AssemblyLinks on r.ContentID equals a.ContentID into Assembly_UIContent
                           from au in Assembly_UIContent.DefaultIfEmpty()
                           where r.ResultID == resultID
                           select au;
                var results = list.ToList();
                if (results.FirstOrDefault() == null)
                {
                    throw new SulpHurErrorException("Failed due to no assembly info.");
                }
                //retrieve assembly info
                foreach (var assemblyLink in results)
                {
                    retValue = string.Format("{0}[|]{1}[|]{2}\n{3}", assemblyLink.AssemblyInfo.AssemblyName, assemblyLink.AssemblyInfo.FullTypeName, assemblyLink.IsPageIdentifier, retValue);
                }
                retValue = retValue.TrimEnd('\n');

                Response.Write(retValue);
            }
            catch (SulpHurErrorException ex)
            {
                Response.Write(string.Format("[E]:{0}", ex.Message));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
                Response.Write(string.Format("[E]:{0}", "Unknown error."));
            }
        }
        public void Ajax_GetReproSteps(int resultID)
        {
            string retValue = string.Empty;

            try
            {
                SulpHurEntities entities = new SulpHurEntities();

                //get LaunchedFrom and WindowHierarchy
                var list = from r in entities.Results
                           join u in entities.UIContents on r.ContentID equals u.ContentID
                           where r.ResultID == resultID
                           select new { u.LaunchedFrom, u.WindowHierarchy };
                var results = list.ToList();
                var reproSteps = results.FirstOrDefault();
                if (reproSteps == null)
                {
                    throw new SulpHurErrorException("Failed due to no UI content found.");
                }
                retValue = string.Format("{0}[|]{1}", reproSteps.LaunchedFrom, reproSteps.WindowHierarchy);

                //get custom repro step
                var customReproStepList = from r in entities.Results
                                          join al in entities.AssemblyLinks on r.ContentID equals al.ContentID
                                          join ai in entities.AssemblyInfoes on al.TypeID equals ai.TypeID
                                          join bt in entities.BuildTypes on true equals true
                                          where bt.AssemblyName == ai.AssemblyName &&
                                                bt.TypeName == ai.FullTypeName &&
                                                al.IsPageIdentifier == true &&
                                                r.ResultID == resultID
                                          orderby bt.BuildNo descending
                                          select new { bt.LanuchSteps };
                var customReproStepResults = customReproStepList.ToList();
                var customReproStep = customReproStepResults.FirstOrDefault();
                if (customReproStep == null)
                {
                    retValue = string.Format("{0}[|]{1}", retValue, "-1");
                }
                else
                {
                    retValue = string.Format("{0}[|]{1}", retValue, customReproStep.LanuchSteps);
                }

                Response.Write(retValue);
            }
            catch (SulpHurErrorException ex)
            {
                Response.Write(string.Format("[E]:{0}", ex.Message));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
                Response.Write(string.Format("[E]:{0}", "Unknown error."));
            }
        }
        public void Ajax_UpdateReproSteps(int resultID, string launchedFrom, string windowHierarchy, string CustomReproStep)
        {
            string retValue = string.Empty;

            try
            {
                SulpHurEntities entities = new SulpHurEntities();

                //Result
                Result result = entities.Results.FirstOrDefault(item => item.ResultID == resultID);
                if (result == null)
                {
                    throw new SulpHurErrorException("Failed due to no result found by the result ID.");
                }
                //UIContent
                UIContent content = entities.UIContents.FirstOrDefault(item => item.ContentID == result.ContentID);
                if (content == null)
                {
                    throw new SulpHurErrorException("Failed due to no UI content found.");
                }
                content.LaunchedFrom = launchedFrom;
                content.WindowHierarchy = windowHierarchy;

                //save UIContent
                entities.SaveChanges();

                //BuildTypes
                var customReproStepList = from r in entities.Results
                                          join al in entities.AssemblyLinks on r.ContentID equals al.ContentID
                                          join ai in entities.AssemblyInfoes on al.TypeID equals ai.TypeID
                                          join bt in entities.BuildTypes on true equals true
                                          where bt.AssemblyName == ai.AssemblyName &&
                                                bt.TypeName == ai.FullTypeName &&
                                                al.IsPageIdentifier == true &&
                                                r.ResultID == resultID
                                          orderby bt.BuildNo descending
                                          select new { bt };
                var customReproStepResults = customReproStepList.ToList();
                var customReproStep = customReproStepResults.FirstOrDefault();
                if (customReproStep != null && customReproStep.bt != null)
                {
                    customReproStep.bt.LanuchSteps = CustomReproStep;
                    entities.SaveChanges();
                }
            }
            catch (SulpHurErrorException ex)
            {
                Response.Write(string.Format("[E]:{0}", ex.Message));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
                Response.Write(string.Format("[E]:{0}", "Unknown error."));
            }
        }
        public void Ajax_GetBuildLanguage(string buildNos)
        {
            try
            {
                if (string.IsNullOrEmpty(buildNos))
                {
                    throw new SulpHurErrorException("BuildNo cannot be empty.");
                }

                SulpHurEntities entities = new SulpHurEntities();
                string retValue = "";

                string[] buildNoArr = buildNos.Split(',');
                var buildLanguages = entities.BuildInfoes.Where(item => buildNoArr.Contains(item.BuildNo)).GroupBy(item => item.Language).Select(item => item.Key).OrderBy(item => item);
                foreach (var buildLanguage in buildLanguages)
                {
                    if (string.IsNullOrEmpty(retValue))
                    {
                        retValue = buildLanguage;
                    }
                    else
                    {
                        retValue = string.Format("{0}[|]{1}", retValue, buildLanguage);
                    }
                }

                Response.Write(retValue);
            }
            catch (SulpHurErrorException ex)
            {
                Response.Write(string.Format("[E]:{0}", ex.Message));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
                Response.Write(string.Format("[E]:Unknown error."));
            }
        }
        public void Ajax_GetFullTypeName(string assemblyNames)
        {
            try
            {
                if (string.IsNullOrEmpty(assemblyNames))
                {
                    throw new SulpHurErrorException("AssemblyName cannot be empty.");
                }

                SulpHurEntities entities = new SulpHurEntities();
                string retValue = "";

                //select
                var assemblyInfos = from ai in entities.AssemblyInfoes
                                    select ai;
                //where
                if (assemblyNames != "-1")
                {
                    string[] assemblyNameArr = assemblyNames.Split(',');
                    assemblyInfos = assemblyInfos.Where(item => assemblyNameArr.Contains(item.AssemblyName));
                }
                //orderby
                assemblyInfos = assemblyInfos.OrderBy(item => item.FullTypeName);
                //read info
                foreach (var assemblyInfo in assemblyInfos)
                {
                    if (string.IsNullOrEmpty(retValue))
                    {
                        retValue = string.Format("{0}[|]{1}", assemblyInfo.TypeID, assemblyInfo.FullTypeName);
                    }
                    else
                    {
                        retValue = string.Format("{0}\n{1}[|]{2}", retValue, assemblyInfo.TypeID, assemblyInfo.FullTypeName);
                    }
                }

                Response.Write(retValue);
            }
            catch (SulpHurErrorException ex)
            {
                Response.Write(string.Format("[E]:{0}", ex.Message));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
                Response.Write(string.Format("[E]:Unknown error."));
            }
        }
        private bool HasCache(int resultID)
        {
            string imageFolder = @"Tmp_ResultImage";
            string localImageFolder = Path.Combine(Context.Server.MapPath(""), imageFolder);
            string localImagePath = Path.Combine(localImageFolder, string.Format("{0}.jpg", resultID));

            if (System.IO.File.Exists(localImagePath))
                return true;

            return false;
        }

        private void GetDetailInfo(int resultID, out string logMessage, out string screenshotPath)
        {
            logMessage = null;
            screenshotPath = null;

            //string imagePath = string.Format(@"Tmp_ResultImage\{0}.jpg", resultID);
            string imagePath = string.Format(@"Tmp_ResultImage/{0}.jpg", resultID);
            string localImagePath = Path.Combine(Context.Server.MapPath(""), imagePath);
            //System.Diagnostics.Trace.WriteLine(imagePath);
            //System.Diagnostics.Trace.WriteLine(localImagePath);
            string resultLog = string.Empty;
            SulpHurEntities entities = new SulpHurEntities();
            List<ResultReport> resultReportList = new List<ResultReport>();

            if (this.HasCache(resultID))
            {
                //only get result log
                resultLog = (from row in entities.Results
                             where row.ResultID.Equals(resultID)
                             select row.ResultLog).FirstOrDefault();
            }
            else
            {
                //result image
                Result result = entities.Results.FirstOrDefault(item => item.ResultID.Equals(resultID));
                System.Drawing.Image image = null;
                if (result.ResultImage == null)
                {
                    var screenshot = from item in entities.UIContents
                                     where item.ContentID.Equals(result.ContentID)
                                     select item.UIScreenShot;
                    //AllUIContent uiContent = entities.AllUIContents.FirstOrDefault(item => item.ContentID.Equals(result.ContentID));
                    if (screenshot != null)
                    {
                        image = Bitmap.FromStream(new MemoryStream(screenshot.First()));
                    }
                }
                else
                {
                    image = Bitmap.FromStream(new MemoryStream(result.ResultImage));
                }
                image.Save(localImagePath, System.Drawing.Imaging.ImageFormat.Jpeg);

                //result log
                resultLog = result.ResultLog;
            }

            if (string.IsNullOrEmpty(resultLog))
            {
                resultLog = "No any error on this page.";
            }

            //return
            logMessage = resultLog;
            screenshotPath = imagePath;
        }
        public void GetPageIdentifierAssemblyInfo(int resultID, out int contentID, out string assemblyName, out string fullTypeName)
        {
            SulpHurEntities entities = new SulpHurEntities();
            contentID = 0;
            assemblyName = string.Empty;
            fullTypeName = string.Empty;

            //get assembly name and full type name
            //var list = entities.Results
            //    .Where(row => row.ResultID == resultID)
            //    .Join(entities.UIContents, r => r.ContentID, u => u.ContentID, (r, u) => new { u.ContentID })
            //    .Join(entities.AssemblyLinks, u => u.ContentID, a => a.ContentID, (u, a) => new { u.ContentID, a.AssemblyInfo.AssemblyName, a.AssemblyInfo.FullTypeName, a.IsPageIdentifier })
            //    .Where(item => item.IsPageIdentifier == true);

            //Maybe the Assembly is empty,attach the bugid will cause exception. bug:8943245
            var list = from r in entities.Results
                       where r.ResultID == resultID
                       join u in entities.UIContents on r.ContentID equals u.ContentID
                       select new { r.ContentID };
            var result = list.FirstOrDefault();
            if (result == null)
            {
                //throw new SulpHurErrorException("Failed due to no page identifier assembly info.");
            }
            else
            {
                contentID = result.ContentID;
                //assemblyName = result.AssemblyName;
                //fullTypeName = result.FullTypeName;
            }
            var aResult = entities.AssemblyLinks.Where(r => r.ContentID == result.ContentID).Select(r => new { r.AssemblyInfo.AssemblyName, r.AssemblyInfo.FullTypeName }).FirstOrDefault();
            if (aResult != null)
            {
                assemblyName = aResult.AssemblyName;
                fullTypeName = aResult.FullTypeName;
            }

        }
        public void GetAssemblyInfo(int resultID, out string assemblyname, out string fulltypename)
        {
            SulpHurEntities entities = new SulpHurEntities();
            var list = entities.Results
                 .Where(row => row.ResultID == resultID)
                 .Join(entities.AssemblyLinks, r => r.ContentID, a => a.ContentID, (r, a) => new { r.ContentID, a.AssemblyInfo.AssemblyName, a.AssemblyInfo.FullTypeName, a.IsPageIdentifier })
                 .OrderByDescending(row => row.IsPageIdentifier);
            var result = list.FirstOrDefault();
            if (result == null)
            {
                assemblyname = null;
                fulltypename = null;
                //throw new SulpHurErrorException("Failed due to no assembly info.");
            }
            else
            {
                assemblyname = result.AssemblyName;
                fulltypename = result.FullTypeName;
            }
        }
        private void GetResultTypeByInfo(string rule, string assembly, string fulltypename, out string LatestbuildNo, string buildLanguage, out string resultType)
        {
            SulpHurEntities entities = new SulpHurEntities();
            var list = entities.Results
                .Where(row => row.Rule.RuleName == rule)
                .Join(entities.Rules, re => re.RuleID, ru => ru.RuleID, (re, ru) => new { re.RuleID, re.ContentID, ru.RuleName, re.ResultType })
                .Join(entities.AssemblyLinks, c => c.ContentID, al => al.ContentID, (c, al) => new { c.ContentID, al.IsPageIdentifier, al.TypeID, c.ResultType })
                .Where(item => item.IsPageIdentifier == true)
                .Join(entities.AssemblyInfoes, al => al.TypeID, ai => ai.TypeID, (al, ai) => new { al.ContentID, al.ResultType, ai.AssemblyName, ai.FullTypeName })
                .Where(row => row.FullTypeName == fulltypename && row.AssemblyName == assembly)
                .Join(entities.UIContents, re => re.ContentID, c => c.ContentID, (re, c) => new { re.ContentID, c.BuildID, re.ResultType })
                .Join(entities.BuildInfoes, c => c.BuildID, b => b.BuildID, (c, b) => new { b.BuildNo, b.Language, c.ResultType })
                .Where(row => row.Language == buildLanguage)
                .Distinct()
                .OrderByDescending(row => row.BuildNo);
            var result = list.FirstOrDefault();

            if (result == null)
            {
                LatestbuildNo = "0";
                resultType = "";
            }
            else
            {
                list = list
                .Where(row => row.BuildNo == result.BuildNo)
                .OrderBy(row => row.ResultType);
                result = list.FirstOrDefault();
                LatestbuildNo = result.BuildNo;
                resultType = result.ResultType;
            }
        }
        // v-yiwzha bug 464795 Add get buildlanguage, not client OSlanguage
        // v-yiwzha bug 465236 Get Build No by resultID.
        private void GetBuildInfo(int resultID, out string buildLanguage, out string buildNo)
        {
            SulpHurEntities entities = new SulpHurEntities();
            buildLanguage = string.Empty;

            //get buildInfo
            var list = entities.Results
                .Where(row => row.ResultID == resultID)
                .Join(entities.UIContents, r => r.ContentID, u => u.ContentID, (r, u) => new { u.BuildID })
                .Join(entities.BuildInfoes, u => u.BuildID, c => c.BuildID, (u, c) => new { c.Language, c.BuildNo });
            var li = list.FirstOrDefault();
            if (li == null)
            {
                throw new SulpHurErrorException("Failed to get buildLanguage Info!");
            }
            else
            {
                buildLanguage = li.Language;
                buildNo = li.BuildNo;
            }
        }
        // v-yiwzha bug 465236 Get OSType by resultID.
        public void GetUIInfo(int resultID, out string uiName, out string reprostep, out string OSLanguage, out string OSType)
        {
            SulpHurEntities entities = new SulpHurEntities();
            uiName = string.Empty;
            reprostep = string.Empty;
            OSLanguage = string.Empty;

            //get UIInfo
            var list = entities.Results
                .Where(row => row.ResultID == resultID)
                .Join(entities.UIContents, r => r.ContentID, u => u.ContentID, (r, u) => new { u.ClientID, u.UIName, u.LaunchedFrom })
                .Join(entities.Clients, u => u.ClientID, c => c.ClientID, (u, c) => new { u.UIName, u.LaunchedFrom, c.OSLanguage, c.OSType });
            var li = list.FirstOrDefault();
            if (li == null)
            {
                throw new SulpHurErrorException("Failed to get UI Name/Language/reprostep Info!");
            }
            else
            {
                uiName = li.UIName;
                reprostep = li.LaunchedFrom;
                OSLanguage = li.OSLanguage;
                OSType = li.OSType;
            }
        }
        private void GetResultType(int resultID, out string resultType)
        {
            SulpHurEntities entities = new SulpHurEntities();
            resultType = string.Empty;

            //get ResultType
            var list = entities.Results
                .Where(row => row.ResultID == resultID);

            var li = list.FirstOrDefault();
            if (li == null)
            {
                throw new SulpHurErrorException("Failed to get Result Info!");
            }
            else
            {
                resultType = li.ResultType;
            }
        }

        private void GetRuleInfo(int resultID, out string rulename, out string ruledescription)
        {
            SulpHurEntities entities = new SulpHurEntities();
            rulename = string.Empty;
            ruledescription = string.Empty;

            //get RuleInfo
            var list = entities.Results
                .Where(row => row.ResultID == resultID)
                .Join(entities.Rules, re => re.RuleID, ru => ru.RuleID, (re, ru) => new { re.RuleID, ru.RuleName, ru.RuleDesc });
            var li = list.FirstOrDefault();
            if (li == null)
            {
                throw new SulpHurErrorException("Failed to get Rule Info!");
            }
            else
            {
                rulename = li.RuleName;
                ruledescription = li.RuleDesc;
            }
        }

        private void GetReviewLog(int resultID, out string reviewlog)
        {
            SulpHurEntities entities = new SulpHurEntities();
            reviewlog = string.Empty;

            //get reviewlog
            var list = entities.Results
                .Where(row => row.ResultID == resultID);
            var li = list.FirstOrDefault();
            if (li == null)
            {
                throw new SulpHurErrorException("Failed to get ReviewLog!");
            }
            else
            {
                reviewlog = li.ReviewLog;
            }
        }

        private void LinkToBug(int bugId, string assemblyName, string fullTypeName)
        {
            using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
            {
                sqlConnection.Open();

                using (SqlCommand sqlCommand = new SqlCommand("spLinkBug", sqlConnection))
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.Add(new SqlParameter("@bug_id", SqlDbType.Int) { Value = bugId });
                    sqlCommand.Parameters.Add(new SqlParameter("@class_name", SqlDbType.VarChar) { Value = fullTypeName });
                    sqlCommand.Parameters.Add(new SqlParameter("@assembly_name", SqlDbType.VarChar) { Value = assemblyName });

                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        public void Ajax_GetAvailableBuildNo()
        {
            using (SulpHurEntities entity = new SulpHurEntities())
            {
                var list = (from x in entity.BuildInfoes
                            select x.BuildNo).Distinct().OrderByDescending(n => n);
                string result = string.Empty;
                foreach (string x in list)
                {
                    result += x.Trim();
                    result += "[|]";
                }
                result = result.Substring(0, result.Length - "[|]".Length);
                Response.Write(result);
            }
        }
        public void Ajax_GetAvailableBuildLanguage()
        {
            using (SulpHurEntities entity = new SulpHurEntities())
            {
                var list = (from x in entity.BuildInfoes
                            select x.Language).Distinct().OrderByDescending(n => n);

                string result = string.Empty;
                foreach (string x in list)
                {
                    result += x.Trim();
                    result += "[|]";
                }
                result = result.Substring(0, result.Length - "[|]".Length);
                Response.Write(result);
            }
        }
        public void Ajax_GetAvailableOSType()
        {
            using (SulpHurEntities entity = new SulpHurEntities())
            {
                var list = (from x in entity.Clients
                            select x.OSType).Distinct().OrderByDescending(n => n);

                string result = string.Empty;
                foreach (string x in list)
                {
                    result += x.Trim();
                    result += "[|]";
                }
                result = result.Substring(0, result.Length - "[|]".Length);
                Response.Write(result);
            }
        }
        public void Ajax_GetAvailableOSLanguage()
        {
            using (SulpHurEntities entity = new SulpHurEntities())
            {
                var list = (from x in entity.Clients
                            select x.OSLanguage).Distinct().OrderByDescending(n => n);

                string result = string.Empty;
                foreach (string x in list)
                {
                    result += x.Trim();
                    result += "[|]";
                }
                result = result.Substring(0, result.Length - "[|]".Length);
                Response.Write(result);
            }
        }
        public void Ajax_GetAvailableRule()
        {
            using (SulpHurEntities entity = new SulpHurEntities())
            {
                var list = (from x in entity.Rules
                            select x.RuleName).Distinct();
                string result = string.Empty;
                foreach (string x in list)
                {
                    result += x.Trim();
                    result += "[|]";
                }
                result = result.Substring(0, result.Length - "[|]".Length);
                Response.Write(result);
            }
        }
        public void Ajax_GetAvailableFullTypeName()
        {
            using (SulpHurEntities entity = new SulpHurEntities())
            {
                var list = (from x in entity.AssemblyInfoes
                            select x.FullTypeName).Distinct().OrderByDescending(n => n);
                string result = string.Empty;
                foreach (string x in list)
                {
                    result += x.Trim();
                    result += "[|]";
                }
                result = result.Substring(0, result.Length - "[|]".Length);
                Response.Write(result);
            }
        }
        public void Ajax_GetAvailableAssemblyName()
        {
            using (SulpHurEntities entity = new SulpHurEntities())
            {
                var list = (from x in entity.AssemblyInfoes
                            select x.AssemblyName).Distinct().OrderByDescending(n => n);
                string result = string.Empty;
                foreach (string x in list)
                {
                    result += x.Trim();
                    result += "[|]";
                }
                result = result.Substring(0, result.Length - "[|]".Length);
                Response.Write(result);
            }
        }

        public void GetDirectoryList()
        {
            string UIDiffResultPathBase = ConfigurationManager.AppSettings["UIDiffResultPathBase"];
            string UIDiffResultPathRelated = ConfigurationManager.AppSettings["UIDiffResultPathRelated"];
            string[] directoryList = System.IO.Directory.GetDirectories(UIDiffResultPathBase, "*", SearchOption.TopDirectoryOnly);
            string results = string.Empty;
            string buildpath = string.Empty;
            if (directoryList != null)
            {
                foreach (string path in directoryList)
                {
                    buildpath = path.Substring(path.LastIndexOf("\\") + 1);
                    if (string.IsNullOrEmpty(results))
                    {
                        results = buildpath;
                    }
                    else
                    {
                        results += "|" + buildpath;
                    }
                }
            }
            Response.Write(results);
        }

        // Standard format: 5.0.8847.1000; ignore the last minor version
        public bool IsStandardFormat(string buildString)
        {
            string standardFormat = @"^5.0.[0-9]{4}.[0-9]{3}[0-9]+$";
            return Regex.IsMatch(buildString, standardFormat);
        }
    }
}