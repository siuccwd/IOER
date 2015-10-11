<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Rubrics.ascx.cs" Inherits="IOER.Pages.Developers.Rubrics" %>
 
<h2>Introduction</h2>

<p>Illinois is publishing resource evaluations using a derivative of the Achieve Open Education Resources Rubric and IOER's derivative of Achieve's EquIP CCSS Rubric to the Learning Registry.
    Achieve's EQuIP rubric is intended only for units and lessons, but is written in such a way that Illinois feels that it can be applied to more than just units and lessons.  We are using
    a derivative of the Achieve OER rubric as well, because we are not publishing evaluations using Rubric I or Rubric VIII of the Achieve OER rubric.  We are also using a different scale for
    the Achieve rubrics.  Achieve's rubrics are on a 0-3 scale, we are using a 0-100 scale.  Because of these changes, Illinois cannot say we are publishing with the Achieve rubrics, we
    must say that we are publishing with a derivative of those rubrics.
</p>
<p>Achievement Standards Network (ASN) is hosting the dot notations for the Achieve rubrics.  This information was made available to ASN using the 
    <a href="http://www.w3.org/2004/02/skos/" target="_blank">Simple Knowledge Organization System (SKOS)</a> schema in RDF/XML format by Southern Illinois University at Carbondale.  Links
    to the rubrics' SKOS data are at <a href="http://www.achievementstandards.org/content/technical-documentation" target="_blank">http://www.achievementstandards.org/content/technical-documentation</a>.
</p>
   
<h2>1. Common Core State Standards Organization</h2>
<p>Common Core State Standards seem to be organized along the following lines:</p>
<ul>
	<li>CCSS – the acronym for Common Core State Standards</li>
	<li>Subject – CCSS covers Math and English Language Arts/Literacy.</li>
	<li>Sub-subject (for example, Content, or Practice for Math, RL for ELA/Literacy – Reading Standards for Literature).</li>
	<li>Grade Level</li>
	<li>Standard Number</li>
</ul>

<h2>2. EQuIP Rubric Dot Notation Proposal for Individual Bulleted Items in the Rubric</h2>
<p>The EQuIP Rubric is available at <a href="http://www.achieve.org/EQuIP/" target="_blank">http://www.achieve.org/EQuIP/</a>. The CCSS Rubric is a derivative of the EQuIP Rubric.</p>
<p>The CCSS Rubric has a dot notation similar to that used for CCSS. To make this work for other rubrics as they are published, we have done something like this:</p>
<ul>
	<li>Acronym of Rubric – "EQR"</li>
	<li>Subject – subjects used are in quotes.</li>
	<ul>
		<li>"Math"</li>
		<li>ELA/Literacy (Grades 3-5) – "ELA"</li>
		<li>"ELA" (Grades 6-12)</li>
		<li>There is a rubric for K-2 ELA – "ELA".&nbsp; Dot notation for this rubric is under development.</li>
	</ul>
	<li>Grade Level</li>
	<ul>
		<li>Math is always "K-12"</li>
		<li>ELA is divided into 2 grade ranges: "K-2" and "3-12".</li>
	</ul>
	<li>Dimension</li>
	<ul>
		<li>Dimension I: "AD"</li>
		<li>Dimension II: "KS"</li>
		<li>Dimension III: "IS"</li>
		<li>Dimension IV: "AS"</li>
	</ul>
	<li>Criteria Number within dimension – assign in order as they are shown within the dimensions.</li>
</ul>

<p>For example, criteria #3 of Dimension 2 of the CCSS Math Rubric might be laid out as follows:</p> 
<ul>
   <li><b>EQR.Math.K-12.KS.3</b>. criteria #6 of the Dimension 3 of the CCSS ELA-Literacy rubric for grades 3-5 and 6-12 might be laid out as follows:</li>
   <li><b>EQR.ELA.3-12.IS.6</b>. Bullet #1 of Dimension 4 of the CCSS ELA-Literacy rubric for grades K-2 would be laid out as <b>EQR.ELA-Literacy.K-2.AS.1</b>.</li>
</ul>

<h3>2.1. CCSS Rubric Dot Notation Proposal for Dimension of the Rubric</h3>
<p>To publish ratings at the dimension level, simply leave off the bullet number. Thus for the three dimensions given in the previous section, the dot notation for these would be as follows:</p>
<ul>
	<li><b>EQR.Math.K-12.KS.3</b> becomes <b>EQR.Math.K-12.KS.</b></li>
	<li><b>EQR.ELA-Literacy.3-12.IS.6</b> becomes <b>EQR.ELA-Literacy.3-12.IS</b>.</li>
	<li><b>EQR.ELA-Literacy.K-2.AS.1</b> becomes <b>EQR.ELA-Literacy.K-2.AS</b>.</li>
</ul>

<h2>3. Achieve OER Rubric</h2>
<p>The Achieve OER Rubric is available at <a href="http://www.achieve.org/oer-rubrics/" target="_blank">http://www.achieve.org/oer-rubrics</a>. The Achieve OER rubric has a dot notation similar to that used for the EQuIP rubric:</p>
<ul>
  <li>Acronym of Rubric - "AOER"</li>
  <li>Rubric Identifier - (identifier is in quotes)
  <ul>
    <li>Rubric I: "DA"</li>
    <li>Rubric II: "ES"</li>
    <li>Rubric III: "ST"</li>
    <li>Rubric IV: "AS"</li>
    <li>Rubric V: "TI"</li>
    <li>Rubric VI: "IP"</li>
    <li>Rubric VII: "DL"</li>
    <li>Rubric VIII: "AA"</li>
  </ul>
  </li>
  <li>Criteria Number within dimension - assign in order as they are shown within the dimensions.  Applies to Rubric VIII only.</li>
</ul>

<h2>4. How ISLE intends to publish ratings to rubrics to the LR</h2>
<p>ISLE intends to publish ratings to the EQuIP CCSS rubric and AOER rubrics to the LR only at the Dimension Level at this time.  We will not be publishing ratings using dimension 1 of either rubric as we are publishing those to the standards alignment.  We are not publishing ratings using dimension (Rubric) VIII of the AOER rubric at this time.</p>

<h3>4.1 Dimension Level Ratings</h3>
<p>Certain aspects of the rubric only apply if the resource is a Unit of Study. An example is the ELA-Literacy rubric for grades 3-12, Dimension 2, bullet 5, which only applies to units. ISLE will publish ratings for the dimensions on a 0-100 scale. If a resource is not tagged as a unit, ratings for dimension 2 will be published on the first four bullets. If a resource is later tagged as unit, ratings for dimension 2 will then include the last four bullets instead of the first four. Both will be scaled to a 0-100 scale so they will be normalized.</p>

<h2>5. Paradata Samples</h2>
<h3>5.1 Ratings</h3>
<h4>5.1.1. Alignment to the Standard</h4>
<p>A resource can be aligned to multiple standards. Alignments to standards are published as follows:Dimension 1 of the CCSS rubric is the alignment to the rigor of the CCSS, and should therefore be handled as an alignment to a standard. Therefore a rating on a scale of 0-100 for dimension 1 as applied to CCSS.Math.Content.K.G.A.1 would not be published using the dot notation of EQR.Math.K-12.AD, but would be published using the CCSS.Math.Content.K.G.A.1 notation.</p>
<pre>
{ 
	"activity": 
	{ 
		"actor": 
		{
			"description": ["9","10","English Language Arts"],  
			"objectType": "Educator"  },
			"verb": 
			{
				"action": "rated",
				"date": "2013-03-01/2013-03-31",
				"context": 
				{
					"id": "<a href="http://url/to/ISLE/page/">http://url/to/ISLE/page/</a>",
					"description": "ISLE detail page",
					"objectType": "OER"
				},
				"measure": 
				{
					"sampleSize": 100,
					"scaleMin": 0,
					"scaleMax": 100,
					"value": 72.0 
				},
			},
		"object": {	"id": "<a href="http://url/to/resource/">http://URL/to/resource/</a>" },
		"related": 
		[
			{ "objectType": "Common Core State Standard", "id": "CCSS.Math.Content.K.G.A.1", "content": "Describe objects in the environment using names of shapes, and describe the relative positions of these objects using terms such as above, below, beside, in front of, behind, and next to."  }
		]  
	} 
}

</pre>

<h4>5.1.2. Dimensions of the CCSS Rubric</h4>
<p>At present ISLE will publish ratings for the rubric at the dimension level, not at the criteria level. As part of the CCSS rubrics, if dimension 1 is not rated as a "2" or "3" ("Exemplar" or "Exemplar if Improved"), the rest of the rubric is skipped.</p>
<p>A rating of 72 on a scale of 0-100 for math, dimension 2 would be published as follows:</p>
<pre>
{
	"activity": 
	{
		"actor": 
		{
			"description": ["9","10","English Language Arts"],
			"objectType": "Educator"  
		},
		"verb": 
		{  
			"action": "rated",
			"date": "2013-03-01/2013-03-31",
			"context":
			{
				"id": "<a href="http://url/to/ISLE/page/">http://url/to/ISLE/page/</a>",
				"description": "ISLE detail page",
				"objectType": "OER"
			},
			"measure": 
			{
				"sampleSize": 100,
				"scaleMin": 0,
				"scaleMax": 100,
				"value": 72.0
			},
		},
		"object": {	"id": "<a href="http://url/to/resource/">http://URL/to/resource/</a>"	},  
		"related": 
		[
			{
				"objectType": "EQuIP Rubric",
				"id": "EQR.Math.K-12.KS",
				"content": "II. Key Areas of Focus in the CCSS"
			}
		]
	}
}
</pre>

<h3>5.2 Comments</h3>
<p>Comments will be available for each dimension of the rubric (not criteria within a dimension). Comments related to a rubric dimension will be published to the Learning Registry using the LR Paradata 1.0 schema as follows:</p>
<pre>
{
	"activity":
	{
		"actor": 
		{
			"description": ["9","10"],
			"objectType": "educator"
		},
		"verb": 
		{
			"action": "commented",
			"date": "2013-03-01",
			"comment": "Some comment goes here",
			"context": 
		{ 
			"id": "<a href="http://url/to/ISLE/page/">http://url/to/ISLE/page/</a>",
			"description": "ISLE detail page",
			"objectType": "OER"
		}
	},   
	"related": 
	[
		{
			"objectType": "EQuIP Rubric",
			"id": "EQR.Math.K-12.KS",
			"content": "II. Key Areas of Focus in the CCSS"
		}
	],   
	"object": { "id": "<a href="http://url/to/resource/">http://URL/to/resource/</a>" }
	} 
}
</pre>
<h2>Appendix</h2>
<h3>A.1 Appendix A – Dot Notation of EQuIP Rubrics</h3>
<h4>A.1.1 Mathematics Rubric</h4>
<p>ASN is hosting this rubric. It is available at <a href="http://purl.org/ASN/scheme/EQuIPMathK-12/" target="_blank">http://purl.org/ASN/scheme/EQuIPMathK-12/</a>.</p>

<style type="text/css">
	table { width: 100%; }
	table td, table th { padding: 5px; border-bottom: 1px solid #CCC; vertical-align: top; }
	table tr:last-child td { border: none; }
	table th { text-align: left; background-color: #DDD; }
	table tr td:nth-child(1) { width: 66%; }
	table tr td:nth-child(2) { width: 18%; }
	table tr td:nth-child(3) { width: 16%; }
</style>
<table>
	<tbody>
		<tr><th>Text</th><th>Dot Notation</th><th>Notes</th></tr>
		<tr>
			<td><b>I. Alignment to the Depth of the CCSS</b></td>
			<td>EQR.Math.K-12.AD</td>
			<td>Dimension 1</td>
		</tr>
		<tr>
			<td>Targets a set of grade-level CCSS mathematics standard(s) to the full depth of the standards for teaching and learning</td>
			<td>EQR.Math.K-12.AD.1</td>
			<td></td>
		</tr>
		<tr>
			<td>Standards for Mathematical Practice that are central to the lesson are identified, handled in a grade appropriate way, and well connected to the content being addressed.</td>
			<td>EQR.Math.K-12.AD.2</td>
			<td></td>
		</tr>
		<tr>
			<td>Presents a balance of mathematical procedures and deeper conceptual understanding inherent in the CCSS.</td>
			<td>EQR.Math.K12.AD.3</td>
			<td></td>
		</tr>
		<tr>
			<td><b>II. Key Shifts in the CCSS</b></td>
			<td>EQR.Math.K-12.KS</td>
			<td>Dimension 2</td>
		</tr>
		<tr>
			<td><b>Focus:</b> Lessons and units targeting the major work of the grade (at the standard and cluster level) provide an especially in-depth treatment, with especially high expectations. Lessons and units targeting supporting work of the grade (at the standard and cluster level) have visible connection to the major work of the grade and are sufficiently brief. Lessons and units do not hold students responsible for material from later grades.</td>
			<td>EQR.Math.K-12.KS.1</td>
			<td></td>
		</tr>
		<tr>
			<td><b>Coherence:</b> The content develops through reasoning about the new concepts on the basis of previous understandings.</td>
			<td>EQR.Math.K-12.KS.2</td>
			<td></td>
		</tr>
		<tr>
			<td><b>Rigor:</b> Requires students to engage with and demonstrate challenging mathematics with appropriate balance among the following:<p>− Application: Provides opportunities for students to independently apply mathematical concepts in real world situations and problem solve with persistence, choosing and applying an appropriate model or strategy to new situations.</p><p>− Conceptual Understanding: Develops students' understanding through brief conceptual problems and questions, multiple representations and opportunities for students to write and speak about their understanding.</p><p>− Procedural Skill and Fluency: Expects, supports and provides guidelines for procedural skill and fluency with core calculations and mathematical procedures (when called for in the standards for the grade) to be performed quickly and accurately.</p></td>
			<td>EQR.Math.K-12.KS.3</td>
			<td></td>
		</tr>
		<tr>
			<td><b>III. Instructional Supports</b></td>
			<td>EQR.Math.K-12.IS</td>
			<td></td>
		</tr>
		<tr>
			<td>Includes clear and sufficient guidance to support teaching and learning of the targeted standards, including, when appropriate, the use of technology and media.</td>
			<td>EQR.Math.K-12.IS.1</td>
			<td></td>
		</tr>
		<tr>
			<td>Uses and encourages precise and accurate mathematics, academic language, terminology and concrete or abstract representations (e.g., pictures, symbols, expressions, equations, graphics, models) in the discipline.</td>
			<td>EQR.Math.K-12.IS.2</td>
			<td></td>
		</tr>
		<tr>
			<td>Engages students in productive struggle through relevant, thought-provoking questions, problems and tasks that stimulate interest and elicit mathematical thinking.</td>
			<td>EQR.Math.K-12.IS.3</td>
			<td></td>
		</tr>
		<tr>
			<td>Addresses instructional expectations and is easy to understand and use</td>
			<td>EQR.Math.K-12.IS.4</td>
			<td></td>
		</tr>
		<tr>
			<td>Provides appropriate level and type of scaffolding, differentiation, intervention, and support for a broad range of learners.</td>
			<td>EQR.Math.K-12.IS.5</td>
			<td></td>
		</tr>
		<tr>
			<td>Supports diverse cultural and linguistic backgrounds, interests and styles.</td>
			<td>EQR.Math.K-12.IS.6</td>
			<td></td>
		</tr>
		<tr>
			<td>Provides extra supports for students working below grade level.</td>
			<td>EQR.Math.K-12.IS.7</td>
			<td></td>
		</tr>
		<tr>
			<td>Provides extensions for students with high interest or working above grade level.</td>
			<td>EQR.Math.K-12.IS.8</td>
			<td></td>
		</tr>
		<tr>
			<td>Recommend and facilitate a mix of instructional approaches for a variety of learners such as using multiple representations (e.g., including models, using a range of questions, checking for understanding, flexible grouping, pair-share).</td>
			<td>EQR.Math.K-12.IS.9</td>
			<td>Applies to Units or longer lessons</td>
		</tr>
		<tr>
			<td>Gradually remove supports, requiring students to demonstrate their mathematical understanding independently.</td>
			<td>EQR.Math.K-12.IS.10</td>
			<td>Applies to Units or longer lessons</td>
		</tr>
		<tr>
			<td>Demonstrate an effective sequence and a progression of learning where the concepts or skills advance and deepen over time.</td>
			<td>EQR.Math.K-12.IS.11</td>
			<td>Applies to Units or longer lessons</td>
		</tr>
		<tr>
			<td>Expect, support and provide guidelines for procedural skill and fluency with core calculations and mathematical procedures (when called for in the standards for the grade) to be performed quickly and accurately.</td>
			<td>EQR.Math.K-12.IS.12</td>
			<td>Applies to Units or longer lessons</td>
		</tr>
		<tr>
			<td><b>IV. Assessment</b></td>
			<td>EQR.Math.K-12.AS</td>
			<td>Dimension 4</td>
		</tr>
		<tr>
			<td>Is designed to elicit direct, observable evidence of the degree to which a student can independently demonstrate the targeted CCSS.</td>
			<td>EQR.Math.K-12.AS.1</td>
			<td></td>
		</tr>
		<tr>
			<td>Assesses student proficiency using methods that are accessible and unbiased, including the use of grade-level language in student prompts.</td>
			<td>EQR.Math.K-12.AS.2</td>
			<td></td>
		</tr>
		<tr>
			<td>Includes aligned rubrics, answer keys and scoring guidelines that provide sufficient guidance for interpreting student performance.</td>
			<td>EQR.Math.K-12.AS.3</td>
			<td></td>
		</tr>
		<tr>
			<td>Use varied modes of curriculum embedded assessments that may include pre-, formative, summative and self-assessment measures.</td>
			<td>EQR.Math.K-12.AS.4</td>
			<td>Applies to Units or longer lessons</td>
		</tr>
	</tbody>
</table>

<h4>A.1.2. ELA/Literacy (Grades 3-5) and ELA (Grades 6-12)</h4>
<p>ASN is hosting this rubric. It is available at <a href="http://purl.org/ASN/scheme/EQuIPELA3-12/" target="_blank">http://purl.org/ASN/scheme/EQuIPELA3-12/</a>.</p>

<table>
	<tbody>
		<tr><th>Text</th><th>Dot Notation</th><th>Notes</th></tr>
		<tr><td><b>I. Alignment to the Depth of the CCSS</b></td><td>EQR.ELA.3-12.AD</td><td>Dimension 1</td></tr><tr><td>Targets a set of grade-level CCSS ELA/Literacy standards.</td><td>EQR.ELA.3-12.AD.1</td><td></td></tr>
		<tr><td>Includes a clear and explicit purpose for instruction.</td><td>EQR.ELA.3-12.AD.2</td><td></td></tr>
		<tr><td>Selects text(s) that measure within the grade-level text complexity band and are of sufficient quality and scope for the stated purpose (e.g., presents vocabulary, syntax, text structures, levels of meaning/purpose, and other qualitative characteristics similar to CCSS grade-level exemplars in Appendices A &amp; B).</td><td>EQR.ELA.3-12.AD.3</td><td></td></tr><tr><td>Integrate reading, writing, speaking and listening so that students apply and synthesize advancing literacy skills</td><td>EQR.ELA.3-12.AD.4</td><td>Applies to Units or longer lessons.</td></tr>
		<tr><td>(Grades 3-5) Build students' content knowledge and their understanding of reading and writing in social studies, the arts, science or technical subjects through the coherent selection of texts.</td><td>EQR.ELA.3-5.AD.5</td><td>Applies to Units or longer lessons, grades 3-5 only. <b>NOTE:</b> the grade range for the dot notation of this criteria is 3-5, not 3-12.</td></tr>
		<tr><td><b>II. Key Shifts in the CCSS</b></td><td>EQR.ELA.3-12.KS</td><td>Dimension 2</td></tr>
		<tr><td><b>Reading Text Closely:</b> Makes reading text(s) closely, examining textual evidence, and discerning deep meaning a central focus of instruction</td><td>EQR.ELA.3-12.KS.1</td><td></td></tr>
		<tr><td><b>Text-Based Evidence:</b> Facilitates rich and rigorous evidence-based discussions and writing about common texts through a sequence of specific, thought-provoking, and text-dependent questions (including, when applicable, questions about illustrations, charts, diagrams, audio/video, and media).</td><td>EQR.ELA.3-12.KS.2</td><td></td></tr>
		<tr><td>
            <b>Writing from Sources:</b> Routinely expects that students draw evidence from texts to produce clear and coherent writing that informs, explains, or makes an argument in various written forms (e.g., notes, summaries, short responses, or formal essays).</td><td>EQR.ELA.3-12.KS.3</td><td></td></tr>
		<tr><td><b>Academic Vocabulary:</b> Focuses on building students' academic vocabulary in context throughout instruction.</td><td>EQR.ELA.3-12.KS.4</td><td></td></tr>
		<tr><td>
            <b>Increasing Text Complexity:</b> Focus students on reading a progression of complex texts drawn from the grade-level band. Provide text centered learning that is sequenced, scaffolded and supported to advance students toward independent reading of complex texts at the CCR level.</td><td>EQR.ELA.3-12.KS.5</td><td>Applies to Units or longer lessons.</td></tr>
		<tr><td><p>
               <b>Building Disciplinary Knowledge:</b> Provide opportunities for students to build knowledge about a topic or subject through analysis of a </p><p>coherent selection of strategically sequenced, discipline-specific texts.</p></td><td>EQR.ELA.3-12.KS.6</td><td>Applies to Units or longer lessons.</td></tr>
		<tr><td>
            <b>Balance of Texts:</b> Within a collection of grade-level units a balance of informational and literary texts is included according to guidelines in the CCSS (p. 5).</td><td>EQR.ELA.3-12.KS.7</td><td>Applies to Units or longer lessons.</td></tr>
		<tr><td><p>
               <b>Balance of Writing:</b> Include a balance of on-demand and process writing (e.g., multiple drafts and revisions over time) and short, </p><p>focused research projects, incorporating digital texts where appropriate.</p></td><td>EQR.ELA.3-12.KS.8</td><td>Applies to Units or longer lessons.</td></tr>
		<tr><td><b>III. Instructional Supports</b></td><td>EQR.ELA.3-12.IS</td><td>Dimension 3</td></tr>
		<tr><td>Cultivates student interest and engagement in reading, writing and speaking about texts.</td><td>EQR.ELA.3-12.IS.1</td><td></td></tr><tr><td>Addresses instructional expectations and is easy to understand and use.</td><td>EQR.ELA.3-12.IS.2</td><td></td></tr><tr><td>Provides all students with multiple opportunities to engage with text of appropriate complexity for the grade level; includes appropriate scaffolding so that students directly experience the complexity of the text.</td><td>EQR.ELA.3-12.IS.3</td><td></td></tr><tr><td>Focuses on challenging sections of text(s) and engages students in a productive struggle through discussion questions and other supports that build toward independence.</td><td>EQR.ELA.3-12.IS.4</td><td></td></tr><tr><td>Integrates appropriate supports in reading, writing, listening and speaking for students who are ELL, have disabilities, or read well below the grade level text band.</td><td>EQR.ELA.3-12.IS.5</td><td></td></tr><tr><td>Provides extensions and/or more advanced text for students who read well above the grade level text band.</td><td>EQR.ELA.3-12.IS.6</td><td></td></tr><tr><td>Include a progression of learning where concepts and skills advance and deepen over time.</td><td>EQR.ELA.3-12.IS.7</td><td>Applies to Units or longer lessons.</td></tr><tr><td>Gradually remove supports, requiring students to demonstrate their independent capacities.</td><td>EQR.ELA.3-12.IS.8</td><td>Applies to Units or longer lessons.</td></tr><tr><td>Provide for authentic learning, application of literacy skills, student directed inquiry, analysis, evaluation and/or reflection.</td><td>EQR.ELA.3-12.IS.9</td><td>Applies to Units or longer lessons.</td></tr><tr><td>Integrate targeted instruction in such areas as grammar and conventions, writing strategies, discussion rules and all aspects of foundational reading for grades 3-5.</td><td>EQR.ELA.3-5.IS.10</td><td>Applies to Units or longer lessons, for grades 3-5 only. 
            <b>NOTE: </b>The grade range for this criteria is 3-5, not 3-12.</td></tr><tr><td>Include independent reading based on student choice and interest to build stamina, confidence and motivation; indicates how students are accountable for that reading.</td><td>EQR.ELA.3-12.IS.11</td><td>Applies to Units or longer lessons.</td></tr><tr><td>Use technology and media to deepen learning and draw attention to evidence and texts as appropriate.</td><td>EQR.ELA.3-12.IS.12</td><td>Applies to Units or longer lessons.</td></tr><tr><td>
            <b>IV.</b><b> </b>
            <b>Assessment</b></td><td>EQR.ELA.3-12.AS</td><td>Dimension 4</td></tr><tr><td>Elicits direct, observable evidence of the degree to which a student can independently demonstrate the major targeted grade-level CCSS standards with appropriately complex text(s).</td><td>EQR.ELA.3-12.AS.1</td><td></td></tr><tr><td>Assesses student proficiency using methods that are unbiased and accessible to all students.</td><td>EQR.ELA.3-12.AS.2</td><td></td></tr><tr><td>Includes aligned rubrics or assessment guidelines that provide sufficient guidance for interpreting student performance.</td><td>EQR.ELA.3-12.AS.3</td><td></td></tr><tr><td>Use varied modes of assessment, including a range of pre, formative, summative and self-assessment measures.</td><td>EQR.ELA.3-12.AS.4</td><td>Applies to Units or longer lessons.</td></tr>

	</tbody>
</table>

<h4>A.1.3. ELA/Literacy (Grades K-2)</h4>
<p>ASN is hosting this rubric. It is available at <a href="http://purl.org/ASN/scheme/EQuIPELAK-2/" target="_blank">http://purl.org/ASN/scheme/EQuIPELAK-2/</a>.</p>
    <table>
        <tbody>
            <tr>
               <th><b>Text</b></th>
               <th><b>Dot Notation</b></th>
               <th><b>Notes</b></th>
            </tr>
            <tr>
                <td><b>I. Alignment to the Depth of the CCSS</b></td>
                <td>EQR.ELA.K-2.AD</td>
                <td>IOER does not publish rubrics using this dimension.</td>
            </tr>
            <tr>
                <td>Targets a set of K-2 ELA/Literacy CCSS for teaching and learning.</td>
                <td>EQR.ELA.K-2.AD.1</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>Includes a clear and explicit purpose for instruction.</td>
                <td>EQR.ELA.K-2.AD.2</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>Selects quality text(s) that align with the requirements outlined in the standards, presents characteristics similar to CCSS K-2 exemplars (Appendix B), and are of sufficient scope for the stated purpose.</td>
                <td>EQR.ELA.K-2.AD.3</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>Provides opportunities for students to present ideas and information through writing and/or drawing and speaking experiences.</td>
                <td>EQR.ELA.K-2.AD.4</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>Emphasize the explicit, systematic development of foundational literacy skills (concepts of print, phonological awareness, the alphabetic principle, high frequyency sight words, and phonics).</td>
                <td>EQR.ELA.K-2.AD.5</td>
                <td>Applies to units or longer lessons.</td>
            </tr>
            <tr>
                <td>Regularly include specific fluency-building techniques supported by research (e.g., monitored partner reading, choral reading, repeated readings with text, following along in the text when teacher or other fluent reader is reading aloud, short timed practice that is slightly challenging to the reader).</td>
                <td>EQR.ELA.K-2.AD.6</td>
                <td>Applies to units or longer lessons.</td>
            </tr>
            <tr>
                <td>Integrate reading, writing, speaking and listening so that students apply and synthesize advancing literacy skills.</td>
                <td>EQR.ELA.K-2.AD.7</td>
                <td>Applies to units or longer lessons.</td>
            </tr>
            <tr>
                <td>Build students' content knowledge in social studies, the arts, science or technical subjects through a coherent sequence of texts and series of questions that build knowledge withyin the topic.</td>
                <td>EQR.ELA.K-2.AD.8</td>
                <td>Applies to units or longer lessons.</td>
            </tr>
            <tr>
                <td><b>II.</b><b> </b><b>Key Shifts in the CCSS</b></td>
                <td>EQR.ELA.K-2.KS</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td><b>Reading Text Closely:</b> Makes reading text(s) closely (including read alouds) a central focus of instruction and includes regular opportunities for students to ask and answer text-dependent questions.</td>
                <td>EQR.ELA.K-2.KS.1</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td><b>Text-Based Evidence:</b> Facilitates rich text-based discussions and writing through specific, thought-provoking questions about common texts (including read aloutds, and when applicable, illustrations, audio/video and other media).</td>
                <td>EQR.ELA.K-2.KS.2</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td><b>Academic Vocabulary:</b> Focuses on explicitly building students' academic vocabulary and concepts of syntax throughout instruction.</td>
                <td>EQR.ELA>K-2.KS.3</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td><b>Grade-Level Reading:</b> Include a progression of texts as students learn to read (e.g., additional phonic patterns are introduced, increasing sentence length).  Provides text-centered learning that is sequenced, scaffolded and supported to advance students toward independent grade-level reading.</td>
                <td>EQR.ELA.K-2.KS.4</td>
                <td>Applies to units or longer lessons.</td>
            </tr>
            <tr>
                <td><b>Balance of Texts:</b> Focus instruction equally on literary and informational texts as stipulated in the CCSS (p.5) and indicated by instructional time (<i>may be more applicable across a year or several units</i>).</td>
                <td>EQR.ELA.K-2.KS.5</td>
                <td>Applies to units or longer lessons.</td>
            </tr>
            <tr>
                <td><b>Balance of Writing:</b> Include prominent and varied writing opportunities for students that balance communicating thinking and answering questions with self-expression and exploration.</td>
                <td>EQR.ELA.K-2.KS.6</td>
                <td>Applies to units or longer lessons.</td>
            </tr>
            <tr>
                <td><b>III.</b><b> </b><b>Instructional Supports.</b></td>
                <td>EQR.ELA.K-2.IS</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>Cultivates student interest and engagement in reading, writing and speaking about texts.</td>
                <td>EQR.ELA.K-2.IS.1</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>Addresses instructional expectations and is easy to understand and use for teachers (e.g., clear directions, sample proficient student responses, sections that build teacher understanding of the whys and how of the material).</td>
                <td>EQR.ELA.K-2.IS.2</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>Integrates targeted instruction in multiple areas such as grammar and syntax, writing strategies, discussion rules and aspects of foundational reading.</td>
                <td>EQR.ELA.K-2.IS.3</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>Provides substantial materials to support students who need more time and attention to achieve automaticity with decoding, phonemic awareness, fluency and/or vocabulary acquisition.</td>
                <td>EQR.ELA.K-2.IS.4</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>Provides <i>all</i> students (including emergent and beginning readers) with extensive opportunities to engage with grade-level texts and read alouds that are at high levels of complexity including appropriate scaffolding so that students directly experience the complexity of text.</td>
                <td>EQR.ELA.K-2.IS.5</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>Focuses on sections of rich text(s) (including read alouds) that present the greatest challenge; provides discussion questions and other supports to promote student engagement, understanding and progress toward independence.</td>
                <td>EQR.ELA.K-2.IS.6</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>Integrates appropriate, extensive and easily implemented supports for students who are ELL, have disabilities and/or read or write below grade level.</td>
                <td>EQR.ELA.K-2.IS.7</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>Provides extensions and/or more advanced text for students who read or write above grade level.</td>
                <td>EQR.ELA.K-12.IS.8</td>
            </tr>
            <tr>
                <td>Include a progression of learning where concepts, knowledge and skills advance and deepen over time (<i>may be more applicable across the year or several units</i>).</td>
                <td>EQR.ELA.K-2.IS.9</td>
                <td>Applies to units or longer lessons.</td>
            </tr>
            <tr>
                <td>Gradually remove supports, allowing students to demonstrate their independent capacities (<i>may be more applicable across the year or several units</i>).</td>
                <td>EQR.ELA.K-2.IS.10</td>
                <td>Applies to units or longer lessons.</td>
            </tr>
            <tr>
                <td>Provide for authentic learning, application of literacy skills and/or student-directed inquiry.</td>
                <td>EQR.ELA.K-2.IS.11</td>
                <td>Applies to units or longer lessons.</td>
            </tr>
            <tr>
                <td>Indicate how students are accountable for independent engaged reading based on student choice and interest to build stamina, confidence and motivation (<i>may be more applicable across the year or several units</i>).</td>
                <td>EQR.ELA.K-2.IS.12</td>
                <td>Applies to units or longer lessons.</td>
            </tr>
            <tr>
                <td>Use technology and media to deepen learning and draw attention to evidence and texts as appropriate.</td>
                <td>EQR.ELA.K-2.IS.13</td>
                <td>Applies to units or longer lessons.</td>
            </tr>
            <tr>
                <td><b>IV.</b><b> </b><b>Assessment</b></td>
                <td>EQR.ELA.K-2.AS</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>Elicits direct, observable evidence of the degree to which a student can independently demonstrate foundational skills and targeted grade level literacy CCSS (e.g., reading, writing, speaking and listining and/or language).</td>
                <td>EQR.ELA.K-2.AS.1</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>Assesses student proficiency using methods that are unbiased and accessible to all students.</td>
                <td>EQR.ELA.K-2.AS.2</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>Includes aligned rubrics or assessment guidelines that provide sufficient guidance for interpreting student performance and responding to areas where students are not yet meeting standards.</td>
                <td>EQR.ELA.K-2.AS.3</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>Use varied modes of assessment, including a range of pre-, formative, summative and self-assessment measures.</td>
                <td>EQR.ELA.K-2.AS.4</td>
                <td>Applies to units or longer lessons.</td>
            </tr>
        </tbody>
    </table>

<h2>A.2. Attributions and Licensing</h2>
<p>This document is based on the EQuIP rubric, available at <a href="http://www.achieve.org/EQuIP/" target="_blank">http://www.achieve.org/EQuIP/</a>. This document is Copyright © 2013 by Southern Illinois University at Carbondale, and is licensed under the Creative Commons Attribution 3.0 Unported License, viewable at <a href="http://www.creativecommons.org/licenses/by/3.0/" target="_blank">http://www.creativecommons.org/licenses/by/3.0/</a>. If modified, please attribute Southern Illinois University at Carbondale and EQuIP, and re-title.</p>

   <h2>B.1 Appendix B - Dot Notation of Achieve OER Rubric</h2>
   <p>ASN hosts this rubric for us at <a href="http://purl.org/ASN/scheme/AOER/" target="_blank">http://purl.org/ASN/scheme/AOER/</a>. The scoring guide is not shown here, but is available at <a href="http://www.achieve.org/files/AchieveOERRubrics.pdf" target="_blank">http://www.achieve.org/files/AchieveOERRubrics.pdf</a>. All rubrics with the exception of Rubric VIII use a 0-3 scale, plus N/A. Rubric VIII uses Yes, No, and N/A.
   </p>
   <table>
       <tbody>
           <tr>
               <th><b>Text</b></th>
               <th><b>Dot Notation</b></th>
               <th><b>Notes</b></th>
           </tr>
           <tr>
               <td><p><strong>Rubric I: Degree of alignment to Standards</strong></p><p>This rubric is applied to learning objects that have suggested alignments to standards. It is used to rate the degree to which an individual object actually ​aligns to each proposed standard. The rubric was designed specifically for the Common Core State Standards, but can be used with any set of standards. Before the rubric can be applied, the assumption is that a user has proposed an alignment between the object and the selected standard(s).</p><p>There are two major aspects of standards that are vital to a meaningful alignment review: content and performance expectations. It is important that the content addressed in the object matches the content addressed in each proposed standard. Evaluating the alignment of the performances required in both the object and the standard is equally essential and should be considered along with the content.</p></td>
               <td>AOER.DA</td>
               <td>IOER will not publish to the Learning Registry with this notation.</td>
           </tr>
           <tr>
               <td><p><strong>Rubric II: Quality of Explanation of the Subject Matter</strong></p><p>This rubric is applied to objects designed to explain subject matter. It is used to rate how thoroughly the subject matter is explained or otherwise revealed in the object. Teachers might use this object with a whole class, a small group, or an individual student. Students might use the object to self-tutor. For objects that are primarily intended for teacher use, the rubric is applied to the explanation of the subject matter not to the planning instructions for the teacher.</p></td>
               <td>AOER.ES</td>
               <td>&nbsp;</td>
           </tr>
           <tr>
               <td><p><strong>Rubric III: Utility of Materials Designed to Support Teaching</strong></p><p>This rubric is applied to objects designed to support teachers in planning or presenting subject matter. The primary user would be a teacher. This rubric evaluates the potential utility of an object at the intended grade level for the majority of instructors.</p></td>
               <td>AOER.ST</td>
               <td>&nbsp;</td>
           </tr>
           <tr>
               <td><p><strong>Rubric IV: Quality of Assessments.</strong></p><p>This rubric is applied to those objects designed to determine what a student knows before, during, or after a topic is taught. When many assessment items are included in one object, as is often the case, the rubric is applied to the entire set.</p></td>
               <td>AOER.AS</td>
               <td>&nbsp;</td>
           </tr>
           <tr>
               <td><p><strong>Rubric V: Quality of Technology Interactivity</strong></p><p>This rubric is applied to objects designed with a technology-based interactive component. It is used to rate the degree and quality of the interactivity of that component. "Interactivity" is used broadly to mean that the object responds to the user, in other words, it behaves differently based on what the user does. This is not a rating for technology in general, but for technological <i>interactivity</i>. The rubric does not apply to interaction between students, but rather to how the technology responds to the individual user.</p></td>
               <td>AOER.TI</td>
               <td>&nbsp;</td>
           </tr>
           <tr>
               <td><p><strong>Rubric VI: Quality of Instructional and Practice Exercises</strong></p><p>This rubric is applied to objects that contain exercises designed to provide an opportunity to practice and strengthen specific skills and knowledge. The purpose of these exercises is to deepen understanding of subject matter and to routinize foundational skills and procedures. When concepts and skills are introduced, providing a sufficient number of exercises to support skill acquisition is critical. However when integrating skills in complex tasks, the number of exercise problems is less important than their richness. These types of practice opportunities may include as few as one or two instructional exercises designed to provide practice applying specific concepts and/or skills. Sets of practice exercises are treated as a single object, with the rubric applied to an entire group.</p></td>
               <td>AOER.IP</td>
               <td>&nbsp;</td>
           </tr>
           <tr>
               <td><p><strong>Rubric VII: Opportunities for Deeper Learning</strong></p><p>This rubric is applied to objects designed to engage learners in at least one of the following deeper learning skills, which can be applied across all content areas:</p><ul><li>Think critically and solve complex problems.</li><li>Work collaboratively.</li><li>Communicate effectively.</li><li>Learn how to learn.</li><li>Reason abstractly.</li><li>Construct viable arguments and critique the reasoning of others.</li><li>Apply discrete knowledge and skills to real-world situations.</li><li>Construct, use, or analyze models.</li></ul></td>
               <td>AOER.DL</td>
               <td>&nbsp;</td>
           </tr>
           <tr>
               <td><p><strong>Rubric VIII: Assurance of Accessibility Standards</strong></p><p>This rubric is used to assure materials are accessible to all students, including students identified as blind, visually impaired or print disabled, and those students who may qualify under the Chafee Amendment to the U.S. 1931 Act to Provide Books to the Adult Blind as Amended. It was developed to assess compliance with U.S. standards and requirements, but could be adapted to accommodate differences in other sets of requirements internationally.</p><p>Accessibility is critically important for all learners and should be considered in the design of all online materials. Identification of certain characteristics will assist in determining if materials will be fully accessible for all students. Assurance that materials are compliant with the standards, recommendations, and guidelines specified assists educators in the selection and use of accessible versions of materials that can be used with all students, including those with different kinds of challenges and assistive devices.</p><p>The Assurance of Accessibility Standards Rubric does not ask reviewers to make a judgment on the degree of object quality. Instead, it requests that a determination (yes/no) of characteristics be made that, together with assurance of specific Standards, may determine the degree to which the materials are accessible. Only those who feel qualified to make judgments about an object's accessibility should use this rubric.</p></td>
               <td>AOER.AA</td>
               <td>IOER is not utilizing this rubric at this time.</td>
           </tr>
           <tr>
               <td>Available in Tagged PDF Format</td>
               <td>AOER.AA.1</td>
               <td>Adobe maintains this standard.</td>
           </tr>
           <tr>
               <td>Available in ePUB format</td>
               <td>AOER.AA.2</td>
               <td>International Digital Publishing Forum maintains this standard.</td>
           </tr>
           <tr>
               <td>Accessible Course within an Open Learning Management System (LMS)</td>
               <td>AOER.AA.3</td>
               <td>Moodle maintains this standard.</td>
           </tr>
           <tr>
               <td>Accessible Course within another Learning Management System (LMS)</td>
               <td>AOER.AA.4</td>
               <td>LMS Provider maintains this standard.</td>
           </tr>
           <tr>
               <td>Available in an accessible media format and includes alternate text or subtitles.</td>
               <td>AOER.AA.5</td>
               <td>Provider or Publisher maintains this standard.</td>
           </tr>
           <tr>
               <td>Includes alternative text (image)</td>
               <td>AOER.AA.6</td>
               <td>Provider or Publisher maintains this standard.</td>
           </tr>
           <tr>
               <td>Includes captions and subtitles (video)</td>
               <td>AOER.AA.7</td>
               <td>Provider or Publisher maintains this standard.</td>
           </tr>
           <tr>
               <td>Includes flash accessibility functions (SWF)</td>
               <td>AOER.AA.8</td>
               <td>Adobe maintains this standard.</td>
           </tr>
           <tr>
               <td>Includes functionality that provide [sic] accessibility</td>
               <td>AOER.AA.9</td>
               <td>Provider or Publisher maintains this standard.</td>
           </tr>
           <tr>
               <td>Complies with WC3 [sic] WCAG2 Recommendations for Web Pages</td>
               <td>AOER.AA.10</td>
               <td>Should be W3C not WC3.  W3C maintains this standard.</td>
           </tr>
           <tr>
               <td>Compliant with Section 508 of the Rehabilitation Act</td>
               <td>AOER.AA.11</td>
               <td>The US Government maintains this standard.</td>
           </tr>
           <tr>
               <td>Available in National Accessible Instructional Materials Standard (NIMAS) format - Accessible XML</td>
               <td>AOER.AA.12</td>
               <td>NIMAS Center at CAST maintains this standard.</td>
           </tr>
           <tr>
               <td>Complies with Video/Audio Cassette Production Standards</td>
               <td>AOER.AA.13</td>
               <td>ITA Standards maintains this standard.</td>
           </tr>
           <tr>
               <td>Complies with DVD/DVD-ROM Production Standards</td>
               <td>AOER.AA.14</td>
               <td>DVD Forum maintains this standard.</td>
           </tr>
           <tr>
               <td>Complies with Blue-ray [sic] Disk Production Standards</td>
               <td>AOER.AA.15</td>
               <td>It's actually Blu-ray, not Blue-ray.  UDF 2.5 - Blu-Ray Disk Association maintains this standard.</td>
           </tr>
           <tr>
               <td>Complies with NCAM Guidelines for Movies, Web, and Multimedia</td>
               <td>AOER.AA.16</td>
               <td>The Carl and Ruth Shapiro Family National Center for Accessible Media at Boston public broadcaster WGBH maintains this standard.</td>
           </tr>
       </tbody>
   </table>

   <h2>B.2 - Attributions and Licensing of Achieve OER Rubric</h2>
   <p>This document is based on the Achieve Rubrics for Evaluating Open Education Resources, available at <a href="http://www.achieve.org/files/AchieveOERRubrics.pdf" target="_blank">http://www.achieve.org/files/AchieveOERRubrics.pdf</a> and <a href="http://www.achieve.org/oer-rubrics/" target="_blank">http://www.achieve.org/oer-rubrics/</a>.  The rubrics are licensed under the Creative Commons Attribution 3.0 Unported License.</p>
   <p>This document is Copyright &copy; 2014 by Southern Illinois University at Carbondale, and is licensed under the Creative Commons Attribution 3.0 Unported License.  If modified, please attribute Southern Illinois University at Carbondale and Achieve Rubrics for Evaluating Open Education Resources, and re-title.</p>
   <p>The Creative Commons Attribution 3.0 Unported License is viewable at <a href="http://www.creativecommons.org/licenses/by/3.0/" target="_blank">http://www.creativecommons.org/licenses/by/3.0/</a></p>

<h2>Paradata Decoded</h2>
<h3>1. Overview</h3>
<p>There are four main parts to a basic paradata statement:</p>
<ul>
   <li>actor</li>
   <li>verb</li>
   <li>object</li>
   <li>related</li>
</ul>
<h3>2. Actor</h3>
<p>The actor is the description of the entity or persona that performed the activity. Actors contain an object type, and one or more description. An actor might be "An educator of grades 9, 10, 11, and 12." The object type would be "educator" and the descriptions would be 9, 10, 11, and 12.</p>
<h3>3. Verb</h3>
<p>The verb is either a string or an object. A verb might simply be "taught." So "an educator", "taught", "the lesson at some URL." A verb which is an object would contain an action (taught), a date (today), and a measure (how many times the teacher taught). <b>There can be only one verb per paradata document.</b> This precludes having a standard alignment (with an action of "aligned" or "matched") with a rating (degree of alignment), and (generally) precludes having a comment tied to a rating. Standards alignments and ratings would have an object rather than a string for the verb. This is necessary so the scale (minimum and maximum possible rating), number of ratings, and average rating can be included with the verb. </p><p>A verb can contain a context. A context typically is where the action was performed. An example context would be "the OER Commons detail page."</p>
<p>An example of a teacher bookmarking a resource on Delicious would have an actor of "Educator...", an action of "bookmarked", and a context of "Delicious"</p>
<h3>4. Object</h3>
<p>The object is what the paradata statement is about (for example, "<a href="http://url/to/lesson/">http://URL/to/lesson/</a>"). In the example of an educator bookmarking a resource on Delicious, the actor would be "Educator …", the action would be "bookmarked", the context would be "Delicious", and the object would be "http://URL/to/lesson/".</p>
<h3>5. Related</h3>
<p>Related is an array of objects. This is how you would tie a rating to a standard, thus showing degree of alignment, or how you would tie a rating to a rubric. At the present time, nobody is publishing a single rating record that ties a rating to a standard and a rubric for rating the standard. For example, OER Commons publishes multiple ratings. If a resource is aligned to three standards and six rubrics, they publish:</p>
<p>One rating for each of the three standards that shows how well the resource aligns to the standards.</p>
<p>One rating for each dimension of the rubric - in their case, "Quality of Explanation of the Subject Matter," "Utility of Materials Designed to Support Teaching," "Quality of Assessments", "Quality of Technological Interactivity," "Quality of Instructional and Practice Exercises," and "Opportunities for Deeper Learning."</p>
<p>LR Publishers such as OER Commons never ever publish a rating to both a rubric dimension that applies to a specific learning standard.</p>
<h3>6. Comments</h3>
<p>The LR supports comments about a resource. LR publishers (like OER Commons and CTE Online) publish comments about a resource. They do not publish comments about a rating. This is because ratings are published as a summary (e.g. "91 people rated this resource a 2.5 on a scale of 0-3). Because data incoming from the LR is summarized in this fashion, ISLE also summarizes ratings and displays them as an average rating, not as "50 people rated this a 1, 35 people rated it a 2, and 95 people rated it a 3. Publishing ratings as a summary precludes publishing comments tied to a rating.</p>
<p>Comments will be displayed in exactly one location on the page, and not as part of the rubric dimensions. Nothing prevents a user from commenting "I rated this a 1 for dimension 1 of the Tri-State Rubric because …"</p>
<h3>7. Publishing Resource Ratings</h3>
<p>Publishers routinely publish ratings about a resource in general. ISLE has taken the approach of showing Likes and Dislikes. At the present time, no LR publisher is publishing Likes and Dislikes to the LR. Instead they are publishing ratings on a numeric scale. Based on what is currently contained in the LR, ISLE will publish Likes and Dislikes as ratings. ISLE will publish two ratings summaries at a time, one for likes, and one for dislikes. This will allow others to consume our likes and dislikes and interpret them as ratings, or as likes and dislikes. Likes will be published with a rating of 3 on a scale of 0-3. Dislikes will be published with a rating of 0 on a scale of 0-3.</p>
<h3>8. User Interface for Standards Alignment and Degree of Alignment</h3>
<p>The LR paradata schema specification limits publishers to one verb per paradata statement. A single paradata statement cannot both align a resource to a standard, and rate the degree of alignment. This is done with two separate statements. The user interfaces at OER Commons and at ISLE treat standards alignment and rating a degree of alignment as two separate things, just as the LR does. In order to rate the degree of alignment to a standard, the resource must <b>first</b> be aligned to the standard. Therefore, for every degree of alignment rating of a resource to a standard in the LR, meta- or paradata aligning the resource to the standard should exist in the LR.</p>
<p>When an ISLE user aligns a new resource to a standard, metadata is included which aligns the resource to a standard. When the ISLE user aligns an already existing resource to a standard (such as an item published to the LR by NSDL), paradata will be published which aligns the resource to the standard.</p>
<p>Once the resource is aligned to the standard, ISLE users can rate the degree of alignment to the standard. These ratings will be periodically summarized and published to the LR.</p>
<h3>9. ISLE Storing of Likes/Dislikes and Publishing Them to the LR</h3>
<p>Individual likes and dislikes coming from our system will be stored in a different table from those that come in from the LR. This gives ISLE the flexibility to take a facebook-style approach to indicate to a user "You like this." It is also important from the standpoint of publishing likes and dislikes to the LR, as it is better to publish these in a summarized form rather than one paradata statement for each person rating. If 100 people like a resource, and they are published to the LR as individual paradata statements, that's a hundred statements that an LR consumer will have to consume, instead of consuming a single statement that contains a summary of the activity. ISLE will take the approach of publishing likes in one LR paradata statement, and dislikes in a second LR paradata statement. This will allow LR consumers to have the flexibility to interpret our likes/dislikes as ratings, or as likes/dislikes.</p>
<p>For example, suppose that between Feb. 1st and Feb. 28th, 2013, 100 people like resource A, and 50 people dislike it. We would publish this in the following fashion:</p>
<p>100 likes:</p>
<pre>
{ 
	"activity": 
	{
		"actor": 
		{  
			"description": ["9","10"],  
			"objectType": "educator"  
		},
		"verb": 
		{
			"action": "rated",
			"date": "2013-02-01/2013-02-28",
			"context": 
			{
				"id": "<a href="http://url/to/resource/Detail/Page">http://url/to/resource/Detail/Page</a>",  
			},  
			"measure":
			{
				"sampleSize": 100,  
				"scaleMin": 0,  
				"scaleMax": 3,  
				"value": 3.0  
			},  
		},  
		"object": { "id": "<a href="http://url/to/resource/A">http://url/to/resource/A</a>"  },
		"content": "100 educators rated <a href="http://url/to/resource/A">http://url/to/resource/A</a> 3 on a scale from 0 to 3 during February 2013." 
	}
}
</pre>
<p>50 dislikes:</p>
<pre>
{ 
	"activity": 
	{
		"actor": 
		{  
			"description": ["9","10"],  
			"objectType": "educator"  
		},
		"verb": 
		{
			"action": "rated",
			"date": "2013-02-01/2013-02-28",
			"context": 
			{
				"id": "<a href="http://url/to/resource/Detail/Page">http://url/to/resource/Detail/Page</a>",  
			},  
			"measure":
			{
				"sampleSize": 50,  
				"scaleMin": 0,  
				"scaleMax": 3,  
				"value": 0.0  
			},  
		},  
		"object": { "id": "<a href="http://url/to/resource/A">http://url/to/resource/A</a>"  },
		"content": "50 educators rated <a href="http://url/to/resource/A">http://url/to/resource/A</a> 0 on a scale from 0 to 3 during February 2013." 
	}
}
</pre>
<p>10. Statistics on Ratings tied to Standards, general ratings, and rubrics</p><p>As of 2/28/2013 there are 6,920 rating summary rows for 444,695 resources (so about 1.5% of all resources have a rating).</p><p>803 resources have a general rating tied to them (0.18%).</p><p>909 resources have a rating tied to an OER Rubric (0.2%). There are 3788 ratings for OER Rubrics (0.6%).</p><p>666 resources have a rating tied to at least one CCSS (0.14%). There are 1512 ratings for CCSS standards (0.3%).</p><p>And finally, there are 664 resources with a rating tied to both a CCSS and an OER Rubric.</p>

<h2>Publishing Paradata</h2>
<h3>1. Overview</h3>
<p>Paradata is often published as a summary of activity to the Learning Registry (LR). Publishing the paradata as a summary where possible makes it easier for those consuming the LR to extract, transform, and load the data into their own database for analysis and searching. Items for which summarization is useful include ratings (general ratings as well as degree of alignment to a standard and ratings based on a rubric), likes and dislikes, resource views, and favorite counts.</p>
<p>In some limited cases, Paradata items can be published individually. An example of this would be comments, which generally cannot be summarized. Another example would be standards alignment.</p>
<p>The Illinois Shared Learning Environment (ISLE) will publish Paradata to the LR on a periodic basis. The period may vary depending upon the type of paradata being published and whether or not it is paradata that can be summarized. Periods will include daily, weekly, and monthly publishing.</p>

<h3>2. Views</h3>
<p>An example of a record where 50 people have viewed a resource during the month of March 2013:</p>
<pre>
{
	"activity": 
	{
		"actor": "multiple users",
		"verb": 
		{
			"action": "viewed",
			"date": "2013-03-01/2013-03-31",
			"measure": 
			{
				"measureType": "count",
				"value": 50
			}
		},
		"object": {  "id": "<a href="http://url/to/resource/">http://url/to/resource/</a>"  }
	}
}
</pre>

<h3>3. Favorites</h3>
<p>An example of a record where 50 people have favorited a resource (added it to a library) during the month of March 2013:</p>
<pre>
{
	"activity": 
	{
		"actor": "multiple users",
		"verb": 
		{
			"action": "favorited",
			"date": "2013-03-01/2013-03-31",
			"measure": 
			{
				"measureType": "count",
				"value": 50
			}
		},
		"object": {  "id": "<a href="http://url/to/resource/">http://url/to/resource/</a>"  }
	}
}
</pre>

<h3>4. Standards Alignment</h3>
<p>A teacher aligned a resource to a common core standard (in this case CCSS.Math.Content.K.CC.A.1):</p>
<pre>
{
	"activity": 
	{
		"actor": "educator",
		"verb": 
		{
			"action": "aligned",
			"date": "2013-03-01"
		},
		"related": [ 
			{ "object": { "objectType": "Common Core Standard",  "id": "CCSS.Math.Content.K.CC.A.1"  }  } 
		],
		"object": { "id": "<a href="http://url/to/resource/">http://url/to/resource/</a>"  }
	}
}
</pre>

<h3>5. Tags</h3>
<p>An example of a record where someone has tagged additional keywords to a resource:</p>
<pre>
{
	"activity": 
	{
		"actor":
		{
			"description": ["9","10"],
			"objectType": "educator"  
		},
		"verb": 
		{  
			"action": "tagged", 
			"date": "2013-03-01",
			"context": 
			{  
				"id": "http://some.isle.page/",  
				"description": "ISLE Detail Page",  
				"objectType": "OER"
			},
			"description": ["physics","chemistry"]  
		},
		"object": {  "id": "<a href="http://url/to/resource">http://URL/to/resource</a>"  }
	}
}
</pre>

<h3>6. Comments</h3>
<p>Each individual comment will need to be published separately. There's no good way to summarize these, but they will be published probably on a monthly basis, just as ratings are.</p>
<pre>
{
	"activity": 
	{
		"actor": "educator",
		"verb": 
		{
			"action": "commented",
			"date": "2013-03-01",
			"comment": "This resource is a good one for working with kids who have trouble reading."
		},
		"object": {  "id": "<a href="http://url/to/resource/">http://url/to/resource/</a>"  }
	}
}
</pre>

<h3>7. Assertions</h3>
<h4>7.1 Deleted Resources</h4>
<pre>
{
	"activity":
	{
		"actor": "educator",
		"verb": 
		{
			"action": "deleted",
			"date": "2013-03-01"
		},
		"object": {  "id": "<a href="http://url/to/resource/">http://url/to/resource/</a>"  }
	}
}
</pre>

<h4>7.2 Resource is Superceded (has moved)</h4>
<p>Organization A asserts that the URL of resource X supercedes the URL of resource Y (Y has moved to X):</p>
<pre>
{
	"activity": 
	{
		"verb": "superceded", 
		"object": "http://resourceurl/resourceX/", 
    "related": ["http://resourceurl/resourceY/"]
	}
}
</pre>


