using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
    public class CodeGradeLevel : BaseBusinessDataEntity
    {
        private string _title = "";
        public string Title
        {
            get { return this._title; }
            set { this._title = value; }
        }

        private int _ageLevel;
        public int AgeLevel
        {
            get { return this._ageLevel; }
            set { this._ageLevel = value; }
        }

        private string _description = "";
        public string Description
        {
            get { return this._description; }
            set { this._description = value; }
        }

        private bool _isPathwaysLevel;
        public bool IsPathwaysLevel
        {
            get { return this._isPathwaysLevel; }
            set { this._isPathwaysLevel = value; }
        }

        private string _alignmentUrl = "";
        public string AlignmentUrl
        {
            get { return this._alignmentUrl; }
            set { this._alignmentUrl = value; }
        }

        private int _sortOrder;
        public int SortOrder
        {
            get { return this._sortOrder; }
            set { this._sortOrder = value; }
        }

        private int _warehouseTotal;
        public int WarehouseTotal
        {
            get { return this._warehouseTotal; }
            set { this._warehouseTotal = value; }
        }

        private string _gradeRange = "";
        public string GradeRange
        {
            get { return this._gradeRange; }
            set { this._gradeRange = value; }
        }

        private string _gradeGroup = "";
        public string GradeGroup
        {
            get { return this._gradeGroup; }
            set { this._gradeGroup = value; }
        }

        private bool _isEducationBand;
        public bool IsEducationBand
        {
            get { return this._isEducationBand; }
            set { this._isEducationBand = value; }
        }

        private int _fromAge;
        public int FromAge
        {
            get { return this._fromAge; }
            set { this._fromAge = value; }
        }

        private int _toAge;
        public int ToAge
        {
            get { return this._toAge; }
            set { this._toAge = value; }
        }
    }

    public class CodeGradeLevelCollection : List<CodeGradeLevel>
    {
        // Alias for List<CodeGradeLevel>
    }
}
