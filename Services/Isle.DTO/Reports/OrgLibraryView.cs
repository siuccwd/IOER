using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Isle.DTO.Reports
{
    public class OrgLibraryView
    {
        public int ObjectOrgId { get; set; }
        public int ObjectId { get; set; }
        public string ObjectTitle { get; set; }
        public int NbrViews { get; set; }
        public List<OrgCollectionView> OrgCollectionViews { get; set; }
        public List<OrgResourceView> OrgResourceViews { get; set; }

        public OrgLibraryView()
        {
            this.ObjectTitle = "";
            this.OrgCollectionViews = new List<OrgCollectionView>();
            this.OrgResourceViews = new List<OrgResourceView>();
        }
    }

    public class OrgCollectionView
    {
        public int ObjectOrgId { get; set; }
        public int ObjectId { get; set; }
        public string ObjectTitle { get; set; }
        public int ObjectParentId { get; set; }
        public int NbrViews { get; set; }
        public List<OrgResourceView> OrgResourceViews { get; set; }

        public OrgCollectionView()
        {
            this.ObjectTitle = "";
            OrgResourceViews = new List<OrgResourceView>();
        }
    }

    public class OrgResourceView
    {
        public int ObjectOrgId { get; set; }
        public int ObjectId { get; set; }
        public string ObjectTitle { get; set; }
        public int ObjectParentId { get; set; }
        public int ObjectRootId { get; set; }
        public int NbrViews { get; set; }

        public OrgResourceView()
        {
            this.ObjectTitle = "";
        }
    }
}
