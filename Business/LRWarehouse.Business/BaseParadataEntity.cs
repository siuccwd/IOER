using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
    public class BaseParadataEntity : BaseBusinessDataEntity
    {
        private string _actordescription = "";
        public string ActorDescription
        {
            get { return this._actordescription; }
            set { this._actordescription = value; }
        }

        private string _actorUserType = "";
        public string ActorUserType
        {
            get { return this._actorUserType; }
            set { this._actorUserType = value; }
        }

        private string _resourceUrl = "";
        public string ResourceUrl
        {
            get { return this._resourceUrl; }
            set { this._resourceUrl = value; }
        }

        private string _contextId = "";
        public string ContextId
        {
            get { return this._contextId; }
            set { this._contextId = value; }
        }

        private string _relatedObjectType = "";
        public string RelatedObjectType
        {
            get { return this._relatedObjectType; }
            set { this._relatedObjectType = value; }
        }

        private string _relatedObjectId = "";
        public string RelatedObjectId
        {
            get { return this._relatedObjectId; }
            set { this._relatedObjectId = value; }
        }

        private string _relatedObjectContent = "";
        public string RelatedObjectContent
        {
            get { return this._relatedObjectContent; }
            set { this._relatedObjectContent = value; }
        }

    }
}
