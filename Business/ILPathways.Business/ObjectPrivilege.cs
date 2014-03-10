/*********************************************************************************
= Author: Michael Parsons
=
= Date: Jan 21/2009
= Assembly: ILPathways.Business
= Description:
= Notes:
=
=
= Copyright 2007, Illinois workNet All rights reserved.
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace ILPathways.Business
{
	///<summary>
	///Represents an object that describes a ObjectPrivilege
	///</summary>
	[Serializable]
  public class ObjectPrivilege : BaseBusinessDataEntity
	{

    ///<summary>
    ///Initializes a new instance of the ILPathways.Business.ObjectPrivilege class.
    ///</summary>
		public ObjectPrivilege() { }

		#region Properties

		private int _objectId;
		/// <summary>
		/// Gets/Sets ObjectId
		/// </summary>
		public int ObjectId
		{
			get
			{
				return this._objectId;
			}
			set
			{
				if ( this._objectId == value )
				{
					//Ignore set
				} else
				{
					this._objectId = value;
					HasChanged = true;
				}
			}
		}

		private string _objectName;
		/// <summary>
		/// Gets/Sets ObjectName
		/// </summary>
		public string ObjectName
		{
			get { return this._objectName; }
			set
			{
				if ( this._objectName == value )
				{
					//Ignore set
				} else
				{
					this._objectName = value;
				}
			}
		}

		private string _displayName;
		/// <summary>
		/// Gets/Sets DisplayName
		/// </summary>
		public string DisplayName
		{
			get { return this._displayName; }
			set
			{
				if ( this._displayName == value )
				{
					//Ignore set
				} else
				{
					this._displayName = value;
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
				if ( this._subObjectId == value )
				{
					//Ignore set
				} else
				{
					this._subObjectId = value;
					HasChanged = true;
				}
			}
		}

		private string _subObjectName;
		/// <summary>
		/// Gets/Sets SubObjectName
		/// </summary>
		public string SubObjectName
		{
			get { return this._subObjectName; }
			set
			{
				if ( this._subObjectName == value )
				{
					//Ignore set
				} else
				{
					this._subObjectName = value;
					HasChanged = true;
				}
			}
		}

		private string _desc;
		/// <summary>
		/// Gets/Sets Description
		/// </summary>
		public string Description
		{
			get { return this._desc; }
			set
			{
				if ( this._desc == value )
				{
					//Ignore set
				} else
				{
					this._desc = value;
					HasChanged = true;
				}
			}
		}
		private int _sequence = 0;
		/// <summary>
		/// Gets/Sets Sequence
		/// </summary>
		public int Sequence
		{
			get
			{
				return this._sequence;
			}
			set
			{
				if ( this._sequence == value )
				{
					//Ignore set
				} else
				{
					this._sequence = value;
					HasChanged = true;
				}
			}
		}

		private int _createPrivilege=0;
		/// <summary>
		/// Gets/Sets CreatePrivilege
		/// </summary>
		public int CreatePrivilege
		{
			get
			{
				return this._createPrivilege;
			}
			set
			{
				if ( this._createPrivilege == value )
				{
					//Ignore set
				} else
				{
					this._createPrivilege = value;
					HasChanged = true;
					if ( _createPrivilege > ( int ) EPrivilegeDepth.None )
					{
						this.CanEdit = true;
					}
				}
			}
		}

		private int _readPrivilege = 0;
		/// <summary>
		/// Gets/Sets ReadPrivilege
		/// </summary>
		public int ReadPrivilege
		{
			get
			{
				return this._readPrivilege;
			}
			set
			{
				if ( this._readPrivilege == value )
				{
					//Ignore set
				} else
				{
					this._readPrivilege = value;
					HasChanged = true;
					if ( _readPrivilege > 0 )
						this.CanView = true;
				}
			}
		}//

		private int _writePrivilege = 0;
		/// <summary>
		/// Gets/Sets WritePrivilege
		/// </summary>
		public int WritePrivilege
		{
			get
			{
				return this._writePrivilege;
			}
			set
			{
				if ( this._writePrivilege == value )
				{
					//Ignore set
				} else
				{
					this._writePrivilege = value;
					HasChanged = true;
					if ( _writePrivilege > 0 )
						this.CanEdit = true;
				}
			}
		}//

		private int _deletePrivilege = 0;
		/// <summary>
		/// Gets/Sets DeletePrivilege
		/// </summary>
		public int DeletePrivilege
		{
			get
			{
				return this._deletePrivilege;
			}
			set
			{
				if ( this._deletePrivilege == value )
				{
					//Ignore set
				} else
				{
					this._deletePrivilege = value;
					HasChanged = true;
				}
			}
		}//

		private int _appendPrivilege = 0;
		/// <summary>
		/// Gets/Sets AppendPrivilege
		/// </summary>
		public int AppendPrivilege
		{
			get
			{
				return this._appendPrivilege;
			}
			set
			{
				if ( this._appendPrivilege == value )
				{
					//Ignore set
				} else
				{
					this._appendPrivilege = value;
					HasChanged = true;
				}
			}
		}//

		private int _appendToPrivilege = 0;
		/// <summary>
		/// Gets/Sets AppendToPrivilege
		/// </summary>
		public int AppendToPrivilege
		{
			get
			{
				return this._appendToPrivilege;
			}
			set
			{
				if ( this._appendToPrivilege == value )
				{
					//Ignore set
				} else
				{
					this._appendToPrivilege = value;
					HasChanged = true;
				}
			}
		}//

		private int _assignPrivilege = 0;
		/// <summary>
		/// Gets/Sets AssignPrivilege
		/// </summary>
		public int AssignPrivilege
		{
			get
			{
				return this._assignPrivilege;
			}
			set
			{
				if ( this._assignPrivilege == value )
				{
					//Ignore set
				} else
				{
					this._assignPrivilege = value;
					HasChanged = true;
				}
			}
		}//

		private int _approvePrivilege = 0;
		/// <summary>
		/// Gets/Sets ApprovePrivilege
		/// </summary>
		public int ApprovePrivilege
		{
			get
			{
				return this._approvePrivilege;
			}
			set
			{
				if ( this._approvePrivilege == value )
				{
					//Ignore set
				} else
				{
					this._approvePrivilege = value;
					HasChanged = true;
				}
			}
		}//

		private int _sharePrivilege = 0;
		/// <summary>
		/// Gets/Sets SharePrivilege
		/// </summary>
		public int SharePrivilege
		{
			get
			{
				return this._sharePrivilege;
			}
			set
			{
				if ( this._sharePrivilege == value )
				{
					//Ignore set
				} else
				{
					this._sharePrivilege = value;
					HasChanged = true;
				}
			}
		}


		#endregion
		#region ===== Can/Has ===============================
		/// <summary>
		/// Does user have create privilege on the object?
		/// </summary>
		/// <returns></returns>
		public bool CanCreate()
		{
			return ( this.CreatePrivilege > 0 );
		} //

		/// <summary>
		/// Determine if user has Create privilege for Region 
		/// </summary>
		/// <returns>True if has Region create</returns>
		public bool HasRegionCreate()
		{
			if ( CreatePrivilege >= ( int ) EPrivilegeDepth.Region )
				return true;
			else
				return false;
		} //

		/// <summary>
		/// Does user have read privilege on the object?
		/// </summary>
		/// <returns></returns>
		public bool CanRead()
		{
			return ( this.ReadPrivilege > 0 );
		} //

		/// <summary>
		/// Determine if user can view all levels
		/// </summary>
		/// <returns></returns>
		public bool CanReadAll()
		{
			if ( ReadPrivilege > ( int ) EPrivilegeDepth.State )
				return true;
			else
				return false;
		} //
		/// <summary>
		/// Determine if user has Global View
		/// </summary>
		/// <returns>True if has global read</returns>
		public bool CanReadGlobal()
		{
			if ( ReadPrivilege > ( int ) EPrivilegeDepth.Region )
				return true;
			else
				return false;
		} //
		/// <summary>
		/// Determine if user has Region View
		/// </summary>
		/// <returns>True if has Region read</returns>
		public bool CanReadRegion()
		{
			if ( ReadPrivilege >= ( int ) EPrivilegeDepth.Region )
				return true;
			else
				return false;
		} //

		/// <summary>
		/// Does user have update privilege on the object?
		/// </summary>
		/// <returns></returns>
		public bool CanUpdate()
		{
			return ( this.WritePrivilege > 0 );
		} //


		/// <summary>
		/// Determine if user has Update privilege for Region 
		/// </summary>
		/// <returns>True if has Region create</returns>
		public bool HasRegionUpdate()
		{
			if ( WritePrivilege >= ( int ) EPrivilegeDepth.Region )
				return true;
			else
				return false;
		} //
		/// <summary>
		/// Determine if user has Update privilege for State 
		/// </summary>
		/// <returns>True if has State create</returns>
		public bool HasStateUpdate()
		{
			if ( WritePrivilege >= ( int ) EPrivilegeDepth.State )
				return true;
			else
				return false;
		} //

		/// <summary>
		/// Does user have delete privilege on the object?
		/// </summary>
		/// <returns></returns>
		public bool CanDelete()
		{
			return ( this.DeletePrivilege > 0 );
		} //

		#endregion

		#region ===== Other Behaviours ===============================
		/// <summary>
		/// Return true if user has the privilege to update the passed object id
		/// </summary>
		/// <param name="myObjId"></param>
		/// <param name="destObjId"></param>
		/// <returns></returns>
		public bool CanUpdateObject( int myObjId, int destObjId )
		{
			if ( CanUpdate() )
				if ( myObjId == destObjId )
					return true;

			return false;

		} //


		#endregion
	}
}
