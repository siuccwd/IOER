var mode = "view";
var doingStandardRating = false;
var timeRequiredMinutesValues = [0, 1, 5, 10, 15, 30, 45];
var moreLikeThisAttempts = 0;
var validUpdate = false;
var userIsAdmin = userIsAdmin.toLowerCase() == "true";
var thumbDivTypes = [
    { match: ".doc", header: ".doc File", text: "Microsoft Word Document" },
    { match: ".ppt", header: ".ppt File", text: "Microsoft PowerPoint" },
    { match: ".xls", header: ".xls File", text: "Microsoft Excel Spreadsheet" },
    { match: ".docx", header: ".docx File", text: "Microsoft Word Document" },
    { match: ".pptx", header: ".pptx File", text: "Microsoft PowerPoint" },
    { match: ".xlsx", header: ".xlsx File", text: "Microsoft Excel Spreadsheet" },
    //{ match: ".pdf", header: ".pdf File", text: "Adobe PDF" },
    { match: ".swf", header: ".swf File", text: "Adobe Shockwave File" }
];
var thumbIconTypes = [
    { match: ".pdf", header: "Adobe PDF", file: "/images/icons/filethumbs/filethumb_pdf_400x300.png" },
];


/* ---   ---   Initialization   ---   --- */
$(document).ready(function () {
    //handle Resizing
    $(window).on("resize", function () { resizeStuff(); }).trigger("resize");
    initTabBox();
    refreshData(resource);
});

/* ---   ---   Data Management   ---   --- */
//Load data via Ajax
function loadResourceData() {
  console.log("Reloading Resource Data");
    doAjax("LoadAllResourceData", { vid: resourceVersionID }, refreshData);
}

/* ---   ---   Data Rendering   ---   --- */
function refreshData(data) {
    resource = data;
    renderResource();
}
function renderResource() {
    //Update view fields
    renderViewTextFields();
    renderViewMetadataTags();
    renderViewSpecialData();
    renderViewTabCounts();
    renderViewComments();
    renderViewSubjectsKeywords();
    renderLikesDislikes();
    renderBadges();
    renderMyLibrary();
    renderRubrics();
    renderStandards(true);
    prefillEditFields();
    renderButtonBox();
    renderReportIssueBox();

    //Update edit fields
    renderEditTags();

    //toggle visibility
    toggleShowHide();

    //more like this
    moreLikeThisAttempts = 0;
    renderMoreLikeThis();
}
//Text fields
function renderViewTextFields() {
    $(".view").each(function () {
        var targetID = $(this).parent().attr("id");
        if (targetID != undefined) {
            $(this).html(resource[targetID]);
        }
    });
}
//Checkboxes and DDLs share a view type
function renderViewMetadataTags() {
    $(".cbxl, .ddl").each(function () {
        var box = $(this).find(".view");
        var targetID = $(this).attr("id");
        var jTarget = resource[targetID];
        var output = "";
        $(this).html("");
        for (i in jTarget.items) {
            var current = jTarget.items[i];
            if (current.selected) {
                output += ", " + current.title;
            }
        }
        $(this).append($("#template_CBXL").html()
            .replace(/{title}/g, jTarget.printTitle)
            .replace(/{list}/g, (output.length > 0 ? output.substr(2) : ""))
        );
        if (output.length > 0) {
            $(this).attr("data-hasContent", "true");
        }
        else {
            $(this).attr("data-hasContent", "false");
        }
    });
}
//Special metadata
function renderViewSpecialData() {
    //Time required
    if (resource.timeRequired == "0 Days 0 Hours 0 Minutes" || resource.timeRequired == "Unknown" || resource.timeRequired == null || resource.timeRequired == "") {
        $("#timeRequired").attr("data-hasContent", "false");
    }
    else { $("#timeRequired").attr("data-hasContent", "true"); }
    //Fill out the DDLs
    $("#timeRequiredHours").html("");
    for (var i = 0; i < 24; i++) {
        $("#timeRequiredHours").append($("<option></option>").attr("value", i).html(i));
    }
    $("#timeRequiredMinutes").html("");
    for (i in timeRequiredMinutesValues) {
        $("#timeRequiredMinutes").append($("<option></option>").attr("value", timeRequiredMinutesValues[i]).html(timeRequiredMinutesValues[i]));
    }
    //Attempt to parse a properly-formatted string
    var splitTime = resource.timeRequired.toLowerCase().split(" ");
    if (splitTime.length == 6) {
        try {
            $("#timeRequiredDays").val(splitTime[0]);
            $("#timeRequiredHours option").attr("selected", false);
            $("#timeRequiredHours option[value=" + splitTime[2] + "]").attr("selected", "selected");
            $("#timeRequiredMinutes option").attr("selected", false);
            $("#timeRequiredMinutes option[value=" + splitTime[4] + "]").attr("selected", "selected");
        }
        catch (e) { }
    }

    //Usage Rights
    $("#usageRights .view").attr("src", resource.usageRightsIconURL ? resource.usageRightsIconURL : "http://mirrors.creativecommons.org/presskit/cc.primary.srr.gif");
    $("#pickUsageRights.edit select option").attr("selected", "false");
    $("#pickUsageRights.edit select option[value=" + resource.usageRights.usageRightsValue + "]").attr("selected", "selected");
    $("#pickUsageRights.edit .txtConditionsOfUse").val(resource.usageRightsURL);

    //Resource URL
    $("#resourceURL, #thumbnail").attr("href", resource.url);
    $("#resourceURL, #thumbnail").on("click", function () {
      doAjax("AddClickThrough", { userGUID: userGUID, versionID: resourceVersionID }, updateClickThroughs);
    });
    $("#resourceURL").html((resource.url.indexOf("/ContentDocs") == 0 ? "http://ioer.ilsharedlearning.org" + resource.url : resource.url));

    //Thumbnail
    $("#thumbnail img").attr("src", "//ioer.ilsharedlearning.org/OERThumbs/large/" + resource.intID + "-large.png");
    for (i in thumbIconTypes) {
      console.log(resource.url.indexOf(thumbIconTypes[i].match) > -1);
      console.log(thumbIconTypes[i].file);
      if(resource.url.indexOf(thumbIconTypes[i].match) > -1){
        $("#thumbnail img").attr("src", thumbIconTypes[i].file);
      }
    }
    $("#thumbnail img").on("error", function () {
      $(this).replaceWith(
          $("<div></div>")
              .attr("id", "mainThumbDiv")
              .addClass("thumbnailDiv")
              .html("Generating Thumbnail...")
      );
      setTimeout(function () {
        doAjax("GetThumbnail", { intID: resource.intID, url: resource.url }, successGetThumbnail);
      }, 15000);
    });
    for (i in thumbDivTypes) {
        if (resource.url.indexOf(thumbDivTypes[i].match) > -1) {
            $("#thumbnail img").replaceWith(
                $("<div></div>")
                    .addClass("thumbnailDiv")
                    .html(thumbDivTypes[i].text)
            );
        }
    }

    //Click-Throughs
    $("#clickthroughs").html("Visited " + resource.paradata.clickThroughs + " Times");

    //Requirements
    $("#requires").attr("data-hasContent", (resource.requires.length > 0 ? "true" : "false"));

    //Critical Info
    $("#creator span").html(resource.creator);
    $("#publisher span").html(resource.publisher);
    $("#submitter span").html(resource.submitter);
    userIsAdmin ? {} : $("#submitter").hide();
    $("#created span").html(resource.created);

    //LR Doc ID
    userIsAdmin ? $("#lrDocLink").html("<a href=\"http://node01.public.learningregistry.net/harvest/getrecord?request_ID=" + resource.lrDocID + "&by_doc_ID=true\" target=\"_blank\">View LR Document</a>") : {};

}
function renderViewTabCounts() {
    //Tab counts
    var standardsRatingsCount = 0;
    for (i in resource.standards) {
        standardsRatingsCount += resource.standards[i].ratingCount;
    }
    var rubricsRatingsCount = 0;
    for (i in resource.rubrics) {
        rubricsRatingsCount += resource.rubrics[i].ratingCount;
    }
    $(".tabNavigator a[data-id=comments]").html(resource.comments.length);
    $(".tabNavigator a[data-id=tags]").html($("#tags div[data-hasContent=true]").length);
    $(".tabNavigator a[data-id=keyword]").html(resource.keyword.items.length);
    $(".tabNavigator a[data-id=subject]").html(resource.subject.items.length);
    $(".tabNavigator a[data-id=moreLikeThis]").html($("#moreLikeThis .resourceLikeThis").length);
    $(".tabNavigator a[data-id=alignedStandards]").html(resource.standards.length);
    $(".tabNavigator a[data-id=standardsRatings]").html(standardsRatingsCount);
    $(".tabNavigator a[data-id=rubrics]").html(rubricsRatingsCount);
    $(".tabNavigator a[data-id=likedislike]").html(resource.paradata.likes + resource.paradata.dislikes);
    $(".tabNavigator a[data-id=library]").html(resource.libColInfo.libraries.length);

}
//Comments
function renderViewComments() {
    $(".tab#comments .view").html("");
    for (i in resource.comments) {
        var current = resource.comments[i];
        $(".tab#comments .view").append(
            $("#template_comment").html()
                .replace(/{name}/g, current.name)
                .replace(/{date}/g, current.commentDate)
                .replace(/{text}/g, current.commentText)
        );
    }
    if (resource.comments.length == 0) {
        $(".tab#comments .view").html("<p class=\"pleaseLogin\">" + (userGUID == "" ? "Please login to be" : "Be") + " the first to comment!</p>");
    }
}
//Subjects and Keywords
function renderViewSubjectsKeywords() {
    //subjects
    var subjectList = $("#subject .view");
    var keywordList = $("#keyword .view");
    var listTemplate = $("#template_subjectkeyword").html();
    subjectList.html(resource.subject.items.length > 0 ? "" : "<p class=\"pleaseLogin\">No subjects yet.</p>");
    for (i in resource.subject.items) {
        subjectList.append(
            listTemplate.replace(/{text}/g, resource.subject.items[i])
        );
    }
    $("#suggestedSubjects").html("");
    for (i in resource.subject.recommended) {
        $("#suggestedSubjects").append(
            $("#template_suggestedsubject").html().replace(/{text}/g, resource.subject.recommended[i])
        );
    }
    //keywords
    keywordList.html(resource.keyword.items.length > 0 ? "" : "<p class=\"pleaseLogin\">No keywords yet.</p>");
    for (i in resource.keyword.items) {
        keywordList.append(
            listTemplate.replace(/{text}/g, resource.keyword.items[i])
        );
    }
}
//Likes and Dislikes
function renderLikesDislikes() {
    var box = $("#likedislike");
    //info stuff
    box.find("#text .spanlikes").html(resource.paradata.likes);
    box.find("#text .spandislikes").html(resource.paradata.dislikes);
    if (resource.paradata.iLikeThis) {
        box.find(".myopinion").show().html("You like this Resource.");
        box.find("input").hide();
    }
    else if (resource.paradata.iDislikeThis) {
        box.find(".myopinion").show().html("You dislike this Resource.");
        box.find("input").hide();
    }
    else if (userGUID == "") {
        box.find(".myopinion").show().html("<p class=\"pleaseLogin\">Please login to Like or Dislike this Resource.</p>");
        box.find("input").hide();
    }
    else {
        box.find("input").show();
        box.find(".myopinion").hide();
    }
    //bar stuff
    var total = resource.paradata.likes + resource.paradata.dislikes;
    if (total == 0) {
        var likesWidth = .5;
        var dislikesWidth = .5;
    }
    else {
        var likesWidth = resource.paradata.likes / total;
        var dislikesWidth = resource.paradata.dislikes / total;
    }
    box.find("#likebar").css("width", (likesWidth * 100) + "%");
    box.find("#dislikebar").css("width", (dislikesWidth * 100) + "%");
    if (resource.paradata.dislikes == 0 && resource.paradata.likes > 0) {
        $("#likebar").css("border-radius", "5px");
    }
    else if (resource.paradata.likes == 0 && resource.paradata.dislikes > 0) {
        $("#dislikebar").css("border-radius", "5px");
    }
    else {
        $("#likebar").css("border-radius", "5px 0 0 5px");
        $("#dislikebar").css("border-radius", "0 5px 5px 0");
    }

    $("#btnAddLike").on("click", function () { addLike(); });
    $("#btnAddDislike").on("click", function () { addDislike(); });
}
//Badges
function renderBadges() {
    var badgeList = $("#badges");
    var badgeTemplate = $("#template_badge").html();
    badgeList.html(resource.libColInfo.libraries.length > 0 ? "<h3>This Resource appears in the following Librar" + (resource.libColInfo.libraries.length > 1 ? "ies" : "y") + ":</h3>" : "");
    for (i in resource.libColInfo.libraries) {
        var current = resource.libColInfo.libraries[i];
        badgeList.append(
            badgeTemplate
                .replace(/{img}/g, (current.avatarURL == "" ? "src=\"/images/isle.png\"" : "src=\"" + current.avatarURL + "\""))
                .replace(/{id}/g, current.id)
                .replace(/{title}/g, current.title)
        );
    }
    if (resource.libColInfo.libraries.length == 0) {
        $("#badges").html("<p class=\"pleaseLogin\">This Resource does not appear in any Libraries yet.</p>");
    }
}
//My Library
function renderMyLibrary() {
  var box = $("#myLibrary");
  if (userGUID == "") {
    box.html("<p class=\"pleaseLogin\">Please login to add this Resource to your Library.</p>");
  }
  else if (resource.libColInfo.myLibraries.length == 0) {
    $("#myCollections").replaceWith("<p class=\"pleaseLogin\">This account is not associated with a Library.</p>");
    $("#myLibrary input, #myLibraries").remove();
  }
  else {
    //Render Libraries DDL
    $("#myLibraries").html("");
    $("#myLibraries").append($("<option></option>").attr("value", "0").html("Select a Library..."));
    for (i in resource.libColInfo.myLibraries) {
      var current = resource.libColInfo.myLibraries[i];
      $("#myLibraries").append(
        $("<option></option>")
          .attr("value", current.id)
          .attr("disabled", current.isInLibrary ? "disabled" : false )
        .html(current.title + (current.isInLibrary ? " (Already in this Library)" : ""))
      );
    }
    $("#myLibraries").unbind().on("change", function () {
      renderMyCollections();
    }).trigger("change");
  }
}
function renderMyCollections() {
  $("#myCollections").html("");
  var selectedLibrary = parseInt($("#myLibraries option:selected").attr("value"));
  if (selectedLibrary == 0) {
    $("#myCollections, #myLibrary input").hide();
    return;
  }
  else {
    $("#myCollections, #myLibrary input").show();
  }
  for (i in resource.libColInfo.myLibraries) {
    var current = resource.libColInfo.myLibraries[i];
    if (current.id == selectedLibrary) {
      for (j in current.collections) {
        $("#myCollections").append(
          $("<option></option>")
            .attr("value", current.collections[j].id)
            .attr("selected", current.collections[j].id == current.defaultCollectionID)
            .html(current.collections[j].title)
        );
      }
    }
  }
}
//Rubrics
function renderRubrics() {
    var box = $("#rubrics");
    var template = $("#template_rubricScoreometer").html();
    box.html("<h2>CCSS Rubric Evaluations</h2>");
    if (resource.rubrics.length == 0) {
        box.append("<p class=\"pleaseLogin\">This Resource has not been evaluated yet. Note that this Resource must align to at least one Standard in order to be evaluated.</p>");
        box.append("<p class=\"pleaseLogin\"><b>Please Note: The Rubric tool is currently under construction and is not currently available.</b></p>");
        return;
    }
    for (i in resource.rubrics) {
        var current = resource.rubrics[i];
        var percent = getPercent(current.communityRating);
        box.append(
            template
                .replace(/{title}/g, current.description)
                .replace(/{count}/g, current.ratingCount)
                .replace(/{plural}/g, current.ratingCount == 1 ? "" : "s")
                .replace(/{score}/g, percent.text)
                .replace(/{rawscore}/g, percent.rawPercent)
        );
        box.find(".scoreometer").last().find(".bar").css("width", percent.text);
    }
    $("#rubrics .scoreometer").sort(sortScores).appendTo("#rubrics");
    box.append("<p class=\"pleaseLogin\"><b>Please Note: The Rubric tool is currently under construction and is not currently available.</b></p>");
}
//Standards
function renderStandards(hide) {
    var box = $("#standardsRatings");
    var listBox = $("#alignedStandards .view");
    var template = $("#template_standardScoreometer").html();
    box.html("<h2 class=\"title\">Aligned Standards</h2>");
    if (resource.standards.length == 0) {
        box.append("<p class=\"pleaseLogin\">This Resource has not been aligned to any Standards yet.</p>");
        listBox.html("<p class=\"pleaseLogin\">This Resource has not been aligned to any Standards yet.</p>");
        return;
    }
    listBox.html("");
    for (i in resource.standards) {
        //Ratings
        var current = resource.standards[i];
        var percent = current.ratingCount > 0 ? getPercent(current.communityRating) : getPercent(3);
        box.append(
            template
                .replace(/{title}/g, (current.code.length == 0 ? current.description.substr(50) + "..." : current.code))
                .replace(/{alignment}/g, current.alignmentType)
                .replace(/{count}/g, current.ratingCount)
                .replace(/{plural}/g, current.ratingCount == 1 ? "" : "s")
                .replace(/{score}/g, percent.text)
                .replace(/{rawscore}/g, current.ratingCount == 0 ? (doingStandardRating ? 101 : -1) : percent.rawPercent)
                .replace(/{description}/g, current.description)
                .replace(/{standardID}/g, current.id)
        );
        var target = box.find(".scoreometer").last();
        target.find(".bar").css("width", percent.text);
        if (current.ratingCount == 0) {
            target.addClass("notrated");
            target.find(".bar").addClass("notrated");
            target.find(".percent").html("Not Rated");
        }
        if (current.myRating > -1) {
            var tester = current.myRating;
            var myTextRating = tester == 0 ? "Weak" : tester == 1 ? "Limited" : tester == 2 ? "Strong" : tester == 3 ? "Superior" : "";
            target.find(".myScore").html("<b>My Rating: " + myTextRating + "</b>");
        }

        //Listing
        listBox.append(
            $("#template_listedStandard").html()
                .replace(/{alignment}/g, current.alignmentType)
                .replace(/{title}/g, (current.code.length == 0 ? current.description.substr(50) + "..." : current.code))
                .replace(/{description}/g, current.description)
                .replace(/{standardID}/g, current.id)
        );
    }
    $("#standardsRatings .scoreometer").sort(sortScores).appendTo("#standardsRatings");
    box.find("h2.title").append(
        $("<input />")
            .attr("type", "button")
            .attr("value", "More/Less")
            .attr("class", "btn blue tiny").on("click", function () {
                //Rearrange
                if (doingStandardRating) {
                    doingStandardRating = false;
                    box.find(".scoreometer.notrated").appendTo("#standardsRatings");
                }
                else {
                    doingStandardRating = true;
                    box.find(".scoreometer.notrated").insertAfter("#standardsRatings h2.title");
                }
                //Animate
                $("#standardsRatings .description").slideToggle();
                if (userGUID == "") {
                    $("#standardsRatings .myScore").slideToggle();
                }
            })
    );
    if (hide) { $("#standardsRatings .description").hide(); }
    if (userGUID == "") {
        $("#standardsRatings .myScore").hide().html("<i>Please login to rate Standards.</i>");
        return;
    }
    /*$("#standardsRatings .myScore a").on("click", function () {
        $(this).parent().find("a").attr("data-selected", "false");
        $(this).attr("data-selected", true);
        return false;
    });*/
    $("#standardsRatings .myScore input").on("click", function () {
        /*var parent = $(this).parent();
        var selected = parent.find("a[data-selected=true]");
        if (selected.length == 0) { return; }
        $(this).attr("value", "...").attr("disabled", "disabled");
        doStandardRating(parseInt(parent.attr("data-standard")), parseInt(selected.attr("value")));*/
        var parent = $(this).parent();
        var selected = parent.find("select option:selected");
        if (selected.attr("value") == "0") { return; }
        $(this).attr("value", "...").attr("disabled", "disabled");
        doStandardRating(parseInt(parent.attr("data-standard")), parseInt(selected.attr("value")));
    });

}
function getPercent(score) {
    var raw = (score / 3);
    var rawPercent = Math.round(raw * 100);
    return {
        raw: raw,
        rawPercent: rawPercent,
        text: rawPercent + "%"
    };
}
function sortScores(a, b) {
    var compA = parseInt($(a).attr("data-score"));
    var compB = parseInt($(b).attr("data-score"));
    return compA > compB ? -1 : 1;
}

//Prefilling Edit fields
function prefillEditFields() {
    $("#title .edit input[type=text]").val(resource.title);
    $("#description .edit textarea").html(resource.description);
}

//Resource update buttons
function renderButtonBox() {
    if ($("#modifyThis input").length == 0) {
        $("#modifyThis p").addClass("pleaseLogin").html("Please login to modify this Resource.");
    }
}

//Report issue
function renderReportIssueBox() {
  if (userGUID == "") {
    $("#report p").addClass("pleaseLogin").html("Please login to report an issue.");
  }
}

//Edit tags
function renderEditTags() {
    $(".tab#tags .cbxl").each(function () {
        var box = $(this).find(".edit");
        var itemID = $(this).attr("id");
        box.html("");
        for (i in resource[itemID].items) {
            var current = resource[itemID].items[i];
            box.append(
                $("<a></a>")
                    .attr("href", "#")
                    .attr("data-id", current.id)
                    .attr("data-preselected", current.selected)
                    .attr("data-selected", current.selected)
                    .on("click", function () {
                        if ($(this).attr("data-preselected") == "true") { return false; }
                        $(this).attr("data-selected", $(this).attr("data-selected") == "true" ? "false" : "true");
                        return false;
                    }).html(current.title)
            );
        }
    });
    $(".tab#tags .ddl").each(function () {
        var box = $(this).find(".edit");
        var itemID = $(this).attr("id");
        box.html("");
        box.append("<select></select>").attr("data-name", itemID);
        box.find("select").append(
            $("<option></option>")
                .attr("data-id", "0")
                .attr("selected", false)
                .attr("value", "0").html("Select...")
        );
        for (i in resource[itemID].items) {
            var current = resource[itemID].items[i];
            box.find("select")
                .append(
                    $("<option></option>")
                        .attr("data-id", current.id)
                        .attr("selected", current.selected)
                        .attr("value", current.id)
                        .html(current.title)
                );
        }
    });
    $("#subject input[type=text]").unbind().on("keyup", function (event) {
        if (event.which == 13) {
            addFreeText($("#subject input[type=text]").val(), "subject");
        }
    });
    $("#keyword input[type=text]").unbind().on("keyup", function (event) {
        if (event.which == 13) {
            addFreeText($("#keyword input[type=text]").val(), "keyword");
        }
    });
}


//More Like This
function renderMoreLikeThis() {
    $("#moreLikeThis .resourcesLikeThis").html("");
    var attempts = [
        { fields: "standardNotations", min: getPartialLength(resource.standards.length, 0.5) },
        { fields: "title,description", min: getPartialLength(resource.title.split(" ").length, 0.5) },
        { fields: "gradeLevels,subjects", min: getPartialLength(resource.gradeLevel.length + resource.subject.length, 0.7) },
        { fields: "gradeLevels,keywords", min: getPartialLength(resource.gradeLevel.length + resource.keyword.length, 0.7) },
        { fields: "creator,publisher", min: 1 }
    ]
    try {
        var data = { versionID: resourceVersionID, parameters: attempts[moreLikeThisAttempts].fields, minFieldMatches: attempts[moreLikeThisAttempts].min };
        console.log(data);
        doAjax("FindMoreLikeThis", data, updateMoreLikeThis);
        moreLikeThisAttempts++;
    }
    catch (e) {
        $("#moreLikeThis .resourcesLikeThis").append("<p class=\"pleaseLogin\">Sorry, no similar Resources found.</p>");
    }
}
function getPartialLength(value, percentage) {
    try {
        var toReturn = Math.floor(value * percentage);
        if (isNaN(toReturn)) {
            return 2;
        }
        else {
            return toReturn;
        }
    }
    catch (e) {
        return 2;
    }
}


/* ---   ---   Ajax Functions   ---   --- */
function doAjax(method, data, success) {
    $.ajax({
        url: "/Services/DetailService6.asmx/" + method,
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

function doStandardRating(standardID, rating) {
    if (userGUID == "") { return; }
    doAjax("DoStandardRating", { userGUID: userGUID, standardID: standardID, rating: rating, versionID: resourceVersionID }, updateStandards);
}

function addToCollection() {
  $("#myLibrary input").attr("disabled", "disabled").attr("value", "...");
  var targetLibraryID = parseInt($("select#myLibraries option:selected").attr("value"));
    var targetCollectionID = parseInt($("select#myCollections option:selected").attr("value"));
    doAjax("AddToCollection", { userGUID: userGUID, libraryID: targetLibraryID, collectionID: targetCollectionID, intID: resource.intID }, updateLibraryInfo);
}

function postComment() {
    if (userGUID == "") { return; }
    $("#comments input[type=button]").attr("disabled", "disabled").attr("value", "...");
    var text = $("#comments textarea").val().replace(/</g, "").replace(/>/g, "").replace(/"/g, "'");
    doAjax("PostComment", { userGUID: userGUID, text: text, versionID: resourceVersionID }, updateComments);
}

function addLike() {
    $("#btnAddLike, #btnAddDislike").attr("disabled", "disabled").fadeOut();
    doAjax("AddLike", { userGUID: userGUID, versionID: resourceVersionID }, updateLikesDislikes);
}

function addDislike() {
    $("#btnAddLike, #btnAddDislike").attr("disabled", "disabled").fadeOut();
    doAjax("AddDislike", { userGUID: userGUID, versionID: resourceVersionID }, updateLikesDislikes);

}


/* ---   ---   Ajax Success Functions   ---   --- */
function updateStandards(data) {
    resource.standards = data;
    renderStandards(false);
    renderViewTabCounts();
}

function updateLibraryInfo(data) {
    $("#myLibrary input").attr("disabled", false).attr("value", "Add");
    console.log(data);
    resource.libColInfo = data;
    renderMyLibrary();
    renderBadges();
    renderViewTabCounts();
}

function updateComments(data) {
    if (data.validComment) {
        resource.comments = data.comments;
        $("#comments textarea").val("");
        renderViewComments();
        renderViewTabCounts();
    }
    else {
        alert(data.status);
    }
    $("#comments input[type=button]").attr("disabled", false).attr("value", "Submit");
}

function updateMoreLikeThis(data) {
    if (typeof (data) == "string") { renderMoreLikeThis(); return; }
    if (data.hits.total == 0) {
        renderMoreLikeThis();
    }
    else {
        $(".resourcesLikeThis").html("");
        var counter = 0;
        for (i in data.hits.hits) {
            var current = data.hits.hits[i]._source;
            if (current.versionID == resourceVersionID || current.url == resource.url) { continue; }
            counter++;
            $(".resourcesLikeThis").append(
                $("#template_moreLikeThis").html()
                    .replace(/{vid}/g, current.versionID)
                    .replace(/{urlTitle}/g, current.title.replace(/ /g, "_").replace(/:/g, "").substring(0, 100))
                    .replace(/{title}/g, current.title)
                    .replace(/{description}/g, current.description)
                    .replace(/{intID}/g, current.intID)
            );
            for (i in thumbDivTypes) {
                if (current.url.indexOf(thumbDivTypes[i].match) > -1) {
                    $(".resourcesLikeThis").last().find("img").replaceWith(
                        $("<div></div>").addClass("thumbnailDiv").css({ "width": "150px", "height": "113px" }).html(thumbDivTypes[i].header)
                    );
                }
            }
        }
        $(".tabNavigator a[data-id=moreLikeThis]").html(counter);
    }
}

function updateLikesDislikes(data) {
    if (data) {
        resource.paradata = data;
        renderLikesDislikes();
        renderViewTabCounts();
    }
}

function updateClickThroughs(data) {
    console.log(data);
    resource.paradata.clickThroughs = parseInt(data);
    $("#clickthroughs").html("Visited " + resource.paradata.clickThroughs + " Times");
}

function updateResource(data) {
    console.log(data);
    $(".btn.finish").attr("value", "Save Changes").attr("disabled", false);
    resource = data;
    $(".tab .addedTextItem").remove();
    $("#selectedStandards .selectedStandard").remove();
    renderResource();
    mode = "view";
    toggleShowHide();
}

function confirmDeactivated(data) {
    console.log(data);
    alert(data);
    location.reload();
}

function successGetThumbnail(data) {
  if (data.isValid) {
    $("#mainThumbDiv").replaceWith(
      $("<img />").attr("src", "//ioer.ilsharedlearning.org/OERThumbs/large/" + resource.intID + "-large.png")
    );
  }
}

/* ---   ---   Page Functions   ---   --- */
//Resizing
function resizeStuff() {
    var width = $(window).width();
    $("#description .edit textarea").width($("#description").width() - (width > 580 ? 435 : 20));
}
//Tab Box
function initTabBox() {
    $(".tabNavigator a").on("click", function () {
        var parent = $(this).parent().parent();
        parent.find(".tab").attr("data-selected", "false");
        parent.find("#" + $(this).attr("data-id")).attr("data-selected", "true");
        parent.find(".tabNavigator a").attr("data-selected", "false");
        $(this).attr("data-selected", "true");
        $(window).trigger("resize");
        resizeStuff();
        return false;
    });
    $(".tabNavigator a[data-id=tags], .tabNavigator a[data-id=comments]").trigger("click");
}
//Keywords and Subjects
function addFreeText(text, field) {
  console.log("called");
  console.log(text);
    text = text.replace(/</g, "").replace(/>/g, "").replace(/\"/g, "").replace(/'/g, "");
    var box = $("#" + field);
    var isValid = true;
    var error = "";
    //Check existing
    for (i in resource[field].items) {
        if (resource[field].items[i].toLowerCase() == text.toLowerCase()) {
            isValid = false;
            error += " That word already exists in the list.";
        }
    }
    box.find(".addedFreeText .addedTextItem span").each(function () {
        if ($(this).html().toLowerCase() == text.toLowerCase()) {
            isValid = false;
            error += " You already added that word.";
        }
    });

    //Check length
    if (text.length < 3) {
        isValid = false;
        error += " Please add a " + field + " of meaningful length.";
    }

    //Add
    if (isValid) {
        box.find(".addedFreeText").append(
            $("#template_addedTextItem").html()
                .replace(/{text}/g, text)
        );
        //Clear
        box.find("input[type=text]").val("");
    }
    else {
        alert(error);
    }

    return false;
}
function removeAddedText(item) {
    $(item).parent().remove();
    return false;
}
//Thumbnail Fix
function fixThumbnail() {
    $("#thumbnail img").replaceWith("<div class=\"thumbnailDiv\">Sorry, Preview Unavailable</div>");
}

//Update the Resource Metadata
function sendUpdatedResource() {
    validUpdate = true;
    
    //construct the updated resource object
    var update = {
        title: readText("#title .edit input", 8, "Title"),
        description: readText("#description .edit textarea", 10, "Description"),
        requires: readText("#requires .edit input", 0, "Requirements"),
        gradeLevel: readMVF(".cbxl#gradeLevel"),
        careerCluster: readMVF(".cbxl#careerCluster"),
        endUser: readMVF(".cbxl#endUser"),
        groupType: readMVF(".cbxl#groupType"),
        resourceType: readMVF(".cbxl#resourceType"),
        mediaType: readMVF(".cbxl#mediaType"),
        educationalUse: readMVF(".cbxl#educationalUse"),
        usageRights: readUsageRights("#pickUsageRights"),
        timeRequired: readTimeRequired("#timeRequired .edit"),
        accessRights: readDDL("#accessRights .edit select"),
        language: readDDL("#language .edit select"),
        itemType: readDDL("#itemType .edit select"),
        subject: readFreeText(".addedFreeText[data-id=subject]", "Subject"),
        keyword: readFreeText(".addedFreeText[data-id=keyword]", "Keyword"),
        standards: readStandards("#selectedStandards")
    }

    console.log(update);
    //send the updated object
    if (validUpdate) {
      doAjax("UpdateResource", { versionID: resourceVersionID, userGUID: userGUID, update: update }, updateResource);
    }
}

function readText(jInput, minLength, name) {
    var text = $(jInput).val().replace(/</g, "&lt;").replace(/>/g, "&gt;");
    if (text.length < minLength) {
        alert("Please enter a " + name + " of meaningful length.");
        $(jInput).css("border-color", "#F00");
        validUpdate = false;
        return "";
    }
    $(jInput).css("border-color", "#CCC");
    return text;
}

function readMVF(selector) {
    var list = $(selector).find(".edit a[data-selected=true]").not("a[data-preselected=true]");
    var output = [];
    list.each(function () {
        output.push({
            id: parseInt($(this).attr("data-id")),
            value: $(this).html()
        });
    });
    return output;
}

function readDDL(ddl) {
    var item = $(ddl).find("option:selected");
    return { id: parseInt(item.attr("value")), value: item.html() };
}

function readUsageRights(selector) {
    if ($(selector).find("select option:selected").attr("value") == "4") {
        return readText(".txtConditionsOfUse", 0, "Conditions of Use");
    }
    else {
        return $(selector).find(".conditions_descriptor a").attr("href");
    }
}

function readTimeRequired(selector) {
    var jSelector = $(selector);
    var days = readText(jSelector.find("#timeRequiredDays"), 0, "Days Required");
    var hours = readDDL(jSelector.find("#timeRequiredHours")).id;
    var minutes = readDDL(jSelector.find("#timeRequiredMinutes")).id;
    var valid = "1234567890".split("");
    for (i in days) {
        if ($.inArray(days[i], valid) == -1) {
            alert("Days Required must be a number");
            return "P0D0H0M";
        }
    }
    if (days == "" && hours == "0" && minutes == "0") {
        return "P0D0H0M";
    }
    else {
        return "P" + days + "D" + hours + "H" + minutes + "M";
    }
}

function readFreeText(selector, name) {
    var list = [];
    $(selector).find(".addedTextItem span").each(function () {
        list.push($(this).html());
    });
    return list;
}

function readStandards(selector) {
    var standards = [];
    $(selector).find(".selectedStandard").each(function () {
        var item = $(this);
        standards.push({
            id: parseInt(item.attr("data-id")),
            code: item.attr("data-code"),
            alignment: readDDL(item.find("select"))
        });
    });
    return standards;
}

/* ---   ---   Page Buttons   ---   --- */
//Switch the page to update mode
function switchToUpdateMode() {
    mode = "edit";
    toggleShowHide();
    resizeStuff();
}

//Save updates
function saveUpdates() {
    $(".btn.finish").attr("value", "Working...").attr("disabled", "disabled");
    sendUpdatedResource();
}

//Cancel updates
function cancelChanges() {
    mode = "view";
    toggleShowHide();
    renderResource();
    resizeStuff();
}

function toggleShowHide() {
    if (mode == "edit") {
        $(".view").not(".view.admin").hide();
        $(".edit").not(".edit.admin").show();
        if (userIsAdmin) {
            $(".view.admin").hide();
            $(".edit.admin").show();
        }
        $("#tags .cbxl, #tags .ddl, #timeRequired").css("display", "inline-block").show();
        $("#alignedStandards .view, #keyword .view, #subject .view, #comments .view").show();
        $("#requires").show();

        if (userGUID == "") {
            $("#comments .edit").hide();
        }
        else {
            $("#comments .edit").show();
        }
    }
    else {
        $(".edit").hide();
        $(".view").show();
        $("#tags div[data-hascontent=false]").hide();
        $("#requires").attr("style", "");

        if (userGUID == "") {
            $("#comments .edit").hide();
        }
        else {
            $("#comments .edit").show();
        }
    }
    resizeStuff();

}

//Deactivate the Resource
function deactivate() {
    doAjax("DeactivateResource", { userGUID: userGUID, versionID: resourceVersionID }, confirmDeactivated);
}


//Report a problem
function reportIssue() {
    var report = readText($("#txtReportProblem"), 10, "Report");
    if (report.length == 0) { return false; }
    else {
        doAjax("ReportIssue", { issue: report, userGUID: userGUID, versionID: resourceVersionID }, confirmReportReceived);
    }
}
function confirmReportReceived() {
    alert("Your report has been received. Thank you.");
    $("#txtReportProblem").val("");
}