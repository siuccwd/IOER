/*********************************************************************************
= Author: Michael Parsons
=
= Date: Sep 08/2009
= Assembly: ILPathways.Business
= Description:
= Notes:
=
=
= Copyright 2009, Illinois workNet All rights reserved.
********************************************************************************/
using System;

namespace ILPathways.Business
{
	///<summary>
	///Represents an object that describes a ImageStore
	///</summary>
	[Serializable]
    public class ImageStore : ApplicationResource, IImage
	{
		///<summary>
		///Initializes a new instance of the ILPathways.Business.ImageStore class.
		///</summary>
		public ImageStore() { }

		#region Properties created from dictionary for ImageStore

		private string _position = "";
		/// <summary>
		/// Gets/Sets Position - where to render in a container
		/// </summary>
		public string Position
		{
			get
			{
				return this._position;
			}
			set
			{
				if ( this._position == value )
				{
					//Ignore set
				} else
				{
					this._position = value.Trim();
					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Gets/Sets ImageFileName
		/// </summary>
		public string ImageFileName
		{
			get
			{
				return this.FileName;
			}
			set
			{
				if ( this.FileName == value )
				{
					//Ignore set
				} else
				{
					this.FileName = value.Trim();
					HasChanged = true;
				}
			}
		}

		private int _height = 0;
		/// <summary>
		/// Gets/Sets Height - height of the original image
		/// </summary>
		public int Height
		{
			get
			{
				return this._height;
			}
			set
			{
				if ( this._height == value )
				{
					//Ignore set
				} else
				{
					this._height = value;
					HasChanged = true;
				}
			}
		}

		private int _width = 0;
		/// <summary>
		/// Gets/Sets Width - width of the original image
		/// </summary>
		public int Width
		{
			get
			{
				return this._width;
			}
			set
			{
				if ( this._width == value )
				{
					//Ignore set
				} else
				{
					this._width = value;
					HasChanged = true;
				}
			}
		}//

		/// <summary>
		/// Gets/Sets ImageBytes - length of the image
		/// </summary>
		public long Bytes
		{
			get
			{
				return this.ResourceBytes;
			}
			set
			{
				if ( this.ResourceBytes == value )
				{
					//Ignore set
				} else
				{
					this.ResourceBytes = value;
					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Gets/Sets ImageFileName
		/// </summary>
		public byte[] ImageData
		{
			get
			{
				return this.ResourceData;
			}
		}


        private int _displayHeight = 0;
        /// <summary>
        /// Gets/Sets DisplayHeight 
        /// </summary>
        public int DisplayHeight
        {
            get
            {
                return this._displayHeight;
            }
            set
            {
                if ( this._displayHeight == value )
                {
                    //Ignore set
                }
                else
                {
                    this._displayHeight = value;
                    HasChanged = true;
                }
            }
        }

        private int _displayWidth = 0;
        /// <summary>
        /// Gets/Sets DisplayWidth
        /// </summary>
        public int DisplayWidth
        {
            get
            {
                return this._displayWidth;
            }
            set
            {
                if ( this._displayWidth == value )
                {
                    //Ignore set
                }
                else
                {
                    this._displayWidth = value;
                    HasChanged = true;
                }
            }
        }//

        private int _thumbnailHeight = 0;
        /// <summary>
        /// Gets/Sets ThumbnailHeight - height of planned thumbnail
        /// </summary>
        public int ThumbnailHeight
        {
            get
            {
                return this._thumbnailHeight;
            }
            set
            {
                if ( this._thumbnailHeight == value )
                {
                    //Ignore set
                }
                else
                {
                    this._thumbnailHeight = value;
                    HasChanged = true;
                }
            }
        }

        private int _thumbnailWidth = 0;
        /// <summary>
        /// Gets/Sets ThumbnailWidth - width of planned thumbnail
        /// </summary>
        public int ThumbnailWidth
        {
            get
            {
                return this._thumbnailWidth;
            }
            set
            {
                if ( this._thumbnailWidth == value )
                {
                    //Ignore set
                }
                else
                {
                    this._thumbnailWidth = value;
                    HasChanged = true;
                }
            }
        }//

		#endregion

		#region Helper Methods
		/// <summary>
		/// Assign the image data from an object
		/// </summary>
		/// <param name="bytes"></param>
		/// <param name="data"></param>
		public void SetImageData(long bytes, object data)
		{
			SetResourceData( bytes, data );

		}//

		/// <summary>
		/// Assign the image data from a byte array
		/// </summary>
		/// <param name="bytes"></param>
		/// <param name="imageData"></param>
		public void SetImageData( long bytes, byte[] imageData )
		{
			SetResourceData( bytes, imageData );
		}
		#endregion
	} // end class 
} // end Namespace 

