<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Schemas.ascx.cs" Inherits="IOER.Pages.Developers.Schemas" %>

<p>The Learning Registry (LR) is a repository of metadata and paradata about online resources.  It is currently used primarily for K-12, but should be used for P-20 data, of which K-12 is a subset.  It is designed to be a storage and transport mechanism, and not a searchable repository.</p>
<p>The Learning Registry (LR) network is a collection of LR nodes (or servers), distributed over a wide geographic area.  Each LR node has its own database.  The nodes all replicate their data to either node01 or node02, which are the two nodes at the "top" of the network.  Likewise, node01 and node02 replicate data outward to the other nodes on the network, and to each other.  Changes to one LR node's database is replicated across the network, resulting in eventual consistency of the data across nodes (for more about eventual consistency, see <a href="http://en.wikipedia.org/wiki/Eventual_consistency" target="_blank">http://en.wikipedia.org/wiki/Eventual_consistency</a>).</p>
<p>The Illinois Shared Learning Environment (ISLE) will provide a means for finding relevant resources contained in the LR.  Because the LR is not a searchable repository, data must be extracted from it, transformed into a useful, searchable form, and loaded in a database in such a way that searching becomes simple, efficient, and has the most current data available for a resource.  This document outlines how ISLE will make the contents of the LR searchable.</p>
<p>A schema is a way for a computer to validate the structure of a document.  For example, suppose I asked you "Is this a representation of a valid postal address?"</p>

<p>
  <pre>
    &lt;address&gt;
       &lt;name&gt;Jon Jones&lt;/name&gt;
       &lt;street&gt;123 W. Main Street&lt;/street&gt;
       &lt;city&gt;Timbuktu&lt;/city&gt;
       &lt;state&gt;IL&lt;/state&gt;
       &lt;zip&gt;62999&lt;/zip&gt;
    &lt;/address&gt;
  </pre>
</p>

<p>Mentally, you would compare the address to a schema you have in your head about what an address should look like.  It should contain a person's or organization's name, or both, one or more lines for a street address, followed by exactly one each of city, state or province, and postal code.  Further, in the US a postal code must be 5 or 9 numeric digits, and if it is 9 digits long, there must be a hyphen between the 5th and 6th digits.  The above address is a valid postal address.  Using the above informally defined schema, we can determine that the following is not a valid postal address, because it has two states and the zipcode is neither 5 or 9 digits long, nor is it numeric:</p>

<p>
  <pre>
    &lt;address&gt;
       &lt;name&gt;Jane Doe&lt;/name&gt;
       &lt;street&gt;555 Boa Way&lt;/street&gt;
       &lt;city&gt;Reptile City&lt;/city&gt;
       &lt;state&gt;IL&lt;/state&gt;
       &lt;state&gt;FL&lt;/state&gt;
       &lt;zip&gt;blue&lt;/zip&gt;
    &lt;/address&gt;
  </pre>
</p>

<h2>Current Schemas</h2>
<p>Currently there are schemas present in the LR for metadata and paradata.  Metadata schemas encountered so far are:</p>
<ul>
  <li>NSDL_DC - This is a variation of the Dublin Core schema. It includes some elements from LOM such as <b>typicalLearningTime</b>. For more information about NSDL_DC, see <a href="https://wiki.ucar.edu/display/nsdldocs/nsdl_dc" target="_blank">https://wiki.ucar.edu/display/nsdldocs/nsdl_dc</a>.</li>
  <li>DC - This is the original Dublin Core schema. For more information about Dublin Core, see <a href="http://wiki.dublincore.org/index.php/User_Guide#Dublin_Core_URIs_and_namespaces" target="_blank">http://wiki.dublincore.org/index.php/User_Guide#Dublin_Core_URIs_and_namespaces</a>.</li>
  <li>LOM - This is the IEEE's schema for Learning Object Metadata. It is commonly used in Europe. For more information about LOM, see <a href="http://standards.ieee.org/findstds/standard/1484.12.3-2005.html" target="_blank">http://standards.ieee.org/findstds/standard/1484.12.3-2005.html</a>.</li>
  <li>LRMI - This is the Learning Resource Metadata Initiative schema and is available at <a href="http://www.lrmi.net/the-specification" target="_blank">http://www.lrmi.net/the-specification</a>. It includes elements from schema.org such as title and description.</li>
</ul>

<p>Paradata schemas encountered so far are:</p>
<ul>
  <li>LR Paradata 1.0 - This is an adaptation of activity streams (for more on activity streams, see <a href="http://en.wikipedia.org/wiki/Activity_stream" target="_blank">http://en.wikipedia.org/wiki/Activity_stream</a>. An example of an activity stream is Facebook's News Feed) used by the LR. Most paradata follows this schema. The LR's paradata cookbook is a valuable guide, and can be found at <a href="http://goo.gl/jJXtI" target="_blank">http://goo.gl/jJXtI</a>.</li>
  <li>comm_para - This is a paradata schema created by the National Science Digital Library and was used in the very early days of the LR before the switch to LR Paradata 1.0. For more information about the comm_para schema, see <a href="https://wiki.ucar.edu/display/nsdldocs/comm_para" target="_blank">https://wiki.ucar.edu/display/nsdldocs/comm_para</a>.  This schema has been superseded by LR Paradata 1.0</li>
</ul>

<h2>Handling New Schemas</h2>
<p>If a document comes in from the LR with an unknown schema, the record will be logged and reported.  Technical staff can then examine the record and determine if it is a valid schema that should be imported.  A new schema handler will be created and added to the import process so that the new schema will be recognized.</p>