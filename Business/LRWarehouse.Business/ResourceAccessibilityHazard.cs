using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
    public class ResourceAccessibilityHazard : ResourceChildItem
    {
        private int _antonymId;
        public int AntonymId
        {
            get { return this._antonymId; }
            set { this._antonymId = value; }
        }
    }
}
