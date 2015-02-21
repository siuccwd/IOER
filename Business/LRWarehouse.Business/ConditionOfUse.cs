using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
    
    [Serializable]
    public class ConditionOfUse 
    {
        public ConditionOfUse()
        {
            SortOrder = 10;
        }

        public int Id { get; set; }
        private string _title = "";
        public string Title
        {
            get { return this._title; }
            set { this._title = value; }
        }
        /// <summary>
        /// Summary is basically a short title that is easily understandable
        /// </summary>
        public string Summary { get; set; }
        public string Description { get; set; }

        public bool IsActive { get; set; }
        public string Url { get; set; }
        public string IconUrl { get; set; }
        public string MiniIconUrl { get; set; }

        public int SortOrder { get; set; }
        public int WarehouseTotal { get; set; }
    }
}
