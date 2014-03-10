using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    ///<summary>
    ///Represents an object that describes a Group
    ///</summary>
    [Serializable]
    public class AppGroup : BaseBusinessDataEntity
    {

        ///<summary>
        ///Initializes a new instance of the workNet.BusObj.Entity.Group class.
        ///</summary>
        public AppGroup() { }
        public static int GROUP_TYPEID_GENERAL = 1;
        public static int GROUP_TYPEID_AUTHORIZATION = 2;
        public static int GROUP_TYPE_PERSONAL = 3;
        public static int GROUP_TYPE_ORGANIZATION = 4;

        #region Properties created from dictionary for Group


        private string _groupName = "";
        /// <summary>
        /// Gets/Sets Title
        /// </summary>
        public string Title
        {
            get
            {
                return this._groupName;
            }
            set
            {
                if ( this._groupName == value )
                {
                    //Ignore set
                }
                else
                {
                    this._groupName = value.Trim();
                    HasChanged = true;
                }
            }
        }
        /// <summary>
        /// Gets/Sets GroupName - alias for Title
        /// </summary>
        public string GroupName
        {
            get
            {
                return this._groupName;
            }
            set
            {
                if ( this._groupName == value )
                {
                    //Ignore set
                }
                else
                {
                    this._groupName = value.Trim();
                    HasChanged = true;
                }
            }
        }
        private string _groupCode = "";
        /// <summary>
        /// Gets/Sets GroupCode
        /// </summary>
        public string GroupCode
        {
            get
            {
                return this._groupCode;
            }
            set
            {
                if ( this._groupCode == value )
                {
                    //Ignore set
                }
                else
                {
                    this._groupCode = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private int _parentGroupId;
        /// <summary>
        /// Gets/Sets ParentGroupId
        /// </summary>
        public int ParentGroupId
        {
            get
            {
                return this._parentGroupId;
            }
            set
            {
                if ( this._parentGroupId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._parentGroupId = value;
                    HasChanged = true;
                }
            }
        }//
        
        private string _description = "";
        /// <summary>
        /// Gets/Sets Description
        /// </summary>
        public string Description
        {
            get
            {
                return this._description;
            }
            set
            {
                if ( this._description == value )
                {
                    //Ignore set
                }
                else
                {
                    this._description = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private int _groupTypeId;
        /// <summary>
        /// Gets/Sets ParentGroupId
        /// </summary>
        public int GroupTypeId
        {
            get
            {
                return this._groupTypeId;
            }
            set
            {
                if ( this._groupTypeId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._groupTypeId = value;
                    HasChanged = true;
                }
            }
        }//
        private string _groupType = "";
        /// <summary>
        /// Gets/Sets GroupType
        /// </summary>
        public string GroupType
        {
            get
            {
                return this._groupType;
            }
            set
            {
                if ( this._groupType == value )
                {
                    //Ignore set
                }
                else
                {
                    this._groupType = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private int _contactId;
        /// <summary>
        /// Gets/Sets ContactId
        /// </summary>
        public int ContactId
        {
            get
            {
                return this._contactId;
            }
            set
            {
                if ( this._contactId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._contactId = value;
                    HasChanged = true;
                }
            }
        }

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
                }
                else
                {
                    this._orgId = value;
                    HasChanged = true;
                }
            }
        }

        //private bool _hasPrivateCredentials;
        ///// <summary>
        ///// Gets/Sets HasPrivateCredentials
        ///// </summary>
        //public bool HasPrivateCredentials
        //{
        //    get
        //    {
        //        return this._hasPrivateCredentials;
        //    }
        //    set
        //    {
        //        if ( this._hasPrivateCredentials == value )
        //        {
        //            //Ignore set
        //        }
        //        else
        //        {
        //            this._hasPrivateCredentials = value;
        //            HasChanged = true;
        //        }
        //    }
        //}

        //private int _memberCount;
        ///// <summary>
        ///// Gets/Sets MemberCount
        ///// </summary>
        //public int MemberCount
        //{
        //    get
        //    {
        //        return this._memberCount;
        //    }
        //    set
        //    {
        //        this._memberCount = value;
        //    }
        //}
        #endregion

        #region behaviours


        #endregion

        #region External entities
        AppUser _contact = null;
        /// <summary>
        /// Get/Set primary contact
        /// </summary>
        public AppUser PrimaryContact
        {
            get
            {
                return this._contact;
            }
            set
            {
                if ( this._contact == value )
                {
                    //Ignore set
                }
                else
                {
                    this._contact = value;
                    //HasChanged = true;
                }
            }
        }


        #endregion

    } // end class 
} // end Namespace 

