using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.UICompliance;
using System.Xml.Linq;
using System.Drawing;
using System.Windows;

namespace RuleManager
{
    //public class DBWrapper:IDisposable
    //{

    //    public DBWrapper() {
    //    }

    //    public List<string> QueryAllAvailableBuild()
    //    {
    //        using (DataClasses1DataContext content = new DataClasses1DataContext())
    //        {
    //            var buildList = (from b in content.BuildInfos
    //                             select b.BuildNo).Distinct().OrderByDescending(c => c);
    //            List<string> list = new List<string>();
    //            foreach (string x in buildList)
    //            {
    //                list.Add(x);
    //            }
    //            return list;
    //        }
    //    }
    //    public List<string> QueryAllAvailableLanguage()
    //    {
    //        using (DataClasses1DataContext content = new DataClasses1DataContext())
    //        {
    //            var languageList = (from b in content.BuildInfos
    //                             select b.Language).Distinct().OrderByDescending(c => c);
    //            List<string> list = new List<string>();
    //            foreach (string x in languageList)
    //            {
    //                list.Add(x);
    //            }
    //            return list;
    //        }
    //    }
    //    public List<string> QueryAllAvailableOSType()
    //    {
    //        using (DataClasses1DataContext content = new DataClasses1DataContext())
    //        {
    //            var osList = (from b in content.Clients
    //                                select b.OSType).Distinct().OrderByDescending(c => c);
    //            List<string> list = new List<string>();
    //            foreach (string x in osList)
    //            {
    //                list.Add(x);
    //            }
    //            return list;
    //        }
    //    }
    //    public List<string> QueryAllAvailableBuildInBuildTypes() {
    //        using (DataClasses1DataContext content = new DataClasses1DataContext())
    //        {
    //            var buildList = (from b in content.BuildTypes
    //                             select b.BuildNo).Distinct().OrderByDescending(c => c);
    //            List<string> list = new List<string>();
    //            foreach (string x in buildList)
    //            {
    //                list.Add(x);
    //            }
    //            return list;
    //        }
    //    }
    //    public void PersistenceTypes(List<PageControl> list,string buildno,string inheritBuild) {
    //        using (DataClasses1DataContext dataContext = new DataClasses1DataContext())
    //        {
    //            if (dataContext.BuildTypes.Any(c => c.BuildNo == buildno)) {
    //                if (System.Windows.MessageBox.Show(string.Format("Will delete {0} info, do you want to continue",buildno),"Confirm",System.Windows.MessageBoxButton.YesNo)==MessageBoxResult.No) {
    //                    return;
    //                }
    //            }
    //            var list1 = from x in dataContext.BuildTypes
    //                        where x.BuildNo == buildno
    //                        select x;
    //            dataContext.BuildTypes.DeleteAllOnSubmit(list1);
    //            dataContext.SubmitChanges();

    //            foreach (PageControl p in list)
    //            {
    //                BuildType type = new BuildType();
    //                type.AssemblyName = p.FileName;
    //                type.TypeName = p.PageName;
    //                if (dataContext.BuildTypes.Any(c => c.AssemblyName == p.FileName && c.TypeName == p.PageName && c.BuildNo == inheritBuild))
    //                {
    //                    BuildType bt = dataContext.BuildTypes.Single(c => c.AssemblyName == p.FileName && c.TypeName == p.PageName && c.BuildNo == inheritBuild);
    //                    if (!string.IsNullOrEmpty(bt.LanuchSteps)) {
    //                        type.LanuchSteps = bt.LanuchSteps;
    //                    }
    //                    type.Mark = bt.Mark;
    //                }
    //                else {
    //                    type.Mark = "Valid UI (Missing Capture)";
    //                }
    //                type.BuildNo = buildno;
    //                dataContext.BuildTypes.InsertOnSubmit(type);
    //            }
    //            dataContext.SubmitChanges();
    //        }
    //    }

    //    public void AnalysisConfirmDialog() {
    //        int count = 0;
    //        using (DataClasses1DataContext dataContext = new DataClasses1DataContext()) {
    //            var refList = from x in dataContext.BuildTypes
    //                          where x.Mark == "Captured"
    //                          select x;
    //            foreach (var c in refList) {
    //                var validContent = (from z in dataContext.ViewAssemblySummaries
    //                                    where z.AssemblyName == c.AssemblyName && z.FullTypeName == c.TypeName
    //                                    select z.ContentID).Distinct();
    //                if (validContent.Count() == 1) {
    //                    int id=validContent.FirstOrDefault();
    //                    var name = from z in dataContext.UIContents
    //                                   where z.ContentID==id
    //                                   select z.UIName;
    //                    string namex = name.FirstOrDefault();
    //                    if (namex == "Configuration Manager") {
    //                        count++;
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    public List<PageControl> GetPagesList(string buildno)
    //    {
    //        using (DataClasses1DataContext dataContext = new DataClasses1DataContext())
    //        {
    //            List<PageControl> resultList = new List<PageControl>();
    //            var list = from x in dataContext.BuildTypes
    //                       where x.BuildNo == buildno
    //                       select x;
    //            foreach (var t in list)
    //            {
    //                PageControl p = new PageControl();
    //                p.FileName = t.AssemblyName;
    //                p.PageName = t.TypeName;
    //                p.Mark = t.Mark;
    //                resultList.Add(p);
    //            }
    //            return resultList;
    //        }
    //    }

    //    public List<ContentView> GetSpecifiedContents(string contentID) {
    //        using (DataClasses1DataContext context = new DataClasses1DataContext())
    //        {
    //            int id = Int32.Parse(contentID);
    //            List<ContentView> list = new List<ContentView>();
    //            var varList1 = from p in context.UIContents
    //                           where p.ContentID == id
    //                           select p;
    //            foreach (var c in varList1)
    //            {
    //                list.Add(
    //                    new ContentView(c.ContentID.ToString(), c.UIName,
    //                        MS.Internal.SulpHur.UICompliance.Utility.BytesToBmp_MemStream(c.UIScreenShot), c.UIContent1, c.Client.UserName));
    //                c.UIContent1.Save(@"c:\1.xml");

    //            }

    //            return list;
    //        }
    //    }

    //    public List<ContentView> GetContents(string BuildNO,int rang1,int rang2)
    //    {
    //        using (DataClasses1DataContext context = new DataClasses1DataContext())
    //        {
    //            List<ContentView> list = new List<ContentView>();
    //            var varList1 = from p in context.UIContents
    //                           where p.BuildInfo.BuildNo == BuildNO &&p.ContentID>rang1&&p.ContentID<rang2
    //                           select new ContentView(p.ContentID.ToString(), p.UIName, p.Client.UserName);
    //            foreach (var c in varList1)
    //            {
    //                list.Add(c);
    //            }
    //            return list;
    //        }
    //    }
    //    public List<ContentView> GetContents(string BuildNO, string language, string osType)
    //    {
    //        using (DataClasses1DataContext context = new DataClasses1DataContext())
    //        {
    //            List<ContentView> list = new List<ContentView>();
    //            var varList1 = (from p in context.ViewRescans
    //                            where p.BuildNo == BuildNO
    //                            select new ContentView(p.ContentID.ToString(), p.UIName, p.UserName)).Distinct();
    //            if (language != "All" && osType != "All")
    //            {
    //                varList1 = (from p in context.ViewRescans
    //                            where p.BuildNo == BuildNO && p.Language == language && p.OSType == osType
    //                            select new ContentView(p.ContentID.ToString(), p.UIName, p.UserName)).Distinct();
    //            }
    //            else if (language == "All" && osType != "All")
    //            {
    //                varList1 = (from p in context.ViewRescans
    //                            where p.BuildNo == BuildNO && p.OSType == osType
    //                            select new ContentView(p.ContentID.ToString(), p.UIName, p.UserName)).Distinct();
    //            }
    //            else if (language != "All" && osType == "All")
    //            {
    //                varList1 = (from p in context.ViewRescans
    //                            where p.BuildNo == BuildNO && p.Language == language
    //                            select new ContentView(p.ContentID.ToString(), p.UIName, p.UserName)).Distinct();
    //            }
    //            foreach (var c in varList1)
    //            {
    //                list.Add(c);
    //            }
    //            return list;
    //        }
    //    }
    //    public List<ContentView> GetContents(int rang1, int rang2)
    //    {
    //        using (DataClasses1DataContext context = new DataClasses1DataContext())
    //        {
    //            List<ContentView> list = new List<ContentView>();
    //            var varList1 = from p in context.UIContents
    //                           where p.ContentID >= rang1 && p.ContentID <= rang2
    //                           select new ContentView(p.ContentID.ToString(), p.UIName, p.Client.UserName);
    //            foreach (var c in varList1)
    //            {
    //                list.Add(c);
    //            }
    //            return list;
    //        }
    //    }
    //    public List<ContentView> GetContents(string BuildNO,int take)
    //    {
    //        using (DataClasses1DataContext context = new DataClasses1DataContext())
    //        {
    //            List<ContentView> list = new List<ContentView>();
    //            var varList1 = (from p in context.UIContents
    //                           where p.BuildInfo.BuildNo == BuildNO
    //                            select new ContentView(p.ContentID.ToString(), p.UIName, p.Client.UserName)).Take(take);
    //            foreach (var c in varList1)
    //            {
    //                list.Add(c);
    //            }
    //            return list;
    //        }
    //    }
    //    public List<ContentView> GetContents(string BuildNO)
    //    {
    //        using (DataClasses1DataContext context = new DataClasses1DataContext())
    //        {
    //            List<ContentView> list = new List<ContentView>();
    //            var varList1 = (from p in context.UIContents
    //                            where p.BuildInfo.BuildNo == BuildNO
    //                            select new ContentView(p.ContentID.ToString(), p.UIName, p.Client.UserName));
    //            foreach (var c in varList1)
    //            {
    //                list.Add(c);
    //            }
    //            return list;
    //        }
    //    }
    //    public ContentView GetContents(int resultID)
    //    {
    //        using (DataClasses1DataContext context = new DataClasses1DataContext())
    //        {
    //            try
    //            {
    //                if (!context.Results.Any(c => c.ResultID == resultID)) {
    //                    return null;
    //                }
    //                int content = context.Results.First(c => c.ResultID == resultID).UIContent.ContentID;
    //                var varList1 = (from p in context.UIContents
    //                                where p.ContentID == content
    //                                select new ContentView(p.ContentID.ToString(), p.UIName, p.Client.UserName));
    //                return varList1.FirstOrDefault();
    //            }
    //            catch (Exception e)
    //            {
    //                System.Windows.MessageBox.Show(e.ToString());
    //                return null;
    //            }
    //        }
    //    }
    //    public Bitmap GetBitmapByID(int id)
    //    {
    //        using (DataClasses1DataContext context = new DataClasses1DataContext())
    //        {
    //            var bit = (from x in context.UIContents
    //                       where x.ContentID == id
    //                       select MS.Internal.SulpHur.UICompliance.Utility.BytesToBmp_MemStream(x.UIScreenShot)).FirstOrDefault();
    //            return bit;
    //        }
    //    }
    //    public Bitmap GetBitmapByIDENU(int id)
    //    {
    //        using (DataClasses1DataContext context = new DataClasses1DataContext())
    //        {
    //            var list = (from x in context.ViewAssemblySummaries
    //                        where x.ContentID == id && x.IsPageIdentifier == true
    //                       select new { x.AssemblyName, x.FullTypeName }).Distinct();

    //            string assembly = string.Empty;
    //            string fullType = string.Empty;
    //            foreach (var x in list) {
    //                assembly = x.AssemblyName;
    //                fullType = x.FullTypeName;
    //            }
    //            var bit = (from x in context.ViewENUBitmaps
    //                       where x.Language=="ENU"&&x.AssemblyName==assembly&&x.FullTypeName==fullType
    //                       select MS.Internal.SulpHur.UICompliance.Utility.BytesToBmp_MemStream(x.UIScreenShot)).FirstOrDefault();
    //            return bit;
    //        }
    //    }
    //    public ElementInformation GetElementByID(int id)
    //    {
    //        using (DataClasses1DataContext context = new DataClasses1DataContext())
    //        {
    //            var bit = (from x in context.UIContents
    //                       where x.ContentID == id
    //                       select MS.Internal.SulpHur.Utilities.ExtensionMethods.FromXElement<ElementInformation>(x.UIContent1)).FirstOrDefault();
    //            return bit;
    //        }
    //    }
    //    public XElement GetElementByID1(int id)
    //    {
    //        using (DataClasses1DataContext context = new DataClasses1DataContext())
    //        {
    //            var bit = (from x in context.UIContents
    //                       where x.ContentID == id
    //                       select x.UIContent1).FirstOrDefault();
    //            return bit;
    //        }
    //    }
    //    public CapturedData GetSpecifiedData(int resultID)
    //    {
    //        using (DataClasses1DataContext dataContext = new DataClasses1DataContext())
    //        {

    //            int content = dataContext.Results.First(c => c.ResultID == resultID).UIContent.ContentID;

    //            CapturedData data = new CapturedData();
    //            var varList = from r in dataContext.UIContents
    //                          where r.ContentID == content
    //                          select new CapturedData(MS.Internal.SulpHur.UICompliance.Utility.BytesToBmp_MemStream(r.UIScreenShot), r.UIContent1);

    //            foreach (CapturedData d in varList)
    //            {
    //                data = d;
    //            }
    //            XElement xe = MS.Internal.SulpHur.Utilities.ExtensionMethods.ToXElement<ElementInformation>(data.Ei);
    //            xe.Save(@"c:\1.xml");
    //            return data;
    //        }
    //    }

    //    public List<PageControl> GetAssemblyandTypesInDB()
    //    {
    //        using (DataClasses1DataContext dataContext = new DataClasses1DataContext())
    //        {
    //            var varList = from x in dataContext.AssemblyInfos
    //                          select new PageControl(x.FullTypeName, x.AssemblyName);
    //            List<PageControl> list = new List<PageControl>();
    //            foreach (PageControl p in varList)
    //            {
    //                list.Add(p);
    //            }
    //            return list;
    //        }
    //    }

    //    public XElement QueryAllDirtyUI()
    //    {
    //        using (DataClasses1DataContext dataContext = new DataClasses1DataContext())
    //        {
    //            var varList = from r in dataContext.UIContents
    //                          where r.ContentID == 1548
    //                          select r;
    //            return varList.First().UIContent1;
    //        }
    //    }

    //    public void Dispose()
    //    {
    //    }
    //}

    public class UIANDRule
    {
        public UIANDRule(XElement ele, string ruleName, bool isWebUI, Bitmap bit, int uiid)
        {
            this.element = ele;
            this.ruleName = ruleName;
            this.isWebUI = isWebUI;
            this.bitmap = bit;
            this.uiid = uiid;
        }

        public UIANDRule(XElement ele, bool isWebUI, Bitmap bit, int uiid)
        {
            this.element = ele;
            this.isWebUI = isWebUI;
            this.bitmap = bit;
            this.uiid = uiid;
        }
        XElement element;

        Bitmap bitmap;

        public Bitmap Bitmap
        {
            get { return bitmap; }
            set { bitmap = value; }
        }

        public XElement Element
        {
            get { return element; }
            set { element = value; }
        }
        string ruleName;

        public string RuleName
        {
            get { return ruleName; }
            set { ruleName = value; }
        }

        private bool isWebUI;

        public bool IsWebUI
        {
            get { return isWebUI; }
            set { isWebUI = value; }
        }

        int uiid;

        public int Uiid
        {
            get { return uiid; }
            set { uiid = value; }
        }

    }
}
