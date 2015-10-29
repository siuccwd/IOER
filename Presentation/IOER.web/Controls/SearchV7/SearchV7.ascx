<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SearchV7.ascx.cs" Inherits="IOER.Controls.SearchV7.SearchV7" %>
<%@ Register TagPrefix="uc1" TagName="Standards" Src="/Controls/StandardsBrowser7.ascx" %>
<% var serializer = new System.Web.Script.Serialization.JavaScriptSerializer(); %>
<%
	var rightsGroups = new Dictionary<string, List<int>>
	{
		{ "Attribution", new List<int>() { 364, 381 } },
		{ "Attribution, Share Alike", new List<int>() { 379, 392 } },
		{ "Attribution, No Derivatives", new List<int>() { 382, 395 } },
		{ "Attribution, Non-Commercial", new List<int>() { 365, 383 } },
		{ "Attribution, Non-Commercial, Share Alike", new List<int>() { 367, 385 } },
		{ "Attribution, Non-Commercial, No Derivatives", new List<int>() { 366, 386 } },
		{ "License Unknown", new List<int>() { 377 } },
		{ "Public Domain", new List<int>() { 387, 391 } },
		{ "Non-Creative Commons License", new List<int>() { 368 } },
	};	
%>
<div id="themeBox" class="themeBox" runat="server"></div>

<script type="text/javascript">
	/* Global Variables */
	var theme = "<%=LoadTheme %>";
	var libraryIDs = <%=serializer.Serialize(LibraryIds) %>;
	var collectionIDs = <%=serializer.Serialize(CollectionIds) %>;
	var SB7mode = "search";
	var sortMode = <%=serializer.Serialize(Config.Sort) %>;
	var pageSize = <%=Config.PageSize %>;
	var viewMode = "<%=Config.ViewMode %>";
	var usageRightsData = <%=UsageRightsIconsJSON %>;
	var keywordSchemas = <%=serializer.Serialize( Config.ResultTagSchemas ) %>;
	var advancedMode = <%=Config.StartAdvanced ? "true" : "false" %>;
	var hasStandards = <%=Config.HasStandards ? "true" : "false" %>;
	var useResourceURL = <%=Config.UseResourceUrl ? "true" : "false" %>;
	var doAutoSearch = <%=( Config.DoAutoSearch || !string.IsNullOrWhiteSpace( PreselectedText ) || PreselectedTagIDs.Count() > 0 ) ? "true" : "false" %>;
	var doPreloadNewest = <%=( Config.DoPreloadNewestSearch && string.IsNullOrWhiteSpace( PreselectedText ) && PreselectedTagIDs.Count() == 0 ) ? "true" : "false" %>;
	var allowLibColInputs = <%=Config.ShowLibColInputs ? "true" : "false" %>;
	var currentResults = <%=InitialResultsJSON %>;
	var countdown = 0;
	var cooldown = 0;
	var canDoSearch = true;
	var currentStartPage = 1;
	var currentQuery = {};
	var autoTextFilters = [ "delete", "freesound", "bookshare", "smarter balanced" ];
	var fileIcons = [
		{ match: [ ".zip", ".rar", ".tar", ".gz", ".7z" ], icon: "/images/icons/icon_zip_400x300.png" },
		{ match: [ ".avi", ".mp4", ".mpg", ".flv", ".3gp" ], icon: "/images/icons/icon_video_400x300.png" },
		{ match: [ ".ram", ".mp3", ".wma", ".ogg", ".wav" ], icon: "/images/icons/icon_audio_400x300.png"  },
		{ match: [ ".jnlp", ".tga", ".tif", ".tiff", "view.d2l" ], icon: "/images/icons/icon_download_400x300.png" }
	];
	var searchStages = {
		resetCountdown: "resetCountdown",
		resetPaginator: "resetPaginator",
		packQuery: "packQuery",
		handleResponse: "handleResponse",
		render: "render"
	};
	var windowWidth = 0;
	var widthTimer = 0;
</script>
<script type="text/javascript">
	/* Initialization */
	$(document).ready(function() {
		//Init Search Box
		setupSearchBox();
		//Setup Filter interactivity
		setupFilters();
		//Setup Standards Browser interactivity
		setupStandardsBrowser();
		//Setup drop-down lists
		setupDDLs();
		//Setup window resize actions
		setupResizing();
		//Log resource views if in widget mode
		setupViewLogging();
		//Do automatic search
		setupAutoSearch();
		//Setup Accessibility
		setupAccessibility();
		//Miscellaneous
		setupMiscellaneous();
	});

	//Setup Search box interactivity
	function setupSearchBox() {
		$("form").attr("onsubmit", "return false;");
		$("#txtSearch, #txtLibraryIDs, #txtCollectionIDs").on("keyup change", function (e) {
			var value = $("#txtSearch").val();
			if(e.which == 9 || e.keycode == 9) { return; } //Do nothing on press of tab key
			if(e.which == 13 || e.keyCode == 13) {
				e.stopPropagation();
			}
			search(searchStages.resetCountdown);
		});
	}

	//Do a search when filters change
	function setupFilters() {
		$(".tags input").on("change", function () {
			packQuery();
			search(searchStages.resetCountdown);
		});
	}

	//Setup standards browser interactivity
	function setupStandardsBrowser() {
		$(window).on("SB7search", function(event,selected,all){
			search(searchStages.resetCountdown);
		});
		$(window).on("SB7showResults", function() {
			$("#btnToggleFilters, #filters").removeClass("expanded").addClass("collapsed");
			$("#standardsBrowserBox").removeClass("expanded").addClass("collapsed").slideUp(250);
		});
		$(window).on("standard_added standard_removed", function() {
			renderSelectedTags();
		});
	}

	//Setup drop-down lists
	function setupDDLs() {
		//Sort Order
		$("#ddlSortMode").on("change", function () {
			var sortModeParts = $(this).find("option:selected").attr("value").split("|");
			sortMode = { field: sortModeParts[0], order: sortModeParts[1] };
			currentStartPage = 1;
			search(searchStages.resetCountdown);
		});
		//Number of items to show
		$("#ddlPageSize").on("change", function () {
			pageSize = parseInt($(this).find("option:selected").attr("value"));
			currentStartPage = 1;
			search(searchStages.resetCountdown);
		});
	}

	//Setup window resize events
	function setupResizing() {
		$(window).on("resize", function () {
			calculateExposedDescriptions();
		});
		triggerResize(0);
	}

	//Enable selecting tags via event
	function setupAutoSelectTags() {
		$(window).on("selectTags", function (msg, data) {
			for(i in data.tags){
				$(".tags input[data-id=" + data.tags[i] + "]").prop("checked", true).trigger("change");
			}
		});
	}

	//Setup view logging if in widget mode
	function setupViewLogging() {
		if(!useResourceURL){
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

	//Setup auto search
	function setupAutoSearch() {
		if(doPreloadNewest){
			search(searchStages.render);
		}
		if(doAutoSearch) {
			search(searchStages.resetCountdown);
		}
	}

	//Setup accessibility things
	function setupAccessibility(){
		//Enable skipping to contents with home key - temp change to the escape key (27)
		$(window).on("keyup", function(e) {
			if(e.which == 27 || e.keyCode == 27){
				$("#skipButtons").show().focus();
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

	//Setup additional features
	function setupMiscellaneous() {
		//Fix chrome focus issues
		$("input[type=button]").on("mouseout", function() {
			$(this).blur();
		});

		//Start in advanced mode if selected
		if(advancedMode){
			advancedMode = false;
			toggleAdvancedMode();
		}

		//Close filters on small screens
		$(window).on("resize", function() {
			clearTimeout(widthTimer);
			widthTimer = setTimeout(function() {
				var width = $(window).width();
				if(width != windowWidth){
					windowWidth = width;
					if(width <= 1000 && $("#filtersBox").hasClass("expanded") || width > 1000 && !$("#filtersBox").hasClass("expanded")){
						$("#btnToggleFilters").trigger("click");
					}
				}
			}, 100);
		}).trigger("resize");

		//Enable selecting tags via postmessage
		$(window).on("selectTags", function (msg, data) {
			for(i in data.tags){
				$(".tags input[data-tagID=" + data.tags[i] + "]").prop("checked",true).trigger("change");
			}
		});
	}

</script>
<script type="text/javascript">
	/* Search Engine */
	function search(stage){
		switch (stage) {
			//Reset countdown and call search when it expires
			case searchStages.resetCountdown:
				renderSelectedTags();
				resetCountdown();
				break;

				//Do a new search that requires resetting the paginators
			case searchStages.resetPaginator:
				currentQuery.PageStart = 1;

				//Prepare query and do search
			case searchStages.packQuery:
				packQuery();
				renderSelectedTags();
				doSearch();
				break;

				//Render results
			case searchStages.render:
				renderResults();
				renderPaginators();
				break;

			default: break;
		}
	}

	//Pack Query
	function packQuery() {
		selectedStandardsIDs_search = [];
		if(hasStandards){
			selectedStandardsIDs_search = allSelectedIDs;
		}
		var queryText = getQueryText();
		currentQuery = {
			text: queryText,
			fields: getSelectedTags(),
			allStandardIDs: selectedStandardsIDs_search,
			libraryIDs: allowLibColInputs ? cslToInts("#txtLibraryIDs") : libraryIDs,
			collectionIDs: allowLibColInputs ? cslToInts("#txtCollectionIDs") : collectionIDs,
			sort: sortMode,
			size: pageSize,
			start: (currentStartPage - 1) * pageSize,
			not: getActiveAutoTextFilters(queryText)
		};
	}

	//Comma-separated list to int list
	function cslToInts(jSelector){
		var text = $(jSelector).val().split(",");
		var result = [];
		for(var i in text){
			try {
				var test = parseInt(text[i].trim());
				if(!isNaN(test) && typeof(test) == "number"){
					result.push(test);
				}
			}
			catch(e){ }
		}
		return result;
	}

	//Reset countdown (before search)
	function resetCountdown() {
		clearTimeout(countdown);
		countdown = setTimeout(function () {
			search(searchStages.resetPaginator);
		}, 800);
	}

	//Reset cooldown (after search)
	function resetCooldown() {
		clearTimeout(cooldown);
		cooldown = setTimeout(function() {
			canDoSearch = true;
		}, 1000);
	}

</script>
<script type="text/javascript">
	/* AJAX */
	//Do a search
	function doSearch() {
		//Avoid spammy requests
		if(!canDoSearch){
			return;
		}
		else {
			canDoSearch = false;
			resetCooldown();
		}

		//Set status
		setStatus("Searching...", "searching");

		//Do the search
		var request = $.ajax({
			url: "/Services/ElasticSearchService.asmx/DoSearchCollection7",
			async: true,
			headers: { "Accept": "application/json", "Content-type": "application/json; charset=utf-8" },
			dataType: "json",
			type: "POST",
			data: JSON.stringify({ input: currentQuery }), //Write this
			success: function (msg) {
				var message = msg.d ? msg.d : msg;
				currentResults = $.parseJSON(message.data);
				currentResults.extra = message.extra;
				search(searchStages.render);
			}
		});
	}

</script>
<script type="text/javascript">
	/* Page Functions */
	//Set view mode
	function setViewMode(mode){
		$("#searchResults").attr("data-mode", mode);
		viewMode = mode;
		$("#viewModeButtons input").removeClass("selected").filter("[data-mode=" + mode + "]").addClass("selected");
		search(searchStages.render);
	}

	//Clear search text
	function clearText() {
		$("#txtSearch").val("").trigger("change");
	}

	//Clear search tags
	function clearTags() {
		$(".selectedTag input").trigger("click");
	}

	//Toggle collapsible stuff
	function toggleCollapse(target){
		var box = $("#" + target);
		if(box.hasClass("collapsed")){
			box.removeClass("collapsed").addClass("expanded");
			box.find("> .collapsible").slideDown(250);
		}
		else {
			box.removeClass("expanded").addClass("collapsed");
			box.find("> .collapsible").slideUp(250);
		}
	}

	//Toggle standards browser
	function toggleStandardsBrowser() {
		var button = $("#btnToggleStandardsBrowser");
		var box = $("#standardsBrowserContainer");
		if(box.hasClass("collapsed")){
			button.addClass("selected");
			box.removeClass("collapsed").addClass("expanded").slideDown(250);
		}
		else {
			button.removeClass("selected");
			box.removeClass("expanded").addClass("collapsed").slideUp(250);
		}
	}

	//Toggle advanced mode
	function toggleAdvancedMode() {
		var box = $("#searchHeader, #filtersBox");
		if(advancedMode){
			//Shift to basic mode
			advancedMode = false;
			box.removeClass("advanced").addClass("basic");
			$("#btnToggleAdvancedMode").attr("value", "Advanced Search").removeClass("selected");
			$("#advancedBox").slideUp(250);
		}
		else {
			//Shift to advanced mode
			advancedMode = true;
			box.removeClass("basic").addClass("advanced");
			$("#btnToggleAdvancedMode").attr("value", "Basic Search").addClass("selected");
			$("#advancedBox").slideDown(250);
		}
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

		search(searchStages.resetCountdown);
	}

	//Jump to page
	function jumpToPage(page){
		currentStartPage = page;
		$("html, body").animate({ scrollTop: 0 }, 500);
      
		search(searchStages.packQuery);
	}

	//Remove selected tag
	function removeTag(id, type){
		switch(type){
			case "tag":
				$(".tags input[type=checkbox][data-tagID='" + id + "']").prop("checked", false);
				break;
			case "standard":
				$(window).trigger("SB7removeStandard", id);
				break;
			case "text":
				$("#" + id).val("");
				break;
			default: break;
		}

		renderSelectedTags();
		search(searchStages.resetCountdown);
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
	/* Rendering */
	//Render search results
	function renderResults() {
		if(typeof(currentResults.hits) == "undefined"){
			return;
		}

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
				var link = useResourceURL ? current.Url : "/Resource/" + current.ResourceId + "/" + ( current.UrlTitle == null ? "" : current.UrlTitle).replace(/&/g, "" ) + getLibColIDs();
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
			box.hide().fadeIn(500);
		}
	}
	function getThumbnail(current){
		if(typeof(current.ThumbnailUrl) != "undefined" && current.ThumbnailUrl != "") {
			return current.ThumbnailUrl;
		}
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
				result += "<span class=\"moreStandards\">(+" + (current.StandardNotations.length - count) + " additional standards)</span>"
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
			return "<img class='rightsIcon' title='Usage Rights Unknown' alt='Usage Rights Unknown' src='/images/icons/license_unknown_mini.png' />";
		}
		for(var i in usageRightsData){
			if(current.UsageRightsUrl == usageRightsData[i].Url){
				return "<a class='rightsIcon' title='" + current.Title + "' href='" + current.UsageRightsUrl + "' target='_blank'><img alt='" + current.Title + "' src='" + usageRightsData[i].MiniIconUrl + "' /></a>";
			}
		}
		return "<a class='rightsIcon' title='Custom Usage Rights - Click for Details' href='" + current.UsageRightsUrl + "' target='_blank'><img alt='Custom Usage Rights - Click for Details' src='/images/icons/license_custom_mini.png' /></a>";
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
				boxes.append("<input type=\"button\" value=\"" + page + "\" class=\"isleButton bgMain " + ((currentStartPage == page) ? "current" : "") + "\" title=\"" + ((currentStartPage == page) ? "Currently on page " : "Jump to results page ") + page + "\" onclick=\"jumpToPage(" + i + ");\" />");
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
		//Tags
		$(".tags input:checked").each(function() {
			var tag = $(this);
			box.append(template.replace(/{id}/g, tag.attr("data-tagID")).replace(/{text}/g, tag.parent().text()).replace(/{type}/g, "tag"));
		});
		//Standards
		if(hasStandards){
			for(var i in selectedStandards){
				var standard = selectedStandards[i];
				var code = standard.code == null || standard.code == "" ? standard.description.substring(0,50) + "..." : standard.code
				box.append(template.replace(/{id}/g, standard.id).replace(/{text}/g, code).replace(/{type}/g, "standard"));
			}
		}
		//Advanced
		if(allowLibColInputs){
			var libs = $("#txtLibraryIDs").val().trim();
			if(libs.length > 0){
				box.append(template.replace(/{id}/g, "txtLibraryIDs").replace(/{text}/g, "Library IDs: " + libs).replace(/{type}/g, "text"));
			}
			var cols = $("#txtCollectionIDs").val().trim();
			if(cols.length > 0){
				box.append(template.replace(/{id}/g, "txtCollectionIDs").replace(/{text}/g, "Collection IDs: " + cols).replace(/{type}/g, "text"));
			}
		}
		triggerResize(100);

		if(box.find(".selectedTag").length > 0){
			$("#selectedTagsBox").fadeIn(250);
		}
		else {
			$("#selectedTagsBox").fadeOut(250);
		}
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
		$(".filter").each(function () {
			var filter = { Id: parseInt($(this).attr("data-filterID")), Ids: [] };
			$(this).find("input:checked").each(function () {
				//Altered to allow for multiple IDs for usage rights groups
				var data = $(this).attr("data-tagID").split(",");
				for(var i in data){
					filter.Ids.push(parseInt(data[i]));
				}
				//Single IDs
				//filter.Ids.push(parseInt($(this).attr("data-tagID")));
			});
			if (filter.Ids.length > 0) {
				result.push(filter);
			}
		});
		return result;
	}

	//Accessibility skip functions
	function skipToSearchBar() { $("#txtSearch").focus(); }
	function skipToFilters() { $("#btnToggleFilters").focus(); }
	function skipToStandards() { 
		var button = $("#btnToggleStandardsBrowser");
		if(!button.hasClass("selected")){
			button.click();
		}
		$("#SBddlBody").focus(); 
	}
	function skipToStandardsSearch() {
		var button = $("#btnToggleStandardsBrowser");
		if(!button.hasClass("selected")){
			button.click();
		}
		$("#SBbtnSearch").focus();
	}
	function skipToSearchResults() { $("#status").focus(); }
	function hideSkipButtons() { $("#skipButtons").hide(); }

</script>

<style type="text/css">
	/* Big Stuff */
	#filtersBox *, #searchHeader * { transition: background-color 0.2s, color 0.2s, opacity 0.2s; }
	h1:empty { display: none; }
	#content { min-height: 1000px; }
	#columns { position: relative; }
	#leftColumn, #rightColumn { display: inline-block; vertical-align: top; }
	#leftColumn { width: 260px; }
	#rightColumn { width: calc(100% - 260px); }
	#skipButtons { margin-bottom: 10px; }

	/* Header */
	#searchHeader { position: relative; }
	#filtersBox { width: 250px; }
	#textBox { white-space: nowrap; margin-bottom: 5px; }
	#textBox * { display: inline-block; vertical-align: top; font-size: 25px; height: 35px; line-height: 32px; }
	#searchTools { padding: 0 10px; }
	.filter > input[type=button], .arrow { white-space: normal; background-position: center right 5px; background-size: 15px 15px; padding-right: 25px; background-repeat: no-repeat; background-image: url('/images/arrow-down-white.png'); }
	.filter.expanded > input[type=button], .arrow.selected { background-image: url('/images/arrow-up-white.png'); }

	/* Filters */
	#btnToggleFilters { font-size: 26px; height: 35px; transition: border 0.5s; }
	#filtersBox.expanded #btnToggleFilters { border-radius: 5px 5px 0 0; }
	.tags { background-color: rgba(255,255,255,0.9); }
	.tags label { display: block; padding: 2px 5px 2px 20px; border-radius: 5px; position: relative; }
	.tags label input[type=checkbox], .tags label input[type=radio] { position: absolute; top: 5px; left: 2px; }
	.tags label:hover, .tags label:focus { background-color: #EEE; cursor: pointer; }
	.filter .collapseButton { border-radius: 0; font-size: 18px; text-align: left; }
	.filter:last-of-type .collapseButton { border-radius: 0 0 5px 5px; }

	/* Search Box */
	#txtSearch { width: calc(100% - 200px); background-image: linear-gradient(#EEE, #FFF 50%, #FFF 85%, #EFEFEF); border-radius: 5px 0 0 5px; }
	#btnClearText { width: 25px; border-radius: 0; }
	#btnDoSearch { width: 175px; border-radius: 0 5px 5px 0; }

	#searchTools .toolBox { margin-bottom: 5px; padding: 5px; }
	#searchTools .toolBox .header { margin: -5px -5px 5px -5px; }
	#standardsBrowserContainer { position: relative; }
	#standardsBrowserContainer #SB7 { padding: 0; }
	#standardsBrowserContainer #SB7 #SBtree { max-height: 300px; }
	#standardsBrowserContainer #btnCloseStandardsBrowser, #advancedBox #btnCloseAdvancedOptions { width: auto; height: 24px; position: absolute; top: 2px; right: 2px; }

	#optionsBox { position: relative; min-height: 25px; margin-bottom: 5px; text-align: right; }
	#optionsBox > * { width: 160px; margin-left: 5px; display: inline-block; vertical-align: top; height: 25px; }
	#btnToggleStandardsBrowser { position: absolute; left: 0; top: 0; margin-left: 0; }

	#viewModeButtons input { width: 33.333332%; height: 25px; border-radius: 0; }
	#viewModeButtons input:first-child { border-radius: 5px 0 0 5px; }
	#viewModeButtons input:last-child { border-radius: 0 5px 5px 0; }

	#advancedBox { position: relative; }
	#advancedLeftColumn, #advancedRightColumn { display: inline-block; vertical-align: top; width: 50%; }
	#advancedBox input, #advancedBox select { width: 100%; display: block; margin-bottom: 5px; }
	#advancedRightColumn { padding-left: 5px; }

	#tips { padding: 5px 5px 5px 5px; background-color: #EEE; border-radius: 5px; border: 1px solid #CCC; }
	#tips p a { display: inline-block; padding: 2px 5px; border: 1px solid #CCC; border-radius: 5px; }
	#selectedTagsBox { padding: 2px 30px 2px 5px; position: relative; border-radius: 5px; border: 1px solid #CCC; }
	#selectedTagsBox .clearButton { position: absolute; top: 0; right: 0; height: 100%; border-radius: 0 5px 5px 0; border-top-width: 0; }
	#selectedTags, #btnClearTags { display: inline-block; vertical-align: top; }
	#selectedTags { width: calc(100% - 25px); }
	#btnClearTags { width: 25px; height: 25px; }
	#standardsBrowserContainer #SB7 #SBtree * { transition: none; }

	#viewModeButtons input { background-repeat: no-repeat; background-position: center center; background-size: 20px 20px; }

	@media (max-width: 1025px) {
		#leftColumn { position: absolute; top: 50px; left: 10px; z-index: 9999; padding-right: 10px; }
		#filtersBox { width: auto; }
		#rightColumn { width: 100%; }
		#optionsBox { padding-left: 260px; min-height: 55px; }
		#optionsBox > * { width: 49%; margin-left: 0; margin-bottom: 5px; }
		#optionsBox > *:nth-child(2n + 0) { margin-left: 0.5%; }
		#optionsBox > *:nth-child(2n + 1) { margin-right: 0.5%; }
		#btnToggleStandardsBrowser { position: static; }
	}
	@media (max-width: 650px) {
		#txtSearch { width: calc(100% - 125px); }
		#btnDoSearch { width: 100px; }
		#optionsBox > * { width: 100%; display: block; margin: 0 0 5px 0; }
		#optionsBox > *:nth-child(2n + 0) { margin-left: 0; }
		#optionsBox > *:nth-child(2n + 1) { margin-right: 0; }
		#txtSearch { font-size: 20px; }
	}
	@media (max-width: 500px) {
		#optionsBox { padding: 40px 0 0 0; }
		#leftColumn { top: 40px; left: 10px; width: calc(100% - 10px); }
	}

	/* Collapsible Tweaks */
	.filter > .collapsible, #standardsBrowserContainer, #tipsBox .collapsible { display: none; }

	/* Status */
	#status { margin: 5px 0; padding: 5px; text-align: center; font-size: 20px; transition: all 0.5s; border-radius: 5px; }
	#status.neutral { background-color: transparent; padding: 5px; }
	#status.searching { background-color: rgba(50,255,50,0.8); padding: 20px; border-radius: 10px; }
	#status.error { background-color: rgba(255,25,25,0.8); padding: 50px; border-radius: 25px; }
	#status:empty { background-color: transparent; padding: 0; }
	
  /* Paginators */
  .paginator { padding: 5px; text-align: center; }
  .paginator:empty { display: none; }
  .paginator input { min-width: 50px; padding: 2px 5px; border-radius: 0; margin: 1px; border: none; transition: background 0.2s; width: auto; display: inline-block; vertical-align: top; }
  .paginator input:first-child { border-radius: 5px 0 0 5px; }
  .paginator input:last-child { border-radius: 0 5px 5px 0; }
  .paginator input:only-child { border-radius: 5px; }

  /* Search Results */
	.selectedTag { margin: 2px 1px; border-radius: 5px; background-color: #EEE; padding: 2px 35px 2px 5px; height: 25px; display: inline-block; vertical-align: top; position: relative; max-width: 275px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
	.selectedTag input { display: inline-block; height: 100%; width: 25px; border-radius: 0 5px 5px 0; position: absolute; top: 0; right: 0; }

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
	@media (max-width: 850px) {
		.result.list .keywords { display: none; }
		.result.list .descriptionBox { width: calc(100% - 200px); padding-right: 10px; }
		#standardsBrowserContainer #SB7 #SBtree { overflow-x: hidden; overflow-y: auto; }
		#standardsBrowserContainer #SB7 #SBleftColumn, #standardsBrowserContainer #SB7 #SBrightColumn { width: 100%; display: block; padding-bottom: 10px; }
		
	}
  @media (max-width: 500px) {
    .result.list .descriptionBox { width: 70%; }
    .result.list .images { width: 30%; }
    .result.list .created { display: none; }
  }

	/* Advanced stuff */
	#searchHeader.basic .advancedOnly, #filtersBox.basic .advancedOnly { display: none; }
	#filtersBox.basic .filter.lastBasic input { border-radius: 0 0 5px 5px; }
</style>
<style type="text/css">
	.isleButton.bgMain { background-color: rgba(<%=Config.ThemeColorMain.R %>,<%=Config.ThemeColorMain.G %>,<%=Config.ThemeColorMain.B %>,1); }
	.isleButton.bgMain:hover, .isleButton.bgMain:focus { background-color: #FF6A00; }
	.isleButton.bgMain.selected { background-color: rgba(<%=Config.ThemeColorSelected.R %>,<%=Config.ThemeColorSelected.G %>,<%=Config.ThemeColorSelected.B %>,1); }
	.collapseBox.expanded > .collapseButton, .paginator input.current { background-color: rgba(<%=Config.ThemeColorSelected.R %>,<%=Config.ThemeColorSelected.G %>,<%=Config.ThemeColorSelected.B %>,1); }
	.grayBox .header { background-color: rgba(<%=Config.ThemeColorHeader.R %>,<%=Config.ThemeColorHeader.G %>,<%=Config.ThemeColorHeader.B %>,1); }
	.result.list .expandCollapseBox input { background-color: transparent; color: rgba(<%=Config.ThemeColorMain.R %>,<%=Config.ThemeColorMain.G %>,<%=Config.ThemeColorMain.B %>,1); border: none; font-style: italic; }
  .theme .result.grid .paradata { background-image: linear-gradient(90deg, transparent 10%, rgba(<%=Config.ThemeColorMain.R %>,<%=Config.ThemeColorMain.G %>,<%=Config.ThemeColorMain.B %>,0.8);); }
	#standardsBrowserContainer #SB7 #SBbuttons input { background-color: rgba(<%=Config.ThemeColorMain.R %>,<%=Config.ThemeColorMain.G %>,<%=Config.ThemeColorMain.B %>,1); }
	#standardsBrowserContainer #SB7 #SBbuttons input:hover, #SB7 #SBbuttons input:focus { background-color: #FF6A00; }

</style>

<div id="content">
	<h1 class="isleH1" tabindex="0"><%=Config.SearchTitle %></h1>

	<div id="themeHeaderContent"></div>

	<!-- skipbuttons -->
	<div id="skipButtons" class="grayBox" tabindex="0" style="display: none;">
		<div id="hideSkipbuttons"><input type="button" class="isleButton bgMain" id="btnHideSkipButtons" onclick="hideSkipButtons();" value="Hide this section (Alt + H)" /></div>
		<p>Press the Escape key at any time to jump back to this menu. Use the items here to skip to major sections of this page.</p>
		<p>Any time you change the text of the search text box or change your filter selection, the search results are automatically updated.</p>
		<% if(Config.HasStandards) { %><p>If you select one or more learning standards, you will need to press the Search button within the standards browser to perform a search.</p><% } %>
		<input type="button" class="isleButton bgMain" onclick="skipToSearchBar();" value="Skip to Search text box" />
		<input type="button" class="isleButton bgMain" onclick="skipToFilters();" value="Skip to Filters" />
		<% if(Config.HasStandards){  %>
		<input type="button" class="isleButton bgMain" onclick="skipToStandards();" value="Skip to Standards Browser" />
		<input type="button" class="isleButton bgMain" onclick="skipToStandardsSearch();" value="Skip to Standards Browser Search Button" />
		<% } %>
		<input type="button" class="isleButton bgMain" onclick="skipToSearchResults();" value="Skip to Search Results" />
	</div><!--/skipButtons -->

	<div id="columns">
		<div id="leftColumn">
			<!-- Filters -->
			<div id="filtersBox" class="collapseBox expanded basic">
				<input type="button" class="isleButton bgMain collapseButton" id="btnToggleFilters" value="Filters" onclick="toggleCollapse('filtersBox');" />
				<div id="filters" class="collapsible">
					<% var lastBasicFilter = Filters.Where( m => !Config.AdvancedFieldSchemas.Contains( m.Schema ) ).LastOrDefault(); %>
					<% foreach(var filter in Filters) { %>
						<div class="filter collapseBox collapsed <%=( Config.AdvancedFieldSchemas.Contains(filter.Schema) ? "advancedOnly" : "" ) %><%=(filter == lastBasicFilter ? "lastBasic" : "") %>" id="filter_<%=filter.Schema %>" data-filterID="<%=filter.Id %>">
							<input type="button" class="isleButton bgMain collapseButton" data-filterID="<%=filter.Id %>" data-schema="<%=filter.Schema %>" value="<%=filter.Title %>" onclick="toggleCollapse('filter_<%=filter.Schema %>	');" />
							<div class="tags collapsible" data-filterID="<%=filter.Id %>" data-schema="<%=filter.Schema %>">
								<% if(filter.Schema == "usageRights"){ %>
									<% foreach(var tag in rightsGroups) { %>
										<label><input type="checkbox" data-filterID="<%=filter.Id %>" data-tagID="<%=String.Join(",", tag.Value) %>" <%=(PreselectedTagIDs.Intersect(tag.Value).Count() > 0 ? "checked=\"checked\"" : "" ) %> /> <%=tag.Key %></label>
									<% } %>
								<% } else { %>
									<% foreach(var tag in filter.Tags) { %>
										<label><input type="checkbox" data-filterID="<%=filter.Id %>" data-tagID="<%=tag.Id %>" <%=(PreselectedTagIDs.Contains(tag.Id) ? "checked=\"checked\"" : "" ) %> /> <%=tag.Title %></label>
									<% } %>
								<% } %>
							</div>
						</div>
					<% } %>
				</div>
			</div>
		</div><!--
		--><div id="rightColumn">
			<div id="searchHeader" class="basic">
				<!-- Search Text -->
				<div id="textBox">
					<input type="text" id="txtSearch" placeholder="Start typing to search..." value="<%=PreselectedText.Replace( "\"", "&quot;" ) %>" /><!--
					--><input type="button" value="X" id="btnClearText" class="isleButton bgRed clearButton" onclick="clearText();" /><!--
					--><input type="button" value="Search" id="btnDoSearch" onclick="search(searchStages.packQuery);" class="isleButton bgMain" />
				</div>
				<!-- Search Tools -->
				<div id="searchTools">
					<!-- Options -->
					<div id="optionsBox">
						<% if(Config.HasStandards) { %><input type="button" class="isleButton bgMain optionItem collapseButton arrow" id="btnToggleStandardsBrowser" onclick="toggleStandardsBrowser();" value="Learning Standards" /><% } %><!--
						--><select id="ddlSortMode" class="optionItem">
							<option value="_score|desc" selected="selected">Most Relevant</option>
							<option value="UrlTitle|asc">Alphabetical A-Z</option>
							<option value="UrlTitle|desc">Alphabetical Z-A</option>
							<option value="ResourceId|desc">Newest First</option>
							<option value="ResourceId|asc">Oldest First</option>
							<option value="Paradata.ResourceViews|desc">Most Visited</option>
							<%--<option class="advancedOnly" value="Paradata.Likes|desc">Most Total Likes</option>
							<option class="advancedOnly" value="Paradata.Rating|desc">Most Net Likes vs Dislikes</option>
							<option class="advancedOnly" value="Paradata.Rating|asc">Most Net Dislikes vs Likes</option>
							<option class="advancedOnly" value="Paradata.Dislikes|desc">Most Total Dislikes</option>
							<option class="advancedOnly" value="Paradata.EvaluationsScore|desc">Best Net Evaluation Score</option>--%>
							<option value="Paradata.Evaluations|desc">Most Total Evaluations</option>
							<option value="Paradata.Favorites|desc">In the Most Libraries</option>
							<option class="advancedOnly" value="Paradata.Comments|desc">Most Comments</option>
						</select><!--
						--><div id="viewModeButtons" class="optionItem <%=( Config.ViewMode ) %>">
							<input data-mode="list" type="button" class="isleButton bgMain selected" title="List View" onclick="setViewMode('list');" style="background-image:url('/images/icons/icon_viewmode_list.png');" /><!--
						--><input data-mode="grid" type="button" class="isleButton bgMain" title="Grid View" onclick="setViewMode('grid');" style="background-image:url('/images/icons/icon_viewmode_grid.png');" /><!--
						--><input data-mode="text" type="button" class="isleButton bgMain" title="Text View" onclick="setViewMode('text');" style="background-image:url('/images/icons/icon_viewmode_text.png');" />
						</div><!--
						--><input type="button" class="isleButton bgMain optionItem arrow" id="btnToggleAdvancedMode" onclick="toggleAdvancedMode();" value="Advanced Search" />
					</div>
					<% if(Config.HasStandards) { %>
						<div id="standardsBrowserContainer" class="toolBox grayBox collapsed" style="display:none;">
							<h2 class="header">Learning Standards</h2>
							<input id="btnCloseStandardsBrowser" type="button" class="isleButton bgRed clearButton" value="Close" onclick="toggleStandardsBrowser();" title="Close Standards Browser" />
							<uc1:Standards ID="standardsBrowser" runat="server" />
						</div>
					<% } %>
					<!-- Advanced Stuff -->
					<div id="advancedBox" style="display:none;">
						<div id="advancedOptions" class="grayBox toolBox">
							<h2 class="header">Advanced Options</h2>
							<input id="btnCloseAdvancedOptions" type="button" class="isleButton bgRed clearButton" value="Close" onclick="toggleAdvancedMode();" title="Close Standards Browser" />
							<div id="advancedLeftColumn">
								<select id="ddlPageSize" class="optionItem">
									<option value="20" <%=( Config.PageSize == 20 ? "selected=\"selected\"" : "" ) %>>Show 20 Items</option>
									<option value="50" <%=( Config.PageSize == 50 ? "selected=\"selected\"" : "" ) %>>Show 50 Items</option>
									<option value="100" <%=( Config.PageSize == 100 ? "selected=\"selected\"" : "" ) %>>Show 100 Items</option>
								</select>
								<% if(Config.ShowLibColInputs) { %>
								<input type="text" id="txtLibraryIDs" placeholder="Library IDs (comma-separated)" value="<%=String.Join<int>( ",", LibraryIds ) %>" />
								<input type="text" id="txtCollectionIDs" placeholder="Collection IDs (comma-separated)" value="<%=String.Join<int>( ",", CollectionIds ) %>" />
								<% } %>
							</div><!--
							--><div id="advancedRightColumn">
							
							</div>
							<div id="tipsBox" class="collapseBox collapsed">
								<input type="button" class="isleButton bgMain collapseButton" id="btnToggleTips" value="Search Tips" onclick="toggleCollapse('tipsBox');" />
								<div id="tips" class="collapsible">
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
									</ul>
									<p>You can wrap phrases in double quotes ( " ) to search for whole phrases:</p>
									<ul>
										<li><a href="#" onclick="searchFor(this.innerHTML, false)">"3rd grade" literacy</a></li>
										<li><a href="#" onclick="searchFor(this.innerHTML, false)">"pythagorean theorem"</a></li>
									</ul>
									<p>You can use + or - to require/exclude words or phrases:</p>
									<ul>
										<li><a href="#" onclick="searchFor(this.innerHTML, false)">+trees -forest</a> <span>(Finds Resources about trees, avoiding Resources about forests)</span></li>
										<li><a href="#" onclick="searchFor(this.innerHTML, false)">formula -chemistry</a> <span>(Finds Non-Chemistry Resources about Formulas)</span></li>
										<li><a href="#" onclick="searchFor(this.innerHTML, false)">formula +chemistry</a> <span>(Finds Only Chemistry Resources about Formulas)</span></li>
									</ul>
									<p>You can use | to look for one term/phrase or another:</p>
									<ul>
										<li><a href="#" onclick="searchFor(this.innerHTML, false)">trees forest|math</a></li>
										<li><a href="#" onclick="searchFor(this.innerHTML, false)">"civil rights"|"civil engineering"</a> <span>(Finds Civil Rights or Civil Engineering Resources)</span></li>
									</ul>
									<p>Put them together. Go nuts!</p>
									<ul>
										<li><a href="#" onclick="searchFor(this.innerHTML, false)">"grade 3" geometry -triangles</a> <span>(Finds Grade 3 Geometry Resources about things other than Triangles)</span></li>
										<li><a href="#" onclick="searchFor(this.innerHTML, false)">"charles dickens" +"christmas carol" film|video</a> <span>(Finds film or video Resources about Charles Dickens' <u>A Christmas Carol</u>)</span></li>
										<li><a href="#" onclick="searchFor(this.innerHTML, false)">+free math "grade 3"|"third grade"|"3rd grade" game|activity -test -quiz application/software student</a> <span>(Finds Free Math Resources for Third Graders that are Games or Activities but not Tests or Quizzes, and is software meant for students.)</span></li>
									</ul>
								</div>
							</div>
						</div>
					</div>
					<!-- Selected Tags -->
					<div id="selectedTagsBox" class="toolBox" style="display:none;">
						<div id="selectedTags"></div><!--
						--><input type="button" id="btnClearTags" onclick="clearTags();" value="X" class="isleButton bgRed clearButton" />
					</div>
				</div>
			</div><!--/searchHeader-->

			<!-- Results -->
			<div id="resultsBox">
				<div id="status" tabindex="0"></div>
				<div id="paginatorTop" class="paginator" style="display:none;"></div>
				<div id="searchResults" data-mode="list"></div>
				<div id="paginatorBottom" class="paginator"></div>
			</div>

		</div><!--/rightColumn -->
	</div><!--/columns-->
</div><!--/content-->

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
    <div class="selectedTag" data-id="{id}">{text} <input type="button" value="X" class="isleButton bgRed clearButton" onclick="removeTag('{id}', '{type}')" /></div>
  </script>
</div>
