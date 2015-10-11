<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ImageHelper.ascx.cs" Inherits="IOER.LRW.controls.ImageHelper" %>

 <link href="/Styles/jquery.Jcrop.css" rel="stylesheet" type="text/css" />
<%--<script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jquery/1.3/jquery.min.js"></script>--%>
<script type="text/javascript" src="/Scripts/jquery.Jcrop.min.js"></script>
<script src="/Scripts/jquery.jcrop.preview.js" type="text/javascript"></script> 

 <script type="text/javascript">

   jQuery(document).ready(function () {

     var originalRatio = 1.0
     var widthRatio1 = $("#<%=txtWidthHeightRatio.ClientID %>");

     var widthRatio = <%=WidthHeightRatio %>
     alert("widthRatio1: " + widthRatio1 + ",  widthRatio: " + widthRatio);
     if (widthRatio== null)
        widthRatio= originalRatio;

     jQuery('#imgCrop').Jcrop({
       onChange: showPreview,
       onSelect: storeCoords,
       aspectRatio: widthRatio
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


<asp:Panel ID="messagePanel" runat="server" Visible="false">
<p><asp:Label ID="formMessage" runat="server"></asp:Label></p>
</asp:Panel>

<asp:Panel ID="contentPanel" runat="server" Visible="true">

    <asp:Panel ID="pnlUpload" runat="server">
      <asp:FileUpload ID="fileUpload" runat="server" />
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
            <div style="margin: 50px; width: 200px; text-align: center">
              <div style="width: 100px; height: 100px; overflow: hidden; margin-left: 5px;">
                <asp:Image ID="previewImage" CssClass="" Style="width: 150px; height: 200px;" runat="server" />
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
          </asp:Panel>
    <asp:Panel ID="pnlResized" runat="server" Visible="false">
      <asp:Image ID="imgResized" runat="server" />
      <br />
      <asp:Button ID="btnSave" runat="server" Text="Save - soon" OnClick="btnSave_Click" />
    </asp:Panel>
</asp:Panel>

<asp:Panel ID="hiddenPanel" runat="server" Visible="false">

<asp:Literal ID="txtWorkImageName" runat="server" ></asp:Literal>
<asp:Literal ID="txtDocumentVersionId" runat="server" ></asp:Literal>
<asp:Literal ID="txtWidthHeightRatio" runat="server" >1.0</asp:Literal>
<asp:Literal ID="txtTargetWidth" runat="server" >150</asp:Literal>
<asp:Literal ID="txtUsingExactWidth" runat="server" >true</asp:Literal>
<asp:Literal ID="txtUsingPreview" runat="server" >no</asp:Literal>

<asp:Literal ID="txtWorkPath" runat="server" >temp</asp:Literal>
<asp:Literal ID="txtWorkPathKey" runat="server" >path.ReportsOutputPath</asp:Literal>
<asp:Literal ID="txtWorkPathUrl" runat="server" >temp</asp:Literal>
<asp:Literal ID="txtDestinationPath" runat="server" >temp</asp:Literal>

</asp:Panel>