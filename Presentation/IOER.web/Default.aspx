<%@ Page Title="Illinois Open Education Resources Search" Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="IllinoisPathways._Default" MasterPageFile="~/Masters/Responsive.Master" %>
<%@ Register TagPrefix="uc1" TagName="ElasticSearchController" Src="/Controls/Splash2.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="BodyContent" runat="server">
  <uc1:ElasticSearchController id="searchController" runat="server" />
</asp:Content>