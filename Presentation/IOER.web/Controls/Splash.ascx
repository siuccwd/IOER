<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Splash.ascx.cs" Inherits="IOER.Controls.Splash" %>
<%@ Register TagPrefix="uc1" TagName="SplashMini" Src="/Controls/SplashMini.ascx" %>

<style type="text/css">
  /* Big Stuff */
  #content { transition: padding-left 1s; -webkit-transition: padding-left 1s; min-width: 300px; }
  #content, #content * { box-sizing: border-box; -moz-box-sizing: border-box; }
  #columns { position: relative; }
  #columns .column { display: inline-block; height: 100%; }
  #columns .column.left, #columns .column.right { background-color: #EEE; padding: 5px; border-radius: 5px; }

  /* Specific Items */
  h1 { padding: 5px; font-size: 25px; text-align: center; margin: 5px 0; color: #F5F5F5; background-color: #333; border-radius: 5px; }

  /* Mini-Splash Stuff */
  #links .splashItem { margin: 5px 5px 5px 0; padding-left: 60px; }
  #links .splashItem h2 { font-size: 18px; }

  /* Responsive */
  @media screen and (min-width: 850px) {
    .column.left, .column.right { width: 250px; position: absolute; top: 0; }
    .column.left { left: 0; }
    .column.right { right: 0; }
    .column.center { width: 100%; padding: 5px 255px; }
  }
  @media screen and (max-width: 849px) {
    h1 { font-size: 18px; }
    .column { width: 100%; display: block; }
  }
  @media screen and (min-width: 980px) {
    #content { padding-left: 50px; }
  }
  @media screen and (max-width: 1200px) {
    #links .splashItem { width: 98%; }
  }
  @media screen and (min-width: 1201px) and (max-width: 1800px) {
    #links .splashItem { width: 48%; }
  }
</style>

<h1>Illinois Shared Learning Environment Open Education Resources</h1>
<div id="content">
  <div id="columns">
    <div class="column center">
      <p>A concise description of the ISLE OER Project goes here. A concise description of the ISLE OER Project goes here. A concise description of the ISLE OER Project goes here.</p>

      <div id="links">
        <uc1:SplashMini runat="server" ID="splashMini" />
      </div>
    </div>
    <div class="column left">Left Column</div>
    <div class="column right">Right Column</div>
  </div>
</div>