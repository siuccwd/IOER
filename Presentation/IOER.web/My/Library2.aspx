<%@ Page Title="" Language="C#" MasterPageFile="~/Masters/Pathway.Master" AutoEventWireup="true" CodeBehind="Library2.aspx.cs" Inherits="ILPathways.My.Library2" %>
<%@ Register TagPrefix="uc1" TagName="librarySearch" Src="/LRW/controls/LibrarySearch.ascx" %>
<%--<%@ Register TagPrefix="uc1" TagName="librarySearch" Src="/LRW/controls/ESLibrary.ascx" %>--%>


<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
<%--<uc1:librarysearch ID="librarySearch1" runat="server" />--%>

<uc1:librarySearch ID="librarySearch1" IsPersonalLibraryView="true" DisplayURL="/My/Library.aspx"  runat="server" />


</asp:Content>
