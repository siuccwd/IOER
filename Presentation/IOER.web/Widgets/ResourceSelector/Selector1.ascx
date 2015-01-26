<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Selector1.ascx.cs" Inherits="ILPathways.Widgets.ResourceSelector.Selector1" %>

<link rel="stylesheet" type="text/css" href="/Styles/ISLE.css" />
<link rel="stylesheet" type="text/css" href="/Styles/common2.css" />
<script src="//ajax.googleapis.com/ajax/libs/jquery/1.10.2/jquery.min.js" type="text/javascript"></script>
<script type="text/javascript">
  var baseURL = "http://localhost:2012/";
  var collapsed = false;

  $(document).ready(function() {
    $("#ioerFrame").attr("src", baseURL);
    $(window).on("resize", function() {
      resizeList();
    }).trigger("resize");
  });

  var resources = [];

  function addResource(data) {
    var found = false;
    for(i in resources){
      if(resources[i].id == data.id){
        found = true;
        break;
      }
    }
    if(!found){
      resources.push(data);
      renderList();
    }
    else {
      alert("You already added " + data.title + "." );
    }
  }

  function renderList() {
    var box = $("#resourceList");
    var template = $("#template_resource").html();
    box.html("");
    for (i in resources) {
      box.append(
        template
          .replace(/{id}/g, resources[i].id)
          .replace(/{thumburl}/g, resources[i].thumbImage)
          .replace(/{message}/g, resources[i].thumbMessage)
          .replace(/{title}/g, resources[i].title)
          .replace(/{version}/g, resources[i].version)
      );
    }
  }

  function resizeList() {
    var targetHeight = $("#frameHeader").outerHeight() - $("#resourceList").position().top - 15;
    $("#resourceList").height(targetHeight);
    if($(window).outerWidth() <= 500){
      $("#frameHeader").css("margin-left", "0");
      $("#frameContainer").css("padding-left", "0");
      $("#collapseButton").css("background-image", "url('/images/arrow-left-offwhite.png')");
      collapsed = false;
    }
    else {
      collapsed = true;
      toggleCollapse();
    }
  }

  function hasSelector() { }

  function viewResource(id){
    $("#ioerFrame").attr("src", baseURL + "IOER/" + id);
  }

  function removeResource(id){
    var fresh = [];
    for(i in resources){
      if(resources[i].id != id){
        fresh.push(resources[i]);
      }
    }
    resources = fresh;
    renderList();
  }

  function finish() {
    var sendData = [];
    var test = "";
    for(i in resources){
      sendData.push(resources[i].id);
      test += resources[i].id + ",";
    }
    test = test.substr(0, test.length - 1);
    alert("The following list of IDs is sent back to the original site:\r\n" + test);
  }

  function showHideTips() {
    $("#frameHeader p").slideToggle(function() {
      $(window).trigger("resize"); 
    });
  }

  function toggleCollapse() {
    if(collapsed){
      $("#frameHeader").css("margin-left", "0");
      $("#frameContainer").css("padding-left", "250px");
      $("#collapseButton").css("background-image", "url('/images/arrow-left-offwhite.png')");
      collapsed = false;
    }
    else {
      $("#frameHeader").css("margin-left", "-225px");
      $("#frameContainer").css("padding-left", "25px");
      $("#collapseButton").css("background-image", "url('/images/arrow-right-offwhite.png')");
      collapsed = true;
    }
  }
</script>

<style type="text/css">
  html, body, form, * { margin: 0; padding: 0; box-sizing: border-box; -moz-box-sizing: border-box; }
  html, body, form { width: 100%; height: 100%; }
  #ioerFrame { margin-bottom: -5px; }
  #frameContainer { min-width: 300px; width: 100%; height: 100%; padding-left: 250px; position: relative; transition: padding 1s; }
  #ioerFrame { border: none; width: 100%; height: 100%; }
  #frameHeader { position: absolute; top: 0; left: 0; width: 250px; border-radius: 0; height: 100%; box-shadow: 0 0 25px -5px #000; background: linear-gradient(#DFDFDF, #EFEFEF); background: -webkit-linear-gradient(#DFDFDF, #EFEFEF); transition: margin 1s; }
  .grayBox h2.header { border-radius: 0; }
  .grayBox h2.mid { background-color: #CCC; }
  #resourceList { padding: 5px 2px; box-shadow: 0 0 20px -5px #333 inset; border-radius: 10px; overflow-y: scroll; }
  .resource { box-shadow: 0 0 20px -5px #000; margin: 5px 2px 10px 2px; padding: 2px; border-radius: 5px; background: linear-gradient(#CCC, #EFEFEF); background: -webkit-linear-gradient(#CCC, #EFEFEF); }
  .resource .thumbContainer, .resource .buttons { display: inline-block; width: 50%; margin-right: -4px; vertical-align: top; }
  .resource .thumbContainer { border-radius: 5px; overflow: hidden; background-size: cover; }
  .resource .thumbContainer img { width: 100%; }
  .resource .buttons { padding: 5px 0 0 2px; }
  .resource .buttons input { margin-bottom: 5px; }
  #hideLink { display: block; text-align: center; margin-bottom: 10px; }
  .title { text-overflow: ellipsis; overflow: hidden; }
  #collapseButton { width: 27px; height: 27px; position: absolute; top: 0; right: 0; background: url('/images/arrow-left-offwhite.png') no-repeat center center; background-size: contain; background-color: #FF6A00; }

  @media screen and (max-width: 500px){
    #frameHeader, #ioerFrame { width: 100%; display: block; }
    #frameHeader { transition: margin 0s; height: 400px; }
    #frameContainer { padding: 400px 0 0 0; transition: padding 0s; height: 100%; }
    #ioerFrame { height: 100%; min-height: 400px; }
    #collapseButton { display: none; }
  }
</style>

<div id="frameContainer">
  <div id="frameHeader" class="grayBox">
    <a href="#" id="collapseButton" onclick="toggleCollapse()"></a>
    <h2 class="header">IOER Resource Selector</h2>
    <p>As you use the IOER site, you can add Resources to send back to the site you came from!</p>
    <p>To add Resources, use the "Send Resource to External Site" button on a Resource's detail page.</p>
    <p>When you're finished, click the Finish button below to send the selected Resources to the other site.</p>
    <a id="hideLink" class="textLink" href="#" onclick="showHideTips()">Show/Hide Tips</a>
    <input type="button" class="isleButton bgGreen" value="Finish" onclick="finish()" />
    <h2 class="mid">Resources:</h2>
    <div id="resourceList"></div>
  </div>
  <iframe id="ioerFrame" src=""></iframe>
</div>

<div id="templates" style="display: none;">
  <div id="template_resource">
    <div class="resource" data-id="{id}">
      <div class="topHalf">
        <div class="thumbContainer" style="background-image:url('{thumburl}')">
          <img src="/images/ThumbnailResizer.png" />
          <div class="message">{message}</div>
        </div>
        <div class="buttons">
          <input type="button" class="isleButton bgGreen" value="View" onclick="viewResource({version})" />
          <input type="button" class="isleButton bgRed" value="Remove" onclick="removeResource({id})" />
        </div>
      </div>
      <div class="title">{title}</div>
    </div>
  </div>
</div>