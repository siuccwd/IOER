/*********************************************************************************
= Author: Michael Parsons
=
= Date: Nov 30/2009
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
  ///Represents an object that describes a AppItemRelationship
  ///</summary>
  [Serializable]
  public class AppItemRelationship : BaseBusinessDataEntity
  {
    ///<summary>
    ///Initializes a new instance of the ILPathways.Business.AppItemRelationship class.
    ///</summary>
    public AppItemRelationship() {}

    

    #region Properties created from dictionary for AppItemRelationship


		private Guid _parentRowId;
    /// <summary>
    /// Gets/Sets ParentRowId
    /// </summary>
		public Guid ParentRowId
    {
      get
      {
        return this._parentRowId;
      }
      set
      {
        if (this._parentRowId == value) {
          //Ignore set
        } else {
          this._parentRowId = value;
          HasChanged = true;
        }
      }
    }

		private Guid _relatedRowId;
    /// <summary>
    /// Gets/Sets RelatedRowId
    /// </summary>
		public Guid RelatedRowId
    {
      get
      {
        return this._relatedRowId;
      }
      set
      {
        if (this._relatedRowId == value) {
          //Ignore set
        } else {
          this._relatedRowId = value;
          HasChanged = true;
        }
      }
    }

    private int _sequenceNbr = 1;
    /// <summary>
    /// Gets/Sets SequenceNbr
    /// </summary>
    public int SequenceNbr
    {
      get
      {
        return this._sequenceNbr;
      }
      set
      {
        if (this._sequenceNbr == value) {
          //Ignore set
        } else {
          this._sequenceNbr = value;
          HasChanged = true;
        }
      }
    }

    private string _relationship = "AppItem-AppItem";
    /// <summary>
    /// Gets/Sets Relationship
    /// </summary>
    public string Relationship
    {
      get
      {
        return this._relationship;
      }
      set
      {
        if (this._relationship == value) {
          //Ignore set
        } else {
          this._relationship = value.Trim();
          HasChanged = true;
        }
      }
    }


    #endregion

		#region composite properties

		//??any current parent AppItemRelationship entity
		private AppItem _relatedParentItem;
		/// <summary>
		/// Gets/Sets RelatedParentItem
		/// </summary>
		public AppItem RelatedParentItem
		{
			get
			{
				return this._relatedParentItem;
			}
			set
			{
				if ( this._relatedParentItem == value )
				{
					//Ignore set
				} else
				{
					this._relatedParentItem = value;
				}
			}
		}//

		private AppItem _relatedChildItem;
		/// <summary>
		/// Gets/Sets RelatedChildItem
		/// </summary>
		public AppItem RelatedChildItem
		{
			get
			{
				return this._relatedChildItem;
			}
			set
			{
				if ( this._relatedChildItem == value )
				{
					//Ignore set
				} else
				{
					this._relatedChildItem = value;
				}
			}
		}//
		#endregion
  } // end class 
} // end Namespace 

