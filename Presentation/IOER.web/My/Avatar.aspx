<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Avatar.aspx.cs" Inherits="IOER.My.Avatar" %>

<!DOCTYPE HTML>
<html>
<head>
<style type="text/css">
  * { margin: 0; padding: 0; width: 100%; font-family: Calibri, Arial, Sans-Serif; font-size: 14px; }
  span { color: #D00; font-weight: bold; padding-left: 2px; }
  input[type=text] { display: none; }
  input[type=file]:hover, input[type=file]:focus { cursor: pointer; }
</style>
</head>
<body style="background-color: transparent;">
    <form id="form1" runat="server">
    
    <asp:FileUpload ID="fileUpload" runat="server" />
    <asp:Button ID="SubmitAvatar" class="submitAvatar" runat="server" OnClick="SubmitAvatar_Click" style="display:none;" />
    <asp:TextBox ID="collectionID" CssClass="collectionID" Text="" runat="server" />
    <asp:TextBox ID="libraryID" CssClass="libraryID" Text="" runat="server" /> 
    <asp:Label ID="ltlOutput" runat="server"></asp:Label>

    </form>
</body>
</html>
