using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
    public class BlacklistedHost : BaseBusinessDataEntity
    {
        private string _hostname = "";
        public string Hostname
        {
            get { return this._hostname; }
            set { this._hostname = value; }
        }

        private string _recordSource = "";
        public string RecordSource
        {
            get { return this._recordSource; }
            set { this._recordSource = value; }
        }
    }
}
