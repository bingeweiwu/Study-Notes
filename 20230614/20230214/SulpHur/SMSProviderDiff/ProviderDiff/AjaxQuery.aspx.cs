using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Xml;
using System.Threading;
using System.Xml.XPath;
using System.Collections;
using System.Text.RegularExpressions;
using System.Web.UI.DataVisualization.Charting;
using System.Diagnostics;
using System.Web.Services;
using System.Data;
using System.Data.SqlClient;
using System.Web.Script.Services;
using System.Web.Script.Serialization;
using SulpHurServiceAbstract;

namespace ProviderDiff
{
    //[WebService(Namespace = "http://tempuri.org/")]
    //[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    //[ScriptService]
    //public class TestService : WebService
    //{
    //    [WebMethod]
    //    public BuildType[] BindDatatable(string rb, string sl, string sb, string ass, string sc)
    //    {
    //        //string selectedLanguage = this.Request.Form["sl"];
    //        //string selectedBuild = this.Request.Form["sb"]; ;
    //        //string selectedCategory = this.Request.Form["sc"]; ;
    //        //List<string> builds = selectedBuild.Split('|').ToList();
    //        //List<string> categories = selectedCategory.Split('|').ToList();
    //        //string buildno = this.Request.Form["rb"]; ;
    //        //string selectedAssembly = this.Request.Form["ass"]; ;

    //        string selectedLanguage = sl;
    //        string selectedBuild = sb;
    //        string selectedCategory = sc;
    //        List<string> builds = selectedBuild.Split('|').ToList();
    //        List<string> categories = selectedCategory.Split('|').ToList();
    //        string buildno = rb;
    //        string selectedAssembly = ass;

    //        DataTable dt = new DataTable();
    //        List<BuildType> details = new List<BuildType>();
    //        List<BuildType> detailResult = new List<BuildType>();
    //        using (SulpHurEntities entity = new SulpHurEntities())
    //        {
    //            var referencedList = from x in entity.BuildTypes
    //                                 where x.BuildNo == buildno
    //                                 select x;

    //            if (selectedAssembly != "All")
    //            {
    //                referencedList = referencedList.Where(c => c.AssemblyName == selectedAssembly);
    //            }
    //            int z = referencedList.Count();
    //            foreach (var c in referencedList)
    //            {
    //                if (c.Mark == "Captured" && categories.Contains("Captured"))
    //                {
    //                    BuildType temp = new BuildType();
    //                    temp.AssemblyName = c.AssemblyName;
    //                    temp.TypeName = c.TypeName;
    //                    temp.Mark = c.Mark;
    //                    details.Add(temp);
    //                    continue;
    //                }

    //                if (c.Mark != "Captured" &&
    //                    entity.ViewAssemblySummaries.Any(v =>
    //                        c.AssemblyName == v.AssemblyName
    //                        && c.TypeName == v.FullTypeName))
    //                {
    //                    c.Mark = "Captured";
    //                    entity.SaveChanges();
    //                    BuildType temp = new BuildType();
    //                    temp.AssemblyName = c.AssemblyName;
    //                    temp.TypeName = c.TypeName;
    //                    temp.Mark = c.Mark;
    //                    details.Add(temp);
    //                    continue;
    //                }

    //                if (selectedLanguage == "All")
    //                {

    //                    if (!entity.ViewAssemblySummaries.Any(q => builds.Contains(q.BuildNo)
    //                        && c.AssemblyName == q.AssemblyName
    //                        && c.TypeName == q.FullTypeName))
    //                    {
    //                        if (entity.ViewAssemblySummaries.Any(v =>
    //                        c.AssemblyName == v.AssemblyName
    //                        && c.TypeName == v.FullTypeName))
    //                        {
    //                            BuildType temp = new BuildType();
    //                            temp.AssemblyName = c.AssemblyName;
    //                            temp.TypeName = c.TypeName;
    //                            temp.Mark = "Captured In Other Builds/Language";
    //                            details.Add(temp);
    //                        }
    //                        else
    //                        {
    //                            BuildType temp = new BuildType();
    //                            temp.AssemblyName = c.AssemblyName;
    //                            temp.TypeName = c.TypeName;
    //                            temp.Mark = c.Mark;
    //                            details.Add(temp);
    //                        }
    //                    }


    //                }
    //                else
    //                {
    //                    if (!entity.ViewAssemblySummaries.Any(q => builds.Contains(q.BuildNo)
    //                            && q.Language == selectedLanguage
    //                            && c.AssemblyName == q.AssemblyName
    //                            && c.TypeName == q.FullTypeName))
    //                    {
    //                        if (entity.ViewAssemblySummaries.Any(v =>
    //                        c.AssemblyName == v.AssemblyName
    //                        && c.TypeName == v.FullTypeName))
    //                        {
    //                            BuildType temp = new BuildType();
    //                            temp.AssemblyName = c.AssemblyName;
    //                            temp.TypeName = c.TypeName;
    //                            temp.Mark = "Captured In Other Builds/Language";
    //                            details.Add(temp);
    //                        }
    //                        else
    //                        {
    //                            BuildType temp = new BuildType();
    //                            temp.AssemblyName = c.AssemblyName;
    //                            temp.TypeName = c.TypeName;
    //                            temp.Mark = c.Mark;
    //                            details.Add(temp);
    //                        }
    //                    }

    //                }
    //            }
    //        }

    //        foreach (BuildType bt in details)
    //        {
    //            if (categories.Contains(bt.Mark))
    //            {
    //                detailResult.Add(bt);
    //            }
    //        }

    //        return detailResult.ToArray() ;
    //    }
    //}



    public partial class AjaxQuery : System.Web.UI.Page
    {
        string[] xmlFiles;
        private static string sourceCodeFolder = System.Configuration.ConfigurationManager.AppSettings["SourceCodeFolder"];
        string preBuildNum;
        string postBuildNum;
        bool copySuccess = false;
        string preMofFilePath;
        string postMofFilePath;
        List<string> usedWMIClassList;
        string[] csFiles;
        List<string> assemblies;
        SulpHurEntities entities;
        Dictionary<string, string> ruleDir = new Dictionary<string, string>() { 
        { "Text Truncation", "Text Truncation Rule" },
        { "Tab Order", "Tab Order Rule" } ,
        {"Window Size","Window Size Rule"},
        {"Access Key","Access Key Rule"},
        {"Overlap","Overlap Rule"},
        {"CM Alignment","CM Alignment Rule"},
        {"UA Text","UA Text Rule"}
        };
        List<string> mileStone;
        string joinContentTable = "JOIN [dbo].[UIContents] AS C ON A.[ContentID] = C.[ContentID] ";
        string joinBuildInfoTable = "JOIN [dbo].[BuildInfo] AS D ON D.[BuildID] = C.[BuildID] ";
        string joinClientsTable = "JOIN [dbo].[Clients] AS E ON E.[ClientID] = C.[ClientID] ";
        string joinAssemblyInfoTable = "JOIN [dbo].[AssemblyInfo] AS B ON B.[TypeID] = A.[TypeID] ";
        string formatbuildno0 = "AND (D.[BuildNo] IN ({0})) ";
        string formatbuildno0NoAnd = "AND (D.[BuildNo] IN ({0})) ";
        string joinRuleTable = "JOIN [dbo].[Rules] AS G ON G.[RuleID] = A.[RuleID] ";
        string formatlanguage1 = "AND (D.[Language] IN ({1})) ";
        string formatostype2 = "AND (E.[OSType] IN ({2})) ";

        public string ComparedBuilds
        {
            get
            {
                return preBuildNum + " - " + postBuildNum;
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.Request.Form["IsAjax"] != null && this.Request.Form["IsAjax"].ToLower().Equals("true"))
            {
                try
                {
                    this.GetType().GetMethod(this.Request.Form["Method"].ToString()).Invoke(this, null);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError("Exception: {0}", ex);
                    Response.Write(string.Format("[E]:{0}", "Unknown error."));
                }
            }
        }
        /// <summary>
        /// 'a|b'->N'a',N'b'
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
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
        private void SaveJpeg(string path, System.Drawing.Image img, int quality)
        {
            EncoderParameter qualityParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            ImageCodecInfo jpegCodec = GetEncoderInfo(@"image/jpeg");
            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = qualityParam;
            System.IO.MemoryStream mss = new System.IO.MemoryStream();
            System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite);
            img.Save(mss, jpegCodec, encoderParams);
            byte[] matriz = mss.ToArray();
            fs.Write(matriz, 0, matriz.Length);
            mss.Close();
            fs.Close();
        }
        private void UpdateMarkOnBuildTypes(string buildno)
        {
            SqlConnection conn = new SqlConnection("Data Source=sulphurserver14;Initial Catalog=SulpHur;Integrated Security=True");
            string sql = string.Format("update buildtypes set mark='Captured' where buildno='{0}' and mark!='Captured' and typename in(select fulltypename from assemblyinfo)",buildno);
            SqlCommand command = new SqlCommand(sql, conn);
            command.CommandType = CommandType.Text;
            command.CommandTimeout = 30000;
            conn.Open();
            command.ExecuteNonQuery();
            conn.Close();
        }
        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }
        public void Ajax_GetClass()
        {
            try
            {
                //parse paraments
                int resultID;
                try
                {
                    if (!int.TryParse(this.Request.Form["ResultID"], out resultID))
                    {
                        throw new Exception("Invalid ResultID!");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex);
                    throw new Exception("Invalid Paraments!");
                }

                string preClass = string.Empty;
                string postClass = string.Empty;
                SulpHurEntities entities = new SulpHurEntities();
                var list = from x in entities.ProviderDiffResults
                           where x.ResultID == resultID
                           select x;
                foreach (var x in list)
                {
                    if (x.DiffType == "Modified")
                    {
                        preClass = x.OldClass;
                        postClass = x.NewClass;

                        string preLightedClass = string.Empty;
                        string postLightedClass = string.Empty;
                        preLightedClass = preLightedClass.Replace("\t", "&nbsp&nbsp&nbsp&nbsp").Replace(" ", "&nbsp");
                        postLightedClass = postLightedClass.Replace("\t", "&nbsp&nbsp&nbsp&nbsp").Replace(" ", "&nbsp");

                        HighlightClass(preClass, postClass, out preLightedClass, out postLightedClass);

                        Response.Write(string.Format("{0}[|]{1}", preLightedClass, postLightedClass));
                    }
                    else
                    {
                        preClass = x.OldClass;
                        preClass = preClass.Replace("\t", "&nbsp&nbsp&nbsp&nbsp").Replace(" ", "&nbsp").Replace("\r\n", "<br/>");
                        postClass = x.NewClass;
                        postClass = postClass.Replace("\t", "&nbsp&nbsp&nbsp&nbsp").Replace(" ", "&nbsp").Replace("\r\n", "<br/>");
                        Response.Write(string.Format("{0}[|]{1}", preClass, postClass));
                    }
                }

            }
            catch (Exception)
            {
                Response.Write(string.Format("[E]:{0}", "Unknown error."));
            }
        }
        public void Ajax_GetImage()
        {
            try
            {

                //parse paraments
                int resultID;
                try
                {
                    if (!int.TryParse(this.Request.Form["ResultID"], out resultID))
                    {
                        throw new Exception("Invalid ResultID!");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex);
                    throw new Exception("Invalid Paraments!");
                }

                string preBuild = string.Empty;
                string postBuild = string.Empty;

                SulpHurEntities entities = new SulpHurEntities();
                var comparedBuild = from x in entities.ProviderDiffResults
                                    where x.ResultID == resultID
                                    select x.ComparedBuild;
                preBuild = comparedBuild.First().Split('-')[0].Trim();
                postBuild = comparedBuild.First().Split('-')[1].Trim();

                string preImagePath = string.Format(@"{0}{1}.jpg", resultID, preBuild);
                string preLocalImagePath = Path.Combine(Context.Server.MapPath(""), preImagePath);
                string postImagePath = string.Format(@"{0}{1}.jpg", resultID, postBuild);
                string postLocalImagePath = Path.Combine(Context.Server.MapPath(""), postImagePath);

                string assemblyName = this.Request.Form["AssemblyName"];
                string pageName = this.Request.Form["PageName"];
                string preImageID = string.Empty;
                string postImageID = string.Empty;
                string resultStatus = "none";
                string latestImageID = string.Empty;
                string latestBuildID = string.Empty;
                if (entities.AssemblyInfoes.Any(x => x.TypeName == pageName && x.AssemblyName == assemblyName))
                {
                    var typeID = from x in entities.AssemblyInfoes
                                 where x.AssemblyName == assemblyName && x.TypeName == pageName
                                 select x.TypeID;
                    int screentTypeID = typeID.FirstOrDefault();
                    if (entities.ViewTypeBuilds.Any(x => x.TypeID == screentTypeID && x.BuildNo.Contains(preBuild)))
                    {

                        var preScreen = from x in entities.ViewTypeBuilds
                                        where x.TypeID == screentTypeID && x.BuildNo.Contains(preBuild) && x.UIName != "Configuration Manager"
                                        select x;
                        if (preScreen.Any(c => c.Language == "ENU"))
                        {
                            preImageID = preScreen.First(c => c.Language == "ENU").ContentID.ToString();
                        }
                        else
                        {
                            preImageID = preScreen.First().ContentID.ToString();
                        }
                        //preImageID = preScreen.First().ToString();
                        //System.Drawing.Image preImage = Bitmap.FromStream(new MemoryStream(preScreen.First()));
                        //if (!File.Exists(preLocalImagePath))
                        //{
                        //preImage.Save(preLocalImagePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                        //SaveJpeg(preLocalImagePath, preImage, 1);
                        //}
                        //else
                        //{
                        //    File.Delete(preLocalImagePath);
                        //    SaveJpeg(preLocalImagePath, preImage, 1);
                        //}

                    }

                    if (entities.ViewTypeBuilds.Any(x => x.TypeID == screentTypeID && x.BuildNo.Contains(postBuild)))
                    {
                        var postScreen = from x in entities.ViewTypeBuilds
                                         where x.TypeID == screentTypeID && x.BuildNo.Contains(postBuild) && x.UIName != "Configuration Manager"
                                         select x;

                        if (postScreen.Any(c => c.Language == "ENU"))
                        {
                            postImageID = postScreen.First(c => c.Language == "ENU").ContentID.ToString();
                        }
                        else
                        {
                            postImageID = postScreen.First().ContentID.ToString();
                        }

                        //postImageID=postScreen.First().ToString();
                        //System.Drawing.Image postImage = Bitmap.FromStream(new MemoryStream(postScreen.First()));
                        //if (!File.Exists(postLocalImagePath))
                        //{
                        //ostImage.Save(preLocalImagePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                        //SaveJpeg(postLocalImagePath, postImage, 1);
                        //}
                        //else
                        //{
                        //    File.Delete(postLocalImagePath);
                        //    SaveJpeg(postLocalImagePath, postImage, 1);
                        //}


                    }

                    if (string.IsNullOrEmpty(preImageID) && string.IsNullOrEmpty(postImageID))
                    {
                        resultStatus = "latest";

                        var latestScreen = from x in entities.ViewTypeBuilds
                                           where x.TypeID == screentTypeID && x.UIName != "Configuration Manager"
                                           orderby x.BuildNo descending
                                           select x;
                        if (latestScreen.Count() < 1)
                        {
                            resultStatus = "none";
                        }
                        else
                        {
                            if (latestScreen.Any(c => c.Language == "ENU"))
                            {
                                var temp = latestScreen.First(c => c.Language == "ENU");

                                latestImageID = temp.ContentID.ToString();
                                latestBuildID = temp.BuildNo;
                            }
                            else
                            {

                                var temp = latestScreen.First();
                                latestImageID = temp.ContentID.ToString();
                                latestBuildID = temp.BuildNo;

                            }
                        }

                    }
                    else
                    {
                        resultStatus = "build";
                    }
                }
                else
                {
                    resultStatus = "none";
                }

                Response.Write(string.Format("{0}[|]{1}[|]{2}[|]{3}[|]{4}", preImageID, postImageID, resultStatus, latestImageID, latestBuildID.Trim()));
            }
            catch (Exception e1)
            {
                Response.Write(string.Format("[E]:{0}", e1.ToString()));
            }
        }
        public void Ajax_RunDiff()
        {
            preBuildNum = this.Request.Form["PreBuildNumber"];
            postBuildNum = this.Request.Form["PostBuildNumber"];
            bool isoverwrite = Convert.ToBoolean(this.Request.Form["IsOverWrite"]);

            #region Copy mof file from server
            preMofFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, preBuildNum + ".mof");
            postMofFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, postBuildNum + ".mof");

            if (!IfLocalMofExist())
            {
                copySuccess = false;
                CopyMof(preBuildNum);
                if (copySuccess)
                {
                    copySuccess = false;
                    CopyMof(postBuildNum);
                    if (!copySuccess)
                    {
                        ShowError("Post build mof not exist.");
                        return;
                    }
                }
                else
                {
                    ShowError("Pre build mof not exist.");
                    return;
                }
            }

            #endregion

            #region if result already exists
            entities = new SulpHurEntities();

            if (entities.ProviderDiffResults.Any(c => c.ComparedBuild == ComparedBuilds) && !isoverwrite)
            {
                ShowError("The result already exists");
                return;
            }

            if (entities.ProviderDiffResults.Any(c => c.ComparedBuild == ComparedBuilds) && isoverwrite)
            {
                var list = from x in entities.ProviderDiffResults
                           where x.ComparedBuild == ComparedBuilds
                           select x;
                foreach (var c in list)
                {
                    entities.DeleteObject(c);
                }
                entities.SaveChanges();
            }
            #endregion

            Thread diffThread = new Thread(new ThreadStart(StartRun));
            if (HttpContext.Current.Session["isRunning"] == null)
                HttpContext.Current.Session["isRunning"] = false;
            if (!(bool)HttpContext.Current.Session["isRunning"])
            {
                diffThread.Start();
                HttpContext.Current.Session["isRunning"] = true;
            }
            else
            {
                ShowError("provider diff is already running.");
                return;
            }
            ShowError("Diff start running.");
        }
        public void Ajax_UpdateMark()
        {
            string recordIDStr = this.Request.Form["RecordID"];
            string markValue = this.Request.Form["MarkValue"];

            int recordID = Int32.Parse(recordIDStr);
            using (SulpHurEntities e = new SulpHurEntities())
            {
                var records = from x in e.BuildTypes
                              where x.RecordID == recordID
                              select x;

                foreach (var c in records)
                {
                    c.Mark = markValue;
                }
                e.SaveChanges();
            }
        }
        public void Ajax_CalculateCaptureRate()
        {
            string ReferencedBuild = this.Request.Form["ReferencedBuild"];
            string ComparedBuild = this.Request.Form["ComparedBuild"];

            double rate = 0;
            using (SulpHurEntities en = new SulpHurEntities())
            {
                int totalCount = (from x in en.BuildTypes
                                  where x.BuildNo == ReferencedBuild && x.Mark == "ExistingPage"
                                  select x).Count();

                var list = from x in en.AssemblyInfoes
                           select x;

                List<AssemblyInfo> all = new List<AssemblyInfo>();
                foreach (var c in list)
                {
                    all.Add(c);
                }
                List<AssemblyInfo> filtered = new List<AssemblyInfo>();

                if (ComparedBuild != "All")
                {
                    foreach (AssemblyInfo ai in all)
                    {
                        if (en.ViewTypeBuilds.Any(c => c.TypeID == ai.TypeID && c.BuildNo == ComparedBuild))
                        {
                            filtered.Add(ai);
                        }
                    }
                }
                else
                {
                    foreach (AssemblyInfo ai in all)
                    {
                        filtered.Add(ai);
                    }
                }


                int existingcount = 0;
                foreach (AssemblyInfo c in filtered)
                {
                    if (en.BuildTypes.Any(v => v.AssemblyName == c.AssemblyName && v.TypeName == c.FullTypeName))
                    {
                        existingcount++;
                    }
                }
                rate = 100 * existingcount / totalCount;
            }

            Response.Write(string.Format("{0}", rate));
        }
        public void QueryProductBuild()
        {
            using (SulpHurEntities entity = new SulpHurEntities())
            {
                var list = (from x in entity.BuildTypes select x.BuildNo).Distinct().OrderByDescending(c=>c);
                string result = string.Empty;
                foreach (string x in list)
                {
                    result += x;
                    result += "[|]";
                }
                result = result.Substring(0, result.Length - "[|]".Length);
                Response.Write(result);
            }
        }
        public void GetMarkByAssemblyandType() {
            string assemblyName = this.Request.Form["AssemblyName"];
            string fullTypeName = this.Request.Form["FullTypeName"];
            string buildno=this.Request.Form["BuildNo"];

            using (SulpHurEntities entity = new SulpHurEntities()) {
                Response.Write(entity.BuildTypes.FirstOrDefault(c => c.AssemblyName == assemblyName
                    && c.TypeName == fullTypeName
                    && c.BuildNo == buildno).Mark);
            }
        }
        public void GetStepsByAssemblyandType() {
            string assemblyName = this.Request.Form["AssemblyName"];
            string fullTypeName = this.Request.Form["FullTypeName"];
            string buildno = this.Request.Form["BuildNo"];

            using (SulpHurEntities entity = new SulpHurEntities())
            {
                string launchedFrom = "",
                    windowHierarchy = "",
                    customSteps = "" ;
                int contentID = 0;
                // UI with same assembly info on ENU build, and generated step is not null (in UIContents table)
                var generatedSteps = from u in entity.UIContents
                                     join al in entity.AssemblyLinks
                                      on u.ContentID equals al.ContentID
                                     join ai in entity.AssemblyInfoes
                                      on al.TypeID equals ai.TypeID
                                     join bi in entity.BuildInfoes
                                      on u.BuildID equals bi.BuildID
                                      where ai.AssemblyName == assemblyName &&
                                            ai.FullTypeName == fullTypeName &&
                                            bi.Language == "ENU" &&
                                            u.LaunchedFrom != null &&
                                            u.WindowHierarchy != null
                                      select new
                                      {
                                          u.ContentID,
                                          u.LaunchedFrom,
                                          u.WindowHierarchy
                                      };
                var generatedStep = generatedSteps.FirstOrDefault();
                if (generatedStep != null)
                {
                    contentID = generatedStep.ContentID;
                    launchedFrom = generatedStep.LaunchedFrom;
                    windowHierarchy = generatedStep.WindowHierarchy;
                }

                // custom steps (in BuildTypes table)
                customSteps = entity.BuildTypes.FirstOrDefault(c => c.AssemblyName == assemblyName
                    && c.TypeName == fullTypeName
                    && c.BuildNo == buildno).LanuchSteps;

                // return
                var retValue = new { ContentID = contentID, LaunchedFrom = launchedFrom, WindowHierarchy = windowHierarchy, CustomSteps = customSteps };
                Response.Write(retValue.ToJson());
            }
        }
        public void SaveMarkAndSteps()
        {
            string assemblyName = this.Request.Form["AssemblyName"];
            string fullTypeName = this.Request.Form["FullTypeName"];
            string buildno = this.Request.Form["BuildNo"];
            string mark = this.Request.Form["Mark"];
            string steps = this.Request.Form["LanuchSteps"];

            using (SulpHurEntities entity = new SulpHurEntities())
            {
                try
                {
                    BuildType temp = entity.BuildTypes.FirstOrDefault(c => c.AssemblyName == assemblyName
                         && c.TypeName == fullTypeName
                         && c.BuildNo == buildno);
                    temp.LanuchSteps = steps;
                    temp.Mark = mark;
                    entity.SaveChanges();
                    Response.Write("Success");
                }
                catch
                {
                    Response.Write("Error");
                }
            }
        }
        public void QueryAvailableAssemblyInBuildTypes()
        {
            using (SulpHurEntities entity = new SulpHurEntities())
            {
                var list = (from x in entity.BuildTypes select x.AssemblyName).Distinct();
                string result = string.Empty;
                foreach (string x in list)
                {
                    result += x;
                    result += "[|]";
                }
                result = result.Substring(0, result.Length - "[|]".Length);
                Response.Write(result);
            }
        }
        public void QueryCapturedBuild()
        {
            using (SulpHurEntities entity = new SulpHurEntities())
            {
                var list = (from x in entity.BuildInfoes
                            select x.BuildNo).Distinct().OrderByDescending(n => n);
                string result = string.Empty;
                foreach (string x in list)
                {
                    result += x.Trim();
                    result += "[|]";
                }
                result = result.Substring(0, result.Length - "[|]".Length);
                Response.Write(result);
            }
        }
        public void QueryBuildLan()
        {
            using (SulpHurEntities entity = new SulpHurEntities())
            {
                var list = (from x in entity.BuildInfoes
                            select x.Language).Distinct().OrderByDescending(n => n);

                string result = string.Empty;
                foreach (string x in list)
                {
                    result += x.Trim();
                    result += "[|]";
                }
                result = result.Substring(0, result.Length - "[|]".Length);
                Response.Write(result);
            }
        }
        public void QueryAvailableOSType() {
            using (SulpHurEntities entity = new SulpHurEntities())
            {
                var list = (from x in entity.Clients
                            select x.OSType).Distinct().OrderByDescending(n => n);

                string result = string.Empty;
                foreach (string x in list)
                {
                    result += x.Trim();
                    result += "[|]";
                }
                result = result.Substring(0, result.Length - "[|]".Length);
                Response.Write(result);
            }
        }
        public void QueryPagesSummaryData()
        {
            string selectedLanguage = this.Request.Form["SelectedLanguage"];
            string selectedOSType = this.Request.Form["OSType"];
            string selectedBuild = this.Request.Form["SelectedBuild"];
            List<string> builds = selectedBuild.Split('|').ToList();
            string buildno = this.Request.Form["ReferencedBuild"];

            using (SulpHurEntities entity = new SulpHurEntities())
            {
                entity.CommandTimeout = 600000;
                #region image total
                //int selectedUICount = 0;
                //if (selectedLanguage == "All"&&selectedOSType=="All")
                //{
                //    selectedUICount = (from x in entity.ViewAssemblySummaries
                //                       where builds.Contains(x.BuildNo.Trim())&&x.IsPageIdentifier==true
                //                       select new { x.AssemblyName, x.FullTypeName }).Distinct().Count();
                //}
                //else if (selectedLanguage != "All" && selectedOSType == "All")
                //{
                //    selectedUICount = (from x in entity.ViewAssemblySummaries
                //                       where builds.Contains(x.BuildNo.Trim()) && x.Language == selectedLanguage && x.IsPageIdentifier == true
                //                       select new { x.AssemblyName, x.FullTypeName }).Distinct().Count();
                //}
                //else if (selectedLanguage == "All" && selectedOSType != "All")
                //{
                //    selectedUICount = (from x in entity.ViewAssemblySummaries
                //                       where builds.Contains(x.BuildNo.Trim()) && x.OSType == selectedOSType && x.IsPageIdentifier == true
                //                       select new { x.AssemblyName, x.FullTypeName }).Distinct().Count();
                //}
                //else if (selectedLanguage != "All" && selectedOSType != "All") {
                //    selectedUICount = (from x in entity.ViewAssemblySummaries
                //                       where builds.Contains(x.BuildNo.Trim()) && x.OSType == selectedOSType && x.Language == selectedLanguage && x.IsPageIdentifier == true
                //                       select new { x.AssemblyName, x.FullTypeName }).Distinct().Count();
                //}

                //int capturedUICount = 0;
                var totalUI = from x in entity.BuildTypes
                              where x.BuildNo == buildno
                              select x;
                foreach (var v in totalUI)
                {
                    if (entity.ViewAssemblySummaries.Any(c => c.AssemblyName == v.AssemblyName && c.FullTypeName == v.TypeName))
                    {
                        v.Mark = "Captured";
                        //capturedUICount++;
                    }
                }
                entity.SaveChanges();

                //List<string> validMarks = new List<string>() { "Valid UI (Missing Capture)" };
                //int validUICount = (from x in entity.BuildTypes
                //                    where validMarks.Contains(x.Mark)&& x.BuildNo == buildno
                //                    select x).Count();

                List<string> validMarks = new List<string>() { "Valid UI (Missing Capture)", "Captured" };

                var listAll = from x in entity.BuildTypes
                              where validMarks.Contains(x.Mark) && x.BuildNo == buildno
                              select x;
                int validUICount = listAll.Count();
                int selectedUICount = 0;
                foreach (var x in listAll)
                {
                    if (selectedLanguage == "All" && selectedOSType == "All")
                    {
                        if (entity.ViewAssemblySummaries.Any(c => builds.Contains(c.BuildNo.Trim()) && c.AssemblyName == x.AssemblyName
                            && c.FullTypeName == x.TypeName))
                        {
                            selectedUICount++;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if (selectedLanguage != "All" && selectedOSType == "All")
                    {
                        if (entity.ViewAssemblySummaries.Any(c => builds.Contains(c.BuildNo.Trim()) && c.AssemblyName == x.AssemblyName
                            && c.FullTypeName == x.TypeName && c.Language == selectedLanguage))
                        {
                            selectedUICount++;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if (selectedLanguage == "All" && selectedOSType != "All")
                    {
                        if (entity.ViewAssemblySummaries.Any(c => builds.Contains(c.BuildNo.Trim()) && c.AssemblyName == x.AssemblyName
                            && c.FullTypeName == x.TypeName && c.OSType == selectedOSType))
                        {
                            selectedUICount++;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if (selectedLanguage != "All" && selectedOSType != "All")
                    {
                        if (entity.ViewAssemblySummaries.Any(c => builds.Contains(c.BuildNo.Trim()) && c.AssemblyName == x.AssemblyName
                            && c.FullTypeName == x.TypeName && c.OSType == selectedOSType && c.Language == selectedLanguage))
                        {
                            selectedUICount++;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
                int missed = validUICount - selectedUICount;
                List<DataPoint> listDp = new List<DataPoint>();
                DataPoint dpSelected = new DataPoint();
                dpSelected.AxisLabel = "Captured";
                dpSelected.YValues = new double[] { selectedUICount };

                DataPoint dpMissed = new DataPoint();
                dpMissed.AxisLabel = "Missed";
                dpMissed.YValues = new double[] { missed };

                listDp.Add(dpSelected);
                listDp.Add(dpMissed);
                #endregion

                List<DataPoint> listDpFail = new List<DataPoint>();
                List<DataPoint> listDpPass = new List<DataPoint>();
                List<DataPoint> listDpWarning = new List<DataPoint>();

                if (selectedLanguage != "All" && selectedOSType == "All")
                {
                    foreach (KeyValuePair<string, string> s in ruleDir)
                    {
                        DataPoint temp1 = new DataPoint();
                        temp1.AxisLabel = s.Key;

                        int fail = (from x in entity.ResultsSummaries
                                    where x.Language == selectedLanguage && builds.Contains(x.BuildNo.Trim()) && x.RuleName == s.Value && x.ResultType == "Fail"
                                    select x.ResultID).Distinct().Count();
                        temp1.YValues = new double[] { fail };
                        listDpFail.Add(temp1);

                        DataPoint temp2 = new DataPoint();
                        temp2.AxisLabel = s.Key;
                        int pass = (from x in entity.ResultsSummaries
                                    where x.Language == selectedLanguage && builds.Contains(x.BuildNo.Trim()) && x.RuleName == s.Value && x.ResultType == "Pass"
                                    select x.ResultID).Distinct().Count();
                        temp2.YValues = new double[] { pass };
                        listDpPass.Add(temp2);

                        DataPoint temp3 = new DataPoint();
                        temp3.AxisLabel = s.Key;
                        int warning = (from x in entity.ResultsSummaries
                                       where x.Language == selectedLanguage && builds.Contains(x.BuildNo.Trim()) && x.RuleName == s.Value && x.ResultType == "Warning"
                                       select x.ResultID).Distinct().Count();
                        temp3.YValues = new double[] { warning };
                        listDpWarning.Add(temp3);
                    }
                }
                else if (selectedLanguage == "All" && selectedOSType == "All")
                {
                    foreach (KeyValuePair<string, string> s in ruleDir)
                    {
                        DataPoint temp1 = new DataPoint();
                        temp1.AxisLabel = s.Key;

                        int fail = (from x in entity.ResultsSummaries
                                    where builds.Contains(x.BuildNo.Trim()) && x.RuleName == s.Value && x.ResultType == "Fail"
                                    select x.ResultID).Distinct().Count();
                        temp1.YValues = new double[] { fail };
                        listDpFail.Add(temp1);

                        DataPoint temp2 = new DataPoint();
                        temp2.AxisLabel = s.Key;
                        int pass = (from x in entity.ResultsSummaries
                                    where builds.Contains(x.BuildNo.Trim()) && x.RuleName == s.Value && x.ResultType == "Pass"
                                    select x.ResultID).Distinct().Count();
                        temp2.YValues = new double[] { pass };
                        listDpPass.Add(temp2);

                        DataPoint temp3 = new DataPoint();
                        temp3.AxisLabel = s.Key;
                        int warning = (from x in entity.ResultsSummaries
                                       where builds.Contains(x.BuildNo.Trim()) && x.RuleName == s.Value && x.ResultType == "Warning"
                                       select x.ResultID).Distinct().Count();
                        temp3.YValues = new double[] { warning };
                        listDpWarning.Add(temp3);
                    }
                }
                else if (selectedLanguage == "All" && selectedOSType != "All")
                {
                    foreach (KeyValuePair<string, string> s in ruleDir)
                    {
                        DataPoint temp1 = new DataPoint();
                        temp1.AxisLabel = s.Key;

                        int fail = (from x in entity.ResultsSummaries
                                    where x.OSType == selectedOSType && builds.Contains(x.BuildNo.Trim()) && x.RuleName == s.Value && x.ResultType == "Fail"
                                    select x.ResultID).Distinct().Count();
                        temp1.YValues = new double[] { fail };
                        listDpFail.Add(temp1);

                        DataPoint temp2 = new DataPoint();
                        temp2.AxisLabel = s.Key;
                        int pass = (from x in entity.ResultsSummaries
                                    where x.OSType == selectedOSType && builds.Contains(x.BuildNo.Trim()) && x.RuleName == s.Value && x.ResultType == "Pass"
                                    select x.ResultID).Distinct().Count();
                        temp2.YValues = new double[] { pass };
                        listDpPass.Add(temp2);

                        DataPoint temp3 = new DataPoint();
                        temp3.AxisLabel = s.Key;
                        int warning = (from x in entity.ResultsSummaries
                                       where x.OSType == selectedOSType && builds.Contains(x.BuildNo.Trim()) && x.RuleName == s.Value && x.ResultType == "Warning"
                                       select x.ResultID).Distinct().Count();
                        temp3.YValues = new double[] { warning };
                        listDpWarning.Add(temp3);
                    }
                }
                else if (selectedLanguage != "All" && selectedOSType != "All")
                {
                    foreach (KeyValuePair<string, string> s in ruleDir)
                    {
                        DataPoint temp1 = new DataPoint();
                        temp1.AxisLabel = s.Key;

                        int fail = (from x in entity.ResultsSummaries
                                    where x.OSType == selectedOSType && x.Language == selectedLanguage && builds.Contains(x.BuildNo.Trim()) && x.RuleName == s.Value && x.ResultType == "Fail"
                                    select x.ResultID).Distinct().Count();
                        temp1.YValues = new double[] { fail };
                        listDpFail.Add(temp1);

                        DataPoint temp2 = new DataPoint();
                        temp2.AxisLabel = s.Key;
                        int pass = (from x in entity.ResultsSummaries
                                    where x.OSType == selectedOSType && x.Language == selectedLanguage && builds.Contains(x.BuildNo.Trim()) && x.RuleName == s.Value && x.ResultType == "Pass"
                                    select x.ResultID).Distinct().Count();
                        temp2.YValues = new double[] { pass };
                        listDpPass.Add(temp2);

                        DataPoint temp3 = new DataPoint();
                        temp3.AxisLabel = s.Key;
                        int warning = (from x in entity.ResultsSummaries
                                       where x.OSType == selectedOSType && x.Language == selectedLanguage && builds.Contains(x.BuildNo.Trim()) && x.RuleName == s.Value && x.ResultType == "Warning"
                                       select x.ResultID).Distinct().Count();
                        temp3.YValues = new double[] { warning };
                        listDpWarning.Add(temp3);
                    }
                }

                string path = Server.MapPath(".");
                string name1 = "totalsummary.jpeg";
                string path1 = System.IO.Path.Combine(path, "img", name1);
                GeneratePlot(listDp, path1, 430, 200);
                string name2 = "misssummary.jpeg";
                string path2 = System.IO.Path.Combine(path, "img", name2);
                Dictionary<string, IList<DataPoint>> seriesList = new Dictionary<string, IList<DataPoint>>();
                seriesList.Add("Pass", listDpPass);
                seriesList.Add("Warning", listDpWarning);
                seriesList.Add("Fail", listDpFail);
                GeneratePlot(seriesList, path2, 855, 450);

                Response.Write(string.Format("{0}[|]{1}", "img/" + name1, "img/" + name2));
            }

        }
        public void QueryPagesSummaryDataNew()
        {
            string selectedLanguage = this.Request.Form["SelectedLanguage"];
            string selectedOSType = this.Request.Form["OSType"];
            string selectedBuild = this.Request.Form["SelectedBuild"];
            List<string> builds = selectedBuild.Split('|').ToList();
            string buildno = this.Request.Form["ReferencedBuild"];

            UpdateMarkOnBuildTypes(buildno);
        //    SulpHurDBReader.SulpHurDBReader dbReader = new SulpHurDBReader.SulpHurDBReader();

            ISulpHurTable dbReader = SulpHurTableFactoryBase.Instance().GetSulpHurTable();

            string sqlValidUICount =
                string.Format("select count(*) from buildtypes as F where mark in ('Valid UI (Missing Capture)','Captured') and buildno='{0}'", buildno);
            int validUICount = (int)dbReader.ExecuteScalar(sqlValidUICount);

            string prefix = sqlValidUICount+" and Exists (select distinct b.fulltypename from assemblylink as A "
                + joinAssemblyInfoTable + joinContentTable + joinBuildInfoTable + joinClientsTable;
            string sqlSelectedUI = string.Format(prefix + formatbuildno0NoAnd + formatlanguage1 + formatostype2
                +"Where F.typename=b.fulltypename)"
                , GenerateSQLCondition(selectedBuild)
                , GenerateSQLCondition(selectedLanguage)
                , GenerateSQLCondition(selectedOSType));

            int selectedUICount = (int)dbReader.ExecuteScalar(sqlSelectedUI);
            int missed = validUICount - selectedUICount;
            List<DataPoint> listDp = new List<DataPoint>();
            DataPoint dpSelected = new DataPoint();
            dpSelected.AxisLabel = "Captured";
            dpSelected.YValues = new double[] { selectedUICount };

            DataPoint dpMissed = new DataPoint();
            dpMissed.AxisLabel = "Missed";
            dpMissed.YValues = new double[] { missed };

            listDp.Add(dpSelected);
            listDp.Add(dpMissed);

            List<DataPoint> listDpFail = new List<DataPoint>();
            List<DataPoint> listDpPass = new List<DataPoint>();
            List<DataPoint> listDpWarning = new List<DataPoint>();

            foreach (KeyValuePair<string, string> s in ruleDir)
            {
                DataPoint temp1 = new DataPoint();
                temp1.AxisLabel = s.Key;

                prefix = "select count(distinct A.resultid) FROM [dbo].[Results] AS A "
                + joinRuleTable + joinContentTable + joinBuildInfoTable + joinClientsTable + "Where A.ResultType='Fail' ";
                prefix = prefix + " and G.RuleName='" + s.Value + "' ";
                string failCountSql = string.Format(prefix + formatbuildno0 + formatlanguage1 + formatostype2
                , GenerateSQLCondition(selectedBuild)
                , GenerateSQLCondition(selectedLanguage)
                , GenerateSQLCondition(selectedOSType));
                int fail = (int)dbReader.ExecuteScalar(failCountSql);
                temp1.YValues = new double[] { fail };
                listDpFail.Add(temp1);

                DataPoint temp2 = new DataPoint();
                temp2.AxisLabel = s.Key;
                prefix = "select count(distinct A.resultid) FROM [dbo].[Results] AS A "
                    + joinRuleTable + joinContentTable + joinBuildInfoTable + joinClientsTable + "Where A.ResultType='Pass' ";
                prefix = prefix + " and G.RuleName='" + s.Value + "' ";
                string passCountSql = string.Format(prefix + formatbuildno0 + formatlanguage1 + formatostype2
                , GenerateSQLCondition(selectedBuild)
                , GenerateSQLCondition(selectedLanguage)
                , GenerateSQLCondition(selectedOSType));
                int pass = (int)dbReader.ExecuteScalar(passCountSql);
                temp2.YValues = new double[] { pass };
                listDpPass.Add(temp2);

                DataPoint temp3 = new DataPoint();
                temp3.AxisLabel = s.Key;
                prefix = "select count(distinct A.resultid) FROM [dbo].[Results] AS A "
                    + joinRuleTable + joinContentTable + joinBuildInfoTable + joinClientsTable + "Where A.ResultType='Warning' ";
                prefix = prefix + " and G.RuleName='" + s.Value + "' ";
                string warningCountSql = string.Format(prefix + formatbuildno0 + formatlanguage1 + formatostype2
                , GenerateSQLCondition(selectedBuild)
                , GenerateSQLCondition(selectedLanguage)
                , GenerateSQLCondition(selectedOSType));
                int warning = (int)dbReader.ExecuteScalar(warningCountSql);
                listDpWarning.Add(temp3);
            }

            string path = Server.MapPath(".");
            string name1 = "totalsummary.jpeg";
            string path1 = System.IO.Path.Combine(path, "img", name1);
            GeneratePlot(listDp, path1, 430, 200);
            string name2 = "misssummary.jpeg";
            string path2 = System.IO.Path.Combine(path, "img", name2);
            Dictionary<string, IList<DataPoint>> seriesList = new Dictionary<string, IList<DataPoint>>();
            seriesList.Add("Pass", listDpPass);
            seriesList.Add("Warning", listDpWarning);
            seriesList.Add("Fail", listDpFail);
            GeneratePlot(seriesList, path2, 855, 450);

            Response.Write(string.Format("{0}[|]{1}", "img/" + name1, "img/" + name2));
        }
        public void GetProductTypes()
        {
            string consolePath = this.Request.Form["ConsolePath"];
            bool isoverwrite = Convert.ToBoolean(this.Request.Form["IsOverWrite"]);
            if (!Directory.Exists(consolePath))
            {
                ShowError("Folder not exists");
                return;
            }
        }
        public void StartRun()
        {
            List<Results> preMofList = ParseMofFile(preMofFilePath);
            List<Results> postMofList = ParseMofFile(postMofFilePath);
            List<Results> results = AssignDiffType(preMofList, postMofList);

            GetXmlFiles();
            ProcessXMLFile(preMofList, postMofList, results);

            GetCSFiles();
            Dictionary<string, string> fileTargetAssembly = GetFileTargetAssembly();
            ProcessCSFile(preMofList, postMofList, results, fileTargetAssembly);
            DetectUnusedClass(results);
            HttpContext.Current.Session["isRunning"] = false;
        }
        private string ObjectToString(List<BuildType> list) {
            string r = string.Empty;
            return r;
        }
        private void TryGetAssembly(string f, out string assembly)
        {
            try
            {
                XPathDocument document = new XPathDocument(f);
                XPathNavigator nav = document.CreateNavigator();
                XPathNodeIterator iterator = nav.SelectDescendants("Assembly", "http://schemas.microsoft.com/SystemsManagementServer/2005/03/ConsoleFramework", false);
                string s = "";
                while (iterator.MoveNext())
                {
                    s = iterator.Current.GetAttribute("Name", string.Empty);
                }
                if (!s.EndsWith(".dll") && !string.IsNullOrEmpty(s))
                {
                    s += ".dll";
                }
                assembly = s;
            }
            catch
            {
                assembly = "";
            }
        }
        private void TryGetPages(string f, out string page)
        {
            try
            {
                XPathDocument document = new XPathDocument(f);
                XPathNavigator nav = document.CreateNavigator();
                XPathNodeIterator iteratorNamespace = nav.SelectDescendants("Assembly", "http://schemas.microsoft.com/SystemsManagementServer/2005/03/ConsoleFramework", false);
                string namespacePrefix = "";
                while (iteratorNamespace.MoveNext())
                {
                    namespacePrefix = iteratorNamespace.Current.GetAttribute("Namespace", string.Empty);
                }

                XPathNodeIterator iterator = nav.SelectDescendants("Page", "http://schemas.microsoft.com/SystemsManagementServer/2005/03/ConsoleFramework", false);
                string s = "";
                while (iterator.MoveNext())
                {
                    s = iterator.Current.GetAttribute("Type", string.Empty);
                }
                s = namespacePrefix + "." + s;
                page = s;
            }
            catch
            {
                page = "";
            }
        }
        private void ProcessCSFile(List<Results> preMofList, List<Results> postMofList, List<Results> results, Dictionary<string, string> fileTargetAssembly)
        {
            foreach (string f in csFiles)
            {
                string text = System.IO.File.ReadAllText(f);
                foreach (Results r in results)
                {
                    string key = r.WmiClassName;
                    int keyIndex = text.IndexOf(key);
                    while (keyIndex >= 0)
                    {
                        int start = text.Substring(0, keyIndex).LastIndexOf("\r\n") + "\r\n".Length;
                        int end = text.IndexOf("\r\n", keyIndex);
                        string sLine = "";
                        if (end == -1)
                        {
                            sLine = text.Substring(start).Trim();
                        }
                        else
                        {
                            sLine = text.Substring(start, end - start).Trim();
                        }
                        if (sLine.StartsWith("//") || sLine.StartsWith("///"))
                        {
                            keyIndex = text.IndexOf(key, keyIndex + key.Length);
                            continue;
                        }
                        Results resTemp = new Results();
                        resTemp.SourceFileName = f;
                        resTemp.WmiClassName = key;
                        if (!usedWMIClassList.Contains(key))
                        {
                            usedWMIClassList.Add(key);
                        }

                        string assembly = "";
                        fileTargetAssembly.TryGetValue(f.ToUpper(), out assembly);
                        if (!string.IsNullOrEmpty(assembly))
                        {
                            resTemp.Assembly = assembly;
                        }
                        else
                        {
                            Console.WriteLine("file not complied.");
                        }

                        resTemp.DiffTypes = r.DiffTypes;

                        if (r.DiffTypes == DiffType.Modified || r.DiffTypes == DiffType.None)
                        {
                            if (!string.IsNullOrEmpty(resTemp.Assembly) && !assemblies.Contains(assembly))
                            {
                                assemblies.Add(assembly);
                            }
                            string oldClass = preMofList.Single(s => s.WmiClassName == r.WmiClassName).ClassDefinition;
                            resTemp.OldClass = oldClass;
                            string newClass = postMofList.Single(s => s.WmiClassName == r.WmiClassName).ClassDefinition;
                            resTemp.NewClass = newClass;
                        }
                        if (r.DiffTypes == DiffType.Added)
                        {
                            resTemp.OldClass = "N/A";
                            string newClass = postMofList.Single(s => s.WmiClassName == r.WmiClassName).ClassDefinition;
                            resTemp.NewClass = newClass;
                        }
                        if (r.DiffTypes == DiffType.Removed)
                        {
                            string oldClass = preMofList.Single(s => s.WmiClassName == r.WmiClassName).ClassDefinition;
                            resTemp.OldClass = oldClass;
                            resTemp.NewClass = "N/A";
                        }

                        AddRow(resTemp);
                        break;
                    }
                }
            }
        }
        private void ProcessXMLFile(List<Results> preMofList, List<Results> postMofList, List<Results> results)
        {
            usedWMIClassList = new List<string>();
            assemblies = new List<string>();

            foreach (string f in xmlFiles)
            {
                if (f.StartsWith(System.IO.Path.Combine(sourceCodeFolder, "bin"), StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }
                if (f.StartsWith(System.IO.Path.Combine(sourceCodeFolder, "XmlStorage"), StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }
                string text = System.IO.File.ReadAllText(f);

                foreach (Results r in results)
                {
                    string key = r.WmiClassName;
                    int keyIndex = text.IndexOf(key);

                    while (keyIndex >= 0)
                    {
                        Results resTemp = new Results();
                        resTemp.SourceFileName = f;
                        resTemp.WmiClassName = key;
                        if (!usedWMIClassList.Contains(key))
                        {
                            usedWMIClassList.Add(key);
                        }
                        string assembly = "";
                        TryGetAssembly(f, out assembly);
                        resTemp.Assembly = assembly;

                        //string page="";
                        //TryGetPages(f, out page);
                        //resTemp.PageName = page;
                        resTemp.DiffTypes = r.DiffTypes;

                        if (r.DiffTypes == DiffType.Modified || r.DiffTypes == DiffType.None)
                        {
                            string oldClass = preMofList.Single(s => s.WmiClassName == r.WmiClassName).ClassDefinition;
                            resTemp.OldClass = oldClass;
                            string newClass = postMofList.Single(s => s.WmiClassName == r.WmiClassName).ClassDefinition;
                            resTemp.NewClass = newClass;
                        }
                        if (r.DiffTypes == DiffType.Added)
                        {
                            resTemp.OldClass = "N/A";
                            string newClass = postMofList.Single(s => s.WmiClassName == r.WmiClassName).ClassDefinition;
                            resTemp.NewClass = newClass;
                        }
                        if (r.DiffTypes == DiffType.Removed)
                        {
                            string oldClass = preMofList.Single(s => s.WmiClassName == r.WmiClassName).ClassDefinition;
                            resTemp.OldClass = oldClass;
                            resTemp.NewClass = "N/A";
                        }
                        AddRow(resTemp);
                        break;
                    }
                }
            }
        }
        private void GeneratePlot(IList<DataPoint> series, string path, int width, int height)
        {
            using (var ch = new Chart())
            {
                Title t = new System.Web.UI.DataVisualization.Charting.Title("Summary", Docking.Top, new Font("Segoe UI", 12, FontStyle.Bold), Color.FromArgb(102, 153, 204));
                ch.Titles.Add(t);
                ChartArea area = new ChartArea();
                ChartArea3DStyle style = new ChartArea3DStyle();
                style.Enable3D = true;
                style.IsClustered = true;
                style.Perspective = 15;
                style.Inclination = 60;
                area.Area3DStyle = style;
                ch.ChartAreas.Add(area);
                var s = new Series();
                s.ChartType = SeriesChartType.Pie;
                s.IsValueShownAsLabel = true;
                //SmartLabelStyle smart=new SmartLabelStyle();
                //smart.Enabled=true;
                //smart.IsMarkerOverlappingAllowed=false;
                //smart.MaxMovingDistance=50;
                //smart.CalloutLineWidth = 1;
                //smart.MinMovingDistance = 30;
                //s.SmartLabelStyle = smart;
                foreach (var pnt in series) s.Points.Add(pnt);
                //s["PieLabelStyle"] = "Outside";
                ch.Series.Add(s);
                Legend legend = new Legend("Lengend1");
                ch.Legends.Add(legend);
                s.Legend = "Lengend1";
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                ch.Width = width;
                ch.Height = height;
                ch.Palette = ChartColorPalette.BrightPastel;

                ch.SaveImage(path, ChartImageFormat.Jpeg);
            }
        }
        private void GeneratePlot(Dictionary<string,IList<DataPoint>> seriesList, string path, int width, int height)
        {
            using (var ch = new Chart())
            {
                Title t = new System.Web.UI.DataVisualization.Charting.Title("Captured UI Results", Docking.Top, new Font("Segoe UI", 12, FontStyle.Bold), Color.FromArgb(102, 153, 204));
                ch.Titles.Add(t);
                ChartArea area = new ChartArea();
                ch.ChartAreas.Add(area);
                Legend legend = new Legend("Lengend1");
                ch.Legends.Add(legend);
                foreach (KeyValuePair<string, IList<DataPoint>> pair in seriesList)
                {
                    var sTemp = new Series();
                    sTemp.LabelFormat = "{0;0;#}";
                    sTemp.Name = pair.Key;
                    sTemp.ChartType = SeriesChartType.StackedColumn100;
                    sTemp.IsValueShownAsLabel = true;
                    foreach (var pnt in pair.Value) {
                        //if (pnt.YValues[0] != 0)
                        //{
                            sTemp.Points.Add(pnt);
                        //}
                    
                    }
                    sTemp.Legend = "Lengend1";
                    ch.Series.Add(sTemp);
                }

                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                ch.Width = width;
                ch.Height = height;
                ch.Palette = ChartColorPalette.BrightPastel;
                ch.SaveImage(path, ChartImageFormat.Jpeg);
            }
        }
        private void GetCSFiles()
        {
            csFiles = Directory.GetFiles(sourceCodeFolder, "*.cs", SearchOption.AllDirectories);
        }
        private void DetectUnusedClass(List<Results> results)
        {
            foreach (Results r in results)
            {
                if (!usedWMIClassList.Contains(r.WmiClassName))
                {
                    //r.DiffTypes = DiffType.Unused;
                    Results temp = new Results();
                    temp = r;
                    AddRow(temp);
                }
            }
        }
        private void AddRow(Results r)
        {
            ProviderDiffResult result = new ProviderDiffResult();
            result.DiffType = r.DiffTypes.ToString();
            result.WmiClassName = r.WmiClassName;
            if (!string.IsNullOrEmpty(r.WmiClassBody))
            {
                result.WmiClassBody = r.WmiClassBody;
            }
            else
            {
                result.WmiClassBody = string.Empty;
            }
            if (!string.IsNullOrEmpty(r.SourceFileName))
            {

                string fileName = r.SourceFileName.Substring(r.SourceFileName.LastIndexOf("\\") + 1);
                result.SourceFileName = fileName;
                if (!r.SourceFileName.EndsWith(".xml", StringComparison.CurrentCultureIgnoreCase))
                {
                    result.PageName = fileName.Substring(0, fileName.Length - ".cs".Length);
                }
            }
            else
            {
                result.SourceFileName = string.Empty;
                result.PageName = "N/A";
            }

            if (!string.IsNullOrEmpty(r.Assembly))
            {
                result.AssemblyName = r.Assembly;
            }
            else
            {
                result.AssemblyName = "N/A";
            }

            if (!string.IsNullOrEmpty(r.WmiClassBody))
            {
                result.WmiClassBody = r.WmiClassBody;
            }
            else
            {
                result.WmiClassBody = "N/A";
            }

            if (!string.IsNullOrEmpty(r.NewClass))
            {
                result.NewClass = r.NewClass;
            }
            else
            {
                result.NewClass = "N/A";
            }


            if (!string.IsNullOrEmpty(r.OldClass))
            {
                result.OldClass = r.OldClass;
            }
            else
            {
                result.OldClass = "N/A";
            }

            result.ComparedBuild = this.ComparedBuilds;

            entities.ProviderDiffResults.AddObject(result);
            entities.SaveChanges();
        }
        private bool IfLocalMofExist()
        {
            return File.Exists(preMofFilePath) && File.Exists(postMofFilePath);
        }
        private void CopyMof(string buildNum)
        {
            string novashafs01 = System.Configuration.ConfigurationManager.AppSettings["novashafs01"];
            //string zizdfsr01 = System.Configuration.ConfigurationManager.AppSettings["ziz-dfsr01"];
            mileStone = System.Configuration.ConfigurationManager.AppSettings["milestone"].ToString().Split('|').ToList();

            List<string> mofServers = new List<string>() { 
                //System.IO.Path.Combine(novashafs01,Constant.sccmv5),
                //System.IO.Path.Combine(novashafs01,Constant.sccmv5sp1),
                //System.IO.Path.Combine(zizdfsr01,Constant.sccmv5),
                //System.IO.Path.Combine(zizdfsr01,Constant.sccmv5sp1)
            };
            foreach (string x in mileStone) {
                mofServers.Add(System.IO.Path.Combine(novashafs01, x));
            }

            foreach (string s in mofServers)
            {
                if (CopyIsMofExistInServer(s, buildNum))
                {
                    copySuccess = true;
                    break;
                }
            }
        }
        private void ShowError(string msg)
        {
            Response.Write(string.Format("[E]:{0}", msg));
        }
        private void CopyToLocal(string path, string desName)
        {
            string desFile = System.IO.Path.Combine(Environment.CurrentDirectory, desName);
            System.IO.File.Copy(path, desFile, true);
        }
        private string ParseName(string sLine)
        {
            int i = sLine.IndexOf(" ");
            int j = sLine.IndexOf(":");
            if (j == -1)
            {
                j = sLine.IndexOf(" ", i + 1);
            }
            string className = "";
            if (j == -1)
            {
                className = sLine.Substring(i).Trim();
            }
            else
            {
                className = sLine.Substring(i, j - i).Trim();
            }
            return className;
        }
        private List<Results> ParseMofFile(string path)
        {
            List<Results> wmiClassList = new List<Results>();
            Results mofTemp;
            string text = System.IO.File.ReadAllText(path);
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);
            string sLine = "";
            string previousLine = "";
            while (sLine != null)
            {
                if (sLine.Trim().StartsWith("class", StringComparison.CurrentCultureIgnoreCase))
                {
                    mofTemp = new Results();
                    string className = ParseName(sLine);
                    int startIndex = text.IndexOf(sLine.Trim());
                    int u = text.IndexOf("};", startIndex);
                    if (previousLine.Contains("]"))
                    {
                        while (text[startIndex] != '[')
                        {
                            startIndex = startIndex - 1;
                        }
                    }
                    mofTemp.ClassDefinition = text.Substring(startIndex, u - startIndex + "};".Length);
                    mofTemp.WmiClassName = className;
                    //int i = text.IndexOf(className);
                    //i = text.IndexOf("{", i) + 1;
                    //int j = text.IndexOf("};", i);
                    mofTemp.WmiClassBody = mofTemp.ClassDefinition.Replace(" ", "").Replace("\r\n", "").Replace("\t", "");

                    mofTemp.DiffTypes = DiffType.None;
                    wmiClassList.Add(mofTemp);
                }
                previousLine = sLine;
                sLine = sr.ReadLine();
            }
            return wmiClassList;
        }
        private List<Results> AssignDiffType(List<Results> preMofList, List<Results> postMofList)
        {
            List<Results> temp = new List<Results>();

            //detect change class
            foreach (Results mc1 in postMofList)
            {
                foreach (Results mc2 in preMofList)
                {
                    if (!temp.Select(item => item.WmiClassName).ToArray().Contains(mc1.WmiClassName))
                    {
                        temp.Add(mc1);
                    }
                    if (!temp.Select(item => item.WmiClassName).ToArray().Contains(mc2.WmiClassName))
                    {
                        temp.Add(mc2);
                    }

                    if (mc1.WmiClassName == mc2.WmiClassName && mc1.WmiClassBody != mc2.WmiClassBody)
                    {
                        temp.Single(item => item.WmiClassName == mc1.WmiClassName).DiffTypes = DiffType.Modified;
                        temp.Single(item => item.WmiClassName == mc1.WmiClassName).NewClass = mc1.ClassDefinition;
                        temp.Single(item => item.WmiClassName == mc1.WmiClassName).OldClass = mc2.ClassDefinition;
                    }

                    if (mc1.WmiClassName == mc2.WmiClassName && mc1.WmiClassBody == mc2.WmiClassBody)
                    {
                        temp.Single(item => item.WmiClassName == mc1.WmiClassName).NewClass = mc1.ClassDefinition;
                        temp.Single(item => item.WmiClassName == mc1.WmiClassName).OldClass = mc2.ClassDefinition;
                    }
                }
            }

            //detect new added class
            foreach (Results mc1 in postMofList)
            {
                bool find = false;
                foreach (Results mc2 in preMofList)
                {
                    if (mc1.WmiClassName == mc2.WmiClassName)
                    {
                        find = true;
                        break;
                    }
                }
                if (!find)
                {
                    temp.Single(item => item.WmiClassName == mc1.WmiClassName).DiffTypes = DiffType.Added;
                    temp.Single(item => item.WmiClassName == mc1.WmiClassName).NewClass = mc1.ClassDefinition;
                    temp.Single(item => item.WmiClassName == mc1.WmiClassName).OldClass = "N/A";
                }
            }

            //removed class
            foreach (Results mc2 in preMofList)
            {
                bool find = false;
                foreach (Results mc1 in postMofList)
                {
                    if (mc1.WmiClassName == mc2.WmiClassName)
                    {
                        find = true;
                        break;
                    }
                }
                if (!find)
                {
                    temp.Single(item => item.WmiClassName == mc2.WmiClassName).DiffTypes = DiffType.Removed;
                    temp.Single(item => item.WmiClassName == mc2.WmiClassName).NewClass = "N/A";
                    temp.Single(item => item.WmiClassName == mc2.WmiClassName).OldClass = mc2.ClassDefinition;
                }
            }

            return temp;
        }
        private void GetXmlFiles()
        {
            xmlFiles = Directory.GetFiles(sourceCodeFolder, "*.xml", SearchOption.AllDirectories);
        }
        private Dictionary<string, string> GetFileTargetAssembly()
        {
            string[] projFiles = Directory.GetFiles(System.IO.Path.Combine(sourceCodeFolder), "*.csproj", SearchOption.AllDirectories);
            Dictionary<string, string> dir = new Dictionary<string, string>();
            foreach (string f in projFiles)
            {
                XmlDocument xmldoc = new XmlDocument();
                try
                {
                    xmldoc.Load(f);
                }
                catch
                {
                    Console.WriteLine("load csproj file failed.");
                    continue;
                }

                XmlNamespaceManager mgr = new XmlNamespaceManager(xmldoc.NameTable);
                mgr.AddNamespace("x", "http://schemas.microsoft.com/developer/msbuild/2003");

                string assemblyName = "";
                foreach (XmlNode item in xmldoc.SelectNodes("//x:AssemblyName", mgr))
                {
                    assemblyName = item.InnerText.ToString();
                }
                if (!string.IsNullOrEmpty(assemblyName))
                {
                    assemblyName += ".dll";
                }
                else
                {
                    Console.WriteLine("assembly name should not be null");
                    continue;
                }
                FileInfo fi = new FileInfo(f);
                string dicName = fi.DirectoryName;
                foreach (XmlNode item in xmldoc.SelectNodes("//x:Compile", mgr))
                {
                    string includeFile = item.Attributes["Include"].Value.ToString();
                    includeFile = System.IO.Path.Combine(dicName, includeFile).ToString();
                    if (includeFile.EndsWith(".cs"))
                    {
                        includeFile = includeFile.ToUpper();
                        dir.Add(includeFile, assemblyName);
                    }
                    else
                    {
                        Console.WriteLine("include file is not a cs file");
                    }
                }
            }
            return dir;
        }
        private bool CopyIsMofExistInServer(string server, string build)
        {
            if (System.IO.Directory.Exists(System.IO.Path.Combine(server, build)))
            {
                string buildFolder = System.IO.Path.Combine(server, build);
                if (System.IO.Directory.Exists(System.IO.Path.Combine(buildFolder, Constant.cdretail)))
                {
                    string filePath = System.IO.Path.Combine(System.IO.Path.Combine(buildFolder, Constant.cdretail), Constant.mofPath);
                    CopyToLocal(filePath, build + ".mof");
                    return true;
                }
                else if (System.IO.Directory.Exists(System.IO.Path.Combine(buildFolder, Constant.cddebug)))
                {
                    string filePath = System.IO.Path.Combine(System.IO.Path.Combine(buildFolder, Constant.cddebug), Constant.mofPath);
                    CopyToLocal(filePath, build + ".mof");
                    return true;
                }
            }
            return false;
        }
        private void HighlightClass(string preClass, string postClass, out string pre, out string post)
        {
            pre = string.Empty;
            post = string.Empty;
            Dictionary<int, string> dirNew = new Dictionary<int, string>();
            dirNew = ParseStringToParagraph(preClass);

            Dictionary<int, string> dirOld = new Dictionary<int, string>();
            dirOld = ParseStringToParagraph(postClass);

            Diff.Item[] items = Diff.DiffText(preClass, postClass, true, true, false);
            foreach (Diff.Item item in items)
            {
                for (int i = 0; i < item.deletedA; i++)
                {
                    dirNew[item.StartA + i] = "<b class=prebackground>" + dirNew[item.StartA + i] + "</b>";
                }
                for (int j = 0; j < item.insertedB; j++)
                {
                    try
                    {
                        dirOld[item.StartB + j] = "<b class=postbackground>" + dirOld[item.StartB + j] + "</b>";
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            foreach (KeyValuePair<int, string> pair in dirNew)
            {
                pre += pair.Value + "<br/>";
            }
            foreach (KeyValuePair<int, string> pair in dirOld)
            {
                post += pair.Value + "<br/>";
            }
        }
        private Dictionary<int, string> ParseStringToParagraph(string text)
        {
            Dictionary<int, string> temp = new Dictionary<int, string>();
            using (StringReader reader = new StringReader(text))
            {
                string line;
                int i = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    temp.Add(i, line);
                    i++;
                }
            }
            return temp;
        }
    }

    public class Constant
    {
        public static string cddebug = "cd.debug";
        public static string cdretail = "cd.retail";
        public static string sccmv5 = "SCCMv5";
        public static string sccmv5sp1 = "SCCMv5_SP1";
        public static string mofPath = @"SMSSETUP\BIN\X64\_smsprov.mof";
    }
    public enum DiffType
    {
        Added,
        Modified,
        Removed,
        Unused,
        None
    }
    public class Results
    {

        public Results()
        {
        }
        DiffType diffTypes = DiffType.None;

        public DiffType DiffTypes
        {
            get { return diffTypes; }
            set { diffTypes = value; }
        }

        string wmiClassName;

        public string WmiClassName
        {
            get { return wmiClassName; }
            set { wmiClassName = value; }
        }

        string wmiClassBody;

        public string WmiClassBody
        {
            get { return wmiClassBody; }
            set { wmiClassBody = value; }
        }
        string sourceFileName;

        public string SourceFileName
        {
            get { return sourceFileName; }
            set { sourceFileName = value; }
        }
        string impactedUI = "";

        public string ImpactedUI
        {
            get { return impactedUI; }
            set { impactedUI = value; }
        }

        string assembly = "";

        public string Assembly
        {
            get { return assembly; }
            set { assembly = value; }
        }

        string classDefinition;

        public string ClassDefinition
        {
            get { return classDefinition; }
            set { classDefinition = value; }
        }

        string oldClass;

        public string OldClass
        {
            get { return oldClass; }
            set { oldClass = value; }
        }

        string newClass;

        public string NewClass
        {
            get { return newClass; }
            set { newClass = value; }
        }

        string detailText;

        public string DetailText
        {
            get { return detailText; }
            set { detailText = value; }
        }

        string pageName;
        public string PageName
        {
            get
            {
                if (sourceFileName.EndsWith(".cs"))
                {
                    string fileName = sourceFileName.Substring(sourceFileName.LastIndexOf("\\") + 1);
                    return fileName.Substring(0, fileName.Length - ".cs".Length - 1);
                }
                else
                {
                    return pageName;
                }
            }
            set
            {
                this.pageName = value;
            }
        }

    }
    public class Diff
    {

        /// <summary>details of one difference.</summary>
        public struct Item
        {
            /// <summary>Start Line number in Data A.</summary>
            public int StartA;
            /// <summary>Start Line number in Data B.</summary>
            public int StartB;

            /// <summary>Number of changes in Data A.</summary>
            public int deletedA;
            /// <summary>Number of changes in Data A.</summary>
            public int insertedB;
        } // Item

        /// <summary>
        /// Shortest Middle Snake Return Data
        /// </summary>
        private struct SMSRD
        {
            internal int x, y;
            // internal int u, v;  // 2002.09.20: no need for 2 points 
        }


        #region self-Test

#if (SELFTEST)
    /// <summary>
    /// start a self- / box-test for some diff cases and report to the debug output.
    /// </summary>
    /// <param name="args">not used</param>
    /// <returns>always 0</returns>
    public static int Main(string[] args) {
      StringBuilder ret = new StringBuilder();
      string a, b;

      System.Diagnostics.ConsoleTraceListener ctl = new System.Diagnostics.ConsoleTraceListener(false);
      System.Diagnostics.Debug.Listeners.Add(ctl);

      System.Console.WriteLine("Diff Self Test...");
      
      // test all changes
      a = "a,b,c,d,e,f,g,h,i,j,k,l".Replace(',', '\n');
      b = "0,1,2,3,4,5,6,7,8,9".Replace(',', '\n');
      System.Diagnostics.Debug.Assert(TestHelper(Diff.DiffText(a, b, false, false, false))
        == "12.10.0.0*", 
        "all-changes test failed.");
      System.Diagnostics.Debug.WriteLine("all-changes test passed.");
      // test all same
      a = "a,b,c,d,e,f,g,h,i,j,k,l".Replace(',', '\n');
      b = a;
      System.Diagnostics.Debug.Assert(TestHelper(Diff.DiffText(a, b, false, false, false))
        == "",
        "all-same test failed.");
      System.Diagnostics.Debug.WriteLine("all-same test passed.");

      // test snake
      a = "a,b,c,d,e,f".Replace(',', '\n');
      b = "b,c,d,e,f,x".Replace(',', '\n');
      System.Diagnostics.Debug.Assert(TestHelper(Diff.DiffText(a, b, false, false, false))
        == "1.0.0.0*0.1.6.5*",
        "snake test failed.");
      System.Diagnostics.Debug.WriteLine("snake test passed.");

      // 2002.09.20 - repro
      a = "c1,a,c2,b,c,d,e,g,h,i,j,c3,k,l".Replace(',', '\n');
      b = "C1,a,C2,b,c,d,e,I1,e,g,h,i,j,C3,k,I2,l".Replace(',', '\n');
      System.Diagnostics.Debug.Assert(TestHelper(Diff.DiffText(a, b, false, false, false))
        == "1.1.0.0*1.1.2.2*0.2.7.7*1.1.11.13*0.1.13.15*",
        "repro20020920 test failed.");
      System.Diagnostics.Debug.WriteLine("repro20020920 test passed.");
      
      // 2003.02.07 - repro
      a = "F".Replace(',', '\n');
      b = "0,F,1,2,3,4,5,6,7".Replace(',', '\n');
      System.Diagnostics.Debug.Assert(TestHelper(Diff.DiffText(a, b, false, false, false))
        == "0.1.0.0*0.7.1.2*", 
        "repro20030207 test failed.");
      System.Diagnostics.Debug.WriteLine("repro20030207 test passed.");
      
      // Muegel - repro
      a = "HELLO\nWORLD";
      b = "\n\nhello\n\n\n\nworld\n";
      System.Diagnostics.Debug.Assert(TestHelper(Diff.DiffText(a, b, false, false, false))
        == "2.8.0.0*", 
        "repro20030409 test failed.");
      System.Diagnostics.Debug.WriteLine("repro20030409 test passed.");

    // test some differences
      a = "a,b,-,c,d,e,f,f".Replace(',', '\n');
      b = "a,b,x,c,e,f".Replace(',', '\n');
      System.Diagnostics.Debug.Assert(TestHelper(Diff.DiffText(a, b, false, false, false))
        == "1.1.2.2*1.0.4.4*1.0.6.5*", 
        "some-changes test failed.");
      System.Diagnostics.Debug.WriteLine("some-changes test passed.");

      System.Diagnostics.Debug.WriteLine("End.");
      System.Diagnostics.Debug.Flush();

      return (0);
    }


    public static string TestHelper(Item []f) {
      StringBuilder ret = new StringBuilder();
      for (int n = 0; n < f.Length; n++) {
        ret.Append(f[n].deletedA.ToString() + "." + f[n].insertedB.ToString() + "." + f[n].StartA.ToString() + "." + f[n].StartB.ToString() + "*");
      }
      // Debug.Write(5, "TestHelper", ret.ToString());
      return (ret.ToString());
    }
#endif
        #endregion


        /// <summary>
        /// Find the difference in 2 texts, comparing by textlines.
        /// </summary>
        /// <param name="TextA">A-version of the text (usualy the old one)</param>
        /// <param name="TextB">B-version of the text (usualy the new one)</param>
        /// <returns>Returns a array of Items that describe the differences.</returns>
        public Item[] DiffText(string TextA, string TextB)
        {
            return (DiffText(TextA, TextB, false, false, false));
        } // DiffText


        /// <summary>
        /// Find the difference in 2 text documents, comparing by textlines.
        /// The algorithm itself is comparing 2 arrays of numbers so when comparing 2 text documents
        /// each line is converted into a (hash) number. This hash-value is computed by storing all
        /// textlines into a common hashtable so i can find dublicates in there, and generating a 
        /// new number each time a new textline is inserted.
        /// </summary>
        /// <param name="TextA">A-version of the text (usualy the old one)</param>
        /// <param name="TextB">B-version of the text (usualy the new one)</param>
        /// <param name="trimSpace">When set to true, all leading and trailing whitespace characters are stripped out before the comparation is done.</param>
        /// <param name="ignoreSpace">When set to true, all whitespace characters are converted to a single space character before the comparation is done.</param>
        /// <param name="ignoreCase">When set to true, all characters are converted to their lowercase equivivalence before the comparation is done.</param>
        /// <returns>Returns a array of Items that describe the differences.</returns>
        public static Item[] DiffText(string TextA, string TextB, bool trimSpace, bool ignoreSpace, bool ignoreCase)
        {
            // prepare the input-text and convert to comparable numbers.
            Hashtable h = new Hashtable(TextA.Length + TextB.Length);

            // The A-Version of the data (original data) to be compared.
            DiffData DataA = new DiffData(DiffCodes(TextA, h, trimSpace, ignoreSpace, ignoreCase));

            // The B-Version of the data (modified data) to be compared.
            DiffData DataB = new DiffData(DiffCodes(TextB, h, trimSpace, ignoreSpace, ignoreCase));

            h = null; // free up hashtable memory (maybe)

            LCS(DataA, 0, DataA.Length, DataB, 0, DataB.Length);
            return CreateDiffs(DataA, DataB);
        } // DiffText


        /// <summary>
        /// Find the difference in 2 arrays of integers.
        /// </summary>
        /// <param name="ArrayA">A-version of the numbers (usualy the old one)</param>
        /// <param name="ArrayB">B-version of the numbers (usualy the new one)</param>
        /// <returns>Returns a array of Items that describe the differences.</returns>
        public static Item[] DiffInt(int[] ArrayA, int[] ArrayB)
        {
            // The A-Version of the data (original data) to be compared.
            DiffData DataA = new DiffData(ArrayA);

            // The B-Version of the data (modified data) to be compared.
            DiffData DataB = new DiffData(ArrayB);

            LCS(DataA, 0, DataA.Length, DataB, 0, DataB.Length);
            return CreateDiffs(DataA, DataB);
        } // Diff


        /// <summary>
        /// This function converts all textlines of the text into unique numbers for every unique textline
        /// so further work can work only with simple numbers.
        /// </summary>
        /// <param name="aText">the input text</param>
        /// <param name="h">This extern initialized hashtable is used for storing all ever used textlines.</param>
        /// <param name="trimSpace">ignore leading and trailing space characters</param>
        /// <returns>a array of integers.</returns>
        private static int[] DiffCodes(string aText, Hashtable h, bool trimSpace, bool ignoreSpace, bool ignoreCase)
        {
            // get all codes of the text
            string[] Lines;
            int[] Codes;
            int lastUsedCode = h.Count;
            object aCode;
            string s;

            // strip off all cr, only use lf as textline separator.
            aText = aText.Replace("\r", "");
            Lines = aText.Split('\n');

            Codes = new int[Lines.Length];

            for (int i = 0; i < Lines.Length; ++i)
            {
                s = Lines[i];
                if (trimSpace)
                    s = s.Trim();

                if (ignoreSpace)
                {
                    s = Regex.Replace(s, "\\s+", " ");            // TODO: optimization: faster blank removal.
                }

                if (ignoreCase)
                    s = s.ToLower();

                aCode = h[s];
                if (aCode == null)
                {
                    lastUsedCode++;
                    h[s] = lastUsedCode;
                    Codes[i] = lastUsedCode;
                }
                else
                {
                    Codes[i] = (int)aCode;
                } // if
            } // for
            return (Codes);
        } // DiffCodes


        /// <summary>
        /// This is the algorithm to find the Shortest Middle Snake (SMS).
        /// </summary>
        /// <param name="DataA">sequence A</param>
        /// <param name="LowerA">lower bound of the actual range in DataA</param>
        /// <param name="UpperA">upper bound of the actual range in DataA (exclusive)</param>
        /// <param name="DataB">sequence B</param>
        /// <param name="LowerB">lower bound of the actual range in DataB</param>
        /// <param name="UpperB">upper bound of the actual range in DataB (exclusive)</param>
        /// <returns>a MiddleSnakeData record containing x,y and u,v</returns>
        private static SMSRD SMS(DiffData DataA, int LowerA, int UpperA, DiffData DataB, int LowerB, int UpperB)
        {
            SMSRD ret;
            int MAX = DataA.Length + DataB.Length + 1;

            int DownK = LowerA - LowerB; // the k-line to start the forward search
            int UpK = UpperA - UpperB; // the k-line to start the reverse search

            int Delta = (UpperA - LowerA) - (UpperB - LowerB);
            bool oddDelta = (Delta & 1) != 0;

            /// vector for the (0,0) to (x,y) search
            int[] DownVector = new int[2 * MAX + 2];

            /// vector for the (u,v) to (N,M) search
            int[] UpVector = new int[2 * MAX + 2];

            // The vectors in the publication accepts negative indexes. the vectors implemented here are 0-based
            // and are access using a specific offset: UpOffset UpVector and DownOffset for DownVektor
            int DownOffset = MAX - DownK;
            int UpOffset = MAX - UpK;

            int MaxD = ((UpperA - LowerA + UpperB - LowerB) / 2) + 1;

            // Debug.Write(2, "SMS", String.Format("Search the box: A[{0}-{1}] to B[{2}-{3}]", LowerA, UpperA, LowerB, UpperB));

            // init vectors
            DownVector[DownOffset + DownK + 1] = LowerA;
            UpVector[UpOffset + UpK - 1] = UpperA;

            for (int D = 0; D <= MaxD; D++)
            {

                // Extend the forward path.
                for (int k = DownK - D; k <= DownK + D; k += 2)
                {
                    // Debug.Write(0, "SMS", "extend forward path " + k.ToString());

                    // find the only or better starting point
                    int x, y;
                    if (k == DownK - D)
                    {
                        x = DownVector[DownOffset + k + 1]; // down
                    }
                    else
                    {
                        x = DownVector[DownOffset + k - 1] + 1; // a step to the right
                        if ((k < DownK + D) && (DownVector[DownOffset + k + 1] >= x))
                            x = DownVector[DownOffset + k + 1]; // down
                    }
                    y = x - k;

                    // find the end of the furthest reaching forward D-path in diagonal k.
                    while ((x < UpperA) && (y < UpperB) && (DataA.data[x] == DataB.data[y]))
                    {
                        x++; y++;
                    }
                    DownVector[DownOffset + k] = x;

                    // overlap ?
                    if (oddDelta && (UpK - D < k) && (k < UpK + D))
                    {
                        if (UpVector[UpOffset + k] <= DownVector[DownOffset + k])
                        {
                            ret.x = DownVector[DownOffset + k];
                            ret.y = DownVector[DownOffset + k] - k;
                            // ret.u = UpVector[UpOffset + k];      // 2002.09.20: no need for 2 points 
                            // ret.v = UpVector[UpOffset + k] - k;
                            return (ret);
                        } // if
                    } // if

                } // for k

                // Extend the reverse path.
                for (int k = UpK - D; k <= UpK + D; k += 2)
                {
                    // Debug.Write(0, "SMS", "extend reverse path " + k.ToString());

                    // find the only or better starting point
                    int x, y;
                    if (k == UpK + D)
                    {
                        x = UpVector[UpOffset + k - 1]; // up
                    }
                    else
                    {
                        x = UpVector[UpOffset + k + 1] - 1; // left
                        if ((k > UpK - D) && (UpVector[UpOffset + k - 1] < x))
                            x = UpVector[UpOffset + k - 1]; // up
                    } // if
                    y = x - k;

                    while ((x > LowerA) && (y > LowerB) && (DataA.data[x - 1] == DataB.data[y - 1]))
                    {
                        x--; y--; // diagonal
                    }
                    UpVector[UpOffset + k] = x;

                    // overlap ?
                    if (!oddDelta && (DownK - D <= k) && (k <= DownK + D))
                    {
                        if (UpVector[UpOffset + k] <= DownVector[DownOffset + k])
                        {
                            ret.x = DownVector[DownOffset + k];
                            ret.y = DownVector[DownOffset + k] - k;
                            // ret.u = UpVector[UpOffset + k];     // 2002.09.20: no need for 2 points 
                            // ret.v = UpVector[UpOffset + k] - k;
                            return (ret);
                        } // if
                    } // if

                } // for k

            } // for D

            throw new ApplicationException("the algorithm should never come here.");
        } // SMS


        /// <summary>
        /// This is the divide-and-conquer implementation of the longes common-subsequence (LCS) 
        /// algorithm.
        /// The published algorithm passes recursively parts of the A and B sequences.
        /// To avoid copying these arrays the lower and upper bounds are passed while the sequences stay constant.
        /// </summary>
        /// <param name="DataA">sequence A</param>
        /// <param name="LowerA">lower bound of the actual range in DataA</param>
        /// <param name="UpperA">upper bound of the actual range in DataA (exclusive)</param>
        /// <param name="DataB">sequence B</param>
        /// <param name="LowerB">lower bound of the actual range in DataB</param>
        /// <param name="UpperB">upper bound of the actual range in DataB (exclusive)</param>
        private static void LCS(DiffData DataA, int LowerA, int UpperA, DiffData DataB, int LowerB, int UpperB)
        {
            // Debug.Write(2, "LCS", String.Format("Analyse the box: A[{0}-{1}] to B[{2}-{3}]", LowerA, UpperA, LowerB, UpperB));

            // Fast walkthrough equal lines at the start
            while (LowerA < UpperA && LowerB < UpperB && DataA.data[LowerA] == DataB.data[LowerB])
            {
                LowerA++; LowerB++;
            }

            // Fast walkthrough equal lines at the end
            while (LowerA < UpperA && LowerB < UpperB && DataA.data[UpperA - 1] == DataB.data[UpperB - 1])
            {
                --UpperA; --UpperB;
            }

            if (LowerA == UpperA)
            {
                // mark as inserted lines.
                while (LowerB < UpperB)
                    DataB.modified[LowerB++] = true;

            }
            else if (LowerB == UpperB)
            {
                // mark as deleted lines.
                while (LowerA < UpperA)
                    DataA.modified[LowerA++] = true;

            }
            else
            {
                // Find the middle snakea and length of an optimal path for A and B
                SMSRD smsrd = SMS(DataA, LowerA, UpperA, DataB, LowerB, UpperB);
                // Debug.Write(2, "MiddleSnakeData", String.Format("{0},{1}", smsrd.x, smsrd.y));

                // The path is from LowerX to (x,y) and (x,y) ot UpperX
                LCS(DataA, LowerA, smsrd.x, DataB, LowerB, smsrd.y);
                LCS(DataA, smsrd.x, UpperA, DataB, smsrd.y, UpperB);  // 2002.09.20: no need for 2 points 
            }
        } // LCS()


        /// <summary>Scan the tables of which lines are inserted and deleted,
        /// producing an edit script in forward order.  
        /// </summary>
        /// dynamic array
        private static Item[] CreateDiffs(DiffData DataA, DiffData DataB)
        {
            ArrayList a = new ArrayList();
            Item aItem;
            Item[] result;

            int StartA, StartB;
            int LineA, LineB;

            LineA = 0;
            LineB = 0;
            while (LineA < DataA.Length || LineB < DataB.Length)
            {
                if ((LineA < DataA.Length) && (!DataA.modified[LineA])
                  && (LineB < DataB.Length) && (!DataB.modified[LineB]))
                {
                    // equal lines
                    LineA++;
                    LineB++;

                }
                else
                {
                    // maybe deleted and/or inserted lines
                    StartA = LineA;
                    StartB = LineB;

                    while (LineA < DataA.Length && (LineB >= DataB.Length || DataA.modified[LineA]))
                        // while (LineA < DataA.Length && DataA.modified[LineA])
                        LineA++;

                    while (LineB < DataB.Length && (LineA >= DataA.Length || DataB.modified[LineB]))
                        // while (LineB < DataB.Length && DataB.modified[LineB])
                        LineB++;

                    if ((StartA < LineA) || (StartB < LineB))
                    {
                        // store a new difference-item
                        aItem = new Item();
                        aItem.StartA = StartA;
                        aItem.StartB = StartB;
                        aItem.deletedA = LineA - StartA;
                        aItem.insertedB = LineB - StartB;
                        a.Add(aItem);
                    } // if
                } // if
            } // while

            result = new Item[a.Count];
            a.CopyTo(result);

            return (result);
        }

    } // class Diff
    internal class DiffData
    {

        /// <summary>Number of elements (lines).</summary>
        internal int Length;

        /// <summary>Buffer of numbers that will be compared.</summary>
        internal int[] data;

        /// <summary>
        /// Array of booleans that flag for modified data.
        /// This is the result of the diff.
        /// This means deletedA in the first Data or inserted in the second Data.
        /// </summary>
        internal bool[] modified;

        /// <summary>
        /// Initialize the Diff-Data buffer.
        /// </summary>
        /// <param name="data">reference to the buffer</param>
        internal DiffData(int[] initData)
        {
            data = initData;
            Length = initData.Length;
            modified = new bool[Length + 2];
        } // DiffData

    } // class DiffData
}