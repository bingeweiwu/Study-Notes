using CMSherlock.VsoOdataClient;
using iTextSharp.text;
using iTextSharp.text.pdf;
using SulpHurManagementSystem.Models;
using SulpHurManagementSystem.Utility;
using SulpHurServiceAbstract;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Services;

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

        private static List<string> osRelatedRuleIDs;
        private static List<string> osNotRelatedRuleIDs;

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
            if (assemblyLink == null)
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
        public int GetTotalRecords(string buildNO, string buildLan, string osType, string osLanguage, string ruleid,
            string resultTypes, string assemblyName, string typeID, string username, string pagetitle, string reviewComments,
            string ReviewedType, string resultid, string searchDateTime, bool getLatest, int latestDays)
        {
            if (typeID == "All")
                typeID = "-1";
            SQLUtility.osRelatedRuleIDs = osRelatedRuleIDs;
            SQLUtility.osNotRelatedRuleIDs = osNotRelatedRuleIDs;
            int count = SQLUtility.GetCountOfTotalRecords(buildNO, buildLan, osType, osLanguage, ruleid, resultTypes, assemblyName, typeID,
                username, pagetitle, reviewComments, ReviewedType, resultid, searchDateTime, getLatest, latestDays);
            return count;
        }
        [WebMethod]
        public List<QueryResult> BindingTable(string buildNO, string buildLan, string osType, string osLanguage, string ruleid,
            string resultTypes, string assemblyName, string typeID, string pageIndex, string username, string pagetitle, string reviewComments,
            string ReviewedType, string resultid, string sortBy, string searchDateTime, string currentPageResults,
            string diffentReviewStatusResultsCount, bool getLatest, int latestDays)
        {
            if (typeID == "All")
                typeID = "-1";
            SQLUtility.osRelatedRuleIDs = osRelatedRuleIDs;
            SQLUtility.osNotRelatedRuleIDs = osNotRelatedRuleIDs;
            List<QueryResult> resultReportList = SQLUtility.GetDetailResultsOfSpecifiedPage(buildNO, buildLan, osType, osLanguage, ruleid,
                resultTypes, assemblyName, typeID, pageIndex, username, pagetitle, reviewComments, ReviewedType, resultid, sortBy, searchDateTime,
                currentPageResults, diffentReviewStatusResultsCount, getLatest, latestDays);
            return resultReportList;
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

        [WebMethod]
        public string[] GetUIDiffSubDirectory()
        {
            string UIDiffResultPathBase = ConfigurationManager.AppSettings["UIDiffResultPathBase"];
            string[] directoryList = DirectoryUtility.GetSubDirectory(UIDiffResultPathBase);
            return directoryList;
        }
        [WebMethod]
        public string GetScreenshotPath(int resultID)
        {
            string imagePath = string.Format(@"Tmp_ResultImage/{0}.jpg", resultID);
            string localImagePath = Path.Combine(Server.MapPath(""), imagePath);
            if (!File.Exists(localImagePath))
            {
                byte[] resultImage = SQLUtility.GetResultScreenshot(resultID);
                System.Drawing.Image image = null;
                if (resultImage == null)
                {
                    resultImage = SQLUtility.GetUIScreenshot(resultID);
                    if (resultImage != null)
                        image = Bitmap.FromStream(new MemoryStream(resultImage));
                }
                else
                    image = Bitmap.FromStream(new MemoryStream(resultImage));
                image.Save(localImagePath, ImageFormat.Jpeg);
            }

            return imagePath;
        }
        [WebMethod]
        public string GeneratePDF(int resultID)
        {
            try
            {
                string fileFullPath = string.Empty;

                ResultDetailInfo resultDetailInfo = SQLUtility.GetResultInfo(resultID);
                string resultType = resultDetailInfo.ResultType;
                string logMessage = resultDetailInfo.ResultLog;

                BuildInfo buildInfo = SQLUtility.GetBuildInfo(resultID);
                string buildNo = buildInfo.BuildNo;
                string buildLanguage = buildInfo.Language;

                Client client = SQLUtility.GetClient(resultID);
                string osType = client.OSType;
                string osLanguage = client.OSLanguage;


                UIInfo uiInfo = SQLUtility.GetUIInfo(resultID);
                string uiName = uiInfo.UIName;
                string reproStep = uiInfo.LaunchedFrom;
                if (string.IsNullOrEmpty(reproStep))
                    reproStep = "No repro steps captured";

                Rule rule = SQLUtility.GetRule(resultID);
                string ruleName = rule.RuleName;
                string ruleDescription = rule.RuleDesc;

                string imagePath = string.Format(@"Tmp_ResultImage/{0}.jpg", resultID);
                string screenshotPath = Path.Combine(Server.MapPath(""), imagePath);

                Dictionary<string, List<string>> assemblyInfoDic = SQLUtility.GetAssemblyInfoDic(resultID);
                string assemblyName = string.Empty;
                string fullTypeName = string.Empty;

                string shortuiName = uiName;
                if (uiName.Length > 40)
                    shortuiName = uiName.Substring(0, 40) + "~~~";
                string fileName = string.Format("{0} [{1}] {2}_{3}_ResultID{4}_{5}", ruleName, resultType, buildLanguage, uiName, resultID, logMessage);
                fileName = this.UpdateFileName(fileName);
                string path = ConfigurationManager.AppSettings["PDFFolder"];
                if (fileName.Length > 180)
                    fileName = fileName.Substring(0, 180) + "~~~";
                fileFullPath = path + "\\" + fileName + ".pdf";

                string latestPassResultLinkFormat = "http://{0}/SulpHurReports/CapturedUIReport.aspx?buildlanguage={1}&rule={2}&assembly={3}&fulltypename={4}&resulttype=Pass&showlatest=buildno";
                bool generated = this.WritePDFDocument(fileFullPath, resultID, logMessage, uiName, reproStep, buildNo, buildLanguage, osType, osLanguage, ruleName,
                    ruleDescription, latestPassResultLinkFormat, assemblyInfoDic, screenshotPath);
                string returnFilePath = string.Empty;
                if (generated)
                    returnFilePath = fileFullPath;
                return returnFilePath;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region private methods
        private string UpdateFileName(string originalFileName)
        {
            string updatedFileName = originalFileName;
            updatedFileName = updatedFileName.Replace("'", "‘");
            updatedFileName = updatedFileName.Replace("\"", "‘");

            updatedFileName = updatedFileName.Replace("\\", " ");
            updatedFileName = updatedFileName.Replace("/", " ");
            updatedFileName = updatedFileName.Replace(":", " ");
            updatedFileName = updatedFileName.Replace("*", " ");
            updatedFileName = updatedFileName.Replace("?", " ");
            updatedFileName = updatedFileName.Replace("<", " ");
            updatedFileName = updatedFileName.Replace(">", " ");
            updatedFileName = updatedFileName.Replace("|", " ");
            updatedFileName = updatedFileName.Replace(".", " ");
            updatedFileName = updatedFileName.Replace("\r", " ");
            updatedFileName = updatedFileName.Replace("\n", " ");
            return updatedFileName;
        }
        private bool WritePDFDocument(string fileFullPath, int resultID, string resultLog, string uiName, string reproStep, string buildNo, string buildLanguage,
            string osType, string osLanguage, string ruleName, string ruleDescription, string latestPassResultLinkFormat,
            Dictionary<string, List<string>> assemblyInfoDic, string screenshotPath)
        {
            Document d = new Document(PageSize.A4, 10, 10, 25, 25);
            try
            {
                FileStream fs = new FileStream(fileFullPath, FileMode.OpenOrCreate);
                PdfWriter writer = PdfWriter.GetInstance(d, fs);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            string fontPath = ConfigurationManager.AppSettings["FontPath"];
            string fontDetail;
            switch (buildLanguage)
            {
                case "CHS":
                    fontDetail = "SIMSUN.TTC,1";
                    break;
                case "CHT":
                    fontDetail = "Msjh.TTC,1";
                    break;
                case "JPN":
                    fontDetail = "MSMincho.TTC,1";
                    break;
                case "KOR":
                    fontDetail = "Gulim.TTC,1";
                    break;
                default:
                    fontDetail = "Arial.TTF";
                    break;
            }
            BaseFont baseFont = BaseFont.CreateFont(fontPath + fontDetail, BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
            iTextSharp.text.Font fontTitle = new iTextSharp.text.Font(baseFont, 16, 1);
            iTextSharp.text.Font fontDesc = new iTextSharp.text.Font(baseFont, 11, 1);
            iTextSharp.text.Font fontLink = FontFactory.GetFont(fontPath + fontDetail, 12, iTextSharp.text.Font.UNDERLINE, new BaseColor(0, 0, 255));

            iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(screenshotPath);
            if (img.Width > 550)
                img.ScaleToFit(550, 600);

            d.Open();
            d.Add(new Paragraph("File Name:", fontTitle));
            d.Add(new Paragraph(fileFullPath.Substring(fileFullPath.LastIndexOf("\\") + 1), fontDesc));
            d.Add(new Paragraph("Result ID:", fontTitle));
            d.Add(new Paragraph(resultID.ToString(), fontDesc));
            d.Add(new Paragraph("UI Name:", fontTitle));
            d.Add(new Paragraph(uiName, fontDesc));
            d.Add(new Paragraph("Repro Steps:", fontTitle));
            d.Add(new Paragraph(reproStep, fontDesc));
            d.Add(new Paragraph("Build Language:", fontTitle));
            d.Add(new Paragraph(buildLanguage, fontDesc));
            d.Add(new Paragraph("Real OS Language:", fontTitle));
            d.Add(new Paragraph(osLanguage, fontDesc));
            d.Add(new Paragraph("OS Type:", fontTitle));
            d.Add(new Paragraph(osType, fontDesc));
            d.Add(new Paragraph("Build No:", fontTitle));
            d.Add(new Paragraph(buildNo, fontDesc));
            d.Add(new Paragraph("Rule Name:", fontTitle));
            d.Add(new Paragraph(ruleName, fontDesc));
            d.Add(new Paragraph("Rule Description:", fontTitle));
            d.Add(new Paragraph(ruleDescription, fontDesc));
            if (assemblyInfoDic.Count > 0)
            {
                foreach (string keyAssemblyName in assemblyInfoDic.Keys)
                {
                    d.Add(new Paragraph("Assembly:", fontTitle));
                    d.Add(new Paragraph(keyAssemblyName, fontDesc));
                    d.Add(new Paragraph("Full Type Name:", fontTitle));
                    foreach (string keyFullTypeName in assemblyInfoDic[keyAssemblyName])
                        d.Add(new Paragraph(keyFullTypeName, fontDesc));
                }
                d.Add(new Paragraph("Latest Pass result:", fontTitle));
                Anchor anchor = null;
                string latestPassResultLink;
                foreach (string keyAssemblyName in assemblyInfoDic.Keys)
                {
                    foreach (string itemFullTypeName in assemblyInfoDic[keyAssemblyName])
                    {
                        latestPassResultLink = string.Format(latestPassResultLinkFormat, this.serverName, buildLanguage, ruleName, keyAssemblyName, itemFullTypeName);
                        anchor = new Anchor(latestPassResultLink, fontLink);
                        anchor.Reference = latestPassResultLink;
                        d.Add(anchor);
                    }
                }
            }
            d.Add(new Paragraph("Result Log:", fontTitle));
            d.Add(new Paragraph(resultLog, fontDesc));
            d.Add(img);
            d.Close();

            bool generated = File.Exists(fileFullPath);
            return generated;
        }
        #endregion

        #region update from multiple tables
        [WebMethod]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="resultID"></param>
        /// <param name="bugIDs">the bug ID string should be like 111,456,223</param>
        public void LinkBug(int resultID, string bugIDs)
        {
            string[] bugIDStringArray = bugIDs.Split(',');
            List<int> bugIDList = new List<int>();
            foreach (string bugIDString in bugIDStringArray)
                bugIDList.Add(int.Parse(bugIDString));

            Dictionary<string, List<string>> assemblyInfoDic = SQLUtility.GetAssemblyInfoDic(resultID);
            foreach (int bugID in bugIDList)
            {
                foreach (string assemblyName in assemblyInfoDic.Keys)
                {
                    foreach (string fullTypeName in assemblyInfoDic[assemblyName])
                    {
                        SQLUtility.LinkToBug(bugID, assemblyName, fullTypeName);
                    }
                }
            }
        }
        #endregion

        #region query from table - results
        [WebMethod]
        public string GetReviewLog(int resultID)
        {
            string reviewLog = SQLUtility.GetReviewLog(resultID);
            return reviewLog;
        }
        [WebMethod]
        public string GetResultLog(int resultID)
        {
            string resultLog = SQLUtility.GetResultLog(resultID);
            return resultLog;
        }
        [WebMethod]
        public Models.ResultDetailInfo GetResultDetailInfo(int resultID)
        {
            Models.ResultDetailInfo resultDetailInfo = SQLUtility.GetResultInfo(resultID);
            return resultDetailInfo;
        }
        #endregion

        #region query from multiple tables
        [WebMethod]
        public UIInfo GetUIInfo(int resultID)
        {
            UIInfo uiInfo = SQLUtility.GetUIInfo(resultID);
            return uiInfo;
        }
        [WebMethod]
        public List<bug> GetRelatedBugs(int resultID)
        {
            List<bug> bugs = SQLUtility.GetRelatedBugs(resultID);
            return bugs;
        }
        [WebMethod]
        public List<Models.AssemblyInfo> GetRelatedAssemblies(int resultID)
        {
            List<Models.AssemblyInfo> assemblyInfolist = SQLUtility.GetAssemblyInfoes(resultID);
            return assemblyInfolist;
        }
        [WebMethod]
        public Dictionary<string, List<string>> GetRelatedAssemblyDic(int resultID)
        {
            Dictionary<string, List<string>> relatedAssemblyDic = SQLUtility.GetAssemblyInfoDic(resultID);
            return relatedAssemblyDic;
        }
        [WebMethod]
        public Dictionary<string, Dictionary<string, LatestResultSummary>> GetLatestResultSummary(string aboveBuildNo, string buildLanguages, string ruleIDs, string assemblyName, string fullTypeName)
        {
            if (buildLanguages != "All")
            {
                buildLanguages = buildLanguages.Replace(",", "','");
                buildLanguages = "'" + buildLanguages + "'";
            }
            Dictionary<string, Dictionary<string, LatestResultSummary>> dic = SQLUtility.GetLatestResultSummary(aboveBuildNo, assemblyName, ruleIDs, fullTypeName, buildLanguages);
            return dic;
        }
        #endregion
    }
}