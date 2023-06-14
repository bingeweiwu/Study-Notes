using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.Objects;
using System.Linq.Expressions;
using System.Drawing;
using System.IO;
using SulpHurManagementSystem.Models;
using SulpHurManagementSystem.Common;
using System.Text.RegularExpressions;

namespace SulpHurManagementSystem
{
    public partial class Report : System.Web.UI.Page
    {
        //search condition splitter 
        private string fieldSplitter = ",\r\n";
        private string rowSplitter = @"$";
        private string escapedRowSplitter = @"\$";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                this.InitPageData();

                //grid init
                this.btnSearch_Click(null, null);
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                string[] buildNoArr = null;
                string[] buildLanguageArr = null;
                string[] osTypeArr = null;
                string[] osLanguageArr = null;
                int[] ruleIDArr = null;
                string[] resultTypeArr = null;
                string pageTitle = "";
                string[] assemblyNameArr = null;
                int[] typeIDArr = null;

                //validate NULL
                if (string.IsNullOrEmpty(this.hdSearchCondition.Value))
                {
                    throw new SulpHurErrorException("Search Conditon is NULL!");
                }

                //retrieve serach condition
                string[] allSearchConditionArr = Regex.Split(this.hdSearchCondition.Value, escapedRowSplitter, RegexOptions.None);
                //BuildNo
                if (allSearchConditionArr[0] != "-1")
                {
                    buildNoArr = Regex.Split(allSearchConditionArr[0], fieldSplitter, RegexOptions.None);
                }
                //BuildLanguage
                if (allSearchConditionArr[1] != "-1")
                {
                    buildLanguageArr = Regex.Split(allSearchConditionArr[1], fieldSplitter, RegexOptions.None);
                }
                //OSType
                if (allSearchConditionArr[2] != "-1")
                {
                    osTypeArr = Regex.Split(allSearchConditionArr[2], fieldSplitter, RegexOptions.None);
                }
                //OSLanguage
                if (allSearchConditionArr[3] != "-1")
                {
                    osLanguageArr = Regex.Split(allSearchConditionArr[3], fieldSplitter, RegexOptions.None);
                }
                //Rule
                if (allSearchConditionArr[4] != "-1")
                {
                    ruleIDArr = Regex.Split(allSearchConditionArr[4], fieldSplitter, RegexOptions.None).Select(item => int.Parse(item)).ToArray();
                }
                //Result
                if (allSearchConditionArr[5] != "-1")
                {
                    resultTypeArr = Regex.Split(allSearchConditionArr[5], fieldSplitter, RegexOptions.None);
                }
                //AssemblyName
                if (allSearchConditionArr[6] != "-1")
                {
                    assemblyNameArr = Regex.Split(allSearchConditionArr[6], fieldSplitter, RegexOptions.None);
                }
                //PageTitle
                pageTitle = allSearchConditionArr[7];
                //FullTypeName
                if (allSearchConditionArr[8] != "-1")
                {
                    typeIDArr = Regex.Split(allSearchConditionArr[8], fieldSplitter, RegexOptions.None).Select(item => int.Parse(item)).ToArray();
                }

                //bind grid
                this.Session["GridView"] = this.GetResult(buildNoArr, buildLanguageArr, osTypeArr, osLanguageArr, ruleIDArr, resultTypeArr, pageTitle, assemblyNameArr, typeIDArr);
                this.GridView1.DataSource = this.Session["GridView"];
                this.GridView1.DataBind();
            }
            catch (SulpHurErrorException)
            {
                this.Session["GridView"] = null;
                this.GridView1.DataSource = this.Session["GridView"];
                this.GridView1.DataBind();
            }
            catch (Exception)
            {
                this.Session["GridView"] = null;
                this.GridView1.DataSource = this.Session["GridView"];
                this.GridView1.DataBind();
            }
        }

        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.GridView1.PageIndex = e.NewPageIndex;
            this.RefreshGridView();
        }

        private void InitPageData()
        {
            //ddlBuildNo
            this.ddlBuildNo.DataBind();
            //ddlBuildLanguage
            this.ddlBuildLanguage.DataBind();
            //ddlOSType
            this.ddlOSType.DataBind();
            //ddlOSLanguage
            this.ddlOSLanguage.DataBind();
            //ddlResult
            this.ddlResult.DataBind();
            //ddlRule
            this.ddlRule.DataBind();
            //ddlAssemblyName
            this.ddlAssemblyName.DataBind();
            //ddlFullTypeName
            this.ddlFullTypeName.DataBind();

            //default search condition
            string defaultSearchCondition = string.Empty;
            //latest BuildNo
            this.ddlBuildNo.SelectedIndex = 0;
            this.ddlBuildNo_SelectedIndexChanged(this.ddlBuildNo, null);
            defaultSearchCondition += this.ddlBuildNo.SelectedValue.Trim();
            //ENU build or the first language option if no ENU build
            ListItem enuBuild = this.ddlBuildLanguage.Items.FindByValue("ENU");
            if (enuBuild != null)
            {
                defaultSearchCondition += rowSplitter + enuBuild.Value.Trim();
            }
            else
            {
                defaultSearchCondition += rowSplitter + this.ddlBuildLanguage.Items[0].Value.Trim();
            }
            //All OSType
            defaultSearchCondition += rowSplitter + "-1";
            //All OSLanguage
            defaultSearchCondition += rowSplitter + "-1";
            //ScreenShotRule or the first rule option if no ScreenShotRule
            ListItem screenshotRule = this.ddlRule.Items.FindByText("ScreenShot Rule");
            if (screenshotRule != null)
            {
                defaultSearchCondition += rowSplitter + screenshotRule.Value.Trim();
            }
            else
            {
                defaultSearchCondition += rowSplitter + this.ddlRule.Items[0].Value.Trim();
            }
            //All Result
            defaultSearchCondition += rowSplitter + "-1";
            //All AssemblyName
            defaultSearchCondition += rowSplitter + "-1";
            //unset PageTitle
            defaultSearchCondition += rowSplitter;
            //all FullTypeName
            defaultSearchCondition += rowSplitter + "-1";

            this.hdSearchCondition.Value = defaultSearchCondition;
        }
        private void RefreshGridView()
        {
            this.GridView1.DataSource = null;
            this.GridView1.DataSource = this.Session["GridView"];
            this.GridView1.DataBind();
        }

        //private List<ResultReport> GetAllResult()
        //{
        //    SulpHurEntities entities = new SulpHurEntities();
        //    List<ResultReport> resultReportList = new List<ResultReport>();

        //    //grid data
        //    var result = entities.Results.OrderByDescending((item)=>item.CreateDate).Select((item) => new {
        //        item.ResultID, 
        //        item.UIContent.BuildInfo.BuildNo, 
        //        item.Rule.RuleName, 
        //        item.UIContent.UIName, 
        //        item.ResultType, 
        //        item.UIContent.Client.UserName, 
        //        item.UIContent.BuildInfo.Language, 
        //        item.UIContent.Client.OSType, 
        //        //item.UIContent.BugLinks,
        //        item.UIContent.DateUploaded,
        //        item.CreateDate });
        //    int rowId = 1;
        //    foreach (var row in result)
        //    {
        //        ResultReport resultReport = new ResultReport();
        //        resultReport.ResultID = row.ResultID;
        //        resultReport.RowID = rowId;
        //        resultReport.BuildNo = row.BuildNo;
        //        resultReport.RuleName = row.RuleName;
        //        resultReport.PageTitle = row.UIName;
        //        resultReport.Result = row.ResultType;
        //        resultReport.UserName = row.UserName;
        //        resultReport.BuildLanguage = row.Language;
        //        resultReport.OSType = row.OSType;
        //        ////Linked bug id
        //        //row.BugLinks.ToList().ForEach(item => resultReport.LinkedBugIDs = string.Format("{0},{1}", resultReport.LinkedBugIDs, item.BugID));
        //        //if (!string.IsNullOrEmpty(resultReport.LinkedBugIDs))
        //        //{
        //        //    resultReport.LinkedBugIDs = resultReport.LinkedBugIDs.Remove(0, 1);
        //        //}
        //        resultReport.DateUploaded = row.DateUploaded;
        //        resultReport.DateChecked = row.CreateDate;

        //        resultReportList.Add(resultReport);
        //        rowId++;
        //    }

        //    return resultReportList;
        //}
        //private List<ResultReport> GetResult(string buildNo, string buildLanguage, int ruleID, string resultType, string uiName, string assemblyName, int typeId)
        //{
        //    SulpHurEntities entities = new SulpHurEntities();
        //    List<ResultReport> resultReportList = new List<ResultReport>();

        //    //entities.Results.Where("it.BuildID = @BuildID", new ObjectParameter("BuildID", buildID))
        //    //    .Include("
        //    //var result = from item in entities.Results
        //    //             where item.RuleID.Equals(ruleID) 
        //    //             where item.ResultType.Equals(resultType) &&
        //    //                    item.UIContent.UIName.Contains(uiName) &&
        //    //                    item.UIContent.BuildInfo.BuildID.Equals(buildID)
        //    //             orderby item.CreateDate descending
        //    //             select new { item.UIContent.BuildInfo.BuildNo, item.Rule.RuleName, item.UIContent.UIName, item.ResultType, item.UIContent.Client.UserName, item.UIContent.BuildInfo.Language, item.UIContent.Client.OSType, item.CreateDate };

        //    //query
        //    var list = entities.Results.Select(item => new
        //    {
        //        item.ResultID,
        //        item.UIContent.BuildInfo.BuildNo,
        //        item.RuleID,
        //        item.Rule.RuleName,
        //        item.UIContent.UIName,
        //        item.ResultType,
        //        item.UIContent.Client.UserName,
        //        item.UIContent.BuildInfo.BuildID,
        //        item.UIContent.BuildInfo.Language,
        //        item.UIContent.Client.OSType,
        //        item.UIContent.AssemblyLinks,
        //        item.UIContent.DateUploaded,
        //        item.CreateDate
        //    });
        //    if (ruleID > -1)
        //    {
        //        list = list.Where(item => item.RuleID.Equals(ruleID));
        //    }
        //    if (!buildNo.Equals("-1"))
        //    {
        //        list = list.Where(item => item.BuildNo.Equals(buildNo));
        //    }
        //    if (!buildLanguage.Equals("-1"))
        //    {
        //        list = list.Where(item => item.Language.Equals(buildLanguage));
        //    }
        //    if (!resultType.Equals("-1"))
        //    {
        //        list = list.Where(item => item.ResultType.Equals(resultType));
        //    }
        //    if (!string.IsNullOrEmpty(uiName))
        //    {
        //        list = list.Where(item => item.UIName.Contains(uiName));
        //    }
        //    if (!assemblyName.Equals("-1"))
        //    {
        //        list = list.Where(item => item.AssemblyLinks.Any(assemblyLink => assemblyLink.AssemblyInfo.AssemblyName.Equals(assemblyName)));
        //    }
        //    if (!typeId.Equals(-1))
        //    {
        //        list = list.Where(item => item.AssemblyLinks.Any(assemblyLink => assemblyLink.TypeID.Equals(typeId)));
        //    }
        //    //define result set
        //    var resultSetQuery = list.Select(item => new
        //    {
        //        item.ResultID,
        //        item.BuildNo,
        //        item.RuleID,
        //        item.RuleName,
        //        item.UIName,
        //        item.ResultType,
        //        item.UserName,
        //        item.BuildID,
        //        item.Language,
        //        item.OSType,
        //        item.DateUploaded,
        //        item.CreateDate
        //    }).OrderByDescending(item => item.CreateDate);
        //    //string sql = ((System.Data.Objects.ObjectQuery)resultSetQuery).ToTraceString();
        //    var result = resultSetQuery.ToList();

        //    //fectch result
        //    int rowId = 1;
        //    foreach (var row in result)
        //    {
        //        ResultReport resultReport = new ResultReport();
        //        resultReport.ResultID = row.ResultID;
        //        resultReport.RowID = rowId;
        //        resultReport.BuildNo = row.BuildNo;
        //        resultReport.RuleName = row.RuleName;
        //        resultReport.PageTitle = row.UIName;
        //        resultReport.Result = row.ResultType;
        //        resultReport.UserName = row.UserName;
        //        resultReport.BuildLanguage = row.Language;
        //        resultReport.OSType = row.OSType;
        //        resultReport.DateUploaded = row.DateUploaded;
        //        resultReport.DateChecked = row.CreateDate;
        //        ////Linked bug id
        //        //row.BugLinks.ToList().ForEach(item => resultReport.LinkedBugIDs = string.Format("{0},{1}", resultReport.LinkedBugIDs, item.BugID));
        //        //if (!string.IsNullOrEmpty(resultReport.LinkedBugIDs))
        //        //{
        //        //    resultReport.LinkedBugIDs = resultReport.LinkedBugIDs.Remove(0,1);
        //        //}
        //        resultReportList.Add(resultReport);
        //        rowId++;
        //    }

        //    return resultReportList;
        //}
        private List<ResultReport> GetResult(string[] buildNoArr, string[] buildLanguageArr, string[] osTypeArr, string[] osLanguageArr, int[] ruleIDArr, string[] resultTypeArr, string uiName, string[] assemblyNameArr, int[] typeIDArr)
        {
            SulpHurEntities entities = new SulpHurEntities();
            List<ResultReport> resultReportList = new List<ResultReport>();

            //query
            var list = entities.Results.Select(item => new
            {
                item.ResultID,
                item.UIContent.BuildInfo.BuildNo,
                item.RuleID,
                item.Rule.RuleName,
                item.UIContent.UIName,
                item.ResultType,
                item.UIContent.Client.UserName,
                item.UIContent.BuildInfo.BuildID,
                item.UIContent.BuildInfo.Language,
                item.UIContent.Client.OSType,
                item.UIContent.Client.OSLanguage,
                item.UIContent.AssemblyLinks,
                item.UIContent.DateUploaded,
                item.CreateDate
            });

            //BuildNo
            if (buildNoArr != null && buildNoArr.Length > 0)
            {
                list = list.Where(item => buildNoArr.Contains(item.BuildNo));
            }
            //BuildLanguage
            if (buildLanguageArr != null && buildLanguageArr.Length > 0)
            {
                list = list.Where(item => buildLanguageArr.Contains(item.Language));
            }
            //OSType
            if (osTypeArr != null && osTypeArr.Length > 0)
            {
                list = list.Where(item => osTypeArr.Contains(item.OSType));
            }
            //OSLanguage
            if (osLanguageArr != null && osLanguageArr.Length > 0)
            {
                list = list.Where(item => osLanguageArr.Contains(item.OSLanguage));
            }
            //RuleID
            if (ruleIDArr != null && ruleIDArr.Length > 0)
            {
                list = list.Where(item => ruleIDArr.Contains(item.RuleID));
            }
            //ResultType
            if (resultTypeArr != null && resultTypeArr.Length > 0)
            {
                list = list.Where(item => resultTypeArr.Contains(item.ResultType));
            }
            //AssemblyName
            if (assemblyNameArr != null && assemblyNameArr.Length > 0)
            {
                list = list.Where(item => item.AssemblyLinks.Any(assemblyLink => assemblyNameArr.Contains(assemblyLink.AssemblyInfo.AssemblyName)));
            }
            //PageTitle
            if (!string.IsNullOrEmpty(uiName))
            {
                list = list.Where(item => item.UIName.Contains(uiName));
            }
            //TypeID
            if (typeIDArr != null && typeIDArr.Length > 0)
            {
                list = list.Where(item => item.AssemblyLinks.Any(assemblyLink => typeIDArr.Contains(assemblyLink.TypeID)));
            }
            //define result set
            var resultSetQuery = list.Select(item => new
            {
                item.ResultID,
                item.BuildNo,
                item.RuleID,
                item.RuleName,
                item.UIName,
                item.ResultType,
                item.UserName,
                item.BuildID,
                item.Language,
                item.OSType,
                item.DateUploaded,
                item.CreateDate
            }).OrderByDescending(item => item.CreateDate);
            //string sql = ((System.Data.Objects.ObjectQuery)resultSetQuery).ToTraceString();
            var result = resultSetQuery.ToList();

            //fectch result
            int rowId = 1;
            foreach (var row in result)
            {
                ResultReport resultReport = new ResultReport();
                resultReport.ResultID = row.ResultID;
                resultReport.RowID = rowId;
                resultReport.BuildNo = row.BuildNo;
                resultReport.RuleName = row.RuleName;
                resultReport.PageTitle = row.UIName;
                resultReport.Result = row.ResultType;
                resultReport.UserName = row.UserName;
                resultReport.BuildLanguage = row.Language;
                resultReport.OSType = row.OSType;
                resultReport.DateUploaded = row.DateUploaded;
                resultReport.DateChecked = row.CreateDate;
                resultReportList.Add(resultReport);
                rowId++;
            }

            return resultReportList;
        }
        private void UpdateBuildLanguage(string buildNo)
        {
            EntityDataSource6.Where = "it.BuildNo = @BuildNo";
            EntityDataSource6.WhereParameters.Clear();
            EntityDataSource6.WhereParameters.Add("BuildNo", TypeCode.String, buildNo);
            EntityDataSource6.DataBind();

            //ddlFullTypeName
            this.ddlBuildLanguage.DataBind();
        }
        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType.Equals(DataControlRowType.DataRow))
            {
                
                //e.Row.Attributes.Add("onclick", string.Format("onRowClick(this, {0});", this.GridView1.DataKeys[e.Row.RowIndex].Values["ResultID"].ToString()));
                e.Row.Attributes.Add("resultID", this.GridView1.DataKeys[e.Row.RowIndex].Values["ResultID"].ToString());
                e.Row.Attributes.Add("style", "cursor:hand");
                e.Row.Style.Add("cursor", "default");
            }
        }

        protected void ddlAssemblyName_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Update data source
            DropDownList ddlSender = sender as DropDownList;
            if (ddlSender.SelectedValue.Equals("-1"))
            {
                EntityDataSource5.Where = "";
                EntityDataSource5.WhereParameters.Clear();
                EntityDataSource5.DataBind();
            }
            else
            {
                EntityDataSource5.Where = "it.AssemblyName = @AssemblyName";
                EntityDataSource5.WhereParameters.Clear();
                EntityDataSource5.WhereParameters.Add("AssemblyName", TypeCode.String, ddlSender.SelectedValue);
                EntityDataSource5.DataBind();
            }
            //ddlFullTypeName
            this.ddlFullTypeName.DataBind();
            //this.ddlFullTypeName.Items.Insert(0, new ListItem("<All Type>", "-1"));
        }
        protected void ddlBuildNo_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Update data source
            DropDownList ddlSender = sender as DropDownList;
            if (ddlSender.SelectedValue.Equals("-1"))
            {
                EntityDataSource6.Where = "";
                EntityDataSource6.WhereParameters.Clear();
                EntityDataSource6.DataBind();
            }
            else
            {
                EntityDataSource6.Where = "it.BuildNo = @BuildNo";
                EntityDataSource6.WhereParameters.Clear();
                EntityDataSource6.WhereParameters.Add("BuildNo", TypeCode.String, ddlSender.SelectedValue);
                EntityDataSource6.DataBind();
            }
            //ddlFullTypeName
            this.ddlBuildLanguage.DataBind();
            //this.ddlBuildLanguage.Items.Insert(0, new ListItem("<All Language>", "-1"));
        }
    }
}