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
        public List<QueryResult> GetSearchResults(string parameter,string searchDateTime, int latestDays,string pageIndex)
        {
            string sql = string.Empty;
            string connStr = ConfigurationManager.ConnectionStrings["ADOConn"].ToString();

            int page = Int32.Parse(pageIndex);
           // int diffentReviewStatusResults = int.Parse(diffentReviewStatusResultsCount);
            int actualStartIndex = (page - 1) * 20 + 1;
            int actualEndIndex = page * 20;
            //if (ReviewedType == "0" || ReviewedType == "1")
            //{
            //    actualStartIndex -= diffentReviewStatusResults;
            //    actualEndIndex -= diffentReviewStatusResults;
            //}


            if (Regex.IsMatch(parameter.Replace(" ",""), @"^(\d+,)*\d+$"))
            {
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



            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand command = new SqlCommand(sql, conn);
            command.CommandType = CommandType.Text;
            command.CommandTimeout = 300000;

            conn.Open();
            DataTable table = new DataTable();
            SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
            dataAdapter.Fill(table);

            List<QueryResult> resultList = new List<QueryResult>();
            foreach (DataRow dr in table.Rows)
            {
                QueryResult result = new QueryResult()
                {
                    ResultID = Convert.ToInt32(dr["resultid"]),
                    BuildNo = dr["buildno"].ToString(),
                    Language = dr["Language"].ToString(),
                    RuleName = dr["rulename"].ToString(),
                    ResultType = dr["ResultType"].ToString(),
                    UIName = dr["uiname"].ToString(),
                    UserName = dr["UserName"].ToString(),
                    OSType = dr["OSType"].ToString(),
                    DateUploadedStr = dr["dateuploaded"].ToString(),
                    DateCheckedStr = dr["createdate"].ToString(),
                    ReviewFlag = Convert.ToBoolean(dr["ReviewFlag"]),
                    //ReviewLog = dr["ReviewLog"].ToString()
                };
                resultList.Add(result);
            }
            conn.Close();
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
    }
}
