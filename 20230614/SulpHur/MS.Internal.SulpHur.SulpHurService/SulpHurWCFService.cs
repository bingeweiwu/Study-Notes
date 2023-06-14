using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.IO;
using MS.Internal.SulpHur.UICompliance;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Xml.Serialization;
using System.Net;
using System.Xml.Linq;
using System.Data.Linq;
using SulpHurServiceAbstract;
using MS.Internal.SulpHur.Utilities;

namespace MS.Internal.SulpHur.SulpHurService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "SulpHurWCFService" in both code and config file together.
    public class SulpHurWCFService : ISulpHurWCFService
    {
        RuleOperations ruleManager;
        VerifyOperations verifyThread;
        public static Dictionary<string, ICallBack> clients = new Dictionary<string, ICallBack>();
        public static ICallBack admin;

        //private Type dBOperator;

        public SulpHurWCFService(RuleOperations ruleManager, VerifyOperations verifyThread)
        {
            this.ruleManager = ruleManager;
            this.verifyThread = verifyThread;
            //try
            //{
            //    dBOperator = Assembly.LoadFile(@"F:\SulpHur\SulpHurService\MS.Internal.SulpHur.SulpHurService.exe").GetType("MS.Internal.SulpHur.SulpHurService.DBOperator");
            //}
            //catch (Exception ex)
            //{
            //    Log.WriteServerLog(@"LoadFile fail:F:\SulpHur\SulpHurService\MS.Internal.SulpHur.SulpHurService.exe,execption:" +ex.Message);
            //}

        }
        //public string GetServerName()
        //{
        //    return ServiceEnviroment._SERVERSHOWNAME;
        //}
        public UploadResults ReceiveUI(WindowPageInfo windowPageInfo, System.Drawing.Bitmap bitmap, AdditionInformations additionInformations)
        {
            Log.WriteServerLog("Receiving new UI...",TraceLevel.Verbose);
            UploadResults result;

            //retrieve page element info
            ElementInformation root = windowPageInfo.ei;
            XElement ele = MS.Internal.SulpHur.Utilities.ExtensionMethods.ToXElement<ElementInformation>(root);

            //screenshot
            Binary binary = MS.Internal.SulpHur.UICompliance.Utility.BmpToBytes_MemStream(bitmap);
            //UIName
            string name;
            if (string.IsNullOrEmpty(root.Name))
                name = string.Empty;
            else
                name = root.Name;

            using (DBOperator dbOperator = new DBOperator())
            {
                dbOperator.InsertUI(ele, binary, additionInformations, name, windowPageInfo.AssemblyInfoList, windowPageInfo.LaunchedFrom, windowPageInfo.WindowHierarchy, false, out result);
            }
            return result;
        }
        public UploadResults ReceiveWebUI(WebPageInfo webPageInfo, System.Drawing.Bitmap bitmap, AdditionInformations additionInformations)
        {
            Log.WriteServerLog(string.Format("user:{0},computer:{1},is uploading UI.",additionInformations.Alias,additionInformations.MacAddress),TraceLevel.Info);
            UploadResults result;
            XElement ele = MS.Internal.SulpHur.Utilities.ExtensionMethods.ToXElement<WebElementInfo>(webPageInfo.webElementInfo);
            Binary binary = MS.Internal.SulpHur.UICompliance.Utility.BmpToBytes_MemStream(bitmap);

            string name;
            if (string.IsNullOrEmpty(webPageInfo.Url))
                name = string.Empty;
            else
                name = webPageInfo.Url;

            DBOperator dbOperator = new DBOperator();
            dbOperator.InsertUI(ele, binary, additionInformations, name, null, null, null, true,out result);
            return result;
        }
        public List<ComplianceRule> GetRules()
        {
            List<ComplianceRule> rules = new List<ComplianceRule>();

            foreach (UIComplianceRuleBase b in RuleManager.RuleList)
            {
                ComplianceRule rTemp = new ComplianceRule();
                rTemp.Name = b.Name;
                rTemp.Description = b.Description;
                rTemp.IsEnable = b.IsEnabled;
                rules.Add(rTemp);
            }
            return rules;
        }
        public ComplianceResult QueryResult(string UIName)
        {
            using (DBOperator dbOperator = new DBOperator())
            {
                return dbOperator.QueryResultByName(UIName);
            }
        }
        public void SetRuleStatus(bool status, string ruleName)
        {
            this.ruleManager.SetRuleStatus(status, ruleName);
        }
        public void RescanByBuildNo(string buildNo)
        {
            this.verifyThread.RescanByBuildNo(buildNo);
        }
        public List<string> GetBuildList()
        {
            using (DBOperator dp = new DBOperator())
            {
                return dp.QueryBuildInfo();
            }
        }
        public void ReloadRules()
        {
            ruleManager.ReLoadLoadRules();
        }
        public void Register(string ComputerName)
        {
            ICallBack callback = OperationContext.Current.GetCallbackChannel<ICallBack>();
            if (!clients.ContainsKey(ComputerName))
            {
                clients.Add(ComputerName, callback);
            }
            else
            {
                ICallBack value;
                clients.TryGetValue(ComputerName, out value);
                if (!IsCallBackValid(value))
                {
                    clients.Remove(ComputerName);
                    clients.Add(ComputerName, callback);
                }
            }
        }
        private bool IsCallBackValid(ICallBack cb)
        {
            if (cb == null) return false;

            ICommunicationObject co = (ICommunicationObject)cb;
            if (co.State == CommunicationState.Faulted || co.State == CommunicationState.Closed)
            {
                return false;
            }
            return true;
        }
        public void AdminLogin()
        {
            admin = OperationContext.Current.GetCallbackChannel<ICallBack>();
        }
        public ForegroundData AdOpGetForegroundData(string computerName)
        {
            try
            {
                ICallBack cb;
                if (clients.TryGetValue(computerName, out cb))
                {
                    return cb.getForegroundData();
                }
            }
            catch (Exception e) {
                Log.WriteServerLog(e.Message, TraceLevel.Error);
            }
            return null;
        }
        public List<ComplianceResult> AdOpVerifyUI(string computerName)
        {
            List<ComplianceResult> list = new List<ComplianceResult>();
            ICallBack cb;
            if (clients.TryGetValue(computerName, out cb))
            {
                CapturedData data = cb.capturedData();
                list = VerifySingleUI(data);
            }
            return list;
        }
        private List<ComplianceResult> VerifySingleUI(CapturedData data) {
            List<ComplianceResult> list = new List<ComplianceResult>();
            List<ElementInformation> eiList = verifyThread.ParseTreeToList(data.Ei);
            ElementInformation eiRoot = eiList[0];
            for (int i = 0; i < RuleManager.RuleList.Count; i++)
            {
                if (RuleManager.RuleList[i].IsEnabled)
                {
                    List<UIComplianceResultBase> results = RuleManager.RuleList[i].UIVerify(eiList);
                    foreach (UIComplianceResultBase b in results)
                    {
                        ComplianceResult cr = new ComplianceResult();
                        cr.Image = verifyThread.DrawBitmap(data.Image, b.Controls, eiRoot);
                        cr.Message = b.Message;
                        cr.Type = b.Type.ToString();
                        cr.RuleName = RuleManager.RuleList[i].Name;
                        list.Add(cr);
                    }
                }
            }
            return list;
        }
        public List<string> AdOpUpdateClientsList()
        {
            try
            {
                foreach (KeyValuePair<string, ICallBack> pair in clients)
                {
                    if (!IsCallBackValid(pair.Value))
                    {
                        clients.Remove(pair.Key);
                    }
                }
                return new List<string>(clients.Keys);
            }
            catch (Exception e)
            {
                Log.WriteServerLog(e.Message, TraceLevel.Error);
                return new List<string>();
            }
        }
        public List<ComplianceResult> AdOpVerifySpecifiedUI(CapturedData data)
        {
            return VerifySingleUI(data);
        }
        public void RescanByBuildNo1(string buildNo, List<string> rules)
        {
            try
            {
                this.verifyThread.RescanByBuildNo(buildNo, rules);
            }
            catch (Exception e) {
                Log.WriteServerLog(e.Message, TraceLevel.Error);
            }
        }
        public void ChangeLogSwith(int i)
        {
            switch (i)
            {
                case 0:
                    Log.generalSwitch = new System.Diagnostics.TraceSwitch("LogSwitch0", "Switch server log");
                    break;
                case 1:
                    Log.generalSwitch = new System.Diagnostics.TraceSwitch("LogSwitch1", "Switch server log");
                    break;
                case 2:
                    Log.generalSwitch = new System.Diagnostics.TraceSwitch("LogSwitch2", "Switch server log");
                    break;
                case 3:
                    Log.generalSwitch = new System.Diagnostics.TraceSwitch("LogSwitch3", "Switch server log");
                    break;
                case 4:
                    Log.generalSwitch = new System.Diagnostics.TraceSwitch("LogSwitch4", "Switch server log");
                    break;
            }
            Log.WriteServerLog(string.Format("Log Switch Change to:{0}", i),TraceLevel.Warning);
        }
        public void RescanByContentList(List<int> contentList, List<string> rules)
        {
            verifyThread.RescanByContentList(contentList, rules);
        }
        
        public void UploadLog(string ExceptionContent, AdditionInformations additionInformations)
        {
            try
            {
                #region change the buildNo to old Format
                //no need to use reflection
                //string standardVersion = dBOperator.InvokeMember("ChangeBuildNo", BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, null, null, new string[] { additionInformations.ProductVersion.ToString() }).ToString();
                string standardVersion = DBOperator.ChangeBuildNo(additionInformations.ProductVersion.ToString());
                additionInformations.ProductVersion = new Version(standardVersion);

                //ignore abnormal versioin
                if (string.IsNullOrEmpty(standardVersion) || !CMUtility.IsStandardFormat(standardVersion))
                    return;

                #endregion
                //Get Client ID
                ISulpHurTable table = SulpHurTableFactoryBase.Instance().GetSulpHurTable();
                DataRowAdapter adapter = new DataRowAdapter(additionInformations);
                SulpHurClientIdentity identity = adapter.GetRow<SulpHurClientIdentity>();
                if (!identity.IsExist)
                {
                    SulpHurClientInfo row = adapter.GetRow<SulpHurClientInfo>();
                    table.InsertRow<SulpHurClientInfo>(row);
                }
                int clientid = identity.ID;

                //Get Build ID
                SulpHurBuildInfo buildinfo = adapter.GetRow<SulpHurBuildInfo>();
                if (!buildinfo.IsExist)
                {
                    table.InsertRow<SulpHurBuildInfo>(buildinfo);
                    Log.WriteServerLog(String.Format("SulphurWCFService.UploadLog,buildinfo.IsExist=FALSE,buildno:{0},language:{1}",buildinfo.BuildNo,buildinfo.Language),TraceLevel.Warning);
                    return;
                }
                int buildID = buildinfo.ID;

                LogException le = new LogException();
                le.ClientID = clientid;
                le.BuildID = buildID;
                le.ExceptionContent = ExceptionContent;
                if (!le.IsExist)
                {
                    le.InsertTime = DateTime.Now;
                    le.LastModifyTime = DateTime.Now;
                    le.ExceptionCount = 1;
                    table.InsertRow<LogException>(le);
                }
                else
                {
                    table.UpdateRow<LogException>(le);
                }
            }
            catch (Exception e) {
                Log.WriteServerLog(e.ToString(), TraceLevel.Error);
            }
        }
    }

    

}
