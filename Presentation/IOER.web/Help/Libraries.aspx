<%@ Page Title="IOER Libraries Guide" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="Libraries.aspx.cs" Inherits="ILPathways.Help.Libraries" %>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
  <link rel="Stylesheet" type="text/css" href="/Styles/common.css" />
  <style type="text/css">
    .mainContent { transition: padding-left 1s; -webkit-transition: padding-left 1s; }
    @media screen and (min-width: 980px) {
      .mainContent { padding-left: 50px; }
    }
  </style>
  <div class="mainContent">
    <h1 class="isleH1">IOER Libraries</h1>
    <div style="padding: 2px 15px;">
      The Illinois Shared Learning Environment (ISLE) Open Education Resources (OER) tools include libraries for: 
      <ul>
        <li>creating collections of resources </li>
        <li>setting collections as private or shared for others to find and use</li>
        <li>following collections </li>
      </ul>

      Sharing collections with others is more beneficial when they are setup with meaningful names; holistic descriptions; targeted to subjects and/or grade levels and contain good resources. It’s easy to create, share, and follow libraries. 
      <br />
      Begin by logging in, select to My Library, give a meaningful name and description. Personalize it by uploading an image. Once you create a library, you can create collections for resources that you create or are created by others.
    </div>
  </div>
</asp:Content>
