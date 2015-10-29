using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    [Serializable]
    public class Library : BaseBusinessDataEntity, IBaseObject 
    {
       
        public Library()
        {
            AllowJoinRequest = true;
			FriendlyTitle = "Library";
        }


        public static int PERSONAL_LIBRARY_ID = 1;

        public static int MY_EVALUATIONS_LIBRARY_SECTION_ID = 1;
        public static int MY_AUTHORED_LIBRARY_SECTION_ID = 2;
        public static int GENERAL_LIBRARY_SECTION_ID = 3;
   

        #region Properties created from dictionary for Library

        private string _title = "";
        /// <summary>
        /// Gets/Sets Title
        /// </summary>
        public string Title
        {
            get
            {
                return this._title;
            }
            set
            {
                if ( this._title == value )
                {
                    //Ignore set
                }
                else
                {
                    this._title = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private string _description = "";
        /// <summary>
        /// Gets/Sets Description
        /// </summary>
        public string Description
        {
            get
            {
                return this._description;
            }
            set
            {
                if ( this._description == value )
                {
                    //Ignore set
                }
                else
                {
                    this._description = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private int _libraryTypeId;
        /// <summary>
        /// Gets/Sets LibraryTypeId
        /// </summary>
        public int LibraryTypeId
        {
            get
            {
                return this._libraryTypeId;
            }
            set
            {
                if ( this._libraryTypeId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._libraryTypeId = value;
                    HasChanged = true;
                }
            }
        }

        private string _libraryType;
        /// <summary>
        /// Gets/Sets LibraryType
        /// </summary>
        public string LibraryType
        {
            get
            {
                return this._libraryType;
            }
            set
            {
                if ( this._libraryType == value )
                {
                    //Ignore set
                }
                else
                {
                    this._libraryType = value;
                    HasChanged = true;
                }
            }
        }


        private int _orgId;
        /// <summary>
        /// Gets/Sets OrgId
        /// </summary>
        public int OrgId
        {
            get
            {
                return this._orgId;
            }
            set
            {
                if ( this._orgId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._orgId = value;
                    HasChanged = true;
                }
            }
        }

        private string _org;
        /// <summary>
        /// Gets/Sets Organization
        /// </summary>
        public string Organization
        {
            get
            {
                if ( _org != null )
                    return this._org;
                else
                    return "";
            }
            set
            {
                if ( this._org == value )
                {
                    //Ignore set
                }
                else
                {
                    this._org = value;
                    HasChanged = true;
                }
            }
        }

        private bool _isDiscoverable;
        /// <summary>
        /// Gets/Sets IsDiscoverable
        /// </summary>
        public bool IsDiscoverable
        {
            get
            {
                return this._isDiscoverable;
            }
            set
            {
                if ( this._isDiscoverable == value )
                {
                    //Ignore set
                }
                else
                {
                    this._isDiscoverable = value;
                    HasChanged = true;
                }
            }
        }

        /// <summary>
        /// Get/Set the Public access level
        /// </summary>
        public EObjectAccessLevel PublicAccessLevel { get; set; }
        public int PublicAccessLevelInt { 
            get 
            {
                return ( int ) PublicAccessLevel;
            } 
        }
        /// <summary>
        /// Get/Set the access level for members of the same org
        /// </summary>
        public EObjectAccessLevel OrgAccessLevel { get; set; }
        public int OrgAccessLevelInt
        {
            get
            {
                return ( int ) OrgAccessLevel;
            }
        }

        public bool IsPublic
        {
            get
            {
                //TODO - transition to use of PublicAccessLevel
                if ( ( int ) PublicAccessLevel > 0 )
                    return true;
                else
                    return false;
            }
           
        }

        /// <summary>
        /// Gets/Sets AllowJoinRequest
        /// if false, will not allow requests to join the library
        /// </summary>
        public bool AllowJoinRequest { get; set; }

        private string _imageUrl = "";
        /// <summary>
        /// Gets/Sets ImageUrl
        /// </summary>
        public string ImageUrl
        {
            get
            {
                return this._imageUrl;
            }
            set
            {
                if ( this._imageUrl == value )
                {
                    //Ignore set
                }
                else
                {
                    this._imageUrl = value.Trim();
                    HasChanged = true;
                }
            }
        }


		/// <summary>
		/// Return a url friendly title
		/// ==> this is no longer set in this object, it needs to be set from external methods like:
		/// ILPathways.Utilities.UtilityManager.UrlFriendlyTitle()
		/// </summary>
		public string FriendlyTitle 
		{
			get
			{
				//return UrlFriendlyTitle( this._title );
				if ( string.IsNullOrWhiteSpace( _friendlyTitle ) )
					_friendlyTitle = "Library";
				return this._friendlyTitle;
			}
			set
			{
				if ( this._friendlyTitle != value )
				{
					this._friendlyTitle = value.Trim();
				}
			}
		} //
		private string _friendlyTitle = "Library";

		/// <summary>
		/// Return a friendly url
		/// ==> this is no longer set in this object, it needs to be set from 
		/// </summary>
        public string FriendlyUrl
        {
            get
            {
                if ( Id == 0 )
                    return "";
                else
                    return string.Format( "/Library/{0}/{1}", Id, this.FriendlyTitle );
            }
        }

		#endregion


		#region External
        public bool IsMyPersonalLibrary( int userId)
        {
            if ( LibraryTypeId == PERSONAL_LIBRARY_ID && this.CreatedById == userId )
                return true;
            else
                return false;
        }

		#endregion


		#region Helper Methods

        public string LibrarySummary()
        {
            string summary = string.Format("<strong>{0}</strong>", Title);
            //string template = "<div style='text-align:right; width: 100px;'>{0}</div>";

            if ( this.Description.Length > 0 )
                summary = FormatHtmlList( summary, Description );
                        
            if ( _orgId > 0 )
                summary = FormatHtmlList( summary, "Organization: " + Organization );

            summary = FormatHtmlList( summary, "Library Type: " + LibraryType );
            summary = FormatHtmlList( summary, "Public Access: " + PublicAccessLevel.ToString() );

            summary = FormatHtmlList( summary, "Organization Access: " + OrgAccessLevel.ToString() );

            return summary;
} //

        public string LibrarySummaryFormatted()
        {
            return LibrarySummaryFormatted( "" );
        } //

        public string LibrarySummaryFormatted( string h1Style)
        {

            string summary = string.Format( "<h1 class='{0}'>{1}</h1>", h1Style, Title );

            if ( _orgId > 0 )
                summary += FormatLabelData( "Organization:", Organization );

            if ( this.Description.Length > 0 )
                summary += FormatLabelData( "Description:", Description );

            summary += FormatLabelData( "Library Type:", LibraryType );
            summary += FormatLabelData( "Public Access: ", PublicAccessLevel.ToString() );
            summary += FormatLabelData( "Organization Access: ", OrgAccessLevel.ToString() );

            return summary;
        } //
        		
        
        #endregion
    }
}

