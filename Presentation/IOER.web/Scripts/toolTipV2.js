//Tooltip V2
$(document).ready(function () {
  $(".toolTipLink").each(function () { //For each tooltip item
    var link = $(this); //Set the link object
    if (link.attr("title") == "") { return; } //Skip this element if its title is empty
    if (typeof (link.attr("href")) == "undefined" || link.attr("href") == "#") {
      link.attr("href", "#").attr("onclick", "return false;"); //Ensure the link has the correct set of properties for accessibility
    }
    var raw = link.attr("title").split("|"); //Break the text into groups
    var html = "<h4 class=\"toolTipHeader\">" + raw[0] + "<a href=\"#\" class=\"toolTipCloseButton\">X</a></h4>"; //Set the first group as the header
    html += "<div class=\"toolTipContent\">";
    for (var i = 1; i < raw.length; i++) { //Set remaining groups as paragraphs
      html += "<div>" + raw[i] + "</div>";
    }
    html += "</div>";
    var toolTip = $("<div></div>"); //Create the actual tip element
    toolTip.append(html).addClass("toolTipDiv"); //Add stuff to the element
    link.attr("title", ""); //Empty out the title attribute
    link.attr("aria-haspopup", "true");
    link.after(toolTip); //Attach the tooltip just after the link
    link.on("mouseover focus", function () { //On hover/focus of the link...
      toolTip.stop(true, true).show().css("opacity", "0").animate({ "opacity": "1" }, 250); //Show the tooltip
      toolTip.css({ "width": "auto", "height": "auto", "left": "0", "top": "0" }); //Reset things for proper measurements later
      var jWindow = $(window); //Grab the current window state

      var pos = link.offset(); //Get the absolute position of the link
      pos.left = pos.left - jWindow.scrollLeft();
      pos.top = pos.top - jWindow.scrollTop();
      pos.right = pos.left + (link.outerWidth() * 0.9); //Determine the right edge of the link, with a slight overlap
      pos.bottom = pos.top + (link.outerHeight() * 0.9);

      toolTip.css("top", pos.top).css("left", pos.right); //Position the tooltip next to the link
      var windowPadding = 10; //Add some space between the edge of the window and the edge of the screen
      var tipPos = toolTip.offset(); //Get the position of the tooltip
      tipPos.right = tipPos.left + toolTip.outerWidth();
      tipPos.bottom = tipPos.top + toolTip.outerHeight();

      var xOverlap = tipPos.right - jWindow.width() + windowPadding; //Determine whether the tooltip is too close to the edge of the window

      if (xOverlap > 0) {  //If the right edge of the tip overlaps the right edge of the window...
        if (jWindow.width() > 400) { //If the window isn't super-narrow...
          //Attempt to squish the tooltip
          var squishedWidth = toolTip.width() - xOverlap;
          var xDiff = 250 + windowPadding - squishedWidth;
          if (squishedWidth > 250 + windowPadding) { //If squishing will not badly distort the tooltip...
            toolTip.css("width", squishedWidth + "px"); //Squish the tooltip to fit
          }
          else { //Otherwise, shift the tooltip's position
            toolTip.css("left", pos.right - xDiff).css("width", "auto").css("width", "-=" + windowPadding + "px");
          }
        }
        else { //If the window is mobile, just set to full width
          toolTip.css("left", windowPadding + "px").css("width", "100%").css("width", "-=" + (windowPadding * 2) + "px");
        }
      }
      else {
        toolTip.css("width", "auto");
      }

      toolTip.css("top", pos.top + "px");
      var content = toolTip.find(".toolTipContent");
      content.css("max-height", "500px");
      var yOverlap = pos.top + content.outerHeight() - jWindow.height() + windowPadding + toolTip.find(".toolTipHeader").outerHeight() + 55; //Determine whether the tooltip is too close to the bottom edge of the window

      if (yOverlap > 0) {
        //Attempt to squish the tooltip
        var squishedHeight = content.height() - yOverlap;
        var yDiff = 300 + windowPadding - squishedHeight;
        if (squishedHeight > 300 + windowPadding) { //If squishing will not badly distort the tooltip...
          content.css("max-height", squishedHeight + "px");
        }
        else {//Otherwise, shift it upwards
          var upScoot = pos.top - yOverlap;
          if (upScoot > windowPadding) { //If it can be scooted up without pushing past the top of the window...
            toolTip.css("top", upScoot + "px"); //Do so
          }
          else { //Otherwise, move it to the top and shrink the height
            toolTip.css("top", windowPadding + "px");
          }
          //Finally, force a squish if needed
          if (jWindow.outerHeight() - ( windowPadding * 2 ) > 500) {
            content.css("max-height", "500px");
          }
          else {
            content.css("max-height", (jWindow.outerHeight() - (windowPadding * 2) - toolTip.find(".toolTipHeader").outerHeight() - 55) + "px");
          }
        }
      }
      else {
        content.css("max-height", "500px");
      }

    });
    link.on("mouseout blur", function () {
      setTimeout(function () {
        if (toolTip.is(":hover") || toolTip.find("a").is(":focus")) { }
        else {
          toolTip.stop(true, true).fadeOut("fast", function () { toolTip.hide(); });
        }
      }, 10);
    });
    toolTip.on("mouseout", function () {
      if (!link.is(":hover") && !toolTip.is(":hover")) {
        toolTip.stop(true, true).fadeOut("fast", function () { toolTip.hide(); });
      }
    });
    toolTip.hide("fast");
    toolTip.find("a.toolTipCloseButton").on("click", function () { //
      toolTip.stop(true, true).fadeOut("fast", function () { toolTip.hide(); });
    })
    toolTip.find("*").on("blur", function () {
      setTimeout(function () {
        if (toolTip.find("*").is(":focus")) { }
        else {
          toolTip.stop(true, true).fadeOut("fast", function () { toolTip.hide(); });
        }
      }, 10);
    });
  });
});

