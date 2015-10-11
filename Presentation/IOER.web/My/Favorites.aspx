<%@ Page Title="My Favorite Libraries" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="Favorites.aspx.cs" Inherits="IOER.My.Favorites" %>

<%@ Register TagPrefix="uc1" TagName="LibrariesSearch" Src="/Controls/Libraries/LibrariesSearch.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">


  <uc1:librariesSearch ID="librariesSearch1" SubscribedLibrariesView="yes" runat="server" />
</asp:Content>
