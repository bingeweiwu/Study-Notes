<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="ProviderDiff.HomeN" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>SulpHur - Home</title>
    <script src="scripts/common.js" type="text/javascript"></script>
    <script type="text/javascript" src="jquery-1.8.2.js"></script>
    <link href="HomeStyle.css" rel="stylesheet" type="text/css" />
    <style type="text/css">
        body
        {
        }
        .YellowTile
        {
            background-color: #FFAA31;
        }
        .YellowTile1
        {
            background-color: #EF9C00;
        }
        .DeepYellowTile
        {
            background-color: #F67D20;
        }
        .GreenTile
        {
            background-color: #6CBE42;
        }
        .GreenTile1
        {
            background-color: #009E3D;
        }
        .BlueTile
        {
            background-color: #58BAE9;
        }
        .BlueTile1
        {
            background-color: #0082B6;
        }
        .BlueTile2
        {
            background-color: #03AEDA;
        }
        .LinkTile
        { 
            padding-top: 12px;
            padding-left: 9px;
            width: 115px;
            height: 115px;
            background-repeat: no-repeat;
            background-position: center;
            margin:1px;
        }
        .LinkTile:hover
        {
            cursor: pointer;
        }
        .TileContainer
        {
            padding: 0px;
            float: left;
            margin-left: 10px;
        }
        #logo
        {
            font-size: 25px;
            font-family: Segoe UI;
            margin-top: 50px;
            margin-left: 0px;
            float: left;
        }
        #nav_links
        {
            float: right;
        }
        .tileName
        {
            font-family: Segoe UI;
            font-size: small;
            color: White;
        }
        .hideText
        {
            display: none;
        }
        #explain
        {
            font-family: Segoe UI;
            color: #4d4d4d;
            font-size: 13px;
            padding-top: 15px;
            clear: both;
        }
        table
        {
            border: 0px;
            width: 100%;
        }
        tr td:first-child
        {
            text-align: right;
            width: 200px;
        }
        tr td:first-child + td + td
        {
            width: 150px;
        }
        tr td:first-child + td
        {
            color: #0063dc;
        }
        .SummaryTitle .dot
        {
            border: 1px dotted grey;
            width: 300px;
            height: 20px;
        }
        .SummaryTitle div
        {
            float: left;
        }
        .SummaryTitle .ttext
        {
            text-align: center;
            width: 200px;
        }
        #Results
        {
            margin-top: 10px;
        }
        .title
        {
            background-color: #4888F0;
            color: White;
            display: block;
            font-family: "nevis" , "Droid Arabic Kufi" , Helvetica, Georgia, serif;
            font-size: 12px;
            font-weight: normal;
            line-height: 1;
            margin-left: 0px;
            margin-right: 0px;
            margin-top: 10px;
            padding-bottom: 6px;
            padding-left: 10px;
            padding-right: 10px;
            padding-top: 6px;
            position: relative;
            text-transform: uppercase;
            border-bottom-width: 0px;
        }

        .arrow-s
        {
            border-style: dashed;
            border-color: transparent;
            border-width: 0.53em;
            display: -moz-inline-box;
            display: inline-block; /* Use font-size to control the size of the arrow. */
            font-size: 10px;
            height: 0;
            line-height: 0;
            position: relative;
            vertical-align: middle;
            width: 0;
        }
        .arrow-s
        {
            border-top-width: 1em;
            border-top-style: solid;
            border-top-color: #4888F0;
            top: -2px;
            left: 10px;
        }

    </style>
    <script type="text/javascript">
        $(document).ready(function () {
            //Navigations
            LinkTileAction();
            ConditonCollpaseAction();
            ShowMoreAction();
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
                            HidenChildren($parent);
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
                    }

                    HighLightSelected();
                    BindingQueryClick();
                    QueryClick();
                }
            });
            //CSS
            $("table tr td").css({ "border-bottom": "1px dotted grey" });
        });
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div id="page-border">
        <div id="Navigation">
            <div id="logo">
                SulpHur Report&nbsp;&nbsp;
            </div>
            <div id="nav_links">
                <div>
                    <div class="TileContainer" onclick="javascript:nav_targetblank('ProviderDiff.aspx')">
                        <div class="YellowTile1 LinkTile" style="background-image: url('img/diff.png'); background-size: 60px 60px;">
                            <span class="tileName">Provider Diff</span> <span class="hideText">Smsprovider diff
                                report between builds and impacted UIs</span>
                        </div>
                    </div>
                    <!--
                    <div class="TileContainer" onclick="javascript:nav_targetblank('http://172.22.92.173:81/')">
                        <div class="GreenTile1 LinkTile">
                            <span class="tileName">SQM Tool</span> <span class="hideText"></span>
                        </div>
                    </div>-->
                    <div class="TileContainer" onclick="javascript:nav_targetblank('http://SulpHurServer14.redmond.corp.microsoft.com/SulpHurReports/CapturedUIReport.aspx')">
                        <div class="BlueTile2 LinkTile" style="background-image: url('img/captured.png');
                            background-size: 60px 60px;">
                            <span class="tileName">Captured UIs</span> <span class="hideText">Query captured UIs
                                in SulpHur DB</span>
                        </div>
                    </div>
                    <div class="TileContainer" onclick="javascript:nav_targetblank('https://microsoft.sharepoint.com/teams/ConfigMgr/Wiki%20Pages/SulpHur%20User%20Manual.aspx')">
                        <div class="YellowTile LinkTile">
                            <span class="tileName">Manual</span> <span class="hideText">Wiki to help you use SulpHur
                                tool</span>
                        </div>
                    </div>
                    <!--
                    <div class="TileContainer" onclick="javascript:nav_targetblank('http://SulpHurServer14/SulpHurClient')">
                        <div class="GreenTile1 LinkTile" style="background-image: url('img/client.png');
                            background-size: 60px 60px;">
                            <span class="tileName">Client</span> <span class="hideText">Download and install SulpHur
                                client</span>
                        </div>
                    </div>-->
                    <div class="TileContainer" onclick="javascript:nav_targetblank('MissPageDetails.aspx')">
                        <div class="GreenTile LinkTile">
                            <span class="tileName">Missed UIs</span> <span class="hideText">Review UIs in console
                                binaries but not captured</span>
                        </div>
                    </div>
                    <div class="TileContainer" onclick="javascript:nav_targetblank('mailto:lixuan@microsoft.com')">
                        <div class="DeepYellowTile LinkTile" style="background-image: url('img/mail.png');
                            background-size: 60px 60px;">
                            <span class="tileName">Mail</span> <span class="hideText">lixuan for any issues with
                                SulpHur</span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div id="explain" style="height: 30px;">
        </div>
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
                    <table>
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
                    </table>
                </div>
                <div id="btnInput">
                    <input class="modern" type="button" value="Query" id="btnQuery" name="btnQuery" />
                </div>
            </div>
        </div>
        <div id="Results">
            <div id="CSummary">
                <span class="title">Captured UI Summary </span><span class="arrow-s"></span>
                <div>
                    <div class="Message">
                        Click Query to show summary
                    </div>
                    <div class="sumImg">
                    </div>
                </div>
            </div>
            <div id="MSummary">
                <span class="title">Miss Pages Summary </span><span class="arrow-s"></span>
                <div>
                    <div class="Message">
                        Click Query to show summary
                    </div>
                    <div class="sumImg">
                    </div>
                </div>
            </div>
            <div>
            </div>
        </div>
    </div>
    </form>
</body>
</html>
