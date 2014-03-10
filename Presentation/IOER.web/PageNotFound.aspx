<%@ Page Title="Illinois OER Search Page Not Found" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="PageNotFound.aspx.cs" Inherits="ILPathways.PageNotFound" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server"></asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
  <link rel="Stylesheet" type="text/css" href="/Styles/common.css" />
  <style type="text/css">
    .mainContent { transition: padding-left 1s; -webkit-transition: padding-left 1s; min-height: 500px; }
    @media screen and (min-width: 980px) {
      .mainContent { padding-left: 50px; }
    }
  </style>

	<div id="centerContent" class="mainContent">		    
		<h1>Illinois OER Search Page Unavailable</h1>

		<p>The page you are looking for is currently unavailable or no longer exists (perhaps due to an outdated saved link). The Web site might be experiencing technical difficulties, or you may need to adjust your browser settings.</p>
		<p>This failure has been logged with our system administrators, who are currently working to resolve the problem. We 
		apologize for any inconvenience caused by this temporary service outage, and we 
		appreciate your patience as we work to improve our application.	</p>		
		<p>You can:</p>
		<ul>
		  <li><a href="/">Click here to navigate to the Illinois OER Search landing page</a></li>
		  <li>Or close your browser and open a new browser window.</li>
		</ul> 					
		<p>You can close your browser to clear your session. You can then open a new browser to re-access the Illinois OER Search. It is also recommended that you navigate using the current links, rather than a saved link that may be obsolete.</p>									
		<p><asp:label id="lblInfo" runat="server"></asp:label></p>
	</div>

</asp:Content>
