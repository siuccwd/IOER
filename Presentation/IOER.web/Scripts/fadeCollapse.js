if (!heightThreshhold) {
    var heightThreshhold = 80;
}
$(document).ready(function () {
    //show/hide text blocks that are too long
    initExpandCollapse();
});

function expandAll() {
    $(".expandCollapseBox .showHideLink").each(function () {
        $(this).parent().animate({ "height": $(this).parent()[0].scrollHeight + $(this).outerHeight() + "px" }, 250);
        $(this).html("Show Less &lt;&lt;");
    });
}
function hideAll() {
    $(".expandCollapseBox .showHideLink").each(function () {
        $(this).parent().animate({ "height": heightThreshhold + "px" }, 250);
        $(this).html("Show More &gt;&gt;");
    });
}

function initExpandCollapse() {
    var collapseID = 0;
    $(".expandCollapseBox").each(function () {
        if ($(this).height() > heightThreshhold) {
            $(this).append("<a class=\"showHideLink\" href=\"javascript:showHideClick('showHide_" + collapseID + "')\" id=\"showHide_" + collapseID + "\">Show More &gt;&gt;</a>");
            $(this).css("height", heightThreshhold + "px").css("overflow", "hidden");
            collapseID++;
        }
    });
}

function showHideClick(target) {
    var jTarget = $("#" + target);
    if (jTarget.text().indexOf("More") > -1) {
        jTarget.parent().animate({ "height": jTarget.parent()[0].scrollHeight + jTarget.outerHeight() + "px" }, 250);
        jTarget.html("Show Less &lt;&lt;");
    }
    else {
        jTarget.parent().animate({ "height": heightThreshhold + "px" }, 250);
        jTarget.html("Show More &gt;&gt;");
    }
}