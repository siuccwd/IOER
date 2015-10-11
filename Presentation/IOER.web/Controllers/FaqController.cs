using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using EmailHelper = ILPathways.Utilities.EmailManager;

using ILPathways.Business;
using IOER.classes;
using ILPathways.DAL;
using IOER.Library;
using ILPathways.Utilities;

using wsItem = IOER.FaqServiceReference;

namespace IOER.Controllers
{
    public class FaqController : BaseUserControl
    {

        string className = "FaqController";
        static string workNetItemsPassword = "Achtung!SieVerlassenJetztWest-Berlin";
        #region CORE

        /// <summary>
        /// Post a question for referrenced faq category
        /// </summary>
        /// <param name="question"></param>
        /// <param name="category"></param>
        /// <param name="createdBy"></param>
        public bool PostQuestion( string question, string category, string createdByEmail, ref string statusMessage )
        {
            bool isValid = true;
            string defaultTargetPathway = "3";

            wsItem.FaqServiceSoapClient wsClient = new wsItem.FaqServiceSoapClient();
            string message = wsClient.PostQuestion( question, category, createdByEmail, defaultTargetPathway, workNetItemsPassword );
            if ( message != null && message.Length == 36 )
            {

            }
            else
            {
                //error - message??
                isValid = false;
                statusMessage = message;
            }
            return isValid;
        }

        public bool GetQuestion( string questionId, ref string statusMessage )
        {
            bool isValid = true;

            wsItem.FaqServiceSoapClient wsClient = new wsItem.FaqServiceSoapClient();
            string status = wsClient.ConfirmQuestion( questionId, workNetItemsPassword );
            //??
            if ( status != null && status == "successful" )
            {

            }
            else
            {
                //error - message??
                isValid = false;
                statusMessage = status;
            }
            return isValid;
        }
        public bool ConfirmQuestion( string questionId, ref string statusMessage )
        {
            bool isValid = true;

            wsItem.FaqServiceSoapClient wsClient = new wsItem.FaqServiceSoapClient();
            string status = wsClient.ConfirmQuestion( questionId, workNetItemsPassword );
            //??
            if ( status != null && status == "successful" )
            {

            }
            else
            {
                //error - message??
                isValid = false;
                statusMessage = status;
            }
            return isValid;
        }
        /// <summary>
        /// Delete an FaqItem record
        /// </summary>
        /// <param name="pRowId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool Delete( string pRowId, ref string statusMessage )
        {
            bool successful = false;
            //string connectionString = GetMainDBConnection();

            //SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
            //sqlParameters[ 0 ] = new SqlParameter( "@RowId", SqlDbType.UniqueIdentifier );
            //sqlParameters[ 0 ].Value = new Guid( pRowId );

            //try
            //{
            //    SqlHelper.ExecuteNonQuery( connectionString, CommandType.StoredProcedure, DELETE_PROC, sqlParameters );
            //    successful = true;
            //}
            //catch ( Exception ex )
            //{
            //    LogError( ex, className + ".Delete() " );
            //    statusMessage = className + "- Unsuccessful: Delete(): " + ex.Message.ToString();

            //    successful = false;
            //}
            return successful;
        }//

        /// <summary>
        /// Add an FaqItem record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public string Create( FaqItem entity, ref string statusMessage )
        {
            string newId = "";
            //string connectionString = GetMainDBConnection();

            //#region parameters
            //SqlParameter[] sqlParameters = new SqlParameter[ 12 ];
            //sqlParameters[ 0 ] = new SqlParameter( "@SequenceNbr", SqlDbType.Int );
            //sqlParameters[ 0 ].Value = entity.SequenceNbr;

            //sqlParameters[ 1 ] = new SqlParameter( "@FaqCode", entity.FaqCode );
            //sqlParameters[ 2 ] = new SqlParameter( "@Title", entity.Title );

            //sqlParameters[ 3 ] = new SqlParameter( "@Description", entity.Description );

            //sqlParameters[ 4 ] = new SqlParameter( "@CategoryId", entity.CategoryId );
            //sqlParameters[ 5 ] = new SqlParameter( "@SubcategoryId", entity.SubcategoryId );
            //sqlParameters[ 6 ] = new SqlParameter( "@Status", entity.Status );

            //sqlParameters[ 7 ] = new SqlParameter( "@IsActive", SqlDbType.Bit );
            //sqlParameters[ 7 ].Value = entity.IsActive;

            //sqlParameters[ 8 ] = new SqlParameter( "@ExpiryDate", SqlDbType.DateTime );
            //if ( entity.ExpiryDate == DateTime.MinValue )
            //    sqlParameters[ 8 ].Value = DBNull.Value;
            //else
            //    sqlParameters[ 8 ].Value = entity.ExpiryDate;

            //sqlParameters[ 9 ] = new SqlParameter( "@ImageId", SqlDbType.Int );
            //sqlParameters[ 9 ].Value = entity.ImageId;

            //sqlParameters[ 10 ] = new SqlParameter( "@CreatedById", SqlDbType.Int );
            //sqlParameters[ 10 ].Value = entity.CreatedById;

            //sqlParameters[ 11 ] = new SqlParameter( "@CreatedBy", entity.CreatedBy );

            //#endregion

            //try
            //{
            //    SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, CommandType.StoredProcedure, INSERT_PROC, sqlParameters );
            //    if ( dr.HasRows )
            //    {
            //        dr.Read();
            //        newId = dr[ 0 ].ToString();
            //    }
            //    dr.Close();
            //    dr = null;
            //    statusMessage = "successful";

            //}
            //catch ( Exception ex )
            //{
            //    LogError( ex, className + ".Create() " );
            //    statusMessage = className + "- Unsuccessful: Create(): " + ex.Message.ToString();
            //    entity.Message = statusMessage;
            //    entity.IsValid = false;
            //}

            return newId;
        }

        		/// <summary>
		/// /// Update an FaqItem record
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public string Update( FaqItem entity )
		{
			string message = "successful";


			#region parameters
			SqlParameter[] sqlParameters = new SqlParameter[ 11 ];
			sqlParameters[ 0 ] = new SqlParameter( "@RowId", SqlDbType.VarChar );
			sqlParameters[ 0 ].Size = 16;
			sqlParameters[ 0 ].Value = entity.RowId;

			sqlParameters[ 1 ] = new SqlParameter( "@SequenceNbr", entity.SequenceNbr );
			sqlParameters[ 2 ] = new SqlParameter( "@Title", entity.Title );

			sqlParameters[ 3 ] = new SqlParameter( "@Description", SqlDbType.VarChar );
			sqlParameters[ 3 ].Size = -1;
			sqlParameters[ 3 ].Value = entity.Description;

			sqlParameters[ 4 ] = new SqlParameter( "@CategoryId", entity.CategoryId );
			sqlParameters[ 5 ] = new SqlParameter( "@SubcategoryId", entity.SubcategoryId );
			sqlParameters[ 6 ] = new SqlParameter( "@Status", entity.Status );

			sqlParameters[ 7 ] = new SqlParameter( "@IsActive", SqlDbType.Bit );
			sqlParameters[ 7 ].Value = entity.IsActive;

			sqlParameters[ 8 ] = new SqlParameter( "@ExpiryDate", SqlDbType.DateTime );
			if ( entity.ExpiryDate == DateTime.MinValue )
				sqlParameters[ 8 ].Value = DBNull.Value;
			else
				sqlParameters[ 8 ].Value = entity.ExpiryDate;

			sqlParameters[ 9 ] = new SqlParameter( "@ImageId", SqlDbType.Int );
			sqlParameters[ 9 ].Value = entity.ImageId;

			sqlParameters[ 10 ] = new SqlParameter( "@LastUpdatedById", SqlDbType.Int );
			sqlParameters[ 10 ].Value = entity.LastUpdatedById;

			//sqlParameters[ 11 ] = new SqlParameter( "@PathwayId", SqlDbType.Int );
			//sqlParameters[ 11 ].Value = entity.PathwayId;
			#endregion

			try
			{
                //SqlHelper.ExecuteNonQuery( GetMainDBConnection(), UPDATE_PROC, sqlParameters );
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
		#endregion


        #region ====== Retrieval Methods ===============================================
        public DataSet SelectByCategory( string category )
        {
            string pOrderBy = "";
            int pTotalRows = 0;
            int pStartPageIndex = 1;
            int pMaximumRows = 100;

            return  SelectByCategory( category, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
        }


        public DataSet SelectByCategory( string category, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {


            try
            {
                wsItem.FaqServiceSoapClient wsClient = new wsItem.FaqServiceSoapClient();
                DataSet ds = wsClient.FaqsByCategory( category, "" );
                return ds;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".SelectByCategory() " );
                return null;

            }
        }

        /// <summary>
        /// Get FaqItem record
        /// </summary>
        /// <param name="pRowId"></param>
        /// <returns>FaqItem</returns>
        public FaqItem Get( string pRowId )
        {
            FaqItem entity = new FaqItem();

            try
            {
                //string connectionString = GetReadOnlyConnection();
                //SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                //sqlParameters[ 0 ] = new SqlParameter( "@RowId", pRowId );
                //sqlParameters[ 1 ] = new SqlParameter( "@FaqCode", "" );

                //SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, GET_PROC, sqlParameters );

                //if ( dr.HasRows )
                //{
                //    // it should return only one record.
                //    while ( dr.Read() )
                //    {
                //        entity = Fill( dr );
                //    }

                //}
                //else
                //{
                //    entity.Message = "Record not found";
                //    entity.IsValid = false;
                //}
                //dr.Close();
                //dr = null;
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

        public FaqItem GetByCode( string pFaqCode )
        {
            FaqItem entity = new FaqItem();


            try
            {
                //string connectionString = GetReadOnlyConnection();
                //SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                //sqlParameters[ 0 ] = new SqlParameter( "@RowId", "" );
                //sqlParameters[ 1 ] = new SqlParameter( "@FaqCode", pFaqCode );

                //SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, GET_PROC, sqlParameters );

                //if ( dr.HasRows )
                //{
                //    // it should return only one record.
                //    while ( dr.Read() )
                //    {
                //        entity = Fill( dr );
                //    }

                //}
                //else
                //{
                //    entity.Message = "Record not found";
                //    entity.IsValid = false;
                //}
                //dr.Close();
                //dr = null;
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
        /// Search for FAQ related data using passed parameters
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

            try
            {
                wsItem.FaqServiceSoapClient wsClient = new wsItem.FaqServiceSoapClient();
                DataSet ds = wsClient.Search( pFilter, pOrderBy, pStartPageIndex, pMaximumRows );
                if ( DatabaseManager.DoesDataSetHaveRows( ds ) ) 
                {
                    pTotalRows = ds.Tables[ 0 ].Rows.Count;
                    return ds;
                } else 
                {
                    pTotalRows = 0;
                    return null;
                }
                
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".Search() " );
                return null;

            }
        }


        /// <summary>
        /// Update # of Hits on a FAQ record
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        public bool UpdateHits( string pRowId )
        {
            bool successful = false;
            //string connectionString = GetMainDBConnection();

            //SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
            //sqlParameters[ 0 ] = new SqlParameter( "@RowId", SqlDbType.VarChar );
            //sqlParameters[ 0 ].Value = pRowId;

            //try
            //{
            //    SqlHelper.ExecuteNonQuery( connectionString, CommandType.StoredProcedure, "Faq_UpdateHits", sqlParameters );
            //    successful = true;
            //}
            //catch ( Exception ex )
            //{
            //    LogError( ex, className + ".Hits() " );
            //    successful = false;
            //}
            return successful;
        }//

        #endregion


        #region ====== FaqCategory Methods ===============================================
        /// <summary>
        /// Return distinct categories used for the pathway
        /// </summary>
        /// <param name="pPathwayId"></param>
        /// <param name="pPathway"></param>
        /// <returns></returns>
        public DataSet FAQ_PathwayCategoriesSelect( int pPathwayId, string pPathway )
        {
          
            SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
            sqlParameters[ 0 ] = new SqlParameter( "@PathwayId", pPathwayId );
            sqlParameters[ 1 ] = new SqlParameter( "@Pathway", pPathway );


            DataSet ds = new DataSet();
            try
            {
                //ds = SqlHelper.ExecuteDataset( GetReadOnlyConnection(), CommandType.StoredProcedure, "[FAQ.PathwayCategoriesSelect]", sqlParameters );

                //if ( ds.HasErrors )
                //{
                //    return null;
                //}
                return ds;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".FAQ_PathwayCategoriesSelect() " );
                return null;

            }
        }
        #endregion

        #region ====== FaqSubCategory Methods ===============================================

        /// <summary>
        /// Return distinct subcategories used for the pathway
        /// </summary>
        /// <param name="pPathwayId"></param>
        /// <param name="pPathway"></param>
        /// <returns></returns>
        public DataSet FAQ_PathwaySubCategoriesSelect( int pPathwayId, string pPathway )
        {

            SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
            sqlParameters[ 0 ] = new SqlParameter( "@PathwayId", pPathwayId );
            sqlParameters[ 1 ] = new SqlParameter( "@Pathway", pPathway );


            DataSet ds = new DataSet();
            try
            {
                //ds = SqlHelper.ExecuteDataset( GetReadOnlyConnection(), CommandType.StoredProcedure, "[FAQ.PathwaySubCategoriesSelect]", sqlParameters );

                //if ( ds.HasErrors )
                //{
                //    return null;
                //}
                return ds;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".FAQ_PathwaySubCategoriesSelect() " );
                return null;

            }
        }
        /// Get subcategories and description for the provided category
        /// </summary>
        /// <param name="pCategory"></param>
        /// <returns></returns>
        public DataSet FAQSubcategories_Select( int pPathwayId, int pCategoryId )
        {
            string pPathway = "";
            string pCategory = "";
            return FAQSubcategories_Select( pPathwayId, pCategoryId, pPathway, pCategory );
        }

        /// <summary>
        /// Get subcategories and description for the provided category
        /// </summary>
        /// <param name="pCategory"></param>
        /// <returns></returns>
        public DataSet FAQSubcategories_Select( string pPathway, string pCategory )
        {
            int pPathwayId = 0;
            int pCategoryId = 0;
            return FAQSubcategories_Select( pPathwayId, pCategoryId, pPathway, pCategory );

        }
        public DataSet FAQSubcategories_Select( int pPathwayId, int pCategoryId, string pPathway, string pCategory )
        {
            
            SqlParameter[] sqlParameters = new SqlParameter[ 4 ];
            sqlParameters[ 0 ] = new SqlParameter( "@PathwayId", SqlDbType.Int );
            sqlParameters[ 0 ].Value = pPathwayId;

            sqlParameters[ 1 ] = new SqlParameter( "@CategoryId", SqlDbType.Int );
            sqlParameters[ 1 ].Value = pCategoryId;

            sqlParameters[ 2 ] = new SqlParameter( "@Pathway", SqlDbType.VarChar );
            sqlParameters[ 2 ].Size = 50;
            sqlParameters[ 2 ].Value = pPathway;

            sqlParameters[ 2 ] = new SqlParameter( "@Category", SqlDbType.VarChar );
            sqlParameters[ 2 ].Size = 50;
            sqlParameters[ 2 ].Value = pCategory;

            DataSet ds = new DataSet();
            try
            {
                //ds = SqlHelper.ExecuteDataset( GetReadOnlyConnection(), CommandType.StoredProcedure, "FAQSubcategory_Select", sqlParameters );

                //if ( ds.HasErrors )
                //{
                //    return null;
                //}
                return ds;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".FAQSubcategories2_Select() " );
                return null;

            }
        }
        #endregion


        #region ====== Helper Methods ===============================================
        /// <summary>
        /// Fill an FAQ object from a SqlDataReader
        /// </summary>
        /// <param name="dr">SqlDataReader</param>
        /// <returns>FaqItem</returns>
        private FaqItem Fill( SqlDataReader dr )
        {

            FaqItem entity = new FaqItem();

            //string rowId = GetRowColumn( dr, "RowId", "" );
            //entity.RowId = new Guid( rowId );

            //entity.Id = GetRowColumn( dr, "Id", 0 );
            //entity.SequenceNbr = GetRowColumn( dr, "SequenceNbr", 0 );
            //entity.FaqCode = GetRowColumn( dr, "FaqCode", "" );
            //entity.Title = GetRowColumn( dr, "Title", "" );
            //entity.Description = GetRowColumn( dr, "Description", "" );
            ////entity.PathwayId = GetRowColumn( dr, "PathwayId", 0 );
            ////entity.SitePathName = GetRowColumn( dr, "SitePathName", "" );

            //entity.CategoryId = GetRowColumn( dr, "CategoryId", 0 );
            //entity.Category = GetRowColumn( dr, "Category", "" );
            //entity.SubcategoryId = GetRowColumn( dr, "SubcategoryId", 0 );
            //entity.Subcategory = GetRowColumn( dr, "Subcategory", "" );

            //entity.Status = GetRowColumn( dr, "Status", "" );
            //entity.IsActive = GetRowColumn( dr, "IsActive", false );
            //entity.ExpiryDate = GetRowColumn( dr, "ExpiryDate", System.DateTime.MinValue );
            //entity.ImageId = GetRowColumn( dr, "ImageId", 0 );
            //if ( entity.ImageId > 0 )
            //{
            //    entity.AppItemImage = ImageStoreManager.Get( entity.ImageId );
            //}
            //entity.Hits = GetRowColumn( dr, "Hits", 0 );
            //entity.Created = GetRowColumn( dr, "Created", System.DateTime.MinValue );
            //entity.CreatedBy = GetRowColumn( dr, "CreatedBy", "" );
            //entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );

            //entity.LastUpdated = GetRowColumn( dr, "LastUpdated", System.DateTime.MinValue );
            //entity.LastUpdatedById = GetRowColumn( dr, "LastUpdatedById", 0 );
            //entity.LastUpdatedBy = GetRowColumn( dr, "LastUpdatedBy", "" );

            return entity;
        }//

        #endregion


        #region ====== Faq.FaqPathway Methods ===============================================
        /// <summary>
        /// Adds/deletes Faq.FaqPathway entries (only actual changes, rather than all)
        /// </summary>
        /// <param name="pUserID"></param>
        /// <param name="pNewSelectedItems"></param>
        /// <param name="pUnSelectedItems"></param>
        /// <returns></returns>
        public bool FaqPathway_ApplyChanges( Guid pFaqRowId,
                                                                int pCreatedById,
                                                                string pNewSelectedItems,
                                                                string pUnSelectedItems )
        {
            string statusMessage = "";
            try
            {
                int counter = 0;

                foreach ( string newItem in pNewSelectedItems.Split( '|' ) )
                {
                    if ( newItem.Length > 0 )
                    {
                        int pPathwayId = Int32.Parse( newItem );
                        FaqPathway_Insert( pFaqRowId, pPathwayId, ref statusMessage );
                        counter++;
                    }
                }

                foreach ( string removedItem in pUnSelectedItems.Split( '|' ) )
                {
                    if ( removedItem.Length > 0 )
                    {
                        int pPathwayId = Int32.Parse( removedItem );
                        SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                        sqlParameters[ 0 ] = new SqlParameter( "@FaqRowId", SqlDbType.UniqueIdentifier );
                        sqlParameters[ 0 ].Value = pFaqRowId;
                        sqlParameters[ 1 ] = new SqlParameter( "@PathwayId", SqlDbType.Int );
                        sqlParameters[ 1 ].Value = pPathwayId;

                        //SqlHelper.ExecuteNonQuery( GetVosDBCon(), CommandType.StoredProcedure, "[Faq.FaqPathwayDelete]", sqlParameters );

                        counter++;
                    }

                }

                return true;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".FaqPathway_ApplyChanges() " );
                return false;
            }
        }
        /// <summary>
        /// Delete *ALL* FaqPathway records for Faq record using rowId
        /// </summary>
        /// <param name="pRowId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool FaqPathway_Delete( Guid pFaqRowId, ref string statusMessage )
        {

            bool successful;

            SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
            sqlParameters[ 0 ] = new SqlParameter( "@FaqRowId", SqlDbType.UniqueIdentifier );
            sqlParameters[ 0 ].Value = pFaqRowId;
            sqlParameters[ 1 ] = new SqlParameter( "@PathwayId", SqlDbType.Int );
            sqlParameters[ 1 ].Value = 0;

            try
            {
                //SqlHelper.ExecuteNonQuery( GetVosDBCon(), CommandType.StoredProcedure, "[Faq.FaqPathwayDelete]", sqlParameters );
                successful = true;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".FaqPathway_Delete() " );
                statusMessage = className + "- FaqPathway_Delete: Delete(): " + ex.Message.ToString();

                successful = false;
            }
            return successful;
        }//

        /// <summary>
        /// Add an FaqPathway record
        /// </summary>
        /// <param name="pFaqRowId"></param>
        /// <param name="pPathwayId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool FaqPathway_Insert( Guid pFaqRowId, int pPathwayId, ref string statusMessage )
        {

            bool isValid = false;

            try
            {

                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                sqlParameters[ 0 ] = new SqlParameter( "@FaqRowId", SqlDbType.UniqueIdentifier );
                sqlParameters[ 0 ].Value = pFaqRowId;

                sqlParameters[ 1 ] = new SqlParameter( "@PathwayId", SqlDbType.Int );
                sqlParameters[ 1 ].Value = pPathwayId;

                //sqlParameters[ 2 ] = new SqlParameter( "@CreatedById", SqlDbType.Int );
                //sqlParameters[ 2 ].Value = pCreatedById;

                #endregion

                //SqlHelper.ExecuteNonQuery( GetVosDBCon(), CommandType.StoredProcedure, "[Faq.FaqPathwayInsert]", sqlParameters );

                isValid = true;
                statusMessage = "successful";

            }
            catch ( Exception ex )
            {
                LogError( ex, className + string.Format( ".FaqPathway_Insert() for RowId: {0} and PathwayId: {1} ", pFaqRowId.ToString(), pPathwayId.ToString() ) );
                statusMessage = className + "- Unsuccessful: FaqPathway_Insert(): " + ex.Message.ToString();

            }

            return isValid;
        }

        /// <summary>
        /// get all AppPathway records and inner join to Faq.FaqPathway to indicate where selected
        /// </summary>
        /// <param name="pFaqRowId"></param>
        /// <returns></returns>
        public DataSet FaqPathway_Select( Guid pFaqRowId )
        {
            SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
            sqlParameters[ 0 ] = new SqlParameter( "@FaqRowId", SqlDbType.UniqueIdentifier );
            sqlParameters[ 0 ].Value = pFaqRowId;
            DataSet ds = new DataSet();
            //ds = SqlHelper.ExecuteDataset( GetVosDBCon(), "[Faq.FaqPathwaySelect]", sqlParameters );

            return ds;
        }

        /// <summary>
        /// Return all the faqs for a path
        /// </summary>
        /// <param name="pPathwayId"></param>
        /// <param name="pPathway"></param>
        /// <returns></returns>
        public DataSet FAQ_PathwayFaqsSelect( int pPathwayId, string pPathway )
        {

            SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
            sqlParameters[ 0 ] = new SqlParameter( "@PathwayId", pPathwayId );
            sqlParameters[ 1 ] = new SqlParameter( "@Pathway", pPathway );


            DataSet ds = new DataSet();
            try
            {
                //ds = SqlHelper.ExecuteDataset( GetReadOnlyConnection(), CommandType.StoredProcedure, "[FAQ.PathwayFaqsSelect]", sqlParameters );

                if ( ds.HasErrors )
                {
                    return null;
                }
                return ds;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".FAQ_PathwayFaqsSelect() " );
                return null;

            }
        }
        #endregion

    }
}