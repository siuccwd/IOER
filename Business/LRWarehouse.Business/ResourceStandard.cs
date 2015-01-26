using System;
using System.Collections.Generic;
using System.Text;

namespace LRWarehouse.Business
{
    [Serializable]
    public class ResourceStandard 
    {
        //: BaseBusinessDataEntity
        public int Id { get; set; }
        public System.DateTime Created { get; set; }
        public int AlignedById { get; set; }

        private int _createdById;
        public int CreatedById
        {
            get
            {
                return _createdById;
            }
            set
            {
                _createdById = value;
            }
        }

        private int _resourceIntId;
        public int ResourceIntId
        {
            get { return this._resourceIntId; }
            set { this._resourceIntId = value; }
        }

        private int _standardId;
        public int StandardId
        {
            get { return this._standardId; }
            set { this._standardId = value; }
        }
        private string _standardDescription;
        public string StandardDescription
        {
            get { return this._standardDescription; }
            set { this._standardDescription = value; }
        }

        private string _standardNotationCode;
        public string StandardNotationCode
        {
            get { return this._standardNotationCode; }
            set { this._standardNotationCode = value; }
        }
        /*
        private string _originalValue;
        public string OriginalValue
        {
            get { return this._originalValue; }
            set { this._originalValue = value; }
        }*/

        /*public int CodeId 
        {
            get { return this._standardId; }
            set { this._standardId = value; }
        }
        private string _mappedValue;
        public string MappedValue
        {
            get { return this._mappedValue; }
            set { this._mappedValue = value; }
        }*/
        private string _standardUrl;
        public string StandardUrl
        {
            get { return this._standardUrl; }
            set { this._standardUrl = value; }
        }

        private int _alignmentTypeCodeId;
        public int AlignmentTypeCodeId
        {
            get { return this._alignmentTypeCodeId; }
            set { this._alignmentTypeCodeId = value; }
        }

        private string _alignmentTypeValue = "";
        public string AlignmentTypeValue
        {
            get { return this._alignmentTypeValue; }
            set { this._alignmentTypeValue = value; }
        }

        private int _alignmentDegreeId;
        public int AlignmentDegreeId
        {
            get { return this._alignmentDegreeId; }
            set { this._alignmentDegreeId = value; }
        }

        private string _alignmentDegree = "";
        public string AlignmentDegree
        {
            get { return this._alignmentDegree; }
            set { this._alignmentDegree = value; }
        }


    }


}
