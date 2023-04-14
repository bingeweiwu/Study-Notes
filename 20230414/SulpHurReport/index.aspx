<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="index.aspx.cs" Inherits="SulpHurReport.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>SulpHur Report</title>    
    <link rel="stylesheet" href="./CSS/index.css"/>  
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="./Scripts/index.js"></script> 
</head>
<body>
    <!-- 头部header:负责头部区域的样式，wrapper只负责版心效果 -->
    <div class="header">
        <div class="logo">
        <h1>
            <img src="./images/logo.png" alt="">
          <span>Capture UIs Report</span>
        </h1>
        <!-- <ul style="float:left; line-height: 52px; padding-right:20px">
            <li><a href="#">UI diff</a></li>
        </ul> -->
    </div>      
        
        <!-- 搜索 -->
        <div class="search">
            <input type="text" name="" id="" placeholder="Result ID / Page Title / User Name"><button>Search</button>
        </div>
        <!-- 用户 -->
        <div class="user">
            <img src="./images/v-mogenzhang.png" alt="v-mogenzhang">
            <asp:Label ID="UserName" runat="server" CssClass="username"></asp:Label>
        </div>
    </div>

    <!-- 条件区 -->
    <div class="list-category">
        <div class="wrapper" id="list-category">
            <div class="category-item Build-No collapse" id="SC_BuildNo">
                <span class="Tag TagAll" data-value="All"> Build No</span>              
            </div>
            <div class="category-item Build-Language collapse" id="SC_BuildLanguage">
                <span class="Tag TagAll" data-value="All"> Build
                    Language</span>              
            </div>
            <div class="category-item OS-Type collapse" id="SC_OSType">
                <span class="Tag TagAll" data-value="All"> OS Type</span>               
            </div>
            <div class="category-item OS-Language collapse" id="SC_OSLanguage">
                <span class="Tag TagAll" data-value="All">OS Language</span>              
            </div>
            <div class="category-item Rule collapse" id="SC_Rule">
                <span class="Tag TagAll" data-value="All"> Rule</span>               
            </div>
            <div class="category-item Result-Type collapse" id="SC_ResultType">
                <span class="Tag TagAll " data-value="All"> Result Type</span>
                <span class="Tag TagCommon selected" data-value="Fail">Fail</span>
                <span class="Tag TagCommon" data-value="Info">Info</span>
                <span class="Tag TagCommon" data-value="Pass">Pass</span>
                <span class="Tag TagCommon" data-value="Warning">Warning</span>
            </div>
            <div class="category-item Assembly-Name collapse" id="SC_AssemblyName">
                <span class="Tag TagAll" data-value="All">Assembly Name</span>
            </div>
            <div class="category-item Full-Type-Name collapse" id="SC_FullTypeName">
                <span class="Tag TagAll" data-value="All">Full Type Name</span>              
            </div>
            <div class="category-item Reviewed-Status" id="SC_ReviewedStatus">
                <span class="Tag TagAll" data-value="All"> Reviewed Status</span>
                <span class="Tag TagCommon" data-value="Reviewed">Reviewed</span>
                <span class="Tag TagCommon selected" data-value="UnReviewed">UnReviewed</span>
            </div>
            <hr>
            <div id="ConditionFilter" class="category-item">
                Time limit is
                <select name="selectDays" id="selectDays" class="selectDays">
                    <option class="Tag" value="7">7 days</option>
                    <option class="Tag" value="14">14 days</option>
                    <option class="Tag" value="30">1 month</option>
                    <option class="Tag" value="60">2 months</option>
                    <option class="Tag" value="90">3 months</option>
                    <option class="Tag" value="120" selected="selected">4 months</option>
                    <option class="Tag" value="180">6 months</option>
                    <option class="Tag" value="365">1 year</option>
                    <option class="Tag" value="-1">No limit</option>
                </select>
                <input type="checkbox" id="showLatest" value="Show Latest Build" name="showLatest"
                    checked="checked" />Show Latest Result
            </div>
        </div>
    </div>

    <!-- 内容区 -->
    <div class="content wrapper">
        <div id="ResultsTableContainer" class="ResultsTableContainer">
            <table id="ResultsTable" class="ResultsTable">
                <col width="auto;" />
                <col width="auto;" />
                <col width="auto;" />
                <col width="auto;" />
                <col width="auto;" />
                <col style="min-width: auto;" />
                <col width="auto;" />
                <col width="auto;" />
                <col width="auto;" />
                <col width="auto;" />
                <col width="auto;" />
                <col width="auto;" />
                <col width="auto;" />
                <col width="auto;" />
                <thead>
                    <tr>
                        <th>Row ID
                        </th>
                        <th>Build No.
                        </th>
                        <th>Build Language
                        </th>
                        <th>Rule Name
                        </th>
                        <th>Result
                        </th>
                        <th>Page Title
                        </th>
                        <th>User Name
                        </th>
                        <th>OS Type
                        </th>
                        <th>Build Same OS
                        </th>
                        <th>Date Uploaded (UTC)
                        </th>
                        <th>Date Checked (UTC)
                        </th>
                        <th>Reviewed
                            <br />
                            <input type="checkbox" id="ckbReviewBugs" onclick="selectAllToReview(this)" />
                        </th>
                        <th>Comments
                        </th>
                        <th>Select All<br />
                            <input type="checkbox" id="ckbFileMultiBugs" onclick="selectAllToFileBug(this)" />
                        </th>
                    </tr>
                    <tr class="lineheader">
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                    </tr>
                    <tr id="Row_DetailInfo_55961944" style="height: auto; display: table-row;" class="Row_DetailInfoClass">
                        <td colspan="19">
                            <div id="LogMessage" class="textMessage">
                                The following words in the highlighted controls did not pass the spelling check: occurences   
    
                                ControlType: Text   
    
                                completeString: Select an event type to add to the subscription. All occurences of the chosen event type
                                will trigger a notification to the subscription's target URL.</div>
                            <div id="ReviewLog" class="textMessage">No reviewed comments.
                            </div>
                            <div id="Screenshot"><img src="./images/55961944.jpg" style="max-width: 1087px; margin-left: 20px;"></div>
                        </td>
                    </tr>
                    <tr class="lineheader">
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                    </tr>
                    <tr id="Row_DetailInfo_55961944" style="height: auto; display: table-row;" class="Row_DetailInfoClass">
                        <td colspan="19">
                            <div id="LogMessage" class="textMessage">
                                The following words in the highlighted controls did not pass the spelling check: occurences   
    
                                ControlType: Text   
    
                                completeString: Select an event type to add to the subscription. All occurences of the chosen event type
                                will trigger a notification to the subscription's target URL.</div>
                            <div id="ReviewLog" class="textMessage">No reviewed comments.
                            </div>
                            <div id="Screenshot"><img src="./images/55961944.jpg" style="max-width: 1087px; margin-left: 20px;"></div>
                        </td>
                    </tr>
                    <tr class="lineheader">
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                    </tr>
                    <tr id="Row_DetailInfo_55961944" style="height: auto; display: table-row;" class="Row_DetailInfoClass">
                        <td colspan="19">
                            <div id="LogMessage" class="textMessage">
                                The following words in the highlighted controls did not pass the spelling check: occurences   
    
                                ControlType: Text   
    
                                completeString: Select an event type to add to the subscription. All occurences of the chosen event type
                                will trigger a notification to the subscription's target URL.</div>
                            <div id="ReviewLog" class="textMessage">No reviewed comments.
                            </div>
                            <div id="Screenshot"><img src="./images/55961944.jpg" style="max-width: 1087px; margin-left: 20px;"></div>
                        </td>
                    </tr>
                    <tr class="lineheader">
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                    </tr>
                    <tr id="Row_DetailInfo_55961944" style="height: auto; display: table-row;" class="Row_DetailInfoClass">
                        <td colspan="19">
                            <div id="LogMessage" class="textMessage">
                                The following words in the highlighted controls did not pass the spelling check: occurences   
    
                                ControlType: Text   
    
                                completeString: Select an event type to add to the subscription. All occurences of the chosen event type
                                will trigger a notification to the subscription's target URL.</div>
                            <div id="ReviewLog" class="textMessage">No reviewed comments.
                            </div>
                            <div id="Screenshot"><img src="./images/55961944.jpg" style="max-width: 1087px; margin-left: 20px;"></div>
                        </td>
                    </tr>
                    <tr class="lineheader">
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                    </tr>
                    <tr id="Row_DetailInfo_55961944" style="height: auto; display: table-row;" class="Row_DetailInfoClass">
                        <td colspan="19">
                            <div id="LogMessage" class="textMessage">
                                The following words in the highlighted controls did not pass the spelling check: occurences   
    
                                ControlType: Text   
    
                                completeString: Select an event type to add to the subscription. All occurences of the chosen event type
                                will trigger a notification to the subscription's target URL.</div>
                            <div id="ReviewLog" class="textMessage">No reviewed comments.
                            </div>
                            <div id="Screenshot"><img src="./images/55961944.jpg" style="max-width: 1087px; margin-left: 20px;"></div>
                        </td>
                    </tr>
                    <tr class="lineheader">
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                    </tr>
                    <tr id="Row_DetailInfo_55961944" style="height: auto; display: table-row;" class="Row_DetailInfoClass">
                        <td colspan="19">
                            <div id="LogMessage" class="textMessage">
                                The following words in the highlighted controls did not pass the spelling check: occurences   
    
                                ControlType: Text   
    
                                completeString: Select an event type to add to the subscription. All occurences of the chosen event type
                                will trigger a notification to the subscription's target URL.</div>
                            <div id="ReviewLog" class="textMessage">No reviewed comments.
                            </div>
                            <div id="Screenshot"><img src="./images/55961944.jpg" style="max-width: 1087px; margin-left: 20px;"></div>
                        </td>
                    </tr>
                    <tr class="lineheader">
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                    </tr>
                    <tr id="Row_DetailInfo_55961944" style="height: auto; display: table-row;" class="Row_DetailInfoClass">
                        <td colspan="19">
                            <div id="LogMessage" class="textMessage">
                                The following words in the highlighted controls did not pass the spelling check: occurences   
    
                                ControlType: Text   
    
                                completeString: Select an event type to add to the subscription. All occurences of the chosen event type
                                will trigger a notification to the subscription's target URL.</div>
                            <div id="ReviewLog" class="textMessage">No reviewed comments.
                            </div>
                            <div id="Screenshot"><img src="./images/55961944.jpg" style="max-width: 1087px; margin-left: 20px;"></div>
                        </td>
                    </tr>
                    <tr class="lineheader">
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                    </tr>
                    <tr id="Row_DetailInfo_55961944" style="height: auto; display: table-row;" class="Row_DetailInfoClass">
                        <td colspan="19">
                            <div id="LogMessage" class="textMessage">
                                The following words in the highlighted controls did not pass the spelling check: occurences   
    
                                ControlType: Text   
    
                                completeString: Select an event type to add to the subscription. All occurences of the chosen event type
                                will trigger a notification to the subscription's target URL.</div>
                            <div id="ReviewLog" class="textMessage">No reviewed comments.
                            </div>
                            <div id="Screenshot"><img src="./images/55961944.jpg" style="max-width: 1087px; margin-left: 20px;"></div>
                        </td>
                    </tr>
                    <tr class="lineheader">
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                    </tr>
                    <tr id="Row_DetailInfo_55961944" style="height: auto; display: table-row;" class="Row_DetailInfoClass">
                        <td colspan="19">
                            <div id="LogMessage" class="textMessage">
                                The following words in the highlighted controls did not pass the spelling check: occurences   
    
                                ControlType: Text   
    
                                completeString: Select an event type to add to the subscription. All occurences of the chosen event type
                                will trigger a notification to the subscription's target URL.</div>
                            <div id="ReviewLog" class="textMessage">No reviewed comments.
                            </div>
                            <div id="Screenshot"><img src="./images/55961944.jpg" style="max-width: 1087px; margin-left: 20px;"></div>
                        </td>
                    </tr>
                    <tr class="lineheader">
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                    </tr>
                    <tr id="Row_DetailInfo_55961944" style="height: auto; display: table-row;" class="Row_DetailInfoClass">
                        <td colspan="19">
                            <div id="LogMessage" class="textMessage">
                                The following words in the highlighted controls did not pass the spelling check: occurences   
    
                                ControlType: Text   
    
                                completeString: Select an event type to add to the subscription. All occurences of the chosen event type
                                will trigger a notification to the subscription's target URL.</div>
                            <div id="ReviewLog" class="textMessage">No reviewed comments.
                            </div>
                            <div id="Screenshot"><img src="./images/55961944.jpg" style="max-width: 1087px; margin-left: 20px;"></div>
                        </td>
                    </tr>
                    <tr class="lineheader">
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                    </tr>
                    <tr id="Row_DetailInfo_55961944" style="height: auto; display: table-row;" class="Row_DetailInfoClass">
                        <td colspan="19">
                            <div id="LogMessage" class="textMessage">
                                The following words in the highlighted controls did not pass the spelling check: occurences   
    
                                ControlType: Text   
    
                                completeString: Select an event type to add to the subscription. All occurences of the chosen event type
                                will trigger a notification to the subscription's target URL.</div>
                            <div id="ReviewLog" class="textMessage">No reviewed comments.
                            </div>
                            <div id="Screenshot"><img src="./images/55961944.jpg" style="max-width: 1087px; margin-left: 20px;"></div>
                        </td>
                    </tr>
                    <tr class="lineheader">
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                    </tr>
                    <tr id="Row_DetailInfo_55961944" style="height: auto; display: table-row;" class="Row_DetailInfoClass">
                        <td colspan="19">
                            <div id="LogMessage" class="textMessage">
                                The following words in the highlighted controls did not pass the spelling check: occurences   
    
                                ControlType: Text   
    
                                completeString: Select an event type to add to the subscription. All occurences of the chosen event type
                                will trigger a notification to the subscription's target URL.</div>
                            <div id="ReviewLog" class="textMessage">No reviewed comments.
                            </div>
                            <div id="Screenshot"><img src="./images/55961944.jpg" style="max-width: 1087px; margin-left: 20px;"></div>
                        </td>
                    </tr>
                    <tr class="lineheader">
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                    </tr>
                    <tr id="Row_DetailInfo_55961944" style="height: auto; display: table-row;" class="Row_DetailInfoClass">
                        <td colspan="19">
                            <div id="LogMessage" class="textMessage">
                                The following words in the highlighted controls did not pass the spelling check: occurences   
    
                                ControlType: Text   
    
                                completeString: Select an event type to add to the subscription. All occurences of the chosen event type
                                will trigger a notification to the subscription's target URL.</div>
                            <div id="ReviewLog" class="textMessage">No reviewed comments.
                            </div>
                            <div id="Screenshot"><img src="./images/55961944.jpg" style="max-width: 1087px; margin-left: 20px;"></div>
                        </td>
                    </tr>
                    <tr class="lineheader">
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                    </tr>
                    <tr id="Row_DetailInfo_55961944" style="height: auto; display: table-row;" class="Row_DetailInfoClass">
                        <td colspan="19">
                            <div id="LogMessage" class="textMessage">
                                The following words in the highlighted controls did not pass the spelling check: occurences   
    
                                ControlType: Text   
    
                                completeString: Select an event type to add to the subscription. All occurences of the chosen event type
                                will trigger a notification to the subscription's target URL.</div>
                            <div id="ReviewLog" class="textMessage">No reviewed comments.
                            </div>
                            <div id="Screenshot"><img src="./images/55961944.jpg" style="max-width: 1087px; margin-left: 20px;"></div>
                        </td>
                    </tr>
                    <tr class="lineheader">
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                    </tr>
                    <tr id="Row_DetailInfo_55961944" style="height: auto; display: table-row;" class="Row_DetailInfoClass">
                        <td colspan="19">
                            <div id="LogMessage" class="textMessage">
                                The following words in the highlighted controls did not pass the spelling check: occurences   
    
                                ControlType: Text   
    
                                completeString: Select an event type to add to the subscription. All occurences of the chosen event type
                                will trigger a notification to the subscription's target URL.</div>
                            <div id="ReviewLog" class="textMessage">No reviewed comments.
                            </div>
                            <div id="Screenshot"><img src="./images/55961944.jpg" style="max-width: 1087px; margin-left: 20px;"></div>
                        </td>
                    </tr>
                    <tr class="lineheader">
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                    </tr>
                    <tr id="Row_DetailInfo_55961944" style="height: auto; display: table-row;" class="Row_DetailInfoClass">
                        <td colspan="19">
                            <div id="LogMessage" class="textMessage">
                                The following words in the highlighted controls did not pass the spelling check: occurences   
    
                                ControlType: Text   
    
                                completeString: Select an event type to add to the subscription. All occurences of the chosen event type
                                will trigger a notification to the subscription's target URL.</div>
                            <div id="ReviewLog" class="textMessage">No reviewed comments.
                            </div>
                            <div id="Screenshot"><img src="./images/55961944.jpg" style="max-width: 1087px; margin-left: 20px;"></div>
                        </td>
                    </tr>
                    <tr class="lineheader">
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                    </tr>
                    <tr id="Row_DetailInfo_55961944" style="height: auto; display: table-row;" class="Row_DetailInfoClass">
                        <td colspan="19">
                            <div id="LogMessage" class="textMessage">
                                The following words in the highlighted controls did not pass the spelling check: occurences   
    
                                ControlType: Text   
    
                                completeString: Select an event type to add to the subscription. All occurences of the chosen event type
                                will trigger a notification to the subscription's target URL.</div>
                            <div id="ReviewLog" class="textMessage">No reviewed comments.
                            </div>
                            <div id="Screenshot"><img src="./images/55961944.jpg" style="max-width: 1087px; margin-left: 20px;"></div>
                        </td>
                    </tr>
                    <tr class="lineheader">
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                    </tr>
                    <tr id="Row_DetailInfo_55961944" style="height: auto; display: table-row;" class="Row_DetailInfoClass">
                        <td colspan="19">
                            <div id="LogMessage" class="textMessage">
                                The following words in the highlighted controls did not pass the spelling check: occurences   
    
                                ControlType: Text   
    
                                completeString: Select an event type to add to the subscription. All occurences of the chosen event type
                                will trigger a notification to the subscription's target URL.</div>
                            <div id="ReviewLog" class="textMessage">No reviewed comments.
                            </div>
                            <div id="Screenshot"><img src="./images/55961944.jpg" style="max-width: 1087px; margin-left: 20px;"></div>
                        </td>
                    </tr>
                    <tr class="lineheader">
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                    </tr>
                    <tr id="Row_DetailInfo_55961944" style="height: auto; display: table-row;" class="Row_DetailInfoClass">
                        <td colspan="19">
                            <div id="LogMessage" class="textMessage">
                                The following words in the highlighted controls did not pass the spelling check: occurences   
    
                                ControlType: Text   
    
                                completeString: Select an event type to add to the subscription. All occurences of the chosen event type
                                will trigger a notification to the subscription's target URL.</div>
                            <div id="ReviewLog" class="textMessage">No reviewed comments.
                            </div>
                            <div id="Screenshot"><img src="./images/55961944.jpg" style="max-width: 1087px; margin-left: 20px;"></div>
                        </td>
                    </tr>
                    <tr class="lineheader">
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                    </tr>
                    <tr id="Row_DetailInfo_55961944" style="height: auto; display: table-row;" class="Row_DetailInfoClass">
                        <td colspan="19">
                            <div id="LogMessage" class="textMessage">
                                The following words in the highlighted controls did not pass the spelling check: occurences   
    
                                ControlType: Text   
    
                                completeString: Select an event type to add to the subscription. All occurences of the chosen event type
                                will trigger a notification to the subscription's target URL.</div>
                            <div id="ReviewLog" class="textMessage">No reviewed comments.
                            </div>
                            <div id="Screenshot"><img src="./images/55961944.jpg" style="max-width: 1087px; margin-left: 20px;"></div>
                        </td>
                    </tr>
                    <tr class="lineheader">
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                    </tr>
                    <tr id="Row_DetailInfo_55961944" style="height: auto; display: table-row;" class="Row_DetailInfoClass">
                        <td colspan="19">
                            <div id="LogMessage" class="textMessage">
                                The following words in the highlighted controls did not pass the spelling check: occurences   
    
                                ControlType: Text   
    
                                completeString: Select an event type to add to the subscription. All occurences of the chosen event type
                                will trigger a notification to the subscription's target URL.</div>
                            <div id="ReviewLog" class="textMessage">No reviewed comments.
                            </div>
                            <div id="Screenshot"><img src="./images/55961944.jpg" style="max-width: 1087px; margin-left: 20px;"></div>
                        </td>
                    </tr>
                    <tr class="lineheader">
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                        <th>1</th>
                    </tr>
                    <tr id="Row_DetailInfo_55961944" style="height: auto; display: table-row;" class="Row_DetailInfoClass">
                        <td colspan="19">
                            <div id="LogMessage" class="textMessage">
                                The following words in the highlighted controls did not pass the spelling check: occurences   
    
                                ControlType: Text   
    
                                completeString: Select an event type to add to the subscription. All occurences of the chosen event type
                                will trigger a notification to the subscription's target URL.</div>
                            <div id="ReviewLog" class="textMessage">No reviewed comments.
                            </div>
                            <div id="Screenshot"><img src="./images/55961944.jpg" style="max-width: 1087px; margin-left: 20px;"></div>
                        </td>
                    </tr>
                </thead>
            </table>
        </div>
        <div class="right">
            <div class="backtotop"><a href="#top">top</a></div>
            <div class="Expand">
                <button onclick="Expand()">Expand</button>
            </div>
            <div class="backtobottom"><a href="#gobottom">bottom</a></div>
        </div>
    </div>
</body>

</html>
