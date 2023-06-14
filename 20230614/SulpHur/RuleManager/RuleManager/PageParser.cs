using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using Microsoft.ConfigurationManagement.AdminConsole;
using System.Xml.Serialization;

namespace RuleManager
{
    public class SmsFormDataParseResult
    {
        public SmsFormType formType = SmsFormType.Unknown;
        public List<Type> controls = new List<Type>();
        /// <summary>
        /// FormName
        /// PageName
        /// </summary>
        public List<PageControl> ReturnList = new List<PageControl>();
        public Dictionary<Type, string> result = new Dictionary<Type, string>();
    }

    public class SmsFormDataParser
    {
        private string binDir;
        private StringBuilder parseLog = new StringBuilder();
        public string LastParseLog
        {
            get { return parseLog.ToString(); }
        }

        public Dictionary<string, string> DataPageControlDir = new Dictionary<string, string>();

        public SmsFormDataParser(string binDir)
        {
            this.binDir = binDir;

            AssemblyResolver.AddSearchPath(this.binDir);
            AssemblyResolver.AddSearchPath(Environment.SystemDirectory);
        }

        public void AddAssemblySearchPath(string path)
        {
            AssemblyResolver.AddSearchPath(path);
        }

        private bool AddControlToResult(SmsFormDataParseResult result, Type type)
        {
            bool hasDuplicate = false;
            foreach (Type control in result.controls)
                if (control.Equals(type))
                {
                    hasDuplicate = true;
                    break;
                }
            if (!hasDuplicate)
            {
                result.controls.Add(type);
                return true;
            }
            return false;
        }
        private void AddControlToResult(SmsFormDataParseResult result, string pageName, string FileName)
        {
            bool hasDuplicate = false;
            if (result.ReturnList.Count > 0)
            {
                foreach (PageControl page in result.ReturnList)
                    if (page.PageName.Equals(pageName) && page.FileName.Equals(FileName))
                    {
                        hasDuplicate = true;
                        break;
                    }
                if (!hasDuplicate)
                {
                    result.ReturnList.Add(new PageControl(pageName, FileName));

                }
            }
            else
                result.ReturnList.Add(new PageControl(pageName, FileName));
        }

        public SmsFormDataParseResult Parse(string xmlFile)
        {
            try
            {

                //PageScanner.OutputString(string.Format("Parsing {0} ...", xmlFile));
                parseLog.Remove(0, parseLog.Length);
                parseLog.AppendLine("XML: " + xmlFile);

                if (!File.Exists(xmlFile))
                {
                    parseLog.AppendLine("File not found");
                    return null;
                }

                FileStream fs = File.Open(xmlFile, FileMode.Open, FileAccess.Read);
                XmlSerializer xs = new XmlSerializer(typeof(SmsFormData));
                SmsFormData formData = (SmsFormData)xs.Deserialize(fs);
                fs.Close();

                SmsFormDataParseResult ret = new SmsFormDataParseResult();
                ret.formType = formData.Form.SmsFormType;
                string pagename = string.Empty;
                string assembleyNamespace = string.Empty;
                string file = string.Empty;
                switch (ret.formType)
                {
                    case SmsFormType.Dialog:
                        pagename = formData.Form.AssemblyData.Class;
                        assembleyNamespace = formData.Form.AssemblyData.Namespace;
                        file = formData.Form.AssemblyData.Assembly;
                        if (!file.EndsWith(".dll"))
                        {
                            file += ".dll";
                        }
                        if (string.IsNullOrEmpty(pagename))
                        {
                            pagename = formData.Form.Pages[0].Class;
                            AddControlToResult(ret, assembleyNamespace + "." + pagename, file);
                        }
                        else
                        {
                            parseLog.AppendLine("Type: " + pagename);
                            AddControlToResult(ret, assembleyNamespace + "." + pagename, file);
                            parseLog.AppendLine(" -> OK");
                        }
                        break;
                    case SmsFormType.CustomDialog:
                        file = formData.Form.AssemblyData.Assembly;
                        if (!file.EndsWith(".dll"))
                        {
                            file += ".dll";
                        }
                        parseLog.AppendLine("Form type handler: CustomDialog/Dialog");
                        pagename = formData.Form.AssemblyData.Class;
                        parseLog.AppendLine("Type: " + pagename);
                        assembleyNamespace = formData.Form.AssemblyData.Namespace;
                        AddControlToResult(ret, assembleyNamespace + "." + pagename, file);
                        parseLog.AppendLine(" -> OK");
                        break;
                    default:
                        foreach (SmsPageXmlData page in formData.Form.Pages)
                        {
                            file = formData.Form.AssemblyData.Assembly;
                            parseLog.AppendLine("Form type handler: Default... ");
                            assembleyNamespace = formData.Form.AssemblyData.Namespace;
                            if (!string.IsNullOrEmpty(page.Namespace))
                            {
                                assembleyNamespace = page.Namespace;
                            }
                            if (!string.IsNullOrEmpty(page.Assembly)) {
                                file = page.Assembly;
                            }
                            if (!file.EndsWith(".dll"))
                            {
                                file += ".dll";
                            }
                            AddControlToResult(ret, assembleyNamespace + "." + page.Class, file);
                            parseLog.AppendLine(" -> OK");
                        }
                        break;
                }
                return ret;
            }
            catch (Exception ex)
            {
                parseLog.AppendLine("Exception caught: " + ex.Message);
                return null;
            }
        }

        private bool IsSubclassOf(Type type, string baseClass)
        {
            if (type == null)
                return false;
            if (type.FullName == baseClass)
                return true;
            return IsSubclassOf(type.BaseType, baseClass);
        }

        private ManagedMD.Type DASM_FindType(ManagedMD.Module dasm_module, string type)
        {
            foreach (ManagedMD.Type dasm_type in dasm_module.Types)
                if (type == dasm_type.NameSpace + "." + dasm_type.Name)
                    return dasm_type;
            return null;
        }
    }

    public static class AssemblyResolver
    {
        private static List<string> binSearchPaths = new List<string>();

        private static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in loadedAssemblies)
                if (0 == StringComparer.InvariantCultureIgnoreCase.Compare(assembly.FullName, args.Name))
                    return assembly;

            AssemblyName assemblyName = new AssemblyName(args.Name);
            string dllName = assemblyName.Name + ".dll";
            string exeName = assemblyName.Name + ".exe";
            for (int n = 0; n < binSearchPaths.Count; n++)
            {
                string dllPath = Path.Combine(binSearchPaths[n], dllName);
                string exePath = Path.Combine(binSearchPaths[n], exeName);
                if (File.Exists(dllPath))
                    return Assembly.LoadFrom(dllPath);
                if (File.Exists(exePath))
                    return Assembly.LoadFrom(exePath);
            }

            return null;
        }

        public static void Install()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyResolve);
        }

        public static void Uninstall()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(AssemblyResolve);
        }

        public static void AddSearchPath(string path)
        {
            if (!path.EndsWith("\\"))
                path = path + "\\";

            if (!binSearchPaths.Contains(path))
                binSearchPaths.Add(path);
        }

        public static void ClearSearchPath()
        {
            binSearchPaths.Clear();
        }
    }

    public class SMSPageControlParser
    {
        private string assemblyPath;
        private string binDir;
        private StringBuilder parseLog = new StringBuilder();

        public SMSPageControlParser(string assemblyPath)
        {
            FileInfo fi = new FileInfo(assemblyPath);
            this.binDir = fi.Directory.FullName;
            if (this.binDir.EndsWith("\\") == false)
                this.binDir = this.binDir + "\\";
            this.assemblyPath = assemblyPath;
            AssemblyResolver.AddSearchPath(binDir);
            AssemblyResolver.AddSearchPath(Environment.SystemDirectory);
        }

        public void Parse()
        {
            GetSMSTypes();
            ManagedMD.Module dasm_module = new ManagedMD.Module(assemblyPath);
            List<ManagedMD.Type> dasm_types = DASM_GetSMSTypes(dasm_module);

        }

        public SmsFormDataParseResult ParseDll()
        {

            //PageScanner.OutputString(string.Format("Parsing {0} ...", assemblyPath));
            List<Type> types = GetSMSTypes();

            SmsFormDataParseResult ret = new SmsFormDataParseResult();
            ret.formType = SmsFormType.Wizard;
            ret.controls = types;
            //ret.formType = SmsFormType.;

            return ret;
        }

        public SmsFormDataParseResult ParseDllforDialogs()
        {

            //PageScanner.OutputString(string.Format("Parsing {0} ...", assemblyPath));
            List<Type> types = GetSMSCustomDialogs();

            SmsFormDataParseResult ret = new SmsFormDataParseResult();
            ret.formType = SmsFormType.Wizard;
            ret.controls = types;
            //ret.formType = SmsFormType.;

            return ret;
        }

        public SmsFormDataParseResult ParseDllforDialogsandControls()
        {

            //PageScanner.OutputString(string.Format("Parsing {0} ...", assemblyPath));
            Dictionary<Type, string> result = GetSMSDialogsandPageTypes();

            SmsFormDataParseResult ret = new SmsFormDataParseResult();
            ret.formType = SmsFormType.Wizard;
            ret.controls = GetSMSDialogs();
            ret.result = result;
            //ret.formType = SmsFormType.;

            return ret;
        }

        public static bool IsSMSForm(Type type)
        {
            Type bType = type;
            while (bType != null)
            {
                if (bType.FullName == "Microsoft.ConfigurationManagement.AdminConsole.SmsWizardForm")
                {
                    return false;
                }
                if (bType == typeof(System.Windows.Forms.Form))
                    return true;
                bType = bType.BaseType;
            }
            return false;
        }
        public static string GetSMSPageType(Type type)
        {
            Type bType = type;
            while (bType != null)
            {
                if (bType == typeof(SmsWizardForm) || bType == typeof(SmsPropertyPage)
                    || bType == typeof(Microsoft.ConfigurationManagement.AdminConsole.WizardFramework.WizardPage)
                    || bType == typeof(System.Windows.Forms.Form)
                    || bType == typeof(Microsoft.ConfigurationManagement.AdminConsole.Common.SmsWpfDialogBase)
                    || bType == typeof(Microsoft.ConfigurationManagement.AdminConsole.SmsPageControl)
                    )
                    return "Dialogs";

                if (bType == typeof(System.Windows.Forms.UserControl))
                    return bType.Name;

                bType = bType.BaseType;
            }
            return "";
        }
        public static bool IsUserControl(Type type)
        {
            Type bType = type.BaseType;
            while (bType != null)
            {
                //WriteToDailyCopyLog("Finding Propertysheet");
                if (bType.FullName =="Microsoft.ConfigurationManagement.AdminConsole.SmsPropertyPage")
                    return false;
                if (bType.FullName == "Microsoft.ConfigurationManagement.AdminConsole.WizardFramework.WizardPage")
                    return false;
                if (bType == typeof(System.Windows.Forms.UserControl))
                    return true;
                bType = bType.BaseType;
            }
            return false;
        }

        public static bool IsSMSWpfControl(Type type)
        {
            Type bType = type.BaseType;
            while (bType != null)
            {
                if (bType == typeof(Microsoft.ConfigurationManagement.AdminConsole.Common.SmsWpfDialogBase))
                    return true;
                bType = bType.BaseType;
            }
            return false;
        }

        public static bool IsPageControl(Type type)
        {
            Type bType = type.BaseType;
            while (bType != null)
            {

                if (bType.FullName == "Microsoft.ConfigurationManagement.AdminConsole.SmsPageControl")
                    return true;
                bType = bType.BaseType;
            }
            return false;
        }

        public List<Type> GetSMSTypes()
        {
            //AssemblyResolver.AddSearchPath(binDir);
            //AssemblyResolver.AddSearchPath(Environment.SystemDirectory);
            AssemblyResolver.Install();

            List<Type> results = new List<Type>();
            try
            {
                Assembly asm = Assembly.LoadFile(assemblyPath);
                foreach (Type t1 in asm.GetTypes().ToList())
                {
                    if (IsPageControl(t1) || IsSMSForm(t1) || IsUserControl(t1) || IsSMSWpfControl(t1))
                    {
                        results.Add(t1);
                        continue;
                    }
                }
                return results;
            }
            catch
            {
                return results;
            }
            finally
            {
                AssemblyResolver.Uninstall();
            }

        }

        public List<Type> GetSMSCustomDialogs()
        {
            //AssemblyResolver.AddSearchPath(binDir);
            //AssemblyResolver.AddSearchPath(Environment.SystemDirectory);
            AssemblyResolver.Install();

            List<Type> results = new List<Type>();
            try
            {
                Assembly asm = Assembly.LoadFile(assemblyPath);
                foreach (Type t1 in asm.GetTypes().ToList())
                {
                    if (IsSMSForm(t1))
                    {
                        results.Add(t1);
                        continue;
                    }
                }
                return results;
            }
            catch
            {
                return results;
            }
            finally
            {
                AssemblyResolver.Uninstall();
            }

        }

        public Dictionary<Type, string> GetSMSDialogsandPageTypes()
        {
            Dictionary<Type, string> result = new Dictionary<Type, string>();
            try
            {
                Assembly asm = Assembly.LoadFrom(assemblyPath);

                foreach (Type t1 in asm.GetTypes().ToList())
                {
                    if (t1.FullName == "Microsoft.ConfigurationManagement.AdminConsole.CollectionProperty.GeneralControl")
                    {
                        int debug = 1;  
                    }
                    if (IsPageControl(t1))
                    {
                        result.Add(t1, GetSMSPageType(t1));
                        continue;
                    }
                    if (IsSMSForm(t1))
                    {
                        result.Add(t1, GetSMSPageType(t1));
                        continue;
                    }
                    if (IsUserControl(t1))
                    {
                        result.Add(t1, GetSMSPageType(t1));
                        continue;
                    }
                }
                return result;
            }
            catch
            {
                return result;
            }
        }


        public List<Type> GetSMSDialogs()
        {
            AssemblyResolver.Install();
            //Dictionary<Type, string> result = new Dictionary<Type, string>();
            List<Type> results = new List<Type>();
            try
            {
                Assembly asm = Assembly.LoadFile(assemblyPath);
                foreach (Type t1 in asm.GetTypes().ToList())
                {
                    if (IsPageControl(t1))
                    {
                        results.Add(t1);
                        continue;
                    }
                    if (IsSMSForm(t1))
                    {
                        results.Add(t1);
                        continue;
                    }
                    if (IsUserControl(t1))
                    {
                        results.Add(t1);
                        continue;
                    }
                }
                return results;
            }
            catch
            {
                return results;
            }
            finally
            {
                AssemblyResolver.Uninstall();
            }

        }

        private List<ManagedMD.Type> DASM_GetSMSTypes(ManagedMD.Module dasm_module)
        {
            List<ManagedMD.Type> results = new List<ManagedMD.Type>();
            StringBuilder re = new StringBuilder();

            foreach (ManagedMD.Type dasm_type in dasm_module.Types)
            {
                //Type t1 = dasm_type.BaseType.;
                if (dasm_type.BaseType != null)
                {
                    if (dasm_type.Name == "Form" || dasm_type.Name == "SmsPageControl")
                        results.Add(dasm_type);
                    re.AppendLine(string.Format("{0}    {1}    {2}     {3}", dasm_type.NameSpace, dasm_type.Name, dasm_type.BaseType.NameSpace, dasm_type.BaseType.Name));
                }
                string n1 = dasm_type.Name;
                string n2 = dasm_type.NameSpace;
            }

            //foreach (ManagedMD.Type dasm_type in dasm_module.Types)
            //    if (type == dasm_type.NameSpace + "." + dasm_type.Name)
            //        return dasm_type;
            return results;
        }

        static void WriteToDailyCopyLog(string line)
        {
            try
            {
                string mCopyDLLog = "c:\\telescope\\SMSPageControlParser.txt";
                System.IO.FileInfo fi = new System.IO.FileInfo(mCopyDLLog);
                if (!fi.Directory.Exists)
                    fi.Directory.Create();
                System.IO.FileStream fs = new System.IO.FileStream(mCopyDLLog, System.IO.FileMode.Append, System.IO.FileAccess.Write);
                System.IO.StreamWriter sw = new System.IO.StreamWriter(fs);
                sw.WriteLine(string.Format("[{0}] {1}", DateTime.Now.ToString(), line));
                sw.Close();
                fs.Close();
            }
            catch
            {
                //eat it
            }
        }



    }
}
