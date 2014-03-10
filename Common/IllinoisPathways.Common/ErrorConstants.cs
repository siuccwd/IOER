using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IllinoisPathways.Common
{
    public static class ErrorConstants
    {
        public const string DUPLICATE_USER = "USR-MSG-1001";
        public const string INVALID_PASSWORD = "USR-MSG-1002";
        public const string USER_CREATION_FAILED = "USR-MSG-1003";
        public const string USER_UPDATE_FAILED = "USR-MSG-1004";

        public const string PATRON_CREATION_FAILED = "PAT-MSG-1001";
        public const string PATRON_UPDATE_FAILED = "PAT-MSG-1002";
        public const string PATRON_ACTIVATE_FAILED = "PAT-MSG-1004";

        public const string PARTNER_CREATION_FAILED = "PAR-MSG-1001";
        public const string AUTHORIZE_PARTNER_FAILED = "PAR-MSG-1002";
        public const string PARTNER_UPDATE_FAILED = "PAR-MSG-1003";

        public const string AUDIT_CREATION_FAILED = "AUD-MSG-1001";
        public const string AUDIT_GEO_CREATION_FAILED = "AUD-MSG-1002";
	}
}
