<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ManageUsers.ascx.cs" Inherits="IOER.Controls.ManageUsers" %>
<%@ Register Src="~/Organizations/controls/Import.ascx" TagPrefix="uc1" TagName="Import" %>
<%@ Register Src="~/Controls/ToolTipV3.ascx" TagPrefix="uc1" TagName="Tooltip" %>

<uc1:Tooltip id="toolTip" runat="server" />

<%-- <script type="text/javascript" language="javascript" src="/Scripts/toolTip.js"></script>
<link rel="Stylesheet" type="text/css" href="/Styles/ToolTip.css" />--%>

<div id="userManagerError" class="userManagerPanel" runat="server" visible="false">
  <style type="text/css">
    .userManagerPanel p { padding: 25px 10px; text-align: center; }

  </style>
  <p>Unable to manage this object.</p>
  <p id="userManagerErrorMessage" runat="server"></p>
</div>
<div id="userManagerPanel" class="userManagerPanel" runat="server" visible="true">

  <script type="text/javascript">
    //Emulate namespacing to avoid conflicts with other controls on the page
    var um = {};
    um.requests = [];
    um.timer = 0;
    um.lastQuery = {};
    um.queryCooldown = 0;
    um.queryAvailable = true;
    um.users = [];
    um.pendingUsers = [];

    /* --- Initialize --- */
    $(document).ready(function() {
      $(".umPanel[data-panel=invite] .email").on("keyup change", function () {
        um.canInviteUser();
      });
      um.refreshExistingUsers();
      um.refreshPendingUsers();
        //??
      $("#umSelectedUser").hide();
    });
    
    /* --- Page Functions --- */
    //Show tab
    um.showTab = function (target) {
      $(".umTabs input, .umPanel").removeClass("active").filter("[data-panel=" + target + "]").addClass("active");
    };

    //Check existing email
    um.canInviteUser = function () {
      clearInterval(um.timer);
      um.timer = setTimeout(function() {
      	var email = um.getInviteEmail();
      	if(email.length == 0){
      		um.setStatus("Enter an email address to begin", "");
      		return;
      	}
      	else {
      		um.doAjax("AjaxCanInviteUser", um.getQuery(0, um.getInviteEmail(),"","", 0, "", []), um.successCanInviteUser);
      	}
      }, 800);
    };

    //Send an invite
    um.sendInvite = function() {
        var message = "";
        var email = um.getInviteEmail();
        if (email.length == 0)
            message = "Email is required\r\n";
        var firstName = um.getFirstName();
        if (firstName.length == 0)
            message += "First Name is required\r\n";
        var lastName = um.getLastName();
        if (lastName.length == 0)
            message += "Last Name is required\r\n";
        if (message.length > 0) {
            alert("Errors: \r\n" + message);
        } else {
            um.doAjax("AjaxInviteOrgUser", um.getQuery(0, email, firstName,lastName, um.getInviteMemberType(), um.getText($(".umPanel[data-panel=invite] .message")), um.getTargetMemberRoleIDs(false)), um.successInvite);
        }
    };

  	//Refresh the list of existing users
    um.refreshExistingUsers = function() {
        um.doAjax("AjaxGetUsers", um.getQuery(0, "","","", 0, "", []), um.successRefreshExistingUsers);
    }

  	//Refresh the list of pending users
    um.refreshPendingUsers = function() {
        um.doAjax("AjaxGetPendingUsers", um.getQuery(0, "","","", 0, "", []), um.successRefreshPendingUsers);
    }

    //Get query (some parameters are always the same per page load):
    um.getQuery = function(userID, userEmail, firstName, lastName,userMemberType, message, roleIDs){
    	return { input: {
    		ObjectId: <%=ObjectId %>,
    		ObjectType: "<%=ObjectType %>",
    		TargetUserId: userID,
    		TargetUserEmail: userEmail,
    		TargetFirstName: firstName,
    		TargetLastName: lastName,
    		IsNameRequired: true,
    		TargetMemberTypeId: userMemberType,
    		Message: message,
    		TargetMemberRoleIds: roleIDs
    	} };
    }

    //Get email invite value
    um.getInviteEmail = function () {
      return um.getText($(".umPanel[data-panel=invite] .email"));
    };
    um.getFirstName = function () {
        return um.getText($(".umPanel[data-panel=invite] .firstName"));
    };
    um.getLastName = function () {
        return um.getText($(".umPanel[data-panel=invite] .lastName"));
    };
    //Get generic text value
    um.getText = function (jItem) {
      return jItem.val().trim();
    };
    //Get Invitation Member Type value
    um.getInviteMemberType = function () {
      return um.getDDL($(".umPanel[data-panel=invite] .umMemberType"), true);
    };
    //Get generic DDL value, with option to parseInt
    um.getDDL = function (jItem, isInteger) {
      var value = jItem.find("option:selected").attr("value");
      return isInteger ? parseInt(value) : value;
    };
  	//Get selected role IDs, if any
    um.getTargetMemberRoleIDs = function(existing) {
    	var boxes;
			//If indicated, select from the existing users tab
    	if(existing){
    		boxes = $("#umSelectedUser_roles input:checked");
    	}
			//Otherwise, select from the invite users tab
    	else {
    		boxes = $(".umMemberRoles input[type=checkbox]:checked");
    	}
    	var results = [];
    	if(boxes.length > 0){
    		boxes.each(function() {
    			results.push(parseInt($(this).attr("value")));
    		});
    	}
    	return results;
    };

  	//Set status message
    um.setStatus = function(message, cssClass){
    	$(".umPanel .status").attr("class", "status " + cssClass).html(message);
    }

  	//Select a user
    um.selectUser = function(id) {

        
    	//Select the user
    	var currentUser = {};
    	for(var i in um.users){
    		if(um.users[i].UserId == id){
    			currentUser = um.users[i];
    			$("#umSelectedUser").show();
    			break;
    		}
    	}
    	console.log("Selected Member:", currentUser);
        $("html, body").animate({ scrollTop: $("#umSelectedUser").position().top }, 250);

        $("#btnUpdateSelectedUser").show();
        $("#btnRemoveSelectedUser").show();
    	//Populate the details
    	$("#umSelectedUser_name").html(currentUser.MemberFullName);
    	$("#umSelectedUser_email").html(currentUser.Email == null ? "" : currentUser.Email);
    	$("#umSelectedUser").attr("data-UserID", currentUser.UserId);
    	$("#umSelectedUser_ddlType option[value=" + currentUser.MemberTypeId + "]").prop("selected", true);
    	$("#umSelectedUser_roles input").each(function() {
				//Figure out whether or not the checkbox should be checked
    		var cbx = $(this);
    		cbx.prop("checked", false);
    		var value = parseInt(cbx.attr("value"));
    		for(var i in currentUser.Roles){
    			if(currentUser.Roles[i].Id == value){
    				cbx.prop("checked", true);
    			}
    		}
    	});
    	$("#umSelectedUser_avatar").attr("style", "background-image: url('" + currentUser.MemberImageUrl + "');");
    }

		//Manage an existing user
  	um.updateSelectedUser = function() {
  		var id = parseInt($("#umSelectedUser").attr("data-userID"));
  		var selectedType = parseInt($("#umSelectedUser_ddlType option:selected").attr("value"));
  		var selectedRoles = um.getTargetMemberRoleIDs(true);
  		query = um.getQuery(id, "","","", selectedType, "", selectedRoles);
  		console.log(selectedType);
  		console.log(selectedRoles);
  		console.log(query);

  		um.doAjax("AjaxUpdateUserMemberType", query, um.successUpdateUserMemberType)
  	}

		//Remove an existing user
  	um.removeSelectedUser = function(){
  		var id = parseInt($("#umSelectedUser").attr("data-userID"));
  		var name = $("#umSelectedUser_name").html();
  		if(id > 0){
  			if(confirm("Are you sure you want to remove " + name + " from this <%=ObjectTypeTitle %>?")){
  			    um.doAjax("AjaxRemoveUser", um.getQuery(id, "", "","", 0, "", []), um.successRemoveUserMember)
  			}
  		}
  	}

    /* --- AJAX --- */
    um.doAjax = function (method, data, success) {
      /*
    	//Skip if same
      var stringifiedData = JSON.stringify(data);
      if(stringifiedData == um.lastQuery && !um.queryAvailable){ return; }
      um.lastQuery = stringifiedData;

			//Allow duplicate queries after a short time
      um.queryAvailable = false;
      clearTimeout(um.queryCooldown);
      um.queryCooldown = setTimeout(function() { um.queryAvailable = true; }, 500);

      */

      //Do AJAX
      $.ajax({
        url: "/services/usermanagementservice.asmx/" + method,
        async: true,
        headers: { "Accept": "application/json", "Content-type": "application/json; charset=utf-8" },
        dataType: "json",
        type: "POST",
        data: JSON.stringify(data),
        success: function (msg) {
          var message = msg.d ? $.parseJSON(msg.d) : msg;
          console.log(message);
          success(message);
        }
      });
    };

  	/* --- Success --- */
    um.fail = function(msg){
    	alert(msg.status);
    	console.log(msg.extra);
    }

    um.successCanInviteUser = function(msg){
    	if(msg.valid && msg.data){ //Transaction may be valid, but user may still not be inviteable
    		if(msg.extra) { //if user already exists in IOER
    			um.setStatus("Ready to add this member", "green");
    		}
    		else {
    			um.setStatus("Ready to invite this person", "green");
    		}
    	}
    	else {
    		um.setStatus(msg.status, "red");
    	}
    };
    
    um.successInvite = function(msg){
    	if(msg.valid){
    		alert("Person invited!");
    		$(".umPanel[data-panel=invite] .email").val("").trigger("change");
            $(".umPanel[data-panel=invite] .firstName").val("").trigger("change");
            $(".umPanel[data-panel=invite] .lastName").val("").trigger("change");
    		$(".umPanel[data-panel=invite] .memberType option:first-child").prop("selected", true).trigger("change");
    		$(".umPanel[data-panel=invite] .message").val("").trigger("change");
    		um.refreshPendingUsers();
    		um.refreshExistingUsers();
            //clear invite
    	}
    	else {
    		um.fail(msg);
    	}
    };

    um.successRefreshExistingUsers = function(msg){
    	if(msg.valid){
    		um.users = msg.data;
    		um.renderExistingUsers(msg.data);
    	}
    	else {
    		console.log("Error updating existing members:", msg.status);
    		console.log(msg.extra);
    	}
    }

    um.successRefreshPendingUsers = function(msg){
    	if(msg.valid){
    		um.pendingUsers = msg.data;
    		um.renderPendingUsers(msg.data);
    	}
    	else {
    		console.log("Error refreshing pending members:", msg.status);
    		console.log(msg.extra);
    	}
    }
    um.successUpdateUserMemberType = function(msg){
        if(msg.valid){
            alert("Changes saved!");
            um.refreshExistingUsers();
        }
        else { 
            um.fail(msg);
        }
    }

    um.successRemoveUserMember = function(msg){
    	if(msg.valid){
    	    alert("Person removed from Organization!");
    	    $("#umSelectedUser").hide();
    		um.refreshExistingUsers();
    	}
    	else { 
    		um.fail(msg);
    	}
    }

  	/* --- Rendering --- */
    um.renderExistingUsers = function(users){
    	var box = $(".umPanel[data-panel=current] .userList");
    	var template = $("#template_existingUser").html();
    	box.html("");
    	if(users.length > 0) {
    	    $("[id$='btnDownload']").show();
    	}
    	for(var i in users){
    		var user = users[i];
    		box.append(template
					.replace(/{id}/g, user.UserId)
					.replace(/{name}/g, user.FirstName + " " + user.LastName)
					.replace(/{memberType}/g, user.MemberType)
					.replace(/{memberTypeID}/g, user.MemberTypeId)
                    .replace(/{memberRoles}/g, user.MemberRoles)
                    .replace(/{memberLastLogin}/g, user.LastLoginShortDate)
					.replace(/{img}/g, user.MemberImageUrl)
					<% foreach(var item in MemberRoles) { %>
					.replace(/{checked_<%=item.Id %>}/g, user.Roles.indexOf(<%=item.Id %>) > -1 ? "checked=\"checked\"" : "" )
					<% } %>
				);
    	}
    }

  	um.renderPendingUsers = function(users){
  		var box = $(".umPanel[data-panel=pending] .userList");
  		var template = $("#template_pendingUser").html();
  		box.html("");
  		if(users.length == 0){
  			box.html("<p class=\"grayMessage\">No members are pending.</p>");
  			return;
  		}
  		for(var i in users){
  			var user = users[i];
  			box.append(template
					.replace(/{id}/g, user.id)
					.replace(/{email}/g, user.Email)
					.replace(/{memberType}/g, user.MemberType)
					.replace(/{invitedDate}/g, user.CreatedText)
				);
  		}
  	}

  	//function openImportPage( rId) {
  	//    var id = $("#umOrgRId").html();

  	//    //window.location.href = "/Organizations/Import.aspx?rid=" + id;
  	//    var win = window.open("/Organizations/Import.aspx?rid=" + id, '_blank');
  	//}
  </script>
  <style type="text/css">
		/* Object Type specific things */
  	.userManagerPanel[data-type=learninglist] .umTabs input { width: 50%; }
  	.userManagerPanel[data-type=learninglist] .umTabs input[data-panel=pending] { display: none; }

    /* Big Stuff */
    .umTabs { text-align: center; padding-bottom: 5px; }
    .umTabs input { display: inline-block; vertical-align: top; width: 33.333%; border-radius: 0; }
    .umTabs input:first-child { border-radius: 5px 0 0 5px; }
    .umTabs input:last-child { border-radius: 0 5px 5px 0; }
    .umTabs input.active { background-color: #9984BD; }
    .umPanel { display: none; }
    .umPanel.active { display: block; }
  	.umPanel .label { margin-bottom: 10px; }
    .umButtons { text-align: right; }
    .umPanel textarea { min-height: 3em; max-height: 10em; height: 5em; resize: vertical; }
  	.umPanel .status { padding: 2px 5px; font-style: italic; min-height: 1.5em; color: #555; transition: color 0.2s; }
  	.umPanel .status.red { color: #900; }
  	.umPanel .status.green { color: #090; }
  	p.grayMessage { text-align: center; color: #333; font-style: italic; padding: 50px; }

    /* Details */
    .umColumn { display: inline-block; vertical-align: top; }
    .umColumn.form { width: 66%; }
    .umColumn.help { width: 34%; padding: 0 10px 5px 10px; }
  	.umPanel select:hover, .umPanel select:focus { cursor: pointer; }

		/* Users */
  	.umUser { padding: 5px; border-radius: 5px; margin: 0 0 5px 0; transition: background-color 0.2s; position: relative; width: 100%; display: inline-block; vertical-align: top; }
  	.umUser .avatar { background-size: cover; background-repeat: no-repeat; position: absolute; top: 0; left: 0; width: 75px; height: 100%; background-color: #DDD; background-position: center center; border-radius: 5px 0 0 5px; }
  	.umExisting:hover, .umExisting:focus { background-color: #DFDFDF; cursor: pointer; }
  	.umExisting { padding-left: 80px; border: 1px solid #CCC; }
  	.umExisting .umUserTitle, .umExisting .umUserControls { display: inline-block; vertical-align: top; }
  	.umExisting .umUserTitle { width: calc(100% - 75px); }
  	.umExisting .umUserControls { width: 75px; }
  	.umExisting .umUserControls input { opacity: 0; width: 0; }
  	.umExisting .umUserControls input:focus { opacity: 1; width: 100%; }
  	.umExisting .umName { font-weight: bold; }
  	.umExisting .umMemberType { font-style: italic; }
  	.umPending * { display: inline-block; margin: 0; vertical-align: top; width: 30%; padding: 2px 5px; }
  	.umMemberRoles label, #umSelectedUser label { display: block; padding: 2px 5px; border-radius: 5px; transition: background-color 0.2s; }
  	.umMemberRoles label:hover, .umExisting .umMemberRoles label:focus, #umSelectedUser label:hover, #umSelectedUser label:focus { background-color: #DDD; cursor: pointer; }
  	#umSelectedUser label { display: inline-block; vertical-align: top; width: 50%; max-width: 100%; text-align: left; }
  	#umSelectedUser { padding-right: 160px; position: relative; min-height: 160px; margin: 5px 0; }
  	#umSelectedUser_properties, #umSelectedUser_buttons { display: inline-block; vertical-align: top; }
  	#umSelectedUser_properties { width: calc(100% - 200px); }
  	#umSelectedUser_buttons { width: 200px; padding: 5px 0 0 5px; }
  	#umSelectedUser_name { font-weight: bold; font-size: 24px; background-color: #4F4E4F; color: #FFF; padding: 2px 5px; border-radius: 5px; margin-bottom: 5px; }
  	#umSelectedUser_type { margin-bottom: 10px; }
  	#umSelectedUser_avatar { width: 150px; height: 150px; border-radius: 5px; position: absolute; top: 0; right: 5px; background-color: #CCC; border: 1px solid #4F4E4F; background-size: cover; background-repeat: no-repeat; background-position: center center; }
  	#umSelectedUser_buttons input { margin-bottom: 5px; }
  	#umUser_ImportExportbuttons { text-align: right; }
  	#umUser_ImportExportbuttons .isleButton { width: 130px; border: none; text-align: center; vertical-align: top; display: inline-block; height: 30px; line-height: 25px; }
  </style>

<% if(CanAdministerUsers ==  false){ %>
<style>
    #btnUpdateSelectedUser, #btnRemoveSelectedUser { display: none; }
</style>
<% } %>

  <div class="umTabs">
    <input type="button" class="isleButton bgBlue active" value="Current Members" onclick="um.showTab('current')" data-panel="current" /><!--
    --><input type="button" class="isleButton bgBlue" value="Invite Members" onclick="um.showTab('invite')" data-panel="invite" /><!--
    --><input type="button" class="isleButton bgBlue" value="Pending Members" onclick="um.showTab('pending')" data-panel="pending" />
  </div><!--/umTabs--><!--

  --><div class="umPanels">
    <div class="umPanel" data-panel="invite">
      <h3 class="mid">Invite New Member</h3>
      <div class="umColumn form">
        <!-- Elements to invite a new user -->
        <div class="label">
          <div>First Name</div><!--
          --><input type="text" id="inviteFirstName" class="firstName" />
        </div>
          <div class="label">
          <div>Last Name</div><!--
          --><input type="text" id="inviteLastName" class="lastName" />
        </div>
        <div class="label">
          <div>Email Address</div><!--
          --><input type="text" class="email" />
        </div>
        <div class="status">Enter an email address to begin</div>
        <div class="label">
          <div class="toolTip toolTipBubbleBefore toolTipBubbleOnly" title="Organization Member Types|<ul><li><strong>Administrator</strong> - An administrator can performance all available functions including: adding people, assigning any role, creating libraries, and communities. As well will have the same privileges as a staff member.</li><li><strong>Staff member/Employee</strong> - A staff member will have access to functions that are defined as staff only, will be able to contribute to organzation libraries (unless restricted by an library administrator), and other functions that may be only allowed for members of an organization.</li><li><strong>Student</strong> - A student will have many of the privileges of a staff member. A user may exclude students from accesssing content such as assessments.</li><li><strong>Contractor/External</strong> - People from outside an organization may be added to an organization to facilitate collaboration on learning lists, or perhaps to easily enable access to libraries. An external will have the same implicit privileges as a staff member.</li></ul>">Select Member Type</div><!--
          --><select class="umMemberType">
						<% foreach(var item in MemberTypes){ %>
							<option value="<%=item.Id %>"><%=item.Title %></option>
						<% } %>
          </select>
        </div>
				<% if(MemberRoles.Count() > 0) { %>
        <div class="label">
          <div class="toolTip toolTipBubbleBefore toolTipBubbleOnly" title="Organization Roles|<ul><li><strong>Administrator</strong> - An administrator can perform all the functions associated with any of the organization roles. As an administrator can also assign the administrator role to others.</li><li ><strong>Content Administrator</strong> - May approve content - where content approval has been implemented by an organization.</li><li><strong>Library Administrator</strong> - May create libraries, and assign any role, including administrator to any other user.</li><li class='offScreen'><strong>Account Administrator</strong> - Will have access to manage organization people??.</li><li><strong>Content Curator</strong> - Can add/invite people to share content like a learning list.</li></ul>">Select Member Role(s)</div><!--
          --><div class="umMemberRoles">
						<% foreach(var item in MemberRoles){ %>
							<label><input type="checkbox" value="<%=item.Id %>" /> <%=item.Title %></label>
						<% } %>
          </div>
        </div>
				<% } %>
        <div class="label">
          <div>Message to Member</div><!--
          --><textarea class="message"></textarea>
        </div>
        <div class="umButtons">
          <input type="button" class="isleButton bgBlue" value="Send Invite" onclick="um.sendInvite()" />
        </div>

      </div><!--
      --><div class="umColumn help">
        <!-- User help -->
        <h3>To invite a member...</h3>
        <ol>
          <li>Enter the email address of the person you want to invite</li>
					<li>Select the member type (and role, if applicable) you want this person to be when they join</li>
          <li>Optionally, add a message to send along with the invitation</li>
        </ol>
      </div>

    </div><!--/invite-->
    <div class="umPanel" data-panel="pending">
      <h3 class="mid">Pending Members</h3>
      <p>These people have either requested access or been invited:</p>
      <div class="userList"></div>

    </div><!--/pending-->
    <div class="umPanel active" data-panel="current">
			<div id="umSelectedUser" data-userID="0" style="display: none;">
			<div id="umSelectedUser_name"></div>
			<div id="umSelectedUser_properties">
				<a href="mailto:" id="umSelectedUser_email"></a>
				<div id="umSelectedUser_type">
						<div class="toolTip toolTipBubbleBefore toolTipBubbleOnly" title="Organization Member Types|<ul><li><strong>Administrator</strong> - An administrator can performance all available functions including: adding people, assigning any role, creating libraries, and communities. As well will have the same privileges as a staff member.</li><li><strong>Staff member/Employee</strong> - A staff member will have access to functions that are defined as staff only, will be able to contribute to organzation libraries (unless restricted by an library administrator), and other functions that may be only allowed for members of an organization.</li><li><strong>Student</strong> - A student will have many of the privileges of a staff member. A user may exclude students from accesssing content such as assessments.</li><li><strong>Contractor/External</strong> - People from outside an organization may be added to an organization to facilitate collaboration on learning lists, or perhaps to easily enable access to libraries. An external will have the same implicit privileges as a staff member.</li></ul>"><strong> Member Type:</strong></div>
					<select id="umSelectedUser_ddlType">
						<% foreach(var item in MemberTypes){ %>
							<option value="<%=item.Id %>"><%=item.Title %></option>
						<% } %>
					</select>
				</div>

				<% if(MemberRoles.Count() > 0){ %>
				<div id="umSelectedUser_roles">
						<div class="toolTip toolTipBubbleBefore toolTipBubbleOnly" title="Organization Roles|<ul><li><strong>Administrator</strong> - An administrator can perform all the functions associated with any of the organization roles. As an administrator can also assign the administrator role to others.</li><li ><strong>Content Administrator</strong> - May approve content - where content approval has been implemented by an organization.</li><li><strong>Library Administrator</strong> - May create libraries, and assign any role, including administrator to any other user.</li><li class='offScreen'><strong>Account Administrator</strong> - Will have access to manage organization people??.</li><li><strong>Content Curator</strong> - Can add/invite people to share content like a learning list.</li></ul>"><strong> <%=ObjectTypeTitle %> Roles:</strong></div>
					<% foreach(var item in MemberRoles){ %><label><input type="checkbox" value="<%=item.Id %>" /> <%=item.Title %></label><% } %>
				</div>
				<% } %>
			</div><!--
			--><div id="umSelectedUser_buttons">
				<input type="button" class="isleButton bgBlue" id="btnUpdateSelectedUser" onclick="um.updateSelectedUser();" value="Update" />
				<input type="button" class="isleButton bgRed" id="btnRemoveSelectedUser" onclick="um.removeSelectedUser();" value="Remove Member" />
			</div>
			<div id="umSelectedUser_avatar"></div>
		</div>
        <div id="umUser_ImportExportbuttons">
    <!-- download button -->
        <asp:Button ID="btnDownload" runat="server" CssClass="isleButton bgBlue" OnClick="btnDownload_Click" Text="Export Members" />
        <asp:HyperLink ID="hlkImport" runat="server" CssClass="isleButton bgBlue" NavigateUrl="" Text="Import Members" Target="_blank" Visible="true"></asp:HyperLink>
        <%--<asp:Button ID="btnImport" runat="server" CssClass="isleButton bgBlue" visible="false" OnClientClick="openImportPage()" Text="Import Users2" Style="width: 110px; height: 25px; margin-bottom: 5px; margin-right: 5px; float: right;" />--%>
            </div>
      <div style="float:left;">
        <h3>Select a member to manage:</h3>
      </div>
      <br  />
      <div class="userList"></div>
    </div><!--/current-->

    </div><!--/umPanels-->
	<div id="umTemplates" style="display:none;">
		<script id="template_existingUser" type="text/template"><!--
			--><div class="umUser umExisting" tabindex="0" data-id="{id}" onclick="um.selectUser({id})">
				<div class="avatar" style="background-image:url({img});"></div>
				<div class="umUserTitle">
					<div class="umName">{name}</div>
					<div class="umMemberType">{memberType}</div>
                    <div class="umMemberRoles">Roles: {memberRoles}</div>
                    <div class="umMemberLastLogin">Last Login: {memberLastLogin}</div>
				</div><!--
				--><div class="umUserControls">
					<input type="button" class="isleButton bgBlue" value="Select" onclick="um.selectUser({id})" />
				</div>
			</div><!--
		--></script>
		<script id="template_pendingUser" type="text/template">
			<div class="umUser umPending grayBox" data-id="{id}">
				<a href="mailto:{email}">{email}</a>
				<p>Invited as: {memberType}</p>
				<p>Invited on {invitedDate}</p>
			</div>
		</script>
	</div>
    <asp:literal ID="txtImportUrl" runat="server" Visible="false">/Organizations/Import.aspx?rid={0}</asp:literal>
    <div id="umOrgRId" class="offScreen"><asp:literal ID="litOrgRId" runat="server" ></asp:literal></div>
</div><!-- /userManagerPanel -->