/*********************************************************************************
* Author: Michael Parsons
*  
* Date: Jun, 2007
* Assembly: ILPathways.Business
* Description: 
* Notes:
* 
* 
* Copyright 2007, Illinois workNet All rights reserved.
*********************************************************************************/
using System;

namespace ILPathways.Business
{
	/// <summary>
	/// Represents an object that describes a Contact.
	/// </summary>
	[Serializable]
	public class Contact : BaseBusinessDataEntity
	{
		/// <summary>
		/// Initializes a new instance of the ILPathways.Business.Contact class.
		/// </summary>
		public Contact()
		{
		}


		#region Properties created from dictionary for Contact

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
				if ( this._orgId == value )
				{
					//Ignore set
				} else
				{
					this._orgId = value;
					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Get/Set Parent OrgId
		/// </summary>
		public int ParentOrgId
		{
			get { return _parentOrgId; }
			set
			{
				if ( this._parentOrgId == value )
				{
					//Ignore set
				} else
				{
					this._parentOrgId = value;
					HasChanged = true;
				}
			}
		}
		private int _parentOrgId = 0;
		private string _companyName = "";
		/// <summary>
		/// Company name
		/// </summary>
		public string CompanyName
		{
			get
			{
				return _companyName;
			}

			set
			{
				if ( this._companyName == value )
				{
					//Ignore set
				} else
				{
					this._companyName = value.Trim();
					HasChanged = true;
				}
			}
		}

		private string _deptName = "";
		/// <summary>
		/// Department name
		/// </summary>
		public string DepartmentName
		{
			get
			{
				return _deptName;
			}

			set
			{
				if ( this._deptName == value )
				{
					//Ignore set
				} else
				{
					this._deptName = value.Trim();
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
				if ( this._firstName == value )
				{
					//Ignore set
				} else
				{
					this._firstName = value.Trim();
					HasChanged = true;
				}
			}
		}

		private string _middleName = "";
		/// <summary>
		/// Gets/Sets MiddleName
		/// </summary>
		public string MiddleName
		{
			get
			{
				return this._middleName;
			}
			set
			{
				if ( this._middleName == value )
				{
					//Ignore set
				} else
				{
					this._middleName = value.Trim();
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
				if ( this._lastName == value )
				{
					//Ignore set
				} else
				{
					this._lastName = value.Trim();
					HasChanged = true;
				}
			}
		}

		private string _salutation = "";
		/// <summary>
		/// Gets/Sets Salutation
		/// </summary>
		public string Salutation
		{
			get
			{
				return this._salutation;
			}
			set
			{
				if ( this._salutation == value )
				{
					//Ignore set
				} else
				{
					this._salutation = value.Trim();
					HasChanged = true;
				}
			}
		}

		private string _suffix = "";
		/// <summary>
		/// Gets/Sets Suffix
		/// </summary>
		public string Suffix
		{
			get
			{
				return this._suffix;
			}
			set
			{
				if ( this._suffix == value )
				{
					//Ignore set
				} else
				{
					this._suffix = value.Trim();
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

        private string _SSN = "";
        /// <summary>
        /// 
        /// </summary>
        public string SSN
        {
            get
            {
                return _SSN;
            }

            set
            {
                if ( this._SSN == value )
                {
                    //Ignore set
                }
                else
                {
                    string work;
                    if ( value != null )
                    {
                        work = value.Trim();
                        work = work.Replace( "-", "" );
                        this._SSN = work;
                    }
                    else
                    {
                        this._SSN = "";
                    }
                    HasChanged = true;
                }
            }
        }

		private string _mainPhone = "";
		/// <summary>
		/// Gets/Sets MainPhone
		/// </summary>
		public string PhoneNumber
		{
			get
			{
				return this._mainPhone;
			}
			set
			{
				if ( this._mainPhone == value )
				{
					//Ignore set
				} else
				{
					_mainPhone = this.StripPhone( value.Trim() );
					HasChanged = true;
				}
			}
		}

		private string _mainPhoneExt = "";
		/// <summary>
		/// Gets/Sets MainPhoneExt
		/// </summary>
		public string MainPhoneExt
		{
			get
			{
				return this._mainPhoneExt;
			}
			set
			{
				if ( this._mainPhoneExt == value )
				{
					//Ignore set
				} else
				{
					this._mainPhoneExt = value.Trim();
					HasChanged = true;
				}
			}
		}

		private string _altPhone = "";
		/// <summary>
		/// Gets/Sets AltPhone
		/// </summary>
		public string AltPhone
		{
			get
			{
				return this._altPhone;
			}
			set
			{
				if ( this._altPhone == value )
				{
					//Ignore set
				} else
				{
          if (value != null)
          {
            _altPhone = this.StripPhone(value.Trim());
          }
          else
          {
            _altPhone = "";
          }
					HasChanged = true;
				}
			}
		}

		private string _altPhoneExt = "";
		/// <summary>
		/// Gets/Sets AltPhoneExt
		/// </summary>
		public string AltPhoneExt
		{
			get
			{
				return this._altPhoneExt;
			}
			set
			{
				if ( this._altPhoneExt == value )
				{
					//Ignore set
				} else
				{
					this._altPhoneExt = value.Trim();
					HasChanged = true;
				}
			}
		}

		private string _fax = "";
		/// <summary>
		/// Gets/Sets Fax
		/// </summary>
		public string FaxNumber
		{
			get
			{
				return this._fax;
			}
			set
			{
				if ( this._fax == value )
				{
					//Ignore set
				} else
				{
          if (value != null)
          {
            _fax = this.StripPhone(value.Trim());
          }
          else
          {
            _fax = "";
          }

					HasChanged = true;
				}
			}
		}

		private string _pager = "";
		/// <summary>
		/// Gets/Sets Pager
		/// </summary>
		public string Pager
		{
			get
			{
				return this._pager;
			}
			set
			{
				if ( this._pager == value )
				{
					//Ignore set
				} else
				{
					if ( value != null )
					{
						_pager = this.StripPhone( value.Trim() );
					} else
					{
						_pager = "";
					}

					HasChanged = true;
				}
			}
		}

		private string _mobilePhone = "";
		/// <summary>
		/// Gets/Sets MobilePhone
		/// </summary>
		public string MobilePhone
		{
			get
			{
				return this._mobilePhone;
			}
			set
			{
				if ( this._mobilePhone == value )
				{
					//Ignore set
				} else
				{
					if ( value != null )
					{
						_mobilePhone = this.StripPhone( value.Trim() );
					} else
					{
						_mobilePhone = "";
					}
					HasChanged = true;
				}
			}
		}//

        /// <summary>
        /// Prefered language to use
        /// </summary>
        public string PreferedLanguage
        {
            get
            {
                return _PreferedLanguage;
            }

            set
            {
                if ( this._PreferedLanguage == value )
                {
                    //Ignore set
                }
                else
                {
                    this._PreferedLanguage = value.Trim();
                    HasChanged = true;
                }
            }
        }
        private string _PreferedLanguage = "en";
		/// <summary>
		/// Get/Set for Email Type
		/// </summary>
		public string EmailType
		{
			get { return _emailType; }
			set
			{
				if ( this._emailType == value )
				{
					//Ignore set
				} else
				{
					this._emailType = value.Trim();
					HasChanged = true;
				}
			}
		}
		private string _emailType = "HTML";

		private Address _address = new Address();
		/// <summary>
		/// Get/Set for Address
		/// </summary>
		public Address Address
		{
			get
			{
				return this._address;
			}
			set
			{
				if ( this._address == value )
				{
					//Ignore set
				} else
				{
					this._address = value;
					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Gets/Sets Address1
		/// </summary>
		public string Address1
		{
			get
			{
				return this.Address.Address1;
				//return this._address1;
			}
			set
			{
				if ( this.Address.Address1 == value )
				{
					//Ignore set
				} else
				{
					this.Address.Address1 = value.Trim();
					HasChanged = true;
				}
				//if ( this._address1 == value )
				//{
				//  //Ignore set
				//} else
				//{
				//  this._address1 = value.Trim();
				//  HasChanged = true;
				//}
			}
		}

		/// <summary>
		/// Gets/Sets Address2
		/// </summary>
		public string Address2
		{
			get
			{
				return this.Address.Address2;
				//return this._address2;
			}
			set
			{
				if ( this.Address.Address2 == value )
				{
					//Ignore set
				} else
				{
					this.Address.Address2 = value.Trim();
					HasChanged = true;
				}
				//if ( this._address2 == value )
				//{
				//  //Ignore set
				//} else
				//{
				//  this._address2 = value.Trim();
				//  HasChanged = true;
				//}
			}
		}

		/// <summary>
		/// Gets/Sets City
		/// </summary>
		public string City
		{
			get
			{
				return this.Address.City;
				//return this._city;
			}
			set
			{
				if ( this.Address.City == value )
				{
					//Ignore set
				} else
				{
					this.Address.City = value.Trim();
					HasChanged = true;
				}

				//if ( this._city == value )
				//{
				//  //Ignore set
				//} else
				//{
				//  this._city = value.Trim();
				//  HasChanged = true;
				//}
			}
		}

		/// <summary>
		/// Gets/Sets State
		/// </summary>
		public string State
		{
			get
			{
				return this.Address.State;
				//return this._state;
			}
			set
			{
				if ( this.Address.State == value )
				{
					//Ignore set
				} else
				{
					this.Address.State = value.Trim();
					HasChanged = true;
				}
				//if ( this._state == value )
				//{
				//  //Ignore set
				//} else
				//{
				//  this._state = value.Trim();
				//  HasChanged = true;
				//}
			}
		}

		/// <summary>
		/// Get/Set for State
		/// </summary>
		public string StateAbbreviation
		{
			get
			{
				return this.Address.State;
				//return this._state;
			}
			set
			{
				if ( this.Address.State == value )
				{
					//Ignore set
				} else
				{
					this.Address.State = value.Trim();
					HasChanged = true;
				}
				//if ( this._state == value )
				//{
				//  //Ignore set
				//} else
				//{
				//  this._state = value.Trim();
				//  HasChanged = true;
				//}
			}
		}
		/// <summary>
		/// Gets/Sets ZipCode
		/// </summary>
		public string ZipCode
		{
			get
			{
				return this.Address.ZipCode;
				//return this._zipCode;
			}
			set
			{
				if ( this.Address.ZipCode == value )
				{
					//Ignore set
				} else
				{
					this.Address.ZipCode = value.Trim();
					HasChanged = true;
				}

				//if ( this._zipCode == value )
				//{
				//  //Ignore set
				//} else
				//{
				//  //TODO - may want to have a check for 5 digits. If 9 or 10, then set ZipCode4??
				//  string zc = value.Trim();
				//  zc = zc.Replace( "-", "" );
				//  if ( zc.Length <= 5 )
				//  {
				//    this._zipCode = zc;

				//  } else if ( zc.Length == 9 )
				//  {
				//    this._zipCode = zc.Substring( 0, 5 );
				//    ZipCode4 = zc.Substring( 5, 4 );

				//  } else if ( zc.Length == 10 )
				//  {
				//    //MP - we will force ZIP Code plus 4 entry to include the dash, so 10 would be valid
				//    this._zipCode = zc.Substring( 0, 5 );
				//    ZipCode4 = zc.Substring( 6, 4 );
				//  } else
				//  {
				//    //apparantly invalid, so ??
				//  }
				//  HasChanged = true;
				//}
			}
		}

		/// <summary>
		/// Gets/Sets ZipCode plus4 digits
		/// </summary>
		public string ZipCode4
		{
			get
			{
				return this.Address.ZipCodePlus4;
				//return this._zipCode4;
			}
			set
			{
				if ( this.Address.ZipCodePlus4 == value )
				{
					//Ignore set
				} else
				{
					this.Address.ZipCodePlus4 = value.Trim();
					HasChanged = true;
				}
				//if ( this._zipCode4 == value )
				//{
				//  //Ignore set
				//} else
				//{
				//  this._zipCode4 = value.Trim();
				//  HasChanged = true;
				//}
			}
		}

		/// <summary>
		/// Retrive a full ZIP Code - will include the plus4 if present
		/// </summary>
		public string ZipCodeFull
		{
			get
			{
				return this.Address.ZipCodeFull;
			}
		}

		/// <summary>
		/// Gets/Sets Country
		/// </summary>
		public string Country
		{
			get
			{
				return this.Address.Country;
				//return this._country;
			}
			set
			{
				if ( this.Address.Country == value )
				{
					//Ignore set
				} else
				{
					this.Address.Country = value.Trim();
					HasChanged = true;
				}
			}
		}

		private string _notes = "";
		/// <summary>
		/// Gets/Sets Notes
		/// </summary>
		public string Notes
		{
			get
			{
				return this._notes;
			}
			set
			{
				if ( this._notes == value )
				{
					//Ignore set
				} else
				{
					this._notes = value.Trim();
					HasChanged = true;
				}
			}
		}

		private string _DateOfBirth = "1900-01-01";
		/// <summary>
		///  Date of birth
		/// </summary>
		public string DateOfBirth
		{
			get
			{
				return _DateOfBirth;
			}

			set
			{
				if ( this._DateOfBirth == value )
				{
					//Ignore set
				} else
				{
					this._DateOfBirth = value.Trim();
					HasChanged = true;
				}
			}
		}

		private bool _canContactByEmail = true;
		/// <summary>
		/// Gets/Sets CanContactByEmail
		/// </summary>
		public bool CanContactByEmail
		{
			get	{	return _canContactByEmail; }
			set
			{
				if ( this._canContactByEmail == value )
				{
					//Ignore set
				} else
				{
					this._canContactByEmail = value;
					HasChanged = true;
				}
			}
		}
        /// <summary>
        /// Gets/Sets UsesAssistiveTechnology
        /// </summary>
        public bool UsesAssistiveTechnology
        {
            get
            {
                return this._useAssistiveTech;
            }
            set
            {
                if ( this._useAssistiveTech == value )
                {
                    //Ignore set
                }
                else
                {
                    this._useAssistiveTech = value;
                    HasChanged = true;
                }
            }
        }
        private bool _useAssistiveTech = false;


        /// <summary>
        /// OrgId Get/Set Parent organization name
        /// </summary>
        public string OrganizationName
        {
            get { return _orgName; }
            set { _orgName = value; }
        }//
        private string _orgName = "";

        //private Organization _org;
        ///// <summary>
        ///// Gets/Set user's organization 
        ///// </summary>
        //public Organization MyOrganization
        //{
        //    get
        //    {
        //        return this._org;
        //    }
        //    set
        //    {
        //        this._org = value;
        //    }
        //} //
		#endregion

		#region ===== Behaviours ===============================

		/// <summary>
		/// Returns full name of user
		/// </summary>
		/// <returns></returns>
        public string FullName()
        {
            if (string.IsNullOrWhiteSpace(FirstName))
                return "Incomplete - Update Profile";
            else
                return this.FirstName + " " + this.LastName;
        }

        /// <summary>
        /// Returns user name as LastName, FirstName
        /// </summary>
        /// <returns></returns>
        public string SortName()
        {
            if (string.IsNullOrWhiteSpace(FirstName))
                return "Incomplete - Update Profile";
            else
                return this.LastName + ", " + this.FirstName;
        }//

        /// <summary>
        /// Returns Short Name of user - first initial of first name plus last name.
        /// </summary>
        /// <returns></returns>
        public string ShortName()
        {
            if (string.IsNullOrWhiteSpace(FirstName))
                return "Incomplete - Update Profile";
            else
            {
                string shortName = this.FirstName.Substring(0, 1) + this.LastName;
                shortName = shortName.Replace(" ", "");
                return shortName;
            }
        }//


		/// <summary>
		/// Format the address lines as one line
		/// </summary>
		/// <returns></returns>
		public string AddressLine()
		{
			return this.Address.AddressLine();
		}

		/// <summary>
		/// Format the full address line - non-html, often for exports
		/// </summary>
		/// <returns></returns>
		public string FullAddress()
		{
			return this.Address.FullAddress();
		} //


		/// <summary>
		/// Format the full address as Html
		/// </summary>
		/// <returns></returns>
		public string FormatHtmlAddress()
		{
			return Address.HtmlFormat();
		} //

		/// <summary>
		/// Format address as a single line
		/// </summary>
		public string AddressToString()
		{
			return Address.AddressString();
		}//

		/// <summary>
		/// Format user summary as Html:
		/// - Name, title, phone, e-mail 
		/// </summary>
		/// <returns></returns>
        //public virtual string ContactSummary()
        //{

        //    string summary = FullName();

        //    if ( Title.Length > 0 )
        //        summary = FormatHtmlList( summary, Title );

        //    if ( PhoneNumber.Length > 0 )
        //        summary = FormatHtmlList( summary, "Phone: " + DisplayPhone( PhoneNumber, MainPhoneExt ) );

        //    if ( Email.Length > 0 )
        //        summary = FormatHtmlList( summary, "E-mail: " + Email );


        //    return summary;
        //} //



		/// <summary>
		/// return user's age if birth date is valid
		/// </summary>
		/// <returns></returns>
		public int MyAge()
		{
			int age = 0;
			DateTime birthDate;

			try 
			{
				if ( DateOfBirth.Length > 0 )
				{
					birthDate = DateTime.Parse( DateOfBirth );

					// get the difference in years
					age = DateTime.Now.Year - birthDate.Year;
					// subtract another year if we're before the
					// birth day in the current year
					if ( DateTime.Now.Month < birthDate.Month ||
							( DateTime.Now.Month == birthDate.Month &&
							DateTime.Now.Day < birthDate.Day ) )
						age--;
				}
			} catch (Exception ex) 
			{
				age = 0;
			}

			return age;
		} //


		/// <summary>
		/// determine if current contact is a youth (between 12 and 18)
		/// </summary>
		/// <returns></returns>
		public bool IsYouth()
		{
			if ( MyAge() > 11 && MyAge() < 19)
				return true;
			else
				return false;
		} //
		#endregion
	}
}
