using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    ///<summary>
    ///Represents an object that describes a GroupOrgMember
    ///</summary>
    [Serializable]
    public class GroupOrgMember : BaseBusinessDataEntity
    {
        ///<summary>
        ///Initializes a new instance of the ILPathways.Business.GroupOrgMember class.
        ///</summary>
        public GroupOrgMember()
        {
            IsActive = true;
            RelatedOrg = new Organization();
        }

        #region Properties created from dictionary for GroupOrgMember


        private int _groupId;
        /// <summary>
        /// Gets/Sets GroupId
        /// </summary>
        public int GroupId
        {
            get
            {
                return this._groupId;
            }
            set
            {
                if ( this._groupId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._groupId = value;
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
                }
                else
                {
                    this._status = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private string _category = "";
        /// <summary>
        /// Gets/Sets Category
        /// </summary>
        public string Category
        {
            get
            {
                return this._category;
            }
            set
            {
                if ( this._category == value )
                {
                    //Ignore set
                }
                else
                {
                    this._category = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private string _memberType = "";
        /// <summary>
        /// Gets/Sets MemberType
        /// </summary>
        public string MemberType
        {
            get
            {
                return this._memberType;
            }
            set
            {
                if ( this._memberType == value )
                {
                    //Ignore set
                }
                else
                {
                    this._memberType = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private string _varchar1 = "";
        /// <summary>
        /// Gets/Sets Varchar1
        /// </summary>
        public string Varchar1
        {
            get
            {
                return this._varchar1;
            }
            set
            {
                if ( this._varchar1 == value )
                {
                    //Ignore set
                }
                else
                {
                    this._varchar1 = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private string _varchar2 = "";
        /// <summary>
        /// Gets/Sets Varchar2
        /// </summary>
        public string Varchar2
        {
            get
            {
                return this._varchar2;
            }
            set
            {
                if ( this._varchar2 == value )
                {
                    //Ignore set
                }
                else
                {
                    this._varchar2 = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private string _varchar3 = "";
        /// <summary>
        /// Gets/Sets Varchar3
        /// </summary>
        public string Varchar3
        {
            get
            {
                return this._varchar3;
            }
            set
            {
                if ( this._varchar3 == value )
                {
                    //Ignore set
                }
                else
                {
                    this._varchar3 = value.Trim();
                    HasChanged = true;
                }
            }
        }


        #endregion


        #region External entities
        Organization _relatedOrg = null;
        /// <summary>
        /// Get/Set AppUser for group member
        /// </summary>
        public Organization RelatedOrg
        {
            get
            {
                return this._relatedOrg;
            }
            set
            {
                this._relatedOrg = value;
            }
        }//

        #endregion

        #region Helper Properties - outside actual table

        private string _groupName = "";
        /// <summary>
        /// Composite: Gets/Sets GroupName
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
        /// Composite: Gets/Sets GroupCode from the parent group
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

        private string _fullName = "";
        /// <summary>
        /// Composite: Gets/Sets group org name (where full org not provided)
        /// </summary>
        public string OrgName
        {
            get
            {
                return this._fullName;
            }
            set
            {
                if ( this._fullName == value )
                {
                    //Ignore set
                }
                else
                {
                    this._fullName = value.Trim();
                    HasChanged = true;
                }
            }
        }

        #endregion
    } // end class 
} // end Namespace 

