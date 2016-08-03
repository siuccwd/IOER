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
using LRWarehouse.Business;

namespace LRWarehouse.DAL
{/// <summary>
    /// Data access manager for StandardBody.Topic
    /// </summary>
    public class StandardDataManager : BaseDataManager
    {
        const string className = "StandardDataManager";

        /// <summary>
        /// Base procedures
        /// </summary>
        const string GET_PROC = "[StandardBody.TopicGet]";
        const string SELECT_PROC = "[StandardBody.TopicSelect]";
        const string DELETE_PROC = "[StandardBody.TopicDelete]";
        const string INSERT_PROC = "[StandardBody.TopicInsert]";
        const string UPDATE_PROC = "[StandardBody.TopicUpdate]";


        /// <summary>
        /// Default constructor
        /// </summary>
        public StandardDataManager()
        {
            //base constructor sets common connection strings
        }//

        #region ====== StandardSubject Methods ===============================================

        /// <summary>
        /// Add an StandardBody StandardSubject record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int StandardSubject_Create( StandardSubject entity, ref string statusMessage )
        {
            int newId = 0;

            try
            {
                #region parameters
                //replace following with actual nbr of parameters and do assignments
                SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
                sqlParameters[ 0 ] = new SqlParameter( "@StandardBodyId", entity.StandardBodyId );
                sqlParameters[ 1 ] = new SqlParameter( "@Title", entity.Title );
                sqlParameters[ 2 ] = new SqlParameter( "@Description", entity.Description );
                sqlParameters[ 3 ] = new SqlParameter( "@Url", entity.Url );
                sqlParameters[ 4 ] = new SqlParameter( "@Language", entity.Language );
                #endregion

                SqlDataReader dr = SqlHelper.ExecuteReader( ConnString, CommandType.StoredProcedure, "[StandardBody.SubjectInsert]", sqlParameters );
                if ( dr.HasRows )
                {
                    dr.Read();
                    newId = int.Parse( dr[ 0 ].ToString() );
                    entity.Id = newId;
                }
                dr.Close();
                dr = null;
                statusMessage = "successful";

            }
            catch ( Exception ex )
            {
                LogError( ex, className + string.Format( ".Create() for RowId: {0} and CreatedBy: {1}", entity.RowId.ToString(), entity.CreatedBy ) );
                statusMessage = className + "- Unsuccessful: Create(): " + ex.Message.ToString();
                entity.Message = statusMessage;
                entity.IsValid = false;
            }

            return newId;
        }

        /// <summary>
        /// Update an StandardSubject record - future
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private string StandardSubject_Update( StandardSubject entity )
        {
            string message = "successful";

            try
            {

                #region parameters
                //replace following with actual nbr of parameters and do assignments
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@RowId", SqlDbType.UniqueIdentifier );
                sqlParameters[ 0 ].Size = 16;
                sqlParameters[ 0 ].Value = entity.RowId;

                //...

                #endregion

                SqlHelper.ExecuteNonQuery( ConnString, UPDATE_PROC, sqlParameters );
                message = "successful";

            }
            catch ( Exception ex )
            {
                LogError( ex, className + string.Format( ".Update() for RowId: {0} and LastUpdatedBy: {1}", entity.RowId.ToString(), entity.LastUpdatedBy ) );
                message = className + "- Unsuccessful: Update(): " + ex.Message.ToString();
                entity.Message = message;
                entity.IsValid = false;
            }

            return message;

        }//

        /// <summary>
        /// retrieve a standard subject
        /// </summary>
        /// <param name="pSubjectId"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public StandardSubject StandardSubject_Get( int pSubjectId, string title )
        {

            return StandardSubject_Get( 0, pSubjectId, title );
        }

        public StandardSubject StandardSubject_Get( int pId )
        {
            return StandardSubject_Get( pId, 0, "" );
        }

        private StandardSubject StandardSubject_Get( int pId, int pSubjectId, string title )
        {
            StandardSubject entity = new StandardSubject();
            SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
            sqlParameters[ 0 ] = new SqlParameter( "@Id", pId );
            sqlParameters[ 1 ] = new SqlParameter( "@SubjectId", pSubjectId );
            sqlParameters[ 2 ] = new SqlParameter( "@Title", title );

            DataSet ds = new DataSet();
            try
            {
                SqlDataReader dr = SqlHelper.ExecuteReader( ReadOnlyConnString, "[StandardBody.SubjectGet]", sqlParameters );

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
                LogError( ex, className + ".StandardSubject_Get() " );
                return null;

            }
        }
        /// <summary>
        /// Fill a StandardSubject object from a SqlDataReader
        /// </summary>
        /// <param name="dr">SqlDataReader</param>
        /// <returns>MyEntity</returns>
        public StandardSubject Fill( SqlDataReader dr )
        {
            StandardSubject entity = new StandardSubject();

            entity.IsValid = true;

            entity.Id = GetRowColumn( dr, "Id", 0 );
            entity.StandardBodyId = GetRowColumn( dr, "StandardBodyId", 0 );
            entity.Title = GetRowColumn( dr, "Title", "missing" );
            entity.Description = GetRowColumn( dr, "Description", "" );
            entity.Url = GetRowColumn( dr, "Url", "" );
            entity.Language = GetRowPossibleColumn( dr, "Language", "en" );
			entity.Created = GetRowPossibleColumn( dr, "Created", System.DateTime.MinValue );

            return entity;
        }//
        /// <summary>
        /// Select StandardSubject related data using passed parameters
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
		//private DataSet StandardSubject_Select( int pId, string parm2 )
		//{

		//	//replace following with actual nbr of parameters and do assignments
		//	SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
		//	sqlParameters[ 0 ] = new SqlParameter( "@Id", pId );
		//	sqlParameters[ 1 ] = new SqlParameter( "@parm2", parm2 );

		//	DataSet ds = new DataSet();
		//	try
		//	{
		//		ds = SqlHelper.ExecuteDataset( ReadOnlyConnString, CommandType.StoredProcedure, SELECT_PROC, sqlParameters );

		//		if ( ds.HasErrors )
		//		{
		//			return null;
		//		}
		//		return ds;
		//	}
		//	catch ( Exception ex )
		//	{
		//		LogError( ex, className + ".StandardSubject_Select() " );
		//		return null;

		//	}
		//}

        #endregion

        #region ====== Standard_SubjectStandardConnector Methods ===============================================
        public bool SubjectStandardConnector_Create( Standard_SubjectStandardConnector entity, ref string statusMessage )
        {
            bool isValid = true;

            try
            {
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                sqlParameters[ 0 ] = new SqlParameter( "@StandardSubjectId", entity.StandardSubjectId );
                sqlParameters[ 1 ] = new SqlParameter( "@DomainNodeId", entity.DomainNodeId );
                #endregion

                SqlDataReader dr = SqlHelper.ExecuteReader( ConnString, CommandType.StoredProcedure, "[StandardBody.SubjectStandardConnectorInsert]", sqlParameters );

                dr.Close();
                dr = null;
                statusMessage = "successful";

            }
            catch ( Exception ex )
            {
                LogError( ex, className + string.Format( ".SubjectStandardConnector_Create() for StandardSubjectId: {0} and DomainNodeId: {1}", entity.StandardSubjectId, entity.DomainNodeId ) );
                statusMessage = className + "- Unsuccessful: Create(): " + ex.Message.ToString();
                isValid = false;
            }
            return isValid;

        }//


        #endregion

        #region ====== StandardBody.Node Methods ===============================================
        public int StandardItem_Create( StandardItem entity, ref string statusMessage )
        {
            int newId = 0;

            try
            {
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 7 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ParentId", entity.ParentId );
                sqlParameters[ 1 ] = new SqlParameter( "@LevelType", entity.LevelType );
                sqlParameters[ 2 ] = new SqlParameter( "@NotationCode", entity.NotationCode );
                sqlParameters[ 3 ] = new SqlParameter( "@Description", entity.Description );
                sqlParameters[ 4 ] = new SqlParameter( "@StandardUrl", entity.StandardUrl );
                sqlParameters[ 5 ] = new SqlParameter( "@AltUrl", entity.AltUrl );
                sqlParameters[ 6 ] = new SqlParameter( "@StandardGuid", entity.StandardGuid );
                #endregion

                SqlDataReader dr = SqlHelper.ExecuteReader( ConnString, CommandType.StoredProcedure, "[StandardBody.NodeInsert]", sqlParameters );
                if ( dr.HasRows )
                {
                    dr.Read();
                    newId = int.Parse( dr[ 0 ].ToString() );
                    entity.Id = newId;
                }
                dr.Close();
                dr = null;
                statusMessage = "successful";

            }
            catch ( Exception ex )
            {
                LogError( ex, className + string.Format( ".StandardItem_Create() for LevelType: {0} and NotationCode: {1}", entity.LevelType.ToString(), entity.NotationCode ) );
                statusMessage = className + "- Unsuccessful: StandardItem_Create(): " + ex.Message.ToString();
                entity.Message = statusMessage;
                entity.IsValid = false;
            }

            return newId;
        }
        /// <summary>
        /// retrieve a standard item
        /// </summary>
        /// <param name="notationCode"></param>
        /// <returns></returns>
        public StandardItem StandardItem_GetByCode( string notationCode )
        {

            return StandardItem_Get( 0, "", notationCode, "" );
        }
        public StandardItem StandardItem_GetByUrl( string standardUrl)
        {

            return StandardItem_Get( 0, standardUrl, "", "" );
        }
        public StandardItem StandardItem_GetByAltUrl( string altUrl )
        {

            return StandardItem_Get( 0, "", "", altUrl );
        }
        public StandardItem StandardItem_Get( int pId )
        {
            return StandardItem_Get( pId, "", "", "" );
        }

        public StandardItem StandardItem_Get( int pId, string standardUrl, string notationCode, string altUrl )
        {
            StandardItem entity = new StandardItem();
            SqlParameter[] sqlParameters = new SqlParameter[ 4 ];
            sqlParameters[ 0 ] = new SqlParameter( "@Id", pId );
            sqlParameters[ 1 ] = new SqlParameter( "@StandardUrl", standardUrl );
            sqlParameters[ 2 ] = new SqlParameter( "@NotationCode", notationCode );
            sqlParameters[ 3 ] = new SqlParameter( "@AltUrl", altUrl );

            DataSet ds = new DataSet();
            try
            {
                SqlDataReader dr = SqlHelper.ExecuteReader( ReadOnlyConnString, "[StandardBody.NodeGet]", sqlParameters );

                if ( dr.HasRows )
                {
                    // it should return only one record.
                    while ( dr.Read() )
                    {
                        entity = StandardItem_Fill( dr );
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
                LogError( ex, className + ".StandardItem_Get() " );
                return null;

            }
        }

        /// <summary>
        /// Prototype for select, not sure what parms are needed as yet. A search may be better?
        /// </summary>
        /// <param name="pResourceIntId"></param>
        /// <returns></returns>
		//public List<StandardItem> StandardItem_Select( int pParentId )
		//{

		//	List<StandardItem> collection = new List<StandardItem>();

		//	SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
		//	sqlParameters[ 0 ] = new SqlParameter( "@ParentId", pParentId );

		//	DataSet ds = new DataSet();
		//	try
		//	{
		//		ds = SqlHelper.ExecuteDataset( ReadOnlyConnString, CommandType.StoredProcedure, "[StandardBody.NodeSelect]", sqlParameters );

		//		if ( DoesDataSetHaveRows( ds ) )
		//		{
		//			foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
		//			{
		//				StandardItem entity = StandardItem_Fill( dr );
		//				collection.Add( entity );
		//			}
		//		}
		//		return collection;
		//	}
		//	catch ( Exception ex )
		//	{
		//		LogError( ex, className + ".StandardItem_Select( int pResourceIntId ) " );
		//		return null;

		//	}
		//}
        public StandardItem StandardItem_Fill( SqlDataReader dr )
        {
            StandardItem entity = new StandardItem();

            entity.IsValid = true;

            entity.Id = GetRowColumn( dr, "Id", 0 );
            entity.ParentId = GetRowColumn( dr, "ParentId", 0 );
            entity.LevelType = GetRowColumn( dr, "LevelType", "" );
            entity.Description = GetRowColumn( dr, "Description", "" );
            entity.StandardUrl = GetRowColumn( dr, "StandardUrl", "" );
            entity.NotationCode = GetRowColumn( dr, "NotationCode", "" );
			//entity.Language = GetRowPossibleColumn( dr, "Language", "en" );
            entity.StandardGuid = GetRowColumn( dr, "StandardGuid", "" );
            entity.GradeLevels = GetRowPossibleColumn( dr, "GradeLevels", "" );

			//entity.Created = GetRowPossibleColumn( dr, "Created", System.DateTime.MinValue );

            //?????
           // entity.EducationLevelStart = GetRowColumn( dr, "EducationLevelStart", "" );
            //entity.EducationLevelEnd = GetRowColumn( dr, "EducationLevelEnd", "" );
            return entity;

        }//
        public StandardItem StandardItem_Fill( DataRow dr )
        {
            StandardItem entity = new StandardItem();

            entity.IsValid = true;

            entity.Id = GetRowColumn( dr, "Id", 0 );
            entity.ParentId = GetRowColumn( dr, "ParentId", 0 );
            entity.LevelType = GetRowColumn( dr, "LevelType", "" );
            entity.Description = GetRowColumn( dr, "Description", "" );
            entity.StandardUrl = GetRowColumn( dr, "StandardUrl", "" );
            entity.NotationCode = GetRowColumn( dr, "NotationCode", "" );
			//entity.Language = GetRowPossibleColumn( dr, "Language", "en" );
            entity.StandardGuid = GetRowColumn( dr, "StandardGuid", "" );
            entity.GradeLevels = GetRowPossibleColumn( dr, "GradeLevels", "" );

			//entity.Created = GetRowPossibleColumn( dr, "Created", System.DateTime.MinValue );

            //?????
            // entity.EducationLevelStart = GetRowColumn( dr, "EducationLevelStart", "" );
            //entity.EducationLevelEnd = GetRowColumn( dr, "EducationLevelEnd", "" );
            return entity;

        }//
        #endregion

        #region ====== StandardGradeLevel Methods ===============================================
        public int StandardGradeLevel_Create( int parentId, string gradeLevel, ref string statusMessage )
        {
            return StandardGradeLevel_Create( parentId, 0, gradeLevel, ref statusMessage );
        }
        public int StandardGradeLevel_Create( int parentId, int gradeLevelId, ref string statusMessage )
        {
            return StandardGradeLevel_Create( parentId, gradeLevelId, "", ref statusMessage );
        }
        public int StandardGradeLevel_Create( int parentId, int gradeLevelId, string gradeLevel, ref string statusMessage )
        {
            int newId = 0;

            try
            {
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ParentId", parentId );
                sqlParameters[ 1 ] = new SqlParameter( "@LevelType", gradeLevel );
                sqlParameters[ 2 ] = new SqlParameter( "@GradeLevelId", gradeLevelId );
                ;
                #endregion

                SqlDataReader dr = SqlHelper.ExecuteReader( ConnString, CommandType.StoredProcedure, "[StandardBody.NodeGradeLevelInsert]", sqlParameters );
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
                LogError( ex, className + string.Format( ".StandardGradeLevel_Create() for ParentId: {0} and gradeLevelId: {1} OR gradeLevel: {2}", parentId, gradeLevelId, gradeLevel ) );
                statusMessage = className + "- Unsuccessful: StandardGradeLevel_Create(): " + ex.Message.ToString();
            }

            return newId;
        }
        #endregion

    }
}