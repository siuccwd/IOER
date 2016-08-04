<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Dashboard1.ascx.cs" Inherits="IOER.LRW.controls.Dashboard1" %>
<%@ Register TagPrefix="uc1" TagName="HeaderControl" Src="/Controls/Includes/Header8.ascx" %>

<form id="mainForm" runat="server">

<% 
  //Easy CSS colors
  string css_black      = "#4F4E4F";
  string css_red        = "#B03D25";
  string css_orange     = "#FF5707";
  string css_purple     = "#9984BD";
  string css_teal       = "#4AA394";
  string css_gray       = "#909297";
  string css_blue       = "#3572B8";
  string css_white      = "#E6E6E6";
  string css_lightblue  = "#4C98CC";
%>

<script type="text/javascript" language="javascript" src="http://ajax.googleapis.com/ajax/libs/jquery/1.9.0/jquery.min.js"></script>
<link rel="Stylesheet" type="text/css" href="/Styles/ISLE.css" />

<uc1:HeaderControl ID="Header" runat="server" />

<!-- Data -->
<script type="text/javascript">
  /*var profileData = {
    name: "Nate",
    role: "Application Developer",
    organization: "ISLE OER",
    avatar: "/images/ioer_med.png"
  };*/
  var profileData;
  /*var libraryData = {
    name: "Nate's personal Library",
    avatar: "/images/ioer_med.png",
    description: "This is my personal Library"
  };*/
  var libraryData;
  /*var collectionItems = [
    { id: 1, href: "#", name: "Collection 1", resources: 99, image: "/images/ioer_med.png" },
    { id: 2, href: "#", name: "Collection 2", resources: 99, image: "/images/ioer_med.png" },
    { id: 3, href: "#", name: "Collection 3", resources: 99, image: "/images/ioer_med.png" },
    { id: 4, href: "#", name: "Collection 4", resources: 99, image: "/images/ioer_med.png" },
    { id: 5, href: "#", name: "Collection 5", resources: 99, image: "/images/ioer_med.png" },
    { id: 6, href: "#", name: "Collection 6", resources: 99, image: "/images/ioer_med.png" },
    { id: 7, href: "#", name: "Collection 7", resources: 99, image: "/images/ioer_med.png" },
    { id: 8, href: "#", name: "Collection 8", resources: 99, image: "/images/ioer_med.png" },
    { id: 9, href: "#", name: "Collection 9", resources: 99, image: "/images/ioer_med.png" },
    { id: 10, href: "#", name: "Collection 10", resources: 99, image: "/images/ioer_med.png" },
    { id: 11, href: "#", name: "Collection 11", resources: 99, image: "/images/ioer_med.png" },
  ];*/
  var collectionItems;
  /*var followedItems = [
    { id: 1, href: "#", name: "Library 1", resources: 99, image: "/images/ioer_med.png", following: 1 },
    { id: 2, href: "#", name: "Library 2", resources: 99, image: "/images/ioer_med.png", following: 2 },
    { id: 3, href: "#", name: "Library 3", resources: 99, image: "/images/ioer_med.png", following: 2 },
    { id: 4, href: "#", name: "Library 4", resources: 99, image: "/images/ioer_med.png", following: 1 },
    { id: 5, href: "#", name: "Library 5", resources: 99, image: "/images/ioer_med.png", following: 1 },
    { id: 6, href: "#", name: "Library 6", resources: 99, image: "/images/ioer_med.png", following: 1 },
    { id: 7, href: "#", name: "Library 7", resources: 99, image: "/images/ioer_med.png", following: 2 },
    { id: 8, href: "#", name: "Library 8", resources: 99, image: "/images/ioer_med.png", following: 2 },
    { id: 9, href: "#", name: "Library 9", resources: 99, image: "/images/ioer_med.png", following: 1 },
    { id: 10, href: "#", name: "Library 10", resources: 99, image: "/images/ioer_med.png", following: 1 },
    { id: 11, href: "#", name: "Library 11", resources: 99, image: "/images/ioer_med.png", following: 2 },
    { id: 12, href: "#", name: "Library 12", resources: 99, image: "/images/ioer_med.png", following: 1 },
  ];*/
  var followedItems;
  /*var resourceItems = [
    { id: 1, href: "#", name: "My Resource 1", image: "/images/ioer_med.png" },
    { id: 1, href: "#", name: "My Resource 2", image: "/images/ioer_med.png" },
    { id: 1, href: "#", name: "My Resource 3", image: "/images/ioer_med.png" },
    { id: 1, href: "#", name: "My Resource 4", image: "/images/ioer_med.png" },
    { id: 1, href: "#", name: "My Resource 5", image: "/images/ioer_med.png" },
    { id: 1, href: "#", name: "My Resource 6", image: "/images/ioer_med.png" },
    { id: 1, href: "#", name: "My Resource 7", image: "/images/ioer_med.png" },
    { id: 1, href: "#", name: "My Resource 8", image: "/images/ioer_med.png" },
    { id: 1, href: "#", name: "My Resource 9", image: "/images/ioer_med.png" },
    { id: 1, href: "#", name: "My Resource 10", image: "/images/ioer_med.png" },
    { id: 1, href: "#", name: "My Resource 11", image: "/images/ioer_med.png" },
    { id: 1, href: "#", name: "My Resource 12", image: "/images/ioer_med.png" },
    { id: 1, href: "#", name: "My Resource 13", image: "/images/ioer_med.png" },
    { id: 1, href: "#", name: "My Resource 14", image: "/images/ioer_med.png" },
    { id: 1, href: "#", name: "My Resource 15", image: "/images/ioer_med.png" },
    { id: 1, href: "#", name: "My Resource 16", image: "/images/ioer_med.png" },
    { id: 1, href: "#", name: "My Resource 17", image: "/images/ioer_med.png" },
    { id: 1, href: "#", name: "My Resource 18", image: "/images/ioer_med.png" },
    { id: 1, href: "#", name: "My Resource 19", image: "/images/ioer_med.png" },
    { id: 1, href: "#", name: "My Resource 20", image: "/images/ioer_med.png" },
  ];*/
  var resourceItems;
</script>

<!-- Initialization -->
<script type="text/javascript" language="javascript">
  var jWindow;
  var profileBox;
  var expanders;
  var myLibrary;
  var myFollowed;
  var myResources;
  var expandoSliders;
  var container;
  var sliders;
  $(document).ready(function () {
    //Init items
    jWindow = $(window);
    profileBox = $("#profile");
    expanders = $("#expanders");
    myLibrary = $("#myLibrary");
    myFollowed = $("#myFollowed");
    myResources = $("#myResources");
    expandoSliders = $(".expandoSlider");
    container = $("#container");
    sliders = $(".slider");

    //Setup sizing
    jWindow.resize(function () { resizeStuff(); }).resize();

    //Setup Avatar trigger
    $(".fileProfileAvatar").change(function () {
      submitProfileAvatar();
    });
    $(".fileLibraryAvatar").change(function () {
      submitLibraryAvatar();
    });

    //Get initial data
    loadProfile();
    loadLibrary();

    //Get more data
    loadCollectionItems();
    loadFollowedItems();
    loadResourceItems();

  });

</script>
<!-- Server Communication -->
<script type="text/javascript" language="javascript">

  function loadProfile() {
    //Get data from server
    bounceBack("GetProfile", renderProfile, {});
    //renderProfile(profileData); //Temporary
  }
  function loadLibrary() {
    bounceBack("GetLibrary", renderLibrary, {});
    //renderLibrary(libraryData);  //Temporary
  }
  function loadCollectionItems() {
    bounceBack("GetCollectionItems", renderCollectionItems, {});
    //renderCollectionItems(collectionItems); //Temporary
  }
  function loadFollowedItems() {
    bounceBack("GetFollowedItems", renderFollowedItems, {});
    //renderFollowedItems(followedItems); //Temporary
  }
  function loadResourceItems() {
    bounceBack("GetResourceItems", renderResourceItems, {});
    //renderResourceItems(resourceItems); //Temporary
  }

  function submitProfileUpdate() {
    //bounceBack("UpdateProfile", renderProfile, profileData); //Activate this once the database stuff is ready
    renderProfile(profileData); //Temporary
  }
  function submitLibraryUpdate() {
    //bounceBack("UpdateLibrary", renderLibrary, libraryData); //Activate this once the database stuff is ready
    renderLibrary(libraryData); //Temporary
  }
  function submitfollowingUpdate() {
    //bounceback("UpdateFollowing", renderFollowedItems, followedItems); //Activate this once the database stuff is ready
    renderFollowedItems(followedItems) //Temporary
  }

  function submitProfileAvatar() {
    bounceBack("UpdateProfileAvatar", renderProfile, profileData);
    //May need a postback instead
    //File uploads are still tricky
    //renderProfile(profileData); //Temporary

    //$(".formProfileAvatar").submit();
    $(".btnSubmitProfileAvatar").click();
  }
  function submitLibraryAvatar() {
    bounceBack("UpdateLibraryAvatar", renderProfile, libraryData);
    //May need a postback instead
    //File uploads are still tricky
    //renderProfile(profileData); //Temporary

    //$(".formLibraryAvatar").submit();
    $(".btnSubmitLibraryAvatar").click();
  }


  //Generic AJAX
  function bounceBack(serverMethod, returnMethod, info) {
    var envelope = {
      user: "<%=userGUID %>",
      data: info
    };
    $.ajax({
      url: "/Services/DashboardService.asmx/" + serverMethod,
      async: true,
      success: function (msg) { returnMethod(msg.d) },
      error: function (msg) { console.log(msg) },
      type: "POST",
      data: JSON.stringify({ input: envelope }),
      dataType: "json",
      contentType: "application/json; charset=utf-8",
    });
  }
</script>
<!-- Rendering -->
<script type="text/javascript" language="javascript">
  function renderProfile(data) {
    profileData = data;
    $("#profileName").html(data.name);
    $("#profileRole").html(data.role);
    $("#profileOrganization").html(data.organization);
    $("#profileAvatar").css("background-image", "url('" + data.avatar + "')");
    $("#txtProfileName").val(data.name);
    $("#txtProfileRole").val(data.role);
    $("#txtProfileOrganization").val(data.organization);
    profileFunctions.hideUpdateItems();
  }
  function renderLibrary(data) {
    libraryData = data;
    $("#libraryTitle").html(data.name);
    $("#libraryDescription").html(data.description);
    $("#libraryAvatar").css("background-image", "url('" + data.avatar + "')");
    $("#txtLibraryTitle").val(data.name);
    $("#txtLibraryDescription").val(data.description);
    libraryFunctions.hideUpdateItems();
  }
  function renderCollectionItems(data) {
    collectionItems = data;
    var librarySlider = $("#librarySlider");
    var libraryContent = librarySlider.find(".sliderContent");
    libraryContent.html("");
    for (i in data) {
      libraryContent.append(
        $("#template_thumbnailItem").html()
        .replace("{type}", "myLibrary")
        .replace("{id}", data[i].id)
        .replace("{href}", data[i].href)
        .replace("{image}", data[i].image)
        .replace("{title}", data[i].name + "<br />" + data[i].resources + " Resources")
      );
    }
    libraryContent.width(data.length * ($(".thumbnail").width() + 15));
  }
  function renderFollowedItems(data) {
    followedItems = data;
    var followedSlider = $("#followedSlider");
    var followedContent = followedSlider.find(".sliderContent");
    followedContent.html("");
    for (i in data) {
      var titleText = $("#template_followDDL").html().replace(/{id}/g, data[i].id) + data[i].name;
      followedContent.append(
        $("#template_thumbnailItem").html()
        .replace("{type}", "followed")
        .replace("{id}", data[i].id)
        .replace("{href}", data[i].href)
        .replace("{image}", data[i].image)
        .replace("{title}", titleText)
      );
      $("#ddl_" + data[i].id + " option").each(function () {
        if ($(this).attr("value") == data[i].following) {
          $(this).attr("selected", "selected");
        }
      });
    }
    followedContent.width(data.length * ($(".thumbnail").width() + 15));
  }
  function renderResourceItems(data) {
    resourceItems = data;
    var resourceSlider = $("#resourceSlider");
    var resourceContent = resourceSlider.find(".sliderContent");
    resourceContent.html("");
    for (i in data) {
      resourceContent.append(
        $("#template_thumbnailItem").html()
        .replace("{type}", "followed")
        .replace("{id}", data[i].id)
        .replace("{href}", data[i].href)
        .replace("{image}", data[i].image)
        .replace("{title}", data[i].name)
      );
    }
    resourceContent.width(data.length * ($(".thumbnail").width() + 15));
  }
</script>
<!-- Utilities -->
<script type="text/javascript" language="javascript">
  //Resizes the stuff to fit properly
  function resizeStuff() {
    var availableWidth = container.width() - profileBox.outerWidth() + 1 -
    (parseInt(container.css("padding-left"), 10) + parseInt(container.css("padding-right"), 10)) -
    (parseInt(profileBox.css("margin-left"), 10) + parseInt(profileBox.css("margin-right"), 10));
    var availableHeight = jWindow.height() - 165;
    if (availableHeight < 350) { availableHeight = 350; }
    var combinedHeight = 8;
    expandoSliders.each(function () {
      combinedHeight += $(this).height();
    });
    if (combinedHeight < 350) { combinedHeight = 350; }
    if (container.width() >= 750) {
      expanders.width(availableWidth);
      expanders.height(availableHeight);
      profileBox.height(combinedHeight);
      profileBox.width(200);
    }
    else {
      expanders.css("width", "100%");
      expanders.css("height", "350px");
      profileBox.css("height", "350px");
      profileBox.css("width", "100%");
    }
    sliders.each(function () {
      $(this).height($(this).parent().height() - 50);
    });
    $("#librarySlider").width(myLibrary.width() - $("#libraryProfile").width() - 30);
    $("#followedSlider").width(myFollowed.width() - 18);
    $("#resourceSlider").width(myResources.width() - 18);

  }

  function expandoToggle(target) {
    var item = $("#" + target);
    if (item.attr("toggled") == "true") {
      expandoSliders.css("height", "33.3%").css("min-height","275px");
      item.attr("toggled", "false");
      item.find(".slider").css("overflow-x", "scroll").css("overflow-y", "hidden");

      $(".sliderContent").each(function () {
        var content = $(this);
        var thumbs = content.find(".thumbnail");
        content.css("width", (thumbs.width() + 15) * thumbs.length);
        thumbs.css("height", "90%");
      });

    }
    else {
      expandoSliders.css("min-height", "25px").css("height", "25px").attr("toggled", "false");
      item.css("height", jWindow.height() - 215).css("min-height","275px");
      item.attr("toggled", "true");

      item.find(".slider").css("overflow", "auto");
      item.find(".sliderContent").css("width", "auto");
      item.find(".thumbnail").css("height", "200px");

    }
    setTimeout(resizeStuff, 505);
  }
</script>
<!-- Profile manipulation -->
<script type="text/javascript" language="javascript">
  var profileFunctions = {
    startUpdate: function () {
      this.showUpdateItems();
    },
    saveUpdate: function () {
      this.hideUpdateItems();
      if (this.validateProfileBoxes()) {
        profileData.name = $("#txtProfileName").val();
        profileData.role = $("#txtProfileRole").val();
        profileData.organization = $("#txtProfileOrganization").val();
        submitProfileUpdate();
      }
    },
    showUpdateItems: function () {
      $("#profileDisplayItems").hide();
      $("#profileStartUpdate").hide();
      $("#profileUpdateItems").fadeIn("fast");
      $("#profileSaveUpdate").fadeIn("fast");

    },
    hideUpdateItems: function () {
      $("#profileDisplayItems").fadeIn("fast");
      $("#profileStartUpdate").fadeIn("fast");
      $("#profileUpdateItems").hide();
      $("#profileSaveUpdate").hide();

    },
    validateProfileBoxes: function () {
      var isValid = true;
      //Invalid characters
      var illegals = this.illegalCharacters.split("");
      //For each box...
      $("#profileUpdateItems input[type=text]").each(function () {
        var box = $(this);
        box.val(box.val().trim());
        //Check length
        if (box.val().length < 3) {
          alert("One or more fields is too short.");
          isValid = false;
        }
        //Check illegal characters
        for (i in illegals) {
          if (box.val().indexOf(illegals[i]) > 0) {
            alert("One or more illegal characters detected.");
            isValid = false;
          }
        }
      });
      return isValid;
    },
    illegalCharacters: "\"<>!/\\@&%$#^*"
  };

</script>
<!-- Library manipulation -->
<script type="text/javascript" language="javascript">
  var libraryFunctions = {
    startUpdate: function () {
      this.showUpdateItems();
    },
    saveUpdate: function () {
      this.hideUpdateItems();
      if (this.validateProfileBoxes()) {
        libraryData.name = $("#txtLibraryTitle").val();
        libraryData.role = $("#txtLibraryDescsription").val();
        submitLibraryUpdate();
      }
    },
    showUpdateItems: function () {
      $("#libraryDisplayItems").hide();
      $("#libraryStartUpdate").hide();
      $("#libraryUpdateItems").fadeIn("fast");
      $("#librarySaveUpdate").fadeIn("fast");

    },
    hideUpdateItems: function () {
      $("#libraryDisplayItems").fadeIn("fast");
      $("#libraryStartUpdate").fadeIn("fast");
      $("#libraryUpdateItems").hide();
      $("#librarySaveUpdate").hide();

    },
    validateProfileBoxes: function () {
      var isValid = true;
      //Invalid characters
      var illegals = this.illegalCharacters.split("");
      //For each box...
      $("#libraryUpdateItems input[type=text]").each(function () {
        var box = $(this);
        box.val(box.val().trim());
        //Check length
        if (box.val().length < 3) {
          alert("One or more fields is too short.");
          isValid = false;
        }
        //Check illegal characters
        for (i in illegals) {
          if (box.val().indexOf(illegals[i]) > 0) {
            alert("One or more illegal characters detected.");
            isValid = false;
          }
        }
      });
      return isValid;
    },
    illegalCharacters: "\"<>!/\\@&%$#^*"
  };

</script>
<!-- Subscription manipulation -->
<script type="text/javascript" language="javascript">
  function changeFollowing(select) {
    var ddl = $(select);
    console.log(ddl.find("option:selected").attr("value"));
    console.log(ddl.attr("id").replace("ddl_", ""));
    for (i in followedItems) {
      if (followedItems[i].id == ddl.attr("id").replace("ddl_", "")) {
        followedItems[i].following = parseInt(ddl.find("option:selected").attr("value"));
      }
    }
    submitFollowingUpdate();
  }
</script>

<!-- Major page elements -->
<style type="text/css">
html, body {
  margin: 0;
  padding: 0;
  height: 100%;
  width: 100%;
}
#container {
  padding: 5px;
  min-width: 400px;
}
#profile, #expanders, .expandoSlider {
  -webkit-box-sizing: border-box;
  -moz-box-sizing: border-box;
  box-sizing: border-box;
  border-radius: 5px;
  border: 1px solid <%=css_white %>;
  margin: 2px;
}
#profile, #expanders {
  display: inline-block;
  vertical-align: top;
  min-height: 350px;
  margin-bottom: 10px;
}
#profile {
  width: 200px;
  height: 100%;
  min-height: 300px;
}
#expanders {
  border: none;
  padding: 0;
  margin: -0.5px -20px 10px -0.5px;
  width: 80%;
}
.expandoSlider {
  padding: 0;
  width: 100%;
  height: 33.3%;
  min-height: 275px;
  position: relative;
  transition: height 0.5s, min-height 0.5s;
  -webkit-transition: height 0.5s, min-height 0.5s;
  -moz-transition: height 0.5s, min-height 0.5s;
  overflow: hidden;
  text-align: center;
}
.expandoSlider h2 {
  text-align: left;
}
#profile > h2.isleBox_H2, .expandoSlider > h2.isleBox_H2 {
  margin: -1px -1px 5px -1px;
}
.expandoSlider > div {
  -webkit-box-sizing: border-box;
  -moz-box-sizing: border-box;
  box-sizing: border-box;
}
</style>
<!-- Profile -->
<style type="text/css">
#profileAvatar {
  height: 120px;
  width: 120px;
  background-position: center center;
  background-repeat: no-repeat;
  border-radius: 5px;
  margin: 10px auto;
}
#profileInfo div {
  padding: 5px;
  font-size: 16px;
}
#profile #profileUpdateItems input[type=text] {
  margin: 6px auto;
  font-family: Calibri, Arial, Helvetica, Sans-Serif;
}
#profile input[type=button] {
  width: 180px;
  padding: 2px;
  display: block;
  margin: 2px auto;
}
#profileUpdateItems input[type=text], .fileProfileAvatar {
  width: 190px;
  display: block;
  margin: 2px auto;
}
.fileProfileAvatar {
  margin-top: -30px;
  opacity: 0;
  filter: alpha(0);
  font-size: 12px;
}
#profileAvatarBox:hover .fileProfileAvatar, .fileProfileAvatar:focus {
  display: block;
  opacity: 1;
  filter: alpha(100);
}
#profileName, #txtProfileName {
  font-weight: bold;
}
</style>
<!-- Library -->
<style type="text/css">
#libraryProfile {
  width: 200px;
  padding: 5px;
  display: inline-block;
  vertical-align: top;
}
#libraryProfile div {
  padding: 2px;
  
}
#libraryAvatar {
  width: 100px;
  height: 100px;
  margin: 5px auto;
  background-position: center center;
  background-repeat: no-repeat;
  border-radius: 5px;
}
.fileLibraryAvatar {
  margin-top: -30px;
  opacity: 0;
  filter: alpha(0);
  font-size: 12px;
  width: 180px;
}
#libraryAvatarBox:hover .fileLibraryAvatar, .fileLibraryAvatar:focus {
 opacity: 1;
 filter: alpha(100); 
}
#libraryProfile input[type=button] {
  width: 180px;
  padding: 2px;
  display: block;
  margin: 2px auto;
}
#libraryProfile input[type=text] {
  width: 180px;
  display: block;
  margin: 2px auto;
}
#libraryTitle, #txtLibraryTitle {
  font-weight: bold;
}
</style>
<!-- Miscellaneous -->
<style type="text/css">
.expandoToggle {
  position: absolute;
  right: 2px;
  top: 1.5px;
  font-weight: bold;
  background-color: #FFF;
  color: <%=css_blue %>;
  border-radius: 5px;
  padding: 1px 5px;
}
.expandoToggle:hover, .expandoToggle:focus {
  color: #FFF;
  background-color: <%=css_orange %>;
}
.defaultButton {
  color: #FFF;
  background-color: <%=css_black %>;
  font-weight: bold;
  padding: 2px 5px;
  border-radius: 5px;
  cursor: pointer;
}
.defaultButton:hover, .defaultButton:focus {
  background-color: <%=css_orange %>;
}
.hidden {
  display:none;
}
.center {
  display: block;
  text-align: center;
}
#libraryDisplayItems {
  text-align: left;
}
</style>
<!-- Sliders -->
<style type="text/css">
.slider {
  display: inline-block;
  min-width: 180px;
  border: 1px solid <%=css_white %>;
  overflow-x: scroll;
  overflow-y: hidden;
  border-radius: 5px;
  vertical-align: top;
  margin: 5px;
}
.thumbnail {
  width: 150px;
  height: 95%;
  margin: 5px;
  border: 1px solid <%=css_white %>;
  border-radius: 5px;
  display: inline-block;
  vertical-align: top;
  -webkit-box-sizing: border-box;
  -moz-box-sizing: border-box;
  box-sizing: border-box;

}
.thumbnail a {
  background-position: center center;
  background-repeat: no-repeat;
  height: 100%;
  width: 100%;
  vertical-align: bottom;
  display: block;
  text-align: center;
  position: relative;
}
.thumbnail a span {
  display: block;
  padding: 2px;
  position: absolute;
  bottom: 0;
  width: 100%;
  background-color: #FFF;
  font-weight: bold;
  -webkit-box-sizing: border-box;
  -moz-box-sizing: border-box;
  box-sizing: border-box;
}
.thumbnail select {
  width: 90%;
}
</style>
<div id="container">
  <h1 class="isleH1">My Dashboard</h1>
  <div id="dashboardContent" runat="server">
    <!-- Profile -->
    <%--<div id="profile">
      <h2 class="isleBox_H2">My Profile</h2>
      <div id="profileInfo">
        <div id="profileAvatarBox">
          <div id="profileAvatar"></div>
          <input type="file" id="fileProfileAvatar" class="fileProfileAvatar" runat="server" />
          <asp:Button ID="submitProfileAvatar" runat="server" OnClick="submitProfileAvatar_Click" CssClass="hidden btnSubmitProfileAvatar" />
        </div>
        <div id="profileDisplayItems">
          <div id="profileName"></div>
          <div id="profileRole"></div>
          <div id="profileOrganization"></div>
        </div>
        <div id="profileUpdateItems">
          <input type="text" id="txtProfileName" placeholder="Name" />
          <input type="text" id="txtProfileRole" placeholder="Role" />
          <input type="text" id="txtProfileOrganization" placeholder="Organization" />
        </div>
      </div>
      <input type="button" class="defaultButton" id="profileStartUpdate" onclick="profileFunctions.startUpdate()" value="Update Basic Info" />
      <input type="button" class="defaultButton" id="profileSaveUpdate" onclick="profileFunctions.saveUpdate()" value="Save Updates" />
    </div>--%>
    <div id="profile">
      <h2 class="isleBox_H2">My Profile</h2>
      <div id="profileInfo">
        <div id="profileAvatar"></div>
        <div id="profileDisplayItems">
          <div id="profileName"></div>
          <div id="profileRole"></div>
          <div id="profileOrganization"></div>
        </div>
        <a href="/Account/Profile.aspx" class="center">Update My Profile</a>
      </div>
    </div>
    <div id="expanders">
      <!-- My Library -->
      <div id="myLibrary" class="expandoSlider">
        <h2 class="isleBox_H2">My Library</h2>
        <%--<div id="libraryProfile">
          <div id="libraryAvatarBox">
            <div id="libraryAvatar"></div>
            <input type="file" id="fileLibraryAvatar" class="fileLibraryAvatar" runat="server" />
            <asp:Button ID="submitLibraryAvatar" runat="server" OnClick="submitLibraryAvatar_Click" CssClass="hidden btnSubmitLibraryAvatar" />
          </div>
          <div id="libraryDisplayItems">
            <div id="libraryTitle"></div>
            <div id="libraryDescription"></div>
          </div>
          <div id="libraryUpdateItems">
            <input type="text" id="txtLibraryTitle" placeholder="Library Title" />
            <input type="text" id="txtLibraryDescription" placeholder="Library Description" />
          </div>
          <input type="button" class="defaultButton" id="libraryStartUpdate" onclick="libraryFunctions.startUpdate()" value="Update Library" />
          <input type="button" class="defaultButton" id="librarySaveUpdate" onclick="libraryFunctions.saveUpdate()" value="Save Updates" />
        </div>--%>
        <div id="libraryProfile">
          <div id="libraryAvatar"></div>
          <div id="libraryDisplayItems">
            <div id="libraryTitle"></div>
            <div id="libraryDescription"></div>
          </div>
          <a href="http://localhost:99/My/Library.aspx" class="center">Update My Library</a>
        </div>
        <div id="librarySlider" class="slider"><div class="sliderContent"></div></div>
        <a href="javascript:expandoToggle('myLibrary')" id="expandContract" class="expandoToggle">Expand/Contract</a>
      </div><!-- /myLibrary -->

      <!-- My Followed -->
      <div id="myFollowed" class="expandoSlider">
        <h2 class="isleBox_H2">My Followed Libraries</h2>
        <div id="followedSlider" class="slider"><div class="sliderContent"></div></div>
        <a href="javascript:expandoToggle('myFollowed')" id="expandContract" class="expandoToggle">Expand/Contract</a>
      </div><!-- /myFollowed -->

      <!-- My Resources -->
      <div id="myResources" class="expandoSlider">
        <h2 class="isleBox_H2">My Resources</h2>
        <div id="resourceSlider" class="slider"><div class="sliderContent"></div></div>
        <a href="javascript:expandoToggle('myResources')" id="expandContract" class="expandoToggle">Expand/Contract</a>
      </div><!-- /myResources -->
    </div><!-- /expanders -->
  </div><!-- /dashboardContent -->
  <div id="notLoggedInContent" runat="server" visible="false">
    <p class="center">You must be signed in to view your Dashboard.</p>
  </div>
</div><!-- /container -->

<div id="hiddenStuff" style="display:none;">
  <div id="template_thumbnailItem">
    <div class="thumbnail" type="{type}" id="{id}">
      <a href="{href}" style="background-image:url('{image}')"><span>{title}</span></a>
    </div>
  </div>
  <div id="template_followDDL">
    <select id="ddl_{id}" onchange="changeFollowing(this)" onclick="return false;">
      <option value="0">Unfollow</option>
                      <option value="1">Follow in my Timeline</option>
                      <option value="2">Follow with a daily email and in my Timeline</option>
                      <option value="3">Follow with a weekly email and in my Timeline</option>
    </select>
  </div>
</div>

<asp:Panel ID="hiddenItems" runat="server" Visible="false">
  <asp:Literal ID="demoMode" runat="server">false</asp:Literal>
</asp:Panel>

</form>