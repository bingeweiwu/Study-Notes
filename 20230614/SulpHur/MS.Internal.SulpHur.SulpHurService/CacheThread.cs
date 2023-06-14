using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MS.Internal.SulpHur.SulpHurService
{
    public class CacheThread
    {
        private Thread cacheFastTableThread;
        private Thread ClearTempResultImageThread;
        string connStr;
        string[] aliases;
        bool isEmpty = false;
        bool isClearTmp_ResultImage = false;
        int index = 0;
        public CacheThread()
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
            aliases = DBOperator.ExecuteQuery(connStr, "select * from SulPhurReviewer", "alias");
            foreach (string s in aliases)
            {
                Log.WriteServerLog(s, TraceLevel.Info);
            }
            Log.WriteServerLog("ExecuteQuery finish...", TraceLevel.Info);


            cacheFastTableThread = new Thread(TimingCacheResultInfo);
            ClearTempResultImageThread = new Thread(TimingClearTempResultImage);
        }
        public void Start()
        {
            cacheFastTableThread.Start();
            ClearTempResultImageThread.Start();
        }
        public bool IsAlive()
        {
            return cacheFastTableThread.IsAlive && ClearTempResultImageThread.IsAlive;
        }
        public void Stop()
        {
            cacheFastTableThread.Join(new TimeSpan(0, 2, 0));
            ClearTempResultImageThread.Join(new TimeSpan(0, 2, 0));
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
                    DBOperator.TruncateCacheTable(connStr, "TRUNCATE TABLE ResultsInfoByAlias");
                    Trace.WriteLine("clear ResultsInfoByAlias complete");
                    isEmpty = true;
                }

                if (beijingTime.DayOfWeek == DayOfWeek.Friday && beijingTime.Hour <= 12 && beijingTime.Hour >= 6)
                {
                    Trace.WriteLine("time in 6:00 - 12:00 on Friday, isEmpty = " + isEmpty.ToString());
                    isWithinSchedule = true;
                }

                if (isWithinSchedule && isEmpty)
                {
                    Trace.WriteLine("The current time is within the specified time ");
                    for (index = 0; index < aliases.Length; index++)
                    {
                        Trace.WriteLine("start cache " + aliases[index] + " data");
                        try
                        {
                            DBOperator.ExecuteScalar(connStr, insertSqls[index]);
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine("index = " + index + "alias = " + aliases[index] + " ex.Message = " + ex.Message);
                        }
                        Trace.WriteLine("cache " + aliases[index] + " data complete");
                    }

                    resultIDs = DBOperator.ExecuteQuery(connStr, "select ResultID from ResultsInfoByAlias", "ResultID");
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
                            CacheScreenShot(int.Parse(resultID), "F:\\WebSites\\SulpHurReports");
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
        public void CacheScreenShot(int resultID, string projectPath)
        {
            string imagePath = string.Format(@"Tmp_ResultImage/{0}.jpg", resultID);
            Trace.WriteLine("imagePath = " + imagePath);
            string localImagePath = Path.Combine(projectPath, imagePath);
            Trace.WriteLine("localImagePath = " + localImagePath);
            if (!File.Exists(localImagePath))
            {
                byte[] resultImage = DBOperator.GetResultScreenshot(resultID);
                System.Drawing.Image image = null;
                if (resultImage == null)
                {
                    resultImage = DBOperator.GetUIScreenshot(resultID);
                    if (resultImage != null)
                        image = Bitmap.FromStream(new MemoryStream(resultImage));
                }
                else
                    image = Bitmap.FromStream(new MemoryStream(resultImage));
                image.Save(localImagePath, ImageFormat.Jpeg);
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
            readyToDelete = DBOperator.ExecuteQuery(connStr, getReviewedResultID, "ResultID");
            foreach (string s in readyToDelete)
            {
                Trace.WriteLine("delete " + folderPath + '\\' + s + ".jpg");
                File.Delete(folderPath + '\\' + s + ".jpg");
            }
        }
    }
}
