﻿<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="OrgSearch.ascx.cs" Inherits="IOER.Controls.OrgMgmt.OrgSearch" %>



<script type="text/javascript">
    //From server
    <%=filters %>
    <%=userGUID %>
    
</script>
<script type="text/javascript" src="/Scripts/OrgSearch.js"></script>

<link type="text/css" rel="stylesheet" href="/Styles/common2.css" />
<style type="text/css">

  /* Big Stuff */
  * { box-sizing: border-box; -moz-box-sizing: border-box; }
  #content { transition: padding 1s; -webkit-transition: padding 1s; min-width: 300px; }

  /* Page-specific stuff */
  #columns { font-size: 0; padding: 0 5px 10px 5px; }
  .column { display: inline-block; font-size: 16px; vertical-align: top; margin-bottom: 10px; }
  #leftColumn { width: 200px; }
  #rightColumn { width: calc(100% - 200px); padding: 0 0 0 5px; }
  #txtSearch { width: 100%; font-size: 26px; }
  #resultsCount { text-align: center; }
  #filterLists { margin-bottom: 15px; }
  #filters input[type=button] { margin-bottom: 5px; }
  #filterToggler { background-color: #3572B8; color: #FFF; border-radius: 5px; padding: 5px; text-align: center; display: block; font-weight: bold; display: none; margin-bottom: 8px; }
  #filterToggler:hover, #filterToggler:focus { background-color: #FF6A00; }
  label { display: block; position: relative; padding: 2px; border-radius: 5px; }
  label:hover, label:focus { cursor: pointer; background-color: #DDD; }
  .tip { font-style: italic; color: #555; margin-bottom: 1px; }
  #searchParameters { font-size: 0; padding: 5px 0; max-width: 800px; }
  #searchType { font-size: 0; }
  #searchType label { display: inline-block; vertical-align: top; width: 50%; font-size: 16px; }
  #searchType, #ddlSortingOptions { display: inline-block; vertical-align: top; width: 50%; }

  /* Search Results stuff */
  #resultsList { opacity: 1; transition: opacity 0.5s; -webkit-transition: opacity 0.5s; }
  #resultsList.hidden { opacity: 0; }
  .result { margin-bottom: 50px; box-shadow: 0 10px 80px -30px #CCC inset; padding: 5px; border-radius: 5px; font-size: 0; }
  .result .fixedThumb, .result .stretchyThumb, .result .data, .result .collections, .result .collections .fixedCollectionThumb, .result .collections p { display: inline-block; vertical-align: top; font-size: 16px; }
  .result .fixedThumb { width: 150px; height: 150px; background-position: center center; background-size: contain; border-radius: 5px; background-repeat: no-repeat; display: none; }
  .result .stretchyThumb { width: 150px; max-height: 200px; overflow: hidden; border-radius: 5px; }
  .result .stretchyThumb img { width: 100%; border-radius: 5px; }
  .result .data { width: calc(100% - 150px); padding: 0 5px; }
  .result .data h2 { font-size: 20px; }
  .result .data h2 a { font-size: inherit; }
  .result .collections { width: 100%; height: 125px; min-height: 70px; overflow-x: auto; overflow-y: hidden; white-space: nowrap; padding: 2px 5px; box-shadow: 0 0 80px -30px #CCC inset; border-radius: 5px; }
  .result .collections .collection { font-size: 0; background-color: #EEE; border-radius: 5px; padding: 2px; width: 250px; display: inline-block; vertical-align: top; height: 105px; overflow: hidden;  }
  .result .collections .fixedCollectionThumb { width: 100px; height: 100px; border-radius: 5px; background-position: center center; background-size: cover; vertical-align: middle; }
  .result .collections .collection p { padding-left: 5px; width: calc(100% - 100px); margin: 0; vertical-align: middle; white-space: normal; }

  .result .collectionsTitle { font-style: italic; padding-left: 10px; color: #555; }

  /* Fancy tricks */
  #widthMode { width: 100%; }
  #filtersBox { max-height: 700px; transition: max-height 1s; -webkit-transition: max-height 1s; overflow: hidden; }
  #filtersBox.hidden { max-height: 68px; }

  @media screen and (min-width: 980px){
    #content { padding-left: 50px; }
  }
  @media screen and (max-width: 950px){
    #searchType { display: block; width: 100%; }
    #ddlSortingOptions { display: block; width: 100%; }
  }
  @media screen and (max-width: 950px){
    .result .data { width: calc(100% - 150px); }
  }
  @media screen and (max-width: 600px){
    #widthMode { width: 600px; }

    .column { display: block; }
    #leftColumn, #rightColumn { width: 100%; }
    #filterToggler { display: block; }
  }
  @media screen and (max-width: 450px){
    #searchType label { display: block; width: 100%; }
    .result .data { width: calc(100% - 75px); }
    .result .fixedThumb, .result .stretchyThumb { width: 75px; max-height: 100px; }
  }

</style>

<div id="content">
  <h1 class="isleH1">IOER Organization Search</h1>

  <div id="columns">
    <!-- Left Column -->
    <div class="column" id="leftColumn">

      <div class="grayBox" id="filtersBox">
        <h2 class="header">Filters...</h2>
        <a href="#" onclick="showHideFilters(); return false;" id="filterToggler">Show/Hide Filters</a>
        <div id="filters">
          <div id="filterLists">

          </div>
          <input type="button" id="btnClearFilters" onclick="clearFilters()" class="isleButton bgBlue shaded" value="Clear Filters" />
          <input type="button" id="btnResetSearch" onclick="resetSearch()" class="isleButton bgBlue shaded" value="Reset Search" />
        </div>
      </div>
    </div><!-- /leftColumn -->

    <!-- Right Column -->
    <div class="column" id="rightColumn">

      <!-- Contains search bar, buttons, etc -->
      <div id="searchBox">
        <input type="text" id="txtSearch" class="txtSearch" placeholder="Search for Organizations ..." />
        <div id="searchParameters">

          <select id="ddlSortingOptions">
            <option value="">No special Sorting</option>
            <option value="organization|asc" selected="selected">Organization Name A-Z</option>
            <option value="organization|desc">Organization Name Z-A</option>
            <option value="updated|desc" >Most Recently Updated</option>
            <option value="updated|asc">Least Recently Updated</option>
          </select>
        </div>
        <div id="resultsCount"></div>
      </div>

      <!-- Contains paginators and results list -->
      <div id="resultsBox">
        <div class="paginator" id="topPaginator">

        </div>
        <div id="resultsList" class="hidden">

        </div>
        <div class="paginator" id="Div1">

        </div>
      </div>

    </div><!-- /rightColumn -->
  </div>

  <div id="templates" style="display:none;">
    <div id="template_cbx">
      <label for="{id}"><input type="checkbox" id="{id}" name="{name}" value="{value}" data-listID="{listID}" data-itemID="{itemID}" /> {text}</label>
    </div>
    <div id="template_rb">
      <label for="{id}"><input type="radio" id="Radio1" name="{name}" value="{value}" data-listID="{listID}" data-itemID="{itemID}" /> {text}</label>
    </div>
    <div id="template_searchResult">
      <div class="result" data-orgID="{orgID}">
        <a class="fixedThumb" style="background-image:url('{iconURL}');" href="{orgUrl}"></a>
        <a class="stretchyThumb" href="{orgUrl}"><img src="{iconURL}" /></a>
        <div class="data">
          <h2><a href="{orgUrl}">{title}</a></h2>
            <div class="orgTitle"><br />City: {city}</div>
        </div>

      </div>
    </div>

  </div>
  <div id="mediaQueryJSInterface" style="display:none">
    <div id="widthMode"></div>
  </div>
</div>

<asp:Panel ID="hiddenStuff" runat="server" Visible="false">


</asp:Panel>