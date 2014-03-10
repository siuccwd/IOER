function initStandardsBrowser() {
    var options = { mode: "browse", preselectedBody: "", preselectedGrade: "", helperUrl: "" };
    if (typeof(standardsBrowserOptions) != "undefined") {
        options.mode = (typeof(standardsBrowserOptions.mode) != "undefined" ? standardsBrowserOptions.mode.toLowerCase() : "browse");
        options.preselectedBody = (typeof(standardsBrowserOptions.preselectedBody) != "undefined" ? standardsBrowserOptions.preselectedBody.toLowerCase() : "");
        options.preselectedGrade = (typeof(standardsBrowserOptions.preselectedGrade) != "undefined" ? standardsBrowserOptions.preselectedGrade.toLowerCase() : "");
        options.helperUrl = (typeof(standardsBrowserOptions.helperUrl) != "undefined" ? standardsBrowserOptions.helperUrl : "");
    }
    $("#ioer_standardsbrowser").replaceWith("<iframe id=\"ioerStandardsBrowser\" src=\"http://ioer.ilsharedlearning.org/testing/browserframe.aspx?standardsbrowsermode=" + options.mode + "&preselectedbody=" + options.preselectedBody + "&preselectedgrade=" + options.preselectedGrade + "&helperUrl=" + options.helperUrl + "\" />");
    var browserBox = $("#ioerStandardsBrowser");
    browserBox.css({ "border": "none", "min-width": "300px" });
    $(window).on("resize", function () {
        browserBox.height(browserBox.parent().height());
        browserBox.width(browserBox.parent().width());
    });
    setTimeout(function () { $(window).trigger("resize"); }, 1000);
    //Force iframe to resize, to trigger internal sizing
    setTimeout(function () { browserBox.height(browserBox.height() + 1) }, 1500);
    setTimeout(function () { browserBox.height(browserBox.height() - 1) }, 1550);
}