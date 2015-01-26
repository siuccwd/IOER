using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    [Serializable]
    public class GroupTeamMember : BaseBusinessDataEntity
    {
        ///<summary>
        ///Initializes a new instance of the ILPathways.Business.GroupManager class.
        ///</summary>
        public GroupTeamMember()
        {
            GroupPrivilege = new ObjectPrivilege();
        }//


        #region Properties created from dictionary for GroupMemberManager

        private int _groupId;
        /// <summary>
        /// Gets/Sets GroupId
        /// </summary>
        public int GroupId
        {
            get
            {
                return this._groupId;
            }
            set
            {
                if ( this._groupId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._groupId = value;
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
                }
                else
                {
                    this._userId = value;
                    HasChanged = true;
                }
            }
        }

        private ObjectPrivilege _grpPrivilege;
        /// <summary>
        /// Gets/Sets GroupPrivilege - uses ObjectPrivilege class to store/retrieve values
        /// </summary>
        public ObjectPrivilege GroupPrivilege
        {
            get
            {
                return this._grpPrivilege;
            }
            set
            {
                this._grpPrivilege = value;
            }
        } // end property


        #endregion

        /// <summary>
        /// AssignDefaultPrivileges
        /// </summary>
        /// <param name="privilegeLevel"></param>
        /// <returns></returns>
        public void AssignDefaultPrivileges( int privilegeLevel )
        {

            GroupPrivilege.CreatePrivilege = privilegeLevel;
            GroupPrivilege.ReadPrivilege = privilegeLevel;
            GroupPrivilege.WritePrivilege = privilegeLevel;
            GroupPrivilege.DeletePrivilege = privilegeLevel;
            GroupPrivilege.AppendPrivilege = privilegeLevel;

        }
    } // end class 
} // end Namespace 

