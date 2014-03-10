using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
    public class BaseParadataSummary : BaseParadataEntity
    {
        private string _dateRange = "";
        public string DateRange
        {
            get { return this._dateRange; }
            set { this._dateRange = value; }
        }

        private decimal _value;
        public decimal Value
        {
            get { return this._value; }
            set { this._value = value; }
        }

    }
}
