<%@ Page Title="My IOER" Async="true"  Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="Library.aspx.cs" Inherits="ILPathways.My.Library" %>

<%@ Register TagPrefix="uc1" TagName="librarySearch" Src="/LRW/controls/ESLibrary2.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
<uc1:librarySearch ID="libraryMgmt1" runat="server" />

<%--      <uc1:librarySearch ID="libraryMgmt1" IsPersonalLibraryView="true" DisplayURL="/My/Library.aspx"  runat="server" />--%>


</asp:Content>
