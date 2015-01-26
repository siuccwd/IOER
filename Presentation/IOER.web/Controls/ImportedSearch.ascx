<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ImportedSearch.ascx.cs" Inherits="ILPathways.Controls.ImportedSearch" %>

<script type="text/javascript">
  function renderSelectedTags() { }
</script>
<style>
  #content, #libraryHeader {
    transition: padding 1s;
    -webkit-transition: padding 1s;
    min-width: 300px;
  }
  #libraryHeader { 
    padding-right: 5px;
  }
  #libraryHeader #libColTitle { padding-bottom: 5px; }
  #libraryHeader h2 {
    font-size: initial;
    line-height: initial;
    padding: initial;
    margin: initial;
    border-radius: initial;
  }

  #navbar .topNav h2 { line-height: initial; }
  @media screen and (min-width: 980px) {
    #content {
      padding-left: 25px;
    }
    #libraryHeader { 
      padding-left: 55px;
    }
  }
</style>
<div id="container">
  <div id="importedContent" runat="server" class="importedContent"></div>
</div>
<script type="text/javascript">
  var searchHost = "http://localhost:2013";
  $(".importedContent script, .importedContent link").each(function () {
    var box = $(this);
    var parent = box.parent();
    if (box.attr("src")) {
      box.attr("src", searchHost + box.attr("src"));
      box.remove();
      parent.append(box);
    }
    if (box.attr("href")) {
      box.attr("href", searchHost + box.attr("href"));
    }
  });
  $(document).ready(function () {
    setTimeout(function () {
      ajaxGet = function (parameters, success) {
        var params = "?";
        for (i in parameters) {
          params += "&" + encodeURI(i) + "=" + encodeURI(parameters[i]);
        }
        params = params.replace("?&", "?");

        if (activeRequest) {
          activeRequest.abort();
        }
        activeRequest = $.get( searchHost + "/api/search/" + siteID + params, success);
        console.log(activeRequest);
      };
      doSearch();
    }, 1000);
  });
  function updateNarrowing() {
    //Add Library/Collection Filters if available
    if ($("#libraryHeader").length > 0) {
      var active = getActive();
      var temp = { field: "libraryIDs", title: "Library", es: "libraryIDs", items: [] };
      temp.items.push({ text: libraryData.library.title, id: libraryData.library.id });
      activeFiltering.idFilters.push(temp);

      if (!active.isLibrary) {
        var temp2 = { field: "collectionIDs", title: "Collection", es: "collectionIDs", items: [] };
        temp2.items.push({ text: active.data.title, id: active.data.id });
        activeFiltering.idFilters.push(temp2);
      }
    }

  } //placeholder

</script>