using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Drawing;
using System.Diagnostics;
using MS.Internal.SulpHur.UICompliance;
using MS.Internal.SulpHur.SulpHurClient.ServerContact;
using System.Threading;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using XmlExchange;

namespace MS.Internal.SulpHur.SulpHurClient
{
    public class PageData
    {
        public WindowPageInfo winPageInfo;
        public Bitmap bitMap;
        public bool isLocalData = false;
        public string buildLanguage = "";
        public Version productVersion;
        /// <summary>
        /// if data is localdata ,guid is the filename
        /// </summary>
        public string guid;
    }

    public class DataCenter
    {
        //instance
        private static DataCenter instance = null;
        //locker
        public static readonly object _queueLocker = new object();
        //v-yiwzha: Upload State
        public enum uploadState
        {
            Processing,
            Abandon,
            Ok,
            Error
        }

        //v-yiwzha: EventHandler For Upload
        public event EventHandler UploadBegin;
        public event EventHandler UploadAbandon;
        public event EventHandler UploadEnd;
        public event EventHandler<UploadErrorEventArgs> UploadError;
        private string abandonDataFolder = ConfigurationManager.AppSettings["AbandonDataFolder"];

        //SettingView
        public SettingsView SettingView { get; set; }
        //DataQueue
        private Queue<PageData> pageDataQueue = new Queue<PageData>();
        public Queue<PageData> PageDataQueue
        {
            get
            {
                return pageDataQueue;
            }
        }
        private Thread sendDataThread = null;
        private Thread loadLocalDataThread = null;

        //singleton
        public static DataCenter Instance
        {
            get
            {
                if (instance == null)
                    instance = new DataCenter();

                return instance;
            }
        }
        private DataCenter()
        {
            ServerContacter.Instance.ServerReconnected += new EventHandler(delegate
            {
                this.StartToLoadLocalData();
            });
        }


        public void StartToSendData()
        {
            sendDataThread = new Thread(new ThreadStart(this.SendData));
            sendDataThread.Name = "SendData";
            sendDataThread.IsBackground = true;
            sendDataThread.Priority = ThreadPriority.BelowNormal;
            sendDataThread.Start();
        }
        public void StartToLoadLocalData()
        {
            loadLocalDataThread = new Thread(new ThreadStart(this.LoadLocalData));
            loadLocalDataThread.Name = "LoadLocalData";
            loadLocalDataThread.IsBackground = true;
            loadLocalDataThread.Priority = ThreadPriority.BelowNormal;
            loadLocalDataThread.Start();
        }
        public void StopSend()
        {
            if (this.sendDataThread != null && this.sendDataThread.IsAlive)
            {
                this.sendDataThread.Abort();
                this.sendDataThread = null;
            }
        }
        public bool IsCacheDataExist
        {
            get
            {
                if (this.CachedPageCount > 0)
                {
                    return true;
                }

                return false;
            }
        }
        public int CachedPageCount
        {
            get
            {
                if (!string.IsNullOrEmpty(SettingView.LocalDataFolder) && Directory.Exists(SettingView.LocalDataFolder))
                {
                    return Directory.GetDirectories(SettingView.LocalDataFolder).Length;
                }

                return 0;
            }
        }
        public static Dictionary<Guid, List<AssemblyInfo>> guidDic = new Dictionary<Guid, List<AssemblyInfo>>();
        private void SendData()
        {
            Trace.WriteLine(string.Format("SettingView.IsSendThreadRun:{0}", SettingView.IsSendThreadRun));

            //Dictionary<Guid, List<AssemblyInfo>> guidDic = new Dictionary<Guid, List<AssemblyInfo>>();
            while (SettingView.IsSendThreadRun)
            {
                try
                {
                    //idle
                    System.Threading.Thread.Sleep(5000);
                    if (this.pageDataQueue.Count > 0)
                    {
                        PageData infoTemp;
                        lock (DataCenter._queueLocker)
                        {
                            infoTemp = this.pageDataQueue.Dequeue();
                        }
                        //miss item without control type window which does not a valid page
                        if (infoTemp.winPageInfo.ei.ControlType.Equals(MS.Internal.SulpHur.UICompliance.ControlType.Window) == false&&!App.UIAutoMainInterfaceCaptureFlag)
                        {
                            //Trace.WriteLine($"infoTemp.winPageInfo.ei.ControlType.Equals(MS.Internal.SulpHur.UICompliance.ControlType.Window)={infoTemp.winPageInfo.ei.ControlType.Equals(MS.Internal.SulpHur.UICompliance.ControlType.Window)}");
                            continue;
                        }
                        //GUID
                        Guid currentGuid;
                        if (infoTemp.isLocalData)
                        {
                            currentGuid = new Guid(infoTemp.guid);
                        }
                        else
                        {
                            CMGUIDGenerator generator = new CMGUIDGenerator();
                            currentGuid = generator.GenerateGUID(infoTemp.winPageInfo.ei, infoTemp.buildLanguage);
                        }

                        //Remove the duplicate data
                        if (guidDic.ContainsKey(currentGuid))
                        {
                            Trace.WriteLine("The picture with GUID:[" + currentGuid + "] is already uploaded.");
                            //v-yiwzha: update states of Uploading            
                            if (this.UploadAbandon != null)
                            {
                                this.UploadAbandon(uploadState.Abandon, null);
                            }
                            try
                            {
                                string guidFolderPath = Path.Combine(SettingView.SaveLocalFolder, currentGuid.ToString());
                                //Trace.WriteLine("folder path:" + guidFolderPath);
                                if (Directory.Exists(guidFolderPath))
                                {
                                    Directory.Delete(guidFolderPath);
                                }
                            }
                            catch (Exception ex)
                            {
                                Trace.WriteLine($"delete failed:{ex.Message}");
                            }
                        }


                        //PageIdentifier
                        AssemblyInfo pageIdentifier = null;
                        if (infoTemp.winPageInfo.AssemblyInfoList != null)
                        {
                            pageIdentifier = infoTemp.winPageInfo.AssemblyInfoList.FirstOrDefault(item => item.IsPageIdentifier == true);
                        }
                        if (!guidDic.ContainsKey(currentGuid) ||
                            (pageIdentifier != null &&
                             guidDic[currentGuid] != null &&
                             !guidDic[currentGuid].Contains(pageIdentifier)) ||
                            infoTemp.isLocalData)
                        {
                            //write log
                            if (infoTemp.isLocalData)
                            {
                                //GUID
                                Trace.WriteLine(string.Format("currentGuid(CachedData): {0}", currentGuid));
                                //AssemblyInfo
                                if (pageIdentifier != null)
                                {
                                    Trace.WriteLine(string.Format("PageIdentifier(CachedData): {0}${1}", pageIdentifier.AssemblyName, pageIdentifier.FullTypeName));
                                }
                                else
                                {
                                    Trace.WriteLine("No page identifier.(CachedData)");
                                }
                            }
                            else
                            {
                                //GUID
                                Trace.WriteLine(string.Format("currentGuid: {0}", currentGuid));
                                //AssemblyInfo
                                if (pageIdentifier != null)
                                {
                                    Trace.WriteLine(string.Format("PageIdentifier: {0}${1}", pageIdentifier.AssemblyName, pageIdentifier.FullTypeName));
                                }
                                else
                                {
                                    Trace.WriteLine("No page identifier.");
                                }
                            }

                            //cache data
                            string guidFolderPath = Path.Combine(SettingView.SaveLocalFolder, currentGuid.ToString());
                            if (SettingView.IsSaveLocalEnable)
                            {
                                if (!Directory.Exists(SettingView.SaveLocalFolder))
                                {
                                    System.IO.Directory.CreateDirectory(SettingView.SaveLocalFolder);
                                }
                                //When uploading locally cached data,should not save the newly extracted data again
                                if (!infoTemp.isLocalData && !Directory.Exists(guidFolderPath) || App.UIAutoFlag)
                                {
                                    SaveLocalData(infoTemp.bitMap, infoTemp.winPageInfo, infoTemp.buildLanguage, infoTemp.productVersion, currentGuid);
                                }
                            }
                            //upload data
                            if (SettingView.IsSendToServer && ServerContacter.Instance.IsConnected)
                            {
                                try
                                {
                                    string message = string.Empty;
                                    bool isSucceed = ServerContacter.Instance.TryUploadUIData(infoTemp.winPageInfo, infoTemp.bitMap, guidFolderPath, out message);
                                    //delete guid folder if upload succeed
                                    if (isSucceed)
                                    {

                                        //guidDic
                                        if (!guidDic.ContainsKey(currentGuid))
                                        {
                                            List<AssemblyInfo> assemblyInfoList = new List<AssemblyInfo>() { pageIdentifier };
                                            guidDic.Add(currentGuid, assemblyInfoList);
                                        }
                                        else if (!guidDic[currentGuid].Contains(pageIdentifier))
                                        {
                                            guidDic[currentGuid].Add(pageIdentifier);
                                        }

                                        try
                                        {
                                            if (UploadEnd != null)
                                            {
                                                UploadEnd(uploadState.Ok, null);
                                            }
                                            if (Directory.Exists(guidFolderPath))
                                            {
                                                Directory.Delete(guidFolderPath, true);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Trace.WriteLine(string.Format("Exception when delete GUID folder: {0}{1}", infoTemp.guid, ex));
                                        }
                                    }
                                    else
                                    {
                                        //v-yiwzha: error event
                                        if (this.UploadError != null)
                                        {
                                            UploadErrorEventArgs eArgs = new UploadErrorEventArgs(message);
                                            this.UploadError(uploadState.Error, eArgs);
                                        }
                                        //Remove the logic here, and transfer only known error data, such as language not obtained
                                        //if (!message.Contains("TimeoutException"))
                                        //{
                                        Trace.WriteLine(DateTime.Now + ":Upload Failed." + message);
                                        //    Trace.WriteLine("😵upload failed,Guid(1)===> " + currentGuid + ",please investigate:" + guidFolderPath);
                                        //    Directory.Move(guidFolderPath, this.abandonDataFolder + currentGuid.ToString());
                                        //}                                        
                                    }
                                }
                                catch (Exception e)
                                {
                                    //v-yiwzha: error event
                                    if (this.UploadError != null)
                                    {
                                        UploadErrorEventArgs eArgs = new UploadErrorEventArgs(e.Message);
                                        this.UploadError(uploadState.Error, eArgs);
                                    }
                                    //Remove the logic here, and transfer only known error data, such as language not obtained
                                    //Trace.WriteLine(DateTime.Now + ":Upload Failed." + e.Message);
                                    //Trace.WriteLine("😵upload failed,Guid(2)==> " + currentGuid + ",please investigate:" + guidFolderPath);
                                    //Directory.Move(guidFolderPath, this.abandonDataFolder + currentGuid.ToString());
                                }
                            }
                        }
                        // dispose image since it is unmanaged resource
                        infoTemp.bitMap.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(string.Format("Unexpected Exception: {0}", ex));
                }
            }
        }
        private void LoadLocalData()
        {
            Trace.WriteLine(string.Format("SettingView.LocalDataFolder:{0}", SettingView.LocalDataFolder));

            try
            {
                if (!string.IsNullOrEmpty(SettingView.LocalDataFolder) && Directory.Exists(SettingView.LocalDataFolder))
                {
                    IEnumerable<string> ienu = Directory.EnumerateDirectories(SettingView.LocalDataFolder);
                    IEnumerator<string> enumerator = ienu.GetEnumerator();
                    bool validData;
                    while (enumerator.MoveNext())
                    {
                        validData = true;
                        string dir = enumerator.Current;
                        string bitmapPath = Path.Combine(dir, "img.jpg");
                        string elementPath = Path.Combine(dir, "uidata.xml");
                        string lanPath = Path.Combine(dir, "lan.txt");
                        string currentGuid = dir.Substring(dir.LastIndexOf("\\") + 1);
                        if (!File.Exists(bitmapPath))
                        {
                            Trace.WriteLine("***JPG file " + bitmapPath + " doesn't exist in local data, please investigate***");
                            validData = false;
                        }
                        if (!File.Exists(lanPath))
                        {
                            Trace.WriteLine("***Lan file " + lanPath + " doesn't exist in local data, please investigate***");
                            validData = false;
                        }
                        if (!File.Exists(elementPath))
                        {
                            Trace.WriteLine("***XML file " + elementPath + " doesn't exist in local data, please investigate***");
                            validData = false;
                        }
                        if (!validData)
                        {
                            try
                            {
                                Directory.Move(dir, this.abandonDataFolder + currentGuid);
                                Trace.WriteLine("😵error data,Guid==> " + currentGuid + ",please investigate");
                            }
                            catch (Exception ex)
                            {
                                Trace.WriteLine("Move file fail:" + ex.Message);
                            }
                            continue;
                        }

                        XElement xTemp = null;
                        Bitmap bitmap = null;
                        try
                        {
                            //Modified problem that resource is not normally released lead to Unable to delete image 
                            Image img = Bitmap.FromFile(bitmapPath);
                            bitmap = new Bitmap(img);
                            img.Dispose();
                            //bitmap = new Bitmap(Bitmap.FromFile(bitmapPath));
                            //BitmapImage
                            xTemp = XElement.Load(elementPath);
                        }
                        catch
                        {
                            Trace.WriteLine("***Error when read local data, please investigate***");
                            Trace.WriteLine("Error Folder:" + dir);
                            try
                            {
                                Directory.Move(dir, this.abandonDataFolder + currentGuid);
                                Trace.WriteLine("😵error data,Guid==> " + currentGuid + ",please investigate");
                            }
                            catch (Exception ex)
                            {
                                Trace.WriteLine("Move file fail:" + ex.Message);
                            }
                            continue;
                        }
                        WindowPageInfo winPageInfo = null;
                        try
                        {
                            winPageInfo = MS.Internal.SulpHur.Utilities.ExtensionMethods.FromXElement<WindowPageInfo>(xTemp);
                        }
                        catch
                        {
                            Trace.WriteLine("***Error when parse element, please investigate***");
                            Trace.WriteLine("Error Folder:" + dir);
                            try
                            {
                                Directory.Move(dir, this.abandonDataFolder + currentGuid);
                                Trace.WriteLine("😵error data,Guid==> " + currentGuid + ",please investigate");
                            }
                            catch (Exception ex)
                            {
                                Trace.WriteLine("Move file fail:" + ex.Message);
                            }
                            continue;
                        }

                        if (winPageInfo != null)
                        {
                            PageData info = new PageData();
                            info.winPageInfo = winPageInfo;
                            info.bitMap = bitmap;
                            info.isLocalData = true;
                            info.guid = dir.Substring(dir.LastIndexOf("\\") + 1);
                            lock (DataCenter._queueLocker)
                            {
                                this.pageDataQueue.Enqueue(info);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Unexpected Exception: {0}", ex));
            }
        }
        private void SaveLocalData(Bitmap bitMap, WindowPageInfo winPageInfo, string buildLanguage, Version productVersion, Guid guid)
        {
            XElement xe = MS.Internal.SulpHur.Utilities.ExtensionMethods.ToXElement<WindowPageInfo>(winPageInfo);

            Trace.WriteLine("buildlanguage：" + buildLanguage + ",production:+" + (productVersion == null));
            string path = System.IO.Path.Combine(SettingView.SaveLocalFolder, guid.ToString());
            Directory.CreateDirectory(path);
            string lanpath = System.IO.Path.Combine(path, "lan.txt");
            bitMap.Save(System.IO.Path.Combine(path, "img.jpg"));

            try
            {
                xe.Save(System.IO.Path.Combine(path, "uidata.xml"));
            }
            catch
            {
                Trace.WriteLine(DateTime.Now + ":Generate xml failed.");
            }
            //UI-Auto save data
            if (App.UIAutoFlag)
            {
                string ui_autoPath = @"C:\SulpHurClient\UIAutoData";
                Directory.CreateDirectory(ui_autoPath);
                string name = winPageInfo.ei.Name.Trim().Replace(" ", "");
                //bitMap.Save(System.IO.Path.Combine(ui_autoPath, name + ".jpg"));
                string _headerBar = "";

                UICompliance.ElementInformation root = winPageInfo.ei;
                XElement ele = MS.Internal.SulpHur.Utilities.ExtensionMethods.ToXElement<UICompliance.ElementInformation>(root);
                List<UICompliance.ElementInformation> list = Tool.ParseTreeToList(ele);
                foreach (var item in list)
                {
                    //_headerBar
                    //if (item.AutomationId == "_headerBar")
                    if (item.AutomationId == "_headlineLabel")
                    {
                        _headerBar = "_" + item.Name.Trim().Replace(" ", "");
                        break;
                    }
                }
                //bug 7387791
                StringBuilder rBuilder = new StringBuilder(name);
                foreach (char rInvalidChar in Path.GetInvalidFileNameChars())
                    rBuilder.Replace(rInvalidChar.ToString(), string.Empty);
                name = rBuilder.ToString();

                Trace.WriteLine($"ui_autoPath:{ui_autoPath};name:{name};_headerBar:{_headerBar}");
                
                string fileName = System.IO.Path.Combine(ui_autoPath, name + _headerBar + "_" + DateTime.Now.Hour +"."+ DateTime.Now.Minute +"."+ DateTime.Now.Second + ".xml");
                try
                {
                    xe.Save(fileName);
                }
                catch
                {
                    Trace.WriteLine(DateTime.Now + ":Generate UIAuto xml failed.");
                }
            }

            if (!File.Exists(lanpath))
            {
                using (StreamWriter sw = File.CreateText(lanpath))
                {
                    sw.AutoFlush = true;
                    if (!string.IsNullOrEmpty(buildLanguage))
                    {
                        sw.WriteLine(buildLanguage);
                    }
                    if (productVersion != null)
                    {
                        sw.WriteLine(productVersion.ToString());
                    }
                }
            }

        }
    }
    public class UploadErrorEventArgs : EventArgs
    {
        public string exceptionMessage { get; set; }

        public UploadErrorEventArgs(string exceptionMessage)
        {
            this.exceptionMessage = exceptionMessage;
        }
    }
}