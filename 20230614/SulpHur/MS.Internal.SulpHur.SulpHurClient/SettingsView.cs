using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Threading;

namespace MS.Internal.SulpHur.SulpHurClient
{
    public class SettingsView : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        public SettingsView()
        {
            string sectionName = "AppSettingGroup/AppSettings";
            StartupConfigSection currentSection = null;

            try
            {
                currentSection = (StartupConfigSection)ConfigurationManager.GetSection(sectionName);
                this.ServerName = currentSection.ServerName.Value;

                this.isSaveLocalEnable = currentSection.SaveLocal.Enable;
                this.saveLocalFolder = currentSection.SaveLocal.Folder;

                this.isSendToServer = currentSection.SendToServer.Enable;
                this.localDataFolder = currentSection.SendToServer.LocalDataFolder;


                this.userName = currentSection.UserName.Value;

                string backupPath = ConfigurationManager.AppSettings["BackupPath"];
                if (userName.Equals("unknown") && File.Exists($"{backupPath}User.txt"))
                {
                    {
                        using (StreamReader reader = new StreamReader($"{backupPath}User.txt", Encoding.Default))
                        {
                            string name = reader.ReadLine();
                            if (!string.IsNullOrEmpty(name))
                            {
                                this.userName = name;
                            }
                        }
                    }
                }
                if (!userName.Equals("unknown"))
                {
                    if (!File.Exists($"{backupPath}User.txt"))
                    {
                        if (!Directory.Exists(backupPath))
                        {
                            Directory.CreateDirectory(backupPath);
                        }
                        using (File.Create($"{backupPath}User.txt")) { }
                        using (FileStream fs = new FileStream($"{backupPath}User.txt", FileMode.Create))
                        {
                            using (StreamWriter sw = new StreamWriter(fs))
                            {
                                sw.Write(this.userName);
                                sw.Flush();
                            }
                        }
                    }
                }
            }
            catch (ConfigurationErrorsException ex)
            {
                System.Diagnostics.Trace.WriteLine($"Ignore config read exception, check config file.ex:{ex.Message}");
            }
        }

        #region backend Settings
        private bool isCaptureEnable = false;

        public bool IsCaptureEnable
        {
            get { return isCaptureEnable; }
            set { isCaptureEnable = value; OnPropertyChanged(new PropertyChangedEventArgs("IsCaptureEnable")); }
        }

        private bool isSendThreadRun = true;

        public bool IsSendThreadRun
        {
            get { return isSendThreadRun; }
            set { isSendThreadRun = value; }
        }

        #endregion

        #region UI Settings
        private bool warningVisible = true;

        public bool WarningVisible
        {
            get { return warningVisible; }
            set { warningVisible = value; OnPropertyChanged(new PropertyChangedEventArgs("WarningVisible")); }
        }

        private bool succeccMessageVisible = false;

        public bool SucceccMessageVisible
        {
            get { return succeccMessageVisible; }
            set { succeccMessageVisible = value; OnPropertyChanged(new PropertyChangedEventArgs("SucceccMessageVisible")); }
        }
        #endregion

        #region setting in config file
        private string serverName;

        public string ServerName
        {
            get { return serverName; }
            set { serverName = value; OnPropertyChanged(new PropertyChangedEventArgs("ServerName")); }
        }

        private string userName;
        public string UserName
        {
            get { return userName; }
            set { userName = value; }
        }



        private bool isSaveLocalEnable;

        public bool IsSaveLocalEnable
        {
            get { return isSaveLocalEnable; }
            set { isSaveLocalEnable = value; }
        }

        private string saveLocalFolder;

        public string SaveLocalFolder
        {
            get { return saveLocalFolder; }
            set { saveLocalFolder = value; }
        }

        private bool isSendToServer;

        public bool IsSendToServer
        {
            get { return isSendToServer; }
            set { isSendToServer = value; }
        }

        private string localDataFolder;

        public string LocalDataFolder
        {
            get { return localDataFolder; }
            set { localDataFolder = value; }
        }
        #endregion
    }

    public class StartupConfigSection : ConfigurationSection
    {

        [System.Configuration.ConfigurationProperty("ServerName")]
        public ServerName ServerName
        {
            get
            {
                return ((ServerName)(base["ServerName"]));
            }
        }

        [System.Configuration.ConfigurationProperty("UserName")]
        public UserName UserName
        {
            get
            {
                return ((UserName)(base["UserName"]));
            }
        }



        [System.Configuration.ConfigurationProperty("SaveLocal")]
        public SaveLocal SaveLocal
        {
            get
            {
                return ((SaveLocal)(base["SaveLocal"]));
            }
        }

        [System.Configuration.ConfigurationProperty("SendToServer")]
        public SendToServer SendToServer
        {
            get
            {
                return ((SendToServer)(base["SendToServer"]));
            }
        }

    }

    public class SaveLocal : ConfigurationElement
    {


        [ConfigurationProperty("enable", IsRequired = true)]
        public bool Enable
        {
            get
            {
                return ((bool)(base["enable"]));
            }

            set
            {
                base["enable"] = value;
            }
        }


        [ConfigurationProperty("folder", IsRequired = true)]
        public string Folder
        {
            get
            {
                return ((string)(base["folder"]));
            }

            set
            {
                base["folder"] = value;
            }
        }
    }

    public class ServerName : ConfigurationElement
    {


        [ConfigurationProperty("value", IsRequired = true)]
        public string Value
        {
            get
            {
                return ((string)(base["value"]));
            }

            set
            {
                base["value"] = value;
            }
        }
    }

    public class UserName : ConfigurationElement
    {


        [ConfigurationProperty("value", IsRequired = true)]
        public string Value
        {
            get
            {
                return ((string)(base["value"]));
            }

            set
            {
                base["value"] = value;
            }
        }
    }



    public class SendToServer : ConfigurationElement
    {
        [ConfigurationProperty("enable", IsRequired = true)]
        public bool Enable
        {
            get
            {
                return ((bool)(base["enable"]));
            }

            set
            {
                base["enable"] = value;
            }
        }


        [ConfigurationProperty("localdatafolder")]
        public string LocalDataFolder
        {
            get
            {
                return ((string)(base["localdatafolder"]));
            }

            set
            {
                base["localdatafolder"] = value;
            }
        }
    }
}
