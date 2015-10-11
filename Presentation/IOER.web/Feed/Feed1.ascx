<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Feed1.ascx.cs" Inherits="IOER.Feed.Feed1" %>

<script type="text/javascript">
  <%=ranges %>
</script>
<script type="text/javascript">
  var icons = {
    news: "/images/icons/icon_swirl_med.png",
    comment: "/images/icons/icon_comments_med.png",
    resource: "/images/icons/icon_resources_med.png",
    collection: "/images/icons/icon_library_med.png"
  };

  $(document).ready(function () {
    renderRanges(false);
  });

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
    }

    return range;
  }

  function renderEvent(data, range) {
    return $("#template_event").html()
      .replace(/{icon}/g, icons[data.item.type])
      .replace(/{type}/g, data.item.type)
      .replace(/{item}/g, data.title)
      .replace(/{action}/g, data.action)
      .replace(/{actor}/g, data.actor.title)
      .replace(/{location}/g, data.location.title)
      .replace(/{date}/g, data.date)
      .replace(/{details}/g, getDetails(data))
      .replace(/{locationImage}/g, data.location.thumbnail)
      .replace(/{link}/g, data.location.link);
    }

  function getDetails(data) {
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
    else if (data.item.type == "resource") {
      //Do resource posting
      return $("#template_resource").html()
        .replace(/{title}/g, data.item.title)
        .replace(/{description}/g, data.item.description)
        .replace(/{thumb}/g, data.item.thumbnail)
        .replace(/{link}/g, data.item.link);
    }
    else if (data.item.type == "collection") {
      //Do collection posting
      return $("#template_collection").html()
        .replace(/{title}/g, data.item.title)
        .replace(/{description}/g, data.item.description)
        .replace(/{thumb}/g, data.item.thumbnail)
        .replace(/{link}/g, data.item.link);
    }
  }


</script>

<style type="text/css">
  /* Big Stuff */
  * { box-sizing: border-box; }
  #content { transition: padding 1s; -webkit-transition: padding 1s; min-width: 300px; }

  /* Top Box */
  #topBox { background-color: #EEE; padding: 5px; text-align: center; margin: 5px 5px 20px 5px; border-radius: 5px; margin: 0 auto; max-width: 1000px; margin-bottom: 25px; }
  #topBox #filtering a { display: inline-block; vertical-align: top; background-color: #3572B8; color: #FFF; text-align: center; padding: 5px; }
  #topBox #filtering a:first-child { border-radius: 5px 0 0 5px; }
  #topBox #filtering a:last-child { border-radius: 0 5px 5px 0; }
  #topBox #filtering a:hover, #topBox #filtering a:focus { background-color: #FF5707; }
  /* Contribute */
  #topBox #contribute { text-align: center; width: 80%; margin: 5px auto; }
  #topBox #contribute .contributeLabel { display: inline-block; width: 32%; padding: 5px; color: #FFF; }
  #topBox #contribute #txtComment { width: 100%; resize: none; height: 4em; border: 1px solid #4AA394; }
  #topBox #contribute span.contributeLabel { background-color: #4AA394; border-top-left-radius: 5px; }
  #topBox #contribute a.contributeLabel { background-color: #3572B8; }
  #topBox #contribute a.rounded { border-top-right-radius: 5px; }
  #topBox #contribute a.contributeLabel:hover, #topBox #contribute a.contributeLabel:focus, #topBox #contribute #btnSubmit:hover, #topBox #contribute #btnSubmit:focus { background-color: #FF5707; cursor: pointer; }
  #topBox #contribute #commentSubmitResult { display: inline-block; width: 78%; vertical-align: top; min-height: 1.2em; text-align: right; }
  #topBox #contribute #btnSubmit { width: 20%; background-color: #4AA394; color: #FFF; display: inline-block; vertical-align: top; border-radius: 5px; }


  /* Ranges */
  .range { margin-bottom: 30px; max-width: 1200px; margin: 0 auto 50px auto; }
  .range .separator, #nothing { text-align: center; margin: 5px 10% 15px 10%; padding: 5px; background-color: #EEE; border-radius: 5px; font-weight: bold; font-size: 20px; }

  /* Events */
  .event, .separator, .range, #nothing { transition: height 1s, opacity 1s, padding 1s, margin 1s; -webkit-transition: height 1s, opacity 1s, padding 1s, margin 1s; height: auto; opacity: 1; }
  .event[data-showing=false], .separator[data-showing=false], .range[data-showing=false], #nothing[data-showing=false] { height: 0; opacity: 0; margin-top: 0; margin-bottom: 0; padding-top: 0; padding-bottom: 0;  }
  .event { background-color: #EEE; border-radius: 5px; position: relative; margin-left: 25px; padding-left: 25px; font-size: 0; margin-bottom: 10px; height: auto; opacity: 1; }
  .event .icon { width: 50px; position: absolute; top: 0; left: -25px; background-color: #EEE; border-radius: 50%; }
  .event .basics, .event .details { display: inline-block; vertical-align: top; font-size: 16px; }
  .event .basics { width: 200px; padding: 5px; }
  .event .basics h2 { font-size: 20px; }
  .event .locationThumbnail { width: 100%; display: block; }
  .event .locationThumbnail img { width: 75px; }
  .event .locationThumbnail div { width: calc(100% - 80px); padding-left: 5px; overflow: hidden; text-overflow: ellipsis; }
  .event .locationThumbnail img, .event .locationThumbnail div { display: inline-block; vertical-align: middle; }
  .event .date { position: absolute; top: 5px; right: 5px; }
  .event .details { width: calc(100% - 200px); padding: 5px; }
  .event .details .thumb { display: inline-block; vertical-align: top; width: 125px; max-height: 125px; overflow: hidden; float: left; }
  .event .details .thumb img { width: 100%; }
  .event .details h2 { font-size: 20px; }
  .event .details h2, .event[data-type=comment] .details { padding-right: 75px; }
  .event .details .shareLike { padding-left: 130px; }
  .event .details .share, .event .details .likeBox { width: 48%; display: inline-block; }
  .event .details .share label { display: inline-block; width: 40px; }
  .event .details .share input { display: inline-block; width: calc(100% - 45px); }
  .event .details .likeBox { font-size: 0; }
  .event .details .likeBox input { background-color: #4AA394; color: #FFF; border-radius: 5px 0 0 5px; display: inline-block; width: 40px; }
  .event .details .likeBox .likeBarContainer { width: calc(100% - 40px); border: 1px solid #CCC; border-radius: 0 5px 5px 0; height: 21px; display: inline-block; vertical-align: bottom; }
  .event a { font-size: inherit; }

  /* Responsive */
  @media screen and (min-width: 980px) {
    #content { padding-left: 50px; }
  }
  @media screen and (max-width: 900px) {
    .event .basics { width: 25%; }
    .event .details { width: 75%; }
    .event .details .thumb { width: 23%; }
    .event .details .shareLike { padding-left: 24%; }
    .event .locationThumbnail img { width: 50px; }
    .event .locationThumbnail div { width: calc(100% - 55px); }
  }
  @media screen and (max-width: 600px) {
    #topBox #filtering a { border-radius: 5px; margin: 2px 0; }
    #topBox #filtering a:first-child { border-radius: 5px; }
    #topBox #filtering a:last-child { border-radius: 5px; }
    .event .details .shareLike { padding-left: 5px; clear: both; }
    .event .details .share, .event .details .likeBox { width: 100%; display: block; margin: 2px 0; }
    .event .details .likeBox { padding-right: 5px; }
    .event .locationThumbnail div { width: 100%; }
    .event .locationThumbnail img { display: none; }
  }
  @media screen and (max-width: 500px) {
    .event .basics { display: block; width: 100%; }
    .event .basics p { display: inline-block; }
    .event .details { display: block; width: 100%; }
  }
  @media screen and (max-width: 400px){
    
  }
</style>
<div id="content">
  <h1 class="isleH1">ISLE OER Activity Feed</h1>
  <div id="topBox">
    <div id="contribute">
      <span class="contributeLabel">Post a Message</span>
      <a class="contributeLabel" href="/Contribute/?mode=upload">Contribute a File</a>
      <a class="contributeLabel rounded" href="/Contribute/?mode=tag">Contribute a Link</a>
      <textarea id="txtComment"></textarea>
      <p id="commentSubmitResult"></p>
      <input id="btnSubmit" type="button" onclick="return false;" value="Post" />
    </div>
    <div id="filtering">
      <a href="#" onclick="show('all'); return false;">Show All Activity</a>
      <a href="#" onclick="show('news'); return false;">IOER News Updates</a>
      <a href="#" onclick="show('comment'); return false;">Recent Comments</a>
      <a href="#" onclick="show('resource'); return false;">Resource Activity</a>
      <a href="#" onclick="show('collection'); return false;">Library Activity</a>
    </div>
  </div>
  <div id="timeline"></div>
  <div id="nothing">No recent activity.</div>

  <div id="templates" style="display: none;">
    <div id="template_separator">
      <div class="separator" data-date="{date}">
        Events on {date}:
      </div>
    </div>
    <div id="template_event">
      <div class="event" data-type="{type}">
        <img src="{icon}" class="icon" />
        <div class="basics">
          <h2>{actor}</h2>
          <p>{action} a {item} to</p>
          <a href="{link}" target="_blank" class="locationThumbnail">
            <img src="{locationImage}" />
            <div>{location}</div>
          </a>
          <div class="date">{date}</div>
        </div>
        <div class="details">{details}</div>
      </div>
    </div>
    <div id="template_news">
      <h2><a href="{link}" target="_blank">{title}</a></h2>
      <p>{description}</p>
    </div>
    <div id="template_comment">
      <p>{description}</p>
    </div>
    <div id="template_resource">
      <h2><a href="{link}" target="_blank">{title}</a></h2>
      <div class="boxWithThumb">
        <a href="{link}" target="_blank" class="thumb" title="Resource Image"><img src="{thumb}" /></a>
        <p>{description}</p>
        <div class="shareLike">
          <div class="share">Share: <input title="share {title}" type="text" onclick="this.select()" readonly="readonly" value="{link}" /></div>
          <div class="likeBox">
            <input type="button" onclick="like('resource', {id}); return false;" value="+ Like" />
            <div class="likeBarContainer">
              <div class="likeBar"></div>
              <div class="dislikeBar"></div>
              <div class="likeBarText"></div>
              <div class="dislikeBarText"></div>
            </div>
          </div>
        </div>
      </div>
    </div>
    <div id="template_collection">
      <h2><a href="{link}" target="_blank">{title}</a></h2>
      <div class="boxWithThumb">
        <a href="{link}" target="_blank" class="thumb" title="Resource Image"><img src="{thumb}" /></a>
        <p>{description}</p>
        <div class="shareLike">
          <p class="share"><label>Share: </label><input title="share {title}" type="text" onclick="this.select()" readonly="readonly" value="{link}" /></p>
          <div class="likeBox">
            <input type="button" onclick="like('collection', {id}); return false;" value="+ Like" />
            <div class="likeBarContainer">
              <div class="likeBar"></div>
              <div class="dislikeBar"></div>
              <div class="likeBarText"></div>
              <div class="dislikeBarText"></div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</div>