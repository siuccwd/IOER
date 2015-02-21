using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Isle.DTO.Content
{
    public class Curriculum
    {

        #region Properties
        public int Id { get; set; }

        private string _title = "";
        /// <summary>
        /// Gets/Sets Title
        /// </summary>
        public string Title
        {
            get
            {
                return this._title;
            }
            set
            {
                if ( this._title == value )
                {
                    //Ignore set
                }
                else
                {
                    this._title = value.Trim();
                    
                }
            }
        }

        private string _summary = "";
        /// <summary>
        /// Gets/Sets Summary - for Learning Registry
        /// </summary>
        public string Summary
        {
            get
            {
                return this._summary;
            }
            set
            {
                if ( this._summary == value )
                {
                    //Ignore set
                }
                else
                {
                    this._summary = value.Trim();
                    
                }
            }
        }

        private int _typeId;
        /// <summary>
        /// Gets/Sets TypeId
        /// </summary>
        public int TypeId
        {
            get
            {
                return this._typeId;
            }
            set
            {
                if ( this._typeId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._typeId = value;
                    
                }
            }
        }
        private string _contentType = "";
        /// <summary>
        /// Gets/Sets ContentType
        /// </summary>
        public string ContentType
        {
            get
            {
                if ( _contentType.Length > 0 )
                    return this._contentType;
                else
                {
                    if ( TypeId == 0 )
                    {
                        return "";
                    }
                    else
                    {
                        if ( TypeId == 59 ) return "Assessment";
                        else if ( TypeId == 58 ) return "Activity";
                        else if ( TypeId == 56 ) return "Lesson";
                        else if ( TypeId == 54 ) return "Unit";
                        else if ( TypeId == 52 ) return "Module";
                        else if ( TypeId == 50 ) return "Curriculum";
                        else if ( TypeId == 40 ) return "Document";
                        else if ( TypeId == 30 ) return "Lesson Plan";
                        else if ( TypeId == 15 ) return "Template";
                        else if ( TypeId == 10 ) return "Content";
                        else return "Content";
                    }
                }
            }
            set
            {
                if ( this._contentType == value )
                {
                    //Ignore set
                }
                else
                {
                    this._contentType = value.Trim();
                    
                }
            }
        }

        private bool _isOrgContent;
        /// <summary>
        /// Gets/Sets IsOrgContentOwner
        /// </summary>
        public bool IsOrgContentOwner
        {
            get
            {
                return this._isOrgContent;
            }
            set
            {
                if ( this._isOrgContent == value )
                {
                    //Ignore set
                }
                else
                {
                    this._isOrgContent = value;
                    
                }
            }
        }

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
                }
                else
                {
                    this._orgId = value;
                    
                }
            }
        }

        private string _organization = "";
        /// <summary>
        /// Gets/Sets Organization
        /// </summary>
        public string Organization
        {
            get
            {
                return this._organization;
            }
            set
            {
                this._organization = value.Trim();
            }
        }

        public int ParentId { get; set; }
        public int SortOrder { get; set; }

        private int _statusId;
        /// <summary>
        /// Gets/Sets StatusId
        /// </summary>
        public int StatusId
        {
            get
            {
                return this._statusId;
            }
            set
            {
                if ( this._statusId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._statusId = value;
                    
                }
            }
        }


        private string _status = "";
        /// <summary>
        /// Gets/Sets Status
        /// </summary>
        public string Status
        {
            get
            {
                if ( StatusId > 0 && _status == "" )
                    _status = GetStatusTitle( StatusId );
                return this._status;
            }
            set
            {
                if ( this._status == value )
                {
                    //Ignore set
                }
                else
                {
                    this._status = value.Trim();
                    
                }
            }
        }
        public string GetStatusTitle( int statusId )
        {
            string title = "";

            if ( statusId == 1 )
                return "Draft";
            else if ( statusId == 2 )
                return "In Progress";
            else if ( statusId == 3 )
                return "Submitted";
            else if ( statusId == 4 )
                return "Revisions Required";
            else if ( statusId == 5 )
                return "Published";
            else if ( statusId == 8 )
                return "Inactive";
            else
                return "";
        }
        private bool _isPublished = false;
        /// <summary>
        /// Gets/Sets IsPublished
        /// TODO - should probably just depend on the statusId = 5??
        /// </summary>
        public bool IsPublished
        {
            get
            {
                return this._isPublished;
            }
            set
            {
                if ( this._isPublished == value )
                {
                    //Ignore set
                }
                else
                {
                    this._isPublished = value;
                    
                }
            }
        }

        private int _privilegeTypeId;
        /// <summary>
        /// Gets/Sets PrivilegeTypeId
        /// </summary>
        public int PrivilegeTypeId
        {
            get
            {
                return this._privilegeTypeId;
            }
            set
            {
                if ( this._privilegeTypeId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._privilegeTypeId = value;
                    
                }
            }
        }

        private string _privilegeType = "";
        /// <summary>
        /// Gets/Sets PrivilegeType
        /// </summary>
        public string PrivilegeType
        {
            get
            {
                return this._privilegeType;
            }
            set
            {
                if ( this._privilegeType == value )
                {
                    //Ignore set
                }
                else
                {
                    this._privilegeType = value.Trim();
                    
                }
            }
        }


        private int _conditionsOfUseId;
        /// <summary>
        /// Gets/Sets ConditionsOfUseId
        /// </summary>
        public int ConditionsOfUseId
        {
            get
            {
                return this._conditionsOfUseId;
            }
            set
            {
                if ( this._conditionsOfUseId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._conditionsOfUseId = value;
                    
                }
            }
        }


        private int _resourceIntId;
        /// <summary>
        /// Gets/Sets the ResourceIntId
        /// </summary>
        public int ResourceIntId
        {
            get
            {
                return this._resourceIntId;
            }
            set
            {
                if ( this._resourceIntId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._resourceIntId = value;
 
                }
            }
        }//

        private string _useRightsUrl = "";
        /// <summary>
        /// Gets/Sets UseRightsUrl
        /// </summary>
        public string UseRightsUrl
        {
            get
            {
                return this._useRightsUrl;
            }
            set
            {
                if ( this._useRightsUrl == value )
                {
                    //Ignore set
                }
                else
                {
                    this._useRightsUrl = value.Trim();
                    
                }
            }
        }

        #endregion
    }
}
