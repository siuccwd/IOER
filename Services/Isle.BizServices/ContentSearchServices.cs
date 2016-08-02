using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;

using Isle.DTO;

using ILPathways.Business;
using LRWarehouse.Business;
using ILPathways.Utilities;
using ILPathways.Common;
using MyManager = ILPathways.DAL.ContentManager;
using DocManager = ILPathways.DAL.DocumentStoreManager;
using DBM = ILPathways.DAL.DatabaseManager;

namespace Isle.BizServices
{
	public class ContentSearchServices
	{
		//static string thisClassName = "ContentServices";
		MyManager myMgr = new MyManager();

		private static string PublicFilter = "(base.PrivilegeTypeId = 1 AND base.IsActive = 1 AND base.StatusId = 5 ) ";
		private static string AdminFilter = "(base.PrivilegeTypeId >= 1 AND base.IsActive = 1 AND base.StatusId >= 0 ) ";
		//private static string BaseAuthFilter = "(base.PrivilegeTypeId = 1 AND base.StatusId = 5 ) ";
		private static string KeywordTemplate = "(base.Title like '{0}' OR base.[Summary] like '{0}' OR base.[Description] like '{0}' OR auth.SortName like '{0}') ";
		private static string SharedWithMeFilter = "(base.StatusId >= 0 AND base.privilegetypeid >= 1 AND base.ContentId in (SELECT [ContentId] FROM [dbo].[Content.Partner] where  [UserId] = {0} and [PartnerTypeId] > 0) )";
		private static string MyOrgFilter = "((base.OrgId = {0} OR auth.OrganizationId = {0}) AND base.StatusId = 5) ";
		string ContentTypeFilter = " base.TypeId in ({0})";
		string StatusTypeFilter = " base.StatusId in ({0})";
		string PrivilegeTypeFilter = " base.PrivilegeTypeId in ({0})";
		private static string StandardsFilter = "(base.ContentId in (SELECT [ContentId] FROM [dbo].[ContentStandard_Summary] where [StandardId] in ({0})) )  ";

		#region === content search ====
		/// <summary>
		/// Search for Content related data using passed parameters
		/// - uses custom paging
		/// - only requested range of rows will be returned
		/// </summary>
		/// <param name="pFilter"></param>
		/// <param name="pOrderBy">Sort order of results. If blank, the proc should have a default sort order</param>
		/// <param name="pStartPageIndex"></param>
		/// <param name="pMaximumRows"></param>
		/// <param name="pTotalRows"></param>
		/// <returns></returns>
		public List<ContentSearchResult> Search( ContentSearchQuery query, Patron user, ref int pTotalRows )
		{
			var results = new List<ContentSearchResult>();
			//following now done at login
			//if (new AccountServices().IsUserAdmin(user))
			//	user.TopAuthorization = 2;

			string filterDesc = "";
			string filter = FormatFilters( query, user, ref filterDesc );
			ContentSearchResult csrItem = new ContentSearchResult();
			string pOrderBy = query.SortOrder;
			if ( query.SortReversed )
				pOrderBy += " DESC";

			DataSet ds = myMgr.Search( filter, pOrderBy, query.PageStart, query.PageSize, ref pTotalRows );
			if ( ServiceHelper.DoesDataSetHaveRows( ds ) )
			{
				foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
				{
					bool allowingEdit = false;

					csrItem = new ContentSearchResult();
					csrItem.Id = ServiceHelper.GetRowColumn( dr, "ContentId", 0 );
					csrItem.Title = ServiceHelper.GetRowColumn( dr, "Title", "missing" );
					csrItem.Description = ServiceHelper.GetRowColumn( dr, "Summary", "" );

					csrItem.Guid = ServiceHelper.GetRowColumn( dr, "ContentRowId", "" );
					csrItem.ImageUrl = ServiceHelper.GetRowColumn( dr, "ImageUrl", "" );
					if ( csrItem.ImageUrl == "" )
					{
						var thumbName = "/OERThumbs/large/content-" + csrItem.Id + "-large.png";
						csrItem.ImageUrl = System.IO.File.Exists( @"\\OERDATASTORE" + thumbName ) ? thumbName : "";  //TBD
					}
					csrItem.OrganizationTitle = ServiceHelper.GetRowColumn( dr, "Organization", "" );
					csrItem.Privilege = ServiceHelper.GetRowColumn( dr, "ContentPrivilege", "");

					csrItem.Status = ServiceHelper.GetRowColumn( dr, "ContentStatus", "" );
					
					csrItem.ParentId = ServiceHelper.GetRowColumn( dr, "ParentId", 0 );
					csrItem.TypeId = ServiceHelper.GetRowColumn( dr, "TypeId", 10 );
					csrItem.Type = ServiceHelper.GetRowColumn( dr, "ContentType", "" );
					if ( csrItem.TypeId == 40 && csrItem.ParentId > 0 )
						csrItem.Type = "Learning List Document";
					
					csrItem.Author = ServiceHelper.GetRowColumn( dr, "Author", "" );
					csrItem.Updated = ServiceHelper.GetRowColumn( dr, "LastUpdated", DateTime.Today.ToShortDateString() );

					csrItem.Url = SetPublicUrl( csrItem.TypeId.ToString(), csrItem.Id, csrItem.Title, csrItem.ParentId );
					csrItem.Editable = false;		//TBD - check old

					string partnerList = ServiceHelper.GetRowColumn( dr, "PartnerList", "" );
					if ( user.TopAuthorization < 6 
						|| IsUserAPartner( partnerList, user.Id.ToString() ) )
						allowingEdit = true;

					if ( query.IsMyAuthoredView == true )
					{
						//must be authenticated ==> cannot user created by as user may have access removed
						//Label createdById = ( Label ) e.Row.FindControl( "lblCreatedById" );
						//if ( ( createdById != null && createdById.Text == user.Id.ToString() )
						//	|| allowingEdit )
						//{
						//	//editLink
						//	HyperLink actionsLink = ( HyperLink ) e.Row.FindControl( "editLink" );
						//	if ( actionsLink != null )
						//		actionsLink.Visible = true;
						//}
					}

					if ( allowingEdit )
					{
						csrItem.Editable = true;
						csrItem.EditUrl = SetEditUrl( csrItem );
						if ( csrItem.EditUrl.Length == 0 )
							csrItem.Editable = false;
					}
					results.Add( csrItem );

				}
			}
			if ( query.PageStart == 1 )
			{
				ActivityBizServices.ContentSearchHit( filterDesc, user );
			}
			return results;
		}

		/// <summary>
		/// Custom content search to return all public published learning lists for an org
		/// - At some point may want paging
		/// </summary>
		/// <param name="orgId"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public List<ContentSearchResult> GetLearningListsForOrganization( int orgId, Patron user, ref string message )
		{

			//show all for now, eventually will need paging
			int startingPageNbr = 1;
			int pageSize = 1000;

			return GetLearningListsForOrganization( orgId, user, startingPageNbr, pageSize, ref message );
		} //
		public List<ContentSearchResult> GetLearningListsForOrganization( int orgId, Patron user, int startingPageNbr, int pageSize, ref string message )
		{
			ContentSearchQuery query = new ContentSearchQuery();
			//
			int pTotalRows = 0;
			string filter = string.Format( " base.TypeId = 50 AND base.IsActive = 1 AND base.StatusId = 5 AND base.OrgId = {0}", orgId );
			var results = new List<ContentSearchResult>();

			query.Text = ""; //Prevent null reference error
			query.PageStart = startingPageNbr;
			query.CustomSearch = filter;
			query.SortOrder = ContentSearchQuery.SORT_BY_TITLE;
			results = new ContentSearchServices().Search( query, user, ref pTotalRows );

			message = query.Message; //May change this implementation soon(tm)

			return results;
		} //
		bool IsUserAPartner( string partnerList, string userId )
		{
			bool yes = false;
			if ( partnerList == null || partnerList.Trim().Length == 0 )
				return false;

			string[] partners = partnerList.Split( new char[] { ',' } );
			foreach ( string partner in partners )
			{
				if ( partner == userId )
				{
					yes = true;
					break;
				}
			}
			return yes;
		}
		public DataSet Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
		{
			//TODO - create a List<> version
			return myMgr.Search( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
		}
		#endregion

		public string SetPublicUrl( string typeId, int contentId, string title, int parentId )
		{
			string template = "/Content/{0}/{1}";
			string currTemplate = "/LearningList/{0}/{1}";
			string urlTitle = CleanTitle( title );

			//need to check if part of a curriculum
			//update search to provide
			if ( typeId.Equals( "40" ) )
				if ( parentId > 0 )
					return string.Format( currTemplate, parentId, urlTitle );
				else
					return string.Format( template, contentId, urlTitle );

			else if ( "50 52 54 56 ".IndexOf( typeId ) > -1 )
				return string.Format( currTemplate, contentId, urlTitle, title );
			else
				return string.Format( template, contentId, urlTitle, title );
		}//
		
		/// <summary>
		/// Determine editor for this content item.
		/// If item has a parent, determine if it should be the editor
		/// </summary>
		/// <param name="csrItem"></param>
		/// <returns></returns>
		public string SetEditUrl( ContentSearchResult csrItem )
		{
			string template = "/My/{0}.aspx?rid={1}";
			

			if ( csrItem.TypeId == 50 )
				return string.Format( "/My/LearningList/{0}/Edit", csrItem.Id );

			else if ( csrItem.TypeId == 52 || csrItem.TypeId == 54 || csrItem.TypeId == 56 )
				return string.Format( "/My/LearningList/{0}/Edit", csrItem.Id );
			else if ( csrItem.TypeId == 60 )
				return string.Format( "/My/LearningSet/{0}/Edit", csrItem.Id );
			else if ( csrItem.TypeId == 40 )
			{
				if ( csrItem.ParentId == 0 )
					return string.Format( template, "DocumentEditor", csrItem.Guid );
				else
					return string.Format( "/My/LearningList/{0}/Edit?childId={1}", csrItem.ParentId, csrItem.Id );
			}
			else if ( csrItem.TypeId == 41 )
			{
				if ( csrItem.ParentId == 0 ) //no editor
					return "";
				else
					return string.Format( "/My/LearningList/{0}/Edit?childId={1}", csrItem.ParentId, csrItem.Id );
				
			}
			else
				return string.Format( template, "Author", csrItem.Guid );
		}//
		public string CleanTitle( string text )
		{
			if ( string.IsNullOrEmpty( text ) )
				return "";
			else
				return ResourceBizService.FormatFriendlyTitle( text );
		}//


		#region === filtering
		public string FormatFilters( ContentSearchQuery query, Patron user, ref string filterDesc )
		{
			//min is active (would an option need if allowing to set inactive - vs unpublish!
			string filter = "";
			string booleanOperator = "AND";
			filterDesc = "";
			string authenticatedFilter = "";
			//Skip formatting if custom filter has already been supplied
			if ( !string.IsNullOrWhiteSpace( query.CustomSearch ) )
			{
				return query.CustomSearch;
			}

			filter = PublicFilter;
			//if only current author
			if ( AccountServices.IsUserAuthenticated( user ) )
			{
				//if (user.TopAuthorization > 4)
				//	authenticatedFilter = " (base.IsActive = 1) ";
				
				//booleanOperator = " OR ";
				FormatOrgFilters( query, booleanOperator, user, ref authenticatedFilter, ref filterDesc );
				if ( authenticatedFilter.Length > 0 )
				{
					filter = "(" + filter + " OR " + authenticatedFilter + ")";
					//filter = authenticatedFilter;
					authenticatedFilter = "";
				}
				else
				{
					//if no owner filters, then if has system admin, show all states
					if ( new AccountServices().IsUserAdmin( user ) )
					{
						filter = AdminFilter;
					}
				}

				//nothing here yet, so skip to avoid confusion
				//FormatPrivilegeTypeFilter( query, booleanOperator, user, ref authenticatedFilter, ref filterDesc );

				//if ( authenticatedFilter.Length > 0 )
				//{
				//	//?????
				//	//filter = "(" + filter + " OR " + authenticatedFilter + ")";
				//	filter = authenticatedFilter;
				//}
				//booleanOperator = "AND";

				//check for any privileges
				//if ( authenticatedFilter.ToLower().IndexOf( "base.privilegetypeid " ) == -1 )
				//	authenticatedFilter += ServiceHelper.FormatSearchItem( authenticatedFilter, " base.PrivilegeTypeId = 1 ", booleanOperator );

				//if ( authenticatedFilter.Length > 0 ) 
				//{
				//	//filter = "(" + filter + " OR " + authenticatedFilter + ")";
				//	filter = authenticatedFilter;
				//}
			}
			else
			{
				//restrictions?
				//only public ==> may want to use IsActive, so as not to show work in progress
				//filter = PublicFilter;
			}
			//date filters
			FormatDatesFilter( query, booleanOperator, ref filter, ref filterDesc );

			FormatContentTypeFilter( query, booleanOperator, ref filter, ref filterDesc );

			FormatStatusFilter( query, booleanOperator, ref filter, ref filterDesc );

			FormatStandardsFilter( query, booleanOperator, ref filter, ref filterDesc );
			//
			FormatKeyword( query.Text, booleanOperator, ref filter );

			//15-10-14 may want to save this to the end - ==> I had commented out for some reason?
			//should be able to see non pub for org - only if a partner
			//status is not in interface, so will never be set
			if ( filter.ToLower().IndexOf( "base.statusid " ) == -1 )
				filter += ServiceHelper.FormatSearchItem( filter, " base.statusid = 5 ", booleanOperator );

			if ( ServiceHelper.IsLocalHost() || (user != null && user.Id == 2 ))
			{
				ServiceHelper. SetConsoleSuccessMessage( "sql: " + filter );
				ServiceHelper.DoTrace( 6, "sql: " + filter );
			}
			return filter;
		}	//


		private void FormatKeyword( string text, string booleanOperator, ref string filter )
		{
			if ( string.IsNullOrWhiteSpace( text ) )
				return;
			string keyword = ServiceHelper.HandleApostrophes( ServiceHelper.CleanText( text.Trim() ) );
			string keywordFilter = "";

			if ( keyword.Length > 0 )
			{
				//filterDesc = filterDesc + "<div class='searchSection isleBox'>" + keyword + "</div>";
				keyword = keyword.Replace( "*", "%" );
				if ( keyword.IndexOf( "," ) > -1 )
				{
					string[] phrases = keyword.Split( new char[] { ',' } );
					foreach ( string phrase in phrases )
					{
						string next = phrase.Trim();
						if ( next.IndexOf( "%" ) == -1 )
							next = "%" + next + "%";
						string where = string.Format( KeywordTemplate, next );
						keywordFilter += ServiceHelper.FormatSearchItem( keywordFilter, where, "OR" );
					}
				}
				else
				{
					if ( keyword.IndexOf( "%" ) == -1 )
						keyword = "%" + keyword + "%";

					keywordFilter = string.Format( KeywordTemplate, keyword );

				}

				if ( keywordFilter.Length > 0 )
					filter += ServiceHelper.FormatSearchItem( filter, keywordFilter, booleanOperator );
			}
		}	//

		/// <summary>
		/// user has to be authenticated or these options will not show
		/// </summary>
		/// <param name="booleanOperator"></param>
		/// <param name="filter"></param>
		private void FormatOrgFilters( ContentSearchQuery query, string booleanOperator, Patron user, ref string filter, ref string filterDesc )
		{

			//need to allow approver to see submitted status
			//might be better to have a status filter, and hide when n/a

			//now handle multiple filters
			string where = "";
			string OR = "";
			string selDesc = "";
			foreach ( ContentSearchFilter li in query.Filters )
			{
				if ( li.Category == "Creator" )
				{
					foreach ( string tag in li.Tags )
					{
						int listCreatedBy = Int32.Parse( tag );
						if ( listCreatedBy == 1 )
						{
							//me.
							//however, need to consider where original creator no lonber has access (ex Scarlett created stuff for isbe)
							//at very least, the search results need to only allow edits if creator still has access
							//where = OR + string.Format( "(base.CreatedById = {0} AND base.StatusId == 4 ) ", user.Id );
							where = OR + FormatPersonalAccess(user.Id);
							OR = " OR ";
							selDesc = "Created By Me";
						}
						else if ( listCreatedBy == 2 )
						{
							//my org
							//TODO - need to use org mbrs now
							//		- for now my org will only mean their main one (should be user.OrgId)
							//and content.Partner
							//-limited to only published?
							//-now with multiple, avoid duplicates from created by me
							
							if ( user.OrgId > 0 )
							{
								//==> for now, overlay where (no OR)
								//where = FormatPersonalAccess( user.Id );
								where += OR + string.Format( MyOrgFilter, user.OrgId );
								
								selDesc = "Created by my organization";
								OR = " OR ";
							}
							else
							{
								query.Message = "You are not associated with an organization, so the <em>My Organization</em> option will not return results";
								//ServiceHelper.SetConsoleInfoMessage( "You are not associated with an organization, so this search option will not return results" );
								//where = string.Format( "(base.OrgId = {0} OR auth.OrganizationId = {0}) ", -1 );
								//OR = " OR ";
							}
							
						}

						else if ( listCreatedBy == 3 )
						{
							//only content shared with me 
							//status can probably be anything
							where += OR + string.Format( SharedWithMeFilter, user.Id ) ;

							selDesc = "Content that has been shared with me";
							OR = " OR ";
						}

						else if (listCreatedBy == 4)
						{
							//if anyone, should just set blank, and use default below
							where += OR + PublicFilter;
						}
					}

				}
			}	//

			if ( where == "" )
			{
				//all ==> so should not have any filter.
				//default privilege id will be set in FormatPrivilegeTypeFilter
				//this is confusing. It makes sense only for the first time. after that treat like a regular search
				selDesc = "";
				if ( query.IsMyAuthoredView == true )
				{
					where = FormatPersonalAccess( user.Id );
					if ( user.OrgId > 0 )
					{
						
						//where += string.Format( "OR (base.StatusId = 5 " +
						//							"AND ( (base.OrgId = {0} OR base.ParentOrgId = {0} OR auth.OrganizationId = {0})   ", user.OrgId );
						//if ( user.ParentOrgId > 0 )
						//	where += string.Format( "OR (base.OrgId = {0} OR base.ParentOrgId = {0} OR auth.OrganizationId = {0}) ) )", user.ParentOrgId );
						//else
						//	where += ") )";
						////or all others that are public and published
						//where += " OR (base.StatusId = 5 AND base.PrivilegeTypeId = 1 )";
					}
					else
					{
						//where = "(base.StatusId = 5 ) ";
					}
				}
				else
				{
					//==> set to minimum for authenticated user - means all shared, org related
					where = string.Format("(createdById = {0} AND base.StatusId > 0 )", user.Id );
					where += " OR " + string.Format( SharedWithMeFilter, user.Id );
					where += " OR " + string.Format( MyOrgFilter, user.OrgId );
				}
			}

			if ( where.Trim().Length > 0 )
			{
				filter += ServiceHelper.FormatSearchItem( filter, where, booleanOperator );
				if ( selDesc.Trim().Length > 5 )
					filterDesc = filterDesc + "<div class='searchSection isleBox'>" + selDesc + "</div>";
			}
			else if ( selDesc.Trim().Length > 5 )
			{
				filterDesc = filterDesc + "<div class='searchSection isleBox'>" + selDesc + "</div>";
			}
		}	//

		private string FormatPersonalAccess( int userid )
		{
		//eventually will not use created by. As the creator is added as an admin partner on create, we can use it
			string where = string.Format( "( (createdById = {0} AND base.StatusId > 0 ) OR "
						+ "(base.ContentId in (SELECT [ContentId] FROM [dbo].[Content.Partner] where  [UserId] = {0} and [PartnerTypeId] = 4) ) ) ", userid );
			return where;
		}

		private void FormatDatesFilter( ContentSearchQuery query, string booleanOperator, ref string filter, ref string filterDesc )
		{
			DateTime endDate = new DateTime();
			foreach ( ContentSearchFilter li in query.Filters )
			{
				if ( li.Category == "UpdatedRange" )
				{
					//just one?
					foreach ( string tag in li.Tags )
					{
						int rblIDateCreated = Int32.Parse( tag );
						if ( rblIDateCreated == 0 )
						{

							endDate = System.DateTime.Now.AddDays( -7 );
						}
						else if ( rblIDateCreated == 1 )
						{
							endDate = System.DateTime.Now.AddDays( -30 );
						}
						else if ( rblIDateCreated == 2 )
						{
							endDate = System.DateTime.Now.AddDays( -180 );
						}
						break;
					}

				}
			}
			if ( endDate == new DateTime() )
				return;


			string where = string.Format( " base.LastUpdated > '{0}'", endDate.ToString( "yyyy-MM-dd" ) );
			filter += ServiceHelper.FormatSearchItem( filter, where, booleanOperator );
			string selDesc = string.Format( " Modified > {0}", endDate.ToString( "yyyy-MM-dd" ) );
			filterDesc = filterDesc + "<div class='searchSection isleBox'>" + selDesc + "</div>";
		}

		public void FormatStatusFilter( ContentSearchQuery query, string booleanOperator, ref string filter, ref string filterDesc )
		{
			string csv = "";
			string selDesc = "";
			string comma = "";

			//==> consider resource.tag approach
			foreach ( ContentSearchFilter li in query.Filters )
			{
				if ( li.Category == "Status" )
				{
					foreach ( string tag in li.Tags )
					{
						if ( tag != "0" )
						{
							csv += tag + ",";
							selDesc += comma + tag; //can we get the text name
							comma = ", ";
						}
					}

				}
			}
			if ( csv.Length > 0 )
			{
				csv = csv.Substring( 0, csv.Length - 1 );

				string where = string.Format( StatusTypeFilter, csv );
				filter += ServiceHelper.FormatSearchItem( filter, where, booleanOperator );
				filterDesc = filterDesc + "<div class='searchSection isleBox'>" + selDesc + "</div>";
			}
		
		}

		public void FormatContentTypeFilter( ContentSearchQuery query, string booleanOperator, ref string filter, ref string filterDesc )
		{
			string csv = "";
			string selDesc = "";
			string comma = "";

			//==> consider resource.tag approach
			foreach ( ContentSearchFilter li in query.Filters )
			{
				if ( li.Category == "ContentType" )
				{
					foreach ( string tag in li.Tags )
					{
						if ( tag != "0" )
						{
							csv += tag + ",";
							selDesc += comma + tag; //can we get the text name
							comma = ", ";
						}
					}

				}
			}
			if ( csv.Length > 0 )
			{
				csv = csv.Substring( 0, csv.Length - 1 );

				string where = string.Format( ContentTypeFilter, csv );
				filter += ServiceHelper.FormatSearchItem( filter, where, booleanOperator );
				filterDesc = filterDesc + "<div class='searchSection isleBox'>" + selDesc + "</div>";
			}
			else
			{
				//typeId may may have to be configurable
				//filter += ServiceHelper.FormatSearchItem( filter, "TypeId in (10, 50)", booleanOperator );
			}
		}

		public void FormatPrivilegeTypeFilter( ContentSearchQuery query, string booleanOperator, Patron user, ref string filter, ref string filterDesc )
		{
			string csv = "";
			string selDesc = "";
			string comma = "";
			bool hasPublicFilter = filter.ToLower().IndexOf( "base.privilegetypeid " ) > 0;
			//==> interface needs to prevent unauth options, like a student

			foreach ( ContentSearchFilter li in query.Filters )
			{
				if ( li.Category == "Privilege" )
				{
					foreach ( string tag in li.Tags )
					{
						if ( tag != "0" )
						{
							csv += tag + ",";
							selDesc += comma + tag; //can we get the text name
							comma = ", ";
						}
					}

				}
			}
			if ( csv.Length > 0 )
			{
				csv = csv.Substring( 0, csv.Length - 1 );

				string where = string.Format( PrivilegeTypeFilter, csv );
				filter += ServiceHelper.FormatSearchItem( filter, where, booleanOperator );
				//filterDesc = filterDesc + "<div class='searchSection isleBox'>" + selDesc + "</div>";
			}
			else
			{
				//should already be covered by the public filter
				//if ( hasPublicFilter == false)
				//	filter += ServiceHelper.FormatSearchItem( filter, "( base.PrivilegeTypeId = 1)", booleanOperator );
			}
		}

		public void FormatStandardsFilter( ContentSearchQuery query, string booleanOperator, ref string filter, ref string filterDesc )
		{
			if ( query.StandardIds == null || query.StandardIds.Count == 0 )
				return;

			string csv = "";
			string selDesc = "";
			string comma = "";
			bool hasPublicFilter = filter.ToLower().IndexOf( "base.privilegetypeid " ) > 0;
			//==> interface needs to prevent unauth options, like a student

			foreach ( int id in query.StandardIds )
			{
				if ( id > 0 )
				{
					csv += id.ToString() + ",";
					selDesc += comma + id.ToString(); //can we get the text name
					comma = ", ";
				}
			}
			if ( csv.Length > 0 )
			{
				csv = csv.Substring( 0, csv.Length - 1 );

				string where = string.Format( StandardsFilter, csv );
				filter += ServiceHelper.FormatSearchItem( filter, where, booleanOperator );
				//filterDesc = filterDesc + "<div class='searchSection isleBox'>" + selDesc + "</div>";
			}
			
		}


		private string FormatApproverFilter( string keyword, Patron user, ref string filterDesc, ref string message )
		{

			string filter = "";
			string booleanOperator = "AND";
			string where = "";
			string selDesc = "";

			if ( user.OrgId > 0 )
			{
				where = string.Format( "(base.StatusId = 3 " +
											"AND ( base.OrgId = {0} OR base.ParentOrgId = {0} ) )  ", user.OrgId );
				selDesc = "Approvals in my district";
			}
			else
			{
				message = "You are not associated with an organization, approver is not possible";
				where = string.Format( "(base.OrgId = {0} OR auth.OrganizationId = {0}) ", -1 );
			}
			if ( where.Trim().Length > 5 )
			{
				filter += ServiceHelper.FormatSearchItem( filter, where, booleanOperator );
				filterDesc = filterDesc + "<div class='searchSection isleBox'>" + selDesc + "</div>";
			}
			else if ( selDesc.Trim().Length > 5 )
			{
				filterDesc = filterDesc + "<div class='searchSection isleBox'>" + selDesc + "</div>";
			}

			FormatKeyword( keyword, booleanOperator, ref filter );

			return filter;
		}	//
		#endregion
	}
}
