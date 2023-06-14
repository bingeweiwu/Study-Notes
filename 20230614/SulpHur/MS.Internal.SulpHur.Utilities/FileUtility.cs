using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MS.Internal.SulpHur.Utilities
{
    public class FileUtility
    {
        public static string mainBuildRootPath = string.Empty;
        public static string releaseBuildRootPath = string.Empty;
        public static string repoVersionRelatedPath = string.Empty;
        public static Regex regexVersionFolder = new Regex(@"^\d\d\d\d\.\d\d\d\d$");
        public static Regex regexMilestoneFolder = new Regex(@"^CM\d\d\d\d$");

        static FileUtility()
        {
            mainBuildRootPath = ConfigurationManager.AppSettings["MainBuildRootPath"];
            if (string.IsNullOrEmpty(mainBuildRootPath))
                throw new ArgumentNullException("mainBuildRootPath");
            releaseBuildRootPath = ConfigurationManager.AppSettings["ReleaseBuildRootPath"];
            if (string.IsNullOrEmpty(releaseBuildRootPath))
                throw new ArgumentNullException("releaseBuildRootPath");
            repoVersionRelatedPath = ConfigurationManager.AppSettings["RepoVersionFileRelatedPath"];
            if (string.IsNullOrEmpty(repoVersionRelatedPath))
                throw new ArgumentNullException("repoVersionRelatedPath");
        }

        public static string ChangeNewBuildToOldFormat(string newBuild)
        {
            string oldVersion = SearchMainBuild(newBuild);
            if (string.IsNullOrEmpty(oldVersion))
                oldVersion = SearchReleaseBuild(newBuild);
            return oldVersion;
        }
        public static string SearchMainBuild(string newBuild)
        {
            if (!Directory.Exists(mainBuildRootPath))
                throw new DirectoryNotFoundException(mainBuildRootPath);

            string[] versionFolderPaths = null;
            string fileFullPath = string.Empty;
            string oldVersionFolder = string.Empty;
            string newVersionFromFile = string.Empty;
            try
            {
                versionFolderPaths = Directory.GetDirectories(mainBuildRootPath);
                for (int i = versionFolderPaths.Length - 1; i >= 0; i--)
                {
                    oldVersionFolder = versionFolderPaths[i].Substring(versionFolderPaths[i].LastIndexOf('\\') + 1);
                    if (!regexVersionFolder.IsMatch(oldVersionFolder))
                        continue;

                    fileFullPath = Path.Combine(versionFolderPaths[i], repoVersionRelatedPath);
                    if (!File.Exists(fileFullPath))
                        throw new FileNotFoundException(fileFullPath);

                    KeyValuePair<string, string> versionMap = GetBuildVersionPair(fileFullPath);
                    if (string.IsNullOrEmpty(versionMap.Key) || string.IsNullOrEmpty(versionMap.Value))
                        throw new ArgumentNullException("versionMap");

                    newVersionFromFile = versionMap.Key;
                    if (newBuild.Equals(newVersionFromFile))
                        return versionMap.Value;
                    else if (string.Compare(newBuild, newVersionFromFile) > 0)
                        break;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return null;
        }
        public static string SearchReleaseBuild(string newBuild)
        {
            if (!Directory.Exists(releaseBuildRootPath))
                throw new DirectoryNotFoundException(releaseBuildRootPath);

            string[] milestoneFolderPaths = null;
            string[] versionFolderPaths = null;
            string fileFullPath = string.Empty;
            string milestone = string.Empty;
            string oldVersionFolder = string.Empty;
            string newVersionFromFile = string.Empty;
            bool finish = false;
            try
            {
                milestoneFolderPaths = Directory.GetDirectories(releaseBuildRootPath);
                for (int i = milestoneFolderPaths.Length - 1; i >= 0; i--)
                {
                    milestone = milestoneFolderPaths[i].Substring(milestoneFolderPaths[i].LastIndexOf('\\') + 1);
                    if (!regexMilestoneFolder.IsMatch(milestone))
                        continue;

                    finish = false;
                    versionFolderPaths = Directory.GetDirectories(milestoneFolderPaths[i]);
                    for (int j = versionFolderPaths.Length - 1; j >= 0; j--)
                    {
                        oldVersionFolder = versionFolderPaths[j].Substring(versionFolderPaths[j].LastIndexOf('\\') + 1);
                        if (!regexVersionFolder.IsMatch(oldVersionFolder))
                            continue;

                        fileFullPath = Path.Combine(versionFolderPaths[j], repoVersionRelatedPath);
                        if (!File.Exists(fileFullPath))
                            throw new FileNotFoundException(fileFullPath);

                        KeyValuePair<string, string> versionMap = GetBuildVersionPair(fileFullPath);
                        if (string.IsNullOrEmpty(versionMap.Key) || string.IsNullOrEmpty(versionMap.Value))
                            throw new ArgumentNullException("versionMap");

                        newVersionFromFile = versionMap.Key;
                        if (newBuild.Equals(newVersionFromFile))
                            return versionMap.Value;
                        else if (string.Compare(newBuild, newVersionFromFile) > 0)
                        {
                            finish = true;
                            break;
                        }
                    }
                    if (finish)
                        break;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return null;
        }

        /// <summary>
        /// Get build version map with format --- new build format,old build format
        /// e.g. 5.2211.1047.1000,5.0.9091.1008
        /// </summary>
        /// <param name="RepoVersionFilePath"></param>
        /// <returns></returns>
        public static KeyValuePair<string, string> GetBuildVersionPair(string RepoVersionFilePath)
        {
            if (!File.Exists(RepoVersionFilePath))
                throw new FileNotFoundException(RepoVersionFilePath);

            KeyValuePair<string, string> buildVersionPair = default(KeyValuePair<string, string>);
            string line1 = string.Empty, line2 = string.Empty;
            string newFormatVersion = string.Empty, oldFormatVersion = string.Empty;
            using (StreamReader sr = new StreamReader(RepoVersionFilePath))
            {
                line1 = sr.ReadLine().Trim();
                newFormatVersion = line1.Substring(line1.IndexOf('=') + 1);
                line2 = sr.ReadLine().Trim();
                oldFormatVersion = line2.Substring(line2.IndexOf('=') + 1);
                buildVersionPair = new KeyValuePair<string, string>(newFormatVersion, oldFormatVersion);
            }
            return buildVersionPair;
        }
    }
}
