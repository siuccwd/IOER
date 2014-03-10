using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
    public class ResourceSubject : BaseBusinessDataEntity
    {
        #region === Properties ===

        private Guid _resourceId;
        public Guid ResourceId
        {
            get { return this._resourceId; }
            set { this._resourceId = value; }
        }

        private string _subject = "";
        public string Subject
        {
            get { return this._subject; }
            set { this._subject = value; }
        }

        private string _subjectCsv = "";
        public string SubjectCsv
        {
            get { return this._subjectCsv; }
            set { this._subjectCsv = value; }
        }

        private int _resourceIntId;
        public int ResourceIntId
        {
            get { return this._resourceIntId; }
            set { this._resourceIntId = value; }
        }

        #endregion
    }
}
