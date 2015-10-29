<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="OrganizationManagement.ascx.cs" Inherits="IOER.Organizations.OrganizationManagement" %>

<%@ Register TagPrefix="uc1" TagName="UserManager" Src="/Controls/ManageUsers.ascx" %>

<% var baseURL = "/organizations/?organizationID="; %>

 <link rel="stylesheet" type="text/css" href="/styles/common2.css" />

<script type="text/javascript">
    /* --- Initialization --- */
    $(document).ready(function () {
			setupOrganizationSelectorDDL();
			$("form").removeAttr("onsubmit");
    });

    function setupOrganizationSelectorDDL() {
      $("#ddlMyOrganizations").on("change", function () {
          window.location.href = "<%=baseURL %>" + $(this).find("option:selected").attr("value");
			});
    }

    function addOrg() {
        window.location.href = "/organizations/?action=new";
    }

    function saveOrg() {
        //alert('saveOrg');
        return true;
    }
    //function timeline(id) {
    //    window.location.href = "/org/" + id + "/timeline";
    //}
    function doConfirmation() {

        if (confirm("Are you sure that you want to remove this organization? \r\nIt will be set inactive. As well libraries, and related components will no longer be available."))
        {
            showSection("delOrg");
            return true;
        } else 
            return false;
    }
</script>
<script type="text/javascript">
    /* --- Page Functions --- */
    //Show a section
    function showSection(section) {
        $("#sectionSelector input").removeClass("selected").filter("[data-sectionID=" + section + "]").addClass("selected");
        $(".section").removeClass("selected").filter("[data-sectionID=" + section + "]").addClass("selected");
        $("#mode").val(section);
    }

    //new org
    function setNewOrg() {
        showSection('newOrg');
        $("#CurrentOrgId").val("-1");
    }
</script>
<style type="text/css">
	/* Big Stuff */
	#content { min-height: 500px; }
	#leftColumn, #rightColumn { display: inline-block; vertical-align: top; }
	#leftColumn { width: 275px; }
	#rightColumn { width: calc(100% - 275px); }
	input[type=text], select, textarea { width: 100%; }
	input[type=checkbox]:hover, input[type=checkbox]:focus, input[type=radio]:hover, input[type=radio]:focus { cursor: pointer; }
	
	/* Left Column */
	.grayBox { padding: 5px; }
	.grayBox .header { margin: -5px -5px 5px -5px; }
	#sectionSelector select { width: 100%; border-radius: 5px 5px 0 0; }
	#sectionSelector select.round { border-radius: 5px; }
	#sectionSelector input { border-radius: 0; border-width: 1px; }
    #sectionSelector a { display: block; width: calc(100% - 1px);margin-left: 1px; text-align: center; border-radius: 0; border-width: 1px;}

	#sectionSelector input:last-child { border-radius: 0 0 5px 5px; }
	#sectionSelector input:disabled { display: none; }
	#sectionSelector input.selected { background-color: #9984BD; }

	/* Right Column */
	#rightColumn { padding-left: 10px; }
	.section h2 { font-size: 20px; border-left: 15px solid #4F4E4F; padding: 2px 5px; margin-bottom: 10px; }
	.section { display: none; }
	.section.selected { display: block; }

	/* Section stuff */
	.section .item { margin-bottom: 15px; }
	.section .item label { width: 100%; display: block; border-radius: 5px; transition: background 0.2s; }
	.section .item label:hover, .section .item label:focus { background-color: #EEE; cursor: pointer; }
	.section .item span, .section .item input, .section .item select { display: inline-block; vertical-align: top; min-height: 25px; }
	.section .item span { width: 25%; text-align: right; padding: 2px 10px 0 0; }
	.section .item span.data { width: 75%; text-align: left; }
	.section .item:not(.cbx) input, .section .item select { width: 75%; }
	.section .item.split span.mainLabel { width: 25%; }
	.section .item.split span.extraLabel { width: 10%; padding: 2px 5px 0 5px; }
	.section .item.split input { width: 32.5%; }
	.section .item .tip { padding-left: 25%; }
	.section .required span:first-of-type { color: #D22; }

    .buttons .btnAction {width: 45%; margin-left: 10px;}

    .libLink { padding: 0 0 5px 5px;}
    .optionLinks {width: 255px;}
    .optionLinks a { margin-bottom: 1px; background-color: #3572B8; color: #FFF; display: block; 
                     text-decoration: none; font-size: 1.2em; text-align: center; border-radius: 5px;}
    .optionLinks a:hover, .optionLinks a:focus { margin-bottom: 1px; background-color: #3572B8; color: #FFF; display: block; text-decoration: none; }

	@media (max-width: 850px) {
		.section .item:not(.cbx) span:first-child, .section .item:not(.cbx):not(.split) input, .section .item select { display: block; width: 100%; text-align: left; }
		.section .item.cbx span, .section .item.cbx input { width: auto;  text-align: right; padding-left: 10px; }
		.section .item.cbx input { float: left; margin-left: 10px; }
		.section .item .tip { padding-left: 0; }
		.section .item.split input { width: 42.5%; }
		.section .item.split span.extraLabel { width: 15%; }
	}

	/* Default */
	.section[data-sectionID=default] a { display: block; margin-bottom: 2px; }

	/* Properties */
	.section[data-sectionID=properties] { max-width: 800px; }

	/* Users */
	.userManagerContainer .umTabs input[data-panel=pending] { display: none; }
	.userManagerContainer .umTabs input[data-panel=invite] { border-radius: 0 5px 5px 0; }
	.userManagerContainer .umTabs input { width: 33%; }

	/* Libraries and Learning Lists */
	.manageBox { margin-bottom: 10px; padding: 5px 165px 5px 5px; min-height: 160px; position: relative; }
	.manageBox .title { font-size: 22px; font-weight: bold; display: block; padding: 2px 5px; }
	.manageBox .orgAvatar { position: absolute; width: 150px; height: 150px; top: 5px; right: 5px; background: center center no-repeat; background-size: contain; border-radius: 5px; background-color: #CCC; border: 1px solid #CCC; }
	.manageBox .access { font-style: italic; padding: 5px; text-align: right; }
	.manageBox .manageLinks { padding: 2px 5px; }
	.manageBox .manageLinks a { display: inline-block; }
	.manageBox p { margin: 0; padding: 5px; }

	/* Libraries */

	/* Learning Lists */

	/* Responsive */
	@media (max-width: 900px) {
		#umSelectedUser #umSelectedUser_properties, #umSelectedUser #umSelectedUser_buttons { display: block; width: 100%; }
	}
	@media (max-width: 850px) {
		.manageBox .manageLinks .divider { display: none; }
		.manageBox .manageLinks a { display: block; }
	}
	@media ( max-width: 750px) {
		#leftColumn, #rightColumn { display: block; width: 100%; margin-bottom: 50px; }
	}
	@media (max-width: 575px) {
		.userManagerPanel #umSelectedUser label { display: block; width: 100%; }
		.userManagerPanel #umSelectedUser_avatar { width: 75px; height: 75px; }
		.userManagerPanel #umSelectedUser { padding-right: 85px; }
		#umSelectedUser #umSelectedUser_buttons { text-align: center; }
		#umSelectedUser #umSelectedUser_buttons input { display: inline-block; vertical-align: top; width: 48%; }
	}
	@media (max-width: 400px) {
		#umSelectedUser #umSelectedUser_buttons input { display: block; width: 100%; }
	}

</style>
<style type="text/css">
	/* Styles for user management widget */
	@media (min-width: 1500px) {
		.userList .umUser.umExisting { width: 31.3333%; margin-left: 1%; margin-right: 1%; }
	}
	@media (max-width: 1499px) and (min-width: 949px) {
		.userList .umUser.umExisting { width: 49.5%; }
		.userList .umUser.umExisting:nth-child(2n+2) { margin-left: 1%; }
	}
	@media (max-width: 950px) {
		.userList .umUser.umExisting { width: 100%; display: block; }
		.userList .umUser.umPending * { width: 100%; display: block; }
	}
</style>


<% if(CanAdministerUsers ==  false){ %>
<style>
    .userManagerContainer .umTabs input[data-panel=invite] { display: none; }
    .userManagerContainer .umTabs input[data-panel=import] { display: none; }
    	.userManagerContainer .umTabs input { width: 50%; }
</style>
<% } %>
<div id="content">
	<input type="hidden" name="Mode" id="mode" value="properties" />
    <input type="hidden" name="CurrentOrgId" id="CurrentOrgId" value="0" />

	<h1 class="isleH1">Organization Management</h1>

	<div id="leftColumn">
		<div class="grayBox" id="sectionSelector">
			<h2 class="header">Manage Organization</h2>
			<select id="ddlMyOrganizations" class="<%=(activeOrganization.Id == 0 ? "round" : "") %>" title="Select an Organization to Manage">
				<option value="0">Select an Organization to Manage...</option>
				<% foreach(var item in myOrganizations){ %>
				<option value="<%=item.OrgId %>" <%=( activeOrganization.Id == item.OrgId ? "selected=\"selected\"" : "" ) %>><%=item.Organization %></option>
				<% } %>
				<% if(isSiteAdmin){ %>
				<option disabled="disabled" value="0">--- Other Organizations ---</option>
				<% foreach(var item in allOrganizations) { %>
				<option value="<%=item.Id %>" <%=( activeOrganization.Id == item.Id ? "selected=\"selected\"" : "" ) %>><%=item.Name %></option>
				<% } %>
				<% } %>
			</select>
			<input type="button" class="isleButton bgBlue <%=( activeOrganization.Id != 0 ? "selected" : "" ) %>" <%=(activeOrganization.Id == 0 ? "disabled=\"disabled\"" : "") %> value="Organization Information" data-sectionID="properties" onclick="showSection('properties');" />
			<input type="button" class="isleButton bgBlue" <%=(activeOrganization.Id < 1 ? "disabled=\"disabled\"" : "") %> value="Users" data-sectionID="users" onclick="showSection('users');" />
			<input type="button" class="isleButton bgBlue" <%=(activeOrganization.Id < 1 ? "disabled=\"disabled\"" : "") %> value="Libraries" data-sectionID="libraries" onclick="showSection('libraries');" />
			<input type="button" class="isleButton bgBlue" <%=(activeOrganization.Id < 1 ? "disabled=\"disabled\"" : "") %> value="Learning Lists" data-sectionID="learninglists" onclick="showSection('learninglists');" />
            <a href="./Reports/LibraryViews.aspx<%=(activeOrganization.Id > 0 ? "?organizationID=" + activeOrganization.Id.ToString() : "") %>" class="isleButton bgBlue" <%=(activeOrganization.Id < 1 ? "style=\"display:none\"" : "style=\"margin-top:1px; margin-bottom:1px;\"") %> width: 253px; text-align: center; target="ogActivity">Library Activity Reporting</a>

            <a href="/org/<%=activeOrganization.Id%>/timeline" class="isleButton bgBlue" <%=(activeOrganization.Id < 1 ? "style=\"display:none\"" : "") %> width: 253px; text-align: center;" target="ogTmline" >Organization Timeline</a>

            <input type="button" class="isleButton bgBlue" style="margin-top: 10px;"  value="New Organization" data-sectionID="newOrg" onclick="addOrg()";" />

		</div>
	</div><!-- /leftColumn

	--><div id="rightColumn">
		<div id="sectionContents">

			<!-- Default -->
			<% if(activeOrganization.Id == 0){ %>
			<div class="section selected" data-sectionID="default">
				<h2>Select an organization to manage</h2>
				<% foreach(var item in myOrganizations) { %>
				<a href="<%=baseURL %><%=item.OrgId %>"><%=item.Organization %></a>
				<% } %>
				<% if(isSiteAdmin){ %>
                <hr style="width: 50%; margin-top: 10px;" />
				<h2 style="margin-top:15px;">Other Organizations</h2>
				<% foreach(var item in allOrganizations) { %>
				<a href="<%=baseURL %><%=item.Id %>"><%=item.Name %> <%=item.IsActive == false ? " (INACTIVE)" : "" %></a>
				<% } %>
				<% } %>
			</div>

			<% } else { %>

			<!-- Properties -->
			<div class="section selected" data-sectionID="properties">
				<h2>Managing Properties for <%=activeOrganization.Name %></h2>
				<p class="required">Fields marked in <span>red</span> are required.</p>
				<% if(isSiteAdmin) { //TODO: change this to only be true if the user is staff/superadmin %>
				<div id="siteAdminProperties">
					<div class="item">
						<span>Organization ID</span><!--
						--><span style="text-align: left;" title="Organization ID"><%=activeOrganization.Id %></span>
					</div>
					<div class="item cbx">
						<label>
							<span>Active</span><!--
							--><input type="checkbox" name="IsActive" <%=activeOrganization.IsActive ? "checked" : "" %> title="Check this box if the Organization is active" />
						</label>
					</div>
				</div>
				<% } %>
				<div class="item required">
					<span>Name</span><!--
					--><input type="text" name="Name" placeholder="Name" value="<%=activeOrganization.Name %>" title="Organization Name (This field is required.)" />
				</div>
				<div class="item required">
					<span>Type</span><!--
					--><select name="OrgTypeId" title="Select the Organization type (This field is required.)">
						<% foreach(var item in orgTypes){ %>
						<option value="<%=item.Id %>" <%=activeOrganization.OrgTypeId == item.Id ? "selected=\"selected\"" : "" %>><%=item.Title %></option>
						<% } %>
				  </select>
				</div>
				<div class="item cbx">
					<label>
						<span>Member of ISLE</span><!--
					--><% if(isSiteAdmin){ %><!--
					--><input type="checkbox" name="IsIsleMember" <%=activeOrganization.IsIsleMember ? "checked" : "" %> title="Check this box if the Organization is a member of ISLE" />
                        <% } else { %><!--
					--><span class="data"><b><%=activeOrganization.IsIsleMember ? "True" : "False" %></b></span>
                        <% } %>
					</label>
				</div>
				<div class="item">
					<span>Email Domain</span><!--
					--><input type="text" name="EmailDomain" placeholder="Email Domain" value="<%=activeOrganization.EmailDomain %>" title="Email Domain for this Organization" />
					<div class="tip">
						<p>Example: <b>mydomain.com</b></p>
						<p>The email domain can be used to automatically add other people with the same domain as organization members when they register for IOER accounts.</p>
						<p><b>Use carefully:</b> For example, using "illinois.gov" would result in <b>all</b> Illinois government users being automatically added to this organization.</p>
					</div>
				</div>
				<div class="item">
					<span>Website</span><!--
					--><input type="text" name="WebSite" placeholder="http://" value="<%=activeOrganization.WebSite %>" title="Website URL" />
				</div>
				<div class="item split">
					<span class="mainLabel">Main Phone</span><!--
					--><input type="text" name="MainPhone" placeholder="123-456-7890" value="<%=activeOrganization.MainPhone %>" title="Area code and seven-digit phone number" /><!--
					--><span class="extraLabel">Ext.</span><!--
					--><input type="text" name="MainExtension" placeholder="1234" value="<%=activeOrganization.MainExtension %>" title="Phone number extension" />
				</div>
				<div class="item">
					<span>Fax</span><!--
					--><input type="text" name="Fax" placeholder="123-456-7890" value="<%=activeOrganization.Fax %>" title="Fax line phone number" />
				</div>
				<div class="item required">
					<span>Address 1</span><!--
					--><input type="text" name="Address1" placeholder="Address Line 1" value="<%=activeOrganization.Address1 %>" title="Address Field: Street address Line 1 (This field is required.)" />
				</div>
				<div class="item">
					<span>Address 2</span><!--
					--><input type="text" name="Address2" placeholder="Address Line 2" value="<%=activeOrganization.Address2 %>" title="Address Field: Street address Line 2" />
				</div>
				<div class="item required">
					<span>City</span><!--
					--><input type="text" name="City" placeholder="City" value="<%=activeOrganization.City %>" title="Address Field: City (This field is required.)" />
				</div>
				<div class="item required">
					<span>State</span><!--
					--><select name="State" title="Address Field: State (This field is required.)">
						<% foreach(var item in states){ %>
						<option value="<%=item.Title %>" <%=activeOrganization.State == item.Title ? "selected=\"selected\"" : "" %>><%=item.Description %></option>
						<% } %>
					</select>
				</div>

				<div class="item split required">
					<span class="mainLabel">Zip Code</span><!--
					--><input type="text" name="Zipcode" placeholder="12345" value="<%=activeOrganization.Zipcode %>" maxlength="5" title="Address Field: Five-digit zipcode (This field is required.)" /><!--
					--><span class="extraLabel">Ext.</span><!--
					--><input type="text" name="ZipCode4" placeholder="6789" value="<%=activeOrganization.ZipCode4 %>" maxlength="4" title="Address Field: Four-digit zipcode extension" />
					<div class="tip">
						<a href="https://tools.usps.com/go/ZipLookupAction_input" target="_blank">Look up your full zipcode here</a>
					</div>
				</div>



                <asp:Panel ID="imagePanel" runat="server" Visible="true">
                    <!-- -->
                    <div class="clearFloat"></div>
                    <div class="labelColumn">
                        <asp:Label ID="Label4" runat="server">Organization Logo</asp:Label>
                    </div>
                    <div class="dataColumn" style="max-width: 200px; ">
                        <asp:Literal ID="currentImage" runat="server" Visible="false"></asp:Literal>
                        <% if ( !string.IsNullOrWhiteSpace( activeOrganization.ImageUrl )  )
                   { %>
                <img id="thumbnail" src="<%=activeOrganization.ImageUrl %>" alt="Organzation preview thumbnail" />
                <% } %>
                    </div>
                    <div class="dataColumn" style="width: 150px; padding-top: 20px;">
                        <asp:Label ID="noLibraryImagelabel" runat="server" Visible="false">You can upload an image to represent the organization.</asp:Label>
                    </div>
                    <div class="clearFloat"></div>
                    <div class="labelColumn">&nbsp; </div>
                    <div class="dataColumn">
                        <asp:Label ID="currentFileName" runat="server" Visible="false"></asp:Label>


                        <br />
                        <span style="font-weight: bold;">Select an image for the organization</span><a class="toolTipLink" id="tipFile" title="organization Image|Select a image for your organization. It must be roughly square and should be 140px (width) x 140px (height) or it will be resized to a width of 140px and a height not taller than 140px. If the image is still taller than 140px, it will be cropped to fit the height."><img
                            src="/images/icons/infoBubble.gif" alt="" /></a>
                        <br />
                        <asp:FileUpload ID="fileUpload" runat="server" />
                    </div>
                </asp:Panel>

                <div class="item">
					<span>History</span><!--
					--><span style="text-align:left;""><%=activeOrganization.HistoryTitle() %></span>
				</div>
                <div class="buttons" style="margin-left: 25%;">
                    <asp:Panel ID="orgActionsPanel" runat="server" Visible="true">
				<input type="submit" class="isleButton bgGreen btnAction" value="Save Changes"   />
                        <asp:Button ID="btnSaveOrg" runat="server" cssclass="isleButton bgGreen btnAction" text="Save Changes" OnClick="btnSaveOrg_Click" Visible="false"/>
                    <asp:Button ID="btnDeleteOrg" runat="server" cssclass="isleButton bgGreen btnAction" text="Remove Organization" OnClientClick="doConfirmation()" OnClick="btnDeleteOrg_Click"/>
                        </asp:Panel>
                </div>
			</div>
			
			<!-- Users -->
            <div class="section" data-sectionid="users">
                <h2>Managing Users for <%=activeOrganization.Name %></h2>
                <div class="userManagerContainer" id="userManagerContainer" runat="server">
                    <uc1:UserManager ID="userManager" runat="server" />
                </div>

            </div>

			<!-- Libraries -->
			<div class="section" data-sectionID="libraries">
				<h2>Managing Libraries for <%=activeOrganization.Name %></h2>
                <div class="optionLinks">
                    <a class="libLink" href="/Libraries/Admin.aspx?action=new"  target="orgLink">New Library</a>
                </div>
				<% if(libraries.Count() > 0) { %>
				<p>The following Libraries are available for this organization:</p>
				<div id="orgLibraries">
					<% foreach(var item in libraries) { %>
					<div class="manageBox grayBox" data-id="<%=item.Id %>">
						<h3 class="title"><%=item.Title %></h3>
						<div class="manageLinks">
							<a href="/library/<%=item.Id %>/<%=item.FriendlyTitle %>" target="orgLink">Go to this Library</a>
							<span class="divider"> | </span>
							<a href="/Libraries/Admin.aspx?id=<%=item.Id %>&action=edit" target="orgLink">Manage this Library</a>
						</div>
						<p><%=item.Description %></p>
						<div class="access">
							<p>Organization Access: <b><%=libraryCodes.Where( m => m.Id == item.OrgAccessLevelInt ).FirstOrDefault().Title %></b></p>
							<p>Public Access: <b><%=libraryCodes.Where( m => m.Id == item.PublicAccessLevelInt ).FirstOrDefault().Title %></b></p>
						</div>
						<div class="orgAvatar" style="background-image:url('<%=item.ImageUrl %>');"></div>
					</div>
					<% } %>
				</div>
				<% } %>
			</div>

			<!-- Learning Lists -->
			<div class="section" data-sectionID="learninglists">
				<h2>Managing Learning Lists for <%=activeOrganization.Name %></h2>
                <div class="optionLinks">
                    <a class="libLink"  href="/My/LearningList/new"  target="orgLink">New Learning List</a>
                </div>
				<% if(lists.Count() > 0) { %>
				<p>The following Learning Lists are available for this organization:</p>
				<div id="learningLists">
					<% foreach(var item in lists) { %>
					<div class="manageBox grayBox" data-id="<%=item.Id %>">
						<h3 class="title"><%=item.Title %></h3>
						<div class="manageLinks">
							<a href="<%=item.Url %>" target="orgLink">Go to this Learning List</a>
							<span class="divider"> | </span>
							<a href="/my<%=item.Url %>" target="orgLink">Manage this Learning List</a>
						</div>
						<p><%=item.Description %></p>
						<div class="orgAvatar" style="background-image:url('<%=item.ImageUrl %>');"></div>
					</div>
					<% } %>
				</div>
				<% } %>
			</div>

			<% } //show the above sections if activeOrganization.Id != 0 %>
		</div><!-- /sectionContents -->
	</div><!-- /rightColumn -->
</div>
<asp:Panel ID="hiddenStuff" runat="server" Visible="false">
<!-- filter to only show orgs where user is an admin, or account admin-->
<asp:Literal ID="orgAdminFilter" runat="server" Visible="false" >( base.Id in (SELECT  OrgId FROM [OrganizationMember.RoleIdCSV] where ([IsAdmin] = 1 or [IsAccountAdmin] = 1) AND  UserId = {0})) </asp:Literal>

<asp:Literal ID="txtCurrentOrgId" runat="server" Visible="false" >0</asp:Literal>
<asp:Literal ID="litCommonDomains" runat="server" Visible="false" >gmail.com hotmail.com outlook.com comcast.com</asp:Literal>

<asp:Literal ID="txtNewOrgRequest" runat="server" ><p>{0} has created a new organzation. Please login to organization management, and review the organization for approval. <p><b>New Organization</b>: {1}</p><p>There should probably be a more elborate process, but that will come later.</p></asp:Literal>
</asp:Panel>
