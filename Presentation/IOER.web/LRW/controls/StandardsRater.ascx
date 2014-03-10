<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="StandardsRater.ascx.cs" Inherits="ILPathways.LRW.controls.StandardsRater" %>

<!-- Server Data -->
<script type="text/javascript">
  var contextData = <%=contextData %>;
  var standardsData = [];
</script>
<!-- Server Communication -->
<script type="text/javascript">
  $(document).ready(function () {
    getStandardsData();
  });

  function getStandardsData() {
    //Use contextData to get Standards via AJAX
    var message = { standardID: 0, rating: 0, userGUID: contextData.guid, intID: contextData.intID };
    hitServer("FetchStandardsData", message, resetData);
  }

  function rate(id, rating) {
    $(".standardsRatingSelector[standardID=" + id + "] a").removeClass("selected");
    $(".standardsRatingSelector a[ratingLinkID=" + id + "-" + rating + "]").addClass("selected");
  }

  function saveRating(standardID) {
    var targetRating = $(".standardsRatingSelector[standardID=" + standardID + "] a.selected").attr("rating");
    //send an AJAX request using the standard ID, the issued rating, and context data
    var message = { id: standardID, rating: targetRating, userGUID: contextData.guid, intID: contextData.intID };
    hitServer("RateStandard", message, resetData);
  }

  function hitServer(serviceName, message, targetFunction) {
    $.ajax({
      url: "/Services/WebDALService.asmx/" + serviceName,
      async: false,
      success: function (msg) { targetFunction(msg.d) },
      type: "POST",
      data: JSON.stringify({ input: message }),
      contentType: "application/json; charset=utf-8",
      dataType: "json"
    });
  }

  function resetData(data) {
    standardsData = data;
    renderExistingStandards();
  }

</script>
<!-- Rendering -->
<script type="text/javascript">
  function renderExistingStandards() {
    $("#existingStandards").html("");
    for (i in standardsData) {
      if (contextData.guid != "") {
        if (standardsData[i].userRated) {
          var ratingSystem = "<div class=\"youRated\">You rated this Standard.</div>";
        }
        else {
          ratingSystem = $("#template_ratingSystem").html();
        }
      }
      if (contextData.guid == "") { ratingSystem = ""; }
      $("#existingStandards").append(
        $("#template_existingStandard").html()
          .replace(/{tab}/g, "tabindex=\"0\"")
          .replace(/{align}/g, standardsData[i].align)
          .replace(/{link}/g, standardsData[i].link)
          .replace(/{code}/g, standardsData[i].code)
          .replace(/{text}/g, standardsData[i].text)
          .replace(/{current}/g, standardsData[i].rating)
          .replace(/{ratingSelector}/g, ratingSystem)
          .replace(/{id}/g, standardsData[i].id)
      );
    }
  }
</script>

<style type="text/css">
  .existingStandard { position: relative; box-sizing: border-box; -moz-box-sizing: border-box; border-radius: 5px; margin-bottom: 15px; }
  .existingStandard:hover, .existingStandard:focus { background-color: #EEE; }
  .existingStandard > p { font-weight: bold; padding: 2px; margin: 0; }
  .existingStandard .existingStandardDetail { position: absolute; right: 0; bottom: 100%; width: 250px; z-index: 1000; display: none; }
  .existingStandard:hover .existingStandardDetail, .existingStandard:focus .existingStandardDetail { display: block; }
  .existingStandard .standardsRatingSelector { display: block; overflow: hidden; width: 100%; margin: 3px auto; border-radius: 5px; font-size: 0; background-color: #E6E6E6; }
  .existingStandard .standardsRatingSelector * { font-size: 12px; display: inline-block; vertical-align: top; width: 20%; text-align: center; padding: 2px 0; margin: 0; }
  .existingStandard .standardsRatingSelector a { color: #222; font-weight: bold; height: 24px; line-height: 20px; }
  .existingStandard .standardsRatingSelector a:hover, .existingStandard .standardsRatingSelector a:focus, .existingStandard .standardsRatingSelector a.selected { color: #FFF; background-color: #FF5707; }
  .existingStandard .standardsRatingSelector input { background-color: #4AA394; font-weight: bold; color: #FFF; }
  .existingStandard .standardsRatingSelector input:hover, .existingStandard .standardsRatingSelector input:focus { background-color: #FF5707; cursor: pointer; }
  .existingStandard .youRated { text-align: center; font-style: italic; color: #AAA; }
</style>

<div id="existingStandards"></div>

<div id="template_existingStandards" style="display:none;">
  <div id="template_existingStandard">
    <div {tab} class="existingStandard">
      <p>{align}:</p>
      <p><a href="{link}" target="_blank">{code}</a></p>
      {ratingSelector}
      <div class="existingStandardDetail isleBox">
        <h3 class="isleBox_H2">{code}</h3>
        <p>{text}</p>
        <p>Current Overall Rating: <b>{current}</b></p>
      </div>
    </div>
  </div>
  <div id="template_ratingSystem">
    <div class="standardsRatingSelector" standardID="{id}">
      <a href="javascript:rate({id}, 0)" ratingLinkID="{id}-0" rating="0">Weak</a>
      <a href="javascript:rate({id}, 1)" ratingLinkID="{id}-1" rating="1">Limited</a>
      <a href="javascript:rate({id}, 2)" ratingLinkID="{id}-2" rating="2">Strong</a>
      <a href="javascript:rate({id}, 3)" ratingLinkID="{id}-3" rating="3">Superior</a>
      <input type="button" class="save" value="Rate" onclick="saveRating({id})" />
    </div>
  </div>
</div>