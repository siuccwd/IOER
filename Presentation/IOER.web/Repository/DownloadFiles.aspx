<%@ Page Title="" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="DownloadFiles.aspx.cs" Inherits="ILPathways.Repository.DownloadFiles" %>
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

        <div class="isleBox" style="width: 400px;">
            <h2 class="isleBox_H2">Download Requested File</h2>
            <asp:label ID="lblResults" runat="server" ></asp:label>
            <asp:HyperLink ID="hlFileUrl" runat="server" Visible="false" Target="ioerdnld">Download requested file</asp:HyperLink>

        </div>

        
  </div>
    <asp:Literal ID="doingImmediateDownload" runat="server" Visible="false">yes</asp:Literal>
    <asp:Literal ID="usingOption2" runat="server" Visible="false">yes</asp:Literal>
    <asp:Literal ID="checkForExistingFile" runat="server" Visible="false">no</asp:Literal>
</asp:Content>
