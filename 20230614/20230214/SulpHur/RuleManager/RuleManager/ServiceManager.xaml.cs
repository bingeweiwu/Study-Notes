using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Configuration;
using System.Reflection;
using System.Threading;
using System.IO;
using System.Globalization;
using System.Diagnostics;
using System.Windows.Automation;
using interop.UIAutomationCore;
using System.Drawing;
using System.Windows.Forms;
using MS.Internal.SulpHur.UICompliance;
using System.Runtime.InteropServices;
using SulpHurServiceAbstract;

namespace RuleManager
{
    /// <summary>
    /// Interaction logic for ServiceManager.xaml
    /// </summary>
    public partial class ServiceManager : Window
    {
        WindowDispatcher wd;
        IWinService winService;

        public ServiceManager()
        {
            InitializeComponent();

            view = new MainView();
            this.DataContext = view;
            wd = new WindowDispatcher(this);
            winService = WCFServiceWrapperBase.Instance().GetWindowsService(serverName);
            view.winService = winService;

            loaded = true;

            if (this.enablePageTab)
            {
                this.PageTypesTab.Visibility = Visibility.Visible;
            }
            else
            {
                this.PageTypesTab.Visibility = Visibility.Collapsed;
            }

            this.ServiceStatusTab.Visibility = Visibility.Collapsed;
        }

        #region Private Field
        public MainView view;
        private static System.Windows.Forms.Padding ZeroMargin = new System.Windows.Forms.Padding(0, 0, 0, 0);

        string serverName = ConfigurationManager.AppSettings["ServerName"];
        bool localRescan = bool.Parse(ConfigurationManager.AppSettings["LocalRescan"].ToString());
        bool enablePageTab = bool.Parse(ConfigurationManager.AppSettings["EnablePagesTab"].ToString());

        List<SulpHurServiceAbstract.AssemblyInfo> pageInDB;
        List<SulpHurServiceAbstract.AssemblyInfo> listInDll = new List<SulpHurServiceAbstract.AssemblyInfo>();
        private static string GETUIOPTION_CAPTURE = "Capture Attached Dialog";
        bool loaded = false;
        #endregion

        #region Private funcations

        private void RefreshRestoreButton()
        {
            if (this.WindowState == WindowState.Normal)
            {
                this.PART_Max.Content = "1";
                this.PART_Max.ToolTip = "Maximize";
            }
            else
            {
                this.PART_Max.Content = "2";
                this.PART_Max.ToolTip = "Restore";
            }
        }

        private void GetPageControlInProduct()
        {
            try
            {
                string binPath = System.IO.Path.Combine(wd.AdminConsolePath, "bin");
                AssemblyResolver.AddSearchPath(binPath);
                AssemblyResolver.Install();

                string[] dllFiles = Directory.GetFiles(binPath, "*.dll");
                foreach (string f1 in dllFiles)
                {
                    if (f1.Contains("Ribbon"))
                    {
                        continue;
                    }
                    SMSPageControlParser parse = new SMSPageControlParser(f1);
                    SmsFormDataParseResult result = null;
                    FileInfo fi = new FileInfo(f1);
                    try
                    {
                        result = parse.ParseDllforDialogsandControls();
                        foreach (Type t in result.controls)
                        {
                            if (!listInDll.Any(c => c.FullTypeName == t.FullName && c.FileName == fi.Name))
                            {
                                listInDll.Add(new SulpHurServiceAbstract.AssemblyInfo(t.FullName, fi.Name, result.result[t]));
                            }
                        }
                        foreach (Type t in result.controls)
                        {
                            if (!view.MissUI.Any(c => c.PageName == t.FullName && c.FileName == fi.Name))
                            {
                                PageControl tempPageControl = new PageControl(t.FullName, fi.Name, result.result[t]);
                                if (pageInDB.Any(c => c.FileName == fi.Name && c.FullTypeName == t.FullName))
                                {
                                    tempPageControl.IsInDB = true;
                                }
                                wd.AddMissUI(tempPageControl);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.ToString());
                    }
                }

                string consoleExe = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", binPath, "Microsoft.ConfigurationManagement.exe");
                string version = FileVersionInfo.GetVersionInfo(consoleExe).ProductVersion;

                ISulpHurTable sulpHurTable = SulpHurTableFactoryBase.Instance().GetSulpHurTable();

                sulpHurTable.ExecuteScalar(string.Format("Delete from BuildTypes WHERE BuildNo='{0}'", version));

                List<SulpHurServiceAbstract.BuildType> inherits;

                

                if (!string.IsNullOrEmpty(this.wd.InheriBuild))
                {
                    inherits = sulpHurTable.QueryTable<SulpHurServiceAbstract.BuildType>(
                       string.Format("Select * from BuildTypes WHERE BuildNo='{0}'", this.wd.InheriBuild));
                }
                else
                {
                    inherits = new List<SulpHurServiceAbstract.BuildType>();
                }

                foreach (SulpHurServiceAbstract.AssemblyInfo assembly in listInDll)
                {
                    SulpHurServiceAbstract.BuildType type = new SulpHurServiceAbstract.BuildType();
                    type.AssemblyName = assembly.FileName;
                    type.TypeName = assembly.FullTypeName;
                    type.BuildNo = version;
                    type.LanuchSteps = string.Empty;
                    type.Mark = "Valid UI (Missing Capture)";
                    foreach (SulpHurServiceAbstract.BuildType item in inherits)
                    {
                        if (item.AssemblyName == type.AssemblyName && item.TypeName == type.TypeName)
                        {
                            type.LanuchSteps = item.LanuchSteps;
                            type.Mark = item.Mark;
                        }
                    }
                    sulpHurTable.InsertRow<SulpHurServiceAbstract.BuildType>(type);
                }

                AssemblyResolver.Uninstall();
            }
            catch (Exception e1)
            {
                this.Dispatcher.BeginInvoke(new Action(delegate()
                {
                    view.MessageError = e1.ToString();
                }));
            }
        }
        
        #endregion

        #region Events funcations
        private void LogSwitch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (loaded)
            {
                LogSwitchLevel tag = (LogSwitchLevel)Enum.Parse(typeof(LogSwitchLevel), ((ComboBoxItem)this.LogSwitch.SelectedItem).Content.ToString().ToUpperInvariant());
                IWinService winService = WCFServiceWrapperBase.Instance().GetWindowsService(serverName);
                winService.SetLogSwitch(tag);
            }
        }
        private void PART_Min_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void PART_Max_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            RefreshRestoreButton();
        }
        private void PART_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void PART_TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                RefreshRestoreButton();
            }
            this.DragMove();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                view.InitialData();

                if (this.cbAvailableBuild.Items.Count > 0)
                    this.cbAvailableBuild.SelectedIndex = 0;

                if (this.cbAvailableBuild1.Items.Count > 0)
                    this.cbAvailableBuild1.SelectedIndex = 0;

                if (this.cbAvailableLanguage.Items.Count > 0)
                    this.cbAvailableLanguage.SelectedIndex = 0;

                if (this.cbAvailableOSType.Items.Count > 0)
                    this.cbAvailableOSType.SelectedIndex = 0;
            }
            catch (Exception e1)
            {
                System.Windows.MessageBox.Show(e1.ToString());
            }
        }
        
        private void QueryButton_Click(object sender, RoutedEventArgs e)
        {

            if ((bool)this.checkBoxResultID.IsChecked)
            {
                try
                {
                    int resultID = Int32.Parse(this.tbResultID.Text.Trim());
                    view.AddContentByResultID(resultID);
                }
                catch (FormatException)
                {
                    System.Windows.MessageBox.Show("Not valid id.");
                    return;
                }
            }
            else if ((bool)this.checkBoxContentID.IsChecked)
            {
                int contentID = Int32.Parse(this.tbContentID.Text.Trim());
                view.AddContentByContentID(contentID);
            }
            else
            {
                string buildLanguage = this.cbAvailableLanguage.SelectedValue.ToString();
                if (buildLanguage.ToLower() == "all") buildLanguage = "";

                string osType = this.cbAvailableOSType.SelectedValue.ToString();
                if (osType.ToLower() == "all") osType = "";

                string buildnumber = this.cbAvailableBuild.SelectedValue.ToString().Trim();
                view.AddContentByBuildNo(buildnumber, buildLanguage, osType);
            }
        }
        private void ContentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ContentView current = (ContentView)this.ContentList.SelectedItem;
            if (current != null)
            {
                view.LoadBitmapbyID(current.ContentID);
            }
        }
        
        private void btnShowMissUI_Click(object sender, RoutedEventArgs e)
        {
            view.MissUI.Clear();
            ISulpHurTable assemblyTable = SulpHurTableFactoryBase.Instance().GetSulpHurTable();
            pageInDB = assemblyTable.QueryTable<SulpHurServiceAbstract.AssemblyInfo>("select * from assemblyinfo");
            view.PageCountsInDB = string.Format("Page Count in DB:{0}", pageInDB.Count);
            Thread mainThread = new Thread(new ThreadStart(GetPageControlInProduct));
            mainThread.IsBackground = true;
            mainThread.Start();
        }
        
        private void btnAnaConfirm_Click(object sender, RoutedEventArgs e)
        {
            
        }
        private void checkBoxAll_Checked(object sender, RoutedEventArgs e)
        {
            
            foreach (ContentView cv in view.ContentList)
            {
                if (!cv.IsChecked)
                    cv.IsChecked = true;
            }
        }
        private void checkBoxAll_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (ContentView cv in view.ContentList)
            {
                if (cv.IsChecked)
                    cv.IsChecked = false;
            }
            
        }
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            view.IsStop = true;
        }
        private void idlinkButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button clickedButton = (System.Windows.Controls.Button)sender;
            string id = clickedButton.Content.ToString();

            
        }
        
        private void btnRescan_Click(object sender, RoutedEventArgs e)
        {
            if (!localRescan)
            {
                List<int> contentIDs = new List<int>();
                foreach (ContentView cv in view.ContentList)
                {
                    contentIDs.Add(Int32.Parse(cv.ContentID));
                }
                List<string> rules = new List<string>();
                foreach (ComplianceRule cr in view.RuleList1)
                {
                    if (cr.IsSelected)
                    {
                        rules.Add(cr.Name);
                    }
                }
                if (contentIDs.Count > 0 && rules.Count > 0)
                {
                    IWinService scanner = WCFServiceWrapperBase.Instance().GetWindowsService(this.serverName);
                    scanner.ReScanByContentList(contentIDs, rules);
                }
            }
            else
            {
                try
                {
                    
                }
                catch (Exception e1)
                {
                    System.Windows.MessageBox.Show(e1.ToString());
                }
            }
        }
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("kernel32.dll")]
        static extern int GetProcessId(IntPtr handle);
        private void btnShowENU_Click(object sender, RoutedEventArgs e)
        {
            ContentView current = (ContentView)this.ContentList.SelectedItem;
            if (current != null)
            {
                view.LoadENUBitmapbyID(current.ContentID);
            }
        }
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            
            Process[] ps = Process.GetProcesses();
            int errors = 0;
    
            foreach (Process currentProcess in ps)
            {
                try
                {
                    System.Windows.MessageBox.Show(currentProcess.MainModule.FileVersionInfo.ToString());
                }
                catch (Exception) {
                    System.Windows.MessageBox.Show("E");
                }
                
            }

            
        }
        #endregion

        private void checkBoxResultID_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void checkBoxResultID_Checked(object sender, RoutedEventArgs e)
        {
            this.checkBoxContentID.IsChecked = false;
        }

        private void checkBoxContentID_Checked(object sender, RoutedEventArgs e)
        {
            this.checkBoxResultID.IsChecked = false;
        }

        private void CleanButton_Click(object sender, RoutedEventArgs e)
        {
            string buildnumber = this.cbBuildClean.SelectedValue.ToString().Trim();
            IBuildClean cleaner= SulpHurTableFactoryBase.Instance().GetBuildClean();
            cleaner.CleanBuild(buildnumber);
        }

        private void RecoverButton_Click(object sender, RoutedEventArgs e)
        {

        }


    }
}
