using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Text.RegularExpressions;
using MS.Internal.SulpHur.SulpHurClient.ServerContact;
using System.Configuration;
using System.Diagnostics;

namespace MS.Internal.SulpHur.SulpHurClient
{
    /**
     日志功能*/
    public class AdminUILogMonitor
    {
        static DateTime lastModifiedTime = DateTime.MinValue;
        static long oldLength = 0;
        static Regex reg = new Regex("(Exception)+");
        public static void MonitorFile(string path){
            try
            {
                Trace.WriteLine("start monitor:" + path);
                oldLength = new FileInfo(path).Length;
                lastModifiedTime = File.GetLastWriteTime(path);

                using (FileSystemWatcher watcher = new FileSystemWatcher(Path.GetDirectoryName(path), Path.GetFileName(path)))
                {
                    while (bool.Parse(ConfigurationManager.AppSettings["EnableUploadLogException"]))
                    {
                        Trace.WriteLine("wait log change");
                        WaitForChangedResult result = watcher.WaitForChanged(WatcherChangeTypes.Changed);
                        //Console.WriteLine(result.ChangeType.ToString());
                        //MessageBox.Show(result.ChangeType.ToString());
                        Trace.WriteLine("detect change");
                        if (File.GetLastWriteTime(path) > lastModifiedTime)
                        {
                            using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                stream.Position = oldLength;

                                using (StreamReader reader = new StreamReader(stream))
                                {
                                    string newLines = reader.ReadToEnd();
                                    oldLength = stream.Length;
                                    //Console.WriteLine(newLines);
                                    //MessageBox.Show(newLines);

                                    if (reg.IsMatch(newLines))
                                    {
                                        string pattern = @"\[.*\]\[.*\].*:";

                                        IList<int> indecesS = new List<int>();
                                        IList<int> indecesE = new List<int>();
                                        foreach (Match match in Regex.Matches(newLines, pattern))
                                        {
                                            indecesS.Add(match.Index + match.Value.Length);
                                            indecesE.Add(match.Index);
                                        }

                                        List<string> exUploads = new List<string>();
                                        for (int i = 0; i < indecesS.Count; i++)
                                        {
                                            if (i == indecesS.Count - 1)
                                            {
                                                exUploads.Add(newLines.Substring(indecesS[i]));
                                            }
                                            else
                                            {
                                                exUploads.Add(newLines.Substring(indecesS[i], indecesE[i + 1] - indecesS[i]));
                                            }
                                        }

                                        foreach (string s in exUploads)
                                        {
                                            if (!string.IsNullOrEmpty(s) && s.Length > 5&&reg.IsMatch(s))
                                            {
                                                ServerContacter.Instance.UploadLog(s);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e) {
                Trace.WriteLine(e);
            }
        }
    }
}
