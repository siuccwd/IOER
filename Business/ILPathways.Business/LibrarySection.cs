/*********************************************************************************
= Author: Michael Parsons
=
= Date: Mar 08/2013
= Assembly: LRWarehouse.Business
= Description:
= Notes:
=
=
= Copyright 2013, Isle All rights reserved.
********************************************************************************/
using System; 
using System.Collections.Generic;

namespace ILPathways.Business
{
  ///<summary>
  ///Represents an object that describes a LibrarySection
  ///</summary>
  [Serializable]
    public class LibrarySection : BaseBusinessDataEntity, IBaseObject
  {
    ///<summary>
    ///Initializes a new instance of the LRWarehouse.Business.LibrarySection class.
    ///</summary>
    public LibrarySection() 
    {
        ParentLibrary = new Library();
    }

    #region Properties created from dictionary for LibrarySection

    private int _libraryId;
    /// <summary>
    /// Gets/Sets LibraryId
    /// </summary>
    public int LibraryId
    {
      get
      {
        return this._libraryId;
      }
      set
      {
        if (this._libraryId == value) {
          //Ignore set
        } else {
          this._libraryId = value;
          HasChanged = true;
        }
      }
    }

    private string _libraryTitle = "";
    /// <summary>
    /// Gets/Sets LibraryTitle
    /// </summary>
    public string LibraryTitle
    {
        get
        {
            return this._libraryTitle;
        }
        set
        {
            this._libraryTitle = value.Trim();
        }
    }
    private int _sectionTypeId;
    /// <summary>
    /// Gets/Sets SectionTypeId
    /// </summary>
    public int SectionTypeId
    {
      get
      {
        return this._sectionTypeId;
      }
      set
      {
        if (this._sectionTypeId == value) {
          //Ignore set
        } else {
          this._sectionTypeId = value;
          HasChanged = true;
        }
      }
    }

    private string _sectionType = "";
    /// <summary>
    /// Gets/Sets SectionType
    /// </summary>
    public string SectionType
    {
        get
        {
            return this._sectionType;
        }
        set
        {
            if ( this._sectionType == value )
            {
                //Ignore set
            }
            else
            {
                this._sectionType = value.Trim();
                HasChanged = true;
            }
        }
    }
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
        if (this._description == value) {
          //Ignore set
        } else {
          this._description = value.Trim();
          HasChanged = true;
        }
      }
    }

    private int _parentId;
    /// <summary>
    /// Gets/Sets ParentId
    /// </summary>
    public int ParentId
    {
      get
      {
        return this._parentId;
      }
      set
      {
        if (this._parentId == value) {
          //Ignore set
        } else {
          this._parentId = value;
          HasChanged = true;
        }
      }
    }
    private bool _isDefaultSection;
    /// <summary>IsDefaultSection
    /// </summary>
    public bool IsDefaultSection
    {
        get
        {
            return this._isDefaultSection;
        }
        set
        {
            if ( this._isDefaultSection == value )
            {
                //Ignore set
            }
            else
            {
                this._isDefaultSection = value;
                HasChanged = true;
            }
        }
    }

    /// <summary>
    /// Get/Set the Public access level
    /// </summary>
    public EObjectAccessLevel PublicAccessLevel { get; set; }

    /// <summary>
    /// Get/Set the access level for members of the same org
    /// </summary>
    public EObjectAccessLevel OrgAccessLevel { get; set; }

    private bool _isPublic;
    /// <summary>
    /// Gets/Sets IsPublic
    /// </summary>
    public bool IsPublic
    {
        get
        {
            //TODO - transition to use of PublicAccessLevel
            if ( ( int )PublicAccessLevel > 0 )
                return true;
            else
                return false;
            //return this._isPublic;
        }
        set
        {
            if ( this._isPublic == value )
            {
                //Ignore set
            }
            else
            {
                this._isPublic = value;
                HasChanged = true;
            }
        }
    }
    private bool _areContentsReadOnly;
    /// <summary>
    /// Gets/Sets AreContentsReadOnly
    /// </summary>
    public bool AreContentsReadOnly
    {
      get
      {
        return this._areContentsReadOnly;
      }
      set
      {
        if (this._areContentsReadOnly == value) {
          //Ignore set
        } else {
          this._areContentsReadOnly = value;
          HasChanged = true;
        }
      }
    }

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
    public string AvatarURL
    {
        get
        {
            return ImageUrl;
        }
        set
        {
            ImageUrl = value.Trim();
        }
    }
    #endregion

    #region Helper Methods
    public Library ParentLibrary { get; set; }
    
    public string SectionSummary()
    {
        return SectionSummary( "h1");
    } //
    public string SectionSummary( string h1Style)
    {
        
        string summary = "";
        
        summary = FormatLabelDataRow( "Collection: ", Title );

        if ( this.Description.Length > 0 )
            summary += FormatLabelDataRow( "Description: ", Description );

        summary += FormatLabelDataRow( "SectionType: ", SectionType );
        summary += FormatLabelDataRow( "Is DefaultSection: ", IsDefaultSection );
        summary += FormatLabelDataRow( "Is Public: ", IsPublic );

        //construct table
        summary = string.Format( "<h1 class='{0}'>{1}</h1>", h1Style, LibraryTitle )
            + "<table>" + summary + "</table>";

        return summary;
} //

    public string SectionSummaryFormatted()
    {
        return SectionSummaryFormatted( "" );
    } //

    public string SectionSummaryFormatted( string h1Style )
    {
        string summary = string.Format( "<h1 class='{0}'>{1}</h1>", h1Style, LibraryTitle );
        summary += FormatLabelData( "Collection:", Title );

        if ( this.Description.Length > 0 )
            summary += FormatLabelData( "Description:", Description );

        summary += FormatLabelData( "Section Type:", SectionType );
        summary += FormatLabelData( "Is DefaultSection: ", IsDefaultSection );
        summary += FormatLabelData( "Is Public:", IsPublic );
        summary += FormatLabelData( "Are Contents Read Only:", AreContentsReadOnly );

        return summary;
    } //
        		
        
    #endregion
  } // end class 
} // end Namespace 

