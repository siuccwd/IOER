using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Web.Script.Serialization;
using ILPathways.Library;
using System.Data;
using LRWarehouse.DAL;
using System.Data.SqlClient;

namespace ILPathways.LRW.controls
{
    public partial class TagList : BaseUserControl
    {
        public string mode { get; set; } //read, write, update
        public int currentResourceIntID { get; set; }
        public IConfiguration config { get; set; }

        TagListJSON recommendedTagsList { get; set; }
        TagListJSON existingTagsList { get; set; }
        public TagListJSON inputTagsList { get; set; }
        JavaScriptSerializer serializer;

        protected void Page_Load( object sender, EventArgs e )
        {
            InitStuff();
            try
            {
                inputTagsList = new TagListJSON();
                inputTagsList.entryType = config.entryType;
                inputTagsList.tags = serializer.Deserialize<List<string>>( hdnList.Value.Replace( @"<", "" ).Replace( @">", "" ) );
            }
            catch { }
            if ( currentResourceIntID > 0 )
            {
                GetList();
            }
            if ( config != null )
                OutputItems();
        }

        protected void InitStuff()
        {
            if ( config == null ) //Must have a config
            {
                this.Visible = false;
                return;
            }

            hdnList.Attributes.Add( "class", config.entryType + "_hdn" );
            serializer = new JavaScriptSerializer();
            recommendedTagsList = new TagListJSON();
            recommendedTagsList.tags = new List<string>();
            existingTagsList = new TagListJSON();
            existingTagsList.tags = new List<string>();
            inputTagsList = new TagListJSON();
            inputTagsList.tags = new List<string>();

            recommendedTagsList.tags.Clear();
            if ( config.suggestedTags.Length > 0 && ( mode == "write" || mode == "update" ) )
            {
                recommendedTags.InnerHtml = "<h3>Recommended Tags:</h3>";
            }
            foreach(string item in config.suggestedTags)
            {
                recommendedTagsList.tags.Add( item );
            }
            if ( mode == null || mode == "" ) { mode = "read"; }
            else { mode = mode.ToLower(); }
   
            if ( mode == "read" && currentResourceIntID == 0 ) { //Must have an ID if we're going to read anything
                this.Visible = false;
                return; 
            }
            if ( mode == "read" )
            {
                entryBox.Visible = false;
                userEnteredTags.Visible = false;
            }
            AddAttributes();
        }

        protected void GetList()
        {
            SqlParameter[] tempParams = new SqlParameter[config.selectParameters.Length];
            config.selectParameters.CopyTo( tempParams, 0 );
            tempParams[ 0 ] = new SqlParameter( "@ResourceIntId", currentResourceIntID );
            DataSet ds = DatabaseManager.ExecuteProc( config.selectProc, tempParams );
            if ( DoesDataSetHaveRows( ds ) )
            {
                foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                {
                    existingTagsList.tags.Add( DatabaseManager.GetRowPossibleColumn( dr, config.selectColumn ) );
                }
            }
        }

        protected void AddAttributes()
        {
            entryBox.Attributes.Add( "entryType", config.entryType );
            userEnteredTags.Attributes.Add( "entryType", config.entryType );
            recommendedTags.Attributes.Add( "entryType", config.entryType );
            existingTags.Attributes.Add( "entryType", config.entryType );
            recommendedTagsList.entryType = config.entryType;
            existingTagsList.entryType = config.entryType;
            inputTagsList.entryType = config.entryType;
        }

        protected void OutputItems()
        {
            inputScript.Text = "<script type=\"text/javascript\" language=\"javascript\">";
            inputScript.Text = inputScript.Text + "var " + config.entryType + "_existingTags = " + serializer.Serialize( existingTagsList ) + ";" + System.Environment.NewLine;
            inputScript.Text = inputScript.Text + "var " + config.entryType + "_recommendedTags = " + serializer.Serialize( recommendedTagsList ) + ";" + System.Environment.NewLine;
            inputScript.Text = inputScript.Text + "</script>";

        }

        public abstract class IConfiguration
        {
            abstract public string entryType { get; }
            abstract public string selectProc { get; }
            abstract public string selectColumn { get; }
            abstract public string[] suggestedTags { get; }
            abstract public SqlParameter[] selectParameters { get; }
        }
        public class Configuration_Keywords : IConfiguration
        {
            public override string entryType { get { return "keyword"; } }
            public override string selectProc { get { return "[Resource.KeywordSelect]"; } }
            public override string selectColumn { get { return "Keyword"; } }
            public override string[] suggestedTags { get { return new string[ 0 ]; } }
            public override SqlParameter[] selectParameters { get { 
                SqlParameter[] parameters = new SqlParameter[2];
                parameters[ 0 ] = new SqlParameter( "@ResourceIntId", 0 );
                parameters[ 1 ] = new SqlParameter( "@Originalvalue", "" );
                return parameters;
            } }
        }
        public class Configuration_Subjects : IConfiguration
        {
            public override string entryType { get { return "subject"; } }
            public override string selectProc { get { return "[Resource.SubjectSelect]"; } }
            public override string selectColumn { get { return "Subject"; } }
            public override string[] suggestedTags { get { return new string[] { "Mathematics", "English Language Arts", "Science", "Social Studies", "Arts", "World Languages", "Health", "Physical Education" }; } }
            public override SqlParameter[] selectParameters
            { get {
                    SqlParameter[] parameters = new SqlParameter[ 1 ];
                    parameters[ 0 ] = new SqlParameter( "@ResourceIntId", 0 );
                    return parameters;
            } }

        }

        public class TagListJSON
        {
            public List<string> tags;
            public string entryType;
        }
    }
}