using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data;

namespace ProviderDiff
{
    /// <summary>
    /// Summary description for BindingTable
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class BindingTable : System.Web.Services.WebService
    {

        [WebMethod]
        public List<BuildType> BindDatatable(string ot,string rb, string sl, string sb, string ass, string sc)
        {
            //string selectedLanguage = this.Request.Form["sl"];
            //string selectedBuild = this.Request.Form["sb"]; ;
            //string selectedCategory = this.Request.Form["sc"]; ;
            //List<string> builds = selectedBuild.Split('|').ToList();
            //List<string> categories = selectedCategory.Split('|').ToList();
            //string buildno = this.Request.Form["rb"]; ;
            //string selectedAssembly = this.Request.Form["ass"]; ;

            string selectedLanguage = sl;
            string selectedBuild = sb;
            string selectedCategory = sc;
            List<string> builds = selectedBuild.Split('|').ToList();
            for (int i = 0; i < builds.Count(); i++) {
                builds[i] = builds[i].Trim();
            }
            List<string> categories = selectedCategory.Split('|').ToList();
            for (int i = 0; i < categories.Count(); i++)
            {
                categories[i] = categories[i].Trim();
            }
            string buildno = rb;
            string selectedAssembly = ass;
            string selectedOSType = ot;

            DataTable dt = new DataTable();
            List<BuildType> details = new List<BuildType>();
            List<BuildType> detailResult = new List<BuildType>();
            using (SulpHurEntities entity = new SulpHurEntities())
            {
                var referencedList = (from x in entity.BuildTypes
                                     where x.BuildNo == buildno
                                     select x).ToList();

                if (selectedAssembly != "All")
                {
                    referencedList = referencedList.Where(c => c.AssemblyName == selectedAssembly).ToList();
                }
                int z = referencedList.Count();
                foreach (var c in referencedList)
                {
                    #region Add New Captured
                    if (c.Mark != "Captured" &&
                        entity.ViewAssemblySummaries.Any(v =>
                            c.AssemblyName == v.AssemblyName
                            && c.TypeName == v.FullTypeName))
                    {
                        c.Mark = "Captured";
                    }
                    entity.SaveChanges();
                    #endregion

                    #region filter os and language
                    if (selectedLanguage == "All"&&selectedOSType=="All")
                    {
                        if (!entity.ViewAssemblySummaries.Any(q => builds.Contains(q.BuildNo)
                            && c.AssemblyName == q.AssemblyName
                            && c.TypeName == q.FullTypeName))
                        {
                            if (entity.ViewAssemblySummaries.Any(v =>
                            c.AssemblyName == v.AssemblyName
                            && c.TypeName == v.FullTypeName))
                            {
                                BuildType temp = new BuildType();
                                temp.AssemblyName = c.AssemblyName;
                                temp.TypeName = c.TypeName;
                                temp.Mark = "Captured In Other Builds/Language/OS";
                                details.Add(temp);
                            }
                            else
                            {
                                BuildType temp = new BuildType();
                                temp.AssemblyName = c.AssemblyName;
                                temp.TypeName = c.TypeName;
                                temp.Mark = c.Mark;
                                details.Add(temp);
                            }
                        }
                        else {
                            BuildType temp = new BuildType();
                            temp.AssemblyName = c.AssemblyName;
                            temp.TypeName = c.TypeName;
                            temp.Mark = c.Mark;
                            details.Add(temp);
                        }
                    }
                    else if(selectedLanguage != "All"&&selectedOSType=="All")
                    {
                        if (!entity.ViewAssemblySummaries.Any(q => builds.Contains(q.BuildNo)
                                && q.Language == selectedLanguage
                                && c.AssemblyName == q.AssemblyName
                                && c.TypeName == q.FullTypeName))
                        {
                            if (entity.ViewAssemblySummaries.Any(v =>
                            c.AssemblyName == v.AssemblyName
                            && c.TypeName == v.FullTypeName))
                            {
                                BuildType temp = new BuildType();
                                temp.AssemblyName = c.AssemblyName;
                                temp.TypeName = c.TypeName;
                                temp.Mark = "Captured In Other Builds/Language/OS";
                                details.Add(temp);
                            }
                            else
                            {
                                BuildType temp = new BuildType();
                                temp.AssemblyName = c.AssemblyName;
                                temp.TypeName = c.TypeName;
                                temp.Mark = c.Mark;
                                details.Add(temp);
                            }
                        }
                        else
                        {
                            BuildType temp = new BuildType();
                            temp.AssemblyName = c.AssemblyName;
                            temp.TypeName = c.TypeName;
                            temp.Mark = c.Mark;
                            details.Add(temp);
                        }

                    }
                    else if (selectedLanguage == "All" && selectedOSType != "All")
                    {
                        if (!entity.ViewAssemblySummaries.Any(q => builds.Contains(q.BuildNo)
                                && q.OSType == selectedOSType
                                && c.AssemblyName == q.AssemblyName
                                && c.TypeName == q.FullTypeName))
                        {
                            if (entity.ViewAssemblySummaries.Any(v =>
                            c.AssemblyName == v.AssemblyName
                            && c.TypeName == v.FullTypeName))
                            {
                                BuildType temp = new BuildType();
                                temp.AssemblyName = c.AssemblyName;
                                temp.TypeName = c.TypeName;
                                temp.Mark = "Captured In Other Builds/Language/OS";
                                details.Add(temp);
                            }
                            else
                            {
                                BuildType temp = new BuildType();
                                temp.AssemblyName = c.AssemblyName;
                                temp.TypeName = c.TypeName;
                                temp.Mark = c.Mark;
                                details.Add(temp);
                            }
                        }
                        else
                        {
                            BuildType temp = new BuildType();
                            temp.AssemblyName = c.AssemblyName;
                            temp.TypeName = c.TypeName;
                            temp.Mark = c.Mark;
                            details.Add(temp);
                        }

                    }
                    else if (selectedLanguage != "All" && selectedOSType != "All")
                    {
                        if (!entity.ViewAssemblySummaries.Any(q => builds.Contains(q.BuildNo)
                                && q.Language == selectedLanguage
                                && q.OSType == selectedOSType
                                && c.AssemblyName == q.AssemblyName
                                && c.TypeName == q.FullTypeName))
                        {
                            if (entity.ViewAssemblySummaries.Any(v =>
                            c.AssemblyName == v.AssemblyName
                            && c.TypeName == v.FullTypeName))
                            {
                                BuildType temp = new BuildType();
                                temp.AssemblyName = c.AssemblyName;
                                temp.TypeName = c.TypeName;
                                temp.Mark = "Captured In Other Builds/Language/OS";
                                details.Add(temp);
                            }
                            else
                            {
                                BuildType temp = new BuildType();
                                temp.AssemblyName = c.AssemblyName;
                                temp.TypeName = c.TypeName;
                                temp.Mark = c.Mark;
                                details.Add(temp);
                            }
                        }
                        else
                        {
                            BuildType temp = new BuildType();
                            temp.AssemblyName = c.AssemblyName;
                            temp.TypeName = c.TypeName;
                            temp.Mark = c.Mark;
                            details.Add(temp);
                        }
                    }
                    #endregion
                }
            }

            if (categories.Count() == 1 && categories[0] == "All")
            {
                foreach (BuildType bt in details)
                {
                    detailResult.Add(bt);
                }
            }
            else
            {
                foreach (BuildType bt in details)
                {
                    if (categories.Contains(bt.Mark))
                    {
                        detailResult.Add(bt);
                    }
                }
            }

            return detailResult.ToList();
            // Response.Write(json.Serialize(detailResult.ToArray()));
        }
    }
}
