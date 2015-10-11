<%@ Page Title="Library Administration" Language="C#" MasterPageFile="~/Masters/Responsive2.Master" AutoEventWireup="true" CodeBehind="Admin.aspx.cs" Inherits="IOER.Libraries.Admin" %>

<%@ Register Src="~/Controls/Libraries/LibraryAdmin.ascx" TagPrefix="uc1" TagName="LibraryAdmin" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
<style type="text/css">
#pageContainer { min-height: 500px; padding-left: 10px; }

@media screen and (max-width: 800px) {
#pageContainer { min-height: 100px; padding-left: 5px; }
}
</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
    <uc1:LibraryAdmin runat="server" id="LibraryAdmin1" />
      
</asp:Content>
