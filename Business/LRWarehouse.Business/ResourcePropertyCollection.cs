using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
    public class ResourcePropertyCollection : List<ResourceProperty>
    {
        public ResourcePropertyCollection()
        {
        }

        public ResourcePropertyCollection(int capacity)
        {
            this.Capacity = capacity;
        }

        public ResourceProperty Search(string type, string stringToFind)
        {
            foreach (ResourceProperty rp in this)
            {
                if ((rp.Name == type || type == "") && rp.Value == stringToFind)
                {
                    return rp;
                }
            } //foreach
            //not found
            return null;
        }

        public ResourcePropertyCollection FindAllNew()
        {
            ResourcePropertyCollection collection = new ResourcePropertyCollection(this.Count);
            Guid defaultGuid = new Guid("00000000-0000-0000-0000-000000000000");

            foreach (ResourceProperty entity in this)
            {
                if (entity.RowId == null || entity.RowId == defaultGuid)
                {
                    collection.Add(entity);
                }
            }

            return collection;
        }

    }
}
