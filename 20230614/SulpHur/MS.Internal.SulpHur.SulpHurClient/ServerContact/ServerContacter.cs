using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using MS.Internal.SulpHur.UICompliance;
using System.ServiceModel;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace MS.Internal.SulpHur.SulpHurClient.ServerContact
{
    public class ServerContacter
    {
        public event EventHandler ServerReconnected;

        private static readonly object _serverContacterLocker = new object();
        private static ServerContacter _instance;
        public static ServerContacter Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_serverContacterLocker)
                    {
                        if (_instance == null)
                        {
                            _instance = new ServerContacter();
                        }
                    }
                }

                return _instance;
            }
        }
        public static ClientCallBackImplements callback = new ClientCallBackImplements();


        public AdditionInformations additionInformation = new AdditionInformations();
        //Client
        private SulpHurWCFServiceClient client = null;
        private SulpHurWCFServiceClient StableClient
        {
            get
            {
                //steady connection, re-try maximum 10 times
                int tries = 1;
                int maxTries = 10;
                while ((client == null ||
                        client.State.Equals(CommunicationState.Closed) ||
                        client.State.Equals(CommunicationState.Closing) ||
                        client.State.Equals(CommunicationState.Faulted)) &&
                        tries < maxTries)
                {
                    Trace.WriteLine(string.Format("{0} tries to reconnect to server {1}.", tries, this.serverName));
                    this.Reconnect();
                    tries++;
                }

                //start connection waiter, default will try once per 30 min
                if (this.isConnected == false)
                {
                    this.isWaitingConnection = true;
                    this.connectionWaiter = new ConnectionWaiter(this);
                    this.connectionWaiter.ServerConnected += new EventHandler(delegate
                        {
                            if (this.ServerReconnected != null)
                            {
                                this.ServerReconnected(this, null);
                            }
                            this.connectionWaiter.Stop();
                            this.connectionWaiter = null;
                            this.isWaitingConnection = false;
                        });
                    this.connectionWaiter.Start();
                }

                return client;
            }
        }
        //IsConnected
        private bool isConnected = false;
        public bool IsConnected
        {
            get
            {
                return isConnected;
            }
        }
        //ServerName
        private string serverName = string.Empty;
        public string ServerName
        {
            get { return serverName; }
        }
        private ConnectionWaiter connectionWaiter;
        //IsWaitingConnection
        private bool isWaitingConnection = false;
        public bool IsWaitingConnection
        {
            get
            {
                return IsWaitingConnection;
            }
        }


        private ServerContacter()
        {
        }
        SettingsView settingsView = new SettingsView();
        App app = App.Current as App;
        public bool Connect(string serverName)
        {
            this.serverName = serverName;
            try
            {
                NetTcpBinding binding = new NetTcpBinding("netTcpBindingEndPoint");
                string address = string.Format("net.tcp://{0}:50069/sulpHurservice", this.serverName);
                EndpointAddress endpoint = new EndpointAddress(address);
                this.client = new SulpHurWCFServiceClient(new InstanceContext(callback), binding, endpoint);

                this.client.ClientCredentials.Windows.ClientCredential.Domain = this.serverName;
                this.client.Open();
                client.Register(additionInformation.ComputerName);
                this.isConnected = true;
                this.SetNotifyIconText(app.NotifyIcon, string.Format(MS.Internal.SulpHur.SulpHurClient.Properties.Resources.TrayMessage_Connected, serverName));
                Trace.WriteLine(string.Format("Connet succeed With Name:{0}", this.serverName));

                ////to workaround tray icon text 64 characters limitation
                //this.SetNotifyIconText(app.NotifyIcon, string.Format(MS.Internal.SulpHur.SulpHurClient.Properties.Resources.TrayMessage_Connected, settingsView.ServerName));
                ////tip text
                //string tipTitle = MS.Internal.SulpHur.SulpHurClient.Properties.Resources.AppName;
                //string tipText = string.Format(MS.Internal.SulpHur.SulpHurClient.Properties.Resources.TrayMessage_Connected, settingsView.ServerName);
                //app.NotifyIcon.ShowBalloonTip(3000, tipTitle, tipText, System.Windows.Forms.ToolTipIcon.Info);

                //context menu items
                app.NotifyIcon.ContextMenu.MenuItems.Find("UploadCachedData", false)[0].Enabled = true;

                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Connet Failed With Name:{0}, Exception:{1}", this.serverName, ex));

                SetNotifyIconText(app.NotifyIcon, string.Format(MS.Internal.SulpHur.SulpHurClient.Properties.Resources.TrayMessage_ConnectFailed, settingsView.ServerName));
                //tip text
                string tipTitle = MS.Internal.SulpHur.SulpHurClient.Properties.Resources.AppName;
                string tipText = string.Format(MS.Internal.SulpHur.SulpHurClient.Properties.Resources.TrayMessage_ConnectFailed, settingsView.ServerName);
                app.NotifyIcon.ShowBalloonTip(3000, tipTitle, tipText, System.Windows.Forms.ToolTipIcon.Error);
                keybd_event((byte)Keys.NumLock, 0, 0, 0);
                app.NotifyIcon.ContextMenu.MenuItems.Find("UploadCachedData", false)[0].Enabled = true;
                return false;
            }
        }


        [DllImport("user32.dll", EntryPoint = "keybd_event")]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
        //to workaround tray icon text 64 characters limitation
        private void SetNotifyIconText(System.Windows.Forms.NotifyIcon ni, string text)
        {
            if (text.Length >= 128) throw new ArgumentOutOfRangeException("Text limited to 127 characters");
            Type t = typeof(System.Windows.Forms.NotifyIcon);
            BindingFlags hidden = BindingFlags.NonPublic | BindingFlags.Instance;
            t.GetField("text", hidden).SetValue(ni, text);
            if ((bool)t.GetField("added", hidden).GetValue(ni))
                t.GetMethod("UpdateIcon", hidden).Invoke(ni, new object[] { true });
        }

        public void DisConnect()
        {
            if (this.client != null &&
                !this.client.State.Equals(CommunicationState.Closed) &&
                !this.client.State.Equals(CommunicationState.Faulted))
            {
                this.client.Close();
                this.isConnected = false;
            }
        }
        public void StopWaitConnection()
        {
            if (this.isWaitingConnection == true && this.connectionWaiter != null)
            {
                this.connectionWaiter.Stop();
            }
        }
        private void Reconnect()
        {
            if (this.client!=null)
            {
                this.client.Abort();
                this.client.Close();
                this.isConnected = false;                
            }
            this.Connect(settingsView.ServerName);
        }

        public bool TryUploadUIData(WindowPageInfo winPageInfo, Bitmap bitmap, string guidFolderPath, out string message)
        {
            try
            {
                //v-yiwzha: override the buildLanguage to the captured time.
                string buildLanguage = "";
                string productVersion = "";
                using (StreamReader sr = File.OpenText(Path.Combine(guidFolderPath, "lan.txt")))
                {
                    for (int trytime = 6; buildLanguage == "" && trytime > 0; trytime--)
                    {
                        buildLanguage = sr.ReadLine();
                        productVersion = sr.ReadLine();
                    }
                    if (buildLanguage == "")
                    {
                        Trace.WriteLine("UI uploaded failed: Error occurred in opening language txt file.--buildLanguage");
                        message = "Error occurred in opening language txt file.";
                        return false;
                    }
                    if (productVersion == "")
                    {
                        Trace.WriteLine("UI uploaded failed: Error occurred in opening language txt file.--productVersion");
                        message = "Error occurred in opening language txt file.";
                        return false;
                    }
                }
                additionInformation.ProductLanguage = buildLanguage;
                additionInformation.ProductVersion = new Version(productVersion);
                DateTime start = DateTime.Now;
                this.StableClient.ReceiveUI(winPageInfo, bitmap, additionInformation);
                DateTime end = DateTime.Now;
                TimeSpan span = end - start;

                Trace.WriteLine("😜UI Uploaded success. Build Type:" + additionInformation.BuildType + ",uploadtime:" + span.TotalSeconds + "s");
                //AsyncCallback cb = new AsyncCallback(UploadUICallBack);
                //this.StableClient.BeginReceiveUI(winPageInfo, bitmap, additionInformation, cb, this.StableClient);
                message = null;
                return true;
            }
            catch (Exception e)
            {
                Trace.WriteLine(string.Format("UI uploaded failed: {0}", e));
                message = string.Format("{0}", e);
                return false;
            }
        }

        public List<ComplianceRule> GetEnableRules()
        {
            List<ComplianceRule> rules = new List<ComplianceRule>();
            try
            {
                ComplianceRule[] rul = this.StableClient.GetRules();
                foreach (ComplianceRule r in rul)
                {
                    rules.Add(r);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
            }
            return rules;
        }

        //public ComplianceResult QueryResultByName(string name)
        //{
        //    ComplianceResult r = new ComplianceResult();
        //    SulpHurWCFServiceClient client = null;
        //    try
        //    {
        //        //init connection
        //        client = new SulpHurWCFServiceClient();
        //        r = client.QueryResult(name);
        //    }
        //    catch
        //    { }
        //    return r;
        //}

        //public void UploadWebUIData(WebPageInfo webPageInfo, Bitmap bitmap, Tile t)
        //{
        //    try
        //    {
        //        AsyncCallback cb = new AsyncCallback(UploadWebUICallBack);
        //        this.StableClient.BeginReceiveWebUI(webPageInfo, bitmap, additionInformation, cb, this.StableClient);
        //        t.Status = "Uploading...";
        //    }
        //    catch (Exception e)
        //    {
        //        t.Status = "Abandoned";
        //        Trace.WriteLine(e);
        //    }
        //}

        public void UploadUICallBack(IAsyncResult ar)
        {
            Trace.WriteLine(DateTime.Now + ":UI Uploaded success.");
        }

        //public void UploadWebUICallBack(IAsyncResult ar)
        //{
        //    SulpHurWCFServiceClient client = ar.AsyncState as SulpHurWCFServiceClient;
        //    UploadResults result = client.EndReceiveWebUI(ar);
        //    string status = string.Format("{0},{1}", result.Type.ToString(), result.Message);
        //    ServerContacter.Instance.GallaryModel.listbox.Dispatcher.Invoke(new Action(delegate()
        //    {
        //        try
        //        {
        //            Tile callbackT = GallaryModel.TileList.Single(t => t.id == result.id);
        //            callbackT.Status = status;
        //        }
        //        catch (Exception e)
        //        {
        //            Trace.WriteLine(e);
        //        }
        //    }));
        //}

        public void UploadLog(string newLines)
        {
            try
            {
                this.StableClient.UploadLog(newLines, additionInformation);
            }
            catch (Exception)
            {
                Trace.WriteLine("fail to upload log");
            }
        }
    }
}
