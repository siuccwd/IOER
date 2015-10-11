<%@ Page Language="C#" Title="Illinois Open Educational Resources Search" AutoEventWireup="true" CodeBehind="CustomSearch.aspx.cs" Inherits="IOER.LearningListsSearch" MasterPageFile="/Masters/Responsive.Master" %>

<%@ Register TagPrefix="uc1" TagName="ElasticSearchController" Src="/Controls/SearchV6/SearchV6.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="BodyContent" runat="server">
  <div id="content">
    <h1 class="isleH1"><%=Title %></h1>
    <uc1:ElasticSearchController id="searchController" runat="server" />
  </div>
</asp:Content>