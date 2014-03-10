/*********************************************************************************
= Author: Michael Parsons
=
= Date: Feb 07/2014
= Assembly: ILPathways.Business
= Description:
= Notes:
=
=
= Copyright 2014, Illinois workNet All rights reserved.
********************************************************************************/
using System;
using System.Collections.Generic;

namespace ILPathways.Business
{
    ///<summary>
    ///Represents an object that describes a OrganizationMemberRole
    ///</summary>
    [Serializable]
    public class OrganizationMemberRole : BaseBusinessDataEntity
    {
        ///<summary>
        ///Initializes a new instance of the ILPathways.Business.OrganizationMemberRole class.
        ///</summary>
        public OrganizationMemberRole() { }


        #region Properties created from dictionary for OrganizationMemberRole

        private int _orgMemberId;
        /// <summary>
        /// Gets/Sets OrgMemberId
        /// </summary>
        public int OrgMemberId
        {
            get
            {
                return this._orgMemberId;
            }
            set
            {
                if ( this._orgMemberId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._orgMemberId = value;
                    HasChanged = true;
                }
            }
        }
        public int OrgContactId
        {
            get
            {
                return this._orgMemberId;
            }
            set
            {
                if ( this._orgMemberId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._orgMemberId = value;
                    HasChanged = true;
                }
            }
        }
        private int _roleId;
        /// <summary>
        /// Gets/Sets RoleId
        /// </summary>
        public int RoleId
        {
            get
            {
                return this._roleId;
            }
            set
            {
                if ( this._roleId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._roleId = value;
                    HasChanged = true;
                }
            }
        }
        public string RoleTitle { get; set; }

        #endregion
    } // end class 
} // end Namespace 

