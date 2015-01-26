var mode = "view";
var doingStandardRating = false;
var timeRequiredMinutesValues = [0, 1, 5, 10, 15, 30, 45];
var moreLikeThisAttempts = 0;
var validUpdate = false;
var userIsAdmin = userIsAdmin.toLowerCase() == "true";
var userIsAuthor = userIsAuthor.toLowerCase() == "true";
var thumbDivTypes = [
    //{ match: ".docXX", header: ".doc File", text: "Microsoft Word Document" },
    //{ match: ".ppt", header: ".ppt File", text: "Microsoft PowerPoint" },
    //{ match: ".xls", header: ".xls File", text: "Microsoft Excel Spreadsheet" },
    //{ match: ".docx", header: ".docx File", text: "Microsoft Word Document" },
    //{ match: ".pptx", header: ".pptx File", text: "Microsoft PowerPoint" },
    //{ match: ".xlsx", header: ".xlsx File", text: "Microsoft Excel Spreadsheet" },
    //{ match: ".pdf", header: ".pdf File", text: "Adobe PDF" },
    //{ match: ".swfXX", header: ".swf File", text: "Adobe Shockwave File" }
    //{ match: ".zip", header: "Archive File", file: "/images/icons/icon_zip_400x300.png" }
];
var thumbIconTypes = [
    //{ match: ".pdf", header: "Adobe PDF", file: "/images/icons/filethumbs/filethumb_pdf_400x300.png" },
    /*{ match: ".ppt", header: "Microsoft PowerPoint", file: "/images/icons/filethumbs/filethumb_pptx_400x300.png" },
    { match: ".pptx", header: "Microsoft PowerPoint", file: "/images/icons/filethumbs/filethumb_pptx_400x300.png" },
    { match: ".doc", header: "Microsoft Word Document", file: "/images/icons/filethumbs/filethumb_docx_400x300.png" },
    { match: ".docx", header: "Microsoft Word Document", file: "/images/icons/filethumbs/filethumb_docx_400x300.png" },
    { match: ".xls", header: "Microsoft Excel Spreadsheet", file: "/images/icons/filethumbs/filethumb_xlsx_400x300.png" },
    { match: ".xlsx", header: "Microsoft Excel Spreadsheet", file: "/images/icons/filethumbs/filethumb_xlsx_400x300.png" },*/
    //{ match: ".swf", header: "Adobe Shockwave File", file: "/images/icons/filethumbs/filethumb_swf_400x400.png" },
    { match: ".zip", header: "Archive File", file: "/images/icons/icon_zip_400x300.png" },
    { match: ".rar", header: "Archive File", file: "/images/icons/icon_zip_400x300.png" },
    { match: ".7z", header: "Archive File", file: "/images/icons/icon_zip_400x300.png" },
    { match: ".tar", header: "Archive File", file: "/images/icons/icon_zip_400x300.png" }
];


/* ---   ---   Initialization   ---   --- */
$(document).ready(function () {
    //handle Resizing
    $(window).on("resize", function () { resizeStuff(); }).trigger("resize");
    initTabBox();
    refreshData(resource);
    if (typeof (parent.hasSelector) != "function") {
      $("#btnSendResource").remove();
    }
    if (window == window.top) {
      $("#btnMsgResource").remove();
    }
});

/* ---   ---   Data Management   ---   --- */
//Load data via Ajax
function loadResourceData() {
  console.log("Reloading Resource Data");
    doAjax("LoadAllResourceData", { vid: resourceVersionID }, refreshData);
}

//Send Resource to external frame
function sendResource() {
  var data = {
    version: resource.versionID,
    id: resource.intID,
    title: resource.title,
    thumbImage: ($("#thumbnail img").length > 0 ? $("#thumbnail img").attr("src") : ""),
    thumbMessage: ($(".thumbnailDiv").length > 0 ? $(".thumbnailDiv").html() : "")
  };
  parent.addResource(data);
}

//Send Resource ID using postmessage
function sendResourceMsg() {
  parent.postMessage(resource.intID, "*");
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
    renderEvaluations();
    //renderRubrics();
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
    $("#usageRights .view").attr("src", resource.usageRights.usageRightsIconURL != "" ? resource.usageRights.usageRightsIconURL : "http://mirrors.creativecommons.org/presskit/cc.primary.srr.gif");
    $("#pickUsageRights.edit select option").attr("selected", "false");
    $("#pickUsageRights.edit select option[value=" + resource.usageRights.usageRightsValue + "]").attr("selected", "selected");
    $("#pickUsageRights.edit .txtConditionsOfUse").val(resource.usageRightsURL);

    //Resource URL
    $("#resourceURL, #thumbnail").attr("href", resource.url);
    $("#resourceURL, #thumbnail").on("click", function () {
      doAjax("AddClickThrough", { userGUID: userGUID, versionID: resourceVersionID }, updateClickThroughs);
    });
    $("#resourceURL").html((resource.url.indexOf("/ContentDocs") == 0 ? "http://ioer.ilsharedlearning.org" + resource.url : resource.url));
    if (resource.url == "#")
        $("#resource.url").css("display", "none");

    //Thumbnail
    if (resource.IsPrivateDocument == false) {

        $("#thumbnail img").attr("src", "//ioer.ilsharedlearning.org/OERThumbs/large/" + resource.intID + "-large.png");
        for (i in thumbIconTypes) {
            console.log(resource.url.indexOf(thumbIconTypes[i].match) > -1);
            console.log(thumbIconTypes[i].file);
            if (resource.url.indexOf(thumbIconTypes[i].match) > -1) {
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
    } else {
        hideThumbnail();
    }
    //Click-Throughs
    $("#clickthroughs").html("Visited " + resource.paradata.clickThroughs + " Times");

    //Requirements
    $("#requires").attr("data-hasContent", (resource.requires.length > 0 ? "true" : "false"));

    //Critical Info
    $("#creator span").html(resource.creator);
    $("#publisher span").html('<a href="/search?q=' + encodeURIComponent( resource.publisher ) + '" target="_blank">' + resource.publisher + '</a>');
    $("#submitter span").html(resource.submitter);
    userIsAdmin ? {} : $("#submitter").hide();
    $("#created span").html(resource.created);

    //LR Doc ID
    userIsAdmin ? $("#lrDocLink").html("<a href=\"http://node01.public.learningregistry.net/harvest/getrecord?request_ID=" + resource.lrDocID + "&by_doc_ID=true\" target=\"_blank\">View LR Document</a>") : {};

}
function renderViewTabCounts() {
    //Tab counts
    var rubricsRatingsCount = 0;
    /*for (i in resource.rubrics) {
        rubricsRatingsCount += resource.rubrics[i].ratingCount;
    }*/
    for (i in resource.evaluations) {
      rubricsRatingsCount += resource.evaluations[i].HasCertificationTotal + resource.evaluations[i].NonCertificationTotal;
    }
    $(".tabNavigator a[data-id=comments]").html(resource.comments.length);
    $(".tabNavigator a[data-id=tags]").html($("#tags div[data-hasContent=true]").length);
    $(".tabNavigator a[data-id=keyword]").html(resource.keyword.items.length);
    $(".tabNavigator a[data-id=subject]").html(resource.subject.items.length);
    $(".tabNavigator a[data-id=moreLikeThis]").html($("#moreLikeThis .resourceLikeThis").length);
    $(".tabNavigator a[data-id=alignedStandards]").html(resource.standards.length);
    $(".tabNavigator a[data-id=standardsRatings]").html(resource.standards.length);
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
                .replace(/{img}/g, (current.avatarURL == "" ? "src=\"/images/ioer_med.png\"" : "src=\"" + current.avatarURL + "\""))
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
//Evaluations
function renderEvaluations() {

  var box = $("#rubrics #rubricsData");
  var rubricTemplate = $("#template_evaluation_rubric").html();
  var dimensionTemplate = $("#template_evaluation_dimension").html();
  var barsTemplate = $("#template_evaluationBars").html();

  box.html("");
  if (resource.evaluations.length == 0) {
    box.append("<p class=\"pleaseLogin\">This Resource has not been evaluated yet.</p>");
  }
  if (userGUID != "") {
    if (resource.userAlreadyEvaluated) {
      //box.append("<p class=\"pleaseLogin\">You already evaluated this Resource.</p>");
    }
    else if (resource.userCanEvaluate) {
      box.append("<p class=\"pleaseLogin\"><a href=\"/Rubrics/?resourceIntID=" + resource.intID + "&resourceURL=" + resource.url + "\">Evaluate this Resource</a></p>");
    }
    else {
      box.append("<p class=\"pleaseLogin\">Currently only trained users can evaluate Resources.<br /><a href=\"http://oerevaluationteam.weebly.com/\" target=\"_blank\">Learn more about evaluator training</a></p>");
    }
  }
  else {
    box.append("<p class=\"pleaseLogin\">Please login to Evaluate.</p>");
  }

  for (i in resource.evaluations) { //For each rubric...
    var rubric = resource.evaluations[i];
    //Determine whether the user can evaluate with this rubric
    var evalText = "";
    if (rubric.RequiresCertification) {
      evalText = "This Rubric requires <a href=\"#\" target=\"_blank\">certification</a> to use."; //Link to certification
    }
    else {
      evalText = "You are not able to use this Evaluation."; //Shouldn't happen
    }
    if (rubric.UserHasCompletedEvaluation) { //Overwrite any above text if the user has already completed the evaluation
      evalText = "You have already completed this Evaluation with this Resource.";
    }

    //Fill out each dimension..
    var dimensionsText = "";
    for (j in rubric.Dimensions) {
      var dimension = rubric.Dimensions[j];
      dimension.width = 'style="width:' + dimension.percent + ';"';
      dimensionsText += dimensionTemplate
        .replace(/{title}/g, dimension.DimensionTitle)
        .replace(/{ratings}/g, fillOutEvaluationBars(dimension))
    }
    //Append the HTML
    box.append(rubricTemplate
      .replace(/{title}/g, rubric.EvaluationTitle + " Rubric")
      .replace(/{overallRatings}/g, fillOutEvaluationBars(rubric))
      .replace(/{dimensions}/g, dimensionsText)
      .replace(/{evalText}/g, evalText)
      .replace(/{requiresCert}/g, rubric.RequiresCertification)
    );
  }
}
function fillOutEvaluationBars(data) {
  var template = $("#template_evaluationBars").html();
  return template
    .replace(/{trainedRatingsCount}/g, (data.CertifiedAverageScore == -1 ? "N/A" : data.CertifiedAverageScore + "%") + " - " + data.HasCertificationTotal + " Certified Rating" + (data.HasCertificationTotal != 1 ? "s" : ""))
    .replace(/0px/g, ((data.CertifiedAverageScore == -1 | data.HasCertificationTotal == 0) ? 100 : data.CertifiedAverageScore) + '%')
    .replace(/{untrainedRatingsCount}/g, (data.NonCertifiedAverageScore == -1 ? "N/A" : data.NonCertifiedAverageScore + "%") + " - " + data.NonCertificationTotal + " User Rating" + (data.NonCertificationTotal != 1 ? "s" : ""))
    .replace(/9999px/g, ((data.NonCertifiedAverageScore == -1 | data.NonCertificationTotal == 0) ? 100 : data.NonCertifiedAverageScore) + '%')
    .replace(/{noTrainedRatings}/g, data.HasCertificationTotal == 0 ? "noRatings" : "")
    .replace(/{noUntrainedRatings}/g, data.NonCertificationTotal == 0 ? "noRatings" : "")
    .replace(/{trainedNotApplicable}/g, data.CertifiedAverageScore == -1 ? "notApplicable" : "")
    .replace(/{untrainedNotApplicable}/g, data.NonCertifiedAverageScore == -1 ? "notApplicable" : "");
}

//Standards
function renderStandards() {
  var box = $("#standardsRatings");
  var listBox = $("#alignedStandards .view");
  var itemTemplate = $("#template_standard_ratings").html();
  var listTemplate = $("#template_listedStandard").html();
  var ddlTemplate = $("#template_doRating").html();
  box.html("<h2 class=\"title\">Aligned Standards</h2>");
  listBox.html("");
  var totalPercentRatings = 0;
  var totalPrePercentRating = 0;
  var totalRatings = 0;

  //Messages
  if (resource.standards.length == 0) {
    box.append("<p class=\"pleaseLogin\">This Resource has not been aligned to any Standards yet.</p>");
    listBox.html("<p class=\"pleaseLogin\">This Resource has not been aligned to any Standards yet.</p>");
    return;
  }

  box.append(
    "<div class=\"lightbox\"><h3>Overall</h3>" +
    $("#template_evaluation_bar").html().replace(/{id}/g, "overallStandards")
    + "</div>"
  );

  for (i in resource.standards) {
    var current = resource.standards[i];
    var percent = current.AverageRating == -1 ? "N/A" : current.AverageRating + "%";
    totalRatings += current.TotalRatings;
    if (percent != "N/A") { 
      totalPercentRatings++; 
      totalPrePercentRating += current.AverageRating; 
    }

    //Ratings
    var ddlText = "";
    if(current.HasUserRated){ ddlText = "<p class=\"pleaseLogin message\">You already rated this Standard's alignment.</p>"; }
    else if (userGUID == "") { ddlText = "<p class=\"pleaseLogin message\">Please login to rate.</p>"; }
    else { ddlText = ddlTemplate.replace(/{standardID}/g, current.StandardId); }
    var code = (current.NotationCode == null || current.NotationCode.length == 0) ? current.Description.substr(0, 30) + "..." : current.NotationCode;
    box.append(
      itemTemplate
        .replace(/{title}/g, code)
        .replace(/{standardID}/g, current.StandardId)
        .replace(/{description}/g, current.Description)
        .replace(/99px/g, percent)
        .replace(/{ratingsCount}/g, current.TotalRatings)
        .replace(/{noRatings}/g, current.TotalRatings == 0 ? "noRatings" : "")
        .replace(/{notApplicable}/g, current.AverageRating == -1 ? "notApplicable" : "")
        .replace(/{doRating}/g, ddlText)
    );

    //Listing
    listBox.append(
      listTemplate
        .replace(/{alignment}/g, current.AlignmentType)
        .replace(/{title}/g, code)
        .replace(/{description}/g, current.Description)
        .replace(/{standardID}/g, current.StandardId)
    );

  }

  //Overall
  var overallPercent = 0;
  if (totalPercentRatings == 0) {
    overallPercent = "N/A";
    $("#overallStandards .evaluationBarFill").addClass("noRatings");
  }
  else {
    overallPercent = Math.round(totalPrePercentRating / totalPercentRatings) + "%";
  }
  $("#overallStandards .evaluationBarText").html(overallPercent + " - " + totalRatings + " Ratings");
  if (overallPercent == "N/A") {
    $("#overallStandards .evaluationBarFill").addClass("notApplicable");
  }
  else {
    $("#overallStandards .evaluationBarFill").css("width", overallPercent);
  }
  
}
function renderStandardsOld(hide) {
  //return; //temporary
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
        var data = { intID: resourceID, text: resource.title, parameters: attempts[moreLikeThisAttempts].fields, minFieldMatches: attempts[moreLikeThisAttempts].min };
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

function doStandardRating(standardID) {
  var rating = parseInt($("select#standardRating_" + standardID + " option:selected").attr("value"));
  if (rating == -1) { return; }
  if (confirm("Are you sure you want to rate this Standard?")) {
    doAjax("DoStandardRating", { userGUID: userGUID, standardID: standardID, rating: rating, intID: resourceID }, successDoRating);
  }
}
function doStandardRatingOld(standardID, rating) {
    if (userGUID == "") { return; }
    doAjax("DoStandardRating", { userGUID: userGUID, standardID: standardID, rating: rating, versionID: resourceVersionID }, updateStandards);
}

function addToCollection() {
    $("#myLibrary input").attr("disabled", "disabled").attr("value", "...");
    $("#submissionMessage").html("");
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
    $("#submissionMessage").html(data.message);
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
                    .replace(/{rid}/g, current.intID)
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
   // $("#description .edit textarea").width($("#description").width() - (width > 580 ? 435 : 20));
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
    $(".tabNavigator a[data-id=tags], .tabNavigator a[data-id=library]").trigger("click");
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
//private document
//not hard to figure out naming conventions - force create of a blank or a secure page??
function hideThumbnail() {
    $("#thumbnail img").replaceWith("<div class=\"thumbnailDiv\">Sorry, this is a private document</div>");
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
        k12subject: readMVF(".cbxl#k12subject"),
        usageRights: readUsageRights("#pickUsageRights"),
        timeRequired: readTimeRequired("#timeRequired .edit"),
        accessRights: readDDL("#accessRights .edit select"),
        language: readDDL("#language .edit select"),
        itemType: readDDL("#itemType .edit select"),
        subject: readFreeText(".addedFreeText[data-id=subject]", "Subject"),
        keyword: readFreeText(".addedFreeText[data-id=keyword]", "Keyword"),
        accessibilityControl: readDDL("#accessibilityControl .edit select"),
        accessibilityFeature: readDDL("#accessibilityFeature .edit select"),
        accessibilityHazard: readDDL("#accessibilityHazard .edit select"),
        standards: readStandardsV7()
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

function readStandardsV7() {
  var standards = [];
  $("#SBselected .selectedStandard").each(function () {
    var item = $(this);
    standards.push({
      id: parseInt(item.attr("data-standardID")),
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
    if (confirm("Really deactivate this Resource?")) {
        doAjax("DeactivateResource", { userGUID: userGUID, versionID: resourceVersionID }, confirmDeactivated);
    }
}


//Report a problem
function reportIssue() {
    var report = readText($("#txtReportProblem"), 10, "Report");
    if (report.length == 0) { return false; }
    else {
        doAjax("ReportIssue", { issue: report, userGUID: userGUID, resourceID: resourceID }, confirmReportReceived);
    }
}
function confirmReportReceived() {
    alert("Your report has been received. Thank you.");
    $("#txtReportProblem").val("");
}


//Regenerate thumbnail
function regenerateThumbnail() {
  alert("Regenerating thumbnail. Results may take up to 30 seconds to show.");
  doAjax("RegenerateThumbnail", { userGUID: userGUID, resourceID: resourceID, url: resource.url }, confirmRegenerateThumbnail);
}
function confirmRegenerateThumbnail(data) {
  if (data.isValid) {
    setTimeout(function () {
      $("#thumbnail img").attr("src", $("#thumbnail img").attr("src") + "?rand=" + Math.random());
    }, 10000);
  }
  else {
    alert(data.status);
  }
}

//Rating
function successDoRating(data) {
  if (data.isValid) {
    resource.standards = data.data;
    renderStandards();
  }
  else {
    alert(data.status);
  }
}

//Standards
function expandCollapseStandard(id, button) {
  $(".ratedStandard[data-standardID=" + id + "]").toggleClass("expanded");
  $(button).attr("value", $(button).attr("value") == "+" ? "-" : "+");
}