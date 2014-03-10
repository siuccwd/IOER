<%@ Page Title="" Language="C#" MasterPageFile="~/Masters/Pathway.Master" AutoEventWireup="true" CodeBehind="ComingSoon.aspx.cs" Inherits="ILPathways.My.ComingSoon" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
<h1><%=MyTitle %></h1>
<div class="isleMainSection">
<asp:Label ID="noContentMesssage" runat="server" >Coming Soon.</asp:Label>
</div>

</asp:Content>
