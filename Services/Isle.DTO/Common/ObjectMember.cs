using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Isle.DTO
{
    [Serializable]
    public class ObjectMember
    {
        public ObjectMember() 
        {
            FirstName = "";
            LastName = "";
						Roles = new List<ILPathways.Common.CodeItem>();
        }

        public int Id { get; set; }
        /// <summary>
        /// PK for object of which the person is a member
        /// </summary>
        public int ObjectId { get; set; }

        /// <summary>
        /// The name of the owing org (if any) for the object
        /// </summary>
        public string ParentOrganization { get; set; }
        public int ParentOrgId { get; set; }

        public int UserId { get; set; }
        public int MemberTypeId { get; set; }
        public string MemberType { get; set; }
        //may need means to identify pending - actually typeId of zero
        public bool IsPendingMember 
        {
            get
            {
                if ( MemberTypeId > 0 )
                    return false;
                else
                    return true;
            }
        }

        public DateTime Created { get; set; }
        public DateTime LastUpdated { get; set; }
				public string CreatedText { get { return Created.ToShortDateString(); } }
				public string LastUpdatedText { get { return LastUpdated.ToShortDateString(); } }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string MemberSortName 
        {
            get { return LastName + ", " + FirstName; } 
        }
        public string MemberFullName
        {
            get { return FirstName + " " + LastName; }
        }
        /// <summary>
        /// is this the person's org, or the object's org?
        /// We should generally have the context of the object's parent org before doing gets?
        /// </summary>
        public string Organization { get; set; }
        public int OrgId { get; set; }

        public string MemberHomeUrl { get; set; }
        public string MemberImageUrl { get; set; }
				public List<ILPathways.Common.CodeItem> Roles { get; set; }
    }
}
