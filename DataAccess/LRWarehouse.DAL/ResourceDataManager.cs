using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.SqlClient;
using Microsoft.ApplicationBlocks.Data;

namespace LRWarehouse.DAL
{
    public class ResourceDataManager : BaseDataManager
    {
        string className = "ResourceDataManager";

        #region data management
        //Get a record (SELECTED_CODES_PROC)
        public DataSet SelectedCodes( IResourceDataSubclass table, int resourceIntID )
        {
          return GetDSFromIntID( table.selectedCodesProc, resourceIntID, table.tableName, ".SelectedCodes" );
        }

        //Select records (SELECT_PROC)
        public DataSet Select( IResourceDataSubclass table, int resourceIntID )
        {
          return GetDSFromIntID( table.selectProc, resourceIntID, table.tableName, ".Select" );
        }

        //Create a record (CREATE_PROC)
        public int Create( IResourceDataSubclass table, int resourceIntID, int codeID, string originalValue, int createdByID )
        {
          int newID = 0;
          try
          {
            SqlParameter[] parameters = table.getCreateProcParameters( resourceIntID, codeID, originalValue, createdByID );
            DataSet ds = GetDSFromSQL( table.createProc, parameters, table.tableName, ".Create()" );
            if ( DoesDataSetHaveRows( ds ) )
            {
              newID = GetRowPossibleColumn( ds.Tables[ 0 ].Rows[ 0 ], "Id", 0 );
            }
            else
            {
              newID = 0;
            }
          }
          catch ( Exception ex )
          {
            newID = 0;
          }
          return newID;
        }

        //Get a code table (CODETABLE_PROC)
        public DataSet GetCodetable( IResourceDataSubclass table )
        {
          return GetDSFromSQL( table.codetableProc, null, table.tableName, ".GetCodeTable()" );
        }

        protected DataSet GetDSFromIntID( string proc, int resourceIntID, string tableName, string methodName )
        {
          SqlParameter[] parameters = new SqlParameter[] { new SqlParameter( "@ResourceIntId", resourceIntID ) };
          return GetDSFromSQL( proc, parameters, tableName, methodName );
        }

        protected DataSet GetDSFromSQL( string proc, SqlParameter[] parameters, string tableName, string methodName )
        {
            DataSet ds = new DataSet();
            try
            {
                if ( parameters == null )
                {
                    ds = DatabaseManager.DoQuery( proc );
                }
                else
                {
                    ds = SqlHelper.ExecuteDataset( LRWarehouseRO(), CommandType.StoredProcedure, proc, parameters );
                }
                if ( ds.HasErrors )
                {
                    return null;
                }
                return ds;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + "'s " + tableName + methodName + ": " + ex.ToString() );
                return null;
            }
        }

        #endregion

        #region subclasses
        public static class ResourceDataSubclassFinder
        {
            static Dictionary<string, IResourceDataSubclass> classes = new Dictionary<string, IResourceDataSubclass> { 
                { "accessibilityControl", new accessibilityControl() },
                { "accessibilityFeature", new accessibilityFeature() },
                { "accessibilityHazard", new accessibilityHazard() },
                { "assessmentType", new assessmentType() },
                { "accessRights", new accessRights() },
                { "careerCluster", new careerCluster() },
                { "educationalUse", new educationalUse() },
                { "mediaType", new mediaType() },
                { "gradeLevel", new gradeLevel() },
                { "groupType", new groupType() },
                { "endUser", new endUser() },
                { "itemType", new itemType() },
                { "language", new language() },
                { "resourceType", new resourceType() },
                { "subject", new subject() },
                { "originalVersionURL", new originalVersionURL() },
                //{ "k12subject", new k12subject() }
            };

            public static IResourceDataSubclass getSubclassByName( string name )
            {
                foreach ( KeyValuePair<string, IResourceDataSubclass> entry in classes )
                {
                    if ( entry.Key == name )
                    {
                        return entry.Value;
                    }
                }
                return null;
            }
        }
        //Items to hold information about tables/methods/etc
        public interface IResourceDataSubclass
        {
          string tableName { get; set; }
          string typeIDString { get; set; }
          string selectedCodesProc { get; set; }
          string selectProc { get; set; }
          string createProc { get; set; }
          string codetableProc { get; set; }
          SqlParameter[] getCreateProcParameters( int resourceIntID, int codeID, string originalValue, int createdByID );
        }
        public abstract class ResourceDataSubclass : IResourceDataSubclass
        {
          public string tableName { get; set; }
          public string typeIDString { get; set; }
          public string selectedCodesProc { get; set; }
          public string selectProc { get; set; }
          public string createProc { get; set; }
          public string codetableProc { get; set; }
          public abstract SqlParameter[] getCreateProcParameters( int resourceIntID, int codeID, string originalValue, int createdByID );
        }
        public class accessRights : ResourceDataSubclass
        {
          public accessRights()
          {
            tableName = "AccessRights";
            typeIDString = "";
            selectedCodesProc = "[Resource.AccessRights_SelectedCodes]";
            selectProc = "SELECT [AccessRightsId] AS Id, [AccessRights] AS Title FROM [Resource.Version] WHERE ResourceIntId = @ResourceIntId";
            createProc = "";
            codetableProc = "SELECT [Id], [Title], [Description], [WarehouseTotal] AS ItemCount FROM [Codes.AccessRights] WHERE [IsActive] = 1 ORDER BY [SortOrder]";
          }
          public override SqlParameter[] getCreateProcParameters( int resourceIntID, int codeID, string originalValue, int createdByID ) //Handled by resource.version stuff
          {
            SqlParameter[] parameters = new SqlParameter[ 0 ];
            return parameters;
          }
        }

        public class accessibilityControl : ResourceDataSubclass
        {
            string id = "AccessibilityControl";
            public accessibilityControl()
          {
              tableName = id;
              typeIDString = string.Format( "@{0}Id]", id );
              selectedCodesProc = string.Format( "[Resource.{0}_SelectedCodes]", id );
              selectProc = string.Format( "[Resource.{0}Select]", id );
              createProc = string.Format( "[Resource.{0}Insert]", id );
              codetableProc = string.Format( "SELECT [Id], [Title], [Description], [WarehouseTotal] AS ItemCount FROM [Codes.{0}] WHERE [IsActive] = 1 ORDER BY [Title]", id );
          }

          public override SqlParameter[] getCreateProcParameters( int resourceIntID, int codeID, string originalValue, int createdByID )
          {
            SqlParameter[] parameters = new SqlParameter[ 3 ];
            parameters[ 0 ] = new SqlParameter( "@ResourceIntId", resourceIntID );
            parameters[ 1 ] = new SqlParameter( typeIDString, codeID );
            parameters[ 2 ] = new SqlParameter( "@CreatedbyId", createdByID );
            return parameters;
          }
        }


        public class accessibilityFeature : ResourceDataSubclass
        {
            string id = "AccessibilityFeature";
            public accessibilityFeature()
            {
                tableName = id;
                typeIDString = string.Format( "@{0}Id]", id );
                selectedCodesProc = string.Format( "[Resource.{0}_SelectedCodes]", id );
                selectProc = string.Format( "[Resource.{0}Select]", id );
                createProc = string.Format( "[Resource.{0}Insert]", id );
                codetableProc = string.Format( "SELECT [Id], [Title], [Description], [WarehouseTotal] AS ItemCount FROM [Codes.{0}] WHERE [IsActive] = 1 ORDER BY [Title]", id );
            }

            public override SqlParameter[] getCreateProcParameters( int resourceIntID, int codeID, string originalValue, int createdByID )
            {
                SqlParameter[] parameters = new SqlParameter[ 3 ];
                parameters[ 0 ] = new SqlParameter( "@ResourceIntId", resourceIntID );
                parameters[ 1 ] = new SqlParameter( typeIDString, codeID );
                parameters[ 2 ] = new SqlParameter( "@CreatedbyId", createdByID );
                return parameters;
            }
        }


        public class accessibilityHazard : ResourceDataSubclass
        {
            string id = "AccessibilityHazard";
            public accessibilityHazard()
            {
                tableName = id;
                typeIDString = string.Format( "@{0}Id]", id );
                selectedCodesProc = string.Format("[Resource.{0}_SelectedCodes]", id);
                selectProc = string.Format( "[Resource.{0}Select]", id );
                createProc = string.Format( "[Resource.{0}Insert]", id );
                codetableProc = string.Format("SELECT [Id], [Title], [Description], [WarehouseTotal] AS ItemCount FROM [Codes.{0}] WHERE [IsActive] = 1 ORDER BY [Id]", id);
            }

            public override SqlParameter[] getCreateProcParameters( int resourceIntID, int codeID, string originalValue, int createdByID )
            {
                SqlParameter[] parameters = new SqlParameter[ 3 ];
                parameters[ 0 ] = new SqlParameter( "@ResourceIntId", resourceIntID );
                parameters[ 1 ] = new SqlParameter( typeIDString, codeID );
                parameters[ 2 ] = new SqlParameter( "@CreatedbyId", createdByID );
                return parameters;
            }
        }

        public class assessmentType : ResourceDataSubclass
        {
            public assessmentType()
            {
                tableName = "AssessmentType";
                typeIDString = "@AssessmentTypeId";
                selectedCodesProc = "[Resource.AssessmentType_SelectedCodes]";
                selectProc = "[Resource.AssessmentTypeSelect]";
                createProc = "[Resource.AssessmentTypeInsert]";
                codetableProc = "SELECT [Id], [Title], [Description], [WarehouseTotal] AS ItemCount FROM [Codes.AssessmentType] WHERE [IsActive] = 1 ORDER BY [SortOrder]";
            }
            public override SqlParameter[] getCreateProcParameters( int resourceIntID, int codeID, string originalValue, int createdByID )
            {
                SqlParameter[] parameters = new SqlParameter[ 3 ];
                parameters[ 0 ] = new SqlParameter( "@ResourceIntId", resourceIntID );
                parameters[ 1 ] = new SqlParameter( typeIDString, codeID );
                parameters[ 2 ] = new SqlParameter( "@CreatedbyId", createdByID );
                return parameters;
            }
        }

        public class careerCluster : ResourceDataSubclass
        {
          public careerCluster()
          {
            tableName = "CareerCluster";
            typeIDString = "@ClusterId";
            selectedCodesProc = "[Resource.ClusterSelect_SelectedCodes2]";
            selectProc = "[Resource.ClusterSelect2]";
            createProc = "[Resource.ClusterInsert2]";
            codetableProc = "SELECT [Id], [IlPathwayName] AS Title, [Description], [WarehouseTotal] AS ItemCount FROM [CareerCluster] WHERE [IsActive] = 1 AND [IsIlPathway] = 1 ORDER BY [CareerCluster]";
          }
          public override SqlParameter[] getCreateProcParameters( int resourceIntID, int codeID, string originalValue, int createdByID )
          {
            SqlParameter[] parameters = new SqlParameter[ 3 ];
            parameters[ 0 ] = new SqlParameter( "@ResourceIntId", resourceIntID );
            parameters[ 1 ] = new SqlParameter( typeIDString, codeID );
            parameters[ 2 ] = new SqlParameter( "@CreatedbyId", createdByID );
            return parameters;
          }
        }
        public class educationalUse : ResourceDataSubclass
        {
          public educationalUse()
          {
            tableName = "EducationalUse";
            typeIDString = "@EducationUseId";
            selectedCodesProc = "[Resource.EducationUse_SelectedCodes]";
            selectProc = "[Resource.EducationUseSelect]";
            createProc = "[Resource.EducationUseInsert]";
            codetableProc = "SELECT [Id], [Title], [Description], [WarehouseTotal] AS ItemCount FROM [Codes.EducationalUse] WHERE [IsActive] = 1 ORDER BY [SortOrder]";
          }
          public override SqlParameter[] getCreateProcParameters( int resourceIntID, int codeID, string originalValue, int createdByID )
          {
            SqlParameter[] parameters = new SqlParameter[ 5 ];
            parameters[ 0 ] = new SqlParameter( "@ResourceIntId", resourceIntID );
            parameters[ 1 ] = new SqlParameter( typeIDString, codeID );
            parameters[ 2 ] = new SqlParameter( "@CreatedbyId", createdByID );
            parameters[ 3 ] = new SqlParameter( "@OriginalValue", originalValue );
            parameters[ 4 ] = new SqlParameter( "@ResourceRowId", "" );
            return parameters;
          }
        }
        public class mediaType : ResourceDataSubclass
        {
          public mediaType()
          {
            tableName = "ResourceFormat";
            typeIDString = "@ResourceFormatId";
            selectedCodesProc = "[Resource.Format_SelectedCodes]";
            selectProc = "[Resource.ResourceTypeSelect2]";
            createProc = "[Resource.FormatInsert]";
            codetableProc = "SELECT [Id], [Title], [Description], [WarehouseTotal] AS ItemCount FROM [Codes.ResourceFormat] WHERE [IsActive] = 1 ORDER BY [Title]";
          }
          public override SqlParameter[] getCreateProcParameters( int resourceIntID, int codeID, string originalValue, int createdByID )
          {
            SqlParameter[] parameters = new SqlParameter[ 4 ];
            parameters[ 0 ] = new SqlParameter( "@ResourceIntId", resourceIntID );
            parameters[ 1 ] = new SqlParameter( typeIDString, codeID );
            parameters[ 2 ] = new SqlParameter( "@CreatedbyId", createdByID );
            parameters[ 3 ] = new SqlParameter( "@OriginalValue", originalValue );
            return parameters;
          }
        }
        public class gradeLevel : ResourceDataSubclass
        {
          public gradeLevel()
          {
            tableName = "GradeLevel";
            typeIDString = "@GradeLevelId";
            selectedCodesProc = "[Resource.GradeLevel_SelectedCodes]";
            selectProc = "[Resource.GradeLevelSelect]";
            createProc = "[Resource.GradeLevel_Insert]";
            codetableProc = "SELECT MAX([Id]) AS Id, [GradeRange] AS Title, MAX([SortOrder]) As SortOrder, [IsActive], [Description], [WarehouseTotal] AS ItemCount FROM [Codes.GradeLevel] WHERE [IsActive] = 1 GROUP BY [GradeRange], [IsActive], [Description], [WarehouseTotal] ORDER BY [SortOrder]";
          }
          public override SqlParameter[] getCreateProcParameters( int resourceIntID, int codeID, string originalValue, int createdByID )
          {
            SqlParameter[] parameters = new SqlParameter[ 3 ];
            parameters[ 0 ] = new SqlParameter( "@resourceIntId", resourceIntID );
            parameters[ 1 ] = new SqlParameter( typeIDString, codeID );
            parameters[ 2 ] = new SqlParameter( "@CreatedbyId", createdByID );
            return parameters;
          }
        }
        public class groupType : ResourceDataSubclass
        {
          public groupType()
          {
            tableName = "GroupType";
            typeIDString = "@GroupTypeId";
            selectedCodesProc = "[Resource.GroupType_SelectedCodes]";
            selectProc = "[Resource.GroupTypeSelect]";
            createProc = "[Resource.GroupTypeInsert]";
            codetableProc = "SELECT [Id], [Title], [Description], [WarehouseTotal] AS ItemCount FROM [Codes.GroupType] WHERE [IsActive] = 1 ORDER BY [Title]";
          }
          public override SqlParameter[] getCreateProcParameters( int resourceIntID, int codeID, string originalValue, int createdByID )
          {
            SqlParameter[] parameters = new SqlParameter[ 3 ];
            parameters[ 0 ] = new SqlParameter( "@ResourceIntId", resourceIntID );
            parameters[ 1 ] = new SqlParameter( typeIDString, codeID );
            parameters[ 2 ] = new SqlParameter( "@CreatedbyId", createdByID );
            return parameters;
          }
        }
        public class endUser : ResourceDataSubclass
        {
          public endUser()
          {
            tableName = "AudienceType";
            typeIDString = "@AudienceId";
            selectedCodesProc = "[Resource.IntendedAudience_SelectedCodes]";
            selectProc = "[Resource.IntendedAudienceSelect2]";
            createProc = "[Resource.IntendedAudienceInsert]";
            codetableProc = "SELECT [Id], [Title], [Description], [WarehouseTotal] AS ItemCount FROM [Codes.AudienceType] WHERE [IsActive] = 1 ORDER BY [Title]";
          }
          public override SqlParameter[] getCreateProcParameters( int resourceIntID, int codeID, string originalValue, int createdByID )
          {
            SqlParameter[] parameters = new SqlParameter[ 4 ];
            parameters[ 0 ] = new SqlParameter( "@ResourceIntId", resourceIntID );
            parameters[ 1 ] = new SqlParameter( typeIDString, codeID );
            parameters[ 2 ] = new SqlParameter( "@CreatedbyId", createdByID );
            parameters[ 3 ] = new SqlParameter( "@OriginalAudience", originalValue );
            return parameters;
          }
        }
        public class itemType : ResourceDataSubclass
        {
          public itemType()
          {
            tableName = "ItemType";
            typeIDString = "@ItemTypeId";
            selectedCodesProc = "[Resource.ItemType_SelectedCodes]";
            selectProc = "[Resource.ItemTypeSelect]";
            createProc = "[Resource.ItemTypeInsert]";
            codetableProc = "SELECT [Id], [Title], [Description], [WarehouseTotal] AS ItemCount FROM [Codes.ItemType] WHERE [IsActive] = 1 ORDER BY [SortOrder]";
          }
          public override SqlParameter[] getCreateProcParameters( int resourceIntID, int codeID, string originalValue, int createdByID )
          {
            SqlParameter[] parameters = new SqlParameter[ 3 ];
            parameters[ 0 ] = new SqlParameter( "@ResourceIntId", resourceIntID );
            parameters[ 1 ] = new SqlParameter( typeIDString, codeID );
            parameters[ 2 ] = new SqlParameter( "@CreatedbyId", createdByID );
            return parameters;
          }
        }
        public class language : ResourceDataSubclass
        {
          public language()
          {
            tableName = "Language";
            typeIDString = "@LanguageId";
            selectedCodesProc = "[Resource.Language_SelectedCodes]";
            selectProc = "[Resource.LanguageSelect2]";
            createProc = "[Resource.LanguageInsert]";
            codetableProc = "[Codes.LanguageSelect]";
          }
          public override SqlParameter[] getCreateProcParameters( int resourceIntID, int codeID, string originalValue, int createdByID )
          {
            SqlParameter[] parameters = new SqlParameter[ 4 ];
            parameters[ 0 ] = new SqlParameter( typeIDString, codeID );
            parameters[ 1 ] = new SqlParameter( "@ResourceIntId", resourceIntID );
            parameters[ 2 ] = new SqlParameter( "@CreatedById", createdByID );
            parameters[ 3 ] = new SqlParameter( "@OriginalLanguage", originalValue );
            return parameters;
          }
        }
        public class resourceType : ResourceDataSubclass
        {
          public resourceType()
          {
            tableName = "ResourceType";
            typeIDString = "@ResourceTypeId";
            selectedCodesProc = "[Resource.ResourceType_SelectedCodes]";
            selectProc = "[Resource.ResourceTypeSelect]";
            createProc = "[Resource.ResourceTypeInsert]";
            codetableProc = "SELECT [Id], [Title], [Description], [WarehouseTotal] AS ItemCount FROM [Codes.ResourceType] WHERE [IsActive] = 1 ORDER BY [SortOrder]";
          }
          public override SqlParameter[] getCreateProcParameters( int resourceIntID, int codeID, string originalValue, int createdByID )
          {
            SqlParameter[] parameters = new SqlParameter[ 4 ];
            parameters[ 0 ] = new SqlParameter( "@ResourceIntId", resourceIntID );
            parameters[ 1 ] = new SqlParameter( typeIDString, codeID );
            parameters[ 2 ] = new SqlParameter( "@CreatedbyId", createdByID );
            parameters[ 3 ] = new SqlParameter( "@OriginalValue", originalValue );
            return parameters;
          }
        }
        public class subject : ResourceDataSubclass
        {
          public subject()
          {
            tableName = "Subject";
            typeIDString = "@SubjectId";
            selectedCodesProc = "[Resource.Subject_SelectedCodes]";
            selectProc = "";
            createProc = "[Resource.SubjectInsert2]";
            codetableProc = "SELECT [Id], [Title], [Description], [WarehouseTotal] AS ItemCount FROM [Codes.Subject] WHERE [IsActive] = 1 ORDER BY [Title]";
          }
          public override SqlParameter[] getCreateProcParameters( int resourceIntID, int codeID, string originalValue, int createdByID )
          {
            SqlParameter[] parameters = new SqlParameter[ 3 ];
            parameters[ 0 ] = new SqlParameter( "@ResourceIntId", resourceIntID );
            parameters[ 1 ] = new SqlParameter( "@CreatedbyId", createdByID );
            parameters[ 2 ] = new SqlParameter( "@Subject", originalValue );
            return parameters;
          }
        }
        public class originalVersionURL : ResourceDataSubclass
        {
          public originalVersionURL()
          {
            tableName = "RelatedUrl";
            typeIDString = "@RelatedUrl";
            selectedCodesProc = "";
            selectProc = "";
            createProc = "[Resource.RelatedUrlInsert]";
            codetableProc = "";
          }
          public override SqlParameter[] getCreateProcParameters( int resourceIntID, int codeID, string originalValue, int createdByID )
          {
            SqlParameter[] parameters = new SqlParameter[ 3 ];
            parameters[ 0 ] = new SqlParameter( "@ResourceIntId", resourceIntID );
            parameters[ 1 ] = new SqlParameter( "@CreatedbyId", createdByID );
            parameters[ 2 ] = new SqlParameter( "@RelatedUrl", originalValue );
            return parameters;
          }
        }
        /*public class k12subject : ResourceDataSubclass
        {
          public k12subject()
          {
            tableName = "k12subject";
            typeIDString = "@";
          }
        }*/

        #endregion
    }
}
