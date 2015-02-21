<%@ Page Title="Download Files" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="Download.aspx.cs" Inherits="ILPathways.Content.Download" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
     
  <style type="text/css">
    .mainContent { transition: padding-left 1s; -webkit-transition: padding-left 1s; min-height: 500px; }
    @media screen and (min-width: 980px) {
      .mainContent { padding-left: 50px; }
    }

    </style>
    <div class="mainContent">
    <h1 class="isleH1">Download IOER Files</h1>
        <asp:label ID="lblResults" runat="server" >download location</asp:label>
        
  </div>
</asp:Content>