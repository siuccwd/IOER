using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.DAL;
using System.Data;
using System.IO;
using System.Web.Script.Serialization;

namespace ILPathways.testing
{
    public partial class StandardsJSONbuilder : System.Web.UI.Page
    {
        protected void Page_Load( object sender, EventArgs e )
        {
            if ( 1 == 2 )
            {
                DWResources();
            }
            else
            {
                DataSet dsMath = DatabaseManager.DoQuery( "SELECT * FROM [Isle_IOER].[dbo].[StandardList] WHERE [Id] <= 694 OR ( [Id] >= 1778 AND [Id] <= 2481 )" );
                DataSet dsELA = DatabaseManager.DoQuery( "SELECT * FROM [Isle_IOER].[dbo].[StandardList] WHERE [Id] > 694 AND [Id] <= 1778" );
                //DataSet dsNGSS = DatabaseManager.DoQuery( "SELECT * FROM [Isle_IOER].[dbo].StandardList] WHERE [Id] >= 2500" );
                BuildStandards( dsMath, "ccssMath", "ccssMathJSON.js" );
                BuildStandards( dsELA, "ccssELA", "ccssELAJSON.js" );
            }
        }

        public void DWResources()
        {
            //***** OBSOLETE - NOW USING ADMIN/DataDump.aspx ****************************************
            DataSet ds = DatabaseManager.DoQuery( "SELECT Id, ResourceUrl, Title, [Description],isnull(DisabilityCategory,'') As DisabilityCategory, isnull(ServiceCategory,'') As ServiceCategory, isnull(Region, '') As Region, isnull(EndUser, '') As EndUser, isnull(MediaType, '') As MediaType, isnull(Keywords, '') As Keywords, ApplicationPath, Created  FROM [workNet2013].[dbo].[Resource] order by 2" );
            string varName = "whatever";
            string fileName = "whatever";
            if ( DatabaseManager.DoesDataSetHaveRows( ds ) == false )
            {
                message.Text = "NOT FOUND";
            }
            else
            {
                message.Text = "FOUND: " + ds.Tables[0].Rows.Count.ToString();

                List<DWResource> list = new List<DWResource>();
                foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                {
                    DWResource json = new DWResource();
                    json.id = int.Parse( DatabaseManager.GetRowPossibleColumn( dr, "Id" ) );
                    
                    json.title = DatabaseManager.GetRowPossibleColumn( dr, "Title" );
                    message.Text += "<br/>" + json.title;
                    json.description = DatabaseManager.GetRowPossibleColumn( dr, "Description" );
                    json.path = DatabaseManager.GetRowPossibleColumn( dr, "ApplicationPath" );
                    json.url = DatabaseManager.GetRowPossibleColumn( dr, "ResourceUrl" );
                    json.created = DatabaseManager.GetRowPossibleColumn( dr, "Created" );

                    json.disabilityCategory = DatabaseManager.GetRowPossibleColumn( dr, "DisabilityCategory" ).Split( ',' );
                    json.serviceCategory = DatabaseManager.GetRowPossibleColumn( dr, "ServiceCategory" ).Split( ',' );
                    json.region = DatabaseManager.GetRowPossibleColumn( dr, "Region" ).Split( ',' );

                    json.keywords = DatabaseManager.GetRowPossibleColumn( dr, "Keywords" ).Split( ',' );
                    json.endUsers = DatabaseManager.GetRowPossibleColumn( dr, "EndUser" ).Split( ',' );
                    json.mediaTypes = DatabaseManager.GetRowPossibleColumn( dr, "MediaType" ).Split( ',' );
                    list.Add( json );
                }
                string output = "var " + varName + " = " + new JavaScriptSerializer().Serialize( list ) + ";";
               // File.WriteAllText( @"C:\elasticSearchJSON\" + fileName, output );
            }
        }

        public void BuildStandards( DataSet ds, string varName, string fileName )
        {
            List<StandardJSON> list = new List<StandardJSON>();
            foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
            {
                StandardJSON json = new StandardJSON();
                json.id = int.Parse( DatabaseManager.GetRowPossibleColumn( dr, "Id" ) );
                try { json.parent = int.Parse( DatabaseManager.GetRowPossibleColumn( dr, "parentId" ) ); }
                catch { json.parent = null; }
                json.code = DatabaseManager.GetRowPossibleColumn( dr, "notationCode" );
                json.url = DatabaseManager.GetRowPossibleColumn( dr, "url" );
                json.grades = DatabaseManager.GetRowPossibleColumn( dr, "gradeLevel" ).Split( ',' );
                json.text = DatabaseManager.GetRowPossibleColumn( dr, "description" );
                list.Add( json );
            }
            string output = "var " + varName + " = " + new JavaScriptSerializer().Serialize( list ) + ";";
            File.WriteAllText( @"C:\elasticSearchJSON\" + fileName, output );
        }
        public class StandardJSON
        {
            public int id;
            public object parent;
            public string code;
            public string url;
            public string[] grades;
            public string text;
        }
        public class DWResource
        {
            public int id;
            public string title;
            public object parent;
            public string path;
            public string url;
            public string[] disabilityCategory;
            public string[] serviceCategory;
            public string[] region;

            public string[] keywords;
            public string[] endUsers;
            public string[] mediaTypes;
            public string description;
            public string created;
        }
    }
}