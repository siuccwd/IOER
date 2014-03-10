<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Splash2.ascx.cs" Inherits="ILPathways.Controls.Splash2" %>
<%@ Register TagPrefix="uc1" TagName="SplashMini" Src="/Controls/SplashMini.ascx" %>

<script type="text/javascript">
  var newestResourcesRaw = <%=newestResources %>;
  var followedResourcesRaw = <%=followedResources %>;
  var mostCommentedResourcesRaw = <%=mostCommentedResources %>;
</script>
<script type="text/javascript">
  var newestResources = [];
  var fileTypes = [
    /*{ match: ".doc", text: ".doc File" },
    { match: ".ppt", text: ".ppt File" },
    { match: ".xls", text: ".xls File" },
    { match: ".docx", text: ".docx File" },
    { match: ".pptx", text: ".pptx File" },
    { match: ".xlsx", text: ".xlsx File" },
    //{ match: ".pdf", text: ".pdf File" },*/
    { match: ".swf", text: ".swf File" },
    { match: "localhost", text: "Test Data" }
  ];
  var thumbIconTypes = [
    { match: ".pdf", header: "Adobe PDF", file: "/images/icons/filethumbs/filethumb_pdf_200x150.png" },
    { match: ".doc", header: "Microsoft Word Document", file: "/images/icons/filethumbs/filethumb_docx_200x150.png" },
    { match: ".docx", header: "Microsoft Word Document", file: "/images/icons/filethumbs/filethumb_docx_200x150.png" },
    { match: ".ppt", header: "Microsoft PowerPoint Document", file: "/images/icons/filethumbs/filethumb_pptx_200x150.png" },
    { match: ".pptx", header: "Microsoft PowerPoint Document", file: "/images/icons/filethumbs/filethumb_pptx_200x150.png" },
    { match: ".xls", header: "Microsoft Excel Spreadsheet", file: "/images/icons/filethumbs/filethumb_xlsx_200x150.png" },
    { match: ".xlsx", header: "Microsoft Excel Spreadsheet", file: "/images/icons/filethumbs/filethumb_xlsx_200x150.png" },
  ];
  $(document).ready(function () {
    renderResources(newestResourcesRaw, "newestResources");
    console.log("next");
    renderResources(followedResourcesRaw, "followedResources");
    console.log("next");
    renderResources(mostCommentedResourcesRaw, "commentedResources");
    console.log("next");
  });

  function renderResources(data, id) {
    try {
      var resources = data.hits.hits;
      var items = [];
      for(i in resources){
        items.push(resources[i]._source);
      }

      renderList(id, items);
    }
    catch(e){
      $("div[data-id=" + id + "]").hide();
    }
  }

  function renderList(target, list){
    var div = $("#" + target);
    if(list.length == 0){ 
      $("div[data-id=" + target + "]").hide();
    }
    else {
      div.html("");
      var template = $("#template_resource").html();
      var valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ _-";
      for(i in list){
        var fixedTitle = "";
        for(j in list[i].title){
          if(valid.indexOf(list[i].title[j]) >= 0){
            fixedTitle += list[i].title[j];
          }
        }
        fixedTitle = fixedTitle.replace(/ /g, "_");
        var useThumb = "//ioer.ilsharedlearning.org/OERThumbs/thumb/" + list[i].intID + "-thumb.png";
        for(j in thumbIconTypes){
          if(list[i].url.indexOf(thumbIconTypes[j].match) > -1){
            useThumb = thumbIconTypes[j].file;
          }
        }
        div.append(
          template.replace(/{detailURL}/g, "/IOER/" + list[i].versionID + "/" + fixedTitle)
          .replace(/{thumbURL}/g, useThumb )
          .replace(/{thumbSRC}/g, "img src=\"" + useThumb + "\"" )
          .replace(/{text}/g, list[i].title)
          .replace(/{id}/g, list[i].intID)
        );
        for(j in fileTypes){
          if(list[i].url.indexOf(fileTypes[j].match) > -1){
            div.find(".resource").last().find(".message").html(fileTypes[j].text);
          }
        }
      }
    }
  }

  function handleBrokenThumbnail(id){
    var target = $("#message_" + id);
    if(target.html().length == 0){
      $("#message_" + id).html("Generating Thumbnail");
    }
  }
</script>

<style type="text/css">
  /* Big Stuff */
  body { padding: 0; }
  #header { margin-bottom: 0; }
  #header #logoLink { height: 70px; }
  #content { min-width: 300px; background-color: #EEE; }
  #content, #content * { box-sizing: border-box; -moz-box-sizing: border-box; }
  .contentBox { transition: margin-left 1s; -webkit-transition: margin-left 1s; }
  .tealBox { background-color: #4AA394; background: linear-gradient(#4AA394, #5AB3A4); background: -webkit-linear-gradient(#4AA394, #5AB3A4); padding: 10px 0; }
  .interestBox { padding: 10px; }

  /* Specific Items */
  #content h1 { padding: 5px; font-size: 25px; text-align: center; margin: 5px 5px; color: #F5F5F5; background-color: #333; border-radius: 5px; }
  .interestBox h2 { font-size: 20px; color: #444; }
  #content p.bigText { padding: 15px; text-align: center; font-size: 24px; }
  p.bigText.white { color: #FFF; }
  #content h2 a { font-size: inherit; }
  #content h2 a:hover, #content h2 a:focus { text-decoration: underline; }

  /* Mini Splash Items */
  #links { transition: padding 1s; -webkit-transition: padding 1s; }
  #links .splashItem img { box-shadow: none; }
  #links .splashItem:hover img { background-color: #3572B8; }
  #links .splashItem a { box-shadow: none; }
  #links .splashItem a:hover, #links .splashItem a:focus { box-shadow: 0 0 15px #3572B8; background-color: #3572B8; }
  #links .splashItem h2 { background-color: #333; color: #FFF; box-shadow: none; font-size: 22px; }

  /* Resource Items */
  .resourcesBox { text-align: center; height: 230px; overflow: hidden; }
  .resource { 
    text-align: left; 
    border-radius: 5px; 
    border: 1px solid #CCC; 
    display: inline-block; 
    width: 18%; 
    vertical-align: top; 
    margin: 10px 1% 200px 0; 
    max-width: 200px; 
    white-space: nowrap; 
    overflow: hidden;
  }
  .resource .title { 
    height: 60px; 
    overflow: hidden; 
    text-align: center; 
    padding: 2px; 
    white-space: initial; 
    background-color: #FFF; 
    text-overflow: ellipsis; 
  }
  .resource .message { 
    text-align: center; 
    padding: 10px; 
    font-weight: bold; 
    font-size: 26px; 
    height: calc(100% - 56px);
    width: 100%;
    position: absolute;
    top: 0;
    left: 0;
    white-space: initial;
  }
  .resource a { display: block; width: 100%; height: 100%; position: relative; background: linear-gradient(#DFDFDF, #EFEFEF); background: -webkit-linear-gradient(#DFDFDF, #EFEFEF); }
  .resource img { width: 100%; margin-bottom: -4px; }
  .resource img.resizer { background-size: contain;  }
  .resource img.tester { display: none; }
  .resource:hover { box-shadow: 0 0 15px #FF5707; }

  /* Responsive */
  @media screen and (min-width: 980px) {
    .contentBox #links { padding: 5px 5%; }
    .contentBox { margin-left: 65px; }
  }
  @media screen and (max-width: 1350px) {
    #links .splashItem { width: 47%; }
  }
  @media screen and (max-width: 800px) {
    .resource { width: 30%; margin: 10px 2% 200px 0; }
  }
  @media screen and (max-width: 700px) {
    #links .splashItem { width: 98%; }
    #content h1 { font-size: 18px; }
  }
  @media screen and (max-width: 500px){
    .resource { width: 48%; margin: 10px 1% 200px 0; }
  }
</style>

<div id="content">
  <div class="tealBox">
    <div class="contentBox">
      <h1>Illinois Shared Learning Environment Open Education Resources</h1>
      <p class="bigText white">IOER provides you with one-click access to open, standards-aligned educational content. Use our tools to find, remix, and comment on resources for your personalized IOER learning library.</p>
      <div id="links">
        <uc1:SplashMini runat="server" ID="splashMini" useNewWindow="false" />
      </div>
    </div>
  </div>
  <div class="contentBox interestBox" data-id="followedResources">
    <h2>Latest Resources from Libraries I Follow:</h2>
    <div id="followedResources" class="resourcesBox"></div>
  </div>
  <div class="contentBox interestBox" data-id="newestResources">
    <h2><a href="/Search.aspx?sort=newest">Newest Resources:</a></h2>
    <div id="newestResources" class="resourcesBox"></div>
  </div>
  <div class="contentBox interestBox" data-id="commentedResources">
    <h2><a href="/Search.aspx?sort=comments">Most Talked About Resources:</a></h2>
    <div id="commentedResources" class="resourcesBox"></div>
  </div>
</div>

<div id="templates" style="display:none;">
  <div id="template_resource">
    <div class="resource">
      <a href="{detailURL}">
        <img src="/images/ThumbnailResizer.png" class="resizer" style="background-image:url('{thumbURL}');" />
        <img {thumbSRC} class="tester" onerror="handleBrokenThumbnail({id});" />
        <div class="message" id="message_{id}"></div>
        <div class="title">{text}</div>
      </a>
    </div>
  </div>
</div>