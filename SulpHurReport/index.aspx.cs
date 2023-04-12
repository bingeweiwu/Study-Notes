using SulpHurReport.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
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
            //eg:FAREAST\v-xiqian
            string fullUsername = HttpContext.Current.Request.LogonUserIdentity.Name;            
            string alias = fullUsername.Substring(fullUsername.IndexOf(@"\") + 1);

            string connectionString_SCCM_ITL_TLT = ConfigurationManager.ConnectionStrings["ConnectionString_SCCM_ITL_TLT"].ToString();
            string sql = string.Format("select Name from SH_People where Alias='{0}'", alias);
            object result = SQLUtility.ExecuteScalar(connectionString_SCCM_ITL_TLT, sql);
            if (result != null)
            {              

                this.UserName.Text = result.ToString();
                //this.lnkCurrentUserAlias.InnerText = alias;
                ////eg:Eric Qiang
                //this.lnkCurrentUserName.InnerText = result.ToString();
                //this.lnkCurrentUserImage.InnerText = "<img src=\"" + user.ImageLink + "\"/>";
            }
        }
    }
}