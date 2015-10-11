<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RubricsV2.ascx.cs" Inherits="IOER.Controls.RubricsV2.RubricsV2" %>

<link rel="stylesheet" type="text/css" href="/styles/common2.css" />

<div id="content">
	<%-- Evaluation Selector --%>
	<div id="evaluationSelector" runat="server" visible="true">
		<script type="text/javascript">
			$(document).ready(function () {
				$("#rubricChoices input[type=radio]").on("change", function () {
					$(".rubricDescription").removeClass("selected").filter("[data-rubricID=" + $(this).attr("data-rubricID") + "]").addClass("selected");
				});
				$("#rubricChoices input[type=radio]").first().prop("checked", "checked").trigger("change");
			});
		</script>
		<style type="text/css">
			.isleH1 { text-align: center; }
			#rubricChoices, #rubricDescriptions { display: inline-block; vertical-align: top; }
			#rubricChoices { width: 300px; padding: 0 10px; }
			#rubricDescriptions { width: calc(100% - 300px); }
			#rubricChoices label { display: block; transition: background-color 0.2s; border-radius: 5px; padding: 5px; }
			#rubricChoices label:hover, #rubricChoices label:focus { background-color: #F5F5F5; cursor: pointer; }
			.rubricDescription { display: none; }
			.rubricDescription.selected { display: block;  }
			#choiceButtons { text-align: center; padding: 5px; }
			#btnSubmitChoice { width: 300px; font-size: 20px; }
			.hasRating { padding: 5px; background-color: #F5F5F5; border-radius: 5px; margin: 10px; }
		</style>
		<h1 class="isleH1">IOER Rubric Evaluation Tool</h1>
		<p>Please select a Rubric to begin:</p>
		<div id="rubricChoices">
			<% foreach(var item in AvailableRubrics) { %>
			<label><input type="radio" id="cbx_<%=item.RubricId %>" data-rubricID="<%=item.RubricId %>" name="rubricChoice" value="<%=item.RubricId %>" /> <%=item.Title %></label>
			<% } %>
		</div><!--
		--><div id="rubricDescriptions">
			<% foreach(var item in AvailableRubrics) { %>
			<div class="rubricDescription" data-rubricID="<%=item.RubricId %>">
				<p><%=item.Description %></p>
				<% if(item.HasUserRating){ %>
				<div class="hasRating">
					<p>Your rating for this Resource with this Rubric is: <b><%=item.GetOverallScoreWord() %> (<%=item.OverallScore %>%)</b>.</p>
					<p>You may re-rate this Resource with this Rubric; however, only your most recent rating is kept.</p>
				</div>
				<% } %>
			</div>
			<% } %>
		</div>
		<div id="choiceButtons">
			<input type="hidden" name="postbackType" value="selectRubric" />
			<input type="submit" class="isleButton bgGreen" id="btnSubmitChoice" onclick="window.document.forms[0].onsubmit = null" value="Begin" />
		</div>
	</div><!-- /evaluationSelector -->

	<%-- Evaluation System --%>
	<div id="evaluator" runat="server" visible="false">
		<script type="text/javascript">
			var currentDimension = 0;
			var dimensions = <%=ActiveRubric.GetMyIdsJSON(true) %>;
		</script>
		<script type="text/javascript">
			//Initialize
			$(document).ready(function () {
				setupDimensions();
				setupScoring();
				$("form").removeAttr("onsubmit");
			});

			//Setup dimensions
			function setupDimensions() {
				$("#ddlDimensions").on("change", function () {
					var targetID = parseInt($(this).find("option:selected").attr("value"));
					showDimension(targetID);
				});
				showDimension(0);
			}

			//Setup scoring
			function setupScoring() {
				$(".score label input").on("change", function() {
					calculateScores();
				});
				calculateScores();
			}

			//Show/hide dimensions
			function showDimension(targetID) {
				var count = 0;
				for (var i in dimensions) {
					if (dimensions[i] == targetID) {
						currentDimension = parseInt(i);
					}
				}
				$("#ddlDimensions option[value=" + targetID + "]").prop("selected", true);
				$(".dimension").removeClass("selected").filter("[data-dimensionID=" + targetID + "]").addClass("selected");
				$(".nextPrevButton").prop("disabled", false);
				if (currentDimension + 1 == dimensions.length) {
					$("#btnNext").prop("disabled", true);
				}
				if (currentDimension == 0) {
					$("#btnPrevious").prop("disabled", true);
				}
				$(".score").hide().filter("[data-dimensionID=" + targetID + "]").show();
			}
			function showNextDimension() {
					showDimension(dimensions[currentDimension + 1]);
			}
			function showPreviousDimension() {
					showDimension(dimensions[currentDimension - 1]);
			}

			//Load resource in iframe
			function previewInFrame(){
				var url = "<%=ActiveRubric.ResourceUrl %>";
				if(url.indexOf("https") == 0){
					alert("This Resource might not load properly in the frame. If it does not, please use the 'open in new window' button instead.");
				}
				$("#preview").attr("src", url.replace("https","http"));
			}

			//Calculate scores
			function calculateScores() {
				var scoreBoxes = $(".score");
				var scores = [];
				scoreBoxes.each(function() {
					var box = $(this);
					var dimensionID = box.attr("data-dimensionID");
					var rating = box.find("input:checked").attr("value");
					if(isNaN(rating)){
						$("#ratings .ratingBarInner[data-dimensionID=" + dimensionID + "]").addClass("notApplicable").css("width", "100%");
					}
					else {
						var score = (parseInt(rating) * 100) / 3;
						scores.push(score);
						$("#ratings .ratingBarInner[data-dimensionID=" + dimensionID + "]").removeClass("notApplicable").css("width", score + "%");
					}
				});
				if(scores.length > 0){
					finalScore = 0
					for(var i in scores){
						finalScore += scores[i];
					}
					finalScore = finalScore / scores.length;
					var overalls = [
						{ score: 0, word: "Very Weak", css: "veryweak" },
						{ score: 25, word: "Limited", css: "limited" },
						{ score: 50, word: "Strong", css: "strong" },
						{ score: 75, word: "Superior", css: "superior" }
					];
					var selectedOverall = overalls[0];
					for(var i in overalls){
						if(finalScore > overalls[i].score) {
							selectedOverall = overalls[i];
						}
					}
					$("#overallRatingWord").attr("class", selectedOverall.css).html(selectedOverall.word);
					$("#ratings .ratingBarOuter.overall .ratingBarInner").removeClass("notApplicable").css("width", finalScore + "%");
				}
				else {
					$("#overallRatingWord").attr("class","notApplicable").html("Not Applicable");
					$("#ratings .ratingBarOuter.overall .ratingBarInner").addClass("notApplicable").css("width", "100%");
				}
			}

			//Submit rating
			function submitRating(){
				$("#btnFinish").prop("disabled", true);
				$("form").removeAttr("onsubmit");
				$("form")[0].submit();
			}
		</script>
		<style type="text/css">
			.isleH1 { text-align: center; }
			#controls, #dimensions { display: inline-block; vertical-align: top; }
			#controls { width: 400px; }
			#dimensions { width: calc(100% - 400px); }
			#rubricHeader { min-height: 200px; max-width: 1500px; margin: 0 auto 10px auto; }
			#frameWrapper { width: 100%; height: 800px; }
			#frameWrapper iframe { width: 100%; height: 100%; display: block; border: 0; background-color: #EEE; border-radius: 0 0 5px 5px; border: 1px solid #CCC; }
			#ddlDimensions { width: 100%; display: block; border-radius: 5px 5px 0 0; }
			#dimensionButtons { margin-bottom: 5px; }
			#dimensionButtons .nextPrevButton { width: 50%; display: inline-block; vertical-align: top; font-size: 18px; }
			#dimensionButtons .nextPrevButton:disabled { opacity: 0.5; }
			#btnPrevious { border-radius: 0 0 0 5px; }
			#btnNext { border-radius: 0 0 5px 0; }
			.dimension { display: none; }
			.dimension.selected { display: block; }
			.dimension h2 { font-size: 18px; padding: 5px; background-color: #DDD; margin-bottom: 5px; border-radius: 5px; }
			.dimension p { line-height: 1.3em; margin-bottom: 1em; }
			.dimension ol, .dimension ul { margin-bottom: 1em; }
			.dimension p + ol, .dimension p + ul { margin-top: -0.7em; }
			.dimension li { margin-bottom: 0.2em; line-height: 1.3em; }
			.dimension.scrolling h2 { border-radius: 5px 5px 0 0; margin-bottom: 0; }
			.dimension.scrolling .dimensionText { max-height: 500px; padding: 5px; border-radius: 0 0 5px 5px; border: 1px solid #CCC; background-color: #F5F5F5; overflow-y: auto; }
			#controls .score { padding: 5px; border-radius: 5px; margin: 5px 0; background-color: #EEE; border: 1px solid #CCC; }
			#controls .score div { font-weight: bold; text-align: center; font-size: 18px; }
			#controls .score label { display: block; transition: background-color 0.2s; border-radius: 5px; padding: 5px; }
			#controls .score label:hover, .dimension .score label:focus { background-color: #DDD; cursor: pointer; }
			#overallRating { font-weight: bold; text-align: center; padding: 5px; font-size: 22px; }
			#overallRatingWord { transition: color 0.5s; font-size: inherit; }
			#overallRatingWord.notApplicable { color: #CCC; }
			#overallRatingWord.veryweak { color: #C33; }
			#overallRatingWord.limited { color: #B90; }
			#overallRatingWord.strong { color: #3B3; }
			#overallRatingWord.superior { color: #09D; }
			#dimensions { padding: 0 10px; }
			#previewButtons { text-align: center; padding: 2px 5%; background-color: #DDD; border-radius: 5px 5px 0 0; border: 1px solid #CCC; }
			#previewButtons * { display: inline-block; vertical-align: top; width: 31.3332%; margin: 0 1%; }
			#ratings .ratingBarOuter { background-color: #AAA; background-image: linear-gradient(rgba(0,50,50,0.3),rgba(0,0,0,0)); position: relative; height: 26px; border: 1px solid #CCC; margin-bottom: 1px; padding: 1px; }
			#ratings .ratingBarInner { background-color: #4AA394; height: 100%; transition: width 0.5s, background-color 0.5s; }
			#ratings .ratingBarInner.notApplicable { background-color: #BBB; opacity: 0.8; }
			#ratings .ratingBarText { position: absolute; top: 0; left: 0; width: 100%; height: 100%; text-align: center; line-height: 26px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; color: #FFF; }
			#ratings .ratingBarOuter:first-child, #ratings .ratingBarOuter:first-child .ratingBarInner { border-radius: 5px 5px 0 0; }
			#ratings .ratingBarOuter:last-child, #ratings .ratingBarOuter:last-child .ratingBarInner { border-radius: 0 0 5px 5px; }
			#btnFinish { font-size: 18px; padding: 5px; margin: 10px 0; }

			@media (max-width: 850px) {
				#controls, #dimensions { width: 50%; }
			}
			@media (max-width: 600px) {
				#controls, #dimensions { width: 100%; display: block; }
			}
		</style>
		<h1 class="isleH1"><%=ActiveRubric.Title %> Rubric</h1>
		<div id="rubricHeader">
			<div id="dimensions">
				<% var merged = ActiveRubric.GetMergedDimensions(); %>
				<% foreach(var item in merged) { %>
				<div class="dimension <% if(ActiveRubric.Dimensions.Contains(item)){ %>scrolling<% } %>" data-dimensionID="<%=item.DimensionId %>">
					<h2><%=item.Title %></h2>
					<div class="dimensionText"><%=item.Description %></div>
					<% if(item == ActiveRubric.Introduction) { %>
					<p><b>Click the "Next" button to begin.</b></p>
					<p>Optionally, you may preview the Resource using one of the buttons below.</p>
					<% } %>
					<% if(item == ActiveRubric.Finish){ %>
					<input type="hidden" name="rubricChoice" value="<%=ActiveRubric.RubricId %>" />
					<input type="hidden" name="postbackType" value="submitRating" />
					<input type="button" id="btnFinish" class="isleButton bgGreen" onclick="submitRating()" value="I'm finished. Issue my ratings!" />
					<% } %>
				</div>
				<% } %>
			</div><!--
			--><div id="controls">
				<div id="navButtons">
					<select id="ddlDimensions">
						<% foreach(var item in merged) { %>
						<option value="<%=item.DimensionId %>"><%=item.Title %></option>
						<% } %>
					</select>
					<div id="dimensionButtons">
						<input type="button" value="Prev" class="nextPrevButton isleButton bgBlue" id="btnPrevious" onclick="showPreviousDimension();" /><!--
						--><input type="button" value="Next" class="nextPrevButton isleButton bgBlue" id="btnNext" onclick="showNextDimension();" />
					</div>
				</div>
				<div id="ratings">
					<% foreach(var item in ActiveRubric.Dimensions) { %>
					<div class="ratingBarOuter">
						<div class="ratingBarInner" data-dimensionID="<%=item.DimensionId %>"></div>
						<div class="ratingBarText"><%=item.Title %></div>
					</div>
					<% } %>
					<div class="ratingBarOuter overall">
						<div class="ratingBarInner" data-dimensionID="overall"></div>
						<div class="ratingBarText">Overall</div>
					</div>
				</div>
				<div id="overallRating">Overall Rating: <span id="overallRatingWord"></span></div>
				<div id="scores">
					<% foreach(var item in merged){ %>
					<% if( ActiveRubric.Dimensions.Contains( item ) ) { %>
					<div class="score" data-dimensionID="<%=item.DimensionId %>">
						<div>Please rate the strength of the alignment of this Resource to the displayed criteria:</div>
						<label><input type="radio" name="score_<%=item.DimensionId %>" value="3" /> Superior</label><!--
						--><label><input type="radio" name="score_<%=item.DimensionId %>" value="2" /> Strong</label><!--
						--><label><input type="radio" name="score_<%=item.DimensionId %>" value="1" /> Limited</label><!--
						--><label><input type="radio" name="score_<%=item.DimensionId %>" value="0" /> Very Weak</label><!--
						--><label><input type="radio" name="score_<%=item.DimensionId %>" value="NA" checked="checked" /> Not Applicable</label>
					</div>
					<% } %>
					<% } %>
				</div>
			</div>
		</div>
		<div id="previewButtons">
			<span>Show the Resource:</span><!--
			--><a class="previewButton isleButton bgBlue" id="btnPreviewFrame" onclick="previewInFrame(); return false;" href="#">In the frame below</a><!--
			--><a class="previewButton isleButton bgBlue" id="btnPreviewNew" target="_blank" href="<%=ActiveRubric.ResourceUrl %>">In a new window</a>
		</div>
		<div id="frameWrapper">
			<iframe id="preview" src=""></iframe>
		</div>
	</div>

	<%--  Error --%>
	<div id="errorBox" runat="server" visible="false">
		<p style="padding: 50px; text-align: center;"><asp:Literal ID="errorMessage" runat="server" /></p>
	</div>
</div>