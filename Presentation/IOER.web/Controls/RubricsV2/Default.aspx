<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="IOER.Controls.RubricsV2.Default" MasterPageFile="/Masters/Responsive.Master" %>
<%@ Register TagPrefix="uc1" TagName="Rubrics" Src="/Controls/RubricsV2/RubricsV2.ascx" %>

<asp:Content ContentPlaceHolderID="HeadContent" runat="server"></asp:Content>
<asp:Content ContentPlaceHolderID="BodyContent" runat="server">
	<uc1:Rubrics ID="rubrics" runat="server" />
</asp:Content>