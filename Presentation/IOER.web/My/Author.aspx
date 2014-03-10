<%@ Page Title="" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="Author.aspx.cs" Inherits="ILPathways.My.Author" %>
<%@ Register TagPrefix="uc1" TagName="AuthorTool" Src="/Controls/Content/authoring.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
  <script type="text/javascript">
    $("form").removeAttr("onsubmit");
  </script>
  <%--<link rel="Stylesheet" type="text/css" href="/Styles/common.css" />--%>
  <style type="text/css">
    .mainContent { transition: padding-left 1s; -webkit-transition: padding-left 1s; }
    @media screen and (min-width: 980px) {
      .mainContent { padding-left: 50px; }
    }
  </style>
  <ajaxToolkit:ToolkitScriptManager runat="Server" EnablePartialRendering="true" ID="ScriptManager1" />
  <div class="mainContent">
    <uc1:AuthorTool ID="AuthoringTool" runat="server" />
  </div>
</asp:Content>
