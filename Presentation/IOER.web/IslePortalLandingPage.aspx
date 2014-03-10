<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="IslePortalLandingPage.aspx.cs" Inherits="ILPathways.IslePortalLandingPage" %>

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
        <asp:Literal ID="authenticationPage" runat="server" Text="/secure/IsleSSO.aspx" />
        <asp:Literal ID="displayPage" runat="server" Text="/PortalSplash.aspx" />
        <asp:Literal ID="SSOauthenticateUser" runat="server" Text="true" />
    </asp:Panel>
    </form>
</body>
</html>
