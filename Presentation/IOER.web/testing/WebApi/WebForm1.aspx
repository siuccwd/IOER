<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="ILPathways.testing.WebApi.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>test 1</title>
    <script src="../../Scripts/jquery-1.10.2.min.js"></script>
    <script type="text/javascript">
        function getProducts() {
            $.getJSON("api/sitefilters",
                function (data) {
                    $('#products').empty(); // Clear the table body.

                    // Loop through the list of products.
                    $.each(data, function (key, val) {
                        // Add a table row for the product.
                        var row = '<td>' + val.Id + '</td><td>' + val.Title + '</td>';
                        $('<tr/>', { html: row })  // Append the name.
                            .appendTo($('#products'));
                    });
                });
        }

        $(document).ready(getProducts);
</script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <h2>Products</h2>
    <table>
    <thead>
        <tr><th>Name</th><th>Price</th></tr>
    </thead>
    <tbody id="products">
    </tbody>
    </table>

    </div>
    </form>
</body>
</html>
