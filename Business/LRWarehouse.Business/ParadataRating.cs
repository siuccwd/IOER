using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
    public class ParadataRating : BaseParadataSummary
    {
        private int _sampleSize;
        public int SampleSize
        {
            get { return this._sampleSize; }
            set { this._sampleSize = value; }
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

    }
}
