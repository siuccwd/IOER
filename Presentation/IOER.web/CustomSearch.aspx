<%@ Page Language="C#" Title="Illinois Open Educational Resources Search" AutoEventWireup="true" CodeBehind="CustomSearch.aspx.cs" Inherits="IOER.LearningListsSearch" MasterPageFile="/Masters/Responsive.Master" %>

<%@ Register TagPrefix="uc1" TagName="ElasticSearchController" Src="/Controls/SearchV7/SearchV7.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="BodyContent" runat="server">
	<uc1:ElasticSearchController id="searchController" runat="server" />
</asp:Content>