using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    public interface IImage
    {
        int Id
        {
            get;
            set;
        }
        string FileName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets Height - height of the original image
        /// </summary>
        int Height
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets Width - width of the original image
        /// </summary>
        int Width
        {
            get;
            set;
        }
        /// <summary>
        /// Gets/Sets DisplayHeight - height of image for current context
        /// </summary>
        int DisplayHeight
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets DisplayWidth - width of image for current context
        /// </summary>
        int DisplayWidth
        {
            get;
            set;
        }
        /// <summary>
        /// Gets/Sets ThumbnailHeight - height of image for current context
        /// </summary>
        int ThumbnailHeight
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets ThumbnailWidth - width of image for current context
        /// </summary>
        int ThumbnailWidth
        {
            get;
            set;
        }
        long Bytes
        {
            get;
            set;
        }
        //byte[] Data
        //{
        //	get
        //	{
        //		return this._resourceData;
        //	}
        //}

        /// <summary>
        /// Assign the image data from an object
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="data"></param>
        void SetImageData( long bytes, object data );
        string Message
        {
            get;
            set;
        }
    }
}
