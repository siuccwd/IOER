<%@ Page Title="Getting Started" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ILPathways.Help.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
  <link rel="Stylesheet" type="text/css" href="/Styles/common.css" />
  <style type="text/css">
    .mainContent { transition: padding-left 1s; -webkit-transition: padding-left 1s; }
    @media screen and (min-width: 980px) {
      .mainContent { padding-left: 50px; }
    }
  </style>
  <div class="mainContent">
    <h1 class="isleH1">Getting Started with IOER</h1>

    <h2 class="isleH2">Search Open Resources</h2>
    <h2 class="isleH2">Libraries</h2>

    <h3 class="isleH3">User Libraries</h3>
    All users may create a library:
    <ul><li>personalize libraries and collection images</li>
        <li>add resources found using the resourse search </li>
        <li>copy resources from other libraries</li></ul>

    <h3 class="isleH3">Organization Libraries</h3>
        <a href="/Help/Libraries.aspx">More information - Libraries Guide</a>

    <h2 class="isleH2">Contributing</h2>

    <h3 class="isleH3">Tagging Online resourses</h3>
    <h3 class="isleH3">Creating new resourses</h3>

  </div>
</asp:Content>
