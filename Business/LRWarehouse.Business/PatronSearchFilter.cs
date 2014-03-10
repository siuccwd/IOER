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
  ///Represents an object that describes a PatronSearchFilter
  ///</summary>
  [Serializable]
  public class PatronSearchFilter : BaseBusinessDataEntity
  {
    ///<summary>
    ///Initializes a new instance of the LRWarehouse.Business.PatronSearchFilter class.
    ///</summary>
    public PatronSearchFilter() {}

    

    #region Properties created from dictionary for PatronSearchFilter


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
    private string _filter = "";
    /// <summary>
    /// Gets/Sets Filter
    /// </summary>
    public string Filter
    {
      get
      {
        return this._filter;
      }
      set
      {
        if (this._filter == value) {
          //Ignore set
        } else {
          this._filter = value.Trim();
          HasChanged = true;
        }
      }
    }


    #endregion
  } // end class 
} // end Namespace 

