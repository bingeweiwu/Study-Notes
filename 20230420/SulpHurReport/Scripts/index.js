﻿//class Name 
var commonTagClassName = 'TagCommon';
var assemblyTagClassName = 'TagAssembly';
var SelectedTagClassName = 'selected';
var fullTypeTagClassName = 'TagFullType';
var SIBuildNoCondition_List;
var SIExpandButton = '#Expand';

//jquery selector id
//SI:selector id
var SICompareBuildNoCondition = '#SC_BuildNo';
var SIBuildLanguageCondition = '#SC_BuildLanguage';
var SIOSTypeCondition = '#SC_OSType';
var SIOSLanguageCondition = '#SC_OSLanguage';
var SIRulesCondition = '#SC_Rule';
var SIResultTypeCondition = '#SC_ResultType';
var SIAssemblyNameCondition = '#SC_AssemblyName';
var SIFullTypeCondition = '#SC_FullTypeName';
var SIReviewedStatusCondition = '#SC_ReviewedStatus';
var SIResultBody = '#ResultsBody';
var SIPromptMessage = '#Prompt-Message';
var SISearch = '#search';

var SIBuildNo_List = '#SC_BuildNo .selected';
var SIBuildLanguage_List = '#SC_BuildLanguage .selected';
var SIOSType_List = '#SC_OSType .selected';
var SIOSLanguege_List = '#SC_OSLanguage .selected';
var SIRule_List = '#SC_Rule .selected';
var SIResultType_List = '#SC_ResultType .selected';
var SIAssemblyName_List = '#SC_AssemblyName .selected';
var SIFullTypeName_List = '#SC_FullTypeName .selected';
var SIReviewedStatus_List = '#SC_ReviewedStatus .selected';

var pagetitle;
var reviewComments;
var searchDateTime;
var showLatest;
var SelectedReviewed;
var resultid;
var alias;
var latestDays;
var SelectedSortBy;
var pageResults = new Array();
var GlobalResultIDList;
var pages;
var pageIndex = "1";
var pageDiffentReviewStatusResultsCount = new Array();


var searchValue;
var SelectedBuildNos;
var SelectedBuildLanguages;
var SelectedOSTypes;
var SelectedOSLanguages;
var SelectedRules;
var SelectedResultTypes;
var SelectedAssemblyNames;
var SelectedFulltypeNames;
var SelectedReviewedStatus;

var assemblyInfoDic;
var availableBuildNo;
var availableBuildLanguage;
var availableOSType;
var availableOSLanguage;
var availableRules;
var availableAssembly;
var availableRuleIDs = '';

var ruleCount = $('#SC_Rule .TagCommon.selected').length;

$(SIExpandButton).click(function () {
    const categoryList = document.querySelectorAll('.list-category');
    categoryList.forEach(list => {
        list.classList.remove('collapse');
    });
});

$(document).ready(function () {
    QueryClear();
    $('.list-category').on('click', '.Tag', function () {
        var value = $(this).data('value');
        var row = $(this).closest('.category-item');
        var classNames = row.attr('class').split(' ');
        var className = classNames[1];

        if (value == 'All') {
            if (showLatest && className === 'Rule') {
                alert("Only one rule can be selected now for latest result!");
                return;
            }
            row.find('.Tag').removeClass('selected');
            $(this).addClass('selected');
            getSelectedCondition();
            Query();
        } else {
            if (className === 'Rule') {
                if (showLatest) {
                    row.find('.Tag').removeClass('selected');
                    $(this).addClass('selected');
                } else {
                    $(this).toggleClass('selected');
                    row.find('.Tag[data-value="All"]').removeClass('selected');
                    ruleCount = $('#SC_Rule .TagCommon.selected').length
                    if (ruleCount == 0) {
                        alert("At least one rule is selected.");
                        return;
                    }
                }
            }
            else {
                row.find('.Tag[data-value="All"]').removeClass('selected');
                $(this).toggleClass('selected');
                var allUnchecked = row.find('.TagCommon').length === row.find('.TagCommon:not(.selected)').length;
                row.find('.TagAll').toggleClass('selected', allUnchecked);
            }
            getSelectedCondition();
            Query();
        }
    });

    var checkbox = document.getElementById("showLatest");
    checkbox.addEventListener("change", function () {

        if (this.checked) {
            if (ruleCount > 1) {
                alert("Only one rule can be selected now for latest result!");
                $('#SC_Rule .TagCommon').removeClass('selected');
                return;
            }
            getSelectedCondition();
            console.log("selected");
        } else {
            if (ruleCount == 0) {
                row.find('.Tag[data-value="All"]').addClass('selected');
            }
            getSelectedCondition();
            console.log("not selected");
        }
        Query();
    });


    $('#selectDays').on('change', function () {
        if (showLatest) {
            console.log("showLatest selected and change days");
            getSelectedCondition();
            Query();
        }
    });

});

$(function () {
    /* Load and Initial Condition options data */
    $.ajax({
        url: 'IndexService.asmx/QueryAvailableData',
        contentType: "application/json;charset=utf-8",
        type: 'POST',
        dataType: 'json',
        error: function (data) {
            console.log(data.responseText);
            console.log(data);
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
            for (var item in availableRules) {
                if (availableRuleIDs == '')
                    availableRuleIDs = item;
                else
                    availableRuleIDs += ',' + item;
            }
            //InitialExportTestCoverageForm();
            InitialConditions();
            //  getSelectedCondition();

            var queryString = window.location.search;
            if (queryString != null && queryString != '')
                DoQuery(0, 0);
            $("#divLoading").hide();
        }
    });

    var timer = setInterval(function () {
        var parent = document.getElementById('SC_Rule');
        var child = parent.querySelector('.TagCommon');
        if (child !== null) {
            SetDefaultCondition();
            getSelectedCondition();
            Query();
            clearInterval(timer);
        }
    }, 1000);
});

function Search() {
    var searchValue = $(SISearch).val();
    if (searchValue == null || searchValue == "") {
        alert("searchValue must have a value");
        return;
    }
    GlobalResultIDList = "";
    $.ajax({
        type: "POST",
        url: "IndexService.asmx/GetSearchResults",
        data: "{parameter:\"" + searchValue + "\",searchDateTime:\"" + searchDateTime + "\",latestDays:\"" + latestDays + "\",pageIndex:\"" + pageIndex + "\"}",
        contentType: "application/json; charset=utf-8",
        error: function (data) {
            alert("Fail to get records result!");
        },
        success: function (data) {
            if (data.d.length == 0) {
                $(SIPromptMessage).show().html('No records found according to the conditions.');
                QueryClear();
            } else {
                $(SIPromptMessage).hide();
                QueryClear();
                var unreviewedCount = 0;
                var reviewedCount = 0;
                for (var i = 0; i < data.d.length; i++) {
                    GlobalResultIDList = GlobalResultIDList + data.d[i].ResultID + ",";
                    var j = i + 1;
                    var checkboxhtml
                    var havereviewlog;
                    if (data.d[i].ReviewFlag == 1) {
                        checkboxhtml = "<input type=\"checkbox\" class='reviewchk' id='CheckedStatus" + i + "' checked=true />";
                        havereviewlog = "Yes";
                    } else {
                        checkboxhtml = "<input type=\"checkbox\" class='reviewchk' id='CheckedStatus" + i + "' />";
                        havereviewlog = "No";
                    }

                    $(ResultsBody).append("<tr class='CommonTr' id='" + data.d[i].ResultID + "''><td class='tdCss'>" + j
                        + "</td><td class='tdCss'>" + data.d[i].BuildNo
                        + "</td><td class='tdCss'>" + data.d[i].Language
                        + "</td><td class='tdCss'>" + data.d[i].RuleName
                        + "</td><td class='tdCss'>" + data.d[i].ResultType
                        + "</td><td class='tdCss' title='" + data.d[i].UIName + "'>" + data.d[i].UIName
                        + "</td><td class='tdCss'>" + data.d[i].UserName
                        + "</td><td class='tdCss'>" + data.d[i].OSType
                        + "</td><td class='tdCss'>" + data.d[i].DateUploadedStr
                        + "</td><td class='tdCss'>" + data.d[i].DateCheckedStr
                        + "</td><td class='tdCss' id='HaveComments'>" + havereviewlog
                        + "</td><td class='tdCss'>" + checkboxhtml
                        /*+ "</td><td class='tdCss'>" + "<input type=\"checkbox\" class='chkboxForFileBug' value = '" + data.d[i].ResultID + "' />"*/
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
        //success: function (data) {            
        //    if (data.d > 20) {
        //        pages = Math.ceil(data.d / 20);
        //        EnablePageNavigation(data.d);
        //        $('#PageNavigation').show();
        //    } else {
        //        $('#PageNavigation').hide();
        //    }
        //    resultsCount = data.d;
        //    if (data.d == 0) {                
        //        $("#divLoading").hide();
        //        $(SIPromptMessage).show().html('No records found according to the conditions.');
        //    } else {
        //        ShowResults(SelectedBuildNos, SelectedBuildLanguages, SelectedOSTypes, SelectedOSLanguages, SelectedRules, SelectedResultTypes,
        //            SelectedAssemblyNames, SelectedFulltypeNames, SelectedReviewedStatus, SelectedSortBy,
        //            searchDateTime, 0, showLatest, latestDays,);
        //    }

        //}
    });
}


function Query() {
    pageIndex = 1;
    console.log("enter query ");
    SelectedSortBy = "Build No.";
    QueryClear();
    $.ajax({
        type: "POST",
        url: "IndexService.asmx/GetResultNumber",

        data: "{buildNOs:\"" + SelectedBuildNos + "\",buildLans:\"" + SelectedBuildLanguages + "\",osTypes:\"" + SelectedOSTypes + "\",osLanguages:\""
            + SelectedOSLanguages + "\",ruleIDs:\"" + SelectedRules + "\",resultTypes:\"" + SelectedResultTypes + "\",assemblyNames:\"" + SelectedAssemblyNames
            + "\",typeIDs:\"" + SelectedFulltypeNames + "\",ReviewedStatus:\"" + SelectedReviewedStatus + "\",searchDateTime:\"" + searchDateTime + "\",getLatest:\""
            + showLatest + "\",latestDays:\"" + latestDays /*+ "\",parameter:\"" + searchValue*/+"\"}",

        contentType: "application/json; charset=utf-8",
        error: function (data) {
            alert("Fail to get records count!");
            //            console.log("error = "+ error);
        },
        success: function (data) {
            //console.log("response = " + response);
            if (data.d > 20) {
                pages = Math.ceil(data.d / 20);
                EnablePageNavigation(data.d);
                //  $('#TotalPageInfo').html("Total " + pages + " Pages");
                $('#PageNavigation').show();
            } else {
                $('#PageNavigation').hide();
            }
            resultsCount = data.d;
            if (data.d == 0) {
                // $(SIBtnQuery).removeAttr("disabled");
                $("#divLoading").hide();
                $(SIPromptMessage).show().html('No records found according to the conditions.');
                QueryClear();
            } else {
                //ShowResults(SelectedBuildNos, SelectedBuildLanguages, SelectedOSTypes, SelectedOSLanguages, SelectedRules, SelectedResultTypes,
                //    SelectedAssemblyNames, SelectedFulltypeNames, SelectedReviewedStatus, SelectedSortBy,
                //    searchDateTime, 0, showLatest, latestDays,pageIndex);
                //ShowResults(SelectedBuildNos, SelectedBuildLanguages, SelectedOSTypes, SelectedOSLanguages, SelectedRules, SelectedResultTypes,
                //    SelectedAssemblyNames, SelectedFulltypeNames, SelectedReviewedStatus, searchDateTime, showLatest, latestDays, SelectedSortBy, pageResults, pageIndex);
                ShowResults(pageIndex);
            }

        }
    });
}

//function ShowResults(SelectedBuildNos, SelectedBuildLanguages, SelectedOSTypes, SelectedOSLanguages, SelectedRules, SelectedResultTypes,
//    SelectedAssemblyNames, SelectedFulltypeNames, SelectedReviewedStatus, searchDateTime, showLatest, latestDays, SelectedSortBy, pageResults, pageIndex) {
function ShowResults(pageIndex) {
    console.log("enter ShowResult");    
    GlobalResultIDList = "";
    $.ajax({
        type: "POST",
        url: "IndexService.asmx/GetConditionQueryResults",
        data: "{buildNOs:\"" + SelectedBuildNos + "\",buildLans:\"" + SelectedBuildLanguages + "\",osTypes:\"" + SelectedOSTypes + "\",osLanguages:\""
            + SelectedOSLanguages + "\",ruleIDs:\"" + SelectedRules + "\",resultTypes:\"" + SelectedResultTypes + "\",assemblyNames:\"" + SelectedAssemblyNames
            + "\",typeIDs:\"" + SelectedFulltypeNames + "\",ReviewedStatus:\"" + SelectedReviewedStatus + "\",searchDateTime:\"" + searchDateTime + "\",getLatest:\""
            + showLatest + "\",latestDays:\"" + latestDays + "\",sortBy:\"" + SelectedSortBy + "\",currentPageResults:\"" + pageResults[1]
            + "\",pageIndex:\""
            + pageIndex /*+ "\",parameter:\"" + searchValue*/ + "\"}",
        contentType: "application/json; charset=utf-8",
        error: function (data) {
            alert("Fail to query records!");

        },
        success: function (data) {
            if (data.d.length == 0) {
                $(SIPromptMessage).show().html('No records found according to the conditions.');
                QueryClear();
            } else {
                $(SIPromptMessage).hide();
                var unreviewedCount = 0;
                var reviewedCount = 0;
                for (var i = 0; i < data.d.length; i++) {
                    GlobalResultIDList = GlobalResultIDList + data.d[i].ResultID + ",";
                    var j = i + 1;
                    var checkboxhtml
                    var havereviewlog;
                    if (data.d[i].ReviewFlag == 1) {
                        checkboxhtml = "<input type=\"checkbox\" class='reviewchk' id='CheckedStatus" + i + "' checked=true />";
                        havereviewlog = "Yes";
                    } else {
                        checkboxhtml = "<input type=\"checkbox\" class='reviewchk' id='CheckedStatus" + i + "' />";
                        havereviewlog = "No";
                    }

                    $(ResultsBody).append("<tr class='CommonTr' id='" + data.d[i].ResultID + "''><td class='tdCss'>" + j
                        + "</td><td class='tdCss'>" + data.d[i].BuildNo
                        + "</td><td class='tdCss'>" + data.d[i].Language
                        + "</td><td class='tdCss'>" + data.d[i].RuleName
                        + "</td><td class='tdCss'>" + data.d[i].ResultType
                        + "</td><td class='tdCss' title='" + data.d[i].UIName + "'>" + data.d[i].UIName
                        + "</td><td class='tdCss'>" + data.d[i].UserName
                        + "</td><td class='tdCss'>" + data.d[i].OSType
                        + "</td><td class='tdCss'>" + data.d[i].DateUploadedStr
                        + "</td><td class='tdCss'>" + data.d[i].DateCheckedStr
                        + "</td><td class='tdCss' id='HaveComments'>" + havereviewlog
                        + "</td><td class='tdCss'>" + checkboxhtml
                        /*+ "</td><td class='tdCss'>" + "<input type=\"checkbox\" class='chkboxForFileBug' value = '" + data.d[i].ResultID + "' />"*/
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

function QueryClear() {
    /*document.getElementById("ResultsBody").innerHTML = "";*/
    $(SIResultBody).html('');
    $('#PageIndexs').html('');
    $('#PageNavigation').hide();
}

function QueryClear_old() {
    $('#GroupDetailsContainer').appendTo('body');
    $(SIRowDetails).appendTo('body');
    $(SIRowDetails).hide();
    $(SIGroupDetails).html('');
    $(SIGroupDetails).hide();
    $(SIResultBody).html('');
    $(SIPromptMessage).show().html('<img class="Loading-Image" src="Images/loading.gif"/>');
}

//create web page tag like  <span class="Tag TagCommon selected" data-value="48">Spelling Rule</span>
function CreateSelectorLabels() {
    SetDefaultAvailableOptions(availableBuildNo, $(SICompareBuildNoCondition), 6);
    SetDefaultAvailableOptions(availableBuildLanguage, $(SIBuildLanguageCondition), -1);
    SetDefaultAvailableOptions(availableOSType, $(SIOSTypeCondition), -1);
    SetDefaultAvailableOptions(availableOSLanguage, $(SIOSLanguageCondition), 3);
    $.each(availableAssembly, function (index, value) {
        $(SIAssemblyNameCondition).append(CreateTag3(value, assemblyTagClassName));
    });
    $.each(assemblyInfoDic, function (key, value) {
        $(SIFullTypeCondition).append(CreateTag3(key.split(',')[1], commonTagClassName, value, key.split(',')[0]));
        //$(SIFullType_List).append(CreateTag3(key.split(',')[1], fullTypeTagClassName, value, key.split(',')[0]));
    });
    InitialRuleConditions(SIRulesCondition);

}
//SetDefaultAvailableOptions(availableBuildLanguage, $(SIBuildLanguageCondition), -1);
function SetDefaultAvailableOptions(arrayData, parent, HidenStartIndex) {
    $.each(arrayData, function (index, value) {
        $(parent).append(CreateTag3(value, commonTagClassName, "", ""));
    });
    //HidenStartIndex=-1 won't hidden any tag
    //if (HidenStartIndex != -1) {
    //    HidenChildrenGT($(parent), HidenStartIndex);
    //}
}

function SetDefaultCondition() {
    var tagIndexes = [7];
    $.each(tagIndexes, function (index, value) {
        $('#SC_BuildLanguage > .Tag:eq(' + value + ')').addClass(SelectedTagClassName);
    });
    //$('#SC_BuildLanguage > .Tag:eq(1,2)').addClass(SelectedTagClassName).siblings().removeClass(SelectedTagClassName);
    $('#SC_BuildNo > .Tag:eq(0)').addClass(SelectedTagClassName).siblings().removeClass(SelectedTagClassName);
    $('#SC_OSType > .Tag:eq(0)').addClass(SelectedTagClassName).siblings().removeClass(SelectedTagClassName);
    $('#SC_OSLanguage > .Tag:eq(0)').addClass(SelectedTagClassName).siblings().removeClass(SelectedTagClassName);
    $('#SC_Rule > .Tag:eq(4)').addClass(SelectedTagClassName).siblings().removeClass(SelectedTagClassName);
    $('#SC_AssemblyName > .Tag:eq(0)').addClass(SelectedTagClassName).siblings().removeClass(SelectedTagClassName);
    $('#SC_FullTypeName > .Tag:eq(0)').addClass(SelectedTagClassName).siblings().removeClass(SelectedTagClassName);
}

function getSelectedCondition() {
    SelectedBuildNos = GetRangeList(SIBuildNo_List);
    SelectedBuildLanguages = GetRangeList(SIBuildLanguage_List);
    SelectedOSTypes = GetRangeList(SIOSType_List);
    SelectedOSLanguages = GetRangeList(SIOSLanguege_List);
    SelectedRules = GetRangeList(SIRule_List);
    //SelectedRules = GetSelectedRules(SIRule_List);
    SelectedResultTypes = GetRangeList(SIResultType_List, 4);
    SelectedAssemblyNames = GetRangeList(SIAssemblyName_List);
    SelectedFulltypeNames = GetRangeList(SIFullTypeName_List);
    SelectedReviewedStatus = GetRangeList(SIReviewedStatus_List, 2);
    alias = $("#UserName").text();
    latestDays = $("#selectDays").val();
    showLatest = document.getElementById("showLatest").checked;
    searchDateTime = getNowFormatDate();
    ruleCount = $('#SC_Rule .TagCommon.selected').length;
    console.log(alias);
}

function InitialRuleConditions(ruleContionElement) {
    $.each(availableRules, function (index, value) {
        $(ruleContionElement).append(CreateTag4(availableRules[index]["RuleName"], commonTagClassName, availableRules[index]["RuleDescription"], index));
    });
}
function InitialConditions() {
    CreateSelectorLabels();
}

function CreateTag3(text, actionClass, tagTitle, value) {
    // console.log('<span class="Tag ' + actionClass + '" data-value="' + text + '">' + text + '</span>');
    return '<span class="Tag ' + actionClass + '" data-value="' + text + '">' + text + '</span>';
}

function CreateTag4(text, actionClass, tagTitle, value) {
    //console.log('<span class="Tag ' + actionClass + '" data-value="' + value + '">' + text + '</span>');
    return '<span class="Tag ' + actionClass + '" data-value="' + value + '">' + text + '</span>';
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

function RemoveAllConvinenceTags() {
    RemoveConvinenceTags(SIAssemblyAllConvinenceTag, SIAssemblyConvinenceContainer);
    RemoveConvinenceTags(SIFullTypeAllConvinenceTag, SIFullTypeConvinenceContainer);
}
function RemoveConvinenceTags(AllConvinenceTags, ConvinenceTagContainer) {
    $(AllConvinenceTags).each(function () {
        $(this).remove();
    });
    $(ConvinenceTagContainer).hide();
}

function GetRangeList(Tags) {
    return GetRangeList(Tags, null);
}
function GetRangeList(Tags, totalNum) {
    var arrayTemp = new Array();
    $(Tags).each(function () {
        arrayTemp.push($(this).data('value'));
        //arrayTemp.push($(this).text().replace(/(\r\n|\n|\r)/gm, ""));
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

function Expand() {
    var btn = document.querySelector(".Expand button");
    var buildNo = document.getElementById("SC_BuildNo");
    var span = buildNo.querySelector(".TagAll");

    if (btn.textContent === "Expand") {
        btn.textContent = "Collapse";
        span.style.paddingRight = "59px";
    } else {
        btn.textContent = "Expand";
        span.style.paddingRight = "28px";
    }
    var items = document.getElementsByClassName("category-item");
    for (var i = 0; i < items.length; i++) {
        items[i].classList.toggle("collapse");
    }
}

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
        $('#PageIndexs').append(CreatePageTag(pages));

    }
    $('#PageIndexs .PageTag').click(function () {
        var currentPage = $(this).text();
        $(SIResultBody).html('');  
        ChangePageIndex(currentPage);
        ShowResults(currentPage);
        //DoQuery(currentPage, 1);
    });
    $('#PageIndexs .PageTag:first').addClass("SelectedPageIndex");
    $('#prePage').hide();
}

function Previous() {
    var p = parseInt($(".SelectedPageIndex").text());
    p = p - 1;
    ChangePageIndex(p);
    $(SIResultBody).html('');
    ShowResults(p);
    currentPageNumber = p;
}

function Next() {
    var p = parseInt($(".SelectedPageIndex").text());
    p = p + 1;
    ChangePageIndex(p);    
    $(SIResultBody).html('');  
    ShowResults(p);
    currentPageNumber = p;
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
        $('#PageIndexs').append(CreatePageTag(pages));
    } else if (pages > 7 && v > 4 && pages - v >= 3) {
        $('#PageIndexs').append(CreatePageTag(1));
        $('#PageIndexs').append("...");
        for (var i = 1; i <= 7; i++) {
            var c = v - 4 + i;
            $('#PageIndexs').append(CreatePageTag(c));
        }
        $('#PageIndexs').append("...");
        $('#PageIndexs').append(CreatePageTag(pages));
    } else if (pages > 7 && pages - v < 3) {
        $('#PageIndexs').append(CreatePageTag(1));
        $('#PageIndexs').append("...");
        for (var i = 7; i >= 1; i--) {
            var c = pages - i + 1;
            $('#PageIndexs').append(CreatePageTag(c));
        }
    }
    $('#PageIndexs .PageTag').click(function () {
        var currentPage = $(this).text();
        ChangePageIndex(currentPage);
        $(SIResultBody).html('');
        ShowResults(currentPage);
    });
    $("#PageIndexs .PageTag").each(function () {
        if ($(this).text() == curentPageIndex) {
            $(this).addClass("SelectedPageIndex");
        }
    });
}

function CreatePageTag(number) {
    return "<span class='PageTag'>" + number + "</span>";
}