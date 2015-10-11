<%@ Page Title="IOER Publisher Search" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="Publishers.aspx.cs" Inherits="IOER.Publishers" %>
<%@ Register TagPrefix="uc1" TagName="Publishers" Src="/LRW/controls/PublishersSearch2.ascx" %>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
  <script type="text/javascript">
    $(document).ready(function () {
      $("form").removeAttr("onsubmit");
    });
  </script>
  <link rel="Stylesheet" type="text/css" href="/Styles/common.css" />
  <style type="text/css">
    .mainContent { transition: padding-left 1s; -webkit-transition: padding-left 1s; min-width: 300px; }
    @media screen and (min-width: 980px) {
      .mainContent { padding-left: 50px; }
    }
  </style>
  <asp:Panel ID="searchPanel" runat="server" Visible="true" CssClass="mainContent">
    <uc1:Publishers ID="Publishers1" InitializingOnLoad="true"  runat="server"></uc1:Publishers>
  </asp:Panel>
	
</asp:Content>
