using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
    public class ResourceChildItem : BaseBusinessDataEntity
    {

        #region Properties
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

        private int _codeId;
        public int CodeId
        {
            get { return this._codeId; }
            set { this._codeId = value; }
        }


        private string _lrValue = "";
        public string OriginalValue
        {
            get { return this._lrValue; }
            set { this._lrValue = value; }
        }

        private string _mappedValue;
        public string MappedValue
        {
            get { return this._mappedValue; }
            set { this._mappedValue = value; }
        }
        #endregion
    }
}
