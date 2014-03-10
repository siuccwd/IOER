<%@ Page Language="C#" MasterPageFile="/Masters/Pathway.Master" AutoEventWireup="true"  CodeBehind="MapResourceNSDL.aspx.cs" Inherits="ILPathways.Pages.MapResourceNSDL" %>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">

<style type="text/css">
        .table {position:relative;}
        div.table div.row {display:table; width:100%; border-collapse:collapse;}
        div.table div.row div.cell {display: table-cell;}

        div.table div.row div.cell {border-bottom:1px solid #a1b04a; background:#f8f9f4;}
        
        div.table div.row div.cell.col1 {border-left:1px solid #a1b04a; width:25%; padding-left:5px}
        div.table div.row div.cell.col2 {border-left:1px solid #a1b04a; width:50%}
        div.table div.row div.cell.col3 {border-left: 1px solid #a1b04a; border-right:1px solid #a1b04a; width:25%; padding-left:5px}

        div.table div.row.head div.cell {text-align:center; font-weight:bold; border-top:1px solid #a1b04a; background:#0073AE; color:#ffffff;}
        div.table div.row.first div.cell {border-top:1px solid #a1b04a;}
        div.table div.row.even div.cell {background:#ffffff;}
        div.table div.row.head div.cell a {color:#ffffff;}
</style>

<!-- content start -->

<style type="text/css">
  #crosswalkTable {
    width: 1000px;
    margin: 10px auto;
    font-size: 85%;
  }
  #crosswalkTable tr:nth-child(odd) {
     background-color: #F5F5F5;
  }
  #crosswalkTable th {
    background-color: #CFDEFF;
    color: #505050;
    padding: 8px;
    font-size: 120%;
  }
  #crosswalkTable th.wide {
    width: 300px;
  }
  #crosswalkTable th a {
    color: #505050;
  }
  #crosswalkTable td {
    padding: 5px;
  }
  #crosswalkTable .matching {
    font-weight: bold;
    text-align: center;
  }
  #crosswalkTable tr:hover {
    background-color: #CFDEFF;
  }
</style>

<table id="crosswalkTable">
  <tr>
    <th><a href="http://nsdl.org/contribute/metadata-guide" target="_blank">NSDL_DC</a></th>
    <th class="wide">NSDL_DC Definition</th>
    <th><a href="http://www.lrmi.net/the-specification" target="_blank">LRMI</a></th>
    <th class="wide">LRMI Definition</th>
    <th><a href="http://slcedu.org/sites/default/files/downloads/SLC_Learning_Standards_Alignment_Whitepaper_v1.2.pdf" target="_blank">SLC</a></th>
  </tr>

  <tr>
    <td class="matching">dc:title</td>
    <td>The name by which the resource or collection of resources is formally known.</td>
    <td class="matching">name</td>
    <td>The title of the resource.</td>
    <td class="matching">name</td>
  </tr>

  <tr>
    <td class="matching">dc:subject</td>
    <td>Populate each Subject field with only one subject term (or phrase) that describes the topics, concepts or content of the resource; repeat as needed.</td>
    <td class="matching">about</td>
    <td>The subject of the content.</td>
    <td class="matching">about</td>
  </tr>

  <tr>
    <td class="matching">dct:educationLevel</td>
    <td>Use to describe the appropriate learning level or range associated with a resource. A refinement of the audience element. NSDL controlled vocabulary available.</td>
    <td class="matching">typicalAgeRange</td>
    <td>The typical range of ages the content’s intendedEndUser.</td>
    <td class="matching">typicalAgeRange</td>
  </tr>

  <tr>
    <td class="matching">dct:audience</td>
    <td>A broad category that best describes the recipient or user for whom the resource is primarily intended.</td>
    <td class="matching">intendedEndUserRole</td>
    <td>The individual or group for which the work in question was produced.</td>
    <td class="matching">intendedEndUserRole</td>
  </tr>

  <tr>
    <td class="matching">dc:type</td>
    <td>The nature, function or typical use of a resource. To describe the file format, physical medium, or dimensions of the resource, use Format element.</td>
    <td class="matching">learningResourceType</td>
    <td>The predominate type or kind characterizing the learning resource.</td>
    <td class="matching">learningResourceType</td>
  </tr>

  <tr>
    <td class="matching">dc:format</td>
    <td>Physical medium and/or file/MIME format</td>
    <td class="matching">mediaType</td>
    <td>The type of media which is being described.</td>
    <td class="matching">mediaType</td>
  </tr>

  <tr>
    <td class="matching">dc:creator</td>
    <td>Entity primarily responsible for making the resource</td>
    <td class="matching">author</td>
    <td>The individual credited with the creation of the resource.</td>
    <td class="matching">author<br />/name</td>
  </tr>

  <tr>
    <td class="matching">dc:publisher</td>
    <td>Entity responsible for making the resource available.</td>
    <td class="matching">publisher</td>
    <td>The organization credited with publishing the resource.</td>
    <td class="matching">publisher<br />/name</td>
  </tr>

  <tr>
    <td class="matching">dc:language</td>
    <td>Primary language of the resource. NSDL_DC recommends use of LOC's ISO 639-2 controlled vocabulary.</td>
    <td class="matching">inLanguage</td>
    <td>The primary language of the resource.</td>
    <td class="matching">inLanguage</td>
  </tr>

  <tr>
    <td class="matching">dc:date</td>
    <td>A point or period of time associated with an event in the lifecycle of the resource. Employ W3CDTF encoding scheme that looks like YYYY-MM-DD.</td>
    <td class="matching">dateCreated</td>
    <td>The date on which the resource was created.</td>
    <td class="matching">dateCreated</td>
  </tr>

  <tr>
    <td class="matching">ieee:interactivityType</td>
    <td>The type of interactions supported by a resource (active, expositive, mixed, undefined)</td>
    <td class="matching">interactivityType</td>
    <td>The predominate mode of learning supported by the learning resource. Acceptable values are active, expositive, or mixed.</td>
    <td class="matching">interactivityType</td>
  </tr>

  <tr>
    <td class="matching">ieee:typicalLearningTime</td>
    <td>The typical amount of time for a particular education level to interact with the resource.</td>
    <td class="matching">timeRequired</td>
    <td>Approximate or typical time it takes to work with or through this learning resource for the typical intended target audience.</td>
    <td class="matching">timeRequired</td>
  </tr>

  <tr>
    <td class="matching">dc:rights</td>
    <td>Rights information typically includes a free-text statement about various property rights associated with the resource, including intellectual property rights. May be populated with a URL that links to specific rights language in the resource.</td>
    <td class="matching">useRightsUrl</td>
    <td>The URL where the owner specifies permissions for using the resource.</td>
    <td class="matching">useRightsUrl</td>
  </tr>

  <tr>
    <td class="matching">dct:conformsTo<br />xsi:type="dct:URI"</td>
    <td>A refinement of the Relation element. Also used to provide educational standard via a URI (e.g. as ASN URIs).</td>
    <td class="matching">educationalAlignment<br />/targetUrl</td>
    <td>The URL of a node in an established educational framework.</td>
    <td class="matching">educationalAlignment<br />/targetUrl</td>
  </tr>

  <tr>
    <td class="matching">dc:relation</td>
    <td>A related resource. Best practice to express relationships to related resources and the item being cataloged is to employ the applicable refinements below. Enter either the title and/or URL of the related resource.</td>
    <td class="matching">isBasedOnUrl</td>
    <td>A resource that was used in the creation of this resource. This term can be repeated for multiple sources.</td>
    <td class="matching">isBasedOnUrl</td>
  </tr>

  <tr>
    <td class="matching">dc:identifier</td>
    <td>URL to the resource.</td>
    <td></td>
    <td></td>
    <td class="matching">url</td>
  </tr>

  <tr>
    <td></td>
    <td></td>
    <td class="matching">educationalAlignment<br />/alignmentType</td>
    <td>A category of alignment between the learning resource and the framework node. Recommended values include: ‘assesses’, ‘teaches’, ‘requires’, ‘textComplexity’, ‘readingLevel’, ‘educationalSubject’, and ‘educationLevel’.</td>
    <td class="matching">educationalAlignment<br />/alignmentType</td>
  </tr>

  <tr>
    <td></td>
    <td></td>
    <td class="matching">educationalUse</td>
    <td>The purpose of the work in the context of education.</td>
    <td class="matching">educationalUse</td>
  </tr>

  <tr>
    <td></td>
    <td></td>
    <td class="matching">educationalAlignment<br />/targetName</td>
    <td>The name of a node in an established educational framework.</td>
    <td class="matching">educationalAlignment<br />/targetName</td>
  </tr>

  <tr>
    <td class="matching">dct:accessRights</td>
    <td>Information describing conditions or requirements for viewing and/or downloading NSDL material. NSDL controlled vocabulary available; a refinement of the Rights element.</td>
    <td></td>
    <td></td>
    <td></td>
  </tr>

  <tr>
    <td class="matching">dc:description</td>
    <td>A free-text account of a resource. May include abstracts or table of contents. Used as primary search field and display field.</td>
    <td></td>
    <td></td>
    <td></td>
  </tr>


  <tr>
    <td></td>
    <td></td>
    <td></td>
    <td>Group Type is pulled from SLC's metadata tagger and search tools. We have yet to find documentation on its purpose or how it might relate to other metadata tags in NSDL/LRMI schema.</td>
    <td class="matching">groupType</td>
  </tr>


</table>
</asp:Content>