using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
    public class ResourceLikeSummary : BaseBusinessDataEntity
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

        private int _likeCount;
        public int LikeCount
        {
            get { return this._likeCount; }
            set { this._likeCount = value; }
        }

        private int _dislikeCount;
        public int DislikeCount
        {
            get { return this._dislikeCount; }
            set { this._dislikeCount = value; }
        }

        //private DateTime _lastUpdated;
        //public DateTime LastUpdated
        //{
        //    get { return this._lastUpdated; }
        //    set { this._lastUpdated = value; }
        //}

        /// <summary>
        /// Get HasRating- will be true if current user has aleady rated the resource
        /// </summary>
        private bool _HasRating = false;
        public bool HasRating
        {
            get { return this._HasRating; }
            set { this._HasRating = value; }
        }

        private bool _youDislikeThis;
        public bool YouDislikeThis
        {
            get { return this._youDislikeThis; }
            set { this._youDislikeThis = value; }
        }

        private bool _youLikeThis;
        public bool YouLikeThis
        {
            get { return this._youLikeThis; }
            set { this._youLikeThis = value; }
        }
    }
}
