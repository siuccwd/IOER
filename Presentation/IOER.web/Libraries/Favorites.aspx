<%@ Page Title="My Favorite Libraries" Language="C#" MasterPageFile="~/Masters/Pathway.Master" AutoEventWireup="true" CodeBehind="Favorites.aspx.cs" Inherits="ILPathways.Libraries.Favorites" %>

<%@ Register TagPrefix="uc1" TagName="librariesSearch" Src="/LRW/controls/librariesSearch.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
    <h1 class="isleH1"><asp:label ID="libHeading" runat="server">My Favorite IOER Libraries</asp:label></h1>

<div class="isleMainSection">

<asp:Label ID="noContentMesssage" runat="server" Visible="false" >No publically available libraries are available at this time.</asp:Label>
 <asp:Panel ID="searchPanel" runat="server" Visible="true">
      <uc1:librariesSearch ID="librariesSearch1" SubscribedLibrariesView="yes" AutoSearch="true" runat="server" />

    </asp:Panel>

</div>
</asp:Content>
