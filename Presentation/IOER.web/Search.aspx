<%@ Page Title="Illinois Open Educational Resources Search"  Language="C#" AutoEventWireup="true" CodeBehind="Search.aspx.cs" Inherits="IOER.ElasticSearch" MasterPageFile="~/Masters/Responsive.Master" %>
<%@ Register TagPrefix="uc1" TagName="ElasticSearchController" Src="/Controls/SearchV7/SearchV7.ascx" %>
<%-- <%@ Register TagPrefix="uc1" TagName="ElasticSearchController" Src="/LRW/Controls/ElasticSearch3.ascx" %>--%>
<%--<%@ Register TagPrefix="uc1" TagName="ElasticSearchController" Src="/Controls/ImportedSearch.ascx" %>--%>

<asp:Content ID="Content1" ContentPlaceHolderID="BodyContent" runat="server">
  <uc1:ElasticSearchController id="searchController" LoadTheme="ioer" runat="server" />
</asp:Content>