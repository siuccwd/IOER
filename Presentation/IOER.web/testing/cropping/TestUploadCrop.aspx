<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestUploadCrop.aspx.cs" Inherits="JqueryDevelopment.Projects.ImageResizerSamples.TestUploadCrop" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
  <title>Crop testing</title>
 
 <link href="/Styles/jquery.Jcrop.css" rel="stylesheet" type="text/css" />

<script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jquery/1.3/jquery.min.js"></script>

<script type="text/javascript" src="/Scripts/jquery.Jcrop.min.js"></script>
<script src="/Scripts/jquery.jcrop.preview.js" type="text/javascript"></script> 
 
 <script type="text/javascript">
   jQuery(document).ready(function () {
     jQuery('#imgCrop').Jcrop({
       onChange: showPreview,
       onSelect: storeCoords,
       aspectRatio: 1.00
     });
   }); //end ready

  
   function storeCoords(c) {
     jQuery('#X').val(c.x);
     jQuery('#Y').val(c.y);
     jQuery('#W').val(c.w);
     jQuery('#H').val(c.h);
   };
 

//When the selection is moved, this function is called:
function showPreview(coords)
{
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

</head>
<body>
  <form id="form1" runat="server">
  <div>
  <h1>Crop testing</h1>
  
  <br />

    <asp:Panel ID="pnlUpload" runat="server">
      <asp:FileUpload ID="Upload" runat="server" />
      <br />
      <asp:Button ID="btnUpload" runat="server" OnClick="btnUpload_Click" Text="Upload" />
      <asp:Label ID="lblError" runat="server" Visible="false" />
    </asp:Panel>
    <asp:Panel ID="pnlCrop" runat="server" Visible="false">
      <table>
        <tr>
          <td>
            <asp:Image ID="imgCrop" runat="server" />
          </td>
          <td>
            <div style="margin: 50px; width: 500px; text-align: center">
              <div style="width: 400px; height: 100px; overflow: hidden; margin-left: 5px;">
                <asp:Image ID="previewImage" CssClass="" Style="width: 200px;" runat="server" />
              </div>
            </div>
          </td>
        </tr>
      </table>
      <br />
      <asp:HiddenField ID="X" runat="server" />
      <asp:HiddenField ID="Y" runat="server" />
      <asp:HiddenField ID="W" runat="server" />
      <asp:HiddenField ID="H" runat="server" />
      <asp:Button ID="btnCrop" runat="server" Text="View" OnClick="btnCrop_Click" />
    </asp:Panel>
    <asp:Panel ID="pnlCropped" runat="server" Visible="false">
      <asp:Image ID="imgCropped" runat="server" />
      <br />
      <asp:Image ID="imgResized" runat="server" />
      <br />
      <asp:Button ID="btnSave" runat="server" Text="Save - soon" OnClick="btnSave_Click" />
        
    </asp:Panel>
      <hr />
      <asp:Button ID="btnNew" runat="server" Text="New Image" OnClick="btnNew_Click" />
  </div>
    <asp:Panel ID="hiddenPanel" runat="server" Visible="false">
    <asp:Literal ID="imagePath" runat="server" ></asp:Literal>
    </asp:Panel>
  </form>
</body>
</html>
