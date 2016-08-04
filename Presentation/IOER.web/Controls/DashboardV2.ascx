<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DashboardV2.ascx.cs" Inherits="IOER.Controls.DashboardV2" %>

<div id="ErrorBox" runat="server" visible="false">
	<p><asp:Literal ID="ErrorMessage"></asp:Literal></p>
</div>
<div id="DashboardBox" runat="server" visible="true">

	<script type="text/javascript">

	</script>
	<style type="text/css">
		#content { min-height: 500px; }
		#leftBar, #rightBar { display: inline-block; vertical-align: top; }
		#leftBar { width: 300px; }
		#rightBar { width: calc(100% - 300px); padding-left: 10px; }
		#tabButtons input { display: inline-block; vertical-align: top; width: 20%; white-space: normal; border-radius: 0; }
		#tabButtons input:first-child { border-radius: 5px 0 0 5px; }
		#tabButtons input:last-child { border-radius: 0 5px 5px 0; }
		.row h3 { border-left: 10px solid #4F4E4F; padding: 5px; font-size: 20px; }
	</style>

	<div id="content">
		<h1 class="isleH1">[User Name]'s Dashboard</h1>

		<div id="leftBar">
			<div id="profileBox" class="grayBox">
				<h2 class="header">[User Name]</h2>

			</div><!--/profileBox -->
		</div><!-- /leftBar --><!--

		--><div id="rightBar">
			<div id="tabButtons">
				<input type="button" class="isleButton bgBlue" value="Overview" /><!--
				--><input type="button" class="isleButton bgBlue" value="My Library" /><!--
				--><input type="button" class="isleButton bgBlue" value="Library Memberships" /><!--
				--><input type="button" class="isleButton bgBlue" value="My Resources" /><!--
				--><input type="button" class="isleButton bgBlue" value="Following" />
			</div><!-- /tabButtons -->
			<div id="tabsBox">

				<div class="tab" data-tabID="overview">
					<h2 class="isleH2">Overview</h2>

					<div class="row">
						<h3>My Library</h3>
						<div class="list">
							<div class="listItem">
								<div class="listItemImage"></div>
								<div class="listItemTitle">Title Here</div>
							</div>
							<div class="listItem">
								<div class="listItemImage"></div>
								<div class="listItemTitle">Title Here</div>
							</div>
						</div>
					</div>
					<div class="row">
						<h3>Library Memberships</h3>
						<div class="list">
							<div class="listItem">
								<div class="listItemImage"></div>
								<div class="listItemTitle">Title Here</div>
							</div>
							<div class="listItem">
								<div class="listItemImage"></div>
								<div class="listItemTitle">Title Here</div>
							</div>
						</div>
					</div>
					<div class="row">
						<h3>My Resources</h3>
						<div class="list">
							<div class="listItem">
								<div class="listItemImage"></div>
								<div class="listItemTitle">Title Here</div>
							</div>
							<div class="listItem">
								<div class="listItemImage"></div>
								<div class="listItemTitle">Title Here</div>
							</div>
						</div>
					</div>
					<div class="row">
						<h3>Following</h3>
						<div class="list">
							<div class="listItem">
								<div class="listItemImage"></div>
								<div class="listItemTitle">Title Here</div>
							</div>
							<div class="listItem">
								<div class="listItemImage"></div>
								<div class="listItemTitle">Title Here</div>
							</div>
						</div>
					</div>

				</div><!-- /overview -->
			</div><!-- /tabsBox -->
		</div><!-- /rightBar -->

	</div><!-- /content -->

</div><!-- /Dashboard Box -->