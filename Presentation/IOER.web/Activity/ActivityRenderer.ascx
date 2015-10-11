<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ActivityRenderer.ascx.cs" Inherits="IOER.Activity.ActivityRenderer" %>

<link rel="stylesheet" href="//code.jquery.com/ui/1.11.4/themes/smoothness/jquery-ui.css">
<script src="//code.jquery.com/ui/1.11.4/jquery-ui.js"></script>

<%-- 
	To setup activity rendering on a new page, you will need:
	- An instance of this ActivityRenderer control
	- A call to the GetDateRange() method in the ActivityRenderer instance
	- A javascript call to the setupDatePickers() method in this control
	- The following HTML (contains the inputs and elements that are referenced in the two above methods, as well as by the CSS in this control):

		<div id="dateSelector">
			Show activity from <input type="text" class="date startDate" id="txtStartDate" runat="server" /> to <input type="text" class="date endDate" id="txtEndDate" runat="server" /> <input type="button" class="isleButton bgBlue" value="Show" onclick="submit();" />
		</div>


	--%>

<script type="text/javascript">
  //Parent page will need to supply JSON to translate activity data to display-friendly data, eg
  function getActivity(templateName, activityCount, legend) {
    var template = $("#activityTemplate").html();
    var dataHTML = "";
    
    for (i in activityCount.Activities) {
      var current = activityCount.Activities[i];
      //Skip it if it has no items
      if (current.length == 0) { return "<p class=\"noActivity\">No activity.</p>"; }
      //Get activity data
      var title = legend[i];
      if (title == null) { continue; }
      var activityData = "";
      if (getSum(current) == 0) {
        activityData += "<p class=\"noActivity\">No activity.</p>";
      }
      else {
        switch (templateName) {
          case "divbars":
            activityData = renderActivity_divbars(current, legend, title);
            break;
          case "totals":
            activityData = renderActivity_totals(current, legend, title);
            break;
          default:
            break;
        }
      }
      //Construct the totals
      dataHTML += "<div class=\"activityItem " + i + "\"><div class=\"title\">{title} <div class=\"timespan\">{timespan}</div></div><div class=\"activityData\">{activityData}</div></div>"
        .replace(/{title}/g, title).replace(/{activityData}/g, activityData).replace(/{timespan}/g, legend.timespan.replace(/{count}/g, current.length));
    }

    return template
			.replace(/{id}/g, activityCount.Id)
			.replace(/{title}/g, (activityCount.Title == null ? "" : activityCount.Title))
			.replace(/{type}/g, templateName)
			.replace(/{data}/g, dataHTML);
  }

  function getActivityTable(name, title, activityList, legend) {
    //Create table
    var dataHTML = "<table class=\"activity table\" data-name=\"" + name + "\"><tbody>";

    //Add header
    dataHTML += "<tr class=\"header\">";
    dataHTML += "<th>" + title + "</th>";
    for (i in legend) {
      dataHTML += "<th>" + legend[i] + "</th>";
    }
    dataHTML += "</tr>";

    //Add rows
    for (i in activityList) {
      var current = activityList[i];
      dataHTML += "<tr data-id=\"" + current.Id + "\">";
      dataHTML += "<td>" + current.Title + "</td>";
      for (j in legend) {
        var total = getSum(current.Activities[j]);
        dataHTML += "<td " + (total == 0 ? "class=\"noActivity\"" : "") + ">" + (total == 0 ? "None" : total) + "</td>";
      }
      dataHTML += "</tr>";
    }

    //Finish and return table
    dataHTML += "</tbody></table>";
    return dataHTML;
  }

  //Div-based bars
  function renderActivity_divbars(current, legend, title) {
    //Get basic data
    var barWidth = 1 / current.length;
    //Get the biggest item
    var barMax = Math.max.apply(Math, current);
    //Avoid division by zero
    if (barMax == 0) { barMax = 1; }
    //Construct the bars
    var bars = "";
    for (var j = 0; j < current.length; j++) {
      bars += "<div class=\"activityBarZone\" style=\"width:" + (barWidth * 100) + "%;\" data-index=\"" + j + "\"><div tabindex=\"0\" class=\"activityBar\" style=\"height: " + ((current[j] / barMax) * 100) + "%\" title=\"" + current[j] + " " + title + " on " + dates[j] + "\"></div></div>";
      }
    return bars;
  }

  //Simple numeric totals
  function renderActivity_totals(current, legend, title) {
    var result = "";
    //Get basic data
    var sum = getSum(current);
    //Construct the totals
    var result = sum;
    return result;
  }

  function getSum(data) {
    var sum = 0;
    for (i in data) {
      sum += data[i];
    }
    return sum;
  }

	//Setup date pickers located in external code
  function setupDatePickers() {
  	$(".startDate").datepicker({
  		minDate: "01/01/2014", maxDate: "+0D", onClose: function (selectedDate) {
  			$(".endDate").datepicker("option", "minDate", selectedDate)
  		}
  	});
  	$(".endDate").datepicker({
  		minDate: "01/01/2014", maxDate: "+0D", onClose: function (selectedDate) {
  			$(".startDate").datepicker("option", "maxDate", selectedDate)
  		}
  	});
  }

</script>

<style type="text/css">
  /* General */
  .activityItem .timespan { font-style: italic; opacity: 0.8; }
  .activityItem .noActivity { text-align: center; padding: 10px; }

  /* divbars */
  .activity.divbars .activityData { height: 100px; line-height: 100px; }
  .activity.divbars .activityBarZone { padding: 0 0.2%; height: 100%; display: inline-block; vertical-align: bottom; position: relative; }
  .activity.divbars .activityBar { background-color: #4AA394; background-image: linear-gradient(rgba(74, 163, 148, 1), rgba(74, 163, 148, 0.7)); min-height: 1px; display: inline-block; vertical-align: bottom; width: 100%; transition: box-shadow 0.2s; border-radius: 2px 2px 0 0; }
  .activity.divbars .activityBar:hover, .activity.divbars .activityBar:focus { box-shadow: 0 0 10px 1px #FF5707; }
  .activity.divbars .activityBarZone:hover, .activity.divbars .activityBarZone:focus { z-index: 100; }
  /* totals */
  .activity.totals { display: inline-block; vertical-align: top; width: 32%; margin: 5px 0.5%; }
  .activity.totals .timespan, .activity.totals .title, .activity.totals .activityData { display: inline; padding: 0 2px; }
  .activity.totals .activityTitle { font-weight: bold; }

	#dateSelector { text-align: center; }
	#dateSelector input { display: inline; width: 100px; text-align: center; }

</style>

<div id="activityTemplates" style="display:none;">

  <div id="activityTemplate" data-id="{id}">
    <div class="activity {type}">
      <div class="activityTitle">{title}</div>
      <div class="activityItems">{data}</div>
    </div>
  </div>

</div>