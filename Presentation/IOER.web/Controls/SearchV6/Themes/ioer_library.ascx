<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ioer_library.ascx.cs" Inherits="ILPathways.Controls.SearchV6.Themes.ioer_library" %>

<script type="text/javascript">
  var keywordSchemas = [ "gradeLevel", "learningResourceType", "mediaType", "k12Subject" ];
</script>
<style type="text/css">
  /* Search Header */
  #btnToggleFilters { background-color: <%=MainColorHex %>; color: #FFF; }
  #btnToggleFilters.expanded { background-color: #9984BD; }

  /* Filters */
  #filters #categories input { background-color: rgba(<%=MainColor.R %>,<%=MainColor.G %>,<%=MainColor.B %>, 0.9); color: #FFF; }
  #filters #categories input.selected { background-color: #9984BD; }
  #tags .tagList h2 { background-color: #4AA394; color: #FFF; }

  /* Paginators */
  .paginator input { background-color: <%=MainColorHex %>; color: #FFF; }
  .paginator input.current { background-color: #9984BD; }

  /* Results */
  .result.list .expandCollapseBox input { background-color: transparent; color: <%=MainColorHex %>; border: none; font-style: italic; }
  .result.list .expandCollapseBox input:hover, .result.list .expandCollapseBox input:focus { color: #4C98CC; }
  .theme .result.grid .paradata { background-image: linear-gradient(90deg, transparent 10%, rgba(<%=MainColor.R %>,<%=MainColor.G %>,<%=MainColor.B %>,0.8)); }
</style>

<div id="errorMessage" runat="server" visible="false" style="padding: 50px; text-align: center;"></div> 
<div id="mainLibraryHeader" class="mainLibraryHeader" runat="server">
  <script type="text/javascript">
    var pageData = <%=LibraryPageDataJSON %>;
  </script>
  <script type="text/javascript">
    /* Initialization */
    $(document).ready(function(){
      lib_engine("libColUpdated");
    });

  </script>
  <script type="text/javascript">
    /* Library Engine */
    function lib_engine(target){
      switch(target){
        case "libColUpdated":
          lib_renderLibColList();
          lib_selectTab("#libColDetails");
          determineSelectLibCol();
          break;
        default:
          break;
      }
    }
  </script>
  <script type="text/javascript">
    /* Page Functions */
    //Toggle the list of libcols
    function lib_toggleLibColList() {
      $("#libColSelector").toggleClass("expanded");
      $("#btnToggleLibColList").removeClass("expanded");
      if($("#libColSelector").hasClass("expanded")){
        $("#btnToggleLibColList").addClass("expanded");
      }
    }
    //Select a library or collection
    function lib_selectLibCol(libOrCol, target){
      $(".libColItem").removeClass("selected");
      $(".libColItem[data-type=" + libOrCol + "][data-id=" + target + "]").addClass("selected");
      lib_toggleLibColList();
    }
    //Auto-reselect the current libcol if available
    function determineSelectLibCol() {
      if($(".libColItem.selected").length == 0){
        lib_selectLibCol("library", pageData.CurrentLibrary.Id);
      }
      $("#libColSelector").removeClass("expanded");
      $("#btnToggleLibColList").removeClass("expanded");
    }
    //Select a tab within the current library/collection
    function lib_selectTab(target){
      $(".libColTab").removeClass("selected");
      $(target).addClass("selected");
    }
  </script>
  <script type="text/javascript">
    /* Rendering */
    //Render the library and collection list at the top
    function lib_renderLibColList() {
      var box = $("#libColSelector");
      var template = $("#template_libColItem").html();
      box.html("");
      box.append(renderLibColItem(pageData.CurrentLibrary, true, template));
      for(i in pageData.CurrentLibrary.Collections){
        box.append(renderLibColItem(pageData.CurrentLibrary.Collections[i], false, template));
      }
    }
    function renderLibColItem(item, isLibrary, template){
      return template.replace(/{libraryOrCollection}/g, isLibrary ? "library" : "collection").replace(/{Id}/g, item.Id).replace(/{ImageUrl}/g, item.ImageUrl).replace(/{Title}/g, item.Title);
    }
  </script>

  <style type="text/css">
    /* Big stuff and blending */
    #libColBox { background-color: #F5F5F5; border-radius: 5px 5px 0 0; border: 1px solid #CCC; border-bottom: none; }
    #searchHeader #btnToggleFilters { border-top-left-radius: 0; }
    #searchHeader #txtSearch { border-top-right-radius: 0; }

    /* LibColHeader */
    #libColHeader { position: relative; padding-top: 80px; }
    #btnToggleLibColList { border-radius: 5px 0 0 0; border: none; background-color: #3572B8; width: 251px; font-weight: bold; color: #FFF; height: 81px; font-size: 30px; margin: -1px 0 0 -1px; white-space: normal; transition: background-color 0.1s, color 0.1s; position: absolute; top: 0; left: 0; }
    #btnToggleLibColList.expanded { background-color: #9984BD; }
    #btnToggleLibColList:hover, #btnToggleLibColList:focus { background-color: #FF6A00; }
    #libColSelector { padding: 2px; position: absolute; top: 0; left: 250px; right: 0; height: 80px; z-index: 100; background-color: #DDD; }
    #libColHeader .libColItem { height: 0; border: 1px solid #CCC; border-width: 0 1px 0 1px; margin-bottom: -1px; position: relative; transition: box-shadow 0.2s, border 0.2s, height 0.5s, border 0.5s; overflow: hidden; background-color: #F5F5F5; }
    #libColHeader .libColItem .imageWrapper { width: 200px; height: 100%; background: #EEE url('') center center; background-size: cover; }
    #libColHeader .libColItem .libColTitle { line-height: 66px; font-size: 20px; padding: 5px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; width: calc(100% - 200px); }
    #libColHeader .libColItem .imageWrapper, #libColHeader .libColItem .libColTitle { display: inline-block; vertical-align: top; }
    #libColHeader .libColItem .type { position: absolute; top: 0; left: 200px; right: 0; height: 18px; padding: 0 5px; text-transform: capitalize; font-weight: normal; font-style: italic; line-height: 18px; color: #333; background-image: linear-gradient(90deg, #CCC, transparent); background-color: #EEE; }
    #libColHeader .libColItem:hover, #libColHeader #libColItem:focus { cursor: pointer; box-shadow: 0 0 10px -1px #FF6A00; border-color: #FF6A00; z-index: 10; }
    #libColHeader .libColItem.selected, #libColHeader #libColSelector.expanded .libColItem { height: 76px; border-width: 1px; }
  </style>

  <div id="libColBox">
    <div id="libColHeader">
      <input type="button" value="Library and Collections..." id="btnToggleLibColList" onclick="lib_toggleLibColList();" />
      <div id="libColSelector">

      </div>
    </div>
    <div id="libColContent">
      <div id="libColDetails" class="libColTab">
        Details
      </div>
      <div id="libColShareFollow" class="libColTab">
        Share and Follow
      </div>
      <div id="libColComments" class="libColTab">
        Comments
      </div>
      <div id="libColJoin" class="libColTab">
        Join
      </div>
      <div id="libColSettings" class="libColTab">
        Settings
      </div>
    </div>
  </div>

  <div id="libTemplates" style="display:none;">
    <div id="template_libColItem">
      <div class="libColItem" data-type="{libraryOrCollection}" data-id="{Id}" data-active="false" tabindex="0" onclick="lib_selectLibCol('{libraryOrCollection}',{Id});">
        <div class="imageWrapper" style="background-image:url('{ImageUrl}');"></div><!--
      --><h3 class="type">{libraryOrCollection}</h3><!--
      --><h2 class="libColTitle">{Title}</h2>
      </div>
    </div>
  </div>
</div>