using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    [Serializable]
    public class LibraryInvitation : BaseBusinessDataEntity
  {
        ///<summary>
      ///Initializes a new instance of the ILPathways.Business.LibraryInvitation class.
        ///</summary>
        public LibraryInvitation() 
        {
            InvitationToUser = new AppUser();
            InvitationByUser = new AppUser();
            IsActive = true;
            StartingUrl = "";
        }

        #region Properties created from dictionary for GroupInvitation


        private int _parentId;
        /// <summary>
        /// Gets/Sets ParentId
        /// </summary>
        public int ParentId
        {
          get
          {
              return this._parentId;
          }
          set
          {
              if ( this._parentId == value )
              {
              //Ignore _parentId
            } else {
                this._parentId = value;
              HasChanged = true;
            }
          }
        }

        private int _libMemberTypeId;
        /// <summary>
        /// Gets/Sets LibMemberTypeId
        /// </summary>
        public int LibMemberTypeId
        {
            get
            {
                return this._libMemberTypeId;
            }
            set
            {
                if ( this._libMemberTypeId == value )
                {
                    //Ignore _parentId
                }
                else
                {
                    this._libMemberTypeId = value;
                    HasChanged = true;
                }
            }
        }


	    private string _invitationType = "";
	    /// <summary>
	    /// Gets/Sets InvitationType
        /// -personal
        /// -group
	    /// </summary>
	    public string InvitationType
	    {
		    get
		    {
			    return this._invitationType;
		    }
		    set
		    {
			    if ( this._invitationType == value )
			    {
				    //Ignore set
			    } else
			    {
				    this._invitationType = value.Trim();
				    HasChanged = true;
			    }
		    }
	    }

	    private string _PassCode = "";
	    /// <summary>
	    /// Gets/Sets PassCode
	    /// </summary>
	    public string PassCode
	    {
		    get
		    {
			    return this._PassCode;
		    }
		    set
		    {
			    if ( this._PassCode == value )
			    {
				    //Ignore set
			    } else
			    {
				    this._PassCode = value.Trim();
				    HasChanged = true;
			    }
		    }
	    }

        private string _targetEmail = "";
        /// <summary>
        /// Gets/Sets TargetEmail - will have email or UserId, not both to distinguish that one has an account and the other does not
        /// </summary>
        public string TargetEmail
        {
          get
          {
            return this._targetEmail;
          }
          set
          {
            if (this._targetEmail == value) {
              //Ignore set
            } else {
              this._targetEmail = value.Trim();
              HasChanged = true;
            }
          }
        }

        private int _targetUserId;
        /// <summary>
        /// Gets/Sets TargetUserId
        /// </summary>
        public int TargetUserId
        {
          get
          {
            return this._targetUserId;
          }
          set
          {
            if (this._targetUserId == value) {
              //Ignore set
            } else {
              this._targetUserId = value;
              HasChanged = true;
            }
          }
        }

        private int _addToOrgId;
        /// <summary>
        /// Gets/Sets AddToOrgId- invitee will be added to org on accept
        /// </summary>
        public int AddToOrgId
        {
            get
            {
                return this._addToOrgId;
            }
            set
            {
                if ( this._addToOrgId == value )
                {
                    //Ignore _parentId
                }
                else
                {
                    this._addToOrgId = value;
                    HasChanged = true;
                }
            }
        }

        private int _addAsOrgMemberTypeId;
        /// <summary>
        /// Gets/Sets AddAsOrgMemberTypeId- invitee will be added to as org member type
        /// </summary>
        public int AddAsOrgMemberTypeId
        {
            get
            {
                return this._addAsOrgMemberTypeId;
            }
            set
            {
                if ( this._addAsOrgMemberTypeId == value )
                {
                    //Ignore _parentId
                }
                else
                {
                    this._addAsOrgMemberTypeId = value;
                    HasChanged = true;
                }
            }
        }

        /// <summary>
        /// CSV list of organization roles to add for a new mbr (typically used for new user that is self registering)
        /// </summary>
        public string OrgMbrRoles { get; set; }

        /// <summary>
        /// Optionally include a starting url on confirmation
        /// </summary>
        public string StartingUrl { get; set; }

        private string _subject = "";
        /// <summary>
        /// Gets/Sets Subject
        /// </summary>
        public string Subject
        {
          get
          {
            return this._subject;
          }
          set
          {
            if (this._subject == value) {
              //Ignore set
            } else {
              this._subject = value.Trim();
              HasChanged = true;
            }
          }
        }

        private string _messageContent = "";
        /// <summary>
        /// Gets/Sets MessageContent
        /// </summary>
        public string MessageContent
        {
          get
          {
            return this._messageContent;
          }
          set
          {
            if (this._messageContent == value) {
              //Ignore set
            } else {
              this._messageContent = value.Trim();
              HasChanged = true;
            }
          }
        }

        private string _emailNoticeCode = "";
        /// <summary>
        /// Gets/Sets EmailNoticeCode
        /// </summary>
        public string EmailNoticeCode
        {
          get
          {
            return this._emailNoticeCode;
          }
          set
          {
            if (this._emailNoticeCode == value) {
              //Ignore set
            } else {
              this._emailNoticeCode = value.Trim();
              HasChanged = true;
            }
          }
        }

        private string _response = "";
        /// <summary>
        /// Gets/Sets Response
        /// </summary>
        public string Response
        {
          get
          {
            return this._response;
          }
          set
          {
            if (this._response == value) {
              //Ignore set
            } else {
              this._response = value.Trim();
              HasChanged = true;
            }
          }
        }

        private DateTime _responseDate;
        /// <summary>
        /// Gets/Sets ResponseDate
        /// </summary>
        public DateTime ResponseDate
        {
          get
          {
            return this._responseDate;
          }
          set
          {
            if (this._responseDate == value) {
              //Ignore set
            } else {
              this._responseDate = value;
              HasChanged = true;
            }
          }
        }

        private DateTime _expiryDate;
        /// <summary>
        /// Gets/Sets ExpiryDate
        /// - create job to remove pending invites
        /// </summary>
        public DateTime ExpiryDate
        {
          get
          {
            return this._expiryDate;
          }
          set
          {
            if (this._expiryDate == value) {
              //Ignore set
            } else {
              this._expiryDate = value;
              HasChanged = true;
            }
          }
        }

        private bool _deleteOnResponse;
        /// <summary>
        /// Gets/Sets DeleteOnResponse
        /// - if true, delete the invite record on response
        /// </summary>
        public bool DeleteOnResponse
        {
          get
          {
            return this._deleteOnResponse;
          }
          set
          {
            if (this._deleteOnResponse == value) {
              //Ignore set
            } else {
              this._deleteOnResponse = value;
              HasChanged = true;
            }
          }
        }

        #endregion

	    #region External stuff

        //???????????
        public AppUser InvitationToUser { get; set; }
        public AppUser InvitationByUser { get; set; }

	    #endregion
	} // end class 
} // end Namespace 

