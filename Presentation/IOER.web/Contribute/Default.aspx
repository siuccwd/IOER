<%@ Page Language="C#" Title="IOER Contribute" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ILPathways.Contribute.Default" MasterPageFile="/Masters/Responsive.Master" %>  
<%@ Register TagPrefix="uc1" TagName="QuickContribute" Src="/Controls/QuickContribute.ascx" %>

<asp:Content ContentPlaceHolderID="BodyContent" runat="server">
  <div id="pnlIntro" runat="server">
    <script type="text/javascript">

    </script>
    <style type="text/css">
      /* Big Stuff */
      #content { transition: padding-left 1s; -webkit-transition: padding-left 1s; min-width: 300px; }
      h1 {  text-align: center; }
      .intro { font-size: 24px; text-align: center;  }
      #paths { margin: 0 10%; text-align: center; }
      .path { text-align: left; margin: 0 auto 25px auto; padding: 5px; clear: both; min-height: 125px; max-width: 1000px; width: 48%; display: inline-block; vertical-align: top; }
      .path img { float: left; margin: 0 10px 5px 0; width: 14%; background-color: #4AA394; border-radius: 50%; }
      #FooterSection { clear: both; }
      .path h2 { font-size: 20px; color: #FFF; background-color: #4AA394; border-radius: 5px; margin-left: 7%; }
      .path p { font-size: 16px; }
      .path .goLink { display: block; text-align: right; font-size: 20px; font-weight: bold; text-decoration: none; }
      .path .goLink:hover, .path .goLink:focus { color: #FF5707; }
      .clear { clear: both; }

      @media screen and (min-width: 980px) {
        #content { padding-left: 50px; }
      }
      @media screen and (max-width: 800px) {
        .path { width: 100%; }
      }
    </style>

    <div id="content">
      <h1 class="isleH1">Contribute Open Education Resources!</h1>
      <p class="intro">There are a few ways to add your Resources to the IOER Repository. Take a look:</p>

      <div id="paths">
        <div class="path" id="quick">
          <img src="/images/icons/icon_swirl_med.png" />
          <h2>Quick Tag</h2>
          <p>Submit a webpage or a file that is already hosted online, tag it with basic information, and enhance your tags later.</p>
          <a class="goLink" href="?mode=tag">Tag Now &rarr;</a>
          <div class="clear"></div>
        </div>
        <div class="path" id="quick">
          <img src="/images/icons/icon_upload_med.png" />
          <h2>Quick Upload</h2>
          <p>Upload a file, tag it with basic information, and enhance your tags later.</p>
          <a class="goLink" href="?mode=upload">Upload Now &rarr;</a>
          <div class="clear"></div>
        </div>
        <div class="path" id="curriculum">
          <img src="/images/icons/icon_standards_med.png" />
          <h2>Create a Learning List</h2>
          <p>Group related resources and files together. Works for small lessons, but is ideal for full curricula, and everything in between.</p>
          <a class="goLink" href="/My/LearningList/new">Go to Learning List Builder &rarr;</a>
          <div class="clear"></div>
        </div>
        <div class="path" id="author">
          <img src="/images/icons/icon_create_med.png" />
          <h2>Create a New Resource</h2>
          <p>Easily create a simple webpage and attach multiple files to it with this tool.</p>
          <a class="goLink" href="/My/Author.aspx">Go to Authoring Tool &rarr;</a>
          <div class="clear"></div>
        </div>
        <div class="path" id="publish">
          <img src="/images/icons/icon_tag_med.png" />
          <h2>Tag an Online Resource</h2>
          <p>Want to thoroughly tag a website or a file that's already hosted online? Start here.</p>
          <a class="goLink" href="/Publish.aspx">Go to Tagging Tool &rarr;</a>
          <div class="clear"></div>
        </div>
      </div>
    </div>
  </div>
  <uc1:QuickContribute runat="server" id="contributer" Visible="false" />
</asp:Content>