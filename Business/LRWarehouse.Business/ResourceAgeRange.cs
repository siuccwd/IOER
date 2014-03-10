using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
    public class ResourceAgeRange : BaseBusinessDataEntity
    {
        private int _resourceIntId;
        public int ResourceIntId
        {
            get { return this._resourceIntId; }
            set { this._resourceIntId = value; }
        }

        private int _fromAge;
        public int FromAge
        {
            get { return this._fromAge; }
            set { this._fromAge = value; }
        }

        private int _toAge;
        public int ToAge
        {
            get { return this._toAge; }
            set { this._toAge = value; }
        }

        public int AgeSpan
        {
            get { return Math.Abs(ToAge - FromAge) + 1; }
        }

        private string _originalLevel = "";
        public string OriginalValue
        {
            get { return this._originalLevel; }
            set { this._originalLevel = value; }
        }
    }
}
