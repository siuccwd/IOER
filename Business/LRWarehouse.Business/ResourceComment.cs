using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
    public class ResourceComment : BaseBusinessDataEntity
    {
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

        private string _comment = "";
        public string Comment
        {
            get { return this._comment; }
            set { this._comment = value; }
        }

        private string _commenter = "";
        public string Commenter
        {
            get { return this._commenter; }
            set { this._commenter = value; }
        }

        private string _docId = "";
        public string DocId
        {
            get { return this._docId; }
            set { this._docId = value; }
        }
    }
}
