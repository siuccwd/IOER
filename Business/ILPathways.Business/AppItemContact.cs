/*********************************************************************************
= Author: Michael Parsons
=
= Date: Mar 16/2011
= Assembly: ILPathways.Business
= Description:
= Notes:
=
=
= Copyright 2011, Illinois workNet All rights reserved.
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace ILPathways.Business
{
    [Serializable]
	public class AppItemContact : BaseBusinessDataEntity
	{

		private Guid _parentRowId;
		/// <summary>
		/// Gets/Sets the RowId of the parent AppItem
		/// </summary>
		public Guid AppItemRowId
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

		private int _userId = 0;
		/// <summary>
		/// Gets/Sets related contact UserId
		/// </summary>
		public int UserId
		{
			get
			{
				return this._userId;
			}
			set
			{
				if ( this._userId == value )
				{
					//Ignore set
				} else
				{
					this._userId = value;
					HasChanged = true;
				}
			}
		}//

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
	}
}
