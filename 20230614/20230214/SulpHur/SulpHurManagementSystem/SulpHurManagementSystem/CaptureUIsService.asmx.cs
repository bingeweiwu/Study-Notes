using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Runtime.Serialization;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.Text.RegularExpressions;
using SulpHurServiceAbstract;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Threading.Tasks;
using CMSherlock.VsoOdataClient;
using SulpHurManagementSystem.Utility;

namespace SulpHurManagementSystem
{
    /// <summary>
    /// Summary description for CaptureUIsService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class CaptureUIsService : System.Web.Services.WebService
    {
        string serverName = ConfigurationManager.AppSettings["ServerName"];
        string filebugFileFolder = ConfigurationManager.AppSettings["filevsobugFileFolder"];
        string connStr = ConfigurationManager.ConnectionStrings["ADOConn"].ToString();
        string smsAccountName = ConfigurationManager.AppSettings["SMSAccountName"];

        Dictionary<string, string[]> dicLan = new Dictionary<string, string[]>()
        {
            {"CHS", new string[]{"Chinese (Simplified)", "zh-CN" }},
            {"CHT", new string[]{"Chinese (Traditional)", "zh-TW" }},
            {"CSY", new string[]{"ICP Czech", "cs-CZ" }},
            {"DEU", new string[]{"German", "de-DE" }},
            {"ENU", new string[]{"Core Code", "en-US" }},
            {"ESN", new string[]{"ICP Spanish", "es-ES" }},
            {"FRA", new string[]{"French", "fr-FR" }},
            {"HUN", new string[]{"ICP Hungarian", "hu-HU" }},
            {"ITA", new string[]{"ICP Italian", "it-IT" }},
            {"JPN", new string[]{"Japanese", "ja-JP" }},
            {"KOR", new string[]{"Korean", "ko-KR" }},
            {"NLD", new string[]{"ICP Dutch", "nl-NL" }},
            {"PLK", new string[]{"ICP Polish", "pl-PL" }},
            {"PTB", new string[]{"ICP Portuguese (Brazil)", "pt-BR" }},
            {"PTG", new string[]{"ICP Portuguese (Portugal)", "pt-PT" }},
            {"RUS", new string[]{"Russian", "ru-RU" }},
            {"SVE", new string[]{"ICP Swedish", "sv-SE" }},
            {"TRK", new string[]{"ICP Turkish", "tr-TR" }}
        };

        private static List<string> osTypeNotRelatedRuleIDs;
        private static List<string> osTypeLanguageNotRelatedRuleIDs;
        private static List<string> osRelatedRuleIDs;
        private static List<string> osNotRelatedRuleIDs;

        #region SQL sentences
        string joinTables = @" INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID]
 LEFT OUTER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID]
 LEFT OUTER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID]
 LEFT OUTER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 LEFT OUTER JOIN [AssemblyLink] ON [UIContents].[ContentID]=[AssemblyLink].[ContentID]";

        string joinTablesNoAssembly = @" INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID]
 LEFT OUTER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID]
 LEFT OUTER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID]
 LEFT OUTER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID] ";

        string sqlQueryCoreColumns = @" [Results].[ResultID], [BuildInfo].[BuildNo], [BuildInfo].[Language],
 [Rules].[RuleName], [Results].[ResultType], [UIContents].[UIName], [Clients].[UserName],
 [BuildInfo].[BuildID], [Clients].[OSType], [UIContents].[DateUploaded], [Results].[CreateDate],
 [Results].[ReviewFlag], [Results].[ReviewLog]";
        string sqlQueryCoreTableForQueryLatestResult = @" FROM [Results]
 LEFT OUTER JOIN [AssemblyLink] ON [AssemblyLink].[ContentID] = [Results].[ContentID]
 LEFT OUTER JOIN [AssemblyInfo] ON [AssemblyLink].[TypeID] = [AssemblyInfo].[TypeID]
 LEFT OUTER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID]
 LEFT OUTER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID]
 LEFT OUTER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID]
 LEFT OUTER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]";

        string sqlSelectResultIDFROM = @"SELECT [ResultID] FROM (";
        string sqlSelectColumns = @"
 SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [Rules].[RuleID], [UIContents].[UIName],
 [UIContents].[DateUploaded], [AssemblyLink].[TypeID], [Results].[ResultType], [Results].[ReviewFlag]";
        string sqlFROMResults = @"
 FROM [Results]";
        string sqlLeftJoin = @"
 LEFT OUTER JOIN [AssemblyLink] ON [AssemblyLink].ContentID = [Results].[ContentID]
 LEFT OUTER JOIN [AssemblyInfo] ON [AssemblyLink].[TypeID] = [AssemblyInfo].[TypeID]
 LEFT OUTER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 LEFT OUTER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 LEFT OUTER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 LEFT OUTER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]";
        string sqlWhereClause = @"
 WHERE [AssemblyLink].[IsPageIdentifier]=1";
        string sqlRenameTOTALAndSelectGroupColumns = @"
 ) AS [TOTAL]
 LEFT OUTER JOIN (SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [Rules].[RuleID], [UIContents].[UIName],
 [AssemblyLink].[TypeID]";
        string sqlGroupBy = @"
 GROUP BY [Rules].[RuleID], [UIContents].[UIName], [AssemblyLink].[TypeID]";
        string sqlRenameGroupTableAndOn = @"
 ) AS [GroupTable]
 ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
 AND [TOTAL].[RuleID]=[GroupTable].[RuleID] 
 AND [TOTAL].[UIName]=[GroupTable].[UIName]
 AND [TOTAL].[TypeID]=[GroupTable].[TypeID]";
        string sqlOnOSType = @"
 AND [TOTAL].[OSType]=[GroupTable].[OSType]";
        string sqlOnLanguage = @"
 AND [TOTAL].[Language]=[GroupTable].[Language]";
        string sqlWHEREClauseOnTOTALAndGroupTable = @"
 WHERE [TOTAL].[TypeID]=[GroupTable].[TypeID]";
        #endregion

        private object ExecuteScalar(string sql)
        {
            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand command = new SqlCommand(sql, conn);
            command.CommandType = CommandType.Text;
            command.CommandTimeout = 30000;
            conn.Open();
            object result = command.ExecuteScalar();
            conn.Close();
            return result;
        }
        internal string GenerateSQLCondition(string c)
        {
            string[] array = c.Split('|');
            string result = string.Empty;
            foreach (string s in array)
            {
                result = result + "N'" + s + "',";
            }
            return result.Substring(0, result.Length - 1);
        }
        private void DeleteSQL(string sql)
        {
            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand command = new SqlCommand(sql, conn);
            command.CommandType = CommandType.Text;
            command.CommandTimeout = 30000;
            conn.Open();
            command.ExecuteNonQuery();
            conn.Close();
        }

        #region Filter clause
        private string GenerateAssemblyNameFilterClause(string assemblyName)
        {
            string assemblyNameFilterClause = string.Empty;
            if (assemblyName != "All")
            {
                assemblyNameFilterClause = @" AND (EXISTS (SELECT 1 FROM [dbo].[AssemblyLink]
                INNER JOIN [dbo].[AssemblyInfo] ON [AssemblyLink].[TypeID] = [AssemblyInfo].[TypeID]
                WHERE [Results].[ContentID] = [AssemblyLink].[ContentID] AND [AssemblyInfo].[AssemblyName] IN (" + assemblyName + ")))";
            }
            return assemblyNameFilterClause;
        }
        private string GenerateTypeIDFilterClause(string typeID)
        {
            string typeIDFilterClause = string.Empty;
            if (typeID != "All")
                typeIDFilterClause = @" AND (EXISTS (SELECT 1 FROM [dbo].[AssemblyLink]
 WHERE [Results].[ContentID] = [AssemblyLink].[ContentID] AND [AssemblyLink].[TypeID] IN (" + typeID + ")))";
            return typeIDFilterClause;
        }
        private string GenerateRuleIDFilterClause(string ruleid)
        {
            string ruleidFilterClause = string.Empty;
            if (ruleid != "All")
                ruleidFilterClause = " AND [Results].[RuleID] IN (" + ruleid + ")";

            return ruleidFilterClause;
        }
        private string GenerateResultTypesFilterClause(string resultTypes)
        {
            string resultTypesFilterClause = string.Empty;
            if (resultTypes != "All")
                resultTypesFilterClause = " AND [Results].[ResultType] IN (" + resultTypes + ")";

            return resultTypesFilterClause;
        }
        private string GenerateResultTypesFilterClause(string resultTypes, string renameTable)
        {
            string resultTypesFilterClause = string.Empty;
            if (resultTypes != "All")
                resultTypesFilterClause = " AND [" + renameTable + "].[ResultType] IN (" + resultTypes + ")";

            return resultTypesFilterClause;
        }
        private string GenerateBuildNOFilterClause(string buildNO)
        {
            string buildNOFilterClause = string.Empty;
            if (buildNO.Contains("AND"))
            {
                buildNOFilterClause = " AND [BuildInfo].[BuildNo] BETWEEN " + buildNO;
            }
            else
            {
                if (buildNO != "All")
                    buildNOFilterClause = " AND [BuildInfo].[BuildNo] IN (" + buildNO + ")";
            }

            return buildNOFilterClause;
        }
        private string GenerateBuildLanFilterClause(string buildLan)
        {
            string buildLanFilterClause = string.Empty;
            if (buildLan != "All")
                buildLanFilterClause = " AND [BuildInfo].[Language] IN (" + buildLan + ")";

            return buildLanFilterClause;
        }
        private string GenerateOSTypeFilterClause(string osType)
        {
            string osTypeFilterClause = string.Empty;

            if (osType != "All")
                osTypeFilterClause = " AND [Clients].[OSType] IN (" + osType + ")";

            return osTypeFilterClause;
        }
        private string GenerateOSLanguageFilterClause(string osLanguage)
        {
            string osLanguageFilterClause = string.Empty;

            if (osLanguage != "All")
                osLanguageFilterClause = " AND [Clients].[OSLanguage] IN (" + osLanguage + ")";

            return osLanguageFilterClause;
        }
        private string GenerateUserNameFilterClause(string username)
        {
            string usernameFilterClause = string.Empty;
            if (!string.IsNullOrEmpty(username))
                usernameFilterClause = " AND [Clients].[UserName] LIKE N'%" + username + "%' ";

            return usernameFilterClause;
        }
        private string GeneratePageTitleFilterClause(string pagetitle)
        {
            string pagetitleFilterClause = string.Empty;
            if (!string.IsNullOrEmpty(pagetitle))
            {
                pagetitle = pagetitle.Replace("'", "''");
                pagetitleFilterClause = " AND [UIContents].[UIName] LIKE N'%" + pagetitle + "%' ";
            }

            return pagetitleFilterClause;
        }
        private string GenerateReviewCommentsFilterClause(string reviewComments)
        {
            string reviewCommentsFilterClause = string.Empty;
            if (!string.IsNullOrEmpty(reviewComments))
            {
                reviewComments = reviewComments.Replace("'", "''");
                reviewCommentsFilterClause = " AND [Results].[ReviewLog] LIKE N'%" + reviewComments + "%' ";
            }

            return reviewCommentsFilterClause;
        }
        private string GenerateReviewedTypeFilterClause(string reviewedType)
        {
            string reviewedTypeFilterClause = string.Empty;
            if (reviewedType != "-1")
                reviewedTypeFilterClause = " AND [Results].[ReviewFlag]=" + reviewedType;

            return reviewedTypeFilterClause;
        }
        private string GenerateReviewedTypeFilterClause(string reviewedType, string renameTable)
        {
            string reviewedTypeFilterClause = string.Empty;
            if (reviewedType != "-1")
                reviewedTypeFilterClause = " AND [" + renameTable + "].[ReviewFlag]=" + reviewedType;

            return reviewedTypeFilterClause;
        }
        private string GenerateBuildTypeFilterClause(string buildType)
        {
            string buildTypeFilterClause = string.Empty;
            if (buildType != "All")
                buildTypeFilterClause = " And [UIContents].[Reserve2]=" + buildType;

            return buildTypeFilterClause;
        }
        private string GenerateSearchDatetimeFilterClause(string searchDatetime)
        {
            string searchDatetimeFilterClause = string.Empty;
            if (!string.IsNullOrEmpty(searchDatetime))
                searchDatetimeFilterClause = " AND [CreateDate]<'" + searchDatetime + "'";
            return searchDatetimeFilterClause;
        }
        #endregion

        #region Generate sql string1

        private string GenerateFormatString(string prefix, string assemblyName, string typeID, string ruleid, string resultTypes,
            string buildNO, string buildLan, string osType, string osLanguage)
        {
            prefix = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}",
                prefix,
                GenerateAssemblyNameFilterClause(assemblyName),
                GenerateTypeIDFilterClause(typeID),
                GenerateRuleIDFilterClause(ruleid),
                GenerateResultTypesFilterClause(resultTypes),
                GenerateBuildNOFilterClause(buildNO),
                GenerateBuildLanFilterClause(buildLan),
                GenerateOSTypeFilterClause(osType),
                GenerateOSLanguageFilterClause(osLanguage));
            return prefix;
        }

        private string GenerateListSQLByResultID(string resultId)
        {
            string sql = string.Format("SELECT{0} FROM [dbo].[Results]{1} WHERE 1=1 AND [Results].ResultID IN ({2})",
                sqlQueryCoreColumns,
                joinTablesNoAssembly,
                resultId);
            return sql;
        }
        private string GenerateListSQLByResultID(string resultId, string sortBy, string assembly)
        {
            string sortColumn = GenerateSQLSort(sortBy);
            string sql = string.Format("SELECT{0} FROM [dbo].[Results]{1} WHERE [Results].ResultID IN ({2}) ORDER BY {3} DESC",
                sqlQueryCoreColumns,
                joinTablesNoAssembly,
                resultId,
                sortColumn);
            return sql;
        }
        private string GenerateCountSQLByResultID(string resultId)
        {
            string sql = string.Format("SELECT COUNT(*) FROM [dbo].[Results] Where [Results].ResultID in ({0})",
                resultId);
            return sql;
        }

        private string GenerateLatestListSQL(string buildNO, string buildLan, string osType, string osLanguage, string ruleid, string resultTypes,
            string assemblyName, string typeID, int pageIndex, string username, string pagetitle, string reviewComments, string ReviewedType,
            string buildtype, string searchDatetime, string sortBy, int numbersEveryPage, int diffentReviewStatusResults)
        {
            int actualStartIndex = (pageIndex - 1) * numbersEveryPage + 1;
            int actualEndIndex = pageIndex * numbersEveryPage;
            if (ReviewedType == "0" || ReviewedType == "1")
            {
                actualStartIndex -= diffentReviewStatusResults;
                actualEndIndex -= diffentReviewStatusResults;
            }

            string sortColumn = GenerateSQLSort(sortBy);
            string osTypeLanguageRelatedRuleID = string.Empty, osTypeNotRelatedRuleID = string.Empty, osTypeLanguageNotRelatedRuleID = string.Empty;
            SeparateRuleIDs(ruleid, ref osTypeLanguageRelatedRuleID, ref osTypeNotRelatedRuleID, ref osTypeLanguageNotRelatedRuleID);
            string sqlOSTypeLanguageRelated = GenerateOSTypeLanguageRelatedLatestResultIDListSQL(buildNO, buildLan, osType, osLanguage,
                osTypeLanguageRelatedRuleID, resultTypes, assemblyName, typeID, username, pagetitle, reviewComments, ReviewedType, buildtype, searchDatetime);
            string sqlOSTypeNotRelated = GenerateOSTypeNotRelatedLatestResultIDListSQL(buildNO, buildLan, osType, osLanguage,
                osTypeNotRelatedRuleID, resultTypes, assemblyName, typeID, username, pagetitle, reviewComments, ReviewedType, buildtype, searchDatetime);
            string sqlOSTypeLanguageNotRelated = GenerateOSTypeLanguageNotRelatedLatestResultIDListSQL(buildNO, buildLan, osType, osLanguage,
                osTypeLanguageNotRelatedRuleID, resultTypes, assemblyName, typeID, username, pagetitle, reviewComments, ReviewedType, buildtype, searchDatetime);
            string sqlLatestBuild = GenerateLatestBuildResultIDListSQL(buildNO, buildLan, osType, osLanguage, ruleid, resultTypes, assemblyName,
                typeID, username, pagetitle, reviewComments, ReviewedType, buildtype, searchDatetime);

            string tempSQL = string.Empty;
            if (!string.IsNullOrEmpty(sqlOSTypeLanguageRelated))
            {
                tempSQL += sqlOSTypeLanguageRelated + " union ";
            }
            if (!string.IsNullOrEmpty(sqlOSTypeNotRelated))
            {
                tempSQL += sqlOSTypeNotRelated + " union ";
            }
            if (!string.IsNullOrEmpty(sqlOSTypeLanguageNotRelated))
            {
                tempSQL += sqlOSTypeLanguageNotRelated + " union ";
            }

            string sql = string.Format("SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY {0} DESC) AS rowNum,{1}{2}{3} AND [Results].[ResultID] IN("
                + "{4} {5})) AS t WHERE rowNum BETWEEN {6} AND {7} ORDER BY rowNum",
                sortColumn,
                sqlQueryCoreColumns,
                sqlQueryCoreTableForQueryLatestResult,
                sqlWhereClause,
                tempSQL,
                sqlLatestBuild,
                actualStartIndex,
                actualEndIndex);
            return sql;
        }
        private string GenerateOSTypeLanguageRelatedLatestResultIDListSQL(string buildNO, string buildLan, string osType, string osLanguage, string ruleid,
            string resultTypes, string assemblyName, string typeID, string username, string pagetitle, string reviewComments, string ReviewedType,
            string buildtype, string searchDatetime)
        {
            string groupFilter = GenerateSQLGroupFilter(buildNO, buildLan, osType, osLanguage, ruleid, assemblyName, typeID, username, pagetitle, reviewComments,
                buildtype, searchDatetime);
            string filter = GenerateSQLFilter(buildNO, buildLan, osType, osLanguage, ruleid, resultTypes, assemblyName, typeID, username, pagetitle, reviewComments,
                ReviewedType, buildtype, searchDatetime);

            string sql = string.Empty;
            if (!string.IsNullOrEmpty(ruleid))
            {
                sql = string.Format(@"{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}",
                    sqlSelectResultIDFROM,
                    sqlSelectColumns + ", [BuildInfo].[Language], [Clients].[OSType]",
                    sqlFROMResults,
                    sqlLeftJoin,
                    sqlWhereClause + filter,
                    sqlRenameTOTALAndSelectGroupColumns + ", [BuildInfo].[Language], [Clients].[OSType]",
                    sqlFROMResults,
                    sqlLeftJoin,
                    sqlWhereClause + groupFilter,
                    sqlGroupBy + ", [BuildInfo].[Language], [Clients].[OSType]",
                    sqlRenameGroupTableAndOn + sqlOnOSType + sqlOnLanguage,
                    sqlWHEREClauseOnTOTALAndGroupTable);
            }
            return sql;
        }
        private string GenerateOSTypeNotRelatedLatestResultIDListSQL(string buildNO, string buildLan, string osType, string osLanguage, string ruleid,
            string resultTypes, string assemblyName, string typeID, string username, string pagetitle, string reviewComments, string ReviewedType,
            string buildtype, string searchDatetime)
        {
            string groupFilter = GenerateSQLGroupFilter(buildNO, buildLan, osType, osLanguage, ruleid, assemblyName, typeID, username, pagetitle, reviewComments,
                buildtype, searchDatetime);
            string filter = GenerateSQLFilter(buildNO, buildLan, osType, osLanguage, ruleid, resultTypes, assemblyName, typeID, username, pagetitle, reviewComments,
                ReviewedType, buildtype, searchDatetime);

            string sql = string.Empty;
            if (!string.IsNullOrEmpty(ruleid))
            {
                sql = string.Format(@"{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}",
                    sqlSelectResultIDFROM,
                    sqlSelectColumns + ", [BuildInfo].[Language]",
                    sqlFROMResults,
                    sqlLeftJoin,
                    sqlWhereClause + filter,
                    sqlRenameTOTALAndSelectGroupColumns + ", [BuildInfo].[Language]",
                    sqlFROMResults,
                    sqlLeftJoin,
                    sqlWhereClause + groupFilter,
                    sqlGroupBy + ", [BuildInfo].[Language]",
                    sqlRenameGroupTableAndOn + sqlOnLanguage,
                    sqlWHEREClauseOnTOTALAndGroupTable);
            }
            return sql;
        }
        private string GenerateOSTypeLanguageNotRelatedLatestResultIDListSQL(string buildNO, string buildLan, string osType, string osLanguage, string ruleid,
            string resultTypes, string assemblyName, string typeID, string username, string pagetitle, string reviewComments, string ReviewedType,
            string buildtype, string searchDatetime)
        {
            string groupFilter = GenerateSQLGroupFilter(buildNO, buildLan, osType, osLanguage, ruleid, assemblyName, typeID, username, pagetitle, reviewComments,
                buildtype, searchDatetime);
            string filter = GenerateSQLFilter(buildNO, buildLan, osType, osLanguage, ruleid, resultTypes, assemblyName, typeID, username, pagetitle, reviewComments,
                ReviewedType, buildtype, searchDatetime);

            string sql = string.Empty;
            if (!string.IsNullOrEmpty(ruleid))
            {
                sql = string.Format(@"{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}",
                    sqlSelectResultIDFROM,
                    sqlSelectColumns,
                    sqlFROMResults,
                    sqlLeftJoin,
                    sqlWhereClause + filter,
                    sqlRenameTOTALAndSelectGroupColumns,
                    sqlFROMResults,
                    sqlLeftJoin,
                    sqlWhereClause + groupFilter,
                    sqlGroupBy,
                    sqlRenameGroupTableAndOn,
                    sqlWHEREClauseOnTOTALAndGroupTable);
            }
            return sql;
        }
        private string GenerateLatestBuildResultIDListSQL(string buildNO, string buildLan, string osType, string osLanguage, string ruleid,
            string resultTypes, string assemblyName, string typeID, string username, string pagetitle, string reviewComments, string ReviewedType,
            string buildtype, string searchDatetime)
        {
            string groupFilter = GenerateSQLGroupFilter(buildNO, buildLan, osType, osLanguage, ruleid, assemblyName, typeID, username, pagetitle,
                reviewComments, buildtype, searchDatetime);
            string filter = GenerateSQLFilter(buildNO, buildLan, osType, osLanguage, ruleid, resultTypes, assemblyName, typeID, username,
                pagetitle, reviewComments, ReviewedType, buildtype, searchDatetime);

            string maxBuildNoQuery = string.Format(@"SELECT MAX(BuildNo) FROM (SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [Rules].[RuleID],
 [UIContents].[UIName], [AssemblyLink].[TypeID]{0}{1}{2}{3}) AS MaxBuildNo",
                sqlFROMResults,
                sqlLeftJoin,
                sqlWhereClause + groupFilter,
                sqlGroupBy);

            string sql = string.Format("SELECT [ResultID]{0}{1}{2}",
                sqlQueryCoreTableForQueryLatestResult,
                sqlWhereClause + filter,
                " AND [BuildInfo].[BuildNo] IN (" + maxBuildNoQuery + ")");
            return sql;
        }
        private string GenerateListSQL(string buildNO, string buildLan, string osType, string osLanguage, string ruleid, string resultTypes,
            string assemblyName, string typeID, int pageIndex, string username, string pagetitle, string reviewComments, string ReviewedType,
            string buildtype, string searchDatetime, string sortBy, int numbersEveryPage, int diffentReviewStatusResults)
        {
            int actualStartIndex = (pageIndex - 1) * numbersEveryPage + 1;
            int actualEndIndex = pageIndex * numbersEveryPage;
            if (ReviewedType == "0" || ReviewedType == "1")
            {
                actualStartIndex -= diffentReviewStatusResults;
                actualEndIndex -= diffentReviewStatusResults;
            }

            string sortColumn = GenerateSQLSort(sortBy);
            string filter = GenerateSQLFilter(buildNO, buildLan, osType, osLanguage, ruleid, resultTypes, assemblyName, typeID, username,
                pagetitle, reviewComments, ReviewedType, buildtype, searchDatetime);
            string sql = string.Empty;
            if (assemblyName == "All")
            {
                sql = string.Format("SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY {0} DESC) AS rowNum,{1} FROM [dbo].[Results]{2}"
        + " WHERE 1=1 {3}) AS t WHERE rowNum BETWEEN {4} AND {5} ORDER BY rowNum",
        sortColumn, sqlQueryCoreColumns, joinTablesNoAssembly, filter, actualStartIndex, actualEndIndex);
            }
            else
            {
                sql = string.Format("SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY {0} DESC) AS rowNum,{1} FROM [dbo].[Results]{2}"
        + " WHERE 1=1 {3}) AS t WHERE rowNum BETWEEN {4} AND {5} ORDER BY rowNum",
        sortColumn, sqlQueryCoreColumns, joinTablesNoAssembly, filter, actualStartIndex, actualEndIndex);
            }
            return sql;
        }
        #endregion

        #region File bug method

        [WebMethod]
        public string FileVSOBug(string openby, string resultid, string attachfilepath)
        {
            string createdBy = openby;
            createdBy = createdBy.Replace('<', '(').Replace('>', ')');
            if (string.IsNullOrEmpty(attachfilepath)) attachfilepath = "";
            string fileBugResult = string.Empty;
            UIRecord record = new UIRecord();
            try
            {
                record.QueryRecordByResultID(int.Parse(resultid));
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
            //openby:
            for (int c = 0; c < openby.Length; c++)
            {
                if (openby[c] == '\\')
                {
                    openby = openby.Substring(c + 1);
                    break;
                }
            }
            string pagetitle = record.PageTitle;
            string buildNO = record.BuildNo;
            string buildLan = record.BuildLanguage;
            string assignTo = openby;
            string osType = record.OSType;
            string rulename = record.RuleName;
            string resultTypes = record.ResultType;
            //page Title
            if (pagetitle.Length > 40)
            {
                pagetitle = pagetitle.Substring(0, 40) + "~~~";
            }
            //log resultLog
            string resultLog = string.Empty;
            SulpHurEntities entities = new SulpHurEntities();
            int resultidToInt = Int32.Parse(resultid);
            Result result = entities.Results.FirstOrDefault(itemresult => itemresult.ResultID.Equals(resultidToInt));//????
            resultLog = result.ResultLog;
            if (string.IsNullOrEmpty(resultLog))
            {
                resultLog = "No any error on this page.";
            }
            string logMessage = resultLog;
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
                    for (int c = 0; c < info.Length; c++)
                    {
                        if (info[c] == '\"') count++;
                        if (count == 1 && tag == 0)
                        {
                            start1 = c;
                            tag++;
                        }
                        if (count == 2 && tag == 1)
                        {
                            end = c;
                            tag++;
                            controlname1 = info.Substring(start1, end - start1);
                            if (end - start1 > 40)
                            {
                                shortcontrolname1 = info.Substring(start1, 40);
                            }
                        }
                        if (count == 3 && tag == 2)
                        {
                            start2 = c;
                            tag++;
                        }
                        if (count == 4 && tag == 3)
                        {
                            end = c;
                            controlname2 = info.Substring(start2, end - start2);
                            if (end - start2 > 40)
                            {
                                shortcontrolname2 = info.Substring(start2, 40);
                            }
                            info = info.Substring(0, c + 1);
                            break;
                        }
                        if (c == info.Length - 1)
                        {
                            info = resultid.ToString();
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
                //Page Title
                if (string.IsNullOrEmpty(info))
                {
                    info = "No any error on this page.";
                }
                pagetitle = rulename + " [" + resultTypes + "] " + buildLan + "_" + pagetitle + "_" + GetLatestPassResult(resultid) + "_" + info;
                if (info != string.Empty)
                {
                    pagetitle = pagetitle.Replace("': \"", "' in \"");
                    pagetitle = pagetitle.Replace("'", "‘");
                    pagetitle = pagetitle.Replace("\\", "");
                    pagetitle = pagetitle.Replace("/", "");
                    pagetitle = pagetitle.Replace(":", "");
                    pagetitle = pagetitle.Replace("*", "");
                    pagetitle = pagetitle.Replace("?", "");
                    pagetitle = pagetitle.Replace("<", "");
                    pagetitle = pagetitle.Replace(">", "");
                    pagetitle = pagetitle.Replace("|", "");
                    pagetitle = pagetitle.Replace("\"", "'");
                    pagetitle = pagetitle.Replace("\"", "‘");
                    pagetitle = pagetitle.Replace(".", "");
                    pagetitle = pagetitle.Replace("\r", "");
                    pagetitle = pagetitle.Replace("\n", "");
                }
            }
            //bug 478039 + bug 478284
            else
            {
                pagetitle = rulename + " [" + resultTypes + "] " + buildLan + "_" + pagetitle + "_" + GetLatestPassResult(resultid) + "_" + resultLog;
                pagetitle = pagetitle.Replace("'", "‘");
                pagetitle = pagetitle.Replace("\"", "‘");
                pagetitle = pagetitle.Replace("\r\n", "");
                //v-edy: bug485629
                pagetitle = pagetitle.Replace("\n", "");
                if (pagetitle.Length > 250)
                {
                    pagetitle = pagetitle.Substring(0, 247) + "~~~";
                }
            }
            string pagetitle1 = pagetitle;
            //Build No
            string p1 = string.Empty;
            string p2 = string.Empty;
            int i = 0;
            List<char> buildNoArray = buildNO.ToList();
            while (buildNoArray[0] != '.')
            {
                p1 = p1 + buildNO[0];
                buildNoArray.Remove(buildNoArray[0]);
                i++;
            }
            if (i == 1) p1 = '0' + p1;
            buildNoArray.Remove(buildNoArray[0]);
            int j = 0;
            while (buildNoArray[0] != '.')
            {
                p2 = p2 + buildNoArray[0];
                buildNoArray.Remove(buildNoArray[0]);
                j++;
            }
            if (j == 1) p2 = '0' + p2;
            string p3 = string.Empty;
            foreach (char c in buildNoArray)
            {
                p3 = p3 + c;
            }
            buildNO = p1 + '.' + p2 + p3;
            //v-edy: bug481718&bug481854
            //latest, the loc bug and corecode bug should file bug to two database
            string filepath = string.Empty;
            if (buildLan == "ENU")
            {
                filepath = Path.Combine(Server.MapPath("~/"), "filevsobug-BugFields_CoreCode.txt");
            }
            else
            {
                filepath = Path.Combine(Server.MapPath("~/"), "filevsobug-BugFields_Loc.txt");
            }
            if (!File.Exists(filepath)) return "9";
            string temp = "\\\\scfs\\Users\\INTL\\SulphurBugFiles\\filevsobug-BugFields_temp_" + resultid + ".txt";

            string issuetype = string.Empty;
            string breatharea = string.Empty;
            switch (buildLan)
            {
                case "ENU":
                    issuetype = "Code Defect";
                    if (ConfigurationManager.AppSettings["Temp_ENU_ITCodeDefect_BATranslation"].ToString().Contains(rulename))
                    {
                        breatharea = "Translation";
                    }
                    else if (ConfigurationManager.AppSettings["Temp_ENU_ITCodeDefect_BALocalizability"].ToString().Contains(rulename))
                    {
                        breatharea = "Localizability";
                    }
                    break;
                default:
                    issuetype = "Localization (Non-linguistic)";
                    if (ConfigurationManager.AppSettings["Temp_NENU_ITLocalization_BATranslation"].ToString().Contains(rulename))
                    {
                        breatharea = "Translation";
                    }
                    else if (ConfigurationManager.AppSettings["Temp_NENU_ITLocalization_BALocalization"].ToString().Contains(rulename))
                    {
                        breatharea = "Localization";
                        if (pagetitle.Contains("miss hotkeys") || "Tab Order Rule" == rulename)
                        {
                            return "No need to file bug about Tab Order or Miss Hotkey on non-ENU lan: " + resultid;
                        }
                    }
                    break;
            }
            //Build Lan
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("ZHH", "Chinese (Hong Kong SAR)");
            dic.Add("CHS", "Chinese (Simplified)");
            dic.Add("CHT", "Chinese (Traditional)");
            dic.Add("ENU", "Core Code");
            dic.Add("FRA", "French");
            dic.Add("JPN", "Japanese");
            dic.Add("DEU", "German");
            dic.Add("RUS", "Russian");
            dic.Add("NLD", "ICP Dutch");
            dic.Add("HUN", "ICP Hungarian");
            dic.Add("ITA", "ICP Italian");
            dic.Add("KOR", "Korean");
            dic.Add("PLK", "ICP Polish");
            dic.Add("PTB", "ICP Portuguese (Brazil)");
            dic.Add("PTG", "ICP Portuguese (Portugal)");
            dic.Add("CSY", "ICP Czech");
            dic.Add("ESN", "ICP Spanish");
            dic.Add("SVE", "ICP Swedish");
            dic.Add("TRK", "ICP Turkish");
            buildLan = dic[buildLan];
            //Description
            string createdByString = @"This bug is created by '" + createdBy + "'.";
            string template = createdByString + @"

This bug is filed from Sulphur tool, please review it before assign it to the Loc team.

Page Title: {0}
Rule Name : {1}
Result Type : {2}
Build Language: {3}
ResultID: {4}

Latest Pass Build Infomation: {5}

Please go to shared pdf file for the detailed info: 
{6}
";
            string description = string.Format(
               template,
               pagetitle1,
               rulename,
               resultTypes,
               buildLan,
               resultid,
               GetLatestPassResult(resultid),
               "<a aria-label=\"CTRL+Click or CTRL+Enter to follow link " + attachfilepath + "\" href=\"" + attachfilepath.Replace(" ", "%20") + "\">" + attachfilepath + "</a>");

            //v-edy: bug481854
            //string filepath = Path.Combine(Server.MapPath("/"), "filebug-BugFields.json");
            //string temp = "\\\\scfs\\Users\\INTL\\SulphurBugFiles\\filebug-BugFields_temp.json";
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                //startInfo.Arguments = "/c net use \\\\scfs " + Microsoft.ConfigurationManagement.Test.KeyVault.CommonSecretIdentifiers.SMSAccessSecretID + " /user:smsaccess";
                Process.Start(startInfo);
                Thread.Sleep(400);

                FileAttributes fa;
                if (File.Exists(temp))
                {
                    fa = File.GetAttributes(temp);
                    if ((fa & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        File.SetAttributes(temp, FileAttributes.Normal);
                    }
                }
                File.Copy(filepath, temp, true);
                fa = File.GetAttributes(temp);
                if ((fa & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    File.SetAttributes(temp, FileAttributes.Normal);
                }
                string content = File.ReadAllText(temp)
                    .Replace("$title$", "\"" + pagetitle1 + "\"")
                    .Replace("$issuetype$", issuetype)
                    .Replace("$assignedto$", openby)
                    .Replace("$language$", buildLan)
                    .Replace("$foundinbuild$", buildNO)
                    .Replace("$reprosteps$", "")
                    .Replace("$description$", "\"" + description + "\"")
                    .Replace("$tags$", breatharea);
                File.WriteAllText(temp, content, System.Text.Encoding.Unicode);
                //File.Copy(tempFilePath, "\\\\scfs\\Users\\INTL\\TempFile\\" + resultid + ".txt", true);
                string tempFilePath = filebugFileFolder + resultid + ".txt";
                using (FileStream fs1 = new FileStream(tempFilePath, FileMode.Create, FileAccess.ReadWrite))
                {
                    if (record.BuildLanguage == "ENU")
                    {
                        string fileContent = "msazure.visualstudio.com" + "\r\n" + "configmgr" + "\r\n" + "Bug" + "\r\n" + temp;
                        StreamWriter sw = new StreamWriter(fs1, System.Text.Encoding.Unicode);
                        sw.Write(fileContent);
                        sw.Flush();
                    }
                    else
                    {
                        string fileContent = "dev.azure.com" + "\r\n" + "ceintl" + "\r\n" + "Feedback" + "\r\n" + temp;
                        StreamWriter sw = new StreamWriter(fs1, System.Text.Encoding.Unicode);
                        sw.Write(fileContent);
                        sw.Flush();
                    }
                }
                DirectoryInfo folder = new DirectoryInfo(filebugFileFolder);
                bool completeFlag = false;
                while (true)
                {

                    foreach (FileInfo f in folder.GetFiles("*.txt"))
                    {
                        if (f.Name.Contains(resultid + "_"))
                        {
                            completeFlag = true;
                            break;
                        }
                    }
                    if (completeFlag)
                    {
                        break;
                    }
                }
                foreach (FileInfo f in folder.GetFiles("*.txt"))
                {
                    if (f.Name.Contains(resultid))
                    {
                        fileBugResult = f.Name.Substring(f.Name.IndexOf("_") + 1, f.Name.Length - f.Name.IndexOf("_") - 5);
                        f.Delete();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //File.Delete(temp);
                //v-edy : change filebug.exe tool run on server
                //File.Delete(tempFilePath);
            }
            return fileBugResult;
        }
        [WebMethod]
        public string FileVSOBugDirectly1(string openBy, string resultid, string attachfilepath, bool fileMSAzureBug)
        {
            //open by account should be like v-XXX@microsoft.com
            string openByEmailAddress = openBy + "@microsoft.com";
            //customr format should be like FAREAST\v-xxx
            string customer = @"FAREAST\" + openBy;
            string actualFilePath = Server.MapPath(attachfilepath);

            if (string.IsNullOrEmpty(attachfilepath)) attachfilepath = "";
            string fileBugResult = string.Empty;
            UIRecord record = new UIRecord();
            try
            {
                record.QueryRecordByResultID(int.Parse(resultid));
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
            string pagetitle = record.PageTitle;
            string buildNo = record.BuildNo;
            string buildLan = record.BuildLanguage;
            string osType = record.OSType;
            string rulename = record.RuleName;
            string resultTypes = record.ResultType;
            //page Title
            if (pagetitle.Length > 40)
            {
                pagetitle = pagetitle.Substring(0, 40) + "~~~";
            }
            //log resultLog
            string resultLog = string.Empty;
            SulpHurEntities entities = new SulpHurEntities();
            int resultidToInt = Int32.Parse(resultid);
            Result result = entities.Results.FirstOrDefault(itemresult => itemresult.ResultID.Equals(resultidToInt));//????
            resultLog = result.ResultLog;
            if (string.IsNullOrEmpty(resultLog))
            {
                resultLog = "No any error on this page.";
            }
            string logMessage = resultLog;
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
                    for (int c = 0; c < info.Length; c++)
                    {
                        if (info[c] == '\"') count++;
                        if (count == 1 && tag == 0)
                        {
                            start1 = c;
                            tag++;
                        }
                        if (count == 2 && tag == 1)
                        {
                            end = c;
                            tag++;
                            controlname1 = info.Substring(start1, end - start1);
                            if (end - start1 > 40)
                            {
                                shortcontrolname1 = info.Substring(start1, 40);
                            }
                        }
                        if (count == 3 && tag == 2)
                        {
                            start2 = c;
                            tag++;
                        }
                        if (count == 4 && tag == 3)
                        {
                            end = c;
                            controlname2 = info.Substring(start2, end - start2);
                            if (end - start2 > 40)
                            {
                                shortcontrolname2 = info.Substring(start2, 40);
                            }
                            info = info.Substring(0, c + 1);
                            break;
                        }
                        if (c == info.Length - 1)
                        {
                            info = resultid.ToString();
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
                //Page Title
                if (string.IsNullOrEmpty(info))
                {
                    info = "No any error on this page.";
                }
                pagetitle = rulename + " [" + resultTypes + "] " + buildLan + "_" + pagetitle + "_" + GetLatestPassResult(resultid) + "_" + info;
                if (info != string.Empty)
                {
                    pagetitle = pagetitle.Replace("': \"", "' in \"");
                    pagetitle = pagetitle.Replace("'", "‘");
                    pagetitle = pagetitle.Replace("\\", "");
                    pagetitle = pagetitle.Replace("/", "");
                    pagetitle = pagetitle.Replace(":", "");
                    pagetitle = pagetitle.Replace("*", "");
                    pagetitle = pagetitle.Replace("?", "");
                    pagetitle = pagetitle.Replace("<", "");
                    pagetitle = pagetitle.Replace(">", "");
                    pagetitle = pagetitle.Replace("|", "");
                    pagetitle = pagetitle.Replace("\"", "'");
                    pagetitle = pagetitle.Replace("\"", "‘");
                    pagetitle = pagetitle.Replace(".", "");
                    pagetitle = pagetitle.Replace("\r", "");
                    pagetitle = pagetitle.Replace("\n", "");
                }
            }
            //bug 478039 + bug 478284
            else
            {
                pagetitle = rulename + " [" + resultTypes + "] " + buildLan + "_" + pagetitle + "_" + GetLatestPassResult(resultid) + "_" + resultLog;
                pagetitle = pagetitle.Replace("'", "‘");
                pagetitle = pagetitle.Replace("\"", "‘");
                pagetitle = pagetitle.Replace("\r\n", "");
                //v-edy: bug485629
                pagetitle = pagetitle.Replace("\n", "");
                if (pagetitle.Length > 250)
                {
                    pagetitle = pagetitle.Substring(0, 247) + "~~~";
                }
            }
            string pagetitle1 = pagetitle;
            //Build No
            string p1 = string.Empty;
            string p2 = string.Empty;
            int i = 0;
            List<char> buildNoArray = buildNo.ToList();
            while (buildNoArray[0] != '.')
            {
                p1 = p1 + buildNo[0];
                buildNoArray.Remove(buildNoArray[0]);
                i++;
            }
            if (i == 1) p1 = '0' + p1;
            buildNoArray.Remove(buildNoArray[0]);
            int j = 0;
            while (buildNoArray[0] != '.')
            {
                p2 = p2 + buildNoArray[0];
                buildNoArray.Remove(buildNoArray[0]);
                j++;
            }
            if (j == 1) p2 = '0' + p2;
            string p3 = string.Empty;
            foreach (char c in buildNoArray)
            {
                p3 = p3 + c;
            }
            buildNo = p1 + '.' + p2 + p3;

            //Description
            string template = @"This bug is created by '{0}'.<br />
<br />
This bug is filed from Sulphur tool, please review it before assign it.<br />
<br />
Title: {1}<br />
ResultID: {2}<br />
UI Name: {3}<br />
Repro Steps: {4}<br />
Build Language: {5}<br />
Real OS Language: {6}<br />
OS Type: {7}<br />
Build No: {8}<br />
Assembly: {9}<br />
Full Type Name: {10}<br />
Rule Name : {11}<br />
Result Type : {12}<br />
Latest Pass result: {13}<br />
Result Log: {14}<br />
<img src=""https://msazure.visualstudio.com/efd2d5e7-dd4c-4fca-88df-4af3dcfec07b/_apis/wit/attachments/{15}"">";

            List<string> lstAttachments = AzureDevOps.GetAttachmentFiles(actualFilePath);

            int resultIdInInt = int.Parse(resultid);
            string uiName = string.Empty;
            string reprostep = string.Empty;
            string OSLanguage = string.Empty;
            string OSType = string.Empty;
            Ajax ajax = new Ajax();
            ajax.GetUIInfo(resultIdInInt, out uiName, out reprostep, out OSLanguage, out OSType);

            int uiContentID = 0;
            string assemblyName = string.Empty;
            string fullTypeName = string.Empty;
            try
            {
                ajax.GetPageIdentifierAssemblyInfo(resultIdInInt, out uiContentID, out assemblyName, out fullTypeName);
            }
            catch
            {
                assemblyName = "Can't find assembly Info";
                fullTypeName = "Can't find fullTypeName";
            }

            string latestPassResult = GetLatestPassResult(resultid);

            string language= string.Empty;
            if (fileMSAzureBug)
                language = dicLan[buildLan][0];
            else
                language = dicLan[buildLan][1];
            string description = string.Format(
               template,
               customer,
               pagetitle1,
               resultid,
               record.PageTitle,
               reprostep,
               language,
               OSLanguage,
               OSType,
               buildNo,
               assemblyName,
               fullTypeName,
               rulename,
               resultTypes,
               latestPassResult,
               logMessage,
               lstAttachments[0]);

            int bugId = -1;
            VsoClient vso = null;
            try
            {
                if (fileMSAzureBug)
                {
                    vso = new VsoClient(VsoProject.MSAZURE);
                    VsoMSAzureBug bug = new VsoMSAzureBug()
                    {
                        Title = pagetitle1,
                        IssueType = "Code Defect",
                        AssignedTo = openByEmailAddress,  // need email format as v-xxx@microsoft.com
                        AreaPath = "Configmgr",
                        IterationPath = ConfigurationManager.AppSettings["MSAzureBug_IterationPath"],
                        Priority = 2,
                        Customer = customer,
                        Description = description,
                        ApprovalStatus = "Unreviewed",
                        FoundIn = buildNo,
                        SecurityImpact = "Not a Security Bug",
                        Language = language,
                        Source = "Product Engineering Team",
                        HowFoundItem = "Automated test case",
                        AttachedFile = lstAttachments[0]
                    };
                    bugId = vso.FileBugInMSAzure(bug);
                }
                else
                {
                    vso = new VsoClient(VsoProject.CEAPEX);
                    VsoCepeaxFeedback feedback = new VsoCepeaxFeedback()
                    {
                        Title = pagetitle1,
                        AssignedTo = openByEmailAddress,  // need email format as v-xxx@microsoft.com
                        Tags = ConfigurationManager.AppSettings["MSCepeaxFeedback_Tags"],
                        AreaPath = @"CEINTL\Enterprise Mobility (ECM)\ConfigMgr (SCCM)",
                        IterationPath = @"CEINTL\BACKLOG",
                        ProjectType = "Software",
                        LocPriority = 1,
                        CustomerName = customer,
                        LanguageOrigin = language,
                        Language = language,
                        Description = description,
                        AttachedFile = lstAttachments[0]
                    };
                    bugId = vso.FileFeedbackInCEAPEX(feedback);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return bugId.ToString();
            #region
            ////v-edy: bug481854
            ////string filepath = Path.Combine(Server.MapPath("/"), "filebug-BugFields.json");
            ////string temp = "\\\\scfs\\Users\\INTL\\SulphurBugFiles\\filebug-BugFields_temp.json";
            //try
            //{
            //    ProcessStartInfo startInfo = new ProcessStartInfo();
            //    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //    startInfo.FileName = "cmd.exe";
            //    //startInfo.Arguments = "/c net use \\\\scfs " + Microsoft.ConfigurationManagement.Test.KeyVault.CommonSecretIdentifiers.SMSAccessSecretID + " /user:smsaccess";
            //    Process.Start(startInfo);
            //    Thread.Sleep(400);

            //    FileAttributes fa;
            //    if (File.Exists(temp))
            //    {
            //        fa = File.GetAttributes(temp);
            //        if ((fa & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            //        {
            //            File.SetAttributes(temp, FileAttributes.Normal);
            //        }
            //    }
            //    File.Copy(filepath, temp, true);
            //    fa = File.GetAttributes(temp);
            //    if ((fa & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            //    {
            //        File.SetAttributes(temp, FileAttributes.Normal);
            //    }
            //    string content = File.ReadAllText(temp)
            //        .Replace("$title$", "\"" + pagetitle1 + "\"")
            //        //.Replace("$issuetype$", issuetype)
            //        .Replace("$assignedto$", openBy)
            //        .Replace("$language$", buildLan)
            //        .Replace("$foundinbuild$", buildNo)
            //        .Replace("$reprosteps$", "")
            //        .Replace("$description$", "\"" + description + "\"")
            //        .Replace("$tags$", breatharea);
            //    File.WriteAllText(temp, content, System.Text.Encoding.Unicode);
            //    //File.Copy(tempFilePath, "\\\\scfs\\Users\\INTL\\TempFile\\" + resultid + ".txt", true);
            //    string tempFilePath = filebugFileFolder + resultid + ".txt";
            //    using (FileStream fs1 = new FileStream(tempFilePath, FileMode.Create, FileAccess.ReadWrite))
            //    {
            //        if (record.BuildLanguage == "ENU")
            //        {
            //            string fileContent = "msazure.visualstudio.com" + "\r\n" + "configmgr" + "\r\n" + "Bug" + "\r\n" + temp;
            //            StreamWriter sw = new StreamWriter(fs1, System.Text.Encoding.Unicode);
            //            sw.Write(fileContent);
            //            sw.Flush();
            //        }
            //        else
            //        {
            //            string fileContent = "dev.azure.com" + "\r\n" + "ceintl" + "\r\n" + "Feedback" + "\r\n" + temp;
            //            StreamWriter sw = new StreamWriter(fs1, System.Text.Encoding.Unicode);
            //            sw.Write(fileContent);
            //            sw.Flush();
            //        }
            //    }
            //    DirectoryInfo folder = new DirectoryInfo(filebugFileFolder);
            //    bool completeFlag = false;
            //    while (true)
            //    {

            //        foreach (FileInfo f in folder.GetFiles("*.txt"))
            //        {
            //            if (f.Name.Contains(resultid + "_"))
            //            {
            //                completeFlag = true;
            //                break;
            //            }
            //        }
            //        if (completeFlag)
            //        {
            //            break;
            //        }
            //    }
            //    foreach (FileInfo f in folder.GetFiles("*.txt"))
            //    {
            //        if (f.Name.Contains(resultid))
            //        {
            //            fileBugResult = f.Name.Substring(f.Name.IndexOf("_") + 1, f.Name.Length - f.Name.IndexOf("_") - 5);
            //            f.Delete();
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
            //finally
            //{
            //    //File.Delete(temp);
            //    //v-edy : change filebug.exe tool run on server
            //    //File.Delete(tempFilePath);
            //}

            //return fileBugResult;
            #endregion
        }

        [WebMethod]
        public string FileVSOBugDirectly(string openBy, string resultid, string attachfilepath, bool fileMSAzureBug)
        {
            //open by account should be like v-XXX@microsoft.com
            string openByEmailAddress = openBy + "@microsoft.com";
            //customr format should be like FAREAST\v-xxx
            string customer = @"FAREAST\" + openBy;
            string actualFilePath = Server.MapPath(attachfilepath);
            if (string.IsNullOrEmpty(attachfilepath)) attachfilepath = "";

            int resultidToInt = Int32.Parse(resultid);

            SulpHurEntities entities = new SulpHurEntities();
            Result result = entities.Results.FirstOrDefault(itemResult => itemResult.ResultID == resultidToInt);
            UIContent uiContent = entities.UIContents.FirstOrDefault(itemUIContent => itemUIContent.ContentID == result.ContentID);
            BuildInfo buildInfo = entities.BuildInfoes.FirstOrDefault(itemBuildInfo => itemBuildInfo.BuildID == uiContent.BuildID);
            Client client = entities.Clients.FirstOrDefault(itemClient => itemClient.ClientID == uiContent.ClientID);
            Rule rule = entities.Rules.FirstOrDefault(itemRule => itemRule.RuleID == result.RuleID);

            string resultTypes = result.ResultType;
            string resultLog = result.ResultLog;
            string pagetitle = uiContent.UIName;
            string buildNo = buildInfo.BuildNo;
            string buildLan = buildInfo.Language;
            string osType = client.OSType;
            string rulename = rule.RuleName;

            //page Title
            if (pagetitle.Length > 40)
            {
                pagetitle = pagetitle.Substring(0, 40) + "~~~";
            }

            if (string.IsNullOrEmpty(resultLog))
            {
                resultLog = "No any error on this page.";
            }
            string logMessage = resultLog;
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
                    for (int c = 0; c < info.Length; c++)
                    {
                        if (info[c] == '\"') count++;
                        if (count == 1 && tag == 0)
                        {
                            start1 = c;
                            tag++;
                        }
                        if (count == 2 && tag == 1)
                        {
                            end = c;
                            tag++;
                            controlname1 = info.Substring(start1, end - start1);
                            if (end - start1 > 40)
                            {
                                shortcontrolname1 = info.Substring(start1, 40);
                            }
                        }
                        if (count == 3 && tag == 2)
                        {
                            start2 = c;
                            tag++;
                        }
                        if (count == 4 && tag == 3)
                        {
                            end = c;
                            controlname2 = info.Substring(start2, end - start2);
                            if (end - start2 > 40)
                            {
                                shortcontrolname2 = info.Substring(start2, 40);
                            }
                            info = info.Substring(0, c + 1);
                            break;
                        }
                        if (c == info.Length - 1)
                        {
                            info = resultid.ToString();
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
                //Page Title
                if (string.IsNullOrEmpty(info))
                {
                    info = "No any error on this page.";
                }
                pagetitle = rulename + " [" + resultTypes + "] " + buildLan + "_" + pagetitle + "_" + GetLatestPassResult(resultid) + "_" + info;
                if (info != string.Empty)
                {
                    pagetitle = pagetitle.Replace("': \"", "' in \"");
                    pagetitle = pagetitle.Replace("'", "‘");
                    pagetitle = pagetitle.Replace("\\", "");
                    pagetitle = pagetitle.Replace("/", "");
                    pagetitle = pagetitle.Replace(":", "");
                    pagetitle = pagetitle.Replace("*", "");
                    pagetitle = pagetitle.Replace("?", "");
                    pagetitle = pagetitle.Replace("<", "");
                    pagetitle = pagetitle.Replace(">", "");
                    pagetitle = pagetitle.Replace("|", "");
                    pagetitle = pagetitle.Replace("\"", "'");
                    pagetitle = pagetitle.Replace("\"", "‘");
                    pagetitle = pagetitle.Replace(".", "");
                    pagetitle = pagetitle.Replace("\r", "");
                    pagetitle = pagetitle.Replace("\n", "");
                }
            }
            //bug 478039 + bug 478284
            else
            {
                pagetitle = rulename + " [" + resultTypes + "] " + buildLan + "_" + pagetitle + "_" + GetLatestPassResult(resultid) + "_" + resultLog;
                pagetitle = pagetitle.Replace("'", "‘");
                pagetitle = pagetitle.Replace("\"", "‘");
                pagetitle = pagetitle.Replace("\r\n", "");
                //v-edy: bug485629
                pagetitle = pagetitle.Replace("\n", "");
                if (pagetitle.Length > 250)
                {
                    pagetitle = pagetitle.Substring(0, 247) + "~~~";
                }
            }
            string pagetitle1 = pagetitle;
            //Build No
            string p1 = string.Empty;
            string p2 = string.Empty;
            int i = 0;
            List<char> buildNoArray = buildNo.ToList();
            while (buildNoArray[0] != '.')
            {
                p1 = p1 + buildNo[0];
                buildNoArray.Remove(buildNoArray[0]);
                i++;
            }
            if (i == 1) p1 = '0' + p1;
            buildNoArray.Remove(buildNoArray[0]);
            int j = 0;
            while (buildNoArray[0] != '.')
            {
                p2 = p2 + buildNoArray[0];
                buildNoArray.Remove(buildNoArray[0]);
                j++;
            }
            if (j == 1) p2 = '0' + p2;
            string p3 = string.Empty;
            foreach (char c in buildNoArray)
            {
                p3 = p3 + c;
            }
            buildNo = p1 + '.' + p2 + p3;

            //Description
            string template = @"This bug is created by '{0}'.<br />
<br />
This bug is filed from Sulphur tool, please review it before assign it.<br />
<br />
Title: {1}<br />
ResultID: {2}<br />
UI Name: {3}<br />
Repro Steps: {4}<br />
Build Language: {5}<br />
Real OS Language: {6}<br />
OS Type: {7}<br />
Build No: {8}<br />
Assembly: {9}<br />
Full Type Name: {10}<br />
Rule Name : {11}<br />
Result Type : {12}<br />
Latest Pass result: {13}<br />
Result Log: {14}<br />
<img src=""https://msazure.visualstudio.com/efd2d5e7-dd4c-4fca-88df-4af3dcfec07b/_apis/wit/attachments/{15}"">";

            List<string> lstAttachments = AzureDevOps.GetAttachmentFiles(actualFilePath);

            int resultIdInInt = int.Parse(resultid);
            string uiName = uiContent.UIName;
            string reprostep = uiContent.LaunchedFrom;
            string OSLanguage = client.OSLanguage;
            string OSType = client.OSType;

            string assemblyName = string.Empty;
            string fullTypeName = string.Empty;
            AssemblyLink assemblyLink = entities.AssemblyLinks.FirstOrDefault(itemAssemblyLink => itemAssemblyLink.IsPageIdentifier && itemAssemblyLink.ContentID == uiContent.ContentID);
            if(assemblyLink==null)
            {
                assemblyName = "Can't find assembly Info";
                fullTypeName = "Can't find fullTypeName";
            }
            else
            {
                AssemblyInfo assemblyInfo = entities.AssemblyInfoes.FirstOrDefault(itemAssemblyInfo => itemAssemblyInfo.TypeID == assemblyLink.TypeID);
                assemblyName = assemblyInfo.AssemblyName;
                fullTypeName = assemblyInfo.FullTypeName;
            }

            string latestPassResult = GetLatestPassResult(resultid);

            string language = string.Empty;
            if (fileMSAzureBug)
                language = dicLan[buildLan][0];
            else
                language = dicLan[buildLan][1];
            string description = string.Format(
               template,
               customer,
               pagetitle1,
               resultid,
               uiContent.UIName,
               reprostep,
               language,
               OSLanguage,
               OSType,
               buildNo,
               assemblyName,
               fullTypeName,
               rulename,
               resultTypes,
               latestPassResult,
               logMessage,
               lstAttachments[0]);

            int bugId = -1;
            VsoClient vso = null;
            try
            {
                if (fileMSAzureBug)
                {
                    vso = new VsoClient(VsoProject.MSAZURE);
                    VsoMSAzureBug bug = new VsoMSAzureBug()
                    {
                        Title = pagetitle1,
                        IssueType = "Code Defect",
                        AssignedTo = openByEmailAddress,  // need email format as v-xxx@microsoft.com
                        AreaPath = "Configmgr",
                        IterationPath = ConfigurationManager.AppSettings["MSAzureBug_IterationPath"],
                        Priority = 2,
                        Customer = customer,
                        Description = description,
                        ApprovalStatus = "Unreviewed",
                        FoundIn = buildNo,
                        SecurityImpact = "Not a Security Bug",
                        Language = language,
                        Source = "Product Engineering Team",
                        HowFoundItem = "Automated test case",
                        AttachedFile = lstAttachments[0]
                    };
                    bugId = vso.FileBugInMSAzure(bug);
                }
                else
                {
                    vso = new VsoClient(VsoProject.CEAPEX);
                    VsoCepeaxFeedback feedback = new VsoCepeaxFeedback()
                    {
                        Title = pagetitle1,
                        AssignedTo = openByEmailAddress,  // need email format as v-xxx@microsoft.com
                        Tags = ConfigurationManager.AppSettings["MSCepeaxFeedback_Tags"],
                        AreaPath = @"CEINTL\Enterprise Mobility (ECM)\ConfigMgr (SCCM)",
                        IterationPath = @"CEINTL\BACKLOG",
                        ProjectType = "Software",
                        LocPriority = 1,
                        CustomerName = customer,
                        LanguageOrigin = language,
                        Language = language,
                        Description = description,
                        AttachedFile = lstAttachments[0]
                    };
                    bugId = vso.FileFeedbackInCEAPEX(feedback);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return bugId.ToString();
        }
        #endregion

        #region Generate sql string2
        private string GenerateLatestCountSQL(string buildNO, string buildLan, string osType, string osLanguage, string ruleid, string resultTypes,
            string assemblyName, string typeID, string username, string pagetitle, string reviewComments, string ReviewedType, string buildtype,
            string searchDatetime, bool getLatest)
        {
            string osTypeLanguageRelatedRuleID = string.Empty, osTypeNotRelatedRuleID = string.Empty, osTypeLanguageNotRelatedRuleID = string.Empty;
            SeparateRuleIDs(ruleid, ref osTypeLanguageRelatedRuleID, ref osTypeNotRelatedRuleID, ref osTypeLanguageNotRelatedRuleID);

            string sqlOSTypeLanguageRelated = GenerateOSTypeLanguageRelatedLatestResultIDListSQL(buildNO, buildLan, osType, osLanguage,
                osTypeLanguageRelatedRuleID, resultTypes, assemblyName, typeID, username, pagetitle, reviewComments, ReviewedType, buildtype, searchDatetime);
            string sqlOSTypeNotRelated = GenerateOSTypeNotRelatedLatestResultIDListSQL(buildNO, buildLan, osType, osLanguage,
                osTypeNotRelatedRuleID, resultTypes, assemblyName, typeID, username, pagetitle, reviewComments, ReviewedType, buildtype, searchDatetime);
            string sqlOSTypeLanguageNotRelated = GenerateOSTypeLanguageNotRelatedLatestResultIDListSQL(buildNO, buildLan, osType, osLanguage,
                osTypeLanguageNotRelatedRuleID, resultTypes, assemblyName, typeID, username, pagetitle, reviewComments, ReviewedType, buildtype, searchDatetime);
            string sqlLatestBuild = GenerateLatestBuildResultIDListSQL(buildNO, buildLan, osType, osLanguage, ruleid,
                resultTypes, assemblyName, typeID, username, pagetitle, reviewComments, ReviewedType, buildtype, searchDatetime);

            string tempSQL = string.Empty;
            if (!string.IsNullOrEmpty(sqlOSTypeLanguageRelated))
            {
                tempSQL += sqlOSTypeLanguageRelated + " union ";
            }
            if (!string.IsNullOrEmpty(sqlOSTypeNotRelated))
            {
                tempSQL += sqlOSTypeNotRelated + " union ";
            }
            if (!string.IsNullOrEmpty(sqlOSTypeLanguageNotRelated))
            {
                tempSQL += sqlOSTypeLanguageNotRelated + " union ";
            }

            string sql = string.Format("SELECT COUNT(*) {0}{1} AND [Results].[ResultID] IN({2} {3})",
                sqlQueryCoreTableForQueryLatestResult,
                sqlWhereClause,
                tempSQL,
                sqlLatestBuild);
            return sql;
        }
        private string GenerateCountSQL(string buildNO, string buildLan, string osType, string osLanguage, string ruleid, string resultTypes,
            string assemblyName, string typeID, string username, string pagetitle, string reviewComments, string ReviewedType, string buildtype,
            string searchDatetime)
        {
            string filter = GenerateSQLFilter(buildNO, buildLan, osType, osLanguage, ruleid, resultTypes, assemblyName, typeID, username,
                pagetitle, reviewComments, ReviewedType, buildtype, searchDatetime);
            string sql = string.Empty;
            if (assemblyName == "" || assemblyName == null)
            {
                sql = string.Format("SELECT COUNT(*) FROM [dbo].[Results]{0} WHERE 1=1 {1}", joinTablesNoAssembly, filter);
            }
            else
            {
                sql = string.Format("SELECT COUNT(*) FROM [dbo].[Results]{0} WHERE 1=1 {1}", joinTablesNoAssembly, filter);
            }
            return sql;
        }

        private string GenerateSQLFilter(string buildNO, string buildLan, string osType, string osLanguage, string ruleid, string resultTypes,
            string assemblyName, string typeID, string username, string pagetitle, string reviewComments, string ReviewedType, string buildtype,
            string searchDatetime)
        {
            string filter = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}{13}",
                GenerateBuildNOFilterClause(buildNO),
                GenerateBuildLanFilterClause(buildLan),
                GenerateOSTypeFilterClause(osType),
                GenerateOSLanguageFilterClause(osLanguage),
                GenerateRuleIDFilterClause(ruleid),
                GenerateResultTypesFilterClause(resultTypes),
                GenerateAssemblyNameFilterClause(assemblyName),
                GenerateTypeIDFilterClause(typeID),
                GenerateUserNameFilterClause(username),
                GeneratePageTitleFilterClause(pagetitle),
                GenerateReviewCommentsFilterClause(reviewComments),
                GenerateReviewedTypeFilterClause(ReviewedType),
                GenerateBuildTypeFilterClause(buildtype),
                GenerateSearchDatetimeFilterClause(searchDatetime));
            return filter;
        }
        private string GenerateSQLGroupFilter(string buildNO, string buildLan, string osType, string osLanguage, string ruleid,
            string assemblyName, string typeID, string username, string pagetitle, string reviewComments, string buildtype, string searchDatetime)
        {
            string filter = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}",
                GenerateBuildNOFilterClause(buildNO),
                GenerateBuildLanFilterClause(buildLan),
                GenerateOSTypeFilterClause(osType),
                GenerateOSLanguageFilterClause(osLanguage),
                GenerateRuleIDFilterClause(ruleid),
                GenerateAssemblyNameFilterClause(assemblyName),
                GenerateTypeIDFilterClause(typeID),
                GenerateUserNameFilterClause(username),
                GeneratePageTitleFilterClause(pagetitle),
                GenerateReviewCommentsFilterClause(reviewComments),
                GenerateBuildTypeFilterClause(buildtype),
                GenerateSearchDatetimeFilterClause(searchDatetime));
            return filter;
        }
        private string GenerateSQLSort(string sortBy)
        {
            string sortColumn = string.Empty;
            switch (sortBy)
            {
                case "Page Title":
                    sortColumn = "[UIContents].[UIName]";
                    break;
                case "Build No.":
                    sortColumn = "[BuildInfo].[BuildNo]";
                    break;
                case "Upload Time":
                    sortColumn = "[UIContents].[DateUploaded]";
                    break;
                default:
                    sortColumn = "[Results].[ResultID]";
                    break;
            }
            return sortColumn;
        }
        #endregion

        #region Rule ID string generator
        private string GetRuleIDs(string[] ruleList, Dictionary<string, AvailableRule> availableRules)
        {
            string ruleIDs = string.Empty;
            foreach (string s in ruleList)
            {
                foreach (AvailableRule rule in availableRules.Values)
                {
                    if (rule.RuleName == s)
                    {
                        ruleIDs += "," + rule.RuleID;
                        break;
                    }
                }
            }
            if (!string.IsNullOrEmpty(ruleIDs))
                ruleIDs = ruleIDs.Remove(0, 1);
            return ruleIDs;
        }
        private List<string> GetRuleIDsList(string[] ruleList, Dictionary<string, AvailableRule> availableRules)
        {
            List<string> ruleIDsArray = new List<string>();
            foreach (string s in ruleList)
            {
                foreach (AvailableRule rule in availableRules.Values)
                {
                    if (rule.RuleName == s)
                    {
                        ruleIDsArray.Add(rule.RuleID);
                        break;
                    }
                }
            }
            return ruleIDsArray;
        }
        private void SeparateRuleIDs(string sourceRuleID, ref string osTypeLanguageRelatedRuleID, ref string osTypeNotRelatedRuleID,
            ref string osTypeLanguageNotRelatedRuleID)
        {
            string[] sourceIDs = sourceRuleID.Split(',');
            foreach (string ruleID in sourceIDs)
            {
                if (osTypeNotRelatedRuleIDs.Contains(ruleID))
                    osTypeNotRelatedRuleID += ',' + ruleID;
                else if (osTypeLanguageNotRelatedRuleIDs.Contains(ruleID))
                    osTypeLanguageNotRelatedRuleID += ',' + ruleID;
                else
                    osTypeLanguageRelatedRuleID += ',' + ruleID; ;
            }
            if (osTypeNotRelatedRuleID.StartsWith(","))
                osTypeNotRelatedRuleID = osTypeNotRelatedRuleID.TrimStart(',');
            if (osTypeLanguageNotRelatedRuleID.StartsWith(","))
                osTypeLanguageNotRelatedRuleID = osTypeLanguageNotRelatedRuleID.TrimStart(',');
            if (osTypeLanguageRelatedRuleID.StartsWith(","))
                osTypeLanguageRelatedRuleID = osTypeLanguageRelatedRuleID.TrimStart(',');
        }
        #endregion

        //v-yiwzha: Get the latest pass result of current result
        private string GetLatestPassResult(string resultID)
        {
            string result = string.Empty;
            string sql = "select top 1 BuildNo from BuildInfo where BuildID in(Select distinct BuildID from UIContents where ContentID in(Select ContentID from Results where ContentID in(select ContentID from UIContents where ContentID in(select ContentID from AssemblyLink where TypeID in (select TypeID from AssemblyLink where ContentID in (select ContentID from Results where ResultID = " + resultID + ") and IsPageIdentifier = 1))and BuildID in(select BuildID from BuildInfo where Language in(select Language from BuildInfo where BuildID in (Select BuildID from UIContents where ContentID in (select ContentID from Results where ResultID = " + resultID + ")))))and RuleID in (select RuleID from Results where ResultID = " + resultID + ")and ResultType = 'Pass')) order by BuildNo desc";
            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand command = new SqlCommand(sql, conn);
            command.CommandType = CommandType.Text;
            command.CommandTimeout = 300000;

            conn.Open();
            try
            {

                DataTable table = new DataTable();
                SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                dataAdapter.Fill(table);
                result = "[TheLatestPassBuildIs" + table.Rows[0][0].ToString() + "]";
                conn.Close();
            }
            catch (Exception ex)
            {
                conn.Close();
                result = ex.Message;
            }
            if (result == "There is no row at position 0.")
            {
                result = "[NoPassHistory]";
            }
            return result;
        }

        [WebMethod]
        public List<LogRecords> QueryLogExceptions(string buildNO, string username, string exValue)
        {
            List<LogRecords> records = new List<LogRecords>();
            string sql = "select L.logid,L.exceptioncontent,L.lastmodifytime,L.inserttime,A.buildno,B.username,B.ostype,L.exceptioncount from logexception as L join buildinfo as A on L.buildid=A.buildid join clients as B on L.clientid=b.clientid where A.buildno in (" + GenerateSQLCondition(buildNO) + ")";
            if (username != "")
            {
                sql += " and B.username like '%" + username + "%'";
            }
            if (exValue != "")
            {
                sql += " and L.exceptioncontent like '%" + exValue + "%'";
            }
            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand command = new SqlCommand(sql, conn);
            command.CommandType = CommandType.Text;
            command.CommandTimeout = 300000;

            conn.Open();
            DataTable table = new DataTable();
            SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
            dataAdapter.Fill(table);

            foreach (DataRow dr in table.Rows)
            {
                LogRecords r = new LogRecords();
                r.LogID = (int)dr["logid"];
                r.ExceptionContent = dr["exceptioncontent"].ToString();
                //r.ExceptionContent = Regex.Replace(r.ExceptionContent, "(\r\n)", "<br/>");
                r.LTime = dr["lastmodifytime"].ToString();
                r.FTime = dr["inserttime"].ToString();
                r.BuildNo = dr["buildno"].ToString();
                r.UserName = dr["username"].ToString();
                r.OSType = dr["ostype"].ToString();
                r.Count = (int)dr["exceptioncount"];
                records.Add(r);
            }
            return records;
        }
        [WebMethod]
        public int GetTotalRecords1(string buildNO, string buildLan, string osType, string osLanguage, string ruleid,
            string resultTypes, string assemblyName, string typeID, string username, string pagetitle, string reviewComments,
            string ReviewedType, string buildtype, string resultid, string searchDateTime, bool getLatest)
        {
            string sql = string.Empty;
            if (!string.IsNullOrEmpty(resultid))
            {
                sql = GenerateCountSQLByResultID(resultid);
            }
            else if (getLatest)
            {
                sql = GenerateLatestCountSQL(buildNO, buildLan, osType, osLanguage, ruleid, resultTypes, assemblyName, typeID,
                    username, pagetitle, reviewComments, ReviewedType, buildtype, searchDateTime, getLatest);
            }
            else
            {
                sql = GenerateCountSQL(buildNO, buildLan, osType, osLanguage, ruleid, resultTypes, assemblyName, typeID,
                    username, pagetitle, reviewComments, ReviewedType, buildtype, searchDateTime);
            }

            int count = (int)ExecuteScalar(sql);
            return count;
        }
        [WebMethod]
        public int GetTotalRecords(string buildNO, string buildLan, string osType, string osLanguage, string ruleid,
            string resultTypes, string assemblyName, string typeID, string username, string pagetitle, string reviewComments,
            string ReviewedType, string resultid, string searchDateTime, bool getLatest, int latestDays)
        {
            SQLUtility.osRelatedRuleIDs = osRelatedRuleIDs;
            SQLUtility.osNotRelatedRuleIDs = osNotRelatedRuleIDs;
            int count = SQLUtility.GetCountOfTotalRecords(buildNO, buildLan, osType, osLanguage, ruleid, resultTypes, assemblyName, typeID,
                username, pagetitle, reviewComments, ReviewedType, resultid, searchDateTime, getLatest, latestDays);
            return count;
        }
        [WebMethod]
        public List<ViewCapturedReport> BindingTable1(string buildNO, string buildLan, string osType, string osLanguage, string ruleid,
            string resultTypes, string assemblyName, string typeID, string pageIndex, string username, string pagetitle, string reviewComments,
            string ReviewedType, string buildtype, string resultid, string sortBy, string searchDateTime, string currentPageResults,
            string diffentReviewStatusResultsCount, bool getLatest)
        {
            List<ViewCapturedReport> resultReportList = new List<ViewCapturedReport>();
            int page = Int32.Parse(pageIndex);
            int diffentReviewStatusResults = int.Parse(diffentReviewStatusResultsCount);

            string sql = string.Empty;
            if (!string.IsNullOrEmpty(resultid))
            {
                sql = GenerateListSQLByResultID(resultid);
            }
            else if (currentPageResults != "undefined")
            {
                sql = GenerateListSQLByResultID(currentPageResults, sortBy, assemblyName);
            }
            else if (getLatest)
            {
                sql = GenerateLatestListSQL(buildNO, buildLan, osType, osLanguage, ruleid, resultTypes, assemblyName, typeID, page,
                    username, pagetitle, reviewComments, ReviewedType, buildtype, searchDateTime, sortBy, 20, diffentReviewStatusResults);
            }
            else
            {
                sql = GenerateListSQL(buildNO, buildLan, osType, osLanguage, ruleid, resultTypes, assemblyName, typeID, page,
                    username, pagetitle, reviewComments, ReviewedType, buildtype, searchDateTime, sortBy, 20, diffentReviewStatusResults);
            }

            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand command = new SqlCommand(sql, conn);
            command.CommandType = CommandType.Text;
            command.CommandTimeout = 300000;

            conn.Open();
            DataTable table = new DataTable();
            SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
            dataAdapter.Fill(table);

            foreach (DataRow dr in table.Rows)
            {
                ViewCapturedReport resultReport = new ViewCapturedReport();
                resultReport.ResultID = (int)dr["resultid"];
                resultReport.BuildNo = dr["buildno"].ToString();
                resultReport.RuleName = dr["rulename"].ToString();
                resultReport.UIName = dr["uiname"].ToString();
                resultReport.ResultType = dr["ResultType"].ToString();
                resultReport.UserName = dr["UserName"].ToString();
                resultReport.Language = dr["Language"].ToString();
                resultReport.OSType = dr["OSType"].ToString();
                resultReport.DateUploadedStr = dr["dateuploaded"].ToString();
                resultReport.CreateDateStr = dr["createdate"].ToString();
                resultReport.ReviewFlag = (int)dr["ReviewFlag"];
                resultReport.ReviewLog = dr["ReviewLog"].ToString();
                resultReportList.Add(resultReport);
            }
            conn.Close();

            return resultReportList;
        }
        [WebMethod]
        public List<ViewCapturedReport> BindingTable(string buildNO, string buildLan, string osType, string osLanguage, string ruleid,
            string resultTypes, string assemblyName, string typeID, string pageIndex, string username, string pagetitle, string reviewComments,
            string ReviewedType, string resultid, string sortBy, string searchDateTime, string currentPageResults,
            string diffentReviewStatusResultsCount, bool getLatest, int latestDays)
        {
            SQLUtility.osRelatedRuleIDs = osRelatedRuleIDs;
            SQLUtility.osNotRelatedRuleIDs = osNotRelatedRuleIDs;
            List<ViewCapturedReport> resultReportList = SQLUtility.GetDetailResultsOfSpecifiedPage(buildNO, buildLan, osType, osLanguage, ruleid,
                resultTypes, assemblyName, typeID, pageIndex, username, pagetitle, reviewComments, ReviewedType, resultid, sortBy, searchDateTime,
                currentPageResults, diffentReviewStatusResultsCount, getLatest, latestDays);
            return resultReportList;
        }
        [WebMethod]
        public List<GroupTitle> GetGroupResult(string buildNO, string buildLan, string osType, string osLanguage, string ruleid, string resultTypes,
            string assemblyName, string typeID, string username, string pagetitle, string ReviewedType, string buildtype)
        {
            List<GroupTitle> results = new List<GroupTitle>();
            string columns = "max([UIContents].[UIName]) AS [UIName], count(*), [UIContents].[ContentID] AS [ContentID] ";
            string prefix = "select " + columns + "FROM [dbo].[Results] " + joinTablesNoAssembly + " Where ";
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(pagetitle))
            {
                prefix = prefix + " [Clients].UserName like N'%" + username + "%' and [UIContents].UIName like N'%" + pagetitle + "%' ";
            }
            if (!string.IsNullOrEmpty(username) && string.IsNullOrEmpty(pagetitle))
            {
                prefix = prefix + " [Clients].UserName like N'%" + username + "%' ";
            }
            if (string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(pagetitle))
            {
                prefix = prefix + "[UIContents].UIName like N'%" + pagetitle + "%' ";
            }

            if (ReviewedType != "-1")
            {
                string[] reviewedarray = ReviewedType.Split('|');
                string rvresult = string.Empty;
                foreach (string s in reviewedarray)
                {
                    rvresult = rvresult + "'" + s + "',";
                }
                if (prefix.EndsWith("Where "))
                {
                    prefix = prefix + "[Results].ReviewFlag in (" + rvresult.Substring(0, rvresult.Length - 1) + ") ";
                }
                else
                {
                    prefix = prefix + " And [Results].ReviewFlag in (" + rvresult.Substring(0, rvresult.Length - 1) + ") ";
                }
            }

            if (buildtype != "All")
            {
                string[] buildtypearray = buildtype.Split('|');
                string btresult = string.Empty;
                foreach (string s in buildtypearray)
                {
                    btresult = btresult + "'" + s + "',";
                }
                if (prefix.EndsWith("Where "))
                {
                    prefix = prefix + "[UIContents].Reserve2 in (" + btresult.Substring(0, btresult.Length - 1) + ") ";
                }
                else
                {
                    prefix = prefix + " And [UIContents].Reserve2 in (" + btresult.Substring(0, btresult.Length - 1) + ") ";
                }
            }

            string sql = GenerateFormatString(prefix, assemblyName, typeID, ruleid, resultTypes, buildNO, buildLan, osType, osLanguage);
            sql = sql + "Group By [UIContents].[ContentID]";
            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand command = new SqlCommand(sql, conn);
            command.CommandType = CommandType.Text;
            command.CommandTimeout = 300000;

            conn.Open();
            DataTable table = new DataTable();
            SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
            dataAdapter.Fill(table);

            foreach (DataRow dr in table.Rows)
            {
                GroupTitle g = new GroupTitle();
                g.UIName = dr[0].ToString();
                g.Count = dr[1].ToString();
                g.ContentID = dr[2].ToString();
                if (!string.IsNullOrEmpty(g.UIName))
                {
                    results.Add(g);
                }
            }
            conn.Close();

            return results;
        }
        [WebMethod(EnableSession = true)]
        public int DeleteAuthutication(string userName, string password)
        {
            if (userName == string.Empty && password == string.Empty)
            {
                Session["CanDelete"] = 1;
                return 1;
            }
            else
            {
                Session["CanDelete"] = 0;
                return 0;
            }
        }
        [WebMethod(EnableSession = true)]
        public void CancelAuthutication()
        {
            Session["CanDelete"] = 0;
        }
        [WebMethod(EnableSession = true)]
        public int DeleteByResultID(string resultID)
        {
            if (Session["CanDelete"].ToString() == "0")
            {
                return 0;
            }
            // 1. Query UI Content ID
            string sql = string.Format("select contentid from results where resultid={0}", resultID);
            string contentid = ExecuteScalar(sql).ToString();

            // 2. Query AssemblyLinkIDs
            string sql1 = string.Format("select assemblylinkid from assemblylink where contentid={0}", contentid);
            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand command = new SqlCommand(sql1, conn);
            command.CommandType = CommandType.Text;
            command.CommandTimeout = 30000;
            conn.Open();
            DataTable table = new DataTable();
            SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
            dataAdapter.Fill(table);

            string linkIDS = "";
            foreach (DataRow r in table.Rows)
            {
                linkIDS += r[0].ToString() + "|";
            }
            if (linkIDS != "")
            {
                linkIDS = linkIDS.Substring(0, linkIDS.Length - 1);
                linkIDS = "'" + linkIDS + "'";
            }
            else
            {
                linkIDS = "''";
            }
            conn.Close();

            // 2. Insert UI to DeletedContent Table
            string insertSql = string.Format("INSERT INTO [SulpHur].[dbo].[DeletedContents]" +
           " ([ContentID],[GUID],[ClientID],[BuildID],[UIName],[UIContent],[UIScreenShot],[IsWebUI],[DateUploaded]"
           + ",[TraceID],[Reserve1],[Reserve2],[Reserve3],[Reserve4],[Reserve5],[LaunchedFrom],[WindowHierarchy]"
           + ",[AssemblyLinkIDs]) "
           + "SELECT [ContentID],[GUID],[ClientID],[BuildID],[UIName],[UIContent],[UIScreenShot],[IsWebUI]"
           + ",[DateUploaded],[TraceID],[Reserve1],[Reserve2],[Reserve3],[Reserve4],[Reserve5],[LaunchedFrom]"
           + ",[WindowHierarchy],{0} FROM [SulpHur].[dbo].[UIContents]"
           + " where [ContentID]={1}", linkIDS, contentid);
            command = new SqlCommand(insertSql, conn);
            command.CommandType = CommandType.Text;
            command.CommandTimeout = 30000;
            conn.Open();
            command.ExecuteNonQuery();
            conn.Close();

            // 3. Delete From Results, RuleStatus, AssemblyLink, Contents
            string deleteSql = string.Format("delete from results where contentid={0}", contentid);
            DeleteSQL(deleteSql);

            deleteSql = string.Format("delete from rulestatus where contentid={0}", contentid);
            DeleteSQL(deleteSql);

            deleteSql = string.Format("delete from assemblylink where contentid={0}", contentid);
            DeleteSQL(deleteSql);

            deleteSql = string.Format("delete from uicontents where contentid={0}", contentid);
            DeleteSQL(deleteSql);

            return 1;
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
                Dictionary<string, AvailableRule> availableRules = pq.QueryAvailableRules();
                hash.Add("AvailableRules", availableRules);
                string basicRules = System.Configuration.ConfigurationManager.AppSettings["BasicRules"].ToString();
                hash.Add("BasicRules", basicRules);
                string[] basicRuleList = basicRules.Split('|');
                string basicRuleIDs = GetRuleIDs(basicRuleList, availableRules);
                hash.Add("BasicRuleIDs", basicRuleIDs);
                string osTypeNotRelatedRules = System.Configuration.ConfigurationManager.AppSettings["OSTypeNotRelatedRules"].ToString();
                string[] osTypeNotRelatedRuleList = osTypeNotRelatedRules.Split('|');
                osTypeNotRelatedRuleIDs = GetRuleIDsList(osTypeNotRelatedRuleList, availableRules);
                string osLanguageNotRelatedRules = System.Configuration.ConfigurationManager.AppSettings["OSTypeLanguageNotRelatedRules"].ToString();
                string[] osLanguageNotRelatedRuleList = osLanguageNotRelatedRules.Split('|');
                osTypeLanguageNotRelatedRuleIDs = GetRuleIDsList(osLanguageNotRelatedRuleList, availableRules);

                string osRelatedRules = System.Configuration.ConfigurationManager.AppSettings["OSRelatedRules"].ToString();
                string[] osRelatedRuleList = osRelatedRules.Split('|');
                osRelatedRuleIDs = GetRuleIDsList(osRelatedRuleList, availableRules);
                string osNotRelatedRules = System.Configuration.ConfigurationManager.AppSettings["OSNotRelatedRules"].ToString();
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
        public List<UIRecord> QueryUIByBuildNo(string buildno)
        {
            UIRecord record = new UIRecord();
            return record.QueryUIRecords(buildno, "", "");
        }
        [WebMethod]
        public UIRecord QueryRecordByResultID(string resultID)
        {
            UIRecord record = new UIRecord();
            record.QueryRecordByResultID(int.Parse(resultID));
            return record;
        }
        [WebMethod]
        public UIRecord QueryUIByContentID(string contentID)
        {
            UIRecord record = new UIRecord();
            record.QueryRecordByContentID(int.Parse(contentID));
            return record;
        }
        [WebMethod]
        public List<string> QueryAvaliableRecoverBuild()
        {
            List<string> buildList = new List<string>();
            List<SulpHurBuildInfo> buildTemp = new List<SulpHurBuildInfo>();
            ISulpHurTable client = SulpHurTableFactoryBase.Instance().GetSulpHurTable();
            buildTemp = client.QueryTable<SulpHurBuildInfo>("select * from buildinfo where buildno in (select buildno from deletedcontents)");
            foreach (SulpHurBuildInfo sbi in buildTemp)
            {
                buildList.Add(sbi.BuildNo);
            }
            return buildList;
        }
        [WebMethod]
        public string ClearUIByBuildNo(string buildno)
        {
            string result;
            try
            {
                IBuildClean cleaner = SulpHurTableFactoryBase.Instance().GetBuildClean();
                cleaner.CleanBuild(buildno);
                result = "success";
            }
            catch
            {
                result = "Fail to clear UI.";
            }
            return result;
        }
        [WebMethod]
        public string DeleteByContentID(string contentID)
        {
            string result;
            try
            {
                IBuildClean cleaner = SulpHurTableFactoryBase.Instance().GetBuildClean();
                cleaner.CleanUI(int.Parse(contentID));
                result = "success";
            }
            catch
            {
                result = "Fail to clear UI.";
            }
            return result;
        }
        [WebMethod]
        public string RescanByContentID(string contentID)
        {
            string result;
            try
            {
                IWinService winService = WCFServiceWrapperBase.Instance().GetWindowsService(this.serverName);
                List<int> contentIDs = new List<int>();
                contentIDs.Add(int.Parse(contentID));
                List<string> rules = new List<string>();
                RuleInfo ri = new RuleInfo();
                foreach (RuleInfo cr in ri.AvailableRules)
                {
                    rules.Add(cr.RuleName);
                }
                winService.RescanByContentID(contentIDs, rules);
                result = "success";
            }
            catch
            {
                result = "Fail to clear UI.";
            }
            return result;
        }
        [WebMethod]
        public string RescanByBuildNo(string buildno)
        {
            string result;
            try
            {
                string sql = string.Format("select A.contentid from uicontents as A join Buildinfo as B on A.buildid=B.buildid where B.buildno='{0}'", buildno);
                ISulpHurTable table = SulpHurTableFactoryBase.Instance().GetSulpHurTable();
                DataTable dt = table.QueryTable(sql);
                List<int> contentIDList = new List<int>();
                foreach (DataRow r in dt.Rows)
                {
                    contentIDList.Add((int)r["contentid"]);
                }
                IWinService winService = WCFServiceWrapperBase.Instance().GetWindowsService(this.serverName);
                List<string> rules = new List<string>();
                RuleInfo ri = new RuleInfo();
                foreach (RuleInfo cr in ri.AvailableRules)
                {
                    rules.Add(cr.RuleName);
                }
                winService.RescanByContentID(contentIDList, rules);
                result = "success";
            }
            catch
            {
                result = "Fail to clear UI.";
            }
            return result;
        }
        [WebMethod]
        public string RecoverUIByBuildNo(string buildno)
        {
            string result;
            try
            {
                IBuildClean cleaner = SulpHurTableFactoryBase.Instance().GetBuildClean();
                cleaner.RecoverBuild(buildno);
                result = "success";
            }
            catch
            {
                result = "Fail to recover UI.";
            }
            return result;
        }
        [WebMethod]
        public void SetLogLevel(string level)
        {
            LogSwitchLevel tag = (LogSwitchLevel)Enum.Parse(typeof(LogSwitchLevel), level.ToUpper());
            IWinService winService = WCFServiceWrapperBase.Instance().GetWindowsService(this.serverName);
            winService.SetLogSwitch(tag);
        }
        [WebMethod]
        public int UpdateReviewedByResultID(string resultID, string reviewflag, string reviewlog)
        {
            SqlConnection conn = new SqlConnection(connStr);
            try
            {
                string sql = string.Format("Update Results set ReviewFlag ='{0}',ReviewLog = '{1}' ", int.Parse(reviewflag), reviewlog);
                sql = sql + "where ResultID in (" + resultID.Trim(',') + ")";
                SqlCommand command = new SqlCommand(sql, conn);
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 30000;
                conn.Open();
                command.ExecuteNonQuery();
            }
            catch
            {
                throw;
            }
            finally
            {
                conn.Close();
            }
            return 1;
        }

        //v-danpgu file multi bug
        [WebMethod]
        public string GetRetryListMethod(string openby)
        {
            openby = openby.Replace("\\", "");
            string retryList = System.IO.File.ReadAllText(@"\\scfs\Users\INTL\RetryResultIDs\retryResultIDs_" + openby + ".txt");
            return retryList;
        }

        [WebMethod]
        public string GetBugIDListMethod(string openby)
        {
            openby = openby.Replace("\\", "");
            string bugIDList = System.IO.File.ReadAllText(@"\\scfs\Users\INTL\RetryResultIDs\bugIDList_" + openby + ".txt");
            return bugIDList;
        }

        //V-DANPGU FILEBUG
        [WebMethod]
        public string FileBug(string openby, string resultid, string attachfilepath)
        {
            if (string.IsNullOrEmpty(attachfilepath)) attachfilepath = "";
            string fileBugResult = string.Empty;
            UIRecord record = new UIRecord();
            try
            {
                record.QueryRecordByResultID(int.Parse(resultid));
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }

            //openby:
            for (int c = 0; c < openby.Length; c++)
            {
                if (openby[c] == '\\')
                {
                    openby = openby.Substring(c + 1);
                    break;
                }
            }
            string pagetitle = record.PageTitle;
            string buildNO = record.BuildNo;
            string buildLan = record.BuildLanguage;
            string assignTo = openby;
            string osType = record.OSType;
            string rulename = record.RuleName;
            string resultTypes = record.ResultType;
            string path = @"\SCCM\Localization";
            //page Title
            if (pagetitle.Length > 40)
            {
                pagetitle = pagetitle.Substring(0, 40) + "~~~";
            }
            //log resultLog
            string resultLog = string.Empty;
            SulpHurEntities entities = new SulpHurEntities();
            int resultidToInt = Int32.Parse(resultid);
            Result result = entities.Results.FirstOrDefault(itemresult => itemresult.ResultID.Equals(resultidToInt));//????
            resultLog = result.ResultLog;
            if (string.IsNullOrEmpty(resultLog))
            {
                resultLog = "No any error on this page.";
            }
            string logMessage = resultLog;
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
                    for (int c = 0; c < info.Length; c++)
                    {
                        if (info[c] == '\"') count++;
                        if (count == 1 && tag == 0)
                        {
                            start1 = c;
                            tag++;
                        }
                        if (count == 2 && tag == 1)
                        {
                            end = c;
                            tag++;
                            controlname1 = info.Substring(start1, end - start1);
                            if (end - start1 > 40)
                            {
                                shortcontrolname1 = info.Substring(start1, 40);
                            }
                        }
                        if (count == 3 && tag == 2)
                        {
                            start2 = c;
                            tag++;
                        }
                        if (count == 4 && tag == 3)
                        {
                            end = c;
                            controlname2 = info.Substring(start2, end - start2);
                            if (end - start2 > 40)
                            {
                                shortcontrolname2 = info.Substring(start2, 40);
                            }
                            info = info.Substring(0, c + 1);
                            break;
                        }
                        if (c == info.Length - 1)
                        {
                            info = resultid.ToString();
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
                //Page Title
                if (string.IsNullOrEmpty(info))
                {
                    info = "No any error on this page.";
                }
                pagetitle = rulename + " [" + resultTypes + "] " + buildLan + "_" + pagetitle + "_" + GetLatestPassResult(resultid) + "_" + info;
                if (info != string.Empty)
                {
                    pagetitle = pagetitle.Replace("': \"", "' in \"");
                    pagetitle = pagetitle.Replace("'", "‘");
                    pagetitle = pagetitle.Replace("\\", "");
                    pagetitle = pagetitle.Replace("/", "");
                    pagetitle = pagetitle.Replace(":", "");
                    pagetitle = pagetitle.Replace("*", "");
                    pagetitle = pagetitle.Replace("?", "");
                    pagetitle = pagetitle.Replace("<", "");
                    pagetitle = pagetitle.Replace(">", "");
                    pagetitle = pagetitle.Replace("|", "");
                    pagetitle = pagetitle.Replace("\"", "'");
                    pagetitle = pagetitle.Replace("\"", "‘");
                    pagetitle = pagetitle.Replace(".", "");
                    pagetitle = pagetitle.Replace("\r", "");
                    pagetitle = pagetitle.Replace("\n", "");
                }
            }

            //bug 478039 + bug 478284
            else
            {
                pagetitle = rulename + " [" + resultTypes + "] " + buildLan + "_" + pagetitle + "_" + GetLatestPassResult(resultid) + "_" + resultLog;
                pagetitle = pagetitle.Replace("'", "‘");
                pagetitle = pagetitle.Replace("\"", "‘");
                pagetitle = pagetitle.Replace("\r\n", "");
                //v-edy: bug485629
                pagetitle = pagetitle.Replace("\n", "");
                if (pagetitle.Length > 250)
                {
                    pagetitle = pagetitle.Substring(0, 247) + "~~~";
                }
            }
            string pagetitle1 = pagetitle;
            //Build No
            string p1 = string.Empty;
            string p2 = string.Empty;
            int i = 0;
            List<char> buildNoArray = buildNO.ToList();
            while (buildNoArray[0] != '.')
            {
                p1 = p1 + buildNO[0];
                buildNoArray.Remove(buildNoArray[0]);
                i++;
            }
            if (i == 1) p1 = '0' + p1;
            buildNoArray.Remove(buildNoArray[0]);
            int j = 0;
            while (buildNoArray[0] != '.')
            {
                p2 = p2 + buildNoArray[0];
                buildNoArray.Remove(buildNoArray[0]);
                j++;
            }
            if (j == 1) p2 = '0' + p2;
            string p3 = string.Empty;
            foreach (char c in buildNoArray)
            {
                p3 = p3 + c;
            }
            buildNO = p1 + '.' + p2 + p3;

            //v-edy: bug481718&bug481854
            string filepath = Path.Combine(Server.MapPath("~/"), "filebug-BugFields.json");
            string temp = "\\\\scfs\\Users\\INTL\\SulphurBugFiles\\filebug-BugFields_temp_" + resultid + ".json";

            //v-edy : change filebug.exe tool run on server
            string tempFilePath = @"C:\SulpHur\SulpHurService\FileBugDetail\" + resultid + ".txt";

            switch (buildLan)
            {
                case "ENU":
                    if (ConfigurationManager.AppSettings["Temp_ENU_ITCodeDefect_BATranslation"].ToString().Contains(rulename))
                    {
                        filepath = Path.Combine(Server.MapPath("~/"), "filebug-BugFields_E_ITCodeDefect_BATranslation.json");
                    }
                    else if (ConfigurationManager.AppSettings["Temp_ENU_ITCodeDefect_BALocalizability"].ToString().Contains(rulename))
                    {
                        filepath = Path.Combine(Server.MapPath("~/"), "filebug-BugFields_E_ITCodeDefect_BALocalizability.json");
                    }
                    else if (ConfigurationManager.AppSettings["Temp_ENU_ITCodeDefect_BAAccessbility"].ToString().Contains(rulename))
                    {
                        filepath = Path.Combine(Server.MapPath("~/"), "filebug-BugFields_E_ITCodeDefect_BAAccessbility.json");
                    }
                    break;
                default:
                    if (ConfigurationManager.AppSettings["Temp_NENU_ITLocalization_BATranslation"].ToString().Contains(rulename))
                    {
                        filepath = Path.Combine(Server.MapPath("~/"), "filebug-BugFields_N_ITLocalization_BATranslation.json");
                    }
                    else if (ConfigurationManager.AppSettings["Temp_NENU_ITLocalization_BALocalization"].ToString().Contains(rulename))
                    {
                        filepath = Path.Combine(Server.MapPath("~/"), "filebug-BugFields_N_ITLocalization_BALocalization.json");
                        if (pagetitle.Contains("miss hotkeys") || "Tab Order Rule" == rulename)
                        {
                            return "No need to file bug about Tab Order or Miss Hotkey on non-ENU lan: " + resultid;
                        }
                    }
                    break;
            }
            //Buil Lan
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("ZHH", "Chinese (Hong Kong SAR)");
            dic.Add("CHS", "Chinese (Simplified)");
            dic.Add("CHT", "Chinese (Traditional)");
            dic.Add("ENU", "Core Code");
            dic.Add("FRA", "French");
            dic.Add("JPN", "Japanese");
            dic.Add("DEU", "German");
            dic.Add("RUS", "Russian");
            dic.Add("NLD", "ICP Dutch");
            dic.Add("HUN", "ICP Hungarian");
            dic.Add("ITA", "ICP Italian");
            dic.Add("KOR", "Korean");
            dic.Add("PLK", "ICP Polish");
            dic.Add("PTB", "ICP Portuguese (Brazil)");
            dic.Add("PTG", "ICP Portuguese (Portugal)");
            dic.Add("CSY", "ICP Czech");
            dic.Add("ESN", "ICP Spanish");
            dic.Add("SVE", "ICP Swedish");
            dic.Add("TRK", "ICP Turkish");
            buildLan = dic[buildLan];

            //Description
            string template = @"
This bug is filed from Sulphur tool, please review it before assign it to the Loc team.

Page Title: {0}
Rule Name : {1}
Reuslt Type : {2}
Build Language: {3}
ResultID: {4}

Latest Pass Build Infomation: {5}

Please go to attached pdf file for the detailed info.
";
            string description = string.Format(
               template,
               pagetitle1,
               rulename,
               resultTypes,
               buildLan,
               resultid,
               GetLatestPassResult(resultid));

            //v-edy: bug481854
            //string filepath = Path.Combine(Server.MapPath("/"), "filebug-BugFields.json");
            //string temp = "\\\\scfs\\Users\\INTL\\SulphurBugFiles\\filebug-BugFields_temp.json";
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                //startInfo.Arguments = "/c net use \\\\scfs " + Microsoft.ConfigurationManagement.Test.KeyVault.CommonSecretIdentifiers.SMSAccessSecretID + " /user:smsaccess";
                Process.Start(startInfo);
                Thread.Sleep(400);

                FileAttributes fa;
                if (File.Exists(temp))
                {
                    fa = File.GetAttributes(temp);
                    if ((fa & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        File.SetAttributes(temp, FileAttributes.Normal);
                    }
                }
                File.Copy(filepath, temp, true);
                fa = File.GetAttributes(temp);
                if ((fa & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    File.SetAttributes(temp, FileAttributes.Normal);
                }
                string content = File.ReadAllText(temp)
                    .Replace("$SMSLan$", buildLan)
                    .Replace("$Description$", description);
                File.WriteAllText(temp, content);

                //v-edy : change filebug.exe tool run on server 
                using (FileStream fs1 = new FileStream(tempFilePath, FileMode.Create, FileAccess.ReadWrite))
                {
                    string fileContent = openby + "\r\n" + pagetitle + "\r\n" + assignTo + "\r\n" + path + "\r\n" + buildNO + "\r\n" + temp + "\r\n" + attachfilepath;
                    StreamWriter sw = new StreamWriter(fs1, System.Text.Encoding.Unicode);
                    sw.Write(fileContent);
                    sw.Flush();

                }
                string filebugFileFolder = ConfigurationManager.AppSettings["filebugFileFolder"].ToString();
                DirectoryInfo folder = new DirectoryInfo(filebugFileFolder);
                bool completeFlag = false;
                while (true)
                {

                    foreach (FileInfo f in folder.GetFiles("*.txt"))
                    {
                        if (f.Name.Contains(resultid + "_"))
                        {
                            completeFlag = true;
                            break;
                        }
                    }
                    if (completeFlag)
                    {
                        break;
                    }
                }
                foreach (FileInfo f in folder.GetFiles("*.txt"))
                {
                    if (f.Name.Contains(resultid))
                    {
                        fileBugResult = f.Name.Substring(f.Name.IndexOf("_") + 1, 6);
                        f.Delete();
                    }
                }
                //File.Copy(tempFilePath, "\\\\scfs\\Users\\INTL\\TempFile\\" + resultid + ".txt", true);

                //Process process = new Process();
                //process.StartInfo.FileName = "cmd.exe";
                //process.StartInfo.Arguments = "/c " + Path.Combine(Server.MapPath("/"), "filebug.exe")
                //    + " /OpenedBy:" + openby + " /Title:\"" + pagetitle + "\" /AssignedTo:" + assignTo + " /Path:\"" + path
                //    + "\" /OpenBuild:" + buildNO + " /BugFields:\"" + temp + "\" /Attachments:\"" + attachfilepath + "\"";
                //process.StartInfo.UseShellExecute = false;
                //process.StartInfo.RedirectStandardOutput = true;
                //process.StartInfo.CreateNoWindow = true;

                //process.Start();
                //string output = process.StandardOutput.ReadToEnd();
                //int idx = output.IndexOf("File bug successfully:");

                //if (idx > 0)
                //{
                //    fileBugResult = output.Substring(idx).Replace(".\r\n", "");
                //}
                //else
                //{
                //    fileBugResult = output;
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                File.Delete(temp);
                //v-edy : change filebug.exe tool run on server
                //File.Delete(tempFilePath);
            }
            return fileBugResult;
        }
    }

    //return array data split by vertical
    public interface IProductQuery
    {
        string QueryAvailableProductBuilds();
        string QueryAvailableOSTypes();
        string QueryAvailableOSLanguage();
        string QueryAvailableTypes();
        string QueryAvailableCapturedLanguages();
        string QueryAvailableCapturedBuilds();
        string QueryAvailableAssembly();
        Dictionary<string, string> QueryRules();
        Dictionary<string, string> QueryAssemblyTypesInfo();
        Dictionary<string, AvailableRule> QueryAvailableRules();
    }
    public class LogRecords
    {
        public int LogID { get; set; }
        public string BuildNo { get; set; }
        public string UserName { get; set; }
        public string OSType { get; set; }
        public string ExceptionContent { get; set; }
        public string FTime { get; set; }
        public string LTime { get; set; }
        public int Count { get; set; }
    }
    public class ProductQuery : IProductQuery
    {
        string vertical = "|";
        string connStr = ConfigurationManager.ConnectionStrings["ADOConn"].ToString();

        public string QueryAvailableProductBuilds()
        {
            string sql = "select distinct buildno from buildtypes order by buildno asc";
            return GetListResult(sql);
        }
        public string QueryAvailableOSTypes()
        {
            string sql = "select distinct ostype from clients order by ostype asc";
            return GetListResult(sql);
        }
        public string QueryAvailableCapturedLanguages()
        {
            string sql = "select distinct language from buildinfo order by language asc";
            return GetListResult(sql);
        }
        public string QueryAvailableCapturedBuilds()
        {
            string sql = "select distinct buildno from buildinfo order by buildno desc";
            return GetListResult(sql);
        }
        public string QueryAvailableAssembly()
        {
            string sql = "select distinct assemblyname from assemblyinfo order by assemblyname asc";
            return GetListResult(sql);
        }
        public string QueryAvailableOSLanguage()
        {
            string sql = "select distinct oslanguage from clients order by oslanguage asc";
            return GetListResult(sql);
        }

        public string QueryAvailableTypes()
        {
            string sql = "select distinct typeid, fulltypename from assemblyinfo order by fulltypename asc";
            return GetListResultWithID(sql);
        }

        public Dictionary<string, AvailableRule> QueryAvailableRules()
        {
            string enabledRules = ConfigurationManager.AppSettings["EnabledRules"];
            string tempRuleNameString = string.Empty;
            if (!string.IsNullOrEmpty(enabledRules))
            {
                string[] ruleList = enabledRules.Split('|');
                for (int i = 0; i < ruleList.Length; i++)
                {
                    if (i == 0)
                        tempRuleNameString += "'" + ruleList[i];
                    else if (i == ruleList.Length - 1)
                        tempRuleNameString += "','" + ruleList[i] + "'";
                    else
                        tempRuleNameString += "','" + ruleList[i];
                }
            }

            Dictionary<string, AvailableRule> availableRules = new Dictionary<string, AvailableRule>();
            //string sql1 = "select ruleid, rulename, ruledesc from rules where IsEnabled=1 order by rulename asc";
            string sql1 = "select ruleid, rulename, ruledesc from rules where rulename in(" + tempRuleNameString + ") order by rulename asc";
            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand command = new SqlCommand(sql1, conn);
            command.CommandType = CommandType.Text;
            conn.Open();

            SqlDataReader dr = command.ExecuteReader();
            AvailableRule availableRule;
            while (dr.Read())
            {
                availableRule = new AvailableRule()
                {
                    RuleID = dr[0].ToString(),
                    RuleName = dr[1].ToString(),
                    RuleDescription = dr[2].ToString(),
                };
                availableRules.Add(dr[0].ToString(), availableRule);
            }
            conn.Close();

            return availableRules;
        }

        public Dictionary<string, string> QueryRules()
        {
            string sql1 = "select ruleid, rulename from rules order by rulename asc";
            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand command = new SqlCommand(sql1, conn);
            command.CommandType = CommandType.Text;
            conn.Open();

            Dictionary<string, string> temp = new Dictionary<string, string>();
            SqlDataReader dr = command.ExecuteReader();
            while (dr.Read())
            {
                temp.Add(dr.GetInt32(0).ToString(), dr.GetString(1));
            }
            conn.Close();

            return temp;
        }


        public Dictionary<string, string> QueryAssemblyTypesInfo()
        {
            string sql1 = "select typeid, fulltypename,assemblyname from assemblyinfo order by fulltypename asc";
            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand command = new SqlCommand(sql1, conn);
            command.CommandType = CommandType.Text;
            conn.Open();

            Dictionary<string, string> temp = new Dictionary<string, string>();
            SqlDataReader dr = command.ExecuteReader();
            while (dr.Read())
            {
                temp.Add(dr.GetInt32(0) + "," + dr.GetString(1), dr.GetString(2));
            }
            conn.Close();

            return temp;
        }

        private string GetListResult(string sql)
        {
            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand command = new SqlCommand(sql, conn);
            command.CommandType = CommandType.Text;
            conn.Open();

            List<string> list = new List<string>();
            SqlDataReader dr = command.ExecuteReader();
            while (dr.Read())
            {
                list.Add(dr.GetString(0));
            }
            string result = string.Join(vertical, list);
            conn.Close();
            return result;
        }

        private string GetListResultWithID(string sql)
        {
            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand command = new SqlCommand(sql, conn);
            command.CommandType = CommandType.Text;
            conn.Open();

            List<string> list = new List<string>();
            SqlDataReader dr = command.ExecuteReader();
            while (dr.Read())
            {
                list.Add(dr.GetInt32(0) + "," + dr.GetString(1));
            }
            string result = string.Join(vertical, list);
            conn.Close();
            return result;
        }
    }

    public partial class ViewCapturedReport
    {
        [DataMemberAttribute()]
        public global::System.String DateUploadedStr
        {
            get;
            set;
        }

        [DataMemberAttribute()]
        public global::System.String CreateDateStr
        {
            get;
            set;
        }
    }

    [Serializable()]
    [DataContractAttribute(IsReference = true)]
    public class GroupTitle
    {
        [DataMemberAttribute()]
        public string UIName { get; set; }
        [DataMemberAttribute()]
        public string Count { get; set; }
        [DataMemberAttribute()]
        public string ContentID { get; set; }
    }

    public class AvailableRule
    {
        [DataMemberAttribute()]
        public string RuleID { get; set; }
        [DataMemberAttribute()]
        public string RuleName { get; set; }
        [DataMemberAttribute()]
        public string RuleDescription { get; set; }
    }
}

