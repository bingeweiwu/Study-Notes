﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.269
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
[System.ServiceModel.ServiceContractAttribute(ConfigurationName="ISulpHurWCFService", CallbackContract=typeof(ISulpHurWCFServiceCallback), SessionMode=System.ServiceModel.SessionMode.Required)]
public interface ISulpHurWCFService
{
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISulpHurWCFService/ReceiveWebUI", ReplyAction="http://tempuri.org/ISulpHurWCFService/ReceiveWebUIResponse")]
    MS.Internal.SulpHur.UICompliance.UploadResults ReceiveWebUI(MS.Internal.SulpHur.UICompliance.WebPageInfo webPageInfo, System.Drawing.Bitmap bitmap, MS.Internal.SulpHur.UICompliance.AdditionInformations additionInformations);
    
    [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/ISulpHurWCFService/ReceiveWebUI", ReplyAction="http://tempuri.org/ISulpHurWCFService/ReceiveWebUIResponse")]
    System.IAsyncResult BeginReceiveWebUI(MS.Internal.SulpHur.UICompliance.WebPageInfo webPageInfo, System.Drawing.Bitmap bitmap, MS.Internal.SulpHur.UICompliance.AdditionInformations additionInformations, System.AsyncCallback callback, object asyncState);
    
    MS.Internal.SulpHur.UICompliance.UploadResults EndReceiveWebUI(System.IAsyncResult result);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISulpHurWCFService/ReceiveUI", ReplyAction="http://tempuri.org/ISulpHurWCFService/ReceiveUIResponse")]
    MS.Internal.SulpHur.UICompliance.UploadResults ReceiveUI(MS.Internal.SulpHur.UICompliance.WindowPageInfo root, System.Drawing.Bitmap bitmap, MS.Internal.SulpHur.UICompliance.AdditionInformations additionInformations);
    
    [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/ISulpHurWCFService/ReceiveUI", ReplyAction="http://tempuri.org/ISulpHurWCFService/ReceiveUIResponse")]
    System.IAsyncResult BeginReceiveUI(MS.Internal.SulpHur.UICompliance.WindowPageInfo root, System.Drawing.Bitmap bitmap, MS.Internal.SulpHur.UICompliance.AdditionInformations additionInformations, System.AsyncCallback callback, object asyncState);
    
    MS.Internal.SulpHur.UICompliance.UploadResults EndReceiveUI(System.IAsyncResult result);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISulpHurWCFService/GetServerName", ReplyAction="http://tempuri.org/ISulpHurWCFService/GetServerNameResponse")]
    string GetServerName();
    
    [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/ISulpHurWCFService/GetServerName", ReplyAction="http://tempuri.org/ISulpHurWCFService/GetServerNameResponse")]
    System.IAsyncResult BeginGetServerName(System.AsyncCallback callback, object asyncState);
    
    string EndGetServerName(System.IAsyncResult result);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISulpHurWCFService/GetRules", ReplyAction="http://tempuri.org/ISulpHurWCFService/GetRulesResponse")]
    MS.Internal.SulpHur.UICompliance.ComplianceRule[] GetRules();
    
    [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/ISulpHurWCFService/GetRules", ReplyAction="http://tempuri.org/ISulpHurWCFService/GetRulesResponse")]
    System.IAsyncResult BeginGetRules(System.AsyncCallback callback, object asyncState);
    
    MS.Internal.SulpHur.UICompliance.ComplianceRule[] EndGetRules(System.IAsyncResult result);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISulpHurWCFService/QueryResult", ReplyAction="http://tempuri.org/ISulpHurWCFService/QueryResultResponse")]
    MS.Internal.SulpHur.UICompliance.ComplianceResult QueryResult(string UIName);
    
    [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/ISulpHurWCFService/QueryResult", ReplyAction="http://tempuri.org/ISulpHurWCFService/QueryResultResponse")]
    System.IAsyncResult BeginQueryResult(string UIName, System.AsyncCallback callback, object asyncState);
    
    MS.Internal.SulpHur.UICompliance.ComplianceResult EndQueryResult(System.IAsyncResult result);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISulpHurWCFService/SetRuleStatus", ReplyAction="http://tempuri.org/ISulpHurWCFService/SetRuleStatusResponse")]
    void SetRuleStatus(bool status, string ruleName);
    
    [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/ISulpHurWCFService/SetRuleStatus", ReplyAction="http://tempuri.org/ISulpHurWCFService/SetRuleStatusResponse")]
    System.IAsyncResult BeginSetRuleStatus(bool status, string ruleName, System.AsyncCallback callback, object asyncState);
    
    void EndSetRuleStatus(System.IAsyncResult result);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISulpHurWCFService/ReloadRules", ReplyAction="http://tempuri.org/ISulpHurWCFService/ReloadRulesResponse")]
    void ReloadRules();
    
    [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/ISulpHurWCFService/ReloadRules", ReplyAction="http://tempuri.org/ISulpHurWCFService/ReloadRulesResponse")]
    System.IAsyncResult BeginReloadRules(System.AsyncCallback callback, object asyncState);
    
    void EndReloadRules(System.IAsyncResult result);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISulpHurWCFService/RescanByBuildNo", ReplyAction="http://tempuri.org/ISulpHurWCFService/RescanByBuildNoResponse")]
    void RescanByBuildNo(string buildNo);
    
    [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/ISulpHurWCFService/RescanByBuildNo", ReplyAction="http://tempuri.org/ISulpHurWCFService/RescanByBuildNoResponse")]
    System.IAsyncResult BeginRescanByBuildNo(string buildNo, System.AsyncCallback callback, object asyncState);
    
    void EndRescanByBuildNo(System.IAsyncResult result);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISulpHurWCFService/GetBuildList", ReplyAction="http://tempuri.org/ISulpHurWCFService/GetBuildListResponse")]
    string[] GetBuildList();
    
    [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/ISulpHurWCFService/GetBuildList", ReplyAction="http://tempuri.org/ISulpHurWCFService/GetBuildListResponse")]
    System.IAsyncResult BeginGetBuildList(System.AsyncCallback callback, object asyncState);
    
    string[] EndGetBuildList(System.IAsyncResult result);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISulpHurWCFService/Register", ReplyAction="http://tempuri.org/ISulpHurWCFService/RegisterResponse")]
    void Register(string ComputerName);
    
    [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/ISulpHurWCFService/Register", ReplyAction="http://tempuri.org/ISulpHurWCFService/RegisterResponse")]
    System.IAsyncResult BeginRegister(string ComputerName, System.AsyncCallback callback, object asyncState);
    
    void EndRegister(System.IAsyncResult result);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISulpHurWCFService/AdminLogin", ReplyAction="http://tempuri.org/ISulpHurWCFService/AdminLoginResponse")]
    void AdminLogin();
    
    [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/ISulpHurWCFService/AdminLogin", ReplyAction="http://tempuri.org/ISulpHurWCFService/AdminLoginResponse")]
    System.IAsyncResult BeginAdminLogin(System.AsyncCallback callback, object asyncState);
    
    void EndAdminLogin(System.IAsyncResult result);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISulpHurWCFService/AdOpGetForegroundData", ReplyAction="http://tempuri.org/ISulpHurWCFService/AdOpGetForegroundDataResponse")]
    MS.Internal.SulpHur.UICompliance.ForegroundData AdOpGetForegroundData(string computerName);
    
    [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/ISulpHurWCFService/AdOpGetForegroundData", ReplyAction="http://tempuri.org/ISulpHurWCFService/AdOpGetForegroundDataResponse")]
    System.IAsyncResult BeginAdOpGetForegroundData(string computerName, System.AsyncCallback callback, object asyncState);
    
    MS.Internal.SulpHur.UICompliance.ForegroundData EndAdOpGetForegroundData(System.IAsyncResult result);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISulpHurWCFService/AdOpVerifyUI", ReplyAction="http://tempuri.org/ISulpHurWCFService/AdOpVerifyUIResponse")]
    MS.Internal.SulpHur.UICompliance.ComplianceResult[] AdOpVerifyUI(string computerName);
    
    [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/ISulpHurWCFService/AdOpVerifyUI", ReplyAction="http://tempuri.org/ISulpHurWCFService/AdOpVerifyUIResponse")]
    System.IAsyncResult BeginAdOpVerifyUI(string computerName, System.AsyncCallback callback, object asyncState);
    
    MS.Internal.SulpHur.UICompliance.ComplianceResult[] EndAdOpVerifyUI(System.IAsyncResult result);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISulpHurWCFService/AdOpUpdateClientsList", ReplyAction="http://tempuri.org/ISulpHurWCFService/AdOpUpdateClientsListResponse")]
    string[] AdOpUpdateClientsList();
    
    [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/ISulpHurWCFService/AdOpUpdateClientsList", ReplyAction="http://tempuri.org/ISulpHurWCFService/AdOpUpdateClientsListResponse")]
    System.IAsyncResult BeginAdOpUpdateClientsList(System.AsyncCallback callback, object asyncState);
    
    string[] EndAdOpUpdateClientsList(System.IAsyncResult result);

    [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/ISulpHurWCFService/UploadLog", ReplyAction = "http://tempuri.org/ISulpHurWCFService/UploadLogResponse")]
    void UploadLog(string ExceptionContent, MS.Internal.SulpHur.UICompliance.AdditionInformations additionInformations);

    [System.ServiceModel.OperationContractAttribute(AsyncPattern = true, Action = "http://tempuri.org/ISulpHurWCFService/UploadLog", ReplyAction = "http://tempuri.org/ISulpHurWCFService/UploadLogResponse")]
    System.IAsyncResult BeginUploadLog(string ExceptionContent, MS.Internal.SulpHur.UICompliance.AdditionInformations additionInformations, System.AsyncCallback callback, object asyncState);

    void EndUploadLog(System.IAsyncResult result);
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
public interface ISulpHurWCFServiceCallback
{
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISulpHurWCFService/getForegroundData", ReplyAction="http://tempuri.org/ISulpHurWCFService/getForegroundDataResponse")]
    MS.Internal.SulpHur.UICompliance.ForegroundData getForegroundData();
    
    [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/ISulpHurWCFService/getForegroundData", ReplyAction="http://tempuri.org/ISulpHurWCFService/getForegroundDataResponse")]
    System.IAsyncResult BegingetForegroundData(System.AsyncCallback callback, object asyncState);
    
    MS.Internal.SulpHur.UICompliance.ForegroundData EndgetForegroundData(System.IAsyncResult result);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISulpHurWCFService/capturedData", ReplyAction="http://tempuri.org/ISulpHurWCFService/capturedDataResponse")]
    MS.Internal.SulpHur.UICompliance.CapturedData capturedData();
    
    [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/ISulpHurWCFService/capturedData", ReplyAction="http://tempuri.org/ISulpHurWCFService/capturedDataResponse")]
    System.IAsyncResult BegincapturedData(System.AsyncCallback callback, object asyncState);
    
    MS.Internal.SulpHur.UICompliance.CapturedData EndcapturedData(System.IAsyncResult result);
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
public interface ISulpHurWCFServiceChannel : ISulpHurWCFService, System.ServiceModel.IClientChannel
{
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
public partial class SulpHurWCFServiceClient : System.ServiceModel.DuplexClientBase<ISulpHurWCFService>, ISulpHurWCFService
{
    
    public SulpHurWCFServiceClient(System.ServiceModel.InstanceContext callbackInstance) : 
            base(callbackInstance)
    {
    }
    
    public SulpHurWCFServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName) : 
            base(callbackInstance, endpointConfigurationName)
    {
    }
    
    public SulpHurWCFServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress) : 
            base(callbackInstance, endpointConfigurationName, remoteAddress)
    {
    }
    
    public SulpHurWCFServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
            base(callbackInstance, endpointConfigurationName, remoteAddress)
    {
    }
    
    public SulpHurWCFServiceClient(System.ServiceModel.InstanceContext callbackInstance, System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
            base(callbackInstance, binding, remoteAddress)
    {
    }
    
    public MS.Internal.SulpHur.UICompliance.UploadResults ReceiveWebUI(MS.Internal.SulpHur.UICompliance.WebPageInfo webPageInfo, System.Drawing.Bitmap bitmap, MS.Internal.SulpHur.UICompliance.AdditionInformations additionInformations)
    {
        return base.Channel.ReceiveWebUI(webPageInfo, bitmap, additionInformations);
    }
    
    public System.IAsyncResult BeginReceiveWebUI(MS.Internal.SulpHur.UICompliance.WebPageInfo webPageInfo, System.Drawing.Bitmap bitmap, MS.Internal.SulpHur.UICompliance.AdditionInformations additionInformations, System.AsyncCallback callback, object asyncState)
    {
        return base.Channel.BeginReceiveWebUI(webPageInfo, bitmap, additionInformations, callback, asyncState);
    }
    
    public MS.Internal.SulpHur.UICompliance.UploadResults EndReceiveWebUI(System.IAsyncResult result)
    {
        return base.Channel.EndReceiveWebUI(result);
    }
    
    public MS.Internal.SulpHur.UICompliance.UploadResults ReceiveUI(MS.Internal.SulpHur.UICompliance.WindowPageInfo root, System.Drawing.Bitmap bitmap, MS.Internal.SulpHur.UICompliance.AdditionInformations additionInformations)
    {
        return base.Channel.ReceiveUI(root, bitmap, additionInformations);
    }
    
    public System.IAsyncResult BeginReceiveUI(MS.Internal.SulpHur.UICompliance.WindowPageInfo root, System.Drawing.Bitmap bitmap, MS.Internal.SulpHur.UICompliance.AdditionInformations additionInformations, System.AsyncCallback callback, object asyncState)
    {
        return base.Channel.BeginReceiveUI(root, bitmap, additionInformations, callback, asyncState);
    }
    
    public MS.Internal.SulpHur.UICompliance.UploadResults EndReceiveUI(System.IAsyncResult result)
    {
        return base.Channel.EndReceiveUI(result);
    }
    
    public string GetServerName()
    {
        return base.Channel.GetServerName();
    }
    
    public System.IAsyncResult BeginGetServerName(System.AsyncCallback callback, object asyncState)
    {
        return base.Channel.BeginGetServerName(callback, asyncState);
    }
    
    public string EndGetServerName(System.IAsyncResult result)
    {
        return base.Channel.EndGetServerName(result);
    }
    
    public MS.Internal.SulpHur.UICompliance.ComplianceRule[] GetRules()
    {
        return base.Channel.GetRules();
    }
    
    public System.IAsyncResult BeginGetRules(System.AsyncCallback callback, object asyncState)
    {
        return base.Channel.BeginGetRules(callback, asyncState);
    }
    
    public MS.Internal.SulpHur.UICompliance.ComplianceRule[] EndGetRules(System.IAsyncResult result)
    {
        return base.Channel.EndGetRules(result);
    }
    
    public MS.Internal.SulpHur.UICompliance.ComplianceResult QueryResult(string UIName)
    {
        return base.Channel.QueryResult(UIName);
    }
    
    public System.IAsyncResult BeginQueryResult(string UIName, System.AsyncCallback callback, object asyncState)
    {
        return base.Channel.BeginQueryResult(UIName, callback, asyncState);
    }
    
    public MS.Internal.SulpHur.UICompliance.ComplianceResult EndQueryResult(System.IAsyncResult result)
    {
        return base.Channel.EndQueryResult(result);
    }
    
    public void SetRuleStatus(bool status, string ruleName)
    {
        base.Channel.SetRuleStatus(status, ruleName);
    }
    
    public System.IAsyncResult BeginSetRuleStatus(bool status, string ruleName, System.AsyncCallback callback, object asyncState)
    {
        return base.Channel.BeginSetRuleStatus(status, ruleName, callback, asyncState);
    }
    
    public void EndSetRuleStatus(System.IAsyncResult result)
    {
        base.Channel.EndSetRuleStatus(result);
    }
    
    public void ReloadRules()
    {
        base.Channel.ReloadRules();
    }
    
    public System.IAsyncResult BeginReloadRules(System.AsyncCallback callback, object asyncState)
    {
        return base.Channel.BeginReloadRules(callback, asyncState);
    }
    
    public void EndReloadRules(System.IAsyncResult result)
    {
        base.Channel.EndReloadRules(result);
    }
    
    public void RescanByBuildNo(string buildNo)
    {
        base.Channel.RescanByBuildNo(buildNo);
    }
    
    public System.IAsyncResult BeginRescanByBuildNo(string buildNo, System.AsyncCallback callback, object asyncState)
    {
        return base.Channel.BeginRescanByBuildNo(buildNo, callback, asyncState);
    }
    
    public void EndRescanByBuildNo(System.IAsyncResult result)
    {
        base.Channel.EndRescanByBuildNo(result);
    }
    
    public string[] GetBuildList()
    {
        return base.Channel.GetBuildList();
    }
    
    public System.IAsyncResult BeginGetBuildList(System.AsyncCallback callback, object asyncState)
    {
        return base.Channel.BeginGetBuildList(callback, asyncState);
    }
    
    public string[] EndGetBuildList(System.IAsyncResult result)
    {
        return base.Channel.EndGetBuildList(result);
    }
    
    public void Register(string ComputerName)
    {
        base.Channel.Register(ComputerName);
    }
    
    public System.IAsyncResult BeginRegister(string ComputerName, System.AsyncCallback callback, object asyncState)
    {
        return base.Channel.BeginRegister(ComputerName, callback, asyncState);
    }
    
    public void EndRegister(System.IAsyncResult result)
    {
        base.Channel.EndRegister(result);
    }
    
    public void AdminLogin()
    {
        base.Channel.AdminLogin();
    }
    
    public System.IAsyncResult BeginAdminLogin(System.AsyncCallback callback, object asyncState)
    {
        return base.Channel.BeginAdminLogin(callback, asyncState);
    }
    
    public void EndAdminLogin(System.IAsyncResult result)
    {
        base.Channel.EndAdminLogin(result);
    }
    
    public MS.Internal.SulpHur.UICompliance.ForegroundData AdOpGetForegroundData(string computerName)
    {
        return base.Channel.AdOpGetForegroundData(computerName);
    }
    
    public System.IAsyncResult BeginAdOpGetForegroundData(string computerName, System.AsyncCallback callback, object asyncState)
    {
        return base.Channel.BeginAdOpGetForegroundData(computerName, callback, asyncState);
    }
    
    public MS.Internal.SulpHur.UICompliance.ForegroundData EndAdOpGetForegroundData(System.IAsyncResult result)
    {
        return base.Channel.EndAdOpGetForegroundData(result);
    }
    
    public MS.Internal.SulpHur.UICompliance.ComplianceResult[] AdOpVerifyUI(string computerName)
    {
        return base.Channel.AdOpVerifyUI(computerName);
    }
    
    public System.IAsyncResult BeginAdOpVerifyUI(string computerName, System.AsyncCallback callback, object asyncState)
    {
        return base.Channel.BeginAdOpVerifyUI(computerName, callback, asyncState);
    }
    
    public MS.Internal.SulpHur.UICompliance.ComplianceResult[] EndAdOpVerifyUI(System.IAsyncResult result)
    {
        return base.Channel.EndAdOpVerifyUI(result);
    }
    
    public string[] AdOpUpdateClientsList()
    {
        return base.Channel.AdOpUpdateClientsList();
    }
    
    public System.IAsyncResult BeginAdOpUpdateClientsList(System.AsyncCallback callback, object asyncState)
    {
        return base.Channel.BeginAdOpUpdateClientsList(callback, asyncState);
    }
    
    public string[] EndAdOpUpdateClientsList(System.IAsyncResult result)
    {
        return base.Channel.EndAdOpUpdateClientsList(result);
    }

    public void UploadLog(string ExceptionContent, MS.Internal.SulpHur.UICompliance.AdditionInformations additionInformations)
    {
        base.Channel.UploadLog(ExceptionContent, additionInformations);
    }

    public System.IAsyncResult BeginUploadLog(string ExceptionContent, MS.Internal.SulpHur.UICompliance.AdditionInformations additionInformations, System.AsyncCallback callback, object asyncState)
    {
        return base.Channel.BeginUploadLog(ExceptionContent, additionInformations, callback, asyncState);
    }

    public void EndUploadLog(System.IAsyncResult result)
    {
        base.Channel.EndUploadLog(result);
    }
}
