<%@ Page Title="IOER Author search" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="Authored.aspx.cs" Inherits="ILPathways.My.Authored" %>
<%@ Register TagPrefix="uc1" TagName="search" Src="/Controls/Content/ContentSearch.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
  <script type="text/javascript">
    $("form").removeAttr("onsubmit");
  </script>
  <style type="text/css">
    .mainContent { transition: padding-left 1s; -webkit-transition: padding-left 1s; }
    @media screen and (min-width: 980px) {
      .mainContent { padding-left: 50px; }
    }
  </style>
  <ajaxToolkit:ToolkitScriptManager runat="Server" EnablePartialRendering="true" ID="ScriptManager1" />
  <div class="mainContent">
    <h1 class="isleH1">My Resources</h1>
    <div class="isleMainSection">
      <asp:Label ID="noContentMesssage" Visible="false" runat="server" >You have not authored any content as yet.</asp:Label>
      <asp:Panel ID="searchPanel" runat="server" Visible="true">
        <uc1:search ID="search1"  IsMyAuthoredView="true" runat="server" />
      </asp:Panel>
    </div>
  </div>

</asp:Content>
