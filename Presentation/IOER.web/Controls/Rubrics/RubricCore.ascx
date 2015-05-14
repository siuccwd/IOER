<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RubricCore.ascx.cs" Inherits="ILPathways.Controls.Rubrics.RubricCore" %>
<%@ Register TagPrefix="rubric" TagName="AchieveRubricControl" Src="/Controls/Rubrics/AchieveRubric.ascx" %>
<%@ Register TagPrefix="rubric" TagName="CCSSMathRubricControl" Src="/Controls/Rubrics/CCSSMathRubric.ascx" %>
<%@ Register TagPrefix="rubric" TagName="CCSSELAK_2RubricControl" Src="/Controls/Rubrics/CCSSELAK-2Rubric.ascx" %>
<%@ Register TagPrefix="rubric" TagName="CCSSELA3_12RubricControl" Src="/Controls/Rubrics/CCSSELA3-12Rubric.ascx" %>

<style type="text/css">
  #tool { padding: 5px; }
  p { padding: 5px; }
  .rbl { margin-bottom: 30px; }
  label { display: block; padding: 2px 5px; border-radius: 5px; }
  label:hover, label:focus { background-color: #EEE; cursor: pointer; }
  #frame { background-color: #4AA394; }
  #previewFrame { background: transparent url('/images/icons/icon_ratings_med.png') no-repeat right -200px bottom -200px; background-size: 800px; }
  #previewFrame[src=""] { opacity: 0.4; }

  /* Scoreboard stuff */
  #scoreboardWrapper { background-color: #EEE; border-radius: 5px; margin: 0 0 5px 0; padding: 2px; }
  #scoreboardWrapper h2 { background-color: #4AA394; color: #FFF; margin: -2px -2px 2px -2px; border-radius: 5px 5px 0 0; padding: 2px 5px; }
  #scoreboard { background-color: #333; border-radius: 5px; padding: 2px; color: #FFF; }
  .scoreboardItem { position: relative; border-bottom: 1px solid #999; }
  .scoreboardItem:last-child { border: none; }
  .scoreboardBar { background-color: #9984BD; position: absolute; top: 0; left: 0; height: 100%; z-index: 1; transition: width 1s, background-color 1s, box-shadow 1s; -webkit-transition: width 1s, background-color 1s, box-shadow 1s; min-width: 1%; /*box-shadow: 0 0 10px 1px #9984BD;*/ }
  .scoreboardBar.NA { background-color: #555; box-shadow: none; }
  .scoreboardText { position: relative; z-index: 100; padding: 1px 2px; transition: box-shadow 0.2s; -webkit-transition: box-shadow 0.2s; display: block; color: #FFF; }
  a.scoreboardText:hover, a.scoreboardText:focus { box-shadow: 0px 0px 25px 0px #FF6A00 inset; cursor: pointer; color: #FFF; }
  .btnSubmitScores { display: none; }
</style>
<script type="text/javascript">
  /* Data from server */
  var data = <%=rubricDataString %>;
</script>
<script type="text/javascript">
  /* Initialization */
  $(document).ready(function () {
    handlePreviewMode();
    setupDimensions();
    setupTabs();
    setupScoreboard();
    $(window).on("scoresUpdated", renderScoreboard);
    fixStupidRadioButtonListLayout();
  });

  //Dimensions
  function setupDimensions(){
    if(typeof(data.dimensions) != "undefined"){
      data.dimensions.push({
        id: "overall",
        name: "Overall Score",
        score: null
      });
    }
  }

  //Tabs
  function setupTabs() {
    $("#rubricTitle").html(data.title);
    var selector = $("#tabSelector");
    for(i in data.dimensions){
      selector.append("<option value=\"" + data.dimensions[i].id + "\">" + data.dimensions[i].name + "</option>");
    }
    selector.on("change", function () { showTab($(this).find("option:selected").attr("value")); });
    showTab("intro");
  }

  //Scoreboard
  function setupScoreboard(){
    var board = $("#scoreboard");
    board.html();
    for(i in data.dimensions){
      board.append(
        $("#template_scoreboardItem").html()
          .replace(/{id}/g, data.dimensions[i].id)
          .replace(/{name}/g, data.dimensions[i].name)
      );
    }
  }

  //Fix asp.net
  function fixStupidRadioButtonListLayout() {
    $(".rblRubricSelector input").each(function() {
      $(this).prependTo($(this).next("label"));
      $(this).after(" ");
    });
    $(".rblRubricSelector br").remove();
  }

  //Preview mode
  function handlePreviewMode() {
    if (data.previewMode == "frame") { $("#previewFrame").attr("src", data.resourceURL); }
    else if (data.previewMode == "popup") { var popup = window.open(data.resourceURL); alert("Attempted to open the Resource in a new window. If this didn't work, please check your popup blocker."); }
  }

</script>
<script type="text/javascript">
  /* Page Functions */

  //Tabs
  function showTab(target) {
    $(".tab").hide();
    $(".tab[data-tabID=" + target + "]").show();
    $("#tabSelector option[value=" + target + "]").prop("selected", true);
  }

  //Calculate overall score
  function calculateOverallScore(){
    var totalScore = 0;
    var totalMax = 0;
    for(i in data.dimensions){
      if(data.dimensions[i].id == "overall"){ continue; }
      if(data.dimensions[i].score == null){ continue; }
      totalScore += data.dimensions[i].score;
      totalMax += 100;
    }
    //Not Applicable
    if(totalMax == 0){
      return null;
    }
    else {
      return Math.ceil((totalScore / totalMax) * 100);
    }
  }

  //Submit scores
  function submitScores() {
    for(i in data.dimensions){
      if(data.dimensions[i].id == "overall"){ data.dimensions[i].id = 0; }
      if(data.dimensions[i].score == null){ data.dimensions[i].score = -1; }
    }
    $(".hdnScoreHolder").val(JSON.stringify(data));
    $(".btnSubmitScores").click();
  }

</script>
<script type="text/javascript">
  /* Rendering */

  //Render Scoreboard
  function renderScoreboard() {
    for(i in data.dimensions){
      var bar = $("#scoreboard .scoreboardItem[data-dimensionID=" + data.dimensions[i].id + "] .scoreboardBar");
      if(data.dimensions[i].id == "overall"){ data.dimensions[i].score = calculateOverallScore(); }
      if(data.dimensions[i].score == null){
        bar.addClass("NA").css("width", "100%").attr("title", "Not Applicable");
      }
      else {
        bar.removeClass("NA").css("width", data.dimensions[i].score + "%").attr("title", data.dimensions[i].score + "%");
      }
    }
  }

</script>

<style type="text/css">
  select { width: 100%; }
  .tab h2 { margin: 5px; }
  ul { margin: 5px 5px 5px 25px; }
  ul li { display: list-item; }
  #navButtons { white-space: nowrap; }
  #navButtons input { width: 50%; }
  #tabSelector { display: none; }
</style>

<div id="scoreboardWrapper">
  <h2>Scoreboard</h2>
  <div id="scoreboard"></div>
  <div id="navButtons">
    <input type="button" class="isleButton bgBlue" value="Intro" onclick="showTab('intro');" /><input type="button" class="isleButton bgBlue" value="Finish" onclick="showTab('overall');" />
  </div>
</div>

<div id="rubricInfrastructure" runat="server">
  <h1 class="isleH1" id="rubricTitle"></h1>
  <select id="tabSelector">
    <option value="intro">Introduction</option>
  </select>
</div>

<div id="rubricContent">
  <rubric:AchieveRubricControl ID="AchieveRubric" runat="server" Visible="false" />
  <rubric:CCSSMathRubricControl ID="CCSSMathRubric" runat="server" Visible="false" />
  <rubric:CCSSELAK_2RubricControl ID="CCSSELAK_2Rubric" runat="server" Visible="false" />
  <rubric:CCSSELA3_12RubricControl ID="CCSSELA3_12Rubric" runat="server" Visible="false" />
  <div class="tab" data-tabID="overall">
    <h2>Finish</h2>
    <p>Please carefully review your scores and make sure you did not skip anything applicable.</p>
    <p>When you are satisfied with your ratings, click the "Finish" button.</p>
    <input type="button" id="btnFinish" value="Finish" class="isleButton bgGreen" onclick="submitScores()" />
  </div>

</div>

<div id="selectRubric" runat="server">
  <script type="text/javascript">
    $(document).ready(function() { $("#scoreboardWrapper").remove(); });
  </script>
  <h1 class="isleH1">IOER Resource Evaluator</h1>
  <p>Please select a Rubric to evaluate this Resource:</p>

  <asp:RadioButtonList ID="rblSelectRubric" runat="server" RepeatLayout="Flow" CssClass="rblRubricSelector" Visible="true" />

  <p>Please select a Previewing option:</p>
  <div id="rblPreviewSelector" class="rbl">
    <label for="rbPreviewFrame"><input type="radio" name="previewOption" id="rbPreviewFrame" value="frame" checked="checked" /> Preview in this window</label>
    <label for="rbPreviewPopup"><input type="radio" name="previewOption" id="rbPreviewPopup" value="popup" /> Preview in a popup window</label>
    <label for="rbPreviewNone"><input type="radio" name="previewOption" id="rbPreviewNone" value="none" /> Do not preview the Resource</label>
  </div>

  <input type="submit" class="isleButton bgGreen" value="Begin Evaluation" />
</div>

<div id="template_scoreboardItem" style="display:none;">
  <div class="scoreboardItem" data-dimensionID="{id}" data-name="{name}">
    <div class="scoreboardBar"></div>
    <a href="#" onclick="showTab('{id}'); return false;" class="scoreboardText">{name}</a>
  </div>
</div>

<asp:Button ID="btnSubmitScores" OnClick="btnSubmitScores_Click" runat="server" CssClass="btnSubmitScores" />
<input type="hidden" id="hdnScoreHolder" class="hdnScoreHolder" runat="server" value="selectingRubric" />