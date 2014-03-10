var widgetList = [
    { div: "ioer_search", code: "standardsbrowser.js", run: "initStandardsBrowser" },
    { div: "ioer_standardsbrowser", code: "ioersearch.js", run: "initWidgetSearch" }
];
$(document).ready(function () {
    for (i in widgetList) {
        doWidgetLoad(widgetList[i]);
    }
});
function doWidgetLoad(item) {
    if ($("#" + item.div).length > 0) { $.getScript("http://ioer.ilsharedlearning.org/Scripts/widgets/" + item.code, function () { window[item.run]() }); }
}
