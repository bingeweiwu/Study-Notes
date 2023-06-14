<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CapturedUIReport.aspx.cs"
    Inherits="SulpHurManagementSystem.CapturedUIReport" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta http-equiv="X-UA-Compatible" content="IE=Edge,chrome=1" />
    <title>Captured UIs Report</title>
    <link rel="stylesheet" type="text/css" href="styles/jquery-ui/ui-lightness/jquery-ui-1.8.23.css" />
    <link rel="stylesheet" type="text/css" href="styles/jquery.multiselect.css" />
    <link href="styles/StyleSearch.css" rel="stylesheet" type="text/css" />
    <script type="text/javascript" src="Scripts/jquery-1.8.2.js"></script>
    <script type="text/javascript" src="Scripts/jquery-ui-1.8.23.min.js"></script>
    <script type="text/javascript" src="Scripts/jquery.multiselect.js"></script>
    <script type="text/javascript" src="Scripts/common.js"></script>
    <script type="text/javascript">
    var serverName = 'AzureSulpHur1';
    var UIDiffResultPathBase = '\\\\scfs\\Teams\\TAO\\AdminConsole\\Retail\\Diff\\Result\\';
    var UIDiffResultPathRelated = '\\Result\\PageNav.xml';
    var basicRules;
    var basicRuleIDs;
    var basicRulesArray;
    var basicENURules = "Access Key non Duplicate Rule|Tab Order Rule|Spelling Rule";
    var clickedObj;
    var arrowUp = 'Images/arrow.jpg';
    var arrowDown = 'Images/arrow1.jpg';
    var assemblyInfoDic;
    var availableBuildNo;
    var availableBuildLanguage;
    var availableOSType;
    var availableOSLanguage;
    var availableRules;
    var availableAssembly;
    var clickedID;
    var pages;
    var currentPageNumber;
    var pageResults = new Array();
    var pageDiffentReviewStatusResultsCount = new Array();
    var diffentReviewStatusResultsCount = 0;
        var days = 14;
    //class names
    var commonTagClassName = 'TagCommon';
    var assemblyTagClassName = 'TagAssembly';
    var fullTypeTagClassName = 'TagFullType';
    var SelectedTagClassName = 'SelectedTag';

    //jquery selector id
    var SIBuildSelectChanger = '#BuildSelectChanger_span';
    var SIBuildShowMore = '#BuildNoShowMore';
    var SIBuildNoRangeSelecterLeft = '#BuildNoSelect_Start';
    var SIBuildNoRangeSelecterRight = '#BuildNoSelect_End';
    var SITag = '.Tag';
    var SITagCommon = '.TagCommon';
    var SITagAssembly = '.TagAssembly';
    var SITagAssemblyAll = '.TagAssemblyAll';
    var SITagFullTypeAll = '.TagFullTypeAll';
    var SITagFullType = '.TagFullType';
    var SITagAll = '.TagAll';
    var SISelectedTag = '.SelectedTag';
    var SIBtnQuery = '#btnQuery';
    var SIBtnLogin = '#btnLogin';
    var SIBuildNoCondition = '#SC_BuildNo';
    var SIBuildNoCondition_List = '#SC_BuildNo_List';
    var SIBuildNoCondition_Range = '#SC_BuildNo_Range';
    var SICompareBuildNoCondition = '#Compare_SC_BuildNo';
    var SIBtnShowUIDiffResult = '#btnShowUIDiffResult';
    var SIBuildLanguageCondition = '#SC_BuildLanguage';
    var SISelectedTestCoverageBuildLanguageTags = '#TestCoverage_BuildLanguage .SelectedTag';
    var SICompareBuildLanguageCondition = '#Compare_SC_BuildLanguage';
    var SIOSTypeCondition = '#SC_OSType';
    var SIOSLanguageCondition = '#SC_OSLanguage';
    var SIRulesCondition = '#SC_Rule';
    var SIUIDiffRulesCondition = '#UIDiff_Rule';
    var SITestCoverageRulesCondition = '#TestCoverage_Rule';
    var SITestCoverageBuildLanguagesCondition = '#TestCoverage_BuildLanguage';
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
    var SISelectedBuildNoCondition_ListTags = "#SC_BuildNo_List .SelectedTag";
    var SISelectedComparedWithBuildNoTags = "#Compare_SC_BuildNo .SelectedTag";
    var SISelectedLanguageTags = "#SC_BuildLanguage .SelectedTag";
    var SISelectedComparedWithLanguageTags = "#Compare_SC_BuildLanguage .SelectedTag";
    var SISelectedOSTypeTags = '#SC_OSType .SelectedTag';
    var SISelectedOSLanguageTags = '#SC_OSLanguage .SelectedTag';
    var SISelectedRuleTags = '#SC_Rule .SelectedTag';
    var SISelectedUIDiffRuleTags = '#UIDiff_Rule .SelectedTag';
    var SISelectedTestCoverageRuleTags = '#TestCoverage_Rule .SelectedTag';
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
    // v-danpgu: expandall
    var SIExpandAll = '#btnExpandAll';
    var SICollapseAll = '#btnCollapseAll';
    var SIAllRow = '.commonTr';
    var SIDropDownSortBy = '#dropdownSortBy';

    var changeRangeSelect = 'Change to range select';
    var changeListSelect = 'Change to list select';
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
    var SelectedSortBy;
    var resultid;
    var reviewComments;

    var SILnkCurrentUserAlias = '#lnkCurrentUserAlias';
    var SILnkCurrentUserName = '#lnkCurrentUserName';
    var SICPUserName = '#cpUserName';

    //V-DANPGU FILEBUG
    var PSUserAlias;
    var PSUserName;
    var IsFileBug = false;
    var GlobalFilePath;
    var GlobalResultIDList;
    var GlobalBugId;

    //v-danpgu fileMultiBug
    var chkboxForFileBughtml = "<input type=\"checkbox\" class='chkboxForFileBug' />";
    var fileMultiBugFlag = 0;
    var ResultsList = [];
    var MultiPdfList = [];
    var ResultIDList = "";
    //var FilePath = '\\scfs\Users\INTL\SulphurBugFiles';
    var PDFFolder = "\\\\scfs\\Users\\INTL\\SulphurBugFiles";

    //The datetime of clicking 'Search'
    var searchDateTime = getNowFormatDate();

    function getNowFormatDate() {
        var date = new Date();
        var seperator1 = "-";
        var seperator2 = ":";
        var month = date.getMonth() + 1;
        var strDate = date.getDate();
        if (month >= 1 && month <= 9) {
            month = "0" + month;
        }
        if (strDate >= 0 && strDate <= 9) {
            strDate = "0" + strDate;
        }
        var currentdate = date.getFullYear() + seperator1 + month + seperator1 + strDate
            + " " + date.getHours() + seperator2 + date.getMinutes()
            + seperator2 + date.getSeconds();
        return currentdate;
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
    function AddConvinenceTag(parent, text) {
        $(parent).show();
        var tag = "<span class='ConvinenceTag'>" + text + "</span>";
        $(parent).append(tag);
    }
    function RemoveConvinenceTag(convinenceContainer, allconvinences, text) {
        $(convinenceContainer).find(".ConvinenceTag:contains('" + text + "')").remove();
        if ($(allconvinences).length == 0) {
            $(convinenceContainer).hide();
        }
    }
    function RemoveConvinenceTags(AllConvinenceTags, ConvinenceTagContainer) {
        $(AllConvinenceTags).each(function () {
            $(this).remove();
        });
        $(ConvinenceTagContainer).hide();
    }
    function RemoveAllConvinenceTags() {
        RemoveConvinenceTags(SIAssemblyAllConvinenceTag, SIAssemblyConvinenceContainer);
        RemoveConvinenceTags(SIFullTypeAllConvinenceTag, SIFullTypeConvinenceContainer);
    }

    function copyBugID(menuItem) {
        copyToClipboard($(menuItem).attr('bugID'));
    }

    function validatePathInput(path) {
        //is empty
        if (path == '') {
            return 'path cannot be empty.';
        }
        var reg = /^(\\)((\\)[^///*/?/|/:"<>]{1,255})+$/;
        //validate
        var isvalidpath = reg.test(path);
        if (isvalidpath) {
            return '';
        }
        return path + " is invalid path.\nPath must be start as share path like \\\\."
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
    function copyToClipboard(copiedColumnName, text) {
        var aux = document.createElement("input");
        aux.setAttribute("value", text);
        document.body.appendChild(aux);
        aux.select();
        document.execCommand("copy");
        document.body.removeChild(aux);
      //  alert(copiedColumnName + " copied!");
        var popUpString = text + " copied success";
        var w = window.open('', '', 'width=260,height=90')
        w.document.write(popUpString)
        w.focus()
        setTimeout(function () { w.close(); }, 800)
    }
    function nav_targetblank(link) {
        window.open(link);
    }
    function SelectDefaultCondition() {
        $('#SC_BuildNo_List > .Tag:eq(0)').addClass(SelectedTagClassName).siblings().removeClass(SelectedTagClassName);
        $('#SC_BuildLanguage > .Tag:eq(0)').addClass(SelectedTagClassName).siblings().removeClass(SelectedTagClassName);
        $('#SC_BuildType > .Tag:eq(0)').addClass(SelectedTagClassName).siblings().removeClass(SelectedTagClassName);
        $('#SC_OSType > .Tag:eq(0)').addClass(SelectedTagClassName).siblings().removeClass(SelectedTagClassName);
        $('#SC_OSLanguage > .Tag:eq(0)').addClass(SelectedTagClassName).siblings().removeClass(SelectedTagClassName);
        $('#SC_Rule > .Tag:eq(0)').addClass(SelectedTagClassName).siblings().removeClass(SelectedTagClassName);
        $('#SC_Result > .Tag:eq(1)').addClass(SelectedTagClassName).siblings().removeClass(SelectedTagClassName);
        $('#SC_AssemblyName .Tag:eq(0)').addClass(SelectedTagClassName).siblings().removeClass(SelectedTagClassName);
        $('#SC_FullTypeName .Tag:eq(0)').addClass(SelectedTagClassName).siblings().removeClass(SelectedTagClassName);
        $('#SC_Reviewed > .Tag:eq(1)').addClass(SelectedTagClassName).siblings().removeClass(SelectedTagClassName);

        RemoveAllConvinenceTags();
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
            DoQuery(currentPage, 1);
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
            DoQuery(currentPage, 1);
        });
        $('#PageIndexs .PageTag:first').addClass("SelectedPageIndex");
        $('#prePage').hide();
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

    function GetSelectedRules(Tags) {
        var arrayRule = new Array();
        var SelectedRules = '';
        $(Tags).each(function () {
            arrayRule.push($(this).attr('value'));
        });
        if (arrayRule.length == 0) {
            return;
        }
        else if (arrayRule[0] == "All") {
            SelectedRules = 'All';
        }
        else if (arrayRule[0] == "Basic") {
            SelectedRules = basicRuleIDs;
        }
        else {
            SelectedRules = arrayRule.join(",");
        }
        return SelectedRules;
    }
    function QueryClear() {
        $('#GroupDetailsContainer').appendTo('body');
        $(SIRowDetails).appendTo('body');
        $(SIRowDetails).hide();
        $(SIGroupDetails).html('');
        $(SIGroupDetails).hide();
        $(SIResultBody).html('');
        $(SIPromptMessage).show().html('<img class="Loading-Image" src="Images/loading.gif"/>');
    }

    function ExpandGroupItem(row) {
        var contentid = $(row).attr('title');
        $(row).addClass("Selected-Tr").removeClass("TrOver").siblings().removeClass("Selected-Tr");
        $(SIRowDetails).appendTo('body');
        $(SIRowDetails).hide();
        pageIndex = -1;
        if (clickedObj != contentid) {
            clickedObj = contentid;

            $.ajax({
                beforeSend: function () {
                    $(SIBtnQuery).attr("disabled", "disabled");
                    $('#GroupDetailsContainer').insertAfter(row);
                    $("#GroupDetails").html('<img class="Loading-Image" src="Images/loading.gif"/>');
                },
                url: 'CaptureUIsService.asmx/BindingTable',
                contentType: "application/json;charset=utf-8",
                type: 'POST',
                data: "{buildNO:'" + SelectedBuild + "',buildLan:'" + SelectedBuildLanguage + "',osType:'"
                    + SelectedOSType + "',osLanguage:'" + SelectedOSLanguage + "',ruleid:'"
                    + SelectedRules + "',resultTypes:'" + SelectedResult + "',assemblyName:'" + SelectedAssembly + "',typeID:'"
                    + SelectedTypes + "',pageIndex:'" + pageIndex + "',username:'" + alias + "',pagetitle:'" + contentid + "',ReviewedType:'"
                    + SelectedReviewed + "',buildtype:'" + SelectedBuildType + "'}",
                dataType: 'json',
                error: function (data) {
                    $(SIBtnQuery).removeAttr("disabled");
                    alert("Fail to query group details!");
                },
                success: function (data) {
                    $(SIGroupDetails).html('');
                    $(SIBtnQuery).removeAttr("disabled");
                    if (data.d.length == 0) {
                        $(SIPromptMessage).show().html('No records found according to the conditions.');
                    } else {
                        /*
                        if (firstLoad == 1) {
                        var totalCount = parseInt(data.d[0].UIName);
                        if (totalCount > 100) { 
                        }
                        }*/
                        $("#divLoading").hide();
                        $(SIPromptMessage).hide();
                        for (var i = 0; i < data.d.length; i++) {
                            var j = i + 1;
                            if (data.d[i].ReviewFlag == 1) { var checkboxhtml = "<input type=\"checkbox\" class='reviewchk' checked=true />" }
                            else { var checkboxhtml = "<input type=\"checkbox\" class='reviewchk' />" }
                            if (data.d[i].ReviewFlag == 1 && data.d[i].ReviewLog != "" && data.d[i].ReviewLog != null) { var havereviewlog = "Yes" }
                            else { var havereviewlog = "No" }

                            $(ResultsBody).append("<tr class='commonTr' id='" + data.d[i].ResultID + "' style='height:25px;'><td class='tdCss'>" + j
                                + "</td><td class='tdCss'>" + data.d[i].BuildNo
                                + "</td><td class='tdCss'>" + data.d[i].Language
                                + "</td><td class='tdCss'>" + data.d[i].RuleName
                                + "</td><td class='tdCss'>" + data.d[i].ResultType
                                + "</td><td class='tdCss' title='" + data.d[i].UIName + "'>" + data.d[i].UIName
                                + "</td><td class='tdCss'>" + data.d[i].UserName
                                + "</td><td class='tdCss'>" + data.d[i].OSType
                                + "</td><td class='tdCss'>" + data.d[i].DateUploadedStr
                                + "</td><td class='tdCss'>" + data.d[i].CreateDateStr
                                + "</td><td class='tdCss'>" + checkboxhtml
                                + "</td><td class='tdCss'>" + havereviewlog
                                + "</td><td class='tdCss'>" + "<input type=\"checkbox\" class='chkboxForFileBug' value = '" + data.d[i].ResultID + "' />"
                                + "</td></tr>");
                        }

                        $("#GroupDetails .commonTr:odd").addClass("odd");
                        $("#GroupDetails .commonTr:even").addClass("even");
                        $(SIGroupDetails).show();

                        $("#GroupDetails .commonTr").mouseover(function () {
                            $(this).addClass("TrOver");
                        }).mouseout(function () {
                            $(this).removeClass("TrOver");
                        }).click(function () {
                            $(this).addClass("Selected-Tr").removeClass("TrOver").siblings().removeClass("Selected-Tr");
                        }).dblclick(function () {
                            var id = $(this).attr("id");
                            if ($("#Row_DetailInfo_" + id).length > 0) {
                                $("#Row_DetailInfo_" + id).toggle();
                            } else {
                                if (clickedID != id) {
                                    clickedID = id;
                                    showDetailInfoAfterRow($(this));
                                }
                                else {
                                    $("#Row_DetailInfo_" + id).toggle();
                                    //v-danpgu: fix bug 469921, cannot expand first record after double click
                                    showDetailInfoAfterRow($(this));
                                }
                            }
                            return false;
                        }).contextmenu(function (e) {
                            $(this).addClass("Selected-Tr").removeClass("TrOver").siblings().removeClass("Selected-Tr");

                            $(SIGridMenu).css({
                                top: e.pageY + 'px',
                                left: e.pageX + 'px'
                            }).slideDown('fast');
                            return false;
                        });

                        $(".reviewchk").change(function () {
                            if ($(SILnkCurrentUserAlias).text() == "Guest" && this.checked) {
                                this.checked = false;
                                alert("Please Log in your work machine.");
                                //alert("Please Log in with your alias, and it will be the user who mark reviewed.");
                                //$(SIBtnLogin).click();
                            }
                            else {
                                var isreviewchecked = this.checked;
                                var selectrid = -1;
                                if (isreviewchecked) {
                                    selectrid = $(this).parent().parent().attr("id"); //$(SISelectedRow).attr("id");
                                    UpdateReviewedByResultID(selectrid, 1, "Reviewed by " + $(SILnkCurrentUserAlias).text());
                                }
                                else {
                                    selectrid = $(this).parent().parent().attr("id");
                                    UpdateReviewedByResultID(selectrid, 0, "");
                                }
                            }

                        });

                    }
                }
            });
        }
        else {
            $(SIGroupDetails).toggle();
        }
        return false;
    }

    //Cavalry: Added function for get url param. For Bug 461042 improve uidiff with sulphur
    function getUrlParam(name) {
        var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)");
        var r = window.location.search.substr(1).match(reg);
        if (r != null) return unescape(r[2]);
        return null;
    }

    //flag - 0; from BtnQuery
    //flag - 1; from page change
    function DoQuery(pageIndex, flag) {
        $("#ckbReviewBugs").attr("checked", false);
        $("#ckbFileMultiBugs").attr("checked", false);
        GlobalResultIDList = "";
        //v-danpgu:orderby bug464598 
        var e = document.getElementById("dropdownSortBy");
        SelectedSortBy = e.options[e.selectedIndex].text;
        if (getUrlParam('showlatest') != null && getUrlParam('resulttype') == null) {
            $("#showLatest").attr("checked", true);
            if (getUrlParam('showlatest') == "uploadtime") SelectedSortBy = "Upload Time";
            if (getUrlParam('showlatest') == "buildno") SelectedSortBy = "Build No.";
            if (getUrlParam('showlatest') == "pagetitle") SelectedSortBy = "Page Title";
        }
        //Cavalry: Bug 458599 Search All for BuildNo in the captured UI report doesn't work
        //SelectedBuild = GetConditions(SISelectedBuildNoTags, "Please Select build no.", "#SC_BuildNo .Tag:gt(1)");
        //selected Build No
        var buildNoSelectChanger = $(SIBuildSelectChanger)[0].innerText;
        if (getUrlParam('build') != null) {
            SelectedBuild = "'" + getUrlParam('build') + "'";
            //Added this code to use for hotfix in build, but build No. format is changed.
            //if (getUrlParam('build').length == 4) {
            //    if (getUrlParam('maxhotfix') != null) {
            //        SelectedBuild = "";
            //        for (var i = 0; i <= getUrlParam('maxhotfix') ; i++) {
            //            SelectedBuild = SelectedBuild + "5.0." + getUrlParam('build') + ".100" + i + "|";
            //        }
            //    }
            //    else {
            //        SelectedBuild = "5.0." + getUrlParam('build') + ".1000";
            //    }
            //}
        }
        else {
            if (buildNoSelectChanger == changeRangeSelect) {
                SelectedBuild = GetRangeList(SISelectedBuildNoCondition_ListTags);
            } else if (buildNoSelectChanger == changeListSelect) {
                SelectedBuild = "'" + $(SIBuildNoRangeSelecterLeft)[0].value + "' AND '" + $(SIBuildNoRangeSelecterRight)[0].value + "'";
            }
        }
        if (typeof SelectedBuild == 'undefined') {
            alert('Please select build no.');
            return;
        } else if (buildNoSelectChanger == changeListSelect && $(SIBuildNoRangeSelecterLeft)[0].value > $(SIBuildNoRangeSelecterRight)[0].value) {
            alert('If build No. range select, please make sure left build No. is less than or equal right one.');
            return;
        }
        //selected Build Language
        if (getUrlParam('buildlanguage') != null) {
            SelectedBuildLanguage = "'" + getUrlParam('buildlanguage') + "'";
        }
        else {
            SelectedBuildLanguage = GetRangeList(SISelectedLanguageTags);
        }
        if (typeof SelectedBuildLanguage == 'undefined') {
            alert('Please select build language.');
            return;
        }
        //selected Build Type
        SelectedBuildType = GetRangeList(SISelectedBuildTypeTags, 2);
        if (typeof SelectedBuildType == 'undefined') {
            alert('Please select build type.');
            return;
        }
        //selected OS Type
        SelectedOSType = GetRangeList(SISelectedOSTypeTags);
        if (typeof SelectedOSType == 'undefined') {
            alert('Please select OS type.');
            return;
        }
        //selected OS Language
        SelectedOSLanguage = GetRangeList(SISelectedOSLanguageTags);
        if (typeof SelectedOSLanguage == 'undefined') {
            alert('Please select OS language.');
            return;
        }
        //selected Rule
        if (getUrlParam('rule') != null) {
            $.each(availableRules, function (key, value) {
                if (value['RuleName'] == getUrlParam('rule'))
                    SelectedRules = key;
            });
        }
        else {
            SelectedRules = GetSelectedRules(SISelectedRuleTags);
        }
        if (typeof SelectedRules == 'undefined') {
            alert('Please select rule.');
            return;
        }
        //selected Result
        SelectedResult = GetRangeList(SISelectedResultTags, 4);
        if (typeof SelectedResult == 'undefined') {
            alert('Please select result.');
            return;
        }
        if (getUrlParam('resulttype') != null) {
            SelectedResult = "'" + getUrlParam('resulttype') + "'";
        }
        //selected Assembly Name
        if (getUrlParam('assembly') != null) {
            SelectedAssembly = "'" + getUrlParam('assembly') + "'";
        }
        else {
            SelectedAssembly = GetRangeList(SISelectedAssemblyTags);
        }
        if (typeof SelectedAssembly == 'undefined') {
            alert('Please select assembly name.');
            return;
        }
        //selected Full Type Name
        if (getUrlParam('fulltypename') != null) {
            $.each(assemblyInfoDic, function (key, value) {
                if (key.split(',')[1] == getUrlParam('fulltypename')) SelectedTypes = key.split(',')[0];
            });
        }
        else {
            var arrayTemp = new Array();
            $(SISelectedFullTypeTags).each(function () {
                arrayTemp.push($(this).attr('value'));
            });
            if (arrayTemp.length == 0) {
                alert("Please select type");
                return;
            }
            else if (arrayTemp[0] == "All") {
                SelectedTypes = "All";
            } else {
                SelectedTypes = arrayTemp.join(",");
            }
        }
        if (typeof SelectedTypes == 'undefined') {
            alert('Please select full type name.');
            return;
        }
        //selected Review Status
        //-1 == All; 0 == UnReviewed; 1 == Reviewed; 'undefined' means none is selected
        var arrayReviewedTemp = new Array();
        $(SISelectedReviewedTags).each(function () {
            arrayReviewedTemp.push($("#SC_Reviewed .Tag").index($(this)) - 1);
        });
        if (arrayReviewedTemp.length == 2) {
            SelectedReviewed = '-1';
        } else {
            SelectedReviewed = arrayReviewedTemp[0];
        }
        if (typeof SelectedReviewed == 'undefined') {
            alert('Please select review status.');
            return;
        }

        //User Name, Page Title, ResultID
        alias = $("#alias").val().replace(/\\/g, "\\\\");
        pagetitle = $("#pageTitle").val();

        //selected ResultID
        if (getUrlParam('ResultID') != null) {
            resultid = getUrlParam('ResultID');
        } else {
            resultid = $("#resultid").val();
        }

        reviewComments = $("#reviewComments").val();
        QueryClear();

        //if ($("#groupbyname").is(":checked")) {
        //$.ajax({
        //    beforeSend: function () {
        //        $(SIBtnQuery).attr("disabled", "disabled");
        //        $('#PageNavigation').hide();
        //    },
        //    url: 'CaptureUIsService.asmx/GetGroupResult',
        //    contentType: "application/json;charset=utf-8",
        //    type: 'POST',
        //    data: "{buildNO:\"" + SelectedBuild + "\",buildLan:\"" + SelectedBuildLanguage + "\",osType:\"" + SelectedOSType + "\",osLanguage:\""
        //    + SelectedOSLanguage + "\",ruleid:\"" + SelectedRules + "\",resultTypes:\"" + SelectedResult + "\",assemblyName:\"" + SelectedAssembly
        //    + "\",typeID:\"" + SelectedTypes + "\",username:\"" + alias + "\",pagetitle:\"" + pagetitle + "\",ReviewedType:\"" + SelectedReviewed
        //    + "\",buildtype:\"" + SelectedBuildType + "\" }",
        //    dataType: 'json',
        //    error: function (data) {
        //        $(SIBtnQuery).removeAttr("disabled");
        //        alert("Fail to query group!");
        //    },
        //    success: function (data) {
        //        $(SIBtnQuery).removeAttr("disabled");
        //        if (data.d.length == 0) {
        //            $(SIPromptMessage).show().html('No records found according to the conditions.');
        //        } else {
        //            $("#divLoading").hide();
        //            $(SIPromptMessage).hide();
        //            for (var i = 0; i < data.d.length; i++) {
        //                var uiname = data.d[i].UIName;
        //                var contentid = data.d[i].ContentID;
        //                $(ResultsBody).append("<tr class='grouptr' title='" + contentid + "'>"
        //                    + "<td class='tdCss' colspan='1'><img src='Images/arrowright.png' style='width:20px;height:20px;'>"
        //                    + "</td><td class='tdCss' colspan='5'>" + uiname
        //                    + "</td><td class='tdCss' colspan='5'>" + data.d[i].Count
        //                    + "</td></tr>");
        //            }

        //            $("#ResultsBody .grouptr img").click(function () {
        //                var row = $(this).parent().parent();
        //                ExpandGroupItem(row);
        //            });

        //            $("#ResultsBody .grouptr").mouseover(function () {
        //                $(this).addClass("TrOver");
        //            }).mouseout(function () {
        //                $(this).removeClass("TrOver");
        //            }).click(function () {
        //                $(this).addClass("Selected-Tr").removeClass("TrOver").siblings().removeClass("Selected-Tr");
        //            }).dblclick(function () {
        //                ExpandGroupItem($(this));
        //            });
        //        }
        //    }
        //});
        //}

        var showLatest = $("#showLatest").is(":checked");
        var lastDays = $("#selectDays").val();
        //flag=0, means click the search button 'BtnQuery'
        if (flag == 0) {
            if (pageIndex == 0) {
                pageIndex = 1;
                currentPageNumber = 1;
                pageResults = new Array();
                pageDiffentReviewStatusResultsCount = new Array();

                $('#PageNavigation').hide();
                $('#PageIndexs').html('');
                $.ajax({
                    beforeSend: function () {
                    },
                    url: 'CaptureUIsService.asmx/GetTotalRecords',
                    contentType: "application/json;charset=utf-8",
                    type: 'POST',
                    data: "{buildNO:\"" + SelectedBuild + "\",buildLan:\"" + SelectedBuildLanguage + "\",osType:\"" + SelectedOSType + "\",osLanguage:\""
                        + SelectedOSLanguage + "\",ruleid:\"" + SelectedRules + "\",resultTypes:\"" + SelectedResult + "\",assemblyName:\"" + SelectedAssembly
                        + "\",typeID:\"" + SelectedTypes + "\",username:\"" + alias + "\",pagetitle:\"" + pagetitle + "\",reviewComments:\"" + reviewComments
                        + "\",ReviewedType:\"" + SelectedReviewed + "\",buildtype:\"" + SelectedBuildType + "\",resultid:\"" + resultid + "\",searchDateTime:\""
                        //+ searchDateTime + "\",getLatest:\"" + showLatest + "\"}",
                        + searchDateTime + "\",getLatest:\"" + showLatest + "\",lastDays:\"" + lastDays + "\"}",
                    dataType: 'json',
                    error: function (data) {
                        alert("Fail to get records count!" + data);
                        //alert("Fail to get records count!");
                    },
                    success: function (data) {
                        if (data.d > 20) {
                            pages = Math.ceil(data.d / 20);
                            EnablePageNavigation(data.d);
                            $('#TotalPageInfo').html("Total " + pages + " Pages");
                            $('#PageNavigation').show();
                        } else {
                            $('#PageNavigation').hide();
                        }
                    }
                });
            }
        }

        diffentReviewStatusResultsCount = 0;
        if (SelectedReviewed == 0 || SelectedReviewed == 1) {
            $.each(pageDiffentReviewStatusResultsCount, function (key, value) {
                if (key < pageIndex - 1 && typeof value != 'undefined') {
                    diffentReviewStatusResultsCount += value;
                }
            });
        }

        $.ajax({
            beforeSend: function () {
                $(SIBtnQuery).attr("disabled", "disabled");
                $(SIPromptMessage).show().html('<img class="Loading-Image" src="Images/loading.gif"/>');
            },
            url: 'CaptureUIsService.asmx/BindingTable',
            contentType: "application/json;charset=utf-8",
            type: 'POST',
            data: "{buildNO:\"" + SelectedBuild + "\",buildLan:\"" + SelectedBuildLanguage + "\",osType:\"" + SelectedOSType + "\",osLanguage:\""
                + SelectedOSLanguage + "\",ruleid:\"" + SelectedRules + "\",resultTypes:\"" + SelectedResult + "\",assemblyName:\"" + SelectedAssembly
                + "\",typeID:\"" + SelectedTypes + "\",pageIndex:\"" + pageIndex + "\",username:\"" + alias + "\",pagetitle:\"" + pagetitle
                + "\",reviewComments:\"" + reviewComments + "\",ReviewedType:\"" + SelectedReviewed + "\",buildtype:\"" + SelectedBuildType
                + "\",sortBy:\"" + SelectedSortBy + "\",resultid:\"" + resultid + "\",searchDateTime:\"" + searchDateTime
                + "\",currentPageResults:\"" + pageResults[pageIndex] + "\",diffentReviewStatusResultsCount:\"" + diffentReviewStatusResultsCount
               // + "\",getLatest:\"" + showLatest + "\"}",
                + "\",getLatest:\"" + showLatest + "\",lastDays:\"" + lastDays + "\"}",            
            dataType: 'json',
            error: function (data) {
                $(SIBtnQuery).removeAttr("disabled");
                alert("Fail to query records!");
            },
            success: function (data) {
                $(SIBtnQuery).removeAttr("disabled");
                $("#divLoading").hide();
                if (data.d.length == 0) {
                    $(SIPromptMessage).show().html('No records found according to the conditions.');
                } else {
                    $(SIPromptMessage).hide();
                    var unreviewedCount = 0;
                    var reviewedCount = 0;
                    for (var i = 0; i < data.d.length; i++) {
                        GlobalResultIDList = GlobalResultIDList + data.d[i].ResultID + ",";
                        var j = i + 1;
                        if (data.d[i].ReviewFlag == 1) { var checkboxhtml = "<input type=\"checkbox\" class='reviewchk' id='CheckedStatus' checked=true />" }
                        else { var checkboxhtml = "<input type=\"checkbox\" class='reviewchk' id='CheckedStatus' />" }
                        if (data.d[i].ReviewFlag == 1 && data.d[i].ReviewLog != "" && data.d[i].ReviewLog != null) { var havereviewlog = "Yes" }
                        else { var havereviewlog = "No" }

                        $(ResultsBody).append("<tr class='commonTr' id='" + data.d[i].ResultID + "' style='height:25px;'><td class='tdCss'>" + j
                            + "</td><td class='tdCss'>" + data.d[i].BuildNo
                            + "</td><td class='tdCss'>" + data.d[i].Language
                            + "</td><td class='tdCss'>" + data.d[i].RuleName
                            + "</td><td class='tdCss'>" + data.d[i].ResultType
                            + "</td><td class='tdCss' title='" + data.d[i].UIName + "'>" + data.d[i].UIName
                            + "</td><td class='tdCss'>" + data.d[i].UserName
                            + "</td><td class='tdCss'>" + data.d[i].OSType
                            + "</td><td class='tdCss'>" + data.d[i].DateUploadedStr
                            + "</td><td class='tdCss'>" + data.d[i].CreateDateStr
                            + "</td><td class='tdCss'>" + checkboxhtml
                            + "</td><td class='tdCss' id='HaveComments'>" + havereviewlog
                            + "</td><td class='tdCss'>" + "<input type=\"checkbox\" class='chkboxForFileBug' value = '" + data.d[i].ResultID + "' />"
                            + "</td></tr>");

                        if (data.d[i].ReviewFlag == 0) {
                            unreviewedCount++;
                        }
                        else {
                            reviewedCount++;
                        }
                    }
                    if (pageResults[pageIndex] == null) {
                        pageResults[pageIndex] = GlobalResultIDList.substring(0, GlobalResultIDList.length - 1);
                    }
                    if (pageDiffentReviewStatusResultsCount[pageIndex - 1] == null) {
                        if (SelectedReviewed == 0) {
                            pageDiffentReviewStatusResultsCount[pageIndex - 1] = reviewedCount;
                        } else if (SelectedReviewed == 1) {
                            pageDiffentReviewStatusResultsCount[pageIndex - 1] = unreviewedCount;
                        }
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
                        if ($("#Row_DetailInfo_" + id).length > 0) {
                            $("#Row_DetailInfo_" + id).toggle();
                        } else {
                            if (clickedID != id) {
                                clickedID = id;
                                showDetailInfoAfterRow($(this));
                            }
                            else {
                                $("#Row_DetailInfo_" + id).toggle();
                                //v-danpgu: fix bug 469921, cannot expand first record after double click
                                showDetailInfoAfterRow($(this));
                            }
                        }
                        return false;
                    }).contextmenu(function (e) {
                        $(this).addClass("Selected-Tr").removeClass("TrOver").siblings().removeClass("Selected-Tr");
                        $(document.body).css({ "overflow-y": "hidden" });
                        //$("#GridMenu").css({
                        //    top: e.pageY + 'px',
                        //    left: e.pageX + 'px'
                        //}).slideDown("fast");
                        //size of GridMenu
                        var gridX = $("#GridMenu").width();
                        var gridY = $("#GridMenu").height();
                        //get the size of current window
                        var mouseX = e.clientX;
                        var mouseY = e.clientY;
                        //the size of current window
                        var screenHeight = window.innerHeight;
                        var screenWidth = window.innerWidth;
                        //The GridMenu's position if you right click in the fourth quadrant
                        var forthX = mouseX - gridX + "px";
                        var forthY = mouseY - gridY + "px";
                        //The GridMenu's position if you right click in the third quadrant
                        var thirdX = mouseX + "px";
                        var thirdY = mouseY - gridY + "px";
                        //The GridMenu's position if you right click in the first quadrant
                        var firX = mouseX - gridX + "px";
                        var firY = mouseY + "px";
                        if (mouseX > screenWidth / 2 && mouseY < innerHeight / 2) {
                            //right click in the first quadrant
                            $("#GridMenu").css({
                                "position": "fixed",
                                "left": firX,
                                "top": firY,
                                "z-index": "99",
                                "hasFocus": "true"
                            }).slideDown("fast");
                        } else if (mouseX < screenWidth / 2 && mouseY < innerHeight / 2) {
                            //right click in the Second quadrant
                            $("#GridMenu").css({
                                "position": "fixed",
                                "left": mouseX,
                                "top": mouseY,
                                "z-index": "99"
                            }).slideDown("fast");
                        } else if (mouseX < screenWidth / 2 && mouseY > innerHeight / 2) {
                            //right click in the third quadrant
                            $("#GridMenu").css({
                                "position": "fixed",
                                "left": thirdX,
                                "top": thirdY,
                                "z-index": "99"
                            }).slideDown("fast");
                        } else if (mouseX > screenWidth / 2 && mouseY > innerHeight / 2) {
                            //right click in the fourth quadrant

                            $("#GridMenu").css({
                                //"overflow":"visible",

                                "position": "fixed",
                                "left": forthX,
                                "top": forthY,
                                "z-index": "99"
                            }).slideDown("fast");
                        }
                        if ($("#GridMenu").is(":hidden")) {
                            $(document.body).css({ "overflow-y": "auto" });
                        }
                        //previous GridMenu shows code
                        //$("#GridMenu").css({
                        //    top: e.pageY + 'px',
                        //    left: e.pageX + 'px'
                        //}).slideDown("fast");
                        return false;
                    });
                    $("body").click(function () {
                        $(document.body).css({ "overflow-y": "auto" });
                    });

                    $(".reviewchk").change(function () {
                        if ($(SILnkCurrentUserAlias).text() == "Guest" && this.checked) {
                            this.checked = false;
                            alert("Please Log in your work machine.");
                            //alert("Please Log in with your alias, and it will be the user who mark reviewed.");
                            //$(SIBtnLogin).click();
                        }
                        else {
                            var isreviewchecked = this.checked;
                            var selectrid = -1;
                            if (isreviewchecked) {
                                selectrid = $(this).parent().parent().attr("id"); //$(SISelectedRow).attr("id");
                                UpdateReviewedByResultID(selectrid, 1, "Reviewed by " + $(SILnkCurrentUserAlias).text());
                            }
                            else {
                                selectrid = $(this).parent().parent().attr("id");
                                UpdateReviewedByResultID(selectrid, 0, "");
                            }

                        }

                    });
                }
            }
        });
    }

    //V-DANPGU FILEBUG
    function UpdateReviewedByResultID(resultID, ischeckedflag, reviewlog) {
        $.ajax({
            beforeSend: function () {
            },
            url: 'CaptureUIsService.asmx/UpdateReviewedByResultID',
            contentType: "application/json;charset=utf-8",
            type: 'POST',
            data: "{resultID:'" + resultID + "',reviewflag:'" + ischeckedflag + "',reviewlog:'" + reviewlog + "'}",
            dataType: 'json',
            error: function (data) {
                alert("Fail to update the following data: " + resultID + "\nPlease retry to mark review for these results.");
            },
            success: function () {
                var resultIDarray = resultID.split(',');
                for (var i = 0; i < resultIDarray.length; i++) {
                    if (ischeckedflag == 1) {
                        var parent = $('#ResultsBody').find('#' + resultIDarray[i]).find('#CheckedStatus');
                        parent[0].checked = true;
                        if (reviewlog != "") {
                            $('#ResultsBody').find('#' + resultIDarray[i]).find('#HaveComments').text("Yes");
                            $('#Row_DetailInfo_' + resultIDarray[i]).find('#ReviewLog').text("Reviewed Comments:" + reviewlog);
                        }
                        else {
                            $('#ResultsBody').find('#' + resultIDarray[i]).find('#HaveComments').text("No");
                            $('#Row_DetailInfo_' + resultIDarray[i]).find('#ReviewLog').text("No reviewed comments.");
                        }
                        if (SelectedReviewed == 0) {
                            if (resultIDarray.length >= 20) {
                                pageDiffentReviewStatusResultsCount[currentPageNumber - 1] = 20;
                            } else {
                                pageDiffentReviewStatusResultsCount[currentPageNumber - 1] += resultIDarray.length;
                            }
                        } else {
                            if (resultIDarray.length >= 20) {
                                pageDiffentReviewStatusResultsCount[currentPageNumber - 1] = 0;
                            } else {
                                pageDiffentReviewStatusResultsCount[currentPageNumber - 1] -= resultIDarray.length;
                            }
                        }
                    }
                    else {
                        $('#Row_DetailInfo_' + resultIDarray[i]).find('#ReviewLog').text("No reviewed comments.");
                        if (SelectedReviewed == 1) {
                            if (resultIDarray.length >= 20) {
                                pageDiffentReviewStatusResultsCount[currentPageNumber - 1] = 20;
                            } else {
                                pageDiffentReviewStatusResultsCount[currentPageNumber - 1] += resultIDarray.length;
                            };
                        } else {
                            if (resultIDarray.length >= 20) {
                                pageDiffentReviewStatusResultsCount[currentPageNumber - 1] = 0;
                            } else {
                                pageDiffentReviewStatusResultsCount[currentPageNumber - 1] -= resultIDarray.length;
                            }
                        }
                    }
                }
            }
        });
    }
    //v-yiwzha Add function to get reviewlog by resultid
    function getReviewLog(resultID) {
        $.ajax({
            url: 'Ajax.aspx',
            type: 'POST',
            data: {
                IsAjax: true,
                Method: 'Ajax_GetReviewLog',
                ResultID: resultID
            },
            error: function (data) {
                //                    alert(data.responseText);
                alert("Ajax request error in reviewlog!");
            },
            success: function (data) {
                //error message
                if (data.substring(0, 4) == "[E]:") {
                    //show error message
                    var errMsg = data.substring(4);
                    alert(errMsg);
                    return;
                }
                //ReviewLog
                $('#Row_DetailInfo_' + resultID).find('#ReviewLog').text(data.substring(4));
            }
        });
    }

    function showDetailInfoAfterRow(row) {
        //detail info row position
        var resultID = $(row).attr("id");
        //v-edy: bug481286
        if ($("#Row_DetailInfo_" + resultID).length <= 0) {
            $(GetRow_DetailInfo(resultID)).insertAfter(row);
            //ajax to get detail info
            getDetailInfo(resultID);
            //ajax to get reviewlog
            getReviewLog(resultID);
            //save latest dblclick row
        }
        $("#Row_DetailInfo_" + resultID).hide();
        $("#Row_DetailInfo_" + resultID).show();
    }

    function showGroupDetailAfterRow(row) {
        //detail info row position
        $(SIGroupDetails).insertAfter(row).show();
    }
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
                //$('#Row_DetailInfo').find('#Screenshot').html('<img src="Images/ajax-loader.gif"/>');
                $('#Row_DetailInfo_' + resultID).find('#Screenshot').html('<img src="Images/ajax-loader.gif"/>');
            },
            error: function (data) {
                //                    alert(data.responseText);
                alert("Ajax request error in detail info!");
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
                $('#Row_DetailInfo_' + resultID).find('#LogMessage').text(arrData[1]);
                if (arrData[2] == 'Yes') {
                    $('#Row_DetailInfo_' + resultID).find('#LogMessage').css({ 'background-color': 'yellow' });
                }
                //screenshot
                var max_width = $('#Row_DetailInfo_' + resultID).find('#Screenshot').width();
                $('#Row_DetailInfo_' + resultID).find('#Screenshot').html('<img src="' + arrData[0] + '" />');
                //img size
                $('#Row_DetailInfo_' + resultID).find('#Screenshot').find('img').css({ 'max-width': max_width }).bind('click', function (e) {
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
    function SetDefaultAvailableOptions(arrayData, parent, HidenStartIndex) {
        $.each(arrayData, function (index, value) {
            $(parent).append(CreateTag3(value, commonTagClassName, "", ""));
        });
        //HidenStartIndex=-1 won't hidden any tag
        if (HidenStartIndex != -1) {
            HidenChildrenGT($(parent), HidenStartIndex);
        }
    }
    function SetBuildNoSelectionMode() {
        $(SIBuildSelectChanger).click(function () {
            var BuildSelectChanger = $(SIBuildSelectChanger).html();
            if (BuildSelectChanger == changeRangeSelect) {
                $(SIBuildSelectChanger).html(changeListSelect);
                $(SIBuildNoCondition_List).hide();
                $(SIBuildShowMore).hide();
                $(SIBuildNoCondition_Range).show();
            } else if (BuildSelectChanger == changeListSelect) {
                $(SIBuildSelectChanger).html(changeRangeSelect);
                $(SIBuildNoCondition_Range).hide();
                $(SIBuildNoCondition_List).show();
                $(SIBuildShowMore).show();
            }
        });

    }
    function SetConditions() {
        //Set build select changer
        SetBuildNoSelectionMode();

        //Set BuildNo for list
        SetDefaultAvailableOptions(availableBuildNo, $(SIBuildNoCondition_List), 6);

        //Set BuildNo for range
        var buildNoCount = 0;
        $.each(availableBuildNo, function (index, value) {
            buildNoCount++;
        });
        var optionStart;
        var optionEnd;
        $.each(availableBuildNo, function (index, value) {
            //if (index == 0) {
            //    optionStart = "<option value='" + value + "'>" + value + "</option>";
            //    optionEnd = "<option value='" + value + "' selected='selected'>" + value + "</option>";
            //}
            //else if (index == buildNoCount - 1) {
            //    optionStart = "<option value='" + value + "' selected='selected'>" + value + "</option>";
            //    optionEnd = "<option value='" + value + "'>" + value + "</option>";
            //} else {
            //    optionStart = "<option value='" + value + "'>" + value + "</option>";
            //    optionEnd = "<option value='" + value + "'>" + value + "</option>";
            //}
            if (index == 0) {
                optionStart = "<option value='" + value + "' selected='selected'>" + value + "</option>";
                optionEnd = "<option value='" + value + "' selected='selected'>" + value + "</option>";
            } else {
                optionStart = "<option value='" + value + "'>" + value + "</option>";
                optionEnd = "<option value='" + value + "'>" + value + "</option>";
            }
            $('#BuildNoSelect_Start').append(optionStart);
            $('#BuildNoSelect_End').append(optionEnd);
        });

        //Set BuildNo on [compared with] popup dialog
        SetDefaultAvailableOptions(availableBuildNo, $(SICompareBuildNoCondition), 6);

        //Set Build Language
        SetDefaultAvailableOptions(availableBuildLanguage, $(SIBuildLanguageCondition), -1);

        //Set Build Language on [compared with] popup dialog
        SetDefaultAvailableOptions(availableBuildLanguage, $(SICompareBuildLanguageCondition), -1);

        //Set OS Type
        SetDefaultAvailableOptions(availableOSType, $(SIOSTypeCondition), -1);

        //Set OS Language
        SetDefaultAvailableOptions(availableOSLanguage, $(SIOSLanguageCondition), 3);
        //var enLanguage = $("#SC_Rule .Tag:contains('English (United States)')").remove();
        //Cavalry: Bug 458599 Search All for BuildNo in the captured UI report doesn't work
        //$("#SC_OSLanguage .TagAll").after(object);
        //$("#SC_OSLanguage .TagAll").after(enLanguage);

        //Set Rules
        InitialRuleConditions(SIRulesCondition);
        HidenChildrenGT($(SIRulesCondition), 6);

        //Set Assembly
        $.each(availableAssembly, function (index, value) {
            $(SIAssemblyListContainer).append(CreateTag3(value, assemblyTagClassName));
        });
        HidenChildrenGT($(SIAssemblyListContainer), 2);

        //Set FullTypes
        $.each(assemblyInfoDic, function (key, value) {
            $(SIFullTypeListContainer).append(CreateTag3(key.split(',')[1], fullTypeTagClassName, value, key.split(',')[0]));
        });
        HidenChildrenGT($(SIFullTypeListContainer), 3);
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

        $(SITagAssemblyAll).click(function () {
            $(this).hasClass(SelectedTagClassName) ? $(this).removeClass(SelectedTagClassName) : $(this).addClass(SelectedTagClassName).siblings().removeClass(SelectedTagClassName);
            RemoveConvinenceTags(SIAssemblyAllConvinenceTag, SIAssemblyConvinenceContainer);
            UpdateTypesTag();
        });

        $(SITagAssembly).click(function () {
            if ($(this).hasClass(SelectedTagClassName)) {
                $(this).removeClass(SelectedTagClassName);
                RemoveConvinenceTag(SIAssemblyConvinenceContainer, SIAssemblyAllConvinenceTag, $(this).text());
            } else {
                $(this).addClass(SelectedTagClassName).siblings(":lt(1)").removeClass(SelectedTagClassName);
                AddConvinenceTag(SIAssemblyConvinenceContainer, $(this).text());
            }
            UpdateTypesTag();
        }).css({ "min-width": "300px" });

        BindingFullTypeTagAction();
    }
    function InitialConditions() {
        SetConditions();
        SetConditionActions();
        SelectDefaultCondition();
    }
    function InitialExportTestCoverageForm() {
        InitialRuleConditions(SITestCoverageRulesCondition);
        SetDefaultAvailableOptions(availableBuildLanguage, $(SITestCoverageBuildLanguagesCondition), -1);
    }
    function InitialRuleConditions(ruleContionElement) {
        //set basic rules
        $.each(basicRulesArray, function (index, value) {
            $(ruleContionElement).append(CreateTag3(availableRules[value]["RuleName"], commonTagClassName, availableRules[value]["RuleDescription"], value));
        });
        //set the other rules
        $.each(availableRules, function (index, value) {
            if (basicRulesArray.indexOf(index) == -1) {
                $(ruleContionElement).append(CreateTag3(availableRules[index]["RuleName"], commonTagClassName, availableRules[index]["RuleDescription"], index));
            }
        });
        $(ruleContionElement + " .Tag:contains('Basic')").attr("title", basicRules);
        //HidenChildrenGT($(ruleContionElement), 6);
    }
    function UpdateTypesTag() {
        RemoveConvinenceTags(SIFullTypeAllConvinenceTag, SIFullTypeConvinenceContainer);
        var arrayTemp = new Array();
        $(SISelectedAssemblyTags).each(function () {
            arrayTemp.push($(this).text().replace(/(\r\n|\n|\r)/gm, ""));
        });
        $(SIFullTypeListContainer).html('<span class="Tag TagFullTypeAll" value="All">All</span>');
        if (arrayTemp.length == 0) {
            return;
        }
        else if (arrayTemp[0] == "All") {
            $.each(assemblyInfoDic, function (key, value) {
                $(SIFullTypeListContainer).append(CreateTag3(key.split(',')[1], "TagFullType", value, key.split(',')[0]));
            });
            $(".ShowMoreActionFullType").children('img').attr({ 'src': arrowDown }).end().children('span').text("Show More");
            HidenChildrenGT($(SIFullTypeListContainer), 3);
        } else {
            $.each(assemblyInfoDic, function (key, value) {
                var c = $.inArray(value, arrayTemp);
                if (c == -1) { }
                else {
                    $(SIFullTypeListContainer).append(CreateTag3(key.split(',')[1], fullTypeTagClassName, value, key.split(',')[0]));
                }
            });
            $(".ShowMoreActionFullType").children('img').attr({ 'src': arrowUp }).end().children('span').text("Collapse");
        }
        BindingFullTypeTagAction();
    }
    function BindingFullTypeTagAction() {
        $(SITagFullTypeAll).click(function () {
            $(this).hasClass(SelectedTagClassName) ? $(this).removeClass(SelectedTagClassName) : $(this).addClass(SelectedTagClassName).siblings().removeClass(SelectedTagClassName);
            RemoveConvinenceTags(SIFullTypeAllConvinenceTag, SIFullTypeConvinenceContainer);
        });

        $(SITagFullType).click(function () {
            if ($(this).hasClass(SelectedTagClassName)) {
                $(this).removeClass(SelectedTagClassName);
                RemoveConvinenceTag(SIFullTypeConvinenceContainer, SIFullTypeAllConvinenceTag, $(this).text());
            } else {
                $(this).addClass(SelectedTagClassName).siblings(":lt(1)").removeClass(SelectedTagClassName);
                AddConvinenceTag(SIFullTypeConvinenceContainer, $(this).text());
            }
        });
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

    function GetRangeList(Tags) {
        return GetRangeList(Tags, null);
    }
    function GetRangeList(Tags, totalNum) {
        var arrayTemp = new Array();
        $(Tags).each(function () {
            arrayTemp.push($(this).text().replace(/(\r\n|\n|\r)/gm, ""));
        });
        if (arrayTemp.length == 0) {
            return;
        }
        var selectedCondition;
        if (arrayTemp[0] == 'All' || (typeof totalNum != 'undefined' && arrayTemp.length == totalNum)) {
            selectedCondition = 'All';
        }
        else {
            selectedCondition = arrayTemp.join("','");
            selectedCondition = "'" + selectedCondition + "'";
        }
        return selectedCondition;
    }

        <%--v-linchz related bugs--%>
    function getRelatedBugs(resultID) {
        $.ajax({
            url: 'Ajax.aspx',
            type: 'POST',
            data: {
                IsAjax: true,
                Method: 'Ajax_GetRelatedBugs',
                ResultID: resultID
            },
            beforeSend: function () {
                $('#RelatedBugsDialog').html('<img src="images/ajax-loader.gif"/>');
                $('#RelatedBugsDialog').dialog({ width: 'auto', height: 'auto', modal: true });
            },
            error: function (data) {
                $('#RelatedBugsDialog').dialog('close');
                alert("Ajax request error in related bug!");
            },
            success: function (data) {
                //error message
                switch (data.substring(0, 4)) {
                    case "[E]:":
                    case "[W]:":
                    case "[I]:":
                        //show error message
                        var errMsg = data.substring(4);
                        $('#RelatedBugsDialog').dialog('close');
                        alert(errMsg);
                        return;
                    default:
                        break;
                }
                //No bug found
                if (!data) {
                    var errMsg = "No related bug found.";
                    $('#RelatedBugsDialog').dialog('close');
                    alert(errMsg);
                    return;
                }
                //\n is separator for data row
                var $RelatedBugsTable = $('#RelatedBugsTableTemplate').clone();
                var relatedBugsArr = data.split('\n');
                for (var index in relatedBugsArr) {
                    //[|] is separator for data field
                    var relatedBugsRow = relatedBugsArr[index].split('[|]');
                    var bugID = relatedBugsRow[0];
                    var bugTitle = relatedBugsRow[1];
                    var bugStatus = relatedBugsRow[2];
                    var $row = $('<tr></tr>');

                    var colorSetting;
                    switch (bugStatus) {
                        case '0': // 0 means Active
                            colorSetting = 'Red';
                            break;
                        case '1': // 1 means Resolved
                            colorSetting = 'GoldenRod';
                            break;
                        case '2': // 2 means Closed
                            colorSetting = 'Green';
                            break;
                        default:
                            colorSetting = 'Black';
                            break;
                    }

                    var buglink;
                    // msazure bugs
                    if (bugID.length >= 7) {
                        buglink = '<a href="https://msazure.visualstudio.com/Configmgr/_workitems/edit/' + bugID + '/" target="_blank" style="color:' + colorSetting + ';">' + bugID + '</a>';
                    }
                    // ceapex feedback
                    else if (bugID.length <= 6) {
                        buglink = '<a href="https://ceapex.visualstudio.com/CEINTL/_workitems/edit/' + bugID + '/" target="_blank" style="color:' + colorSetting + ';">' + bugID + '</a>';
                    }

                    var paddingSetting = '0 5px 0 5px';
                    $('<td>' + buglink + '</td>').css({ padding: paddingSetting, color: colorSetting }).appendTo($row);
                    $('<td></td>').css({ padding: paddingSetting, color: colorSetting }).text(bugTitle).appendTo($row);
                    $row.appendTo($RelatedBugsTable);
                }
                $('#RelatedBugsDialog').html('').append($RelatedBugsTable.show()).dialog('option', 'position', 'center');
            }
        });
    }
    function unloadCompareWithConditionForm() {
        //unload form
        $('#CompareWithConditionForm').hide();
        //unlock screen
        $('#ScreenLocker').hide();
    }
    function unloadAttachedBugIDForm() {
        //unload form
        $('#AttachBugIDForm').hide();
        //unlock screen
        $('#ScreenLocker').hide();
    }
    function unloadGeneratePSQForm() {
        //unload form
        $('#GeneratePSQForm').hide();
        //unlock screen
        $('#ScreenLocker').hide();
    }
    function unloadExportInformationForm() {
        //unload form
        $('#ExportInformationForm').hide();
        //unlock screen
        $('#ScreenLocker').hide();
    }
    function unloadReviewLogForm() {
        //unload form
        $('#ReviewLogForm').hide();
        //unlock screen
        $('#ScreenLocker').hide();
    }
    function unloadDAForm() {
        //unload form
        $('#DeleteAuthenticationForm').hide();
        //unlock screen
        $('#ScreenLocker').hide();
    }
    function unloadCPForm() {
        //unload form
        $('#LoginAuthenticationForm').hide();
        //unlock screen
        $('#ScreenLocker').hide();
    }
    function unloadShowUIDiffForm() {
        //unload form
        $('#ShowUIDiffResultForm').hide();
        //unlock screen
        $('#ScreenLocker').hide();
    }
    function unloadExportTestCoverageForm() {
        //unload form
        $('#ExportTestCoverageForm').hide();
        //unlock screen
        $('#ScreenLocker').hide();
    }
    function GeneratePSQ(path, resultID) {
        $.ajax({
            url: 'Ajax.aspx',
            type: 'POST',
            data: {
                IsAjax: true,
                Method: 'Ajax_GeneratePSQ',
                path: path,
                resultID: resultID
            },
            error: function (data) {
                //                                        alert(data.responseText);
                alert("Fail to Generate PSQ file!");
            },
            success: function (data) {
                //error message
                switch (data.substring(0, 4)) {
                    case "[E]:":
                    case "[S]:":
                        //show error message
                        var Msg = data.substring(4);
                        alert(Msg);
                        return;
                    default:
                        alert("unknown issue");
                }
            }
        });
    }
    //function ExportInformation(path, resultID, isFilebugProcess) {
    //    $.ajax({
    //        beforeSend: function () {
    //            //$('#ScreenLocker').css({ 'height': $(document).height() }).show();
    //            $("#divLoading").show();
    //        },
    //        url: 'Ajax.aspx',
    //        type: 'POST',
    //        data: {
    //            IsAjax: true,
    //            Method: 'Ajax_ExportInformation',
    //            path: path,
    //            resultID: resultID,
    //            flag: 0
    //        },
    //        //v-edy :set time out
    //        timeout: 180000,
    //        error: function (XMLHttpRequest, textStatus, errorThrown) {
    //            $("#divLoading").hide();
    //            //                                        alert(data.responseText);
    //            if (textStatus == "timeout") {
    //                alert("File bug time out.");
    //            }
    //            alert("Fail to Export Information file!" + resultID);
    //        },
    //        success: function (data) {
    //            //error message
    //            $("#divLoading").hide();
    //            GlobalFilePath = data;
    //            if (data.substring(0, 10) == "Exception:") {
    //                alert(data);
    //            } else {
    //                if (isFilebugProcess) {
    //                    if (confirm("Successful to create pdf file:" + data + "!\n\nDo you want to continue to file bug to VSO?")) {
    //                        //var OpenBy = $(SILnkCurrentUserAlias).text();
    //                        var OpenBy = $(SILnkCurrentUserName).text().trim() + " <" + $(SILnkCurrentUserAlias).text() + "@microsoft.com>";
    //                        fileBugInVSO(OpenBy, resultID, data);
    //                    }
    //                    else if (confirm("Do you want to continue to file bug to PS?")) {
    //                        var OpenBy = $(SILnkCurrentUserAlias).text();
    //                        fileBug(OpenBy, resultID, data);
    //                    }
    //                    IsFileBug = false;
    //                }
    //                else {
    //                    alert("Successful to create pdf file: " + data + "!");
    //                }
    //            }
    //        }
    //    });
    //}
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

    function BindingPageNavigationAction() {
        $("#btnPageOK").click(function () {
            var currentPage = $("#gotoPage").val();
            if (pages != null && currentPage > pages) {
                alert('page number should less than:' + pages);
                return;
            }
            ChangePageIndex(currentPage);
            DoQuery(currentPage, 1);
            currentPageNumber = currentPage;
        });

        $("#prePage").click(function () {
            var p = parseInt($(".SelectedPageIndex").text());
            p = p - 1;
            ChangePageIndex(p);
            DoQuery(p, 1);
            currentPageNumber = p;
        });
        $("#nextPage").click(function () {
            var p = parseInt($(".SelectedPageIndex").text());
            p = p + 1;
            ChangePageIndex(p);
            DoQuery(p, 1);
            currentPageNumber = p;
        });
    }
    function BindingReadyElementActions() {
        $("#EnableDelete").click(function () {
            if (!$(this).is(":checked")) {
                $.ajax({
                    url: 'CaptureUIsService.asmx/CancelAuthutication',
                    contentType: "application/json;charset=utf-8",
                    type: 'POST',
                    dataType: 'json',
                    error: function (data) {
                        alert("Fail to disable delete!");
                    },
                    success: function (msg) {
                    }
                });
            } else {
                //lock screen
                var documentWidth = $(document).width();
                var documentHeight = $(document).height();
                $('#ScreenLocker').css({ 'height': documentHeight }).fadeIn('fast');

                //show form
                //init the form
                $('#daUserName').val('');
                $('#daPassword').val('');
                var formWidth = $("#DeleteAuthenticationForm").width();
                var formHeight = $("#DeleteAuthenticationForm").height();
                $("#DeleteAuthenticationForm").css({ "margin-top": 0 - formHeight / 2, "margin-left": 0 - formWidth / 2 }).fadeIn('fast');
                $('#daUserName').focus();
            }
        });

        /* Query Click Action */
        $(SIBtnQuery).click(function () {
            if ($("#groupbyname").is(":checked")) {
                alert("Disable the group function due to bug 481419");
                return;
            }
            searchDateTime = getNowFormatDate();
            DoQuery(0, 0);
        });

        //$(SIBtnLogin).click(function () {
        //    var documentWidth = $(document).width();
        //    var documentHeight = $(document).height();
        //    $('#ScreenLocker').css({ 'height': documentHeight }).fadeIn('fast');

        //    //show form
        //    //init the form
        //    $(SICPUserName).val('');
        //    //$('#cpPassword').val('');
        //    var formWidth = $("#LoginAuthenticationForm").width();
        //    var formHeight = $("#LoginAuthenticationForm").height();
        //    $("#LoginAuthenticationForm").css({ "margin-top": 0 - formHeight / 2, "margin-left": 0 - formWidth / 2 }).fadeIn('fast');
        //    $(SICPUserName).focus();
        //});

        $(SIBtnShowUIDiffResult).click(function () {
            $.ajax({
                url: 'Ajax.aspx',
                type: 'POST',
                data: {
                    IsAjax: true,
                    Method: 'GetDirectoryList'
                },
                timeout: 1200000,
                error: function (data) {
                    alert("Fail to access file server - SCFS!");
                },
                success: function (data) {
                    var documentWidth = $(document).width();
                    var documentHeight = $(document).height();
                    $('#ScreenLocker').css({ 'height': documentHeight }).fadeIn('fast');
                    var UIDIffResultPath = data.split('|');
                    for (var i = UIDIffResultPath.length - 1; i >= 0; i--) {
                        $('#UIDiffPath').append("<option value='" + UIDiffResultPathBase + UIDIffResultPath[i] + UIDiffResultPathRelated + "'>" + UIDIffResultPath[i] + "</option");
                    }
                    var formWidth = $("#ShowUIDiffResultForm").width();
                    var formHeight = $("#ShowUIDiffResultForm").height();
                    $("#ShowUIDiffResultForm").css({ "margin-top": 0 - formHeight / 2, "margin-left": 0 - formWidth / 2 }).fadeIn('fast');
                    $('#UIDiffPath').focus();
                }
            });
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

        $(SIExpandIconImage).toggle(function () {
            $(this).css({ "margin-left": "-19px" });
            $(SISearchBody).hide();
        }, function () {
            $(this).css({ "margin-left": "-110px" });
            $(SISearchBody).show();
        });

        $("#btnCompareOK").click(function () {
            DoCompareWith();
        });
        $("#btnCompareCancel").click(function () {
            //make sure form is shown
            if (!$('#CompareWithConditionForm').is(':visible')) {
                return;
            }

            //unload form
            unloadCompareWithConditionForm();
        });
        $("#btnAttachBugOK").click(function () {
            if (!$('#AttachBugIDForm').is(':visible')) {
                return;
            }

            var resultID = $(SISelectedRow).attr("id");
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
            GlobalBugId = '';
        });

        $("#btnDAOK").click(function () {
            if (!$('#DeleteAuthenticationForm').is(':visible')) {
                return;
            }

            var username = $('#daUserName').val();
            var password = $('#daPassword').val();

            //check authentication
            $.ajax({
                url: 'CaptureUIsService.asmx/DeleteAuthutication',
                contentType: "application/json;charset=utf-8",
                type: 'POST',
                data: "{userName:'" + username + "',password:'" + password + "'}",
                dataType: 'json',
                error: function (data) {
                    alert("Fail to get condition options!");
                },
                success: function (msg) {
                    if (msg.d == 0) {
                        alert("Username or password not correct");
                        $("#EnableDelete").removeAttr("checked");
                    }
                }
            });

            unloadDAForm();
        });

        $("#btnDACancel").click(function () {
            if (!$('#DeleteAuthenticationForm').is(':visible')) {
                return;
            }
            unloadDAForm();
            $("#EnableDelete").removeAttr("checked");
        });

        $("#btnCPOK").click(function () {
            var tempName = $(SICPUserName).val().substring($(SICPUserName).val().lastIndexOf('\\') + 1).trim();
            //Verify the account info is valid
            $.ajax({
                url: 'CaptureUIsService.asmx/GetPeopleWholeName',
                contentType: "application/json;charset=utf-8",
                type: 'POST',
                data: "{alias:'" + tempName + "'}",
                dataType: 'json',
                error: function (data) {
                    alert("Invalid account!");
                },
                success: function (data) {
                    $(SILnkCurrentUserAlias).text(tempName);
                    $(SILnkCurrentUserName).text(data.d);
                }
            });

            if (!$('#LoginAuthenticationForm').is(':visible')) {
                return;
            }

            //var username = $('#cpUserName').val();
            //var password = $('#cpPassword').val();

            ////Connect PS
            //$.ajax({
            //    url: 'Ajax.aspx',
            //    type: 'POST',
            //    data: {
            //        IsAjax: true,
            //        Method: 'Ajax_StoredPSCredential',
            //        username: username,
            //        password: password
            //    },
            //    error: function (data) {
            //        alert("Fail to Connect PS!");
            //    },
            //    success: function (data) {
            //        alert("Stored password successful!");
            //    }
            //});

            unloadCPForm();
        });

        //$("#btnCPCancel").click(function () {
        //    if (!$('#LoginAuthenticationForm').is(':visible')) {
        //        return;
        //    }
        //    unloadCPForm();
        //});
        $("#btnShowUIDiffOK").click(function () {
            if (!$('#ShowUIDiffResultForm').is(':visible')) {
                return;
            }

            var UIDiffPath = $('#UIDiffPath').val();
            window.open("CMIntlTestResults.aspx?url=" + UIDiffPath);

            unloadShowUIDiffForm();
        });

        $("#btnShowUIDiffCancel").click(function () {
            if (!$('#ShowUIDiffResultForm').is(':visible')) {
                return;
            }
            unloadShowUIDiffForm();
        });

        $("#btnExportTestCoverageOK").click(function () {
            if (!$('#ExportTestCoverageForm').is(':visible')) {
                return;
            }

            var resultID = $(SISelectedRow).attr('id');
            var buildNo = $('#txtCoverageBuild').val().trim();
            if (buildNo.length != 4) {
                alert("Incorrect build version format!");
                return;
            } else if (!/^[0-9]+$/.test(buildNo)) {
                alert("Incorrect build version!");
                return;
            }
            var languages = GetConditions(SISelectedTestCoverageBuildLanguageTags, "Please Select Build Languages.", $("#SC_BuildLanguage .Tag:gt(0)"));
            if (typeof languages == 'undefined') return;
            languages = languages.split('|').join(',');
            var rules = GetConditions(SISelectedTestCoverageRuleTags, "Please Select Rules.", $("#SC_Rule .Tag:gt(1)"));
            if (typeof rules == 'undefined') return;
            rules = rules.split('|').join(',');
            if (rules == "Basic") {
                rules = basicRules.split('|').join(',');
            }

            //validate input
            //var errMsg = validatePathInput(UIDiffPath);
            //if (errMsg != '') {
            //    alert(errMsg);
            //    return;
            //}
            //var errMsg = validatePathInput(exportPath);
            //if (errMsg != '') {
            //    alert(errMsg);
            //    return;
            //}

            //Export Test Coverage Report
            getTestCoverageResult(resultID, languages, rules, buildNo);

            unloadExportTestCoverageForm();
        });

        $("#btnExportTestCoverageCancel").click(function () {
            if (!$('#ExportTestCoverageForm').is(':visible')) {
                return;
            }
            unloadExportTestCoverageForm();
        });
        $("#btnAttachBugCancel").click(function () {
            if (!$('#AttachBugIDForm').is(':visible')) {
                return;
            }
            unloadAttachedBugIDForm();
        });
        $("#btnGeneratePSQOK").click(function () {
            if (!$('#GeneratePSQForm').is(':visible')) {
                return;
            }

            var resultID = $(SISelectedRow).attr("id");
            var path = $('#txtPSQPath').val();
            $("#psqPath").data("value", path);
            //validate input
            var errMsg = validatePathInput(path);
            if (errMsg != '') {
                alert(errMsg);
                return;
            }

            //ajax request
            GeneratePSQ(path, resultID);

            //unload form
            unloadGeneratePSQForm();
        });
        $("#btnGeneratePSQCancel").click(function () {
            if (!$('#GeneratePSQForm').is(':visible')) {
                return;
            }
            unloadGeneratePSQForm();
        });
        $("#btnExportInformationOK").click(function () {
            if (!$('#ExportInformationForm').is(':visible')) {
                return;
            }

            var resultID = $(SISelectedRow).attr("id");
            var path = $('#txtExportInformationPath').val();
            var $path = $('#pdfPath');
            $path.data("value", path);
            //validate input
            var errMsg = validatePathInput(path);
            if (errMsg != '') {
                alert(errMsg);
                return;
            }

            //ajax request
            ExportInformation(path, resultID, IsFileBug);

            //unload form
            unloadExportInformationForm();
        });
        $("#btnReviewLogOK").click(function () {
            if ($(SILnkCurrentUserAlias).text() == "Guest") {
                alert("Please Log in your work machine.");
                //alert("Please Log in with your alias, and it will be the user who mark reviewed.");
                //$(SIBtnLogin).click();
            }
            else {
                if (!$('#ReviewLogForm').is(':visible')) {
                    return;
                }
                var reviewlog = $('#txtReviewLog').val();
                var resultID = $('#txtResultID').val();

                UpdateReviewedByResultID(resultID, 1, "Reviewed by " + $(SILnkCurrentUserAlias).text() + ". " + reviewlog);

                //unload form
                unloadReviewLogForm();
            }

            //Remove below codes since the result records are updated in method - UpdateReviewedByResultID
            //binding table with data
            //var p = parseInt($(".SelectedPageIndex").text());
            //DoQuery(p, 1);
        });
        $("#btnReviewLogCancel").click(function () {
            if (!$('#ReviewLogForm').is(':visible')) {
                return;
            }
            unloadReviewLogForm();
        });
        $("#btnExportInformationCancel").click(function () {
            if (!$('#ExportInformationForm').is(':visible')) {
                return;
            }
            unloadExportInformationForm();
        });
    }
    function BindingCopyAction() {
        $("#btnCopyTestCoverageReport").click(function () {
            alert("Please copy by yourself with select the details.");
        });
    }
    function BindingShowMoreAction(Icon, StartIndex) {
        $(Icon).toggle(function () {
            $(this).children('img').attr({ 'src': arrowUp }).end().children('span').text("Collapse");
            $(this).parent().prev().find('.Tag:gt(' + StartIndex + ')').show();
        }, function () {
            $(this).children('img').attr({ 'src': arrowDown }).end().children('span').text("Show More");
            $(this).parent().prev().find('.Tag:gt(' + StartIndex + ')').hide();
        });
    }
    function BindingMenuAction() {
        $("#MenuShowScreenShot").click(function () {
            showDetailInfoAfterRow($(SISelectedRow));
        });
        $("#MenuHideScreenShot").click(function () {
            //v-edy: bug474215
            $("#Row_DetailInfo_" + $(SISelectedRow).attr("id")).hide();
        });
        $("#MenuCopyResultID").click(function () {
            var selectRowResultID = $(SISelectedRow).attr('id');
            copyToClipboard('ResultID', selectRowResultID);
        }).mouseover(function () {
            $(this).attr('title', 'ResultID: ' + $(SISelectedRow).attr('id'));
        });
        $("#MenuCopyPageTitle").click(function () {
            var selectedPageTitle = $(SISelectedRow)[0].cells[5].title;
            copyToClipboard('PageTitle', selectedPageTitle);
        });
        $("#MenuCompareWith").click(function () {
            //lock screen
            var documentHeight = $(document).height();
            $('#ScreenLocker').css({ 'height': documentHeight }).fadeIn('fast');

            //show form
            var selectedRowBuildNo = $(SISelectedRow).children(":eq(1)").text();
            var selectedRowBuildLanguage = $(SISelectedRow).children(":eq(2)").text();
            //init the form
            $("#Compare_SC_BuildNo > .Tag:contains('" + selectedRowBuildNo + "'),#Compare_SC_BuildLanguage > .Tag:contains('" + selectedRowBuildLanguage + "')")
                .addClass(SelectedTagClassName).siblings().removeClass(SelectedTagClassName);

            //show the form in the center of screen
            var formWidth = $("#CompareWithConditionForm").width();
            var formHeight = $("#CompareWithConditionForm").height();
            $("#CompareWithConditionForm").css({ "margin-top": 0 - formHeight / 2, "margin-left": 0 - formWidth / 2 }).fadeIn('fast');
        });
            <%--v-linchz related bugs--%>
        $("#MenuRelatedBugs").click(function () {
            var resultID = $(SISelectedRow).attr("id");
            var relatedbugs = getRelatedBugs(resultID);
        })


        $("#MenuAttachBugID").click(function () {
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
            $('#txtBugID').val(GlobalBugId);
        });
        $("#MenuShowReproSteps").click(function () {
            var resultID = GetSelectedRowID();
            $.ajax({
                url: 'Ajax.aspx',
                type: 'POST',
                data: {
                    IsAjax: true, Method: 'Ajax_GetReproSteps', ResultID: resultID
                },
                beforeSend: function () {
                    $('#ReproStepsDialog #LaunchedFrom').html('<img src="images/ajax-loader.gif"/>');
                    $('#ReproStepsDialog #WindowHierarchy').html('<img src="images/ajax-loader.gif"/>');
                    $('#ReproStepsDialog #CustomReproStep').html('<img src="images/ajax-loader.gif"/>');
                    $('#ReproStepsDialog').dialog({
                        width: 'auto', height: 'auto', modal: true,
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
                                        IsAjax: true, Method: 'Ajax_UpdateReproSteps', ResultID: resultID, LaunchedFrom: launchedFrom,
                                        WindowHierarchy: windowHierarchy, CustomReproStep: customReproStep
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
        });
        $("#MenuShowAssembly").click(function () {
            var resultID = GetSelectedRowID();
            var assemblyInfo = getAssemblyInfo(resultID);
        });
        $("#MenuTestCoverageReport").click(function () {
            var documentWidth = $(document).width();
            var documentHeight = $(document).height();
            $('#ScreenLocker').css({ 'height': documentHeight }).fadeIn('fast');

            //show form
            //set rule as main report page settings
            $(SIRulesCondition).children().each(function (i, n) {
                if (n.className.indexOf("SelectedTag") != -1) {
                    $($(SITestCoverageRulesCondition).children()[i]).addClass(SelectedTagClassName);
                } else {
                    $($(SITestCoverageRulesCondition).children()[i]).removeClass(SelectedTagClassName);
                }
            });

            ////set build language as main report page settings
            $(SIBuildLanguageCondition).children().each(function (i, n) {
                if (n.className.indexOf("SelectedTag") != -1) {
                    $($(SITestCoverageBuildLanguagesCondition).children()[i]).addClass(SelectedTagClassName);
                } else {
                    $($(SITestCoverageBuildLanguagesCondition).children()[i]).removeClass(SelectedTagClassName);
                }
            });

            var formWidth = $("#ExportTestCoverageForm").width();
            var formHeight = $("#ExportTestCoverageForm").height();
            $("#ExportTestCoverageForm").css({ "margin-top": 0 - formHeight / 2, "margin-left": 0 - formWidth / 2 }).fadeIn('fast');
            $('#txtCoverageBuild').focus();
        });
        $("#MenuGeneratePSQuery").click(function () {
            if ($('#psqPath').data("value") != undefined) {
                var resultID = $(SISelectedRow).attr("id");
                var path = $('#psqPath').data("value");
                //validate input
                var errMsg = validatePathInput(path);
                if (errMsg != '') {
                    alert(errMsg);
                    return;
                }

                //ajax request
                GeneratePSQ(path, resultID);
            }
            else {
                //lock screen
                var documentWidth = $(document).width();
                var documentHeight = $(document).height();
                $('#ScreenLocker').css({ 'height': documentHeight }).fadeIn('fast');

                //show form
                //init the form
                $('#txtPSQPath').val('');
                var formWidth = $("#GeneratePSQForm").width();
                var formHeight = $("#GeneratePSQForm").height();
                $("#GeneratePSQForm").css({ "margin-top": 0 - formHeight / 2, "margin-left": 0 - formWidth / 2 }).fadeIn('fast');
                $('#txtPSQPath').focus();
            }
        });
        $("#MenuExportInformation").click(function () {
            var resultID = $(SISelectedRow).attr("id");
            //var documentWidth = $(document).width();
            //var documentHeight = $(document).height();
            //$('#ScreenLocker').css({ 'height': documentHeight }).fadeIn('fast');

            ////ExportInformation(PDFFolder, resultID, false);
            //$('#ScreenLocker').hide();

            $.ajax({
                beforeSend: function () {
                    var documentHeight = $(document).height();
                    $('#ScreenLocker').css({ 'height': documentHeight }).fadeIn('fast');
                    $("#divLoading").show();
                },
                url: 'Ajax.aspx',
                type: 'POST',
                data: {
                    IsAjax: true,
                    Method: 'Ajax_ExportInformation',
                    path: PDFFolder,
                    resultID: resultID,
                    flag: 0
                },
                timeout: 180000,
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    $("#divLoading").hide();
                    var documentHeight = $(document).height();
                    $('#ScreenLocker').css({ 'height': documentHeight }).fadeOut('fast');
                    if (textStatus == "timeout") {
                        alert("Export Information file time out.");
                    }
                    alert("Fail to Export Information file!" + resultID);
                },
                success: function (data) {
                    $("#divLoading").hide();
                    var documentHeight = $(document).height();
                    $('#ScreenLocker').css({ 'height': documentHeight }).fadeOut('fast');
                    GlobalFilePath = data;
                    if (data.substring(0, 10) == "Exception:") {
                        alert(data);
                    } else {
                        alert("Successful to create pdf file: " + data + "!");
                    }
                }
            });
        });
        $("#MenuEditComments").click(function () {
            var resultid = $(SISelectedRow).attr("id");
            if ($('#Row_DetailInfo_' + resultid).find('#ReviewLog').text() == "") {
                $(GetRow_DetailInfo(resultid)).insertAfter($(SISelectedRow));
                //ajax to get detail info
                getDetailInfo(resultid);
                //ajax to get reviewlog
                getReviewLog(resultid);
                //save latest dblclick row
                $("#Row_DetailInfo_" + resultid).show();
                alert("Please review the result first, then click OK to continue");
            }
            //lock screen
            var documentWidth = $(document).width();
            var documentHeight = $(document).height();
            $('#ScreenLocker').css({ 'height': documentHeight }).fadeIn('fast');
            //show form
            //init the form
            $('#txtResultID').val(resultid);
            var reviewlog = $('#Row_DetailInfo_' + resultid).find('#ReviewLog').text();
            if (reviewlog == "No reviewed comments.") {
                $('#txtReviewLog').val();
            }
            else {
                $('#txtReviewLog').val(reviewlog.substring(18));
            }
            var formWidth = $("#ReviewLogForm").width();
            var formHeight = $("#ReviewLogForm").height();
            $("#ReviewLogForm").css({ "margin-top": 0 - formHeight / 2, "margin-left": 0 - formWidth / 2 }).fadeIn('fast');
            $('#txtReviewLog').focus();
        });
        $("#MenuDelete").click(function () {
            if (confirm("UI will be deleted from database, Do you want to continue?")) {
                var resultID = $(SISelectedRow).attr("id");
                $.ajax({
                    url: 'CaptureUIsService.asmx/DeleteByResultID',
                    contentType: "application/json;charset=utf-8",
                    type: 'POST',
                    data: "{resultID:'" + resultID + "'}",
                    dataType: 'json',
                    error: function (data) {
                        alert("Fail to delete this UI!");
                    },
                    success: function (msg) {
                        if (msg.d == 0) {
                            alert("Delete is not allow.");
                        } else {
                            DoQuery(0, 0);
                        }
                    }
                });
            } else {
                // alert("Cancel delete");
            }
        });

        //V-DANPGU FILEBUG
        $("#MenuFileBug").click(function () {
            PSUserName = $(SILnkCurrentUserName).text().trim();
            if (PSUserName == 'Guest') {
                alert("Please Log in your work machine.");
                return;
            }

            var ruleName = $(SISelectedRow).children(":eq(3)").text();
            //Currently 'FileBug' only supports basic rules.
            if (basicRules.indexOf(ruleName) < 0) {
                alert("Currently 'FileBug' only supports basic rules.");
                return;
            }

            var ResultType = $(SISelectedRow).children(":eq(4)").text();
            if (ResultType != 'Fail') {
                var RowID = $(SISelectedRow).children(":eq(0)").text();
                var conf = window.confirm('The result is NOT fail, continue to file it? (RowID: ' + RowID + ')');
                if (!conf)
                    return;
            }

            //And below rules only support ENU:
            //Access Key non Duplicate Rule
            //Tab Order Rule
            //Spelling Rule
            var languageName = $(SISelectedRow).children(":eq(2)").text();
            if (languageName == 'ENU') {
                var ResultId = $(SISelectedRow).attr("id");
                PSUserAlias = $(SILnkCurrentUserAlias).text().trim();
                //get screenshot
                var row = $(SIRowDetails + '_' + ResultId);
                var screenshotPath = $(SIRowDetails + '_' + ResultId).children("td").eq(0).children("div").eq(2).children("img").eq(0).attr("src");

                $.ajax({
                    beforeSend: function () {
                        $('#ScreenLocker').css({ 'height': $(document).height() }).show();
                        $("#divLoading").show();
                    },
                    url: 'CaptureUIsService.asmx/FileVSOBugDirectly',
                    contentType: "application/json; charset = utf-8",
                    type: 'POST',
                    data: "{openBy:'" + PSUserAlias + "',resultid:'" + ResultId + "',attachfilepath:'" + screenshotPath + "',fileMSAzureBug:'" + true + "'}",
                    dataType: 'json',
                    timeout: 180000,
                    error: function (xhr, err) {
                        alert("readyState: " + xhr.readyState + "\nstatus: " + xhr.status);
                        alert("responseText: " + xhr.responseText);
                    },
                    success: function (data) {
                        $("#divLoading").hide();
                        //$('#ScreenLocker').hide();
                        var documentHeight = $(document).height();
                        $('#ScreenLocker').css({ 'height': documentHeight }).fadeOut('fast');
                        bugid = data.d;
                        if (bugid == '-1') {
                            alert("Failed to file bug!");
                            return;
                        }
                        var tempMessage = "Filed bug: " + bugid;
                        alert(tempMessage);
                        $("#" + ResultId + " .reviewchk").prop("checked", true);
                        UpdateReviewedByResultID(ResultId, 1, "Reviewed by " + PSUserAlias);
                        GlobalBugId = bugid;
                        $("#MenuAttachBugID").click();
                    }
                });
            } else {
                if (languageName != 'ENU' && basicENURules.indexOf(ruleName) >= 0) {
                    alert("'" + ruleName + "' only supports to file bug for 'ENU'.");
                    return;
                }

                //lock screen
                $('#ScreenLocker').css({ 'height': $(document).height() }).fadeIn('fast');

                //show form
                //init the form
                var formWidth = $("#SelectBugTypeForm").width();
                var formHeight = $("#SelectBugTypeForm").height();
                $("#SelectBugTypeForm").css({ "margin-top": 0 - formHeight / 2, "margin-left": 0 - formWidth / 2 }).fadeIn('fast');
            }
        });
        $("#btnContinueFileBugYes").click(function () {
            unloadContinueFileBugForm();

            var ResultId = $(SISelectedRow).attr("id");
            PSUserAlias = $(SILnkCurrentUserAlias).text().trim();
            //get screenshot
            var row = $(SIRowDetails + '_' + ResultId);
            var screenshotPath = $(SIRowDetails + '_' + ResultId).children("td").eq(0).children("div").eq(2).children("img").eq(0).attr("src");
            var fileMSAzureBug = $('#ButTypeBug')[0].checked;

            $.ajax({
                beforeSend: function () {
                    $('#ScreenLocker').css({ 'height': $(document).height() }).show();
                    $("#divLoading").show();
                },
                url: 'CaptureUIsService.asmx/FileVSOBugDirectly',
                contentType: "application/json; charset = utf-8",
                type: 'POST',
                data: "{openBy:'" + PSUserAlias + "',resultid:'" + ResultId + "',attachfilepath:'" + screenshotPath + "',fileMSAzureBug:'" + fileMSAzureBug + "'}",
                dataType: 'json',
                timeout: 180000,
                error: function (xhr, err) {
                    alert("readyState: " + xhr.readyState + "\nstatus: " + xhr.status);
                    alert("responseText: " + xhr.responseText);
                },
                success: function (data) {
                    $("#divLoading").hide();
                    //$('#ScreenLocker').hide();
                    var documentHeight = $(document).height();
                    $('#ScreenLocker').css({ 'height': documentHeight }).fadeOut('fast');
                    bugid = data.d;
                    if (bugid == '-1') {
                        alert("Failed to file bug!");
                        return;
                    }
                    if (fileMSAzureBug) {
                        var tempMessage = "Filed bug: " + bugid;
                    } else {
                        var tempMessage = "Filed feedback: " + bugid;
                    }
                    alert(tempMessage);
                    $("#" + ResultId + " .reviewchk").prop("checked", true);
                    UpdateReviewedByResultID(ResultId, 1, "Reviewed by " + PSUserAlias);
                    GlobalBugId = bugid;
                    $("#MenuAttachBugID").click();
                }
            });

        });
        $("#btnContinueFileBugNo").click(function () {
            unloadContinueFileBugForm();
        });

        $("#FileBugWithAttachNo").click(function () {
            unloadFileBugWithAttachForm();
            //var OpenBy = $(SILnkCurrentUserAlias).text();
            var OpenBy = $(SILnkCurrentUserName).text();
            var ResultId = $(SISelectedRow).attr("id");
            FilePath = "";
            if (confirm("Do you file bug in VSO?")) {
                fileBugInVSO(OpenBy, ResultId, FilePath)
            }
            else if (confirm("Do you file bug in PS?")) {
                fileBug(OpenBy, ResultId, FilePath)
            }
        });
        $("#FileBugWithAttachCancel").click(function () {
            unloadFileBugWithAttachForm();
        });
        //v-danpgu file multi bugs
        $("#FileMultiBugWithAttachYes").click(function () {
            unloadFileMultiBugWithAttachForm();
            if (confirm("Do you file these bugs in VSO?")) {
                AttachMultiPdfsInVSO(PSUserName, FilePath, ResultIDList, false);
            }
            else if (confirm("Do you file these bugs in PS?")) {
                AttachMultiPdfs(PSUserName, FilePath, ResultIDList, false);
            }
        });

        $("#FileMultiBugWithAttachNo").click(function () {
            unloadFileMultiBugWithAttachForm();
            if (confirm("Do you file these bugs in PS?")) {
                AttachMultiPdfs(PSUserName, "", ResultIDList, false);
            }
            else if (confirm("Do you file these bugs in VSO?")) {
                AttachMultiPdfsInVSO(PSUserName, FilePath, ResultIDList, false);
            }
        });

        $("#FileMultiBugCancel").click(function () {
            unloadFileMultiBugWithAttachForm();
        });
        }

        function unloadContinueFileBugForm() {
            //unload form
            $('#SelectBugTypeForm').hide();
            //unlock screen
            $('#ScreenLocker').hide();
        }

    function unloadFileMultiBugWithAttachForm() {
        $('#FileMultiBugWithAttachForm').hide();
    }

    //V-DANPGU FILEBUG
    function unloadFileBugWithAttachForm() {
        //unload form
        $('#FileBugWithAttachForm').hide();
        //unlock screen
        $('#ScreenLocker').hide();
    }
    function HideMenu() {
        $(SIGridMenu).hide();
        $(SIRelatedBugSubMenu).hide();
    }
    function GetSelectedRowID() {
        return $(SISelectedRow).attr("id");
    }
    function ShowComapredWithResult(data) {
        var $content = $('#PopupContainer #Content').html('');
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
    function DoCompareWith() {
        //make sure form is shown
        if (!$('#CompareWithConditionForm').is(':visible')) {
            return;
        }

        var compareSelectedBuild = GetConditions(SISelectedComparedWithBuildNoTags, "Please Select build no.", $("#Compare_SC_BuildNo .Tag:gt(1)"));
        if (typeof compareSelectedBuild == 'undefined') return;
        compareSelectedBuild = compareSelectedBuild.split('|').join(',');

        var compareSelectedBuildLanguage = GetConditions(SISelectedComparedWithLanguageTags, "Please Select build language.", $("#Compare_SC_BuildLanguage .Tag:gt(1)"));
        if (typeof compareSelectedBuildLanguage == 'undefined') return;
        compareSelectedBuildLanguage = compareSelectedBuildLanguage.split('|').join(',');

        var resultID = GetSelectedRowID();

        $.ajax({
            url: 'Ajax.aspx',
            type: 'POST',
            data: {
                IsAjax: true, Method: 'Ajax_CompareWith', ResultID: resultID, BuildNos: compareSelectedBuild, BuildLanguages: compareSelectedBuildLanguage,
                IsFuzzySearch: true
            },
            beforeSend: function () {
                var buildNo = $(SISelectedRow).children(":eq(1)").text();
                var buildLanguage = $(SISelectedRow).children(":eq(2)").text();
                var rule = $(SISelectedRow).children(":eq(3)").text();
                var title = 'Search Result (BuildNo:' + buildNo + ', BuildLanguage:' + buildLanguage + ', Rule:' + rule + ')';
                $('#PopupContainer #Title').text(title);
                $('#PopupContainer #Content').html('<img src="Images/ajax-loader.gif"/>');
            },
            error: function (data) {
                alert("Fail to get compared with results!");
            },
            success: function (data) {
                if (data.substring(0, 4) == "[E]:") {
                    var errMsg = data.substring(4);
                    alert(errMsg);
                    $('#PopupContainer #Title img').click();
                    return;
                }
                ShowComapredWithResult(data);
            }
        });

        //popup container
        var documentHeight = $(document).height();
        $('#PopupContainer').css({ 'height': documentHeight, top: documentHeight }).show().animate({ top: 0 }, 1000);

        //unload condition form
        unloadCompareWithConditionForm();
    }
    function getAssemblyInfo(resultID) {
        $.ajax({
            url: 'Ajax.aspx',
            type: 'POST',
            data: {
                IsAjax: true, Method: 'Ajax_GetAssemblyInfo', ResultID: resultID
            },
            beforeSend: function () {
                $('#AssemblyInfoDialog').html('<img src="images/ajax-loader.gif"/>');
                $('#AssemblyInfoDialog').dialog({ width: 'auto', height: 'auto', modal: true });
            },
            error: function (data) {
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
    function getTestCoverageResult(resultID, languages, rules, buildNo) {
        $.ajax({
            url: 'Ajax.aspx',
            type: 'POST',
            data: {
                IsAjax: true, Method: 'Ajax_GetTestCoverageResult', resultID: resultID, languages: languages, rules: rules, buildNo: buildNo
            },
            beforeSend: function () {
                $('#TestCoverageDialog').html('<img src="images/ajax-loader.gif"/>');
                $('#TestCoverageDialog').dialog({ width: 'auto', height: 'auto', modal: true, resizable: false });
            },
            error: function (data) {
                $('#TestCoverageDialog').dialog('close');
                alert("Fail to get assembly info!");
            },
            success: function (data) {
                //error message
                if (data.substring(0, 4) == "[E]:") {
                    //show error message
                    var errMsg = data.substring(4);
                    $('#TestCoverageDialog').dialog('close');
                    alert(errMsg);
                    return;
                }

                //show Test Coverage Report Dialog
                var currentInfoArr = data.split('~');
                var assembly = currentInfoArr[0];
                var fulltypename = currentInfoArr[1];
                var currentInfo = "Build since: " + buildNo + " | Assembly: " + assembly + " | FullTypeName: " + fulltypename;
                var testCoverageReport = currentInfoArr[2];
                var $TestCoverageTable = $('#TestCoverageTableTemplate').clone();
                var $assemblyrow = $('<tr style="background: #B8BFD8"></tr>');
                $('<th colspan="25"></th>').text(currentInfo).appendTo($assemblyrow);
                $assemblyrow.appendTo($TestCoverageTable);
                var $headrow = $('<tr style=" background: #ccc"></tr>');
                var languageArr = languages.split(',');
                $('<th style="border: 1px solid"></th>').text("Rule").appendTo($headrow);
                for (var count in languageArr) {
                    $('<th style="border: 1px solid"></th>').text(languageArr[count]).appendTo($headrow);
                }
                $headrow.appendTo($TestCoverageTable);
                var testCoverageArr = testCoverageReport.split('\n');
                for (var index in testCoverageArr) {
                    var testCoverageInfo = testCoverageArr[index].split('[|]');
                    var $row = $('<tr></tr>');
                    var currentrule = null;
                    for (var lancount in testCoverageInfo) {
                        if (testCoverageInfo[lancount].toString().substring(0, 4) == "[P]:") {
                            var href = "http://" + serverName + "/SulpHurReports/CapturedUIReport.aspx?buildlanguage=" + languageArr[lancount - 1] + "&rule=" + currentrule + "&assembly=" + assembly + "&fulltypename=" + fulltypename + "&showlatest=buildno";
                            var $cell = $('<td style="border: 1px solid"></td>').css({ padding: '0 5px 0 5px' });
                            $('<a target="_blank"></a>').attr("href", href).css({ color: 'Green' }).text(testCoverageInfo[lancount]).appendTo($cell);
                            $cell.appendTo($row);
                        }
                        else if (testCoverageInfo[lancount].toString().substring(0, 4) == "[W]:") {
                            var href = "http://" + serverName + "/SulpHurReports/CapturedUIReport.aspx?buildlanguage=" + languageArr[lancount - 1] + "&rule=" + currentrule + "&assembly=" + assembly + "&fulltypename=" + fulltypename + "&showlatest=buildno";
                            var $cell = $('<td style="border: 1px solid"></td>').css({ padding: '0 5px 0 5px' });
                            $('<a target="_blank"></a>').attr("href", href).css({ color: 'GoldenRod' }).text(testCoverageInfo[lancount]).appendTo($cell);
                            $cell.appendTo($row);
                        }
                        else if (testCoverageInfo[lancount].toString().substring(0, 4) == "[F]:") {
                            var href = "http://" + serverName + "/SulpHurReports/CapturedUIReport.aspx?buildlanguage=" + languageArr[lancount - 1] + "&rule=" + currentrule + "&assembly=" + assembly + "&fulltypename=" + fulltypename + "&resulttype=Fail";
                            var $cell = $('<td style="border: 1px solid"></td>').css({ padding: '0 5px 0 5px' });
                            $('<a target="_blank"></a>').attr("href", href).css({ color: 'Red' }).text(testCoverageInfo[lancount]).appendTo($cell);
                            $cell.appendTo($row);
                        }
                        else if (testCoverageInfo[lancount] == "NotRun") {
                            $('<td style="border: 1px solid"></td>').css({ padding: '0 5px 0 5px', color: 'Grey' }).text("Not Run").appendTo($row);
                        }
                        else {
                            $('<td style="border: 1px solid;font-weight:bold"></td>').css({ padding: '0 5px 0 5px', color: 'Black' }).text(testCoverageInfo[lancount]).appendTo($row);
                            currentrule = testCoverageInfo[lancount];
                        }
                    }
                    $row.appendTo($TestCoverageTable);
                }
                var $copybutton = $('<div class="ButtonPanel"></div>');
                $('<input class="Modern-Button" id="btnCopyTestCoverageReport" type="button" value="Copy" style="margin: 0px 0 0 0px;" />').appendTo($copybutton);
                $('#TestCoverageDialog').html('').append($TestCoverageTable.show()).dialog('option', 'position', 'center');
                $copybutton.appendTo($('#TestCoverageDialog'));
                BindingCopyAction();
            }
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
                assemblyInfoDic = msg.d.hash["AssemblyInfo"];
                availableAssembly = msg.d.hash["AvailableAssembly"].split('|');
                availableBuildNo = msg.d.hash["AvailableCapturedBuilds"].split('|');
                availableBuildLanguage = msg.d.hash["AvailableCapturedLanguages"].split('|');
                availableOSType = msg.d.hash["AvailableOSTypes"].split('|');
                availableOSLanguage = msg.d.hash["AvailableOSLanguage"].split('|');
                availableRules = msg.d.hash["AvailableRules"];
                basicRules = msg.d.hash["BasicRules"];
                basicRuleIDs = msg.d.hash["BasicRuleIDs"];
                basicRulesArray = msg.d.hash["BasicRuleIDs"].split(',')
                InitialExportTestCoverageForm();
                InitialConditions();
                var queryString = window.location.search;
                if (queryString != null && queryString != '')
                    DoQuery(0, 0);
                $("#divLoading").hide();
            }
        });

        BindingPageNavigationAction();
        BindingReadyElementActions();
        BindingMenuAction();
        BindingShowMoreAction($(".ShowMoreAction"), 6);
        BindingShowMoreAction($(".ShowMoreActionFullType"), 2);
        BindingShowMoreAction($(".ShowMoreActionAssembly"), 3);
        BindingShowMoreAction($(".ShowMoreActionOSLanguage"), 3);
        $(document).bind("contextmenu", function (e) {
            return false;
        });
        $(SIGridMenu).click(function () {
            HideMenu();
        });
        $(document).click(function () {
            HideMenu();
        });

        /* Set Condition Table Style */
        $('#SearchBody table tr td').css({ 'border-bottom': '1px dotted grey' });
        $('#CompareWithConditionTable table tr td').css({ 'border-bottom': '1px dotted grey' });
        $('#UIDiffSelectRule table tr td').css({ 'border-bottom': '1px dotted grey' });
        $('#TestCoverageSelectRule table tr td').css({ 'border-bottom': '1px dotted grey' });
        $('#TestCoverageSelectBuildLanguage table tr td').css({ 'border-bottom': '1px dotted grey' });
    });
    //v-danpgu: ExpandAll/CollapseAll
    function ExpandAllTr(flag) {
        if (flag) {
            if ($(".Row_DetailInfoClass").length == 20) {
                $(".Row_DetailInfoClass").show();
            } else {
                $(".Row_DetailInfoClass").remove();
                $(SIAllRow).each(function (i, n) {
                    showDetailInfoAfterRow(n);
                })
            }
        } else {
            $(".Row_DetailInfoClass").hide();
        }
    }

    function EnableExpandRow(flag) {
        if (flag) {
            $(SIExpandAll).prop("disabled", flag).addClass("DisabledExpandButton");
            $(SICollapseAll).prop("disabled", flag).addClass("DisabledExpandButton");
        } else {
            $(SIExpandAll).prop("disabled", flag).removeClass("DisabledExpandButton");
            $(SICollapseAll).prop("disabled", flag).removeClass("DisabledExpandButton");
        }
    }

    function GetRow_DetailInfo(rowId) {
        var str = "<tr id=\"Row_DetailInfo_" + rowId + "\" style=\"height:auto;\" class=\"Row_DetailInfoClass\">"
            + "<td colspan=\"11\">"
            + "<div id=\"LogMessage\" style=\"width: 100%; margin: 5px 0 10px 0;\">"
            + "</div>"
            + "<div id=\"ReviewLog\" style=\"width: 100%; margin: 15px 0 20px 0;\">"
            + "</div>"
            + "<div id=\"Screenshot\">"
            + "</div>"
            + "</td>"
            + "</tr>";
        return str;
    }

    //v-danpgu:orderby dropdownlist bug 464598
    function EnableSortBy(flag) {
        if (flag) {
            $("#labelSortBy").show();
            $("#dropdownSortBy").val("buildNo");
            $(SIDropDownSortBy).show();
        } else {
            $(SIDropDownSortBy).hide();
            $("#labelSortBy").hide();
        }
    }
    function changeF() {
        document.getElementById('txtReviewLog').value =
            document.getElementById('reviewLogSe').options[document.getElementById('reviewLogSe').selectedIndex].value;
    }

    //v-danpgu
    function selectAllToFileBug() {
        var checkboxes = document.getElementsByClassName('chkboxForFileBug');
        if (checkboxes.length == 0) return;
        if (document.getElementById('ckbFileMultiBugs').checked) {
            for (var i = 0; i < checkboxes.length; i++) {
                checkboxes[i].checked = true;
            }
        }
        else {
            for (var i = 0; i < checkboxes.length; i++) {
                checkboxes[i].checked = false;
            }
        }
    }

    //v-yiwzha bug482148 mark all review for records in one page at the same time
    function selectAllToReview() {
        var checkboxes = document.getElementsByClassName('reviewchk');
        if (checkboxes.length == 0) return;
        if ($(SILnkCurrentUserAlias).text() == "Guest") {
            document.getElementById('ckbReviewBugs').checked = false;
            alert("Please Log in your work machine.");
            //alert("Please Log in with your alias, and it will be the user who mark reviewed.");
            //$(SIBtnLogin).click();
            return;
        }
        if (document.getElementById('ckbReviewBugs').checked) {
            //UpdateReviewedByResultID(GlobalResultIDList, 1, "Reviewed by: " + $(SILnkCurrentUserAlias).text());
            for (var i = 0; i < checkboxes.length; i++) {
                if (checkboxes[i].checked == false) {
                    checkboxes[i].checked = true;
                    getReviewLog(GlobalResultIDList.split(',')[i]);
                    var logmessage = $('#Row_DetailInfo_' + GlobalResultIDList.split(',')[i]).find('#ReviewLog').text();
                    if (logmessage == "") {
                        alert("Please expand all results then select mark review, the result " + GlobalResultIDList.split(',')[i] + " not expanded.");
                        return;
                    }
                    if (logmessage == "No reviewed comments.") {
                        UpdateReviewedByResultID(GlobalResultIDList.split(',')[i], 1, "Reviewed by: " + $(SILnkCurrentUserAlias).text());
                    }
                    else {
                        logmessage = logmessage.substring(logmessage.indexOf(':') + 1);
                        UpdateReviewedByResultID(GlobalResultIDList.split(',')[i], 1, "Reviewed by: " + $(SILnkCurrentUserAlias).text() + "|" + logmessage);
                    }
                }
            }
        }
        else {
            UpdateReviewedByResultID(GlobalResultIDList, 0, "");
            for (var i = 0; i < checkboxes.length; i++) {
                checkboxes[i].checked = false;
            }
        }
    }
    //function FileMultiBugs() {
    //    //PSUserName = $(SILnkCurrentUserAlias).text();
    //    PSUserName = $(SILnkCurrentUserName).text();
    //    if (PSUserName == 'Guest') {
    //        alert("Please Log in your work machine.");
    //    }
    //    else {
    //        var checkboxes = document.getElementsByClassName('chkboxForFileBug');
    //        if (checkboxes.length == 0)
    //            alert('No bug is listed!');
    //        ResultIDList = "";
    //        PassWarningRowIDList = '';
    //        FilePath = '\\\\scfs\\Users\\INTL\\SulphurBugFile';
    //        var j = 0;
    //        for (var i = 0; i < checkboxes.length; i++) {
    //            if (checkboxes[i].checked == true) {
    //                j++;
    //                var ResultId = checkboxes[i].value;
    //                ResultIDList = ResultIDList + ResultId + ',';
    //                if (checkboxes[i].parentNode.parentNode.cells[4].innerText != 'Fail')
    //                    PassWarningRowIDList += checkboxes[i].parentNode.parentNode.cells[0].innerText + ',';
    //            }
    //        }
    //        if (ResultIDList == '') {
    //            alert('No bug is selected!');
    //            return;
    //        }
    //        if (j > 10) {
    //            j = 0;
    //            alert('Please do not select more than ten bugs! (This operation might lead to file bug time out)');
    //        }
    //        ResultIDList = ResultIDList.substring(0, ResultIDList.length - 1);
    //        if (PassWarningRowIDList != '') {
    //            PassWarningRowIDList = PassWarningRowIDList.substring(0, PassWarningRowIDList.length - 1);
    //            var conf = window.confirm('The bug list contains result which is NOT fail, continue to file them? (RowIDList: ' + PassWarningRowIDList + ')');
    //            if (!conf)
    //                return;
    //        }
    //        $("#FileMultiBugWithAttachForm").show();
    //    }
    //}

    var retryTimes = 4;
    var resultIdArray;
    //function AttachMultiPdfs(openby, path, ResultIDList, retry) {
    //    $.ajax({
    //        beforeSend: function () {
    //            $('#ScreenLocker').css({ 'height': $(document).height() }).show();
    //            $("#divLoading").show();
    //        },
    //        url: 'Ajax.aspx',
    //        type: 'POST',
    //        data: {
    //            IsAjax: true,
    //            Method: 'Ajax_ExportMultiPdf',
    //            openby: openby,
    //            path: path,
    //            ResultIDList: ResultIDList,
    //            retry: retry
    //        },
    //        timeout: 1200000,
    //        error: function (XMLHttpRequest, textStatus, errorThrown) {
    //            if (textStatus == "timeout") {
    //                alert("File multi bugs time out.");
    //            } else {
    //                if (retryTimes > 0) {
    //                    GetRetryList(openby, path, true, false);
    //                    retryTimes--;
    //                } else {
    //                    retryTimes = 4;
    //                    GetRetryList(openby, path, false, false);
    //                }
    //            }
    //        },
    //        success: function (data) {
    //            retryTimes = 4;
    //            $("#divLoading").hide();
    //            $('#ScreenLocker').hide();
    //            resultIdArray = ResultIDList.split(',');
    //            for (var i = 0; i < resultIdArray.length; i++) {
    //                $("#" + resultIdArray[i] + " .reviewchk").prop("checked", true);
    //            }
    //            UpdateReviewedByResultID(ResultIDList, 1, "Reviewed by " + openby.substring(openby.lastIndexOf('\\') + 1));
    //            GetBugIDList(openby, true, "");
    //        }
    //    });
    //}
    //function AttachMultiPdfsInVSO(openby, path, ResultIDList, retry) {
    //    $.ajax({
    //        beforeSend: function () {
    //            $('#ScreenLocker').css({ 'height': $(document).height() }).show();
    //            $("#divLoading").show();
    //        },
    //        url: 'Ajax.aspx',
    //        type: 'POST',
    //        data: {
    //            IsAjax: true,
    //            Method: 'Ajax_ExportMultiPdfInVSO',
    //            openby: openby,
    //            path: path,
    //            ResultIDList: ResultIDList,
    //            retry: retry
    //        },
    //        timeout: 1200000,
    //        error: function (XMLHttpRequest, textStatus, errorThrown) {
    //            if (textStatus == "timeout") {
    //                alert("File multi bugs time out.");
    //            } else {
    //                if (retryTimes > 0) {
    //                    GetRetryList(openby, path, true, true);
    //                    retryTimes--;
    //                } else {
    //                    retryTimes = 4;
    //                    GetRetryList(openby, path, false, true);
    //                }
    //            }
    //        },
    //        success: function (data) {
    //            retryTimes = 4;
    //            $("#divLoading").hide();
    //            $('#ScreenLocker').hide();
    //            resultIdArray = ResultIDList.split(',');
    //            for (var i = 0; i < resultIdArray.length; i++) {
    //                $("#" + resultIdArray[i] + " .reviewchk").prop("checked", true);
    //            }
    //            UpdateReviewedByResultID(ResultIDList, 1, "Reviewed by " + openby.substring(openby.lastIndexOf('\\') + 1));
    //            GetBugIDList(openby, true, "");
    //        }
    //    });
    //}

    //function GetRetryList(openby, path, flag, isVSO) {
    //    $.ajax({
    //        url: 'CaptureUIsService.asmx/GetRetryListMethod',
    //        contentType: "application/json;charset=utf-8",
    //        type: 'POST',
    //        data: "{openby:'" + openby.replace(/\\/g, "\\\\") + "'}",
    //        dataType: 'json',
    //        error: function (data) {
    //            alert("Fail to get the retry resultIDs!");
    //        },
    //        success: function (data) {
    //            if (data.d.length != 0) {
    //                if (flag) {
    //                    if (isVSO) {
    //                        AttachMultiPdfsInVSO(openby, path, data.d, true);
    //                    }
    //                    else {
    //                        AttachMultiPdfs(openby, path, data.d, true);
    //                    }
    //                } else {
    //                    GetBugIDList(openby, flag, data.d);
    //                }
    //            }
    //        }
    //    });
    //}

    function GetBugIDList(openby, flag, failedResultIDs) {
        openby = openby.replace(/\\/g, "\\\\");
        $.ajax({
            url: 'CaptureUIsService.asmx/GetBugIDListMethod',
            contentType: "application/json;charset=utf-8",
            type: 'POST',
            data: "{openby:'" + openby + "'}",
            dataType: 'json',
            error: function (data) {
                alert("Fail to get the bugIDs!");
            },
            success: function (data) {
                if (flag) {
                    alert("Bugs have been filed successfully, the bug IDs are: " + data.d);
                } else {
                    $("#divLoading").hide();
                    $('#ScreenLocker').hide();
                    for (var i = 0; i < resultIdArray.length; i++) {
                        $("#" + resultIdArray[i] + " .reviewchk").prop("checked", true);
                    }
                    UpdateReviewedByResultID(ResultIDList, 1, "Reviewed by " + openby.substring(openby.lastIndexOf('\\') + 1));
                    alert("Successfully filed BugIDs are: " + data.d + ". The following ResultIDs are failed to file bug:" + failedResultIDs);
                }
            }
        });
    }

    //V-YIWZHA FILEBUG
    function fileBugInVSO(PSUserName, ResultId, FilePath) {
        if (FilePath != null) {
            FilePath = FilePath.replace(/\\/g, "\\\\");
        }
        $.ajax({
            beforeSend: function () {
                //$('#ScreenLocker').css({ 'height': $(document).height() }).show();
                //$("#divLoading").show();
            },
            url: 'CaptureUIsService.asmx/FileVSOBugDirectly',
            contentType: "application/json; charset = utf-8",
            type: 'POST',
            data: "{openBy:'" + PSUserAlias + "',resultid:'" + ResultId + "',attachfilepath:'" + FilePath + "'}",
            dataType: 'json',
            timeout: 180000,
            error: function (xhr, err) {
                alert("readyState: " + xhr.readyState + "\nstatus: " + xhr.status);
                alert("responseText: " + xhr.responseText);
            },
            success: function (data) {
                //$("#divLoading").hide();
                //$('#ScreenLocker').hide();
                if (data.d == '-1') {
                    alert("Failed to file bug!");
                    return;
                }
                bugid = data.d;
                var tempMessage = "Filed bug: " + bugid;
                alert(tempMessage);
                $("#" + ResultId + " .reviewchk").prop("checked", true);
                UpdateReviewedByResultID(ResultId, 1, "Reviewed by " + PSUserAlias);
                GlobalBugId = bugid;
                $("#MenuAttachBugID").click();
            }
        });
    }
    //V-DANPGU FILEBUG
    function fileBug(PSUserName, ResultId, FilePath) {
        if (FilePath != null) {
            FilePath = FilePath.replace(/\\/g, "\\\\");
        }
        PSUserName = PSUserName.replace(/\\/g, "\\\\");
        $.ajax({
            beforeSend: function () {
                $('#ScreenLocker').css({ 'height': $(document).height() }).show();
                $("#divLoading").show();
            },
            url: 'CaptureUIsService.asmx/FileBug',
            contentType: "application/json; charset = utf-8",
            type: 'POST',
            data: "{openby:'" + PSUserName + "',resultid:'" + ResultId + "',attachfilepath:'" + FilePath + "'}",
            dataType: 'json',
            timeout: 180000,
            error: function (xhr, err) {
                alert("readyState: " + xhr.readyState + "\nstatus: " + xhr.status);
                alert("responseText: " + xhr.responseText);
            },
            success: function (data) {
                $("#divLoading").hide();
                $('#ScreenLocker').hide();
                $("#" + ResultId + " .reviewchk").prop("checked", true);
                UpdateReviewedByResultID(ResultId, 1, "Reviewed by " + PSUserName.substring(PSUserName.lastIndexOf('\\') + 1));
                //v-edy: bug481854
                if (data.d.indexOf("No need") + 1) {
                    alert("No need to file bug");
                }
                else {
                    if (confirm(data.d + "\n\nDo you want to Attach the bugID?")) {
                        var bugid = data.d;
                        GlobalBugId = bugid;
                        $("#MenuAttachBugID").click();

                    }
                }
                //alert(data.d);
            }

        });
    }

    </script>
</head>
<body>
    <form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true">
    </asp:ScriptManager>
    <div id="ScreenLocker" oncontextmenu="return false;" style="filter: alpha(opacity=80);">
    </div>
         <%--       v-danpgu filebug--%>
    <div id="divLoading" class="img-Loading">
    </div>
    <table style="display: none;">
        <tr id="Row_DetailInfo" style=" height:auto;">
            <td colspan="11">
                <div id="LogMessage" style='white-space: pre-line'>
                <%--<div id="LogMessage" style="width: 100%; margin: 5px 0 10px 0;">--%>
                </div>
                <div id="ReviewLog">
                </div>
                <div id="Screenshot">
                </div>
            </td>
        </tr>

    </table>
    <table>
        <tr id="GroupDetailsContainer" style=" height:auto;">
        <td  colspan="11">
        <div style=" float:left; width:1170px; margin:auto;">
            <table id="GroupDetails" style="table-layout:fixed;width:1170px;display:none;">
            </table>
        </div>
        </td>
        </tr>
    </table>
    <div id="GridMenu" class="PopupMenu" style="background-color:#e5eecc;border:solid 1px #c3c3c3;">
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
    <img alt='copy' id="CopyIcon" src="Images\copy.png" style="display:none; width:14px;"/>
    <div id="ZoomInImageBlock" class="PopupWindow">
        <div name="Title"><span name="Build" style="float: left;"></span><span name="Result"  style="float: right;"></span><br style="clear:both;" /></div>
        <div name="LogMessage"></div>
        <img hotkey="ESC" onclick="ZoomInImageBlock_Clicked(this);"/>
    </div>
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
    <div id="AssemblyInfoDialog", title="Assembly Info" style=" text-align: center; vertical-align:middle;">
    </div>
    <table id="AssemblyInfoTableTemplate" style=" width: auto; display: none;line-height: 150%; border-collapse: collapse; border:1px solid gray;">
        <tr style=" background: #ccc">
            <th>Assembly Name</th>
            <th>Full Type Name</th>
            <th>Is Page Identifier</th>
        </tr>
    </table>
    <div id="TestCoverageDialog" title="Test Coverage Report" style="overflow-x:auto; text-align: center; vertical-align:middle;">
    </div>
    <table id="TestCoverageTableTemplate" style=" width: auto; display: none;line-height: 150%; border-collapse: collapse; border:1px solid gray;">
    </table>
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
    <div id="ImageBlockTemplate" class="image-block">
        <div name="Title"><span name="Build" style="float: left;"></span><span name="Result"  style="float: right;"></span><br style="clear:both;" /></div>
        <div name="LogMessage"></div>
        <img onload="ResizeImage(this, 300, 280);" onclick="ImageBlockTemplate_Clicked(this);"/>
    </div>
    <%--v-linchz related bugs--%>
    <div id="RelatedBugsDialog", title="Related Bugs" style=" text-align: left; vertical-align:middle;">
    </div>
    <table id="RelatedBugsTableTemplate" style=" width: auto; display: none;line-height: 150%; border-collapse: collapse; border:1px solid gray;">
        <tr style=" background: #ccc">
            <th>Bug ID</th>
            <th>Bug Title</th>
        </tr>
    </table>
    <div id="AttachBugIDForm" class="PopupWindow" oncontextmenu="return false;">
        <div>
            <span style="float: left; margin: 0 10px 0 0px;">Bug ID:</span>
            <input id="txtBugID" type="text" value="" style="width: 70%;" />
            <br />
            <span style="color: blue; font: italic bold 12px/20px arial,sans-serif;">* Split multi
                bug with "," (example: 123456,467890)</span>
        </div>
        <div class="ButtonPanel">
            <input hotkey="ENTER" class="Modern-Button" id="btnAttachBugOK" type="button" value="OK" style="margin: 0px 0 0 0px;" />
            <input hotkey="ESC" class="Modern-Button" id="btnAttachBugCancel" type="button" value="Cancel"
                style="margin: 0px 0 0 20px;" />
        </div>
    </div>
    <div id="SelectBugTypeForm" class="PopupWindow" oncontextmenu="return false;">
        <div>
            <span style="float: left; margin: 0 10px 0 0px;">Bug Type:</span>
            <br />
            <input id="ButTypeBug" name="BugType" type="radio" value="Bug" checked="checked" />Bug
            <br />
            <input id="ButTypeFeedback" name="BugType" type="radio" value="Feedback" />Feedback
        </div>
        <div class="ButtonPanel">
            <input hotkey="ENTER" class="Modern-Button" id="btnContinueFileBugYes" type="button" value="Continue" style="margin: 0px 0 0 0px;" />
            <input hotkey="ESC" class="Modern-Button" id="btnContinueFileBugNo" type="button" value="Cancel" style="margin: 0px 0 0 20px;" />
        </div>
    </div>
    <div id="GeneratePSQForm" class="PopupWindow" oncontextmenu="return false;">
        <div>
            <span style="float: left; margin: 0 10px 0 0px;">Generate PSQ to:</span>
            <input id="txtPSQPath" type="text" value="" style="width: 70%;" />
            <br />
            <span style="color: blue; font: italic bold 12px/20px arial,sans-serif;">(Please input the path you want to export.)</span>
        </div>
        <div class="ButtonPanel">
            <input hotkey="ENTER" class="Modern-Button" id="btnGeneratePSQOK" type="button" value="OK" style="margin: 0px 0 0 0px;" />
            <input hotkey="ESC" class="Modern-Button" id="btnGeneratePSQCancel" type="button" value="Cancel"
                style="margin: 0px 0 0 20px;" />
        </div>
    </div>

                  <%-- v-danpgu filebug--%>
    <div id="FileBugWithAttachForm" class="PopupWindow" oncontextmenu="return false;">
        <div>
            <span style="float: left; margin: 0 10px 0 0px; ">Do you want to attach the .pdf file of bug details to this bug?</span>
            <br />
            <br />
        </div>
        <div class="ButtonPanel">
            <input hotkey="ENTER"  id="FileBugWithAttachYes" type="button" value="Yes" style="margin: 0px 0 0 0px;" />
            <%--v-linchz-no need the "No" button, because if someone open a bug from sulphur, he/she must attach the .pdf--%>
            <%--<input hotkey="ESC" id="FileBugWithAttachNo" type="button" value="No" style="margin: 0px 0 0 20px;" />--%>
            <input id="FileBugWithAttachCancel" type="button" value="Cancel" style="margin: 0px 0 0 20px;" />
        </div>
    </div>

   <div id="FileMultiBugWithAttachForm" class="PopupWindow" oncontextmenu="return false;">
        <div>
            <span style="float: left; margin: 0 10px 0 0px; ">Do you want to attach files?</span>
            <br />
            <br />
        </div>
        <div class="ButtonPanel">
            <input hotkey="Y"  id="FileMultiBugWithAttachYes" type="button" value="Yes" style="margin: 0px 0 0 0px;" />
            <input hotkey="N" id="FileMultiBugWithAttachNo" type="button" value="No" style="margin: 0px 0 0 20px;" />
            <input hotkey="ESC" id="FileMultiBugCancel" type="button" value="Cancel" style="margin: 0px 0 0 20px;" />
        </div>
    </div>

    <div id="ExportInformationForm" class="PopupWindow" oncontextmenu="return false;">
        <div>
            <span style="float: left; margin: 0 10px 0 0px;">Export Information to:</span>
            <input id="txtExportInformationPath" type="text" value="\\scfs\Users\INTL\SulphurBugFiles" style="width: 70%;" />
            <br />
            <span style="color: blue; font: italic bold 12px/20px arial,sans-serif;">(Please input the path you want to export.)</span>
            <br />
            <span style="color: red; font: italic bold 12px/20px arial,sans-serif;">(For file bug to PS, the path must be \\scfs\Users\INTL\SulphurBugFiles.)</span>
        </div>
        <div class="ButtonPanel">
            <input hotkey="ENTER" class="Modern-Button" id="btnExportInformationOK" type="button" value="OK" style="margin: 0px 0 0 0px;" />
            <input hotkey="ESC" class="Modern-Button" id="btnExportInformationCancel" type="button" value="Cancel"
                style="margin: 0px 0 0 20px;" />
        </div>
    </div>
    <div id="ExportTestCoverageForm" class="PopupWindow" oncontextmenu="return false;" style="width:950px;">
        <div>
            <table id="TestCoverageSelectRule" style="width:850px;">
                <tr>
                    <td style="width:160px;">
                        Export Rules:
                    </td>
                    <td id="TestCoverage_Rule">
                        <span class="Tag TagAll" value='All'>All</span> <span class="Tag TagAll" value="Basic">Basic</span>
                    </td>
                    <td>
                    </td>
                </tr>
             </table>
        </div>
        <div>
            <table id="TestCoverageSelectBuildLanguage" style="width:850px;">
                <tr>
                    <td style="width:160px;">
                        Export Build Languages:
                    </td>
                    <td id="TestCoverage_BuildLanguage">
                        <span class="Tag TagAll">All</span>
                    </td>
                    <td>
                    </td>
                </tr>
             </table>
        </div>
        <div>
            <span style="float: left; margin: 0 10px 0 0px;">Verified above the Build:</span>
            <input id="txtCoverageBuild" type="text" value="" style="width: 30%;" />
            <br />
            (Please enter build main version. E.g. 8838 represents 5.0.8838.1000)
            <br />
        </div>
        <div class="ButtonPanel">
            <input hotkey="ENTER" class="Modern-Button" id="btnExportTestCoverageOK" type="button" value="OK" style="margin: 0px 0 0 0px;" />
            <input hotkey="ESC" class="Modern-Button" id="btnExportTestCoverageCancel" type="button" value="Cancel"
                style="margin: 0px 0 0 20px;" />
        </div>
    </div>
    <div id="ReviewLogForm" class="PopupWindow" oncontextmenu="return false;">
        <div>
            <span style="float: left; margin: 0 10px 0 0px;">ResultID:</span>
            <input id="txtResultID" type="text" value="" style="width: 70%;" disabled="disabled" />
            <br />
            <span style="float: left; margin: 0 10px 0 0px;">Review Comments:</span>
            <br />
            <span style="position:absolute;border:1pt solid #c1c1c1;overflow:hidden;width:278px;height:19px;Clip:rect(-1px 280px 190px 260px);">
            <select name="reviewLogSe" id="reviewLogSe" style="width:280px;height:20px;margin:-2px;" onchange="changeF()"> 
		<option value=""></option>
                <option value="The control in failed result no need access key.">The control in failed result no need access key.</option>
                <option value="Previous control can focus on miss hotkey panel.">Previous control can focus on miss hotkey panel.</option>
                <option value="Related duplicate controls are all schedured controls, keep it duplicate.">Related duplicate controls are all schedured controls, keep it duplicate.</option>
                <option value="No available key to assign in both 2 duplicate controls.">No available key to assign in both 2 duplicate controls.</option>
            </select>
            </span>
            <span style="position:absolute;border-top:1pt solid #c1c1c1;border-left:1pt solid #c1c1c1;border-bottom:1pt solid #c1c1c1;width:260px;height:19px;">
                <input id="txtReviewLog" type="text" name="txtReviewLog" value="" style="width:260px;height:15px;border:0pt;" />
            </span>
            <br />
            <br />
            <span style="color: blue; font: italic bold 12px/20px arial,sans-serif;">(Please input BugID or why you mark the result as reviewed.)</span>
        </div>
        <div class="ButtonPanel">
            <input hotkey="ENTER" class="Modern-Button" id="btnReviewLogOK" type="button" value="OK" style="margin: 0px 0 0 0px;" />
	    <input hotkey="ESC" class="Modern-Button" id="btnReviewLogCancel" type="button" value="Cancel"
                style="margin: 0px 0 0 20px;" />
        </div>
    </div>
    <div id="CompareWithConditionForm" style="width:810px;" class="PopupWindow">
    <div>
            <table id="CompareWithConditionTable">
                <tr>
                    <td>
                        Build No:
                    </td>
                    <td id="Compare_SC_BuildNo">
                        <span class="Tag TagAll">All</span>
                    </td>
                    <td>
                        <div class="ShowMoreContainer ShowMoreAction">
                            <img src="Images/arrow1.jpg" alt="" /><span>Show More</span></div>
                    </td>
                </tr>
                <tr>
                    <td>
                        Build Language:
                    </td>
                    <td id="Compare_SC_BuildLanguage">
                        <span class="Tag TagAll">All</span>
                    </td>
                    <td>
                    </td>
                </tr>
            </table>
    </div>
            <div class="ButtonPanel">
            <input hotkey="ENTER" class="Modern-Button" type="button" value="OK" style="margin:0px 0 0 0px;" id="btnCompareOK"/>
            <input hotkey="ESC" class="Modern-Button" type="button" value="Cancel" style="margin:0px 0 0 20px;" id="btnCompareCancel" />
        </div>
    </div>
    <div id="DeleteAuthenticationForm" class="PopupWindow" oncontextmenu="return false;">
        <div>
            <span style="float: left; margin: 0 10px 0 0px; width:90px;">User Name:</span>
            <input id="daUserName" type="text" value="" style="width: 100px;" />
            <br />
        </div>
        <div>
            <span style="float: left; margin: 0 10px 0 0px;width:90px;">Password:</span>
            <input id="daPassword" type="password" value="" style="width: 100px;" />
            <br />
        </div>
        <div class="ButtonPanel">
            <input hotkey="ENTER" class="Modern-Button" id="btnDAOK" type="button" value="OK" style="margin: 0px 0 0 0px;" />
            <input hotkey="ESC" class="Modern-Button" id="btnDACancel" type="button" value="Cancel"
                style="margin: 0px 0 0 20px;" />
        </div>
    </div>
        <div id="LoginAuthenticationForm" class="PopupWindow" oncontextmenu="return false;">
        <div>
            <span style="float: left; margin: 0 10px 0 0px; width:90px;">User Name:</span>
            <input id="cpUserName" type="text" value="Guest" style="width: 100px;" runat="server" />
            <span style="float: left; margin: 0 10px 0 0px; width:180px;">(Example: fareast\v-yiwzha)</span>
            <br />
        </div>
        <div class="ButtonPanel">
            <input hotkey="ENTER" class="Modern-Button" id="btnCPOK" type="button" value="OK" style="margin: 0px 0 0 0px;" runat="server" />
            <input hotkey="ESC" class="Modern-Button" id="btnCPCancel" type="button" value="Cancel" style="margin: 0px 0 0 20px;"  runat="server"/>
        </div>
    </div>
    <div id="ShowUIDiffResultForm" style="width:555px;" class="PopupWindow" oncontextmenu="return false;" runat="server">
        <center>
        <div >
            <h3>Please select UIDiff build path:</h3>
            <select id="UIDiffPath" style="width: 200px;height:25px;border-radius:3px;box-shadow:0 0 3px #ccc;" />
            <input hotkey="ENTER" class="Modern-Button" id="btnShowUIDiffOK1" type="button" value="OK" style="margin: 0px 0 0 0px;display:none" />
        </div>
        </center>
        <div class="ButtonPanel" >
            <input hotkey="ENTER" class="Modern-Button" id="btnShowUIDiffOK" type="button" value="OK" style="margin: 0px 0 0 0px;" />
            <input hotkey="ESC" class="Modern-Button" id="btnShowUIDiffCancel" type="button" value="Cancel" style="margin: 0px 0 0 20px;" />
        </div>
    </div>
    <div id="PageHeader" class="divTitle">
        <div class="divNavigator">
            <!--<a href="javascript:nav_targetblank('http://SulpHurServer15.redmond.corp.microsoft.com/SulpHur/')">HOME</a> >-->
            <a href="javascript:nav_targetblank('http://AzureSulpHur1.redmond.corp.microsoft.com/SulpHur/')">HOME</a> >
            <span>Captured UIs</span>
        </div>
        <div class="divUserInfo">
            Welcome!
        </div>
        <div class="divUserInfo" id="pnlWinLoginInfo">
            <a id="lnkCurrentUserName" style="color:deepskyblue" runat="server">Guest</a>
            <a id="lnkCurrentUserAlias" style="color:deepskyblue;display:none" runat="server">Guest</a>
            <a id="lnkCurrentUserImage" runat="server" runat="server"></a>
        </div>
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
                            <div style="color:blue"><span id="BuildSelectChanger_span">Change to range select</span></div>
                        </td>
                        <td id="SC_BuildNo">
                            <div id="SC_BuildNo_List">
                                <span class="Tag TagAll">All</span>
                            </div>
                            <div id="SC_BuildNo_Range" style="display:none">
                                <select id="BuildNoSelect_Start">
                                </select>~~~
                                <select id="BuildNoSelect_End">
                                </select>
                            </div>
                        </td>
                        <td>
                            <div id="BuildNoShowMore" class="ShowMoreContainer ShowMoreAction">
                                <img src="Images/arrow1.jpg" alt="" /><span>Show More</span></div>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Build Language:
                        </td>
                        <td id="SC_BuildLanguage">
                            <span class="Tag TagAll">All</span>
                        </td>
                        <td>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Build Type:
                        </td>
                        <td id="SC_BuildType">
                            <span class="Tag TagAll">All</span> <span class="Tag TagCommon">Main</span> <span class="Tag TagCommon">Private</span>
                        </td>
                        <td>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            OS Type:
                        </td>
                        <td id="SC_OSType">
                            <span class="Tag TagAll">All</span>
                        </td>
                        <td>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            OS Language:
                        </td>
                        <td id="SC_OSLanguage">
                            <span class="Tag TagAll">All</span>
                        </td>
                        <td>
                            <div class="ShowMoreContainer ShowMoreActionOSLanguage">
                                <img src="Images/arrow1.jpg" alt="" /><span>Show More</span></div>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Rule:
                        </td>
                        <td id="SC_Rule">
                            <span class="Tag TagAll" value='All'>All</span> 
                            <span class="Tag TagAll" value="Basic">Basic</span>
                        </td>
                        <td>
                            <div class="ShowMoreContainer ShowMoreAction">
                                <img src="Images/arrow1.jpg" alt="" /><span>Show More</span>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Result:
                        </td>
                        <td id="SC_Result">
                            <span class="Tag TagAll">All</span>
                            <span class="Tag TagCommon">Fail</span>
                            <span class="Tag TagCommon">Info</span>
                            <span class="Tag TagCommon">Pass</span>
                            <span class="Tag TagCommon">Warning</span>
                        </td>
                        <td>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Assembly Name:
                        </td>
                        <td id="SC_AssemblyName">
                            <div class="HaveSelected">
                                <b>You Have Selected:</b>
                            </div>
                            <div class="LargeListTd">
                                <span class="Tag TagAssemblyAll">All</span>
                            </div>
                        </td>
                        <td class="td-topAlign">
                            <!-- 
                            <div class="ShowMoreContainer">
                                <img src="Images/arrow1.jpg" alt="" /><span>MultiSelect</span></div>
                                -->
                            <div class="ShowMoreContainer ShowMoreActionAssembly">
                                <img src="Images/arrow1.jpg" alt="" /><span>Show More</span></div>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Full Type Name:
                        </td>
                        <td id="SC_FullTypeName">
                            <div class="HaveSelected">
                                <b>You Have Selected:</b>
                            </div>
                            <div class="LargeListTd">
                                <span class="Tag TagFullTypeAll" value="All">All</span>
                            </div>
                        </td>
                        <td class="td-topAlign">
                            <!--
                            <div class="ShowMoreContainer">
                                <img src="Images/arrow1.jpg" alt="" /><span>MultiSelect</span></div>  -->
                            <div class="ShowMoreContainer ShowMoreActionFullType">
                                <img src="Images/arrow1.jpg" alt="" /><span>Show More</span></div>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Review Status:
                        </td>
                        <td id="SC_Reviewed">
                            <span class="Tag TagAll">All</span>
                            <span class="Tag TagCommon">UnReviewed</span>
                            <span class="Tag TagCommon">Reviewed</span>
                        </td>
                        <td>
                        </td>
                    </tr>
                    <tr>
                    <td>User Name:</td>
                    <td><input type="text" id="alias" size="18" maxlength="28" /></td>
                    <td></td>
                    </tr>
                    <tr>
                    <td>Page Title:</td>
                    <td><input type="text" id="pageTitle" size="52" maxlength="500" /></td>
                    <td></td>
                    </tr>
                    <tr>
                        <td>Review Comments:</td>
                        <td><input type="text" id="reviewComments" size="52" maxlength="500" /></td>
                        <td></td>
                    </tr>
                    <tr>
                        <td>ResultID:</td>
                        <td><input type="text" id="resultid" size="18" maxlength="28" /></td>
                        <td></td>
                    </tr>
                </table>
                <div id="ButtonContainer">
                    <input class="Modern-Button" type="button" value="Search" id="btnQuery" name="btnQuery" />
                    <input class="Modern-Button" type="button" value="Expand All" id="btnExpandAll" name="btnExpandAll" onclick="ExpandAllTr(true)" />
                    <input class="Modern-Button" type="button" value="Collapse All" id="btnCollapseAll" name="btnCollapseAll" onclick="ExpandAllTr(false)" />
                    <input class="Modern-Button" type="button" value="Log In" id="btnLogin" name="btnLogin" style="display:none" />
                    <!--<input class="Modern-Button" type="button" value="File Multi Bugs" id ="btnFileMultiBugs" name="btnFileMultiBugs" onclick="FileMultiBugs()" />-->
                    <input class="Modern-Button" type="button" value="UIDiff Result" id="btnShowUIDiffResult" name="btnShowUIDiffResult" />
                    <input type="hidden" id="pdfPath" name="pdfPath" value=""/>
                    <input type="hidden" id="psqPath" name="psqPath" value=""/>
                </div>
            </div>
        </div>

        <div id="ConditionFilter">
            <input type="checkbox" id="EnableDelete" value="Enable Delete" />Enable Delete
	        <input type="checkbox" id="groupbyname"  value="Group By UI" onclick="EnableExpandRow(this.checked)" />Group By UI
            <input type="checkbox" id="showLatest"  value="Show Latest Build" name="showLatest" onclick="EnableSortBy(this.checked)" checked />Latest Result 

               <select name="selectDays" id="selectDays">
              <option value="7">7 days</option>
              <option value="14" selected>14 days</option>
              <option value="30">1 month</option>
              <option value="60">2 month</option>
              <option value="90">3 month</option>             
              <option value="365">1 year</option>             
              <option value="730">2 year</option>             
          </select>

            <label style="display:none" id="labelSortBy">Order By</label>
            <select name="Sort By" id="dropdownSortBy"  style="display:none">
                <option value="ResultID">ResultID</option>
                <option value="buildNo">Build No. </option>
                <option value="uiName">Page Title</option>
                <option value="uploadTime">Upload Time </option>
            </select>
        </div>

        <div id="Results">
            <div id="ResultsTableContainer">
                <table id="ResultsTable">
                    <col width="27px;" />
                    <col width="110px;" />
                    <col width="58px;" />
                    <col width="160px;" />
                    <col width="49px;" />
                    <col style="min-width: 50px;" />
                    <col width="100px;" />
                    <col width="103px;" />
                    <col width="129px;" />
                    <col width="129px;" />
                    <col width="56px;" />
                    <col width="62px;" />
                    <col width="38px;" />
                    <thead>
                        <tr>
                            <th>
                                Row ID
                            </th>
                            <th>
                                Build No.
                            </th>
                            <th>
                                Build Language
                            </th>
                            <th>
                                Rule Name
                            </th>
                            <th>
                                Result
                            </th>
                            <th>
                                Page Title
                            </th>
                            <th>
                                User Name
                            </th>
                            <th>
                                OS Type
                            </th>
                            <th>
                                Date Uploaded (UTC)
                            </th>
                            <th>
                                Date Checked (UTC)
                            </th>
                            <th>
                                Reviewed <br />
                                <input type="checkbox" id="ckbReviewBugs" onclick="selectAllToReview(this)"/>
                            </th>
                            <th>
                                Comments
                            </th>
                            <th>
                                 Select All<br />
                                 <input type="checkbox" id="ckbFileMultiBugs" onclick="selectAllToFileBug(this)" /> 
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
                    <input  class="icon-btn" type="button" value="Previous" />
                </div>
                <div id="PageIndexs"></div>
                <div id="nextPage">
                    <input  class="icon-btn" type="button" value="Next"/>
                </div>
                <div id="TotalPageInfo"></div>
                <div id="pageIndexContainer">Go&nbsp;&nbsp;<input type="text" id="gotoPage" style="width: 38px; display:inline"/></div>
                <div id="btnIndexOKContainer">&nbsp;<input class="icon-btn" type="button" value="ok" id="btnPageOK" /></div>
            </div>
        </div>
    </div>
    </form>
    <div style=" height:20px; float:left; width:1000px;"></div>
</body>
</html>