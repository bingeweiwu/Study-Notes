using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace SulpHurManagementSystem.Utility
{
    public class ProductQuery : IProductQuery
    {
        string vertical = "|";
        string connStr = ConfigurationManager.ConnectionStrings["ADOConn"].ToString();

        public string QueryAvailableOSTypes()
        {
            string sql = "select distinct ostype from clients order by ostype asc";
            return GetListResult(sql);
        }
        public string QueryAvailableCapturedLanguages()
        {
            string sql = "select distinct language from buildinfo order by language asc";
            return GetListResult(sql);
        }
        public string QueryAvailableCapturedBuilds()
        {
            string sql = "select distinct buildno from buildinfo order by buildno desc";
            return GetListResult(sql);
        }
        public string QueryAvailableAssembly()
        {
            string sql = "select distinct assemblyname from assemblyinfo order by assemblyname asc";
            return GetListResult(sql);
        }
        public string QueryAvailableOSLanguage()
        {
            string sql = "select distinct oslanguage from clients order by oslanguage asc";
            return GetListResult(sql);
        }

        public string QueryAvailableTypes()
        {
            string sql = "select distinct typeid, fulltypename from assemblyinfo order by fulltypename asc";
            return GetListResultWithID(sql);
        }

        public Dictionary<string, Rule> QueryAvailableRules()
        {
            string enabledRules = ConfigurationManager.AppSettings["EnabledRules"];
            string tempRuleNameString = string.Empty;
            if (!string.IsNullOrEmpty(enabledRules))
            {
                string[] ruleList = enabledRules.Split('|');
                for (int i = 0; i < ruleList.Length; i++)
                {
                    if (i == 0)
                        tempRuleNameString += "'" + ruleList[i];
                    else if (i == ruleList.Length - 1)
                        tempRuleNameString += "','" + ruleList[i] + "'";
                    else
                        tempRuleNameString += "','" + ruleList[i];
                }
            }

            Dictionary<string, Rule> availableRules = new Dictionary<string, Rule>();
            string sql = "select ruleid, rulename, ruledesc from rules where IsEnabled=1 order by rulename asc";
            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand command = new SqlCommand(sql, conn);
            command.CommandType = CommandType.Text;
            conn.Open();

            SqlDataReader dr = command.ExecuteReader();
            Rule availableRule;
            while (dr.Read())
            {
                availableRule = new Rule()
                {
                    RuleID = Convert.ToInt32(dr[0]),
                    RuleName = dr[1].ToString(),
                    RuleDesc = dr[2].ToString(),
                };
                availableRules.Add(dr[0].ToString(), availableRule);
            }
            conn.Close();

            return availableRules;
        }

        public Dictionary<string, string> QueryRules()
        {
            string sql1 = "select ruleid, rulename from rules order by rulename asc";
            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand command = new SqlCommand(sql1, conn);
            command.CommandType = CommandType.Text;
            conn.Open();

            Dictionary<string, string> temp = new Dictionary<string, string>();
            SqlDataReader dr = command.ExecuteReader();
            while (dr.Read())
            {
                temp.Add(dr.GetInt32(0).ToString(), dr.GetString(1));
            }
            conn.Close();

            return temp;
        }


        public Dictionary<string, string> QueryAssemblyTypesInfo()
        {
            string sql1 = "select typeid, fulltypename,assemblyname from assemblyinfo order by fulltypename asc";
            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand command = new SqlCommand(sql1, conn);
            command.CommandType = CommandType.Text;
            conn.Open();

            Dictionary<string, string> temp = new Dictionary<string, string>();
            SqlDataReader dr = command.ExecuteReader();
            while (dr.Read())
            {
                temp.Add(dr.GetInt32(0) + "," + dr.GetString(1), dr.GetString(2));
            }
            conn.Close();

            return temp;
        }

        private string GetListResult(string sql)
        {
            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand command = new SqlCommand(sql, conn);
            command.CommandType = CommandType.Text;
            conn.Open();

            List<string> list = new List<string>();
            SqlDataReader dr = command.ExecuteReader();
            while (dr.Read())
            {
                list.Add(dr.GetString(0));
            }
            string result = string.Join(vertical, list);
            conn.Close();
            return result;
        }

        private string GetListResultWithID(string sql)
        {
            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand command = new SqlCommand(sql, conn);
            command.CommandType = CommandType.Text;
            conn.Open();

            List<string> list = new List<string>();
            SqlDataReader dr = command.ExecuteReader();
            while (dr.Read())
            {
                list.Add(dr.GetInt32(0) + "," + dr.GetString(1));
            }
            string result = string.Join(vertical, list);
            conn.Close();
            return result;
        }
    }
}