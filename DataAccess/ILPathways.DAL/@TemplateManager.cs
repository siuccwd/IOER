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
using MyEntity = ILPathways.Business.DataItem;

/*
  * USE this template for tables where the PK is an integer
 * =======================================================================
Instructions
- change all Template to the target table/entity name
- change all MyEntity to the target table/entity name
- use Code Generator to create data access methods
- copy and paste into related methods
  
NOTES
- Methods are no longer static. the calling code will have to first instantiate the data manager before using any methods
- re: RowId - this template contains Delete and Get using both Id and RowId. Usually only one of the latter is required, 
  so delete the other one

*/
namespace ILPathways.DAL
{
	/// <summary>
	/// Data access manager for Template
	/// </summary>
	public class TemplateManager : BaseDataManager
	{
		static string thisClassName = "TemplateManager";

		/// <summary>
		/// Base procedures
		/// </summary>
		const string GET_PROC = "[TemplateGet]";
        const string SEARCH_PROC = "[TemplateSearch]";
		const string SELECT_PROC = "[TemplateSelect]";
		const string DELETE_PROC = "[TemplateDelete]";
		const string INSERT_PROC = "[TemplateInsert]";
		const string UPDATE_PROC = "[TemplateUpdate]";


		/// <summary>
		/// Default constructor
		/// </summary>
		public TemplateManager()
		{ }//

		#region ====== Core Methods ===============================================
		/// <summary>
		/// Delete a Template record
		/// </summary>
		/// <param name="pId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool Delete( int pId, ref string statusMessage )
		{
			bool successful = false;
            using ( SqlConnection conn = new SqlConnection( ILPathwaysConnection() ) )
            {
                try
                {
                    SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@Id", pId );

                    SqlHelper.ExecuteNonQuery( conn, CommandType.StoredProcedure, DELETE_PROC, sqlParameters );
                    successful = true;
                }
                catch ( Exception ex )
                {
                    LogError( ex, thisClassName + ".Delete() " );
                    statusMessage = thisClassName + "- Unsuccessful: Delete(): " + ex.Message.ToString();

                    successful = false;
                }
            }
			return successful;
		}//

		/// <summary>
		/// Add a Template record
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public int Create( MyEntity entity, ref string statusMessage )
		{
			int newId = 0;
            using ( SqlConnection conn = new SqlConnection( ILPathwaysConnection() ) )
            {
                try
                {

                    #region parameters
                    //replace following with actual nbr of parameters and do assignments
                    SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@id", SqlDbType.Int );
                    sqlParameters[ 0 ].Value = entity.Id;

                    //...


                    #endregion

                    SqlDataReader dr = SqlHelper.ExecuteReader( conn, CommandType.StoredProcedure, INSERT_PROC, sqlParameters );
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
                    LogError( ex, thisClassName + string.Format( ".Create() for Id: {0} and ???: {1} and CreatedBy: {2}", entity.Id.ToString(), entity.Title.ToString(), entity.CreatedBy ) );
                    //ex:
                    //LogError( ex, thisClassName + string.Format( ".Create() for orgId: {0} and userid: {1} and programId: {2} and contact type: {3}", entity.OrgId.ToString(), entity.UserId.ToString(), entity.ProgramId.ToString(), entity.ContactType ) );

                    statusMessage = thisClassName + "- Unsuccessful: Create(): " + ex.Message.ToString();
                   
                    entity.Message = statusMessage;
                    entity.IsValid = false;
                }
            }
			return newId;
		}

		/// <summary>
		/// Update a Template record
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public string Update( MyEntity entity )
		{
			string message = "successful";
            using ( SqlConnection conn = new SqlConnection( ILPathwaysConnection() ) )
            {
                try
                {

                    #region parameters
                    //replace following with actual nbr of parameters and do assignments
                    SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@id", SqlDbType.Int );
                    sqlParameters[ 0 ].Value = entity.Id;

                    //...

                    #endregion

                    SqlHelper.ExecuteNonQuery( conn, UPDATE_PROC, sqlParameters );
                    message = "successful";

                }
                catch ( Exception ex )
                {
                    LogError( ex, thisClassName + string.Format( ".Update() for Id: {0} and ???: {1} and LastUpdatedBy: {2}", entity.Id.ToString(), entity.Title.ToString(), entity.LastUpdatedBy ) );
                    //ex:
                    //LogError( ex, thisClassName + string.Format( ".Update() for orgId: {0} and userid: {1} and programId: {2} and contact type: {3}", entity.OrgId.ToString(), entity.UserId.ToString(), entity.ProgramId.ToString(), entity.ContactType ) );
                    message = thisClassName + "- Unsuccessful: Update(): " + ex.Message.ToString();
                    entity.Message = message;
                    entity.IsValid = false;
                }
            }
			return message;

		}//
		#endregion

		#region ====== Retrieval Methods ===============================================
		/// <summary>
		/// Get Template record
		/// </summary>
		/// <param name="pId"></param>
		/// <returns></returns>
		public MyEntity Get( int pId )
		{
			MyEntity entity = new MyEntity();
            using ( SqlConnection conn = new SqlConnection( GetReadOnlyConnection() ) )
            {
                try
                {
                    SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@id", SqlDbType.Int );
                    sqlParameters[ 0 ].Value = pId;


                    SqlDataReader dr = SqlHelper.ExecuteReader( conn, GET_PROC, sqlParameters );

                    if ( dr.HasRows )
                    {
                        // it should return only one record.
                        while ( dr.Read() )
                        {
                            entity = Fill( dr );
                        }

                    }
                    else
                    {
                        entity.Message = "Record not found";
                        entity.IsValid = false;
                    }
                    dr.Close();
                    dr = null;
                    return entity;

                }
                catch ( Exception ex )
                {
                    LogError( ex, thisClassName + ".Get() " );
                    entity.Message = "Unsuccessful: " + thisClassName + ".Get(): " + ex.Message.ToString();
                    entity.IsValid = false;
                    return entity;

                }
            }

		}//

		/// <summary>
		/// Select Template related data using passed parameters
		/// </summary>
		/// <param name="pId"></param>
		/// <returns></returns>
		public DataSet Select( int pId, string parm2 )
		{

            using ( SqlConnection conn = new SqlConnection( GetReadOnlyConnection() ) )
            {
                //replace following with actual nbr of parameters and do assignments
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@id", SqlDbType.Int );
                sqlParameters[ 0 ].Value = pId;


                DataSet ds = new DataSet();
                try
                {
                    ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, SELECT_PROC, sqlParameters );

                    if ( ds.HasErrors )
                    {
                        return null;
                    }
                    return ds;
                }
                catch ( Exception ex )
                {
                    LogError( ex, thisClassName + ".Select() " );
                    return null;

                }
            }
		}

		/// <summary>
		/// Search for Template related data using passed parameters
		/// - uses custom paging
		/// - only requested range of rows will be returned
		/// </summary>
		/// <param name="pFilter"></param>
		/// <param name="pOrderBy">Sort order of results. If blank, the proc should have a default sort order</param>
		/// <param name="pStartPageIndex"></param>
		/// <param name="pMaximumRows"></param>
		/// <param name="pTotalRows"></param>
		/// <returns></returns>
		public DataSet Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
		{
			//***** delete if not using custom paging *******
			
            int outputCol = 4;
			SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
			sqlParameters[ 0 ] = new SqlParameter( "@Filter", pFilter );
            sqlParameters[ 1 ] = new SqlParameter( "@SortOrder", pOrderBy );

			sqlParameters[ 2 ] = new SqlParameter( "@StartPageIndex", SqlDbType.Int );
			sqlParameters[ 2 ].Value = pStartPageIndex;

			sqlParameters[ 3 ] = new SqlParameter( "@PageSize", SqlDbType.Int );
			sqlParameters[ 3 ].Value = pMaximumRows;

            sqlParameters[ outputCol ] = new SqlParameter( "@TotalRows", SqlDbType.Int );
            sqlParameters[ outputCol ].Direction = ParameterDirection.Output;

            using ( SqlConnection conn = new SqlConnection( GetReadOnlyConnection() ) )
            {
                DataSet ds = new DataSet();
                try
                {
                    ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, SEARCH_PROC, sqlParameters );
                    //get output paramter
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
                    LogError( ex, thisClassName + ".Search() " );
                    return null;

                }
            }
		}

		#endregion

		#region ====== Helper Methods ===============================================
		/// <summary>
		/// Fill a Template object from a SqlDataReader
		/// </summary>
		/// <param name="dr">SqlDataReader</param>
		/// <returns>MyEntity</returns>
		public MyEntity Fill( SqlDataReader dr )
		{
			MyEntity entity = new MyEntity();

			entity.IsValid = true;

			entity.Id = GetRowColumn( dr, "Id", 0 );
			entity.Title = GetRowColumn( dr, "Title", "missing" );
			//...
			entity.IsActive = GetRowColumn( dr, "IsActive", false );
			entity.Created = GetRowColumn( dr, "Created", System.DateTime.MinValue );
			//Optional:		entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
			entity.CreatedBy = GetRowColumn( dr, "CreatedBy", "" );
			entity.LastUpdated = GetRowColumn( dr, "LastUpdated", System.DateTime.MinValue );
			//Optional:		entity.LastUpdatedById = GetRowColumn( dr, "LastUpdatedById", 0 );
			entity.LastUpdatedBy = GetRowColumn( dr, "LastUpdatedBy", "" );

			//NOTE - only include the RowId in the Get/Fill process if the business requirements demand it. 
			//Otherwise delete the following (however make sure the rowId is not part of Update if not in Fill!!!)
			string rowId = GetRowColumn( dr, "RowId", "" );
			//if expecting a RowId and it is not found is probably an error condition and someone should be notified!!
			if ( rowId.Length > 35 )
				entity.RowId = new Guid( rowId );

			return entity;
		}//

		#endregion

	}
}