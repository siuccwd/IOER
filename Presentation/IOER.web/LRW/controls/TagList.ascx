<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TagList.ascx.cs" Inherits="IOER.LRW.controls.TagList" %>

<!-- data from server -->
<asp:Literal ID="inputScript" runat="server" />
<!-- Init -->
<script type="text/javascript" language="javascript">
  var <%=config.entryType %>_enteredTags = [];
  $(document).ready(function () {
    if ("<%=mode %>" == "read" || "<%=mode %>" == "update") {
      for(i in <%=config.entryType %>_existingTags.tags){
        $(".existingTags[entryType=<%=config.entryType %>]")
        .append("<a href=\"/Search.aspx?<%=config.entryType %>=" + <%=config.entryType %>_existingTags.tags[i] + "\">" + <%=config.entryType %>_existingTags.tags[i] + "</a>");
      }
    }
    if("<%=mode %>" == "update" || "<%=mode %>" == "write"){
      for(i in <%=config.entryType %>_recommendedTags.tags){
        $(".recommendedTags[entryType=<%=config.entryType %>]")
        .append("<a href=\"javascript:addTag('" + <%=config.entryType %>_recommendedTags.tags[i] + "','<%=config.entryType %>')\">" + <%=config.entryType %>_recommendedTags.tags[i] + "</a>");
      }
    }
    if(!tagListsInitted){
      initTagLists();
      tagListsInitted = true;
    }
  });
</script>
<!-- Functionality -->
<script type="text/javascript" language="javascript">
  function initTagLists() {
    var tagBoxes = $(".entryBox");
    tagBoxes.bind("keyup blur", function (event) {
      event.stopPropagation();
      var code = (event.keyCode ? event.keyCode : event.which);
      if (code == 13 | code == 9 | code == 44) {
        var value = $(this).val().replace(/,/g, "").replace(/<script>/g, "").replace(/<\/script>/g, "").replace(/</g, "").replace(/>/g, "");
        addTag(value, $(this).attr("entryType"));
        $(this).val("");
      }
    });

    return false;
  }
  function addTag(value, entryType) {
    if (value.length < 3) {
      alert("That tag is too short!");
      return false;
    }
    var myList = window[entryType + "_enteredTags"];
    var count = 0;
    for (i in myList) {
      if (typeof (myList[i]) != "undefined") { //Avoids reassigning IDs after deleting items
        count++;
        if (myList[i] == value) {
          alert("That tag already exists.");
          return false;
        }
      }
    }
    if(count >= 10){
      alert("You have entered enough tags.");
      return false;
    }
    myList.push(value);
    updateUserAddedDisplay(entryType);
  }
  function updateUserAddedDisplay(entryType) {
    var outputBox = $(".userEnteredTags[entryType=" + entryType + "]");
    outputBox.html("");
    for (i in window[entryType + "_enteredTags"]) {
      outputBox.append("<div class=\"enteredTag\" tagID=>" + window[entryType + "_enteredTags"][i] + "<a href=\"javascript:removeTag('" + entryType + "'," + i + ");\">X</a></div>");
    }
  }
  function removeTag(entryType, id) {
    delete window[entryType + "_enteredTags"][id];
    updateUserAddedDisplay(entryType);
  }
  function <%=config.entryType %>_recordTags() {
    $(".<%=config.entryType %>_hdn").val( JSON.stringify(<%=config.entryType %>_enteredTags) );
  }
</script>

<input type="hidden" id="hdnList" runat="server" />
<div id="existingTags" runat="server" class="existingTags"></div>
<div id="recommendedTags" runat="server" class="recommendedTags"></div>
<input type="text" id="entryBox" runat="server" class="entryBox" placeholder="Type a tag and press Enter" onkeydown="return(event.keyCode !=13);" maxlength="50" />
<div id="userEnteredTags" runat="server" class="userEnteredTags"></div>