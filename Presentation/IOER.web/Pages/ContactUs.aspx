<%@ Page Title="" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="ContactUs.aspx.cs" Inherits="ILPathways.Pages.ContactUs" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">

    <h1>Contact Us</h1>

    <div class="column">
    <div class="grayBox bigText" id="account">

      <h2 class="header">Your Information</h2>
      <label>Name</label><input type="text" id="userName" class="email2" runat="server" />
      <label>Email Address</label>
        <input type="text" id="email1" class="email1" data-validation="email" runat="server" />

        <asp:Label style="display: none; position: absolute; right: -10000px;"  AssociatedControlID="txtAddress" runat="server">Do not enter anything here</asp:Label>
        <input type="text" id="txtAddress" style="display: none; position: absolute; right: -10000px;"  runat="server" />
      <p class="vm" id="validation_email" data-validation="email"></p>
      
        <label>Topic</label>
        <asp:DropDownList ID="ddlTopics" runat="server" ></asp:DropDownList>

      <label>Request</label>
        <asp:TextBox ID="txtRequest" runat="server" Rows="4" TextMode="MultiLine"></asp:TextBox>
        

    </div>
  </div><!-- /account -->

</asp:Content>
