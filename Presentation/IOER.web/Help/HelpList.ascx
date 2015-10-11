<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="HelpList.ascx.cs" Inherits="IOER.Help.HelpList" %>
<link rel="stylesheet" type="text/css" href="/styles/common2.css" />

<script type="text/javascript">
</script>

<style type="text/css">
	/* Big Stuff */
	#tabs h2 { font-size: 35px; border-left: 15px solid #4F4E4F; padding-left: 5px; }
	#tabs h3 { font-size: 20px; }
	.iblock { display: inline-block; vertical-align: top; }

	/* Menu */
	#helpMenu { width: 300px; margin-bottom: 25px; }
	#helpMenu a { display: block; padding: 5px 10px; margin: 0 -10px; transition: background-color 0.2s, color 0.2s; }
	#helpMenu a:hover, #helpMenu a:focus { background-color: #DDD; color: #000; }

	/* Tabs */
	#tabs { width: calc(100% - 300px); padding: 0 10px; }
	.description { }
	.helpContent { width: calc(100% - 400px); padding: 5px; margin-bottom: 25px; }
	.tagline { padding: 5px; display: block; }
	
	/* Media */
	.helpMedia { width: 400px; }
	.mediaBox { position: relative; margin-bottom: 15px; border: 1px solid #CCC; border-radius: 5px; overflow: hidden; }
	.mediaBox img { width: 100%; height: auto; }
	.mediaBox iframe { display: block; width: 100%; height: 100%; position: absolute; top: 0; left: 0; }

	@media (min-width: 1400px) {
		.helpContent { width: 60%; }
		.helpMedia { width: 40%; }
	}
	@media (max-width: 1000px) {
		.helpContent, .helpMedia { width: 100%; display: block; }
		.helpMedia { text-align: center; }
	}
	@media (max-width: 675px ) {
		#helpMenu, #tabs { width: 100%; display: block; }
	}
</style>

<div id="content">
	<h1 class="isleH1">Illinois Open Educational Resources User Guide</h1>
	<div id="helpMenu" class="grayBox iblock">
		<h2 class="header">Topics</h2>
		<% foreach(var item in Data) { %>
		<a href="/help/guide/<%=item.TabName %>" class="<%=item.TabName == HelpItem.TabName ? "current" : "" %>"><%=item.Title %></a>
		<% } %>
	</div><!--
	--><div id="tabs" class="iblock">
		<div class="tab" data-tab="<%=HelpItem.TabName %>">
			<h2><%=HelpItem.Title %></h2>
			<div class="helpContent iblock">
				<i class="tagline"><%=HelpItem.Tagline %></i>
				<p class="description"><%=HelpItem.Description %></p>
				<% if(HelpItem.Downloads.Count() > 0){ %>
					<h3>Downloads</h3>
					<ul>
					<% foreach(var dl in HelpItem.Downloads) { %>
						<li><a href="<%=dl.Url %>" target="_blank"><%=dl.Title %></a></li>
					<% } %>
					</ul>
				<% } %>
				<% if(HelpItem.Links.Count > 0) { %>
					<h3>Links</h3>
					<ul>
					<% foreach(var link in HelpItem.Links) { %>
						<li><a href="<%=link.Url %>" target="_blank"><%=link.Title %></a></li>
					<% } %>
					</ul>
				<% } %>
			</div><!--
			--><div class="helpMedia iblock">
				<% foreach(var media in HelpItem.Media) { %>
				<div class="mediaBox" title="Embedded Media: <%=media.Title %>" tabindex="0">
          <img alt="" style="width:<%=media.Proportion %>%;" src="/images/youtube-autoresizer.png" />
          <iframe src="<%=media.Url %>" frameborder="0" allowfullscreen></iframe>
				</div>
				<% } %>
			</div>
		</div>
	</div>
</div>