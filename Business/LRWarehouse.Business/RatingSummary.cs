using System;
using System.Collections.Generic;
using System.Text;

namespace LRWarehouse.Business
{
    /// <summary>
    /// Represents an object that describes a RatingSummary
    /// </summary>
    [Serializable]
    public class RatingSummary : BaseBusinessDataEntity
    {

        private Guid _resourceId;
        public Guid ResourceId
        {
            get { return this._resourceId; }
            set { this._resourceId = value; }
        }

        private int _ratingTypeId;
        public int RatingTypeId
        {
            get { return this._ratingTypeId; }
            set { this._ratingTypeId = value; }
        }

        private int _ratingCount;
        public int RatingCount
        {
            get { return this._ratingCount; }
            set { this._ratingCount = value; }
        }

        private int _ratingTotal;
        public int RatingTotal
        {
            get { return this._ratingTotal; }
            set { this._ratingTotal = value; }
        }

        private decimal _ratingAverage;
        public decimal RatingAverage
        {
            get { return this._ratingAverage; }
            set { this._ratingAverage = value; }
        }

        private string _ratingType = "";
        public string RatingType
        {
            get { return this._ratingType; }
            set { this._ratingType = value; }
        }

        private string _ratingTypeIdentifier = "";
        public string RatingTypeIdentifier
        {
            get { return this._ratingTypeIdentifier; }
            set { this._ratingTypeIdentifier = value; }
        }

        private string _ratingTypeDescription = "";
        public string RatingTypeDescription
        {
            get { return this._ratingTypeDescription; }
            set { this._ratingTypeDescription = value; }
        }

        private int _resourceIntId;
        public int ResourceIntId
        {
            get { return this._resourceIntId; }
            set { this._resourceIntId = value; }
        }

    }
}
