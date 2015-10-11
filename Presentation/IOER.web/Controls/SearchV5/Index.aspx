<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="IOER.Controls.SearchV5.Index" MasterPageFile="/Masters/Responsive.Master" %>
<%@ Register TagPrefix="uc1" TagName="Search" Src="~/Controls/SearchV5/SearchV5.ascx" %>

<asp:Content runat="server" ContentPlaceHolderID="HeadContent"></asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="BodyContent">
  <uc1:Search ID="SearchMain" runat="server" />
</asp:Content>