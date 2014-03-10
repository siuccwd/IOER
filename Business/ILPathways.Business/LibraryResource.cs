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
  ///Represents an object that describes a LibraryResource
  ///</summary>
  [Serializable]
  public class LibraryResource : BaseBusinessDataEntity
  {
    ///<summary>
    ///Initializes a new instance of the LRWarehouse.Business.LibraryResource class.
    ///</summary>
    public LibraryResource() 
    {
        ResourceLibrary = new Library();
        ResourceSection = new LibrarySection();
    }

    #region Properties created from dictionary for LibraryResource

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
            if ( this._libraryId == value )
            {
                //Ignore set
            }
            else
            {
                this._libraryId = value;
                HasChanged = true;
            }
        }
    }

    private int _librarySectionId;
    /// <summary>
    /// Gets/Sets LibrarySectionId
    /// </summary>
    public int LibrarySectionId
    {
      get
      {
        return this._librarySectionId;
      }
      set
      {
        if (this._librarySectionId == value) {
          //Ignore set
        } else {
          this._librarySectionId = value;
          HasChanged = true;
        }
      }
    }

    private int _resourceIntId;
    /// <summary>
    /// Gets/Sets ResourceIntId
    /// </summary>
    public int ResourceIntId
    {
      get
      {
        return this._resourceIntId;
      }
      set
      {
        if (this._resourceIntId == value) {
          //Ignore set
        } else {
          this._resourceIntId = value;
          HasChanged = true;
        }
      }
    }
    public int ResourceVersionId
    {
        get
        {
            return this._resourceIntId;
        }
        set
        {
            if ( this._resourceIntId == value )
            {
                //Ignore set
            }
            else
            {
                this._resourceIntId = value;
                HasChanged = true;
            }
        }
    }
    private string _comment = "";
    /// <summary>
    /// Gets/Sets Comment
    /// </summary>
    public string Comment
    {
      get
      {
        return this._comment;
      }
      set
      {
        if (this._comment == value) {
          //Ignore set
        } else {
          this._comment = value.Trim();
          HasChanged = true;
        }
      }
    }


    #endregion


    #region Properties of external objects

    private Library _resourceLibrary;
    public Library ResourceLibrary
    {
        get
        {
            if ( _resourceLibrary == null )
                _resourceLibrary = new Library();

            return this._resourceLibrary;
        }
        set
        {
            if ( this._resourceLibrary == value )
            {
                //Ignore set
            }
            else
            {
                this._resourceLibrary = value;
                HasChanged = true;
            }
        }
    } //

    private LibrarySection _resourceSection;
    public LibrarySection ResourceSection
    {
        get
        {
            if ( _resourceSection == null )
                _resourceSection = new LibrarySection();

            return this._resourceSection;
        }
        set
        {
            if ( this._resourceSection == value )
            {
                //Ignore set
            }
            else
            {
                this._resourceSection = value;
                HasChanged = true;
            }
        }
    } //

    #endregion

  } // end class 
} // end Namespace 

