<%@ Page Title="" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ILPathways.Organizations.Default" %>

<%@ Register Src="~/Controls/OrgMgmt/OrgSearch.ascx" TagPrefix="uc1" TagName="OrgSearch" %>


<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">

    <uc1:OrgSearch runat="server" ID="OrgSearch" />
</asp:Content>
