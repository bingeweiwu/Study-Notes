using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Text.RegularExpressions;

namespace SulpHurManagementSystem
{
    public partial class ShowTestCoverage : System.Web.UI.Page
    {
        private string serverName = ConfigurationManager.AppSettings["ServerName"];
        public string assembly = string.Empty;
        public string fulltype = string.Empty;
        public string beforebuild = string.Empty;
        public string afterbuild = string.Empty;
        protected void Page_Load(object sender, EventArgs e)
        {
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
            string languages = System.Configuration.ConfigurationManager.AppSettings["AllLan"].ToString();
            string basicRules = System.Configuration.ConfigurationManager.AppSettings["BasicRules"].ToString();
            int rulesCount = basicRules.ToCharArray().Count(x => x == '|') + 1;
            int languagesCount = languages.ToCharArray().Count(x => x == ',') + 1;
            string result = GetTestCoverageResult(assembly, fulltype, languages, basicRules, txtbuild.Value);
            string[] resultArray = result.Split('|');
            TestCoverageTable.BackColor = System.Drawing.Color.White;
            TestCoverageTable.BorderColor = System.Drawing.Color.Black;
            TestCoverageTable.BorderWidth = 1;
            int rows = rulesCount + 1;
            int columns = languagesCount + 1;
            for (int i = 0; i < rows; i++)
            {
                TableRow tr = new TableRow();
                TestCoverageTable.Rows.Add(tr);
                for (int j = 0; j < columns; j++)
                {
                    HyperLink link = new HyperLink();
                    string hyperlink = string.Format("http://{0}/SulpHurReports/CapturedUIReport.aspx?buildlanguage={1}&rule={2}&assembly={3}&fulltypename={4}&showlatest=buildno",
                        this.serverName, resultArray[j], resultArray[i * columns], assembly, fulltype);
                    link.NavigateUrl = hyperlink;
                    link.Target = "_blank";
                    TableCell tc = new TableCell();
                    link.Text = resultArray[i * columns + j];
                    if (link.Text.Contains("[P]"))
                    {
                        link.Style.Add(HtmlTextWriterStyle.Color, "Green");
                        link.Text = link.Text.Remove(0, 4);
                        tc.Controls.Add(link);
                    }
                    else if (link.Text.Contains("[W]"))
                    {
                        link.Style.Add(HtmlTextWriterStyle.Color, "Gold");
                        link.Text = link.Text.Remove(0, 4);
                        tc.Controls.Add(link);
                    }
                    else if (link.Text.Contains("[F]"))
                    {
                        link.Style.Add(HtmlTextWriterStyle.Color, "Red");
                        link.Text = link.Text.Remove(0, 4);
                        tc.Controls.Add(link);
                    }
                    else if (link.Text.Contains("NotRun"))
                    {
                        tc.Text = link.Text;
                        tc.Style.Add(HtmlTextWriterStyle.Color, "Grey");
                    }
                    else
                    {
                        tc.Text = link.Text;
                        tc.Style.Add(HtmlTextWriterStyle.BorderWidth, "1");
                    }
                    tr.Cells.Add(tc);
                }
            }
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
                            bool isStandardBuildFormat = this.IsStandardFormat(latestbuildNo);
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
        private void GetResultTypeByInfo(string rule, string assembly, string fulltypename, out string LatestbuildNo, string buildLanguage, out string resultType, out int resultID)
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

        // Standard format: 5.0.8847.1000; ignore the last minor version
        public bool IsStandardFormat(string buildString)
        {
            string standardFormat = @"^5.0.[0-9]{4}.[0-9]{3}[0-9]+$";
            return Regex.IsMatch(buildString, standardFormat);
        }
    }
}