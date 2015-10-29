using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//using System.Web.Script.Serialization;

namespace LRWarehouse.Business.ResourceV2
{
  #region Base Classes
  //Base class that applies to everything
  public class ResourceBase
  {
    public ResourceBase()
    {
			LrDocId = "";
			Title = "";
			UrlTitle = "";
			Description = "";
			Requirements = "";
			Url = "";
			ResourceCreated = "";
			Creator = "";
			Publisher = "";
			Submitter = "";
			ThumbnailUrl = "";
      Keywords = new List<string>();
      IsleSectionIds = new List<int>();
    }
    public int ResourceId { get; set; }
    public int VersionId { get; set; } //Hopefully we won't need this one day!
    public string LrDocId { get; set; }
    public string Title { get; set; }
    public string UrlTitle { get; set; } //aka SortTitle
    public string Description { get; set; }
    public string Requirements { get; set; }
    public string Url { get; set; }
    public string ResourceCreated { get; set; } //stringified date
    public string Creator { get; set; }
    public string Publisher { get; set; }
    public string Submitter { get; set; }
    public string ThumbnailUrl { get; set; }
    public List<string> Keywords { get; set; }
    public List<int> IsleSectionIds { get; set; }
  }

  //Standards Base
  public class StandardsBase
  {
    public int StandardId { get; set; }
    public string Description { get; set; }
    public string NotationCode { get; set; }
    public int AlignmentTypeId { get; set; }
    public string AlignmentType { get; set; }
    public int AlignmentDegreeId { get; set; }
    public string AlignmentDegree { get; set; }
	public bool IsDirectStandard { get; set; }
  }

  //Usage Rights
  public class UsageRights
  {
    public int CodeId { get; set; } //ID from the [ConditionOfUse] code table
    public string Title { get; set; }
    public string Description { get; set; }
    public string Url { get; set; }
    public string IconUrl { get; set; }
    public string MiniIconUrl { get; set; }
    public int TagId { get; set; } //ID from the [Resource.TagValue] code table
		public bool Custom { get; set; } //Is custom
		public bool Unknown { get; set; } //Is unknown
  }

  //Paradata
  public class ParadataBase
  {
    public int Favorites { get; set; }
    public int ResourceViews { get; set; }
    public int Likes { get; set; }
    public int Dislikes { get; set; }
  }

  //Base TagCategoryField thing
  public class FieldBase
  {
    public int Id { get; set; }
    public string Title { get; set; }
    public string Schema { get; set; }
  }

  //Base tag
  public class TagBase
  {
    public int Id { get; set; }
    public string Title { get; set; }
  }

  #endregion 

  #region Resource Database
  //Resource for use with database CRUD
  public class ResourceDB : ResourceBase
  {
    public ResourceDB()
    {
      Created = new DateTime();
      Updated = new DateTime();
      IsActive = true;
      Standards = new List<StandardsDB>();
      Paradata = new ParadataDB();
      Fields = new List<FieldDB>();
      UsageRights = new UsageRights();
    }
    //Single Value Fields
    public int CreatedById { get; set; }
    public DateTime Created { get; set; } //Database record
    public DateTime Updated { get; set; } 
    public bool IsActive { get; set; }

    //Special Fields
    public List<StandardsDB> Standards { get; set; }
    public ParadataDB Paradata { get; set; }
    public List<FieldDB> Fields { get; set; }
    public UsageRights UsageRights { get; set; }
  }

  //Standards for use with database CRUD
  public class StandardsDB : StandardsBase
  {
    public int RecordId { get; set; } //database record
    public int CreatedById { get; set; }
		public string StandardUrl { get; set; }
  }

  //Paradata for use with database CRUD
  public class ParadataDB : ParadataBase
  {
    public ParadataDB()
    {
      Comments = new List<ResourceComment>();
			RubricEvaluations = new List<EvaluationSummaryV2>();
			StandardEvaluations = new List<EvaluationSummaryV2>();
    }
    public List<ResourceComment> Comments { get; set; }
		public List<EvaluationSummaryV2> RubricEvaluations { get; set; }
		public List<EvaluationSummaryV2> StandardEvaluations { get; set; }
  }

  //Field (aka TagCategory) for use with database
  public class FieldDB : FieldBase
  {
    public FieldDB()
    {
      Tags = new List<TagDB>();
      IsleSectionIds = new List<int>();
    }
    public int SortOrder { get; set; }
    public List<int> IsleSectionIds { get; set; }
    public List<TagDB> Tags { get; set; }
  }

  //Tag for use with database
  public class TagDB : TagBase
  {
    public bool Selected { get; set; }
    public int CreatedById { get; set; }
    public int CategoryId { get; set; }
    public int OldCodeId { get; set; }
    public string Schema { get; set; }
  }

  #endregion

  #region Resource Data Transfer Object
  //Resource for use with client-side detail pages
  public class ResourceDTO : ResourceBase
  {
    public ResourceDTO()
    {
      Standards = new List<StandardsDTO>();
      Paradata = new ParadataDTO();
      Fields = new List<FieldDTO>();
      UsageRights = new UsageRights();
    }

    //Special Fields
	public int CreatedById { get; set; }
	public int PrivilegeId { get; set; } //public, only teachers in org, etc.
	public int OrganizationId { get; set; }
	public int LibraryId { get; set; }
	public int CollectionId { get; set; }
	public int ContentId { get; set; }
	public List<StandardsDTO> Standards { get; set; }
	public ParadataDTO Paradata { get; set; }
	public List<FieldDTO> Fields { get; set; }
	public UsageRights UsageRights { get; set; }
  }

  //Standards for use with client-side detail pages
  public class StandardsDTO : StandardsBase
  {
    public int BodyId { get; set; }
		public string Url { get; set; }
  }

  //Paradata for use with client-side detail pages
  public class ParadataDTO : ParadataBase
  {
    public ParadataDTO()
    {
      Comments = new List<CommentDTO>();
      Evaluations = new List<EvaluationDTO>();
      StandardEvaluations = new List<EvaluationDTO>();
    }
    public List<CommentDTO> Comments { get; set; }
    public List<EvaluationDTO> Evaluations { get; set; }
    public List<EvaluationDTO> StandardEvaluations { get; set; }
  }

  //Simple Comment
  public class CommentDTO
  {
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; }
    public string Date { get; set; }
    public string Text { get; set; }
  }

  //Simple Evaluation
  public class EvaluationDTO
  {
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ContextId { get; set; } //Standard ID or Rubric ID 
    public double Score { get; set; }
  }

  //Field for use with client-side detail pages
  public class FieldDTO : FieldBase
  {
    public FieldDTO()
    {
      Tags = new List<TagDTO>();
      IsleSectionIds = new List<int>();
    }
    public List<int> IsleSectionIds { get; set; }
    public List<TagDTO> Tags { get; set; }
  }

  //Tag for use with client-side detail pages
  public class TagDTO : TagBase
  {
    public bool Selected { get; set; }
  }

  #endregion

  #region Resource ElasticSearch
  //Resource for use with ElasticSearch
  public class ResourceES : ResourceBase
  {
    public ResourceES()
    {
			UsageRightsUrl = "";
      GradeAliases = new List<string>();
      LibraryIds = new List<int>();
      CollectionIds = new List<int>();
      StandardIds = new List<int>();
      StandardNotations = new List<string>();
      Paradata = new ParadataES();
      Fields = new List<FieldES>();
    }
    
    //Single Value Fields
		public string UsageRightsUrl { get; set; }

    //Special Fields
    public List<string> GradeAliases { get; set; }
    public List<int> LibraryIds { get; set; }
    public List<int> CollectionIds { get; set; }
    public List<int> StandardIds { get; set; }
    public List<string> StandardNotations { get; set; }
    public ParadataES Paradata { get; set; }
    public List<FieldES> Fields { get; set; }
  }

  //Paradata for use with Elastic Search
  public class ParadataES : ParadataBase
  {
    public int Comments { get; set; }
    public int Evaluations { get; set; }
    public double EvaluationsScore { get; set; }
    public double Rating { get; set; }
  }

  //Field for use with ElasticSearch. Contains tags in a different format
  public class FieldES : FieldBase
  {
    public FieldES()
    {
      Ids = new List<int>();
      Tags = new List<string>();
    }
    public List<int> Ids { get; set; }
    public List<string> Tags { get; set; }
  }

  #endregion

}
