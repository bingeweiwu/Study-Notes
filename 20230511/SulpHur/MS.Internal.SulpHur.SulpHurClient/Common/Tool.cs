using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MS.Internal.SulpHur.SulpHurClient.Common
{
    public class Tool
    {
        /// <summary>
        /// If the client need to upgrade.
        /// </summary>
        /// <returns>If true,the client is not the latest</returns>
        public static bool UpdateState()
        {
            try
            {
                string locVersion = LocVersion();
                string remoteVersion = RemoteVersion();
                if (!string.IsNullOrEmpty(remoteVersion) && !locVersion.Equals(remoteVersion))
                {
                    Trace.WriteLine($"locVersion is {locVersion},remoteVersion is {remoteVersion}.Need to upgrade!");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Get the update state failed,EX:{ex.Message}");
            }
            return false;
        }

        /// <summary>
        /// Local version of client
        /// </summary>
        /// <returns>current version</returns>
        public static string LocVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        /// <summary>
        /// Lemote version of client
        /// </summary>
        /// <returns>remote version</returns>
        public static string RemoteVersion()
        {
            string appName = Assembly.GetExecutingAssembly().GetName().Name;
            string deployPath = ConfigurationManager.AppSettings["DeployPath"];

            //The keyvault need the device install the certificate. If not, it will be failed to get the pwd
            //{
            //    string url = "https://sccmsharedkeyvault.vault.azure.net:443/secrets/Nova-SMSAccess";
            //    string pwd = string.Empty;
            //    try
            //    {
            //        pwd = Microsoft.ConfigurationManagement.Test.KeyVault.KeyVault.DefaultInstance.GetSecret(url);
            //        ExecuteCmd($"net use \"{deployPath}\" /u:SMSAccess {pwd} &exit");
            //    }
            //    catch (Exception ex)
            //    {
            //        Trace.WriteLine($"Can't get the pwd from keyvault,EX:{ex.Message}");
            //    }
            //}
            string remoteVersion = GetVersionInfo($"{deployPath + appName}.exe");

            return remoteVersion;
        }

        /// <summary>
        /// Execute a cmd use dos window
        /// </summary>
        /// <param name="cmd">the cmd you want to run</param>
        public static void ExecuteCmd(string cmd)
        {
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = false;
            p.Start();
            p.StandardInput.WriteLine(cmd);
        }

        /// <summary>
        /// Get version of a file
        /// </summary>
        /// <param name="path">full name of a file</param>
        /// <returns></returns>
        public static string GetVersionInfo(string path)
        {
            System.IO.FileInfo fileInfo = null;
            try
            {
                fileInfo = new System.IO.FileInfo(path);
            }
            catch (Exception e)
            {
                Trace.WriteLine($"{path} is not exist,ex:{e.Message}");
            }

            if (fileInfo != null && fileInfo.Exists)
            {
                System.Diagnostics.FileVersionInfo info = System.Diagnostics.FileVersionInfo.GetVersionInfo(path);
                return info.ProductVersion;
            }
            return string.Empty;
        }
        /// <summary>
        /// Sulphur client upgrade
        /// </summary>
        /// <param name="file">File name(current path)</param>
        public static void RunBat(string file)
        {
            if (File.Exists(file))
            {
                try
                {
                    Process p = new Process();
                    p.StartInfo.FileName = file;
                    p.Start();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                    Trace.WriteLine("升级失败");
                }
            }
            else
            {
                Trace.WriteLine($"{file} is not exists!!!");
            }
        }
    }
}
