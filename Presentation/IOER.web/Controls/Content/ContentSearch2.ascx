<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ContentSearch2.ascx.cs" Inherits="IOER.Controls.Content.ContentSearch2" %>
<%@ Register TagPrefix="uc1" TagName="StandardsBrowser" Src="/controls/StandardsBrowser7.ascx" %>

<link rel="stylesheet" href="/styles/common2.css" type="text/css" />

<script type="text/javascript">
	/* Global Variables */
	var currentQuery = {
		Text: "",
		Filters: [],
		StandardIds: [],
		PageStart: 1,
		PageSize: 25,
		SortOrder: "",
		SortReversed: false
	};
	var currentResults = {};
	var currentTotalResults = 0;
	var countdown = 0;

</script>
<script type="text/javascript">
	/* Initialization */
	$(document).ready(function () {
		//Setup
		setupTextSearch();
		setupFilterSearch();
		setupDDLSearch();
		setupClickCollapsing();
		setupStandardsSearch();

		setDefaultSearchParms();

		//Auto search
		search(searchStages.resetCountdown);
	});

	//Setup text box change causing search
	function setupTextSearch() {
		$("#txtSearch").on("keyup change", function () {
			search(searchStages.resetCountdown);
		});
	}

	//Setup filter changes causing search
	function setupFilterSearch() {
		$(".filter input").on("change", function () {
			search(searchStages.resetCountdown);
		});
	}

	//Setup DDL changes causing search
	function setupDDLSearch() {
		$("#searchHeader select").on("change", function () {
			search(searchStages.resetCountdown);
		});
	}

	//Collapse filters and standards if the user clicks outside them
	function setupClickCollapsing() {
		$("#filters, #btnToggleFilters, #standards, #btnToggleStandards").on("click", function(e) {
			e.stopPropagation();
		});
		$("html").not("#filters, #btnToggleFilters").on("click", function() {
			toggleBox("filters", $("#btnToggleFilters")[0], "collapse");
		});
		$("html").not("#standards, #btnToggleStandards").on("click", function() {
			toggleBox("standards", $("#btnToggleStandards")[0], "collapse");
		});
	}

	//Add event handlers for when the user does a standards search
	function setupStandardsSearch() {
		$(window).on("SB7search", function() {
			search(searchStages.resetPaginator);
		});
	}

	//Set default parms for initial blind search
	function setDefaultSearchParms() {
        <% if (IsMyAuthoredView == false) { %>
	        $( "#cbxLearningList" ).prop( "checked", true );
	        $( "#cbxContent" ).prop( "checked", true );
        <% } else { %>
	        $( "#cbxContentAll" ).prop( "checked", true );
        <% }  %>
	}
</script>
<script type="text/javascript">
	/* Search Engine */
	//Pseudo-Enum
	var searchStages = {
		resetCountdown: "resetCountdown",
		resetPaginator: "resetPaginator",
		packQuery: "packQuery",
		handleResponse: "handleResponse",
		render: "render"
	};
	function search(stage) {
		switch (stage) {
			//Reset countdown and call search when it expires
			case searchStages.resetCountdown:
				//renderQuery(); //Not very useful right now
				resetCountdown();
				break;

			//Do a new search that requires resetting the paginators
			case searchStages.resetPaginator:
				currentQuery.PageStart = 1;

			//Prepare query and do search
			case searchStages.packQuery:
				packQuery();
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

	//Reset countdown
	function resetCountdown() {
		clearTimeout(countdown);
		countdown = setTimeout(function () {
			search(searchStages.resetPaginator);
		}, 800);
	}

	//Pack query
	function packQuery() {
		//Set text
		currentQuery.Text = $("#txtSearch").val().trim().replace(/>/g, "").replace(/</g, "");

		//Set filters
		currentQuery.Filters = [];
		$(".filter").each(function () {
			var box = $(this);
			var category = box.attr("data-filterID");
			var tags = [];
			box.find("input:checked").each(function () {
				tags.push($(this).attr("value"));
			});
			currentQuery.Filters.push({ Category: category, Tags: tags });
		});

		//Set standards
		currentQuery.StandardIds = allSelectedIDs;

		//Set basic items
		currentQuery.PageSize = parseInt($("#ddlPageSize option:selected").attr("value"));
		var sort = $("#ddlSortOrder option:selected");
		currentQuery.SortOrder = sort.attr("value");
		currentQuery.SortReversed = sort.attr("data-reversed") == "true";
		//PageStart is not to be set here

	}

	//Do a search
	function doSearch() {
		setStatus("searching", "Searching...");
		doAjax("DoSearchJSON", { query: currentQuery }, success_doSearch, null);
	}

</script>
<script type="text/javascript">
	/* Page Functions */
	//Set the status class and message
	function setStatus(css, message) {
		$("#status").attr("class", css).html(message);
	}

	//Switch page
	function jumpToPage(page){
		currentQuery.PageStart = page;
		search(searchStages.packQuery);
	}

	//Toggle filters or standards' visibility
	function toggleBox(target, button, command) {
		var box = $("#" + target);
		var jButton = $(button);

		//Collapse the other one(s)
		$(".toggleBox").not(box).attr("data-mode", "collapsed").fadeOut(250);
		$(".btnToggleBox").not(jButton).attr("data-mode", "collapsed");

		if(command == "auto") { 
			command = box.attr("data-mode") == "collapsed" ? "expand" : "collapse"; 
		}
		if(command == "expand"){
			//expand
			box.attr("data-mode", "expanded").fadeIn(250);
			jButton.attr("data-mode", "expanded");
		}
		else {
			//collapse
			box.attr("data-mode", "collapsed").fadeOut(250);
			jButton.attr("data-mode", "collapsed");
		}
	}

	//Reset search
	function resetSearch() {
		//Text
		$("#txtSearch").val("");

		//Filters
		$(".filter").each(function() {
			$(this).find("input").prop("checked", false);
			$(this).find("input[type=radio]").first().prop("checked", true);
		});

		//Standards
		selectedStandards = [];
		allSelectedStandards = [];
		allSelectedIDs = [];
		renderSelectedStandards();

		search(searchStages.packQuery);
	}
</script>
<script type="text/javascript">
	/* AJAX Functions */
	//Do AJAX
	function doAjax(method, data, success, extra) {
		$.ajax({
			url: "/services/ContentSearch2Service.asmx/" + method,
			headers: { "Accept": "application/json", "Content-type": "application/json; charset=utf-8" },
			data: JSON.stringify(data),
			dataType: "json",
			type: "POST",
			success: function (msg) {
				var message = msg.d ? msg.d : msg;
				var data = $.parseJSON(message);
				console.log("Returned Data", data);
				success(data, extra);
			}
		});
	}

	//Handle response from a search
	function success_doSearch(data) {
		if (data.valid) {
			currentResults = data.data;
			if (data.extra.TotalResults > 0) {
				currentTotalResults = data.extra.TotalResults;
				setStatus("found", "Found " + data.extra.TotalResults + " Results");
			}
			else {
				currentTotalResults = 0;
				setStatus("noresults", "Sorry, no results were found. Please try modifying your search.");
                <% if (IsMyAuthoredView == true) { %>
			    toggleBox('filters', btnToggleFilters, 'auto');
                    <% } %>
			}
			if(data.extra.Message != null && data.extra.Message != ""){
				setStatus("noresults", data.extra.Message); //Notify the user of a problem
			}
		}
		else {
			currentTotalResults = 0;
			currentResults = [];
			setStatus("error", data.status); //User-friendly
			console.log(data.extra.Error); //Technical/debug
		}
		search(searchStages.render);
	}
</script>
<script type="text/javascript">
	/* Rendering Functions */
	//Render results
	function renderResults() {
		var template = $("#template_result").html();
		var box = $("#results");
		box.html("").hide();
		for (var i in currentResults) {
			var res = currentResults[i];
			box.append(template
				.replace(/{Id}/g, res.Id)
				.replace(/{Url}/g, res.Url)
				.replace(/{Title}/g, res.Title)
				.replace(/{Editable}/g, res.Editable)
                .replace(/{EditUrl}/g, res.EditUrl)
				.replace(/{Guid}/g, res.Guid)
				.replace(/{Description}/g, res.Description)
				.replace(/{Status}/g, res.Status)
				.replace(/{Author}/g, res.Author)
				.replace(/{OrganizationTitle}/g, res.OrganizationTitle)
				.replace(/{Updated}/g, res.Updated)
				.replace(/{Type}/g, res.Type)
				.replace(/{ImageUrl}/g, res.ImageUrl || "/images/icons/icon_upload_400x300.png")
				.replace(/{ShowingPartners}/g, res.Partners.length > 0 ? "true" : "false")
				.replace(/{Partners}/g, res.Partners.join(", "))
			);
		}
		box.fadeIn();
	}

	//Render paginators
	function renderPaginators() {
		var boxes = $(".paginator");
		var template = $("#template_paginatorButton").html();
		if (currentTotalResults == 0) {
			boxes.html("");
			return;
		}

		boxes.html("Page: ");

		var totalPages = Math.ceil(currentTotalResults / currentQuery.PageSize);
		var skips = [1, 5, 10, 25, 50, 500, 1000, 2500, 5000, 10000, totalPages];
		for (var i = 1; i <= totalPages; i++) {
			var page = i;
			if (page >= (currentQuery.PageStart - 1) && page <= (currentQuery.PageStart + 1) || skips.indexOf(i) > -1 || i == totalPages) {
				boxes.append(template.replace(/{page}/g, page).replace(/{current}/g, currentQuery.PageStart == page ? "current" : ""));
			}
		}
	}

	//Render the current query's tags
	function renderQuery() {
		var box = $("#currentQuery");
		var queryTemplate = $("#template_queryItem").html();
		var standardTemplate = $("#template_standardQueryItem").html();
		box.html("");
		//Filters
		$(".filter").each(function () {
			var filter = $(this);
			var category = filter.attr("data-filterID");
			filter.find("input:checked").each(function () {
				var tag = $(this);
				box.append(queryTemplate
					.replace(/{category}/g, category)
					.replace(/{id}/g, tag.attr("value"))
					.replace(/{text}/g, tag.parent().text())
				);
			});
		});
		//Standards
		for(var i in selectedStandards){
			box.append(standardTemplate
				.replace(/{id}/g, selectedStandards[i].Id)
				.replace(/{text}/g, selectedStandards[i].description)
			);
		}
	}
</script>

<style type="text/css">
	/* Major items */
	#content { min-height: 700px; position: relative; }

	/* Filters */
	.toggleBox { padding: 5px; position: absolute; left: 0; top: 69px; width: 300px; z-index: 1000; background-color: rgba(240,240,240,0.95); display: none; margin-bottom: 100px; }
	.toggleBox h2.header { margin: -5px -5px -5px -5px; font-size: 18px; }
	.toggleBox h2.mid { margin: 5px -5px 5px -5px; }
	#standards { width: 100%; }
	#standards h2.header { margin-bottom: 5px; }
	.filter { margin-bottom: 10px; }
	.filter label { display: block; padding: 2px 5px; border-radius: 5px; transition: background-color 0.2s; }
	.filter label:hover, .filter label:focus { background-color: #DDD; cursor: pointer; }
	#btnToggleFilters, #btnToggleStandards { background-image: url('/images/arrow-right-white.png'); background-position: right 5px center; background-repeat: no-repeat; background-size: auto 60%; }
	#btnToggleFilters[data-mode=expanded], #btnToggleStandards[data-mode=expanded] { background-image: url('/images/arrow-down-white.png'); background-color: #9984BD; }

	/* Search Header */
	#searchHeader { position: relative; padding-left: 200px; }
	#headerItems { width: 100%; }
	#txtSearch { width: 100%; border-radius: 0 5px 0 0; padding: 0 60px 0 5px; height: 35px; font-size: 26px; background-image: linear-gradient(#EEE, #FFF 50%, #FFF 85%, #EFEFEF); }
	#btnClear { width: 50px; height: 35px; line-height:30px; font-size: 30px; border-radius: 0 5px 0 0; background-color: #D55; color: #FFF; font-weight: bold; text-align: center; border-width: 1px; position: absolute; top: 0; right: 0; transition: background-color 0.2s; }
	#btnClear:hover, #btnClear:focus { background-color: #F00; cursor: pointer; }
	#ddlPageSize , #ddlSortOrder { width: 50%; display: inline-block; vertical-align: top; height: 35px; margin-top: -1px; cursor: pointer; }
	#ddlPageSize { border-radius: 0; }
	#ddlSortOrder { border-radius: 0 0 5px 0; }
	#toggleButtons, #headerItems { display: inline-block; vertical-align: top; }
	#toggleButtons { width: 200px; position: absolute; top: 0; left: 0; }
	#toggleButtons input { height: 35px; font-size: 30px; line-height: 30px; border-width: 1px; }
	#toggleButtons input:first-child { border-radius: 5px 0 0 0; }
	#toggleButtons input:last-child { border-radius: 0 0 0 5px; margin-top: -1px; }
	
	/* Status */
	#status { text-align: center; padding: 5px; transition: background-color 0.3s, padding 0.3s, color 0.3s, margin 0.3s, border 0.5s; border-radius: 5px; }
	#status.searching { background-color: rgba(0,255,0,0.2); padding: 10px 5px; margin: 2px; border-radius: 15px; }
	#status.found { }
	#status.noresults { background-color: rgba(255,255,0,0.2); padding: 10px 5px; margin: 5px 2px; }
	#status.error { background-color: rgba(255,0,0,0.2); padding: 20px 5px; margin: 10px 2px; }

	/* Search Results */
	.result { margin-bottom: 10px; position: relative; background-image: linear-gradient(rgba(150,150,150,0.1),rgba(255,255,255,0)); border-radius: 5px; padding: 5px; }
	.result .editLink[data-showing=false], .result .partners[data-showing=false] { display: none; }
	.result .title { font-size: 20px; font-weight: bold; display: block; padding: 0 175px 0 0; min-height: 1.5em; }
	.result .editLink { font-size: 18px; font-style: italic; padding: 2px 30px 2px 2px; display: inline-block; position: absolute; top: 5px; right: 5px; background: url('/images/icons/Edit-Document-icon24.png') no-repeat right center; }
	.result .description, .result .metadata, .result .thumbnail, .result .textData { display: inline-block; vertical-align: top; margin: 0; }
	.result .textData { width: calc(100% - 150px); }
	.result .description { width: 65%; padding: 0 10px 0 0; }
	.result .metadata { width: 35%; padding: 5px; }
	.result .thumbnail { width: 150px; background-repeat: no-repeat; background-size: contain; background-position: center center; border: 1px solid #CCC; overflow: hidden; background-color: #EEE; border-radius: 5px; }
	.result .thumbnail a { border: none; }
	.result .thumbnail img { width: 100%; }
	.result .partners { text-align: right; font-style: italic; padding: 0 5px; opacity: 0.5; }

	/* Paginators */
	.paginator { margin: 5px 0; padding: 5px; background-color: #EEE; border-radius: 5px; text-align: center; line-height: 23px; }
	#paginatorTop { margin-top: 0; }
	.paginator:empty { display: none; }
	.paginator input { display: inline-block; vertical-align: top; width: auto; min-width: 50px; padding: 2px 5px; border-width: 0; margin: 1px; border-radius: 0; }
	.paginator input:first-child { border-radius: 5px 0 0 5px; }
	.paginator input:last-child { border-radius: 0 5px 5px 0; }
	.paginator input.current { background-color: #9984BD; }

	/* Query tags */
	.queryItem { display: inline-block; margin: 2px 5px; border-radius: 5px; padding: 2px 5px; position: relative; background-color: #EEE; max-width: 200px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }

	/* Responsive */
	@media (max-width: 1100px) {
		.result .description, .result .metadata { display: block; width: 100%; }
		.result .metadata div { display: inline-block; vertical-align: top; width: 50%; padding: 2px 5px; }
	}
	@media (max-width: 850px) {
		.result .metadata div { display: block; width: 100%; padding: 2px; }
	}
	@media (max-width: 600px) {
		.paginator input { min-width: 30px; }
		#searchHeader { padding-left: 0; }
		#headerItems #txtSearch { border-radius:5px 5px 0 0; }
		#headerItems #ddlSortOrder { border-radius: 0; }
		#toggleButtons { position: static; width: 100%; }
		#txtSearch { font-size: 20px; }
		#toggleButtons input { width: 50%; display: inline-block; vertical-align: top; font-size: 20px; }
		#toggleButtons input:first-child { border-radius: 0 0 0 5px; }
		#toggleButtons input:last-child { border-radius: 0 0 5px 0; margin-top: 0; }
		.result .textData { width: 75%; }
		.result .thumbnail { width: 25%; }
		.toggleBox { top: 104px; }
	}
</style>

<!-- Page Content -->
<div id="content">

	<h1 class="isleH1">IOER Content Search</h1>

		<div id="searchHeader">
			<div id="headerItems"><!--
				--><input type="text" id="txtSearch" placeholder="Start typing here to search..." /><!--
				--><input type="button" value="X" id="btnClear" onclick="resetSearch()" title="Reset Search" /><!--
				--><select id="ddlPageSize">
					<option value="25">25 items per page</option>
					<option value="50">50 items per page</option>
					<option value="100">100 items per page</option>
				</select><!--
				--><select id="ddlSortOrder">
					<option value="title" data-reversed="false">Title A-Z</option>
					<option value="SortName" data-reversed="false">Author A-Z</option>
					<option value="base.Organization" data-reversed="false">Organization A-Z</option>
					<option value="base.LastUpdated" data-reversed="true" selected="selected">Most Recently Updated</option>
					<option value="title" data-reversed="true">Title Z-A</option>
					<option value="SortName" data-reversed="true">Author Z-A</option>
					<option value="base.Organization" data-reversed="true">Organization Z-A</option>
					<option value="base.LastUpdated" data-reversed="false">Least Recently Updated</option>
				</select>
			</div><!--
			--><div id="toggleButtons">
				<input type="button" id="btnToggleFilters" onclick="toggleBox('filters', this, 'auto');" value="Filters" class="btnToggleBox isleButton bgBlue" data-mode="collapsed" /><!--
				--><input type="button" id="btnToggleStandards" onclick="toggleBox('standards', this, 'auto');" value="Standards" class="btnToggleBox isleButton bgBlue" data-mode="collapsed" />
			</div>

			<div id="filters" data-mode="collapsed" class="grayBox toggleBox">
				<h2 class="header">Filter by Tags</h2>

				<div class="filter" data-filterID="UpdatedRange">
					<h2 class="mid">Last Updated</h2>
					<label><input type="radio" name="UpdatedRange" value="4" checked="checked" /> Any Time</label>
					<label><input type="radio" name="UpdatedRange" value="1" /> Last 7 Days</label>
					<label><input type="radio" name="UpdatedRange" value="2" /> Last 30 Days</label>
					<label><input type="radio" name="UpdatedRange" value="3" /> Last 6 Months</label>
				</div>

				<% if(IsUserAuthenticated()) { %>
				<div class="filter" data-filterID="Creator">
					<h2 class="mid">Ownership</h2>
					<% if (IsMyAuthoredView == true) { %>
					<label><input type="checkbox" name="Creator" value="1" checked="checked" /> Me</label>
                    <% } else { %>
					<label><input type="checkbox" name="Creator" value="1" /> Me</label>
                    <% }  %>
					<label><input type="checkbox" name="Creator" value="2" /> My Organization</label>
					<label><input type="checkbox" name="Creator" value="3" /> Shared With Me</label>
					<label><input type="checkbox" name="Creator" value="4" /> Anyone</label>
				</div>
				<% } %>

				<div class="filter" data-filterID="ContentType">
					<h2 class="mid">Content Type</h2>
					<label><input type="checkbox" name="ContentType" value="0" id="cbxContentAll" /> All</label>
					<label><input type="checkbox" name="ContentType" value="50" id="cbxLearningList" /> Learning List</label>
					<label><input type="checkbox" name="ContentType" value="52" /> Learning List Level</label>
					<label><input type="checkbox" name="ContentType" value="40" /> Attachment (Document)</label>
					<label><input type="checkbox" name="ContentType" value="42" /> Attachment (URL)</label>
					<label><input type="checkbox" name="ContentType" value="10" id="cbxContent" /> Content</label>
				</div>

				<%--
				<div class="filter" data-filterID="Privilege">
					<h2 class="mid">Access Privilege</h2>
					<label><input type="checkbox" name="Privilege" value="1" /> Anyone can access, including students</label>
					<label><input type="checkbox" name="Privilege" value="2" /> Only staff of an Isle Approved Organization</label>
					<label><input type="checkbox" name="Privilege" value="3" /> Only education staff at my school</label>
					<label><input type="checkbox" name="Privilege" value="4" /> Only education staff AND students at my school</label>
				</div>

                <% if ( IsUserAuthenticated() )
                   { %>
				<div class="filter" data-filterID="Status">
					<h2 class="mid">Publish Status</h2>
					<label><input type="checkbox" name="Status" value="2" /> In Progress</label>
					<label><input type="checkbox" name="Status" value="5" /> Published</label>
                    <label><input type="checkbox" name="Status" value="8" /> Inactive</label>
				</div>
			<% } %>

	--%>
			</div><!-- /filters -->
		
			<div id="standards" class="grayBox toggleBox">
				<h2 class="header">Filter by Standards</h2>
				<div id="standardsBox">
					<script type="text/javascript">
						var SB7mode = "search";
					</script>
					<uc1:StandardsBrowser ID="standardsBrowser" runat="server" />
				</div>
		</div>

		</div><!-- /searchHeader -->

		<div id="currentQuery"></div>

		<div id="status">Ready</div>

		<div id="paginatorTop" class="paginator"></div>

		<div id="results"></div>

		<div id="paginatorBottom" class="paginator"></div>

</div><!-- /Page Content -->

<div id="templates" style="display:none;">
	
	<script type="text/template" id="template_result">
		<div class="result" id="{Id}">
			<a class="title" href="{Url}" target="ioerDtl">{Title}</a>
			<a class="editLink" data-showing="{Editable}" href="{EditUrl}" target="editCtnt">Edit This Content</a>
			<div class="data">
				<div class="textData">
					<p class="description">{Description}</p><!--
					--><div class="metadata">
						<div><b>Status:</b> {Status}</div><!--
						--><div><b>Author:</b> {Author}</div><!--
						--><div><b>Organization:</b> {OrganizationTitle}</div><!--
						--><div><b>Last Updated:</b> {Updated}</div><!--
						--><div><b>Type:</b> {Type}</div>
					</div>
				</div><!--
				--><div class="thumbnail" style="background-image:url('{ImageUrl}');">
					<a href="{Url}"  target="ioerDtl"><img alt="" src="/images/ThumbnailResizer.png" /></a>
				</div>
			</div>
			<div class="partners" data-showing="{ShowingPartners}">Partners: {Partners}</div>
		</div>
	</script>

	<script type="text/template" id="template_paginatorButton"><!--
		--><input type="button" class="isleButton bgBlue paginatorButton {current}" page="{page}" value="{page}" onclick="jumpToPage({page});" /><!--
	--></script>

	<script type="text/template" id="template_queryItem">
		<div class="queryItem" data-category="{category}" data-id="{id}">{text}</div>
	</script>

	<script type="text/template" id="template_standardQueryItem">
		<div class="queryItem" data-category="standard" data-id="{id}">{text}</div>
	</script>

</div>

<asp:Panel ID="hiddenStuff" runat="server" Visible="false">

<asp:Literal ID="txtCustomFilter" runat="server" Visible="false"></asp:Literal>

<asp:Literal ID="txtAuthorSecurityName" runat="server" Visible="false"></asp:Literal>

<asp:Literal ID="txtLibraryId" runat="server" Visible="false">0</asp:Literal>

<asp:Literal ID="formattedTitleTemplate" runat="server" Visible="false"><a style="color:#000;  cursor:pointer;" href="{0}" target="_blank" title="Website link opens in a new window">{1}</a></asp:Literal>

</asp:Panel>