<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ILPathways.Organizations.UnityPoint.Default" MasterPageFile="/Masters/Responsive.Master" %>

<asp:Content ContentPlaceHolderID="HeadContent" runat="server"></asp:Content>
<asp:Content ContentPlaceHolderID="BodyContent" runat="server">

  <link rel="stylesheet" href="/Styles/common2.css" />
  <style type="text/css">
    p.big { font-size: 150%; margin: 20px 5%; }
    p.big.top { margin: 20px 2%; }
    a { font-size: inherit; }
    p a:hover { text-decoration: underline; }
    .icons { text-align: center; white-space: nowrap; }
    .icons a { display: inline-block; width: 32%; font-size: 160%; font-weight: bold; }
    .icons a img { display: block; width: 90%; max-width: 250px; margin: 0 auto; border-radius: 50%; }
    .icons a:hover img, .icons a:focus img { box-shadow: 0 0 20px -1px #4C98CC; background-color: #4C98CC; }
    #schoolLogo { float: left; max-width: 25%; margin: 10px; transition: margin 1s, width 1s, max-width 1s; -webkit-transition: margin 1s, width 1s, max-width 1s; }
    .clearfix { clear: both; }
    @media screen and (max-width: 800px) {
      #schoolLogo { max-width: 40%; }
    }
    @media screen and (max-width: 450px){
      #schoolLogo { max-width: 100%; width: 100%; float: none; display: block; margin: 10px auto; }
    }
  </style>
  <div class="content" id="content">
    
    <h1 class="isleH1" id="logoHeader">Welcome to Illinois Open Educational Resources!</h1>
    <p class="big top"><img src="logo.png" id="schoolLogo" />Thank you for allowing us to demonstrate IOER at your school. We have created this page to help you get started.</p>
    <p class="big top">Find more information about other IOER features with the <a href="/Help/Guide.aspx">IOER User Guide</a></p>

    <h2 class="isleH2 clearfix">Overview</h2>
    <p class="big">IOER's main goals are to help you:</p>

    <div class="icons">
      <a href="/OERThumbs/files/Search.pdf" id="iconSearch"><img src="/images/icons/icon_search_med.png" />Search</a>
      <a href="/OERThumbs/files/Libraries.pdf" id="iconSave"><img src="/images/icons/icon_library_med.png" />Save</a>
      <a href="/OERThumbs/files/Contribute.pdf" id="iconShare"><img src="/images/icons/icon_tag_med.png" />Share</a>
    </div>


    <p class="big">We'd love to hear from you! <a href="https://www.surveymonkey.com/s/TZ9VWK3">Tell us what you think!</a></p>

    <p class="big">Let's webinar! <a href="mailto:dyoung@siuccwd.com">Send Deb an Email</a></p>

  </div>

</asp:Content>