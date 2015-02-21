using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

using ILPathways.Utilities;
using Isle.BizServices;
using Isle.DTO;
using Isle.DTO.Filters;
using LRWarehouse.Business;
using IOERBusinessEntities;

namespace Isle.Factories.Filters
{
  public class FiltersFactory
  {
    /// <summary>
    /// Gets a list of filters for the selected site, but only returns filters that contain the tags in the tagIDs field.
    /// </summary>
    /// <param name="tagIDs">The list of "absolute" tag IDs to return filters for.</param>
    /// <param name="onlyIncludeMatchingTags">If true, don't return any tags whose IDs aren't found in tagIDs. If false, include all tags for each filter returned.</param>
    /// <param name="siteID">The ID of the site to select filters for.</param>
    /// <param name="includeDescriptions"></param>
    /// <param name="includeCounts"></param>
    /// <returns>A list of Filter objects whose tags contain at least one of the tagIDs.</Filter></returns>
    public List<Filter> GetFiltersForTags( List<int> tagIDs, bool onlyIncludeMatchingTags, int siteID, bool includeDescriptions, bool includeCounts )
    {
      //Get the full list of filters for the site
      var filters = GetFilters( siteID, false, includeDescriptions, includeCounts );

      //For each tag ID, populate the appropriate filter
      var output = new List<Filter>();
      foreach ( var filter in filters )
      {
        var foundTags = new List<Tag>();
        foreach ( var tag in filter.tags )
        {
          if ( tagIDs.Contains( tag.id ) )
          {
            foundTags.Add( tag );
          }
        }
        if ( foundTags.Count() > 0 )
        {
          if ( onlyIncludeMatchingTags )
          {
            filter.tags = foundTags;
          }
          output.Add( filter );
        }
      }

      //Return the list
      return output;
    }


    public List<Filter> GetFilters( int siteID, bool mustHaveValues, bool includeDescriptions, bool includeCounts )
    {
      bool valid = false;
      string status = "";
      return GetFilters( siteID, mustHaveValues, includeDescriptions, includeCounts, ref valid, ref status );
    }


    public List<Filter> GetFilters( int siteID, bool mustHaveValues, bool includeDescriptions, bool includeCounts, ref bool valid, ref string status )
    {
      LogLine( "Beginning Filter Get" );
        var cachingFilters = UtilityManager.GetAppKeyValue( "cachingFilters", "yes" );
        if ( cachingFilters == "yes" )
        {
            if ( HttpContext.Current.Cache[ "filters" ] != null )
            {
                //Attempt to return a cached version if it matches the requirements and is less than an hour old
                var temp = ( List<CachedFilters> ) HttpContext.Current.Cache[ "filters" ];
                try
                {
                    var target = temp.Where(
                      thing => ( thing.lastUpdated > DateTime.Now.AddHours( -1 ) &&
                      thing.siteID == siteID && thing.mustHaveValues == mustHaveValues &&
                      thing.includeDescriptions == includeDescriptions &&
                      thing.includeCounts == includeCounts )
                    ).Last();
                    if ( target != null )
                    {
                        return target.filters;
                    }
                }
                catch ( Exception ex )
                {
                    var debug = ex;
                    string thing = ex.Message;
                    //continue the process
                }
            }
        }

      var output = new List<Filter>();
      try
      {
        LogLine( "Site ID: " + siteID );
        CodesSite site = EFCodesManager.Codes_Site_Get( siteID, mustHaveValues );
        LogLine( "CodesSite acquired" );
        if ( site.SiteTagCategories.Count == 0 )
        {
          throw new Exception( "Error: No Filters found for ID " + siteID );
        }
        //should already be in this order
        site.SiteTagCategories.OrderBy( item => item.SortOrder ).ThenBy( item => item.Title);

        foreach ( CodesSiteTagCategory inFilter in site.SiteTagCategories )
        {
          if ( inFilter.TagCategory.TagValues.Count() == 0 )
          {
            continue;
          }
          //dropped/hidden metadata fields
          if ( inFilter.SchemaTag == "assessmentType" 
              || inFilter.SchemaTag == "groupType" 
              || inFilter.SchemaTag == "educationalUse" )
          {
            continue;
          }

          var outFilter = new Filter();
          outFilter.serializeDescription = includeDescriptions;

          outFilter.id = inFilter.Id;
          outFilter.title = inFilter.Title;
          outFilter.description = inFilter.Description;
          outFilter.schema = inFilter.SchemaTag;
          outFilter.codeID = inFilter.CategoryId;

          inFilter.TagCategory.TagValues.OrderBy( item => item.SortOrder).ThenBy( item => item.Title);

          foreach ( CodesTagValue inTag in inFilter.TagCategory.TagValues )
          {
            var outTag = new Tag();
            outTag.serializeDescription = includeDescriptions;
            outTag.serializeCounts = includeCounts;

            outTag.codeID = inTag.CodeId;
            outTag.id = inTag.Id;
            outTag.title = inTag.Title;
            outTag.description = inTag.Description;
            outTag.counts = inTag.WarehouseTotal;
            if ( inTag.TagValueKeywords != null )
            {
                TagValueKeyword tvk;
                outTag.hasKeywords = true;
                //actually only do this if tagging, or ensure it doesn't mess up search view
                foreach ( CodesTagValueKeyword kw in inTag.TagValueKeywords ) 
                {
                    outTag.keywords.Add( kw.Keyword );

                    tvk = new TagValueKeyword();
                    tvk.Id = kw.Id;
                    tvk.TagValueId = kw.TagValueId;
                    tvk.Keyword = kw.Keyword;
                    outTag.tagValueKeywords.Add( tvk );
                }
            }
            outFilter.tags.Add( outTag );
          }

          output.Add( outFilter );
        }
      }
      catch ( Exception ex )
      {
        valid = false;
        status = ex.Message.ToString();
        LoggingHelper.LogError( ex, "FiltersFactory.GetFilters" );
        LogLine( "Error:" );
        LogLine( ex.Message.ToString() );
        LogLine( "" );
        LogLine( "Big Error: " );
        LogLine( ex.ToString() );
        return output;
      }

      valid = true;
      status = "okay";

      //Cache the output
      var cacheThing = new CachedFilters()
          {
            filters = output,
            lastUpdated = DateTime.Now,
            siteID = siteID,
            mustHaveValues = mustHaveValues,
            includeDescriptions = includeDescriptions,
            includeCounts = includeCounts
          };

      if ( HttpContext.Current.Cache[ "filters" ] != null )
      {
        var temp = ( List<CachedFilters> ) HttpContext.Current.Cache[ "filters" ];
        try
        {
          temp.RemoveAll( thing => (
            thing.siteID == siteID && thing.mustHaveValues == mustHaveValues &&
            thing.includeDescriptions == includeDescriptions &&
            thing.includeCounts == includeCounts )
          );
          temp.Add( cacheThing );
          HttpContext.Current.Cache.Insert( "filters", temp );
        }
        catch
        {
          //continue the process
        }
      }
      else
      {
        HttpContext.Current.Cache.Insert( "filters", new List<CachedFilters>() { cacheThing } );
      }

      LogLine( "Logging filters:" );
      LogLine( new System.Web.Script.Serialization.JavaScriptSerializer().Serialize( output ) );
      LogLine( "Filters logged." );

      return output;
    }

    /// <summary>
    /// Retrieve filters for library, and only where associated with a resource.tag for a resource in the library
    /// </summary>
    /// <param name="siteID"></param>
    /// <param name="libraryId"></param>
    /// <param name="valid"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    public List<Filter> GetLibraryFilters( int siteID, int libraryId, ref bool valid, ref string status )
    {
        return GetLibraryFilters( siteID, libraryId, 0, ref valid, ref status );
    }

    /// <summary>
    /// Retrieve filters for collection, and only where associated with a resource.tag for a resource in the collection
    /// </summary>
    /// <param name="siteID"></param>
    /// <param name="collectionId"></param>
    /// <param name="valid"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    public List<Filter> GetCollectionFilters( int siteID, int collectionId, ref bool valid, ref string status )
    {
        return GetLibraryFilters( siteID, 0, collectionId, ref valid, ref status );
    }

    private List<Filter> GetLibraryFilters( int siteID, int libraryId, int collectionId, ref bool valid, ref string status )
    {
        ServiceHelper.DoTrace( "Beginning Filter Get" );

        var output = new List<Filter>();
        try
        {
            ServiceHelper.DoTrace( "Site ID: " + siteID );
            Site site = new Site();
            if ( collectionId > 0 )
                site = LibraryBizService.Collection_GetPresentFiltersOnly( siteID, collectionId );
            else if ( libraryId > 0 )
                site = LibraryBizService.Library_GetPresentFiltersOnly( siteID, libraryId );
            else
                return output;

            ServiceHelper.DoTrace( "CodesSite acquired" );

            if ( site.SiteTagCategories.Count == 0 )
            {
                throw new Exception( string.Format( "Error: No Filters found for request of siteID: {0}, libraryId: {1}, collectionId: {2}  ", siteID, libraryId, collectionId ) );
            }
            //should already be in this order
            site.SiteTagCategories.OrderBy( item => item.SortOrder ).ThenBy( item => item.Title );

            foreach ( SiteTagCategory inFilter in site.SiteTagCategories )
            {
                //not necessary now
                if ( inFilter.TagValues.Count() == 0 )
                {
                    continue;
                }

                var outFilter = new Filter();
                outFilter.serializeDescription = false;

                outFilter.id = inFilter.Id;
                outFilter.title = inFilter.Title;
                outFilter.description = inFilter.Description;
                outFilter.schema = inFilter.SchemaTag;
                outFilter.codeID = inFilter.CategoryId;

                foreach ( TagFilterBase inTag in inFilter.TagValues )
                {
                    var outTag = new Tag();
                    outTag.serializeDescription = false;
                    outTag.serializeCounts = false;

                    outTag.codeID = inTag.codeID;
                    outTag.id = inTag.id;
                    outTag.title = inTag.title;
                    outTag.description = inTag.description;
                    outTag.counts = 0;
                    outFilter.tags.Add( outTag );
                }

                output.Add( outFilter );
            }
        }
        catch ( Exception ex )
        {
            valid = false;
            status = ex.Message.ToString();
            LoggingHelper.LogError( ex, "FiltersFactory.GetFilters" );
   
            ServiceHelper.DoTrace( ex.Message.ToString() );
           
            ServiceHelper.DoTrace( "Big Error:======================================= " );
            ServiceHelper.DoTrace( ex.ToString() );
            return output;
        }

        valid = true;
        status = "okay";


        ServiceHelper.DoTrace( "Logging filters:" );
        ServiceHelper.DoTrace( new System.Web.Script.Serialization.JavaScriptSerializer().Serialize( output ) );
        ServiceHelper.DoTrace( "Filters logged." );

        return output;
    }

    public void LogLine( string text )
    {
      ServiceHelper.DoTrace( 6,text );
    }
  }
}
