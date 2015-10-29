<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Metadata.ascx.cs" Inherits="IOER.Pages.Developers.Metadata" %>

<style type="text/css">
  .metadataItem { padding: 5px; margin: 10px 0; border-top: 1px solid #CCC; }
  .metadataItem, .metadataItem * { box-sizing: border-box; -moz-box-sizing: border-box; }
  .metadataItem { margin: 10px 0; border-top: 1px solid #CCC; }
  .metadataItem .metadata, .metadataItem .vocabulary { padding: 5px; }
  .metadataItem .metadata, .metadataItem .vocabulary, .metadataItem dd { display: inline-block;	vertical-align: top; }
  .metadataItem dl { margin: 0; position: relative; }
  .metadataItem dt { padding: 2px 0; border-top: 1px solid #DDD; display: block; }
  .metadataItem dd { margin-bottom: 5px; padding: 2px 0;  }
  .metadataItem dl dt:first-child { border-top: none; }
  .metadataItem dl dd:nth-child(2) { border-top: none; }
  .metadataItem dl dd:empty { border-top: none; }
  .metadataItem .metadata { width: 32%; }
  .metadataItem .vocabulary { width: 65%; }
  .metadataItem .title { font-weight: bold;	font-size: 120%; padding: 5px 0 0 0; }
  .metadataItem *:before { font-style: italic; color: #999; }
  .metadataItem .tag:before { content: "Tag: "; display: block; }
  .metadataItem .source:before { content: "Source: "; display: block; }
  .metadataItem .definition:before { content: "Definition: "; display: block; }
  .metadataItem .tagsSource:before { content: "Vocabulary Source: "; display: block; }
  .metadataItem dl dd:nth-child(2):empty:before { content: " "; }
  #metadataMainBigTable { display: none; }
</style>
 
<p>IOER Metadata is based on:</p>
<ul>
  <li><a href="http://dublincore.org/dcx/lrmi-terms/1.1/" target="_blank">Learning Resource Metadata Initiative (LRMI) schema</a>&nbsp;<em>version 1.1</em></li>
  <li>The LRMI 1.0 was adopted by Schema.org (SCH).</li>
  <li>Additional metadata has been introduced and potential exists for additional needs to be added over time. LRMI seeks to have SCH adopt any new schema.</li>
  <li>Learning Registry (LR)</li>
  <li>NSDL Learning Application Readiness (LAR)</li>
  <li>Vocabulary used by IOER mirrors that of the federal <a href="https://ceds.ed.gov/" target="_blank">Common Education Data Standards</a> (CEDS)</li>
  <li>Race to the Top State Support (RttT)</li>
  <li><a href="http://json-ld.org/" target="_blank">JSON-LD</a></li>
</ul>

<div class="metadataItem"><div class="title">Title</div><div class="metadata"><div class="tag">name</div><div class="source">Schema.org, LRMI</div><div class="tagsSource">Schema.org, LRMI</div><div class="definition"> The name of the Resource. </div></div>&nbsp; 
   <div class="vocabulary"><dl><dt>[Text]</dt><dd></dd></dl></div></div>
<div class="metadataItem"><div class="title">Resource URL</div><div class="metadata"><div class="tag">url</div><div class="source">Schema.org, LRMI</div><div class="tagsSource">Schema.org, LRMI</div><div class="definition"> The web address of the Resource. </div></div> 
   <div class="vocabulary"><dl><dt>[Text]</dt><dd>URL-formatted text.</dd> </dl></div></div>
<div class="metadataItem"><div class="title">Description</div><div class="metadata"><div class="tag">description</div><div class="source">Schema.org</div><div class="tagsSource">Schema.org</div><div class="definition"> A brief summary of the content and scope of the Resource. </div></div>&nbsp; 
   <div class="vocabulary"><dl><dt>[Text]</dt><dd></dd> </dl></div></div>
<div class="metadataItem"><div class="title">Creator</div><div class="metadata"><div class="tag">author</div><div class="source">Schema.org, LRMI</div><div class="tagsSource">Schema.org, LRMI</div><div class="definition"> The individual or entity credited with the creation of the Resource. </div></div>&nbsp; 
   <div class="vocabulary"><dl><dt>[Text]</dt><dd></dd> </dl></div></div>
<div class="metadataItem"><div class="title">Publisher</div><div class="metadata"><div class="tag">publisher</div><div class="source">Schema.org, LRMI</div><div class="tagsSource">Schema.org, LRMI</div><div class="definition"> The entity responsible for making the Resource available or for publishing the Resource. </div></div>&nbsp; 
   <div class="vocabulary"><dl><dt>[Text]</dt><dd></dd> </dl></div></div>
<div class="metadataItem"><div class="title">Keyword</div><div class="metadata"><div class="tag">&nbsp;</div><div class="source">Learning Registry Document</div><div class="tagsSource">ISLE</div><div class="definition"> Unique words or phrases that make it easier to find the Resource via Search tools. </div></div>&nbsp; 
   <div class="vocabulary"><dl><dt>[Text]</dt><dd></dd> </dl></div></div>
<div class="metadataItem"><div class="title">Learning Standards</div><div class="metadata"><div class="tag">educationalAlignment</div><div class="source">Schema.org, LRMI</div><div class="tagsSource">Schema.org, ISLE</div><div class="definition"> One or more Learning Standards to which the Resource aligns. </div></div>&nbsp; 
   <div class="vocabulary"><dl><dt>[Text]</dt><dd>Dot-notation identifying a particular Standard or group of Standards.</dd><dt>Teaches</dt><dd>The Resource teaches this Standard.</dd><dt>Assesses</dt><dd>The Resource assesses this Standard.</dd><dt>Requires</dt><dd>The Resource requires the user to know this Standard.</dd> </dl>
      <br>
   </div></div>
<div class="metadataItem"><div class="title">Date Created</div><div class="metadata"><div class="tag">dateCreated</div><div class="source">Schema.org, LRMI</div><div class="tagsSource">Schema.org, LRMI</div><div class="definition"> The date on which the resource was created. </div></div> 
   <div class="vocabulary"><dl><dt>[Text]</dt><dd>Date-formatted text.</dd> <span style="background-color:transparent;"></span></dl></div></div>
<div class="metadataItem"><div class="title">Instructions &amp; Equipment Requirements</div><div class="metadata"><div class="tag">isBasedOnUrl</div><div class="source">ISLE</div><div class="tagsSource">ISLE</div><div class="definition"> Any hardware, software, equipment, instructions, or other materials required to use the Resource. </div></div> 
   <div class="vocabulary"><dl><dt>[Text]</dt><dd></dd> </dl>  </div></div>
<div class="metadataItem"><div class="title">Access Rights</div><div class="metadata"><div class="tag">accessRestrictions</div><div class="source">LAR, ISLE</div><div class="tagsSource">ISLE</div><div class="definition"> Conditions that govern the user’s ability to access (viewing, downloading, etc.) the Resource. </div></div> 
   <div class="vocabulary"><dl><dt>Free Access</dt><dd>The right to view and/or download material without financial, registration, or excessive advertising barriers.</dd><dt>Free Access with Registration</dt><dd>The right to view and/or download material without financial barriers but users are required to register or experience some other low-barrier to use.</dd><dt>Limited Free Access</dt><dd>Some material is available for viewing and/or downloading but most material tends to be accessible through other means.</dd><dt>Available for Purchase</dt><dd>The right to view, keep, and/or download material upon payment of a one-time fee.</dd><dt>Available by Subscription</dt> 
         <dd>The right to view and/or download material, often for a set period of time, by way of a financial agreement between rights holders and authorized users.</dd></dl>  </div> </div>
<div class="metadataItem"><div class="title">Usage Rights</div><div class="metadata"><div class="tag">useRightsUrl 
         <i>or</i> license</div><div class="source">LRMI (useRightsUrl), Schema.org, LRMI (license)</div><div class="tagsSource">Creative Commons</div><div class="definition">Conditions that govern the allowed use, modification, and/or redistribution of the Resource. 
         <p> 
            <i>Note: According to the LRMI specification page, useRightsUrl was not adopted by schema.org. Subsequently schema.org adopted a property called "license" which encompasses the same functions as useRightsUrl. The license property is likely to be more widely understood by consumers of schema.org mark-up than the LRMI useRightsUrl property.</i></p><p> 
            <i>The ISLE OER system publishes with both tags for maximum compatibility, and understands either when importing.</i></p></div></div> 
   <div class="vocabulary"><dl><dt>Attribution</dt><dd>http://creativecommons.org/licenses/by/3.0/</dd><dt>Attribution - Non-Commercial</dt><dd>http://creativecommons.org/licenses/by-nc/3.0/</dd><dt>Attribution - Non-Commercial - No Derivatives</dt><dd>http://creativecommons.org/licenses/by-nc-nd/3.0/</dd><dt>Attribution - Non-Commercial - Share Alike</dt><dd>http://creativecommons.org/licenses/by-nc-sa/3.0/</dd><dt>Custom Licensing</dt> 
         <dd>User-selected Licensing. This should be a URL that links to the appropriate Usage Rights for the Resource.</dd></dl>  </div> </div>
<div class="metadataItem"><div class="title">Language</div><div class="metadata"><div class="tag">inLanguage</div><div class="source">Schema.org, LRMI</div><div class="tagsSource">ISLE</div><div class="definition"> The primary language of the Resource. </div></div> 
   <div class="vocabulary"><dl><dt>English</dt><dd></dd><dt>Spanish</dt><dd></dd><dt>Polish</dt><dd></dd><dt>Chinese</dt><dd></dd><dt>Russian</dt> 
         <dd></dd></dl>  </div></div>
<div class="metadataItem"><div class="title">K-12 Subject</div><div class="metadata"><div class="tag">alignmentObject.educationalFramework</div><div class="source">Schema.org, LRMI</div><div class="tagsSource">ISLE</div><div class="definition"> The Primary US School subjects for Kindergarten through High School. </div></div> 
   <div class="vocabulary"><dl><dt>Mathematics</dt><dd></dd><dt>English Language Arts</dt><dd></dd><dt>Science</dt><dd></dd><dt>Social Studies</dt><dd></dd><dt>Arts</dt> 
         <dd></dd><dt>World Languages</dt><dd></dd><dt>Health</dt><dd></dd><dt>Physical Education</dt><dd></dd><dt>Technology</dt><dd></dd> </dl>  </div></div>
<div class="metadataItem"><div class="title">End User</div><div class="metadata"><div class="tag">educationalRole</div><div class="source">Schema.org, LRMI</div><div class="tagsSource">ISLE</div><div class="definition"> One or more broad categories of users that describe the intended audience of the Resource. </div></div> 
   <div class="vocabulary"><dl><dt>Administrator</dt><dd>A district or school level person of authority and responsibility.</dd><dt>General Public</dt><dd>The Public at large.</dd><dt>Mentor</dt><dd>Someone who advises, trains, supports, and/or guides.</dd><dt>Parent</dt><dd>A parent or legal guardian.</dd><dt>Professional</dt> 
         <dd>Someone already practicing a profession; an industry partner, or professional development trainer.</dd><dt>Student</dt><dd>The Learner.</dd><dt>Teacher/Education Specialist</dt><dd>A certified person directly involved with student instruction.</dd> </dl>  </div></div>
<div class="metadataItem"><div class="title">Resource Type</div><div class="metadata"><div class="tag">learningResourceType</div><div class="source">Schema.org, LRMI</div><div class="tagsSource">ISLE, NSDL, Others (mixed)</div><div class="definition"> A description of the primary type or category of the Resource. </div></div> 
   <div class="vocabulary"><dl><dt>Alternate Assessment</dt><dd>RTTT Assessment subtype.</dd><dt>Assessment Item</dt><dd>A single assessment item.</dd><dt>Career Information</dt><dd>ISLE: A resource that focuses on a specific career or career pathway.<br>NSDL: Resource describing specific science, technology, engineering, and mathematics (STEM) information and/or insight into STEM careers and requisite skills.</dd><dt>Course</dt><dd>NSDL: A set of teaching materials (generally for instructors) or learning materials (generally for students) intended to achieve a range of objectives over an extended period of time.</dd><dt>Curriculum</dt> 
         <dd>ISLE: A Heirarchical representation of a learning curriculum.</dd><dt>Demonstration/Simulation</dt><dd>NSDL: Imitative representation of a system, process, photo, setting, or principle.</dd><dt>Formative Assessment</dt><dd>RTTT Assessment subtype.</dd><dt>Game</dt><dd>NSDL: An interactive environment in which participants compete, strategize, play, role-play, troubleshoot, or make decisions in order to learn a subject or skill.</dd><dt>Image/Visuals</dt><dd>NSDL: Visual material that is not in motion and is not intended to annotate pieces of text (use for: poster, digital image of a painting or print).</dd><dt>Interim/Summative Assessment</dt><dd>RTTT Assessment subtype.</dd><dt>Lab Material</dt><dd>Compact Oxford English Dictionary: A scientific procedure undertaken to make a discovery, test a hypothesis, or demonstrate a known fact.</dd><dt>Learning Task</dt><dd>ISLE: A modular unit of learning that can be applied at one or more levels of educational scope.</dd><dt>Learning Curriculum Map</dt><dd>ISLE: Visual representation of modules or steps required to attain mastery of a particular concept, subject, field of study, or steps required to achieve certification for a career or career function.</dd><dt>Manipulative</dt><dd>ISLE: Resource that provides tools or templates for creating visual or tactile representations of a process or concept.</dd><dt>Other</dt><dd>ISLE: Any resources not found within this list.</dd><dt>Primary Source</dt><dd>ISLE: The primary source of information for a given topic.</dd><dt>Reading Material</dt><dd>ISLE: Any textual representation of information.</dd><dt>Reference Material</dt><dd>ISLE: Any source of information useful for referencing pieces of information (e.g., a dictionary, encyclopedia, or wiki)</dd><dt>Rubric/Scoring Guide</dt><dd>NSDL: Statements describing the abilities, knowledge, or understanding of a content area in order to reach a certain level of mastery.</dd><dt>Self Assessment</dt><dd>RTTT Assessment subtype.</dd><dt>Syllabus</dt><dd>iLumina: Plan showing the structure of a particular course, which may include course description and objectives, grading policy, materials, assignments, lesson sequence, and course calendar.</dd><dt>Unit</dt><dd>NSDL: A set of teaching materials, generally for instructors, or alternatively learning materials, generally for students, intended to achieve specific and focused objectives over a limited period of time such that units often constitute.</dd> </dl>  </div></div>
<div class="metadataItem"><div class="title">Media Type</div><div class="metadata"><div class="tag">mediaType</div><div class="source">Dublin Core, LRMI, ISLE (see Note)</div><div class="tagsSource">ISLE</div><div class="definition"> The physical or digital medium that contains, stores, transports, and/or provides the Resource. 
         <p> 
            <i>Note: Media Type was included in an earlier version of the LRMI schema (as "mediaType"). It was removed in the current version. An equivalent property, "Format", exists in the Dublin Core schema, which is also used by the NSDL_DC and LAR schemas. 
               <a href="https://groups.google.com/forum/#%21topic/lrmi/HVBcJFwX_FQ" target="_blank">There does not appear to be a clear equivalent to this property within Schema.org</a>, as Schema.org defines many properties that would be equivalent to some aspect or subset of this property. This property is still in use by the OER system.</i></p></div></div><div class="vocabulary"><dl><dt>Application/Software</dt><dd>Any computer program, such as office software, web-based tools, video games, or other interactive electronic media.</dd><dt>Archive</dt><dd>A compressed file that contains other files.</dd><dt>Audio</dt><dd>Any audio files, such as sound clips, music, narration, audio books, etc.</dd><dt>Document/Text</dt><dd>Any of a wide range of text, ranging from small text files to whole textbooks and beyond.</dd><dt>Image</dt><dd>Any image file.</dd><dt>Interactive Whiteboard</dt><dd>Any interactive system that functions as a digital or semi-digital whiteboard or chalkboard, typically used to combine physical writing or marking with digital content.</dd><dt>Non-Digital</dt><dd>Any non-digital resource, such as books or magazines. Also used to indicate a Non-Digital version of another Format on this list.</dd><dt>Other</dt><dd>Any other Format not found on this list.</dd><dt>PDF</dt><dd>Adobe PDF file.</dd><dt>Slides</dt><dd>Sequentially-presented text and/or graphics; a slideshow.</dd><dt>Spreadsheet</dt><dd>A grid-based document used to represent information in a structured way.</dd><dt>Video</dt><dd>Any video file.</dd><dt>Webpage</dt><dd>An HTML-encoded document or linked series of documents, usually accessed on the Internet.</dd> </dl>  </div></div>
<div class="metadataItem"><div class="title">Educational Use</div><div class="metadata"><div class="tag">educationalUse</div><div class="source">Schema.org, LRMI</div><div class="tagsSource">ISLE</div><div class="definition"> The primary way(s) the Resource is intended by its creator to be used. </div></div> 
   <div class="vocabulary"><dl><dt>Assessment</dt><dd>Material(s) used to assess.</dd><dt>Curriculum and Instruction</dt><dd>Material(s) used to teach students.</dd><dt>Professional Development</dt><dd>Material(s) used to teach teachers &amp; professionals.</dd><dt>Enhancement</dt><dd>A resource used to teach a high-performing student.</dd><dt>Intervention</dt> 
         <dd>A resource used to teach a low-performing student.</dd> </dl>  </div></div>
<div class="metadataItem"><div class="title">Grade Level</div><div class="metadata"><div class="tag">alignmentObject.educationalFramework 
         <i>and</i> typicalAgeRange</div><div class="source">LRMI, ISLE (see Note)</div><div class="tagsSource">ISLE unless otherwise noted</div><div class="definition"> The US K-12 Grade(s) and/or Postsecondary educational audience that the Resource is intended for. 
         <p> 
            <i>Note: Wherever possible, the OER system translates between typicalAgeRange and grade level (as defined by US schools). The typicalAgeRange property does not adequately address postsecondary, technical, or adult education. However, grade level does not exist in LRMI. Grade level as used by the OER system is derived from the NSDL_DC/LAR implementation of "Education Level", adjusted for US schools. The OER system publishes both grade level and typicalAgeRange where possible, and understands either when importing data.</i></p></div></div> 
   <div class="vocabulary"><dl><dt>Pre-Kindergarten</dt><dd></dd><dt>Kindergarten</dt><dd></dd><dt>Grade 1</dt><dd></dd><dt>Grade 2</dt><dd></dd><dt>Grade 3</dt> 
         <dd></dd><dt>Grade 4</dt><dd></dd><dt>Grade 5</dt><dd></dd><dt>Grade 6</dt><dd></dd><dt>Grade 8</dt><dd></dd><dt>Grades 9-10</dt><dd></dd><dt>Grades 11-12</dt><dd></dd> 
         <dt>Postsecondary</dt><dd>Any courses designed for an adult (post high school level).</dd><dt>Technical</dt><dd>Resources that address a specific skill set for a technical field.</dd><dt>Adult Education</dt><dd>NSDL: An educational level indicating a resource represents informal educational programming designed for a general audience, of diverse ages (adults or adults with children) and knowledge levels.</dd></dl>  </div></div>
<div class="metadataItem"><div class="title">Career Cluster</div><div class="metadata"><div class="tag">careerCluster</div><div class="source">ISLE</div><div class="tagsSource">ISLE</div><div class="definition"> The Illinois Career Cluster Frameworks items that describe broad career fields that Resources can help users prepare to enter and/or be a part of. </div></div> 
   <div class="vocabulary"><dl><dt>Agriculture, Food, and Natural Resources</dt><dd>Development, production, processing, distribution, of agricultural commodities and resources including food, fiber, wood products, natural resources, horticulture, and other plant and animal products/resources.</dd><dt>Architecture and Construction</dt><dd>Designing, planning, managing, building, and maintaining the built environment including the use of green technologies.</dd><dt>Energy</dt><dd>Developing, planning and managing the production of energy including renewable energy and clean coal technology and its distribution through smart grid technologies.</dd><dt>Finance</dt><dd>Securities and investments, business finance, accounting, insurance, and banking services.</dd><dt>Health Science</dt> 
         <dd>Planning, managing and providing therapeutic, diagnostic, health informatics, and support services as well as biomedical research and development.</dd><dt>Information Technology</dt><dd>Designing, developing, managing, supporting and integrating hardware and software systems.</dd><dt>Manufacturing</dt><dd>Product and process development and managing and performing the processing of materials into intermediate or final products and related support activities.</dd><dt>Research and Development</dt><dd>Scientific research and professional and technical services including laboratory and testing services, and research and development services.</dd><dt>Transportation, Distribution, and Logistics</dt><dd>Planning, management and movement of people, materials and goods across all transportation modes as well as maintaining and improving transportation technologies.</dd> </dl>  </div></div>
<div class="metadataItem"><div class="title">Group Type</div><div class="metadata"><div class="tag">groupType</div><div class="source">InBloom, ISLE</div><div class="tagsSource">InBloom, ISLE</div><div class="definition"> The size or nature of the group for which the Resource is intended. 
         <p> 
            <i>Note: This property and its vocabulary originated with the InBloom system. Their system later dropped the tag. It is still in use by the OER system.</i></p></div></div> 
   <div class="vocabulary"><dl><dt>Class</dt><dd> </dd><dt>Group Large (6+ Members)</dt><dd></dd><dt>Group Small (3-5 Members)</dt><dd></dd><dt>Individual</dt><dd></dd><dt>Multiple Class</dt> 
         <dd></dd><dt>Pair</dt><dd></dd> </dl>  </div></div>
<div class="metadataItem"><div class="title">Accessibility Control</div><div class="metadata"><div class="tag">accessibilityControl</div><div class="source">LRMI</div><div class="tagsSource">WebSchemas, LRMI</div><div class="definition">Identifies input methods that are sufficient to fully control the described resource.</div></div> 
   <div class="vocabulary"><dl><dt>Full Keyboard Control</dt><dd></dd><dt>Full Mouse Control</dt><dd></dd><dt>Full Switch Control</dt><dd></dd><dt>Full Touch Control</dt><dd></dd><dt>Full Video Control</dt><dd></dd><dt>Full Voice Control</dt><dd></dd></dl></div></div>
<div class="metadataItem"><div class="title">Accessibility Feature</div><div class="metadata"><div class="tag">accessibilityFeature</div><div class="source">LRMI</div><div class="tagsSource">WebSchemas, LRMI</div><div class="definition">Content features of the resource, such as accessible media, alternatives and supported enhancements for accessibility.</div></div> 
   <div class="vocabulary"><dl><dt>Alternative Text</dt><dd></dd><dt>Annotations</dt><dd></dd><dt>Audio Description</dt><dd></dd><dt>Bookmarks</dt><dd></dd><dt>Braille</dt><dd></dd><dt>Captions</dt><dd></dd><dt>ChemML</dt><dd></dd><dt>Display Transformability</dt><dd></dd><dt>Display Transformability - Background Color</dt><dd></dd><dt>Display Transformability - Color</dt><dd></dd><dt>Display Transformability - Font Family</dt><dd></dd><dt>Display Transformability - Font Size</dt><dd></dd><dt>Display Transformability - Line Height</dt><dd></dd><dt>Display Transformability - Word Spacing</dt><dd></dd><dt>High Contrast Audio</dt><dd></dd><dt>High Contrast Audio - No Background</dt><dd></dd><dt>High Contrast Audio - Reduced Background</dt><dd></dd><dt>High Contrast Audio - Switchable Background</dt><dd></dd><dt>High Contrast Display</dt><dd></dd><dt>Index</dt><dd></dd><dt>Large Print</dt><dd></dd><dt>LaTeX</dt><dd></dd><dt>Long Description</dt><dd></dd><dt>MathML</dt><dd></dd><dt>Print Page Numbers</dt><dd></dd><dt>Reading Order</dt><dd></dd><dt>Sign Language</dt><dd></dd><dt>Structural Navigation</dt><dd></dd><dt>Table of Contents</dt><dd></dd><dt>Tactile Graphic</dt><dd></dd><dt>Tactile Object</dt><dd></dd><dt>Tagged PDF</dt><dd></dd><dt>Timing Control</dt><dd></dd><dt>Transcript</dt><dd></dd><dt>Unlocked</dt><dd></dd></dl></div></div>
<div class="metadataItem"><div class="title">Accessibility Hazard</div><div class="metadata"><div class="tag">accessibilityHazard</div><div class="source">LRMI</div><div class="tagsSource">WebSchemas, LRMI</div><div class="definition">A characteristic of the described resource that is physiologically dangerous to some users. Related to WCAG 2.0 guideline 2.3.</div></div> 
   <div class="vocabulary"><dl><dt>Flashing</dt><dd></dd><dt>Motion Simulation</dt><dd></dd><dt>No Flashing</dt><dd></dd><dt>No Motion Simulation</dt><dd></dd><dt>No Sound</dt><dd></dd><dt>Sound</dt><dd></dd> </dl></div></div>

<h2>LRMI Schema Extensions</h2>
<p>The following extensions to the LRMI schema are required to handle ISLE needs:</p>
<dl>
  <dt>Description</dt>
  <dd>Metadata tag: "description"</dd>
  <dd>Adequately expressed in schema.org as "description" - a part of "Thing" which is the most generic type of item.</dd>
  <dd>Also present in LAR, LOM, NSDL_DC, and Dublin Core.</dd>
  <dd>Free-text to describe a Resource</dd>
</dl>
<dl>
  <dt>Group Type</dt>
  <dd>Metadata tag: "groupType"</dd>
  <dd>Not in schema.org, NSDL_DC, or LAR.</dd>
  <dd>Defines the intended size of the group for which a Resource is meant</dd>
</dl>
<dl>
  <dt>Access Rights</dt>
  <dd>Metadata tag: "accessRestrictions"</dd>
  <dd>Not in schema.org.</dd>
  <dd>borrowed from NSDL/LAR (LAR calls it "accessRestrictions" while NSDL_DC calls it "accessRights"). Different from Usage Rights.</dd>
  <dd>Defines the cost/requirements associated with accessing a Resource</dd>
  <dd>NSDL_DC Definition: Information describing conditions or requirements for viewing and/or downloading the material.</dd>
  <dd>LAR Definition: Information about a user's access to a resource in regards to conditions or regulations imposed by the rights owner.</dd>
  <dd>NSDL_DC and LAR use the same controlled vocabulary with one exception: NSDL has "Free Access with Registration" while LAR has "Free Access with User Action" - These are nearly identical with LAR having the broader definition.</dd>
</dl>
<dl>
  <dt>Instructions &amp; Equipment Requirements</dt>
  <dd>Metadata tag: "requires"</dd>
  <dd>ISLE Extension</dd>
  <dd>"dct:requires" is a part of NSDL_DC and Dublin Core,.</dd>
  <dd>Not a part of LAR or schema.org.</dd>
  <dd>Free-text to describe any equipment, hardware, instructions, materials, etc., necessary to use a Resource</dd>
</dl>
<dl>
  <dt>Career Clusters</dt>
  <dd>Metadata tag: "careerCluster" with properties for "country" and "region" (e.g., &lt;careerCluster country="US" region="IL"&gt;[a given cluster name]&lt;/careerCluster&gt;)</dd>
  <dd>ISLE Extension</dd>
  <dd>Not a part of Dublin Core, NSDL_DC, LOM, LRMI, LAR, or schema.org.</dd>
  <dd>ISLE's focus is not solely K-12; the Career Cluster tag will be used with the STEM fields and other high-demand occupational fields for adults/professionals</dd><dd>The career cluster "Energy" is a career cluster local to Illinois; ISLE will be publishing this as a local rather than a Federal career cluster.</dd>
  <dd>Federal career clusters will be published with a "Country" attribute of "US" and will lack a "Region" attribute. Local career clusters are published with a "Country" attribute of "US" and a "Region" attribute of "IL". ISLE will use USPS abbreviations for regions, so a career cluster local to Puerto Rico would be published with a "Country" attribute of "US" and a "Region" attribute of "PR", because Puerto Rico is a Commonwealth of the US.</dd>
  <dd>ISLE understands all 16 Federal career clusters + one local cluster, "Energy".</dd>
</dl>