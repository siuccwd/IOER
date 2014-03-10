/*********************************************************************************
= Author: Michael Parsons
=
= Date: Mar 22/2012
= Assembly: ILPathways.Business
= Description: Store properties related to a success story
= Notes:
=
=
= Copyright 2012, Illinois workNet All rights reserved.
********************************************************************************/
using System; 
using System.Collections.Generic; 

namespace ILPathways.Business
{
  ///<summary>
  ///Represents an object that describes a AppItemStoryProperties
  ///</summary>
  [Serializable]
  public class AppItemStoryProperties : BaseBusinessDataEntity
  {
    ///<summary>
    ///Initializes a new instance of the ILPathways.Business.AppItemStoryProperties class.
    ///</summary>
    public AppItemStoryProperties() {}

		/// <summary>
		/// Story type constants
		/// </summary>
		public static int StoryTypePersonal = 0;
		public static int StoryTypeWia = 1;
		public static int StoryTypeIwtsWorker = 2;
		public static int StoryTypeBusiness = 3;
		public static int StoryTypeProjects = 4;

    #region Properties created from dictionary for AppItemStoryProperties

    private Guid _appItemRowId;
    /// <summary>
    /// Gets/Sets AppItemRowId
    /// </summary>
    public Guid AppItemRowId
    {
      get
      {
        return this._appItemRowId;
      }
      set
      {
        if (this._appItemRowId == value) {
          //Ignore set
        } else {
          this._appItemRowId = value;
          HasChanged = true;
        }
      }
    }

    private int _storyType;
    /// <summary>
    /// Gets/Sets StoryType
		/// 0-personal
		/// 1-wia
		/// 2-iwts
		/// 3-business
		/// 4-projects
    /// </summary>
    public int StoryType
    {
      get
      {
        return this._storyType;
      }
      set
      {
        if (this._storyType == value) {
          //Ignore set
        } else {
          this._storyType = value;
          HasChanged = true;
        }
      }
    }

    private string _iwdsServiceType = "";
    /// <summary>
    /// Gets/Sets IwdsServiceType Code
		/// Relates to Group.MemberType:
		/// 1-Adult
		/// 2-Dislocated worker
		/// 3-Youth
		/// 4-TAA
    /// </summary>
    public string IwdsServiceType
    {
      get
      {
        return this._iwdsServiceType;
      }
      set
      {
        if (this._iwdsServiceType == value) {
          //Ignore set
        } else {
          this._iwdsServiceType = value.Trim();
          HasChanged = true;
        }
      }
    }
		/// <summary>
		/// Gets IwdsServiceType Title
		/// </summary>
		public string IwdsServiceTypeTitle
		{
			get
			{
				string title = "";
				switch ( IwdsServiceType )
				{
					case "1":
						title = "Adult";
						break;
					case "2":
						title = "Dislocated worker";
						break;
					case "3":
						title = "Youth";
						break;
					case "4":
						title = "TAA";
						break;
					default:
						break;
				}
				return title;
			}
		}

    private int _targetUserId;
    /// <summary>
    /// Gets/Sets TargetUserId
    /// </summary>
    public int TargetUserId
    {
      get
      {
        return this._targetUserId;
      }
      set
      {
        if (this._targetUserId == value) {
          //Ignore set
        } else {
          this._targetUserId = value;
          HasChanged = true;
        }
      }
    }

    private string _firstName = "";
    /// <summary>
    /// Gets/Sets FirstName
    /// </summary>
    public string FirstName
    {
      get
      {
        return this._firstName;
      }
      set
      {
        if (this._firstName == value) {
          //Ignore set
        } else {
          this._firstName = value.Trim();
          HasChanged = true;
        }
      }
    }

    private string _lastName = "";
    /// <summary>
    /// Gets/Sets LastName
    /// </summary>
    public string LastName
    {
      get
      {
        return this._lastName;
      }
      set
      {
        if (this._lastName == value) {
          //Ignore set
        } else {
          this._lastName = value.Trim();
          HasChanged = true;
        }
      }
    }

    private string _targetTitle = "";
    /// <summary>
    /// Gets/Sets TargetTitle
    /// </summary>
    public string TargetTitle
    {
      get
      {
        return this._targetTitle;
      }
      set
      {
        if (this._targetTitle == value) {
          //Ignore set
        } else {
          this._targetTitle = value.Trim();
          HasChanged = true;
        }
      }
    }

    private string _address1 = "";
    /// <summary>
    /// Gets/Sets Address1
    /// </summary>
    public string Address1
    {
      get
      {
        return this._address1;
      }
      set
      {
        if (this._address1 == value) {
          //Ignore set
        } else {
          this._address1 = value.Trim();
          HasChanged = true;
        }
      }
    }

    private string _address2 = "";
    /// <summary>
    /// Gets/Sets Address2
    /// </summary>
    public string Address2
    {
      get
      {
        return this._address2;
      }
      set
      {
        if (this._address2 == value) {
          //Ignore set
        } else {
          this._address2 = value.Trim();
          HasChanged = true;
        }
      }
    }

    private string _city = "";
    /// <summary>
    /// Gets/Sets City
    /// </summary>
    public string City
    {
      get
      {
        return this._city;
      }
      set
      {
        if (this._city == value) {
          //Ignore set
        } else {
          this._city = value.Trim();
          HasChanged = true;
        }
      }
    }

    private string _zipcode = "";
    /// <summary>
    /// Gets/Sets Zipcode
    /// </summary>
    public string Zipcode
    {
      get
      {
        return this._zipcode;
      }
      set
      {
        if (this._zipcode == value) {
          //Ignore set
        } else {
          this._zipcode = value.Trim();
          HasChanged = true;
        }
      }
    }
		public string ZipCode
		{
			get
			{
				return this._zipcode;
			}
			set
			{
				if ( this._zipcode == value )
				{
					//Ignore set
				} else
				{
					this._zipcode = value.Trim();
					HasChanged = true;
				}
			}
		}
		private string _stateCode = "IL";
		/// <summary>
		/// Gets/Sets StateCode
		/// </summary>
		public string StateCode
		{
			get
			{
				return this._stateCode;
			}
			set
			{
				if ( this._stateCode == value )
				{
					//Ignore set
				} else
				{
					this._stateCode = value.Trim();
					HasChanged = true;
				}
			}
		}//

		private int _lwia;
		/// <summary>
		/// Gets/Sets Lwia
		/// </summary>
		public int Lwia
		{
			get
			{
				return this._lwia;
			}
			set
			{
				if ( this._lwia == value )
				{
					//Ignore set
				} else
				{
					this._lwia = value;
					HasChanged = true;
				}
			}
		}
    private string _congDistrict = "";
    /// <summary>
    /// Gets/Sets CongDistrict
    /// </summary>
    public string CongDistrict
    {
      get
      {
        return this._congDistrict;
      }
      set
      {
        if (this._congDistrict == value) {
          //Ignore set
        } else {
          this._congDistrict = value.Trim();
          HasChanged = true;
        }
      }
    }

    private string _serviceLocation = "";
    /// <summary>
    /// Gets/Sets ServiceLocation
    /// </summary>
    public string ServiceLocation
    {
      get
      {
        return this._serviceLocation;
      }
      set
      {
        if (this._serviceLocation == value) {
          //Ignore set
        } else {
          this._serviceLocation = value.Trim();
          HasChanged = true;
        }
      }
    }

    private string _cipCode = "";
    /// <summary>
    /// Gets/Sets CipCode
    /// </summary>
    public string CipCode
    {
      get
      {
        return this._cipCode;
      }
      set
      {
        if (this._cipCode == value) {
          //Ignore set
        } else {
          this._cipCode = value.Trim();
          HasChanged = true;
        }
      }
    }

		private int _serviceId;
		/// <summary>
		/// Gets/Sets ServiceId
		/// </summary>
		public int ServiceId
		{
			get
			{
				return this._serviceId;
			}
			set
			{
				if ( this._serviceId == value )
				{
					//Ignore set
				} else
				{
					this._serviceId = value;
					HasChanged = true;
				}
			}
		}
    private string _serviceDesc = "";
    /// <summary>
    /// Gets/Sets ServiceDesc
    /// </summary>
    public string ServiceDesc
    {
      get
      {
        return this._serviceDesc;
      }
      set
      {
        if (this._serviceDesc == value) {
          //Ignore set
        } else {
          this._serviceDesc = value.Trim();
          HasChanged = true;
        }
      }
    }

    private int _industryId;
    /// <summary>
    /// Gets/Sets IndustryId
    /// </summary>
    public int IndustryId
    {
      get
      {
        return this._industryId;
      }
      set
      {
        if (this._industryId == value) {
          //Ignore set
        } else {
          this._industryId = value;
          HasChanged = true;
        }
      }
    }

    private bool _youthYearRound;
    /// <summary>
    /// Gets/Sets YouthYearRound
    /// </summary>
    public bool YouthYearRound
    {
      get
      {
        return this._youthYearRound;
      }
      set
      {
        if (this._youthYearRound == value) {
          //Ignore set
        } else {
          this._youthYearRound = value;
          HasChanged = true;
        }
      }
    }

    private bool _youthEducation;
    /// <summary>
    /// Gets/Sets YouthEducation
    /// </summary>
    public bool YouthEducation
    {
      get
      {
        return this._youthEducation;
      }
      set
      {
        if (this._youthEducation == value) {
          //Ignore set
        } else {
          this._youthEducation = value;
          HasChanged = true;
        }
      }
    }

    private bool _youthSummer;
    /// <summary>
    /// Gets/Sets YouthSummer
    /// </summary>
    public bool YouthSummer
    {
      get
      {
        return this._youthSummer;
      }
      set
      {
        if (this._youthSummer == value) {
          //Ignore set
        } else {
          this._youthSummer = value;
          HasChanged = true;
        }
      }
    }


    #endregion

		#region external properties
		AppUser _contact = null;
		/// <summary>
		/// Get/Set Related Contact
		/// </summary>
		public AppUser RelatedContact
		{
			get
			{
				return this._contact;
			}
			set
			{
				this._contact = value;
			}
		}

		private string _careerCluster = "";
		/// <summary>
		/// Gets/Sets CareerCluster
		/// </summary>
		public string CareerCluster
		{
			get
			{
				return this._careerCluster;
			}
			set
			{
				this._careerCluster = value.Trim();
			}
		}
		#endregion
	} // end class 
} // end Namespace 

