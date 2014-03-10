using System;
using System.Collections.Generic;
using System.Text;

namespace LRWarehouse.Business
{
    /// <summary>
    /// Represents an object that describes a RatingType
    /// </summary>
    [Serializable]
    public class RatingType : BaseBusinessDataEntity
    {

        private string _type = "";
        public string Type
        {
            get { return this._type; }
            set { this._type = value; }
        }

        private string _identifier = "";
        public string Identifier
        {
            get { return this._identifier; }
            set { this._identifier = value; }
        }

        private string _description = "";
        public string Description
        {
            get { return this._description; }
            set { this._description = value; }
        }

    }
}
