using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SulpHurServiceAbstract
{
    public enum LogSwitchLevel { 
        OFF,
        ERROR,
        WARNING,
        VERBOSE,
        INFO
    }
    public interface IWinService
    {
        void SetRuleStatus(bool state, string ruleName);
        void ReLoadRules();
        //List<string> GetClients();
        //ForegroundData GetForegroundData(string computerName);
        //List<ComplianceResult> GetVerifyResult(string computerName);
        //List<ComplianceResult> VerifySingleUI(CapturedData data);
        void SetLogSwitch(LogSwitchLevel lsl);
        bool Connect();
        void ReScanByBuildNo(string buildNo);
        void ReScanByBuildNo(string buildNo, List<string> rules);
        void ReScanByContentList(List<int> contentList, List<string> rules);
        void RescanByContentID(List<int> contentList, List<string> rules);
    }
    //public interface IRescan {

    //}
    public interface IBuildClean
    {
        void CleanBuild(string buildno);
        void RecoverBuild(string buildno);
        void CleanUI(int contentID);
        void RecoverUI(int contentID);
    }
    public abstract class WCFServiceWrapperBase : BaseFactory
    {
        private static WCFServiceWrapperBase wrapper = null;
        private static string AssemblySettingName
        {
            get { return "WCFServiceWrapperAssembly"; }
        }

        private static string ClassNameSettingName
        {
            get { return "WCFServiceWrapperClassName"; }
        }

        private static string DefaultAssemblyName
        {
            get { return "SulpHurServiceImp.dll"; }
        }

        private static string DefaultClassName
        {
            get { return "SulpHurServiceImplements.WCFServiceWrapper"; }
        }

        public static WCFServiceWrapperBase Instance()
        {
            if (wrapper == null)
            {
                wrapper = (WCFServiceWrapperBase)BaseFactory.Instance(WCFServiceWrapperBase.AssemblySettingName, WCFServiceWrapperBase.ClassNameSettingName, WCFServiceWrapperBase.DefaultAssemblyName, WCFServiceWrapperBase.DefaultClassName);
            }

            return wrapper;
        }

        public abstract IWinService GetWindowsService(string serverName);

  //      public abstract IRescan GeScanner();
    }
}
