using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
    /// <summary>
    /// This is an alias for List<ResourceSubject>
    /// </summary> 
    public class ResourceSubjectCollection : List<ResourceSubject>
    {
        public ResourceSubjectCollection()
        {
        }

        public ResourceSubjectCollection(int capacity)
        {
            this.Capacity = capacity;
        }
    }
}
