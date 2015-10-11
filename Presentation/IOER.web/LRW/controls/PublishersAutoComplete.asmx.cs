using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Services;

using ILPathways.Utilities;
using LogManager = ILPathways.Utilities.LoggingHelper;
using LRWarehouse.DAL;

namespace IOER.LRW.controls
{
    /// <summary>
    /// Summary description for PublishersAutoComplete
    /// </summary>
    [WebService( Namespace = "http://tempuri.org/" )]
    [WebServiceBinding( ConformsTo = WsiProfiles.BasicProfile1_1 )]
    [System.ComponentModel.ToolboxItem( false )]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class PublishersAutoComplete : System.Web.Services.WebService
    {
        string thisClassName= "PublishersAutoComplete";

        [WebMethod]
        public string[] GetPublishers( string prefixText, int count )
        {
            LoggingHelper.DoTrace( 4, "GetPublishers for " + prefixText );
            //int count = 10;
            string sql = "SELECT [Publisher], [Publisher]  + ' ( ' + convert(varchar, [ResourceTotal]) + ' ) ' As Combined FROM [dbo].[PublisherSummary] where IsActive = 1 And Publisher like @prefixText order by 1";

            LoggingHelper.DoTrace( 4, "GetPublishers for sql:" + sql );
            try
            {
                SqlDataAdapter da = new SqlDataAdapter( sql, BaseDataManager.GetReadOnlyConnection() );

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
                LogManager.LogError( ex, thisClassName + ".GetPublishers() - Unexpected error encountered while attempting search. " );
                return null;
            }

            
        }

        [WebMethod]
        public string[] GetCompletionList( string prefixText, int count )
        {
            LoggingHelper.DoTrace( 4, "GetCompletionList for " + prefixText );
            if ( count == 0 )
            {
                count = 10;
            }

            if ( prefixText.Equals( "xyz" ) )
            {
                return new string[ 0 ];
            }

            Random random = new Random();
            List<string> items = new List<string>( count );
            for ( int i = 0 ; i < count ; i++ )
            {
                char c1 = ( char ) random.Next( 65, 90 );
                char c2 = ( char ) random.Next( 97, 122 );
                char c3 = ( char ) random.Next( 97, 122 );

                items.Add( prefixText + c1 + c2 + c3 );
            }

            return items.ToArray();
        }
    }
}
