using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ILPathways.Business

{
	///<summary>
	///Represents an object that describes a Organization
	///</summary>
	[Serializable]
    public class Organization : AbstractOrganization
	{
		///<summary>
        ///Initializes a new instance of the ILPathways.Business.Organization class.
		///</summary>
		public Organization() 
        {
            OrgMembers = new List<OrganizationMember>();
            IsIsleMember = false;
            ExternalIdentifier = "";
        }

        #region Properties for Organization
        private int _OrgTypeId;
        /// <summary>
        /// Gets/Sets OrgTypeId
        /// </summary>
        public int OrgTypeId
        {
            get { return this._OrgTypeId; }
            set
            {
                if ( this._OrgTypeId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._OrgTypeId = value;
                    HasChanged = true;
                }
            }
        }
        public string OrgType { get; set; }

        /// <summary>
        /// Gets/Sets IsIsleMember
        /// </summary>
        public bool IsIsleMember { get; set; }

        /// <summary>
        /// Gets/Sets ExternalIdentifier
        /// </summary>
        public string ExternalIdentifier { get; set; }
       
        #endregion

        #region behaviours
        public List<OrganizationMember> OrgMembers { get; set; }
		#endregion


		#region Authentication methods

		public bool IsRTTTOrg()
		{
            //TBD
            if ( OrgTypeId == 1 )
				return true;
			else
				return false;
		}//
		/// <summary>
		/// Return true if a school (K-12) site
        /// - may make an external property than is derived
		/// </summary>
        /// <returns>True if OrgTypeId = 1</returns>
		public bool IsSchoolOrg()
		{
            //TBD
            if ( OrgTypeId == 1 )
				return true;
			else
				return false;
		}//
        /// <summary>
        /// Return true if is a Stem  ==> can a site be a more than one type???
        /// </summary>
        /// <returns>True if OrgTypeId = 2</returns>
        public bool IsStemOrg()
        {
            //TBD
            if ( OrgTypeId == 2 )
                return true;
            else
                return false;
        }//
		#endregion
	} // end class 
} // end Namespace 

