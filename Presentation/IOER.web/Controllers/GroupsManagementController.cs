using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using EmailHelper = ILPathways.Utilities.EmailManager;
using MyManager = ILPathways.DAL.GroupManager;
using ILPathways.Business;
using IOER.classes;
using ILPathways.DAL;
using IOER.Library;
using ILPathways.Utilities;
//using bizService = ILPathways.GatewayServiceReference;

namespace IOER.Controllers
{
    public class GroupsManagementController : BaseUserControl
    {
        /// <summary>
        /// Set constant for this control to be used in log messages, etc
        /// </summary>
        const string thisClassName = "GroupsManagementController";

        /// <summary>
        /// Session variable for group-user search parameters
        /// </summary>
        public static string GROUPS_SESSIONVAR_USERSEARCH_PARMS = "group_user_search_parms";

        //bizService.GatewayServicesClient client = new bizService.GatewayServicesClient();

        public GroupsManagementController() 
        {
            //client.Endpoint.Address = new EndpointAddress( ServiceReferenceHelper.GetServicesAddress() + "GatewayServices.svc" );
            //client.AppGroupGetCompleted += new EventHandler<bizService.AppGroupGetCompletedEventArgs>( client_AppGroupGetCompleted );
        }



        //public event EventHandler GetGroupNoComponents;
        //public event EventHandler GetGroupSuccess;
        //public event EventFailureEventHandler GetGroupFailure;
        //private bizService.GroupDataContract _groupDC;
        //public bizService.GroupDataContract GroupContract
        //{
        //    get
        //    {
        //        if ( this._groupDC == null )
        //        {
        //            this._groupDC = new bizService.GroupDataContract();
        //        }
        //        return this._groupDC;
        //    }
        //    set
        //    {
        //        this._groupDC = value;
        //        // this.OnPropertyChanged( "CodesList" );
        //    }
        //}

        #region Group Management Constants
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
                    if ( Session[ "CurrentGroup" ] == null )
                        Session[ "CurrentGroup" ] = new AppGroup();

                    return Session[ "CurrentGroup" ] as AppGroup;
                }
                catch
                {
                    Session[ "CurrentGroup" ] = new AppGroup();
                    return Session[ "CurrentGroup" ] as AppGroup;
                }
            }

            set { Session[ "CurrentGroup" ] = value; }
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
                    if ( Session[ "CurrentGroup_DidParentIdChange" ] == null )
                        Session[ "CurrentGroup_DidParentIdChange" ] = false;

                    return bool.Parse( Session[ "CurrentGroup_DidParentIdChange" ].ToString() );
                }
                catch
                {
                    Session[ "CurrentGroup_DidParentIdChange" ] = false;
                    return false;
                }

            }
            set { Session[ "CurrentGroup_DidParentIdChange" ] = value; }
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
                    if ( Session[ "CurrentGroup_GroupMemberId" ] == null )
                        Session[ "CurrentGroup_GroupMemberId" ] = 0;

                    return Int32.Parse( Session[ "CurrentGroup_GroupMemberId" ].ToString() );
                }
                catch
                {
                    Session[ "CurrentGroup_GroupMemberId" ] = 0;
                    return 0;
                }
            }
            set { Session[ "CurrentGroup_GroupMemberId" ] = value.ToString(); }
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
                    if ( Session[ "CurrentGroup_GroupTeamMemberId" ] == null )
                        Session[ "CurrentGroup_GroupTeamMemberId" ] = 0;

                    return Int32.Parse( Session[ "CurrentGroup_GroupTeamMemberId" ].ToString() );
                }
                catch
                {
                    Session[ "CurrentGroup_GroupTeamMemberId" ] = 0;
                    return 0;
                }

            }
            set { Session[ "CurrentGroup_GroupTeamMemberId" ] = value.ToString(); }
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
                    if ( Session[ "CurrentGroup_ViewIndx" ] == null )
                        Session[ "CurrentGroup_ViewIndx" ] = 0;

                    return Int32.Parse( Session[ "CurrentGroup_ViewIndx" ].ToString() );
                }
                catch
                {
                    Session[ "CurrentGroup_ViewIndx" ] = 0;
                    return 0;
                }
            }
            set { Session[ "CurrentGroup_ViewIndx" ] = value.ToString(); }
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
                    if ( Session[ "CurrentGroup_LastAccordianPane" ] == null )
                        Session[ "CurrentGroup_LastAccordianPane" ] = 0;

                    return Int32.Parse( Session[ "CurrentGroup_LastAccordianPane" ].ToString() );
                }
                catch
                {
                    return -1;
                }
            }
            set { Session[ "CurrentGroup_LastAccordianPane" ] = value.ToString(); }
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

                CurrentGroup = MyManager.Get( id );

            }
            catch ( Exception ex )
            {
                action = false;
                string msg = "Unexpected error encountered while attempting to set a new current group to group id = " + id.ToString() + ". ";
                LoggingHelper.LogError( ex, thisClassName + ".SetCurrentGroup() - " + msg );
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
                entity.LastUpdatedBy = WebUser.FullName();
                entity.LastUpdatedById = WebUser.Id;

                statusMessage = MyManager.Update( entity );

                //refresh group - not sure if necessary here as latter are the only changes
                CurrentGroup = MyManager.Get( entity.Id );
            }
            catch ( Exception ex )
            {
                action = false;

                LoggingHelper.LogError( ex, thisClassName + ".UpdateCurrentGroup() - Unexpected error encountered while attempting to update the current group." );
                statusMessage = "An unexpected error was encountered while attempting to update the current group. System administration has been notified:<br/> " + ex.Message.ToString();
            }
            return action;
        }//

        #endregion


        //#region async services

        //public void GroupGet( int groupId )
        //{
        //    try
        //    {
        //        bizService.GroupGetRequest r = new bizService.GroupGetRequest();
        //        r.Id = groupId;
        //        r.GroupCode = "";

        //        client.AppGroupGetAsync( r );
        //        //client.Close();
        //    }
        //    catch ( Exception ex )
        //    {
        //        LoggingHelper.LogError( ex, thisClassName + ".GroupGet(groupId) " );
        //    }

        //}
        //public void GroupGet( string groupCode )
        //{
        //    try
        //    {
        //        bizService.GroupGetRequest r = new bizService.GroupGetRequest();
        //        r.Id = 0;
        //        r.GroupCode = groupCode;

        //        client.AppGroupGetAsync( r );
        //        //client.Close();
        //    }
        //    catch ( Exception ex )
        //    {
        //        LoggingHelper.LogError( ex, thisClassName + ".GroupGet(groupId) " );
        //    }

        //}
        //void client_AppGroupGetCompleted( object sender, bizService.AppGroupGetCompletedEventArgs e )
        //{
        //    try
        //    {
        //        bizService.GroupGetResponse r = e.Result as bizService.GroupGetResponse;
                
        //        if ( r.Status == bizService.StatusEnumDataContract.Success )
        //        {
        //            this.GroupContract = e.Result.Group;
        //            if ( GroupContract != null && GroupContract.Id > 0 )
        //            {
        //                if ( GetGroupSuccess != null )
        //                    this.GetGroupSuccess( this, null );
        //            }
        //            else
        //            {
        //                if ( GetGroupNoComponents != null )
        //                    this.GetGroupNoComponents( this, null );
        //            }
        //        }
        //        else
        //        {
        //            this.GroupContract = new bizService.GroupDataContract();
        //            EventFailureEventArgs args = new EventFailureEventArgs( r.Error.Message );

        //            if ( GetGroupFailure != null )
        //                this.GetGroupFailure( this, args );
        //        }
        //    }
        //    catch ( Exception ex )
        //    {
        //        EventFailureEventArgs args = new EventFailureEventArgs( ex.Message );
        //        if ( GetGroupFailure != null )
        //            this.GetGroupFailure( this, args );

        //        LoggingHelper.LogError( ex, thisClassName + ".client_AppGroupGetCompleted() " );
        //    }
        //}

        //public AppGroup MapGroup( bizService.GroupDataContract gc )
        //{
        //    AppGroup entity = new AppGroup();
        //    entity.Id = gc.Id;
        //    entity.Title = gc.Title;
        //    entity.Description = gc.Description;
        //    entity.GroupTypeId = gc.GroupTypeId;
        //    entity.GroupType = gc.GroupType;
        //    entity.GroupCode = gc.GroupCode;
        //    entity.OrgId = gc.OrgId;
        //    entity.ContactId = gc.ContactId;
        //    entity.ParentGroupId = gc.ParentGroupId;
        //    entity.IsActive = gc.IsActive;

        //    return entity;
        //}
        //#endregion
    }
}
