using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
    public class CareerCluster : BaseBusinessDataEntity
    {
        #region === Attributes ===

        private Guid _resourceId;
        public Guid ResourceId
        {
            get { return this._resourceId; }
            set { this._resourceId = value; }
        }

        private int _clusterId;
        public int Clusterid
        {
            get { return this._clusterId; }
            set { this._clusterId = value; }
        }

        private string _ilPathwayName = "";
        public string IlPathwayName
        {
            get { return this._ilPathwayName; }
            set { this._ilPathwayName = value; }
        }

        private string _clusterName = "";
        public string ClusterName
        {
            get { return this._clusterName; }
            set { this._clusterName = value; }
        }

        private int _resourceIntId;
        public int ResourceIntId
        {
            get { return this._resourceIntId; }
            set { this._resourceIntId = value; }
        }

        #endregion

        public CareerCluster()
        {
        }
    }

    /// <summary>
    /// CareerClusterCollection is an alias of a List of CareerCluster objects
    /// </summary>
    public class CareerClusterCollection : List<CareerCluster>
    {
    }
}
