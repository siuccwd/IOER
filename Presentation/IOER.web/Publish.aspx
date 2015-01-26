<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeBehind="Publish.aspx.cs" Inherits="ILPathways.Publish" MasterPageFile="/Masters/Responsive.Master" %>

<%@ Register TagPrefix="uc1" TagName="Publisher" Src="/Controls/Publish3.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
  <link rel="Stylesheet" type="text/css" href="/Styles/common2.css" />
</asp:Content>

<asp:Content ContentPlaceHolderID="BodyContent" runat="server">
  <div id="content">
    <uc1:Publisher ID="PublisherControl" runat="server" />
  </div>
</asp:Content>
