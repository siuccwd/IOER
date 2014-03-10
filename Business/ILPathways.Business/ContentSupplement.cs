using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    [Serializable]
    public class ContentSupplement : BaseBusinessDataEntity
    {

        ///<summary>
        ///Initializes a new instance of the ILPathways.Business.ContentSupplement class.
        ///</summary>
        public ContentSupplement()
        {
            RelatedDocument = new DocumentVersion();
        }


        #region Properties

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
                    HasChanged = true;
                }
            }
        }

        private string _description = "";
        /// <summary>
        /// Gets/Sets Description
        /// </summary>
        public string Description
        {
            get
            {
                return this._description;
            }
            set
            {
                if ( this._description == value )
                {
                    //Ignore set
                }
                else
                {
                    this._description = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private int _parentId;
        /// <summary>
        /// Gets/Sets the id of the parent item
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
                    //Ignore set
                }
                else
                {
                    this._parentId = value;
                    HasChanged = true;
                }
            }
        }//

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
                    HasChanged = true;
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
                    HasChanged = true;
                }
            }
        }

        private string _resourceUrl = "";
        /// <summary>
        /// Gets/Sets ResourceUrl
        /// </summary>
        public string ResourceUrl
        {
            get
            {
                return this._resourceUrl;
            }
            set
            {
                if ( this._resourceUrl == value )
                {
                    //Ignore set
                }
                else
                {
                    this._resourceUrl = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private Guid _documentRowId;
        public Guid DocumentRowId
        {
            get
            {
                return this._documentRowId;
            }
            set
            {
                if ( this._documentRowId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._documentRowId = value;
                    HasChanged = true;
                }
            }
        }
        #endregion

        #region Helpers
        public DocumentVersion RelatedDocument { get; set; }
       

        //private string _fileName = "";
        ///// <summary>
        ///// Gets/Sets FileName
        ///// </summary>
        //public string FileName
        //{
        //    get
        //    {
        //        return this._fileName;
        //    }
        //    set
        //    {
        //        if ( this._fileName == value )
        //        {
        //            //Ignore set
        //        }
        //        else
        //        {
        //            this._fileName = value.Trim();
        //            HasChanged = true;
        //        }
        //    }
        //}


        //private DateTime _fileDate;
        ///// <summary>
        ///// Gets/Sets FileDate
        ///// </summary>
        //public DateTime FileDate
        //{
        //    get
        //    {
        //        return this._fileDate;
        //    }
        //    set
        //    {
        //        if ( this._fileDate == value )
        //        {
        //            //Ignore set
        //        }
        //        else
        //        {
        //            this._fileDate = value;
        //            HasChanged = true;
        //        }
        //    }
        //}
        //private long _resourceBytes = 0;
        ///// <summary>
        ///// Gets/Sets ResourceBytes - length of the resource
        ///// </summary>
        //public long ResourceBytes
        //{
        //    get
        //    {
        //        return this._resourceBytes;
        //    }
        //    set
        //    {
        //        if ( this._resourceBytes == value )
        //        {
        //            //Ignore set
        //        }
        //        else
        //        {
        //            this._resourceBytes = value;
        //            HasChanged = true;
        //        }
        //    }
        //}

        //private byte[] _resourceData;
        ///// <summary>
        ///// Gets/Sets ResourceData
        ///// </summary>
        //public byte[] ResourceData
        //{
        //    get
        //    {
        //        return this._resourceData;
        //    }
        //}
        ///// <summary>
        ///// Assign the resource data from an object
        ///// </summary>
        ///// <param name="bytes"></param>
        ///// <param name="data"></param>
        //public void SetResourceData( long bytes, object data )
        //{
        //    if ( bytes > 0 )
        //    {
        //        _resourceData = new byte[ bytes ];
        //        _resourceData = ( byte[] ) data;
        //    }
        //}//

        ///// <summary>
        ///// Assign the resource data from a byte array
        ///// </summary>
        ///// <param name="bytes"></param>
        ///// <param name="resourceData"></param>
        //public void SetResourceData( long bytes, byte[] resourceData )
        //{
        //    if ( bytes > 0 )
        //    {
        //        _resourceData = new byte[ bytes ];
        //        _resourceData = resourceData;
        //    }
        //}
        #endregion
    } // end class 
} // end Namespace 

