using Microsoft.Office.Interop.Word;
using MS.Internal.SulpHur.UICompliance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup.Localizer;
using Newtonsoft.Json;
using System.Net.Http;
using System.Data.Linq.SqlClient;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Net.Mail;

namespace MS.Internal.SulpHur.CMRules
{
    public class UnlocalizationRule : UIComplianceRuleBase
    {
        const string HOST = "https://api.cognitive.microsofttranslator.com";
        const string ROUTE_DETECT = "/detect?api-version=3.0";
        const string ROUTE_TRANSLATETEXT = "/translate?api-version=3.0";
        const string ROUTE_LANGUAGES = "/languages?api-version=3.0";

        public static List<string> nonEnglishButtonNameList = new List<string>();
        public static List<string> wordsByLanguage = new List<string>();

        public override string Name
        {
            get { return @"unlocalization Rule"; }
        }

        public override string Description
        {
            get { return @"This rule checks unlocalization bug."; }
        }

        static UnlocalizationRule()
        {
            SulpHurEntitiesForCMRules sulpHurEntitiesForCMRules = new SulpHurEntitiesForCMRules();           
            nonEnglishButtonNameList = sulpHurEntitiesForCMRules.FilterSpellCheck_nonEnglishButtonName.Select(x=>x.nonEnglishButtonName).ToList();
        }


        public override List<UIComplianceResultBase> UIVerify(ICollection<WebElementInfo> webElements)
        {

            throw new NotImplementedException();
        }

        public override List<UIComplianceResultBase> UIVerify(ICollection<ElementInformation> Controls)
        {

            List<UIComplianceResultBase> result = new List<UIComplianceResultBase>();
            UIComplianceResultBase newResult = null;
            string controlTypeMessage = "ControlType: {0}";
            if (lan != "ENU")
            {
                CMDBoperator CMDB = new CMDBoperator();
                wordsByLanguage = CMDB.GetWordsByLanguage(lan);
                int wordNumber = 0;
                try
                {
                    string eiName = string.Empty;

                    foreach (ElementInformation ei in Controls)
                    {
                        if (ei.ControlType == ControlType.Button
                            || "_navPanel" == ei.AutomationId.Trim()
                            || ei.ControlType == ControlType.CheckBox
                            || ei.ControlType == ControlType.RadioButton
                            || (ei.ControlType == ControlType.Text)
                            || ei.ControlType == ControlType.Group
                            || ei.ControlType == ControlType.List
                            )
                        {
                            eiName = ei.Name.Trim();
                            if (string.IsNullOrEmpty(eiName))
                            {
                                continue;
                            }
                            wordNumber = eiName.Split(' ').Length;
                            string eiNameLanguage = Detect(eiName);

                            bool isOtherLanguage = false;
                            if (eiNameLanguage == "en" && wordNumber > 4)
                            {
                                foreach (string s in wordsByLanguage)
                                {
                                    if (eiName.ToLower().Contains(s.ToLower()))
                                    {
                                        // same as English word, cannot be used as a judgement standard. for example: "Start" is both ENU and DEU
                                        if (CMDB.WhetherSameAsEnglish(s))
                                            continue;
                                        // As long as it is not English, it can be considered that it has been localized
                                        isOtherLanguage = true;
                                        break;
                                    }
                                }

                                if (isOtherLanguage == true
                                   || (ei.ControlType == ControlType.Button && nonEnglishButtonNameList.Contains(eiName))
                                   ) continue;
                                else
                                {
                                    controlTypeMessage = string.Format(controlTypeMessage, ei.ControlType);
                                    newResult = new UIComplianceResultBase(ResultType.Fail, "This control may contain an unlocalized vulnerability :" + "\r\n\r\n" + controlTypeMessage + "\r\n\r\n" + "completeString: " + CommonRuleUtility.TruncateControlFullName(eiName), this.Name);
                                    newResult.AddRelatedControls(ei);
                                    result.Add(newResult);
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    result.Add(new UIComplianceResultBase(ResultType.Info, "Encounter an error in verification.", Name));
                }
                if (result.Count == 0)
                {
                    result.Add(new UIComplianceResultBase(ResultType.Pass, "No unlocalized issue found.", Name));
                }
            }
            else
            {
                result.Add(new UIComplianceResultBase(ResultType.Warning, "Unlocalization rule works on all language but not ENU. ENU will set to warning.\n", Name));
            }
            return result;
        }
        public string Detect(string text)
        {
            try
            {
                Uri uri = new Uri(string.Format("{0}{1}", HOST, ROUTE_DETECT));
                string jsonResponse = HttpRequest(text, uri, HttpMethod.Post);
                dynamic ret = JsonConvert.DeserializeObject(jsonResponse);
                return ret[0].language;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex + " at SulpHur\\MS.Internal.SulpHur.CMRules\\UnlocalizationRule.cs line 141");
                throw ex;
            }
        }

        public string HttpRequest(string text, Uri uri, HttpMethod method)
        {
            string requestBody = string.Empty;
            if (!string.IsNullOrEmpty(text))
            {
                System.Object[] body = new System.Object[] { new { Text = text } };
                requestBody = JsonConvert.SerializeObject(body);
            }

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = method;
                // Construct the full URI 
                request.RequestUri = uri;
                if (!string.IsNullOrEmpty(text))
                {
                    // Add the serialized JSON object to your request 
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                }
                // Add the authorization header                 
                request.Headers.Add("Ocp-Apim-Subscription-Key", "3d64864b660d45b5bc51c4d85e415ebf");
                // Send request, get response 
                var response = client.SendAsync(request).Result;
                return response.Content.ReadAsStringAsync().Result;
            }
        }

    }
}