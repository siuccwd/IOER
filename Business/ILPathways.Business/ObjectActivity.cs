using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business 
{
    /// <summary>
    /// Represents a site activity object
    /// </summary>
    [Serializable]
    public class ObjectActivity : BaseBusinessDataEntity
    {
        //When	ActorId	Actor	Activity	ObjectType	TargetObjectId	TargetUrl	TargetImageUrl
        public ObjectActivity() 
        {

        }
        /// <summary>
        /// Activity day - no hours, minutes. Format: yyyy-mm-dd
        /// </summary>
        public DateTime ActivityDay  { get; set; }

        public int ActorId { get; set; }
        public string Actor { get; set; }
        public int ActorTypeId { get; set; }
        public string ActorUrl { get; set; }
        public string ActorImageUrl { get; set; }
        /// <summary>
        /// If true, the actor is the current user - pertinent in my network mode only
        /// </summary>
        public bool IsMyAction { get; set; }

        public string Action { get; set; }
        /// <summary>
        /// Formatted activity message
        /// </summary>
        public string Activity { get; set; }

        //
        public int ObjectId { get; set; }
        public string ObjectType { get; set; }
        public string ObjectTitle { get; set; }
        public string ObjectText { get; set; }
        public string ObjectUrl { get; set; }
        public string ObjectImageUrl { get; set; }
        /// <summary>
        /// True if the user has the object - typically used for Likes, will be true if user alreaady has an entry
        /// </summary>
        public bool HasObject { get; set; }
        /// <summary>
        /// Total of object where relevent (ex. total likes, comments)
        /// </summary>
        public int ObjectCount{ get; set; }
        /// <summary>
        /// Used for dislikes only at this time
        /// </summary>
        public int ObjectCount2 { get; set; }
        //public int ObjectTypeId { get; set; }

        public int TargetObjectId { get; set; }
        public string TargetType { get; set; }
        public string TargetTitle { get; set; }
        public string TargetText { get; set; }
        public string TargetUrl { get; set; }
        public string TargetImageUrl { get; set; }


    }
}
