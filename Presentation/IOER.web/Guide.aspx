<%@ Page Language="C#" Title="Illinois Open Educational Resources Guide" AutoEventWireup="true" CodeBehind="Guide.aspx.cs" Inherits="IOER.Guide" MasterPageFile="/Masters/Responsive.Master" %>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">

<% 
  //Easy CSS colors
  string css_black      = "#4F4E4F";
  string css_red        = "#B03D25";
  string css_orange     = "#FF5707";
  string css_purple     = "#9984BD";
  string css_teal       = "#4AA394";
  string css_gray       = "#909297";
  string css_blue       = "#3572B8";
  string css_white      = "#E6E6E6";
  string css_lightblue  = "#4C98CC";
%>

<script type="text/javascript" src="/Scripts/slider2.js"></script>
<link rel="Stylesheet" type="text/css" href="/Styles/slider2.css" />

<script type="text/javascript" language="javascript">
  $(document).ready(function () {
    //$("#playlist li:first-child a").click();
  });
  function setVideo(source) {
    //$("#youtubePlayer").attr("src", $(source).attr("url"));
  }
</script>
<link rel="Stylesheet" type="text/css" href="/Styles/common.css" />
<style type="text/css">
#slider_PDFs {
  width: 1000px;
  height: 150px;
  margin: 10px auto;
  border-radius: 15px;
  overflow: hidden;
}
#slider_PDFs div.img {
  height: 100px;
}
.sliderArrow {
  width: 30px;
  color: #FFF;
  background-color: #4C98CC;
}
.sliderArrow:hover {
  color: #FFF;
  background-color: #FF5707;
}
#player {
  width: 1000px;
  box-sizing: border-box;
  -moz-box-sizing: border-box;
  -webkit-box-sizing: border-box;
  margin: 10px auto;
  padding: 0;
}
#youtubePlayer, #playlist {
  height: 400px;
  display: inline-block;
  *display: inline;
  zoom: 1;
  vertical-align: top;
}
#youtubePlayer {
  width: 600px;
  margin: 0 10px 10px 10px;
  padding: 0;
  float: right;
}
p {
  margin: 10px;
}
.surveys {
  display: inline-block;
  *display: inline;
  zoom: 1;
  width: 375px;
  margin-right:-10px;
  margin-bottom: 15px;
}
.surveys ul {
  list-style-type: none;
}
.surveys li {
  margin-left: -3px;
}
.surveys li img {
  vertical-align: middle;
}
h2 { padding-left: 10px; }
</style>
<style type="text/css">
  .mainContent { transition: padding-left 1s; -webkit-transition: padding-left 1s; }
  @media screen and (min-width: 980px) {
    .mainContent { padding-left: 50px; }
  }
</style>

<div class="mainContent">
<h1 class="isleH1">ISLE Open Educational Resources Usage Guide</h1>

<iframe id="youtubePlayer" src="//www.youtube.com/embed/videoseries?list=PLZG3yuusmGNzdCti7BIUyVcA5Z4UGPMPG" frameborder="0" allowfullscreen></iframe>

<h2>Guide</h2>
<p>The ISLE Open Educational Resources toolset is a series of utilities to help teachers and students find, create, and evaluate Learning Resources.</p>
<p>The IOER Tools leverage the <a href="http://learningregistry.org/" class="textLink" target="_blank">Learning Registry</a>, the <a href="http://www.corestandards.org/" class="textLink" target="_blank">Common Core State Standards</a>, the <a href="http://www.nextgenscience.org/" class="textLink" target="_blank">Next Generation Science Standards</a>, and other sources of high-quality, useful data.</p>
<p>Use the videos and documents here to learn the basics of using the IOER tools.</p>

<div class="isleBox surveys">
  <h2 class="isleBox_H2">Tell us what you think!</h2>
  <ul>
    <li><img src="/images/icons/icon_search.png" /><a target="_blank" href="https://www.surveymonkey.com/s/searchingIOER">Searching, Narrowing, and Filtering</a></li>
    <li><img src="/images/icons/icon_library.png" /><a target="_blank" href="https://www.surveymonkey.com/s/creatingIOER">Creating, Sharing, and Following Library Collections</a></li>
    <li><img src="/images/icons/icon_tag.png" /><a target="_blank" href="https://www.surveymonkey.com/s/tagandpublishIOER">Tagging and Publishing Resources</a></li>
    <li><img src="/images/icons/icon_create.png" /><a target="_blank" href="https://www.surveymonkey.com/s/authorIOER">Authoring/Creating new Educational Resource Web Pages</a></li>
    <li><img src="/images/icons/icon_ratings.png" /><a target="_blank" href="https://www.surveymonkey.com/s/evaluatingIOER">Evaluating Resource CCSS Alignment with EQuIP Rubric</a></li>
    <li><img src="/images/icons/icon_app.png" /><a target="_blank" href="https://www.surveymonkey.com/s/technologyusedIOER">Technologies Used</a></li>
  </ul>
</div>

<h2>Sandbox</h2>
<p>The <a href="http://sandbox.ilsharedlearning.org/" target="_blank">IOER Sandbox</a> is an independent environment that mirrors the functionality found on this site. It exists to allow users to practice creating and tagging Resources. Note that you won't find much in the Sandbox's search!</p>

<div style="clear:both;"></div>

<div id="slider_PDFs"></div>

<div id="sliderData" style="display:none">
  var sliders = [
    {
      sliderID: "slider_PDFs",
      itemsToDisplay: 3,
      shiftCount: 2,
      animationTime: 2000,
      useTextArrows: true,
      autoAdvance: 5000,
      items: [
        { link: "large/{intID}-large.png"/files/feast/ISLE OER Library Guide July 2013 FINALPrint.pdf", img: { header: "Adobe PDF", text: "PDF" }, text: "Library Guide" },
        { link: "//ioer.ilsharedlearning.org/OERThumbs/files/feast/ISLE OER Overview July 2013-FINALPrint.pdf", img: { header: "Adobe PDF", text: "PDF" }, text: "Overview" },
        { link: "//ioer.ilsharedlearning.org/OERThumbs/files/feast/ISLE OER Tagging Guide July 2013-FINALPrint.pdf", img: { header: "Adobe PDF", text: "PDF" }, text: "Tagging Guide" },
        { link: "//ioer.ilsharedlearning.org/OERThumbs/files/feast/ISLE OER Authoring Guide July 2013-FINALPrint.pdf", img: { header: "Adobe PDF", text: "PDF" }, text: "Authoring Guide" },
        { link: "//ioer.ilsharedlearning.org/OERThumbs/files/stem/ISLEOERUpdateSTEMLESept_18_2013.pdf", img: { header: "Adobe PDF", text: "PDF" }, text: "STEM Update" },
      ]
    }
  ];
</div>

</div>
</asp:Content>