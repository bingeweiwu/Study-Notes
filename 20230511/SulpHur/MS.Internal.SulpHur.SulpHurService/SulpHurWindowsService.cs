using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.ServiceModel;
using System.IO;
using System.Xml.Linq;
using System.Configuration;

namespace MS.Internal.SulpHur.SulpHurService
{
    public partial class SulpHurWindowsService : ServiceBase
    {
        internal static ServiceHost sulpHurServiceHost = null;
        RuleManager ruleManager;
        public UIContentVerification uiVerify;

        public SulpHurWindowsService()
        {
            InitializeComponent();
            this.ServiceName = ServiceEnviroment._WinSERVICENAME;
        }

        protected override void OnStart(string[] args)
        {
            #if DEBUG
            System.Threading.Thread.Sleep(20000);
            #endif

            Log.WriteServerLog("Starting Service...", TraceLevel.Info);
            PrepareEnviroment();

            ruleManager = new RuleManager();
            uiVerify = new UIContentVerification();

            bool isEnable = bool.Parse(SLConfigurationReader.ReadCustomSection("IsVerifyThreadEnabled"));
            if (isEnable)
            {
                Log.WriteServerLog("Load rules from:" + ServiceEnviroment.RULEFOLDER,TraceLevel.Info);
                ruleManager.LoadRules();

                Log.WriteServerLog("Starting verify....",TraceLevel.Info);
                uiVerify.Start();
            }

            if (sulpHurServiceHost != null)
            {
                sulpHurServiceHost.Close();
            }

            Log.WriteServerLog("Start WCF Service...",TraceLevel.Info);
            sulpHurServiceHost = new SulpHurServiceHost(ruleManager, uiVerify, typeof(SulpHurWCFService));
            sulpHurServiceHost.Open();
        }

        protected override void OnStop()
        {
            Log.WriteServerLog("Stopping windows service.",TraceLevel.Info);
            if (uiVerify != null)
            {
                if (uiVerify.IsAlive())
                {
                    Log.WriteServerLog("Stop verify.",TraceLevel.Info);
                    uiVerify.Stop();
                }
            }

            if (sulpHurServiceHost != null)
            {
                Log.WriteServerLog("Stop WCF Service....",TraceLevel.Info);
                sulpHurServiceHost.Close();
                sulpHurServiceHost = null;
            }

            AssemblyResolver.Uninstall();
        }

        public void PrepareEnviroment()
        {
            ServiceEnviroment.RULEFOLDER = SLConfigurationReader.ReadCustomSection("RuleFolder");
            ServiceEnviroment._SERVERSHOWNAME = SLConfigurationReader.ReadCustomSection("ServerName");

            if (!System.IO.Directory.Exists(ServiceEnviroment.RULEFOLDER))
            {
                System.IO.Directory.CreateDirectory(ServiceEnviroment.RULEFOLDER);
            }

            AssemblyResolver.Install();
        }
    }
}
