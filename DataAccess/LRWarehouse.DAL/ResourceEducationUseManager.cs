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
using MyEntity = LRWarehouse.Business.ResourceChildItem;
using Entity = LRWarehouse.Business.ResourceChildItem;
using EntityCollection = System.Collections.Generic.List<LRWarehouse.Business.ResourceChildItem>;

namespace LRWarehouse.DAL
{/// <summary>
    /// Data access manager for EducationUse
    /// </summary>
    public class ResourceEducationUseManager : BaseDataManager, IResourceIntManager
    {
        const string className = "EducationUseManager";

        /// <summary>
        /// Base procedures
        /// </summary>
        const string GET_PROC = "[Resource.EducationUseGet]";
        const string SELECT_PROC = "[Resource.EducationUseSelect]";
        const string DELETE_PROC = "[Resource.EducationUseDelete]";
        const string INSERT_PROC = "[Resource.EducationUseInsert]";
        const string IMPORT_PROC = "[Resource.EducationUseImport]";

        /// <summary>
        /// Default constructor
        /// </summary>
        public ResourceEducationUseManager()
        {
            //base constructor sets common connection strings
        }//

        #region ====== Core Methods ===============================================
        /// <summary>
        /// Delete an EducationUse record using resourceId and education use id
        /// </summary>
        /// <param name="pRowId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool Delete( int pResourceId, int pEducationUseId, ref string statusMessage )
        {
            bool successful;

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", pResourceId );
                sqlParameters[ 1 ] = new SqlParameter( "@EducationUseId", pEducationUseId );

                SqlHelper.ExecuteNonQuery( ConnString, CommandType.StoredProcedure, DELETE_PROC, sqlParameters );
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
        /// Add an EducationUse record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int Create( MyEntity entity, ref string statusMessage )
        {
            string resourceRowId = "";
            //if ( entity.ResourceId.ToString() != entity.DEFAULT_GUID )
            //    resourceRowId = entity.ResourceId.ToString();

            return Create( entity.ResourceIntId, entity.CodeId, entity.OriginalValue, entity.CreatedById, resourceRowId, ref statusMessage );
        }

        public int Create( int pResourceId, string pOriginalValue, int pCreatedById, ref string statusMessage )
        {
            //Guid pResourceRowId = Guid.NewGuid();
            return Create( pResourceId, 0, pOriginalValue, pCreatedById, "", ref statusMessage );
        }
        /// <summary>
        /// Required when implementing IResourceManager (issue with int vs string pResourceId)
        /// </summary>
        /// <param name="pResourceId"></param>
        /// <param name="pCodeId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool Insert( int pResourceId, int pEducationUseId, int pCreatedById, ref string statusMessage )
        {

            //if ( Int32.TryParse( pResourceId, out resourceId ) )
            if ( pResourceId > 0 )
            {
                int id = Create( pResourceId, pEducationUseId, "", pCreatedById, "", ref statusMessage );
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pResourceId"></param>
        /// <param name="pCodeId"></param>
        /// <param name="pOriginalValue"></param>
        /// <param name="pCreatedById"></param>
        /// <param name="pResourceRowId">Now obsolete</param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int Create( int pResourceId, int pCodeId, string pOriginalValue, int pCreatedById, string pResourceRowId, ref string statusMessage )
        {
            statusMessage = "successful";
            int newId = 0;
           // bool isDup = false;
           // string msg = "";
            string resourceRowId = "";
            MyEntity entity = new MyEntity();
            if ( pResourceRowId.Length > 35 && pResourceRowId.ToString() != entity.DEFAULT_GUID )
                resourceRowId = pResourceRowId.ToString();
            try
            {
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", pResourceId );
                sqlParameters[ 1 ] = new SqlParameter( "@EducationUseId", pCodeId );
                sqlParameters[ 2 ] = new SqlParameter( "@OriginalValue", pOriginalValue );
                sqlParameters[ 3 ] = new SqlParameter( "@CreatedbyId", pCreatedById );
                sqlParameters[ 4 ] = new SqlParameter( "@ResourceRowId", "" );
                #endregion

                SqlDataReader dr = SqlHelper.ExecuteReader( ConnString, CommandType.StoredProcedure, INSERT_PROC, sqlParameters );
                if ( dr.HasRows )
                {
                    dr.Read();
                    //newId = dr[ 0 ].ToString();
                    newId = GetRowColumn( dr, "Id", 0 );
                    //isDup = GetRowColumn( dr, "IsDuplicate", false );
                    //msg = GetRowColumn( dr, "Message", "" );
                }
                dr.Close();
                dr = null;
                statusMessage = "successful";
            }
            catch ( Exception ex )
            {
                LogError( "EducationUseManager.Create(): " + ex.ToString() );
                statusMessage = ex.ToString();
            }

            return newId;
        }


        #endregion

        #region ====== Retrieval Methods ===============================================
        public MyEntity Get( int pId )
        {
            return Get( pId, 0, 0 );
        }
        /// <summary>
        /// Get EducationUse record ==> un likely, except maybe to delete?
        /// </summary>
        /// <param name="pRowId"></param>
        /// <returns></returns>
        public MyEntity Get( int pId, int resourceId, int educationUseId )
        {
            MyEntity entity = new MyEntity();

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
                sqlParameters[ 0 ] = new SqlParameter( "@Id", pId );
                sqlParameters[ 1 ] = new SqlParameter( "@ResourceIntId", resourceId );
                sqlParameters[ 2 ] = new SqlParameter( "@EducationUseId", educationUseId );

                SqlDataReader dr = SqlHelper.ExecuteReader( ReadOnlyConnString, GET_PROC, sqlParameters );

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
                LogError( ex, className + ".Get() " );
                entity.Message = "Unsuccessful: " + className + ".Get(): " + ex.Message.ToString();
                entity.IsValid = false;
                return entity;

            }

        }//
        /// <summary>
        /// Select EducationUse related data using passed parameters
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        public DataSet Select( int resourceId, int educationUseId )
        {

            //replace following with actual nbr of parameters and do assignments
            SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
            sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", resourceId );
            sqlParameters[ 1 ] = new SqlParameter( "@EducationUseId", educationUseId );

            DataSet ds = new DataSet();
            try
            {
                ds = SqlHelper.ExecuteDataset( ReadOnlyConnString, CommandType.StoredProcedure, SELECT_PROC, sqlParameters );

                if ( ds.HasErrors )
                {
                    return null;
                }
                return ds;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".Select() " );
                return null;

            }
        }

        #endregion

        #region ====== Helper Methods ===============================================
        /// <summary>
        /// Fill an EducationUse object from a SqlDataReader
        /// </summary>
        /// <param name="dr">SqlDataReader</param>
        /// <returns>MyEntity</returns>
        public MyEntity Fill( SqlDataReader dr )
        {
            MyEntity entity = new MyEntity();

            entity.IsValid = true;
            //string rowId = GetRowColumn( dr, "RowId", "" );
            //if ( rowId.Length > 35 )
            //    entity.RowId = new Guid( rowId );

            //rowId = GetRowColumn( dr, "ResourceId", "" );
            //if ( rowId.Length > 35 )
            //    entity.ResourceId = new Guid( rowId );

            entity.Id = GetRowColumn( dr, "Id", 0 );
            entity.ResourceIntId = GetRowColumn( dr, "ResourceIntId", 0 );
            entity.CodeId = GetRowColumn( dr, "CodeId", 0 );
            entity.OriginalValue = GetRowColumn( dr, "OriginalValue", "" );

            return entity;
        }//

        #endregion


        #region Import methods
        public int Import(MyEntity entity, ref string status)
        {
            status = "successful";
            int nbrRows = 0;

            try
            {
                #region SqlParameters
                SqlParameter[] parameters = new SqlParameter[4];
                parameters[0] = new SqlParameter("@ResourceIntId", entity.ResourceIntId);
                parameters[1] = new SqlParameter("@EducationUseId", entity.CodeId);
                parameters[2] = new SqlParameter("@OriginalValue", SqlDbType.VarChar);
                parameters[2].Size = 100;
                if (entity.OriginalValue.Length > 100)
                {
                    parameters[2].Value = entity.OriginalValue.Substring(0, 100);
                }
                else
                {
                    parameters[2].Value = entity.OriginalValue;
                }
                parameters[3] = new SqlParameter("@TotalRows", SqlDbType.Int);
                parameters[3].Value = 0;
                parameters[3].Direction = ParameterDirection.Output;
                //parameters[4] = new SqlParameter("@ResourceRowId", SqlDbType.UniqueIdentifier);
                //parameters[4].Value = entity.ResourceId;
                #endregion

                DataSet ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, IMPORT_PROC, parameters);
                if (DoesDataSetHaveRows(ds))
                {
                    nbrRows = (int)parameters[3].Value;
                }
            }
            catch (Exception ex)
            {
                LogError(className + ".Import(): " + ex.ToString());
                status = ex.Message;
            }

            return nbrRows;
        }

        #endregion

        public DataSet SelectedCodes( string pResourceId )
        {
            int resourceId = 0;

            if ( Int32.TryParse( pResourceId, out resourceId ) )
            {
                return SelectedCodes( resourceId );
            } else 
            {
                return null;
            }
        }

        public DataSet SelectedCodes( int pResourceId )
        {

            DataSet ds = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", pResourceId );

                ds = SqlHelper.ExecuteDataset( LRWarehouseRO(), CommandType.StoredProcedure, "[Resource.EducationUse_SelectedCodes]", sqlParameters );

                if ( ds.HasErrors )
                {
                    return null;
                }
                return ds;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".SelectedCodes() " );
                return null;

            }
        }

        /// <summary>
        /// placeholder as required when implementing IResourceManager (issue with int vs string pResourceId)
        /// </summary>
        /// <param name="pResourceId"></param>
        /// <param name="pCreatedById"></param>
        /// <param name="pNewSelectedItems"></param>
        /// <param name="pUnSelectedItems"></param>
        /// <returns></returns>
        public bool ApplyChanges( string pResourceId,
                                    int pCreatedById,
                                    string pNewSelectedItems,
                                    string pUnSelectedItems )
        {
            int resourceId = 0;

            if ( Int32.TryParse( pResourceId, out resourceId ) )
            {
                return ApplyChanges( resourceId, pCreatedById, pNewSelectedItems, pUnSelectedItems );
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Adds/deletes Resource EducationLevel (only actual changes, rather than all)
        /// </summary>
        /// <param name="pResourceId"></param>
        /// <param name="pCreatedById"></param>
        /// <param name="pNewSelectedItems"></param>
        /// <param name="pUnSelectedItems"></param>
        /// <returns></returns>
        public bool ApplyChanges( int pResourceId,
                                    int pCreatedById,
                                    string pNewSelectedItems,
                                    string pUnSelectedItems )
        {
            string connectionString = LRWarehouse();
            string statusMessage = "";
            try
            {
                int counter = 0;

                foreach ( string newItem in pNewSelectedItems.Split( '|' ) )
                {
                    if ( newItem.Length > 0 )
                    {
                        int codeId = Int32.Parse( newItem );
                        Insert( pResourceId, codeId, pCreatedById, ref statusMessage );

                        counter++;
                    }
                }

                foreach ( string removedItem in pUnSelectedItems.Split( '|' ) )
                {
                    if ( removedItem.Length > 0 )
                    {
                        int id = Int32.Parse( removedItem );
                        Delete( pResourceId, id, ref statusMessage );

                        counter++;
                    }

                }

                return true;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".ApplyChanges() " );
                return false;
            }
        }



    }
}
