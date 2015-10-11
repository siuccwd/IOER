using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using System.Data;
using LRWarehouse.DAL;
using System.Web.Script.Serialization;
using LRWarehouse.Business;

namespace IOER.Services
{
    /// <summary>
    /// Summary description for ResourceStandardsService
    /// </summary>
    [WebService( Namespace = "http://ilsle.com/" )]
    [WebServiceBinding( ConformsTo = WsiProfiles.BasicProfile1_1 )]
    [System.ComponentModel.ToolboxItem( false )]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class ResourceStandardsService : System.Web.Services.WebService
    {
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        #region Test Methods
        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        [WebMethod]
        public string ReturnData( string testData )
        {
            return "returning: " + testData;
        }
        #endregion

        #region Web Methods

        [WebMethod]
        public string GetSubjects()
        {
            return serializer.Serialize( new Subjects() );
        }
        public Subjects GetSubjects_object()
        {
            return new Subjects();
        }

        [WebMethod]
        public string GetGrades( string subjectID )
        {
            Grades gradeItems = new Grades();
            gradeItems.BuildList( subjectID );
            return serializer.Serialize( gradeItems );
        }
        public Grades GetGrades_object( string subjectID )
        {
            Grades gradeItems = new Grades();
            gradeItems.BuildList( subjectID );
            return gradeItems;
        }

        [WebMethod]
        public string GetDomains( string subjectID, string gradeLevelID )
        {
            Domains domainItems = new Domains();
            domainItems.BuildList( subjectID, gradeLevelID );
            return serializer.Serialize( domainItems );
        }
        public Domains GetDomains_object( string subjectID, string gradeLevelID )
        {
            Domains domainItems = new Domains();
            domainItems.BuildList( subjectID, gradeLevelID );
            return domainItems;
        }

        [WebMethod]
        public string GetClusters( string parentID, string gradeLevelID )
        {
            Clusters clusterItems = new Clusters();
            clusterItems.BuildList( parentID, gradeLevelID );
            return serializer.Serialize( clusterItems );
        }
        public Clusters GetClusters_object( string parentID, string gradeLevelID )
        {
            Clusters clusterItems = new Clusters();
            clusterItems.BuildList( parentID, gradeLevelID );
            return clusterItems;
        }

        [WebMethod]
        public string GetStandardsAndComponents( string parentID, string gradeLevelID )
        {
            StandardsAndComponents standardsAndComponentsItems = new StandardsAndComponents();
            standardsAndComponentsItems.BuildList( parentID, gradeLevelID );
            return serializer.Serialize( standardsAndComponentsItems );
        }
        public StandardsAndComponents GetStandardsAndComponents_object( string parentID, string gradeLevelID )
        {
            StandardsAndComponents standardsAndComponentsItems = new StandardsAndComponents();
            standardsAndComponentsItems.BuildList( parentID, gradeLevelID );
            return standardsAndComponentsItems;
        }

        [WebMethod]
        public string CheckExistingURL( string targetURL )
        {
            CheckedURL checkedURL = new CheckedURL();
            checkedURL.checkURL( targetURL );
            return serializer.Serialize( checkedURL );
        }

        [WebMethod]
        public string GetAlignmentTypes()
        {
            AlignmentTypes alignmentTypes = new AlignmentTypes();
            alignmentTypes.BuildList();
            return serializer.Serialize( alignmentTypes );
        }

        [WebMethod]
        public string GetStandardData( string standardID )
        {
            StandardsAndComponents standardsAndComponentsItems = new StandardsAndComponents();
            standardsAndComponentsItems.GetItem( standardID );
            return serializer.Serialize( standardsAndComponentsItems );
        }

        #endregion

        #region Sub-Classes

        public class Subjects //TODO: make this dynamic
        {
            public string[] texts = { "CCSS Math", "CCSS ELA/Literacy" };
            public string[] values = { "3", "4" };
        }

        public class Grades
        {
            private DataSet ds;
            private List<string> listTexts = new List<string>();
            private List<string> listValues = new List<string>();
            public string[] texts;
            public string[] values;

            public void BuildList( string subjectID )
            {
                //ds = DatabaseManager.DoQuery( "SELECT DISTINCT [StandardGradeLevelId] AS Id, [StandardGradeLevel] AS Description FROM [StandardDomainGradeLevels] WHERE [SubjectId] = " + subjectID + " AND [StandardGradeLevelId] IS NOT NULL ORDER BY 1" );
                ds = DatabaseManager.DoQuery( "SELECT [Id], [Title] AS Description FROM [Codes.GradeLevel] WHERE [IsActive] = 1 AND [IsK12Level] = 1" );
                if ( DatabaseManager.DoesDataSetHaveRows( ds ) )
                {
                    foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                    {
                        listValues.Add( DatabaseManager.GetRowColumn( dr, "Id" ) );
                        listTexts.Add( DatabaseManager.GetRowColumn( dr, "Description" ) );
                    }
                }
                else
                {
                    listValues.Add( "no rows detected" );
                    listTexts.Add( "no rows detected" );
                }
                texts = listTexts.ToArray();
                values = listValues.ToArray();
            }
        }

        public class Domains
        {
            private DataSet ds;
            private List<string> listTexts = new List<string>();
            private List<string> listValues = new List<string>();
            private List<string> listCodes = new List<string>();
            public string[] texts;
            public string[] values;
            public string[] codes;

            public void BuildList( string parentID, string gradeLevelID )
            {
                //ds = DatabaseManager.DoQuery( "SELECT DISTINCT [DomainId] AS Id, [Domain] AS Description FROM [StandardDomainGradeLevels] WHERE [SubjectId] = " + parentID + " AND [StandardGradeLevelId] = " + gradeLevelID + " ORDER BY 1" );
                ds = DatabaseManager.DoQuery( "SELECT DISTINCT [DomainId] AS Id, [Domain] AS Description, [NotationCode] FROM [StandardDomainGradeLevels] levels JOIN [StandardBody.Node] node ON levels.DomainId = node.Id WHERE [SubjectId] = " + parentID + " AND [StandardGradeLevelId] = " + gradeLevelID + " ORDER BY 1" );

                if ( DatabaseManager.DoesDataSetHaveRows( ds ) )
                {
                    foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                    {
                        listValues.Add( DatabaseManager.GetRowColumn( dr, "Id" ) );
                        listTexts.Add( DatabaseManager.GetRowColumn( dr, "Description" ) );
                        listCodes.Add( DatabaseManager.GetRowPossibleColumn( dr, "NotationCode" ) );
                    }
                    texts = listTexts.ToArray();
                    values = listValues.ToArray();
                    codes = listCodes.ToArray();
                }
            }
        }

        public class Clusters
        {
            private DataSet ds;
            private List<string> listTexts = new List<string>();
            private List<string> listValues = new List<string>();
            private List<string> listCodes = new List<string>();
            public string[] texts;
            public string[] values;
            public string[] codes;

            public void BuildList( string parentID, string gradeLevelID )
            {
                //ds = DatabaseManager.DoQuery( "SELECT DISTINCT [ClusterId] AS Id, [Cluster] AS Description FROM [StandardDomainGradeLevels] WHERE [DomainId] = " + parentID + " AND [StandardGradeLevelId] = " + gradeLevelID + " ORDER BY 1" );
                ds = DatabaseManager.DoQuery( "SELECT DISTINCT [ClusterId] AS Id, [Cluster] AS Description, [NotationCode] FROM [StandardDomainGradeLevels] levels JOIN [StandardBody.Node] node ON levels.ClusterId = node.Id WHERE [DomainId] = " + parentID + " AND [StandardGradeLevelId] = " + gradeLevelID + " ORDER BY 1" );

                if ( DatabaseManager.DoesDataSetHaveRows( ds ) )
                {
                    foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                    {
                        listValues.Add( DatabaseManager.GetRowColumn( dr, "Id" ) );
                        listTexts.Add( DatabaseManager.GetRowColumn( dr, "Description" ) );
                        listCodes.Add( DatabaseManager.GetRowPossibleColumn( dr, "NotationCode" ) );
                    }
                    texts = listTexts.ToArray();
                    values = listValues.ToArray();
                    codes = listCodes.ToArray();
                }
            }
        }

        public class StandardsAndComponents
        {
            private DataSet ds;
            private DataSet ds2;
            private List<string> listNotationCodes = new List<string>();        //Notation codes
            private List<string> listTexts = new List<string>();                //Text descriptions
            private List<string> listValues = new List<string>();               //database ID number
            private List<string> listURLs = new List<string>();                 //standard/component URLs
            private List<string> listGrades = new List<string>();               //applicable grade levels
            private List<string> listToolTip1 = new List<string>();             //ToolTip data for Domain
            private List<string> listToolTip2 = new List<string>();             //ToolTip data for Cluster
            private List<string> listToolTip3 = new List<string>();             //ToolTip data for Standard (where applicable)
            public string[] notationCodes;
            public string[] texts;
            public string[] values;
            public string[] urls;
            public string[] grades;
            public string[] toolTip1;
            public string[] toolTip2;
            public string[] toolTip3;


            private string sqlGetStandard = "SELECT DISTINCT [StandardId], [Standard] AS Description, [StandardGradeLevel] AS GradeLevel, [Domain] AS ToolTip1, [Cluster] AS ToolTip2, node.[NotationCode], node.[StandardUrl] FROM [StandardDomainGradeLevels] base LEFT JOIN [StandardBody.Node] node ON base.[StandardId] = node.[Id] WHERE [ClusterId] = {0} AND [StandardGradeLevelId] = {1}";
            private string sqlGetComponent = "SELECT DISTINCT [StandardId], [ComponentId], [Component] AS Description, [StandardGradeLevel] AS GradeLevel, [Domain] AS ToolTip1, [Cluster] AS ToolTip2, [Standard] AS ToolTip3, node.[NotationCode], node.[StandardUrl] FROM [StandardDomainGradeLevels] base LEFT JOIN [StandardBody.Node] node ON base.[ComponentId] = node.[Id] WHERE [ParentId] = {0} AND [StandardGradeLevelId] = {1}";
            private string sqlGetItemStandard = "SELECT DISTINCT TOP 1 [StandardId], [ComponentId], [Standard] AS StandardDescription, [Component] AS ComponentDescription, [StandardGradeLevel] AS GradeLevel, [Domain] AS ToolTip1, [Cluster] AS ToolTip2, [Standard] AS ToolTip3, node.[NotationCode], node.[StandardUrl] FROM [StandardDomainGradeLevels] base LEFT JOIN [StandardBody.Node] node ON base.[StandardId] = node.[Id] WHERE node.[Id] = '{0}' ORDER BY [NotationCode] DESC";
            private string sqlGetItemComponent = "SELECT DISTINCT TOP 1 [StandardId], [ComponentId], [Standard] AS StandardDescription, [Component] AS ComponentDescription, [StandardGradeLevel] AS GradeLevel, [Domain] AS ToolTip1, [Cluster] AS ToolTip2, [Standard] AS ToolTip3, node.[NotationCode], node.[StandardUrl] FROM [StandardDomainGradeLevels] base LEFT JOIN [StandardBody.Node] node ON base.[StandardId] = node.[ParentId] WHERE node.[ParentId] = '{0}' AND ( ( [ComponentId] = '{1}' AND node.[Id] = '{1}' ) OR [ComponentId] IS NULL ) ORDER BY [NotationCode] DESC";

            public void BuildList( string parentID, string gradeLevelID )
            {
                //Get the fields
                ds = DatabaseManager.DoQuery( string.Format( sqlGetStandard, parentID, gradeLevelID ) );

                //If the results are valid...
                if ( DatabaseManager.DoesDataSetHaveRows( ds ) )
                {
                    //For each standard
                    foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                    {
                        //Check for components
                        ds2 = DatabaseManager.DoQuery( string.Format( sqlGetComponent, DatabaseManager.GetRowPossibleColumn( dr, "StandardId" ), gradeLevelID ) );

                        //If there are any...
                        if ( DatabaseManager.DoesDataSetHaveRows( ds2 ) )
                        {
                            //Add them to the list
                            foreach ( DataRow dr2 in ds2.Tables[ 0 ].Rows )
                            {
                                AddItem( dr2 );
                            }
                        }
                        //Otherwise
                        else //remove this part to switch to displaying standards that have components
                        {
                            AddItem( dr );
                        }
                    }
                }

                notationCodes = listNotationCodes.ToArray();
                texts = listTexts.ToArray();
                values = listValues.ToArray();
                urls = listURLs.ToArray();
                grades = listGrades.ToArray();
                toolTip1 = listToolTip1.ToArray();
                toolTip2 = listToolTip2.ToArray();
                toolTip3 = listToolTip3.ToArray();

            }

            public void GetItem( string standardID )
            {
                string standard = standardID.Split( '-' )[ 0 ];
                string component = standardID.Split( '-' )[ 1 ];
                //DataSet ds;

                if ( component == "" )
                {
                    ds = DatabaseManager.DoQuery( string.Format( sqlGetItemStandard, standard ) );
                }
                else
                {
                    ds = DatabaseManager.DoQuery( string.Format( sqlGetItemComponent, standard, component ) );
                }
                if ( DatabaseManager.DoesDataSetHaveRows( ds ) )
                {
                    foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                    {
                        AddItem( dr );
                    }
                }

                notationCodes = listNotationCodes.ToArray();
                texts = listTexts.ToArray();
                values = listValues.ToArray();
                urls = listURLs.ToArray();
                grades = listGrades.ToArray();
                toolTip1 = listToolTip1.ToArray();
                toolTip2 = listToolTip2.ToArray();
                toolTip3 = listToolTip3.ToArray();

            }

            protected void AddItem( DataRow dr )
            {
                string standardID = "";
                string componentID = "";
                try
                {
                    standardID = DatabaseManager.GetRowPossibleColumn( dr, "StandardId" );
                }
                catch ( Exception ex ) { }
                try
                {
                    componentID = DatabaseManager.GetRowPossibleColumn( dr, "ComponentId" );
                }
                catch ( Exception ex ) { }

                listGrades.Add( DatabaseManager.GetRowPossibleColumn( dr, "GradeLevel" ) );
                listTexts.Add( DatabaseManager.GetRowPossibleColumn( dr, "Description" ) );
                listValues.Add( standardID + "-" + componentID );
                listToolTip1.Add( DatabaseManager.GetRowPossibleColumn( dr, "ToolTip1" ) );
                listToolTip2.Add( DatabaseManager.GetRowPossibleColumn( dr, "ToolTip2" ) );
                listToolTip3.Add( DatabaseManager.GetRowPossibleColumn( dr, "ToolTip3" ) );
                listNotationCodes.Add( DatabaseManager.GetRowPossibleColumn( dr, "NotationCode" ) );
                listURLs.Add( DatabaseManager.GetRowPossibleColumn( dr, "StandardUrl" ) );
            }

        }


        public class CheckedURL
        {
            public string checkedURL = "false";
            public string existingPageVID = "";

            public void checkURL( string targetURL )
            {
                DataSet ds = DatabaseManager.DoQuery( "SELECT TOP 1 res.[ResourceUrl], vers.[Id] AS vid FROM [Resource] res JOIN [Resource.Version] vers ON res.[Id] = vers.[ResourceIntId] WHERE [ResourceUrl] = '" + targetURL + "' AND res.[IsActive] = 1 AND vers.[IsActive] = 1" );
                if ( DatabaseManager.DoesDataSetHaveRows( ds ) )
                {
                    checkedURL = "true";
                    existingPageVID = DatabaseManager.GetRowColumn( ds.Tables[ 0 ].Rows[ 0 ], "vid" );
                }
            }
        }


        public class AlignmentTypes
        {
            private DataSet ds;
            private List<string> listTexts = new List<string>();
            private List<string> listValues = new List<string>();
            public string[] texts;
            public string[] values;

            public void BuildList()
            {
                ds = DatabaseManager.DoQuery( "SELECT DISTINCT [Id], [Title] AS Description FROM [Codes.AlignmentType] WHERE [IsActive] = 1" );

                if ( DatabaseManager.DoesDataSetHaveRows( ds ) )
                {
                    foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                    {
                        listValues.Add( DatabaseManager.GetRowColumn( dr, "Id" ) );
                        listTexts.Add( DatabaseManager.GetRowColumn( dr, "Description" ) );
                    }
                    texts = listTexts.ToArray();
                    values = listValues.ToArray();
                }
            }
        }


        #endregion
    }
}
