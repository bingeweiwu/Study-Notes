using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.IO;

namespace SulpHurManagementSystem
{
    public partial class ShowAvailableSulpHurPicture : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["ADOConn"].ToString();
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
            if (cookie != null && cookie["prebuild"].ToString() != "" && cookie["postbuild"].ToString() != "")
            {
                beforebuild = cookie["prebuild"].ToString();
                afterbuild = cookie["postbuild"].ToString();
            }
        }
        protected void ShowResults_Click(object sender, EventArgs e)
        {
            string preMessage = string.Empty;
            preMessage = GetSulpHurPathWithBuild(beforebuild, assembly, fulltype);
            string postMessage = string.Empty;
            postMessage = GetSulpHurPathWithBuild(afterbuild, assembly, fulltype);
            if (string.Compare(preMessage.Substring(preMessage.Length - 4,4),".jpg") == 0)
            {
                Label1.Visible = true;
                availableBeforeBuild.Visible = true;
                availableBeforeBuild.InnerText = beforebuild;
                SulpHurPreImage.ImageUrl = preMessage;
                PreLabel.Visible = false;
                SulpHurPreImage.Visible = true;
            }
            else
            {
                Label1.Visible = true;
                availableBeforeBuild.Visible = false;
                PreLabel.Text = preMessage;
                SulpHurPreImage.Visible = false;
                PreLabel.Visible = true;
            }
            if (string.Compare(postMessage.Substring(postMessage.Length - 4, 4), ".jpg") == 0)
            {
                Label2.Visible = true;
                availableAfterBuild.Visible = true;
                availableAfterBuild.InnerText = afterbuild;
                SulpHurPostImage.ImageUrl = postMessage;
                SulpHurPostImage.Visible = true;
                PostLabel.Visible = false;
            }
            else
            {
                Label2.Visible = true;
                availableAfterBuild.Visible = false;
                PostLabel.Text = postMessage;
                SulpHurPostImage.Visible = false;
                PostLabel.Visible = true;
            }
        }
        protected void ShowAvailableResults_Click(object sender, EventArgs e)
        {
            string preMessage = string.Empty;
            preMessage = GetSulpHurPathWithAvailableBuild(beforebuild, beforebuild, afterbuild, assembly, fulltype);
            string postMessage = string.Empty;
            postMessage = GetSulpHurPathWithAvailableBuild(afterbuild, beforebuild, afterbuild, assembly, fulltype);
            if (string.Compare(preMessage.Substring(preMessage.Length - 4, 4), ".jpg") == 0)
            {
                Label1.Visible = true;
                availableBeforeBuild.Visible = true;
                availableBeforeBuild.InnerText = preMessage.Split('|')[0];
                SulpHurPreImage.ImageUrl = preMessage.Split('|')[1];
                PreLabel.Visible = false;
                SulpHurPreImage.Visible = true;
            }
            else
            {
                Label1.Visible = true;
                availableBeforeBuild.Visible = false;
                PreLabel.Text = preMessage;
                SulpHurPreImage.Visible = false;
                PreLabel.Visible = true;
            }
            if (string.Compare(postMessage.Substring(postMessage.Length - 4, 4), ".jpg") == 0)
            {
                Label2.Visible = true;
                availableAfterBuild.Visible = true;
                availableAfterBuild.InnerText = postMessage.Split('|')[0];
                SulpHurPostImage.ImageUrl = postMessage.Split('|')[1];
                SulpHurPostImage.Visible = true;
                PostLabel.Visible = false;
            }
            else
            {
                Label2.Visible = true;
                availableAfterBuild.Visible = false;
                PostLabel.Text = postMessage;
                SulpHurPostImage.Visible = false;
                PostLabel.Visible = true;
            }
        }
        public string GetSulpHurPathWithBuild(string build, string assemblyname, string fulltypename)
        {
            string imagePath = string.Empty;
            try
            {
                string ContentID = string.Empty;
                byte[] UIScreenshot = new byte[0];
                string queryString = @"select top 1 result2.ContentID,result2.BuildID,result2.UIScreenShot from
                                    (select UIContents.ContentID,UIContents.BuildID,UIContents.UIScreenShot from
                                    (select ContentID from
                                    (Select * from AssemblyInfo where AssemblyName = '" + assemblyname +
                                        @"' and FullTypeName = '" + fulltypename +
                                        @"') as result
                                    left join AssemblyLink on AssemblyLink.TypeID = result.TypeID) as result1
                                    left join UIContents on UIContents.ContentID = result1.ContentID) as result2
                                    inner join (Select * from BuildInfo where Language = 'ENU' and BuildNo like '%" + build + "%') as builds on builds.BuildID = result2.BuildID order by BuildNo desc";
                //using (SqlConnection con = new SqlConnection("Data Source=sulphurserver15.redmond.corp.microsoft.com;Initial Catalog=SulpHur;integrated security=True"))
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(queryString, con);
                    con.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        ContentID = reader[0].ToString();
                        imagePath = Path.Combine(Context.Server.MapPath(""), @"Tmp_ContentImage\" + ContentID + ".jpg");
                        UIScreenshot = (byte[])reader["UIScreenShot"];
                        System.Drawing.Image image = null;
                        using (MemoryStream ms = new MemoryStream(UIScreenshot))
                        {
                            using (image = System.Drawing.Image.FromStream(ms))
                            {
                                
                                if (!File.Exists(imagePath))
                                {                                    
                                    image.Save(imagePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                                }   
                            }
                        }
                        //imagePath = @"Tmp_ContentImage\" + ContentID + ".jpg";
                        imagePath = @"Tmp_ContentImage/" + ContentID + ".jpg";
                    }
                    else
                    {
                        throw new Exception("No Available picture found for " + build + ", please click “Show Available Result…” if you want to see the latest available picture!");
                    }
                }
            }
            catch(Exception e)
            {
                imagePath = e.Message;
            }
            return imagePath;
        }
        public string GetSulpHurPathWithAvailableBuild(string build, string preBuild, string postBuild, string assemblyname, string fulltypename)
        {
            string info = string.Empty;
            try
            {
                string ContentID = string.Empty;
                byte[] UIScreenshot = new byte[0];
                string queryString = string.Empty;
                if (build == preBuild)
                {
                    queryString = @"select top 1 result2.ContentID,result2.BuildID,result2.UIScreenShot,builds.BuildNo from
                                    (select UIContents.ContentID,UIContents.BuildID,UIContents.UIScreenShot from
                                    (select ContentID from
                                    (Select * from AssemblyInfo where AssemblyName = '" + assemblyname +
                                        @"' and FullTypeName = '" + fulltypename +
                                        @"') as result
                                    left join AssemblyLink on AssemblyLink.TypeID = result.TypeID) as result1
                                    left join UIContents on UIContents.ContentID = result1.ContentID) as result2
                                    inner join (Select * from BuildInfo where Language = 'ENU' and BuildNo < '5.0." + preBuild +
                                    @".9999') as builds on builds.BuildID = result2.BuildID order by BuildNo desc";
                }
                if (build == postBuild)
                {
                    queryString = @"select top 1 result2.ContentID,result2.BuildID,result2.UIScreenShot,builds.BuildNo from
                                    (select UIContents.ContentID,UIContents.BuildID,UIContents.UIScreenShot from
                                    (select ContentID from
                                    (Select * from AssemblyInfo where AssemblyName = '" + assemblyname +
                                        @"' and FullTypeName = '" + fulltypename +
                                        @"') as result
                                    left join AssemblyLink on AssemblyLink.TypeID = result.TypeID) as result1
                                    left join UIContents on UIContents.ContentID = result1.ContentID) as result2
                                    inner join (Select * from BuildInfo where Language = 'ENU' and BuildNo > '5.0." + preBuild +
                                    @".9999' and BuildNo < '5.0." + build +
                                    @".9999') as builds on builds.BuildID = result2.BuildID order by BuildNo desc";
                }
                //using (SqlConnection con = new SqlConnection("Data Source=sulphurserver15.redmond.corp.microsoft.com;Initial Catalog=SulpHur;integrated security=True"))
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(queryString, con);
                    con.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        ContentID = reader[0].ToString();
                        info = reader[3].ToString();
                        string localImagePath = Path.Combine(Context.Server.MapPath(""), @"Tmp_ContentImage\" + ContentID + ".jpg");
                        //info += "|" + @"Tmp_ContentImage\" + ContentID + ".jpg";
                        info += "|" + @"Tmp_ContentImage/" + ContentID + ".jpg";
                        UIScreenshot = (byte[])reader["UIScreenShot"];
                        System.Drawing.Image image = null;
                        using (MemoryStream ms = new MemoryStream(UIScreenshot))
                        {
                            using (image = System.Drawing.Image.FromStream(ms))
                            {
                                if (!File.Exists(localImagePath))
                                {
                                    image.Save(localImagePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (build == preBuild)
                        {
                            throw new Exception("No Available picture found before " + build + "!");
                        }
                        else
                        {
                            throw new Exception("No Available picture found between " + preBuild + " and " + postBuild + "!");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                info = e.Message;
            }
            return info;
        }

        protected void HiddenResults_Click(object sender, EventArgs e)
        {
            Label1.Visible = false;
            Label2.Visible = false;
            availableBeforeBuild.Visible = false;
            availableAfterBuild.Visible = false;
            SulpHurPreImage.Visible = false;
            SulpHurPostImage.Visible = false;
            PreLabel.Visible = false;
            PostLabel.Visible = false;
        }
    }
}