using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ProviderDiff
{
    public partial class Image : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["ImageID"] != null)
            {
                string id = Request.QueryString["ImageID"];
                if (!string.IsNullOrEmpty(id))
                {
                    SulpHurEntities entities1 = new SulpHurEntities();
                    int imageId = Int32.Parse(id);
                    var list = from x in entities1.UIContents
                               where x.ContentID == imageId
                               select x.UIScreenShot;
                    foreach (byte[] x in list)
                    {
                        Byte[] bytes = x;
                        Response.ContentType = "image/jpeg";
                        Response.BinaryWrite(bytes);
                        Response.End();
                        break;
                    }
                }
            }
        }
    }
}