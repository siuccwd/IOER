var rp = {
  //Variables
  previewer: null,
  frameContainer: null,
  frame: null,
  commentsBox: null,
  title: null,
  publisher: null,
  created: null,
  resource: null,
  intID: null,

  //Setup
  init: function() {
    previewer = $("#resourcePreviewer");
    frameContainer = $("#resourcePreviewer #rpPreviewFrame");
    frame = $("#resourcePreviewer #rpFrame");
    commentsBox = $("#resourcePreviewer #rpCommentsBox");
    title = $("#resourcePreviewer #rpTitle");
    publisher = $("#resourcePreviewer #rpPublisher");
    created = $("#resourcePreviewer #rpCreated");
    if (window != window.top) {
      window.parent.postMessage({ action: "appendToBody", data: $("#rpMainContainer").html() }, "*");
    }
  },
  //Add Like
  toggleLikeBox: function() {
    this.hitServer("addLike", {});
  },
  //View Comments
  toggleCommentsBox: function () {
    frameContainer.toggleClass("withComments");
    commentsBox.toggleClass("withComments");
    if(frameContainer.hasClass("withComments")){
      this.hitServer("getComments", {});
    }
  },
  //View Libraries
  toggleLibrariesBox: function() {
    this.hitServer("addToLibrary", {});
  },
  //Go to Detail Page
  goToDetailPage: function () {
    window.location.href = "/Resource/" + intID;
  },
  //Open the previewer
  open: function(data) {
    var resource = data;
    frame.attr("src", resource.url);
    title.html(resource.title);
    publisher.html(resource.publisher);
    created.html(resource.created);
    intID = resource.intID;
    previewer.addClass("block");
    $(".at4-share-outer").hide();
    previewer.addClass("visible");
  },
  //Close the previewer
  close: function() {
    frame.attr("src", "");
    previewer.removeClass("visible");
    setTimeout(function () {
      previewer.removeClass("block");
      $(".at4-share-outer").show();
    }, 500);
  },
  renderComments: function(data){
    commentsBox.find("#rpCommentsList").html(data);
  },
  hitServer: function (method, data) {
    data.intID = intID;
    console.log(method);
    console.log(data);
    rpLink[0].contentWindow.postMessage({ type: method, data: data }, "*");
  }
};
//Trigger the process
$(document).ready(function () {
  rp.init();
});