<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeBehind="Publish.aspx.cs" Inherits="ILPathways.Publish" %>

<%@ Register TagPrefix="uc1" TagName="PublishNarrow" Src="/LRW/controls/PublishResource_Narrow2.ascx" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>ISLE Publishing Tool</title>
</head>
<body>
    <form id="form1" runat="server" defaultbutton="NullButton">
      <uc1:PublishNarrow id="publisher" runat="server"></uc1:PublishNarrow>
      <asp:Button ID="NullButton" runat="server" style="display:none;" OnClientClick="return false;" />
    </form>
</body>
</html>