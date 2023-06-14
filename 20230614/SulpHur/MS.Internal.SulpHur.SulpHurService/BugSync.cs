using CMSherlock.VsoOdataClient;
using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace MS.Internal.SulpHur.SulpHurService
{
    public class BugSync
    {
        private static string connectString = string.Empty;

        static BugSync()
        {
            connectString = ConfigurationManager.ConnectionStrings["SulpHurConnectionString"].ToString();
        }
        public BugSync()
        {
        }

        public static void SyncVSOBug()
        {
            VsoClient vsoMSAzureClient = null;
            VsoClient vsoCEAPEXClient = null;
            VsoCepeaxFeedback cepeaxBug = null;
            VsoMSAzureBug msAzurebug = null;

            int bugID = 0;
            int bugState = 0;
            string state = string.Empty;
            int UpdatedBugState = 0;
            int UpdateReturnResult = 0;
            string sqlCommandString = "update bugs set status={0}, title=N'{1}', assigned_to='{2}', priority={3}, severity={4}, issue_type='{5}' where bug_id={6}";
            string sqlBugSelector = "select * from bugs where status<2";

            try
            {
                vsoMSAzureClient = new VsoClient(VsoProject.MSAZURE);
                vsoCEAPEXClient = new VsoClient(VsoProject.CEAPEX);

                using (SqlConnection sqlConnection = new SqlConnection(connectString))
                {
                    sqlConnection.Open();
                    DataAdapter da = new SqlDataAdapter(sqlBugSelector, sqlConnection);
                    DataSet dataSet = new DataSet();
                    da.Fill(dataSet);
                    Log.WriteServerLog($"Bug Count:{dataSet.Tables[0].Rows.Count}", TraceLevel.Info);
                    for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
                    {
                        bugID = Convert.ToInt32(dataSet.Tables[0].Rows[i][0]);
                        bugState = Convert.ToInt32(dataSet.Tables[0].Rows[i][1]);
                        try
                        {
                            if (bugID > 1000000)
                            {
                                msAzurebug = vsoMSAzureClient.GetVsoMSAzureBug(bugID);
                                state = msAzurebug.State;
                            }
                            else
                            {
                                cepeaxBug = vsoCEAPEXClient.GetVsoCEAPEXFeedback(bugID);
                                state = cepeaxBug.State;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.WriteServerLog("Failed to query the bug, id:" + bugID);
                            Log.WriteServerLog("With exception:" + ex.Message);
                            continue;
                        }
                        switch (state)
                        {
                            case "Active":
                            case "New":
                            case "In Progress":
                            case "In Review":
                            case "Blocked":
                                UpdatedBugState = 0;
                                break;
                            case "Resolved":
                                UpdatedBugState = 1;
                                break;
                            case "Closed":
                                UpdatedBugState = 2;
                                break;
                            default:
                                Log.WriteServerLog($"bugID:{bugID},{state}");
                                break;
                        }
                        string sql = string.Empty;
                        if (bugID > 1000000)
                        {
                            sql = string.Format(sqlCommandString, UpdatedBugState, msAzurebug.Title.Replace("'", "''"),
                                msAzurebug.AssignedTo, msAzurebug.Priority, int.Parse(msAzurebug.Severity.Substring(0, 1)), msAzurebug.IssueType, msAzurebug.Id);
                        }
                        else
                        {
                            sql = string.Format(sqlCommandString, UpdatedBugState, cepeaxBug.Title.Replace("'", "''"),
                                cepeaxBug.AssignedTo, cepeaxBug.LocPriority, 0, cepeaxBug.FeedbackType, cepeaxBug.Id);
                        }
                        try
                        {
                            SqlCommand sqlCommand = new SqlCommand(sql, sqlConnection);
                            UpdateReturnResult = sqlCommand.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            Log.WriteServerLog($"bugID:{bugID},{state}");
                            Log.WriteServerLog(ex.Message, TraceLevel.Error);
                        }

                        if (UpdateReturnResult == 1)
                        {
                            Log.WriteServerLog("Succeed to sync VSO Bug:" + bugID); ;
                        }
                        else
                        {
                            Log.WriteServerLog("Fail to sync VSO Bug:" + bugID);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteServerLog("Failed to Sync bug due to Exception: " + ex.Message, TraceLevel.Error);
            }
        }

        public static void BugSyncTask()
        {
            Task.Run(() =>
            {
                Log.WriteServerLog("Start bug sync service.", TraceLevel.Info);
                while (true)
                {
                    Log.WriteServerLog("Start to sync bug.", TraceLevel.Info);
                    SyncVSOBug();
                    Log.WriteServerLog("Sync bug End.", TraceLevel.Info);
                    Thread.Sleep(TimeSpan.FromMinutes(15));
                }
            });
        }
    }
}