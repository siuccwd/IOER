<%@ Page Title="Illinois Open Educational Resources - Resource" Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="IOER.Controls.DetailV7.Default" MasterPageFile="/Masters/Responsive.Master" %>
<%@ Register TagPrefix="uc1" TagName="Detail" Src="/Controls/DetailV7/DetailV7.ascx" %>

<asp:Content ContentPlaceHolderID="HeadContent" runat="server"></asp:Content>
<asp:Content ContentPlaceHolderID="BodyContent" runat="server">
	<uc1:Detail id="detail" runat="server"></uc1:Detail>
</asp:Content>