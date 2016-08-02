using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

//using EFDAL = GatewayBusinessEntities;\
//using GatewayContext = IoerContentBusinessEntities;
using EFDAL = IoerContentBusinessEntities;
using ILPathways.Business;
using ILPathways.Common;
using ILPathways.DAL;
using Patron = LRWarehouse.Business.Patron;

namespace Isle.BizServices
{
    public class GroupServices : ServiceHelper
    {
		string thisClassName = "GroupServices";
        EFDAL.GatewayContext ctx = new EFDAL.GatewayContext();

		#region Group Management Constants
		/// <summary>
		/// Session variable for group-user search parameters
		/// </summary>
		public static string GROUPS_SESSIONVAR_USERSEARCH_PARMS = "group_user_search_parms";

		public static string CONTROLLER_CURRENT_GROUP = "CurrentGroup";

		public static int CONTROLLER_PANE_SEARCH = 0;
		public static int CONTROLLER_PANE_DETAIL = 1;
		public static int CONTROLLER_PANE_TEAM = 2;
		public static int CONTROLLER_PANE_CUSTOMERS = 2;
		public static int CONTROLLER_PANE_PRIVILEGES = 3;
		public static int CONTROLLER_PANE_CUSTOMER_DETAIL = 4;

		public static string AUTHORIZATION_GROUP_TYPE = "Authorization";

		public static string PERSONAL_GROUP_TYPE = "PersonalGroup";

		#endregion


		#region properties
		string _instanceGroupCode = CONTROLLER_CURRENT_GROUP;
		/// <summary>
		/// Contains code to use for session key for current group instance - set on load by child controls
		/// </summary>
		public string InstanceGroupCode
		{
			get { return _instanceGroupCode; }

			set { _instanceGroupCode = value; }
		}
		/// <summary>
		/// INTERNAL PROPERTY: CurrentRecord
		/// Set initially and store in Session
		/// WARNING - have to be careful if user can open more than one group - future may be just append id to code or even use rowId!
		/// </summary>
		public AppGroup CurrentGroup
		{
			get
			{
				try
				{
					if ( HttpContext.Current.Session[ "CurrentGroup" ] == null )
						HttpContext.Current.Session[ "CurrentGroup" ] = new AppGroup();

					return HttpContext.Current.Session[ "CurrentGroup" ] as AppGroup;
				}
				catch
				{
					HttpContext.Current.Session[ "CurrentGroup" ] = new AppGroup();
					return HttpContext.Current.Session[ "CurrentGroup" ] as AppGroup;
				}
			}

			set { HttpContext.Current.Session[ "CurrentGroup" ] = value; }
		}


		/// <summary>
		/// Store retrieve whether the parent record just changed
		/// </summary>
		public bool DidParentRecordChange
		{
			get
			{
				try
				{
					if ( HttpContext.Current.Session[ "CurrentGroup_DidParentIdChange" ] == null )
						HttpContext.Current.Session[ "CurrentGroup_DidParentIdChange" ] = false;

					return bool.Parse( HttpContext.Current.Session[ "CurrentGroup_DidParentIdChange" ].ToString() );
				}
				catch
				{
					HttpContext.Current.Session[ "CurrentGroup_DidParentIdChange" ] = false;
					return false;
				}

			}
			set { HttpContext.Current.Session[ "CurrentGroup_DidParentIdChange" ] = value; }
		}


		/// <summary>
		/// Get/Set Last selected user from the group member pane
		/// </summary>
		public int LastSelectedGroupMemberId
		{
			get
			{
				try
				{
					if ( HttpContext.Current.Session[ "CurrentGroup_GroupMemberId" ] == null )
						HttpContext.Current.Session[ "CurrentGroup_GroupMemberId" ] = 0;

					return Int32.Parse( HttpContext.Current.Session[ "CurrentGroup_GroupMemberId" ].ToString() );
				}
				catch
				{
					HttpContext.Current.Session[ "CurrentGroup_GroupMemberId" ] = 0;
					return 0;
				}
			}
			set { HttpContext.Current.Session[ "CurrentGroup_GroupMemberId" ] = value.ToString(); }
		}

		/// <summary>
		/// Get/Set Last selected user from the group team member pane
		/// </summary>
		public int LastSelectedGroupTeamMemberId
		{
			get
			{
				try
				{
					if ( HttpContext.Current.Session[ "CurrentGroup_GroupTeamMemberId" ] == null )
						HttpContext.Current.Session[ "CurrentGroup_GroupTeamMemberId" ] = 0;

					return Int32.Parse( HttpContext.Current.Session[ "CurrentGroup_GroupTeamMemberId" ].ToString() );
				}
				catch
				{
					HttpContext.Current.Session[ "CurrentGroup_GroupTeamMemberId" ] = 0;
					return 0;
				}

			}
			set { HttpContext.Current.Session[ "CurrentGroup_GroupTeamMemberId" ] = value.ToString(); }
		}

		/// <summary>
		/// Store retrieve the last containter (ex accordion pane, or a tab index)
		/// </summary>
		public int LastActiveContainerIdx
		{
			get
			{
				try
				{
					if ( HttpContext.Current.Session[ "CurrentGroup_ViewIndx" ] == null )
						HttpContext.Current.Session[ "CurrentGroup_ViewIndx" ] = 0;

					return Int32.Parse( HttpContext.Current.Session[ "CurrentGroup_ViewIndx" ].ToString() );
				}
				catch
				{
					HttpContext.Current.Session[ "CurrentGroup_ViewIndx" ] = 0;
					return 0;
				}
			}
			set { HttpContext.Current.Session[ "CurrentGroup_ViewIndx" ] = value.ToString(); }
		}


		/// <summary>
		/// Store retrieve the last active pane index
		/// </summary>
		public int LastActiveContainer
		{
			get
			{
				try
				{
					if ( HttpContext.Current.Session[ "CurrentGroup_LastAccordianPane" ] == null )
						HttpContext.Current.Session[ "CurrentGroup_LastAccordianPane" ] = 0;

					return Int32.Parse( HttpContext.Current.Session[ "CurrentGroup_LastAccordianPane" ].ToString() );
				}
				catch
				{
					return -1;
				}
			}
			set { HttpContext.Current.Session[ "CurrentGroup_LastAccordianPane" ] = value.ToString(); }
		}


		#endregion
		#region General group managment stuff
		/// <summary>
		/// struct for transporting user search parameters between a group and a search control
		/// - Should instantiate with the new operator to endure all properties are initialized
		/// </summary>
		[Serializable]
		public struct GroupUserSearchParameters
		{
			public string searchTitle;
			public string groupId;
			public string roleId;
			public string childEntityType;
			public string searchLwia;
			public bool isSYEPSearch;
			public bool isSYEP_ReturningSearch;
			public bool isElevateAmericaSearch;
			public bool showCloseButton;
			public string emailNoticeCode;
			public string programCode;
		}

		#endregion
		#region Groups
		public static bool Delete( int id, ref string statusMessage )
        {
            return GroupManager.Delete( id, ref statusMessage );
        }//
        public static int Create( AppGroup entity, ref string statusMessage )
        {
            return GroupManager.Create( entity, ref statusMessage);
        }
        public static string Update( AppGroup entity )
        {
            return GroupManager.Update( entity );
        }
        public static AppGroup Get( int id)
        {
            return GroupManager.Get( id );
        }//

        public static AppGroup GetByCode( string groupCode )
        {
            return GroupManager.GetByCode( groupCode );
        }//

         public static DataSet Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            return GroupManager.Search( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
        }
        #endregion

		 #region Current Groups methods - use specific codes as can be multiple group related pages active in same session

		 /// <summary>
		 /// Check if a group of requrest type has been saved in session
		 /// </summary>
		 /// <returns>True if there is a group in the session</returns>
		 public bool CurrentGroupExists()
		 {
			 bool action = false;

			 try
			 {
				 if ( CurrentGroup != null && CurrentGroup.Id > 0 )
				 {
					 action = true;
				 }
				 else
				 {
					 if ( IsTestEnv() )
					 {
						 //populate a default current group to allow component testing
						 //get manager
						 //MyManager myManager = new MyManager();
						 //CurrentGroup = myManager.GetLowestGroup();
						 //if ( CurrentGroup != null && CurrentGroup.Id > 0 )
						 //{
						 //  action = true;
						 //}
					 }

				 }
			 }
			 catch
			 {

			 }

			 return action;
		 } //

		 /// <summary>
		 /// Get current group from session
		 /// </summary>
		 /// <returns></returns>
		 public AppGroup GetCurrentGroup()
		 {
			 AppGroup entity = new AppGroup();
			 try
			 {
				 if ( CurrentGroup == null )
				 {
					 //set some defaults??
					 entity.Title = "Missing";

					 //initialize session
					 CurrentGroup = entity;

				 }
				 else
				 {
					 entity = CurrentGroup;
				 }
			 }
			 catch
			 {

			 }

			 return entity;
		 } //


		 /// <summary>
		 /// Set a new current group 
		 /// - ensures we have any updates and access to columns that are not part of the interface
		 /// </summary>
		 /// <returns></returns>
		 public bool SetCurrentGroup( int id, ref string statusMessage )
		 {
			 CurrentGroup = new AppGroup();
			 bool action = true;

			 try
			 {
				 //GroupManager manager = new GroupManager();

				 CurrentGroup = Get( id );

			 }
			 catch ( Exception ex )
			 {
				 action = false;
				 string msg = "Unexpected error encountered while attempting to set a new current group to group id = " + id.ToString() + ". ";
				 LogError( ex, thisClassName + ".SetCurrentGroup() - " + msg );
				 statusMessage = msg + "<br/>System administration has been notified:<br/> " + ex.Message.ToString();
			 }

			 return action;
		 } //

		 /// <summary>
		 /// Update the current group
		 /// </summary>
		 /// <param name="entity"></param>
		 /// <returns></returns>
		 public bool UpdateCurrentGroup( AppGroup entity, ref string statusMessage )
		 {
			 bool action = true;

			 try
			 {
				 //update the group with the orgId
				 //GroupManager manager = new GroupManager();

				 entity.LastUpdated = System.DateTime.Now;
				 //entity.LastUpdatedBy = WebUser.FullName();
				 //entity.LastUpdatedById = WebUser.Id;

				 statusMessage = Update( entity );

				 //refresh group - not sure if necessary here as latter are the only changes
				 CurrentGroup = Get( entity.Id );
			 }
			 catch ( Exception ex )
			 {
				 action = false;

				 LogError( ex, thisClassName + ".UpdateCurrentGroup() - Unexpected error encountered while attempting to update the current group." );
				 statusMessage = "An unexpected error was encountered while attempting to update the current group. System administration has been notified:<br/> " + ex.Message.ToString();
			 }
			 return action;
		 }//

		 #endregion


        #region Group Members
        public static bool GroupMember_Delete( int id, ref string statusMessage )
        {
            return GroupMemberManager.Delete( id, ref statusMessage );
        }//

        public static int GroupMember_Create( GroupMember entity, ref string statusMessage )
        {
            return GroupMemberManager.Create( entity, ref statusMessage );
        }//
        public static DataSet GroupMember_Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            return GroupMemberManager.Search( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
        }//


        #endregion
        #region Org Group Members
        public static bool GroupOrgMember_Delete( int id, ref string statusMessage )
        {
            bool action = false;
            EFDAL.GatewayContext ctx = new EFDAL.GatewayContext();
            EFDAL.AppGroup_OrgMember entity = ctx.AppGroup_OrgMember.SingleOrDefault( s => s.ID == id );
            if ( entity != null )
            {
                ctx.AppGroup_OrgMember.Remove( entity );
                ctx.SaveChanges();
                action = true;
            }
            return action;
        }//

        public static int GroupOrgMember_Create( GroupOrgMember gom, ref string statusMessage )
        {
            EFDAL.GatewayContext ctx = new EFDAL.GatewayContext();

            //entity = OrganizationMember_FromMap( gom );
            EFDAL.AppGroup_OrgMember to = new EFDAL.AppGroup_OrgMember();
            to.GroupId = gom.GroupId;
            to.OrgId = gom.OrgId;
            to.IsActive = true;

            to.LastUpdatedById = gom.CreatedById;
            to.Created = System.DateTime.Now;
            to.LastUpdatedById = gom.CreatedById;
            to.LastUpdated = System.DateTime.Now;
            to.RowId = Guid.NewGuid();
 
            ctx.AppGroup_OrgMember.Add( to );

            // submit the change to database
            int count = ctx.SaveChanges();
            if ( count > 0 )
            {
                statusMessage = "Successful";
                return to.ID;
            }
            else
            {
                statusMessage = "Error - GroupOrgMember_Create failed";
                //?no info on error
                return 0;
            }
        }//

        public static DataSet GroupOrgMember_Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            return GroupMemberManager.GroupOrgMbrSearch( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
        }//


        #endregion

        #region Privileges
        public static ApplicationRolePrivilege GetGroupObjectPrivileges( IWebUser currentUser, string pObjectName )
        {
            return SecurityManager.GetGroupObjectPrivileges( currentUser, pObjectName );
        }//


        public static ApplicationRolePrivilege GetGroupObjectPrivileges( AppUser currentUser, string pObjectName )
        {
            return SecurityManager.GetGroupObjectPrivileges( currentUser, pObjectName );
        }//
        #endregion

        #region organization group authorization ==> Temp

        /// <summary>
        /// retrieve list of approvers for an org
        /// </summary>
        /// <param name="pOrgId"></param>
        /// <returns></returns>
        public static List<GroupMember> OrgApproversSelect( int pOrgId )
        {
            return GroupMemberManager.OrgApproversSelect( pOrgId );
        }//

        /// <summary>
        /// return true, if user can approve the passed orgId
        /// </summary>
        /// <param name="pOrgId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool IsUserAnOrgApprover( int pOrgId, int userId )
        {
            return GroupMemberManager.IsUserAnOrgApprover( pOrgId, userId );
        }//

        public static bool IsUserAnyOrgApprover( int userId )
        {
            string code = "OrgApprovers";
            return GroupMemberManager.IsAGroupMember( code, userId );
        }//

        /// <summary>
        /// TODO - list of all orgs where user is an approver - useful for searches
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static List<Organization> ApproverOrgsSelect( int userId )
        {
            List<Organization> list = new List<Organization>();
            return list;    //GroupMemberManager.OrgApproversSelect( userId );
        }//
        #endregion


        #region Codes
        public static List<CodeItem> GroupTypeCodes_Select()
        {
			return EFDAL.GroupsManager.Codes_GroupType_Get();   
        }//

		public static DataSet CodesGroupType_Select()
		{
			DataSet ds = DatabaseManager.DoQuery("SELECT [Id],[Title]  FROM [Gateway].[dbo].[Codes.GroupType] order by [Title]");
			return ds;    //GroupMemberManager.OrgApproversSelect( userId );
		}//
        #endregion
    }
}
