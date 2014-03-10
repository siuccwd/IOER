/*********************************************************************************
* Author: Michael Parsons
*					EPrivilegeDepth
* Date: Aug, 2007
* Assembly: workNet.BusObj.Entity
* Description: 
* Notes:
* 
* 
* Copyright 2007, Illinois All rights reserved.
*********************************************************************************/
using System;


namespace ILPathways.Business
{
	/// <summary>
	/// Specifies the Privilege Depth possible values
	/// </summary>
	public enum EPrivilegeDepth : int
	{
		/// <summary>
		/// Indicates no privilege
		/// </summary>
		None = 0,

		/// <summary>
		/// Indicates local (ex. current dept.
		/// </summary>
		Local = 1,

		/// <summary>
		/// Indicates for organization and subordinates
		/// </summary>
		Organization = 2,

		/// <summary>
		/// Indicates a region
		/// </summary>
		Region = 3,

		/// <summary>
		/// Indicates statewide
		/// </summary>
		State = 4,

		/// <summary>
		/// Indicates all
		/// </summary>
		Global = 5

	}
}
