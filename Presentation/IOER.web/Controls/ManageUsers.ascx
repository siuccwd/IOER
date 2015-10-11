<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ManageUsers.ascx.cs" Inherits="IOER.Controls.ManageUsers" %>

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
      		um.doAjax("AjaxCanInviteUser", um.getQuery(0, um.getInviteEmail(), 0, "", []), um.successCanInviteUser);
      	}
      }, 800);
    };

    //Send an invite
    um.sendInvite = function() {
      um.doAjax("AjaxInviteUser", um.getQuery(0, um.getInviteEmail(), um.getInviteMemberType(), um.getText($(".umPanel[data-panel=invite] .message")), um.getTargetMemberRoleIDs(false)), um.successInvite);
    };

  	//Refresh the list of existing users
    um.refreshExistingUsers = function() {
    	um.doAjax("AjaxGetUsers", um.getQuery(0, "", 0, "", []), um.successRefreshExistingUsers);
    }

  	//Refresh the list of pending users
    um.refreshPendingUsers = function() {
    	um.doAjax("AjaxGetPendingUsers", um.getQuery(0, "", 0, "", []), um.successRefreshPendingUsers);
    }

    //Get query (some parameters are always the same per page load):
    um.getQuery = function(userID, userEmail, userMemberType, message, roleIDs){
    	return { input: {
    		ObjectId: <%=ObjectId %>,
    		ObjectType: "<%=ObjectType %>",
    		TargetUserId: userID,
    		TargetUserEmail: userEmail,
    		TargetMemberTypeId: userMemberType,
    		Message: message,
    		TargetMemberRoleIds: roleIDs
    	} };
    }

    //Get email invite value
    um.getInviteEmail = function () {
      return um.getText($(".umPanel[data-panel=invite] .email"));
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
    	console.log("Selected User:", currentUser);

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
  		query = um.getQuery(id, "", selectedType, "", selectedRoles);
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
  				um.doAjax("AjaxRemoveUser", um.getQuery(id, "", 0, "", []), um.successUpdateUserMemberType)
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
    			um.setStatus("Ready to add this user", "green");
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
    		alert("User invited!");
    		$(".umPanel[data-panel=invite] .email").val("").trigger("change");
    		$(".umPanel[data-panel=invite] .memberType option:first-child").prop("selected", true).trigger("change");
    		$(".umPanel[data-panel=invite] .message").val("").trigger("change");
    		um.refreshPendingUsers();
    		um.refreshExistingUsers();
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
    		console.log("Error updating existing users:", msg.status);
    		console.log(msg.extra);
    	}
    }

    um.successRefreshPendingUsers = function(msg){
    	if(msg.valid){
    		um.pendingUsers = msg.data;
    		um.renderPendingUsers(msg.data);
    	}
    	else {
    		console.log("Error refreshing pending users:", msg.status);
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

  	/* --- Rendering --- */
    um.renderExistingUsers = function(users){
    	var box = $(".umPanel[data-panel=current] .userList");
    	var template = $("#template_existingUser").html();
    	box.html("");
    	for(var i in users){
    		var user = users[i];
    		box.append(template
					.replace(/{id}/g, user.UserId)
					.replace(/{name}/g, user.FirstName + " " + user.LastName)
					.replace(/{memberType}/g, user.MemberType)
					.replace(/{memberTypeID}/g, user.MemberTypeId)
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
  			box.html("<p class=\"grayMessage\">No users are pending.</p>");
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
  	.umUser .avatar { background-size: cover; background-repeat: no-repeat; position: absolute; top: 0; left: 0; width: 75px; height: 100%; background-color: #DDD; background-position: center center; }
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

  </style>

  <div class="umTabs">
    <input type="button" class="isleButton bgBlue active" value="Current Users" onclick="um.showTab('current')" data-panel="current" /><!--
    --><input type="button" class="isleButton bgBlue" value="Invite Users" onclick="um.showTab('invite')" data-panel="invite" /><!--
    --><input type="button" class="isleButton bgBlue" value="Pending Users" onclick="um.showTab('pending')" data-panel="pending" />
  </div><!--/umTabs--><!--

  --><div class="umPanels">
    <div class="umPanel" data-panel="invite">
      <h3 class="mid">Invite New User</h3>
      <div class="umColumn form">
        <!-- Elements to invite a new user -->
        <div class="label">
          <div>Email Address</div><!--
          --><input type="text" class="email" />
        </div>
        <div class="status">Enter an email address to begin</div>
        <div class="label">
          <div>Select Member Type</div><!--
          --><select class="umMemberType">
						<% foreach(var item in MemberTypes){ %>
							<option value="<%=item.Id %>"><%=item.Title %></option>
						<% } %>
          </select>
        </div>
				<% if(MemberRoles.Count() > 0) { %>
        <div class="label">
          <div>Select Member Role(s)</div><!--
          --><div class="umMemberRoles">
						<% foreach(var item in MemberRoles){ %>
							<label><input type="checkbox" value="<%=item.Id %>" /> <%=item.Title %></label>
						<% } %>
          </div>
        </div>
				<% } %>
        <div class="label">
          <div>Message to User</div><!--
          --><textarea class="message"></textarea>
        </div>
        <div class="umButtons">
          <input type="button" class="isleButton bgBlue" value="Send Invite" onclick="um.sendInvite()" />
        </div>

      </div><!--
      --><div class="umColumn help">
        <!-- User help -->
        <h3>To invite a user...</h3>
        <ol>
          <li>Enter the email address of the person you want to invite</li>
					<li>Select the member type (and role, if applicable) you want this person to be when they join</li>
          <li>Optionally, add a message to send along with the invitation</li>
        </ol>
      </div>

    </div><!--/invite-->
    <div class="umPanel" data-panel="pending">
      <h3 class="mid">Pending Users</h3>
      <p>These users have either requested access or been invited:</p>
      <div class="userList"></div>

    </div><!--/pending-->
    <div class="umPanel active" data-panel="current">
			<div id="umSelectedUser" data-userID="0" style="display: none;">
				<div id="umSelectedUser_name"></div>
				<div id="umSelectedUser_properties">
					<a href="mailto:" id="umSelectedUser_email"></a>
					<div id="umSelectedUser_type">
						<h3>Member Type:</h3>
						<select id="umSelectedUser_ddlType">
							<% foreach(var item in MemberTypes){ %>
								<option value="<%=item.Id %>"><%=item.Title %></option>
							<% } %>
						</select>
					</div>
					<% if(MemberRoles.Count() > 0){ %>
					<div id="umSelectedUser_roles">
						<h3><%=ObjectTypeTitle %> Roles:</h3>
						<% foreach(var item in MemberRoles){ %><label><input type="checkbox" value="<%=item.Id %>" /> <%=item.Title %></label><% } %>
					</div>
					<% } %>
				</div><!--
				--><div id="umSelectedUser_buttons">
					<input type="button" class="isleButton bgBlue" id="btnUpdateSelectedUser" onclick="um.updateSelectedUser();" value="Update" />
					<input type="button" class="isleButton bgRed" id="btnRemoveSelectedUser" onclick="um.removeSelectedUser();" value="Remove User" />
				</div>
				<div id="umSelectedUser_avatar"></div>
			</div>
      <h3>Select a user to manage:</h3>
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

</div><!-- /userManagerPanel -->