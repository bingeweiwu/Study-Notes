using MogenDebug;
using MS.Internal.SulpHur.CMRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MS.Internal.SulpHur.UICompliance;
using System.IO;
using System.Reflection;
using MS.Internal.SulpHur.SulpHurService;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing;
using System.Data.Entity.Core;
using System.Xml.Linq;
using System.Threading;

namespace ZMogenDebug
{
    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public DateTime Birth { get; set; }
    }

    public class Program
    {

        string connStr = ConfigurationManager.ConnectionStrings["ADOConn"].ToString();


        //2 ge person
        //bool
        public delegate bool SortDelegate(Person p1, Person P2);
        public static void SortName(List<Person> personList)
        {
            //if(personList[i].Name
            if (string.Compare(personList[0].Name, personList[1].Name) > 0) { }

        }
        public static void SortAge(List<Person> personList)
        {
            //if(personList[i].Age
            if (personList[0].Age > personList[1].Age) { }
        }
        public static void SortBirth(List<Person> personList)
        {
            //if(personList[i].Birth
            if (personList[0].Birth > personList[1].Birth) { }
        }

        public static void SortUpdate(List<Person> personList, SortDelegate compareMethod)
        {
            //if(personList[i].Name
            if (compareMethod(personList[0], personList[1])) { }

        }
        public static bool CompareName(Person p1, Person p2)
        {
            return string.Compare(p1.Name, p2.Name) > 0;
        }

        static List<UIComplianceRuleBase> ruleList = new List<UIComplianceRuleBase>();
        public static bool IsSubclassOf(Type t, Type b)
        {
            if (t.BaseType == null) return false;
            if (t.BaseType.Equals(b)) return true;
            return IsSubclassOf(t.BaseType, b);
        }
        private string truncateString(string s)
        {
            return s.Substring(0, 200);
        }

        //public static void UpdateTable()
        //{
        //    ZMogenDebugSulpHurEntities zMogenDebugSulpHurEntities = new ZMogenDebugSulpHurEntities();
        //    var results = from n in zMogenDebugSulpHurEntities.FilterSpellChecks
        //                  where n.normalPunctuation != null
        //                  select n.normalPunctuation;
        //    //int i = 1;
        //    FilterSpellCheck_normalPunctuation filterSpellCheck_normalPunctuation;
        //    foreach (var result in results)
        //    {
        //        filterSpellCheck_normalPunctuation = new FilterSpellCheck_normalPunctuation()
        //        {
        //            //ID = i++,
        //            normalPunctuation = result
        //        };

        //        zMogenDebugSulpHurEntities.FilterSpellCheck_normalPunctuation.Add(filterSpellCheck_normalPunctuation);
        //    }
        //    zMogenDebugSulpHurEntities.SaveChanges();
        //}

        public static string GenerateReviewedTypeFilterClause(string reviewedType)
        {
            //-1 == All; 0 == UnReviewed; 1 == Reviewed; 'undefined' means none is selected
            string reviewedTypeFilterClause = string.Empty;
            if (reviewedType == "UnReviewed") reviewedType = "0";
            if (reviewedType == "Reviewed") reviewedType = "1";
            if (reviewedType == "All") reviewedType = "-1";//"'Reviewed'"
            if (reviewedType != "-1")
                reviewedTypeFilterClause = " AND [Results].[ReviewFlag]=" + reviewedType;

            return reviewedTypeFilterClause;
        }

        public void TimingCacheResultID()
        {
            string[] sqls = new string[10];
            string currentDate = DateTime.Now.ToString();
            string beforeCurrentDate = DateTime.Now.AddDays(-120).ToString();

            //           sqls[0] = @"SELECT ResultID FROM (
            //SELECT ROW_NUMBER() OVER (ORDER BY [TOTAL].[BuildNo] DESC, [TOTAL].[ResultID] DESC) AS rowNum, [TOTAL].[ResultID], [TOTAL].[BuildNo], 
            //[TOTAL].[Language], [TOTAL].[RuleName], [TOTAL].[ResultType], [TOTAL].[UIName], [TOTAL].[UserName], [TOTAL].[OSType], [TOTAL].[DateUploaded], 
            //[TOTAL].[CreateDate], [TOTAL].[ReviewFlag] FROM (
            //SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [Rules].[RuleName], [UIContents].[UIName], [Clients].[UserName], 
            //[UIContents].[DateUploaded], [Results].[ResultType], [Results].[CreateDate], [Results].[ReviewFlag], [BuildInfo].[Language], [Clients].[OSType]
            //FROM [Results]
            //INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            //INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            //INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            //INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            //WHERE 1=1  AND [BuildInfo].[Language] IN ('CHS','CHT','CSY','DEU','JPN') AND [Results].[RuleID] IN (36) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"' AND [Results].[ResultType] IN ('Fail') AND [Results].[ReviewFlag]=0) AS [TOTAL]
            //INNER JOIN ( 
            //SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType] FROM [Results]
            //INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            //INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            //INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            //INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            //WHERE 1=1  AND [BuildInfo].[Language] IN ('CHS','CHT','CSY','DEU','JPN') AND [Results].[RuleID] IN (36) AND [CreateDate]> '" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"'
            //GROUP BY [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType]) AS [GroupTable]
            //ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
            //AND [TOTAL].[UIName]=[GroupTable].[UIName]
            //AND [TOTAL].[OSType]=[GroupTable].[OSType]
            //AND [TOTAL].[Language]=[GroupTable].[Language]) AS newT  ORDER BY rowNum";
            //           sqls[1] = @"SELECT ResultID FROM (
            //SELECT ROW_NUMBER() OVER (ORDER BY [TOTAL].[BuildNo] DESC, [TOTAL].[ResultID] DESC) AS rowNum, [TOTAL].[ResultID], [TOTAL].[BuildNo], 
            //[TOTAL].[Language], [TOTAL].[RuleName], [TOTAL].[ResultType], [TOTAL].[UIName], [TOTAL].[UserName], [TOTAL].[OSType], [TOTAL].[DateUploaded], 
            //[TOTAL].[CreateDate], [TOTAL].[ReviewFlag] FROM (
            //SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [Rules].[RuleName], [UIContents].[UIName], [Clients].[UserName], 
            //[UIContents].[DateUploaded], [Results].[ResultType], [Results].[CreateDate], [Results].[ReviewFlag], [BuildInfo].[Language], [Clients].[OSType]
            //FROM [Results]
            //INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            //INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            //INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            //INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            //WHERE 1=1  AND [BuildInfo].[Language] IN ('ENU') AND [Results].[RuleID] IN (36) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"' AND [Results].[ResultType] IN ('Fail') AND [Results].[ReviewFlag]=0) AS [TOTAL]
            //INNER JOIN ( 
            //SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType] FROM [Results]
            //INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            //INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            //INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            //INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            //WHERE 1=1  AND [BuildInfo].[Language] IN ('ENU') AND [Results].[RuleID] IN (36) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"'
            //GROUP BY [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType]) AS [GroupTable]
            //ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
            //AND [TOTAL].[UIName]=[GroupTable].[UIName]
            //AND [TOTAL].[OSType]=[GroupTable].[OSType]
            //AND [TOTAL].[Language]=[GroupTable].[Language]) AS newT  ORDER BY rowNum";
            //           sqls[2] = @"SELECT ResultID FROM (
            //SELECT ROW_NUMBER() OVER (ORDER BY [TOTAL].[BuildNo] DESC, [TOTAL].[ResultID] DESC) AS rowNum, [TOTAL].[ResultID], [TOTAL].[BuildNo], 
            //[TOTAL].[Language], [TOTAL].[RuleName], [TOTAL].[ResultType], [TOTAL].[UIName], [TOTAL].[UserName], [TOTAL].[OSType], [TOTAL].[DateUploaded], 
            //[TOTAL].[CreateDate], [TOTAL].[ReviewFlag] FROM (
            //SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [Rules].[RuleName], [UIContents].[UIName], [Clients].[UserName], 
            //[UIContents].[DateUploaded], [Results].[ResultType], [Results].[CreateDate], [Results].[ReviewFlag], [BuildInfo].[Language], [Clients].[OSType]
            //FROM [Results]
            //INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            //INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            //INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            //INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            //WHERE 1=1  AND [BuildInfo].[Language] IN ('ESN','FRA','HUN','ITA','KOR','NLD') AND [Results].[RuleID] IN (36) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"' AND [Results].[ResultType] IN ('Fail') AND [Results].[ReviewFlag]=0) AS [TOTAL]
            //INNER JOIN ( 
            //SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType] FROM [Results]
            //INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            //INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            //INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            //INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            //WHERE 1=1  AND [BuildInfo].[Language] IN ('ESN','FRA','HUN','ITA','KOR','NLD') AND [Results].[RuleID] IN (36) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"'
            //GROUP BY [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType]) AS [GroupTable]
            //ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
            //AND [TOTAL].[UIName]=[GroupTable].[UIName]
            //AND [TOTAL].[OSType]=[GroupTable].[OSType]
            //AND [TOTAL].[Language]=[GroupTable].[Language]) AS newT  ORDER BY rowNum";
            //           sqls[3] = @"SELECT ResultID FROM (
            //SELECT ROW_NUMBER() OVER (ORDER BY [TOTAL].[BuildNo] DESC, [TOTAL].[ResultID] DESC) AS rowNum, [TOTAL].[ResultID], [TOTAL].[BuildNo], 
            //[TOTAL].[Language], [TOTAL].[RuleName], [TOTAL].[ResultType], [TOTAL].[UIName], [TOTAL].[UserName], [TOTAL].[OSType], [TOTAL].[DateUploaded], 
            //[TOTAL].[CreateDate], [TOTAL].[ReviewFlag] FROM (
            //SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [Rules].[RuleName], [UIContents].[UIName], [Clients].[UserName], 
            //[UIContents].[DateUploaded], [Results].[ResultType], [Results].[CreateDate], [Results].[ReviewFlag], [BuildInfo].[Language], [Clients].[OSType]
            //FROM [Results]
            //INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            //INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            //INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            //INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            //WHERE 1=1  AND [BuildInfo].[Language] IN ('PLK','PTB','PTG','RUS','SVE','TRK') AND [Results].[RuleID] IN (36) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"' AND [Results].[ResultType] IN ('Fail') AND [Results].[ReviewFlag]=0) AS [TOTAL]
            //INNER JOIN ( 
            //SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType] FROM [Results]
            //INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            //INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            //INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            //INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            //WHERE 1=1  AND [BuildInfo].[Language] IN ('PLK','PTB','PTG','RUS','SVE','TRK') AND [Results].[RuleID] IN (36) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"'
            //GROUP BY [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType]) AS [GroupTable]
            //ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
            //AND [TOTAL].[UIName]=[GroupTable].[UIName]
            //AND [TOTAL].[OSType]=[GroupTable].[OSType]
            //AND [TOTAL].[Language]=[GroupTable].[Language]) AS newT  ORDER BY rowNum";
            //           sqls[4] = @"SELECT ResultID FROM (
            //SELECT ROW_NUMBER() OVER (ORDER BY [TOTAL].[BuildNo] DESC, [TOTAL].[ResultID] DESC) AS rowNum, [TOTAL].[ResultID], [TOTAL].[BuildNo], 
            //[TOTAL].[Language], [TOTAL].[RuleName], [TOTAL].[ResultType], [TOTAL].[UIName], [TOTAL].[UserName], [TOTAL].[OSType], [TOTAL].[DateUploaded], 
            //[TOTAL].[CreateDate], [TOTAL].[ReviewFlag] FROM (
            //SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [Rules].[RuleName], [UIContents].[UIName], [Clients].[UserName], 
            //[UIContents].[DateUploaded], [Results].[ResultType], [Results].[CreateDate], [Results].[ReviewFlag], [BuildInfo].[Language], [Clients].[OSType]
            //FROM [Results]
            //INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            //INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            //INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            //INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            //WHERE 1=1  AND [BuildInfo].[Language] IN ('CHS') AND [Results].[RuleID] IN (31) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"' AND [Results].[ResultType] IN ('Fail') AND [Results].[ReviewFlag]=0) AS [TOTAL]
            //INNER JOIN ( 
            //SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType] FROM [Results]
            //INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            //INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            //INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            //INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            //WHERE 1=1  AND [BuildInfo].[Language] IN ('CHS') AND [Results].[RuleID] IN (31) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"'
            //GROUP BY [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType]) AS [GroupTable]
            //ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
            //AND [TOTAL].[UIName]=[GroupTable].[UIName]
            //AND [TOTAL].[OSType]=[GroupTable].[OSType]
            //AND [TOTAL].[Language]=[GroupTable].[Language]) AS newT  ORDER BY rowNum";
            //           sqls[5] = @"SELECT ResultID FROM (
            //SELECT ROW_NUMBER() OVER (ORDER BY [TOTAL].[BuildNo] DESC, [TOTAL].[ResultID] DESC) AS rowNum, [TOTAL].[ResultID], [TOTAL].[BuildNo], 
            //[TOTAL].[Language], [TOTAL].[RuleName], [TOTAL].[ResultType], [TOTAL].[UIName], [TOTAL].[UserName], [TOTAL].[OSType], [TOTAL].[DateUploaded], 
            //[TOTAL].[CreateDate], [TOTAL].[ReviewFlag] FROM (
            //SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [Rules].[RuleName], [UIContents].[UIName], [Clients].[UserName], 
            //[UIContents].[DateUploaded], [Results].[ResultType], [Results].[CreateDate], [Results].[ReviewFlag], [BuildInfo].[Language], [Clients].[OSType]
            //FROM [Results]
            //INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            //INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            //INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            //INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            //WHERE 1=1  AND [BuildInfo].[Language] IN ('CHT','CSY','DAN','DEU','ELL','ESN','FIN','FRA','HUN','ITA','JPN','KOR','NLD','NOR','PLK','PTB','PTG','RUS','SVE','TRK') AND [Results].[RuleID] IN (31) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"' AND [Results].[ResultType] IN ('Fail') AND [Results].[ReviewFlag]=0) AS [TOTAL]
            //INNER JOIN ( 
            //SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType] FROM [Results]
            //INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            //INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            //INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            //INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            //WHERE 1=1  AND [BuildInfo].[Language] IN ('CHT','CSY','DAN','DEU','ELL','ESN','FIN','FRA','HUN','ITA','JPN','KOR','NLD','NOR','PLK','PTB','PTG','RUS','SVE','TRK') AND [Results].[RuleID] IN (31) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"'
            //GROUP BY [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType]) AS [GroupTable]
            //ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
            //AND [TOTAL].[UIName]=[GroupTable].[UIName]
            //AND [TOTAL].[OSType]=[GroupTable].[OSType]
            //AND [TOTAL].[Language]=[GroupTable].[Language]) AS newT ORDER BY rowNum";
            //           sqls[6] = @"SELECT ResultID FROM (
            //SELECT ROW_NUMBER() OVER (ORDER BY [TOTAL].[BuildNo] DESC, [TOTAL].[ResultID] DESC) AS rowNum, [TOTAL].[ResultID], [TOTAL].[BuildNo], 
            //[TOTAL].[Language], [TOTAL].[RuleName], [TOTAL].[ResultType], [TOTAL].[UIName], [TOTAL].[UserName], [TOTAL].[OSType], [TOTAL].[DateUploaded], 
            //[TOTAL].[CreateDate], [TOTAL].[ReviewFlag] FROM (
            //SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [Rules].[RuleName], [UIContents].[UIName], [Clients].[UserName], 
            //[UIContents].[DateUploaded], [Results].[ResultType], [Results].[CreateDate], [Results].[ReviewFlag], [BuildInfo].[Language], [Clients].[OSType]
            //FROM [Results]
            //INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            //INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            //INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            //INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            //WHERE 1=1  AND [BuildInfo].[Language] IN ('ENU') AND [Results].[RuleID] IN (31) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"' AND [Results].[ResultType] IN ('Fail') AND [Results].[ReviewFlag]=0) AS [TOTAL]
            //INNER JOIN ( 
            //SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType] FROM [Results]
            //INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            //INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            //INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            //INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            //WHERE 1=1  AND [BuildInfo].[Language] IN ('ENU') AND [Results].[RuleID] IN (31) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"'
            //GROUP BY [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType]) AS [GroupTable]
            //ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
            //AND [TOTAL].[UIName]=[GroupTable].[UIName]
            //AND [TOTAL].[OSType]=[GroupTable].[OSType]
            //AND [TOTAL].[Language]=[GroupTable].[Language]) AS newT  ORDER BY rowNum";
            //           sqls[7] = @"SELECT ResultID FROM (
            //SELECT ROW_NUMBER() OVER (ORDER BY [TOTAL].[BuildNo] DESC, [TOTAL].[ResultID] DESC) AS rowNum, [TOTAL].[ResultID], [TOTAL].[BuildNo], 
            //[TOTAL].[Language], [TOTAL].[RuleName], [TOTAL].[ResultType], [TOTAL].[UIName], [TOTAL].[UserName], [TOTAL].[OSType], [TOTAL].[DateUploaded], 
            //[TOTAL].[CreateDate], [TOTAL].[ReviewFlag] FROM (
            //SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [Rules].[RuleName], [UIContents].[UIName], [Clients].[UserName], 
            //[UIContents].[DateUploaded], [Results].[ResultType], [Results].[CreateDate], [Results].[ReviewFlag], [BuildInfo].[Language], [Clients].[OSType]
            //FROM [Results]
            //INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            //INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            //INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            //INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            //WHERE 1=1  AND [BuildInfo].[Language] IN ('ENU') AND [Results].[RuleID] IN (39) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"' AND [Results].[ResultType] IN ('Fail') AND [Results].[ReviewFlag]=0) AS [TOTAL]
            //INNER JOIN ( 
            //SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName]
            //FROM [Results]
            //INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            //INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            //INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            //INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            //WHERE 1=1  AND [BuildInfo].[Language] IN ('ENU') AND [Results].[RuleID] IN (39) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"'
            //GROUP BY [UIContents].[UIName]) AS [GroupTable]
            //ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
            //AND [TOTAL].[UIName]=[GroupTable].[UIName]) AS newT ORDER BY rowNum";
            //           sqls[8] = @"SELECT ResultID FROM (
            //SELECT ROW_NUMBER() OVER (ORDER BY [TOTAL].[BuildNo] DESC, [TOTAL].[ResultID] DESC) AS rowNum, [TOTAL].[ResultID], [TOTAL].[BuildNo], 
            //[TOTAL].[Language], [TOTAL].[RuleName], [TOTAL].[ResultType], [TOTAL].[UIName], [TOTAL].[UserName], [TOTAL].[OSType], [TOTAL].[DateUploaded], 
            //[TOTAL].[CreateDate], [TOTAL].[ReviewFlag] FROM (
            //SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [Rules].[RuleName], [UIContents].[UIName], [Clients].[UserName], 
            //[UIContents].[DateUploaded], [Results].[ResultType], [Results].[CreateDate], [Results].[ReviewFlag], [BuildInfo].[Language], [Clients].[OSType]
            //FROM [Results]
            //INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            //INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            //INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            //INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            //WHERE 1=1  AND [BuildInfo].[Language] IN ('ENU') AND [Results].[RuleID] IN (41) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"' AND [Results].[ResultType] IN ('Fail') AND [Results].[ReviewFlag]=0) AS [TOTAL]
            //INNER JOIN ( 
            //SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType] FROM [Results]
            //INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            //INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            //INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            //INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            //WHERE 1=1  AND [BuildInfo].[Language] IN ('ENU') AND [Results].[RuleID] IN (41) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"'
            //GROUP BY [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType]) AS [GroupTable]    
            //ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
            //AND [TOTAL].[UIName]=[GroupTable].[UIName]
            //AND [TOTAL].[OSType]=[GroupTable].[OSType]
            //AND [TOTAL].[Language]=[GroupTable].[Language]) AS newT ORDER BY rowNum";
            //           sqls[9] = "";

            string[] aliases = { "v-fjiang", "v-xihl", "v-tomyang", "v-haos", "v-lulliu", "v-xichu", "v-juanwu", "v-daojunsong", "v-danielxu", "v-mogenzhang" };


            // 获取当前时间并将其转换为北京时间
            DateTime localTime = DateTime.Now;
            TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
            DateTime beijingTime = TimeZoneInfo.ConvertTime(localTime, cstZone);

            // 判断当前时间是否为周四晚上7点到次日凌晨5点之间
            bool isWithinSchedule = false;

            if (beijingTime.DayOfWeek == DayOfWeek.Thursday && beijingTime.Hour >= 19)
            {
                isWithinSchedule = true;
            }
            else if (beijingTime.DayOfWeek == DayOfWeek.Friday && beijingTime.Hour < 5)
            {
                isWithinSchedule = true;
            }

            // 如果当前时间在指定的时间范围内，则按照每个小时顺序读取包含十个元素的数组中的字符串
            if (isWithinSchedule)
            {
                // 计算当前时间距离开始时间（周四晚上7点）的小时数
                TimeSpan span = beijingTime - new DateTime(beijingTime.Year, beijingTime.Month, beijingTime.Day, 19, 0, 0);
                int hour = (int)Math.Floor(span.TotalHours);

                // 读取数组中对应小时数的字符串
                string sql = sqls[hour];
                // get resultID by sql and save to database
                // 输出结果

            }
            else
            {
                Trace.WriteLine("The current time is not within the specified time range");
            }

            Console.ReadKey();
        }

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
        public static void TruncateResultIDbyAlias(string connStr)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = conn;
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 300000;

                    string truncateSql = "TRUNCATE TABLE ResultIDbyAlias";
                    command.CommandText = truncateSql;
                    command.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        public static void TruncateResultsInfoByResultID(string connStr)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = conn;
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 300000;

                    string truncateSql = "TRUNCATE TABLE ResultsInfoByAlias";
                    command.CommandText = truncateSql;
                    command.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

       
        public static void CacheResultInfo()
        {
            string connStr = ConfigurationManager.ConnectionStrings["ADOConn"].ToString();
            TruncateResultIDbyAlias(connStr);
            TruncateResultsInfoByResultID(connStr);
            string currentDate = DateTime.Now.ToString();
            string beforeCurrentDate = DateTime.Now.AddDays(-120).ToString();
            string[] aliases = ExecuteQuery(connStr, "select * from SulPhurReviewer", "alias");
            //            string[] sqls = new string[10];
            //            sqls[0] = @"insert into ResultIDByAlias (ResultID,alias)
            //SELECT newT.ResultID , '"+ aliases[0] + @"'as alias FROM (
            // SELECT ROW_NUMBER() OVER (ORDER BY [TOTAL].[BuildNo] DESC, [TOTAL].[ResultID] DESC) AS rowNum, [TOTAL].[ResultID], [TOTAL].[BuildNo], 
            // [TOTAL].[Language], [TOTAL].[RuleName], [TOTAL].[ResultType], [TOTAL].[UIName], [TOTAL].[UserName], [TOTAL].[OSType], [TOTAL].[DateUploaded], 
            // [TOTAL].[CreateDate], [TOTAL].[ReviewFlag] FROM (
            // SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [Rules].[RuleName], [UIContents].[UIName], [Clients].[UserName], 
            // [UIContents].[DateUploaded], [Results].[ResultType], [Results].[CreateDate], [Results].[ReviewFlag], [BuildInfo].[Language], [Clients].[OSType]
            // FROM [Results]
            // INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            // INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            // INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            // INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            // WHERE 1=1  AND [BuildInfo].[Language] IN ('CHS','CHT','CSY','DEU','JPN') AND [Results].[RuleID] IN (36) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"' AND [Results].[ResultType] IN ('Fail') AND [Results].[ReviewFlag]=0) AS [TOTAL]
            // INNER JOIN ( 
            // SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType] FROM [Results]
            // INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            // INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            // INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            // INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            // WHERE 1=1  AND [BuildInfo].[Language] IN ('CHS','CHT','CSY','DEU','JPN') AND [Results].[RuleID] IN (36) AND [CreateDate]> '" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"'
            // GROUP BY [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType]) AS [GroupTable]
            // ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
            // AND [TOTAL].[UIName]=[GroupTable].[UIName]
            // AND [TOTAL].[OSType]=[GroupTable].[OSType]
            // AND [TOTAL].[Language]=[GroupTable].[Language]) AS newT  ORDER BY rowNum";
            //            sqls[1] = @"insert into ResultIDByAlias (ResultID,alias)
            //SELECT newT.ResultID , '"+ aliases[1] + @"'as alias FROM (
            // SELECT ROW_NUMBER() OVER (ORDER BY [TOTAL].[BuildNo] DESC, [TOTAL].[ResultID] DESC) AS rowNum, [TOTAL].[ResultID], [TOTAL].[BuildNo], 
            // [TOTAL].[Language], [TOTAL].[RuleName], [TOTAL].[ResultType], [TOTAL].[UIName], [TOTAL].[UserName], [TOTAL].[OSType], [TOTAL].[DateUploaded], 
            // [TOTAL].[CreateDate], [TOTAL].[ReviewFlag] FROM (
            // SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [Rules].[RuleName], [UIContents].[UIName], [Clients].[UserName], 
            // [UIContents].[DateUploaded], [Results].[ResultType], [Results].[CreateDate], [Results].[ReviewFlag], [BuildInfo].[Language], [Clients].[OSType]
            // FROM [Results]
            // INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            // INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            // INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            // INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            // WHERE 1=1  AND [BuildInfo].[Language] IN ('ENU') AND [Results].[RuleID] IN (36) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"' AND [Results].[ResultType] IN ('Fail') AND [Results].[ReviewFlag]=0) AS [TOTAL]
            // INNER JOIN ( 
            // SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType] FROM [Results]
            // INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            // INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            // INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            // INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            // WHERE 1=1  AND [BuildInfo].[Language] IN ('ENU') AND [Results].[RuleID] IN (36) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"'
            // GROUP BY [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType]) AS [GroupTable]
            // ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
            // AND [TOTAL].[UIName]=[GroupTable].[UIName]
            // AND [TOTAL].[OSType]=[GroupTable].[OSType]
            // AND [TOTAL].[Language]=[GroupTable].[Language]) AS newT  ORDER BY rowNum";
            //            sqls[2] = @"insert into ResultIDByAlias (ResultID,alias)
            //SELECT newT.ResultID , '"+ aliases[2] + @"'as alias FROM (
            // SELECT ROW_NUMBER() OVER (ORDER BY [TOTAL].[BuildNo] DESC, [TOTAL].[ResultID] DESC) AS rowNum, [TOTAL].[ResultID], [TOTAL].[BuildNo], 
            // [TOTAL].[Language], [TOTAL].[RuleName], [TOTAL].[ResultType], [TOTAL].[UIName], [TOTAL].[UserName], [TOTAL].[OSType], [TOTAL].[DateUploaded], 
            // [TOTAL].[CreateDate], [TOTAL].[ReviewFlag] FROM (
            // SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [Rules].[RuleName], [UIContents].[UIName], [Clients].[UserName], 
            // [UIContents].[DateUploaded], [Results].[ResultType], [Results].[CreateDate], [Results].[ReviewFlag], [BuildInfo].[Language], [Clients].[OSType]
            // FROM [Results]
            // INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            // INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            // INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            // INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            // WHERE 1=1  AND [BuildInfo].[Language] IN ('ESN','FRA','HUN','ITA','KOR','NLD') AND [Results].[RuleID] IN (36) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"' AND [Results].[ResultType] IN ('Fail') AND [Results].[ReviewFlag]=0) AS [TOTAL]
            // INNER JOIN ( 
            // SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType] FROM [Results]
            // INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            // INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            // INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            // INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            // WHERE 1=1  AND [BuildInfo].[Language] IN ('ESN','FRA','HUN','ITA','KOR','NLD') AND [Results].[RuleID] IN (36) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"'
            // GROUP BY [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType]) AS [GroupTable]
            // ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
            // AND [TOTAL].[UIName]=[GroupTable].[UIName]
            // AND [TOTAL].[OSType]=[GroupTable].[OSType]
            // AND [TOTAL].[Language]=[GroupTable].[Language]) AS newT  ORDER BY rowNum";
            //            sqls[3] = @"insert into ResultIDByAlias (ResultID,alias)
            //SELECT newT.ResultID , '"+ aliases[3] + @"'as alias FROM (
            // SELECT ROW_NUMBER() OVER (ORDER BY [TOTAL].[BuildNo] DESC, [TOTAL].[ResultID] DESC) AS rowNum, [TOTAL].[ResultID], [TOTAL].[BuildNo], 
            // [TOTAL].[Language], [TOTAL].[RuleName], [TOTAL].[ResultType], [TOTAL].[UIName], [TOTAL].[UserName], [TOTAL].[OSType], [TOTAL].[DateUploaded], 
            // [TOTAL].[CreateDate], [TOTAL].[ReviewFlag] FROM (
            // SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [Rules].[RuleName], [UIContents].[UIName], [Clients].[UserName], 
            // [UIContents].[DateUploaded], [Results].[ResultType], [Results].[CreateDate], [Results].[ReviewFlag], [BuildInfo].[Language], [Clients].[OSType]
            // FROM [Results]
            // INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            // INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            // INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            // INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            // WHERE 1=1  AND [BuildInfo].[Language] IN ('PLK','PTB','PTG','RUS','SVE','TRK') AND [Results].[RuleID] IN (36) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"' AND [Results].[ResultType] IN ('Fail') AND [Results].[ReviewFlag]=0) AS [TOTAL]
            // INNER JOIN ( 
            // SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType] FROM [Results]
            // INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            // INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            // INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            // INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            // WHERE 1=1  AND [BuildInfo].[Language] IN ('PLK','PTB','PTG','RUS','SVE','TRK') AND [Results].[RuleID] IN (36) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"'
            // GROUP BY [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType]) AS [GroupTable]
            // ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
            // AND [TOTAL].[UIName]=[GroupTable].[UIName]
            // AND [TOTAL].[OSType]=[GroupTable].[OSType]
            // AND [TOTAL].[Language]=[GroupTable].[Language]) AS newT  ORDER BY rowNum";
            //            sqls[4] = @"insert into ResultIDByAlias (ResultID,alias)
            //SELECT newT.ResultID , '"+ aliases[4] + @"'as alias FROM (
            // SELECT ROW_NUMBER() OVER (ORDER BY [TOTAL].[BuildNo] DESC, [TOTAL].[ResultID] DESC) AS rowNum, [TOTAL].[ResultID], [TOTAL].[BuildNo], 
            // [TOTAL].[Language], [TOTAL].[RuleName], [TOTAL].[ResultType], [TOTAL].[UIName], [TOTAL].[UserName], [TOTAL].[OSType], [TOTAL].[DateUploaded], 
            // [TOTAL].[CreateDate], [TOTAL].[ReviewFlag] FROM (
            // SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [Rules].[RuleName], [UIContents].[UIName], [Clients].[UserName], 
            // [UIContents].[DateUploaded], [Results].[ResultType], [Results].[CreateDate], [Results].[ReviewFlag], [BuildInfo].[Language], [Clients].[OSType]
            // FROM [Results]
            // INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            // INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            // INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            // INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            // WHERE 1=1  AND [BuildInfo].[Language] IN ('CHS') AND [Results].[RuleID] IN (31) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"' AND [Results].[ResultType] IN ('Fail') AND [Results].[ReviewFlag]=0) AS [TOTAL]
            // INNER JOIN ( 
            // SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType] FROM [Results]
            // INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            // INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            // INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            // INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            // WHERE 1=1  AND [BuildInfo].[Language] IN ('CHS') AND [Results].[RuleID] IN (31) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"'
            // GROUP BY [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType]) AS [GroupTable]
            // ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
            // AND [TOTAL].[UIName]=[GroupTable].[UIName]
            // AND [TOTAL].[OSType]=[GroupTable].[OSType]
            // AND [TOTAL].[Language]=[GroupTable].[Language]) AS newT  ORDER BY rowNum";
            //            sqls[5] = @"insert into ResultIDByAlias (ResultID,alias)
            //SELECT newT.ResultID , '"+ aliases[5] + @"'as alias FROM (
            // SELECT ROW_NUMBER() OVER (ORDER BY [TOTAL].[BuildNo] DESC, [TOTAL].[ResultID] DESC) AS rowNum, [TOTAL].[ResultID], [TOTAL].[BuildNo], 
            // [TOTAL].[Language], [TOTAL].[RuleName], [TOTAL].[ResultType], [TOTAL].[UIName], [TOTAL].[UserName], [TOTAL].[OSType], [TOTAL].[DateUploaded], 
            // [TOTAL].[CreateDate], [TOTAL].[ReviewFlag] FROM (
            // SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [Rules].[RuleName], [UIContents].[UIName], [Clients].[UserName], 
            // [UIContents].[DateUploaded], [Results].[ResultType], [Results].[CreateDate], [Results].[ReviewFlag], [BuildInfo].[Language], [Clients].[OSType]
            // FROM [Results]
            // INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            // INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            // INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            // INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            // WHERE 1=1  AND [BuildInfo].[Language] IN ('CHT','CSY','DAN','DEU','ELL','ESN','FIN','FRA','HUN','ITA','JPN','KOR','NLD','NOR','PLK','PTB','PTG','RUS','SVE','TRK') AND [Results].[RuleID] IN (31) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"' AND [Results].[ResultType] IN ('Fail') AND [Results].[ReviewFlag]=0) AS [TOTAL]
            // INNER JOIN ( 
            // SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType] FROM [Results]
            // INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            // INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            // INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            // INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            // WHERE 1=1  AND [BuildInfo].[Language] IN ('CHT','CSY','DAN','DEU','ELL','ESN','FIN','FRA','HUN','ITA','JPN','KOR','NLD','NOR','PLK','PTB','PTG','RUS','SVE','TRK') AND [Results].[RuleID] IN (31) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"'
            // GROUP BY [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType]) AS [GroupTable]
            // ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
            // AND [TOTAL].[UIName]=[GroupTable].[UIName]
            // AND [TOTAL].[OSType]=[GroupTable].[OSType]
            // AND [TOTAL].[Language]=[GroupTable].[Language]) AS newT ORDER BY rowNum";
            //            sqls[6] = @"insert into ResultIDByAlias (ResultID,alias)
            //SELECT newT.ResultID , '"+ aliases[6] + @"'as alias FROM (
            // SELECT ROW_NUMBER() OVER (ORDER BY [TOTAL].[BuildNo] DESC, [TOTAL].[ResultID] DESC) AS rowNum, [TOTAL].[ResultID], [TOTAL].[BuildNo], 
            // [TOTAL].[Language], [TOTAL].[RuleName], [TOTAL].[ResultType], [TOTAL].[UIName], [TOTAL].[UserName], [TOTAL].[OSType], [TOTAL].[DateUploaded], 
            // [TOTAL].[CreateDate], [TOTAL].[ReviewFlag] FROM (
            // SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [Rules].[RuleName], [UIContents].[UIName], [Clients].[UserName], 
            // [UIContents].[DateUploaded], [Results].[ResultType], [Results].[CreateDate], [Results].[ReviewFlag], [BuildInfo].[Language], [Clients].[OSType]
            // FROM [Results]
            // INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            // INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            // INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            // INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            // WHERE 1=1  AND [BuildInfo].[Language] IN ('ENU') AND [Results].[RuleID] IN (31) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"' AND [Results].[ResultType] IN ('Fail') AND [Results].[ReviewFlag]=0) AS [TOTAL]
            // INNER JOIN ( 
            // SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType] FROM [Results]
            // INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            // INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            // INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            // INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            // WHERE 1=1  AND [BuildInfo].[Language] IN ('ENU') AND [Results].[RuleID] IN (31) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"'
            // GROUP BY [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType]) AS [GroupTable]
            // ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
            // AND [TOTAL].[UIName]=[GroupTable].[UIName]
            // AND [TOTAL].[OSType]=[GroupTable].[OSType]
            // AND [TOTAL].[Language]=[GroupTable].[Language]) AS newT  ORDER BY rowNum";
            //            sqls[7] = @"insert into ResultIDByAlias (ResultID,alias)
            //SELECT newT.ResultID , '"+ aliases[7] + @"'as alias FROM (
            // SELECT ROW_NUMBER() OVER (ORDER BY [TOTAL].[BuildNo] DESC, [TOTAL].[ResultID] DESC) AS rowNum, [TOTAL].[ResultID], [TOTAL].[BuildNo], 
            // [TOTAL].[Language], [TOTAL].[RuleName], [TOTAL].[ResultType], [TOTAL].[UIName], [TOTAL].[UserName], [TOTAL].[OSType], [TOTAL].[DateUploaded], 
            // [TOTAL].[CreateDate], [TOTAL].[ReviewFlag] FROM (
            // SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [Rules].[RuleName], [UIContents].[UIName], [Clients].[UserName], 
            // [UIContents].[DateUploaded], [Results].[ResultType], [Results].[CreateDate], [Results].[ReviewFlag], [BuildInfo].[Language], [Clients].[OSType]
            // FROM [Results]
            // INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            // INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            // INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            // INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            // WHERE 1=1  AND [BuildInfo].[Language] IN ('ENU') AND [Results].[RuleID] IN (39) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"' AND [Results].[ResultType] IN ('Fail') AND [Results].[ReviewFlag]=0) AS [TOTAL]
            // INNER JOIN ( 
            // SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName]
            // FROM [Results]
            // INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            // INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            // INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            // INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            // WHERE 1=1  AND [BuildInfo].[Language] IN ('ENU') AND [Results].[RuleID] IN (39) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"'
            // GROUP BY [UIContents].[UIName]) AS [GroupTable]
            // ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
            // AND [TOTAL].[UIName]=[GroupTable].[UIName]) AS newT ORDER BY rowNum";
            //            sqls[8] = @"insert into ResultIDByAlias (ResultID,alias)
            //SELECT newT.ResultID , '"+ aliases[8] + @"'as alias FROM (
            // SELECT ROW_NUMBER() OVER (ORDER BY [TOTAL].[BuildNo] DESC, [TOTAL].[ResultID] DESC) AS rowNum, [TOTAL].[ResultID], [TOTAL].[BuildNo], 
            // [TOTAL].[Language], [TOTAL].[RuleName], [TOTAL].[ResultType], [TOTAL].[UIName], [TOTAL].[UserName], [TOTAL].[OSType], [TOTAL].[DateUploaded], 
            // [TOTAL].[CreateDate], [TOTAL].[ReviewFlag] FROM (
            // SELECT DISTINCT [BuildInfo].[BuildNo], [Results].[ResultID], [Rules].[RuleName], [UIContents].[UIName], [Clients].[UserName], 
            // [UIContents].[DateUploaded], [Results].[ResultType], [Results].[CreateDate], [Results].[ReviewFlag], [BuildInfo].[Language], [Clients].[OSType]
            // FROM [Results]
            // INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            // INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            // INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            // INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            // WHERE 1=1  AND [BuildInfo].[Language] IN ('ENU') AND [Results].[RuleID] IN (41) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"' AND [Results].[ResultType] IN ('Fail') AND [Results].[ReviewFlag]=0) AS [TOTAL]
            // INNER JOIN ( 
            // SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType] FROM [Results]
            // INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
            // INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
            // INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
            // INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
            // WHERE 1=1  AND [BuildInfo].[Language] IN ('ENU') AND [Results].[RuleID] IN (41) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"'
            // GROUP BY [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType]) AS [GroupTable]    
            // ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
            // AND [TOTAL].[UIName]=[GroupTable].[UIName]
            // AND [TOTAL].[OSType]=[GroupTable].[OSType]
            // AND [TOTAL].[Language]=[GroupTable].[Language]) AS newT ORDER BY rowNum";
            //            sqls[9] = "";

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
 WHERE 1=1  AND [BuildInfo].[Language] IN ('ENU') AND [Results].[RuleID] IN (41) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"' AND [Results].[ResultType] IN ('Fail') AND [Results].[ReviewFlag]=0) AS [TOTAL]
 INNER JOIN ( 
 SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType] FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 WHERE 1=1  AND [BuildInfo].[Language] IN ('ENU') AND [Results].[RuleID] IN (41) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"'
 GROUP BY [UIContents].[UIName], [BuildInfo].[Language], [Clients].[OSType]) AS [GroupTable]    
 ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
 AND [TOTAL].[UIName]=[GroupTable].[UIName]
 AND [TOTAL].[OSType]=[GroupTable].[OSType]
 AND [TOTAL].[Language]=[GroupTable].[Language]) AS newT ORDER BY rowNum";
            insertSqls[9] = @"INSERT INTO ResultsInfoByAlias (ResultID, buildNo, Language, RuleName, ResultType, UIName, UserName, OSType, DateUploaded, CreateDate, ReviewFlag, alias) SELECT ResultID, buildNo, Language, RuleName, ResultType, UIName, UserName, OSType, DateUploaded, CreateDate, ReviewFlag,'" + aliases[9] + @" as alias FROM (
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
 WHERE 1=1  AND [BuildInfo].[Language] IN ('ENU') AND [Results].[RuleID] IN (48) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"' AND [Results].[ResultType] IN ('Fail') AND [Results].[ReviewFlag]=0) AS [TOTAL]
 INNER JOIN ( 
 SELECT DISTINCT MAX([BuildInfo].[BuildNo]) AS [BuildNo], [UIContents].[UIName]
 FROM [Results]
 INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID] 
 INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID] 
 INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID] 
 INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID]
 WHERE 1=1  AND [BuildInfo].[Language] IN ('ENU') AND [Results].[RuleID] IN (48) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"'
 GROUP BY [UIContents].[UIName]) AS [GroupTable]
 ON [TOTAL].[BuildNo]=[GroupTable].[BuildNo] 
 AND [TOTAL].[UIName]=[GroupTable].[UIName]) AS newT  ORDER BY rowNum";
            string sql = "insert into ResultIDbyAlias (ResultID,alias) select ResultID,alias from ResultsInfobyAlias";



            for (int i = 0; i < 10; i++)
            {
                ExecuteScalar(connStr, insertSqls[i]);
                Console.WriteLine(i);
            }
            ExecuteScalar(connStr, sql);
            string[] resultIDs = null;
            Console.WriteLine("complete! press any key to continue");






            string folderPath = @"F:\v-mogenzhang\Deploy\SulpHurReport\Tmp_ResultImage";
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
            string sql1 = "select ResultID from Results where reviewflag =1 and resultID in (" + clause + ")";
            readyToDelete = ExecuteQuery(connStr, sql1, "ResultID");
            foreach (string s in readyToDelete)
            {
                File.Delete(folderPath + '\\' + s + ".jpg");
            }
            resultIDs = ExecuteQuery(connStr, "select ResultID from ResultsInfoByAlias", "ResultID");
            foreach (string resultID in resultIDs)
            {
                GetScreenshotPath(int.Parse(resultID));
            }
        }

        public static string GetScreenshotPath(int resultID)
        {
            string imagePath = string.Format(@"Tmp_ResultImage/{0}.jpg", resultID);
            string localImagePath = Path.Combine(@"F:\v-mogenzhang\Deploy\SulpHurReport\", imagePath);
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

        public static string GetScreenshotPath1(int resultID)
        {
            string imagePath = string.Format(@"Tmp_ResultImage/{0}.jpg", resultID);
            string localImagePath = Path.Combine(@"F:\v-mogenzhang\Deploy\SulpHurReport\Tmp_ResultImage", imagePath);

            if (!File.Exists(localImagePath))
            {
                byte[] resultImage = GetResultScreenshot(resultID);
                System.Drawing.Image image = null;

                if (resultImage == null)
                {
                    resultImage = GetUIScreenshot(resultID);
                    if (resultImage != null)
                    {
                        using (var ms = new MemoryStream(resultImage))
                        {
                            image = Bitmap.FromStream(ms);
                        }
                    }
                }
                else
                {
                    using (var ms = new MemoryStream(resultImage))
                    {
                        image = Bitmap.FromStream(ms);
                    }
                }

                if (image != null)
                {
                    try
                    {
                        image.Save(localImagePath, ImageFormat.Jpeg);
                    }
                    catch (Exception ex)
                    {
                        // 打印错误信息
                        Console.WriteLine(ex.ToString());
                    }
                    finally
                    {
                        image.Dispose();
                    }
                }
            }

            return imagePath;
        }


        public static byte[] GetResultScreenshot(int resultID)
        {
            try
            {
                ZMogenDebugSulpHurEntities entities = new ZMogenDebugSulpHurEntities();
                var item = from r in entities.Results
                           where r.ResultID == resultID
                           select r.ResultImage;
                //byte[] resultImage = item.FirstOrDefault();
                byte[] resultImage = item.FirstOrDefault();
                return resultImage;
            }
            catch (MappingException)
            {
                return GetResultScreenshot(resultID);
            }
        }

        public static Byte[] GetUIScreenshot(int resultID)
        {
            ZMogenDebugSulpHurEntities entities = new ZMogenDebugSulpHurEntities();
            var list = from r in entities.Results
                       join u in entities.UIContents on r.ContentID equals u.ContentID
                       where r.ResultID == resultID
                       select u.UIScreenShot;
            Byte[] uiScreenshot = list.FirstOrDefault()?.ToArray();
            return uiScreenshot;
        }

        public static string[] ExecuteQuery(string connStr, string sql, string columnName)
        {
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

        public static void testThread()
        {
            while (true)
            {
                if (DateTime.Now.Second >= 30)
                {
                    Console.WriteLine("--------------------------------------------------------");
                    Thread.Sleep(1000);
                }
                else
                {
                    Console.WriteLine("in test Thread while");
                    Thread.Sleep(1000);
                }
            }
        }

        public static void testThread2()
        {
            while(true)
            {
                if(DateTime.Now.Minute >= 40 )
                {
                    Console.WriteLine("**************************************************");
                    Thread.Sleep(1000);
                }
                else
                {
                    Console.WriteLine("in test Thread 2 else");
                    Thread.Sleep(1000);
                }
            }
        }

        static void Main(string[] args)
        {
            Thread t1 = new Thread(testThread);
            Thread t2 = new Thread(testThread2);
            t1.Start();
            t2.Start();


            //            string currentDate = DateTime.Now.AddHours(15).ToString();
            //            string beforeCurrentDate = DateTime.Now.AddDays(-120).ToString();
            //            string ssssss= @"INSERT INTO ResultsInfoByAlias (ResultID, buildNo, Language, RuleName, ResultType, UIName, UserName, OSType, DateUploaded, CreateDate, ReviewFlag, alias) 
            //SELECT ResultID, buildNo, Language, RuleName, ResultType, UIName, UserName, OSType, DateUploaded, CreateDate, ReviewFlag,'" + "v-danielxu" + @"' as alias  FROM (
            // SELECT ROW_NUMBER() OVER (ORDER BY [BuildNo] DESC, [ResultID] DESC) AS rowNum, * FROM (
            // SELECT distinct [Results].[ResultID], [BuildInfo].[BuildNo], [BuildInfo].[Language], [Rules].[RuleName], [Results].[ResultType], 
            // [UIContents].[UIName], [Clients].[UserName], [BuildInfo].[BuildID], [Clients].[OSType], [UIContents].[DateUploaded], [Results].[CreateDate], 
            // [Results].[ReviewFlag] FROM [dbo].[Results] 
            // INNER JOIN [UIContents] ON [Results].[ContentID] = [UIContents].[ContentID]
            // INNER JOIN [BuildInfo] ON [UIContents].[BuildID] = [BuildInfo].[BuildID]
            // INNER JOIN [Rules] ON [Results].[RuleID] = [Rules].[RuleID]
            // INNER JOIN [Clients] ON [UIContents].[ClientID] = [Clients].[ClientID] 
            // LEFT OUTER JOIN [AssemblyLink] ON [AssemblyLink].[ContentID]=[UIContents].[ContentID]
            // LEFT OUTER JOIN [AssemblyInfo] ON [AssemblyInfo].[TypeID]=[AssemblyLink].[TypeID]
            // WHERE 1=1 AND [Results].[RuleID] IN (41) AND [CreateDate]>'" + beforeCurrentDate + @"' AND [CreateDate]<'" + currentDate + @"' AND [Results].[ResultType] IN ('Fail') AND [Results].[ReviewFlag]=0) AS newT) AS t ORDER BY rowNum";










            //            #region
            //            //UIContentVerification uIContentVerification = new UIContentVerification();
            //            //uIContentVerification.Start();



            //            //CacheResultsInfo cacheResultsInfo = new CacheResultsInfo();
            //            //cacheResultsInfo.TimingCacheResultInfo();



            //            //GetScreenshotPath(51336597);
            //            //   CacheResultInfo();

            //            //TimingCacheResultInfo();
            //            //VerifyThread












            //            //string folderPath = @"F:\SD\ConfigMgr-Test\src\Tools\SulpHur20\SulpHur\SulpHurReport\Tmp_ResultImage";
            //            //string[] files = Directory.GetFiles(folderPath);
            //            //string clause = string.Empty;
            //            //string[] readyToDelete;
            //            //foreach (string file in files)
            //            //{
            //            //    string fileName = Path.GetFileName(file);
            //            //    string ResultID = fileName.Substring(0, fileName.IndexOf("."));
            //            //    clause += ResultID + ",";
            //            //}
            //            //clause =  clause.TrimEnd(',');
            //            //Console.WriteLine(clause);
            //            //string sql = "select ResultID from Results where reviewflag =1 and resultID in (" + clause + ")";
            //            //readyToDelete = ExecuteQuery(ConfigurationManager.ConnectionStrings["ADOConn"].ToString(), sql, "ResultID");
            //            //foreach (string s in readyToDelete)
            //            //{               
            //            //    File.Delete(folderPath+'\\'+s+".jpg");
            //            //}




            //            //CacheResultInfo();
            //            //Console.ReadKey();           


            //            // string[] aliases = ExecuteQuery(ConfigurationManager.ConnectionStrings["ADOConn"].ToString(), "select * from SulPhurReviewer", "alias");
            //            //foreach(string result in resultList)
            //            //{
            //            //    Console.WriteLine($"{result}");
            //            //}


            //            // GetResultID();


            //            //    string r = "Reviewed";

            //            //    string r2 = GenerateReviewedTypeFilterClause(r);


            //            //    int resultID = 0;
            //            //    int[] resultIDs = { 57164977 };
            //            //    do
            //            //    {
            //            //        Console.WriteLine();
            //            //        Console.WriteLine("----------------------------------------------------------------------");           
            //            //        foreach (int i in resultIDs)
            //            //        {
            //            //            resultID = i;
            //            //            testMethod.spellRule(resultID);
            //            //            Console.WriteLine("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^");
            //            //        }
            //            //        Console.WriteLine("press any key to continue: ");
            //            //        Console.ReadKey();
            //            //    } while (resultID != 0);
            //            #endregion

            //            //DBOperator dBOperator = new DBOperator();
            //            //dBOperator.QueryAllDirtyUI();
            //            int contentID = 3242422;
            //            UIContentVerification uv = new UIContentVerification();

            //            RuleManager ruleManager = new RuleManager();
            //            ruleManager.LoadRules();

            //            ZMogenDebugSulpHurEntities entity = new ZMogenDebugSulpHurEntities();
            //            UIContent uiContent= entity.UIContents.Where(u => u.ContentID == contentID).FirstOrDefault();
            //            if(uiContent != null)
            //            {
            //                MS.Internal.SulpHur.UICompliance.ElementInformation root = MS.Internal.SulpHur.Utilities.ExtensionMethods.FromString<MS.Internal.SulpHur.UICompliance.ElementInformation>(uiContent.UIContent1);
            //                List<MS.Internal.SulpHur.UICompliance.ElementInformation> list = uv.ParseTreeToList(root);

            //                foreach(UIComplianceRuleBase rule in RuleManager.RuleList )
            //                {
            //                    if (rule.IsEnabled)
            //                    {
            //                        rule.UIVerify(list);
            //                        Console.WriteLine("Finished-" + rule.Name);

            //                    }
            //                }
            //            }

            Console.WriteLine("Finished");
          //  Console.ReadLine();
        }



    }
}



