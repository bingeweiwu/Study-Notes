using System;
using SulpHurServiceAbstract;
using System.Configuration;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Data;

namespace SulpHurServiceImplements
{
    //public class AdminUILog : SQLConnection, IAdminUILog
    //{
    //    public AdminUILog()
    //        : base()
    //    {
    //    }
    //    public bool AddLogException(LogException exception)
    //    {
    //        try
    //        {
    //            SulpHurDBBase command = SulpHurDBWrapper.Instance().GetSulpHurDBCommand(connStr);
    //            string sql = "insert into logexception (buildid,clientid,exceptioncontent,inserttime,lastmodifytime,exceptioncount) values(@buildid,@clientid,@exceptioncontent,@inserttime,@lastmodifytime,@exceptioncount)";

    //            SqlParameter pBuildID = new SqlParameter("@buildid", System.Data.DbType.Int32);
    //            pBuildID.Value = exception.BuildID;
    //            command.Parameters.Add(pBuildID);
    //            SqlParameter pClientID = new SqlParameter("@clientid", System.Data.DbType.Int32);
    //            pClientID.Value = exception.ClientID;
    //            command.Parameters.Add(pClientID);
    //            SqlParameter pExceptionContent = new SqlParameter("@exceptioncontent", System.Data.DbType.String);
    //            pExceptionContent.Value = exception.ExceptionContent;
    //            command.Parameters.Add(pExceptionContent);
    //            SqlParameter pInsertTime = new SqlParameter("@inserttime", System.Data.DbType.DateTime);
    //            pInsertTime.Value = exception.InsertTime;
    //            command.Parameters.Add(pInsertTime);
    //            SqlParameter pLastModityTime = new SqlParameter("@lastmodifytime", System.Data.DbType.DateTime);
    //            pLastModityTime.Value = exception.LastModifyTime;
    //            command.Parameters.Add(pLastModityTime);
    //            SqlParameter pExceptionCount = new SqlParameter("@exceptioncount", System.Data.DbType.Int32);
    //            pExceptionCount.Value = exception.ExceptionCount;
    //            command.Parameters.Add(pExceptionCount);

    //            command.ExecuteNoneQuery(sql);
    //            return true;
    //        }
    //        catch (Exception)
    //        {
    //            return false;
    //        }
    //    }
    //    public List<LogException> QueryLogException(string sql)
    //    {
    //        List<LogException> eList = new List<LogException>();
    //        SulpHurDBBase command = SulpHurDBWrapper.Instance().GetSulpHurDBCommand(connStr);
    //        DataTable dt = command.SelectDataSet(sql);
    //        foreach (DataRow dr in dt.Rows)
    //        {
    //            LogException leTemp = new LogException();
    //            leTemp.LogID = (int)dr["LogID"];
    //            leTemp.ClientID = (int)dr["ClientID"];
    //            leTemp.BuildID = (int)dr["BuildID"];
    //            leTemp.ExceptionContent = (string)dr["ExceptionContent"];
    //            leTemp.ExceptionCount = (int)dr["ExceptionCount"];
    //            leTemp.InsertTime = (DateTime)dr["InsertTime"];
    //            leTemp.LastModifyTime = (DateTime)dr["LastModifyTime"];

    //            eList.Add(leTemp);
    //        }
    //        return eList;
    //    }
    //    public bool EditLogException(int id, DateTime newTime, int newCount)
    //    {
    //        try
    //        {
    //            SulpHurDBBase command = SulpHurDBWrapper.Instance().GetSulpHurDBCommand(connStr);
    //            string sql = "update logexception set lastmodifytime='"
    //                + newTime
    //                + "',exceptioncount='" + newCount
    //                + "' where logid='" + id + "'";
    //            command.ExecuteNoneQuery(sql);
    //            return true;
    //        }
    //        catch (Exception)
    //        {
    //            return false;
    //        }
    //    }
    //}

    //public class AdminUIlogFactory : AdminUILogBase
    //{
    //    public override IAdminUILog GetAdminUILog()
    //    {
    //        return new AdminUILog();
    //    }
    //}
}
