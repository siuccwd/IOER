<%@ Page Title="Illinois Open Education Resources Search News" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ILPathways.LRW.News.Default" %>
<%@ Register TagPrefix="uc2" TagName="News" Src="~/Controls/AppItems/NewsItemSearch.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server"></asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
  <script type="text/javascript">
    $("form").removeAttr("onsubmit");
  </script>
  <link rel="Stylesheet" type="text/css" href="/Styles/common.css" />
  <style type="text/css">
    .mainContent { transition: padding-left 1s; -webkit-transition: padding-left 1s; min-height: 500px; }
    @media screen and (min-width: 980px) {
      .mainContent { padding-left: 50px; }
    }
  </style>
  <ajaxToolkit:ToolkitScriptManager runat="Server" EnablePartialRendering="true" ID="ScriptManager1" />
  <div class="mainContent">
  <uc2:News ID="News1" runat="server" 
      AllowingCategoriesPanelDisplay="no"
      NewsItemTemplateCode="IOERS"   
      UsingWebService="yes" HeadingText=""
      NbrItems="5" 
    />
</div>
<asp:Literal ID="customPageTitle" runat="server" Visible="false"></asp:Literal>
</asp:Content>
