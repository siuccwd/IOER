using System;
using System.Collections.Generic;
using System.Text;

namespace ILPathways.Business
{
	/// <summary>
	///Represents an object that describes a EmailNoticeJob
	/// </summary>
    public class EmailNoticeJob : BaseBusinessDataEntity
    {
        private string _noticeCode;
        /// <summary>
        /// Gets/sets NoticeCode
        /// </summary>
        public string NoticeCode
        {
            get { return this._noticeCode; }
            set { this._noticeCode = value; }
        }

        private int _sqlId;
        /// <summary>
        /// Gets/sets SqlId
        /// </summary>
        public int SqlId
        {
            get { return this._sqlId; }
            set { this._sqlId = value; }
        }

        private string _status;
        /// <summary>
        /// Gets/sets Status
        /// </summary>
        public string Status
        {
            get { return this._status; }
            set { this._status = value; }
        }

        private string _sql;
        /// <summary>
        /// Gets/sets Sql
        /// </summary>
        public string Sql
        {
            get { return this._sql; }
            set { this._sql = value; }
        }

        private string _jobClass;
        /// <summary>
        /// Gets/sets JobClass
        /// </summary>
        public string JobClass
        {
            get { return this._jobClass; }
            set { this._jobClass = value; }
        }

        private string _subject;
        /// <summary>
        /// Gets/sets Subject
        /// </summary>
        public string Subject
        {
            get { return this._subject; }
            set { this._subject = value; }
        }

    }
}
