<%@ Page Title="Illinois Open Educational Resources - Widget Previewer" Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="IOER.Widgets.PreviewerV1.Index" MasterPageFile="/Masters/Plain.Master" %>
<%@ Register TagPrefix="uc1" TagName="Previewer" Src="/Widgets/PreviewerV1/PreviewerV1.ascx" %>

<asp:Content ContentPlaceHolderID="BodyContent" runat="server">
  <uc1:Previewer ID="previewer" runat="server" />
</asp:Content>