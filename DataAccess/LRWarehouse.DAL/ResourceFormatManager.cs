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
    /// Data access manager for ResourceFormat
    /// </summary>
    public class ResourceFormatManager : BaseDataManager, IResourceIntManager
    {
        const string className = "ResourceFormatManager";

        /// <summary>
        /// Base procedures
        /// </summary>
        const string GET_PROC = "[Resource.FormatGet]";
        const string SELECT_PROC = "[Resource.FormatSelect]";
        const string SELECT2_PROC = "[Resource.FormatSelect2]";
        const string DELETE_PROC = "[Resource.FormatDelete]";
        const string INSERT_PROC = "[Resource.FormatInsert]";
        const string UPDATE_PROC = "[Resource.FormatUpdate]";
        const string IMPORT_PROC = "[Resource.Format_Import]";


        /// <summary>
        /// Default constructor
        /// </summary>
        public ResourceFormatManager()
        {
            //base constructor sets common connection strings
            
            /*
            ConnString = LRWarehouse();
            ReadOnlyConnString = LRWarehouseRO();
             */
        }//

        #region ====== Core Methods ===============================================
        /// <summary>
        /// Delete an ResourceFormat record using rowId
        /// </summary>
        /// <param name="pResourceIntId"></param>
        /// <param name="pCodeId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool Delete( int pResourceIntId, int pCodeId, ref string statusMessage )
        {
            bool successful;

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", pResourceIntId );
                sqlParameters[ 1 ] = new SqlParameter( "@CodeId", pCodeId );

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
        /// Delete an ResourceFormat record using rowId ==> not implemented
        /// </summary>
        /// <param name="pRowId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        private bool Delete( string pRowId, ref string statusMessage )
        {
            bool successful;

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@RowId", pRowId );

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
        public string CreateFromEntity( Entity entity, ref string statusMessage )
        {
            statusMessage = "";
            string newId = "";

            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[ 4 ];
                //sqlParameter[ 0 ] = new SqlParameter( "@ResourceId", entity.ResourceId.ToString() );
                sqlParameter[ 0 ] = new SqlParameter( "@ResourceIntId", entity.ResourceIntId );
                sqlParameter[ 1 ] = new SqlParameter( "@ResourceFormatId", entity.CodeId );
                sqlParameter[ 2 ] = new SqlParameter( "@OriginalValue", entity.OriginalValue);
                sqlParameter[ 3 ] = new SqlParameter( "@CreatedById", entity.CreatedById );

                SqlDataReader dr = SqlHelper.ExecuteReader( ConnString, CommandType.StoredProcedure, "[Resource.FormatInsert]", sqlParameter );
                if ( dr.HasRows )
                {
                    dr.Read();
                    newId = dr[ 0 ].ToString();
                    entity.RowId = new Guid( newId );
                }
                dr.Close();
                dr = null;
                statusMessage = "successful";
            }
            catch ( Exception ex )
            {
                LogError( "ResourceFormatManager.Create(): " + ex.ToString() );
                statusMessage = ex.Message;
            }
            return newId;
        }
        /*
        /// <summary>
        /// Add an ResourceFormat record ===> obsolete
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        
        private string Create( MyEntity entity, ref string statusMessage )
        {
            string newId = "";
            bool isDup = false;
            string msg = "";
            try
            {
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceId", entity.ResourceId );
                sqlParameters[ 1 ] = new SqlParameter( "@ResourceFormatId", entity.CodeId );
                sqlParameters[ 2 ] = new SqlParameter( "@OriginalValue", entity.OriginalValue );
                //sqlParameters[ 3 ] = new SqlParameter( "@CreatedbyId", entity.CreatedById );
                #endregion

                SqlDataReader dr = SqlHelper.ExecuteReader( ConnString, CommandType.StoredProcedure, INSERT_PROC, sqlParameters );
                if ( dr.HasRows )
                {
                    dr.Read();
                    newId = GetRowColumn( dr, "Id", "");
                    isDup = GetRowPossibleColumn( dr, "IsDuplicate", false );
                    msg = GetRowColumn( dr, "Message", "" );

                    if ( newId.Length == 36)
                        entity.RowId = new Guid( newId );
                }
                dr.Close();
                dr = null;
                if ( isDup )
                {
                    statusMessage = msg;
                    //TODO - not sure if the dup value should be returned???
                    newId = "";
                }
                else if ( newId == "" )
                {
                    //prob a mapping error
                    statusMessage = msg;
                }
                else
                {
                    statusMessage = "successful";
                }
            }
            catch ( Exception ex )
            {
                LogError( ex, className + string.Format( ".Create() for ResourceId: {0}, CodeId: {1}, Vale: {2}, and CreatedBy: {4}", entity.ResourceId.ToString(), entity.CodeId, entity.OriginalValue, entity.CreatedById ) );
                statusMessage = className + "- Unsuccessful: Create(): " + ex.Message.ToString();
            }

            return newId;
        }*/

        //public string Create( string pResourceId, int pCodeId, int pCreatedById, ref string statusMessage )
        //{
        //    return Create( pResourceId, pCodeId, "", pCreatedById, ref statusMessage );
        //}
        //public string Create( string pResourceId, int pCodeId, string pOriginalValue, ref string statusMessage )
        //{
        //    return Create( pResourceId, pCodeId, pOriginalValue, 0, ref statusMessage );
        //}

        //public string Create( string pResourceId, int pCodeId, string pOriginalValue, int pCreatedById, ref string statusMessage )
        //{
        //    statusMessage = "successful";
        //    string newId = "";


        //    try
        //    {
        //        SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
        //        sqlParameters[ 0 ] = new SqlParameter( "@ResourceId", pResourceId.ToString() );
        //        sqlParameters[ 1 ] = new SqlParameter( "@ResourceFormatId", pCodeId );
        //        sqlParameters[ 2 ] = new SqlParameter( "@OriginalValue", pOriginalValue );
        //        //sqlParameters[ 3 ] = new SqlParameter( "@CreatedbyId", pCreatedById );
        //        SqlDataReader dr = SqlHelper.ExecuteReader( ConnString, CommandType.StoredProcedure, "[Resource.FormatInsert]", sqlParameters );
        //        if ( dr.HasRows )
        //        {
        //            dr.Read();
        //            newId = dr[ 0 ].ToString();
        //            //entity.RowId = new Guid(newId);
        //        }
        //        dr.Close();
        //        dr = null;
        //        statusMessage = "successful";
        //    }
        //    catch ( Exception ex )
        //    {
        //        LogError( "ResourceFormatManager.Create(): " + ex.ToString() );
        //        statusMessage = ex.ToString();
        //    }

        //    return newId;
        //}

        /// <summary>
        /// Import
        /// the Insert is now the same as this essentially, so probably will not use
        /// May still be used by the process to resolve mapping issues, but likely to be called from a sproc
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public string Import( MyEntity entity, ref string statusMessage )
        {
            string newId = "";
            bool isDup = false;
            string msg = "";
            int outputCol = 3;
            int pTotalRows = 0;
            DataSet ds = new DataSet();
                        
            try
            {
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 4 ];
                //sqlParameters[ 0 ] = new SqlParameter( "@ResourceId", entity.ResourceId );
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", entity.ResourceIntId );
                sqlParameters[ 1 ] = new SqlParameter( "@ResourceFormatId", entity.CodeId );
                sqlParameters[ 2 ] = new SqlParameter( "@OriginalValue", entity.OriginalValue );
                sqlParameters[ outputCol ] = new SqlParameter( "@TotalRows", SqlDbType.Int );
                sqlParameters[ outputCol ].Direction = ParameterDirection.Output;
                
                #endregion
                //??
                ds = SqlHelper.ExecuteDataset( ConnString, CommandType.StoredProcedure, IMPORT_PROC, sqlParameters );

                string rows = sqlParameters[ outputCol ].Value.ToString();
                try
                {
                    pTotalRows = Int32.Parse( rows );
                }
                catch
                {
                    pTotalRows = 0;
                }
                //OR ????????????????????
                //SqlDataReader dr = SqlHelper.ExecuteReader( ConnString, CommandType.StoredProcedure, IMPORT_PROC, sqlParameters );
                //if ( dr.HasRows )
                //{
                //    dr.Read();
                //    newId = GetRowColumn( dr, "Id", "" );
                //    isDup = GetRowColumn( dr, "IsDuplicate", false );
                //    msg = GetRowColumn( dr, "Message", "" );
                //}
                //dr.Close();
                //dr = null;
                statusMessage = "successful";

            }
            catch ( Exception ex )
            {
                LogError( ex, className + string.Format( ".Import() for ResourceId: {0}, CodeId: {1}, Vale: {2}, and CreatedBy: {4}", entity.ResourceId.ToString(), entity.CodeId, entity.OriginalValue, entity.CreatedById ) );
                statusMessage = className + "- Unsuccessful: Import(): " + ex.Message.ToString();
            }

            return newId;
        }

        /// <summary>
        /// Add an ResourceFormat record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public string Update( MyEntity entity )
        {
            string message = "successful";
            try
            {
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
                sqlParameters[ 0 ] = new SqlParameter( "@RowId", entity.RowId );
                sqlParameters[ 1 ] = new SqlParameter( "@CodeId", entity.CodeId );
                sqlParameters[ 2 ] = new SqlParameter( "@MappedValue", entity.OriginalValue );
                #endregion

                SqlHelper.ExecuteNonQuery( ConnString, UPDATE_PROC, sqlParameters );
                message = "successful";
            }
            catch ( Exception ex )
            {
                LogError( ex, className + string.Format( ".Update() for ResourceId: {0}, CodeId: {1}, Vale: {2}, and UpdatedBy: {4}", entity.ResourceId.ToString(), entity.CodeId, entity.OriginalValue, entity.LastUpdatedById ) );
                message = className + "- Unsuccessful: Update(): " + ex.Message.ToString();
                entity.Message = message;
                entity.IsValid = false;
            }

            return message;
        }
        #endregion

        #region ====== Retrieval Methods ===============================================
        /// <summary>
        /// Get ResourceFormat record via rowId ==> un likely, except maybe to delete?
        /// </summary>
        /// <param name="pRowId"></param>
        /// <returns></returns>
        public Entity Get( string pRowId )
        {
            Entity entity = new Entity();

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@RowId", SqlDbType.UniqueIdentifier );
                sqlParameters[ 0 ].Value = new Guid( pRowId );


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
        /// Select ResourceFormat related data using passed parameters
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        private DataSet SelectDS( string pResourceId, int pCodeId )
        {

            //replace following with actual nbr of parameters and do assignments
            SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
            sqlParameters[ 0 ] = new SqlParameter( "@ResourceId", pResourceId );
            sqlParameters[ 1 ] = new SqlParameter( "@CodeId", pCodeId );

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


        public EntityCollection Select( int pResourceIntId )
        {
            EntityCollection collection = new EntityCollection();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", pResourceIntId );

                DataSet ds = SqlHelper.ExecuteDataset( ConnString, CommandType.StoredProcedure, SELECT2_PROC, sqlParameters );
                if (DoesDataSetHaveRows(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        Entity entity = FillEntity( dr );
                        collection.Add(entity);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("ResourceFormatManager.Select(): " + ex.ToString());
                return null;
            }

            return collection;
        }
        public List<MyEntity> SelectList( int pResourceIntId )
        {

            List<MyEntity> collection = new List<MyEntity>();

            SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
            sqlParameters[ 0 ] = new SqlParameter( "@pResourceIntId", pResourceIntId );

            DataSet ds = new DataSet();
            try
            {
                ds = SqlHelper.ExecuteDataset( ReadOnlyConnString, CommandType.StoredProcedure, SELECT2_PROC, sqlParameters );

                if ( DoesDataSetHaveRows( ds ) )
                {
                    foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                    {
                        MyEntity entity = Fill( dr );
                        collection.Add( entity );
                    }
                }
                return collection;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".Select( int pResourceIntId ) " );
                return null;

            }
        }
        #endregion

        #region ====== Helper Methods ===============================================
        /// <summary>
        /// Fill an ResourceChildItem object from a DataRow
        /// </summary>
        /// <param name="dr">DataRow</param>
        /// <returns>MyEntity</returns>
        public MyEntity Fill( DataRow dr )
        {
            MyEntity entity = new MyEntity();
            string rowId = GetRowColumn( dr, "RowId", "" );
            if ( rowId.Length > 35 )
                entity.RowId = new Guid( rowId );

            entity.ResourceIntId = GetRowColumn( dr, "ResourceIntId", 0 );
            entity.OriginalValue = GetRowColumn( dr, "OriginalValue", "" );
            entity.CodeId = GetRowColumn( dr, "ResourceTypeId", 0 );

            entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
            entity.Created = GetRowColumn( dr, "Created", DateTime.MinValue );

            return entity;
        }

        public Entity FillEntity( DataRow dr )
        {
            Entity entity = new Entity();
            string rowId = GetRowColumn( dr, "RowId", "" );
            if ( rowId.Length > 35 )
                entity.RowId = new Guid( rowId );

            entity.ResourceIntId = GetRowColumn( dr, "ResourceIntId", 0 );
            entity.OriginalValue = GetRowColumn( dr, "OriginalValue", "" );
            entity.CodeId = GetRowColumn( dr, "CodeId", 0 );

            entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
            entity.Created = GetRowColumn( dr, "Created", DateTime.MinValue );

            return entity;
        }
        public Entity Fill( SqlDataReader dr )
        {
            Entity entity = new Entity();
            string rowId = GetRowColumn( dr, "RowId", "" );
            if ( rowId.Length > 35 )
                entity.RowId = new Guid( rowId );

            entity.ResourceIntId = GetRowColumn( dr, "ResourceIntId", 0 );
            entity.OriginalValue = GetRowColumn( dr, "OriginalValue", "" );
            entity.CodeId = GetRowColumn( dr, "CodeId", 0 );

            return entity;
        }//

        #endregion

         
        #region Import methods
        public Entity GetResourceMap( string rowId )
        {
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[ 1 ];
                sqlParameter[ 0 ] = new SqlParameter( "@RowId", SqlDbType.UniqueIdentifier );
                sqlParameter[ 0 ].Value = new Guid( rowId );
                DataSet ds = SqlHelper.ExecuteDataset( ConnString, CommandType.StoredProcedure, "[Resource.FormatGet]", sqlParameter );
                if ( DoesDataSetHaveRows( ds ) )
                {
                    Entity entity = FillEntity( ds.Tables[ 0 ].Rows[ 0 ] );
                    return entity;
                }
                else
                {
                    return null;
                }
            }
            catch ( Exception ex )
            {
                LogError( "ResourceFormatManager.Get(): " + ex.ToString() );
                return null;
            }
        }

        /// <summary>
        /// garbage ==> the proc doesn't exist, parms don't make sense
        /// 
        /// </summary>
        /// <param name="rowId"></param>
        /// <param name="resourceId"></param>
        /// <param name="codeId"></param>
        /// <returns></returns>
        //public EntityCollection Select( string rowId, string resourceId, int codeId )
        //{
        //    EntityCollection collection = new EntityCollection();
        //    try
        //    {
        //        SqlParameter[] sqlParameter = new SqlParameter[ 3 ];
        //        sqlParameter[ 0 ] = new SqlParameter( "@RowId", rowId );
        //        sqlParameter[ 1 ] = new SqlParameter( "@ResourceId", resourceId );
        //        sqlParameter[ 2 ] = new SqlParameter( "@ResourceFormatId", codeId );

        //        DataSet ds = SqlHelper.ExecuteDataset( ConnString, CommandType.StoredProcedure, "[Resource.Select]", sqlParameter );
        //        if ( DoesDataSetHaveRows( ds ) )
        //        {
        //            foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
        //            {
        //                Entity entity = FillEntity( dr );
        //                collection.Add( entity );
        //            }
        //        }
        //    }
        //    catch ( Exception ex )
        //    {
        //        LogError( "ResourceFormatManager.Select(): " + ex.ToString() );
        //        return null;
        //    }
        //    return collection;
        //}


        #endregion
        public DataSet SelectedCodes( int pResourceId )
        {

            DataSet ds = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[1];
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", pResourceId );

                ds = SqlHelper.ExecuteDataset( LRWarehouseRO(), CommandType.StoredProcedure, "[Resource.Format_SelectedCodes]", sqlParameters );

                if (ds.HasErrors)
                {
                    return null;
                }
                return ds;
            }
            catch (Exception ex)
            {
                LogError(ex, className + ".SelectedCodes() ");
                return null;

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
            Entity entity = new Entity();
            entity.ResourceIntId = pResourceId;
            entity.CreatedById = pCreatedById;

            try
            {
                int counter = 0;

                foreach ( string newItem in pNewSelectedItems.Split( '|' ) )
                {
                    if ( newItem.Length > 0 )
                    {
                        int codeId = Int32.Parse( newItem );
                        //Create( pResourceId, codeId, pCreatedById, ref statusMessage );

                        entity.CodeId = Int32.Parse( newItem );
                        CreateFromEntity( entity, ref statusMessage );
                        counter++;
                    }
                }

                foreach ( string removedItem in pUnSelectedItems.Split( '|' ) )
                {
                    if ( removedItem.Length > 0 )
                    {
                        int id = Int32.Parse( removedItem );
                        SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                        sqlParameters[ 0 ] = new SqlParameter( "@ResourceId", pResourceId );
                        sqlParameters[ 1 ] = new SqlParameter( "@CodeId", id );

                        SqlHelper.ExecuteNonQuery( connectionString, CommandType.StoredProcedure, "[Resource.FormatDelete]", sqlParameters );

                        counter++;
                    }

                }

                return true;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".ResourceEducationLevel_ApplyChanges() " );
                return false;
            }
        }

        public bool Insert( int pResourceId, int pTypeId, int pCreatedById, ref string statusMessage )
        {
            Entity entity = new Entity();
            entity.ResourceIntId = pResourceId;
            entity.CreatedById = pCreatedById;
            entity.CodeId = pTypeId;
            string result = CreateFromEntity( entity, ref statusMessage );
            //string result = Create( pResourceId, pFormatId, "", ref statusMessage );
            if ( result != "" ) { return true; }
            else { return false; }
        }

        #region string resourceId versions
        //public DataSet SelectedCodes( string pResourceId )
        //{
        //    int resourceId = 0;

        //    if ( Int32.TryParse( pResourceId, out resourceId ) )
        //    {
        //        return SelectedCodes( resourceId );
        //    } else 
        //    {
        //        return null;
        //    }
        //}


        ///// <summary>
        ///// Adds/deletes Resource EducationLevel (only actual changes, rather than all)
        ///// </summary>
        ///// <param name="pResourceId"></param>
        ///// <param name="pCreatedById"></param>
        ///// <param name="pNewSelectedItems"></param>
        ///// <param name="pUnSelectedItems"></param>
        ///// <returns></returns>
        //public bool ApplyChanges(string pResourceId,
        //                        int pCreatedById,
        //                        string pNewSelectedItems,
        //                        string pUnSelectedItems)
        //{
        //    string connectionString = LRWarehouse();
        //    string statusMessage = "";
        //    try
        //    {
        //        int counter = 0;

        //        foreach (string newItem in pNewSelectedItems.Split('|'))
        //        {
        //            if (newItem.Length > 0)
        //            {
        //                int codeId = Int32.Parse(newItem);
        //                Insert( pResourceId, codeId, pCreatedById, ref statusMessage );

        //                counter++;
        //            }
        //        }

        //        foreach (string removedItem in pUnSelectedItems.Split('|'))
        //        {
        //            if (removedItem.Length > 0)
        //            {
        //                int id = Int32.Parse(removedItem);
        //                SqlParameter[] sqlParameters = new SqlParameter[2];
        //                sqlParameters[0] = new SqlParameter("@ResourceId", pResourceId);
        //                sqlParameters[1] = new SqlParameter("@CodeId", id);

        //                SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "[Resource.FormatDelete]", sqlParameters);

        //                counter++;
        //            }

        //        }

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        LogError(ex, className + ".ResourceEducationLevel_ApplyChanges() ");
        //        return false;
        //    }
        //}

        //public bool Insert( string pResourceId, int pFormatId, int pCreatedById, ref string statusMessage )
        //{
        //    string result = Create( pResourceId, pFormatId, "", ref statusMessage );
        //    if ( result != "" ) { return true; }
        //    else { return false; }
        //}
        #endregion

    }
}
