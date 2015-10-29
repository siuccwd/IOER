<%@ Page Title="Illinois Open Educational Resources - Login" Language="C#" MasterPageFile="/Masters/Responsive2.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="IOER.Account.Login" %>
<%@ Register TagPrefix="uc1" TagName="LoginController" Src="/Account/controls/LoginController.ascx" %>

<asp:Content ID="LoginContent" ContentPlaceHolderID="BodyContent" runat="server">
  <uc1:LoginController runat="server" ID="LoginController" />
</asp:Content>