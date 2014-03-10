using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    ///<summary>
    ///Represents an object that describes a GroupMember
    ///</summary>
    [Serializable]
    public class GroupMember : BaseBusinessDataEntity
    {
        ///<summary>
        ///Initializes a new instance of the workNet.BusObj.Entity.GroupMember class.
        ///</summary>
        public GroupMember()
        {
            IsActive = true;
            RelatedAppUser = new AppUser();
        }

        #region Properties created from dictionary for GroupMember


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


        private int _contactId;
        /// <summary>
        /// Gets/Sets UserId
        /// </summary>
        public int UserId
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
        /// Gets/Sets OrgId - as alternate to UserId
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

        private string _comment = "";
        /// <summary>
        /// Gets/Sets Comment
        /// </summary>
        public string Comment
        {
            get
            {
                return this._comment;
            }
            set
            {
                if ( this._comment == value )
                {
                    //Ignore set
                }
                else
                {
                    this._comment = value.Trim();
                    HasChanged = true;
                }
            }
        }

   

        #endregion


        #region External entities
        AppUser _relatedAppUser = null;
        /// <summary>
        /// Get/Set AppUser for group member
        /// </summary>
        public AppUser RelatedAppUser
        {
            get
            {
                return this._relatedAppUser;
            }
            set
            {
                this._relatedAppUser = value;
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
        /// Composite: Gets/Sets group member's FullName
        /// </summary>
        public string FullName
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

        private string _sortName = "";
        /// <summary>
        /// Composite: Gets/Sets group member's SortName
        /// </summary>
        public string SortName
        {
            get
            {
                return this._sortName;
            }
            set
            {
                if ( this._sortName == value )
                {
                    //Ignore set
                }
                else
                {
                    this._sortName = value.Trim();
                    HasChanged = true;
                }
            }
        }


        private string _userEmail = "";
        /// <summary>
        /// Composite: Gets/Sets group member's UserEmail
        /// </summary>
        public string UserEmail
        {
            get
            {
                return this._userEmail;
            }
            set
            {
                if ( this._userEmail == value )
                {
                    //Ignore set
                }
                else
                {
                    this._userEmail = value.Trim();
                    HasChanged = true;
                }
            }
        }
        #endregion
    } // end class 
} // end Namespace 

