<%@ Page Title="Illinois Open Educational Resources Search"  Language="C#" AutoEventWireup="true" CodeBehind="Search.aspx.cs" Inherits="ILPathways.ElasticSearch" MasterPageFile="~/Masters/Responsive.Master" %>
<%@ Register TagPrefix="uc1" TagName="ElasticSearchController" Src="/Controls/SearchV6/SearchV6.ascx" %>
<%-- <%@ Register TagPrefix="uc1" TagName="ElasticSearchController" Src="/LRW/Controls/ElasticSearch3.ascx" %>--%>
<%--<%@ Register TagPrefix="uc1" TagName="ElasticSearchController" Src="/Controls/ImportedSearch.ascx" %>--%>

<asp:Content ID="Content1" ContentPlaceHolderID="BodyContent" runat="server">
  <div id="content">
    <h1 class="isleH1">IOER Resource Search</h1>
    <uc1:ElasticSearchController id="searchController" runat="server" />
  </div>
</asp:Content>