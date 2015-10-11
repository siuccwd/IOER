<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Activity1.ascx.cs" Inherits="IOER.Controls.Activity1" %>


<script type="text/javascript">
  <%=userGUID %>
  <%=ranges %>
</script>
<script type="text/javascript">
  var icons = {
    news: "/images/icons/icon_swirl_med.png",
    comment: "/images/icons/icon_comments_med.png",
    resource: "/images/icons/icon_resources_med.png",
    collection: "/images/icons/icon_library_med.png",
    communityposting: "/images/icons/icon_community_med.png",
    like: "/images/icons/icon_likes_med.png",
    evaluation: "/images/icons/icon_ratings_med.png",
    following: "/images/icons/icon_click-throughs_med.png"
  };

  $(document).ready(function () {
    renderRanges(false);
    show("all");
    var search = window.location.search;
    if(search.length > 0){
      show(search.split("=")[1]);
    }

    showTab("message", $(".contributeTabs a").first()[0]);

    //Fix for Safari/mobile
    if (navigator.userAgent.indexOf("5.1.7 Safari") > -1 || navigator.userAgent.indexOf("5.0 Safari") > -1 || navigator.userAgent.indexOf("4.0 Mobile Safari") > -1) {
      setTimeout(function() {
        $(".event .basics, .event .details, .event .details .likeBox input, .event .details .likeBox .dislikeBar, .event .details .likeBox .likeBar").css("margin-right","-4px");
      }, 3000);
      //setInterval(doSafariFixes, 15000);
      $(window).on("resize", function() { 
        doSafariFixes(); 
        setTimeout( doSafariFixes, 1000 ); 
        setTimeout( doSafariFixes, 2000 );
      }).trigger("resize");
    }

  });

  function doSafariFixes() {
    $(".event .details .likeBox .likeBarContainer").css("width", "100%").css("width","-=75px");
    $(".event .details .share input").css("width", "100%").css("width", "-=50px");
    if($(window).width() <= 700){
      $(".event .details").css("width", "100%");
    }
    else {
      $(".event .details").css("width", "100%").css("width", "-=200px");
    }
  }

  function show(target) {
    if (target == "all") {
      $(".event").attr("data-showing", "true");
      $(".separator").attr("data-showing", "true");
      $(".range").attr("data-showing", "true");
      $("#nothing").attr("data-showing", "false");
    }
    else {
      var eligible = $(".event[data-type=" + target + "]");
      eligible.attr("data-showing", "true");
      $(".event").not("[data-type=" + target + "]").attr("data-showing", "false");

      $(".range").each(function () {
        var thisRange = $(this);
        if (thisRange.find(".event[data-type=" + target + "]").length > 0) {
          thisRange.find(".separator").attr("data-showing", "true");
          thisRange.attr("data-showing", "true");
        }
        else {
          thisRange.find(".separator").attr("data-showing", "false");
          thisRange.attr("data-showing", "false");
        }
      });

      if (eligible.length > 0) {
        $("#nothing").attr("data-showing", "false");
      }
      else {
        $("#nothing").attr("data-showing", "true");
      }
    }
  }

  function renderRanges(clear) {
    var timeline = $("#timeline");
    if (clear) {
      timeline.html("");
    }
    for (i in ranges) {
      var range = renderRange(ranges[i]);

      //Append the box
      timeline.append(range);
    }

    $("input[type=text]").scrollLeft($(this).width());
    $(".likeBox").each(function() {
      renderLikes($(this));
    });
  }

  function renderRange(data){
    //Add separator
    var separator = $("#template_separator").html().replace(/{date}/g, ranges[i].startDate);
    var range = $("<div></div>");
    range.attr("class", "range");
    range.append(separator);

    //Render items
    for (i in data.events) {
      range.append(renderEvent(data.events[i], range));
      if(data.hasRated){
        replaceLikeButton(range.find(".event").last().find(".likeBox"), "Rated!");
      }
    }

    return range;
  }

  function renderEvent(data, range) {
    return $("#template_event").html()
      .replace(/{icon}/g, icons[data.item.type])
      .replace(/{type}/g, data.item.type)
      .replace(/{item}/g, data.actionTitle)
      .replace(/{action}/g, data.actionType)
      .replace(/{actor}/g, data.actor.title)
        .replace(/{userId}/g, data.actor.id)
      .replace(/{locationType}/g, data.location.type)
      .replace(/{location}/g, data.location.title)
      .replace(/{date}/g, data.date)
      .replace(/{details}/g, getDetails(data))
      .replace(/{locationImage}/g, data.location.thumbnail)
        .replace(/{extraClass}/g, data.location.extraClass)
      .replace(/{link}/g, data.location.link)
      .replace(/{actorAvatar}/g, data.actor.thumbnail);
    }

  function getDetails(data) {
    if (data.item.type == "like"){ 
      return ""; 
    }
    if (data.item.type == "news") {
      //Do news posting
      return $("#template_news").html()
        .replace(/{title}/g, data.item.title)
        .replace(/{description}/g, data.item.description)
        .replace(/{link}/g, data.item.link);

    }
    else if (data.item.type == "comment") {
      //Do comment posting
      return $("#template_comment").html()
        .replace(/{description}/g, data.item.description)
        .replace(/{link}/g, data.item.link);
    }
    else if (data.item.type == "evaluation") {
        //Do evaluation
        return $("#template_evaluation").html()
            .replace(/{location}/g, data.location.title)
            .replace(/{location_description}/g, data.location.description)
            .replace(/{title}/g, data.item.title)
            .replace(/{description}/g, data.item.description)
            .replace(/{thumb}/g, data.item.thumbnail)
            .replace(/{link}/g, data.item.link)
            .replace(/{id}/g, data.item.id)
            .replace(/{likes}/g, data.likes)
            .replace(/{dislikes}/g, data.dislikes);
    }
    else if (data.item.type == "communityposting") {
        //Do posting
        return $("#template_communityposting").html()
          .replace(/{description}/g, data.item.description)
          .replace(/{link}/g, data.item.link);
    }
    else if (data.item.type == "resource") {
      //Do resource posting
      return $("#template_resource").html()
        .replace(/{title}/g, data.item.title)
        .replace(/{description}/g, data.item.description)
        .replace(/{thumb}/g, data.item.thumbnail)
        .replace(/{link}/g, data.item.link)
        .replace(/{id}/g, data.item.id)
        .replace(/{likes}/g, data.likes)
        .replace(/{dislikes}/g, data.dislikes);
      }
    else if (data.item.type == "collection") {
      //Do collection posting
      return $("#template_collection").html()
        .replace(/{title}/g, data.item.title)
        .replace(/{description}/g, data.item.description)
        .replace(/{thumb}/g, data.item.thumbnail)
        .replace(/{link}/g, data.item.link)
        .replace(/{id}/g, data.item.id)
        .replace(/{likes}/g, data.likes)
        .replace(/{dislikes}/g, data.dislikes);
      }
  }

    //Like something
    //Warning used for multiple purposes
  function like(button, type, id){
    if(userGUID == ""){
      alert("Please login to Like!");
      return;
    }

    doDalAjax("Like", { userGUID: userGUID, type: type, id: id }, successLike, $(button));
  }

  function doDalAjax(method, data, success, button){
    $.ajax({
      url: "/Services/WebDALService.asmx/" + method,
      async: true,
      success: function (msg) {
        try {
          success($.parseJSON(msg.d), data, button);
        }
        catch (e) {
          success(msg.d, data, button);
        }
      },
      type: "POST",
      data: JSON.stringify(data),
      dataType: "json",
      contentType: "application/json; charset=utf-8"
    });
  }

  function successLike(data, item, button){
    console.log(data);
    console.log(typeof(data));
    console.log(item);
    if(typeof(data) == "string"){
      data = $.parseJSON(data);
    }
    if(data.isValid){
      var target = $(".event[data-type=" + item.type + "] .likeBox[data-id=" + item.id + "]");
      target.find(".likeBarText").html(data.data);

      renderLikes(target);

      replaceLikeButton(target, "Liked!");
    }
    else {
      if(typeof(data.status) == "undefined"){
        return;
      }
      alert(data.status);
    }
    unlockButton(button);
  }

  function renderLikes(target){
    var likes = parseInt(target.find(".likeBarText").html());
    var dislikes = parseInt(target.find(".dislikeBarText").html());
      
    var likePercent = ((likes / (likes + dislikes)) * 100);
    target.find(".likeBar").css("width", likePercent + "%");
    target.find(".dislikeBar").css("width", 100 - likePercent + "%");

    if(likePercent == 100){
      target.find(".likeBar").addClass("full");
    }
    else {
      target.find(".likeBar").removeClass("full");
    }

  }

  function lockButton(button){
    if(button.length > 0){
      button.attr("text", button.attr("value"));
      button.attr("disabled","disabled");
      button.attr("value","...");
    }
  }
  function unlockButton(button){
    if(button.length > 0){
      button.attr("disabled",false);
      button.attr("value", button.attr("text"));
    }
  }
  function replaceLikeButton(box, text){
    var button = box.find("input");
    button.replaceWith("<p class=\"replacementButton\">" + text + "</p>");
    box.find(".likeBarContainer").css("border-radius", "5px");
    box.find(".likeBar").css("border-radius", "5px");
  }
</script>

<script>
  //Page functions
    function postMessage(button) {
    
        var communityID = parseInt($(".communitiesList option:selected").val());
        if (communityID == 0) {
            $("#commentSubmitResult").html("You must select a community before posting a message!");
            return;
        }
        var val = $("#txtComment").val();
        if (val.length > 0) {
            doAjax("PostMessageFromTimeline", { userGUID: userGUID, communityID: communityID, parentID: 0, text: val, isMyTimeline: (window.location.href.indexOf("my") > -1) }, successPostMessage, $(button));
        }
        else {
          $("#commentSubmitResult").html("You cannot post an empty message!");
        }
      }

  function showTab(target, tab){
    $(".contributeTabs a").removeClass("selected");
    $(tab).addClass("selected");
    $(".contributeTab").hide();
    $(".contributeTab[data-tabID=" + target + "]").show();
  }

  //AJAX stuff
  function doAjax(method, data, success, button) {
    disableButton(button);
    $.ajax({
      url: "/Services/CommunityService.asmx/" + method,
      async: true,
      success: function (msg) {
        try {
          success($.parseJSON(msg.d));
        }
        catch (e) {
          success(msg.d);
        }
        enableButton(button);
      },
      type: "POST",
      data: JSON.stringify(data),
      dataType: "json",
      contentType: "application/json; charset=utf-8"
    });
  }

  function disableButton(button) {
    if(button == null){ return; }
    button.attr("originalVal", button.attr("value"));
    button.attr("value", "...");
    button.attr("disabled", "disabled");
  }
  function enableButton(button) {
    if(button == null){ return; }
    button.attr("value", button.attr("originalVal"));
    button.removeAttr("disabled");
  }

  function successPostMessage(data) {
    if (data.isValid) {
      ranges = data.data;
      $("#commentSubmitResult").html("Posted!");
      $("#txtComment").val("");
      renderRanges(true);
      $("#commentSubmitResult").val("");
    }
    else {
      $("#commentSubmitResult").html(data.status);
    }
  }

</script>

<style type="text/css">
  /* Big Stuff */
  * { box-sizing: border-box; -moz-box-sizing: border-box; -webkit-box-sizing: border-box; }
  #content { transition: padding 1s; -webkit-transition: padding 1s; min-width: 300px; }
  .sectionBox { padding: 10px; text-align: center; border-radius: 5px; margin: 0 auto 25px auto; max-width: 500px;  }
  /* Top Box */
  #topBox { background-color: #EEE; padding: 10px; text-align: center; border-radius: 5px; margin: 0 auto 25px auto; max-width: 1000px; border-top: 1px solid #4AA394; }
  #topBox #filtering a { display: inline-block; vertical-align: top; background-color: #3572B8; color: #FFF; text-align: center; padding: 5px; }
  #topBox #filtering a:first-child { border-radius: 5px 0 0 5px; }
  #topBox #filtering a:last-child { border-radius: 0 5px 5px 0; }
  #topBox #filtering a:hover, #topBox #filtering a:focus { background-color: #FF5707; }
  /* Contribute */
  #topBox .contribute { text-align: center; }
  #topBox .contribute .contributeLabel { display: inline-block; width: 32%; padding: 5px; color: #FFF; }
  #topBox .contribute #txtComment { width: 100%; resize: none; height: 4em; border: 1px solid #4AA394; }
  #topBox .contribute span.contributeLabel { background-color: #4AA394; border-top-left-radius: 5px; }
  #topBox .contribute a.contributeLabel { background-color: #3572B8; }
  #topBox .contribute a.rounded { border-top-right-radius: 5px; }
  #topBox .contribute a.contributeLabel:hover, #topBox .contribute a.contributeLabel:focus, #topBox .contribute #btnSubmit:hover, #topBox .contribute #btnSubmit:focus { background-color: #FF5707; cursor: pointer; }
  #topBox .contribute #commentSubmitResult { display: inline-block; width: 78%; vertical-align: top; min-height: 1.2em; text-align: right; }
  #topBox .contribute #btnSubmit { width: 20%; background-color: #4AA394; color: #FFF; display: inline-block; vertical-align: top; border-radius: 5px; }

  .contributeTab { margin-bottom: 10px; }
  .contributeTabs { margin: 0 auto; max-width: 1000px; padding-left: 5px; }
  .contributeTabs a { display: inline-block; padding: 2px 5px; background-color: #3572B8; color: #FFF; border-radius: 5px 5px 0 0; }
  .contributeTabs a.selected { background-color: #4AA394; }
  .contributeTabs a:hover, .contributeTabs a:focus { background-color: #FF5707; }

  .wayToContribute { display: inline-block; vertical-align: top; width: 24%; position: relative; padding-left: 15px; margin-bottom: 5px; min-width: 100px; }
  .wayToContribute h4 { border-radius: 5px; background-color: #4AA394; color: #FFF; padding: 2px 5px 2px 20px; }
  .wayToContribute img { position: absolute; top: 0; left: 0px; border-radius: 50%; background-color: #4AA394; }
  .wayToContribute p { padding-left: 15px; text-align: left; }
  .wayToContribute a { display: block; font-weight: bold; text-align: right; }

  /* Ranges */
  .range { margin-bottom: 30px; max-width: 1200px; margin: 0 auto 50px auto; }
  .range .separator, #nothing { text-align: center; margin: 5px 10% 15px 10%; padding: 5px; background-color: #EEE; border-radius: 5px; font-weight: bold; font-size: 20px; }

  /* Events */
  .event, .separator, .range, #nothing { transition: height 1s, opacity 1s, padding 1s, margin 1s; -webkit-transition: height 1s, opacity 1s, padding 1s, margin 1s; height: auto; opacity: 1; }
  .event[data-showing=false], .separator[data-showing=false], .range[data-showing=false], #nothing[data-showing=false] { height: 0; opacity: 0; margin-top: 0; margin-bottom: 0; padding-top: 0; padding-bottom: 0; min-height: 0; }
  .event { background-color: #EEE; border-radius: 5px; position: relative; margin-left: 25px; padding-left: 25px; font-size: 0; margin-bottom: 10px; height: auto; opacity: 1; min-height: 50px; }
  .event .icon { width: 50px; position: absolute; top: 0; left: -25px; background-color: #EEE; border-radius: 50%; }
  .event .basics, .event .details { display: inline-block; vertical-align: top; font-size: 16px; }
  .event .basics { width: 200px; padding: 5px; }
  .event .basics h2 { font-size: 20px; min-height: 50px; }
  .event .locationThumbnail img { width: 75px; }
  .event .locationThumbnail div { width: calc(100% - 80px); padding-left: 5px; overflow: hidden; text-overflow: ellipsis; }
  .event .locationThumbnail img, .event .locationThumbnail div { display: inline-block; vertical-align: middle; }
  .event .date { position: absolute; top: 5px; right: 5px; }
  .event .details { width: calc(100% - 200px); padding: 5px; }
  .event .details .thumb { display: inline-block; vertical-align: top; width: 125px; max-height: 125px; overflow: hidden; float: left; margin-right: 10px; }
  .event .details .thumb img { width: 100%; }
  .event .details h2 { font-size: 20px; }
  .event .details h2, .event[data-type=comment] .details { padding-right: 75px; }
  .event .details .shareLike { padding-left: 130px; }
  .event .details .share, .event .details .likeBox { width: 48%; display: inline-block; }
  .event .details .share label { display: inline-block; width: 40px; }
  .event .details .share input { display: inline-block; width: calc(100% - 45px); }
  .event a { font-size: inherit; }
  .event .actorAvatar { max-height: 75px; width: 50px; display: inline-block; vertical-align: top; float: left; margin: 0 5px 5px 0; }
  .event .locationThumbnail div { width: calc(100% - 55px); }
  .event .locationThumbnail img { width: 50px; max-height: 75px; }
  .event .basics h2, .event .basics p, .event .basics a { display: inline-block; vertical-align: top; width: 100%; }
  .event .basics p { padding-left: 55px; width: 100%; }

  /* Type-based stuff */
  .event[data-type=like] .basics { width: 100%; }
  .event[data-type=like] .details { display: none; }

  .event[data-type=comment] .basics { width: 100%; }
  .event[data-type=comment] .details { width: 100%; display: block; }

  .event[data-type=following] .basics { width: 100%; }
  .event[data-type=following] .details { display: none; }

  .event[data-type=like] .basics h2, 
  .event[data-type=comment] .basics h2, 
  .event[data-type=following] .basics h2, 
  .event[data-type=like] .basics p, 
  .event[data-type=comment] .basics p, 
  .event[data-type=following] .basics p, 
  .event[data-type=like] .basics a, 
  .event[data-type=comment] .basics a, 
  .event[data-type=following] .basics a 
  { display: inline-block; width: auto; vertical-align: middle; }
  .event[data-type=like] .basics h2, .event[data-type=comment] .basics h2 { line-height: 50px; }
  .event[data-type=like] .basics p, .event[data-type=comment] .basics p, .event[data-type=following] .basics p { padding: 0; }
  .event[data-type=like] .basics a div, .event[data-type=comment] .basics a div, .event[data-type=following] .basics a div { width: auto; }

    /* duplicate comments for communityposting for now ==> do responsive later */
    .event[data-type=communityposting] .basics { width: 100%; }
  .event[data-type=communityposting] .details { width: 100%; display: block; }
  .event[data-type=communityposting] .basics h2, 
  .event[data-type=communityposting] .basics p, 
  .event[data-type=communityposting] .basics a
            { display: inline-block; width: auto; vertical-align: middle; }
  .event[data-type=communityposting] .basics h2 { line-height: 50px; }
  .event[data-type=communityposting] .basics p { padding: 0; }
  .event[data-type=communityposting] .basics a div{ width: auto; }


  /* Likes and Dislikes */
  .event .details .likeBox { font-size: 0; }
  .event .details .likeBox input { background-color: #4AA394; color: #FFF; border-radius: 5px 0 0 5px; display: inline-block; width: 75px; }
  .event .details .likeBox input:hover, .event .details .likeBox input:focus { background-color: #FF5707; cursor: pointer; }
  .event .details .likeBox .likeBarContainer 
  { 
    width: calc(100% - 75px); 
    border: 1px solid #CCC; 
    border-radius: 0 5px 5px 0; 
    height: 23px; 
    display: inline-block; 
    vertical-align: bottom; 
    position: relative; 
    font-size: 0; 
    padding: 1px;
  }
  .event .details .likeBox .likeBar, .event .details .likeBox .dislikeBar { width: 50%; height: 19px; display: inline-block; transition: width 1s; -webkit-transition: width 1s; }
  .event .details .likeBox .likeBar { background-color: #4AA394; }
  .event .details .likeBox .dislikeBar { background-color: #B03D25; }
  .event .details .likeBox .dislikeBar, .event .details .likeBox .likeBar.full { border-radius: 0 5px 5px 0; }
  .event .details .likeBox .likeBarText, .event .details .likeBox .dislikeBarText { position: absolute; top: 0; height: 19px; line-height: 19px; color: #FFF; }
  .event .details .likeBox .likeBarText { background: url('/images/icons/icon_likes_white.png') no-repeat left center; padding-left: 30px; left: 0; }
  .event .details .likeBox .dislikeBarText { background: url('/images/icons/icon_dislikes_white.png') no-repeat right center; padding-right: 30px; right: 0; }

  p.replacementButton { width: 75px; display: inline-block; text-align: center; color: #555; font-style: italic; height: 23px; line-height: 23px; margin: 0; }
  .locationType { font-style: italic; text-transform: capitalize; color: #555; }

    .event .basics a.hideSection { display: none; }


  /* Responsive */
  /*@media screen and (min-width: 980px) {
    #content { padding-left: 50px; }
  }*/
  @media screen and (max-width: 900px) {
    .event .basics { width: 25%; }
    .event .details { width: 75%; }
    .event .details .thumb { width: 23%; }
    .event .details .shareLike { padding-left: 24%; }
    .event .locationThumbnail img { width: 50px; }
    .event .details .shareLike { padding-left: 5px; clear: both; }
    .event .details .share, .event .details .likeBox { width: 100%; display: block; margin: 2px 0; }
    .event .details .likeBox { padding-right: 5px; }
    .event .details .thumb { max-height: calc(100% - 50px); }
  }
  @media screen and (max-width: 700px) {
    #topBox #filtering a { border-radius: 5px; margin: 2px 0; }
    #topBox #filtering a:first-child { border-radius: 5px; }
    #topBox #filtering a:last-child { border-radius: 5px; }
    .event .basics img { display: none; }
    .event .basics h2, .event .basics a, .event basics p { display: inline; }
    .event .locationThumbnail { width: auto; }
    .event .basics h2 { width: auto; }
    .event .basics p { width: auto; display: inline-block; padding-left: 0; }
    .event[data-type=resource] .basics, 
    .event[data-type=collection] .basics,
    .event[data-type=comment] .basics, 
    .event[data-type=resource] .details, 
    .event[data-type=collection] .details, 
    .event[data-type=comment] .details,
    .event[data-type=resource] .basics a div, 
    .event[data-type=collection] .basics a div, 
    .event[data-type=comment] .basics a div 
    { display: block; width: 100%; }
    .wayToContribute { width: 49%; }
    
  }
  @media screen and (max-width: 500px) {
    .event .basics, .event .basics p, .event .details, .event[data-type=comment] .details { display: block; width: 100%; }
  }
  @media screen and (max-width: 400px){
    
  }
</style>
<div id="content">
  <h1 class="isleH1"><asp:literal ID="pageHeading" runat="server">IOER Timeline</asp:literal></h1>

  <div id="contributeTabs" class="contributeTabs" runat="server">
    <a href="#" onclick="showTab('message', this); return false;">Post a Message</a>
    <a href="#" onclick="showTab('contribute', this); return false;">Contribute a Resource</a>
  </div>
  <div id="topBox">
    <div id="contribute" runat="server" class="contribute">
      <div class="grayBox contributeTab" data-tabID="message">
          <asp:DropDownList ID="communitiesList" CssClass="communitiesList" runat="server" ></asp:DropDownList>
        <textarea id="txtComment"></textarea>
        <p id="commentSubmitResult"></p>
        <input id="btnSubmit" type="button" onclick="postMessage(this); return false;" value="Post" />
      </div>
      <div class="grayBox contributeTab" data-tabID="contribute">
        <div class="wayToContribute" data-id="quickTag">
          <img alt="" src="/images/icons/icon_swirl_bg.png">
          <h4>Quick Tag</h4>
          <p>Submit a webpage or a file that is already hosted online, tag it with basic information, and enhance your tags later.</p>
          <a href="/tagger?theme=quick&mode=tag">Tag Now &rarr;</a>
        </div>
        <div class="wayToContribute" data-id="quickUpload">
          <img alt="" src="/images/icons/icon_upload_bg.png">
          <h4>Quick Upload</h4>
          <p>Upload a file, tag it with basic information, and enhance your tags later.</p>
          <a href="/tagger?theme=quick&mode=file">Upload Now &rarr;</a>
        </div>
        <div class="wayToContribute" data-id="author">
          <img alt="" src="/images/icons/icon_create_bg.png">
          <h4>Create a New Resource</h4>
          <p>Easily create a simple webpage and attach multiple files to it with this tool.</p>
          <a href="/My/Author.aspx">Go to Authoring Tool &rarr;</a>
        </div>
        <div class="wayToContribute" data-id="publisher">
          <img alt="" src="/images/icons/icon_tag_bg.png">
          <h4>Tag an Online Resource</h4>
          <p>Want to thoroughly tag a website or a file that's already hosted online? Start here.</p>
          <a href="/tagger?theme=ioer&mode=tag">Go to Tagging Tool &rarr;</a>
        </div>
      </div>
    </div>

    <div id="filtering">
      <a href="#" onclick="show('all'); return false;">Show All Activity</a>
      <!--<a href="#" onclick="show('news'); return false;">IOER News Updates</a>-->
      <a href="#" onclick="show('comment'); return false;">Recent Comments</a>
        <a href="#" onclick="show('communityposting'); return false;">Recent Community Posts</a>
      <a href="#" onclick="show('resource'); return false;">Resource Activity</a>
      <a href="#" onclick="show('collection'); return false;">Library Activity</a>
      <a href="#" onclick="show('like'); return false;">Likes</a>
    </div>
  </div>
    <div runat="server" id="ioerTimelineMessage" class="sectionBox" visible="false">
        <h2>Recent activity for the IOER site</h2>
    </div>
    <div runat="server" id="myTimelineMessage" class="sectionBox" visible="false">
        <h3>Recent activity for the people and libraries that you are following</h3>
    </div>
    <div runat="server" id="orgTimelineMessage" class="sectionBox" visible="false">
        <h3>Recent Activity for this Organization</h3>
    </div>

  <div id="timeline"></div>
  <div id="nothing">No recent activity.</div>

  <div id="templates" style="display: none;">
    <div id="template_separator">
      <div class="separator" data-date="{date}">
        Events on {date}:
      </div>
    </div>
    <div id="template_small">
      <div class="event" data-type="{type}">
        <img alt="" src="{icon}" />
        <div class="narrowDetails">
          <h2>{actor}</h2>
          <p>{action} a {item} </p>
          <a href="{link}" target="_self" class="locationThumbnail">
            <img alt="" src="{locationImage}" />
            <div>{location}</div>
          </a>
        </div>
      </div>
    </div>
    <div id="template_event">
      <div class="event" data-type="{type}">
        <img alt="" src="{icon}" class="icon" />
        <div class="basics">
          <h2><a href="/Profile/{userId}/{actor}" target="_self" ><img alt=""class="actorAvatar" src="{actorAvatar}" /> {actor}</a></h2>
          <p>{item}</p>
          <a href="{link}" target="_self" class="locationThumbnail {extraClass}">
            <img alt="" src="{locationImage}" />
            <div><span class="locationType">{locationType}</span><br />{location}</div>
          </a>
          <div class="date">{date}</div>
        </div>
        <div class="details">{details}</div>
      </div>
    </div>
    <div id="template_news">
      <h2><a href="{link}" target="_self">{title}</a></h2>
      <p>{description}</p>
    </div>
    <div id="template_comment">
      <p>{description}</p>
    </div>
    <div id="template_communityposting">
      <p>{description}</p>
    </div>
    <div id="template_resource">
      <h2><a href="{link}" target="_self">{title}</a></h2>
      <div class="boxWithThumb">
        <a href="{link}" target="_self" class="thumb" title="Resource Image"><img alt="" src="{thumb}" /></a>
        <p>{description}</p>
        <div class="shareLike">
          <div class="share">Share: <input title="share {title}" type="text" onclick="this.select()" readonly="readonly" value="http://ioer.ilsharedlearning.org{link}" /></div>
          <div class="likeBox" data-id="{id}">
            <input type="button" onclick="like(this, 'resource', {id}); return false;" value="+ Like" />
            <div class="likeBarContainer">
              <div class="likeBar"></div>
              <div class="dislikeBar"></div>
              <div class="likeBarText">{likes}</div>
              <div class="dislikeBarText">{dislikes}</div>
            </div>
          </div>
        </div>
      </div>
    </div>
      <div id="template_evaluation">
          <h2><a href="{link}" target="_self">{title}</a></h2>
          <div class="boxWithThumb">
            <a href="{link}" target="_self" class="thumb" title="Resource Image"><img alt="" src="{thumb}" /></a>
            <p>{description}</p>
            <div class="shareLike">
              <div class="share">Share: <input title="share {title}" type="text" onclick="this.select()" readonly="readonly" value="http://ioer.ilsharedlearning.org{link}" /></div>
              <div class="likeBox" data-id="{id}">
                <input type="button" onclick="like(this, 'resource', {id}); return false;" value="+ Like" />
                <div class="likeBarContainer">
                  <div class="likeBar"></div>
                  <div class="dislikeBar"></div>
                  <div class="likeBarText">{likes}</div>
                  <div class="dislikeBarText">{dislikes}</div>
                </div>
              </div>
            </div>
          </div>
    </div>
    <div id="template_collection">
      <h2><a href="{link}" target="_self">{title}</a></h2>
      <div class="boxWithThumb">
        <a href="{link}" target="_self" class="thumb" title="Resource Image"><img alt="" src="{thumb}" /></a>
        <p>{description}</p>
        <div class="shareLike">
          <p class="share"><label>Share: </label><input title="share {title}" type="text" onclick="this.select()" readonly="readonly" value="http://ioer.ilsharedlearning.org{link}" /></p>
          <div class="likeBox">
            <input type="button" onclick="like(this, 'collection', {id}); return false;" value="+ Like" />
            <div class="likeBarContainer">
              <div class="likeBar"></div>
              <div class="dislikeBar"></div>
              <div class="likeBarText">{likes}</div>
              <div class="dislikeBarText">{dislikes}</div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
    
    <asp:literal ID="myIoerHeader" Visible="false" runat="server">My IOER Timeline</asp:literal>
    <asp:literal ID="orgIoerHeader" Visible="false" runat="server">{0} IOER Timeline</asp:literal>
</div>