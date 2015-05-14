<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UberTagger.aspx.cs" Inherits="ILPathways.UberTagger" MasterPageFile="/Masters/Responsive.Master" %>
<%@ Register TagPrefix="uc1" TagName="UberTagger" Src="/Controls/UberTaggerV1/UberTaggerV1.ascx" %>

<asp:Content runat="server" ContentPlaceHolderID="HeadContent"></asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="BodyContent">
  <uc1:UberTagger ID="uberTagger" runat="server" />
</asp:Content>