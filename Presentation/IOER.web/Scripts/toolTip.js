function initToolTips() {
    //$(document).ready(function () {
    //Remember who is clickable
    var clickModes = Array();
    //And who has been clicked
    var clicked = Array();
    //Store things to be loaded
    var toFill = Array();

    //Construct the tooltips
    $(".toolTipLink").each(function () {

        //Configuration options
        var clickMode = false;
        var widthOverride = "0";
        var tempClasses = this.className.toLowerCase().split(" ");
        for (var i = 0; i < tempClasses.length; i++) {
            if (tempClasses[i] == "tooltipclickmode") { clickMode = true; }
            if (tempClasses[i].indexOf("tooltipwidth_") > -1) {
                var tempWidth = tempClasses[i].split("_");
                widthOverride = tempWidth[1];
            }
        }
        clickModes["#" + this.id + "_toolTip"] = clickMode;

        //Configure the tooltip's text
        var count = 0;
        var textLines = this.title.split("|");
        var text = "<h3>" + textLines[0] + "</h3><div class=\"toolTip_closeBox\" id=\"" + this.id + "_toolTipClose" + "\">&nbsp;</div>";
        for (var i = 1; i < textLines.length; i++) {
            if (textLines[i].indexOf("[URL]") > -1) {
                var urlText = textLines[i].substring(5);
                text += "<div class=\"externalContentHolder\" id=\"holder_" + count + "\"></div>";
                toFill[count] = urlText;
                count++;
            } else {
                text += "<p>" + textLines[i] + "</p>";
            }
        }
        this.title = "";

        //Create the actual tooltip element
        var toolTipDiv = document.createElement("div");
        toolTipDiv.id = this.id + "_toolTip";
        toolTipDiv.innerHTML = "<div class=\"toolTip\">" + text + "</div>";
        toolTipDiv.className = "toolTipOuter";
        //document.body.appendChild(toolTipDiv);
        this.appendChild(toolTipDiv);
        if (widthOverride != "0") { $("#" + toolTipDiv.id + " div.toolTip").css("width", widthOverride + "px"); }
        for (var i = 0; i < toFill.length; i++) {
            var temp = "#" + this.id + " .externalContentHolder";
            $.get(toFill[i], function (data) {
                var jData = jQuery(data);
                $(temp).html("<p>" + jData.find("#GlossaryDisplay_txtDescription").html() + "</p>");
            });
        }
    });

    //Show/hide the tooltips
    $(".toolTipLink").hover(
        function () {
            var position = $("#" + this.id).offset();
            var thisToolTip = "#" + this.id + "_toolTip";
            var difference = position.left + ($("#" + this.id).width() * 0.6) + $(thisToolTip).width() - ($(window).width() - 30);
            var differenceHeight = position.top + $(thisToolTip).height() - ($(window).height() - 30) - $(window).scrollTop();
            $("#" + this.id + "_toolTip").css("left", position.left + ($("#" + this.id).width() * 0.6) - (difference > 0 ? difference : 0) + "px").css("top", position.top - $("#" + this.id).height() - (differenceHeight > 0 ? differenceHeight : 0) + "px").fadeIn("fast");
        }
        ,
        function () {
            if (!clicked["#" + this.id + "_toolTip"]) {
                $("#" + this.id + "_toolTip").fadeOut("fast", function () { $("#" + this.id).css("left", "-10000px") });
            }
        }
    );

        $(".toolTipLink").focus(function () {
                var position = $("#" + this.id).offset();
                var thisToolTip = "#" + this.id + "_toolTip";
                var difference = position.left + ($("#" + this.id).width() * 0.6) + $(thisToolTip).width() - ($(window).width() - 30);
                $("#" + this.id + "_toolTip").css("left", position.left + ($("#" + this.id).width() * 0.6) - (difference > 0 ? difference : 0) + "px").css("top", position.top - $("#" + this.id).height() + "px").fadeIn("fast");
            });

        $(".toolTipLink").blur(function () {
            if (!clicked["#" + this.id + "_toolTip"]) {
                $("#" + this.id + "_toolTip").fadeOut("fast", function () { $("#" + this.id).css("left", "-10000px") });
            }
        });

    //Toggle click
    $(".toolTipLink").click(function () {
        if (clickModes["#" + this.id + "_toolTip"]) { clicked["#" + this.id + "_toolTip"] = true; }
    });

    //Enable the close button
    $(".toolTip_closeBox").click(function (event) {
        event.stopPropagation();
        //var target = this.id.replace("_toolTip","");
        //$("#" + target[0] + "_toolTip").fadeOut("fast", function () { $("#" + this.id).css("left", "-10000px") });
        $(this).parent().parent().fadeOut("fast", function () { $("#" + this.id).css("left", "-10000px") });
        //clicked["#" + target[0] + "_toolTip"] = false;
        clicked["#" + this.id ] = false;
    });

    //});

}

$(document).ready(function () {
    initToolTips();
});