<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConditionsOfUseSelector.ascx.cs" Inherits="IOER.LRW.controls.ConditionsOfUseSelector" %>

<!-- Conditions of Use Script -->
<script type="text/javascript" >
var conditions = {
  <%=conditionsData %>
  updateSelection: function(jDDL) {
    var selectedIndex = jDDL.prop("selectedIndex");
    console.log(selectedIndex);
    $(".conditions_descriptor").html($(".conditions_template_descriptor").html()
      .replace(/{url}/g, conditions.data[selectedIndex].url)
      .replace(/{descriptor}/g, conditions.data[selectedIndex].descriptor)
    );
    $(".conditions_thumbnail").html($(".conditions_template_descriptor").html()
      .replace(/{url}/g, conditions.data[selectedIndex].url)
      .replace(/{descriptor}/g, "<img src=\"" + conditions.data[selectedIndex].thumbnail + "\">")
    );
    var conditionsURL = $(".txtConditionsOfUse");
    conditionsURL.val( conditions.data[selectedIndex].url );
    if(conditions.data[selectedIndex].hide){
      conditionsURL.hide();
    }
    else {
      conditionsURL.show();
    }
  }
};

$(document).ready(function() {
  var jConditionsOfUse = $(".ddlConditionsOfUse");
  jConditionsOfUse.change(function() {
    conditions.updateSelection(jConditionsOfUse);
  });
  $(".txtConditionsOfUse").attr("placeholder", "Enter the License URL");
  jConditionsOfUse.change();
});
</script>
<!-- Conditions of Use CSS -->
<style type="text/css">
  .conditionsSelector { width: 100%; border-collapse: collapse; }
  .conditionsSelector td { padding: 1px 0; }
  .conditions_thumbnail { width: 95px; text-align: center; vertical-align: middle; }
  .ddlConditionsOfUse { width: 100%; margin: 0; }
  .conditions_descriptor { vertical-align: top; margin: 0; padding: 2px; }
  .txtConditionsOfUse { width: 100%; }
</style>

<table class="conditionsSelector">
  <tr><td rowspan="2" class="conditions_thumbnail"></td><td><asp:DropDownList ID="ddlConditionsOfUse" CssClass="ddlConditionsOfUse" runat="server" /></td></tr>
  <tr><td class="conditions_descriptor"></td></tr>
  <tr><td colspan="2" class="txtConditionsOfUseHolder"><asp:TextBox ID="txtConditionsOfUse" CssClass="txtConditionsOfUse" runat="server" /></td></tr>
</table>

<div class="conditions_template_descriptor" style="display:none;">
  <a href="{url}" target="_blank">{descriptor}</a>
</div>

<asp:Panel ID="hiddenStuff" runat="server" Visible="false">
    <!-- rights unknown -->
  <asp:Literal ID="ltlConditionsOfUseDefaultValue" Text="8" runat="server" />
    <asp:Literal ID="litIsNewContext" Text="no" runat="server" />
</asp:Panel>