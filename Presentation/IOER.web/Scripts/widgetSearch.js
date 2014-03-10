//Using JSON to create namespacing prevents conflicts with other items in the host page--at the expense of a little added complexity to this script.
var isleSearch = {
    //variables
    containerName: "#isleSearchWidget",
    resultsToDisplay: 5,

    //templates
    template: {
        searchBox: { tag: "input", attributes: { type: "text", id: "isle_searchBox"} },
        searchResults: { tag: "div", attributes: { id: "isle_searchResults"} },
        searchResult: { tag: "div", attributes: { class: "isle_searchResult" }, children: [{ tag: "a", attributes: { target: "_blank", href: ""} }, { tag: "div", attributes: { class: "isle_resultText"}}] }
    }
    ,

    //functions
    //Startup
    getJQuery: function () {
        isleSearch.jScript = document.createElement("script");
        isleSearch.jScript.type = "text/javascript";
        isleSearch.jScript.src = "http://ajax.googleapis.com/ajax/libs/jquery/1.9.1/jquery.min.js";
        document.getElementsByTagName("head")[0].appendChild(isleSearch.jScript);

        checker = setInterval(function () { isleSearch.checkJQuery() }, 100);
    }
    ,
    //Check for jQuery being loaded
    checkJQuery: function () {
        if (isleSearch.jScript) {
            isleSearch.initJQuery();
            clearInterval(checker);
        }
    }
    ,
    //Inject functionality
    initJQuery: function () {
        $(document).ready(function () {
            isleSearch.loadHTMLFromJSON($(isleSearch.containerName), isleSearch.template.searchBox); //Search Textbox
            isleSearch.loadHTMLFromJSON($(isleSearch.containerName), isleSearch.template.searchResults); //Results container
            if ($(isleSearch.containerName).attr("resultstodisplay")) {
                isleSearch.resultsToDisplay = parseInt($(isleSearch.containerName).attr("resultstodisplay"));
                if (isleSearch.resultsToDisplay < 1) { isleSearch.resultsToDisplay = 1; }
                if (isleSearch.resultsToDisplay > 25) { isleSearch.resultsToDisplay = 25; }
            }
            isleSearch.initSearch();
        });
    }
    ,
    //HTML conversion
    loadHTMLFromJSON: function (jHolder, jsonData) {
        var element = document.createElement(jsonData.tag);
        var jElement = $(element);
        for (var i in jsonData.attributes) { //Add attributes to the HTML element
            jElement.attr(i, jsonData.attributes[i]);
        }
        if (jsonData.children) {
            for (var j in jsonData.children) { //Recurse through to construct an HTML element with children
                isleSearch.loadHTMLFromJSON($(element), jsonData.children[j]);
            }
        }
        jHolder.append(element);
        return element;
    }
    ,
    //Search functionality
    initSearch: function () {
        var countdownTime = 800;
        var counter;
        $("#isle_searchBox").bind("keyup change", function () {
            clearTimeout(counter);
            counter = setTimeout(function () { isleSearch.doSearch() }, countdownTime);
        });
    }
    ,
    //AJAX search
    doSearch: function () {
        var searchText = $("#isle_searchBox").val();
        try {
            console.log("starting");
            $.ajax({
                url: "/Admin/widgetTestingReceiver.aspx", //Workaround
                async: true,
                success: function (msg) { isleSearch.updateResults(msg); },
                type: "GET",
                data: '{ "searchText" : "' + searchText + '", "pageSize" : "' + isleSearch.resultsToDisplay + '" }',
                contentType: "text/html",
                dataType: "jsonp"
            });

        }
        catch (e) {
            console.log("fail");
            console.log(e);
        }
    }
    ,
    updateResults: function (data) {
        var resultsBox = $("#isle_searchResults");
        resultsBox.html("<p>Sorry, no results found.</p>");
        var info = data;
        var jsonData = isleSearch.template.searchResult;
        if (info.hits.hits.length > 0) {
            resultsBox.html("");
            for (var i in info.hits.hits) {
                var current = info.hits.hits[i];
                var element = $(isleSearch.loadHTMLFromJSON(resultsBox, isleSearch.template.searchResult));
                var titleText = current._source.title;
                if (titleText.length > 85) {
                    titleText = titleText.substring(0, 75) + "...";
                }
                var descriptionText = current._source.description;
                if (descriptionText.length > 110) {
                    descriptionText = descriptionText.substring(0, 100) + "...";
                }
                element.find("a").attr("href", "/ResourceDetail.aspx?vid=" + current._source.versionID).html(titleText);
                element.find("div.isle_resultText").html(descriptionText);
            }
        }

    }
}

//Get jQuery
isleSearch.getJQuery();
