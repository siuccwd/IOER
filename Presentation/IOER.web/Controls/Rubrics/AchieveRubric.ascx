<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AchieveRubric.ascx.cs" Inherits="ILPathways.Controls.Rubrics.AchieveRubric" %>

<script type="text/javascript">
  //Initialization
  $(document).ready(function () {
    //Ratings
    $(".rating input[type=radio]").on("change", function () { 
      var button = $(this);
      showCriteriaDescription(button.attr("name")); 
      updateScore(parseInt(button.attr("value")), parseInt(button.parentsUntil(".rating").parent().attr("data-dimensionID")));
    });
    $(".rating input:last-child").trigger("change");

  }); // end document.ready

  //Page Functions
  function showCriteriaDescription(name) {
    var target = $(".rating[data-ratingID=" + name + "] input:checked").attr("id");
    $(".ratingDetails[data-ratingID=" + name + "] .ratingDetail").hide().filter("[data-for=" + target + "]").show();
  }

  //Scoring functions
  function updateScore(rating, id){
    if(isNaN(rating)){ rating = null; }
    else {
      rating = Math.ceil((rating / 3) * 100);
    }
    for(i in data.dimensions){
      if(data.dimensions[i].id == id){
        data.dimensions[i].score = rating;
      }
    }
    $(window).trigger("scoresUpdated");
  }

</script>

<style type="text/css">

</style>

<div class="tab" data-tabID="intro">
  <h2>Introduction</h2>
	<p>This tool is modeled on the <a href="http://www.achieve.org/files/AchieveOERRubrics.pdf" target="_blank">Achieve OER Rubrics Collection</a> and is designed to evaluate Resources hosted in the IOER site.</p>
	<p>To use this tool, select each applicable rubric from the scoreboard above and review the criteria for each rating. Select the rating that best applies to the Resource.</p>
	<p>For full details, please review the document linked above. Note that this tool omits the first dimension of the rubric, as Standards Alignment ratings are done on the Resource detail page you came from.</p>
</div>

<div class="tab dimension" data-tabID="2" data-dimensionID="2">
  <h2>Quality of Explanation of the Subject Matter</h2>
	<p>This rubric is applied to objects designed to explain subject matter. It is used to rate how thoroughly the subject matter is explained or otherwise revealed in the object. Teachers might use this object with a whole class, a small group, or an individual student. Students might use the object to self-tutor. For objects that are primarily intended for teacher use, the rubric is applied to the explanation of the subject matter not to the planning instructions for the teacher.</p>
  <div class="rating" data-ratingID="dimension2" data-dimensionID="2">
    <label for="dimension2_rating3"><input type="radio" name="dimension2" id="dimension2_rating3" value="3" /> Superior</label>
    <label for="dimension2_rating2"><input type="radio" name="dimension2" id="dimension2_rating2" value="2" /> Strong</label>
    <label for="dimension2_rating1"><input type="radio" name="dimension2" id="dimension2_rating1" value="1" /> Limited</label>
    <label for="dimension2_rating0"><input type="radio" name="dimension2" id="dimension2_rating0" value="0" /> Very Weak/None</label>
    <label for="dimension2_ratingNA"><input type="radio" name="dimension2" id="dimension2_ratingNA" value="NA" checked="checked" /> Not Applicable</label>
  </div>
  <div class="ratingDetails" data-ratingID="dimension2">
    <div class="ratingDetail" data-for="dimension2_rating3">
      An object is rated <i>superior</i> for explanation of subject matter only if <b>all</b> of the following are true:
			<ul>
				<li>The object provides comprehensive information so effectively that the target audience should be able to understand the subject matter.</li>
				<li>The object connects important associated concepts within the subject matter. <i>For example, a lesson on multi-digit addition makes connections with place value, rather than simply showing how to add multi-digit numbers. Or a lesson designed to analyze how an author develops ideas across extended text would make connections among the various developmental steps and the various purposes the author has for the text</i>.</li>
				<li>The object does not need to be augmented with additional explanation or materials.</li>
				<li>The main ideas of the subject matter addressed in the object are clearly identified for the learner.</li>
			</ul>
    </div>
    <div class="ratingDetail" data-for="dimension2_rating2">
      An object is rated <i>strong</i> for explanation of subject matter if it explains the subject matter in a way that makes skills, procedures, concepts, and/or information understandable. It falls short of <i>superior</i> in that it does not make connections among important associated concepts within the subject matter. <i>For example, a lesson on multi-digit addition may focus on the procedure and fail to connect it with place value</i>.
    </div>
    <div class="ratingDetail" data-for="dimension2_rating1">
      An object is rated <i>limited</i> for explanation of subject matter if it explains the subject matter correctly but in a limited way. This cursory treatment of the content is not sufficiently developed for a first-time learner of the content. The explanations are not thorough and would likely serve as a review for most learners.
    </div>
    <div class="ratingDetail" data-for="dimension2_rating0">
      An object is rated <i>very weak or no value</i> for explanation of subject matter if its explanations are confusing or contain errors. There is little likelihood that this object will contribute to understanding.
    </div>
    <div class="ratingDetail" data-for="dimension2_ratingNA">
      This rubric is <i>not applicable</i> (N/A) for an object that is not designed to explain subject matter, for example, a sheet of mathematical formulae or a map. It may be possible to apply the object in some way that aids a learner’s understanding, but that is beyond any obvious or described purpose of the object.
    </div>
  </div>
</div><!-- /dimension2 -->

<div class="tab dimension" data-tabID="3" data-dimensionID="3">
  <h2>Utility of Materials Designed to Support Teaching</h2>
	<p>This rubric is applied to objects designed to support teachers in planning or presenting subject matter. The primary user would be a teacher. This rubric evaluates the potential utility of an object at the intended grade level for the majority of instructors.</p>
  <div class="rating" data-ratingID="dimension3" data-dimensionID="3">
    <label for="dimension3_rating3"><input type="radio" name="dimension3" id="dimension3_rating3" value="3" /> Superior</label>
    <label for="dimension3_rating2"><input type="radio" name="dimension3" id="dimension3_rating2" value="2" /> Strong</label>
    <label for="dimension3_rating1"><input type="radio" name="dimension3" id="dimension3_rating1" value="1" /> Limited</label>
    <label for="dimension3_rating0"><input type="radio" name="dimension3" id="dimension3_rating0" value="0" /> Very Weak/None</label>
    <label for="dimension3_ratingNA"><input type="radio" name="dimension3" id="dimension3_ratingNA" value="NA" checked="checked" /> Not Applicable</label>
  </div>
  <div class="ratingDetails" data-ratingID="dimension3">
    <div class="ratingDetail" data-for="dimension3_rating3">
     An object is rated <i>superior</i> for the utility of materials designed to support teaching only if <b>all</b> of the following are true:
			<ul>
				<li>The object provides materials that are comprehensive and easy to understand and use.</li>
				<li>The object includes suggestions for ways to use the materials with a variety of learners. These suggestions include materials such as “common error analysis tips” and “precursor skills and knowledge” that go beyond the basic lesson or unit elements.</li>
				<li>All objects and all components are provided and function as intended and described. For example, the time needed for lesson planning appears accurately estimated, materials lists are complete, and explanations make sense.</li>
				<li>For larger objects like units, materials facilitate the use of a mix of instructional approaches (direct instruction, group work, investigations, etc.).</li>
			</ul>
    </div>
    <div class="ratingDetail" data-for="dimension3_rating2">
      An object is rated <i>strong</i> for the utility of materials designed to support teaching if it offers materials that are comprehensive and easy to understand and use but falls short of <i>superior</i> for either one of two reasons: 
			<ul>
				<li>The object does not include suggestions for ways to use the materials with a variety of learners (e.g., error analysis tips).</li>
				<li>Some core components (e.g., directions) are underdeveloped in the object.</li>
			</ul>
    </div>
    <div class="ratingDetail" data-for="dimension3_rating1">
      An object is rated <i>limited</i> for the utility of materials designed to support teaching if it includes a useful approach or idea to teach an important topic but falls short of <i>strong</i> for either one of two reasons:
			<ul>
				<li>The object is missing important elements (e.g. directions for some parts of a lesson are not included).</li>
				<li>Important elements do not function as they are intended to (e.g. directions are unclear or practice exercises are missing or inadequate). Teachers would need to supplement this object to use it effectively.</li>
			</ul>
    </div>
    <div class="ratingDetail" data-for="dimension3_rating0">
      An object is rated <i>very weak or no value</i> for the utility of materials designed to support teaching if it is confusing, contains errors, is missing important elements, or is for some other reason simply not useful, in spite of an intention to be used as a support for teachers in planning or preparation.
    </div>
    <div class="ratingDetail" data-for="dimension3_ratingNA">
      This rubric is <i>not applicable</i> (N/A) for an object that is not designed to support teachers in planning and/or presenting subject matter. It may be possible that an educator could find an application for such an object during a lesson, but that would not be the intended use.
    </div>
  </div>
</div><!-- /dimension3 -->

<div class="tab dimension" data-tabID="4" data-dimensionID="4">
  <h2>Quality of Assessment</h2>
	<p>This rubric is applied to those objects designed to determine what a student knows before, during, or after a topic is taught. When many assessment items are included in one object, as is often the case, the rubric is applied to the entire set.</p>
  <div class="rating" data-ratingID="dimension4" data-dimensionID="4">
    <label for="dimension4_rating3"><input type="radio" name="dimension4" id="dimension4_rating3" value="3" /> Superior</label>
    <label for="dimension4_rating2"><input type="radio" name="dimension4" id="dimension4_rating2" value="2" /> Strong</label>
    <label for="dimension4_rating1"><input type="radio" name="dimension4" id="dimension4_rating1" value="1" /> Limited</label>
    <label for="dimension4_rating0"><input type="radio" name="dimension4" id="dimension4_rating0" value="0" /> Very Weak/None</label>
    <label for="dimension4_ratingNA"><input type="radio" name="dimension4" id="dimension4_ratingNA" value="NA" checked="checked" /> Not Applicable</label>
  </div>
  <div class="ratingDetails" data-ratingID="dimension4">
    <div class="ratingDetail" data-for="dimension4_rating3">
     An object is rated <i>superior</i> for the quality of its assessments only if <b>all</b> of the following are true:
			<ul>
				<li>All of the skills and knowledge assessed align clearly to the content and performance expectations intended, as stated or implied in the object.</li>
				<li>Nothing is assessed that is not included in the scope of intended material unless it is differentiated as extension material.</li>
				<li>The most important aspects of the expectations are targeted and are given appropriate weight/attention in the assessment.</li>
				<li>The assessment modes used in the object, such as selected response, long and short constructed response, or group work require the student to demonstrate proficiency in the intended concept/skill.</li>
				<li>The level of difficulty is a result of the complexity of the subject-area content and performance expectations and of the degree of cognitive demand, rather than a result of unrelated issues (e.g. overly complex vocabulary used in math word problems).</li>
			</ul>
    </div>
    <div class="ratingDetail" data-for="dimension4_rating2">
      An object is rated <i>strong</i> for the quality of its assessments if it assesses all of the content and performance expectations intended, but the assessment modes used do not consistently offer the student opportunities to demonstrate proficiency in the intended concept/skill.
    </div>
    <div class="ratingDetail" data-for="dimension4_rating1">
      An object is rated <i>limited</i> for the quality of its assessments if it assesses some of the content or performance expectations intended, as stated or implicit in the object, but omits some important content or performance expectations and/or fails to offer the student opportunities to demonstrate proficiency in the intended content/skills.
    </div>
    <div class="ratingDetail" data-for="dimension4_rating0">
      An object is rated <i>very weak or no value</i> for the quality of its assessments if its assessments contain significant errors, do not assess important content/skills, are written in a way that is confusing to students, or are unsound for other reasons.
    </div>
    <div class="ratingDetail" data-for="dimension4_ratingNA">
      This rubric is <i>not applicable</i> (N/A) for an object that is not designed to have an assessment component. Even if one might imagine ways an object could be used for assessment purposes, if it is not the intended purpose, <i>not applicable</i> is the appropriate score.
    </div>
  </div>
</div><!-- /dimension4 -->

<div class="tab dimension" data-tabID="5" data-dimensionID="5">
  <h2>Quality of Technological Interactivity</h2>
	<p>This rubric is applied to objects designed with a technology-based interactive component. It is used to rate the degree and quality of the interactivity of that component. “Interactivity” is used broadly to mean that the object responds to the user, in other words, it behaves differently based on what the user does. This is not a rating for technology in general, but for technological <i>interactivity</i>. The rubric does not apply to interaction between students, but rather to how the technology responds to the individual user.</p>
  <div class="rating" data-ratingID="dimension5" data-dimensionID="5">
    <label for="dimension5_rating3"><input type="radio" name="dimension5" id="dimension5_rating3" value="3" /> Superior</label>
    <label for="dimension5_rating2"><input type="radio" name="dimension5" id="dimension5_rating2" value="2" /> Strong</label>
    <label for="dimension5_rating1"><input type="radio" name="dimension5" id="dimension5_rating1" value="1" /> Limited</label>
    <label for="dimension5_rating0"><input type="radio" name="dimension5" id="dimension5_rating0" value="0" /> Very Weak/None</label>
    <label for="dimension5_ratingNA"><input type="radio" name="dimension5" id="dimension5_ratingNA" value="NA" checked="checked" /> Not Applicable</label>
  </div>
  <div class="ratingDetails" data-ratingID="dimension5">
    <div class="ratingDetail" data-for="dimension5_rating3">
     An object, or interactive component of an object, is rated <i>superior</i> for the quality of its technological interactivity only if <b>all</b> of the following are true: 
			<ul>
				<li>The object is responsive to student input in a way that creates an individualized learning experience. This means the object adapts to the user based on what s/he does, or the object allows the user some flexibility or individual control during the learning experience.</li>
				<li>The interactive element is purposeful and directly related to learning.</li>
				<li>The object is well-designed and easy to use, encouraging learner use.</li>
				<li>The object appears to function flawlessly on the intended platform.</li>
			</ul>
    </div>
    <div class="ratingDetail" data-for="dimension5_rating2">
      An object, or interactive component of an object, is rated <i>strong</i> for the quality of its technological interactivity if it has an interactive feature that is purposeful and directly related to learning, but does not provide an individualized learning experience. Similarly to the <i>superior</i> objects, <i>strong</i> interactive objects must be well designed, easy-to-use, and function flawlessly on the intended platform. Some technological elements may not be directly related to the content but for a <i>strong</i> rating they must not detract from the learning experience. These kinds of interactive elements, including earning points or achieving levels for correct answers, might be designed to increase student motivation and to build content understanding by rewarding or entertaining the learner, and may extend the time the user engages with the content.
    </div>
    <div class="ratingDetail" data-for="dimension5_rating1">
      An object, or interactive component of an object, is rated <i>limited</i> for the quality of its technological interactivity if its interactive element does not relate to the subject matter and may detract from the learning experience. These kinds of interactive elements may slightly increase motivation but do not provide strong support for understanding the subject matter addressed in the object. It is unlikely that this interactive feature will increase understanding or extend the time a user engages with the content.
    </div>
    <div class="ratingDetail" data-for="dimension5_rating0">
      An object, or interactive component of an object, is rated <i>very weak or no value</i> for the quality of its technological interactivity if it has interactive features that are poorly conceived and/or executed. The interactive features might fail to operate as intended, distract the user, or unnecessarily take up user time.
    </div>
    <div class="ratingDetail" data-for="dimension5_ratingNA">
      This rubric is <i>not applicable</i> (N/A) for an object that does not have an interactive technological element. <i>For example, the rubric does not apply if interaction with the object is limited to, for example, opening a user-selected PDF</i>.
    </div>
  </div>
</div><!-- /dimension5 -->

<div class="tab dimension" data-tabID="6" data-dimensionID="6">
  <h2>Quality of Instructional and Practice Exercises</h2>
	<p>This rubric is applied to objects that contain exercises designed to provide an opportunity to practice and strengthen specific skills and knowledge. The purpose of these exercises is to deepen understanding of subject matter and to routinize foundational skills and procedures. When concepts and skills are introduced, providing a sufficient number of exercises to support skill acquisition is critical. However when integrating skills in complex tasks, the number of exercise problems is less important than their richness. These types of practice opportunities may include as few as one or two instructional exercises designed to provide practice applying specific concepts and/or skills. Sets of practice exercises are treated as a single object, with the rubric applied to an entire group.</p>
  <div class="rating" data-ratingID="dimension6" data-dimensionID="6">
    <label for="dimension6_rating3"><input type="radio" name="dimension6" id="dimension6_rating3" value="3" /> Superior</label>
    <label for="dimension6_rating2"><input type="radio" name="dimension6" id="dimension6_rating2" value="2" /> Strong</label>
    <label for="dimension6_rating1"><input type="radio" name="dimension6" id="dimension6_rating1" value="1" /> Limited</label>
    <label for="dimension6_rating0"><input type="radio" name="dimension6" id="dimension6_rating0" value="0" /> Very Weak/None</label>
    <label for="dimension6_ratingNA"><input type="radio" name="dimension6" id="dimension6_ratingNA" value="NA" checked="checked" /> Not Applicable</label>
  </div>
  <div class="ratingDetails" data-ratingID="dimension6">
    <div class="ratingDetail" data-for="dimension6_rating3">
     An object is rated <i>superior</i> for the quality of its instructional and practice exercises only if all of the following are true: 
      <ul>
        <li>The object offers more exercises than needed for the average student to facilitate mastery of the targeted skills, as stated or implied in the object. For complex tasks, one or two rich practice exercises may be considered more than enough.</li>
        <li>The exercises are clearly written and supported by accurate answer keys or scoring guidelines as applicable.</li>
        <li>There are a variety of exercise types <b>and/or</b> the exercises are available in a variety of formats, as appropriate to the targeted concepts and skills. For more complex practice exercises the formats used provide an opportunity for the learner to integrate a variety of skills.</li>
      </ul>
    </div>
    <div class="ratingDetail" data-for="dimension6_rating2">
      An object is rated <i>strong</i> for the quality of its instructional and practice exercises if it offers only a sufficient number of well-written exercises to facilitate mastery of targeted skills, which are supported by accurate answer keys or scoring guidelines, but there is little variety of exercise types or formats.
    </div>
    <div class="ratingDetail" data-for="dimension6_rating1">
      An object is rated <i>limited</i> for the quality of its instructional and practice exercises if it has some, but too few exercises to facilitate mastery of the targeted skills, is without answer keys, and provides no variation in type or format.
    </div>
    <div class="ratingDetail" data-for="dimension6_rating0">
      An object is rated <i>very weak or no value</i> for the quality of its instructional and practice exercises if the exercises provided do not facilitate mastery of the targeted skills, contain errors, or are unsound for other reasons.
    </div>
    <div class="ratingDetail" data-for="dimension6_ratingNA">
      This rubric is not applicable (N/A) to an object that does not include opportunities to practice targeted skills.
    </div>
  </div>
</div><!-- /dimension6 -->

<div class="tab dimension" data-tabID="7" data-dimensionID="7">
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
  <div class="rating" data-ratingID="dimension7" data-dimensionID="7">
    <label for="dimension7_rating3"><input type="radio" name="dimension7" id="dimension7_rating3" value="3" /> Superior</label>
    <label for="dimension7_rating2"><input type="radio" name="dimension7" id="dimension7_rating2" value="2" /> Strong</label>
    <label for="dimension7_rating1"><input type="radio" name="dimension7" id="dimension7_rating1" value="1" /> Limited</label>
    <label for="dimension7_rating0"><input type="radio" name="dimension7" id="dimension7_rating0" value="0" /> Very Weak/None</label>
    <label for="dimension7_ratingNA"><input type="radio" name="dimension7" id="dimension7_ratingNA" value="NA" checked="checked" /> Not Applicable</label>
  </div>
  <div class="ratingDetails" data-ratingID="dimension7">
    <div class="ratingDetail" data-for="dimension7_rating3">
     An object is rated <i>superior</i> for its opportunities for deeper learning only if all of the following are true:
      <ul>
        <li>At least three of the deeper learning skills from the list identified in this rubric are required in the object.</li>
        <li>The object offers a range of cognitive demand that is appropriate and supportive of the material.</li>
        <li>Appropriate scaffolding and direction are provided.</li>
      </ul> 
    </div>
    <div class="ratingDetail" data-for="dimension7_rating2">
      An object is rated <i>strong</i> for its opportunities for deeper learning if it includes one or two deeper learning skills identified in this rubric. <i>For example, the object might involve a complex problem that requires abstract reasoning skills to reach a solution</i>.
    </div>
    <div class="ratingDetail" data-for="dimension7_rating1">
      An object is rated <i>limited</i> for its opportunities for deeper learning if it includes one deeper learning skill identified in the rubric but is missing clear guidance on how to tap into the various aspects of deeper learning. <i>For example, an object might include a provision for learners to collaborate, but the process and product are unclear</i>.
    </div>
    <div class="ratingDetail" data-for="dimension7_rating0">
      An object is rated <i>very weak</i> for its opportunities for deeper learning if it appears to be designed to provide some of the deeper learning opportunities identified in this rubric, but it is not useful as it is presented. <i>For example, the object might be based on poorly formulated problems and/or unclear directions, making it unlikely that this lesson or activity will lead to skills like critical thinking, abstract reasoning, constructing arguments, or modeling</i>.
    </div>
    <div class="ratingDetail" data-for="dimension7_ratingNA">
      This rubric is <i>not applicable</i> (N/A) to an object that does not appear to be designed to provide the opportunity for deeper learning, even though one might imagine how it could be used to do so.
    </div>
  </div>
</div><!-- /dimension7 -->

