using SulpHurManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

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

        #region latest query string
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
        public static string sqlCountOSRelatedWithAssemblyLatestByTempTable = @"
 if OBJECT_ID('TEMPDB..#TempTotal') IS NOT NULL
 drop table #TempTotal
 if OBJECT_ID('TEMPDB..#TempGroupTable') IS NOT NULL
 drop table #TempGroupTable

 SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [UIContents].[UIName], [AssemblyLink].[TypeID], [Results].[ResultType], 
 [Results].[ReviewFlag], [BuildInfo].[Language], [Clients].[OSType] into #TempTotal FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 LEFT OUTER JOIN [AssemblyLink] ON [AssemblyLink].ContentID = [Results].[ContentID]
 LEFT OUTER JOIN [AssemblyInfo] ON [AssemblyLink].[TypeID] = [AssemblyInfo].[TypeID]
 WHERE 1=1 {0}

 SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [AssemblyLink].[TypeID], [BuildInfo].[Language], [Clients].[OSType] into #TempGroupTable
 FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 LEFT OUTER JOIN [AssemblyLink] ON [AssemblyLink].ContentID = [Results].[ContentID]
 LEFT OUTER JOIN [AssemblyInfo] ON [AssemblyLink].[TypeID] = [AssemblyInfo].[TypeID]
 WHERE 1=1 {1}
 GROUP BY [UIContents].[UIName], [AssemblyLink].[TypeID], [BuildInfo].[Language], [Clients].[OSType]

 SELECT COUNT(DISTINCT [ResultID]) FROM #TempTotal AS [TOTAL]
 INNER JOIN #TempGroupTable AS [GroupTable]
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
        public static string sqlCountOSNotRelatedWithAssemblyLatestByTempTable1 = @"CREATE TABLE #TempTotal (
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
        //count query
        //only one rule - os not related rule
        //assembly info is specified
        //by temp table
        public static string sqlCountOSNotRelatedWithAssemblyLatestByTempTable = @"
 if OBJECT_ID('TEMPDB..#TempTotal') IS NOT NULL
 drop table #TempTotal
 if OBJECT_ID('TEMPDB..#TempGroupTable') IS NOT NULL
 drop table #TempGroupTable

 SELECT DISTINCT [BuildInfo].[BuildNo], [BuildInfo].[Language], [Results].[ResultID], [UIContents].[UIName], [AssemblyLink].[TypeID], [Results].[ResultType], 
 [Results].[ReviewFlag] into #TempTotal FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 LEFT OUTER JOIN [AssemblyLink] ON [AssemblyLink].ContentID = [Results].[ContentID]
 LEFT OUTER JOIN [AssemblyInfo] ON [AssemblyLink].[TypeID] = [AssemblyInfo].[TypeID]
 WHERE 1=1 {0}

 SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [AssemblyLink].[TypeID], [BuildInfo].[Language] into #TempGroupTable
 FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 LEFT OUTER JOIN [AssemblyLink] ON [AssemblyLink].ContentID = [Results].[ContentID]
 LEFT OUTER JOIN [AssemblyInfo] ON [AssemblyLink].[TypeID] = [AssemblyInfo].[TypeID]
 WHERE 1=1 {1}
 GROUP BY [UIContents].[UIName], [AssemblyLink].[TypeID], [BuildInfo].[Language]

 SELECT COUNT(DISTINCT [ResultID]) FROM #TempTotal AS [TOTAL]
 INNER JOIN #TempGroupTable AS [GroupTable]
 ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
 AND [TOTAL].[UIName]=[GroupTable].[UIName]
 AND [TOTAL].[TypeID]=[GroupTable].[TypeID]
 AND [TOTAL].[Language]=[GroupTable].[Language]

 drop table #TempTotal
 drop table #TempGroupTable";

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
        //only one rule - os related rule
        //assembly info is specified
        public static string sqlResultsOSRelatedWithAssemblyLatestByTempTable = @"
 if OBJECT_ID('TEMPDB..#TempTotal') IS NOT NULL
 drop table #TempTotal
 if OBJECT_ID('TEMPDB..#TempGroupTable') IS NOT NULL
 drop table #TempGroupTable

 SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [UIContents].[UIName], [AssemblyLink].[TypeID], [Results].[ResultType], 
 [Results].[ReviewFlag], [BuildInfo].[Language], [Rules].[RuleName],[Clients].[UserName],[Clients].[OSType],[UIContents].[DateUploaded],
 [Results].[CreateDate] INTO #TempTotal FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 LEFT OUTER JOIN [AssemblyLink] ON [AssemblyLink].ContentID = [Results].[ContentID]
 LEFT OUTER JOIN [AssemblyInfo] ON [AssemblyLink].[TypeID] = [AssemblyInfo].[TypeID]
 WHERE 1=1 {0}

 SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [AssemblyLink].[TypeID], [BuildInfo].[Language], [Clients].[OSType] INTO #TempGroupTable
 FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 LEFT OUTER JOIN [AssemblyLink] ON [AssemblyLink].ContentID = [Results].[ContentID]
 LEFT OUTER JOIN [AssemblyInfo] ON [AssemblyLink].[TypeID] = [AssemblyInfo].[TypeID]
 WHERE 1=1 {1}
 GROUP BY [UIContents].[UIName], [AssemblyLink].[TypeID], [BuildInfo].[Language], [Clients].[OSType]

SELECT * FROM (
 SELECT ROW_NUMBER() OVER (ORDER BY [BuildNo] DESC, [ResultID] DESC) AS rowNum, * FROM (
 SELECT DISTINCT #tempTOTAL.[BuildNo], #tempTOTAL.[ResultID], #tempTOTAL.[UIName], #tempTOTAL.[ResultType], #tempTOTAL.[ReviewFlag], #tempTOTAL.[Language],
 #tempTOTAL.[RuleName],#tempTOTAL.[UserName],#tempTOTAL.[OSType],#tempTOTAL.[DateUploaded],#tempTOTAL.[CreateDate]
 FROM #tempTOTAL
 INNER JOIN #tempGroupTable
 ON #tempTOTAL.[BuildNo]=#tempGroupTable.[BuildNo] 
 AND #tempTOTAL.[UIName]=#tempGroupTable.[UIName]
 AND #tempTOTAL.[TypeID]=#tempGroupTable.[TypeID]
 AND #tempTOTAL.[Language]=#tempGroupTable.[Language]
 AND #tempTOTAL.[OSType]=#tempGroupTable.[OSType]
 ) AS [TOTALWithROWNUMBER]) AS newT WHERE rowNum BETWEEN {2} AND {3} ORDER BY rowNum

 drop table #TempTotal
 drop table #TempGroupTable";

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
        //result query
        //only one rule - os not related rule
        //assembly info is specified
        public static string sqlResultsOSNotRelatedWithAssemblyLatestByTempTable = @"
 if OBJECT_ID('TEMPDB..#TempTotal') IS NOT NULL
 drop table #TempTotal
 if OBJECT_ID('TEMPDB..#TempGroupTable') IS NOT NULL
 drop table #TempGroupTable

 SELECT DISTINCT [BuildInfo].[BuildNo], [BuildInfo].[Language], [Results].[ResultID], [UIContents].[UIName], [AssemblyLink].[TypeID], [Results].[ResultType], 
 [Results].[ReviewFlag], [BuildInfo].[Language], [Rules].[RuleName],[Clients].[UserName],[Clients].[OSType],[UIContents].[DateUploaded],
 [Results].[CreateDate] INTO #TempTotal FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 LEFT OUTER JOIN [AssemblyLink] ON [AssemblyLink].ContentID = [Results].[ContentID]
 LEFT OUTER JOIN [AssemblyInfo] ON [AssemblyLink].[TypeID] = [AssemblyInfo].[TypeID]
 WHERE 1=1 {0}

 SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [AssemblyLink].[TypeID], [BuildInfo].[Language] INTO #TempGroupTable
 FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 LEFT OUTER JOIN [AssemblyLink] ON [AssemblyLink].ContentID = [Results].[ContentID]
 LEFT OUTER JOIN [AssemblyInfo] ON [AssemblyLink].[TypeID] = [AssemblyInfo].[TypeID]
 WHERE 1=1 {1}
 GROUP BY [UIContents].[UIName], [AssemblyLink].[TypeID], [BuildInfo].[Language]

 SELECT * FROM (
 SELECT ROW_NUMBER() OVER (ORDER BY [BuildNo] DESC, [ResultID] DESC) AS rowNum, * FROM (
 SELECT DISTINCT #tempTOTAL.[BuildNo], #tempTOTAL.[ResultID], #tempTOTAL.[UIName], #tempTOTAL.[ResultType], #tempTOTAL.[ReviewFlag], #tempTOTAL.[Language],
 #tempTOTAL.[RuleName],#tempTOTAL.[UserName],#tempTOTAL.[OSType],#tempTOTAL.[DateUploaded],#tempTOTAL.[CreateDate]
 FROM #tempTOTAL
 INNER JOIN #tempGroupTable
 ON #tempTOTAL.[BuildNo]=#tempGroupTable.[BuildNo] 
 AND #tempTOTAL.[UIName]=#tempGroupTable.[UIName]
 AND #tempTOTAL.[TypeID]=#tempGroupTable.[TypeID]
 AND #tempTOTAL.[Language]=#tempGroupTable.[Language]
 ) AS [TOTALWithROWNUMBER]) AS newT WHERE rowNum BETWEEN {2} AND {3} ORDER BY rowNum

 drop table #TempTotal
 drop table #TempGroupTable";
        #endregion

        #region common query string
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
        public static string GenerateTypeIDInFilterClause(int typeID)
        {
            string typeIDFilterClause = string.Empty;
            if (typeID != -1)
                typeIDFilterClause = " AND [AssemblyInfo].[TypeID] IN (" + typeID + ")";

            return typeIDFilterClause;
        }
        public static string GenerateTypeIDInFilterClause(string typeID)
        {
            string typeIDFilterClause = string.Empty;
            if (typeID != "-1")
                typeIDFilterClause = " AND [AssemblyInfo].[FullTypeName] IN (" + typeID + ")";

            return typeIDFilterClause;
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
                typeIDFilterClause = " AND [AssemblyInfo].[FullTypeName] IN ('" + typeNameString + "')";

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
        public static string GenerateBuildNOFilterClause(string operatorString, string buildNO)
        {
            string buildNOFilterClause = string.Empty;
            buildNOFilterClause = string.Format(" AND [BuildInfo].[BuildNo]{0}'{1}'", operatorString, buildNO);

            return buildNOFilterClause;
        }
        public static string GenerateBuildLanFilterClause(string buildLan)
        {
            string buildLanFilterClause = string.Empty;
            if (!string.IsNullOrEmpty(buildLan) && buildLan != "All")
            {
                //string[] lans = buildLan.Split(',');
                //string lanStr = lans[0];
                //for (int i = 1; i < lans.Length; i++)
                //    lanStr += string.Format("','{0}", lans[i]);
                //buildLanFilterClause = " AND [BuildInfo].[Language] IN (" + lanStr + ")";
                buildLanFilterClause = " AND [BuildInfo].[Language] IN (" + buildLan + ")";
            }

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

        #region complex query method

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
            string sql = string.Format(@"select * from [Results]
INNER JOIN[UIContents] on[UIContents].[ContentID] =[Results].[ContentID]
INNER JOIN[BuildInfo] on[BuildInfo].[BuildID] =[UIContents].[BuildID]
INNER JOIN[Rules] on[Rules].[RuleID] =[Results].[RuleID]
INNER JOIN[Clients] on[Clients].[ClientID] =[UIContents].[ClientID]
WHERE[Results].[ResultID] IN({0})",
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
            int typeIDinInt = -1;
            string whereClauseAssemblyFilter = string.Format("{0}{1}",
                SQLUtility.GenerateAssemblyNameFilterClause(assemblyName),
                int.TryParse(typeID, out typeIDinInt) ? SQLUtility.GenerateTypeIDInFilterClause(typeIDinInt) : SQLUtility.GenerateTypeIDInFilterClause(typeID));
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
                    if (assemblyName == "All" && typeID == "-1")
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
                    if (assemblyName == "All" && typeID == "-1")
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
                if (assemblyName == "All" && typeID == "-1")
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
        public static List<QueryResult> GetDetailResultsOfSpecifiedPage(string buildNO, string buildLan, string osType, string osLanguage,
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
            int typeIDinInt = -1;
            string whereClauseAssemblyFilter = string.Format("{0}{1}",
                SQLUtility.GenerateAssemblyNameFilterClause(assemblyName),
                int.TryParse(typeID, out typeIDinInt) ? SQLUtility.GenerateTypeIDInFilterClause(typeIDinInt) : SQLUtility.GenerateTypeIDInFilterClause(typeID));
            string whereClauseResultTypeAndReviewType = string.Format("{0}{1}",
                SQLUtility.GenerateResultTypesFilterClause(resultTypes),
                SQLUtility.GenerateReviewedTypeFilterClause(ReviewedType));

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
                    if (assemblyName == "All" && typeID == "-1")
                    {
                        sql = string.Format(SQLUtility.sqlResultsOSRelatedLatest,
                            whereClauseCommon + whereClauseResultTypeAndReviewType,
                            whereClauseCommon,
                            actualStartIndex,
                            actualEndIndex);
                    }
                    else
                    {
                        sql = string.Format(SQLUtility.sqlResultsOSRelatedWithAssemblyLatestByTempTable,
                            whereClauseCommon + whereClauseAssemblyFilter + whereClauseResultTypeAndReviewType,
                            whereClauseCommon + whereClauseAssemblyFilter,
                            actualStartIndex,
                            actualEndIndex);
                    }
                }
                else if (osNotRelatedRuleIDs.Contains(ruleid))
                {
                    if (assemblyName == "All" && typeID == "-1")
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
        #endregion

        #region query from table - Results
        public static string GetResultType(int resultID)
        {
            SulpHurEntities entities = new SulpHurEntities();
            var list = from r in entities.Results
                       where r.ResultID == resultID
                       select r.ResultType;
            string resultType = list.FirstOrDefault();

            return resultType;
        }
        public static string GetResultLog(int resultID)
        {
            SulpHurEntities entities = new SulpHurEntities();
            var list = from r in entities.Results
                       where r.ResultID == resultID
                       select r.ResultLog;
            string resultLog = list.FirstOrDefault();

            return resultLog;
        }
        public static string GetReviewLog(int resultID)
        {
            string sql = @"select ReviewLog from Results where ResultID={0}";
            sql = string.Format(sql, resultID);

            string reviewLogString = string.Empty;
            object reviewLog = ExecuteScalar(sql);
            if (reviewLog != null)
                reviewLogString = reviewLog.ToString();
            return reviewLogString;
        }
        public static ResultDetailInfo GetResultInfo(int resultID)
        {
            try
            {
                SulpHurEntities  entities = new SulpHurEntities();
                //var list = from r in entities.Results
                //           where r.ResultID == resultID
                //           select new ResultDetailInfo() { ResultType = r.ResultType, ResultLog = r.ResultLog, ReviewLog = r.ReviewLog };
                var item = entities.Results.Where(x => x.ResultID == resultID).FirstOrDefault();
                return new ResultDetailInfo() { ResultType = item.ResultType, ResultLog = item.ResultLog, ReviewLog = item.ReviewLog };
            }
            catch (MappingException)
            {
                return GetResultInfo(resultID);
            }
        }
        public static int GetContentID(int resultID)
        {
            string sql = @"select ContentID from Results where ResultID={0}";
            sql = string.Format(sql, resultID);

            int contentID = Convert.ToInt32(ExecuteScalar(sql));

            return contentID;
        }
        public static byte[] GetResultScreenshot(int resultID)
        {
            try
            {
                SulpHurEntities entities = new SulpHurEntities();
                var item = from r in entities.Results
                           where r.ResultID == resultID
                           select r.ResultImage;
                byte[] resultImage = item.FirstOrDefault();

                return resultImage;
            }
            catch (MappingException)
            {
                return GetResultScreenshot(resultID);
            }
        }
        #endregion

        #region query from multiple tables
        public static Rule GetRule(int resultID)
        {
            SulpHurEntities entities = new SulpHurEntities();
            var list = from ru in entities.Rules
                       join re in entities.Results on ru.RuleID equals re.RuleID
                       where re.ResultID == resultID
                       select new { ru.RuleID, ru.RuleName, ru.RuleDesc, ru.IsEnabled };
            var item = list.FirstOrDefault();
            Rule rule = new Rule() { RuleID = item.RuleID, RuleName = item.RuleName, RuleDesc = item.RuleDesc, IsEnabled = item.IsEnabled };

            return rule;
        }
        public static UIInfo GetUIInfo(int resultID)
        {
            SulpHurEntities entities = new SulpHurEntities();

            //get LaunchedFrom and WindowHierarchy
            var list = from r in entities.Results
                       join u in entities.UIContents on r.ContentID equals u.ContentID
                       where r.ResultID == resultID
                       select new UIInfo() { UIName = u.UIName, LaunchedFrom = u.LaunchedFrom, WindowHierarchy = u.WindowHierarchy };
            UIInfo uiInfo = list.FirstOrDefault();

            return uiInfo;
        }
        public static Byte[] GetUIScreenshot(int resultID)
        {
            SulpHurEntities entities = new SulpHurEntities();
            var list = from r in entities.Results
                       join u in entities.UIContents on r.ContentID equals u.ContentID
                       where r.ResultID == resultID
                       select u.UIScreenShot;
            Byte[] uiScreenshot = list.FirstOrDefault();

            return uiScreenshot;
        }
        public static BuildInfo GetBuildInfo(int resultID)
        {
            SulpHurEntities entities = new SulpHurEntities();
            var list = from b in entities.BuildInfoes
                       join u in entities.UIContents on b.BuildID equals u.BuildID
                       join r in entities.Results on u.ContentID equals r.ContentID
                       where r.ResultID == resultID
                       select new { b.BuildNo, b.Language };
            var item = list.FirstOrDefault();
            BuildInfo buildInfo = new BuildInfo() { BuildNo = item.BuildNo, Language = item.Language };

            return buildInfo;
        }
        public static List<Models.AssemblyInfo> GetAssemblyInfoes(int resultID)
        {
            string sql = @"select AssemblyInfo.* from AssemblyInfo
inner join AssemblyLink on AssemblyInfo.TypeID=AssemblyLink.TypeID
inner join Results on AssemblyLink.ContentID=Results.ContentID
where ResultID={0}";
            sql = string.Format(sql, resultID);

            DataTable table = GetDataTable(sql);
            List<Models.AssemblyInfo> assemblyInfolist = new List<Models.AssemblyInfo>();
            foreach (DataRow dr in table.Rows)
            {
                Models.AssemblyInfo assemblyInfoItem = new Models.AssemblyInfo()
                {
                    TypeID = Convert.ToInt32(dr["TypeID"]),
                    AssemblyName = dr["AssemblyName"].ToString(),
                    FullTypeName = dr["FullTypeName"].ToString()
                };
                assemblyInfolist.Add(assemblyInfoItem);
            }

            return assemblyInfolist;
        }
        public static Dictionary<string, List<string>> GetAssemblyInfoDic(int resultID)
        {
            string sql = @"select AssemblyInfo.* from AssemblyInfo
inner join AssemblyLink on AssemblyInfo.TypeID=AssemblyLink.TypeID
inner join Results on AssemblyLink.ContentID=Results.ContentID
where ResultID={0}
order by AssemblyName, FullTypeName";
            sql = string.Format(sql, resultID);

            DataTable table = GetDataTable(sql);
            Dictionary<string, List<string>> assemblyInfoDic = new Dictionary<string, List<string>>();
            string assemblyName, fullTypeName;
            foreach (DataRow dr in table.Rows)
            {
                assemblyName = dr["AssemblyName"].ToString();
                fullTypeName = dr["FullTypeName"].ToString();
                if (assemblyInfoDic.ContainsKey(assemblyName))
                    assemblyInfoDic[assemblyName].Add(fullTypeName);
                else
                    assemblyInfoDic.Add(assemblyName, new List<string>() { fullTypeName });
            }

            return assemblyInfoDic;
        }
        public static List<bug> GetRelatedBugs(int resultID)
        {
            string sql = @"select distinct bugs.bug_id,bugs.status,bugs.title from bugs
inner join bug_surface on bug_surface.bug_id=bugs.bug_id
inner join surfaces on surfaces.s_id=bug_surface.s_id
inner join AssemblyInfo on AssemblyInfo.AssemblyName=surfaces.assembly_name and AssemblyInfo.FullTypeName=surfaces.class_name
inner join AssemblyLink on AssemblyLink.TypeID=AssemblyInfo.TypeID
inner join Results on Results.ContentID=AssemblyLink.ContentID
where Results.ResultID={0}
order by bugs.bug_id desc";
            sql = string.Format(sql, resultID);

            DataTable table = GetDataTable(sql);
            List<bug> bugList = new List<bug>();
            foreach (DataRow dr in table.Rows)
            {
                bug bugItem = new bug()
                {
                    bug_id = Convert.ToInt32(dr["bug_id"]),
                    status = Convert.ToInt16(dr["status"]),
                    title = dr["title"].ToString(),
                };
                bugList.Add(bugItem);
            }

            return bugList;
        }
        public static Client GetClient(int resultID)
        {
            SulpHurEntities entities = new SulpHurEntities();
            var list = from c in entities.Clients
                       join u in entities.UIContents on c.ClientID equals u.ClientID
                       join r in entities.Results on u.ContentID equals r.ContentID
                       where r.ResultID == resultID
                       select new { c.OSLanguage, c.OSType };
            var item = list.FirstOrDefault();
            Client client = new Client() { OSLanguage = item.OSLanguage, OSType = item.OSType };

            return client;
        }

        /// <summary>
        /// &gt;ruleID, buildLanguage, latestBuildNo, latestResultType>
        /// latestResultType - Fail > Warning > Pass
        /// </summary>
        /// <param name="aboveBuildNo"></param>
        /// <param name="assemblyName"></param>
        /// <param name="fullTypeName"></param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, LatestResultSummary>> GetLatestResultSummary(string aboveBuildNo, string assemblyName, string ruleIDs, string fullTypeName, string buildLanguages)
        {
            string sql = @"select results.RuleID,BuildInfo.Language,MAX(BuildInfo.BuildNo) as maxBuildNo,Results.ResultType from Results
inner join UIContents on Results.ContentID=UIContents.ContentID
inner join BuildInfo on UIContents.BuildID=BuildInfo.BuildID
inner join AssemblyLink on Results.ContentID=AssemblyLink.ContentID
inner join AssemblyInfo on AssemblyLink.TypeID=AssemblyInfo.TypeID
where 1=1{0}
group by results.RuleID,results.ResultType,BuildInfo.Language
order by RuleID,Language,maxBuildNo desc";
            string buildNoFilterClause = string.Empty;
            if (!string.IsNullOrEmpty(aboveBuildNo))
                buildNoFilterClause = GenerateBuildNOFilterClause(">=", aboveBuildNo);
            string buildLanguageFilterClause = string.Empty;
            if (!string.IsNullOrEmpty(buildLanguages))
                buildLanguageFilterClause = GenerateBuildLanFilterClause(buildLanguages);
            string ruleIDFilterClause = string.Empty;
            if (!string.IsNullOrEmpty(ruleIDs))
                ruleIDFilterClause = GenerateRuleIDFilterINClause(ruleIDs);
            string assemblyNameFilerClause = string.Empty;
            if (!string.IsNullOrEmpty(assemblyName))
                assemblyNameFilerClause = GenerateAssemblyNameFilterClause(assemblyName);
            string fullTypeNameFilerClause = string.Empty;
            if (!string.IsNullOrEmpty(fullTypeName))
                fullTypeNameFilerClause = GenerateFullTypeNameInFilterClause(fullTypeName);
            string fullFilterClause = buildNoFilterClause + buildLanguageFilterClause + ruleIDFilterClause + assemblyNameFilerClause + fullTypeNameFilerClause;
            sql = string.Format(sql, fullFilterClause);

            DataTable table = GetDataTable(sql);

            Dictionary<string, Dictionary<string, LatestResultSummary>> dic = new Dictionary<string, Dictionary<string, LatestResultSummary>>();
            string ruleIDString;
            string buildLanguage, maxBuildNo, resultType;
            int hasLatestBuildNo;
            LatestResultSummary latestResultSummary;
            Dictionary<string, LatestResultSummary> languageAndLatestResultSumamry;
            foreach (DataRow dr in table.Rows)
            {
                ruleIDString = dr["RuleID"].ToString();
                buildLanguage = dr["Language"].ToString();
                maxBuildNo = dr["maxBuildNo"].ToString();
                maxBuildNo = maxBuildNo.Split('.')[2];
                resultType = dr["ResultType"].ToString();

                if (dic.ContainsKey(ruleIDString))
                {
                    if (dic[ruleIDString].ContainsKey(buildLanguage))
                    {
                        hasLatestBuildNo = string.Compare(dic[ruleIDString][buildLanguage].MainBuildVersion, maxBuildNo);
                        if (hasLatestBuildNo == -1)
                        {
                            dic[ruleIDString][buildLanguage].MainBuildVersion = maxBuildNo;
                            dic[ruleIDString][buildLanguage].ResultType = resultType;
                        }
                        else if (hasLatestBuildNo == 0)
                        {
                            if (dic[ruleIDString][buildLanguage].ResultType == "Pass" || (dic[ruleIDString][buildLanguage].ResultType == "Warning" && resultType == "Fail"))
                                dic[ruleIDString][buildLanguage].ResultType = resultType;
                        }
                    }
                    else
                    {
                        latestResultSummary = new LatestResultSummary() { MainBuildVersion = maxBuildNo, ResultType = resultType };
                        dic[ruleIDString].Add(buildLanguage, latestResultSummary);
                    }
                }
                else
                {
                    latestResultSummary = new LatestResultSummary() { MainBuildVersion = maxBuildNo, ResultType = resultType };
                    languageAndLatestResultSumamry = new Dictionary<string, LatestResultSummary>();
                    languageAndLatestResultSumamry.Add(buildLanguage, latestResultSummary);
                    dic.Add(ruleIDString, languageAndLatestResultSumamry);
                }
            }

            return dic;
        }
        #endregion

        #region update from multiple tables
        public static int LinkToBug(int bugId, string assemblyName, string fullTypeName)
        {
            using (SqlConnection sqlConnection = new SqlConnection(connStr))
            using (SqlCommand sqlCommand = new SqlCommand("spLinkBug", sqlConnection))
            {
                sqlConnection.Open();
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.Add(new SqlParameter("@bug_id", SqlDbType.Int) { Value = bugId });
                sqlCommand.Parameters.Add(new SqlParameter("@class_name", SqlDbType.VarChar) { Value = fullTypeName });
                sqlCommand.Parameters.Add(new SqlParameter("@assembly_name", SqlDbType.VarChar) { Value = assemblyName });

                return sqlCommand.ExecuteNonQuery();
            }
        }
        #endregion

        #region common sql function
        /// <summary>
        /// Executes the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored.
        /// </summary>
        /// <param name="connStr"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static object ExecuteScalar(string connStr, string sql)
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

        public static DataTable GetDataTable(string sql)
        {
            DataTable table = null;
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand command = new SqlCommand(sql, conn))
            {
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 300000;

                conn.Open();
                table = new DataTable();
                SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                dataAdapter.Fill(table);
            }

            return table;
        }
        #endregion
    }
}