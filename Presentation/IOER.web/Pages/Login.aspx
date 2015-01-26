<%@ Page Title="Login" Language="C#" MasterPageFile="/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="ILPathways.Pages.Login" %>

<%@ Register Src="~/Account/controls/LoginController.ascx" TagPrefix="uc1" TagName="LoginController" %>


<asp:Content ID="LoginContent" ContentPlaceHolderID="BodyContent" runat="server">
  <uc1:LoginController runat="server" ID="LoginController" />
</asp:Content>
