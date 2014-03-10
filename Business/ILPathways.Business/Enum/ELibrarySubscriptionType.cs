using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    /// <summary>
    /// Specifies the types of library/collection subscriptions
    /// </summary>
    public enum ELibrarySubscriptionType : int
    {
        /// <summary>
        /// Indicates subscribed without any notifications.
        /// </summary>
        Reader = 1,

        /// <summary>
        /// Indicates subscribed and gets weekly updates ==> not active at this time
        /// </summary>
        Summary = 2,

        /// <summary>
        /// Indicates subscribed and gets daily updates
        /// </summary>
        Watcher = 3,

        /// <summary>
        /// Indicates subscribed and gets immediate updates
        /// </summary>
        Follower = 4

        
    }
}
