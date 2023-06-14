using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace SulpHurManagementSystem.Utility
{
    public class SQLUtility
    {
        private static string connStr = string.Empty;

        public static List<string> osRelatedRuleIDs = null;
        public static List<string> osNotRelatedRuleIDs = null;

        static SQLUtility()
        {
            connStr = ConfigurationManager.ConnectionStrings["ADOConn"].ToString();
        }

        #region latest query
        //count query
        //only one rule - os related rule
        //assembly info is not specified
        public static string sqlCountOSRelatedLatest = @"SELECT COUNT(*) FROM (
 SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [UIContents].[UIName], [UIContents].[DateUploaded], [Results].[ResultType], 
 [Results].[ReviewFlag], [BuildInfo].[Language], [Clients].[OSType] FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 WHERE 1=1 {0}) AS [TOTAL]
 INNER JOIN (
 SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType] FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 WHERE 1=1 {1}
 GROUP BY  [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType]) AS [GroupTable]
 ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
 AND [TOTAL].[UIName]=[GroupTable].[UIName]
 AND [TOTAL].[OSType]=[GroupTable].[OSType]
 AND [TOTAL].[Language]=[GroupTable].[Language]";

        //count query
        //only one rule - os not related rule
        //assembly info is not specified
        public static string sqlCountOSNotRelatedLatest = @"SELECT COUNT(*) FROM (
 SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [UIContents].[UIName], [UIContents].[DateUploaded], [Results].[ResultType], 
 [Results].[ReviewFlag] FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 WHERE 1=1 {0}) AS [TOTAL]
 INNER JOIN (
 SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName] FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 WHERE 1=1 {1}
 GROUP BY [UIContents].[UIName]) AS [GroupTable]
 ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
 AND [TOTAL].[UIName]=[GroupTable].[UIName]";

        //result query
        //only one rule - os related rule
        //assembly info is not specified
        public static string sqlResultsOSRelatedLatest = @"SELECT * FROM (
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
 WHERE 1=1 {0}) AS [TOTAL]
 INNER JOIN ( 
 SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType] FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 WHERE 1=1 {1}
 GROUP BY [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType]) AS [GroupTable]
 ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
 AND [TOTAL].[UIName]=[GroupTable].[UIName]
 AND [TOTAL].[OSType]=[GroupTable].[OSType]
 AND [TOTAL].[Language]=[GroupTable].[Language]) AS newT WHERE rowNum BETWEEN {2} AND {3} ORDER BY rowNum";

        //result query
        //only one rule - os not related rule
        //assembly info is not specified
        public static string sqlResultsOSNotRelatedLatest = @"SELECT * FROM (
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
 WHERE 1=1 {0}) AS [TOTAL]
 INNER JOIN ( 
 SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName]
 FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 WHERE 1=1 {1}
 GROUP BY [UIContents].[UIName]) AS [GroupTable]
 ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
 AND [TOTAL].[UIName]=[GroupTable].[UIName]) AS newT WHERE rowNum BETWEEN {2} AND {3} ORDER BY rowNum";

        //count query
        //only one rule - os related rule
        //assembly info is specified
        public static string sqlCountOSRelatedWithAssemblyLatest = @"SELECT COUNT(DISTINCT [ResultID]) FROM (
 SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [UIContents].[UIName], [AssemblyLink].[TypeID], [Results].[ResultType], 
 [Results].[ReviewFlag], [BuildInfo].[Language], [Clients].[OSType] FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 LEFT OUTER JOIN [AssemblyLink] ON [AssemblyLink].ContentID = [Results].[ContentID]
 LEFT OUTER JOIN [AssemblyInfo] ON [AssemblyLink].[TypeID] = [AssemblyInfo].[TypeID]
 WHERE 1=1 {0}) AS [TOTAL]
 INNER JOIN ( 
 SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [AssemblyLink].[TypeID], [BuildInfo].[Language], [Clients].[OSType]
 FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 LEFT OUTER JOIN [AssemblyLink] ON [AssemblyLink].ContentID = [Results].[ContentID]
 LEFT OUTER JOIN [AssemblyInfo] ON [AssemblyLink].[TypeID] = [AssemblyInfo].[TypeID]
 WHERE 1=1 {1}
 GROUP BY [UIContents].[UIName], [AssemblyLink].[TypeID], [BuildInfo].[Language], [Clients].[OSType]) AS [GroupTable]
 ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
 AND [TOTAL].[UIName]=[GroupTable].[UIName]
 AND [TOTAL].[TypeID]=[GroupTable].[TypeID]
 AND [TOTAL].[OSType]=[GroupTable].[OSType]
 AND [TOTAL].[Language]=[GroupTable].[Language]";

        //count query
        //only one rule - os related rule
        //assembly info is specified
        //by temp table
        public static string sqlCountOSRelatedWithAssemblyLatestByTempTable = @"CREATE TABLE #TempTotal (
	[BuildNo] varchar(50),
	[ResultID] int,
	[UIName] nvarchar(max),
	[TypeID] int,
	[ResultType] varchar(30),
	[ReviewFlag] int,
	[Language] nvarchar(50),
	[OSType] varchar(30)
)

 insert into #TempTotal SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [UIContents].[UIName], [AssemblyLink].[TypeID], [Results].[ResultType], 
 [Results].[ReviewFlag], [BuildInfo].[Language], [Clients].[OSType] FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 LEFT OUTER JOIN [AssemblyLink] ON [AssemblyLink].ContentID = [Results].[ContentID]
 LEFT OUTER JOIN [AssemblyInfo] ON [AssemblyLink].[TypeID] = [AssemblyInfo].[TypeID]
 WHERE 1=1 {0}

 SELECT COUNT(DISTINCT [ResultID]) FROM #TempTotal AS [TOTAL]
 INNER JOIN ( 
 SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [AssemblyLink].[TypeID], [BuildInfo].[Language], [Clients].[OSType]
 FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 LEFT OUTER JOIN [AssemblyLink] ON [AssemblyLink].ContentID = [Results].[ContentID]
 LEFT OUTER JOIN [AssemblyInfo] ON [AssemblyLink].[TypeID] = [AssemblyInfo].[TypeID]
 WHERE 1=1 {1}
 GROUP BY [UIContents].[UIName], [AssemblyLink].[TypeID], [BuildInfo].[Language], [Clients].[OSType]) AS [GroupTable]
 ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
 AND [TOTAL].[UIName]=[GroupTable].[UIName]
 AND [TOTAL].[TypeID]=[GroupTable].[TypeID]
 AND [TOTAL].[OSType]=[GroupTable].[OSType]
 AND [TOTAL].[Language]=[GroupTable].[Language]

 drop table #TempTotal";

        //count query
        //only one rule - os not related rule
        //assembly info is specified
        public static string sqlCountOSNotRelatedWithAssemblyLatest = @"SELECT COUNT(DISTINCT [ResultID]) FROM (
 SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [UIContents].[UIName], [AssemblyLink].[TypeID], [Results].[ResultType], 
 [Results].[ReviewFlag] FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 LEFT OUTER JOIN [AssemblyLink] ON [AssemblyLink].ContentID = [Results].[ContentID]
 LEFT OUTER JOIN [AssemblyInfo] ON [AssemblyLink].[TypeID] = [AssemblyInfo].[TypeID]
 WHERE 1=1 {0}) AS [TOTAL]
 INNER JOIN ( 
 SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [AssemblyLink].[TypeID] FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 LEFT OUTER JOIN [AssemblyLink] ON [AssemblyLink].ContentID = [Results].[ContentID]
 LEFT OUTER JOIN [AssemblyInfo] ON [AssemblyLink].[TypeID] = [AssemblyInfo].[TypeID]
 WHERE 1=1 {1}
 GROUP BY [UIContents].[UIName], [AssemblyLink].[TypeID], [BuildInfo].[Language], [Clients].[OSType]) AS [GroupTable]
 ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
 AND [TOTAL].[UIName]=[GroupTable].[UIName]
 AND [TOTAL].[TypeID]=[GroupTable].[TypeID]";

        //count query
        //only one rule - os not related rule
        //assembly info is specified
        //by temp table
        public static string sqlCountOSNotRelatedWithAssemblyLatestByTempTable = @"CREATE TABLE #TempTotal (
	[BuildNo] varchar(50),
	[ResultID] int,
	[UIName] nvarchar(max),
	[TypeID] int,
	[ResultType] varchar(30),
	[ReviewFlag] int
)

 insert into #TempTotal SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [UIContents].[UIName], [AssemblyLink].[TypeID], [Results].[ResultType], 
 [Results].[ReviewFlag] FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 LEFT OUTER JOIN [AssemblyLink] ON [AssemblyLink].ContentID = [Results].[ContentID]
 LEFT OUTER JOIN [AssemblyInfo] ON [AssemblyLink].[TypeID] = [AssemblyInfo].[TypeID]
 WHERE 1=1 {0}

 SELECT COUNT(DISTINCT [ResultID]) FROM #TempTotal AS [TOTAL]
 INNER JOIN ( 
 SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [AssemblyLink].[TypeID] FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 LEFT OUTER JOIN [AssemblyLink] ON [AssemblyLink].ContentID = [Results].[ContentID]
 LEFT OUTER JOIN [AssemblyInfo] ON [AssemblyLink].[TypeID] = [AssemblyInfo].[TypeID]
 WHERE 1=1 {1}
 GROUP BY [UIContents].[UIName], [AssemblyLink].[TypeID], [BuildInfo].[Language], [Clients].[OSType]) AS [GroupTable]
 ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
 AND [TOTAL].[UIName]=[GroupTable].[UIName]
 AND [TOTAL].[TypeID]=[GroupTable].[TypeID]

 drop table #TempTotal";

        //result query
        //only one rule - os related rule
        //assembly info is specified
        public static string sqlResultsOSRelatedWithAssemblyLatest = @"SELECT * FROM (
 SELECT ROW_NUMBER() OVER (ORDER BY [BuildNo] DESC, [ResultID] DESC) AS rowNum, * FROM (
 SELECT DISTINCT [TOTAL].[BuildNo], [TOTAL].[ResultID], [TOTAL].[UIName], [TOTAL].[ResultType], [TOTAL].[ReviewFlag], [TOTAL].[Language],
 [TOTAL].[RuleName],[TOTAL].[UserName],[TOTAL].[OSType],[TOTAL].[DateUploaded],[TOTAL].[CreateDate]
 FROM (
 SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [UIContents].[UIName], [AssemblyLink].[TypeID], [Results].[ResultType], 
 [Results].[ReviewFlag], [BuildInfo].[Language], [Rules].[RuleName],[Clients].[UserName],[Clients].[OSType],[UIContents].[DateUploaded],
 [Results].[CreateDate] FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 LEFT OUTER JOIN [AssemblyLink] ON [AssemblyLink].ContentID = [Results].[ContentID]
 LEFT OUTER JOIN [AssemblyInfo] ON [AssemblyLink].[TypeID] = [AssemblyInfo].[TypeID]
 WHERE 1=1 {0}) AS [TOTAL]
 INNER JOIN ( 
 SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [AssemblyLink].[TypeID], [BuildInfo].[Language], [Clients].[OSType]
 FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 LEFT OUTER JOIN [AssemblyLink] ON [AssemblyLink].ContentID = [Results].[ContentID]
 LEFT OUTER JOIN [AssemblyInfo] ON [AssemblyLink].[TypeID] = [AssemblyInfo].[TypeID]
 WHERE 1=1 {1}
 GROUP BY [UIContents].[UIName], [AssemblyLink].[TypeID], [BuildInfo].[Language], [Clients].[OSType]) AS [GroupTable]
 ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
 AND [TOTAL].[UIName]=[GroupTable].[UIName]
 AND [TOTAL].[TypeID]=[GroupTable].[TypeID]
 AND [TOTAL].[Language]=[GroupTable].[Language]
 AND [TOTAL].[OSType]=[GroupTable].[OSType]
 ) AS [TOTALWithROWNUMBER]) AS newT WHERE rowNum BETWEEN {2} AND {3} ORDER BY rowNum";

        //result query
        //only one rule - os not related rule
        //assembly info is specified
        public static string sqlResultsOSNotRelatedWithAssemblyLatest = @"SELECT * FROM (
 SELECT ROW_NUMBER() OVER (ORDER BY [BuildNo] DESC, [ResultID] DESC) AS rowNum, * FROM (
 SELECT DISTINCT [TOTAL].[BuildNo], [TOTAL].[ResultID], [TOTAL].[UIName], [TOTAL].[ResultType], [TOTAL].[ReviewFlag], [TOTAL].[Language],
 [TOTAL].[RuleName],[TOTAL].[UserName],[TOTAL].[OSType],[TOTAL].[DateUploaded],[TOTAL].[CreateDate]
 FROM (
 SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [UIContents].[UIName], [AssemblyLink].[TypeID], [Results].[ResultType], 
 [Results].[ReviewFlag], [BuildInfo].[Language], [Rules].[RuleName], [Clients].[UserName], [Clients].[OSType], [UIContents].[DateUploaded],
 [Results].[CreateDate] FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 LEFT OUTER JOIN [AssemblyLink] ON [AssemblyLink].ContentID = [Results].[ContentID]
 LEFT OUTER JOIN [AssemblyInfo] ON [AssemblyLink].[TypeID] = [AssemblyInfo].[TypeID]
 WHERE 1=1 {0}) AS [TOTAL]
 INNER JOIN ( 
 SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [AssemblyLink].[TypeID] FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 LEFT OUTER JOIN [AssemblyLink] ON [AssemblyLink].ContentID = [Results].[ContentID]
 LEFT OUTER JOIN [AssemblyInfo] ON [AssemblyLink].[TypeID] = [AssemblyInfo].[TypeID]
 WHERE 1=1 {1}
 GROUP BY [UIContents].[UIName], [AssemblyLink].[TypeID]) AS [GroupTable]
 ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
 AND [TOTAL].[UIName]=[GroupTable].[UIName]
 AND [TOTAL].[TypeID]=[GroupTable].[TypeID]
 ) AS [TOTALWithROWNUMBER]) AS newT WHERE rowNum BETWEEN {2} AND {3} ORDER BY rowNum";
        #endregion

        #region common query
        //count query
        //assembly info is not specified
        public static string sqlCountCommon = @"SELECT COUNT(*) FROM [dbo].[Results] 
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID]
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID]
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID]
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 WHERE 1=1 {0}";

        //count query
        //assembly info is specified
        public static string sqlCountCommonWithAssembly = @"SELECT COUNT(DISTINCT [ResultID]) FROM [dbo].[Results] 
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID]
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID]
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID]
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 LEFT OUTER JOIN [AssemblyLink] ON [AssemblyLink].[ContentID]=[UIContents].[ContentID]
 LEFT OUTER JOIN [AssemblyInfo] ON [AssemblyInfo].[TypeID]=[AssemblyLink].[TypeID]
 WHERE 1=1 {0}";

        //result query
        //assembly info is not specified
        public static string sqlResultsCommon = @"SELECT * FROM (
 SELECT ROW_NUMBER() OVER (ORDER BY [BuildInfo].[BuildNo] DESC, [Results].[ResultID] DESC) AS rowNum, [Results].[ResultID], [BuildInfo].[BuildNo], 
 [BuildInfo].[Language], [Rules].[RuleName], [Results].[ResultType], [UIContents].[UIName], [Clients].[UserName], [BuildInfo].[BuildID], 
 [Clients].[OSType], [UIContents].[DateUploaded], [Results].[CreateDate], [Results].[ReviewFlag] FROM [dbo].[Results] 
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID]
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID]
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID]
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID] 
 WHERE 1=1 {0}) AS newT WHERE rowNum BETWEEN {1} AND {2} ORDER BY rowNum";

        //result query
        //assembly info is specified
        public static string sqlResultsCommonWithAssembly = @"SELECT * FROM (
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
 WHERE 1=1 {0}) AS newT) AS t WHERE rowNum BETWEEN {1} AND {2} ORDER BY rowNum";


        public static string sqlResultsByResultIDs = @"SELECT [Results].[ResultID], [BuildInfo].[BuildNo], [BuildInfo].[Language], 
 [Rules].[RuleName], [Results].[ResultType], [UIContents].[UIName], [Clients].[UserName], [BuildInfo].[BuildID], [Clients].[OSType], 
 [UIContents].[DateUploaded], [Results].[CreateDate], [Results].[ReviewFlag] FROM [dbo].[Results] 
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID]
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID]
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID]
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID] 
 WHERE [Results].[ResultID] IN ({0}) ORDER BY [BuildInfo].[BuildNo] DESC, [Results].[ResultID] DESC";
        #endregion

        #region Filter clause
        public static string GenerateAssemblyNameFilterClause(string assemblyName)
        {
            string assemblyNameFilterClause = string.Empty;
            if (assemblyName != "All")
                assemblyNameFilterClause = @" AND [AssemblyInfo].[AssemblyName] IN (" + assemblyName + ")";

            return assemblyNameFilterClause;
        }
        public static string GenerateFullTypeNameFilterClause(string typeNameString)
        {
            string typeIDFilterClause = string.Empty;
            if (typeNameString != "All")
            {
                string[] types = typeNameString.Split(',');
                typeIDFilterClause = " AND ([AssemblyInfo].[FullTypeName]=" + types[0];
                for (int i = 1; i < types.Length; i++)
                    typeIDFilterClause += " OR [AssemblyInfo].[FullTypeName]=" + types[i];
                typeIDFilterClause += ")";
            }

            return typeIDFilterClause;
        }
        public static string GenerateFullTypeNameInFilterClause(string typeNameString)
        {
            string typeIDFilterClause = string.Empty;
            if (typeNameString != "All")
                typeIDFilterClause = " AND [AssemblyInfo].[FullTypeName] IN (" + typeNameString + ")";

            return typeIDFilterClause;
        }
        public static string GenerateRuleIDFilterClause(string ruleid)
        {
            string ruleidFilterClause = string.Empty;
            if (ruleid != "All")
                ruleidFilterClause = " AND [Results].[RuleID]=" + ruleid;

            return ruleidFilterClause;
        }
        public static string GenerateRuleIDFilterINClause(string ruleid)
        {
            string ruleidFilterClause = string.Empty;
            if (ruleid != "All")
                ruleidFilterClause = " AND [Results].[RuleID] IN (" + ruleid + ")";

            return ruleidFilterClause;
        }
        public static string GenerateResultTypesFilterClause(string resultTypes)
        {
            string resultTypesFilterClause = string.Empty;
            if (resultTypes != "All")
                resultTypesFilterClause = " AND [Results].[ResultType] IN (" + resultTypes + ")";

            return resultTypesFilterClause;
        }
        public static string GenerateBuildNOFilterClause(string buildNO)
        {
            string buildNOFilterClause = string.Empty;
            if (buildNO.Contains("AND"))
                buildNOFilterClause = " AND [BuildInfo].[BuildNo] BETWEEN " + buildNO;
            else
                if (buildNO != "All")
                    buildNOFilterClause = " AND [BuildInfo].[BuildNo] IN (" + buildNO + ")";

            return buildNOFilterClause;
        }
        public static string GenerateBuildLanFilterClause(string buildLan)
        {
            string buildLanFilterClause = string.Empty;
            if (buildLan != "All")
                buildLanFilterClause = " AND [BuildInfo].[Language] IN (" + buildLan + ")";

            return buildLanFilterClause;
        }
        public static string GenerateOSTypeFilterClause(string osType)
        {
            string osTypeFilterClause = string.Empty;
            if (osType != "All")
                osTypeFilterClause = " AND [Clients].[OSType] IN (" + osType + ")";

            return osTypeFilterClause;
        }
        public static string GenerateOSLanguageFilterClause(string osLanguage)
        {
            string osLanguageFilterClause = string.Empty;
            if (osLanguage != "All")
                osLanguageFilterClause = " AND [Clients].[OSLanguage] IN (" + osLanguage + ")";

            return osLanguageFilterClause;
        }
        public static string GenerateUserNameFilterClause(string username)
        {
            string usernameFilterClause = string.Empty;
            if (!string.IsNullOrEmpty(username))
                usernameFilterClause = " AND [Clients].[UserName] LIKE N'%" + username + "%' ";

            return usernameFilterClause;
        }
        public static string GeneratePageTitleFilterClause(string pagetitle)
        {
            string pagetitleFilterClause = string.Empty;
            if (!string.IsNullOrEmpty(pagetitle))
            {
                pagetitle = pagetitle.Replace("'", "''");
                pagetitleFilterClause = " AND [UIContents].[UIName] LIKE N'%" + pagetitle + "%' ";
            }

            return pagetitleFilterClause;
        }
        public static string GenerateReviewCommentsFilterClause(string reviewComments)
        {
            string reviewCommentsFilterClause = string.Empty;
            if (!string.IsNullOrEmpty(reviewComments))
            {
                reviewComments = reviewComments.Replace("'", "''");
                reviewCommentsFilterClause = " AND [Results].[ReviewLog] LIKE N'%" + reviewComments + "%' ";
            }

            return reviewCommentsFilterClause;
        }
        public static string GenerateReviewedTypeFilterClause(string reviewedType)
        {
            string reviewedTypeFilterClause = string.Empty;
            if (reviewedType != "-1")
                reviewedTypeFilterClause = " AND [Results].[ReviewFlag]=" + reviewedType;

            return reviewedTypeFilterClause;
        }
        public static string GenerateSearchDatetimeFilterClause(string searchDatetime)
        {
            string searchDatetimeFilterClause = string.Empty;
            if (!string.IsNullOrEmpty(searchDatetime))
                searchDatetimeFilterClause = " AND [CreateDate]<'" + searchDatetime + "'";
            return searchDatetimeFilterClause;
        }
        public static string GenerateSearchDatetimeFilterClause(string searchDatetime, int latestDays)
        {
            string searchDatetimeFilterClause = string.Empty;
            if (!string.IsNullOrEmpty(searchDatetime))
            {
                if (latestDays != -1)
                {
                    DateTime dt = Convert.ToDateTime(searchDatetime);
                    dt = dt.AddDays(-latestDays);
                    searchDatetimeFilterClause = " AND [CreateDate]>'" + dt + "'";
                }
                searchDatetimeFilterClause += " AND [CreateDate]<'" + searchDatetime + "'";
            }

            return searchDatetimeFilterClause;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static string GenerateCountSQLByResultID(string resultId)
        {
            string sql = string.Format("SELECT COUNT(*) FROM [dbo].[Results] Where [Results].ResultID in ({0})",
                resultId);
            return sql;
        }
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static string GenerateListSQLByResultID(string resultId)
        {
            string sql = string.Format("SELECT{0} FROM [dbo].[Results]{1} WHERE 1=1 AND [Results].ResultID IN ({2})",
                //sqlQueryCoreColumns,
                //joinTablesNoAssembly,
                resultId);
            return sql;
        }

        public static int GetCountOfTotalRecords(string buildNO, string buildLan, string osType, string osLanguage, string ruleid,
            string resultTypes, string assemblyName, string typeID, string username, string pagetitle, string reviewComments,
            string ReviewedType, string resultid, string searchDateTime, bool getLatest, int latestDays)
        {
            string whereClauseCommon = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}",
                SQLUtility.GenerateBuildNOFilterClause(buildNO),
                SQLUtility.GenerateBuildLanFilterClause(buildLan),
                SQLUtility.GenerateOSTypeFilterClause(osType),
                SQLUtility.GenerateOSLanguageFilterClause(osLanguage),
                SQLUtility.GenerateRuleIDFilterINClause(ruleid),
                SQLUtility.GenerateUserNameFilterClause(username),
                SQLUtility.GeneratePageTitleFilterClause(pagetitle),
                SQLUtility.GenerateReviewCommentsFilterClause(reviewComments),
                SQLUtility.GenerateSearchDatetimeFilterClause(searchDateTime, latestDays));
            string whereClauseAssemblyFilter = string.Format("{0}{1}",
                SQLUtility.GenerateAssemblyNameFilterClause(assemblyName),
                SQLUtility.GenerateFullTypeNameInFilterClause(typeID));
            string whereClauseResultTypeAndReviewType = string.Format("{0}{1}",
                SQLUtility.GenerateResultTypesFilterClause(resultTypes),
                SQLUtility.GenerateReviewedTypeFilterClause(ReviewedType));

            string sql = string.Empty;
            if (!string.IsNullOrEmpty(resultid))
            {
                sql = GenerateCountSQLByResultID(resultid);
            }
            else if (getLatest)
            {
                //sql = GenerateLatestCountSQL(buildNO, buildLan, osType, osLanguage, ruleid, resultTypes, assemblyName, typeID,
                //    username, pagetitle, reviewComments, ReviewedType, buildtype, searchDateTime);


                if (osRelatedRuleIDs.Contains(ruleid))
                {
                    if (assemblyName == "All" && typeID == "All")
                    {
                        sql = string.Format(SQLUtility.sqlCountOSRelatedLatest,
                            whereClauseCommon + whereClauseResultTypeAndReviewType,
                            whereClauseCommon);
                    }
                    else
                    {
                        sql = string.Format(SQLUtility.sqlCountOSRelatedWithAssemblyLatestByTempTable,
                            whereClauseCommon + whereClauseAssemblyFilter + whereClauseResultTypeAndReviewType,
                            whereClauseCommon + whereClauseAssemblyFilter);
                    }
                }
                else if (osNotRelatedRuleIDs.Contains(ruleid))
                {
                    if (assemblyName == "All" && typeID == "All")
                    {
                        sql = string.Format(SQLUtility.sqlCountOSNotRelatedLatest,
                            whereClauseCommon + whereClauseResultTypeAndReviewType,
                            whereClauseCommon);
                    }
                    else
                    {
                        sql = string.Format(SQLUtility.sqlCountOSNotRelatedWithAssemblyLatestByTempTable,
                            whereClauseCommon + whereClauseAssemblyFilter + whereClauseResultTypeAndReviewType,
                            whereClauseCommon + whereClauseAssemblyFilter);
                    }
                }
            }
            else
            {
                //sql = GenerateCountSQL(buildNO, buildLan, osType, osLanguage, ruleid, resultTypes, assemblyName, typeID,
                //    username, pagetitle, reviewComments, ReviewedType, searchDateTime);
                if (assemblyName == "All" && typeID == "All")
                {
                    sql = string.Format(SQLUtility.sqlCountCommon,
                        whereClauseCommon + whereClauseResultTypeAndReviewType,
                        whereClauseCommon);
                }
                else
                {
                    sql = string.Format(SQLUtility.sqlCountCommonWithAssembly,
                        whereClauseCommon + whereClauseAssemblyFilter + whereClauseResultTypeAndReviewType,
                        whereClauseCommon + whereClauseAssemblyFilter);
                }
            }

            int count = (int)ExecuteScalar(sql);
            return count;
        }
        public static List<ViewCapturedReport> GetDetailResultsOfSpecifiedPage(string buildNO, string buildLan, string osType, string osLanguage, 
            string ruleid, string resultTypes, string assemblyName, string typeID, string pageIndex, string username, string pagetitle, 
            string reviewComments, string ReviewedType, string resultid, string sortBy, string searchDateTime, string currentPageResults,
            string diffentReviewStatusResultsCount, bool getLatest, int latestDays)
        {
            string whereClauseCommon = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}",
                SQLUtility.GenerateBuildNOFilterClause(buildNO),
                SQLUtility.GenerateBuildLanFilterClause(buildLan),
                SQLUtility.GenerateOSTypeFilterClause(osType),
                SQLUtility.GenerateOSLanguageFilterClause(osLanguage),
                SQLUtility.GenerateRuleIDFilterINClause(ruleid),
                SQLUtility.GenerateUserNameFilterClause(username),
                SQLUtility.GeneratePageTitleFilterClause(pagetitle),
                SQLUtility.GenerateReviewCommentsFilterClause(reviewComments),
                SQLUtility.GenerateSearchDatetimeFilterClause(searchDateTime, latestDays));
            string whereClauseAssemblyFilter = string.Format("{0}{1}",
                SQLUtility.GenerateAssemblyNameFilterClause(assemblyName),
                SQLUtility.GenerateFullTypeNameInFilterClause(typeID));
            string whereClauseResultTypeAndReviewType = string.Format("{0}{1}",
                SQLUtility.GenerateResultTypesFilterClause(resultTypes),
                SQLUtility.GenerateReviewedTypeFilterClause(ReviewedType));

            List<ViewCapturedReport> resultReportList = new List<ViewCapturedReport>();
            int page = Int32.Parse(pageIndex);
            int diffentReviewStatusResults = int.Parse(diffentReviewStatusResultsCount);
            int actualStartIndex = (page - 1) * 20 + 1;
            int actualEndIndex = page * 20;
            if (ReviewedType == "0" || ReviewedType == "1")
            {
                actualStartIndex -= diffentReviewStatusResults;
                actualEndIndex -= diffentReviewStatusResults;
            }

            string sql = string.Empty;
            if (!string.IsNullOrEmpty(resultid))
            {
                sql = GenerateListSQLByResultID(resultid);
            }
            else if (currentPageResults != "undefined")
            {
                //sql = GenerateListSQLByResultID(currentPageResults, sortBy, assemblyName);
                sql = string.Format(SQLUtility.sqlResultsByResultIDs, currentPageResults);
            }
            else if (getLatest)
            {
                //sql = GenerateLatestListSQL(buildNO, buildLan, osType, osLanguage, ruleid, resultTypes, assemblyName, typeID, page,
                //    username, pagetitle, reviewComments, ReviewedType, searchDateTime, sortBy, 20, diffentReviewStatusResults);
                if (osRelatedRuleIDs.Contains(ruleid))
                {
                    if (assemblyName == "All" && typeID == "All")
                    {
                        sql = string.Format(SQLUtility.sqlResultsOSRelatedLatest,
                            whereClauseCommon + whereClauseResultTypeAndReviewType,
                            whereClauseCommon,
                            actualStartIndex,
                            actualEndIndex);
                    }
                    else
                    {
                        sql = string.Format(SQLUtility.sqlResultsOSRelatedWithAssemblyLatest,
                            whereClauseCommon + whereClauseAssemblyFilter + whereClauseResultTypeAndReviewType,
                            whereClauseCommon + whereClauseAssemblyFilter,
                            actualStartIndex,
                            actualEndIndex);
                    }
                }
                else if (osNotRelatedRuleIDs.Contains(ruleid))
                {
                    if (assemblyName == "All" && typeID == "All")
                    {
                        sql = string.Format(SQLUtility.sqlResultsOSNotRelatedLatest,
                            whereClauseCommon + whereClauseResultTypeAndReviewType,
                            whereClauseCommon,
                            actualStartIndex,
                            actualEndIndex);
                    }
                    else
                    {
                        sql = string.Format(SQLUtility.sqlResultsOSNotRelatedWithAssemblyLatest,
                            whereClauseCommon + whereClauseAssemblyFilter + whereClauseResultTypeAndReviewType,
                            whereClauseCommon + whereClauseAssemblyFilter,
                            actualStartIndex,
                            actualEndIndex);
                    }
                }
            }
            else
            {
                //sql = GenerateListSQL(buildNO, buildLan, osType, osLanguage, ruleid, resultTypes, assemblyName, typeID, page,
                //    username, pagetitle, reviewComments, ReviewedType, searchDateTime, sortBy, 20, diffentReviewStatusResults);
                if (assemblyName == "All" && typeID == "All")
                {
                    sql = string.Format(SQLUtility.sqlResultsCommon,
                        whereClauseCommon + whereClauseResultTypeAndReviewType,
                        actualStartIndex,
                        actualEndIndex);
                }
                else
                {
                    sql = string.Format(SQLUtility.sqlResultsCommonWithAssembly,
                        whereClauseCommon + whereClauseAssemblyFilter + whereClauseResultTypeAndReviewType,
                        actualStartIndex,
                        actualEndIndex);
                }
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
                //resultReport.ReviewLog = dr["ReviewLog"].ToString();
                resultReportList.Add(resultReport);
            }
            conn.Close();

            return resultReportList;
        }

        #region common function
        private static object ExecuteScalar(string sql)
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
        #endregion
    }
}