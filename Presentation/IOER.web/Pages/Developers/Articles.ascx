<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Articles.ascx.cs" Inherits="IOER.Pages.Developers.Articles" %>

<div class="api">
  <div class="title">Get Categories</div>
  <div class="description">Returns a JSON object which lists of all the Categories as used for the Articles.</div>
  <div class="link">
    <div class="method">GET</div><div class="uri">Services/API/Article/GetCategories</div>
  </div>
  <ul class="example">
    <li>https://apps.il-work-net.com/services/api/article/getcategories</li>
  </ul>
</div>

<div class="api">
  <div class="title">Get Collections</div>
  <div class="description">Returns a JSON object which lists all the Main Collections and their associated colors as used for the Articles.</div>
  <div class="link">
    <div class="method">GET</div><div class="uri">Services/API/Article/GetCollections</div>
  </div>
  <ul class="example">
    <li>​https://apps.il-work-net.com/services/api/article/getcollections</li>
  </ul>
</div>

<div class="api">
  <div class="title">Search</div>
  <div class="description">​Returns a JSON object which contains all the Articles for a given category matching the given SearchTerm.</div>
  <div class="link">
    <div class="method">GET</div><div class="uri">​Services/API/Article/Search/{SearchTerm}/{Collection}/{Count}</div>
  </div>
  <dl class="parameters">
    <dt>SearchTerm</dt>
    <dd>A case insensitive search on the title and body of the articles.</dd>
    <dt>Collection</dt>
    <dd>If supplied, only results matching the search results <b>and</b> matching the Collection will be returned.</dd>
    <dt>Count</dt>
    <dd>Specifies the number of related results to return (0 returns all).</dd>
  </dl>
  <ul class="example">
    <li>​https://apps.il-work-net.com/services/api/article/Search/Explore/​</li>
    <li>​​https://apps.il-work-net.com/services/api/article/Search/Labor/Explore/</li>
  </ul>
</div>

<div class="api">
  <div class="title">Get Article (raw article)</div>
  <div class="description">​Returns the HTML necessary to display the Article matching the ArticleID page along with its supporting information.</div>
  <div class="link">
    <div class="method">GET</div><div class="uri">Services/API/Article/ArticleID/{ArticleID}/{Count}</div>
  </div>
  <dl class="parameters">
    <dt>ArticleID</dt>
    <dd>ID of the Article to get.</dd>
    <dt>Count</dt>
    <dd>Specifies the number of related results to return (0 returns all).</dd>
  </dl>
  <ul class="example">
    <li>https://apps.il-work-net.com/services/api/article/articleid/7​</li>
  </ul>
</div>

<div class="api">
  <div class="title">Get Article (using viewer)</div>
  <div class="description">Returns the actual page which is hosted externally.</div>
  <div class="link">
    <div class="method">GET</div><div class="uri">/ArticleViewer/Article/Index/{ArticleID}/{Count}</div>
  </div>
  <dl class="parameters">
    <dt>ArticleID</dt>
    <dd>ID of the Article to get.</dd>
    <dt>Count</dt>
    <dd>Specifies the number of related results to return (0 returns all) (default is 2).</dd>
  </dl>
  <ul class="example">
    <li>https://apps.il-work-net.com/ArticleViewer/Article/Index/7​</li>
  </ul>
</div>

