//class Name 
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

//var container = document.querySelector('.expandable');
//var isExpanded = false;


$(SIExpandButton).click(function () {
    const categoryList = document.querySelectorAll('.list-category');
    categoryList.forEach(list => {
        list.classList.remove('collapse');
    });
});

//const collapseButton = document.querySelector('.collapseButton');
//collapseButton.addEventListener('click', expand);


/*
$('.list-category').on('click', '.Tag', function () {
    var value = $(this).data('value');
    var row = $(this).closest('.category-item');
    var rowClass = row.attr('class');
    if (value == 'All') {
        row.find('.Tag').removeClass('selected');
        $(this).addClass('selected');
        getSelectedCondition();
        Query();
    } else {
        row.find('.Tag[data-value="All"]').removeClass('selected');
        $(this).toggleClass('selected');
        var allUnchecked = row.find('.Tag[data-value!="All"]').length === row.find('.Tag[data-value!="All"].selected').length;
        row.find('.Tag[data-value="All"]').toggleClass('selected', allUnchecked);
        getSelectedCondition();
        Query();
    }
});
*/

$(document).ready(function () {
    $('.list-category').on('click', '.Tag', function () {
        var value = $(this).data('value');
        var row = $(this).closest('.category-item');
        var rowClass = row.attr('class');
        if (value == 'All') {
            row.find('.Tag').removeClass('selected');
            $(this).addClass('selected');
            getSelectedCondition();
            Query();
        } else {
            //row.find('.Tag[data-value="All"]').removeClass('selected');
            //$(this).toggleClass('selected');
            //getSelectedCondition();
            //Query();


            //console.log(row.find('.TagCommon').length);
            //console.log(row.find('.TagCommon .selected').length);
            //console.log();
            // var allChecked = row.find('.Tag[data-value!="All"]').length === row.find('.Tag[data-value!="All"].selected').length;
            // row.find('.Tag[data-value="All"]').toggleClass('selected', allChecked);

            row.find('.Tag[data-value="All"]').removeClass('selected');
            $(this).toggleClass('selected');
            var allUnchecked = row.find('.TagCommon').length === row.find('.TagCommon:not(.selected)').length;
            row.find('.TagAll').toggleClass('selected', allUnchecked);
            getSelectedCondition();
            Query();
        }
    });
    $('#selectDays').on('change', function () {
        getSelectedCondition();
        Query();
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

function Query() {
    $.ajax({
        type: "POST",
        url: "IndexService.asmx/ReceiveCondition",
        //data: "{buildLan:\"" + SelectedBuildLanguages + "\"}",

        //data: "{buildLan:\"" + SelectedBuildLanguages + "\",osType:\"" + SelectedOSTypes + "\",osLanguage:\""
        //    + "All" + "\",ruleid:\"" + SelectedRules + "\",resultTypes:\"" + SelectedResultTypes
        //    + "\",ReviewedType:\"" + SelectedReviewedStatus
        //    + "\"}",

        data: "{buildNO:\"" + SelectedBuildNos + "\",buildLan:\"" + SelectedBuildLanguages + "\",osType:\"" + SelectedOSTypes + "\",osLanguage:\""
            + SelectedOSLanguages + "\",ruleid:\"" + SelectedRules + "\",resultTypes:\"" + SelectedResultTypes + "\",assemblyName:\"" + SelectedAssemblyNames
            + "\",typeID:\"" + SelectedFulltypeNames + "\",ReviewedType:\"" + SelectedReviewedStatus + "\"}",
        
        contentType: "application/json; charset=utf-8",
       
        success: function (response) {
            console.log(response);            
        },
        error: function (response) {
            console.log("error");
        }
    });
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
    $('#SC_Rule > .Tag:eq(6)').addClass(SelectedTagClassName).siblings().removeClass(SelectedTagClassName);
    $('#SC_AssemblyName > .Tag:eq(0)').addClass(SelectedTagClassName).siblings().removeClass(SelectedTagClassName);
    $('#SC_FullTypeName > .Tag:eq(0)').addClass(SelectedTagClassName).siblings().removeClass(SelectedTagClassName);
}

function getSelectedCondition() {
    SelectedBuildNos = GetRangeList(SIBuildNo_List);
    SelectedBuildLanguages = GetRangeList(SIBuildLanguage_List);
    SelectedOSTypes = GetRangeList(SIOSType_List);
    SelectedOSLanguages = GetRangeList(SIOSLanguege_List);
    SelectedRules = GetRangeList(SIRule_List);
    SelectedResultTypes = GetRangeList(SIResultType_List, 4);
    SelectedAssemblyNames = GetRangeList(SIAssemblyName_List);
    SelectedFulltypeNames = GetRangeList(SIFullTypeName_List);
    SelectedReviewedStatus = GetRangeList(SIReviewedStatus_List, 2);    
    var alias = $("#UserName").text();
    var latestDays = $("#selectDays").val();
    console.log(alias);
    console.log(latestDays);
    //console.log('after click  value is ' + SelectedBuildLanguages + ' ' + SelectedOSTypes + ' ' + SelectedRules + ' ' + SelectedResultTypes + ' ' + SelectedReviewedStatus);
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

//function expand() {
//    const categoryList = document.querySelectorAll('.list-category');
//    categoryList.forEach(list => {
//        list.classList.remove('collapse');
//    });
//}

//const collapseButton = document.querySelector('.collapseButton button');
//collapseButton.addEventListener('click', expand);

//var container = document.querySelector('.category-item');
//var isExpanded = false;

//container.addEventListener('click', function () {
//    if (!isExpanded) {
//        container.style.whiteSpace = 'normal';
//        container.style.overflow = 'visible';
//        container.style.textOverflow = 'clip';
//        isExpanded = true;
//    } else {
//        container.style.whiteSpace = 'nowrap';
//        container.style.overflow = 'hidden';
//        container.style.textOverflow = 'ellipsis';
//        isExpanded = false;
//    }
//});
