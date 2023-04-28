using SulpHurReport.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SulpHurReport
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        //protected void Page_Load(object sender, EventArgs e)
        //{
        //    Session["CanDelete"] = 0;
        //    //eg:FAREAST\v-xiqian
        //    string fullUsername = HttpContext.Current.Request.LogonUserIdentity.Name;
        //    string alias = fullUsername.Substring(fullUsername.IndexOf(@"\") + 1);

        //    string connectionString_SCCM_ITL_TLT = ConfigurationManager.ConnectionStrings["ConnectionString_SCCM_ITL_TLT"].ToString();
        //    string sql = string.Format("select Name from SH_People where Alias='{0}'", alias);
        //    object result = SQLUtility.ExecuteScalar(connectionString_SCCM_ITL_TLT, sql);
        //    if (result != null)
        //    {              
        //        this.UserName.Text = result.ToString();
        //        this.lnkCurrentUserAlias.Text = alias;
        //        ////eg:Eric Qiang
        //        //this.lnkCurrentUserName.InnerText = result.ToString();
        //        //this.lnkCurrentUserImage.InnerText = "<img src=\"" + user.ImageLink + "\"/>";
        //    }
        //}


        protected void Page_Load(object sender, EventArgs e)
        {
            Session["CanDelete"] = 0;
            //eg:FAREAST\v-xiqian
            string fullUsername = HttpContext.Current.Request.LogonUserIdentity.Name;
            string alias = fullUsername.Substring(fullUsername.IndexOf(@"\") + 1);

            string connectionString_SCCM_ITL_TLT = ConfigurationManager.ConnectionStrings["ConnectionString_SCCM_ITL_TLT"].ToString();
            string sql = string.Format("select Name, ImageLink from SH_People where Alias='{0}'", alias); // 添加查询 ImageLink 字段
            using (SqlConnection connection = new SqlConnection(connectionString_SCCM_ITL_TLT))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string userName = reader["Name"].ToString();
                            string userImageLink = reader["ImageLink"].ToString(); // 获取用户头像链接
                            this.UserName.Text = userName;
                            this.lnkCurrentUserAlias.Text = alias;
                            //https://cmsherlock.redmond.corp.microsoft.com/photos/v-mogenzhang.png
                          //  UserProfileImage.Src = "https://cmsherlock.redmond.corp.microsoft.com"+userImageLink; // 设置用户头像链接到图片标签
                        }
                    }
                }
            }
        }



    }
}