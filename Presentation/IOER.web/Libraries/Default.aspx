<%@ Page Title="IOER Libraries" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="IOER.Libraries.Default" %>
<%@ Register TagPrefix="uc1" TagName="LibrariesSearch" Src="/Controls/Libraries/LibrariesSearch.ascx" %>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">

  <uc1:LibrariesSearch id="LibSearch" runat="server" />
</asp:Content>
