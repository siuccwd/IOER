using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace LRWarehouse.DAL
{
    public interface IResourceManager
    {
        bool ApplyChanges( string pResourceId, int pCreatedById, string pNewSelectedItems, string pUnselectedItems );
        bool Insert( string pResourceId, int pCodeId, int pCreatedById, ref string statusMessage );
        DataSet SelectedCodes( string pResourceId );
    }
}
