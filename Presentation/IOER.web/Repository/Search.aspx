<%@ Page Title="" Language="C#" MasterPageFile="~/Masters/Pathway.Master" AutoEventWireup="true" CodeBehind="Search.aspx.cs" Inherits="ILPathways.Content.Search" %>

<%@ Register TagPrefix="uc1" TagName="search" Src="/Controls/Content/ContentSearch.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">


<h1 class="isleH1">Content</h1>
<div class="isleMainSection">

    <asp:Panel ID="searchPanel" runat="server" Visible="true">
      <uc1:search ID="search1"  IsMyAuthoredView="false" runat="server" />

    </asp:Panel>
</div>


</asp:Content>
