<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CCSSMathRubric.ascx.cs" Inherits="IOER.Controls.Rubrics.CCSSMathRubric" %>

<script type="text/javascript" src="/controls/rubrics/shared/ccss.js"></script>
<script type="text/javascript">

</script>
<style type="text/css">

</style>

<div class="tab" data-tabID="intro">
  <h2>Introduction</h2>
  <p>This tool is a <i>derivative</i> of the <a href="http://www.achieve.org/files/EQuIPmathrubric-06-17-13_1.pdf" target="_blank">Achieve EQuIP Rubric</a> for Mathematics. It can be used with any type of Resource.</p>
  <p>To use this tool, select each applicable rubric from the scoreboard above and review the criteria for each rating. Select the rating that best applies to the Resource.</p>
	<p>For full details, please review the document linked above. Note that this tool omits the first dimension of the rubric, as Standards Alignment ratings are done on the Resource detail page you came from.</p>
</div>

<div class="tab dimension" data-tabID="10">
  <h2>Key Shifts in the CCSS</h2>
  <p>Please rate how well the Resource meets the following criteria:</p>
  <ul>
    <li><b>Focus:</b> Lessons and units targeting the major work of the grade provide an especially in-depth treatment, with especially high expectations. Lessons and units targeting supporting work of the grade have visible connection to the major work of the grade and are sufficiently brief. Lessons and units do not hold students responsible for material from later grades.</li>
    <li><b>Coherence:</b> The content develops through reasoning about the new concepts on the basis of previous understandings. Where appropriate, provides opportunities for students to connect knowledge and skills within or across clusters, domains and learning progressions.</li>
    <li><b>Rigor:</b> Requires students to engage with and demonstrate challenging mathematics with appropriate balance among the following:</li>
    <ul>
      <li><b>Application:</b> Provides opportunities for students to independently apply mathematical concepts in real-world situations and solve challenging problems with persistence, choosing and applying an appropriate model or strategy to new situations.</li>
      <li><b>Conceptual Understanding:</b> Develops students’ conceptual understanding through tasks, brief problems, questions, multiple representations and opportunities for students to write and speak about their understanding.</li> 
      <li><b>Procedural Skill and Fluency:</b> Expects, supports and provides guidelines for procedural skill and fluency with core calculations and mathematical procedures (when called for in the standards for the grade) to be performed quickly and accurately.</li>
    </ul>
  </ul>

  <div class="rating" data-ratingID="dimension10" data-dimensionID="10"></div>
</div>

<div class="tab dimension" data-tabID="11">
  <h2>Instructional Supports</h2>
  <p>Please rate how well the Resource meets the following criteria:</p>
  <ul>
    <li>Includes clear and sufficient guidance to support teaching and learning of the targeted standards, including, when appropriate, the use of technology and media.</li>
    <li>Uses and encourages precise and accurate mathematics, academic language, terminology and concrete or abstract representations (e.g., pictures, symbols, expressions, equations, graphics, models) in the discipline. o Engages students in productive struggle through relevant, thought-provoking questions, problems and tasks that stimulate interest and elicit mathematical thinking.</li>
    <li>Addresses instructional expectations and is easy to understand and use.</li>
    <li>Provides appropriate level and type of scaffolding, differentiation, intervention and support for a broad range of learners.</li>
    <ul>
      <li>Supports diverse cultural and linguistic backgrounds, interests and styles.</li>
      <li>Provides extra supports for students working below grade level.</li>
      <li>Provides extensions for students with high interest or working above grade level.</li>
    </ul>
  </ul>
  <p>A unit or longer lesson should:</p>
  <ul>
      <li>Recommend and facilitate a mix of instructional approaches for a variety of learners such as using multiple representations (e.g., including models, using a range of questions, checking for understanding, flexible grouping, pair-share).</li>
    <li>Gradually remove supports, requiring students to demonstrate their mathematical understanding independently.</li>
    <li>Demonstrate an effective sequence and a progression of learning where the concepts or skills advance and deepen over time.</li>
    <li>Expect, support and provide guidelines for procedural skill and fluency with core calculations and mathematical procedures (when called for in the standards for the grade) to be performed quickly and accurately.</li>
  </ul>

  <div class="rating" data-ratingID="dimension11" data-dimensionID="11"></div>
</div>

<div class="tab dimension" data-tabID="12">
  <h2>Assessment</h2>
  <p>Please rate how well the Resource meets the following criteria:</p>
  <ul>
    <li>Is designed to elicit direct, observable evidence of the degree to which a student can independently demonstrate the targeted CCSS.</li>
    <li>Asseses student proficiency using methods that are accessible and unbiased, including the use of grade-level language in student prompts.</li>
    <li>Includes aligned rubrics, answer keys, and scoring guidelines that provide sufficient guidance for interpreting student performance.</li>
  </ul>
  <p>A unit or longer lesson should:</p>
  <ul>
    <li>Use varied models of curriculum-embedded assessments that may include pre-, formative, summative, and self-assessment measures.</li>
  </ul>

  <div class="rating" data-ratingID="dimension12" data-dimensionID="12"></div>
</div>

<div id="template_radio" style="display: none;">
  <label for="{rid}_rating{value}"><input type="radio" name="{rid}" id="{rid}_rating{value}" value="{value}" /> {name}</label>
</div>