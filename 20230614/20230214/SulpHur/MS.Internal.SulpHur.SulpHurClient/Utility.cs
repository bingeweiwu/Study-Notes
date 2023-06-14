using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.Win32;
using MS.Internal.SulpHur.UICompliance;
using MS.Internal.SulpHur.Utilities;
using MS.Internal.SulpHur.Utilities.Exceptions;
using MS.Internal.SulpHur.SulpHurClient.Monitors;
using interop.UIAutomationCore;
using MS.Internal.SulpHur.SulpHurClient.UIA3;
using System.Reflection;
using System.Windows.Automation;
using System.Runtime;
using System.Runtime.InteropServices;
using PropertyConditionFlags = interop.UIAutomationCore.PropertyConditionFlags;
using System.Windows.Documents;
using MS.Internal.SulpHur.SulpHurClient.Common;

namespace MS.Internal.SulpHur.SulpHurClient
{
    public class Utility
    {
        public static IUIAutomationElement cachedActiveCMConsole = null;
        public static IUIAutomationElement cachedActiveWindow = null;
        public static string cachedLaunchedFrom = null;
        public static string cachedWindowHierarchy = null;
        /// <summary>
        /// Gets the directory name where UI is located.
        /// </summary>
        public static string ConsoleDirectory
        {
            get
            {
                try
                {
                    using (RegistryKey setupKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\ConfigMgr10\AdminUI\DialogFactory"))
                    {
                        if (setupKey != null)
                        {
                            string temp = (string)setupKey.GetValue("Assembly Path");//(string)setupKey.GetValue("UI Installation Directory");
                            if (!string.IsNullOrEmpty(temp))
                            //return temp + @"\bin";
                            {
                                int lastOne = temp.LastIndexOf("\\", StringComparison.Ordinal);
                                return temp.Remove(lastOne);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"ConsoleDirectory not exist{ex.Message}");
                }

                return null;
                //throw new NotSupportedException("UI Installation Directory cannot be found.");
            }
        }
        public static int ConsoleBuildNumber
        {
            get
            {
                string consoleExe = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", ConsoleDirectory, "Microsoft.ConfigurationManagement.exe");
                int buildNumberFromFile = FileVersionInfo.GetVersionInfo(consoleExe).FileBuildPart;
                //v-yiwzha For X64 OS
                using (RegistryKey consoleVersionKey = Registry.LocalMachine.OpenSubKey(@"software\wow6432Node\microsoft\configmgr10\setup"))
                {
                    if (consoleVersionKey != null)
                    {
                        string temp = (string)consoleVersionKey.GetValue("AdminConsoleVersion");
                        if (!string.IsNullOrEmpty(temp))
                        {
                            int buildNumberFromReg = int.Parse(temp.Substring(5, 4));
                            if (buildNumberFromFile != buildNumberFromReg)
                            {
                                return buildNumberFromReg;
                            }
                        }
                    }
                }
                //v-yiwzha For X86 OS
                using (RegistryKey consoleVersionKey = Registry.LocalMachine.OpenSubKey(@"software\microsoft\configmgr10\setup"))
                {
                    if (consoleVersionKey != null)
                    {
                        string temp = (string)consoleVersionKey.GetValue("AdminConsoleVersion");
                        if (!string.IsNullOrEmpty(temp))
                        {
                            int buildNumberFromReg = int.Parse(temp.Substring(5, 4));
                            if (buildNumberFromFile != buildNumberFromReg)
                            {
                                return buildNumberFromReg;
                            }
                        }
                    }
                }
                return buildNumberFromFile;
            }
        }
        public static string ConsoleVersion
        {
            get
            {
                string detaultResult = "0.00.0000.0000";
                string result = FindByReg();
                if (!string.IsNullOrEmpty(result))
                {
                    return result;
                }

                //if sherlock setting exists. HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\CMSherlockAgent
                RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                using (RegistryKey consoleVersionKey = hklm.OpenSubKey(@"software\wow6432Node\Microsoft\CMSherlockAgent"))
                {
                    if (consoleVersionKey != null)
                    {
                        string temp = (string)consoleVersionKey.GetValue("SiteVersion");
                        if (!string.IsNullOrEmpty(temp))
                        {
                            return temp;
                        }
                    }
                }
                using (RegistryKey consoleVersionKey = hklm.OpenSubKey(@"software\Microsoft\CMSherlockAgent"))
                {
                    if (consoleVersionKey != null)
                    {
                        string temp = (string)consoleVersionKey.GetValue("SiteVersion");
                        if (!string.IsNullOrEmpty(temp))
                        {
                            return temp;
                        }
                    }
                }

                //string consoleExe = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", ConsoleDirectory, "Microsoft.ConfigurationManagement.exe");
                //string consoleVersionFromFile = FileVersionInfo.GetVersionInfo(consoleExe).ProductVersion;
                ////v-yiwzha For x64 OS
                //using (RegistryKey consoleVersionKey = hklm.OpenSubKey(@"software\wow6432Node\microsoft\configmgr10\setup"))
                //{
                //    if (consoleVersionKey != null)
                //    {
                //        string temp = (string)consoleVersionKey.GetValue("AdminConsoleVersion");
                //        if (!string.IsNullOrEmpty(temp))
                //        {
                //            string consoleVersionFromReg = temp;
                //            if (consoleVersionFromFile != consoleVersionFromReg)
                //            {
                //                return consoleVersionFromReg;
                //            }
                //        }
                //    }
                //}
                ////v-yiwzha For x86 OS
                //using (RegistryKey consoleVersionKey = hklm.OpenSubKey(@"software\microsoft\configmgr10\setup"))
                //{
                //    if (consoleVersionKey != null)
                //    {
                //        string temp = (string)consoleVersionKey.GetValue("AdminConsoleVersion");
                //        if (!string.IsNullOrEmpty(temp))
                //        {
                //            string consoleVersionFromReg = temp;
                //            if (consoleVersionFromFile != consoleVersionFromReg)
                //            {
                //                return consoleVersionFromReg;
                //            }
                //        }
                //    }
                //}
                //return consoleVersionFromFile;

                return detaultResult;
            }
        }
        public static bool IsProductLanguagePackInstalled
        {
            get
            {
                if (GetProductCultureInfo() == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        //v-danpgu: Create a dictionary for the languages, to fix the language inaccuracy launching with LaunchSCCM tool.
        private static Dictionary<int, string> FillLanguageDictionary()
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();
            dic.Add(3076, "ZHH");
            dic.Add(2052, "CHS");
            dic.Add(1028, "CHT");
            dic.Add(1033, "ENU");
            dic.Add(1036, "FRA");
            dic.Add(1041, "JPN");
            dic.Add(1031, "DEU");
            dic.Add(1049, "RUS");
            dic.Add(1043, "NLD");
            dic.Add(1038, "HUN");
            dic.Add(1040, "ITA");
            dic.Add(1042, "KOR");
            dic.Add(1045, "PLK");
            dic.Add(1046, "PTB");
            dic.Add(2070, "PTG");
            dic.Add(1029, "CSY");
            dic.Add(3082, "ESN");
            dic.Add(1053, "SVE");
            dic.Add(1055, "TRK");
            return dic;
        }
        public static Dictionary<int, string> langDict
        {
            get
            {
                return FillLanguageDictionary();
            }
        }

        //v-danpgu: GetPID
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();


        public static string GetPID()
        {
            IntPtr hwnd = GetForegroundWindow();
            uint UIPID;
            GetWindowThreadProcessId(hwnd, out UIPID);
            Trace.WriteLine(string.Format("UIPID is :{0}", UIPID));

            var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            var subKey = hklm.OpenSubKey("Software\\Microsoft\\LaunchSCCMUI\\AdminConsole");

            if (subKey == null)
            {
                return ConsoleLanguage;
            }
            string[] registryPID = subKey.GetSubKeyNames();
            int uiPidInt = Convert.ToInt32(UIPID);
            foreach (var rp in registryPID)
            {
                int rpInt = Convert.ToInt32(rp);
                Trace.WriteLine(string.Format("PID in the SCCM registry is: {0}", rpInt));

                if (uiPidInt == rpInt)
                {
                    RegistryKey newKey = subKey.OpenSubKey(rp);
                    var lang = newKey.GetValue("LCID");
                    return langDict[Convert.ToInt32(lang)];
                }
            }
            return ConsoleLanguage;
        }


        public static string ConsoleLanguage
        {
            get
            {
                CultureInfo productCultureInfo = GetProductCultureInfo();
                if (productCultureInfo == null)
                {
                    Trace.WriteLine($"No expected language package installed. Set as {System.Threading.Thread.CurrentThread.CurrentCulture.Name}");
                    productCultureInfo = new CultureInfo(System.Threading.Thread.CurrentThread.CurrentCulture.Name);
                }
                //v-danpgu: change all ESP to ESN
                var lan = productCultureInfo.ThreeLetterWindowsLanguageName;
                if (lan == "ESP") lan = "ESN";
                return lan;
            }
        }
        private static CultureInfo GetProductCultureInfo()
        {
            CultureInfo currentCultureInfo = CultureInfo.CurrentUICulture;
            //CultureInfo currentCultureInfo = new CultureInfo(Registry.GetValue("HKEY_CURRENT_USER\\Control Panel\\Desktop", "PreferredUILanguages", CultureInfo.CurrentUICulture).ToString());
            //find by current OS culture info
            Trace.WriteLine(string.Format("Try to find by current OS cultureinfo: {0} ", currentCultureInfo.Name));
            CultureInfo ci = GetProductCultureInfo(currentCultureInfo);
            if (ci == null)
            {
                //neutral culture
                if (currentCultureInfo.IsNeutralCulture)
                {
                    currentCultureInfo = CultureInfo.CreateSpecificCulture(currentCultureInfo.Name);
                    Trace.WriteLine(string.Format("Try to find by current OS cultureinfo child: {0} ", currentCultureInfo.Name));
                }
                //specific culture
                else
                {
                    currentCultureInfo = currentCultureInfo.Parent;
                    Trace.WriteLine(string.Format("Try to find by current OS cultureinfo parent: {0} ", currentCultureInfo.Name));
                }
                //find by parent or child of current OS culture info 
                ci = GetProductCultureInfo(currentCultureInfo);
            }

            return ci;
        }
        private static CultureInfo GetProductCultureInfo(CultureInfo currentCultureInfo)
        {
            if (ConsoleDirectory == null)
            {
                return null;
            }
            List<string> installedLanguagePackageList = Directory.GetDirectories(ConsoleDirectory).ToList().Select(item => Path.GetFileName(item)).ToList();
            installedLanguagePackageList.Remove("resources");
            installedLanguagePackageList.Remove("i386");

            Trace.WriteLine(string.Format("currentCultureInfo.Name: {0}, currentCultureInfo.IsNeutralCulture: {1} ", currentCultureInfo.Name, currentCultureInfo.IsNeutralCulture));
            foreach (string languagePack in installedLanguagePackageList)
            {
                Trace.WriteLine(string.Format("languagePack: {0}", languagePack));
                try
                {
                    CultureInfo ci = new CultureInfo(languagePack);
                    if (currentCultureInfo.Equals(ci))
                    {
                        Trace.WriteLine(string.Format("Found by cultureinfo: {0}", currentCultureInfo.Name));
                        return currentCultureInfo;
                    }
                }
                catch
                {
                    Trace.WriteLine(string.Format("Exception when GetProductCultureInfo. languagePack: {0}", languagePack));
                }
            }

            Trace.WriteLine("Not found.");
            return null;
        }

        [Obsolete("Please use TryGetPageControlList to instead of it.", false)]
        public static bool TryGetPageControl(IUIAutomationElement uiObject, out IUIAutomationElement objPageControl)
        {
            string automationId;
            string frameworkId;
            objPageControl = null;

            try
            {
                Loop:
                // the root uiObject comes with a set of cached properties
                automationId = (string)uiObject.CurrentAutomationId;
                frameworkId = (string)uiObject.CurrentFrameworkId;

                if (string.Equals(automationId, "SmsWizardForm", StringComparison.OrdinalIgnoreCase))
                {
                    // find the page control in wizard
                    objPageControl = uiObject.GetElementByAutomationId(@"_pagePanel\_interiorPagePanel\WizardPage").FirstChild();
                }
                else if (string.Equals(automationId, "SheetFramework", StringComparison.OrdinalIgnoreCase))
                {
                    // find the page control in property sheet
                    objPageControl = uiObject.GetChild("tabPages");

                    IUIAutomationElementArray objArray = objPageControl.FindAllChildren();

                    for (int i = 0; i < objArray.Length; i++)
                    {
                        if (objArray.GetElement(i).FindAllChildren().Length > 0)
                        {
                            objPageControl = objArray.GetElement(i).FirstChild().FirstChild();
                            break;
                        }
                    }
                }
                else if (string.Equals(automationId, "SccmPageControlDialog", StringComparison.OrdinalIgnoreCase))
                {
                    objPageControl = uiObject.GetChild("panelContentArea").FirstChild();
                }
                else if (string.Equals(frameworkId, "WinForm", StringComparison.OrdinalIgnoreCase) == false &&
                         string.Equals(frameworkId, "WPF", StringComparison.OrdinalIgnoreCase) == false)
                {
                    // if the dialog is a Win32 dialog find the parent
                    IUIAutomationElement objParent = uiObject.GetParent();

                    // if Win32 dialog has no WF or WPF parent, #32769 -> root
                    if (objParent.CurrentClassName.Equals("#32769"))
                    {
                        objPageControl = uiObject;
                    }
                    else
                    {
                        uiObject = objParent;

                        // link it to a parent that is either WF or WPF
                        goto Loop;
                    }
                }
                else if (string.Equals(automationId, "SccmExceptionDialog", StringComparison.OrdinalIgnoreCase) == true)
                {
                    // if the dialog is a Win32 dialog find the parent
                    IUIAutomationElement objParent = uiObject.GetParent();

                    // if SccmExceptionDialog dialog has no WF or WPF parent, #32769 -> root
                    if (objParent.CurrentClassName.Equals("#32769"))
                    {
                        objPageControl = uiObject;
                    }
                    else
                    {
                        uiObject = objParent;

                        // link it to a parent that is either WF or WPF
                        goto Loop;
                    }
                }
                else
                {
                    // root dialog is the one that should be fingerprinted (already cached)
                    objPageControl = uiObject;
                }
                return true;
            }
            //catch (ArgumentOutOfRangeException)
            //{
            //    // ui not available
            //}
            catch (ArgumentException)
            {
                // process has exited
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Exception happens during get page control: {0}", ex));
            }

            return false;
        }
        public static bool TryGetPageControlList(IUIAutomationElement uiObject, out List<IUIAutomationElement> objPageControlList, out IUIAutomationElement objPageIdentifier)
        {
            string automationId;
            string frameworkId;
            IUIAutomationElement objPageControl = null;
            IUIAutomationElement objEmbeddedUserControl = null;
            objPageControlList = new List<IUIAutomationElement>();
            objPageIdentifier = null;

            try
            {
                // the root uiObject comes with a set of cached properties
                automationId = (string)uiObject.CurrentAutomationId;
                frameworkId = (string)uiObject.CurrentFrameworkId;
                Trace.WriteLine("automationId =" + automationId + "frameworkId =" + frameworkId);
                //retrieve assembly info for window
                objPageControl = uiObject;
                AddPageIdentifier(ref objPageControlList, ref objPageIdentifier, objPageControl);

                //retrieve assembly info for user control in window
                if (string.Equals(frameworkId, "WinForm", StringComparison.OrdinalIgnoreCase) == true ||
                    string.Equals(frameworkId, "WPF", StringComparison.OrdinalIgnoreCase) == true)
                {
                    switch (automationId.ToLower())
                    {
                        case "smswizardform":
                            // find the page control in wizard
                            if (uiObject.TryGetElementByAutomationId(@"_pagePanel\_interiorPagePanel\WizardPage", out objPageControl))
                            {
                                objPageControl = objPageControl.FirstChild();
                                AddPageIdentifier(ref objPageControlList, ref objPageIdentifier, objPageControl);
                                //find embedded user control in wizard page
                                switch (objPageControl.CurrentAutomationId.ToLower())
                                {
                                    case "modifyimporteddatapagecontrol":
                                        if (objPageControl.TryGetElementByAutomationId(@"panelWinMobile\wmContentLocationForSign", out objEmbeddedUserControl))
                                        {
                                            AddControlList(ref objPageControlList, objEmbeddedUserControl);
                                        }
                                        break;
                                    case "importinfo":
                                        // ImportFailed, ImportProgress
                                        objEmbeddedUserControl = objPageControl.GetElementByAutomationId("panelContainer").FirstChild();
                                        if (objEmbeddedUserControl.CurrentAutomationId.Equals("panelInfo", StringComparison.OrdinalIgnoreCase) == false)
                                        {
                                            AddControlList(ref objPageControlList, objEmbeddedUserControl);
                                        }
                                        break;
                                    case "subscriptiondeliverycontrol":
                                        // FileSubscriptionControl...
                                        IUIAutomationElementArray objChildren_Delivery = objPageControl.FindAllChildren();
                                        objEmbeddedUserControl = objChildren_Delivery.GetElement(objChildren_Delivery.Length - 1);
                                        if (objEmbeddedUserControl.CurrentAutomationId.Contains("Control"))
                                        {
                                            AddPageIdentifier(ref objPageControlList, ref objPageIdentifier, objEmbeddedUserControl);
                                        }
                                        break;
                                    case "subscriptionschedulecontrol":
                                        // WeeklyScheduleControl
                                        objPageControl = objPageControl.GetElementByAutomationId("panel1");
                                        IUIAutomationElementArray objChildren_Schedule = objPageControl.FindAllChildren();
                                        objEmbeddedUserControl = objChildren_Schedule.GetElement(objChildren_Schedule.Length - 1);
                                        if (objEmbeddedUserControl.CurrentAutomationId.Contains("Control"))
                                        {
                                            AddPageIdentifier(ref objPageControlList, ref objPageIdentifier, objEmbeddedUserControl);
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            break;
                        case "sheetframework":
                            // find the page control in property sheet
                            objPageControl = uiObject.GetChild("tabPages");
                            IUIAutomationElementArray objArray = objPageControl.FindAllChildren();
                            for (int i = 0; i < objArray.Length; i++)
                            {
                                if (objArray.GetElement(i).FindAllChildren().Length > 0)
                                {
                                    //property page
                                    objPageControl = objArray.GetElement(i).FirstChild().FirstChild();
                                    AddPageIdentifier(ref objPageControlList, ref objPageIdentifier, objPageControl);

                                    //sub-page in property dialog
                                    switch (objPageControl.CurrentAutomationId.ToLower())
                                    {
                                        //DCM CI setting
                                        case "cilocalsettingpagecontrol":
                                            objPageControl = objPageControl.GetElementByAutomationId(@"settingConcreteInfomationPane\panelControlsContainer").FirstChild();
                                            AddPageIdentifier(ref objPageControlList, ref objPageIdentifier, objPageControl);
                                            break;
                                        case "collectionalertpagecontrol":
                                            //AlertControl
                                            objPageControl = objPageControl.GetElementByAutomationId(@"groupBoxInformation\AlertControl");
                                            AddPageIdentifier(ref objPageControlList, ref objPageIdentifier, objPageControl);
                                            //ParameterControl
                                            objPageControl = objPageControl.GetElementByAutomationId(@"panel3").FirstChild();
                                            AddPageIdentifier(ref objPageControlList, ref objPageIdentifier, objPageControl);
                                            break;
                                        case "generalpagecontrol":
                                            // IPSubnetType
                                            if (objPageControl.TryGetElementByAutomationId(@"pnlType", out objEmbeddedUserControl))
                                            {
                                                objEmbeddedUserControl = objEmbeddedUserControl.FirstChild();
                                                AddPageIdentifier(ref objPageControlList, ref objPageIdentifier, objEmbeddedUserControl);
                                            }
                                            break;
                                        case "subscriptiondeliverycontrol":
                                            // FileSubscriptionControl...
                                            IUIAutomationElementArray objChildren_Delivery = objPageControl.FindAllChildren();
                                            objEmbeddedUserControl = objChildren_Delivery.GetElement(objChildren_Delivery.Length - 1);
                                            if (objEmbeddedUserControl.CurrentAutomationId.Contains("Control"))
                                            {
                                                AddPageIdentifier(ref objPageControlList, ref objPageIdentifier, objEmbeddedUserControl);
                                            }
                                            break;
                                        case "subscriptionschedulecontrol":
                                            // WeeklyScheduleControl
                                            objPageControl = objPageControl.GetElementByAutomationId("panel1");
                                            IUIAutomationElementArray objChildren_Schedule = objPageControl.FindAllChildren();
                                            objEmbeddedUserControl = objChildren_Schedule.GetElement(objChildren_Schedule.Length - 1);
                                            if (objEmbeddedUserControl.CurrentAutomationId.Contains("Control"))
                                            {
                                                AddPageIdentifier(ref objPageControlList, ref objPageIdentifier, objEmbeddedUserControl);
                                            }
                                            break;
                                    }
                                    break;
                                }
                            }
                            break;
                        case "sccmpagecontroldialog":
                            objPageControl = uiObject.GetChild("panelContentArea").FirstChild();
                            AddPageIdentifier(ref objPageControlList, ref objPageIdentifier, objPageControl);
                            //sub-page in property dialog
                            switch (objPageControl.CurrentAutomationId.ToLower())
                            {
                                // Detection rule page
                                case "clausedialog":
                                case "macclausedialog":
                                    objPageControl = objPageControl.FirstChild(2);
                                    AddPageIdentifier(ref objPageControlList, ref objPageIdentifier, objPageControl);
                                    break;
                                // create global condition dialog
                                case "createglobalcondition":
                                    IUIAutomationElement objSettingPage = objPageControl.GetChild("settingConcreteInfomationPane");
                                    if (objSettingPage != null)
                                    {
                                        objSettingPage = objSettingPage.GetChild("panelControlsContainer").FirstChild();
                                        AddPageIdentifier(ref objPageControlList, ref objPageIdentifier, objPageControl);
                                    }
                                    else
                                    {
                                        objPageControl = objPageControl.GetChild("expressionControlPane");
                                        AddPageIdentifier(ref objPageControlList, ref objPageIdentifier, objPageControl);
                                    }
                                    break;
                                // client setting dialog
                                case "homepagecontrol":
                                    objPageControl = objPageControl.GetChild("splitContainer").GetChild(1).FirstChild();
                                    AddPageIdentifier(ref objPageControlList, ref objPageIdentifier, objPageControl);
                                    break;
                            }
                            break;
                        //create requirement dialog
                        case "editclausedialog":
                            // ValueClauseControl, FilePermissionClauseControl
                            IUIAutomationCondition paneCondition = UIA3Automation.RawInstance.CreatePropertyCondition(UIA_PropertyIds.UIA_ControlTypePropertyId, UIA_ControlTypeIds.UIA_PaneControlTypeId);
                            IUIAutomationCondition notBottomLineCondition = UIA3Automation.RawInstance.CreateNotCondition(UIA3Automation.RawInstance.CreatePropertyCondition(UIA_PropertyIds.UIA_AutomationIdPropertyId, "groupBoxBottomLine"));
                            IUIAutomationCondition condition = UIA3Automation.RawInstance.CreateAndCondition(paneCondition, notBottomLineCondition);
                            IUIAutomationElementArray objPaneArr = uiObject.FindAll(interop.UIAutomationCore.TreeScope.TreeScope_Children, condition);
                            for (int i = 0; i < objPaneArr.Length; i++)
                            {
                                IUIAutomationElement objUserControl = objPaneArr.GetElement(i);
                                if (objUserControl.CurrentAutomationId.Contains("Control"))
                                {
                                    objPageControl = objUserControl;
                                    AddPageIdentifier(ref objPageControlList, ref objPageIdentifier, objPageControl);
                                    break;
                                }
                            }
                            break;
                        case "tasksequenceeditordialog":
                            if (objPageControl.GetElementByAutomationId(@"tableLayoutPanel\splitContainerGroups").GetChild(1).FindAllChildren().Length > 0)
                            {
                                //TSE right pane, header control will show only when a page loading completely, so take header control as page identifier
                                objPageControl = objPageControl.GetElementByAutomationId(@"tableLayoutPanel\splitContainerGroups").GetChild(1).FirstChild();
                                if (objPageControl.FirstChild().FindAllChildren().Length > 0)
                                {
                                    objPageControl = objPageControl.FirstChild().FirstChild();
                                    AddPageIdentifier(ref objPageControlList, ref objPageIdentifier, objPageControl);
                                }
                                else
                                {
                                    objPageControl = objPageControl.GetChild(1).FirstChild();
                                    AddPageIdentifier(ref objPageControlList, ref objPageIdentifier, objPageControl);
                                }
                            }
                            break;
                        // Add Exchange Server Connector and DCM Wizard
                        case "settinggroupdialog":
                            if (objPageControl.TryGetElementByAutomationId("splitContainer1", out objEmbeddedUserControl))
                            {
                                objPageControl = objPageControl.FirstChild(4);
                            }
                            else
                            {
                                objPageControl = objPageControl.FirstChild(1);
                            }
                            AddPageIdentifier(ref objPageControlList, ref objPageIdentifier, objPageControl);
                            break;
                        case "useraccount":
                            objPageControl = uiObject.FirstChild();
                            if (objPageControl.CurrentAutomationId.Equals("panelVerify", StringComparison.OrdinalIgnoreCase))
                            {
                                objPageControl = objPageControl.GetElementByAutomationId("panelDataSource").FirstChild();
                                AddPageIdentifier(ref objPageControlList, ref objPageIdentifier, objPageControl);
                            }
                            break;
                        case "scheduledialog":
                            // ServiceWindowTimeControl
                            if (uiObject.TryGetElementByAutomationId(@"groupBoxTime", out objEmbeddedUserControl))
                            {
                                objPageControl = objEmbeddedUserControl.FirstChild();
                                AddPageIdentifier(ref objPageControlList, ref objPageIdentifier, objPageControl);
                            }
                            // RecurrenceWeeklyControl
                            if (uiObject.TryGetElementByAutomationId(@"groupBoxRecurrenceSchedule\panelRecurrenceDetails", out objEmbeddedUserControl))
                            {
                                objPageControl = objEmbeddedUserControl.FirstChild();
                                AddPageIdentifier(ref objPageControlList, ref objPageIdentifier, objPageControl);
                            }
                            break;
                        case "createruledialog":
                            // RuleValueControl
                            IUIAutomationElementArray objChildren = uiObject.FindAllChildren();
                            objEmbeddedUserControl = objChildren.GetElement(objChildren.Length - 2);
                            if (objEmbeddedUserControl.CurrentControlType.Equals(UIA_ControlTypeIds.UIA_PaneControlTypeId))
                            {
                                AddPageIdentifier(ref objPageControlList, ref objPageIdentifier, objEmbeddedUserControl);
                            }
                            break;
                        case "browsefilesystemdialog":
                            // BrowseFileSystesmRuleControl
                            objPageControl = uiObject.GetElementByAutomationId("splitContainerBrowse").GetChild(1);
                            if (objPageControl.TryGetElementByAutomationId("fileSystemRuleControl", out objEmbeddedUserControl))
                            {
                                AddControlList(ref objPageControlList, objEmbeddedUserControl);
                            }
                            break;
                        case "browseregistrydialog":
                            // BrowseFileSystesmRuleControl
                            objPageControl = uiObject.GetElementByAutomationId("splitContainerBrowse").GetChild(1);
                            if (objPageControl.TryGetElementByAutomationId("browseRegistryRuleControl", out objEmbeddedUserControl))
                            {
                                AddControlList(ref objPageControlList, objEmbeddedUserControl);
                            }
                            break;
                        case "gatheruserinputforpromptvaluedialog":
                            // Queries PromptInput dialog
                            objEmbeddedUserControl = uiObject.GetElementByAutomationId("panelInputControlContainer").FirstChild();
                            AddPageIdentifier(ref objPageControlList, ref objPageIdentifier, objEmbeddedUserControl);
                            break;
                        case "queryruledialog":
                            // find the page control in property sheet
                            objPageControl = uiObject.GetChild("tabControl");
                            IUIAutomationElementArray objTabArray = objPageControl.FindAllChildren();
                            for (int i = 0; i < objTabArray.Length; i++)
                            {
                                if (objTabArray.GetElement(i).FindAllChildren().Length > 0)
                                {
                                    //property page
                                    objPageControl = objTabArray.GetElement(i).FirstChild();
                                    AddPageIdentifier(ref objPageControlList, ref objPageIdentifier, objPageControl);
                                    break;
                                }
                            }
                            break;
                    }
                }
                else
                {
                    return false;
                }

                return true;
            }
            catch (ArgumentException)
            {
                // process has exited
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Exception happens during get page controls: {0}", ex));
            }

            return false;
        }
        /// <summary>
        /// some containers' property (e.g. name) will be changed with their page control's changing, thus there is a situation exist:
        /// captured contianer info is for previous page, and captured page control is for the next page, then the whole captured info 
        /// is not in a same page, it's wrong.
        /// This method return a element object to represent the page control, we could detect if the page control valid or not to determin 
        /// whether we need to fail the capturing or not.
        /// 
        /// For CM console, the Wizard dialog meet this description.
        /// </summary>
        /// <param name="uiObject"></param>
        /// <param name="objPageControl"></param>
        /// <returns></returns>
        public static bool TryGetPageIdentifyControl(IUIAutomationElement uiObject, out IUIAutomationElement objPageControl)
        {
            string automationId;
            string frameworkId;
            //objPageControl = uiObject;
            objPageControl = null;
            IUIAutomationElement objEmbeddedUserControl = null;
            try
            {
                // the root uiObject comes with a set of cached properties
                automationId = (string)uiObject.CurrentAutomationId;
                frameworkId = (string)uiObject.CurrentFrameworkId;
                switch (automationId.ToLower())
                {
                    case "smswizardform":
                        // find the page control in wizard
                        IUIAutomationElement objResult = null;
                        if (uiObject.TryGetElementByAutomationId(@"_pagePanel\_interiorPagePanel\WizardPage", out objResult))
                        {
                            objPageControl = objResult.FirstChild();
                            //find embedded user control in wizard page
                            switch (objPageControl.CurrentAutomationId.ToLower())
                            {
                                case "subscriptiondeliverycontrol":
                                    // FileSubscriptionControl...
                                    IUIAutomationElementArray objChildren_Delivery = objPageControl.FindAllChildren();
                                    objEmbeddedUserControl = objChildren_Delivery.GetElement(objChildren_Delivery.Length - 1);
                                    if (objEmbeddedUserControl.CurrentAutomationId.Contains("Control"))
                                    {
                                        objPageControl = objEmbeddedUserControl;
                                    }
                                    break;
                                case "subscriptionschedulecontrol":
                                    // WeeklyScheduleControl...
                                    objPageControl = objPageControl.GetElementByAutomationId("panel1");
                                    IUIAutomationElementArray objChildren_Schedule = objPageControl.FindAllChildren();
                                    objEmbeddedUserControl = objChildren_Schedule.GetElement(objChildren_Schedule.Length - 1);
                                    if (objEmbeddedUserControl.CurrentAutomationId.Contains("Control"))
                                    {
                                        objPageControl = objEmbeddedUserControl;
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            // progress page
                            objPageControl = null;
                        }
                        break;
                    case "sheetframework":
                        // find the page control in property sheet
                        objPageControl = uiObject.GetChild("tabPages");

                        IUIAutomationElementArray objArray = objPageControl.FindAllChildren();

                        for (int i = 0; i < objArray.Length; i++)
                        {
                            if (objArray.GetElement(i).FindAllChildren().Length > 0)
                            {
                                //property page
                                objPageControl = objArray.GetElement(i).FirstChild().FirstChild();

                                //sub-page in property dialog
                                switch (objPageControl.CurrentAutomationId)
                                {
                                    //DCM CI setting
                                    case "CILocalSettingPageControl":
                                        objPageControl = objPageControl.GetElementByAutomationId(@"settingConcreteInfomationPane\panelControlsContainer").FirstChild();
                                        break;
                                    case "collectionalertpagecontrol":
                                        //AlertControl -> ParameterControl
                                        objPageControl = objPageControl.GetElementByAutomationId(@"groupBoxInformation\AlertControl\panel3").FirstChild();
                                        break;
                                    case "generalpagecontrol":
                                        // IPSubnetType
                                        if (objPageControl.TryGetElementByAutomationId(@"pnlType\IPSubnetType", out objEmbeddedUserControl))
                                        {
                                            objPageControl = objEmbeddedUserControl;
                                        }
                                        break;
                                    case "subscriptiondeliverycontrol":
                                        // FileSubscriptionControl...
                                        IUIAutomationElementArray objChildren_Delivery = objPageControl.FindAllChildren();
                                        objEmbeddedUserControl = objChildren_Delivery.GetElement(objChildren_Delivery.Length - 1);
                                        if (objEmbeddedUserControl.CurrentAutomationId.Contains("Control"))
                                        {
                                            objPageControl = objEmbeddedUserControl;
                                        }
                                        break;
                                    case "subscriptionschedulecontrol":
                                        // WeeklyScheduleControl...
                                        objPageControl = objPageControl.GetElementByAutomationId("panel1");
                                        IUIAutomationElementArray objChildren_Schedule = objPageControl.FindAllChildren();
                                        objEmbeddedUserControl = objChildren_Schedule.GetElement(objChildren_Schedule.Length - 1);
                                        if (objEmbeddedUserControl.CurrentAutomationId.Contains("Control"))
                                        {
                                            objPageControl = objEmbeddedUserControl;
                                        }
                                        break;
                                }
                                break;
                            }
                        }
                        break;
                    case "sccmpagecontroldialog":
                        objPageControl = uiObject.GetChild("panelContentArea").FirstChild();
                        //sub-page in property dialog
                        switch (objPageControl.CurrentAutomationId.ToLower())
                        {
                            //Detection rule page
                            case "clausedialog":
                            case "macclausedialog":
                                objPageControl = objPageControl.FirstChild(2);
                                break;
                            case "createglobalcondition":
                                IUIAutomationElement objSettingPage = objPageControl.GetChild("settingConcreteInfomationPane");
                                if (objSettingPage != null)
                                {
                                    objSettingPage = objSettingPage.GetChild("panelControlsContainer").FirstChild();
                                }
                                else
                                {
                                    objPageControl = objPageControl.GetChild("expressionControlPane");
                                }
                                break;
                            case "homepagecontrol":
                                    //My little friend and I were shocked that the screenshot of clientsetting failed 
                                    var tempPageControl = objPageControl.GetChild("splitContainer");
                                    objPageControl = tempPageControl.GetChild(1).FirstChild();
                                    if (objPageControl == null)
                                    {
                                        objPageControl = tempPageControl.GetChild(0).FirstChild();
                                    }
                                break;
                        }
                        break;
                    case "editclausedialog":
                        // ValueClauseControl, FilePermissionClauseControl
                        IUIAutomationCondition paneCondition = UIA3Automation.RawInstance.CreatePropertyCondition(UIA_PropertyIds.UIA_ControlTypePropertyId, UIA_ControlTypeIds.UIA_PaneControlTypeId);
                        IUIAutomationCondition notBottomLineCondition = UIA3Automation.RawInstance.CreateNotCondition(UIA3Automation.RawInstance.CreatePropertyCondition(UIA_PropertyIds.UIA_AutomationIdPropertyId, "groupBoxBottomLine"));
                        IUIAutomationCondition condition = UIA3Automation.RawInstance.CreateAndCondition(paneCondition, notBottomLineCondition);
                        IUIAutomationElementArray objPaneArr = uiObject.FindAll(interop.UIAutomationCore.TreeScope.TreeScope_Children, condition);
                        for (int i = 0; i < objPaneArr.Length; i++)
                        {
                            IUIAutomationElement objUserControl = objPaneArr.GetElement(i);
                            if (objUserControl.CurrentAutomationId.Contains("Control"))
                            {
                                objPageControl = objUserControl;
                                break;
                            }
                        }
                        break;
                    case "tasksequenceeditordialog":
                        try
                        {
                            uiObject.GetElementByAutomationId(@"tableLayoutPanel\splitContainerGroups");
                        }
                        catch
                        {
                            if (uiObject.GetElementByAutomationId(@"tableLayoutPanel1\tabControl").GetChild(0).FindAllChildren().Length > 0)
                            {
                                objPageControl = uiObject.GetElementByAutomationId(@"tableLayoutPanel1\tabControl").GetChild(0).FirstChild();
                                if (objPageControl.FirstChild().FindAllChildren().Length > 0)
                                {
                                    objPageControl = objPageControl.FirstChild().FirstChild();
                                }
                                else
                                {
                                    objPageControl = objPageControl.GetChild(1).FirstChild();
                                }
                            }
                            break;
                        }

                        if (uiObject.GetElementByAutomationId(@"tableLayoutPanel\splitContainerGroups").GetChild(1).FindAllChildren().Length > 0)
                        {
                            //TSE right pane, header control will show only when a page loading completely, so take header control as page identifier
                            objPageControl = uiObject.GetElementByAutomationId(@"tableLayoutPanel\splitContainerGroups").GetChild(1).FirstChild();
                            if (objPageControl.FirstChild().FindAllChildren().Length > 0)
                            {
                                objPageControl = objPageControl.FirstChild().FirstChild();
                            }
                            else
                            {
                                objPageControl = objPageControl.GetChild(1).FirstChild();
                            }
                        }
                        break;
                    case "useraccount":
                        objPageControl = uiObject.FirstChild();
                        if (objPageControl.CurrentAutomationId.Equals("panelVerify", StringComparison.OrdinalIgnoreCase))
                        {
                            objPageControl = objPageControl.GetElementByAutomationId("panelDataSource").FirstChild();
                        }
                        break;
                    case "scheduledialog":
                        // RecurrenceWeeklyControl
                        if (uiObject.TryGetElementByAutomationId(@"groupBoxRecurrenceSchedule\panelRecurrenceDetails", out objEmbeddedUserControl))
                        {
                            objPageControl = objEmbeddedUserControl.FirstChild();
                        }
                        else
                        {
                            // AI Synchronization Point Schedule dialog
                            return false;
                        }
                        break;
                    default:
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Exception happens during get page identify control: {0}", ex));
                throw ex;
            }

            //return false;
        }
        public static bool TryGetUITrackingRoute(IUIAutomationElement uiObject, out string launchedFrom, out string windowHierarchy)
        {
            launchedFrom = null;
            windowHierarchy = null;

            //read from cache
            try
            {
                if (cachedActiveWindow != null &&
                    uiObject.CurrentNativeWindowHandle.Equals(cachedActiveWindow.CurrentNativeWindowHandle))
                {
                    //Trace.WriteLine("Cached");
                    launchedFrom = cachedLaunchedFrom;
                    windowHierarchy = cachedWindowHierarchy;
                    return true;
                }
            }
            catch
            { }

            //read from runtime
            try
            {
                //retrieve launchedFrom
                IUIAutomationElement objCMConsole = GetCMConsoleUsingTrackingRoute(uiObject.CurrentProcessId);
                if (objCMConsole == null)
                {
                    return false;
                }

                IUIAutomationElement objBreadCrumbBar = objCMConsole.GetChild("BreadcrumbBar");
                if (objBreadCrumbBar == null) // When tool client and console are running under different user context, the automation id is empty for the breadcrumbar
                {
                    IUIAutomationElementArray consoleChildren = objCMConsole.FindAllChildren();
                    for (int i = 0; i < consoleChildren.Length; i++)
                    {
                        if (consoleChildren.GetElement(i).CurrentName == "BreadcrumbBar")
                        {
                            objBreadCrumbBar = consoleChildren.GetElement(i);
                            break;
                        }
                    }
                }

                if (objBreadCrumbBar == null)
                    return false;


                if (objBreadCrumbBar.FirstChild().CurrentControlType.Equals(UIA_ControlTypeIds.UIA_ComboBoxControlTypeId))
                {
                    launchedFrom = (string)objBreadCrumbBar.FirstChild().FirstChild().GetCurrentPattern(UIA_PatternIds.UIA_ValuePatternId);
                }
                else
                {
                    string crumbValue = string.Empty;

                    IUIAutomationElement objCrumbsPanel = objBreadCrumbBar.GetChild("CrumbsPanel");
                    if (objCrumbsPanel == null)// When tool client and console are running under different user context, the automation id is empty for the objCrumbsPanel
                    {
                        IUIAutomationElementArray breadChildren = objBreadCrumbBar.FindAllChildren();
                        if (breadChildren.Length == 5 && breadChildren.GetElement(2).CurrentName.Trim() == string.Empty)
                            objCrumbsPanel = breadChildren.GetElement(2);
                    }

                    if (objCrumbsPanel == null)
                        return false;

                    IUIAutomationElementArray children = objCrumbsPanel.FindAllChildren();
                    for (int i = 2; i < children.Length; i++)
                    {
                        string node = children.GetElement(i).CurrentName;
                        crumbValue = string.Format(@"{0}\{1}", crumbValue, node);
                    }

                    launchedFrom = crumbValue;
                    cachedLaunchedFrom = launchedFrom;
                }

                //retrieve windowHierarchy
                IUIAutomationElement objWnd = uiObject;
                string tmpWndHierarchy = string.Empty;
                List<string> wndHierarchyList = new List<string>();
                do
                {
                    wndHierarchyList.Add(objWnd.CurrentName);
                    objWnd = objWnd.GetParent();
                }
                //root ||
                while (!objWnd.CurrentClassName.Equals("#32769") &&
                       !objWnd.CurrentNativeWindowHandle.Equals(objCMConsole.CachedNativeWindowHandle));
                wndHierarchyList.Reverse();
                foreach (string wndName in wndHierarchyList)
                {
                    windowHierarchy = string.Format(@"{0}→{1}", windowHierarchy, wndName);
                }
                windowHierarchy = windowHierarchy.TrimStart('→');
                cachedWindowHierarchy = windowHierarchy;

                cachedActiveWindow = uiObject;
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Exception when get UI tracking route: {0}", ex));
                return false;
            }
        }
        public static IUIAutomationElement GetCMConsoleUsingTrackingRoute(int processId)
        {
            //from cache
            try
            {
                if (cachedActiveCMConsole != null && cachedActiveCMConsole.CachedProcessId == processId)
                {
                    if (cachedActiveCMConsole.FirstChild() != null)
                    {
                        return cachedActiveCMConsole;
                    }

                }
            }
            catch
            { }

            //from runtime
            try
            {
                //IUIAutomationCondition conditionClassName = UIA3.UIA3Automation.RawInstance.CreatePropertyConditionEx(UIA_PropertyIds.UIA_ClassNamePropertyId, "Window", PropertyConditionFlags.PropertyConditionFlags_IgnoreCase);
                //IUIAutomationCondition conditionFrameworkId = UIA3.UIA3Automation.RawInstance.CreatePropertyConditionEx(UIA_PropertyIds.UIA_FrameworkIdPropertyId, "WPF", PropertyConditionFlags.PropertyConditionFlags_IgnoreCase);
                //IUIAutomationCondition condition = UIA3.UIA3Automation.RawInstance.CreateAndCondition(conditionClassName, conditionFrameworkId);
                //IUIAutomationCacheRequest cacheRequest = UIA3.UIA3Automation.RawInstance.CreateCacheRequest();
                //cacheRequest.AutomationElementMode = AutomationElementMode.AutomationElementMode_Full;
                //cacheRequest.AddProperty(UIA_PropertyIds.UIA_ProcessIdPropertyId);
                //cacheRequest.AddProperty(UIA_PropertyIds.UIA_NativeWindowHandlePropertyId);
                //IUIAutomationElementArray children = UIA3.UIA3Automation.RawInstance.GetRootElement().FindAllBuildCache(TreeScope.TreeScope_Children, condition, cacheRequest);
                //for (int i = 0; i < children.Length; i++)
                //{
                //    IUIAutomationElement element = children.GetElement(i);
                //    if (element.CachedProcessId == processId)
                //    {
                //        cachedActiveCMConsole = element;
                //        return cachedActiveCMConsole;
                //    }
                //}

                MS.Internal.Mita.Modeling.Application consoleApp = new MS.Internal.Mita.Modeling.Application(processId);
                if (consoleApp != null)
                {
                    MS.Internal.Mita.Modeling.MainWindow consoleFX = consoleApp.MainWindow;
                    if (consoleFX != null)
                    {
                        IUIAutomationElement children = UIA3.UIA3Automation.RawInstance.GetRootElement();
                        IUIAutomationCacheRequest cacheRequest = UIA3.UIA3Automation.RawInstance.CreateCacheRequest();
                        cacheRequest.AutomationElementMode = interop.UIAutomationCore.AutomationElementMode.AutomationElementMode_Full;
                        cacheRequest.AddProperty(UIA_PropertyIds.UIA_ProcessIdPropertyId);
                        cacheRequest.AddProperty(UIA_PropertyIds.UIA_NativeWindowHandlePropertyId);
                        cacheRequest.TreeScope = interop.UIAutomationCore.TreeScope.TreeScope_Element;
                        IUIAutomationElement consoleElement = UIA3.UIA3Automation.RawInstance.ElementFromHandle(consoleFX.NativeWindowHandle);
                        cachedActiveCMConsole = consoleElement.BuildUpdatedCache(cacheRequest);
                        return cachedActiveCMConsole;
                    }
                }
            }
            catch
            { }

            return null;
        }


        public static IUIAutomationElement GetCMConsole(int processId)
        {
            //from cache
            try
            {
                if (cachedActiveCMConsole != null && cachedActiveCMConsole.CachedProcessId == processId)
                {
                    if (cachedActiveCMConsole.FirstChild() != null)
                    {
                        return cachedActiveCMConsole;
                    }

                }
            }
            catch
            { }

            //from runtime
            try
            {
                IUIAutomationCondition conditionClassName = UIA3.UIA3Automation.RawInstance.CreatePropertyConditionEx(UIA_PropertyIds.UIA_ClassNamePropertyId, "Window", PropertyConditionFlags.PropertyConditionFlags_IgnoreCase);
                IUIAutomationCondition conditionFrameworkId = UIA3.UIA3Automation.RawInstance.CreatePropertyConditionEx(UIA_PropertyIds.UIA_FrameworkIdPropertyId, "WPF", PropertyConditionFlags.PropertyConditionFlags_IgnoreCase);
                IUIAutomationCondition condition = UIA3.UIA3Automation.RawInstance.CreateAndCondition(conditionClassName, conditionFrameworkId);
                IUIAutomationCacheRequest cacheRequest = UIA3.UIA3Automation.RawInstance.CreateCacheRequest();
                cacheRequest.AutomationElementMode = interop.UIAutomationCore.AutomationElementMode.AutomationElementMode_Full;
                cacheRequest.AddProperty(UIA_PropertyIds.UIA_ProcessIdPropertyId);
                cacheRequest.AddProperty(UIA_PropertyIds.UIA_NativeWindowHandlePropertyId);
                IUIAutomationElementArray children = UIA3.UIA3Automation.RawInstance.GetRootElement().FindAllBuildCache(interop.UIAutomationCore.TreeScope.TreeScope_Children, condition, cacheRequest);
                for (int i = 0; i < children.Length; i++)
                {
                    IUIAutomationElement element = children.GetElement(i);
                    if (element.CachedProcessId == processId)
                    {
                        cachedActiveCMConsole = element;
                        return cachedActiveCMConsole;
                    }
                }

                //MS.Internal.Mita.Modeling.Application consoleApp = new MS.Internal.Mita.Modeling.Application(processId);
                //if (consoleApp != null)
                //{
                //    MS.Internal.Mita.Modeling.MainWindow consoleFX = consoleApp.MainWindow;
                //    if (consoleFX != null)
                //    {
                //        IUIAutomationElement children = UIA3.UIA3Automation.RawInstance.GetRootElement();
                //        IUIAutomationCacheRequest cacheRequest = UIA3.UIA3Automation.RawInstance.CreateCacheRequest();
                //        cacheRequest.AutomationElementMode = AutomationElementMode.AutomationElementMode_Full;
                //        cacheRequest.AddProperty(UIA_PropertyIds.UIA_ProcessIdPropertyId);
                //        cacheRequest.AddProperty(UIA_PropertyIds.UIA_NativeWindowHandlePropertyId);
                //        cacheRequest.TreeScope = TreeScope.TreeScope_Element;
                //        IUIAutomationElement consoleElement = UIA3.UIA3Automation.RawInstance.ElementFromHandle(consoleFX.NativeWindowHandle);
                //        cachedActiveCMConsole = consoleElement.BuildUpdatedCache(cacheRequest);
                //        return cachedActiveCMConsole;
                //    }
                //}
            }
            catch
            { }
            return null;
        }

        public static bool IsProductInstalled
        {
            get
            {
                try
                {
                    //just check if the console directory exist
                    if (Directory.Exists(ConsoleDirectory))
                    {
                        return true;
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);

                    return false;
                }
            }
        }
        public static bool IsProductProcess(IntPtr hWnd)
        {
            string processName = "Microsoft.ConfigurationManagement";
            string processNameSpace = "Microsoft.ConfigurationManager";

            int processId, threadId;
            threadId = NativeMethods.GetWindowThreadProcessId(hWnd, out processId);
            string currentProcessName = Process.GetProcessById(processId).ProcessName;
            if (currentProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase) ||
                currentProcessName.StartsWith(processNameSpace, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                //Trace.WriteLine("IsProductProcess =false;CurrentProcessName:" + currentProcessName);
                return false;
            }
        }
        public static bool IsProductProcess(IUIAutomationElement element)
        {
            if (IsProductProcess(element.CurrentNativeWindowHandle))
            {
                return true;
            }
            return false;
        }
        public static bool IsSulphurClientProcess(IUIAutomationElement element)
        {
            string sulphur = "MS.Internal.SulpHur.SulpHurClient";
            int processId, threadId;
            threadId = NativeMethods.GetWindowThreadProcessId(element.CurrentNativeWindowHandle, out processId);
            string currentProcessName = Process.GetProcessById(processId).ProcessName;
            if (sulphur.Equals(currentProcessName))
            {
                return true;
            }
            return false;
        }
        public static bool IsWindow(IUIAutomationElement element)
        {
            if (element.CurrentControlType.Equals(UIA_ControlTypeIds.UIA_WindowControlTypeId))
            {
                //Trace.WriteLine("IsWindow=true;CurrentControlType=" + element.CurrentControlType);
                return true;
            }
            else
            {
                //Trace.WriteLine("IsWindow=false;CurrentControlType=" + element.CurrentControlType);
                return false;
            }
        }
        public static bool IsCMConsole(IUIAutomationElement element)
        {
            IUIAutomationElement aeRibbon = null;
            if ((element.CurrentBoundingRectangle.left <= 0 && element.CurrentBoundingRectangle.right <= 0) ||
                 (element.CurrentClassName.Contains("HwndWrapper[Microsoft.ConfigurationManagement.exe")) ||
                 (element.TryGetElementByClassName(@"ConsoleRibbon", out aeRibbon)))
            {
                //Trace.WriteLine("IsCMConsole=true;CurrentClassName=" + element.CurrentClassName);
                return true;
            }
            else
            {
                //Trace.WriteLine("IsCMConsole=false;CurrentClassName=" + element.CurrentClassName);
                return false;
            }
        }
        public static bool IsSystemDialog(IUIAutomationElement uiObject)
        {
            Trace.WriteLine($"uiObject.CurrentName:{uiObject.CurrentName}");
            if (
                uiObject.CurrentClassName.Equals("#32770") && !uiObject.CurrentName.Equals("Configuration Manager") && !uiObject.CurrentName.Contains(" AAD ")
                )
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        public static bool IsValidWindow(IUIAutomationElement element)
        {
            bool IsProductProcess = Utility.IsProductProcess(element);
            bool IsCMConsole = Utility.IsCMConsole(element);
            bool IsSystemDialog = Utility.IsSystemDialog(element);
            bool IsWindow = Utility.IsWindow(element);
            bool result = !IsProductProcess || IsCMConsole || IsSystemDialog || !IsWindow;
            Trace.WriteLine($"IsValidWindow:{!result}");
            Trace.WriteLine($"IsProductProcess:{IsProductProcess}");
            Trace.WriteLine($"IsCMConsole:{IsCMConsole}");
            Trace.WriteLine($"IsSystemDialog:{IsSystemDialog}");
            Trace.WriteLine($"IsWindow:{IsWindow}");
            return !result;
        }
        public static bool ValidateWindowOpenedEvent(IUIAutomationElement uiObject)
        {
            //validate process
            if (!IsProductProcess(uiObject.CurrentNativeWindowHandle))
                return false;
            //miss CM console
            IUIAutomationElement objCMConsole = GetCMConsole(uiObject.CurrentProcessId);
            if (objCMConsole.CachedNativeWindowHandle.Equals(uiObject.CurrentNativeWindowHandle) ||
                (uiObject.CurrentBoundingRectangle.left <= 0 && uiObject.CurrentBoundingRectangle.right <= 0))
            {
                return false;
            }
            //miss sub-window (modal dialog) but sub-window of console
            IUIAutomationElement objParent = uiObject.GetParent();
            if (!uiObject.GetParent().CurrentClassName.Equals("#32769") &&
                !objParent.CurrentNativeWindowHandle.Equals(objCMConsole.CachedNativeWindowHandle))
            {
                return false;
            }
            return true;
        }
        public static bool ValidateStructureChangedEvent(IUIAutomationElement uiObject)
        {
            //define invalid structure changed event source
            if ((uiObject.CurrentControlType.Equals(UIA_ControlTypeIds.UIA_ListControlTypeId) ||
                uiObject.CurrentControlType.Equals(UIA_ControlTypeIds.UIA_ListItemControlTypeId) ||
                uiObject.CurrentControlType.Equals(UIA_ControlTypeIds.UIA_MenuControlTypeId) ||
                uiObject.CurrentControlType.Equals(UIA_ControlTypeIds.UIA_MenuBarControlTypeId) ||
                uiObject.CurrentControlType.Equals(UIA_ControlTypeIds.UIA_MenuItemControlTypeId) ||
                uiObject.CurrentControlType.Equals(UIA_ControlTypeIds.UIA_TreeControlTypeId) ||
                uiObject.CurrentControlType.Equals(UIA_ControlTypeIds.UIA_TreeItemControlTypeId) ||
                uiObject.CurrentControlType.Equals(UIA_ControlTypeIds.UIA_DataGridControlTypeId) ||
                uiObject.CurrentControlType.Equals(UIA_ControlTypeIds.UIA_DataItemControlTypeId) ||
                //当气泡控件出现时，可以自动截取包含该气泡的图
                //uiObject.CurrentControlType.Equals(UIA_ControlTypeIds.UIA_ToolTipControlTypeId)) ||
                //error icon
                (uiObject.CurrentControlType.Equals(UIA_ControlTypeIds.UIA_PaneControlTypeId) &&
                 (uiObject.CurrentBoundingRectangle.right - uiObject.CurrentBoundingRectangle.left).Equals(16) &&
                 (uiObject.CurrentBoundingRectangle.bottom - uiObject.CurrentBoundingRectangle.top).Equals(16))))
                return false;

            return true;
        }
        private static void AddPageIdentifier(ref List<IUIAutomationElement> objPageControlList, ref IUIAutomationElement objPageIdentifier, IUIAutomationElement objPageControl)
        {
            objPageControlList.Add(objPageControl);
            objPageIdentifier = objPageControl;
        }
        private static void AddControlList(ref List<IUIAutomationElement> objPageControlList, IUIAutomationElement objPageControl)
        {
            objPageControlList.Add(objPageControl);
        }

        public static ElementInformation ReadTree(IntPtr ptr, ElementChangeMonitor elementChangeMonitor, bool checkLoadingStatus = false)
        {
            if (elementChangeMonitor.IsChanged || GlobalChangeMonitor.Instance.IsChanged) throw new SulpHurClientUIChangedException();
            IUIAutomationElement root = UIA3Automation.RawInstance.ElementFromHandle(ptr);
            tagRECT rootRect = root.CurrentBoundingRectangle;
            string firstChildName = UIName(root);

            ElementInformation result = ReadTree(root, elementChangeMonitor, checkLoadingStatus);

            IUIAutomationElement rootNew = UIA3Automation.RawInstance.ElementFromHandle(ptr);
            tagRECT rectNew = rootNew.CurrentBoundingRectangle;
            string firstChildNameNew = UIName(rootNew);

            if (rectNew.left == rootRect.left && rectNew.top == rootRect.top && firstChildName.Equals(firstChildNameNew))
            {
                return result;
            }
            else
            {
                throw new SulpHurClientUIChangedException();
            }

        }

        public static string UIName(IUIAutomationElement root)
        {
            string result = string.Empty;
            switch (root.CurrentName)
            {
                case "SmsWizardForm":
                    result = root.GetElementByAutomationId("_headerBannerPanel").CurrentName;
                    break;
                case "SheetFramework":
                    result = root.GetElementByAutomationId("tabPages").FirstChild().CurrentName;
                    break;
                default:
                    break;
            }
            return result;
        }

        public static ElementInformation ReadTree(IUIAutomationElement root, ElementChangeMonitor elementChangeMonitor, bool checkLoadingStatus)
        {
            //if (elementChangeMonitor.IsChanged || GlobalChangeMonitor.Instance.IsChanged) throw new SulpHurClientUIChangedException();
            ElementInformation eiRoot = new ElementInformation(root);
            tagRECT rect = root.CurrentBoundingRectangle;
            if (root.CurrentControlType == UIA_ControlTypeIds.UIA_EditControlTypeId)
            {
                //AutomationElement aRoot = AutomationElement.FromHandle(root.CurrentNativeWindowHandle);
                //aRoot.FindAll(System.Windows.Automation.TreeScope.Subtree, new PropertyCondition(AutomationProperty.LookupById(0), aRoot.Current.AutomationId));
                AutomationElement ele = AutomationElement.FromPoint(new System.Windows.Point(rect.left + 1, rect.top + 1));
                eiRoot.Name = ele.GetText();
            }
            //string sourceText = getStringOnly(eiRoot);
            //eiRoot.Name = sourceText;
            eiRoot.Children = new List<ElementInformation>();
            if (IsContainer(eiRoot.ControlType))
            {
                //if (eiRoot.ControlType == MS.Internal.SulpHur.UICompliance.ControlType.MenuItem
                //    || eiRoot.ControlType == MS.Internal.SulpHur.UICompliance.ControlType.List
                //    )
                //{
                //    uia3ObjectArray = root.FindAllChildren();
                //}
                //else
                //{
                //    IUIAutomationCondition onScreenCondition = UIA3Automation.RawInstance.CreatePropertyCondition(UIA_PropertyIds.UIA_IsOffscreenPropertyId, false);
                //    uia3ObjectArray = root.FindAll(TreeScope.TreeScope_Children, onScreenCondition);
                //}

                //fix bug:5995368
                //compare the two function :
                //FindAllChildren() and FindAll(TreeScope.TreeScope_Children, onScreenCondition) 
                //the pre function can catch the controls property isofscreen = true;
                IUIAutomationElementArray uia3ObjectArray = root.FindAllChildren();
                for (int i = 0; i < uia3ObjectArray.Length; i++)
                {
                    IUIAutomationElement uia3Ae = uia3ObjectArray.GetElement(i);
                    //pre-process for element info
                    //miss SCCM dynamically generated WizardPage which will be able to be seen by UIA but won't be shown on UI
                    //if (uia3Ae.CurrentAutomationId.Equals("WizardPage") && string.IsNullOrEmpty(uia3Ae.CurrentName))
                    //    continue;

                    //miss TitleBar
                    if (uia3Ae.CurrentControlType == UIA_ControlTypeIds.UIA_TitleBarControlTypeId)
                        continue;

                    //retrieve element information
                    ElementInformation eiChild = ReadTree(uia3Ae, elementChangeMonitor, checkLoadingStatus);

                    //post-process for element info
                    //if (checkLoadingStatus && eiChild.BoundingRectangle.Left < rootRect.left)
                    //{
                    //    //throw new SulpHurClientUINotCompelteLoading();
                    //}

                    //add child
                    eiRoot.Children.Add(eiChild);
                }
            }
            return eiRoot;
        }

        //with the help of the UITextTranslation tool logic, However, it doesn't make any difference
        private static string getStringOnly(ElementInformation eiRoot)
        {
            string sourceText = string.Empty;
            object obj;
            AutomationElement ae = AutomationElement.FromPoint(new System.Windows.Point(eiRoot.X + eiRoot.BoundingRectangle.Width / 2, eiRoot.Y + eiRoot.BoundingRectangle.Height / 2));
            AutomationElement.AutomationElementInformation current = ae.Current;
            if (ae.TryGetCurrentPattern(ValuePattern.Pattern, out obj))
            {
                ValuePattern.ValuePatternInformation current1 = ((ValuePattern)ae.GetCurrentPattern(ValuePattern.Pattern)).Current;
                sourceText = current1.Value;
            }
            else
            {
                if (ae.TryGetCurrentPattern(RangeValuePattern.Pattern, out obj))
                {
                    RangeValuePattern.RangeValuePatternInformation current2 = ((RangeValuePattern)ae.GetCurrentPattern(RangeValuePattern.Pattern)).Current;
                    sourceText = current2.Value.ToString();
                }
                else
                {
                    StringBuilder stringBuilder = new StringBuilder(8000);
                    sourceText = stringBuilder.ToString();

                    if (string.IsNullOrEmpty(sourceText))
                    {
                        if (current.ControlType.Equals(System.Windows.Automation.ControlType.Text))
                        {
                            sourceText = current.Name;
                            if (sourceText.Contains('_'))
                            {
                                sourceText = sourceText.Replace('_', '&');
                            }
                        }

                        if (string.IsNullOrEmpty(sourceText))
                        {
                            sourceText = current.GetType().GetProperty("Name").GetValue(current, null).ToString();
                        }
                    }

                }
            }

            return sourceText;
        }

        public static ElementInformation ReadTree(IUIAutomationElement root)
        {
            ElementInformation eiRoot = null;
            eiRoot = new ElementInformation(root);
            eiRoot.Children = new List<ElementInformation>();

            if (IsContainer(eiRoot.ControlType))
            {
                IUIAutomationCondition onScreenCondition = UIA3Automation.RawInstance.CreatePropertyCondition(UIA_PropertyIds.UIA_IsOffscreenPropertyId, false);
                IUIAutomationElementArray uia3ObjectArray = root.FindAll(interop.UIAutomationCore.TreeScope.TreeScope_Children, onScreenCondition);
                for (int i = 0; i < uia3ObjectArray.Length; i++)
                {
                    IUIAutomationElement uia3Ae = uia3ObjectArray.GetElement(i);
                    //pre-process for element info
                    if (IsExpectedElement(uia3Ae) == false)
                    {
                        continue;
                    }

                    //retrieve element information
                    ElementInformation eiChild = ReadTree(uia3Ae);

                    //add child
                    eiRoot.Children.Add(eiChild);
                }
            }
            return eiRoot;
        }
        public static bool IsExpectedElement(IUIAutomationElement uia3Ae)
        {
            //miss SCCM dynamically generated WizardPage which will be able to be seen by UIA but won't be shown on UI
            if (//(uia3Ae.CurrentAutomationId.Equals("WizardPage") && string.IsNullOrEmpty(uia3Ae.CurrentName)) ||
                //miss TitleBar
                uia3Ae.CurrentControlType == UIA_ControlTypeIds.UIA_TitleBarControlTypeId ||
                //miss invalid element
                uia3Ae.CurrentNativeWindowHandle.Equals(0))
            {
                return false;
            }

            return true;
        }
        static public bool IsManagedControl(IUIAutomationElement window)
        {
            if (window.CurrentFrameworkId.IndexOf("wpf", StringComparison.InvariantCultureIgnoreCase) != -1
                || window.CurrentFrameworkId.IndexOf("winform", StringComparison.InvariantCultureIgnoreCase) != -1)
                return true;
            return false;
        }
        static private bool IsContainer(MS.Internal.SulpHur.UICompliance.ControlType type)
        {
            //if (App.UIAutoFlag && App.UIAutoMainInterfaceCaptureFlag)
            //{
            //    return true;
            //}
            return true;
            //switch (type)
            //{
            //    case MS.Internal.SulpHur.UICompliance.ControlType.Edit:
            //    case MS.Internal.SulpHur.UICompliance.ControlType.ToolTip:
            //    case MS.Internal.SulpHur.UICompliance.ControlType.Group:
            //    case MS.Internal.SulpHur.UICompliance.ControlType.MenuBar:
            //    case MS.Internal.SulpHur.UICompliance.ControlType.Pane:
            //    case MS.Internal.SulpHur.UICompliance.ControlType.Tab:
            //    case MS.Internal.SulpHur.UICompliance.ControlType.TabItem:
            //    case MS.Internal.SulpHur.UICompliance.ControlType.ToolBar:
            //    case MS.Internal.SulpHur.UICompliance.ControlType.Window:
            //    case MS.Internal.SulpHur.UICompliance.ControlType.Custom:
            //    case MS.Internal.SulpHur.UICompliance.ControlType.ComboBox:
            //    case MS.Internal.SulpHur.UICompliance.ControlType.NumericUpDown:
            //    // for left pane of browse query dialog which list sub-folder by treeitem
            //    case MS.Internal.SulpHur.UICompliance.ControlType.Tree:
            //    // for app dependency relationship dialog
            //    case MS.Internal.SulpHur.UICompliance.ControlType.Button:
            //    // for hyperlink (hyperlink is child of text element)
            //    case MS.Internal.SulpHur.UICompliance.ControlType.Text:
            //    //this three item make the xml file contains combox and menuitem
            //    case MS.Internal.SulpHur.UICompliance.ControlType.MenuItem:
            //    case MS.Internal.SulpHur.UICompliance.ControlType.Menu:
            //    case MS.Internal.SulpHur.UICompliance.ControlType.List:
            //    case MS.Internal.SulpHur.UICompliance.ControlType.ListItem:
            //    case MS.Internal.SulpHur.UICompliance.ControlType.Table:
            //    case MS.Internal.SulpHur.UICompliance.ControlType.Document:
            //    case MS.Internal.SulpHur.UICompliance.ControlType.Hyperlink:
            //    case MS.Internal.SulpHur.UICompliance.ControlType.TreeItem:
            //    case MS.Internal.SulpHur.UICompliance.ControlType.Header:
            //    case MS.Internal.SulpHur.UICompliance.ControlType.HeaderItem:
            //        return true;
            //    default:
            //        return false;
            //}
        }

        public static Dictionary<string, string> LanDic = new Dictionary<string, string>();
        public static void OSLan()
        {
            LanDic.Add("en-US", "ENU");
            LanDic.Add("zh-CN", "CHS");
            LanDic.Add("de-DE", "DEU");
            LanDic.Add("fr-FR", "FRA");
            LanDic.Add("ja-JP", "JPN");
            LanDic.Add("ru-RU", "RUS");
            LanDic.Add("Zh-TW", "CHT");
            LanDic.Add("Zh-HK", "ZHH");
            LanDic.Add("it-IT", "ITA");
            LanDic.Add("nl-NL", "NLD");
            LanDic.Add("ko-KR", "KOR");
            LanDic.Add("cs-CZ", "CSY");
            LanDic.Add("pl-PL", "PLK");
            LanDic.Add("tr-TR", "TRK");
            LanDic.Add("sv-SE", "SVE");
            LanDic.Add("hu-HU", "HUN");
            LanDic.Add("pt-PT", "PTG");
            LanDic.Add("es-ES", "ESN");
            LanDic.Add("pt-BR", "PTB");
            LanDic.Add("Fi-FI", "FIN");
            LanDic.Add("El-GR", "ELL");
            LanDic.Add("Da-DK", "DAN");
            LanDic.Add("No-NO", "NOR");
            LanDic.Add("Ro-RO", "ROM");
        }
        public static string FindByReg()
        {
            string result = "0.00.0000.0000";

            string adminConsoleVersionRegPath = "SOFTWARE";
            string _64NodePath = @"\WOW6432Node";
            string adminConsoleVersionTailPath = @"\Microsoft\ConfigMgr10\Setup";
            if (Environment.Is64BitOperatingSystem)
                adminConsoleVersionRegPath += _64NodePath;
            adminConsoleVersionRegPath += adminConsoleVersionTailPath;

            using (RegistryKey Key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(adminConsoleVersionRegPath))
            {
                if (Key != null)
                {
                    //string temp = (string)Key.GetValue("ProductVersion");
                    string temp = (string)Key.GetValue("AdminConsoleVersion");
                    if (string.IsNullOrEmpty(temp))
                        return result;
                    return temp;
                }
            }
            return result;
        }
    }
}
