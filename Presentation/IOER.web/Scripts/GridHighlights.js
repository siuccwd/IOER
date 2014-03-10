// JScript File for STEM Grids
  var $sg = jQuery.noConflict();
  var notApplicableText = "<SPAN class='offScreen'>(not applicable) </SPAN>";
  var removeText = "<SPAN class=offScreen>(not applicable) </SPAN>";
  
  function hideAll(cssClass) {
    hideAllStuff(cssClass, true);
  }  
  
  function hideAllStuff(cssClass, includePathList) {
    cellsWithoutLinks = $sg("tbody tr td."+cssClass+".nolink");
    for(i=0;i<cellsWithoutLinks.length;i++) {
      cellsWithoutLinks[i].innerHTML = notApplicableText + cellsWithoutLinks[i].innerHTML;
    }
    links = $sg("tbody tr td."+cssClass+" a");
    for(i=0;i<links.length;i++) {
      links[i].innerHTML = notApplicableText + links[i].innerHTML;
    }
    $sg("tbody tr td."+cssClass).removeClass(cssClass).addClass("grayed");
    if(includePathList == true) {
      $sg("#pathList li."+cssClass).removeClass(cssClass).addClass("grayed");
    }
  }
  
  function showSome(cssClass,highlightClass) {
    showSomeStuff(cssClass,highlightClass,true);
  }
  
  function showSomeStuff(cssClass,highlightClass,includePathList) {
    $sg("tbody tr td."+cssClass).removeClass("grayed").addClass(highlightClass);
    if(includePathList == true) {
      $sg("#pathList li."+cssClass).removeClass("grayed").addClass(highlightClass);
    }
    cells = $sg("tbody tr td."+highlightClass);
    for(i=0;i<cells.length;i++) {
      innerHtml = cells[i].innerHTML;
      innerHtml = innerHtml.replace(removeText,"");
      cells[i].innerHTML = innerHtml;
    }
  }
  
  function showAll(cssClassArray, highlightClass) {
    for(j=0;j<cssClassArray.length;j++) {
      showSomeStuff(cssClassArray[j], highlightClass, false);
    }
  }
  
  function showCluster(cssClass,highlightClass) { 
		$sg("#pathList li."+cssClass).removeClass("grayed").addClass(highlightClass);  
  }

  function enableDisableButton(ddlId, buttonId) {
    if(document.getElementById(ddlId).selectedIndex == 0) {
      document.getElementById(buttonId).disabled = true;
    } else {
      document.getElementById(buttonId).disabled = false;
    }
  }

  /**********************************************************************************************
   * Opens popup, waits 3 seconds.  If popup open was not successful, open page in same window. *
   **********************************************************************************************/
  function openPopupWithFallback(location,windowName) {
    win = window.open(location, windowName, "height=555,width=480,left=100,top=10,status=yes,menubar=yes,scrollbars=1,location=yes,resizable=1");
    if(window.focus) {
      win.focus();
    }
    timeoutString = "if(!win.opener) { document.location='"+location+"'; }";
    setTimeout(timeoutString, 3000);
  }
