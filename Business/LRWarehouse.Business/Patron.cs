using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILPathways.Business;

namespace LRWarehouse.Business
{
    [Serializable]
    public class Patron : BaseBusinessDataEntity, IWebUser
    {
        #region Private variables
        private string _username = "";
        private string _password = "";

        private int _fldSecret_Question_ID;
        private string _secretAnswer = "";

        private string _roleName = "";
        private int _roleId;

        private int _worknetId;

        private string _lastTermsAcceptedDate = System.DateTime.MinValue.ToString();

        /// <summary>
        /// defaults
        /// </summary>
        private const int _ProgramStaff = 4;
        private const int _LwiaCoordinator = 10;
        private const int _OrganizationAdministration = 20;

        #endregion

        public Patron()
        { }

        #region Properties

        private string _firstName;
        public string FirstName
        {
            get { return _firstName; }
            set
            {
                if ( this._firstName == value )
                {
                    //Ignore set
                }
                else
                {
                    this._firstName = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private string _lastName;
        public string LastName
        {
            get { return _lastName; }
            set
            {
                if ( this._lastName == value )
                {
                    //Ignore set
                }
                else
                {
                    this._lastName = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private string _email = "";
        /// <summary>
        /// Gets/Sets Email
        /// </summary>
        public string Email
        {
            get
            {
                return this._email;
            }
            set
            {
                if ( this._email == value )
                {
                    //Ignore set
                }
                else
                {
                    this._email = value.Trim();
                    HasChanged = true;
                }
            }
        }
        /// <summary>
        /// Get/Set for User name
        /// </summary>
        public string Username
        {
            get { return _username; }
            set
            {
                if ( this._username == value )
                {
                    //Ignore set
                }
                else
                {
                    this._username = value.Trim();
                    HasChanged = true;
                }
            }
        }
        public string UserName
        {
            get { return _username; }
            set
            {
                if ( this._username == value )
                {
                    //Ignore set
                }
                else
                {
                    this._username = value.Trim();
                    HasChanged = true;
                }
            }
        }
        /// <summary>
        /// Get/Set for Password
        /// </summary>
        public string Password
        {
            get { return _password; }
            set
            {
                if ( this._password == value )
                {
                    //Ignore set
                }
                else
                {
                    this._password = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private string _zipCode = "";
        /// <summary>
        /// Gets/Sets ZipCode
        /// </summary>
        public string ZipCode
        {
            get
            {
                return this._zipCode;
            }
            set
            {
                if ( this._zipCode != value )
                {
                    this._zipCode = value.Trim();
                    HasChanged = true;
                }
            }
        }

        /// <summary>
        /// Get/Set SecretQuestionID
        /// </summary>
        public int SecretQuestionID
        {
            get { return _fldSecret_Question_ID; }
            set
            {
                if ( this._fldSecret_Question_ID == value )
                {
                    //Ignore set
                }
                else
                {
                    this._fldSecret_Question_ID = value;
                    HasChanged = true;
                }
            }
        }
        /// <summary>
        /// Get/Set SecretAnswer
        /// </summary>
        public string SecretAnswer
        {
            get { return _secretAnswer; }
            set
            {
                if ( this._secretAnswer == value )
                {
                    //Ignore set
                }
                else
                {
                    this._secretAnswer = value.Trim();
                    HasChanged = true;
                }
            }
        }


        /// <summary>
        /// Get/Set LastTermsAcceptedDate - the last date that the user agreed to a terms of agreement
        /// </summary>
        public string LastTermsAcceptedDate
        {
            get { return _lastTermsAcceptedDate; }
            set
            {
                if ( this._lastTermsAcceptedDate == value )
                {
                    //Ignore set
                }
                else
                {
                    this._lastTermsAcceptedDate = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private int _topAuthorization = 99;
        /// <summary>
        /// Gets/Sets TopAuthorization
        /// </summary>
        public int TopAuthorization
        {
            get
            {
                return this._topAuthorization;
            }
            set
            {
                if ( this._topAuthorization == value )
                {
                    //Ignore set
                }
                else
                {
                    this._topAuthorization = value;
                    HasChanged = true;
                }
            }
        } //

        public int worknetId
        {
            get
            {
                return this._worknetId;
            }
            set
            {
                if ( this._worknetId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._worknetId = value;
                    HasChanged = true;
                }
            }
        } //

        #endregion

        #region Properties of external objects

        private int _currentLibraryId;
        public int CurrentLibraryId
        {
            get
            {
                return this._currentLibraryId;
            }
            set
            {
                if ( this._currentLibraryId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._currentLibraryId = value;
                    HasChanged = true;
                }
            }
        } //


        #endregion


        #region Helper Methods
        /// <summary>
        /// Returns Identifying Name of user - used for lastUpdatedBy, etc.
        /// </summary>
        /// <returns></returns>
        public string IdentifyingName()
        {
            string idName = "";

            if ( this.Username.ToLower().Equals( this.Email.ToLower() ) )
            {
                idName = this.FullName();
            }
            else if ( this.Username.IndexOf( "@" ) > -1 )
            {
                idName = this.FullName();
            }
            else
            {
                idName = this.Username;
            }

            return idName;
        }//

        /// <summary>
        /// Create. populate and return a guest Patron object
        /// </summary>
        /// <returns>Patron formatted as a guest</returns>
        public Patron GuestUser()
        {
            Patron guestUser = new Patron();
            using ( guestUser )
            {
                Id = 0;
                Username = "guest";
                FirstName = "guest";
                LastName = "user-" + System.DateTime.Now.ToString();
                //RoleId = 1;
                IsActive = true;
                IsValid = true;
            }
            return guestUser;
        }
      
        /// <summary>
        /// Format fields for use as an e-mail signature
        /// - for now the same as contact summary; but created new method to allow independent updates
        /// </summary>
        /// <returns></returns>
        public string EmailSignature()
        {
            //string summary = this.FullName();

            //if ( MyOrganization != null && MyOrganization.Id > 0 )
            //{
            //  summary += "<br/>" + MyOrganization.Name;

            //} else if ( OrganizationName.Length > 0 )
            //{
            //  summary += "<br/>" + OrganizationName;
            //}

            //if ( Title.Length > 0 )
            //  summary += "<br/>" + Title;

            //if ( this.PhoneNumber.Length > 0 )
            //{
            //  summary += "<br/>Phone: " + this.DisplayPhone( PhoneNumber, this.MainPhoneExt );
            //}

            //if ( this.Email.Length > 0 )
            //  summary += "<br/>E-mail: " + this.Email;

            //convert to stringbuilder
            StringBuilder summary = new StringBuilder();
            summary.Append( this.FullName() );
            if ( this.Email.Length > 0 )
            {
                summary.Append( "<br/>E-mail: " + this.Email );
            }

            //if ( MyOrganization != null && MyOrganization.Id > 0 )
            //{
            //    summary.Append( "<br/>" + MyOrganization.Name );

            //}
            //else 
            if ( HasUserProfile() )
            {
                if ( UserProfile.OrganizationId > 0 )
                {
                    summary.Append( "<br/>" + UserProfile.Organization );
                }
                if ( UserProfile.PublishingRoleId > 0 )
                {
                    summary.Append( "<br/>" + UserProfile.PublishingRole );
                }
            }

            return summary.ToString();
        } //


        #endregion

        #region Authentication methods

        /// <summary>
        /// Check if current user is Program Staff (or above)
        /// </summary>
        /// <returns>True if program staff, otherwise false.</returns>
        public bool IsProgramStaff()
        {
            //if ( this.Roles.RoleExists( "Program Staff" ) || TopAuthorization < _ProgramStaff )
            //    return true;
            //else
                return false;
        }
        /// <summary>
        /// Check if current user is an LWIA Coordinator
        /// </summary>
        /// <returns>True if is a Coordinator, otherwise false</returns>
        public bool IsCoordinator()
        {
            //if ( this.Roles.RoleExists( "LWIA Coordinator" ) || TopAuthorization < _LwiaCoordinator )
            //    return true;
            //else
                return false;
        } //

        /// <summary>
        /// Check if current user is an Organization Administrator
        /// </summary>
        public bool IsOrganizationAdministrator()
        {
            //if ( this.Roles.RoleExists( "Organization Administration" ) || TopAuthorization < _OrganizationAdministration )
            //    return true;
            //else
                return false;
        } //

        /// <summary>
        /// Check if current user is an Organization Business Manager
        /// </summary>
        /// <returns></returns>
        public bool IsOrganizationBusinessManager()
        {
            //if ( this.Roles.RoleExists( "Organization Business Manager" ) )
            //    return true;
            //else
                return false;
        }


        #endregion

        #region Properties for Profile 
        public bool HasUserProfile()
        {
            if ( UserProfile != null && UserProfile.UserId > 0 )
                return true;
            else
                return false;
        }
        private PatronProfile _prof;
        /// <summary>
        /// Gets/Sets UserProfile
        /// </summary>
        public PatronProfile UserProfile
        {
            get
            {
                return this._prof;
            }
            set
            {
                if ( this._prof == value )
                {
                    //Ignore set
                }
                else
                {
                    this._prof = value;
                    OrgId = value.OrganizationId;
                    HasChanged = true;
                }
            }
        }

        //fake orgId
        private int _orgId = 0;
        public int OrgId
        {
            get
            {
                return _orgId;
                //if ( HasUserProfile() )
                //    return UserProfile.OrganizationId;
                //else
                //    return 0;
            }
            set
            {
                if ( this._orgId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._orgId = value;
                    HasChanged = true;
                }
            }
        } //

        /// <summary>
        ///  Get/Set ParentOrgId
        /// </summary>
        private int _parentOrgId = -1;
        public int ParentOrgId
        {
            get
            {
                return _parentOrgId;
            }
            set
            {
                this._parentOrgId = value;
            }
        } //
        #endregion

        #region ===== Behaviours ===============================

        /// <summary>
        /// Returns full name of user
        /// </summary>
        /// <returns></returns>
        public string FullName()
        {
            return this.FirstName + " " + this.LastName;
        }

        /// <summary>
        /// Returns user name as LastName, FirstName
        /// </summary>
        /// <returns></returns>
        public string SortName()
        {
            return this.LastName + ", " + this.FirstName;
        }//

        /// <summary>
        /// Returns Short Name of user - first initial of first name plus last name.
        /// </summary>
        /// <returns></returns>
        public string ShortName()
        {
            string shortName = this.FirstName.Substring( 0, 1 ) + this.LastName;
            shortName = shortName.Replace( " ", "" );
            return shortName;
        }//

        #endregion
    }
}
