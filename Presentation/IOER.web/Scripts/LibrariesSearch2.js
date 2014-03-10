/* Initial Data */
var results = [];
var currentMode = "";
var oldMode = "";
var searchTimeout;
var searchType = "libraries";
var sortMode = "updated|desc";

var css;

/* Initial Setup */
$(document).ready(function () {
  setupFilters();
  setupSearchBox();
  setupParameters();

  $(window).on("resize", function () {
    handleResizing();

    //Fix for Safari/mobile
    if (navigator.userAgent.indexOf("5.1.7 Safari") > -1 || navigator.userAgent.indexOf("5.0 Safari") > -1 || navigator.userAgent.indexOf("4.0 Mobile Safari") > -1) {
      setInterval(doSafariFixes, 1000);
      doSafariFixes();
    }

  }).trigger("resize");

  resetTimeout();
});


//Setup Search Box
function setupSearchBox() {
  $("#txtSearch").on("keyup", function (e) {
    if (e.which == 13 || e.keyCode == 13) {
      resetTimeout(10);
      return false;
    }
  })
}

//Setup Filters
function setupFilters() {
  var list = $("#filterLists");
  //For each filter in the list
  for (i in filters) {
    //Append the header
    list.append($("<h2>" + filters[i].header + "</h2>").addClass("mid"));
    list.append($("<p>" + filters[i].tip + "</h2>").addClass("tip"));
    //Generate the list
    for (j in filters[i].items) {
      var current = filters[i].items[j];
      var template = "";
      if (filters[i].type == "rbl") {
        template = $("#template_rb").html();
      }
      else if (filters[i].type == "cbxl") {
        template = $("#template_cbx").html();
      }
      list.append(template
        .replace(/{id}/g, filters[i].name + "_" + current.value)
        .replace(/{name}/g, filters[i].name)
        .replace(/{value}/g, current.value)
        .replace(/{text}/g, current.text)
        .replace(/{listID}/g, i)
        .replace(/{itemID}/g, j)
      );
      if (current.selected) {
        list.find("input").last().prop("checked", true);
      }
    }
  }

  //On click of a filter...
  $("#filterLists input").on("change", function () {
    updateFilters();
  });
}

function setupParameters() {
  //Search Type
  $("#searchType input").on("change", function () {
    if ($(this).prop("checked")) {
      searchType = $(this).val();
    }

    if (searchType == "libraries") {
      $("#txtSearch").attr("placeholder", "Search for Libraries...");
    }
    else {
      $("#txtSearch").attr("placeholder", "Search for Collections...");
    }

    resetTimeout(10);
  });

  //Sorting DDL
  $("#ddlSortingOptions").on("change", function () {
    sortMode = $(this).find("option:selected").val();
    resetTimeout(10);
  });
}

function doSafariFixes() {
  var windowWidth = $(window).width();
  $("#rightColumn").width($("#rightColumn").parent().width() - 225);
  $("#searchType, #searchType label").css("width", "100%");
  $(".result .collections p").each(function () { $(this).width($(this).parent().width() - 125); });
  if (windowWidth <= 580) {
    $("#rightColumn").width($("#rightColumn").css("width","100%"));
  }
  if(windowWidth > 950){
    $(".result .data").each(function () { $(this).width($(this).parent().width() - 175); });
  }
  else if (windowWidth > 450) {
    $(".result .data").each(function () { $(this).width($(this).parent().width() - 175); });
  }
  else {
    $(".result .data").each(function () { $(this).width($(this).parent().width() - 100); });
  }
}


/* Page Functions */
function handleResizing() {
  //Determine the current size mode
  currentMode = $("#widthMode").css("width") == "100%" ? "full" : "compact";
  //If the mode hasn't changed, skip the rest
  if (currentMode == oldMode) {
    return;
  }
  //Otherwise set the mode and continue
  oldMode = currentMode;

  if ($("#filterToggler").css("display") == "block") {
    hideFilters();
  }
  else {
    showFilters();
  }
}

function hideFilters() {
  $("#filtersBox").addClass("hidden");
}
function showFilters() {
  $("#filtersBox").removeClass("hidden");
}
function showHideFilters() {
  if ($("#filtersBox").hasClass("hidden")) {
    showFilters();
  }
  else {
    hideFilters();
  }
}

function updateFilters() {
  $("#filterLists input").each(function () {
    var item = $(this);
    var listID = parseInt(item.attr("data-listID"));
    var itemID = parseInt(item.attr("data-itemID"));

    filters[listID].items[itemID].selected = item.prop("checked");

  });

  resetTimeout(800);
}

function resetTimeout(delay) {
  clearTimeout(searchTimeout);
  searchTimeout = setTimeout(doSearch, delay);
}

function doSearch() {
  $("#resultsCount").html("Searching...");
  var data = {
    searchType: searchType,
    text: $("#txtSearch").val().trim(),
    filters: getFilters(),
    userGUID: userGUID,
    useSubscribedLibraries: useSubscribedLibraries,
    sort: sortMode,
    start: 0
  }

  doAjax("DoLibrariesSearch", data, successDoSearch);
}

function getFilters() {
  var output = [];
  for (i in filters) {
    var currentFilter = filters[i];
    var currentOut = { name: currentFilter.name, ids: [] };
    for (j in currentFilter.items) {
      var currentItem = currentFilter.items[j];
      if (currentItem.selected) {
        currentOut.ids.push(currentItem.value);
      }
    }
    if (currentOut.ids.length > 0) {
      output.push(currentOut);
    }
  }

  return output;
}

function resetSearch() {
  clearFilters();
  $("#txtSearch").val("");
  resetTimeout();
}
function clearFilters() {
  $("input[name=dateRange][value=5]").prop("checked", true);
  $("input[type=checkbox]").prop("checked", false);
  $("input[name=view][value=1]").prop("checked", true);
  resetTimeout();
}

/* AJAX Functions */
function doAjax(method, data, success) {
  $.ajax({
    url: "/Services/LibraryService.asmx/" + method,
    async: true,
    success: function (msg) {
      try {
        success($.parseJSON(msg.d));
      }
      catch (e) {
        success(msg.d);
      }
    },
    type: "POST",
    data: JSON.stringify(data),
    dataType: "json",
    contentType: "application/json; charset=utf-8"
  });
}

function successDoSearch(data) {
  console.log(data.isValid == true)
  if (data.isValid) {
    results = data.data;
    loadResults(data.data, data.extra.totalResults);
    updatePaginator(data.extra.totalResults);
  }
  else {
    $("#resultsCount").html("Error: " + data.status);
  }
}


/* Rendering Functions */
function loadResults(data, total) {
  //Setup initial data
  $("#resultsCount").html((total > 0) ? "Found " + total + " results." : "Sorry, no results found.");
  var resultTemplate = $("#template_searchResult").html();
  var colTemplate = $("#template_collection").html();
  var list = $("#resultsList");
  list.addClass("hidden");

  //Clear the results container
  list.html("");

  //For each item in the results...
  for (i in data) {
    var current = data[i];
    var colList = "";

    //Build the collection list for this result
    if (searchType == "libraries") {
      for (j in current.collections) {
        colList += colTemplate
          .replace(/{title}/g, current.collections[j].title)
          .replace(/{iconURL}/g, ((current.collections[j].iconURL == "" || current.collections[j].iconURL == "defaultURL") ? "/images/isle.png" : current.collections[j].iconURL))
          .replace(/{iconSRC}/g, ((current.collections[j].iconURL == "" || current.collections[j].iconURL == "defaultURL") ? "src=\"/images/isle.png\"" : "img src=\"" + current.collections[j].iconURL + "\""))
          .replace(/{description}/g, current.collections[j].description)
          .replace(/{collectionURL}/g, current.collections[j].url)
      }

        //.replace(/{collectionURL}/g, current.url + "?col=" + current.collections[j].id)
    }

    //Build the result
    list.append(
      resultTemplate
        .replace(/{title}/g, current.title)
        .replace(/{libraryID}/g, current.id)
        .replace(/{iconURL}/g, ((current.iconURL == "" || current.iconURL == "defaultUrl") ? "/images/isle.png" : current.iconURL))
        .replace(/{iconSRC}/g, ((current.iconURL == "" || current.iconURL == "defaultUrl") ? "src=\"/images/isle.png\"" : "img src=\"" + current.iconURL + "\""))
        .replace(/{libraryURL}/g, current.url)
        .replace(/{description}/g, current.description)
        .replace(/{collections}/g, colList)
    );
    
    if (searchType == "collections") {
      list.find(".collectionsTitle").remove();
    }
  }

  if (searchType == "collections") {
    $("#resultsList .collections").remove();
  }
  setTimeout(function () { list.removeClass("hidden"); }, 500);
}

function updatePaginator(total) {

}