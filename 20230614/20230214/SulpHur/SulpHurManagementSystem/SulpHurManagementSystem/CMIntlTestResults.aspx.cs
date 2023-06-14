using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

namespace SulpHurManagementSystem
{
    public partial class CMIntlTestResults : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string url = Request.QueryString["url"];
            HttpCookie cookie = new HttpCookie("buildinfo");
            cookie.Values["url"] = url;
            cookie.Values["prebuild"] = url.Substring(url.IndexOf("_") - 4, 4);
            cookie.Values["postbuild"] = url.Substring(url.IndexOf("_") + 1, 4);
            cookie.Expires = DateTime.Now.AddDays(100);
            Response.Cookies.Add(cookie); 
        }
    }
}