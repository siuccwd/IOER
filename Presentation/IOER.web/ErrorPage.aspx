<%@ Page Title="Illinois OER Search Page Unavailable" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="ErrorPage.aspx.cs" Inherits="IOER.ErrorPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server"></asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
  <link rel="Stylesheet" type="text/css" href="/Styles/common.css" />
  <style type="text/css">
    .mainContent { margin-left: auto; margin-right: auto; width: 1200px; min-height: 500px;  }
    .mainContent p {font-size: 200%;}
    h1 { text-align:center; }
    @media screen and (max-width: 980px) {
      .mainContent { width: 90%; }
      .mainContent p {font-size: 150%;}
    }
  </style>
    <div id="centerContent" class="pageContent">
        <asp:Panel ID="e404Panel" runat="server" Visible="false">
            <h1>Illinois OER Page Unavailable</h1>
            <div class="mainContent">
            <p>The page you are looking for is currently unavailable or no longer exists (perhaps due to an outdated saved link). The Web site might be experiencing technical difficulties, or you may need to adjust your browser settings.</p>
            <p>This failure has been logged with our system administrators, who are currently working to resolve the problem. We apologize for any inconvenience caused by this temporary service outage, and we appreciate your patience as we work to improve our application.
            </p>
            <p>You must close your browser to clear your session. You can then open a new browser to re-access the Illinois OER Search. It is also recommended that you navigate using the current links, rather than a saved link that may be obsolete.</p>
            <p>Please close your browser and open a new browser window.</p>
                </div>
        </asp:Panel>

        <asp:Panel ID="e401Panel" runat="server" Visible="false">
            <h1>Unauthorized Access Attempt</h1>
            <div class="mainContent">
            <p>The requested page requires authentication and valid authorization.</p>
            <asp:Label ID="lblNotLoggedIn" runat="server" Visible="false">
            <p>Please Login in to IOER. 
                <br />Note:You still must be authorized to use the feature.</p>
            <p> If you still cannot access the feature and believe that you should have access, please contact IOER.
            </p>
            </asp:Label>
                        <asp:Label ID="lblLoggedIn" runat="server" Visible="false">
            <p>You have not been given the necessary privileges to access the last page.
                <br /> If you believe that you should have access to this feature, please contact IOER.
            </p>
            </asp:Label>
                </div>
        </asp:Panel>

        <p>
            <asp:Label ID="lblInfo" runat="server"></asp:Label>
        </p>
    </div>
</asp:Content>
