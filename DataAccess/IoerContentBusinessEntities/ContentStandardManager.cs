using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IB = ILPathways.Business;
using ILPathways.Common;
using ILPathways.Utilities;
using DAL = ILPathways.DAL;
using Isle.DTO;
using dtoc = Isle.DTO.Content;

namespace IoerContentBusinessEntities
{
	/// <summary>
	/// Methods for managing content standards and related standards
	/// </summary>
	public class ContentStandardManager
	{
		static string thisClassName = "EF.ContentStandardManager";

		#region Content.Standards - core 

		public int ContentStandard_Add( IB.ContentStandard standard )
		{
			return ContentStandard_Add( standard.ContentId, standard.StandardId, standard.AlignmentTypeCodeId, standard.UsageTypeId, standard.CreatedById );
		}
		public int ContentStandard_Add( int contentId, int standardId, int alignmentTypeCodeId, int usageTypeId, int createdById )
		{
			Content_Standard efEntity = new Content_Standard();
			int newId = 0;
			using ( var context = new IsleContentContext() )
			{
				try
				{
					//interface doesn't prevent duplicate adds, so check here
					efEntity = context.Content_Standard.SingleOrDefault( s => s.ContentId == contentId && s.StandardId == standardId );
					if ( efEntity != null && efEntity.Id > 0 )
					{
						//need to check what caller does with this. return zero, so a related standard is not created
						return 0;
					}

					efEntity = new Content_Standard();
					efEntity.ContentId = contentId;
					efEntity.StandardId = standardId;
					efEntity.AlignmentTypeCodeId = alignmentTypeCodeId;
					efEntity.UsageTypeId = usageTypeId;
					efEntity.CreatedById = createdById;
					efEntity.Created = System.DateTime.Now;
					efEntity.LastUpdatedById = createdById;
					efEntity.LastUpdated = System.DateTime.Now;
					efEntity.IsDirectStandard = true;

					context.Content_Standard.Add( efEntity );

					int count = context.SaveChanges();
					if ( count > 0 )
					{
						newId = efEntity.Id;
						//add related standard
						//may need at a higher level so that can reference elastic update code
						//ContentRelatedStandard_Add( contentId, newId );
					}
					else
					{
						//?no info on error
					}
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + ".ContentStandard_Add()" );
				}
			}

			return newId;
		}

		/// <summary>
		/// Update a standard.
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <param name="alignmentTypeCodeId"></param>
		/// <param name="usageTypeId"></param>
		/// <param name="lastUpdatedById"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool ContentStandard_Update( int id, int alignmentTypeCodeId, int usageTypeId, int lastUpdatedById, ref string statusMessage )
		{
			bool isValid = false;
			try
			{
				using ( var context = new IsleContentContext() )
				{
					Content_Standard efEntity = context.Content_Standard.SingleOrDefault( s => s.Id == id );
					if ( efEntity != null && efEntity.Id > 0 )
					{
						efEntity.AlignmentTypeCodeId = alignmentTypeCodeId;
						efEntity.UsageTypeId = usageTypeId;
						efEntity.LastUpdatedById = lastUpdatedById;
						efEntity.LastUpdated = System.DateTime.Now;

						int cnt = context.SaveChanges();
						if ( cnt > 0 )
							isValid = true;
					}
				}

			}
			catch ( Exception ex )
			{
				statusMessage = "Error: " + ex.Message;
				LoggingHelper.LogError( ex, thisClassName + ".ContentStandard_Add()" );
			}

			return isValid;
		}
		public bool ContentStandard_Delete( int id, ref string statusMessage )
		{
			bool isSuccessful = false;
			try
			{
				statusMessage = "";
				using ( var context = new IsleContentContext() )
				{
					Content_Standard item = context.Content_Standard.SingleOrDefault( s => s.Id == id );

					if ( item != null && item.Id > 0 )
					{
						context.Content_Standard.Remove( item );
						context.SaveChanges();
						isSuccessful = true;

						//remove related standard
						//need to do from services level in order to be able to do elastic updates
						//ContentRelatedStandard_Delete( id );
					}
				}
			}
			catch ( Exception ex )
			{
				isSuccessful = false;
				statusMessage = ex.Message;
			}
			return isSuccessful;
		}

		public static List<IB.Content_StandardSummary> Fill_ContentStandards( int contentId )
		{
			List<IB.Content_StandardSummary> list = new List<IB.Content_StandardSummary>();
			IB.Content_StandardSummary item = new IB.Content_StandardSummary();
			using ( var context = new IsleContentContext() )
			{
				List<ContentStandard_Summary> eflist = context.ContentStandard_Summary
						.Where( s => s.ContentId == contentId )
							.OrderBy( s => s.UsageTypeId ).ThenBy( s => s.NotationCode )
							.ToList();

				if ( eflist != null && eflist.Count > 0 )
				{
					foreach ( ContentStandard_Summary efom in eflist )
					{
						item = new IB.Content_StandardSummary();
						item.ContentId = efom.ContentId;
						item.Id = efom.ContentStandardId;
						item.StandardId = efom.StandardId;
						item.StandardUrl = efom.StandardUrl;
						item.NotationCode = efom.NotationCode;
						item.Description = efom.Description;

						if ( efom.Created != null )
							item.Created = ( System.DateTime ) efom.Created;
						item.CreatedById = efom.CreatedById;

						if ( efom.LastUpdated != null )
							item.LastUpdated = ( System.DateTime ) efom.LastUpdated;
						item.LastUpdatedById = efom.LastUpdatedById;

						item.AlignmentType = efom.AlignmentType;
						item.AlignmentTypeCodeId = efom.AlignmentTypeCodeId;
						item.UsageTypeId = efom.UsageTypeId;
						item.StandardUsage = efom.StandardUsage;

						item.IsDirectStandard = efom.IsDirectStandard == null ? true : ( bool ) efom.IsDirectStandard;

						list.Add( item );
					}
				}
			}

			return list;
		}

		#endregion


		#region Content.Standards - helpers

		public static void GetDistinctCurriculumStandards( IsleContentContext context, IB.ContentItem entity )
		{
			IB.Content_StandardSummary item = new IB.Content_StandardSummary();
			List<IB.Content_StandardSummary> list = new List<IB.Content_StandardSummary>();

			//get standards from all children and merge
			//need to skip docs, as already done
			List<Curriculum_DistinctStandards> childList = context.Curriculum_DistinctStandards
						.Where( s => s.CurriculumId == entity.Id )
						.OrderBy( s => s.UsageTypeId ).ThenBy( s => s.NotationCode )
						.ToList();

			if ( childList != null && childList.Count > 0 )
			{
				foreach ( Curriculum_DistinctStandards efom in childList )
				{

					item = new IB.Content_StandardSummary();
					item.ContentId = efom.CurriculumId;
					item.Id = efom.ContentStandardId;
					item.StandardId = efom.StandardId;
					item.StandardUrl = efom.StandardUrl;
					item.NotationCode = efom.NotationCode;
					item.Description = efom.Description;

					//if ( efom.Created != null )
					//    item.Created = ( System.DateTime ) efom.Created;
					//item.CreatedById = efom.CreatedById;

					//if ( efom.LastUpdated != null )
					//    item.LastUpdated = ( System.DateTime ) efom.LastUpdated;
					//item.LastUpdatedById = efom.LastUpdatedById;

					item.AlignmentType = efom.AlignmentType;
					//item.AlignmentTypeCodeId = efom.AlignmentTypeCodeId;
					item.UsageTypeId = efom.UsageTypeId;
					item.StandardUsage = efom.StandardUsage;

					item.IsDirectStandard = efom.IsDirectStandard == null ? true : ( bool ) efom.IsDirectStandard;

					list.Add( item );

					//if ( childNode.HasStandards || childNode.HasChildStandards )
					//{
					//    //entity.ChildrenStandards.AddRange( child.Standards );
					//    Content_MergeStandards( childNode, entity );
					//}

				}
			}
		}//

        
		#endregion


		#region Content.RelatedStandards
		public int ContentRelatedStandard_Add(  int contentId, int contentStandardId )
		{
			//may not be necessary
			int curriculumID = 0;
			return ContentRelatedStandard_Add( curriculumID, contentId, contentStandardId );
		}
		public int ContentRelatedStandard_Add( int curriculumID, int contentId, int contentStandardId )
		{
			int count = 0;
			using ( var context = new IsleContentContext() )
			{
				try
				{
					Content_RelatedStandard crs = new Content_RelatedStandard();
					crs.ContentId = contentId;
					crs.ContentStandardId = contentStandardId;
					crs.Created = System.DateTime.Now;

					context.Content_RelatedStandard.Add( crs );

					count = context.SaveChanges();

				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + ".ContentRelatedStandard_Add()" );
				}
			}

			return count;
		}
		[Obsolete]
		private int ContentRelatedStandard_AddGroup( int contentId, int contentStandardId )
		{
			Content_Standard efEntity = new Content_Standard();
			int newId = 0;
			using ( var context = new IsleContentContext() )
			{
				Content_RelatedStandard crs = new Content_RelatedStandard();
				try
				{
					//get content and add to parent
					Content item = context.Contents
							.SingleOrDefault( s => s.Id == contentId );

					while ( item.ParentId > 0 )
					{
						//add
						crs = new Content_RelatedStandard();
						crs.ContentId = ( int ) item.ParentId;
						crs.ContentStandardId = contentStandardId;
						crs.Created = System.DateTime.Now;

						context.Content_RelatedStandard.Add( crs );

						int count = context.SaveChanges();

						//if has resourceId, do something 
						if ( item.ResourceIntId > 0 )
						{

						}
						//get next
						item = context.Contents
							.SingleOrDefault( s => s.Id == item.ParentId );

					} //while

				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + ".ContentRelatedStandard_Add()" );
				}
			}

			return newId;
		}

		public bool ContentRelatedStandard_Delete( int contentStandardId, int contentId )
		{
			bool isSuccessful = false;

			using ( var context = new IsleContentContext() )
			{
				Content_RelatedStandard item = context.Content_RelatedStandard.SingleOrDefault( s => s.ContentStandardId == contentStandardId && s.ContentId == contentId );

				if ( item != null && item.ContentId > 0 )
				{
					context.Content_RelatedStandard.Remove( item );
					context.SaveChanges();
					isSuccessful = true;
				}

			}
			return isSuccessful;
		}//

		[Obsolete]
		private void ContentRelatedStandard_Delete( int contentStandardId )
		{
			List<int> contentItems = new List<int>();

			using ( var context = new IsleContentContext() )
			{
				//may want to create a view, that would include any existing resource ids, so can handle - but this would be at a higher level
				List<Content_RelatedStandard> list = context.Content_RelatedStandard
					.Where( s => s.ContentStandardId == contentStandardId )
					.ToList();
				foreach ( Content_RelatedStandard item in list )
				{
					context.Content_RelatedStandard.Remove( item );
					context.SaveChanges();

					//or may be better to store in a work (delayed type) table
					contentItems.Add( item.ContentId );
				}
			}

		}//


		public List<Content_RelatedStandard> ContentRelatedStandard_Select( int contentStandardId )
		{
			List<Content_RelatedStandard> list = new List<Content_RelatedStandard>();
			
			using ( var context = new IsleContentContext() )
			{
				try
				{
					list = context.Content_RelatedStandard
						.Where( s => s.ContentStandardId == contentStandardId )
						.OrderBy( s => s.ContentId )
						.ToList();

				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + ".ContentRelatedStandard_Select()" );
				}
			}

			return list;
		}
		public static List<IB.Content_RelatedStandardsSummary> ContentRelatedStandard_Summary( int contentStandardId )
		{
			List<IB.Content_RelatedStandardsSummary> list = new List<IB.Content_RelatedStandardsSummary>();
			IB.Content_RelatedStandardsSummary item = new IB.Content_RelatedStandardsSummary();
			using ( var context = new IsleContentContext() )
			{
				List<Content_RelatedStandardsSummary> eflist = context.Content_RelatedStandardsSummary
						.Where( s => s.ContentStandardId == contentStandardId )
							.OrderBy( s => s.UsageTypeId ).ThenBy( s => s.NotationCode )
							.ToList();

				if ( eflist != null && eflist.Count > 0 )
				{
					foreach ( Content_RelatedStandardsSummary efom in eflist )
					{
						item = new IB.Content_RelatedStandardsSummary();
						item.ContentId = efom.ContentId;
						item.TypeId = efom.TypeId == null ? 10 : ( int ) efom.TypeId;
						item.ResourceIntId = efom.ResourceIntId == null ? 0 : ( int ) efom.ResourceIntId;
						item.StatusId = efom.StatusId == null ? 2 : (int) efom.StatusId;

						item.ContentStandardId = efom.ContentStandardId;
						item.StandardId = efom.StandardId;
						item.StandardUrl = efom.StandardUrl;
						item.NotationCode = efom.NotationCode;
						item.Description = efom.Description;

						if ( efom.RelatedStandardCreated != null )
							item.Created = ( System.DateTime ) efom.RelatedStandardCreated;
						item.CreatedById = efom.CreatedById;

						item.AlignmentType = efom.AlignmentType;
						item.AlignmentTypeCodeId = efom.AlignmentTypeCodeId;
						item.UsageTypeId = efom.UsageTypeId;
						item.StandardUsage = efom.StandardUsage;

						item.IsDirectStandard = false;
					
						list.Add( item );
					}
				}
			}

			return list;
		}

		#endregion



		#region Content.Standards - OLD



		#endregion

	}
}
