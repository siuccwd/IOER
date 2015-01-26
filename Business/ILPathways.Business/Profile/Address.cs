using System;
using System.Collections.Generic;
using System.Text;

namespace ILPathways.Business
{
	/// <summary>
	/// Address Class
	/// </summary>
	[Serializable]
	public class Address : BaseBusinessDataEntity
	{
		///<summary>
		///Initializes a new instance of the workNet.Business.Library.Address class.
		///</summary>
		public Address() { }

		#region Properties
		private string _address1 = "";
		/// <summary>
		/// Gets/Sets Address
		/// </summary>
		public string Address1
		{
			get
			{
				return this._address1;
			}
			set
			{
				if ( this._address1 == value )
				{
					//Ignore set
				} else
				{
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
				if ( this._address2 == value )
				{
					//Ignore set
				} else if ( value == null )
				{
					this._address2 = null;
					HasChanged = true;
				} else
				{
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
				if ( this._city == value )
				{
					//Ignore set
				} else
				{
					this._city = value.Trim();
					HasChanged = true;
				}
			}
		}

		private string _state = "";
		/// <summary>
		/// Gets/Sets State
		/// </summary>
		public string State
		{
			get
			{
				return this._state;
			}
			set
			{
				if ( this._state == value )
				{
					//Ignore set
				} else
				{
					this._state = value.Trim();
					HasChanged = true;
				}
			}
		}

		private string _zipCode = "";
		/// <summary>
		/// Gets/Sets ZipCode
		/// </summary>
		public string ZipCode
		{
			get
			{
				return this._zipCode;
			}
			set
			{
				if ( this._zipCode == value )
				{
					//Ignore set
				} else
				{
					//TODO - may want to have a check for 5 digits. If 9 or 10, then set ZipCodePlus4??
					string zc = value.Trim();
					zc = zc.Replace( "-", "" );
					if ( zc.Length <= 5 )
					{
						this._zipCode = zc;

					} else if ( zc.Length == 9 )
					{
						this._zipCode = zc.Substring( 0, 5 );
						ZipCodePlus4 = zc.Substring( 5, 4 );

					} else if ( zc.Length == 10 )
					{
						//MP - we will force ZIP Code plus 4 entry to include the dash, so 10 would be valid
						this._zipCode = zc.Substring( 0, 5 );
						ZipCodePlus4 = zc.Substring( 6, 4 );
					} else
					{
						//apparantly invalid, so ??
					}
					HasChanged = true;
				}
			}
		}
		/// <summary>
		/// temp with alternate spelling
		/// </summary>
		public string Zipcode1
		{
			get
			{
				return _zipCode;
			}
			set
			{
				ZipCode = value.Trim();
			}
		}
		private string _zipcodePlus4 = "";
		/// <summary>
		/// Gets/Sets ZipCodePlus4
		/// </summary>
		public string ZipCodePlus4
		{
			get
			{
				return this._zipcodePlus4;
			}
			set
			{
				if ( this._zipcodePlus4 == value )
				{
					//Ignore set
				} else
				{
					this._zipcodePlus4 = value.Trim();
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
				if ( ZipCodePlus4.Length == 4 )
					return _zipCode + "-" + _zipcodePlus4;
				else
				{
					if ( _zipCode.Length == 5 )
						return this._zipCode;
					else
						return "";
				}
			}
		}//

		private string _countyCd = "";
		/// <summary>
		/// Gets/Sets County
		/// </summary>
		public string CountyCode
		{
			get
			{
				return this._countyCd;
			}
			set
			{
				if ( this._countyCd == value )
				{
					//Ignore set
				} else
				{
					this._countyCd = value.Trim();
					HasChanged = true;
				}
			}
		} //

		private string _county = "";
		/// <summary>
		/// Gets/Sets County
		/// </summary>
		public string County
		{
			get
			{
				return this._county;
			}
			set
			{
				if ( this._county == value )
				{
					//Ignore set
				} else
				{
					this._county = value.Trim();
					HasChanged = true;
				}
			}
		} //

		private string _country = "USA";
		/// <summary>
		/// Gets/Sets Country
		/// </summary>
		public string Country
		{
			get
			{
				return this._country;
			}
			set
			{
				if ( this._country == value )
				{
					//Ignore set
				} else
				{
					this._country = value.Trim();
					HasChanged = true;
				}
			}
		}
		#endregion

		#region Behaviors
		/// <summary>
		/// Format the full address line - non-html, often for exports
		/// </summary>
		/// <returns></returns>
		public string FullAddress()
		{
			string address = "";

			//if city found then include whole address, otherwise just zip
			if ( City.Length > 0 )
			{
				address = AddressLine();

				address += "\n" + City + "," + this.State + "\n";
			}
			//if ( this.IsUSA() )
			//{
			address += ZipCodeFull;
			//} else
			//{
			//  address += "\n" + this.Country + "    " + this.ZipCodeFull;
			//}

			return address;
		} //
		/// <summary>
		/// Format the address lines as one line
		/// </summary>
		/// <returns></returns>
		public string AddressLine()
		{
			if ( this.Address2.Length > 0 )
				return Address1 + ", " + Address2;
			else
				return Address1;
		}

		/// <summary>
		/// Return address as a string
		/// </summary>
		/// <returns></returns>
		public string AddressString()
		{
            string address = Address1.Length > 0 ? Address1 : "";

            if ( Address2.Length > 0 )
                address += " " + Address2;

			//a valid address needs to have a city
			if ( City.Length > 0 )
			{
				address += address.Length > 0 ? ", " + City : City ;
			}
            if ( State.Length > 0 )
                address += address.Length > 0 ? ", " + State : this.State + " ";

            if ( ZipCodeFull.Length > 0 )
                address += address.Length > 0 ? ", " + ZipCodeFull : this.ZipCodeFull + " ";

			return address;
		}//

		/// <summary>
		/// Return address formatted as Html
		/// </summary>
		/// <returns></returns>
		public string HtmlFormat()
		{
			string address = "";
			//a valid address needs to have a city
			if ( City.Length > 0 )
			{
				if ( this.Address2.Length > 0 )
					address = Address1 + "<br/> " + Address2;
				else
					address = Address1;

				address += "<br/>" + City + "," + this.State + "&nbsp;&nbsp;" + this.ZipCodeFull; ;
			}	else 
			{

				//if ( this.IsUSA() )
				//{
				address += this.ZipCodeFull;
				//} else
				//{
				//  address += "<br/>" + this.Country + "    " + this.ZipCode;
				//}
			}
			return address;
		}

		/// <summary>
		/// ZipCodeSplit-Handle 5 or 9 digit zip input 
		/// </summary>
		/// <returns></returns>
		public void ZipCodeSplit( string zip )
		{
			//does latter hold plus4
			if ( zip.Trim().Length == 5 )
			{
				ZipCode = zip;
				ZipCodePlus4 = "";
			} else if ( zip.Trim().Length == 10 )
			{
				ZipCode = zip.Substring(0,5);
				ZipCodePlus4 = zip.Substring( 6, 4 );
			} else 
			{
				//??
				ZipCode = "";
				ZipCodePlus4 = "";
			}

		}//
		/// <summary>
		/// Handle 5 or 9 digit zip display
		/// </summary>
		/// <returns></returns>
		public string ZipCodeDisplay()
		{
			string zip = ZipCode;

			//does latter hold plus4
			if ( zip.Length == 5 )
			{
				//if not append
				if ( ZipCodePlus4.Length > 0 )
					zip += "-" + ZipCodePlus4;
			}

			return zip;
		}//
		#endregion
	}
}
