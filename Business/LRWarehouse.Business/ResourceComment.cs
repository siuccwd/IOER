using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
    public class ResourceComment
    {
        //private Guid _resourceId;
        //public Guid ResourceId
        //{
        //    get { return this._resourceId; }
        //    set { this._resourceId = value; }
        //}
        private int id;
        /// <summary>
        /// Gets/Sets the BaseBusinessDataEntity's associated ID
        /// </summary>
        public int Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }//
        private int _resourceIntId;
        public int ResourceIntId
        {
            get { return this._resourceIntId; }
            set { this._resourceIntId = value; }
        }

        private string _comment = "";
        public string Comment
        {
            get { return this._comment; }
            set { this._comment = value; }
        }

        private string _commenter = "";
        public string Commenter
        {
            get { return this._commenter; }
            set { this._commenter = value; }
        }

        private string _docId = "";
        public string DocId
        {
            get { return this._docId; }
            set { this._docId = value; }
        }

        private DateTime created;
        /// <summary>
        /// Gets/Sets the Creation Date
        /// </summary>
        public DateTime Created
        {
            get
            {
                return this.created;
            }
            set
            {
                this.created = value;
            }
        }

        private int createdById;
        /// <summary>
        /// Gets/Sets the BaseBusinessDataEntity's Created By Contact ID
        /// </summary>
        public int CreatedById
        {
            get
            {
                return this.createdById;
            }
            set
            {
                this.createdById = value;
            }
        }

        private string createdBy = "";
        /// <summary>
        /// Gets/Sets the Last Update userid - alternate to FK to user - display only? Will this be persited?
        /// </summary>
        public string CreatedBy
        {
            get
            {
                return this.createdBy;
            }
            set
            {
                this.createdBy = value;
            }
        }

			public string CreatedString {
				get { 
					return this.Created.ToShortDateString(); 
				}
			}
    }
}
