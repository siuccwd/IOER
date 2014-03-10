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
  ///Represents an object that describes a LibrarySectionType
  ///</summary>
  [Serializable]
  public class LibrarySectionType : BaseBusinessDataEntity
  {
    ///<summary>
    ///Initializes a new instance of the LRWarehouse.Business.LibrarySectionType class.
    ///</summary>
    public LibrarySectionType() {}

    

    #region Properties created from dictionary for LibrarySectionType


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
        if (this._title == value) {
          //Ignore set
        } else {
          this._title = value.Trim();
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

    private string _decription = "";
    /// <summary>
    /// Gets/Sets Decription
    /// </summary>
    public string Decription
    {
      get
      {
        return this._decription;
      }
      set
      {
        if (this._decription == value) {
          //Ignore set
        } else {
          this._decription = value.Trim();
          HasChanged = true;
        }
      }
    }

    private string _sectionCode = "";
    /// <summary>
    /// Gets/Sets SectionCode
    /// </summary>
    public string SectionCode
    {
      get
      {
        return this._sectionCode;
      }
      set
      {
        if (this._sectionCode == value) {
          //Ignore set
        } else {
          this._sectionCode = value.Trim();
          HasChanged = true;
        }
      }
    }


    #endregion
  } // end class 
} // end Namespace 

