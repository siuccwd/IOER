<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="JSONtesting.aspx.cs" Inherits="ILPathways.testing.JSONtesting" MasterPageFile="/Masters/Responsive.Master" %>

<asp:Content runat="server" ContentPlaceHolderID="BodyContent">
  <link rel="stylesheet" href="/Styles/common2.css" type="text/css" />
  <script type="text/javascript" language="javascript">

    $(document).ready(function () {
      var siteID = 5;
      var includeCounts = false;
      var includeDescriptions = true;
      var mustHaveResources = true;
      $.getJSON("/Rest/api/filters/" + siteID + "?mustHaveResources=" + mustHaveResources + "&includeCounts=" + includeCounts + "&mustHaveResources=" + mustHaveResources + "&includeDescriptions= " + includeDescriptions, function (data) { successGetFilters(data.data); });
    });

    function successGetFilters(data) {
      console.log(data);
      for (i in data) {
        $("#content").append(
          $("#template_filter").html()
            .replace(/{title}/g, data[i].title)
            .replace(/{tags}/g, renderTags(data[i].tags)
          )
        );
      }
    }

    function renderTags(data) {
      var result = "";
      for (i in data) {
        result += $("#template_tag").html()
          .replace(/{id}/g, data[i].id)
          .replace(/{title}/g, data[i].title)
          .replace(/{description}/g, data[i].description)
        }
      return result;
    }
  </script>
  <style type="text/css">
    h2 { font-size: 24px; }
    .tag { display: inline-block; width: 200px; vertical-align: top; padding: 5px; font-size: 20px; }
    .description { font-size: 80%; color: #333; word-wrap: break-word; }
  </style>

  <div id="content">

  </div>

  <div id="templates" style="display:none;">
    <div id="template_filter">
      <div class="filter">
        <h2>{title}</h2>
        {tags}
      </div>
    </div>
    <div id="template_tag">
      <a href="#" id="{id}" class="tag" data-selected="false">{title} <div class="description">{description}</div></a>
    </div>
  </div>

</asp:Content>