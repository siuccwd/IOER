using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    /// <summary>
    /// Specifies the types of library/collection members
    /// </summary>
    public enum ELibraryMemberType : int
    {
        /// <summary>
        /// Indicates read only access.
        /// </summary>
        Reader = 1,

        /// <summary>
        /// Indicates can read and contribute
        /// </summary>
        Contributor = 2,

        /// <summary>
        /// Indicates an editor
        /// </summary>
        Editor = 3,

        /// <summary>
        /// Indicates an administrator
        /// </summary>
        Administrator = 4


    }
}
