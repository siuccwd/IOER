if (!flyoutWidth) {
    var flyoutWidth = 200;
}

function slideInOut(targetID, targetWidth) {
    //First we determine whether or not the second value was supplied--Javascript doesn't support overloaded methods.
    if (!targetWidth) {
        targetWidth = 200;  // flyoutWidth;
    }

    //Next we match the container to the trigger
    var currentVictim = $("#" + targetID)
    currentVictim.find(".flyoutContent").css("width", targetWidth + "px"); //Keeps the text from scrunching up for custom-width boxes.

    //Shrink ALL of the flyout containers, and then expand the targeted one. 
    //This would ensure that only one container is open at a time. The code would look like this:
    $(".flyoutContainer").each(function () {
        $(this).animate(
			{ width: "0" }
			,
			5
		);
    });
    currentVictim.animate(
		{ width: targetWidth + "px" }
		,
		250
	);

}


function slideOut(targetID, targetWidth) {
    //First we determine whether or not the second value was supplied--Javascript doesn't support overloaded methods.
    if (!targetWidth) {
        targetWidth = 200;  // flyoutWidth;
    }

    //Next we match the container to the trigger
    var currentVictim = $("#" + targetID)
    currentVictim.find(".flyoutContent").css("width", targetWidth + "px"); //Keeps the text from scrunching up for custom-width boxes.

    //Approach One
    //Now, because there is no shorthand toggle function for horizontal sliding like there is for vertical sliding, we must determine the current state of the element.
    if(currentVictim.width() == 0){ //If it's compacted
    currentVictim.animate( //the animate() function is a bit fancy. Basically you supply a JSON object of CSS name/value pairs to be applied to the matched element, and then the duration in milliseconds the animation should last. Note that without plugins, animate() does not support animating colors.
    { width : targetWidth + "px" } //All we need to do here is expand the container to the desired width
    ,
    500 //It'll take half a second
    );
    }
    else { //If it's already expanded
    currentVictim.animate(
    { width : "0" } //Shrink the width to zero.
    ,
    500
    );
    }
   
}
