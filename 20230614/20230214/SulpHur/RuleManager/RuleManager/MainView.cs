using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using MS.Internal.SulpHur.UICompliance;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing;
using System.Xml.Linq;
using MS.Internal.SulpHur.CMRules;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Windows;
using SulpHurServiceAbstract;

namespace RuleManager
{
    public class MainView : INotifyPropertyChanged
    {
        public IWinService winService { get; set; }
        private string pageCountsInDB;
        public string PageCountsInDB
        {
            get { return pageCountsInDB; }
            set { pageCountsInDB = value; OnPropertyChanged(new PropertyChangedEventArgs("PageCountsInDB")); }
        }
        private string pageCountsInBinary;
        public string PageCountsInBinary
        {
            get { return pageCountsInBinary; }
            set { pageCountsInBinary = value; OnPropertyChanged(new PropertyChangedEventArgs("PageCountsInBinary")); }
        }
        bool isStop = false;
        public bool IsStop
        {
            get { return isStop; }
            set { isStop = value; }
        }
        object propertyValue;
        public object PropertyValue
        {
            get { return propertyValue; }
            set { propertyValue = value; OnPropertyChanged(new PropertyChangedEventArgs("PropertyValue")); }
        }
        string resultID;
        public string ResultID
        {
            get { return resultID; }
            set { resultID = value; OnPropertyChanged(new PropertyChangedEventArgs("ResultID")); }
        }
        int inScanCount;
        public int InScanCount
        {
            get { return inScanCount; }
            set { inScanCount = value; OnPropertyChanged(new PropertyChangedEventArgs("InScanCount")); }
        }
        private string scanPageMessage;
        public string ScanPageMessage
        {
            get { return scanPageMessage; }
            set { scanPageMessage = value; OnPropertyChanged(new PropertyChangedEventArgs("ScanPageMessage")); }
        }
        private string resultMessage;
        public string ResultMessage
        {
            get { return resultMessage; }
            set { resultMessage = value; OnPropertyChanged(new PropertyChangedEventArgs("ResultMessage")); }
        }
        string messageError = "";
        public string MessageError
        {
            get { return messageError; }
            set { messageError = value; OnPropertyChanged(new PropertyChangedEventArgs("MessageError")); }
        }
        int progressValue = 0;
        public int ProgressValue
        {
            get { return progressValue; }
            set { progressValue = value; OnPropertyChanged(new PropertyChangedEventArgs("ProgressValue")); }
        }

        ObservableCollection<PageControl> missUI = new ObservableCollection<PageControl>();
        public ObservableCollection<PageControl> MissUI
        {
            get { return missUI; }
            set { missUI = value; }
        }
        ObservableCollection<PageControl> uiInDB = new ObservableCollection<PageControl>();
        public ObservableCollection<PageControl> UIInDB
        {
            get { return uiInDB; }
            set { uiInDB = value; }
        }
        ObservableCollection<ComplianceRule> ruleList = new ObservableCollection<ComplianceRule>();
        public ObservableCollection<ComplianceRule> RuleList
        {
            get { return ruleList; }
            set { ruleList = value; }
        }

        public void InitialRules()
        {
            RuleInfo ri = new RuleInfo();
            List<RuleInfo> ruleList = ri.AvailableRules;
            this.RuleList.Clear();

            foreach (RuleInfo r in ruleList)
            {
                ComplianceRule cr = new ComplianceRule(winService, r);
                this.RuleList.Add(cr);

                ComplianceRule temp = new ComplianceRule();
                temp.Name = r.RuleName;
                temp.Description = r.Descriptions;
                temp.IsSelected = false;
                this.RuleList1.Add(temp);
            }
        }
        public void InitialBuilds()
        {
            SulpHurBuildInfo bi = new SulpHurBuildInfo();
            this.AvailableBuildNo.Clear();
            this.AvailableBuildNo = bi.AvailableBuilds;
            this.AvailableBuildNo1.Clear();
            this.AvailableBuildNo1 = bi.AvailableBuilds;
        }
        public void InitialBuildLanguages()
        {
            SulpHurBuildInfo bi = new SulpHurBuildInfo();
            this.AvailableLanguage.Clear();
            this.AvailableLanguage.Add("All");
            this.AvailableLanguage.AddRange(bi.AvailableBuildLanguages);
        }
        public void InitialOSType()
        {
            SulpHurClientInfo ci = new SulpHurClientInfo();
            this.AvailableOSType.Clear();
            this.AvailableOSType.Add("All");
            this.AvailableOSType.AddRange(ci.AvailableOSTypes);
        }
        ObservableCollection<ComplianceRule> ruleList1 = new ObservableCollection<ComplianceRule>();

        public ObservableCollection<ComplianceRule> RuleList1
        {
            get { return ruleList1; }
            set { ruleList1 = value; }
        }

        ObservableCollection<ContentView> contentList = new ObservableCollection<ContentView>();

        public ObservableCollection<ContentView> ContentList
        {
            get { return contentList; }
            set { contentList = value; }
        }

        public void AddContentByResultID(int resultid)
        {
            UIRecord record = new UIRecord();
            record.QueryRecordByResultID(resultid);
            ContentView cv = new ContentView(record);
            this.ContentList.Add(cv);
        }
        public void AddContentByContentID(int contentid)
        {
            UIRecord record = new UIRecord();
            record.QueryRecordByContentID(contentid);
            ContentView cv = new ContentView(record);
            this.ContentList.Add(cv);
        }
        string previousID;
        public void LoadBitmapbyID(string id)
        {
            UIContentInfo ui = new UIContentInfo(Int32.Parse(id));
            ClearPreviousBitmap();
            previousID = id;

            this.ContentList.Single(c => c.ContentID == id).BitmapImage = ConvertBitmapToBitmapImage(ui.ScreenShot);
        }
        public void LoadENUBitmapbyID(string id)
        {
            UIContentInfo ui = new UIContentInfo(Int32.Parse(id));
            ClearPreviousBitmap();
            previousID = id;

            byte[] enuScreen = ui.ScreenShotENU;
            if (enuScreen != null)
            {
                this.ContentList.Single(c => c.ContentID == id).BitmapImage = ConvertBitmapToBitmapImage(ui.ScreenShotENU);
            }
        }
        private void ClearPreviousBitmap() {
            if (this.ContentList.Any(c => c.ContentID == previousID))
            {
                this.ContentList.Single(c => c.ContentID == previousID).BitmapImage = null;
            }
        }
        private BitmapImage ConvertBitmapToBitmapImage(byte[] bitmap)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                //bitmap.Save(memoryStream, ImageFormat.Jpeg);
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(bitmap);
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }
        public void AddContentByBuildNo(string buildno, string buildLanguage = "", string osType = "")
        {
            UIRecord record = new UIRecord();
            List<UIRecord> listUI = record.QueryUIRecords(buildno, buildLanguage, osType);
            this.ContentList.Clear();
            foreach (UIRecord r in listUI)
            {
                ContentView cv = new ContentView(r);
                this.ContentList.Add(cv);
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        List<string> availableLanguage = new List<string>();

        public List<string> AvailableLanguage
        {
            get { return availableLanguage; }
            set { availableLanguage = value; OnPropertyChanged(new PropertyChangedEventArgs("AvailableLanguage")); }
        }

        List<string> availableOSType = new List<string>();

        public List<string> AvailableOSType
        {
            get { return availableOSType; }
            set { availableOSType = value; OnPropertyChanged(new PropertyChangedEventArgs("AvailableOSType")); }
        }

        List<string> availableBuildNo = new List<string>();

        public List<string> AvailableBuildNo
        {
            get { return availableBuildNo; }
            set { availableBuildNo = value; OnPropertyChanged(new PropertyChangedEventArgs("AvailableBuildNo")); }
        }

        List<string> availableBuildNo1 = new List<string>();

        public List<string> AvailableBuildNo1
        {
            get { return availableBuildNo1; }
            set { availableBuildNo1 = value; OnPropertyChanged(new PropertyChangedEventArgs("AvailableBuildNo1")); }
        }

        private BitmapImage foregroudUI;

        public BitmapImage ForegroudUI
        {
            get { return foregroudUI; }
            set { foregroudUI = value; OnPropertyChanged(new PropertyChangedEventArgs("ForegroudUI")); }
        }

        private BitmapImage dbImage;

        public BitmapImage DbImage
        {
            get { return dbImage; }
            set { dbImage = value; OnPropertyChanged(new PropertyChangedEventArgs("DbImage")); }
        }

        private int UICount;

        public int UICount1
        {
            get { return UICount; }
            set { UICount = value; OnPropertyChanged(new PropertyChangedEventArgs("UICount1")); }
        }

        List<string> clients = new List<string>();
        public List<string> Clients
        {
            get { return clients; }
            set { clients = value; OnPropertyChanged(new PropertyChangedEventArgs("Clients")); }
        }

        ObservableCollection<Results> ruleResults = new ObservableCollection<Results>();
        public ObservableCollection<Results> RuleResults
        {
            get { return ruleResults; }
            set { ruleResults = value; OnPropertyChanged(new PropertyChangedEventArgs("RuleResults")); }
        }

        ObservableCollection<ViewComplianceResult> complianceResults = new ObservableCollection<ViewComplianceResult>();
        public ObservableCollection<ViewComplianceResult> ComplianceResults
        {
            get { return complianceResults; }
            set { complianceResults = value; OnPropertyChanged(new PropertyChangedEventArgs("ComplianceResults")); }
        }

        private string foregroundInfo;

        public string ForegroundInfo
        {
            get { return foregroundInfo; }
            set { foregroundInfo = value; OnPropertyChanged(new PropertyChangedEventArgs("ForegroundInfo")); }
        }

        public void InitialData()
        {
            InitialRules();
            InitialBuilds();
            InitialBuildLanguages();
            InitialOSType();
        }
    }

    public class Results : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        private string ruleName;

        public string RuleName
        {
            get { return ruleName; }
            set { ruleName = value; OnPropertyChanged(new PropertyChangedEventArgs("RuleName")); }
        }
    }

    public class ComplianceRule : INotifyPropertyChanged
    {
        IWinService op;

        public ComplianceRule() { }
        public ComplianceRule(IWinService operation, RuleInfo rule)
        {
            this.op = operation;
            this.name = rule.RuleName;
            this.description = rule.Descriptions;
            this.isEnable = rule.IsEnabled;
        }

        private string name;
        private string description;
        private bool isEnable;

        public bool IsEnable
        {
            get { return isEnable; }
            set
            {
                isEnable = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsEnable"));
                if (op != null)
                {
                    op.SetRuleStatus(value, this.name);
                }
            }
        }

        public string Name
        {
            get { return name; }
            set { name = value; OnPropertyChanged(new PropertyChangedEventArgs("Name")); }
        }

        public string Description
        {
            get { return description; }
            set { description = value; OnPropertyChanged(new PropertyChangedEventArgs("Description")); }
        }

        private bool isSelected = false;
        public bool IsSelected
        {
            get { return isSelected; }
            set { isSelected = value; OnPropertyChanged(new PropertyChangedEventArgs("IsSelected")); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
    }

    public class ViewComplianceResult : ComplianceResult
    {

        public ViewComplianceResult(ComplianceResult cr)
        {
            this.RuleName = cr.RuleName;
            this.Type = cr.Type;
            this.Message = cr.Message;
            this.Root = cr.Root;
            this.controls = cr.Controls;
            if (cr.Image != null)
            {
                this.BitmapImage = ConvertBitmapToBitmapImage(cr.Image);
            }
            this.resultID = Guid.NewGuid();
        }

        private ElementInformation root;

        public ElementInformation Root
        {
            get { return root; }
            set { root = value; }
        }

        private List<ElementInformation> controls;

        public List<ElementInformation> Controls
        {
            get { return controls; }
            set { controls = value; }
        }

        private BitmapImage bitmapImage;

        public BitmapImage BitmapImage
        {
            get { return bitmapImage; }
            set { bitmapImage = value; OnPropertyChanged(new PropertyChangedEventArgs("BitmapImage")); }
        }

        private string uiid;

        public string UIID
        {
            get { return uiid; }
            set { uiid = value; OnPropertyChanged(new PropertyChangedEventArgs("UIID")); }
        }

        private Guid resultID;

        public Guid ResultID
        {
            get { return resultID; }
            set { resultID = value; }
        }

        private BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            MemoryStream memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Jpeg);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(memoryStream.ToArray());
            bitmapImage.EndInit();

            return bitmapImage;
        }
    }

    public class PageControl : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
        private string pageName;
        public string PageName
        {
            get { return pageName; }
            set { pageName = value; OnPropertyChanged(new PropertyChangedEventArgs("PageName")); }
        }

        public string fileName;
        public string FileName
        {
            get { return fileName; }
            set { fileName = value; OnPropertyChanged(new PropertyChangedEventArgs("FileName")); }
        }
        public PageControl()
        { }

        private string typeName;
        public string TypeName
        {
            get { return typeName; }
            set { typeName = value; OnPropertyChanged(new PropertyChangedEventArgs("TypeName")); }
        }

        private bool isInDB=false;

        public bool IsInDB
        {
            get { return isInDB; }
            set { isInDB = value; OnPropertyChanged(new PropertyChangedEventArgs("IsInDB")); }
        }
        private bool isInBinary;

        public bool IsInBinary
        {
            get { return isInBinary; }
            set { isInBinary = value; OnPropertyChanged(new PropertyChangedEventArgs("IsInBinary")); }
        }

        private string mark;

        public string Mark
        {
            get { return mark; }
            set { mark = value; OnPropertyChanged(new PropertyChangedEventArgs("Mark")); }
        }

        public PageControl(string pageName, string fileName)
        {
            PageName = pageName;
            FileName = fileName;
        }
        public PageControl(string pageName, string fileName, string typeName)
        {
            PageName = pageName;
            FileName = fileName;
            TypeName = typeName;
        }
        public PageControl(SulpHurServiceAbstract.AssemblyInfo ai)
        {
            PageName = ai.FullTypeName;
            FileName = ai.FileName;
            TypeName = ai.TypeName;
        }
    }

    public class ContentView : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
        private string contentID;
        public string ContentID
        {
            get { return contentID; }
            set { contentID = value; OnPropertyChanged(new PropertyChangedEventArgs("ContentID")); }
        }

        private Bitmap image;

        public Bitmap Image
        {
            get { return image; }
            set { image = value; }
        }

        private BitmapImage bitmapImage;

        public BitmapImage BitmapImage
        {
            get { return bitmapImage; }
            set { bitmapImage = value; OnPropertyChanged(new PropertyChangedEventArgs("BitmapImage")); }
        }
        public ContentView()
        { }
        public ContentView(UIRecord record)
        {
            this.UserName = record.UserName;
            this.ContentID = record.ContentID.ToString();
            this.UIName = record.PageTitle;
        }

        public ContentView(string contentID, string name, string userName) {
            this.contentID = contentID;
            this.uiName = name;
            this.userName = userName;
        }

        private BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            Bitmap bit1 = new Bitmap(bitmap);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                bit1.Save(memoryStream, ImageFormat.Jpeg);
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(memoryStream.ToArray());
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        private bool isChecked = false;

        public bool IsChecked
        {
            get { return isChecked; }
            set { isChecked = value; OnPropertyChanged(new PropertyChangedEventArgs("IsChecked")); }
        }

        private string uiName;


        public string UIName
        {
            get { return uiName; }
            set { uiName = value; OnPropertyChanged(new PropertyChangedEventArgs("UIName")); }
        }

        private ElementInformation ei;

        public ElementInformation Ei
        {
            get { return ei; }
            set { ei = value; }
        }

        private string userName;

        public string UserName
        {
            get { return userName; }
            set { userName = value; OnPropertyChanged(new PropertyChangedEventArgs("UserName")); }
        }
    }

    //public class ViewCommand
    //{
    //    MainView view;
    //    IAdminOperations ruleOperation;
    //    public ServiceManager window;
    //    private string previousID = string.Empty;
    //    private Guid previousID_ResultTab = Guid.Empty;
    //    List<UIComplianceRuleBase> ruleInstances = new List<UIComplianceRuleBase>();

    //    public ViewCommand(MainView view, IAdminOperations ruleOperation)
    //    {
    //        this.view = view;
    //        this.ruleOperation = ruleOperation;
    //    }
    //    public void VeriyQueried()
    //    {
    //        FileInfo fi1 = new FileInfo(@"C:\Users\v-sturs\Desktop\1.png");
    //        FileStream fs1 = fi1.OpenRead();
    //        System.Drawing.Image Img1 = System.Drawing.Image.FromStream(fs1);
    //        Bitmap bmp = new Bitmap(Img1);//可以设定图片的大小
    //        for (int X = 0; X < bmp.Width; X++)
    //        {
    //            for (int Y = 0; Y < bmp.Height; Y++)
    //            {
    //                System.Drawing.Color rgb = bmp.GetPixel(X, Y);
    //                byte bBlue = rgb.B;
    //                byte bRed = rgb.R;
    //                byte bGreen = rgb.G;
    //                int color = (9798 * bRed + 19235 * bGreen + 3735 * bBlue) / 32768;
    //                System.Drawing.Color NewColor = System.Drawing.Color.FromArgb(color, color, color);
    //                bmp.SetPixel(X, Y, NewColor);
    //            }
    //        }
    //        bmp.Save(@"C:\Users\v-sturs\Desktop\2.png", ImageFormat.Jpeg);
    //        bmp.Dispose();
    //        fs1.Dispose();
    //        fs1.Close();

    //    }
    //    public void OpenElementInfo(string id) {
    //        using (DBWrapper wrapper = new DBWrapper())
    //        {
    //            XElement element = wrapper.GetElementByID1(Int32.Parse(id));
    //            string folder=Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    //            string path=System.IO.Path.Combine(folder,"TempElement.xml");
    //            element.Save(path);
    //            Process.Start(path);
    //        }
    //    }
    //    public void LoadRuleList()
    //    {
    //        List<ComplianceRule> list = ruleOperation.GetRuleList();
    //        view.RuleList.Clear();
    //        foreach (ComplianceRule r in list)
    //        {
    //            view.RuleList.Add(r);
    //        }
    //    }
    //    public void LoadAvailableBuild()
    //    {
    //        using (DBWrapper wrapper = new DBWrapper())
    //        {
    //            view.AvailableBuildNo.Clear();
    //            view.AvailableBuildNo = wrapper.QueryAllAvailableBuild();
    //        }
    //    }
    //    public void LoadAvailableLanguage()
    //    {
    //        using (DBWrapper wrapper = new DBWrapper())
    //        {
    //            view.AvailableLanguage.Clear();
    //            view.AvailableLanguage.Add("All");
    //            view.AvailableLanguage.AddRange(wrapper.QueryAllAvailableLanguage());
    //        }
    //    }
    //    public void LoadAvailableOSType()
    //    {
    //        using (DBWrapper wrapper = new DBWrapper())
    //        {
    //            view.AvailableOSType.Clear();
    //            view.AvailableOSType.Add("All");
    //            view.AvailableOSType.AddRange(wrapper.QueryAllAvailableOSType());
    //        }
    //    }
    //    public void LoadAvailableBuild1()
    //    {
    //        using (DBWrapper wrapper = new DBWrapper())
    //        {
    //            view.AvailableBuildNo1.Clear();
    //            view.AvailableBuildNo1 = wrapper.QueryAllAvailableBuildInBuildTypes();
    //        }
    //    }
    //    public void Rescan(string buildNo)
    //    {
    //        ruleOperation.ReScanByBuildNo(buildNo);
    //    }
    //    public void ReLoadRules()
    //    {
    //        ruleOperation.ReLoadRules();
    //    }
    //    public void LoadClients()
    //    {
    //        view.Clients.Clear();
    //        view.Clients = ruleOperation.GetClients();
    //    }
    //    public void GenerateResult(ElementInformation ei, Bitmap screen, string id)
    //    {
    //        view.ComplianceResults.Clear();
    //        List<MS.Internal.SulpHur.UICompliance.ElementInformation> list = ParseTreeToList(ei);
    //        MS.Internal.SulpHur.UICompliance.ElementInformation root = list[0];
    //        ControlScreen.CurrentBit = screen;
    //        foreach (ComplianceRule r in view.RuleList1.Where(c => c.IsSelected == true))
    //        {
    //            List<ComplianceResult> list1 = new List<ComplianceResult>();
    //            UIComplianceRuleBase rule = ruleInstances.Single(c => c.Name == r.Name);
    //            CommonRuleUtility.NULLAssign(list);
    //            List<UIComplianceResultBase> results = rule.UIVerify(list);

    //            foreach (UIComplianceResultBase b in results)
    //            {
    //                ComplianceResult cr = new ComplianceResult();
    //                if (b.Image == null)
    //                {
    //                    cr.Controls = b.Controls;
    //                    cr.Root = root;
    //                    cr.Image = DrawBitmap(screen, b.Controls, root);
    //                }
    //                else
    //                {
    //                    cr.Image = b.Image;
    //                }
    //                cr.Message = b.Message;
    //                cr.Type = b.Type.ToString();
    //                cr.RuleName = rule.Name;
    //                list1.Add(cr);
    //            }

    //            foreach (ComplianceResult cr in list1)
    //            {
    //                ViewComplianceResult vcr = new ViewComplianceResult(cr);
    //                vcr.UIID = id;
    //                view.ComplianceResults.Add(vcr);
    //            }
    //        }
    //    }
    //    public void VerifySelectedUI()
    //    {
    //        //window.Dispatcher.Invoke(new Action(delegate()
    //        //{
    //        //    view.ComplianceResults.Clear();
    //        //}));
    //        DBWrapper wrap = new DBWrapper();
    //        foreach (ContentView cv in view.ContentList)
    //        {
    //            if (cv.IsChecked)
    //            {
    //                cv.Ei = wrap.GetElementByID(Int32.Parse(cv.ContentID));
    //                if (cv.Image == null)
    //                {
    //                    cv.Image = wrap.GetBitmapByID(Int32.Parse(cv.ContentID));
    //                }
    //                MS.Internal.SulpHur.UICompliance.ElementInformation ei = cv.Ei;
    //                List<MS.Internal.SulpHur.UICompliance.ElementInformation> list = ParseTreeToList(ei);
    //                MS.Internal.SulpHur.UICompliance.ElementInformation root = list[0];
    //                ControlScreen.CurrentBit = cv.Image;

    //                CMAccessKeyRule rule = new CMAccessKeyRule();
    //                //foreach (ComplianceRule r in view.RuleList1.Where(c => c.IsSelected == true)) {
    //                List<ComplianceResult> list1 = new List<ComplianceResult>();
    //                //UIComplianceRuleBase rule = ruleInstances.Single(c => c.Name == r.Name);
    //                List<UIComplianceResultBase> results = rule.UIVerify(list);

    //                //MessageBox.Show(results.Count.ToString());
    //                foreach (UIComplianceResultBase b in results)
    //                {
    //                    ComplianceResult cr = new ComplianceResult();
    //                    if (b.Image == null)
    //                    {
    //                        cr.Controls = b.Controls;
    //                        cr.Root = root;
    //                        //cr.Image = DrawBitmap(cv.Image, b.Controls, root);
    //                    }
    //                    else
    //                    {
    //                        cr.Image = b.Image;
    //                    }
    //                    cr.Message = b.Message;
    //                    cr.Type = b.Type.ToString();
    //                    cr.RuleName = rule.Name;
                        
    //                    list1.Add(cr);
    //                }

    //                //foreach (ComplianceResult cr in list1)
    //                //{
    //                //    window.Dispatcher.Invoke(new Action(delegate()
    //                //    {
    //                //        ViewComplianceResult vcr = new ViewComplianceResult(cr);
    //                //        vcr.UIID = cv.ContentID;
    //                //        view.ComplianceResults.Add(vcr);
    //                //    }));
    //                //    Thread.Sleep(500);
    //                //}
    //            }
    //            cv.Image = null;
    //            cv.Ei = null;
    //            //}
    //        }
    //        //window.Dispatcher.Invoke(new Action(delegate()
    //        //{
    //        //    view.ResultMessage = "Finish.";
    //        //}));
    //    }
    //    public void VerifyLocalRules(string computerName)
    //    {
    //        ForegroundData fd = ruleOperation.GetForegroundData(computerName);
    //        if (fd != null)
    //        {
    //            view.ForegroundInfo = string.Format("[AssemblyName:]{0}, [FullType Name:]{1}", fd.AssemblyName, fd.AssemblyType);
    //            view.ForegroudUI = ConvertBitmapToBitmapImage(fd.Image);

    //            List<ComplianceResult> list1 = new List<ComplianceResult>();
    //            MS.Internal.SulpHur.UICompliance.ElementInformation ei = fd.Ei;
    //            List<MS.Internal.SulpHur.UICompliance.ElementInformation> list = ParseTreeToList(ei);
    //            MS.Internal.SulpHur.UICompliance.ElementInformation root = list[0];
    //            OverlapRule rule = new OverlapRule();
    //            List<UIComplianceResultBase> results = rule.UIVerify(list);
    //            foreach (UIComplianceResultBase b in results)
    //            {
    //                ComplianceResult cr = new ComplianceResult();
    //                cr.Image = DrawBitmap(fd.Image, b.Controls, root);
    //                cr.Message = b.Message;
    //                cr.Type = b.Type.ToString();
    //                cr.RuleName = rule.Name;
    //                list1.Add(cr);
    //            }
    //            view.ComplianceResults.Clear();
    //            foreach (ComplianceResult cr in list1)
    //            {
    //                ViewComplianceResult vcr = new ViewComplianceResult(cr);
    //                view.ComplianceResults.Add(vcr);
    //            }
    //        }
    //    }
    //    private Bitmap DrawBitmap(Bitmap bit, List<MS.Internal.SulpHur.UICompliance.ElementInformation> relatedControls, MS.Internal.SulpHur.UICompliance.ElementInformation root)
    //    {
    //        Bitmap tempBit = new Bitmap(bit);

    //        Graphics gfxImage = Graphics.FromImage(tempBit);
    //        foreach (MS.Internal.SulpHur.UICompliance.ElementInformation ei in relatedControls)
    //        {
    //            System.Drawing.Pen controlPen = new System.Drawing.Pen(System.Drawing.Color.Yellow, 2);
    //            System.Windows.Rect rectTemp = new System.Windows.Rect(ei.BoundingRectangle.X, ei.BoundingRectangle.Y, ei.BoundingRectangle.Width, ei.BoundingRectangle.Height);
    //            double offsetX = -root.BoundingRectangle.X;
    //            double offsetY = -root.BoundingRectangle.Y;
    //            rectTemp.Offset(offsetX, offsetY);
    //            gfxImage.DrawRectangle(controlPen, CommonUtility.ToWinRectangle(rectTemp));
    //        }
    //        return tempBit;
    //    }
    //    private Bitmap DrawBitmapWithNumber(Bitmap bit, List<MS.Internal.SulpHur.UICompliance.ElementInformation> relatedControls, MS.Internal.SulpHur.UICompliance.ElementInformation root)
    //    {
    //        Bitmap tempBit = new Bitmap(bit);

    //        Graphics gfxImage = Graphics.FromImage(tempBit);
    //        int i = 1;
    //        foreach (MS.Internal.SulpHur.UICompliance.ElementInformation ei in relatedControls)
    //        {
    //            System.Drawing.Pen controlPen = new System.Drawing.Pen(System.Drawing.Color.Yellow, 2);
    //            System.Windows.Rect rectTemp = new System.Windows.Rect(ei.BoundingRectangle.X, ei.BoundingRectangle.Y, ei.BoundingRectangle.Width, ei.BoundingRectangle.Height);
    //            double offsetX = -root.BoundingRectangle.X;
    //            double offsetY = -root.BoundingRectangle.Y;
    //            rectTemp.Offset(offsetX, offsetY);
    //            gfxImage.DrawRectangle(controlPen, CommonUtility.ToWinRectangle(rectTemp));
    //            StringFormat stringFormat = new StringFormat();
    //            stringFormat.Alignment = StringAlignment.Center;
    //            stringFormat.LineAlignment = StringAlignment.Center;
    //            gfxImage.DrawString(i.ToString(), new Font("Segoe UI", 24), System.Drawing.Brushes.Red, CommonUtility.ToWinRectangle(rectTemp), stringFormat);
    //            i++;
    //        }
    //        return tempBit;
    //    }
    //    private List<MS.Internal.SulpHur.UICompliance.ElementInformation> ParseTreeToList(MS.Internal.SulpHur.UICompliance.ElementInformation eiRoot)
    //    {
    //        List<MS.Internal.SulpHur.UICompliance.ElementInformation> infoList = new List<MS.Internal.SulpHur.UICompliance.ElementInformation>();
    //        infoList.Add(eiRoot);

    //        AddChild(eiRoot, infoList);

    //        foreach (MS.Internal.SulpHur.UICompliance.ElementInformation ei in infoList)
    //        {
    //            if (ei.Children.Count > 0 && ei.Descendants == null)
    //            {
    //                ei.Descendants = new List<MS.Internal.SulpHur.UICompliance.ElementInformation>();
    //            }

    //            if (ei.Parent != null && ei.Ancestors == null)
    //            {
    //                ei.Ancestors = new List<MS.Internal.SulpHur.UICompliance.ElementInformation>();
    //            }
    //        }

    //        foreach (MS.Internal.SulpHur.UICompliance.ElementInformation ei in infoList)
    //        {
    //            AddDescents(ei, ei.Descendants);
    //            AddAncestors(ei, ei.Ancestors);
    //        }
    //        return infoList;
    //    }
    //    private void AddChild(WebElementInfo webEi, List<WebElementInfo> infoList)
    //    {
    //        for (int i = 0; i < webEi.Children.Count; i++)
    //        {
    //            WebElementInfo temp = webEi.Children[i];
    //            temp.Parent = webEi;
    //            infoList.Add(temp);
    //            AddChild(temp, infoList);
    //        }
    //    }
    //    private void AddChild(MS.Internal.SulpHur.UICompliance.ElementInformation ei, List<MS.Internal.SulpHur.UICompliance.ElementInformation> infoList)
    //    {
    //        if (ei.Children == null) return;
    //        if (ei.Children.Count > 0)
    //        {
    //            ei.FirstChild = ei.Children.First();
    //            ei.LastChild = ei.Children.Last();
    //        }


    //        for (int i = 0; i < ei.Children.Count; i++)
    //        {

    //            MS.Internal.SulpHur.UICompliance.ElementInformation temp = ei.Children[i];
    //            temp.Parent = ei;
    //            if (temp.NativeWindowHandle != 0||temp.ControlType==ControlType.Hyperlink)
    //            {
    //                //This attribute not used current time
    //                //temp.treeLevel = ei.treeLevel + 1;
    //                AddSibling(i, temp, ei.Children);
    //                infoList.Add(temp);
    //                AddChild(temp, infoList);
    //            }
    //        }
    //    }
    //    internal void AddDescents(MS.Internal.SulpHur.UICompliance.ElementInformation ei, List<MS.Internal.SulpHur.UICompliance.ElementInformation> descents)
    //    {
    //        foreach (MS.Internal.SulpHur.UICompliance.ElementInformation temp in ei.Children)
    //        {
    //            descents.Add(temp);
    //            AddDescents(temp, descents);
    //        }
    //    }
    //    internal void AddAncestors(MS.Internal.SulpHur.UICompliance.ElementInformation ei, List<MS.Internal.SulpHur.UICompliance.ElementInformation> ancestors)
    //    {
    //        if (ei.Parent == null) return;
    //        else
    //        {
    //            ancestors.Add(ei.Parent);
    //            AddAncestors(ei.Parent, ancestors);
    //        }
    //    }
    //    internal void AddSibling(int current, MS.Internal.SulpHur.UICompliance.ElementInformation currentEi, List<MS.Internal.SulpHur.UICompliance.ElementInformation> siblingCollection)
    //    {
    //        for (int i = 0; i < siblingCollection.Count; i++)
    //        {
    //            if (i != current)
    //            {
    //                if (currentEi.Siblings == null) currentEi.Siblings = new List<MS.Internal.SulpHur.UICompliance.ElementInformation>();
    //                currentEi.Siblings.Add(siblingCollection[i]);
    //            }

    //            if (i == current - 1)
    //            {
    //                if (currentEi.PreviousSibling == null) currentEi.PreviousSibling = new MS.Internal.SulpHur.UICompliance.ElementInformation();
    //                currentEi.PreviousSibling = siblingCollection[i];
    //            }

    //            if (i == current + 1)
    //            {
    //                if (currentEi.NextSibling == null) currentEi.NextSibling = new MS.Internal.SulpHur.UICompliance.ElementInformation();
    //                currentEi.NextSibling = siblingCollection[i];
    //            }
    //        }
    //    }
    //    public void LoadContentByBuild(string buildno)
    //    {
    //        using (DBWrapper wrapper = new DBWrapper())
    //        {
    //            view.ContentList.Clear();
    //            List<ContentView> list = wrapper.GetContents(buildno);
    //            foreach (ContentView cv in list)
    //            {
    //                view.ContentList.Add(cv);
    //            }
    //        }
    //    }
    //    public void LoadContentByBuild(string buildno,string buildlanguage,string osType)
    //    {
    //        using (DBWrapper wrapper = new DBWrapper())
    //        {
    //            view.ContentList.Clear();
    //            List<ContentView> list = wrapper.GetContents(buildno,buildlanguage,osType);
    //            foreach (ContentView cv in list)
    //            {
    //                view.ContentList.Add(cv);
    //            }
    //        }
    //    }
    //    public void LoadContentByContenRange(int start, int end)
    //    {
    //        using (DBWrapper wrapper = new DBWrapper())
    //        {
    //            view.ContentList.Clear();
    //            List<ContentView> list = wrapper.GetContents(start, end);
    //            if (list.Count == 0) {
    //                System.Windows.MessageBox.Show("No UI in this range.");
    //            }
    //            foreach (ContentView cv in list)
    //            {
    //                view.ContentList.Add(cv);
    //            }
    //        }
    //    }
    //    public void LoadContentByResultID(int resultID)
    //    {
    //        using (DBWrapper wrapper = new DBWrapper())
    //        {
    //            view.ContentList.Clear();
    //            ContentView ui = wrapper.GetContents(resultID);
    //            if (ui == null) {
    //                System.Windows.MessageBox.Show("Result not found");
    //                return;
    //            }
    //            view.ContentList.Add(ui);
    //        }
    //    }
    //    public void LoadBitmapbyID(string id) {
    //        if (view.ContentList.Any(c => c.ContentID == previousID)) {
    //            view.ContentList.Single(c => c.ContentID == previousID).BitmapImage = null;
    //        }
    //        previousID = id;
    //        using (DBWrapper wrapper = new DBWrapper())
    //        {
    //            view.ContentList.Single(c => c.ContentID == id).BitmapImage = ConvertBitmapToBitmapImage(wrapper.GetBitmapByID(Int32.Parse(id)));
    //        }
    //    }
    //    public void LoadBitmapbyIDENU(string id)
    //    {
    //        if (view.ContentList.Any(c => c.ContentID == previousID))
    //        {
    //            view.ContentList.Single(c => c.ContentID == previousID).BitmapImage = null;
    //        }
    //        previousID = id;
    //        using (DBWrapper wrapper = new DBWrapper())
    //        {
    //            view.ContentList.Single(c => c.ContentID == id).BitmapImage = ConvertBitmapToBitmapImage(wrapper.GetBitmapByIDENU(Int32.Parse(id)));
    //        }
    //    }
    //    public void LoadBitmapbyID_ResultTab(Guid guid, string id,
    //        List<MS.Internal.SulpHur.UICompliance.ElementInformation> relatedControls,
    //        MS.Internal.SulpHur.UICompliance.ElementInformation root)
    //    {
    //        if (view.ComplianceResults.Any(c => c.ResultID == previousID_ResultTab))
    //        {
    //            view.ComplianceResults.Single(c => c.ResultID == previousID_ResultTab).BitmapImage = null;
    //        }
    //        previousID_ResultTab = guid;
    //        using (DBWrapper wrapper = new DBWrapper())
    //        {
    //            Bitmap temp = DrawBitmap(wrapper.GetBitmapByID(Int32.Parse(id)), relatedControls, root);
    //            view.ComplianceResults.Single(c => c.ResultID == guid).BitmapImage = ConvertBitmapToBitmapImage(temp);
    //        }
    //    }
    //    public void LoadLocalRules()
    //    {
    //        view.RuleList1.Clear();
    //        ruleInstances.Clear();
    //        CMAccessKeyRule accessKeyRule = new CMAccessKeyRule();
    //        ComplianceRule accessKeyRuleView = new ComplianceRule();
    //        accessKeyRuleView.Name = accessKeyRule.Name;
    //        accessKeyRuleView.IsSelected = false;
    //        view.RuleList1.Add(accessKeyRuleView);
    //        ruleInstances.Add(accessKeyRule);

    //        CMAlignmentRule alignmentRule = new CMAlignmentRule();
    //        ComplianceRule alignmentRuleView = new ComplianceRule();
    //        alignmentRuleView.Name = alignmentRule.Name;
    //        alignmentRuleView.IsSelected = false;
    //        view.RuleList1.Add(alignmentRuleView);
    //        ruleInstances.Add(alignmentRule);

    //        CMButtonRule buttonRule = new CMButtonRule();
    //        ComplianceRule buttonRuleView = new ComplianceRule();
    //        buttonRuleView.Name = buttonRule.Name;
    //        buttonRuleView.IsSelected = false;
    //        view.RuleList1.Add(buttonRuleView);
    //        ruleInstances.Add(buttonRule);

    //        EmptyStringResourceRule emptyStringResourceRule = new EmptyStringResourceRule();
    //        ComplianceRule emptyStringResourceRuleView = new ComplianceRule();
    //        emptyStringResourceRuleView.Name = emptyStringResourceRule.Name;
    //        emptyStringResourceRuleView.IsSelected = false;
    //        view.RuleList1.Add(emptyStringResourceRuleView);
    //        ruleInstances.Add(emptyStringResourceRule);

    //        CMLabelAlignment labelAlignmentRule = new CMLabelAlignment();
    //        ComplianceRule labelAlignmentRuleView = new ComplianceRule();
    //        labelAlignmentRuleView.Name = labelAlignmentRule.Name;
    //        labelAlignmentRuleView.IsSelected = false;
    //        view.RuleList1.Add(labelAlignmentRuleView);
    //        ruleInstances.Add(labelAlignmentRule);

    //        LeftEdgeRule leftEdgeRule = new LeftEdgeRule();
    //        ComplianceRule leftEdgeRuleView = new ComplianceRule();
    //        leftEdgeRuleView.Name = leftEdgeRule.Name;
    //        leftEdgeRuleView.IsSelected = false;
    //        view.RuleList1.Add(leftEdgeRuleView);
    //        ruleInstances.Add(leftEdgeRule);

    //        OverlapRule overlapRule = new OverlapRule();
    //        ComplianceRule overlapRuleView = new ComplianceRule();
    //        overlapRuleView.Name = overlapRule.Name;
    //        overlapRuleView.IsSelected = false;
    //        view.RuleList1.Add(overlapRuleView);
    //        ruleInstances.Add(overlapRule);

    //        PunctuationRule punctuationRule = new PunctuationRule();
    //        ComplianceRule punctuationRuleView = new ComplianceRule();
    //        punctuationRuleView.Name = punctuationRule.Name;
    //        punctuationRuleView.IsSelected = false;
    //        view.RuleList1.Add(punctuationRuleView);
    //        ruleInstances.Add(punctuationRule);

    //        CMQueryAllUIRule allScreenShotRule = new CMQueryAllUIRule();
    //        ComplianceRule allScreenShotRuleView = new ComplianceRule();
    //        allScreenShotRuleView.Name = allScreenShotRule.Name;
    //        allScreenShotRuleView.IsSelected = false;
    //        view.RuleList1.Add(allScreenShotRuleView);
    //        ruleInstances.Add(allScreenShotRule);

    //        TabOrderRule tabOrderRule = new TabOrderRule();
    //        ComplianceRule tabOrderRuleView = new ComplianceRule();
    //        tabOrderRuleView.Name = tabOrderRule.Name;
    //        tabOrderRuleView.IsSelected = false;
    //        view.RuleList1.Add(tabOrderRuleView);
    //        ruleInstances.Add(tabOrderRule);

    //        Truncation30Rule truncation30Rule = new Truncation30Rule();
    //        ComplianceRule truncation30RuleView = new ComplianceRule();
    //        truncation30RuleView.Name = truncation30Rule.Name;
    //        truncation30RuleView.IsSelected = false;
    //        view.RuleList1.Add(truncation30RuleView);
    //        ruleInstances.Add(truncation30Rule);

    //        TruncationRule truncationRule = new TruncationRule();
    //        ComplianceRule truncationRuleView = new ComplianceRule();
    //        truncationRuleView.Name = truncationRule.Name;
    //        truncationRuleView.IsSelected = false;
    //        view.RuleList1.Add(truncationRuleView);
    //        ruleInstances.Add(truncationRule);

    //        UATextRule uaTextRule = new UATextRule();
    //        ComplianceRule uaTextRuleView = new ComplianceRule();
    //        uaTextRuleView.Name = uaTextRule.Name;
    //        uaTextRuleView.IsSelected = false;
    //        view.RuleList1.Add(uaTextRuleView);
    //        ruleInstances.Add(uaTextRule);

    //        WindowSizeRule windowsSizeRule = new WindowSizeRule();
    //        ComplianceRule windowsSizeRuleView = new ComplianceRule();
    //        windowsSizeRuleView.Name = windowsSizeRule.Name;
    //        windowsSizeRuleView.IsSelected = false;
    //        view.RuleList1.Add(windowsSizeRuleView);
    //        ruleInstances.Add(windowsSizeRule);

    //        CriticalAccessKeyRule criticalAccessKeyRule = new CriticalAccessKeyRule();
    //        ComplianceRule criticalAccessKeyRuleView = new ComplianceRule();
    //        criticalAccessKeyRuleView.Name = criticalAccessKeyRule.Name;
    //        criticalAccessKeyRuleView.IsSelected = false;
    //        view.RuleList1.Add(criticalAccessKeyRuleView);
    //        ruleInstances.Add(criticalAccessKeyRule);


    //        HelpTopicRule helpTopicRule = new HelpTopicRule();
    //        ComplianceRule helpTopicRuleView = new ComplianceRule();
    //        helpTopicRuleView.Name = helpTopicRule.Name;
    //        helpTopicRuleView.IsSelected = false;
    //        view.RuleList1.Add(helpTopicRuleView);
    //        ruleInstances.Add(helpTopicRule);
    //    }
    //    public void MapServerRules() {
    //        view.RuleList1.Clear();
    //        foreach (ComplianceRule cr in view.RuleList)
    //        {
    //            ComplianceRule temp = new ComplianceRule();
    //            temp.Name = cr.Name;
    //            temp.IsSelected = cr.IsEnable;
    //            view.RuleList1.Add(temp);
    //        }
    //    }
    //    private bool IsSubclassOf(Type t, Type b)
    //    {
    //        if (t.BaseType == null) return false;
    //        if (t.BaseType.Equals(b)) return true;
    //        return IsSubclassOf(t.BaseType, b);
    //    }
    //    private BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
    //    {
    //        Bitmap bit = new Bitmap(bitmap);

    //        using (MemoryStream memoryStream = new MemoryStream())
    //        {
    //            bit.Save(memoryStream, ImageFormat.Jpeg);
    //            BitmapImage bitmapImage = new BitmapImage();
    //            bitmapImage.BeginInit();
    //            bitmapImage.StreamSource = new MemoryStream(memoryStream.ToArray());
    //            bitmapImage.EndInit();

    //            return bitmapImage;
    //        }
    //    }
    //    public void ShowResult(string computerName)
    //    {
    //        List<ComplianceResult> results = ruleOperation.GetVerifyResult(computerName);
    //        view.ComplianceResults.Clear();
    //        foreach (ComplianceResult cr in results)
    //        {
    //            ViewComplianceResult vcr = new ViewComplianceResult(cr);
    //            view.ComplianceResults.Add(vcr);
    //        }
    //    }
    //    public void VerifyDBSingleUI()
    //    {
    //        DBWrapper wrapper = new DBWrapper();
    //        CapturedData data = wrapper.GetSpecifiedData(Int32.Parse(view.ResultID));
    //        List<ComplianceResult> results = ruleOperation.VerifySingleUI(data);
    //        view.ComplianceResults.Clear();
    //        foreach (ComplianceResult cr in results)
    //        {
    //            ViewComplianceResult vcr = new ViewComplianceResult(cr);
    //            view.ComplianceResults.Add(vcr);
    //        }
    //    }
    //    public void ShowDBImage()
    //    {
    //        DBWrapper wrapper = new DBWrapper();
    //        CapturedData data = wrapper.GetSpecifiedData(Int32.Parse(view.ResultID));
    //        view.DbImage = ConvertBitmapToBitmapImage(data.Image);

    //        List<ComplianceResult> list1 = new List<ComplianceResult>();
    //        MS.Internal.SulpHur.UICompliance.ElementInformation ei = data.Ei;
    //        List<MS.Internal.SulpHur.UICompliance.ElementInformation> list = ParseTreeToList(ei);
    //        MS.Internal.SulpHur.UICompliance.ElementInformation root = list[0];
    //        ControlScreen.CurrentBit = data.Image;
    //        OverlapRule rule = new OverlapRule();
    //        List<UIComplianceResultBase> results = rule.UIVerify(list);
    //        foreach (UIComplianceResultBase b in results)
    //        {
    //            ComplianceResult cr = new ComplianceResult();
    //            if (b.Image == null)
    //            {
    //                cr.Image = DrawBitmap(data.Image, b.Controls, root);
    //            }
    //            else
    //            {
    //                cr.Image = b.Image;
    //            }
    //            cr.Message = b.Message;
    //            cr.Type = b.Type.ToString();
    //            cr.RuleName = "Tab Order";
    //            list1.Add(cr);
    //        }





    //        view.ComplianceResults.Clear();
    //        foreach (ComplianceResult cr in list1)
    //        {
    //            ViewComplianceResult vcr = new ViewComplianceResult(cr);
    //            view.ComplianceResults.Add(vcr);
    //        }
    //    }
    //}

    public enum TabName { 
        Scan,
        Result
    }
}
