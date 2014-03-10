function getParam(name) {
    name = name.toLowerCase().replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
    var regexS = "[\\?&]" + name + "=([^&#]*)";
    var regex = new RegExp(regexS);
    var results = regex.exec(window.location.href.toLowerCase());

    return (results ? decodeURIComponent(results[1].replace(/\+/g, " ")) : "");
}

function resizeIframePipe(e, pipeData) {
    console.log("content changed");
    var url = getParam("helperUrl");
    var view = (pipeData && pipeData.view) ? pipeData.view : getParam("view");
    var urlParams = url + "?height=" + $("#standardsBrowser").height() + (view ? "&view=" + view : "") + "&cacheb=" + Math.random();

    $("#helpFrame").attr("src", urlParams);
}

$(document).ready(function () {
    if (isWidget) {
        $("body").append($("<iframe></iframe>").css("display", "none").attr("id", "helpFrame").attr("src", ""));
        $("body").on("afterContentChange", resizeIframePipe);
    }
});