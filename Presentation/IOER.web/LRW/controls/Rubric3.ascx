<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Rubric3.ascx.cs" Inherits="ILPathways.LRW.controls.Rubric3" %>
<%@ Register TagPrefix="uc1" TagName="CommentBox" Src="/LRW/controls/Comments.ascx" %>
<%@ Register TagPrefix="uc1" TagName="HeaderControl" Src="/Controls/Includes/Header6.ascx" %>

<asp:ScriptManager runat="server" />
<% 
  //Easy CSS colors
  string css_black      = "#4F4E4F";
  string css_red        = "#B03D25";
  string css_orange     = "#FF5707";
  string css_purple     = "#9984BD";
  string css_teal       = "#4AA394";
  string css_gray       = "#909297";
  string css_blue       = "#3572B8";
  string css_white      = "#E6E6E6";
  string css_lightblue  = "#4C98CC";
%>

<script type="text/javascript" language="javascript" src="http://ajax.googleapis.com/ajax/libs/jquery/1.9.0/jquery.min.js"></script>
<link rel="Stylesheet" type="text/css" href="/Styles/ISLE.css" />

<!-- Major Page Styles -->
<style type="text/css">
/* Major Page Sections */
html, body, form, #mainContainer {
  margin: 0;
  padding: 0;
  width: 100%;
  height: 100%;
  overflow: hidden;
}
body {
  background-color: <%=css_white %>;
}
#toolbox {
  width: 400px;
  height: 100%;
  background-color: #FFF;
  overflow-x: hidden;
  overflow-y: scroll;
}
#previewFrame {
  position: fixed;
  left: 400px;
  top: 80px;
  height: 100%;
  border: none;
}
.page, .comments {
  padding: 5px;
}
</style>
<!-- Standards Objects -->
<style type="text/css">
  .appliedStandard h2 a {
    color: #FFF;
  }
  .appliedStandard h2 a:hover, .appliedStandard h2 a:focus {
    color: #FFF;
    text-decoration: underline;
  }
  .skipLink {
    display: inline-block;
    *display: inline;
    zoom: 1;
    float: right;
  }
</style>
<!-- Page Stuff -->
<style type="text/css">
  .page input[type=button], .page input[type=submit] {
    background-color: <%=css_blue %>;
    color: #FFF;
    font-weight: bold;
    padding: 1px 5px;
    border-radius: 5px;
    display: block;
    min-width: 150px;
  }
  .page input[type=button]:hover, .page input[type=button]:focus, .page input[type=submit]:hover, .page input[type=submit]:focus {
    background-color: <%=css_orange %>;
    cursor: pointer;
  }
  .page input[type=submit] {
    width: 100%;
    padding: 3px 0;
  }
  .nav {
    height: 30px;
    margin-top: 25px;
  }
  .nav .left {
    float: left;
  }
  .nav .right {
    float: right;
  }
  .page input[type=checkbox] {
    margin: 2px 5px 0 5px;
  }
  .page label {
    display: block;
    border-radius: 5px;
    overflow: hidden;
    vertical-align: top;
    background-image: url('/images/whiteMiddleGradient50.png');
    background-position: 0 -25px;
    background-repeat: repeat-x;
  }
  .dimension ul li label {
    padding: 8px 5px;
  }
  .page label:hover, .page label:focus {
    background-color: #EEE;
    cursor: pointer;
  }
  ul ul {
    margin-left: 25px;
    list-style-type: disc;
  }
</style>
<!-- Introduction items -->
<style type="text/css">
  .previewLink {
    display: block;
    text-align: right;
  }
  .previewLink span {
    display: inline-block;
    height: 30px;
    padding: 2px;
  }
  .previewLink .icon {
    height: 25px;
    width: 25px;
    background: transparent url('/images/icons/preview-popup.png') no-repeat top left;
  }
  .previewLink .text {
    vertical-align: middle;
  }
  .previewLink .frame {
    background-position: 0 0;
  }
  .previewLink .popup {
    background-position: -30px 0;
  }
  .previewLink:hover .frame, .previewLink:focus .frame {
    background-position: 0 -30px;
  }
  .previewLink:hover .popup, .previewLink:focus .popup {
    background-position: -30px -30px;
  }
  .btnIntro {
    width: 300px;
    margin: 1px auto;
  }
  #intro h1.isleH1 {
    margin-top: 0;
  }
  #intro h3.isleH3_Block {
    margin-top: 10px;
  }
</style>
<!-- Ratings boxes -->
<style type="text/css">
  .rating {
    width: 340px;
    margin: 2px auto;
    border-radius: 5px;
    overflow: hidden;
  }
  .rating label {
    display: inline-block;
    *display: inline;
    zoom: 1;
    width: 86px;
    background: #111 url('images/scoreboard.png') 0 0;
    background-repeat: repeat;
    margin: 0 -4px 0 0;
    color: #DDD;
    text-align: center;
    height: 40px;
    line-height: 40px;
    font-weight: bold;
    border-radius: 0;
  }
  .rating input[type=radio] {
    display: none;
  }
  .rating label:hover, .rating label:focus {
    color: #FFF;
    background-color: <%=css_black %>;
    background-color: #333;
    text-shadow: 0 0 0.2em #FFF;
  }
  .rating label.selected, .quality0 { /* fallback for older browsers */
    color: #FFF;
    text-shadow: 0 0 0.5em #FFF;
  }
  .rating label.selected:nth-child(1), .quality1 {
    color: #F55;
    text-shadow: 0 0 0.5em #F55;
  }
  .rating label.selected:nth-child(2), .quality2 {
    color: #FF5;
    text-shadow: 0 0 0.5em #FF5;
  }
  .rating label.selected:nth-child(3), .quality3 {
    color: #5F5;
    text-shadow: 0 0 0.5em #5F5;
  }
  .rating label.selected:nth-child(4), .quality4 {
    color: #5CF;
    text-shadow: 0 0 0.5em #5CF;
  }
  .dimension ul li label.selected {
    background-color: #EEE;
  }
</style>
<!-- Scoreboard -->
<style type="text/css">
  .scoreboard {
    list-style-type: none;
    border-radius: 5px;
    overflow: hidden;
    background-color: <%=css_black %>;
    background: #111 url('images/scoreboard.png') 0 0;
    background-repeat: repeat;
    color: #FFF;
    text-shadow: 0 0 0.5em #FFF;
    font-weight: bold;
  }
  .scoreboard li {
    border: 1px solid <%=css_black %>;
  }
  .scoreboard li div {
    height: 12px;
    line-height: 12px;
    padding: 1px 2px;
    text-transform: uppercase;
    display: inline-block;
    *display: inline;
    zoom: 1;
    margin-right: -4px;
    overflow: hidden;
  }
  .scoreboard div.title {
    width: 105px;
    text-align: right;
  }
  .scoreboard div.score {
    width: 225px;
    padding-left: 10px;
  }
</style>
<!-- Miscellaneous -->
<style type="text/css">
  .page.last input[type=button].right {
    display: none;
  }
  .warning {
    border-radius: 5px;
    background-color: <%=css_red %>;
    padding: 2px;
    text-align: center;
    font-weight: bold;
    color: #FFF;
    margin: 5px auto;
  }
</style>
<!-- Comments -->
<style type="text/css">
  /* Comments */
  .addCommentBox {
    padding: 8px;
  }
  .addCommentBox textarea {
    width: 100%;
    max-width: 100%;
    min-width: 100%;
    height: 75px;
    min-height:60px;
    max-height:125px;
  }
  .addCommentBox .commentStatus, .addCommentBox .responseMessage {
    display: inline-block;
    *display: inline;
    zoom: 1;
    width: 250px;
    height: 25px;
    line-height: 25px;
  }
  .addCommentBox input[type=submit] {
    width: 75px;
    background-color: <%=css_blue %>;
    border-radius: 5px;
    padding: 1px;
    float: right;
    color: #FFF;
    margin: 1px;
  }
  .addCommentBox input[type=submit]:hover, .addCommentBox input[type=submit]:focus {
    background-color: <%=css_orange %>;
    cursor: pointer;
  }
</style>

<!-- Global Variables -->
<script type="text/javascript" language="javascript">
  //Global Variables
  <%=rubricData %>
  <%=standardsData %>
  simpleMode = false;

  //Global objects
  var rubrics = [
      { code: "math", name: "Mathematics", active: false, dimensionParents : [ "2", "3", "4", "5" ] },     //Would like a dynamic way to populate these,
      { code: "ela", name: "ELA/Literacy", active: false, dimensionParents : [ "29", "30", "31", "32" ] }  //but they're unlikely to ever change
    ];

</script>
<!-- Page Setup -->
<script type="text/javascript" language="javascript">
  //Page Setup
  var jWindow;
  $(document).ready(function () {
    jWindow = $(window);
    $(".page").not(".noButtons").append($("#template_navButtons").html());

    jWindow.resize(function () {
      resizeFrame();
    });
    jWindow.resize();

    showPage('intro');

    <%=returnMessage %>
  });

  function resizeFrame() {
    var desiredHeight = jWindow.height() - 80;
    $("#previewFrame").width(jWindow.width() - 400).height(desiredHeight);
    $("#toolbox").height(desiredHeight);
  }

</script>
<!-- Page Navigation -->
<script type="text/javascript" language="javascript">
  //Page Navigation
  function showPrevious(sender) {
    var target = $(sender).parentsUntil(".page").parent();
    if(target.hasClass("first")){ }
    else {
      showPage(target.prev()[0].id);
    }
  }
  function showNext(sender) {
    var target = $(sender).parentsUntil(".page").parent();
    if(target.hasClass("last")){ }
    else {
      if (target.find("div.rating").length > 0) {
        if(target.find("div.rating label input:checked").length == 0){
          alert("You have one or more ratings left to issue for this section.");
          return false;
        }
      }
      showPage(target.next()[0].id);
    }
  }
  function showPage(targetID) {
    updateScores();
    $(".page").hide();
    $("#" + targetID).fadeIn("fast");
    if (targetID == "intro") {
      $(".scoreBox").hide();
    }
    //Skip the rest of the rubric if the first part is too weak
    if (targetID == "dimension_2") {
      if ((scoreHolder.dimensions[0].score / scoreHolder.dimensions[0].max) < 0.5) {
        if (confirm("The Rubric is designed to ignore the rest of the evaluation if the first section is too weak. Do you want to issue a very low rating for this Resource and finish evaluating now?")) {
          $(".page").not("#dimension_1").find(".dimension .rating").each(function () {
            $(this).find("label:first").click();
          });
          updateScores();
          finish();
          $(".btnFinish").click();
        }
        else {
          showPage('dimension_1');
          return false;
        }
      }
    }
    else {
      $(".scoreBox").fadeIn("fast");
    }
  }
</script>
<!-- Scoring -->
<script type="text/javascript" language="javascript">
  function initScoreTriggers() {
    $(".page input[type=checkbox], .page .rating input[type=radio]").change(function () {
      var thisBox = $(this);
      if (thisBox.attr("type") == "radio") {
        thisBox.parent().siblings().removeClass("selected");
      }
      if (thisBox.prop("checked")) {
        thisBox.parent().addClass("selected");
      }
      else {
        thisBox.parent().removeClass("selected");
      }
      updateScores();
    });
    $(".page .rating").each(function(){ 
      //$(this).find("label:first").click();
    });
    updateScores();
  }

  var textScores = {
    scores: [
      { threshhold: -1, class: "quality0", text: "Waiting" },
      { threshhold: 0, class: "quality1", text: "Not Representing CCSS Quality" },
      { threshhold: 25, class: "quality2", text: "Approaching CCSS Quality" },
      { threshhold: 50, class: "quality3", text: "Developing Toward CCSS Quality" },
      { threshhold: 75, class: "quality4", text: "Exemplifies CCSS Quality" }
    ],
    classes : "quality0 quality1 quality2 quality3 quality4"
  };

  var scoreHolder = {
    standards: [ ],
    dimensions: [ ]
  };

  function updateScores() {
    scoreHolder.standards = [];
    scoreHolder.dimensions = [];
    //Standards Alignment
    $(".appliedStandard").not("#template_standard .appliedStandard").each(function () {
      var standard = {};
      var thisBox = $(this);
      standard.id = thisBox.attr("standardID");
      standard.rubric = thisBox.attr("standardRubric");
      standard.score = parseInt(thisBox.find(".rating input[type=radio]:checked").attr("value"));
      scoreHolder.standards.push(standard);
    });

    //Dimensions
    $(".page .dimension > ul").each(function () {
      var dimension = {};
      var thisBox = $(this);
      dimension.subjectCode = thisBox.attr("subject");
      dimension.dimensionNumber = thisBox.attr("dimension");
      if (simpleMode) {
        var element = thisBox.parent().find(".rating[rubricCode=" + dimension.subjectCode + "] input[type=radio]:checked")
        dimension.max = 3;
        dimension.score = parseInt(element.attr("value"));
      }
      else {
        dimension.max = thisBox.find("input[type=checkbox]").length;
        dimension.score = thisBox.find("input[type=checkbox]:checked").length;
      }
      scoreHolder.dimensions.push(dimension);
    });

    //Rendering
    //Fix nulls/zeroes
    if (scoreHolder.standards.length == 0) {
      var standard = { id: "0", rubric: "null", score: 0 };
      scoreHolder.standards.push(standard);
    }
    if (scoreHolder.dimensions.length == 0) {
      for (var i = 1; i <= 4; i++) {
        var dimension = { subjectCode: "null", dimensionNumber: i.toString(), max: 0, score: 0 };
        scoreHolder.dimensions.push(dimension);
      }
    }

    //Standards
    var standardsScore = 0;
    var standardsMax = 0;
    for(var i = 0; i < scoreHolder.standards.length; i++){
      standardsScore += scoreHolder.standards[i].score;
      standardsMax += 3;
    }
    var standardsPercent = standardsScore / standardsMax;
    renderScore(standardsPercent, $("#score_standardsAlignment"));

    //Dimensions
    var dimensionRender = {
      "dimension_1" : { score: 0, max: 0 },
      "dimension_2" : { score: 0, max: 0 },
      "dimension_3" : { score: 0, max: 0 },
      "dimension_4" : { score: 0, max: 0 },
      "total" : { score: 0, max: 0 }
    };
    for(var i = 0; i < scoreHolder.dimensions.length; i++){
      var item = scoreHolder.dimensions[i];
      dimensionRender["dimension_" + item.dimensionNumber].score += item.score;
      dimensionRender["dimension_" + item.dimensionNumber].max += item.max;
      dimensionRender["total"].score += item.score;
      dimensionRender["total"].max += item.max;
    }
    for(var i = 1; i <=4; i++){
      renderScore(dimensionRender["dimension_" + i].score / dimensionRender["dimension_" + i].max, $("#score_dimension_" + i));
    }
    var dimensionsPercent = dimensionRender["total"].score / dimensionRender["total"].max;
    renderScore(dimensionsPercent, $("#score_dimensions"));

    //Overall
    renderScore((standardsPercent + dimensionsPercent) / 2, $("#score_overall"));
  }

  function renderScore(percent, scorePanel){
    percent = percent * 100;
    var currentHTML = scorePanel.html();
    for (var i = 0; i < textScores.scores.length; i++) {
      if (percent >= textScores.scores[i].threshhold) {
        scorePanel.removeClass(textScores.classes).addClass(textScores.scores[i].class).html(textScores.scores[i].text);
      }
    }
    if (currentHTML != scorePanel.html()) {
      scorePanel.css("opacity", 0).animate({ "opacity": 1 }, 250);
    }
  }
</script>
<!-- Rubric Selection -->
<script type="text/javascript" language="javascript">
  function showRubric(rubricCode, simple) {
    simpleMode = simple;
    for (var i = 0; i < rubrics.length; i++) {
      rubrics[i].active = false;
      var selectedRubrics = rubricCode.split(",");
      for (var j = 0; j < selectedRubrics.length; j++) {
        if (selectedRubrics[j] == rubrics[i].code) {
          rubrics[i].active = true;
        }
      }
    }
    resetStandardsRatings();
    resetDimensions();
    initScoreTriggers();
    showPage('alignment');
  }

  function resetStandardsRatings() {
    var standardsList = $("#standardsList");
    standardsList.html("");
    for (var i = 0; i < standardsData.items.length; i++ ) {
      var matchingRubric = "";
      if( standardsData.items[i].notation.toLowerCase().indexOf("math") > -1){
        matchingRubric = "math";
      }
      else if( standardsData.items[i].notation.toLowerCase().indexOf("ela") > -1){
        matchingRubric = "ela";
      }
      for (var j = 0; j < rubrics.length; j++) {
        if (rubrics[j].code == matchingRubric && rubrics[j].active == true) {
          standardsList.append(
            $("#template_standard").html()
              .replace(/{id}/g, standardsData.items[i].id)
              .replace(/{subject}/g, matchingRubric)
              .replace(/{url}/g, standardsData.items[i].url)
              .replace(/{notation}/g, standardsData.items[i].notation)
              .replace(/{description}/g, standardsData.items[i].description)
          );
          $("#standard_" + standardsData.items[i].id + " .ratingsBox").append(
            $("#template_rating").html()
              .replace(/{id}/g, "standard_" + standardsData.items[i].id)
              .replace(/{code}/g, matchingRubric)
              .replace(/{name}/g, "rating_" + standardsData.items[i].id)
          );
        }
      }
    }
  }

  //Todo: add mid-headers
  //Todo: store criteria choices

  function resetDimensions() {
    $(".dimension").html("");
    for (var i = 0; i < rubrics.length; i++) {
      if (rubrics[i].active) {
        for (var j = 1; j <= 4; j++) {
          $("#dimension_" + j + " .dimension").append(
            "<ul subject=\"" + rubrics[i].code + "\" dimension=\"" + j + "\">" + 
            $("#template_criteriaHeader").html()
              .replace(/{title}/g, "For " + rubrics[i].name + ":")
              .replace(/{theResourceDots}/g, needDots(rubrics[i], j)) +
            printCriteria(rubrics[i], j) +
            "</ul>" +
            addDimensionRating(rubrics[i], j)
          );
        }
      }
    }
    if (simpleMode) {
      $(".page input[type=checkbox]").remove();
    }
  }

  function printCriteria(rubric, dimension) {
    var toReturn = "";
    var count = 1;
    for (var i = 0; i < rubricData.items.length; i++) {
      if (rubricData.items[i].parentID == rubric.dimensionParents[dimension - 1]) {
        toReturn += $("#template_criterion").html()
          .replace(/{identifier}/g, "cbx_" + rubric.code + "_" + dimension + "_" + count)
          .replace(/{description}/g, rubricData.items[i].description)
          .replace(/{notation}/g, rubricData.items[i].notation);
        count++;
      }
    }
    return toReturn;
  }

  function needDots(rubric, dimension) {
    if ((dimension == 1 && rubric.code == "math") || dimension == 2) {
      return "";
    }
    else {
      return "The Resource...";
    }
  }

  function addDimensionRating(rubric, dimension) {
    if (simpleMode) {
      return "<h3 class=\"isleH3_Block\">How strong is the Resource's alignment to the above criteria?</h3>" + 
        $("#template_rating").html()
          .replace(/{code}/g, rubric.code)
          .replace(/{id}/g, rubric.code + "_" + dimension)
          .replace(/{name}/g, rubric.code + "_" + dimension);
    }
    else {
      return "";
    }
  }
</script>
<!-- Frame/Iframe manipulation -->
<script type="text/javascript" language="javascript">
  function previewInFrame() {
    $("#previewFrame").prop("src", "<%=resourceURL %>");
  }
  function previewInPopup() {
    if (typeof resourcePreviewWindow == "object") {
      resourcePreviewWindow = null;
    }
    resourcePreviewWindow = window.open("<%=resourceURL %>", "", "toolbar=1,menubar=1,resizable=1,scrollbars=1,width=" + $("#previewFrame").width() + ",height=" + screen.height);
    resourcePreviewWindow.moveTo(400, 0);
    resourcePreviewWindow.focus();
  }
</script>
<!-- Publishing -->
<script type="text/javascript" language="javascript">
  function finish() {
    updateScores();
    var hdnScores = $(".hdnScores");
    var standardsScores = scoreHolder.standards;
    var dimensionScores = scoreHolder.dimensions;
    var value = "";

    value = "&standardsScores="
    for (var i = 0; i < standardsScores.length; i++) {
      if (standardsScores[i].rubric == "null") { } //skip
      else {
        value += standardsScores[i].rubric + "|" + standardsScores[i].id + "|" + standardsScores[i].score + "|" + 3 + ","
      }
    }

    value += "&dimensionScores="
    for (var i = 0; i < dimensionScores.length; i++) {
      if (dimensionScores[i].subjectCode == "null") { } //skip
      else {
        value += dimensionScores[i].subjectCode + "|" + dimensionScores[i].dimensionNumber + "|" + dimensionScores[i].score + "|" + dimensionScores[i].max + ","
      }
    }

    if (!simpleMode) {
      value += "&criteriaScores="
      $(".dimension").each(function () {
        value += "dimension_" + $(this).attr("dimension") + ":"
        $(this).find("input.criterion:checked").each(function () {
          value += $(this).attr("notation") + "|";
        });
        value += ",";
        $(this).find("input.criterion:not(:checked)").each(function () {
          value += $(this).attr("notation") + "|";
        });
        value += "~";
      });
    }

    hdnScores.val("").val(value);
    console.log(hdnScores.val());
  }
</script>
<!-- Miscellaneous -->
<script type="text/javascript" language="javascript">
  function removeStandard(target) {
    var toRemove = $(target).parentsUntil(".appliedStandard").parent();
    if (toRemove.parent().find(".appliedStandard").length == 1) {
      if (confirm("You must rate at least one standard. Would you like to start again?")) {
        resetStandardsRatings();
        return false;
      }
      else {
        return false;
      }
    }
    toRemove.animate({ "height": "0", "opacity": "0" }, "fast", function () {
      toRemove.remove();
      updateScores();
    });
    
  }
</script>

<uc1:HeaderControl ID="header" runat="server" />

<div id="mainContainer">

<div id="toolbox">

  <div class="scoreBox isleBox">
    <h2 class="isleBox_H2">Resource Scoreboard</h2>
    <ul class="scoreboard">
      <li>
        <div class="title">CCSS Alignment</div>
        <div class="score" id="score_standardsAlignment"></div>
      </li>
      <li>
        <div class="title">CCSS Depth</div>
        <div class="score" id="score_dimension_1"></div>
      </li>
      <li>
        <div class="title">Key Shifts</div>
        <div class="score" id="score_dimension_2"></div>
      </li>
      <li>
        <div class="title">Supports</div>
        <div class="score" id="score_dimension_3"></div>
      </li>
      <li>
        <div class="title">Assessment</div>
        <div class="score" id="score_dimension_4"></div>
      </li>
      <li>
        <div class="title">All Dimensions</div>
        <div class="score" id="score_dimensions"></div>
      </li>
      <li>
        <div class="title">Overall</div>
        <div class="score" id="score_overall"></div>
      </li>
    </ul>
  </div>

  <div class="page first noButtons" id="intro">
    <h1 class="isleH1">ISLE Evaluation</h1>
    <h3 class="isleH3_Block">Introduction</h3>
    <p>This tool is derived from <a href="http://www.achieve.org" target="_blank">Achieve.org</a>'s <a href="http://www.achieve.org/EQuIP" target="_blank">EQuIP</a> Rubrics to help you evaluate Learning Resources.</p>
    <p>Once you select the rubric(s) you want to use, examine the presented criteria and determine which ones apply. This will determine the Resource's score.</p>
    <h3 class="isleH3_Block">View the Resource</h3>
    <p>Would you like to see the Resource while you evaluate it?</p>
    <a href="javascript:previewInFrame()" class="previewLink"><span class="text">Preview in the frame to the right </span><span class="icon frame"></span></a>
    <p>Resource not working properly in the frame?</p>
    <a href="javascript:previewInPopup()" class="previewLink"><span class="text">Preview in a popup window </span><span class="icon popup"></span></a>
    <h3 class="isleH3_Block">Select Rubric(s)</h3>
    <input type="button" class="btnIntro" value="Mathematics" onclick="showRubric('math', false)" />
    <input type="button" class="btnIntro" value="ELA/Literacy 3-12" onclick="showRubric('ela', false)" />
    <input type="button" class="btnIntro" value="Both Rubrics" onclick="showRubric('math,ela', false)" />
    <!--<br />
    <input type="button" class="btnIntro" value="Mathematics (Simple Mode)" onclick="showRubric('math', true)" />
    <input type="button" class="btnIntro" value="ELA/Literacy 3-12 (Simple Mode)" onclick="showRubric('ela', true)" />
    <input type="button" class="btnIntro" value="Both Rubrics (Simple Mode)" onclick="showRubric('math,ela', true)" />
    <br />-->
    <input type="button" class="btnIntro" value="Cancel Evaluation" onclick="history.back()" />
  </div>

  <div class="page" id="alignment">
    <h2 class="isleH2">CCSS Alignment</h2>
    <p>How strong is the Resource's alignment to each listed Standard?</p>
    <div id="standardsList"></div>
  </div>

  <div class="page" id="dimension_1">
    <h2 class="isleH2">Alignment to the Depth of the CCSS</h2>
    <p>Please rate the Resource's alignment with the letter and spirit of the CCSS.</p>
    <p><b>Note:</b> If the Resource does not score well here, the rest of the evaluation will be skipped.</p>
    <div class="dimension" dimension="1"></div>
  </div>

  <div class="page" id="dimension_2">
    <h2 class="isleH2">Key Shifts in the CCSS</h2>
    <p>Please rate how the Resource addresses key shifts in the CCSS.</p>
    <div class="dimension" dimension="2"></div>
  </div>

  <div class="page" id="dimension_3">
    <h2 class="isleH2">Instructional Supports</h2>
    <p>Please rate how responsive the Resource is to varied student learning needs.</p>
    <div class="dimension" dimension="3"></div>
  </div>

  <div class="page" id="dimension_4">
    <h2 class="isleH2">Assessment</h2>
    <p>Please rate how the Resource assesses whether students are mastering standards-based content and skills.</p>
    <div class="dimension" dimension="4"></div>
  </div>

  <div class="page last" id="review">
    <h2 class="isleH2">Review</h2>
    <p>Please review the scores above. If you are satisfied, click the Finish button below.</p>
    <div class="warning">
    <p>You cannot change your evaluation once you click Finish.</p>
      <asp:button ID="btnFinish" OnClientClick="finish()" OnClick="btnFinish_Click" runat="server" Text="Finish!" CssClass="btnFinish" />
    </div>
  </div>

  <div class="comments">
    <uc1:CommentBox ID="Comments" runat="server" />
  </div>

</div><!-- end toolbox -->

<iframe id="previewFrame"></iframe> <!-- TODO: script iframe stuff -->

</div><!-- end mainContainer -->
<input type="hidden" id="hdnScores" runat="server" class="hdnScores" />

<div id="templates" style="display:none;">

  <div id="template_navButtons">
    <div class="nav">
      <input type="button" onclick="showPrevious(this)" class="left" value="&larr; Back" />
      <input type="button" onclick="showNext(this)" class="right" value="Next &rarr;" />
    </div>
  </div>

  <div id="template_standard">
    <div class="appliedStandard isleBox" standardID="{id}" standardRubric="{subject}" id="standard_{id}">
      <h2 class="isleBox_H2"><a href="{url}" target="_blank">{notation}</a><a href="javascript:void('')" onclick="removeStandard(this)" class="skipLink">Skip this Rating</a></h2>
      <p>{description}</p>
      <div class="ratingsBox" standardID="{id}"></div>
    </div>
  </div>

  <div id="template_rating">
    <div class="rating" rubricCode="{code}">
      <label for="{id}_1"><input type="radio" id="{id}_1" name="{name}" value="0" />Very Weak</label>
      <label for="{id}_2"><input type="radio" id="{id}_2" name="{name}" value="1" />Limited</label>
      <label for="{id}_3"><input type="radio" id="{id}_3" name="{name}" value="2" />Strong</label>
      <label for="{id}_4"><input type="radio" id="{id}_4" name="{name}" value="3" />Superior</label>
    </div>
  </div>

  <div id="template_criteriaHeader">
    <h3 class="isleH3_Block">{title}</h3>
    <p>How does the Resource relate to these criteria?</p>
    {theResourceDots}
  </div>

  <div id="template_criterion">
    <li><label for="{identifier}"><input type="checkbox" id="{identifier}" class="criterion" notation="{notation}" />{description}</label></li>
  </div>

  <div id="template_comment">
    <div class="comment isleBox" id="comment_{id}">
      <div class="isleBox_H2">
        <span class="commentName">{name}</span> wrote:<span class="dateTime">{date}</span>
      </div>
      <div class="commentText">{commentText}</div>
    </div>
  </div>


</div>

<script>
  (function (i, s, o, g, r, a, m) {
    i['GoogleAnalyticsObject'] = r; i[r] = i[r] || function () {
      (i[r].q = i[r].q || []).push(arguments)
    }, i[r].l = 1 * new Date(); a = s.createElement(o),
  m = s.getElementsByTagName(o)[0]; a.async = 1; a.src = g; m.parentNode.insertBefore(a, m)
  })(window, document, 'script', '//www.google-analytics.com/analytics.js', 'ga');

  ga('create', 'UA-42065465-1', 'ilsharedlearning.org');
  ga('send', 'pageview');

</script>

<asp:Panel ID="hiddenStuff" runat="server" Visible="false">
  <asp:Literal ID="rubricSelect" runat="server">SELECT [Id], [ParentId], [Notation], [pUrl], [ShortDescription] AS Description FROM [Rubric.Node]</asp:Literal>
</asp:Panel>