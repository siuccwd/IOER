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
  ///Represents an object that describes a AppItem
  ///</summary>
  [Serializable]
  public class AppItem : BaseBusinessDataEntity, IAppItem
  {
      /// <summary>
      /// AppItem type of GeneralItem
      /// </summary>
      public static int GeneralItemType = 1;

      /// <summary>
      /// AppItem type of HelpItem
      /// </summary>
      public static int HelpItemType = 1000;
      /// <summary>
      /// AppItem type of FaqItem
      /// </summary>
      public static int FAQItemType = 1010;

      /// <summary>
      /// AppItem type of AnnouncementItem
      /// </summary>
      public static int AnnouncementItemType = 1020;

      /// <summary>
      /// AppItem type of ActivityItem
      /// </summary>
      public static int ActivityItemType = 1030;


      /// <summary>
      /// AppItem type of ActivityItem
      /// </summary>
      public static int SuccessStoryItemType = 1035;

      /// <summary>
      /// AppItem type of SuccessLetterItemType
      /// </summary>
      public static int SuccessLetterItemType = 1036;

      /// <summary>
      /// AppItem type of ExpertSystemItem
      /// </summary>
      public static int ExpertSystemItemType = 1055;

      /// <summary>
      /// AppItem type of RssFeed Item
      /// </summary>
      public static int RssFeedItemType = 1060;

      /// <summary>
      /// AppItem type of HighlightedTopic
      /// </summary>
      public static int HighlightedTopicItemType = 1065;

      /// <summary>
      /// AppItem type of Video
      /// </summary>
      public static int Video = 1070;

      /// <summary>
      /// AppItem type of AppItemGroup.  Useful for Video Playlists and grouping Featured Items together.
      /// </summary>
      public static int AppItemGroup = 1075;

      /// <summary>
      /// AppItem type of Tool.  Useful for displaying tools on a page
      /// </summary>
      public static int ToolItemType = 1080;

      /// <summary>
      /// AppItem type of News.  Used for E-mailing news items, which were split from announcements.
      /// </summary>
      public static int NewsItemType = 1085;

      /// <summary>
      /// AppItem type of MapItem.  Used for displaying markers on a map.
      /// </summary>
      public static int MapItemType = 1090;
    ///<summary>
    ///Initializes a new instance of the ILPathways.Business.AppItem class.
    ///</summary>
    public AppItem()
		{
			AppItemImage = new ImageStore();
		}
    

    #region Properties created from dictionary for AppItem


    private int _versionNbr = 1;
    /// <summary>
    /// Gets/Sets VersionNbr
    /// </summary>
    public int VersionNbr
    {
      get
      {
        return this._versionNbr;
      }
      set
      {
        if (this._versionNbr == value) {
          //Ignore set
        } else {
          this._versionNbr = value;
          HasChanged = true;
        }
      }
    }

		private Guid _parentRowId;
		/// <summary>
		/// Gets/Sets the RowId of the parent AppItem
		/// </summary>
		public Guid ParentRowId
		{
			get
			{
				return this._parentRowId;
			}
			set
			{
				if ( this._parentRowId == value )
				{
					//Ignore set
				} else
				{
					this._parentRowId = value;
					HasChanged = true;
				}
			}
		}//

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
				if ( this._sequenceNbr == value )
				{
					//Ignore set
				} else
				{
					this._sequenceNbr = value;
					HasChanged = true;
				}
			}
		}//

    private int _typeId;
    /// <summary>
    /// Gets/Sets TypeId
    /// </summary>
		public virtual int TypeId
    {
      get
      {
        return this._typeId;
      }
      set
      {
        if (this._typeId == value) {
          //Ignore set
        } else {
          this._typeId = value;
          HasChanged = true;
        }
      }
    }

		private string _appItemCode = "";
    /// <summary>
    /// Gets/Sets Type
    /// </summary>
		public string AppItemCode
    {
      get
      {
				return this._appItemCode;
      }
      set
      {
				if ( this._appItemCode == value )
				{
          //Ignore set
        } else {
					this._appItemCode = value.Trim();
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


    private string _title2 = "";
    /// <summary>
    /// Gets/Sets Title2
    /// </summary>
    public string Title2
    {
        get
        {
            return this._title2;
        }
        set
        {
            if ( this._title2 == value )
            {
                //Ignore set
            }
            else
            {
                this._title2 = value.Trim();
                HasChanged = true;
            }
        }
    }

    private string _description2 = "";
    /// <summary>
    /// Gets/Sets Description2
    /// </summary>
    public string Description2
    {
        get
        {
            return this._description2;
        }
        set
        {
            if ( this._description2 == value )
            {
                //Ignore set
            }
            else
            {
                this._description2 = value.Trim();
                HasChanged = true;
            }
        }
    }

    private string _userControl = "";
    /// <summary>
    /// Gets/Sets UserControl
    /// </summary>
    public string UserControl
    {
        get
        {
            return this._userControl;
        }
        set
        {
            if ( this._userControl == value )
            {
                //Ignore set
            }
            else
            {
                this._userControl = value.Trim();
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

    private string _subcategory = "";
    /// <summary>
    /// Gets/Sets Subcategory
    /// </summary>
    public string Subcategory
    {
      get
      {
        return this._subcategory;
      }
      set
      {
        if (this._subcategory == value) {
          //Ignore set
        } else {
          this._subcategory = value.Trim();
          HasChanged = true;
        }
      }
    }

    private string _status = "";
    /// <summary>
    /// Gets/Sets Status
    /// </summary>
    public string Status
    {
      get
      {
        return this._status;
      }
      set
      {
        if (this._status == value) {
          //Ignore set
        } else {
          this._status = value.Trim();
          HasChanged = true;
        }
      }
    }

    private DateTime _startDate;
    /// <summary>
    /// Gets/Sets StartDate
    /// </summary>
    public DateTime StartDate
    {
      get
      {
        return this._startDate;
      }
      set
      {
        if (this._startDate == value) {
          //Ignore set
        } else {
          this._startDate = value;
          HasChanged = true;
        }
      }
    }

    private DateTime _endDate;
    /// <summary>
    /// Gets/Sets EndDate
    /// </summary>
    public DateTime EndDate
    {
      get
      {
        return this._endDate;
      }
      set
      {
        if (this._endDate == value) {
          //Ignore set
        } else {
          this._endDate = value;
          HasChanged = true;
        }
      }
    }

    private DateTime _expiryDate;
    /// <summary>
    /// Gets/Sets ExpiryDate
    /// </summary>
    public DateTime ExpiryDate
    {
      get
      {
        return this._expiryDate;
      }
      set
      {
        if (this._expiryDate == value) {
          //Ignore set
        } else {
          this._expiryDate = value;
          HasChanged = true;
        }
      }
    }

    private DateTime _approved;
    /// <summary>
    /// Gets/Sets Approved
    /// </summary>
    public DateTime Approved
    {
      get
      {
        return this._approved;
      }
      set
      {
        if (this._approved == value) {
          //Ignore set
        } else {
          this._approved = value;
          HasChanged = true;
        }
      }
    }

    private int _approvedById;
    /// <summary>
    /// Gets/Sets ApprovedById
    /// </summary>
    public int ApprovedById
    {
      get
      {
        return this._approvedById;
      }
      set
      {
        if (this._approvedById == value) {
          //Ignore set
        } else {
          this._approvedById = value;
          HasChanged = true;
        }
      }
    }


		private Guid _relatedObjectRowId;
		/// <summary>
		/// Gets/Sets the RowId of the parent AppItem
		/// </summary>
		public Guid RelatedObjectRowId
		{
			get
			{
				return this._relatedObjectRowId;
			}
			set
			{
				if ( this._relatedObjectRowId == value )
				{
					//Ignore set
				} else
				{
					this._relatedObjectRowId = value;
					HasChanged = true;
				}
			}
		}//

		private int _imageId;
		/// <summary>
		/// Gets/Sets ImageId
		/// </summary>
		public int ImageId
		{
			get
			{
				return this._imageId;
			}
			set
			{
				if ( this._imageId == value )
				{
					//Ignore set
				} else
				{
					this._imageId = value;
					HasChanged = true;
				}
			}
		}

		private string _string1 = "";
		/// <summary>
		/// Gets/Sets String1
		/// </summary>
		public string String1
		{
			get
			{
				return this._string1;
			}
			set
			{
				if ( this._string1 == value )
				{
					//Ignore set
				} else
				{
					this._string1 = value.Trim();
					HasChanged = true;
				}
			}
		}

		private string _string2 = "";
		/// <summary>
		/// Gets/Sets String2
		/// </summary>
		public string String2
		{
			get
			{
				return this._string2;
			}
			set
			{
				if ( this._string2 == value )
				{
					//Ignore set
				} else
				{
					this._string2 = value.Trim();
					HasChanged = true;
				}
			}
		}

		private string _string3 = "";
		/// <summary>
		/// Gets/Sets String3
		/// </summary>
		public string String3
		{
			get
			{
				return this._string3;
			}
			set
			{
				if ( this._string3 == value )
				{
					//Ignore set
				} else
				{
					this._string3 = value.Trim();
					HasChanged = true;
				}
			}
		}

		private string _string4 = "";
		/// <summary>
		/// Gets/Sets String4
		/// </summary>
		public string String4
		{
			get
			{
				return this._string4;
			}
			set
			{
				if ( this._string4 == value )
				{
					//Ignore set
				} else
				{
					this._string4 = value.Trim();
					HasChanged = true;
				}
			}
		}
		private string _shortString1 = "";
		/// <summary>
		/// Gets/Sets ShortString1
		/// </summary>
		public string ShortString1
		{
			get
			{
				return this._shortString1;
			}
			set
			{
				if ( this._shortString1 == value )
				{
					//Ignore set
				} else
				{
					this._shortString1 = value.Trim();
					HasChanged = true;
				}
			}
		}

		private string _shortString2 = "";
		/// <summary>
		/// Gets/Sets ShortString2
		/// </summary>
		public string ShortString2
		{
			get
			{
				return this._shortString2;
			}
			set
			{
				if ( this._shortString2 == value )
				{
					//Ignore set
				} else
				{
					this._shortString2 = value.Trim();
					HasChanged = true;
				}
			}
		}

		//private string _shortString3 = "";
		///// <summary>
		///// Gets/Sets ShortString3
		///// </summary>
		//public string ShortString3
		//{
		//  get
		//  {
		//    return this._shortString3;
		//  }
		//  set
		//  {
		//    if ( this._shortString3 == value )
		//    {
		//      //Ignore set
		//    } else
		//    {
		//      this._shortString3 = value.Trim();
		//      HasChanged = true;
		//    }
		//  }
		//}

		//private string _shortString4 = "";
		///// <summary>
		///// Gets/Sets ShortString4
		///// </summary>
		//public string ShortString4
		//{
		//  get
		//  {
		//    return this._shortString4;
		//  }
		//  set
		//  {
		//    if ( this._shortString4 == value )
		//    {
		//      //Ignore set
		//    } else
		//    {
		//      this._shortString4 = value.Trim();
		//      HasChanged = true;
		//    }
		//  }
		//}
		//private string _shortString5 = "";
		///// <summary>
		///// Gets/Sets ShortString5
		///// </summary>
		//public string ShortString5
		//{
		//  get
		//  {
		//    return this._shortString5;
		//  }
		//  set
		//  {
		//    if ( this._shortString5 == value )
		//    {
		//      //Ignore set
		//    } else
		//    {
		//      this._shortString5 = value.Trim();
		//      HasChanged = true;
		//    }
		//  }
		//}

		//private string _shortString6 = "";
		///// <summary>
		///// Gets/Sets ShortString6
		///// </summary>
		//public string ShortString6
		//{
		//  get
		//  {
		//    return this._shortString6;
		//  }
		//  set
		//  {
		//    if ( this._shortString6 == value )
		//    {
		//      //Ignore set
		//    } else
		//    {
		//      this._shortString6 = value.Trim();
		//      HasChanged = true;
		//    }
		//  }
		//}
		//private string _shortString7 = "";
		///// <summary>
		///// Gets/Sets ShortString7
		///// </summary>
		//public string ShortString7
		//{
		//  get
		//  {
		//    return this._shortString7;
		//  }
		//  set
		//  {
		//    if ( this._shortString7 == value )
		//    {
		//      //Ignore set
		//    } else
		//    {
		//      this._shortString7 = value.Trim();
		//      HasChanged = true;
		//    }
		//  }
		//}

		//private string _shortString8 = "";
		///// <summary>
		///// Gets/Sets ShortString8
		///// </summary>
		//public string ShortString8
		//{
		//  get
		//  {
		//    return this._shortString8;
		//  }
		//  set
		//  {
		//    if ( this._shortString8 == value )
		//    {
		//      //Ignore set
		//    } else
		//    {
		//      this._shortString8 = value.Trim();
		//      HasChanged = true;
		//    }
		//  }
		//}
    private string _bigString1 = "";
    /// <summary>
    /// Get/Set for BigString1
    /// </summary>
    public string BigString1
    {
      get { return this._bigString1; }
      set
      {
        if (this._bigString1 == value)
        {
          //Ignore set
        }
        else
        {
          this._bigString1 = value.Trim();
          HasChanged = true;
        }
      }
    }

    private string _bigString2 = "";
    public string BigString2
    {
      get { return this._bigString2; }
      set
      {
        if (this._bigString2 == value)
        {
          //Ignore set
        }
        else
        {
          this._bigString2 = value;
          HasChanged = true;
        }
      }
    }

		//private int _int1;
		///// <summary>
		///// Gets/Sets Int1
		///// </summary>
		//public int Int1
		//{
		//  get
		//  {
		//    return this._int1;
		//  }
		//  set
		//  {
		//    if ( this._int1 == value )
		//    {
		//      //Ignore set
		//    } else
		//    {
		//      this._int1 = value;
		//      HasChanged = true;
		//    }
		//  }
		//}

		private Guid _documentRowId;
		/// <summary>
		/// Gets/Sets the RowId of a related Document
		/// </summary>
		public Guid DocumentRowId
		{
			get
			{
				return this._documentRowId;
			}
			set
			{
				if ( this._documentRowId == value )
				{
					//Ignore set
				} else
				{
					this._documentRowId = value;
					HasChanged = true;
				}
			}
		}//
    #endregion

		/// <summary>
		/// Return a general summary of the AppItem
		/// </summary>
		/// <param name="hostName">Provide results of a call to HTTP_HOST to allow getting the current domain (to show images)</param>
		/// <returns></returns>
		public string Summary( string hostName)
		{
			string summary = Title;
			if ( Description.Length > 0 )
				summary = FormatHtmlList( summary, Description );

			if ( Category.Length > 0 )
				summary = FormatHtmlList( summary, "Category: " + Category );
			if ( Subcategory.Length > 0 )
				summary = FormatHtmlList( summary, "Subcategory: " + Subcategory );
			if ( Status.Length > 0 )
				summary = FormatHtmlList( summary, "Status: " + Status );
			if ( ImageId > 0 )
			{
				//TBD
				string picUrl = "http://" + hostName + "/vos_portal/showPicture.aspx?imgSrc=is&id=" + ImageId.ToString();

				if (AppItemImage != null && AppItemImage.Title.Length > 0)
					summary = FormatHtmlList( summary, "Image: " + "<img src='" + picUrl + "' alt='" + AppItemImage.Title + "'/>" );
				else
					summary = FormatHtmlList( summary, "Image: " + "<img src='" + picUrl + "' />" );
			}

			//if ( DocumentRowId.ToString() != DEFAULT_GUID )
			//{
			//  //TBD
			//}
			if ( ShortString1.Length > 0 )
				summary = FormatHtmlList( summary, "ShortString1: " + ShortString1 );
			if ( ShortString2.Length > 0 )
				summary = FormatHtmlList( summary, "ShortString2: " + ShortString2 );
			//if ( ShortString3.Length > 0 )
			//  summary = FormatHtmlList( summary, "ShortString3: " + ShortString3 );
			//if ( ShortString4.Length > 0 )
			//  summary = FormatHtmlList( summary, "ShortString4: " + ShortString4 );
			//if ( ShortString5.Length > 0 )
			//  summary = FormatHtmlList( summary, "ShortString5: " + ShortString5 );
			//if ( ShortString6.Length > 0 )
			//  summary = FormatHtmlList( summary, "ShortString6: " + ShortString6 );
			//if ( ShortString7.Length > 0 )
			//  summary = FormatHtmlList( summary, "ShortString7: " + ShortString7 );
			//if ( ShortString8.Length > 0 )
			//  summary = FormatHtmlList( summary, "ShortString8: " + ShortString8 );

			if ( String1.Length > 0 )
				summary = FormatHtmlList( summary, "String1: " + String1 );
			if ( String2.Length > 0 )
				summary = FormatHtmlList( summary, "String2: " + String2 );
			if ( String3.Length > 0 )
				summary = FormatHtmlList( summary, "String3: " + String3 );
			if ( String4.Length > 0 )
				summary = FormatHtmlList( summary, "String4: " + String4 );

			//if ( Int1 > 0 )
			//  summary = FormatHtmlList( summary, "Int1: " + Int1 );

			if ( BigString1.Length > 0 )
				summary = FormatHtmlList( summary, "BigString1: " + BigString1 );
			if ( BigString2.Length > 0 )
				summary = FormatHtmlList( summary, "BigString2: " + BigString2 );

			if (Created != null && Created > DefaultDate)
				summary = FormatHtmlList( summary, "Created: " + As_DOW_MMM_DD_YYYY(Created) );
			if ( LastUpdated != null && LastUpdated > DefaultDate )
				summary = FormatHtmlList( summary, "LastUpdated: " + As_DOW_MMM_DD_YYYY( LastUpdated ) );

			return summary;
		} //
		#region External entities
		ImageStore _image = null;
		/// <summary>
		/// Get/Set AppItemImage
		/// </summary>
		public ImageStore AppItemImage
		{
			get
			{
				return this._image;
			}
			set
			{
				this._image = value;
			}
		}

		DataItem _item = new DataItem();
		/// <summary>
		/// Get/Set FaqSubcategory - only used with a FAQ item
		/// </summary>
		public DataItem FaqSubcategory
		{
			get
			{
				return this._item;
			}
			set
			{
				this._item = value;
			}
		}

		DocumentVersion _doc = null;
		/// <summary>
		/// Get/Set RelatedDocument
		/// </summary>
		public DocumentVersion RelatedDocument
		{
			get
			{
				return this._doc;
			}
			set
			{
				this._doc = value;
			}
		}
		#endregion

		#region composite properties
		private string _appItemTypeTitle = "";
		/// <summary>
		/// Gets/Sets AppItemTypeTitle - from related AppItemType
		/// </summary>
		public string AppItemTypeTitle
		{
			get
			{
				return this._appItemTypeTitle;
			}
			set
			{
				if ( this._appItemTypeTitle == value )
				{
					//Ignore set
				} else
				{
					this._appItemTypeTitle = value.Trim();
					HasChanged = true;
				}
			}
		}

		private string _approvedBy = "";
		/// <summary>
		/// Gets/Sets ApprovedBy
		/// </summary>
		public string ApprovedBy
		{
			get
			{
				return this._approvedBy;
			}
			set
			{
				if ( this._approvedBy == value )
				{
					//Ignore set
				} else
				{
					this._approvedBy = value.Trim();
					HasChanged = true;
				}
			}
		}

		//??any current parent AppItemRelationship entity
		private AppItemRelationship _relatedParentItem;
		/// <summary>
		/// Get/Set RelatedParentItem
		/// </summary>
		public AppItemRelationship RelatedParentItem
		{
			get
			{
				return this._relatedParentItem;
			}
			set
			{
				this._relatedParentItem = value;
			}
		}

		AppItemStoryProperties _storyProperties = null;
		/// <summary>
		/// Get/Set StoryProperties
		/// </summary>
		public AppItemStoryProperties StoryProperties
		{
			get
			{
				return this._storyProperties;
			}
			set
			{
				this._storyProperties = value;
			}
		}

		//other - collection of related children items?
		#endregion

	} // end class 
} // end Namespace 

