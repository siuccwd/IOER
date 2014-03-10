<%@ Page Title="IOER Library" Async="true"  Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="Library.aspx.cs" Inherits="ILPathways.Libraries.Library" %>

<%-- <%@ Register TagPrefix="uc1" TagName="librarySearch" Src="/LRW/controls/LibrarySearch.ascx" %> --%>
<%@ Register TagPrefix="uc1" TagName="librarySearch" Src="/LRW/controls/ESLibrary2.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">


</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
  <uc1:librarysearch ID="librarySearch1" runat="server" />

<%-- 
<div class="isleMainSection">
   
  <div id="subTabPanel">
    <asp:Panel ID="searchPanel" runat="server" Visible="true">
      <uc1:librarySearch ID="librarySearch1" IsPersonalLibraryView="false" DisplayURL="/Libraries/Library.aspx" runat="server" />

    </asp:Panel>
	</div>


  </div>
--%>
</asp:Content>
