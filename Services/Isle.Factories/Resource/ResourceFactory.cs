using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ILPathways.Utilities;
using User = LRWarehouse.Business.Patron;
using ResBiz = Isle.BizServices.ResourceBizService;
using AcctMgr = Isle.BizServices.AccountServices;
//IOER is still ,Net 4.0 so cannot have a reference to a 4.5 project
//using ResDTO = Isle.BizServices.DTO;
using Isle.DTO;
using Isle.Factories.Common;
using LRWarehouse.Business;



namespace Isle.Factories
{
    public class ResourceFactory
    {
        /// <summary>
        /// Add or update a full resource
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="formData"></param>
        /// <returns></returns>
        public bool SaveResource( Isle.DTO.Resource.Resource resource, NameValueCollection formData )
        {
            //future:
            bool skipLRPublish = false;


            ResourceTransformDTO dto = new ResourceTransformDTO();
            //TODO - assume resource is active, but will need to handle scenario where approvals are required.
            try
            {
                User user = new User();
                string authorName = "";
                dto.IsActive = true;
                //Pre-processing and merging special data in from the form
                dto.ResourceTagsIds = Utilities.SplitListInt( formData[ "filter.tags" ], "," );
                dto.ResourceStandardIds = Utilities.SplitListInt( formData[ "hdnStandards" ], "," );
                dto.Resource_Keywords = Utilities.SplitListString( formData[ "hdnKeywords" ], "," );

                int selectedCollectionID = 0;
                Int32.TryParse( formData[ "ddlSelectedCollection" ] , out selectedCollectionID );
                dto.SelectedCollectionID = selectedCollectionID;
                
                int selectedUserID = int.Parse( formData[ "ddlSelectedUserID" ] );
               
                dto.TargetSiteIds = resource.targetSiteIds;

                DateTime createdDate = System.DateTime.Now; 
                System.DateTime.TryParse( resource.created, out createdDate );
                dto.Created = createdDate;

                dto.Resource_Version.Created = dto.Created;

                //if (resource.created.Length
                //will need to convert
                dto.ResourceTagsIds.Add( int.Parse( formData[ "inLanguage" ] ) );
                //OR???
                dto.LanguageId = int.Parse( formData[ "inLanguage" ] );
                //will need to convert
                int accessRightsId = int.Parse( formData[ "accessRights" ] );

                //if id is zero, then create, else update
                dto.Id = resource.id;
                if ( selectedUserID > 0 )
                {
                    user = AcctMgr.GetUser( selectedUserID );

                    dto.CreatedById = selectedUserID;
                    if ( user != null && user.Id > 0 )
                    {
                        authorName = user.FullName();
                        dto.Resource_Version.Submitter = "Isle OER on behalf of " + user.FullName();
                    }
                }
                else
                {
                    dto.Resource_Version.Submitter = "Isle OER";
                }
                
              

                dto.ResourceUrl = resource.url;
                //res.version
                //TODO - ensure there is a resourceVersionID for updates!
                dto.Resource_Version.Id = resource.resourceVersionID;

                //default to active - see previous note
                dto.Resource_Version.IsActive = dto.IsActive;

                dto.Resource_Version.Title = resource.title;
                dto.Resource_Version.Description = resource.description;
                //handle differently for updates, or will be done at EF level
                dto.Resource_Version.DocId = resource.lrDocID;

                dto.Resource_Version.Publisher = resource.publisher;
                dto.Resource_Version.Creator = resource.creator;
                if ( resource.usageRights != null
                    && resource.usageRights.url != null )
                    dto.Resource_Version.Rights = resource.usageRights.url;

                dto.Resource_Version.AccessRightsId = accessRightsId;
                //dto.Resource_Version.Modified = resource.created;

                dto.Resource_Version.IsSkeletonFromParadata = false;
                dto.Resource_Version.Requirements = resource.requirements;
                //be sure to create sortTitle = previously done in proc

                //================================================

                //then save to database
                if ( dto.Id == 0 )
                {
                    int id = ResBiz.ResourceCompleteCreate( dto, skipLRPublish );
                }
                else
                {
                    bool isValid = ResBiz.ResourceCompleteUpdate( dto );
                }

                //Temporary(?) workaround
                try
                {
                  var successful = true;
                  var status = "";
                  var tempID = Isle.BizServices.ResourceBizService.ResourceGet_ViaVersionID( dto.Resource_Version.Id ).Id;
                  Isle.BizServices.PublishingServices.PublishToElasticSearch( tempID, ref successful, ref status );
                  if ( successful == false )
                  {
                    throw new Exception( status );
                  }
                }
                catch ( Exception ex )
                {
                  LoggingHelper.LogError( "Error publishing to ElasticSearch from MVC Tagger:" );
                  LoggingHelper.LogError( ex.Message );
                }

                return true;
            }
            catch ( Exception ex )
            {
                //logging, messages to caller
                LoggingHelper.LogError( ex, "ResourceFactory.SaveResource" );
                return false;
            }
        }

        /// <summary>
        /// Get a full resource for display
        /// Notes:
        /// - access rights is not stored as tag, on RV - confirm handling
        /// - the resource dto has language as a separate tag list- is it not just part of the generic tags?
        /// - Usage rights is an object, but we just store url
        /// </summary>
        /// <param name="resourceId"></param>
        public Isle.DTO.Resource.Resource GetResource( int resourceId )
        {
            ResourceTransformDTO resource = new ResourceTransformDTO();

            Isle.DTO.Resource.Resource output = new DTO.Resource.Resource();
            resource = ResBiz.Resource_CompleteGet( resourceId );

            output.targetSiteIds = resource.TargetSiteIds;

            //resource
            output.id = resource.Id;
            output.url = resource.ResourceUrl;
            output.created = resource.Created.ToShortDateString();

            //not issued - published by id may may important for updating
            output.createdByID = resource.CreatedById;

            //res.version
            output.resourceVersionID = resource.Resource_Version.Id;

            output.title = resource.Resource_Version.Title;
            output.description = resource.Resource_Version.Description;

            output.lrDocID = resource.Resource_Version.DocId;

            output.usageRights = new DTO.Resource.UsageRights();
            output.usageRights.url = resource.Resource_Version.Rights;

            //??
            //fortunately access rights is the first tag, so relative and absolute values match
            output.accessRightsId = resource.Resource_Version.AccessRightsId;
            output.accessRights = new List<DTO.Filters.Tag>();
            output.accessRights.Add(new DTO.Filters.Tag() { id = resource.Resource_Version.AccessRightsId, title = "do not know"});

            //output.Resource_Version.Modified = resource.created;

            output.publisher = resource.Resource_Version.Publisher;
            output.creator = resource.Resource_Version.Creator;
            output.submitter = resource.Resource_Version.Submitter ;
            output.requirements = resource.Resource_Version.Requirements;

            //tags
            output.filterIds = resource.ResourceTagsIds;
            output.keywords = resource.Resource_Keywords;
            output.subjects = resource.Resource_Subjects;

            //standards - what is need, just ids??
            output.standardIds = resource.ResourceStandardIds;
            foreach ( ResourceStandard rs in resource.Standards ) 
            {
                output.standards.Add(new DTO.Resource.Standard() { id = rs.Id, code = rs.StandardNotationCode, description = rs.StandardDescription});

            }


            //TODO - paradata

            //comments (no change)

            //libraries - no change

            return output;
        }


        public static ResourceDTO GetResourceSummary( int resourceId )
        {
            Resource res = ResBiz.Resource_FillSummary( resourceId );

            return TransformResource( res );
        } //

        public static ResourceDTO TransformResource( Resource res )
        {
            ResourceDTO dto = new ResourceDTO();

            dto.Id = res.Id;
            dto.Title = res.Version.Title;
            dto.Description = res.Version.Description;
            dto.ResourceUrl = res.ResourceUrl;
            dto.OerUrl = ResBiz.FormatFriendlyResourceUrl( res.Version );
            dto.Publisher = res.Version.Publisher;
            dto.Creator = res.Version.Creator;

            if ( res.Gradelevel != null && res.Gradelevel.Count > 0 )
            {
                foreach ( ResourceChildItem item in res.Gradelevel )
                {
                    dto.GradeLevels.Add( item.MappedValue );
                }
            }

            if ( res.SubjectMap != null && res.SubjectMap.Count > 0 )
            {
                foreach ( ResourceChildItem item in res.SubjectMap )
                {
                    dto.Subjects.Add( item.MappedValue );
                }
            }

            StandardDTO standard;

            if ( res.Standard != null && res.Standard.Count > 0 )
            {
                foreach ( ResourceStandard item in res.Standard )
                {
                    standard = new StandardDTO();
                    standard.Id = item.StandardId;
                    standard.Description = item.StandardDescription;
                    standard.NotationCode = item.StandardNotationCode;
                    standard.AlignmentDegree = item.AlignmentDegree;
                    standard.AlignmentType = item.AlignmentTypeValue;
                    standard.StandardUrl = item.StandardUrl;

                    dto.Standards.Add( standard );
                }
            }
            return dto;

        }
    }
}
