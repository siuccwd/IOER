using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    public class OrganizationMember : BaseBusinessDataEntity
    {

        public static int MEMBERTYPE_ADMINISTRATION = 1;
        public static int MEMBERTYPE_EMPLOYEE = 2;
        public static int MEMBERTYPE_STUDENT = 3;
        public static int MEMBERTYPE_CONTRACTOR = 4;

        public static int MEMBERROLE_ADMINISTRATOR = 1;
        public static int MEMBERROLE_CONTENT_APPROVER = 2;
        public static int MEMBERROLE_LIBRARY_ADMIN = 3;
        public static int MEMBERROLE_ACCOUNT_ADMIN = 4;

        public OrganizationMember()
        {
            OrgMemberType = "";
            MemberRoles = new List<OrganizationMemberRole>();
        }
    
        public int OrgId { get; set; }
        public int UserId { get; set; }
        public int OrgMemberTypeId { get; set; }

        public string OrgMemberType { get; set; }
        public string Organization { get; set; }

        #region Behaviours/helper methods
        /// <summary>
        /// True if contact tupe is employee
        /// ??however, an admin is also an employee????
        /// </summary>
        /// <returns></returns>
        public bool IsEmployee()
        {
            if ( OrgMemberTypeId == MEMBERTYPE_EMPLOYEE )
                return true;
            else
                return false;
        }
        public bool IsAdministration()
        {
            if ( OrgMemberTypeId == MEMBERTYPE_ADMINISTRATION )
                return true;
            else
                return false;
        }

        public List<OrganizationMemberRole> MemberRoles { get; set; }
        public bool HasAdministratorRole()
        {
            bool hasRole = false;
            if ( MemberRoles == null || MemberRoles.Count == 0 )
                return false;
            foreach ( OrganizationMemberRole role in MemberRoles )
            {
                if ( role.RoleId == MEMBERROLE_ADMINISTRATOR )
                {
                    hasRole = true;
                    break;
                }
            }
            return hasRole;
        }
        public bool HasContentApproverRole()
        {
            bool hasRole = false;
            if ( MemberRoles == null || MemberRoles.Count == 0 )
                return false;
            foreach ( OrganizationMemberRole role in MemberRoles )
            {
                if ( role.RoleId == MEMBERROLE_CONTENT_APPROVER )
                {
                    hasRole = true;
                    break;
                }
            }
            return hasRole; ;
        }
        public bool HasLibraryAdministratorRole()
        {
            bool hasRole = false;
            if ( MemberRoles == null || MemberRoles.Count == 0 )
                return false;
            foreach ( OrganizationMemberRole role in MemberRoles )
            {
                if ( role.RoleId == MEMBERROLE_LIBRARY_ADMIN )
                {
                    hasRole = true;
                    break;
                }
            }
            return hasRole; ;
        }
        #endregion
    }
}
