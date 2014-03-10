/*********************************************************************************
= Author: Michael Parsons
=
= Date: Feb 12/2009
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
  ///Represents an object that describes a workNetObject
  ///</summary>
  [Serializable]
  public class ApplicationObject : BaseBusinessDataEntity
  {
    ///<summary>
    ///Initializes a new instance of the ILPathways.Business.workNetObject class.
    ///</summary>
		public ApplicationObject() { }

    

    #region Properties created from dictionary for workNetObject


    private string _objectName = "";
    /// <summary>
    /// Gets/Sets ObjectName
    /// </summary>
    public string ObjectName
    {
      get
      {
        return this._objectName;
      }
      set
      {
        if (this._objectName == value) {
          //Ignore set
        } else {
          this._objectName = value.Trim();
          HasChanged = true;
        }
      }
    }

    private string _displayName = "";
    /// <summary>
    /// Gets/Sets DisplayName
    /// </summary>
    public string DisplayName
    {
      get
      {
        return this._displayName;
      }
      set
      {
        if (this._displayName == value) {
          //Ignore set
        } else {
          this._displayName = value.Trim();
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


    private int _subObjectId;
    /// <summary>
    /// Gets/Sets SubObjectId
    /// </summary>
    public int SubObjectId
    {
      get
      {
        return this._subObjectId;
      }
      set
      {
        if (this._subObjectId == value) {
          //Ignore set
        } else {
          this._subObjectId = value;
          HasChanged = true;
        }
      }
    }

    private string _subObjectName = "";
    /// <summary>
    /// Gets/Sets SubObjectName
    /// </summary>
    public string SubObjectName
    {
      get
      {
        return this._subObjectName;
      }
      set
      {
        if (this._subObjectName == value) {
          //Ignore set
        } else {
          this._subObjectName = value.Trim();
          HasChanged = true;
        }
      }
    }

    private string _objectType = "";
    /// <summary>
    /// Gets/Sets ObjectType
    /// </summary>
    public string ObjectType
    {
      get
      {
        return this._objectType;
      }
      set
      {
        if (this._objectType == value) {
          //Ignore set
        } else {
          this._objectType = value.Trim();
          HasChanged = true;
        }
      }
    }

		private string _relatedUrl = "";
    /// <summary>
		/// Gets/Sets RelatedUrl
    /// </summary>
		public string RelatedUrl
    {
      get
      {
				return this._relatedUrl;
      }
      set
      {
				if ( this._relatedUrl == value )
				{
          //Ignore set
        } else {
					this._relatedUrl = value.Trim();
          HasChanged = true;
        }
      }
    }


    #endregion
  } // end class 
} // end Namespace 

