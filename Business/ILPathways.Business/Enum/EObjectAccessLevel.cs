using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    public enum EObjectAccessLevel : int
    {
        /// <summary>
        /// Indicates no access
        /// </summary>
        None = 0,
        /// <summary>
        /// indicates that the library is private, but discoverable in that access may be requested.
        /// </summary>
        ByRequestOnly = 1,
        /// <summary>
        /// Indicates read only access to the object
        /// </summary>
        ReadOnly = 2,
        /// <summary>
        /// Indicates can contribute, but approval is required
        /// </summary>
        ContributeWithApproval = 3,
        /// <summary>
        /// Indicates can contribute, and approval is NOT required
        /// </summary>
        ContributeWithNoApproval = 4
    }
}
