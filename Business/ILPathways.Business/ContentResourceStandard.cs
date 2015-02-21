using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    [Serializable]
    public class ContentResourceStandard
    {
        public ContentResourceStandard()
        {
            ContentItemIds = new List<int>();
        }

        public int ContentId { get; set; }

        public int ResourceStandardId { get; set; }
        public int StandardRecordId
        {
            get
            {
                return ResourceStandardId;
            }
            set { ResourceStandardId = value; }
        }
        //resource values
        public int ResourceIntId { get; set; }
        //public int ResourceVersionIntId { get; set; }
        public string ResourceTitle { get; set; }
        public string ResourceSortTitle { get; set; }

        public int StandardId { get; set; }
        public string StandardUrl { get; set; }
        public string NotationCode { get; set; }
        public string Description { get; set; }
        public int AlignedById { get; set; }
        /// <summary>
        /// an alias to ensure works with new code that uses CreatedById instead of AlignedById
        /// </summary>
        public int CreatedById
        {
            get
            {
                return AlignedById;
            }

        }
        public int AlignmentTypeCodeId { get; set; }
        public string AlignmentType { get; set; }

        public int AlignmentDegreeId { get; set; }
        public string AlignmentDegree { get; set; }

        public System.DateTime Created { get; set; }
  
        /// <summary>
        /// when gathering unique standards for a list of items, store unique ids that could be used to display all content items with the standard
        /// </summary>
        public List<int> ContentItemIds { get; set; }

        #region === methods for comparing records
        /// <summary>
        /// Checks if the provided object is equal to the current ContentResourceStandard
        /// </summary>
        /// <param name="obj">Object to compare to the current ContentResourceStandard</param>
        /// <returns>True if equal, false if not</returns>
        public override bool Equals( object obj )
        {
            // Try to cast the object to compare to to be a Person
            var standard = obj as ContentResourceStandard;

            return Equals( standard );
        }

        /// <summary>
        /// Returns an identifier for this instance
        /// </summary>
        public override int GetHashCode()
        {
            return NotationCode.GetHashCode();
        }

        /// <summary>
        /// Checks if the provided ContentResourceStandard is equal to the current ContentResourceStandard
        /// </summary>
        /// <param name="standardToCompareTo">ContentResourceStandard to compare to the ContentResourceStandard person</param>
        /// <returns>True if equal, false if not</returns>
        public bool Equals( ContentResourceStandard standardToCompareTo )
        {
            // Check if person is being compared to a non person. In that case always return false.
            if ( standardToCompareTo == null ) 
                return false;

            // If the person to compare to does not have a Name assigned yet, we can't define if it's the same. Return false.
            if ( string.IsNullOrEmpty( standardToCompareTo.NotationCode ) )
                return false;

            // Check if both person objects contain the same Name. In that case they're assumed equal.
            return NotationCode.Equals( standardToCompareTo.NotationCode );
        }
        #endregion
    }

}
