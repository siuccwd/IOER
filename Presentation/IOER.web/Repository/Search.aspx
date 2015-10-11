<%@ Page Title="Illinois Open Educational Resources - Content Search"  Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="Search.aspx.cs" Inherits="IOER.Content.Search" %>

<%@ Register TagPrefix="uc1" TagName="search" Src="/Controls/Content/ContentSearch2.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
      <uc1:search ID="search1"  IsMyAuthoredView="false" runat="server" />
</asp:Content>
