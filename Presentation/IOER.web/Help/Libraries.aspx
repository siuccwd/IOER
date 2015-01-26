<%@ Page Title="IOER Libraries Guide" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="Libraries.aspx.cs" Inherits="ILPathways.Help.Libraries" %>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
  <link rel="Stylesheet" type="text/css" href="/Styles/common.css" />
  <style type="text/css">
    .mainContent { transition: padding-left 1s; -webkit-transition: padding-left 1s; }
    @media screen and (min-width: 980px) {
      .mainContent { padding-left: 50px; }
    }
  </style>
  <div class="mainContent">
    <h1 class="isleH1">IOER Libraries</h1>
    <div style="padding: 2px 15px;">
      The Illinois Shared Learning Environment (ISLE) Open Educational Resources (OER) tools includes the ability to create and manage personal libraries. 
        <h2>Getting Started</h2>
        <ul>
        <li>All authenticated users can create their own personal library (a default library is created upon registration confirmation).</li>
        <li>A library is made of collections. You can add as many collections as makes sense to a library.</li>
            <li>Provide meaningful names for your libraries and collections.
                </li>
                <li>Sharing collections with others is more beneficial when they are setup with meaningful names; holistic descriptions; targeted subjects and/or grade levels and contain good resources.  </li>
        <li>Resources can be added to collections. How:
            <ul>
                <li>Use the resources search to find interesting resources, and select a resource.</li>
                <li>On the detail page, there is a library tab (2nd tab on the right side). Here you can select a library and collection, then add the resource to the selected collection.</li>
                <li>Resources can be copied from other libraries as well.</li>
            </ul>
        </li>
        <li>You can request access privileges to other libraries, including role of contributor, curator, and administrator</li>
        <li>You can invite other users to access your library, including assigning one of the above roles.</li>
      </ul>
        <h2>Personalization</h2>
      <ul>
        <li>An image can be uploaded to associate with a library or a collection.</li>
        <li>The user can control how the general public can view or interact with their library or collections.</li>
        <li>As well, the user can specify different access type for members of their organization.</li>
      </ul>
      <h2>Sharing</h2>
      <ul>
        <li>You can follow other libraries or collections.</li>
        <li>You can copy resources between libraries.</li>
        <li>You can "like" libraries and collections.</li>
        <li>You can "comment" on libraries and collections.</li>
      </ul>
      <p>
      <br />
      Ready - Set - Go
          </p>
    </div>
  </div>
</asp:Content>
