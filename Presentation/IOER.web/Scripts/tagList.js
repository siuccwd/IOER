$(document).ready(function () {

    //System to add tags
    $(".tagEntry").keypress(function (event) {
        var code = (event.keyCode ? event.keyCode : event.which);
        var targetID = this.id.split("_")[1];
        if (code == 13 | code == 9 | code == 44) {
            var value = this.value.replace(/,/g, "").replace(/<script>/g, "").replace(/<\/script>/g, "").replace(/</g, "").replace(/>/g, "");
            addTag(this, targetID, value);
        }
    });

    $(".tagEntry").blur(function () {
        addTag(this, this.id.split("_")[1], this.value);
    });

});

//An ID to assign to tags for later reference
var tagCount = 0;

function addTag(box, destination, value) {
    var minLength = 3;
    var maxItems = 12;
    if (value.length < minLength) {
        if (value.length > 0) {
            alert("Tags must be at least " + minLength + " characters.");
        }
    }
    else if ($("#tags_" + destination + " li").length >= maxItems) {
        $("#tagEntry_" + destination).blur();
        alert("There can be no more than " + maxItems + " tags in this field.");
    }
    else if (value.indexOf("separated by pressing Enter") > -1) { } //Do nothing
    else {
        $("#tags_" + destination).append("<li id=\"tag_" + tagCount + "\" class=\"tag\">" + value + " <a href=\"javascript:removeTag(" + tagCount + ")\">X</a></li>");
        tagCount++;
        box.value = "";

        //Custom handlers
        if (typeof customTagListClickHandler == 'function') {
            customTagListClickHandler();
        }

        //Remove comma
        setTimeout(function () {
            $(box).val($(box).val().replace(/,/g, ""));
        }, 50);
    }
}

//Remove a tag
function removeTag(target) {
    $("#tag_" + target).remove();
    $("#tag_" + target).remove();
}
