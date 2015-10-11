<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SearchV6.ascx.cs" Inherits="IOER.Controls.SearchV6.SearchV6" %>
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
  var currentResults = <%=InitialResultsJSON %>;
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
  var fileIcons = [
    { match: [ ".zip", ".rar", ".tar", ".gz", ".7z" ], icon: "/images/icons/icon_zip_400x300.png" },
    { match: [ ".avi", ".mp4", ".mpg", ".flv", ".3gp" ], icon: "/images/icons/icon_video_400x300.png" },
    { match: [ ".ram", ".mp3", ".wma", ".ogg", ".wav" ], icon: "/images/icons/icon_audio_400x300.png"  },
		{ match: [ ".jnlp", ".tga", ".tif", ".tiff", "view.d2l" ], icon: "/images/icons/icon_download_400x300.png" }
  ];
	var usageRightsData = <%=UsageRightsIconsJSON %>;

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
    setupAutoSearch();
    $("form").attr("onsubmit", "return false;");
    setupPreselectedSortOrder();
  	//Setup accessibility items
    setupAccessibility();
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
    if(window.location.search.indexOf("text=") > -1){
      sortMode = { field: "", order: "" };
      return;
    }
    if(window.location.search.indexOf("sort") == -1){ 
      setTimeout(function() {
        var sortModeParts = $("#ddlSortOrder option:selected").attr("value").split("|");
        sortMode = { field: sortModeParts[0], order: sortModeParts[1] };
      }, 805);
    }
    else {
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
    $("html").not("#btnToggleFilters, #filters, #tags, #standardsBrowserBox, #btnToggleStandardsBrowser, #skipButtons, #btnToggleSearchTips, #searchTips").on("click", function () {
    	$("#btnToggleFilters, #btnToggleStandardsBrowser, #filters, #tags, #standardsBrowserBox").removeClass("expanded").addClass("collapsed");
    	$("#standardsBrowserBox, #searchTips").slideUp(250);
    	$(".btnCategory, .tagList").removeClass("selected");
    	$(".collapseButton, .collapsible").removeClass("expanded").addClass("collapsed");
    });
    $("#btnToggleFilters, #filters, #tags, #standardsBrowserBox, #btnToggleStandardsBrowser, #skipButtons, #btnToggleSearchTips, #searchTips").on("click", function (e) {
      e.stopPropagation();
    });
    $("#btnToggleFilters, #filters, #tags").on("click", function() {
      $("#standardsBrowserBox, #searchTips").removeClass("expanded").addClass("collapsed").slideUp(250);
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
      $("#standardsBrowserBox").removeClass("expanded").addClass("collapsed").slideUp(250);
    });
    $(window).on("standard_added standard_removed", function() {
      pipeline("updateFiltering");
    });
  }

	//Setup accessibility things
  function setupAccessibility(){
		//Enable skipping to contents with home key - temp change to the escape key (27)
  	$(window).on("keyup", function(e) {
  		if(e.which == 27 || e.keyCode == 27){
  			$("#skipButtons").show().focus();
  			$(".collapsible, .collapseButton").removeClass("expanded").addClass("collapsed");
  			$("#standardsBrowserBox, #searchTips").slideUp(250);
  			$(".btnCategory, .tagList").removeClass("selected");
  		}
  	});
  	//Show skipbuttons if user comes upon it while tabbing
  	$("#skipLinkTarget, #skipButtons, h1").on("focus", function() {
  		$("#skipButtons").show();
  		$(".btnAccessibilityFilterHelper").show();
  	});
  	//Hide skipbuttons with alt-H
  	$(window).on("keydown", function(e){
  		if((e.which == 72 || e.keyCode == 72) && e.altKey){
  			$("#skipButtons").hide();
  		}
  	});

  }

	//Handle auto search
  function setupAutoSearch() {
  	var location = window.location.href.toLowerCase();
		//If there is no preexisting data or the page is not the main search or there is a user-entered query, do an auto-search
  	//if(typeof(currentResults.hits) == "undefined" || location.indexOf("/search") == -1 || location.indexOf("text=") > -1){
  	if(typeof(currentResults.hits) == "undefined"|| location.indexOf("text=") > -1) {
  		pipeline("updateFiltering");
  		pipeline("resetCountdown");
  	}
  	else {
  		pipeline("loadResults");
  	}
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
  	console.log("test");
    selectedStandardsIDs_search = [];
    if(usingStandards){
      selectedStandardsIDs_search = allSelectedIDs;
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
    $(window).trigger("resultsLoaded", currentResults.hits.total);
  }

  function toggleCollapsible(target, slide){
  	var button = $(".collapseButton[data-collapsibleID=" + target + "]");
  	var box = $(".collapsible[data-collapsibleID=" + target + "]");
  	var collapsed = button.hasClass("collapsed");
  	$(".collapseButton, .collapsible").removeClass("expanded").addClass("collapsed");
  	$(".collapsible[data-slide=true]").slideUp(250);
  	$(".tagList").removeClass("selected");
  	if(collapsed){
  		button.removeClass("collapsed").addClass("expanded");
  		box.removeClass("collapsed").addClass("expanded");
  		if(slide){
  			box.slideDown(250);
  		}
  		box.focus();
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
      filter.find("label").first().focus();
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
  function searchFor(text, wrapInQuotes, prefix) {
  	$("#txtSearch").val(text);

    if(wrapInQuotes){
      $("#txtSearch").val('"' + text + '"');
    }

    if(typeof(prefix) != "undefined" && prefix != null && prefix.length > 0){
    	$("#txtSearch").val(prefix.replace(":", "") + ":" + $("#txtSearch").val());
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
  
	//Accessibility skip functions
  function skipToSearchBar() { $("#txtSearch").focus(); }
  function skipToFilters() { $("#btnToggleFilters").focus(); }
  function skipToStandards() { 
  	if($("#btnToggleStandardsBrowser").hasClass("collapsed")){
  		toggleCollapsible('standards', true);
  	}
  	$("#SBddlBody").focus(); 
  }
  function skipToStandardsSearch() {
  	if($("#btnToggleStandardsBrowser").hasClass("collapsed")){
  		toggleCollapsible('standards', true);
  	}
  	$("#SBbtnSearch").focus();
  }
  function skipToSearchOptions() { $("#ddlSortOrder").focus(); }
  function skipToSearchResults() { $("#status").trigger("click").focus(); }
  function skipToFilter(id){ $("#filters input[data-filterID=" + id + "]").focus(); }
  function skipToSearchTips() { $("#btnToggleSearchTips").focus(); }
  function hideSkipButtons() { $("#skipButtons").hide(); }
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
        if(current == null || current.ResourceId == 0){ continue; }
        var link = usingResourceUrl ? current.Url : "/Resource/" + current.ResourceId + "/" + ( current.UrlTitle == null ? "" : current.UrlTitle).replace(/&/g, "" ) + getLibColIDs();
        //Get thumbnail
        //var thumbnailText = thumbnailTemplate.replace(/{resultURL}/g, link).replace(/{imageURL}/g, "//ioer.ilsharedlearning.org/OERThumbs/large/" + current.ResourceId + "-large.png");
        var thumbnailText = thumbnailTemplate.replace(/{resultURL}/g, link).replace(/{imageURL}/g, getThumbnail(current)).replace(/{title}/g, current.Title);
        //Get keywords
        var keywords = getKeywords(current);
        //Get paradata
        var paradata = getParadata(current);
        //Get standards
        var standards = getStandards(current);
      	//Get usage rights mini icon
        var usageRightsIcon = getUsageRightsIcon(current);
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
						.replace(/{usageRightsIcon}/g, usageRightsIcon)
        );

      }
      //Trigger any extra theme-based result injections
      $(window).trigger("resultsRendered");
      box.hide().fadeIn();
    }
  }
  function getThumbnail(current){
    for(var i in fileIcons){
      for(var j in fileIcons[i].match){
        if(current.Url.toLowerCase().indexOf(fileIcons[i].match[j]) > -1){
          return fileIcons[i].icon;
        }
      }
    }
    return "//ioer.ilsharedlearning.org/OERThumbs/large/" + current.ResourceId + "-large.png";
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
      result += "<a href=\"#\" onclick=\"searchFor('" + list[i] + "', false); return false;\" title=\"Search for " + list[i] + "\">" + list[i] + "</a>";
    }
    return result;
  }
  //Get a list of paradata icons
  function getParadata(current) {
    var result = "";
    var data = current.Paradata;
    var paradataFormat = [
      { img: "icon_comments", data: data.Comments, tip: "Comments: Number of comments this Resource has received", valid: data.Comments > 0 },
      { img: "icon_likes", data: data.Likes, tip: "Likes: Number of people who like this Resource", valid: data.Likes > 0 },
      { img: "icon_dislikes", data: data.Dislikes, tip: "Dislikes: Number of people who don't like this Resource", valid: data.Dislikes > 0 },
      //{ img: "icon_ratings", data:  ((data.EvaluationsScore / 3) * 100).toString().split(".")[0] + "%", tip: "Rating: An overall evaluation score for this Resource", valid: data.Evaluations > 0 },
      { img: "icon_ratings", data:  data.EvaluationsScore + "%", tip: "Rating: An overall evaluation score for this Resource", valid: data.Evaluations > 0 },
      { img: "icon_library", data: current.LibraryIds.length, tip: "Favorites: Number of IOER Libraries this Resource has been added to", valid: data.Favorites > 0 },
      { img: "icon_click-throughs", data: data.ResourceViews, tip: "Click-throughs: Number of times this Resource has been visited",  valid: data.ResourceViews > 0 }
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
	//May need to improve this later with "show all" functionality
  function getStandards(current) {
    var result = "";
    if (current.StandardNotations.length == 0) { return ""; }
    var max = 25;
    var count = 0;
    for (i in current.StandardNotations) {
    	if(count <= max){ 
    		result += "<a href=\"#\" onclick=\"searchFor('standard:" + current.StandardNotations[i] + "'); return false;\" title=\"" + current.StandardNotations[i] + "\">" + current.StandardNotations[i] + "</a>";
    		count++;
    	}
    	else {
    		break;
    	}
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
	//Get usage rights mini icon
  function getUsageRightsIcon(current) {
  	if(current.UsageRightsUrl == "" || current.UsageRightsUrl == null || current.UsageRightsUrl.toLowerCase() == "unknown"){
  		return "<img class='rightsIcon' title='Usage Rights Unknown' alt='Usage Rights Unknown' src='/images/icons/rightsunknownmini.png' />";
  	}
  	for(var i in usageRightsData){
  		if(current.UsageRightsUrl == usageRightsData[i].Url){
  			return "<a class='rightsIcon' title='" + current.Title + "' href='" + current.UsageRightsUrl + "' target='_blank'><img alt='" + current.Title + "' src='" + usageRightsData[i].MiniIconUrl + "' /></a>";
  		}
  	}
  	return "<a class='rightsIcon' title='Custom Usage Rights - Click for Details' href='" + current.UsageRightsUrl + "' target='_blank'><img alt='Custom Usage Rights - Click for Details' src='/images/icons/rightsreserved.png' /></a>";
  }
	//If in the context of a library and/or collection, append these to the URL
  function getLibColIDs() {
  	if(typeof(libraryData) == "object"){
  		var libID = libraryData.library.id;
  		var tempActive = getActive();
  		var colID = tempActive.isLibrary ? 0 : tempActive.data.id;
  		return "?libId=" + libID + "&colId=" + colID;
  	}
  	else {
  		return "";
  	}
  }

  //Render Paginators
  function renderPaginators(){
    var boxes = $(".paginator");
    var totalResults = currentResults.hits.total;
    if(totalResults == 0){
      boxes.html("");
      return;
    }

    boxes.html("Page: ");
    
    var totalPages = Math.ceil(totalResults / pageSize);
    var skips = [1, 5, 10, 25, 50, 500, 1000, 2500, 5000, 10000, totalPages];
    for(var i = 1; i <= totalPages; i++){
      var page = i;
      if(page >= (currentStartPage - 2) && page <= (currentStartPage + 2) || skips.indexOf(i) > -1 || i == totalPages){
        boxes.append("<input type=\"button\" value=\"" + page + "\" class=\"" + ((currentStartPage == page) ? "current" : "") + "\" title=\"" + ((currentStartPage == page) ? "Currently on page " : "Jump to results page ") + page + "\" onclick=\"jumpToPage(" + i + ");\" />");
      }
    }

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
  	//Fix broken URLs in quotes
    text = text.replace(new RegExp("http://", "g"), "").replace(new RegExp("https://"), "g");

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
  #txtSearch { font-size: 26px; }
  #buttons input { height: 35px; font-size: 22px; font-weight: bold; line-height: 30px; display: inline-block; vertical-align: top; width: 33.333%; transition: background-color 0.1s, color 0.1s; border-width: 1px; }
  #buttons input:hover, #buttons input:focus { background-color: #FF6A00; color: #FFF; }
	#buttons input:first-child { border-radius: 0 0 0 5px; }
	#buttons input:last-child { border-radius: 0 0 5px 0; }
  #txtSearch { width: 100%; border-radius: 5px 5px 0 0; padding-left: 6px; background-image: linear-gradient(#EEE, #FFF 50%, #FFF 85%, #EFEFEF); line-height: 30px; }
  #ddls { white-space: nowrap; }
  #ddls select { width: 33.333%; font-size: 20px; height: 35px; cursor: pointer; border-radius: 0; }
  #selectedTags { padding: 5px 0; }
  .selectedTag { margin: 2px 1px; border-radius: 5px; background-color: #EEE; padding: 2px 35px 2px 5px; height: 25px; display: inline-block; vertical-align: top; position: relative; max-width: 275px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
  .selectedTag input { display: inline-block; height: 100%; width: 25px; border-radius: 0 5px 5px 0; position: absolute; top: 0; right: 0; }
	#btnToggleFilters, #btnToggleStandardsBrowser, #btnToggleSearchTips { background-image: url('/images/arrow-down-white.png'); background-position: right 5px center; background-repeat: no-repeat; background-size: auto 60%; }
	#skipButtons { max-width: 800px; margin: 15px auto; }
	#standardsBrowserBox, #searchTips { position: absolute; z-index: 95; width: 100%; display: none; }
  #standardsBrowserBox #SB7, #searchTips { padding: 10px; background-color: rgba(240,240,240,0.95); border-radius: 0 0 5px 5px; box-shadow: 0 5px 15px rgba(0,0,0,0.3); }
	#searchTips h2 { font-size: 20px; }
	#searchTips p a { display: inline-block; padding: 2px 5px; border: 1px solid #CCC; border-radius: 5px; }

  /* Filters */
  #filters { border-radius: 0 0 5px 5px; position: absolute; top: 105px; left: 0; width: 25%; margin-bottom: 50px; height: 0; transform-origin: top center; -webkit-transform-origin: top center; transition: transform 0.3s, -webkit-transform 0.3s, -ms-transform 0.3s; display: block; z-index: 100; box-shadow: 0 5px 15px rgba(0,0,0,0.3); }
  #filters.expanded { height: auto; transform: scaleY(1); -webkit-transform: scaleY(1); -ms-transform: scaleY(1); }
  #filters.collapsed { transform: scaleY(0); -webkit-transform: scaleY(0); -ms-transform: scaleY(0); }
  #filters #categories, #tags { display: inline-block; vertical-align: top; }
  #filters #categories { width: 100%; }
  #filters #categories input { width: 100%; border-radius: 0; padding: 5px; text-align: right; white-space: normal; font-weight: bold; border-width: 1px; transition: background-color 0.1s, color 0.1s; }
  #filters #categories input:last-child { border-radius: 0 0 0 5px; }
  #filters #categories input:hover, #filters #categories input:focus { background-color: #FF6A00; }
  #tags { position: absolute; top: 105px; left: 25%; z-index: 150; width: 100%; }
  #tags .tagList h2 { padding: 6px 10px; border-radius: 0 5px 0 0; }
  #tags .tagList .tagCBXL { padding: 5px; box-shadow: 0 1px 10px rgba(0,0,0,0.3); }
  #tags .tagList .tagCBXL label { display: block; padding: 3px 5px; border-radius: 5px; }
  #tags .tagList .tagCBXL label:hover, #tags .tagList .tagCBXL label:focus { cursor: pointer; background-color: #FDFDFD; }
  #tags .tagList { min-width: 275px; display: block; background-color: rgba(240,240,240,0.95); border-radius: 0 5px 5px 0; position: absolute; top: 0; margin-bottom: 50px; transform-origin: left center; -webkit-transform-origin: left center; transition: transform 0.2s, -webkit-transform 0.2s, -ms-transform 0.2s; transform: scaleX(0); -webkit-transform: scaleX(0); -ms-transform: scaleX(0); }
  #tags .tagList.selected { transform: scaleX(1); -webkit-transform: scaleX(1); -ms-transform: scaleX(1); }
  #tags .tagList.standards { width: calc(100% - 200px); }
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
  .result.list { padding: 5px; border-radius: 5px; background-image: linear-gradient(#F5F5F5, #FFF); position: relative; }
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
	.result.list .links a.creator { width: 100%; display: block; }
  .result.list .title { font-size: 20px; font-weight: bold; padding: 2px 90px 5px 2px; display: block; }
	.result.list .rightsIcon { position: absolute; top: 10px; right: 5px; display: inline-block; border: none; }

  .result.grid { width: 250px; display: inline-block; vertical-align: top; margin: 15px 1%; box-shadow: 0 0 15px -1px #CCC; padding: 5px; border-radius: 5px; transition: box-shadow 0.2s; }
  .result.grid .thumbnailBox { position: relative; margin: -5px -5px 5px -5px; }
  .result.grid .title { font-size: 18px; min-height: 4em; display: block; padding: 2px 5px; word-break: break-word; word-wrap: break-word; }
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
  #status { padding: 10px; margin: 3px 0 0 0; text-align: center; transition: background 0.8s, padding 0.8s; border-radius: 5px; }
  #status.searching { background-color: rgba(0,255,0,0.6); }
  #status.error { padding: 50px; background-color: rgba(255,0,0,0.2); }
  #status.caution { padding: 15px; background-color: rgba(255,204,51,0.3); }
  .redX { background-color: rgba(255,0,0,0.6); color: #FFF; text-align: center; font-weight: bold; transition: background-color 0.2s; border: none; }
  .redX:hover, .redX:focus { cursor: pointer; background-color: #F00; }
  div[data-filterid=searchTips] p { margin: 2px 5px; }
  div[data-filterid=searchTips] ul { margin-left: 25px; margin-bottom: 20px; }
  div[data-filterid=searchTips] ul li { margin-bottom: 2px; }
	.btnAccessibilityFilterHelper { display: none; }

  /* Media Queries */
  @media (max-width: 850px) {
    .result.list .keywords { display: none; }
    .result.list .descriptionBox { width: calc(100% - 200px); padding-right: 10px; }
  }
	@media (max-width: 750px) {
		#buttons input { width: 100%; }
		#filters { top: 175px; width: 125px; }
		#tags { top: 175px; left: 125px; }
		#standardsBrowserBox { top: 206px; }
	}
  @media (max-width: 625px) {
    #queryBox { padding-left: 0; }
    #btnToggleFilters, #txtSearch { width: 100%; display: block; }
    #btnToggleFilters { position: static; border-radius: 0; }
    #txtSearch { border-radius: 5px 5px 0 0; }
    #ddls select { width: 100%; display: block; }
    #ddls #ddlSortOrder, #ddls #ddlPageSize { border-radius: 0; }
    #ddls #ddlViewMode { border-radius: 0 0 5px 5px; }
  }
  @media (max-width: 500px) {
    .result.list .descriptionBox { width: 70%; }
    .result.list .images { width: 30%; }
    .result.list .created { display: none; }
  }
</style>

<div class="theme">
  <div id="themeHeaderContent"></div>
	<div id="skipButtons" class="grayBox" tabindex="0" style="display: none;">
		<div id="hideSkipbuttons"><input type="button" class="isleButton bgBlue" id="btnHideSkipButtons" onclick="hideSkipButtons();" value="Hide this section (Alt + H)" /></div>
		<p>Press the Escape key at any time to jump back to this menu. Use the items here to skip to major sections of this page.</p>
		<p>Any time you change the text of the search text box or change your filter selection, the search results are automatically updated.</p>
		<% if(UseStandards) { %><p>If you select one or more learning standards, you will need to press the Search button within the standards browser to perform a search.</p><% } %>
		<input type="button" class="isleButton bgBlue" onclick="skipToSearchBar();" value="Skip to Search text box" />
		<input type="button" class="isleButton bgBlue" onclick="skipToSearchOptions();" value="Skip to Search Options" />
		<input type="button" class="isleButton bgBlue" onclick="skipToFilters();" value="Skip to Filters" />
		<% if(UseStandards){  %>
		<input type="button" class="isleButton bgBlue" onclick="skipToStandards();" value="Skip to Standards Browser" />
		<input type="button" class="isleButton bgBlue" onclick="skipToStandardsSearch();" value="Skip to Standards Browser Search Button" />
		<% } %>
		<input type="button" class="isleButton bgBlue" onclick="skipToSearchTips();" value="Skip to Search Tips" />
		<input type="button" class="isleButton bgBlue" onclick="skipToSearchResults();" value="Skip to Search Results" />
	</div>
  <div id="searchHeader">
		<div id="queryBox">
      <input type="text" id="txtSearch" class="txtSearch" placeholder="Start typing here to search..." value='<%=PreselectedText %>' />
			<div id="ddls">
				<select title="Sort Order" id="ddlSortOrder">
					<option value="|" selected="selected">Most Relevant</option>
					<option value="ResourceId|desc">Newest First</option>
					<option value="ResourceId|asc">Oldest First</option>
					<option value="Paradata.ResourceViews|desc">Most Visited</option>
					<option value="Paradata.Likes|desc">Most Total Likes</option>
					<option value="Paradata.Rating|desc">Most Net Likes vs Dislikes</option>
					<option value="Paradata.Rating|asc">Most Net Dislikes vs Likes</option>
					<option value="Paradata.Dislikes|desc">Most Total Dislikes</option>
					<option value="Paradata.EvaluationsScore|desc">Best Net Evaluation Score</option>
					<option value="Paradata.Evaluations|desc">Most Total Evaluations</option>
					<option value="Paradata.Favorites|desc">In the Most Libraries</option>
					<option value="Paradata.Comments|desc">Most Comments</option>
				</select><!--
				--><select title="Page Size" id="ddlPageSize">
					<option value="20">Show 20 Items</option>
					<option value="50">Show 50 Items</option>
					<option value="100">Show 100 Items</option>
				</select><!--
				--><select title="Display Format" id="ddlViewMode">
					<option value="list" <%=( ViewMode == "list" ? "selected=\"selected\"" : "" ) %>>List View</option>
					<option value="grid" <%=( ViewMode == "grid" ? "selected=\"selected\"" : "" ) %>>Grid View</option>
					<option value="text" <%=( ViewMode == "text" ? "selected=\"selected\"" : "" ) %>>Text-Only</option>
				</select>
			</div>
		</div>
		<div id="buttons">
			<input type="button" id="btnToggleFilters" class="collapseButton collapsed" value="Filter by Tags" data-collapsibleID="filters" onclick="toggleCollapsible('filters', false)" /><!--
				<% if(UseStandards){  %>
      --><input type="button" id="btnToggleStandardsBrowser" class="collapseButton collapsed" data-collapsibleID="standards" onclick="toggleCollapsible('standards', true)" value="Filter by Standards" /><!--
				<% } %>
			--><input type="button" id="btnToggleSearchTips" class="collapseButton collapsed" data-collapsibleID="tips" onclick="toggleCollapsible('tips', true)" value="Search Tips" />
		</div>

    <!-- Filters -->
    <div id="filters" data-collapsibleID="filters" data-slide="false" class="collapsible collapsed" tabindex="0">
      <div id="categories">
      <% foreach(var item in Filters){ %>
        <input type="button" class="btnCategory" data-filterID="<%=item.Id %>" value="<%=item.Title %>" onclick="toggleFilter(this)" />
      <% } %>
      </div><!--
      -->
    </div>
    <div id="tags">
    <% foreach(var item in Filters){ %>
      <div class="tagList" data-filterID="<%=item.Id %>">
        <h2><%=item.Title %></h2>
        <div class="tagCBXL">
        <% foreach(var tag in item.Tags) { %>
          <label><input type="checkbox" name="cbx_<%=tag.Id %>" data-id="<%=tag.Id %>" <%=(PreselectedTags.Contains(tag.Id) ? "checked=\"checked\"" : "") %> title="<%=tag.Title %>" /> <%=tag.Title %></label>
        <% } %>
					<input type="button" class="isleButton bgBlue btnAccessibilityFilterHelper" value="Back to Filter List" onclick="skipToFilter(<%=item.Id %>);" />
        </div>
      </div>
    <% } %>
    </div>
    <!-- Drop-down lists -->
    <% if(UseStandards){ %>
    <div id="standardsBrowserBox" data-collapsibleID="standards" data-slide="true" class="collapsible collapsed" tabindex="0">
      <uc1:Standards ID="standardsBrowser" runat="server" />
    </div>
    <% } else { %>
		<style type="text/css">
			#buttons input { width: 50%; }
			@media (max-width: 750px) {
				#filters { top: 105px; }
				#tags { top: 105px; }
			}
			@media (max-width: 625px) {
				#filters { top: 175px; width: 125px; }
				#tags { top: 175px; left: 125px; }
			}
		</style>
		<% } %>
		<div id="searchTips" data-collapsibleID="tips" data-slide="true" class="grayBox collapsible collapsed" tabindex="0">
      <h2 class="header">Search Tips</h2>
			<p>You can limit searches to specific fields by typing the field's name followed by : and entering the rest of your query. Separate fields with a comma to look in multiple fields.</p>
			<p>Currently-supported fields include: 
				<a href="#" onclick="searchFor(this.innerHTML, false);">resourceid:</a>
				<a href="#" onclick="searchFor(this.innerHTML, false);">title:</a>
				<a href="#" onclick="searchFor(this.innerHTML, false);">description:</a>
				<a href="#" onclick="searchFor(this.innerHTML, false);">url:</a>
				<a href="#" onclick="searchFor(this.innerHTML, false);">creator:</a>
				<a href="#" onclick="searchFor(this.innerHTML, false);">publisher:</a>
				<a href="#" onclick="searchFor(this.innerHTML, false);">submitter:</a>
				<a href="#" onclick="searchFor(this.innerHTML, false);">keywords:</a>
				<a href="#" onclick="searchFor(this.innerHTML, false);">standard:</a>
			</p>
			<ul>
				<li><a href="#" onclick="searchFor(this.innerHTML, false);">creator:ioer</a></li>
				<li><a href="#" onclick="searchFor(this.innerHTML, false);">creator,publisher:ioer</a></li>
				<li><a href="#" onclick="searchFor(this.innerHTML, false);">standard,title:math</a></li>
			</ul>
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

    <!-- Selected tags -->
    <div id="selectedTags"></div>
  </div><!-- /searchHeader -->
  <div id="status" tabindex="0"></div>
  <div id="paginatorTop" class="paginator top" style="display:none;"></div>
  <div id="searchResults" data-mode="<%=ViewMode %>"></div><!-- /searchResults -->
  <div id="paginatorBottom" class="paginator bottom"></div>

</div>

<div id="templates" style="display:none;">
  <script type="text/template" id="template_thumbnail">
    <div class="thumbnail" style="background-image:url('{imageURL}');" title="Thumbnail: {title}"><img alt="" src="/images/ThumbnailResizer.png" /></div>
  </script>
  <script type="text/template" id="template_result_list">
    <div class="result list" data-resourceID="{resourceID}" data-versionID="{versionID}">
			{usageRightsIcon}
      <a class="title resourceLink" href="{resultURL}" target="ioerDtl">{title}</a>
      <div class="modBox_before" data-resourceID="{resourceID}"></div>
      <div class="resultContent">
        <div class="descriptionBox">
          <div class="description" data-collapsed="collapsed">{description}</div>
          <div class="expandCollapseBox">
            <input type="button" onclick="toggleExpandCollapse({resourceID}, this);" value="More" />
          </div>
          <div class="links">
            <a class="creator" href="#" onclick="searchFor('{creator}', true, 'creator');">{creator}</a>
            {standards}
          </div>
          <div class="modBox_middle"></div>
        </div><!--
     --><div class="keywords">{keywords}</div><!--
     --><div class="images">
          <a href="{resultURL}" target="ioerDtl" class="thumbnailBox resourceLink">
            {thumbnail}
          </a>
          <div class="paradata">{paradata}</div>
          <div class="created">Created {created}</div>
        </div>
      </div>
      <div class="modBox_after"></div>
    </div>
  </script>
  <script type="text/template" id="template_result_grid">
    <a class="result grid resourceLink" data-resourceID="{resourceID}" href="{resultURL}" target="ioerDtl">
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
      <a href="{resultURL}" target="ioerDtl" class="resourceLink">{title}</a>
      <div class="description">{description}</div>
      <div class="modBox_after"></div>
    </div>
  </script>
  <script type="text/template" id="template_selectedTag">
    <div class="selectedTag" data-id="{id}">{text} <input type="button" value="X" class="redX" onclick="removeTag('{id}', {isStandard})" /></div>
  </script>
</div>
