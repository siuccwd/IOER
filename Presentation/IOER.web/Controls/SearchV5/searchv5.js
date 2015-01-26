/* ---   ---   ---   --- Page Data Variables ---   ---   ---   --- */
var index = {
  sort: {
    relevancy: { field: "", order: "" },
    newest: { field: "intID", order: "desc" },
    oldest: { field: "intID", order: "asc" },
    liked: { field: "paradata.ratings.likes", order: "desc" },
    disliked: { field: "paradata.ratings.dislikes", order: "desc" },
    rated: { field: "paradata.evaluations.score", order: "desc" },
    viewed: { field: "paradata.views.resource", order: "desc" },
    favorited: { field: "paradata.favorites", order: "desc" },
    comments: { field: "paradata.comments", order: "desc" }
  },
  standardsField: "standards.tags",
  ajaxURL: "/Services/ElasticSearchService.asmx/DoSearchV5",
  ajaxType: "POST",
  ajaxHeaders: { "Accept": "application/json", "Content-type": "application/json; charset=utf-8" },
}
var specialFilters = {
  sb: { name: "standardsBrowser", title: "Standards Browser", id: "'sb'" },
  tips: { name: "tips", title: "Search Tips", id: "'tips'" }
};
var autoTextFilters = [
  { term: "delete", filter: "-delete" },
  { term: "sound", filter: "-freesound" },
  { term: "bookshare", filter: "-bookshare" },
  { term: "smarter balanced", filter: "-\"Smarter Balanced Assessment Consortium\"" },
];

/* ---   ---   ---   --- Page Status Variables ---   ---   ---   --- */
var countdown = {};
var sortMode = { field: "intID", order: "desc" }
var pageSize = 20;
var viewMode = "list";
var selectedTags = [];
var selectedRawStandards = [];
var selectedStandardsIDs = [];
var currentStartPage = 1;
var currentQuery = {};
var requests = [];
var currentResults = {};

/* ---   ---   ---   --- Initialization ---   ---   ---   --- */
$(document).ready(function () {
  renderFilters();
  initSearchBox();
  initTags();
  initStandards();
  insertSpecialButtons();
  initPipeline();
  initDDLs();
  initResize();
  resetCountdown();
});

//Init Search Box
function initSearchBox() {
  $(".txtSearch").on("keyup change", function (e) {
    resetCountdown();
  });
}

//Init Tags
function initTags() {
  //Foreach filter...
  $("#tags .filter").each(function () {
    var filter = $(this);
    //Foreach checkbox in it...
    filter.find("input[type=checkbox]").on("change", function () {
      var box = $(this);
      for (i in filters) {
        //Find the matching JS object by ID
        if (filters[i].id == parseInt(filter.attr("data-filterID"))) {
          for (j in filters[i].items) {
            if (filters[i].items[j].id == parseInt(box.attr("data-tagID"))) {
              //Toggle its selected property
              filters[i].items[j].selected = box.prop("checked");
              //Fire an event to signal the change
              pipeline("updateFiltering");
            }
          }
        }
      }
    });
  });
  //Handle clicking outside the filters
  $("#btnToggleFilters, #filters, #tags").on("click", function (event) {
    event.stopPropagation();
  });
  $("body").not("#btnToggleFilters, #filters, #tags").on("click", function () {
    hideAllFilters();
  });
}

//Handle receiving messages from standards browser
function initStandards() {
  $(window).on("SB7search", function (event, d1, d2) {
    selectedRawStandards = d1;
    selectedStandardsIDs = d2;
    pipeline("updateFiltering");
  });
}

//Insert special buttons
function insertSpecialButtons() {
  var filters = $("#filters");
  var template = $("#template_filterButton").html();
  //Insert at arbitrary positions
  filters.prepend(jsCommon.fillTemplate(template, specialFilters.sb));
  filters.append(jsCommon.fillTemplate(template, specialFilters.tips));
}

//Have things happen when the DDLs change
function initDDLs() {
  //Sort Order
  $("#ddlSort").on("change", function () {
    sortMode = index.sort[$(this).find("option:selected").attr("value")];
    currentStartPage = 1;
    pipeline("skipCountdown");
  });
  //Number of items to show
  $("#ddlPageSize").on("change", function () {
    pageSize = parseInt($(this).find("option:selected").attr("value"));
    currentStartPage = 1;
    pipeline("skipCountdown");
  });
  //View mode
  $("#ddlViewMode").on("change", function () {
    viewMode = $(this).find("option:selected").attr("value");
    pipeline("searchResultsReturned");
  });
}

//Handle things happening on the page related to searches
function initPipeline() {
  $(window).on("pipeline", function (event, data) {
    switch (data.from) {
      case "updateFiltering":
        updateTags();
        renderStandardsTags();
        renderTags();
      case "updateTextQuery":
        resetCountdown();
        break;
      case "countdownEnded":
      case "skipCountdown":
        packQuery();
        doSearch();
        break;
      case "searchResultsReturned":
        renderSearchResults();
        renderPaginators();
        doPostSearchActions();
        break;
      default:
        break;
    }
  });
}

//Handle things that should happen when the window is resized
function initResize() {
  $(window).on("resize", function () {
    exposeShortDescriptions();
  });

  $(window).trigger("resize");
}

/* ---   ---   ---   --- Page Functions ---   ---   ---   --- */
//Hit the pipeline at a specified point
function pipeline(target) {
  $(window).trigger("pipeline", { from: target });
}

//Reset the search countdown
function resetCountdown() {
  clearTimeout(countdown);
  countdown = setTimeout(function () { pipeline("countdownEnded"); }, 800);
}

//Show/hide the filter buttons
function toggleFilters() {
  $("#filters").toggleClass("showing").fadeToggle(250);
  if (!$("#filters").hasClass("showing")) {
    $("#tags .filter").removeClass("showing").fadeOut(250);
  }
}

//Show/hide the filters
function showHideFilter(target) {
  var target = $("#tags .filter[data-filterID=" + target + "]");
  var allFilters = $("#tags .filter");
  if (target.hasClass("showing")) {
    allFilters.removeClass("showing").fadeOut(250);
  }
  else {
    allFilters.removeClass("showing");
    allFilters.not(target).fadeOut(250);
    target.addClass("showing").fadeIn(250);
  }
}

//Hide open filters & filter list
function hideAllFilters() {
  $("#tags .filter").removeClass("showing").fadeOut(250);
  $("#filters").removeClass("showing").fadeOut(250);
}

//Update the selected tags/filters
function updateTags() {
  selectedTags = [];
  for (i in filters) {
    var insert = { field: filters[i].esID, items: [] };
    for (j in filters[i].items) {
      if (filters[i].items[j].selected) {
        insert.items.push(filters[i].items[j].id);
      }
    }
    if (insert.items.length > 0) {
      selectedTags.push(insert);
    }
  }
}

//Prepare a query to send to the search
function packQuery() {
  var query = {};
  query.text = getSearchText();
  query.filters = selectedTags;
  if (selectedStandardsIDs.length > 0) {
    query.filters.push({ field: index.standardsField, items: selectedStandardsIDs });
  }
  query.size = pageSize;
  query.sort = index.sort[sortMode];
  query.start = (currentStartPage - 1) * pageSize;
  currentQuery = query;
}

//Format text for a search
function getSearchText() {
  var text = $(".txtSearch").val().trim();
  if (text == "") { text = "* "; }
  var appliedTextFilters = "";
  try {
    //Apply auto filters if the terms aren't part of the query
    for (i in autoTextFilters) {
      for (j in autoTextFilters[i].terms) {
        if (text.indexOf(autoTextFilters[i].terms[j]) > -1) {
          autoTextFilters[i].applied = false;
        }
      }
      if (autoTextFilters[i].applied) {
        appliedTextFilters += " " + autoTextFilters[i].filter;
      }
    }
  }
  catch (e) { }
  return text + appliedTextFilters;
}

//Do a search
function doSearch() {
  //Cancel any pending requests
  for (i in requests) {
    requests[i].abort();
  }
  requests = [];
  //Do the search
  var request = $.ajax({
    url: index.ajaxURL,
    async: true,
    headers: index.ajaxHeaders,
    dataType: "json",
    type: index.ajaxType,
    data: JSON.stringify({ query: currentQuery }),
    success: function (msg) { (msg.d ? currentResults = msg.d : currentResults = msg); pipeline("searchResultsReturned"); }
  });
  requests.push(request);
}

//Do miscellaneous things after a search has been completed
function doPostSearchActions() {
  exposeShortDescriptions();
}

//Do an auto search when clicking on an item
function searchMe(text) {
  $(".txtSearch").val(text);
  pipeline("skipCountdown");
}

//Expand/collapse a description
function expandCollapse(id, button) {
  var box = $(".result[data-resourceID=" + id + "]");
  var description = box.find(".description");
  box.toggleClass("collapsed");
  if(box.hasClass("collapsed")){
    description.css("max-height", "500px;");
    setTimeout(function () { description.css("max-height", "75px"); }, 1);
    $(button).attr("value", "More");
  }
  else {
    description.css("max-height", "500px");
    setTimeout(function () { description.css("max-height", "none"); }, 250);
    $(button).attr("value", "Less");
  }
}

//Show or hide the fade effect for descriptions based on length
function exposeShortDescriptions() {
  $(".result").each(function () {
    var box = $(this);
    if (box.find(".description").outerHeight() < 75) {
      box.addClass("short");
    }
    else {
      box.removeClass("short");
    }
  });
}

//Switch pages
function paginate(targetPage) {
  currentStartPage = targetPage;
  pipeline("skipCountdown");
}

/* ---   ---   ---   --- Rendering Functions ---   ---   ---   --- */
//Render Filters
function renderFilters() {
  //Setup
  var filterLinksHTML = "";
  var tagListHTML = "";
  var filterButtonTemplate = $("#template_filterButton").html();
  var tagListTemplate = $("#template_tagList").html();
  var tagTemplate = $("#template_tag").html();
  //Foreach filter...
  for (i in filters) {
    //Fix missing ID if needed
    if (typeof (filters[i].id) == "undefined") {
      filters[i].id = i;
    }
    filters[i].esID = filters[i].field + "IDs";
    var tagsHTML = "";
    //Foreach tag in each filter...
    for (j in filters[i].items) {
      filters[i].items[j].selected = false;
      //Fill the tags
      tagsHTML += jsCommon.fillTemplate(tagTemplate, filters[i].items[j]);
    }
    //Fill the tag holder and add tags to it
    tagListHTML += jsCommon.fillTemplate(tagListTemplate, filters[i]);
    tagListHTML = tagListHTML.replace(/{insertTags}/, tagsHTML);
    //Create the filter buttons
    filterLinksHTML += jsCommon.fillTemplate(filterButtonTemplate, filters[i]);
  }
  //Append the HTML
  $("#tags").append(tagListHTML);
  $("#filters").append(filterLinksHTML);
}

//Render selected standards
function renderStandardsTags(data) {
  console.log("Standards Data:");
  console.log(data);
  console.log("renderStandardsTags() not implemented yet");
}

//Render selected tags
function renderTags() {
  console.log("renderTags() not implemented yet");
}

//Render search results
function renderSearchResults() {
  if (typeof (currentResults) == "string") {
    currentResults = $.parseJSON(currentResults);
  }
  var list = currentResults.hits.hits;
  console.log(currentResults);
  var box = $("#searchResults");
  var template = "";
  switch(viewMode){
    case "list": template = $("#template_result_list").html(); break;
    case "grid": template = $("#template_result_grid").html(); break;
    case "text": template = $("#template_result_text").html(); break;
    default: break;
  }
  box.html("").hide();

  //If no results
  if (currentResults.hits.total == 0) {
    $("#searchStatus").html("Sorry, no results found. Please adjust your search.");
  }
  else {
    $("#searchStatus").html("Found " + currentResults.hits.total + " results");
    for (i in list) {
      var current = list[i]._source;
      var data = {
        resourceID: current.intID,
        versionID: current.versionID,
        elasticID: list[i]._id,
        title: current.title,
        shortTitle: current.simpleTitle,
        creator: current.creator,
        description: current.description,
        created: current.created,
        metadata: renderMetadata(current),
        standards: renderStandards(current),
        thumbnail: renderThumbnail(current)
      };
      box.append(jsCommon.fillTemplate(template, data));
    }
  }

  box.fadeIn();
}
//Render HTML for metadata section
function renderMetadata(data) {
  var toReturn = "";
  for (i in data.keywords) {
    toReturn += '<input type="button" class="keyword linkButton" onclick="searchMe(\'' + data.keywords[i] + '\');" title="' + data.keywords[i] + '" value="' + data.keywords[i] + '" />';
    if (i > 6) { break; }
  }
  return toReturn;
}
//Render HTML for standards section of search results
function renderStandards(data) {
  console.log(data.standards);
  var toReturn = "";
  for (i in data.standards.tags) {
    toReturn += '<input type="button" class="standard linkButton" onclick="searchMe(\'' + data.standards.tags[i] + '\');" value="' + data.standards.tags[i] + '" />';
  }
  return toReturn;
}
//Render HTML for thumbnail
function renderThumbnail(data) {
  var template = $("#template_thumbnail").html();
  return jsCommon.fillTemplate(template, { resourceID: "564" + (Math.random() * 1000).toString().split(".")[0], paradata: renderParadata(data) }); //Temporary
  //return jsCommon.fillTemplate(template, { resourceID: data.intID, paradata: "" });
}
//Render HTML for paradata
function renderParadata(data) {
  var html = "";
  var info = data.paradata;
  var template = $("#template_paradataIcon").html();
  //Comments
  if (info.comments > 0) {
    html += jsCommon.fillTemplate(template, {text: "Comments", numbers: info.comments, icon: "/images/icons/icon_comments_bg.png" });
  }
  //Evaluations
  if (info.evaluations.count > 0) {
    html += jsCommon.fillTemplate(template, { text: "Evaluation Average Score", numbers: info.evaluations.score, icon: "/images/icons/icon_ratings_bg.png" });
  }
  //Favorites
  if (info.favorites > 0) {
    html += jsCommon.fillTemplate(template, { text: "Favorites/Library Additions", numbers: info.favorites, icon: "/images/icons/icon_library_bg.png" });
  }
  //Likes
  if (info.ratings.likes > 0) {
    html += jsCommon.fillTemplate(template, { text: "Likes", numbers: info.ratings.likes, icon: "/images/icons/icon_likes_bg.png" });
  }
  //Dislikes
  if (info.ratings.dislikes > 0) {
    html += jsCommon.fillTemplate(template, { text: "Dislikes", numbers: info.ratings.dislikes, icon: "/images/icons/icon_dislikes_bg.png" });
  }
  //Click-Throughs
  if (info.views.resource > 0) {
    html += jsCommon.fillTemplate(template, { text: "Resource Visits", numbers: info.views.resource, icon: "/images/icons/icon_click-throughs_bg.png" });
  }

  return html;
}

//Render paginators
function renderPaginators() {
  var totalResults = currentResults.hits.total;
  var template = $("#template_paginatorButton").html();
  var totalPages = Math.ceil(totalResults / pageSize);
  var pageStops = [1, 5, 10, 25, 50, 100, 250, 500, 1000, 5000, 10000, 100000, totalPages];

  $(".paginator").html("");
  if (totalResults > 0) {
    for (var i = 1; i < totalPages + 1; i++) {
      if ((i >= currentStartPage - 2 && i <= currentStartPage + 2) || pageStops.indexOf(i) > -1) {
        $(".paginator").append(template.replace(/{page}/g, i).replace(/{current}/g, (i == currentStartPage ? "selected" : "")));
      }
    }
  }
}