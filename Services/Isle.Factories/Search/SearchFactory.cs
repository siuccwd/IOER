using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Web.Script.Serialization;
using Isle.DTO.Common;

namespace Isle.Factories.Search
{
  public class SearchFactory
  {
    public static AJAXResponse DoSearch( int siteID, string searchText, List<int> tagIDs, int libraryID, int collectionID, List<int> standardIDs, int pageSize, int pageStart, string sortField, string sortOrder )
    {
      if ( searchText == null || searchText == "" )
      {
        searchText = "*";
      }
      //Ensure search text is cleaned up
      searchText = searchText
        .Replace( "(", "" ).Replace( ")", "" )
        .Replace( "<", "" ).Replace( ">", "" )
        .Replace( "{", "" ).Replace( "}", "" )
        .Replace( "\\", "" ).Replace( ".", " " );

      //Get the reference list of filters
      var filters = new Factories.Filters.FiltersFactory().GetFilters( siteID, false, false, false );

      //This holds the list of fields to do full text searches on
      var searchFields = new List<string>() { "title", "description", "url", "urlParts", "gradeLevelAliases", "standardAliases", "keywords", "creator", "publisher", "lrDocID" };
      //Add the fields from the filter list
      foreach ( var filter in filters )
      {
        searchFields.Add( filter.schema + ".tags" );
      }

      //Create the class that gets sent to ElasticSearch
      dynamic query = new
      {
        @bool = new
        {
          must = new List<object>
          {
            new {
              query_string = new {
                query = searchText,
                use_dis_max = true,
                default_operator = "and",
                fields = searchFields
              }
            },
            new {
              terms = new {
                url = new string[] {
                  "http", "https", "ftp"
                }
              }
            }/*,
            new {
              terms = new {
                isleSectionIDs = new int[] {
                  siteID
                }
              }
            }*/
          },
          should = new List<object>
          {
            new {
              match_phrase = new {
                description = new {
                  query = searchText,
                  slop = 1,
                  boost = 5
                }
              }
            }
          }
        }
      };

      //For each ID, look up its associated filter. Construct a list that only includes the filters and tags selected by the user.
      foreach ( var filter in filters )
      {
        var matchedFilters = new Dictionary<string, List<int>>();
        var filterES = filter.schema + ".ids";
        var foundIDs = new List<int>();
        foreach ( var tag in filter.tags )
        {
          if ( tagIDs.Contains( tag.id ) )
          {
            foundIDs.Add( tag.id );
          }
        }
        if ( foundIDs.Count() > 0 )
        {
          matchedFilters.Add( filterES, foundIDs );
          query.@bool.must.Add(
            new
            {
              terms = matchedFilters
            }
          );
        }
      }

      if ( libraryID != 0 )
      {
        query.@bool.must.Add(
          new
          {
            terms = new
            {
              libraryIDs = new List<int>() { libraryID }
            }
          }
        );
      }

      if ( collectionID != 0 )
      {
        query.@bool.must.Add(
          new
          {
            terms = new
            {
              collectionIDs = new List<int>() { collectionID }
            }
          }
        );
      }

      //Handle standard IDs
      if ( standardIDs.Count > 0 )
      {
        var standards = new Dictionary<string, List<int>>();
        standards.Add( "standards.ids", standardIDs );
        query.@bool.must.Add(
          new
          {
            terms = standards
          }
        );
      }

      dynamic jsonQ;

      //Handle sorting
      if ( sortField != null && sortField != "" && sortOrder != null && sortOrder != "" )
      {
        var sorter = new Dictionary<string, string>();
        sorter.Add( sortField, sortOrder );
        jsonQ = new
        {
          sort = sorter,
          size = pageSize,
          from = pageStart,
          query = query
        };
      }
      else
      {
        jsonQ = new
        {
          size = pageSize,
          from = pageStart,
          query = query
        };
      }

      var jsonQuery = new JavaScriptSerializer().Serialize( jsonQ );

      return SearchContact.ContactServer( "POST", jsonQuery, "_search/" );

    }

    public static AJAXResponse DoSearchOld( int siteID, string searchText, List<int> tagIDs, int pageSize, int pageStart, string sortField, string sortOrder )
    {
      if ( searchText == null || searchText == "" )
      {
        searchText = "*";
      }
      //Ensure search text is cleaned up
      searchText = searchText
        .Replace( "(", "" ).Replace( ")", "" )
        .Replace( "<", "" ).Replace( ">", "" )
        .Replace( "{", "" ).Replace( "}", "" )
        .Replace( "\\", "" ).Replace(".", " ");

      //Setup the list of fields to do full-text searches on
      string[] searchFields = new string[] { "accessRights", "audiences", "clusters", "creator", "description", "gradeLevelAliases", "gradeLevels", "educationalUses", "groupTypes", "keywords", "languages", "mediaTypes", "notationParts", "publisher", "resourceTypes", "standardNotations", "subjects", "submitter", "title", "url", "urlParts", "usageRights" };

      //Create the class that gets sent to ElasticSearch
      dynamic query = new
      {
        @bool = new
        {
          must = new List<object>
          {
            new {
              query_string = new {
                query = searchText,
                use_dis_max = true,
                default_operator = "and",
                fields = searchFields
              }
            },
            new {
              terms = new {
                url = new string[] {
                  "http", "https", "ftp"
                }
              }
            }
          },
          should = new List<object>
          {
            new {
              match_phrase = new {
                description = new {
                  query = searchText,
                  slop = 1,
                  boost = 5
                }
              }
            }
          }
        }
      };

      //Get the reference list of filters
      var filters = new Factories.Filters.FiltersFactory().GetFilters( siteID, false, false, false );

      //For each ID, look up its associated filter. Construct a list that only includes the filters and tags selected by the user.
      foreach ( var filter in filters )
      {
        var matchedFilters = new Dictionary<string, List<int>>();
        var filterES = filter.schema;
        var foundIDs = new List<int>();
        foreach ( var tag in filter.tags )
        {
          if ( tagIDs.Contains( tag.id ) )
          {
            foundIDs.Add( tag.id );
          }
        }
        if ( foundIDs.Count() > 0 )
        {
          matchedFilters.Add( filterES + "IDs", foundIDs );
          query.@bool.must.Add(
            new
            {
              terms = matchedFilters
            }
          );
        }
      }

      dynamic jsonQ;

      //Handle sorting
      if ( sortField != null && sortField != "" && sortOrder != null && sortOrder != "" )
      {
        var sorter = new Dictionary<string, string>();
        sorter.Add( sortField, sortOrder );
        jsonQ = new
        {
          sort = sorter,
          size = pageSize,
          from = pageStart,
          query = query
        };
      }
      else
      {
        jsonQ = new
        {
          size = pageSize,
          from = pageStart,
          query = query
        };
      }

      var jsonQuery = new JavaScriptSerializer().Serialize( jsonQ );

      //return Isle.Factories.Common.Utilities.Respond( jsonQuery, false, "", null );

      return SearchContact.ContactServer( "POST", jsonQuery, "_search/" );
    }
  }
}
