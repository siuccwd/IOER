<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CCSSELA3-12Rubric.ascx.cs" Inherits="ILPathways.Controls.Rubrics.CCSSELA3_12Rubric" %>

<script type="text/javascript" src="/controls/rubrics/shared/ccss.js"></script>
<script type="text/javascript">

</script>
<style type="text/css">

</style>

<div class="tab" data-tabID="intro">
  <h2>Introduction</h2>
  <p>This tool is a <i>derivative</i> of the <a href="http://www.achieve.org/files/EQuIP-ELArubric-06-24-13-FINAL.pdf" target="_blank">Achieve EQuIP Rubric</a> for ELA (Grades 3-12). It can be used with any type of Resource.</p>
  <p>To use this tool, select each applicable rubric from the scoreboard above and review the criteria for each rating. Select the rating that best applies to the Resource.</p>
	<p>For full details, please review the document linked above. Note that this tool omits the first dimension of the rubric, as Standards Alignment ratings are done on the Resource detail page you came from.</p>
</div>

<div class="tab dimension" data-tabID="18">
  <h2>Key Shifts in the CCSS</h2>
  <p>Please rate how well the Resource meets the following criteria:</p>
  <ul>
    <li><b>Reading Text Closely:</b> Makes reading text(s) closely, examining textual evidence, and discerning deep meaning a central focus of instruction.</li>
    <li><b>Text-Based Evidence:</b> Facilitates rich and rigorous evidence-based discussions and writing about common texts through a sequence of specific, thought-provoking, and text-dependent questions (including, when applicable, questions about illustrations, charts, diagrams, audio/video, and media).</li>
    <li><b>Writing From Sources:</b> Routinely expects that students draw evidence from texts to produce clear and coherent writing that informs, explains, or makes an argument in various written forms (e.g., notes, summaries, short responses, or formal essays).</li>
    <li><b>Academic Vocabulary:</b> Focuses on building students’ academic vocabulary in context throughout instruction.</li>
  </ul>
  <p>A unit or longer lesson should:</p>
  <ul>
    <li><b>Increasing Text Complexity:</b> Focus students on reading a progression of complex texts drawn from the grade-level band. Provide text-centered learning that is sequenced, scaffolded and supported to advance students toward independent reading of complex texts at the CCR level.</li>
    <li><b>Building Disciplinary Knowledge:</b> Provide opportunities for students to build knowledge about a topic or subject through analysis of a coherent selection of strategically sequenced, discipline-specific texts.</li>
    <li><b>Balance of Texts:</b> Within a collection of grade-level units a balance of informational and literary texts is included according to guidelines in the CCSS (p. 5).</li>
    <li><b>Balance of Writing:</b> Include a balance of on-demand and process writing (e.g., multiple drafts and revisions over time) and short, focused research projects, incorporating digital texts where appropriate.</li>
  </ul>

  <div class="rating" data-ratingID="dimension18" data-dimensionID="18"></div>
</div>

<div class="tab dimension" data-tabID="19">
  <h2>Instructional Supports</h2>
  <p>Please rate how well the Resource meets the following criteria:</p>
  <ul>
    <li>Cultivates student interest and engagement in reading, writing and speaking about texts.</li>
    <li>Addresses instructional expectations and is easy to understand and use.</li>
    <li>Provides all students with multiple opportunities to engage with text of appropriate complexity for the grade level; includes appropriate scaffolding so that students directly experience the complexity of the text.</li>
    <li>Focuses on challenging sections of text(s) and engages students in a productive struggle through discussion questions and other supports that build toward independence.</li>
    <li>Integrates appropriate supports in reading, writing, listening and speaking for students who are ELL, have disabilities, or read well below the grade level text band.</li>
    <li>Provides extensions and/or more advanced text for students who read well above the grade level text band.</li>
  </ul>
  <p>A unit or longer lesson should:</p>
  <ul>
    <li>Include a progression of learning where concepts and skills advance and deepen over time (may be more applicable across the year or several units).</li>
    <li>Gradually remove supports, requiring students to demonstrate their independent capacities (may be more applicable across the year or several units).</li>
    <li>Provide for authentic learning, application of literacy skills, student-directed inquiry, analysis, evaluation and/or reflection.</li>
    <li>Integrate targeted instruction in such areas as grammar and conventions, writing strategies, discussion rules and all aspects of foundational reading for grades 3-5.</li>
    <li>Indicate how students are accountable for independent reading based on student choice and interest to build stamina, confidence and motivation (may be more applicable across the year or several units).</li>
    <li>Use technology and media to deepen learning and draw attention to evidence and texts as appropriate.</li>
  </ul>

  <div class="rating" data-ratingID="dimension19" data-dimensionID="19"></div>
</div>

<div class="tab dimension" data-tabID="20">
  <h2>Assessment</h2>
  <p>Please rate how well the Resource meets the following criteria:</p>
  <ul>
    <li>Elicits direct, observable evidence of the degree to which a student can independently demonstrate the major targeted grade-level CCSS standards with appropriately complex text(s).</li>
    <li>Assesses student proficiency using methods that are unbiased and accessible to all students.</li>
    <li>Includes aligned rubrics or assessment guidelines that provide sufficient guidance for interpreting student performance.</li>
  </ul>
  <p>A unit or longer lesson should:</p>
  <ul>
    <li>Use varied modes of assessment, arranging of pre-, formative, summative, and self-assessment measures.</li>
  </ul>
  <div class="rating" data-ratingID="dimension20" data-dimensionID="20"></div>
</div>

<div id="template_radio" style="display: none;">
  <label for="{rid}_rating{value}"><input type="radio" name="{rid}" id="{rid}_rating{value}" value="{value}" /> {name}</label>
</div>