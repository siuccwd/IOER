<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ContentSearch.aspx.cs" Inherits="IOER.Controls.Content.ContentSearch1" MasterPageFile="/Masters/Responsive.Master" %>
<%@ Register TagPrefix="uc1" TagName="ContentSearch" Src="/Controls/Content/ContentSearch2.ascx" %>

<asp:Content ContentPlaceHolderID="HeadContent" runat="server"></asp:Content>
<asp:Content ContentPlaceHolderID="BodyContent" runat="server">
	<uc1:ContentSearch id="contentSearch" runat="server"></uc1:ContentSearch>
</asp:Content>