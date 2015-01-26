/*********************************************************************************
* Author: Michael Parsons
*  
* Date: Jun, 2007
* Assembly: workNet.Business.Library
* Description: 
* Notes:
* 
* 
* Copyright 2007, Illinois workNet All rights reserved.
*********************************************************************************/
using System;
using System.Collections;
using System.Globalization;


namespace LRWarehouse.Business
{
	/// <summary>
	/// This is the base class that all data entity classes in the Application 
	/// It is the root of the data object hierarchy.
	/// It also contains helper methods to avoid calls to external libraries
	/// </summary>
	[Serializable]
	public class BaseBusinessDataEntity : IDisposable
	{
		/// <summary>
		/// Initializes a new instance of the workNet.BusObj.Entity.BaseBusinessDataEntity class.
		/// This is the base class for all data entity objects
		/// </summary>
		public BaseBusinessDataEntity()
		{
		}

		#region IDisposable
		//Implement IDisposable.
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this); 
		} //

		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				// Free other state (managed objects).
			}
			// Free your own state (unmanaged objects).
			// Set large fields to null.
		} //

		// Use C# destructor syntax for finalization code.
		~BaseBusinessDataEntity()
		{
			// Simply call Dispose(false).
			Dispose (false);
		}
		#endregion

		public string DEFAULT_GUID = "00000000-0000-0000-0000-000000000000";
		/// <summary>
		/// Default date in place of the min date
		/// </summary>
		public DateTime DefaultDate = new System.DateTime( 1900, 1, 1 );

		#region === Attributes ===
		private bool _hasChanged = false;
		/// <summary>
		/// Set to true if the entity contents change 
		/// </summary>
		public bool HasChanged
		{
			get
			{
				return this._hasChanged;
			}
			set
			{
				this._hasChanged = value;
			}
		}

		private bool isValid = true;
		/// <summary>
		/// Gets/Sets IsValid - future use for determining if an entity is valid
		///										- defaults to Yes on create
		/// </summary>
		public bool IsValid
		{
			get
			{
				return this.isValid;
			}
			set
			{
				this.isValid = value;
			}
		}

		private bool _isActive = true;
		/// <summary>
		/// IsActive - Get/Set
		/// </summary>
		/// 
		public bool IsActive
		{
			get { return _isActive; }
			set { _isActive = value; }
		}

		private int id;
		/// <summary>
		/// Gets/Sets the BaseBusinessDataEntity's associated ID
		/// </summary>
		public virtual int Id
		{
			get
			{
				return this.id;
			}
			set
			{
				this.id = value;
			}
		}//

		private Guid _rowId;
		/// <summary>
		/// Gets/Sets the BaseBusinessDataEntity's RowId
		/// </summary>
		public Guid RowId
		{
			get
			{
				return this._rowId;
			}
			set
			{
				this._rowId = value;
			}
		}//

		private int lastUpdatedById;
		/// <summary>
		/// Gets/Sets the BaseBusinessDataEntity's Last Update Contact ID
		/// </summary>
		public int LastUpdatedById
		{
			get
			{
				return this.lastUpdatedById;
			}
			set
			{
				this.lastUpdatedById = value;
			}
		}

		private string lastUpdatedBy = "";
		/// <summary>
		/// Gets/Sets the BaseBusinessDataEntity's Last Update userid - alternate to FK to user
		/// </summary>
		public string LastUpdatedBy
		{
			get
			{
				return this.lastUpdatedBy;
			}
			set
			{
				this.lastUpdatedBy = value;
			}
		}

		private DateTime lastUpdated;
		/// <summary>
		/// Gets/Sets the BaseBusinessDataEntity's Last Updated Date/time
		/// </summary>
		public DateTime LastUpdated
		{
			get
			{
				return this.lastUpdated;
			}
			set
			{
				this.lastUpdated = value;
			}
		}

		private DateTime created;
		/// <summary>
		/// Gets/Sets the BaseBusinessDataEntity's Creation Date
		/// </summary>
		public DateTime Created
		{
			get
			{
				return this.created;
			}
			set
			{
				this.created = value;
			}
		}

		private int createdById;
		/// <summary>
		/// Gets/Sets the BaseBusinessDataEntity's Created By Contact ID
		/// </summary>
		public int CreatedById
		{
			get
			{
				return this.createdById;
			}
			set
			{
				this.createdById = value;
			}
		}

		private string createdBy = "";
		/// <summary>
		/// Gets/Sets the BaseBusinessDataEntity's Last Update userid - alternate to FK to user
		/// </summary>
		public string CreatedBy
		{
			get
			{
				return this.createdBy;
			}
			set
			{
				this.createdBy = value;
			}
		}

		private string message = "";
		/// <summary>
		/// Gets/Sets the entity message
		/// </summary>
		public string Message
		{
			get
			{
				return this.message;
			}
			set
			{
				this.message = value;
			}
		}

		private bool canEdit = false;
		/// <summary>
		/// Gets/Sets CanEdit - indicates if current object may be edited
		/// </summary>
		public bool CanEdit
		{
			get { return this.canEdit; }
			set { this.canEdit = value; }
		} // end property

		private bool canView = true;
		/// <summary>
		/// Gets/Sets CanView - indicates if current object may be viewed
		/// </summary>
		public bool CanView
		{
			get { return this.canView; }
			set { this.canView = value; }
		} // end property


		private string _tempProperty1 = "";
		/// <summary>
		/// Gets/Sets TempProperty1
		/// Typically used to hold temporary data that neither is persisted nor found in the data store.
		/// as is temporary assuming we do not want to indicate that the entity has changed
		/// </summary>
		public string TempProperty1
		{
			get
			{
				return this._tempProperty1;
			}
			set
			{
				if ( this._tempProperty1 == value )
				{
					//Ignore set
				}
				else
				{
					this._tempProperty1 = value.Trim();
					//as is temporary assuming we do not want to indicate that the entity has changed
					//HasChanged = true;
				}
			}
		}//

		private string _tempProperty2 = "";
		/// <summary>
		/// Gets/Sets TempProperty2
		/// Typically used to hold temporary data that neither is persisted nor found in the data store.
		/// as is temporary assuming we do not want to indicate that the entity has changed
		/// </summary>
		public string TempProperty2
		{
			get
			{
				return this._tempProperty2;
			}
			set
			{
				if ( this._tempProperty2 == value )
				{
					//Ignore set
				}
				else
				{
					this._tempProperty2 = value.Trim();
					//as is temporary assuming we do not want to indicate that the entity has changed
					//HasChanged = true;
				}
			}
		}//


		private string _changeLog = "";
		/// <summary>
		/// Gets/Sets ChangeLog - record differences between current and prior values of entity
		/// </summary>
		public string ChangeLog
		{
			get
			{
				return this._changeLog;
			}
			set
			{
				if ( this._changeLog == value )
				{
					//Ignore set
				}
				else
				{
					this._changeLog = value.Trim();
				}
			}
		}
		#endregion

		#region === Behaviour Methods ===
		/// <summary>
		/// Return true if the entity RowId equals the initial value for a guid
		/// </summary>
		/// <returns></returns>
		public bool HasInitialRowId()
		{
			if ( RowId.ToString() == DEFAULT_GUID )
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Return true if the entity RowId is valid and not equal to the initial value
		/// </summary>
		/// <returns></returns>
		public bool HasValidRowId()
		{
			if ( RowId == null || RowId.ToString() == DEFAULT_GUID )
			{
				return false;
			}
			else
			{
				return true;
			}
		}
		/// <summary>
		/// Return true if the passed Guid equals the initial value for a guid
		/// </summary>
		/// <param name="guid"></param>
		/// <returns></returns>
		public bool IsInitialGuid( Guid guid )
		{
			if ( guid.ToString() == DEFAULT_GUID )
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		#endregion

		#region === Validation Methods ===
		/// <summary>
		/// IsDateNull - determine if passed date is null
		/// </summary>
		/// <param name="dtValue">Date</param>
		/// <returns>True if a valid date, otherwise false</returns>
		public bool IsDateNull( DateTime dtValue )
		{
			if ( dtValue == DateTime.MinValue )
				return true;
			else
				return false;
		} //

		/// <summary>
		/// IsInteger - test if passed string is an integer
		/// </summary>
		/// <param name="stringToTest"></param>
		/// <returns></returns>
		public bool IsInteger( string stringToTest )
		{
			int newVal;
			bool result = false;
			try
			{
				newVal = Int32.Parse( stringToTest );

				// If we get here, then number is an integer
				result = true;
			}
			catch
			{

				result = false;
			}
			return result;

		}

		/// <summary>
		/// IsNumeric - test if passed string is numeric
		/// </summary>
		/// <param name="stringToTest"></param>
		/// <returns></returns>
		public bool IsNumeric( string stringToTest )
		{
			double newVal;
			bool result = false;
			try
			{
				result = double.TryParse( stringToTest, NumberStyles.Any,
					NumberFormatInfo.InvariantInfo, out newVal );
			}
			catch
			{

				result = false;
			}
			return result;

		}


		/// <summary>
		/// IsDate - test if passed string is a valid date
		/// </summary>
		/// <param name="stringToTest"></param>
		/// <returns></returns>
		public bool IsDate( string stringToTest )
		{

			DateTime newDate;
			bool result = false;
			try
			{
				newDate = System.DateTime.Parse( stringToTest );
				result = true;
			}
			catch
			{

				result = false;
			}
			return result;

		} //end


		#endregion

		#region === Display Methods ===
        /// <summary>
        /// Format a title (such as for a library) to be url friendly
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public static string UrlFriendlyTitle( string title )
        {
            if ( title == null || title.Trim().Length == 0 )
                return "";

            string encodedTitle = title.Replace( " - ", "-" );
            encodedTitle = encodedTitle.Replace( " ", "_" );
            //encodedTitle = encodedTitle.Replace( ".", "-" );
            encodedTitle = encodedTitle.Replace( "'", "" );
            encodedTitle = encodedTitle.Replace( "&", "-" );
            encodedTitle = encodedTitle.Replace( "#", "" );
            encodedTitle = encodedTitle.Replace( "$", "S" );
            encodedTitle = encodedTitle.Replace( "%", "percent" );
            encodedTitle = encodedTitle.Replace( "^", "" );
            encodedTitle = encodedTitle.Replace( "*", "" );
            encodedTitle = encodedTitle.Replace( "+", "_" );
            encodedTitle = encodedTitle.Replace( "~", "_" );
            encodedTitle = encodedTitle.Replace( "`", "_" );
            encodedTitle = encodedTitle.Replace( ":", "" );
            encodedTitle = encodedTitle.Replace( ";", "" );
            encodedTitle = encodedTitle.Replace( "?", "" );
            encodedTitle = encodedTitle.Replace( "\"", "_" );
            encodedTitle = encodedTitle.Replace( "\\", "_" );
            encodedTitle = encodedTitle.Replace( "<", "_" );
            encodedTitle = encodedTitle.Replace( ">", "_" );
            encodedTitle = encodedTitle.Replace( "__", "_" );
            encodedTitle = encodedTitle.Replace( "__", "_" );
            if ( encodedTitle.EndsWith( "." ) )
                encodedTitle = encodedTitle.Substring( 0, encodedTitle.Length - 1 );
            return encodedTitle;
        } //

		/// <summary>
		/// Format a date as short date format
		/// </summary>
		/// <param name="dtValue"></param>
		/// <returns></returns>
		public string AsShortDate( DateTime dtValue )
		{

			if ( dtValue.ToShortDateString() == "1/1/0001" )
				return "";
			else
				return dtValue.ToShortDateString();

		} // end property
          
		/// <summary>
		/// Format a date as MMM DD/YYYY (ex. Mar 12/2007)
		/// </summary>
		/// <param name="dtValue"></param>
		/// <returns></returns>
		public string AsMMMDDYYYY( DateTime dtValue )
		{

			if ( dtValue.ToShortDateString() == "1/1/0001" )
				return "";
			else
				return dtValue.ToString( "MMM dd, yyyy" );

		} // end 
		/// <summary>
		/// Format a date as Weekday, MMM DD/YYYY (ex. Thursday, Mar 12/2007
		/// </summary>
		/// <param name="dtValue"></param>
		/// <returns></returns>
		public string As_DOW_MMM_DD_YYYY( DateTime dtValue )
		{

			if ( dtValue.ToShortDateString() == "1/1/0001" )
				return "";
			else
				return dtValue.ToString( "dddd, MMM dd, yyyy" );

		} // end 

		/// <summary>
		/// CreatedByTitle - Format created by date and userid
		/// </summary>
		/// <returns></returns>
		public string CreatedByTitle()
		{
			// Default to the use of the userid
			// Could override or extend in descendents
			string title = "";
			string separator = "";

			if ( !IsDateNull( this.Created ) )
			{
				title = this.Created.ToShortDateString();
				separator = " - ";
			}
			if ( this.CreatedBy.Length > 0 )
			{
				title += separator + this.CreatedBy;

			}
			return title;
		} //

		/// <summary>
		/// UpdatedByTitle - Format updated by date and userid
		/// </summary>
		/// <returns></returns>
		public string UpdatedByTitle()
		{
			// Default to the use of the userid
			// Could override or extend in descendents
			string title = "";
			string separator = "";

			if ( !IsDateNull( this.LastUpdated ) )
			{
				title = this.LastUpdated.ToShortDateString();
				separator = " - ";
			}
			if ( this.LastUpdatedBy.Length > 0 )
			{
				title += separator + this.LastUpdatedBy;

			}
			return title;
		} //

		/// <summary>
		/// Strip special characters from a phone number
		/// </summary>
		/// <param name="phone"></param>
		/// <returns></returns>
		public string StripPhone( string phone )
		{
			if ( phone.Trim().Length == 0 )
				return "";

			string workPhone = phone.Trim();
			workPhone = workPhone.Replace( "-", "" );
			workPhone = workPhone.Replace( " ", "" );
			workPhone = workPhone.Replace( "_", "" );
			workPhone = workPhone.Replace( ".", "" );
			workPhone = workPhone.Replace( "(", "" );
			workPhone = workPhone.Replace( ")", "" );

			return workPhone;
		}

		/// <summary>
		/// Display a formatted phone number
		/// </summary>
		/// <param name="phone"></param>
		/// <returns>a formatted phone number</returns>
		public string DisplayPhone( string phone )
		{
			//use default format
			return DisplayPhone( 2, phone, "" );
		}
		/// <summary>
		/// Display a formatted phone number
		/// </summary>
		/// <param name="phone"></param>
		/// <param name="ext"></param>
		/// <returns>a formatted phone number</returns>
		public string DisplayPhone( string phone, string ext )
		{
			//use default format
			return DisplayPhone( 2, phone, ext );
		}

		/// <summary>
		/// Display a formatted phone number
		/// </summary>
		/// <param name="displayFormat"></param>
		/// <param name="phone"></param>
		/// <param name="ext"></param>/// 
		/// <returns></returns>
		public string DisplayPhone( int displayFormat, string phone, string ext )
		{
			string part1, part2, part3;
			if ( phone.Length > 9 )
			{
				part1 = phone.Substring( 0, 3 );
				part2 = phone.Substring( 3, 3 );
				part3 = phone.Substring( 6, 4 );

				return DisplayPhone( displayFormat, part1, part2, part3, ext );
			}
			else
			{
				return "";
			}
		}
		/// <summary>
		/// Display a formatted phone number
		/// </summary>
		/// <param name="displayFormat"></param>
		/// <param name="part1"></param>/// 
		/// <param name="part2"></param>
		/// <param name="part3"></param>
		/// <param name="ext"></param>
		/// <returns></returns>
		private string DisplayPhone( int displayFormat, string part1, string part2, string part3, string ext )
		{
			string phone;

			if ( displayFormat == 1 )
			{
				phone = "(" + part1 + ") ";
				phone += part2 + "-";
				phone += part3;

			}
			else if ( displayFormat == 2 )
			{
				phone = part1 + "-" + part2 + "-" + part3;
			}
			else
			{
				phone = "(" + part1 + ") ";
				phone += part2 + "-";
				phone += part3;
			}

			if ( ext.Length > 0 )
			{
				phone += " ext: " + ext;
			}

			return phone;
		}
		/// <summary>
		/// Join the 3 parts of phone number 
		/// </summary>
		/// <param name="part1"></param>
		/// <param name="part2"></param>
		/// <param name="part3"></param>
		/// <returns></returns>
		public string JoinPhone( string part1, string part2, string part3 )
		{
			string phone;

			phone = part1;
			phone += part2;
			phone += part3;

			return phone;
		}

		/// <summary>
		/// Spilt a phone number into 3 parts
		/// </summary>
		/// <param name="phone"></param>
		/// <param name="part1"></param>
		/// <param name="part2"></param>
		/// <param name="part3"></param>
		public void SplitPhone( string phone, ref string part1, ref string part2, ref string part3 )
		{
			if ( phone.Length > 9 )
			{
				part1 = phone.Substring( 0, 3 );
				part2 = phone.Substring( 3, 3 );
				part3 = phone.Substring( 6, 4 );
			}
		}

		/// <summary>
		/// HistoryTitle - Format created by and updated by information in one line
		/// </summary>
		/// <returns></returns>
		public virtual string HistoryTitle()
		{
			string createdTitle = this.CreatedByTitle();
			string updatedTitle = this.UpdatedByTitle();
			string title = "";

			if ( createdTitle.Length > 0 ) title = "Created: " + createdTitle + " ";
			if ( updatedTitle.Length > 0 ) title += "Last Updated: " + updatedTitle + " ";

			return title;

		}

		/// <summary>
		/// format an HTML list - determines whether a break should be included
		/// </summary>
		/// <param name="list"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		protected string FormatHtmlList( string list, string value )
		{
			string prefix = "";

			if ( list.Length > 0 )
				prefix = "<br/>";

			if ( value.Length > 0 )
				return list + prefix + value;
			else
				return list;

		} //
		#endregion

		/// <summary>
		/// Log change to a property
		/// </summary>
		/// <param name="propertyName"></param>
		/// <param name="prevValue"></param>
		/// <param name="currentValue"></param>
		protected void LogPropertyChange( string propertyName, string prevValue, string currentValue )
		{
			string change = "";
			//do quick change check
			if ( prevValue.Trim() == currentValue.Trim() )
				return;

			HasChanged = true;

			change = string.Format( " Property: {0} - Old: [{1}], New: [{2}] ", propertyName, prevValue, currentValue );

			if ( ChangeLog.Length == 0 )
				ChangeLog = change;
			else
				ChangeLog += "\\r" + change;

		} //
	}
}
