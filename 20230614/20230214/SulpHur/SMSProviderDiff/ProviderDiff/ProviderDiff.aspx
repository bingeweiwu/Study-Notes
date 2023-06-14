<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ProviderDiff.aspx.cs" Inherits="ProviderDiff.Index" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Provider Diff</title>
    <link rel="Stylesheet" type="text/css" href="DefaultStyleSheet.css" />
    <script type="text/javascript" src="http://ajax.microsoft.com/ajax/jQuery/jquery-1.4.1.js"></script>
    <script type="text/javascript" src="http://ajax.microsoft.com/ajax/jQuery/jquery-1.4.1-vsdoc.js"></script>
    <script type="text/javascript" src="jquery.bpopup-0.7.0.min.js"></script>
    <script type="text/javascript">
        $.ajaxSetup({
            cache: false
        });

        var detailsLeft;
        var detailsTop;

        $(document).ready(function () {
            $(document).mousemove(function (e) {
                detailsLeft = e.pageX;
                detailsTop = e.pageY;
            });

            $('#DetailViewContainer').click(function () {
                $(this).hide();
            });

        })

        var oldgridSelectedColor;
        var oldgridSeleectedForColor;
        var objOld;


        function setMouseOverColor(element) {
            oldgridSelectedColor = element.style.backgroundColor;
            element.style.backgroundColor = 'yellow';
            element.style.cursor = 'hand';
            element.style.textDecoration = 'underline';
        }

        function setMouseOutColor(element) {
            element.style.backgroundColor = oldgridSelectedColor;
            element.style.color = oldgridSeleectedForColor;
        }

        function setMouseOut(element) {
            $('#DetailViewContainer').hide();
        }

        function setMouseClick(element) {
            $('#DetailViewContainer').hide();
            if (objOld != null) {
                objOld.style.backgroundColor = oldgridSelectedColor;
                objOld.style.color = oldgridSeleectedForColor;
            }
            oldgridSelectedColor = element.style.backgroundColor;
            oldgridSeleectedForColor = element.style.color;
            element.style.backgroundColor = "#C5BBAF";
            element.style.color = "#333333";
            objOld = element;
        }

        function setMouseOverClass(element) {
            element = element.parentNode;
            if (objOld != null) {
                objOld.style.backgroundColor = oldgridSelectedColor;
                objOld.style.color = oldgridSeleectedForColor;
            }
            oldgridSelectedColor = element.style.backgroundColor;
            oldgridSeleectedForColor = element.style.color;
            element.style.backgroundColor = "#C5BBAF";
            element.style.color = "#333333";
            objOld = element;
            var resultID = element.cells[0].children[0].value;
            var pageName = element.cells[1].children[0].value;
            var assemblyName = element.cells[7].innerText;
            getClass(resultID);
        }

        function setMouseOverImg(element) {
            element = element.parentNode;
            if (objOld != null) {
                objOld.style.backgroundColor = oldgridSelectedColor;
                objOld.style.color = oldgridSeleectedForColor;
            }
            oldgridSelectedColor = element.style.backgroundColor;
            oldgridSeleectedForColor = element.style.color;
            element.style.backgroundColor = "#C5BBAF";
            element.style.color = "#333333";
            objOld = element;
            var resultID = element.cells[0].children[0].value;
            var pageName = element.cells[1].children[0].value;
            var assemblyName = element.cells[5].innerText;
            getDetailInfo(resultID, pageName, assemblyName);

        }

        function getClass(resultID) {
            $.ajax({
                beforeSend: function () {
                    $('#loading').show().html('<img src="img/ajax-loader.gif"/>').css({ 'left': detailsLeft, 'top': detailsTop, 'position': 'fixed', 'z-index': '100' });
                },
                url: 'AjaxQuery.aspx',
                type: 'POST',
                data: {
                    IsAjax: true,
                    Method: 'Ajax_GetClass',
                    ResultID: resultID
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
                    $('#PreScreenShot').html('');
                    $('#PostScreenShot').html('');
                    $('#preTD').css({ 'background-color': '#FFFFCC' });
                    $('#postTD').css({ 'background-color': '#99CCCC' });
                    $('#DetailViewContainer').find('#PreClass').html(arrData[0]);

                    $('#DetailViewContainer').find('#PostClass').html(arrData[1]);

                    $('#loading').hide().html('');
                    var SelectedVal = $('#<%=ddBuild.ClientID %>').val();
                    var sResult = SelectedVal.split("-");
                    $('#LabelPreBuild').text(sResult[0]);
                    $('#LabelPostBuild').text(sResult[1]);
                    var maskHeight = $(document).height();
                    var maskWidth = $(window).width();
                    var dialogTop = (maskHeight / 3) - ($('#dialog-box').height());
                    var dialogLeft = (maskWidth / 2) - ($('#dialog-box').width() / 2);
                    $('#DetailViewContainer').bPopup({ follow: [true, true], position: [10, 10] });
                }
            });
        }

        function getDetailInfo(resultID, pageName, assemblyName) {
            $.ajax({
                beforeSend: function () {
                    $('#loading').show().html('<img src="img/ajax-loader.gif"/>').css({ 'left': detailsLeft, 'top': detailsTop, 'position': 'fixed', 'z-index': '100' });
                },
                url: 'AjaxQuery.aspx',
                type: 'POST',
                data: {
                    IsAjax: true,
                    Method: 'Ajax_GetImage',
                    ResultID: resultID,
                    PageName: pageName,
                    AssemblyName: assemblyName
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
                    var showStatus = arrData[2];
                    $('#DetailViewContainer').find('#PreClass').html('');
                    $('#DetailViewContainer').find('#PostClass').html('');
                    $('#PreScreenShot').html('');
                    $('#PostScreenShot').html('');
                    $('#LabelPreBuild').text('');
                    $('#LabelPostBuild').text('');
                    $('#preTD').css({ 'background-color': '#FFFFCC' });
                    $('#postTD').css({ 'background-color': '#99CCCC' });

                    if (showStatus == "build") {
                        var url = "Image.aspx?ImageID=" + arrData[0];
                        /**$('#PreScreenShot').html('<asp:Image ID="Image1" runat="server" ImageUrl="' + url + '" />');**/
                        $('#PreScreenShot').html('<img  src="' + url + '" runat="server" alt="image for this build not found"/>');

                        url = "Image.aspx?ImageID=" + arrData[1];
                        /** $('#PostScreenShot').html('<asp:Image ID="Image2" runat="server" ImageUrl="' + url + '" />');**/
                        $('#PostScreenShot').html('<img  src="' + url + '" alt="image for this build not found"/>');


                        var SelectedVal = $('#<%=ddBuild.ClientID %>').val();
                        var sResult = SelectedVal.split("-");
                        $('#LabelPreBuild').text(sResult[0]);
                        $('#LabelPostBuild').text(sResult[1]);
                        /*
                        var maskHeight = $(document).height();
                        var maskWidth = $(window).width();
                        var dialogTop = (maskHeight / 3) - ($('#dialog-box').height());
                        var dialogLeft = (maskWidth / 2) - ($('#dialog-box').width() / 2);
                        var width = $('#DetailViewContainer').css('width');
                        */
                    } else if (showStatus == "latest") {
                        var buildNum = arrData[4];
                        $('#LabelPreBuild').text("neither of the two build image are found, show the latest(buildno=" + buildNum + ") image");
                        var url = "Image.aspx?ImageID=" + arrData[3];
                        $('#PreScreenShot').html('<img  src="' + url + '" runat="server" alt="image for this build not found"/>');
                        $('#postTD').css({ 'background-color': 'white' });
                    } else if (showStatus == "none") {
                        $('#LabelPreBuild').text("No image for the two build are found.");
                        $('#postTD').css({ 'background-color': 'white' });
                    }

                    $('#loading').hide().html('');
                    $('#DetailViewContainer').bPopup({ follow: [true, true], position: [10, 10] });
                }
            });
        }

        function RowSelect() {
            var Ptr = document.getElementById("Gridview1").getElementsByTagName("tr");
            for (var i = 1; i < Ptr.length; i++) {
                Ptr[i].onclick = function Clk() {
                    this.className = "Tr_Click";
                }
            }
        }

        function ExpandCollapseToolBar() {
            obj = document.getElementById("ToolBar");

            if (obj.style.display == "none" || obj.style.display == "") {
                obj.style.display = "block";
                /*
                objButton.value = "\u25B2";
                */
            } else {
                obj.style.display = "none";
                /*
                objButton.value = "\u25BC";
                */
            }
        }

        function ExpandCollapseDetails() {
            obj = document.getElementById("DetailView");

            if (obj.style.display == "none" || obj.style.display == "") {
                obj.style.display = "block";
            } else {
                obj.style.display = "none";
            }
        }

        function RunDiff() {
            $('#errorMsg').text("");
            var preNumber = $('#preBuildNumber').val();
            var postNumber = $('#postBuildNumber').val();
            var isoverwrite = $('#checkboxRerun').attr('checked');

            if (preNumber != "" && postNumber != "") {
                var value1 = preNumber.replace(/^\s\s*/, '').replace(/\s\s*$/, '');
                var value2 = postNumber.replace(/^\s\s*/, '').replace(/\s\s*$/, '');
                var intRegex = /^\d+$/;
                if (!intRegex.test(value1) || !intRegex.test(value2)) {
                    success = false;
                } else {
                    success = true;
                }
            } else {
                success = false;
            }

            if (!success) {
                return $('#errorMsg').text("Please input valid build number.");
            }

            $.ajax({
                beforeSend: function () {
                    $('#errorMsg').html('<img src="img/ajax-loader.gif"/>');
                },
                url: 'AjaxQuery.aspx',
                type: 'POST',
                data: {
                    IsAjax: true,
                    Method: 'Ajax_RunDiff',
                    PreBuildNumber: preNumber,
                    PostBuildNumber: postNumber,
                    IsOverWrite: isoverwrite
                },
                error: function (data) {
                    //                    alert(data.responseText);
                    alert("Ajax request error!");
                },
                success: function (data) {
                    $('#errorMsg').html('');

                    if (data.substring(0, 4) == "[E]:") {
                        var errMsg = data.substring(4);

                        $('#errorMsg').text(errMsg);

                        return;
                    }
                    else {

                    }
                }
            });
        }

    </script>
</head>
<body>
    <form id="form1" runat="server" class="form">
    <div id="ToolBarContainer">
        <div id="ToolBar">
            <table class="toolbarTable">
                <tr>
                    <td>
                        <label>
                            Pre Build:</label>
                    </td>
                    <td>
                        <input type="text" id="preBuildNumber" name="preBuildNumber" />
                    </td>
                    <td>
                        <label>
                            Post Build:</label>
                    </td>
                    <td>
                        <input type="text" id="postBuildNumber" name="postBuildNumber" />
                    </td>
                </tr>
                <tr>
                    <td colspan="4">
                        <input id="checkboxRerun" type="checkbox" name="Rerun" value="Rerun" />overwrite
                        existing result
                    </td>
                </tr>
                <tr>
                    <td colspan="2" class="submit">
                        <input id="btnSubmit" type="button" value="Submit" onclick="javascript:RunDiff()" />
                    </td>
                    <td colspan="2" style="color: Red">
                        <label id="errorMsg" runat="server">
                        </label>
                    </td>
                </tr>
            </table>
        </div>
        <div id="ExpandButtonContainer">
            <input type="button" id="btnToolbar" class="ecButton" value="ToolBar" onclick="javascript:ExpandCollapseToolBar()" />
        </div>
    </div>
    <div id="MainGrid">
        <div id="gridContainer">
            <div class="styled-select">
                <asp:DropDownList ID="ddDiffType" DataSourceID="SqlDataSourceDiffType" AutoPostBack="true"
                    DataValueField="DiffType" runat="server" Width="140px" Font-Size="11px" AppendDataBoundItems="true">
                    <asp:ListItem Text="All" Value="%"></asp:ListItem>
                </asp:DropDownList>
                <asp:DropDownList ID="ddSourceFile"  runat="server" Width="140px" AutoPostBack="true"
                    Font-Size="11px" AppendDataBoundItems="true">
                    <asp:ListItem Text="CS File" Value=".cs"></asp:ListItem>
                    <asp:ListItem Text="Xml File" Value=".xml"></asp:ListItem>
                    <asp:ListItem Text="All" Value="%"></asp:ListItem>
                </asp:DropDownList>
                <asp:DropDownList ID="ddBuild" DataSourceID="SqlDataSourceBuild"  AutoPostBack="true"
                    DataValueField="ComparedBuild" runat="server" Width="200px" Font-Size="11px"
                    AppendDataBoundItems="true">
                </asp:DropDownList>
                <asp:DropDownList ID="ddAssembly" DataSourceID="SqlDataSourceAssembly" AutoPostBack="true"
                    DataValueField="AssemblyName" runat="server" Width="200px" Font-Size="11px" AppendDataBoundItems="true">
                    <asp:ListItem Text="All" Value="%"></asp:ListItem>
                </asp:DropDownList>
            </div>
            <asp:GridView ID="Gridview1" runat="server" AutoGenerateColumns="False" AllowSorting="True"
                DataSourceID="SqlDataSourceMain" DataKeyNames="ResultID" CssClass="mGrid" CellPadding="4"
                Font-Size="Medium" ForeColor="#333333" GridLines="None" OnRowDataBound="GridView1_RowDataBound"
                HeaderStyle-HorizontalAlign="Left" Width="100%" AllowPaging="true" PageSize="22"
                OnSelectedIndexChanged="GridView1_SelectedIndexChanged">
                
                <EmptyDataTemplate>
                    <table border="1px" width="100%">
                        <tr style=" background-color:#1C5E55; color:White; font-weight:bold">
                            <th >
                                DiffType
                            </th>
                            <th >
                                WmiClassName
                            </th>
                            <th >
                                SourceFileName
                            </th>
                            <th >
                                AssemblyName
                            </th>
                        </tr>
                        <tr>
                            <td colspan="4" align="center">
                                No data available.
                            </td>
                        </tr>
                    </table>
                </EmptyDataTemplate>
                <AlternatingRowStyle BackColor="White" />
                <Columns>
                    <asp:TemplateField ShowHeader="False">
                        <ItemTemplate>
                            <asp:HiddenField ID="resultID" runat="server" Value='<%# Eval("ResultID") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField ShowHeader="False">
                        <ItemTemplate>
                            <asp:HiddenField ID="PageName" runat="server" Value='<%# Eval("PageName") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField HeaderStyle-HorizontalAlign="Left" DataField="DiffType" HeaderText="DiffType" SortExpression="DiffType" />
                    <asp:BoundField HeaderStyle-HorizontalAlign="Left" DataField="WmiClassName" HeaderText="WmiClass Name" SortExpression="WmiClassName" />
                    <asp:BoundField HeaderStyle-HorizontalAlign="Left" DataField="SourceFileName" HeaderText="SourceFile Name" SortExpression="SourceFileName" />
                    <asp:BoundField HeaderStyle-HorizontalAlign="Left" DataField="AssemblyName" HeaderText="Assembly Name" SortExpression="AssemblyName" />
                    <asp:TemplateField ShowHeader="False">
                        <ItemTemplate>
                            <asp:Image ID="Image1" runat="server" Width="20" Height="20"  ImageUrl="~/img/services-swatchbook.png" BorderStyle="None" ToolTip="Show Images" AlternateText="Show Images" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField ShowHeader="False">
                        <ItemTemplate>
                            <img id="img2" width="20" height="20" src="img/find_magnify.png" alt="Show Class"
                                style="border-style: none" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <SelectedRowStyle ForeColor="#333333" Font-Bold="True" BackColor="#C5BBAF"></SelectedRowStyle>
                <EditRowStyle BackColor="#7C6F57" />
                <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
                <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
                <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
                <RowStyle BackColor="#E3EAEB"></RowStyle>
                <SortedAscendingCellStyle BackColor="#F8FAFA" />
                <SortedAscendingHeaderStyle BackColor="#246B61" />
                <SortedDescendingCellStyle BackColor="#D4DFE1" />
                <SortedDescendingHeaderStyle BackColor="#15524A" />
            </asp:GridView>
            <asp:SqlDataSource ID="SqlDataSourceMain" runat="server" ConnectionString="Data Source=SulphurServer14.redmond.corp.microsoft.com;Initial Catalog=SulpHur;Persist Security Info=True;Integrated Security=True"
                ProviderName="System.Data.SqlClient" SelectCommand="select * from [ProviderDiffResults] where ComparedBuild=@ComparedBuild"
                FilterExpression="DiffType like '{0}' and SourceFileName like '%{1}' and AssemblyName like '{2}'">
                <SelectParameters>
                     <asp:ControlParameter Name="ComparedBuild" ControlID="ddBuild" PropertyName="SelectedValue" />
                </SelectParameters>
                <FilterParameters>
                    <asp:ControlParameter Name="DiffType" ControlID="ddDiffType" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="ddSourceFile" PropertyName="SelectedValue" Name="filesuffix" />
                    <asp:ControlParameter Name="AssemblyName" ControlID="ddAssembly" PropertyName="SelectedValue" />
                </FilterParameters>
            </asp:SqlDataSource>
            <asp:SqlDataSource ID="SqlDataSourceDiffType" runat="server" ConnectionString="Data Source=SulphurServer14.redmond.corp.microsoft.com;Initial Catalog=SulpHur;Persist Security Info=True;Integrated Security=True"
                ProviderName="System.Data.SqlClient" SelectCommand="select DISTINCT [DiffType] from [ProviderDiffResults]">
            </asp:SqlDataSource>
            <asp:SqlDataSource ID="SqlDataSourceAssembly" runat="server" ConnectionString="Data Source=SulphurServer14.redmond.corp.microsoft.com;Initial Catalog=SulpHur;Persist Security Info=True;Integrated Security=True"
                ProviderName="System.Data.SqlClient" SelectCommand="select DISTINCT [AssemblyName] from [ProviderDiffResults] order by [AssemblyName] asc">
            </asp:SqlDataSource>
            <asp:SqlDataSource ID="SqlDataSourceBuild" runat="server" ConnectionString="Data Source=SulphurServer14.redmond.corp.microsoft.com;Initial Catalog=SulpHur;Persist Security Info=True;Integrated Security=True"
                ProviderName="System.Data.SqlClient" SelectCommand="select DISTINCT [ComparedBuild] from [ProviderDiffResults] order by [ComparedBuild] desc">
            </asp:SqlDataSource>
        </div>
    </div>
    <div id="loading">
    </div>
    <div id="DetailViewContainer">
        <table>
            <tr>
                <td id="preTD">
                    <span id="LabelPreBuild"></span>
                </td>
                <td id="postTD">
                    <span id="LabelPostBuild"></span>
                </td>
            </tr>
            <tr>
                <td id="PreScreenShot">
                </td>
                <td id="PostScreenShot">
                </td>
            </tr>
            <tr>
                <td id="PreClass" style=" background-color:White; vertical-align:top;">
                </td>
                <td id="PostClass" style=" background-color:White;vertical-align:top;">
                </td>
            </tr>
        </table>
    </div>
    </form>
</body>
</html>
