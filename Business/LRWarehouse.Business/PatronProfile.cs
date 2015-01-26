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
  ///Represents an object that describes a PatronProfile
  ///</summary>
  [Serializable]
  public class PatronProfile : BaseBusinessDataEntity
  {
    ///<summary>
    ///Initializes a new instance of the LRWarehouse.Business.PatronProfile class.
    ///</summary>
    public PatronProfile() {}

    #region Properties created from dictionary for PatronProfile

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
                this._imageUrl = ( value == null ) ? "" : value.Trim();
                HasChanged = true;
            }
        }
    }

    private string _mainPhone = "";
    /// <summary>
    /// Gets/Sets MainPhone
    /// </summary>
    public string MainPhone
    {
      get
      {
        return this._mainPhone;
      }
      set
      {
        if (this._mainPhone == value) {
          //Ignore set
        } else {
            this._mainPhone = ( value == null ) ? "" : value.Trim();
          HasChanged = true;
        }
      }
    }

    private string _jobTitle = "";
    /// <summary>
    /// Gets/Sets JobTitle
    /// </summary>
    public string JobTitle
    {
      get
      {
        return this._jobTitle;
      }
      set
      {
        if (this._jobTitle == value) {
          //Ignore set
        } else {
            this._jobTitle = (value == null) ? "" : value.Trim();
          HasChanged = true;
        }
      }
    }

    private int _publishingRoleId;
    /// <summary>
    /// Gets/Sets PublishingRoleId
    /// </summary>
    public int PublishingRoleId
    {
      get
      {
        return this._publishingRoleId;
      }
      set
      {
        if (this._publishingRoleId == value) {
          //Ignore set
        } else {
          this._publishingRoleId = value;
          HasChanged = true;
        }
      }
    }
    private string _publishingRole = "";
    /// <summary>
    /// Gets/Sets PublishingRole
    /// </summary>
    public string PublishingRole
    {
        get
        {
            return this._publishingRole;
        }
        set
        {
            this._publishingRole = value.Trim();
        }
    }

    private string _roleProfile = "";
    /// <summary>
    /// Gets/Sets RoleProfile
    /// </summary>
    public string RoleProfile
    {
      get
      {
        return this._roleProfile;
      }
      set
      {
          this._roleProfile = value.Trim();
      }
    }

    private int _organizationId;
    /// <summary>
    /// Gets/Sets OrganizationId
    /// </summary>
    public int OrganizationId
    {
      get
      {
        return this._organizationId;
      }
      set
      {
        if (this._organizationId == value) {
          //Ignore set
        } else {
          this._organizationId = value;
          HasChanged = true;
        }
      }
    }
    private string _organization = "";
    /// <summary>
    /// Gets/Sets Organization
    /// </summary>
    public string Organization
    {
        get
        {
            return this._organization;
        }
        set
        {
            this._organization = value.Trim();
        }
    }
    #endregion
  } // end class 
} // end Namespace 

