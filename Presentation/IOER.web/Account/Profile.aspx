<%@ Page Title="Profile" Language="C#" MasterPageFile="/Masters/Responsive2.Master" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="ILPathways.Account.Profile" %>
<%@ Register Src="~/Account/controls/UserProfile2.ascx" TagPrefix="uc1" TagName="UserProfile2" %>
<%@ Register Src="~/Account/controls/UserProfile.ascx" TagPrefix="uc1" TagName="UserProfile" %>
<%@ Register Src="~/Account/controls/AccountRegisterUpdateController.ascx" TagPrefix="uc1" TagName="AccountRegisterUpdateController" %>

<asp:Content ID="LoginContent" ContentPlaceHolderID="BodyContent" runat="server">

  <script type="text/javascript">
    //$("form").removeAttr("onsubmit");
  </script>
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
      <uc1:UserProfile runat="server" id="UserProfile1" Visible="false" />
      <uc1:AccountRegisterUpdateController runat="server" CurrentView="profile" ID="AccountRegisterUpdateController" />

  </div>
  
    <%--<uc1:UserProfile2 runat="server" id="UserProfile2" />--%>
</asp:Content>