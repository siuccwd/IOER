using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Isle.DTO;

namespace IoerContentBusinessEntities
{
    public class ActivityManager
    {
        public static List<HierarchyActivityRecord> ActivityTotals_Accounts( int objectId, DateTime startDate, DateTime endDate )
        {
            List<HierarchyActivityRecord> list = new List<HierarchyActivityRecord>();
            
            using ( var context = new IsleContentContext() )
            {
                List<Library_SectionResourceSummary> items = context.Library_SectionResourceSummary
                                .Where( s => s.ResourceCreated >= startDate && s.ResourceCreated < endDate )
                                .OrderByDescending( s => s.DateAddedToCollection )
                                .ToList();

                if ( items.Count > 0 )
                {
                    foreach ( Library_SectionResourceSummary item in items )
                    {
                        

                    }
                }
            }
            return list;
        }
    }
}
