<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SearchV6.ascx.cs" Inherits="ILPathways.Controls.SearchV6.SearchV6" %>
<%@ Register TagPrefix="uc1" TagName="Standards" Src="/Controls/StandardsBrowser7.ascx" %>

<link rel="stylesheet" type="text/css" href="/styles/common2.css" />
<div id="themesBox" runat="server" class="themesBox"></div>

<script type="text/javascript">
  /* Server Data */
  var ajaxURL = "<%=AjaxSearchUrl %>";
  var libraryIDs = <%=JSON_LibraryIds %>;
  var collectionIDs = <%=JSON_CollectionIds %>;
  var usingStandards = <%=(UseStandards ? "true" : "false") %>;
  var usingResourceUrl = <%=(UseResourceUrl ? "true" : "false") %>;
</script>
<script type="text/javascript">
  /* Page variables */
  var SB7mode = "search";
  var countdown = {};
  var sortMode = { field: "ResourceId", order: "desc" };
  var preselectedSort = "<%=SortOrder %>";
  var pageSize = 20;
  var viewMode = "<%=ViewMode %>";
  var currentStartPage = 1;
  var currentResults = {};
  var selectedTags = [];
  var currentQuery = {};
  var requests = [];
  var selectedStandardsIDs_search = [];
  var autoTextFilters = [
    "delete",
    "freesound",
    "bookshare",
    "smarter balanced"
  ];
  var previousText = "";


  /* Initialization */
  $(document).ready(function () {
    //Event-driven Pipeline engine
    initPipeline();
    //Init Search Box
    setupSearchBox();
    //Setup Filter interactivity
    setupFilterClicking();
    //Setup Standards Browser interactivity
    setupStandardsInteraction();
    //Setup drop-down lists
    setupDDLs();
    //Setup window resize actions
    setupResizing();    
    //Listen for messages indicating we should auto-select some tags
    setupAutoSelectTags();
    //Log resource views if in widget mode
    setupViewLogging();
    //Do automatic search
    pipeline("updateFiltering");
    pipeline("resetCountdown");
    $("form").attr("onsubmit", "return false;");
    setupPreselectedSortOrder();
  });

  //Setup window resize events
  function setupResizing() {
    $(window).on("resize", function () {
      calculateExposedDescriptions();
    });
    triggerResize(0);
  }

  //Setup preselected sort order
  function setupPreselectedSortOrder() {
    $("#ddlSortOrder option").each(function() {
      if($(this).attr("value").toLowerCase() == preselectedSort.toLowerCase()){
        console.log("true");
        $(this).prop("selected", true);
      }
    });
    setTimeout(function() {
      $("#ddlSortOrder").trigger("change");
    }, 805);
  }

  //Setup drop-down lists
  function setupDDLs() {
    //Sort Order
    $("#ddlSortOrder").on("change", function () {
      var sortModeParts = $(this).find("option:selected").attr("value").split("|");
      sortMode = { field: sortModeParts[0], order: sortModeParts[1] };
      //currentStartPage = 1;
      pipeline("doSearch");
    });
    //Number of items to show
    $("#ddlPageSize").on("change", function () {
      pageSize = parseInt($(this).find("option:selected").attr("value"));
      //currentStartPage = 1;
      pipeline("doSearch");
    });
    //View mode
    $("#ddlViewMode").on("change", function () {
      viewMode = $(this).find("option:selected").attr("value");
      $("#searchResults").attr("data-mode", viewMode);
      pipeline("loadResults");
    });
  }

  //Setup Search box interactivity
  function setupSearchBox() {
    $("#txtSearch").on("keyup change", function (e) {
      var value = $("#txtSearch").val();
      if(previousText != value || e.which == 13 || e.keyCode == 13){
        pipeline("resetCountdown");
        previousText = value;
      }
    });
  }

  //Setup Filter interactivity
  function setupFilterClicking() {
    $("html").not("#btnToggleFilters, #filters, #tags, #standardsBrowserFloat").on("click", function () {
      $("#btnToggleFilters, #filters, #tags, #standardsBrowserFloat").removeClass("expanded").addClass("collapsed");
      $(".btnCategory, .tagList").removeClass("selected");
    });
    $("#btnToggleFilters, #filters, #tags, #standardsBrowserFloat").on("click", function (e) {
      e.stopPropagation();
    });
    $("#btnToggleFilters, #filters, #tags").on("click", function() {
      $("#standardsBrowserFloat").removeClass("expanded").addClass("collapsed");
    });
    $("#tags input").on("change", function () {
      pipeline("updateFiltering");
      pipeline("resetCountdown");
    });
  }

  //Setup standards browser interactivity
  function setupStandardsInteraction() {
    $(window).on("SB7search", function(event,selected,all){
      pipeline("doSearch");
    });
    $(window).on("SB7showResults", function() {
      $("#btnToggleFilters, #filters").removeClass("expanded").addClass("collapsed");
      $("#standardsBrowserFloat").removeClass("expanded").addClass("collapsed");
    });
    $(window).on("standard_added standard_removed", function() {
      pipeline("updateFiltering");
    });
  }

  //Pipeline Engine
  function initPipeline() {
    $(window).on("pipeline", function (event, data) {
      switch (data.from) {
        case "updateFiltering":
          //Update Tags
          updateTags();
          //Render selected Tags and Standards
          renderSelectedTags();
          break;
        case "resetCountdown":
          //Reset Countdown
          resetCountdown();
          break;
        case "doSearch":
          //Reset search page if needed
          if(data.data != "isPagedSearch"){
            currentStartPage = 1;
          }
          //Pack Query
          packQuery();
          //Do search
          doSearch();
          break;
        case "loadResults":
          //Render Search Results
          renderResults();
          //Render Paginators
          renderPaginators();
          //Do post-search actions
          calculateExposedDescriptions();
          showStandardHits();
          break;
        default:
          break;
      }
      triggerResize(250);
    });
  }

  //Trigger pipeline
  function pipeline(target, data) {
    $(window).trigger("pipeline", { from: target, data: data });
  }

  //Reset countdown
  function resetCountdown() {
    clearTimeout(countdown);
    countdown = setTimeout(function () { pipeline("doSearch"); }, 800);
  }

  //Pack Query
  function packQuery(isPagedSearch) {
    selectedStandardsIDs_search = [];
    if(usingStandards){
      for(i in allSelectedStandards){
        selectedStandardsIDs_search.push(allSelectedStandards[i].id);
      }
    }
    var queryText = getQueryText();
    currentQuery = {
      text: queryText,
      fields: getSelectedTags(),
      allStandardIDs: selectedStandardsIDs_search,
      libraryIDs: libraryIDs,
      collectionIDs: collectionIDs,
      sort: sortMode,
      size: pageSize,
      start: (currentStartPage - 1) * pageSize,
      not: getActiveAutoTextFilters(queryText)
    };
  }

  //Show results in standards browser
  function showStandardHits(){
    $(window).trigger("resultsLoaded", currentResults.hits.hits.length);
  }

  function showHideStandardsBrowser() {
    var box = $("#standardsBrowserFloat");
    if(box.hasClass("expanded")){
      box.removeClass("expanded").addClass("collapsed");
    }
    else {
      box.removeClass("collapsed").addClass("expanded");
    }
  }

  //Enable selecting tags via event
  function setupAutoSelectTags() {
    $(window).on("selectTags", function (msg, data) {
      for(i in data.tags){
        $(".tagList input[data-id=" + data.tags[i] + "]").prop("checked", true).trigger("change");
      }
    });
  }

  //Setup view logging if in widget mode
  function setupViewLogging() {
    if(!usingResourceUrl){
      return;
    }
    $(window).on("resultsRendered", function() {
      $("#searchResults .result").each(function() {
        var id = parseInt($(this).attr("data-resourceid"));
        $(this).find(".resourceLink").on("click", function() {
          logResourceView(id);
        });
      });
    });
  }
</script>
<script type="text/javascript">
  /* Page Functions */
  //Toggle Filters 
  function toggleFilters() {
    $("#btnToggleFilters, #filters").toggleClass("collapsed expanded");
    if($("#filters").hasClass("collapsed")){
      $(".tagList").removeClass("selected");
    }
    triggerResize(100);
  }
  //Toggle an individual filter 
  function toggleFilter(input) {
    var button = $(input);
    var filter = $(".tagList[data-filterID=" + button.attr("data-filterID") + "]");
    var keep = button.hasClass("selected");
    $(".btnCategory, .tagList").removeClass("selected");
    if (!keep) {
      button.addClass("selected");
      filter.addClass("selected");
    }
    triggerResize(100);
  }

  //Show/truncate descriptions based on length and page width
  function calculateExposedDescriptions() {
    $(".result.list").each(function() {
      var box = $(this);
      var descriptionBox = $(this).find(".descriptionBox");

      if(descriptionBox.find(".description").outerHeight() >= 100){ descriptionBox.removeClass("short"); }
      else { descriptionBox.addClass("short"); }
    });
  }

  //Toggle expand/collapse of description
  function toggleExpandCollapse(id, button){
    var jButton = $(button);
    var box = $(".result.list[data-resourceID=" + id + "]");
    var description = box.find(".description");

    description.attr("data-collapsed", description.attr("data-collapsed") == "collapsed" ? "expanded" : "collapsed");
    jButton.attr("value", description.attr("data-collapsed") == "collapsed" ? "More" : "Less");
    triggerResize(250);
  }

  //Search for arbitrary text
  function searchFor(text, wrapInQuotes) {
    if(wrapInQuotes){
      $("#txtSearch").val('"' + text + '"');
    }
    else {
      $("#txtSearch").val(text);
    }
    pipeline("doSearch");
  }

  //Jump to page
  function jumpToPage(page){
    currentStartPage = page;
    pipeline("doSearch", "isPagedSearch");
  }

  //Update Tags and Standards
  function updateTags(){
    selectedTags = [];
    $("#tags .tagList:not(.standards) input:checked").each(function() {
      selectedTags.push({ id: $(this).attr("data-id"), text: $(this).parent().text().trim(), isStandard: false });
    });
    if(usingStandards){
      for(i in selectedStandards){
        selectedTags.push({ 
          id: selectedStandards[i].id, 
          text: (selectedStandards[i].code == "" || selectedStandards[i].code == null) ? selectedStandards[i].description : selectedStandards[i].code, 
          isStandard: true 
        });
      }
    }
  }

  //Remove selected tag
  function removeTag(id, isStandard){
    if(isStandard){
      $(window).trigger("SB7removeStandard", id);
      //removeSelectedStandard(id);
    }
    else {
      $("#tags input[data-id=" + id + "]").prop("checked", false);
    }
    pipeline("updateFiltering");
    pipeline("resetCountdown");
  }

  //Trigger page resize
  function triggerResize(delay) {
    if(delay){
      setTimeout(function() { triggerResize(0); }, delay); 
    }
    $(window).trigger("resize");
  }

  //Record a resource view
  function logResourceView(id){
    var title = "";
    for(var i in currentResults.hits.hits){
      if(currentResults.hits.hits[i]._source.ResourceId == id){
        title = currentResults.hits.hits[i]._source.Title;
        break;
      }
    }

    $.ajax({
      url: "/services/activityservice.asmx/Resource_ResourceView",
      async: true,
      headers: { "Accept": "application/json", "Content-type": "application/json; charset=utf-8" },
      dataType: "json",
      type: "POST",
      data: JSON.stringify({ resourceID: id, title: title }),
      success: function (msg) {
        var message = msg.d ? msg.d : msg;
        console.log(message);
      }
    });
  }
  
</script>
<script type="text/javascript">
  /* AJAX */
  //Do a search
  function doSearch() {
    //Cancel any pending requests
    for (i in requests) {
      requests[i].abort();
    }
    requests = [];
    setStatus("Searching...", "searching");
    //Do the search
    var request = $.ajax({
      url: ajaxURL,
      async: true,
      headers: { "Accept": "application/json", "Content-type": "application/json; charset=utf-8" },
      dataType: "json",
      type: "POST",
      data: JSON.stringify({ input: currentQuery }), //Write this
      success: function (msg) {
        var message = msg.d ? msg.d : msg;
        currentResults = $.parseJSON(message.data);
        currentResults.extra = message.extra;
        pipeline("loadResults"); 
      }
    });
    requests.push(request);
  }

</script>
<script type="text/javascript">
  /* Rendering */
  //Render search results
  function renderResults() {
    //Get structures
    var template = $("#template_result_" + viewMode).html();
    var thumbnailTemplate = $("#template_thumbnail").html();
    var box = $("#searchResults");
    var results = currentResults.hits.hits;
    //Clear result list
    box.html("");
    //Load results
    if (results.length == 0) {
      setStatus("Sorry, no results were found. Please try a different combination of search terms and filters.", "error");
      return;
    }
    else {
      setStatus("Found " + currentResults.hits.total + " Resources");
      for (i in results) {
        var current = results[i]._source;
        var link = usingResourceUrl ? current.Url : "/Resource/" + current.ResourceId + "/" + ( current.UrlTitle == null ? "" : current.UrlTitle).replace(/&/g, "" );
        //Get thumbnail
        var thumbnailText = thumbnailTemplate.replace(/{resultURL}/g, link).replace(/{imageURL}/g, "//ioer.ilsharedlearning.org/OERThumbs/large/" + current.ResourceId + "-large.png")
        //Get keywords
        var keywords = getKeywords(current);
        //Get paradata
        var paradata = getParadata(current);
        //Get standards
        var standards = getStandards(current);
        //Fill template
        box.append(
          template.replace(/{resourceID}/g, current.ResourceId)
            .replace(/{versionID}/g, current.VersionId)
            .replace(/{resultURL}/g, link)
            .replace(/{title}/g, current.Title)
            .replace(/{description}/g, current.Description)
            .replace(/{keywords}/g, keywords)
            .replace(/{thumbnail}/g, thumbnailText)
            .replace(/{paradata}/g, paradata)
            .replace(/{created}/g, current.ResourceCreated)
            .replace(/{creator}/g, getAuthor(current) )
            .replace(/{standards}/g, standards)
        );

      }
      //Trigger any extra theme-based result injections
      $(window).trigger("resultsRendered");
      box.hide().fadeIn();
    }
  }
  //Get a list of keyword-style metadata links
  function getKeywords(current) {
    var list = [];
    var result = "";
    for (i in current.Fields) {
      var item = current.Fields[i];
      if (keywordSchemas.indexOf(item.Schema) > -1) {
        list = list.concat(item.Tags);
      }
    }
    for (var i = 0; i < (list.length < 10 ? list.length : 10); i++) {
      if(list[i].toLowerCase() == "other"){ continue; }
      result += "<a href=\"#\" onclick=\"searchFor('" + list[i] + "'); return false;\" title=\"" + list[i] + "\">" + list[i] + "</a>";
    }
    return result;
  }
  //Get a list of paradata icons
  function getParadata(current) {
    var result = "";
    var data = current.Paradata;
    var paradataFormat = [
      { img: "icon_comments", data: data.Comments, tip: "Comments", valid: data.Comments > 0 },
      { img: "icon_likes", data: data.Likes, tip: "Likes", valid: data.Likes > 0 },
      { img: "icon_dislikes", data: data.Dislikes, tip: "Dislikes", valid: data.Dislikes > 0 },
      { img: "icon_ratings", data:  ((data.EvaluationsScore / 3) * 100).toString().split(".")[0] + "%", tip: "Rating", valid: data.Evaluations > 0 },
      { img: "icon_library", data: current.LibraryIds.length, tip: "Favorites", valid: data.Favorites > 0 },
      { img: "icon_click-throughs", data: data.ResourceViews, tip: "Number of users who visited this Resource",  valid: data.ResourceViews > 0 }
    ];
    for (i in paradataFormat) {
      var item = paradataFormat[i];
      if (item.valid) {
        result += "<div style=\"background-image:url('/images/icons/" + item.img + ".png');\" title=\"" + item.tip + "\">" + item.data + "</div>";
      }
    }
    return result;
  }
  //Get a list of Standard links
  function getStandards(current) {
    var result = "";
    if (current.StandardNotations.length == 0) { return ""; }
    for (i in current.StandardNotations) {
      result += "<a href=\"#\" onclick=\"searchFor('" + current.StandardNotations[i] + "'); return false;\" title=\"" + current.StandardNotations[i] + "\">" + current.StandardNotations[i] + "</a>";
    }
    return result;
  }
  //Try to get a name to display
  function getAuthor(current){
    var authors = ["Publisher", "Creator"];
    for(i in authors){
      if(current[authors[i]] != null && current[authors[i]] != ""){ return current[authors[i]]; }
    }
    var link = document.createElement("a");
    link.href = current.Url;
    return link.hostname;
  }

  //Render Paginators
  function renderPaginators(){
    var box = $("#paginatorTop");
    var totalResults = currentResults.hits.total;
    if(totalResults == 0){
      box.html("");
      $("#paginatorBottom").html("");
      return;
    }

    box.html("Page: ");
    
    var totalPages = Math.ceil(totalResults / pageSize);
    var skips = [1, 5, 10, 25, 50, 500, 1000, 2500, 5000, 10000, totalPages];
    for(var i = 1; i <= totalPages; i++){
      var page = i;
      if(page >= (currentStartPage - 2) && page <= (currentStartPage + 2) || skips.indexOf(i) > -1 || i == totalPages){
        box.append("<input type=\"button\" value=\"" + page + "\" class=\"" + ((currentStartPage == page) ? "current" : "") + "\" onclick=\"jumpToPage(" + i + ");\" />");
      }
    }

    $("#paginatorBottom").html(box.html());
    triggerResize(100);
  }

  //Update the status message
  function setStatus(text, css){
    $("#status").attr("class", "").addClass(css).html(text);
    triggerResize(1000);
  }

  //Render selected tags
  function renderSelectedTags() {
    var box = $("#selectedTags");
    var template = $("#template_selectedTag").html();
    var temp = [];
    box.html("");
    for(i in selectedTags){
      box.append(template.replace(/{id}/g, selectedTags[i].id).replace(/{text}/g, selectedTags[i].text).replace(/{isStandard}/, selectedTags[i].isStandard ? "true" : "false"));
      temp.push(selectedTags[i].id);
    }
    triggerResize(100);
    console.log("Selected Tags:", temp.join(","));
  }

</script>
<script type="text/javascript">
  /* Helper Functions */
  //Get filtered query text
  function getQueryText() {
    var text = $("#txtSearch").val().trim();
    //Asterisk
    if (text == "") {
      text = "*";
    }

    return text;
  }
  //Get in-use auto text filters
  function getActiveAutoTextFilters(text){
    var result = "";
    text = text.toLowerCase();
    for(i in autoTextFilters){
      var current = autoTextFilters[i];
      if(text.indexOf(current) == -1){
        result += " '" + current + "' ";
      }
    }
    return result;
  }
  //Get selected fields/tags
  function getSelectedTags() {
    var result = [];
    $("#tags .tagList").each(function () {
      if($(this).hasClass("standards")){ return; }
      var filter = { Id: parseInt($(this).attr("data-filterID")), Ids: [] };
      $(this).find("input:checked").each(function () {
        filter.Ids.push(parseInt($(this).attr("data-id")));
      });
      if (filter.Ids.length > 0) {
        result.push(filter);
      }
    });
    return result;
  }
</script>
<style type="text/css">
  /* Big Items */
  body { min-height: 1000px; }
  .theme { padding: 5px 0; }
  input[type=button] { -webkit-appearance: none; }

  /* Search Header */
  #searchHeader { position: relative; }
  #queryBox { padding-left: 250px; }
  #btnToggleFilters, #txtSearch { height: 40px; display: inline-block; font-size: 30px; vertical-align: top; }
  #btnToggleFilters { width: 250px; border-radius: 5px 0 0 0; position: absolute; top: 0; left: 0; border: 0; transition: background-color 0.1s, color 0.1s; font-weight: bold; }
  #btnToggleFilters:hover, #btnToggleFilters:focus { background-color: #FF6A00; color: #FFF; }
  #txtSearch { width: 100%; border-radius: 0 5px 0 0; padding-left: 15px; background-image: linear-gradient(#EEE, #FFF 50%, #FFF 85%, #EFEFEF); }
  #ddls { white-space: nowrap; }
  #ddls select { width: 33.333%; font-size: 20px; height: 30px; cursor: pointer; }
  #ddls #ddlSortOrder { border-radius: 0 0 0 5px; }
  #ddls #ddlPageSize { border-radius: 0; }
  #ddls #ddlViewMode { border-radius: 0 0 5px 0; }
  .selectedTag { margin: 2px 1px; border-radius: 5px; background-color: #EEE; padding: 2px 35px 2px 5px; height: 25px; display: inline-block; vertical-align: top; position: relative; max-width: 275px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
  .selectedTag input { display: inline-block; height: 100%; width: 25px; border-radius: 0 5px 5px 0; position: absolute; top: 0; right: 0; }

  /* Filters */
  #filters { border-radius: 0 0 5px 5px; position: absolute; top: 40px; left: 0; width: 250px; margin-bottom: 50px; height: 0; transform-origin: top center; -webkit-transform-origin: top center; transition: transform 0.3s, -webkit-transform 0.3s, -ms-transform 0.3s; display: block; z-index: 100; }
  #filters.expanded { height: auto; transform: scaleY(1); -webkit-transform: scaleY(1); -ms-transform: scaleY(1); }
  #filters.collapsed { transform: scaleY(0); -webkit-transform: scaleY(0); -ms-transform: scaleY(0); }
  #filters #categories, #tags { display: inline-block; vertical-align: top; }
  #filters #categories { width: 100%; }
  #filters #categories input { width: 100%; border-radius: 0; padding: 5px; text-align: right; white-space: normal; font-weight: bold; border-width: 1px; transition: background-color 0.1s, color 0.1s; }
  #filters #categories input:last-child { border-radius: 0 0 0 5px; }
  #filters #categories input:hover, #filters #categories input:focus { background-color: #FF6A00; }
  #tags { position: absolute; top: 40px; left: 0; z-index: 150; width: 100%; }
  #tags .tagList { margin-left: 250px; }
  #tags .tagList h2 { padding: 6px 10px; border-radius: 0 5px 0 0; }
  #tags .tagList .tagCBXL { padding: 5px; }
  #tags .tagList .tagCBXL label { display: block; padding: 3px 5px; border-radius: 5px; }
  #tags .tagList .tagCBXL label:hover, #tags .tagList .tagCBXL label:focus { cursor: pointer; background-color: #FDFDFD; }
  #tags .tagList { min-width: 275px; display: block; background-color: rgba(240,240,240,0.95); border-radius: 0 5px 5px 0; position: absolute; top: 0; margin-bottom: 50px; transform-origin: left center; -webkit-transform-origin: left center; transition: transform 0.2s, -webkit-transform 0.2s, -ms-transform 0.2s; transform: scaleX(0); -webkit-transform: scaleX(0); -ms-transform: scaleX(0); }
  #tags .tagList.selected { transform: scaleX(1); -webkit-transform: scaleX(1); -ms-transform: scaleX(1); }

  #tags .tagList.standards { width: calc(100% - 250px); }
  .selectedStandard { background-color: #F5F5F5; }

  /* Paginators */
  .paginator { padding: 5px; text-align: center; }
  .paginator:empty { display: none; }
  .paginator input { min-width: 50px; padding: 2px 5px; border-radius: 0; margin: 1px; border: none; transition: background 0.2s; }
  .paginator input:first-child { border-radius: 5px 0 0 5px; }
  .paginator input:last-child { border-radius: 0 5px 5px 0; }
  .paginator input:only-child { border-radius: 5px; }
  .paginator input:hover, .paginator input:focus { background-color: #FF6A00; }

  /* Search Results */
  #searchResults[data-mode=list] { }
  #searchResults[data-mode=grid] { text-align: center; }
  #searchResults[data-mode=text] { }
  .result.list { padding: 5px; border-radius: 5px; background-image: linear-gradient(#F5F5F5, #FFF); }
  .result.list .resultcontent { white-space: nowrap; }
  .result.list .descriptionBox, .result.list .keywords, .result.list .images, .result.list .images .thumbnailBox, .result.list .paradata div { display: inline-block; vertical-align: top; }
  .result.list .descriptionBox { width: calc(100% - 400px); }
  .result.list .keywords { width: 200px; padding: 0 10px; }
  .result.list .images { width: 200px; }
  .result.list .thumbnailBox { width: 100%; }
  .result.list .keywords a { display: block; text-align: right; max-width: 100%; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; border-bottom: 1px solid rgba(0,0,0,0.1); }
  .result.list .keywords a:last-child { border-bottom: none; }
  .result.list .description { position: relative; max-height: 400px; overflow: hidden; transition: max-height 0.5s; padding-bottom: 15px; }
  .result.list .description[data-collapsed=collapsed] { max-height: 100px; }
  .result.list .descriptionBox .description::after { background-image: linear-gradient(rgba(255,255,255,0), #FFF); content: " "; height: 15px; display: block; position: absolute; bottom: 0; left: 0; right: 0; transition: height 0.5s; }
  .result.list .description[data-collapsed=collapsed]::after { height: 65px; }
  .result.list .descriptionBox.short .description::after, .result.list .descriptionBox.short .expandCollapseBox input { display: none; }
  .result.list .expandCollapseBox { height: 25px; }
  .result.list .expandCollapseBox input { float: right; width: 100px; }
  .result.list .links a { display: inline-block; vertical-align: top; overflow: hidden; text-overflow: ellipsis; width: 30%; margin: 2px 1%; text-align: left; white-space: nowrap; }
  .result.list .title { font-size: 20px; font-weight: bold; padding: 2px 5px 5px 2px; }

  .result.grid { width: 250px; display: inline-block; vertical-align: top; margin: 15px 1%; box-shadow: 0 0 15px -1px #CCC; padding: 5px; border-radius: 5px; transition: box-shadow 0.2s; }
  .result.grid .thumbnailBox { position: relative; margin: -5px -5px 5px -5px; }
  .result.grid .title { font-size: 18px; min-height: 4em; display: block; }
  .result.grid .title { padding: 2px 5px; }
  .result.grid .paradata { min-height: 32px; position: absolute; top: 0; right: 0; background-image: linear-gradient(90deg, rgba(255,255,255,0) 10%, rgba(0,0,0,0.5)); height: 100%; border-radius: 0 5px 5px 0; overflow: hidden; }
  .result.grid:hover, .result.grid:focus { box-shadow: 0 0 20px -1px #FF6A00; }
  .result.grid .thumbnail { border-radius: 5px 5px 0 0; }

  .result.text { margin-bottom: 10px; }
  .result.text a { font-size: 20px; font-weight: bold; display: block; padding: 5px; }
  .result.text .description { padding: 0 10px 10px 10px; }

  .result .thumbnail { background: #EEE url('') center center no-repeat; background-size: 100% 100%; display: block; border-radius: 5px; overflow: hidden; box-shadow: 0 0 5px #CCC; }
  .result .thumbnail img { display: block; width: 100%; }
  .result .description { padding: 5px; white-space: normal; }
  .result .created { text-align: right; opacity: 0.8; font-style: italic; font-size: 90%; padding: 2px; }
  .result .paradata { text-align: right; padding: 2px 0; }
  .result .paradata div { color: #000; background: #EEE url('') left center no-repeat; text-align: right; padding: 0 5px; height: 25px; line-height: 25px; width: 60px; border-radius: 5px; margin: 1px; border: 1px solid #999; }

  /* Miscellaneous */
  #status { padding: 2px; margin: 3px 0 0 0; text-align: center; transition: background 0.8s, padding 0.8s; border-radius: 5px; }
  #status.searching { background-color: rgba(0,255,0,0.6); }
  #status.error { padding: 50px; background-color: rgba(255,0,0,0.2); }
  #status.caution { padding: 15px; background-color: rgba(255,204,51,0.3); }
  .redX { background-color: rgba(255,0,0,0.6); color: #FFF; text-align: center; font-weight: bold; transition: background-color 0.2s; border: none; }
  .redX:hover, .redX:focus { cursor: pointer; background-color: #F00; }
  div[data-filterid=searchTips] p { margin: 2px 5px; }
  div[data-filterid=searchTips] ul { margin-left: 25px; margin-bottom: 20px; }
  div[data-filterid=searchTips] ul li { margin-bottom: 2px; }

  /* Media Queries */
  @media (max-width: 850px) {
    .result.list .keywords { display: none; }
    .result.list .descriptionBox { width: calc(100% - 200px); padding-right: 10px; }
  }
  @media (max-width: 800px) {
    #queryBox { padding-left: 150px; }
    #btnToggleFilters { width: 150px; }
    #filters { width: 150px; }
    #tags .tagList { min-width: 250px; margin-left: 150px; }
    #tags .tagList.standards { width: calc(100% - 150px); }
  }
  @media (max-width: 550px) {
    #queryBox { padding-left: 0; }
    #btnToggleFilters, #txtSearch { width: 100%; display: block; }
    #btnToggleFilters { position: static; border-radius: 0; }
    #txtSearch { border-radius: 5px 5px 0 0; }
    #ddls select { width: 100%; display: block; }
    #ddls #ddlSortOrder, #ddls #ddlPageSize { border-radius: 0; }
    #ddls #ddlViewMode { border-radius: 0 0 5px 5px; }
    #filters { top: 80px; width: 125px; }
    #tags { top: 80px; }
    #tags .tagList { width: calc(100% - 150px); min-width: 150px; margin-left: 125px; }
    #tags .tagList.standards { width: 100%; margin-left: 0; border-radius: 0 0 5px 5px; }
    #tags .tagList.standards h2 { border-radius: 0; }
  }
  @media (max-width: 500px) {
    .result.list .descriptionBox { width: 70%; }
    .result.list .images { width: 30%; }
    .result.list .created { display: none; }
  }
</style>

<div class="theme">
  <div id="themeHeaderContent"></div>
  <div id="searchHeader">
    <!-- Filters and Text Search -->
    <div id="queryBox">
      <input type="text" id="txtSearch" class="txtSearch" placeholder="Start typing here to search..." value="<%=PreselectedText %>" /><!--
      --><input type="button" id="btnToggleFilters" class="btnToggleFilters collapsed" value="Filters" onclick="toggleFilters()" />
    </div>
    <!-- Filters -->
    <div id="filters" class="collapsed">
      <div id="categories">
        <input type="button" class="btnCategory" data-filterID="searchTips" value="Search Tips" onclick="toggleFilter(this)" />
      <% foreach(var item in Filters){ %>
        <input type="button" class="btnCategory" data-filterID="<%=item.Id %>" value="<%=item.Title %>" onclick="toggleFilter(this)" />
      <% } %>
      </div><!--
      -->
    </div>
    <div id="tags">
      <div class="tagList" data-filterID="searchTips">
        <h2>Search Tips</h2>
        <p>You can wrap phrases in double quotes ( " ) to search for whole phrases:</p>
        <ul>
          <li><a href="#" onclick="searchFor(this.innerHTML, false)">"3rd grade" literacy</a> <span>(Literacy Resources for Third-Graders)</span></li>
          <li><a href="#" onclick="searchFor(this.innerHTML, false)">"pythagorean theorem"</a></li>
          <li><a href="#" onclick="searchFor(this.innerHTML, false)">"dinosaur footprints"</a></li>
        </ul>
        <p>You can use + or - to require/exclude words or phrases:</p>
        <ul>
          <li><a href="#" onclick="searchFor(this.innerHTML, false)">+trees -forest</a> <span>(Finds Resources about trees, avoiding Resources about forests)</span></li>
          <li><a href="#" onclick="searchFor(this.innerHTML, false)">formula -chemistry</a> <span>(Finds Non-Chemistry Resources about Formulas)</span></li>
          <li><a href="#" onclick="searchFor(this.innerHTML, false)">formula +chemistry</a> <span>(Finds Only Chemistry Resources about Formulas)</span></li>
          <li><a href="#" onclick="searchFor(this.innerHTML, false)">geometry +"isosceles triangle"</a> <span>(Finds Geometry Resources focusing on Isosceles Triangles)</span></li>
        </ul>
        <p>You can use | to look for one term/phrase or another:</p>
        <ul>
          <li><a href="#" onclick="searchFor(this.innerHTML, false)">math|english</a></li>
          <li><a href="#" onclick="searchFor(this.innerHTML, false)">trees forest|math</a></li>
          <li><a href="#" onclick="searchFor(this.innerHTML, false)">"civil rights"|"civil engineering"</a> <span>(Finds Civil Rights or Civil Engineering Resources)</span></li>
        </ul>
        <p>Put them together. Go nuts!</p>
        <ul>
          <li><a href="#" onclick="searchFor(this.innerHTML, false)">"grade 3" geometry -triangles</a> <span>(Finds Grade 3 Geometry Resources about things other than Triangles)</span></li>
          <li><a href="#" onclick="searchFor(this.innerHTML, false)">"charles dickens" +"christmas carol" film|video</a> <span>(Finds film or video Resources about Charles Dickens' <u>A Christmas Carol</u>)</span></li>
          <li><a href="#" onclick="searchFor(this.innerHTML, false)">history|biography +war -ww2 -"world war 2" -"world war II"</a> <span>(Finds History or Biography Resources about war, but not about World War 2)</span></li>
          <li><a href="#" onclick="searchFor(this.innerHTML, false)">+free math "grade 3"|"third grade"|"3rd grade" game|activity -test -quiz application/software student</a> <span>(Finds Free Math Resources for Third Graders that are Games or Activities but not Tests or Quizzes, and is software meant for students.)</span></li>
        </ul>
      </div>
    <% foreach(var item in Filters){ %>
      <div class="tagList" data-filterID="<%=item.Id %>">
        <h2><%=item.Title %></h2>
        <div class="tagCBXL">
        <% foreach(var tag in item.Tags) { %>
          <label><input type="checkbox" name="cbx_<%=tag.Id %>" data-id="<%=tag.Id %>" <%=(PreselectedTags.Contains(tag.Id) ? "checked=\"checked\"" : "") %> /> <%=tag.Title %></label>
        <% } %>
        </div>
      </div>
    <% } %>
    </div>
    <!-- Drop-down lists -->
    <div id="ddls">
      <select id="ddlSortOrder">
        <option value="|">Relevance</option>
        <option value="ResourceId|desc" selected="selected">Newest First</option>
        <option value="ResourceId|asc">Oldest First</option>
        <option value="Paradata.ResourceViews|desc">Most Visited</option>
        <option value="Paradata.Rating|desc">Best Rated</option>
        <option value="Paradata.EvaluationsScore|asc">Worst Rated</option>
        <option value="Paradata.Comments|desc">Most Commented</option>
        <option value="Paradata.Favorites|desc">Most Favorited</option>
      </select><!--
      --><select id="ddlPageSize">
        <option value="20">Show 20 Items</option>
        <option value="50">Show 50 Items</option>
        <option value="100">Show 100 Items</option>
      </select><!--
      --><select id="ddlViewMode">
        <option value="list" <%=( ViewMode == "list" ? "selected=\"selected\"" : "" ) %>>List View</option>
        <option value="grid" <%=( ViewMode == "grid" ? "selected=\"selected\"" : "" ) %>>Grid View</option>
        <option value="text" <%=( ViewMode == "text" ? "selected=\"selected\"" : "" ) %>>Text-Only</option>
      </select>
    </div>
    <!-- Selected tags -->
    <div id="selectedTags"></div>
  </div><!-- /searchHeader -->
  <div id="status"></div>
  <div id="paginatorTop" class="paginator top"></div>
  <div id="searchResults" data-mode="<%=ViewMode %>"></div><!-- /searchResults -->
  <div id="paginatorBottom" class="paginator bottom"></div>

  <% if(UseStandards){ %>
  <style type="text/css">
    .theme { padding-right: 30px; }
    #standardsBrowserFloat { position: absolute; top: 200px; height: 550px; right: 0; background-color: rgba(240,240,240,0.95); border-radius: 5px 0 0 5px; transition: width 0.5s; z-index: 1000; }
    #standardsBrowserFloat.expanded { width: calc(100% - 100px); }
    #standardsBrowserFloat.collapsed #SB7 { overflow: hidden; padding-left: 0; padding-right: 0; }
    #standardsBrowserFloat.collapsed { width: 0; }
    #standardsBrowserFloat #SB7 { height: 100%; transition: padding 0.5s; }
    #standardsBrowserFloat #SB7 #SBleftColumn { height: calc(100% - 30px); }
    #standardsBrowserFloat #SB7 #SBtree { max-height: 100%; overflow-x: hidden; }
    #btnToggleStandardsSide { transform: rotate(-90deg); transform-origin: bottom left; -webkit-transform: rotate(-90deg); -webkit-transform-origin: bottom left; -ms-transform: rotate(-90deg); -ms-transform-origin: bottom left; position: absolute; height: 35px; width: 250px; left: 0; top: calc(50% + 85px); display: block; border: 1px solid #CCC; border-radius: 5px 5px 0 0; }
    #standardsBrowserFloat.expanded #btnToggleStandardsSide { background-color: #9984BD; }
    @media screen and (max-width: 600px) {
      #standardsBrowserFloat #SB7 #SBleftColumn { height: calc(100% - 100px); }
    }
  </style>
  <div id="standardsBrowserFloat" class="collapsed">
    <input type="button" id="btnToggleStandardsSide" value="Standards Browser" class="isleButton bgBlue" onclick="showHideStandardsBrowser();" />
    <uc1:Standards ID="standardsBrowserSide" runat="server" />
  </div>
  <% } %>
</div>

<div id="templates" style="display:none;">
  <script type="text/template" id="template_thumbnail">
    <div class="thumbnail" style="background-image:url('{imageURL}');"><img src="/images/ThumbnailResizer.png" /></div>
  </script>
  <script type="text/template" id="template_result_list">
    <div class="result list" data-resourceID="{resourceID}" data-versionID="{versionID}">
      <a class="title resourceLink" href="{resultURL}" target="resultWindow">{title}</a>
      <div class="modBox_before" data-resourceID="{resourceID}"></div>
      <div class="resultContent">
        <div class="descriptionBox">
          <div class="description" data-collapsed="collapsed">{description}</div>
          <div class="expandCollapseBox">
            <input type="button" onclick="toggleExpandCollapse({resourceID}, this);" value="More" />
          </div>
          <div class="links">
            <a class="creator" href="#" onclick="searchFor('{creator}', true);">{creator}</a>
            {standards}
          </div>
          <div class="modBox_middle"></div>
        </div><!--
     --><div class="keywords">{keywords}</div><!--
     --><div class="images">
          <a href="{resultURL}" target="resultWindow" class="thumbnailBox resourceLink">
            {thumbnail}
            <div class="paradata">{paradata}</div>
            <div class="created">Created {created}</div>
          </a>
        </div>
      </div>
      <div class="modBox_after"></div>
    </div>
  </script>
  <script type="text/template" id="template_result_grid">
    <a class="result grid resourceLink" data-resourceID="{resourceID}" href="{resultURL}" target="resultWindow">
      <div class="modBox_before"></div>
      <div class="thumbnailBox">
        {thumbnail}
        <div class="paradata">{paradata}</div>
      </div>
      <div class="title">{title}</div>
      <div class="modBox_after"></div>
    </a>
  </script>
  <script type="text/template" id="template_result_text">
    <div class="result text" data-resourceID="{resourceID}">
      <div class="modBox_before"></div>
      <a href="{resultURL}" target="resultWindow" class="resourceLink">{title}</a>
      <div class="description">{description}</div>
      <div class="modBox_after"></div>
    </div>
  </script>
  <script type="text/template" id="template_selectedTag">
    <div class="selectedTag" data-id="{id}">{text} <input type="button" value="X" class="redX" onclick="removeTag('{id}', {isStandard})" /></div>
  </script>
</div>
