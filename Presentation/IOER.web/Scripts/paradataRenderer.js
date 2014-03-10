var desiredVars = {
    "comments": { name: "commentsCount", img: "icon_comments_bg.png", tip: "Comments", showType: "normal" },
    "likes": { name: "likeCount", img: "icon_likes_bg.png", tip: "Likes", showType: "normal" },
    "dislikes": { name: "dislikeCount", img: "icon_dislikes_bg.png", tip: "Dislikes", showType: "normal" },
    "evaluationScore": { name: "evaluationScoreTotal", img: "icon_ratings_bg.png", tip: "Evaluation Score", showType: "percent" },
    "standards": { name: "standardIDs", img: "icon_standards_bg.png", tip: "Applied Standards", showType: "count" },
    "views": { name: "resourceViews", img: "icon_click-throughs_bg.png", tip: "Click-Throughs", showType: "normal" },
    "favorites": { name: "favorites", img: "icon_library_bg.png", tip: "Favorites", showType: "normal" }
};
var paradataTemplate = "<li><img src=\"{img}\" alt=\"{tip}\" title=\"{tip}\" /><span>{data}</span></li>";
var dataSource;
function getParadataDisplay(data) {
    var display = "";
    for (i in desiredVars) {
        display += standardHandler(data, desiredVars[i]);
    }
    return "<ul class=\"paradata\">" + display + "</ul>";
}
function standardHandler(data, item) {
    var datum = data[item.name];
    var display = null;
    if (valid(datum)) {
        if (item.showType == "count" && datum.length > 0) {
            display = datum.length;
        }
        else if (item.showType == "reverse" && datum < 0) {
            display = datum * -1;
        }
        else if (item.showType == "normal" && datum > 0) {
            display = datum;
        }
        else if (item.showType == "percent" && datum > 0) {
            display = Math.round(datum * 100) + "%";
        }
        if (display != null) {
            return paradataTemplate
          .replace("{img}", "/images/icons/" + item.img)
          .replace(/{tip}/g, item.tip)
          .replace("{data}", display);
        }
        else {
            return "";
        }
    }
    else {
        return "";
    }
}
function valid(item) {
    return (typeof (item) != "undefined");
}
function getParadata(versionID, jTarget) {
    $.ajax({
        url: "/Services/ElasticSearchService.asmx/GetByVersionID", //The elastic search location
        async: true,
        success: function (msg) { renderData(msg.d, jTarget) },
        type: "POST",
        data: '{ "versionID" : "' + versionID + '" }',
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        dataType: "json"
    });
}
function renderData(dataRaw, jTarget) {
    var data = jQuery.parseJSON(dataRaw);
    var source = data.hits.hits[0]._source;
    dataSource = source;
    jTarget.html(getParadataDisplay(source));
}