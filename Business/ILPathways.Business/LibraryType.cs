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
  ///Represents an object that describes a LibraryType
  ///</summary>
  [Serializable]
  public class LibraryType : BaseBusinessDataEntity
  {
    ///<summary>
    ///Initializes a new instance of the LRWarehouse.Business.LibraryType class.
    ///</summary>
    public LibraryType() {}

    

    #region Properties created from dictionary for LibraryType


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


    #endregion
  } // end class 
} // end Namespace 

