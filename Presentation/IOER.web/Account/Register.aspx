<%@ Page Title="Register" Language="C#" MasterPageFile="/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="ILPathways.Account.Register" %>
<%@ Register TagPrefix="uc1" TagName="AccountRegisterUpdateController" Src="/Account/controls/AccountRegisterUpdateController.ascx" %>
<%@ Register TagPrefix="uc1" TagName="RegisterController" Src="/Account/controls/Registration2.ascx" %>

<asp:Content ID="LoginContent" ContentPlaceHolderID="BodyContent" runat="server">
    <link href="/Styles/Isle_large.css" type="text/css" rel="stylesheet" />

    <uc1:RegisterController runat="server" ID="Registration2" />

</asp:Content>