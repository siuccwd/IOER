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

//namespace ILPathways.HTMLEditor.CustomPopups
namespace CustomPopups
{
    #region [ Customized 'Image properties' popup class ]
    // Declare the customized 'Image properties' popup class
    public class ImagesPropertiesWithCustomBrowser : Obout.Ajax.UI.HTMLEditor.Popups.ImageProperties
    {
        #region [ Fields ]

        private Obout.Ajax.UI.HTMLEditor.Popups.BrowseButton _Button = null;

        #endregion

        #region [ Properties ]

        // redefine the 'Browse' button of the popup
        protected override Obout.Ajax.UI.HTMLEditor.Popups.BrowseButton Browse
        {
            get
            {
                if ( _Button == null )
                {
                    _Button = new CustomBrowseButton1( this );
                    _Button.Style[ HtmlTextWriterStyle.Width ] = "70px";
                }
                return _Button;
            }
        }

        #endregion
    }
    #endregion

    #region [ Custom 'BrowseButton' class ]

    public class CustomBrowseButton1 : Obout.Ajax.UI.HTMLEditor.Popups.BrowseButton
    {
        #region [ Fields ]

        private Obout.Ajax.UI.HTMLEditor.Popups.Popup _relatedPopup = null;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new button
        /// </summary>
        public CustomBrowseButton1( Obout.Ajax.UI.HTMLEditor.Popups.Popup ownerPopup )
            : base( ownerPopup )
        {
        }

        #endregion

        #region [ Properties ]

        public sealed override Obout.Ajax.UI.HTMLEditor.Popups.Popup RelatedPopup
        {
            get
            {
                if ( _relatedPopup == null )
                {
                    _relatedPopup = new CustomImageBrowser1();
                }
                return _relatedPopup;
            }
        }

        #endregion
    }

    #endregion

    #region [ Custom 'ImageBrowser' class ]

    public class CustomImageBrowser1 : Obout.Ajax.UI.HTMLEditor.Popups.ImageBrowser
    {
        // our custom button for the manager's toolbar
        CustomManagerButton1 CButton;

        // override the method that generates popup's markup
        protected override void OkCancelFillContent()
        {
            // do default
            base.OkCancelFillContent();

            // get the main table of the markup
            Table table = Iframe.Parent.Parent.Parent as Table;
            // get the cell in the row where all manager's buttons are in (toolbar)
            TableCell cell = table.Rows[ 0 ].Cells[ 0 ];

            // add our custom button to the toolbar
            CButton = new CustomManagerButton1();
            CButton.ToolTip = GetField( "CustomManagerButton1", "Tooltip", "User's custom action" );
            CButton.Name = "customaction1";
            CButton.Style[ HtmlTextWriterStyle.MarginRight ] = "2px";
            cell.Controls.Add( CButton );
        }

        // ovveride the base method
        protected override void DescribeComponent( ScriptComponentDescriptor descriptor )
        {
            // register client-side handler for our custom button
            RegisteredHandlers.Add( new Obout.Ajax.UI.HTMLEditor.Popups.RegisteredField( "customaction1", CButton ) );
            base.DescribeComponent( descriptor );
        }

        // what file in the client-side type is located
        protected override string ScriptPath
        {
            get { return "~/App_Obout/HTMLEditor/Scripts/CustomImageBrowser1.js"; }
        }

        // what client-side type to initiate
        protected override string ClientControlType
        {
            get { return "CustomPopup.CustomImageBrowser1"; }
        }
    }

    #endregion

    #region [ Custom button for the Manager's toolbar ]
    public class CustomManagerButton1 : Obout.Ajax.UI.HTMLEditor.Popups.PopupImageButton
    {
        #region [ Constructors ]

        /// <summary>
        /// Initializes a new ImageButton
        /// </summary>
        public CustomManagerButton1()
            : base()
        {
            CssClass = "oae_popup_imagebutton_tr";
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Base name of the button's image
        /// </summary>
        protected override string BaseImageName
        {
            get { return "CustomManagerButton1"; }
        }

        /// <summary>
        /// Folder with the button images
        /// </summary>
        protected override string ButtonImagesFolder
        {
            get { return "~/App_Obout/HTMLEditor/customButtons/"; }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Override the default
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender( EventArgs e )
        {
            this.Attributes.Add( "align", "middle" );
            base.OnPreRender( e );
        }

        #endregion
    }
    #endregion



}