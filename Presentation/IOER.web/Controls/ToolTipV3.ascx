<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ToolTipV3.ascx.cs" Inherits="IOER.Controls.ToolTipV3" %>

<script type="text/javascript">
	var toolTipIDs = 0;

	$(document).ready(function () {
		setupToolTips();
	});
	//

	function setupToolTips() {
		//Get items that haven't been initialized yet
		var items = $(".toolTip").not(".hasToolTip");
		//For each one...
		items.each(function () {
			var item = $(this);
			item.addClass("hasToolTip");
			var itemID = toolTipIDs;
			toolTipIDs++;

			//Get the data
			var tipData = item.attr("title").split("|");
			item.removeAttr("title");
			var title = tipData[0].trim();
			var content = "";
			for(var i in tipData){
				if(i == 0){ continue; } //skip first
				content += "<div>" + tipData[i].trim() + "</div>";
			}

			var useWholeElement = !item.hasClass("toolTipBubbleOnly")
			var bubblePosition = item.hasClass("toolTipBubbleAfter") ? "after" : item.hasClass("toolTipBubbleBefore") ? "before" : "none";
			var bubbleBox = $("#template_tooltip_bubble").html();
			var hoverItem = item;
			switch(bubblePosition){
				case "after":
					hoverItem = $(bubbleBox).appendTo(item);
					break;
				case "before":
					hoverItem = $(bubbleBox).prependTo(item);
					break;
				default:
					hoverItem = item;
					break;
			}
			if(useWholeElement){ //Override
				hoverItem = item;
			}

			//Create the tip box
			var boxTemplate = $("#template_tooltip").html();
			$("body").append(boxTemplate
				.replace(/{id}/g, itemID)
				.replace(/{title}/g, title)
				.replace(/{content}/g, content)
			);
			var box = getToolTipBox(itemID);

			//Accessibility
			hoverItem.attr("aria-label", "Tool tip: " + box.text().replace(/\./g, ". "));

			//Set the item to toggle the tip box
			hoverItem.attr("tabindex", "0");
			hoverItem.addClass("toolTipInitialized");
			hoverItem.on("mouseover focus", function () {
				showToolTip(hoverItem, itemID);
			});
			hoverItem.add(box).on("mouseout blur", function() {
				setTimeout(
					function() {
						if(!box.is(":hover") && !hoverItem.is(":hover")){
							hideToolTip(itemID);
						}
				}, 10);
			});

		    //Start out hidden
			hideToolTip(itemID);
		});
	}
	//

	function showToolTip(item, id){
		var windowItem = $(window);
		var winHeight = windowItem.height() - 5;
		var winWidth = windowItem.width() - 5;
		var box = getToolTipBox(id);

		//Get normal position
		var realPosition = item.offset();
		var realTop = realPosition.top - ( document.documentElement["scrollTop"] || document.body["scrollTop"] ) + (item.height() * 0.5);
		var realLeft = realPosition.left - ( document.documentElement["scrollLeft"] || document.body["scrollLeft"] ) + (item.width() * 0.5);
		var boxHeight = box.height();
		var boxWidth = box.width();
		var boxBottom = boxHeight + realTop;
		var boxRight = boxWidth + realLeft;
		
		//Prevent tooltip from being off-screen
		if(boxBottom > winHeight){
			if(boxHeight > winHeight) {
				realTop = 5;
			}
			else {
				realTop = realTop - (boxBottom - winHeight);
			}
		}
		if(boxRight > winWidth){
			if(boxWidth > winWidth) {
				realLeft = 5;
			}
			else {
				realLeft = realLeft - (boxRight - winWidth);
			}
		}

		//Apply positioning
		box.css("top", realTop).css("left", realLeft).stop().fadeIn(250);
	}
	//

	function hideToolTip(id){
		getToolTipBox(id).stop().fadeOut(250);
	}
	//

	function getToolTipBox(id){
		return $(".toolTipBox[data-toolTipID=" + id + "]");
	}
	//
</script>

<style type="text/css">
	.toolTipInitialized { cursor: help; }
	.toolTipBox { position: fixed; background-color: #F5F5F5; box-shadow: 0 0 10px -2px rgba(0,0,0,0.5); border-radius: 5px; overflow: hidden; min-width: 250px; max-height: calc(100vh - 10px); max-width: calc(100vw * 0.5); z-index: 1000000; }
	.toolTipTitle { background-color: #4AA394; color: #FFF; font-weight: bold; padding: 5px; padding-right: 30px; }
	.toolTipContent { padding: 5px; }
	.toolTipContent > div { margin-bottom: 5px; }
	.toolTipClose { position: absolute; top: 5px; right: 5px; width: 20px; height: 20px; display: block; border-radius: 5px; background-color: #F00; color: #FFF; font-weight: bold; opacity: 0.8; border: none; transition: opacity 0.2s; }
	.toolTipClose:hover, .toolTipClose:focus { opacity: 1; cursor: pointer; }
	.toolTipIcon { display: inline-block; width: 24px; min-height: 20px; height: 100%; background-image: url('/images/icons/infoBubble.gif'); background-repeat: no-repeat; background-position: center center; vertical-align: text-top; }
	@media ( max-width: 500px ) {
		.toolTipBox { max-width: calc(100vw - 10px); }
	}
</style>

<script type="text/template" id="template_tooltip" style="display:none;">
	<div class="toolTipBox" data-tooltipID="{id}">
		<div class="toolTipTitle">{title}</div>
		<div class="toolTipContent">{content}</div>
		<input type="button" class="toolTipClose" value="X" onclick="hideToolTip({id});" />
	</div>
</script>
<script type="text/template" id="template_tooltip_bubble" style="display:none;">
	<div class="toolTipIcon"></div>
</script>