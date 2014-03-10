using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    /// <summary>
    /// Specifies the User role possible values
    /// </summary>
    public enum EUserRole : int
    {
        /// <summary>
        /// Indicates an System Administrator.
        /// </summary>
        SystemAdministrator = 1,

        /// <summary>
        /// Indicates a Program Administrator.
        /// </summary>
        ProgramAdministrator = 2,

        /// <summary>
        /// Indicates a Program Manager.
        /// </summary>
        ProgramManager = 3,

        /// <summary>
        /// Indicates a Program Staff.
        /// </summary>
        ProgramStaff = 4,

        /// <summary>
        /// Indicates a Lwia Coordinator.
        /// </summary>
        LwiaCoordinator = 10,

        /// <summary>
        /// Indicates a Lwia Manager.
        /// </summary>
        LwiaManager = 11,

        /// <summary>
        /// Indicates a Business Account Manager
        /// </summary>
        BusinessAccountManager = 13,
        /// <summary>
        /// Indicates a Lwia Staff.
        /// </summary>
        LwiaStaff = 15,

        /// <summary>
        /// Indicates an Organization Administrator/manager - can view/update all projects associated with organization.
        /// </summary>
        OrganizationAdministrator = 20,

        /// <summary>
        /// Indicates an Organization manager - resp tbd
        /// </summary>
        OrganizationManager = 21,

        /// <summary>
        /// Indicates manager specializing in some specific business functions within an org
        /// </summary>
        OrganizationBusinessManager = 22,

        /// <summary>
        /// Indicates an Organization contact, with no default update rights
        /// </summary>
        OrganizationStaff = 25,

        /// <summary>
        /// Indicates an Organization department administrator/manager - can update projects for all users in dept.
        /// </summary>
        DepartmentAdministrator = 30,

        /// <summary>
        /// Indicates a Department Manager - can see all projects for dept, but cannot update
        /// </summary>
        DepartmentManager = 31,

        /// <summary>
        /// Indicates a workNet Partner - no update rights, use for view access to channels
        /// </summary>
        workNetPartner = 40,

        /// <summary>
        /// Indicates a Public viewer - no update rights
        /// </summary>
        PublicViewer = 50,


        /// <summary>
        /// Indicates a Guest.
        /// </summary>
        Guest = 60
    }
}
