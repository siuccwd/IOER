<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UberTaggerV3.ascx.cs" Inherits="IOER.Controls.UberTaggerV3.UberTaggerV3" %>
<%@ Register TagPrefix="uc1" TagName="StandardsBrowser" Src="/Controls/StandardsBrowser7.ascx" %>

<script type="text/javascript">
	//Server and global variables
	var SB7mode = "tag";
	var curriculumMode = true;
	var preselectedKeywords = <%=serializer.Serialize( Resource.Keywords ) %>;
  var preselectedStandards = <%=serializer.Serialize( Resource.Standards ) %>;
	var addedKeywords = []
	var myLibraries = <%=LibColData %>;
  var resourceID = <%=Resource.ResourceId %>;
	var versionID = <%=Resource.VersionId %>;
	var contentID = <%=Resource.ContentId %>;
	var updateMode = resourceID > 0;
</script>
<script type="text/javascript">
	//Initialization
	$(document).ready(function() {
		removeAddThis();
		setupUsageRights();
	});

	//Remove addthis
	function removeAddThis(){
		if($("[class*=at4]").length == 0){
			setTimeout(removeAddThis, 100);
		}
		else {
			$("[class*=at4]").remove();
		}
	}

	//Setup Usage Rights
	function setupUsageRights() {
		var box = $("#usageRightsSelectorBox");
		var ddl = box.find("#ddlUsageRights");
		var link = box.find("#usageRightsLink");
		var urlBox = $("#txtUsageRightsUrl");
		ddl.on("change", function () {
			var data = ddl.find("option:selected");
			link.attr("href", data.attr("data-url"));
			link.html(data.attr("data-description"));
			box.css("background-image", "url('" + data.attr("data-icon") + "')");
			urlBox.val(data.attr("data-url"));
			if (data.attr("data-iscustom") == "true") {
				urlBox.show();
			}
			else {
				urlBox.hide();
			}
			urlBox.trigger("change");
		});
		ddl.trigger("change");
		setTimeout(function() { $("#txtUsageRightsUrl").trigger("change"); }, 500);
	}

</script>
<script type="text/javascript">
	//Page Methods

	//Toggle visibility of tags
	function toggleCollapse(schema){
		var box = $(".tagList[data-schema=" + schema + "]");
		var button = $(".tagHeader[data-schema=" + schema + "]");
		var buttonText = button.attr("value").replace("+ ", "").replace("- ", "");
		if(box.attr("data-visible") == "false"){
			box.attr("data-visible", "true");
			button.attr("data-visible", "true").attr("value", "- " + buttonText);
		}
		else {
			box.attr("data-visible", "false");
			button.attr("data-visible", "false").attr("value", "+ " + buttonText);
		}
	}
</script>
<script type="text/javascript">
	//AJAX and success Methods

</script>
<script type="text/javascript">
	//Rendering Methods

</script>
<script type="text/javascript">
	//Validation Methods

</script>
<script type="text/javascript">
	//Miscellaneous Methods

</script>

<style type="text/css">
	/* Big Stuff*/
	#content { padding: 0 10px; }
	input[type=text], select, textarea { width: 100%; display: block; }

	/* Steps */
	#steps { max-width: 1400px; margin: 0 auto; }
	.step { margin-bottom: 25px; padding: 0 10px; }
	.stepHeader { font-size: 24px; background-color: #DDD; border-radius: 5px; padding: 2px 5px; margin: 0 -10px 10px -10px; }
	.instructions, .inputs { display: inline-block; vertical-align: top; width: 50%; }
	.instructions { padding-left: 15px; }
	.item { margin-bottom: 35px; padding-top: 5px; border-top: 1px solid #CCC; }
	.step .item:first-of-type { border: none; }
	.data { margin-bottom: 10px; }
	.inputs h3, .tagField h3, .itemHeader { font-size: 20px; }
	#usageRightsSelectorBox { padding-left: 100px; background: url('') no-repeat left 5px center; clear: both; }
  #usageRightsLink { padding: 2px 5px 5px 5px; }

	/* Standards */
	.preselectedStandard { background-color: #EEE; border-radius: 5px; padding: 2px 5px; margin-bottom: 5px; }
	.preselectedStandard .psNotationCode { font-weight: bold; display: inline-block; vertical-align: top; padding: 2px 15px 2px 5px; min-width: 25%; }
	.preselectedStandard .psAlignment { display: inline-block; vertical-align: top; padding: 2px 15px; float: right; font-style: italic; }

	/* Tags */
	.bigInstructions { text-align: center; padding: 10px; font-size: 18px; }
	.tagHeader { font-size: 20px; border: none; background-color: transparent; display: block; padding: 2px 5px; transition: background-color 0.2s; font-weight: bold; display: block; width: 100%; text-align: left; border-radius: 0 0 5px 5px; }
	.tagHeader:hover, .tagHeader.focus { background-color: #EEE; cursor: pointer; }
	.tagField { margin-bottom: 15px; border-top: 1px solid #CCC; }
	.tagField .selectLinks { font-size: 16px; text-align: center; padding: 0 5px 10px 0; }
	.tagList { padding: 0 5px; font-size: 0; }
	.tagList label { display: inline-block; vertical-align: top; padding: 2px 5px; border-radius: 5px; transition: background 0.2s; width: 33.333%; font-size: 16px; }
	.tagList label:hover, label:focus { background-color: #EEE; cursor: pointer; }
	.tagList[data-visible=true] { display: block; }
	.tagList[data-visible=false] { display: none; }

	/* Extras */
	.library { margin-bottom: 10px; position: relative; padding-left: 160px; min-height: 175px; }
	.library h4 { font-size: 18px; }
	.libraryAvatar, .collectionAvatar { background-color: #CCC; background-position: center center; background-size: contain; background-repeat: no-repeat; }
	.libraryAvatar { width: 150px; height: 150px; border-radius: 5px; border: 1px solid #CCC; position: absolute; top: 0; left: 0; }
	.library .collections { font-size: 0; }
	.library .collections label { border-radius: 5px; border: 1px solid #CCC; padding: 5px 45px 5px 25px; position: relative; margin: 1px 1%; display: inline-block; vertical-align: top; width: 48%; font-size: 16px; transition: background-color 0.2s; }
	.library .collections label:hover, .library .collections label:focus { background-color: #EEE; cursor: pointer; }
	.library .collections label input { position: absolute; left: 5px; top: 8px; }
	.library .collections label .collectionAvatar { position: absolute; top: 0; right: 0; border-radius: 0 5px 5px 0; width: 40px; height: 100%; }

	/* Finish */
	#globalStatus { padding: 10px; font-size: 20px; text-align: center; }
	#btnFinish { font-size: 30px; }

	/* Responsive */
	.fullscreen { display: inline; }
	.mobile { display: none; }

	@media (max-width: 700px) {
    .fullscreen { display: none; }
    .mobile { display: inline; }
	}

</style>

<div id="content">
	<h1 class="isleH1">IOER Resource Tagger</h1>

	<div id="steps">
		<div class="step" id="urlOrFiles">
			<h2 class="stepHeader">Step 1. The Resource</h2>
			<p class="bigInstructions">First, provide a URL to the resource or upload a file.</p>

			<!-- URL and File Upload -->
			<div class="item">
				<div class="inputs">
					<div class="data">
						<h3>Resource URL</h3>
						<input type="text" data-schema="Url" placeholder="http://" title="Resource URL" />
						<div class="validationMessage" data-schema="Url" data-valid="neutral"></div>
					</div><!--
					--><div class="data">
						<h3>Upload a File</h3>
						<p>TODO: Add this</p>
					</div>
				</div><div class="instructions">
					<p>The URL is the address of the resource on the Internet.</p>
					<p>This item is required.</p>
					<ul>
						<li>If the resource is a web page, just paste its URL into the box <span class="fullscreen">to the left</span><span class="mobile">above</span>.</li>
						<li>If the resource is a file online that you can link to, paste the URL to the file in the box <span class="fullscreen">to the left</span><span class="mobile">above</span>. Make sure it is publicly accessible.</li>
						<li>If the resource is a file that does <b>not</b> exist online in a publicly-accessible location, use the "Upload a File" option <span class="fullscreen">to the right</span><span class="mobile">below</span> and click the Upload button. We'll host the file and provide the URL for you.</li>
						<li>You may upload most file types (up to 25MB) except for executables and other potentially dangerous files. All files uploaded are subject to a virus scan.</li>
						<li style="color:#D33;">Once you <b>publish</b> a file, you may update the file, but <u>only</u> if the replacement has the same name and extension.</li>
					</ul>
				</div>
			</div><!-- /URL and File upload -->

		</div><!-- /urlOrFiles -->

		<div class="step" id="basicFields">
			<h2 class="stepHeader">Step 2. Basic Information</h2>
			<p class="bigInstructions">Provide basic information about the resource.</p>

			<!-- Title -->
			<div class="item">
				<div class="inputs">
					<h3>Title</h3>
					<input type="text" data-schema="Title" title="Resource Title" />
					<div class="validationMessage" data-schema="Title" data-valid="neutral"></div>
				</div><!--
				--><div class="instructions">
					<p>Enter a descriptive title for the resource.</p>
					<p>This item is required.</p>
				</div>
			</div>

			<!-- Description -->
			<div class="item">
				<div class="inputs">
					<h3>Description</h3>
					<textarea data-schema="Description" title="Resource Description"></textarea>
					<div class="validationMessage" data-schema="Description" data-valid="neutral"></div>
				</div><!--
				--><div class="instructions">
					<p>Enter a meaningful description of the resource.</p>
					<p>This item is required.</p>
				</div>
			</div>

			<!-- Keywords -->
			<div class="item">
				<div class="inputs">
					<h3>Keywords</h3>
					<input type="text" data-schema="Keywords" title="Type a keyword or phrase and press Enter." placeholder="Type a keyword or phrase and press Enter." />
					<div class="validationMessage" data-schema="Keywords" data-valid="neutral"></div>
					<div id="newKeywords"></div>
					<div id="existingKeywords" style="display:none;">
						<h3>These keywords have already been added to this resource:</h3>
						<div id="existingKeywordsList"></div>
					</div>
				</div><!--
				--><div class="instructions">
					<p>Enter up to 25 keywords to help other users find the resource. Use meaningful words or phrases that directly relate to your resource.</p>
					<p>This item is required.</p>
				</div>
			</div>

			<!-- Usage Rights -->
			<div class="item">
				<div class="inputs">
					<h3>Usage Rights</h3>
					<div id="usageRightsSelectorBox">
						<select id="ddlUsageRights" data-schema="UsageRights.Url">
							<% foreach(var item in UsageRights) { %>
								<option value="<%=item.CodeId %>" data-url="<%=item.Url %>" data-description="<%=item.Description %>" data-iscustom="<%=item.Custom ? "true" : "false" %>" data-isunknown="<%=item.Unknown ? "true" : "false" %>" data-icon="<%=item.IconUrl %>" <% if(item.CodeId == Resource.UsageRights.CodeId || (Resource.ResourceId == 0 && item.Unknown) ) { %> selected="selected" <% } %>><%=item.Title %></option>
							<% } %>
						</select>
						<a id="usageRightsLink" href="#"></a>
					</div>
					<input type="text" id="txtUsageRightsUrl" data-schema="UsageRights.Url" placeholder="http://" value="<%=Resource.UsageRights.Url %>" />
					<div class="validationMessage" data-schema="UsageRights.Url" data-valid="neutral"></div>
				</div><!--
				--><div class="instructions">
					<p>Usage Rights describes legal restrictions on using, altering, and/or republishing the resource.</p>
					<p>This item is required.</p>
					<ul>
						<li>If you own the resource you're tagging, or you know which Creative Commons license it uses, select the appropriate license.</li>
						<li>If an appropriate license isn't available, but you have a URL that links to the appropriate license, select "Read the Fine Print" and enter the URL in the box provided.</li>
						<li>If you don't know which license is appropriate, select "Rights Unknown".</li>
					</ul>
				</div>
			</div>

			<!-- Access Rights -->
			<% if( CurrentTheme.VisibleSingleValueFields.Contains( "AccessRights" ) ) {  %>
				<% var targetField = CurrentTheme.VisibleTagData.FirstOrDefault( m => m.Schema == "accessRights" ); %>
				<% var selectedAccess = 0; try { selectedAccess = Resource.Fields.Where( f => f.Schema == "accessRights" ).FirstOrDefault().Tags.Where( t => t.Selected ).FirstOrDefault().Id; } catch { } %>
				<div class="item">
					<div class="inputs">
						<h3>Access Rights</h3>
						<select id="ddlAccessRights" data-schema="AccessRights">
							<% foreach(var item in targetField.Tags){ %>
								<option value="<%=item.Id %>" <% if(item.Id == selectedAccess) { %> selected="selected" <% } %>><%=item.Title %></option>
							<% } %>
						</select>
					</div><!--
					--><div class="instructions">
						<p>Access Rights describe any applicable rights the user must have in order to access the Resource.</p>
						<p>For example, if the user must login or subscribe to content to have the right to access it, select the appropriate option.</p>
					</div>
				</div>
			<% } %>

			<!-- Creator -->
			<div class="item">
				<div class="inputs">
					<h3>Creator</h3>
					<input type="text" data-schema="Creator" title="Enter the name of the person or entity that created the resource" />
					<div class="validationMessage" data-schema="Creator" data-valid="neutral"></div>
				</div><!--
				--><div class="instructions">
					<p>Enter the name of the person or entity that originally created the resource itself.</p>
				</div>
			</div>

			<!-- Publisher -->
			<div class="item">
				<div class="inputs">
					<h3>Publisher</h3>
					<input type="text" data-schema="Publisher" title="Enter the name of the entity that publishes, hosts, or otherwise makes available the resource." />
					<div class="validationMessage" data-schema="Publisher" data-valid="neutral"></div>
				</div><!--
				--><div class="instructions">
					<p>Enter the name of the entity that publishes, hosts, or otherwise makes available the resource. For example:</p>
					<ul>
						<li>For a book, this would be the publisher indicated by the book itself.</li>
						<li>For a webpage, this would be the person or entity that owns the site that hosts the resource.</li>
					</ul>
				</div>
			</div>

			<!-- Requirements -->
			<div class="item">
				<div class="inputs">
					<h3>Technical/Equipment Requirements</h3>
					<input type="text" data-schema="Requirements" title="If the resource requires any specific device, software, or equipment, list it here." />
					<div class="validationMessage" data-schema="Requirements" data-valid="neutral"></div>
				</div><!--
				--><div class="instructions">
					<p>If the resource requires any specific device, software, or equipment, list it here. For example:</p>
					<ul>
						<li>If the resource works in any web browser, you don't need to list "web browser". However, if the resource <i>only</i> works in a specific browser, list it here.</li>
						<li>If the resource requires specific or uncommon device or operating system, list it here.</li>
						<li>If the resource requires specialized equipment like microscopes or basketball hoops, list it here.</li>
					</ul>
				</div>
			</div>

		</div><!-- /basicFields -->

		<div class="step" id="standards">
			<h2 class="stepHeader">Step 4. Learning Standards</h2>
			<p class="bigInstructions">Select the learning standards, if any, that apply to this resource.</p>
			<uc1:StandardsBrowser ID="standardsBrowser" runat="server" />
      <input type="hidden" class="hdnStandards" id="hdnStandards" runat="server" />
			<% if(Resource.Standards.Count() > 0) { %>
			<h4>These standards have already been added:</h4>
			<div id="preselectedStandards">
				<% foreach(var item in Resource.Standards ) { %>
				<div class="preselectedStandard" data-id="<%=item.StandardId %>">
					<div class="psAlignment"><%=item.AlignmentDegree %></div><!--
					--><div class="psNotationCode"><%=item.AlignmentType %> <%=item.NotationCode %></div>
					<div class="psDescription"><%=item.Description %></div>
				</div>
				<% } %>
			</div>
			<% } %>

		</div><!-- /standards -->

		<div class="step" id="tagFields">
			<h2 class="stepHeader">Step 3. Tags</h2>
			<p class="bigInstructions">Select all tags below that apply to the resource. </p>
			<% var requiredFields = new List<string>() { "learningResourceType", "inLanguage" }; %>
			<% var skipTags = new List<string>() { "accessRights", "usageRights" }; %>
			<% var selectedTags = Resource.Fields.SelectMany( t => t.Tags ).Where( t => t.Selected ).Select( t => t.Id ).ToList(); %>
			<% CurrentTheme.VisibleTagData = CurrentTheme.VisibleTagData.OrderBy( m => !requiredFields.Contains( m.Schema ) ).ToList(); %>
			<% foreach(var item in CurrentTheme.VisibleTagData) { %>
				<% if ( skipTags.Contains(item.Schema) ) { continue; } %>
				<div class="tagField" data-required="<%=(requiredFields.Contains(item.Schema) ? "required" : "") %>" data-schema="<%=item.Schema %>" data-title="<%=item.Title %>">
					<input type="button" class="tagHeader" value="+ <%=item.Title %>" data-visible="false" data-schema="<%=item.Schema %>" onclick="toggleCollapse('<%=item.Schema %>');" />
					<div class="tagList" data-visible="false" data-schema="<%=item.Schema %>">
						<% if(item.Schema == "gradeLevel") { %>
							<div class="selectLinks">
								<a href="javascript:selectTags('gradeLevel', 'elementary'); return false;">Select all Elementary</a>
									| 
								<a href="javascript:selectTags('gradeLevel', 'high school'); return false;">Select all High School</a>
							</div>
						<% } %>
						<% foreach(var tag in item.Tags){ %>
							<label>
								<input type="checkbox" data-category="<%=item.Id %>" data-id="<%=tag.Id %>" name="cbx_<%=tag.Id %>" <% if(selectedTags.Contains(tag.Id)) { %> checked="checked" disabled="disabled" <% } %> />
								<span> <%=tag.Title %></span>
							</label>
						<% } %>
					</div>
				</div>
      <% } %>

		</div><!-- /tagFields -->

		<div class="step" id="siteStuff">
			<h2 class="stepHeader">Step 5. Optional Extras</h2>
			<p class="bigInstructions">Select additional options.</p>

			<!-- Access Limitations -->
			<div class="item">
				<div class="inputs">
					<h3>Uploaded Content Access Limitations</h3>
					<select id="ddlPrivilegeType" data-schema="PrivilegeId">
						<% foreach(var item in ContentPrivileges){ %>
						<option value="<%=item.Id %>" <% if( Resource.PrivilegeId == item.Id) { %> selected="selected" <% } %>><%=item.Title %></option>
						<% } %>
					</select>
					<div class="validationMessage" data-schema="PrivilegeId" data-valid="neutral"></div>
				</div><!--
				--><div class="instructions">
					<p>Uploaded Content Access Limitations determine how user-created uploaded content can be accessed by other users within IOER.</p>
					<p><b>This field does not apply to resources tagged as URLs, only to content uploaded with the option at the top of this tagger.</b></p>
				</div>
			</div>

			<% if(OrganizationData.Count() > 0) { %>
			<!-- Organization -->
			<div class="item">
				<div class="inputs">
					<h3>Tag on Behalf of an Organization</h3>
					<select id="ddlOrganization">
						<option value="0">None (not tagging on behalf of an organization)</option>
						<% foreach(var item in OrganizationData){ %>
							<option value="<%=item.Id %>" <% if(item.Id == Resource.OrganizationId) { %> selected="selected" <% } %>><%=item.Organization %></option>
						<% } %>
					</select>				
				</div><!--
				--><div class="instructions">
					<p>If you are tagging this resource for an organization you belong to, select it here.</p>
				</div>
			</div>
			<% } %>

			<!-- Library and Collections -->
			<div class="item">
				<h3 class="itemHeader">Add this resource to your librar<%=(UserLibraries.Count() > 1 ? "ies" : "y") %></h3>
				<p class="bigInstructions">Select all of the collections you want to add this resource to once it's published.</p>
				<% foreach(var item in UserLibraries) { %>
					<div class="library" data-id="<%=item.Id %>">
						<div class="libraryAvatar" style="background-image: url('<%=item.ImageUrl %>');"></div><!--
						--><div class="libraryContent">
							<h4><%=item.Title %></h4>
							<div class="collections">
								<% foreach(var col in item.Collections) { %>
									<label class="collections">
										<input type="checkbox" data-value="<%=col.Id %>" /><span> <%=col.Title %></span><div class="collectionAvatar" style="background-image: url('<%=col.ImageUrl %>');"></div>
									</label>
								<% } %>
							</div>
						</div>
					</div>
				<% } %>
			</div>

		</div><!-- /siteStuff -->

		<div class="step" id="finish">
			<h2 class="stepHeader">Step 6. Finish</h2>
			<p class="bigInstructions">Review all of the information you have provided, and when you are done, click the Finish button.</p>
			<div id="globalStatus"></div>
			<input type="button" class="isleButton bgGreen" id="btnFinish" onclick="finish();" value="Finish!">

		</div><!-- /finish -->

	</div><!-- /steps -->

</div><!-- /content -->
