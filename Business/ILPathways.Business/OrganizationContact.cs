using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
	///<summary>
	///Represents an object that describes a OrganizationContact
	///</summary>
	[Serializable]
	public class OrganizationContact : BaseBusinessDataEntity
	{
    ///<summary>
		///Initializes a new instance of the workNet.BusObj.Entity.OrganizationContact class.
    ///</summary>
		public OrganizationContact() 
		{
			User = new AppUser();
			ThisOrganization = new Organization();
		}
        public static int ADMINISTRATOR_TYPE = 1;
        public static int EMPLOYEE_TYPE = 2;
        public static int APPROVER_TYPE = 3;
		#region Base Properties for OrganizationContact

		private int _orgId;
		/// <summary>
		/// Gets/Sets OrgId
		/// </summary>
		public int OrgId
		{
			get
			{
				return this._orgId;
			}
			set
			{
				if ( this._orgId == value )
				{
					//Ignore set
				} else
				{
					this._orgId = value;
					HasChanged = true;
				}
			}
		}

		private int _userId;
		/// <summary>
		/// Gets/Sets UserId
		/// </summary>
		public int UserId
		{
			get
			{
				return this._userId;
			}
			set
			{
				if ( this._userId == value )
				{
					//Ignore set
				} else
				{
					this._userId = value;
					HasChanged = true;
				}
			}
		}

        private int _contactTypeId;
        /// <summary>
        /// Gets/Sets ContactTypeId
        /// </summary>
        public int ContactTypeId
        {
            get
            {
                return this._contactTypeId;
            }
            set
            {
                if ( this._contactTypeId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._contactTypeId = value;
                    HasChanged = true;
                }
            }
        }//
		private string _contactType = "";
		/// <summary>
		/// Gets/Sets ContactType
		/// </summary>
		public string ContactType
		{
			get
			{
				return this._contactType;
			}
			set
			{
				if ( this._contactType == value )
				{
					//Ignore set
				} else
				{
					this._contactType = value.Trim();
					HasChanged = true;
				}
			}
		}//

		/// <summary>
		/// Retrieve whether this is a primary contact
		/// </summary>
		public bool IsPrimaryContact
		{
			get
			{
				if ( ContactType.ToLower().Equals( "primary" ) )
					return true;
				else
					return false;
			}
		}//

		private string _comment = "";
		/// <summary>
		/// Gets/Sets Comment
		/// </summary>
		public string Comment
		{
			get
			{
				return this._comment;
			}
			set
			{
				if ( this._comment == value )
				{
					//Ignore set
				} else
				{
					this._comment = value.Trim();
					HasChanged = true;
				}
			}
		}//

		#endregion

		#region Extended Properties for OrganizationContact - For display of full contact/organization info
		private AppUser _user;
		/// <summary>
		/// Gets/Sets User
		/// </summary>
		public AppUser User
		{
			get { return this._user; }
			set { this._user = value; }
		}//

		private Organization _thisOrganization;
		/// <summary>
		/// Gets/Sets base Organization
		/// </summary>
		public Organization ThisOrganization
		{
			get { return this._thisOrganization; }
			set { this._thisOrganization = value; }
		}//

		private string _businessName = "";
		/// <summary>
        /// Gets/Sets OrgName - from organization not appUser record!
		/// </summary>
		public string OrgName
		{
			get { return this._businessName; }
			set { this._businessName = value; }
		}//


		#endregion

		#region Behaviours/helper methods
        /// <summary>
        /// True if contact tupe is employee
        /// ??however, an admin is also an employee????
        /// </summary>
        /// <returns></returns>
        public bool IsEmployee()
        {
            if ( ContactTypeId == EMPLOYEE_TYPE )
                return true;
            else
                return false;
        }
        public bool IsAdministrator()
        {
            if ( ContactTypeId == ADMINISTRATOR_TYPE )
                return true;
            else
                return false;
        }
        public bool IsApprover()
        {
            if ( ContactTypeId == APPROVER_TYPE )
                return true;
            else
                return false;
        }
		/// <summary>
		/// Display summary personal information for this contact
		/// </summary>
		/// <returns></returns>
		public string ContactPersonalSummary()
		{
			string summary = User.EmailSignature();

		
			return summary;

			//return User.SummaryAsHtml();
		}//
		#endregion
	}
}
