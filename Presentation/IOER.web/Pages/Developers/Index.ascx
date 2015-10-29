<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Index.ascx.cs" Inherits="IOER.Pages.Developers.Index" %>

<style type="text/css">
  #edTech { padding-left: 160px; background: url('//ioer.ilsharedlearning.org/OERThumbs/large/574951-large.png') left 10px center no-repeat; background-size: 150px auto; margin-bottom: 10px; }
	#ppt { float: right; margin: 0 0 10px 10px; width: 400px; display: inline-block; }
	#pptFrame { height: 300px; }
	#pptFrame iframe { display: block; border: 1px solid #CCC; border-radius: 5px; overflow: hidden; width: 100%; height: 100%; }
	#ppt a { display: block; padding: 2px 5px; text-align: center; }
	@media (max-width: 1175px) {
		#ppt { float: none; margin: 5px auto; display: block; max-width: 98%; }
	}
</style>

<div id="ppt">
	<div id="pptFrame">
		<iframe src="//www.slideshare.net/slideshow/embed_code/key/2uEokoAnXG8dVv" allowfullscreen="allowfullscreen"></iframe>
	</div>
	<a href="/OERThumbs/files/ioer_integration_final.pdf">IOER Integration Guide (PDF)</a>
</div>

<p><b>IOER Developer documentation is subject to change, and will evolve as the project progresses. We recommend you check back regularly for updates and changes.</b></p>

<div id="edTech">
  <p>The US Department of Education has published a developers' guide:</p>
  <ul>
    <li><a href="http://ioer.ilsharedlearning.org/Resource/574951/Ed_Tech_Developers_Guide" target="_blank">Ed Tech Developers' Guide Metadata on IOER</a></li>
    <li><a href="http://www.ed.gov/news/press-releases/secretary-duncan-announces-education-department%e2%80%99s-first-ever-guide-ed-tech-developers" target="_blank">Press Release</a></li>
    <li><a href="https://tech.ed.gov/developers-guide/" target="_blank">Ed Tech Developers' Guide Website</a></li>
  </ul>
</div>

<p>The items below introduce the key integration requirements for selected ISLE Applications:</p>

<h2 style="clear:both;">Identity Integration</h2>
<p>ISLE provides a federated approach to single sign on (SSO) and identity management service.​</p>
<p class="listHeader">Schema and Other Requirements:</p>
<ul>
  <li>ISLE Identity Integration using SAML 2.0</li>
  <li>More Info: <a href="/developers/rolemanagement">Role Management</a></li>
</ul>

<h2>ISLE Dashboards</h2>
<p>Dashboard applications for teachers, administrators, and students triggers events that will be broadcast by the portal and consumed by other applications within the portal environment, including the learning map and assessment authoring apps. In turn,  applications such as the learning maps and assessment authoring tools will trigger events that will be broadcast by the portal and consumed by the dashboard application. The values that update the dashboard such as student achievement data, also updates the information on the Learning Map or Assessment Authoring tool.</p>

<h2>ISLE Educational Resources Store for Learning Registry Replication</h2>
<p>Resources created through integrated applications will be tagged and published using metadata, related vocabularies, and paradata as identified with the ISLE schemas. ISLE will offer APIs to retrieve and publish metadata and paradata.</p>
<p>Resources created through integrated applications will be tagged and published using metadata, related vocabularies, and paradata as identified with the ISLE schemas.</p>
<p>ISLE publishes resources to the Learning Registry (LR) for replication to ensure ISLE ​benefits from resource sharing.  Further, all related digital objects (i.e. assessment items, assessments, and other learning objects including learning map components) that are tagged will be stored within the ISLE content repository.</p>
<p class="listHeader">Schema and Other Requirements:</p>
<ul>
  <li><a href="http://dublincore.org/dcx/lrmi-terms/1.1/" target="_blank">LRMI Schema</a></li>
  <li><a href="/developers/schemas">Metadata and Schemas Overview</a></li>
  <li><a href="/developers/paradata">Paradata Overview</a></li>
</ul>

<h2>ISLE Content Repository</h2>
<p>Related digital objects (i.e. assessment items, assessments, and other learning objects including learning map components) that are tagged will be stored within the ISLE content repository.</p>
<p class="listHeader">Schema and Other Requirements:</p>
<ul>
  <li>The content repository will support a wide variety of file types, but file size may be limited.</li>
</ul>

<h2>Event Calendar</h2>
<p>The Event Calendar maintains calendars of events for Illinois workNet and for each of the nine STEM learning exchanges.</p>
<p class="listHeader">Schema and Other Requirements:</p>
<ul>
  <li><a href="/developers/eventcalendar">Event Calendar</a></li>
</ul>
