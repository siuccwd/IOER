<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CCSSELAK-2Rubric.ascx.cs" Inherits="ILPathways.Controls.Rubrics.CCSSELAK_2Rubric" %>

<script type="text/javascript" src="/controls/rubrics/shared/ccss.js"></script>
<script type="text/javascript">

</script>
<style type="text/css">

</style>

<div class="tab" data-tabID="intro">
  <h2>Introduction</h2>
  <p>This tool is a <i>derivative</i> of the <a href="http://www.achieve.org/files/K-2ELALiteracyEQuIPRubric-07-18-13_1.pdf" target="_blank">Achieve EQuIP Rubric</a> for ELA (Grades K-2). It can be used with any type of Resource.</p>
  <p>To use this tool, select each applicable rubric from the scoreboard above and review the criteria for each rating. Select the rating that best applies to the Resource.</p>
	<p>For full details, please review the document linked above. Note that this tool omits the first dimension of the rubric, as Standards Alignment ratings are done on the Resource detail page you came from.</p>
</div>

<div class="tab dimension" data-tabID="14">
  <h2>Key Shifts in the CCSS</h2>
  <p>Please rate how well the Resource meets the following criteria:</p>
  <ul>
    <li><b>Reading Text Closely:</b> Makes reading text(s) closely (including read alouds) a central focus of instruction and includes regular opportunities for students to ask and answer text-dependent questions.</li>
    <li><b>Text-Based Evidence:</b> Facilitates rich text-based discussions and writing through specific, thought-provoking questions about common texts (including read alouds and, when applicable, illustrations, audio/video and other media).</li>
    <li><b>Academic Vocabulary:</b> Focuses on explicitly building students’ academic vocabulary and concepts of syntax throughout instruction.</li>
  </ul>
  <p>A unit or longer lesson should:</p>
  <ul>
    <li><b>Grade-Level Reading:</b> Include a progression of texts as students learn to read (e.g., additional phonic patterns are introduced, increasing sentence length). Provides text-centered learning that is sequenced, scaffolded and supported to advance students toward independent grade-level reading. </li>
    <li><b>Balance of Texts:</b> Focus instruction equally on literary and informational texts as stipulated in the CCSS (p.5) and indicated by instructional time (may be more applicable across a year or several units). </li>
    <li><b>Balance of writing:</b> Include prominent and varied writing opportunities for students that balance communicating thinking and answering questions with self-expression and exploration.</li>
  </ul>
  
  <div class="rating" data-ratingID="dimension14" data-dimensionID="14"></div>
</div>

<div class="tab dimension" data-tabID="15">
  <h2>Instructional Supports</h2>
  <p>Please rate how well the Resource meets the following criteria:</p>
  <ul>
    <li>Cultivates student interest and engagement in reading, writing and speaking about texts.</li>
    <li>Addresses instructional expectations and is easy to understand and use for teachers (e.g., clear directions, sample proficient student responses, sections that build teacher understanding of the whys and how of the material).</li>
    <li>Integrates targeted instruction in multiple areas such as grammar and syntax, writing strategies, discussion rules and aspects of foundational reading. o Provides substantial materials to support students who need more time and attention to achieve automaticity with decoding, phonemic awareness, fluency and/or vocabulary acquisition.</li>
    <li>Provides all students (including emergent and beginning readers) with extensive opportunities to engage with grade-level texts and read alouds that are at high levels of complexity including appropriate scaffolding so that students directly experience the complexity of text.</li>
    <li>Focuses on sections of rich text(s) (including read alouds) that present the greatest challenge; provides discussion questions and other supports to promote student engagement, understanding and progress toward independence. </li>
    <li>Integrates appropriate, extensive and easily implemented supports for students who are ELL, have disabilities and/or read or write below grade level. o Provides extensions and/or more advanced text for students who read or write above grade level.</li>
  </ul>
  <p>A unit or longer lesson should:</p>
  <ul>
    <li>Include a progression of learning where concepts, knowledge and skills advance and deepen over time (may be more applicable across the year or several units).</li>
    <li>Gradually remove supports, allowing students to demonstrate their independent capacities (may be more applicable across the year or several units).</li>
    <li>Provide for authentic learning, application of literacy skills and/or student-directed inquiry.</li>
    <li>Indicate how students are accountable for independent engaged reading based on student choice and interest to build stamina, confidence, and motivation (may be more applicable across the year or several unis).</li>
    <li>Use technology and media to deepen learning and draw attention to evidence and use texts as appropriate.</li>
  </ul>
  <div class="rating" data-ratingID="dimension15" data-dimensionID="15"></div>
</div>

<div class="tab dimension" data-tabID="16">
  <h2>Assessment</h2>
  <p>Please rate how well the Resource meets the following criteria:</p>
  <ul>
    <li>Elicits direct, observable evidence of the degree to which a student can independently demonstrate foundational skills and targeted grade level literacy CCSS (e.g., reading, writing, speaking and listening and/or language).</li>
    <li>Assesses student proficiency using methods that are unbiased and accessible to all students.</li>
    <li>Includes aligned rubrics or assessment guidelines that provide sufficient guidance for interpreting student performance and responding to areas where students are not yet meeting standards.</li>
  </ul>
  <p>A unit or longer lesson should:</p>
  <ul>
    <li>Use varied modes of assessment, arranging of pre-, formative, summative, and self-assessment measures.</li>
  </ul>
  <div class="rating" data-ratingID="dimension16" data-dimensionID="16"></div>
</div>

<div id="template_radio" style="display: none;">
  <label for="{rid}_rating{value}"><input type="radio" name="{rid}" id="{rid}_rating{value}" value="{value}" /> {name}</label>
</div>