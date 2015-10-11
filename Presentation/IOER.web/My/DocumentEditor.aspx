<%@ Page Title="Illinois Open Educational Resources - Document Editor" Language="C#" MasterPageFile="~/Masters/Responsive2.Master" AutoEventWireup="true" CodeBehind="DocumentEditor.aspx.cs" Inherits="IOER.My.DocumentEditor" %>

<%@ Register TagPrefix="uc1" TagName="AuthorTool" Src="/Controls/Content/DocumentEditor.ascx" %>

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
    <uc1:AuthorTool ID="DocumentEditor1" runat="server" />
  </div>
</asp:Content>
