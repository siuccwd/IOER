/* ---   ---   ---   Variables   ---   ---   --- */
//Array index (not ID) of currently-previewed file
var currentPreviewID = 0;
//Indicates whether or not all standards are currently expanded
var showingAllStandards = false;
var prevNodeID = 0;
var nextNodeID = 0;

/* ---   ---   ---   Initialization   ---   ---   --- */
$(document).ready(function () {
  //Prevent clicking on a preview button from opening the resource
  $(".preview").on("click", function (e) {
    e.stopPropagation();
    return false;
  });
  $("html").not("#tools").not("#btnShowTools").not(".btnOpenMap").on("click", function () {
    hideTools();
  });
  $("#btnShowTools, #tools, .btnOpenMap").on("click", function (e) {
    e.stopPropagation();
  });
  //Enable minimizing parts of the curriculum map
  setupCurriculumMap();
  //Handle resize events
  $(window).on("resize", function () { handleResize(); });
  $(window).trigger("resize");
  //Calculate next/previous nodes
  setupSiblingNodes();
  //Render comments
  renderComments();
  //Setup widget links
  $(".widgetLink").on("click", function () { this.select(); });
  //Update following DDL
  getFollowing();
});

//Add next/previous functionality
function setupSiblingNodes() {
  var nodes = $("#tab_curriculumMap li a");
  nodes.each(function (index) {
    var id = $(this).attr("data-id");
    if (id == nodeID) {
      if (index < nodes.length - 1) {
        nextNodeID = $(nodes.get(index + 1)).attr("data-id");
      }
      return false;
    }
    prevNodeID = id;
  });
  $("#btnPrevNode").on("click", function () { goToSibling(prevNodeID); });
  $("#btnNextNode").on("click", function () { goToSibling(nextNodeID); });
  if (prevNodeID == 0) { $("#btnPrevNode").hide(); }
  if (nextNodeID == 0) { $("#btnNextNode").hide(); }
}

//Show/Hide parts of the curriculum map
function setupCurriculumMap() {
  var map = $("#tab_curriculumMap > ul.layer");
  var collapseBoxes = map.find("> .layer > .layer");
  collapseBoxes.each(function () {
    var box = $(this);
    var header = box.prev();
    box.addClass("collapseBox");
    header.prepend('<input type="button" value="-" class="isleButton bgBlue curriculumExpandCollapseButton" />');
    header.addClass("curriculumHeader");
    var button = header.find("input");
    header.find("input").on("click", function () {
      box.toggleClass("collapsed");
      if (box.hasClass("collapsed")) {
        box.slideUp();
        button.attr("value", "+");
      }
      else {
        box.slideDown();
        button.attr("value", "-");
      }
      triggerResize(500, function () { return $("#tab_curriculumMap").height() > $("body").height() });
    });
  });
}

function getFollowing() {
  doAjax("CurriculumService1", "GetSubscription", { nodeID: nodeID }, successGetFollowing, null, null);
}

/* ---   ---   ---   Page Functions   ---   ---   --- */
//Trigger resize event (for widget purposes)
function triggerResize(delay, useDocumentHeight) {
  if (isWidget) {
    if (delay) {
      setTimeout(function () { triggerResize(0, useDocumentHeight); }, delay);
      return;
    }
    if (useDocumentHeight) {
      $("body").css("height", $(document).height());
    }
    else {
      $("body").css("height", "auto");
    }
    $(window).trigger("resize");
  }
}

//Show tools overlay
function showTools() {
  $("#tools").fadeIn();
  triggerResize(260, true);
}
//Hide tools overlay
function hideTools() {
  $("#tools").fadeOut();
  triggerResize(260);
}
//Toggle tools overlay
function toggleTools() {
  if ($("#tools").is(":visible")) {
    hideTools();
  }
  else {
    showTools();
  }
}
//Show a tab within the tools overlay
function toggleTab(target) {
  $("#tools .tab").removeClass("showing");
  $("#" + target).toggleClass("showing");
  $("#tools #buttons input").removeClass("selected");
  $("#tools #buttons input[data-id=" + target + "]").addClass("selected");
  triggerResize();
}
//Preview a Resource
function preview(id, useGoogle) {
  $("body").addClass("previewing");
  $("#previewerOverlay").fadeIn(1000);
  for (i in files) {
    if (files[i].id == id) {
      currentPreviewID = i;
      if ((files[i].url.toLowerCase().indexOf(".pdf") > 0 || files[i].url.toLowerCase().indexOf(".htm") > 0) && $(window).width() > 500) {  //Attempt to force previewer on mobile for PDF
        $("#previewerFrame").attr("src", files[i].url);
        $("#googlePreviewerLink").show();
        $("#officePreviewerLink").hide();
      }
      else {
        var target = "";
        if (files[i].url.toLowerCase().indexOf("contentdocs") > -1) {
          if (useGoogle || $(window).width() <= 500 || files[i].url.toLowerCase().indexOf(".pdf") > 0) {
            target = "http://docs.google.com/viewer?embedded=true&url=" + encodeURI("http://ioer.ilsharedlearning.org" + files[i].url);
            $("#googlePreviewerLink").hide();
            $("#officePreviewerLink").show();
          }
          else {
            target = "http://view.officeapps.live.com/op/view.aspx?src=" + encodeURI("http://ioer.ilsharedlearning.org" + files[i].url);
            $("#googlePreviewerLink").show();
            $("#officePreviewerLink").hide();
          }
        }
        else {
          target = files[i].url;
        }
        if ($("#previewerFrame").attr("src") != target) {
          $("#previewerFrame").attr("src", target);
        }
      }
      $("#previewer #previewerTitle").attr("href", files[i].url).html(files[i].title);
      $("#previewer #previewerTracker").html((parseInt(currentPreviewID) + 1) + " of " + files.length);
    }
  }
  triggerResize(1100);
  //Make AJAX tracking call
  trackActivity(id);
}
//Alternate preview
function previewWithGoogle() {
  preview(files[currentPreviewID].id, true);
}
//Alternate preview
function previewWithOffice() {
  preview(files[currentPreviewID].id, false);
}
//Hide the preview overlay
function hidePreview() {
  $("body").removeClass("previewing");
  $("#previewerOverlay").fadeOut();
  triggerResize(260);
}
//Skip to next Resource in previewer
function previewerNext() {
  if (files.length - 1 == currentPreviewID) {
    currentPreviewID = 0;
  }
  else {
    currentPreviewID++;
  }
  if (files[currentPreviewID].url.length < 8) {
    previewerNext();
  }
  else {
    preview(files[currentPreviewID].id, false);
  }
}
//Skip to previous Resource in previewer
function previewerPrevious() {
  if (currentPreviewID == 0) {
    currentPreviewID = files.length - 1;
  }
  else {
    currentPreviewID--;
  }
  if (files[currentPreviewID].url.length < 8) {
    previewerPrevious();
  }
  else {
    preview(files[currentPreviewID].id, false);
  }
}
//Show a Standard in the standards area
function showStandard(id) {
  $(".standardText[data-standardID=" + id + "]").slideToggle();
  triggerResize();
}
//Show or hide all standards
function toggleAllStandards() {
  if (showingAllStandards) {
    $("#btnToggleAllStandards").attr("value", "Expand All");
    $(".standardText").slideUp();
    showingAllStandards = false;
    triggerResize(260);
  }
  else {
    $("#btnToggleAllStandards").attr("value", "Collapse All");
    $(".standardText").slideDown();
    showingAllStandards = true;
    triggerResize(500);
  }
}
//Show a level/type of standard
function showStandardsType(id) {
  $(".standardsLevel").removeClass("showing");
  $(".standardsLevel[data-levelID=" + id + "]").addClass("showing");
  triggerResize();
}

//Handle resizing
function handleResize() {
  var widthCategory = $("#cssDetector").css("content").replace(/'/g, "");
  if (!hasFeaturedItem) {
    if (widthCategory == "775") {
      $("#filesBox").addClass("scrolling").removeClass("grid");
    }
    else {
      $("#filesBox").removeClass("scrolling").addClass("grid");
    }
  }
}


//Expand or collapse all of the blocks in the curriculum map
function toggleCurriculumExpandCollapseAll(inputButton) {
  var button = $(inputButton);
  var boxes = $(".collapseBox");
  var exColButtons = $(".curriculumExpandCollapseButton");
  if (button.attr("data-collapsed") == "true") {
    button.attr("data-collapsed", "false");
    button.attr("value", "Collapse All");
    boxes.removeClass("collapsed");
    exColButtons.attr("value", "-");
    boxes.slideDown();
    triggerResize(500, true);
  }
  else {
    button.attr("data-collapsed", "true");
    button.attr("value", "Expand All");
    boxes.addClass("collapsed");
    exColButtons.attr("value", "+");
    boxes.slideUp();
    triggerResize(260);
  }
}

//Add a comment
function comment() {
  var text = $("#txtComment").val();
  if (text.length < 10) {
    alert("Please enter a comment of meaningful length.");
    return;
  }
  doAjax("CurriculumService1", "Comment", { text: text, nodeID: nodeID }, successComment, $("#btnComment"));
}

//Update subscription
function updateFollowing(id) {
  var option = parseInt($("#ddlTimelineSubscribe option:selected").attr("value"));
  doAjax("CurriculumService1", "UpdateSubscription", { nodeID: id, type: option }, successUpdateFollowing, $("#btnFollow"));
}

//Add a like
function like(id, button, type) {
  doAjax("CurriculumService1", "Like", { nodeID: id }, successAddLike, $(button), type);
}

//Track Activity
function trackActivity(contentID) {
  //Assemble AJAX stuff
  var data = { curriculumID: curriculumID, nodeID: nodeID, contentID: contentID };
  //Do AJAX call
  doAjax("ActivityService", "LearningList_DocumentHit", data, successTrackActivity, null, null);
}

/* ---   ---   ---   AJAX   ---   ---   --- */
function doAjax(service, method, data, success, button, extra) {
  if (button) { button.attr("disabled", "disabled"); }
  $.ajax({
    url: "/Services/" + service + ".asmx/" + method,
    contentType: "application/json; charset=utf-8",
    type: "POST",
    data: JSON.stringify(data),
    success: function (msg) { success((msg.d ? msg.d : msg), extra); }
  }).always(function () {
    if (button) { button.removeAttr("disabled"); }
  });
}

function successComment(data) {
  if (data.valid) {
    comments = data.data;
    $("#txtComment").val("");
    renderComments();
  }
  else {
    alert(data.status);
  }
}

function successUpdateFollowing(data) {
  if (data.valid) {
    alert("Your preference has been saved.");
  }
  else {
    alert(data.status);
  }
}

function successAddLike(data, type) {
  if (data.valid) {
    $("#likeCount_" + type + " span").html(data.data);
    $("#likeButton_" + type).replaceWith("<span class=\"grayMessage\">You like this.</span>");
  }
  else {
    alert(data.status);
  }
}

function successGetFollowing(data) {
  if (data.valid) {
    $("#ddlTimelineSubscribe option[value=" + data.type + "]").prop("selected", true);
  }
}

function successTrackActivity(data) {
  console.log(data);
}

/* ---   ---   ---   Rendering   ---   ---   --- */

function renderComments() {
  var template = $("#template_comment").html();
  var box = $("#commentsList");
  if (comments.length == 0) {
    box.html("<p class='grayMessage'>No comments found for this Node</p>");
    return;
  }
  else {
    box.html("");
    for (i in comments) {
      box.append(jsCommon.fillTemplate(template, comments[i]));
    }
  }
  triggerResize();
}