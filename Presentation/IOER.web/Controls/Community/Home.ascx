<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Home.ascx.cs" Inherits="IOER.Controls.Community.Home" %>

<link rel="stylesheet" type="text/css" href="/Styles/common2.css" />

<style type="text/css">
  #content > p { font-size: 20px; text-align: center; }
  .communityList { width: 18%; margin: 10px 1%; display: inline-block; vertical-align: top; margin-right: -4px; }
  .communityList a { display: block; padding: 2px;  font-size: 18px; }
  .communityList a:hover, .communityList a:focus { text-decoration: underline; }
  .communityList a:nth-child(2) { border-top-left-radius: 5px; border-top-right-radius: 5px; }
  .communityList a:last-child { border-bottom-left-radius: 5px; border-bottom-right-radius: 5px; }
</style>

<div id="content">
  <h1 class="isleH1">IOER Communities</h1>
  <p>Communities are a great way to keep in touch with people who share your interests!</p>

  <div id="communitiesList">
    <div class="grayBox communityList">
      <h2 class="header">General Communities</h2>
      <a href="/Community/1/IOER_Community">IOER Community</a>
    </div>

    <div class="grayBox communityList">
      <h2 class="header">Math</h2>
      <a href="/Communities/Community.aspx?id=2">Grades K-3 Math</a>
      <a href="/Community/1/IOER_Community">Grades 4-6 Math</a>
      <a href="/Community/1/IOER_Community">Grades 7-8 Math</a>
      <a href="/Community/1/IOER_Community">High School Math</a>
      <a href="/Community/1/IOER_Community">College Math</a>
      <a href="/Community/1/IOER_Community">Adult Education Math</a>
    </div>

    <div class="grayBox communityList">
      <h2 class="header">English</h2>
      <a href="/Community/1/IOER_Community">Grades K-2 English</a>
      <a href="/Community/1/IOER_Community">Grades 3-5 English</a>
      <a href="/Community/1/IOER_Community">Grades 6-8 English</a>
      <a href="/Community/1/IOER_Community">High School English</a>
    </div>

    <div class="grayBox communityList">
      <h2 class="header">Science</h2>
      <a href="/Community/1/IOER_Community">Grades K-5 Science</a>
      <a href="/Community/1/IOER_Community">Grades 6-8 Science</a>
      <a href="/Community/1/IOER_Community">High School Science</a>
      <a href="/Community/1/IOER_Community">Health Science</a>
      <a href="/Community/1/IOER_Community">Biology</a>
      <a href="/Community/1/IOER_Community">Physics and Chemistry</a>
    </div>

    <div class="grayBox communityList">
      <h2 class="header">Technology</h2>
      <a href="/Community/1/IOER_Community">Computers</a>
      <a href="/Community/1/IOER_Community">Gadgets</a>
      <a href="/Community/1/IOER_Community">Future Tech</a>
      <a href="/Community/1/IOER_Community">Classic Tech</a>
    </div>

  </div><!-- /communitiesList -->
</div>

