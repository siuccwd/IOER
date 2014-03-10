using System;
using System.Collections.Generic;
using System.Text;

namespace LRWarehouse.Business
{
    public class Rule
    {
        private int _id;
        public int Id
        {
            get { return this._id; }
            set { this._id = value; }
        }

        private int _propertyTypeId;
        public int PropertyTypeId
        {
            get { return this._propertyTypeId; }
            set { this._propertyTypeId = value; }
        }

        private string _propertyType;
        public string PropertyType
        {
            get { return this._propertyType; }
            set { this._propertyType = value; }
        }

        private string _originalValue;
        public string OriginalValue
        {
            get { return this._originalValue; }
            set { this._originalValue = value; }
        }

        private bool _isRegex;
        public bool IsRegex
        {
            get { return this._isRegex; }
            set { this._isRegex = value; }
        }

        private bool _isCaseSensitive;
        public bool IsCaseSensitive
        {
            get { return this._isCaseSensitive; }
            set { this._isCaseSensitive = value; }
        }

        private bool _importWithoutTranslation;
        public bool ImportWithoutTranslation
        {
            get { return this._importWithoutTranslation; }
            set { this._importWithoutTranslation = value; }
        }

        private bool _doNotImport;
        public bool DoNotImport
        {
            get { return this._doNotImport; }
            set { this._doNotImport = value; }
        }

        private string _mappedValue;
        public string MappedValue
        {
            get { return this._mappedValue; }
            set { this._mappedValue = value; }
        }

        private int _sequence;
        public int Sequence
        {
            get { return this._sequence; }
            set { this._sequence = value; }
        }

        private bool _isActive;
        public bool IsActive
        {
            get { return this._isActive; }
            set { this._isActive = value; }
        }

        private DateTime _created;
        public DateTime Created
        {
            get { return this._created; }
            set { this._created = value; }
        }

        private string _createdBy;
        public string CreatedBy
        {
            get { return this._createdBy; }
            set { this._createdBy = value; }
        }

        private DateTime _lastUpdated;
        public DateTime LastUpdated
        {
            get { return this._lastUpdated; }
            set { this._lastUpdated = value; }
        }

        private string _lastUpdatedBy;
        public string LastUpdatedBy
        {
            get { return this._lastUpdatedBy; }
            set { this._lastUpdatedBy = value; }
        }

        private int _mappedId;
        public int MappedId
        {
            get { return this._mappedId; }
            set { this._mappedId = value; }
        }
    }
}
