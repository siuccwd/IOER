<%@ Page Title="Illinois Open Educational Resources - Collections Widget" Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="IOER.Widgets.Collection.Default" %>

<!DOCTYPE HTML>
<html lang="en">
<head id="Head1" runat="server">
    <title>IOER Collection</title>
        <script src="//ajax.googleapis.com/ajax/libs/jquery/1.10.2/jquery.min.js" type="text/javascript"></script>
<link rel="stylesheet" type="text/css" href="/Styles/ISLE.css" />
<link rel="stylesheet" type="text/css" href="/Styles/common2.css" />
<script type="text/javascript">
    //From server
    <%=libraryData %>
    <%=libraryIdString %>
    <%=collectionIdString %>
    <%=resourceMaxString %>
    <%=usingTargetUrlString %>
    

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
    function successGetLibrary2(data) {
        renderLibrary(data);
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

        if (data.showCollections == true) {
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
</head>
<body>
    <form id="form1" runat="server">
        <input type="hidden" id="senderId" name="senderid" value="<%=(ViewState["senderId"] != null ? ViewState["senderId"].ToString() : "")%>" />

<div id="widgetContent">
  <h1 id="libraryHeader"><a id="libraryTitle" class="textLink" target="_blank"></a></h1>
  <div id="allCollections">
    <h2 class="collectionTitle">Browse All Collections:</h2>
    <div id="list"></div>
  </div>
  <div id="collections"></div>
    <br />TEMP
    <div id="divMessages2" style="color: red;"></div>
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
        <img alt="" class="resizer" src="/images/ThumbnailResizer.png" />
        <img alt="" class="thumbnail" {thumbsrc} onerror="this.src='/images/icons/filethumbs/filethumb_unavailable_400x300.png';" />
        <div class="message"></div>
      </div>
      <div class="title">{title}</div>
    </a>
  </div>
  <div id="template_listCollection">
    <a href="{link}" target="_blank" title="{title}" class="listCol">
      <img alt="" class="collectionThumb" {thumbsrc} />
      <div class="title">{title}</div>
    </a>
  </div>
</div>

<script type="text/javascript">
    function getQueryStringParameter(key, urlToParse) {
        /// <signature>
        /// <summary>Gets a querystring parameter, case sensitive.</summary>
        /// <param name="key" type="String">The querystring key (case sensitive).</param>
        /// <param name="urlToParse" type="String">A url to parse.</param>
        /// </signature>
        /// <signature>
        /// <summary>Gets a querystring parameter from the document's URL, case sensitive.</summary>
        /// <param name="key" type="String">The querystring key (case sensitive).</param>
        /// </signature>
        if (!urlToParse || urlToParse.length === 0) {
            urlToParse = document.URL;
        }
        if (urlToParse.indexOf("?") === -1) {
            return "";
        }
        var params = urlToParse.split('?')[1].split('&');
        for (var i = 0; i < params.length; i = i + 1) {
            var singleParam = params[i].split('=');
            if (singleParam[0] === key) {
                return decodeURIComponent(singleParam[1]);
            }
        }
        return "";
    }

    (function () {

        var getCollectionNameListener = function (e) {
            /// <summary>Callback function for getting the host page's info via postMessage api.</summary>
            console.log("Collections: postMessage response received: " + e.data);

            var messageData;
            try {
                messageData = JSON.parse(e.data);
                console.log("Collections: postMessage response received JSON: " + messageData);
                RefreshList(messageData);

            }
            catch (error) {
                console.log("Collections: Unable to parse the response from the host page.");
                return;
            }
        }

        // Register the listener
        if (typeof window.addEventListener !== 'undefined') {
            window.addEventListener('message', getCollectionNameListener, false);
        }
        else if (typeof window.attachEvent !== 'undefined') {
            window.attachEvent('onmessage', getCollectionNameListener);
        }

        // Send the host page a message
        var hostPageMessage = {};
        hostPageMessage.message = "getCollectionName";
        var hostPageMessageString = JSON.stringify(hostPageMessage);

        window.parent.postMessage(hostPageMessageString, document.referrer);
        console.log("Collections: Sent host page a message: " + hostPageMessageString);

    })();

    function RefreshList(collectionName) {
       // __doPostBack('collectionMsg', category);
        doAjax("GetCollection", { libraryId: libraryID,  collectionName: collectionName,  resourceMax: resourceMax, usingTargetUrl: usingTargetUrl}, successGetLibrary);
    }

    function doAjax(method, data, success) {
        console.log("doAjax: data: " + data);

        $.ajax({
            type: "POST",
            async: true,
            url: "Default.aspx/" + method,
            data: JSON.stringify(data),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                try {
                    success($.parseJSON(msg.d));
                }
                catch (e) {
                    success($.parseJSON(msg.d));
                }
            }
        });
    }

    </script>
<script type="text/javascript">
    /* initial test, just show the message*/
    window.addEventListener("message", receiveMessageInApp, false);
    function receiveMessageInApp(event) {
        divMsg = document.getElementById('divMessages2');
        divMsg.innerHTML = event.data + '<br/>';

    }


</script>
        

<asp:Panel ID="hiddenPanel" runat="server" Visible="false">
<asp:Literal ID="txtSenderId" runat="server"></asp:Literal>
<asp:Literal ID="txtLibraryId" runat="server">0</asp:Literal>
<asp:Literal ID="txtCollectionId" runat="server">0</asp:Literal>
<asp:Literal ID="txtResourceMax" runat="server">10</asp:Literal>
<asp:Literal ID="txtCollectionMax" runat="server">5</asp:Literal>
<asp:Literal ID="txtShowingHdr" runat="server">false</asp:Literal>
<asp:Literal ID="txtUsingTargetUrl" runat="server">true</asp:Literal>
<asp:Literal ID="txtShowingCollectionList" runat="server">false</asp:Literal>
<asp:Literal ID="txtKeyword" runat="server"></asp:Literal>
<asp:Literal ID="txtCollectionsList" runat="server"></asp:Literal>
<asp:Literal ID="txtCollectionName" runat="server"></asp:Literal>

</asp:Panel>
    </form>

    
<script type="text/javascript">
    // Set the style of the app part page to be consistent with the host web.
    // Get the URL of the host web and load the styling of it.
    function setStyleSheet() {
        var hostUrl = ""
        if (document.URL.indexOf("?") != -1) {
            var params = document.URL.split("?")[1].split("&");
            for (var i = 0; i < params.length; i++) {
                p = decodeURIComponent(params[i]);
                if (/^SPHostUrl=/i.test(p)) {
                    hostUrl = p.split("=")[1];
                    document.write("<link rel=\"stylesheet\" href=\"" + hostUrl +
                        "/_layouts/15/defaultcss.ashx\" />");
                    break;
                }
            }
        }
        // if no host web URL was available, load the default styling
        if (hostUrl == "") {
            document.write("<link rel=\"stylesheet\" " +
                "href=\"/_layouts/15/1033/styles/themable/corev15.css\" />");
        }
    }
    setStyleSheet();
</script>
<script type="text/javascript">

    "use strict";
    window.Communica = window.Communica || {};

    $(document).ready(function () {
        Communica.Part.init();

        setTimeout(function () {
            Communica.Part.init();
        }, 5000);

        $(window).resize(function () {
            Communica.Part.init();
            console.log("resize");
        });
    });

    Communica.Part = {
        senderId: '',

        init: function () {
            this.adjustSize();
        },

        adjustSize: function () {
            var step = 30,
                newHeight,
                newWidth = "100%",
                contentHeight = $('body').height(),
                resizeMessage = '<message senderId={Sender_ID}>resize({Width}, {Height})</message>';

            newHeight = (step - (contentHeight % step)) + contentHeight;

            //if (window.console === undefined) {
            //    console.log("New Height: " + newHeight);
            //    console.log("SenderId: " + $("#senderId").html());
            //    console.log("windowWidth: " + windowWidth);
            //}

            console.log("newHeight: " + newHeight);
            console.log("new width: " + newWidth);

            resizeMessage = resizeMessage.replace("{Sender_ID}", $("#senderId").val());
            resizeMessage = resizeMessage.replace("{Height}", newHeight);
            resizeMessage = resizeMessage.replace("{Width}", newWidth);

            window.parent.postMessage(resizeMessage, "*");
        }
    };
    </script>
</body>
</html>
