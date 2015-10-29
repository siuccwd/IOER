<%@ Page Title="Illinois Open Educational Resources - Register" Language="C#" MasterPageFile="/Masters/Responsive2.Master" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="IOER.Account.Register" %>

<%@ Register TagPrefix="uc1" TagName="RegisterController" Src="/Account/controls/Registration2.ascx" %>

<asp:Content ID="LoginContent" ContentPlaceHolderID="BodyContent" runat="server">
    <link href="/Styles/Isle_large.css" type="text/css" rel="stylesheet" />

    <uc1:RegisterController runat="server" ID="Registration2" />

</asp:Content>