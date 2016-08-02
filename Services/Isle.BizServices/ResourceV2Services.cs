using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

using ILPathways.Business;
using ILPathways.Utilities;
using LRWarehouse.Business;
using LRWarehouse.Business.ResourceV2;
using LRWarehouse.DAL;
using Microsoft.ApplicationBlocks.Data;
using EFMgr = IOERBusinessEntities;
using IsleDTO = Isle.DTO;
using ThisUser = LRWarehouse.Business.Patron;

namespace Isle.BizServices
{
	public class ResourceV2Services : ServiceHelper
	{
		#region Get Methods
		/// <summary>
		/// Get Resource DB
		/// Called by detail7 as well
		/// How does this differ from GetResourceDTO
		/// </summary>
		/// <param name="resourceID"></param>
		/// <returns></returns>
		public ResourceDB GetResourceDB( int resourceID )
		{
			var result = new ResourceDB();
			result.UsageRights = GetUsageRights("", 0);
			result.Fields = GetFieldAndTagCodeData(false);

			if (resourceID == 0)
			{
				return result;
			}

			//Get Resource Data
			var data = new ResourceManager().Get(resourceID);

			//Handle erroneous ID
			if (data.Id == 0)
			{
				return result;
			}

			//Get Version Data
			data.Version = ResourceBizService.ResourceVersion_GetByResourceId( resourceID );

			//Get Keywords
			var keywords = new ResourceKeywordManager().Select(resourceID).Select(m => m.OriginalValue).ToList();

			//Get Standards
			var standardsRaw = new ResourceStandardManager().Select(resourceID);
			var standards = new List<StandardsDB>();
			foreach (var item in standardsRaw)
			{
				standards.Add(new StandardsDB()
				{
					StandardId = item.StandardId,
					RecordId = item.Id,
					CreatedById = item.CreatedById,
					Description = item.StandardDescription,
					NotationCode = item.StandardNotationCode,
					AlignmentType = string.IsNullOrWhiteSpace(item.AlignmentTypeValue) ? "Aligns to" : item.AlignmentTypeValue,
					AlignmentTypeId = item.AlignmentTypeCodeId,
					UsageType = string.IsNullOrWhiteSpace(item.UsageType) ? "Normal" : item.UsageType,
					UsageTypeId = item.UsageTypeId,
					StandardUrl = item.StandardUrl
				});
			}

			//Get Usage Rights
			var usageRights = GetUsageRights( data.Version.Rights, data.Version.UsageRightsId);

			//Get ISLE Section IDs
			List<int> isleSectionIds = new List<int>();
			var isleSectionIdData = DatabaseManager.DoQuery("SELECT [SiteId] FROM [Resource.Site] WHERE ResourceIntId = " + resourceID);
			foreach (DataRow dr in isleSectionIdData.Tables[0].Rows)
			{
				isleSectionIds.Add(int.Parse(DatabaseManager.GetRowColumn(dr, "SiteId")));
			}
			if (!isleSectionIds.Contains(1)) { isleSectionIds.Add(1); } //All sites at least have IOER
			if (!isleSectionIds.Contains(5)) { isleSectionIds.Add(5); } //All sites also have all sites

			//Get Paradata
			var likeData = DatabaseManager.DoQuery( "SELECT [IsLike] FROM [Resource.Like] WHERE ResourceIntId = " + resourceID );
			var likesCount = 0;
			var dislikesCount = 0;
			foreach (DataRow dr in likeData.Tables[0].Rows)
			{
				var isLike = GetColumn<bool>("IsLike", dr);
				if (isLike)
				{
					likesCount++;
				}
				else
				{
					dislikesCount++;
				}
			}
			var favorites = SqlHelper.ExecuteDataset(DatabaseManager.ContentConnectionRO(), CommandType.Text, "SELECT Id FROM [Library.Resource] WHERE ResourceIntId = " + resourceID).Tables[0].Rows.Count;
			var resourceViews = DatabaseManager.DoQuery("SELECT Id FROM [Resource.View] WHERE ResourceIntId = " + resourceID).Tables[0].Rows.Count;
			//Evaluations
			var rubricEvaluations = GetRubricRatingsForResource( resourceID );

			//Standards
			var standardEvaluations = GetStandardsRatingsForResource( resourceID );

			var paradata = new ParadataDB()
			{
				Comments = new ResourceCommentManager().SelectList(resourceID),
				Likes = likesCount,
				Dislikes = dislikesCount,
				Favorites = favorites,
				RubricEvaluations = rubricEvaluations,
				StandardEvaluations = standardEvaluations,
				ResourceViews = resourceViews
			};

			//Get Fields
			var fields = GetFieldAndTagData(resourceID, false);

			//Get Thumbnail
			//TODO - 15-10-20 mp - need to update to handle custom images
			if (string.IsNullOrWhiteSpace(data.ImageUrl))
				data.ImageUrl = GetThumbnailUrl( resourceID );

			//Fill Resource Data
			var resource = new ResourceDB()
			{
				ResourceId = resourceID,
				VersionId = data.Version.Id,
				CreatedById = data.CreatedById,
				LrDocId = data.Version.LRDocId,
				Title = data.Version.Title,
				UrlTitle = data.Version.SortTitle,
				Description = data.Version.Description,
				Requirements = data.Version.Requirements,
				Url = data.ResourceUrl,
				ResourceCreated = data.Version.Created.ToShortDateString(),
				Creator = data.Version.Creator,
				Publisher = data.Version.Publisher,
				Submitter = data.Version.Submitter,
				ThumbnailUrl = data.ImageUrl,
				Keywords = keywords,
				IsleSectionIds = isleSectionIds,
				Created = data.Created,
				IsActive = true,
				Standards = standards,
				Updated = data.Version.LastUpdated,
				UsageRights = usageRights,
				Paradata = paradata,
				Fields = fields
			};

			return resource;
		} //

		/// <summary>
		/// Get all Rubric Dimension evaluations for a Resource
		/// </summary>
		/// <param name="resourceID"></param>
		/// <returns></returns>
		public List<EvaluationSummaryV2> GetRubricRatingsForResource( int resourceID )
		{
			var rubricEvaluations = new List<EvaluationSummaryV2>();
			var rubricData = DatabaseManager.DoQuery( "SELECT ResourceIntId, EvaluationId, EvalDimensionId, DimensionTitle, ([HasNOTCertificationTotal] + [HasCertificationTotal]) AS TotalEvaluations, CASE WHEN TotalEvaluations = 0 THEN 0 ELSE ((([HasCertificationTotal] * [HasCertificationTotalScore]) + ([HasNOTCertificationTotal] * [HasNOTCertificationTotalScore])) / ([HasNOTCertificationTotal] + [HasCertificationTotal])) END AS FinalScore FROM [Resource.EvalDimensionsSummary] WHERE ResourceIntId = " + resourceID );
			foreach ( DataRow dr in rubricData.Tables[ 0 ].Rows )
			{
				rubricEvaluations.Add( new EvaluationSummaryV2()
				{
					EvaluationId = GetColumn<int>( "EvaluationId", dr ),
					ContextId = GetColumn<int>( "EvalDimensionId", dr ),
					ResourceId = resourceID,
					Title = GetColumn<string>( "DimensionTitle", dr ),
					AverageScorePercent = ( double ) GetColumn<int>( "FinalScore", dr ),
					TotalEvaluations = GetColumn<int>( "TotalEvaluations", dr )
				} );
			}
			return rubricEvaluations;
		} //

		/// <summary>
		/// Get all Standards Ratings for a Resource
		/// </summary>
		/// <param name="resourceID"></param>
		/// <returns></returns>
		public List<EvaluationSummaryV2> GetStandardsRatingsForResource( int resourceID )
		{
			var standardEvaluations = new List<EvaluationSummaryV2>();
			var standardData = DatabaseManager.DoQuery( "SELECT ResourceIntId, StandardId, NotationCode, Description, AverageScorePercent, TotalEvals FROM [Resource.StandardEvaluationSummary] WHERE ResourceIntId = " + resourceID );
			foreach ( DataRow dr in standardData.Tables[ 0 ].Rows )
			{
				standardEvaluations.Add( new EvaluationSummaryV2()
				{
					EvaluationId = 0,
					ContextId = GetColumn<int>( "StandardId", dr ),
					Title = GetColumn<string>( "NotationCode", dr ),
					Description = GetColumn<string>( "Description", dr ),
					AverageScorePercent = ( double ) GetColumn<int>( "AverageScorePercent", dr ),
					TotalEvaluations = GetColumn<int>( "TotalEvals", dr )
				} );
			}
			return standardEvaluations;
		} //

		/// <summary>
		/// Get All Standards Ratings for a Resource that the user has issued
		/// Optionally, limit to a specific standard ID
		/// </summary>
		/// <param name="resourceID"></param>
		/// <param name="userID"></param>
		/// <returns></returns>
		public List<EvaluationSummaryV2> GetUserStandardRatingsForResource( int resourceID, int userID, int standardID )
		{
			var userStandardEvaluations = new List<EvaluationSummaryV2>();
			var userData = DatabaseManager.DoQuery( "SELECT DISTINCT list.[ResourceIntId], list.[StandardId], list.[CreatedById], list.[Score] AS UserScorePercent, summary.[TotalEvals], summary.[AverageScorePercent] FROM [Resource.StandardEvaluationSummary] summary LEFT JOIN [Resource.StandardEvaluationList] list ON list.[ResourceintId] = summary.[ResourceIntId] AND list.[StandardId] = summary.[StandardId] WHERE list.[ResourceIntId] = " + resourceID + " AND list.[CreatedById] = " + userID + ( standardID > 0 ? " AND list.[StandardId] = " + standardID : "" ) );
			foreach ( DataRow dr in userData.Tables[ 0 ].Rows )
			{
				userStandardEvaluations.Add( new EvaluationSummaryV2()
				{
					ResourceId = resourceID,
					ContextId = GetColumn<int>( "StandardId", dr ),
					CreatedById = GetColumn<int>( "CreatedbyId", dr ),
					UserScorePercent = ( double ) GetColumn<int>( "UserScorePercent", dr ),
					AverageScorePercent = ( double ) GetColumn<int>( "AverageScorePercent", dr ),
					TotalEvaluations = GetColumn<int>( "TotalEvals", dr )
				} );
			}

			return userStandardEvaluations;
		}

		/// <summary>
		/// Get Resource DTO
		/// Starts by using GetResourceDB
		/// </summary>
		/// <param name="resourceID"></param>
		/// <returns></returns>
		public ResourceDTO GetResourceDTO(int resourceID)
		{
			//Load basic data
			var data = GetResourceDB(resourceID);
			//Handles conversion because casting/as-ing doesn't convert base type to derived type (all properties are null, even shared ones), but this does
			var serializer = new JavaScriptSerializer();

			var resource = serializer.Deserialize<ResourceDTO>(serializer.Serialize(data));

			//Load Standards
			//Actually, these deserialize just fine in the above method call
			/*foreach (var item in data.Standards)
			{
				var standard = serializer.Deserialize<StandardsDTO>(serializer.Serialize(item));
				resource.Standards.Add(standard);
			}*/

			//Load Paradata
			//resource.Paradata = serializer.Deserialize<ParadataDTO>(serializer.Serialize(data.Paradata));
			resource.Paradata.Comments.Clear();
			foreach (var item in data.Paradata.Comments)
			{
				resource.Paradata.Comments.Add(new CommentDTO()
				{
					Id = item.Id,
					Date = item.Created.ToShortDateString(),
					Name = item.Commenter,
					UserId = item.CreatedById,
					Text = item.Comment
				});
			}
			resource.Paradata.Evaluations.Clear();
			foreach (var item in data.Paradata.RubricEvaluations)
			{
				resource.Paradata.Evaluations.Add(new LRWarehouse.Business.ResourceV2.EvaluationDTO()
				{
					Id = item.EvaluationId,
					ContextId = item.ContextId,
					Score = item.AverageScorePercent,
					UserId = item.CreatedById
				});
			}
			resource.Paradata.StandardEvaluations.Clear();
			foreach (var item in data.Paradata.StandardEvaluations)
			{
				resource.Paradata.StandardEvaluations.Add(new LRWarehouse.Business.ResourceV2.EvaluationDTO()
				{
					Id = item.EvaluationId,
					ContextId = item.ContextId,
					Score = item.AverageScorePercent,
					UserId = item.CreatedById
				});
			}

			//Load Fields
			//Not necessary
			//resource.Fields = serializer.Deserialize<List<FieldDTO>>(serializer.Serialize(data.Fields));

			//Load additional data from an associated content item if applicable
			var content = new ContentItem(); //Placeholder while Mike writes a method to get content item by resource id
			if ( content != null && content.Id > 0 )
			{
				resource.ContentId = content.Id;
				resource.PrivilegeId = content.PrivilegeTypeId;
			}

			return resource;
		}

		/// <summary>
		/// Populate resource for Elastic collection
		/// </summary>
		/// <param name="resourceID"></param>
		/// <returns></returns>
		public ResourceES GetResourceES(int resourceID)
		{
			var result = new ResourceES();
			ResourceManager mgr = new ResourceManager();
			try
			{
				//Convert old style tags to new table
				mgr.ResourceTag_ConvertById( resourceID );

				//DatabaseManager.ExecuteProc("[ResourceTag.ConvertById]", new SqlParameter[] { new SqlParameter("@resourceID", resourceID) });
			}
			catch (Exception ex)
			{
				ServiceHelper.LogError(ex, "Error converting old tags", true);
				throw;
			}

			DataSet resourceData;
			DataSet tagData;
			try
			{
				//Get data from tag tables
				resourceData = mgr.Resource_IndexV3TextsUpdate( resourceID );
				tagData = mgr.Resource_IndexV3TagsUpdate( resourceID );

			}
			catch (Exception ex)
			{
				ServiceHelper.LogError(ex, "Error getting data from tag tables");
				throw;
			}

			//Temporary(?) fix for error where there is no version record
			if ( ServiceHelper.DoesDataSetHaveRows( resourceData) == false)
			{
				throw new IndexOutOfRangeException("There was no Resource record for ID " + resourceID + ".");
			}
			//End temporary

			try
			{
				if (resourceData != null && tagData != null)
				{
					//Get version Data
					result = GetResourceESVersionData(resourceData.Tables[0].Rows[0]);
					result.Fields = GetFieldCodes();

					//Assemble tag data
					foreach (DataRow dr in tagData.Tables[0].Rows)
					{
						//Construct field data
						var fieldData = new FieldES()
						{
							Id = GetColumn<int>("CategoryId", dr),
							Ids = GetIntList("IDs", dr),
							Tags = GetStringList("Titles", dr)
						};

						//Get aliases if available
						var aliases = GetColumn<string>("AliasValues", dr).Split(',');
						if (aliases.Count() > 0)
						{
							result.GradeAliases = aliases.ToList();
						}

						//For the matching field, add the ID and Tag data
						var targetField = result.Fields.Where(f => f.Id == fieldData.Id).FirstOrDefault();
						if (targetField != null)
						{
							targetField.Ids = fieldData.Ids;
							targetField.Tags = fieldData.Tags;
						}
					}

					//Add usage rights
					//15-11-09 mparsons - no need for tags for usage rights - validate can be removed
					var usageRightsField = result.Fields.Where( m => m.Schema == "usageRights" ).FirstOrDefault();
					//var rights = GetUsageRights(result.UsageRightsUrl);
					//usageRightsField.Tags.Add(rights.Title);
					//usageRightsField.Ids.Add(rights.TagId);

				}
			}
			catch (Exception ex)
			{
				ServiceHelper.LogError(ex, "Error assembling tag data", true);
			}

			//Return resource
			return result;
		}

		/// <summary>
		/// Get Resource ES from Version Data
		/// </summary>
		/// <param name="versionData"></param>
		/// <returns></returns>
		public ResourceES GetResourceESVersionData(DataRow versionData)
		{
			//Get Resource ID
			var resourceID = GetColumn<int>("ResourceIntId", versionData);

			//Safely construct created date
			var created = DateTime.Now;
			try
			{
				created = GetColumn<DateTime>("ResourceCreated", versionData);
			}
			catch { }

			//Safely construct evaluation score
			var evaluations = GetColumn<int>("P_Evaluations", versionData);
			var scoreRaw = GetColumn<int>("P_EvaluationsScore", versionData);
			var score = GetPercentage(scoreRaw, evaluations);

			//Safely construct rating
			var likes = GetColumn<int>("P_Likes", versionData);
			var dislikes = GetColumn<int>("P_Dislikes", versionData);
			var rating = GetPercentage(likes - dislikes, likes + dislikes);

			//Safely determine favorites
			var favoritesRaw = GetColumn<int>("P_Favorites", versionData);
			var libIDs = GetIntList("LibraryIds", versionData);
			var favorites = libIDs.Count(); // favoritesRaw > libIDs.Count() ? favoritesRaw : libIDs.Count();

			
			//Get Thumbnail
			//15-10-20 mp - updated to handle custom images
			string imageUrl = GetColumn<string>( "CustomImageUrl", versionData );
			if ( string.IsNullOrWhiteSpace( imageUrl ) )
				imageUrl = GetThumbnailUrl( resourceID );

			//Build the rest of the resource
			//15-11-09 mp - added UsageRightsId. 
			//15-11-10 na - changed to RightsId in table
			var res = new ResourceES()
			{
				ResourceId = resourceID,
				VersionId = GetColumn<int>("ResourceVersionId", versionData),
				LrDocId = GetColumn<string>("DocId", versionData),
				Title = GetColumn<string>("Title", versionData),
				Description = GetColumn<string>("Description", versionData),
				Requirements = GetColumn<string>("Requirements", versionData),
				Url = GetColumn<string>("Url", versionData),
				ResourceCreated = created.ToShortDateString(),
				UrlTitle = GetColumn<string>("UrlTitle", versionData),
				ThumbnailUrl = imageUrl,
				Creator = GetColumn<string>("Creator", versionData),
				Publisher = GetColumn<string>("Publisher", versionData),
				Submitter = GetColumn<string>("Submitter", versionData),
				UsageRightsUrl = GetColumn<string>("RightsUrl", versionData),
				UsageRightsId = GetColumn<int>( "RightsId", versionData ),
				Keywords = GetStringList("Keywords", versionData).Where(t => t.Length >= 3).ToList(),
				LibraryIds = libIDs,
				CollectionIds = GetIntList("CollectionIds", versionData),
				StandardIds = GetIntList("StandardIds", versionData),
				StandardNotations = GetStringList("StandardNotations", versionData),
				Paradata = new ParadataES()
				{
					Favorites = favorites,
					ResourceViews = GetColumn<int>("P_ResourceViews", versionData),
					Likes = likes,
					Dislikes = dislikes,
					Rating = rating,
					Comments = GetColumn<int>("P_Comments", versionData),
					Evaluations = evaluations,
					EvaluationsScore = score
				}
			};

			return res;
		}
		private double GetPercentage(int score, int total)
		{
			if (total == 0)
			{
				return 0.0;
			}
			return (double)(score / total);
		}

		//Merge resource data, tag data, and tag codetable data
		public ResourceES GetResourceESViaDataMerge(DataRow versionData, List<GroupedTagData> tagData, List<UsageRights> usageRightsCodes, FieldES usageRights, List<FieldES> fieldCodes)
		{
			//Resource Data
			var res = GetResourceESVersionData(versionData);

			//Thumbnail Url
			//this should already have been handled in GetResourceESVersionData
			//res.ThumbnailUrl = GetThumbnailUrl(res.ResourceId);

			//Clone Fields to avoid pass-by-reference problems
			for (int i = 0; i < fieldCodes.Count(); i++)
			{
				if (fieldCodes[i].Schema == "usageRights") { continue; } //Prevent double-adding

				//IDs and Tags will be new or filled
				var ids = new List<int>();
				var tags = new List<string>();

				//For each item in tagData...
				for (int j = 0; j < tagData.Count(); j++)
				{
					//If the item has any alias values, set the resource's grade aliases to them
					if (tagData[j].AliasValues.Count() > 0)
					{
						res.GradeAliases = tagData[j].AliasValues;
					}
					//If the item's ID matches the current codetable item's ID...
					if (tagData[j].FieldData.Id == fieldCodes[i].Id)
					{
						//Fill out the ID and Tag lists
						ids = tagData[j].FieldData.Ids;
						tags = tagData[j].FieldData.Tags;
						break;
					}
				}

				//Add the field to the resource
				res.Fields.Add(new FieldES()
				{
					Id = fieldCodes[i].Id,
					Title = fieldCodes[i].Title,
					Schema = fieldCodes[i].Schema,
					Ids = ids,
					Tags = tags
				});
			}

			//Add usage rights
			//15-11-09 mparsons - should not need to have usage rights in tags, - validate and remove
			//		otherwise need to handle UsageRightsId

			var rightsField = GetUsageRights( res.UsageRightsUrl, res.UsageRightsId, usageRightsCodes );
			res.Fields.Add( new FieldES()
			{
				Id = usageRights.Id,
				Schema = usageRights.Schema,
				Title = usageRights.Title,
				Ids = new List<int>() { rightsField.TagId },
				Tags = new List<string>() { rightsField.Title }
			} );
			//TODO: update this to properly assign "unknown" and "custom"
			/*var rightsTag = usageRights.Tags[0];
			var rightsID = usageRights.Ids[0];
			if (res.UsageRightsUrl != "" && res.UsageRightsUrl != "Unknown")
			{
				for (var i = 0; i < usageRightsCodes.Count(); i++)
				{
					if (usageRightsCodes[i].Url == res.UsageRightsUrl)
					{
						rightsTag = usageRightsCodes[i].Title;
						rightsID = usageRightsCodes[i].TagId;
						break;
					}
				}
			}
			res.Fields.Add(new FieldES()
			{
				Id = usageRights.Id,
				Schema = usageRights.Schema,
				Title = usageRights.Title,
				Ids = new List<int>() { rightsID },
				Tags = new List<string>() { rightsTag }
			});*/

			return res;
		}

		//Get Resource ES from Version and Tag data
		public ResourceES GetResourceESFromVersionAndTagData(DataRow versionData, List<GroupedTagData> groupData, ResourceJSONManager json, List<FieldES> fieldCodes)
		{
			var res = GetResourceESVersionData(versionData);

			//Thumbnail Url
			//this should already have been handled in GetResourceESVersionData
			//res.ThumbnailUrl = GetThumbnailUrl(res.ResourceId);

			//Fields
			res.Fields = fieldCodes; //Should have been cloned before getting here

			//var targetData = tagData.Where( i => i.ResourceId == res.ResourceId ).ToList();

			foreach (var item in groupData)
			{
				var fieldSet = res.Fields.Where(f => f.Id == item.FieldData.Id).FirstOrDefault();
				if (fieldSet == null) { continue; }
				fieldSet.Ids = new List<int>(item.FieldData.Ids);
				fieldSet.Tags = new List<string>(item.FieldData.Tags);
				if (item.AliasValues.Count > 0)
				{
					res.GradeAliases = item.AliasValues;
				}
			}

			//Ensure site IDs are properly added
			var siteIDs = res.Fields.Where(f => f.Id == 27).FirstOrDefault();
			try
			{
				if (!siteIDs.Ids.Contains(275))
				{
					siteIDs.Ids.Add(275);
					siteIDs.Tags.Add("IOER");
				}
			}
			catch { }

			if (siteIDs != null)
			{
				res.IsleSectionIds = siteIDs.Ids;
			}

			return res;
		}

/*		public string GetJSONLRMIPayloadFromResource( ResourceDTO input )
		{
			var log = new List<string>();
			return GetJSONLRMIPayloadFromResource( input, ref log );
		}

		/// <summary>
		/// Get LR Payload from Resource DTO
		/// </summary>
		/// <param name="input"></param>
		/// <param name="logging"></param>
		/// <returns></returns>
		public string GetJSONLRMIPayloadFromResource(ResourceDTO input, ref List<string> logging)
		{
			logging.Add( "input status: " + (input == null ? "Null!" : "not null") );
			logging.Add( "Adding basic fields..." );

			//Hold output
			var resource = new Dictionary<string, object>();

			//Simple fields
			//TODO - do we need to add thumbnail?
			//No - no need to publish thumbnail data to LR - NA
			resource.Add("name", input.Title);
			resource.Add("url", input.Url);
			resource.Add("description", input.Description);
			resource.Add("author", input.Creator);
			resource.Add("publisher", input.Publisher);
			resource.Add("dateCreated", input.ResourceCreated);
			resource.Add("requires", input.Requirements);
			resource.Add("useRightsUrl", input.UsageRights.Url);
			resource.Add("about", input.Keywords);

			logging.Add( "Adding dynamic fields..." );
			logging.Add( "Fields status: " + (input.Fields == null ? "Null!" : "not null") );
			logging.Add( "Field count: " + input.Fields.Count() );

			//Dynamic fields
			foreach (var item in input.Fields.Where(m => m.Tags.Where(t => t.Selected).Count() > 0).ToList())
			{
				if (item.Schema != "gradeLevel") //Grade Level is handled separately
				{
					logging.Add( "Handling field: " + item.Schema );
					resource.Add(item.Schema, item.Tags.Where(t => t.Selected).Select(t => t.Title).ToList());
				}
			}

			logging.Add( "Handling special fields..." );

			//Special fields
			//If any grades are selected, go through the cantankerous process of figuring out age range and assign the grade level alignment object
			var grades = input.Fields.Where(m => m.Schema == "gradeLevel").FirstOrDefault().Tags.Where(t => t.Selected).OrderBy(t => t.Id).ToList();
			if (grades.Count > 0)
			{
				logging.Add( "Determining age ranges..." );
				var ageRangeDS = CodeTableBizService.DeterminingAgeRanges( input );
				//var ageRangeDS = DatabaseManager.DoQuery(
				//	"SELECT codes.[Id], codes.[FromAge], codes.[ToAge], tags.[Id] AS TagId, tags.Title " +
				//	"FROM [Codes.GradeLevel] codes " +
				//	"LEFT JOIN [Codes.TagValue] tags ON tags.CodeId = codes.Id " +
				//	"WHERE tags.CategoryId = " + input.Fields.Where(m => m.Schema == "gradeLevel").FirstOrDefault().Id + " AND tags.IsActive = 1"
				//	);
				var fromAge = "";
				var toAge = "";
				foreach (DataRow dr in ageRangeDS.Tables[0].Rows)
				{
					var currentID = int.Parse(DatabaseManager.GetRowColumn(dr, "TagId"));
					if (currentID == grades.First().Id)
					{
						fromAge = DatabaseManager.GetRowColumn(dr, "FromAge");
					}
					if (currentID == grades.Last().Id)
					{
						toAge = DatabaseManager.GetRowColumn(dr, "ToAge");
					}
				}
				resource.Add("typicalAgeRange", fromAge + "-" + toAge);
				var alignmentObject = new AlignmentObject()
				{
					targetName = "gradeLevel",
					educationalFramework = "US P-20",
					targetDescription = grades.Select(m => m.Title).ToList(),
					alignmentType = "gradeLevel"
				};
				resource.Add("gradeLevel", alignmentObject);
			}

			logging.Add( "Handling standards..." );

			//Standards
			//Get standard bodies
			var standardBodies = GetStandardBodiesList();
			//Assign standard bodies to standards
			foreach (var item in input.Standards)
			{
				logging.Add( "Handling standard: " + item.NotationCode );
				//Remember to add standards to standard bodies at the bottom of this file when they're added to IOER!
				item.BodyId = standardBodies.Where(m => m.IdRanges.Where(n => n.Key <= item.StandardId && n.Value >= item.StandardId).Count() > 0).FirstOrDefault().BodyId;
				if (item.NotationCode == null || item.NotationCode == "")
				{
					item.NotationCode = item.Description;
				}
			}
			//Create alignmentObjects
			var alignments = new List<AlignmentObject>();
			var alignmentTypes = new string[] { "Learning Standard", "Assesses", "Teaches", "Requires" };
			foreach (var item in input.Standards)
			{
				var body = standardBodies.Where(m => m.BodyId == item.BodyId).FirstOrDefault();
				if (body == null) { continue; }

				if (item.NotationCode == item.Description)
				{
					item.NotationCode = "";
				}

				alignments.Add(new AlignmentObject()
				{
					educationalFramework = body.Url,
					targetDescription = item.Description,
					alignmentType = alignmentTypes[item.AlignmentTypeId],
					targetName = item.NotationCode,
					targetUrl = item.Url
				});
			}

			//Add alignments
			if (alignments.Count() > 0)
			{
				resource.Add("learningStandards", alignments);
			}

			logging.Add( "Serializing..." );

			return new JavaScriptSerializer().Serialize(resource);
		}*/

        public string GetJsonLdLrmiForPage(ResourceDTO input)
        {
            var log = new List<string>();
            return GetJsonLdLrmiPayloadFromResource(input, true, ref log);
        }

        public string GetJsonLdLrmiPayloadFromResource(ResourceDTO input)
        {
            var log = new List<string>();
            return GetJsonLdLrmiPayloadFromResource(input, false, ref log);
        }

        public string GetJsonLdLrmiPayloadFromResource(ResourceDTO input, bool forPage, ref List<string> logging)
        {
            logging.Add("input status: " + (input == null ? "Null!" : "not null"));
            logging.Add("Building context and type");

            // Hold output
            var resource = new Dictionary<string, object>();

            // Build context - this is the same for all resources
            var context = new Dictionary<string, object>();
            context.Add("@vocab", "http://schema.org/");
            context.Add("requires", "http://purl.org/dc/terms/requires");
            context.Add("mediaType", "http://purl.org/dc/terms/format");
            context.Add("accessRights", "http://purl.org/dc/terms/AccessRights");
            context.Add("useRightsUrl", "http://purl.org/dcx/lrmi-terms/useRightsUrl");
            var aud = new Dictionary<string, object>();
            aud.Add("@id", "audience");
            aud.Add("@type", "EducationalAudience");
            context.Add("audience", aud);
            resource.Add("@context", context);
            resource.Add("@type", "CreativeWork");

            logging.Add("Adding basic fields...");
            resource.Add("name", input.Title);
            if (!forPage)
            {
                resource.Add("url", input.Url);
            }
            resource.Add("description", input.Description);
            resource.Add("dateCreated", DateTime.Parse(input.ResourceCreated).ToString("yyyy-MM-dd"));
            resource.Add("requires", input.Requirements);
            resource.Add("useRightsUrl", input.UsageRights.Url);
            // added license, useRightsUrl is an LRMI 1.1 property, license is a schema.org property.
            resource.Add("license", input.UsageRights.Url);
            resource.Add("about", input.Keywords);

            // author and publisher in the strictest sense are classes (see http://schema.org/author and http://schema.org/publisher)
            var obj = new Dictionary<string, object>();
            obj.Add("name", input.Creator);
            resource.Add("author", obj);
            obj = new Dictionary<string,object>();
            obj.Add("name",input.Publisher);
            resource.Add("publisher", obj);

            // Dynamic fields
            logging.Add("Adding dynamic fields...");
            logging.Add("  Fields status: " + (input.Fields == null ? "Null!" : "not null"));
            logging.Add("  Field count: " + input.Fields.Count());

			var specialFieldSchemas = new List<string>() { 
					"gradeLevel", 
					"k12Subject", 
					"careerCluster", 
					"educationalRole", 
					"groupType", 
					"learningResourceType",
					"assessmentType"
			};

            foreach (var item in input.Fields.Where(m => m.Tags.Where(t => t.Selected).Count() > 0).ToList())
            {
                if (!specialFieldSchemas.Contains(item.Schema))
                {
                    // gradeLevel, k12Subject, careerCluster, educationalRole, groupType, learningResourceType, and assessmentType are handled separately.
                    logging.Add("  Handling field: " + item.Schema);
                    resource.Add(item.Schema, item.Tags.Where(t => t.Selected).Select(t => t.Title).ToList());
                }
            }

            // Special fields - gradeLevel, k12subject, careerCluster, educationalRole, groupType, learningResourceType, and assessmentType
            logging.Add("  Handling special fields...");

            List<AlignmentObject> alignments = new List<AlignmentObject>();
            // If any grades are selected, go through the cantankerous process of figuring out age range and assign the grade level alignment object
            var grades = input.Fields.Where(m => m.Schema == "gradeLevel").FirstOrDefault().Tags.Where(t => t.Selected).OrderBy(t => t.Id).ToList();
            if (grades.Count > 0)
            {
                logging.Add("  Determining age ranges...");
                var ageRangeDS = DatabaseManager.DoQuery(
                    "SELECT codes.[Id], codes.[FromAge], codes.[ToAge], tags.[Id] AS TagId, tags.Title\n" +
                    "FROM [Codes.GradeLevel] codes\n" +
                    "LEFT JOIN [Codes.TagValue] tags on tags.CodeId = codes.Id\n" +
                    "WHERE tags.CategoryId = " + input.Fields.Where(m => m.Schema == "gradeLevel")
                        .FirstOrDefault().Id + " AND tags.IsActive = 1"
                );
                var fromAge = "";
                var toAge = "";
                foreach (DataRow dr in ageRangeDS.Tables[0].Rows)
                {
                    var currentId = int.Parse(DatabaseManager.GetRowColumn(dr, "TagId"));
                    if (currentId == grades.First().Id)
                    {
                        fromAge = DatabaseManager.GetRowColumn(dr, "FromAge");
                    }
                    if (currentId == grades.Last().Id)
                    {
                        toAge = DatabaseManager.GetRowColumn(dr, "ToAge");
                    }
                }
                resource.Add("typicalAgeRange", fromAge + "-" + toAge);
                logging.Add("  Handling grade levels...");
                foreach (TagDTO grade in grades)
                {
                    AlignmentObject alignmentObject = new AlignmentObject()
                    {
                        targetName = grade.Title,
                        educationalFramework = grade.Title.IndexOf("NRS") == -1 ? "US P-20" : "NRS Adult Education",
                        targetDescription = grade.Title,
                        alignmentType = "educationLevel"
                    };
                    alignments.Add(alignmentObject);
                }
            }

            // Handling Standards
            logging.Add("  Handling Standards...");
            var standardBodies = GetStandardBodiesList();
            //Assign standard bodies to standards
            foreach (var item in input.Standards)
            {
                logging.Add("    Handling standard: " + item.NotationCode);
                item.BodyId = standardBodies.Where(m => m.IdRanges.Where(n => n.Key <= item.StandardId && n.Value >= item.StandardId).Count() > 0).FirstOrDefault().BodyId;
                if (item.NotationCode == null || item.NotationCode == "")
                {
                    item.NotationCode = item.Description;
                }
            }
            //Create alignment objects
            var alignmentTypes = new string[] { "learning standard", "assesses", "teaches", "requires" };
            foreach (var item in input.Standards)
            {
                var body = standardBodies.Where(m => m.BodyId == item.BodyId).FirstOrDefault();
                if (body == null) { continue; }
                if (item.NotationCode == item.Description)
                {
                    item.NotationCode = "";
                }
                alignments.Add(new AlignmentObject()
                {
                    educationalFramework = body.Url,
                    targetDescription = item.Description,
                    alignmentType = alignmentTypes[item.AlignmentTypeId],
                    targetName = item.NotationCode,
                    targetUrl = item.Url
                });
            }

            // Handling K12 subjects
            logging.Add("  Handling K12 Subjects...");
            var subjects = input.Fields.Where(m => m.Schema == "k12Subject").FirstOrDefault().Tags.Where(t => t.Selected).OrderBy(t => t.Id).ToList();
            foreach (var item in subjects)
            {
                alignments.Add(new AlignmentObject()
                {
                    educationalFramework = "US K-12 Academic Subjects",
                    targetDescription = item.Title,
                    alignmentType = "educationalSubject",
                    targetName = item.Title
                });
            }

			if ( GetAppKeyValue( "handlingClustersSeparately", "no" ) == "yes" )
			{
				// Handling Career Clusters
				var clusters = input.Fields.Where( m => m.Schema == "careerCluster" ).FirstOrDefault().Tags.Where( t => t.Selected ).OrderBy( t => t.Id ).ToList();
				if ( clusters.Count > 0 )
				{
					logging.Add( "  Handling Career Clusters..." );
					foreach ( var item in clusters )
					{
						alignments.Add( new AlignmentObject()
						{
							educationalFramework = item.Title == "Energy" ? "US-IL Career Clusters" : "US Career Clusters",
							targetDescription = item.Title,
							targetName = item.Title,
							alignmentType = "educationalSubject"
						} );
					}
				}
			}

            // Now that we've handled everything that should be handled as an AlignmentObject, add the alignments, if any.
            if (alignments.Count() > 0)
            {
                resource.Add("educationalAlignment", alignments);
            }

            // Now handle anything that uses an Audience object (educationalRole and groupType)
            var audiences = new List<Dictionary<string, object>>();
            var educationalRoles = input.Fields.Where(m => m.Schema == "educationalRole").FirstOrDefault().Tags.Where(t => t.Selected).OrderBy(t => t.Id).ToList();
            if (educationalRoles.Count > 0)
            {
                logging.Add("  Handling Educational Role/End User...");
                List<string> edRole = new List<string>();
                Dictionary<string, object> audience = new Dictionary<string, object>();
                foreach (var item in educationalRoles)
                {
                    edRole.Add(item.Title);
                }
                audience.Add("educationalRole", edRole);
                audiences.Add(audience);
            }
            var groupTypes = input.Fields.Where(m => m.Schema == "groupType").FirstOrDefault().Tags.Where(t => t.Selected).OrderBy(t => t.Id).ToList();
            if (groupTypes.Count > 0)
            {
                logging.Add("  Handling Audience Type/Group Type...");
                List<string> audType = new List<string>();
                Dictionary<string, object> audience = new Dictionary<string, object>();
                foreach (var item in groupTypes)
                {
                    audType.Add(item.Title);
                }
                audience.Add("audienceType", audType);
                audiences.Add(audience);
            }

            // now that we've handled everything that should be handled as an EducationalAudience, add the audiences, if any.
            if (audiences.Count() > 0)
            {
                resource.Add("audience", audiences);
            }

            var resourceTypes = new List<string>();
            // Now handle learningResourceType and assessmentType
            var learningResourceTypes = input.Fields.Where(m => m.Schema == "learningResourceType").FirstOrDefault().Tags.Where(t => t.Selected).OrderBy(t => t.Id).ToList();
            if (learningResourceTypes.Count > 0)
            {
                logging.Add("  Handling Learning Resource Types");
                foreach (var item in learningResourceTypes)
                {
                    resourceTypes.Add(item.Title);
                }
            }
            var assessmentTypes = input.Fields.Where(m => m.Schema == "assessmentType").FirstOrDefault().Tags.Where(t => t.Selected).OrderBy(t => t.Id).ToList();
            if (assessmentTypes.Count > 0)
            {
                logging.Add("  Handling Assessment Types");
                foreach (var item in assessmentTypes)
                {
                    resourceTypes.Add(item.Title);
                }
            }
            if (resourceTypes.Count() > 0)
            {
                resource.Add("learningResourceType", resourceTypes);
            }

            logging.Add("Serializing...");
            return new JavaScriptSerializer().Serialize(resource);
        }//

		public ResourceDTO GetResourceDTOFromContent( int contentID, Patron user )
		{
			var valid = true;
			var status = "";
			var approval = "";
			var action = "";
			return GetResourceDTOFromContent( contentID, user, ref valid, ref status, ref approval, ref action );
		}
		/// <summary>
		/// Get ResourceDTO from ContentItem
		/// Usage - to populate a resource object used on uberTagger
		/// </summary>
		/// <param name="contentID"></param>
		/// <param name="user"></param>
		/// <param name="valid"></param>
		/// <param name="status"></param>
		/// <param name="requiresApproval"></param>
		/// <param name="lrPublishAction"></param>
		/// <returns></returns>
		public ResourceDTO GetResourceDTOFromContent( int contentID, Patron user, ref bool valid, ref string status, ref string requiresApproval, ref string lrPublishAction )
		{
			var result = new ResourceDTO()
			{
				ContentId = contentID
			};

			//Get the content item
			//Hmm - check, may should not use a basic get - Uses DAL, will not return standards
			var contentItem = new ContentServices().Get( contentID, true );
			if ( contentItem == null || contentItem.Id == 0 )
			{
				status = "That ID does not match valid content record";
				valid = false;
				return result;
			}

			//For now, don't handle updates
			if ( contentItem.ResourceIntId > 0 )
			{
				status = "The referenced content record has already been published.";
				valid = false;
				result.ResourceId = contentItem.ResourceIntId;
				return result;
			}

			//Check authorization
			ContentPartner cp = ContentServices.ContentPartner_Get( result.ContentId, user.Id );
			if ( cp == null && cp.PartnerTypeId < ContentPartner.PARTNER_TYPE_ID_CONTRIBUTOR )
			{
				status = "You do not have authorization to publish the requested contentItem";
				valid = false;
				return result;
			}

			result.Url = GetContentResourceUrl( contentItem );
			//TODO - not shown on  tagger so picked up later
			result.ThumbnailUrl = contentItem.ImageUrl;

			result.Title = contentItem.Title;
			result.Description = contentItem.Summary;
			//if id not equal: 0,4,8, just return Id - tagger needs to handle
			//if 4, need to also include url
			result.UsageRights = GetUsageRights( contentItem.ConditionsOfUseUrl, contentItem.ConditionsOfUseId );
			if ( contentItem.ConditionsOfUseId == 4 )
			{
				result.UsageRights.Url = contentItem.ConditionsOfUseUrl;
			}
			result.Creator = contentItem.HasOrg() ? contentItem.ContentOrg.Name : user.FullName();
			result.Publisher = ILPathways.Utilities.UtilityManager.GetAppKeyValue( "defaultPublisher", "Illinois Shared Learning Environment" );

			if ( contentItem.PrivilegeTypeId == ContentItem.PUBLIC_PRIVILEGE )
			{
				result.PrivilegeId = contentItem.PrivilegeTypeId;
			}
			else
			{
				lrPublishAction = "no";
			}

			//this is to be configuragle, not hard coded!
			if ( contentItem.IsOrgContent() )
			{
				result.OrganizationId = contentItem.OrgId;
				if ( UtilityManager.GetAppKeyValue( "doingOrgContentApprovals", "no" ) == "yes" )
				{
					//doingOrgContentApprovals
					requiresApproval = "yes";
					if ( contentItem.PrivilegeTypeId == ContentItem.PUBLIC_PRIVILEGE )
					{
						lrPublishAction = "save";
					}
				}
			}

			//NOTE: for learning lists, the standards will be applied later, so don't populate, and produce message!
			if ( contentItem.IsHierarchyType || contentItem.TypeId == ContentItem.LEARNING_SET_CONTENT_ID) 
			{
				ServiceHelper.SetConsoleInfoMessage( "NOTE: All standards associated with this item will be copied to the resource after publishing has completed. You should not manually add any standards that aleady are associated with the list." );
			} else if (  contentItem.ContentStandards.Count > 0 )
			{
				foreach ( Content_StandardSummary css in contentItem.ContentStandards )
				{
					var standard = new StandardsDTO();
					//issue the StandardsDTO doesn't have aligned by
					//often the same. However, I suppose it is ok for publishing
					standard.StandardId = css.StandardId;
					standard.AlignmentTypeId = css.AlignmentTypeCodeId;
					//major/supporting/additional - recycles old object's at/above/below field
					standard.UsageTypeId = css.UsageTypeId;
					standard.IsDirectStandard = css.IsDirectStandard;

					result.Standards.Add( standard );
				}
			}

			valid = true;
			status = "okay";
			return result;
		}
		public string GetContentResourceUrl( ContentItem contentItem )
		{
			string url = "";
			string contentUrl = ServiceHelper.GetAppKeyValue( "contentUrl" );
			if ( contentItem.TypeId == 50 )
			{
				url = ServiceHelper.GetAppKeyValue( "learningListUrl" );
				if ( url.Length > 10 )
					url = string.Format( url, contentItem.Id, ResourceBizService.FormatFriendlyTitle( contentItem.Title ) );
			}
			else if ( contentItem.TypeId == ContentItem.LEARNING_SET_CONTENT_ID )
			{
				//really need to check the parent, and if an LL, then know the url pattern to use
				url = ServiceHelper.GetAppKeyValue( "learningSetUrl" );
				if ( url.Length > 10 )
					url = string.Format( url, contentItem.Id, ResourceBizService.FormatFriendlyTitle( contentItem.Title ) );
			}
			else if ( contentItem.TypeId > 50 && contentItem.TypeId < 58 )
			{
				//really need to check the parent, and if an LL, then know the url pattern to use
				url = ServiceHelper.GetAppKeyValue( "learningListUrl" );
				if ( url.Length > 10 )
					url = string.Format( url, contentItem.Id, ResourceBizService.FormatFriendlyTitle( contentItem.Title ) );
			}
			else if ( contentItem.TypeId == 10 )
			{
				if ( contentUrl.Length > 10 )
					url = string.Format( contentUrl, contentItem.Id, ResourceBizService.FormatFriendlyTitle( contentItem.Title ) );
			}
			else if ( contentItem.DocumentUrl != null && contentItem.DocumentUrl.Length > 10 )
			{
				//Should probably direct to the content page, not the document URL, so that the published URL will always go through security checks in the event that the owner changes permissions later  -NA
				url = ILPathways.Utilities.UtilityManager.FormatAbsoluteUrl( contentItem.DocumentUrl, false );
			}
			else
			{
				//not sure, so default to content
				if ( contentUrl.Length > 10 )
					url = string.Format( contentUrl, contentItem.Id, ResourceBizService.FormatFriendlyTitle( contentItem.Title ) );
			}

			return url;
		}

		//Used by import
		public void ImportRefreshResources(List<int> resourceIDs)
		{
			ImportRefreshResources(resourceIDs, true);
		}
		/// <summary>
		/// import resources from a csv list
		/// </summary>
		/// <param name="resourceIdList"></param>
		public void ImportRefreshResources( string resourceIdList )
		{
			List<string> strResources = resourceIdList.Split( ',' ).ToList();
			List<int> resources = new List<int>();
			foreach ( string rid in strResources )
			{
				if ( rid != "" )
				{
					resources.Add( int.Parse( rid ) );
				}
			}
			ImportRefreshResources( resources, true );

		}
		public void ImportRefreshResources(List<int> resourceIDs, bool sendEmailErrors)
		{
			var errors = new List<string>();
			try
			{
				var resources = new List<ResourceES>();
				foreach (var item in resourceIDs)
				{
					try
					{
						var resource = GetResourceES( item );
						if ( resource != null && resource.ResourceId > 0 )
							resources.Add( resource );
					}
					catch (Exception ex)
					{
						errors.Add("Error updating resource " + item + ": " + ex.Message + (errors.Count() == 0 ? "<br />" + ex.ToString() : "")); //include stack trace with first error
					}
				}

				//Do the bulk upload
				var bulk = new List<object>();
				foreach ( var item in resources )
				{
					LoadBulkItem( ref bulk, item );
				}
				new ElasticSearchManager().DoChunkedBulkUpload( bulk );

				//Log information as needed
				if (ServiceHelper.GetAppKeyValue("currentlyRebuildingIndex", "no") == "yes")
				{
					var messageTitle = "Resources need to be reindexed after import";
					var messageBody = "Resources " + string.Join(", ", resourceIDs) + " were created/updated during index rebuild on " + DateTime.Now.ToShortDateString() + " and their index entries need to be refreshed.";
					if (sendEmailErrors)
					{
						ServiceHelper.NotifyAdmin(messageTitle, messageBody);
					}
					else
					{
						throw new DataException(messageTitle + "<br />" + System.Environment.NewLine + messageBody);
					}
				}

				if (errors.Count() > 0)
				{
					var messageTitle = "One or more resources failed to update during import";
					var messageBody = "There were one or more errors during the GetResourceES() method in the ImportRefreshResources() method in ResourceV2Services: " + (sendEmailErrors ? System.Environment.NewLine : "<br />") + string.Join((sendEmailErrors ? System.Environment.NewLine : "<br />"), errors);
					if (sendEmailErrors)
					{
						ServiceHelper.NotifyAdmin(messageTitle, messageBody);
					}
					else
					{
						throw new DataException(messageTitle + "<br />" + System.Environment.NewLine + messageBody);
					}
				}
			}
			catch (DataException dex)
			{
				throw dex;
			}
			catch (Exception ex)
			{
				var messageTitle = "Entire index update failed during import";
				var messageBody = "The bulk index upload process encountered an error while attempting to update the index. Resources " + string.Join(", ", resourceIDs) + " were created/updated on " + DateTime.Now.ToShortDateString() + " and their index entries need to be refreshed. The error was: " + (sendEmailErrors ? System.Environment.NewLine : "<br />") + ex.Message + (sendEmailErrors ? System.Environment.NewLine : "<br />") + ex.ToString();

				if (sendEmailErrors)
				{
					ServiceHelper.NotifyAdmin(messageTitle, messageBody);
				}
				else
				{
					throw new Exception(messageTitle + "<br />" + System.Environment.NewLine + messageBody);
				}
			}
		}

		//Update or create a resource
		public void RefreshResource(int resourceID)
		{
			ServiceHelper.DoTrace(5, "index update check 1");

			if (ServiceHelper.GetAppKeyValue("currentlyRebuildingIndex", "no") == "yes")
			{
				System.IO.File.AppendAllText(@"C:\IOER\@logs\rebuildlog_" + DateTime.Today.ToString("yyMMdd") + ".txt", resourceID + ", ");
				//ServiceHelper.NotifyAdmin( "Resource needs to be reindexed", "Resource " + resourceID + " was updated during index rebuild on " + DateTime.Now.ToShortDateString() + " and its index entry needs to be refreshed: http://ioer.ilsharedlearning.org/Resource/" + resourceID );
			}

			try
			{
				var manager = new ElasticSearchManager();
				//manager.RefreshResourceOld( resourceID );

				var resource = GetResourceES(resourceID);

				var bulk = new List<object>();
				LoadBulkItem(ref bulk, resource);

				var response = manager.DoBulkUpload(bulk);

			}
			catch (IndexOutOfRangeException iex)
			{ //resource was deactivated
				return;
			}
			catch (Exception ex)
			{
				ServiceHelper.LogError(ex, "Error updating elasticsearch for resource " + resourceID + ":");
			}

		}

		//Get fields for a library/collection as DTO
		public List<FieldDTO> GetFieldsDTOForLibCol(List<int> libraryIDs, List<int> collectionIDs, int isleSectionID)
		{
			var output = new List<FieldDTO>();
			var fields = GetFieldsForLibCol(libraryIDs, collectionIDs, isleSectionID);
			foreach (var item in fields)
			{
				var newField = new FieldDTO()
				{
					Id = item.Id,
					Title = item.Title,
					Schema = item.Schema,
					IsleSectionIds = item.IsleSectionIds
				};
				foreach (var tag in item.Tags)
				{
					newField.Tags.Add(new TagDTO()
					{
						Id = tag.Id,
						Title = tag.Title,
						Selected = tag.Selected
					});
				}
				output.Add(newField);
			}
			return output;
		}

		//Get fields for a library/collection
		//There should probably be a more efficient way of doing this that lives closer to the database
		public List<FieldDB> GetFieldsForLibCol(List<int> libraryIDs, List<int> collectionIDs, int isleSectionID)
		{
			var output = new List<FieldDB>();
			var codes = GetFieldAndTagCodeData(false);

			//Get based on collections
			foreach (var item in collectionIDs)
			{
				var collectionFilters = LibraryBizService.Collection_GetPresentFiltersOnly(isleSectionID, item);
				AddFieldsAndTags(0, item, collectionFilters.SiteTagCategories, codes, ref output);
			}
			//Get based on libraries if no collections are present
			if ( collectionIDs == null || collectionIDs.Count() == 0 )
			{
				foreach (var item in libraryIDs)
				{
					var libraryFilters = LibraryBizService.Library_GetPresentFiltersOnly(isleSectionID, item);
					AddFieldsAndTags(item, 0, libraryFilters.SiteTagCategories, codes, ref output);
				}
			}
			//If no library or collection IDs, just get all fields
			if (libraryIDs.Count() == 0 && collectionIDs.Count() == 0)
			{
				output = codes;
			}

			return output;
		}
		private void AddFieldsAndTags(int libraryID, int collectionID, List<Isle.DTO.SiteTagCategory> data, List<FieldDB> codes, ref List<FieldDB> output)
		{
			//For each field in the data for this filter...
			foreach (var filter in data)
			{
				//Find the matching field already in output
				var target = output.Where(f => f.Id == filter.CategoryId).FirstOrDefault(); //Id or CategoryId ?
				//If the output already contains this field...
				if (target != null)
				{
					//...then try to get the right tags for this filter.
					var tagsForThisFilter = GetTagsForThisFilter(filter.TagValues, libraryID, collectionID, filter.CategoryId);

					//For each surviving tag...
					foreach (var tag in tagsForThisFilter)
					{
						//...check to see if the tag is already there.
						var targetTag = target.Tags.Where(t => t.Id == tag.id).FirstOrDefault();
						//If the tag isn't there...
						if (targetTag == null)
						{
							//Try to add it.
							try
							{
								target.Tags.Add(codes.Single(m => m.Id == target.Id).Tags.Single(n => n.Id == tag.id));
							}
							catch { }
						}
					}
				}
				//If the output doesn't contain this field yet...
				else
				{
					//Find the matching field in the code table
					var targetCode = codes.Where(i => i.Id == filter.CategoryId).FirstOrDefault(); //Id or CategoryId ?
					//If it exists...
					if (targetCode != null)
					{
						//Add a copy of it (don't want to copy the raw tags) to the output
						output.Add(new FieldDB() { Id = targetCode.Id, Title = targetCode.Title, Schema = targetCode.Schema, IsleSectionIds = targetCode.IsleSectionIds, SortOrder = targetCode.SortOrder });
						//Then add only the desired tags
						var targetfield = output.Single(f => f.Id == targetCode.Id);

						var tagsForThisFilter = GetTagsForThisFilter(filter.TagValues, libraryID, collectionID, filter.CategoryId);
						foreach (var tag in tagsForThisFilter)
						{
							try
							{
								targetfield.Tags.Add(targetCode.Tags.Single(s => s.Id == tag.id));
							}
							catch { }
						}
					}
				}
			}
		}
		private List<Isle.DTO.Filters.TagFilterBase> GetTagsForThisFilter(List<Isle.DTO.Filters.TagFilterBase> tagValues, int libraryID, int collectionID, int filterID)
		{
			var tagsForThisFilterData = new List<Isle.DTO.Filters.TagFilterBase>();
			if (collectionID > 0)
			{
				tagsForThisFilterData = LibraryBizService.AvailableFiltersForCollectionCategory(collectionID, filterID);
			}
			else
			{
				tagsForThisFilterData = LibraryBizService.AvailableFiltersForLibraryCategory(libraryID, filterID);
			}
			return tagsForThisFilterData;
		}

		//Delete a resource from elastic
		//public void DeleteResourceByVersionIDXXXX(int versionID)
		//{
		//	var intID = new ResourceBizService().GetIntIDFromVersionID(versionID);
		//	DeleteResourceXX(intID);
		//}
		//public void DeleteResourceXX(int resourceID)
		//{
		//	new ElasticSearchManager().DeleteResource(resourceID);
		//}
		public void BulkDeleteResources(List<int> ids)
		{
			new ElasticSearchManager().DeleteResources(ids);
		}
        public void DeleteResource(int resourceID)
        {
            new ElasticSearchManager().DeleteResource(resourceID);
        }


		#endregion

		#region == delayed publishing ==
		/// <summary>
		/// This method is typically used to check if afer changes to a content item, particularly if part of a learning list, there is data to sync to the resource(s)
		/// </summary>
		/// <param name="nodeID"></param>
		/// <param name="userId"></param>
		public void CheckForDelayedPublishing( int nodeID, int userId )
		{
			string resourceList = "";
			string status = "";
			CurriculumServices csMgr = new CurriculumServices();
			ContentItem currentNode = csMgr.GetBasic( nodeID );
			if ( currentNode.Id > 0 && currentNode.ResourceIntId > 0 )
			{
				//get top node and call 
				int curriculumID = csMgr.GetCurriculumIDForNode( currentNode );
				bool successful = new ResourceManager().InitiateDelayedPublishing( curriculumID, currentNode.ResourceIntId, userId, ref resourceList, ref status );
				if ( successful && resourceList.Length > 0 )
				{
					System.Threading.ThreadPool.QueueUserWorkItem( delegate
					{
						//the list of resources are not passed, as this method will read the Resource_DelayedPublish table directly
						int cntr = AddDelayedResourcesToElastic( curriculumID, ref status );
					} );
				}
			}
		}
		public int ResourceDelayedPublish_AddElasticRequest( int contentId, int resourceId )
		{
			IsleDTO.ResourceDelayedPublish entity = new IsleDTO.ResourceDelayedPublish();
			entity.ContentId = contentId;
			entity.ResourceIntId = resourceId;
			entity.ElasticStatusId = 1;

			return new EFMgr.PublishingManager().ResourceDelayedPublish_Add( entity );
		}
		//public int ResourceDelayedPublish_Add( IsleDTO.ResourceDelayedPublish entity )
		//{
		//	return new EFMgr.PublishingManager().ResourceDelayedPublish_Add( entity );
		//}
		//public void PublishRelatedChildContent( int contentId, Patron user )
		//{
		//	ContentServices contentServices = new ContentServices();
		//	var content = contentServices.Get( contentId );
		//	PublishRelatedChildContent( content, user );
		//}

		/// <summary>
		/// Publish child content of the provided content item. This will be any content item that can have a child node, document item, or reference item.
		/// </summary>
		/// <param name="content"></param>
		/// <param name="user"></param>
		public void PublishRelatedChildContent( ContentItem content, Patron user )
		{
			//get parent
			//TODO - need to handle where a new node has been added, and not published - should not autopublish
			if ( content.ResourceIntId == 0 )
			{
				//perhaps a notify??

				return;
			}
			string status = "";

			//need to allow for any hierarchy type - to allow for publishing a part added later
			if ( content.TypeId == ContentItem.CURRICULUM_CONTENT_ID
				|| content.TypeId == ContentItem.LEARNING_SET_CONTENT_ID
				|| content.IsHierarchyType )
			{
				//just 50 for now
				//start auto publish  of hierarchy
				string resourceList = "";
				bool successful = new ResourceManager().InitiateDelayedPublishing( content.Id, content.ResourceIntId, user.Id, ref resourceList, ref status );

				if ( successful && resourceList.Length > 0 )
				{
					//we could test the scheduler here, as typically will be used when adding content to existing learning list.
					if ( UtilityManager.GetAppKeyValue( "useSchedulerWithNodeAutoPublish" ) == "yes" )
					{
						return;
					}


					//now add to other parts
					ResourceV2Services mgr2 = new ResourceV2Services();
					//mgr2.ImportRefreshResources( resourceList );
					string statusMessage = "";
					//do the thumbs
					//int thumbCntr = mgr2.AddThumbsForDelayedResources( content.Id, ref statusMessage );
					System.Threading.ThreadPool.QueueUserWorkItem( delegate
					{
						int thumbCntr = mgr2.AddThumbsForDelayedResources( content.Id, ref statusMessage );
					} );

					//now update elastic
					if ( UtilityManager.GetAppKeyValue( "doElasticIndexUpdateWithAutoPublish" ) == "yes" )
					{
						//int cntr = mgr2.AddDelayedResourcesToElastic( content.Id, ref statusMessage );
						System.Threading.ThreadPool.QueueUserWorkItem( delegate
						{
							int cntr = mgr2.AddDelayedResourcesToElastic( content.Id, ref statusMessage );
						} );
					}

					//Log activity
					System.Threading.ThreadPool.QueueUserWorkItem( delegate
					{
						new ActivityBizServices().AutoPublishActivity( new ResourceManager().Get( content.ResourceIntId ), user, resourceList );
					} );
				}
				else
				{
					//SetConsoleErrorMessage( "InitiateDelayedPublishing failed, or didn't return any resources.<br> : resourceList" );
					LoggingHelper.LogError( "Unexpected condition - no related content was found under a learning list/hierarchy item", true );
					//return;
				}
			}
			//}
		}
		public int AddThumbsForDelayedResources()
		{
			int lastResourceId = 0;
			int resourceCntr = 0;
			//cannot return anything for a scheduled process. Use activities
			string statusMessage = "";

			List<IsleDTO.ResourceDelayedPublish> list = EFMgr.PublishingManager.ResourceDelayedPublish_SelectPendingThumbnails();
			try
			{
				if ( list.Count == 0 )
				{
					LoggingHelper.DoTrace( 7, "ResourceV2Services.AddThumbsForDelayedResources. No pending delayed publishing request" );
					return 0;
				}
				foreach (IsleDTO.ResourceDelayedPublish item in list)
				{
					//not sure on status. This should be first, then ES
					if ( item.ThumbnailsStatusId == 1 )
					{
						resourceCntr++;
						//generate thumb. 
						ThumbnailServices.CreateThumbnail(item.ResourceIntId.ToString(), item.ResourceUrl, false);  //or true for overwriting

						lastResourceId = item.ResourceIntId;
						statusMessage += lastResourceId.ToString() + ", ";
						//now update status - not sure if completely appropriate as don't know if successful
						item.StatusId = 2;
						item.ThumbnailsStatusId = 2; //????

						int count = new EFMgr.PublishingManager().ResourceDelayedPublish_Update(item);
						
					}
				}
				//statusMessage = "Processed: " + statusMessage;
				LoggingHelper.DoTrace( 6, string.Format("ResourceV2Services.AddThumbsForDelayedResources. Requested thumbs: {0} ", list.Count) );
			}
			catch (Exception ex)
			{
				//could be a timeout
				statusMessage = string.Format("Error encountered. Had processed: {0}. Last successful resourceId: {1}", resourceCntr, lastResourceId);
				ServiceHelper.LogError(ex, "ResourceV2Services.AddDelayedResourcesToElastic. " + statusMessage);
			}
			//don't want to return anything for a schedule process
			//need to add to the activity log!
			return resourceCntr;
		}

		public int AddDelayedResourcesToElastic()
		{
			int lastResourceId = 0;
			int resourceCntr = 0;
			string statusMessage = "";
			EFMgr.PublishingManager pubMgr = new EFMgr.PublishingManager();
			List<IsleDTO.ResourceDelayedPublish> list = EFMgr.PublishingManager.ResourceDelayedPublish_SelectPendingElasticUpdates();
			try
			{
				if ( list.Count == 0 )
				{
					LoggingHelper.DoTrace( 7, "ResourceV2Services.AddDelayedResourcesToElastic. No pending delayed publishing request" );
					return 0;
				}

				foreach (IsleDTO.ResourceDelayedPublish item in list)
				{
					if ( item.ElasticStatusId == 1 )
					{
						resourceCntr++;
						//refresh resource in elastic index
						RefreshResource(item.ResourceIntId);
						lastResourceId = item.ResourceIntId;
						statusMessage += lastResourceId.ToString() + ", ";
						//now update status
						item.ElasticStatusId = 2;
						item.StatusId = 3;
						int count = pubMgr.ResourceDelayedPublish_Update( item );
			
					}
				}
				string message = string.Format( "ResourceV2Services.AddDelayedResourcesToElastic. Requested items: {0} ", list.Count );
				ServiceHelper.SetConsoleSuccessMessage( message );
				//statusMessage = "Processed: " + statusMessage;
				LoggingHelper.DoTrace( 6, message );
			}
			catch (Exception ex)
			{
				//could be a timeout
				statusMessage = string.Format("Error encountered. Had processed: {0}. Last successful resourceId: {1}", resourceCntr, lastResourceId);
				ServiceHelper.LogError(ex, "ResourceV2Services.AddDelayedResourcesToElastic. " + statusMessage);
			}
			//don't want to return anything for a schedule process
			//need to add to the activity log!
			return resourceCntr;
		}

		public void PublishDelayedResourcesToLR()
		{
			//get type 3, or maybe a specific lrStateId
			List<IsleDTO.ResourceDelayedPublish> list = EFMgr.PublishingManager.ResourceDelayedPublish_SelectPendingLRUpdates();

			foreach (IsleDTO.ResourceDelayedPublish item in list)
			{
				if ( item.LRPublishStatusId == 1 )
				{

					//???????????????????????????
					//format and call LR publish

					//now update status
					item.LRPublishStatusId = 2; //????
					item.StatusId = 3;
					item.PublishedDate = System.DateTime.Now;

					//int count = new EFMgr.PublishingManager().ResourceDelayedPublish_Update( item );
					//if ( item.ContentTypeId == 40 )
					//{

					//}
				}
			}

		}
		/// <summary>
		/// Request thumbnails for all appropriate components of the passed content item if
		/// </summary>
		/// <param name="parentContentId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public int AddThumbsForDelayedResources( int parentContentId, ref string statusMessage )
		{
			int lastResourceId = 0;
			int resourceCntr = 0;
			statusMessage = "";

			List<IsleDTO.ResourceDelayedPublish> list = EFMgr.PublishingManager.ResourceDelayedPublish_Select( parentContentId );
			try
			{
				foreach ( IsleDTO.ResourceDelayedPublish item in list )
				{
					//not sure on status. This should be first, then ES
					if ( item.ThumbnailsStatusId == 1 )
					{
						resourceCntr++;
						//generate thumb. 
						ThumbnailServices.CreateThumbnail( item.ResourceIntId.ToString(), item.ResourceUrl, false );  //or true for overwriting

						lastResourceId = item.ResourceIntId;
						statusMessage += lastResourceId.ToString() + ", ";
						//now update status
						item.ThumbnailsStatusId = 2; //????
						item.StatusId = 2;
						int count = new EFMgr.PublishingManager().ResourceDelayedPublish_Update( item );
						//copy image
						//if ( item.ContentTypeId == 40 )
						//{
						//	//try to copy an existing image to the resource folders
						//	/*
						//	 *	from	/OERThumbs/large/content-6101-large.png

						//		to		/OERThumbs/large/585607-large.png

						//	 * 
						//	 */
						//	string srcImage = string.Format( "????/OERThumbs/large/content-{0}-large.png", item.ContentId );
						//	string destImage = string.Format( "????/OERThumbs/large/{0}-large.png", item.ResourceIntId );
						//}
					}
				}
				statusMessage = "Processed: " + statusMessage;
			}
			catch ( Exception ex )
			{
				//could be a timeout
				statusMessage = string.Format( "Error encountered. Had processed: {0}. Last successful resourceId: {1}", resourceCntr, lastResourceId );
				ServiceHelper.LogError( ex, "ResourceV2Services.AddDelayedResourcesToElastic. " + statusMessage );
			}
			return resourceCntr;
		}
		/// <summary>
		/// Retrieve any pending delayed publishing resources for the provided content Id.
		/// Checks ElasticStatusId. If 1, then has not been processed yet. Process record and set to 2 (done)
		/// </summary>
		/// <param name="parentContentId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public int AddDelayedResourcesToElastic( int parentContentId, ref string statusMessage )
		{
			int lastResourceId = 0;
			int resourceCntr = 0;
			statusMessage = "";

			List<IsleDTO.ResourceDelayedPublish> list = EFMgr.PublishingManager.ResourceDelayedPublish_Select( parentContentId );
			try
			{
				foreach ( IsleDTO.ResourceDelayedPublish item in list )
				{
					if ( item.ElasticStatusId == 1 )
					{
						resourceCntr++;
						//refresh resource in elastic index
						RefreshResource( item.ResourceIntId );

						lastResourceId = item.ResourceIntId;
						statusMessage += lastResourceId.ToString() + ", ";
						//now update status
						item.ElasticStatusId = 2;
						item.StatusId = 3;
						int count = new EFMgr.PublishingManager().ResourceDelayedPublish_Update( item );
						
					}
				}
				statusMessage = "Processed: " + statusMessage;
			}
			catch ( Exception ex )
			{
				//could be a timeout
				statusMessage = string.Format( "Error encountered. Had processed: {0}. Last successful resourceId: {1}", resourceCntr, lastResourceId );
				ServiceHelper.LogError( ex, "ResourceV2Services.AddDelayedResourcesToElastic. " + statusMessage );
			}
			return resourceCntr;
		}
		public void PublishDelayedResourcesToLR( int parentContentId )
		{

			//TBD
			List<IsleDTO.ResourceDelayedPublish> list = EFMgr.PublishingManager.ResourceDelayedPublish_Select( parentContentId );

			foreach ( IsleDTO.ResourceDelayedPublish item in list )
			{
				if ( item.LRPublishStatusId == 1 )
				{

					//???????????????????????????
					//now update status
					item.StatusId = 3;
					item.LRPublishStatusId = 2;
					item.PublishedDate = System.DateTime.Now;
					//int count = new EFMgr.PublishingManager().ResourceDelayedPublish_Update( item );
			
				}
			}

		}
		#endregion

		#region Utilities - Reindexing methods

		public string HandlePendingResourcesToReindex()
		{
			DateTime maxDate = new DateTime();
			//List<int> list = ResourceV2Services.Resource_ReindexList_Pending( ref maxDate);
			//maxDate = new DateTime();
			List<int> list = EFMgr.EFResourceManager.Resource_ReindexList_Pending( ref  maxDate );

			if ( list == null || list.Count == 0 )
			{
				SetConsoleErrorMessage( "There were no records in the table to reindex " );
				return "There were no records in the table to reindex ";
			}
			LoggingHelper.DoTrace(2, string.Format("$$$$$ ResourceV2Services.HandlePendingResourcesToReindex. Starting reindex of {0} resources", list.Count));
			new ResourceV2Services().ImportRefreshResources( list, false );

			LoggingHelper.DoTrace(2, "$$$$$ ResourceV2Services.HandlePendingResourcesToReindex. Updating status to 2");
			// set statusId to 2 for max date: {1}!
			//new ResourceV2Services().Resource_ReindexList_UpdatePending( maxDate );
			new IOERBusinessEntities.procs.ResourceManager().Resource_ReindexList_UpdatePending( maxDate );

			//if list count is 2000, there may be more --> actually assess how the reindex of 14000 resources when on Mar.12th
			string extra = "";
			if (list.Count == 2000) {
				extra = "<br/><br/>NOTE: there are probably more resources, do another reindex";
			}
			string message = string.Format( "Reindexed {0} Resources!" + extra, list.Count, maxDate.ToShortDateString() );
			SetConsoleSuccessMessage(message);

			return message;
			
		}

		/// <summary>
		/// Return list of resources with a pending status from the resource.ReindexList table.
		/// </summary>
		/// <param name="maxDate"></param>
		/// <returns></returns>
		public static List<int> Resource_ReindexList_Pending( ref DateTime maxDate )
		{
			maxDate = new DateTime();
			List<int> list = EFMgr.EFResourceManager.Resource_ReindexList_Pending( ref  maxDate );

			return list;
		}

		/// <summary>
		/// Set pending items in the resource.reindex list table to completed (status = 2) where created date is <= passed date
		/// </summary>
		/// <param name="maxDate"></param>
		public void Resource_ReindexList_UpdatePending( DateTime maxDate )
		{
			new IOERBusinessEntities.procs.ResourceManager().Resource_ReindexList_UpdatePending( maxDate );
		}
			
		#endregion

		#region Update methods

		public void AddResourceClickThrough( int resourceID, ThisUser user, string title )
		{
			new ResourceViewManager().Create(resourceID, user.Id);

			ActivityBizServices.ResourceClickThroughHit(resourceID, user.RowId.ToString(), title);

			new Isle.BizServices.ResourceV2Services().RefreshResource(resourceID);
		}

		#endregion
		#region Helper methods
		//Get strongly typed value
		public T GetColumn<T>(string columnName, DataRow row)
		{
			try
			{
				return row.Field<T>(columnName);
			}
			catch
			{
				return default(T);
			}
		}
		public List<int> GetIntList(string columnName, DataRow row)
		{
			var data = GetColumn<string>(columnName, row);
			try
			{
				return data.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
			}
			catch
			{
				return new List<int>();
			}
		}
		public List<string> GetStringList(string columnName, DataRow row)
		{
			var data = GetColumn<string>(columnName, row);
			try
			{
				return data.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
			}
			catch
			{
				return new List<string>();
			}
}

		/// <summary>
		/// Return list of all creative commons usage rights
		/// </summary>
		/// <returns></returns>
		public List<UsageRights> GetUsageRightsList()
		{
			return GetUsageRightsList(false);
		}


		/// <summary>
		/// Return list of creative commons usage rights
		/// </summary>
		/// <param name="forNewResourcesOnly">If true, only licenses applicable to new resources will be returned.</param>
		/// <returns></returns>
		public List<UsageRights> GetUsageRightsList( bool forNewResourcesOnly )
		{
			//TODO - should not be using with Codes.TagValue!!!!
			var rights = new List<UsageRights>();

			DataSet ds = CodeTableManager.ConditionsOfUse_Select();
			if ( DatabaseManager.DoesDataSetHaveRows( ds ) )
			{
				foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
				{
					if ( !forNewResourcesOnly
					|| ( forNewResourcesOnly && DatabaseManager.GetRowPossibleColumn( dr, "IsAllowedForNewResource", true ) ) )
					{
						rights.Add( new UsageRights()
						{
							CodeId = int.Parse( DatabaseManager.GetRowColumn( dr, "Id" ) ),
							Url = DatabaseManager.GetRowPossibleColumn( dr, "Url" ),
							Summary = DatabaseManager.GetRowColumn( dr, "Summary" ),
							Title = DatabaseManager.GetRowColumn( dr, "Title" ),
							Description = DatabaseManager.GetRowColumn( dr, "Description" ),
							IconUrl = DatabaseManager.GetRowColumn( dr, "IconUrl" ),
							MiniIconUrl = DatabaseManager.GetRowColumn( dr, "MiniIconUrl" ),
							TagId = 0,	//int.Parse( DatabaseManager.GetRowColumn( dr, "TagId" ) ),
							Custom = bool.Parse( DatabaseManager.GetRowColumn( dr, "IsCustom" ) ),
							Unknown = bool.Parse( DatabaseManager.GetRowColumn( dr, "IsUnknown" ) ),
							IsAllowedForNewResource = DatabaseManager.GetRowPossibleColumn( dr, "IsAllowedForNewResource", true )

						} );
					}
				}
			}
			

			return rights;
		}
		public UsageRights GetUsageRights( string url, int usageRightsId )
		{
			//get all options
			var rights = GetUsageRightsList();
			return GetUsageRights( url, usageRightsId, rights );
		}
		//public UsageRights GetUsageRights( int usageRightsId )
		//{
		//	//get all options
		//	var rights = GetUsageRightsList();
		//	return GetUsageRights( usageRightsId, rights );
		//}
		public UsageRights GetUsageRights_NA( int usageRightsId, List<UsageRights> rights )
		{
			var result = new UsageRights();
			//if missing, or unknown, what about fine print? need to have actual url
			if ( "0 8 ".IndexOf( usageRightsId.ToString() ) > -1 )
				return result;

			for (var i = 0; i < rights.Count(); i++)
			{
				if ( rights[ i ].CodeId == usageRightsId )
				{
					result = rights[i];
					result.Url = rights[ i ].Url;
					return result;
				}
			}

			//not found, return empty
			return result;
		}

		/// <summary>
		/// Populate UsageRights using a code id, or url if latter not present
		/// </summary>
		/// <param name="url"></param>
		/// <param name="usageRightsId"></param>
		/// <param name="rights"></param>
		/// <returns></returns>
		public UsageRights GetUsageRights( string url, int usageRightsId, List<UsageRights> rights )
		{
			var result = new UsageRights();
			if ( string.IsNullOrWhiteSpace( url ) && usageRightsId == 0 )
			{
				result = rights.Where( m => m.CodeId == 8 ).FirstOrDefault();
				return result;
			}

			if ( usageRightsId > 0  )
			{
				result = rights.Where( m => m.CodeId == usageRightsId ).FirstOrDefault();
				if ( usageRightsId == 4 )	//url should be provided
					result.Url = url;
				
				return result;
			}
			//else if ( usageRightsId == 8 )
			//{
			//	//no code, so should be unknown, but should handle where not set yet
			//	result = rights.Where( m => m.CodeId == 8 ).FirstOrDefault();
			//	result.Url = url;
			//	return result;
			//}

			//should not get to here, as usageRightsId should be fully populated now?
			//actually should look up url, just in case
			for ( var i = 0; i < rights.Count(); i++ )
			{
				if ( rights[ i ].Url.ToLower() == url.ToLower() )
				{
					result = rights[ i ];
					result.CodeId = rights[ i ].CodeId;
					//???, just found by url
					//result.Url = url;
					return result;
				}
			}
			if ( !string.IsNullOrWhiteSpace( url ) )
			{
				//work to eliminate Custom,and Unknown
				result = rights.Where( m => m.CodeId == 4 ).FirstOrDefault();
				result.Url = url;
				return result;
			}
			else
			{
				result = rights.Where( m => m.CodeId == 8 ).FirstOrDefault();
				result.Url = url;
				return result;
			}
		}

		public List<DDLItem> GetContentPrivilegesList()
		{
			var result = new List<DDLItem>();
			var data = new ContentServices().ContentPrivilegeCodes_Select();
			if (DoesDataSetHaveRows(data))
			{
				foreach (DataRow dr in data.Tables[0].Rows)
				{
					result.Add(new DDLItem()
					{
						Id = GetColumn<int>("Id", dr),
						Title = GetColumn<string>("Title", dr)
					});
				}
			}
			return result;
		}

		public List<DDLLibrary> GetUserLibraryAndCollectionList(int userID)
		{
			var libService = new LibraryBizService();
			var libraries = libService.Library_SelectListWithContributeAccess(userID);
			var myLibrariesData = new List<DDLLibrary>();
			foreach (var item in libraries)
			{
				var lib = new DDLLibrary()
				{
					Id = item.Id,
					Title = item.Title,
					ImageUrl = item.ImageUrl
				};

				var colData = libService.LibrarySections_SelectListWithContributeAccess(item.Id, userID);//.Distinct(); //Not sure why this is returning duplicates
				foreach (var col in colData)
				{
					lib.Collections.Add(new DDLCollection()
					{
						Id = col.Id,
						Title = col.Title,
						ImageUrl = col.ImageUrl
					});
				}

				myLibrariesData.Add(lib);
			}
			return myLibrariesData;
		}

		/// <summary>
		/// Get a code table style object of all fields and tags
		/// </summary>
		/// <param name="pWithValuesOnly">If true, will only return codes that have been referenced (warehouseTotal > 0)</param>
		/// <returns></returns>
		public List<FieldDB> GetFieldAndTagCodeData(bool pWithValuesOnly = true)
		{
			return GetFieldAndTagData(0, pWithValuesOnly);
		}
		public List<FieldES> GetFieldCodes()
		{
			//Get code table
			//var fieldCodesDS = DatabaseManager.DoQuery("SELECT [Id], [Title], [SchemaTag], [SortOrder] FROM [Codes.TagCategory] WHERE IsActive = 1 ORDER BY [SortOrder], [Title]");

			var fieldCodesDS = CodeTableManager.CodesTagCategory_Select();
			var json = new ResourceJSONManager();

			//Assemble code data
			var fieldCodes = new List<FieldES>();
			foreach (DataRow dr in fieldCodesDS.Tables[0].Rows)
			{
				fieldCodes.Add(new FieldES()
				{
					Id = json.MakeInt(json.Get(dr, "Id")),
					Title = json.MakeString(json.Get(dr, "Title")),
					Schema = json.MakeString(json.Get(dr, "SchemaTag"))
				});
			}

			//Return data
			return fieldCodes;
		}
		/// <summary>
		/// Get a code table style object of all fields and tags for a resource (but can be zero). 
		/// </summary>
		/// <param name="resourceID">Provide zero, if just want all pertinent tag values</param>
		/// <param name="pWithValuesOnly">If true, will only return codes that have been referenced (warehouseTotal > 0)</param>
		/// <returns></returns>
		public List<FieldDB> GetFieldAndTagData(int resourceID, bool pWithValuesOnly )
		{
			var fields = new List<FieldDB>();

			//Get code data
			//Getall, including those without a used value
			//==> determine if called from somewhere that only wants those with values?
			var codesDS = CodeTableBizService.Codes_TagValue_GetAll(pWithValuesOnly);

			//Get Isle Section IDs
			var sitesDS = CodeTableManager.Codes_SiteTagCategory_Select();

			//Make a list of objects to temporarily hold the Isle Section IDs data
			var sites = new List<SiteData>();
			foreach (DataRow dr in sitesDS.Tables[0].Rows)
			{
				sites.Add(new SiteData()
				{
					SiteId = int.Parse(DatabaseManager.GetRowColumn(dr, "SiteId")),
					CategoryId = int.Parse(DatabaseManager.GetRowColumn(dr, "CategoryId"))
				});
			}

			//Build the list of fields
			foreach (DataRow dr in codesDS.Tables[0].Rows)
			{
				//Get ID
				var id = int.Parse(DatabaseManager.GetRowColumn(dr, "CategoryId"));
				//Skip this row if this category was already added
				if (fields.Where(m => m.Id == id).Count() > 0) { continue; }
				//Otherwise, add the category
				var field = new FieldDB()
				{
					Id = id,
					SortOrder = int.Parse(DatabaseManager.GetRowColumn(dr, "CategorySort")),
					Title = DatabaseManager.GetRowColumn(dr, "Category"),
					Schema = DatabaseManager.GetRowColumn(dr, "SchemaCat")
				};
				//Determine the list of section IDs
				field.IsleSectionIds = sites.Where(m => m.CategoryId == field.Id).Select(m => m.SiteId).ToList();

				//Add the field
				fields.Add(field);
			}

			var selectedIDs = new List<int>();
			if (resourceID > 0)
			{
				selectedIDs = EFMgr.ResourceTaggingManager.Resource_GetTagIds( resourceID );
				//Get resource tags
				//var idsDS = GetResourceTagValueIDs(resourceID);

				//Set "Selected" = true for things that were selected
				//foreach (DataRow dr in idsDS.Tables[0].Rows)
				//{
				//	selectedIDs.Add(int.Parse(DatabaseManager.GetRowColumn(dr, "TagvalueId")));
				//}
			}

			//Add tags to fields
			foreach (DataRow dr in codesDS.Tables[0].Rows)
			{
				var categoryID = int.Parse(DatabaseManager.GetRowColumn(dr, "CategoryId"));
				var id = int.Parse(DatabaseManager.GetRowColumn(dr, "TagId"));
				fields.Where(m => m.Id == categoryID).FirstOrDefault().Tags.Add(new TagDB()
				{
					Id = id,
					Title = DatabaseManager.GetRowColumn(dr, "Tag"),
					CategoryId = categoryID,
					Selected = selectedIDs.Contains(id),
					OldCodeId = int.Parse(DatabaseManager.GetRowColumn(dr, "CodeId")),
					Schema = DatabaseManager.GetRowColumn(dr, "SchemaTag")
				});
			}

			//Have to keep this around for legacy resources
			if (resourceID > 0)
			{
				GetOldStyleTagData(resourceID, ref fields);
			}

			return fields;
		}

		public string GetThumbnailUrl(int resourceID)
		{
			return "/OERThumbs/large/" + resourceID + "-large.png";
		}

		//Add stuff to a list
		public void AddItemPairToBulkList(ref List<object> bulkList, DataRow dr, List<GroupedTagData> groupData, ResourceJSONManager json, List<FieldES> fieldCodes)
		{
			//Format Data
			var data = GetResourceESFromVersionAndTagData(dr, groupData, json, fieldCodes);

			//Add index and resource pairs
			LoadBulkItem(ref bulkList, data);

		}
		public void LoadBulkItem(ref List<object> bulk, ResourceES item)
		{
			LoadBulkItem(ref bulk, item, "mainSearchCollection");
		}
		public void LoadBulkItem(ref List<object> bulk, ResourceES item, string indexName)
		{
			bulk.Add(new { index = new { _index = indexName, _type = "resource", _id = item.ResourceId } });
			bulk.Add(item);
		}

		//Get old style tag data - hopefully temporary!
		private void GetOldStyleTagData(int resourceID, ref List<FieldDB> fields)
		{
			//Resource Type
			var resourceTypeIDs = new ResourceTypeManager().Select(resourceID).Select(m => m.CodeId).ToList();
			SelectTagsFromIDs(resourceTypeIDs, ref fields, "learningResourceType");

			//Media Type
			var mediaTypeIDs = new ResourceFormatManager().Select(resourceID).Select(m => m.CodeId).ToList();
			SelectTagsFromIDs(mediaTypeIDs, ref fields, "mediaType");

			//K-12 Subject		??????????????????????????????
			var subjectIDsDS = DatabaseManager.DoQuery("SELECT [CodeId] FROM [Resource.Subject] WHERE ResourceIntId = 9 AND [CodeId] IS NOT NULL");
			var subjectIDs = new List<int>();
			foreach (DataRow dr in subjectIDsDS.Tables[0].Rows)
			{
				subjectIDs.Add(int.Parse(DatabaseManager.GetRowColumn(dr, "CodeId")));
			}
			SelectTagsFromIDs(subjectIDs, ref fields, "k12Subject");

			//Educational Use
			var edUseIDsDS = new ResourceEducationUseManager().SelectedCodes(resourceID);
			var edUseIDs = new List<int>();
			foreach (DataRow dr in edUseIDsDS.Tables[0].Rows)
			{
				if ( DatabaseManager.GetRowColumn( dr, "IsSelected" ).ToLower() == "true" )
				{
					edUseIDs.Add( int.Parse( DatabaseManager.GetRowColumn( dr, "Id" ) ) );
				}
			}
			SelectTagsFromIDs(edUseIDs, ref fields, "educationalUse");

			//Group Type
			var groupTypeIDsDS = new ResourceGroupTypeManager().SelectedCodes(resourceID);
			var groupTypeIDs = new List<int>();
			foreach (DataRow dr in groupTypeIDsDS.Tables[0].Rows)
			{
				if ( DatabaseManager.GetRowColumn( dr, "IsSelected" ).ToLower() == "true" )
				{
					groupTypeIDs.Add( int.Parse( DatabaseManager.GetRowColumn( dr, "Id" ) ) );
				}
			}
			SelectTagsFromIDs(groupTypeIDs, ref fields, "groupType");

			//Career Cluster
			var careerClusterIDsDS = new ResourceClusterManager().SelectedCodes(resourceID);
			var careerClusterIDs = new List<int>();
			foreach (DataRow dr in careerClusterIDsDS.Tables[0].Rows)
			{
				if (DatabaseManager.GetRowColumn(dr, "IsSelected").ToLower() == "true")
				{
					careerClusterIDs.Add(int.Parse(DatabaseManager.GetRowColumn(dr, "Id")));
				}
			}
			SelectTagsFromIDs(careerClusterIDs, ref fields, "careerCluster");

			//Grade Level
			var gradeLevelIDs = new ResourceGradeLevelManager().Select(resourceID).Select(m => m.CodeId).ToList();
			SelectTagsFromIDs(gradeLevelIDs, ref fields, "gradeLevel");

			//End User
			var endUserIDs = new ResourceIntendedAudienceManager().SelectCollection(resourceID).Select(m => m.CodeId).ToList();
			SelectTagsFromIDs(endUserIDs, ref fields, "educationalRole");

			//Access Rights
			ResourceVersion rv = ResourceBizService.ResourceVersion_GetByResourceId( resourceID );
			var accessRightsIDs = new List<int>() { rv.AccessRightsId };
			if (accessRightsIDs[0] < 2) { accessRightsIDs[0] = 2; }
			SelectTagsFromIDs(accessRightsIDs, ref fields, "accessRights");

			//Language
			var languageIDs = new ResourceLanguageManager().Select(resourceID).Select(m => m.CodeId).ToList();
			SelectTagsFromIDs(languageIDs, ref fields, "inLanguage");

		}
		private void SelectTagsFromIDs(List<int> ids, ref List<FieldDB> fields, string schema)
		{
			try
			{
				var field = fields.Where(m => m.Schema == schema).FirstOrDefault();
				foreach (var item in ids)
				{
					field.Tags.Where(m => m.OldCodeId == item).FirstOrDefault().Selected = true;
				}
			}
			catch { }
		}

		#endregion
		#region Helper classes
		public class AlignmentObject
		{
			public string targetName { get; set; }
			public string educationalFramework { get; set; }
			public object targetDescription { get; set; }
			public string alignmentType { get; set; }
			public string targetUrl { get; set; }
		}
		public struct GroupedTagData
		{
			public int ResourceId { get; set; }
			public FieldES FieldData { get; set; }
			public List<string> AliasValues { get; set; }
		}
		public class SiteData
		{
			public int SiteId { get; set; }
			public int CategoryId { get; set; }
		};
		public class DDLItem
		{
			public int Id { get; set; }
			public string Title { get; set; }
		}
		public class DDLLibrary : DDLItem
		{
			public DDLLibrary()
			{
				Collections = new List<DDLCollection>();
				ImageUrl = "";
			}
			public string ImageUrl { get; set; }
			public List<DDLCollection> Collections { get; set; }
		}
		public class DDLCollection : DDLItem
		{
			public DDLCollection()
			{
				ImageUrl = "";
			}
			public string ImageUrl { get; set; }
		}

		#endregion

		//Super mega kludge - is this association made at all in the database? I couldn't figure how, if it is.
		public List<StandardBody> GetStandardBodiesList()
		{
			var output = new List<StandardBody>();
			//CCSS Math
			output.Add(new StandardBody()
			{
				BodyId = 1,
				Url = "http://asn.desire2learn.com/resources/D10003FB",
				Title = "Common Core Math Standards",
				NotationPrefix = "CCSS.Math.Content",
				IdRanges = new Dictionary<int, int>() { { 1, 694 }, { 2472, 2481 } } //Pesky math practice standards
			});
			//CCSS English
			output.Add(new StandardBody()
			{
				BodyId = 2,
				Url = "http://asn.desire2learn.com/resources/D10003FC",
				Title = "Common Core ELA/Literacy Standards",
				NotationPrefix = "CCSS.ELA-Literacy",
				IdRanges = new Dictionary<int, int>() { { 695, 1778 } }
			});
			//NGSS
			output.Add(new StandardBody()
			{
				BodyId = 3,
				Url = "http://asn.desire2learn.com/resources/D2603111",
				Title = "Next Generation Science Standards",
				NotationPrefix = "NGSS",
				IdRanges = new Dictionary<int, int>() { { 2500, 2829 } }
			});
			//Fine Arts
			output.Add(new StandardBody()
			{
				BodyId = 4,
				Url = "http://asn.jesandco.org/resources/D10002E1",
				Title = "Illinois Fine Arts Standards",
				NotationPrefix = "IL.FA",
				IdRanges = new Dictionary<int, int>() { { 3000, 3082 } }
			});
			//Physical Development and Health
			output.Add(new StandardBody()
			{
				BodyId = 5,
				Url = "http://asn.jesandco.org/resources/D2406779",
				Title = "Illinois Physical Development and Health Standards",
				NotationPrefix = "IL.PDH",
				IdRanges = new Dictionary<int, int>() { { 3250, 3402 } }
			});
			//Social Science
			output.Add(new StandardBody()
			{
				BodyId = 5,
				Url = "http://asn.jesandco.org/resources/D2407056",
				Title = "Illinois Social Science Standards",
				NotationPrefix = "IL.SS",
				IdRanges = new Dictionary<int, int>() { { 3500, 3782 } }
			});
			//NHES
			output.Add(new StandardBody()
			{
				BodyId = 6,
				Url = "http://asn.desire2learn.com/resources/D2589605",
				Title = "National Health Education Standards",
				NotationPrefix = "NHES",
				IdRanges = new Dictionary<int, int>() { { 4003, 4154 } }
			});
			//Social Emotional
			output.Add(new StandardBody()
			{
				BodyId = 7,
				Url = "http://asn.jesandco.org/resources/D2406942",
				Title = "Illinois Social/Emotional Development Standards",
				NotationPrefix = "IL.SED",
				IdRanges = new Dictionary<int, int>() { { 4200, 4314 } }
			});
			//ABE Reading
			output.Add(new StandardBody()
			{
				BodyId = 8,
				Url = "http://asn.desire2learn.com/resources/D2609410",
				Title = "Illinois Adult Education (ABE/ASE) Reading Standards",
				NotationPrefix = "IL.ABE.Reading",
				IdRanges = new Dictionary<int, int>() { { 4500, 4643 } }
			});
			//ABE Writing
			output.Add(new StandardBody()
			{
				BodyId = 9,
				Url = "http://asn.desire2learn.com/resources/D2609411",
				Title = "Illinois Adult Education (ABE/ASE) Writing Standards",
				NotationPrefix = "IL.ABE.Writing",
				IdRanges = new Dictionary<int, int>() { { 5000, 5100 } }
			});
			//ABE Math
			output.Add(new StandardBody()
			{
				BodyId = 10,
				Url = "http://asn.desire2learn.com/resources/D2609857",
				Title = "Illinois Adult Education (ABE/ASE) Mathematics Standards",
				NotationPrefix = "IL.ABE.Math",
				IdRanges = new Dictionary<int, int>() { { 5500, 6115 } }
			});
			//K-12 Personal Finance
			output.Add(new StandardBody()
			{
				BodyId = 11,
				Url = "http://asn.desire2learn.com/resources/D2609021",
				Title = "National Standards in K-12 Personal Finance Education",
				NotationPrefix = "Finance.K12PFE",
				IdRanges = new Dictionary<int, int>() { { 6200, 6588 } }
			});
			//Voluntary Economics
			output.Add(new StandardBody()
			{
				BodyId = 12,
				Url = "http://asn.desire2learn.com/resources/D2604645",
				Title = "Voluntary National Content Standards in Economics",
				NotationPrefix = "Finance.VNCSE",
				IdRanges = new Dictionary<int, int>() { { 6700, 6943 } }
			});
			//Financial Literacy
			output.Add( new StandardBody()
			{
				BodyId = 13,
				Url = "http://asn.desire2learn.com/resources/D2604492",
				Title = "National Standards for Financial Literacy",
				NotationPrefix = "Finance.NSFL",
				IdRanges = new Dictionary<int, int>() { { 7000, 7150 } }
			} );
			//Adult Ed Math
			output.Add( new StandardBody()
			{
				BodyId = 14,
				Url = "http://asn.desire2learn.com/resources/D2609857/",
				Title = "Illinois Adult Education (ABE/ASE) Mathematics",
				NotationPrefix = "",
				IdRanges = new Dictionary<int, int>() { { 7500, 8232 } }
			} );
			//Adult Ed ELA Reading
			output.Add( new StandardBody()
			{
				BodyId = 15,
				Url = "http://asn.desire2learn.com/resources/S2615274",
				Title = "Illinois Adult Education (ABE/ASE) English Language Arts Standards for Reading",
				NotationPrefix = "",
				IdRanges = new Dictionary<int, int>() { { 8501, 9084 } }
			} );
			//Adult Ed ELA Writing
			output.Add( new StandardBody()
			{
				BodyId = 16,
				Url = "http://asn.desire2learn.com/resources/S2615843",
				Title = "Illinois Adult Education (ABE/ASE) English Language Arts Standards for Writing",
				NotationPrefix = "",
				IdRanges = new Dictionary<int, int>() { { 9085, 9547 } }
			} );
			//Adult Ed ELA Speaking/listening
			output.Add( new StandardBody()
			{
				BodyId = 17,
				Url = "http://asn.desire2learn.com/resources/S2616416",
				Title = "Illinois Adult Education (ABE/ASE) English Language Arts Standards for Speaking and Listening",
				NotationPrefix = "",
				IdRanges = new Dictionary<int, int>() { { 9548, 9682 } }
			} );
			//Illinois Foreign Language Standards
			output.Add( new StandardBody()
			{
				BodyId = 18,
				Url = "http://asn.jesandco.org/resources/D2615086",
				Title = "Illinois Foreign Language Standards",
				NotationPrefix = "",
				IdRanges = new Dictionary<int, int>() { { 9750, 9936 } } 
			} );
			//Framework for 21st Century Learning
			output.Add( new StandardBody()
			{
				BodyId = 18,
				Url = "http://asn.jesandco.org/resources/D10003D2",
				Title = "Framework for 21st Century Learning",
				NotationPrefix = "",
				IdRanges = new Dictionary<int, int>() { { 10000, 10158 } }
			} );
			//Rhode Island Civics
			output.Add( new StandardBody()
			{
				BodyId = 19,
				Url = "http://asn.jesandco.org/resources/S113A149",
				Title = "Rhode Island GSEs for Civics & Government",
				NotationPrefix = "",
				IdRanges = new Dictionary<int, int>() { { 10250, 10438 } }
			} );
			//Rhode Island History
			output.Add( new StandardBody()
			{
				BodyId = 20,
				Url = "http://asn.jesandco.org/resources/S113A14A",
				Title = "Rhode Island GSEs for Historical Perspectives/Rhode Island History",
				NotationPrefix = "",
				IdRanges = new Dictionary<int, int>() { { 10500, 10590 } }
			} );
			return output;
		}
		public class StandardBody
		{
			public int BodyId { get; set; }
			public string Url { get; set; }
			public string Title { get; set; }
			public string NotationPrefix { get; set; }
			public Dictionary<int, int> IdRanges { get; set; }
		}

	}

}
