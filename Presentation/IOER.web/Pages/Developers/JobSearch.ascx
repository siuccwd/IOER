<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="JobSearch.ascx.cs" Inherits="IOER.Pages.Developers.JobSearch" %>

<p>The Indeed API searches several websites, job boards, staffing firms, associations and company pages and aggredates the results into a single XML list to display. To use the Indeed API, you must create a Publisher account to receive an API key. To do this use the following link: <a href="https://ads.indeed.com/jobroll/signup​​" target="_blank">https://ads.indeed.com/jobroll/signup​​</a></p>

<h3>Attribution</h3>
<p>​You must use the following attribution when displaying Indeed's job search results.</p>
<p>
  <code style="background-color:#EEEEEE;">
    &lt;span id=indeed_at&gt;&lt;a href="http://www.indeed.com/"&gt;jobs&lt;/a&gt; by &lt;a href="http://www.indeed.com/" title="Job Search"&gt;&lt;img ​src="http://www.indeed.com/p/jobsearch.gif" style="border: 0; vertical-align: middle;" alt="Indeed job search"&gt;&lt;/a&gt;&lt;/span&gt;​
  </code>
</p>

<h3>DNS Caching</h3>
<p>To maintain redundancy across multiple geographic regions, your application should regularly update the api.indeed.com IP address from our DNS servers.​</p>

<dl>
  <dt>publisher</dt>
  <dd>Publisher ID. <b>Your publisher ID is "xxxxAPI Keyxxxxx"</b>. This is assigned when you register as a publisher.</dd>
  <dt>v</dt>
  <dd>Version. Which version of the API you wish to use. <b>All publishers should be using version 2</b>. Currently available versions are 1 and 2. This parameter is required.</dd>
  <dt>format</dt>
  <dd>Format. Which output format of the API you wish to use. The options are "xml" and "json." If omitted or invalid, the XML format is used.</dd>
  <dt>callback</dt>
  <dd>Callback. The name of a javascript function to use as a callback to which the results of the search are passed. This only applies when format=json. For security reasons, the callback name is restricted letters, numbers, and the underscore character.</dd>
  <dt>q</dt>
  <dd>Query. By default terms are ANDed. To see what is possible, use our <a href="http://www.indeed.com/advanced_search" target="_blank">advanced search page</a> to perform a search and then check the url for the q value.</dd>
  <dt>l</dt>
  <dd>Location. Use a postal code or a "city, state/province/region" combination.</dd>
  <dt>sort</dt>
  <dd>Sort by relevance or date. Default is relevance.</dd>
  <dt>radius</dt>
  <dd>Distance from search location ("as the crow flies"). Default is 25.</dd>
  <dt>st</dt>
  <dd>Site type. To show only jobs from job boards use 'jobsite'. For jobs from direct employer websites use 'employer'.</dd>
  <dt>jt</dt>
  <dd>Job type. Allowed values: "fulltime", "parttime", "contract", "internship", "temporary".</dd>
  <dt>start</dt>
  <dd>Start results at this result number, beginning with 0. Default is 0.</dd>
  <dt>limit</dt>
  <dd>Maximum number of results returned per query. Default is 10</dd>
  <dt>fromage</dt>
  <dd>Number of days back to search.</dd>
  <dt>highlight</dt>
  <dd>Setting this value to 1 will bold terms in the snippet that are also present in q. Default is 0.</dd>
  <dt>filter</dt>
  <dd>Filter duplicate results. 0 turns off duplicate job filtering. Default is 1.</dd>
  <dt>latlong</dt>
  <dd>If latlong=1, returns latitude and longitude information for each job result. Default is 0.</dd>
  <dt>co</dt>
  <dd>Search within country specified. Default is the United States (us). See below for a complete list of supported countries.</dd>
  <dt>chnl</dt>
  <dd>Channel Name: Group API requests to a specific channel</dd>
  <dt>userip</dt>
  <dd>The IP number of the end-user to whom the job results will be displayed. This field is required.</dd>
  <dt>useragent</dt>
  <dd>The User-Agent (browser) of the end-user to whom the job results will be displayed. This can be obtained from the "User-Agent" HTTP request header from the end-user. This field is required.</dd>
</dl>

<h3>Sample Request</h3>
<p>Sample format of an xml request:</p>
<p>http://api.indeed.com/ads/apisearch?​publisher=APIKey&q=java&l=austin%2C+tx&sort=&radius=&st=&jt=&start=&limit=&fromage=&filter=&latlong=1&co=us&chnl=&userip=1.2.3.4&useragent=Mozilla/%2F4.0%28Firefox%29&v=2</p>