using System;
using System.Configuration;
using System.Reflection;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Data;

namespace SulpHurServiceAbstract
{
    public abstract class SulpHurDBWrapperBase : BaseFactory
    {
        private static SulpHurDBWrapperBase wrapper = null;
        private static string AssemblySettingName
        {
            get { return "SulpHurDBWrapperAssembly"; }
        }

        private static string ClassNameSettingName
        {
            get { return "SulpHurDBWrapperClassName"; }
        }

        private static string DefaultAssemblyName
        {
            get { return "SulpHurServiceImp.dll"; }
        }

        private static string DefaultClassName
        {
            get { return "SulpHurServiceImplements.SulpHurDBWrapper"; }
        }

        public static SulpHurDBWrapperBase Instance()
        {
            if (wrapper == null)
            {
                wrapper = (SulpHurDBWrapperBase)BaseFactory.Instance(SulpHurDBWrapperBase.AssemblySettingName, SulpHurDBWrapperBase.ClassNameSettingName, SulpHurDBWrapperBase.DefaultAssemblyName, SulpHurDBWrapperBase.DefaultClassName);
            }

            return wrapper;
        }

        public abstract SulpHurDBBase GetSulpHurDBCommand(string connStr);
    }

    public interface ISulpHurDBCommand
    {
        object ExecuteScalar(string sql);
        void ExecuteNoneQuery(string sql);
        DataTable SelectDataSet(string sql);
    }

    public abstract class SulpHurDBBase : ISulpHurDBCommand
    {
        protected SqlConnection conn;
        public List<SqlParameter> Parameters { get; set; }
        public SulpHurDBBase() { }
        public SulpHurDBBase(string conn)
        {
            this.conn = new SqlConnection(conn);
            Parameters = new List<SqlParameter>();
        }

        public abstract SqlCommand CreateCommand(string sql);

        public object ExecuteScalar(string sql)
        {
            SqlCommand command = CreateCommand(sql);
            conn.Open();
            object result = command.ExecuteScalar();
            conn.Close();
            return result;
        }

        public void ExecuteNoneQuery(string sql)
        {
            SqlCommand command = CreateCommand(sql);
            conn.Open();
            command.ExecuteNonQuery();
            conn.Close();
        }


        public DataTable SelectDataSet(string sql)
        {
            try
            {
                conn.Open();
                SqlDataAdapter sda = new SqlDataAdapter();
                sda.SelectCommand = CreateCommand(sql);
                DataSet ds = new DataSet();
                sda.Fill(ds, "data");
                return ds.Tables["data"];
            }
            finally {
                conn.Close();
            }
        }
    }

    public abstract class SQLConnection {
        protected string connStr = "";

        public SQLConnection() {
//#if DEBUG
//            //connStr = "Data Source=sulphurserver14;Initial Catalog=SulpHur;Integrated Security=True";
//#else
            connStr = ConfigurationManager.ConnectionStrings["MS.Internal.SulpHur.SulpHurService.Properties.Settings.SulpHurConnectionString"].ConnectionString;
//#endif
        }
    }
}
