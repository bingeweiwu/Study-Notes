<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ShowTestCoverage.aspx.cs" Inherits="SulpHurManagementSystem.ShowTestCoverage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Show Test Coverage Report</title>
</head>
<body>
    <form id="form1" runat="server">
    <span style="float: left; margin: 0 10px 0 0px;">Verify Above Build:</span>
    <input runat="server" id="txtbuild" type="text" value="" style="width: 5%;" />
    <br />
    <asp:Button ID="GetTestCoverageReport" runat="server" OnClick="GetTestCoverageReport_Click" Text="GetTestCoverageReport" />
    <asp:Table ID="TestCoverageTable" runat="server" GridLines="Both" CellSpacing="2"></asp:Table>
    </form>
    </body>
</html>
