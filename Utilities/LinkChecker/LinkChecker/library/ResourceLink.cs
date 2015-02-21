using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinkChecker.library
{
    public class ResourceLink
    {
        private int _resourceIntId;
        public int ResourceIntId
        {
            get { return this._resourceIntId; }
            set { this._resourceIntId = value; }
        }

        private DateTime _lastCheckDate;
        public DateTime LastCheckDate
        {
            get { return this._lastCheckDate; }
            set { this._lastCheckDate = value; }
        }

        private string _hostName = "";
        public string HostName
        {
            get { return this._hostName; }
            set { this._hostName = value; }
        }

        private bool _isDeleted;
        public bool IsDeleted
        {
            get { return this._isDeleted; }
            set { this._isDeleted = value; }
        }

        private int _nbrDnsErrors;
        public int NbrDnsErrors
        {
            get { return this._nbrDnsErrors; }
            set { this._nbrDnsErrors = value; }
        }

        private int _nbrTimeouts;
        public int NbrTimeouts
        {
            get { return this._nbrTimeouts; }
            set { this._nbrTimeouts = value; }
        }

        private int _nbrInternalServerErrors;
        public int NbrInternalServerErrors
        {
            get { return this._nbrInternalServerErrors; }
            set { this._nbrInternalServerErrors = value; }
        }

        private int _nbrUnableToConnect;
        public int NbrUnableToConnect
        {
            get { return this._nbrUnableToConnect; }
            set { this._nbrUnableToConnect = value; }
        }

        private string _resourceUrl = "";
        public string ResourceUrl
        {
            get { return this._resourceUrl; }
            set { this._resourceUrl = value; }
        }
    }
}
