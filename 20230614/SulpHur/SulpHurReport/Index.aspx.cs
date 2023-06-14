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
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["CanDelete"] = 0;
            string fullUsername = HttpContext.Current.Request.LogonUserIdentity.Name;
            string alias = fullUsername.Substring(fullUsername.IndexOf(@"\") + 1);
            string connectionString_SCCM_ITL_TLT = ConfigurationManager.ConnectionStrings["ConnectionString_SCCM_ITL_TLT"].ToString();
            string sql = string.Format("select Name, ImageLink from SH_People where Alias='{0}'", alias);
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
                            string userImageLink = reader["ImageLink"].ToString();
                            this.UserName.Text = userName;
                            this.lnkCurrentUserAlias.Text = alias;
                            // Set user profile image source to relative path of user image
                            if (string.IsNullOrEmpty(userImageLink))
                            {
                                UserProfileImage.ImageUrl = "~/Images/user.png";
                            }
                            else
                            {
                                UserProfileImage.ImageUrl = "UserImages/" + alias + ".png";
                            }
                        }
                    }
                }
            }
        }
    }
}