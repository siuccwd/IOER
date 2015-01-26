<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Library1.ascx.cs" Inherits="ILPathways.Widgets.Library.Library1" %>

<script src="//ajax.googleapis.com/ajax/libs/jquery/1.10.2/jquery.min.js" type="text/javascript"></script>
<link rel="stylesheet" type="text/css" href="/Styles/ISLE.css" />
<link rel="stylesheet" type="text/css" href="/Styles/common2.css" />
<script type="text/javascript">
  //From server
  <%=libraryData %>
</script>
<script type="text/javascript">
  $(document).ready(function () {
    //Load data
    successGetLibrary(libraryData);
  });

  function successGetLibrary(data) {
    if (data.isValid) {
      renderLibrary(data.data);
    }
    else {
      $("#libraryTitle").html("Error: " + data.status);
    }
  }

  function renderLibrary(data) {
    $("#libraryTitle").attr("href", data.link).html(data.title);
    var collectionTemplate = $("#template_collection").html();
    var resourceTemplate = $("#template_resource").html();
    var collectionListTemplate = $("#template_listCollection").html();

    $("#collections").html("");

    for (i in data.items) {
      var resources = "";
      for (j in data.items[i].items) {
        var current = data.items[i].items[j];
        resources += resourceTemplate
          .replace(/{thumbsrc}/g, "src=\"" + current.thumbURL + "\"")
          .replace(/{title}/g, current.title)
          .replace(/{link}/g, current.link)
      }

      $("#collections").append(
        collectionTemplate
          .replace(/{title}/g, data.items[i].title)
          .replace(/{link}/g, data.items[i].link)
          .replace(/{list}/g, resources)
      );
    }
    
    if (data.showCollections == true ) {
        $("#allCollections #list").html("");
        for (i in data.collections) {
            var current = data.collections[i];
            $("#allCollections #list").append(
              collectionListTemplate
                .replace(/{thumbsrc}/g, "src=\"" + current.thumbURL + "\"")
                .replace(/{title}/g, current.title)
                .replace(/{link}/g, current.link)
            );
        }
    } else {
        $("#allCollections").css("display", "none");
    }

    if (data.showHeader == false) 
        $("#libraryHeader").css("display", "none");
  }
</script>
<style type="text/css">
  body { padding: 0; margin: 0; background-color: #EEE; }
  a { font-size: inherit; }
  #libraryTitle { font-size: 26px; padding: 5px; background-color: #4AA394; color: #FFF; display: block; }
  .collectionTitle { font-size: 20px; background-color: #DDD; padding: 10px 5px 5px 5px; }
  .collectionList, #allCollections #list { box-shadow: 0 0 100px -20px #CCC inset; background-color: #FFF; white-space: nowrap; overflow-x: scroll; overflow-y: hidden; }
  .resource, .listCol { 
    background-color: #EEE; 
    background: linear-gradient(#DFDFDF, #EFEFEF);
    background: -webkit-linear-gradient(#DFDFDF, #EFEFEF);
    display: inline-block;
    vertical-align: top;
    border-radius: 5px; 
    margin: 5px 2.5px;
    padding: 5px;
  }
  .collectionList .resource { width: 180px; }
  .collectionList .thumbnailBox { width: 100%; position: relative; border-radius: 5px; overflow: hidden; }
  .collectionList .resizer { width: 100%; }
  .collectionList .thumbnail, .collectionList .message { position: absolute; top: 0; left: 0; height: 100%; width: 100%; }
  .title { height: 3.5em; overflow: hidden; white-space: normal; }

  #allCollections #list .listCol { height: 100px; white-space: nowrap; }
  #allCollections #list .collectionThumb { height: 100%; min-width: 50px; }
  #allCollections #list .title { padding: 0 5px; max-width: 150px; }
  #allCollections #list .collectionThumb, #allCollections #list .title { display: inline-block; vertical-align: top; margin-right: -4px; white-space: normal;  }
</style>
<input type="hidden" id="senderId" name="senderid" value="<%=(ViewState["senderId"] != null ? ViewState["senderId"].ToString() : "")%>" />

<div id="widgetContent">
  <h1 id="libraryHeader"><a id="libraryTitle" class="textLink" target="_blank"></a></h1>
  <div id="allCollections">
    <h2 class="collectionTitle">Browse All Collections:</h2>
    <div id="list"></div>
  </div>
  <div id="collections"></div>

    <div id="divMessages"></div>
</div>

<div id="templates" style="display:none;">
  <div id="template_collection">
    <div class="collection">
      <h2 class="collectionTitle">Collection: <a href="{link}" class="textLink" target="_blank">{title}</a></h2>
      <div class="collectionList">
        {list}
      </div>
    </div>
  </div>
  <div id="template_resource">
    <a class="resource textLink" href="{link}" target="_blank" title="{title}">
      <div class="thumbnailBox">
        <img class="resizer" src="/images/ThumbnailResizer.png" />
        <img class="thumbnail" {thumbsrc} onerror="this.src='/images/icons/filethumbs/filethumb_unavailable_400x300.png';" />
        <div class="message"></div>
      </div>
      <div class="title">{title}</div>
    </a>
  </div>
  <div id="template_listCollection">
    <a href="{link}" target="_blank" title="{title}" class="listCol">
      <img class="collectionThumb" {thumbsrc} />
      <div class="title">{title}</div>
    </a>
  </div>
</div>


<script type="text/javascript">
    /* initial test, just show the message*/
    window.addEventListener("message", receiveMessageInApp, false);
    function receiveMessageInApp(event)
    {   
        divMsg=document.getElementById('divMessages');
        divMsg.innerHTML= divMsg.innerHTML+event.data+'<br/>';
    }


</script>