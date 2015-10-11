<%@ Page Title="Illinois Open Educational Resources - Content Author" Language="C#" MasterPageFile="~/Masters/Responsive2.Master" AutoEventWireup="true" CodeBehind="Author.aspx.cs" Inherits="IOER.My.Author" %>
<%@ Register TagPrefix="uc1" TagName="AuthorTool" Src="/Controls/Content/authoring.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
  
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
