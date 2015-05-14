<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="ILPathways.Controls.UberTaggerV2.Index" MasterPageFile="/Masters/Responsive.Master" %>
<%@ Register TagPrefix="uc1" TagName="UberTagger" Src="/Controls/UberTaggerV2/UberTaggerV2.ascx" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="BodyContent" runat="server">
  <uc1:UberTagger runat="server" id="tagger" Visible="true" />
</asp:Content>