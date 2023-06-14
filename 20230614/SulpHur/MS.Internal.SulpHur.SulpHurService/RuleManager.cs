using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.UICompliance;
using System.IO;
using System.Reflection;

namespace MS.Internal.SulpHur.SulpHurService
{
    public class RuleManager : RuleOperations
    {
        private object obj = new object();

        private static List<UIComplianceRuleBase> ruleList = new List<UIComplianceRuleBase>();
        public static List<UIComplianceRuleBase> RuleList
        {
            get { return ruleList; }
            set { ruleList = value; }
        }

        public RuleManager()
        {
            AssemblyResolver.AddSearchPath(ServiceEnviroment.RULEFOLDER);
        }
        public void LoadRules()
        {
            //string[] assemblyFiles = Directory.GetFiles(ServiceEnviroment.RULEFOLDER, System.Configuration.ConfigurationManager.AppSettings["EnabledRuleDLL"]);
D            Log.WriteServerLog("Load rules from2:" + ServiceEnviroment.RULEFOLDER, TraceLevel.Info);
            Log.WriteServerLog("LoadRules1", TraceLevel.Info);
            Log.WriteServerLog(ServiceEnviroment.RULEFOLDER, TraceLevel.Info);
            Log.WriteServerLog("LoadRules2", TraceLevel.Info);
            Log.WriteServerLog(System.Configuration.ConfigurationManager.AppSettings["EnabledRuleDLL"], TraceLevel.Info);
            Log.WriteServerLog("LoadRules3", TraceLevel.Info);
            string ruleFile = Path.Combine(ServiceEnviroment.RULEFOLDER, System.Configuration.ConfigurationManager.AppSettings["EnabledRuleDLL"]);
            Log.WriteServerLog(string.Format("Load rules from dll:{0}", ruleFile), TraceLevel.Info);


            LoadRule(ruleFile);

            if (ruleList.Count > 0)
            {               
                using (DBOperator dbOperator = new DBOperator())
                {
                    dbOperator.UpdateRule(ruleList);
                }
            }
        }

        //public UIComplianceRuleBase GetRuleByName(string name)
        //{
        //    try
        //    {
        //        return ruleList.SingleOrDefault(r => r.Name == name);
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

        private void LoadRule(string ruleFile)
        {   
            //FileInfo fi = new FileInfo(ruleFile);
            //if (!fi.Name.StartsWith("MS.Internal.SulpHur")) return;

            // Load the assembly
            Assembly assembly = null;
            try
            {
                assembly = Assembly.LoadFile(ruleFile);
                Log.WriteServerLog(string.Format("Load Rule File: {0}", ruleFile),TraceLevel.Verbose);
            }
            catch (Exception ex)
            {
                Log.WriteServerLog("Load rule exception :" + ex.Message,TraceLevel.Error);
            }

            // Load the Rule from assembly
            foreach (Type type in assembly.GetTypes())
            {
                if (IsSubclassOf(type, typeof(UIComplianceRuleBase)))
                {
                    try
                    {
                        UIComplianceRuleBase rule = (UIComplianceRuleBase)assembly.CreateInstance(type.FullName);
                        ruleList.Add(rule);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteServerLog("fail to create rule instance exception:" + ex.Message + ",type:" + type.Name,TraceLevel.Error);
                    }
                }
            }
        }

        private bool IsSubclassOf(Type t, Type b)
        {
            if (t.BaseType == null) return false;
            if (t.BaseType.Equals(b)) return true;
            return IsSubclassOf(t.BaseType, b);
        }

        public void ReLoadLoadRules()
        {
            lock (obj)
            {
                ruleList.Clear();
                LoadRules();
            }
        }

        public List<UIComplianceRuleBase> GetRuleList()
        {
            return ruleList;
        }

        public void SetRuleStatus(bool state, string ruleName)
        {
            try
            {
                ruleList.SingleOrDefault(r => r.Name == ruleName).IsEnabled = state;
                using (DBOperator dbOperator = new DBOperator())
                {
                    dbOperator.SetRuleStatus(state, ruleName);
                }
            }
            catch
            {
                return;
            }
        }
    }


    public interface RuleOperations
    {
        void ReLoadLoadRules();

        List<UIComplianceRuleBase> GetRuleList();

        void SetRuleStatus(bool state, string ruleName);
    }
}