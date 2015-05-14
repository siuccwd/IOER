<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Curriculum2.ascx.cs" Inherits="ILPathways.Controls.Curriculum.Curriculum2" %>

<script type="text/javascript">
  /* From Server */
  
</script>
<script type="text/javascript" src="/scripts/jscommon.js"></script>
<script type="text/javascript" src="/controls/curriculum/curriculum2.js"></script>
<link rel="stylesheet" type="text/css" href="/styles/common2.css" />
<link rel="stylesheet" type="text/css" href="/controls/curriculum/curriculum2.css" />

<div id="container" class="addThis">
  
  <!-- Only visible in create mode -->
  <div id="createMode" runat="server">
    <h1 class="isleH1">IOER Curriculum Builder <input type="button" id="btnHelp" class="isleButton bgBlue" value="Help" onlick="toggleHelp();" /></h1>
    <div class="columnContainer pageColumnContainer createMode">
      <div id="details" class="pageColumn grayBox">
        <h2 class="header">Node Details</h2>
        <div id="curriculumDetails" class="showing">
          <h2 class="mid">Basic Info</h2>
          <input type="text" id="txtMainTitle" placeholder="Curriculum Title" />
          <select id="ddlMainSubject">
            <option value="0">Select a Subject...</option>
            <% foreach(var item in k12Subjects) { %>
              <option value="<%=item.Id %>"><%=item.Title %></option>
            <% } %>
          </select>
          <textarea id="txtMainDescription" placeholder="Curriculum Description"></textarea>
          <h2 class="mid">Grade Levels</h2>
          <div class="inputList">
          <%  foreach(var item in gradeLevels) {%>
                <label><input type="checkbox" name="cbxlMainGradeLevels" id="cbx_<%=item.Id %>" value="<%=item.Id %>" /> <%=item.Title %></label>
          <%} %>
          </div>

          <asp:Panel ID="orgsPanel" runat="server" >
            <h2 class="mid">Content Owner</h2>
          <div class="inputList">
              <select id="ddlOrgs">
          <%  foreach(var item in orgs) {%>
                <option value="<%=item.Id %>"><%=item.Title %></option>
          <%} %>
              </select>
          </div>
        </asp:Panel>
        </div>
        <div id="nodeDetails" class="">
          <h2 class="mid">Basic Info</h2>
          <input type="text" id="txtNodeTitle" placeholder="Node Title" />
          <textarea id="txtNodeDescription" placeholder="Node Description"></textarea>
          <h2 class="mid">Attachments</h2>
          <div class="lightbox">
            <input type="text" id="txtFileTitle" placeholder="Title to display for this file" />
            <input type="file" id="fileNodeAttachment" class="" />
            <input type="button" id="btnUploadAttachment" value="Attach a File to this Node" class="isleButton bgBlue" />
          </div>
          <h2 class="mid">Resources</h2>
          <input type="button" class="isleButton bgBlue" value="Find Resources for this Node" />
          <h2 class="mid">Standards</h2>
          <input type="button" class="isleButton bgBlue" value="Add Standards to this Node" />
        </div>
      </div><!-- /details column -->
      <div id="contents" class="pageColumn">
        <div id="nodes"></div>
        <div id="addNode" class="node create grayBox" data-nodeID="0" data-parentID="0">
          <div class="notation">Add a Node</div>
          <input type="text" class="nodeTitle" id="txtAddNode" placeholder="Node Title" />
          <div class="nodeButtons columnContainer">
            <input type="button" class="nodeButton nodeAdd column isleButton bgBlue" value="Add" title="Add this Node" onclick="saveNode(0);" />
          </div>
        </div>
      </div><!-- /curriculum contents -->
    </div>
  </div><!-- /Create Mode tools -->

  <!-- Only visible in view mode -->
  <div id="viewMode" runat="server">
    <h1 class="isleH1">Curriculum Title</h1>
    <div  class="columnContainer pageColumnContainer viewMode">
      <div id="details" class="pageColumn grayBox">
        <h2 class="header">Details</h2>
        Some content here
      </div><!-- /details column -->
      <div id="contents" class=pageColumn">
        Some Content here
      </div><!-- /curriculum contents -->
    </div>
  </div><!-- /View Mode tools -->

</div><!-- /container -->

<div id="templates" style="display:none;">
  <div id="template_create_nodeList_node">
    <div class="node create grayBox" data-nodeID="{id}" data-parentID="{parentID}" data-depth="{depth}" style="margin-left:{indent}px;">
      <div class="notation">{notation}</div>
      <div class="nodeTitle">{title}</div>
      <div class="nodeButtons columnContainer">
        <input type="button" class="nodeButton nodeInfo column isleButton bgBlue" value="Edit" title="Node Info" onclick="showNodeInfo({id}, {depth});" />
      </div>
    </div>
  </div>
</div>