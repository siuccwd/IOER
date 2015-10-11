using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinkChecker2.App_Code.BusObj
{
    public class ResourceRedirect
    {
        private int _resourceIntId;
        public int ResourceIntId
        {
            get { return this._resourceIntId; }
            set { this._resourceIntId = value; }
        }

        private string _oldUrl = "";
        public string OldUrl
        {
            get { return this._oldUrl; }
            set { this._oldUrl = value; }
        }

        private string _newUrl = "";
        public string NewUrl
        {
            get { return this._newUrl; }
            set { this._newUrl = value; }
        }

        private DateTime _created;
        public DateTime Created
        {
            get { return this._created; }
            set { this._created = value; }
        }
    }
}
