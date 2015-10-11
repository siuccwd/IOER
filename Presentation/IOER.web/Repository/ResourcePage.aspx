<%@ Page Title="Illinois Open Educational Resources - Resource Detail" Language="C#" AutoEventWireup="true" CodeBehind="ResourcePage.aspx.cs" MasterPageFile="/Masters/Responsive.Master" Inherits="IOER.Content.ResourcePage" %>
<%@ Register TagPrefix="uc1" TagName="ResourcePageData" Src="/Controls/Content/ContentDisplayV2.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="BodyContent" runat="server">
  <link rel="Stylesheet" type="text/css" href="/Styles/common.css" />
  <style type="text/css">
    .mainContent { transition: padding-left 1s; -webkit-transition: padding-left 1s; min-height: 500px; }
    @media screen and (min-width: 980px) {
      .mainContent { padding-left: 50px; }
    }
  </style>
  <div class="mainContent">
    <uc1:ResourcePageData id="ResourcePageData1" runat="server" />
  </div>
</asp:Content>
