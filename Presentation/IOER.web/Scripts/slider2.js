$(document).ready(function () {
    createSliders();
});
function createSliders() {
	//var script = document.createElement("script");
	//script.type = "text/javascript";
	//script.language = "javascript";
	//$(script).html( $("#sliderData").text() );
    //$("head").append(script);

    //workaround for IE7/8
	$("head").append("<script type=\"text/javascript\" language=\"javascript\">" + $("#sliderData").text() + "</script>");
	
	for(i in sliders){
		loadSlider(sliders[i], $("#" + sliders[i].sliderID));
	}
}
function loadSlider(sliderData, slider) {
	//Setup variables
	var outerWidth;
	var displayWidth;
	var displayHeight;
	var perItemWidth;
	var displayCount = sliderData.itemsToDisplay;
	var itemCount = sliderData.items.length;
	var sliderScreen;
	var sliderContent;

	//Setup infrastructure
	slider.html("");
	slider.append("<a class=\"sliderArrow slideLeft\" href=\"javascript:slide('" + sliderData.sliderID + "',-1," + sliderData.shiftCount + "," + sliderData.animationTime + ")\"></a>");
	slider.append("<div class=\"sliderScreen\"><ul class=\"sliderContent\" sliderID=\"" + sliderData.sliderID + "\"></ul></div>");
	slider.append("<a class=\"sliderArrow slideRight\" href=\"javascript:slide('" + sliderData.sliderID + "',1," + sliderData.shiftCount + "," + sliderData.animationTime + ")\"></a>");
	if(sliderData.useTextArrows){
		slider.find(".slideLeft").html("&larr;");
		slider.find(".slideRight").html("&rarr;");
	}
	sliderScreen = slider.find(".sliderScreen");
	sliderContent = slider.find(".sliderContent");
	
	//Get data
	outerWidth = slider.width();
	sliderScreen.width(outerWidth - slider.find(".sliderArrow").width() * 2);
	displayWidth = sliderScreen.width();
	displayHeight = slider.height();
	perItemWidth = displayWidth / displayCount;
	sliderContent.width(itemCount * perItemWidth * 3).css("margin-left", -(itemCount * perItemWidth) + "px");
	
	//Construct interface
	for(var i = 0; i < 3; i++){
	    for (j in sliderData.items) {
            var image = "";
            if (typeof (sliderData.items[j].img) == "object") {
                image = "<div class=\"img isleBox\"><h2 class=\"isleBox_H2\">" + sliderData.items[j].img.header + "</h2><span>" + sliderData.items[j].img.text + "</span></div>";
            }
            else {
                image = "<div class=\"img\" style=\"background: url('" + sliderData.items[j].img + "') no-repeat center center\"></div>";
            }
			sliderContent.append(
			"<li><a href=\"" + sliderData.items[j].link + "\">" +
			image +
			"<div class=\"description\">" + sliderData.items[j].text +
			"</div></a></li>");
        }
	}
	slider.find("a.sliderArrow").css("line-height", displayHeight + "px");
	var count = 0;
	slider.find(".sliderContent li").each(function() {
		var item = $(this);
		item.css({ "height": displayHeight + "px", "width": perItemWidth + "px"});
		count++;
	});
	
	if(sliderData.autoAdvance > 0){
		slider.attr("hovering", "false");
		slider.hover(function () { slider.attr("hovering", "true") }, function() { slider.attr("hovering", "false") });
		setInterval(function() { autoAdvance(sliderData.sliderID, sliderData.shiftCount, sliderData.animationTime)}, sliderData.autoAdvance);
	}
    //Initialize the thing--fixes odd formatting bug
	slide(sliderData.sliderID, -1, sliderData.shiftCount, 0);
}
function slide(id, direction, shift, animationTime){
	var content = $(".sliderContent[sliderID=" + id + "]");
	var items = content.find("> li");
	var width = items.first().width();
	//var offset = (items.length / 3) * width;
	var offset = content.parent().width();
	content.animate({"margin-left": (width * direction * shift) - offset + "px"}, animationTime, function() {
		direction > 0 ? shiftRight(id, shift) : shiftLeft(id, shift);
		content.css("margin-left", -offset);
	});
}
function shiftLeft(id, shift){
	for(var i = 0; i < shift; i++){
		var items = $(".sliderContent[sliderID=" + id + "] > li");
		items.first().insertAfter(items.last());
	}
}
function shiftRight(id, shift){
	for(var i = 0; i < shift; i++){
		var items = $(".sliderContent[sliderID=" + id + "] > li");
		items.last().insertBefore(items.first());
	}
}
function autoAdvance(id, count, time){
	var slider = $("#" + id);
	if(slider.attr("hovering") == "false"){
		slide(id, 1, count, time);
	}
}
//Example/reference of what the sliderData and related divs should look like:
/*
<div id="sliderItem"></div>
<div id="anotherSlider"></div>
<div id="sliderData" style="display: none;">
var sliders = [
	{
		sliderID: "sliderItem",
		itemsToDisplay: 3,
		shiftCount: 3,
		animationTime: 100,
		useTextArrows: true,
		autoAdvance: 0,
		items: [
			{ link: "#", img: "", text: "Item 1" },
			{ link: "#", img: "", text: "Item 2" },
			{ link: "#", img: "", text: "Item 3" },
		]
	},
	{
		sliderID: "anotherSlider",
		itemsToDisplay: 3,
		shiftCount: 3,
		animationTime: 1000,
		useTextArrows: true,
		autoAdvance: 2000,
		items: [
			{ link: "#", img: "", text: "Item A" },
			{ link: "#", img: "", text: "Item B" },
			{ link: "#", img: "", text: "Item C" },
		]
	},
]
</div>
*/
