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

        /// <summary>
        /// Gets/Sets member UserId
        /// </summary>
        public int UserId
        {
          get
          {
              return this.CreatedById;
          }
          set
          {
              if ( this.CreatedById == value )
              {
              //Ignore set
            } else {
                this.CreatedById = value;
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

        public string MemberType{ get; set; }
        public string MemberName { get; set; }
        public string MemberSortName { get; set; }
        #endregion
    }
}
