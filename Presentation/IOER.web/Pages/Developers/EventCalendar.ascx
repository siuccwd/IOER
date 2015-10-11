<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EventCalendar.ascx.cs" Inherits="IOER.Pages.Developers.EventCalendar" %>

<style type="text/css">
  #data table { width: 100%; margin-bottom: 25px; }
  #data table td { padding: 0 5px; }
  #data table td { border-bottom: 1px solid #CCC; }
  #data table tr td:nth-child(1) { width: 15%; }
  #data table tr td:nth-child(2) { width: 30%; word-break: break-all; }
  #data table tr td:nth-child(3) { width: 20%; }
  #data table tr td:nth-child(4) { width: 35%; }
</style>

<p><b>Conventions in this Document:</b></p>
<p>Text in<b>bold</b> is required. Text in<i>italics</i> should be substituted with the value of a parameter. </p>
<p>All URIs are relative to https://apps.il-work-net.com/.</p>

<h2>API interfaces </h2>

<h3>Events </h3>
<div class="api">
  <div class="description">Gets list of all events meeting the search criteria. All parameters are optional except radius, which is required if you specify a zip. <b>NOTE:</b> You will never find a grade level of "All" (id 1) or "Not Applicable" (id 19) in the database. "All" exists for the convenience of the user entering the event, and selects "Pre-Kindergarten" through "Technical" (ids 2-18). "Not Applicable" exists for validation purposes, to require that the user select at least one grade level. It is not stored in the database.</div>
  <div class="link">
    <div class="method">GET</div><div class="uri">/CalendarAPI/api/Events/{id}?searchCalendars={searchCalendars}&eventtype={eventtype}&keyword={keyword}&zipcode={zipcode}&miles={miles}&zipcodelist={zipcodelist}&interval={interval}&skip={skip}&approval={approvalStatus}&createdby={createdby}&start={start}&end={end}&audienceType={audienceType}&gradeLevel={gradeLevel}&interest={interest}</div>
  </div>
  <dl class="parameters">
    <dt>id</dt>
    <dd>integer ID of the Calendar.</dd>
    <dt>searchCalendars</dt>
    <dd>CSV list of calendar IDs (all integers).</dd>
    <dt>eventType</dt>
    <dd>CSV list of event type IDs (all integers).</dd>
    <dt>keyword</dt>
    <dd>Keyword to search on.</dd>
    <dt>zipcode</dt>
    <dd>A single zipcode to use as a starting point for a search (specifying this requires also specifying miles).</dd>
    <dt>miles</dt>
    <dd>Radius from zipcode to search within (required if zipcode is specified).</dd>
    <dt>zipcodelist</dt>
    <dd>CSV of zipcodes to search near - useful for searching by LWIA or county.</dd>
    <dt>interval</dt>
    <dd>Number of events to return.</dd>
    <dt>skip</dt>
    <dd>Number of matches to skip before returning results.</dd>
    <dt>approvalStatus</dt>
    <dd>Approved, Unapproved, or All</dd>
    <dt>createdby</dt>
    <dd>Name of the account that created the event</dd>
    <dt>start</dt>
    <dd>Start date for the events</dd>
    <dt>end</dt>
    <dd>End date for the events</dd>
    <dt>audienceType</dt>
    <dd>CSV of Audience IDs (all integers)</dd>
    <dt>gradeLevel</dt>
    <dd>CSV of Grade Level IDs (all integers)</dd>
    <dt>interest</dt>
    <dd>CSV of Interest IDs (all integers)</dd>
  </dl>
</div>
<%--<table>
  <tbody>
    <tr><td><b>Method</b></td><td><b>URI</b></td><td><b>Parameters</b></td><td><b>Description</b></td></tr>
    <tr>
      <td>GET</td>
      <td><p><b>/CalendarAPI/api/Events/</b><i>id </i><span>?searchCalendars=</span><i>calendarList </i><span>&amp;eventtype=</span><i>eventTypeList </i><span>&amp;keyword=</span><i>keyword </i><span>&amp;zipcode=</span><i>zip </i><span>&amp;miles=</span><i>radius </i><span>&amp;zipcodelist=</span><i>zipList </i><span>&amp;interval=</span><i>take </i><span>&amp;skip=</span><i>skip </i><span>&amp;approval=</span><i>approvalStatus </i><span>&amp;createdby=</span><i>creator </i><span>&amp;start=</span><i>start </i><span>&amp;end=</span><i>end </i><span>&amp;audienceType=</span><i>audienceList </i><span>&amp;gradeLevel=</span><i>gradeList </i><span>&amp;interest=</span><i>interestList</i></p></td>
      <td><p><i>id</i> – integer Id of the calendar <i>calendarList – </i><span>CSV list of calendar Ids (all integers) </span><i>eventTypeList</i><span> – CSV list of event type Ids (all integers) </span><i>keyword</i><span> – Keyword to search on. </span><i>zip</i><span> – a single zipcode to use as a  starting point for a search. </span><i>radius</i><span> – radius from </span><i>zip</i><span> to search </span><span>for.</span><span> </span><b>Required if zip is specified. </b><i>zipList</i><span> – CSV list of zipcodes to search.</span><span> </span><span>Useful for searching by LWIA or county. </span><i>take</i><span> – number of events to return. </span><i>skip</i><span> – Skip the first </span><i>skip</i><span> events that meet the search criteria. </span><i>approvalStatus – </i><span>one of {Approved, Unapproved, All}.</span><span> </span><span>This is a string. </span><i>creator</i><span> – string value of the account name that created the event. </span><i>start</i><span> – start date for the events.</span><span> </span><span>If omitted, defaults to now. </span><i>end</i><span> – end date for the events.</span><span> </span><span>If omitted, defaults to no end date. </span><i>audienceList</i><span> – CSV list of audience Ids (all integers). </span><i>gradeList</i><span> – CSV list of grade level Ids (all integers). </span><i>interestList</i><span> – CSV list of field of interest Ids (all integers)</span></p></td>
      <td>Gets list of all events meeting the search criteria. All parameters are optional except <i>radius</i>, which is required if you specify a <i>zip</i>.  <b>NOTE:</b>You will<span><i></i><b style="text-decoration:underline;"><i>never</i></b><span style="text-decoration:underline;"></span><i></i> find a grade level of"All" (id 1) or "Not Applicable" (id 19) in the database. "All" exists for the convenience of the user entering the event, and selects "Pre-Kindergarten" through "Technical" (ids 2-18). "Not Applicable" exists for validation purposes, to require that the user select at least one grade level. It is<b style="text-decoration:underline;"><i>not</i></b><span style="text-decoration:underline;"></span><i></i> stored in the database.</span> </td>
    </tr>
  </tbody>
</table>
--%>



<h3>Audience</h3>

<table>
  <tbody>
    <tr><td><b>Method</b></td><td><b>URI</b></td><td><b>Parameters</b></td><td><b>Description</b></td></tr>
    <tr><td>GET</td><td><b>/CalendarAPI/api/Audience/</b></td><td>None</td><td>Gets list of all Audiences</td></tr>
    <tr><td>GET</td><td><b>/CalendarAPI/api/Audience/</b><b><i>id</i></b></td><td><b><i>id</i></b></td><td>Retrieves the audience whose Id = <b><i>id</i></b><b>.</b></td></tr>
  </tbody>
</table>

<h3>Bad Words</h3>
<table>
  <tbody>
    <tr><td><b>Method</b></td><td><b>URI</b></td><td><b>Parameters</b></td><td><b>Description</b></td></tr>
    <tr><td>GET</td><td><b>/CalendarAPI/api/Badwords/?inputtext=</b><b><i>text</i></b></td><td><b>text</b></td><td>Checks <i>text </i>for bad words.</td></tr>
  </tbody>
</table>

<h3>Calendar</h3>
<table>
  <tbody>
    <tr><td><b>Method</b></td><td><b>URI</b></td><td><b>Parameters</b></td><td><b>Description</b></td></tr>
    <tr><td>GET</td><td><b>/CalendarAPI/api/Calendar/</b></td><td>None</td><td>Gets list of all available calendars</td></tr>
    <tr><td>GET</td><td><b>/CalendarAPI/api/Calendar/?defaultcalendar=true</b></td><td><b><i>defaultcalendar</i></b></td><td>Retrieves the default calendar</td></tr>
    <tr><td>GET</td><td><b>/CalendarAPI/api/Calendar/</b><b><i>id</i></b></td><td><b><i>id</i></b></td><td>Retrieves the calendar with Id = <b><i>id</i></b>.</td></tr>
    <tr><td>POST</td><td><b>/CalendarAPI/api/Calendar/</b></td><td><b><i>CalendarDTO</i></b></td><td>Creates the calendar specified in the <b><i>CalendarDTO</i></b><b> </b>in the body of the request. CalendarDTO is a JSON object. Requires authentication.</td></tr>
  </tbody>
</table>

<h3>City</h3>
<table>
  <tbody>
    <tr><td><b>Method</b></td><td><b>URI</b></td><td><b>Parameters</b></td><td><b>Description</b></td></tr>
    <tr><td>GET</td><td><b>/CalendarAPI/api/City/city=</b><b><i>city</i></b>&amp;<i>take=nbr</i></td><td><b><i>city</i></b> is the start of a city name. <i>take</i> is the number of results to return.</td><td>Lookup city from first few characters.</td></tr>
    <tr><td>GET</td><td><b>/CalendarAPI/api/City/zipcode=</b><b><i>zip</i></b></td><td><b><i>Zip</i></b></td><td>Lookup city by zipcode.</td></tr>
  </tbody>
</table>

<h3>Event Details </h3>
<table>
  <tbody>
    <tr><td><b>Method</b></td><td><b>URI</b></td><td><b>Parameters</b></td><td><b>Description</b></td></tr>
    <tr><td>GET</td><td><b>/CalendarAPI/api/EventDetails/</b><b><i>id</i></b></td><td><b><i>id</i></b></td><td>Gets Event details of the event whose Id = <b><i>id</i></b><i>.</i></td></tr>
    <tr><td>DELETE</td><td><b>/CalendarAPI/api/EventDetails/</b><b><i>id</i></b></td><td><b><i>id</i></b></td><td>Retrieves the audience whose Id = <b><i>id</i></b>. Requires authentication.</td></tr>
    <tr><td>POST</td><td><b>/CalendarAPI/api/EventDetails/</b></td><td><b><i>EventDTO</i></b></td><td>Creates the event specified in the <b><i>EventDTO </i></b>in the body of the request. EventDTO is a JSON object. Requires authentication.</td></tr>
    <tr><td>PUT</td><td><b>/CalendarAPI/api/EventDetails/</b></td><td><b><i>EventDTO</i></b></td><td>Updates the event specified in the <b><i>EventDTO</i></b> in the body of the request. EventDTO is a JSON object. Requires authentication.</td></tr>
  </tbody>
</table>

<h3>Event Types</h3>
<table><tbody><tr><td><b>Method</b></td><td><b>URI</b></td><td><b>Parameters</b></td><td><b>Description</b></td></tr><tr><td>GET</td><td><b>/CalendarAPI/api/EventTypes</b></td><td>None</td><td>Gets list of all Event Types</td></tr><tr><td>GET</td><td><b>/CalendarAPI/api/EventTypes/</b><b><i>id</i></b></td><td><b><i>id</i></b></td><td>Retrieves the event type whose Id = <b><i>id</i></b><b>.</b></td></tr><tr><td>POST</td><td><b>/CalendarAPI/api/EventTypes/</b></td><td><b><i>EventTypeDTO</i></b></td><td>Creates the event type specified in <b><i>EventTypeDTO</i></b> in the body of the request. EventTypeDTO is a JSON object. Requires authentication. </td></tr></tbody></table>

<h3>Fields of Interest</h3>
<table><tbody><tr><td><b>Method</b></td><td><b>URI</b></td><td><b>Parameters</b></td><td><b>Description</b></td></tr><tr><td>GET</td><td><b>/CalendarAPI/api/FieldOfInterest/</b></td><td>None</td><td>Gets list of all fields of interest.</td></tr><tr><td>GET</td><td><b>/CalendarAPI/api/FieldOfInterest/</b><b><i>id</i></b></td><td><b><i>id</i></b></td><td>Retrieves the field of interest whose Id = <b><i>id</i></b><b>.</b></td></tr></tbody></table>

<%--
<h3>Full Calendar Events </h3>
<table><tbody><tr><td><b>Method</b></td><td><b>URI</b></td><td><b>Parameters</b></td><td><b>Description</b></td></tr><tr><td>GET</td><td><p><b>/CalendarAPI/api/Events/</b><i>id </i><span>?searchCalendars=</span><i>calendarList </i><span>&amp;eventtype=</span><i>eventTypeList </i><span>&amp;keyword=</span><i>keyword </i><span>&amp;zipcode=</span><i>zip </i><span>&amp;miles=</span><i>radius </i><span>&amp;zipcodelist=</span><i>zipList </i><span>&amp;interval=</span><i>take </i><span>&amp;skip=</span><i>skip </i><span>&amp;approval=</span><i>approvalStatus </i><span>&amp;createdby=</span><i>creator </i><span>&amp;start=</span><i>start </i><span>&amp;end=</span><i>end </i><span>&amp;audienceType=</span><i>audienceList </i><span>&amp;gradeLevel=</span><i>gradeList </i><span>&amp;interest=</span><i>interestList</i></p></td><td><p><i>id</i> – integer Id of the calendar <i>calendarList – </i><span>CSV list of calendar Ids (all integers) </span><i>eventTypeList</i><span> – CSV list of event type Ids (all integers) </span><i>keyword</i><span> – Keyword to search on. </span><i>zip</i><span> – a single zipcode to use as a starting point for a search. </span><i>radius</i><span> – radius from </span><i>zip</i><span> to search for. </span><b>Required if zip is specified. </b><i>zipList</i><span> – CSV list of zipcodes to search. Useful for searching by LWIA or county. </span><i>take</i><span> – number of events to return. </span><i>skip</i><span> – Skip the first </span><i>skip</i><span> events that meet the search criteria. </span><i>approvalStatus – </i><span>one of {Approved, Unapproved, All}. This is a string. </span><i>creator</i><span> – string value of the account name that created the event. </span><i>start</i><span> – start date for the events. If omitted, defaults to now. </span><i>end</i><span> – end date for the events. If omitted, defaults to no end date. </span><i>audienceList</i><span> – CSV list of audience Ids (all integers). </span><i>gradeList</i><span> – CSV list of grade level Ids (all integers). </span><i>interestList</i><span> – CSV list of field of </span><span>interest Ids (all integers)</span></p></td><td>Used exactly like the GET method in Events, but returns redefinitions of some fields that are necessary for the jQuery plugin "fullCalendar" to be used.</td></tr></tbody></table>
--%>

<h3>Grade Level </h3>
<table><tbody><tr><td><b>Method</b></td><td><b>URI</b></td><td><b>Parameters</b></td><td><b>Description</b></td></tr><tr><td>GET</td><td><b>/CalendarAPI/api/GradeLevel/</b></td><td>None</td><td>Gets list of all grade levels.</td></tr><tr><td>GET</td><td><b>/CalendarAPI/api/GradeLevel/</b><b><i>id</i></b></td><td><b><i>id</i></b></td><td>Retrieves the grade level whose Id = <b><i>id</i></b><b>.</b></td></tr></tbody></table>

<h3>iCal </h3>
<table><tbody><tr><td><b>Method</b></td><td><b>URI</b></td><td><b>Parameters</b></td><td><b>Description</b></td></tr><tr><td>GET</td><td><b>/CalendarAPI/api/iCal/</b><b><i>id</i></b></td><td><b><i>id</i></b></td><td>Retrieves the event whose Id = <b><i>id</i></b><b> </b>in iCal format, suitable for importing into Outlook or other calendars.</td></tr></tbody></table>

<h3>Luminosity </h3>
<table><tbody><tr><td><b>Method</b></td><td><b>URI</b></td><td><b>Parameters</b></td><td><b>Description</b></td></tr><tr><td>GET</td><td><p><b>/CalendarAPI/api/Luminosity/</b></p><p><b>?pColor1=</b><b><i>color1</i></b></p><p><b>&amp;pColor2=</b><b><i>color2</i></b></p></td><td><b><i>color1</i></b><i> </i>and<i> </i><i><b>color2</b></i></td><td>Calculates luminosity ratio for two colors, expressed as standard color names (e.g. Blue) or as hex codes. Useful for determining if two colors are accessible or not. Returns a float.</td></tr></tbody></table>

<h3>Spell Check</h3>
<table><tbody><tr><td><b>Method</b></td><td><b>URI</b></td><td><b>Parameters</b></td><td><b>Description</b></td></tr><tr><td>POST</td><td><b>/CalendarAPI/api/SpellCheck/</b></td><td><b><i>SpellDTO</i></b></td><td>Returns JSON object with list of incorrect words or suggested words based on the action parameter.</td></tr><tr><td>GET</td><td><b>/CalendarAPI/api/SpellCheck/</b></td><td><b><i>SpellDTO</i></b></td><td>Returns JSON object with list of incorrectly spelled words.</td></tr></tbody></table>

<h3>ZIP Code </h3>
<table><tbody><tr><td><b>Method</b></td><td><b>URI</b></td><td><b>Parameters</b></td><td><b>Description</b></td></tr><tr><td>GET</td><td><p><b>/CalendarAPI/api/Zipcode/?zipcode=</b><b><i>zip </i></b><span>&amp;take=</span><i>take</i></p></td><td><p><b><i>zip</i></b><i> – zip code fragment </i><i>take</i><i><b> – </b></i><span>number of results to return</span></p></td><td>Gets list of zipcodes based on partial string value of a zipcode. Used for autocomplete features for a zipcode field.</td></tr><tr><td>GET</td><td><b>/CalendarAPI/api/FieldOfInterest/</b><b><i>?</i></b><b>city=</b><b><i>city</i></b></td><td><b><i>city</i></b></td><td>Retrieves a list of zipcodes for a city name.</td></tr></tbody></table>

<h2>Data Transfer Object (DTO) classes </h2>
<h3>CalendarDTO</h3>
<p>
  <pre>
  public class CalendarDTO 
  {    
    public int Id;  
    public string Name; 
    public bool Default; 
    public string Description; 
  } 
  </pre>
</p>
<h3>EventDTO</h3>
<p>
  <pre>
  public class EventDTO 
  { 
    public int Id; 
    public string Title; 
    public DateTime Start; 
    public DateTime StartUTC;  
    public DateTime End; 
    public DateTime EndUTC;  
    public string Code; 
    public string Description; 
    public string DescriptionHtml;  
    public bool Active; 
    public bool Virtual;  
    public string Url; 
    public bool AllDay;  
    public string Address;  
    public string City;  
    public string Zipcode; 
    public decimal Latitude; 
    public decimal Longitude;  
    public string ContactInfo; 
    public bool ApprovalRequired; 
    public bool Approved; 
    public DateTime ApprovalDate;  
    public string ApprovedById; 
    public DateTime RegistrationCloseDate; 
    public int EventTypeId; 
    public string EventTypeName; 
    public string CreatedById; 
    public DateTime CreatedDateUTC; 
    public string UpdatedById; 
    public string RemainingDates; 
    public string AllEventDates; 
    public bool ApproveAllEvents;
    public bool IsInOccurrence;
    public bool EditAsSingleEvent; 
    public int OccurrenceId; 
    public OccurrenceDTO Occurrence;
    public int CalendarId; 
    public string CalendarName;
    public virtual ICollection&lt;AudienceDTO&gt; Audiences; 
    public virtual ICollection&lt;GradeLevelDTO&gt; GradeLevels; 
    public virtual ICollection&lt;FieldOfInterestDTO&gt; FieldsOfInterest; 
    public ICollection&lt;int&gt; AudienceIDs; 
    public ICollection&lt;int&gt; GradeLevelIDs; 
    public ICollection&lt;int&gt; FieldsOfInterestIDs;
    public string GoogleCalendarURL; 
    public string LiveCalendarURL; 
    public string YahooCalendarURL; 
  }
  </pre>
</p>

<h3>EventType DTO</h3>
<p>
  <pre>
  public class EventTypeDTO 
  { 
    public string Name; 
  } 
  </pre>
</p>

<h3>SpellDTO </h3>
<p>
  <pre>
  public class EventTypeDTO 
  { 
    public string Name; 
  }  
  </pre>
</p>
