<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdminUILogException.aspx.cs"
    Inherits="SulpHurManagementSystem.AdminUILogException" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Log Exception</title>
    <link rel="stylesheet" type="text/css" href="styles/jquery-ui/ui-lightness/jquery-ui-1.8.23.css" />
    <link rel="stylesheet" type="text/css" href="styles/jquery.multiselect.css" />
    <link href="styles/StyleSearch.css" rel="stylesheet" type="text/css" />
    <script type="text/javascript" src="Scripts/jquery-1.8.2.js"></script>
    <script type="text/javascript" src="Scripts/jquery-ui-1.8.23.min.js"></script>
    <script type="text/javascript" src="Scripts/jquery.multiselect.js"></script>
    <script type="text/javascript" src="Scripts/common.js"></script>
    <script type="text/javascript">
        var basicRules;
        var basicRulesArray;
        var clickedObj;
        var arrowUp = 'Images/arrow.jpg';
        var arrowDown = 'Images/arrow1.jpg';
        var assemblyInfoDic;
        var availableBuildNo;
        var availableBuildLanguage;
        var availableOSType;
        var availableOSLanguage;
        var availableRulesDic;
        var availableAssembly;
        var clickedID;
        var pages;

        //class names
        var commonTagClassName = 'TagCommon';
        var assemblyTagClassName = 'TagAssembly';
        var fullTypeTagClassName = 'TagFullType';
        var SelectedTagClassName = 'SelectedTag';

        //jquery selector id
        var SIBasicTag = "#SC_Rule .Tag:contains('Basic')";
        var SIScreenShotTag = "#SC_Rule .Tag:contains('ScreenShot Rule')";
        var SITag = '.Tag';
        var SITagCommon = '.TagCommon';
        var SITagAssembly = '.TagAssembly';
        var SITagAssemblyAll = '.TagAssemblyAll';
        var SITagFullTypeAll = '.TagFullTypeAll';
        var SITagFullType = '.TagFullType';
        var SITagAll = '.TagAll';
        var SISelectedTag = '.SelectedTag';
        var SIBtnQuery = '#btnQuery';
        var SIBuildNoCondition = '#SC_BuildNo';
        var SICompareBuildNoCondition = '#Compare_SC_BuildNo';
        var SIBuildLanguageCondition = '#SC_BuildLanguage';
        var SICompareBuildLanguageCondition = '#Compare_SC_BuildLanguage';
        var SIOSTypeCondition = '#SC_OSType';
        var SIOSLanguageCondition = '#SC_OSLanguage';
        var SIRulesCondition = '#SC_Rule';
        var SIResultsCondition = '#SC_Result';
        var SIReviewedCondition = '#SC_Reviewed';
        var SIBuildTypeCondition = '#SC_BuildType';
        var SIAssemblyCondition = '#SC_AssemblyName';
        var SIFullTypeCondition = '#SC_FullTypeName';
        var SIResultBody = '#ResultsBody';
        var SIFullTypeListContainer = '#SC_FullTypeName .LargeListTd';
        var SIAssemblyListContainer = '#SC_AssemblyName .LargeListTd';
        var SIAssemblyAllConvinenceTag = '#SC_AssemblyName .HaveSelected .ConvinenceTag';
        var SIFullTypeAllConvinenceTag = '#SC_FullTypeName .HaveSelected .ConvinenceTag';
        var SIAssemblyConvinenceContainer = '#SC_AssemblyName .HaveSelected';
        var SIFullTypeConvinenceContainer = '#SC_FullTypeName .HaveSelected';
        var SISelectedAssemblyTags = '#SC_AssemblyName .SelectedTag';
        var SISelectedFullTypeTags = '#SC_FullTypeName .SelectedTag';
        var SISelectedBuildNoTags = "#SC_BuildNo .SelectedTag";
        var SISelectedComparedWithBuildNoTags = "#Compare_SC_BuildNo .SelectedTag";
        var SISelectedLanguageTags = "#SC_BuildLanguage .SelectedTag";
        var SISelectedComparedWithLanguageTags = "#Compare_SC_BuildLanguage .SelectedTag";
        var SISelectedOSTypeTags = '#SC_OSType .SelectedTag';
        var SISelectedOSLanguageTags = '#SC_OSLanguage .SelectedTag';
        var SISelectedRuleTags = '#SC_Rule .SelectedTag';
        var SISelectedResultTags = '#SC_Result .SelectedTag';
        var SISelectedReviewedTags = '#SC_Reviewed .SelectedTag';
        var SISelectedBuildTypeTags = '#SC_BuildType .SelectedTag';
        var SIExpandIconImage = '.Expand-Collpase-Icon > img';
        var SISearchBody = '#SearchBody';
        var SIGridMenu = '#GridMenu';
        var SIRelatedBugSubMenu = '#RelatedBugsSubMenu';
        var SISelectedRow = '.commonTr.Selected-Tr';
        var SIRowDetails = '#Row_DetailInfo';
        var SIGroupDetails = '#GroupDetails';
        var SIPromptMessage = '.Prompt-Message';

        //Selected Conditions
        var SelectedBuild;
        var SelectedBuildLanguage;
        var SelectedOSType;
        var SelectedOSLanguage;
        var SelectedResult;
        var SelectedReviewed;
        var SelectedBuildType
        var SelectedAssembly;
        var SelectedTypes;
        var alias;
        var pagetitle;
        function copyToClipboard(text) {
            if (window.clipboardData) {
                window.clipboardData.setData('text', text);
            }
        }
        function nav_targetblank(link) {
            window.open(link);
        }
        String.prototype.replaceAll  = function(s1,s2){   
            return this.replace(new RegExp(s1,"gm"),s2);   
        }
        function showDetailInfoAfterRow(row,text) {
            //detail info row position
            $(SIRowDetails).insertAfter(row);
            //ajax to get detail info
            var logtID = $(row).attr("id");
            text = text.replaceAll("\r\n", "<br />");
            $('#Row_DetailInfo').find('#LogMessage').html(text);
            //save latest dblclick row
            $(SIRowDetails).show();
        }
        function ChangePageIndex(curentPageIndex) {
            var v = parseInt(curentPageIndex);
            $('#PageIndexs').html('');
            v == 1 ? $("#prePage").hide() : $("#prePage").show();
            v == pages ? $("#nextPage").hide() : $("#nextPage").show();

            if (pages <= 7) {
                for (var i = 1; i <= pages; i++) {
                    $('#PageIndexs').append(CreatePageTag(i));
                }
            } else if (pages > 7 && v <= 4) {
                for (var i = 1; i <= 7; i++) {
                    $('#PageIndexs').append(CreatePageTag(i));
                }
                $('#PageIndexs').append("...");
            } else if (pages > 7 && v > 4 && pages - v >= 3) {
                $('#PageIndexs').append("...");
                for (var i = 1; i <= 7; i++) {
                    var c = v - 4 + i;
                    $('#PageIndexs').append(CreatePageTag(c));
                }
                $('#PageIndexs').append("...");
            } else if (pages > 7 && pages - v < 3) {
                $('#PageIndexs').append("...");
                for (var i = 7; i >= 1; i--) {
                    var c = pages - i + 1;
                    $('#PageIndexs').append(CreatePageTag(c));
                }
            }
            $('#PageIndexs .PageTag').click(function () {
                var currentPage = $(this).text();
                ChangePageIndex(currentPage);
                DoQuery(currentPage);
            });
            $("#PageIndexs .PageTag").each(function () {
                if ($(this).text() == curentPageIndex) {
                    $(this).addClass("SelectedPageIndex");
                }
            });
        }
        function EnablePageNavigation(totalCount) {
            if (pages <= 7) {
                for (var i = 1; i <= pages; i++) {
                    $('#PageIndexs').append(CreatePageTag(i));
                }
            } else {
                for (var i = 1; i <= 7; i++) {
                    $('#PageIndexs').append(CreatePageTag(i));
                }
                $('#PageIndexs').append("...");
            }
            $('#PageIndexs .PageTag').click(function () {
                var currentPage = $(this).text();
                ChangePageIndex(currentPage);
                DoQuery(currentPage);
            });
            $('#PageIndexs .PageTag:first').addClass("SelectedPageIndex");
            $('#prePage').hide();
        }

        function convert(str) {
            str = str.replace(/&/g, "&amp;");
            str = str.replace(/>/g, "&gt;");
            str = str.replace(/</g, "&lt;");
            str = str.replace(/"/g, "&quot;");
            str = str.replace(/'/g, "&#039;");
            return str;
        }
        function DoQuery(pageIndex) {
            $(SIRowDetails).appendTo('body');
            $(SIRowDetails).hide();
            $(SIResultBody).html('');
            $(SIPromptMessage).show().html('<img class="Loading-Image" src="Images/loading.gif"/>');

            SelectedBuild = GetConditions(SISelectedBuildNoTags, "Please Select build no.", "#SC_BuildNo .Tag:gt(1)");
            if (typeof SelectedBuild == 'undefined') return;

            alias = $("#alias").val();
            var exValue = $("#tException").val();
            $('#PageNavigation').hide();
            $('#PageIndexs').html('');

            $.ajax({
                beforeSend: function () {
                    $(SIBtnQuery).attr("disabled", "disabled");
                    $(SIPromptMessage).show().html('<img class="Loading-Image" src="Images/loading.gif"/>');

                },
                url: 'CaptureUIsService.asmx/QueryLogExceptions',
                contentType: "application/json;charset=utf-8",
                type: 'POST',
                data: "{buildNO:'" + SelectedBuild + "',username:'" + alias + "',exValue:'" + exValue + "'}",
                dataType: 'json',
                error: function (data) {
                    $(SIBtnQuery).removeAttr("disabled");
                    alert("Fail to query records!");
                },
                success: function (data) {
                    $(SIBtnQuery).removeAttr("disabled");
                    if (data.d.length == 0) {
                        $(SIPromptMessage).show().html('No records found according to the conditions.');
                    } else {
                        $(SIPromptMessage).hide();
                        for (var i = 0; i < data.d.length; i++) {
                            var j = i + 1;

                            var ex = convert(data.d[i].ExceptionContent);

                            $(ResultsBody).append("<tr class='commonTr' id='" + data.d[i].LogID + "' style='height:25px;'><td class='tdCss'>" + j
                            + "</td><td class='tdCss'>" + data.d[i].BuildNo
                            + "</td><td class='tdCss'>" + data.d[i].UserName
                            + "</td><td class='tdCss'>" + data.d[i].OSType
                            + "</td><td class='tdCss' title='" + ex + "'>" + ex
                            + "</td><td class='tdCss'>" + data.d[i].FTime
                            + "</td><td class='tdCss'>" + data.d[i].LTime
                            + "</td><td class='tdCss'>" + data.d[i].Count
                            + "</td></tr>");
                        }

                        $("#ResultsBody tr:odd").addClass("odd");
                        $("#ResultsBody tr:even").addClass("even");

                        $("#ResultsBody tr").mouseover(function () {
                            $(this).addClass("TrOver");
                        }).mouseout(function () {
                            $(this).removeClass("TrOver");
                        }).click(function () {
                            $(this).addClass("Selected-Tr").removeClass("TrOver").siblings().removeClass("Selected-Tr");
                        }).dblclick(function () {

                            var id = $(this).attr("id");
                            if (clickedID != id) {
                                clickedID = id;
                                var tex = $(this).children().eq(4).text();
                                showDetailInfoAfterRow($(this), tex);
                            }
                            else {
                                $("#Row_DetailInfo").toggle();
                            }
                            return false;
                        });
                    }
                }
            });
        }

        function SetDefaultAvailableOptions(arrayData, parent, HidenStartIndex) {
            $.each(arrayData, function (index, value) {
                $(parent).append(CreateTag3(value, commonTagClassName, "", ""));
            });
            //HidenStartIndex=-1 won't hidden any tag
            if (HidenStartIndex != -1) {
                HidenChildrenGT($(parent), HidenStartIndex);
            }
        }
        function CreateTag3(text, actionClass, tagTitle, value) {
            return "<span class='Tag " + actionClass + "' title='" + tagTitle + "' value='" + value + "'>" + text + "</span>";
        }
        function CreatePageTag(number) {
            return "<span class='PageTag'>" + number + "</span>";
        }
        function HidenChildrenGT(parent, index) {
            $(parent).children(":gt(" + index + ")").hide();
        }
        function BindingReadyElementActions() {
            /* Query Click Action */
            $(SIBtnQuery).click(function () {
                DoQuery(0);
            });

            $("#SetDefault").click(function () {
                SelectDefaultCondition();
            });

            $("#ClearAll").click(function () {
                $(".Tag").each(function () {
                    $(this).removeClass(SelectedTagClassName);
                });
            });
        }
        function SetConditions() {
            //Set BuildNo
            SetDefaultAvailableOptions(availableBuildNo, $(SIBuildNoCondition), 6);
        }
        function GetConditions(Tags, NullWarningText, AllTagsSelector) {
            var arrayTemp = new Array();
            $(Tags).each(function () {
                arrayTemp.push($(this).text().replace(/(\r\n|\n|\r)/gm, ""));
            });
            if (arrayTemp.length == 0) {
                alert(NullWarningText);
                return;
            }
            else if (arrayTemp[0] == "Latest") {
                arrayTemp[0] = $(Tags).next().text();
            }
            else if (arrayTemp[0] == "All") {
                arrayTemp.pop();
                $(AllTagsSelector).each(function () {
                    arrayTemp.push($(this).text());
                });
            }
            var SelectedCondition = arrayTemp.join('|');
            return SelectedCondition;
        }
        function BindingPageNavigationAction() {
            $("#btnPageOK").click(function () {
                var currentPage = $("#gotoPage").val();
                if (pages != null && currentPage > pages) {
                    alert('page number should less than:' + pages);
                    return;
                }
                ChangePageIndex(currentPage);
                DoQuery(currentPage);
            });

            $("#prePage").click(function () {
                var p = parseInt($(".SelectedPageIndex").text());
                p = p - 1;
                ChangePageIndex(p);
                DoQuery(p);
            });
            $("#nextPage").click(function () {
                var p = parseInt($(".SelectedPageIndex").text());
                p = p + 1;
                ChangePageIndex(p);
                DoQuery(p);
            });
        }
        function SetConditionActions() {
            $(SITagCommon).click(function () {
                if ($(this).hasClass(SelectedTagClassName)) {
                    $(this).removeClass(SelectedTagClassName);
                } else {
                    $(this).addClass(SelectedTagClassName).siblings(":not('.TagCommon')").removeClass(SelectedTagClassName).end();
                }
            });
            $(SITagAll).click(function () {
                if ($(this).hasClass(SelectedTagClassName)) {
                    $(this).removeClass(SelectedTagClassName);
                } else {
                    $(this).addClass(SelectedTagClassName).siblings().removeClass(SelectedTagClassName);
                }
            });
        }
        function InitialConditions() {
            SetConditions();
            SetConditionActions();
            SelectDefaultCondition();
        }
        function SelectDefaultCondition() {
            $("#SC_AssemblyName .Tag:eq(0),#SC_FullTypeName .Tag:eq(0),#SC_BuildNo > .Tag:contains('All'),#SC_BuildLanguage > .Tag:contains('All'),#SC_OSType > .Tag:eq(0),#SC_OSLanguage > .Tag:eq(0),#SC_Rule > .Tag:contains('ScreenShot Rule'),#SC_Result > .Tag:eq(0),#SC_Reviewed > .Tag:eq(0),#SC_BuildType > .Tag:eq(0)")
    .addClass(SelectedTagClassName).siblings().removeClass(SelectedTagClassName);
        }

        $(SIBtnQuery).click(function () {
            DoQuery(0);
        });
        $("#SetDefault").click(function () {
            SelectDefaultCondition();
        });

        $("#ClearAll").click(function () {
            $(".Tag").each(function () {
                $(this).removeClass(SelectedTagClassName);
            });
            RemoveAllConvinenceTags();
        });
        function BindingShowMoreAction(Icon, StartIndex) {
            $(Icon).toggle(function () {
                $(this).children('img').attr({ 'src': arrowUp }).end().children('span').text("Collapse");
                $(this).parent().prev().find('.Tag:gt(' + StartIndex + ')').show();
            }, function () {
                $(this).children('img').attr({ 'src': arrowDown }).end().children('span').text("Show More");
                $(this).parent().prev().find('.Tag:gt(' + StartIndex + ')').hide();
            });
        }
        $(function () {
            /* Load and Initial Condition options data */
            $.ajax({
                url: 'CaptureUIsService.asmx/QueryAvailableData',
                contentType: "application/json;charset=utf-8",
                type: 'POST',
                dataType: 'json',
                error: function (data) {
                    alert("Fail to get condition options!");
                },
                success: function (msg) {
                    availableBuildNo = msg.d.hash["AvailableCapturedBuilds"].split('|');
                    InitialConditions();
                    DoQuery(0);
                }
            });

            BindingPageNavigationAction();
            BindingReadyElementActions();
            BindingShowMoreAction($(".ShowMoreAction"), 6);

            /* Set Condition Table Style */
            $('#SearchBody table tr td').css({ 'border-bottom': '1px dotted grey' });
        });
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true">
    </asp:ScriptManager>
    <table style="display: none;">
        <tr id="Row_DetailInfo" style="height: auto;">
            <td colspan="11">
                <div id="LogMessage" style="width: 100%; margin: 5px 0 10px 0;">
                </div>
                <div id="Screenshot">
                </div>
            </td>
        </tr>
    </table>
    <div id="PageHeader">
        <a href="javascript:nav_targetblank('http://172.22.92.173/SulpHur/')">HOME</a> >
        <span>AdminUI Log Exceptions</span>
    </div>
    <div id="PageBody">
        <div id="SearchCondition">
            <div id="SearchTitle">
                <div class="Expand-Collpase-Icon">
                    <img src="Images/icons1.jpg" alt="Collpase" style="margin-top: -11px; margin-left: -110px;" />
                </div>
                <p>
                    &nbsp; &nbsp; Select query conditions</p>
                <a id="ClearAll" href="#">Clear All</a> <a id="SetDefault" href="#">Default</a>
            </div>
            <div id="SearchBody">
                <table id="ConditionTable">
                    <tr>
                        <td>
                            Build No:
                        </td>
                        <td id="SC_BuildNo">
                            <span class="Tag TagAll">All</span>
                        </td>
                        <td>
                            <div class="ShowMoreContainer ShowMoreAction">
                                <img src="Images/arrow1.jpg" alt="" /><span>Show More</span></div>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            User Name:
                        </td>
                        <td>
                            <input type="text" id="alias" size="18" maxlength="28" />
                        </td>
                        <td>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Exception:
                        </td>
                        <td>
                            <input type="text" id="tException" size="40" maxlength="50" />
                        </td>
                        <td>
                        </td>
                    </tr>
                </table>
                <div id="ButtonContainer">
                    <input class="Modern-Button" type="button" value="Search" id="btnQuery" name="btnQuery" />
                </div>
            </div>
        </div>
        <div id="Results">
            <div id="ResultsTableContainer">
                <table>
                    <col width="40px;" />
                    <col width="100px;" />
                    <col width="100px;" />
                    <col width="80px;" />
                    <col width="500px;" />
                    <col width="150px;" />
                    <col width="150px;" />
                    <col width="60px;" />
                    <thead>
                        <tr>
                            <th>
                                RowID
                            </th>
                            <th>
                                Build No.
                            </th>
                            <th>
                                User Name
                            </th>
                            <th>
                                OS Type
                            </th>
                            <th>
                                Exception
                            </th>
                            <th>
                                First Uploaded Time (UTC)
                            </th>
                            <th>
                                last Uploaded Time (UTC)
                            </th>
                            <th>
                                Count
                            </th>
                        </tr>
                    </thead>
                    <tbody id="ResultsBody">
                    </tbody>
                </table>
                <div class="Prompt-Message">
                    Click Search to show results
                </div>
            </div>
            <div id="PageNavigation">
                <div id="prePage">
                    <input class="icon-btn" type="button" value="Previous" />
                </div>
                <div id="PageIndexs">
                </div>
                <div id="nextPage">
                    <input class="icon-btn" type="button" value="Next" />
                </div>
                <div id="TotalPageInfo">
                </div>
                <div id="pageIndexContainer">
                    Go&nbsp;&nbsp;<input type="text" id="gotoPage" style="width: 38px; display: inline" /></div>
                <div id="btnIndexOKContainer">
                    &nbsp;<input class="icon-btn" type="button" value="ok" id="btnPageOK" /></div>
            </div>
        </div>
    </div>
    </form>
</body>
</html>
