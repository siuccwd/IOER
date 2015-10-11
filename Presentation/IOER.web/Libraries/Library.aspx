<%@ Page Title="IOER Library" Async="true"  Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="Library.aspx.cs" Inherits="IOER.Libraries.Library" %>

<%-- <%@ Register TagPrefix="uc1" TagName="librarySearch" Src="/LRW/controls/LibrarySearch.ascx" %> --%>
<%@ Register TagPrefix="uc1" TagName="librarySearch" Src="/LRW/controls/ESLibrary2.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">


</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
    <div style="min-height: 400px; ">
  <uc1:librarysearch ID="librarySearch1" runat="server" />

        </div>
</asp:Content>
