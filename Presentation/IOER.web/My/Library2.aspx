<%@ Page Title="" Language="C#" MasterPageFile="~/Masters/Pathway.Master" AutoEventWireup="true" CodeBehind="Library2.aspx.cs" Inherits="ILPathways.My.Library2" %>

<%@ Register Src="~/Controls/Libraries/Library.ascx" TagPrefix="uc2" TagName="Library" %>




<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">


    <uc2:Library runat="server" ID="Library1" />
</asp:Content>
