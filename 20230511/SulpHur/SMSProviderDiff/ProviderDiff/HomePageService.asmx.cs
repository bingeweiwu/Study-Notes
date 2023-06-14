using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace ProviderDiff
{
    /// <summary>
    /// Summary description for HomePageService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class HomePageService : System.Web.Services.WebService
    {
        [WebMethod]
        public object QueryAvailableData()
        {
            var hash = new Dictionary<string, string>();
            IProductQuery pq = new ProductQuery();
            hash.Add("AvailableProductBuilds", pq.QueryAvailableProductBuilds());
            hash.Add("AvailableCapturedBuilds", pq.QueryAvailableCapturedBuilds());
            hash.Add("AvailableCapturedLanguages", pq.QueryAvailableCapturedLanguages());
            hash.Add("AvailableOSTypes", pq.QueryAvailableOSTypes());
            hash.Add("AvailableAssembly", pq.QueryAvailableAssembly());
            return new { hash = hash };
        }
    }

    //return array data split by common
    public interface IProductQuery {
        string QueryAvailableProductBuilds();
        string QueryAvailableOSTypes();
        string QueryAvailableCapturedLanguages();
        string QueryAvailableCapturedBuilds();
        string QueryAvailableAssembly();
    }

    public class ProductQuery : IProductQuery
    {
        public string QueryAvailableProductBuilds()
        {
            using (SulpHurEntities entity = new SulpHurEntities())
            {
                var list = (from x in entity.BuildTypes select x.BuildNo).Distinct().OrderByDescending(c => c);
                string result = string.Empty;
                foreach (string x in list)
                {
                    result += x;
                    result += ",";
                }
                result = result.Substring(0, result.Length - ",".Length);
                return result;
            }
        }
        public string QueryAvailableOSTypes()
        {
            using (SulpHurEntities entity = new SulpHurEntities())
            {
                var list = (from x in entity.Clients
                            select x.OSType).Distinct().OrderByDescending(n => n);

                string result = string.Empty;
                foreach (string x in list)
                {
                    result += x.Trim();
                    result += ",";
                }
                result = result.Substring(0, result.Length - ",".Length);
                return result;
            }
        }
        public string QueryAvailableCapturedLanguages()
        {
            using (SulpHurEntities entity = new SulpHurEntities())
            {
                var list = (from x in entity.BuildInfoes
                            select x.Language).Distinct().OrderByDescending(n => n);

                string result = string.Empty;
                foreach (string x in list)
                {
                    result += x.Trim();
                    result += ",";
                }
                result = result.Substring(0, result.Length - ",".Length);
                return result;
            }
        }
        public string QueryAvailableCapturedBuilds()
        {
            using (SulpHurEntities entity = new SulpHurEntities())
            {
                var list = (from x in entity.BuildInfoes
                            select x.BuildNo).Distinct().OrderByDescending(n => n);
                string result = string.Empty;
                foreach (string x in list)
                {
                    result += x.Trim();
                    result += ",";
                }
                result = result.Substring(0, result.Length - ",".Length);
                return result;
            }
        }
        public string QueryAvailableAssembly()
        {
            using (SulpHurEntities entity = new SulpHurEntities())
            {
                var list = (from x in entity.BuildTypes select x.AssemblyName).Distinct().OrderBy(n=>n);
                string result = string.Empty;
                foreach (string x in list)
                {
                    result += x;
                    result += ",";
                }
                result = result.Substring(0, result.Length - ",".Length);
                return result;
            }
        }
    }
}
