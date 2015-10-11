using System;
using System.Collections.Generic;
using System.Text;

namespace LearningRegistryCache2.App_Code.Classes
{
    public class Paradata
    {
        private Guid _rowId;
        public Guid RowId
        {
            get { return this._rowId; }
            set { this._rowId = value; }
        }

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
