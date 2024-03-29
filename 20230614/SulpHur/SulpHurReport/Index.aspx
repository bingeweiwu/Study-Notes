﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="SulpHurReport.WebForm1" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>SulpHur Report</title>    
    <link rel="icon" type="image/png" href="./images/icon.png"/>
    <link rel="stylesheet" href="./CSS/index.css"/>  
    <link rel="stylesheet" href="./CSS/RightClickMenu.css"/>  
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="./Scripts/Index.js"></script> 
    <script src="./Scripts/RightClickMenu.js"></script> 
</head>
<body>
    <!-- hearder -->
    <div class="header">
        <div class="logo" onclick="goToHomePage()">
        <h1>
            <img src="./images/logo.png" alt="">
            <asp:Label ID="image" runat="server" ></asp:Label>
          <span>Capture UIs Report</span>
        </h1>
    </div>      
        
        <!-- 搜索 -->
        <div class="search">
            <input type="text" name="" id="search" placeholder="Result ID / Page Title / User Name" onkeydown="if (event.keyCode == 13) Search()"><%--
            --%><button onclick="Search()">Search</button>
        </div>
        <!-- 用户 -->
        <div class="user" onclick="FastTableQuery()">            
            <asp:Image ID="UserProfileImage" runat="server" />
            <asp:Label ID="UserName" runat="server" CssClass="username"></asp:Label>
            <asp:Label ID="lnkCurrentUserAlias" runat="server" style="display:none"></asp:Label>
        </div>
    </div>

    <!-- condition -->
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
                <input type="checkbox" id="showLatest" value="Show Latest Build" name="showLatest"/>Show Latest Result
            </div>
        </div>
    </div>

    <!-- content -->
    <div class="content wrapper">
        <div id="ResultsTableContainer" class="ResultsTableContainer">
            <table id="ResultsTable" class="ResultsTable">
                <colgroup>
                    <col width="64px" />
                    <col width="128px" />
                    <col width="64px" />
                    <col width="160px" />
                    <col width="64px" />
                    <col width="300px" />
                    <col width="128px" />
                    <col width="100px" />
                    <col width="160px" />
                    <col width="160px" />
                    <col width="96px" />
                    <col width="80px" />
                    <%--<col width="80px"/>--%>
                    <%--<col width="5%"/>   --%>
                </colgroup>
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
                        <th>Date Uploaded (UTC)
                        </th>
                        <th>Date Checked (UTC)
                        </th>
                        <th>Comments
                        </th>
                        <th>Reviewed
                            <br />
                            <input type="checkbox" id="ckbReviewBugs" onclick="selectAllToReview(this)" />
                        </th>
                        <%--<th>Select All<br />
                            <input type="checkbox" id="ckbFileMultiBugs" onclick="selectAllToFileBug(this)" />
                        </th>--%>
                    </tr>
                </thead>
                <tbody id="ResultsBody" class="ResultsBody">
                </tbody>
            </table>
            <div class="Prompt-Message" id="Prompt-Message" onclick="ConditionQuery()">
                Loading...
            </div>
            <div id="GridMenu" class="PopupMenu" style="border: solid 1px #c3c3c3;display:none;">
                <ul>
                    <li id="MenuCopyResultID">Copy ResultID</li>
                    <li id="MenuCopyPageTitle">Copy page title</li>
                    <li id="MenuShowScreenShot">Show Screenshot</li>
                    <li id="MenuHideScreenShot">Hide Screenshot</li>
                    <li id="MenuAttachBugID">Attach BugID</li>
                    <li id="MenuCompareWith">Compare With...</li>
                    <li id="MenuShowAssembly">Show Assembly Info</li>
                    <li id="MenuShowReproSteps">Show Repro Steps</li>
                    <%--v-linchz related bugs--%>
                    <li id="MenuRelatedBugs">Related Bugs</li>
                    <li id="MenuGeneratePSQuery">Generate PSQ with Related bugs</li>
                    <li id="MenuExportInformation">Export Information</li>
                    <li id="MenuTestCoverageReport">Show Test Coverage Report</li>
                    <li id="MenuEditComments">Add/Edit review Comments</li>
                    <li id="MenuDelete">Delete</li>
                    <%--V-DANPGU FILEBUG--%>
                    <li id="MenuFileBug">File Bug</li>
                </ul>
            </div>
        </div>
        <div id="PageNavigation" class="PageNavigation">
            <div id="prePage" class="prePage">
                <button onclick="Previous()">Previous</button>
            </div>
            <div id="PageIndexs" class="PageIndexs"></div>
            <div id="nextPage" class="nextPage">
                <button onclick="Next()">Next</button>
            </div>
        </div>
        <!-- 导航区 -->
        <div class="right">
            <div class="backtotop"><a href="#top">top</a></div>
            <div class="commonButton">
                <div id="ExpandConditions" class="Expand hide">
                    <button onclick="Expand()">Expand</button>
                </div>
                <div id="Previous" class="Previous hide">
                    <button onclick="Previous()">Previous</button>
                </div>
                <div id="BackToCondition" class="Next hide">
                    <button onclick="BackTocondition()">BackToCdtion</button>
                </div> 
                <div id="next" class="Next hide">
                    <button onclick="Next()">Next</button>
                </div>
                <div class="Collapse hide">
                    <button id="CollapseResults" onclick="CollapseResults()">Expand</button>
                </div>
                <div class="backtobottom" onclick="scrollToBottom()">                   
                    <a> bottom</a>
                </div>
            </div>           
        </div>
    </div>

    <!-- rightClickMenu-->

    <div>
        <div id="ScreenLocker" oncontextmenu="return false;" style="opacity: 0.8;"></div>
        <div id="AttachBugIDForm" class="PopupWindow" oncontextmenu="return false;">
            <div class="TextZone">
                <span style="float: left; margin: 0 10px 0 0px;">Bug ID:</span>
                <input id="txtBugID" type="text" value="" style="width: 75%;" />
                <br />
                <span style="color: blue; font: italic bold 12px/20px arial,sans-serif;">* Split multi
                bug with "," (example: 123456,467890)</span>
            </div>
            <div class="ButtonPanel">
                <input hotkey="ENTER" class="Modern-Button" id="btnAttachBugOK" type="button" value="OK" style="margin: 0px 0 0 0px;" />
                <input hotkey="ESC" class="Modern-Button" id="btnAttachBugCancel" type="button" value="Cancel" style="margin: 0px 0 0 20px;" />
            </div>
        </div>
        <div id="CompareWithConditionForm" style="width: 810px;" class="PopupWindow">
            <div>
                <table id="CompareWithConditionTable">
                    <tr>
                        <td>Build No:
                        </td>
                        <td id="Compare_SC_BuildNo">
                            <span class="Tag TagAll">All</span>
                        </td>
                        <td>
                            <div class="ShowMoreContainer ShowMoreAction">
                                <img src="Images/arrow1.jpg" alt="" /><span>Show More</span>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td>Build Language:
                        </td>
                        <td id="Compare_SC_BuildLanguage">
                            <span class="Tag TagAll">All</span>
                        </td>
                        <td></td>
                    </tr>
                </table>
            </div>
            <div class="ButtonPanel">
                <input hotkey="ENTER" class="Modern-Button" type="button" value="OK" style="margin: 0px 0 0 0px;" id="btnCompareOK" />
                <input hotkey="ESC" class="Modern-Button" type="button" value="Cancel" style="margin: 0px 0 0 20px;" id="btnCompareCancel" />
            </div>
        </div>
    </div>

</body>
</html>
