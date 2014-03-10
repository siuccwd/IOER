using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.ApplicationBlocks.Data;
using ILPathways.Business;

namespace ILPathways.DAL
{
    public class EmailNoticeManager : BaseDataManager
    {
        static string className = "EmailNoticeManager";

        #region Constants for database procedures
        /// <summary>
        /// Base procedures
        /// </summary>
        const string GET_PROC = "EmailNoticeGet";
        const string SELECT_PROC = "EmailNoticeSelect";
        const string DELETE_PROC = "EmailNoticeDelete";
        const string INSERT_PROC = "EmailNoticeInsert";
        const string UPDATE_PROC = "EmailNoticeUpdate";


        #endregion


        #region Constants for email notices
         //
        #endregion

        public EmailNoticeManager()
        { }//

        #region ====== Core Methods ===============================================

        /// <summary>
        /// Delete an event record
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public static bool Delete( int pid, ref string statusMessage )
        {
            bool successful;

            SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
            sqlParameters[ 0 ] = new SqlParameter( "@id", SqlDbType.Int );
            sqlParameters[ 0 ].Value = pid;

            try
            {
                SqlHelper.ExecuteNonQuery( ContentConnection(), CommandType.StoredProcedure, DELETE_PROC, sqlParameters );
                successful = true;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".Delete() " );
                statusMessage = "Unsuccessful: EmailNoticeManager.Delete(): " + ex.Message.ToString();

                successful = false;
            }
            return successful;
        }

        /// <summary>
        ///  Method to create EmailNotice object
        /// </summary>
        public static int Create( EmailNotice entity, ref string statusMessage )
        {

            string connectionString = ContentConnection();
            int newId = 0;

            try
            {
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 13 ];

                sqlParameters[ 0 ] = new SqlParameter( "@NoticeCode", entity.NoticeCode);
                sqlParameters[ 1 ] = new SqlParameter( "@Title", entity.Title);
                sqlParameters[ 2 ] = new SqlParameter( "@Description", entity.Description);
                sqlParameters[ 3 ] = new SqlParameter( "@Filter", entity.Filter);
                sqlParameters[ 4 ] = new SqlParameter( "@isActive", entity.IsActive);

                sqlParameters[ 5 ] = new SqlParameter( "@Category", entity.Category );

                sqlParameters[ 6 ] = new SqlParameter( "@FromEmail", entity.FromEmail);
                sqlParameters[ 7 ] = new SqlParameter( "@CcEmail", entity.CcEmail);
                sqlParameters[ 8 ] = new SqlParameter( "@BccEmail", entity.BccEmail);
                sqlParameters[ 9 ] = new SqlParameter( "@Subject", entity.Subject);

                sqlParameters[ 10 ] = new SqlParameter( "@HtmlBody", entity.HtmlBody);
                sqlParameters[ 11 ] = new SqlParameter( "@TextBody", entity.TextBody);
                sqlParameters[ 12 ] = new SqlParameter( "@CreatedById", entity.CreatedById);

                #endregion

                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, CommandType.StoredProcedure, INSERT_PROC, sqlParameters );
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
                statusMessage = "Unsuccessful: EmailNoticeManager.Create(): " + ex.Message.ToString();
                entity.Message = statusMessage;
                entity.IsValid = false;
            }

            return newId;

        }//

        /// <summary>
        ///  Method to update EmailNotice object
        /// </summary>
        public static string Update( EmailNotice entity )
        {
            string message = "successful";
            try
            {
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 13 ];
                sqlParameters[ 0 ] = new SqlParameter( "@id", entity.Id);

                sqlParameters[ 1 ] = new SqlParameter( "@Title", entity.Title );
                sqlParameters[ 2 ] = new SqlParameter( "@Description", entity.Description );
                sqlParameters[ 3 ] = new SqlParameter( "@Filter", entity.Filter );
                sqlParameters[ 4 ] = new SqlParameter( "@isActive", entity.IsActive );

                sqlParameters[ 5 ] = new SqlParameter( "@Category", entity.Category );

                sqlParameters[ 6 ] = new SqlParameter( "@FromEmail", entity.FromEmail );
                sqlParameters[ 7 ] = new SqlParameter( "@CcEmail", entity.CcEmail );
                sqlParameters[ 8 ] = new SqlParameter( "@BccEmail", entity.BccEmail );
                sqlParameters[ 9 ] = new SqlParameter( "@Subject", entity.Subject );

                sqlParameters[ 10 ] = new SqlParameter( "@HtmlBody", entity.HtmlBody);
                sqlParameters[ 11 ] = new SqlParameter( "@TextBody", entity.TextBody );

                sqlParameters[ 12 ] = new SqlParameter( "@LastUpdatedById", entity.LastUpdatedById);

                #endregion

                SqlHelper.ExecuteNonQuery( ContentConnection(), UPDATE_PROC, sqlParameters );
                message = "successful";

            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".Update(): " );
                message = "Unsuccessful: EmailNoticeManager.Update(): " + ex.Message.ToString();
                entity.Message = message;
                entity.IsValid = false;
            }

            return message;

        }//
        #endregion

        #region ====== Retrieval Methods ===============================================
        /// <summary>
        /// Get a single email notice record with an id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static EmailNotice Get( int id )
        {
            string lang = "";
            string emailNoticeCode = "";

            return Get( id, emailNoticeCode, lang );


        }//
        /// <summary>
        /// Get an EmailNotice record for the provided email notice code
        /// </summary>
        /// <param name="emailNoticeCode"></param>
        /// <returns></returns>
        public static EmailNotice Get( string emailNoticeCode )
        {
            string lang = "en";
            int id = 0;

            return Get( id, emailNoticeCode, lang );


        }//

        /// <summary>
        /// Get an EmailNotice record for the email code and language (while rare, we did have early cases where the same email notice code was used for en an es - then went away from this approach.
        /// </summary>
        /// <param name="emailNoticeCode">e-mail notice code</param>
        /// <param name="lang">Language code</param>
        /// <returns></returns>
        public static EmailNotice Get( string emailNoticeCode, string lang )
        {
            int id = 0;

            return Get( id, emailNoticeCode, lang );


        }//

        /// <summary>
        /// Get an EmailNotice record for the provided id or email code and language
        /// </summary>
        /// <param name="id">Record Id</param>
        /// <param name="emailNoticeCode">e-mail notice code</param>
        /// <param name="lang">Language code</param>
        /// <returns></returns>
        public static EmailNotice Get( int id, string emailNoticeCode, string lang )
        {
            EmailNotice entity = new EmailNotice();

            try
            {

                SqlParameter[] arParms = new SqlParameter[ 3 ];

                arParms[ 0 ] = new SqlParameter( "@id", id );
                arParms[ 1 ] = new SqlParameter( "@NoticeCode", emailNoticeCode );
                arParms[ 2 ] = new SqlParameter( "@LangCode", lang );

                SqlDataReader dr = SqlHelper.ExecuteReader( ContentConnectionRO(), GET_PROC, arParms );


                if ( dr.HasRows )
                {
                    // it should return only one record.
                    dr.Read();

                    entity.Id = GetRowColumn( dr, "id", 0 );
                    entity.Title = GetRowColumn( dr, "Title", "" );
                    entity.NoticeCode = GetRowColumn( dr, "NoticeCode", "" );
                    entity.Description = GetRowColumn( dr, "Description", "" );
                    entity.Filter = GetRowColumn( dr, "Filter", "" );
                    entity.IsActive = GetRowColumn( dr, "IsActive", true );

                    entity.Category = GetRowColumn( dr, "Category", "" );
                    entity.LanguageCode = GetRowColumn( dr, "LanguageCode", "" );

                    entity.FromEmail = GetRowColumn( dr, "fromEmail", "" );
                    if ( entity.FromEmail.Trim().Length == 0 )
                        entity.FromEmail = "info@illinoisworknet.com";

                    entity.CcEmail = GetRowColumn( dr, "CcEmail", "" );
                    entity.BccEmail = GetRowColumn( dr, "BccEmail", "" );
                    entity.Subject = GetRowColumn( dr, "Subject", "" );
                    entity.HtmlBody = GetRowColumn( dr, "HtmlBody", "" );
                    entity.TextBody = GetRowColumn( dr, "TextBody", "" );
                    entity.Created = GetRowColumn( dr, "Created", System.DateTime.MinValue );
                    entity.CreatedBy = GetRowColumn( dr, "CreatedBy", "" );
                    entity.LastUpdated = GetRowColumn( dr, "LastUpdated", System.DateTime.MinValue );
                    entity.LastUpdatedBy = GetRowColumn( dr, "LastUpdatedBy", "" );
                    entity.IsValid = true;

                }
                else
                {
                    entity.IsValid = false;
                }

                dr.Close();
                dr = null;
                return entity;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".Get() " );
                entity.Message = "Error in EmailNoticeManager.Get(): " + ex.Message;
                entity.IsValid = false;
                return entity;
            }

        }//

        /// <summary>
        /// Get all e-mail notices for passed category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public static DataSet Select( string category )
        {
            int isActive = 2;	//ALL
            string lang = "";

            return Select( category, isActive, lang );

        } //

        /// <summary>
        /// Select EmailNotices related data using passed parameters
        /// - Also adds a "Select an e-mail notice" row to the returned dataset
        /// </summary>
        /// <param name="category"></param>
        /// <param name="isActive"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public static DataSet Select( string category, int isActive, string lang )
        {
            string keyword = "";

            return Select( category, isActive, lang, keyword );
        } //

        /// <summary>
        /// Select EmailNotices related data using passed parameters
        /// - Also adds a "Select an e-mail notice" row to the returned dataset
        /// </summary>
        /// <param name="category"></param>
        /// <param name="isActive"></param>
        /// <param name="lang"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public static DataSet Select( string category, int isActive, string lang, string keyword )
        {
            string connectionString = ContentConnection();

            SqlParameter[] arParms = new SqlParameter[ 4 ];

            arParms[ 0 ] = new SqlParameter( "@Category", category );
            arParms[ 1 ] = new SqlParameter( "@IsActive", isActive );
            arParms[ 2 ] = new SqlParameter( "@LangCode", lang );
            arParms[ 3 ] = new SqlParameter( "@Keyword", keyword );

            DataSet ds = new DataSet();
            try
            {
                ds = SqlHelper.ExecuteDataset( connectionString, CommandType.StoredProcedure, SELECT_PROC, arParms );

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

        /// <summary>
        /// Get all e-mail notice distinct categories
        /// - Also adds a "Select a Category" row to the returned dataset
        /// </summary>
        /// <returns></returns>
        public static DataSet SelectCategories( int isActiveFilter )
        {

            try
            {
                string filter = "";
                if ( isActiveFilter > -1 && isActiveFilter < 2 )
                    filter = " where isActive = " + isActiveFilter + " ";

                string sqlDefault = String.Format( "SELECT Distinct Category FROM [EmailNotice] {0} order by Category", filter );

                DataSet ds = SqlHelper.ExecuteDataset( ContentConnectionRO(), System.Data.CommandType.Text, sqlDefault );

                if ( ds.HasErrors )
                {
                    return null;
                }

                return ds;

            }
            catch ( Exception e )
            {
                LogError( e, className + ".SelectCategories() " );
                return null;
            }

        } //
        #endregion

        #region ====== Speciality Methods ===============================================

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="emailNoticeCode"></param>
        /// <param name="maxRecords"></param>
        /// <param name="doingCountOnly"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public static int SendEmailsUsingSql( string sql, string emailNoticeCode, int maxRecords, bool doingCountOnly, ref string statusMessage )
        {

            int emailCount = 0;
            //int maxRecords = 9999;
            if ( maxRecords == 0 )
                maxRecords = 99999;

            int debug = 9;
            if ( doingCountOnly )
                debug = 15;

            SqlParameter[] sqlParameters = new SqlParameter[ 6 ];
            sqlParameters[ 0 ] = new SqlParameter( "@NoticeCode", SqlDbType.VarChar );
            sqlParameters[ 0 ].Size = 50;
            sqlParameters[ 0 ].Value = emailNoticeCode;
            sqlParameters[ 1 ] = new SqlParameter( "@EmailSql", SqlDbType.VarChar );
            sqlParameters[ 1 ].Value = sql;
            sqlParameters[ 2 ] = new SqlParameter( "@MaxRecords", SqlDbType.Int );
            sqlParameters[ 2 ].Value = maxRecords;
            sqlParameters[ 3 ] = new SqlParameter( "@Debug", SqlDbType.Int );
            sqlParameters[ 3 ].Value = debug;
            //not handling attachments here
            sqlParameters[ 4 ] = new SqlParameter( "@Attachments", SqlDbType.VarChar );
            sqlParameters[ 4 ].Value = "";
            sqlParameters[ 5 ] = new SqlParameter( "@EmailCount", SqlDbType.Int );
            sqlParameters[ 5 ].Direction = ParameterDirection.Output;

            try
            {
                SqlHelper.ExecuteNonQuery( ContentConnection(), CommandType.StoredProcedure, "SendEmail_UsingPassedSql", sqlParameters );

                string count = sqlParameters[ 5 ].Value.ToString();
                try
                {
                    emailCount = Int32.Parse( count );
                }
                catch
                {
                    emailCount = 0;
                }

                statusMessage = "successul";
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".SendEmailsUsingSql() " );
                statusMessage = "Unsuccessful: " + className + ".SendEmailsUsingSql(): " + ex.Message.ToString();

                emailCount = 0;
            }
            return emailCount;

        } //

        /// <summary>
        ///  - version for use with Fire and Forget pattern
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="emailNoticeCode"></param>
        /// <param name="maxRecords"></param>
        public static void SendEmailsUsingSql( string sql, string emailNoticeCode, int maxRecords )
        {

            int emailCount = 0;
            //int maxRecords = 9999;
            if ( maxRecords == 0 )
                maxRecords = 99999;

            int debug = 9;


            SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
            sqlParameters[ 0 ] = new SqlParameter( "@NoticeCode", SqlDbType.VarChar );
            sqlParameters[ 0 ].Size = 50;
            sqlParameters[ 0 ].Value = emailNoticeCode;
            sqlParameters[ 1 ] = new SqlParameter( "@EmailSql", SqlDbType.VarChar );
            sqlParameters[ 1 ].Value = sql;
            sqlParameters[ 2 ] = new SqlParameter( "@MaxRecords", SqlDbType.Int );
            sqlParameters[ 2 ].Value = maxRecords;
            sqlParameters[ 3 ] = new SqlParameter( "@Debug", SqlDbType.Int );
            sqlParameters[ 3 ].Value = debug;
            sqlParameters[ 4 ] = new SqlParameter( "@EmailCount", SqlDbType.Int );
            sqlParameters[ 4 ].Direction = ParameterDirection.Output;

            try
            {
                SqlHelper.ExecuteNonQuery( ContentConnection(), CommandType.StoredProcedure, "SendEmail_UsingPassedSql", sqlParameters );

                string count = sqlParameters[ 4 ].Value.ToString();
                try
                {
                    emailCount = Int32.Parse( count );
                }
                catch
                {
                    emailCount = 0;
                }

                //statusMessage = "successul";
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".SendEmailsUsingSql() " );
                //statusMessage = "Unsuccessful: " + className + ".SendEmailsUsingSql(): " + ex.Message.ToString();

                emailCount = 0;
            }
            //return emailCount;

        } //
        #endregion


    }//

}
