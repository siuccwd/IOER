<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SearchV5.ascx.cs" Inherits="IOER.Controls.SearchV5.SearchV5" %>

<link rel="stylesheet" type="text/css" href="/Styles/common2.css" />
<link rel="stylesheet" type="text/css" href="/controls/searchv5/searchv5.css" />
<script type="text/javascript">
  //Server variables
  var apiRoot = "";
  var filters = <%=filtersJSON %>;

</script>
<script type="text/javascript" src="/scripts/jscommon.js"></script>
<script type="text/javascript" src="/controls/searchv5/searchv5.js"></script>

<div id="container" class="addThis">

  <!-- Header -->
  <h2 class="isleH1">ISLE Open Education Resources Search</h2>

  <div id="searchHeader">
    <input type="text" id="txtSearch" class="txtSearch" runat="server" placeholder="Start typing to Search..." />
    <input type="button" id="btnClearTxtSearch" onclick="clearTxtSearch()" value="X" />
    <div id="selectedTags"></div>
    <div id="headerTools" class="iBlockContainer">
      <input type="button" id="btnToggleFilters" onclick="toggleFilters();" value="Filters..." class="isleButton bgBlue iBlockItem" />
      <select id="ddlSort" class="iBlockItem">
        <option value="relevancy">Relevancy</option>
        <option value="newest" selected="selected">Newest First</option>
        <option value="oldest">Oldest First</option>
        <option value="liked">Most Liked</option>
        <option value="disliked">Most Disliked</option>
        <option value="rated">Best Evaluation Scores</option>
        <option value="viewed">Most Visited</option>
        <option value="favorited">In The Most Libraries</option>
        <option value="comments">Most Commented On</option>
      </select>
      <select id="ddlPageSize" class="iBlockItem">
        <option value="20" selected="selected">Show 20 Items</option>
        <option value="50">Show 50 Items</option>
        <option value="100">Show 100 Items</option>
      </select>
      <select id="ddlViewMode" class="iBlockItem">
        <option value="list" selected="selected">List View</option>
        <option value="grid">Grid View</option>
        <option value="text">Text-Only View</option>
      </select>
    </div>
    <div id="filters" class="grayBox"></div>
    <div id="tags">
      <div class="filter grayBox slim" data-filterID="sb" data-filtername="standardsBrowser" data-showing="Standards Browser">
        <h2 class="header">Standards Browser</h2>
        [Browser goes here]
      </div>
      <div class="filter grayBox slim" data-filterID="tips" data-filtername="searchTips" data-showing="Search Tips">
        <h2 class="header">Search Tips</h2>
        [Search tips go here]
      </div>
    </div>
    <div id="searchStatus"></div>
    <div class="paginator grayBox"></div>
    <div id="searchResults"></div>
    <div class="paginator grayBox"></div>
  </div>

  <div id="templates" style="display:none;">
    <!-- Tags and filters -->
    <div id="template_tagList">
      <div class="filter grayBox inputList slim" data-filterID="{id}" data-filtername="{name}" data-filteres="{esID}">
        <h2 class="header">{title}</h2>
        {insertTags}
      </div>
    </div>
    <div id="template_tag">
      <label data-tagID="{id}"><input type="checkbox" data-tagID="{id}" /> {title}</label>
    </div>
    <div id="template_filterButton">
      <input type="button" onclick="showHideFilter({id})" value="{title}" class="isleButton btnFlat bgBlue" />
    </div>

    <!-- Search results -->
    <div id="template_result_list">
      <div class="result list collapsed" data-resourceID="{resourceID}" data-versionID="{versionID}" data-elasticID="{elasticID}">
        <div class="column text">
          <div class="titleBox">
            <a class="title" href="/Resource/{resourceID}/{shortTitle}" target="_resultWindow">{title}</a>
            <input type="button" class="creator linkButton" onclick="searchMe('{creator}');" value="{creator}" />
          </div>
          <div class="libraryControls"></div>
          <p class="description" data-resourceID="{resourceID}">{description}</p>
          <input type="button" class="linkButton expandCollapse" value="More" onclick="expandCollapse({resourceID}, this);" />
          <div class="standards">{standards}</div>
        </div>
        <div class="column metadata">
          {metadata}
        </div>
        <div class="column thumbnail">
          {thumbnail}
          <p class="created">Created {created}</p>
        </div>
      </div>
    </div>
    <div id="template_result_grid">
      <a class="result grid" href="/Resource/{resourceID}/{shortTitle}" target="_resultWindow" data-resourceID="{resourceID}" data-versionID="{versionID}" data-elasticID="{elasticID}">
        {thumbnail}
        <p>{title}</p>
      </a>
    </div>
    <div id="template_result_text">
      <div class="result text" data-resourceID="{resourceID}" data-versionID="{versionID}" data-elasticID="{elasticID}">
        <a href="/Resource/{resourceID}/{shortTitle}" target="_resultWindow">{title}</a>
        <p class="description">{description}</p>
      </div>
    </div>
    <div id="template_thumbnail">
      <div class="thumbnail" style="background-image: url('//ioer.ilsharedlearning.org/OERThumbs/large/{resourceID}-large.png')">
        <img class="placeholder" src="/Images/ThumbnailResizer.png" />
        <div class="overlayText"></div>
        <div class="paradata">{paradata}</div>
      </div>
    </div>
    <div id="template_paginatorButton">
      <input type="button" class="isleButton bgBlue {current}" value="{page}" onclick="paginate({page})" />
    </div>
    <div id="template_paradataIcon">
      <div class="paradataIcon">
        <img src="{icon}" alt="{text}" />
        <div>{numbers}</div>
      </div>
    </div>
  </div><!-- /templates -->
</div> <!-- /container -->