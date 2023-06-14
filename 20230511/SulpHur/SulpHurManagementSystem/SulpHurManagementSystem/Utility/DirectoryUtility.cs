using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace SulpHurManagementSystem.Utility
{
    public class DirectoryUtility
    {
        public static string[] GetSubDirectory(string directory)
        {
            string[] directoryList = Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly);
            string results = string.Empty;
            string buildpath = string.Empty;
            if (directoryList != null)
            {
                for (int i = 0; i < directoryList.Length; i++)
                {
                    directoryList[i] = directoryList[i].Substring(directoryList[i].LastIndexOf("\\") + 1);
                }
            }
            return directoryList;
        }
    }
}