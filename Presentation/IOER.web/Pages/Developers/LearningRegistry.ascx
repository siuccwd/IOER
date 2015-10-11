<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LearningRegistry.ascx.cs" Inherits="IOER.Pages.Developers.LearningRegistry" %>

<h2 class="first">Introduction</h2>
<p>The <a href="http://learningregistry.org" target="_blank">Learning Registry</a> is a software solution and infrastructure that solves the problem of transmitting, storing, and replicating metadata and paradata (metadata about usage) about education resources.  At present, 
    it consists of two root nodes which replicate to each other, as well as a network of other nodes which replicate with one of the two root nodes.  Publishers and
    creators of education resources can publish metadata about education resources to the Learning Registry so that knowledge about them can be transmitted to the
    wider education community.
</p>
<p><span style="font-weight:bold;">What the learning registry is not:</span> The Learning Registry is not a searchable repository, nor is it a specific destination, 
    portal, or engine that educators will go to.  It is an open technology framework to which any content creator can publish, and any technology vendor or
    developer can leverage for applications.  IOER harvests documents from the Learning Registry, applies some business rules to make it easier to use, and stores
    the resulting metadata in a database.  This database is then used to build a searchable index that users can then search for useful education resources.
</p>
<p>A diagram of the flow of data between the Learning Registry and IOER follows:</p>
<div style="width:100%"><img src="../../images/IoerDataFlows.png" style="max-width:100%; height:auto; width:auto\9;" alt="IOER/LR Data Flow Diagram" title="IOER/LR Data Flow Diagram" /></div>

<h2>Structure of a Learning Registry Document</h2>

<p>Items in the Learning Registry (LR) are called documents, typically formatted as <a href="http://www.json.org/" target="_blank">JSON</a> documents. Each document consists of an envelope and a payload. Think of a learning registry document as a letter you might receive in the mail: The envelope is the container that the payload (letter) is put into before it goes into the LR.</p>

<p>The following properties (or fields) of the LR envelope are relevant for importing into the Data Warehouse:</p>
<dl>
  <dt>Signer</dt>
  <dd>The entity who signed the document (For example, SRI International).</dd>
  <dt>Submitter</dt>
  <dd>The person (or entity) who submitted the document (for example, SRI International on behalf of National Science Digital Library).</dd>
  <dt>Signature</dt>
  <dd>The digital signature used to verify the validity of the document.</dd>
  <dt>key_location</dt>
  <dd>The location on the web of the public part of the key used to sign the document.</dd>
  <dt>doc_ID</dt>
  <dd>The LR-assigned ID of the document in the Learning Registry.</dd>
  <dt>resource_locator</dt>
  <dd>The location of the actual resource (usually a URL).</dd>
  <dt>resource_data_type</dt>
  <dd>This is the type of data contained within the resource.  Valid values are <b>metadata</b> and <b>paradata</b>.</dd>
  <dt>payload_placement</dt>
  <dd>Where the payload is placed.  Valid values include <b>inline</b>, <b>linked</b>, and <b>attached</b>.  The import process handles only inline, other values are logged and reported.  So far only inline has been encountered.</dd>
  <dt>submission_tos</dt>
  <dd>The terms of use of the submission, <b>not</b> the terms of use of the resource</dd>
  <dt>payload_schema</dt>
  <dd>The schema used for the payload.  In the case where multiple schemas are present, the payload contains data which matches to multiple schemas.</dd>
  <dt>resource_data</dt>
  <dd>This is the payload (or letter contained within the envelope)</dd>
  <dt>keywords</dt>
  <dd>These are keywords that can be used to search for data in the LR.</dd>
</dl>

<h2>Extracting from the Learning Registry</h2>
<h3>Learning Registry API</h3>
<p>There are defined API methods available to extract data from the LR.  The API most useful for our purposes of extracting desired information for our index is called <b>listrecords</b>.  It is one of the methods in the Harvest API.</p>
<p><b>Listrecords</b> extracts LR documents in JSON format.  These documents are converted to XML and stored in files.  Each file contains approximately 200 LR documents (this is configurable and can be changed at any time).  The files are placed in a queue.  The import process then dequeues each file and imports the data into the database.</p>

<h3>Schedule</h3>
<p>Data is extracted from the LR on a nightly basis.  This is configurable, and currently begins around 6pm.  Data is extracted from the point in time where it last off forward to the current time.</p>

<h3>Handling Long Imports</h3>
<p>Currently LR activity is fairly low most of the month, but with periodic spikes when a publisher publishes a large amount of data.  Importing the data is far more resource intensive than extracting it from the LR.  In some cases it takes a few days to import a single day's data from the LR.  Fortunately most of the time LR activity is very low so there is time for the import to catch up.</p>
<p>The LR takes an eventual consistency approach to managing their data, and we are adapting this approach to the import process.  We visualize teachers doing searches on the database well into the night, and probably starting early in the morning, so have taken the following approach to limiting the hours that the import will run in:</p>
<ul>
  <li>At the end of processing each file in the queue, the file is removed from the queue and placed in an archive folder.</li>
  <li>Data Warehouse totals are updated (this takes under a minute).</li>
  <li>The current time is checked.  If the current time is during the window where the import is not allowed to run (this window is configurable), the import ends.  The next time the import begins, any new data in the LR is added to the end of the queue, and the import picks up at the point where it left off during the previous run.</li>
</ul>

<h2>Transforming and Loading into the Database</h2>
<p>There are two basic types of data in the LR, metadata and paradata.  Metadata is data about the resource.  It includes but is not limited to title, description, education level, subject area, and access and use rights.  Paradata is a specialized form of metadata that contains data about the usage of a resource.  It includes but is not limited to views, favorites, comments, and ratings.</p>
<p>It is possible that multiple entities have submitted metadata and paradata about a resource.  IOER combines metadata from multiple sources into a single object in an attempt to give a more accurate picture of the qualities of the resource.</p>
<p>Paradata from multiple submitters for a single resource will also be combined.  Here's an example showing why it should be this way:</p>
<p>National Science Digital Library publishes an article about how to solve linear equations to the LR.  A teacher from Olympia School District rates the article 4 out of 5 stars.  If the results are kept separate, nobody will know that the teacher from Olympia rated the NSDL-published article.</p>

<h3>Determine Schema</h3>
<p>The payload_schema property of the envelope is used to determine which handler to use for processing the LR document.  This can be a single field or an array.  When it is an array, the payload contains elements from multiple schemas.</p>

<h2>Validating Minimum Requirements</h2>

<h3>Documents Published by IOER</h3>
<p>Documents published by Illinois OER come from us.  They are already in our database, so there is no need to import these documents.  These documents are ignored and not logged.</p>

<h3>Spam Detection</h3>
<p>Currently, spam detection consists of examining the LR document for words on a bad word list.  The bad word list includes various swear words, misspellings of swear words, names of pharmaceutical products commonly associated with email spam, and other words commonly associated with email spam.  If the LR document contains any of the words on the list, it is automatically flagged as spam and the record is thrown away.  Spam detection occurs immediately before the record is transformed and loaded into the database.</p>

<h3>Alternative to Physical Deletes</h3>
<p>On the resource version table there is a field called IsActive.  This field is used when we do not want to display a resource in the search, but do not want to remove it (for example, maybe the resource is good but it is severely lacking in metadata).  In such a case, IsActive is set to false, so this version of the resource will not display in search results.  Cases where IsActive is set to false include:</p>
<ul>
  <li>The title is numeric</li>
  <li>The title is a date</li>
  <li>The title does not meet minimum length requirements (currently titles must be at least six characters long)</li>
</ul>

<h3>Audit Error and Warning Process</h3>
<p>Errors and warnings are stored in a table along with the docID and filename containing the LR document.  In this way we can review errors and warnings and handle as appropriate, including tweaking the import and reprocessing the record.</p>

<h3>Cleansing Age Ranges</h3>
<p>Age ranges come from the LR in various formats.  It is not unusual to see age ranges like "-14+" or " --15-U" in addition to age ranges that make sense like "14-18."  Age ranges are cleaned up using the following process:</p>
<ol>
  <li>All whitespace is removed from the age range.</li>
  <li>If equal to "U-" change to "0-99"</li>
  <li>Month abbreviations are converted to numbers (for example, May-8 would be converted to 5-8)</li>
  <li>HTML entities for <b>&gt;</b> and <b>&lt;</b> are converted to their characters.</li>
  <li><b>&gt;<i>age</i></b> is converted to <b><i>age-</i>>99</b> (for example, &gt;21 is converted to 21-99).</li>
  <li><b>&lt;age</b> is converted to <b>0-<i>age</i></b> (for example, &lt;5 is converted to 0-5).</li>
  <li><b>+</b> is converted to <b>-</b> if it is not the last character in the string.</li>
  <li>Ending <b>+</b> is converted to <b>-99</b>.</li>
  <li>Two or more 9's are converted to 99.</li>
  <li>Leading <b>-</b> characters are removed.</li>
  <li>Multiple consecutive <b>-</b> characters are removed.</li>
  <li>If there are two numbers separated by a <b>-</b> character, remove any trailing <b>-</b> characters, otherwise replace any trailing <b>-</b> with <b>-99</b>.</li>
  <li>By this point there should be two numbers separated by a <b>-</b>. If not, make it a range with only one number (for example, <b>18</b> becomes <b>18-18</b>).</li>
  <li>Replace <b>-U</b> with <b>-99</b>.</li>
  <li>Make sure ages are in the correct order (so <b>99-14</b> becomes <b>14-99</b>).</li>
  <li>Drop any leading zeroes (so 09-10 becomes 9-10).</li>
</ol>

<p>Age ranges, where possible, will be mapped to Grade Levels, unless Grade Levels are also present in the document received from the LR.  This will be done as follows:</p>
<ol>
  <li>If the ending age is greater than 21, map to "General Public."</li>
  <li>If the ending age is between 18 and 22, and the age range is less than or equal to 4, map to appropriate (college) grade levels.</li>
  <li>If the ending age is less than 18, map to K-12 grade levels, including Pre-Kindergarten.</li>
  <li>Otherwise, do not map to grade level.</li>
</ol>

<p>Grade Levels will be converted to Age Ranges where grade levels are available but age ranges are not. This will be done by mapping tables.</p>


<h3>Cleansing Subjects and Keywords</h3>
<p>Occasionally multiple subjects and multiple keywords come through on a single subject or keyword element.  This is not desirable.  If a subject (or keyword) comes through with semicolons in it but does not contain ampersands (that is, it contains semicolons but no HTML entities), the subject or keyword is split on semicolon and each keyword is stored separately.</p>

<h3>Data Normalization</h3>
<p>Different schemas (and indeed different submitters using the same schema) use different vocabulary to describe their data.  This data is normalized to the vocabulary that ISLE is using for storing and displaying data via mapping tables that allow us to map many other vocabularies to our own.</p>
<p>Mapping tables contain the rules that convert the various vocabularies to our values.  For each field crosswalked in the previously mentioned über-crosswalk, there is a mapping table that is used to convert these vocabularies to the ISLE vocabulary.  It is possible for a single LR value to map to multiple values in our vocabulary.  For example, an age range of 9-10 could map to Grade 4 as well as Grade 5, so two rows would be inserted in the Education Level table.</p>

<h3>Orphan Tables</h3>
<p>In the event that a value exists which does not have a rule to crosswalk it to our vocabulary, it is stored in an "Orphan table" so that it is not lost.  When a rule is created that will crosswalk that value to our vocabulary, a process can be ran which will crosswalk as many of the orphan table entries to our vocabulary.  Successfully crosswalked rows will be placed in their respective tables and removed from the orphan tables.  Orphan tables exist for the same fields as the mapping tables.</p>

<h3>Handling Duplicate Values</h3>
<p>It is common for duplicate values to exist in the LR - even encouraged.  For example, grade levels and subjects are commonly placed in the payload in the correct fields, as well as in keywords.  This facilitates doing a slice (one of the LR APIs) to find resources in the LR, which looks only at keyword.  So if <b>Grade 8</b> is present in both education level and keywords, the obvious correct location for this data is in education level.  Duplicate values are removed automatically, and a value will be stored in keywords only if it is not present in the other fields.</p>
<p>It is also fairly common to see the same term twice for a resource.  For example, "Algebra" can appear twice in the subject field.  In cases like this, "Algebra" will be stored only once.</p>