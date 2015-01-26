using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
	/// Data access manager for DocumentVersion
	/// </summary>
	public class DocumentStoreManager : BaseDataManager
	{
        static string className = "DocumentStoreManager";

		/// <summary>
		/// Base procedures
		/// </summary>
		const string GET_PROC = "DocumentVersionGet";
		const string SELECT_PROC = "DocumentVersionSelect";
		const string DELETE_PROC = "DocumentVersionDelete";
		const string INSERT_PROC = "DocumentVersionInsert";
		const string UPDATE_PROC = "DocumentVersionUpdate";


		/// <summary>
		/// Default constructor
		/// </summary>
		public DocumentStoreManager()
		{ }//

		#region ====== Core Methods ===============================================
		/// <summary>
		/// Delete a DocumentVersion record using rowId
		/// </summary>
		/// <param name="pRowId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool Delete( string pRowId, ref string statusMessage )
		{
            string connectionString = ContentConnection();
			bool successful;

			SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
			sqlParameters[ 0 ] = new SqlParameter( "@RowId", SqlDbType.UniqueIdentifier );
			sqlParameters[ 0 ].Value = new Guid( pRowId );

			try
			{
				SqlHelper.ExecuteNonQuery( connectionString, CommandType.StoredProcedure, DELETE_PROC, sqlParameters );
				successful = true;
			} catch ( Exception ex )
			{
				LogError( ex, className + ".Delete() " );
				statusMessage = className + "- Unsuccessful: Delete(): " + ex.Message.ToString();

				successful = false;
			}
			return successful;
		}//

		/// <summary>
		/// Add an DocumentVersion record
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public static string Create( DocumentVersion entity, ref string statusMessage )
		{
            string connectionString = ContentConnection();
			string newId = "";

			#region parameters
			SqlParameter[] sqlParameters = new SqlParameter[ 11 ];
			sqlParameters[ 0 ] = new SqlParameter( "@Title", SqlDbType.VarChar );
			sqlParameters[ 0 ].Size = 200;
			sqlParameters[ 0 ].Value = entity.Title;

			sqlParameters[ 1 ] = new SqlParameter( "@Summary", SqlDbType.VarChar );
			sqlParameters[ 1 ].Size = 500;
			sqlParameters[ 1 ].Value = entity.Summary;

			sqlParameters[ 2 ] = new SqlParameter( "@Status", SqlDbType.VarChar );
			sqlParameters[ 2 ].Size = 25;
			sqlParameters[ 2 ].Value = entity.Status;

			sqlParameters[ 3 ] = new SqlParameter( "@FileName", SqlDbType.VarChar );
			sqlParameters[ 3 ].Size = 150;
			sqlParameters[ 3 ].Value = entity.FileName;

			sqlParameters[ 4 ] = new SqlParameter( "@FileDate", SqlDbType.DateTime );
			if ( entity.FileDate < new System.DateTime( 1980, 1, 1 ) )
				entity.FileDate = System.DateTime.Now;
			sqlParameters[ 4 ].Value = entity.FileDate;

			sqlParameters[ 5 ] = new SqlParameter( "@MimeType", SqlDbType.VarChar );
			sqlParameters[ 5 ].Size = 150;
			sqlParameters[ 5 ].Value = entity.MimeType;

			sqlParameters[ 6 ] = new SqlParameter( "@Bytes", SqlDbType.BigInt );
			sqlParameters[ 6 ].Value = entity.ResourceBytes;

			sqlParameters[ 7 ] = new SqlParameter( "@Data", SqlDbType.VarBinary );
			sqlParameters[ 7 ].Value = entity.ResourceData;

			sqlParameters[ 8 ] = new SqlParameter( "@URL", SqlDbType.VarChar );
			sqlParameters[ 8 ].Size = 150;
			sqlParameters[ 8 ].Value = entity.URL;

			sqlParameters[ 9 ] = new SqlParameter( "@CreatedById", SqlDbType.Int );
			sqlParameters[ 9 ].Value = entity.CreatedById;
            sqlParameters[ 10 ] = new SqlParameter( "@FilePath", entity.FilePath );

			#endregion

			try
			{
				SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, CommandType.StoredProcedure, INSERT_PROC, sqlParameters );
				if ( dr.HasRows )
				{
					dr.Read();
					newId = dr[ 0 ].ToString();
				}
				dr.Close();
				dr = null;
				statusMessage = "successful";

			} catch ( Exception ex )
			{
				LogError( ex, className + ".Create() " );
				statusMessage = className + "- Unsuccessful: Create(): " + ex.Message.ToString();
				entity.Message = statusMessage;
				entity.IsValid = false;
			}

			return newId;
		}

		/// <summary>
		/// /// Update an DocumentVersion record
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public string Update( DocumentVersion entity )
		{
			string message = "successful";
            string connectionString = ContentConnection();

            if ( entity.HasDocument() == false )
            {
                //if missing doc, need to first retrieve a doc, then overlay as needed
                DocumentVersion old = Get( entity.RowId );
                if ( old.IsValid == false )
                {
                    return "Error: - document not found!";
                }
                //only overlay potential missing data
                entity.MimeType = old.MimeType;
                entity.ResourceBytes = old.ResourceBytes;
                entity.SetResourceData( old.ResourceBytes, old.ResourceData );

            }

			#region parameters
			SqlParameter[] sqlParameters = new SqlParameter[ 12 ];
			sqlParameters[ 0 ] = new SqlParameter( "@RowId", SqlDbType.UniqueIdentifier );
			sqlParameters[ 0 ].Value = entity.RowId;

			sqlParameters[ 1 ] = new SqlParameter( "@Title", SqlDbType.VarChar );
			sqlParameters[ 1 ].Size = 200;
			sqlParameters[ 1 ].Value = entity.Title;

			sqlParameters[ 2 ] = new SqlParameter( "@Summary", SqlDbType.VarChar );
			sqlParameters[ 2 ].Size = 500;
			sqlParameters[ 2 ].Value = entity.Summary;

			sqlParameters[ 3 ] = new SqlParameter( "@Status", SqlDbType.VarChar );
			sqlParameters[ 3 ].Size = 25;
			sqlParameters[ 3 ].Value = entity.Status;

			sqlParameters[ 4 ] = new SqlParameter( "@FileName", entity.FileName);

			sqlParameters[ 5 ] = new SqlParameter( "@FileDate", SqlDbType.DateTime );
			sqlParameters[ 5 ].Value = entity.FileDate;

			sqlParameters[ 6 ] = new SqlParameter( "@MimeType",  entity.MimeType);

			sqlParameters[ 7 ] = new SqlParameter( "@Bytes", SqlDbType.BigInt );
			sqlParameters[ 7 ].Value = entity.ResourceBytes;

			sqlParameters[ 8 ] = new SqlParameter( "@Data", SqlDbType.VarBinary );
			sqlParameters[ 8 ].Value = entity.ResourceData;

			sqlParameters[ 9 ] = new SqlParameter( "@URL", SqlDbType.VarChar );
			sqlParameters[ 9 ].Size = 150;
			sqlParameters[ 9 ].Value = entity.URL;

			sqlParameters[ 10 ] = new SqlParameter( "@LastUpdatedById", SqlDbType.Int );
			sqlParameters[ 10 ].Value = entity.LastUpdatedById;

            sqlParameters[ 11 ] = new SqlParameter( "@FilePath", entity.FilePath );
			#endregion

			try
			{
				SqlHelper.ExecuteNonQuery( connectionString, UPDATE_PROC, sqlParameters );
				message = "successful";

			} catch ( Exception ex )
			{
				LogError( ex, className + ".Update() " );
				message = className + "- Unsuccessful: Update(): " + ex.Message.ToString();
				entity.Message = message;
				entity.IsValid = false;
			}

			return message;

		}//


        public string UpdateFileInfo( DocumentVersion entity )
        {
            string message = "successful";
            string connectionString = ContentConnection();

            #region parameters
            SqlParameter[] sqlParameters = new SqlParameter[ 4 ];
            sqlParameters[ 0 ] = new SqlParameter( "@RowId", SqlDbType.UniqueIdentifier );
            sqlParameters[ 0 ].Value = entity.RowId;
            sqlParameters[ 1 ] = new SqlParameter( "@FilePath", entity.FilePath );
            sqlParameters[ 2 ] = new SqlParameter( "@FileName", entity.FileName );
            sqlParameters[ 3 ] = new SqlParameter( "@URL", entity.URL);

            //sqlParameters[ 4 ] = new SqlParameter( "@LastUpdatedById", entity.LastUpdatedById);

            
            #endregion

            try
            {
                SqlHelper.ExecuteNonQuery( connectionString, "[DocumentVersion.UpdateFileInfo]", sqlParameters );
                message = "successful";

            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".UpdateFileInfo() " );
                message = className + "- Unsuccessful: UpdateFileInfo(): " + ex.Message.ToString();
                entity.Message = message;
                entity.IsValid = false;
            }

            return message;

        }//

        /// <summary>
        /// Set the record status to published
        /// Used where the doc version is created before the parent (actually the norm). The status defaults to initial on create.
        /// If something prevented the parent from being saved the record would then be orphaned - depending on how the current process handles the condition.
        /// </summary>
        /// <param name="pRowId"></param>
        /// <returns></returns>
        public static string SetToPublished( string pRowId )
        {
            string message = "successful";
            string connectionString = ContentConnection();

            #region parameters
            SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
            sqlParameters[ 0 ] = new SqlParameter( "@RowId", pRowId );
            #endregion

            try
            {
                SqlHelper.ExecuteNonQuery( connectionString, "DocumentVersion_SetToPublished", sqlParameters );
                message = "successful";

            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".Update() " );
                message = "Error- Unsuccessful: Update(): " + ex.Message.ToString();
            }

            return message;

        }//
		#endregion

		#region ====== Retrieval Methods ===============================================
		/// <summary>
		/// Get DocumentVersion record
		/// </summary>
		/// <param name="pRowId"></param>
		/// <returns></returns>
		public static DocumentVersion Get( string pRowId )
		{
			Guid rowId = new Guid( pRowId );
			return Get( rowId );
		}

		public static DocumentVersion Get( Guid rowId )
		{
            string connectionString = ContentConnectionRO();
			DocumentVersion entity = new DocumentVersion();

			try
			{
				SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
				sqlParameters[ 0 ] = new SqlParameter( "@RowId", SqlDbType.UniqueIdentifier );
				sqlParameters[ 0 ].Value = rowId;

				SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, GET_PROC, sqlParameters );

				if ( dr.HasRows )
				{
					// it should return only one record.
					while ( dr.Read() )
					{
						entity = Fill( dr );
					}

				} else
				{
					entity.Message = "Record not found";
					entity.IsValid = false;
				}
				dr.Close();
				dr = null;
				return entity;

			} catch ( Exception ex )
			{
                LogError( ex, className + string.Format(".Get() guid: {0}", rowId.ToString()) );
				entity.Message = "Unsuccessful: " + className + ".Get(): " + ex.Message.ToString();
				entity.IsValid = false;
				return entity;

			}

		}//

		/// <summary>
		/// Select DocumentVersion related data using passed parameters
		/// Note: not sure a select would be done directly against documents as the context of related docs should be at the parent level
		/// </summary>
		/// <param name="pId"></param>
		/// <returns></returns>
		private static DataSet Select( int pId )
		{
			//SET THIS METHOD TO PRIVATE AS NO CURRENT NEED TO DO A SELECT OF MULTIPLE DOCUMENTS
            string connectionString = ContentConnectionRO();

			SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
			sqlParameters[ 0 ] = new SqlParameter( "@id", SqlDbType.Int );
			sqlParameters[ 0 ].Value = pId;


			DataSet ds = new DataSet();
			try
			{
				ds = SqlHelper.ExecuteDataset( connectionString, CommandType.StoredProcedure, SELECT_PROC, sqlParameters );

				if ( ds.HasErrors )
				{
					return null;
				}
				return ds;
			} catch ( Exception ex )
			{
				LogError( ex, className + ".Select() " );
				return null;

			}
		}

		#endregion

		#region ====== Helper Methods ===============================================
		/// <summary>
		/// Fill an DocumentVersion object from a SqlDataReader
		/// </summary>
		/// <param name="dr">SqlDataReader</param>
		/// <returns>DocumentVersion</returns>
		public static DocumentVersion Fill( SqlDataReader dr )
		{
			DocumentVersion entity = new DocumentVersion();

			entity.IsValid = true;

			string rowId = GetRowColumn( dr, "RowId", "" );
			if ( rowId.Length > 35 )
			  entity.RowId = new Guid( rowId );

			entity.Title = GetRowColumn( dr, "Title", "missing" );
			entity.Summary = GetRowColumn( dr, "Summary", "" );

			entity.URL = GetRowColumn( dr, "URL", "" );
			entity.Status = GetRowColumn( dr, "Status", "" );
			entity.FileName = GetRowColumn( dr, "FileName", "" );
            entity.FilePath = GetRowColumn( dr, "FilePath", "" );
			entity.FileDate = GetRowColumn( dr, "FileDate", System.DateTime.Now );

			entity.MimeType = GetRowColumn( dr, "MimeType", "" );
			entity.ResourceBytes = long.Parse( GetRowColumn( dr, "Bytes", "0" ) );
			if ( entity.ResourceBytes > 0 )
			{
				entity.SetResourceData( entity.ResourceBytes, dr[ "Data" ] );
			}

			entity.Created = GetRowColumn( dr, "Created", System.DateTime.MinValue );
			//entity.CreatedBy = GetRowColumn( dr, "CreatedBy", "" );
			entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );

			entity.LastUpdated = GetRowColumn( dr, "LastUpdated", System.DateTime.MinValue );
			//entity.LastUpdatedBy = GetRowColumn( dr, "LastUpdatedBy", "" );
			entity.LastUpdatedById = GetRowColumn( dr, "LastUpdatedById", 0 );

			return entity;
		}//

		#endregion

	}
}