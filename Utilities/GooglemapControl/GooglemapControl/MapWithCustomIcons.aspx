<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MapWithCustomIcons.aspx.cs" Inherits="Samples_SimpleMapWithCustomIcons" %>

<%@ Register Src="~/GoogleMapForASPNet.ascx" TagName="GoogleMapForASPNet" TagPrefix="uc1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Google Map with customized icons</title>
</head>
<body>
    <form id="form1" runat="server">
    <h3><a href="Default.aspx">Back</a></h3>
    <h3>Google map with customized icons.</h3>
    Click on a pushpins to see it's description.
    <div>
        <uc1:GoogleMapForASPNet ID="GoogleMapForASPNet1" runat="server" />
    </div>
    </form>
        <h3><a href="Default.aspx">Return to Samples Index</a></h3>

</body>
</html>
