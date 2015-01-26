var rpLink;
$(document).ready(function () {
  $(window).on("message", function (message) {
    rpHandlePostMessage(message);
  });
  rpLink = $("<iframe></iframe>");
  $("body").append(rpLink);
  rpLink.css("display", "none");
  rpLink.attr("src", "http://localhost:2012/Widgets/PreviewerV1");
});

function rpHandlePostMessage(message) {
  if (!message.originalEvent) { return; }
  message = message.originalEvent.data;
  if (message.action == "appendToBody") {
    $("body").append(message.data);
  }
  else {
    console.log("message here:");
    console.log(message);
    console.log("rp here:");
    console.log(rp);
    console.log("doing action");
    rp[message.action](message.data);
  }
}

function rpDo(message) {
  rpLink[0].contentWindow.postMessage(message, "*");
}