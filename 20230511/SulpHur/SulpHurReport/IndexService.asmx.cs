using Newtonsoft.Json;
using SulpHurReport.Models;
using SulpHurReport.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Services;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Web.UI.WebControls;

namespace SulpHurReport
{
    /// <summary>
    /// Summary description for IndexService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class IndexService : System.Web.Services.WebService
    {
        private static List<string> osRelatedRuleIDs;
        private static List<string> osNotRelatedRuleIDs;
        string connStr = ConfigurationManager.ConnectionStrings["ADOConn"].ToString();

        [WebMethod]
        public int UpdateReviewedByResultID(string resultID, string reviewflag, string reviewlog)
        {
            int result = 0;
            string sql = string.Format("Update Results set ReviewFlag ='{0}',ReviewLog = '{1}' ", int.Parse(reviewflag), reviewlog);
            sql = sql + "where ResultID in (" + resultID.Trim(',') + ")";
            result = SQLUtility.ExecuteScalar(sql, true);
            return result;
        }
        
        //public int UpdateReviewedByResultID(string[] resultIDs, string reviewflag, string reviewlog)
        //{

        //    return 0;
        //}

        [WebMethod]
        public Models.ResultDetailInfo GetResultDetailInfo(int resultID)
        {
            Models.ResultDetailInfo resultDetailInfo = SQLUtility.GetResultInfo(resultID);
            return resultDetailInfo;
        }

        [WebMethod]
        public string GetScreenshotPath(int resultID)
        {
            string imagePath = string.Format(@"Tmp_ResultImage/{0}.jpg", resultID);
            string localImagePath = Path.Combine(Server.MapPath(""), imagePath);
            if (!File.Exists(localImagePath))
            {
                byte[] resultImage = SQLUtility.GetResultScreenshot(resultID);
                System.Drawing.Image image = null;
                if (resultImage == null)
                {
                    resultImage = SQLUtility.GetUIScreenshot(resultID);
                    if (resultImage != null)
                        image = Bitmap.FromStream(new MemoryStream(resultImage));
                }
                else
                    image = Bitmap.FromStream(new MemoryStream(resultImage));
                image.Save(localImagePath, ImageFormat.Jpeg);
            }
            return imagePath;
        }

        [WebMethod]
        public int GetResultNumber(string buildNOs, string buildLans, string osTypes, string osLanguages, string ruleIDs,
            string resultTypes, string assemblyNames, string typeIDs,string ReviewedStatus,string searchDateTime, bool getLatest,
            int latestDays/*, string parameter*/)
        {
            if (typeIDs == "All")
                typeIDs = "-1";
            ruleIDs = ruleIDs.Replace("'", "");
            SQLUtility.osRelatedRuleIDs = osRelatedRuleIDs;
            SQLUtility.osNotRelatedRuleIDs = osNotRelatedRuleIDs;
            int count = SQLUtility.GetCountOfTotalRecords(buildNOs, buildLans, osTypes, osLanguages, ruleIDs, resultTypes, assemblyNames, typeIDs,
                ReviewedStatus, searchDateTime, getLatest, latestDays);
            return count;
        }
        [WebMethod]
        public List<QueryResult> GetConditionQueryResults(string buildNOs, string buildLans, string osTypes, string osLanguages, string ruleIDs,
            string resultTypes, string assemblyNames, string typeIDs, string ReviewedStatus, string searchDateTime, bool getLatest, int latestDays,
            string sortBy,string currentPageResults,string pageIndex/*, string parameter*/)
        {
            if (typeIDs == "All")
                typeIDs = "-1";
            ruleIDs = ruleIDs.Replace("'", "");
            SQLUtility.osRelatedRuleIDs = osRelatedRuleIDs;
            SQLUtility.osNotRelatedRuleIDs = osNotRelatedRuleIDs;
            List<QueryResult> resultReportList = SQLUtility.GetDetailResultsOfSpecifiedPage(buildNOs, buildLans, osTypes, osLanguages, ruleIDs,
                resultTypes, assemblyNames, typeIDs, ReviewedStatus,  sortBy, searchDateTime,
                currentPageResults, "0", getLatest, latestDays, pageIndex);
            return resultReportList;
        }

        [WebMethod]
        public int GetSearchResultNumber(string parameter, string searchDateTime, int latestDays/*, string pageIndex*/)
        {
            int count = 0;
            string sql = string.Empty;
            bool useResultID = false;
            if (System.Text.RegularExpressions.Regex.IsMatch(parameter, @"\d"))
            {                
                useResultID = true;
                //bool previous = false;
                //string temp = string.Empty;
                //for (int i = 0; i < parameter.Length; i++)
                //{
                //    if (Char.IsDigit(parameter[i]))
                //    {
                //        temp += parameter[i];
                //        previous = true;
                //    }
                //    else
                //    {
                //        if (i == 0)
                //        {

                //        }
                //        else if (i == parameter.Length - 1)
                //        {
                //            if (temp.EndsWith(",")) temp = temp.TrimEnd(',');
                //        }
                //        else
                //        {
                //            if (previous == true)
                //            {
                //                temp += ',';
                //                previous = false;
                //            }
                //        }
                //    }
                //}
                parameter = GetFormatResultID(parameter);               
                sql = SQLUtility.GenerateCountSQLByResultID(parameter);                
            }
            else if (parameter.StartsWith("v-"))
            {
                string whereClauseCommon = string.Format("{0}{1}",
                SQLUtility.GenerateUserNameFilterClause(parameter),
                SQLUtility.GenerateSearchDatetimeFilterClause(searchDateTime, latestDays));
                sql = string.Format(SQLUtility.sqlCountCommon, whereClauseCommon);                
            }
            else
            {
                string whereClauseCommon = string.Format("{0}{1}",
                SQLUtility.GeneratePageTitleFilterClause(parameter),
                SQLUtility.GenerateSearchDatetimeFilterClause(searchDateTime, latestDays));
                sql = string.Format(SQLUtility.sqlCountCommon, whereClauseCommon);
            }
            count = SQLUtility.GetCountOfTotalRecords(sql);
            if (useResultID)
            {
                return 1;
            }
            return count;
        }

        [WebMethod]
        public List<QueryResult> GetSearchResults(string parameter,string searchDateTime, int latestDays,string pageIndex)
        {
            string sql = string.Empty;
            //string connStr = ConfigurationManager.ConnectionStrings["ADOConn"].ToString();

            if (pageIndex == "undefined")
                pageIndex = "1";
            int page = Int32.Parse(pageIndex);
           // int diffentReviewStatusResults = int.Parse(diffentReviewStatusResultsCount);
            int actualStartIndex = (page - 1) * 20 + 1;
            int actualEndIndex = page * 20;           

            if ((System.Text.RegularExpressions.Regex.IsMatch(parameter, @"\d")))
            {
                parameter = GetFormatResultID(parameter);
                sql = SQLUtility.GenerateListSQLByResultID(parameter);
            }
            else if(parameter.StartsWith("v-")){
                string whereClauseCommon = string.Format("{0}{1}",
                SQLUtility.GenerateUserNameFilterClause(parameter),
                SQLUtility.GenerateSearchDatetimeFilterClause(searchDateTime, latestDays));
                sql = string.Format(SQLUtility.sqlResultsCommonWithAssembly, whereClauseCommon, actualStartIndex, actualEndIndex);
            }
            else
            {
                string whereClauseCommon = string.Format("{0}{1}",
                SQLUtility.GeneratePageTitleFilterClause(parameter),
                SQLUtility.GenerateSearchDatetimeFilterClause(searchDateTime, latestDays));
                sql = string.Format(SQLUtility.sqlResultsCommonWithAssembly, whereClauseCommon, actualStartIndex, actualEndIndex);
            }

            List<QueryResult> resultList = new List<QueryResult>();
            resultList = SQLUtility.ExecuteScalar(connStr, sql, true);            
            return resultList;            
        }

        [WebMethod]
        public object QueryAvailableData()
        {
            try
            {
                var hash = new Dictionary<string, object>();
                IProductQuery pq = new ProductQuery();
                hash.Add("AvailableCapturedBuilds", pq.QueryAvailableCapturedBuilds());
                hash.Add("AvailableCapturedLanguages", pq.QueryAvailableCapturedLanguages());
                hash.Add("AvailableOSTypes", pq.QueryAvailableOSTypes());
                hash.Add("AvailableOSLanguage", pq.QueryAvailableOSLanguage());
                Dictionary<string, Rule> availableRules = pq.QueryAvailableRules();
                hash.Add("AvailableRules", availableRules);
                string osRelatedRules = ConfigurationManager.AppSettings["OSRelatedRules"].ToString();
                string[] osRelatedRuleList = osRelatedRules.Split('|');
                osRelatedRuleIDs = GetRuleIDsList(osRelatedRuleList, availableRules);
                string osNotRelatedRules = ConfigurationManager.AppSettings["OSNotRelatedRules"].ToString();
                string[] osNotRelatedRuleList = osNotRelatedRules.Split('|');
                osNotRelatedRuleIDs = GetRuleIDsList(osNotRelatedRuleList, availableRules);
                hash.Add("AssemblyInfo", pq.QueryAssemblyTypesInfo());
                hash.Add("AvailableAssembly", pq.QueryAvailableAssembly());
                hash.Add("AvailableTypes", pq.QueryAvailableTypes());
                return new { hash = hash };
            }
            catch (Exception e)
            {
                return e;
            }
        }
        [WebMethod]
        public int GetTotalRecords(string buildNO, string buildLan, string osType, string osLanguage, string ruleIDs,
            string resultTypes, string assemblyName, string typeIDs, string username, string pagetitle, string reviewComments,
            string ReviewedType, string resultid, string searchDateTime, bool getLatest, int latestDays)
        {
            if (typeIDs == "All")
                typeIDs = "-1";
            ruleIDs = ruleIDs.Replace("'", "");
            SQLUtility.osRelatedRuleIDs = osRelatedRuleIDs;
            SQLUtility.osNotRelatedRuleIDs = osNotRelatedRuleIDs;
            int count = SQLUtility.GetCountOfTotalRecords(buildNO, buildLan, osType, osLanguage, ruleIDs, resultTypes, assemblyName, typeIDs,
                username, pagetitle, reviewComments, ReviewedType, resultid, searchDateTime, getLatest, latestDays);
            return count;
        }
        [WebMethod]
        public List<QueryResult> BindingTable(string buildNO, string buildLan, string osType, string osLanguage, string ruleIDs,
            string resultTypes, string assemblyName, string typeIDs, string pageIndex, string username, string pagetitle, string reviewComments,
            string ReviewedType, string resultid, string sortBy, string searchDateTime, string currentPageResults,
            string diffentReviewStatusResultsCount, bool getLatest, int latestDays)
        {
            if (typeIDs == "All")
                typeIDs = "-1";
            SQLUtility.osRelatedRuleIDs = osRelatedRuleIDs;
            SQLUtility.osNotRelatedRuleIDs = osNotRelatedRuleIDs;
            List<QueryResult> resultReportList = SQLUtility.GetDetailResultsOfSpecifiedPage(buildNO, buildLan, osType, osLanguage, ruleIDs,
                resultTypes, assemblyName, typeIDs, pageIndex, username, pagetitle, reviewComments, ReviewedType, resultid, sortBy, searchDateTime,
                currentPageResults, diffentReviewStatusResultsCount, getLatest, latestDays);
            return resultReportList;
        }

        #region Rule ID string generator
        private List<string> GetRuleIDsList(string[] ruleList, Dictionary<string, Rule> availableRules)
        {
            List<string> ruleIDsArray = new List<string>();
            foreach (string s in ruleList)
            {
                foreach (Rule rule in availableRules.Values)
                {
                    if (rule.RuleName == s)
                    {
                        ruleIDsArray.Add(rule.RuleID.ToString());
                        break;
                    }
                }
            }
            return ruleIDsArray;
        }
        #endregion

        private  string GetFormatResultID(string resultID)
        {
            bool previous = false;
            string temp = string.Empty;
            for (int i = 0; i < resultID.Length; i++)
            {
                if (Char.IsDigit(resultID[i]))
                {
                    temp += resultID[i];
                    previous = true;
                }
                else
                {
                    if (i == 0)
                    {

                    }
                    else if (i == resultID.Length - 1)
                    {
                        if (temp.EndsWith(",")) temp = temp.TrimEnd(',');
                    }
                    else
                    {
                        if (previous == true)
                        {
                            temp += ',';
                            previous = false;
                        }
                    }
                }
            }
            return temp;
        }
    }
}
