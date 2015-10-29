using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    [Serializable]
    public abstract class AbstractOrganization : BaseBusinessDataEntity
    {
        	///<summary>
        ///Initializes a new instance of the ILPathways.Business.Organization class.
		///</summary>
        public AbstractOrganization() { }

        #region Properties created from dictionary for BaseOrganization

        private string _name = "";
		/// <summary>
		/// Gets/Sets Name
		/// </summary>
		public string Name
		{
			get
			{
				return this._name;
			}
			set
			{
				if ( this._name == value )
				{
					//Ignore set
				} else
				{
					this._name = value != null ? value != null ? value.Trim() : "" : "";
					HasChanged = true;
				}
			}
		}
		public string Description { get; set; }

		private int _parentId;
		/// <summary>
		/// Gets/Sets ParentId
		/// </summary>
		public int ParentId
		{
			get
			{
				return this._parentId;
			}
			set
			{
				if ( this._parentId == value )
				{
					//Ignore set
				} else
				{
					this._parentId = value;
					HasChanged = true;
				}
			}
		}

		private int _primaryContactId;
		/// <summary>
		/// Gets/Sets PrimaryContactId
		/// </summary>
		public int PrimaryContactId
		{
			get
			{
				return this._primaryContactId;
			}
			set
			{
				if ( this._primaryContactId == value )
				{
					//Ignore set
				} else
				{
					this._primaryContactId = value;
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
				if ( this._mainPhone == value )
				{
					//Ignore set
				} else
				{
					this._mainPhone = this.StripPhone( value != null ? value.Trim() : "" );
					HasChanged = true;
				}
			}
		}
        public string MainPhoneFormatted
        {
            get
            {
                if ( _mainPhone == null || _mainPhone.Length == 0 )
                    return "";
                if ( _mainPhone.Length == 10 && _mainPhone.IndexOf( "-" ) == -1 )
                {
                    return _mainPhone.Substring( 0, 3 ) 
                            + "-" + _mainPhone.Substring( 3, 3 ) 
                            + "-" + _mainPhone.Substring( 6, 4 );

                } else
                    return this._mainPhone;
            }

        }

		private string _mainExtension = "";
		/// <summary>
		/// Gets/Sets MainExtension
		/// </summary>
		public string MainExtension
		{
			get
			{
				return this._mainExtension;
			}
			set
			{
				if ( this._mainExtension == value )
				{
					//Ignore set
                }
                else if (value == null)
                {
                    this._mainExtension = null;
                    HasChanged = true;
                }
                else
				{
					this._mainExtension = value != null ? value.Trim() : "";
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
        }
        else if (value == null)
        {
            this._altPhone = null;
            HasChanged = true;
        } 
        else
				{
					this._altPhone = this.StripPhone( value != null ? value.Trim() : "" );
					HasChanged = true;
				}
			}
		}

		private string _altExtension = "";
		/// <summary>
		/// Gets/Sets AltExtension
		/// </summary>
		public string AltExtension
		{
			get
			{
				return this._altExtension;
			}
			set
			{
                if (this._altExtension == value)
                {
                    //Ignore set
                }
                else if (value == null)
                {
                    this._altExtension = null;
                    this.HasChanged = true;
                }
                else 
				{
					this._altExtension = value != null ? value.Trim() : "";
					HasChanged = true;
				}
			}
		}

		private string _fax = "";
		/// <summary>
		/// Gets/Sets Fax
		/// </summary>
		public string Fax
		{
			get
			{
				return this._fax;
			}
			set
			{
        if (this._fax == value)
        {
            //Ignore set
        }
        else if (value == null)
        {
            this._fax = null;
            HasChanged = true;
        }
        else
				{
					this._fax = this.StripPhone( value != null ? value.Trim() : "" );
					HasChanged = true;
				}
			}
		}

		private string _tty = "";
		/// <summary>
		/// Gets/Sets TTY
		/// </summary>
		public string TTY
		{
			get
			{
				return this._tty;
			}
			set
			{
        if (this._tty == value)
        {
            //Ignore set
        }
        else if (value == null)
        {
            this._tty = null;
            HasChanged = true;
        }
        else
				{
          if (value.Trim() == "711")
          {
            this._tty = "711";
          }
          else
          {
            this._tty = this.StripPhone(value.Trim());
          }
					HasChanged = true;
				}
			}
		}

		private string _webSite = "";
		/// <summary>
		/// Gets/Sets WebSite
		/// </summary>
		public string WebSite
		{
			get
			{
				return this._webSite;
			}
			set
			{
				if ( this._webSite == value )
				{
					//Ignore set
                }
                else if (value == null)
                {
                    this._webSite = null;
                    HasChanged = true;
                } 
                else
				{
					this._webSite = value.Trim();
					HasChanged = true;
				}
			}
		}

		private string _email = "";
		/// <summary>
		/// Gets/Sets Email
		/// </summary>
		public string Email
		{
			get
			{
				return this._email;
			}
			set
			{
				if ( this._email == value )
				{
					//Ignore set
                }
                else if (value == null)
                {
                    this._email = null;
                    HasChanged = true;
                }
                else
				{
					this._email = value.Trim();
					HasChanged = true;
				}
			}
		}

        private string _emailDomain = "";
        /// <summary>
        /// Gets/Sets EmailDomain
        /// </summary>
        public string EmailDomain
        {
            get
            {
                return this._emailDomain;
            }
            set
            {
                if ( this._emailDomain == value )
                {
                    //Ignore set
                }
                else if ( value == null )
                {
                    this._emailDomain = null;
                    HasChanged = true;
                }
                else
                {
                    this._emailDomain = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private string _logoUrl = "";
		/// <summary>
        /// Gets/Sets LogoUrl
		/// </summary>
        public string LogoUrl
		{
			get
			{
                return this._logoUrl;
			}
			set
			{
                if ( this._logoUrl == value )
				{
					//Ignore set
                }
                else if (value == null)
                {
                    this._logoUrl = null;
                    HasChanged = true;
                }
                else
				{
                    this._logoUrl = value.Trim();
					HasChanged = true;
				}
			}
		}
		public string ImageUrl
		{
			get
			{
				return this._logoUrl;
			}
			set
			{
				if ( this._logoUrl == value )
				{
					//Ignore set
				}
				else if ( value == null )
				{
					this._logoUrl = null;
					HasChanged = true;
				}
				else
				{
					this._logoUrl = value.Trim();
					HasChanged = true;
				}
			}
		}
		private Address _orgAddress = new Address();
		/// <summary>
		/// Get/Set for OrgAddress
		/// </summary>
		public Address OrgAddress
		{
			get
			{
				return this._orgAddress;
			}
			set
			{
				if ( this._orgAddress == value )
				{
					//Ignore set
				} else
				{
					this._orgAddress = value;
					HasChanged = true;
				}
			}
		}

		/// <summary>
        /// Gets/Sets Address1 from OrgAddress object
		/// </summary>
		public string Address1
		{
			get
			{
				return this.OrgAddress.Address1;
				//return this._address1;
			}
			set
			{
				if ( this.OrgAddress.Address1 == value )
				{
					//Ignore set
				} else
				{
					this.OrgAddress.Address1 = value != null ? value.Trim() : "";
					HasChanged = true;
				}
			}
		}
		/// <summary>
        /// Gets/Sets Address2 from OrgAddress object
		/// </summary>
		public string Address2
		{
			get
			{
				return this.OrgAddress.Address2;
			}
			set
			{
				if ( this.OrgAddress.Address2 == value )
				{
					//Ignore set
				} else
				{
					this.OrgAddress.Address2 = value != null ? value.Trim() : "";
					HasChanged = true;
				}
				
			}
		}

		/// <summary>
        /// Gets/Sets City from OrgAddress object
		/// </summary>
		public string City
		{
			get
			{
				return this.OrgAddress.City;
			}
			set
			{
				if ( this.OrgAddress.City == value )
				{
					//Ignore set
				} else
				{
					this.OrgAddress.City = value != null ? value.Trim() : "";
					HasChanged = true;
				}

			}
		}

		/// <summary>
        /// Gets/Sets State from OrgAddress object
		/// </summary>
		public string State
		{
			get
			{
				return this.OrgAddress.State;
			}
			set
			{
				if ( this.OrgAddress.State == value )
				{
					//Ignore set
				} else
				{
					this.OrgAddress.State = value != null ? value.Trim() : "";
					HasChanged = true;
				}
				
			}
		}
		/// <summary>
        /// Gets/Sets ZipCode from OrgAddress object
		/// </summary>
		public string ZipCode
		{
			get
			{
				return this.OrgAddress.ZipCode;
			}
			set
			{
				if ( this.OrgAddress.ZipCode == value )
				{
					//Ignore set
				} else
				{
					this.OrgAddress.ZipCode = value != null ? value.Trim() : "";
					HasChanged = true;
				}
			}
		}
		/// <summary>
		/// Gets/Sets ZipCode - additional attribute for existing spelling difference
		/// </summary>
		public string Zipcode
		{
			get
			{
				return this.OrgAddress.ZipCode;
			}
			set
			{
				if ( this.OrgAddress.ZipCode == value )
				{
					//Ignore set
				} else
				{
					this.OrgAddress.ZipCode = value != null ? value.Trim() : "";
					HasChanged = true;
				}
			}
		}

		/// <summary>
        /// Gets/Sets ZipCode plus4 digits from OrgAddress object
		/// </summary>
		public string ZipCode4
		{
			get
			{
				return this.OrgAddress.ZipCodePlus4;
			}
			set
			{
				if ( this.OrgAddress.ZipCodePlus4 == value )
				{
					//Ignore set
				} else
				{
					this.OrgAddress.ZipCodePlus4 = value != null ? value.Trim() : "";
					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Retrive a full ZIP Code - will include the plus4 if present
		/// </summary>
		public string ZipCodeFull
		{
			get
			{
				return this.OrgAddress.ZipCodeFull;
			}
		}

		/// <summary>
        /// Gets/Sets CountyCode from OrgAddress object
		/// </summary>
		public string CountyCode
		{
			get
			{
				return this.OrgAddress.CountyCode;
			}
			set
			{
				if ( this.OrgAddress.CountyCode == value )
				{
					//Ignore set
				} else
				{
					this.OrgAddress.CountyCode = value != null ? value.Trim() : "";
					HasChanged = true;
				}
			}
		}//

		/// <summary>
        /// Gets/Sets Country from OrgAddress object
		/// </summary>
		public string Country
		{
			get
			{
				return this.OrgAddress.Country;
			}
			set
			{
				if ( this.OrgAddress.Country == value )
				{
					//Ignore set
				} else
				{
					this.OrgAddress.Country = value != null ? value.Trim() : "";
					HasChanged = true;
				}
			}
		}//

        /// <summary>
        /// Return address as a string
        /// </summary>
        /// <returns></returns>
        public string AddressToString
        {
            get
			{
                return this.OrgAddress.AddressString(); 
			}
            
        }//
		#endregion


        #region behaviours

        /// <summary>
        /// Format the address lines as one line
        /// </summary>
        /// <returns></returns>
        public string AddressLine()
        {
            return OrgAddress.AddressLine();
        }

        /// <summary>
        /// Format the full address line - non-html, often for exports
        /// </summary>
        /// <returns></returns>
        public string FullAddress()
        {
            return this.OrgAddress.FullAddress();
        } //



        /// <summary>
        /// Format the full address line for html display
        /// </summary>
        /// <returns></returns>
        public string FormatHtmlAddress()
        {
            return this.OrgAddress.HtmlFormat();

        } //

        /// <summary>
        /// Format a summary of this organization as html
        /// </summary>
        /// <returns></returns>
        public string LocationSummary()
        {
            string location = "";
            location = FormatHtmlAddress();

            if ( MainPhone.Length > 0 )
                location = FormatHtmlList( location, "Phone: " + DisplayPhone( this.MainPhone, MainExtension ) );

            return location;
        } //

        /// <summary>
        /// Format the full location fields for html display
        /// </summary>
        /// <returns></returns>
        public string LocationDetails()
        {
            return FormatLocation();
        } //

        /// <summary>
        /// Format the full location fields for html display
        /// </summary>
        /// <returns></returns>
        public string FormatLocation()
        {
            string location = "";
            location = FormatHtmlAddress();

            if ( MainPhone.Length > 0 )
                location = FormatHtmlList( location, "Phone: " + DisplayPhone( this.MainPhone, MainExtension ) );

            if ( AltPhone.Length > 0 )
                location = FormatHtmlList( location, "Alt. Phone: " + DisplayPhone( AltPhone, AltExtension ) );

            if ( this.Fax.Length > 0 )
                location = FormatHtmlList( location, "Fax: " + DisplayPhone( this.Fax ) );

            if ( TTY.Length > 0 )
                location = FormatHtmlList( location, "TTY: " + DisplayPhone( TTY ) );

            if ( Email.Length > 0 )
                location = FormatHtmlList( location, "Email: " + this.Email );

            if ( WebSite.Length > 0 )
                location = FormatHtmlList( location, "Website: " + this.WebSite );


            return location;
        } //

        #endregion


    }
}
