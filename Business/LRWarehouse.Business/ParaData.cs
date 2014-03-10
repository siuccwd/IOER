using System;
using System.Collections.Generic;
using System.Text;

namespace LRWarehouse.Business
{
    /// <summary>
    /// Represents an object that describes a Resource
    /// </summary>
    [Serializable]
    public class Paradata : BaseBusinessDataEntity
    {

        private string _resourceUrl;
        public string ResourceUrl
        {
            get { return this._resourceUrl; }
            set { this._resourceUrl = value; }
        }

        private int _viewCount;
        public int ViewCount
        {
            get { return this._viewCount; }
            set { this._viewCount = value; }
        }

        private int _favoriteCount;
        public int FavoriteCount
        {
            get { return this._favoriteCount; }
            set { this._favoriteCount = value; }
        }
    }
}
