/*   ---   ---   Page Init   ---   ---    */
$(document).ready(function () {
  injectHeader(); //Inserts the Library controls header into the search code
  renderLibColInfo(libraryData); //Uses the libraryData object written by the server on initial page load to render the Library Data without needing an initial AJAX call
  showPanel("pnlDetails"); //Show the details panel and trigger any related methods, like highlighting the associated tab
  linkedCollectionID == 0 ? pickLibrary() : pickCollection(linkedCollectionID); //If there isn't a valid linked collection (server sets this to 0 if there's no "col" param), select the Library. Otherwise select the desired collection.
  showActiveTitle(); //Trigger the method that shows the currently selected (or hovered) collection
  //setTimeout(updateNarrowing, 500); //Auto-search. updateNarrowing is in the ElasticSearch file, but loaded in memory when the document is ready, and thus accessible from here.
  setupAccessLevelDDLs();
  expandCollapsePanels();

  //Inject library stuff on search result load
  $(window).on("resultsRendered", function () {
    renderLibraryControls();
  });

  if (typeof (parent.hasSelector) == "function") {
    $("<div style=\"text-align: center;\"><input type=\"button\" id=\"btnSendExternal\" onclick=\"sendResultsExternal()\" value=\"Send the Displayed Resources to External Site\" /></div>").insertAfter("#resultCount");
  }

  setupDatePickers();
});

//Inserts the Library controls header into the search code
function injectHeader() { 
  $("#libraryHeader").insertAfter("#pageTitle");
}

//Ensures that the public level is always the same or more public than the organization access level
function setupAccessLevelDDLs() {
  if ($("#pnlSettings").length == 0) { return; }
  var pub = $(".ddlPublicAccessLevels");
  var org = $(".ddlOrganizationAccessLevels");
  pub.on("change", function () {
    var pubVal = parseInt(pub.find("option:selected").val());
    var orgVal = parseInt(org.find("option:selected").val());

    if (orgVal < pubVal) {
      org.find("option[value=" + pubVal + "]").prop("selected", true);
    }
    org.find("option").removeAttr("disabled", false);
    org.find("option").each(function () {
      var option = $(this);
      if (parseInt(option.val()) < pubVal) {
        option.attr("disabled", "disabled");
      }
    });
  });
}


/*   ---   ---   Data-Rendering Functions   ---   ---   */
//Regenerate the Library and Collection avatars and associated information in the Header
function renderLibColInfo(data) {
  renderLibrary(data.library);
  renderCollections(data.collections);
  initHoverEffects();
}
//Set the page header and library avatar to the appropriate values
function renderLibrary(data) {
  $("h1#pageTitle").html(data.title);
  $("#libraryAvatar")
    .css("background-image", "url('" + data.avatarURL + "')")
    .attr("title", data.title);
}
//Setup the collection avatars
function renderCollections(data) {
  var template = $("#template_collectionIcon").html();
  var box = $("#collectionList");
  box.html("");
  for (i in data) { //foreach collection, create its avatar and append it to the box of collection avatars
    box.append(
      template
        .replace(/{id}/g, data[i].id)
        //.replace(/{icon}/g, data[i].avatarURL )
        .replace(/{title}/g, data[i].title)
    );
    box.find("a[data-collectionID=" + data[i].id + "] .iconImg").attr("src", data[i].avatarURL);
  }
}
//Setup the fancy hover effects and text output when the user highlights/tabs to the Library/collection avatars
function initHoverEffects() {
  $("#libraryAvatar").on("mouseover focus", function () { //on mouseover or tab-to..
    $("#libColTitle").html("Library: " + libraryData.library.title); //change this helpful text to indicate what the user is hovering over
  }).on("mouseout blur", function () { //on mouseout or tab-away...
    showActiveTitle(); //change the helpful text back to the item that is actually selected
  });
  $("#collectionList a").each(function () { //foreach avatar in the list of collections, do the same stuff as above. The extra for loop is due to there being a list of collections vs the single Library avatar.
    $(this).on("mouseover focus", function () { 
      for (i in libraryData.collections) {
        if (libraryData.collections[i].id.toString() == $(this).attr("data-collectionID")) {
          $("#libColTitle").html("Collection: " + libraryData.collections[i].title);
        }
      }
    }).on("mouseout blur", function () {
      showActiveTitle();
    });
  });
}
//Change the helper text to indicate the currently-selected item
function showActiveTitle() {
  var active = getActive();
  $("#libColTitle").html((active.isLibrary ? "Library: " : "Collection: ") + active.data.title); //Prepend the helper text with Library or Collection as appropriate
}
//Render the items in the Details panel
function renderDetails() {
  $("#btnAddDislike").remove(); //Possibly temporary
  var active = getActive(); //Get the active object, be it a collection or the library
  //Basics
  $("#detailsAvatar").attr("src","").css("background-image", "url('" + active.data.avatarURL + "')"); //Change the big avatar to match the active item's avatar
  $("#detailsContent .panelHeader").html(active.data.title); //Change the header text
  $("#detailsContent #description").html(active.data.description); //Change the description text

  //Likes/Dislikes
  var likes = active.data.paradata.likes; 
  var dislikes = active.data.paradata.dislikes;
  var max = likes + dislikes;
  var likePercent = "100%"; //Setting these initial values saves time later...
  var dislikePercent = "0%";
  if (max > 0) { //...namely because you only have to bother with calculations if there are actually any ratings to consider 
    likePercent = ((likes / max) * 100) + "%";
    dislikePercent = ((dislikes / max) * 100) + "%";
  }
  $("#likeBar").css("width", likePercent); //The rest of this just fiddles with the CSS and displayed text for presenting the like/dislike bar
  $("#dislikeBar").css("width", dislikePercent);
  $("#likeText").html(likes);
  $("#dislikeText").html(dislikes);
  $("#likeBar, #dislikeBar").removeClass("full");
  if (likePercent == "100%") { $("#likeBar").addClass("full"); }
  if (dislikePercent == "100%") { $("#dislikeBar").addClass("full"); }

  if (userGUID != "") {
    if (active.data.paradata.iLikeThis) {
      $("#btnAddLike, #btnAddDislike").hide();
      $("#likeDislikeText").show().html("You like this " + (active.isLibrary ? "Library" : "Collection") + ".");
    }
    else if (active.data.paradata.iDislikeThis) {
      $("#btnAddLike, #btnAddDislike").hide();
      $("#likeDislikeText").show().html("You dislike this " + (active.isLibrary ? "Library" : "Collection") + ".");
    }
    else {
      $("#btnAddLike, #btnAddDislike").show();
      $("#likeDislikeText").hide().html("");
    }
  }
  else {
    $("#opinionButtons").html("<p class=\"middle\">Please login to Like or Dislike this Library.</p>");
  }
}

//Render the Share and Follow panel
function renderShareFollow() {
  var active = getActive(); //Get the active object, be it a collection or the library
  $("#pnlShareFollow .panelHeader").html("Share &amp; Follow " + active.data.title); //Change the header text
  $("#shareFollowContent #shareBox").attr("data-isLibrary", active.isLibrary).attr("data-id", active.data.id); //Change HTML attributes of the shareBox div to indicate whether or not the active item is the Library, and its ID. These are used elsewhere.

  //Share Link
  if (active.data.currentPublicAccessLevel > 0) {
    $("#shareBox").show();
    $("#shareBox h4.shareLinkHeader").html("Share this " + (active.isLibrary ? "Library:" : "Collection:")); //Changes the shareBox's header to "Share this Library" or "Share this Collection" as appropriate
    var hrefs = window.location.href.split("/");
    $("#txtShareBox").val(hrefs[0] + "//" + hrefs[2] + "/Libraries/Library.aspx?id=" + libraryData.library.id + (active.isLibrary ? "" : "&col=" + active.data.id)); //Changes the share link to match the selected item

    //Widget config
    var box = $("#widgetConfigList");
    box.html("");
    for (i in libraryData.collections) {
      var item = libraryData.collections[i];
      if (item.currentPublicAccessLevel <= 3) {
        console.log("adding collection");
        box.append("<label for=\"w" + item.id + "\"><input type=\"checkbox\" id=\"w" + item.id + "\" value=\"" + item.id + "\"> " + item.title + " <span class=\"shareID\">(ID: " + item.id + ")</span>" + "</label>");
      }
    }
    box.find("input").on("click", function () {
      updateWidgetLink();
    });
  }
  else {
    $("#shareBox").hide();
  }
  updateWidgetLink();

  //Following options
  $("#followBox h4").html("Follow this " + (active.isLibrary ? "Library:" : "Collection:"));
  if (userGUID == "") { //If the user isn't logged in, display a message in place of the DDL. This also replaces the DDL, since it won't be needed, since the user doesn't login via AJAX.
    $("#shareFollowContent #followBox").html("<p class=\"middle\">Please Login to Follow this " + (active.isLibrary ? "Library" : "Collection") + ".</p>");
  }
  else if (libraryData.isMyLibrary) { //Otherwise, if the user is currently on their own library, eliminate the following options.
    $("#shareFollowContent #followBox").html("");
  }
  else if (libraryData.library.paradata.following > 0 && !active.isLibrary) { //If the active item is a collection and the user is already following the library, replace the following options with a message.
    $("#shareFollowContent #followBox #followingMessage").show();
    $("#shareFollowContent #followBox #followingControls").hide();
  }
  else { //Otherwise (the user is logged in and not viewing their own library), unset all existing following option selections and set the correct one. The unset happens because the HTML is not regenerated when a different collection is picked; it is merely tweaked here.
    $("#shareFollowContent #followBox #followingMessage").hide();
    $("#shareFollowContent #followBox #followingControls").show();
    $("#followingOptions option").attr("selected", false);
    $("#followingOptions option[value=" + active.data.paradata.following + "]").prop("selected", true).attr("selected", "selected");
  }

}


//Render the Comments panel
function renderComments() {
  var active = getActive(); //Get the currently-selected object
  $("#pnlComments .panelHeader").html("Comments on " + active.data.title); //Change the header text
  $("#commentsBox #comments").html(""); //Clear out any comments from a previous selection
  var template = $("#template_comment").html();
  for (i in active.data.paradata.comments) { //Comments will be rendered here, once they are in place in the object/code
    var current = active.data.paradata.comments[i];
    $("#commentsBox #comments").append(
      template
        .replace(/{id}/g, current.id)
        .replace(/{date}/g, current.date)
        .replace(/{name}/g, current.name)
        .replace(/{text}/g, current.text)
    );
  }
  if (active.data.paradata.comments.length == 0) { //If there are no comments, display a message
    $("#commentsBox #comments").html("<p class=\"middle\">No Comments Yet!</p>");
  }
  $("#showHideComments").css("display", ($("#commentsBox #comments").height() > $("#postCommentBox").height() ? "block" : "none")); //Only show the button to expand the comments section if the comments push the height of the section beyond a certain threshhold.
  if (userGUID == "") { //Replace the comment box with a message if the user is not logged in
    $("#postCommentBox").html("<p class=\"middle\">Please Login to Comment.</p>");
  }
}

//Render the Settings panel
function renderSettings() {
  if ($("#pnlSettings").length == 0) { return; }
  var active = getActive(); //Get the currently-selected object
  $("#txtTitle").val(active.data.title); //Change the Title box text to match the current object
  $("#txtDescription").val(active.data.description); //Ditto details box text
  $("#libColID").html((active.isLibrary ? "Library" : "Collection") + " ID: " + active.data.id);
  $("#pnlSettings input[type=checkbox]").prop("checked", false); //Uncheck all of the checkboxes
  $(".ddlOrganizationAccessLevels option[value=" + active.data.currentOrganizationAccessLevel + "]").prop("selected", true);
  $(".ddlPublicAccessLevels option[value=" + active.data.currentPublicAccessLevel + "]").prop("selected", true).trigger("change");
  if (active.isLibrary) { //If the current object is the Library...
    $("#defaulter").hide(); //Hide the "Default Collection" stuff
    $("#btnDeleteCollection").hide();
  }
  else { //If the current object is a Collection, show/hide and check/uncheck the relevant boxes
    $("#defaulter").show();
    if (active.data.isDefaultCollection) { //Swap the "Default Collection" stuff for a label if the current collection IS the default collection--we don't want to allow there to be no default collection.
      $("#lblMakeDefault").hide();
      $("#defaulter p").show();
      $("#btnDeleteCollection").hide();
    }
    else { //Note that the showing and hiding is very explicit throughout this method, since the HTML is tweaked rather than completely regenerated
      $("#lblMakeDefault").show();
      $("#defaulter p").hide();
      $("#btnDeleteCollection").show();
    }
  }
}

function renderLibraryControls() {
  $("#searchResults .result.list").each(function () { //foreach search result...
    //Basic Data
    var item = $(this); //create a jQuery object for this result
    var intID = parseInt(item.attr("data-resourceID")); //Grab attributes from this result's HTML
    var index = item.index(); //This item's position in the result list
    var vid = currentResults.hits.hits[index]._source.VersionId;
    var inAllLibraries = currentResults.hits.hits[index]._source.LibraryIds;
    var inAllCollections = currentResults.hits.hits[index]._source.CollectionIds; //currentResults is the object returned by ElasticSearch itself, so we have to dig into it to get to what we want--in this case, the list of every collection that this Resource is in, regardless of Library
    var box = item.find(".modBox_before"); //"box" is the actual part of the displayed search result where the drop-down lists will be inserted
    var ddl = $("#template_ddl").html().replace(/{vid}/g, vid).replace(/{intID}/g, intID); //Grab the HTML for the drop-down lists and inject the attributes we grabbed a moment ago
    var active = getActive(); //Get the currently-selected object (Library or Collection)
    var saveButton = $("<input></input>") //Create the Save button for this set of DDLs
      .attr("type", "button")
      .attr("onclick", "doResourceAction(" + intID + "); return false;")
      .attr("value", "Save")
      .addClass("resourceActionButton");

    //Get lists
    var myLibraries = [];
    var thisLibCollections = [];

    //Fill lists
    for (i in libraryData.myLibraries) { //for each of my libraries...
      var current = libraryData.myLibraries[i];
      var data = {
        id: current.id,
        title: current.title,
        containsThisResource: false,
        collections: []
      };
      for (j in inAllLibraries) { //flag this library if it contains this resource
        if (inAllLibraries[j] == current.id) {
          data.containsThisResource = true;
          break;
        }
      }
      for (j in current.collections) { //for each collection in this library...
        var colData = {
          id: current.collections[j].id,
          title: current.collections[j].title,
          containsThisResource: false
        };
        for (k in inAllCollections) { //flag it if it contains this resource
          if (inAllCollections[k] == current.collections[j].id) {
            colData.containsThisResource = true;
          }
        }
        data.collections.push(colData); //add this collection to the list
      }
      myLibraries.push(data); //add this library to the list
    }

    for (i in libraryData.collections) { //for each of the collections in this library...
      var current = libraryData.collections[i];
      var colData = {
        id: current.id,
        title: current.title,
        containsThisResource: false
      };
      for (j in inAllCollections) { //flag it if it contains this resource
        if (inAllCollections[j] == current.id) {
          colData.containsThisResource = true;
        }
      }
      thisLibCollections.push(colData); //add this collection to the list
    }

    //Setup the DDLs for this Resource
    var ddlTargetCollections = $(ddl)
      .attr("data-name", "targetCollections"); //dynamically filled based on chosen library

    //Fill the DDL of My Libraries
    var ddlTargetLibraries = $(ddl)
      .attr("data-name", "targetLibraries")
      .append("<option value=\"0\">Library...</option>")
      .on("change", function () {
        fillTargetCollections($(this), inAllCollections);
      });
    for (i in myLibraries) {
      var current = myLibraries[i];
      ddlTargetLibraries
        .append(
          $("<option></option>")
          .attr("value", current.id)
          .html(current.title + (current.containsThisResource ? " (Contains this Resource)" : ""))
        );
    }

    //Fill the DDL of Collections that contain this Resource
    var ddlSourceCollections = $(ddl)
      .attr("data-name", "sourceCollections")
      .append("<option value=\"0\">Source Collection...</option>");
    for (i in thisLibCollections) {
      var current = thisLibCollections[i];
      if (current.containsThisResource) {
        ddlSourceCollections
          .append(
            $("<option></option>")
            .attr("value", current.id)
            .html(current.title)
          );
      }
    }

    //Setup the generic Actions DDL
    var ddlActions = $(ddl)
      .attr("data-name", "action")
      .append("<option value=\"none\">Actions...</option>")
      .on("change", function () { //And when the user picks an action...
        swapResourceOptions($(this)); //Display the appropriate combination of other DDLs, and the save button
      });
    if (userGUID != "" && (libraryData.isMyLibrary || libraryData.hasEditAccess)) {
      ddlActions
        .append("<option value=\"move\">Move...</option>")
        .append("<option value=\"copy\">Copy To...</option")
        .append("<option value=\"delete\">Delete From...</option>")
    }
    else if(userGUID != "") {
      ddlActions
        .append("<option value=\"copy\">Copy To...</option")
    }

    //Append appropriate DDLs
    if (userGUID != "" && ( libraryData.isMyLibrary || libraryData.hasEditAccess )) {
      box.append(ddlActions)
        .append(ddlSourceCollections)
        .append(ddlTargetLibraries)
        .append(ddlTargetCollections)
        .append(saveButton);
    }
    else if(userGUID != "") {
      box.append(ddlActions)
        .append(ddlTargetLibraries)
        .append(ddlTargetCollections)
        .append(saveButton);
    }

    //Trigger the default state of the DDLs
    ddlActions.trigger("change");

    //Append library/collection ID to the URL - for stat tracking
    var appendString = "?libId=" + libraryData.library.id + (active.isLibrary ? "" : "&colId=" + active.data.id);
    var url = item.find(".data h2 a");
    var url2 = item.find(".thumbnailLink");
    url.attr("href", url.attr("href") + appendString);
    url2.attr("href", url2.attr("href") + appendString);
  });
}

//Show or hide relevant filters
function renderFilters() {
  setTimeout(function () {
    $("#btnResetSearch").click();
  }, 250);
  var active = getActive();
  var filterLinks = $("#categories input");
  filterLinks.hide();
  for (i in active.data.filters) {
    var current = active.data.filters[i]; //current set of filters

    //Show or hide the top level filter link
    if (current.ids.length > 0) {
      filterLinks.filter("[data-filterid=" + current.id + "]").show();
    }

    //Show or hide items within the filter
    var tags = $(".tagList[data-filterid=" + current.id + "] label");
    tags.hide();
    for (j in current.ids) {
      tags.find("input[data-id=" + current.ids[j] + "]").parent().show();
    }
  }
}

function fillTargetCollections(ddl, list) {
  //Get the Div that contains the DDLs
  var box = ddl.parent();
  var targetDDL = box.find("select[data-name=targetCollections]");
      targetDDL.html("");
      targetDDL.append("<option value=\"0\">Target Collection...</option>");
  //For each of the libraries i have edit access to...
  for (i in libraryData.myLibraries) {
    var current = libraryData.myLibraries[i];
    if (current.id == parseInt(ddl.find("option:selected").attr("value"))) { //Find the selected library
      for(j in current.collections){ //For every collection in the selected library...
        var curCol = current.collections[j];
        var contains = false;
        for (k in list) { //Determine whether or not it contains this Resource
          if (list[k] == curCol.id) {
            contains = true;
            break;
          }
        }
        targetDDL //Append this Collection to the DDL
          .append(
            $("<option></option>")
            .attr("value", curCol.id)
            .attr("disabled", (contains ? "disabled" : false))
            .html(curCol.title + (contains ? " (Contains this Resource)" : ""))
          );
      }
    }
  }
}

function swapResourceOptions(box) {
  var parent = box.parent();
  var sourceCollections = parent.find("select[data-name=sourceCollections]");
  var targetLibraries = parent.find("select[data-name=targetLibraries]");
  var targetCollections = parent.find("select[data-name=targetCollections]");
  var saveButton = parent.find("input");
  switch (box.find("option:selected").attr("value")) {
    case "none": sourceCollections.hide(); targetLibraries.hide(); targetCollections.hide(); saveButton.hide(); break;
    case "move": sourceCollections.show(); targetLibraries.show().trigger("change"); targetCollections.show(); saveButton.show(); break;
    case "copy": sourceCollections.hide(); targetLibraries.show().trigger("change"); targetCollections.show(); saveButton.show(); break;
    case "delete": sourceCollections.show(); targetLibraries.hide(); targetCollections.hide(); saveButton.show(); break;
    default: sourceCollections.hide(); targetLibraries.hide(); targetCollections.hide(); saveButton.hide(); break;
  }
}

//Change which set of DDLs are shown for the currently-selected action
function swapResourceOptions2(box) { //box = the Actions DDL that just had its change event fired
  var parent = box.parent(); //Parent is the box that holds all of the DDLs for this Resource search result
  var from = parent.find("select[data-name=inCollections]"); //Collections in this Library that this Resource is in
  var to = parent.find("select[data-name=myCollections]"); //My Collections
  var btn = parent.find("input"); //Save button
  switch (box.find("option:selected").attr("value")) {
    case "none": from.hide(); to.hide(); btn.hide(); break; //No action? hide everything else
    case "move": from.show(); to.show(); btn.show(); break; //Move: show the from and to boxes, and save button
    case "copy": from.hide(); to.show(); btn.show(); break; //Copy: show the to box, and save button
    case "delete": from.show(); to.hide(); btn.show(); break; //Delete: show the from box, and save button
    default: from.hide(); to.hide(); btn.hide(); break; //Otherwise hide everything...though this should never need to be called.
  }
}

/*   ---   ---   Page Functions   ---   ---   */
//Show the relevant panel when the user clicks on a tab
function showPanel(target) { //target = the name of the panel
  $("#libTabBox a.tab").removeClass("selected");
  $("#libTabBox a.tab[data-id=" + target + "]").addClass("selected");
  $(".panel").hide().removeClass("selected");
  $("#" + target).show().addClass("selected");
  $("#libraryHeaderContent").attr("data-collapsed", "false");
  $("a[data-id=expandCollapse]").html("-");
  return false;
}
//Toggle collapsing the height of the panels
function expandCollapsePanels() {
  var box = $("#libraryHeaderContent");
  box.attr("data-collapsed", box.attr("data-collapsed") == "true" ? "false" : "true");
  $("a[data-id=expandCollapse]").html(box.attr("data-collapsed") == "true" ? "Expand" : "Collapse");
  return false;
}

//Pick the Library object
function pickLibrary() {
  pickLibCol(libraryData.library);
  $("#libColSelector a").removeClass("selected");
  $("#libColSelector #libraryAvatar").addClass("selected");
  $("#avatarFrame").contents().find(".collectionID").attr("value", "0").val("0");
}
//Pick a specific collection
function pickCollection(id) {
  $("#libColSelector a").removeClass("selected");
  $("#libColSelector a[data-collectionid=" + id + "]").addClass("selected");
  for(i in libraryData.collections){
    if(libraryData.collections[i].id == id){
      pickLibCol(libraryData.collections[i]);
      $("#avatarFrame").contents().find(".collectionID").attr("value", id).val(id);
    }
  }
}

//When the Library or a collection is picked, set everything to inactive, then find the chosen object and set it to active.
function pickLibCol(item) {
  //change the active item
  libraryData.library.isActive = false;
  for (i in libraryData.collections) { libraryData.collections[i].isActive = false; }
  item.isActive = true;
  //re-render all of the panels with the current object's data
  renderDetails();
  renderComments();
  renderSettings();
  renderFilters();
  renderShareFollow();

  libraryIDs = [];
  collectionIDs = [];
  var activeThing = getActive();
  if (activeThing.isLibrary) {
    libraryIDs = [activeThing.data.id];
  }
  else {
  	libraryIDs = [libraryID];
    collectionIDs = [activeThing.data.id];
  }
  search(searchStages.resetCountdown);
  //updateNarrowing(); //auto-search with the current object as a filter
  $("#searchBar").attr("placeholder", "Search " + item.title + "...");
  return false;
}

//Examine the Library/Collections data and find the currently active one, and return it. Also indicate whether the currently-active object is the Library or a Collection
function getActive() {
  var active = {};
  if (libraryData.library.isActive) {
    active.isLibrary = true;
    active.data = libraryData.library;
  }
  else {
    active.isLibrary = false;
    for (i in libraryData.collections) {
      if (libraryData.collections[i].isActive) {
        active.data = libraryData.collections[i];
      }
    }
  }
  return active;
}

//Restore the active object, or pick the Library--mostly relates to deleting a collection. Used by AJAX
function restoreActive(id, isLib) {
  if(isLib){ pickLibrary(); }
  else { pickCollection(id); }
  var active = getActive();
  if (active == undefined || active == null) { pickLibrary(); }
}

//Disable the button during an AJAX call
function lockButton(button) {
  if (button) {
    button.attr("disabled", "disabled").attr("data-value", button.attr("value")).attr("value", "...");
  }
}

//Reenable the button after the AJAX call has been processed and returned
function restoreButton(button) {
  if (button) {
    button.attr("value", button.attr("data-value")).attr("disabled", false);
  }
}

//Create a fake empty collection, set it as the active object, and add it to the list of collections if a fake one doesn't already exist there--this all helps ensure other code doesn't crash in the process of creating a new Collection.
function createNewCollection() {
  var fake = {
    avatarURL: "",
    description: "",
    id: -1,
    isActive: true,
    isDefaultCollection: false,
    paradata: { comments: [], dislikes: 0, following: 0, iDislikeThis: false, iLikeThis: false, likes: 0 },
    title: ""
  };
  var found = false;
  for(i in libraryData.collections){
    if(libraryData.collections[i].id == -1){
      libraryData.collections[i] = fake;
      pickLibCol(libraryData.collections[i]);
      found = true;
      break;
    }
  }
  if(!found){ 
    libraryData.collections.push(fake);
    pickLibCol(libraryData.collections[libraryData.collections.length - 1]);
  }
}

//Expand/Contract the Comments box
function showHideComments() {
  var box = $("#commentsBox");
  if (box.attr("data-collapsed") == "true") {
    box.css("height", ($("#comments").height() + 35) + "px");
    $("#btnShowHideComments").css("box-shadow", "0 0 0 0 #EEE");
    box.attr("data-collapsed", "false");
  }
  else {
    box.css("height", "115px");
    box.attr("data-collapsed", "true");
    $("#btnShowHideComments").css("box-shadow", "30px 0 30px 20px #EEE");
  }
}

function requestJoinLibrary() {
  var val = $("#txtIJoinBecause").val();
  if (val.trim().length < 20) {
    alert("Please include a request of meaningful length.");
    return;
  }
  
  libraryAjax("RequestJoin", { userGUID: userGUID, libraryID: libraryData.library.id, message: val }, successRequestJoin, $("#btnRequestJoin"));
}

function sendResultsExternal() {
  try {
    for (i in debugResults.hits.hits) {
      var current = debugResults.hits.hits[i]._source;
      var data = {
        version: current.versionID,
        id: current.intID,
        title: current.title,
        thumbImage: ($(".result[data-vid=" + current.versionID + "] .thumbnailLink img").length > 0 ? $(".result[data-vid=" + current.versionID + "] .thumbnailLink img").attr("src") : ""),
        thumbMessage: ($(".result[data-vid=" + current.versionID + "] .thumbnailDiv").length > 0 ? $(".result[data-vid=" + current.versionID + "] .thumbnailDiv").html() : "")
      };
      parent.addResource(data);
    }
  }
  catch (e) { }
}

//Update the widget share link
function updateWidgetLink() {
  var link = $("#txtWidgetConfig");
  var ids = "";
  $("#widgetConfigList input:checked").each(function () {
    ids += "," + $(this).attr("value");
  });
  if (ids.length > 1) {
    ids = ids.substring(1);
  }
  var hrefs = window.location.href.split("/");
  link.val("<iframe src=\"" + hrefs[0] + "//" + hrefs[2] + "/Widgets/Library?library=" + libraryData.library.id + "&collections=" + ids + "\"></iframe>");
}

/*   ---   ---   AJAX Functions   ---   ---   */
//Semi-generic method to perform AJAX calls
//method = the name of the method on the server
//data = a JSON object containing data relevant to the server method
//success = when the AJAX call returns data, pass it to this javascript method
//button = the button to disable/enable to prevent duplicate AJAX calls
function libraryAjax(method, data, success, button) { 
  var active = getActive();
  var activeID = active.data.id;
  var activeLib = active.isLibrary;
  lockButton(button);
  $.ajax({
    url: "/Services/ESLibrary2Service.asmx/" + method,
    async: true,
    success: function (msg) {
      try {
        success($.parseJSON(msg.d));
        restoreButton(button);
        restoreActive(activeID, activeLib);
      }
      catch (e) {
        success(msg.d);
        restoreButton(button);
        restoreActive(activeID, activeLib);
      }
    },
    type: "POST",
    data: JSON.stringify(data),
    dataType: "json",
    contentType: "application/json; charset=utf-8"
  });
}

function postComment() {
  var textStuff = readText($("#txtCommentInput"), 10, "Comment");
  if (textStuff.valid) {
    var active = getActive();
    var data = {
      userGUID: userGUID,
      libraryID: libraryData.library.id,
      collectionID: (active.isLibrary ? 0 : active.data.id),
      text: textStuff.text
    };
    libraryAjax("PostComment", data, successPostComment, $("#btnPostComment"));
  }
}

function addLike() {
  addLikeDislike(true);
}
function addDislike() {
  addLikeDislike(false);
}
function addLikeDislike(isLike) {
  var active = getActive();
  var data = { userGUID: userGUID, libraryID: libraryData.library.id, collectionID: (active.isLibrary ? 0 : active.data.id), isLike: isLike };
  libraryAjax("AddLikeDislike", data, successAddLikeDislike, $("#btnAddLike, #btnAddDislike"));
}

//Handle copy/move/delete operations on a Resource
function doResourceAction(intID) { //Find the relevant set of DDLs by finding the relevant search result by intID
  var box = $(".result.list[data-resourceID=" + intID + "]").find(".modBox_before");
  if (!libraryData.isMyLibrary && !libraryData.hasEditAccess) { //If it isn't my library or one I manage, then I only need to worry about Copying
    //Copy
    doCopy(box, intID);
  }
  else { //If it is my Library, first get the Action from the Actions DDL
    var action = box.find("select[data-name=action] option:selected").attr("value");
    switch (action) { //depending on which Action was chosen, call the appropriate method
      case "none": return; break;
      case "copy": doCopy(box, intID); break;
      case "move": doMove(box, intID); break;
      case "delete": doDelete(box, intID); break;
    }
  }
}
//Perform copy
function doCopy(box, intID) {
  var toCollection = parseInt(box.find("select[data-name=targetCollections] option:selected").attr("value"));
  if (toCollection == 0) { return; }
  var data = { userGUID: userGUID, libraryID: libraryData.library.id, toCollection: toCollection, intID: intID };
  libraryAjax("ActionCopy", data, successCopy, box.find("input"));
}
//Perform move
function doMove(box, intID) {
  var fromCollection = parseInt(box.find("select[data-name=sourceCollections] option:selected").attr("value"));
  var toCollection = parseInt(box.find("select[data-name=targetCollections] option:selected").attr("value"));
  if (fromCollection == 0 || toCollection == 0) { return; }
  var data = { userGUID: userGUID, libraryID: libraryData.library.id, fromCollection: fromCollection, toCollection: toCollection, intID: intID };
  libraryAjax("ActionMove", data, successMove, box.find("input"));
}
//Perform delete
function doDelete(box, intID) {
  var fromCollection = parseInt(box.find("select[data-name=sourceCollections] option:selected").attr("value"));
  if (fromCollection == 0) { return; }
  var data = { userGUID: userGUID, libraryID: libraryData.library.id, fromCollection: fromCollection, intID: intID };
  libraryAjax("ActionDelete", data, successDelete, box.find("input"));
}

//Perform an update to Following Options for the currently-selected object (Library or a Collection)
function updateFollowingOption() {
  var active = getActive();
  var selection = parseInt($("#followingOptions option:selected").attr("value"));
  var data = {
    userGUID: userGUID,
    libraryID: libraryData.library.id,
    isLibrary: active.isLibrary,
    collectionID: (active.isLibrary ? 0 : active.data.id),
    typeID: selection
  };
  libraryAjax("UpdateFollowingOption", data, successUpdateFollowingOption, $("#btnFollowingUpdate"));
}

//Perform a save operation, reading from the form inputs on the Settings tab
function saveSettings() {
  var active = getActive(); //Get the active object
  var objTitle = readText($("#pnlSettings #txtTitle"), 3, "Title");
  var objDescription = readText($("#pnlSettings #txtDescription"), 5, "Description");
  var settings = { //Create the settings object
    userGUID: userGUID,
    collectionID: (active.isLibrary ? 0 : active.data.id),
    title: objTitle.text,
    description: objDescription.text,
    isLibrary: active.isLibrary,
    publicAccessLevel: parseInt($(".ddlPublicAccessLevels option:selected").val()),
    organizationAccessLevel: parseInt($(".ddlOrganizationAccessLevels option:selected").val()),
    makeDefault: $("#pnlSettings #makeDefault").prop("checked"),
    libraryID: libraryData.library.id
  }
  var toggleFile = ($("#pnlSettings #avatarFrame").contents().find("#fileUpload").val() != ""); //Determine whether or not to trigger the file upload
  if (active.data.id == -1 && !toggleFile) { //Attempting to create a new collection without selecting an avatar
    alert("You must select an icon for the collection you are trying to create.");
    return false;
  }

  if (objTitle.valid && objDescription.valid) { //If there's no error text, continue
    libraryAjax((active.data.id == -1 ? "CreateCollection" : "UpdateLibCol"), settings, successUpdateLibCol, $("#btnSaveSettings")); //If the current object is a fake object (id == -1), use the Create method--otherwise, use the Update method. Everything else is the same regardless.
  }
}

//Reload the avatars, from server if needed
function refreshAvatars(didCreate) {
  if (didCreate) {
    libraryAjax("GetAllLibraryInfo", { userGUID: userGUID, libraryID: libraryData.library.id }, successRenderAvatars);
  }
  else {
    successRenderAvatars(libraryData);
  }
}

function deleteCollection() {
  var active = getActive();
  if (active.isLibrary) {
    return;
  }

  if (active.data.isDefaultCollection) {
    alert("You cannot delete your default Collection!");
    return;
  }

  if (confirm("Are you sure you want to delete " + active.data.title + "? This action cannot be undone.")) {
    libraryAjax("DeleteCollection", { userGUID: userGUID, libraryID: libraryData.library.id, collectionID: active.data.id }, successDeleteCollection, $("#btnDeleteCollection"));
  }

}

//Simple text validation and error messaging
function readText(jInput, minLength, name) {
  var text = $(jInput).val().replace(/</g, "&lt;").replace(/>/g, "&gt;");
  var returner = { text: text, valid: true };
  if (text.length < minLength) {
    alert("Please enter a " + name + " of meaningful length.");
    $(jInput).css("border-color", "#F00");
    returner.valid = false;
    returner.text = "";
    return returner;
  }
  $(jInput).css("border-color", "#CCC");
  return returner;
}


/*   ---   ---   AJAX Success Functions   ---   ---   */
//Semi-generic method to take the libraryData object returned by AJAX and replace the on-page libraryData object with it...
function refreshData(data) {
  if (data == "") { //...but only if it's not empty.
    alert("There was an error processing your request. Please try again later.");
    return false;
  }
  else if(typeof(data) == "string") {
    alert(data);
    return false;
  }
  libraryData = data; //Replace the object
  renderLibColInfo(libraryData); //Re-render everything
  return true;
}
//All of these methods use the pattern of: "if the data is refreshed successfully, alert the user. Otherwise, do nothing (the error is alerted by the refresh method)"
function successUpdateFollowingOption(data) {
  refreshData(data) ? alert("Your changes have been saved.") : {};
}

function successCopy(data) {
  refreshData(data) ? alert("Resource Copied Successfully.") : {};
}

function successMove(data) {
  refreshData(data) ? alert("Resource Moved.") : {};
}

function successDelete(data) {
  pickLibrary();
  refreshData(data) ? alert("Resource Deleted from Collection.") : {};
  pickLibrary();
}

function successDeleteCollection(data) {
  pickLibrary();
  refreshData(data) ? alert("Collection Deleted from Library.") : {};
  pickLibrary();
}

function successPostComment(data) {
  if (typeof (data) != "object") {
    alert(data);
    return;
  }
  refreshData(data);
  $("#txtCommentInput").val("");
}

function successUpdateLibCol(data) {
  var active = getActive();
  var toggleFile = ($("#pnlSettings #avatarFrame").contents().find("#fileUpload").val() != ""); //Determine whether or not to trigger the file upload

  if (toggleFile) { //If there's a need to toggle the avatar upload box...
    var didCreate = active.data.id == -1;
    var targetID = (active.isLibrary ? 0 : active.data.id);

    if (didCreate) {
      for (i in data.collections) { //Determine and select the newly-created collection
        if (data.collections[i].id > targetID) {
          targetID = data.collections[i].id;
        }
      }
    }

    refreshData(data) ? alert("Update Successful. If you updated the icon, it may take up to ten seconds to appear.") : {};
    pickCollection(targetID);

    $("#avatarFrame").contents().find(".collectionID").attr("value", targetID).val(targetID);
    $("#pnlSettings #avatarFrame").contents().find("input[type=submit]").click(); //Reach into the iframe and click its submit button

    setTimeout(function () { refreshAvatars(didCreate); }, 10000);
  }
  else {
    refreshData(data) ? alert("Update Successful.") : {};
  }
}

function successRenderAvatars(data) {
  libraryData.library.avatarURL = data.library.avatarURL;
  //Set the values
  for (i in libraryData.collections) {
    for(j in data.collections){
      if (libraryData.collections[i].id == data.collections[j].id) {
        libraryData.collections[i].avatarURL = data.collections[j].avatarURL;
      }
    }
  }
  //Update the HTML
  $("#libColSelector a").each(function () {
    var item = $(this);
    if (item.attr("id") == "libraryAvatar") {
      item.css("background-image", "url('" + libraryData.library.avatarURL + "?rand=" + (Math.random() * 1000) + "')");
    }
    else {
      var thisID = parseInt(item.attr("data-collectionID"));
      for (i in libraryData.collections) {
        if (thisID == libraryData.collections[i].id) {
          //item.css("background-image", "url('" + libraryData.collections[i].avatarURL + "?rand=" + (Math.random() * 1000) + "')");
          item.find("img").attr("src", libraryData.collections[i].avatarURL + "?rand=" + (Math.random() * 1000));
        }
      }
    }
  });
}

function successAddLikeDislike(data) {
  refreshData(data);
}

function successRequestJoin(data) {
  if (data.isValid) {
    alert(data.data);
    $("#joinInput").html("<p class=\"middle\">" + data.data + "</p>");
  }
  else {
    alert(data.status);
    if (data.extra) {
      $("#joinInput").html("<p class=\"middle\">" + data.status + "</p>");
    }
    else {
      $("#btnRequestJoin").removeAttr("disabled");
    }
  }
}