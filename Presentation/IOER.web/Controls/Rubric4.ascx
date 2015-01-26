<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Rubric4.ascx.cs" Inherits="ILPathways.Controls.Rubric4" %>

<div id="rubric" class="rubric" runat="server">

	<script type="text/javascript">
		$(document).ready(function () {
			$(".tab .list input").on("change", function () {
				showCriterion($(this));
				updateTabScore($(this));
				updateOverallScore();
			})
			showTab("intro");
			$("#criteria .criterion").hide();
		});

		function showTab(target) {
			$(".tab").hide();
			$(".tab[data-tabID=" + target + "]").show();
			if (target == "intro" || target == "overall") {
				$("#criteria").hide();
			}
			else {
				$("#criteria").show();
				showCriterion($(".tab[data-tabID=" + target + "] input:checked"));
			}
		}
		function showCriterion(item) {
			var dim = item.parentsUntil(".list").parent().attr("data-dim");
			var score = item.attr("value");
			$("#criteria .criterion").hide();
			$("#criteria .criterion[data-dim=" + dim + "][data-val=" + score + "]").show();
		}
		function updateTabScore(item) {
			var dim = item.parentsUntil(".list").parent().attr("data-dim");
			var score = item.attr("value");
			$("#navigation a[data-dim=" + dim + "]").attr("class", "s" + score);
			$("#navigation a[data-dim=" + dim + "] .score").html(score == "-1" ? "N/A" : score);
		}
		function updateOverallScore() {
			var scores = [];
			$(".tab .list input:checked").each(function () {
				var value = parseInt($(this).attr("value"));
				if (value != -1) {
					scores.push(value);
				}
			});

			var count = 0;
			var average = -1;
			var displayAverage = "";
			for (i in scores) {
				count += scores[i];
			}
			if (scores.length == 0) {
				average = -1;
				displayAverage = "";
			}
			else {
				average = (count / scores.length).toString().split(".")[0];
				displayAverage = (count / scores.length).toString().substr(0, 4);
			}

			$("#overallScore").attr("class", "s" + average);
			$("#overallScore .score").html(average == -1 ? "N/A" : displayAverage);
		}
	</script>
	<style type="text/css">
		.rubric { padding: 5px; }
		#navigation a { display: block; background-color: #3572B8; color: #FFF; padding: 3px 30px 3px 5px; margin-bottom: 1px; border-right: 25px solid #CCC; position: relative; transition: border 1s; -webkit-transition: border 1s; }
		#navigation a:first-child { border-radius: 5px 5px 0 0; border-right: none; }
		#navigation a:last-child { border-radius: 0 0 5px 5px; }
		#navigation a.s-1 { border-right-color: #CCC; }
		#navigation a.s0 { border-right-color: #FF3333; }
		#navigation a.s1 { border-right-color: #FFFF33; }
		#navigation a.s2 { border-right-color: #33FF33; }
		#navigation a.s3 { border-right-color: #3399FF; }
		#navigation a:hover, #navigation a:focus { background-color: #FF6A00; }
		#navigation a .score { position: absolute; top: 2px; right: 2px; }
		#criteria { background-color: #EEE; padding: 10px; margin: 10px; border-radius: 5px; }
		#criteria ul { margin-left: 25px; }
		.tab h2 { font-size: 24px; }
		.list { margin: 5px; }
		.list li { position: relative; }
		.list li label { display: block; padding-left: 25px; height: 25px; line-height: 25px; font-size: 18px; border-radius: 5px; }
		.list li label:hover, .list li label:focus { background-color: #EEE; }
		.list li input { position: absolute; top: 7px; left: 5px; }
		.btnSubmit { padding: 5px; width: 80%; display: block; margin: 10px auto; }
	</style>

	<div id="navigation">
		<a href="#" onclick="showTab('intro');">Introduction</a>
		<a href="#" onclick="showTab('dim2');" data-dim="dim2">Quality of Explanation of the Subject Matter <div class="score" data-dim="dim2"></div></a>
		<a href="#" onclick="showTab('dim3');" data-dim="dim3">Utility of Material Designed to Support Teaching <div class="score" data-dim="dim3"></div></a>
		<a href="#" onclick="showTab('dim4');" data-dim="dim4">Quality of Assessment <div class="score" data-dim="dim4"></div></a>
		<a href="#" onclick="showTab('dim5');" data-dim="dim5">Quality of Technological Interactivity <div class="score" data-dim="dim5"></div></a>
		<a href="#" onclick="showTab('dim6');" data-dim="dim6">Quality of Instructional and Practice Exercises <div class="score" data-dim="dim6"></div></a>
		<a href="#" onclick="showTab('dim7');" data-dim="dim7">Opportunities for Deeper Learning <div class="score" data-dim="dim7"></div></a>
		<a href="#" onclick="showTab('dim8');" data-dim="dim8">Assurance of Accessibility <div class="score" data-dim="dim8"></div></a>
		<a href="#" onclick="showTab('overall');" id="overallScore">Overall <div class="score" data-dim="overall"></div></a>
	</div>

	<div class="tab" data-tabID="intro">
		<h2>Introduction</h2>
		<p>This tool is modeled on the <a href="http://www.achieve.org/files/AchieveOERRubrics.pdf" target="_blank">Achieve OER Rubrics Collection</a> and is designed to evaluate Resources hosted in the IOER site.</p>
		<p>To use this tool, select each applicable rubric from the list above and review the criteria for each rating. Select the rating that best applies to the Resource.</p>
		<p>For full details, please review the document linked above. Note that this tool omits the first rubric, as Standards Alignment ratings are done on the Resource detail page you came from.</p>

	</div>
	<div class="tab" data-tabID="dim2">
		<h2>Quality of Explanation of the Subject Matter</h2>
		<p>This rubric is applied to objects designed to explain subject matter. It is used to rate how thoroughly the subject matter is explained or otherwise revealed in the object. Teachers might use this object with a whole class, a small group, or an individual student. Students might use the object to self-tutor. For objects that are primarily intended for teacher use, the rubric is applied to the explanation of the subject matter not to the planning instructions for the teacher.</p>
		<asp:RadioButtonList ID="rblDimension2" runat="server" CssClass="list dim2" data-dim="dim2" />

	</div>
	<div class="tab" data-tabID="dim3">
		<h2>Utility of Materials Designed to Support Teaching</h2>
		<p>This rubric is applied to objects designed to support teachers in planning or presenting subject matter. The primary user would be a teacher. This rubric evaluates the potential utility of an 
object at the intended grade level for the majority of instructors.</p>
		<asp:RadioButtonList ID="rblDimension3" runat="server" CssClass="list dim3" data-dim="dim3" />

	</div>
	<div class="tab" data-tabID="dim4">
		<h2>Quality of Assessment</h2>
		<p>This rubric is applied to those objects designed to determine what a student knows before, during, or after a topic is taught. When many assessment items are included in one object, as is often the case, the rubric is applied to the entire set.</p>
		<asp:RadioButtonList ID="rblDimension4" runat="server" CssClass="list dim4" data-dim="dim4" />

	</div>
	<div class="tab" data-tabID="dim5">
		<h2>Quality of Technological Interactivity</h2>
		<p>This rubric is applied to objects designed with a technology-based interactive component. It is used to rate the degree and quality of the interactivity of that component. “Interactivity” is used broadly to mean that the object responds to the user, in other words, it behaves differently based on what the user does. This is not a rating for technology in general, but for technological <i>interactivity</i>. The rubric does not apply to interaction between students, but rather to how the technology responds to the individual user.</p>
		<asp:RadioButtonList ID="rblDimension5" runat="server" CssClass="list dim5" data-dim="dim5" />

	</div>
	<div class="tab" data-tabID="dim6">
		<h2>Quality of Instructional and Practice Exercises</h2>
		<p>This rubric is applied to objects that contain exercises designed to provide an opportunity to practice and strengthen specific skills and knowledge. The purpose of these exercises is to deepen understanding of subject matter and to routinize foundational skills and procedures. When concepts and skills are introduced, providing a sufficient number of exercises to support skill acquisition is critical. However when integrating skills in complex tasks, the number of exercise problems is less important than their richness. These types of practice opportunities may include as few as one or two instructional exercises designed to provide practice applying specific concepts and/or skills. Sets of practice exercises are treated as a single object, with the rubric applied to an entire group.</p>
		<asp:RadioButtonList ID="rblDimension6" runat="server" CssClass="list dim6" data-dim="dim6" />

	</div>
	<div class="tab" data-tabID="dim7">
		<h2>Opportunities for Deeper Learning</h2>
		<p>This rubric is applied to objects designed to engage learners in at least one of the following deeper learning skills, which can be applied across all content areas:</p>
    <ul>
      <li>Think critically and solve complex problems.</li>
      <li>Work collaboratively.</li>
      <li>Communicate effectively.</li>
      <li>Learn how to learn.</li>
      <li>Reason abstractly.</li>
      <li>Construct viable arguments and critique the reasoning of others.</li>
      <li>Apply discrete knowledge and skills to real-world situations.</li>
      <li>Construct, use, or analyze models.</li>
    </ul>
		<asp:RadioButtonList ID="rblDimension7" runat="server" CssClass="list dim7" data-dim="dim7" />

	</div>
	<div class="tab" data-tabID="dim8">
		<h2>Assurance of Accessibiliy</h2>
		<p>This rubric is used to assure materials are accessible to all students, including students identified as blind, visually impaired or print disabled, and those students who may qualify under the Chafee Amendment to the U.S. 1931 Act to Provide Books to the Adult Blind as Amended. It was developed to assess compliance with U.S. standards and requirements, but could be adapted to accommodate differences in other sets of requirements internationally.</p>
    <p>Accessibility is critically important for all learners and should be considered in the design of all online materials. Identification of certain characteristics will assist in determining if materials will be fully accessible for all students. Assurance that materials are compliant with the standards, recommendations, and guidelines specified assists educators in the selection and use of accessible versions of materials that can be used with all students, including those with different kinds of challenges and assistive devices.</p>
    <p>The Assurance of Accessibility Standards Rubric does not ask reviewers to make a judgment on the degree of object quality. Instead, it requests that a determination (yes/no) of characteristics be made that, together with assurance of specific Standards, may determine the degree to which the materials are accessible. Only those who feel qualified to make judgments about an object’s accessibility should use this rubric.</p>
		<asp:RadioButtonList ID="rblDimension8" runat="server" CssClass="list dim8" data-dim="dim8" />

	</div>
	<div class="tab" data-tabID="overall">
		<h2>Overall Score</h2>
		<p>Intro Text</p>
		<asp:Button id="btnSubmit" runat="server" OnClick="btnSubmit_Click" Text="Submit" CssClass="isleButton bgGreen btnSubmit" />

	</div>

	<div id="criteria">
		<h2>Description for this Rating:</h2>
		<div class="criterion" data-dim="dim2" data-val="-1">This rubric is <i>not applicable</i> (N/A) for an object that is not designed to explain subject matter, for example, a sheet of mathematical formulae or a map. It may be possible to apply the object in some way that aids a learner’s understanding, but that is beyond any obvious or described purpose of the object.</div>
		<div class="criterion" data-dim="dim2" data-val="0">An object is rated <i>very weak or no value</i> for explanation of subject matter if its explanations are confusing or contain errors. There is little likelihood that this object will contribute to understanding.</div>
		<div class="criterion" data-dim="dim2" data-val="1">An object is rated <i>limited</i> for explanation of subject matter if it explains the subject matter correctly but in a limited way. This cursory treatment of the content is not sufficiently developed for a first-time learner of the content. The explanations are not thorough and would likely serve as a review for most learners.</div>
		<div class="criterion" data-dim="dim2" data-val="2">An object is rated <i>strong</i> for explanation of subject matter if it explains the subject matter in a way that makes skills, procedures, concepts, and/or information understandable. It falls short of <i>superior</i> in that it does not make connections among important associated concepts within the subject matter. <i>For example, a lesson on multi-digit addition may focus on the procedure and fail to connect it with place value.</i></div>
		<div class="criterion" data-dim="dim2" data-val="3">
			An object is rated <i>superior</i> for explanation of subject matter only if <b>all</b> of the following are true:
			<ul>
				<li>The object provides comprehensive information so effectively that the target audience should be able to understand the subject matter.</li>
				<li>The object connects important associated concepts within the subject matter. <i>For example, a lesson on multi-digit addition makes connections with place value, rather than simply showing how to add multi-digit numbers. Or a lesson designed to analyze how an author develops ideas across extended text would make connections among the various developmental steps and the various purposes the author has for the text.</i></li>
				<li>The object does not need to be augmented with additional explanation or materials.</li>
				<li>The main ideas of the subject matter addressed in the object are clearly identified for the learner.</li>
			</ul>
		</div>

		<div class="criterion" data-dim="dim3" data-val="-1">This rubric is <i>not applicable</i> (N/A) for an object that is not designed to support teachers in planning and/or presenting subject matter. It may be possible that an educator could find an application for such an object during a lesson, but that would not be the intended use.</div>
		<div class="criterion" data-dim="dim3" data-val="0">An object is rated <i>very weak or no value</i> for the utility of materials designed to support teaching if it is confusing, contains errors, is missing important elements, or is for some other reason simply not useful, in spite of an intention to be used as a support for teachers in planning or preparation.</div>
		<div class="criterion" data-dim="dim3" data-val="1">
			An object is rated <i>limited</i> for the utility of materials designed to support teaching if it includes a useful approach or idea to teach an important topic but falls short of <i>strong</i> for either one of two reasons:
			<ul>
				<li>The object is missing important elements (e.g. directions for some parts of a lesson are not included).</li>
				<li>Important elements do not function as they are intended to (e.g. directions are unclear or practice exercises are missing or inadequate). Teachers would need to supplement this object to use it effectively.</li>
			</ul>
		</div>
		<div class="criterion" data-dim="dim3" data-val="2">
			An object is rated <i>strong</i> for the utility of materials designed to support teaching if it offers materials that are comprehensive and easy to understand and use but falls short of <i>superior</i> for either one of two reasons: 
			<ul>
				<li>The object does not include suggestions for ways to use the materials with a variety of learners (e.g., error analysis tips).</li>
				<li>Some core components (e.g., directions) are underdeveloped in the object.</li>
			</ul>
		</div>
		<div class="criterion" data-dim="dim3" data-val="3">
			An object is rated <i>superior</i> for the utility of materials designed to support teaching only if <b>all</b> of the following are true:
			<ul>
				<li>The object provides materials that are comprehensive and easy to understand and use.</li>
				<li>The object includes suggestions for ways to use the materials with a variety of learners. These suggestions include materials such as “common error analysis tips” and “precursor skills and knowledge” that go beyond the basic lesson or unit elements.</li>
				<li>All objects and all components are provided and function as intended and described. For example, the time needed for lesson planning appears accurately estimated, materials lists are complete, and explanations make sense.</li>
				<li>For larger objects like units, materials facilitate the use of a mix of instructional approaches (direct instruction, group work, investigations, etc.).</li>
			</ul>
		</div>

		<div class="criterion" data-dim="dim4" data-val="-1">This rubric is <i>not applicable</i> (N/A) for an object that is not designed to have an assessment component. Even if one might imagine ways an object could be used for assessment purposes, if it is not the intended purpose, <i>not applicable</i> is the appropriate score.</div>
		<div class="criterion" data-dim="dim4" data-val="0">An object is rated <i>very weak or no value</i> for the quality of its assessments if its assessments contain significant errors, do not assess important content/skills, are written in a way that is confusing to students, or are unsound for other reasons.</div>
		<div class="criterion" data-dim="dim4" data-val="1">An object is rated <i>limited</i> for the quality of its assessments if it assesses some of the content or performance expectations intended, as stated or implicit in the object, but omits some important content or performance expectations and/or fails to offer the student opportunities to demonstrate proficiency in the intended content/skills.</div>
		<div class="criterion" data-dim="dim4" data-val="2">An object is rated <i>strong</i> for the quality of its assessments if it assesses all of the content and performance expectations intended, but the assessment modes used do not consistently offer the student opportunities to demonstrate proficiency in the intended concept/skill.</div>
		<div class="criterion" data-dim="dim4" data-val="3">
			An object is rated <i>superior</i> for the quality of its assessments only if <b>all</b> of the following are true:
			<ul>
				<li>All of the skills and knowledge assessed align clearly to the content and performance expectations intended, as stated or implied in the object.</li>
				<li>Nothing is assessed that is not included in the scope of intended material unless it is differentiated as extension material.</li>
				<li>The most important aspects of the expectations are targeted and are given appropriate weight/attention in the assessment.</li>
				<li>The assessment modes used in the object, such as selected response, long and short constructed response, or group work require the student to demonstrate proficiency in the intended concept/skill.</li>
				<li>The level of difficulty is a result of the complexity of the subject-area content and performance expectations and of the degree of cognitive demand, rather than a result of unrelated issues (e.g. overly complex vocabulary used in math word problems).</li>
			</ul>
		</div>

		<div class="criterion" data-dim="dim5" data-val="-1">This rubric is <i>not applicable</i> (N/A) for an object that does not have an interactive technological element. <i>For example, the rubric does not apply if interaction with the object is limited to, for example, opening a user-selected PDF.</i></div>
		<div class="criterion" data-dim="dim5" data-val="0">An object, or interactive component of an object, is rated <i>very weak or no value</i> for the quality of its technological interactivity if it has interactive features that are poorly conceived and/or executed. The interactive features might fail to operate as intended, distract the user, or unnecessarily take up user time.</div>
		<div class="criterion" data-dim="dim5" data-val="1">An object, or interactive component of an object, is rated <i>limited</i> for the quality of its technological interactivity if its interactive element does not relate to the subject matter and may detract from the learning experience. These kinds of interactive elements may slightly increase motivation but do not provide strong support for understanding the subject matter addressed in the object. It is unlikely that this interactive feature will increase understanding or extend the time a user engages with the content.</div>
		<div class="criterion" data-dim="dim5" data-val="2">An object, or interactive component of an object, is rated <i>strong</i> for the quality of its technological interactivity if it has an interactive feature that is purposeful and directly related to learning, but does not provide an individualized learning experience. Similarly to the <i>superior</i> objects, <i>strong</i> interactive objects must be well designed, easy-to-use, and function flawlessly on the intended platform. Some technological elements may not be directly related to the content but for a <i>strong</i> rating they must not detract from the learning experience. These kinds of interactive elements, including earning points or achieving levels for correct answers, might be designed to increase student motivation and to build content understanding by rewarding or entertaining the learner, and may extend the time the user engages with the content. </div>
		<div class="criterion" data-dim="dim5" data-val="3">
			An object, or interactive component of an object, is rated <i>superior</i> for the quality of its technological interactivity only if <b>all</b> of the following are true: 
			<ul>
				<li>The object is responsive to student input in a way that creates an individualized learning experience. This means the object adapts to the user based on what s/he does, or the object allows the user some flexibility or individual control during the learning experience.</li>
				<li>The interactive element is purposeful and directly related to learning.</li>
				<li>The object is well-designed and easy to use, encouraging learner use.</li>
				<li>The object appears to function flawlessly on the intended platform.</li>
			</ul>
		</div>

		<div class="criterion" data-dim="dim6" data-val="-1">This rubric is not applicable (N/A) to an object that does not include opportunities to practice targeted skills.</div>
		<div class="criterion" data-dim="dim6" data-val="0">An object is rated <i>very weak or no value</i> for the quality of its instructional and practice exercises if the exercises provided do not facilitate mastery of the targeted skills, contain 
errors, or are unsound for other reasons.</div>
		<div class="criterion" data-dim="dim6" data-val="1">An object is rated <i>limited</i> for the quality of its instructional and practice exercises if it has some, but too few exercises to facilitate mastery of the targeted skills, is without answer keys, and provides no variation in type or format.</div>
		<div class="criterion" data-dim="dim6" data-val="2">An object is rated <i>strong</i> for the quality of its instructional and practice exercises if it offers only a sufficient number of well-written exercises to facilitate mastery of targeted skills, which are supported by accurate answer keys or scoring guidelines, but there is little variety of exercise types or formats.</div>
		<div class="criterion" data-dim="dim6" data-val="3">
      An object is rated <i>superior</i> for the quality of its instructional and practice exercises only if all of the following are true: 
      <ul>
        <li>The object offers more exercises than needed for the average student to facilitate mastery of the targeted skills, as stated or implied in the object. For complex tasks, one or two rich practice exercises may be considered more than enough.</li>
        <li>The exercises are clearly written and supported by accurate answer keys or scoring guidelines as applicable.</li>
        <li>There are a variety of exercise types <b>and/or</b> the exercises are available in a variety of formats, as appropriate to the targeted concepts and skills. For more complex practice exercises the formats used provide an opportunity for the learner to integrate a variety of skills.</li>
      </ul>
		</div>

		<div class="criterion" data-dim="dim7" data-val="-1">This rubric is <i>not applicable</i> (N/A) to an object that does not appear to be designed to provide the opportunity for deeper learning, even though one might imagine how it could be used to do so.</div>
		<div class="criterion" data-dim="dim7" data-val="0">An object is rated <i>very weak</i> for its opportunities for deeper learning if it appears to be designed to provide some of the deeper learning opportunities identified in this rubric, but it is not useful as it is presented. <i>For example, the object might be based on poorly formulated problems and/or unclear directions, making it unlikely that this lesson or activity will lead to skills like critical thinking, abstract reasoning, constructing arguments, or modeling.</i></div>
		<div class="criterion" data-dim="dim7" data-val="1">An object is rated <i>limited</i> for its opportunities for deeper learning if it includes one deeper learning skill identified in the rubric but is missing clear guidance on how to tap into the various aspects of deeper learning. <i>For example, an object might include a provision for learners to collaborate, but the process and product are unclear.</i></div>
		<div class="criterion" data-dim="dim7" data-val="2">An object is rated <i>strong</i> for its opportunities for deeper learning if it includes one or two deeper learning skills identified in this rubric. <i>For example, the object might involve a complex problem that requires abstract reasoning skills to reach a solution.</i></div>
		<div class="criterion" data-dim="dim7" data-val="3">
      An object is rated <i>superior</i> for its opportunities for deeper learning only if all of the following are true:
      <ul>
        <li>At least three of the deeper learning skills from the list identified in this rubric are required in the object.</li>
        <li>The object offers a range of cognitive demand that is appropriate and supportive of the material.</li>
        <li>Appropriate scaffolding and direction are provided.</li>
      </ul> 
  </div>

		<div class="criterion" data-dim="dim8" data-val="-1">Dimension 8 Not applicable</div>
		<div class="criterion" data-dim="dim8" data-val="0">Dimension 8 Value 0</div>
		<div class="criterion" data-dim="dim8" data-val="1">Dimension 8 Value 1</div>
		<div class="criterion" data-dim="dim8" data-val="2">Dimension 8 Value 2</div>
		<div class="criterion" data-dim="dim8" data-val="3">Dimension 8 Value 3</div>


	</div>

</div>
<div id="error" runat="server">
  <p style="text-align: center; padding: 50px;"><asp:Literal ID="errorMessage" runat="server" /></p>
</div>