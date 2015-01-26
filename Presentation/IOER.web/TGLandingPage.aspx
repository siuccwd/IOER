<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TGLandingPage.aspx.cs" Inherits="ILPathways.TGLandingPage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
    </div>
    <asp:Panel ID="hiddenStuff" runat="server" Visible="false">
        <asp:Literal ID="authenticationPage" runat="server" Text="/secure/IsleSSO.aspx?nextUrl=http://ioer.ilsharedlearning.org/Default.aspx" />
        <asp:Literal ID="displayPage" runat="server" Text="/Default.aspx" />
        <asp:Literal ID="SSOauthenticateUser" runat="server" Text="true" />
    </asp:Panel>
    </form>
</body>
</html>
