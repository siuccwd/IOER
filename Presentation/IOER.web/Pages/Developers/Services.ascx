<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Services.ascx.cs" Inherits="IOER.Pages.Developers.Services" %>

<p>The Services API is avialable at http://apps.il-work-net.com/Locator/.</p>

<p><b>NOTE:</b> <b>PhoneSearch</b> and <b>PhoneDetail</b> are different from <b>Search</b> and <b>Location</b>.  PhoneSearch and PhoneDetail return limited information about locations and are intended for use when bandwidth is limited, such as for a mobile device.  Search and Location return more complete information about locations.</p>

<h2>PhoneSearch</h2>
<p>
  <pre>
    GET api/PhoneSearch/?keyword=keyword
    &zipcode=zipcode
    &latitude=latitude
    &longitude=longitude
    &miles=radius
    &fips=fipsList
    &city=cityName
    &zipcodeList=zipcodeList
    &take=take
    &skip=skip
    &serviceIds=serviceIdList
    &audiencetypeIds=audienceTypeIdList
    &gradeLevelIds=gradeLevelIdList
    &interestIds=interestIdList
  </pre>
</p>

<p><b>Notes:</b></p>
<ol>
  <li>Either a zipcode+radius, latitude+longitude+radius, a fips code list, a city, or a zipcode list should be specified at minimum (A zipcode list returns locations that are within the list of zipcodes.  For example, I could get locations in Peoria and Bloomington by passing a CSV list of Peoria and Bloomington Zipcodes in the zipcodeList parameter).  Radius can go with either zipcode or latitude/longitude.  All lists are CSV lists, and FIPS code lists are assumed to be Illinois county FIPS codes.</li>
  <li>City searches by city name.  "E. Peoria" and "East Peoria" are NOT the same thing.  Events should be entered with the city names spelled out instead of using abbreviations.</li>
  <li>The Grade Levels returned by the GradeLevel GET method return "All" and "Not Applicable."  These two values are special.  "Not Applicable" exists for validation purposes only, since we require the user to select at least one grade level.  It is never stored in the database, so please do not search for it.  "All" searches for grades "Pre-Kindergarten" through "Technical."  It is not stored in the database either.  If you want to search by "All" you should search by all grade levels from "Pre-Kindergarten" through "Technical."  Note that this behavior will return different results from leaving the filter blank.  Leaving the filter blank will not apply the filter, so you can retrieve locations which do not have a grade level associated with them (this is the "Not Applicable" behavior), while searching for grade levels "Pre-Kindergarten" through "Technical" will not include locations which do not have a grade level associated with them.</li>
</ol>

<h2>PhoneDetail</h2>
<p>
  <pre>
    GET api/PhoneDetail/id
  </pre>
</p>
<p>Returns details about a given location.</p>

<h2>Audience</h2>
<p>
  <pre>
    GET api/Audience/
  </pre>
</p>
<p>Returns a list of all audiences.</p>

<p>
  <pre>
    GET api/Audience/id
  </pre>
</p>
<p>Returns an Audience record whose Id matches id.</p>

<h2>FieldOfInterest</h2>
<p>
  <pre>
    GET api/FieldOfInterest/
  </pre>
</p>
<p>Returns all Fields of Interest.</p>

<p>
  <pre>
    GET api/FieldOfInterest/id
  </pre>
</p>
<p>Returns the field of interest that matches id​.</p>

<h2>GradeLevel</h2>
<p>
  <pre>
    ​GET api/GradeLevel/
  </pre>
</p>
<p>Returns all Grade Levels.  See notes under PhoneSearch or Search for details on the two special values, "All" and "Not Applicable."</p>

<p>
  <pre>
    GET api/GradeLevel/id
  </pre>
</p>
<p>Returns the grade level that matches id​.</p>

<h2>Service</h2>
<p>
  <pre>
    GET api/Service/
  </pre>
</p>
<p>Returns all Services with their associated Service Categories.</p>

<p>
  <pre>
    GET api/Service/id
  </pre>
</p>
<p>Returns an individual service that matches id​.</p>

<h2>ServiceCategory</h2>
<p>
  <pre>
    GET api/ServiceCategory/
  </pre>
</p>
<p>Returns all service categories with their associated services.</p>

<p>
  <pre>
    GET api/ServiceCategory/id
  </pre>
</p>
<p>Returns an individual service that matches id.</p>

<h2>City</h2>
<p>
  <pre>
    GET api/City/?city=cityName&state=state&take=take
  </pre>
</p>
<p>Returns the first take cities/states that begin with cityName within state.  If take is omitted, the default is the first 10.  If state is omitted, the states of Illinois, Indiana, Iowa, Kentucky, Missouri, and Wisconsin are searched.</p>

