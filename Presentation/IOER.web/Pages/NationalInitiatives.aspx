<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="NationalInitiatives.aspx.cs" Inherits="ILPathways.Pages.NationalInitiatives" MasterPageFile="/Masters/Responsive.Master" %>

<asp:Content ContentPlaceHolderID="HeadContent" runat="server">
  <link rel="stylesheet" type="text/css" href="/Styles/common2.css" />
</asp:Content>
<asp:Content ContentPlaceHolderID="BodyContent" runat="server">
  <style type="text/css">
    p.center { text-align: center; }
    .partnerBox { max-width: 800px; margin: 10px auto; position: relative; min-height: 120px; }
    .partnerBox .icon { width: 250px; height: 100px; border-radius: 5px; background-color: #FFF; padding: 10px; position: absolute; top: 10px; left: 10px; }
    .partnerBox .icon a { display: block; width: 100%; height: 100%; background: #FFF url('') center center no-repeat; background-size: contain; }
    .partnerBox .text { padding: 5px; width: 100%; padding-left: 260px; }

    #icon_lr { background-image: url('/images/logo_lr.png'); }
    #icon_lrmi { background-image: url('/images/logo_lrmi.png'); }
    #icon_asn { background-image: url('/images/logo_asn.png'); }
    #icon_cc { background-image: url('/images/logo_cc.jpg'); }
    #icon_achieve { background-image: url('/images/logo_achieve.png'); }
    #icon_ceds { background-image: url('/images/logo_ceds.jpg'); }

    @media screen and (max-width: 525px) {
      .partnerBox .icon { position: static; margin: 0 auto 5px auto; }
      .partnerBox .text { padding-left: 0; }
    }
  </style>
  <div id="content">
    <h1 class="isleH1">National Initiatives</h1>
    <p class="center">The ISLE Open Educational Resource program participates with the following national initiatives:</p>

    <div class="partnerBox grayBox">
      <div class="icon"><a id="icon_lr" href="http://learningregistry.org" target="_blank"></a></div>
      <div class="text">
        <p>ISLE OER operates a node in the Learning Registry network to share Resources.</p>
        <p><a class="textLink" href="http://learningregistry.org" target="_blank">http://learningregistry.org</a></p>
      </div>
    </div>

    <div class="partnerBox grayBox">
      <div class="icon"><a id="icon_lrmi" href="http://www.lrmi.net" target="_blank"></a></div>
      <div class="text">
        <p>ISLE OER uses the LRMI Metadata Schema adopted by <a class="textLink" href="http://www.schema.org" target="_blank">schema.org</a>.</p>
        <p><a class="textLink" href="http://www.lrmi.net" target="_blank">http://www.lrmi.net</a></p>
      </div>
    </div>

    <div class="partnerBox grayBox">
      <div class="icon"><a id="icon_asn" href="http://asn.jesandco.org" target="_blank"></a></div>
      <div class="text">
        <p>ISLE OER uses the Achievement Standards Network open metadata framework for learning standards, competencies, and rubrics.</p>
        <p><a class="textLink" href="http://asn.jesandco.org" target="_blank">http://asn.jesandco.org</a></p>
      </div>
    </div>

    <div class="partnerBox grayBox">
      <div class="icon"><a id="icon_cc" href="http://creativecommons.org" target="_blank"></a></div>
      <div class="text">
        <p>ISLE OER provides users direct access to Creative Commons attribution licenses to describe access to Resources.</p>
        <p><a class="textLink" href="http://creativecommons.org" target="_blank">http://creativecommons.org</a></p>
      </div>
    </div>

    <div class="partnerBox grayBox">
      <div class="icon"><a id="icon_achieve" href="http://achieve.org" target="_blank"></a></div>
      <div class="text">
        <p>ISLE OER provides an online OER evaluation rubric based on Achieve's OER Rubrics and a derivative of Achieve's EQuIP Rubrics.</p>
        <p><a class="textLink" href="http://achieve.org" target="_blank">http://achieve.org</a></p>
      </div>
    </div>

    <div class="partnerBox grayBox">
      <div class="icon"><a id="icon_ceds" href="https://ceds.ed.gov" target="_blank"></a></div>
      <div class="text">
        <p>ISLE OER uses selected items of the Common Education Data Standards vocabulary in collaboration with the <a class="textLink" href="http://www2.ed.gov/programs/racetothetop/index.html" target="_blank">Race to the Top Project</a> states.</p>
        <p><a class="textLink" href="https://ceds.ed.gov" target="_blank">https://ceds.ed.gov</a></p>
      </div>
    </div>

  </div>

</asp:Content>