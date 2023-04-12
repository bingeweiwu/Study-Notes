//class Name 
var commonTagClassName = 'TagCommon';
var SelectedTagClassName = 'selected';
var SIBuildNoCondition_List;

//jquery selector id
//SI:selector id
var SIBuildLanguageCondition = '#SC_BuildLanguage';
var SIOSTypeCondition = '#SC_OSType';
var SIRulesCondition = '#SC_Rule';

var SIOSLanguageCondition = '#SC_OSLanguage';
var SIBuildLanguage_List = '#SC_BuildLanguage .selected';
var SIOSType_List = '#SC_OSType .selected';
var SIRule_List = '#SC_Rule .selected';
var SIResultType_List = '#ResultType .selected';
var SIReviewedStatus_List = '#ReviewedStatus .selected';

var SelectedBuildLanguages;
var SelectedOSTypes;
var SelectedRules;
var SelectedResultTypes;
var SelectedReviewedStatus;

var assemblyInfoDic;
var availableBuildNo;
var availableBuildLanguage;
var availableOSType;
var availableOSLanguage;
var availableRules;
var availableAssembly;
var availableRuleIDs = '';


$(document).ready(function () {
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
            getSelectedCondition();
            Query();
            // var allChecked = row.find('.Tag[data-value!="All"]').length === row.find('.Tag[data-value!="All"].selected').length;
            // row.find('.Tag[data-value="All"]').toggleClass('selected', allChecked);
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
            getSelectedCondition();
            var queryString = window.location.search;
            if (queryString != null && queryString != '')
                DoQuery(0, 0);
            $("#divLoading").hide();
        }
    });
});

function Query() {
    $.ajax({
        type: "POST",
        url: "IndexService.asmx/ReceiveCondition",
        //data: "{buildLan:\"" + SelectedBuildLanguages + "\"}",
        data: "{buildLan:\"" + SelectedBuildLanguages + "\",osType:\"" + SelectedOSTypes + "\",osLanguage:\""
            + "All" + "\",ruleid:\"" + SelectedRules + "\",resultTypes:\"" + SelectedResultTypes
            + "\",ReviewedType:\"" + SelectedReviewedStatus
            + "\"}",
        
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
    SetDefaultAvailableOptions(availableBuildLanguage, $(SIBuildLanguageCondition), -1);
    SetDefaultAvailableOptions(availableOSType, $(SIOSTypeCondition), -1);
    InitialRuleConditions(SIRulesCondition);
}

function SetDefaultCondition() {
    var tagIndexes = [1, 3, 5];
    $.each(tagIndexes, function (index, value) {
        $('#SC_BuildLanguage > .Tag:eq(' + value + ')').addClass(SelectedTagClassName);            
    });
    //$('#SC_BuildLanguage > .Tag:eq(1,2)').addClass(SelectedTagClassName).siblings().removeClass(SelectedTagClassName);
    $('#SC_OSType > .Tag:eq(1)').addClass(SelectedTagClassName).siblings().removeClass(SelectedTagClassName);
    $('#SC_Rule > .Tag:eq(1)').addClass(SelectedTagClassName).siblings().removeClass(SelectedTagClassName);
}

function getSelectedCondition() {
    SelectedBuildLanguages = GetRangeList(SIBuildLanguage_List);
    SelectedOSTypes = GetRangeList(SIOSType_List);
    SelectedRules = GetRangeList(SIRule_List);
    SelectedResultTypes = GetRangeList(SIResultType_List, 4);
    SelectedReviewedStatus = GetRangeList(SIReviewedStatus_List, 2);
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
function SetConditions() {
    //Set build select changer
    //SetBuildNoSelectionMode();

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
   // SetDefaultAvailableOptions(availableBuildNo, $(SICompareBuildNoCondition), 6);

    //Set Build Language
    SetDefaultAvailableOptions(availableBuildLanguage, $(SIBuildLanguageCondition), -1);

    //Set Build Language on [compared with] popup dialog
    //SetDefaultAvailableOptions(availableBuildLanguage, $(SICompareBuildLanguageCondition), -1);

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


//SetDefaultAvailableOptions(availableBuildLanguage, $(SIBuildLanguageCondition), -1);
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