<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DetailV7.ascx.cs" Inherits="IOER.Controls.DetailV7.DetailV7" %>

<link rel="stylesheet" type="text/css" href="/styles/common2.css" />

<div id="detailPage" runat="server">

	<script type="text/javascript">
		/* Global Variables */
		var userLibraryData = <%=UserLibrariesJSON %>;
		var resourceLibraryData = <%=ResourceLibrariesJSON %>;
		var resource = <%=ResourceJSON %>;

	</script>
	<script type="text/javascript">
		/* Initialization */
		$(document).ready(function() {
			setupUserLibraries();
			renderResourceLibraries();
			renderComments();
		});

		//Switch displayed collections when user changes selected Library
		function setupUserLibraries() {
			if(userLibraryData == null) { 
				return; 
			}
			var ddl = $("#ddlLibraries");
			ddl.on("change", function() {
				renderUserCollections();
			});
			//No reason to show the DDL if the user only has a personal library
			if(userLibraryData.length == 1){
				ddl.find("option").last().prop("selected", true);
				ddl.trigger("change").hide();
			}
		}

	</script>
	<script type="text/javascript">
		/* Page Functions */
		//Show a tab in the interactive section
		function showTab(target) {
			$("#buttonBar input, #interactive .tab").removeClass("active").filter("[data-tab=" + target + "]").addClass("active");
		}

		//Add the Resource to selected collections
		function addToCollections(){
			var ids = [];
			var collections = $("#userCollections label input:checked").not("[disabled=disabled]").each(function() {
				ids.push(parseInt($(this).attr("data-id")));
			});
			console.log(ids);
			if(ids.length == 0){ return; }
			doAjax("AddResourceToCollectionsJSON", {resourceID: resource.ResourceId, collectionIDs: ids}, successAddToCollections, $("#btnAddToCollections"), null);
		}

		//Post a comment
		function postComment() {
			var text = $("#txtComment").val().trim().replace(/</g, "").replace(/>/g, "");
			if(text.length > 10){
				doAjax("PostCommentJSON", {resourceID: resource.ResourceId, text: text}, successPostComment, $("#btnPostComment"), null);
			}
			else {
				alert("Please enter a comment of meaningful length.");
			}
		}

		//Report an issue
		function reportIssue() {
			var text = $("#txtReportIssue").val().trim().replace(/</g, "").replace(/>/g, "");
			if(text.length > 5){
				doAjax("ReportIssueJSON", {resourceID: resource.ResourceId, text: text}, successReportIssue, $("#btnReportIssue"), null);
			}
			else {
				alert("Please enter a report of meaningful length.");
			}
		}
	</script>
	<script type="text/javascript">
		/* AJAX Functions */
		//Main AJAX handler
		function doAjax(method, data, success, button, extra){
			disableButton(button);
			$.ajax({
				url: "/services/DetailV7Service.asmx/" + method,
				headers: { "Accept": "application/json", "Content-type": "application/json; charset=utf-8" },
				data: JSON.stringify(data),
				dataType: "json",
				type: "POST",
				success: function (msg) {
					var message = msg.d ? msg.d : msg;
					var data = $.parseJSON(message);
					console.log("Returned Data", data);
					success(data, extra);
				} 
			}).done(function() { enableButton(button); });
		}
		//Disable button to prevent duplicate requests
		function disableButton(button){
			button.prop("disabled", "disabled").attr("oldValue", button.attr("value"));
		}
		//Reenable button
		function enableButton(button){
			button.prop("disabled", false).attr("value", button.attr("oldValue"));
		}

		/* AJAX Success Functions */
		//Add Resource to Library
		function successAddToCollections(data){
			if(data.valid){
				resourceLibraryData = data.data.resourceLibraryData;
				userLibraryData = data.data.userLibraryData;
				renderResourceLibraries();
				renderUserCollections();
			}
			else {
				alert(data.status);
			}
		}

		//Post a comment
		function successPostComment(data){
			if(data.valid){
				resource.Paradata.Comments = data.data;
				renderComments();
				$("#txtComment").val("");
			}
			else {
				alert(data.status);
			}
		}

		//Report an issue
		function successReportIssue(data){
			if(data.valid){
				alert("Thank you for your report. The IOER team has been notified.");
				$("#txtReportIssue").val("");
			}
			else {
				alert(data.status);
			}
		}
	</script>
	<script type="text/javascript">
		/* Rendering Functions */
		//Render user's collections
		function renderUserCollections(){ 
			var option = parseInt($("#ddlLibraries option:selected").attr("value"));
			var box = $("#userCollections");
			var template = $("#template_collectionItem").html();
			box.html("");
			for(var i in userLibraryData){
				var data = userLibraryData[i];
				if(data.Id == option){
					for(var j in data.Collections){
						var col = data.Collections[j];
						box.append(template
							.replace(/{id}/g, col.Id)
							.replace(/{title}/g, col.Title)
							.replace(/{image}/g, col.Image)
							.replace(/{disabled}/g, col.Contains ? "disabled=\"disabled\" checked=\"checked\"" : "")
						);
					}
				}
			}
		}

		//Render the libraries that the Resource is in
		function renderResourceLibraries() {
			var template = $("#template_libraryLink").html();
			var box = $("#inLibraries");
			box.html("");
			if(resourceLibraryData.length > 0){
				box.html("<h3>This Resource is in these Libraries:</h3>");
			}
			else {
				box.html("<p class=\"message\">This Resource has not been added to any Libraries yet.</p>");
			}
			for(var i in resourceLibraryData){
				var item = resourceLibraryData[i];
				box.append(template
					.replace(/{id}/g, item.Id)
					.replace(/{image}/g, item.Image)
					.replace(/{title}/g, item.Title)
				);
			}
		}

		//Render comments
		function renderComments() {
			var template = $("#template_comment");
			var box = $("#commentList");
			box.html("");
			for(var i in resource.Paradata.Comments){
				var item = resource.Paradata.Comments[i];
				box.append(template.html()
					.replace( "{id}", item.Id )
					.replace( "{date}", item.CreatedString )
					.replace( "{commenter}", item.CreatedBy )
					.replace( "{comment}", item.Comment )
				);
			}
		}
	</script>
	<style type="text/css">
		/* Big Stuff */
		#content h2 { font-size: 30px; color: #4F4E4F; border-bottom: 1px solid #CCC; margin-bottom: 5px; }
		#content h3 { font-size: 20px; color: #4F4E4F; }

		/* Major Page Parts */
		.iblock { display: inline-block; vertical-align: top; }
		#details { width: calc(100% - 402px); padding: 0 10px 50px 5px; }
		#interactive { width: 402px; }
		.section { margin-bottom: 5px; }
		.message { text-align: center; font-style: italic; opacity: 0.7; padding: 20px 5px; }
		.linkButton { display: inline-block; width: 100%; margin: 2px 0; padding: 3px 5px; text-align: center; border: 1px solid #CCC; }

		/* Metadata Items */
		#resourceTitle { text-align: center; }
		#resourceURL { text-align: center; font-size: 20px; font-weight: bold; display: block; word-break: break-all; }
		#lrDocLink { display: none; }
		#description p { font-size: 20px; }
		#singles { background-color: #F5F5F5; border-radius: 5px; padding: 5px; margin: 15px 5px 15px 5px; }
		#singles div { display: inline-block; width: 50%; padding: 2px 5px; }
		#tags #fields { column-count: 3; -webkit-column-count: 3; -moz-column-count: 3; }
		#tags .field { margin-bottom: 10px; display: inline-block; width: 100%; }
		#tags .field ul { margin-left: 25px; }

		/* Standards */
		#standards .standard { margin-bottom: 5px; border-radius: 5px; border: 1px solid #CCC; }
		#standards .standard .title { font-weight: bold; font-size: 18px; background-color: #F5F5F5; border-radius: 5px 5px 0 0; padding: 2px 5px; }
		#standards .standard .description { padding: 5px 10px; }
		#standards .standard .title .standardDegree { font-weight: normal; font-style: italic; padding-left: 5px; opacity: 0.8; }
		#standards .standard .rating { text-align: right; font-size: 14px; color: #555; padding: 2px 5px; }
		#standards .standard .rating i { font-size: 12px; }

		/* Keywords */
		#keywords a { display: inline-block; vertical-align: top; padding: 2px 5px; border-radius: 5px; border: 1px solid #4C98CC; margin: 2px 5px; transition: color 0.2s, background-color 0.2s; }
		#keywords a:hover, #keywords a:focus { background-color: #4C98CC; color: #FFF; }

		/* Thumbnail */
		#thumbnail { position: relative; overflow: hidden; border-radius: 5px; background-color: #CCC; font-size: 0; border: 1px solid #CCC; }
		#thumbnail #resizer { width: 100%; height: auto; }
		#thumbnail #thumbnailBox { position: absolute; top: 0; left: 0; width: 100%; height: 100%; }
		#thumbnail #thumbnailBox a { font-size: 0; display: block; }
		#thumbnail #thumbnailBox img { width: 100%; height: auto; display: block; }

		/* Paradata */
		#paradata { white-space: nowrap; margin-bottom: 0; }
		#paradata .paradataItem { text-align: center; display: inline-block; vertical-align: top; background: url('') no-repeat left center; background-color: #F5F5F5; border: 1px solid #CCC; padding: 5px 2px 5px 20px; margin: 0; width: 20%; height: 32px; }
		#paradata .paradataItem:first-child { border-radius: 5px 0 0 0; }
		#paradata .paradataItem:last-child { border-radius: 0 5px 0 0; }

		/* Likes and Dislikes */
		#btnLike, #btnDislike { display: inline-block; width: 15%; height: 32px; border: 1px solid #CCC; background-repeat: no-repeat; }
		#btnLike { border-radius: 0 0 0 5px; background-image: url('/images/icons/icon_likes_white.png'); background-position: left 5px center; }
		#btnDislike { border-radius: 0 0 5px 0; background-image: url('/images/icons/icon_dislikes_white.png'); background-position: right 5px center; }
		#likeBarOuter { width: 70%; height: 32px; border: 2px solid #CCC; border-width: 2px 0 2px 0; }
		#likeBar, #dislikeBar { color: #FFF; height: 28px; font-size: 16px; line-height: 28px; padding: 0 5px; }
		#likeBar { background-color: #4AA394; width: 50%; text-align: left; }
		#dislikeBar { background-color: #B03D25; width: 50%; text-align: right; }

		/* Tabs System */
		#buttonBar input { width: 20%; border: 1px solid #CCC; border: 0; border-radius: 0; height: 50px; background-repeat: no-repeat; background-position: center center; background-size: auto 100%; }
		#buttonBar input:first-child { border-radius: 5px 0 0 5px; }
		#buttonBar input:last-child { border-radius: 0 5px 5px 0; }
		#buttonBar #btnShowComments { background-image: url('/images/icons/icon_comments_white_med.png'); }
		#buttonBar #btnShowLibraries { background-image: url('/images/icons/icon_library_white_med.png'); }
		#buttonBar #btnShowRatings { background-image: url('/images/icons/icon_ratings_white_med.png'); }
		#buttonBar #btnShowTools { background-image: url('/images/icons/icon_myisle_white_med.png'); }
		#buttonBar #btnShowReport { background-image: url('/images/icons/icon_report_white_med.png'); }
		.tab { display: none; }
		.tab.active { display: block; }
		#buttonBar input.active { background-color: #9984BD; }

		/* Tabs Items */
		.tab textarea { width: 100%; height: 5em; max-height: 12em; min-height: 5em; display: block; resize: vertical; margin-bottom: 2px; }
		.tab select { width: 100%; }
		#toolButtons input { border-radius: 0; margin-bottom: 1px; padding: 5px 10px; }
		#toolButtons input:first-child { border-radius: 5px 5px 0 0; }
		#toolButtons input:last-child { border-radius: 0 0 5px 5px; }

		/* Comments */
		#commentsInput { margin-bottom: 5px; }
		#commentList .comment { border: 1px solid #CCC; border-radius: 5px; margin-bottom: 5px; }
		#commentList .comment .title { background-color: #F5F5F5; padding: 2px 5px; border-radius: 5px 5px 0 0; }
		#commentList .comment .title::after { display: block; height: 0; width: 0; content: " "; clear: both; }
		#commentList .comment .date { float: right; }
		#commentList .comment .description { padding: 5px; }

		/* Library/Collection Items */
		#ddlLibraries { margin-bottom: 5px; }
		#userCollections label { display: block; height: 40px; border: 1px solid #CCC; position: relative; overflow: hidden; padding-left: 25px; background-color: #F5F5F5; transition: background-color 0.2s; }
		#userCollections label:first-child { border-top-left-radius: 5px; border-top-right-radius: 5px; }
		#userCollections label:last-child { border-bottom-left-radius: 5px; border-bottom-right-radius: 5px; }
		#userCollections label input { position: absolute; left: 0; top: 0; height: 100%; line-height: 100%; margin: 0 5px; }
		#userCollections label img { height: 100%; display: inline-block; margin-left: 10px; float: right; }
		#userCollections label .title { line-height: 36px; }
		#userCollections label:hover, #userCollections label:focus { background-color: #EEE; cursor: pointer; }
		#btnAddToCollections { margin: 5px 0 10px 0; }
		#inLibraries { margin-bottom: 10px; text-align: center; }
		#inLibraries .libraryLink { display: inline-block; vertical-align: top; height: 75px; margin: 2px; }
		#inLibraries .libraryLink img { display: inline-block; height: 100%; }

		/* Ratings */
		#ratings .evaluation { margin-bottom: 5px; }
		#ratings .rating { border-radius: 5px; border: 1px solid #CCC; margin-bottom: 2px; }
		#ratings .rating .title { background-color: #F5F5F5; padding: 2px 5px; border-radius: 5px 5px 0 0; }
		#ratings .rating .scoreBarOuter { background-color: #C0C0C0; background-image: linear-gradient(rgba(0,0,0,0.3),rgba(0,0,0,0)); position: relative; height: 22px; overflow: hidden; border-radius: 0 0 5px 5px; padding: 1px; }
		#ratings .rating .scoreBarInner { background-color: #4AA394; height: 100%; border-radius: 0 0 5px 5px; }
		#ratings .rating .scoreBarText { position: absolute; top: 0; left: 0; width: 100%; height: 100%; text-align: center; color: #FFF; }
		#ratings .rating.overall .title { background-color: #CCC; font-weight: bold; }
		#ratings .rating.overall .scoreBarOuter { height: 30px; line-height: 30px; }
		#ratings .rating.overall .scoreBarText .percent { font-weight: bold; font-size: 22px; }
	</style>

	<div id="content">

		<h1 class="isleH1" id="resourceTitle"><%=Resource.Title %></h1>

		<div id="columns">

			<div id="details" class="iblock">
				<%-- Resource URL --%>
				<div class="section" id="url">
					<a itemprop="url" href="<%=Resource.Url %>" target="_blank" onclick="doResourceView();" id="resourceURL"><%=Resource.Url %></a>
					<a href="http://node01.public.learningregistry.net/harvest/getrecord?request_ID=<%=Resource.LrDocId %>&by_doc_ID=true" id="lrDocLink" target="_blank">View LR Document</a>
				</div>

				<%-- Description --%>
				<div class="section" id="description">
					<h2>Description</h2>
					<p itemprop="description"><%=Resource.Description %></p>
				</div>
			
				<%-- Requirements --%>
				<% if(!string.IsNullOrWhiteSpace(Resource.Requirements)){ %>
				<div class="section" id="requirements">
					<h2>Requirements</h2>
					<p itemprop="requirements"><%=Resource.Requirements %></p>
				</div>
				<% } %>

				<%-- Single Value Items --%>
				<div class="section" id="singles">
					<div><b>Creator:</b> <a itemprop="author" href='/search?text="<%=Resource.Creator %>"' target="_blank"><%=Resource.Creator %></a></div><!--
					--><div><b>Publisher:</b> <a itemprop="publisher" href='/search?text="<%=Resource.Publisher %>"' target="_blank"><%=Resource.Publisher %></a></div><!--
					--><div><b>Submitter:</b> <a itemprop="submitter" href='/search?text="<%=Resource.Submitter %>"' target="_blank"><%=Resource.Submitter %></a></div><!--
					--><div><b>Created on:</b> <span itemprop="dateCreated"><%=Resource.Created.ToShortDateString() %></span></div><!--
					--><div><b>Rights:</b> <a itemprop="useRightsUrl" href="<%=Resource.UsageRights.Url %>" target="_blank"><%=Resource.UsageRights.Title %></a></div>
				</div>

				<%-- Keywords --%>
				<div class="section" id="keywords">
					<h2>Keywords</h2>
					<% if(Resource.Keywords.Count() == 0){ %>
					<p class="message">This Resource does not have any keywords.</p>
					<% } else { %>
					<% foreach(var item in Resource.Keywords) { %>
					<a itemprop="keyword" href='/search?text=<%=item %>' target="_blank"><%=item %></a>
					<% } %>
					<% } %>
					<div id="hashtags" style="width: 0; height: 0; padding: 0; margin: 0; overflow: hidden;">
						<% foreach(var item in Resource.Keywords) { %>
						<%=( "#" + item.Replace( " ", "" ).Replace("\"", "").Replace("#", "").Replace("'", "").Trim() + " " ) %>
						<% } %>
					</div>
				</div>

				<%-- Tags --%>
				<div class="section" id="tags">
					<h2>Tags</h2>
					<div id="fields">
						<% foreach(var item in Resource.Fields){ %>
						<% var activeTags = item.Tags.Where( t => t.Selected ).ToList(); %>
						<% if(activeTags != null && activeTags.Count() > 0){ %>
						<div class="field">
							<h3 data-schema="<%=item.Schema %>"><%=item.Title %></h3>
							<ul>
							<% foreach(var tag in activeTags) { %>
								<li data-tagID="<%=tag.Id %>"><a itemprop="<%=item.Schema %>" href="/search?tagIDs=<%=tag.Id %>" target="_blank"><%=tag.Title %></a></li>
							<% } %>
							</ul>
						</div>
						<% } %>
						<% } %>
					</div>
				</div>

				<%-- Standards --%>
				<div class="section" id="standards">
					<h2>Learning Standards</h2>
					<% if(Resource.Standards.Count() == 0){ %>
					<p class="message">This Resource has not been aligned to any learning standards.</p>
					<% } else { %>
					<% foreach(var item in Resource.Standards){ %>
					<div itemprop="educationalAlignment" class="standard" data-standardID="<%=item.StandardId %>">
						<meta itemprop="alignmentType" content="<%=item.AlignmentType %>" />
						<meta itemprop="targetUrl" content="<%=item.StandardUrl %>" />
						<meta itemprop="targetName" content="<%=item.NotationCode %>" />
						<div class="title"><%=item.AlignmentType %> <a href='/search?text="<%=item.NotationCode %>"' target="_blank"><%=item.NotationCode %></a> <span class="standardDegree">(<%=item.AlignmentDegree %> alignment)</span></div>
						<div itemprop="targetDescription" class="description"><%=item.Description %></div>
						<% var rating = Resource.Paradata.StandardEvaluations.Where( t => t.ContextId == item.StandardId ).FirstOrDefault(); %>
						<% if(rating != null){  %>
						<% var strength = rating.ScorePercent > 75 ? "Superior" : rating.ScorePercent > 50 ? "Strong" : rating.ScorePercent > 25 ? "Poor" : "Very Weak"; %>
						<div class="rating" title="The community has rated this alignment as <%=strength %> (<%=Resource.Paradata.StandardEvaluations.Count() %> Ratings)">
							<b><%=strength %></b> <i>(<%=Resource.Paradata.StandardEvaluations.Count() %> Ratings)</i>
							<%-- foreach(var rating in Resource.Paradata.StandardEvaluations){ %>
							<b><%=rating.Score %></b> 
							<% } --%>
						</div>
						<% } %>
					</div>
					<% } %>
					<% } %>
				</div>

				<%-- More like this --%>
				<div class="section" id="morelikethis">
					<h2>More Like This</h2>
					<div id="moreLikeThisResults"></div>
				</div>

			</div><!-- /details --><!--
			--><div id="interactive" class="iblock">

				<%-- Thumbnail --%>
				<div class="section" id="thumbnail">
					<img alt="" id="resizer" src="/images/ThumbnailResizer.png" />
					<div id="thumbnailBox">
						<a href="<%=Resource.Url %>" onclick="doResourceHit();" target="_blank">
							<img alt="" src="//ioer.ilsharedlearning.org<%=Resource.ThumbnailUrl %>" />
						</a>
					</div>
				</div>

				<%-- Paradata --%>
				<div class="section" id="paradata">
					<div class="paradataItem" title="Resource visited <%=Resource.Paradata.ResourceViews %> times" style="background-image:url('/images/icons/icon_click-throughs.png')">
						<%=Resource.Paradata.ResourceViews %>
					</div><!--
					--><div class="paradataItem" title="Resource commented on <%=Resource.Paradata.Comments.Count() %> times" style="background-image:url('/images/icons/icon_comments.png');">
						<%=Resource.Paradata.Comments.Count() %>
					</div><!--
					<%-- <div class="paradataItem" title="<%=Resource.Paradata.Likes %> <%=Resource.Paradata.Likes == 1 ? "person likes" : "people like" %> this Resource" style="background-image:url('/images/icons/icon_likes.png')">
						<%=Resource.Paradata.Likes %>
					</div>
						<div class="paradataItem" title="<%=Resource.Paradata.Dislikes %> <%=Resource.Paradata.Dislikes == 1 ? "person dislikes" : "people dislike" %> this Resource" style="background-image:url('/images/icons/icon_dislikes.png')">
						<%=Resource.Paradata.Dislikes %>
					</div> --%>
					--><div class="paradataItem" title="This Resource is in <%=Resource.Paradata.Favorites %> Libraries" style="background-image:url('/images/icons/icon_library.png')">
						<%=Resource.Paradata.Favorites %>
					</div><!--
					--><div class="paradataItem" title="This Resource has been evaluated against <%=(Resource.Paradata.RubricEvaluations.Count() + Resource.Paradata.StandardEvaluations.Count()) %> rubric dimensions and/or learning standards" style="background-image:url('/images/icons/icon_standards.png')">
						<%=(Resource.Paradata.RubricEvaluations.Count() + Resource.Paradata.StandardEvaluations.Count()) %>
					</div><!--
					--><div class="paradataItem" title="This Resource has been evaluated <%=(Resource.Paradata.RubricEvaluations.Sum(t => t.TotalEvaluations) + Resource.Paradata.StandardEvaluations.Count()) %> times" style="background-image:url('/images/icons/icon_ratings.png')">
						<%=(Resource.Paradata.RubricEvaluations.Sum(t => t.TotalEvaluations) + Resource.Paradata.StandardEvaluations.Count()) %>
					</div>
				</div>

				<%-- Like/Dislike --%>
				<div class="section" id="likeDislike">
					<input type="button" class="isleButton bgGreen iblock" id="btnLike" onclick="addLike();" title="I like this Resource!" /><!--
					--><div id="likeBarOuter" class="iblock">
						<div id="likeBar" class="iblock" title="<%=Resource.Paradata.Likes %> Likes"><%=Resource.Paradata.Likes %></div><!--
						--><div id="dislikeBar" class="iblock" title="<%=Resource.Paradata.Dislikes %> Dislikes"><%=Resource.Paradata.Dislikes %></div>
					</div><!--
					--><input type="button" class="isleButton bgRed iblock" id="btnDislike" onclick="addDislike();" title="I dislike this Resource!" />
				</div>

				<%-- ButtonBar --%>
				<div class="section" id="buttonBar">
					<input type="button" id="btnShowComments" data-tab="comments" class="isleButton bgBlue active" onclick="showTab('comments');" title="Comments" /><!--
					--><input type="button" id="btnShowLibraries" data-tab="libraries" class="isleButton bgBlue" onclick="showTab('libraries');" title="Libraries" /><!--
					--><input type="button" id="btnShowRatings" data-tab="ratings" class="isleButton bgBlue" onclick="showTab('ratings');" title="Ratings" /><!--
					--><input type="button" id="btnShowTools" data-tab="tools" class="isleButton bgBlue" onclick="showTab('tools');" title="Tools" /><!--
					--><input type="button" id="btnShowReport" data-tab="report" class="isleButton bgBlue" onclick="showTab('report');" title="Report" />
				</div>

				<%-- Comments --%>
				<div class="tab section active" id="comments" data-tab="comments">
					<h2>Comments</h2>
					<% if(Permissions.IsLoggedIn){ %>
					<div id="commentsInput">
						<textarea id="txtComment"></textarea>
						<input type="button" id="btnPostComment" class="isleButton bgGreen" onclick="postComment();" value="Post" />
					</div>
					<% } else { %>
					<p class="message">Login to comment!</p>
					<% } %>
					<% if(Resource.Paradata.Comments.Count() == 0){ %>
					<p class="message">There are no comments on this Resource yet.</p>
					<% } %>
					<div id="commentList"></div>
				</div>

				<%-- Libraries --%>
				<div class="tab section" id="libraries" data-tab="libraries">
					<h2>Libraries</h2>
					<div id="inLibraries"></div>
					<% if(Permissions.IsLoggedIn) { %>
					<div id="libraryAddBox">
						<h3>Add this Resource to your Library:</h3>
						<select id="ddlLibraries">
							<option value="0">Select a Library...</option>
							<% foreach(var item in UserLibraries) { %>
							<option value="<%=item.Id %>"><%=item.Title %></option>
							<% } %>
						</select>
						<div id="userCollections"></div>
						<input type="button" class="isleButton bgGreen" id="btnAddToCollections" value="Add" onclick="addToCollections();" />
					</div>
					<% } else { %>
					<p class="message">Login to add this Resource to your Library!<br /><a href="/help/guide" target="_blank">Learn more about IOER Libraries</a></p>
					<% } %>
				</div>

				<%-- Ratings --%>
				<div class="tab section" id="ratings" data-tab="ratings">
					<h2>Ratings</h2>
					<% if(Permissions.IsLoggedIn){ %>
					<a class="linkButton isleButton bgGreen" href="/evaluate/<%=Resource.ResourceId %>">Evaluate this Resource</a>
					<% } else { %>
					<p class="message">Login to evaluate this Resource!<br /><a href="/help/guide" target="_blank">Learn more about IOER Rubrics and Resource Evaluation</a></p>	
					<% } %>
					<% if ( Resource.Paradata.RubricEvaluations.Count() == 0 ) { %>
					<p class="message">This Resource has not been evaluated yet.</p>
					<% } %>
					<% foreach(var rubric in RubricsList){ %>
						<% var targetRatings = Resource.Paradata.RubricEvaluations.Where( t => t.EvaluationId == rubric.RubricId ).ToList(); %>
						<% if(targetRatings.Count() > 0){ %>
							<div class="evaluation">
							<h3><%=rubric.Title %></h3>
							<div class="rating overall" data-rubricID="<%=rubric.RubricId %>">
								<div class="title">Overall Score</div>
								<div class="scoreBarOuter">
									<div class="scoreBarInner" style="width:<%=rubric.OverallScore %>%"></div>
									<div class="scoreBarText"><span class="percent"><%=rubric.GetOverallScoreWord() %> (<%=rubric.OverallScore %>%)</span></div>
								</div>
							</div>
							<% foreach(var item in targetRatings){ %>
							<div class="rating" data-ratingID="<%=item.EvaluationId %>" data-rubricID="<%=item.ContextId %>">
								<div class="title"><%=item.Title %></div>
								<div class="scoreBarOuter">
									<div class="scoreBarInner <%=item.TotalEvaluations == 0? "noratings" : "" %>" style="width:<%=item.TotalEvaluations > 0 ? item.ScorePercent.ToString() : "100" %>%;" ></div>
									<% if(item.TotalEvaluations > 0) { %>
									<div class="scoreBarText"><span class="percent"><%=item.ScorePercent %>%</span> (<%=item.TotalEvaluations %> Evaluations)</div>
									<% } else { %>
									<div class="scoreBarText noratings">No Ratings</div>
									<% } %>
								</div>
							</div>
							<% } %>
							</div>
						<% } %>
					<% } %>

				</div>

				<%-- Tools --%>
				<div class="tab section" id="tools" data-tab="tools">
					<h2>Tools</h2>
					<% if(Permissions.IsLoggedIn) { %>
					<div id="toolButtons">
						<input type="button" class="isleButton bgGreen" onclick="window.location.href = '/controls/ubertaggerv2?resourceID=<%=Resource.ResourceId %>';" value="Update This Data" />
						<% if(Permissions.IsIOERAdmin) { %>
						<input type="button" class="isleButton bgGreen" onclick="regenerateThumbnail();" value="Regenerate Thumbnail" />
						<% } %>
						<% if(Permissions.CanDelete) { %>
						<input type="button" class="isleButton bgRed" onclick="deactivate()" value="Deactivate Resource" />
						<% } %>
					</div>
					<% } else { %>
					<p class="message">Login to access IOER Resource Tools!<br /><a href="/help/guide" target="_blank">Learn more about IOER Tools</a></p>
					<% } %>
				</div>

				<%-- Report --%>
				<div class="tab section" id="report" data-tab="report">
					<h2>Report an Issue</h2>
					<% if(Permissions.IsLoggedIn) { %>
					<textarea id="txtReportIssue"></textarea>
					<input type="button" class="isleButton bgRed" id="btnReportIssue" onclick="reportIssue()" value="Report" />
					<% } else { %>
					<p class="message">Please login to report an issue.</p>
					<% } %>
				</div>

			</div><!-- /interactive -->

		</div><!-- /columns -->

	</div><!-- /content -->

	<div id="templates" style="display:none;">
		<script type="text/template" id="template_comment">
			<div class="comment" data-id="{id}">
				<div class="title"><div class="date">{date}</div>{commenter}</div>
				<div class="description">{comment}</div>
			</div>		
		</script>
		<script type="text/template" id="template_libraryLink">
			<a class="libraryLink" data-id="{id}" href="/Library/{id}" target="_blank" title="{title}">
				<img alt="" src="{image}" />
			</a>
		</script>
		<script type="text/template" id="template_collectionItem">
			<label><img alt="" src="{image}" /><input type="checkbox" data-id="{id}" {disabled}><div class="title">{title}</div></label>
		</script>

	</div>

</div>

<div id="errorPage" runat="server" style="text-align: center; padding: 50px;">
	Error: Invalid Resource
	<div style="display: none;">
		<%=ErrorMessage %>
	</div>
</div>

<div id="deactivatedPage" runat="server" style="text-align: center; padding: 50px;">
	That Resource was deactivated.
	<div id="reenableButton">
		<input type="submit" id="BtnReactivateResource" runat="server" class="isleButton bgGreen" value="Reactivate" />
	</div>
</div>

<div id="serverItems" runat="server" visible="false">
	<asp:Literal ID="txtGeneralSecurity" runat="server" Visible="false"></asp:Literal>
	<asp:Literal ID="txtFormSecurityName" runat="server" Visible="false">IOER.Pages.ResourceDetail</asp:Literal>
</div>
