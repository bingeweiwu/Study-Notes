<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SulphurAdmin.aspx.cs" Inherits="SulpHurManagementSystem.SulphurAdmin" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Sulphur Administration Panel</title>
    <!-- Jquery Lib -->
    <script type="text/javascript" src="Scripts/jquery-1.8.2.js"></script>
    <script type="text/javascript" src="Scripts/jquery-ui-1.8.23.min.js"></script>
    <!-- Tabs extention -->
    <link rel="stylesheet" href="Scripts/Tabs/jquery-ui.css" />
    <script type="text/javascript" src="Scripts/Tabs/jquery-1.9.1.js"></script>
    <script type="text/javascript" src="Scripts/Tabs/jquery-ui.js"></script>
    <!-- Custom files -->
    <link href="styles/ThemeAdminPage.css" rel="stylesheet" type="text/css" />
    <script type="text/javascript" src="Scripts/Sulphur.js"></script>
    <script type="text/javascript">
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div class="logowrapper">
        <div class="logo">
            <h1>
                SulpHur Admin</h1>
        </div>
    </div>
    <div class="navigationwrapper">
        <div class="navigation">
            <ul>
                <li><a href="#f1" class="active">Service Config</a></li>
                <li><a href="#f2">UI Management</a></li>
                <li><a href="#f3">Recover UI</a></li>
                <li><a href="#f4">Page Types</a></li>
            </ul>
        </div>
    </div>
    <div class="wrapper">
        <!-- Navigation -->
        <div class="clear">
        </div>
        <div class="content">
            <div id="f1">
                <div class="msgLine">
                    <span class="configMsg">
                        <img src="images/check.gif" alt="check" class="icon" />
                        <b></b></span>
                </div>
                <div class="line">
                </div>
                <div id="f1main">
                    <table>
                        <tr>
                            <td>
                                <b>Log Level:</b>
                            </td>
                            <td>
                                <div class="divdrop">
                                    <div class="dropfirst">
                                        <span class="dropselectedoption" id="ccLog">Error</span> <span class="arrow">&or;</span>
                                        <div class="dropthird logAction">
                                            <ul>
                                                <li>Off</li>
                                                <li>Error</li>
                                                <li>Warning</li>
                                                <li>Verbose</li>
                                                <li>Info</li>
                                            </ul>
                                        </div>
                                    </div>
                                </div>
                            </td>
                        </tr>
                        <tr>
                            <td>
                            </td>
                            <td>
                            </td>
                        </tr>
                    </table>
                </div>
            </div>
            <div id="f2">
                <div class="in">
                    <div class="divdrop">
                        <div class="dropfirst">
                            <span class="dropselectedoption" id="cc1">Build No</span> <span class="arrow">&or;</span>
                            <div class="dropthird catogoryAction">
                                <ul>
                                    <li>Build No</li>
                                    <li>Content ID</li>
                                    <li>Result ID</li>
                                </ul>
                            </div>
                        </div>
                    </div>
                    <div class="divinput">
                        <div class="divdrop input1">
                            <div class="dropfirst">
                                <span class="dropselectedoption" id="cc2">Not Selected</span> <span class="arrow">&or;</span>
                                <div class="dropthird">
                                    <ul id="availableBuildNo" class="threecol1">
                                    </ul>
                                </div>
                            </div>
                        </div>
                        <input type="text" class="input2" id="f2input2"/>
                        <input type="text" class="input3" id="f2input3" />
                    </div>
                </div>
                <div class="f2main">
                    <table width="1022px" border="0" cellspacing="0" cellpadding="10" class="table_main">
                        <tbody>
                            <tr id="mainTitle" style="background-color: #d9d8d8; font-size: 14px; height: 30px;">
                                <td width="100px">
                                    <strong>ID</strong>
                                </td>
                                <td width="400px">
                                    <strong>Page Title</strong>
                                </td>
                                <td width="150px">
                                    <strong>User Name</strong>
                                </td>
                                <td width="300px">
                                    <strong>DO IT</strong>
                                </td>
                            </tr>
                            <tr class="nodataline">
                                <td colspan="4" style="text-align: center;">
                                    <span class="msg msgStyle">
                                        <img src="images/x.gif" alt="check" class="icon" style="float: left" />
                                        <b>No data available, please select the query condition!</b></span>
                                </td>
                            </tr>
                            <tr style="background-color: #d9d8d8; font-size: 10px; height: 20px;">
                                <td colspan="4">
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
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <div class="inbottom">
                    <input name="rescanAll" type="button" id="rescanAll" class="com_btn" value="Rescan All" />
                    <input name="deleteAll" type="button" id="deleteAll" class="com_btn" value="Delete All" />
                    <input name="recoverAll" type="button" id="recoverAll" class="com_btn" value="Recover All" />
                </div>
            </div>
            <div id="f3">
                <div class="in">
                    <div class="divdrop">
                        <div class="dropfirst">
                            <span class="dropselectedoption" id="f3cc1">Build No</span> <span class="arrow">&or;</span>
                            <div class="dropthird catogoryAction">
                                <ul>
                                    <li>Build No</li>
                                    <li>Content ID</li>
                                </ul>
                            </div>
                        </div>
                    </div>
                    <div class="divinput">
                        <div class="divdrop input1">
                            <div class="dropfirst">
                                <span class="dropselectedoption" id="f3cc2">Not Selected</span> <span class="arrow">
                                    &or;</span>
                                <div class="dropthird">
                                    <ul id="availableRecoverBuildNo" class="twocol1">
                                    </ul>
                                </div>
                            </div>
                        </div>
                        <input type="text" class="input2" id="f3input2" />
                    </div>
                </div>
                <div class="f2main">
                    <table width="1022px" border="0" cellspacing="0" cellpadding="10" class="table_main">
                        <tbody>
                            <tr id="Tr1" style="background-color: #d9d8d8; font-size: 14px; height: 30px;">
                                <td width="100px">
                                    <strong>ID</strong>
                                </td>
                                <td width="400px">
                                    <strong>Page Title</strong>
                                </td>
                                <td width="150px">
                                    <strong>User Name</strong>
                                </td>
                                <td width="300px">
                                    <strong>DO IT</strong>
                                </td>
                            </tr>
                            <tr class="nodataline">
                                <td colspan="4" style="text-align: center;">
                                    <span class="msg msgStyle">
                                        <img src="images/x.gif" alt="check" class="icon" style="float: left" />
                                        <b>No data available, please select the query condition!</b></span>
                                </td>
                            </tr>
                            <tr style="background-color: #d9d8d8; font-size: 10px; height: 20px;">
                                <td colspan="4">
                                    <div id="Div1">
                                        <div id="Div2">
                                            <input class="icon-btn" type="button" value="Previous" />
                                        </div>
                                        <div id="Div3">
                                        </div>
                                        <div id="Div4">
                                            <input class="icon-btn" type="button" value="Next" />
                                        </div>
                                        <div id="Div5">
                                        </div>
                                        <div id="Div6">
                                            Go&nbsp;&nbsp;<input type="text" id="Text1" style="width: 38px; display: inline" /></div>
                                        <div id="Div7">
                                            &nbsp;<input class="icon-btn" type="button" value="ok" id="Button1" /></div>
                                    </div>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <div class="inbottom">
                    <input name="rescanAll" type="button" id="Button2" class="com_btn" value="Rescan All" />
                    <input name="deleteAll" type="button" id="Button3" class="com_btn" value="Delete All" />
                    <input name="recoverAll" type="button" id="Button4" class="com_btn" value="Recover All" />
                </div>
            </div>
        </div>
    </div>
    <!--
    <div id="PageHeader">
        <a href="">Sulphur Administrator</a>
    </div>
    <div id="mainbody">
        <div id="tabs">
            <ul>
                <li><a href="#f1">
                    <img id="img2" width="20" height="20" src="Images/3d_scanner.png" alt="Clear" style="border-style: none" />
                    <span>UI ReScan</span></a></li>
                <li><a href="#f2">
                    <img id="img1" width="20" height="20" src="Images/Clean-Master-Cleaner-Icon.png"
                        alt="Clear" style="border-style: none" />
                    <span>UI Clear</span></a></li>
                <li><a href="#f3">
                    <img id="img3" width="20" height="20" src="Images/recover-deleted-files-from-blu-ray.jpg"
                        alt="Clear" style="border-style: none" />
                    <span>UI Recover</span></a></li>
            </ul>
            <div id="f1">
                In Progress...
            </div>
            <div id="f2">
                <table>
                    <tr>
                        <td colspan="2">
                            <p id="ClearTabMsg">
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Build No:
                        </td>
                        <td>
                            <select id="buildnotoclean">
                            </select>
                        </td>
                    </tr>
                    <tr>
                        <td>
                        </td>
                        <td>
                            <input type="button" id="btnClear" value="Start Clear" />
                        </td>
                    </tr>
                </table>
            </div>
            <div id="f3">
                <table>
                    <tr>
                        <td colspan="2">
                            <p id="RecoverTabMsg">
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Build No:
                        </td>
                        <td>
                            <select id="buildnotorecover">
                            </select>
                        </td>
                    </tr>
                    <tr>
                        <td>
                        </td>
                        <td>
                            <input type="button" id="btnRecover" value="Start Recover" />
                        </td>
                    </tr>
                </table>
            </div>
        </div>
    </div>-->
    </form>
</body>
</html>
