/*********************************************************************************
= Author: Michael Parsons
=
= Date: Nov 12/2009
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
  ///Represents an object that describes a AppItemType
  ///</summary>
  [Serializable]
  public class AppItemType : BaseBusinessDataEntity
  {
    ///<summary>
    ///Initializes a new instance of the ILPathways.Business.AppItemType class.
    ///</summary>
    public AppItemType() {}

    

    #region Properties created from dictionary for AppItemType


    private string _typeCode = "";
    /// <summary>
    /// Gets/Sets TypeCode
    /// </summary>
    public string TypeCode
    {
      get
      {
        return this._typeCode;
      }
      set
      {
        if (this._typeCode == value) {
          //Ignore set
        } else {
          this._typeCode = value.Trim();
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
				} else
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

    private bool _hasApproval;
    /// <summary>
    /// Gets/Sets HasApproval
    /// </summary>
    public bool HasApproval
    {
      get
      {
        return this._hasApproval;
      }
      set
      {
        if (this._hasApproval == value) {
          //Ignore set
        } else {
          this._hasApproval = value;
          HasChanged = true;
        }
      }
    }

    private short _maxVersions;
    /// <summary>
    /// Gets/Sets MaxVersions
    /// </summary>
    public short MaxVersions
    {
      get
      {
        return this._maxVersions;
      }
      set
      {
        if (this._maxVersions == value) {
          //Ignore set
        } else {
          this._maxVersions = value;
          HasChanged = true;
        }
      }
    }

    #endregion
  } // end class 
} // end Namespace 

