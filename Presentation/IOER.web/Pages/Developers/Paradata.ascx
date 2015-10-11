<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Paradata.ascx.cs" Inherits="IOER.Pages.Developers.Paradata" %>

<p>Paradata is data about the use of a Learning Resource.  Paradata includes but is not limited to counts of views and favorites, ratings, and comments.</p>

<dl>
  <dt>View Counts</dt>
  <dd>Counts of the number of times a resource is viewed</dd>
  <dt>Favorites</dt>
  <dd>Counts of the number of times a resource is added to a library</dd>
  <dt>Comments</dt>
  <dd>Comments are stored in the Resource.Comments table, along with the date they were made.  This facilitates displaying the comments in date order.</dd>
  <dt>Ratings, Likes, and Dislikes</dt>
  <dd>
    Ratings in the LR are one of three things:
    <ul>
      <li>A general rating about the resource.  Presently nobody is publishing likes and dislikes to the LR, so general ratings will be converted to likes/dislikes.</li>
      <li>A degree of alignment of a resource to a learning standard. These will be kept as ratings.</li>
      <li>An evaluation of a resource based on a rubric. These will also be kept as ratings.</li>
    </ul>
  </dd>
</dl>

<h2>Converting General Ratings to Likes/Dislikes</h2>
<p>Presently in our system, ratings from the LR are converted to a 1-5 scale, summarized, and stored in our database.  Here is how we will convert rating summaries currently stored in our database to likes and dislikes:</p>
<ol>
  <li>Add a "Like/Dislike" table to the database which will contain counts of likes and dislikes.</li>
  <li>Convert existing ratings that are tied to neither a standard nor a rubric to a "Like/Dislike" count:</li>
  <ul>
    <li>0 - 2.40 = all dislikes</li>
    <li>2.40-3.40 = percentage split</li>
    <li>3.40+ = all likes</li>
    <li>The formula to use for the percentage split conversion is (value - min) / range * 100%. The range discussed in our meeting was 1.0 because the range was 2.4 to 3.4. The range for percentage is (3.40 - 2.40) = 1.00. Thus the formula to use is (value - 2.40) / 1.00 * 100%.</li>
    <li>100 ratings of 2.40 = ((2.40 - 2.40) / 1.00) * 100% = 0 likes and 100 dislikes.</li>
    <li>100 ratings of 2.60 = ((2.60 - 2.40) / 1.00) * 100% = 20 likes and 80 dislikes.</li>
    <li>100 ratings of 2.90 = ((2.90 - 2.40) / 1.00) * 100% = 50 likes and 50 dislikes.</li>
    <li>100 ratings of 3.20 = ((3.20 - 2.40) / 1.00) * 100% = 80 likes and 20 dislikes.</li>
    <li>100 ratings of 3.40 = ((3.40 - 2.40) / 1.00) * 100% = 100 likes and 0 dislikes.</li>
  </ul>
  <li>New ratings coming in from the learning registry will be converted to likes/dislikes using the same algorithm outlined above.</li>
  <li>We will continue to store the general ratings in the rating summary table in addition to the Likes and Dislikes summary table, so the data will not be lost if we later decide to switch from like/dislike back to a rating scale.</li>
</ol>