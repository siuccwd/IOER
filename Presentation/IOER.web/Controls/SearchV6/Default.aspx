<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ILPathways.Controls.SearchV6.Index" MasterPageFile="/Masters/Responsive.Master" %>
<%@ Register TagPrefix="uc1" TagName="Search" Src="~/Controls/SearchV6/SearchV6.ascx" %>

<asp:Content runat="server" ContentPlaceHolderID="HeadContent"></asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="BodyContent">
  <div id="content">
    <h1 class="isleH1">Resource Search</h1>
    <uc1:Search ID="SearchMain" runat="server" />
  </div>
</asp:Content>