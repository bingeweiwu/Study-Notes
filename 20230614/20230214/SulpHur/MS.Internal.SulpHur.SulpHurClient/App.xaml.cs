using System;
using System.Configuration;
using System.Windows;
using System.Diagnostics;
using System.Threading;
using MS.Internal.SulpHur.Utilities;
using MS.Internal.SulpHur.Utilities.Exceptions;
using MS.Internal.SulpHur.SulpHurClient.ServerContact;
using System.Reflection;
using MS.Internal.SulpHur.SulpHurClient.Monitors;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using MS.Internal.SulpHur.SulpHurClient.GenerateClass;
using MS.Internal.SulpHur.SulpHurClient.Common;

namespace MS.Internal.SulpHur.SulpHurClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        //UserName
        private string userName = string.Empty;
        public string UserName
        {
            get
            {
                return userName;
            }
        }
        //ServerName
        private string serverName = string.Empty;
        public string ServerName
        {
            get
            {
                return serverName;
            }
        }
        //BuildNo
        private string buildNo = string.Empty;
        public string BuildNo
        {
            get
            {
                return buildNo;
            }
        }
        //BuildLanguage
        private string buildLanguage = string.Empty;
        public string BuildLanguage
        {
            get
            {
                return buildLanguage;
            }
        }

        //IsLocalRun
        private bool isLocalRun = true;
        public bool IsLocalRun
        {
            get
            {
                return isLocalRun;
            }
        }

        //TrayIcon
        private System.Windows.Forms.NotifyIcon notifyIcon;
        public System.Windows.Forms.NotifyIcon NotifyIcon
        {
            get
            {
                return notifyIcon;
            }
        }
        //SettingView
        private SettingsView settingView;
        public SettingsView SettingView
        {
            get
            {
                return settingView;
            }
        }

        #region v-jani: support IntlDevHub tool
        private string buildType = "main";
        public string BuildType
        {
            get { return buildType; }
            set { buildType = value; }
        }

        private string mode = "automation";
        public string Mode
        {
            get { return mode; }
            set { mode = value; }
        }
        #endregion

        public App()
        {
            //register app domain unhandled exception handler
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        public void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }
            Trace.WriteLine(string.Format("Unhandled Exception: {0}", ex.ToString()));
            //      MessageBox.Show(ex.ToString(), "Unhandled Exception");
        }
        /// <summary>
        /// when the flag set to true,ctrl+shift+d will save the capture to local file
        /// </summary>
        public static bool UIAutoFlag = false;

        /// <summary>
        /// this represent if the mode was setted to manual mode
        /// </summary>
        private static bool ManualFlag = false;

        /// <summary>
        /// when the flag set to true,the ctrl + shift + D can capture the Main Interface of adminconsole
        /// </summary>
        public static bool UIAutoMainInterfaceCaptureFlag = false;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Trace.WriteLine($"Current version: {Tool.LocVersion()}");

            this.settingView = new SettingsView();

            string abandonDataFolder = ConfigurationManager.AppSettings["AbandonDataFolder"];

            if (!Directory.Exists(abandonDataFolder))
            {
                Directory.CreateDirectory(abandonDataFolder);
            }

            //retrieve arguments
            if (!this.ParseArguments(e))
            {
                //exit
                this.Shutdown();
                return;
            }


            //tray icon
            this.notifyIcon = new System.Windows.Forms.NotifyIcon();
            this.notifyIcon.Icon = MS.Internal.SulpHur.SulpHurClient.Properties.Resources.icon;
            //to workaround tray icon text 64 characters limitation
            this.SetNotifyIconText(this.notifyIcon, MS.Internal.SulpHur.SulpHurClient.Properties.Resources.AppName);
            this.notifyIcon.BalloonTipTitle = MS.Internal.SulpHur.SulpHurClient.Properties.Resources.AppName;
            this.notifyIcon.Visible = true;

            //keep singleton
            string processName = Process.GetCurrentProcess().ProcessName;
            if (Process.GetProcessesByName(processName).Length > 1)
            {
                foreach (Process p in Process.GetProcessesByName(processName))
                {
                    Trace.WriteLine(string.Format("p.Id: {0}", p.Id));
                }
                string tipTitle = MS.Internal.SulpHur.SulpHurClient.Properties.Resources.AppName;
                string tipText = MS.Internal.SulpHur.SulpHurClient.Properties.Resources.TrayMessage_AlreadyRun;
                this.notifyIcon.ShowBalloonTip(3000, tipTitle, tipText, System.Windows.Forms.ToolTipIcon.Info);
                Thread.Sleep(TimeSpan.FromSeconds(3));

                //exit
                this.Shutdown();
                return;
            }

            //build context menu
            //logFilePath
            string logFilePath = ((SulpHurTextWriterTraceListener)Trace.Listeners["clientListener"]).FileName;
            Trace.WriteLine(string.Format("logFilePath: {0}", logFilePath));
            //context menu
            # region context menu
            this.notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(
                new System.Windows.Forms.MenuItem[]
                {
                    //view report
                    new System.Windows.Forms.MenuItem(MS.Internal.SulpHur.SulpHurClient.Properties.Resources.TrayMenu_ViewReport,
                        delegate
                        {
                            //reportUrl
                            string reportUrl = string.Format("http://{0}/SulpHurReports/CapturedUIReport.aspx", ServerContact.ServerContacter.Instance.ServerName);
                            Trace.WriteLine(string.Format("View Report: {0}", reportUrl));
                            Process.Start(reportUrl);
                        })
                        {
                            Name = "ViewReport"
                        },
                        
                    //view log
                    new System.Windows.Forms.MenuItem(MS.Internal.SulpHur.SulpHurClient.Properties.Resources.TrayMenu_ViewLog,
                        delegate
                        {
                            Process.Start(logFilePath);
                        })
                        {
                            Name = "ViewLog"
                        },                    
                    
                    //---------------separator line----------------------
                    new System.Windows.Forms.MenuItem("-"),

                    //Reconnect service
                    new System.Windows.Forms.MenuItem(MS.Internal.SulpHur.SulpHurClient.Properties.Resources.TrayMenu_ConnectSulphur,
                        delegate
                        {
                            if (!ServerContacter.Instance.IsConnected)
                            {
                                ServerContacter.Instance.Connect(settingView.ServerName);
                            }else
                            {
                                string tipTitle = MS.Internal.SulpHur.SulpHurClient.Properties.Resources.AppName;
                                string tipText = "SulphurClient has already connect to sulphur service";
                                this.notifyIcon.ShowBalloonTip(3000, tipTitle, tipText, System.Windows.Forms.ToolTipIcon.Info);
                            }
                        })
                        {
                            Name = "Reconnect Service"
                        },

                    //upload local data
                    new System.Windows.Forms.MenuItem(MS.Internal.SulpHur.SulpHurClient.Properties.Resources.TrayMenu_UploadCachedData,
                        delegate
                        {
                            Trace.WriteLine("Upload cached data!");

                            if (!ServerContacter.Instance.IsConnected)
                            {
                                string message = "SulpHurClient has not connected to server!\nCannot do this action.";
                                System.Windows.MessageBox.Show(message, SulpHurClient.Properties.Resources.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            if(DataCenter.Instance.IsCacheDataExist)
                            {
                                //send local data
                                DataCenter.Instance.StartToLoadLocalData();

                                //tip text
                                string tipTitle = MS.Internal.SulpHur.SulpHurClient.Properties.Resources.AppName;
                                string tipText = "SulpHurClient has started to upload cached data!";
                                this.notifyIcon.ShowBalloonTip(3000, tipTitle, tipText, System.Windows.Forms.ToolTipIcon.Info);
                            }
                            else
                            {
                                //tip text
                                string tipTitle = MS.Internal.SulpHur.SulpHurClient.Properties.Resources.AppName;
                                string tipText = "No cached data exist!";
                                this.notifyIcon.ShowBalloonTip(3000, tipTitle, tipText, System.Windows.Forms.ToolTipIcon.Info);
                            }
                        })
                        {
                            Name = "UploadCachedData"
                        },

                    //capture foreground window
                    new System.Windows.Forms.MenuItem(MS.Internal.SulpHur.SulpHurClient.Properties.Resources.TrayMenu_CaptureForegroundWnd,
                        delegate
                        {
                            Trace.WriteLine("Capture Foreground Window!");
                            //wait 3 second to set foreground window
                            System.Threading.Thread.Sleep(3000);

                            Engine.Instance.CaptureUI();
                            //tip text
                            string tipTitle = MS.Internal.SulpHur.SulpHurClient.Properties.Resources.AppName;
                            string tipText = "Capturing is completed!";
                            this.notifyIcon.ShowBalloonTip(3000, tipTitle, tipText, System.Windows.Forms.ToolTipIcon.Info);
                        }, System.Windows.Forms.Shortcut.CtrlShiftD)
                        {
                            Name = "CaptureForegroundWindow"
                        },

                    //---------------separator line----------------------
                    new System.Windows.Forms.MenuItem("-"),

                    //mode (automation, manual,ui-auto)
                    #region mode
                    new System.Windows.Forms.MenuItem(MS.Internal.SulpHur.SulpHurClient.Properties.Resources.TrayMenu_Mode,
                            new System.Windows.Forms.MenuItem[]
                            {
                                //Automation mode
                                new System.Windows.Forms.MenuItem(SulpHurClient.Properties.Resources.TraySubMenu_Automation,
                                    new EventHandler((senderArg, eArg) =>
                                    {
                                        System.Windows.Forms.MenuItem currentMenuItem = senderArg as System.Windows.Forms.MenuItem;
                                        //no action if it is already checked
                                        if(currentMenuItem.Checked)
                                        {
                                            return;
                                        }

                                        Trace.WriteLine("SET MODE: Automation!");
                                        ManualFlag = false;
                                        //uncheck all the sibling items
                                        foreach (System.Windows.Forms.MenuItem subMenuItem in currentMenuItem.Parent.MenuItems)
                                        {
                                            if(subMenuItem == currentMenuItem)
                                            {
                                                //check the selected item
                                                currentMenuItem.Checked = true;
                                            }
                                            else
                                            {
                                                //uncheck all others item
                                                subMenuItem.Checked = false;
                                                UIAutoFlag = false;
                                            }
                                        }

                                        //disable capture state manager
                                        CaptureStateManager.Instance.Disable();
                                        
                                        //v-yiwzha: disable upload state manager
                                        UploadStateManager.Instance.Disable();
                                        UploadStateManager.Instance.Dispose();

                                        //tip text
                                        string tipTitle = MS.Internal.SulpHur.SulpHurClient.Properties.Resources.AppName;
                                        string tipText = "SulpHurClient has been set as Automation mode!";
                                        this.notifyIcon.ShowBalloonTip(3000, tipTitle, tipText, System.Windows.Forms.ToolTipIcon.Info);
                                    }))
                                    {
                                        Name = "Automation",
                                        RadioCheck = true,
                                        Checked =  mode.ToLower() == "manual" ? false : true
                                    },
                                //Manual mode
                                new System.Windows.Forms.MenuItem(SulpHurClient.Properties.Resources.TraySubMenu_Manual,
                                    new EventHandler((senderArg, eArg) =>
                                    {
                                        System.Windows.Forms.MenuItem currentMenuItem = senderArg as System.Windows.Forms.MenuItem;
                                        //no action if it is already checked
                                        if(currentMenuItem.Checked)
                                        {
                                            return;
                                        }

                                        Trace.WriteLine("SET MODE: Manual!");
                                        ManualFlag = true;

                                        //uncheck all the sibling items
                                        foreach (System.Windows.Forms.MenuItem subMenuItem in currentMenuItem.Parent.MenuItems)
                                        {
                                            if(subMenuItem == currentMenuItem)
                                            {
                                                //check the selected item
                                                currentMenuItem.Checked = true;
                                            }
                                            else
                                            {
                                                //uncheck all others item
                                                subMenuItem.Checked = false;
                                                UIAutoFlag = false;
                                            }
                                        }

                                        //enable capture state manager
                                        CaptureStateManager.Instance.Enable();

                                        //v-yiwzha: enable upload state manager
                                        UploadStateManager.Instance.Enable();
                                        UploadStateManager.Instance.UpdateState(UploadState.Abandon, null);

                                        //tip text
                                        string tipTitle = MS.Internal.SulpHur.SulpHurClient.Properties.Resources.AppName;
                                        string tipText = "SulpHurClient has been set as Manual mode!";
                                        this.notifyIcon.ShowBalloonTip(3000, tipTitle, tipText, System.Windows.Forms.ToolTipIcon.Info);
                                    }))
                                    {
                                        Name = "Manual",
                                        RadioCheck = true,
                                        Checked = mode.ToLower() == "manual" ? true : false
                                    },
                                //UI-Auto mode
                                new System.Windows.Forms.MenuItem(SulpHurClient.Properties.Resources.TraySubMenu_UIAuto,
                                    new EventHandler((senderArg, eArg) =>
                                    {
                                        System.Windows.Forms.MenuItem currentMenuItem = senderArg as System.Windows.Forms.MenuItem;
                                        //no action if it is already checked
                                        if(currentMenuItem.Checked)
                                        {
                                            return;
                                        }

                                        UIAutoFlag = true;
                                        ManualFlag = true;

                                        Trace.WriteLine("SET MODE: UI-Auto!");
                                        //uncheck all the sibling items
                                        foreach (System.Windows.Forms.MenuItem subMenuItem in currentMenuItem.Parent.MenuItems)
                                        {
                                            if(subMenuItem == currentMenuItem)
                                            {
                                                //check the selected item
                                                currentMenuItem.Checked = true;
                                            }
                                            else
                                            {
                                                //uncheck all others item
                                                subMenuItem.Checked = false;
                                            }
                                        }

                                        //enable capture state manager
                                        CaptureStateManager.Instance.Enable();

                                        //v-yiwzha: enable upload state manager
                                        UploadStateManager.Instance.Enable();
                                        UploadStateManager.Instance.UpdateState(UploadState.Abandon, null);

                                        //tip text
                                        string tipTitle = MS.Internal.SulpHur.SulpHurClient.Properties.Resources.AppName;
                                        string tipText = "SulpHurClient has been set as UI-Auto mode!";
                                        this.notifyIcon.ShowBalloonTip(3000, tipTitle, tipText, System.Windows.Forms.ToolTipIcon.Info);
                                    }))
                                    {
                                        Name = "UI-Auto",
                                        RadioCheck = true,
                                        //Checked = mode.ToLower() == "manual" ? false : true
                                        //Checked = false
                                    },
                                //UI-Auto generate class
                                new System.Windows.Forms.MenuItem(SulpHurClient.Properties.Resources.TraySubMenu_GenerateClass,
                                    new EventHandler((senderArg, eArg) =>
                                    {
                                        Trace.WriteLine("Generate Class!");
                                        string Path = @"C:\SulpHurClient\UIAutoData";

                                        Generate(Path);

                                        //tip text
                                        string tipTitle = MS.Internal.SulpHur.SulpHurClient.Properties.Resources.AppName;
                                        string tipText = "Generate Class file to C:\\SulpHurClient\\UIAutoData!";
                                        this.notifyIcon.ShowBalloonTip(3000, tipTitle, tipText, System.Windows.Forms.ToolTipIcon.Info);
                                    }))
                                    {
                                        Name = "GenerateClase",
                                        RadioCheck = true,
                                    }
                            })
                            {
                                Name = "Mode"
                            },
                        #endregion

                    //turn on/off
                    #region turn on/off
                    new System.Windows.Forms.MenuItem(MS.Internal.SulpHur.SulpHurClient.Properties.Resources.TrayMenu_TurnOnOff,
                            new System.Windows.Forms.MenuItem[]
                            {
                                //turn on sub menu
                                new System.Windows.Forms.MenuItem(SulpHurClient.Properties.Resources.TraySubMenu_TurnOn,
                                    new EventHandler((senderArg, eArg) =>
                                    {
                                        System.Windows.Forms.MenuItem currentMenuItem = senderArg as System.Windows.Forms.MenuItem;
                                        //no action if it is already checked
                                        if(currentMenuItem.Checked)
                                        {
                                            return;
                                        }

                                        Trace.WriteLine("Turn on SulpHurClient!");
                                        //uncheck all the sibling items
                                        foreach (System.Windows.Forms.MenuItem subMenuItem in currentMenuItem.Parent.MenuItems)
                                        {
                                            if(subMenuItem == currentMenuItem)
                                            {
                                                //check the selected item
                                                currentMenuItem.Checked = true;
                                            }
                                            else
                                            {
                                                //uncheck all others item
                                                subMenuItem.Checked = false;
                                            }
                                        }

                                        //start engine
                                        Engine.Instance.Start();

                                        //tip text
                                        string tipTitle = MS.Internal.SulpHur.SulpHurClient.Properties.Resources.AppName;
                                        string tipText = "SulpHurClient has been turned On!";
                                        this.notifyIcon.ShowBalloonTip(3000, tipTitle, tipText, System.Windows.Forms.ToolTipIcon.Info);
                                    }))
                                    {
                                        Name = "TurnOn",
                                        RadioCheck = true,
                                        Checked = true
                                    },
                                //turn off sub menu
                                new System.Windows.Forms.MenuItem(SulpHurClient.Properties.Resources.TraySubMenu_TurnOff,
                                    new EventHandler((senderArg, eArg) =>
                                    {
                                        System.Windows.Forms.MenuItem currentMenuItem = senderArg as System.Windows.Forms.MenuItem;
                                        //no action if it is already checked
                                        if(currentMenuItem.Checked)
                                        {
                                            return;
                                        }

                                        Trace.WriteLine("Turn off SulpHurClient!");
                                        //uncheck all the sibling items
                                        foreach (System.Windows.Forms.MenuItem subMenuItem in currentMenuItem.Parent.MenuItems)
                                        {
                                            if(subMenuItem == currentMenuItem)
                                            {
                                                //check the selected item
                                                currentMenuItem.Checked = true;
                                            }
                                            else
                                            {
                                                //uncheck all others item
                                                subMenuItem.Checked = false;
                                            }
                                        }

                                        //stop engine
                                        Engine.Instance.Stop();

                                        //tip text
                                        string tipTitle = MS.Internal.SulpHur.SulpHurClient.Properties.Resources.AppName;
                                        string tipText = "SulpHurClient has been turned Off!";
                                        this.notifyIcon.ShowBalloonTip(3000, tipTitle, tipText, System.Windows.Forms.ToolTipIcon.Info);
                                    }))
                                    {
                                        Name = "TurnOff",
                                        RadioCheck = true
                                    },
                            })
                            {
                                Name = "TurnOnOff"
                            },
                    #endregion

                    //Build Type 
                    #region Private/Main
                    new System.Windows.Forms.MenuItem(MS.Internal.SulpHur.SulpHurClient.Properties.Resources.TrayMenu_BuildType,
                            new System.Windows.Forms.MenuItem[]
                            {
                                //Main sub menu
                                new System.Windows.Forms.MenuItem(SulpHurClient.Properties.Resources.TraySubMenu_Main,
                                    new EventHandler((senderArg, eArg) =>
                                    {
                                        System.Windows.Forms.MenuItem currentMenuItem = senderArg as System.Windows.Forms.MenuItem;
                                        //no action if it is already checked
                                        if(currentMenuItem.Checked)
                                        {
                                            return;
                                        }

                                        Trace.WriteLine("Main Build is selected!");
                                         //uncheck all the sibling items
                                        foreach (System.Windows.Forms.MenuItem subMenuItem in currentMenuItem.Parent.MenuItems)
                                        {
                                            if(subMenuItem == currentMenuItem)
                                            {
                                                //check the selected item
                                                currentMenuItem.Checked = true;
                                            }
                                            else
                                            {
                                                //uncheck all others item
                                                subMenuItem.Checked = false;
                                            }
                                        }

                                        ServerContacter.Instance.additionInformation.BuildType = SulpHurClient.Properties.Resources.TraySubMenu_Main;
                                        //tip text
                                        string tipTitle = MS.Internal.SulpHur.SulpHurClient.Properties.Resources.AppName;
                                        string tipText = "Main Build Type is selected!";
                                        this.notifyIcon.ShowBalloonTip(3000, tipTitle, tipText, System.Windows.Forms.ToolTipIcon.Info);
                                    }))
                                    {
                                        Name = "Main",
                                        RadioCheck = true,
                                        Checked = buildType.ToLower() == "private" ? false : true
                                    },
                                //Private sub menu
                                new System.Windows.Forms.MenuItem(SulpHurClient.Properties.Resources.TraySubMenu_Private ,
                                    new EventHandler((senderArg, eArg) =>
                                    {
                                        System.Windows.Forms.MenuItem currentMenuItem = senderArg as System.Windows.Forms.MenuItem;
                                        //no action if it is already checked
                                        if(currentMenuItem.Checked)
                                        {
                                            return;
                                        }

                                        Trace.WriteLine("Private Build is selected!");
                                        //uncheck all the sibling items
                                        foreach (System.Windows.Forms.MenuItem subMenuItem in currentMenuItem.Parent.MenuItems)
                                        {
                                            if(subMenuItem == currentMenuItem)
                                            {
                                                //check the selected item
                                                currentMenuItem.Checked = true;
                                            }
                                            else
                                            {
                                                //uncheck all others item
                                                subMenuItem.Checked = false;
                                            }
                                        }
                                        ServerContacter.Instance.additionInformation.BuildType = SulpHurClient.Properties.Resources.TraySubMenu_Private;
                                        //tip text
                                        string tipTitle = MS.Internal.SulpHur.SulpHurClient.Properties.Resources.AppName;
                                        string tipText = "Private Build Type is selected!";
                                        this.notifyIcon.ShowBalloonTip(3000, tipTitle, tipText, System.Windows.Forms.ToolTipIcon.Info);
                                    }))
                                    {
                                        Name = "Private",
                                        RadioCheck = true,
                                        Checked = buildType.ToLower() == "private" ? true : false
                                    },
                            })
                            {
                                Name = "BuildType"
                            },
                    #endregion

                    //close
                    new System.Windows.Forms.MenuItem(MS.Internal.SulpHur.SulpHurClient.Properties.Resources.TrayMenu_Close,
                        delegate
                        {
                            //warning user if there is page in processing
                            if(DataCenter.Instance.PageDataQueue.Count > 0)
                            {
                                string message = string.Format("There are {0} pages in processing, do you really want to close SulpHurClient?", DataCenter.Instance.PageDataQueue.Count);
                                MessageBoxResult result = System.Windows.MessageBox.Show(message, SulpHurClient.Properties.Resources.AppName, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                                if (result.Equals(MessageBoxResult.No) || result.Equals(MessageBoxResult.None))
                                {
                                    return;
                                }
                            }

                            //warning for cached data
                            if(DataCenter.Instance.CachedPageCount > 0)
                            {
                                if(ServerContacter.Instance.IsConnected)
                                {
                                    string message = string.Format("There are {0} pages cached locally, do you want to upload them right now?", DataCenter.Instance.CachedPageCount);
                                    MessageBoxResult result = System.Windows.MessageBox.Show(message, SulpHurClient.Properties.Resources.AppName, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                                    if (result.Equals(MessageBoxResult.Yes))
                                    {
                                        DataCenter.Instance.StartToLoadLocalData();
                                        return;
                                    }
                                }
                                else
                                {
                                    string message = string.Format("There are {0} pages cached locally, remember to save and upload them.", DataCenter.Instance.CachedPageCount);
                                    System.Windows.MessageBox.Show(message, SulpHurClient.Properties.Resources.AppName, MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                            }

                            //exit
                            this.Shutdown();
                             return;


                        })
                        {
                            Name = "Close"
                        },
                    //Upgrade
                    new System.Windows.Forms.MenuItem("Upgrade",
                        delegate
                        {
                            if (Tool.UpdateState())
                            {
                                Trace.WriteLine("Upgrade!!!");
                                Tool.RunBat(@"C:\SulpHurClient\bin\ForceUpgrade.bat");
                            }else
                            {
                                string tipTitle = MS.Internal.SulpHur.SulpHurClient.Properties.Resources.AppName;
                                string tipText = "Already the latest version!!!";
                                this.notifyIcon.ShowBalloonTip(3000, tipTitle, tipText, System.Windows.Forms.ToolTipIcon.Info);
                                return;
                            }
                        })
                        {
                            Name = "Upgrade"
                        },
                });
            #endregion

            //start main handler
            if (!this.StartMain())
            {
                //exit
                this.Shutdown();
                return;
            }

            //Tip Set alias
            if (settingView.UserName.Equals("unknown"))
            {
                Trace.WriteLine("==============================Alias is unknown=====================================================");

                string tipTitle = MS.Internal.SulpHur.SulpHurClient.Properties.Resources.AppName;
                string tipText = "Alias is unknown,If you extend to change the alias in config file, exit the Sulphur, modify the config file and restart it!";
                this.notifyIcon.ShowBalloonTip(5000, tipTitle, tipText, System.Windows.Forms.ToolTipIcon.Info);
            }

            try
            {
                // reg hotkey
                // Ctrl+shift+D
                HotKeyManager.Instance.RegisterHotKey(ModifierKeys.Control | ModifierKeys.Shift, System.Windows.Forms.Keys.D, delegate
                {
                    Trace.WriteLine("HotKey(Ctrl+Shift+D) for capturing foreground window is triggered!");

                    //fix bug 6939531 [UI Auto]Sulphur can't take another screenshot after deleting the XML file
                    if (UIAutoFlag)
                    {
                        DataCenter.guidDic.Clear();
                    }

                    // capture
                    Engine.Instance.CaptureUI();
                    // tip text
                    string tipTitle = MS.Internal.SulpHur.SulpHurClient.Properties.Resources.AppName;
                    string tipText = "Capturing is completed!";
                    this.notifyIcon.ShowBalloonTip(3000, tipTitle, tipText, System.Windows.Forms.ToolTipIcon.Info);
                });
            }
            catch (Exception ex)
            {
                Trace.WriteLine("HotKey(Ctrl+Shift+D) for capturing foreground window cannot be triggered!");
                Trace.WriteLine(ex.Message);
                this.Shutdown();
                return;
            }


            //uiauto capture the adminconsole Main interface
            try
            {
                // reg hotkey
                // Ctrl+shift+U
                HotKeyManager.Instance.RegisterHotKey(ModifierKeys.Control | ModifierKeys.Shift, System.Windows.Forms.Keys.U, delegate
                {

                    if (UIAutoMainInterfaceCaptureFlag && UIAutoFlag)
                    {
                        UIAutoMainInterfaceCaptureFlag = false;
                        Trace.WriteLine("UI-Auto capturing main interface of adminconsole feature is shutdown!!!");
                        string tipTitle = MS.Internal.SulpHur.SulpHurClient.Properties.Resources.AppName;
                        string tipText = "UI-Auto capturing main interface of adminconsole feature is shutdown!!!";
                        this.notifyIcon.ShowBalloonTip(3000, tipTitle, tipText, System.Windows.Forms.ToolTipIcon.Info);
                    }
                    else if (!UIAutoMainInterfaceCaptureFlag && UIAutoFlag)
                    {
                        UIAutoMainInterfaceCaptureFlag = true;
                        Trace.WriteLine("UI-Auto capturing main interface of adminconsole feature is setted!!!");
                        string tipTitle = MS.Internal.SulpHur.SulpHurClient.Properties.Resources.AppName;
                        string tipText = "UI-Auto capturing main interface of adminconsole feature is setted!!!";
                        this.notifyIcon.ShowBalloonTip(3000, tipTitle, tipText, System.Windows.Forms.ToolTipIcon.Info);
                    }
                });
            }
            catch (Exception ex)
            {
                Trace.WriteLine("HotKey(Ctrl+Shift+U) for capturing foreground window cannot be triggered!");
                Trace.WriteLine(ex.Message);
                this.Shutdown();
                return;
            }

            //start log monitor
            try
            {
                Thread logMonitor = new Thread(new ThreadStart(MonitroFile));
                logMonitor.IsBackground = true;
                logMonitor.Start();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"ConsoleDirectory not exist,exp:{ex.Message}");
            }
            try
            {
                Tool.RunBat("AccessServer.bat");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"运行AccessServer.bat失败:{ex.Message}");
            }
            TimedTasks.RunTasks();
        }

        public void Generate(string Path)
        {
            DirectoryInfo dir = new DirectoryInfo(Path);
            foreach (FileInfo file in dir.GetFiles("*.xml"))
            {
                //Nodelist=NodeList(file.FullName, "AutomationId");
                XmlDocument xml = new XmlDocument();
                xml.Load(file.FullName);

                XmlNode root = xml.SelectSingleNode("WindowPageInfo/ei");
                if (root == null)
                {
                    root = xml.SelectSingleNode("ElementInformation");

                }
                if (root == null)
                {
                    root = xml.SelectSingleNode("ei");
                }
                if (root == null)
                {
                    Trace.WriteLine("Failed to generate class for file:" + file.Name);
                    continue;
                }
                //XmlNode root = xml.FirstChild;
                XmlNode autoroot = CreateClass.getRootIDNode(root);
                if (autoroot != null)
                {
                    XmlNode AutomationId = root.SelectSingleNode("AutomationId");
                    if (AutomationId == null)
                    {
                        Trace.WriteLine("Failed to generate class for file:" + file.Name);
                        continue;
                    }
                    if (AutomationId.InnerText == "SmsWizardForm")
                    {
                        XmlNode wizard = CreateClass.findNodeByAutomationId(root, "WizardPage");
                        if (wizard == null)
                        {
                            Trace.WriteLine("Failed to generate class for file:" + file.Name);
                            continue;
                        }
                        XmlNode wizardChild = CreateClass.findChildrensElement(wizard)[0];
                        if (wizardChild == null)
                        {
                            Trace.WriteLine("Failed to generate class for file:" + file.Name);
                            continue;
                        }
                        CreateClass.CreateClassByNode(wizardChild, "AutomationId");
                    }
                    else if (AutomationId.InnerText == "SheetFramework")
                    {
                        XmlNode tabPage = CreateClass.findNodeByAutomationId(root, "tabPages");
                        //XmlNode tabPageChild = CreateClass.findChildrensElement(CreateClass.findChildrensElement(CreateClass.findChildrensElement(tabPage)[0])[0])[0];
                        //CreateClass.CreateClassByNode(tabPageChild, "AutomationId");
                        if (tabPage == null)
                        {
                            Trace.WriteLine("Failed to generate class for file:" + file.Name);
                            continue;
                        }
                        XmlNode childNode1 = CreateClass.findChildrensElement(tabPage)[0];
                        if (childNode1 == null)
                        {
                            Trace.WriteLine("Failed to generate class for file:" + file.Name);
                            continue;
                        }
                        XmlNode childNode2 = CreateClass.findChildrensElement(childNode1)[0];
                        if (childNode2 == null)
                        {
                            Trace.WriteLine("Failed to generate class for file:" + file.Name);
                            continue;
                        }
                        XmlNode childNode3 = CreateClass.findChildrensElement(childNode2)[0];
                        if (childNode3 == null)
                        {
                            Trace.WriteLine("Failed to generate class for file:" + file.Name);
                            continue;
                        }
                        CreateClass.CreateClassByNode(childNode3, "AutomationId");
                    }
                    //else if (CreateClass.findNodeByAutomationId(root, "ContentPanel") != null)
                    //{
                    //    XmlNode ContentPanel = CreateClass.findNodeByAutomationId(root, "ContentPanel");
                    //    XmlNode Node = CreateClass.findChildrensElement(CreateClass.findChildrensElement(ContentPanel)[0])[0];
                    //    CreateClass.CreateClassByNode(Node, "AutomationId");
                    //}
                    //else
                    //{
                    //    CreateClass.NormalForm(root);
                    //}
                    else
                    {
                        XmlNode ContentPanel = CreateClass.findNodeByAutomationId(root, "ContentPanel");
                        if (ContentPanel != null)
                        {
                            XmlNode ChildNode = CreateClass.findChildrensElement(ContentPanel)[0];
                            if (ChildNode == null)
                            {
                                Trace.WriteLine("Failed to generate class for file:" + file.Name);
                                continue;
                            }
                            XmlNode Node = CreateClass.findChildrensElement(ChildNode)[0];
                            if (ChildNode == null)
                            {
                                Trace.WriteLine("Failed to generate class for file:" + file.Name);
                                continue;
                            }
                            CreateClass.CreateClassByNode(Node, "AutomationId");
                        }
                        else
                        {
                            CreateClass.NormalForm(root);
                        }
                    }
                }
            }
        }

        public void MonitroFile()
        {
            if (Utility.ConsoleDirectory != null)
            {
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(Utility.ConsoleDirectory);

                string path = System.IO.Path.Combine(di.Parent.FullName, "AdminUILog", "SmsAdminUI.log");
                if (File.Exists(path))
                {
                    AdminUILogMonitor.MonitorFile(path);
                }
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {

            #region dispose all singleton instances
            // unreg hotkey
            HotKeyManager.Instance.Dispose();
            // state manager
            CaptureStateManager.Instance.Dispose();
            ForegroundMonitor.Instance.Dispose();
            MoveSizeMonitor.Instance.Dispose();
            MessageLooper.Instance.Dispose();
            #endregion

            //stop engine

            if ((Engine.Instance != null) && (Engine.Instance.IsStarted))
            {
                Engine.Instance.Stop();
            }

            //destory tray icon
            if (this.notifyIcon != null)
            {
                this.notifyIcon.Dispose();
                this.notifyIcon = null;
            }

            //this line is for ClickOnce Deployment
            //clear Online App cache to make sure that user can load the latest version from server each time

            if (!this.IsLocalRun)
                this.ClearOnlineAppCache();
            //Work around for client crash
            //Trace.WriteLine("Kill The Process");
            Process.GetCurrentProcess().Kill();

        }

        private bool ParseArguments(StartupEventArgs e)
        {
            string arguamentUsage = "Supported Arguments:\nUserName\nServerName\nBuildNo\nBuildLanguage\nIsLocalRun\nBuildType\nMode\n\nExample: \"SulpHurClient ServerName=127.0.0.1\"";

            if (e.Args != null && e.Args.Length > 0)
            {
                try
                {
                    //string serverName = e.Args[0];
                    foreach (string arg in e.Args)
                    {

                        string[] splitArr = arg.Split('=');
                        string argName = splitArr[0];
                        string argValue = splitArr[1];

                        switch (argName.ToLower())
                        {
                            case "username":
                                this.userName = argValue;
                                break;
                            case "servername":
                                this.serverName = argValue;
                                break;
                            case "buildno":
                                Version version = null;
                                if (!Version.TryParse(argValue, out version))
                                {
                                    throw new SulpHurClientGeneralException("BuildNo is invalid.");
                                }
                                this.buildNo = argValue;
                                break;
                            case "buildlanguage":
                                this.buildLanguage = argValue;
                                break;
                            case "islocalrun":
                                if (argValue.ToLower().Trim() == "false")
                                    this.isLocalRun = false;
                                break;
                            case "buildtype":
                                this.buildType = argValue;
                                break;
                            case "mode":
                                this.mode = argValue;
                                break;
                            default:
                                throw new SulpHurClientGeneralException(string.Format("{0} is not valid argument.", argName));
                        }

                    }
                }
                catch (SulpHurClientGeneralException ex)
                {
                    Trace.WriteLine(ex);
                    string message = string.Format("{0}.\n\n{1}", ex.Message, arguamentUsage);
                    Trace.WriteLine(message);
                    //System.Windows.MessageBox.Show(string.Format("{0}.\n\n{1}", ex.Message, arguamentUsage), SulpHurClient.Properties.Resources.AppName);

                    return false;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                    string message = string.Format("Invalid argument.\n\n{0}", arguamentUsage);
                    Trace.WriteLine(message);
                    //System.Windows.MessageBox.Show(string.Format("Invalid argument.\n\n{0}", arguamentUsage), SulpHurClient.Properties.Resources.AppName);

                    return false;
                }
            }

            return true;
        }
        private void ClearOnlineAppCache()
        {
            string cmd = "rundll32 dfshim CleanOnlineAppCache";
            System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + cmd);
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = true;

            Process.Start(procStartInfo);
            Trace.WriteLine(string.Format("ClearOnlineAppCache: {0}", procStartInfo.Arguments));
        }

        private bool StartMain()
        {
            //load data
            //if (!this.LoadData())
            //{
            //    return false;
            //}
            this.LoadData();

            //connect to server
            this.ConnectServer();

            //launch process engine
            Engine.Instance.Start();

            return true;
        }
        private bool LoadData()
        {
            //return if no product installed
            if (!Utility.IsProductInstalled)
            {
                Trace.WriteLine("Admin console is not installed on this machine!");
                if (ManualFlag || UIAutoFlag)
                {
                    string tipTitle = MS.Internal.SulpHur.SulpHurClient.Properties.Resources.AppName;
                    string tipText = "Admin console is not installed on this machine!";
                    this.notifyIcon.ShowBalloonTip(5000, tipTitle, tipText, System.Windows.Forms.ToolTipIcon.Info);
                }
                Utility.OSLan();
            }
            ////product language pack
            //if (!Utility.IsProductLanguagePackInstalled)
            //{
            //    //MessageBoxResult result = MessageBox.Show("Product language pack is not installed!\nDo you want to close SulpHurClient to install language pack?",
            //    //    SulpHurClient.Properties.Resources.AppName,
            //    //    MessageBoxButton.YesNo,
            //    //    MessageBoxImage.Warning);
            //    //if (MessageBoxResult.Yes == result)
            //    //{
            //    //    return false;
            //    //}

            //    return false;
            //}


            //overwrite settings with app arguments
            if (!string.IsNullOrEmpty(this.serverName))
                this.settingView.ServerName = this.serverName;

            this.CollectAdditionData();

            return true;
        }

        //v-danpgu: call this method the get the proper build language every time when capture UI.
        //public void GetBuildLan()
        //{
        //    ServerContacter.Instance.additionInformation.ProductLanguage = Utility.GetPID();
        //    Trace.WriteLine(string.Format("ServerContacter.additionInformation.ProductLanguage:{0}", ServerContacter.Instance.additionInformation.ProductLanguage));
        //}
        private void CollectAdditionData()
        {
            ServerContacter.Instance.additionInformation.Alias = string.Format(@"{0}\{1}", Environment.UserDomainName, Environment.UserName);
            ServerContacter.Instance.additionInformation.ComputerName = Environment.MachineName;
            ServerContacter.Instance.additionInformation.OSType = CommonUtility.GetOSType();
            ServerContacter.Instance.additionInformation.OSLanguage = CommonUtility.GetOSLanguage();
            ServerContacter.Instance.additionInformation.IP = CommonUtility.GetLocalIP();
            ServerContacter.Instance.additionInformation.MacAddress = CommonUtility.GetLocalMac();

            //if adminconsole not installed,the ProductVersion and ProductLanguage will be null,need to be attention;
            ServerContacter.Instance.additionInformation.ProductVersion = new Version(Utility.ConsoleVersion);

            //v-danpgu: Using Get PID to solve the build language improper issue when launching UI with LaunchSCCM tool.
            ServerContacter.Instance.additionInformation.ProductLanguage = Utility.GetPID();
            ServerContacter.Instance.additionInformation.BuildType = SulpHurClient.Properties.Resources.TraySubMenu_Main;

            //overwrite with app arguments
            if (!string.IsNullOrEmpty(this.userName))
                ServerContacter.Instance.additionInformation.Alias = this.userName;
            else if (!string.IsNullOrEmpty(this.SettingView.UserName))
                ServerContacter.Instance.additionInformation.Alias = this.SettingView.UserName;
            if (!string.IsNullOrEmpty(this.buildNo))
                ServerContacter.Instance.additionInformation.ProductVersion = new Version(this.buildNo);
            if (!string.IsNullOrEmpty(this.buildLanguage))
                ServerContacter.Instance.additionInformation.ProductLanguage = this.buildLanguage;

            //write log
            Trace.WriteLine(string.Format("ServerContacter.Instance.additionInformation.Alias:{0}", ServerContacter.Instance.additionInformation.Alias));
            Trace.WriteLine(string.Format("ServerContacter.additionInformation.ProductVersion:{0}", ServerContacter.Instance.additionInformation.ProductVersion));
            Trace.WriteLine(string.Format("ServerContacter.additionInformation.ProductLanguage:{0}", ServerContacter.Instance.additionInformation.ProductLanguage));
        }

        private void ConnectServer()
        {
            if (string.IsNullOrEmpty(this.settingView.ServerName))
                return;

            if (ServerContacter.Instance.Connect(this.settingView.ServerName))
            {
                if (UIAutoFlag || ManualFlag)
                {
                    //tray icon
                    App app = App.Current as App;
                    //to workaround tray icon text 64 characters limitation
                    this.SetNotifyIconText(app.NotifyIcon, string.Format(MS.Internal.SulpHur.SulpHurClient.Properties.Resources.TrayMessage_Connected, this.settingView.ServerName));
                    //tip text
                    string tipTitle = MS.Internal.SulpHur.SulpHurClient.Properties.Resources.AppName;
                    string tipText = string.Format(MS.Internal.SulpHur.SulpHurClient.Properties.Resources.TrayMessage_Connected, this.settingView.ServerName);
                    app.NotifyIcon.ShowBalloonTip(3000, tipTitle, tipText, System.Windows.Forms.ToolTipIcon.Info);

                    ServerContacter.keybd_event((byte)Keys.NumLock, 0, 0, 0);
                }

                //context menu items
                this.notifyIcon.ContextMenu.MenuItems.Find("UploadCachedData", false)[0].Enabled = true;
            }
            else
            {
                if (UIAutoFlag || ManualFlag)
                {
                    //tray icon
                    App app = App.Current as App;
                    //to workaround tray icon text 64 characters limitation
                    this.SetNotifyIconText(app.NotifyIcon, string.Format(MS.Internal.SulpHur.SulpHurClient.Properties.Resources.TrayMessage_ConnectFailed, this.settingView.ServerName));
                    //tip text
                    string tipTitle = MS.Internal.SulpHur.SulpHurClient.Properties.Resources.AppName;
                    string tipText = string.Format(MS.Internal.SulpHur.SulpHurClient.Properties.Resources.TrayMessage_ConnectFailed, this.settingView.ServerName);
                    app.NotifyIcon.ShowBalloonTip(3000, tipTitle, tipText, System.Windows.Forms.ToolTipIcon.Error);

                    ServerContacter.keybd_event((byte)Keys.NumLock, 0, 0, 0);
                }

                //context menu items
                this.notifyIcon.ContextMenu.MenuItems.Find("UploadCachedData", false)[0].Enabled = false;
            }
        }
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
    }
}