using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;

using AutoMapper;

using ILPContentFile = ILPathways.Business.ContentFile;
using IB = ILPathways.Business;
using ILPathways.Common;
using ILPathways.Utilities;
using DAL = ILPathways.DAL;
using Isle.DTO;
using dtoc = Isle.DTO.Content;

namespace IoerContentBusinessEntities
{
	class ContentObsoleteMethods
	{
		static bool usingContentConnector = false;

		#region Content.Connector
		public int ContentConnectorAdd( int parentId, int childId, int createdById )
		{
			int newId = 0;
			//Content_Connector efEntity = new Content_Connector();

			//try
			//{
			//    efEntity.ParentId = parentId;
			//    efEntity.ChildId = childId;
			//    //TODO -could do a max of current, and add 5?
			//    efEntity.SortOrder = DefaultSortOrder;
			//    efEntity.CreatedById = createdById;
			//    efEntity.Created = System.DateTime.Now;

			//    ctx.Content_Connector.Add( efEntity );

			//    // submit the change to database
			//    int count = ctx.SaveChanges();
			//    if ( count > 0 )
			//    {
			//        newId = efEntity.Id;
			//    }
			//    else
			//    {
			//        //?no info on error
			//    }
			//}
			//catch ( Exception ex )
			//{
			//    LoggingHelper.LogError( ex, thisClassName + ".ContentConnectorAdd()" );
			//}

			return newId;
		}
		public bool ContentConnector_Delete( int id, ref string statusMessage )
		{
			bool isSuccessful = false;
			try
			{
				//Content_Connector item = ctx.Content_Connector.SingleOrDefault( s => s.Id == id );

				//if ( item != null && item.ParentId > 0 )
				//{
				//    ctx.Content_Connector.Remove( item );
				//    ctx.SaveChanges();
				//    isSuccessful = true;

				//    //now delete child content
				//    Content_Delete( item.ChildId, ref statusMessage );
				//}
			}
			catch ( Exception ex )
			{
				isSuccessful = false;
				statusMessage = ex.Message;
			}
			return isSuccessful;
		}
		public bool ContentConnector_Delete( int parentId, int childId, ref string statusMessage )
		{
			bool isSuccessful = false;
			try
			{
				//Content_Connector item = ctx.Content_Connector.SingleOrDefault( s => s.ParentId == parentId && s.ChildId == childId );

				//if ( item != null && item.ParentId > 0 )
				//{
				//    ctx.Content_Connector.Remove( item );
				//    ctx.SaveChanges();
				//    isSuccessful = true;

				//    //now delete child content
				//    Content_Delete( childId, ref statusMessage );
				//}
			}
			catch ( Exception ex )
			{
				isSuccessful = false;
				statusMessage = ex.Message;
			}
			return isSuccessful;
		}
		///// <summary>
		///// delete a connector where the child CI is being deleted
		///// </summary>
		///// <param name="parentId"></param>
		///// <param name="childId"></param>
		///// <returns></returns>
		public static bool ContentConnector_DeleteChild( int childId, ref string statusMessage )
		{
			bool isSuccessful = true;
			try
			{
				using ( var context = new IsleContentContext() )
				{
					//List<Content_Connector> eflist = context.Content_Connector
					//                .Where( s => s.ChildId == childId )
					//                .ToList();

					//if ( eflist != null && eflist.Count > 0 )
					//{
					//    foreach ( Content_Connector efom in eflist )
					//    {
					//        context.Content_Connector.Remove( efom );
					//        context.SaveChanges(); ;
					//    }
					//}
				}
			}
			catch ( Exception ex )
			{
				isSuccessful = false;
				statusMessage = ex.Message;
			}
			return isSuccessful;
		}

		private static int ContentConnector_GetParent( IB.ContentItem entity )
		{

			int parentId = 0;
			using ( var context = new IsleContentContext() )
			{
				//Content_Connector parent = context.Content_Connector
				//        .SingleOrDefault( s => s.ChildId == entity.Id );

				//if ( parent != null && parent.Id > 0 )
				//{
				//    parentId = parent.ParentId;
				//}
			}

			return parentId;
		}

		private static IB.ContentItem Content_ToMap( ContentConnector_ChildSummary fromEntity, bool doingCompleteFill )
		{
			IB.ContentItem to = new IB.ContentItem();
			try
			{
				// if ( IsUsingContentStandards() )
				to.UsingContentStandards = true;

				to.IsValid = true;
				to.Id = fromEntity.ContentId;
				to.RowId = fromEntity.ContentRowId;

				to.Title = fromEntity.Title != null ? fromEntity.Title : "";
				to.Description = fromEntity.Description != null ? fromEntity.Description : "";
				to.Summary = fromEntity.Summary != null ? fromEntity.Summary : "";
				to.SortOrder = fromEntity.SortOrder;
				to.ParentId = fromEntity.ParentId;

				//to.IsPublished = ( bool ) fromEntity.IsPublished;
				to.IsActive = ( bool ) fromEntity.IsActive;

				to.TypeId = fromEntity.TypeId != null ? ( int ) fromEntity.TypeId : 0;
				to.ContentType = fromEntity.ContentType != null ? fromEntity.ContentType : "";
				if ( to.ContentType == "" )
				{
					//to.ContentType = ContentType_Get( to.TypeId );
				}
				to.StatusId = fromEntity.StatusId != null ? ( int ) fromEntity.StatusId : 0;
				to.Status = fromEntity.ContentStatus != null ? fromEntity.ContentStatus : "";


				to.PrivilegeTypeId = fromEntity.PrivilegeTypeId != null ? ( int ) fromEntity.PrivilegeTypeId : 0;
				to.PrivilegeType = fromEntity.ContentPrivilege != null ? fromEntity.ContentPrivilege : "";

				to.ConditionsOfUseId = fromEntity.ConditionsOfUseId;
				if ( to.ConditionsOfUseId > 0 )
				{
					//LR_ConditionOfUse_Select cou = ConditionOfUse_Get( to.ConditionsOfUseId );
					//no FK to conditions of use, the caller needs to resolve?
					//to.ConditionsOfUse = cou.Summary != null ? cou.Summary : "";

					//need to check for custom - or get both and let interface handle
					//- if custom exists, then ConditionsOfUseUrl will have been set to be the same
					//to.ConditionsOfUseUrl = cou.Url != null ? cou.Url : "";
					//to.ConditionsOfUseIconUrl = cou.IconUrl != null ? cou.IconUrl : "";
				}

				to.UseRightsUrl = fromEntity.UseRightsUrl != null ? fromEntity.UseRightsUrl : "";

				//to.ResourceVersionId = fromEntity.ResourceVersionId;
				to.ResourceIntId = fromEntity.ResourceIntId != null ? ( int ) fromEntity.ResourceIntId : 0;
				to.ResourceUrl = fromEntity.ResourceUrl != null ? fromEntity.ResourceUrl : "";

				to.DocumentUrl = fromEntity.DocumentUrl != null ? fromEntity.DocumentUrl : "";
				if ( fromEntity.DocumentRowId != null )
					to.DocumentRowId = ( Guid ) fromEntity.DocumentRowId;
				if ( to.DocumentUrl != null && to.IsValidRowId( to.DocumentRowId ) )
				{
					//to.RelatedDocument = Document_Version_Get( to.DocumentRowId );
				}

				to.IsOrgContentOwner = fromEntity.IsOrgContentOwner;

				to.OrgId = fromEntity.OrgId;
				if ( to.OrgId > 0 )
				{
					//Gateway_OrgSummary org = Gateway_OrgSummary_Get( to.OrgId );

					//if ( org != null && org.id > 0 )
					//{
					//	to.Organization = org.Name;

					//	to.ParentOrgId = org.parentId != null ? ( int ) org.parentId : 0;
					//	to.ParentOrganization = org.ParentOrganization != null ? org.ParentOrganization : "";
					//}
				}
				if ( doingCompleteFill )
				{
					//get standards
					//if ( to.UsingContentStandards )
					//{
					//to.ContentStandards = Fill_ContentStandards( to.Id );
					//}
					//else if ( to.ResourceIntId > 0 )
					//{
					//	to.Standards = FillResourceStandards( to.Id, to.ResourceIntId );
					//}
				}

				to.Approved = fromEntity.Approved != null ? ( System.DateTime ) fromEntity.Approved : to.DefaultDate;
				to.ApprovedById = fromEntity.ApprovedById != null ? ( int ) fromEntity.ApprovedById : 0;

				to.Created = fromEntity.Created != null ? ( System.DateTime ) fromEntity.Created : to.DefaultDate;
				to.CreatedById = fromEntity.CreatedById != null ? ( int ) fromEntity.CreatedById : 0;
				to.LastUpdated = fromEntity.LastUpdated != null ? ( System.DateTime ) fromEntity.LastUpdated : to.DefaultDate;
				to.LastUpdatedById = fromEntity.LastUpdatedById != null ? ( int ) fromEntity.LastUpdatedById : 0;
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, "Content_ToMap( ContentConnector_ChildSummary fromEntity )" );
			}
			return to;
		}

		#endregion


		#region Resource.Standards 
		
 
		/// <summary>
		/// soon to be obsolete!!??
		/// </summary>
		/// <param name="childEntity"></param>
		/// <param name="parentEntity"></param>
		//[Obsolete]
		//private static void Content_MergeResourceStandards( IB.ContentItem childEntity, IB.ContentItem parentEntity )
		//{

		//	if ( childEntity.HasStandards == false && childEntity.HasChildStandards == false )
		//		return;

		//	List<IB.ContentResourceStandard> joinList = parentEntity.ChildrenStandards.ToList();

		//	foreach ( IB.ContentResourceStandard s in childEntity.Standards )
		//	{
		//		var existing = joinList.FirstOrDefault( x => x.StandardId == s.StandardId );
		//		if ( existing != null )
		//		{
		//			//add contentId to list - not right, fix in future **************************
		//			existing.ContentItemIds.Add( s.ContentId );
		//			//have to update item in joinlist - how?
		//		}
		//		else
		//		{
		//			joinList.Add( s );
		//		}
		//	}

		//	foreach ( IB.ContentResourceStandard s in childEntity.ChildrenStandards )
		//	{
		//		var existing = joinList.FirstOrDefault( x => x.StandardId == s.StandardId );
		//		if ( existing != null )
		//		{
		//			//add contentId to list
		//			existing.ContentItemIds.Add( s.ContentId );
		//		}
		//		else
		//		{
		//			joinList.Add( s );
		//		}
		//	}

		//	parentEntity.ChildrenStandards = joinList;
		//} //

		//public static List<IB.ContentResourceStandard> FillResourceStandards( int contentId, int resourceId )
		//{
		//	List<IB.ContentResourceStandard> list = new List<IB.ContentResourceStandard>();

		//	using ( var context = new IsleContentContext() )
		//	{
		//		List<Resource_Standard> eflist = context.Resource_Standard
		//						.Where( s => s.ResourceIntId == resourceId )
		//						.OrderBy( s => s.NotationCode )
		//						.ToList();

		//		if ( eflist != null && eflist.Count > 0 )
		//		{
		//			foreach ( Resource_Standard efom in eflist )
		//			{
		//				IB.ContentResourceStandard child = new IB.ContentResourceStandard();
		//				child.ContentId = contentId;
		//				//doesn't make sense here, only in an aggregator context. Or maybe should be here - will test
		//				child.ContentItemIds.Add( contentId );

		//				child.ResourceStandardId = efom.ResourceStandardId;
		//				child.ResourceIntId = efom.ResourceIntId;
		//				//child.ResourceVersionIntId = efom.ResourceVersionIntId;
		//				child.ResourceTitle = efom.ResourceTitle;
		//				child.ResourceSortTitle = efom.ResourceSortTitle;

		//				child.StandardId = efom.StandardId;
		//				child.StandardUrl = efom.StandardUrl;
		//				child.NotationCode = efom.NotationCode;
		//				child.Description = efom.Description;
		//				child.AlignedById = efom.AlignedById;
		//				child.IsDirectStandard = efom.IsDirectStandard == null ? true : ( bool ) efom.IsDirectStandard;

		//				child.AlignmentTypeCodeId = efom.AlignmentTypeCodeId;
		//				child.AlignmentType = efom.AlignmentType;

		//				child.AlignmentDegreeId = efom.AlignmentDegreeId;
		//				child.AlignmentDegree = efom.AlignmentDegree;
		//				if ( efom.Created != null )
		//					child.Created = ( System.DateTime ) efom.Created;
		//				child.AlignmentDegree = efom.AlignmentDegree;

		//				list.Add( child );
		//			}
		//		}
		//	}
		//	return list;

		//}
		//private static List<IB.ContentResourceStandard> FillResourceStandardsByRvId( int contentId, int resourceVersionId )
		//{
		//	List<IB.ContentResourceStandard> list = new List<IB.ContentResourceStandard>();

		//	using ( var context = new IsleContentContext() )
		//	{
		//		List<Resource_Standard> eflist = context.Resource_Standard
		//						.Where( s => s.ResourceVersionIntId == resourceVersionId )
		//						.OrderBy( s => s.NotationCode )
		//						.ToList();

		//		if ( eflist != null && eflist.Count > 0 )
		//		{
		//			foreach ( Resource_Standard efom in eflist )
		//			{
		//				IB.ContentResourceStandard child = new IB.ContentResourceStandard();
		//				child.ContentId = contentId;
		//				child.ResourceStandardId = efom.ResourceStandardId;
		//				child.ResourceIntId = efom.ResourceIntId;
		//				//child.ResourceVersionIntId = efom.ResourceVersionIntId;
		//				child.ResourceTitle = efom.ResourceTitle;
		//				child.ResourceSortTitle = efom.ResourceSortTitle;

		//				child.StandardId = efom.StandardId;
		//				child.StandardUrl = efom.StandardUrl;
		//				child.NotationCode = efom.NotationCode;
		//				child.Description = efom.Description;
		//				child.AlignedById = efom.AlignedById;
		//				child.IsDirectStandard = efom.IsDirectStandard == null ? true : ( bool ) efom.IsDirectStandard;

		//				child.AlignmentTypeCodeId = efom.AlignmentTypeCodeId;
		//				child.AlignmentType = efom.AlignmentType;

		//				child.AlignmentDegreeId = efom.AlignmentDegreeId;
		//				child.AlignmentDegree = efom.AlignmentDegree;
		//				if ( efom.Created != null )
		//					child.Created = ( System.DateTime ) efom.Created;
		//				child.AlignmentDegree = efom.AlignmentDegree;

		//				list.Add( child );
		//			}
		//		}
		//	}
		//	return list;

		//}


		#endregion
	}
}
