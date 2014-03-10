using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ILPathways.Business
{
    /// <summary>
    /// Data structure for a User (Business / Employer)
    /// </summary>
    [Serializable]
    public class AppUser : ILPathways.Business.Contact, IWebUser
    {

        #region Private variables
        private string _username = "";
        private string _password = "";

        private long _fldSecret_Question_ID;
        private string _secretAnswer = "";
        private string _lastTermsAcceptedDate = System.DateTime.MinValue.ToString();
        #endregion

        public AppUser()
        { }

        #region Properties
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


        /// <summary>
        /// Get/Set SecretQuestionID
        /// </summary>
        public long SecretQuestionID
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
        ///// <summary>
        ///// Get/Set Gender
        ///// </summary>
        //public string Gender
        //{
        //    get { return _Gender; }
        //    set
        //    {
        //        if ( this._Gender == value )
        //        {
        //            //Ignore set
        //        }
        //        else
        //        {
        //            this._Gender = value.Trim();
        //            HasChanged = true;
        //        }
        //    }
        //}
        ///// <summary>
        ///// Get GenderId - int version of Gender
        ///// </summary>
        //public int GenderId
        //{
        //    get
        //    {
        //        if ( IsNumeric( _Gender ) )
        //            return Int32.Parse( _Gender );
        //        else
        //            return 3;
        //    }
        //}

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


        #endregion

        #region Properties for Profile
        public bool HasUserProfile()
        {
            if ( UserProfile != null && UserProfile.Id > 0 )
                return true;
            else
                return false;
        }
        private AppUserProfile _prof;
        /// <summary>
        /// Gets/Sets UserProfile
        /// </summary>
        public AppUserProfile UserProfile
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
                    HasChanged = true;
                }
            }
        }

        //fake orgId
        private int _orgId = 0;
        public new int OrgId
        {
            get
            {
                if ( HasUserProfile() )
                    return UserProfile.OrganizationId;
                else
                    return 0;
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
        /// Create. populate and return a guest AppUser object
        /// </summary>
        /// <returns>AppUser formatted as a guest</returns>
        public AppUser GuestUser()
        {
            AppUser guestUser = new AppUser();
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
        /// Format user summary as Html:
        /// - Name, address, phone, e-mail 
        /// </summary>
        /// <returns></returns>
        public virtual string SummaryAsHtml()
        {
            string summary = FullName();

            if ( Title.Length > 0 )
                summary = FormatHtmlList( summary, Title );

            summary = FormatHtmlList( summary, FormatHtmlAddress() );

            if ( PhoneNumber.Length > 0 )
                summary = FormatHtmlList( summary, "Phone: " + DisplayPhone( PhoneNumber, MainPhoneExt ) );

            if ( Email.Length > 0 )
                summary = FormatHtmlList( summary, "E-mail: " + Email );

            return summary;
        } //
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

            //if ( MyOrganization != null && MyOrganization.Id > 0 )
            //{
            //    summary.Append( "<br/>" + MyOrganization.Name );

            //}
            //else 
                if ( OrganizationName.Length > 0 )
            {
                summary.Append( "<br/>" + OrganizationName );
            }
            if ( Title.Length > 0 )
            {
                summary.Append( "<br/>" + Title );
            }
            if ( PhoneNumber.Length == 10 )
            {
                summary.Append( "<br/>Phone: " + this.DisplayPhone( PhoneNumber, this.MainPhoneExt ) );
            }
            if ( this.Email.Length > 0 )
            {
                summary.Append( "<br/>E-mail: " + this.Email );
            }

            return summary.ToString();
        } //


        /// <summary>
        /// Format user detail as Html:
        /// - Name, address, phone, e-mail 
        /// - organization info
        /// </summary>
        /// <returns></returns>
        public string DetailAsHtml()
        {
            string summary = this.SummaryAsHtml();

            summary += "<br/>Username: " + this.Username;
            if ( DateOfBirth.Length > 5 && IsDate( DateOfBirth ) )
            {
                summary += "<br/>Birthdate: " + AsMMMDDYYYY( System.DateTime.Parse( DateOfBirth ) );
            }


            return summary;
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
        /// Check if user has any Illinois workNet roles
        /// </summary>
        /// <returns></returns>
        public bool HasWorkNetRole()
        {
            if ( TopAuthorization < ( int ) EUserRole.Guest )
                return true;
            else
                return false;
        }

        #endregion
    }
}
