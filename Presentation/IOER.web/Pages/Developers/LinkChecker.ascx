<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LinkChecker.ascx.cs" Inherits="IOER.Pages.Developers.LinkChecker" %>

    <h2 class="first">Overview</h2>
    <p>IOER uses four solutions to check for dead, inappropriate, and malicious links, and for checking files uploaded by users for viruses.  We check for 
        <a href="#dead">dead links</a> using a custom solution in a multi-phase, multi-threaded process which checks links to see whether they are good, 
        valid links or not.  We use the <a href="#gsba">Google Safe Browsing API</a> to check for malicious sites, and we have a <a href="#hosts">HOSTS file</a> from 
        <a href="http://winhelp2002.mvps.org/hosts.htm" target="_blank">http://winhelp2002.mvps.org/hosts.htm</a> to check for inappropriate sites.  Finally,
        we use an <a href="#virus">anti-virus scanner</a> to check files uploaded by users for viruses.
    </p>
    <a name="dead">&nbsp;</a>
    <h2>Dead Link Checker</h2>
    <p>The IOER link checker exists to periodically check the URL of each active resource in the IOER system to verify that it is a good, valid resource.  
    Resources are checked in a multiphase process to allow for hosts temporarily being down or busy, and for temporary DNS problems, among others.  
    When a resource is found to be bad, it is removed from the Elastic Search index and any libraries it belongs to, as well as flagged as deleted in the system.</p>
    <h3>Definitions</h3>
    <ul>
        <li>Deleted – The resource is removed from Elastic Search and flagged as deleted in the Link Checker database.</li>
        <li>FQDN - Fully Qualified Domain Name.  For example, ioer.ilsharedlearning.org is a fully qualified domain name.  This is sometimes also called "Host Name."</li>
        <li>Host Name - See FQDN</li>
    </ul>
    <h3>Multi-Threaded Process</h3>
    <p>The IOER link checker uses very few CPU cycles, and database, Elastic Search, and disk I/O are also minimal.  Most of the bottleneck can be 
    attributed to latency across the internet as it checks each resource.  Therefore a significant increase in speed can be obtained by doing checks 
    in multiple threads.  For example, a single-threaded check of 40,000 resources took approximately 9 hours, 32 minutes to complete.  By 
    splitting the 40,000 resources among 4 threads running in parallel, the time needed to check them was reduced to 2 hours, 21 minutes, 
    which is a significant time savings.  Because most of the time is spent in a phase 1 check, multi-threaded processing is implemented only for 
    Phase 1.</p>
    <p>The number of threads, and number of resources to check for Phase 1 is configurable through the link checker's .config file.</p>
    <h2>Phase 1 Processing</h2>
    <p>The number of resources checked each period is configurable through the link checker’s configuration file, and the resources checked for each 
    phase 1 run of the link checker are selected in a least recently checked fashion, with new resources going to the front of the line, since they 
    have never been checked before.  For some conditions, a resource is immediately deleted.  For others, a counter is incremented which results in 
    additional checks being done during Phase 2 processing.</p>
    <p>There are two different protocols currently supported by the link checker.  They are the HTTP and HTTPS protocols (which are treated as one), 
    and the FTP protocol.</p>
    <h3>HTTP/HTTPS</h3>
    <h4>Rules which require staff intervention</h4>
    <ul>
        <li>Too many redirects - if a page redirects more than 10 levels deep, the URL is logged and no action is taken.  It is up to administrators
            to decide what to do with the link.  This prevents the link checker from getting stuck in an infinite loop of redirects and never finishing.</li>
        </li>
        <li>Unknown protocols - if an unknown protocol is encountered, the URL is logged and no action is taken.  This allows technical staff to examine
            the resource's protocol, and write code which will check the resource's validity.
        </li>
    </ul>
    <h4>Code-based rules</h4>
    <p>Code-based rules are used where it is not possible to leverage equality, substrings, or regular expressions, or where the use of such is inefficient
        and slows the link checker down too much.  Examples of code-based rules are:
    </p>
    <ul>
        <li>Checking for redirects via meta-refresh - Some pages redirect to a URL using a &lt;meta&gt; tag with an http-equiv attribute that indicates a 
            redirect.  This type of redirect is normally handled client-side by the browser, but the link checker has code specifically for this since it 
            is not a browser.  When this is detected, the link checker checks the page that is being redirected to via the meta-refresh redirect.
        </li>
        <li>The body of the page contains only a &lt;noscript&gt;&lt;/noscript&gt; tag.  We consider this to be a black hat technique, and delete the
            resource from our system.
        </li>
    </ul>
    <h4>Conditions Which Result in Immediate Deletion</h4>
    <ul>
        <li>Known Bad Protocol – In the early days of the link checker, resources were found which did not have a good protocol on their URL.  
        The decision was made to simply remove these resources.  These known bad protocols are:
        <ul>
            <li>title</li>
            <li>docid</li>
            <li>grade</li>
        </ul></li>
        <li>IOER maintains a table of known 404 pages.  Each row in the table is flagged so that the link checker knows if it needs to check for an 
        exact match of a URL, or whether the URL is a regular expression that needs to be checked, and appropriate logic is used to determine whether 
        to treat it as an exact match or if a regular expression match needs to be done.  If a resource URL is found to be a match in this table, 
        whether an exact match or a regular expression match, or is redirected to a page which matches a rule in this table, it is immediately 
        deleted from the system.</li>
        <li>IOER maintains a table of known bad titles.  Each row in the table is flagged so that the link checker knows if it needs to check 
        for simple equality of a title, or whether the title is a regular expression that needs to be checked, as well as specification on which 
        FQDN the rule applies to.  If a resource’s web page contains a title which is found to be an exact match or a match on a regular expression, 
        it is immediately deleted from the system.</li>
        <li>IOER maintains a table of known bad content.  All rows in this table are assumed to be regular expressions, and apply to a specific FQDN,
        or to all hosts.  Applying the rule to all hosts is indicated by putting “all” in the HostName column.  If the content of a web page pointed 
        to by a resource has a match to the Content field of a row in this table, and the HostName matches, the resource is immediately deleted from 
        the system.</li>
        <li>If a page returns a 404 Not Found, 403 Forbidden, or a 410 Gone HTTP status, or is redirected to a page which returns either of these 
        statuses, the resource is immediately deleted from the system. </li>
        <li>If an “Invalid URI” exception occurs, the resource is immediately deleted from the system.</li>
    </ul>
    <h4>Conditions Which Result in Additional Checks Being Done</h4>
    <p>Each condition outlined below has an individual, configurable threshold that, once exceeded, results in the resource being deleted from the 
    system.  If a resource has a non-zero count of the number of times this resource has been checked, then the resource is flagged for Phase 2 
    checking.  If any of the counts are non-zero, this is interpreted as needing a Phase 2 check.</p>
    <ul>
        <li>The request times out – a timeout occurs if the link checker does not get a response back within 15 seconds.  A connection was established 
        with the host, but it did not return a response before the timeout elapsed.</li>
        <li>Unable to connect – If the link checker is unable to establish a connection to the host.  This is a timeout of a different sort.  In this 
        case, a connection to the host could not be established before the timeout elapsed.  This can also be caused by a host actively refusing the
        connection.</li>
        <li>DNS Errors – A problem occurred when resolving the Host Name to an IP address.  These are usually timeouts when contacting the domain’s 
        DNS server, the FQDN is not found, or the domain has expired.</li>
        <li>Connection to the server was closed, Receive Failure, or Send Failure are all treated as Unable to connect exceptions.</li>
        <li>400 Bad Request – Sometimes something is wrong with the host (or the request itself), and the issue the host is having gets fixed.  
        If it’s not fixed before the threshold is exceeded, then the resource ends up getting deleted.</li>
        <li>500 Internal Server Error – These can happen for any reason from a null reference exception to the server attempting to divide by zero.  
        It’s a horribly cryptic response that usually means there’s something wrong with the page, and should be tried again later after developers 
        have had time to fix it.</li>
    </ul>
    <h4>Table-based rules</h4>
    <p>Many of the rules for detecting whether a link is bad are stored in tables.  By updating a rule in a table, we can avoid having to recompile the
        link checker every time a new rule is added.  The rules are read from the tables each time the link checker starts up, so if you've updated a rule
        and you're running the link checker interactively, you'll have to stop and restart the link checker each time you add a new rule (or set of rules) 
        so that the new rule will be read in from the table and used.
    </p>
    <p>Rules in tables are updated on a regular basis.  There are three tables currently used for storing the rules.</p>
    <ol>
        <li>Known Bad Content table, where rules are placed when looking for a specific piece of content that may indicate that the resource is not valid or 
            inappropriate for our audience.  An example of a resource that is not valid would be a page that contains the words "page requested 
            is not found."  An example of a resource that may be inappropriate for our audience is a page that contains the words "online casino."
        </li>
        <li>Known Bad Title table, where rules are placed when looking for a specific title that may indicate the page is not valid.</li>
        <li>Known 404 Pages table, where rules are placed that identify a URL as one that should be treated as a page not found.  The rules in this table
            differ slightly in that they may apply to all links, or only to links which redirect to a link that matches the rule.  This is useful when
            you have resources that at one time were good, but now all redirect to the home page of a site, where we want to treat the home page of a site
            as a valid resource, but anything that redirects to it as an invalid resource.
        </li>
    </ol>
    <p>All rule tables can leverage the power of Regular Expressions for determining whether or not a given resource matches a rule that indicates the
        resource should be deleted from the system.  It is beyond the scope of this document to discuss regular expressions, however an excellent tutorial
        on how to use them can be found at <a href="http://www.regular-expressions.info/" target="_blank">http://www.regular-expressions.info</a>.
    </p>
    <p>You can view the current rule sets by clicking the links below to download .csv files which contain the current rules.</p>
    <ol>
        <li><asp:LinkButton ID="btnBadContent" runat="server" OnClick="btnBadContent_Click" Text="Download Bad Content Rules" /></li>
        <li><asp:LinkButton ID="btnBadTitle" runat="server" OnClick="btnBadTitle_Click" Text="Download Bad Title Rules" /></li>
        <li><asp:LinkButton ID="btn404Pages" runat="server" OnClick="btn404Pages_Click" Text="Download 404 Pages Rules" /></li>
    </ol>
    <h3>FTP</h3>
    <h4>Conditions Which Result in Immediate Deletion</h4>
    <p>The following conditions result in the resource being immediately deleted from the system:</p>
    <ul>
        <li>550 Not found</li>
        <li>530 Not logged in</li>
    </ul>
    <h4>Conditions Which Result in Additional Checks Being Done</h4>
    <p>Each condition outlined below has an individual, configurable threshold that, once exceeded, results in the resource being deleted from the 
    system.  If a resource has a non-zero count of the number of times this resource has been checked, then the resource is flagged for Phase 2 
    checking.</p>
    <ul>
        <li>DNS errors - see DNS errors in the HTTP/HTTPS section.</li>
        <li>Unable to Connect - see Unable to connect in HTTP/HTTPS section.</li>
        <li>Invalid URI - These are sometimes resolved later, so pass them to phase 2.</li>
    </ul>
    <h2>Phase 2 Checking</h2>
    <p>Phase 2 checking uses the exact same rules as Phase 1, except that only those resources with nonzero number of times they have been checked 
    trigger a Phase 2 check.  All resources which are flagged for Phase 2 checking are checked each time a Phase 2 check is run.  Because the number 
    of resources needing a Phase 2 check is much smaller than the number of resources for a Phase 1 check, this process has not been converted to a 
    multi-threaded process.</p>
    <h2>Reporting</h2>
    <p>At the end of Phase 1 and Phase 2 link checking, a report containing the findings of the Link Checker is generated.</p>
    <h2>Miscellaneous Utilities</h2>
    <p>The link checker has 4 utilities built into it related to link checking.  These are generally run by developers to test changes to the code,
    changes to the rules in the tables, or to make corrections to Elastic Search to reflect the state of the database</p>
    <h3>Phase 1 for ID Range</h3>
    <p>Each resource within IOER is assigned an ID by the system.  This allows developers to run a link check for a specific ID or range of resource 
    IDs.  As usual, the same rules for the ID range are used as for Phase 1 and Phase 2.</p>
    <h3>Delete Resources from Elastic Search</h3>
    <p>This utility issues a delete query to Elastic Search for each resource in the link checker database that is found to be deleted.  After each 
    100 queries are issued to Elastic Search, this utility sleeps for 20 seconds, in order to play nice with Elastic Search.  It runs on the entire 
    Link Checker database, looking for resources that are flagged as deleted in the database.</p>
    <h3>Delete Single Resource from Elastic Search</h3>
    <p>This utility issues a delete query to Elastic Search for a single resource in the link checker database, provided that resource is flagged as 
    deleted within the database.</p>
    <h3>Phase 1 for Host Name</h3>
    <p>This utility performs a Phase 1 check for all resources that have a given FQDN.</p>
    <h3>Bad Link Checking Rules</h3>
    <ul>
        <li>Too many redirects - if a page redirects more than 10 levels deep, the URL is logged and no action is taken.  It is up to administrators
            to decide what to do with the link.  This prevents the link checker from getting stuck in an infinite loop of redirects and never finishing.</li>
        <li>Known bad protocols - If a page's URL starts with a known bad protocol, the URL is logged and the resource is marked as deleted.</li>
        <li>Unknown protocols - if an unknown protocol is encountered, the URL is logged and no action is taken.  This allows technical staff to examine
            the resource's protocol, and write code which will check the resource's validity.
        </li>
    </ul>
    <a name="gsba">&nbsp;</a>
    <h2>Google Safe Browsing API</h2>
    <p>Per the <a href="https://developers.google.com/safe-browsing/developers_guide_v3?hl=en" target="_blank">Google Safe Browsing API's documentation</a>, 
        IOER caches the blacklists from the API, and then checks incoming links on import as well as when a user tags a new resource, to see if the 
        entered link corresponds to a site that Google believes to contain viruses or other malware.  If the site is found, our system rejects the link.
    </p>
    <a name="hosts">&nbsp;</a>
    <h2>HOSTS file</h2>
    <p>The HOSTS file for looking for inappropriate sites is periodically downloaded from 
        <a href="http://winhelp2002.mvps.org/hosts.htm" target="_blank">http://winhelp2002.mvps.org/hosts.htm</a> and imported into a staging table, 
        and from there the table containing a list of blacklisted hosts is updated using a stored procedure.  This table is then used to compare
        incoming host names against, and hosts which match a name on the list are excluded from IOER.
    </p>
    <a name="virus">&nbsp;</a>
    <h2>Checking Uploads for viruses</h2>
    <p>IOER checks files uploaded to our site for viruses and malware using a free, open source virus scanner called ClamAV.  There exists a .NET
        wrapper for this, and our site calls this .NET wrapper to pass the file on to ClamAV for scanning.</p>
    <p><span style="font-weight:bold;">Disclaimer:</span> While IOER uses these methods to provide reasonable assurances that files uploaded on our site 
        by users are virus and malware free, we cannot guarantee that they do not contain some sort of malware or virus.  It is prudent for the user
        to scan the files with their own anti-virus software before attempting to use files downloaded from IOER, or any other source.  IOER 
        <span style="font-weight:bold;">does not</span> guarantee that content or files linked to by us but hosted elsewhere are malware or virus free.
    </p>