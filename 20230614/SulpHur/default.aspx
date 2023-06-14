<%@ Page Language="C#" %> 
<script runat="server"> 
    protected override void OnLoad(EventArgs e) 
    {
        NameValueCollection argumentCollection = HttpUtility.ParseQueryString(Request.QueryString.ToString());
        try
        {
            argumentCollection.Add("UserName", HttpContext.Current.User.Identity.Name);
        }
        catch
        {
        }
        string url = string.Format("MS.Internal.SulpHur.SulpHurClientLauncher.application?{0}", argumentCollection.ToString());
        Response.Redirect(url);        
    } 
</script> 
