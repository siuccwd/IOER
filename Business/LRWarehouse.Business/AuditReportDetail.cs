using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
    public class AuditReportDetail : BaseBusinessDataEntity
    {
        #region === Attributes ===
        private int _reportId;
        public int ReportId
        {
            get { return this._reportId; }
            set { this._reportId = value; }
        }

        private string _fileName = "";
        public string FileName
        {
            get { return this._fileName; }
            set { this._fileName = value; }
        }

        private string _docId = "";
        public string DocId
        {
            get { return this._docId; }
            set { this._docId = value; }
        }

        private string _uri = "";
        public string Uri
        {
            get { return this._uri; }
            set { this._uri = value; }
        }

        private string _messageType = "";
        public string MessageType
        {
            get { return this._messageType; }
            set { this._messageType = value; }
        }

        private string _messageRouting = "";
        public string MessageRouting
        {
            get { return this._messageRouting; }
            set { this._messageRouting = value; }
        }

        #endregion

        public AuditReportDetail()
        {
        }
    }

    public static class ErrorType
    {
        public const string Error = "E";
        public const string Warning = "W";
    }

    public static class ErrorRouting
    {
        public const string Technical = "T";
        public const string Program = "P";
    }

}
