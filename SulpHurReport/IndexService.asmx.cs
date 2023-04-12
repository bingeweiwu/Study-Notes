using Newtonsoft.Json;
using SulpHurReport.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Windows.Forms;

namespace SulpHurReport
{
    /// <summary>
    /// Summary description for IndexService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class IndexService : System.Web.Services.WebService
    {


        private static List<string> osRelatedRuleIDs;
        private static List<string> osNotRelatedRuleIDs;

        [WebMethod]
        public void YourMethodName(string language, string osType)
        {
            MessageBox.Show("Hello, world!", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        [WebMethod]
        public void ReceiveCondition(string buildLan, string osType, string osLanguage, string ruleid, string resultTypes, string ReviewedType)
        {

        }

        [WebMethod]
        public object QueryAvailableData()
        {
            try
            {
                var hash = new Dictionary<string, object>();
                IProductQuery pq = new ProductQuery();
                hash.Add("AvailableCapturedBuilds", pq.QueryAvailableCapturedBuilds());
                hash.Add("AvailableCapturedLanguages", pq.QueryAvailableCapturedLanguages());
                hash.Add("AvailableOSTypes", pq.QueryAvailableOSTypes());
                hash.Add("AvailableOSLanguage", pq.QueryAvailableOSLanguage());
                Dictionary<string, Rule> availableRules = pq.QueryAvailableRules();
                hash.Add("AvailableRules", availableRules);
                string osRelatedRules = ConfigurationManager.AppSettings["OSRelatedRules"].ToString();
                string[] osRelatedRuleList = osRelatedRules.Split('|');
                osRelatedRuleIDs = GetRuleIDsList(osRelatedRuleList, availableRules);
                string osNotRelatedRules = ConfigurationManager.AppSettings["OSNotRelatedRules"].ToString();
                string[] osNotRelatedRuleList = osNotRelatedRules.Split('|');
                osNotRelatedRuleIDs = GetRuleIDsList(osNotRelatedRuleList, availableRules);
                hash.Add("AssemblyInfo", pq.QueryAssemblyTypesInfo());
                hash.Add("AvailableAssembly", pq.QueryAvailableAssembly());
                hash.Add("AvailableTypes", pq.QueryAvailableTypes());
                return new { hash = hash };
            }
            catch (Exception e)
            {
                return e;
            }
        }

        #region Rule ID string generator
        private List<string> GetRuleIDsList(string[] ruleList, Dictionary<string, Rule> availableRules)
        {
            List<string> ruleIDsArray = new List<string>();
            foreach (string s in ruleList)
            {
                foreach (Rule rule in availableRules.Values)
                {
                    if (rule.RuleName == s)
                    {
                        ruleIDsArray.Add(rule.RuleID.ToString());
                        break;
                    }
                }
            }
            return ruleIDsArray;
        }
        #endregion
    }
}
