using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Script.Serialization;

using ILPathways.Library;
using ILPathways.Utilities;
using Isle.BizServices;
using LearningRegistry;
using LearningRegistry.RDDD;
using LRWarehouse.Business;
using LRWarehouse.DAL;
using IOERUser = LRWarehouse.Business.Patron;
using Resource = LRWarehouse.Business.Resource;
using Thumbnailer = LRWarehouse.DAL.ResourceThumbnailManager;
using Version = LRWarehouse.Business.ResourceVersion;

namespace ILPathways.Controllers
{
  class PublishController : BaseUserControl
  {
    public ResourceDataManager dataManager = new ResourceDataManager();

    //#region Publish Methods
    //  /// <summary>
    //  /// Publish a resource to:
    //  /// - LR
    //  /// - Database
    //  /// - ElasticSearch
    //  /// </summary>
    //  /// <param name="input"></param>
    //  /// <param name="isSuccessful"></param>
    //  /// <param name="status"></param>
    //  /// <param name="versionID"></param>
    //  /// <param name="intID"></param>
    //  /// <param name="sortTitle"></param>
    ///// <param name="updatingElasticSearch"></param>
    //  /// <param name="skipLRPublish"></param>
    //public void PublishToAll( Resource input, 
    //            ref bool isSuccessful, 
    //            ref string status, 
    //            ref int versionID, 
    //            ref int intID, 
    //            ref string sortTitle,
    //            bool updatingElasticSearch,
    //            bool skipLRPublish )
    //{
    //  bool success = true;
    //  string tempStatus = "";
    //  string lrDocID = "";
    //  string continueOnPublishError = UtilityManager.GetAppKeyValue( "continueOnPublishError", "yes" );

    //  //Publish to LR. This will give us an LR Doc ID
    //  if ( !skipLRPublish )
    //  {
    //    PublishToLearningRegistry( input, ref success, ref tempStatus, ref lrDocID );
    //    if ( !success && !new Services.WebDALService().IsLocalHost() )
    //    {
    //        if ( continueOnPublishError == "no" )
    //        {
    //            isSuccessful = false;
    //            SetConsoleErrorMessage( "Error: " + tempStatus );
    //            status = status + " " + tempStatus + " ";
    //            versionID = 0;
    //            return;
    //        }
    //        else
    //        {
    //            EmailManager.NotifyAdmin( "Error during LR Publish", "Error: " + tempStatus + "<p>The error was encountered during the LR publish. The system continued with saving to the database and elastic search. </p>" );
    //        }
    //    }

    //  }
    //  input.Version.LRDocId = lrDocID;

    //  //If successful, publish to Database. This will give us a Resource Version ID
    //  PublishToDatabase( input, ref success, ref tempStatus, ref versionID, ref intID, ref sortTitle );
    //  if ( !success )
    //  {
    //    isSuccessful = false;
    //    SetConsoleErrorMessage( "Error: " + tempStatus );
    //    status = status + " " + tempStatus + " ";
    //    versionID = 0;
    //    return;
    //  }
    //  input.Version.Id = versionID;
    //  input.Version.ResourceIntId = intID;
    //  input.Id = intID;

    //  //If successful, publish to ElasticSearch
    //  if ( updatingElasticSearch )
    //  {
    //      PublishToElasticSearch( input, ref success, ref tempStatus );
    //      if ( !success )
    //      {
    //          isSuccessful = false;
    //          SetConsoleErrorMessage( "Error: " + tempStatus );
    //          status = status + " " + tempStatus + " ";
    //          versionID = 0;
    //          return;
    //      }
    //  }
    //  isSuccessful = true;
    //  status = "okay";
    //  SetConsoleSuccessMessage( "Successfully published the Resource" );

    //  //new ResourceThumbnailManager().CreateThumbnailAsynchronously( input.Id, input.ResourceUrl, false );
    //  new Thumbnailer().CreateThumbnail( input.Id, input.ResourceUrl );

    //  SendPublishNotification( (Patron) WebUser, input );
    //}

    //public lr_Envelope CreateLREnvelope( Resource input, ref bool successful, ref string status )
    //{
    //  //Create payload
    //  var payload = new ResourceJSONManager().GetJSONLRMIFromResource( input );

    //  //Create document
    //  lr_document doc = new lr_document();
    //  doc.resource_data_type = "metadata";
    //  doc.payload_placement = "inline";
    //  doc.payload_schema = new List<string> { "LRMI" };
    //  doc.resource_data = payload;
    //  doc.resource_locator = input.ResourceUrl;

    //  //Identity info
    //  lr_identity identity = new lr_identity();
    //  identity.submitter_type = "agent";
    //  identity.submitter = "ISLE OER on Behalf of " + input.Version.Submitter;
    //  identity.signer = "ISLE OER";
    //  doc.identity = identity;

    //  //keywords
    //  foreach ( ResourceChildItem word in input.Keyword )
    //  {
    //    doc.keys.Add( word.OriginalValue.Trim() );
    //  }

    //  //Sign the document
    //  string PgpKeyringLocation = Path.Combine( HttpRuntime.AppDomainAppPath, "App_Data/lrpriv.asc" );
    //  string keyData = File.ReadAllText( PgpKeyringLocation );
    //  string[] PublicKeyLocations = new string[] { "http://pgp.mit.edu:11371/pks/lookup?op=get&search=0x6ce0837335049763" };
    //  string UserID = "ISLEOER (Data Signing Key) <info@siuccwd.com>";
    //  string password = "89k7SMteVzPUY";
    //  PgpSigner signer = new PgpSigner( PublicKeyLocations, keyData, UserID, password );
    //  doc = signer.Sign( doc );

    //  //Build the envelope
    //  lr_Envelope envelope = new lr_Envelope();
    //  envelope.documents.Add( doc );

    //  return envelope;
    //}

    //public void PublishToLearningRegistry( Resource input, ref bool successful, ref string status, ref string lrDocID )
    //{
    //  var envelope = CreateLREnvelope( input, ref successful, ref status );
    //  PublishToLearningRegistry( envelope, ref successful, ref status, ref lrDocID );
    //}
    //public void PublishToLearningRegistry( lr_Envelope envelope, ref bool successful, ref string status, ref string lrDocID )
    //{
    //  //Do publish
    //  string node = UtilityManager.GetAppKeyValue( "learningRegistryNodePublish" ); //"https://node01.public.learningregistry.net", "http://sandbox.learningregistry.org/"
    //  string clientID = "info@siuccwd.com";
    //  string clientPassword = "in5t@ll3r";
    //  LRClient client = new LRClient( node, clientID, clientPassword );
    //  try
    //  {
    //    //Do publish
    //    PublishResponse response = client.Publish( envelope );

    //    //Set return values
    //    successful = true;
    //    lrDocID = response.document_results.ElementAt( 0 ).doc_ID;
    //    status = "Successfully published. LR Doc ID: " + lrDocID;
    //  }
    //  catch ( Exception ex )
    //  {
    //    successful = false;
    //    lrDocID = "";
    //    status = "Failed to Publish: " + ex.Message;
    //  }

    //}

    //public void PublishToDatabase( Resource input, ref bool successful, ref string status, ref int versionID, ref int intID, ref string sortTitle )
    //{
    //  try
    //  {
    //    //Resource
    //    intID = new ResourceManager().Create( input, ref status );
    //    input.Id = intID;
    //    input.Version.ResourceIntId = intID;

    //    //Version
    //    var versionManager = new ResourceVersionManager();
    //    versionID = versionManager.Create( input.Version, ref status );
    //    input.Version.Id = versionID;

    //    //Tags
    //    var tags = new Dictionary<List<ResourceChildItem>, string>
    //    {
    //      { input.ClusterMap, "careerCluster" },
    //      { input.EducationalUse, "educationalUse" },
    //      { input.ResourceFormat, "mediaType" },
    //      { input.Gradelevel, "gradeLevel" },
    //      { input.GroupType, "groupType" },
    //      { input.Audience, "endUser" },
    //      { input.ItemType, "itemType" },
    //      { input.Language, "language" },
    //      { input.ResourceType, "resourceType" },
    //      { input.SubjectMap, "subject" },
    //      { input.relatedURL, "originalVersionURL" }
    //    };
    //    foreach ( KeyValuePair<List<ResourceChildItem>, string> entry in tags )
    //    {
    //      CreateMVFs( entry.Key, findClass( entry.Value ), intID, input.CreatedById );
    //    }

    //    //Keywords
    //    var keywordManager = new ResourceKeywordManager();
    //    foreach ( ResourceChildItem word in input.Keyword )
    //    {
    //      word.ResourceIntId = intID;
    //      keywordManager.Create( word, ref status );
    //    }

    //    //Standards
    //    var standardManager = new ResourceStandardManager();
    //    foreach ( ResourceStandard standard in input.Standard )
    //    {
    //      standard.ResourceIntId = intID;
    //      standardManager.Create( standard, ref status );
    //    }

    //    sortTitle = versionManager.Get( versionID ).SortTitle.Replace(" ", "_");
    //    successful = true;
    //    status = "okay";
    //  }
    //  catch ( Exception ex )
    //  {
    //    successful = false;
    //    status = ex.Message;
    //    return;
    //  }
    //}

    //public void PublishToElasticSearch( Resource input, ref bool successful, ref string status )
    //{
    //  //Do create
    //  try
    //  {
    //    //new ElasticSearchManager().CreateOrReplaceRecord( input.Id );
    //    new ElasticSearchManager().RefreshResource( input.Id );
    //    successful = true;
    //    status = "okay";
    //  }
    //  catch ( Exception ex )
    //  {
    //    successful = false;
    //    status = ex.Message;
    //  }
    //}

    //public void BuildSaveLRDocument( Resource input, ref bool successful, ref string status )
    //{
    //  var envelope = CreateLREnvelope( input, ref successful, ref status );
    //  var pending = new PublishPending()
    //  {
    //    ResourceId = input.Id,
    //    ResourceVersionId = input.Version.Id,
    //    Reason = "Resource requires approval.",
    //    CreatedById = input.CreatedById,
    //    LREnvelope = new JavaScriptSerializer().Serialize(envelope)
    //  };

    //  new ResourceManager().PublishPending_Create( pending, ref status );

    //}

    //#endregion

    //#region Helper Methods
    //public void SendPublishNotification( IOERUser user, Resource resource )
    //{
    //    string toEmail = UtilityManager.GetAppKeyValue( "contactUsMailTo", "info@ilsharedlearning.org" );
    //    string cc = UtilityManager.GetAppKeyValue( "onPublishCC", "" );
    //    string bcc = UtilityManager.GetAppKeyValue( "appAdminEmail", "mparsons@siuccwd.com" );
    //    string fromEmail = user.Email;

    //    string subject = string.Format( "IOER - New publish notification from: {0}", user.FullName() );
    //    string body = string.Format( "<p>{0} published a new resource to IOER. </p>", user.FullName() );
    //    if ( resource.Version != null )
    //    {
    //        body += "<br/>Resource: " + resource.Version.Title;
    //        body += "<br/>" + resource.Version.Description;
    //    }
    //    //body += "<br/>Target Url: <span>" + resource.ResourceUrl + "</span>   ";
    //    body += "<br/><br/>Target Url: " + string.Format( "<a href='{0}'>{1}</a>", resource.ResourceUrl, resource.ResourceUrl );

    //    //string url = UtilityManager.FormatAbsoluteUrl( string.Format( "/ResourceDetail.aspx?vid={0}", resource.Version.Id ), true );
    //    //string title = FormatFriendlyTitle( resource.Version.Title );
    //    string url2 = ResourceBizService.FormatFriendlyResourceUrl( resource.Version );   
        

    //    body += "<br/><br/>Detail Url: " + string.Format( "<a href='{0}'>View published resource</a>", url2 );

    //    body += "<br/>From: " + user.EmailSignature();
    //    //string from = applicant.Email;
    //    EmailManager.SendEmail( toEmail, fromEmail, subject, body, cc, bcc );
    //}

    //private void CreateMVFs( List<ResourceChildItem> input, ResourceDataManager.IResourceDataSubclass className, int intID, int createdByID )
    //{
    //  foreach ( ResourceChildItem item in input )
    //  {
    //    dataManager.Create( className, intID, item.CodeId, item.OriginalValue, createdByID );
    //  }
    //}
    //private ResourceDataManager.IResourceDataSubclass findClass( string tableName )
    //{
    //  return ResourceDataManager.ResourceDataSubclassFinder.getSubclassByName( tableName );
    //}

    //#endregion
  }
}