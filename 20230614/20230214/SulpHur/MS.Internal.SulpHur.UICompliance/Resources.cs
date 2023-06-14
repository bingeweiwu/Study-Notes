using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using MS.Internal.Mita.Localization.Providers;
using MS.Internal.Mita.Localization;
using System.Reflection;
using Microsoft.Win32;

namespace MS.Internal.SulpHur.UICompliance
{
    public class Resources
    {
        public static IStringResourceData OK
        {
            get
            {
                return ResourceExtractor.FromResourceId(String.Format(CultureInfo.InvariantCulture,
                    ";OK;ManagedString;{0}\\AdminUI.Controls.dll;Microsoft.ConfigurationManagement.AdminConsole.Common.Properties.Resources;TextButtonOK", ConsoleBinPath));
            }
        }

        public static IStringResourceData Cancel
        {
            get
            {
                return ResourceExtractor.FromResourceId(String.Format(CultureInfo.InvariantCulture,
                    ";Cancel;ManagedString;{0}\\AdminUI.Controls.dll;Microsoft.ConfigurationManagement.AdminConsole.Common.Properties.Resources;TextButtonCancel", ConsoleBinPath));
            }
        }

        public static IStringResourceData Help
        {
            get
            {
                return ResourceExtractor.FromResourceId(String.Format(CultureInfo.InvariantCulture,
                    ";Help;ManagedString;{0}\\AdminUI.Controls.dll;Microsoft.ConfigurationManagement.AdminConsole.Common.Properties.Resources;HelpTopicsAction", ConsoleBinPath));
            }
        }

        static string consolePath = null;
        public static string ConsoleBinPath {
            get {

                if (!string.IsNullOrEmpty(consolePath)) {
                    return consolePath;
                }

                consolePath = GetAdminUIPath();
                if (string.IsNullOrEmpty(consolePath))
                    return null;

                return System.IO.Path.Combine(consolePath, "bin");
            }
        }

        public static string GetAdminUIPath()
        {
            ProcessorArchitecture CPUArch = typeof(object).Assembly.GetName().ProcessorArchitecture;
            // Get registry by different CPU Architecture
            RegistryKey uiDirectory = Registry.LocalMachine;
            string result = string.Empty;
            if (CPUArch == ProcessorArchitecture.X86)
            {
                uiDirectory = uiDirectory.OpenSubKey(@"SOFTWARE\Microsoft\ConfigMgr\Setup");
            }
            else if (CPUArch == ProcessorArchitecture.Amd64)
            {
                uiDirectory = uiDirectory.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\ConfigMgr\Setup");
            }
            else if (CPUArch == ProcessorArchitecture.IA64)
            {
                uiDirectory = uiDirectory.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\ConfigMgr\Setup");
            }
            else
            {
                uiDirectory = uiDirectory.OpenSubKey(@"SOFTWARE\Microsoft\ConfigMgr\Setup");
            }
            if (uiDirectory == null)
                return result;
            try
            {
                return (string)uiDirectory.GetValue("UI Installation Directory");
            }
            catch
            {
                return result;
            }
        }
    }

    public class ResourceExtractor
    {
        public static IStringResourceData FromResourceId(string resourceId)
        {
            IStringResourceData res = new StringResourceData(resourceId);
            try
            {
                res = new ResourceExtractionProvider().LoadExplicit(resourceId);
            }
            catch
            {
                string[] tmp = resourceId.Split(new char[] { ';' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (tmp.Length > 0)
                {
                    res = new StringResourceData(tmp[0]);
                }
            }

            return res;
        }
    }
}
