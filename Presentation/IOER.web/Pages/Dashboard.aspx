<%@ Page Title="Illinois Open Educational Resources Dashboard" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="IOER.Pages.Dashboard" %>
<%@ Register TagPrefix="uc1" TagName="Dashboard" Src="/Account/controls/Dashboard.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
  <link rel="stylesheet" type="text/css" href="/styles/common2.css" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
  <uc1:Dashboard ID="UserDashboard" runat="server" />
</asp:Content>
