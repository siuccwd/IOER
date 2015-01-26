window.forceResize = {};
$(document).ready(function () {
  resizeParent();
  //Handle window resizing
  $(window).on("resize", function () {
    resizeParent();
  });
  //Handle image loading
  $(document).on("load", "img", function () {
    resizeParent();
  });
  //Handle content changes
  $("body").on("change", function () {
    resizeParent();
  });
  //Brute force page load resizing
  window.forceResize = setInterval(resizeParent, 1000);
  setTimeout(function () { clearInterval(window.forceResize); }, 5500);
});

function resizeParent() {
  if (window == window.top) { return; }
  var height = $("body").outerHeight() + 20;
  var message = { action: "resize", width: "100%", height: height + "px" };
  window.parent.postMessage(JSON.stringify(message), "*");
}