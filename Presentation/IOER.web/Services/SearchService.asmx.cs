using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;
using System.Web.Services.Protocols;
using AjaxControlToolkit;

using Isle.BizServices;
using ILPathways.Utilities;
using LRDAL = LRWarehouse.DAL;

namespace IOER.Services
{
    /// <summary>
    /// Summary description for CascadingDropDownsService
    /// </summary>
    [WebService( Namespace = "http://ilsharedlearning.org/" )]
    [WebServiceBinding( ConformsTo = WsiProfiles.BasicProfile1_1 )]
    [System.ComponentModel.ToolboxItem( false )]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class SearchService : System.Web.Services.WebService
    {
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        string className = "CascadingDropDownsService";
        public SearchService()
        {

            //Uncomment the following line if using designed components 
            //InitializeComponent(); 
        }

        #region Organizations
        [WebMethod]
        public string[] OrganzationsAutoComplete( string prefixText)
        {
            LoggingHelper.DoTrace( 4, "OrganzationsAutoComplete for " + prefixText );
            try 
            {
                string[] items = OrganizationBizService.OrganzationsAutoComplete(prefixText);
                
                return items;
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, className + ".OrganzationsAutoComplete() - Unexpected error encountered while attempting search. " );
                return null;
            }


        }

        [WebMethod]
        public string[] OrganzationsAutoCompleteOLD( string prefixText )
        {
            LoggingHelper.DoTrace( 4, "OrganzationsAutoComplete for " + prefixText );
            //int count = 10;
            string sql = "SELECT [Name], [Name]  + ' [' + convert(varchar, [Id]) + '] ' As Combined FROM  [Gateway].[dbo].[Organization] where [IsActive]= 1 And Name like @prefixText order by 1";

            LoggingHelper.DoTrace( 4, "OrganzationsAutoComplete for sql:" + sql );
            try
            {
                SqlDataAdapter da = new SqlDataAdapter( sql, GetGatewayConnectionString() );

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
                LoggingHelper.LogError( ex, className + ".OrganzationsAutoComplete() - Unexpected error encountered while attempting search. " );
                return null;
            }


        }
        /// <summary>
        /// Retrieve organizations via autocomplete
        /// </summary>
        /// <param name="prefixText"></param>
        /// <returns></returns>
        [WebMethod]
        public string[] OrganizationList( string prefixText )
        {

            string sql = "SELECT top 15  LocationLine + ' [' + convert(varchar,Id) + ']' As Combined FROM ActiveOrganizations  where LocationLine like @prefixText order by LocationLine";


            SqlDataAdapter da = new SqlDataAdapter( sql, GetGatewayConnectionString() );

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
        }//

        /// <summary>
        /// GetTopLevelOrganizations - shows all organizations without a parent
        /// </summary>
        /// <param name="knownCategoryValues"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        [WebMethod]
        public CascadingDropDownNameValue[] GetTopLevelOrganizations( string knownCategoryValues, string category )
        {
            SqlConnection con = new SqlConnection( GetGatewayConnectionString() );
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;
            cmd.CommandType = System.Data.CommandType.Text;

            cmd.CommandText = "SELECT Id, Name FROM [Gateway].[dbo].[Organization] where [IsActive]= 1 And ParentId is null order by 2";
            List<CascadingDropDownNameValue> list = new List<CascadingDropDownNameValue>();

            try
            {
                SqlDataAdapter dAdapter = new SqlDataAdapter();
                dAdapter.SelectCommand = cmd;
                con.Open();

                DataSet objDs = new DataSet();
                dAdapter.Fill( objDs );
                con.Close();


                foreach ( DataRow dRow in objDs.Tables[ 0 ].Rows )
                {
                    string id = dRow[ "Id" ].ToString();
                    string title = dRow[ "Name" ].ToString();
                    list.Add( new CascadingDropDownNameValue( title, id ) );
                }
                return list.ToArray();
            }
            catch ( Exception ex )
            {
                string id = "0";
                string title = ex.Message;
                list.Add( new CascadingDropDownNameValue( title, id ) );
                return list.ToArray();
            }

        }


        /// <summary>
        /// GetChildOrganizations 
        /// </summary>
        /// <param name="knownCategoryValues"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        [WebMethod]
        public CascadingDropDownNameValue[] GetSubjectGradeLevels( string knownCategoryValues, string category )
        {
            string sql = "SELECT Id, Name FROM [Gateway].[dbo].[Organization] where [IsActive]= 1 And ParentId = @ParentId order by 2";
            List<CascadingDropDownNameValue> list = new List<CascadingDropDownNameValue>();
                       
            try
            {
                StringDictionary parentValues = AjaxControlToolkit.CascadingDropDown.ParseKnownCategoryValuesString( knownCategoryValues );
                string parentId = parentValues[ "Id" ];

                DataSet objDs = GetData( sql, parentId, "@ParentId" );


                foreach ( DataRow dRow in objDs.Tables[ 0 ].Rows )
                {
                    string id = dRow[ "Id" ].ToString();
                    string title = dRow[ "Name" ].ToString();
                    list.Add( new CascadingDropDownNameValue( title, id ) );
                }
                return list.ToArray();
            }
            catch ( Exception ex )
            {
                string id = "0";
                string title = ex.Message;
                list.Add( new CascadingDropDownNameValue( title, id ) );
                return list.ToArray();
            }
        }//

        #endregion


        #region Common
        /// <summary>
        /// GetData - retrieve data for list using passed sql and parameters
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parentId"></param>
        /// <param name="parentIdParm"></param>
        /// <returns></returns>
        private DataSet GetData( string sql, string parentId, string parentIdParm )
        {

            SqlConnection con = new SqlConnection( GetLRWConnectionString() );
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.Parameters.AddWithValue( parentIdParm, parentId );
            cmd.CommandText = sql;


            SqlDataAdapter dAdapter = new SqlDataAdapter();
            dAdapter.SelectCommand = cmd;
            con.Open();
            DataSet objDs = new DataSet();
            dAdapter.Fill( objDs );
            con.Close();

            return objDs;
        }//

        /// <summary>
        /// Retrieve database connection string for SqlQuery table
        /// </summary>
        /// <returns>Connection string</returns>
        private static string GetGatewayConnectionString()
        {
            return OrganizationBizService.OrganzationConnection();

        }//
        private static string GetLRWConnectionString()
        {
            return LRDAL.BaseDataManager.LRWarehouseRO(); ;

        }//

        #endregion


    }
}
