<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="ProviderDiff.Home" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
    <link href="CMMetro.css" rel="stylesheet" type="text/css" />
    <script type="text/javascript" src="http://ajax.microsoft.com/ajax/jQuery/jquery-1.4.1.js"></script>
    <script type="text/javascript" src="http://ajax.microsoft.com/ajax/jQuery/jquery-1.4.1-vsdoc.js"></script>
    <script type="text/javascript">
        $.ajaxSetup({
            cache: false
        });

        $(document).ready(function () {
        });

        function nav_targetblank(link) {
            window.open(link);
        }

        function MoveLogo() {
            $("#logo").animate({ top: "30px" }, 1000);
        }

        function QueryRBuild() {
            $.ajax({
                url: 'AjaxQuery.aspx',
                type: 'POST',
                data: {
                    IsAjax: true,
                    Method: 'QueryProductBuild'
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

                    var x = document.getElementById("rbuild");
                    x.options.length = 0;
                    for (i = 0; i < arrData.length; i++) {
                        var option = document.createElement("option");
                        option.text = arrData[i];
                        option.value = arrData[i];
                        try {
                            // for IE earlier than version 8
                            x.add(option, x.options[null]);
                        }
                        catch (e) {
                            x.add(option, null);
                        }
                    }

                    QueryData();
                }
            });
        }

        function QuerySBuild() {
            $.ajax({
                url: 'AjaxQuery.aspx',
                type: 'POST',
                data: {
                    IsAjax: true,
                    Method: 'QueryCapturedBuild'
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

                    var x = document.getElementById("sbuild");
                    x.options.length = 0;
                    var optionAll = document.createElement("option");
                    optionAll.text = "All";
                    optionAll.value = "All";
                    try {
                        // for IE earlier than version 8
                        x.add(optionAll, x.options[null]);
                    }
                    catch (e) {
                        x.add(optionAll, null);
                    }
                    optionAll.selected = "selected";
                    for (i = 0; i < arrData.length; i++) {
                        var option = document.createElement("option");
                        option.text = arrData[i];
                        option.value = arrData[i];
                        try {
                            // for IE earlier than version 8
                            x.add(option, x.options[null]);
                        }
                        catch (e) {
                            x.add(option, null);
                        }
                    }
                    QueryLan();
                }
            });
        }

        function QueryLan() {
            var SelectedBuild = $("#sbuild option:selected").text();
            $.ajax({
                url: 'AjaxQuery.aspx',
                type: 'POST',
                data: {
                    IsAjax: true,
                    Method: 'QueryBuildLan',
                    SelectedBuild: SelectedBuild
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

                    var x = document.getElementById("lan");
                    x.options.length = 0;
                    var optionAll = document.createElement("option");
                    optionAll.text = "All";
                    optionAll.value = "All";

                    try {
                        // for IE earlier than version 8
                        x.add(optionAll, x.options[null]);
                    }
                    catch (e) {
                        x.add(optionAll, null);
                    }
                    optionAll.selected = "selected";
                    for (i = 0; i < arrData.length; i++) {
                        var option = document.createElement("option");
                        option.text = arrData[i];
                        option.value = arrData[i];
                        try {
                            // for IE earlier than version 8
                            x.add(option, x.options[null]);
                        }
                        catch (e) {
                            x.add(option, null);
                        }
                    }
                }
            });
        }

        function QueryAll() {
            QueryRBuild();
            QuerySBuild();
        }

        function QueryData() {
            var ReferencedBuild = $("#rbuild option:selected").text();
            var SelectedBuild = $("#sbuild option:selected").text();
            if (SelectedBuild == "") SelectedBuild = "All";
            var Lan = $("#lan option:selected").text();
            if (Lan == "") Lan = "All";
            $.ajax({
                beforeSend: function () {
                    $('#totalpagesummary').html('<img src="img/loading.gif"/>');
                    $('#misspageimg').html('<img src="img/loading.gif"/>');
                },
                url: 'AjaxQuery.aspx',
                type: 'POST',
                data: {
                    IsAjax: true,
                    Method: 'QueryPagesSummaryData',
                    ReferencedBuild: ReferencedBuild,
                    SelectedBuild: SelectedBuild,
                    Lan: Lan
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
                    var img1 = arrData[0] + '?' + (new Date()).getTime();
                    var img2 = arrData[1] + '?' + (new Date()).getTime();

                    $('#totalpagesummary').html('<a href="MissPages.aspx"><img  height="400px" style="border:none" width="400px" src="' + img1 + '" alt="Total Pages Summary"/></a>');
                    $('#misspageimg').html('<a href="MissPageView.aspx"><img height="400px" style="border:none" width="400px" src="' + img2 + '" alt="Miss Pages Summary"/></a>');
                }
            });
        }

        function MissPagesIn() {
            QueryAll();
            MoveLogo();
            setTimeout(MoveOutMainLink, 1000);
            setTimeout(MoveInMissPage, 1500);
        }

        function MoveOutMainLink() {
            $("#mainlink").animate({ 'left': $('#logo').width() }, 500);
        }

        function MoveInMissPage() {
            $("#MissPagesSummary").css('left', -$('#logo').width());
            $("#MissPagesSummary").show();
            $("#MissPagesSummary").animate({ 'left': 0 }, 500);
        }

        function ClearLan() {
            var x = document.getElementById("lan");
            x.options.length = 0;
            var optionAll = document.createElement("option");
            optionAll.text = "All";
            optionAll.value = "All";

            try {
                // for IE earlier than version 8
                x.add(optionAll, x.options[null]);
            }
            catch (e) {
                x.add(optionAll, null);
            }
            optionAll.selected = "selected";
        }

        function RBuildChange() {
            QueryData();
        }
        function SBuildChange() {
            ClearLan();
            QueryLan();
            QueryData();
        }
        function LanChange() {
            QueryData();
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div id="topdiv">
        <a style="margin-left: 20px;" href="Home.aspx">
            <img src="img/MB_0023_home2 (2).png" onmouseover="this.src='img/MB_0023_home2.png'"
                onmouseout="this.src='img/MB_0023_home2 (2).png'" height="30" style="border: none;"
                alt="Home" /></a>
    </div>
    <div id="bottomdiv">
    </div>
    <div id="logo">
        <a href="Home.aspx">
            <img src="img/sulphur.JPG" id="logotext" alt="SulpHur" style="border: none;" /></a>
    </div>
    <div id="mainlink">
        <div class="metro metro-azul metrosingle " onclick="javascript:nav_targetblank('http://172.22.92.173/SulpHurReports/Report.aspx')">
            <div class='imgsimple'>
            </div>
            <span>SulpHur Report</span>
        </div>
        <div class="metro metro-vermelho metrosingle " onclick="javascript:nav_targetblank('http://172.22.92.173/SulpHur/ProviderDiff.aspx')">
            <div class='imgsimple'>
            </div>
            <span>Provider Diff Report</span>
        </div>
        <div class="metro metro-roxo metrosingle " onclick="javascript:MissPagesIn()">
            <div class='imgsimple'>
            </div>
            <span>Miss Pages Report</span>
        </div>
    </div>
    <div id="MissPagesSummary">
        <div id="selectcontainer">
            <div>
                <label>
                    Reference Build:</label>
                <select id="rbuild" onchange="javascript:RBuildChange()">
                </select></div>
            <div style="margin-left: 20px;">
                <label>
                    Selected Build:</label>
                <select id="sbuild" onchange="javascript:SBuildChange()">
                </select></div>
            <div style="margin-left: 20px;">
                <label>
                    Language:</label>
                <select id="lan" onchange="javascript:LanChange()">
                </select></div>
        </div>
        <div id="charContainer">
            <div class="metrobig metro-azul">
                <div class="chartsimple">
                    <div id="totalpagesummary">
                    </div>
                </div>
                <span>Total Pages Summary</span>
            </div>
            <div class="metrobig metro-azul">
                <div class="chartsimple">
                    <div id="misspageimg">
                    </div>
                </div>
                <span>Miss Pages Summary</span>
            </div>
        </div>
        <div id="uponbottomdiv">
        <input type="button" value="Mark Details" onclick="javascript:nav_targetblank('MissPages.aspx')" />
        <input type="button" value="View Details" onclick="javascript:nav_targetblank('MissPageView.aspx')" />
        </div>
    </div>
    </form>
</body>
</html>
