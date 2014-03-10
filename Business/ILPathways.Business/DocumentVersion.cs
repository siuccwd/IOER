/*********************************************************************************
= Author: Michael Parsons
=
= Date: Jan 14/2011
= Assembly: ILPathways.Business
= Description:
= Notes:
=
=
= Copyright 2011, Illinois workNet All rights reserved.
********************************************************************************/
using System;

namespace ILPathways.Business
{
	///<summary>
	///Represents an object that describes a DocumentVersion
	///</summary>
	[Serializable]
	public class DocumentVersion : ApplicationResource
	{
		///<summary>
		///Initializes a new instance of the ILPathways.Business.DocumentVersion class.
		///</summary>
		public DocumentVersion() { }

		#region Properties created from dictionary for DocumentVersion. See ApplicationResource for inherited properties
		//TODO - add validation for allowable mime types
		private string _summary = "";
		/// <summary>
		/// Gets/Sets Summary
		/// </summary>
		public string Summary
		{
			get
			{
				return this._summary;
			}
			set
			{
				if ( this._summary == value )
				{
					//Ignore set
				} else
				{
					this._summary = value.Trim();
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
				if ( this._status == value )
				{
					//Ignore set
				} else
				{
					this._status = value.Trim();
					HasChanged = true;
				}
			}
		}

		#endregion
	} // end class 
} // end Namespace 

