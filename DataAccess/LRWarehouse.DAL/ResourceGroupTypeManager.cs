using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Microsoft.ApplicationBlocks.Data;
using MyEntity = LRWarehouse.Business.ResourceChildItem;

namespace LRWarehouse.DAL
{
    public class ResourceGroupTypeManager : BaseDataManager, IResourceIntManager
    {
        const string className = "ResourceGroupTypeManager";

        /// <summary>
        /// Base procedures
        /// </summary>
        const string SELECT_PROC = "[Resource.GroupTypeSelect]";
        const string DELETE_PROC = "[Resource.GroupTypeDelete]";
        const string INSERT_PROC = "[Resource.GroupTypeInsert]";
        const string IMPORT_PROC = "[Resource.GroupTypeImport]";
        const string IMPORT_PROCV2 = "[Resource.GroupTypeImportV2]";

        /// <summary>
        /// Default constructor
        /// </summary>
        public ResourceGroupTypeManager()
        {
            //base constructor sets common connection strings
        }//

        #region ====== Core Methods ===============================================
        public bool Delete( int pResourceId, int pGroupTypeId, ref string statusMessage )
        {
            return Delete( pResourceId, pGroupTypeId, 0, ref statusMessage );
        }//

        /// <summary>
        /// Delete an GroupType record
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool Delete( int pId, ref string statusMessage )
        {
            return Delete( 0, 0, pId, ref statusMessage );
        }//

        private bool Delete( int pResourceId, int pGroupTypeId, int pId, ref string statusMessage )
        {
            bool successful;

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", pResourceId );
                sqlParameters[ 1 ] = new SqlParameter( "@GroupTypeId", pGroupTypeId );
                sqlParameters[ 2 ] = new SqlParameter( "@Id", pId );

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
        /// Required when implementing IResourceManager (issue with int vs string pResourceId)
        /// </summary>
        /// <param name="pResourceId"></param>
        /// <param name="pGroupTypeId"></param>
        /// <param name="pCreatedById"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool Insert( int pResourceId, int pGroupTypeId, int pCreatedById, ref string statusMessage )
        {

            //if ( Int32.TryParse( pResourceId, out resourceId ) )
            if ( pResourceId > 0 )
            {
                int id = Create( pResourceId, pGroupTypeId,  pCreatedById, ref statusMessage );
                return true;
            }
            else
            {
                return false;
            }
        }
        public int Create( MyEntity entity, ref string statusMessage )
        {
            //string resourceRowId = "";
            //if ( entity.ResourceId.ToString() != entity.DEFAULT_GUID )
            //    resourceRowId = entity.ResourceId.ToString();

            return Create( entity.ResourceIntId, entity.CodeId, entity.CreatedById, ref statusMessage );
        }

        public int Create( int pResourceId, int pGroupTypeId, int pCreatedById, ref string statusMessage )
        {
            statusMessage = "successful";
            int newId = 0;

            try
            {
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
                //sqlParameters[ 0 ] = new SqlParameter( "@ResourceId", SqlDbType.UniqueIdentifier );
                //sqlParameters[ 0 ].Value = entity.ResourceId;
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", pResourceId );
                sqlParameters[ 1 ] = new SqlParameter( "@GroupTypeId", pGroupTypeId );
                sqlParameters[ 2 ] = new SqlParameter( "@CreatedbyId", pCreatedById );
                #endregion

                SqlDataReader dr = SqlHelper.ExecuteReader( ConnString, CommandType.StoredProcedure, INSERT_PROC, sqlParameters );
                if ( dr.HasRows )
                {
                    dr.Read();
                    newId = GetRowColumn( dr, "Id", 0 );
                }
                dr.Close();
                dr = null;
                statusMessage = "successful";
            }
            catch ( Exception ex )
            {
                LogError( className + ".Create(): " + ex.ToString() );
                statusMessage = ex.ToString();
            }

            return newId;
        }

        public DataSet SelectedCodes( string pResourceId )
        {
            int resourceId = 0;

            if ( Int32.TryParse( pResourceId, out resourceId ) )
            {
                return SelectedCodes( resourceId );
            }
            else
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

                ds = SqlHelper.ExecuteDataset( LRWarehouseRO(), CommandType.StoredProcedure, "[Resource.GroupType_SelectedCodes]", sqlParameters );

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


        #endregion

        #region ====== Retrieval Methods ===============================================
        /// <summary>
        /// Select GroupType related data using passed parameters
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        public DataSet Select( int resourceId )
        {

            //replace following with actual nbr of parameters and do assignments
            SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
            sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", resourceId );

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

        #region ====== Import Methods ===============================================
        public int Import(MyEntity entity, ref string status)
        {
            status = "successful";
            int nbrRows = 0;

            try
            {
                #region SqlParameters
                SqlParameter[] parameters = new SqlParameter[4];
                parameters[0] = new SqlParameter("@ResourceIntId", entity.ResourceIntId);
                parameters[1] = new SqlParameter("@GroupTypeId", entity.CodeId);
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
                parameters[3] = new SqlParameter("@TotalRows", 0);
                parameters[3].Direction = ParameterDirection.Output;
                parameters[3].Value = 0;
                //parameters[4] = new SqlParameter("@ResourceRowId", entity.ResourceId);
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
        }// Import

        public int ImportV2(MyEntity entity, ref string status)
        {
            status = "successful";
            int nbrRows = 0;

            try
            {
                #region SqlParameters
                SqlParameter[] parameters = new SqlParameter[4];
                parameters[0] = new SqlParameter("@ResourceIntId", entity.ResourceIntId);
                parameters[1] = new SqlParameter("@GroupTypeId", entity.CodeId);
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
                parameters[3] = new SqlParameter("@TotalRows", 0);
                parameters[3].Direction = ParameterDirection.Output;
                parameters[3].Value = 0;
                //parameters[4] = new SqlParameter("@ResourceRowId", entity.ResourceId);
                #endregion

                DataSet ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, IMPORT_PROCV2, parameters);
                if (DoesDataSetHaveRows(ds))
                {
                    nbrRows = (int)parameters[3].Value;
                }
            }
            catch (Exception ex)
            {
                LogError(className + ".ImportV2(): " + ex.ToString());
                status = ex.Message;
            }

            return nbrRows;
        }// ImportV2
        #endregion
    }
}
