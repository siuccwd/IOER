<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="gooru.ascx.cs" Inherits="IOER.Controls.SearchV7.Themes.gooru" %>

<div id="config" runat="server" visible="false">
	<asp:Literal ID="searchTitle" runat="server">IOER <a href="http://www.goorulearning.org/" target="_blank">gooru</a> Search</asp:Literal>
	<asp:Literal ID="themeColorMain" runat="server">#3572B8</asp:Literal>
	<asp:Literal ID="themeColorSelected" runat="server">#9984BD</asp:Literal>
	<asp:Literal ID="themeColorHeader" runat="server">#4AA394</asp:Literal>
	<asp:Literal ID="sortField" runat="server">_score</asp:Literal>
	<asp:Literal ID="sortOrder" runat="server">desc</asp:Literal>
	<asp:Literal ID="resultTagSchemas" runat="server"></asp:Literal>
	<asp:Literal ID="siteID" runat="server">1</asp:Literal>
	<asp:Literal ID="startAdvanced" runat="server">0</asp:Literal>
	<asp:Literal ID="hasStandards" runat="server">1</asp:Literal>
	<asp:Literal ID="useResourceUrl" runat="server">0</asp:Literal>
	<asp:Literal ID="doAutoSearch" runat="server">1</asp:Literal>
	<asp:Literal ID="doPreloadNewestSearch" runat="server">0</asp:Literal>
	<asp:Literal ID="showLibColInputs" runat="server">0</asp:Literal>
	<asp:Literal ID="fieldSchemas" runat="server"></asp:Literal>
	<asp:Literal ID="advancedFieldSchemas" runat="server"></asp:Literal>
</div>

<script type="text/javascript">
	var gooruFilters = [
		{ title: "Search Type", schema: "searchType", type: "radio", tags: 
			[
				{ title: "Resources", schema: "resource" }, 
				{ title: "Collections", schema: "collection" }
			] 
		},
		{ title: "Education Level", schema: "gradeLevel", type: "checkbox", tags: 
			[
				{ title: "Kindergarten", schema: "Kindergarten,K-4" },
				{ title: "Grade 1", schema: "1" },
				{ title: "Grade 2", schema: "2" },
				{ title: "Grade 3", schema: "3" },
				{ title: "Grade 4", schema: "4" },
				{ title: "Grade 5", schema: "5" },
				{ title: "Grade 6", schema: "6" },
				{ title: "Grade 7", schema: "7" },
				{ title: "Grade 8", schema: "8" },
				{ title: "Grades 9-10", schema: "9-10" },
				{ title: "Grades 11-12", schema: "11-12" },
				{ title: "Higher Education", schema: "H" },
			]
		},
		{ title: "Subject", schema: "subject", type: "checkbox", tags:
			[
				{ title: "Mathematics", schema: "Math" },
				{ title: "English Language Arts", schema: "Language Arts" },
				{ title: "Science", schema: "Science" },
				{ title: "Social Sciences", schema: "Social Sciences" },
				{ title: "Arts & Humanities", schema: "Arts & Humanities" },
				{ title: "Technology & Engineering", schema: "Technology & Engineering" }
			]
		},
		{ title: "Media Type", schema: "mediaType", type: "checkbox", tags:
			[
				{ title: "Audio", schema: "Audio" },
				{ title: "Exam", schema: "Exam" },
				{ title: "Handout", schema: "Handout" },
				{ title: "Image", schema: "Image" },
				{ title: "Interactive", schema: "Interactive" },
				{ title: "Question", schema: "Question" },
				{ title: "Slide", schema: "Slide" },
				{ title: "Textbook", schema: "Text" },
				{ title: "Video", schema: "Video" },
				{ title: "Website", schema: "Website" }
			]
		}
	];
</script>
<script type="text/javascript">
	var gooruResourcesUrl = "//www.goorulearning.org/gooruapi/rest/search/resource";
	var gooruCollectionsUrl = "//www.goorulearning.org/gooruapi/rest/search/scollection";
	var gooruToken = "<%=GooruSessionToken %>";

	$(document).ready(function () {
		//Create gooru filters
		createGooruFilters();
		//Remove unusable standards bodies
		cleanupGooruStandards();
		//Delayed items
		setTimeout(function () {
			//Replace functions
			overwriteFunctions();
		}, 10);
	});

	//Create gooru filters
	function createGooruFilters() {
		var box = $("#filters");
		var filterTemplate = $("#template_gooruFilter").html();
		var tagTemplate = $("#template_gooruTag").html();

		for (var i in gooruFilters) {
			var filter = gooruFilters[i];
			var tagText = "";
			for (var j in filter.tags) {
				tagText += tagTemplate.replace(/{schema}/g, filter.tags[j].schema).replace(/{title}/g, filter.tags[j].title).replace(/{filter}/g, filter.schema).replace(/{type}/g, filter.type);
				}
			box.append(filterTemplate.replace(/{schema}/g, filter.schema).replace(/{title}/g, filter.title).replace(/{tags}/g, tagText));
		}

		$("[data-filterID=resource]").attr("data-gooruURL", gooruResourcesUrl).attr("data-resultKey", "r").prop("checked", true);
		$("[data-filterID=collection]").attr("data-gooruURL", gooruCollectionsUrl).attr("data-resultKey", "c");
	}

	//Remove unusable standards
	function cleanupGooruStandards() {
		$("#SBddlBody option").not("[value=jsonMath], [value=jsonELA], [value=jsonNGSS]").remove();
		$("#SBddlBody optgroup").not("[label='K-12 Education']").remove();
	}

	//Replace functions
	function overwriteFunctions() {
		//Pack query
		packQuery = function () {
			currentQuery = { //property from main search
				"sessionToken": gooruToken,
				"query": getQueryText(),
				"pageNum": currentStartPage,
				"pageSize": 20, //API limit
				"category": $("[data-filterID=resource]").prop("checked") ? getTags("mediaType", ",") : "", //only include media type for resource searches
				"flt.grade": getTags("gradeLevel", ","),
				"flt.subjectName": getTags("subject", "~~"),
				"flt.standard": getGooruStandards()
			};
		}; //

		//Do search
		doSearch = function () {
			//Avoid spammy requests
			if (!canDoSearch) {
				return;
			}
			else {
				canDoSearch = false;
				resetCooldown();
			}

			//Searching
			setStatus("Searching...", "searching");

			//Do the search
			var targetURL = $("[data-filterID=searchType] input:checked").attr("data-gooruURL");
			$.ajax({
				type: "GET",
				url: targetURL,
				data: currentQuery,
				dataType: "jsonp",
				crossDomain: true,
				cache: false,
				success: function (data) {
					console.log("Results", data);
					currentResults = data;
					currentResults.hits = { total: currentResults.totalHitCount };
					search(searchStages.render);
				}
			});

		}; //

		//Render Results
		renderResults = function () {
			//Get structures
			var template = $("#template_result_" + viewMode).html();
			var thumbnailTemplate = $("#template_thumbnail").html();
			var box = $("#searchResults");
			var results = currentResults.searchResults;

			//Clear result list
			box.html("");

			//Load results
			if (currentResults.totalHitCount == 0) {
				setStatus("Sorry, no results were found. Please try a different combination of search terms and filters", "error");
				return;
			}
			else {
				setStatus("Found " + currentResults.totalHitCount + " Results");
				for (var i in results) {
					var current = results[i];
					//Strip HTML
					var title = $("<div>" + current.title + "</div>").text();
					var description = $("</div>" + current.description + "</div>").text();
					var id = typeof (current.gooruOid) != "undefined" ? current.gooruOid : current.id;
					//Construct link
					var link = "/gooruResource?t=" + $("[data-filterID=searchType] input:checked").attr("data-resultKey") + "&id=" + id;
					//Get thumbnail
					var thumbnailText = thumbnailTemplate.replace(/{resultURL}/g, link).replace(/{imageURL}/g, getGooruThumbnail(current)).replace(/{title}/g, title);
					var keywords = "";
					var paradata = "";
					var standards = "";
					var usageRightsIcon = "";
					box.append(template
						.replace(/{resourceID}/g, id)
						.replace(/{versionID}/g, id)
						.replace(/{resultURL}/g, link)
						.replace(/{title}/g, title)
						.replace(/{description}/g, description)
						.replace(/{keywords}/g, keywords)
            .replace(/{thumbnail}/g, thumbnailText)
            .replace(/{paradata}/g, paradata)
            .replace(/{created}/g, "")
            .replace(/{creator}/g, getGooruCreator(current))
            .replace(/{standards}/g, standards)
						.replace(/{usageRightsIcon}/g, usageRightsIcon)
					);
				}
			}
			
		}; //
	}

	function getTags(target, join) {
		var result = "";
		var items = [];
		$("[data-filterID=" + target + "] input:checked").each(function () {
			items.push($(this).attr("data-tagID"));
		});
		result = items.join(join);
		return result;
	}

	function getGooruStandards() {
		var items = [];
		for (var i in selectedStandards) {
			items.push(selectedStandards[i].fullCode);
		}
		return items.join(",");
	}

	function getGooruThumbnail(current) {
		if (typeof (current.thumbnails) != "undefined") {
			return current.thumbnails.url;
		}
		else {
			var testURL = current.assetURI + (current.thumbnail.indexOf("/") > -1 ? current.thumbnail : current.folder + current.thumbnail);
			if (testURL.indexOf(".png") > -1 || testURL.indexOf(".jpg") > -1 || testURL.indexOf(".jpeg") > -1) {
				return testURL;
			}
			else {
				return "/images/gooru_logo.png";
			}
		}
	}

	function getGooruCreator(current) {
		if (typeof (current.creator) != "undefined") {
			return current.creator.usernameDisplay;
		}
		else {
			return current.creatornameDisplay;
		}
	}
</script>

<style type="text/css">
	#content .result .created { display: none; }
	#content #btnToggleAdvancedMode { display: none; }
	#content #ddlSortMode { display: none; }
	#content #ddlPageSize { display: none; }
	h1 a { color: inherit; text-decoration: underline; }
</style>

<div id="gooruTemplates" style="display: none;">
	<script type="text/template" id="template_gooruFilter">
		<div class="filter collapseBox collapsed" id="filter_{schema}", data-filterID="{schema}">
			<input type="button" class="isleButton bgMain collapseButton" data-filterID="{schema}" data-schema="{schema}" value="{title}" onclick="toggleCollapse('filter_{schema}');" />
			<div class="tags collapsible" data-filterID="{schema}" data-schema="{schema}">
				{tags}
			</div>
		</div>
	</script>
	<script type="text/template" id="template_gooruTag">
		<label><input type="{type}" name="options_{filter}" data-filterID="{schema}" data-tagID="{schema}" /> {title}</label>
	</script>
</div>