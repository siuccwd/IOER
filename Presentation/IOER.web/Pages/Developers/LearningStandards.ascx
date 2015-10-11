<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LearningStandards.ascx.cs" Inherits="IOER.Pages.Developers.LearningStandards" %>

<p>IOER publishes and consumes learning standards metadata as part of its work with learning resources. ISLE/IOER does not create, host, or maintain any learning standards; however, we do rely on resolution services (primarily provided by <a href="http://achievementstandards.org/" target="_blank">Achievement Standards Network (ASN)</a>) to reliably communicate and understand standards to/from other systems via the Learning Registry.</p>
<p>ASN creates canonical URLs that can be used to reference individual standards within a standards framework, and typically provides other identifiers as well. These other identifiers are usually a <a href="http://en.wikipedia.org/wiki/Globally_unique_identifier" target="_blank">GUID</a> and/or dot notation system (akin to that used by the <a href="http://www.corestandards.org/" target="_blank">Common Core State Standards</a>, e.g., "Math.6.EE.1"). IOER utilizes these to match consumed metadata to known standards.</p>
<p>ASN's system is able to accomodate nearly any type of standard set, including educational, employability, and technical standards. Upon request, ASN can work with organizations to have new standards added to ASN's service.</p>

<h2>Publishing Standards</h2>
<p>IOER recommends using LRMI to publish resource metadata. LRMI provides a flexible metadata structure that allows publishing learning standards using a variety of identifiers. A sample of what this structure should look like is below. For more information, see the <a href="/developers/lrmi">Using LRMI</a> page.</p>
<pre>educationalAlignment: [

	{
		alignmentType: "requires",
		educationalFramework: "Common Core State Standards for English Language Arts",
		targetName: "CCSS.ELA-Literacy.RH.6-8.7",
		targetUrl: "http://corestandards.org/ELA-Literacy/RH/6-8/7",
		targetDescription: "Integrate visual information (e.g., in charts, graphs, photographs, videos, or maps) with other information in print and digital texts."
	},
	{
		alignmentType: "assesses",
		educationalFramework: "Common Core State Standards for English Language Arts",
		targetName: "CCSS.ELA-Literacy.RH.6-8.9",
		targetUrl: "http://corestandards.org/ELA-Literacy/RH/6-8/9",
		targetDescription: "Analyze the relationship between a primary and secondary source on the same topic."
	},
	{
		alignmentType: "teaches",
		educationalFramework: "Common Core State Standards for English Language Arts",
		targetName: "CCSS.ELA-Literacy.RH.9-10.9",
		targetUrl: "http://corestandards.org/ELA-Literacy/RH/9-10/9",
		targetDescription: "Compare and contrast treatments of the same topic in several primary and secondary sources."
	},
	{
		alignmentType: "requires",
		educationalFramework: "Common Core State Standards for English Language Arts",
		targetName: "CCSS.ELA-Literacy.RH.11-12.7",
		targetUrl: "http://corestandards.org/ELA-Literacy/RH/11-12/7",
		targetDescription: "Integrate and evaluate multiple sources of information presented in diverse formats and media (e.g., visually, quantitatively, as well as in words) in order to address a question or solve a problem."
	}
]
</pre>