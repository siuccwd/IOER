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

namespace LRWarehouse.Business
{
  ///<summary>
  ///Represents an object that describes a PatronNote
  ///</summary>
  [Serializable]
  public class PatronNote : BaseBusinessDataEntity
  {
    ///<summary>
    ///Initializes a new instance of the LRWarehouse.Business.PatronNote class.
    ///</summary>
    public PatronNote() {}

    

    #region Properties created from dictionary for PatronNote


    private int _userId;
    /// <summary>
    /// Gets/Sets UserId
    /// </summary>
    public int UserId
    {
      get
      {
        return this._userId;
      }
      set
      {
        if (this._userId == value) {
          //Ignore set
        } else {
          this._userId = value;
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
        if (this._title == value) {
          //Ignore set
        } else {
          this._title = value.Trim();
          HasChanged = true;
        }
      }
    }

    private string _category = "";
    /// <summary>
    /// Gets/Sets Category
    /// </summary>
    public string Category
    {
      get
      {
        return this._category;
      }
      set
      {
        if (this._category == value) {
          //Ignore set
        } else {
          this._category = value.Trim();
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

