using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Xml;

namespace SulpHurManagementSystem
{
    public partial class ShowAllTestResults : System.Web.UI.Page
    {
        string serverName = ConfigurationManager.AppSettings["ServerName"];

        protected void Page_Load(object sender, EventArgs e)
        {
            form1.Target = "_Result";
        }

        protected void ShowAllTestResults_Click(object sender, EventArgs e)
        {
            string url = string.Empty;
            HttpCookie cookie = Request.Cookies["buildinfo"];
            if (cookie != null && cookie["url"].ToString() != "")
            {
                url = cookie["url"].ToString();
            }
            url = url.Substring(0, url.LastIndexOf('\\')) + "\\PageNav.xml";
            string xmlpath = updateStateOfXml(url);
            string redirectLink = string.Format("http://{0}/SulpHurReports/CMIntlTestResults.aspx?url={1}", serverName, xmlpath);
            //Response.Redirect(@"http://sulphurserver15/SulpHurReports2/CMIntlTestResults.aspx?url=" + xmlpath);
            Response.Redirect(redirectLink);
        }
        public string updateStateOfXml(string url)
        {
            string afterbuild = url.Substring(url.IndexOf("_") + 1, 4);
            string dirc = url.Substring(0, url.LastIndexOf('\\') + 1);
            XmlDocument Xd = new XmlDocument();
            Xd.Load(url);
            XmlNode rootNode = Xd.SelectSingleNode("/ArrayOfPageDiffInfoByFeature");
            XmlNodeList PageDiffInfoByFeatureNodeList = Xd.SelectNodes("/ArrayOfPageDiffInfoByFeature/PageDiffInfoByFeature");
            string Addcolor = "Grey";
            string Modifycolor = "Grey";
            string Removecolor = "Grey";
            foreach (XmlNode PageDiffInfoByFeatureNode in PageDiffInfoByFeatureNodeList)
            {
                string color = "Grey";
                XmlNodeList pageDiffInfoNodeList = PageDiffInfoByFeatureNode.SelectNodes("Groups/PageDiffInfo");
                if (pageDiffInfoNodeList != null)
                {
                    if (PageDiffInfoByFeatureNode.SelectSingleNode("ModifyType").InnerText == "Add")
                    {
                        if (Addcolor == "Grey")
                        {
                            Addcolor = "Green";
                        }
                        if (color == "Grey")
                        {
                            color = "Green";
                        }
                        XmlElement xesub = Xd.CreateElement("PageDiffInfoState");
                        for (int i = 0; i < pageDiffInfoNodeList.Count; i++)
                        {
                            XmlElement pagexesub = Xd.CreateElement("SulpHurState");
                            pagexesub.InnerText = GetColorForFile(pageDiffInfoNodeList[i].Attributes["Name"].Value, afterbuild);
                            pageDiffInfoNodeList[i].AppendChild(pagexesub);
                            if (pagexesub.InnerText == "Fuchsia")
                            {
                                Addcolor = "Fuchsia";
                            }
                            else if (pagexesub.InnerText == "Red" && Addcolor != "Fuchsia")
                            {
                                Addcolor = "Red";
                            }
                            else if (pagexesub.InnerText == "Orange" && Addcolor != "Fuchsia" && Addcolor != "Red")
                            {
                                Addcolor = "Orange";
                            }
                            if (pagexesub.InnerText == "Fuchsia")
                            {
                                color = "Fuchsia";
                            }
                            else if (pagexesub.InnerText == "Red" && color != "Fuchsia")
                            {
                                color = "Red";
                            }
                            else if (pagexesub.InnerText == "Orange" && color != "Fuchsia" && color != "Red")
                            {
                                color = "Orange";
                            }
                        }
                        xesub.InnerText = color;
                        PageDiffInfoByFeatureNode.AppendChild(xesub);
                    }
                    if (PageDiffInfoByFeatureNode.SelectSingleNode("ModifyType").InnerText == "Modify")
                    {
                        if (Modifycolor == "Grey")
                        {
                            Modifycolor = "Green";
                        }
                        if (color == "Grey")
                        {
                            color = "Green";
                        }
                        XmlElement xesub = Xd.CreateElement("PageDiffInfoState");
                        for (int i = 0; i < pageDiffInfoNodeList.Count; i++)
                        {
                            XmlElement pagexesub = Xd.CreateElement("SulpHurState");
                            pagexesub.InnerText = GetColorForFile(pageDiffInfoNodeList[i].Attributes["Name"].Value, afterbuild);
                            pageDiffInfoNodeList[i].AppendChild(pagexesub);
                            if (pagexesub.InnerText == "Fuchsia")
                            {
                                Modifycolor = "Fuchsia";
                            }
                            else if (pagexesub.InnerText == "Red" && Modifycolor != "Fuchsia")
                            {
                                Modifycolor = "Red";
                            }
                            else if (pagexesub.InnerText == "Orange" && Modifycolor != "Fuchsia" && Modifycolor != "Red")
                            {
                                Modifycolor = "Orange";
                            }
                            if (pagexesub.InnerText == "Fuchsia")
                            {
                                color = "Fuchsia";
                            }
                            else if (pagexesub.InnerText == "Red" && color != "Fuchsia")
                            {
                                color = "Red";
                            }
                            else if (pagexesub.InnerText == "Orange" && color != "Fuchsia" && color != "Red")
                            {
                                color = "Orange";
                            }
                        }
                        xesub.InnerText = color;
                        PageDiffInfoByFeatureNode.AppendChild(xesub);
                    }
                    if (PageDiffInfoByFeatureNode.SelectSingleNode("ModifyType").InnerText == "Remove")
                    {
                        if (Removecolor == "Grey")
                        {
                            Removecolor = "Green";
                        }
                        if (color == "Grey")
                        {
                            color = "Green";
                        }
                        XmlElement xesub = Xd.CreateElement("PageDiffInfoState");
                        for (int i = 0; i < pageDiffInfoNodeList.Count; i++)
                        {
                            XmlElement pagexesub = Xd.CreateElement("SulpHurState");
                            pagexesub.InnerText = GetColorForFile(pageDiffInfoNodeList[i].Attributes["Name"].Value, afterbuild);
                            pageDiffInfoNodeList[i].AppendChild(pagexesub);
                            if (pagexesub.InnerText == "Fuchsia")
                            {
                                Removecolor = "Fuchsia";
                            }
                            else if (pagexesub.InnerText == "Red" && Removecolor != "Fuchsia")
                            {
                                Removecolor = "Red";
                            }
                            else if (pagexesub.InnerText == "Orange" && Removecolor != "Fuchsia" && Removecolor != "Red")
                            {
                                Removecolor = "Orange";
                            }
                            if (pagexesub.InnerText == "Fuchsia")
                            {
                                color = "Fuchsia";
                            }
                            else if (pagexesub.InnerText == "Red" && color != "Fuchsia")
                            {
                                color = "Red";
                            }
                            else if (pagexesub.InnerText == "Orange" && color != "Fuchsia" && color != "Red")
                            {
                                color = "Orange";
                            }
                        }
                        xesub.InnerText = color;
                        PageDiffInfoByFeatureNode.AppendChild(xesub);
                    }
                }
            }
            XmlElement addState = Xd.CreateElement("AddState");
            XmlElement removeState = Xd.CreateElement("RemoveState");
            XmlElement modifyState = Xd.CreateElement("ModifyState");
            addState.InnerText = Addcolor;
            removeState.InnerText = Removecolor;
            modifyState.InnerText = Modifycolor;
            rootNode.AppendChild(addState);
            rootNode.AppendChild(removeState);
            rootNode.AppendChild(modifyState);
            string file = dirc + "PageNav_" + afterbuild + "_" + DateTime.Now.ToString("yyyyMMdd_hhmmss") + ".xml";
            Xd.Save(file);
            return file;
        }

        public string GetColorForFile(string filepath, string build)
        {
            string color = "Grey";
            string assemblyname = filepath.Split(' ')[0] + ".dll";
            string fulltypename = filepath.Split(' ')[1];
            string languages = System.Configuration.ConfigurationManager.AppSettings["AllLan"].ToString();
            string lanRelatedBasicRules = System.Configuration.ConfigurationManager.AppSettings["LanguageRelatedBasicRules"].ToString();
            string lanNotRelatedBasicRules = System.Configuration.ConfigurationManager.AppSettings["LanguageNotRelatedBasicRules"].ToString();
            string buildNo = build;
            string info = GetTestCoverageResult(assemblyname, fulltypename, languages, lanRelatedBasicRules, buildNo) + "|";
            info += GetTestCoverageResult(assemblyname, fulltypename, "ENU", lanNotRelatedBasicRules, buildNo);
            if (!lanRelatedBasicRules.Contains("Access Key Duplicate Rule"))
            {
                info += "|" + GetTestCoverageResult(assemblyname, fulltypename, "ENU,RUS,DEU,FRA", "Access Key Duplicate Rule", buildNo);
            }
            if (info.Contains("NotRun")) color = "Fuchsia";
            else
            {
                string[] infoarray = info.Split('|');
                foreach (string message in infoarray)
                {
                    if (message.Contains("[F]:") && message.Contains("[U]"))
                    {
                        color = "Red";
                        break;
                    }
                    if (message.Contains("[W]:") && message.Contains("[U]"))
                    {
                        color = "Orange";
                    }
                }
                if (color != "Red" && color != "Orange")
                {
                    color = "Green";
                }
            }
            return color;
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
                        int latestbuild = int.Parse(latestbuildNo);
                        int inputbuild = int.Parse(buildNo);
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
                    .OrderBy(row => row.ReviewFlag);
                var failresult = faillist.FirstOrDefault();
                if (failresult != null)
                {
                    list = faillist
                        .Where(row => row.ReviewFlag == failresult.ReviewFlag)
                        .OrderBy(row => row.ResultType);
                    result = list.FirstOrDefault();
                }
                else
                {
                    list = list
                    .Where(row => row.BuildNo == result.BuildNo && row.ResultType == "Pass")
                    .OrderBy(row => row.ReviewFlag);
                    result = list.FirstOrDefault();
                }
                resultID = result.ResultID;
                LatestbuildNo = result.BuildNo.Substring(4, 4);
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
    }
}