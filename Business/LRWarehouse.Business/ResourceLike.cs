using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
    public class ResourceLike : BaseBusinessDataEntity
    {
        private Guid _resourceId;
        public Guid ResourceId
        {
            get { return this._resourceId; }
            set { this._resourceId = value; }
        }

        private int _resourceIntId;
        public int ResourceIntId
        {
            get { return this._resourceIntId; }
            set { this._resourceIntId = value; }
        }

        private bool _isLike;
        public bool IsLike
        {
            get { return this._isLike; }
            set { this._isLike = value; }
        }


        //private int _createdById;
        //public int CreatedById
        //{
        //    get { return this._createdById; }
        //    set { this._createdById = value; }
        //}
    }
}
