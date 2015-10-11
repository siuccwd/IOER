<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ContentDisplayV2.ascx.cs" Inherits="IOER.Controls.Content.ContentDisplayV2" %>

<div id="contentWrapper" runat="server">
	<style type="text/css">
		/* Big Stuff */
		#content { min-height: 500px; }
		#contentSection, #dataSection { display: inline-block; vertical-align: top; }
		#contentSection { width: calc(100% - 350px); padding: 0 10px 50px 0; }
		#dataSection { width: 300px; padding: 0 0 50px; }
		#contentSection h2 { margin-top: 15px; }
		#contentSection h2.first { margin-top: 0; }

		/* Content */
		#contentSection h2 { font-size: 20px; }
		#contentSection #summary { padding: 10px; }
		#contentSection #thumbnail { float: right; margin: 10px 0 10px 10px; border-radius: 5px; border: 1px solid #CCC; }
		#contentSection #descriptionData { border: 1px solid #CCC; border-radius: 5px; padding: 5px; }
		#previewFrame { width: 100%; height: 100%; display: block; border-radius: 5px; border: 1px solid #CCC; background-color: #CCC; }
		#previewFrameWrapper { width: 100%; height: 1000px; }
		#downloadLink { font-size: 20px; font-weight: bold; text-align: center; display: block; padding: 5px; }
		#content .standard, #content .supplement, #content .reference { padding: 5px; margin-bottom: 5px; }
		#content #noAccess { font-size: 20px; color: #D33; font-weight: bold; text-align: center; padding: 50px; }

		/* Data */
		#dataSection .mid:first-of-type { margin-top: -5px; }
		#dataSection .owner { display: block; position: relative; padding: 5px 55px 0 0; min-height: 50px; }
		#dataSection .ownerAvatar { position: absolute; top: 0; right: 0; display: inline-block; width: 50px; height: 50px; vertical-align: middle; border-radius: 5px; background: #CCC no-repeat center center; background-size: cover; border: 1px solid #CCC; }
		#dataSection .useRights { display: block; position: relative; padding: 5px 95px 0 0; min-height: 35px; }
		#dataSection .useRights img { position: absolute; top: 0; right: 0; }
		#dataSection .learningList { display: block; position: relative; padding: 5px 105px 0 0; min-height: 60px; }
		#dataSection .learningList img { position: absolute; top: -15px; right: 0; width: 100px; border-radius: 5px; }
	</style>

    <div id="content">

        <h1 class="isleH1"><%=Content.Title %></h1>
        <asp:Panel ID="notAllowedPanel" runat="server" Style="text-align:center;" Visible="false">
            <h2>Private Content</h2>
            <p>This content has been designated as not available for public viewing.</p>
            <asp:Label ID="lblContentState" runat="server" />
        </asp:Panel>
        <asp:Panel ID="detailPanel" runat="server" Visible="false">
            <div id="contentSection">
                <h2 class="first">Summary</h2>
                <% if ( !string.IsNullOrWhiteSpace( Content.ImageUrl ) && string.IsNullOrWhiteSpace( Content.DocumentUrl ) && string.IsNullOrWhiteSpace( Content.Description ) )
                   { %>
                <img id="thumbnail" src="<%=Content.ImageUrl %>" alt="Content preview thumbnail" />
                <% } %>
                <div id="summary"><%=Content.Summary %></div>

                <% if ( ContentIsVisible )
                   { %>
                <% if ( !string.IsNullOrWhiteSpace( Content.Description ) && Content.Description != Content.Summary )
                   { %>
                <div id="description">
                    <h2>Content Data</h2>
                    <div id="descriptionData">
                        <%=Content.Description %>
                    </div>
                </div>
                <% } %>
                <% if ( !string.IsNullOrWhiteSpace( Content.DocumentUrl ) )
                   { %>
                <h2>File</h2>
                <a id="downloadLink" href="<%=Content.DocumentUrl %>">Download this File</a>

                <div id="previewFrameWrapper">
                    <iframe id="previewFrame" src="<%=ContentPreviewUrl %>"></iframe>
                </div>
                <% } %>
                <% }
                   else
                   { %>
                <div id="noAccess">You are not authorized to access this content.</div>
                <% } %>

                <% if ( Content.Standards.Count() > 0 )
                   { %>
                <h2>Learning Standards</h2>
                <% foreach ( var item in Content.Standards )
                   { %>
                <div class="standard grayBox">
                    <div class="code"><%=item.NotationCode %></div>
                    <div class="description"><%=item.Description %></div>
                    <div class="alignmentInfo">
                        <div class="alignmentType">This content <%=item.AlignmentType %> this standard.</div>
                        <div class="alignmentDegree">This alignment degree is <%=item.AlignmentDegree %>.</div>
                    </div>
                </div>
                <% } %>
                <% } %>

                <% if ( Supplements != null && Supplements.Count() > 0 )
                   { %>
                <h2>Supplementary Content</h2>
                <% foreach ( var item in Supplements )
                   { %>
                <div class="supplement grayBox">
                    <a href="<%=item.ResourceUrl %>" target="_blank" class="title"><%=item.Title %></a>
                    <div class="description"><%=item.Description %></div>
                </div>
                <% } %>
                <% } %>

                <% if ( References != null && References.Count() > 0 )
                   { %>
                <h2>References</h2>
                <% foreach ( var item in References )
                   { %>
                <div class="reference grayBox">
                    <a href="<%=item.ReferenceUrl %>" target="_blank" class="title"><%=item.Title %></a>
                    <div class="description"><%=item.AdditionalInfo %></div>
                </div>
                <% } %>
                <% } %>
            </div>
            <!-- /contentSection -->
            <!--
		-->
            <div id="dataSection">

                <div id="data" class="grayBox">
                    <h2 class="header">Content Information</h2>

                    <h3 class="mid">Author</h3>
                    <a href="/Profile/<%=Owner.Id %>" target="_blank" class="owner">
                        <div><%=Owner.FullName() %></div>
                        <% var OwnerMembership = Owner.OrgMemberships.Where( m => m.Id == Content.OrgId ).FirstOrDefault(); %>
                        <% if ( OwnerMembership != null )
                           { %>
                        <div><%=OwnerMembership.OrgMemberType %></div>
                        <% }
                           else
                           { %>
                        <div>IOER Member</div>
                        <% } %>
                        <div class="ownerAvatar" style="background-image: url('<%=Owner.ImageUrl %>');"></div>
                    </a>

                    <% if ( Content.ResourceIntId > 0 )
                       { %>
                    <h3 class="mid">Resource Tags</h3>
                    <a href="<%=Content.ResourceFriendlyUrl %>">View Resource Tags</a>
                    <% } %>

                    <h3 class="mid">Public Access Level</h3>
                    <%=Content.PrivilegeType %>

                    <h3 class="mid">Usage Rights</h3>
                    <a href="<%=Content.UseRightsUrl %>" target="_blank" class="useRights">
                        <span><%=Content.ConditionsOfUse %></span>
                        <img src="<%=Content.ConditionsOfUseIconUrl %>" alt="<%=Content.ConditionsOfUse %>" />
                    </a>

                    <h3 class="mid">Last Updated</h3>
                    <%=Content.LastUpdated.ToString() %>

                    <% if(Content.OrgId > 0 || Content.ParentOrgId > 0){ %>
                    <h3 class="mid">Organization</h3>
                    <%=( string.IsNullOrWhiteSpace( Content.Organization ) ? Content.ParentOrganization : Content.Organization ) %>
                    <% } %>

                    <h3 class="mid">Status</h3>
                    <%=Content.Status %>

                    <% if(TopLevelNode != null){ %>
                    <h3 class="mid">Learning List</h3>
                    This content is part of:
					<a href="/learninglist/<%=TopLevelNode.Id %>/<%=TopLevelNode.ResourceFriendlyTitle %>" class="learningList">
                        <%=TopLevelNode.Title %>
                        <% if(!string.IsNullOrWhiteSpace(TopLevelNode.ImageUrl)) { %>
                        <img src="<%=TopLevelNode.ImageUrl %>" alt="<%=TopLevelNode.Title %>" />
                        <% } %>
                    </a>
                    <% } %>
                </div>

            </div>
            <!-- /dataSection -->
        </asp:Panel>
    </div>
    <!-- /content -->
</div><!-- /contentWrapper -->

<asp:Panel ID="hiddenStuff" runat="server" Visible="false">
    <asp:Literal ID="googlePreviewer" runat="server">//docs.google.com/viewer?embedded=true&url=</asp:Literal>
    <asp:Literal ID="officePreviewer" runat="server">//view.officeapps.live.com/op/view.aspx?src=</asp:Literal>
    <asp:Literal ID="siteBaseUrl" runat="server">http://ioer.ilsharedlearning.org</asp:Literal>
    <asp:Literal ID="canAuthorApproveOwnContent" runat="server">no</asp:Literal>
    <asp:Literal ID="redirecting50ToLearningList" runat="server">yes</asp:Literal>

    <asp:Literal ID="txtCurrentContentId" runat="server">0</asp:Literal>
    <asp:Literal ID="learningListUrlTemplate" runat="server" Text="/LearningList/{0}/{1}"></asp:Literal>

    <asp:Literal ID="nativeTypes" runat="server">.html .htm .txt .pdf .jpg .jpeg .png .gif .tiff .bmp .webm .mp4 .svg</asp:Literal>
    <asp:Literal ID="officeTypes" runat="server">.doc .docx .xls .xlsx .ppt .pptx</asp:Literal>

    <asp:Literal ID="privateContentMsg" runat="server"><h2>Page not Available</h2><p>This content is under construction or has been designated as not available for public viewing.</p></asp:Literal>
</asp:Panel>
