<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="IsleSSO.ascx.cs" Inherits="IOER.secure.controls.IsleSSO" %>

        <table>
            <tr>
                <td class="label"><asp:Label ID="lblGivenName" runat="server" AssociatedControlID="txtGivenName" Text="First Name" /></td>
                <td><asp:TextBox ID="txtGivenName" runat="server" CssClass="textBox"></asp:TextBox></td>
            </tr>
            <tr>
                <td class="label"><asp:Label id="lblSurname" runat="server" AssociatedControlId="txtSurname" Text="Last Name" /></td>
                <td><asp:TextBox ID="txtSurname" runat="server" CssClass="textbox" /></td>
            </tr>
            <tr>
                <td class="label"><asp:Label ID="lblEmail" runat="server" AssociatedControlID="txtEmail" Text="Email" /></td>
                <td><asp:TextBox ID="txtEmail" runat="server" CssClass="textBox" /></td>
            </tr>
            <tr>
                <td class="label"><asp:Label ID="lblPassword" runat="server" AssociatedControlID="txtPassword" Text="Password" /></td>
                <td><asp:TextBox ID="txtPassword" runat="server" TextMode="Password" /></td>
            </tr>
            <tr>
                <td class="label"><asp:Label ID="lblConfirmPassword" runat="server" AssociatedControlID="txtConfirmPassword" Text="Confirm Password" /></td>
                <td><asp:TextBox ID="txtConfirmPassword" runat="server" TextMode="Password" /></td>
            </tr>
            <tr><td colspan="2" id="loginErrorMessage"><%=errorMessage %></td></tr>
            <tr><td></td><td><asp:Button ID="btnSubmitRegister" runat="server" Text="Login" OnClick="btnSubmitRegister_Click" /></td></tr>
        </table>
    <asp:Panel ID="hiddenStuff" runat="server" Visible="false">
        <asp:Literal ID="defaultRedirectOld" runat="server" Text="/PortalSplash.aspx" />
        <asp:Literal ID="defaultRedirect" runat="server" Text="/Help/Guide.aspx" />
        <asp:Literal ID="profileRedirect" runat="server" Text="/Account/Profile.aspx" />
        <asp:Literal ID="profileCreateMessage" runat="server" Text="Your account has been created.  You should complete your <a href='{0}' target='_blank'>profile</a> for maximum benefits." />
        <asp:Literal ID="useProdHeaders" runat="server" Text="false" />
    </asp:Panel>
