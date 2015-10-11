<%@ Page Title="Illinois Open Educational Resources - Tag Resources"  Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="IOER.Controls.UberTaggerV2.Default" MasterPageFile="/Masters/Responsive.Master" %>
<%@ Register TagPrefix="uc1" TagName="UberTagger" Src="/Controls/UberTaggerV2/UberTaggerV2.ascx" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="BodyContent" runat="server">
  <uc1:UberTagger runat="server" id="tagger" Visible="true" />
</asp:Content>