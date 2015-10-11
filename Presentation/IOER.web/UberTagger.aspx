<%@ Page Language="C#" Title="Illinois Open Educational Resources Tagger" AutoEventWireup="true" CodeBehind="UberTagger.aspx.cs" Inherits="IOER.UberTagger" MasterPageFile="/Masters/Responsive.Master" %>
<%@ Register TagPrefix="uc1" TagName="UberTagger" Src="/Controls/UberTaggerV1/UberTaggerV1.ascx" %>

<asp:Content runat="server" ContentPlaceHolderID="HeadContent"></asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="BodyContent">
  <uc1:UberTagger ID="uberTagger" runat="server" />
</asp:Content>