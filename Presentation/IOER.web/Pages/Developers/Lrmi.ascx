<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Lrmi.ascx.cs" Inherits="IOER.Pages.Developers.Lrmi" %>

<h2>Introduction</h2>
<p>IOER Resources are published using <a href="/developers/metadata">a custom extension</a> of <a href="http://www.lrmi.net/the-specification" target="_blank">the LRMI Schema</a>, which itself is an extension of <a href="http://schema.org/" target="_blank">schema.org</a>.
    Since LRMI was created, it has been accepted for inclusion with schema.org, and LRMI has transferred to the Dublin Core Metadata Initiative.  Background information on LRMI is available at
    <a href="http://www.lrmi.net/" target="_blank">http://www.lrmi.net/</a>.  Education resources can all be thought of as creative works, therefore any of the applicable properties from
    schema.org's <a href="http://www.schema.org/CreativeWork" target="_blank">Creative Work</a> can be used.  Likewise, because schema.org's Creative Work inherits from their 
    <a href="http://www.schema.org/Thing" target="_blank">Thing</a>, which is their most generic type of item, so any of the applicable properties from schema.org's Thing schema are considered
    to be legitimate LRMI properties.
</p>
<p>For publishers who want to ensure they publish with maximum compatibility with IOER's system, please ensure that the JSON documents you publish follow the structure prescribed by these schemas, with the following order of preference:</p>
<ol>
  <li>LRMI Schema - This is the schema that the greatest number of Learning Registry consumers will understand</li>
  <li>Schema.org - The entirety of this is technically valid to use in LRMI; however, due to its size and complexity, not all consumers will have implemented a means of understanding all of it</li>
  <li>IOER's custom extension - This may change from time to time, and should not be the basis of your publishing</li>
</ol>
<p>A description of LRMI's properties as RDF is available at <a href="http://dublincore.org/dcx/lrmi-terms" target="_blank">http://dublincore.org/dcx/lrmi-terms</a>.  An adequate explanation of the terms
    is available at <a href="http://www.lrmi.net/the-specification" target="_blank">http://www.lrmi.net/the-specification</a>.
</p>

<h2>Quirks</h2>
<p>Using the LRMI schema is mostly straightforward; however, there are some quirks worth noting:</p>
<dl>
  <dt>Case Sensitivity</dt>
  <dd>LRMI prescribes using "camelCase" to record property names. The JSON format is case-sensitive, so "learningResourceType" is <b>not</b> the same as "LearningResourceType". This applies to property <i>names</i>, but not necessarily to property <i>values</i> whose expected type (according to <a href="http://www.lrmi.net/the-specification" target="_blank">the LRMI Schema</a>) is "schema.org/Text".</dd>
  <dt>The educationalAlignment Property</dt>
  <dd>Most of the complexity of using LRMI comes from the educationalAlignment property. This is expected to be a specific object (or array of objects) whose structure is given in the specification, but not well explained. See below for IOER's expectations for consuming this property</dd>
  <dt>Vocabulary</dt>
  <dd>LRMI gives specific property names, but only <i>suggests</i> a handful of specific values for some of them. IOER's vocabulary values can be found on <a href="/developers/metadata">this page</a>, but we have implemented ways of understanding hundreds of additional values consumed from the LR by attempting to map/translate them to the values on that page. LRMI does not define a specific way of writing values. IOER publishes its values as an array of camelCased strings that do not contain spaces (except for some fields in AlignmentObjects; see below); however, using any manner (or lack) of case for values, with or without spaces, is technically valid.</dd>
  <dt>Extensions</dt>
  <dd>Schema.org covers many of the fields that would be needed to describe educational resources. However, it was not sufficient, hence the creation of LRMI (which extends Schema.org). Occasionally, LRMI itself is not sufficient, and publishers (including but not exclusively IOER) will add additional fields. We highly recommend periodic inspections of the JSON documents your system is consuming to see if a publisher is using additional custom fields your system can use to enhance its data. One example of this was the "description" field, which was not originally part of LRMI, but was widely included in published JSON documents.</dd>
  <dt>Multiple Values</dt>
  <dd>Most of LRMI's fields can contain one or more values. LRMI does not require or recommend a particular document format (e.g., XML, JSON, HTML), so it does not address the proper syntax for recording a field that <i>may</i> contain multiple values. Learning Registry document payloads are almost always in JSON format, so this can become an issue when a field that could have multiple values only needs <i>one</i> for a given Resource. In this case, using a single-value array or directly writing the property (e.g., { "propertyName": ["value1"] } or { "propertyName": "value1" }) are both arguably valid. However, for consistency, IOER always publishes these fields as arrays regardless of whether or not they contain just one value. We highly recommend others do the same, but we are able to understand and process either method.</dd>
</dl>

<h2>AlignmentObject</h2>
<p>LRMI allows for recording the alignment of Resources to learning standards, grade levels, and potentially other types of categorizations via the educationalAlignment field. This field appears to be intended as a "catch-all" for any type of alignment(s) that a Resource might have. As such, it is defined in broad terms that can make it somewhat difficult to understand, use, or interpret.</p>
<p>IOER uses this field for learning standards and grade levels, and is able to interpret its values as applying to either of these.</p>
<p>LRMI's educationalAlignment field must be an object or array of objects (we recommend always using an array) that have a specific format. This format is given in the specification and referred to as an "AlignmentObject". The fields for this object are given, but not well described (presumably due to this field needing to be very flexible). IOER's interpretation is as follows:</p>
<dl>
  <dt>alignmentType</dt>
  <dd>In the context of a learning standard, this field would be <b>one</b> of three of the recommended five values: "assesses", "teaches", or "requires".  It can be understood as "The Resource [assesses, teaches, or requires] the learning standard defined by this object."</dd>
  <dd>In the context of a grade level, this would use a value to indicate that the object is a grade level rather than a standard (e.g., "educationLevel").</dd>
  <dt>educationalFramework</dt>
  <dd>In the context of a learning standard, this would be the official name of the entirety of a body of standards (e.g., "Common Core State Standards") that contain the specific standard referenced in the targetName field.</dd>
  <dd>In the context of a grade level, this would be the educational system (e.g., "U.S. P-20") to which the grade level is relevant.</dd>
  <dt>targetDescription</dt>
  <dd>This field is not used by IOER, and most other publishers do not appear to use it either. Presumably it would provide additional information about an alignment that was not commonly understood.</dd>
  <dt>targetName</dt>
  <dd>In the context of a learning standard, this would be a dot notation or other specific means of referring to the specific standard being defined by the object (e.g, "CCSS.Math.3.NBT.A.1").</dd>
  <dd>In the context of a grade level, this would be one or more grade levels being defined by the object, either as a range (e.g. "5-7") or an array (e.g. ["5", "6", "7"]) or a single value (where multiple AlignmentObjects are used).</dd>
  <dt>targetUrl</dt>
  <dd>In the context of a learning standard, this would be a URL that is tied to a specific standard being defined by the object (e.g., "http://asn.jesandco.org/resources/S11436C7"). If both targetUrl and targetName are provided, IOER will use targetName.</dd>
  <dd>This field is not used in the context of a grade level.</dd>
</dl>

<p>An example of a properly-formed (according to IOER's best understanding) educationalAlignment field:</p>
<pre>educationalAlignment: [
	{
		alignmentType: "educationLevel",
		educationalFramework: "US K-12 Grade Levels",
		targetName: "6",
	},
	{
		alignmentType: "educationLevel",
		educationalFramework: "US K-12 Grade Levels",
		targetName: "7",
	},
	{
		alignmentType: "educationLevel",
		educationalFramework: "US K-12 Grade Levels",
		targetName: "8",
	},
	{
		alignmentType: "requires",
		educationalFramework: "Common Core State Standards for English Language Arts",
		targetName: "CCSS.ELA-Literacy.RH.6-8.7",
		targetUrl: "http://corestandards.org/ELA-Literacy/RH/6-8/7"
	},
	{
		alignmentType: "assesses",
		educationalFramework: "Common Core State Standards for English Language Arts",
		targetName: "CCSS.ELA-Literacy.RH.6-8.9",
		targetUrl: "http://corestandards.org/ELA-Literacy/RH/6-8/9"
	},
	{
		alignmentType: "teaches",
		educationalFramework: "Common Core State Standards for English Language Arts",
		targetName: "CCSS.ELA-Literacy.RH.9-10.9",
		targetUrl: "http://corestandards.org/ELA-Literacy/RH/9-10/9"
	},
	{
		alignmentType: "requires",
		educationalFramework: "Common Core State Standards for English Language Arts",
		targetName: "CCSS.ELA-Literacy.RH.11-12.7",
		targetUrl: "http://corestandards.org/ELA-Literacy/RH/11-12/7"
	}
]
</pre>



