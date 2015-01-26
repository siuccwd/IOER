function initWidgetSearch() {
    var options = { hasButton: true };
    if (typeof(ioerSearchOptions) != "undefined") {
        options.hasButton = (typeof(ioerSearchOptions.hasButton) != "undefined" ? ioerSearchOptions.hasButton : true )
    }
    $("div#ioer_search").replaceWith($("<input />").attr("type", "text").attr("id", "ioerSearchBar").attr("placeholder", "Search..."));
    if (options.hasButton) {
        $("#ioerSearchBar").after($("<input />").attr("type", "button").attr("id", "ioerSearchButton").attr("value", "Search")); 
    }
    $("#ioerSearchBar").on("keyup", function (event) {
        var bar = $("#ioerSearchBar");
        event.stopPropagation();
        if ((event.which == 13 || event.keyCode == 13) && bar.val().length > 0) {
          doIOERSearch();
        }
        return false;
    });
    $("#ioerSearchButton").on("click", function () {
        doIOERSearch();
    });
}
function doIOERSearch() {
        window.open("http://ioer.ilsharedlearning.org/Search.aspx?q=" + encodeURIComponent($("#ioerSearchBar").val()));
    
}