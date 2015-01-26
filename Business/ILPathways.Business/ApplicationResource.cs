/*********************************************************************************
= Author: Michael Parsons
=
= Date: Jun 08/2010
= Assembly: ILPathways.Business
= Description:
= Notes:
=
=
= Copyright 2010, Illinois workNet All rights reserved.
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace ILPathways.Business
{
	///<summary>
	///Represents an object that describes a resourceStore
	///</summary>
	[Serializable]
	public class ApplicationResource : BaseBusinessDataEntity, IDocument
	{
		///<summary>
		///Initializes a new instance of the ILPathways.Business.ApplicationResource class.
		///</summary>
		public ApplicationResource() { }

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
				} else
				{
					this._title = value.Trim();
					HasChanged = true;
				}
			}
		}

		private string _url = "";
		/// <summary>
		/// Gets/Sets URL - for retrieving the resource
		/// </summary>
		public string URL
		{
			get
			{
				return this._url;
			}
			set
			{
				if ( this._url == value )
				{
					//Ignore set
				} else
				{
					this._url = value.Trim();
					HasChanged = true;
				}
			}
		}
        /// <summary>
        /// Gets/Sets the ResourceUrl - Alias for URL
        /// </summary>
        public string ResourceUrl
        {
            get
            {
                return this._url;
            }
            set
            {
                if ( this._url == value )
                {
                    //Ignore set
                }
                else
                {
                    this._url = value.Trim();
                    HasChanged = true;
                }
            }
        }
		private string _mimeType = "";
		/// <summary>
		/// Gets/Sets MimeType
		/// </summary>
		public string MimeType
		{
			get
			{
				return this._mimeType;
			}
			set
			{
				if ( this._mimeType == value )
				{
					//Ignore set
				} else
				{
					this._mimeType = value.Trim();
					HasChanged = true;
				}
			}
		}

		private string _fileName = "";
		/// <summary>
		/// Gets/Sets the File Name
		/// </summary>
		public string FileName
		{
			get
			{
				return this._fileName;
			}
			set
			{
				if ( this._fileName == value )
				{
					//Ignore set
				} else
				{
					this._fileName = value.Trim();
					HasChanged = true;
				}
			}
		}
        public string FilePath { get; set; }

		private DateTime _modifiedDate = System.DateTime.Now;
		/// <summary>
		/// Gets/Sets FileDate - the modified date of the file at the time of upload
		/// </summary>
		public DateTime FileDate
		{
			get
			{
				return this._modifiedDate;
			}
			set
			{
				if ( this._modifiedDate == value )
				{
					//Ignore set
				} else
				{
					this._modifiedDate = value;
					HasChanged = true;
				}
			}
		}//
		private long _resourceBytes = 0;
		/// <summary>
		/// Gets/Sets ResourceBytes - length of the resource
		/// </summary>
		public long ResourceBytes
		{
			get
			{
				return this._resourceBytes;
			}
			set
			{
				if ( this._resourceBytes == value )
				{
					//Ignore set
				} else
				{
					this._resourceBytes = value;
					HasChanged = true;
				}
			}
		}

		private byte[] _resourceData;
		/// <summary>
		/// Gets/Sets ResourceData
		/// </summary>
		public byte[] ResourceData
		{
			get
			{
				return this._resourceData;
			}
		}

		#endregion

		#region Helper Methods
        /// <summary>
        /// There may be occasions when a documentVersion is retrieved with the byte array (for speed efficiency)). This property can be used to determine if the actual resource needs to be fetched.
        /// </summary>
        /// <returns></returns>
        public bool HasDocument()
        {
            if ( ResourceBytes > 0 )
                return true;
            else
                return false;
        }
       
		/// <summary>
		/// Assign the resource data from an object
		/// </summary>
		/// <param name="bytes"></param>
		/// <param name="data"></param>
		public void SetResourceData( long bytes, object data )
		{
			if ( bytes > 0 )
			{
				_resourceData = new byte[ bytes ];
				_resourceData = ( byte[] ) data;
			}
		}//

		/// <summary>
		/// Assign the resource data from a byte array
		/// </summary>
		/// <param name="bytes"></param>
		/// <param name="resourceData"></param>
		public void SetResourceData( long bytes, byte[] resourceData )
		{
			if ( bytes > 0 )
			{
				_resourceData = new byte[ bytes ];
				_resourceData = resourceData;
			}
		}

        public void CleanFileName()
        {
            if ( FileName == null || FileName.Trim() == "" )
                return;

            FileName = FileName.Replace( " & ", " and " );
            FileName = FileName.Replace( " ", "_" );
            
            FileName = FileName.Replace( "'", "" );
            FileName = FileName.Replace( "/", "_" );
            FileName = FileName.Replace( "\\", "_" );
            FileName = FileName.Replace( "#", "_" );
            FileName = FileName.Replace( "&", "_" );
        }

        /// <summary>
        /// Return absolute path to the file
        /// </summary>
        /// <returns></returns>
        public string FileLocation()
        {
            if ( FileName == null || FileName.Trim() == "" )
                return "";
            if ( FilePath == null || FilePath.Trim() == "" )
                return "";

            if ( FilePath.Trim().EndsWith( "\\" ) )
                return FilePath + FileName;
            else 
                return FilePath + "\\" + FileName;
        }
		#endregion
	} // end class 
} // end Namespace 
