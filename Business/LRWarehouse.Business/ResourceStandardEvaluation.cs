using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
    [Serializable]
    public class ResourceStandardEvaluation : BaseBusinessDataEntity
    {
        //parent resource.standard
        public int ResourceStandardId { get; set; }
      
        private int _rubricId;
        public int RubricId
        {
            get { return this._rubricId; }
            set { this._rubricId = value; }
        }

        private int _value;
        public int Score
        {
            get { return this._value; }
            set { this._value = value; }
        }

        //private int _scaleMin;
        //public int ScaleMin
        //{
        //    get { return this._scaleMin; }
        //    set { this._scaleMin = value; }
        //}

        //private int _scaleMax;
        //public int ScaleMax
        //{
        //    get { return this._scaleMax; }
        //    set { this._scaleMax = value; }
        //}

        //private string _criteriaInfo = "";
        //public string CriteriaInfo
        //{
        //    get { return this._criteriaInfo; }
        //    set { this._criteriaInfo = value; }
        //}
    }
}
