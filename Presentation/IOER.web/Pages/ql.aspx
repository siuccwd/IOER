<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ql.aspx.cs" Inherits="ILPathways.Pages.ql" %>

<%@ Register Src="~/Account/controls/LoginController.ascx" TagPrefix="uc1" TagName="LoginController" %>


<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <uc1:LoginController runat="server" ID="LoginController" />
    </div>
    </form>
</body>
</html>
