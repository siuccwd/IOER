<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Curriculum1.ascx.cs" Inherits="ILPathways.Controls.Curriculum.Curriculum1" %>
<%@ Register TagPrefix="uc1" TagName="SocialBoxControl" Src="/Controls/SocialBox/SocialBox1.ascx" %>

<link rel="stylesheet" type="text/css" href="/Styles/common2.css" />
<script type="text/javascript">
    //From server
    <%=startingNodeId %>
    <%=userGUID %>
    <%=nodes %>


</script>
<script type="text/javascript">
  var treeBox;
  var treeTemplate;
  var loadedData = [];
  $(document).ready(function () {
    if (nodes.isValid) {
      renderTree(nodes.data);
    }
    else {
      alert(nodes.status);
    }

    $(".treeNode").not("[data-depth=d0]").find(".expand").trigger("click");

    $(".isleBox.column.right").appendTo("#treeColumn");

    if (startingNodeId > 0) {
        loadData(startingNodeId);
        //$("#treeColumn").css("display", "none");
    }
    else {
        $(".nodeControls .titleBlock").first().trigger("click");
        $("#treeColumn").css("display", "inline-block");
    }


    //$('.section').hide();
    $('h2.toggle').toggle(
    function () {
            $(this).next('.section').slideDown();
            $(this).addClass('close');
    },
    function () {
            $(this).next('.section').fadeOut();
            $(this).removeClass('close');
    }
    ); // end toggle
  });

  //Rendering functions
  function renderTree(data) {
    treeBox = $("#tree");
    treeTemplate = $("#template_treeNode").html();
    $("#curriculumName").html(data.title);
    treeBox.html("");
    
    renderBranch(data, treeBox, 1, "", true, 0);

    $("#tree .expand").on("click keyup", function (e) {
      if (e.type == "click" || (e.type == "keyup" && (e.keyCode == "13" || e.which == "13"))) {
        var target = $("#tree .children[data-childrenOfID=" + $(this).attr("data-targetID") + "]");
        if (target.html() != "") {
          target.slideToggle();
        }
        if (target.attr("data-expanded") == "true") {
          target.attr("data-expanded", "false");
          $(this).html("+");
        }
        else {
          target.attr("data-expanded", "true");
          $(this).html("-");
        }
      }
    });

    $(".titleBlock").on("click keyup", function (e) {
      if (e.type == "click" || (e.type == "keyup" && (e.keyCode == "13" || e.which == "13"))) {
        loadData(parseInt($(this).attr("data-nodeID")));
      }
    });
  }
  
  function renderBranch(data, place, layer, prefix, isParent, depth) {
    var layerText = prefix + layer;
    if (isParent) {
      layerText = "";
    }

    //append the item to the tree
    place.append(
      treeTemplate
        .replace(/{id}/g, data.id)
        .replace(/{parentID}/g, data.parentID)
        .replace(/{title}/g, data.title)
        .replace(/{layer}/g, layerText)
        .replace(/{depth}/g, depth)
    );

    //render the child items
    if (!isParent) {
      prefix += layer + ".";
    }
    var count = 1;
    for (i in data.children) {
      renderBranch(data.children[i], treeBox.find(".children[data-childrenOfID=" + data.id + "]"), count, prefix, false, depth + 1);
      layer++;
      count++;
    }

    if (data.children.length == 0) {
      treeBox.find(".expand[data-targetID=" + data.id + "]").remove();
    }
  }

    function renderData(data) {
    $("#selectedTitle").html(data.title);
    $("#loadingMessage").css("display", "none");
    $("#documentItemSection").css("display", "none");
    $("#nodeDownload").css("display", "none");

    if (data.resourceUrl == null || data.resourceUrl.length == 0) {
        $("#nodeLinks").css("display", "none");
    } else {
        $("#nodeLinks").css("display", "inline");
        $("#detailUrl").attr("href", data.resourceUrl);
    }

    $("#selectedDescription").css("display", "block");
    $("#selectedDescription").html(data.description);
    $("#nodeItems").html("");
    $("#nodeChildStandards").html("");
    
    if (data.standards.length == 0) {
        $("#nodeStandards").css("display", "none");
        $("#nodeStandards").html("");

    }
    else {
        $("#nodeStandards").css("display", "block");
        $("#nodeStandards").html("<h2  class=\"toggle message\">Standards associated with this " + data.contentType + "</h2>");
      var standardsText = "<div id=\"nodeStandardsDiv\" class=\"section\"><ul class=\"standardsList\">";
      for (i in data.standards) {
        standardsText += "<li><b>" + data.standards[i].code + "</b>" + data.standards[i].description + "</li>";
      }
      standardsText += "</ul></div>";
      $("#nodeStandards").append(standardsText);
    }
    
    if (data.children.length > 0
            || (data.contentType == "Module" || data.contentType == "Unit")) {

        $("#nodeDownload").css("display", "inline-block");
        $("#downloadUrl").attr("href", data.downloadUrl);
            $("#downloadUrl2").attr("href", data.downloadUrl + "&all=false");

        $("#nodeItems").css("display", "block");
        $("#nodeItems").html("<h2 class=\"message\">Resources associated with this " + data.contentType + ":</h2>");

        var template = $("#template_nodeItem").html();
        for (i in data.children) {
        var standardsText = "";
        if (data.children[i].standards.length > 0) {
            standardsText = "<ul class=\"standardsList\">";
            standardsText += "<lh>This Resource aligns to these Standards:</lh>";
            for (j in data.children[i].standards) {
            standardsText += "<li><b>" + data.children[i].standards[j].code + "</b>" + data.children[i].standards[j].description + "</li>";
            }
            standardsText += "</ul>";
        }
        $("#nodeItems").append(
            template
            .replace(/{documentUrl}/g, data.children[i].documentUrl)
            .replace(/{resourceUrl}/g, data.children[i].resourceUrl)
            .replace(/{title}/g, data.children[i].title)
            .replace(/{message}/g, data.children[i].message)
            .replace(/{description}/g, data.children[i].description)
            .replace(/{standards}/g, standardsText)
        );
        if (data.children[i].documentUrl == "#") {
            $("#nodeItems .nodeItem").last().find(".resourceLinks").remove();
        } else {
            $("#nodeItems .nodeItem").last().find(".itemMessage").remove();
        }
      }
    } else {
        $("#nodeItems").css("display", "none");
        $("#nodeItems").html("");
          // $("#nodeItems").html("<h2 class=\"message\">There are no Resources associated with this " + data.contentType + "</h2>");

        if (data.contentType == 'Document' && data.documentUrl.length > 5) {
            $("#documentItemSection").css("display", "inline-block");
            $("#documentItemLink").attr("href", data.documentUrl);
            $("#nodeMessage").html(data.message);
        }
    }

    if (data.childStandards.length == 0) {
        $("#nodeChildStandards").css("display", "none");
        $("#nodeChildStandards").html("");
      //$("#nodeChildStandards").html("<p class=\"message\">There are no Standards associated with Resources belonging to nodes that are a part of this " + data.contentType + "</p>");
    }
    else {
        $("#nodeChildStandards").css("display", "block");
        $("#nodeChildStandards").html("<div id='childStandardsHdr' ><h2 onclick=\"ShowHideSection('nodeChildStandardsDiv');\"  class=\"message\">+/- Standards associated with Resources belonging to nodes that are a part of this " + data.contentType + "</h2></div>");
        var standardsText = "<div id=\"nodeChildStandardsDiv\"  style=\"display:none\"><ul class=\"childStandardsList\">";
      for (i in data.childStandards) {
        standardsText += "<li><b>" + data.childStandards[i].code + "</b>" + data.childStandards[i].description + "</li>";
      }
      standardsText += "</ul><div/>";
      $("#nodeChildStandards").append(standardsText);
    }
  }

  function ShowHideSection(target) {
      $("#" + target).slideToggle();
  }

  function ShowHideSection2(target) {
      $("#" + target).slideToggle();
  }

  function ToggleTreeview(target) {
      $("#" + target).slideToggle();
      var current = $("#toggleTree").html();
      if (current == "Show Curriculum Outline") {
          $("#toggleTree").html('Show Node Detail');
      }
      else {
          $("#toggleTree").html('Show Curriculum Outline');

      }
  }

  //AJAX functions
  function loadData(id) {
      $("#loadingMessage").css("display", "block");
      $("#nodeLinks").css("display", "none");
      $("#nodeDownload").css("display", "none");

      var windowWidth = $(window).width();
      if (windowWidth < 900) {
          $("#treeColumn").css("display", "none");
          $("#toggleSection").css("display", "block");
          $("#toggleTree").html('Show Curriculum Outline');

      } else {
          $("#treeColumn").css("display", "inline-block");
          $("#toggleSection").css("display", "none");
      }

      $("#selectedDescription").css("display", "none");
      $("#nodeStandards").css("display", "none");
      $("#nodeItems").css("display", "none");
      $("#nodeChildStandards").css("display", "none");

    var found = false;
    for (i in loadedData) {
      if (loadedData[i].id == id) {
        renderData(loadedData[i]);
        found = true;
        break;
      }
    }

    if (!found) {
        doAjax("DisplayNode", { userGUID: userGUID, id: id }, successDisplayNode);
    }
  }

  function doAjax(method, data, success) {
    $.ajax({
      url: "/Services/CurriculumService.asmx/" + method,
      async: true,
      success: function (msg) {
        try {
          success($.parseJSON(msg.d));
        }
        catch (e) {
          success($.parseJSON(msg.d));
        }
      },
      type: "POST",
      data: JSON.stringify(data),
      dataType: "json",
      contentType: "application/json; charset=utf-8"
    });

  }

  function successDisplayNode(data) {
    if (data.isValid) {
      var found = false;
      for (i in loadedData) {
        if (loadedData[i].id == data.data.id) {
          loadedData[i] = data.data;
          found = true;
          break;
        }
      }
      if (!found) {
        loadedData.push(data.data);
      }

      renderData(data.data);
    }
    else {
      console.log("error:");
      console.log(data);
      alert(data.status);
    }
  }


</script>
<style type="text/css">
  #content { padding-left: 5px; min-height: 500px; }
  #treeColumn { width: 400px; }
  #data { width: calc(100% - 410px); }
  #treeColumn, #data { display: inline-block; vertical-align: top; }
  #tree .titleBlock { padding: 0px 2px; border-radius: 5px; }
  #tree .treeNode[data-depth=d0] > .nodeControls * { font-size: 26px; font-weight: bold; }
  #tree .treeNode[data-depth=d1] > .nodeControls * { font-size: 22px; font-weight: bold; }
  #tree .treeNode[data-depth=d2] > .nodeControls * { font-size: 18px; font-weight: bold; }
  #tree .treeNode[data-depth=d3] > .nodeControls * { font-size: 16px; font-weight: bold; }
  #tree .children { padding-left: 5px; }
  #tree .layer { color: #777; font-style: italic; padding-right: 5px; font-size: 0.9em; }
  /* #tree .treeNode:hover, #tree .treeNode:focus { box-shadow: 0 0 20px -5px #FF5707 inset; } */
  /* #tree .treeNode:hover, #tree .treeNode:focus { border-left: 1px solid #FF5707; } */
  .expand { background-color: #4AA394; color: #FFF; font-weight: bold; display: inline-block; height: 1em; width: 1em; border-radius: 5px; text-align: center; line-height: 1em; }
  .expand:hover, .expand:focus, .titleBlock:hover, .titleBlock:focus { cursor: pointer; background-color: #FF5707; color: #FFF; }
  #tree .titleBlock:hover .layer, #tree .titleBlock:focus .layer { color: #FFF; }
  #selectedDescription { padding: 5px 10px; }
  p.message { text-align: center; color: #555; font-style: italic; margin: 15px 5px 5px 5px; }
  .nodeItem { background-color: #FFF; box-shadow: 0 0 10px -2px #CCC; margin: 5px 10px; }
  .standardsList b, .childStandardsList b { display: block; }
  .standardsList li, .childStandardsList li { max-height: 5em; overflow: hidden; text-overflow: ellipsis; margin-bottom: 10px; position: relative; }
  .standardsListXX li:after, .childStandardsListXX li:after { content: ""; position: absolute; top: 2em; height: 3em; left: -15px; right: -15px; box-shadow: 0 -20px 35px #FFF inset; }
  .isleBox.column.right { float: none; }
  .pageContent { width: 99%; }
  #treeColumn .isleBox.column.right { width: 100%; margin: 10px 0; }
  .nodeItem h3 { margin-bottom: 0; }
  .resourceLinks { font-size: 80%; margin: 0; }
  .resourceLinks a:hover, .resourceLinks a:focus { text-decoration: underline; }
  .itemMessage { color: red; text-align: left;  }
    #nodeItems { padding-left: 10px;}

  #nodeStandards, #nodeChildStandards {
  background-color: #F5F5F5;
  box-shadow: 3px 3px 4px #AAA;
  border-radius: 5px;
  margin: 10px;
  padding: 5px;
  color: #4F4E4F;
  -moz-box-sizing: border-box;
  -webkit-box-sizing: border-box;
  box-sizing: border-box;
  display:none;
}

#toggleSection { background-color: #3572B8; color: #fff; text-align: center; border-radius: 5px; padding: 5px 10px; margin-bottom: 5px; width: 400px;}
#toggleSection a, #toggleSection a:visited, #toggleSection a:focus {color: #fff;}
#toggleSection:hover{background-color: #FF5707; color: #000;}

h2.toggle {
	background: url(/images/icons/open.png) no-repeat 0 11px;
	padding: 10px 0 0 25px;
	cursor: pointer;
}
h2.close {
	background-image: url(/images/icons/close.png);
}
#nodeDownload { margin-left: 10px; text-align: center; display: none; background-color:#3572B8; color: #fff; border-radius: 5px; }
#nodeDownload a { display: inline-block; color: #fff; padding: 5px; font-weight: bold; line-height: 25px; border-radius: 5px; font-size: 20px; }
#nodeDownload a img, #nodeDownload2 a img { height: 25px; vertical-align: middle; }
#nodeDownload:hover, #nodeDownload2:hover{background-color: #FF5707; color: #000;}

@media print 
{
	#nodeStandardsDiv, #nodeChildStandardsDiv { display: block;  }	
}
@media screen and (max-width: 850px) {
  #data { width: 95%; }
}

@media screen and (max-width: 800px) {
#toggleTree { display: inline-block; }
#toggleSection { width: 300px; }
}

@media screen and (max-width: 450px) {
  
  #toggleSection { width: 95%; }
    #tree .treeNode[data-depth=d0] > .nodeControls * { font-size: 18px; font-weight: bold; }
  #tree .treeNode[data-depth=d1] > .nodeControls * { font-size: 16px; font-weight: bold; }
  #tree .treeNode[data-depth=d2] > .nodeControls * { font-size: 14px; font-weight: bold; }
  #tree .treeNode[data-depth=d3] > .nodeControls * { font-size: 12px; font-weight: bold; }
}
/* </a>*/
</style>
<div id="content">
  <h1 class="isleH1" id="curriculumName" style="display:none;">Curriculum</h1>
    
    <uc1:SocialBoxControl ID="SocialBox" runat="server" shareType="curriculum" Visible="false" />

    <div class="isleBox" id="shareBox" runat="server">
      <h2 class="isleBox_H2">Share this Curriculum</h2>
      <p>Copy and paste this into the HTML of your website (<a href="/widgets#tips">See here</a> for details on styling it)</p>
      <input type="text" readonly="readonly" onclick="this.select();" style="width:100%;" value="<iframe src=&#34;http://ioer.ilsharedlearning.org/widgets/curriculum?cidx=<%=CurrentRecordID %>&#34;></iframe>" />
    </div>

    <div  id="toggleSection"><a href="javascript:void(0);" ><h2 id="toggleTree" onclick="ToggleTreeview('treeColumn');" class="message">Show Curriculum Outline</h2></a></div>
  <div id="treeColumn">
    <div id="tree" class="grayBox"></div><!--/tree-->
  </div>

  <div id="data">

    <h2 id="selectedTitle" class="isleH2">Select an item to continue</h2>
     <p id="nodeLinks" style="display: none;"><a id="detailUrl" href="{resourceUrl}" target="resDetl2">View This Resource's Tags</a> </p>
      <p id="loadingMessage">Loading, Please wait... <br /> <img src="/images/icons/loader.gif" alt="loading icon" /></p>

    <p id="selectedDescription"></p>
    <p id="documentItemSection" style="margin-left: 10px;"><a id="documentItemLink" href="#" target="resDetl2">View Resource</a></p>
      <p id="nodeMessage" class="itemMessage"></p>
    <div id="nodeStandards"></div>
    <div id="nodeChildStandards"></div>
    <div id="nodeDownload">
      <a id="downloadUrl" href="/Repository/Download.aspx?nid=2220" target="_blank">
        <img src="/images/icons/download-orange.png" /> 
        Download This Resource's related documents
      </a> 
    </div>
          <div id="nodeDownload2" style="display:none;">
      <a id="downloadUrl2" href="/Repository/DownloadFiles.aspx?all=false&nid=2220" target="_blank">
        <img src="/images/icons/download-orange.png" /> 
        Download This Resource's related documents (current section only)
      </a> 
    </div>
    <div id="nodeItems"></div>
  </div><!--/data-->
</div>

<div id="templates" style="display:none;">
  <div id="template_treeNode">
    <div class="treeNode" data-id="{id}" data-depth="d{depth}" data-parentID="{parentID}" >
      <div class="nodeControls"><span class="expand" tabindex="0" data-targetID="{id}">-</span> <span class="titleBlock" tabindex="0" data-nodeID="{id}"><span class="layer" style="display:none;">{layer}</span> <span class="nodeTitle">{title}</span></span></div>
      <div class="children" data-childrenOfID="{id}" data-expanded="true"></div>
    </div>
  </div>
  <div id="template_nodeItem">
    <div class="nodeItem grayBox">
      <h3>{title}</h3>
      <p class="resourceLinks"><a href="{documentUrl}" target="resDetl2">View Resource</a> | <a href="{resourceUrl}" target="resDetl2">View This Resource's Tags</a></p>
        <p class="itemMessage">{message}</p>
      <p>{description}</p>
      <p>{standards}</p>
    </div>
  </div>
<div id="template_docOnlyNodeItem">
    <div class="nodeItem grayBox">
      <h3>{title}</h3>
      <p class="resourceLinks"><a href="{documentUrl}" target="resDetl2">View Resource</a> | <a href="{resourceUrl}" target="resDetl2">View This Resource's Tags</a></p>
        <p class="itemMessage">{message}</p>
      <p>{description}</p>
      <p>{standards}</p>
    </div>
  </div>

<div id="template_ChildStandardsHdr">
<div id="childStandardsHdr">
    <span class="expand" tabindex="0" >+</span>
    <span class="nodeTitle">{title}</span>
    <h2 class="message" onclick="ShowHideSection('nodeChildStandardsDiv');" >Standards associated with Resources belonging to nodes that are a part of this Curriculum:</h2>

</div>

  </div>



</div>