<%@ Page Title="Illinois Open Educational Resources - SSO" Language="C#" AutoEventWireup="true" CodeBehind="IsleSSO.aspx.cs" Inherits="IOER.secure.IsleSSO" %>
<%@ Register TagPrefix="uc1" TagName="Login" Src="/secure/controls/IsleSSO.ascx" %>
<form id="aspnet_form" runat="server">
    <div>
        <uc1:Login id="loginControl" runat="server"></uc1:Login>
    </div>
</form>