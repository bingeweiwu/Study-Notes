using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Xml.Linq;
using System.IO;

namespace SulpHurServiceAbstract
{
    public class UIRecord
    {
        public int ResultID { get; set; }
        public int ContentID { get; set; }
        public string BuildNo { get; set; }
        public string BuildLanguage { get; set; }
        public string RuleName { get; set; }
        public string ResultType { get; set; }
        public string PageTitle { get; set; }
        public string UserName { get; set; }
        public string OSType { get; set; }
        public DateTime DateUploaded { get; set; }
        public DateTime DateChecked { get; set; }

        public void QueryRecordByResultID(int resultid)
        {
            ISulpHurTable table = SulpHurTableFactoryBase.Instance().GetSulpHurTable();
            ResultInfo currentResult = table.QueryTable<ResultInfo>("select * from results where resultid=" + resultid)[0];
            RuleInfo currentRule = table.QueryTable<RuleInfo>("select * from rules where ruleid=" + currentResult.RuleID)[0];
            UIContentInfo currentUI = table.QueryTable<UIContentInfo>("select * from uicontents where contentid=" + currentResult.ContentID)[0];
            SulpHurBuildInfo currentBuildRecord = table.QueryTable<SulpHurBuildInfo>("select * from buildinfo where buildid=" + currentUI.BuildID)[0];
            SulpHurClientInfo currentClient = table.QueryTable<SulpHurClientInfo>("select * from clients where clientid=" + currentUI.ClientID)[0];

            this.OSType = currentClient.OSType;
            this.PageTitle = currentUI.UIName;
            this.ContentID = currentUI.ID;
            this.ResultType = currentResult.ResultType;
            this.RuleName = currentRule.RuleName;
            this.UserName = currentClient.UserName;
            this.BuildNo = currentBuildRecord.BuildNo;
            this.BuildLanguage = currentBuildRecord.Language;

        }
        public void QueryRecordByContentID(int contentid)
        {
            ISulpHurTable table = SulpHurTableFactoryBase.Instance().GetSulpHurTable();

            UIContentInfo currentUI = table.QueryTable<UIContentInfo>("select * from uicontents where contentid=" + contentid)[0];
            SulpHurBuildInfo currentBuildRecord = table.QueryTable<SulpHurBuildInfo>("select * from buildinfo where buildid=" + currentUI.BuildID)[0];
            SulpHurClientInfo currentClient = table.QueryTable<SulpHurClientInfo>("select * from clients where clientid=" + currentUI.ClientID)[0];

            this.OSType = currentClient.OSType;
            this.PageTitle = currentUI.UIName;
            this.ContentID = currentUI.ID;
            this.UserName = currentClient.UserName;
            //this.BuildLanguage = currentBuildRecord.BuildNo;
            //v-danpgu
            this.BuildNo = currentBuildRecord.BuildNo;
            this.BuildLanguage = currentBuildRecord.Language;
        }
        public List<UIRecord> QueryUIRecords(string buildno, string buildLanguage = "", string osType = "")
        {
            List<UIRecord> records = new List<UIRecord>();
            string sql = "select A.uiname,C.username,C.ostype,A.contentid,B.buildno,B.language from uicontents as A join buildinfo as B on A.buildid=B.buildid join clients as C on A.clientid=C.clientid where B.buildno='" + buildno + "'";
            if (buildLanguage != "" && osType != "")
            {
                sql += " and B.language='" + buildLanguage + "' and C.ostype='" + osType + "'";
            }
            if (buildLanguage != "" && osType == "")
            {
                sql += " and B.language='" + buildLanguage + "'";
            }
            if (buildLanguage == "" && osType != "")
            {
                sql += " and C.ostype='" + osType + "'";
            }
            ISulpHurTable table = SulpHurTableFactoryBase.Instance().GetSulpHurTable();

            DataTable tContents = table.QueryTable(sql);
            foreach (DataRow row in tContents.Rows)
            {
                UIRecord rTemp = new UIRecord();
                rTemp.ContentID = (int)row["contentid"];
                rTemp.BuildNo = (string)row["buildno"];
                rTemp.BuildLanguage = (string)row["Language"];
                rTemp.UserName = (string)row["UserName"];
                rTemp.PageTitle = (string)row["uiname"];
                rTemp.OSType = (string)row["ostype"];
                records.Add(rTemp);
            }
            return records;
        }
    }
    public class RuleInfo : Row
    {
        public string RuleName { get; set; }
        public bool IsEnabled { get; set; }
        public string Descriptions { get; set; }
        public List<RuleInfo> AvailableRules
        {
            get
            {
                List<RuleInfo> rulesTemp = new List<RuleInfo>();
                ISulpHurTable client = SulpHurTableFactoryBase.Instance().GetSulpHurTable();
                rulesTemp = client.QueryTable<RuleInfo>("select * from rules where isobsoluted=0");
                return rulesTemp;
            }
        }
        public override string InsertSQL
        {
            get { throw new NotImplementedException(); }
        }

        public override List<SqlParameter> InsertParameters
        {
            get { throw new NotImplementedException(); }
        }

        public override void FillRowFromSQL(DataRow data)
        {
            this.ID = (int)data["ruleid"];
            this.RuleName = (string)data["RuleName"];
            this.IsEnabled = (bool)data["IsEnabled"];
            if (data["RuleDesc"] != DBNull.Value)
            {
                this.Descriptions = (string)data["RuleDesc"];
            }
        }

        public override void ConcreateRow(IDataAdapter adapter)
        {
            throw new NotImplementedException();
        }
    }
    public class ResultInfo : Row
    {
        public int ContentID { get; set; }
        public int RuleID { get; set; }
        public string ResultType { get; set; }
        public string ResultLog { get; set; }
        public ResultInfo() { }
        public override string InsertSQL
        {
            get { throw new NotImplementedException(); }
        }

        public override List<SqlParameter> InsertParameters
        {
            get { throw new NotImplementedException(); }
        }

        public override void FillRowFromSQL(DataRow data)
        {
            this.ID = (int)data["resultid"];
            this.ContentID = (int)data["ContentID"];
            this.RuleID = (int)data["RuleID"];
            this.ResultType = (string)data["ResultType"];
            this.ResultLog = (string)data["ResultLog"];
        }

        public override void ConcreateRow(IDataAdapter adapter)
        {
            throw new NotImplementedException();
        }
    }
    public class UIContentInfo : Row
    {
        public Guid GUID { get; set; }
        public int ClientID { get; set; }
        public int BuildID { get; set; }
        public string UIName { get; set; }
        public UIContentInfo(int contentID)
        {
            this.ID = contentID;
        }
        public UIContentInfo() : base() { }
        public byte[] ScreenShot
        {
            get
            {
                ISulpHurTable client = SulpHurTableFactoryBase.Instance().GetSulpHurTable();
                DataTable dt = client.QueryTable("select uiscreenshot from uicontents where contentid='" + this.ID + "'");
                if (dt.Rows.Count == 1)
                {
                    byte[] bmpBytes = (byte[])dt.Rows[0]["uiscreenshot"];
                    return bmpBytes;
                    //using (MemoryStream ms = new MemoryStream(bmpBytes))
                    //{
                    //    System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
                    //    return (Bitmap)img;
                    //}
                }
                return null;
            }
        }
        public byte[] ScreenShotENU
        {
            get
            {
                ISulpHurTable client = SulpHurTableFactoryBase.Instance().GetSulpHurTable();
                DataTable dt = client.QueryTable("select C.buildid, B.assemblyname,B.fulltypename from assemblylink as A join assemblyinfo as B on A.typeid=B.typeid join uicontents as C on A.contentid=C.contentid where  A.contentid='" + this.ID + "'");
                if (dt.Rows.Count > 0)
                {
                    string assemblyname = (string)dt.Rows[0]["assemblyname"];
                    string typeName = (string)dt.Rows[0]["fulltypename"];
                    int buildid = (int)dt.Rows[0]["buildid"];

                    DataTable dt1 = client.QueryTable("select C.uiscreenshot from assemblylink as A join assemblyinfo as B on A.typeid=B.typeid join uicontents as C on A.contentid=C.contentid join buildinfo as D on C.buildid=D.buildid where B.assemblyname='" + assemblyname + "' and B.fulltypename='" + typeName + "' and C.buildid='" + buildid + "' and D.language='ENU'");
                    if (dt1.Rows.Count >= 1)
                    {
                        byte[] bmpBytes = (byte[])dt1.Rows[0]["uiscreenshot"];
                        return bmpBytes;
                    }
                    else
                    {
                        DataTable dt2 = client.QueryTable("select C.contentid,D.buildno from assemblylink as A join assemblyinfo as B on A.typeid=B.typeid join uicontents as C on A.contentid=C.contentid join buildinfo as D on C.buildid=D.buildid where B.assemblyname='" + assemblyname + "' and B.fulltypename='" + typeName + "' and D.language='ENU' order by D.buildno desc");
                        int contentidTemp = (int)dt2.Rows[0]["contentid"];
                        DataTable dt3 = client.QueryTable("select uiscreenshot from uicontents where contentid='" + contentidTemp + "'");
                        if (dt3.Rows.Count == 1)
                        {
                            byte[] bmpBytes = (byte[])dt3.Rows[0]["uiscreenshot"];
                            return bmpBytes;
                        }
                    }
                }
                return null;
            }
        }
        public XElement xml { get; set; }

        public override string InsertSQL
        {
            get { throw new NotImplementedException(); }
        }

        public override List<SqlParameter> InsertParameters
        {
            get { throw new NotImplementedException(); }
        }

        public override void FillRowFromSQL(DataRow data)
        {
            this.ID = (int)data["contentid"];
            this.GUID = (Guid)data["GUID"];
            this.ClientID = (int)data["ClientID"];
            this.BuildID = (int)data["BuildID"];
            this.UIName = (string)data["UIName"];
        }

        public override void ConcreateRow(IDataAdapter adapter)
        {
            throw new NotImplementedException();
        }
    }
    public class AssemblyInfo : Row
    {
        public AssemblyInfo() : base() { }
        public AssemblyInfo(string FullTypeName, string FileName, string TypeName)
            : base()
        {
            this.FullTypeName = FullTypeName;
            this.FileName = FileName;
            this.TypeName = TypeName;
        }
        public string AssemblyName { get; set; }
        public string FullTypeName { get; set; }
        public string TypeName { get; set; }
        public string FileName { get; set; }
        public override string InsertSQL
        {
            get { throw new NotImplementedException(); }
        }

        public override List<SqlParameter> InsertParameters
        {
            get { throw new NotImplementedException(); }
        }

        public override void FillRowFromSQL(DataRow data)
        {
            this.ID = (int)data["typeid"];
            this.AssemblyName = (string)data["AssemblyName"];
            this.FullTypeName = (string)data["FullTypeName"];
            this.TypeName = (string)data["TypeName"];
        }

        public override void ConcreateRow(IDataAdapter adapter)
        {
            throw new NotImplementedException();
        }
    }

    public class BuildType : Row
    {
        public BuildType() : base() { }
        public BuildType(string AssemblyName, string TypeName, string BuildNo, string Mark, string LanuchSteps)
            : base()
        {
            this.AssemblyName = AssemblyName;
            this.TypeName = TypeName;
            this.BuildNo = BuildNo;
            this.Mark = Mark;
            this.LanuchSteps = LanuchSteps;
        }
        public string AssemblyName { get; set; }
        public string TypeName { get; set; }
        public string BuildNo { get; set; }
        public string Mark { get; set; }
        public string LanuchSteps { get; set; }


        public override void FillRowFromSQL(DataRow data)
        {
            this.ID = (int)data["RecordID"];
            this.AssemblyName = (string)data["AssemblyName"];
            this.TypeName = (string)data["TypeName"];
            this.BuildNo = (string)data["BuildNo"];
            this.Mark = (string)data["Mark"];
            this.LanuchSteps = (string)data["LanuchSteps"];
        }

        public override string InsertSQL
        {
            get { return "insert into BuildTypes (AssemblyName,TypeName,BuildNo,Mark,LanuchSteps) values(@AssemblyName,@TypeName,@BuildNo,@Mark,@LanuchSteps)"; }
        }

        public override string UpdateSQL
        {
            get { throw new NotImplementedException(); }
        }
        public override string DeleteSQL
        {
            get
            {
                return "delete from BuildTypes where RecordID='" + this.ID + "'";
            }
        }
        public override List<SqlParameter> InsertParameters
        {
            get
            {
                List<SqlParameter> parameters = new List<SqlParameter>();
                SqlParameter AssemblyNamePara = new SqlParameter("@AssemblyName", System.Data.DbType.String);
                AssemblyNamePara.Value = this.AssemblyName;
                parameters.Add(AssemblyNamePara);

                SqlParameter TypeNamePara = new SqlParameter("@TypeName", System.Data.DbType.String);
                TypeNamePara.Value = this.TypeName;
                parameters.Add(TypeNamePara);

                SqlParameter BuildNoPara = new SqlParameter("@BuildNo", System.Data.DbType.String);
                BuildNoPara.Value = this.BuildNo;
                parameters.Add(BuildNoPara);

                SqlParameter MarkPara = new SqlParameter("@Mark", System.Data.DbType.String);
                MarkPara.Value = this.Mark;
                parameters.Add(MarkPara);

                SqlParameter LanuchStepsPara = new SqlParameter("@LanuchSteps", System.Data.DbType.String);
                LanuchStepsPara.Value = this.LanuchSteps;
                parameters.Add(LanuchStepsPara);

                return parameters;
            }
        }
        public override void ConcreateRow(IDataAdapter adapter)
        {
            BuildType eTemp = adapter.GetRow<BuildType>();
            this.ID = eTemp.ID;
            this.AssemblyName = eTemp.AssemblyName;
            this.TypeName = eTemp.TypeName;
            this.BuildNo = eTemp.BuildNo;
            this.Mark = eTemp.Mark;
            this.LanuchSteps = eTemp.LanuchSteps;
        }
    }

    public class LogException : Row
    {
        public int ClientID { get; set; }
        public int BuildID { get; set; }
        public string ExceptionContent { get; set; }
        public DateTime InsertTime { get; set; }
        public DateTime LastModifyTime { get; set; }
        public int ExceptionCount { get; set; }
        public override void FillRowFromSQL(DataRow data)
        {
            this.ID = (int)data["LogID"];
            this.ClientID = (int)data["ClientID"];
            this.BuildID = (int)data["BuildID"];
            this.ExceptionContent = (string)data["ExceptionContent"];
            this.ExceptionCount = (int)data["ExceptionCount"];
            this.InsertTime = (DateTime)data["InsertTime"];
            this.LastModifyTime = (DateTime)data["LastModifyTime"];
        }
        public override string InsertSQL
        {
            get { return "insert into logexception (buildid,clientid,exceptioncontent,inserttime,lastmodifytime,exceptioncount) values(@buildid,@clientid,@exceptioncontent,@inserttime,@lastmodifytime,@exceptioncount)"; }
        }
        public override string UpdateSQL
        {
            get
            {
                return "update logexception set lastmodifytime='"
                    + DateTime.Now
                    + "',exceptioncount='" + (++this.ExceptionCount)
                    + "' where logid='" + this.ID + "'";
            }
        }
        public override string DeleteSQL
        {
            get
            {
                return "delete from logexception where logid='" + this.ID + "'";
            }
        }
        public override List<SqlParameter> InsertParameters
        {
            get
            {
                List<SqlParameter> parameters = new List<SqlParameter>();
                SqlParameter pBuildID = new SqlParameter("@buildid", System.Data.DbType.Int32);
                pBuildID.Value = this.BuildID;
                parameters.Add(pBuildID);
                SqlParameter pClientID = new SqlParameter("@clientid", System.Data.DbType.Int32);
                pClientID.Value = this.ClientID;
                parameters.Add(pClientID);
                SqlParameter pExceptionContent = new SqlParameter("@exceptioncontent", System.Data.DbType.String);
                pExceptionContent.Value = this.ExceptionContent;
                parameters.Add(pExceptionContent);
                SqlParameter pInsertTime = new SqlParameter("@inserttime", System.Data.DbType.DateTime);
                pInsertTime.Value = this.InsertTime;
                parameters.Add(pInsertTime);
                SqlParameter pLastModityTime = new SqlParameter("@lastmodifytime", System.Data.DbType.DateTime);
                pLastModityTime.Value = this.LastModifyTime;
                parameters.Add(pLastModityTime);
                SqlParameter pExceptionCount = new SqlParameter("@exceptioncount", System.Data.DbType.Int32);
                pExceptionCount.Value = this.ExceptionCount;
                parameters.Add(pExceptionCount);
                return parameters;
            }
        }
        public override void ConcreateRow(IDataAdapter adapter)
        {
            LogException eTemp = adapter.GetRow<LogException>();
            this.ClientID = eTemp.ClientID;
            this.BuildID = eTemp.BuildID;
            this.ExceptionCount = eTemp.ExceptionCount;
            this.InsertTime = eTemp.InsertTime;
            this.LastModifyTime = eTemp.LastModifyTime;
            this.ExceptionCount = eTemp.ExceptionCount;
        }
        public override bool IsExist
        {
            get
            {
                ISulpHurTable client = SulpHurTableFactoryBase.Instance().GetSulpHurTable();

                string content = this.ExceptionContent;
                List<SqlParameter> pList = new List<SqlParameter>();
                SqlParameter pBuildID = new SqlParameter("@buildid", System.Data.DbType.Int32);
                pBuildID.Value = this.BuildID;
                pList.Add(pBuildID);
                SqlParameter pClientID = new SqlParameter("@clientid", System.Data.DbType.Int32);
                pClientID.Value = this.ClientID;
                pList.Add(pClientID);
                SqlParameter pExceptionContent = new SqlParameter("@exceptioncontent", System.Data.DbType.String);
                pExceptionContent.Value = this.ExceptionContent;
                pList.Add(pExceptionContent);

                List<LogException> scList = client.QueryTable<LogException>("select * from logexception where buildid=@buildid and clientid=@clientid and exceptioncontent=@exceptioncontent", pList);

                if (scList.Count == 1)
                {
                    base.ID = scList[0].ID;
                    this.ExceptionCount = scList[0].ExceptionCount;
                    return true;
                }

                return false;
            }
            set
            {
                base.IsExist = value;
            }
        }
    }
    public class SulpHurClientInfo : SulpHurClientIdentity
    {
        public SulpHurClientInfo() : base() { }

        public string IPAddress { get; set; }
        public string OSLanguage { get; set; }
        public string MachineName { get; set; }
        public string OSType { get; set; }
        public List<string> AvailableOSTypes
        {
            get
            {
                List<string> osTemp = new List<string>();
                ISulpHurTable client = SulpHurTableFactoryBase.Instance().GetSulpHurTable();
                DataTable dt = client.QueryTable("select distinct ostype from clients order by ostype asc");
                foreach (DataRow dr in dt.Rows)
                {
                    osTemp.Add((string)dr["ostype"]);
                }
                return osTemp;
            }
        }
        public override void FillRowFromSQL(DataRow data)
        {
            this.IPAddress = (string)data["IPAddress"];
            this.MachineName = (string)data["MachineName"];
            this.OSLanguage = (string)data["OSLanguage"];
            this.OSType = (string)data["OSType"];
            base.FillRowFromSQL(data);
        }

        public override string InsertSQL
        {
            get { return "insert into clients (username,machinename,ipaddress,oslanguage,ostype,macaddress,datecreated) values(@username,@machinename,@ipaddress,@oslanguage,@ostype,@macaddress,@datecreated)"; }
        }

        public override List<SqlParameter> InsertParameters
        {
            get
            {
                List<SqlParameter> parameters = new List<SqlParameter>();
                SqlParameter pUserName = new SqlParameter("@username", System.Data.DbType.String);
                pUserName.Value = this.UserName;
                parameters.Add(pUserName);
                SqlParameter pMachineName = new SqlParameter("@machinename", System.Data.DbType.String);
                pMachineName.Value = this.MachineName;
                parameters.Add(pMachineName);
                SqlParameter pIPAddress = new SqlParameter("@ipaddress", System.Data.DbType.String);
                pIPAddress.Value = this.IPAddress;
                parameters.Add(pIPAddress);
                SqlParameter pOSLanguage = new SqlParameter("@oslanguage", System.Data.DbType.String);
                pOSLanguage.Value = this.OSLanguage;
                parameters.Add(pOSLanguage);
                SqlParameter pOSType = new SqlParameter("@ostype", System.Data.DbType.String);
                pOSType.Value = this.OSType;
                parameters.Add(pOSType);
                SqlParameter pMacAddress = new SqlParameter("@macaddress", System.Data.DbType.String);
                pMacAddress.Value = this.MacAddress;
                parameters.Add(pMacAddress);
                SqlParameter pDateCreated = new SqlParameter("@datecreated", System.Data.DbType.DateTime);
                pDateCreated.Value = DateTime.Now;
                parameters.Add(pDateCreated);

                return parameters;
            }
        }

        public override void ConcreateRow(IDataAdapter adapter)
        {
            SulpHurClientInfo iTemp = adapter.GetRow<SulpHurClientInfo>();
            this.IPAddress = iTemp.IPAddress;
            this.OSLanguage = iTemp.OSLanguage;
            this.MachineName = iTemp.MachineName;
            base.ConcreateRow(adapter);
        }
    }
    public class SulpHurBuildInfo : Row
    {

        public string BuildNo { get; set; }
        public string Language { get; set; }
        public List<string> AvailableBuilds
        {
            get
            {
                List<string> buildsTemp = new List<string>();
                ISulpHurTable client = SulpHurTableFactoryBase.Instance().GetSulpHurTable();
                DataTable dt = client.QueryTable("select distinct buildno from buildinfo order by buildno desc");
                foreach (DataRow dr in dt.Rows)
                {
                    buildsTemp.Add((string)dr["BuildNo"]);
                }
                return buildsTemp;
            }
        }
        public List<string> AvailableBuildLanguages
        {
            get
            {
                List<string> lanTemp = new List<string>();
                ISulpHurTable client = SulpHurTableFactoryBase.Instance().GetSulpHurTable();
                DataTable dt = client.QueryTable("select distinct language from buildinfo order by language asc");
                foreach (DataRow dr in dt.Rows)
                {
                    lanTemp.Add((string)dr["language"]);
                }
                return lanTemp;
            }
        }
        public override bool IsExist
        {
            get
            {
                ISulpHurTable client = SulpHurTableFactoryBase.Instance().GetSulpHurTable();

                List<SulpHurBuildInfo> scList = client.QueryTable<SulpHurBuildInfo>("select * from buildinfo where buildno='" + this.BuildNo + "' and language='" + this.Language + "'");

                //workaround for there's more than 1 records in db which buildinfo and language are same
                //if (scList.Count == 1)
                //System.Diagnostics.Trace.WriteLine("list count:" + scList.Count);
                if (scList.Count >= 1)
                {
                    base.ID = scList[0].ID;
                    //System.Diagnostics.Trace.WriteLine("id:" + scList[0].ID);
                    return true;
                }

                return false;
            }
            set
            {
                base.IsExist = value;
            }
        }
        public override int ID
        {
            get
            {
                if (base.ID == -1)
                {
                    //System.Diagnostics.Trace.WriteLine("id=-1");
                    ISulpHurTable client = SulpHurTableFactoryBase.Instance().GetSulpHurTable();

                    List<SulpHurBuildInfo> scList = client.QueryTable<SulpHurBuildInfo>("select * from buildinfo where buildno='" + this.BuildNo + "' and language='" + this.Language + "'");
                    //System.Diagnostics.Trace.WriteLine("listcount:" + scList.Count);
                    if (scList.Count == 1)
                    {
                        base.ID = scList[0].ID;
                    }
                }
                return base.ID;
            }
            set
            {
                base.ID = value;
            }
        }

        public override string InsertSQL
        {
            get { return "insert into buildinfo (buildno,language,datecreated) values(@buildno,@language,@datecreated)"; }
        }

        public override List<SqlParameter> InsertParameters
        {
            get
            {
                List<SqlParameter> parameters = new List<SqlParameter>();
                SqlParameter pBuildNo = new SqlParameter("@buildno", System.Data.DbType.String);
                pBuildNo.Value = this.BuildNo;
                parameters.Add(pBuildNo);
                SqlParameter pLanguage = new SqlParameter("@language", System.Data.DbType.String);
                pLanguage.Value = this.Language;
                parameters.Add(pLanguage);
                SqlParameter pDateCreated = new SqlParameter("@datecreated", System.Data.DbType.DateTime);
                pDateCreated.Value = DateTime.Now;
                parameters.Add(pDateCreated);

                return parameters;
            }
        }

        public override void FillRowFromSQL(DataRow data)
        {
            this.ID = (int)data["BuildID"];
            this.BuildNo = (string)data["BuildNo"];
            this.Language = (string)data["Language"];
        }

        public override void ConcreateRow(IDataAdapter adapter)
        {
            SulpHurBuildInfo iTemp = adapter.GetRow<SulpHurBuildInfo>();
            this.BuildNo = iTemp.BuildNo;
            this.Language = iTemp.Language;
        }
    }
    public class ConsoleTypesInfo : Row
    {
        public override string InsertSQL
        {
            get { throw new NotImplementedException(); }
        }

        public override List<SqlParameter> InsertParameters
        {
            get { throw new NotImplementedException(); }
        }

        public override void FillRowFromSQL(DataRow data)
        {
            throw new NotImplementedException();
        }

        public override void ConcreateRow(IDataAdapter adapter)
        {
            throw new NotImplementedException();
        }
    }
    public class SulpHurClientIdentity : Row
    {
        public SulpHurClientIdentity() : base() { }
        public string UserName { get; set; }
        public string OSType { get; set; }
        public string MacAddress { get; set; }
        public override bool IsExist
        {
            get
            {
                ISulpHurTable client = SulpHurTableFactoryBase.Instance().GetSulpHurTable();

                List<SulpHurClientIdentity> scList = client.QueryTable<SulpHurClientIdentity>("select * from clients where macaddress='" + this.MacAddress + "' and username='" + this.UserName + "' and osType='" + this.OSType + "'");

                if (scList.Count == 1)
                {
                    base.ID = scList[0].ID;
                    return true;
                }

                return false;
            }
            set
            {
                base.IsExist = value;
            }
        }
        public override int ID
        {
            get
            {
                if (base.ID == -1)
                {
                    ISulpHurTable client = SulpHurTableFactoryBase.Instance().GetSulpHurTable();

                    List<SulpHurClientIdentity> scList = client.QueryTable<SulpHurClientIdentity>("select * from clients where macaddress='" + this.MacAddress + "' and username='" + this.UserName + "' and osType='" + this.OSType + "'");
                    if (scList.Count == 1)
                    {
                        base.ID = scList[0].ID;
                    }
                }
                return base.ID;
            }
            set
            {
                base.ID = value;
            }
        }

        public override void FillRowFromSQL(DataRow data)
        {
            this.ID = (int)data["ClientID"];
            this.MacAddress = (string)data["MacAddress"];
            this.OSType = (string)data["OSType"];
            this.UserName = (string)data["UserName"];
        }
        public override string InsertSQL
        {
            get { return ""; }
        }
        public override List<SqlParameter> InsertParameters
        {
            get
            {
                return new List<SqlParameter>();
            }
        }
        public override void ConcreateRow(IDataAdapter adapter)
        {
            SulpHurClientIdentity iTemp = adapter.GetRow<SulpHurClientIdentity>();
            this.MacAddress = iTemp.MacAddress;
            this.OSType = iTemp.OSType;
            this.UserName = iTemp.UserName;
        }
    }
    public abstract class SulpHurTableFactoryBase : BaseFactory
    {
        private static SulpHurTableFactoryBase tablefactory = null;
        public static SulpHurTableFactoryBase Instance()
        {
            if (tablefactory == null)
            {
                tablefactory = (SulpHurTableFactoryBase)BaseFactory.Instance(SulpHurTableFactoryBase.AssemblySettingName, SulpHurTableFactoryBase.ClassNameSettingName, SulpHurTableFactoryBase.DefaultAssemblyName, SulpHurTableFactoryBase.DefaultClassName);
            }

            return tablefactory;
        }

        public abstract ISulpHurTable GetSulpHurTable();
        public abstract IBuildClean GetBuildClean();

        private static string AssemblySettingName
        {
            get { return "SulpHurTableFactoryAssembly"; }
        }

        private static string ClassNameSettingName
        {
            get { return "SulpHurTableFactoryClassName"; }
        }

        private static string DefaultAssemblyName
        {
            get { return "SulpHurServiceImp.dll"; }
        }

        private static string DefaultClassName
        {
            get { return "SulpHurServiceImplements.SulpHurTableFactory"; }
        }
    }
    public interface ISulpHurTable
    {
        List<T> QueryTable<T>(string sql) where T : Row;
        List<T> QueryTable<T>(string sql, List<SqlParameter> p) where T : Row;
        DataTable QueryTable(string sql);
        object ExecuteScalar(string sql);
        bool InsertRow<T>(T r) where T : Row;
        bool UpdateRow<T>(T r) where T : Row;
        bool Delete<T>(T r) where T : Row;
    }
    public interface IDataAdapter
    {
        T GetRow<T>() where T : Row;
    }
    public abstract class Row
    {
        public Row()
        {
            ID = -1;
        }
        public virtual int ID { get; set; }
        public virtual bool IsExist { get; set; }
        public abstract string InsertSQL { get; }
        public virtual string UpdateSQL { get; set; }
        public virtual string DeleteSQL { get; set; }
        public abstract List<SqlParameter> InsertParameters { get; }
        public abstract void FillRowFromSQL(DataRow data);
        public abstract void ConcreateRow(IDataAdapter adapter);
    }
}
