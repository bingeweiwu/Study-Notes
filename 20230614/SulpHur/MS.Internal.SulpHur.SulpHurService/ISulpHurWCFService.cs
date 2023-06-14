using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Drawing;
using MS.Internal.SulpHur.UICompliance;

namespace MS.Internal.SulpHur.SulpHurService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ISulpHurWCFService" in both code and config file together.
    [ServiceContract(SessionMode = SessionMode.Required,CallbackContract=typeof(ICallBack))]
    [ServiceKnownType(typeof(ElementInformation))]
    [ServiceKnownType(typeof(WindowPageInfo))]
    public interface ISulpHurWCFService
    {
        [OperationContract]
        UploadResults ReceiveWebUI(WebPageInfo webPageInfo, Bitmap bitmap, AdditionInformations additionInformations);
        [OperationContract]
        UploadResults ReceiveUI(WindowPageInfo root, Bitmap bitmap, AdditionInformations additionInformations);
        [OperationContract]
        List<ComplianceRule> GetRules();
        [OperationContract]
        ComplianceResult QueryResult(string UIName);
        [OperationContract]
        void SetRuleStatus(bool status, string ruleName);
        [OperationContract]
        void ReloadRules();
        [OperationContract]
        void RescanByBuildNo(string buildNo);
        [OperationContract]
        void RescanByBuildNo1(string buildNo,List<string> rules);
        [OperationContract]
        List<string> GetBuildList();
        [OperationContract]
        void Register(string ComputerName);
        [OperationContract]
        void AdminLogin();
        [OperationContract]
        ForegroundData AdOpGetForegroundData(string computerName);
        [OperationContract]
        List<ComplianceResult> AdOpVerifyUI(string computerName);
        [OperationContract]
        List<string> AdOpUpdateClientsList();
        [OperationContract]
        List<ComplianceResult> AdOpVerifySpecifiedUI(CapturedData data);
        [OperationContract]
        void ChangeLogSwith(int i);
        [OperationContract]
        void RescanByContentList(List<int> contentList, List<string> rules);
        [OperationContract]
        void UploadLog(string ExceptionContent,AdditionInformations additionInformations);
        //[OperationContract]
        //string GetServerName();
    }

    public interface ICallBack
    {
        [OperationContract]
        ForegroundData getForegroundData();

        [OperationContract]
        CapturedData capturedData();
    }
}
