using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    [Serializable]
    public class OrganizationMember : BaseBusinessDataEntity
    {

		public static int MEMBERTYPE_PENDING = 0;
        public static int MEMBERTYPE_ADMINISTRATION = 1;
        public static int MEMBERTYPE_EMPLOYEE = 2;
        public static int MEMBERTYPE_STUDENT = 3;
        public static int MEMBERTYPE_EXTERNAL = 4;

        public static int MEMBERROLE_ADMINISTRATOR = 1;
        public static int MEMBERROLE_CONTENT_APPROVER = 2;
        public static int MEMBERROLE_LIBRARY_ADMIN = 3;
        public static int MEMBERROLE_ACCOUNT_ADMIN = 4;

        public OrganizationMember()
        {
            OrgMemberType = "";
            MemberRoles = new List<OrganizationMemberRole>();
            FirstName = "";
            LastName = "";
        }
    
        public int OrgId { get; set; }
        public int UserId { get; set; }
        public int OrgMemberTypeId { get; set; }

        private string _orgMemberType = "";
        public string OrgMemberType {
            get
            {
                if ( _orgMemberType.Length == 0
                    && OrgMemberTypeId > 0)
                {
                    if ( OrgMemberTypeId == 1 ) _orgMemberType = "Administration";
                    else if ( OrgMemberTypeId == 2 ) _orgMemberType = "Employee";
                    else if ( OrgMemberTypeId == 3 ) _orgMemberType = "Student";
                    else if ( OrgMemberTypeId == 4 ) _orgMemberType = "External";
					else if ( OrgMemberTypeId == 0 ) _orgMemberType = "Pending";
                }
                    
                return _orgMemberType;
            }
            set 
            { 
                this._orgMemberType = value; 
            }
        }
        public string Organization { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
		public string Email { get; set; }
        public string ImageUrl { get; set; }
        public string UserProfileUrl { get; set; }

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
        /// <summary>
        /// Get/Set list of org member roles
        /// </summary>
        public List<OrganizationMemberRole> MemberRoles { get; set; }

        /// <summary>
        /// Return true if user has an administration role
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Return true if user has an approver role
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Return true if user has a library administration role
        /// </summary>
        /// <returns></returns>
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
