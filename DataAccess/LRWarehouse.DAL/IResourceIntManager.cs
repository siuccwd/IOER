using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace LRWarehouse.DAL
{

    public interface IResourceIntManager
    {
        bool ApplyChanges( int pResourceId, int pCreatedbyId, string pNewSelectedItems, string pUnselectedItems );
        bool Insert( int pResourceId, int pCodeId, int pCreatedbyId, ref string statusMessage );
        DataSet SelectedCodes( int pResourceId );
    }
}
