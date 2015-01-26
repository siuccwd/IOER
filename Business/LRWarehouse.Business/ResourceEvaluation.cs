using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
    public class ResourceEvaluation : BaseBusinessDataEntity
    {
        private int _resourceIntId;
        public int ResourceIntId
        {
            get { return this._resourceIntId; }
            set { this._resourceIntId = value; }
        }
        public int ResourceStandardId { get; set; }
        public int Score { get; set; }

        public int StandardId { get; set; }

        private int _rubricId;
        public int RubricId
        {
            get { return this._rubricId; }
            set { this._rubricId = value; }
        }

        private decimal _value;
        public decimal Value
        {
            get { return this._value; }
            set { this._value = value; }
        }

        private int _scaleMin;
        public int ScaleMin
        {
            get { return this._scaleMin; }
            set { this._scaleMin = value; }
        }

        private int _scaleMax;
        public int ScaleMax
        {
            get { return this._scaleMax; }
            set { this._scaleMax = value; }
        }

        private string _criteriaInfo = "";
        public string CriteriaInfo
        {
            get { return this._criteriaInfo; }
            set { this._criteriaInfo = value; }
        }
    }
}
