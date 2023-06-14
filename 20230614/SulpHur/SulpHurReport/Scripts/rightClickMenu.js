var GlobalBugId;
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
    // <%--v - linchz related bugs-- %>
    $("#MenuRelatedBugs").click(function () {
        var resultID = $(SISelectedRow).attr("id");
        var resultLanguage = $(SISelectedRow).children(":eq(2)").text();
        var resultRuleName = $(SISelectedRow).children(":eq(3)").text();
        ShowRelatedBugs(resultID, resultRuleName, resultLanguage);
    })


    $("#MenuAttachBugID").click(function () {
        console.log("#MenuAttachBugID");
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
            beforeSend: function () {
                $('#ReproStepsDialog #LaunchedFrom').html('<img src="images/ajax-loader.gif"/>');
                $('#ReproStepsDialog #WindowHierarchy').html('<img src="images/ajax-loader.gif"/>');
                $('#ReproStepsDialog').dialog({ width: 'auto', height: 'auto', modal: true });
            },
            url: 'CaptureUIsService.asmx/GetUIInfo',
            contentType: "application/json;charset=utf-8",
            type: 'POST',
            data: "{resultID:'" + resultID + "'}",
            dataType: 'json',
            error: function (data) {
                console.log(data.responseText);
                $('#ReproStepsDialog').dialog('close');
                alert("Fail to get repro steps!");
            },
            success: function (data) {
                var uiInfo = data.d;
                var width = 500;
                var $txtLaunchedFrom = $('<input type="text" readonly="readonly" />')
                $txtLaunchedFrom.val(uiInfo.LaunchedFrom);
                var $txtWindowHierarchy = $('<input type="text" readonly="readonly" />')
                $txtWindowHierarchy.val(uiInfo.WindowHierarchy);
                $('#ReproStepsDialog #LaunchedFrom').html('').append($txtLaunchedFrom.width(width));
                $('#ReproStepsDialog #WindowHierarchy').html('').append($txtWindowHierarchy.width(width));
                $('#ReproStepsDialog').dialog('option', 'position', 'center');
            }
        });
    });
    $("#MenuShowAssembly").click(function () {
        var resultID = GetSelectedRowID();
        var assemblyInfo = ShowRelatedAssemblyInfo(resultID);
    });
    $("#MenuTestCoverageReport").click(function () {
        var documentWidth = $(document).width();
        var documentHeight = $(document).height();
        $('#ScreenLocker').css({ 'height': documentHeight }).fadeIn('fast');

        var resultID = $(SISelectedRow).attr('id');

        //show form
        //set rule as main report page settings
        $(SIRulesCondition).children().each(function (i, n) {
            if (n.className.indexOf("SelectedTag") != -1) {
                $($(SITestCoverageRulesCondition).children()[i]).addClass(SelectedTagClassName);
            } else {
                $($(SITestCoverageRulesCondition).children()[i]).removeClass(SelectedTagClassName);
            }
        });

        //set build language as main report page settings
        $(SIBuildLanguageCondition).children().each(function (i, n) {
            if (n.className.indexOf("SelectedTag") != -1) {
                $($(SITestCoverageBuildLanguagesCondition).children()[i]).addClass(SelectedTagClassName);
            } else {
                $($(SITestCoverageBuildLanguagesCondition).children()[i]).removeClass(SelectedTagClassName);
            }
        });

        //set build list
        $('#AboveBuildNoSelect').empty();
        var buildNoList;
        $.each(availableBuildNo, function (index, value) {
            if (index == 0) {
                buildNoList = "<option value='" + value + "' selected='selected'>" + value + "</option>";
            } else {
                buildNoList = "<option value='" + value + "'>" + value + "</option>";
            }
            $('#AboveBuildNoSelect').append(buildNoList);
        });
        $('#RelatedFullTypeSelect').empty();
        $('#RelatedAssemblySelect').empty();

        $.ajax({
            //beforeSend: function () {
            //    $('#ExportTestCoverageForm #RelatedAssemblySelect').html('<img src="images/ajax-loader.gif"/>');
            //    $('#ExportTestCoverageForm #RelatedAssemblySelect').html('<img src="images/ajax-loader.gif"/>');
            //    $('#ReproStepsDialog').dialog({ width: 'auto', height: 'auto', modal: true });
            //},
            url: 'CaptureUIsService.asmx/GetRelatedAssemblyDic',
            contentType: "application/json;charset=utf-8",
            type: 'POST',
            data: "{resultID:'" + resultID + "'}",
            dataType: 'json',
            error: function (data) {
                console.log(data.responseText);
                //$('#ExportTestCoverageForm').dialog('close');
                alert("Fail to call GetRelatedAssemblies!");
                $('#ScreenLocker').css({ 'height': documentHeight }).fadeOut('fast');
            },
            success: function (data) {
                if (data.d == null || data.d.length == 0) {
                    //$('#TestCoverageDialog').dialog('close');
                    alert("No related assemlby! Cannot continue to generate Test Coverage Report!");
                    $('#ScreenLocker').css({ 'height': documentHeight }).fadeOut('fast');
                    return;
                }
                relatedAssemblyAndFullTypeDic = data.d;

                var addedFirstAssemlby = false, addedFirstFullType = false;
                var relatedAssemlbyList, relatedFullTypeList;
                $('#btnExportTestCoverageOK').attr('disabled', true);
                for (var key in relatedAssemblyAndFullTypeDic) {
                    $('#btnExportTestCoverageOK').attr('disabled', false);
                    if (addedFirstAssemlby) {
                        relatedAssemlbyList = "<option value='" + key + "'>" + key + "</option>";
                    } else {
                        relatedAssemlbyList = "<option value='" + key + "' selected='selected'>" + key + "</option>";
                        addedFirstAssemlby = true;
                        for (var i = 0; i < relatedAssemblyAndFullTypeDic[key].length; i++) {
                            if (addedFirstFullType) {
                                relatedFullTypeList = "<option value='" + relatedAssemblyAndFullTypeDic[key][i] + "'>" + relatedAssemblyAndFullTypeDic[key][i] + "</option>";
                            } else {
                                relatedFullTypeList = "<option value='" + relatedAssemblyAndFullTypeDic[key][i] + "' selected='selected'>" + relatedAssemblyAndFullTypeDic[key][i] + "</option>";
                                addedFirstFullType = true;
                            }
                            $('#RelatedFullTypeSelect').append(relatedFullTypeList);
                        }
                    }
                    $('#RelatedAssemblySelect').append(relatedAssemlbyList);
                }

                var formWidth = $("#ExportTestCoverageForm").width();
                var formHeight = $("#ExportTestCoverageForm").height();
                $("#ExportTestCoverageForm").css({ "margin-top": 0 - formHeight / 2, "margin-left": 0 - formWidth / 2 }).fadeIn('fast');
                $('#AboveBuildNoSelect').focus();
            }
        });
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
        $.ajax({
            beforeSend: function () {
                var documentHeight = $(document).height();
                $('#ScreenLocker').css({ 'height': documentHeight }).fadeIn('fast');
                $("#divLoading").show();
            },
            url: 'CaptureUIsService.asmx/GeneratePDF',
            contentType: "application/json;charset=utf-8",
            type: 'POST',
            data: "{resultID:\"" + resultID + "\"}",
            dataType: 'json',
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
                var message = '';
                if (data.d == '')
                    message = 'Failed to generate PDF file!';
                else
                    message = 'Succeeded to generate PDF file-' + data.d;
                alert(message);
            }
        });
    });
    $("#MenuEditComments").click(function () {
        var resultid = $(SISelectedRow).attr("id");
        if ($('#Row_DetailInfo_' + resultid).find('#ReviewLog').text() == "") {
            $(GetRow_DetailInfo(resultid)).insertAfter($(SISelectedRow));
            showDetailInfo(resultid);
            ////ajax to get detail info
            //getDetailInfo(resultid);
            ////ajax to get reviewlog
            ////getReviewLog(resultid);
            //showReviewLog(resultid);
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
    $("#btnAttachBugOK").click(function () {
        console.log("btnAttachBugOK");
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

    $("#btnAttachBugCancel").click(function () {
        console.log("btnAttachBugCancel");
        if (!$('#AttachBugIDForm').is(':visible')) {
            return;
        }
        unloadAttachedBugIDForm();
    });
}

function validateBugIDFormInput(bugID) {
    console.log("validateBugIDFormInput");
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

function unloadAttachedBugIDForm() {
    console.log("unloadAttachedBugIDForm");
    //unload form
    $('#AttachBugIDForm').hide();
    //unlock screen
    $('#ScreenLocker').hide();
}

function attachBugID(bugIDs, resultID) {
    $.ajax({
        url: 'IndexService.asmx/LinkBug',
        contentType: "application/json;charset=utf-8",
        type: 'POST',
        data: "{resultID:\"" + resultID + "\",bugIDs:\"" + bugIDs + "\"}",
        dataType: 'json',
        error: function (data) {
            console.log(data);
            //                                        alert(data.responseText);
            alert("Fail to attach bug ID!");
        },
        success: function (data) {
            alert('Succeeded to attach bug!');
        }
    });
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
    //$(SIBtnQuery).click(function () {
    //    if ($("#groupbyname").is(":checked")) {
    //        alert("Disable the group function due to bug 481419");
    //        return;
    //    }
    //    searchDateTime = getNowFormatDate();
    //    DoQuery(0, 0);
    //});

    //$(SIBtnShowUIDiffResult).click(function () {
    //    $.ajax({
    //        url: 'CaptureUIsService.asmx/GetUIDiffSubDirectory',
    //        contentType: "application/json;charset=utf-8",
    //        type: 'POST',
    //        error: function (data) {
    //            alert("Fail to access file server - SCFS!");
    //        },
    //        success: function (data) {
    //            var documentWidth = $(document).width();
    //            var documentHeight = $(document).height();
    //            $('#ScreenLocker').css({ 'height': documentHeight }).fadeIn('fast');
    //            var UIDIffResultPath = data.d;
    //            for (var i = UIDIffResultPath.length - 1; i >= 0; i--) {
    //                $('#UIDiffPath').append("<option value='" + UIDiffResultPathBase + UIDIffResultPath[i] + UIDiffResultPathRelated + "'>" + UIDIffResultPath[i] + "</option");
    //            }
    //            var formWidth = $("#ShowUIDiffResultForm").width();
    //            var formHeight = $("#ShowUIDiffResultForm").height();
    //            $("#ShowUIDiffResultForm").css({ "margin-top": 0 - formHeight / 2, "margin-left": 0 - formWidth / 2 }).fadeIn('fast');
    //            $('#UIDiffPath').focus();
    //        }
    //    });
    //});

    $("#SetDefault").click(function () {
        SelectDefaultCondition();
    });

    $("#ClearAll").click(function () {
        $(".Tag").each(function () {
            $(this).removeClass(SelectedTagClassName);
        });
        RemoveAllConvinenceTags();
    });

    //$(SIExpandIconImage).toggle(function () {
    //    $(this).css({ "margin-left": "-19px" });
    //    $(SISearchBody).hide();
    //}, function () {
    //    $(this).css({ "margin-left": "-110px" });
    //    $(SISearchBody).show();
    //});

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

        var buildNo = $('#AboveBuildNoSelect').val().trim();
        var assemblyName = $('#RelatedAssemblySelect').val().trim();
        var fullTypeName = $('#RelatedFullTypeSelect').val().trim();
        var languages = GetSelectedValues(SISelectedTestCoverageBuildLanguageTags, "Please Select Build Languages.", $("#TestCoverage_BuildLanguage .Tag:gt(0)"));
        if (typeof languages == 'undefined') return;
        languages = languages.split('|').join(',');
        var rules = GetSelectedValues(SISelectedTestCoverageRuleTags, "Please Select Rules.", $("#SC_Rule .Tag:gt(0)"));
        if (typeof rules == 'undefined') return;
        rules = rules.split('|').join(',');
        for (var key in availableRules) {
            rules = rules.replace(availableRules[key].RuleName, availableRules[key].RuleID);
        }

        showTestCoverageResult(buildNo, languages, rules, assemblyName, fullTypeName);

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