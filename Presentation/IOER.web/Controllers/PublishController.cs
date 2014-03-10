using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


using ILPathways.Library;
using LRWarehouse.Business;
using LRWarehouse.DAL;
using Resource = LRWarehouse.Business.Resource;
using Version = LRWarehouse.Business.ResourceVersion;
using IOERUser = LRWarehouse.Business.Patron;

using LearningRegistry.RDDD;
using LearningRegistry;
using System.IO;
using ILPathways.Utilities;
using System.Web.Script.Serialization;

using System.Threading;

namespace ILPathways.Controllers
{
  public class PublishController : BaseUserControl
  {
    public ResourceDataManager dataManager = new ResourceDataManager();

    #region Publish Methods
    public void PublishToAll( Resource input, ref bool isSuccessful, ref string status, ref int versionID, ref int intID, ref string sortTitle, bool skipLRPublish )
    {
      bool success = true;
      string tempStatus = "";
      string lrDocID = "";
      //Publish to LR. This will give us an LR Doc ID
      if ( !skipLRPublish )
      {
        PublishToLearningRegistry( input, ref success, ref tempStatus, ref lrDocID );
        if ( !success && !new Services.WebDALService().IsLocalHost() )
        {
          isSuccessful = false;
          SetConsoleErrorMessage( "Error: " + tempStatus );
          status = status + " " + tempStatus + " ";
          versionID = 0;
          return;
        }
      }
      input.Version.LRDocId = lrDocID;

      //If successful, publish to Database. This will give us a Resource Version ID
      PublishToDatabase( input, ref success, ref tempStatus, ref versionID, ref intID, ref sortTitle );
      if ( !success )
      {
        isSuccessful = false;
        SetConsoleErrorMessage( "Error: " + tempStatus );
        status = status + " " + tempStatus + " ";
        versionID = 0;
        return;
      }
      input.Version.Id = versionID;
      input.Version.ResourceIntId = intID;
      input.Id = intID;

      //If successful, publish to ElasticSearch
      PublishToElasticSearch( input, ref success, ref tempStatus );
      if ( !success )
      {
        isSuccessful = false;
        SetConsoleErrorMessage( "Error: " + tempStatus );
        status = status + " " + tempStatus + " ";
        versionID = 0;
        return;
      }

      isSuccessful = true;
      status = "okay";
      SetConsoleSuccessMessage( "Successfully published the Resource" );

      //new ResourceThumbnailManager().CreateThumbnailAsynchronously( input.Id, input.ResourceUrl, false );
      new ResourceThumbnailManager().CreateThumbnail( input.Id, input.ResourceUrl );
    }

    public void PublishToLearningRegistry( Resource input, ref bool successful, ref string status, ref string lrDocID )
    {
      //Create payload
      var payload =  new ResourceJSONManager().GetJSONLRMIFromResource( input );

      //Create document
      lr_document doc = new lr_document();
      doc.resource_data_type = "metadata";
      doc.payload_placement = "inline";
      doc.payload_schema = new List<string> { "LRMI" };
      doc.resource_data = payload;
      doc.resource_locator = input.ResourceUrl;

      //Identity info
      lr_identity identity = new lr_identity();
      identity.submitter_type = "agent";
      identity.submitter = "ISLE OER on Behalf of " + input.Version.Submitter;
      identity.signer = "ISLE OER";
      doc.identity = identity;

      //keywords
      foreach ( ResourceChildItem word in input.Keyword )
      {
        doc.keys.Add( word.OriginalValue.Trim() );
      }

      //Sign the document
      string PgpKeyringLocation = Path.Combine( HttpRuntime.AppDomainAppPath, "App_Data/lrpriv.asc" );
      string keyData = File.ReadAllText( PgpKeyringLocation );
      string[] PublicKeyLocations = new string[] { "http://pgp.mit.edu:11371/pks/lookup?op=get&search=0x6ce0837335049763" };
      string UserID = "ISLEOER (Data Signing Key) <info@siuccwd.com>";
      string password = "89k7SMteVzPUY";
      PgpSigner signer = new PgpSigner( PublicKeyLocations, keyData, UserID, password );
      doc = signer.Sign( doc );

      //Build the envelope
      lr_Envelope envelope = new lr_Envelope();
      envelope.documents.Add( doc );

      //Do publish
      string node = UtilityManager.GetAppKeyValue( "learningRegistryNodePublish" ); //"https://node01.public.learningregistry.net", "http://sandbox.learningregistry.org/"
      string clientID = "info@siuccwd.com";
      string clientPassword = "in5t@ll3r";
      LRClient client = new LRClient( node, clientID, clientPassword );
      try
      {
        //Do publish
        PublishResponse response = client.Publish( envelope );

        //Set return values
        successful = true;
        lrDocID = response.document_results.ElementAt( 0 ).doc_ID;
        status = "Successfully published. LR Doc ID: " + lrDocID;
      }
      catch ( Exception ex )
      {
        successful = false;
        lrDocID = "";
        status = "Failed to Publish: " + ex.Message;
      }
    }

    public void PublishToDatabase( Resource input, ref bool successful, ref string status, ref int versionID, ref int intID, ref string sortTitle )
    {
      try
      {
        //Resource
        intID = new ResourceManager().Create( input, ref status );
        input.Id = intID;
        input.Version.ResourceIntId = intID;

        //Version
        var versionManager = new ResourceVersionManager();
        versionID = versionManager.Create( input.Version, ref status );
        input.Version.Id = versionID;

        //Tags
        var tags = new Dictionary<List<ResourceChildItem>, string>
        {
          { input.ClusterMap, "careerCluster" },
          { input.EducationalUse, "educationalUse" },
          { input.ResourceFormat, "mediaType" },
          { input.Gradelevel, "gradeLevel" },
          { input.GroupType, "groupType" },
          { input.Audience, "endUser" },
          { input.ItemType, "itemType" },
          { input.Language, "language" },
          { input.ResourceType, "resourceType" },
          { input.SubjectMap, "subject" },
          { input.relatedURL, "originalVersionURL" }
        };
        foreach ( KeyValuePair<List<ResourceChildItem>, string> entry in tags )
        {
          CreateMVFs( entry.Key, findClass( entry.Value ), intID, input.CreatedById );
        }

        //Keywords
        var keywordManager = new ResourceKeywordManager();
        foreach ( ResourceChildItem word in input.Keyword )
        {
          word.ResourceIntId = intID;
          keywordManager.Create( word, ref status );
        }

        //Standards
        var standardManager = new ResourceStandardManager();
        foreach ( ResourceStandard standard in input.Standard )
        {
          standardManager.Create( standard, ref status );
        }

        sortTitle = versionManager.Get( versionID ).SortTitle.Replace(" ", "_");
        successful = true;
        status = "okay";
      }
      catch ( Exception ex )
      {
        successful = false;
        status = ex.Message;
        return;
      }
    }

    public void PublishToElasticSearch( Resource input, ref bool successful, ref string status )
    {
      //Do create
      try
      {
        new ElasticSearchManager().CreateOrReplaceRecord( input.Id );
        successful = true;
        status = "okay";
      }
      catch ( Exception ex )
      {
        successful = false;
        status = ex.Message;
      }
    }

    #endregion

    #region Helper Methods
    public void SendPublishNotification( IOERUser user, Resource resource )
    {
        string toEmail = UtilityManager.GetAppKeyValue( "contactUsMailTo", "info@ilsharedlearning.org" );
        string cc = UtilityManager.GetAppKeyValue( "onPublishCC", "" );
        string bcc = UtilityManager.GetAppKeyValue( "systemAdminEmail", "mparsons@siuccwd.com" );
        string subject = string.Format( "IOER - New publish notification from: {0}", user.FullName() );
        string body = string.Format( "<p>{0} published a new resource to the Learning Registry. </p>", user.FullName() );
        if ( resource.Version != null )
        {
            body += "<br/>Resource: " + resource.Version.Title;
            body += "<br/>" + resource.Version.Description;
        }
        //body += "<br/>Target Url: <span>" + resource.ResourceUrl + "</span>   ";
        body += "<br/><br/>Target Url: " + string.Format( "<a href='{0}'>{1}</a>", resource.ResourceUrl, resource.ResourceUrl );

        //string url = UtilityManager.FormatAbsoluteUrl( string.Format( "/ResourceDetail.aspx?vid={0}", resource.Version.Id ), true );
        string title = FormatFriendlyTitle( resource.Version.Title );
        string url2 = FormatFriendlyResourceUrl( resource.Version );   // UtilityManager.FormatAbsoluteUrl( string.Format( "/IOER/{0}/{1}", resource.Version.Id, title ), true );

        body += "<br/><br/>Detail Url: " + string.Format( "<a href='{0}'>View published resource</a>", url2 );

        body += "<br/>From: " + user.EmailSignature();
        //string from = applicant.Email;
        EmailManager.SendEmail( toEmail, toEmail, subject, body, cc, bcc );
    }
    /// <summary>
    /// Format a friendly url for resource
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public string FormatFriendlyResourceUrl( ResourceVersion entity )
    {
        string title = FormatFriendlyTitle( entity.Title );
        return UtilityManager.FormatAbsoluteUrl( string.Format( "/IOER/{0}/{1}", entity.Id, title ), true );
    }

    /// <summary>
    /// Format a string for friendly url display
    /// NOTE: with change to use the sort title, a number of the rules can be skipped
    /// ==> note that we need to handle authored content first
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public string FormatFriendlyTitle( string text )
    {
        if ( text == null || text.Trim().Length == 0 )
            return "";

        string title = text;
        title = title.Replace( " - ", "-" );
        //convert
        title = title.Replace( ".", "-" );
        title = title.Replace( "%", "percent" );
        //remove

        title = title.Replace( "&#039;", "" );
        title = title.Replace( ",", "" );
        title = title.Replace( "'", "" );
        title = title.Replace( "$", "" );
        title = title.Replace( "+", "" );
        title = title.Replace( "#", "" );
        title = title.Replace( "?", "" );
        title = title.Replace( ":", "" );
        title = title.Replace( "\"", "" );
        title = title.Replace( ")", "" );
        title = title.Replace( "(", "" );

        title = title.Replace( " ", "_" );

        title = HttpUtility.HtmlEncode( title );
        return title;
    }//

    private void CreateMVFs( List<ResourceChildItem> input, ResourceDataManager.IResourceDataSubclass className, int intID, int createdByID )
    {
      foreach ( ResourceChildItem item in input )
      {
        dataManager.Create( className, intID, item.CodeId, item.OriginalValue, createdByID );
      }
    }
    private ResourceDataManager.IResourceDataSubclass findClass( string tableName )
    {
      return ResourceDataManager.ResourceDataSubclassFinder.getSubclassByName( tableName );
    }

    #endregion
  }
}