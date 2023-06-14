using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SulpHurManagementSystem
{
    public partial class CapturedUIReport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["CanDelete"] = 0;
            //eg:FAREAST\v-xiqian
            string fullUsername = HttpContext.Current.Request.LogonUserIdentity.Name;
            string alias = fullUsername.Substring(fullUsername.IndexOf(@"\") + 1);
            SCCM_ITL_TLTEntities entities = new SCCM_ITL_TLTEntities();
            var list = from r in entities.SH_People
                       where r.Alias == alias
                       select r;
            if (list.Count() > 0)
            {
                var user = list.First();
                //eg:FAREAST\v-xiqian
                this.cpUserName.Value = fullUsername;
                //eg:v-xiqian
                this.lnkCurrentUserAlias.InnerText = alias;
                //eg:Eric Qiang
                this.lnkCurrentUserName.InnerText = user.Name;
                //this.lnkCurrentUserImage.InnerText = "<img src=\"" + user.ImageLink + "\"/>";
            }
        }
    }
}