using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Net;
using System.Management;
using System.Drawing;
using System.Globalization;

namespace MS.Internal.SulpHur.Utilities
{
    public struct OSType
    {
        public const string Unknown = "Unknown";
        public const string Win95 = "Win95";
        public const string Win98 = "Win98";
        public const string WinMe = "WinMe";
        public const string Win2k = "Win2k";
        public const string WinXP = "WinXP";
        public const string Win2k3 = "Win2k3";
        public const string WinVista = "WinVista";
        public const string Win2k8 = "Win2k8";
        public const string Win7 = "Win7";
        public const string Win2k8R2 = "Win2k8R2";
        public const string Win8 = "Win8";
        public const string Win2012Server = "Win2012Server";
        public const string Win2012R2Server = "Win2012R2Server";
        public const string Win81 = "Win8.1";
        public const string Win10 = "Win10";
        public const string Win10Server = "Win10Server";
    }

    public class CommonUtility
    {
        //GetOSType
        public static string GetOSType()
        {
            OperatingSystem os = System.Environment.OSVersion;
            string osType = OSType.Unknown;
            switch (os.Platform)
            {
                case PlatformID.Win32Windows:
                    switch (os.Version.Minor)
                    {
                        case 0: osType = OSType.Win95; break;
                        case 10: osType = OSType.Win98; break;
                        case 90: osType = OSType.WinMe; break;
                    }
                    break;
                case PlatformID.Win32NT:
                    switch (os.Version.Major)
                    {
                        //case 3: osName = "Windws NT 3.51"; break;
                        //case 4: osName = "Windows NT 4"; break;
                        case 5:
                            switch (os.Version.Minor)
                            {
                                case 0:
                                    osType = OSType.Win2k;
                                    break;
                                case 1:
                                    osType = OSType.WinXP;
                                    break;
                                case 2:
                                    osType = OSType.Win2k3;
                                    break;
                            }
                            break;
                        case 6:
                            string installationType = "";
                            string productName = string.Empty;
                            switch (os.Version.Minor)
                            {
                                case 0:
                                    installationType = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows NT\CurrentVersion", "InstallationType", "");
                                    if (installationType.Equals("Client"))
                                        osType = OSType.WinVista;
                                    else
                                        osType = OSType.Win2k8;
                                    break;
                                case 1:
                                    installationType = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows NT\CurrentVersion", "InstallationType", "");
                                    if (installationType.Equals("Client"))
                                        osType = OSType.Win7;
                                    else
                                        osType = OSType.Win2k8R2;
                                    break;
                                default:
                                    installationType = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows NT\CurrentVersion", "InstallationType", "");
                                    productName = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows NT\CurrentVersion", "ProductName", "");
                                    if (installationType.Equals("Client"))
                                    {
                                        if (productName.Contains("Windows 10 "))
                                            osType = OSType.Win10;
                                        else if (productName.Contains("Windows 8.1 "))
                                            osType = OSType.Win81;
                                        else if (productName.Contains("Windows 8 "))
                                            osType = OSType.Win8;
                                    }
                                    else
                                    {
                                        if (productName.Contains("Windows Server 2012 R2 "))
                                            osType = OSType.Win2012R2Server;
                                        else if (productName.Contains("Windows Server 2012 "))
                                            osType = OSType.Win2012Server;
                                        else
                                            osType = OSType.Win10Server;
                                    }
                                    break;
                            }
                            break;
                    }
                    break;
            }

            return osType;
        }
        //GetOSLanguage
        public static string GetOSLanguage()
        {
            return CultureInfo.CurrentUICulture.EnglishName;
        }
        //GetLocalIP
        public static IPAddress GetLocalIP()
        {
            string strHostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
            return ipEntry.AddressList[0];
        }
        //GetLocalMac
        public static string GetLocalMac()
        {
            string mac = null;
            ManagementObjectSearcher query = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection queryCollection = query.Get();
            foreach (ManagementObject mo in queryCollection)
            {
                if (mo["IPEnabled"].ToString() == "True")
                    mac = mo["MacAddress"].ToString();
            }
            return (mac);
        }


        public static Rectangle ToWinRectangle(System.Windows.Rect boundingRectangle)
        {
            Rectangle rect = new Rectangle
            {
                X = (int)boundingRectangle.X,
                Y = (int)boundingRectangle.Y,
                Width = (int)boundingRectangle.Width,
                Height = (int)boundingRectangle.Height,
            };

            return rect;
        }

        public static System.Windows.Rect ToUiaRect(Rectangle boundingRectangle)
        {
            System.Windows.Rect rect = new System.Windows.Rect
            {
                X = (int)boundingRectangle.X,
                Y = (int)boundingRectangle.Y,
                Width = (int)boundingRectangle.Width,
                Height = (int)boundingRectangle.Height,
            };

            return rect;
        }
    }
}
