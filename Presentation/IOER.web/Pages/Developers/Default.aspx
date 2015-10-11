<%@ Page Title="Illinois Open Educational Resources - Developer Documentation" Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="IOER.Pages.Developers.Default" MasterPageFile="/Masters/Responsive.Master" %>

<asp:Content ContentPlaceHolderID="HeadContent" runat="server">
  <link rel="stylesheet" href="/styles/common2.css" />
</asp:Content>
<asp:Content ContentPlaceHolderID="BodyContent" runat="server">

  <style type="text/css">
    /* Big Items */
    #data h2 { font-size: 24px; margin: 25px 0 5px 0; padding: 5px; background-color: #EEE; border-radius: 5px; }
    #data h2:first-of-type { margin: 0 0 5px 0; }
    #data h3 { font-size: 20px; margin: 5px 0 0 0; border-left: 5px solid #555; padding-left: 5px; }
    #navigationItems, #data { display: inline-block; vertical-align: top; }
    #navigationItems { width: 300px; margin-top: 5px; }
    #data { width: calc(100% - 300px); padding: 0 5px 10px 5px; }
    #data .dataContainer { padding: 0 10px 0 10px; }
    #navigationItems a { display: block; padding: 5px 10px; margin: 0 -10px; transition: background-color 0.2s, color 0.2s; }
    #navigationItems a:hover, #navigationItems a:focus { background-color: #DDD; color: #000; }

    /* Content */
    .date { text-align: right; font-style: italic; }
    p, ul, ol, dl { padding: 0 5px 10px 5px; margin: 0; }
    ul, ol { margin-left: 35px; }
    .listHeader { padding-bottom: 0; margin-bottom: 0; font-style: italic; }
    pre { padding: 5px; background-color: #EEE; border-radius: 5px; white-space: pre-wrap; }

    /* API Stuff */
    .api { margin-bottom: 25px; }
    .api .title { font-size: 18px; font-weight: bold; }
    .api .method, .api .uri { display: inline-block; vertical-align: top; }
    .api .method { width: 75px; text-align: center; font-weight: bold; }
    .api .description { padding: 5px; }
    .api .example { font-style: italic; }
    .api .example::before { display: block; color: #999; padding: 5px; content: "Example: "; }
    .api .link { padding: 5px; background-color: #EEE; border-radius: 5px; margin: 2px 0; }
    .api dt { clear: both; margin-bottom: 5px; }
    .api dt, .api dd { display: inline-block; float: left; min-width: 100px; }
    .api dl::after { content: ""; clear: both; display: block; }

    @media (max-width: 675px) {
      #navigationItems, #data { display: block; width: 100%; }
      #data { padding: 5px 0 10px 0; }
    }
  </style>

  <div id="content">
    <div id="navigationItems" class="grayBox">
      <h2 class="header">IOER Developer Documentation</h2>
      <a href="/developers/">Documentation Home</a>
      <a href="/developers/rolemanagement">Identity Provider (IdP) and Role Management</a>
      <a href="/developers/learningregistry">Learning Registry</a>
      <a href="/developers/schemas">Metadata and Schemas Overview</a>
      <a href="/developers/metadata">IOER Metadata and Vocabularies</a>
      <a href="/developers/lrmi">Using LRMI</a>
      <a href="/developers/learningstandards">Learning Standards</a>
      <a href="/developers/rubrics">Resource Evaluation Rubrics</a>
			<a href="/developers/linkchecker">Checking for Dead, Inappropriate, and Malicious Links</a>
      <a href="/developers/paradata">Paradata Overview</a>
      <a href="/developers/sourcecode">IOER Source Code</a>
      <a href="/developers/elasticsearch">Elasticsearch</a>
      <a href="/developers/references">References</a>
      <div style="display:none;"><h2 class="midHeader">Illinois workNet API and Widgets</h2>
          <a href="/developers/articles">Articles</a>
          <a href="/developers/eventcalendar">Event Calendar</a>
          <a href="/developers/services">Services</a>
          <a href="/developers/trainingproviders">Training Providers</a>
          <a href="/developers/jobsearch">Indeed Job Search</a>
          <a href="/developers/bookmarks">Bookmarks</a>
      </div>
    </div><!--
    --><div id="data">
      <h1 class="isleH1"><%=Data.PageTitle %></h1>
      <p class="date">Last Updated <%=Data.UpdatedDate.ToShortDateString() %></p>
      <div id="dataContainer" class="dataContainer" runat="server"></div>
    </div>
  </div>

</asp:Content>