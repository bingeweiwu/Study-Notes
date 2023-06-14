using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Deployment.Application;
using System.IO;
using System.Security.Principal;
using System.Diagnostics;
using Microsoft.Win32;
using System.Web;

using System.Security;

namespace MS.Internal.SulpHur.SulpHurClientLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {              

                string serverName = string.Empty;
                string isLocalRun = "True";
                NameValueCollection nameValueTable = null;
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    InstallX86();
                    //name back config files
                    RenameConfigFiles();

                    //query string
                    serverName = ApplicationDeployment.CurrentDeployment.ActivationUri.Host;
                    string queryString = ApplicationDeployment.CurrentDeployment.ActivationUri.Query;
                    nameValueTable = HttpUtility.ParseQueryString(queryString);
                    isLocalRun = "False";

                    //identify credential
                    if (!nameValueTable.AllKeys.Contains("UserName") || string.IsNullOrEmpty(nameValueTable["UserName"]))
                    {
                        System.Windows.Forms.MessageBox.Show("Fail to log on!\nPlease try to open SulpHurClient in new IE tab.", "SulpHurClient", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        return;
                    }
                }

                string filePath = Path.Combine(new FileInfo(typeof(Program).Assembly.Location).DirectoryName, "MS.Internal.SulpHur.SulpHurClient.exe");

                //ServerName
                StringBuilder arguments = new StringBuilder();
                if(!string.IsNullOrEmpty(serverName))
                    arguments.Append(string.Format("ServerName={0}", serverName));

                //other arguments from querystring
                if (nameValueTable != null && nameValueTable.Count > 0)
                {
                    foreach (string item in nameValueTable.Keys)
                    {
                        arguments.Append(string.Format(@" {0}={1}", item, nameValueTable[item]));
                    }
                }

                arguments.Append(string.Format(" IsLocalRun={0}", isLocalRun));

                if (args.Length == 0)
                    StartProcess(filePath, arguments.ToString(), false);

                //provide run SulpHurClient under user Context
                else if (args.Length == 2)
                {
                    string userName = args[0].Trim();
                    string password = args[1].Trim();
                    StartProcess(filePath, arguments.ToString(), false, userName, password);
                        
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
         //       Console.ReadLine();
            }

           
        }


        private static void StartProcess(string name, string args, bool waitForExit, string userName, string password)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            string partDomain = string.Empty;
            string partUser = string.Empty;
            SecureString secPswd = new SecureString();
            foreach (char ch in password)
                secPswd.AppendChar(ch);
            
            if (userName.Contains(@"\"))
            {
                partDomain = userName.Substring(0, userName.IndexOf(@"\"));
                partUser = userName.Substring(userName.IndexOf(@"\") + 1);
            }
            else
            {
                partDomain = Environment.MachineName;
                partUser = userName;
                userName = partDomain + @"\" + partUser;
            } 
            
            startInfo.UseShellExecute = false;
            startInfo.UserName = partUser;
            startInfo.Domain = partDomain;
            startInfo.Password = secPswd;
            startInfo.LoadUserProfile = true;
           
            startInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(name);
            startInfo.FileName = name;
            startInfo.Arguments = args;
            using (Process process = Process.Start(startInfo))
            {
                //process.PriorityClass = ProcessPriorityClass.RealTime;
                if (waitForExit == true)
                {
                    if (process.WaitForExit((int)TimeSpan.FromMinutes(15).TotalMilliseconds) == false)
                    {
                        throw new TimeoutException("Timed out waiting for CorFlags");
                    }
                }
            }
        }

        private static void StartProcess(string name, string args, bool waitForExit)
        {
       //    Console.WriteLine(name);
            WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());

            // need to elevate with UAC
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = true;

            if (pricipal.IsInRole(WindowsBuiltInRole.Administrator) == false)
            {
                startInfo.Verb = "runas";
            }

            startInfo.FileName = name;
            startInfo.Arguments = args;
            
            using (Process process = Process.Start(startInfo))
            {
                //process.PriorityClass = ProcessPriorityClass.RealTime;
                if (waitForExit == true)
                {
                    if (process.WaitForExit((int)TimeSpan.FromMinutes(15).TotalMilliseconds) == false)
                    {
                        throw new TimeoutException("Timed out waiting for CorFlags");
                    }
                }
            }
        }

        private static bool IsInstalled(string product)
        {
            string[] vcRedistVersions={" - 10.0.30319"," - 10.0.40219"};
            using (RegistryKey uninstallKey = Registry.LocalMachine.OpenSubKey(@"Software\microsoft\windows\currentversion\uninstall"))
            {
                if (uninstallKey != null)
                {
                    string[] subKeys = uninstallKey.GetSubKeyNames();

                    foreach (string subKeyName in subKeys)
                    {
                        using (RegistryKey subKey = uninstallKey.OpenSubKey(subKeyName))
                        {
                            string displayName = subKey.GetValue("DisplayName") as string;

                            for (int i = 0; i < vcRedistVersions.Length; i++)
                            {
                                if (string.Equals(displayName, product+vcRedistVersions, StringComparison.OrdinalIgnoreCase) == true)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        private static void InstallX86()
        {
            if (IsInstalled("Microsoft Visual C++ 2010  x86 Redistributable") == false)
            {
                string file = Path.Combine(new FileInfo(typeof(Program).Assembly.Location).DirectoryName, "vcredist_x86.exe");

                if (File.Exists(file) == false)
                {
                    using (Stream resourceStream = typeof(Program).Assembly.GetManifestResourceStream("MS.Internal.SulpHur.SulpHurClientLauncher.vcredist_x86.exe"))
                    {
                        using (FileStream fileStream = File.Open(file, FileMode.Create))
                        {
                            resourceStream.CopyTo(fileStream);
                        }
                    }
                }

                StartProcess(file, "/q /norestart", true);
            }
        }

        private static void RenameConfigFiles()
        {
            try
            {
                if (File.Exists("MS.Internal.SulpHur.SulpHurClient.exe.config.dll"))
                {
                    File.Copy("MS.Internal.SulpHur.SulpHurClient.exe.config.dll", "MS.Internal.SulpHur.SulpHurClient.exe.config");
                    File.Delete("MS.Internal.SulpHur.SulpHurClient.exe.config.dll");
                }
            }
            catch
            {
                Console.WriteLine("Exception when rename config files!");
            }
        }
    }
}
