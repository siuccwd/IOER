/*********************************************************************************
= Author: Michael Parsons
=
= Date: Jan 21/2009
= Assembly: ILPathways.Business
= Description:
= Notes:
=
=
= Copyright 2009, Illinois workNet All rights reserved.
********************************************************************************/
using System; 

namespace ILPathways.Business
{
  ///<summary>
  ///Represents an object that describes a ApplicationGroupPrivilege.
	/// Extends ObjectPrivilege to manage access to a group
  ///</summary>
  [Serializable]
	public class ApplicationGroupPrivilege : ObjectPrivilege
  {
    ///<summary>
    ///Initializes a new instance of the ILPathways.Business.ApplicationGroupPrivilege class.
    ///</summary>
    public ApplicationGroupPrivilege() {}

    #region Properties created from dictionary for ApplicationGroupPrivilege

    private int _groupId;
    /// <summary>
    /// Gets/Sets GroupId
    /// </summary>
    public int GroupId
    {
      get
      {
        return this._groupId;
      }
      set
      {
        if (this._groupId == value) {
          //Ignore set
        } else {
          this._groupId = value;
          HasChanged = true;
        }
      }
    }


    #endregion
  } // end class 
} // end Namespace 

