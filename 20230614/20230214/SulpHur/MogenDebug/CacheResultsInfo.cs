using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MogenDebug
{
    public class CacheResultsInfo
    {
        string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["ADOConn"].ToString();
        string[] aliases;

        public CacheResultsInfo()
        {
            aliases = ExecuteQuery(connStr, "select * from SulPhurReviewer", "alias");
        }

        public void TimingCacheResultInfo()
        {
            Console.WriteLine("enter TimingCacheResultInfo");
            string currentDate = DateTime.Now.ToString();
            string beforeCurrentDate = DateTime.Now.AddDays(-120).ToString();
            bool isEmpty = false;
            bool isClearTmp_ResultImage = false;
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
            insertSqls[8] = @"INSERT INTO ResultsInfoByAlias (ResultID, buildNo, Language, RuleName, ResultType, UIName, UserName, OSType, DateUploaded, CreateDate, ReviewFlag, alias) SELECT ResultID, buildNo, Language, RuleName, ResultType, UIName, UserName, OSType, DateUploaded, CreateDate, ReviewFlag,'" + aliases[8] + @"' as alias  FROM (
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
 WHERE 1=1  AND [BuildInfo].[Language] IN ('ENU') AND [Results].[RuleID] IN (41) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"' AND [Results].[ResultType] IN ('Fail') AND [Results].[ReviewFlag]=0) AS newT) AS t ORDER BY rowNum";
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
            string cacheResultIDSql = "insert into ResultIDbyAlias (ResultID,alias) select ResultID,alias from ResultsInfobyAlias";
            string mogen = insertSqls[9];
            Console.WriteLine(mogen);
            ExecuteScalar(connStr, cacheResultIDSql);

            bool isWithinSchedule = false;

            if ( beijingTime.DayOfWeek == DayOfWeek.Thursday && beijingTime.Hour >= 19)
            {
                Console.WriteLine("time after 19:00 on Thursday, isEmpty = " + isEmpty.ToString());
                isWithinSchedule = true;
            }
            else if (beijingTime.DayOfWeek == DayOfWeek.Friday && beijingTime.Hour < 5)
            {
                Console.WriteLine("time before 5:00 on Friday, isEmpty = " + isEmpty.ToString());
                isWithinSchedule = true;
            }

            if (beijingTime.DayOfWeek == DayOfWeek.Thursday && beijingTime.Hour == 18 && !isEmpty)
            {
                Console.WriteLine("start clear ResultIDbyAlias and ResultsInfoByAlias ");
                TruncateCacheTable(connStr, "TRUNCATE TABLE ResultIDbyAlias");
                TruncateCacheTable(connStr, "TRUNCATE TABLE ResultsInfoByAlias");
                Console.WriteLine("clear ResultIDbyAlias and ResultsInfoByAlias complete");
                isEmpty = true;
            }

            if (beijingTime.DayOfWeek == DayOfWeek.Monday && !isClearTmp_ResultImage)
            {
                string folderPath = @"F:\SD\ConfigMgr-Test\src\Tools\SulpHur20\SulpHur\SulpHurReport\Tmp_ResultImage\";
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
                    File.Delete(folderPath + '\\' + s + ".jpg");
                }
                isClearTmp_ResultImage = true;
            }

            if (isWithinSchedule && isEmpty)
            {
                for (int i = 0; i < aliases.Length; i++)
                {
                    Console.WriteLine("start cache " + aliases[i] + " data");
                    ExecuteScalar(connStr, insertSqls[i]);
                    Console.WriteLine("cache " + aliases[i] + " data complete");
                }

                resultIDs = ExecuteQuery(connStr, "select ResultID from ResultsInfoByAlias", "ResultID");
                foreach (string resultID in resultIDs)
                {
                    GetScreenshotPath(int.Parse(resultID));
                }
                isEmpty = false;
            }
            else
            {
                Console.WriteLine("The current time is not within the specified time range or data table has been filled in ");
            }
        }
        //
        public static string[] ExecuteQuery(string connStr, string sql, string columnName)
        {
            Console.WriteLine(sql + "   " + columnName);
            List<string> resultList = new List<string>();

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
            return resultList.ToArray();
        }
        //


        public static void TruncateCacheTable(string connStr, string truncateSql)
        {
            Console.WriteLine(truncateSql);
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
        //
        public static void ExecuteScalar(string connStr, string sql)
        {
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

        //
        public string GetScreenshotPath(int resultID)
        {
            string imagePath = string.Format(@"Tmp_ResultImage/{0}.jpg", resultID);
            Console.WriteLine("imagePath = " + imagePath);
            string localImagePath = Path.Combine("F:\\SD\\ConfigMgr-Test\\src\\Tools\\SulpHur20\\SulpHur\\SulpHurReport\\", imagePath);
            Console.WriteLine("localImagePath = " + localImagePath);
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

            return imagePath;
        }
        //
        public static byte[] GetResultScreenshot(int resultID)
        {
            Console.WriteLine("GetResultScreenshot(" + resultID + ")");
            try
            {
                ZMogenDebugSulpHurEntities entities = new ZMogenDebugSulpHurEntities();
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
        //
        public static Byte[] GetUIScreenshot(int resultID)
        {
            Console.WriteLine("GetUIScreenshot(" + resultID + ")");
            ZMogenDebugSulpHurEntities entities = new ZMogenDebugSulpHurEntities();
            var list = from r in entities.Results
                       join u in entities.UIContents on r.ContentID equals u.ContentID
                       where r.ResultID == resultID
                       select u.UIScreenShot;
            Byte[] uiScreenshot = list.FirstOrDefault()?.ToArray();
            return uiScreenshot;
        }
    }
}
