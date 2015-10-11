<%@ Page Title="Illinois Open Educational Resources - Published" Language="C#" MasterPageFile="~/Masters/Pathway.Master" AutoEventWireup="true" CodeBehind="Published.aspx.cs" Inherits="IOER.My.Published" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
  <h1>My Published Content</h1>
  <div class="isleMainSection">
    <asp:Label ID="noContentMesssage" runat="server" >You have not published any content as yet.</asp:Label>
  </div>
</asp:Content>
