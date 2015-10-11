using System;
using System.Data;
using System.Data.OleDb;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Security.Permissions;
using System.Collections;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.IO;
using Obout.Ajax.UI;
using Obout.Ajax.UI.HTMLEditor;
using Obout.Ajax.UI.HTMLEditor.ToolbarButton;

//using CustomPopups = ILPathways.HTMLEditor.CustomPopups;

[assembly: WebResource( "App_Scripts.HTMLEditor.scripts.InsertDate.js", "application/x-javascript" )]


//namespace IOER.HTMLEditor.CustomToolbarButton
namespace CustomToolbarButton
{
    [ParseChildren( true )]
    [PersistChildren( false )]
    [RequiredScript( typeof( OpenPopupButton ) )]
    [ButtonsList( true )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance" )]
    public class InsertIcon : OpenPopupButton
    {
        #region [ Properties ]

        public override Obout.Ajax.UI.HTMLEditor.Popups.Popup RelatedPopup
        {
            get
            {
                if ( base.RelatedPopup == null )
                {
                    base.RelatedPopup = new CustomPopups.InsertIconPopup();
                }
                return base.RelatedPopup;
            }
        }

        protected override string ButtonImagesFolder
        {
            get { return "~/App_Obout/HTMLEditor/customButtons/"; }
        }

        public override string DefaultToolTip
        {
            get { return "Insert predefined icon"; }
        }

        protected override string BaseImageName
        {
            get { return "ed_insertIcon"; }
        }

        #endregion
    }

    [ParseChildren( true )]
    [PersistChildren( false )]
    [RequiredScript( typeof( MethodButton ) )]
    [ButtonsList( true )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance" )]
    public class InsertDate : MethodButton
    {
        #region [ Properties ]

        // what client-side type to initiate
        protected override string ClientControlType
        {
            get { return "CustomToolbarButton.InsertDate"; }
        }

        // what file in the client-side type is located
        protected override string ScriptPath
        {
            get { return "~/App_Obout/HTMLEditor/Scripts/InsertDate.js"; }
        }

        // custom buttons images folder
        protected override string ButtonImagesFolder
        {
            get { return "~/App_Obout/HTMLEditor/customButtons/"; }
        }

        // base name of this button image,
        //
        // The following images should be present in the folder above:
        // ed_date_n.gif - normal button's image
        // ed_date_a.gif - image when button pressed
        protected override string BaseImageName
        {
            get { return "ed_date"; }
        }

        // tooltip if not found in Localization
        public override string DefaultToolTip
        {
            get { return "Insert current date"; }
        }

        #endregion
    }

    //[ParseChildren(true)]
    //[PersistChildren(false)]
    //[RequiredScript(typeof(MethodButton))]
    //[ButtonsList(true)]
    //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    //public class MakeCodeBlock : MethodButton
    //{
    //    #region [ Properties ]

    //    // what client-side type to initiate
    //    protected override string ClientControlType
    //    {
    //        get { return "CustomToolbarButton.MakeCodeBlock"; }
    //    }

    //    // what file in the client-side type is located
    //    protected override string ScriptPath
    //    {
    //        get { return "~/App_Obout/HTMLEditor/Scripts/MakeCodeBlock.js"; }
    //    }

    //    // custom buttons images folder
    //    protected override string ButtonImagesFolder
    //    {
    //        get { return "~/App_Obout/HTMLEditor/customButtons/"; }
    //    }

    //    // base name of this button image,
    //    protected override string BaseImageName
    //    {
    //        get { return "ed_MakeCodeBlock"; }
    //    }

    //    // tooltip if not found in Localization
    //    public override string DefaultToolTip
    //    {
    //        get { return "Make a code block from selected text"; }
    //    }

    //    #endregion
    //}

    [ParseChildren( true )]
    [PersistChildren( false )]
    [RequiredScript( typeof( OpenPopupButton ) )]
    [ButtonsList( true )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance" )]
    public class MakeCodeBlock : OpenPopupButton
    {
        #region [ Properties ]

        public override Obout.Ajax.UI.HTMLEditor.Popups.Popup RelatedPopup
        {
            get
            {
                if ( base.RelatedPopup == null )
                {
                    base.RelatedPopup = new CustomPopups.InsertCodeBlockPopup();
                }
                return base.RelatedPopup;
            }
        }

        // what client-side type to initiate
        protected override string ClientControlType
        {
            get { return "CustomToolbarButton.MakeCodeBlock"; }
        }

        // what file in the client-side type is located
        protected override string ScriptPath
        {
            get { return "~/App_Obout/HTMLEditor/Scripts/MakeCodeBlock.js"; }
        }

        protected override string ButtonImagesFolder
        {
            get { return "~/App_Obout/HTMLEditor/customButtons/"; }
        }

        // base name of this button image,
        protected override string BaseImageName
        {
            get { return "ed_MakeCodeBlock"; }
        }

        // tooltip if not found in Localization
        public override string DefaultToolTip
        {
            get { return "Make a code block from selected text"; }
        }

        #endregion
    }

    [ParseChildren( true )]
    [PersistChildren( false )]
    [RequiredScript( typeof( MethodButton ) )]
    [ButtonsList( true )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance" )]
    public class RemoveCodeBlock : MethodButton
    {
        #region [ Properties ]

        // what client-side type to initiate
        protected override string ClientControlType
        {
            get { return "CustomToolbarButton.RemoveCodeBlock"; }
        }

        // what file in the client-side type is located
        protected override string ScriptPath
        {
            get { return "~/App_Obout/HTMLEditor/Scripts/RemoveCodeBlock.js"; }
        }

        // custom buttons images folder
        protected override string ButtonImagesFolder
        {
            get { return "~/App_Obout/HTMLEditor/customButtons/"; }
        }

        // base name of this button image,
        protected override string BaseImageName
        {
            get { return "ed_RemoveCodeBlock"; }
        }

        // tooltip if not found in Localization
        public override string DefaultToolTip
        {
            get { return "Remove the code block"; }
        }

        #endregion
    }

    [ParseChildren( true )]
    [PersistChildren( false )]
    [RequiredScript( typeof( OpenPopupButton ) )]
    [ButtonsList( true )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance" )]
    public abstract class ImmediateFileInsert : OpenPopupButton
    {
        #region [ Fields ]

        private bool _RelativeUrl = true;

        #endregion

        #region [ Properties ]
        /// <summary>
        /// Gets or sets a value indicating whether to create a relative URL for selected file (image, document etc.). If false then absolute URL will be generated.
        /// </summary>
        [DefaultValue( true )]
        [Category( "Behavior" )]
        [ExtenderControlProperty]
        [ClientPropertyName( "relativeUrl" )]
        [Description( "What URL insert: relative/absolute" )]
        public virtual bool RelativeUrl
        {
            get { return _RelativeUrl; }
            set { _RelativeUrl = value; }
        }

        // folder for files uploading
        [Category( "Appearance" )]
        [Description( "Folder used for uploading" )]
        public abstract string UploadFolder { get; set; }

        // returns new GalleryManager object
        protected abstract Obout.Ajax.UI.HTMLEditor.Popups.GalleryManager Manager { get; }

        // what file in the client-side type is located
        protected override string ScriptPath
        {
            get { return "~/App_Obout/HTMLEditor/Scripts/ImmediateFileInsert.js"; }
        }

        // where the button's icons are stored
        protected override string ButtonImagesFolder
        {
            get { return "~/App_Obout/HTMLEditor/customButtons/"; }
        }

        #endregion

        #region [ Methods ]
        // for client-side properties setting
        protected override void DescribeComponent( ScriptComponentDescriptor descriptor )
        {
            Obout.Ajax.UI.HTMLEditor.Popups.GalleryManager manager = Manager;
            // set the client-side property - folder where to upload images
            descriptor.AddProperty( "uploadFolder", LocalResolveUrl( UploadFolder ) );
            // set the client-side property - class name of image browser
            descriptor.AddProperty( "browserClassName", manager.GetType().AssemblyQualifiedName );
            // set the client-side property - message when incorrect file extension for uploading is selected
            descriptor.AddProperty( "invalidFileExtensionMessage", manager.InvalidFileExtensionMessage );
            // set the client-side property - semicolon seperated available extensions for images
            descriptor.AddProperty( "availableExtensions", manager.AvailableExtensions );
            // get the root of the site
            descriptor.AddProperty( "httpRoot", Obout.Ajax.UI.HTMLEditor.Popups.GalleryManager.GetHttpRoot( Page ) );
            // call the base method
            base.DescribeComponent( descriptor );
        }
        #endregion
    }

    public class ImmediateImageInsert : ImmediateFileInsert
    {
        #region [ Properties ]

        // folder for images uploading
        [DefaultValue( "~/App_Obout/HTMLEditor/ImageGallery/users_images/" )]
        public override string UploadFolder
        {
            get { return ( string ) ( ViewState[ "UploadFolder" ] ?? "~/App_Obout/HTMLEditor/ImageGallery/users_images/" ); }
            set { ViewState[ "UploadFolder" ] = value; }
        }

        // we already have an embedded images uploader, use it
        public override Obout.Ajax.UI.HTMLEditor.Popups.Popup RelatedPopup
        {
            get
            {
                if ( base.RelatedPopup == null )
                {
                    base.RelatedPopup = new Obout.Ajax.UI.HTMLEditor.Popups.UploadImagePopup();
                }
                return base.RelatedPopup;
            }
        }

        // tooltip if not found in localization
        public override string DefaultToolTip
        {
            get { return "Image uploading with immediate inserting"; }
        }

        // base name of the icon
        protected override string BaseImageName
        {
            get { return "ed_upload_image"; }
        }

        protected override Obout.Ajax.UI.HTMLEditor.Popups.GalleryManager Manager
        {
            //get { return new Obout.Ajax.UI.HTMLEditor.Popups.ImageBrowser(); }
            get { return new CustomPopups.MyImageBrowser(); }
        }

        // what client-side type to initiate
        protected override string ClientControlType
        {
            get { return "CustomToolbarButton.ImmediateImageInsert"; }
        }

        #endregion
    }

    public class ImmediateDocumentInsert : ImmediateFileInsert
    {
        #region [ Properties ]

        // folder for images uploading
        [DefaultValue( "~/App_Obout/HTMLEditor/DocumentsGallery/users_documents/" )]
        public override string UploadFolder
        {
            get { return ( string ) ( ViewState[ "UploadFolder" ] ?? "~/App_Obout/HTMLEditor/DocumentsGallery/users_documents/" ); }
            set { ViewState[ "UploadFolder" ] = value; }
        }

        // we already have an embedded images uploader, use it
        public override Obout.Ajax.UI.HTMLEditor.Popups.Popup RelatedPopup
        {
            get
            {
                if ( base.RelatedPopup == null )
                {
                    base.RelatedPopup = new Obout.Ajax.UI.HTMLEditor.Popups.UploadDocumentPopup();
                }
                return base.RelatedPopup;
            }
        }

        // tooltip if not found in localization
        public override string DefaultToolTip
        {
            get { return "Document uploading with immediate inserting"; }
        }

        // base name of the icon
        protected override string BaseImageName
        {
            get { return "ed_upload_document"; }
        }

        protected override Obout.Ajax.UI.HTMLEditor.Popups.GalleryManager Manager
        {
            get { return new Obout.Ajax.UI.HTMLEditor.Popups.UrlBrowser(); }
        }

        // what client-side type to initiate
        protected override string ClientControlType
        {
            get { return "CustomToolbarButton.ImmediateDocumentInsert"; }
        }

        #endregion
    }

    public class ImmediateImageInsertToDb : ImmediateImageInsert
    {
        #region [ Properties ]
        [DefaultValue( "ImageStream.aspx?ID=" )]
        public override string UploadFolder
        {
            get { return ( string ) ( ViewState[ "UploadFolder" ] ?? "ImageStream.aspx?ID=" ); }
            set { ViewState[ "UploadFolder" ] = value; }
        }
        protected override Obout.Ajax.UI.HTMLEditor.Popups.GalleryManager Manager
        {
            get { return new CustomPopups.ImageBrowserForInsertToDb(); }
        }
        #endregion
    }    
    

}

namespace CustomPopups
{


    /// <summary>
    /// Custom image browser to handle creating folder if not found, optional prevention of overwriting files with the same name ==> TBD if should be configurable
    /// </summary>
    public class MyImageBrowser : Obout.Ajax.UI.HTMLEditor.Popups.ImageBrowser
    {
        /// <summary>
        /// Saves an uploaded file to the managed folder.
        /// Added code to check for existing file. However, sometimes an over write will be desired action?
        /// </summary>
        /// <param name="folder">Folder URL where to save.</param>
        /// <param name="name">Name of the file.</param>
        /// <param name="title">Files' description.</param>
        /// <param name="stream">Stream with uploaded content.</param>
        /// <returns>Name of the saved file</returns>
        protected override string SaveUploadedFile( string folder, string name, string title, Stream stream )
        {
            // check whether this folder exists
            string path = System.Web.HttpContext.Current.Server.MapPath( folder );
            if ( !Directory.Exists( path ) )
            {
                // create it
                Directory.CreateDirectory( path );
            }

            string url = folder + name;
            path = System.Web.HttpContext.Current.Server.MapPath( url );

            while ( File.Exists( path ) ) // while a file with this name exists in the folder
            {
                string fileName = Path.GetFileNameWithoutExtension( name ) + "_n";
                string fileExt = Path.GetExtension( name );
                name = fileName + fileExt;
                url = folder + name;
                path = System.Web.HttpContext.Current.Server.MapPath( url );
            }
            return base.SaveUploadedFile( folder, name, title, stream );
        }
    }


    #region ******************* POPUPS ***************************************
    public class ImageBrowserForInsertToDb : Obout.Ajax.UI.HTMLEditor.Popups.ImageBrowser
    {
        // Virtual path of the database
        protected virtual string DbPath
        {
            get { return "~/App_Data/FilesRepository.mdb"; }
        }

        // [19.10.2010 7:43:02] ilyabutorine: connect timeout=3600;server=OBOUTDB\OBOUTDB;database=obout_site;uid=oboutPublic;pwd=kl3$08
        // [19.10.2010 7:43:26] ilyabutorine: table names:  Editor_tbFolder, Editor_tbImage
        // Connection string
        protected virtual string ConnectionString
        {
            get { return "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + System.Web.HttpContext.Current.Server.MapPath( ResolveUrl( DbPath ) ) + ";"; }
        }

        // OleDbCommand for SELECT @@IDENTITY statement
        private OleDbCommand cmdGetIdentity;

        // override the 'Save uploaded file' method
        protected override string SaveUploadedFile( string folder, string name, string title, Stream stream )
        {
            string returnId = "";
            OleDbConnection connection = new OleDbConnection();
            try
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();
                int folderId = 0; // root folder

                string sqlString = "Select * from tbImage Where fldFolderId=" + folderId.ToString() + " AND fldName=\"" + name + "\"";
                OleDbDataAdapter eAdapter = new OleDbDataAdapter( sqlString, connection );
                DataTable eTable = new DataTable();
                eAdapter.Fill( eTable );

                // Delete the duplicate if it is present
                if ( eTable.Rows.Count > 0 )
                {
                    OleDbCommand myComm = new OleDbCommand( "DELETE FROM tbImage Where fldFolderId=" + folderId.ToString() + " AND fldName=\"" + name + "\"", connection );
                    myComm.ExecuteNonQuery();
                }

                eAdapter.Dispose();
                eTable.Dispose();

                sqlString = "Select * from tbImage Where fldFolderId=" + folderId.ToString();
                eAdapter = new OleDbDataAdapter( sqlString, connection );
                eTable = new DataTable();
                eAdapter.Fill( eTable );

                OleDbCommand cmdInsert = new OleDbCommand( "INSERT INTO tbImage (fldName, fldFolderId, fldDescription, fldContent) VALUES(?, ?, ?, ?)", connection );
                cmdInsert.Parameters.Add( new OleDbParameter( "fldName", OleDbType.VarChar, 50, "fldName" ) );
                cmdInsert.Parameters.Add( new OleDbParameter( "fldFolderId", OleDbType.Integer, 0, "fldFolderId" ) );
                cmdInsert.Parameters.Add( new OleDbParameter( "fldDescription", OleDbType.VarChar, 100, "fldDescription" ) );
                cmdInsert.Parameters.Add( new OleDbParameter( "fldContent", OleDbType.Binary, 0, "fldContent" ) );
                eAdapter.InsertCommand = cmdInsert;

                // Create a command to get IDENTITY Value
                cmdGetIdentity = new OleDbCommand( "SELECT @@IDENTITY", connection );

                eAdapter.RowUpdated += new OleDbRowUpdatedEventHandler( HandleRowUpdated );

                DataRow newRow = eTable.NewRow();
                newRow[ "fldName" ] = name;
                newRow[ "fldFolderId" ] = folderId;
                newRow[ "fldDescription" ] = title;
                byte[] content = new byte[ stream.Length ];
                stream.Read( content, 0, ( int ) stream.Length );
                newRow[ "fldContent" ] = content;

                eTable.Rows.Add( newRow );
                eAdapter.Update( eTable );
                returnId = newRow[ "id" ].ToString();

                // Release the Resources
                cmdInsert = null;
                cmdGetIdentity = null;
                eAdapter.Dispose();
                eTable.Dispose();
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
            return returnId;
        }

        // Event Handler for RowUpdated Event
        private void HandleRowUpdated( object sender, OleDbRowUpdatedEventArgs e )
        {
            if ( e.Status == UpdateStatus.Continue && e.StatementType == StatementType.Insert )
            {
                // Get the Identity column value
                e.Row[ "id" ] = Int32.Parse( cmdGetIdentity.ExecuteScalar().ToString() );
                e.Row.AcceptChanges();
            }
        }

        // get image's content
        public byte[] GetImageBytes( int id )
        {
            byte[] content = null;
            OleDbConnection connection = new OleDbConnection();
            try
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();
                string sqlString = "Select * from tbImage Where id=" + id.ToString();
                OleDbDataAdapter eAdapter = new OleDbDataAdapter( sqlString, connection );
                DataTable eTable = new DataTable();
                eAdapter.Fill( eTable );

                if ( eTable.Rows.Count > 0 )
                {
                    content = ( byte[] ) eTable.Rows[ 0 ][ "fldContent" ];
                }

                eAdapter.Dispose();
                eTable.Dispose();
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
            return content;
        }
    }

    [RequiredScript( typeof( Obout.Ajax.UI.HTMLEditor.Popups.Popup ) )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance" )]
    public class InsertIconPopup : Obout.Ajax.UI.HTMLEditor.Popups.Popup
    {

        #region [ Properties ]

        [DefaultValue( 10 )]
        [Category( "Appearance" )]
        [Description( "Icons in one row" )]
        public int IconsInRow
        {
            get { return ( int ) ( ViewState[ "IconsInRow" ] ?? 10 ); }
            set { ViewState[ "IconsInRow" ] = value; }
        }

        [DefaultValue( "~/App_Obout/HTMLEditor/icons/" )]
        [Category( "Appearance" )]
        [Description( "Folder used for icons" )]
        public string IconsFolder
        {
            get { return ( string ) ( ViewState[ "IconsFolder" ] ?? "~/App_Obout/HTMLEditor/icons/" ); }
            set { ViewState[ "IconsFolder" ] = value; }
        }

        protected override string ScriptPath
        {
            get { return "~/App_Obout/HTMLEditor/Scripts/InsertIconPopup.js"; }
        }

        #endregion

        protected override string ClientControlType
        {
            get { return "CustomPopup.InsertIconPopup"; }
        }

        #region [ Methods ]

        protected override void FillContent()
        {
            Table table = new Table();
            TableRow row = null;
            TableCell cell;

            string iconsFolder = LocalResolveUrl( this.IconsFolder );
            if ( iconsFolder.Length > 0 )
            {
                string lastCh = iconsFolder.Substring( iconsFolder.Length - 1, 1 );
                if ( lastCh != "\\" && lastCh != "/" ) iconsFolder += "/";
            }

            if ( Directory.Exists( System.Web.HttpContext.Current.Server.MapPath( iconsFolder ) ) )
            {
                string[] files = Directory.GetFiles( System.Web.HttpContext.Current.Server.MapPath( iconsFolder ) );
                int j = 0;

                foreach ( string file in files )
                {
                    string ext = Path.GetExtension( file ).ToLower();
                    if ( ext == ".gif" || ext == ".jpg" || ext == ".jpeg" || ext == ".png" )
                    {
                        if ( j == 0 )
                        {
                            row = new TableRow();
                            table.Rows.Add( row );
                        }
                        cell = new TableCell();
                        System.Web.UI.WebControls.Image image = new System.Web.UI.WebControls.Image();
                        image.ImageUrl = iconsFolder + Path.GetFileName( file );
                        image.Attributes.Add( "onmousedown", "insertImage(\"" + iconsFolder + Path.GetFileName( file ) + "\")" );
                        image.Style[ HtmlTextWriterStyle.Cursor ] = "pointer";
                        cell.Controls.Add( image );
                        row.Cells.Add( cell );

                        j++;
                        if ( j == IconsInRow ) j = 0;
                    }
                }
            }
            table.Attributes.Add( "border", "0" );
            table.Attributes.Add( "cellspacing", "2" );
            table.Attributes.Add( "cellpadding", "0" );
            table.Style[ "background-color" ] = "transparent";

            Content.Add( table );
        }

        #endregion
    }

    [RequiredScript( typeof( Obout.Ajax.UI.HTMLEditor.Popups.Popup ) )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance" )]
    public class InsertCodeBlockPopup : Obout.Ajax.UI.HTMLEditor.Popups.Popup
    {

        #region [ Properties ]

        [DefaultValue( "cs;vb;js;xml;html;css" )]
        [Category( "Appearance" )]
        [Description( "Folder used for icons" )]
        public string SupportedLanguages
        {
            get { return ( string ) ( ViewState[ "SupportedLanguages" ] ?? "cs;vb;js;xml;html;css" ); }
            set { ViewState[ "SupportedLanguages" ] = value; }
        }

        protected override string ScriptPath
        {
            get { return "~/App_Obout/HTMLEditor/Scripts/InsertCodeBlockPopup.js"; }
        }

        #endregion

        protected override string ClientControlType
        {
            get { return "CustomPopup.InsertCodeBlockPopup"; }
        }

        #region [ Methods ]

        protected override void FillContent()
        {
            Table table = new Table();
            TableRow row = null;
            TableCell cell;

            string[] names = SupportedLanguages.Split( new char[] { ';' } );

            foreach ( string name in names )
            {
                row = new TableRow();
                table.Rows.Add( row );
                cell = new TableCell();
                cell.Controls.Add( new LiteralControl( name.ToUpper() ) );
                cell.Attributes.Add( "onmousedown", "insertCodeBlock(this)" );
                cell.Attributes.Add( "onmouseover", "this.style.backgroundColor='Blue';this.style.color='White';" );
                cell.Attributes.Add( "onmouseout", "this.style.backgroundColor='Transparent';this.style.color='Blue';" );
                cell.Attributes.Add( "onmousedown", "insertCodeBlock(this); return false;" );
                cell.Style[ HtmlTextWriterStyle.BackgroundColor ] = "Transparent";
                cell.Style[ HtmlTextWriterStyle.FontSize ] = "10pt";
                cell.Style[ HtmlTextWriterStyle.Padding ] = "2px";
                cell.Style[ HtmlTextWriterStyle.FontFamily ] = "Verdana";
                cell.Style[ HtmlTextWriterStyle.Color ] = "Blue";
                cell.Style[ HtmlTextWriterStyle.Cursor ] = "pointer";
                row.Cells.Add( cell );
            }

            table.Attributes.Add( "border", "0" );
            table.Attributes.Add( "cellspacing", "0" );
            table.Attributes.Add( "cellpadding", "0" );
            table.Style[ "background-color" ] = "transparent";

            Content.Add( table );
        }

        #endregion
    }

    [RequiredScript( typeof( Obout.Ajax.UI.HTMLEditor.Popups.Popup ) )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance" )]
    public class SmallImageProperties : Obout.Ajax.UI.HTMLEditor.Popups.ImageProperties
    {
        #region [ Properties ]

        protected override Obout.Ajax.UI.HTMLEditor.Popups.BrowseButton Browse { get { return null; } }
        protected override TextBox ElementCSS { get { return null; } }
        protected override TextBox ElementID { get { return null; } }
        protected override TextBox AlternateText { get { return null; } }

        #endregion
    }

    [RequiredScript( typeof( Obout.Ajax.UI.HTMLEditor.Popups.Popup ) )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance" )]
    public class SmallUrlProperties : Obout.Ajax.UI.HTMLEditor.Popups.LinkProperties
    {
        #region [ Properties ]

        protected override Obout.Ajax.UI.HTMLEditor.Popups.BrowseButton Browse { get { return null; } }
        protected override TextBox ElementCSS { get { return null; } }
        protected override TextBox ElementID { get { return null; } }
        protected override TextBox UrlTooltip { get { return null; } }
        protected override HtmlSelect Target { get { return null; } }

        public override Obout.Ajax.UI.HTMLEditor.Popups.LinkTarget DefaultTarget
        {
            get
            {
                return Obout.Ajax.UI.HTMLEditor.Popups.LinkTarget.New;
            }
        }

        #endregion
    }
    #endregion


}

