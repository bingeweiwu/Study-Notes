<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MissPageDetails.aspx.cs"
    Inherits="ProviderDiff.MissPageDetails" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Miss Page Details</title>
    <style type="text/css">
        .even
        {
            background: #E3EAEB;
        }
        .odd
        {
            background: white;
        }
        .selectedtr
        {
            background: #4888F0;
            color: White;
        }
        #ConditionTable
        {
            border: 0px;
            width: 100%;
        }
        #ConditionTable tr td:first-child
        {
            text-align: right;
            width: 200px;
        }
        #ConditionTable tr td:first-child + td + td
        {
            width: 150px;
        }
        #ConditionTable tr td:first-child + td
        {
            color: #0063dc;
        }
        #tableContainer
        {
            margin-top: 10px;
            border: 1px solid grey;
            padding: 5px;
            height:500px;
            overflow:auto;
        }
        #HomeLink
        {
            font-family: "Open Sans" , "sans-serif";
            margin-bottom: 5px;
            background-color:rgb(240, 240, 240);
            color:blue;
            height:30px;
            border:1px solid grey;
            margin:0 auto;
            padding-top:10px;
            padding-left:40px;
            /*border-bottom: 1px solid grey;*/
        }
        #HomeLink a
        {
            text-decoration: none;
        }
        #tableContainer table tr
        {
        }
        #tableContainer table thead
        {
            height: 25px;
        }
        #EditDetails
        {
            display: none;
            background: white;
            border: 8px solid grey;
            width:800px;
            max-height:600px;
        }
        #EidtMarkTitle
        {
            background-color: #4888F0;
            height: 30px;
            padding:3px;
        }
        #EidtMarkTitle > div:first-child
        {
            float: left;
        }
        #EidtMarkClose
        {
            float: right;
            width:30px;
            font-weight:bold;
            cursor:pointer;
            font-size:26px;
            color:White;
        }
        #EditDetailsBody
        {
            padding: 10px;
            width:780px;
            max-height:540px;
            overflow:auto;
        }
        .break-word
        {
             width:600px;
             word-wrap:break-word;
            }
                    .break-word1
        {
             width:250px;
             word-wrap:break-word;
            }
        .loadingImg
        {
             width:20px;
             height:20px;
            }
    </style>
    <link href="HomeStyle.css" rel="stylesheet" type="text/css" />
    <script type="text/javascript" src="jquery-1.8.2.js"></script>
    <script src="scripts/common.js" type="text/javascript"></script>
    <script type="text/javascript" src="jquery.bpopup-0.7.0.min.js"></script>
    <script type="text/javascript">
        $(function () {
            var assemblyName;
            var fullTypeName;
            var isCaptured;

            ConditonCollpaseAction();
            ShowMoreAction();
            $("table tr td:not(#EditDetailsBody td,#tableContainer td)").css({ "border-bottom": "1px dotted grey" });

            //Load Conditions
            $.ajax({
                type: "POST",
                url: "HomePageService.asmx/QueryAvailableData",
                data: "{}",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                error: function (data) {
                    alert("Ajax request error!");
                },
                success: function (msg) {
                    for (var key in msg.d.hash) {
                        var arrayValue = msg.d.hash[key].split(',');

                        if (key == "AvailableProductBuilds") {
                            var $parent = $("#BinariesBuilds");
                            for (i = 0; i < arrayValue.length; i++) {
                                AddOption($parent, arrayValue[i]);
                            }
                        }
                        if (key == "AvailableCapturedBuilds") {
                            var $parent = $("#CapturedBuilds");
                            for (i = 0; i < arrayValue.length; i++) {
                                AddOption($parent, arrayValue[i]);
                            }

                            $parent.next().find(".showmore").show();
                            HidenChildren($parent)
                        }
                        if (key == "AvailableCapturedLanguages") {
                            var $parent = $("#CapturedLanguage");
                            for (i = 0; i < arrayValue.length; i++) {
                                AddOption($parent, arrayValue[i]);
                            }
                        }
                        if (key == "AvailableOSTypes") {
                            var $parent = $("#CapturedOSType");
                            for (i = 0; i < arrayValue.length; i++) {
                                AddOption($parent, arrayValue[i]);
                            }
                        }
                        if (key == "AvailableAssembly") {
                            var $parent = $("#CAssembly");
                            for (i = 0; i < arrayValue.length; i++) {
                                AddOption($parent, arrayValue[i]);
                            }
                            HidenChildren($parent);
                        }
                    }

                    HighLightSelected();
                    QueryClick1();
                }
            });
            $('#showScreenshot').click(function () {
                var $screenshot = $('#screenshot');
                var $showScreenshot = $('#showScreenshot');
                // do not response when the link is disabled
                if ($showScreenshot.attr('disabled')) {
                    return false;
                }

                // toggle to display screenshot
                if ($showScreenshot.text().indexOf('Show') > -1) {
                    $screenshot.attr('src', 'img/ajax-loader.gif');
                    $screenshot.attr('src', 'Image.aspx?ImageID=' + $screenshot.attr('ContentID'));
                    $screenshot.show();
                    $showScreenshot.text('Hide Screenshot');
                } else {
                    $screenshot.hide();
                    $showScreenshot.text('Show Screenshot');
                }
            });
        });
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true">
    </asp:ScriptManager>
    <div id="HomeLink">
        <a href="javascript:nav_targetblank('Index.aspx')">HOME</a> > <span>Miss Pages Details</span>
    </div>
    <div id="page-border">
        <div id="SearchCondition">
            <div id="SearchTitle">
                <div class="ec_icon">
                    <img src="img/icons1.jpg" alt="Collpase" style="margin-top: -11px; margin-left: -110px;" />
                </div>
                <p>
                    &nbsp; &nbsp; Select query conditions</p>
                                     <a id="ClearAll" href="#">Clear All</a>
                 <a id="SetDefault" href="#">Default</a>
            </div>
            <div id="SearchBody">
                <div id="selectedCondition">
                </div>
                <div id="selection">
                    <table id="ConditionTable">
                        <tr>
                            <td>
                                Binaries based on build:
                            </td>
                            <td id="BinariesBuilds">
                                <span class="markvalue">Latest</span>
                            </td>
                            <td>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                UI captured in Language:
                            </td>
                            <td id="CapturedLanguage">
                                <span class="markvalue">All</span>
                            </td>
                            <td>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                UI captured in OS Type:
                            </td>
                            <td id="CapturedOSType">
                                <span class="markvalue">All</span>
                            </td>
                            <td>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                Assembly:
                            </td>
                            <td id="CAssembly">
                                <span class="markvalue">All</span>
                            </td>
                            <td>
                                <div class="showmore">
                                    <img src="img/arrow1.jpg" alt="" /><span>Show More</span></div>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                UI captured on builds:
                            </td>
                            <td id="CapturedBuilds">
                                <span class="markvalue">All</span> <span class="markvalue">Latest</span>
                            </td>
                            <td>
                                <div class="showmore">
                                    <img src="img/arrow1.jpg" alt="" /><span>Show More</span></div>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                Categories:
                            </td>
                            <td id="CCategories">
                                <span class="markvalue">All</span> 
                                <span class="markvalue">Valid UI (Missing Capture)</span>
                                <span class="markvalue">Embedded User Control</span> 
                                <span class="markvalue">Captured In Other Builds/Language/OS</span> 
                                <span class="markvalue">Obsolete (Dead Codes)</span>
                                <span class="markvalue">Internal Tool</span> 
                                <span class="markvalue">Base and Place-Holder</span>
                                <span class="markvalue">Other (In Review)</span> 
                                <span class="markvalue">Captured</span>
                            </td>
                            <td>
                            </td>
                        </tr>
                    </table>
                </div>
                <div id="btnInput">
                    <input class="modern" type="button" value="Query" id="btnQuery" name="btnQuery" />
                </div>
            </div>
        </div>
        <div id="tableContainer">
            <div id="tableInfo">
            </div>
            <div>
                        <table style=" width:100%">
                <thead style="background: #4888F0; color: White;">
                    <tr>
                        <th>
                            SN
                        </th>
                        <th>
                            Assembly Name
                        </th>
                        <th>
                            Type Name
                        </th>
                        <th>
                            Mark
                        </th>
                        <th>
                        </th>
                    </tr>
                </thead>
                <tbody id="gvDetails">
                </tbody>
            </table>
            </div>

            <div style="margin-top: 10px;">
                <div class="Message">
                    Click Query to show results
                </div>
            </div>
        </div>
    </div>
    <div id="EditDetails">
        <div id="EidtMarkTitle">
            <div>
                <img src="img/edit.png" alt="" width="30px" height="30px" /></div>
            <div id="EidtMarkClose">
                <img src="img/close_icon.png" alt="" width="26" height="26px" />
                </div>
        </div>
        <div id="EditDetailsBody">
            <table>
                <tr>
                    <td>
                        Assembly Name:
                    </td>
                    <td id="DetailsAssemblyName">
                    </td>
                </tr>
                <tr>
                    <td>
                        FullType Name:
                    </td>
                    <td id="DetailsFullTypeName">
                    </td>
                </tr>
                <tr>
                    <td>
                        Mark:
                    </td>
                    <td>
                        <select id="editCategory">
                            <option>Valid UI (Missing Capture)</option>
                            <option>Embedded User Control</option>
                            <option>Obsolete (Dead Codes)</option>
                            <option>Internal Tool</option>
                            <option>Base and Place-Holder</option>
                            <option>Other (In Review)</option>
                        </select>
                        <p id="capureMark">
                            Captured</p>
                    </td>
                </tr>
                <tr>
                    <td>
                        LaunchedFrom:
                    </td>
                    <td id="launchedFrom">
                    </td>
                </tr>
                <tr>
                    <td>
                        WindowHierarchy:
                    </td>
                    <td id="windowHierarchy">
                    </td>
                </tr>
                <tr>
                    <td colspan="2" align="left">
                        <a href="javascript:return false;" id="showScreenshot">Show Screenshot</a>
                        <br />
                        <img id="screenshot" src="" alt="" disabled="disabled" style="display: none;" />
                    </td>
                </tr>
                <tr>
                    <td>
                        Lanuch Steps
                    </td>
                    <td>
                        <textarea id="lanuchSteps" rows="200" cols="400" style="width: 400px; height: 100px;"></textarea>
                    </td>
                </tr>
                <tr>
                    <td>
                        <input type="button" value="Save" class="modern" id="btnSave" />
                    </td>
                    <td>
                        <div id="SaveStatus">
                        </div>
                    </td>
                </tr>
            </table>
        </div>
    </div>
    <div id="quickView"></div>
    </form>
</body>
</html>
