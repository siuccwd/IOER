<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="IOER.Controls.Content.Default" MasterPageFile="/Masters/Responsive.Master" %>
<%@ Register TagPrefix="uc1" TagName="ContentDisplay" Src="/controls/content/ContentDisplayV2.ascx" %>

<asp:Content ContentPlaceHolderID="HeadContent" runat="server"></asp:Content>
<asp:Content ContentPlaceHolderID="BodyContent" runat="server">
	<uc1:ContentDisplay id="display" runat="server"></uc1:ContentDisplay>
</asp:Content>