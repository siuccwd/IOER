IOER
====

Illinois Open Educational Resources (IOER)

The IOER (http://www.ilsharedlearning.org) is for sharing, creating, and curating learning resources. 

This project was developed using Microsoft Visual Studio (2012+), C#, ASP.Net, and Sql server 2012. The community edition of Visual Studio (https://www.visualstudio.com/en-us/news/vs2013-community-vs.aspx) can be used effectively for development. The site includes learning resource metadata and paradata imported from the national Learning Registry (LR) (http://learningregistry.org) and tools to publish resource data to the LR. 

Get more information about developer documentation http://ioer.ilsharedlearning.org/developers.

Key Features:

  -  Consume from and publish to the LR.
  -  Search for resources using ElasticSearch (https://www.elastic.co/)  (version 1.7.2) based index.
  -  Search filters, including a learning standards browser (e.g. Common Core, NGSS) using Achievement Standards Network (ASN) http://www.achievementstandards.org/ 
  -  Resources can be aligned to the Common Core, NGSS, Illinois K-12 standards, Illinois Adult Education Standards, and a variety of national standards.
  -  Open Libraries with collections to share and curate resources.
  -  Create Learning Lists for structuring resources ranging from small sets of learning activities to an entire curriculum.
  -  Follow Libraries and Learning Lists.
  -  Widgets (http://ioer.ilsharedlearning.org/widgets/) for seamless access to Libraries, Learning, search and more on external websites.

This project currently has just the source, database scripts and most dependent packages. Nuget can be used to download the supporting packages.


<p>Last Update: 2015/10/28</p>
<ul>
    <li>Uploaded source to match the production version of Oct. 28, 2015</li>
    <li>Uploaded latest backup copies of the sandbox databases, including a sql script for restoring to Sql Server 2012. Note there is no longer a Gateway database.<br />In Github, these can be found in: https://github.com/siuccwd/IOER/tree/master/Other (for now, pending another update)</li>
    <li>These databases contain up-to-date code tables and representative data</li>
    <li>The IOER search uses an Elastic index. A fully built index is included in the server folder referenced below. This collection matches the sandbox databases, allowing a developer to immediately see search results</li>
    <li>Added a folder containing common folders referenced by the project configuration files. These can be unzipped locally, allowing a developer to quickly get the application running. of server tools to simplify initial installation, including:
        <ul>
            <li>Elastic Index - contains an index collection that matches the sandbox databases</li>
            <li>Supporting folders for uploaded content and thumbnails</li>
            <li>A tools folder</li>
        </ul>

    </li>
</ul>
