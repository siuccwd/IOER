var frameSizes = [];
var messageActions = [
  { action: "resize", method: function (event, data) { resizeFrame(event, data); } }
];
//Receive postMessages
jQuery(document).ready(function () {
  jQuery(window).on("message", function (rawEvent) {
    try {
      var data;
      if (typeof (rawEvent.originalEvent) != "object") { return; }
      var event = rawEvent.originalEvent;
      var rawData = event.data;
      if (typeof (rawData) == "string") { data = jQuery.parseJSON(rawData); }
      else if (typeof (rawData) == "object") { data = rawData; }
      else { return; }
      for (i in messageActions) {
        if (messageActions[i].action == data.action) {
          messageActions[i].method(event, data);
        }
      }
    }
    catch (e) { return; }
  });
});

//Handle Resizing
function resizeFrame(event, data) {
  var frame = null;
  jQuery("iframe").each(function () {
    if (this.contentWindow == event.source) {
      frame = jQuery(this);
    }
  });
  if (frame == null) { return; }

  var resizeID = frame.attr("resizeID");
  //Optimize
  var found = false;
  for (i in frameSizes) {
    if (frameSizes[i].resizeID == resizeID) {
      found = true;
      break;
    }
  }
  if (!found) {
    frame.attr("resizeID", frameSizes.length);
    frameSizes.push({ resizeID: frameSizes.length.toString(), widths: [], heights: [] });
  }

  //Execute
  for (i in frameSizes) {
    if (frameSizes[i].resizeID == resizeID) {
      if (preventSpaz(frameSizes[i].widths, data.width)) {
        frame.css("width", data.width);
      }
      if (preventSpaz(frameSizes[i].heights, data.height)) {
        frame.css("height", data.height);
      }
      break;
    }
  }
}
function preventSpaz(list, size) {
  list.push(size);
  if (list.length > 4) {
    list.shift();
    if (list[0] == list[2] && list[1] == list[3]) { return false; }
  }
  return true;
}

//