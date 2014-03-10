<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LibraryMtce.ascx.cs" Inherits="ILPathways.Controls.Libraries.LibraryMtce" %>


<%@ Register TagPrefix="uc1" TagName="ImageHelper" Src="/LRW/controls/ImageHelper.ascx" %>


  <script type="text/javascript" language="javascript" src="/Scripts/toolTip.js"></script>
  <link rel="Stylesheet" type="text/css" href="/Styles/ToolTip.css" />


    <script language="javascript" type="text/javascript">
<!--
    function confirmDelete(recordTitle, id) {
        // ===========================================================================
        // Function to prompt user to confirm a request to delete a record
        // Note - this could be made generic if the url is passed as well

        var bresult
        bresult = confirm("Are you sure you want to delete this library? All resources will be removed if you continue.\n\n"
            + "Click OK to delete this library or click Cancel to skip the delete.");
        var loc;

        loc = self.location;

        if (bresult) {
            //alert("delete requested for id " + id + "\nlocation = " + self.location);

            //location.href="?id=" + id + "&a=delete";
            return true;
        } else {
            return false;
        }


    } //  
    //-->
</script>
<style type="text/css">
  #containerDetails labelColumn, #containerDetails dataColumn {padding-top: 3px; }
.txtTitle { width: 300px; }
.columnStyle { margin-top: 10px; }
.radioButton label {
  padding: 1px 5px;
  width: 25px;
  display: inline-block;
  *display: inline;
  }
  #containerDetails {
    width: 600px;
    margin: 0 auto;
  }
    .isleH2 { width: 75%; }
  .dataColumn {
    margin: 2px;
  }
</style>
 <script type="text/javascript">
     jQuery(document).ready(function () {
         jQuery('#imgCrop').Jcrop({
             onChange: showPreview,
             onSelect: storeCoords,
             aspectRatio: .75
         });
     }); //end ready


     function storeCoords(c) {
         jQuery('#X').val(c.x);
         jQuery('#Y').val(c.y);
         jQuery('#W').val(c.w);
         jQuery('#H').val(c.h);
     };


     //When the selection is moved, this function is called:
     function showPreview(coords) {
         var rx = 100 / coords.w;
         var ry = 100 / coords.h;

         $('#previewImage').css({
             width: Math.round(rx * 500) + 'px',
             height: Math.round(ry * 370) + 'px',
             marginLeft: '-' + Math.round(rx * coords.x) + 'px',
             marginTop: '-' + Math.round(ry * coords.y) + 'px'
         });
     }
</script>

<asp:validationsummary id="vsErrorSummary" HeaderText="Errors on page" forecolor="" CssClass="errorMessage" runat="server"></asp:validationsummary>
<br />
<asp:Panel ID="displayPanel" runat="server" Visible="false" CssClass="libraryDetailsPanel">
  <asp:label ID="lblLibraryDisplay" runat="server" ></asp:label>

</asp:Panel>

<asp:Panel ID="detailsPanel" runat="server" CssClass="libraryDetailsPanel">

<div id="containerDetails">
	<div class="clearFloat"></div>	
	<h2 class="isleH2"><asp:literal ID="libraryName" runat="server">Library Management</asp:literal></h2>
  <asp:Panel ID="idPanel" runat="server" Visible="false">
	<!-- --> 
	<div class="clearFloat"></div>	
  <div class="labelColumn" > 
    <asp:label id="lblId"  associatedcontrolid="txtId" runat="server">id</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:label id="txtId"  runat="server"></asp:label>  
	</div>
  </asp:Panel>
	<!-- --> 
	<div class="clearFloat"></div>
  <div class="labelColumn requiredField">
    <asp:label id="Label1"  associatedcontrolid="txtTitle" runat="server">Title</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:textbox  id="txtTitle" CssClass="txtTitle"  MaxLength="200" runat="server"></asp:textbox>  <br /><asp:HyperLink ID="viewLibraryLink" runat="server" Text="View Library" Target="_blank"></asp:HyperLink>
	</div>	
<!-- --> 
<asp:Panel ID="libTypePanel" runat="server" Visible="true">
<div class="clearFloat"></div>
  <div class="labelColumn " > 
    <asp:label id="lblType"  associatedcontrolid="ddlLibraryType" runat="server">Library Type</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:dropdownlist id="ddlLibraryType" runat="server" Width="250px"></asp:dropdownlist>

  </div>
</asp:Panel>

<asp:Panel ID="orgPanel" runat="server" Visible="true">
<div class="labelColumn">Organization</div>
<div class="dataColumn">
    <asp:DropDownList ID="ddlOrgs" Visible="false" runat="server" ></asp:DropDownList>
    <asp:label id="lblOrganization" Visible="false"  runat="server"></asp:label> 
</div>
    </asp:Panel>
<!-- --> 
<div class="clearFloat"></div>
  <div class="labelColumn  requiredField" > 
    <asp:label id="lblDetails"  associatedcontrolid="txtDescription" runat="server">Description</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="4" Width="300px"></asp:TextBox>
  </div>

<!-- public access -->
  <div class="labelColumn " > 
    <asp:label id="Label5"  associatedcontrolid="ddlPublicAccessLevel" runat="server">Public Access Level</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:dropdownlist id="ddlPublicAccessLevel" runat="server" Width="250px"></asp:dropdownlist>
  </div>
       <div class="dataColumn">
                        <a class="toolTipLink" id="A1" title="Public Access Level|<ul><li><strong>None</strong> - The library has no default access, and is hidden from searches.</li><li><strong>By Request Only</strong> - The library has no default access, but will enable requests to access the library. The library can be found by a search.</li><li><strong>Read Only</strong> - The library can be viewed by anyone and will be displayed in searches.</li><li><strong>Contribute with Approval</strong> - The library is publically available and any authenticated user may add a resource to the library. The resource will not be visible until it has been approved by a library curator or administrator.</li><li><strong>Contribute No Approval</strong> - Same as the latter, except no approval is required.</li></ul>"><img
			src="/images/icons/infoBubble.gif" alt="" /></a>
                    </div>

<!-- org access -->
<asp:Panel ID="orgAccessPanel" runat="server">
  <div class="labelColumn " > 
    <asp:label id="Label6"  associatedcontrolid="ddlOrgAccessLevel" runat="server">Organization Access Level</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:dropdownlist id="ddlOrgAccessLevel" runat="server" Width="250px"></asp:dropdownlist>
  </div>
 <div class="dataColumn">
                        <a class="toolTipLink" id="A2" title="Organization Access Level|<ul><li><strong>None</strong> - The library has no default access for members of the related organization, and is hidden from searches. </li><li><strong>By Request Only</strong> - The library has no default access for members of the related organization, but will enable requests to access the library. The library can be found by a search.</li><li><strong>Read Only</strong> - The library can be viewed by any member the related organization and will be displayed in searches.</li><li><strong>Contribute with Approval</strong> - The library is publically available and any member of the related organization may add a resource to the library. The resource will not be visible until it has been approved by a library curator or administrator.</li><li><strong>Contribute No Approval</strong> - Same as the latter, except no approval is required.</li></ul>"><img
			src="/images/icons/infoBubble.gif" alt="" /></a>
                    </div>
</asp:Panel>
<!-- --> 
<asp:Panel ID="activePanel" Visible="false" runat="server">
<div class="clearFloat"></div>
  <div class="labelColumn " > 
    <asp:label id="Label2"  associatedcontrolid="rblIsPublic" runat="server">Is Public</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:RadioButtonList id="rblIsPublic" autopostback="false" CssClass="radioButton"  causesvalidation="false"   runat="server" tooltip="True: Library is publically accessible" RepeatDirection="Horizontal">
	<asp:ListItem Text="Yes"  value="Yes" Selected="True"></asp:ListItem>    
 	<asp:ListItem Text="No" 	value="No"  ></asp:ListItem>
</asp:RadioButtonList>
  </div>

<!-- --> 
<div class="clearFloat">  </div>
  <div class="labelColumn " > 
    <asp:label id="Label3"  associatedcontrolid="rblIsDiscoverable" runat="server">Is Discoverable</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:RadioButtonList id="rblIsDiscoverable" autopostback="false" CssClass="radioButton"  causesvalidation="false" runat="server" tooltip="True: Library can be found by searching" RepeatDirection="Horizontal">
	<asp:ListItem Text="Yes"  value="Yes" Selected="True"></asp:ListItem>    
 	<asp:ListItem Text="No" 	value="No"  ></asp:ListItem>
</asp:RadioButtonList>
  </div>
<!-- --> 
<div class="clearFloat"></div>
  <div class="labelColumn " > 
    <asp:label id="lblIsActive"  associatedcontrolid="rblIsActive" runat="server">Is Active</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:RadioButtonList id="rblIsActive" autopostback="false" causesvalidation="false"   CssClass="radioButton"  runat="server" tooltip="True: library is active" RepeatDirection="Horizontal">
	<asp:ListItem Text="Yes"  value="Yes" Selected="True"></asp:ListItem>    
 	<asp:ListItem Text="No" 	value="No"  ></asp:ListItem>
</asp:RadioButtonList>
  </div>
</asp:Panel>

	
	<asp:Panel ID="imagePanel" runat="server" Visible="true">
<!-- --> 
<div class="clearFloat"></div>
  <div class="labelColumn" > 
    <asp:label id="Label4"  runat="server">Library Image</asp:label> 
  </div>
  <div class="dataColumn isleBox" style="width: 145px; height: 145px;">
    <asp:literal ID="currentImage" runat="server" Visible="false"></asp:literal>
	</div>
    <div class="dataColumn" style="width: 150px; padding-top:20px;">
    <asp:label ID="noLibraryImagelabel" runat="server" Visible="false">You can upload an image to represent the library.</asp:label>
	</div>
  <div class="clearFloat"></div>	
  <div class="labelColumn" >&nbsp; </div>
  <div class="dataColumn">
  <asp:Label ID="currentFileName" runat="server" Visible="false"></asp:Label>
  
  
		<br /><span style="font-weight: bold;">Select an image for the library</span><a class="toolTipLink" id="tipFile" title="Library Image|Select a image for your library. It must be roughly square and should be 140px (width) x 140px (height) or it will be resized to a width of 140px and a height not taller than 140px. If the image is still taller than 140px, it will be cropped to fit the height."><img
			src="/images/icons/infoBubble.gif" alt="" /></a>
			<br />
	<asp:FileUpload ID="fileUpload" runat="server"  />
	</div>
	</asp:Panel>
     <asp:Panel ID="docKeyPanel" runat="server" Visible="false" >
      <asp:textbox id="txtFileTitle" MaxLength="100" runat="server"  />

    Attachment ID&nbsp;<asp:TextBox ID="txtAttachmentId" runat="server">0</asp:TextBox>
    Document ID&nbsp;<asp:TextBox ID="txtDocumentRowId" runat="server">0</asp:TextBox>
  </asp:Panel>

  <asp:Panel ID="pnlCrop" runat="server" Visible="false">
    	<%--<uc1:ImageHelper ID="ImageHelper1"  runat="server"></uc1:ImageHelper>--%>
  </asp:Panel>

<!-- -->
<asp:Panel ID="historyPanel" runat="server" Visible="true">
		<div class="clearFloat"></div>	
		<div class="labelColumn">
		<asp:label id="Label12" runat="server">History</asp:label>
		</div>
		<div class="dataColumn">
			<asp:label id="lblHistory" runat="server"></asp:label>
		</div>
</asp:Panel>
		<!-- -->
		<div class="clearFloat"></div>	
		<div class="labelColumn">&nbsp;</div>
		<div class="dataColumn">			
				<asp:button id="btnSave" runat="server" CssClass="defaultButton" width="100px" CommandName="Update" 
				OnCommand="FormButton_Click" Text="Save"		CausesValidation="true"></asp:button>
				<asp:button id="btnDelete" runat="server" CssClass="defaultButton" width="100px" CommandName="Delete" 
				OnCommand="FormButton_Click" Text="Delete"	CausesValidation="False" visible="false"></asp:button> 
				<asp:button id="btnNew" runat="server" style="margin-left: 10px;" CssClass="defaultButton" width="100px" CommandName="New" visible="false"
				OnCommand="FormButton_Click" Text="New Record" causesvalidation="false"></asp:button>

		</div>		
		<div class="clearFloat"></div>
		<br />
</div>

</asp:Panel>

<div>
<!-- validators -->
<asp:requiredfieldvalidator id="rfvTitle" Display="None" ControlToValidate="txtTitle" ErrorMessage="Title is a required field" runat="server"></asp:requiredfieldvalidator>


</div>

<asp:Panel ID="hiddenStuff" runat="server" Visible="false">

<!-- control variables -->
<asp:Literal ID="txtDefaultLibraryTypeId" runat="server" Visible="false">1</asp:Literal>
<asp:Literal ID="showingImagePath" runat="server" Visible="false">no</asp:Literal>


<asp:Literal ID="txtLibraryImageTemplate" runat="server" Visible="false"><img src='{0}' width='{1}px' height='{1}px' alt='libary icon'/></asp:Literal>
<asp:Literal ID="Literal1" runat="server" Visible="false"><img src='{0}' width='{1}px' height='{1}px' alt='libary icon'/></asp:Literal>
</asp:Panel>

