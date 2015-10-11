<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Bookmarks.ascx.cs" Inherits="IOER.Pages.Developers.Bookmarks" %>

<p>The Illinois workNet bookmarks API enables adding booksmarks to a user's account. An app using this API must have access to the user GUID (RowId from the user table),.</p>

<p><b>Conventions in this Document</b></p>
<p>Text in <b>bold</b> is required.  Text in italics should be substituted with the value of a parameter.</p>
<p>All URIs are relative to <a href="https://apps.il-work-net.com/providersApi" target="_blank">https://apps.il-work-net.com/providersApi</a></p>

<h2>Bookmark API</h2>
<h3>Find existing Bookmark</h3>
<div class="api">
  <div class="description">Determines if a bookmark has already been saved. Uses object with userGuid, and url</div>
  <div class="link">
    <div class="method">POST</div><div class="uri">/api/bookmark/find</div>
  </div>
  <dl class="parameters">
    <dt>UserGuid</dt>
    <dd>GUID of the User to search the bookmarks of</dd>
    <dt>Url</dt>
    <dd>Bookmark URL to search for</dd>
  </dl>
  <div class="example">
    <p>Found:</p>
    <pre>
      {
        Successful: true
        Message: ""
        Bookmark: 
        {
          Id: 90
          UserId: 487
          UserGuid: null
          Category: "IOER"
          Title: "TEST ADD 01-15"
          Url: "https://ioer.ilsharedlearning.org/Search"
          IsInternalUrl: false
          Created: "2015-01-16T18:40:13.667"
          LastUpdated: "2015-01-16T18:40:13.667"
        }
      }
    </pre>
    <p>Not Found:</p>
    <pre>
      {
        Successful: false
        Message: "Bookmark not found"
        Bookmark: null
      }
    </pre>
  </div>
</div>

<h3>Add a new Bookmark</h3>
<div class="api">
  <div class="description">Adds the bookmark to the user's account</div>
  <div class="link">
    <div class="method">POST</div><div class="uri">/api/Bookmark</div>
  </div>
  <dl class="parameters">
    <dt>UserGuid</dt>
    <dd>GUID of the User to search the bookmarks of</dd>
    <dt>Url</dt>
    <dd>Bookmark URL to search for</dd>
    <dt>Category</dt>
    <dd>Title of the category to add the bookmark to</dd>
    <dt>Title</dt>
    <dd>Title of the bookmark</dd>
    <dt>IsInternalUrl</dt>
    <dd>boolean indicating whether or not to use a relative (to workNet) URL</dd>
  </dl>
  <div class="example">
    <pre>
      {
        Message: "Bookmark was added."
        Successful: true
      }
    </pre>
  </div>
</div>

<h3>Delete a Bookmark</h3>
<div class="api">
  <div class="description">Deletes the bookmark from the user's account</div>
  <div class="link">
    <div class="method">DELETE</div><div class="uri">/api/Bookmark/</div>
  </div>
  <dl class="parameters">
    <dt>UserGuid</dt>
    <dd>GUID of the User to search the bookmarks of</dd>
    <dt>Url</dt>
    <dd>Bookmark URL to delete</dd>
  </dl>
  <div class="example">
    <p>Success:</p>
    <pre>
      {
        Successful: true
        Message: "Bookmark was removed."
      }
    </pre>
    <p>Failure:</p>
    <pre>
      {
        Successful: false
        Message: "The requested bookmark was not found."
      }
    </pre>
  </div>
</div>

<h3>Get a user's Bookmarks</h3>
<div class="api">
  <div class="description">Get all bookmarks for a User. Returns a collection of Bookmark objects.</div>
  <div class="link">
    <div class="method">GET</div><div class="uri">/api/Bookmark/GetAll?userGuid={UserGuid}</div>
  </div>
  <dl class="parameters">
    <dt>UserGuid</dt>
    <dd>GUID of the User to get the bookmarks of</dd>
  </dl>
</div>