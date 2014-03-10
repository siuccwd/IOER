using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILPathways.Business;
using MyManager = ILPathways.DAL.SecurityManager;


namespace Isle.BizServices
{
    public class SecurityManager
    {
        /// <summary>
        /// Retrieve the privileges for the provided user (actually their highest role) and an object. If the object name is blank, default privileges will be returned
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="pObjectName">Control name, or channel, etc.</param>
        /// <returns></returns>
        public static ApplicationRolePrivilege GetGroupObjectPrivileges( IWebUser currentUser, string pObjectName )
        {
            return MyManager.GetGroupObjectPrivileges( currentUser, pObjectName );
        }//

    }
}
