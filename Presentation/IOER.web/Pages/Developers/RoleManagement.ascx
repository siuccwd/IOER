<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RoleManagement.ascx.cs" Inherits="IOER.Pages.Developers.RoleManagement" %>

<p>ISLE's scope includes development and maintenance of a centralized portal that will provide a unified user-experience while enabling users to personalize and save preferences.  The portal will incorporate a presentation framework for all ISLE applications, including:</p>
<ol>
  <li>Page Header</li>
  <li>Page Footer</li>
  <li>Site Navigation Panel</li>
  <li>User Interface Panel</li>
</ol>

<p>The ISLE development team has selected uPortal as the software that is utilized for the centralized application launcher.  Applications launchable from the application launcher include IOER and the Student Dashboard developed by Northern Illinois University.</p>
<p>Authenticated users from applications such as the Student Dashboard can seamlessly login to IOER using the same credentials as they used to sign in to the student dashboard.  ISLE uses a Shibboleth Identity Provider as its central identity provider (IdP) to provide the identity information to the applications.  Each ISLE application runs an instance of Shibboleth's Service Provider (SP) to consume the information and grant access to secure content.  Shibboleth IdPs and SPs use SAML to communicate the information.  For more information about Shibboleth, see <a href="https://en.wikipedia.org/wiki/Shibboleth_(Internet2)" target="_blank">https://en.wikipedia.org/wiki/Shibboleth_(Internet2)</a>.</p>

<h2>User Access Levels</h2>

<p>The ISLE portal service will establish an interface for four levels of user access:</p>
<dl>
  <dt>Level 1 - Anonymous User Access, through the ISLE Website (<a href="http://ioer.ilsharedlearning.org/">ioer.ilsharedlearning.org</a>)</dt>
  <dd>The website will provide general public access to information on ISLE and applications that do not require the establishment of a user account or need integrated data.</dd>
  <dt>Level 2 - Authenticated User Access</dt>
  <dd>A Level 2 user will either set up his own account, or will sign on to IOER using their school district provided credentials through the ISLE portal.  Level 2 users can create and maintain libraries, collections, and resources.  Level 2 users can also comment, add standards alignments, keywords, and other tags to a resource.  In addition, they can like/dislike and evaluate resources based on a rubric.</dd>
</dl>

<h2>Examples</h2>

<p>The Applications must be designed to support each type of user access.  Examples of the four types user access for the Assessment Application are described in the following vignette:</p>
<p><b>Level 1:</b></p>
<ul>
  <li>Joan, a high school math teacher, hears about ISLE at a manufacturing expo.  Afterward, she accesses the ISLE website for basic information on ISLE.  She is able to find information on the capabilities available through the assessment authoring and delivery tool.</li>
</ul>
<p><b>Level 2:</b></p>
<ul>
  <li>Joan establishes an ISLE account, with username and password, which requires her to self-identify as a teacher at an Illinois school district.  With the account set up, she can author an assessment using the ISLE authoring tool and save that to her own account, but is not able to publish it to the broader ISLE community.</li>
  <li>Sheila, an HR Director, establishes an ISLE account, which requires her to identify as a business partner for the Manufacturing STEM Learning Exchange.  With the account set up, she can search for manufacturing education resources using the content discovery tool and can save those searches. </li>
  <li>Elsa, a 9th grade student, establishes an ISLE account, with username and password.  She is able to create and save a personalized learning plan using ISLE tools, and explore career opportunities and information in the field of manufacturing (her career area of interest).</li>
</ul>
