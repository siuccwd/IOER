<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="curriculae.aspx.cs" Inherits="ILPathways.Pages.curriculae" MasterPageFile="/Masters/Responsive.Master" %>

<asp:Content ContentPlaceHolderID="HeadContent" runat="server">
  <link rel="stylesheet" href="/Styles/common2.css" type="text/css" />
</asp:Content>
<asp:Content ContentPlaceHolderID="BodyContent" runat="server">


  <style type="text/css">
    #content:after { content: " "; display: block; clear: both; }
    .grayBox .mid:nth-child(2) { margin-top: -5px; }
    .column { display: inline-block; vertical-align: top; padding: 5px; }
    .column.left { width: 300px; float: left; }
    .column.right { width: calc(100% - 300px); }
    .result { width: 23%; max-width: 250px; margin: 1%; display: inline-block; vertical-align: top; min-height: 250px; font-weight: bold; text-align: center; font-size: 150%; }
    .result img { width: 80%; display: block; margin: 0 auto; }
    .result div { font-size: inherit; }
  </style>
  <script type="text/javascript">
    var items = [];
    $(document).ready(function () {
      //Fake out items
      $("#subject input").each(function() {
        var subject = $(this);
        $("#grade input").each(function () {
          var grade = $(this);
          items.push({ subject: subject.attr("value"), grade: grade.attr("value"), title: grade.parent().text() + " " + subject.parent().text() + " Curriculum" });
        });
      });

      //print items
      var box = $("#results");
      for (i in items) {
        box.append("<a href='#' class='grayBox result' data-subject='" + items[i].subject + "' data-grade='" + items[i].grade + "'><img src='/images/ioer_med.png' /><div class='title'>" + items[i].title + "</div></a>");
      }

      box.find(".result[data-subject=math][data-grade=grade7]").attr("data-special", "grade7").attr("href", "http://ioer.ilsharedlearning.org/testing/plain.aspx?curriculum=4463&node=4463");
      box.find(".result[data-subject=math][data-grade=grade8]").attr("data-special", "grade8").attr("href", "http://ioer.ilsharedlearning.org/testing/plain.aspx?curriculum=4279&node=4279");
      box.find(".result[data-subject=health]").attr("data-special", "health").attr("href", "http://ioer.ilsharedlearning.org/Content/2197/HSLE-Health_Science_Curriculum");

      $(".inputList input").on("change", function () {
        updateFilters();
      });

    });

    function updateFilters() {
      var activeSubjects = [];
      var activeGrades = [];
      if ($(".inputList input:checked").length == 0) {
        $(".result").fadeIn();
      }
      else {
        $("#subject input:checked").each(function () {
          activeSubjects.push($(this).attr("value"));
        });
        $("#grade input:checked").each(function () {
          activeGrades.push($(this).attr("value"));
        });

        $(".result").hide();
        $(".result").each(function () {
          var show = false;
          var subject = $(this).attr("data-subject");
          var grade = $(this).attr("data-grade");
          if (activeSubjects.length > 0 && activeGrades.length > 0) {
            if (activeSubjects.indexOf(subject) > -1 && activeGrades.indexOf(grade) > -1) {
              $(this).fadeIn();
            }
          }
          else {
            if (activeSubjects.indexOf(subject) > -1 || activeGrades.indexOf(grade) > -1) {
              $(this).fadeIn();
            }
          }
        });
      }
    }
  </script>
  
  <div id="content">
    <h1 class="isleH1">ISBE Model Curriculae</h1>

    <div class="column left">

      <div class="grayBox" id="filters">
        <h2 class="header">Filters</h2>
        <h3 class="mid">Subject</h3>
        <div class="inputList" id="subject">
          <label><input type="checkbox" value="math" /> Math</label>
          <label><input type="checkbox" value="ela" /> English Language Arts</label>
          <label><input type="checkbox" value="science" /> Science</label>
          <label><input type="checkbox" value="socialstudies" /> Social Studies</label>
          <label><input type="checkbox" value="arts" /> Arts</label>
          <label><input type="checkbox" value="language" /> World Languages</label>
          <label><input type="checkbox" value="health" /> Health</label>
          <label><input type="checkbox" value="physed" /> Physical Education</label>
          <label><input type="checkbox" value="technology" /> Technology</label>
        </div>
        <h3 class="mid">Grade Level</h3>
        <div class="inputList" id="grade">
          <label><input type="checkbox" value="grade1" /> Grade 1</label>
          <label><input type="checkbox" value="grade2" /> Grade 2</label>
          <label><input type="checkbox" value="grade3" /> Grade 3</label>
          <label><input type="checkbox" value="grade4" /> Grade 4</label>
          <label><input type="checkbox" value="grade5" /> Grade 5</label>
          <label><input type="checkbox" value="grade6" /> Grade 6</label>
          <label><input type="checkbox" value="grade7" /> Grade 7</label>
          <label><input type="checkbox" value="grade8" /> Grade 8</label>
          <label><input type="checkbox" value="grade10-11" /> Grades 9-10</label>
          <label><input type="checkbox" value="grade11-12" /> Grades 11-12</label>
        </div>
      </div>

    </div><div class="column right">

        <div id="results">

        </div>

      

    </div>
  </div>

</asp:Content>