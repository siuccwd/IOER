using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Isle.DTO
{
    public class HierarchyActivityRecord
    {
        public HierarchyActivityRecord()
        {
            Activity = new ActivityCount();
            ChildrenActivity = new List<ActivityCount>();
        }
        /// <summary>
        /// Activity for the parent item
        /// </summary>
        public ActivityCount Activity { get; set; }      
        /// <summary>
        /// Activity for child items (e.g. collections, nodes, etc)
        /// </summary>
         
        public List<ActivityCount> ChildrenActivity { get; set; }  
        
    }

    //This is the important one
    public class ActivityCount
    {
        public ActivityCount()
        {
            Activities = new Dictionary<string, List<int>>();
        }
        /// <summary>
        /// Id of the object
        /// </summary>
        public int Id { get; set; }                                             
        /// <summary>
        /// Title of the object
        /// </summary>
        public string Title { get; set; }       
        
        /// <summary>
        /// activity details
        /// </summary>
        public Dictionary<string, List<int>> Activities { get; set; }           
    }

}
