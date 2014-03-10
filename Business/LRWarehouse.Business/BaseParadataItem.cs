using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
    public class BaseParadataItem : BaseParadataEntity
    {
        private DateTime _date = new DateTime();
        public DateTime Date
        {
            get { return this._date; }
            set { this._date = value; }
        }

        private string _comment = "";
        public string Comment
        {
            get { return this._comment; }
            set { this._comment = value; }
        }
    }
}
