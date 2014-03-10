//Overridable default width
if (typeof defaultFlyoutWidth != "number") {
    var defaultFlyoutWidth = 300;
}

$(document).ready(function () {
    //Get the navbar width so we know where to position the element
    var navbarWidth = $(".flyoutList").width();

    //Do the actual positioning
    $(".flyoutContainer").css("left", navbarWidth + 10 + "px").css("opacity", "0");

    //Hide anything that's showing


    //Auto-close the flyout on click outside of it 
    $("html").click(function () {
        $(".flyoutContainer").each(function () {
            $(this).animate(
				{ width: "0", opacity: "0", margin: "10px 0", padding: "5px 0" }
				,
				{
				    duration: 500, //Prevents a delay in the animation running (normally jQuery chains animations into a queue)
				    queue: false
				}
			);
        });
    });
    $(".flyoutContainer").click(function (event) {
        event.stopPropagation(); //This prevents a click on the opened panel from "bubbling up" to the HTML level and causing the panel to close.
    });

    //Trigger the flyout
    $(".flyoutTrigger").click(function (event) {
        event.stopPropagation();
        slideInOut(this);
    });

    //Make sure the flyouts start hidden
    $("html").click();
});

//Controls sliding in or out
function slideInOut(element) {
    var trigger = $(element);
    var victim = $("#flyoutContainer_" + element.id.split("_")[1]);
    var targetWidth = defaultFlyoutWidth;

    if (trigger.attr("flyoutWidth")) {
        targetWidth = trigger.attr("flyoutWidth");
    }

    $(".flyoutContainer").each(function () {
        $(this).animate(
            { width: "0", opacity: "0", margin: "10px 0", padding: "5px 0" }
            ,
            {
                duration: 250,
                queue: false
            }
        );
    });
    if (victim.width() == 0) {
        victim.animate(
            { width: targetWidth + "px", opacity: "1", margin: "10px", padding: "5px" }
            ,
            250
        )
    }
}