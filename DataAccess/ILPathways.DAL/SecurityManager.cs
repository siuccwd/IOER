using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Resources;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.UI;
using Microsoft.ApplicationBlocks.Data;
using ILPathways.Business;
//using LRWarehouse.Business;

namespace ILPathways.DAL
{
    public class SecurityManager : BaseDataManager
	{
		static string className = "SecurityManager";

		#region Constants for database procedures
		// Gets role-object privileges
		const string ApplicationRolePrivilegeSelect_PROC = "ApplicationRolePrivilegeSelect";
		// Gets role-object privileges
		const string ApplicationRoleSelect_PROC = "ApplicationRoleSelect";

		#endregion
		#region Constants for Security

		/// <summary>
		/// Channel Privilege Constants
		/// MCMS Property: channelVisibility
		/// </summary>
		public const string AUTHENTICATED_CHANNEL = "authenticated";
		public const string AUTHORIZED_CHANNEL = "authorized";
		public const string PRIVILEGED_CHANNEL = "privileged";

		#endregion

		public SecurityManager()
		{ } //
        #region App Group


        #endregion
        #region App Group Privileges
        /// Delete an GroupMember record
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public static bool AppGroupMemberDelete( int pId, ref string statusMessage )
        {
            string connectionString = GatewayConnection();
            bool successful = false;

            SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
            sqlParameters[ 0 ] = new SqlParameter( "@id", SqlDbType.Int );
            sqlParameters[ 0 ].Value = pId;

            try
            {
                SqlHelper.ExecuteNonQuery( connectionString, CommandType.StoredProcedure, "[AppGroup.MemberDelete]", sqlParameters );
                successful = true;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".Delete() " );
                statusMessage = className + "- Unsuccessful: Delete(): " + ex.Message.ToString();

                successful = false;
            }
            return successful;
        }//

        /// <summary>
        /// Add an GroupMember record - first use the group code to lookup the groupId
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="groupCode"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public static int AppGroupMemberCreate( GroupMember entity, string groupCode, ref string statusMessage )
        {

            AppGroup group = GatewayManager.AppGroupGetByCode( groupCode );
            if ( group == null || group.Id == 0 )
            {
                //??
                statusMessage = className + "- Unsuccessful: Create(): Error the related Group was not found (using a group code of: " + groupCode + ")";
                return 0;
            }
            else
            {
                entity.GroupId = group.Id;
                return AppGroupMemberCreate( entity, ref statusMessage );
            }
        }//

        /// <summary>
        /// Add an GroupMember record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public static int AppGroupMemberCreate( GroupMember entity, ref string statusMessage )
        {
            return GroupMemberManager.Create( entity, ref statusMessage );
           
        }

        /// <summary>
        /// Add an WorkNetGroupPrivilege record
        /// </summary>
        /// <param name="entity">WorkNetGroupPrivilege</param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public static int ApplicationGroupPrivilegeCreate( ApplicationGroupPrivilege entity, ref string statusMessage )
        {
            string connectionString = GatewayConnection();
            int newId = 0;

            #region parameters
            SqlParameter[] sqlParameters = new SqlParameter[ 11 ];
            sqlParameters[ 0 ] = new SqlParameter( "@GroupId", SqlDbType.Int );
            sqlParameters[ 0 ].Value = entity.GroupId;

            sqlParameters[ 1 ] = new SqlParameter( "@ObjectId", SqlDbType.Int );
            sqlParameters[ 1 ].Value = entity.ObjectId;

            sqlParameters[ 2 ] = new SqlParameter( "@CreatePrivilege", SqlDbType.Int );
            sqlParameters[ 2 ].Value = entity.CreatePrivilege;

            sqlParameters[ 3 ] = new SqlParameter( "@ReadPrivilege", SqlDbType.Int );
            sqlParameters[ 3 ].Value = entity.ReadPrivilege;

            sqlParameters[ 4 ] = new SqlParameter( "@WritePrivilege", SqlDbType.Int );
            sqlParameters[ 4 ].Value = entity.WritePrivilege;

            sqlParameters[ 5 ] = new SqlParameter( "@DeletePrivilege", SqlDbType.Int );
            sqlParameters[ 5 ].Value = entity.DeletePrivilege;

            sqlParameters[ 6 ] = new SqlParameter( "@AppendPrivilege", SqlDbType.Int );
            sqlParameters[ 6 ].Value = entity.AppendPrivilege;

            sqlParameters[ 7 ] = new SqlParameter( "@AppendToPrivilege", SqlDbType.Int );
            sqlParameters[ 7 ].Value = entity.AppendToPrivilege;

            sqlParameters[ 8 ] = new SqlParameter( "@AssignPrivilege", SqlDbType.Int );
            sqlParameters[ 8 ].Value = entity.AssignPrivilege;

            sqlParameters[ 9 ] = new SqlParameter( "@ApprovePrivilege", SqlDbType.Int );
            sqlParameters[ 9 ].Value = entity.ApprovePrivilege;

            sqlParameters[ 10 ] = new SqlParameter( "@SharePrivilege", SqlDbType.Int );
            sqlParameters[ 10 ].Value = entity.SharePrivilege;

            #endregion

            try
            {
                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, CommandType.StoredProcedure, "[AppGroup.PrivilegeInsert]", sqlParameters );
                if ( dr.HasRows )
                {
                    dr.Read();
                    newId = int.Parse( dr[ 0 ].ToString() );
                }
                dr.Close();
                dr = null;
                statusMessage = "successful";

            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".Create() " );
                statusMessage = className + "- Unsuccessful: Create(): " + ex.Message.ToString();
                entity.Message = statusMessage;
                entity.IsValid = false;
            }

            return newId;
        }

        #endregion


        #region Get privileges by group, userId
        /// <summary>
        /// Retrieve the privileges for the provided user (actually their highest role) and an object. If the object name is blank, default privileges will be returned
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="pObjectName">Control name, or channel, etc.</param>
        /// <returns></returns>
        public static ApplicationRolePrivilege GetGroupObjectPrivileges( IWebUser currentUser, string pObjectName )
        {
            ApplicationRolePrivilege entity = new ApplicationRolePrivilege();
            if ( currentUser == null )
                return entity;

            try
            {
                SqlConnection sqlConnection = new SqlConnection( GatewayConnectionRO() );
                sqlConnection.Open();

                entity = GetGroupObjectPrivileges( sqlConnection, currentUser.Id, pObjectName );

                //If privileges were found, RoleId is set to GroupId
                if ( entity.RoleId == 0 )
                {
                    if ( currentUser.OrgId > 0 )
                        entity = GetGroupOrgObjectPrivileges( sqlConnection, currentUser.OrgId, pObjectName );
                }
                sqlConnection.Close();
                sqlConnection.Dispose();
                sqlConnection = null;
                return entity;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".GetGroupObjectPrivileges(int id, string objectName: " + pObjectName + ") " );
                return entity;
            }
        }//

        /// <summary>
        /// Retrieve the privileges for the provided user (actually their highest role) and an object. If the object name is blank, default privileges will be returned
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="pObjectName">Control name, or channel, etc.</param>
        /// <returns></returns>
        public static ApplicationRolePrivilege GetGroupObjectPrivileges( AppUser currentUser, string pObjectName )
        {
            ApplicationRolePrivilege entity = new ApplicationRolePrivilege();
            if ( currentUser == null )
                return entity;

            try
            {
                SqlConnection sqlConnection = new SqlConnection( GatewayConnectionRO() );
                sqlConnection.Open();

                entity = GetGroupObjectPrivileges( sqlConnection, currentUser.Id, pObjectName );

                if ( entity.RoleId == 0 )
                {
                    if (currentUser.OrgId > 0)
                        entity = GetGroupOrgObjectPrivileges( sqlConnection, currentUser.OrgId, pObjectName );
                }
                sqlConnection.Close();
                sqlConnection.Dispose();
                sqlConnection = null;
                return entity;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".GetGroupObjectPrivileges(int id, string objectName: " + pObjectName + ") " );
                return entity;
            }
		}//


		/// <summary>
		/// Get Group Object Privileges
		/// </summary>
		/// <param name="connectionString">database connection string - passed in to allow flexibility between edit and production environments</param>
		/// <param name="userId">PK to vos_user for passed connection</param>
		/// <param name="objectName">Name of object to check</param>
		/// <returns></returns>
		public static ApplicationRolePrivilege GetGroupObjectPrivileges( string connectionString, int userId, string objectName )
		{
			SqlConnection sqlConnection = new SqlConnection( connectionString );
			sqlConnection.Open();
			ApplicationRolePrivilege entity = GetGroupObjectPrivileges( sqlConnection, userId, objectName );

			sqlConnection.Close();
			sqlConnection.Dispose();
			sqlConnection = null;
			return entity;
		}

		/// <summary>
		/// Get Group Object Privileges
		/// </summary>
		/// <param name="connectionString">database connection string - passed in to allow flexibility between edit and production environments</param>
		/// <param name="userId">PK to vos_user for passed connection</param>
		/// <param name="objectName">Name of object to check</param>
		/// <returns></returns>
		public static ApplicationRolePrivilege GetGroupObjectPrivileges( SqlConnection sqlConnection, int userId, string objectName )
		{

			ApplicationRolePrivilege entity = new ApplicationRolePrivilege();
            if ( userId == 0 )
                return entity;
			entity.CanView = false;
			bool firstRow = true;

			try
			{
				//determine if user is in a group with access to the object 


				SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
				sqlParameters[ 0 ] = new SqlParameter( "@UserId", userId );
				sqlParameters[ 1 ] = new SqlParameter( "@ObjectName", objectName );

                SqlDataReader drdr = SqlHelper.ExecuteReader( sqlConnection, "AppObject_Group_UserPrivileges_Select", sqlParameters );

				if ( drdr.HasRows )
				{
					while ( drdr.Read() )
					{
						if ( firstRow )
						{
							entity.RoleId = GetRowColumn( drdr, "groupId", 0 );
							entity.SubObjectId = 0;	// GetRowColumn(drdr, "SubObjectId", 0);

							entity.SubObjectName = "";	// GetRowColumn(drdr, "SubObjectName", "");
							entity.Description = GetRowColumn( drdr, "Description", "" );

							entity.Sequence = 1;	// GetRowColumn( drdr, "sequence", 0 );
							entity.CreatePrivilege = GetRowColumn( drdr, "createPrivilege", 0 );
							entity.ReadPrivilege = GetRowColumn( drdr, "readPrivilege", 0 );
							entity.WritePrivilege = GetRowColumn( drdr, "writePrivilege", 0 );
							entity.DeletePrivilege = GetRowColumn( drdr, "deletePrivilege", 0 );
							entity.AppendPrivilege = GetRowColumn( drdr, "appendPrivilege", 0 );
							entity.AppendToPrivilege = GetRowColumn( drdr, "appendToPrivilege", 0 );
							entity.AssignPrivilege = GetRowColumn( drdr, "assignPrivilege", 0 );
							entity.ApprovePrivilege = GetRowColumn( drdr, "approvePrivilege", 0 );
							entity.SharePrivilege = GetRowColumn( drdr, "sharePrivilege", 0 );
						} else
						{
							//check if next row has higher privileges
							entity.HasChanged = false;
							if ( entity.CreatePrivilege < GetRowColumn( drdr, "createPrivilege", 0 ) )
								entity.CreatePrivilege = GetRowColumn( drdr, "createPrivilege", 0 );

							if ( entity.ReadPrivilege < GetRowColumn( drdr, "readPrivilege", 0 ) )
								entity.ReadPrivilege = GetRowColumn( drdr, "readPrivilege", 0 );

							if ( entity.WritePrivilege < GetRowColumn( drdr, "writePrivilege", 0 ) )
								entity.WritePrivilege = GetRowColumn( drdr, "writePrivilege", 0 );

							if ( entity.DeletePrivilege < GetRowColumn( drdr, "deletePrivilege", 0 ) )
								entity.DeletePrivilege = GetRowColumn( drdr, "deletePrivilege", 0 );

							if ( entity.AppendPrivilege < GetRowColumn( drdr, "appendPrivilege", 0 ) )
								entity.AppendPrivilege = GetRowColumn( drdr, "appendPrivilege", 0 );

							if ( entity.AppendToPrivilege < GetRowColumn( drdr, "appendToPrivilege", 0 ) )
								entity.AppendToPrivilege = GetRowColumn( drdr, "appendToPrivilege", 0 );

							if ( entity.AssignPrivilege < GetRowColumn( drdr, "assignPrivilege", 0 ) )
								entity.AssignPrivilege = GetRowColumn( drdr, "assignPrivilege", 0 );

							if ( entity.ApprovePrivilege < GetRowColumn( drdr, "approvePrivilege", 0 ) )
								entity.ApprovePrivilege = GetRowColumn( drdr, "approvePrivilege", 0 );

							if ( entity.SharePrivilege < GetRowColumn( drdr, "sharePrivilege", 0 ) )
								entity.SharePrivilege = GetRowColumn( drdr, "sharePrivilege", 0 );

							if ( entity.HasChanged )
							{
								entity.RoleId = GetRowColumn( drdr, "groupId", 0 );
								entity.Description = GetRowColumn( drdr, "Description", "" );

								entity.Sequence = GetRowColumn( drdr, "sequence", 0 );
							}
						}

						//break;
					}
				}
				drdr.Close();
				drdr = null;
				return entity;

			} catch ( Exception e )
			{
				LogError( className + ".GetGroupObjectPrivileges(SqlConnection sqlConnection, int userId, string objectName: " + objectName + "): " + e.ToString() );
				return entity;
			}

		}//

        /// <summary>
        /// Future - ma be more flexible to use rowId???
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="userRowId"></param>
        /// <param name="objectName"></param>
        /// <returns></returns>
        public static ApplicationRolePrivilege GetGroupObjectPrivileges( SqlConnection sqlConnection, string userRowId, string objectName )
        {

            ApplicationRolePrivilege entity = new ApplicationRolePrivilege();
            if ( userRowId.Trim().Length == 0 )
                return entity;
            entity.CanView = false;
            bool firstRow = true;

            try
            {
                //determine if user is in a group with access to the object 


                SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                sqlParameters[ 0 ] = new SqlParameter( "@UserId", userRowId );
                sqlParameters[ 1 ] = new SqlParameter( "@ObjectName", objectName );

                SqlDataReader drdr = SqlHelper.ExecuteReader( sqlConnection, "AppObject_Group_UserRowIdPrivileges_Select", sqlParameters );

                if ( drdr.HasRows )
                {
                    while ( drdr.Read() )
                    {
                        if ( firstRow )
                        {
                            entity.RoleId = GetRowColumn( drdr, "groupId", 0 );
                            entity.SubObjectId = 0;	// GetRowColumn(drdr, "SubObjectId", 0);

                            entity.SubObjectName = "";	// GetRowColumn(drdr, "SubObjectName", "");
                            entity.Description = GetRowColumn( drdr, "Description", "" );

                            entity.Sequence = 1;	// GetRowColumn( drdr, "sequence", 0 );
                            entity.CreatePrivilege = GetRowColumn( drdr, "createPrivilege", 0 );
                            entity.ReadPrivilege = GetRowColumn( drdr, "readPrivilege", 0 );
                            entity.WritePrivilege = GetRowColumn( drdr, "writePrivilege", 0 );
                            entity.DeletePrivilege = GetRowColumn( drdr, "deletePrivilege", 0 );
                            entity.AppendPrivilege = GetRowColumn( drdr, "appendPrivilege", 0 );
                            entity.AppendToPrivilege = GetRowColumn( drdr, "appendToPrivilege", 0 );
                            entity.AssignPrivilege = GetRowColumn( drdr, "assignPrivilege", 0 );
                            entity.ApprovePrivilege = GetRowColumn( drdr, "approvePrivilege", 0 );
                            entity.SharePrivilege = GetRowColumn( drdr, "sharePrivilege", 0 );
                        }
                        else
                        {
                            //check if next row has higher privileges
                            entity.HasChanged = false;
                            if ( entity.CreatePrivilege < GetRowColumn( drdr, "createPrivilege", 0 ) )
                                entity.CreatePrivilege = GetRowColumn( drdr, "createPrivilege", 0 );

                            if ( entity.ReadPrivilege < GetRowColumn( drdr, "readPrivilege", 0 ) )
                                entity.ReadPrivilege = GetRowColumn( drdr, "readPrivilege", 0 );

                            if ( entity.WritePrivilege < GetRowColumn( drdr, "writePrivilege", 0 ) )
                                entity.WritePrivilege = GetRowColumn( drdr, "writePrivilege", 0 );

                            if ( entity.DeletePrivilege < GetRowColumn( drdr, "deletePrivilege", 0 ) )
                                entity.DeletePrivilege = GetRowColumn( drdr, "deletePrivilege", 0 );

                            if ( entity.AppendPrivilege < GetRowColumn( drdr, "appendPrivilege", 0 ) )
                                entity.AppendPrivilege = GetRowColumn( drdr, "appendPrivilege", 0 );

                            if ( entity.AppendToPrivilege < GetRowColumn( drdr, "appendToPrivilege", 0 ) )
                                entity.AppendToPrivilege = GetRowColumn( drdr, "appendToPrivilege", 0 );

                            if ( entity.AssignPrivilege < GetRowColumn( drdr, "assignPrivilege", 0 ) )
                                entity.AssignPrivilege = GetRowColumn( drdr, "assignPrivilege", 0 );

                            if ( entity.ApprovePrivilege < GetRowColumn( drdr, "approvePrivilege", 0 ) )
                                entity.ApprovePrivilege = GetRowColumn( drdr, "approvePrivilege", 0 );

                            if ( entity.SharePrivilege < GetRowColumn( drdr, "sharePrivilege", 0 ) )
                                entity.SharePrivilege = GetRowColumn( drdr, "sharePrivilege", 0 );

                            if ( entity.HasChanged )
                            {
                                entity.RoleId = GetRowColumn( drdr, "groupId", 0 );
                                entity.Description = GetRowColumn( drdr, "Description", "" );

                                entity.Sequence = GetRowColumn( drdr, "sequence", 0 );
                            }
                        }

                        //break;
                    }
                }
                drdr.Close();
                drdr = null;
                return entity;

            }
            catch ( Exception e )
            {
                LogError( className + ".GetGroupObjectPrivileges(SqlConnection sqlConnection, int userId, string objectName: " + objectName + "): " + e.ToString() );
                return entity;
            }

        }//

		#endregion

        #region Get privileges by group, organization
        /// <summary>
        /// Retrieve the privileges for the provided user (actually their highest role) and an object. If the object name is blank, default privileges will be returned
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="pObjectName">Control name, or channel, etc.</param>
        /// <returns></returns>
        public static ApplicationRolePrivilege GetGroupOrgObjectPrivileges( Organization org, string pObjectName )
        {
            ApplicationRolePrivilege entity = new ApplicationRolePrivilege();
            if ( org == null )
                return entity;

            try
            {
                SqlConnection sqlConnection = new SqlConnection( GatewayConnectionRO() );
                sqlConnection.Open();

                entity = GetGroupOrgObjectPrivileges( sqlConnection, org.Id, pObjectName );
                sqlConnection.Close();
                sqlConnection.Dispose();
                sqlConnection = null;
                return entity;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".GetGroupOrgObjectPrivileges(Organization org, string objectName: " + pObjectName + ") " );
                return entity;
            }
        }//

        /// <summary>
        /// Get Group Object Privileges
        /// </summary>
        /// <param name="connectionString">database connection string - passed in to allow flexibility between edit and production environments</param>
        /// <param name="userId">PK to vos_user for passed connection</param>
        /// <param name="objectName">Name of object to check</param>
        /// <returns></returns>
        public static ApplicationRolePrivilege GetGroupOrgObjectPrivileges( int orgId, string pObjectName )
        {
            ApplicationRolePrivilege entity = new ApplicationRolePrivilege();
            if ( orgId == 0 )
                return entity;

            try
            {
                SqlConnection sqlConnection = new SqlConnection( GatewayConnectionRO() );
                sqlConnection.Open();

                entity = GetGroupOrgObjectPrivileges( sqlConnection, orgId, pObjectName );
                sqlConnection.Close();
                sqlConnection.Dispose();
                sqlConnection = null;
                return entity;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".GetGroupOrgObjectPrivileges(IWebUser user, string objectName: " + pObjectName + ") " );
                return entity;
            }
        }

        /// <summary>
        /// Get Group Object Privileges
        /// </summary>
        /// <param name="connectionString">database connection string - passed in to allow flexibility between edit and production environments</param>
        /// <param name="userId">PK to vos_user for passed connection</param>
        /// <param name="objectName">Name of object to check</param>
        /// <returns></returns>
        public static ApplicationRolePrivilege GetGroupOrgObjectPrivileges( string connectionString, int orgId, string objectName )
        {
            SqlConnection sqlConnection = new SqlConnection( connectionString );
            sqlConnection.Open();
            ApplicationRolePrivilege entity = GetGroupOrgObjectPrivileges( sqlConnection, orgId, objectName );

            sqlConnection.Close();
            sqlConnection.Dispose();
            sqlConnection = null;
            return entity;
        }

        /// <summary>
        /// Get Group Object Privileges
        /// </summary>
        /// <param name="connectionString">database connection string - passed in to allow flexibility between edit and production environments</param>
        /// <param name="userId">PK to vos_user for passed connection</param>
        /// <param name="objectName">Name of object to check</param>
        /// <returns></returns>
        public static ApplicationRolePrivilege GetGroupOrgObjectPrivileges( SqlConnection sqlConnection, int orgId, string objectName )
        {

            ApplicationRolePrivilege entity = new ApplicationRolePrivilege();
            if ( orgId == 0 )
                return entity;
            entity.CanView = false;
            bool firstRow = true;

            try
            {
                //determine if user is in a group with access to the object 


                SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                sqlParameters[ 0 ] = new SqlParameter( "@OrgId", orgId );
                sqlParameters[ 1 ] = new SqlParameter( "@ObjectName", objectName );

                SqlDataReader drdr = SqlHelper.ExecuteReader( sqlConnection, "AppObject_Group_OrgPrivileges_Select", sqlParameters );

                if ( drdr.HasRows )
                {
                    while ( drdr.Read() )
                    {
                        if ( firstRow )
                        {
                            entity.RoleId = GetRowColumn( drdr, "groupId", 0 );
                            entity.SubObjectId = 0;	// GetRowColumn(drdr, "SubObjectId", 0);

                            entity.SubObjectName = "";	// GetRowColumn(drdr, "SubObjectName", "");
                            entity.Description = GetRowColumn( drdr, "Description", "" );

                            entity.Sequence = 1;	// GetRowColumn( drdr, "sequence", 0 );
                            entity.CreatePrivilege = GetRowColumn( drdr, "createPrivilege", 0 );
                            entity.ReadPrivilege = GetRowColumn( drdr, "readPrivilege", 0 );
                            entity.WritePrivilege = GetRowColumn( drdr, "writePrivilege", 0 );
                            entity.DeletePrivilege = GetRowColumn( drdr, "deletePrivilege", 0 );
                            entity.AppendPrivilege = GetRowColumn( drdr, "appendPrivilege", 0 );
                            entity.AppendToPrivilege = GetRowColumn( drdr, "appendToPrivilege", 0 );
                            entity.AssignPrivilege = GetRowColumn( drdr, "assignPrivilege", 0 );
                            entity.ApprovePrivilege = GetRowColumn( drdr, "approvePrivilege", 0 );
                            entity.SharePrivilege = GetRowColumn( drdr, "sharePrivilege", 0 );
                        }
                        else
                        {
                            //check if next row has higher privileges
                            entity.HasChanged = false;
                            if ( entity.CreatePrivilege < GetRowColumn( drdr, "createPrivilege", 0 ) )
                                entity.CreatePrivilege = GetRowColumn( drdr, "createPrivilege", 0 );

                            if ( entity.ReadPrivilege < GetRowColumn( drdr, "readPrivilege", 0 ) )
                                entity.ReadPrivilege = GetRowColumn( drdr, "readPrivilege", 0 );

                            if ( entity.WritePrivilege < GetRowColumn( drdr, "writePrivilege", 0 ) )
                                entity.WritePrivilege = GetRowColumn( drdr, "writePrivilege", 0 );

                            if ( entity.DeletePrivilege < GetRowColumn( drdr, "deletePrivilege", 0 ) )
                                entity.DeletePrivilege = GetRowColumn( drdr, "deletePrivilege", 0 );

                            if ( entity.AppendPrivilege < GetRowColumn( drdr, "appendPrivilege", 0 ) )
                                entity.AppendPrivilege = GetRowColumn( drdr, "appendPrivilege", 0 );

                            if ( entity.AppendToPrivilege < GetRowColumn( drdr, "appendToPrivilege", 0 ) )
                                entity.AppendToPrivilege = GetRowColumn( drdr, "appendToPrivilege", 0 );

                            if ( entity.AssignPrivilege < GetRowColumn( drdr, "assignPrivilege", 0 ) )
                                entity.AssignPrivilege = GetRowColumn( drdr, "assignPrivilege", 0 );

                            if ( entity.ApprovePrivilege < GetRowColumn( drdr, "approvePrivilege", 0 ) )
                                entity.ApprovePrivilege = GetRowColumn( drdr, "approvePrivilege", 0 );

                            if ( entity.SharePrivilege < GetRowColumn( drdr, "sharePrivilege", 0 ) )
                                entity.SharePrivilege = GetRowColumn( drdr, "sharePrivilege", 0 );

                            if ( entity.HasChanged )
                            {
                                entity.RoleId = GetRowColumn( drdr, "groupId", 0 );
                                entity.Description = GetRowColumn( drdr, "Description", "" );

                                entity.Sequence = GetRowColumn( drdr, "sequence", 0 );
                            }
                        }

                        //break;
                    }
                }
                drdr.Close();
                drdr = null;
                return entity;

            }
            catch ( Exception e )
            {
                LogError( className + ".GetGroupOrgObjectPrivileges(SqlConnection sqlConnection, int userId, string objectName: " + objectName + "): " + e.ToString() );
                return entity;
            }

        }//

        /// <summary>
        /// Future - ma be more flexible to use rowId???
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="userRowId"></param>
        /// <param name="objectName"></param>
        /// <returns></returns>
        public static ApplicationRolePrivilege GetGroupOrgObjectPrivileges( SqlConnection sqlConnection, string rowId, string objectName )
        {

            ApplicationRolePrivilege entity = new ApplicationRolePrivilege();
            if ( rowId.Trim().Length == 0 )
                return entity;
            entity.CanView = false;
            bool firstRow = true;

            try
            {
                //determine if user is in a group with access to the object 


                SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                sqlParameters[ 0 ] = new SqlParameter( "@RowId", rowId );
                sqlParameters[ 1 ] = new SqlParameter( "@ObjectName", objectName );

                SqlDataReader drdr = SqlHelper.ExecuteReader( sqlConnection, "AppObject_Group_OrgRowIdPrivileges_Select", sqlParameters );

                if ( drdr.HasRows )
                {
                    while ( drdr.Read() )
                    {
                        if ( firstRow )
                        {
                            entity.RoleId = GetRowColumn( drdr, "groupId", 0 );
                            entity.SubObjectId = 0;	// GetRowColumn(drdr, "SubObjectId", 0);

                            entity.SubObjectName = "";	// GetRowColumn(drdr, "SubObjectName", "");
                            entity.Description = GetRowColumn( drdr, "Description", "" );

                            entity.Sequence = 1;	// GetRowColumn( drdr, "sequence", 0 );
                            entity.CreatePrivilege = GetRowColumn( drdr, "createPrivilege", 0 );
                            entity.ReadPrivilege = GetRowColumn( drdr, "readPrivilege", 0 );
                            entity.WritePrivilege = GetRowColumn( drdr, "writePrivilege", 0 );
                            entity.DeletePrivilege = GetRowColumn( drdr, "deletePrivilege", 0 );
                            entity.AppendPrivilege = GetRowColumn( drdr, "appendPrivilege", 0 );
                            entity.AppendToPrivilege = GetRowColumn( drdr, "appendToPrivilege", 0 );
                            entity.AssignPrivilege = GetRowColumn( drdr, "assignPrivilege", 0 );
                            entity.ApprovePrivilege = GetRowColumn( drdr, "approvePrivilege", 0 );
                            entity.SharePrivilege = GetRowColumn( drdr, "sharePrivilege", 0 );
                        }
                        else
                        {
                            //check if next row has higher privileges
                            entity.HasChanged = false;
                            if ( entity.CreatePrivilege < GetRowColumn( drdr, "createPrivilege", 0 ) )
                                entity.CreatePrivilege = GetRowColumn( drdr, "createPrivilege", 0 );

                            if ( entity.ReadPrivilege < GetRowColumn( drdr, "readPrivilege", 0 ) )
                                entity.ReadPrivilege = GetRowColumn( drdr, "readPrivilege", 0 );

                            if ( entity.WritePrivilege < GetRowColumn( drdr, "writePrivilege", 0 ) )
                                entity.WritePrivilege = GetRowColumn( drdr, "writePrivilege", 0 );

                            if ( entity.DeletePrivilege < GetRowColumn( drdr, "deletePrivilege", 0 ) )
                                entity.DeletePrivilege = GetRowColumn( drdr, "deletePrivilege", 0 );

                            if ( entity.AppendPrivilege < GetRowColumn( drdr, "appendPrivilege", 0 ) )
                                entity.AppendPrivilege = GetRowColumn( drdr, "appendPrivilege", 0 );

                            if ( entity.AppendToPrivilege < GetRowColumn( drdr, "appendToPrivilege", 0 ) )
                                entity.AppendToPrivilege = GetRowColumn( drdr, "appendToPrivilege", 0 );

                            if ( entity.AssignPrivilege < GetRowColumn( drdr, "assignPrivilege", 0 ) )
                                entity.AssignPrivilege = GetRowColumn( drdr, "assignPrivilege", 0 );

                            if ( entity.ApprovePrivilege < GetRowColumn( drdr, "approvePrivilege", 0 ) )
                                entity.ApprovePrivilege = GetRowColumn( drdr, "approvePrivilege", 0 );

                            if ( entity.SharePrivilege < GetRowColumn( drdr, "sharePrivilege", 0 ) )
                                entity.SharePrivilege = GetRowColumn( drdr, "sharePrivilege", 0 );

                            if ( entity.HasChanged )
                            {
                                entity.RoleId = GetRowColumn( drdr, "groupId", 0 );
                                entity.Description = GetRowColumn( drdr, "Description", "" );

                                entity.Sequence = GetRowColumn( drdr, "sequence", 0 );
                            }
                        }

                        //break;
                    }
                }
                drdr.Close();
                drdr = null;
                return entity;

            }
            catch ( Exception e )
            {
                LogError( className + ".GetGroupOrgObjectPrivileges(SqlConnection sqlConnection, int userId, string objectName: " + objectName + "): " + e.ToString() );
                return entity;
            }

        }//

        #endregion

		
	}
}

