using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    [Serializable]
    public class ObjectLike : BaseBusinessDataEntity
    {
        public ObjectLike()
        {
        }

        public int ParentId { get; set; }

        /// <summary>
        /// HasLikeEntry - false if no entry, true if a like/dislike was found
        /// </summary>
        public bool HasLikeEntry { get; set; }

        /// <summary>
        /// IsLIke - true if liked, false if disliked
        /// </summary>
        public bool IsLike { get; set; }
    }
}
