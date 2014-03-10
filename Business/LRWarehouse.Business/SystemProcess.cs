using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
    public class SystemProcess : BaseBusinessDataEntity
    {
        public SystemProcess()
        {
            LastRunDate = DateTime.Now;
        }

        private string _code = "";
        public string Code
        {
            get { return this._code; }
            set { this._code = value; }
        }

        private string _title = "";
        public string Title
        {
            get { return this._title; }
            set { this._title = value; }
        }

        private string _description = "";
        public string Description
        {
            get { return this._description; }
            set { this._description = value; }
        }

        private DateTime _lastRunDate;
        public DateTime LastRunDate
        {
            get { return this._lastRunDate; }
            set { this._lastRunDate = value; }
        }

        private string _stringParameter = "";
        public string StringParameter
        {
            get { return this._stringParameter; }
            set { this._stringParameter = value; }
        }

        private int _intParameter;
        public int IntParameter
        {
            get { return this._intParameter; }
            set { this._intParameter = value; }
        }
    }
}
