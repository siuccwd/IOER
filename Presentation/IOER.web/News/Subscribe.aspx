<%@ Page Title="Illinois Open Education Resources News Subscribe" Language="C#" MasterPageFile="~/Masters/Responsive2.Master" AutoEventWireup="true" CodeBehind="Subscribe.aspx.cs" Inherits="IOER.Pages.Subscribe" %>
<%@ Register TagName="Subscribe" TagPrefix="uc1" Src="~/News/Controls/AnnouncementSubscribe2.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">

<ajaxToolkit:ToolkitScriptManager runat="Server" EnablePartialRendering="true" ID="ScriptManager1" />
<div style="min-height: 500px; margin-left: 50px;">
    <uc1:Subscribe ID="SubscribeMe" 
    NewsItemTemplateCode="IOERS" 
    AnnouncementTitle="Illinois Open Education Resource Search" 
    SubscribeUrl="./Subscribe.aspx" 
    runat="server" />
</div>
<asp:Literal ID="customPageTitle" runat="server" Visible="false"></asp:Literal>
</asp:Content>

