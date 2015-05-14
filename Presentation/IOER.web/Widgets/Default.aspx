<%@ Page Title="" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ILPathways.Widgets.Default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
  <link rel="stylesheet" href="/styles/common2.css" type="text/css" />
  <link rel="stylesheet" href="/styles/tooltipv2.css" type="text/css" />
  
  <script type="text/javascript" src="/scripts/tooltipv2.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
  <%-- 
  <script type="text/javascript">
    var standardsBrowserOptions = { body: null, grade: null, helperUrl: null };
    var searchOptions = { button: null, css: null }
    $(document).ready(function () {
      setupSBconfig();
      setupSearchConfig();

      updateStandardsBrowserOptions();
      updateSearchOptions();
    });

    //Standards Browser
    function setupSBconfig() {
      $("#SBddlBody").on("change", function () {
        standardsBrowserOptions.body = getDDLValue(this);
        updateStandardsBrowserOptions();
      });
      $("#SBddlGrade").on("change", function () {
        standardsBrowserOptions.grade = getDDLValue(this);
        updateStandardsBrowserOptions();
      });
      $("#SBtxtHelperURL").on("change keyup", function () {
        standardsBrowserOptions.helperUrl = getTextValue(this);
        updateStandardsBrowserOptions();
      });
    }
    function updateStandardsBrowserOptions() {
      $("#SBtxtCopyMe").val("<iframe src=\"http://ioer.ilsharedlearning.org/widgets/standards/" + getParams(standardsBrowserOptions) + "\"></iframe>");
    }

    //Search
    function setupSearchConfig() {
      $("#searchUseButton").on("change", function () {
        searchOptions.button = $(this).prop("checked") ? null : "no";
        updateSearchOptions();
      });
      $("#searchTxtCSS").on("change keyup", function () {
        searchOptions.css = getTextValue(this);
        updateSearchOptions();
      });
    }
    function updateSearchOptions() {
      $("#searchTxtCopyMe").val("<iframe src=\"http://ioer.ilsharedlearning.org/widgets/search/" + getParams(searchOptions) + "\"></iframe>");
    }

    //Utility
    function getTextValue(input) {
      var value = $(input).val();
      return value == "" ? null : value;
    }
    function getDDLValue(input) {
      var value = $(input).find("option:selected").attr("value");
      return value == "none" ? null : value;
    }
    function getParams(input) {
      var params = "?";
      for (i in input) {
        if (input[i] != null) {
          params += i + "=" + input[i] + "&";
        }
      }
      if (params == "?") { params = ""; }
      if(params.length > 0) { params = params.substring(0, params.length - 1); }

      return params;
    }
  </script>

  <style type="text/css">
    #intro { max-width: 1100px; margin: 5px auto; }
    code { font-family: Consolas, 'Courier New', monospace; background-color: #EEE; padding: 2px 5px; border-radius: 5px; display: inline-block; }
    #content h3 { margin-top: 5px; }
    #content #items { max-width: 1000px; margin: 0 auto; }
    #content h2 { margin: 30px 0 10px 0; font-size: 120%; }
    .copyMe { width: 100%; display: block; }

    .configOptions { margin: 5px 0; text-align: center; white-space: nowrap; }
    .optionItem { display: inline-block; margin-right: -2px; vertical-align: top; text-align: left; }

    #SBconfigOptions .optionItem { width: 33.33332%; }
    #SBconfigOptions #helperURL { padding-right: 25px; }
    #SBconfigOptions #helperURL input { width: 100%; height: 25px; }

    #searchConfigOptions .optionItem { width: 33%; }
    #searchConfigOptions #searchCSS { padding-right: 25px; }
    #searchConfigOptions #searchCSS input { width: 100%; height: 25px; }

    @media screen and (max-width:600px) {
      #content .configOptions .optionItem { width: 100%; display: block; margin-bottom: 5px; }
    }
  </style>

  <div id="content">
    
    <h1 class="isleH1">IOER Widgets</h1>
    <div id="intro">
      <p>IOER widgets load as iframe elements on your website. Some widgets have other options, which you can configure on this page.</p>
    </div>

    <div id="items">
      <h2>Library and Collections</h2>
      <p>The Library and Collections widget configuration is Library-specific, so to setup a Library widget, please <a class="textLink" href="/Libraries/Default.aspx">visit the desired Library</a> and configure the widget from its Share tab.</p>

      <h2>Events Calendar</h2>
      <p>The Events Calendar widget configuration is highly specialized, so to setup a Calendar widget, please <a class="textLink" href="http://apps.il-work-net.com/Calendar/Public/Events/CodeGen.aspx">visit its configuration page</a>.</p>

      <h2>Curriculum</h2>
      <p>The Curriculum widget must be setup from the Curriculum's page. Please visit the Curriculum (for example, the <a href="http://ioer.ilsharedlearning.org/curriculum/2197" target="_blank">Health Science Curriculum</a>) you wish to share as a widget and copy the code from there.</p>

      <h2>Standards Browser</h2>
      <p>Copy and paste the text from this field directly into your site. Optionally, you can pre-select some items below.</p>
      <input type="text" readonly="readonly" class="copyMe" id="SBtxtCopyMe" onclick="this.select();" />
      <div id="SBconfigOptions" class="configOptions">
        <select id="SBddlBody" class="optionItem">
          <option value="none">No preselected Standard Body</option>
          <option value="math">Common Core Math Standards</option>
          <option value="ela">Common Core ELA/Literacy Standards</option>
          <option value="ngss">Next Generation Science Standards</option>
          <option value="ilfinearts">Illinois Fine Arts Standards</option>
          <option value="ilphysicaldevelopment">Illinois Physical Development and Health Standards</option>
          <option value="ilsocialscience">Illinois Social Science Standards</option>
        </select>
        <select id="SBddlGrade" class="optionItem">
          <option value="none">No preselected Grade Level</option>
          <option value="K">Kindergarten</option>
          <option value="1">1st Grade</option>
          <option value="2">2nd Grade</option>
          <option value="3">3rd Grade</option>
          <option value="4">4th Grade</option>
          <option value="5">5th Grade</option>
          <option value="6">6th Grade</option>
          <option value="7">7th Grade</option>
          <option value="8">8th Grade</option>
          <option value="9">9th Grade</option>
          <option value="10">10th Grade</option>
          <option value="11">11th Grade</option>
          <option value="12">12th Grade</option>
        </select>
        <div class="optionItem" id="helperURL">
          <input type="text" id="SBtxtHelperURL" placeholder="Helper URL" /> <a class="toolTipLink" title="Helper URL|<b>For advanced configurations only!</b>|Most users will not need this.|Helper URL should point to a page on your site that can detect changes in its URL.|When the height of the Standards Browser changes, the URL you enter here will be updated with an appropriate 'height' parameter."></a>
        </div>
      </div>

      <h2>Search</h2>
      <p>Copy and paste the text from this field directly into your site. Optionally, you can pre-select some items below.</p>
      <input type="text" readonly="readonly" class="copyMe" id="searchTxtCopyMe" onclick="this.select();" />
      <div id="searchConfigOptions" class="configOptions">
        <label for="searchUseButton" class="optionItem">
          <input type="checkbox" id="searchUseButton" checked="checked" /> Include "Search" button
        </label>
        <div class="optionItem" id="searchCSS">
          <input type="text" id="searchTxtCSS" placeholder="Style Sheet URL" /> <a class="toolTipLink" title="Style Sheet URL|This widget can load a custom style sheet to better match the look and feel of your site."></a>
        </div>
      </div>

      <h2 id="tips">Tips for styling widgets</h2>
      <h3>Size</h3>
      <p>Iframes typically default to 300x200 pixels wide--not very big. To change this, simply add some <a href="https://developer.mozilla.org/en-US/docs/Web/CSS" target="_blank">CSS</a>.</p>
      <p>For example, use this in a style sheet: <code>iframe { width: 100%; height: 500px; }</code></p>
      <p>Or this in the iframe tag itself: <code>style="width:100%; height:500px;"</code></p>
      <p>This will result in a widget that is as wide as its containing element and 500 pixels tall.</p>

      <h3>Borders</h3>
      <p>Most iframes also have a default border. To change it, use <a href="https://developer.mozilla.org/en-US/docs/Web/CSS" target="_blank">CSS</a>:</p>
      <p>In a style sheet: <code>iframe { border: 1px solid #CCC; }</code></p>
      <p>Or in the iframe tag itself: <code>style="border: 1px solid #CCC;"</code></p>      
      <p>To remove the borders altogether, use: <code>iframe { border: none; }</code> or <code>style="border: none;"</code></p>
      
      <h3>Examples</h3>
      <p>For a borderless 200 pixel wide search widget like this:</p>
      <iframe style="border: none; width: 200px; height: 1.5em;" src="http://ioer.ilsharedlearning.org/widgets/search/"></iframe>
      <p>Use a style sheet:</p>
      <code>
        &lt;style type="text/css"&gt;<br />
        &nbsp;&nbsp;&nbsp;&nbsp;iframe { border: none; width: 200px; height: 1.5em; }<br />
        &lt;/style&gt;<br />
        <br />
        &lt;iframe esrc="http://ioer.ilsharedlearning.org/widgets/search/"&gt;&lt;/iframe&gt;
      </code>
      <p>Or inline styles:</p>
      <code>&lt;iframe style="border:none; width:200px; height:1.5em;" src="http://ioer.ilsharedlearning.org/widgets/search/"&gt;&lt;/iframe&gt;</code>

      <h3>More Examples</h3>
      <p>Check out the <a href="/Pages/SamplePage.aspx">Sample Page</a> to see more IOER widgets in action.</p>
    </div>
    --%>
  
  <div id="content">
    <h1 class="isleH1">IOER Widgets</h1>

    <style type="text/css">
      #widgetsFrame { border: none; width: 100%; }
    </style>
    <script type="text/javascript">
      $(document).ready(function () {
        $(window).on("message", function (msg) {
          var message = $.parseJSON(msg.originalEvent.data);
          console.log(message.action);
          if (message.action == "resize") {
            console.log(message.height);
            $("#widgetsFrame").attr("style", "height: " + message.height);
          }
        });
      });
    </script>
    <iframe src="https://apps.il-work-net.com/worknetminiapps/widgets" id="widgetsFrame"></iframe>

  </div>


</asp:Content>
