using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Common
{
    public static class BusinessConstants
    {

        /// <summary>
        /// Default value for a uniqueidentifier column
        /// </summary>
        public static string ZERO_ROW_ID = "00000000-0000-0000-0000-000000000000";
        public static Guid NULL_GUID = new Guid("00000000-0000-0000-0000-000000000000");
    }
}
