using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Common
{
    [Serializable]
    public class CodeItem 
    {
        public CodeItem()
        {
            SortOrder = 10;
        }

        public int Id { get; set; }
        /// <summary>
        /// Akias fir Id
        /// </summary>
        public int ItemId
        {
            get { return this.Id; }
            set { this.Id = value; }
        }
        private string _title = "";
        public string Title
        {
            get { return this._title; }
            set { this._title = value; }
        }
        /// <summary>
        /// Alias for title
        /// </summary>
        public string ItemType
        {
            get { return this._title; }
            set { this._title = value; }
        }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public int WarehouseTotal { get; set; }
    }
}

