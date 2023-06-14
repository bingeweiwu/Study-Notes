using SulpHurManagementSystem.Common;
using SulpHurManagementSystem.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SulpHurManagementSystem
{
    public partial class Ajax : System.Web.UI.Page
    {
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
                        string logMessage = SQLUtility.GetResultLog(outResult.ResultID);
                        string screenshotPath = new CaptureUIsService().GetScreenshotPath(outResult.ResultID);
                        //this.GetDetailInfo(outResult.ResultID, out logMessage, out screenshotPath);
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
        public void Ajax_GeneratePSQ(string path, int resultID)
        {
            try
            {
                SulpHurEntities entities = new SulpHurEntities();
                int uiContentID = SQLUtility.GetContentID(resultID);
                string assemblyName = string.Empty;
                string fullTypeName = string.Empty;
                List<Models.AssemblyInfo> assemblyInfoList=SQLUtility.GetAssemblyInfoes(resultID);
                if(assemblyInfoList!=null&& assemblyInfoList.Count > 0)
                {
                    assemblyName = assemblyInfoList[0].AssemblyName;
                    fullTypeName = assemblyInfoList[0].FullTypeName;
                }
                //this.GetPageIdentifierAssemblyInfo(resultID, out uiContentID, out assemblyName, out fullTypeName);
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
    }
}