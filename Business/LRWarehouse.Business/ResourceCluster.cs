using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
    public class ResourceCluster : BaseBusinessDataEntity
    {
        //
        //private Guid _resourceId;
        //public Guid ResourceId
        //{
        //    get { return this._resourceId; }
        //    set { this._resourceId = value; }
        //}

        
        private int _resourceIntId;
        public int ResourceIntId
        {
            get { return this._resourceIntId; }
            set { this._resourceIntId = value; }
        }
        private int _clusterId;
        public int ClusterId
        {
            get { return this._clusterId; }
            set { this._clusterId = value; }
        }
        private string _title;
        public string Title
        {
            get { return this._title; }
            set { this._title = value; }
        }
    }
}
