using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    ///<summary>
    ///Represents an object that describes a LibraryMembership
    ///</summary>
    [Serializable]
    public class LibraryMember : BaseBusinessDataEntity
    {
        public static int LIBRARY_MEMBER_TYPE_ID_PENDING = 0;
        public static int LIBRARY_MEMBER_TYPE_ID_READER = 1;
        public static int LIBRARY_MEMBER_TYPE_ID_CONTRIBUTOR = 2;
        public static int LIBRARY_MEMBER_TYPE_ID_EDITOR = 3;
        public static int LIBRARY_MEMBER_TYPE_ID_ADMIN = 4;

        public LibraryMember() 
        {
            ParentLibrary = new Library();
            MemberName = "";
            MemberSortName = "";
            MemberType = "None";
            OrgMemberType = "";
        }


        #region Properties created from dictionary for Library or Section member

        private int _parentId;
        /// <summary>
        /// Gets/Sets ParentId - LibraryId or SectionId
        /// </summary>
        public int ParentId
        {
          get
          {
              return this._parentId;
          }
          set
          {
              if ( this._parentId == value )
              {
              //Ignore set
            } else {
                this._parentId = value;
              HasChanged = true;
            }
          }
        }
        public string Library { get; set; }

        private int _userId;
        /// <summary>
        /// Gets/Sets member UserId
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
            } else {
                this._userId = value;
              HasChanged = true;
            }
          }
        }

        private int _notificationTypeId;
        /// <summary>
        /// Gets/Sets SubscriptionTypeId ==> not implemented yet, still using separate subscription
        /// </summary>
        public int SubscriptionTypeId
        {
            get
            {
                return this._notificationTypeId;
            }
            set
            {
                if ( this._notificationTypeId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._notificationTypeId = value;
                    HasChanged = true;
                }
            }
        }


        private int _memberTypeId;
        /// <summary>
        /// Gets/Sets MemberTypeId
        /// - reader, contributor, editor, admin
        /// </summary>
        public int MemberTypeId
        {
            get
            {
                return this._memberTypeId;
            }
            set
            {
                if ( this._memberTypeId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._memberTypeId = value;
                    HasChanged = true;
                }
            }
        }

        #endregion

        #region Parent/others
        public Library ParentLibrary { get; set; }

        private string _memberType = "";
        public string MemberType
        {
            get
            {
                if ( _memberType != null && _memberType.Length > 0 )
                    return this._memberType;
                else
                {
                    if ( MemberTypeId == LIBRARY_MEMBER_TYPE_ID_READER ) _memberType = "Reader";
                    else if ( MemberTypeId == LIBRARY_MEMBER_TYPE_ID_CONTRIBUTOR ) _memberType = "Contributor";
                    else if ( MemberTypeId == LIBRARY_MEMBER_TYPE_ID_EDITOR ) _memberType = "Editor";
                    else if ( MemberTypeId == LIBRARY_MEMBER_TYPE_ID_ADMIN ) _memberType = "Administrator";
                    else if ( MemberTypeId == LIBRARY_MEMBER_TYPE_ID_PENDING ) _memberType = "Pending";
                    else _memberType = "Unknown";

                    return this._memberType;
                }
            }
            set
            {
                if ( this._memberType != value )
                {
                    this._memberType = value;
                    HasChanged = true;
                }
            }
        }
        /// <summary>
        /// Set member type. Usually called after db read that doesn't include it.
        /// </summary>
        public void SetMemberType()
        {
            this._memberType = "";

            if ( MemberTypeId == 1 ) _memberType = "Reader";
            else if ( MemberTypeId == 2 ) _memberType = "Contributor";
            else if ( MemberTypeId == 3 ) _memberType = "Editor";
            else if ( MemberTypeId == 4 ) _memberType = "Administrator";
            else if ( MemberTypeId == 0 ) _memberType = "Pending";
            else _memberType = "Unknown";
        }

        public AppUser Member { get; set; }

        public string MemberName { get; set; }
        public string MemberEmail { get; set; }
        public string MemberSortName { get; set; }
        public string MemberHomeUrl { get; set; }
        public string MemberImageUrl { get; set; }
        public string Organization { get; set; }
        public int OrganizationId { get; set; }

        /// <summary>
        /// Is an org mbr - true if the current user is a member of the organization
        /// </summary>
        public bool IsAnOrgMbr { get; set; }
        public string OrgMemberType { get; set; }
        public int OrgMemberTypeId { get; set; }
        #endregion
    }
}
