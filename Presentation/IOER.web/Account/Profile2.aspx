<%@ Page Title="My Profile" Language="C#" MasterPageFile="~/Masters/Responsive2.Master" AutoEventWireup="true" CodeBehind="Profile2.aspx.cs" Inherits="ILPathways.Account.Profile2" %>

<%@ Register Src="~/Account/controls/UserProfile.ascx" TagPrefix="uc1" TagName="UserProfile" %>


<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
<link href="/Styles/Isle_large.css" type="text/css" rel="stylesheet" />
  <style type="text/css">
    .mainContent { transition: padding-left 1s; -webkit-transition: padding-left 1s; }
    @media screen and (min-width: 980px) {
      .mainContent { padding-left: 50px; }
    }
  </style>


<ajaxToolkit:ToolkitScriptManager runat="Server" EnablePartialRendering="true" ID="ScriptManager1" />
  <div id="loginContainer" class="mainContent">
  <h1 class="isleH1">IOER User Profile</h1>
      <uc1:UserProfile runat="server" id="UserProfile1" />
</div>
</asp:Content>
