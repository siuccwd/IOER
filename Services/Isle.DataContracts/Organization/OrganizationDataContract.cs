using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Isle.DataContracts 
{
    [DataContract]
    public class OrganizationDataContract : BaseContract
    {

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Notes { get; set; }

        [DataMember]
        public int ParentId { get; set; }

        [DataMember]
        public int PrimaryContactId { get; set; }

        [DataMember]
        public int Lwia { get; set; }

        [DataMember]
        public int IwdsOfficeId { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public string InactiveExplanation { get; set; }

        [DataMember]
        public string MainPhone { get; set; }

        [DataMember]
        public string MainExtension { get; set; }

        [DataMember]
        public string AltPhone { get; set; }

        [DataMember]
        public string AltExtension { get; set; }

        [DataMember]
        public string Fax { get; set; }

        [DataMember]
        public string TTY { get; set; }

        [DataMember]
        public string WebSite { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string Address { get; set; }

        [DataMember]
        public string Address2 { get; set; }

        [DataMember]
        public string City { get; set; }

        [DataMember]
        public string State { get; set; }

        [DataMember]
        public string Zipcode { get; set; }

        [DataMember]
        public string Services { get; set; }

        [DataMember]
        public int AvgWalkin { get; set; }

        [DataMember]
        public string DescClientele { get; set; }

        [DataMember]
        public int NumStaff { get; set; }

        [DataMember]
        public bool TechRequirements { get; set; }

        [DataMember]
        public int NumCompInternet { get; set; }

        [DataMember]
        public bool HighSpeedInternet { get; set; }

        [DataMember]
        public bool IsComprehensive { get; set; }

        [DataMember]
        public bool IsSatellite { get; set; }

        [DataMember]
        public bool HasBilingual { get; set; }

        [DataMember]
        public string BilingualDesc { get; set; }

        [DataMember]
        public bool HasSpanish { get; set; }

        [DataMember]
        public string SpanishDesc { get; set; }

        [DataMember]
        public bool WIAFunded { get; set; }

        [DataMember]
        public bool IsEducational { get; set; }

        [DataMember]
        public bool IsCBO { get; set; }

        [DataMember]
        public bool IsFaithBased { get; set; }

        [DataMember]
        public bool IsStateAgency { get; set; }

        [DataMember]
        public bool IsLibrary { get; set; }

        [DataMember]
        public bool IsCommCenter { get; set; }

        [DataMember]
        public bool IsSocialService { get; set; }

        [DataMember]
        public bool IsPrivateSector { get; set; }

        [DataMember]
        public bool AccessDissem { get; set; }

        [DataMember]
        public DateTime Created { get; set; }

        [DataMember]
        public string CreatedBy { get; set; }

        [DataMember]
        public DateTime LastUpdated { get; set; }

        [DataMember]
        public string LastUpdatedBy { get; set; }

        [DataMember]
        public int AffiliatesId { get; set; }

        [DataMember]
        public DateTime Timestamp { get; set; }

        [DataMember]
        public string SiteStatus { get; set; }

        [DataMember]
        public string WorknetSetupStatus { get; set; }

        [DataMember]
        public string ContentManagementStatus { get; set; }

        [DataMember]
        public string AdditionalTrainingReq { get; set; }

        [DataMember]
        public string CompSatelliteType { get; set; }

        [DataMember]
        public string SiteType { get; set; }

        [DataMember]
        public string StaffingDesc { get; set; }

        [DataMember]
        public int IwdsOfficeNbr { get; set; }

        [DataMember]
        public short IwdsSiteType { get; set; }

        [DataMember]
        public bool IsResourceRoom { get; set; }

        [DataMember]
        public string ContactEmail { get; set; }

        [DataMember]
        public bool IsLwia { get; set; }

        [DataMember]
        public int OrgPathwayId { get; set; }

        [DataMember]
        public bool IsSiteComplete { get; set; }

        [DataMember]
        public int LogoId { get; set; }

        [DataMember]
        public bool IsStateAgencyHO { get; set; }

        [DataMember]
        public bool IsDigitalDivide { get; set; }

        [DataMember]
        public Guid RowId { get; set; }

        [DataMember]
        public bool CanIssueEAVouchers { get; set; }

        [DataMember]
        public bool IsEAExamProctor { get; set; }

        [DataMember]
        public int OrgTypeId { get; set; }

        [DataMember]
        public string ExternalIdentifier { get; set; }

    }
  }//

