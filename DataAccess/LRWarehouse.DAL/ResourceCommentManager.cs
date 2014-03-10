using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.ApplicationBlocks.Data;
using MyEntity = LRWarehouse.Business.ResourceChildItem;
using ResourceComment = LRWarehouse.Business.ResourceComment;

namespace LRWarehouse.DAL
{
    public class ResourceCommentManager : BaseDataManager
    {
        const string className = "ResourceCommentManager";

        /// <summary>
        /// Base procedures
        /// </summary>
        const string GET_PROC = "[Resource.CommentGet]";
        const string SELECT_PROC = "[Resource.CommentSelect]";
        const string DELETE_PROC = "[Resource.CommentDelete]";
        const string INSERT_PROC = "[Resource.CommentInsert]";
        const string IMPORT_PROC = "[Resource.Comment_Import]";

        /// <summary>
        /// Default constructor
        /// </summary>
        public ResourceCommentManager()
        {
            //base constructor sets common connection strings
        }//

        #region ====== Core Methods ===============================================
        /// <summary>
        /// Delete an Comment record 
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool Delete( int pId, ref string statusMessage )
        {
            bool successful;

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@Id", pId );

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



        public int Create( ResourceComment entity, ref string statusMessage )
        {
            statusMessage = "successful";
            int newId = 0;

            try
            {
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 6 ];
                //sqlParameters[ 0 ] = new SqlParameter( "@ResourceId", SqlDbType.UniqueIdentifier );
                //sqlParameters[ 0 ].Value = entity.ResourceId;
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", entity.ResourceIntId );
                sqlParameters[ 1 ] = new SqlParameter( "@Comment", entity.Comment );
                sqlParameters[ 2 ] = new SqlParameter( "@IsActive", entity.IsActive );
                sqlParameters[ 3 ] = new SqlParameter( "@CreatedbyId", entity.CreatedById );
                sqlParameters[ 4 ] = new SqlParameter( "@CreatedBy", entity.CreatedBy );
                sqlParameters[ 5 ] = new SqlParameter( "@Commenter", entity.Commenter );
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
                LogError( "CommentManager.Create(): " + ex.ToString() );
                statusMessage = ex.ToString();
            }

            return newId;
        }


        public string Import(ResourceComment entity)
        {
            string status = "successful";

            try
            {
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[8];
                sqlParameters[0] = new SqlParameter("@ResourceIntId", entity.ResourceIntId);
                sqlParameters[1] = new SqlParameter("@Comment", entity.Comment);
                sqlParameters[2] = new SqlParameter("@IsActive", entity.IsActive);
                if (entity.Created >= SqlDateTime.MinValue && entity.Created <= SqlDateTime.MaxValue)
                {
                    sqlParameters[3] = new SqlParameter("@Created", entity.Created);
                }
                else
                {
                    sqlParameters[3] = new SqlParameter("@Created", DateTime.Now);
                }
                sqlParameters[4] = new SqlParameter("@CreatedBy", entity.CreatedBy);
                sqlParameters[5] = new SqlParameter("@ResourceId", entity.ResourceId);
                sqlParameters[6] = new SqlParameter("@Commenter", entity.Commenter);
                sqlParameters[7] = new SqlParameter("@DocId", entity.DocId);
                #endregion

                SqlHelper.ExecuteNonQuery(ConnString, CommandType.StoredProcedure, IMPORT_PROC, sqlParameters);
            }
            catch (Exception ex)
            {
                LogError("CommentManager.Import(): " + ex.ToString());
                status = ex.ToString();
            }

            return status;
        }


        #endregion

        #region ====== Retrieval Methods ===============================================

        /// <summary>
        /// Get Comment record ==> un likely, except maybe to delete?
        /// </summary>
        /// <param name="pRowId"></param>
        /// <returns></returns>
        public MyEntity Get( int pId )
        {
            MyEntity entity = new MyEntity();

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@Id", pId );


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
        /// Select Comment related data using passed parameters
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

        #region ====== Helper Methods ===============================================
        /// <summary>
        /// Fill an Comment object from a SqlDataReader
        /// </summary>
        /// <param name="dr">SqlDataReader</param>
        /// <returns>MyEntity</returns>
        public MyEntity Fill( SqlDataReader dr )
        {
            MyEntity entity = new MyEntity();

            entity.IsValid = true;

            string rowId = GetRowColumn( dr, "ResourceId", "" );
            if ( rowId.Length > 35 )
                entity.ResourceId = new Guid( rowId );

            entity.Id = GetRowColumn( dr, "Id", 0 );
            entity.ResourceIntId = GetRowColumn( dr, "ResourceIntId", 0 );
            entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
            entity.OriginalValue = GetRowColumn( dr, "Comment", "" );

            //string rowId = GetRowColumn( dr, "RowId", "" );
            //if ( rowId.Length > 35 )
            //    entity.RowId = new Guid( rowId );

            return entity;
        }//
        #endregion
    }
}
