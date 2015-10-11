<%@ Page Title="" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="Accessibility.aspx.cs" Inherits="IOER.Pages.Accessibility" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <%--<link rel="stylesheet" href="/styles/common2.css" />--%>
  <style type="text/css">
    h3 { margin-top: 20px; font-size: 24px;}
    p, p span, p a, li, li em, p span strong {
    font-size: 24px;
    }
    #content {padding: 5px 60px;}
  </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">

  <div id="content">
		<h1 class="isleH1">Accessibility and IOER</h1>
  <p>An important goal of the Illinois Open Educational Resources (IOER) web site is to provide effective access to our website for all users, including those with disabilities.  
      IOER is committed to providing an equal opportunity for all to benefit from the tools provided on our website, and strive to reduce barriers to use.  Please bear in mind
      that many of the resources which can be found using tools such as the IOER search are not hosted by IOER, and, as such, we have no control over whether or not those
      resources meet accessibility standards.
  </p>
  <p>IOER endeavors to meet the <a href="https://www.dhs.state.il.us/page.aspx?item=32765" target="_blank">Illinois
      Information Technology Accessibility Act</a> (IITAA), <a href="http://www.section508.gov/" target="_blank">Section 508 Guidelines</a> and 
      <a href="http://www.w3.org/TR/WCAG/" target="_blank">Web Content Accessibility Guidelines 2.0</a>.
  </p>
  <p>The IITAA requires Illinois agencies and universities to ensure that their web sites, information systems, and information
      technologies are accessible to people with disabilities.  While the <a href="http://www.ada.gov/" target="_blank">Americans with Disabilities Act</a> (ADA) and 
      <a href="http://www.dol.gov/oasam/regs/statutes/sec504.htm" target="_blank">Section 504 of the Rehabilitation Act</a> already require
      the State to ensure accessibility, the IITAA establishes specific standards and encourages the State to address accessibility proactively.
  </p>
  <p>Section 508 of the Rehabilitation Act of 1973, as amended, requires that U.S. governmental agencies with information technology
      products meet specified levels of compliance with accessibility standards.  IOER is committed to support this standard for information technology design,
      development and support.
  </p>
  <p>The Web Content Accessibility Guidelines are from the <a href="http://www.w3.org/" target="_blank">World Wide Web Consortium</a> (W3C), and cover a wide range
      of recommendations for making web content more accessible.  By following these guidelines, content is made accessible to a wide range of people with disabilities,
      including blindness and low vision, deafness and hearing loss, learning disabilities, cognitive limitations, limited movement, speech disabilities, photosensitivity,
      and combinations of these.  By following these guidelines, our tools also become more usable to users in general.
  </p>
		<h2 class="isleH2">Improving the Accessibility of the IOER Website</h2>
  <p>Currently, we are in the process of improving the accessibility of the tools for sharing, curating, and creating the OERs made available through the website.  We continue to evaluate the accessibility
      of the IOER website as new functions are added, and will be monitoring the standards and tools we make available for areas that need improvement.
  </p>
  <p>For more resources related to OER Accessibility, see below.</p>
		<iframe src="//ioer.ilsharedlearning.org/Widgets/Library?library=451&collections=771" style="display:block;height:500px;width:100%;border-width:0;border-radius:5px;"></iframe>
	</div>
</asp:Content>
