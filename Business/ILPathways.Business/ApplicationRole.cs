/*********************************************************************************
= Author: Michael Parsons
=
= Date: Aug 28/2009
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
  ///Represents an object that describes a ApplicationRole
  ///</summary>
  [Serializable]
  public class ApplicationRole : BaseBusinessDataEntity
  {
    ///<summary>
    ///Initializes a new instance of the ILPathways.Business.ApplicationRole class.
    ///</summary>
    public ApplicationRole() {}

    #region Properties created from dictionary for ApplicationRole

    private int _roleCode;
    /// <summary>
    /// Gets/Sets RoleCode
    /// </summary>
		public int RoleCode
    {
      get
      {
        return this._roleCode;
      }
      set
      {
        if (this._roleCode == value) {
          //Ignore set
        } else {
          this._roleCode = value;
          HasChanged = true;
        }
      }
    }

    private string _name = "";
    /// <summary>
    /// Gets/Sets Name
    /// </summary>
		public string RoleName
    {
      get
      {
        return this._name;
      }
      set
      {
        if (this._name == value) {
          //Ignore set
        } else {
          this._name = value.Trim();
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

