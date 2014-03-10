using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    public class ObjectSubscription : BaseBusinessDataEntity
    {
        public ObjectSubscription() {}

        private int _parentId;
        /// <summary>
        /// Gets/Sets ParentId ==> actually LibraryId or SectionId
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
                }
                else
                {
                    this._parentId = value;
                    HasChanged = true;
                }
            }
        }

        public string SubscriptionCategory { get; set; }

        /// <summary>
        /// Gets/Sets UserId (CreatedById)
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
                }
                else
                {
                    this.CreatedById = value;
                    HasChanged = true;
                }
            }
        }

        private int _notificationTypeId;
        /// <summary>
        /// Gets/Sets SubscriptionTypeId
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


        
    }
}
