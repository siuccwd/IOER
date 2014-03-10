
/*********************************************************************************
= Author: Michael Parsons
=
= Date: Sep 19/2013
= Assembly: ILPathways.Business
= Description:
= Notes:
=
=
= Copyright 2013, Illinois workNet All rights reserved.
********************************************************************************/
using System; 
using System.Collections.Generic; 

namespace ILPathways.Business
{
  ///<summary>
  ///Represents an object that describes a OrganizationRequest
  ///</summary>
  [Serializable]
  public class OrganizationRequest : BaseBusinessDataEntity
  {
    ///<summary>
    ///Initializes a new instance of the ILPathways.Business.OrganizationRequest class.
    ///</summary>
    public OrganizationRequest() {}

    

    #region Properties created from dictionary for OrganizationRequest


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
        if (this._orgId == value) {
          //Ignore set
        } else {
          this._orgId = value;
          HasChanged = true;
        }
      }
    }

    private string _organzationName = "";
    /// <summary>
    /// Gets/Sets OrganzationName
    /// </summary>
    public string OrganzationName
    {
      get
      {
        return this._organzationName;
      }
      set
      {
        if (this._organzationName == value) {
          //Ignore set
        } else {
          this._organzationName = value.Trim();
          HasChanged = true;
        }
      }
    }

    private string _action = "";
    /// <summary>
    /// Gets/Sets Action
    /// </summary>
    public string Action
    {
      get
      {
        return this._action;
      }
      set
      {
        if (this._action == value) {
          //Ignore set
        } else {
          this._action = value.Trim();
          HasChanged = true;
        }
      }
    }


    #endregion
  } // end class 
} // end Namespace 


