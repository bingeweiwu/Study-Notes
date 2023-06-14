using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Text.RegularExpressions;
using MS.Internal.SulpHur.Utilities;
using SulpHurManagementSystem.Models;

namespace SulpHurManagementSystem
{
    public partial class ShowTestCoverage : System.Web.UI.Page
    {
        private string serverName = ConfigurationManager.AppSettings["ServerName"];
        private string[] UIDiffHiddenRuleArray;
        private string[] UIDiffRulesForENUArray;
        private SulpHurEntities sulphurEntities = new SulpHurEntities();
        public string assembly = string.Empty;
        public string fulltype = string.Empty;
        public string beforebuild = string.Empty;
        public string afterbuild = string.Empty;
        protected void Page_Load(object sender, EventArgs e)
        {
            string UIDiffHiddenRules= ConfigurationManager.AppSettings["UIDiffHiddenRules"];
            if (!string.IsNullOrEmpty(UIDiffHiddenRules))
                UIDiffHiddenRuleArray = UIDiffHiddenRules.Split('|');
            string UIDiffRulesForENU = ConfigurationManager.AppSettings["UIDiffRulesForENU"];
            if (!string.IsNullOrEmpty(UIDiffRulesForENU))
                UIDiffRulesForENUArray = UIDiffRulesForENU.Split('|');

            string info = Request.QueryString["info"];
            assembly = info.Split('_')[0];
            fulltype = info.Split('_')[1];
            HttpCookie cookie = Request.Cookies["buildinfo"];
            if (cookie != null && cookie["postbuild"].ToString() != "")
            {
                afterbuild = cookie["postbuild"].ToString();
            }
            if (txtbuild.Value == "")
            {
                txtbuild.Value = afterbuild;
            }
        }

        protected void GetTestCoverageReport_Click(object sender, EventArgs e)
        {
            string buildNo = string.Format("5.0.{0}.1000", txtbuild.Value);
            this.RefreshTestCoverageTable(assembly, fulltype, buildNo);
        }
        public string GetTestCoverageResult(string assemblyname, string fulltypename, string languages, string rules, string buildNo)
        {
            try
            {
                string text = "Rule";
                string[] rulearray = rules.Split('|');
                string[] languagearray = languages.Split(',');
                foreach (string lan in languagearray)
                {
                    text += "|" + lan;
                }
                text += "|";
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
                        int resultID = 0;
                        this.GetResultTypeByInfo(rulearray[i], assemblyname, fulltypename, out latestbuildNo, buildLanguage, out resultType, out resultID);
                        int inputbuild = int.Parse(buildNo);
                        if (latestbuildNo == "0")
                        {
                            text += "|NotRun";
                        }
                        else
                        {
                            bool isStandardBuildFormat = CMUtility.IsStandardFormat(latestbuildNo);
                            if (isStandardBuildFormat)
                            {
                                int latestbuild = int.Parse(latestbuildNo.Substring(4, 4));
                                if (inputbuild <= latestbuild)
                                {
                                    if (resultType == "Pass") text = text + "|[P]:" + latestbuild;
                                    if (resultType == "Warning") text = text + "|[W]:" + latestbuild;
                                    if (resultType == "Fail") text = text + "|[F]:" + latestbuild;
                                    if (IsResultReviewed(resultID))
                                    {
                                        text += "[R]";
                                    }
                                    else
                                    {
                                        text += "[U]";
                                    }
                                }
                                else
                                {
                                    text += "|NotRun";
                                }
                            }
                            else
                            {
                                if (resultType == "Pass") text = text + "|[P]:" + latestbuildNo;
                                if (resultType == "Warning") text = text + "|[W]:" + latestbuildNo;
                                if (resultType == "Fail") text = text + "|[F]:" + latestbuildNo;
                                if (IsResultReviewed(resultID))
                                {
                                    text += "[R]";
                                }
                                else
                                {
                                    text += "[U]";
                                }
                            }
                        }
                    }
                    text += "|";
                }
                return text;
            }
            catch (Exception ex)
            {
                return "[E]:" + ex.Message;
            }
        }
        public void GetResultTypeByInfo(string rule, string assembly, string fulltypename, out string LatestbuildNo, string buildLanguage, out string resultType, out int resultID)
        {
            SulpHurEntities entities = new SulpHurEntities();
            var list = entities.Results
                .Where(row => row.Rule.RuleName == rule)
                .Join(entities.Rules, re => re.RuleID, ru => ru.RuleID, (re, ru) => new { re.RuleID, re.ContentID, ru.RuleName, re.ResultType, re.ResultID, re.ReviewFlag })
                .Join(entities.AssemblyLinks, c => c.ContentID, al => al.ContentID, (c, al) => new { c.ContentID, al.IsPageIdentifier, al.TypeID, c.ResultType, c.ResultID, c.ReviewFlag })
                .Where(item => item.IsPageIdentifier == true)
                .Join(entities.AssemblyInfoes, al => al.TypeID, ai => ai.TypeID, (al, ai) => new { al.ContentID, al.ResultType, ai.AssemblyName, ai.FullTypeName, al.ResultID, al.ReviewFlag })
                .Where(row => row.FullTypeName == fulltypename && row.AssemblyName == assembly)
                .Join(entities.UIContents, re => re.ContentID, c => c.ContentID, (re, c) => new { re.ContentID, c.BuildID, re.ResultType, re.ResultID, re.ReviewFlag })
                .Join(entities.BuildInfoes, c => c.BuildID, b => b.BuildID, (c, b) => new { b.BuildNo, b.Language, c.ResultType, c.ResultID, c.ReviewFlag })
                .Where(row => row.Language == buildLanguage)
                .Distinct()
                .OrderByDescending(row => row.BuildNo);
            var result = list.FirstOrDefault();
            if (result == null)
            {
                resultID = 0;
                LatestbuildNo = "0";
                resultType = "";
            }
            else
            {
                var faillist = list
                    .Where(row => row.BuildNo == result.BuildNo && row.ResultType != "Pass")
                    .OrderBy(row => row.ReviewFlag)
                    .ThenBy(row => row.ResultType);
                var failresult = faillist.FirstOrDefault();
                if (failresult != null)
                {
                    //list = faillist
                    //    .Where(row => row.ReviewFlag == failresult.ReviewFlag)
                    //    .OrderBy(row => row.ResultType);
                    //result = list.FirstOrDefault();
                    result = failresult;
                }
                else
                {
                    list = list
                    .Where(row => row.BuildNo == result.BuildNo && row.ResultType == "Pass")
                    .OrderBy(row => row.ReviewFlag);
                    result = list.FirstOrDefault();
                }
                resultID = result.ResultID;
                LatestbuildNo = result.BuildNo;
                resultType = result.ResultType;
            }
        }
        private bool IsResultReviewed(int resultID)
        {
            if (resultID == 0)
            {
                return false;
            }
            else
            {
                SulpHurEntities entities = new SulpHurEntities();
                //get reviewstatus
                var list = entities.Results
                    .Where(row => row.ResultID == resultID);
                var li = list.FirstOrDefault();
                if (li.ReviewFlag == 1) return true;
                else return false;
            }
        }

        private void RefreshTestCoverageTable(string assemblyName, string fulltypename, string buildNo)
        {
            List<Rule> rules = sulphurEntities.Rules.Where(r => r.IsEnabled).ToList();
            Rule rule = null;
            if (UIDiffHiddenRuleArray != null && UIDiffHiddenRuleArray.Length > 0)
            {
                foreach (string UIDiffHiddenRuleName in UIDiffHiddenRuleArray)
                {
                    rule = rules.First(item => item.RuleName == UIDiffHiddenRuleName);
                    if (rule != null)
                        rules.Remove(rule);
                }
            }
            List<Language> languages = sulphurEntities.Languages.Where(l => !l.OnlyClient).ToList();
            Dictionary<string, Dictionary<string, LatestResultStatus>> LatestResultStatusDic = this.GetLatestResultStatusList(this.assembly, this.fulltype, rules, languages, buildNo);

            TestCoverageTable.BackColor = System.Drawing.Color.White;
            TestCoverageTable.BorderColor = System.Drawing.Color.Black;
            TestCoverageTable.BorderWidth = 1;
            int columns = languages.Count() + 1;
            string shortBuildVersion = string.Empty, reviewStatusString = string.Empty, colorString = string.Empty, resultTypeParameterString = string.Empty, reviewStatusParameterString = string.Empty;
            HyperLink link = null;
            TableRow tr = null;
            TableCell tc = null;
            int reviewStatusInt = -1;
            for (int i = -1; i < rules.Count(); i++)
            {
                tr = new TableRow();
                TestCoverageTable.Rows.Add(tr);
                for (int j = -1; j < languages.Count(); j++)
                {
                    tc = new TableCell();
                    if (i == -1)
                    {
                        if (j == -1)
                            tc.Text = "Rule";
                        else
                            tc.Text = languages[j].ThreeLetterWindowsLanguageName;
                    }
                    else
                    {
                        if (j == -1)
                            tc.Text = rules[i].RuleName;
                        else
                        {
                            if (!UIDiffRulesForENUArray.Contains(rules[i].RuleName) || languages[j].ThreeLetterWindowsLanguageName == "ENU")
                            {
                                if (LatestResultStatusDic.ContainsKey(rules[i].RuleName) && LatestResultStatusDic[rules[i].RuleName].ContainsKey(languages[j].ThreeLetterWindowsLanguageName))
                                {
                                    shortBuildVersion = LatestResultStatusDic[rules[i].RuleName][languages[j].ThreeLetterWindowsLanguageName].LatestBuildNo.Substring(4, 4);
                                    if (LatestResultStatusDic[rules[i].RuleName][languages[j].ThreeLetterWindowsLanguageName].Reviewed)
                                    {
                                        reviewStatusString = "[R]";
                                        reviewStatusInt = 1;
                                        colorString = "Green";
                                    }
                                    else
                                    {
                                        reviewStatusString = "[U]";
                                        reviewStatusInt = 0;
                                        if (LatestResultStatusDic[rules[i].RuleName][languages[j].ThreeLetterWindowsLanguageName].ResultType == "Fail")
                                            colorString = "Red";
                                        else if (LatestResultStatusDic[rules[i].RuleName][languages[j].ThreeLetterWindowsLanguageName].ResultType == "Warning")
                                            colorString = "Gold";
                                        else
                                            colorString = "Green";
                                    }

                                    link = new HyperLink();
                                    string hyperlink = string.Format("http://{0}/SulpHurReports/CapturedUIReport.aspx?buildlanguage={1}&rule={2}&assembly={3}&fulltypename={4}&resulttype={5}&reviewstatus={6}&showlatest=buildno",
                                        this.serverName,
                                        languages[j].ThreeLetterWindowsLanguageName,
                                        rules[i].RuleName,
                                        assembly,
                                        fulltype,
                                        LatestResultStatusDic[rules[i].RuleName][languages[j].ThreeLetterWindowsLanguageName].ResultType,
                                        reviewStatusInt);
                                    link.NavigateUrl = hyperlink;
                                    link.Target = "_blank";
                                    link.Text = shortBuildVersion + reviewStatusString;
                                    link.Style.Add(HtmlTextWriterStyle.Color, colorString);
                                    tc.Controls.Add(link);
                                }
                                else
                                {
                                    tc.Text = "NotRun";
                                    tc.Style.Add(HtmlTextWriterStyle.Color, "Grey");
                                }
                            }
                        }
                    }
                    tr.Cells.Add(tc);
                }
            }
        }
        public Dictionary<string,Dictionary<string,LatestResultStatus>> GetLatestResultStatusList(string assembly, string fulltypename, List<Rule> rules, List<Language> languages, string buildNo)
        {
            Dictionary<string, Dictionary<string, LatestResultStatus>> LatestResultStatusDic = new Dictionary<string, Dictionary<string, LatestResultStatus>>();
            LatestResultStatus latestResultStatus = null;
            foreach (Rule r in rules)
            {
                foreach(Language l in languages)
                {
                    if (UIDiffRulesForENUArray.Contains(r.RuleName) && l.ThreeLetterWindowsLanguageName != "ENU")
                        continue;

                    var list = sulphurEntities.Results
                        .Where(row => row.RuleID == r.RuleID)
                        .Join(sulphurEntities.UIContents, row => row.ContentID, c => c.ContentID, (row, c) => new { row.ContentID, row.ResultID, row.ResultType, row.ReviewFlag, c.BuildID })
                        .Join(sulphurEntities.BuildInfoes, row => row.BuildID, b => b.BuildID, (row, b) => new { row.ContentID, row.ResultID, row.ResultType, row.ReviewFlag, b.BuildNo, b.Language })
                        .Where(row => row.Language == l.ThreeLetterWindowsLanguageName && row.BuildNo.CompareTo(buildNo) >= 0)
                        .Join(sulphurEntities.AssemblyLinks, row => row.ContentID, al => al.ContentID, (row, al) => new { row.ResultID, row.ResultType, row.ReviewFlag, row.BuildNo, al.TypeID })
                        .Join(sulphurEntities.AssemblyInfoes, row => row.TypeID, ai => ai.TypeID, (row, ai) => new { row.ResultID, row.ResultType, row.ReviewFlag, row.BuildNo, ai.AssemblyName, ai.FullTypeName })
                        .Where(row => row.AssemblyName == assembly && row.FullTypeName == fulltypename)
                        .Select(row => new { row.ResultID, row.ResultType, row.ReviewFlag, row.BuildNo })
                        .Distinct()
                        .OrderByDescending(row => row.BuildNo);
                    var result = list.FirstOrDefault();
                    if (result != null)
                    {
                        var failOrWarningList = list
                            .Where(row => row.BuildNo == result.BuildNo && row.ResultType != "Pass")
                            .OrderBy(row => row.ReviewFlag);
                        var failOrWarningItem = failOrWarningList.FirstOrDefault();
                        if (failOrWarningItem == null)
                        {
                            list = list
                                .Where(row => row.BuildNo == result.BuildNo && row.ResultType == "Pass")
                                .OrderBy(row => row.ReviewFlag);
                            result= list.FirstOrDefault();
                        }
                        else
                            result = failOrWarningItem;
                        latestResultStatus = new LatestResultStatus() 
                        {
                            RuleName = r.RuleName, 
                            Language = l.ThreeLetterWindowsLanguageName, 
                            LatestBuildNo = result.BuildNo, 
                            ResultType = result.ResultType, 
                            Reviewed = result.ReviewFlag.Value == 1, 
                            ResultID = result.ResultID 
                        };
                        if (LatestResultStatusDic.ContainsKey(r.RuleName))
                            LatestResultStatusDic[r.RuleName].Add(l.ThreeLetterWindowsLanguageName, latestResultStatus);
                        else
                            LatestResultStatusDic.Add(r.RuleName, new Dictionary<string, LatestResultStatus>() { { l.ThreeLetterWindowsLanguageName, latestResultStatus } });
                    }
                }
            }

            return LatestResultStatusDic;
        }        
    }
}