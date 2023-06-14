<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CMIntlTestResults.aspx.cs" Inherits="SulpHurManagementSystem.CMIntlTestResults" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>CM Intl Test Results</title>
    <script type="text/javascript" src="Scripts/jquery-1.8.2.js"></script>
    <script type="text/javascript" src="Scripts/jquery-ui-1.8.23.min.js"></script>
    <script type="text/javascript" src="Scripts/jquery.multiselect.js"></script>
    <script type="text/javascript" src="Scripts/common.js"></script>
    <script type="text/javascript">  
        function GetQueryString(name) {  
            var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)");  
            var r = window.location.search.substr(1).match(reg);  
            if (r != null)  
                return unescape(r[2]);  
            return null;  
        }  
        $(document).ready(function () {  
            $("#frameNav").attr("src", GetQueryString('url'));
        });         
     </script>
</head>
    <frameset cols="300,*" >
       <frame src="" runat="server" id="frameNav" name="frameNav" frameborder="0" application="yes" />
       <frameset rows="300,*,*,50" cols="*">
           <frame src="" name="frameReport" style="border:medium double rgb(0,0,0)" application="yes" />
           <frame src="" name="frameSulpHurPic" style="border:medium double rgb(0,0,0)" application="yes" />
           <frame src="" name="frameTestCoverage" style="border:medium double rgb(0,0,0)" application="yes" />
           <frame src="ShowAllTestResults.aspx" name="frameLead" style="border:medium double rgb(0,0,0)" application="yes" />
       </frameset>
    </frameset>
</html>
