<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="IndexOld.aspx.cs" Inherits="ProviderDiff.HomeN" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>SulpHur - Home</title>
    <style type="text/css">
               .modern{
            width: auto;
            text-align:center;
            width:100px;
            height:28px;
            background: #617798;
            border: 0;
            font-size:11px;
            color: #FFFFFF;}
        body
        {
        }
        #divContainer
        {
            margin-top: 0px;
            margin-left: 0px;
            width: 100%;
            height: 800px;
        }
        #divLeft
        {
            border-width: 1px;
            border-style: solid;
            border-color: #99CCFF;
            width: 860px;
            float: left;
        }
        
        #divRight
        {
            float: left;
            width: 160px;
            margin-left: 10px;
        }
        #divTitle
        {
            height: 30px;
            padding-left: 20px;
            padding-top: 10px;
            background-color: #99CCFF;
            color: White;
            font-family:Segoe UI;
            background-image:url('img/bac1.jpg');
            font-weight:bold;
        }
        #divSummary1
        {
            clear: both;
            width: 860px;
        }
        #divSummary2
        {
            height: 500px;
            width: 855px;
            clear: both;
            border-top-width: 1px;
            border-top-style: solid;
            border-top-color: #99CCFF;
            color:#6699CC;
            text-align:center;
            vertical-align:middle;
        }
        #divSummary2 p
        {
             margin-top:200px;
             font-size:x-large;
            }
                  #divSummary2 .loading
        {
             margin-top:200px;
            }  
        #divInput
        {
            height: 250px;
            width: 420px;
            float: left;
            border-right-width: 1px;
            border-right-style: solid;
            border-right-color: #99CCFF;
        }
        #divSummaryPie
        {
            height: 250px;
            width: 435px;
            float: right;
            color:#6699CC;
            text-align:center;
            vertical-align:middle;
        }
        #divSummaryPie p
        {
             margin-top:100px;
            }
                    #divSummaryPie .loading
        {
             margin-top:100px;
            }
        #divRight div
        {
            width: 140px;
            height: 90px;
            margin-bottom: 10px;
            padding: 10px;
        }
        #divRight div:hover
        {
            filter: alpha(opacity=80);
            opacity: 0.8;
            cursor:pointer;
        }
        #divInput tr
        {
            height: 20px;
        }
        #divInput select
        {
            float: left;
            width: 100px;
        }
        #divInput img
        {
            float: left;
        }
        #capturedBuild
        {
        }
    </style>
    <link href="HomeStyle.css" rel="stylesheet" type="text/css" />
    <script type="text/javascript" src="jquery-1.8.2.js"></script>
    <script type="text/javascript" src="JScriptHome.js"></script>
    <script type="text/javascript">
        $(document).ready(function () {
            var baseBuildSelect = document.getElementById("baseBuild");
            var captureBuildSelect = document.getElementById("capturedBuild");
            var captureLanguageSelect = document.getElementById("capturedLanguage");
            var captureOSTypeSelect = document.getElementById("capturedOSType");
            var captureLanguageSelected = document.getElementById("capturedLanguageSelected");

            SetAvailableProductBuild(baseBuildSelect);
            SetAvailableCapturedBuild(captureBuildSelect);
            SetAvailableLanguage(captureLanguageSelect);
            SetAvailableOSType(captureOSTypeSelect);

            $('#btnQuery').click(function () {
                SelectChange();
            });

            $('#AddCapturedBuild').click(function () {
                var $options = $('#capturedBuild option:selected');
                $options.appendTo('#capturedBuildSelected');
            });

            $('#AddAllCapturedBuild').click(function () {
                var $options = $('#capturedBuild option');
                $options.appendTo('#capturedBuildSelected');
            });

            $('#capturedBuild').dblclick(function () {
                var $options = $("option:selected", this);
                $options.appendTo('#capturedBuildSelected');
            });

            $('#RemoveCapturedBuild').click(function () {
                var $options = $('#capturedBuildSelected option:selected');
                $options.appendTo('#capturedBuild');
            });

            $('#RemoveAllCapturedBuild').click(function () {
                var $options = $('#capturedBuildSelected option');
                $options.appendTo('#capturedBuild');
            });

            $('#capturedBuildSelected').dblclick(function () {
                var $options = $("option:selected", this);
                $options.appendTo('#capturedBuild');
            });

            function SelectChange() {
                var arrayBuild = new Array();
                $('#capturedBuildSelected option').each(function () {
                    arrayBuild.push($(this).val());
                });

                if (arrayBuild.length == 0) {
                    $("form select.required").each(function () {
                        $(this).css({ border: "1px solid #f00" });
                    });
                    return;
                } else {
                    $("form select.required").each(function () {
                        $(this).css({ border: "1px solid #000" });
                    });
                }

                var ReferencedBuild = $("#baseBuild option:selected").text();
                var language = $("#capturedLanguage option:selected").text();
                var osType = $("#capturedOSType option:selected").text();
                QuerySummary(osType, arrayBuild, language, ReferencedBuild, '#divSummaryPie', '#divSummary2');
            }

        });


    </script>
</head>
<body>
    <form id="form1" runat="server">


    <div id="divContainer">
        <div id="divLeft">
            <div id="divTitle">
                Report
            </div>
            <div id="divSummary1">
                <div id="divInput">
                    <table>
                        <tr>
                            <td>
                                Binaries based on build:
                            </td>
                            <td>
                                <select id="baseBuild">
                                </select>
                                <img class="loadingImg" src="img/loading.gif" alt="loading..." />
                            </td>
                        </tr>
                        <tr>
                            <td>
                                UI captured in Language:
                            </td>
                            <td>
                                        <select   id="capturedLanguage">
                                        </select><img class="loadingImg" src="img/loading.gif" alt="loading..." />
                            </td>
                        </tr>
                                                <tr>
                            <td>
                                UI captured in OS Type:
                            </td>
                            <td>
                                        <select   id="capturedOSType">
                                        </select><img class="loadingImg" src="img/loading.gif" alt="loading..." />
                            </td>
                        </tr>
                        <tr>
                            <td>
                                UI captured on builds:
                            </td>
                            <td>
                                <div class="multiselect">
                                    <div class="multiselectLeft">
                                        <select multiple="multiple" class="cBuild" id="capturedBuild" style="height: 100px;">
                                        </select></div>
                                    <div class="multiselectCenter">
                                        <div>
                                            <input type="button" value=">|" id="AddCapturedBuild" /><br />
                                            <input type="button" value=">>" id="AddAllCapturedBuild" /><br />
                                            <input type="button" value="<<" id="RemoveAllCapturedBuild" /><br />
                                            <input type="button" value="|<" id="RemoveCapturedBuild" /><br />
                                        </div>
                                    </div>
                                    <div class="multiselectRight">
                                        <select multiple="multiple" id="capturedBuildSelected" class="cBuild required" style="height: 100px;">
                                        </select>
                                    </div>
                                </div>
                            </td>
                        </tr>
                        <tr>
                            <td><input class="modern" type="button" value="Query" id="btnQuery" name="btnQuery" /></td>
                            <td></td>
                        </tr>
                    </table>
                </div>
                <div id="divSummaryPie">
                   <p> Show a Summary Of Miss Pages.</p>
                </div>
            </div>
            <div id="divSummary2">
               <p> Show a Summary of Captured UI Results.</p>
            </div>
        </div>
        <div id="divRight">
            <div style="background-color: #66CC99;" onclick="javascript:nav_targetblank('ProviderDiff.aspx')">
                <span class="bigText">Provider Diff</span><br />
                <span class="samllText">Smsprovider diff report between builds and impacted UIs</span>
            </div>
            <div style="background-color: #CCCCFF;" onclick="javascript:nav_targetblank('http://172.22.92.173/SulpHurReports/Report.aspx')">
                <span class="bigText">Captured UIs</span><br />
                <span class="samllText">Query captured UIs in SulpHur DB</span>
            </div>
            <div style="background-color: #99CCCC;" onclick="javascript:nav_targetblank('MissPageDetails.aspx')">
                <span class="bigText">Missed UIs</span><br />
                <span class="samllText">Review UIs in console binaries but not captured</span>
            </div>
            <div style="background-color: #99CCFF;" onclick="javascript:nav_targetblank('http://sharepoint/sites/ConfigMgr/Wiki%20Pages/SulpHur%20User%20Manual.aspx')">
                <span class="bigText">Manual</span><br />
                <span class="samllText">Wiki to help you use SulpHur tool</span>
            </div>
            <div style="background-color: #CCCCFF;" onclick="javascript:nav_targetblank('http://172.22.92.173/SulpHurClient')">
                <span class="bigText">Client</span><br />
                <span class="samllText">Download and install SulpHur client</span>
            </div>
            <div style="background-color: #6699CC;" onclick="javascript:nav_targetblank('mailto:taoctrs@microsoft.com')">
                <span class="bigText">Mail</span>
                <br />
                <span class="samllText">taoctrs for any issues with SulpHur</span>
            </div>
        </div>
    </div>
    </form>
</body>
</html>
