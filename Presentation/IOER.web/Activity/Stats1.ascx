<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Stats1.ascx.cs" Inherits="IOER.Activity.Stats1" %>

<link rel="stylesheet" href="/styles/common2.css" type="text/css" />

<script type="text/javascript">

</script>

<style type="text/css">
  .organization { display: inline-block; vertical-align: top; width: 49%; margin: 5px 0.3% 25px 0.3%; }
  .avatar, .data, .content, .content > .description, .stat .title, .stat .description { display: inline-block; vertical-align: top; }
  .avatar { background: url('') center center no-repeat; background-size: contain; background-color: #EDEDED; border-radius: 5px; }
  
  .organization > .avatar { width: 175px; height: 175px; margin-right: 5px; display: none; }
  .organization > .data { /*width: calc(100% - 185px);*/ width: 100%; padding: 5px; }

  .contents { text-align: center; }
  .content { width: 100%; margin: 5px 1%; text-align: left; }
  .content > .avatar { width: 100px; height: 100px; margin: 0 5px 5px 0; }
  .content > .description { width: calc(100% - 110px); }

  .lightBox { border: 1px solid #DDD; padding: 5px; border-radius: 5px; margin: 5px; }
  .lightBox .header { background-color: #DDD; color: #000; margin: -5px -5px 5px -5px; }

  .stat { white-space: nowrap;  width: 49%; display: inline-block; vertical-align: top; }
  .stat .title { font-weight: bold; }
  .stat .title, .stat .description { white-space: normal; padding: 2px 5px; }

  /*@media (max-width: 1050px) {
    .content { width: 48%; }
  }
  @media (max-width: 750px) {
    .content { width: 100%; margin: 5px 0; }
  }
  @media (max-width: 500px) {
    .organization > .avatar { display: block; margin: 5px auto; }
    .organization > .data { width: 100%; display: block; }
  }*/
  @media (min-width: 1300px) {
    .organization { width: 32.5%; }
  }
  @media (max-width: 850px) {
    .organization { display: block; width: 100%; }
  }
</style>

<div id="content">
  <h1 class="isleH1">IOER Statistics</h1>

  <% foreach(var org in Organizations){ %>
    <div class="organization grayBox">
      <h1 class="header"><%=org.Title %></h1>
      <div class="avatar" style="background-image:url('<%=org.ImageUrl %>');"></div>
      <div class="data">
        <div class="description"><%=org.Description %></div>
        <div class="stats">
          <% foreach(var stat in org.Stats){ %>
          <div class="stat">
            <div class="title"><%=stat.Title %></div>
            <div class="description"><%=stat.Description %></div>
          </div>
          <% } %>
        </div>

      </div><!-- /data -->

      <h2 class="mid">Libraries</h2>
      <div class="contents libraries">
        <% foreach(var lib in org.Libraries) { %>

        <div class="content library lightBox">
          <h3 class="header"><%=lib.Title %></h3>
          <div class="avatar" style="background-image:url('<%=lib.ImageUrl %>');"></div>
          <div class="description"><%=lib.Description %></div>
          <div class="stats">
            <% foreach(var stat in lib.Stats) { %>
            <div class="stat">
              <div class="title"><%=stat.Title %></div>
              <div class="description"><%=stat.Description %></div>
            </div>
            <% } %>
          </div>
        </div>

        <% } %>
      </div>
      <%-- <h2 class="mid">Learning Lists</h2>--%>
      <div class="contents learninglists">
        <% foreach(var list in org.LearningLists ){  %>

        <div class="content learninglist lightBox">
          <h3 class="header"><%=list.Title %></h3>
          <div class="avatar" style="background-image:url('<%=list.ImageUrl %>');"></div>
          <div class="description"><%=list.Description %></div>
          <div class="stats">
            <% foreach(var stat in list.Stats) { %>
            <div class="stat">
              <div class="title"><%=stat.Title %></div>
              <div class="description"><%=stat.Description %></div>
            </div>
            <% } %>
          </div>
        </div>

        <% } %>
      </div>

    </div><!-- /organization -->  

  <% } %>
</div>
