<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Library.aspx.cs" Inherits="IOER.Controls.SearchV6.Library" MasterPageFile="/Masters/Responsive.Master" %>
<%@ Register TagPrefix="uc1" TagName="Search" Src="~/Controls/SearchV6/SearchV6.ascx" %>

<asp:Content runat="server" ContentPlaceHolderID="HeadContent"></asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="BodyContent">
  <div id="content">
    <h1 class="isleH1" id="pageHeader">Library Search</h1>
    <uc1:Search ID="SearchMain" runat="server" ThemeName="ioer_library" />
  </div>
</asp:Content>