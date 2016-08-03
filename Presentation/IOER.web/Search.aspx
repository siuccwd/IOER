<%@ Page Title="Illinois Open Educational Resources Search"  Language="C#" AutoEventWireup="true" CodeBehind="Search.aspx.cs" Inherits="IOER.ElasticSearch" MasterPageFile="~/Masters/Responsive.Master" %>
<%@ Register TagPrefix="uc1" TagName="ElasticSearchController" Src="/Controls/SearchV7/SearchV7.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="BodyContent" runat="server">
  <uc1:ElasticSearchController id="searchController" runat="server" />
</asp:Content>