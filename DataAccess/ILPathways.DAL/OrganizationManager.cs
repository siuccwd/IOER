using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Microsoft.ApplicationBlocks.Data;
using ILPathways.Business;

namespace ILPathways.DAL
{
	/// <summary>
	/// Summary description for OrganizationManager
	/// </summary>
	///<remarks></remarks>
	public class OrganizationManager : BaseDataManager
	{
		static string className = "OrganizationManager";

		public static int K12_SCHOOL_ORG_TYPE = 1;
        public static int K12_SCHOOL_DIV_ORG_TYPE = 2;
        public static int STATE_AGENCY_ORG_TYPE = 3;

		#region Constants for database procedures
		// Gets an organization 
		const string GET_PROC = "OrganizationGet";
		// Creates an organization 
		const string INSERT_PROC = "OrganizationInsert";
		// Updates an organization	
		const string UPDATE_PROC = "OrganizationUpdate";
		// Deletes an organization	
		const string OrganizationDelete_PROC = "OrganizationDelete";
		const string SEARCH_PROC = "[OrganizationSearch]";
		
		
		#endregion
		public OrganizationManager()
		{ }
		#region organization Core methods

		/// <summary>
		/// Create an organization 
		/// </summary>
		/// <param name="org"></param>
		/// <returns></returns>
		public static Organization Create( Organization org, string pathway )
		{
			org.IsValid = true;
			org.Message = "";
			try
			{
				#region parameters
				SqlParameter[] sqlParameters = new SqlParameter[ 17 ];
				sqlParameters[ 0 ] = new SqlParameter( "@Name", org.Name );
				sqlParameters[ 1 ] = new SqlParameter( "@OrgTypeId", org.OrgTypeId );

				sqlParameters[ 2 ] = new SqlParameter( "@parentId", org.ParentId );
				sqlParameters[ 3 ] = new SqlParameter( "@IsActive", org.IsActive );
				sqlParameters[ 4 ] = new SqlParameter( "@MainPhone", org.MainPhone );
				sqlParameters[ 5 ] = new SqlParameter( "@MainExtension", org.MainExtension );
				sqlParameters[ 6 ] = new SqlParameter( "@Fax", org.Fax );
				sqlParameters[ 7 ] = new SqlParameter( "@TTY", org.TTY );
				sqlParameters[ 8 ] = new SqlParameter( "@WebSite", org.WebSite );
				sqlParameters[ 9 ] = new SqlParameter( "@email", org.Email );
                sqlParameters[ 10 ] = new SqlParameter( "@LogoUrl", org.LogoUrl );
				sqlParameters[ 11 ] = new SqlParameter( "@Address", org.Address1 );
				sqlParameters[ 12 ] = new SqlParameter( "@Address2", org.Address2 );
				sqlParameters[ 13 ] = new SqlParameter( "@City", org.City );
				sqlParameters[ 14 ] = new SqlParameter( "@State", org.State );
				sqlParameters[ 15 ] = new SqlParameter( "@Zipcode", org.Zipcode );

                sqlParameters[ 16 ] = new SqlParameter( "@CreatedById", org.CreatedById );

				#endregion

				//Add the organization to the Organization table, returning the id of the record just added
				SqlDataReader dr = SqlHelper.ExecuteReader( GatewayConnection(), INSERT_PROC, sqlParameters );
				if ( dr.HasRows )
				{
					dr.Read();
					org.Id = int.Parse( dr[ 0 ].ToString() );

				}
				dr.Close();
				dr = null;

				return Get( org.Id );
			} catch ( Exception e )
			{
                LogError( "OrganizationManager.Create(): " + e.ToString() );
				org.IsValid = false;
				//TODO: should improve messages returned from a proc!!
				org.Message = e.ToString();
				return org;
			}
		}//

		/// <summary>
		/// Update an organization 
		/// </summary>
		/// <param name="org"></param>
		/// <returns></returns>
		public static Organization Update( Organization org )
		{

			//alternate may be to pass organization as reference and use a bool to indicate success or failure
			try
			{
				#region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 16 ];
				sqlParameters[ 0 ] = new SqlParameter( "@id", org.Id );
                sqlParameters[ 1 ] = new SqlParameter( "@Name", org.Name );

                sqlParameters[ 2 ] = new SqlParameter( "@IsActive", org.IsActive );
                sqlParameters[ 3 ] = new SqlParameter( "@MainPhone", org.MainPhone );
                sqlParameters[ 4 ] = new SqlParameter( "@MainExtension", org.MainExtension );
                sqlParameters[ 5 ] = new SqlParameter( "@Fax", org.Fax );
                sqlParameters[ 6 ] = new SqlParameter( "@TTY", org.TTY );
                sqlParameters[ 7 ] = new SqlParameter( "@WebSite", org.WebSite );
                sqlParameters[ 8 ] = new SqlParameter( "@email", org.Email );
                sqlParameters[ 9 ] = new SqlParameter( "@LogoUrl", org.LogoUrl );
                sqlParameters[ 10 ] = new SqlParameter( "@Address", org.Address1 );
                sqlParameters[ 11 ] = new SqlParameter( "@Address2", org.Address2 );
                sqlParameters[ 12 ] = new SqlParameter( "@City", org.City );
                sqlParameters[ 13 ] = new SqlParameter( "@State", org.State );
                sqlParameters[ 14 ] = new SqlParameter( "@Zipcode", org.Zipcode );

                sqlParameters[ 15 ] = new SqlParameter( "@LastUpdatedById", org.LastUpdatedById );

				#endregion

				SqlHelper.ExecuteNonQuery( GatewayConnection(), UPDATE_PROC, sqlParameters );

				org.Message = "successful";
				return org;


			} catch ( Exception e )
			{
				LogError( string.Format("OrganizationManager.Update(): id={0}, \rName:{1}\r\n ", org.Id, org.Name ) + e.ToString()  );
				org.IsValid = false;
				org.Message = e.ToString();
				return org;
			}

		}//

		/// <summary>
		/// Delete a site
		/// </summary>
		/// <param name="id"></param>
		/// <param name="deletedBy">Perhaps use for logging??</param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public static bool Delete( int id, string deletedBy, ref string statusMessage )
		{
			bool successful;

			SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
			sqlParameters[ 0 ] = new SqlParameter( "@id", SqlDbType.Int );
			sqlParameters[ 0 ].Value = id;

			try
			{
				SqlHelper.ExecuteNonQuery( GatewayConnection(), CommandType.StoredProcedure, OrganizationDelete_PROC, sqlParameters );
				successful = true;
				

			} catch ( Exception ex )
			{
				LogError( ex, "OrganizationManager.Delete() " );
				statusMessage = "Unsuccessful: OrganizationManager.Delete(): " + ex.Message.ToString();

				successful = false;
			}
			return successful;
		}
		#endregion
		#region organization retrieval methods
		/// <summary>
		/// Get an organization ONLY
		/// NOTE: Use GetOrganization() to just get the organization and all services and funding sources
		/// </summary>
		/// <param name="id">organization id</param>
		/// <returns></returns>
		public static Organization Get( int id )
		{
			return Get( id, "" );
		}//
        public static Organization GetByRowId( string rowId )
		{
			return Get( 0, rowId );
		}//
		/// <summary>
		/// Get an organization ONLY
		/// NOTE: Use GetOrganization() to just get the organization and all services and funding sources
		/// </summary>
		/// <param name="id">organization id</param>
		/// <returns></returns>
		private static Organization Get( int id, string rowId )
		{

			Organization entity = new Organization();
			try
			{
				//NOTE need to address other calls to this method, webservices, etc. 
				//So using a second proc
				SqlParameter[] sqlParameters = new SqlParameter[1];
				sqlParameters[0] = new SqlParameter("@id", id);
				//sqlParameters[ 1 ] = new SqlParameter( "@RowId", rowId );

				SqlDataReader dr = SqlHelper.ExecuteReader( GatewayConnectionRO(), GET_PROC, sqlParameters );

				if (dr.HasRows)
				{
					while (dr.Read())
					{
						entity = Fill( dr );
						
						//should only have one row returned, so do a break just in case
						break;
					}
				}
				dr.Close();
				dr = null;

				return entity;

			}
			catch (Exception e)
			{
				LogError("OrganizationManager.Get(): " + e.ToString());
				return entity;
				//throw e;
			}

		}//

        /// <summary>
        /// Retrieve organization by name - will need to handle dups in the future, or generate a unique name?
        /// </summary>
        /// <param name="orgName"></param>
        /// <returns></returns>
        public static Organization GetByName( string orgName)
        {

            Organization entity = new Organization();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@Name", orgName );

                SqlDataReader dr = SqlHelper.ExecuteReader( GatewayConnectionRO(), "[OrganizationGetByName]", sqlParameters );

                if ( dr.HasRows )
                {
                    while ( dr.Read() )
                    {
                        entity = Fill( dr );

                        //should only have one row returned, so do a break just in case
                        break;
                    }
                }
                dr.Close();
                dr = null;

                return entity;

            }
            catch ( Exception e )
            {
                LogError( "OrganizationManager.GetByName(): " + e.ToString() );
                return entity;
                //throw e;
            }

        }//

        public static DataSet GetTopLevelOrganzations()
        {

            string filter = " (IsActive = 1 AND parentId is null)";
            return SearchProxy( filter, "Name");
        }//

		/// <summary>
		/// Get all active children of passed organization 
		/// The parent is not returned
		/// </summary>
        /// <param name="parentId">parentId for the  org</param>
		/// <returns>DataSet of child organizations</returns>
		/// <remarks>This should probably be a call to an appropriate method within the organization manager</remarks>
        public static DataSet GetChildOrganizations( int parentId )
		{
            //return GetChildOrganizations( parentId, false );
            string filter = string.Format(" (IsActive = 1 AND parentId = {0}) ", parentId);
            return SearchProxy( filter );
		}//
        public static DataSet GetRTTTOrganzations()
        {

            string filter = " (IsActive = 1 AND (parentId is null OR parentId in (2,3)))";
            return SearchProxy( filter, "Name" );
        }//
		/// <summary>
		/// Get all active children of passed organization 
		/// </summary>
		/// <param name="id">id for the org</param>
		/// <param name="includeParent">True if parent should be returned as well</param>/// 
		/// <returns>DataSet of child organizations (and parent organization if includeParent = true</returns>
		/// <remarks>This should probably be a call to an appropriate method within the organization manager</remarks>
		public static DataSet GetChildOrganizations(int parentId, bool includeParent)
		{
			DataSet ds = new DataSet();

			try
			{
				SqlParameter[] sqlParameters = new SqlParameter[3];
                sqlParameters[ 0 ] = new SqlParameter( "@id", parentId );
				sqlParameters[1] = new SqlParameter("@IsActive", 1);
				sqlParameters[2] = new SqlParameter("@IncludeParent", includeParent);

				ds = SqlHelper.ExecuteDataset(GatewayConnectionRO(), "TBD", sqlParameters);
				if (ds.HasErrors)
				{
					ds = null;
					return null;
				}
				return ds;

			}
			catch (Exception e)
			{
				LogError("ReportingManager.GetChildOrganizations(): " + e.ToString());
				return null;
			}
		}//


        private static DataSet SearchProxy( string filter )
        {
            int pTotalRows = 0;
            int pMaximumRows = 500;

            return Search( filter, "Name", 1, pMaximumRows, ref pTotalRows );
		}//

        private static DataSet SearchProxy( string filter, string orderBy )
        {
            int pTotalRows = 0;
            int pMaximumRows = 500;

            return Search( filter, orderBy, 1, pMaximumRows, ref pTotalRows );
        }//
        /// <summary>
        /// Search for organization related data using passed parameters
        /// </summary>
        /// <param name="pFilter"></param>
        /// <param name="pOrderBy">Sort order of results. If blank, the proc should have a default sort order</param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public static List<Organization> SearchAsList( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            List<Organization> list = new List<Organization>();
            DataSet ds = Search( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
            if ( DatabaseManager.DoesDataSetHaveRows( ds ) )
            {
                foreach ( DataRow dr in ds.Tables[ 0 ].DefaultView.Table.Rows )
                {
                    Organization org = Fill( dr );
                    list.Add( org );
                } //end foreach
            }

            return list;
		}//

		/// <summary>
		/// Search for organization related data using passed parameters
		/// - uses custom paging
		/// - only requested range of rows will be returned
		/// </summary>
		/// <param name="pFilter"></param>
		/// <param name="pOrderBy">Sort order of results. If blank, the proc should have a default sort order</param>
		/// <param name="pStartPageIndex"></param>
		/// <param name="pMaximumRows"></param>
		/// <param name="pTotalRows"></param>
		/// <returns></returns>
        public static DataSet Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
		{
			int outputCol = 4;

			SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
			sqlParameters[ 0 ] = new SqlParameter( "@Filter", pFilter );
			sqlParameters[ 1 ] = new SqlParameter( "@SortOrder", pOrderBy );
			sqlParameters[ 2 ] = new SqlParameter( "@StartPageIndex",  pStartPageIndex);
			sqlParameters[ 3 ] = new SqlParameter( "@PageSize", pMaximumRows);

			sqlParameters[ outputCol ] = new SqlParameter( "@TotalRows", SqlDbType.Int );
			sqlParameters[ outputCol ].Direction = ParameterDirection.Output;

            using ( SqlConnection conn = new SqlConnection( GatewayConnectionRO() ) )
            {
                DataSet ds = new DataSet();
                try
                {
                    ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, SEARCH_PROC, sqlParameters );

                    string rows = sqlParameters[ outputCol ].Value.ToString();
                    try
                    {
                        pTotalRows = Int32.Parse( rows );
                    }
                    catch
                    {
                        pTotalRows = 0;
                    }

                    if ( ds.HasErrors )
                    {
                        return null;
                    }
                    return ds;
                }
                catch ( Exception ex )
                {
                    LogError( ex, className + ".Search() " );
                    return null;

                }
            }
		}

        /// <summary>
        /// Fill an Organization object from a SqlDataReader
        /// </summary>
        /// <param name="dr">SqlDataReader</param>
        /// <returns>Policy</returns>
        public static Organization Fill( SqlDataReader dr )
        {
            Organization entity = new Organization();

            entity.Id = GetRowColumn( dr, "id", 0 );

            entity.Name = dr[ "name" ].ToString();
            entity.OrgTypeId = GetRowColumn( dr, "OrgTypeId", 0 );
            entity.OrgType = GetRowPossibleColumn( dr, "OrgType", entity.OrgTypeId.ToString() );

            entity.ParentId = GetRowColumn( dr, "parentId", 0 );
            //entity.PrimaryContactId = GetRowColumn( dr, "primaryContactId", 0 );

            entity.IsActive = GetRowColumn( dr, "IsActive", false );
            entity.MainPhone = GetRowColumn( dr, "MainPhone", "" );
            entity.MainExtension = GetRowColumn( dr, "MainExtension", "" );
            entity.Fax = GetRowColumn( dr, "Fax", "" );
            entity.TTY = GetRowColumn( dr, "TTY", "" );
            entity.WebSite = GetRowColumn( dr, "WebSite", "" );
            entity.Email = GetRowColumn( dr, "Email", "" );
            entity.LogoUrl = GetRowColumn( dr, "LogoUrl", "" );

            entity.Address1 = GetRowColumn( dr, "Address", "" );
            entity.Address2 = GetRowColumn( dr, "Address2", "" );
            entity.City = GetRowColumn( dr, "City", "" );
            entity.State = GetRowColumn( dr, "State", "" );
            entity.Zipcode = GetRowColumn( dr, "Zipcode", "" );
            entity.ZipCode4 = GetRowPossibleColumn( dr, "ZipCode4", "" );

            entity.Created = DateTime.Parse( dr[ "Created" ].ToString() );
            entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
            //entity.CreatedBy = dr[ "CreatedBy" ].ToString();
            entity.LastUpdated = DateTime.Parse( dr[ "LastUpdated" ].ToString() );
            entity.LastUpdatedById = GetRowColumn( dr, "LastUpdatedById", 0 );
            //entity.LastUpdatedBy = dr[ "LastUpdatedBy" ].ToString();

            string rowId = GetRowColumn( dr, "RowId", "" );
            if ( rowId.Length > 35 )
                entity.RowId = new Guid( rowId );


            return entity;
        }//
        /// <summary>
        /// Fill an Organization object from a SqlDataReader
        /// </summary>
        /// <param name="dr">SqlDataReader</param>
        /// <returns>Policy</returns>
        public static Organization Fill( DataRow dr )
        {
            Organization entity = new Organization();

            entity.Id = GetRowColumn( dr, "id", 0 );

            entity.Name = dr[ "name" ].ToString();
            entity.OrgTypeId = GetRowColumn( dr, "OrgTypeId", 0 );
            entity.OrgType = GetRowPossibleColumn( dr, "OrgType", entity.OrgTypeId.ToString());

            entity.ParentId = GetRowColumn( dr, "parentId", 0 );
            //entity.PrimaryContactId = GetRowColumn( dr, "primaryContactId", 0 );

            entity.IsActive = GetRowColumn( dr, "IsActive", false );
            entity.MainPhone = GetRowColumn( dr, "MainPhone", "" );
            entity.MainExtension = GetRowColumn( dr, "MainExtension", "" );
            entity.Fax = GetRowColumn( dr, "Fax", "" );
            entity.TTY = GetRowColumn( dr, "TTY", "" );
            entity.WebSite = GetRowColumn( dr, "WebSite", "" );
            entity.Email = GetRowColumn( dr, "Email", "" );
            entity.LogoUrl = GetRowColumn( dr, "LogoUrl", "" );

            entity.Address1 = GetRowColumn( dr, "Address", "" );
            entity.Address2 = GetRowColumn( dr, "Address2", "" );
            entity.City = GetRowColumn( dr, "City", "" );
            entity.State = GetRowColumn( dr, "State", "" );
            entity.Zipcode = GetRowColumn( dr, "Zipcode", "" );
            entity.ZipCode4 = GetRowPossibleColumn( dr, "ZipCode4", "" );

            entity.Created = DateTime.Parse( dr[ "Created" ].ToString() );
            entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
            //entity.CreatedBy = dr[ "CreatedBy" ].ToString();
            entity.LastUpdated = DateTime.Parse( dr[ "LastUpdated" ].ToString() );
            entity.LastUpdatedById = GetRowColumn( dr, "LastUpdatedById", 0 );
            //entity.LastUpdatedBy = dr[ "LastUpdatedBy" ].ToString();

            string rowId = GetRowColumn( dr, "RowId", "" );
            if ( rowId.Length > 35 )
                entity.RowId = new Guid( rowId );


            return entity;
        }//

        public static string[] OrganzationsAutoComplete( string prefixText )
        {
            //int count = 10;
            string sql = "SELECT [Name], [Name]  + ' [' + convert(varchar, [Id]) + '] ' As Combined FROM  [Gateway].[dbo].[Organization] where [IsActive]= 1 And Name like @prefixText order by 1";

            try
            {
                DataSet ds = DatabaseManager.DoQuery( sql );

                SqlDataAdapter da = new SqlDataAdapter( sql, GatewayConnectionRO() );

                da.SelectCommand.Parameters.Add( "@prefixText", SqlDbType.VarChar, 100 ).Value = "%" + prefixText + "%";

                DataTable dt = new DataTable();
                da.Fill( dt );
                string[] items = new string[ dt.Rows.Count ];
                int i = 0;
                foreach ( DataRow dr in dt.Rows )
                {
                    items.SetValue( dr[ "Combined" ].ToString(), i );
                    i++;
                }
                return items;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".OrganzationsAutoComplete() - Unexpected error encountered while attempting search. " );
                return null;
            }

        }//
		#endregion


		#region organization Contacts

		/// <summary>
		/// Get all ACTIVE LWIA contacts for an organization 
		/// </summary>
		/// <param name="orgId">id for the org</param>
		/// <param name="includeChildSiteUsers">If True, then also retrieve all users from child organizations (ie. where an organization's parentId equals the passed orgId</param> 
		/// <returns></returns>
		public static DataSet GetOrgUsers( int orgId, bool includeChildSiteUsers )
		{

			DataSet ds = new DataSet();
			try
			{

				SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
				sqlParameters[ 0 ] = new SqlParameter( "@orgId", orgId );
				if ( includeChildSiteUsers )
					sqlParameters[ 1 ] = new SqlParameter( "@parentId", orgId );
				else
					sqlParameters[ 1 ] = new SqlParameter( "@parentId", 0 );

				ds = SqlHelper.ExecuteDataset( GatewayConnectionRO(), "TBD", sqlParameters );
				if ( ds.HasErrors )
				{
					ds = null;
					return null;
				}
				return ds;



			} catch ( Exception e )
			{
				LogError( className + ".GetOrgUsers(): " + e.ToString() );
				return null;
			}

		}//


		#endregion

	
  }
}
