<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ShowAvailableSulpHurPicture.aspx.cs" Inherits="SulpHurManagementSystem.ShowAvailableSulpHurPicture" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <asp:Button ID="ShowResults" runat="server" Text="Show Results" OnClick="ShowResults_Click" />
    <asp:Button ID="ShowAvailableResults" runat="server" Text="Show Available Results" OnClick="ShowAvailableResults_Click" />   
    <asp:Button ID="HiddenResults" runat="server" Text="Hidden Results" OnClick="HiddenResults_Click" />
    <div>     
        <table>
            <tr>
                <td>
                    <asp:Label ID="Label1" runat="server" Text="SulpHurPre:" Visible="false" ForeColor="LightGray"></asp:Label>
                    <span id="availableBeforeBuild" runat="server" style="color: blue"></span>
                </td>
                <td>
                    <asp:Label ID="Label2" runat="server" Text="SulpHurPost" Visible="false" ForeColor="LightGray"></asp:Label>
                    <span id="availableAfterBuild" runat="server" style="color: blue"></span>
                </td>
            </tr>
            <tr>
                <td valign="top">
                    <asp:Image ID="SulpHurPreImage" runat="server" />  
                    <asp:Label ID="PreLabel" runat="server" Text="" Visible="false"></asp:Label>
                </td>
                <td valign="top">      
                    <asp:Image ID="SulpHurPostImage" runat="server" />
                    <asp:Label ID="PostLabel" runat="server" Text="" Visible="false"></asp:Label>
                </td>
            </tr>
        </table>
    </div>
    </form>
</body>
</html>
