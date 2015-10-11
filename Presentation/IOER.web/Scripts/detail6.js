var mode = "view";
var doingStandardRating = false;
var timeRequiredMinutesValues = [0, 1, 5, 10, 15, 30, 45];
var moreLikeThisAttempts = 0;
var validUpdate = false;
var userIsAdmin = userIsAdmin.toLowerCase() == "true";
var userIsAuthor = userIsAuthor.toLowerCase() == "true";
//var mltQueries = [];
//var mltResults = [];
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
    { match: ".tar", header: "Archive File", file: "/images/icons/icon_zip_400x300.png" },
    { match: ".asx", header: "Audio Stream", file: "/images/icons/filethumbs/filethumb_swf_400x400.png" },
    { match: ".mp4", header: "Video File", file: "/images/icons/icon_video_400x300.png" },
    { match: ".avi", header: "Video File", file: "/images/icons/icon_video_400x300.png" },
    { match: ".mpg", header: "Video File", file: "/images/icons/icon_video_400x300.png" },
    { match: ".3gp", header: "Video File", file: "/images/icons/icon_video_400x300.png" },
    { match: ".flv", header: "Video File", file: "/images/icons/icon_video_400x300.png" },
    { match: ".mp3", header: "Audio File", file: "/images/icons/icon_audio_400x300.png" },
    { match: ".ogg", header: "Audio File", file: "/images/icons/icon_audio_400x300.png" },
    { match: ".ram", header: "Audio File", file: "/images/icons/icon_audio_400x300.png" },
    { match: ".wav", header: "Audio File", file: "/images/icons/icon_audio_400x300.png" },
    { match: ".wma", header: "Audio File", file: "/images/icons/icon_audio_400x300.png" },
    { match: ".jnlp", header: "Download File", file: "/images/icons/icon_download_400x300.png" },
    { match: ".tga", header: "Download File", file: "/images/icons/icon_download_400x300.png" },
    { match: ".tiff", header: "Download File", file: "/images/icons/icon_download_400x300.png" },
    { match: ".tif", header: "Download File", file: "/images/icons/icon_download_400x300.png" },
    { match: "view.d2l", header: "Download File", file: "/images/icons/icon_download_400x300.png" },
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
		//Auto show the rubrics tab if the user just did an evaluation
    if (window.location.href.indexOf("evaluationFinished=true") > -1) {
    	$(".tabNavigator a[data-id=rubrics]").trigger("click");
    }
    $(".tabNavigator a").on("mouseout", function () { $(this).blur(); });
});

/* ---   ---   Data Management   ---   --- */
//Load data via Ajax
function loadResourceData() {
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
              //output += ", " + current.title;
              output += ", " + "<a target=\"_blank\" href='/search?text=\"" + encodeURIComponent(current.title) + "\"'>" + current.title + "</a>";
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
    var rightsUnknown = resource.usageRights.usageRightsURL.toLowerCase() == "unknown";
    $("#usageRights .view").attr("src", resource.usageRights.usageRightsIconURL != "" && !rightsUnknown ? resource.usageRights.usageRightsIconURL : "/images/icons/rightsunknown.png");
    $("#usageRights #usageRightsUrl").attr("href", rightsUnknown ? "javascript:void(0)" : resource.usageRights.usageRightsURL);
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
            if (resource.url.indexOf(thumbIconTypes[i].match) > -1) {
                $("#thumbnail img").attr("src", thumbIconTypes[i].file);
            }
        }
        $("#thumbnail img").on("error", function () {
        	$(this).attr("src", "/images/icon_resources_400x300.png");
        	setTimeout(function () {
        		$("#thumbnail img").attr("src", "//ioer.ilsharedlearning.org/OERThumbs/large/" + resource.intID + "-large.png");
        	}, 30000);
            /*$(this).replaceWith(
                $("<div></div>")
                    .attr("id", "mainThumbDiv")
                    .addClass("thumbnailDiv")
                    .html("Generating Thumbnail...")
            );
            setTimeout(function () {
                doAjax("GetThumbnail", { intID: resource.intID, url: resource.url }, successGetThumbnail);
            }, 15000);*/
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
    $("#creator span").html("<a href='/search?text=creator:\"" + encodeURIComponent(resource.creator.replace(/\|/, "").replace(/  /, " ")) + "\"' target=\"_blank\">" + resource.creator + "</a>");
    $("#publisher span").html("<a href='/search?text=publisher:\"" + encodeURIComponent(resource.publisher.replace(/\|/, "").replace(/  /, " ")) + "\"' target=\"_blank\">" + resource.publisher + "</a>");
    $("#submitter span").html("<a href='/search?text=submitter:\"" + encodeURIComponent(resource.submitter.split("<")[0].replace(/\|/, "").replace(/  /, " ")) + "\"' target=\"_blank\">" + resource.submitter + "</a>");
    //userIsAdmin ? {} : $("#submitter").hide();
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
    setTabValue("comments", resource.comments.length);
    setTabValue("tags", $("#tags div[data-hasContent=true]").length);
    setTabValue("keyword", resource.keyword.items.length + resource.subject.items.length);
    setTabValue("alignedStandards", resource.standards.length);
    setTabValue("standardsRatings", resource.standards.length);
    setTabValue("rubrics", rubricsRatingsCount);
    setTabValue("library", resource.libColInfo.libraries.length);

	/*
    $(".tabNavigator a[data-id=comments]").html(resource.comments.length);
    $(".tabNavigator a[data-id=tags]").html($("#tags div[data-hasContent=true]").length);
    $(".tabNavigator a[data-id=keyword]").html(resource.keyword.items.length + resource.subject.items.length);
    //$(".tabNavigator a[data-id=moreLikeThis]").html($("#moreLikeThis .resourceLikeThis").length);
    $(".tabNavigator a[data-id=alignedStandards]").html(resource.standards.length);
    $(".tabNavigator a[data-id=standardsRatings]").html(resource.standards.length);
    $(".tabNavigator a[data-id=rubrics]").html(rubricsRatingsCount);
    $(".tabNavigator a[data-id=library]").html(resource.libColInfo.libraries.length);*/

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
    var printWords = resource.keyword.items.concat(resource.subject.items);
    for (var i in printWords) {
      if (printWords[i].length >= 3) {
        keywordList.append(
            listTemplate.replace(/{text}/g, printWords[i])
        );
      }
    }
	//Hashtag-ify keywords for search engines
    var hashtags = $("#hashtags");
    for (var i in printWords) {
    	var word = printWords[i].replace(/ /g, "").replace(/"/g, "").replace(/#/g, "").replace(/'/g, "").trim();
    	hashtags.append("#" + word + " ");
    }
}
//Likes and Dislikes
function renderLikesDislikes() {
    var box = $("#likedislike");
    //info stuff
    box.find("#text .spanlikes").html(resource.paradata.likes);
    box.find("#text .spandislikes").html(resource.paradata.dislikes);
    if (resource.paradata.iLikeThis) {
        box.find(".myopinion").show().html("You like this resource.");
        box.find("input").hide();
    }
    else if (resource.paradata.iDislikeThis) {
        box.find(".myopinion").show().html("You dislike this resource.");
        box.find("input").hide();
    }
    else if (userGUID == "") {
        box.find(".myopinion").show().html("<p class=\"pleaseLogin\">Please login to Like or Dislike this resource.</p>");
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
    badgeList.html(resource.libColInfo.libraries.length > 0 ? "<h3>This resource is in these public Librar" + (resource.libColInfo.libraries.length > 1 ? "ies" : "y") + ":</h3>" : "");
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
        $("#badges").html("<p class=\"pleaseLogin\">This resource does not appear in any Libraries yet.</p>");
    }
}
//My Library
function renderMyLibrary() {
  var box = $("#myLibrary");
  if (userGUID == "") {
    box.html("<p class=\"pleaseLogin\">Please login to add this resource to your Library.</p>");
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
    box.append("<p class=\"pleaseLogin\">This resource has not been evaluated yet.</p>");
  }
  if (userGUID != "") {
    /*if (resource.userAlreadyEvaluated) {
      //box.append("<p class=\"pleaseLogin\">You already evaluated this resource.</p>");
    }
    else if (resource.userCanEvaluate) {
    	//box.append("<p class=\"pleaseLogin\"><a href=\"/Rubrics/?resourceIntID=" + resource.intID + "&resourceURL=" + resource.url + "\">Evaluate this resource</a></p>");
    	box.append("<p class=\"pleaseLogin\"><a href=\"/evaluate/" + resource.intID + "\">Evaluate this resource</a></p>");
		}
    else {
      box.append("<p class=\"pleaseLogin\">Currently only trained users can evaluate resources.<br /><a href=\"http://oerevaluationteam.weebly.com/\" target=\"_blank\">Learn more about evaluator training</a></p>");
    }*/
		//Allow adding or replacing ratings regardless as long as the user is logged in
    box.append("<p class=\"pleaseLogin\"><a href=\"/evaluate/" + resource.intID + "\">Evaluate this resource</a></p>");
  }
  else {
    box.append("<p class=\"pleaseLogin\">Please login to Evaluate.</p>");
  }

  for (i in resource.evaluations) { //For each rubric...
    var rubric = resource.evaluations[i];
    //Determine whether the user can evaluate with this rubric
    var evalText = "";
    /*if (rubric.RequiresCertification) {
      evalText = "This Rubric requires <a href=\"#\" target=\"_blank\">certification</a> to use."; //Link to certification
    }
    else {
      evalText = "You are not able to use this Evaluation."; //Shouldn't happen
    }
    if (rubric.UserHasCompletedEvaluation) { //Overwrite any above text if the user has already completed the evaluation
      evalText = "You have already completed this Evaluation with this resource.";
    }*/

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
    box.append("<p class=\"pleaseLogin\">This resource has not been aligned to any Standards yet.</p>");
    listBox.html("<p class=\"pleaseLogin\">This resource has not been aligned to any Standards yet.</p>");
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
        box.append("<p class=\"pleaseLogin\">This resource has not been aligned to any standards yet.</p>");
        listBox.html("<p class=\"pleaseLogin\">This resource has not been aligned to any standards yet.</p>");
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
        $("#modifyThis p").addClass("pleaseLogin").html("Please login to modify this resource.");
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
/*function renderMoreLikeThisOLD() {
    $("#moreLikeThis .resourcesLikeThis").html("");
    var attempts = [
        { fields: "Title,Description", min: getPartialLength(resource.title.split(" ").length, 0.5) },
        { fields: "StandardNotations", min: getPartialLength(resource.standards.length, 0.5) },
        { fields: "Fields.Tags,GradeAliases,Keywords", min: getPartialLength(resource.gradeLevel.length + resource.keyword.length, 0.7) },
        { fields: "Creator,Publisher,Submitter", min: 1 }
    ]
    try {
        var data = { intID: resourceID, text: resource.title, parameters: attempts[moreLikeThisAttempts].fields, minFieldMatches: attempts[moreLikeThisAttempts].min };
        console.log(data);
        doAjax("FindMoreLikeThis", data, updateMoreLikeThis);
        moreLikeThisAttempts++;
    }
    catch (e) {
        $("#moreLikeThis .resourcesLikeThis").append("<p class=\"pleaseLogin\">Sorry, no similar resources found.</p>");
    }
}*/

function renderMoreLikeThis() {
	//Start with title and keywords
	var mltTestTerms = [resource.title, resource.creator, resource.publisher].concat(resource.keyword.items).concat(resource.subject.items);
	//Add metadata fields
	var targetFields = ["careerCluster", "educationalUse", "endUser", "gradeLevel", "groupType", "k12subject", "language", "mediaType", "resourceType"];
	for (var i in targetFields) {
		var items = resource[targetFields[i]].items;
		for (var j in items) {
			if (items[j].selected) {
				mltTestTerms.push(items[j].title);
			}
		}
	}
	//Add standards
	for (var i in resource.standards) {
		if (resource.standards[i].NotationCode != null && resource.standards[i].NotationCode.length > 0) {
			mltTestTerms.push(resource.standards[i].NotationCode);
		}
	}
	//Remove noise words
	var noiseWords = "a an the or and with for use by . , : ; & ! _ % $ # @ * ^".split(" ");
	for (var i in mltTestTerms) {
		for (var j in noiseWords) {
			var regex = new RegExp(" " + noiseWords[j] + " ", "g");
			mltTestTerms[i] = mltTestTerms[i].trim().toLowerCase().replace(regex, " ");
		}
	}
	var finalQueryString = mltTestTerms.join(" ");
	var data = { text: finalQueryString };
	console.log("Performing MLT Test with: ", data);
	doAjax("FindMoreLikeThis_MultiMatch", data, updateMoreLikeThis_multiMatch);
}

/*function renderMoreLikeThisOLD2() {
	//Make a query out of the title
	var noiseWords = "a an the or and with for use by . , : ; & ! _ % $ # @ * ^".split(" ");
	var inputTitle = (" " + resource.title + " ").toLowerCase();
	for (var i in noiseWords) {
		var regex = new RegExp(" " + noiseWords[i] + " ", "g");
		inputTitle = inputTitle.replace(regex, "");
	}
	inputTitle = inputTitle.trim();
	mltQueries.push(inputTitle);

	//Make more queries from keywords
	var sourceWords = [].concat(resource.keyword.items).concat(resource.subject.items);
	var searchWords = [];
	console.log("Source words", sourceWords);
	for (var i in sourceWords) {
		//If the word is not too short and not too long and not more than 2 words, add it
		if (sourceWords[i].length > 5 && sourceWords[i].length < 30 && sourceWords[i].split(" ").length < 3) {
			searchWords.push(sourceWords[i]);
		}
	}
	console.log("Search words", searchWords);
	var queryCounter = 0;
	var query = [];
	for (var i in searchWords) {
		if (queryCounter < 3) {
			query.push(searchWords[i]);
			queryCounter++;
		}
		else {
			console.log("query", query);
			mltQueries.push(query.join(" "));
			console.log("mlt queries", mltQueries);
			queryCounter = 0;
			query = [];
		}
	}
	if (query.length > 0) {
		mltQueries.push(query.join(" "));
	}
	console.log("MLT queries:", mltQueries);
	try {
		moreLikeThisAttempts = 0;
		var data = { intID: resourceID, text: mltQueries[moreLikeThisAttempts], parameters: "", minFieldMatches: 0 };
		doAjax("FindMoreLikeThis", data, updateMoreLikeThis);
		//test
		var search = window.location.search;
		if (!testDataSet && search.indexOf("mlt=true") > -1) {
			if (search.indexOf("type=cross_fields") > -1) { data.parameters = "cross_fields"; }
			if (search.indexOf("type=most_fields") > -1) { data.parameters = "most_fields"; }
			if (search.indexOf("type=best_fields") > -1) { data.parameters = "best_fields"; }
			//Need to get a full set of data without breaking the existing process
			//Start with title and keywords
			var mltTestTerms = [resource.title, resource.creator, resource.publisher].concat(resource.keyword.items).concat(resource.subject.items);
			//Add metadata fields
			var targetFields = ["careerCluster", "educationalUse", "endUser", "gradeLevel", "groupType", "k12subject", "language", "mediaType", "resourceType"];
			for (var i in targetFields) {
				var items = resource[targetFields[i]].items;
				for (var j in items) {
					if (items[j].selected) {
						mltTestTerms.push(items[j].title);
					}
				}
			}
			//Add standards
			for (var i in resource.standards) {
				if (resource.standards[i].NotationCode != null && resource.standards[i].NotationCode.length > 0) {
					mltTestTerms.push(resource.standards[i].NotationCode);
				}
			}
			//remove noise words
			for (var i in mltTestTerms) {
				for (var j in noiseWords) {
					var regex = new RegExp(" " + noiseWords[j] + " ", "g");
					mltTestTerms[i] = mltTestTerms[i].trim().toLowerCase().replace(regex, " ");
				}
			}
			var finalQueryString = mltTestTerms.join(" ");
			data.text = finalQueryString
			console.log("Performing MLT Test with: ", data);
			doAjax("FindMoreLikeThis_Test", data, updateMoreLikeThis_test);
		}
		moreLikeThisAttempts++;
	}
	catch (e) {
		box.append("<p class=\"pleaseLogin\">Sorry, no similar resources found.</p>");
	}
}*/

/*
function renderMoreLikeThisOLD() {
  var box = $("#moreLikeThis .resourcesLikeThis");
  box.html("");
  var junkWords = "for,of,and,or,by,with,to,me,the,is,',#,&".split(",");
  var cleanedTitle = " " + resource.title + " ";
  for (var i in junkWords) {
  	cleanedTitle = cleanedTitle.replace(new RegExp(" " + junkWords[i] + " ", "g"), "");
  }
  cleanedTitle = cleanedTitle.trim(); // Not used due to error in spacing
  var keywordTitle = "";
  var tempWords = [];
  var keysAndSubjects = resource.keyword.items.concat(resource.subject.items);
  for (var i = 0; i < keysAndSubjects.length && tempWords.length < 5; i++) {
  	var word = keysAndSubjects[i];
  	if (word.length > 3 && word.length < 25 && word.split(" ").length <= 2) {
  		tempWords.push(word);
  	}
  }
  keywordTitle = tempWords.join(" ");
  var useThisTitle = keywordTitle.length > 5 ? keywordTitle : resource.title;
  console.log("More Like This search with:", useThisTitle);
  var attempts = [
    { fields: "Title,Description,Keywords,Creator,Publisher,Submitter,StandardNotations", min: getPartialLength(cleanedTitle.split(" ").length + resource.keyword.items.length + resource.standards.length, 0.8) },
    { fields: "Title,Description,Keywords,Creator,Publisher,Submitter", min: getPartialLength(cleanedTitle.split(" ").length + resource.keyword.items.length, 0.8) },
    { fields: "Title,Description,Keywords", min: getPartialLength(cleanedTitle.split(" ").length + resource.keyword.items.length, 0.5) },
		{ fields: "Title,Description", min: getPartialLength(cleanedTitle.split(" ").length, 0.8) },
    { fields: "StandardNotations", min: getPartialLength(resource.standards.length, 0.5) },
    { fields: "Fields.Tags,GradeAliases,Keywords", min: getPartialLength(resource.gradeLevel.length + resource.keyword.items.length, 0.7) },
    { fields: "Creator,Publisher,Submitter", min: 3 }
  ];
  try {
  	var data = { intID: resourceID, text: useThisTitle, parameters: attempts[moreLikeThisAttempts].fields, minFieldMatches: attempts[moreLikeThisAttempts].min };
    doAjax("FindMoreLikeThis", data, updateMoreLikeThis);
    moreLikeThisAttempts++;
  }
  catch (e) {
    box.append("<p class=\"pleaseLogin\">Sorry, no similar resources found.</p>");
  }
}
function getPartialLength(value, percentage) {
    try {
        var toReturn = Math.floor(value * percentage);
        if (isNaN(toReturn)) {
            return 5;
        }
        else {
            return toReturn;
        }
    }
    catch (e) {
        return 5;
    }
}
function getKeyPhrases() {
	var phrases = [];
	for (var i in resource.keyword.items) {
		phrases.push('"' + resource.keyword.items[i] + '"');
	}
	return phrases.join(" ");
}
*/

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
        error: function (msg) {
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

/*function updateMoreLikeThis(data) {
	try {
		if (typeof (data) == "string") {
			data = $.parseJSON(data);
		}
		console.log("More Like This (vanilla) Result Data", data);
		var resultsPerQueryLimit = 10 - mltQueries.length;
		resultsPerQueryLimit = resultsPerQueryLimit < 5 ? 5 : resultsPerQueryLimit;
		var addedCount = 0;
		for (var i = 0; i < data.hits.hits.length && addedCount < resultsPerQueryLimit; i++) {
			//Prevent duplicates
			for (var j in mltResults) {
				if (mltResults[j].ResourceId == data.hits.hits[i]._source.ResourceId) { continue; }
			}
			//Add the result if it's not the resource
			if (data.hits.hits[i]._source.ResourceId != resource.intID) {
				mltResults.push(data.hits.hits[i]._source);
				addedCount++;
			}
		}
	}
	catch (e) { }
	if (mltResults.length < 25 && moreLikeThisAttempts <= mltQueries.length && moreLikeThisAttempts < 5) {
		moreLikeThisAttempts++;
		console.log("additional attempt");
		var search = { intID: resourceID, text: mltQueries[moreLikeThisAttempts], parameters: "", minFieldMatches: 0 };
		console.log(search);
		setTimeout(function () {
			doAjax("FindMoreLikeThis", search, updateMoreLikeThis);
		}, 1000);
	}
	else {
		//Render MLT results
		console.log("MLT Results:", mltResults);
		var box = $("#moreLikeThis #moreLikeThisResults");
		box.html("");
		if(mltResults.length > 0){
			for (var i in mltResults) {
				var current = mltResults[i];
				box.append(
					$("#template_moreLikeThis").html()
						.replace(/{rid}/g, current.ResourceId)
						.replace(/{urlTitle}/g, current.UrlTitle.replace(/&/g, ""))
						.replace(/{title}/g, current.Title)
						.replace(/{description}/g, current.Description)
						.replace(/{intID}/g, current.ResourceId)
        );
				for (i in thumbDivTypes) {
					if (current.url.indexOf(thumbDivTypes[i].match) > -1) {
						$(".resourcesLikeThis").last().find("img").replaceWith(
								$("<div></div>").addClass("thumbnailDiv").css({ "width": "150px", "height": "113px" }).html(thumbDivTypes[i].header)
						);
					}
				}
			}
		}
		$(".tabNavigator a[data-id=moreLikeThis]").html(mltResults.length);
	}
}
function noMoreLikeThis() {
	$("#moreLikeThis .resourcesLikeThis").html("<p class=\"pleaseLogin\">Sorry, no similar resources found.</p>");
}*/

function updateMoreLikeThis_multiMatch(data) {
	console.log("More Like This (multi match) Result Data", data);
	renderMLT($.parseJSON(data.data));
}

function renderMLT(data) {
	console.log(data);
	var box = $("#moreLikeThis #moreLikeThisResults");
	box.html("");
	var minRequiredScore = 1;
	//var max = data.hits.max_score;
	//Determine max score (sans self-match)
	var max = 0;
	for (var i in data.hits.hits) {
		if (data.hits.hits[i]._source.ResourceId != resource.intID && data.hits.hits[i]._score > max) {
			max = data.hits.hits[i]._score;
		}
	}

	//Tweak this section to adjust MLT 
	if (max >= 10) { minRequiredScore = 5; }
	else if (max >= 7.5) { minRequiredScore = 3.75; }
	else if (max >= 5) { minRequiredScore = 2.5; }
	else if (max >= 2) { minRequiredScore = 1; }
	else if (max >= 1) { minRequiredScore = 0.85; }
	else { minRequiredScore = 0.7; }

	console.log("Max MLT Score: " + max);
	console.log("Min Required: " + minRequiredScore);
	for (var i in data.hits.hits) {
		//Minimum score tweak
		if (data.hits.hits[i]._score < minRequiredScore) {
			continue;
		}
		var current = data.hits.hits[i]._source;
		//Remove identical document
		if (current.ResourceId == resource.intID) {
			continue;
		}
		box.append(
			$("#template_moreLikeThis").html()
				.replace(/{rid}/g, current.ResourceId)
				.replace(/{urlTitle}/g, current.UrlTitle.replace(/&/g, ""))
				.replace(/{title}/g, current.Title)
				.replace(/{description}/g, current.Description)
				.replace(/{intID}/g, current.ResourceId)
		);
		for (i in thumbDivTypes) {
			if (current.url.indexOf(thumbDivTypes[i].match) > -1) {
				$(".resourcesLikeThis").last().find("img").replaceWith(
						$("<div></div>").addClass("thumbnailDiv").css({ "width": "150px", "height": "113px" }).html(thumbDivTypes[i].header)
				);
			}
		}
	}
	var finalLength = box.find(".resourceLikeThis").length;
	setTabValue("moreLikeThis", finalLength);
	if (finalLength == 0) {
		box.html("<p class=\"pleaseLogin\">Sorry, no similar resources found.</p>");
	}
}

/*
function updateMoreLikeThisOLD(data) {
	if (typeof (data) == "string") {
		data = $.parseJSON(data);
	}
    if (typeof (data) == "string") { renderMoreLikeThis(); return; }
    if (data.hits.total == 0) {
        renderMoreLikeThis();
    }
    else {
    	//moreLikeThisAttempts = 0;
        $(".resourcesLikeThis").html("");
        var counter = 0;
        for (var i in data.hits.hits) {
        	var current = data.hits.hits[i]._source;
            if (current.VersionId == resourceVersionID || current.Url == resource.url) { continue; }
            counter++;
            $(".resourcesLikeThis").append(
                $("#template_moreLikeThis").html()
                    .replace(/{rid}/g, current.ResourceId)
                    .replace(/{urlTitle}/g, current.Title.replace(/ /g, "_").replace(/:/g, "").substring(0, 100))
                    .replace(/{title}/g, current.Title)
                    .replace(/{description}/g, current.Description)
                    .replace(/{intID}/g, current.ResourceId)
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
*/
function updateLikesDislikes(data) {
    if (data) {
        resource.paradata = data;
        renderLikesDislikes();
        renderViewTabCounts();
    }
}

function updateClickThroughs(data) {
    resource.paradata.clickThroughs = parseInt(data);
    $("#clickthroughs").html("Visited " + resource.paradata.clickThroughs + " Times");
}

function updateResource(data) {
    $(".btn.finish").attr("value", "Save Changes").attr("disabled", false);
    resource = data;
    $(".tab .addedTextItem").remove();
    $("#selectedStandards .selectedStandard").remove();
    renderResource();
    mode = "view";
    toggleShowHide();
}

function confirmDeactivated(data) {
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
    $(".tabNavigator a[data-id=tags], .tabNavigator a[data-id=comments]").trigger("click");
}
//Keywords and Subjects
function addFreeText(text, field) {
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

function setTabValue(tabID, value) {
	$(".tabNavigator a[data-id=" + tabID + "] .value").html(value);
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
    if (confirm("Really deactivate this resource?")) {
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