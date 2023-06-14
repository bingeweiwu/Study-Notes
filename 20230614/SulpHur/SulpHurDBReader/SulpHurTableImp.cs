using System;
using SulpHurServiceAbstract;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace SulpHurServiceImplements
{
    public class SulpHurTableFactory : SulpHurTableFactoryBase
    {

        public override ISulpHurTable GetSulpHurTable()
        {
            return new SulpHurTable();
        }

        public override IBuildClean GetBuildClean()
        {
            return new BuildCleaner();
        }
    }

    public class BuildCleaner : IBuildClean
    {
        public void CleanBuild(string buildno)
        {
            // 1. Query UI Content ID
            string sql = string.Format("select A.contentid from uicontents as A join Buildinfo as B on A.buildid=B.buildid where B.buildno={0}", buildno);
            ISulpHurTable table = SulpHurTableFactoryBase.Instance().GetSulpHurTable();
            DataTable dt = table.QueryTable(sql);
            List<int> contentIDList = new List<int>();
            foreach (DataRow r in dt.Rows)
            {
                contentIDList.Add((int)r["contentid"]);
            }

            foreach (int contentID in contentIDList)
            {
                // 2. Query AssemblyLinkIDs
                string sql1 = string.Format("select assemblylinkid from assemblylink where contentid={0}", contentID);
                DataTable dt1 = table.QueryTable(sql1);

                string linkIDS = "";
                foreach (DataRow r in dt1.Rows)
                {
                    linkIDS += r[0].ToString() + "|";
                }
                if (linkIDS != "")
                {
                    linkIDS = linkIDS.Substring(0, linkIDS.Length - 1);
                    linkIDS = "'" + linkIDS + "'";
                }
                else
                {
                    linkIDS = "''";
                }

                // 2. Insert UI to DeletedContent Table
                string insertSql = string.Format("INSERT INTO [SulpHur].[dbo].[DeletedContents]" +
               " ([ContentID],[GUID],[ClientID],[BuildID],[UIName],[UIContent],[UIScreenShot],[IsWebUI],[DateUploaded]"
               + ",[TraceID],[Reserve1],[Reserve2],[Reserve3],[Reserve4],[Reserve5],[LaunchedFrom],[WindowHierarchy]"
               + ",[AssemblyLinkIDs]) "
               + "SELECT [ContentID],[GUID],[ClientID],[BuildID],[UIName],[UIContent],[UIScreenShot],[IsWebUI]"
               + ",[DateUploaded],[TraceID],[Reserve1],[Reserve2],[Reserve3],[Reserve4],[Reserve5],[LaunchedFrom]"
               + ",[WindowHierarchy],{0} FROM [SulpHur].[dbo].[UIContents]"
               + " where [ContentID]={1}", linkIDS, contentID);
                table.ExecuteScalar(insertSql);

                // 3. Delete From Results, RuleStatus, AssemblyLink, Contents
                string deleteSql = string.Format("delete from results where contentid={0}", contentID);
                table.ExecuteScalar(deleteSql);

                deleteSql = string.Format("delete from rulestatus where contentid={0}", contentID);
                table.ExecuteScalar(deleteSql);

                deleteSql = string.Format("delete from assemblylink where contentid={0}", contentID);
                table.ExecuteScalar(deleteSql);

                deleteSql = string.Format("delete from uicontents where contentid={0}", contentID);
                table.ExecuteScalar(deleteSql);
            }
        }

        public void RecoverBuild(string buildno)
        {
            throw new NotImplementedException();
        }

        public void CleanUI(int contentID)
        {
            throw new NotImplementedException();
        }

        public void RecoverUI(int contentID)
        {
            throw new NotImplementedException();
        }
    }

    public class SulpHurTable : SQLConnection, ISulpHurTable
    {
        public SulpHurTable() : base() { }

        //public bool IsClientExist(SulpHurClientIdentity identity, out SulpHurClientInfo clientInfo)
        //{
        //    List<SulpHurClientInfo> scList = QueryClients("select * from clients where macaddress='" + identity.MacAddress + "' and username='" + identity.UserName + "' and osType='" + identity.OSType + "'");

        //    if (scList.Count == 1)
        //    {
        //        clientInfo = scList[0];
        //        return true;
        //    }
        //    else
        //    {
        //        clientInfo = null;
        //        return false;
        //    }
        //}


        public bool InsertRow<T>(T r)
            where T : Row
        {
            try
            {
                SulpHurDBBase command = SulpHurDBWrapper.Instance().GetSulpHurDBCommand(connStr);
                string sql = r.InsertSQL;
                command.Parameters = r.InsertParameters;

                command.ExecuteNoneQuery(sql);
                return true;
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                return false;
            }
        }

        public List<T> QueryTable<T>(string sql)
            where T : Row
        {
            List<T> rows = new List<T>();
            SulpHurDBBase command = SulpHurDBWrapper.Instance().GetSulpHurDBCommand(connStr);
            DataTable dt = command.SelectDataSet(sql);
            foreach (DataRow dr in dt.Rows)
            {
                Type rowType = typeof(T);
                T scTemp = (T)Activator.CreateInstance(rowType);
                scTemp.FillRowFromSQL(dr);

                rows.Add(scTemp);
            }
            return rows;
        }



        public bool UpdateRow<T>(T r) where T : Row
        {
            try
            {
                SulpHurDBBase command = SulpHurDBWrapper.Instance().GetSulpHurDBCommand(connStr);
                string sql = r.UpdateSQL;
                command.ExecuteNoneQuery(sql);
                return true;
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                return false;
            }
        }


        public List<T> QueryTable<T>(string sql, List<SqlParameter> p) where T : Row
        {
            List<T> rows = new List<T>();
            SulpHurDBBase command = SulpHurDBWrapper.Instance().GetSulpHurDBCommand(connStr);
            command.Parameters = p;
            DataTable dt = command.SelectDataSet(sql);
            foreach (DataRow dr in dt.Rows)
            {
                Type rowType = typeof(T);
                T scTemp = (T)Activator.CreateInstance(rowType);
                scTemp.FillRowFromSQL(dr);

                rows.Add(scTemp);
            }
            return rows;
        }


        public DataTable QueryTable(string sql)
        {
            SulpHurDBBase command = SulpHurDBWrapper.Instance().GetSulpHurDBCommand(connStr);
            DataTable dt = command.SelectDataSet(sql);
            return dt;
        }

        public object ExecuteScalar(string sql)
        {
            SulpHurDBBase command = SulpHurDBWrapper.Instance().GetSulpHurDBCommand(connStr);
            return command.ExecuteScalar(sql);
        }


        public bool Delete<T>(T r) where T : Row
        {
            try
            {
                SulpHurDBBase command = SulpHurDBWrapper.Instance().GetSulpHurDBCommand(connStr);
                string sql = r.DeleteSQL;
                command.ExecuteNoneQuery(sql);
                return true;
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                return false;
            }
        }
    }
}
