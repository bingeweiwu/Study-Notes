<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Report.aspx.cs" Inherits="SulpHurManagementSystem.Report" EnableEventValidation="false" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server" >
    <link rel="stylesheet" type="text/css" href="styles/jquery.multiselect.css" />
<link rel="stylesheet" type="text/css" href="styles/style.css" />
<link rel="stylesheet" type="text/css" href="styles/jquery-ui/ui-lightness/jquery-ui-1.8.23.css" />
<script type="text/javascript" src="scripts/jquery-ui-1.8.23.min.js"></script>
<script type="text/javascript" src="scripts/jquery.multiselect.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%--Hiden Controls--%>
    <%--GridMenu--%>
    <div id="GridMenu" class="PopupMenu" >
        <ul>
            <li onclick="copyToClipboard($(gGridView1_SelectedRow).attr('resultID'));" onmouseover="$(this).attr('title', 'ResultID: ' + $(gGridView1_SelectedRow).attr('resultID'));">Copy ResultID (For Tool Testing)</li>
            <li onclick="PopupMenu_ShowScreenshot_Clicked()">Show Screenshot</li>
            <li onclick="PopupMenu_HideScreenshot_Clicked()">Hide Screenshot</li>
            <li onclick="PopupMenu_AttachBugID_Clicked()">Attach BugID</li>
            <li onclick="PopupMenu_CompareWith_Clicked()">Compare With...</li>
            <li onclick="PopupMenu_ShowAssemblyInfo_Clicked()">Show Assembly Info</li>
            <li onclick="PopupMenu_ShowReproSteps_Clicked()">Show Repro Steps</li>
            <li id="RelatedBugsMenuItem" onmouseover="PopupMenu_RelatedBugs_MouseOver()" onmouseout="PopupMenu_RelatedBugs_MouseOut()" style="background-image:url(images/submenu_arrow.png); background-repeat:no-repeat; background-position:center right;">Related Bugs</li>
        </ul>
    </div>
    <div id="RelatedBugsSubMenu" class="PopupMenu" style="width:600px;">
        <ul>
        </ul>
    </div>
    <%--ScreenLocker--%>
    <div id="ScreenLocker" oncontextmenu="return false;" style="filter: alpha(opacity=80);"></div>
    <div id="AttachBugIDForm" style ="width: 300px;" class="PopupWindow" oncontextmenu="return false;">
        <div>
            <span style="float:left;margin:0 10px 0 0px;">Bug ID:</span>
            <input id="txtBugID" type="text" value="" style="width:70%;"/>
            <br />
            <span style="color: blue; font:italic bold 12px/20px arial,sans-serif;"> * Split multi bug with "," (example: 123456,467890)</span>
        </div>
        <div class="ButtonPanel">
            <input hotkey="ENTER" type="button" value="OK" style="margin:0px 0 0 0px;" onclick="AttachBugIDForm_OK_Clicked();"/>
            <input hotkey="ESC" type="button" value="Cancel" style="margin:0px 0 0 20px;" onclick="AttachBugIDForm_Cancel_Clicked();" />
        </div>
    </div>
    <%--CompareWithConditionForm--%>
    <div id="CompareWithConditionForm" style="width:350px;" class="PopupWindow">
        <div>
            <div class="common_lable_input_pair">
                <div>Build No.:</div>
                <asp:DropDownList ID="CompareWith_ddlBuildNo" runat="server" 
                DataSourceID="EntityDataSource7" DataTextField="BuildNo" 
                DataValueField="BuildNo" style="width:200px;">
                </asp:DropDownList>
                <asp:EntityDataSource ID="EntityDataSource7" runat="server" 
                ConnectionString="name=SulpHurEntities" 
                DefaultContainerName="SulpHurEntities" EnableFlattening="False" 
                EntitySetName="BuildInfoes" OrderBy="it.[BuildNo] DESC" 
                Select="it.[BuildNo]" GroupBy="it.[BuildNo]">
                </asp:EntityDataSource>
            </div>
            <div class="common_lable_input_pair">
                <div>Build Language:</div>
                <asp:DropDownList ID="CompareWith_ddlBuildLanguage" runat="server" 
                DataSourceID="EntityDataSource8" DataTextField="Language" 
                DataValueField="Language" style="width:200px;">
                </asp:DropDownList>
                <asp:EntityDataSource ID="EntityDataSource8" runat="server" 
                ConnectionString="name=SulpHurEntities" 
                DefaultContainerName="SulpHurEntities" EnableFlattening="False" 
                EntitySetName="BuildInfoes" OrderBy="it.[Language] DESC" 
                Select="it.[Language]" GroupBy="it.[Language]">
                </asp:EntityDataSource>
            </div>
        </div>
        <div class="ButtonPanel">
            <input hotkey="ENTER" type="button" value="OK" style="margin:0px 0 0 0px;" onclick="CompareWithConditionForm_OK_Clicked();"/>
            <input hotkey="ESC" type="button" value="Cancel" style="margin:0px 0 0 20px;" onclick="CompareWithConditionForm_Cancel_Clicked();" />
        </div>
    </div>
    <div id="PopupContainer" oncontextmenu="return false;" style=" height: auto;">
        <div class="TitleBar">
            <span id="Title"></span>
            <img alt="" hotkey="ESC" src="Images/BigCloseButton.png" style=" float: right;" onload="ResizeImage(this, 30, 30 );" onclick="$(this).parent().parent().stop().animate({top:document.documentElement.scrollHeight}, 1000, 'swing', function(){$(this).hide();});" />
            <br />
        </div>
        <div id="Body">
            <div id="Content"></div>
        </div>
    </div>
    <%--ImageBlockTemplate--%>
    <div id="ImageBlockTemplate" class="image-block">
        <div name="Title"><span name="Build" style="float: left;"></span><span name="Result"  style="float: right;"></span><br style="clear:both;" /></div>
        <div name="LogMessage"></div>
        <img onload="ResizeImage(this, 300, 280);" onclick="ImageBlockTemplate_Clicked(this);"/>
    </div>
    <div id="ZoomInImageBlock" class="PopupWindow">
        <div name="Title"><span name="Build" style="float: left;"></span><span name="Result"  style="float: right;"></span><br style="clear:both;" /></div>
        <div name="LogMessage"></div>
        <img hotkey="ESC" onclick="ZoomInImageBlock_Clicked(this);"/>
    </div>
    <%--Screenshot--%>
    <table style="display:none;">
        <tr id="Row_DetailInfo">
            <td colspan="10">
                <div id="LogMessage" style="width: 100%; margin:5px 0 10px 0;"></div>
                <div id="Screenshot"></div>
            </td>
        </tr>
    </table>
    <img id="CopyIcon" src="Images\copy.png" style="display:none; width:14px;"/>
    <%--error tip panel--%>
    <div id="ErrorTipPanel" onclick="$(this).hide();"></div>
    <%--assembly info dialog--%>
    <table id="AssemblyInfoTableTemplate" style=" width: auto; display: none;line-height: 150%; border-collapse: collapse; border:1px solid gray;">
        <tr style=" background: #ccc">
            <th>Assembly Name</th>
            <th>Full Type Name</th>
            <th>Is Page Identifier</th>
        </tr>
    </table>
    <div id="AssemblyInfoDialog", title="Assembly Info" style=" text-align: center; vertical-align:middle;">
    </div>
    <%--repro step dialog--%>
    <div id="ReproStepsDialog", title="Repro Steps" style=" width: auto; display: none;line-height: 200%;">
        <div class="repro-dialog-row">
            <div class="repro-dialog-row-header">Generated By Tool</div>
            <div class="alginment-row repro-dialog-row-data"><span style=" width:120px; margin-right: 10px;">Launched From:</span><span id="LaunchedFrom"></span><br /></div> 
            <div class="alginment-row repro-dialog-row-data"><span style=" width:120px; margin-right: 10px;">Window Hierarchy:</span><span id="WindowHierarchy"></span><br /></div>
        </div>
        <div class="repro-dialog-row">
            <div class="repro-dialog-row-header">Custom</div>
            <div class="alginment-row repro-dialog-row-data"><span style=" width:120px; margin-right: 10px;">Repro Steps:</span><span id="CustomReproStep"></span><br /></div> 
        </div>
    </div>


    <%--Body Controls--%>
    <div class="ConditionArea">
    <div class="alginment-row">
        <div class="common_lable_input_pair">
        <asp:Label ID="lblBuildNo" runat="server" Text="Build No.:" ></asp:Label>
                        <asp:DropDownList ID="ddlBuildNo" runat="server" 
        DataSourceID="EntityDataSource1" DataTextField="BuildNo" 
        DataValueField="BuildNo" onselectedindexchanged="ddlBuildNo_SelectedIndexChanged" >
        </asp:DropDownList>

                            <asp:EntityDataSource ID="EntityDataSource1" runat="server" 
        ConnectionString="name=SulpHurEntities" 
        DefaultContainerName="SulpHurEntities" EnableFlattening="False" 
        EntitySetName="BuildInfoes" OrderBy="it.[BuildNo] DESC" 
        Select="it.[BuildNo]" GroupBy="it.[BuildNo]" EntityTypeFilter="">
        </asp:EntityDataSource>
    </div>
    <div class="common_lable_input_pair">
        <asp:Label ID="lblBuildLanguage" runat="server" Text="Build Language:" ></asp:Label>
                    <asp:DropDownList ID="ddlBuildLanguage" runat="server" 
        DataSourceID="EntityDataSource6" DataTextField="Language" 
        DataValueField="Language" >
        </asp:DropDownList>
    </div>
                        <asp:EntityDataSource ID="EntityDataSource6" runat="server" 
        ConnectionString="name=SulpHurEntities" DefaultContainerName="SulpHurEntities" 
        EnableFlattening="False" EntitySetName="BuildInfoes" GroupBy="it.[Language]" 
        OrderBy="it.[Language]" Select="it.[Language]">
    </asp:EntityDataSource>
    <br />
    </div>
    <div class="alginment-row">
        <div class="common_lable_input_pair">
        <asp:Label ID="lblOSType" runat="server" Text="OS Type:" ></asp:Label>
                        <asp:DropDownList ID="ddlOSType" runat="server" 
        DataSourceID="EntityDataSource9" DataTextField="OSType" 
        DataValueField="OSType" >
        </asp:DropDownList>

                            <asp:EntityDataSource ID="EntityDataSource9" runat="server" 
        ConnectionString="name=SulpHurEntities" 
        DefaultContainerName="SulpHurEntities" EnableFlattening="False" 
        EntitySetName="Clients" OrderBy="it.[OSType]" 
        Select="it.[OSType]" GroupBy="it.[OSType]" EntityTypeFilter="">
        </asp:EntityDataSource>
    </div>
    <div class="common_lable_input_pair">
        <asp:Label ID="lblOSLanguage" runat="server" Text="OS Language:" ></asp:Label>
                    <asp:DropDownList ID="ddlOSLanguage" runat="server" 
        DataSourceID="EntityDataSource10" DataTextField="OSLanguage" 
        DataValueField="OSLanguage" >
        </asp:DropDownList>
    </div>
                        <asp:EntityDataSource ID="EntityDataSource10" runat="server" 
        ConnectionString="name=SulpHurEntities" DefaultContainerName="SulpHurEntities" 
        EnableFlattening="False" EntitySetName="Clients" GroupBy="it.[OSLanguage]" 
        OrderBy="it.[OSLanguage]" Select="it.[OSLanguage]">
    </asp:EntityDataSource>
    <br />
    </div>
    <div class="alginment-row">
    <div class="common_lable_input_pair">
        <asp:Label ID="lblRule" runat="server" Text="Rule:" ></asp:Label>
                <asp:DropDownList ID="ddlRule" runat="server" DataSourceID="EntityDataSource2" 
        DataTextField="RuleName" DataValueField="RuleID" >
        </asp:DropDownList>
    </div>
                            <asp:EntityDataSource ID="EntityDataSource2" runat="server" 
        ConnectionString="name=SulpHurEntities" 
        DefaultContainerName="SulpHurEntities" EnableFlattening="False" 
        EntitySetName="Rules" EntityTypeFilter="Rule" OrderBy=" it.[RuleName]" 
        Select="it.[RuleID], it.[RuleName]">
    </asp:EntityDataSource>
    <div class="common_lable_input_pair">
        <asp:Label ID="lblResult" runat="server" Text="Result:" ></asp:Label>
                    <asp:DropDownList ID="ddlResult" runat="server" 
        DataSourceID="EntityDataSource3" DataTextField="ResultType" 
        DataValueField="ResultType" >
        </asp:DropDownList>
    </div>
                            <asp:EntityDataSource ID="EntityDataSource3" runat="server" 
        ConnectionString="name=SulpHurEntities" 
        DefaultContainerName="SulpHurEntities" EnableFlattening="False" 
        EntitySetName="Results" EntityTypeFilter="Result" 
        GroupBy="it.[ResultType]" OrderBy="it.[ResultType]" Select="it.[ResultType]">
    </asp:EntityDataSource>

    <br />
    </div>
    <div class="alginment-row">
    <div class="common_lable_input_pair">
        <asp:Label ID="lblAssemblyName" runat="server" Text="Assembly Name:" ></asp:Label>
                        <asp:DropDownList ID="ddlAssemblyName" runat="server" 
        DataSourceID="EntityDataSource4" DataTextField="AssemblyName" 
        DataValueField="AssemblyName" >
        </asp:DropDownList>
    </div>
                            <asp:EntityDataSource ID="EntityDataSource4" runat="server" 
        ConnectionString="name=SulpHurEntities" DefaultContainerName="SulpHurEntities" 
        EnableFlattening="False" EntitySetName="AssemblyInfoes" 
        GroupBy="it.[AssemblyName]" OrderBy="it.[AssemblyName]" 
        Select="it.[AssemblyName]">
    </asp:EntityDataSource>
    <div class="common_lable_input_pair">
        <asp:Label ID="lblPage" runat="server" Text="Page Title:" ></asp:Label>
        <asp:TextBox ID="txtPageTitle" runat="server" Width="302" style=" border: 1px solid #CCCCCC; background-color: White;"></asp:TextBox>
    </div>
    </div>
        <div style="width: 100%; text-align:right;">
            <div class="alginment-row">
                <div class="common_lable_input_pair">
                    <asp:Label ID="lblFullTypeName" runat="server" Text="Full Type Name:" ></asp:Label>
                    <asp:DropDownList ID="ddlFullTypeName" runat="server" 
                        DataSourceID="EntityDataSource5" DataTextField="FullTypeName" 
                        DataValueField="TypeID" style="width: 745px;" >
                    </asp:DropDownList>
                </div>
                <asp:EntityDataSource ID="EntityDataSource5" runat="server" 
                    ConnectionString="name=SulpHurEntities" DefaultContainerName="SulpHurEntities" 
                    EnableFlattening="False" EntitySetName="AssemblyInfoes" 
                    Select="it.[FullTypeName], it.[TypeID]">
                </asp:EntityDataSource>

                <asp:Button ID="btnSearch" runat="server" OnClientClick="return btnSearch_Clicked();" 
                    Text="Search(F9)" style="width: 100px; float: right;" 
                    onclick="btnSearch_Click" />

                <br />
            </div>
        </div>
    </div>

    <a href="#" onclick="ConditionToggle_Clicked();" class="condition-toggle">Show Search Condition&gt&gt</a>
    <div class="search-condition">
        <div class="search-condition-content">
            <div class="common_lable_input_pair alginment-row" style="margin: 2px 0 2px 0;">
                <span>BuildNo.:</span>
                <span style="border:0; width:900px; height:auto; color:#1c94c4; " id="SearchCondition_BuildNo"></span>
                <br />
            </div>
            <div class="common_lable_input_pair alginment-row" style="margin: 2px 0 2px 0;">
                <span>BuildLanguage:</span>
                <span style="border:0; width:900px; height:auto; color:#1c94c4; " id="SearchCondition_BuildLanguage"></span>
                <br />
            </div>
            <div class="common_lable_input_pair alginment-row" style="margin: 2px 0 2px 0;">
                <span>OSType:</span>
                <span style="border:0; width:900px; height:auto; color:#1c94c4; " id="SearchCondition_OSType"></span>
                <br />
            </div>
            <div class="common_lable_input_pair alginment-row" style="margin: 2px 0 2px 0;">
                <span>OSLanguage:</span>
                <span style="border:0; width:900px; height:auto; color:#1c94c4; " id="SearchCondition_OSLanguage"></span>
                <br />
            </div>
            <div class="common_lable_input_pair alginment-row" style="margin: 2px 0 2px 0;">
                <span>Rule:</span>
                <span style="border:0; width:900px; height:auto; color:#1c94c4; " id="SearchCondition_Rule"></span>
                <br />
            </div>
            <div class="common_lable_input_pair alginment-row" style="margin: 2px 0 2px 0;">
                <span>Result:</span>
                <span style="border:0; width:900px; height:auto; color:#1c94c4; " id="SearchCondition_Result"></span>
                <br />
            </div>
            <div class="common_lable_input_pair alginment-row" style="margin: 2px 0 2px 0;">
                <span>AssemblyName:</span>
                <span style="border:0; width:900px; height:auto; color:#1c94c4; " id="SearchCondition_AssemblyName"></span>
                <br />
            </div>
            <div class="common_lable_input_pair alginment-row" style="margin: 2px 0 2px 0;">
                <span>PageTitle:</span>
                <span style="border:0; width:900px; height:auto; color:#1c94c4; " id="SearchCondition_PageTitle"></span>
                <br />
            </div>
            <div class="common_lable_input_pair alginment-row" style="margin: 2px 0 2px 0;">
                <span>FullTypeName:</span>
                <span style="border:0; width:900px; height:auto; color:#1c94c4; " id="SearchCondition_FullTypeName"></span>
                <br />
            </div>
        </div>
        <asp:HiddenField ID="hdSearchCondition" runat="server" />
    </div>
    <div class="ResultArea">
        <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" 
            AllowPaging="True" 
            onpageindexchanging="GridView1_PageIndexChanging" 
            onrowdatabound="GridView1_RowDataBound" DataKeyNames="ResultID" 
            PageSize="20" CssClass="CtlTable">
            <EmptyDataTemplate>
            <table border="1px">
            <tr>
            <th width="50px">
            RowID
            </th>
            <th width="80px">
            Build No.
            </th>
            <th width="100px">
            Build Language
            </th>
            <th width="130px">
            Rule Name
            </th>
            <th width="50px">
            Result
            </th>
            <th width="150px">
            Page Title
            </th>
            <th width="130px">
            User Name
            </th>
            <th width="60px">
            OS Type
            </th>
            <th width="135px">
            Date Uploaded
            </th>
            <th width="135px">
            Date Checked
            </th>
            </tr>
            <tr>
            <td colspan="10" align="center">
            No data available.
            </td>
            </tr>
            </table>
            </EmptyDataTemplate>
            <Columns>
                <asp:BoundField DataField="RowID" HeaderText="RowID">
                <HeaderStyle Width="50px" />
                <ItemStyle Font-Bold="True" HorizontalAlign="Center" />
                </asp:BoundField>
                <asp:BoundField DataField="BuildNo" HeaderText="Build No." 
                    HeaderStyle-Width="80" >
    <HeaderStyle Width="80px"></HeaderStyle>
                </asp:BoundField>
                <asp:BoundField DataField="BuildLanguage" HeaderText="Build Language" 
                    HeaderStyle-Width="100">
    <HeaderStyle Width="100px"></HeaderStyle>
                </asp:BoundField>
                <asp:BoundField DataField="RuleName" HeaderText="Rule Name" 
                    HeaderStyle-Width="130">
    <HeaderStyle Width="130px"></HeaderStyle>
                </asp:BoundField>
                <asp:BoundField DataField="Result" HeaderText="Result" HeaderStyle-Width="50">
    <HeaderStyle Width="50px"></HeaderStyle>
                </asp:BoundField>
                <asp:BoundField DataField="PageTitle" HeaderText="Page Title" 
                    HeaderStyle-Width="150">
    <HeaderStyle Width="150px" Wrap="True"></HeaderStyle>
                </asp:BoundField>
                <asp:BoundField DataField="UserName" HeaderText="User Name" 
                    HeaderStyle-Width="130">
    <HeaderStyle Width="130px"></HeaderStyle>
                </asp:BoundField>
                <asp:BoundField DataField="OSType" HeaderText="OS Type" HeaderStyle-Width="60">
    <HeaderStyle Width="60px"></HeaderStyle>
                </asp:BoundField>
                <asp:BoundField DataField="DateUploaded" HeaderText="Date Uploaded" 
                    HeaderStyle-Width="135">
    <HeaderStyle Width="135px"></HeaderStyle>
                </asp:BoundField>
                <asp:BoundField DataField="DateChecked" HeaderText="Date Checked" 
                    HeaderStyle-Width="135">
    <HeaderStyle Width="135px"></HeaderStyle>
                </asp:BoundField>
            </Columns>
            <PagerSettings Mode="NumericFirstLast" />
            <PagerStyle HorizontalAlign="Center" />
        </asp:GridView>
    </div>

    <script type="text/javascript" language="javascript">
        //global variable
        var gGridView1_SelectedRow = null;
        var gGridView1_Width = $('#<%=GridView1.ClientID %>').width() - 4 + 'px';
        addOnLoadHandler(onLoad);

        function onLoad() {
//            //workaround max-width on IE8 for ddlFullTypeName
//            var max_width = $('#<%=ddlFullTypeName.ClientID%>').width() > 690 ? '690px' : "auto";
//            $('#<%=ddlFullTypeName.ClientID%>').width(max_width);
//            //workaround max-width on IE8 for ddlAssemblyName
//            var max_width = $('#<%=ddlAssemblyName.ClientID%>').width() > 400 ? '400px' : "auto";
//            $('#<%=ddlAssemblyName.ClientID%>').width(max_width);

            bindEvent();
            setTableColor("<%=GridView1.ClientID %>");
            loadCustomControl();
        }
        function bindEvent() {
            //disable document context menu
            $(document).bind("contextmenu", function (e) {
                return false;
            });

            //unload popup menu
            $('#GridMenu').click(function () {
                $('#GridMenu').hide();
                $('#RelatedBugsSubMenu').hide();
            });
            $(document).click(function () {
                $('#GridMenu').hide();
                $('#RelatedBugsSubMenu').hide();
            });
            //bind grid view event
            $('#<%=GridView1.ClientID %>').find('tr').each(function () {
                if (isDataRow(this)) {
                    //popup menu
                    $(this).bind("contextmenu", function (e) {
                        //select the row
                        $(e.currentTarget).click();
                        //position
                        $('#GridMenu').css({
                            top: e.pageY + 'px',
                            left: e.pageX + 'px'
                        }).slideDown('fast');

                        return false;
                    })
                    //disable text selection
                    .bind('selectstart', function () {
                        //                        clearSelection();
                        return false;
                    }).bind('mouseup', function () {
                        $(this).bind('selectstart', function () {
                            return false;
                        })
                    })
                    //enable text selection
                    .bind('mousemove', function (e) {
                        if (window.event.button != 0) {
                            if (window.event.button == 1) {
                                $(this).unbind('selectstart');
                            }
                        }
                    });
                }
            });
        };
        //set table color
        function setTableColor(TableID) {
            var clickClass = "";
            var moveClass = "";
            var clickTR = null;
            var moveTR = null;
            var dblClickTR = null;
            var Ptr = document.getElementById(TableID).getElementsByTagName("tr");
            //set row color
            for (i = 1; i < Ptr.length + 1; i++) {
                if (i == 1) {
                    Ptr[i - 1].className = "Tab_HeaderTr";
                } else {
                    if (!isDataRow(Ptr[i - 1])) {
                        continue;
                    }
                    Ptr[i - 1].className = (i % 2 > 0) ? "Tab_EvenTr" : "Tab_OddTr";
                }
            }
            for (var i = 1; i < Ptr.length; i++) {
                var Owner = Ptr[i].item;
                //mouse over
                Ptr[i].onmouseover = function Move() {
                    //row validation
                    if (!isDataRow(this)) {
                        return;
                    }

                    if (clickTR != this) {
                        if (moveTR != this) {
                            moveClass = this.className;
                            moveTR = this;
                            this.className = "Tr_Move";
                        }
                    }
                }
                //mouse out
                Ptr[i].onmouseout = function Out() {
                    //row validation
                    if (!isDataRow(this)) {
                        return;
                    }

                    if (clickTR != this) {
                        moveTR = null;
                        this.className = moveClass;
                    }
                }
                //mouse click
                Ptr[i].onclick = function Clk() {
                    //row validation
                    if (!isDataRow(this)) {
                        return;
                    }

                    //highlight selected row
                    if (clickTR != this) {
                        if (clickTR) {
                            clickTR.className = clickClass;
                        }
                        clickTR = this;
                        gGridView1_SelectedRow = this;
                        clickClass = moveClass;
                    }
                    this.className = "Tr_Click";
                }
                //mouse dblclick
                Ptr[i].ondblclick = function dblClk() {
                    //row validation
                    if (!isDataRow(this)) {
                        return;
                    }

                    //show detail
                    if (dblClickTR != this) {
                        dblClickTR = this;
                        showDetailInfoAfterRow(this);
                    }
                    else {
                        $('#Row_DetailInfo').toggle();
                    }

                    return false;
                }
            }
        }
        //show detail info
        function showDetailInfoAfterRow(row) {
            //detail info row position
            $('#Row_DetailInfo').insertAfter(row);

            //ajax to get detail info
            var resultID = $(row).attr("resultID");
            getDetailInfo(resultID);

            //save latest dblclick row
            $('#Row_DetailInfo').show();
        }
        //row validation
        function isDataRow(row) {
            var resultID = $(row).attr("resultID");
            if (resultID) {
                return true;
            }

            return false;
        }
        //get detail result info (screenshot and log message)
        function getDetailInfo(resultID) {
            $.ajax({
                url: 'Ajax.aspx',
                type: 'POST',
                data: {
                    IsAjax: true,
                    Method: 'Ajax_GetDetailInfo',
                    ResultID: resultID
                },
                beforeSend: function () {
                    //reset img state
                    $('#Row_DetailInfo').find('#Screenshot').html('<img src="Images/ajax-loader.gif"/>');
                },
                error: function (data) {
                    //                    alert(data.responseText);
                    alert("Ajax request error!");
                },
                success: function (data) {
                    //error message
                    if (data.substring(0, 4) == "[E]:") {
                        //show error message
                        var errMsg = data.substring(4);
                        alert(errMsg);

                        return;
                    }

                    var arrData = data.split('[|]');
                    //show detail info row
                    //log message
//                    $('#Row_DetailInfo').find('#LogMessage').width($('#Row_DetailInfo').find('#LogMessage').parent().width());
                    $('#Row_DetailInfo').find('#LogMessage').text(arrData[1]);
                    //screenshot
                    var max_width = $('#Row_DetailInfo').find('#Screenshot').width();
                    $('#Row_DetailInfo').find('#Screenshot').html('<img src="' + arrData[0] + '" />');
                    //img size
                    $('#Row_DetailInfo').find('#Screenshot').find('img').css({ 'max-width': max_width }).bind('click', function (e) {
                        if ($(this).css('max-width') == 'none') {
                            //zoom out
                            $(this).css({ 'max-width': max_width });
                            $(this).parent().css({ 'max-width': 'none', 'overflow': 'hidden' });
                        }
                        else {
                            //zoom in
                            $(this).css({ 'max-width': 'none' });
                            $(this).parent().css({ 'max-width': max_width, 'overflow': 'scroll' });
                        }
                    });

                }
            });
        }

        //PopupMenu event handler
        function PopupMenu_AttachBugID_Clicked() {
            if (gGridView1_SelectedRow != null) {
                //lock screen
                var documentWidth = $(document).width();
                var documentHeight = $(document).height();
                $('#ScreenLocker').css({ 'height': documentHeight }).fadeIn('fast');

                //show form
                //init the form
                $('#txtBugID').val('');
                var formWidth = $("#AttachBugIDForm").width();
                var formHeight = $("#AttachBugIDForm").height();
                $("#AttachBugIDForm").css({ "margin-top": 0 - formHeight / 2, "margin-left": 0 - formWidth / 2 }).fadeIn('fast');
                $('#txtBugID').focus();
            }
        }
        function PopupMenu_CompareWith_Clicked() {
            if (gGridView1_SelectedRow != null) {
                //lock screen
                var documentHeight = $(document).height();
                $('#ScreenLocker').css({ 'height': documentHeight }).fadeIn('fast');

                //show form
                var selectedBuildNo = $($(gGridView1_SelectedRow).children()[1]).text();
                var selectedBuildLanguage = $($(gGridView1_SelectedRow).children()[2]).text();
                //init the form
                $('#<%=CompareWith_ddlBuildNo.ClientID %>').multiselect('uncheckAll').multiselect('widget').find(":checkbox").each(function () {
                    if (this.value.trim() == selectedBuildNo.trim()) {
                        this.click();
                    }
                });
                $('#<%=CompareWith_ddlBuildLanguage.ClientID %>').multiselect('uncheckAll').multiselect('widget').find(":checkbox").each(function () {
                    if (this.value.trim() == selectedBuildLanguage.trim()) {
                        this.click();
                    }
                });
                //show the form in the center of screen
                var formWidth = $("#CompareWithConditionForm").width();
                var formHeight = $("#CompareWithConditionForm").height();
                $("#CompareWithConditionForm").css({ "margin-top": 0 - formHeight / 2, "margin-left": 0 - formWidth / 2 }).fadeIn('fast');
            }
        }
        function PopupMenu_ShowAssemblyInfo_Clicked() {
            var resultID = $(gGridView1_SelectedRow).attr("resultID");
            var assemblyInfo = getAssemblyInfo(resultID);
        }
        function getAssemblyInfo(resultID) {
            $.ajax({
                url: 'Ajax.aspx',
                type: 'POST',
                data: {
                    IsAjax: true,
                    Method: 'Ajax_GetAssemblyInfo',
                    ResultID: resultID
                },
                beforeSend: function () {
                    $('#AssemblyInfoDialog').html('<img src="images/ajax-loader.gif"/>');
                    $('#AssemblyInfoDialog').dialog({
                        width: 'auto',
                        height: 'auto',
                        modal: true
                    });
                },
                error: function (data) {
                    //                                        alert(data.responseText);
                    $('#AssemblyInfoDialog').dialog('close');
                    alert("Fail to get assembly info!");
                },
                success: function (data) {
                    //error message
                    if (data.substring(0, 4) == "[E]:") {
                        //show error message
                        var errMsg = data.substring(4);
                        $('#AssemblyInfoDialog').dialog('close');
                        alert(errMsg);

                        return;
                    }

                    //show assembly info dialog
                    var $AssemblyInfoTable = $('#AssemblyInfoTableTemplate').clone();
                    var assemblyInfoArr = data.split('\n');
                    for (var index in assemblyInfoArr) {
                        var assemblyInfo = assemblyInfoArr[index].split('[|]');
                        var $row = $('<tr></tr>');
                        $('<td></td>').css({ padding: '0 5px 0 5px' }).text(assemblyInfo[0]).appendTo($row);
                        $('<td></td>').css({ padding: '0 5px 0 5px' }).text(assemblyInfo[1]).appendTo($row);
                        $('<td></td>').css({ padding: '0 5px 0 5px' }).text(assemblyInfo[2]).appendTo($row);
                        $row.appendTo($AssemblyInfoTable);
                    }
                    $('#AssemblyInfoDialog').html('').append($AssemblyInfoTable.show()).dialog('option', 'position', 'center');
                }
            });
        }
        function PopupMenu_ShowReproSteps_Clicked() {
            var resultID = $(gGridView1_SelectedRow).attr("resultID");
            $.ajax({
                url: 'Ajax.aspx',
                type: 'POST',
                data: {
                    IsAjax: true,
                    Method: 'Ajax_GetReproSteps',
                    ResultID: resultID
                },
                beforeSend: function () {
                    $('#ReproStepsDialog #LaunchedFrom').html('<img src="images/ajax-loader.gif"/>');
                    $('#ReproStepsDialog #WindowHierarchy').html('<img src="images/ajax-loader.gif"/>');
                    $('#ReproStepsDialog #CustomReproStep').html('<img src="images/ajax-loader.gif"/>');
                    $('#ReproStepsDialog').dialog({
                        width: 'auto',
                        height: 'auto',
                        modal: true,
                        buttons: {
                            Save: function () {
                                var launchedFrom = $('#ReproStepsDialog #LaunchedFrom input[type=text]').val();
                                var windowHierarchy = $('#ReproStepsDialog #WindowHierarchy input[type=text]').val();
                                var $customReproStep = $('#ReproStepsDialog #CustomReproStep textarea');
                                var customReproStep = '-1';
                                if ($customReproStep) {
                                    customReproStep = $customReproStep.val();
                                } 
                                $.ajax({
                                    url: 'Ajax.aspx',
                                    type: 'POST',
                                    data: {
                                        IsAjax: true,
                                        Method: 'Ajax_UpdateReproSteps',
                                        ResultID: resultID,
                                        LaunchedFrom: launchedFrom,
                                        WindowHierarchy: windowHierarchy,
                                        CustomReproStep: customReproStep
                                    },
                                    beforeSend: function () {
                                        //disable save button
                                        $($('#ReproStepsDialog').dialog('widget').find('button')[0]).attr('disabled', 'disabled');
                                    },
                                    error: function () {
                                        $('#ReproStepsDialog').dialog('close');
                                        alert("Fail to save repro steps!");
                                    },
                                    success: function (data) {
                                        $('#ReproStepsDialog').dialog('close');

                                        //message
                                        switch (data.substring(0, 4)) {
                                            case "[E]:":
                                            case "[W]:":
                                            case "[I]:":
                                                //show error message
                                                var errMsg = data.substring(4);
                                                alert(errMsg);
                                                return;
                                            default:
                                                break;
                                        }
                                    }
                                });
                            },
                            Cancel: function () {
                                $(this).dialog('close');
                            }
                        }
                    });
                },
                error: function (data) {
                    //                                        alert(data.responseText);
                    $('#ReproStepsDialog').dialog('close');
                    alert("Fail to get repro steps!");
                },
                success: function (data) {
                    //error message
                    if (data.substring(0, 4) == "[E]:") {
                        //show error message
                        var errMsg = data.substring(4);
                        $('#ReproStepsDialog').dialog('close');
                        alert(errMsg);

                        return;
                    }

                    //show repro steps dialog
                    var reproStepArr = data.split('[|]');
                    var width = 500;
                    //LaunchedFrom
                    var $txtLaunchedFrom = $('<input type="text" />').val(reproStepArr[0]);
                    $('#ReproStepsDialog #LaunchedFrom').html('').append($txtLaunchedFrom.width(width));
                    //WindowHierarchy
                    var $txtWindowHierarchy = $('<input type="text" />').val(reproStepArr[1]).width(width);
                    $('#ReproStepsDialog #WindowHierarchy').html('').append($txtWindowHierarchy);
                    //CustomReproStep
                    if (reproStepArr[2] == '-1') {
                        $('#ReproStepsDialog #CustomReproStep').html('No page identifier found.');
                    } else {
                        var $txtCustomReproStep = $('<textarea rows="10"></textarea>').val(reproStepArr[2]).width(width);
                        $('#ReproStepsDialog #CustomReproStep').html('').append($txtCustomReproStep);
                    }

                    //re-locate the dialog
                    $('#ReproStepsDialog').dialog('option', 'position', 'center');
                }
            });
        }
        function PopupMenu_RelatedBugs_MouseOver() {
            //avoid to non-sense multi-load sub menu
            if ($('#RelatedBugsSubMenu').is(':visible')) {
                return;
            }
            
            //display ajax loading icon
            $('#RelatedBugsSubMenu ul').html('<img src="Images/ajax-loader.gif"/>');
            //position
            var $srcElement = $(window.event.srcElement);
            var parentPosition = $srcElement.offset();
            var parentWidth = $srcElement.parent().width();
            $('#RelatedBugsSubMenu').css({
                top: parentPosition.top + 'px',
                left: (parentPosition.left + parentWidth) + 'px'
            }).slideDown('fast');

            var resultID = $(gGridView1_SelectedRow).attr('ResultID');
            getRelatedBugs(resultID);
        }
        function PopupMenu_RelatedBugs_MouseOut() {
            //keep submenu showing when move mouse on it
            if (window.event.toElement == null ||
                $(window.event.toElement).attr('Id') == 'RelatedBugsMenuItem' ||
                $(window.event.toElement).attr('Id') == 'RelatedBugsSubMenu' ||
                $('#RelatedBugsMenuItem').has(window.event.toElement).length != 0 ||
                $('#RelatedBugsSubMenu').has(window.event.toElement).length != 0) {
                return;
            }

            $('#RelatedBugsSubMenu').hide();
        }
        function PopupMenu_ShowScreenshot_Clicked() {
            showDetailInfoAfterRow(gGridView1_SelectedRow);
        }
        function PopupMenu_HideScreenshot_Clicked() {
            $('#Row_DetailInfo').hide();
        }


        //AttachBugIDForm event handler
        function AttachBugIDForm_OK_Clicked() {
            if (!$('#AttachBugIDForm').is(':visible')) {
                return;
            }

            var resultID = $(gGridView1_SelectedRow).attr("resultID");
            var bugIDs = $('#txtBugID').val();
            //validate input
            var errMsg = validateBugIDFormInput(bugIDs);
            if (errMsg != '') {
                alert(errMsg);
                return;
            }

            //ajax request
            attachBugID(bugIDs, resultID);

            //unload form
            unloadAttachedBugIDForm();
//            //reload report 
//            $('#<%=btnSearch.ClientID %>').click();
        }
        function AttachBugIDForm_Cancel_Clicked() {
            if (!$('#AttachBugIDForm').is(':visible')) {
                return;
            }

            //unload form
            unloadAttachedBugIDForm();
        }
        function validateBugIDFormInput(bugID) {
            //is empty
            if (bugID == '') {
                return 'BugID cannot be empty.';
            }

            //validate
            var bugIDArr = bugID.split(',');
            for (var index in bugIDArr) {
                var isValidBugID = /^(\d+)$/.test(bugIDArr[index]);
                if (!isValidBugID) {
                    return "[" + bugIDArr[index] + "] is invalid BugID.\nBugID must be positive integer.";
                }
            }

            return '';
        }
        function attachBugID(bugIDs, resultID) {
            $.ajax({
                url: 'Ajax.aspx',
                type: 'POST',
                data: {
                    IsAjax: true,
                    Method: 'Ajax_LinkBugID',
                    BugIDs: bugIDs,
                    ResultID: resultID
                },
                error: function (data) {
                    //                                        alert(data.responseText);
                    alert("Fail to attach bug ID!");
                },
                success: function (data) {
                    //error message
                    switch (data.substring(0, 4)) { 
                        case "[E]:":
                        case "[W]:":
                        case "[I]:":
                            //show error message
                            var errMsg = data.substring(4);
                            alert(errMsg);
                            return;
                        default:
                            break;
                    }
                }
            });
        }
        function unloadAttachedBugIDForm() {
            //unload form
            $('#AttachBugIDForm').hide();
            //unlock screen
            $('#ScreenLocker').hide();
        }

        //compare with... form
        function CompareWithConditionForm_OK_Clicked() {
            //make sure form is shown
            if (!$('#CompareWithConditionForm').is(':visible')) {
                return;
            }
            //validate
            var errMsg = validateCompareWithConditionForm();
            if (errMsg != '') {
                alert(errMsg);
                return;
            }

            //ResultID
            var resultID = $(gGridView1_SelectedRow).attr("resultID");
            //BuildNo
            var buildNo_checkedItemValues = $('#<%=CompareWith_ddlBuildNo.ClientID %>').multiselect('getChecked').map(function(){
                return this.value;
            }).get().join(',');
            //BuildLanguage
            var buildLanguage_checkedItemValues = $('#<%=CompareWith_ddlBuildLanguage.ClientID %>').multiselect('getChecked').map(function(){
                return this.value;
            }).get().join(',');

            //ajax request
            $.ajax({
                url: 'Ajax.aspx',
                type: 'POST',
                data: {
                    IsAjax: true,
                    Method: 'Ajax_CompareWith',
                    ResultID: resultID,
                    BuildNos: buildNo_checkedItemValues,
                    BuildLanguages: buildLanguage_checkedItemValues,
                    IsFuzzySearch: true
                },
                beforeSend: function () {
                    //title
                    var $seletedRow_Cells = $(gGridView1_SelectedRow).children('td');
                    var buildNo = $($seletedRow_Cells.get(1)).text().trim();
                    var buildLanguage = $($seletedRow_Cells.get(2)).text().trim();
                    var rule = $($seletedRow_Cells.get(3)).text().trim();
                    var title = 'Search Result (BuildNo:' + buildNo + ', BuildLanguage:' + buildLanguage + ', Rule:' + rule + ')';
                    $('#PopupContainer #Title').text(title);

                    //ajax loading
                    $('#PopupContainer #Content').html('<img src="Images/ajax-loader.gif"/>');
                },
                error: function (data) {
                    //                                        alert(data.responseText);
                    alert("Ajax request error!");
                },
                success: function (data) {
                    //clear content
                    var $content = $('#PopupContainer #Content').html('');

                    //error message
                    if (data.substring(0, 4) == "[E]:") {
                        //show error message
                        var errMsg = data.substring(4);
                        alert(errMsg);
                        //close result form
                        $('#PopupContainer #Title img').click();

                        return;
                    }

                    //\n is separator for each dispalyed item
                    var arrItem = data.split('[$]');
                    var $row = null;
                    var count = 0;
                    var buildsWithNoResult = '';
                    for (var index in arrItem) {
                        //[|] is separator for item detail info
                        var arrItemDetail = arrItem[index].split('[|]');
                        var build = arrItemDetail[0].trim();
                        var logMessage = arrItemDetail[1].trim();
                        var imagePath = arrItemDetail[2].trim();
                        var result = arrItemDetail[3].trim();

                        //no result build
                        if (!imagePath) {
                            //the last row
                            if (index == arrItem.length - 1) {
                                //end line
                                if ($row && !$row.has('<br />').length) {
                                    $row.width($row.children().width() * 4 + 8);
                                    $('<br />').appendTo($row);
                                }
                            }

                            //cache build without result
                            buildsWithNoResult += build + ', ';
                            continue;
                        }

                        //image block item
                        var $imageBlockItem = $('#ImageBlockTemplate').clone().show();
                        $imageBlockItem.find('[name=Build]').text(build);
                        var $result = $imageBlockItem.find('[name=Result]');
                        $result.text(result);
                        //mark color for result
                        switch (result) {
                            case 'Pass':
                                $result.css({ color: 'green' });
                                break;
                            case 'Fail':
                                $result.css({ color: 'red' });
                                break;
                            case 'Warning':
                                $result.css({ color: 'GoldenRod' });
                                break;
                            default:
                                $result.css({ color: 'black' });
                                break;

                        }
                        $imageBlockItem.children('[name=LogMessage]').text(logMessage).hide();
                        $imageBlockItem.children('img').attr('src', imagePath);

                        //new row
                        if (count % 4 == 0) {
                            //new line
                            $row = $('<div class="alginment-row"></div>');
                            $row.appendTo($content);
                        }

                        //append
                        $imageBlockItem.appendTo($row);
                        count++;

                        //row is full or the last one
                        if (count % 4 == 0 || index == arrItem.length - 1) {
                            //end line
                            if ($row) {
                                $row.width($row.children().width() * 4 + 8);
                                $('<br />').appendTo($row);
                            }
                        }
                    }

                    //no result found
                    if (buildsWithNoResult) {
                        buildsWithNoResult = buildsWithNoResult.replace(/,\s*$/g, '');
                        var txtNoResultFound = 'No result found for these builds: <br/>' + buildsWithNoResult;
                        $row = $('<div style="text-align:left; margin:20px 0 10px 5px; color:gray; font-size:1.3em;"></div>').html(txtNoResultFound);
                        $row.appendTo($content);
                    }

                    $content.width($content.children().width());
                }
            });

            //popup container
            var documentHeight = $(document).height();
            $('#PopupContainer').css({ 'height': documentHeight, top: documentHeight }).show().animate({ top: 0 }, 1000);

            //unload condition form
            unloadCompareWithConditionForm();
        }
        function ImageBlockTemplate_Clicked(obj) {
            var $zoomOutImageBlock = $(obj).parent('#ImageBlockTemplate');
            var $zoomInImageBlock = $('#ZoomInImageBlock');
            $zoomInImageBlock.find('[name=Build]').text($zoomOutImageBlock.find('[name=Build]').text());
            var $result = $zoomInImageBlock.find('[name=Result]');
            var resultValue = $zoomOutImageBlock.find('[name=Result]').text();
            $result.text(resultValue);
            //mark color for result
            switch (resultValue) {
                case 'Pass':
                    $result.css({ color: 'green' });
                    break;
                case 'Fail':
                    $result.css({ color: 'red' });
                    break;
                case 'Warning':
                    $result.css({ color: 'GoldenRod' });
                    break;
                default:
                    $result.css({ color: 'black' });
                    break;
            }
            $zoomInImageBlock.children('[name=LogMessage]').text($zoomOutImageBlock.children('[name=LogMessage]').text());
            $zoomInImageBlock.children('img').attr('src', $zoomOutImageBlock.children('img').attr('src'));
            $zoomInImageBlock.children('img').one('load', function () {
                //image info
                var image = new Image();
                image.src = this.src;
                $zoomInImageBlock.children('[name=LogMessage]').width(image.width);
                $zoomInImageBlock.width(image.width).height('auto');

                //lock screen
                var documentoffsetWidth = document.documentElement.offsetWidth;
                var documentoffsetHeight = document.documentElement.offsetHeight;
                var documentHeight = $(document).height();
                $('#ScreenLocker').css({ 'height': documentHeight }).show();
                $(document.body).css('overflow', 'hidden');

                //show the form in the center of screen
                var formWidth = $zoomInImageBlock.width()
                var formHeight = $zoomInImageBlock.height();
                $zoomInImageBlock.css({ 'top': '50%', 'margin-top': 0 - formHeight / 2, 'left': '50%', 'margin-left': 0 - formWidth / 2, 'overflow-x': 'hidden', 'overflow-y': 'hidden' }).show();
                var hasVerticalScroll = false;
                if (formHeight > documentoffsetHeight) {
                    $zoomInImageBlock.css({ height: documentoffsetHeight, 'top': '0', 'margin-top': '0', 'overflow-y': 'scroll' });
                    hasVerticalScroll = true;
                }
                if (formWidth > documentoffsetWidth) {
                    $zoomInImageBlock.css({ width: documentoffsetWidth, 'left': '0', 'margin-left': '0', 'overflow-x': 'scroll' });
                    //leave space for vertical scrollbar
                    if (hasVerticalScroll) {
                        $zoomInImageBlock.width(documentoffsetWidth - 20);
                    }
                }
            });
        }
        function ZoomInImageBlock_Clicked(obj) {
            $(obj).parent('#ZoomInImageBlock').hide();
            $('#ScreenLocker').hide();
            $(document.body).css('overflow', 'scroll');
        }
        function CompareWithConditionForm_Cancel_Clicked() {
            //make sure form is shown
            if (!$('#CompareWithConditionForm').is(':visible')) {
                return;
            }

            //unload form
            unloadCompareWithConditionForm();
        }
        function validateCompareWithConditionForm() {
            //BuildNo.
            var buildNo_checkedItemArr = $('#<%=CompareWith_ddlBuildNo.ClientID %>').multiselect('getChecked');
            if (buildNo_checkedItemArr.length <= 0)
                return 'Please select BuildNo.!';
            //BuildLanguage
            var buildLanguage_checkedItemArr = $('#<%=CompareWith_ddlBuildLanguage.ClientID %>').multiselect("getChecked");
            if (buildLanguage_checkedItemArr.length <= 0)
                return 'Please select BuildLanguage!';

            return '';
        }
        function unloadCompareWithConditionForm() {
            //unload form
            $('#CompareWithConditionForm').hide();
            //unlock screen
            $('#ScreenLocker').hide();
        }

        //related bugs
        function getRelatedBugs(resultID) {
            $.ajax({
                url: 'Ajax.aspx',
                type: 'POST',
                data: {
                    IsAjax: true,
                    Method: 'Ajax_GetRelatedBugs',
                    ResultID: resultID
                },
                error: function (data) {
                    //                    alert(data.responseText);
                    alert("Ajax request error!");
                },
                success: function (data) {
                    //clear the loading icon
                    $('#RelatedBugsSubMenu ul').html('');

                    //message
                    switch (data.substring(0, 4)) {
                        case "[E]:":
                        case "[W]:":
                        case "[I]:":
                            //show message
                            var errMsg = data.substring(4);
                            $('<li></li>').text(errMsg).appendTo('#RelatedBugsSubMenu > ul');
                            return;
                        default:
                            break;
                    }

                    //No bug found
                    if (!data) {
                        $('<li></li>').text('No related bug found.').appendTo('#RelatedBugsSubMenu > ul');
                        return;
                    }

                    //\n is separator for data row
                    var arrBugs = data.split('\n');
                    for (var index in arrBugs) {
                        //[|] is separator for data field
                        var arrBugRow = arrBugs[index].split('[|]');
                        var bugID = arrBugRow[0];
                        var bugTitle = arrBugRow[1];
                        var bugStatus = arrBugRow[2];
                        var menuItem = $('<li title="Click to copy BugID" onclick="copyBugID(this)"></li>').appendTo('#RelatedBugsSubMenu > ul');
                        menuItem.append($('#CopyIcon').clone().show()).append('  ');
                        menuItem.append(bugID + ' ' + bugTitle);
                        menuItem.attr('bugID', bugID);
                        switch (bugStatus) {
                            case 'Active':
                                menuItem.css({ 'color': 'Red' });
                                break;
                            case 'Resolved':
                                menuItem.css({ 'color': 'GoldenRod' });
                                break;
                            case 'Closed':
                                menuItem.css({ 'color': 'Green' });
                                break;
                            default:
                                menuItem.css({ 'color': 'Black' });
                                break;
                        }
                    }
                }
            });
        }
        function copyBugID(menuItem) {
            copyToClipboard($(menuItem).attr('bugID'));
        }
        function copyToClipboard(text) {
            if (window.clipboardData) {
                window.clipboardData.setData('text', text);
            }
        }
        function ConditionToggle_Clicked() {            
            //toggle button
            if ($('.condition-toggle').text().contains('Show')) {
                //show
                $('.condition-toggle').text('Hide Search Condition<<');
                //search condition
                $('.search-condition-content').show();
            } else {
                //hide
                $('.condition-toggle').text('Show Search Condition>>');
                //search condition
                $('.search-condition-content').hide();
            }
        }
        var fieldSplitter = ',\n';
        var rowSplitter = '$';
        var gSearchConditionArr = $('#<%=hdSearchCondition.ClientID %>').val().split(rowSplitter);
        function btnSearch_Clicked() {
            //validate
            var errMsg = validateInput();
            if (errMsg != '') {
                alert(errMsg);
                return false;
            }

            var searchCondition = '';
            for(var index in gConditionFieldArr)
            {
                if(gConditionFieldArr[index].Type == "select")
                {
                    var $field = $('#MainContent_ddl' + gConditionFieldArr[index].Name);
                    if ($('#SearchCondition_' + gConditionFieldArr[index].Name).text() == '<All>') {
                        searchCondition += rowSplitter + '-1';
                    } else {
                        searchCondition += rowSplitter + $field.multiselect('getChecked').map(function () {
                            return this.value.trim();
                        }).get().join(fieldSplitter);
                    }
                } else if(gConditionFieldArr[index].Type == "text") {
                    var $field = $('#MainContent_txt' + gConditionFieldArr[index].Name);
                    searchCondition += rowSplitter + $field.val();
                }
            }

            var escapedRowSplitter = escapeUserInput(rowSplitter);
            var regExp = new RegExp('^' + escapedRowSplitter + '{1}', 'g');
            searchCondition = searchCondition.replace(regExp, '');
            $('#<%=hdSearchCondition.ClientID %>').val(searchCondition);

            return true;
        }
        function validateInput() {
            for(var index in gConditionFieldArr)
            {
                if(gConditionFieldArr[index].Type == "select")
                {
                    if ($('#SearchCondition_' + gConditionFieldArr[index].Name).text() == '')
                        return gConditionFieldArr[index].Name + ' cannot be empty.';
                }
            }

            return '';
        }
        function showErrorTipPanel(top, left, message, timeout) {
            var maxIntervalCnt = 6;
            var intervalCnt = 0;
            $('#ErrorTipPanel').text(message).css({
                top: top,
                left: left
                //flash the text
            }).show('fast', function () {
                var interval = setInterval(function () {
                    if (intervalCnt > maxIntervalCnt) {
                        clearInterval(interval);
                        setTimeout(function () {
                            $('#ErrorTipPanel').fadeOut(3000);
                        }, timeout ? timeout : 0);
                        $('#ErrorTipPanel').css('color', 'Red').removeAttr('color');
                    } else {
                        var $errorTipPanel = $('#ErrorTipPanel');
                        if ($errorTipPanel.attr('color') == 'Red') {
                            $errorTipPanel.css('color', 'Yellow');
                            $errorTipPanel.attr('color', 'Yellow');
                        } else {
                            $errorTipPanel.css('color', 'Red');
                            $errorTipPanel.attr('color', 'Red');
                        }

                        //cnt
                        intervalCnt++;
                    }
                }, 300);
            });
        }
        var gConditionFieldArr =
        [
            { Name: "BuildNo", Type: "select", options: {
                    close: function () {
                        refreshBuildLanguage();
                    }
                }
            },
            { Name: "BuildLanguage", Type: "select" },
            { Name: "OSType", Type: "select" },
            { Name: "OSLanguage", Type: "select" },
            { Name: "Rule", Type: "select" },
            { Name: "Result", Type: "select" },
            { Name: "AssemblyName", Type: "select", options: {
                    close: function () {
                        refreshFullTypeName();
                    }
                }
            },
            { Name: "PageTitle", Type: "text" },
            { Name: "FullTypeName", Type: "select" }
        ];
        function refreshBuildLanguage(callback) {
            var buildNos = $('#<%=ddlBuildNo.ClientID %>').multiselect('getChecked').map(function () {
                return this.value;
            }).get().join(',');

            $.ajax({
                url: 'Ajax.aspx',
                type: 'POST',
                data: {
                    IsAjax: true,
                    Method: 'Ajax_GetBuildLanguage',
                    BuildNos: buildNos
                },
                beforeSend: function () {
                    //disable ddlBuildLanguage
                    $('#<%=ddlBuildLanguage.ClientID %>').multiselect().multiselect('disable');
                },
                error: function (data) {
                    //                    alert(data.responseText);
                    alert("Ajax request error!");
                },
                success: function (data) {
                    //no BuildLanguage found
                    if (data == '') {
                        $('#<%=ddlBuildLanguage.ClientID %>').empty();
                        return;
                    }

                    //error message
                    if (data.substring(0, 4) == "[E]:") {
                        //show error message
                        var errMsg = data.substring(4);
                        alert(errMsg);

                        return;
                    }

                    //re-load BuildLanguage
                    var $buildLanguageSelect = $('#<%=ddlBuildLanguage.ClientID %>');
                    //remember the checkebox state
                    var checkedBuildLanguageValueArr = $buildLanguageSelect.multiselect('getChecked').map(function () {
                        return this.value;
                    }).get();
                    $buildLanguageSelect.empty();
                    var arrData = data.split('[|]');
                    //load new list items
                    for (var index in arrData) {
                        $('<option />', { text: arrData[index], value: arrData[index] }).appendTo($buildLanguageSelect);
                    }
                    //restore checkbox state
                    if ($('#SearchCondition_BuildLanguage').text() == '<All>') {
                        $buildLanguageSelect.multiselect('refresh').multiselect('checkAll');
                    } else {
                        $buildLanguageSelect.multiselect('refresh').multiselect('uncheckAll').multiselect('widget').find(':checkbox').each(function () {
                            if ($.inArray(this.value, checkedBuildLanguageValueArr) != -1) {
                                this.click();
                            }
                        });
                    }

                    //callback
                    if (callback) {
                        callback();
                    }

                    //enable ddlBuildLanguage
                    $('#<%=ddlBuildLanguage.ClientID %>').multiselect('enable');
                }
            });
        };
        function refreshFullTypeName(callback) {
            var assemblyNames = '';
            if ($('#SearchCondition_AssemblyName').text() == '<All>') {
                assemblyNames = '-1';
            } else {
                assemblyNames = $('#<%=ddlAssemblyName.ClientID %>').multiselect('getChecked').map(function () {
                    return this.value;
                }).get().join(',');
            }

            $.ajax({
                url: 'Ajax.aspx',
                type: 'POST',
                data: {
                    IsAjax: true,
                    Method: 'Ajax_GetFullTypeName',
                    AssemblyNames: assemblyNames
                },
                beforeSend: function () {
                    //disable ddlFullTypeName
                    $('#<%=ddlFullTypeName.ClientID %>').multiselect().multiselect('disable');
                },
                error: function (data) {
                    //                    alert(data.responseText);
                    alert("Ajax request error!");
                },
                success: function (data) {
                    //no FullTypeName found
                    if (data == '') {
                        $('#<%=ddlFullTypeName.ClientID %>').empty();
                        return;
                    }

                    //error message
                    if (data.substring(0, 4) == "[E]:") {
                        //show error message
                        var errMsg = data.substring(4);
                        alert(errMsg);

                        return;
                    }

                    //re-load FullTypeName
                    var $fullTypeNameSelect = $('#<%=ddlFullTypeName.ClientID %>');
                    $fullTypeNameSelect.empty();
                    var arrData = data.split('\n');
                    for (var index in arrData) {
                        var fullType_id_name_pair = arrData[index].split('[|]');
                        var opt = $('<option />', { text: fullType_id_name_pair[1], value: fullType_id_name_pair[0] });
                        opt.appendTo($fullTypeNameSelect);
                    }
                    $fullTypeNameSelect.multiselect('refresh').multiselect('checkAll');

                    //callback
                    if (callback) {
                        callback();
                    }

                    //enable ddlFullTypeName
                    $('#<%=ddlFullTypeName.ClientID %>').multiselect('enable');
                }
            });
        };
        function loadCustomControl() {
            $('#<%=CompareWith_ddlBuildNo.ClientID %>').multiselect({ minWidth: '200', noneSelectedText: '0 selected' });
            $('#<%=CompareWith_ddlBuildLanguage.ClientID %>').multiselect({ minWidth: '200', noneSelectedText: '0 selected' });

            for(var index in gConditionFieldArr)
            {
                if(gConditionFieldArr[index].Type == "select")
                {
                    loadMultiSelect4SearchCondition(gConditionFieldArr[index].Name, gConditionFieldArr[index].MaxSelection, index, gConditionFieldArr[index].options);
                } else if(gConditionFieldArr[index].Type == "text"){
                    loadText4SearchCondition(gConditionFieldArr[index].Name, index);
                }
            }

            //re-load BuildLanguage
            refreshBuildLanguage(function () {
                var selectedBuildLanguageArr = gSearchConditionArr[1].split(fieldSplitter);
                $('#<%=ddlBuildLanguage.ClientID %>').multiselect('uncheckAll').each(function () {
                    if (gSearchConditionArr[1] == '-1') {
                        $(this).multiselect('checkAll');
                    }
                    //for checking each item
                }).multiselect("widget").find('input:not(:checked)').each(function () {
                    if ($.inArray($(this).val().trim(), selectedBuildLanguageArr) !== -1) {
                        this.click();
                    }
                });
            });
            //re-load FullTypeName
            refreshFullTypeName(function () {
                var selectedFullTypeArr = gSearchConditionArr[8].split(fieldSplitter);
                $('#<%=ddlFullTypeName.ClientID %>').multiselect('uncheckAll').each(function () {
                    if (gSearchConditionArr[8] == '-1') {
                        $(this).multiselect('checkAll');
                    }
                    //for checking each item
                }).multiselect("widget").find('input:not(:checked)').each(function () {
                    if ($.inArray($(this).val().trim(), selectedFullTypeArr) !== -1) {
                        this.click();
                    }
                });
            });
        }
		function loadMultiSelect4SearchCondition(fieldName, maxSelection, searchConditionPosition, options){
			var beforeClick = null;
			if(maxSelection){
				beforeClick = function(src){
					//max selection validation
			        if ($(src).multiselect("widget").find("input:checked").length > maxSelection) {
			            var position = $(src).multiselect("widget").position();
			            showErrorTipPanel(position.top, position.left, 'You cannot select more than ' + maxSelection + ' items!', 2000);
			            return false;
			        }

                    return true;
				}
			}
			
			var $field = $('#MainContent_ddl' + fieldName);
			var selectedItemArr = gSearchConditionArr[searchConditionPosition].split(fieldSplitter);
			var $searchConditionField = $('#SearchCondition_' + fieldName);
            //define multiselect option
			var objOptions = {
			    minWidth: '200',
			    noneSelectedText: '0 selected',
			    create: function (e) {
			        $searchConditionField.text($(this).children('option:selected').text().trim());
			    },
			    checkAll: function (e) {
			        $searchConditionField.text('<All>');
			    },
			    uncheckAll: function (e) {
			        $searchConditionField.text('');
			    },
			    click: function (e, option) {
			        if (beforeClick && !beforeClick(this)) {
			            return false;
			        }

			        //for checking one item
			        if (option.checked) {
			            if ($searchConditionField.text() == '') {
			                $searchConditionField.text(option.text.trim());
			            } else {
			                var newText = $searchConditionField.text() + fieldSplitter + option.text.trim();
			                $searchConditionField.text(newText);
			            }
			            //for unchecking one item
			        } else if ($searchConditionField.text() == '<All>') {
			            var arrCheckedItem = $(this).multiselect('getChecked').toArray();
			            var newText = '';
			            for (var index in arrCheckedItem) {
			                if (index == 0) {
			                    newText = arrCheckedItem[index].title.trim();
			                } else {
			                    newText += fieldSplitter + arrCheckedItem[index].title.trim();
			                }
			            }
			            //set text
			            $searchConditionField.text(newText);
			        } else {
			            var escapedText = escapeUserInput(option.text.trim());
			            var regExpInHeaderEnd = new RegExp('^' + escapedText + fieldSplitter + '{1}|' + fieldSplitter + '{1}' + escapedText + '$|^' + escapedText + '$', 'g');
			            var regExpInMiddle = new RegExp(fieldSplitter + '{1}' + escapedText + fieldSplitter + '{1}', 'g');
			            //remove text and the splitter
			            var newText = $searchConditionField.text().replace(regExpInMiddle, fieldSplitter).replace(regExpInHeaderEnd, '');
			            $searchConditionField.text(newText);
			        }
			    },
			    beforeclose: function () {
			        //cannot be empty
			        if ($(this).multiselect('getChecked').length == 0) {
			            var widgetPosition = $(this).multiselect('widget').position();
			            showErrorTipPanel(widgetPosition.top, widgetPosition.left, 'Please select at least one item.', 2000);
			            return false;
			        }
			        return true;
			    }
			};
			//maxSelection header text
            if(maxSelection)
            {
                objOptions.header = 'Choose maximally ' + maxSelection + ' items!';  
            }
			for(var key in options)
			{
                objOptions[key] = options[key];
			}

            //apply multiselect options
			$field.multiselect(objOptions)
			    //for checking all
            .multiselect('uncheckAll').each(function () {
			    if (gSearchConditionArr[searchConditionPosition] == '-1') {
			        $(this).multiselect('checkAll');
			    }
			    //for checking each item
			}).multiselect("widget").find('input:not(:checked)').each(function () {
			    if ($.inArray($(this).val().trim(), selectedItemArr) != -1) {
			        this.click();
			    }
			});
		}
		function loadText4SearchCondition(fieldName, searchConditionPosition)
        {
            var $field = $('#MainContent_txt' + fieldName);
            var $searchConditionField = $('#SearchCondition_' + fieldName);
            $field.addClass('ui-corner-all')
            .change(function () {
                $searchConditionField.text($(this).val());
                //for initialization
            }).each(function () {
                $(this).val(gSearchConditionArr[searchConditionPosition]).change();
            });
        }
    </script>
</asp:Content>

