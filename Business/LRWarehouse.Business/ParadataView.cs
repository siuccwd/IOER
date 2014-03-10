using System;
using System.Collections.Generic;
using System.Text;

namespace LRWarehouse.Business
{
    /// <summary>
    /// Represents an object that describes a ParadataView
    /// </summary>
    [Serializable]
    public class ParadataView : BaseBusinessDataEntity
    {
        private Guid _paradataRowId;
        public Guid ParadataRowId
        {
            get { return this._paradataRowId; }
            set { this._paradataRowId = value; }
        }

        private int _count;
        public int Count
        {
            get { return this._count; }
            set { this._count = value; }
        }
    }
}
