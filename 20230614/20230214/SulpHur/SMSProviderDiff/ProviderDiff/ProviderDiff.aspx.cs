using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Xml;
using System.Threading;
using System.Xml.XPath;

namespace ProviderDiff
{
    public partial class Index : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            //check the item being bound is actually a DataRow, if it is, 
            //wire up the required html events and attach the relevant JavaScripts
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //e.Row.Attributes["onmouseover"] ="javascript:setMouseOverColor(this);";
                //e.Row.Attributes["onmouseout"] ="javascript:setMouseOutColor(this);";
                e.Row.Cells[7].Attributes["onclick"] = "javascript:setMouseOverClass(this);";
                //e.Row.Cells[6].Attributes["onclick"] = "javascript:setMouseOverImg(this);";

                if (e.Row.Cells[4].Text.Trim().EndsWith(".xml"))
                {
                    System.Web.UI.WebControls.Image img = (System.Web.UI.WebControls.Image)e.Row.Cells[6].FindControl("Image1");
                    img.ImageUrl = "~/img/1.png";
                    img.ToolTip = "No img for xml file";
                }
                else {
                    e.Row.Cells[6].Attributes["onclick"] = "javascript:setMouseOverImg(this);";
                }
                //e.Row.Cells[2].Attributes["onmouseout"] = "javascript:setMouseOut(this);";
                e.Row.Attributes["onclick"] = "javascript:setMouseClick(this);";
            }
        }
        protected void GridView1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {

        }
    }
}