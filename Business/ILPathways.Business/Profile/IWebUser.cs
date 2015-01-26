using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    public interface IWebUser
    {

        int Id { get; set; }
        Guid RowId { get; set; }

        string UserName { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string Email { get; set; }
        string ZipCode { get; set; }
        string ProxyId { get; set; }
        int OrgId { get; set; }
        //maybe?
        int TopAuthorization { get; set; }

        string FullName();
        
        string EmailSignature();

        //List<Organization> Organizations { get; set; }
        List<OrganizationMember> OrgMemberships { get; set; }
        DateTime LastOrgMbrCheckDate { get; set; }
    }
}
