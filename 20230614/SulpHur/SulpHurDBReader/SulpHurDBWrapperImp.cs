using System;
using SulpHurServiceAbstract;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;

namespace SulpHurServiceImplements
{
    public class SulpHurDBWrapper : SulpHurDBWrapperBase
    {
        public SulpHurDBWrapper() { }

        public override SulpHurDBBase GetSulpHurDBCommand(string connStr)
        {
            return new SulpHurDBCommonCommand(connStr);
        }
    }

    public class SulpHurDBCommonCommand : SulpHurDBBase
    {
        public SulpHurDBCommonCommand(string conn) : base(conn) { }
        public override SqlCommand CreateCommand(string sql)
        {
            SqlCommand command = new SqlCommand(sql, this.conn);
            command.CommandType = CommandType.Text;
            command.CommandTimeout = 30000;
            foreach (SqlParameter p in Parameters)
            {
                command.Parameters.Add(p);
            }
            return command;
        }
    }
}
