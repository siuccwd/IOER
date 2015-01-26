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
        private string _title = "";
        public string Title
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

