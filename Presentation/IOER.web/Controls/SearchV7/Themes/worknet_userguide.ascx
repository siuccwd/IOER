<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="worknet_userguide.ascx.cs" Inherits="IOER.Controls.SearchV7.Themes.worknet_userguide" %>

<div id="config" runat="server" visible="false">
	<asp:Literal ID="searchTitle" runat="server"></asp:Literal>
	<asp:Literal ID="themeColorMain" runat="server">#B74900</asp:Literal>
	<asp:Literal ID="themeColorSelected" runat="server">#4D4D4D</asp:Literal>
	<asp:Literal ID="themeColorHeader" runat="server">#4D4D4D</asp:Literal>
	<asp:Literal ID="sortField" runat="server">_score</asp:Literal>
	<asp:Literal ID="sortOrder" runat="server">desc</asp:Literal>
	<asp:Literal ID="resultTagSchemas" runat="server">educationalRole,learningResourceType,mediaType,k12Subject</asp:Literal>
	<asp:Literal ID="siteID" runat="server">3</asp:Literal>
	<asp:Literal ID="startAdvanced" runat="server">0</asp:Literal>
	<asp:Literal ID="hasStandards" runat="server">0</asp:Literal>
	<asp:Literal ID="useResourceUrl" runat="server">1</asp:Literal>
	<asp:Literal ID="doAutoSearch" runat="server">1</asp:Literal>
	<asp:Literal ID="doPreloadNewestSearch" runat="server">0</asp:Literal>
	<asp:Literal ID="showLibColInputs" runat="server">0</asp:Literal>
	<asp:Literal ID="fieldSchemas" runat="server">workNetArea,guidanceScenario,educationalRole,resources</asp:Literal>
	<asp:Literal ID="advancedFieldSchemas" runat="server"></asp:Literal>
</div>

<script type="text/javascript">
	//Correlate IDs with images
	var userGuideImages = <%=JSONImageData %>;

	//Inject images on result render
	$(document).ready(function () {
		$(window).on("resultsRendered", function () {
			injectResultExtras();
		});
		loadIconLegend();
	});

	function injectResultExtras() {
		//For each result...
		for(i in currentResults.hits.hits){
			var hit = currentResults.hits.hits[i]._source;
			var box = $("#searchResults .result[data-resourceID=" + hit.ResourceId + "] .modBox_middle");
			//Check each field...
			for(j in hit.Fields){
				//If it's the end user field....
				if(hit.Fields[j].Schema == "educationalRole"){
					var endUser = hit.Fields[j];
					//Then for each ID in that field...
					for(k in endUser.Ids){
						//Check each user guide image...
						for(l in userGuideImages){
							//If the IDs match...
							if(userGuideImages[l].Id == endUser.Ids[k]){
								//Add the image
								box.append("<img src=\"/Controls/SearchV6/Themes/Images/wn_userguide_" + userGuideImages[l].File + "_50x50.png\" title=\"" + userGuideImages[l].Title + "\" />");
							}
						}
					}
					//Break the loop after finding the end user field
					break;
				}
			}
		}
	}
  
	function loadIconLegend() {
		//get template and header
		var template = $("#template_iconLegendItem").html();
		var header = $("#themeHeaderContent");
		//Add show/hide thing
		header.append("<input type=\"button\" id=\"btnShowHideLegend\" onclick=\"showHideLegend(this)\", value=\"+\" /> <span id=\"legendTitle\">Icon Information</span>");
		//Add header items
		header.append("<div id=\"legendItems\"></div>");
		var box = header.find("#legendItems");
		for(i in userGuideImages){
			box.append(template
        .replace(/{iconImage}/g, "<img src=\"/Controls/SearchV6/Themes/Images/wn_userguide_" + userGuideImages[i].File + "_50x50.png\" title=\"" + userGuideImages[i].Title + "\" />")
        .replace(/{iconText}/g, userGuideImages[i].Title)
      );
		}
	}

	function showHideLegend(button){
		var box = $("#legendItems");
		box.toggleClass("visible");
		if(box.hasClass("visible")){
			box.fadeIn();
			$(button).attr("value", "-");
		}
		else {
			box.fadeOut();
			$(button).attr("value", "+");
		}
		triggerResize(250);
	}
</script>
<style type="text/css">
  /* Icons */
  .modBox_middle img { margin: 2px; }
  .links { display: none; }
  @media (max-width: 500px) {
    .modBox_middle img { width: 25px; }
  }

  /* Legend */
  #themeHeaderContent { padding: 10px; background-color: #F5F5F5; border-radius: 10px; margin-bottom: 10px; }
  #legendItems { font-size: 0; margin: 5px 0; display: none; }
  .iconLegend { display: inline-block; vertical-align: top; width: 25%; padding: 2px; font-size: 16px; }
  .iconLegend img, .iconLegend span { display: inline-block; vertical-align: middle; }
  .iconLegend span { width: calc(100% - 50px); padding: 0 5px; }
  #legendTitle { font-weight: bold; font-size: 20px; line-height: 30px; }
  #btnShowHideLegend { border-radius: 5px; border: 1px solid #CCC; color: #FFF; width: 30px; height: 30px; line-height: 25px; transition: background 0.2s; }
  #btnShowHideLegend:hover, #btnShowHideLegend:focus { background-color: #FF6A00; }
  @media (max-width: 775px) {
    .iconLegend { width: 33.3%; }
  }
  @media (max-width: 600px) {
    .iconLegend { width: 50%; }
  }
  @media (max-width: 400px) {
    .iconLegend { width: 100%; }
  }
</style>

<div id="template_iconLegendItem" style="display:none;">
  <div class="iconLegend">
    {iconImage}<span> {iconText}</span>
  </div>
</div>

<asp:Literal ID="ltlTagList" runat="server" Visible="false">youth,veteran,reentryperson,workforcepartner,laidoffworker,employer,personwithdisability,generalpublic</asp:Literal>