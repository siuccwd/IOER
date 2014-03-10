using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
    [Serializable]
    public class PublishPending : BaseBusinessDataEntity
    {

        private int _resourceIntId;
        public int ResourceId
        {
            get { return this._resourceIntId; }
            set { this._resourceIntId = value; }
        }


        private int _resourceVersionId;
        public int ResourceVersionId
        {
            get { return this._resourceVersionId; }
            set { this._resourceVersionId = value; }
        }

        private string _lRDocId;
        public string DocId
        {
            get { return this._lRDocId; }
            set { this._lRDocId = value; }
        }

        private string _reason = "";
        public string Reason
        {
            get { return this._reason; }
            set { this._reason = value; }
        }

        private bool _isPublished;
        public bool IsPublished
        {
            get { return this._isPublished; }
            set { this._isPublished = value; }
        }

        private string _lrEnvelope = "";
        public string LREnvelope
        {
            get { return this._lrEnvelope; }
            set { this._lrEnvelope = value; }
        }

        private DateTime _publishedDate;
        public DateTime PublishedDate
        {
            get { return this._publishedDate; }
            set { this._publishedDate = value; }
        }
    }
}
